using FeatureToggleService.Domain.Entities;

namespace FeatureToggleService.Application.Interfaces;

/// <summary>
/// Service interface for feature flag evaluation
/// </summary>
public interface IFeatureFlagEvaluator
{
    /// <summary>
    /// Evaluate if a feature flag is enabled for the given context
    /// </summary>
    Task<bool> EvaluateAsync(string key, EvaluationContext? context = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Evaluate multiple feature flags at once
    /// </summary>
    Task<Dictionary<string, bool>> EvaluateMultipleAsync(IEnumerable<string> keys, EvaluationContext? context = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get the JSON payload for a feature flag
    /// </summary>
    Task<string?> GetPayloadAsync(string key, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Check if a flag exists
    /// </summary>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
}
