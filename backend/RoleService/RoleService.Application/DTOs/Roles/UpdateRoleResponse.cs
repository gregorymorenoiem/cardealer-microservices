namespace RoleService.Application.DTOs.Roles;

/// <summary>
/// Respuesta al actualizar un rol exitosamente.
/// </summary>
public record UpdateRoleResponse
{
    public bool Success { get; init; } = true;
    public RoleUpdatedData Data { get; init; } = null!;
}

public record RoleUpdatedData
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsActive { get; init; }
    public bool IsSystemRole { get; init; }
    public int PermissionCount { get; init; }
    public DateTime UpdatedAt { get; init; }
}
