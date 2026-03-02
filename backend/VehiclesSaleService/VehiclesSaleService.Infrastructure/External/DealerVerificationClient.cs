using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using VehiclesSaleService.Application.Interfaces;

namespace VehiclesSaleService.Infrastructure.External;

/// <summary>
/// HTTP client that queries DealerManagementService to verify a dealer's KYC status.
/// Used before allowing a dealer to publish a vehicle listing.
/// </summary>
public class DealerVerificationClient : IDealerVerificationClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DealerVerificationClient> _logger;

    public DealerVerificationClient(HttpClient httpClient, ILogger<DealerVerificationClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<bool> IsDealerVerifiedAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"/api/dealers/user/{userId}",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "DealerManagementService returned {StatusCode} for userId {UserId}",
                    response.StatusCode, userId);
                return false;
            }

            var dealer = await response.Content.ReadFromJsonAsync<DealerProfileDto>(
                cancellationToken: cancellationToken);

            if (dealer is null)
                return false;

            // VerificationStatus == "Verified" (string from DealerManagementService)
            return string.Equals(dealer.VerificationStatus, "Verified",
                StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to check dealer verification for userId {UserId}. Defaulting to NOT verified.",
                userId);
            // Fail-closed: treat as unverified if service is unavailable
            return false;
        }
    }

    private record DealerProfileDto(
        Guid Id,
        Guid UserId,
        string BusinessName,
        string VerificationStatus);
}
