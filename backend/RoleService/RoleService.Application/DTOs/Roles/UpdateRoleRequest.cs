namespace RoleService.Application.DTOs.Roles;

public record UpdateRoleRequest(
    string Name,
    string Description,
    int Priority,
    bool IsActive
);
