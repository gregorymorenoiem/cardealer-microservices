using Microsoft.Extensions.Logging;

namespace SchedulerService.Infrastructure.Jobs;

/// <summary>
/// Example job: Clean up old job execution records
/// </summary>
public class CleanupOldExecutionsJob : IScheduledJob
{
    private readonly ILogger<CleanupOldExecutionsJob> _logger;

    public CleanupOldExecutionsJob(ILogger<CleanupOldExecutionsJob> logger)
    {
        _logger = logger;
    }

    public async Task ExecuteAsync(Dictionary<string, string> parameters)
    {
        _logger.LogInformation("Starting cleanup of old job executions...");

        try
        {
            // Get retention days from parameters or use default
            var retentionDays = parameters.ContainsKey("RetentionDays")
                ? int.Parse(parameters["RetentionDays"])
                : 30;

            var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);

            _logger.LogInformation(
                "Cleaning up executions older than {CutoffDate} ({RetentionDays} days)",
                cutoffDate,
                retentionDays);

            // TODO: Implement actual cleanup logic
            // This would query the database and delete old records
            await Task.Delay(1000); // Simulate work

            _logger.LogInformation("Cleanup completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during cleanup job execution");
            throw;
        }
    }
}
