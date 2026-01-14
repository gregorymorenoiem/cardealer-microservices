using Microsoft.EntityFrameworkCore;
using StripePaymentService.Domain.Entities;
using StripePaymentService.Domain.Interfaces;
using StripePaymentService.Infrastructure.Persistence;

namespace StripePaymentService.Infrastructure.Repositories;

/// <summary>
/// Repositorio para Stripe Subscription
/// </summary>
public class StripeSubscriptionRepository : IStripeSubscriptionRepository
{
    private readonly StripeDbContext _context;

    public StripeSubscriptionRepository(StripeDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<StripeSubscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Subscriptions
            .Include(x => x.Customer)
            .Include(x => x.Invoices)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<StripeSubscription?> GetByStripeIdAsync(string stripeSubscriptionId, CancellationToken cancellationToken = default)
    {
        return await _context.Subscriptions
            .Include(x => x.Customer)
            .Include(x => x.Invoices)
            .FirstOrDefaultAsync(x => x.StripeSubscriptionId == stripeSubscriptionId, cancellationToken);
    }

    public async Task<List<StripeSubscription>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _context.Subscriptions
            .Where(x => x.CustomerId == customerId)
            .Include(x => x.Invoices)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<StripeSubscription>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Subscriptions
            .Where(x => x.Status == "active")
            .Include(x => x.Customer)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<StripeSubscription>> GetByStatusAsync(string status, CancellationToken cancellationToken = default)
    {
        return await _context.Subscriptions
            .Where(x => x.Status == status)
            .Include(x => x.Customer)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> CalculateMrrAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Subscriptions
            .Where(x => x.Status == "active")
            .SumAsync(x => x.Amount, cancellationToken);
    }

    public async Task<bool> CancelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var subscription = await GetByIdAsync(id, cancellationToken);
        if (subscription == null)
            return false;

        subscription.Status = "canceled";
        subscription.CanceledAt = DateTime.UtcNow;
        subscription.UpdatedAt = DateTime.UtcNow;

        await UpdateAsync(subscription, cancellationToken);
        return true;
    }

    public async Task CreateAsync(StripeSubscription entity, CancellationToken cancellationToken = default)
    {
        await _context.Subscriptions.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(StripeSubscription entity, CancellationToken cancellationToken = default)
    {
        _context.Subscriptions.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null)
        {
            _context.Subscriptions.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
