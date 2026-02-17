using BackupDRService.Core.Entities;

namespace BackupDRService.Core.Repositories;

public interface IRetentionPolicyRepository
{
    Task<RetentionPolicy?> GetByIdAsync(int id);
    Task<RetentionPolicy?> GetByNameAsync(string name);
    Task<IEnumerable<RetentionPolicy>> GetAllAsync();
    Task<IEnumerable<RetentionPolicy>> GetActiveAsync();
    Task<RetentionPolicy> CreateAsync(RetentionPolicy policy);
    Task<RetentionPolicy> UpdateAsync(RetentionPolicy policy);
    Task DeleteAsync(int id);
}
