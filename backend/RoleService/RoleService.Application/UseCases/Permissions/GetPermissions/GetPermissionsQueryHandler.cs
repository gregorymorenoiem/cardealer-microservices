using MediatR;
using RoleService.Application.DTOs.Permissions;
using RoleService.Domain.Interfaces;

namespace RoleService.Application.UseCases.Permissions.GetPermissions;

public class GetPermissionsQueryHandler : IRequestHandler<GetPermissionsQuery, List<PermissionDetailsDto>>
{
    private readonly IPermissionRepository _permissionRepository;

    public GetPermissionsQueryHandler(IPermissionRepository permissionRepository)
    {
        _permissionRepository = permissionRepository;
    }

    public async Task<List<PermissionDetailsDto>> Handle(GetPermissionsQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<Domain.Entities.Permission> permissions;

        if (!string.IsNullOrEmpty(request.Module))
        {
            permissions = await _permissionRepository.GetByModuleAsync(request.Module, cancellationToken);
        }
        else if (!string.IsNullOrEmpty(request.Resource))
        {
            permissions = await _permissionRepository.GetByResourceAsync(request.Resource, cancellationToken);
        }
        else
        {
            permissions = await _permissionRepository.GetAllAsync(cancellationToken);
        }

        return permissions.Select(p => new PermissionDetailsDto(
            p.Id,
            p.Name,
            p.Description,
            p.Resource,
            p.Action.ToString(),
            p.Module,
            p.IsActive,
            p.IsSystemPermission,
            p.CreatedAt,
            p.UpdatedAt
        )).ToList();
    }
}
