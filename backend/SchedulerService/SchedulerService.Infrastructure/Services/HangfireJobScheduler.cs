using Hangfire;
using SchedulerService.Domain.Entities;
using SchedulerService.Domain.Interfaces;

namespace SchedulerService.Infrastructure.Services;

public class HangfireJobScheduler : IJobScheduler
{
    private readonly IRecurringJobManager _recurringJobManager;
    private readonly IBackgroundJobClient _backgroundJobClient;

    public HangfireJobScheduler(
        IRecurringJobManager recurringJobManager,
        IBackgroundJobClient backgroundJobClient)
    {
        _recurringJobManager = recurringJobManager;
        _backgroundJobClient = backgroundJobClient;
    }

    public string ScheduleRecurringJob(Job job)
    {
        var jobId = job.Id.ToString();
        
        _recurringJobManager.AddOrUpdate(
            jobId,
            () => ExecuteJob(job.Id, job.JobType, job.Parameters),
            job.CronExpression,
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Utc
            });

        return jobId;
    }

    public string ScheduleDelayedJob(Job job, TimeSpan delay)
    {
        var jobId = _backgroundJobClient.Schedule(
            () => ExecuteJob(job.Id, job.JobType, job.Parameters),
            delay);

        return jobId;
    }

    public bool RemoveScheduledJob(string jobId)
    {
        _recurringJobManager.RemoveIfExists(jobId);
        return true;
    }

    public string TriggerJob(Guid jobId)
    {
        _recurringJobManager.Trigger(jobId.ToString());
        return jobId.ToString();
    }

    public bool PauseJob(string jobId)
    {
        _recurringJobManager.RemoveIfExists(jobId);
        return true;
    }

    public bool ResumeJob(string jobId)
    {
        // This will be handled by EnableJobCommand which reschedules the job
        return true;
    }

    [AutomaticRetry(Attempts = 3, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
    public async Task ExecuteJob(Guid jobId, string jobType, Dictionary<string, string> parameters)
    {
        // This method will be overridden by actual job implementations
        // For now, it's a placeholder that Hangfire will call
        await Task.CompletedTask;
    }
}
