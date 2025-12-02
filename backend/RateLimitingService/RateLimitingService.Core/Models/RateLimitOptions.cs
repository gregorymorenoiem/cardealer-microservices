namespace RateLimitingService.Core.Models;

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
    /// Whether rate limiting is enabled globally
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Default rate limit if no policy matches
    /// </summary>
    public int DefaultLimit { get; set; } = 100;

    /// <summary>
    /// Default window in seconds
    /// </summary>
    public int DefaultWindowSeconds { get; set; } = 60;

    /// <summary>
    /// Redis key prefix
    /// </summary>
    public string KeyPrefix { get; set; } = "ratelimit:";

    /// <summary>
    /// Header name for client identification (e.g., API key)
    /// </summary>
    public string ClientIdHeader { get; set; } = "X-API-Key";

    /// <summary>
    /// Header name for user tier
    /// </summary>
    public string UserTierHeader { get; set; } = "X-User-Tier";

    /// <summary>
    /// Whether to use IP address as fallback client identifier
    /// </summary>
    public bool UseIpAsFallback { get; set; } = true;

    /// <summary>
    /// Paths to exclude from rate limiting
    /// </summary>
    public List<string> ExcludedPaths { get; set; } = new()
    {
        "/health",
        "/swagger",
        "/api/ratelimit"
    };

    /// <summary>
    /// Whether to include rate limit headers in response
    /// </summary>
    public bool IncludeHeaders { get; set; } = true;

    /// <summary>
    /// Custom response message when rate limited
    /// </summary>
    public string RateLimitExceededMessage { get; set; } = "Too many requests. Please try again later.";

    /// <summary>
    /// Predefined policies
    /// </summary>
    public List<RateLimitPolicy> Policies { get; set; } = new();
}

/// <summary>
/// User tier definitions for rate limiting
/// </summary>
public static class UserTiers
{
    public const string Free = "free";
    public const string Basic = "basic";
    public const string Premium = "premium";
    public const string Enterprise = "enterprise";
    public const string Unlimited = "unlimited";

    public static readonly Dictionary<string, int> DefaultLimits = new()
    {
        { Free, 100 },
        { Basic, 500 },
        { Premium, 2000 },
        { Enterprise, 10000 },
        { Unlimited, int.MaxValue }
    };
}
