namespace RoleService.Application.DTOs.Roles;

public record RoleListItemDto(
    Guid Id,
    string Name,
    string Description,
    int Priority,
    bool IsActive,
    bool IsSystemRole,
    int PermissionCount,
    DateTime CreatedAt
);
