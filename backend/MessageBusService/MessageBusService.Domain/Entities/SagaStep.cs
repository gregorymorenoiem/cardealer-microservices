using MessageBusService.Domain.Enums;

namespace MessageBusService.Domain.Entities;

/// <summary>
/// Represents a step in a Saga transaction
/// </summary>
public class SagaStep
{
    public Guid Id { get; set; }
    public Guid SagaId { get; set; }
    public int Order { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public string ActionPayload { get; set; } = string.Empty;
    public string? CompensationActionType { get; set; }
    public string? CompensationPayload { get; set; }
    public SagaStepStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? FailedAt { get; set; }
    public DateTime? CompensationStartedAt { get; set; }
    public DateTime? CompensationCompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ResponsePayload { get; set; }
    public int RetryAttempts { get; set; }
    public int MaxRetries { get; set; } = 3;
    public TimeSpan? Timeout { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }

    // Navigation property
    public Saga? Saga { get; set; }

    /// <summary>
    /// Marks step as started
    /// </summary>
    public void Start()
    {
        Status = SagaStepStatus.Running;
        StartedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks step as completed
    /// </summary>
    public void Complete(string? responsePayload = null)
    {
        Status = SagaStepStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        ResponsePayload = responsePayload;
    }

    /// <summary>
    /// Marks step as failed
    /// </summary>
    public void Fail(string errorMessage)
    {
        Status = SagaStepStatus.Failed;
        FailedAt = DateTime.UtcNow;
        ErrorMessage = errorMessage;
    }

    /// <summary>
    /// Starts compensation for this step
    /// </summary>
    public void StartCompensation()
    {
        Status = SagaStepStatus.Compensating;
        CompensationStartedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks compensation as completed
    /// </summary>
    public void CompleteCompensation()
    {
        Status = SagaStepStatus.Compensated;
        CompensationCompletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks compensation as failed
    /// </summary>
    public void FailCompensation(string errorMessage)
    {
        Status = SagaStepStatus.CompensationFailed;
        ErrorMessage = errorMessage;
    }

    /// <summary>
    /// Checks if step has timed out
    /// </summary>
    public bool HasTimedOut()
    {
        if (!Timeout.HasValue || !StartedAt.HasValue)
            return false;

        return DateTime.UtcNow - StartedAt.Value > Timeout.Value;
    }

    /// <summary>
    /// Checks if step can be retried
    /// </summary>
    public bool CanRetry()
    {
        return RetryAttempts < MaxRetries;
    }

    /// <summary>
    /// Increments retry attempt counter
    /// </summary>
    public void IncrementRetry()
    {
        RetryAttempts++;
    }

    /// <summary>
    /// Checks if step has compensation action
    /// </summary>
    public bool HasCompensation()
    {
        return !string.IsNullOrEmpty(CompensationActionType);
    }
}
