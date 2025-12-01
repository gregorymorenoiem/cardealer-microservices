using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UserService.Application.DTOs;
using UserService.Domain.Interfaces;
using UserService.Application.Interfaces;

namespace UserService.Application.UseCases.UserRoles.GetUserRoles
{
    public record GetUserRolesQuery(Guid UserId) : IRequest<UserRolesResponse>;

    public class GetUserRolesQueryHandler : IRequestHandler<GetUserRolesQuery, UserRolesResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IRoleServiceClient _roleServiceClient;

        public GetUserRolesQueryHandler(
            IUserRepository userRepository,
            IUserRoleRepository userRoleRepository,
            IRoleServiceClient roleServiceClient)
        {
            _userRepository = userRepository;
            _userRoleRepository = userRoleRepository;
            _roleServiceClient = roleServiceClient;
        }

        public async Task<UserRolesResponse> Handle(GetUserRolesQuery request, CancellationToken cancellationToken)
        {
            // 1. Obtener usuario
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
            {
                return new UserRolesResponse
                {
                    UserId = request.UserId,
                    UserEmail = string.Empty,
                    Roles = new()
                };
            }

            // 2. Obtener asignaciones de roles del usuario
            var userRoles = await _userRoleRepository.GetByUserIdAsync(request.UserId);

            if (!userRoles.Any())
            {
                return new UserRolesResponse
                {
                    UserId = request.UserId,
                    UserEmail = user.Email,
                    Roles = new()
                };
            }

            // 3. Obtener información de los roles desde RoleService
            var roleIds = userRoles.Select(ur => ur.RoleId).ToList();
            var roles = await _roleServiceClient.GetRolesByIdsAsync(roleIds);

            // 4. Combinar información
            var roleDtos = userRoles.Select(ur =>
            {
                var role = roles.FirstOrDefault(r => r.Id == ur.RoleId);
                return new UserRoleDto
                {
                    RoleId = ur.RoleId,
                    RoleName = role?.Name ?? "Unknown",
                    Priority = role?.Priority ?? 0,
                    AssignedAt = ur.AssignedAt,
                    AssignedBy = ur.AssignedBy,
                    IsActive = ur.IsActive
                };
            }).ToList();

            return new UserRolesResponse
            {
                UserId = request.UserId,
                UserEmail = user.Email,
                Roles = roleDtos
            };
        }
    }
}
