namespace RateLimitingService.Core.Models;

/// <summary>
/// Rate limit rule configuration
/// </summary>
public class RateLimitRule
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Identifier type: User, IP, ApiKey, Endpoint, Custom
    /// </summary>
    public RateLimitIdentifierType IdentifierType { get; set; }

    /// <summary>
    /// Specific identifier value (e.g., userId, IP address, endpoint path)
    /// </summary>
    public string? IdentifierValue { get; set; }

    /// <summary>
    /// Algorithm to use for rate limiting
    /// </summary>
    public RateLimitAlgorithm Algorithm { get; set; } = RateLimitAlgorithm.SlidingWindow;

    /// <summary>
    /// Maximum number of requests allowed
    /// </summary>
    public int Limit { get; set; }

    /// <summary>
    /// Time window in seconds
    /// </summary>
    public int WindowSeconds { get; set; }

    /// <summary>
    /// Burst capacity (for Token Bucket)
    /// </summary>
    public int? BurstCapacity { get; set; }

    /// <summary>
    /// Priority (higher = applied first)
    /// </summary>
    public int Priority { get; set; } = 0;

    /// <summary>
    /// Is this rule active?
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Whitelisted identifiers (bypass rate limit)
    /// </summary>
    public List<string> Whitelist { get; set; } = new();

    /// <summary>
    /// Blacklisted identifiers (always blocked)
    /// </summary>
    public List<string> Blacklist { get; set; } = new();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = "system";

    // Helper properties for compatibility
    public TimeSpan WindowSize => TimeSpan.FromSeconds(WindowSeconds);
    public string? EndpointPattern => IdentifierValue;  // For endpoint-specific rules
}

public enum RateLimitIdentifierType
{
    User,
    IP,
    ApiKey,
    Endpoint,
    Custom,
    UserId = User,  // Alias
    IpAddress = IP,  // Alias
    Global = 100    // Special type for global limits
}
