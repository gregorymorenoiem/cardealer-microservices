using BackupDRService.Core.Models;

namespace BackupDRService.Core.Interfaces;

/// <summary>
/// Interface for restore operations
/// </summary>
public interface IRestoreService
{
    // Restore Point Management
    Task<RestorePoint> CreateRestorePointAsync(BackupResult backupResult, string name, string? description = null);
    Task<RestorePoint?> GetRestorePointAsync(string restorePointId);
    Task<IEnumerable<RestorePoint>> GetAllRestorePointsAsync();
    Task<IEnumerable<RestorePoint>> GetRestorePointsByJobAsync(string jobId);
    Task<IEnumerable<RestorePoint>> GetAvailableRestorePointsAsync();
    Task<bool> DeleteRestorePointAsync(string restorePointId);

    // Restore Execution
    Task<RestoreResult> RestoreFromPointAsync(string restorePointId, RestoreOptions? options = null);
    Task<RestoreResult> RestoreFromBackupAsync(string backupResultId, RestoreOptions? options = null);
    Task<bool> CancelRestoreAsync(string restoreResultId);

    // Restore Results
    Task<RestoreResult?> GetRestoreResultAsync(string resultId);
    Task<IEnumerable<RestoreResult>> GetRestoreResultsAsync();
    Task<IEnumerable<RestoreResult>> GetRecentRestoreResultsAsync(int count = 10);

    // Verification
    Task<bool> VerifyRestorePointAsync(string restorePointId);
    Task<bool> TestRestoreAsync(string restorePointId);

    // Cleanup
    Task<int> CleanupExpiredRestorePointsAsync();
}

/// <summary>
/// Options for restore operations
/// </summary>
public class RestoreOptions
{
    public string? TargetConnectionString { get; set; }
    public string? TargetDatabaseName { get; set; }
    public RestoreMode Mode { get; set; } = RestoreMode.InPlace;
    public bool VerifyAfterRestore { get; set; } = true;
    public bool DropExistingDatabase { get; set; } = false;
    public bool CreateIfNotExists { get; set; } = true;
    public string? InitiatedBy { get; set; }
    public int TimeoutMinutes { get; set; } = 120;
}
