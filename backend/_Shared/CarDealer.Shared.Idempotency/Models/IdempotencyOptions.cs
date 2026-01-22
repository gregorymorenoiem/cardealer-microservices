namespace CarDealer.Shared.Idempotency.Models;

/// <summary>
/// Configuration options for idempotency
/// </summary>
public class IdempotencyOptions
{
    /// <summary>
    /// Configuration section name
    /// </summary>
    public const string SectionName = "Idempotency";

    /// <summary>
    /// Whether idempotency is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Redis connection string
    /// </summary>
    public string RedisConnection { get; set; } = "localhost:6379";

    /// <summary>
    /// Default TTL for idempotency records in seconds (default: 24 hours)
    /// </summary>
    public int DefaultTtlSeconds { get; set; } = 86400;

    /// <summary>
    /// Header name for the idempotency key (default: X-Idempotency-Key)
    /// </summary>
    public string HeaderName { get; set; } = "X-Idempotency-Key";

    /// <summary>
    /// Whether to require idempotency key for POST/PUT/PATCH requests on marked endpoints
    /// </summary>
    public bool RequireIdempotencyKey { get; set; } = true;

    /// <summary>
    /// Paths to exclude from idempotency checks
    /// </summary>
    public List<string> ExcludedPaths { get; set; } = new()
    {
        "/health",
        "/swagger",
        "/metrics"
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
    public string KeyPrefix { get; set; } = "idempotency";

    /// <summary>
    /// Whether to validate request body hash
    /// </summary>
    public bool ValidateRequestHash { get; set; } = true;

    /// <summary>
    /// Timeout for processing requests in seconds
    /// </summary>
    public int ProcessingTimeoutSeconds { get; set; } = 60;
}
