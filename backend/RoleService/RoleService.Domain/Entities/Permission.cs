using RoleService.Domain.Enums;

namespace RoleService.Domain.Entities;

/// <summary>
/// Entidad de Permiso para el sistema RBAC.
/// Los permisos siguen el formato: resource:action (ej: "vehicles:create")
/// </summary>
public class Permission
{
    public Guid Id { get; set; }

    /// <summary>
    /// Nombre completo del permiso en formato resource:action (ej: "vehicles:create")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Nombre visible para UI (ej: "Crear Vehículos")
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Descripción detallada del permiso
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Recurso sobre el que aplica (ej: "vehicles", "users", "dealers")
    /// </summary>
    public string Resource { get; set; } = string.Empty;

    /// <summary>
    /// Acción permitida (Create, Read, Update, Delete, Execute, All)
    /// </summary>
    public PermissionAction Action { get; set; }

    /// <summary>
    /// Módulo al que pertenece (auth, users, vehicles, dealers, billing, etc.)
    /// </summary>
    public string Module { get; set; } = string.Empty;

    /// <summary>
    /// Si el permiso está activo
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Si es un permiso del sistema (no puede ser eliminado)
    /// </summary>
    public bool IsSystemPermission { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

    /// <summary>
    /// Genera el nombre del permiso basado en resource y action
    /// </summary>
    public static string GenerateName(string resource, PermissionAction action)
    {
        return $"{resource.ToLowerInvariant()}:{action.ToString().ToLowerInvariant()}";
    }

    /// <summary>
    /// Lista de módulos permitidos en el sistema
    /// </summary>
    public static readonly string[] AllowedModules = new[]
    {
        "auth", "users", "vehicles", "dealers", "billing", "media",
        "notifications", "reports", "analytics", "kyc", "aml",
        "compliance", "admin", "crm", "support"
    };

    /// <summary>
    /// Valida si el módulo es permitido
    /// </summary>
    public static bool IsValidModule(string module) =>
        AllowedModules.Contains(module.ToLowerInvariant());
}
