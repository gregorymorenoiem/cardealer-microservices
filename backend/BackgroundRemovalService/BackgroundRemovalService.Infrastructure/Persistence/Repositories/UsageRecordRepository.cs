using BackgroundRemovalService.Domain.Entities;
using BackgroundRemovalService.Domain.Enums;
using BackgroundRemovalService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BackgroundRemovalService.Infrastructure.Persistence.Repositories;

public class UsageRecordRepository : IUsageRecordRepository
{
    private readonly BackgroundRemovalDbContext _context;

    public UsageRecordRepository(BackgroundRemovalDbContext context)
    {
        _context = context;
    }

    public async Task<UsageRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.UsageRecords
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<UsageRecord>> GetByUserIdAsync(
        Guid userId, 
        int page = 1, 
        int pageSize = 50, 
        CancellationToken cancellationToken = default)
    {
        return await _context.UsageRecords
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UsageRecord>> GetByBillingPeriodAsync(
        int billingPeriod, 
        CancellationToken cancellationToken = default)
    {
        return await _context.UsageRecords
            .Where(r => r.BillingPeriod == billingPeriod)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UsageRecord>> GetByProviderAsync(
        BackgroundRemovalProvider provider, 
        DateTime from, 
        DateTime to, 
        CancellationToken cancellationToken = default)
    {
        return await _context.UsageRecords
            .Where(r => r.Provider == provider && r.CreatedAt >= from && r.CreatedAt <= to)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<UsageRecord> CreateAsync(UsageRecord record, CancellationToken cancellationToken = default)
    {
        _context.UsageRecords.Add(record);
        await _context.SaveChangesAsync(cancellationToken);
        return record;
    }

    public async Task<decimal> GetTotalCostByUserAsync(
        Guid userId, 
        int billingPeriod, 
        CancellationToken cancellationToken = default)
    {
        return await _context.UsageRecords
            .Where(r => r.UserId == userId && r.BillingPeriod == billingPeriod)
            .SumAsync(r => r.CostUsd, cancellationToken);
    }

    public async Task<int> GetTotalRequestsByUserAsync(
        Guid userId, 
        int billingPeriod, 
        CancellationToken cancellationToken = default)
    {
        return await _context.UsageRecords
            .CountAsync(r => r.UserId == userId && r.BillingPeriod == billingPeriod, cancellationToken);
    }

    public async Task<Dictionary<BackgroundRemovalProvider, int>> GetRequestCountByProviderAsync(
        int billingPeriod, 
        CancellationToken cancellationToken = default)
    {
        var results = await _context.UsageRecords
            .Where(r => r.BillingPeriod == billingPeriod)
            .GroupBy(r => r.Provider)
            .Select(g => new { Provider = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);
        
        return results.ToDictionary(r => r.Provider, r => r.Count);
    }
}
