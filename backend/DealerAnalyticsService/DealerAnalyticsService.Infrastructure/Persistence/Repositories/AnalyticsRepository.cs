using DealerAnalyticsService.Domain.Entities;
using DealerAnalyticsService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DealerAnalyticsService.Infrastructure.Persistence.Repositories;

public class AnalyticsRepository : IAnalyticsRepository
{
    private readonly AnalyticsDbContext _context;

    public AnalyticsRepository(AnalyticsDbContext context)
    {
        _context = context;
    }

    // Profile Views
    public async Task<ProfileView> CreateProfileViewAsync(ProfileView view, CancellationToken ct = default)
    {
        _context.ProfileViews.Add(view);
        await _context.SaveChangesAsync(ct);
        return view;
    }

    public async Task<List<ProfileView>> GetProfileViewsAsync(Guid dealerId, DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        return await _context.ProfileViews
            .Where(v => v.DealerId == dealerId && v.ViewedAt >= startDate && v.ViewedAt <= endDate)
            .OrderByDescending(v => v.ViewedAt)
            .ToListAsync(ct);
    }

    public async Task<int> GetTotalViewsAsync(Guid dealerId, DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        return await _context.ProfileViews
            .CountAsync(v => v.DealerId == dealerId && v.ViewedAt >= startDate && v.ViewedAt <= endDate, ct);
    }

    public async Task<int> GetUniqueVisitorsAsync(Guid dealerId, DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        return await _context.ProfileViews
            .Where(v => v.DealerId == dealerId && v.ViewedAt >= startDate && v.ViewedAt <= endDate)
            .Select(v => v.ViewerIpAddress)
            .Distinct()
            .CountAsync(ct);
    }

    public async Task<double> GetAverageViewDurationAsync(Guid dealerId, DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        var views = await _context.ProfileViews
            .Where(v => v.DealerId == dealerId && v.ViewedAt >= startDate && v.ViewedAt <= endDate)
            .ToListAsync(ct);

        return views.Any() ? views.Average(v => v.DurationSeconds) : 0;
    }

    public async Task<Dictionary<string, int>> GetViewsByDeviceTypeAsync(Guid dealerId, DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        return await _context.ProfileViews
            .Where(v => v.DealerId == dealerId && v.ViewedAt >= startDate && v.ViewedAt <= endDate)
            .GroupBy(v => v.DeviceType ?? "unknown")
            .Select(g => new { DeviceType = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.DeviceType, x => x.Count, ct);
    }

    public async Task<Dictionary<DateTime, int>> GetViewsTimeseriesAsync(Guid dealerId, DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        return await _context.ProfileViews
            .Where(v => v.DealerId == dealerId && v.ViewedAt >= startDate && v.ViewedAt <= endDate)
            .GroupBy(v => v.ViewedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Date, x => x.Count, ct);
    }

    // Contact Events
    public async Task<ContactEvent> CreateContactEventAsync(ContactEvent contactEvent, CancellationToken ct = default)
    {
        _context.ContactEvents.Add(contactEvent);
        await _context.SaveChangesAsync(ct);
        return contactEvent;
    }

    public async Task<List<ContactEvent>> GetContactEventsAsync(Guid dealerId, DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        return await _context.ContactEvents
            .Where(e => e.DealerId == dealerId && e.ClickedAt >= startDate && e.ClickedAt <= endDate)
            .OrderByDescending(e => e.ClickedAt)
            .ToListAsync(ct);
    }

    public async Task<int> GetTotalContactsAsync(Guid dealerId, DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        return await _context.ContactEvents
            .CountAsync(e => e.DealerId == dealerId && e.ClickedAt >= startDate && e.ClickedAt <= endDate, ct);
    }

    public async Task<Dictionary<ContactType, int>> GetContactsByTypeAsync(Guid dealerId, DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        return await _context.ContactEvents
            .Where(e => e.DealerId == dealerId && e.ClickedAt >= startDate && e.ClickedAt <= endDate)
            .GroupBy(e => e.ContactType)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Type, x => x.Count, ct);
    }

