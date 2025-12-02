namespace RateLimitingService.Core.Models;

/// <summary>
/// Rate limit policy defining limits for a specific resource
/// </summary>
public class RateLimitPolicy
{
    /// <summary>
    /// Unique identifier for the policy
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Name of the policy (e.g., "api-default", "auth-strict")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the policy
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// User tier this policy applies to
    /// </summary>
    public RateLimitTier Tier { get; set; } = RateLimitTier.Free;

    /// <summary>
    /// Time window in seconds
    /// </summary>
    public int WindowSeconds { get; set; } = 60;

    /// <summary>
    /// Maximum number of requests allowed in the time window
    /// </summary>
    public int MaxRequests { get; set; } = 100;

    /// <summary>
    /// Burst limit - maximum requests allowed in a short burst
    /// </summary>
    public int BurstLimit { get; set; } = 10;

    /// <summary>
    /// Whether this policy is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Endpoints this policy applies to (supports wildcards)
    /// </summary>
    public List<string> Endpoints { get; set; } = new();

    /// <summary>
    /// IPs excluded from this policy
    /// </summary>
    public List<string> ExcludedIps { get; set; } = new();

    /// <summary>
    /// When the policy was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the policy was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Rate limit tiers with different limits
/// </summary>
public enum RateLimitTier
{
    /// <summary>
    /// Anonymous/unauthenticated users
    /// </summary>
    Anonymous = 1,

    /// <summary>
    /// Free tier users
    /// </summary>
    Free = 2,

    /// <summary>
    /// Basic plan users
    /// </summary>
    Basic = 3,

    /// <summary>
    /// Premium plan users
    /// </summary>
    Premium = 4,

    /// <summary>
    /// Enterprise users
    /// </summary>
    Enterprise = 5,

    /// <summary>
    /// Unlimited - no rate limiting
    /// </summary>
    Unlimited = 6
}
