using IdempotencyService.Core.Models;

namespace IdempotencyService.Core.Interfaces;

/// <summary>
/// Service for managing idempotency records
/// </summary>
public interface IIdempotencyService
{
    /// <summary>
    /// Check if an idempotency key exists and get its record
    /// </summary>
    Task<IdempotencyCheckResult> CheckAsync(string key, string? requestHash = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new idempotency record in processing state
    /// </summary>
    Task<bool> StartProcessingAsync(IdempotencyRecord record, CancellationToken cancellationToken = default);

    /// <summary>
    /// Complete an idempotency record with the response
    /// </summary>
    Task<bool> CompleteAsync(string key, int statusCode, string responseBody, string contentType = "application/json", CancellationToken cancellationToken = default);

    /// <summary>
    /// Mark an idempotency record as failed
    /// </summary>
    Task<bool> FailAsync(string key, string? errorMessage = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get an idempotency record by key
    /// </summary>
    Task<IdempotencyRecord?> GetAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete an idempotency record
    /// </summary>
    Task<bool> DeleteAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get statistics about idempotency records
    /// </summary>
    Task<IdempotencyStats> GetStatsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Clean up expired records (for maintenance)
    /// </summary>
    Task<int> CleanupExpiredAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Statistics about idempotency records
/// </summary>
public class IdempotencyStats
{
    public long TotalRecords { get; set; }
    public long ProcessingRecords { get; set; }
    public long CompletedRecords { get; set; }
    public long FailedRecords { get; set; }
    public long DuplicateRequestsBlocked { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
