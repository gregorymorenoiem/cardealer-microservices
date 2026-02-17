using Microsoft.EntityFrameworkCore;
using UserBehaviorService.Domain.Entities;
using UserBehaviorService.Domain.Interfaces;

namespace UserBehaviorService.Infrastructure.Persistence.Repositories;

public class UserBehaviorRepository : IUserBehaviorRepository
{
    private readonly UserBehaviorDbContext _context;

    public UserBehaviorRepository(UserBehaviorDbContext context)
    {
        _context = context;
    }

    public async Task<UserBehaviorProfile?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.UserBehaviorProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);
    }

    public async Task<List<UserBehaviorProfile>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
    {
        return await _context.UserBehaviorProfiles
            .OrderByDescending(p => p.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<UserBehaviorProfile> CreateOrUpdateAsync(UserBehaviorProfile profile, CancellationToken ct = default)
    {
        var existing = await _context.UserBehaviorProfiles
            .FirstOrDefaultAsync(p => p.UserId == profile.UserId, ct);

        if (existing != null)
        {
            _context.Entry(existing).CurrentValues.SetValues(profile);
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            profile.CreatedAt = DateTime.UtcNow;
            profile.UpdatedAt = DateTime.UtcNow;
            _context.UserBehaviorProfiles.Add(profile);
        }

        await _context.SaveChangesAsync(ct);
        return existing ?? profile;
    }

    public async Task DeleteAsync(Guid userId, CancellationToken ct = default)
    {
        var profile = await GetByUserIdAsync(userId, ct);
        if (profile != null)
        {
            _context.UserBehaviorProfiles.Remove(profile);
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task<UserAction> AddActionAsync(UserAction action, CancellationToken ct = default)
    {
        _context.UserActions.Add(action);
        await _context.SaveChangesAsync(ct);
        return action;
    }

    public async Task<List<UserAction>> GetUserActionsAsync(Guid userId, int limit = 50, CancellationToken ct = default)
    {
        return await _context.UserActions
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.Timestamp)
            .Take(limit)
            .ToListAsync(ct);
    }

    public async Task<Dictionary<string, int>> GetActionCountsByTypeAsync(Guid userId, DateTime since, CancellationToken ct = default)
    {
        return await _context.UserActions
            .Where(a => a.UserId == userId && a.Timestamp >= since)
            .GroupBy(a => a.ActionType)
            .Select(g => new { ActionType = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ActionType, x => x.Count, ct);
    }

    public async Task<List<UserBehaviorProfile>> GetUsersBySegmentAsync(string segmentName, int page, int pageSize, CancellationToken ct = default)
    {
        return await _context.UserBehaviorProfiles
            .Where(p => p.UserSegment == segmentName)
            .OrderByDescending(p => p.PurchaseIntentScore)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<Dictionary<string, int>> GetSegmentDistributionAsync(CancellationToken ct = default)
    {
        return await _context.UserBehaviorProfiles
            .GroupBy(p => p.UserSegment)
            .Select(g => new { Segment = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Segment, x => x.Count, ct);
    }

    public async Task<Dictionary<string, int>> GetTopPreferredMakesAsync(int top = 10, CancellationToken ct = default)
    {
        var profiles = await _context.UserBehaviorProfiles
            .Where(p => p.PreferredMakes.Count > 0)
            .ToListAsync(ct);

        return profiles
            .SelectMany(p => p.PreferredMakes)
            .GroupBy(m => m)
            .OrderByDescending(g => g.Count())
            .Take(top)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task<Dictionary<string, int>> GetTopPreferredModelsAsync(int top = 10, CancellationToken ct = default)
    {
        var profiles = await _context.UserBehaviorProfiles
            .Where(p => p.PreferredModels.Count > 0)
            .ToListAsync(ct);

        return profiles
            .SelectMany(p => p.PreferredModels)
            .GroupBy(m => m)
            .OrderByDescending(g => g.Count())
            .Take(top)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task<(decimal Min, decimal Max)> GetAveragePriceRangeAsync(CancellationToken ct = default)
    {
        var profiles = await _context.UserBehaviorProfiles
            .Where(p => p.PreferredPriceMin.HasValue && p.PreferredPriceMax.HasValue)
            .ToListAsync(ct);

        if (!profiles.Any()) return (0, 0);

        var avgMin = profiles.Average(p => p.PreferredPriceMin ?? 0);
        var avgMax = profiles.Average(p => p.PreferredPriceMax ?? 0);

        return (avgMin, avgMax);
    }

    public async Task<int> GetActiveUsersCountAsync(int days = 7, CancellationToken ct = default)
    {
        var since = DateTime.UtcNow.AddDays(-days);
        return await _context.UserBehaviorProfiles
            .Where(p => p.LastActivityAt.HasValue && p.LastActivityAt >= since)
            .CountAsync(ct);
    }
}
