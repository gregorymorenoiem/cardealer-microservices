namespace BackupDRService.Core.Models;

/// <summary>
/// Represents the result of a backup execution
/// </summary>
public class BackupResult
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string JobId { get; set; } = string.Empty;
    public string JobName { get; set; } = string.Empty;
    public BackupExecutionStatus Status { get; set; } = BackupExecutionStatus.Pending;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public TimeSpan? Duration => CompletedAt.HasValue ? CompletedAt.Value - StartedAt : null;
    public long FileSizeBytes { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string? Checksum { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorDetails { get; set; }
    public BackupType BackupType { get; set; }
    public BackupTarget Target { get; set; }
    public StorageType StorageType { get; set; }
    public bool IsVerified { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }

    public static BackupResult Success(string jobId, string jobName, string filePath, long sizeBytes, string checksum)
    {
        return new BackupResult
        {
            JobId = jobId,
            JobName = jobName,
            Status = BackupExecutionStatus.Completed,
            CompletedAt = DateTime.UtcNow,
            FilePath = filePath,
            FileName = Path.GetFileName(filePath),
            FileSizeBytes = sizeBytes,
            Checksum = checksum
        };
    }

    public static BackupResult Failure(string jobId, string jobName, string errorMessage, string? errorDetails = null)
    {
        return new BackupResult
        {
            JobId = jobId,
            JobName = jobName,
            Status = BackupExecutionStatus.Failed,
            CompletedAt = DateTime.UtcNow,
            ErrorMessage = errorMessage,
            ErrorDetails = errorDetails
        };
    }

    public static BackupResult Running(string jobId, string jobName)
    {
        return new BackupResult
        {
            JobId = jobId,
            JobName = jobName,
            Status = BackupExecutionStatus.Running,
            StartedAt = DateTime.UtcNow
        };
    }

    public string GetFormattedSize()
    {
        if (FileSizeBytes < 1024) return $"{FileSizeBytes} B";
        if (FileSizeBytes < 1024 * 1024) return $"{FileSizeBytes / 1024.0:F2} KB";
        if (FileSizeBytes < 1024 * 1024 * 1024) return $"{FileSizeBytes / (1024.0 * 1024.0):F2} MB";
        return $"{FileSizeBytes / (1024.0 * 1024.0 * 1024.0):F2} GB";
    }
}

public enum BackupExecutionStatus
{
    Pending,
    Running,
    Completed,
    Failed,
    Cancelled,
    Expired
}
