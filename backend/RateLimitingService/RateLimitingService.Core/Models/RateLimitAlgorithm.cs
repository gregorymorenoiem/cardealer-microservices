namespace RateLimitingService.Core.Models;

/// <summary>
/// Rate limiting algorithms supported
/// </summary>
public enum RateLimitAlgorithm
{
    /// <summary>
    /// Token Bucket - Allows bursts up to bucket capacity
    /// </summary>
    TokenBucket,

    /// <summary>
    /// Sliding Window - More accurate but more expensive
    /// </summary>
    SlidingWindow,

    /// <summary>
    /// Fixed Window - Simple, resets at fixed intervals
    /// </summary>
    FixedWindow,

    /// <summary>
    /// Leaky Bucket - Smooth rate, no bursts
    /// </summary>
    LeakyBucket
}
