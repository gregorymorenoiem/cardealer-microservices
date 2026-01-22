using CarDealer.Shared.Idempotency.Models;

namespace CarDealer.Shared.Idempotency.Interfaces;

/// <summary>
/// Interface for idempotency service
/// </summary>
public interface IIdempotencyClient
{
    /// <summary>
    /// Check if an idempotency key exists and get its record
    /// </summary>
    Task<IdempotencyCheckResult> CheckAsync(
        string key,
        string? requestHash = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new idempotency record in processing state
    /// </summary>
    Task<bool> StartProcessingAsync(
        IdempotencyRecord record,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Complete an idempotency record with the response
    /// </summary>
    Task<bool> CompleteAsync(
        string key,
        int statusCode,
        string responseBody,
        string contentType = "application/json",
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Mark an idempotency record as failed
    /// </summary>
    Task<bool> FailAsync(
        string key,
        string? errorMessage = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete an idempotency record
    /// </summary>
    Task<bool> DeleteAsync(
        string key,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate a hash of the request body for conflict detection
    /// </summary>
    string GenerateRequestHash(string requestBody);
}
