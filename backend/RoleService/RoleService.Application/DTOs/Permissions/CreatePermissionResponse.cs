namespace RoleService.Application.DTOs.Permissions;

public record CreatePermissionResponse(
    Guid Id,
    string Name,
    string Description,
    string Resource,
    string Action,
    string Module,
    bool IsSystemPermission,
    DateTime CreatedAt
);
