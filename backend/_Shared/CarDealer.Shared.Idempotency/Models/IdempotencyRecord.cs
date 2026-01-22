namespace CarDealer.Shared.Idempotency.Models;

/// <summary>
/// Represents an idempotency record stored in the cache
/// </summary>
public class IdempotencyRecord
{
    /// <summary>
    /// Unique idempotency key
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// The HTTP method of the original request
    /// </summary>
    public string HttpMethod { get; set; } = string.Empty;

    /// <summary>
    /// The request path
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Hash of the request body for validation
    /// </summary>
    public string RequestHash { get; set; } = string.Empty;

    /// <summary>
    /// The HTTP status code of the original response
    /// </summary>
    public int ResponseStatusCode { get; set; }

    /// <summary>
    /// The response body (JSON)
    /// </summary>
    public string ResponseBody { get; set; } = string.Empty;

    /// <summary>
    /// Response content type
    /// </summary>
    public string ResponseContentType { get; set; } = "application/json";

    /// <summary>
    /// Response headers to replay
    /// </summary>
    public Dictionary<string, string> ResponseHeaders { get; set; } = new();

    /// <summary>
    /// When the record was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the record expires
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Processing status of the request
    /// </summary>
    public IdempotencyStatus Status { get; set; } = IdempotencyStatus.Processing;

    /// <summary>
    /// Client identifier (optional)
    /// </summary>
    public string? ClientId { get; set; }

    /// <summary>
    /// User ID if authenticated
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Additional metadata
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();
}

/// <summary>
/// Status of an idempotency record
/// </summary>
public enum IdempotencyStatus
{
    /// <summary>
    /// Request is currently being processed
    /// </summary>
    Processing,

    /// <summary>
    /// Request completed successfully
    /// </summary>
    Completed,

    /// <summary>
    /// Request failed
    /// </summary>
    Failed
}
