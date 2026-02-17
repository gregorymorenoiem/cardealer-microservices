using BackupDRService.Core.Entities;
using BackupDRService.Core.Repositories;
using Cronos;
using Microsoft.Extensions.Logging;

namespace BackupDRService.Core.Services;

public class SchedulerService
{
    private readonly IBackupScheduleRepository _scheduleRepository;
    private readonly IAuditLogRepository _auditRepository;
    private readonly ILogger<SchedulerService> _logger;

    public SchedulerService(
        IBackupScheduleRepository scheduleRepository,
        IAuditLogRepository auditRepository,
        ILogger<SchedulerService> logger)
    {
        _scheduleRepository = scheduleRepository;
        _auditRepository = auditRepository;
        _logger = logger;
    }

    public async Task<BackupSchedule> CreateScheduleAsync(BackupSchedule schedule, string userId = "system")
    {
        // Validate cron expression
        try
        {
            var cronExpression = CronExpression.Parse(schedule.CronExpression, CronFormat.IncludeSeconds);
            schedule.NextRunAt = cronExpression.GetNextOccurrence(DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Invalid cron expression: {CronExpression}", schedule.CronExpression);
            throw new ArgumentException($"Invalid cron expression: {schedule.CronExpression}", ex);
        }

        var created = await _scheduleRepository.CreateAsync(schedule);

        await _auditRepository.CreateAsync(new AuditLog
        {
            Action = "ScheduleCreated",
            EntityType = "BackupSchedule",
            EntityId = created.Id.ToString(),
            EntityName = created.Name,
            UserId = userId,
            NewValues = System.Text.Json.JsonSerializer.Serialize(created),
            Details = $"Backup schedule created for database {created.DatabaseName}"
        });

        _logger.LogInformation("Backup schedule {ScheduleName} created with cron expression {CronExpression}",
            created.Name, created.CronExpression);

        return created;
    }

    public async Task<BackupSchedule> UpdateScheduleAsync(BackupSchedule schedule, string userId = "system")
    {
        var existing = await _scheduleRepository.GetByIdAsync(schedule.Id);
        if (existing == null)
        {
            throw new InvalidOperationException($"Schedule with ID {schedule.Id} not found");
        }

        // Validate cron expression if changed
        if (schedule.CronExpression != existing.CronExpression)
        {
            try
            {
                var cronExpression = CronExpression.Parse(schedule.CronExpression, CronFormat.IncludeSeconds);
                schedule.NextRunAt = cronExpression.GetNextOccurrence(DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Invalid cron expression: {CronExpression}", schedule.CronExpression);
                throw new ArgumentException($"Invalid cron expression: {schedule.CronExpression}", ex);
            }
        }

        schedule.UpdatedBy = userId;
        var updated = await _scheduleRepository.UpdateAsync(schedule);

        await _auditRepository.CreateAsync(new AuditLog
        {
            Action = "ScheduleUpdated",
            EntityType = "BackupSchedule",
            EntityId = updated.Id.ToString(),
            EntityName = updated.Name,
            UserId = userId,
            OldValues = System.Text.Json.JsonSerializer.Serialize(existing),
            NewValues = System.Text.Json.JsonSerializer.Serialize(updated),
            Details = $"Backup schedule updated for database {updated.DatabaseName}"
        });

        _logger.LogInformation("Backup schedule {ScheduleName} updated", updated.Name);

        return updated;
    }

    public async Task DeleteScheduleAsync(int scheduleId, string userId = "system")
    {
        var schedule = await _scheduleRepository.GetByIdAsync(scheduleId);
        if (schedule == null)
        {
            throw new InvalidOperationException($"Schedule with ID {scheduleId} not found");
        }

        await _scheduleRepository.DeleteAsync(scheduleId);

        await _auditRepository.CreateAsync(new AuditLog
        {
            Action = "ScheduleDeleted",
            EntityType = "BackupSchedule",
            EntityId = scheduleId.ToString(),
            EntityName = schedule.Name,
            UserId = userId,
            OldValues = System.Text.Json.JsonSerializer.Serialize(schedule),
            Details = $"Backup schedule deleted for database {schedule.DatabaseName}"
        });

        _logger.LogInformation("Backup schedule {ScheduleName} deleted", schedule.Name);
    }

    public async Task<IEnumerable<BackupSchedule>> GetSchedulesDueForExecutionAsync()
    {
        var currentTime = DateTime.UtcNow;
        return await _scheduleRepository.GetDueForExecutionAsync(currentTime);
    }

    public async Task<IEnumerable<BackupSchedule>> GetAllSchedulesAsync()
    {
        return await _scheduleRepository.GetAllAsync();
    }

    public async Task<IEnumerable<BackupSchedule>> GetEnabledSchedulesAsync()
    {
        return await _scheduleRepository.GetEnabledAsync();
    }

    public async Task<BackupSchedule?> GetScheduleByIdAsync(int id)
    {
        return await _scheduleRepository.GetByIdAsync(id);
    }

    public async Task UpdateScheduleAfterExecutionAsync(
        int scheduleId,
        bool success,
        DateTime executedAt)
    {
        var schedule = await _scheduleRepository.GetByIdAsync(scheduleId);
        if (schedule == null)
        {
            _logger.LogWarning("Schedule {ScheduleId} not found for post-execution update", scheduleId);
            return;
        }

        // Calculate next run time
        try
        {
            var cronExpression = CronExpression.Parse(schedule.CronExpression, CronFormat.IncludeSeconds);
            var nextRun = cronExpression.GetNextOccurrence(executedAt) ?? DateTime.UtcNow.AddHours(1);

            await _scheduleRepository.UpdateLastRunAsync(scheduleId, executedAt, nextRun);

            if (success)
            {
                await _scheduleRepository.UpdateSuccessCountAsync(scheduleId);
            }
            else
            {
                await _scheduleRepository.UpdateFailureCountAsync(scheduleId);
            }

            _logger.LogInformation("Schedule {ScheduleName} updated. Next run: {NextRun}",
                schedule.Name, nextRun);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating schedule {ScheduleId} after execution", scheduleId);
        }
    }

    public async Task EnableScheduleAsync(int scheduleId, string userId = "system")
    {
        var schedule = await _scheduleRepository.GetByIdAsync(scheduleId);
        if (schedule == null)
        {
            throw new InvalidOperationException($"Schedule with ID {scheduleId} not found");
        }

        schedule.IsEnabled = true;

        // Recalculate next run time
        var cronExpression = CronExpression.Parse(schedule.CronExpression, CronFormat.IncludeSeconds);
        schedule.NextRunAt = cronExpression.GetNextOccurrence(DateTime.UtcNow);

        await _scheduleRepository.UpdateAsync(schedule);

        await _auditRepository.CreateAsync(new AuditLog
        {
            Action = "ScheduleEnabled",
            EntityType = "BackupSchedule",
            EntityId = scheduleId.ToString(),
            EntityName = schedule.Name,
            UserId = userId,
            Details = $"Backup schedule enabled. Next run: {schedule.NextRunAt}"
        });

        _logger.LogInformation("Backup schedule {ScheduleName} enabled", schedule.Name);
    }

    public async Task DisableScheduleAsync(int scheduleId, string userId = "system")
    {
        var schedule = await _scheduleRepository.GetByIdAsync(scheduleId);
        if (schedule == null)
        {
            throw new InvalidOperationException($"Schedule with ID {scheduleId} not found");
        }

        schedule.IsEnabled = false;
        await _scheduleRepository.UpdateAsync(schedule);

        await _auditRepository.CreateAsync(new AuditLog
        {
            Action = "ScheduleDisabled",
            EntityType = "BackupSchedule",
            EntityId = scheduleId.ToString(),
            EntityName = schedule.Name,
            UserId = userId,
            Details = "Backup schedule disabled"
        });

        _logger.LogInformation("Backup schedule {ScheduleName} disabled", schedule.Name);
    }
}
