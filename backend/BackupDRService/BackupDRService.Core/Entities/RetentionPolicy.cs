namespace BackupDRService.Core.Entities;

/// <summary>
/// Entity for backup retention policies
/// </summary>
public class RetentionPolicy
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Daily backups retention
    public int DailyRetentionDays { get; set; } = 7; // Keep daily backups for 7 days
    public int WeeklyRetentionWeeks { get; set; } = 4; // Keep weekly backups for 4 weeks
    public int MonthlyRetentionMonths { get; set; } = 12; // Keep monthly backups for 12 months
    public int YearlyRetentionYears { get; set; } = 5; // Keep yearly backups for 5 years

    // Space management
    public long? MaxStorageSizeBytes { get; set; }
    public int? MaxBackupCount { get; set; }
    public bool DeleteOldestWhenLimitReached { get; set; } = true;

    // Archive settings
    public bool ArchiveOldBackups { get; set; } = false;
    public string? ArchiveStorageType { get; set; }
    public string? ArchivePath { get; set; }
    public int ArchiveAfterDays { get; set; } = 90;

    // Validation
    public bool RequireSuccessfulBackupBeforeDelete { get; set; } = true;
    public bool NotifyBeforeDelete { get; set; } = true;

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = "system";
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public ICollection<BackupSchedule> BackupSchedules { get; set; } = new List<BackupSchedule>();
}
