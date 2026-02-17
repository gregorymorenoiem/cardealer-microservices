namespace BackupDRService.Core.Models;

/// <summary>
/// Statistics about backup operations
/// </summary>
public class BackupStatistics
{
    public int TotalJobs { get; set; }
    public int EnabledJobs { get; set; }
    public int DisabledJobs { get; set; }
    public int RunningJobs { get; set; }
    public int TotalBackups { get; set; }
    public int SuccessfulBackups { get; set; }
    public int FailedBackups { get; set; }
    public int PendingBackups { get; set; }
    public int TotalRestorePoints { get; set; }
    public int AvailableRestorePoints { get; set; }
    public int ExpiredRestorePoints { get; set; }
    public int TotalRestores { get; set; }
    public int SuccessfulRestores { get; set; }
    public int FailedRestores { get; set; }
    public long TotalStorageUsedBytes { get; set; }
    public DateTime? LastBackupAt { get; set; }
    public DateTime? LastRestoreAt { get; set; }
    public DateTime? LastCleanupAt { get; set; }
    public double AverageBackupDurationSeconds { get; set; }
    public double AverageRestoreDurationSeconds { get; set; }
    public Dictionary<BackupTarget, int> BackupsByTarget { get; set; } = new();
    public Dictionary<StorageType, int> BackupsByStorageType { get; set; } = new();
    public Dictionary<string, int> BackupsByJob { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    public double SuccessRate => TotalBackups > 0
        ? (double)SuccessfulBackups / TotalBackups * 100
        : 0;

    public double RestoreSuccessRate => TotalRestores > 0
        ? (double)SuccessfulRestores / TotalRestores * 100
        : 0;

    public string GetFormattedStorageUsed()
    {
        if (TotalStorageUsedBytes < 1024) return $"{TotalStorageUsedBytes} B";
        if (TotalStorageUsedBytes < 1024 * 1024) return $"{TotalStorageUsedBytes / 1024.0:F2} KB";
        if (TotalStorageUsedBytes < 1024 * 1024 * 1024) return $"{TotalStorageUsedBytes / (1024.0 * 1024.0):F2} MB";
        return $"{TotalStorageUsedBytes / (1024.0 * 1024.0 * 1024.0):F2} GB";
    }

    public static BackupStatistics Empty()
    {
        return new BackupStatistics
        {
            TotalJobs = 0,
            EnabledJobs = 0,
            DisabledJobs = 0,
            RunningJobs = 0,
            TotalBackups = 0,
            SuccessfulBackups = 0,
            FailedBackups = 0,
            PendingBackups = 0,
            TotalRestorePoints = 0,
            AvailableRestorePoints = 0,
            ExpiredRestorePoints = 0,
            TotalRestores = 0,
            SuccessfulRestores = 0,
            FailedRestores = 0,
            TotalStorageUsedBytes = 0,
            GeneratedAt = DateTime.UtcNow
        };
    }

    public BackupStatistics WithJobCounts(int total, int enabled, int running)
    {
        TotalJobs = total;
        EnabledJobs = enabled;
        DisabledJobs = total - enabled;
        RunningJobs = running;
        return this;
    }

    public BackupStatistics WithBackupCounts(int total, int successful, int failed, int pending)
    {
        TotalBackups = total;
        SuccessfulBackups = successful;
        FailedBackups = failed;
        PendingBackups = pending;
        return this;
    }

    public BackupStatistics WithRestorePointCounts(int total, int available, int expired)
    {
        TotalRestorePoints = total;
        AvailableRestorePoints = available;
        ExpiredRestorePoints = expired;
        return this;
    }

    public BackupStatistics WithRestoreCounts(int total, int successful, int failed)
    {
        TotalRestores = total;
        SuccessfulRestores = successful;
        FailedRestores = failed;
        return this;
    }
}
