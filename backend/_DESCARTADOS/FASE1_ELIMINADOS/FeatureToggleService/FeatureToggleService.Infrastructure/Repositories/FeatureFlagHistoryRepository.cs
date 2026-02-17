using FeatureToggleService.Domain.Entities;
using FeatureToggleService.Domain.Interfaces;
using FeatureToggleService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FeatureToggleService.Infrastructure.Repositories;

public class FeatureFlagHistoryRepository : IFeatureFlagHistoryRepository
{
    private readonly FeatureToggleDbContext _context;

    public FeatureFlagHistoryRepository(FeatureToggleDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<FeatureFlagHistory>> GetByFlagIdAsync(Guid flagId, CancellationToken cancellationToken = default)
    {
        return await _context.FeatureFlagHistories
            .Where(h => h.FeatureFlagId == flagId)
            .OrderByDescending(h => h.ChangedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<FeatureFlagHistory>> GetByFlagIdAsync(Guid flagId, int limit, CancellationToken cancellationToken = default)
    {
        return await _context.FeatureFlagHistories
            .Where(h => h.FeatureFlagId == flagId)
            .OrderByDescending(h => h.ChangedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<FeatureFlagHistory>> GetByUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _context.FeatureFlagHistories
            .Where(h => h.ChangedBy == userId)
            .OrderByDescending(h => h.ChangedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<FeatureFlagHistory>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _context.FeatureFlagHistories
            .Where(h => h.ChangedAt >= startDate && h.ChangedAt <= endDate)
            .OrderByDescending(h => h.ChangedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<FeatureFlagHistory> AddAsync(FeatureFlagHistory history, CancellationToken cancellationToken = default)
    {
        await _context.FeatureFlagHistories.AddAsync(history, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return history;
    }

    public async Task<int> CountByFlagIdAsync(Guid flagId, CancellationToken cancellationToken = default)
    {
        return await _context.FeatureFlagHistories.CountAsync(h => h.FeatureFlagId == flagId, cancellationToken);
    }
}
