using System.ComponentModel.DataAnnotations;

namespace RoleService.Application.DTOs.Roles;

/// <summary>
/// Request para actualizar un rol existente.
/// Nota: El campo Name es inmutable y no puede ser cambiado.
/// </summary>
public record UpdateRoleRequest
{
    /// <summary>
    /// Nombre visible para UI (opcional - solo actualiza si se proporciona)
    /// </summary>
    [StringLength(100, MinimumLength = 3)]
    public string? DisplayName { get; init; }

    /// <summary>
    /// Descripción del rol (opcional)
    /// </summary>
    [StringLength(500)]
    public string? Description { get; init; }

    /// <summary>
    /// Si el rol está activo (opcional - solo actualiza si se proporciona)
    /// </summary>
    public bool? IsActive { get; init; }

    /// <summary>
    /// Lista de IDs de permisos a asignar (reemplaza los existentes)
    /// </summary>
    public List<Guid>? PermissionIds { get; init; }
}
