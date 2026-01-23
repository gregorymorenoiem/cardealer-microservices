namespace RoleService.Application.DTOs.Roles;

/// <summary>
/// DTO para listar roles con información resumida.
/// </summary>
public record RoleListItemDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public bool IsSystemRole { get; init; }
    public int UserCount { get; init; }
    public int PermissionCount { get; init; }
    public DateTime CreatedAt { get; init; }
}
