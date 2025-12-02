using SchedulerService.Domain.Enums;

namespace SchedulerService.Domain.Entities;

/// <summary>
/// Represents a single execution instance of a job
/// </summary>
public class JobExecution
{
    public Guid Id { get; set; }
    public Guid JobId { get; set; }
    public ExecutionStatus Status { get; set; } = ExecutionStatus.Scheduled;
    public DateTime ScheduledAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int AttemptNumber { get; set; } = 1;
    public string? ErrorMessage { get; set; }
    public string? StackTrace { get; set; }
    public string? Result { get; set; }
    public long? DurationMs { get; set; }
    public string? ExecutedBy { get; set; } // Server/Worker name

    // Navigation property
    public Job Job { get; set; } = null!;

    public void MarkAsRunning(string executedBy)
    {
        Status = ExecutionStatus.Running;
        StartedAt = DateTime.UtcNow;
        ExecutedBy = executedBy;
    }

    public void MarkAsSucceeded(string? result = null)
    {
        Status = ExecutionStatus.Succeeded;
        CompletedAt = DateTime.UtcNow;
        Result = result;
        CalculateDuration();
    }

    public void MarkAsFailed(string errorMessage, string? stackTrace = null)
    {
        Status = ExecutionStatus.Failed;
        CompletedAt = DateTime.UtcNow;
        ErrorMessage = errorMessage;
        StackTrace = stackTrace;
        CalculateDuration();
    }

    public void MarkAsCancelled()
    {
        Status = ExecutionStatus.Cancelled;
        CompletedAt = DateTime.UtcNow;
        CalculateDuration();
    }

    public void MarkAsRetrying()
    {
        Status = ExecutionStatus.Retrying;
        AttemptNumber++;
    }

    private void CalculateDuration()
    {
        if (StartedAt.HasValue && CompletedAt.HasValue)
        {
            DurationMs = (long)(CompletedAt.Value - StartedAt.Value).TotalMilliseconds;
        }
    }

    public bool IsTerminalState() => Status is ExecutionStatus.Succeeded
        or ExecutionStatus.Failed
        or ExecutionStatus.Cancelled;
}
