using MediatR;
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

        public CheckUserPermissionQueryHandler(
            IUserRoleRepository userRoleRepository,
            IRoleServiceClient roleServiceClient)
        {
            _userRoleRepository = userRoleRepository;
            _roleServiceClient = roleServiceClient;
        }

        public async Task<CheckPermissionResponse> Handle(CheckUserPermissionQuery request, CancellationToken cancellationToken)
        {
            // 1. Obtener roles del usuario
            var userRoles = await _userRoleRepository.GetByUserIdAsync(request.UserId);

            if (!userRoles.Any())
            {
                return new CheckPermissionResponse
                {
                    HasPermission = false,
                    GrantedByRoles = new(),
                    Message = "User has no roles assigned"
                };
            }

            // 2. Obtener información de roles desde RoleService
            var roleIds = userRoles.Select(ur => ur.RoleId).ToList();
            var roles = await _roleServiceClient.GetRolesByIdsAsync(roleIds);

            // 3. Verificar permisos
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

            return new CheckPermissionResponse
            {
                HasPermission = grantedByRoles.Any(),
                GrantedByRoles = grantedByRoles,
                Message = grantedByRoles.Any()
                    ? $"Permission granted by roles: {string.Join(", ", grantedByRoles)}"
                    : "Permission denied"
            };
        }
    }
}
