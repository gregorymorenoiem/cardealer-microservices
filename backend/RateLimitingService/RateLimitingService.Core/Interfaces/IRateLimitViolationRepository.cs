using RateLimitingService.Core.Models;

namespace RateLimitingService.Core.Interfaces;

/// <summary>
/// Repository interface for violation persistence
/// </summary>
public interface IRateLimitViolationRepository
{
    /// <summary>
    /// Add a violation record
    /// </summary>
    Task AddViolationAsync(RateLimitViolation violation);

    /// <summary>
    /// Get violations by identifier
    /// </summary>
    Task<IEnumerable<RateLimitViolation>> GetViolationsAsync(
        string identifier,
        RateLimitIdentifierType? type = null,
        DateTime? from = null,
        DateTime? to = null,
        int limit = 100);

    /// <summary>
    /// Get violation count by identifier
    /// </summary>
    Task<int> GetViolationCountAsync(
        string identifier,
        RateLimitIdentifierType? type = null,
        DateTime? from = null,
        DateTime? to = null);

    /// <summary>
    /// Get top violators
    /// </summary>
    Task<IEnumerable<(string Identifier, int Count)>> GetTopViolatorsAsync(
        RateLimitIdentifierType? type = null,
        DateTime? from = null,
        DateTime? to = null,
        int limit = 10);

    /// <summary>
    /// Clean old violations
    /// </summary>
    Task<int> CleanOldViolationsAsync(DateTime olderThan);
}
