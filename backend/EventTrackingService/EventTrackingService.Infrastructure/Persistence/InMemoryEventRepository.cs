using EventTrackingService.Domain.Entities;
using EventTrackingService.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace EventTrackingService.Infrastructure.Persistence;

/// <summary>
/// Implementación InMemory del repositorio de eventos para desarrollo local
/// Cuando ClickHouse no está disponible, usa esta implementación
/// </summary>
public class InMemoryEventRepository : IEventRepository
{
    private readonly ConcurrentBag<TrackedEvent> _events = new();
    private readonly ConcurrentBag<PageViewEvent> _pageViews = new();
    private readonly ConcurrentBag<SearchEvent> _searches = new();
    private readonly ConcurrentBag<VehicleViewEvent> _vehicleViews = new();
    private readonly ILogger<InMemoryEventRepository> _logger;

    public InMemoryEventRepository(ILogger<InMemoryEventRepository> logger)
    {
        _logger = logger;
        _logger.LogInformation("Using InMemory event repository (ClickHouse not available)");
    }

    #region CREATE Operations

    public Task IngestEventAsync(TrackedEvent trackedEvent, CancellationToken cancellationToken = default)
    {
        _events.Add(trackedEvent);
        
        // Also add to specialized collections based on type
        switch (trackedEvent)
        {
            case PageViewEvent pv:
                _pageViews.Add(pv);
                break;
            case SearchEvent se:
                _searches.Add(se);
                break;
            case VehicleViewEvent vv:
                _vehicleViews.Add(vv);
                break;
        }
        
        _logger.LogDebug("Event {EventType} ingested for session {SessionId}", trackedEvent.EventType, trackedEvent.SessionId);
        return Task.CompletedTask;
    }

    public Task IngestBatchAsync(IEnumerable<TrackedEvent> events, CancellationToken cancellationToken = default)
    {
        var eventsList = events.ToList();
        foreach (var e in eventsList)
        {
            _events.Add(e);
            
            switch (e)
            {
                case PageViewEvent pv:
                    _pageViews.Add(pv);
                    break;
                case SearchEvent se:
                    _searches.Add(se);
                    break;
                case VehicleViewEvent vv:
                    _vehicleViews.Add(vv);
                    break;
            }
        }
        _logger.LogInformation("Batch of {Count} events ingested successfully (InMemory)", eventsList.Count);
        return Task.CompletedTask;
    }

    #endregion

    #region READ Operations

    public Task<List<TrackedEvent>> GetEventsByTypeAsync(
        string eventType,
        DateTime startDate,
        DateTime endDate,
        int limit = 1000,
        CancellationToken cancellationToken = default)
    {
        var result = _events
            .Where(e => e.EventType == eventType && e.Timestamp >= startDate && e.Timestamp <= endDate)
            .OrderByDescending(e => e.Timestamp)
            .Take(limit)
            .ToList();

        return Task.FromResult(result);
    }

    public Task<List<TrackedEvent>> GetEventsByUserAsync(
        Guid userId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var result = _events
            .Where(e => e.UserId == userId && e.Timestamp >= startDate && e.Timestamp <= endDate)
            .OrderByDescending(e => e.Timestamp)
            .ToList();

        return Task.FromResult(result);
    }

    public Task<List<TrackedEvent>> GetEventsBySessionAsync(
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        var result = _events
            .Where(e => e.SessionId == sessionId)
            .OrderByDescending(e => e.Timestamp)
            .ToList();

        return Task.FromResult(result);
    }

    public Task<List<PageViewEvent>> GetPageViewsAsync(
        DateTime startDate,
        DateTime endDate,
        int limit = 1000,
        CancellationToken cancellationToken = default)
    {
        var result = _pageViews
            .Where(e => e.Timestamp >= startDate && e.Timestamp <= endDate)
            .OrderByDescending(e => e.Timestamp)
            .Take(limit)
            .ToList();

        return Task.FromResult(result);
    }

    public Task<List<SearchEvent>> GetSearchesAsync(
        DateTime startDate,
        DateTime endDate,
        int limit = 1000,
        CancellationToken cancellationToken = default)
    {
        var result = _searches
            .Where(e => e.Timestamp >= startDate && e.Timestamp <= endDate)
            .OrderByDescending(e => e.Timestamp)
            .Take(limit)
            .ToList();

        return Task.FromResult(result);
    }

    public Task<List<VehicleViewEvent>> GetVehicleViewsAsync(
        DateTime startDate,
        DateTime endDate,
        int limit = 1000,
        CancellationToken cancellationToken = default)
    {
        var result = _vehicleViews
            .Where(e => e.Timestamp >= startDate && e.Timestamp <= endDate)
            .OrderByDescending(e => e.Timestamp)
            .Take(limit)
            .ToList();

        return Task.FromResult(result);
    }

