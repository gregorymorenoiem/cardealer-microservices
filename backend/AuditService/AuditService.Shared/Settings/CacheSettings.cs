namespace AuditService.Shared.Settings;

/// <summary>
/// Configuration settings for caching
/// </summary>
public class CacheSettings
{
    /// <summary>
    /// Redis connection string
    /// </summary>
    public string RedisConnectionString { get; set; } = "localhost:6379";

    /// <summary>
    /// Default cache expiration time in minutes
    /// </summary>
    public int DefaultExpirationMinutes { get; set; } = 30;

    /// <summary>
    /// Audit log cache expiration in minutes
    /// </summary>
    public int AuditLogCacheExpirationMinutes { get; set; } = 15;

    /// <summary>
    /// Statistics cache expiration in minutes
    /// </summary>
    public int StatisticsCacheExpirationMinutes { get; set; } = 5;

    /// <summary>
    /// Whether to enable distributed caching
    /// </summary>
    public bool EnableDistributedCache { get; set; } = true;

    /// <summary>
    /// Whether to enable in-memory caching as fallback
    /// </summary>
    public bool EnableMemoryCache { get; set; } = true;

    /// <summary>
    /// Maximum memory cache size in megabytes
    /// </summary>
    public int MaxMemoryCacheSizeMB { get; set; } = 100;

    /// <summary>
    /// Cache key prefix for audit service
    /// </summary>
    public string CacheKeyPrefix { get; set; } = "auditservice_";

    /// <summary>
    /// Cache key for audit statistics
    /// </summary>
    public string StatisticsCacheKey { get; set; } = "audit_statistics";

    /// <summary>
    /// Cache key for user audit history
    /// </summary>
    public string UserAuditHistoryKey { get; set; } = "user_audit_history_{0}";

    /// <summary>
    /// Cache key for recent audits
    /// </summary>
    public string RecentAuditsKey { get; set; } = "recent_audits";
}