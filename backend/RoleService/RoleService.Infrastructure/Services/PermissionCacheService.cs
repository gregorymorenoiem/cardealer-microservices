using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using RoleService.Application.Interfaces;

namespace RoleService.Infrastructure.Services;

/// <summary>
/// Implementación de cache de permisos usando Redis (IDistributedCache).
/// TTL: 5 minutos para verificaciones individuales.
/// TTL: 10 minutos para lista completa de permisos de un rol.
/// </summary>
public class PermissionCacheService : IPermissionCacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<PermissionCacheService> _logger;
    
    /// <summary>
    /// TTL para verificaciones individuales de permisos (5 minutos según matriz).
    /// </summary>
    private static readonly TimeSpan PermissionCheckTtl = TimeSpan.FromMinutes(5);
    
    /// <summary>
    /// TTL para lista completa de permisos de un rol.
    /// </summary>
    private static readonly TimeSpan RolePermissionsTtl = TimeSpan.FromMinutes(10);

    /// <summary>
    /// Prefijo para claves de verificación de permisos.
    /// Formato: perm:check:{roleId}:{resource}:{action}
    /// </summary>
    private const string PermissionCheckPrefix = "perm:check:";

    /// <summary>
    /// Prefijo para claves de lista de permisos de rol.
    /// Formato: perm:role:{roleId}
    /// </summary>
    private const string RolePermissionsPrefix = "perm:role:";

    public PermissionCacheService(
        IDistributedCache cache,
        ILogger<PermissionCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<bool?> GetCachedPermissionCheckAsync(
        Guid roleId,
        string resource,
        string action,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var key = BuildPermissionCheckKey(roleId, resource, action);
            var cached = await _cache.GetStringAsync(key, cancellationToken);

            if (string.IsNullOrEmpty(cached))
            {
                _logger.LogDebug("Cache MISS for permission check: {Key}", key);
                return null;
            }

            _logger.LogDebug("Cache HIT for permission check: {Key} = {Value}", key, cached);
            return cached == "1";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting cached permission check for role {RoleId}", roleId);
            return null; // Fail open - continuará con la verificación en DB
        }
    }

    public async Task SetCachedPermissionCheckAsync(
        Guid roleId,
        string resource,
        string action,
        bool hasPermission,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var key = BuildPermissionCheckKey(roleId, resource, action);
            var value = hasPermission ? "1" : "0";

            await _cache.SetStringAsync(
                key,
                value,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = PermissionCheckTtl
                },
                cancellationToken);

            _logger.LogDebug("Cached permission check: {Key} = {Value} (TTL: {Ttl})", key, value, PermissionCheckTtl);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error setting cached permission check for role {RoleId}", roleId);
            // No lanzar - el cache es opcional
        }
    }

    public async Task InvalidateRolePermissionsAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Invalidar la lista de permisos del rol
            var roleKey = $"{RolePermissionsPrefix}{roleId}";
            await _cache.RemoveAsync(roleKey, cancellationToken);

            // Nota: Para invalidar todos los checks individuales de un rol,
            // necesitaríamos Redis SCAN o un pattern delete.
            // Por simplicidad, dejamos que expiren naturalmente (5 min TTL).
            // En producción, considerar usar Redis SET para tracking de keys.

            _logger.LogInformation("Invalidated cache for role {RoleId}", roleId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error invalidating cache for role {RoleId}", roleId);
        }
    }

    public async Task InvalidateAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // IDistributedCache no tiene un método para invalidar todo.
            // En producción con Redis, usar FLUSHDB o SCAN+DELETE.
            // Por ahora, loguear advertencia.
            _logger.LogWarning(
                "InvalidateAllAsync called - IDistributedCache does not support pattern deletion. " +
                "Cache entries will expire naturally. Consider using Redis directly for full cache flush.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in InvalidateAllAsync");
        }
    }

    public async Task<IReadOnlyList<string>?> GetCachedRolePermissionsAsync(
        Guid roleId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var key = $"{RolePermissionsPrefix}{roleId}";
            var cached = await _cache.GetStringAsync(key, cancellationToken);

            if (string.IsNullOrEmpty(cached))
            {
                _logger.LogDebug("Cache MISS for role permissions: {Key}", key);
                return null;
            }

            var permissions = JsonSerializer.Deserialize<List<string>>(cached);
            _logger.LogDebug("Cache HIT for role permissions: {Key} ({Count} permissions)", key, permissions?.Count ?? 0);
            return permissions?.AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting cached role permissions for role {RoleId}", roleId);
            return null;
        }
    }

    public async Task SetCachedRolePermissionsAsync(
        Guid roleId,
        IReadOnlyList<string> permissionNames,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var key = $"{RolePermissionsPrefix}{roleId}";
            var value = JsonSerializer.Serialize(permissionNames);

            await _cache.SetStringAsync(
                key,
                value,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = RolePermissionsTtl
                },
                cancellationToken);

            _logger.LogDebug("Cached role permissions: {Key} ({Count} permissions, TTL: {Ttl})", 
                key, permissionNames.Count, RolePermissionsTtl);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error setting cached role permissions for role {RoleId}", roleId);
        }
    }

    /// <summary>
    /// Construye la clave de cache para una verificación de permiso.
    /// Formato: perm:check:{roleId}:{resource}:{action}
    /// </summary>
    private static string BuildPermissionCheckKey(Guid roleId, string resource, string action)
    {
        return $"{PermissionCheckPrefix}{roleId}:{resource.ToLowerInvariant()}:{action.ToLowerInvariant()}";
    }
}
