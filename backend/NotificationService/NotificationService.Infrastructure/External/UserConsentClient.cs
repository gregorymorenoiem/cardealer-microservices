using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using NotificationService.Application.Interfaces;

namespace NotificationService.Infrastructure.External;

/// <summary>
/// Calls UserService API to check user communication preferences before sending marketing notifications.
/// Falls back to "deny" (no marketing) if UserService is unreachable — fail-safe for consent.
/// </summary>
public class UserConsentClient : IUserConsentClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UserConsentClient> _logger;

    public UserConsentClient(HttpClient httpClient, ILogger<UserConsentClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<bool> IsEmailMarketingAllowedAsync(Guid userId, CancellationToken ct = default)
    {
        var prefs = await GetPreferencesAsync(userId, ct);
        // EmailPromotions or EmailNewsletter = marketing opt-in
        return prefs?.Email?.Promotions == true || prefs?.Email?.Newsletter == true;
    }

    public async Task<bool> IsWhatsAppMarketingAllowedAsync(Guid userId, CancellationToken ct = default)
    {
        var prefs = await GetPreferencesAsync(userId, ct);
        return prefs?.WhatsApp?.Marketing == true;
    }

    public async Task<bool> IsSmsMarketingAllowedAsync(Guid userId, CancellationToken ct = default)
    {
        var prefs = await GetPreferencesAsync(userId, ct);
        return prefs?.Sms?.Promotions == true;
    }

    private async Task<UserPreferencesResponse?> GetPreferencesAsync(Guid userId, CancellationToken ct)
    {
        try
        {
            // Internal service-to-service call — uses the internal gateway or direct URL
            var response = await _httpClient.GetAsync(
                $"/api/privacy/preferences?userId={userId}", ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Failed to get consent preferences for UserId={UserId}. HTTP {StatusCode}. Defaulting to DENY marketing.",
                    userId, response.StatusCode);
                return null; // fail-safe: deny marketing
            }

            return await response.Content.ReadFromJsonAsync<UserPreferencesResponse>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, ct);
        }
        catch (Exception ex)
        {
            // Fail-safe: if we can't verify consent, DON'T send marketing
            _logger.LogError(ex,
                "Error checking consent for UserId={UserId}. Defaulting to DENY marketing.", userId);
            return null;
        }
    }

    // Internal DTOs for deserializing UserService response
    private sealed record UserPreferencesResponse(
        EmailPrefs? Email,
        SmsPrefs? Sms,
        PushPrefs? Push,
        WhatsAppPrefs? WhatsApp,
        PrivacyPrefs? Privacy,
        DateTime? LastUpdated
    );

    private sealed record EmailPrefs(
        bool ActivityNotifications, bool ListingUpdates,
        bool Newsletter, bool Promotions, bool PriceAlerts);

    private sealed record SmsPrefs(
        bool VerificationCodes, bool PriceAlerts, bool Promotions);

    private sealed record PushPrefs(
        bool NewMessages, bool PriceChanges, bool Recommendations);

    private sealed record WhatsAppPrefs(
        bool Transactional, bool Marketing, bool PriceAlerts);

    private sealed record PrivacyPrefs(
        bool AllowProfiling, bool AllowThirdPartySharing,
        bool AllowAnalytics, bool AllowRetargeting);
}
