using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using VehiclesSaleService.Application.Interfaces;

namespace VehiclesSaleService.Infrastructure.External;

/// <summary>
/// Checks a user's KYC verification status via KYCService.
/// Consistent with the frontend's useCanSell hook which also checks KYCService.
/// Used before allowing a dealer to publish a vehicle listing.
/// </summary>
public class DealerVerificationClient : IDealerVerificationClient
{
    // KYCStatus.Approved = 5  (from KYCService.Domain.Entities.KYCEntities.cs)
    private const int KycStatusApproved = 5;

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
            // GET /api/KYCProfiles/user/{userId}  — returns the user's KYC profile
            var response = await _httpClient.GetAsync(
                $"/api/KYCProfiles/user/{userId}",
                cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogInformation("No KYC profile found for dealer userId {UserId}", userId);
                return false;
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "KYCService returned {StatusCode} for userId {UserId}",
                    response.StatusCode, userId);
                return false;
            }

            var profile = await response.Content.ReadFromJsonAsync<KycProfileDto>(
                cancellationToken: cancellationToken);

            if (profile is null)
                return false;

            // Approved = 5 (integer) or "Approved" / "approved" (string representation)
            var isApproved = profile.Status == KycStatusApproved
                || string.Equals(profile.StatusName, "Approved", StringComparison.OrdinalIgnoreCase);

            _logger.LogDebug(
                "KYC status for dealer {UserId}: {Status} — IsApproved={IsApproved}",
                userId, profile.Status, isApproved);

            return isApproved;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to check KYC status for dealer userId {UserId}. Defaulting to NOT verified.",
                userId);
            // Fail-closed: treat as unverified if KYCService is unavailable
            return false;
        }
    }

    // Minimal DTO for KYCService /api/KYCProfiles/user/{userId} response.
    // Status is an integer enum; StatusName may be present as the string representation.
    private record KycProfileDto(
        Guid Id,
        Guid UserId,
        int Status,
        string? StatusName);
}
