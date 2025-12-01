namespace RoleService.Application.DTOs.RolePermissions;

public record CheckPermissionResponse(
    bool HasPermission,
    string Message
);
