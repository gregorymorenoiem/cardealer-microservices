namespace ListingAgent.Domain.Interfaces;

/// <summary>
/// Tracks Redis cache hit/miss counters for the ListingAgent.
/// Enables measuring whether the ≥50% hit rate target is being met after the first month.
/// </summary>
public interface IListingCacheMetrics
{
    /// <summary>Record a cache hit (response served from Redis, no LLM API call).</summary>
    Task RecordHitAsync(string cacheKey, CancellationToken ct = default);

    /// <summary>Record a cache miss (LLM API was called to generate the response).</summary>
    Task RecordMissAsync(string cacheKey, CancellationToken ct = default);

    /// <summary>Get the current hit rate (0.0 – 1.0) and raw counters for the current rolling window.</summary>
    Task<ListingCacheStats> GetStatsAsync(CancellationToken ct = default);
}

/// <summary>
/// Cache hit/miss statistics for the ListingAgent.
/// </summary>
public sealed class ListingCacheStats
{
    public long CacheHits { get; init; }
    public long CacheMisses { get; init; }

    /// <summary>Total = Hits + Misses (computed, never stored separately).</summary>
    public long TotalRequests => CacheHits + CacheMisses;

    /// <summary>Hit rate as a percentage (0–100). Target: ≥50% after first month.</summary>
    public double HitRatePercent => TotalRequests == 0 ? 0.0 : Math.Round((double)CacheHits / TotalRequests * 100, 2);

    /// <summary>Whether the ≥50% target is currently being met.</summary>
    public bool TargetMet => HitRatePercent >= 50.0;

    public DateTimeOffset MeasuredAt { get; init; } = DateTimeOffset.UtcNow;
}
