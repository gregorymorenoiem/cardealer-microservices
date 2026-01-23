namespace RoleService.Application.Interfaces;

/// <summary>
/// Servicio de cache para permisos.
/// Implementa caching distribuido con Redis para CheckPermission.
/// TTL: 5 minutos según especificación.
/// </summary>
public interface IPermissionCacheService
{
    /// <summary>
    /// Verifica si un rol tiene un permiso específico (desde cache).
    /// </summary>
    /// <param name="roleId">ID del rol</param>
    /// <param name="resource">Recurso (ej: "vehicles")</param>
    /// <param name="action">Acción (ej: "create")</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>true si tiene permiso, false si no, null si no está en cache</returns>
    Task<bool?> GetCachedPermissionCheckAsync(
        Guid roleId,
        string resource,
        string action,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Almacena el resultado de una verificación de permiso en cache.
    /// </summary>
    /// <param name="roleId">ID del rol</param>
    /// <param name="resource">Recurso</param>
    /// <param name="action">Acción</param>
    /// <param name="hasPermission">Resultado de la verificación</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    Task SetCachedPermissionCheckAsync(
        Guid roleId,
        string resource,
        string action,
        bool hasPermission,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalida todo el cache de permisos para un rol específico.
    /// Debe llamarse cuando se modifican permisos del rol.
    /// </summary>
    /// <param name="roleId">ID del rol a invalidar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    Task InvalidateRolePermissionsAsync(Guid roleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalida todo el cache de permisos.
    /// Útil para cambios masivos o troubleshooting.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación</param>
    Task InvalidateAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todos los permisos de un rol desde cache.
    /// </summary>
    /// <param name="roleId">ID del rol</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de nombres de permisos, o null si no está en cache</returns>
    Task<IReadOnlyList<string>?> GetCachedRolePermissionsAsync(
        Guid roleId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Almacena todos los permisos de un rol en cache.
    /// </summary>
    /// <param name="roleId">ID del rol</param>
    /// <param name="permissionNames">Lista de nombres de permisos</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    Task SetCachedRolePermissionsAsync(
        Guid roleId,
        IReadOnlyList<string> permissionNames,
        CancellationToken cancellationToken = default);
}
