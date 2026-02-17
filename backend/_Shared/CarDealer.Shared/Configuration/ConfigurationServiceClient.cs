using System.Collections.Concurrent;
using System.Net.Http.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CarDealer.Shared.Configuration;

/// <summary>
/// HTTP client that reads configuration values and feature flags from ConfigurationService.
/// Implements in-memory caching (60s TTL) to avoid hitting the service on every request.
/// Fails open: if ConfigurationService is unavailable, returns default values.
/// </summary>
public class ConfigurationServiceClient : IConfigurationServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ConfigurationServiceClient> _logger;
    private readonly ConcurrentDictionary<string, bool> _cacheKeys = new();

    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(60);
    private const string DefaultEnvironment = "Development";

    public ConfigurationServiceClient(
        HttpClient httpClient,
        IMemoryCache cache,
        ILogger<ConfigurationServiceClient> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _logger = logger;
    }

    public async Task<string?> GetValueAsync(string key, CancellationToken ct = default)
    {
        var cacheKey = $"config:{key}";

        if (_cache.TryGetValue(cacheKey, out string? cached))
            return cached;

        try
        {
            var response = await _httpClient.GetAsync(
                $"/api/configurations/{key}?environment={DefaultEnvironment}", ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogDebug(
                    "ConfigurationService returned {Status} for key {Key}",
                    response.StatusCode, key);
                return null;
            }

            var item = await response.Content.ReadFromJsonAsync<ConfigItemResponse>(
                cancellationToken: ct);

            var value = item?.Value;

            _cache.Set(cacheKey, value, CacheDuration);
            _cacheKeys.TryAdd(cacheKey, true);
            return value;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to read configuration key {Key} from ConfigurationService",
                key);
            return null;
        }
    }

    public async Task<string> GetValueAsync(string key, string defaultValue, CancellationToken ct = default)
    {
        var value = await GetValueAsync(key, ct);
        return value ?? defaultValue;
    }

    public async Task<int> GetIntAsync(string key, int defaultValue, CancellationToken ct = default)
    {
        var value = await GetValueAsync(key, ct);
        if (value is not null && int.TryParse(value, out var result))
            return result;
        return defaultValue;
    }

    public async Task<decimal> GetDecimalAsync(string key, decimal defaultValue, CancellationToken ct = default)
    {
        var value = await GetValueAsync(key, ct);
        if (value is not null && decimal.TryParse(value, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var result))
            return result;
        return defaultValue;
    }

    public async Task<bool> IsEnabledAsync(string key, bool defaultValue = true, CancellationToken ct = default)
    {
        var value = await GetValueAsync(key, ct);

        if (value is null)
            return defaultValue;

        return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<Dictionary<string, string?>> GetValuesAsync(
        IEnumerable<string> keys, CancellationToken ct = default)
    {
        var result = new Dictionary<string, string?>();
        var tasks = keys.Select(async key =>
        {
            var value = await GetValueAsync(key, ct);
            return (key, value);
        });

        var values = await Task.WhenAll(tasks);
        foreach (var (key, value) in values)
        {
            result[key] = value;
        }

        return result;
    }

    public async Task<Dictionary<string, string>> GetByCategoryAsync(
        string category, CancellationToken ct = default)
    {
        var cacheKey = $"config:category:{category}";

        if (_cache.TryGetValue(cacheKey, out Dictionary<string, string>? cached) && cached is not null)
            return cached;

        try
        {
            var response = await _httpClient.GetAsync(
                $"/api/configurations/category/{category}?environment={DefaultEnvironment}", ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogDebug(
                    "ConfigurationService returned {Status} for category {Category}",
                    response.StatusCode, category);
                return new Dictionary<string, string>();
            }

            var items = await response.Content.ReadFromJsonAsync<List<ConfigItemResponse>>(
                cancellationToken: ct);

            var result = items?
                .Where(i => i.IsActive && !string.IsNullOrWhiteSpace(i.Value))
                .ToDictionary(i => i.Key, i => i.Value)
                ?? new Dictionary<string, string>();

            _cache.Set(cacheKey, result, CacheDuration);
            _cacheKeys.TryAdd(cacheKey, true);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to read category {Category} from ConfigurationService",
                category);
            return new Dictionary<string, string>();
        }
    }

    public void InvalidateCache(string? key = null)
    {
        if (key is not null)
        {
            _cache.Remove($"config:{key}");
            _cache.Remove($"config:category:{key}");
        }
        else
        {
            foreach (var cacheKey in _cacheKeys.Keys)
            {
                _cache.Remove(cacheKey);
            }
            _cacheKeys.Clear();
        }
    }

    /// <summary>
    /// Minimal DTO matching ConfigurationService response
    /// </summary>
    private sealed record ConfigItemResponse(
        string Key,
        string Value,
        bool IsActive
    );
}
