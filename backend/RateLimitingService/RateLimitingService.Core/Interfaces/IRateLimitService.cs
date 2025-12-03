using RateLimitingService.Core.Models;

namespace RateLimitingService.Core.Interfaces;

/// <summary>
/// Main rate limiting service
/// </summary>
public interface IRateLimitService
{
    /// <summary>
    /// Check if request is allowed
    /// </summary>
    Task<RateLimitCheckResult> CheckAsync(RateLimitCheckRequest request);

    /// <summary>
    /// Reset rate limit for identifier
    /// </summary>
    Task ResetAsync(string identifier, RateLimitIdentifierType type);

    /// <summary>
    /// Get current status for identifier
    /// </summary>
    Task<RateLimitCheckResult> GetStatusAsync(string identifier, RateLimitIdentifierType type, string? endpoint = null);

    /// <summary>
    /// Log rate limit violation
    /// </summary>
    Task LogViolationAsync(RateLimitViolation violation);

    /// <summary>
    /// Get violations
    /// </summary>
    Task<IEnumerable<RateLimitViolation>> GetViolationsAsync(int count = 100);

    /// <summary>
    /// Get statistics
    /// </summary>
    Task<RateLimitStatistics> GetStatisticsAsync(DateTime? from = null, DateTime? to = null);
}
