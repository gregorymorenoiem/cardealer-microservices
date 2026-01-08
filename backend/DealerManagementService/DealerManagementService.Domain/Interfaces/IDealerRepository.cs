using DealerManagementService.Domain.Entities;

namespace DealerManagementService.Domain.Interfaces;

public interface IDealerRepository
{
    // Basic CRUD
    Task<Dealer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Dealer?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Dealer?> GetByRNCAsync(string rnc, CancellationToken cancellationToken = default);
    Task<IEnumerable<Dealer>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<(IEnumerable<Dealer> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        DealerStatus? status = null,
        VerificationStatus? verificationStatus = null,
        string? searchTerm = null,
        CancellationToken cancellationToken = default);
    Task<Dealer> AddAsync(Dealer dealer, CancellationToken cancellationToken = default);
    Task UpdateAsync(Dealer dealer, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    
    // Business Logic
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> RNCExistsAsync(string rnc, CancellationToken cancellationToken = default);
    Task<IEnumerable<Dealer>> GetByStatusAsync(DealerStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Dealer>> GetPendingVerificationAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Dealer>> GetByPlanAsync(DealerPlan plan, CancellationToken cancellationToken = default);
    Task<int> GetActiveDealersCountAsync(CancellationToken cancellationToken = default);
    
    // Subscription Management
    Task UpdateSubscriptionAsync(Guid dealerId, Guid subscriptionId, DealerPlan plan, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task DeactivateSubscriptionAsync(Guid dealerId, CancellationToken cancellationToken = default);
    Task IncrementActiveListingsAsync(Guid dealerId, CancellationToken cancellationToken = default);
    Task DecrementActiveListingsAsync(Guid dealerId, CancellationToken cancellationToken = default);
}
