using RateLimitingService.Core.Models;

namespace RateLimitingService.Core.Interfaces;

/// <summary>
/// Interface for rule management
/// </summary>
public interface IRateLimitRuleService
{
    /// <summary>
    /// Get all rules
    /// </summary>
    Task<IEnumerable<RateLimitRule>> GetAllRulesAsync();

    /// <summary>
    /// Get rule by ID
    /// </summary>
    Task<RateLimitRule?> GetRuleByIdAsync(string id);

    /// <summary>
    /// Create new rule
    /// </summary>
    Task<RateLimitRule> CreateRuleAsync(RateLimitRule rule);

    /// <summary>
    /// Update existing rule
    /// </summary>
    Task<RateLimitRule> UpdateRuleAsync(RateLimitRule rule);

    /// <summary>
    /// Delete rule
    /// </summary>
    Task DeleteRuleAsync(string id);

    /// <summary>
    /// Get applicable rules for a request
    /// </summary>
    Task<IEnumerable<RateLimitRule>> GetApplicableRulesAsync(RateLimitCheckRequest request);

    /// <summary>
    /// Check if identifier is whitelisted
    /// </summary>
    Task<bool> IsWhitelistedAsync(string identifier, RateLimitIdentifierType type);

    /// <summary>
    /// Check if identifier is blacklisted
    /// </summary>
    Task<bool> IsBlacklistedAsync(string identifier, RateLimitIdentifierType type);
}
