using EventTrackingService.Domain.Entities;

namespace EventTrackingService.Domain.Interfaces;

/// <summary>
/// Repository for event tracking operations
/// </summary>
public interface IEventRepository
{
    // ============================================
    // CREATE - Event Ingestion
    // ============================================
    
    /// <summary>
    /// Ingest a single event
    /// </summary>
    Task<TrackedEvent> IngestEventAsync(TrackedEvent trackedEvent, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Ingest events in batch (high performance)
    /// </summary>
    Task<int> IngestBatchAsync(IEnumerable<TrackedEvent> events, CancellationToken cancellationToken = default);
    
    // ============================================
    // READ - Query Events
    // ============================================
    
    /// <summary>
    /// Get events by type within date range
    /// </summary>
    Task<List<TrackedEvent>> GetEventsByTypeAsync(string eventType, DateTime startDate, DateTime endDate, int limit = 1000, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get events by user within date range
    /// </summary>
    Task<List<TrackedEvent>> GetEventsByUserAsync(Guid userId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get events by session
    /// </summary>
    Task<List<TrackedEvent>> GetEventsBySessionAsync(string sessionId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get page view events
    /// </summary>
    Task<List<PageViewEvent>> GetPageViewsAsync(DateTime startDate, DateTime endDate, int limit = 1000, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get search events
    /// </summary>
    Task<List<SearchEvent>> GetSearchesAsync(DateTime startDate, DateTime endDate, int limit = 1000, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get vehicle view events
    /// </summary>
    Task<List<VehicleViewEvent>> GetVehicleViewsAsync(DateTime startDate, DateTime endDate, int limit = 1000, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get vehicle views by specific vehicle
    /// </summary>
    Task<List<VehicleViewEvent>> GetVehicleViewsByVehicleIdAsync(Guid vehicleId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    
    // ============================================
    // AGGREGATIONS - Analytics Queries
    // ============================================
    
    /// <summary>
    /// Count events by type
    /// </summary>
    Task<Dictionary<string, long>> CountEventsByTypeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Count page views by URL
    /// </summary>
    Task<Dictionary<string, long>> CountPageViewsByUrlAsync(DateTime startDate, DateTime endDate, int topN = 100, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get top search queries
    /// </summary>
    Task<Dictionary<string, long>> GetTopSearchQueriesAsync(DateTime startDate, DateTime endDate, int topN = 100, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get most viewed vehicles
    /// </summary>
    Task<Dictionary<Guid, long>> GetMostViewedVehiclesAsync(DateTime startDate, DateTime endDate, int topN = 100, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get unique visitors count
    /// </summary>
    Task<long> GetUniqueVisitorsCountAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get conversion rate (vehicle view to contact/favorite)
    /// </summary>
    Task<double> GetConversionRateAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    
    // ============================================
    // RETENTION & CLEANUP
    // ============================================
    
    /// <summary>
    /// Delete events older than retention period
    /// </summary>
    Task<int> DeleteOldEventsAsync(DateTime olderThan, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Archive old events to cold storage
    /// </summary>
    Task<int> ArchiveOldEventsAsync(DateTime olderThan, CancellationToken cancellationToken = default);
}
