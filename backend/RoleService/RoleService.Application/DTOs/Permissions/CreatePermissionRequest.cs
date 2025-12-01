namespace RoleService.Application.DTOs.Permissions;

public record CreatePermissionRequest(
    string Name,
    string Description,
    string Resource,
    string Action,
    string Module,
    bool IsSystemPermission = false
);
