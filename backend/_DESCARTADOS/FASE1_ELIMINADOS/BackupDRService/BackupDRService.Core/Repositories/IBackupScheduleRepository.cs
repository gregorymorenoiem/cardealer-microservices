using BackupDRService.Core.Entities;

namespace BackupDRService.Core.Repositories;

public interface IBackupScheduleRepository
{
    Task<BackupSchedule?> GetByIdAsync(int id);
    Task<BackupSchedule?> GetByNameAsync(string name);
    Task<IEnumerable<BackupSchedule>> GetAllAsync();
    Task<IEnumerable<BackupSchedule>> GetEnabledAsync();
    Task<IEnumerable<BackupSchedule>> GetByDatabaseNameAsync(string databaseName);
    Task<IEnumerable<BackupSchedule>> GetDueForExecutionAsync(DateTime currentTime);
    Task<BackupSchedule> CreateAsync(BackupSchedule schedule);
    Task<BackupSchedule> UpdateAsync(BackupSchedule schedule);
    Task DeleteAsync(int id);
    Task UpdateLastRunAsync(int id, DateTime lastRun, DateTime nextRun);
    Task UpdateSuccessCountAsync(int id);
    Task UpdateFailureCountAsync(int id);
}
