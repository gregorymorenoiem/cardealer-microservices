namespace BackupDRService.Core.Entities;

/// <summary>
/// Entity for scheduling automatic backups
/// </summary>
public class BackupSchedule
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public string BackupType { get; set; } = "Full"; // Full, Incremental, Differential
    public string CronExpression { get; set; } = string.Empty; // e.g., "0 2 * * *" (daily at 2 AM)
    public string StorageType { get; set; } = "Local";
    public string StoragePath { get; set; } = string.Empty;
    public int RetentionDays { get; set; } = 30;
    public bool IsEnabled { get; set; } = true;
    public bool CompressBackup { get; set; } = true;
    public bool EncryptBackup { get; set; } = false;
    public string? EncryptionKey { get; set; }
    public DateTime? LastRunAt { get; set; }
    public DateTime? NextRunAt { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = "system";
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public int? RetentionPolicyId { get; set; }
    public RetentionPolicy? RetentionPolicy { get; set; }
    public ICollection<BackupHistory> BackupHistories { get; set; } = new List<BackupHistory>();
}
