using MediatR;
using RoleService.Domain.Interfaces;
using RoleService.Shared.Exceptions;
using RoleService.Application.Interfaces;

namespace RoleService.Application.UseCases.RolePermissions.AssignPermission;

public class AssignPermissionCommandHandler : IRequestHandler<AssignPermissionCommand, bool>
{
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IRolePermissionRepository _rolePermissionRepository;
    private readonly IAuditServiceClient _auditClient;

    public AssignPermissionCommandHandler(
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

    public async Task<bool> Handle(AssignPermissionCommand request, CancellationToken cancellationToken)
    {
        // Verificar que el role existe
        var role = await _roleRepository.GetByIdAsync(request.RoleId, cancellationToken);
        if (role == null)
        {
            throw new NotFoundException($"Role with ID '{request.RoleId}' not found");
        }

        // Verificar que el permission existe
        var permission = await _permissionRepository.GetByIdAsync(request.PermissionId, cancellationToken);
        if (permission == null)
        {
            throw new NotFoundException($"Permission with ID '{request.PermissionId}' not found");
        }

        // Verificar si ya tiene el permiso
        var hasPermission = await _rolePermissionRepository.HasPermissionAsync(
            request.RoleId,
            request.PermissionId,
            cancellationToken);

        if (hasPermission)
        {
            throw new ConflictException("Role already has this permission");
        }

        await _rolePermissionRepository.AssignPermissionToRoleAsync(
            request.RoleId,
            request.PermissionId,
            "system", // TODO: Get from JWT claims
            cancellationToken);

        // Auditoría
        _ = _auditClient.LogPermissionAssignedAsync(
            request.RoleId,
            request.PermissionId,
            "system");

        return true;
    }
}
