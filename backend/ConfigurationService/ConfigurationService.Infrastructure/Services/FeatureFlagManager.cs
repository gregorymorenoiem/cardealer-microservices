using ConfigurationService.Application.Interfaces;
using ConfigurationService.Domain.Entities;
using ConfigurationService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ConfigurationService.Infrastructure.Services;

public class FeatureFlagManager : IFeatureFlagManager
{
    private readonly ConfigurationDbContext _context;

    public FeatureFlagManager(ConfigurationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> IsFeatureEnabledAsync(string key, string? environment = null, string? tenantId = null, string? userId = null)
    {
        var flag = await GetFeatureFlagAsync(key, environment);

        if (flag == null || !flag.IsEnabled)
            return false;

        // Check tenant if specified
        if (tenantId != null && flag.TenantId != null && flag.TenantId != tenantId)
            return false;

        // Check date range
        if (flag.StartsAt.HasValue && flag.StartsAt.Value > DateTime.UtcNow)
            return false;

        if (flag.EndsAt.HasValue && flag.EndsAt.Value < DateTime.UtcNow)
            return false;

        // Handle rollout percentage
        if (flag.RolloutPercentage < 100 && userId != null)
        {
            // Use consistent hashing for user-based rollout
            var userHash = Math.Abs(userId.GetHashCode());
            var bucket = userHash % 100;
            return bucket < flag.RolloutPercentage;
        }

        return true;
    }

    public async Task<FeatureFlag?> GetFeatureFlagAsync(string key, string? environment = null)
    {
        var query = _context.FeatureFlags.Where(f => f.Key == key);

        if (environment != null)
            query = query.Where(f => f.Environment == environment || f.Environment == null);

        return await query.FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<FeatureFlag>> GetAllFeatureFlagsAsync(string? environment = null)
    {
        var query = _context.FeatureFlags.AsQueryable();

        if (environment != null)
            query = query.Where(f => f.Environment == environment || f.Environment == null);

        return await query.ToListAsync();
    }

    public async Task<FeatureFlag> CreateFeatureFlagAsync(FeatureFlag flag)
    {
        flag.Id = Guid.NewGuid();
        flag.CreatedAt = DateTime.UtcNow;

        await _context.FeatureFlags.AddAsync(flag);
        await _context.SaveChangesAsync();

        return flag;
    }

    public async Task<FeatureFlag> UpdateFeatureFlagAsync(Guid id, bool isEnabled, int? rolloutPercentage = null)
    {
        var flag = await _context.FeatureFlags.FindAsync(id);
        if (flag == null)
            throw new InvalidOperationException($"Feature flag with ID {id} not found");

        flag.IsEnabled = isEnabled;
        if (rolloutPercentage.HasValue)
            flag.RolloutPercentage = Math.Clamp(rolloutPercentage.Value, 0, 100);

        flag.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return flag;
    }

    public async Task DeleteFeatureFlagAsync(Guid id)
    {
        var flag = await _context.FeatureFlags.FindAsync(id);
        if (flag != null)
        {
            _context.FeatureFlags.Remove(flag);
            await _context.SaveChangesAsync();
        }
    }
}
