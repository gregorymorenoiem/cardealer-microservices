using FeatureToggleService.Application.Interfaces;
using FeatureToggleService.Domain.Entities;
using FeatureToggleService.Domain.Enums;
using FeatureToggleService.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Environment = FeatureToggleService.Domain.Enums.Environment;

namespace FeatureToggleService.Infrastructure.Services;

/// <summary>
/// Service for evaluating feature flags with caching support
/// </summary>
public class FeatureFlagEvaluator : IFeatureFlagEvaluator
{
    private readonly IFeatureFlagRepository _repository;
    private readonly IMemoryCache _cache;
    private readonly ILogger<FeatureFlagEvaluator> _logger;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(1);

    public FeatureFlagEvaluator(
        IFeatureFlagRepository repository,
        IMemoryCache cache,
        ILogger<FeatureFlagEvaluator> logger)
    {
        _repository = repository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<bool> EvaluateAsync(string key, EvaluationContext? context = null, CancellationToken cancellationToken = default)
    {
        var flag = await GetFlagFromCacheAsync(key, cancellationToken);

        if (flag == null)
        {
            _logger.LogDebug("Feature flag '{Key}' not found, returning false", key);
            return false;
        }

        var result = EvaluateFlag(flag, context);
        _logger.LogDebug("Feature flag '{Key}' evaluated to {Result}", key, result);

        return result;
    }

    public async Task<Dictionary<string, bool>> EvaluateMultipleAsync(IEnumerable<string> keys, EvaluationContext? context = null, CancellationToken cancellationToken = default)
    {
        var results = new Dictionary<string, bool>();

        foreach (var key in keys)
        {
            results[key] = await EvaluateAsync(key, context, cancellationToken);
        }

        return results;
    }

    public async Task<string?> GetPayloadAsync(string key, CancellationToken cancellationToken = default)
    {
        var flag = await GetFlagFromCacheAsync(key, cancellationToken);
        return flag?.JsonPayload;
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _repository.ExistsAsync(key, cancellationToken);
    }

    private bool EvaluateFlag(FeatureFlag flag, EvaluationContext? context)
    {
        // Kill switch takes precedence
        if (flag.KillSwitchTriggered)
        {
            _logger.LogDebug("Kill switch triggered for flag '{Key}'", flag.Key);
            return false;
        }

        // Check if flag is enabled
        if (!flag.IsEnabled)
            return false;

        // Check if flag status is active
        if (flag.Status != FlagStatus.Active)
            return false;

        // Check expiration
        if (flag.IsExpired)
        {
            _logger.LogDebug("Flag '{Key}' is expired", flag.Key);
            return false;
        }

        // Check environment match
        if (context != null && !string.IsNullOrEmpty(context.Environment))
        {
            if (flag.Environment != Environment.All)
            {
                var contextEnv = Enum.TryParse<Environment>(context.Environment, true, out var parsedEnv)
                    ? parsedEnv
                    : Environment.Production;

                if (flag.Environment != contextEnv)
                {
                    _logger.LogDebug("Environment mismatch for flag '{Key}': flag is {FlagEnv}, context is {ContextEnv}",
                        flag.Key, flag.Environment, context.Environment);
                    return false;
                }
            }
        }

        // Check if user is explicitly targeted
        if (context != null && !string.IsNullOrEmpty(context.UserId))
        {
            if (flag.TargetUserIds.Contains(context.UserId))
            {
                _logger.LogDebug("User '{UserId}' is targeted for flag '{Key}'", context.UserId, flag.Key);
                return true;
            }
        }

        // Check group targeting
        if (context != null && context.Groups.Any())
        {
            if (flag.TargetGroups.Any(g => context.Groups.Contains(g)))
            {
                _logger.LogDebug("User is in a targeted group for flag '{Key}'", flag.Key);
                return true;
            }
        }

        // Apply percentage rollout
        if (flag.RolloutPercentage < 100)
        {
            if (flag.RolloutPercentage == 0)
                return false;

            var userHash = GetUserHash(context?.UserId ?? Guid.NewGuid().ToString(), flag.Key);
            var inRollout = userHash % 100 < flag.RolloutPercentage;
            _logger.LogDebug("User hash {Hash} for flag '{Key}' with {Percentage}% rollout: {InRollout}",
                userHash, flag.Key, flag.RolloutPercentage, inRollout);
            return inRollout;
        }

        return true;
    }

    private int GetUserHash(string userId, string flagKey)
    {
        var combined = $"{userId}:{flagKey}";
        return Math.Abs(combined.GetHashCode());
    }

    private async Task<FeatureFlag?> GetFlagFromCacheAsync(string key, CancellationToken cancellationToken)
    {
        var cacheKey = $"feature_flag_{key}";

        if (!_cache.TryGetValue(cacheKey, out FeatureFlag? flag))
        {
            flag = await _repository.GetByKeyAsync(key, cancellationToken);

            if (flag != null)
            {
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(_cacheExpiration);

                _cache.Set(cacheKey, flag, cacheOptions);
            }
        }

        return flag;
    }
}
