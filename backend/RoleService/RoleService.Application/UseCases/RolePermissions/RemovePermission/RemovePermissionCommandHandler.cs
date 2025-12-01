using MediatR;
using RoleService.Domain.Interfaces;
using RoleService.Shared.Exceptions;
using RoleService.Application.Interfaces;

namespace RoleService.Application.UseCases.RolePermissions.RemovePermission;

public class RemovePermissionCommandHandler : IRequestHandler<RemovePermissionCommand, bool>
{
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IRolePermissionRepository _rolePermissionRepository;
    private readonly IAuditServiceClient _auditClient;

    public RemovePermissionCommandHandler(
        IRoleRepository roleRepository,
        IPermissionRepository permissionRepository,
        IRolePermissionRepository rolePermissionRepository,
        IAuditServiceClient auditClient)
    {
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
        _rolePermissionRepository = rolePermissionRepository;
        _auditClient = auditClient;
    }

    public async Task<bool> Handle(RemovePermissionCommand request, CancellationToken cancellationToken)
    {
        // Verificar que el role existe
        var role = await _roleRepository.GetByIdAsync(request.RoleId, cancellationToken);
        if (role == null)
        {
            throw new NotFoundException($"Role with ID '{request.RoleId}' not found");
        }

        if (role.IsSystemRole)
        {
            throw new ForbiddenException("Cannot remove permissions from system roles");
        }

        // Verificar que el permission existe
        var permission = await _permissionRepository.GetByIdAsync(request.PermissionId, cancellationToken);
        if (permission == null)
        {
            throw new NotFoundException($"Permission with ID '{request.PermissionId}' not found");
        }

        await _rolePermissionRepository.RemovePermissionFromRoleAsync(
            request.RoleId,
            request.PermissionId,
            cancellationToken);

        // Auditoría
        _ = _auditClient.LogPermissionRemovedAsync(
            request.RoleId,
            request.PermissionId,
            "system");

        return true;
    }
}
