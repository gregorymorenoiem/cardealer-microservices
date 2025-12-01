using RoleService.Domain.Enums;

namespace RoleService.Domain.Interfaces;

public interface IRolePermissionRepository
{
    Task AssignPermissionToRoleAsync(Guid roleId, Guid permissionId, string assignedBy, CancellationToken cancellationToken = default);
    Task RemovePermissionFromRoleAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default);
    Task<bool> HasPermissionAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default);
    Task<bool> RoleHasPermissionAsync(Guid roleId, string resource, PermissionAction action, CancellationToken cancellationToken = default);
}
