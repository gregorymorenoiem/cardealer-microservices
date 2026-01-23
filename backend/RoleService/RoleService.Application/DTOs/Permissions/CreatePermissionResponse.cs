namespace RoleService.Application.DTOs.Permissions;

/// <summary>
/// Respuesta al crear un permiso exitosamente.
/// </summary>
public record CreatePermissionResponse
{
    public bool Success { get; init; } = true;
    public PermissionCreatedData Data { get; init; } = null!;
}

public record PermissionCreatedData
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Resource { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public string Module { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}
