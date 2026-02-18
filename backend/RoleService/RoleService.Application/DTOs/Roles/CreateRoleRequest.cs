using System.ComponentModel.DataAnnotations;

namespace RoleService.Application.DTOs.Roles;

/// <summary>
/// Request para crear un nuevo rol.
/// El nombre debe ser único y seguir el formato: letras, números, guiones y guiones bajos.
/// </summary>
public record CreateRoleRequest
{
    /// <summary>
    /// Nombre técnico del rol (ej: "DealerManager"). 3-50 caracteres, único.
    /// </summary>
    [Required]
    [StringLength(50, MinimumLength = 3)]
    [RegularExpression(@"^[a-zA-Z][a-zA-Z0-9_-]*$",
        ErrorMessage = "Name must start with a letter and contain only letters, numbers, underscores and hyphens")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Nombre visible para UI (ej: "Gerente de Concesionario")
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>
    /// Descripción del rol (opcional pero recomendado)
    /// </summary>
    [StringLength(500)]
    public string? Description { get; init; }

    /// <summary>
    /// Si el rol está activo desde su creación (default: true)
    /// </summary>
    public bool IsActive { get; init; } = true;

    /// <summary>
    /// Lista de IDs de permisos a asignar al crear el rol
    /// </summary>
    public List<Guid>? PermissionIds { get; init; }
}
