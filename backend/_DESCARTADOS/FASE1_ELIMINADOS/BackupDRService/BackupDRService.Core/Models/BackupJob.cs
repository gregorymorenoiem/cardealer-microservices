namespace BackupDRService.Core.Models;

/// <summary>
/// Represents a backup job configuration
/// </summary>
public class BackupJob
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public BackupType Type { get; set; } = BackupType.Full;
    public BackupTarget Target { get; set; } = BackupTarget.PostgreSQL;
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string Schedule { get; set; } = string.Empty; // Cron expression
    public StorageType StorageType { get; set; } = StorageType.Local;
    public string StoragePath { get; set; } = string.Empty;
    public int RetentionDays { get; set; } = 30;
    public bool IsEnabled { get; set; } = true;
    public bool CompressBackup { get; set; } = true;
    public bool EncryptBackup { get; set; } = false;
    public string? EncryptionKey { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastRunAt { get; set; }
    public DateTime? NextRunAt { get; set; }
    public BackupJobStatus Status { get; set; } = BackupJobStatus.Idle;
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }

    public void CalculateNextRun()
    {
        if (string.IsNullOrEmpty(Schedule))
        {
            NextRunAt = null;
            return;
        }

        try
        {
            var cronExpression = Cronos.CronExpression.Parse(Schedule);
            NextRunAt = cronExpression.GetNextOccurrence(DateTime.UtcNow);
        }
        catch
        {
            NextRunAt = null;
        }
    }

    public void MarkSuccess()
    {
        LastRunAt = DateTime.UtcNow;
        Status = BackupJobStatus.Idle;
        SuccessCount++;
        CalculateNextRun();
    }

    public void MarkFailure()
    {
        LastRunAt = DateTime.UtcNow;
        Status = BackupJobStatus.Failed;
        FailureCount++;
        CalculateNextRun();
    }

    public void MarkRunning()
    {
        Status = BackupJobStatus.Running;
    }
}

public enum BackupType
{
    Full,
    Incremental,
    Differential
}

public enum BackupTarget
{
    PostgreSQL,
    SqlServer,
    MongoDB,
    Redis,
    FileSystem
}

public enum StorageType
{
    Local,
    AzureBlob,
    S3,
    Ftp
}

public enum BackupJobStatus
{
    Idle,
    Running,
    Failed,
    Disabled
}
