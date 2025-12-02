using BackupDRService.Core.Models;

namespace BackupDRService.Core.Interfaces;

/// <summary>
/// Interface for backup operations
/// </summary>
public interface IBackupService
{
    // Backup Job Management
    Task<BackupJob> CreateJobAsync(BackupJob job);
    Task<BackupJob?> GetJobAsync(string jobId);
    Task<BackupJob?> GetJobByNameAsync(string name);
    Task<IEnumerable<BackupJob>> GetAllJobsAsync();
    Task<IEnumerable<BackupJob>> GetEnabledJobsAsync();
    Task<BackupJob> UpdateJobAsync(BackupJob job);
    Task<bool> DeleteJobAsync(string jobId);
    Task<bool> EnableJobAsync(string jobId);
    Task<bool> DisableJobAsync(string jobId);

    // Backup Execution
    Task<BackupResult> ExecuteBackupAsync(string jobId);
    Task<BackupResult> ExecuteBackupAsync(BackupJob job);
    Task<bool> CancelBackupAsync(string backupResultId);

    // Backup Results
    Task<BackupResult?> GetBackupResultAsync(string resultId);
    Task<IEnumerable<BackupResult>> GetBackupResultsAsync(string jobId);
    Task<IEnumerable<BackupResult>> GetRecentBackupResultsAsync(int count = 10);
    Task<IEnumerable<BackupResult>> GetBackupResultsByDateRangeAsync(DateTime from, DateTime to);

    // Backup Verification
    Task<bool> VerifyBackupAsync(string backupResultId);
    Task<bool> VerifyBackupIntegrityAsync(string filePath, string? expectedChecksum);

    // Cleanup
    Task<int> CleanupExpiredBackupsAsync();
    Task<int> CleanupBackupsOlderThanAsync(DateTime cutoffDate);

    // Statistics
    Task<BackupStatistics> GetStatisticsAsync();
}
