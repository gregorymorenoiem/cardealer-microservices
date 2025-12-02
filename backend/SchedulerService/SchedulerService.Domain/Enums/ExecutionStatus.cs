namespace SchedulerService.Domain.Enums;

/// <summary>
/// Status of a job execution instance
/// </summary>
public enum ExecutionStatus
{
    /// <summary>
    /// Execution is scheduled but not started
    /// </summary>
    Scheduled = 1,

    /// <summary>
    /// Execution is currently running
    /// </summary>
    Running = 2,

    /// <summary>
    /// Execution completed successfully
    /// </summary>
    Succeeded = 3,

    /// <summary>
    /// Execution failed with error
    /// </summary>
    Failed = 4,

    /// <summary>
    /// Execution was cancelled
    /// </summary>
    Cancelled = 5,

    /// <summary>
    /// Execution is being retried
    /// </summary>
    Retrying = 6
}
