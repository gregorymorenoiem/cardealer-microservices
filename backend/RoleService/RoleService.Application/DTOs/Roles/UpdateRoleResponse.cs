namespace RoleService.Application.DTOs.Roles;

public record UpdateRoleResponse(
    Guid Id,
    string Name,
    string Description,
    int Priority,
    bool IsActive,
    DateTime UpdatedAt
);
