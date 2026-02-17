namespace BackupDRService.Core.Entities;

/// <summary>
/// Entity for auditing all backup/restore operations
/// </summary>
public class AuditLog
{
    public int Id { get; set; }
    public string Action { get; set; } = string.Empty; // BackupCreated, BackupDeleted, RestoreExecuted, ScheduleCreated, etc.
    public string EntityType { get; set; } = string.Empty; // BackupHistory, BackupSchedule, RetentionPolicy
    public string? EntityId { get; set; }
    public string? EntityName { get; set; }
    public string UserId { get; set; } = "system";
    public string UserName { get; set; } = "System";
    public string? UserEmail { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? OldValues { get; set; } // JSON serialized
    public string? NewValues { get; set; } // JSON serialized
    public string? Details { get; set; }
    public string Status { get; set; } = "Success"; // Success, Failed, Warning
    public string? ErrorMessage { get; set; }
    public TimeSpan? Duration { get; set; }
    public Dictionary<string, string> AdditionalData { get; set; } = new();
}
