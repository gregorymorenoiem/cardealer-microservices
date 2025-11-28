namespace AuthService.Shared;

/// <summary>
/// Cache configuration settings
/// </summary>
public class CacheSettings
{
    /// <summary>Redis connection string</summary>
    public string RedisConnectionString { get; set; } = "localhost:6379";

    /// <summary>Default cache expiration in minutes</summary>
    public int DefaultExpirationMinutes { get; set; } = 30;

    /// <summary>User cache expiration in minutes</summary>
    public int UserCacheExpirationMinutes { get; set; } = 15;

    /// <summary>Token cache expiration in minutes</summary>
    public int TokenCacheExpirationMinutes { get; set; } = 5;

    /// <summary>Whether to enable distributed caching</summary>
    public bool EnableDistributedCache { get; set; } = true;
}