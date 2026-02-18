namespace RoleService.Application.DTOs.Permissions;

/// <summary>
/// DTO ligero para listar permisos.
/// Solo incluye campos esenciales para listados y selecciones.
/// </summary>
public record PermissionListItemDto
{
    /// <summary>
    /// ID único del permiso
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Nombre técnico del permiso (formato: resource:action)
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Nombre legible para mostrar en UI
    /// </summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>
    /// Módulo al que pertenece el permiso
    /// </summary>
    public string Module { get; init; } = string.Empty;

    /// <summary>
    /// Si el permiso está activo
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// Si es un permiso del sistema (no editable)
    /// </summary>
    public bool IsSystemPermission { get; init; }
}
