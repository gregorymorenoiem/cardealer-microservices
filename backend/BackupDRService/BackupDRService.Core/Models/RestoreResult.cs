namespace BackupDRService.Core.Models;

/// <summary>
/// Represents the result of a restore operation
/// </summary>
public class RestoreResult
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string RestorePointId { get; set; } = string.Empty;
    public string RestorePointName { get; set; } = string.Empty;
    public RestoreExecutionStatus Status { get; set; } = RestoreExecutionStatus.Pending;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public TimeSpan? Duration => CompletedAt.HasValue ? CompletedAt.Value - StartedAt : null;
    public string TargetConnectionString { get; set; } = string.Empty;
    public string TargetDatabaseName { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public string? ErrorDetails { get; set; }
    public bool VerifiedIntegrity { get; set; }
    public long BytesRestored { get; set; }
    public int TablesRestored { get; set; }
    public int RecordsRestored { get; set; }
    public string InitiatedBy { get; set; } = "system";
    public RestoreMode Mode { get; set; } = RestoreMode.InPlace;

    public static RestoreResult Success(string restorePointId, string restorePointName, long bytesRestored)
    {
        return new RestoreResult
        {
            RestorePointId = restorePointId,
            RestorePointName = restorePointName,
            Status = RestoreExecutionStatus.Completed,
            CompletedAt = DateTime.UtcNow,
            BytesRestored = bytesRestored,
            VerifiedIntegrity = true
        };
    }

    public static RestoreResult Failure(string restorePointId, string restorePointName, string errorMessage, string? errorDetails = null)
    {
        return new RestoreResult
        {
            RestorePointId = restorePointId,
            RestorePointName = restorePointName,
            Status = RestoreExecutionStatus.Failed,
            CompletedAt = DateTime.UtcNow,
            ErrorMessage = errorMessage,
            ErrorDetails = errorDetails
        };
    }

    public static RestoreResult Running(string restorePointId, string restorePointName, string initiatedBy)
    {
        return new RestoreResult
        {
            RestorePointId = restorePointId,
            RestorePointName = restorePointName,
            Status = RestoreExecutionStatus.Running,
            InitiatedBy = initiatedBy
        };
    }

    public string GetFormattedBytesRestored()
    {
        if (BytesRestored < 1024) return $"{BytesRestored} B";
        if (BytesRestored < 1024 * 1024) return $"{BytesRestored / 1024.0:F2} KB";
        if (BytesRestored < 1024 * 1024 * 1024) return $"{BytesRestored / (1024.0 * 1024.0):F2} MB";
        return $"{BytesRestored / (1024.0 * 1024.0 * 1024.0):F2} GB";
    }
}

public enum RestoreExecutionStatus
{
    Pending,
    Running,
    Completed,
    Failed,
    Cancelled,
    RolledBack
}

public enum RestoreMode
{
    InPlace,          // Restore to same location/database
    NewDatabase,      // Restore to a new database
    PointInTime,      // Restore to specific point in time
    Parallel          // Restore alongside existing data
}
