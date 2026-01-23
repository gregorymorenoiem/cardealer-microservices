using System.ComponentModel.DataAnnotations;

namespace RoleService.Application.DTOs.Permissions;

/// <summary>
/// Request para crear un nuevo permiso.
/// El nombre se genera automáticamente en formato: resource:action
/// </summary>
public record CreatePermissionRequest
{
    /// <summary>
    /// Nombre del permiso en formato resource:action (ej: "vehicles:create")
    /// </summary>
    [Required]
    [RegularExpression(@"^[a-z]+:[a-z]+(-[a-z]+)*$", 
        ErrorMessage = "Name must be in format 'resource:action' (lowercase)")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Nombre visible para UI (ej: "Crear Vehículos")
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>
    /// Descripción del permiso
    /// </summary>
    [StringLength(500)]
    public string? Description { get; init; }

    /// <summary>
    /// Recurso sobre el que aplica (ej: "vehicles", "users", "dealers")
    /// </summary>
    [Required]
    [RegularExpression(@"^[a-z]+(-[a-z]+)*$", ErrorMessage = "Resource must be lowercase letters and hyphens only")]
    public string Resource { get; init; } = string.Empty;

    /// <summary>
    /// Acción permitida (ej: "create", "read", "update", "delete")
    /// </summary>
    [Required]
    [RegularExpression(@"^[a-z]+(-[a-z]+)*$", ErrorMessage = "Action must be lowercase letters and hyphens only")]
    public string Action { get; init; } = string.Empty;

    /// <summary>
    /// Módulo al que pertenece (auth, users, vehicles, dealers, billing, etc.)
    /// </summary>
    [Required]
    public string Module { get; init; } = string.Empty;
}
