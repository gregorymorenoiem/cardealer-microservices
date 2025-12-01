namespace RoleService.Application.DTOs.RolePermissions;

public record AssignPermissionRequest(
    Guid RoleId,
    Guid PermissionId
);
