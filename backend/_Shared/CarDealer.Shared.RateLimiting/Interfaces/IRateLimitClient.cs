using CarDealer.Shared.RateLimiting.Models;

namespace CarDealer.Shared.RateLimiting.Interfaces;

/// <summary>
/// Interface for rate limiting client
/// </summary>
public interface IRateLimitClient
{
    /// <summary>
    /// Check if a request is allowed based on rate limiting rules
    /// </summary>
    /// <param name="clientId">Client identifier</param>
    /// <param name="endpoint">The endpoint being accessed</param>
    /// <param name="tier">User tier (anonymous, authenticated, premium, etc.)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Rate limit result</returns>
    Task<RateLimitResult> CheckAsync(
        string clientId,
        string endpoint,
        string? tier = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a request is allowed using request context
    /// </summary>
    Task<RateLimitResult> CheckAsync(
        Microsoft.AspNetCore.Http.HttpContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the client identifier from the HTTP context
    /// </summary>
    string GetClientIdentifier(Microsoft.AspNetCore.Http.HttpContext context);

    /// <summary>
    /// Get the user tier from the HTTP context
    /// </summary>
    string GetUserTier(Microsoft.AspNetCore.Http.HttpContext context);
}
