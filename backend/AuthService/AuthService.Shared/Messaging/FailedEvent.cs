namespace AuthService.Shared.Messaging;

/// <summary>
/// Represents a failed event that needs to be retried.
/// Used by Dead Letter Queue to store and retry events that failed to publish.
/// </summary>
public class FailedEvent
{
    /// <summary>
    /// Unique identifier for this failed event.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Type of event (e.g., "auth.user.registered", "error.critical").
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// JSON serialized event data.
    /// </summary>
    public string EventJson { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the event failed to publish.
    /// </summary>
    public DateTime FailedAt { get; set; }

    /// <summary>
    /// Number of retry attempts made.
    /// </summary>
    public int RetryCount { get; set; } = 0;

    /// <summary>
    /// Scheduled time for next retry attempt.
    /// </summary>
    public DateTime? NextRetryAt { get; set; }

    /// <summary>
    /// Last error message from failed publish attempt.
    /// </summary>
    public string? LastError { get; set; }

    /// <summary>
    /// Schedules the next retry with exponential backoff.
    /// Backoff pattern: 1min, 2min, 4min, 8min, 16min (max).
    /// </summary>
    public void ScheduleNextRetry()
    {
        RetryCount++;
        // Exponential backoff: 2^(n-1) minutes, capped at 16 minutes
        var delayMinutes = Math.Min(Math.Pow(2, RetryCount - 1), 16);
        NextRetryAt = DateTime.UtcNow.AddMinutes(delayMinutes);
    }
}
