namespace CacheService.Domain;

/// <summary>
/// Represents cache configuration settings
/// </summary>
public class CacheConfiguration
{
    public string ConnectionString { get; set; } = "localhost:6379";
    public int DefaultTtlSeconds { get; set; } = 3600; // 1 hour
    public int MaxTtlSeconds { get; set; } = 86400; // 24 hours
    public int MaxKeySizeBytes { get; set; } = 1024; // 1 KB
    public int MaxValueSizeBytes { get; set; } = 1048576; // 1 MB
    public bool EnableStatistics { get; set; } = true;
    public bool EnableCompression { get; set; } = false;
    public int ConnectTimeoutMs { get; set; } = 5000;
    public int SyncTimeoutMs { get; set; } = 5000;
    public int RetryMaxAttempts { get; set; } = 3;
    public string? InstanceName { get; set; }

    /// <summary>
    /// Validates the TTL is within acceptable limits
    /// </summary>
    public bool IsValidTtl(int ttlSeconds)
    {
        return ttlSeconds > 0 && ttlSeconds <= MaxTtlSeconds;
    }

    /// <summary>
    /// Gets the default TTL as TimeSpan
    /// </summary>
    public TimeSpan GetDefaultTtl()
    {
        return TimeSpan.FromSeconds(DefaultTtlSeconds);
    }

    /// <summary>
    /// Gets the max TTL as TimeSpan
    /// </summary>
    public TimeSpan GetMaxTtl()
    {
        return TimeSpan.FromSeconds(MaxTtlSeconds);
    }
}
