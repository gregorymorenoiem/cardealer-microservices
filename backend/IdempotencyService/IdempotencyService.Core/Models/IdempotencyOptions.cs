namespace IdempotencyService.Core.Models;

/// <summary>
/// Configuration options for the idempotency service
/// </summary>
public class IdempotencyOptions
{
    /// <summary>
    /// Configuration section name
    /// </summary>
    public const string SectionName = "Idempotency";

    /// <summary>
    /// Default TTL for idempotency records in seconds (default: 24 hours)
    /// </summary>
    public int DefaultTtlSeconds { get; set; } = 86400;

    /// <summary>
    /// Minimum TTL allowed in seconds (default: 1 minute)
    /// </summary>
    public int MinTtlSeconds { get; set; } = 60;

    /// <summary>
    /// Maximum TTL allowed in seconds (default: 7 days)
    /// </summary>
    public int MaxTtlSeconds { get; set; } = 604800;

    /// <summary>
    /// Header name for the idempotency key (default: X-Idempotency-Key)
    /// </summary>
    public string HeaderName { get; set; } = "X-Idempotency-Key";

    /// <summary>
    /// Whether to require idempotency key for POST/PUT/PATCH requests
    /// </summary>
    public bool RequireIdempotencyKey { get; set; } = false;

    /// <summary>
    /// Paths to exclude from idempotency checks (e.g., /health, /swagger)
    /// </summary>
    public List<string> ExcludedPaths { get; set; } = new()
    {
        "/health",
        "/swagger",
        "/api/idempotency"
    };

    /// <summary>
    /// HTTP methods that should be checked for idempotency
    /// </summary>
    public List<string> IdempotentMethods { get; set; } = new()
    {
        "POST",
        "PUT",
        "PATCH"
    };

    /// <summary>
    /// Redis key prefix for idempotency records
    /// </summary>
    public string KeyPrefix { get; set; } = "idempotency:";

    /// <summary>
    /// Whether to validate request hash on duplicate requests
    /// </summary>
    public bool ValidateRequestHash { get; set; } = true;

    /// <summary>
    /// Timeout for waiting on processing requests (in seconds)
    /// </summary>
    public int ProcessingTimeoutSeconds { get; set; } = 30;
}
