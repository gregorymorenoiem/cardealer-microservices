using BackupDRService.Core.Entities;
using BackupDRService.Core.Models;
using BackupDRService.Core.Repositories;
using Cronos;
using Microsoft.Extensions.Logging;

namespace BackupDRService.Core.Services;

/// <summary>
/// Service for monitoring scheduler health and providing metrics
/// </summary>
public class SchedulerMonitoringService
{
    private readonly IBackupScheduleRepository _scheduleRepository;
    private readonly IBackupHistoryRepository _historyRepository;
    private readonly ILogger<SchedulerMonitoringService> _logger;

    public SchedulerMonitoringService(
        IBackupScheduleRepository scheduleRepository,
        IBackupHistoryRepository historyRepository,
        ILogger<SchedulerMonitoringService> logger)
    {
        _scheduleRepository = scheduleRepository;
        _historyRepository = historyRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get comprehensive health metrics for the scheduler
    /// </summary>
    public async Task<SchedulerHealthMetrics> GetHealthMetricsAsync()
    {
        var metrics = new SchedulerHealthMetrics();

        try
        {
            // Get all schedules
            var allSchedules = await _scheduleRepository.GetAllAsync();
            var enabledSchedules = allSchedules.Where(s => s.IsEnabled).ToList();

            // Get backup history for today
            var today = DateTime.UtcNow.Date;
            var todayBackups = await _historyRepository.GetByDatabaseNameAsync("");

            var scheduledBackupsToday = todayBackups
                .Where(h => h.ScheduleId.HasValue && h.CreatedAt >= today)
                .ToList();

            // Calculate stats
            metrics.Stats = new SchedulerStats
            {
                TotalSchedules = allSchedules.Count(),
                EnabledSchedules = enabledSchedules.Count,
                DisabledSchedules = allSchedules.Count() - enabledSchedules.Count,
                BackupsExecutedToday = scheduledBackupsToday.Count,
                FailedBackupsToday = scheduledBackupsToday.Count(h => h.Status == "Failed"),
                LastBackupTime = scheduledBackupsToday.MaxBy(h => h.CreatedAt)?.CreatedAt,
                NextScheduledBackup = enabledSchedules
                    .Where(s => s.NextRunAt.HasValue)
                    .MinBy(s => s.NextRunAt)?.NextRunAt
            };

            // Calculate success rate
            if (metrics.Stats.BackupsExecutedToday > 0)
            {
                var successCount = metrics.Stats.BackupsExecutedToday - metrics.Stats.FailedBackupsToday;
                metrics.Stats.SuccessRateToday =
                    (double)successCount / metrics.Stats.BackupsExecutedToday * 100;
            }

            // Count schedules due in next 24 hours
            var next24Hours = DateTime.UtcNow.AddHours(24);
            metrics.Stats.SchedulesDueNext24Hours = enabledSchedules
                .Count(s => s.NextRunAt.HasValue && s.NextRunAt.Value <= next24Hours);

            // Get active schedule details
            metrics.ActiveSchedules = enabledSchedules
                .OrderBy(s => s.NextRunAt)
                .Select(s => new ActiveSchedule
                {
                    Id = Guid.NewGuid(), // Generate new GUID as schedule uses int ID
                    Name = s.Name,
                    DatabaseName = s.DatabaseName,
                    CronExpression = s.CronExpression,
                    NextRunAt = s.NextRunAt,
                    LastRunAt = s.LastRunAt,
                    LastRunStatus = s.SuccessCount > 0 || s.FailureCount > 0
                        ? (s.FailureCount == 0 ? "Success" : "Mixed")
                        : null,
                    TimeUntilNextRun = s.NextRunAt.HasValue
                        ? s.NextRunAt.Value - DateTime.UtcNow
                        : null
                })
                .ToList();

            // Health checks
            var issues = new List<string>();

            // Check 1: Are there any enabled schedules?
            if (metrics.Stats.EnabledSchedules == 0)
            {
                issues.Add("No enabled schedules found");
            }

            // Check 2: Are schedules executing?
            if (metrics.Stats.EnabledSchedules > 0 && metrics.Stats.BackupsExecutedToday == 0)
            {
                var lastBackupHistory = await _historyRepository.GetByDatabaseNameAsync("");
                var lastScheduledBackup = lastBackupHistory
                    .Where(h => h.ScheduleId.HasValue && h.CreatedAt >= DateTime.UtcNow.AddDays(-7))
                    .OrderByDescending(h => h.CreatedAt)
                    .FirstOrDefault();

                if (lastScheduledBackup == null)
                {
                    issues.Add("No scheduled backups have executed in the last 7 days");
                }
            }

            // Check 3: High failure rate
            if (metrics.Stats.SuccessRateToday < 80 && metrics.Stats.BackupsExecutedToday >= 5)
            {
                issues.Add($"Success rate today is low: {metrics.Stats.SuccessRateToday:F1}%");
            }

            // Check 4: Overdue schedules
            var overdueSchedules = enabledSchedules
                .Where(s => s.NextRunAt.HasValue && s.NextRunAt.Value < DateTime.UtcNow.AddMinutes(-5))
                .ToList();

            if (overdueSchedules.Any())
            {
                issues.Add($"{overdueSchedules.Count} schedule(s) are overdue");
            }

            // Check 5: Invalid cron expressions
            foreach (var schedule in enabledSchedules)
            {
                if (!IsValidCronExpression(schedule.CronExpression))
                {
                    issues.Add($"Schedule '{schedule.Name}' has invalid cron expression");
                }
            }

            metrics.Issues = issues;
            metrics.IsHealthy = !issues.Any();
            metrics.Status = metrics.IsHealthy ? "Healthy" : "Degraded";

            if (!metrics.IsHealthy)
            {
                _logger.LogWarning(
                    "⚠️ Scheduler health check found {Count} issue(s): {Issues}",
                    issues.Count,
                    string.Join("; ", issues));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting scheduler health metrics");
            metrics.IsHealthy = false;
            metrics.Status = "Error";
            metrics.Issues.Add($"Error: {ex.Message}");
        }

        return metrics;
    }

    /// <summary>
    /// Get summary of upcoming scheduled backups
    /// </summary>
    public async Task<List<ActiveSchedule>> GetUpcomingBackupsAsync(int hours = 24)
    {
        var schedules = await _scheduleRepository.GetAllAsync();
        var enabledSchedules = schedules.Where(s => s.IsEnabled).ToList();
        var cutoffTime = DateTime.UtcNow.AddHours(hours);

        return enabledSchedules
            .Where(s => s.NextRunAt.HasValue && s.NextRunAt.Value <= cutoffTime)
            .OrderBy(s => s.NextRunAt)
            .Select(s => new ActiveSchedule
            {
                Id = Guid.NewGuid(), // Generate new GUID as schedule uses int ID
                Name = s.Name,
                DatabaseName = s.DatabaseName,
                CronExpression = s.CronExpression,
                NextRunAt = s.NextRunAt,
                LastRunAt = s.LastRunAt,
                LastRunStatus = s.LastRunAt.HasValue ? "Completed" : null,
                TimeUntilNextRun = s.NextRunAt.HasValue
                    ? s.NextRunAt.Value - DateTime.UtcNow
                    : (TimeSpan?)null
            })
            .ToList();
    }

    /// <summary>
    /// Validate cron expression
    /// </summary>
    private bool IsValidCronExpression(string cronExpression)
    {
        try
        {
            CronExpression.Parse(cronExpression, CronFormat.IncludeSeconds);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
