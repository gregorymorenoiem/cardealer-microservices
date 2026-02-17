using System.Net.Http.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NotificationService.Application.Interfaces;

namespace NotificationService.Infrastructure.Services;

/// <summary>
/// HTTP client that reads configuration toggles and secrets from ConfigurationService.
/// Implements in-memory caching (60s TTL) to avoid hitting the service on every notification.
/// Fails open: if ConfigurationService is unavailable, features are considered ENABLED.
/// </summary>
public class ConfigurationServiceClient : IConfigurationServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ConfigurationServiceClient> _logger;

    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(60);
    private static readonly TimeSpan SecretCacheDuration = TimeSpan.FromMinutes(5);
    private const string Environment = "Development";

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
                $"/api/configurations/{key}?environment={Environment}", ct);

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
            return value;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to read configuration key {Key} from ConfigurationService. Defaulting to enabled.",
                key);
            return null;
        }
    }

    public async Task<bool> IsEnabledAsync(string key, CancellationToken ct = default)
    {
        var value = await GetValueAsync(key, ct);

        // Fail-open: if key not found or service unavailable, consider enabled
        if (value is null)
            return true;

        return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<string?> GetSecretAsync(string key, CancellationToken ct = default)
    {
        var cacheKey = $"secret:{key}";

        if (_cache.TryGetValue(cacheKey, out string? cached))
            return cached;

        try
        {
            var response = await _httpClient.GetAsync(
                $"/api/secrets/{key}", ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogDebug(
                    "ConfigurationService returned {Status} for secret {Key}",
                    response.StatusCode, key);
                return null;
            }

            var item = await response.Content.ReadFromJsonAsync<SecretItemResponse>(
                cancellationToken: ct);

            var value = item?.DecryptedValue;

            if (!string.IsNullOrWhiteSpace(value))
                _cache.Set(cacheKey, value, SecretCacheDuration);

            return value;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to read secret key {Key} from ConfigurationService.",
                key);
            return null;
        }
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

    /// <summary>
    /// Minimal DTO matching ConfigurationService response for configs
    /// </summary>
    private sealed record ConfigItemResponse(
        string Key,
        string Value,
        bool IsActive
    );

    /// <summary>
    /// Minimal DTO matching ConfigurationService response for secrets
    /// </summary>
    private sealed record SecretItemResponse(
        string Key,
        string? DecryptedValue,
        string? MaskedValue,
        bool IsActive
    );
}
