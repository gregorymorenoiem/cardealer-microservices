namespace BillingService.Domain.Interfaces;

using BillingService.Domain.Entities;

public interface IStripeCustomerRepository
{
    Task<StripeCustomer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<StripeCustomer?> GetByDealerIdAsync(Guid dealerId, CancellationToken cancellationToken = default);
    Task<StripeCustomer?> GetByStripeCustomerIdAsync(string stripeCustomerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<StripeCustomer>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<StripeCustomer> AddAsync(StripeCustomer customer, CancellationToken cancellationToken = default);
    Task UpdateAsync(StripeCustomer customer, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByDealerIdAsync(Guid dealerId, CancellationToken cancellationToken = default);
}
