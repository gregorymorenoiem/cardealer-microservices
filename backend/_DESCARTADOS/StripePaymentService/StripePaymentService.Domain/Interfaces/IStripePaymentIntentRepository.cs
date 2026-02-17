using StripePaymentService.Domain.Entities;

namespace StripePaymentService.Domain.Interfaces;

/// <summary>
/// Repositorio para Payment Intents
/// </summary>
public interface IStripePaymentIntentRepository
{
    Task<StripePaymentIntent> CreateAsync(StripePaymentIntent paymentIntent, CancellationToken cancellationToken = default);
    Task<StripePaymentIntent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<StripePaymentIntent?> GetByStripeIdAsync(string stripePaymentIntentId, CancellationToken cancellationToken = default);
    Task<List<StripePaymentIntent>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<StripePaymentIntent>> GetByStatusAsync(string status, CancellationToken cancellationToken = default);
    Task<StripePaymentIntent> UpdateAsync(StripePaymentIntent paymentIntent, CancellationToken cancellationToken = default);
    Task<List<StripePaymentIntent>> GetPendingAsync(CancellationToken cancellationToken = default);
    Task<(List<StripePaymentIntent> Items, int TotalCount)> GetPaginatedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}
