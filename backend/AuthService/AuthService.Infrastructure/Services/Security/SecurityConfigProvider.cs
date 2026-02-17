using System.Net.Http.Json;
using AuthService.Application.Common.Interfaces;
using AuthService.Shared;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AuthService.Infrastructure.Services.Security;

/// <summary>
/// Reads platform-wide security settings from ConfigurationService (port 15124).
/// Caches values for 60 seconds to minimize HTTP calls.
/// Falls back to appsettings.json defaults when ConfigurationService is unreachable.
/// </summary>
public class SecurityConfigProvider : ISecurityConfigProvider
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<SecurityConfigProvider> _logger;
    private readonly JwtSettings _jwtDefaults;
    private readonly SecuritySettings _securityDefaults;

    // Performance: Increase cache TTL from 60s to 300s — security configs rarely change at runtime
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(300);
    private const string Environment = "Development";

    public SecurityConfigProvider(
        HttpClient httpClient,
        IMemoryCache cache,
        IOptions<JwtSettings> jwtDefaults,
        IOptions<SecuritySettings> securityDefaults,
        ILogger<SecurityConfigProvider> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _jwtDefaults = jwtDefaults.Value;
        _securityDefaults = securityDefaults.Value;
        _logger = logger;
    }

    public async Task<int> GetMaxLoginAttemptsAsync(CancellationToken ct = default)
    {
        var value = await GetIntAsync("security.max_login_attempts", ct);
        return value ?? _securityDefaults.LockoutPolicy?.MaxFailedAccessAttempts ?? 5;
    }

    public async Task<int> GetLockoutDurationMinutesAsync(CancellationToken ct = default)
    {
        var value = await GetIntAsync("security.lockout_duration_minutes", ct);
        return value ?? _securityDefaults.LockoutPolicy?.DefaultLockoutMinutes ?? 15;
    }

    public async Task<int> GetSessionExpirationHoursAsync(CancellationToken ct = default)
    {
        var value = await GetIntAsync("security.session_expiration_hours", ct);
        return value ?? 24;
    }

    public async Task<int> GetMinPasswordLengthAsync(CancellationToken ct = default)
    {
        var value = await GetIntAsync("security.min_password_length", ct);
        return value ?? _securityDefaults.PasswordPolicy?.RequiredLength ?? 8;
    }

    public async Task<int> GetJwtExpiresMinutesAsync(CancellationToken ct = default)
    {
        var value = await GetIntAsync("security.jwt_expires_minutes", ct);
        return value ?? _jwtDefaults.ExpiresMinutes;
    }

    public async Task<int> GetRefreshTokenDaysAsync(CancellationToken ct = default)
    {
        var value = await GetIntAsync("security.refresh_token_days", ct);
        return value ?? _jwtDefaults.RefreshTokenExpiresDays;
    }

    public async Task<bool> GetRequireEmailVerificationAsync(CancellationToken ct = default)
    {
        var value = await GetBoolAsync("security.require_email_verification", ct);
        return value ?? true;
    }

    public async Task<bool> GetAllow2FAAsync(CancellationToken ct = default)
    {
        var value = await GetBoolAsync("security.allow_2fa", ct);
        return value ?? true;
    }

    public async Task<bool> GetForceHttpsAsync(CancellationToken ct = default)
    {
        var value = await GetBoolAsync("security.force_https", ct);
        return value ?? true;
    }

    // =========================================================================
    // Private helpers
    // =========================================================================

    private async Task<int?> GetIntAsync(string key, CancellationToken ct)
    {
        var raw = await GetValueAsync(key, ct);
        if (raw is not null && int.TryParse(raw, out var parsed))
            return parsed;
        return null;
    }

    private async Task<bool?> GetBoolAsync(string key, CancellationToken ct)
    {
        var raw = await GetValueAsync(key, ct);
        if (raw is not null && bool.TryParse(raw, out var parsed))
            return parsed;
        return null;
    }

    private async Task<string?> GetValueAsync(string key, CancellationToken ct)
    {
        var cacheKey = $"securitycfg:{key}";

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

            var item = await response.Content.ReadFromJsonAsync<ConfigItemDto>(cancellationToken: ct);
            var value = item?.Value;

            if (value is not null)
                _cache.Set(cacheKey, value, CacheDuration);

            return value;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to read {Key} from ConfigurationService – using appsettings default", key);
            return null;
        }
    }

    /// <summary>Minimal DTO matching ConfigurationService response.</summary>
    private sealed record ConfigItemDto(string Key, string Value, bool IsActive);
}
