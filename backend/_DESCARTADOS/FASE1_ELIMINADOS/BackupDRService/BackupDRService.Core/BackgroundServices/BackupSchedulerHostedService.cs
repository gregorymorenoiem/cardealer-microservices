using BackupDRService.Core.Models;
using BackupDRService.Core.Services;
using Cronos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BackupDRService.Core.BackgroundServices;

/// <summary>
/// Background service for automatic execution of scheduled backups
/// </summary>
public class BackupSchedulerHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BackupSchedulerHostedService> _logger;
    private readonly TimeSpan _checkInterval;
    private readonly int _maxConcurrentBackups;
    private readonly SemaphoreSlim _executionSemaphore;
    private int _executedBackupsCount;
    private int _failedBackupsCount;
    private DateTime _lastCheckTime;

    public BackupSchedulerHostedService(
        IServiceProvider serviceProvider,
        ILogger<BackupSchedulerHostedService> logger,
        IOptions<BackupOptions> options)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

        var backupOptions = options.Value;
        _checkInterval = TimeSpan.FromSeconds(30); // Check every 30 seconds for better precision
        _maxConcurrentBackups = backupOptions.MaxConcurrentJobs;
        _executionSemaphore = new SemaphoreSlim(_maxConcurrentBackups, _maxConcurrentBackups);
        _lastCheckTime = DateTime.UtcNow;

        _logger.LogInformation(
            "Backup Scheduler initialized with check interval: {Interval}s, Max concurrent: {MaxConcurrent}",
            _checkInterval.TotalSeconds,
            _maxConcurrentBackups);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("✅ Backup Scheduler Service started - Automatic scheduling enabled");

        // Wait a bit before starting to allow services to initialize
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var checkStart = DateTime.UtcNow;
                await CheckAndExecuteScheduledBackupsAsync(stoppingToken);
                _lastCheckTime = DateTime.UtcNow;

                var checkDuration = DateTime.UtcNow - checkStart;
                if (checkDuration > TimeSpan.FromSeconds(5))
                {
                    _logger.LogWarning(
                        "Backup check took longer than expected: {Duration}ms",
                        checkDuration.TotalMilliseconds);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in backup scheduler service");
                _failedBackupsCount++;
            }

            // Calculate next check time with adaptive delay
            var nextCheckDelay = CalculateNextCheckDelay();
            await Task.Delay(nextCheckDelay, stoppingToken);
        }

        _logger.LogInformation(
            "Backup Scheduler Service stopped. Total backups executed: {Executed}, Failed: {Failed}",
            _executedBackupsCount,
            _failedBackupsCount);
    }

    private async Task CheckAndExecuteScheduledBackupsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var schedulerService = scope.ServiceProvider.GetRequiredService<SchedulerService>();
        var backupService = scope.ServiceProvider.GetRequiredService<BackupService>();
        var historyService = scope.ServiceProvider.GetRequiredService<BackupHistoryService>();

        var schedulesDue = await schedulerService.GetSchedulesDueForExecutionAsync();

        if (!schedulesDue.Any())
        {
            return; // No schedules due, skip logging
        }

        _logger.LogInformation("📋 Found {Count} scheduled backup(s) due for execution", schedulesDue.Count());

        var tasks = new List<Task>();

        foreach (var schedule in schedulesDue)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            // Limit concurrent executions
            await _executionSemaphore.WaitAsync(cancellationToken);

            // Execute backup in parallel (fire and forget pattern)
            var task = Task.Run(async () =>
            {
                try
                {
                    await ExecuteScheduledBackupAsync(
                        schedule,
                        schedulerService,
                        backupService,
                        historyService,
                        cancellationToken);
                }
                finally
                {
                    _executionSemaphore.Release();
                }
            }, cancellationToken);

            tasks.Add(task);
        }

        // Wait for all tasks to complete (with timeout)
        if (tasks.Any())
        {
            try
            {
                await Task.WhenAll(tasks).WaitAsync(TimeSpan.FromMinutes(30), cancellationToken);
            }
            catch (TimeoutException)
            {
                _logger.LogWarning("⚠️ Some backup tasks timed out after 30 minutes");
            }
        }
    }

    private async Task ExecuteScheduledBackupAsync(
        Entities.BackupSchedule schedule,
        SchedulerService schedulerService,
        BackupService backupService,
        BackupHistoryService historyService,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "🚀 Executing scheduled backup: {ScheduleName} for database {DatabaseName}",
            schedule.Name,
            schedule.DatabaseName);

        var executedAt = DateTime.UtcNow;
        bool success = false;
        Entities.BackupHistory? history = null;

        try
        {
            // Create backup history record
            history = await historyService.RecordBackupStartAsync(
                jobId: $"scheduled_{schedule.Id}_{executedAt:yyyyMMdd_HHmmss}",
                jobName: schedule.Name,
                databaseName: schedule.DatabaseName,
                backupType: schedule.BackupType,
                storageType: schedule.StorageType,
                scheduleId: schedule.Id);

            // Create backup job from schedule
            var backupJob = new BackupJob
            {
                Id = history.BackupId,
                Name = schedule.Name,
                DatabaseName = schedule.DatabaseName,
                ConnectionString = schedule.ConnectionString,
                Type = ParseBackupType(schedule.BackupType),
                StorageType = ParseStorageType(schedule.StorageType),
                StoragePath = schedule.StoragePath,
                RetentionDays = schedule.RetentionDays,
                CompressBackup = schedule.CompressBackup,
                EncryptBackup = schedule.EncryptBackup,
                EncryptionKey = schedule.EncryptionKey,
                Schedule = schedule.CronExpression
            };

            // Execute backup
            var result = await backupService.ExecuteBackupAsync(backupJob);

            if (result.Status == BackupExecutionStatus.Completed)
            {
                // Update history with success
                await historyService.RecordBackupSuccessAsync(
                    historyId: history.Id,
                    filePath: result.FilePath ?? string.Empty,
                    fileName: result.FileName,
                    fileSizeBytes: result.FileSizeBytes,
                    isCompressed: schedule.CompressBackup,
                    isEncrypted: schedule.EncryptBackup,
                    checksum: result.Checksum);

                success = true;
                _executedBackupsCount++;

                _logger.LogInformation(
                    "✅ Scheduled backup completed: {ScheduleName}, Size: {SizeMB} MB, Duration: {Duration}s",
                    schedule.Name,
                    result.FileSizeBytes / 1024.0 / 1024.0,
                    result.Duration?.TotalSeconds ?? 0);
            }
            else
            {
                // Update history with failure
                await historyService.RecordBackupFailureAsync(
                    historyId: history.Id,
                    errorMessage: result.ErrorMessage ?? "Unknown error");

                _failedBackupsCount++;

                _logger.LogError(
                    "❌ Scheduled backup failed: {ScheduleName}, Error: {ErrorMessage}",
                    schedule.Name,
                    result.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            _failedBackupsCount++;

            _logger.LogError(ex,
                "❌ Exception executing scheduled backup: {ScheduleName}",
                schedule.Name);

            if (history != null)
            {
                try
                {
                    await historyService.RecordBackupFailureAsync(
                        historyId: history.Id,
                        errorMessage: $"Exception: {ex.Message}");
                }
                catch
                {
                    // Ignore errors updating history
                }
            }
        }

        // Update schedule after execution
        try
        {
            await schedulerService.UpdateScheduleAfterExecutionAsync(
                schedule.Id,
                success,
                executedAt);

            // Calculate and log next run time
            var nextRun = CalculateNextRunTime(schedule.CronExpression);
            if (nextRun.HasValue)
            {
                var timeUntilNext = nextRun.Value - DateTime.UtcNow;
                _logger.LogInformation(
                    "📅 Next run for {ScheduleName}: {NextRun} (in {Hours}h {Minutes}m)",
                    schedule.Name,
                    nextRun.Value.ToString("yyyy-MM-dd HH:mm:ss"),
                    (int)timeUntilNext.TotalHours,
                    (int)timeUntilNext.TotalMinutes % 60);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating schedule {ScheduleId} after execution", schedule.Id);
        }
    }

    private DateTime? CalculateNextRunTime(string cronExpression)
    {
        try
        {
            var cron = CronExpression.Parse(cronExpression, CronFormat.IncludeSeconds);
            return cron.GetNextOccurrence(DateTime.UtcNow);
        }
        catch
        {
            return null;
        }
    }

    private TimeSpan CalculateNextCheckDelay()
    {
        // Adaptive delay based on activity
        var minutesSinceLastCheck = (DateTime.UtcNow - _lastCheckTime).TotalMinutes;

        if (minutesSinceLastCheck < 1)
        {
            return _checkInterval; // Default 30 seconds
        }

        // If nothing happened for a while, check less frequently
        return minutesSinceLastCheck > 30
            ? TimeSpan.FromMinutes(1)
            : _checkInterval;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Backup Scheduler Service is stopping");
        await base.StopAsync(cancellationToken);
    }

    private static BackupType ParseBackupType(string backupType)
    {
        return backupType.ToLower() switch
        {
            "full" => BackupType.Full,
            "incremental" => BackupType.Incremental,
            "differential" => BackupType.Differential,
            _ => BackupType.Full
        };
    }

    private static StorageType ParseStorageType(string storageType)
    {
        return storageType.ToLower() switch
        {
            "local" => StorageType.Local,
            "azureblob" or "azure" => StorageType.AzureBlob,
            "s3" => StorageType.S3,
            "ftp" => StorageType.Ftp,
            _ => StorageType.Local
        };
    }
}
