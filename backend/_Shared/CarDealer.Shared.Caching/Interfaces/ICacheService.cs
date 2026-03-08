namespace CarDealer.Shared.Caching.Interfaces;

/// <summary>
/// Standard cache service interface for all OKLA microservices.
/// Provides generic Get/Set/Remove operations with JSON serialization.
/// All operations are fail-safe: Redis failures return default values, never throw.
/// 
/// Key format convention: {service}:{entity}:{identifier}
/// Example: "vehiclessale:vehicle:550e8400-e29b-41d4-a716-446655440000"
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Get a cached value by key. Returns default(T) if not found or on error.
    /// </summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Set a cached value with optional TTL override (in seconds).
    /// If ttlSeconds is null, uses the configured default TTL.
    /// </summary>
    Task SetAsync<T>(string key, T value, int? ttlSeconds = null, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Remove a cached value by key.
    /// </summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a key exists in the cache.
    /// </summary>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a cached value or compute it if missing, then cache the result.
    /// Thread-safe: prevents cache stampede via the underlying cache provider.
    /// </summary>
    Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, int? ttlSeconds = null, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Remove all cached values matching a key pattern (e.g., "vehicle:*").
    /// NOTE: This operation uses Redis SCAN and may be slow for large key spaces.
    /// Only available when Redis is the backend; no-op for in-memory cache.
    /// </summary>
    Task InvalidateByPatternAsync(string pattern, CancellationToken cancellationToken = default);
}
