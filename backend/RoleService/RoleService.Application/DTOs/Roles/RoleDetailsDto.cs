namespace RoleService.Application.DTOs.Roles;

public record RoleDetailsDto(
    Guid Id,
    string Name,
    string Description,
    int Priority,
    bool IsActive,
    bool IsSystemRole,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string CreatedBy,
    string? UpdatedBy,
    List<PermissionDto> Permissions
);

public record PermissionDto(
    Guid Id,
    string Name,
    string Description,
    string Resource,
    string Action,
    string Module
);
