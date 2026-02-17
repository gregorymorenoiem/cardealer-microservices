using BackupDRService.Core.Entities;
using BackupDRService.Core.Repositories;
using Microsoft.Extensions.Logging;

namespace BackupDRService.Core.Services;

public class BackupHistoryService
{
    private readonly IBackupHistoryRepository _historyRepository;
    private readonly IAuditLogRepository _auditRepository;
    private readonly ILogger<BackupHistoryService> _logger;

    public BackupHistoryService(
        IBackupHistoryRepository historyRepository,
        IAuditLogRepository auditRepository,
        ILogger<BackupHistoryService> logger)
    {
        _historyRepository = historyRepository;
        _auditRepository = auditRepository;
        _logger = logger;
    }

    public async Task<BackupHistory> RecordBackupStartAsync(
        string jobId,
        string jobName,
        string databaseName,
        string backupType,
        string storageType,
        int? scheduleId = null)
    {
        var history = new BackupHistory
        {
            BackupId = Guid.NewGuid().ToString(),
            JobId = jobId,
            JobName = jobName,
            DatabaseName = databaseName,
            BackupType = backupType,
            StorageType = storageType,
            StartedAt = DateTime.UtcNow,
            Status = "InProgress",
            ScheduleId = scheduleId
        };

        var created = await _historyRepository.CreateAsync(history);

        await _auditRepository.CreateAsync(new AuditLog
        {
            Action = "BackupStarted",
            EntityType = "BackupHistory",
            EntityId = created.BackupId,
            EntityName = jobName,
            Details = $"Backup started for database {databaseName}"
        });

        _logger.LogInformation("Backup {BackupId} started for database {DatabaseName}",
            created.BackupId, databaseName);

        return created;
    }

    public async Task<BackupHistory> RecordBackupSuccessAsync(
        int historyId,
        string filePath,
        string fileName,
        long fileSizeBytes,
        bool isCompressed,
        bool isEncrypted,
        string? checksum = null,
        Dictionary<string, string>? metadata = null)
    {
        var history = await _historyRepository.GetByIdAsync(historyId);
        if (history == null)
        {
            throw new InvalidOperationException($"Backup history with ID {historyId} not found");
        }

        history.CompletedAt = DateTime.UtcNow;
        history.Duration = history.CompletedAt.Value - history.StartedAt;
        history.Status = "Success";
        history.FilePath = filePath;
        history.FileName = fileName;
        history.FileSizeBytes = fileSizeBytes;
        history.IsCompressed = isCompressed;
        history.IsEncrypted = isEncrypted;
        history.Checksum = checksum;
        if (metadata != null)
        {
            history.Metadata = metadata;
        }

        var updated = await _historyRepository.UpdateAsync(history);

        await _auditRepository.CreateAsync(new AuditLog
        {
            Action = "BackupCompleted",
            EntityType = "BackupHistory",
            EntityId = updated.BackupId,
            EntityName = updated.JobName,
            Details = $"Backup completed successfully. Size: {fileSizeBytes} bytes, Duration: {history.Duration?.TotalSeconds:F2}s"
        });

        _logger.LogInformation("Backup {BackupId} completed successfully in {Duration}s",
            updated.BackupId, history.Duration?.TotalSeconds);

        return updated;
    }

    public async Task<BackupHistory> RecordBackupFailureAsync(
        int historyId,
        string errorMessage)
    {
        var history = await _historyRepository.GetByIdAsync(historyId);
        if (history == null)
        {
            throw new InvalidOperationException($"Backup history with ID {historyId} not found");
        }

        history.CompletedAt = DateTime.UtcNow;
        history.Duration = history.CompletedAt.Value - history.StartedAt;
        history.Status = "Failed";
        history.ErrorMessage = errorMessage;

        var updated = await _historyRepository.UpdateAsync(history);

        await _auditRepository.CreateAsync(new AuditLog
        {
            Action = "BackupFailed",
            EntityType = "BackupHistory",
            EntityId = updated.BackupId,
            EntityName = updated.JobName,
            Status = "Failed",
            ErrorMessage = errorMessage,
            Details = $"Backup failed for database {updated.DatabaseName}"
        });

        _logger.LogError("Backup {BackupId} failed: {ErrorMessage}",
            updated.BackupId, errorMessage);

        return updated;
    }

    public async Task<IEnumerable<BackupHistory>> GetBackupHistoryAsync(
        string? databaseName = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? status = null)
    {
        if (!string.IsNullOrEmpty(databaseName))
        {
            return await _historyRepository.GetByDatabaseNameAsync(databaseName);
        }

        if (startDate.HasValue && endDate.HasValue)
        {
            return await _historyRepository.GetByDateRangeAsync(startDate.Value, endDate.Value);
        }

        if (!string.IsNullOrEmpty(status))
        {
            return await _historyRepository.GetByStatusAsync(status);
        }

        return await _historyRepository.GetAllAsync();
    }

    public async Task<BackupHistory?> GetBackupByIdAsync(string backupId)
    {
        return await _historyRepository.GetByBackupIdAsync(backupId);
    }

    public async Task<long> GetTotalStorageUsedAsync()
    {
        return await _historyRepository.GetTotalStorageUsedAsync();
    }

    public async Task<Dictionary<string, object>> GetBackupStatisticsAsync(DateTime? since = null)
    {
        var totalBackups = await _historyRepository.GetBackupCountByStatusAsync("Success", since);
        var failedBackups = await _historyRepository.GetBackupCountByStatusAsync("Failed", since);
        var inProgressBackups = await _historyRepository.GetBackupCountByStatusAsync("InProgress", since);
        var totalStorage = await _historyRepository.GetTotalStorageUsedAsync();

        return new Dictionary<string, object>
        {
            ["totalBackups"] = totalBackups,
            ["failedBackups"] = failedBackups,
            ["inProgressBackups"] = inProgressBackups,
            ["successRate"] = totalBackups > 0 ? (double)(totalBackups - failedBackups) / totalBackups * 100 : 0,
            ["totalStorageBytes"] = totalStorage,
            ["totalStorageMB"] = totalStorage / (1024.0 * 1024.0),
            ["totalStorageGB"] = totalStorage / (1024.0 * 1024.0 * 1024.0)
        };
    }
}
