using BackupDRService.Core.Entities;

namespace BackupDRService.Core.Repositories;

public interface IBackupHistoryRepository
{
    Task<BackupHistory?> GetByIdAsync(int id);
    Task<BackupHistory?> GetByBackupIdAsync(string backupId);
    Task<IEnumerable<BackupHistory>> GetAllAsync();
    Task<IEnumerable<BackupHistory>> GetByJobIdAsync(string jobId);
    Task<IEnumerable<BackupHistory>> GetByDatabaseNameAsync(string databaseName);
    Task<IEnumerable<BackupHistory>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<BackupHistory>> GetByStatusAsync(string status);
    Task<IEnumerable<BackupHistory>> GetRecentAsync(int count);
    Task<BackupHistory> CreateAsync(BackupHistory history);
    Task<BackupHistory> UpdateAsync(BackupHistory history);
    Task DeleteAsync(int id);
    Task<long> GetTotalStorageUsedAsync();
    Task<long> GetStorageUsedByDatabaseAsync(string databaseName);
    Task<int> GetBackupCountByStatusAsync(string status, DateTime? since = null);
}
