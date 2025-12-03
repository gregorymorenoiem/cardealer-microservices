using MediatR;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UserService.Application.DTOs;
using UserService.Domain.Interfaces;
using UserService.Application.Interfaces;

namespace UserService.Application.UseCases.UserRoles.CheckPermission
{
    public record CheckUserPermissionQuery(
        Guid UserId,
        string Resource,
        string Action
    ) : IRequest<CheckPermissionResponse>;

    public class CheckUserPermissionQueryHandler : IRequestHandler<CheckUserPermissionQuery, CheckPermissionResponse>
    {
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IRoleServiceClient _roleServiceClient;
        private readonly IMemoryCache _cache;
        private const int CacheTtlMinutes = 5;

        public CheckUserPermissionQueryHandler(
            IUserRoleRepository userRoleRepository,
            IRoleServiceClient roleServiceClient,
            IMemoryCache cache)
        {
            _userRoleRepository = userRoleRepository;
            _roleServiceClient = roleServiceClient;
            _cache = cache;
        }

        public async Task<CheckPermissionResponse> Handle(CheckUserPermissionQuery request, CancellationToken cancellationToken)
        {
            // 1. Verificar caché
            var cacheKey = $"permission:{request.UserId}:{request.Resource}:{request.Action}";
            if (_cache.TryGetValue(cacheKey, out CheckPermissionResponse? cachedResponse))
            {
                return cachedResponse!;
            }

            // 2. Obtener roles del usuario
            var userRoles = await _userRoleRepository.GetByUserIdAsync(request.UserId);

            if (!userRoles.Any())
            {
                var noRolesResponse = new CheckPermissionResponse
                {
                    HasPermission = false,
                    GrantedByRoles = new(),
                    Message = "User has no roles assigned"
                };

                // Cache negative result
                _cache.Set(cacheKey, noRolesResponse, TimeSpan.FromMinutes(CacheTtlMinutes));
                return noRolesResponse;
            }

            // 3. Obtener información de roles desde RoleService
            var roleIds = userRoles.Select(ur => ur.RoleId).ToList();
            var roles = await _roleServiceClient.GetRolesByIdsAsync(roleIds);

            // 4. Verificar permisos
            var grantedByRoles = new System.Collections.Generic.List<string>();

            foreach (var role in roles)
            {
                // Verificar si el rol tiene el permiso específico
                var hasPermission = role.Permissions.Any(p =>
                    (p.Resource.Equals(request.Resource, StringComparison.OrdinalIgnoreCase) &&
                     (p.Action.Equals(request.Action, StringComparison.OrdinalIgnoreCase) ||
                      p.Action.Equals("All", StringComparison.OrdinalIgnoreCase))) ||
                    (p.Resource.Equals("*", StringComparison.OrdinalIgnoreCase) &&
                     p.Action.Equals("All", StringComparison.OrdinalIgnoreCase))
                );

                if (hasPermission)
                {
                    grantedByRoles.Add(role.Name);
                }
            }

            var response = new CheckPermissionResponse
            {
                HasPermission = grantedByRoles.Any(),
                GrantedByRoles = grantedByRoles,
                Message = grantedByRoles.Any()
                    ? $"Permission granted by roles: {string.Join(", ", grantedByRoles)}"
                    : "Permission denied"
            };

            // Cache result
            _cache.Set(cacheKey, response, TimeSpan.FromMinutes(CacheTtlMinutes));

            return response;
        }
    }
}
