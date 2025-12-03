namespace MessageBusService.Domain.Enums;

/// <summary>
/// Status of a Saga execution
/// </summary>
public enum SagaStatus
{
    /// <summary>
    /// Saga has been created but not started
    /// </summary>
    Created = 0,

    /// <summary>
    /// Saga is currently executing steps
    /// </summary>
    Running = 1,

    /// <summary>
    /// All saga steps completed successfully
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Saga failed and compensation is in progress
    /// </summary>
    Compensating = 3,

    /// <summary>
    /// Saga compensation completed successfully (rolled back)
    /// </summary>
    Compensated = 4,

    /// <summary>
    /// Saga failed and could not be compensated
    /// </summary>
    Failed = 5,

    /// <summary>
    /// Saga was aborted by user or system
    /// </summary>
    Aborted = 6
}
