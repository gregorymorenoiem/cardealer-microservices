namespace SchedulerService.Infrastructure.Jobs;

/// <summary>
/// Base interface for all job implementations
/// </summary>
public interface IScheduledJob
{
    Task ExecuteAsync(Dictionary<string, string> parameters);
}
