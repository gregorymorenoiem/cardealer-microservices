namespace CarDealer.Shared.RateLimiting.Models;

/// <summary>
/// Result of a rate limit check
/// </summary>
public class RateLimitResult
{
    /// <summary>
    /// Whether the request is allowed
    /// </summary>
    public bool IsAllowed { get; set; }

    /// <summary>
    /// Remaining requests in the current window
    /// </summary>
    public int RemainingRequests { get; set; }

    /// <summary>
    /// Maximum requests allowed (limit)
    /// </summary>
    public int Limit { get; set; }

    /// <summary>
    /// When the rate limit resets
    /// </summary>
    public DateTime ResetAt { get; set; }

    /// <summary>
    /// Time to wait before retrying (when rate limited)
    /// </summary>
    public TimeSpan RetryAfter { get; set; }

    /// <summary>
    /// Client identifier used for rate limiting
    /// </summary>
    public string ClientIdentifier { get; set; } = string.Empty;

    /// <summary>
    /// The policy name that was applied
    /// </summary>
    public string PolicyName { get; set; } = string.Empty;

    /// <summary>
    /// Creates an allowed result
    /// </summary>
    public static RateLimitResult Allowed(string clientId, string policyName, int remaining, DateTime resetAt, int limit = 0)
    {
        return new RateLimitResult
        {
            IsAllowed = true,
            ClientIdentifier = clientId,
            PolicyName = policyName,
            RemainingRequests = remaining,
            ResetAt = resetAt,
            Limit = limit > 0 ? limit : remaining + 1,
            RetryAfter = TimeSpan.Zero
        };
    }

    /// <summary>
    /// Creates a rate limited (denied) result
    /// </summary>
    public static RateLimitResult RateLimited(string clientId, string policyName, int limit, DateTime resetAt, TimeSpan retryAfter)
    {
        return new RateLimitResult
        {
            IsAllowed = false,
            ClientIdentifier = clientId,
            PolicyName = policyName,
            RemainingRequests = 0,
            Limit = limit,
            ResetAt = resetAt,
            RetryAfter = retryAfter
        };
    }

    /// <summary>
    /// Gets HTTP headers for the rate limit response
    /// </summary>
    public Dictionary<string, string> GetHeaders()
    {
        var headers = new Dictionary<string, string>
        {
            ["X-RateLimit-Limit"] = Limit.ToString(),
            ["X-RateLimit-Remaining"] = RemainingRequests.ToString(),
            ["X-RateLimit-Reset"] = new DateTimeOffset(ResetAt).ToUnixTimeSeconds().ToString()
        };

        if (!IsAllowed && RetryAfter.TotalSeconds > 0)
        {
            headers["Retry-After"] = ((int)RetryAfter.TotalSeconds).ToString();
        }

        return headers;
    }
}
