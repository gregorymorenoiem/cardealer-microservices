using Microsoft.EntityFrameworkCore;
using BillingService.Domain.Entities;
using BillingService.Domain.Interfaces;
using BillingService.Infrastructure.Persistence;

namespace BillingService.Infrastructure.Repositories;

public class SubscriptionRepository : ISubscriptionRepository
{
    private readonly BillingDbContext _context;

    public SubscriptionRepository(BillingDbContext context)
    {
        _context = context;
    }

    public async Task<Subscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Subscriptions.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<Subscription?> GetByDealerIdAsync(Guid dealerId, CancellationToken cancellationToken = default)
    {
        return await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.DealerId == dealerId, cancellationToken);
    }

    public async Task<Subscription?> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId, CancellationToken cancellationToken = default)
    {
        return await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.StripeSubscriptionId == stripeSubscriptionId, cancellationToken);
    }

    public async Task<IEnumerable<Subscription>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Subscriptions
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Subscription>> GetByStatusAsync(SubscriptionStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Subscriptions
            .Where(s => s.Status == status)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Subscription>> GetByPlanAsync(SubscriptionPlan plan, CancellationToken cancellationToken = default)
    {
        return await _context.Subscriptions
            .Where(s => s.Plan == plan)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Subscription>> GetExpiringTrialsAsync(int days, CancellationToken cancellationToken = default)
    {
        var expirationDate = DateTime.UtcNow.AddDays(days);
        return await _context.Subscriptions
            .Where(s => s.Status == SubscriptionStatus.Trial)
            .Where(s => s.TrialEndDate <= expirationDate)
            .OrderBy(s => s.TrialEndDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Subscription>> GetDueBillingsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Subscriptions
            .Where(s => s.Status == SubscriptionStatus.Active)
            .Where(s => s.NextBillingDate <= DateTime.UtcNow.Date)
            .OrderBy(s => s.NextBillingDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Subscription> AddAsync(Subscription subscription, CancellationToken cancellationToken = default)
    {
        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync(cancellationToken);
        return subscription;
    }

    public async Task UpdateAsync(Subscription subscription, CancellationToken cancellationToken = default)
    {
        _context.Subscriptions.Update(subscription);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var subscription = await GetByIdAsync(id, cancellationToken);
        if (subscription != null)
        {
            _context.Subscriptions.Remove(subscription);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Subscriptions.AnyAsync(s => s.Id == id, cancellationToken);
    }
}
