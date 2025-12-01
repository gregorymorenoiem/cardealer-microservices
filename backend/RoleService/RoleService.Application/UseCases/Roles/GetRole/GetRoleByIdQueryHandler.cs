using MediatR;
using RoleService.Application.DTOs.Roles;
using RoleService.Domain.Interfaces;

namespace RoleService.Application.UseCases.Roles.GetRole;

public class GetRoleByIdQueryHandler : IRequestHandler<GetRoleByIdQuery, RoleDetailsDto?>
{
    private readonly IRoleRepository _roleRepository;

    public GetRoleByIdQueryHandler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<RoleDetailsDto?> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
    {
        var role = await _roleRepository.GetByIdAsync(request.RoleId, cancellationToken);
        if (role == null)
        {
            return null;
        }

        var permissions = await _roleRepository.GetRolePermissionsAsync(request.RoleId, cancellationToken);

        return new RoleDetailsDto(
            role.Id,
            role.Name,
            role.Description,
            role.Priority,
            role.IsActive,
            role.IsSystemRole,
            role.CreatedAt,
            role.UpdatedAt,
            role.CreatedBy,
            role.UpdatedBy,
            permissions.Select(p => new PermissionDto(
                p.Id,
                p.Name,
                p.Description,
                p.Resource,
                p.Action.ToString(),
                p.Module
            )).ToList()
        );
    }
}
