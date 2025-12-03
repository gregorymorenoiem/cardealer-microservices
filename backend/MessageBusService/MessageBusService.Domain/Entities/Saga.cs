using MessageBusService.Domain.Enums;

namespace MessageBusService.Domain.Entities;

/// <summary>
/// Represents a Saga - a sequence of local transactions with compensating actions
/// </summary>
public class Saga
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SagaType Type { get; set; }
    public SagaStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? FailedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public Dictionary<string, string>? Context { get; set; }
    public int CurrentStepIndex { get; set; }
    public int TotalSteps { get; set; }
    public int MaxRetryAttempts { get; set; } = 3;
    public int CurrentRetryAttempt { get; set; }
    public TimeSpan? Timeout { get; set; }

    // Navigation property
    public List<SagaStep> Steps { get; set; } = new();

    /// <summary>
    /// Adds a step to the saga
    /// </summary>
    public void AddStep(SagaStep step)
    {
        step.Order = Steps.Count;
        step.SagaId = Id;
        Steps.Add(step);
        TotalSteps = Steps.Count;
    }

    /// <summary>
    /// Marks saga as started
    /// </summary>
    public void Start()
    {
        Status = SagaStatus.Running;
        StartedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks saga as completed
    /// </summary>
    public void Complete()
    {
        Status = SagaStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks saga as failed and starts compensation
    /// </summary>
    public void Fail(string errorMessage)
    {
        Status = SagaStatus.Compensating;
        FailedAt = DateTime.UtcNow;
        ErrorMessage = errorMessage;
    }

    /// <summary>
    /// Marks saga as compensated
    /// </summary>
    public void Compensate()
    {
        Status = SagaStatus.Compensated;
    }

    /// <summary>
    /// Checks if saga has timed out
    /// </summary>
    public bool HasTimedOut()
    {
        if (!Timeout.HasValue || !StartedAt.HasValue)
            return false;

        return DateTime.UtcNow - StartedAt.Value > Timeout.Value;
    }

    /// <summary>
    /// Gets the next step to execute
    /// </summary>
    public SagaStep? GetNextStep()
    {
        return Steps
            .Where(s => s.Status == SagaStepStatus.Pending)
            .OrderBy(s => s.Order)
            .FirstOrDefault();
    }

    /// <summary>
    /// Gets steps that need compensation (in reverse order)
    /// </summary>
    public List<SagaStep> GetStepsToCompensate()
    {
        return Steps
            .Where(s => s.Status == SagaStepStatus.Completed)
            .OrderByDescending(s => s.Order)
            .ToList();
    }
}
