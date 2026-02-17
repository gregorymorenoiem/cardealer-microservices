using System.Net.Http.Json;
using System.Text.Json;
using CarDealer.Shared.FeatureFlags.Configuration;
using CarDealer.Shared.FeatureFlags.Interfaces;
using CarDealer.Shared.FeatureFlags.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CarDealer.Shared.FeatureFlags.Services;

/// <summary>
/// Cliente HTTP para FeatureToggleService con cache local
/// </summary>
public class FeatureFlagClient : IFeatureFlagClient
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<FeatureFlagClient> _logger;
    private readonly FeatureFlagOptions _options;
    private readonly JsonSerializerOptions _jsonOptions;

    private const string AllFlagsCacheKey = "feature_flags_all";
    private const string FlagCacheKeyPrefix = "feature_flag_";

    public FeatureFlagClient(
        HttpClient httpClient,
        IMemoryCache cache,
        ILogger<FeatureFlagClient> logger,
        IOptions<FeatureFlagOptions> options)
    {
        _httpClient = httpClient;
        _cache = cache;
        _logger = logger;
        _options = options.Value;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<bool> IsEnabledAsync(string key, CancellationToken cancellationToken = default)
    {
        return await IsEnabledAsync(key, new FeatureFlagContext { Environment = _options.Environment }, cancellationToken);
    }

    public async Task<bool> IsEnabledAsync(string key, FeatureFlagContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await EvaluateAsync(key, context, cancellationToken);
            return result.IsEnabled;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error evaluating feature flag {Key}, returning default value {Default}", key, _options.DefaultValueOnError);
            return _options.DefaultValueOnError;
        }
    }

    public async Task<FeatureFlagEvaluationResult> EvaluateAsync(string key, FeatureFlagContext? context = null, CancellationToken cancellationToken = default)
    {
        var flag = await GetByKeyAsync(key, cancellationToken);

        if (flag == null)
        {
            _logger.LogDebug("Feature flag {Key} not found, returning disabled", key);
            return new FeatureFlagEvaluationResult
            {
                Key = key,
                IsEnabled = _options.DefaultValueOnError,
                Reason = "flag_not_found"
            };
        }

        // Evaluar el flag
        var isEnabled = EvaluateFlag(flag, context);

        return new FeatureFlagEvaluationResult
        {
            Key = key,
            IsEnabled = isEnabled,
            Reason = DetermineReason(flag, context, isEnabled)
        };
    }

    public async Task<IReadOnlyList<FeatureFlagDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        if (_options.EnableCache && _cache.TryGetValue(AllFlagsCacheKey, out IReadOnlyList<FeatureFlagDto>? cached) && cached != null)
        {
            return cached;
        }

        try
        {
            var response = await _httpClient.GetAsync("api/featureflags", cancellationToken);
            response.EnsureSuccessStatusCode();

            var flags = await response.Content.ReadFromJsonAsync<List<FeatureFlagDto>>(_jsonOptions, cancellationToken) 
                ?? new List<FeatureFlagDto>();

            if (_options.EnableCache)
            {
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(_options.CacheTimeSeconds));
                
                _cache.Set(AllFlagsCacheKey, (IReadOnlyList<FeatureFlagDto>)flags, cacheOptions);

                // Cache individual flags
                foreach (var flag in flags)
                {
                    _cache.Set($"{FlagCacheKeyPrefix}{flag.Key}", flag, cacheOptions);
                }
            }

            _logger.LogDebug("Loaded {Count} feature flags from FeatureToggleService", flags.Count);
            return flags;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch feature flags from FeatureToggleService");
            return new List<FeatureFlagDto>();
        }
    }

    public async Task<FeatureFlagDto?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{FlagCacheKeyPrefix}{key}";

        if (_options.EnableCache && _cache.TryGetValue(cacheKey, out FeatureFlagDto? cached))
        {
            return cached;
        }

        try
        {
            var response = await _httpClient.GetAsync($"api/featureflags/key/{key}", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogDebug("Feature flag {Key} not found (HTTP {StatusCode})", key, response.StatusCode);
                return null;
            }

            var flag = await response.Content.ReadFromJsonAsync<FeatureFlagDto>(_jsonOptions, cancellationToken);

            if (flag != null && _options.EnableCache)
            {
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(_options.CacheTimeSeconds));
                _cache.Set(cacheKey, flag, cacheOptions);
            }

            return flag;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch feature flag {Key}", key);
            return null;
        }
    }

    public Task RefreshCacheAsync(CancellationToken cancellationToken = default)
    {
        // Invalidar cache
        _cache.Remove(AllFlagsCacheKey);
        
        // Recargar todos los flags
        return GetAllAsync(cancellationToken);
    }

    private bool EvaluateFlag(FeatureFlagDto flag, FeatureFlagContext? context)
    {
        // Si el flag está deshabilitado globalmente
        if (!flag.IsEnabled)
            return false;

        // Si no hay contexto, usar el valor global
        if (context == null)
            return flag.IsEnabled;

        // Verificar ambiente
        if (!string.IsNullOrEmpty(flag.Environment) && 
            !string.IsNullOrEmpty(context.Environment) &&
            !flag.Environment.Equals(context.Environment, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // Verificar target users
        if (flag.TargetUsers?.Count > 0 && !string.IsNullOrEmpty(context.UserId))
        {
            if (flag.TargetUsers.Contains(context.UserId, StringComparer.OrdinalIgnoreCase))
                return true;
        }

        // Verificar target groups
        if (flag.TargetGroups?.Count > 0 && context.UserGroups?.Count > 0)
        {
            if (flag.TargetGroups.Intersect(context.UserGroups, StringComparer.OrdinalIgnoreCase).Any())
                return true;
        }

        // Verificar rollout percentage
        if (flag.RolloutPercentage.HasValue && flag.RolloutPercentage < 100)
        {
            var hash = GetConsistentHash(context.UserId ?? Guid.NewGuid().ToString());
            var percentage = hash % 100;
            return percentage < flag.RolloutPercentage.Value;
        }

        return flag.IsEnabled;
    }

    private static int GetConsistentHash(string value)
    {
        unchecked
        {
            int hash = 17;
            foreach (char c in value)
            {
                hash = hash * 31 + c;
            }
            return Math.Abs(hash);
        }
    }

    private static string DetermineReason(FeatureFlagDto flag, FeatureFlagContext? context, bool isEnabled)
    {
        if (!flag.IsEnabled)
            return "globally_disabled";
        
        if (!isEnabled)
        {
            if (context?.Environment != null && flag.Environment != null)
                return "environment_mismatch";
            if (flag.RolloutPercentage.HasValue)
                return "rollout_excluded";
            return "targeting_rules";
        }

        if (flag.TargetUsers?.Count > 0)
            return "user_targeted";
        if (flag.TargetGroups?.Count > 0)
            return "group_targeted";
        if (flag.RolloutPercentage.HasValue)
            return "rollout_included";
        
        return "globally_enabled";
    }
}
