using RateLimitingService.Core.Models;

namespace RateLimitingService.Core.Interfaces;

/// <summary>
/// Service for distributed rate limiting
/// </summary>
public interface IRateLimitingService
{
    /// <summary>
    /// Checks if a request is allowed based on rate limiting rules
    /// </summary>
    /// <param name="clientId">Client identifier (API key, user ID, IP, etc.)</param>
    /// <param name="endpoint">The endpoint being accessed</param>
    /// <param name="tier">The user tier for determining limits</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Rate limit result indicating if request is allowed</returns>
    Task<RateLimitResult> CheckRateLimitAsync(
        string clientId,
        string endpoint,
        string? tier = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific policy by ID
    /// </summary>
    /// <param name="policyId">Policy identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The policy if found</returns>
    Task<RateLimitPolicy?> GetPolicyAsync(string policyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a policy by name
    /// </summary>
    /// <param name="policyName">Policy name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The policy if found</returns>
    Task<RateLimitPolicy?> GetPolicyByNameAsync(string policyName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all policies
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of all policies</returns>
    Task<IEnumerable<RateLimitPolicy>> GetAllPoliciesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new rate limit policy
    /// </summary>
    /// <param name="policy">Policy to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created policy</returns>
    Task<RateLimitPolicy> CreatePolicyAsync(RateLimitPolicy policy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing policy
    /// </summary>
    /// <param name="policy">Policy with updates</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated policy</returns>
    Task<RateLimitPolicy> UpdatePolicyAsync(RateLimitPolicy policy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a policy
    /// </summary>
    /// <param name="policyId">Policy ID to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted</returns>
    Task<bool> DeletePolicyAsync(string policyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets rate limit statistics
    /// </summary>
    /// <param name="from">Start time</param>
    /// <param name="to">End time</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Statistics for the period</returns>
    Task<RateLimitStatistics> GetStatisticsAsync(
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets current usage for a client
    /// </summary>
    /// <param name="clientId">Client identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Client statistics</returns>
    Task<ClientStatistics?> GetClientUsageAsync(string clientId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resets rate limit for a client
    /// </summary>
    /// <param name="clientId">Client identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if reset successful</returns>
    Task<bool> ResetClientLimitAsync(string clientId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks service health
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if healthy</returns>
    Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default);
}
