using BillingService.Domain.Entities;

namespace BillingService.Domain.Interfaces;

public interface ISubscriptionRepository
{
    Task<Subscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Subscription?> GetByDealerIdAsync(Guid dealerId, CancellationToken cancellationToken = default);
    Task<Subscription?> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Subscription>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Subscription>> GetByStatusAsync(SubscriptionStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Subscription>> GetByPlanAsync(SubscriptionPlan plan, CancellationToken cancellationToken = default);
    Task<IEnumerable<Subscription>> GetExpiringTrialsAsync(int days, CancellationToken cancellationToken = default);
    Task<IEnumerable<Subscription>> GetDueBillingsAsync(CancellationToken cancellationToken = default);
    Task<Subscription> AddAsync(Subscription subscription, CancellationToken cancellationToken = default);
    Task UpdateAsync(Subscription subscription, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
