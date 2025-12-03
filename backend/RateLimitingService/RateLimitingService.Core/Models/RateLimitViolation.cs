namespace RateLimitingService.Core.Models;

/// <summary>
/// Record of rate limit violation
/// </summary>
public class RateLimitViolation
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Identifier { get; set; } = string.Empty;
    public RateLimitIdentifierType IdentifierType { get; set; }
    public string? Endpoint { get; set; }
    public string RuleId { get; set; } = string.Empty;
    public string RuleName { get; set; } = string.Empty;
    public int AttemptedRequests { get; set; }
    public int AllowedLimit { get; set; }
    public DateTime ViolatedAt { get; set; } = DateTime.UtcNow;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();

    // Helper properties for compatibility
    public DateTime Timestamp => ViolatedAt;
    public string Reason => $"Exceeded limit of {AllowedLimit} requests";
    public int Limit => AllowedLimit;
    public TimeSpan WindowSize => TimeSpan.FromSeconds(60); // Default
}
