namespace RoleService.Application.DTOs.Roles;

/// <summary>
/// Respuesta al crear un rol exitosamente.
/// </summary>
public record CreateRoleResponse
{
    public bool Success { get; init; } = true;
    public RoleCreatedData Data { get; init; } = null!;
}

public record RoleCreatedData
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsActive { get; init; }
    public int PermissionCount { get; init; }
    public DateTime CreatedAt { get; init; }
}
