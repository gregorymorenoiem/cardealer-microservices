using CacheService.Domain;

namespace CacheService.Application.Interfaces;

/// <summary>
/// Interface for cache statistics tracking
/// </summary>
public interface IStatisticsManager
{
    /// <summary>
    /// Gets current cache statistics
    /// </summary>
    Task<CacheStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Resets cache statistics
    /// </summary>
    Task ResetStatisticsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Records a cache hit
    /// </summary>
    void RecordHit(string key);

    /// <summary>
    /// Records a cache miss
    /// </summary>
    void RecordMiss(string key);

    /// <summary>
    /// Records a set operation
    /// </summary>
    void RecordSet();

    /// <summary>
    /// Records a delete operation
    /// </summary>
    void RecordDelete();

    /// <summary>
    /// Gets cache size in bytes
    /// </summary>
    Task<long> GetCacheSizeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets total number of keys
    /// </summary>
    Task<long> GetKeyCountAsync(CancellationToken cancellationToken = default);
}
