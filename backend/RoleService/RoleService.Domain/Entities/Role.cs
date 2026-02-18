namespace RoleService.Domain.Entities;

/// <summary>
/// Entidad de Rol para el sistema RBAC.
/// Los roles del sistema (SuperAdmin, Admin) son inmutables.
/// </summary>
public class Role
{
    public Guid Id { get; set; }

    /// <summary>
    /// Nombre técnico del rol (ej: "DealerOwner"). Inmutable después de creación.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Nombre visible para UI (ej: "Dueño de Concesionario")
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Descripción del rol y sus propósitos
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Prioridad del rol (menor = mayor prioridad). SuperAdmin = 0, Admin = 1, etc.
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// Si el rol está activo. Roles inactivos no pueden ser asignados.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Si es un rol del sistema (SuperAdmin, Admin). Estos son inmutables.
    /// </summary>
    public bool IsSystemRole { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

    // Computed properties for statistics
    public int PermissionCount => RolePermissions?.Count ?? 0;

    /// <summary>
    /// Verifica si el rol puede ser modificado (no es rol del sistema)
    /// </summary>
    public bool CanBeModified() => !IsSystemRole;

    /// <summary>
    /// Verifica si el rol puede ser eliminado (no es rol del sistema y está inactivo)
    /// </summary>
    public bool CanBeDeleted() => !IsSystemRole;
}
