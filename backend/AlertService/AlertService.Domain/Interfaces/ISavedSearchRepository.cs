using AlertService.Domain.Entities;

namespace AlertService.Domain.Interfaces;

public interface ISavedSearchRepository
{
    Task<SavedSearch?> GetByIdAsync(Guid id);
    Task<List<SavedSearch>> GetByUserIdAsync(Guid userId);
    Task<List<SavedSearch>> GetActiveSearchesAsync();
    Task<List<SavedSearch>> GetSearchesDueForNotificationAsync();
    Task CreateAsync(SavedSearch search);
    Task UpdateAsync(SavedSearch search);
    Task DeleteAsync(Guid id);
}
