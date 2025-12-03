namespace RateLimitingService.Core.Models;

/// <summary>
/// Request to check rate limit
/// </summary>
public class RateLimitCheckRequest
{
    /// <summary>
    /// Identifier to check (userId, IP address, API key, etc.)
    /// </summary>
    public string Identifier { get; set; } = string.Empty;

    /// <summary>
    /// Type of identifier
    /// </summary>
    public RateLimitIdentifierType IdentifierType { get; set; }

    /// <summary>
    /// Endpoint being accessed (optional, for endpoint-specific limits)
    /// </summary>
    public string? Endpoint { get; set; }

    /// <summary>
    /// Cost of this request (default 1, can be higher for expensive operations)
    /// </summary>
    public int Cost { get; set; } = 1;

    /// <summary>
    /// Additional context for custom rules
    /// </summary>
    public Dictionary<string, string> Context { get; set; } = new();

    // Helper property for compatibility
    public Dictionary<string, string> Metadata => Context;
}
