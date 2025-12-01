namespace RoleService.Application.DTOs.Roles;

public record CreateRoleResponse(
    Guid Id,
    string Name,
    string Description,
    int Priority,
    bool IsSystemRole,
    DateTime CreatedAt
);
