namespace CarDealer.Shared.RateLimiting.Models;

/// <summary>
/// Configuration options for rate limiting
/// </summary>
public class RateLimitOptions
{
    /// <summary>
    /// Configuration section name
    /// </summary>
    public const string SectionName = "RateLimiting";

    /// <summary>
    /// Whether rate limiting is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Redis connection string for distributed rate limiting
    /// </summary>
    public string RedisConnection { get; set; } = "localhost:6379";

    /// <summary>
    /// URL of the RateLimitingService API (if using HTTP mode)
    /// </summary>
    public string ServiceUrl { get; set; } = "http://ratelimitingservice:8080";

    /// <summary>
    /// Mode: "redis" for direct Redis, "http" for API calls to RateLimitingService
    /// </summary>
    public string Mode { get; set; } = "redis";

    /// <summary>
    /// Default requests per window
    /// </summary>
    public int DefaultLimit { get; set; } = 100;

    /// <summary>
    /// Default window size in seconds
    /// </summary>
    public int DefaultWindowSeconds { get; set; } = 60;

    /// <summary>
    /// Paths to exclude from rate limiting
    /// </summary>
    public List<string> ExcludedPaths { get; set; } = new()
    {
        "/health",
        "/swagger",
        "/metrics",
        "/.well-known"
    };

    /// <summary>
    /// Key prefix in Redis
    /// </summary>
    public string KeyPrefix { get; set; } = "ratelimit";

    /// <summary>
    /// Rate limit policies by endpoint pattern
    /// </summary>
    public Dictionary<string, EndpointRateLimitPolicy> Policies { get; set; } = new();

    /// <summary>
    /// Header to use for client identification (default: X-Client-Id or IP)
    /// </summary>
    public string ClientIdHeader { get; set; } = "X-Client-Id";

    /// <summary>
    /// Whether to use IP address as fallback for client identification
    /// </summary>
    public bool UseIpAsFallback { get; set; } = true;

    /// <summary>
    /// Whether to include user ID in rate limit key (if authenticated)
    /// </summary>
    public bool IncludeUserId { get; set; } = true;
}

/// <summary>
/// Rate limit policy for a specific endpoint pattern
/// </summary>
public class EndpointRateLimitPolicy
{
    /// <summary>
    /// Endpoint pattern (e.g., "/api/auth/*", "/api/vehicles/POST")
    /// </summary>
    public string Pattern { get; set; } = string.Empty;

    /// <summary>
    /// Maximum requests allowed in the window
    /// </summary>
    public int Limit { get; set; } = 100;

    /// <summary>
    /// Window size in seconds
    /// </summary>
    public int WindowSeconds { get; set; } = 60;

    /// <summary>
    /// Whether this policy is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Different limits by user tier (e.g., "anonymous": 50, "authenticated": 100, "premium": 500)
    /// </summary>
    public Dictionary<string, int> TierLimits { get; set; } = new();
}
