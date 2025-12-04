using Microsoft.Extensions.Logging;
using SchedulerService.Application.Interfaces;

namespace SchedulerService.Infrastructure.Jobs;

/// <summary>
/// Job that cleans up old job execution records based on retention policy
/// </summary>
public class CleanupOldExecutionsJob : IScheduledJob
{
    private readonly ILogger<CleanupOldExecutionsJob> _logger;
    private readonly IJobExecutionRepository _executionRepository;
    private const int DefaultRetentionDays = 30;

    public CleanupOldExecutionsJob(
        ILogger<CleanupOldExecutionsJob> logger,
        IJobExecutionRepository executionRepository)
    {
        _logger = logger;
        _executionRepository = executionRepository;
    }

    public async Task ExecuteAsync(Dictionary<string, string> parameters)
    {
        _logger.LogInformation("Starting cleanup of old job executions...");

        try
        {
            // Get retention days from parameters or use default
            var retentionDays = parameters.TryGetValue("RetentionDays", out var retentionStr)
                ? int.Parse(retentionStr)
                : DefaultRetentionDays;

            // Validate retention days (minimum 7 days for safety)
            retentionDays = Math.Max(retentionDays, 7);

            var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);

            _logger.LogInformation(
                "Cleaning up executions older than {CutoffDate} ({RetentionDays} days retention)",
                cutoffDate.ToString("yyyy-MM-dd HH:mm:ss"),
                retentionDays);

            // Delete old executions from database
            var deletedCount = await _executionRepository.DeleteOldExecutionsAsync(cutoffDate);

            _logger.LogInformation(
                "Cleanup completed successfully. Deleted {DeletedCount} old execution records",
                deletedCount);

            // Log metrics for monitoring
            if (deletedCount > 0)
            {
                _logger.LogInformation(
                    "Cleanup metrics: RetentionDays={RetentionDays}, CutoffDate={CutoffDate}, DeletedRecords={DeletedCount}",
                    retentionDays,
                    cutoffDate.ToString("yyyy-MM-dd"),
                    deletedCount);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during cleanup job execution");
            throw;
        }
    }
}
