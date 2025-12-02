namespace SchedulerService.Domain.Enums;

/// <summary>
/// Status of a scheduled job configuration
/// </summary>
public enum JobStatus
{
    /// <summary>
    /// Job is enabled and will be executed
    /// </summary>
    Enabled = 1,

    /// <summary>
    /// Job is disabled and will not be executed
    /// </summary>
    Disabled = 2,

    /// <summary>
    /// Job is paused temporarily
    /// </summary>
    Paused = 3,

    /// <summary>
    /// Job is archived/deleted
    /// </summary>
    Archived = 4
}
