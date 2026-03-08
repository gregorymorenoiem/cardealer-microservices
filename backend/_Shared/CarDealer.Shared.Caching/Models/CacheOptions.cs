namespace CarDealer.Shared.Caching.Models;

/// <summary>
/// Configuration options for the standard caching layer.
/// Binds to the "Caching" section in appsettings.json.
/// </summary>
public class CacheOptions
{
    public const string SectionName = "Caching";

    /// <summary>
    /// Redis connection string. If empty, falls back to in-memory distributed cache.
    /// Can also be provided via REDIS_CONNECTION_STRING environment variable.
    /// </summary>
    public string RedisConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Instance name prefix for all cache keys (e.g., "okla:authservice:").
    /// Prevents key collisions when multiple services share the same Redis instance.
    /// </summary>
    public string InstanceName { get; set; } = "okla:";

    /// <summary>
    /// Default TTL in seconds for cached items when no explicit TTL is provided.
    /// Default: 300 seconds (5 minutes).
    /// </summary>
    public int DefaultTtlSeconds { get; set; } = 300;

    /// <summary>
    /// Maximum TTL in seconds to prevent accidental indefinite caching.
    /// Default: 86400 seconds (24 hours).
    /// </summary>
    public int MaxTtlSeconds { get; set; } = 86400;

    /// <summary>
    /// Whether to use a sliding expiration instead of absolute.
    /// Default: false (absolute expiration).
    /// </summary>
    public bool UseSlidingExpiration { get; set; } = false;

    /// <summary>
    /// Whether to enable cache metrics (hit/miss counters).
    /// Default: true.
    /// </summary>
    public bool EnableMetrics { get; set; } = true;

    /// <summary>
    /// Whether to fall back to in-memory cache when Redis is unavailable.
    /// Default: true (fail-open: return cache miss rather than throw).
    /// </summary>
    public bool FallbackToMemory { get; set; } = true;
}
