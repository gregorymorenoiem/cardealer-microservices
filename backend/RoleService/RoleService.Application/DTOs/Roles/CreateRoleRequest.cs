namespace RoleService.Application.DTOs.Roles;

public record CreateRoleRequest(
    string Name,
    string Description,
    int Priority,
    bool IsSystemRole = false
);
