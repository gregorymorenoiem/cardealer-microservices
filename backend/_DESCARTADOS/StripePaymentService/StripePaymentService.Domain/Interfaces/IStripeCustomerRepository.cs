using StripePaymentService.Domain.Entities;

namespace StripePaymentService.Domain.Interfaces;

/// <summary>
/// Repositorio para Customers de Stripe
/// </summary>
public interface IStripeCustomerRepository
{
    Task<StripeCustomer> CreateAsync(StripeCustomer customer, CancellationToken cancellationToken = default);
    Task<StripeCustomer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<StripeCustomer?> GetByStripeIdAsync(string stripeCustomerId, CancellationToken cancellationToken = default);
    Task<StripeCustomer?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<StripeCustomer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<StripeCustomer> UpdateAsync(StripeCustomer customer, CancellationToken cancellationToken = default);
    Task<List<StripeCustomer>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<(List<StripeCustomer> Items, int TotalCount)> GetPaginatedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}
