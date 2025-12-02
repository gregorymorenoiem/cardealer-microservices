namespace IdempotencyService.Core.Models;

/// <summary>
/// Result of checking an idempotency key
/// </summary>
public class IdempotencyCheckResult
{
    /// <summary>
    /// Whether a record was found for this key
    /// </summary>
    public bool Exists { get; set; }

    /// <summary>
    /// Whether the request is currently being processed
    /// </summary>
    public bool IsProcessing { get; set; }

    /// <summary>
    /// Whether the request has been completed
    /// </summary>
    public bool IsCompleted { get; set; }

    /// <summary>
    /// The existing record if found
    /// </summary>
    public IdempotencyRecord? Record { get; set; }

    /// <summary>
    /// Error message if any
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Whether the request hash matches (for conflict detection)
    /// </summary>
    public bool RequestHashMatches { get; set; } = true;

    public static IdempotencyCheckResult NotFound() => new()
    {
        Exists = false,
        IsProcessing = false,
        IsCompleted = false
    };

    public static IdempotencyCheckResult Processing(IdempotencyRecord record) => new()
    {
        Exists = true,
        IsProcessing = true,
        IsCompleted = false,
        Record = record
    };

    public static IdempotencyCheckResult Completed(IdempotencyRecord record) => new()
    {
        Exists = true,
        IsProcessing = false,
        IsCompleted = true,
        Record = record
    };

    public static IdempotencyCheckResult Conflict(IdempotencyRecord record, string message) => new()
    {
        Exists = true,
        IsProcessing = false,
        IsCompleted = true,
        Record = record,
        RequestHashMatches = false,
        ErrorMessage = message
    };
}
