namespace MediaService.Shared.Settings;

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
    /// Media metadata cache expiration in minutes
    /// </summary>
    public int MediaCacheExpirationMinutes { get; set; } = 60;

    /// <summary>
    /// Upload URLs cache expiration in minutes
    /// </summary>
    public int UploadUrlCacheExpirationMinutes { get; set; } = 5;

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
    /// Cache key prefix for media service
    /// </summary>
    public string CacheKeyPrefix { get; set; } = "mediaservice_";

    /// <summary>
    /// Cache key for media by ID
    /// </summary>
    public string MediaByIdKey { get; set; } = "media_{0}";

    /// <summary>
    /// Cache key for user media list
    /// </summary>
    public string UserMediaListKey { get; set; } = "user_media_{0}";

    /// <summary>
    /// Cache key for upload URLs
    /// </summary>
    public string UploadUrlKey { get; set; } = "upload_url_{0}";
}