    public async Task<double> GetContactConversionRateAsync(Guid dealerId, DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        var totalViews = await GetTotalViewsAsync(dealerId, startDate, endDate, ct);
        var totalContacts = await GetTotalContactsAsync(dealerId, startDate, endDate, ct);

        return totalViews > 0 ? (totalContacts / (double)totalViews) * 100 : 0;
    }

    // Daily Summaries
    public async Task<DailyAnalyticsSummary> GetOrCreateDailySummaryAsync(Guid dealerId, DateTime date, CancellationToken ct = default)
    {
        var dateOnly = date.Date;
        var existing = await _context.DailyAnalyticsSummaries
            .FirstOrDefaultAsync(s => s.DealerId == dealerId && s.Date.Date == dateOnly, ct);

        if (existing != null) return existing;

        var summary = new DailyAnalyticsSummary
        {
            DealerId = dealerId,
            Date = dateOnly
        };

        _context.DailyAnalyticsSummaries.Add(summary);
        await _context.SaveChangesAsync(ct);
        return summary;
    }

    public async Task<DailyAnalyticsSummary> UpdateDailySummaryAsync(DailyAnalyticsSummary summary, CancellationToken ct = default)
    {
        _context.DailyAnalyticsSummaries.Update(summary);
        await _context.SaveChangesAsync(ct);
        return summary;
    }

    public async Task<List<DailyAnalyticsSummary>> GetDailySummariesAsync(Guid dealerId, DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        return await _context.DailyAnalyticsSummaries
            .Where(s => s.DealerId == dealerId && s.Date >= startDate.Date && s.Date <= endDate.Date)
            .OrderBy(s => s.Date)
            .ToListAsync(ct);
    }

    // Top Performers
    public async Task<List<(Guid DealerId, int Views)>> GetTopDealersByViewsAsync(int count, DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        return await _context.DailyAnalyticsSummaries
            .Where(s => s.Date >= startDate.Date && s.Date <= endDate.Date)
            .GroupBy(s => s.DealerId)
            .Select(g => new { DealerId = g.Key, TotalViews = g.Sum(s => s.TotalViews) })
            .OrderByDescending(x => x.TotalViews)
            .Take(count)
            .Select(x => ValueTuple.Create(x.DealerId, x.TotalViews))
            .ToListAsync(ct);
    }

    public async Task<List<(Guid DealerId, double ConversionRate)>> GetTopDealersByConversionAsync(int count, DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        return await _context.DailyAnalyticsSummaries
            .Where(s => s.Date >= startDate.Date && s.Date <= endDate.Date)
            .GroupBy(s => s.DealerId)
            .Select(g => new
            {
                DealerId = g.Key,
                TotalViews = g.Sum(s => s.TotalViews),
                TotalContacts = g.Sum(s => s.TotalContacts)
            })
            .Where(x => x.TotalViews > 0)
            .Select(x => new
            {
                x.DealerId,
                ConversionRate = (x.TotalContacts / (double)x.TotalViews) * 100
            })
            .OrderByDescending(x => x.ConversionRate)
            .Take(count)
            .Select(x => ValueTuple.Create(x.DealerId, x.ConversionRate))
            .ToListAsync(ct);
    }

    // Real-time Stats
    public async Task<int> GetLiveViewersCountAsync(Guid dealerId, int withinMinutes = 5, CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow.AddMinutes(-withinMinutes);
        return await _context.ProfileViews
            .Where(v => v.DealerId == dealerId && v.ViewedAt >= cutoff)
            .Select(v => v.ViewerIpAddress)
            .Distinct()
            .CountAsync(ct);
    }

    public async Task<ProfileView?> GetMostRecentViewAsync(Guid dealerId, CancellationToken ct = default)
    {
        return await _context.ProfileViews
            .Where(v => v.DealerId == dealerId)
            .OrderByDescending(v => v.ViewedAt)
            .FirstOrDefaultAsync(ct);
    }
}
