using Microsoft.EntityFrameworkCore;
using BillingService.Domain.Entities;
using BillingService.Domain.Interfaces;
using BillingService.Infrastructure.Persistence;

namespace BillingService.Infrastructure.Repositories;

public class StripeCustomerRepository : IStripeCustomerRepository
{
    private readonly BillingDbContext _context;

    public StripeCustomerRepository(BillingDbContext context)
    {
        _context = context;
    }

    public async Task<StripeCustomer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.StripeCustomers.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<StripeCustomer?> GetByDealerIdAsync(Guid dealerId, CancellationToken cancellationToken = default)
    {
        return await _context.StripeCustomers
            .FirstOrDefaultAsync(c => c.DealerId == dealerId && c.IsActive, cancellationToken);
    }

    public async Task<StripeCustomer?> GetByStripeCustomerIdAsync(string stripeCustomerId, CancellationToken cancellationToken = default)
    {
        return await _context.StripeCustomers
            .FirstOrDefaultAsync(c => c.StripeCustomerId == stripeCustomerId, cancellationToken);
    }

    public async Task<IEnumerable<StripeCustomer>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.StripeCustomers
            .Where(c => c.IsActive)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StripeCustomer>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.StripeCustomers
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StripeCustomer>> GetTestCustomersAsync(CancellationToken cancellationToken = default)
    {
        return await _context.StripeCustomers
            .Where(c => c.IsTestMode)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StripeCustomer>> GetLiveCustomersAsync(CancellationToken cancellationToken = default)
    {
        return await _context.StripeCustomers
            .Where(c => !c.IsTestMode && c.IsActive)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<StripeCustomer> AddAsync(StripeCustomer customer, CancellationToken cancellationToken = default)
    {
        _context.StripeCustomers.Add(customer);
        await _context.SaveChangesAsync(cancellationToken);
        return customer;
    }

    public async Task UpdateAsync(StripeCustomer customer, CancellationToken cancellationToken = default)
    {
        _context.StripeCustomers.Update(customer);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await GetByIdAsync(id, cancellationToken);
        if (customer != null)
        {
            _context.StripeCustomers.Remove(customer);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.StripeCustomers.AnyAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<bool> DealerHasStripeCustomerAsync(Guid dealerId, CancellationToken cancellationToken = default)
    {
        return await _context.StripeCustomers
            .AnyAsync(c => c.DealerId == dealerId && c.IsActive, cancellationToken);
    }

    public async Task<bool> ExistsByDealerIdAsync(Guid dealerId, CancellationToken cancellationToken = default)
    {
        return await _context.StripeCustomers
            .AnyAsync(c => c.DealerId == dealerId, cancellationToken);
    }
}
