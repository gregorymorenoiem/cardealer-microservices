using StripePaymentService.Domain.Entities;

namespace StripePaymentService.Domain.Interfaces;

/// <summary>
/// Repositorio para Subscriptions de Stripe
/// </summary>
public interface IStripeSubscriptionRepository
{
    Task<StripeSubscription> CreateAsync(StripeSubscription subscription, CancellationToken cancellationToken = default);
    Task<StripeSubscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<StripeSubscription?> GetByStripeIdAsync(string stripeSubscriptionId, CancellationToken cancellationToken = default);
    Task<List<StripeSubscription>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<List<StripeSubscription>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<List<StripeSubscription>> GetByStatusAsync(string status, CancellationToken cancellationToken = default);
    Task<StripeSubscription> UpdateAsync(StripeSubscription subscription, CancellationToken cancellationToken = default);
    Task<StripeSubscription> CancelAsync(Guid id, string? reason = null, CancellationToken cancellationToken = default);
    Task<decimal> GetMRRAsync(CancellationToken cancellationToken = default);
    Task<int> GetActiveCountAsync(CancellationToken cancellationToken = default);
}
