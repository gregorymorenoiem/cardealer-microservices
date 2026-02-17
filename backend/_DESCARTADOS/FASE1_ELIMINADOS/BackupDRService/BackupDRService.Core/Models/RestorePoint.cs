namespace BackupDRService.Core.Models;

/// <summary>
/// Represents a point-in-time restore point
/// </summary>
public class RestorePoint
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string BackupResultId { get; set; } = string.Empty;
    public string JobId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime BackupTimestamp { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string? Checksum { get; set; }
    public BackupType BackupType { get; set; }
    public BackupTarget Target { get; set; }
    public StorageType StorageType { get; set; }
    public bool IsVerified { get; set; }
    public bool IsAvailable { get; set; } = true;
    public DateTime? ExpiresAt { get; set; }
    public RestorePointStatus Status { get; set; } = RestorePointStatus.Available;
    public Dictionary<string, string> Metadata { get; set; } = new();

    public static RestorePoint FromBackupResult(BackupResult result, string name, string? description = null)
    {
        return new RestorePoint
        {
            BackupResultId = result.Id,
            JobId = result.JobId,
            Name = name,
            Description = description ?? $"Restore point from backup {result.FileName}",
            BackupTimestamp = result.StartedAt,
            FilePath = result.FilePath,
            FileSizeBytes = result.FileSizeBytes,
            Checksum = result.Checksum,
            BackupType = result.BackupType,
            Target = result.Target,
            StorageType = result.StorageType,
            IsVerified = result.IsVerified,
            ExpiresAt = result.ExpiresAt
        };
    }

    public bool IsExpired => ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;

    public string GetFormattedSize()
    {
        if (FileSizeBytes < 1024) return $"{FileSizeBytes} B";
        if (FileSizeBytes < 1024 * 1024) return $"{FileSizeBytes / 1024.0:F2} KB";
        if (FileSizeBytes < 1024 * 1024 * 1024) return $"{FileSizeBytes / (1024.0 * 1024.0):F2} MB";
        return $"{FileSizeBytes / (1024.0 * 1024.0 * 1024.0):F2} GB";
    }
}

public enum RestorePointStatus
{
    Available,
    Restoring,
    Restored,
    Expired,
    Deleted,
    Corrupted
}
