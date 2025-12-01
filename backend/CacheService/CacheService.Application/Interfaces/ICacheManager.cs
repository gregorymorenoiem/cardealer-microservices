using CacheService.Domain;

namespace CacheService.Application.Interfaces;

/// <summary>
/// Interface for cache management operations
/// </summary>
public interface ICacheManager
{
    /// <summary>
    /// Gets a value from cache by key
    /// </summary>
    Task<string?> GetAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a value from cache by key with tenant isolation
    /// </summary>
    Task<string?> GetAsync(string key, string tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a value in cache with optional TTL
    /// </summary>
    Task<bool> SetAsync(string key, string value, TimeSpan? ttl = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a value in cache with tenant isolation
    /// </summary>
    Task<bool> SetAsync(string key, string value, string tenantId, TimeSpan? ttl = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a value from cache
    /// </summary>
    Task<bool> DeleteAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a key exists in cache
    /// </summary>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets multiple values from cache
    /// </summary>
    Task<Dictionary<string, string?>> GetManyAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets multiple values in cache
    /// </summary>
    Task<bool> SetManyAsync(Dictionary<string, string> keyValues, TimeSpan? ttl = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes multiple values from cache
    /// </summary>
    Task<long> DeleteManyAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes all keys matching a pattern
    /// </summary>
    Task<long> DeleteByPatternAsync(string pattern, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets cache entry metadata
    /// </summary>
    Task<CacheEntry?> GetEntryAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the TTL for a key
    /// </summary>
    Task<TimeSpan?> GetTtlAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets/updates the TTL for a key
    /// </summary>
    Task<bool> SetTtlAsync(string key, TimeSpan ttl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Flushes all cache data
    /// </summary>
    Task FlushAllAsync(CancellationToken cancellationToken = default);
}
