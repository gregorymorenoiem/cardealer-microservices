using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using ReviewService.Application.Interfaces;

namespace ReviewService.Infrastructure.External;

/// <summary>
/// Implementación de IContactVerificationService que llama a ContactService
/// vía HTTP al endpoint interno /api/internal/contact-requests/buyer/{buyerId}/has-contacted/{sellerId}.
/// Sigue el patrón fail-safe: si ContactService no responde, deniega la reseña (seguridad > conveniencia).
/// </summary>
public class ContactVerificationService : IContactVerificationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ContactVerificationService> _logger;

    public ContactVerificationService(HttpClient httpClient, ILogger<ContactVerificationService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<bool> HasBuyerContactedSellerAsync(Guid buyerId, Guid sellerId, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"/api/internal/contact-requests/buyer/{buyerId}/has-contacted/{sellerId}", ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Failed to verify buyer-seller contact. BuyerId={BuyerId}, SellerId={SellerId}, HTTP {StatusCode}. Defaulting to DENY review.",
                    buyerId, sellerId, response.StatusCode);
                return false; // fail-safe: deny review if we can't verify
            }

            var result = await response.Content.ReadFromJsonAsync<ContactCheckResponse>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, ct);

            return result?.HasContacted ?? false;
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning(
                "Timeout checking buyer-seller contact. BuyerId={BuyerId}, SellerId={SellerId}. Defaulting to DENY review.",
                buyerId, sellerId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error checking buyer-seller contact. BuyerId={BuyerId}, SellerId={SellerId}. Defaulting to DENY review.",
                buyerId, sellerId);
            return false;
        }
    }

    /// <summary>
    /// Internal DTO for deserializing ContactService response.
    /// Matches: { buyerId, sellerId, hasContacted }
    /// </summary>
    private sealed record ContactCheckResponse(Guid BuyerId, Guid SellerId, bool HasContacted);
}
