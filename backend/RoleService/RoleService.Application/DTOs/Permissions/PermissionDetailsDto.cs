namespace RoleService.Application.DTOs.Permissions;

/// <summary>
/// DTO detallado de un permiso.
/// </summary>
public record PermissionDetailsDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Resource { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public string Module { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public bool IsSystemPermission { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
