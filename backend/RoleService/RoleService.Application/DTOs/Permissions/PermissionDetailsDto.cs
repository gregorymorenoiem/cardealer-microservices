namespace RoleService.Application.DTOs.Permissions;

public record PermissionDetailsDto(
    Guid Id,
    string Name,
    string Description,
    string Resource,
    string Action,
    string Module,
    bool IsActive,
    bool IsSystemPermission,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
