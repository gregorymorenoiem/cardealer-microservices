using SchedulerService.Domain.Entities;

namespace SchedulerService.Domain.Interfaces;

/// <summary>
/// Interface for job scheduling operations
/// </summary>
public interface IJobScheduler
{
    /// <summary>
    /// Schedule a recurring job using cron expression
    /// </summary>
    string ScheduleRecurringJob(Job job);

    /// <summary>
    /// Schedule a one-time job with delay
    /// </summary>
    string ScheduleDelayedJob(Job job, TimeSpan delay);

    /// <summary>
    /// Remove a scheduled job
    /// </summary>
    bool RemoveScheduledJob(string jobId);

    /// <summary>
    /// Trigger immediate execution of a job
    /// </summary>
    string TriggerJob(Guid jobId);

    /// <summary>
    /// Pause a recurring job
    /// </summary>
    bool PauseJob(string jobId);

    /// <summary>
    /// Resume a paused job
    /// </summary>
    bool ResumeJob(string jobId);
}
