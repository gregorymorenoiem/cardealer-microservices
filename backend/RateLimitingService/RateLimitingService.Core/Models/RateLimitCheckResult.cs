namespace RateLimitingService.Core.Models;

/// <summary>
/// Result of rate limit check
/// </summary>
public class RateLimitCheckResult
{
    /// <summary>
    /// Is the request allowed?
    /// </summary>
    public bool IsAllowed { get; set; }

    /// <summary>
    /// Remaining requests in current window
    /// </summary>
    public int Remaining { get; set; }

    /// <summary>
    /// Maximum requests allowed in window
    /// </summary>
    public int Limit { get; set; }

    /// <summary>
    /// Time when limit resets (Unix timestamp)
    /// </summary>
    public long ResetAt { get; set; }

    /// <summary>
    /// Seconds until reset
    /// </summary>
    public int RetryAfterSeconds { get; set; }

    /// <summary>
    /// Rule that was applied
    /// </summary>
    public string? RuleId { get; set; }

    /// <summary>
    /// Reason for denial (if not allowed)
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// Is this identifier blacklisted?
    /// </summary>
    public bool IsBlacklisted { get; set; }

    /// <summary>
    /// Is this identifier whitelisted?
    /// </summary>
    public bool IsWhitelisted { get; set; }
}
