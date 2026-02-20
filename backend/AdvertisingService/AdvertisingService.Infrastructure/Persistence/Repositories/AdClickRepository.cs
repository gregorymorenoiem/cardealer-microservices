using AdvertisingService.Domain.Entities;
using AdvertisingService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AdvertisingService.Infrastructure.Persistence.Repositories;

public class AdClickRepository : IAdClickRepository
{
    private readonly AdvertisingDbContext _context;

    public AdClickRepository(AdvertisingDbContext context) => _context = context;

    public async Task AddAsync(AdClick click, CancellationToken ct = default)
    {
        await _context.AdClicks.AddAsync(click, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<int> CountByCampaignAsync(Guid campaignId, DateTime since, CancellationToken ct = default)
        => await _context.AdClicks.CountAsync(c => c.CampaignId == campaignId && c.RecordedAt >= since, ct);

    public async Task<List<(DateTime Date, int Count)>> GetDailyCountsByCampaignAsync(Guid campaignId, DateTime since, CancellationToken ct = default)
        => await _context.AdClicks
            .Where(c => c.CampaignId == campaignId && c.RecordedAt >= since)
            .GroupBy(c => c.RecordedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .OrderBy(x => x.Date)
            .AsNoTracking()
            .Select(x => ValueTuple.Create(x.Date, x.Count))
            .ToListAsync(ct);

    public async Task<List<(DateTime Date, int Count)>> GetDailyCountsByOwnerAsync(Guid ownerId, DateTime since, CancellationToken ct = default)
        => await _context.AdClicks
            .Include(c => c.Campaign)
            .Where(c => c.Campaign.OwnerId == ownerId && c.RecordedAt >= since)
            .GroupBy(c => c.RecordedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .OrderBy(x => x.Date)
            .AsNoTracking()
            .Select(x => ValueTuple.Create(x.Date, x.Count))
            .ToListAsync(ct);

    public async Task<int> GetTotalClicksSinceAsync(DateTime since, CancellationToken ct = default)
        => await _context.AdClicks.CountAsync(c => c.RecordedAt >= since, ct);
}
