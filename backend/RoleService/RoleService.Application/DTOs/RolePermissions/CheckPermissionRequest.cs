namespace RoleService.Application.DTOs.RolePermissions;

public record CheckPermissionRequest(
    Guid UserId,
    string Resource,
    string Action
);
