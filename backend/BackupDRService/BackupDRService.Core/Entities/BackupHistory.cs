namespace BackupDRService.Core.Entities;

/// <summary>
/// Entity representing backup execution history
/// </summary>
public class BackupHistory
{
    public int Id { get; set; }
    public string BackupId { get; set; } = Guid.NewGuid().ToString();
    public string JobId { get; set; } = string.Empty;
    public string JobName { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string BackupType { get; set; } = string.Empty; // Full, Incremental, Differential
    public string StorageType { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public TimeSpan? Duration { get; set; }
    public string Status { get; set; } = string.Empty; // Success, Failed, InProgress
    public string? ErrorMessage { get; set; }
    public bool IsCompressed { get; set; }
    public bool IsEncrypted { get; set; }
    public string? Checksum { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = "system";

    // Navigation properties
    public int? ScheduleId { get; set; }
    public BackupSchedule? Schedule { get; set; }
}
