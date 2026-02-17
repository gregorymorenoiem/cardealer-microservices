using UserService.Domain.Entities;

namespace UserService.Domain.Interfaces;

/// <summary>
/// Repository interface for dealer module subscriptions
/// </summary>
public interface IDealerModuleRepository
{
    Task<List<DealerModule>> GetActiveByDealerIdAsync(Guid dealerId);
    Task<DealerModule?> GetByIdAsync(Guid dealerId, Guid moduleId);
    Task<DealerModule> AddAsync(DealerModule dealerModule);
    Task UpdateAsync(DealerModule dealerModule);
    Task<bool> IsSubscribedAsync(Guid dealerId, Guid moduleId);
}
