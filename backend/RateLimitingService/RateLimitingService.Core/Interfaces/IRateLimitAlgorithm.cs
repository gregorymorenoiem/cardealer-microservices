using RateLimitingService.Core.Models;

namespace RateLimitingService.Core.Interfaces;

/// <summary>
/// Interface for rate limiting algorithms
/// </summary>
public interface IRateLimitAlgorithm
{
    /// <summary>
    /// Check if request is allowed under this algorithm
    /// </summary>
    Task<RateLimitCheckResult> CheckAsync(string key, RateLimitRule rule, int cost = 1);

    /// <summary>
    /// Reset limit for a key
    /// </summary>
    Task ResetAsync(string key);

    /// <summary>
    /// Get current status for a key
    /// </summary>
    Task<RateLimitCheckResult> GetStatusAsync(string key, RateLimitRule rule);
}
