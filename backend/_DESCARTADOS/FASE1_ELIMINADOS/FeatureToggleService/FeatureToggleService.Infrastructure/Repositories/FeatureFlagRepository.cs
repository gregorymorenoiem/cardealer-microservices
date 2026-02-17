using FeatureToggleService.Domain.Entities;
using FeatureToggleService.Domain.Enums;
using FeatureToggleService.Domain.Interfaces;
using FeatureToggleService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FeatureToggleService.Infrastructure.Repositories;

public class FeatureFlagRepository : IFeatureFlagRepository
{
    private readonly FeatureToggleDbContext _context;

    public FeatureFlagRepository(FeatureToggleDbContext context)
    {
        _context = context;
    }

    public async Task<FeatureFlag?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.FeatureFlags
            .Include(f => f.RolloutStrategy)
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public async Task<FeatureFlag?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _context.FeatureFlags
            .Include(f => f.RolloutStrategy)
            .FirstOrDefaultAsync(f => f.Key == key, cancellationToken);
    }

    public async Task<IEnumerable<FeatureFlag>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.FeatureFlags
            .Include(f => f.RolloutStrategy)
            .OrderBy(f => f.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<FeatureFlag>> GetByEnvironmentAsync(Domain.Enums.Environment environment, CancellationToken cancellationToken = default)
    {
        return await _context.FeatureFlags
            .Include(f => f.RolloutStrategy)
            .Where(f => f.Environment == environment || f.Environment == Domain.Enums.Environment.All)
            .Where(f => f.Status != FlagStatus.Archived)
            .OrderBy(f => f.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<FeatureFlag>> GetByStatusAsync(FlagStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.FeatureFlags
            .Include(f => f.RolloutStrategy)
            .Where(f => f.Status == status)
            .OrderBy(f => f.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<FeatureFlag>> GetByTagAsync(string tag, CancellationToken cancellationToken = default)
    {
        return await _context.FeatureFlags
            .Include(f => f.RolloutStrategy)
            .Where(f => f.Tags.Contains(tag))
            .Where(f => f.Status != FlagStatus.Archived)
            .OrderBy(f => f.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<FeatureFlag>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.FeatureFlags
            .Include(f => f.RolloutStrategy)
            .Where(f => f.Status == FlagStatus.Active && f.IsEnabled && !f.KillSwitchTriggered)
            .OrderBy(f => f.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<FeatureFlag>> GetExpiredAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.FeatureFlags
            .Include(f => f.RolloutStrategy)
            .Where(f => f.ExpiresAt.HasValue && f.ExpiresAt.Value < now)
            .Where(f => f.Status != FlagStatus.Archived)
            .OrderBy(f => f.ExpiresAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<FeatureFlag> AddAsync(FeatureFlag flag, CancellationToken cancellationToken = default)
    {
        await _context.FeatureFlags.AddAsync(flag, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return flag;
    }

    public async Task<FeatureFlag> UpdateAsync(FeatureFlag flag, CancellationToken cancellationToken = default)
    {
        _context.FeatureFlags.Update(flag);
        await _context.SaveChangesAsync(cancellationToken);
        return flag;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var flag = await _context.FeatureFlags.FindAsync(new object[] { id }, cancellationToken);
        if (flag != null)
        {
            _context.FeatureFlags.Remove(flag);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
        return false;
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _context.FeatureFlags.AnyAsync(f => f.Key == key, cancellationToken);
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.FeatureFlags.CountAsync(cancellationToken);
    }
}
