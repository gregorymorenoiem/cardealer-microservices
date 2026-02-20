using AdvertisingService.Domain.Entities;
using AdvertisingService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AdvertisingService.Infrastructure.Persistence.Repositories;

public class AdImpressionRepository : IAdImpressionRepository
{
    private readonly AdvertisingDbContext _context;

    public AdImpressionRepository(AdvertisingDbContext context) => _context = context;

    public async Task AddAsync(AdImpression impression, CancellationToken ct = default)
    {
        await _context.AdImpressions.AddAsync(impression, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<int> CountByCampaignAsync(Guid campaignId, DateTime since, CancellationToken ct = default)
        => await _context.AdImpressions.CountAsync(i => i.CampaignId == campaignId && i.RecordedAt >= since, ct);

    public async Task<List<(DateTime Date, int Count)>> GetDailyCountsByCampaignAsync(Guid campaignId, DateTime since, CancellationToken ct = default)
        => await _context.AdImpressions
            .Where(i => i.CampaignId == campaignId && i.RecordedAt >= since)
            .GroupBy(i => i.RecordedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .OrderBy(x => x.Date)
            .AsNoTracking()
            .Select(x => ValueTuple.Create(x.Date, x.Count))
            .ToListAsync(ct);

    public async Task<List<(DateTime Date, int Count)>> GetDailyCountsByOwnerAsync(Guid ownerId, DateTime since, CancellationToken ct = default)
        => await _context.AdImpressions
            .Include(i => i.Campaign)
            .Where(i => i.Campaign.OwnerId == ownerId && i.RecordedAt >= since)
            .GroupBy(i => i.RecordedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .OrderBy(x => x.Date)
            .AsNoTracking()
            .Select(x => ValueTuple.Create(x.Date, x.Count))
            .ToListAsync(ct);

    public async Task<int> GetTotalImpressionsSinceAsync(DateTime since, CancellationToken ct = default)
        => await _context.AdImpressions.CountAsync(i => i.RecordedAt >= since, ct);
}