    public Task<List<VehicleViewEvent>> GetVehicleViewsByVehicleIdAsync(
        Guid vehicleId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var result = _vehicleViews
            .Where(e => e.VehicleId == vehicleId && e.Timestamp >= startDate && e.Timestamp <= endDate)
            .OrderByDescending(e => e.Timestamp)
            .ToList();

        return Task.FromResult(result);
    }

    #endregion

    #region Analytics Operations

    public Task<Dictionary<string, long>> CountEventsByTypeAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var result = _events
            .Where(e => e.Timestamp >= startDate && e.Timestamp <= endDate)
            .GroupBy(e => e.EventType)
            .ToDictionary(g => g.Key, g => (long)g.Count());

        return Task.FromResult(result);
    }

    public Task<Dictionary<string, long>> CountPageViewsByUrlAsync(
        DateTime startDate,
        DateTime endDate,
        int topN = 100,
        CancellationToken cancellationToken = default)
    {
        var result = _pageViews
            .Where(e => e.Timestamp >= startDate && e.Timestamp <= endDate)
            .GroupBy(e => e.PageUrl)
            .OrderByDescending(g => g.Count())
            .Take(topN)
            .ToDictionary(g => g.Key, g => (long)g.Count());

        return Task.FromResult(result);
    }

    public Task<Dictionary<string, (long Count, double AvgResults, double CTR)>> GetTopSearchQueriesAsync(
        DateTime startDate,
        DateTime endDate,
        int topN = 100,
        CancellationToken cancellationToken = default)
    {
        var result = _searches
            .Where(e => e.Timestamp >= startDate && e.Timestamp <= endDate)
            .GroupBy(e => e.SearchQuery)
            .OrderByDescending(g => g.Count())
            .Take(topN)
            .ToDictionary(
                g => g.Key,
                g => (
                    Count: (long)g.Count(),
                    AvgResults: g.Average(e => e.ResultsCount),
                    CTR: g.Count(e => e.ClickedPosition.HasValue) / (double)g.Count() * 100
                )
            );

        return Task.FromResult(result);
    }

    public Task<Dictionary<Guid, (string Title, long Views, double AvgTime, long Contacts, long Favorites, double ConversionRate)>> GetMostViewedVehiclesAsync(
        DateTime startDate,
        DateTime endDate,
        int topN = 100,
        CancellationToken cancellationToken = default)
    {
        var result = _vehicleViews
            .Where(e => e.Timestamp >= startDate && e.Timestamp <= endDate)
            .GroupBy(e => e.VehicleId)
            .OrderByDescending(g => g.Count())
            .Take(topN)
            .ToDictionary(
                g => g.Key,
                g => (
                    Title: g.First().VehicleTitle,
                    Views: (long)g.Count(),
                    AvgTime: g.Where(e => e.TimeSpentSeconds.HasValue).Select(e => e.TimeSpentSeconds!.Value).DefaultIfEmpty(0).Average(),
                    Contacts: (long)g.Count(e => e.ClickedContact),
                    Favorites: (long)g.Count(e => e.AddedToFavorites),
                    ConversionRate: g.Count(e => e.ClickedContact || e.AddedToFavorites) / (double)g.Count() * 100
                )
            );

        return Task.FromResult(result);
    }

    public Task<long> GetUniqueVisitorsCountAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var count = _events
            .Where(e => e.Timestamp >= startDate && e.Timestamp <= endDate)
            .Select(e => e.SessionId)
            .Distinct()
            .Count();

        return Task.FromResult((long)count);
    }

    public Task<double> GetConversionRateAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var vehicleViews = _vehicleViews
            .Where(e => e.Timestamp >= startDate && e.Timestamp <= endDate)
            .ToList();

        if (!vehicleViews.Any())
            return Task.FromResult(0.0);

        var conversions = vehicleViews.Count(e => e.ClickedContact || e.AddedToFavorites);
        var rate = (double)conversions / vehicleViews.Count * 100;

        return Task.FromResult(rate);
    }

    #endregion

    #region Cleanup Operations

    public Task DeleteOldEventsAsync(DateTime olderThan, CancellationToken cancellationToken = default)
    {
        // InMemory no necesita delete ya que se limpia al reiniciar
        _logger.LogInformation("Delete not needed for InMemory repository (olderThan: {OlderThan})", olderThan);
        return Task.CompletedTask;
    }

    public Task ArchiveOldEventsAsync(DateTime olderThan, CancellationToken cancellationToken = default)
    {
        // InMemory no necesita archive
        _logger.LogInformation("Archive not needed for InMemory repository (olderThan: {OlderThan})", olderThan);
        return Task.CompletedTask;
    }

    #endregion
}
