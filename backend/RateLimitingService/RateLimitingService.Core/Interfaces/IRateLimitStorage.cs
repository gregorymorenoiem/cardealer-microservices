using RateLimitingService.Core.Models;

namespace RateLimitingService.Core.Interfaces;

/// <summary>
/// Interface for rate limit storage
/// </summary>
public interface IRateLimitStorage
{
    /// <summary>
    /// Increment counter and get current value
    /// </summary>
    Task<long> IncrementAsync(string key, TimeSpan expiry);

    /// <summary>
    /// Get current value
    /// </summary>
    Task<long> GetAsync(string key);

    /// <summary>
    /// Set value with expiry
    /// </summary>
    Task SetAsync(string key, long value, TimeSpan expiry);

    /// <summary>
    /// Delete key
    /// </summary>
    Task DeleteAsync(string key);

    /// <summary>
    /// Get time to live for key
    /// </summary>
    Task<TimeSpan?> GetTtlAsync(string key);

    /// <summary>
    /// Add to sorted set (for sliding window)
    /// </summary>
    Task<long> AddToSortedSetAsync(string key, double score, string member, TimeSpan expiry);

    /// <summary>
    /// Remove from sorted set by score range
    /// </summary>
    Task RemoveFromSortedSetByScoreAsync(string key, double minScore, double maxScore);

    /// <summary>
    /// Count items in sorted set by score range
    /// </summary>
    Task<long> CountInSortedSetByScoreAsync(string key, double minScore, double maxScore);

    /// <summary>
    /// Set expiry on key
    /// </summary>
    Task<bool> ExpireAsync(string key, TimeSpan expiry);
}
