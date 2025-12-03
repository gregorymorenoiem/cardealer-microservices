namespace MessageBusService.Domain.Enums;

/// <summary>
/// Status of a Saga Step execution
/// </summary>
public enum SagaStepStatus
{
    /// <summary>
    /// Step has been created but not executed
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Step is currently executing
    /// </summary>
    Running = 1,

    /// <summary>
    /// Step completed successfully
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Step failed
    /// </summary>
    Failed = 3,

    /// <summary>
    /// Step is being compensated (rolled back)
    /// </summary>
    Compensating = 4,

    /// <summary>
    /// Step compensation completed successfully
    /// </summary>
    Compensated = 5,

    /// <summary>
    /// Step compensation failed
    /// </summary>
    CompensationFailed = 6,

    /// <summary>
    /// Step was skipped
    /// </summary>
    Skipped = 7
}
