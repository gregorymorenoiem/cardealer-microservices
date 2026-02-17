namespace RoleService.Application.DTOs.RolePermissions;

/// <summary>
/// Respuesta de verificación de permiso.
/// Incluye detalles sobre qué rol otorgó el permiso y si fue cacheado.
/// </summary>
public record CheckPermissionResponse
{
    /// <summary>
    /// Si el usuario tiene el permiso solicitado
    /// </summary>
    public bool HasPermission { get; init; }
    
    /// <summary>
    /// Nombre del permiso verificado (formato: resource:action)
    /// </summary>
    public string Permission { get; init; } = string.Empty;
    
    /// <summary>
    /// Nombre del rol que otorgó el permiso (si fue concedido)
    /// </summary>
    public string? GrantedByRole { get; init; }
    
    /// <summary>
    /// Si la respuesta provino del cache
    /// </summary>
    public bool Cached { get; init; }
    
    /// <summary>
    /// Mensaje descriptivo del resultado
    /// </summary>
    public string? Reason { get; init; }
}
