using MediatR;
using UserService.Application.DTOs.UserRoles;
using UserService.Domain.Interfaces;

namespace UserService.Application.UseCases.UserRoles.GetUserRoles
{
    /// <summary>
    /// Query para obtener todos los roles de un usuario con su información
    /// </summary>
    public record GetUserRolesQuery(Guid UserId) : IRequest<UserRolesResponse>;

    public class GetUserRolesQueryHandler : IRequestHandler<GetUserRolesQuery, UserRolesResponse>
    {
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IRoleServiceClient _roleServiceClient;

        public GetUserRolesQueryHandler(
            IUserRoleRepository userRoleRepository,
            IRoleServiceClient roleServiceClient)
        {
            _userRoleRepository = userRoleRepository;
            _roleServiceClient = roleServiceClient;
        }

        public async Task<UserRolesResponse> Handle(GetUserRolesQuery request, CancellationToken cancellationToken)
        {
            // 1. Obtener asignaciones de roles del usuario
            var userRoles = await _userRoleRepository.GetByUserIdAsync(request.UserId);

            if (!userRoles.Any())
            {
                return new UserRolesResponse
                {
                    UserId = request.UserId,
                    Roles = new List<UserRoleDto>()
                };
            }

            // 2. Obtener información de los roles desde RoleService
            var roleIds = userRoles.Select(ur => ur.RoleId).ToList();
            var roles = await _roleServiceClient.GetRolesByIdsAsync(roleIds);

            // 3. Combinar información
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
                Roles = roleDtos
            };
        }
    }
}
