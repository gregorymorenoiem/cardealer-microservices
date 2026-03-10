using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using UserService.Application.Interfaces;
using UserService.Domain.Interfaces;

namespace UserService.Infrastructure.External;

/// <summary>
/// Evaluates the 4 criteria for the "Dealer Verificado OKLA" badge:
///   1. RNC → Verified in DGII (admin flag or future DGII API)
///   2. WhatsApp → Verified by OTP (from Dealer entity)
///   3. ≥10 active listings with moderated photos (via VehiclesSaleServiceClient)
///   4. No fraud reports in last 90 days (via ReportsService HTTP call)
/// </summary>
public class DealerBadgeEvaluator : IDealerBadgeEvaluator
{
    private readonly IDealerRepository _dealerRepository;
    private readonly IVehiclesSaleServiceClient _vehiclesClient;
    private readonly HttpClient _reportsClient;
    private readonly ILogger<DealerBadgeEvaluator> _logger;

    private const int MinModeratedListings = 10;

    public DealerBadgeEvaluator(
        IDealerRepository dealerRepository,
        IVehiclesSaleServiceClient vehiclesClient,
        IHttpClientFactory httpClientFactory,
        ILogger<DealerBadgeEvaluator> logger)
    {
        _dealerRepository = dealerRepository;
        _vehiclesClient = vehiclesClient;
        _reportsClient = httpClientFactory.CreateClient("ReportsService");
        _logger = logger;
    }

    public async Task<DealerBadgeEvaluation> EvaluateAsync(
        Guid dealerId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Evaluating badge criteria for dealer {DealerId}", dealerId);

        // 1. Load dealer entity for flag-based criteria (RNC, WhatsApp)
        var dealer = await _dealerRepository.GetByIdAsync(dealerId);
        if (dealer is null)
        {
            _logger.LogWarning("Dealer {DealerId} not found for badge evaluation", dealerId);
            return new DealerBadgeEvaluation(
                DealerId: dealerId,
                IsRncVerified: false, DgiiTaxpayerStatus: null,
                IsWhatsAppVerified: false,
                ModeratedListingsCount: 0,
                HasRecentFraudReports: false, FraudReportCount90Days: 0,
                IsOklaVerified: false,
                Summary: "Dealer no encontrado");
        }

        // 2. Run external checks in parallel
        var listingsTask = CountActiveListingsAsync(dealerId, cancellationToken);
        var fraudTask = CheckFraudReportsAsync(dealerId, cancellationToken);

        await Task.WhenAll(listingsTask, fraudTask);

        var moderatedCount = await listingsTask;
        var (hasFraud, fraudCount) = await fraudTask;

        // Criteria from entity flags
        var isRncVerified = dealer.IsRncVerified;
        var dgiiStatus = dealer.DgiiTaxpayerStatus;
        var isWhatsAppVerified = dealer.IsWhatsAppVerified;

        var isVerified = isRncVerified
                         && isWhatsAppVerified
                         && moderatedCount >= MinModeratedListings
                         && !hasFraud;

        // Build human-readable summary
        var criteriaMissing = new List<string>();
        if (!isRncVerified) criteriaMissing.Add("RNC no verificado en DGII");
        if (!isWhatsAppVerified) criteriaMissing.Add("WhatsApp no verificado por OTP");
        if (moderatedCount < MinModeratedListings)
            criteriaMissing.Add($"Solo {moderatedCount}/{MinModeratedListings} listings activos moderados");
        if (hasFraud)
            criteriaMissing.Add($"{fraudCount} reportes de fraude en últimos 90 días");

        var met = 4 - criteriaMissing.Count;
        var summary = isVerified
            ? $"✅ Badge otorgado — 4/4 criterios cumplidos"
            : $"❌ {met}/4 criterios — Falta: {string.Join("; ", criteriaMissing)}";

        _logger.LogInformation("Badge evaluation for {DealerId}: {Summary}", dealerId, summary);

        // Persist latest evaluation snapshot on the entity
        dealer.ModeratedListingsCount = moderatedCount;
        dealer.ModeratedListingsUpdatedAt = DateTime.UtcNow;
        dealer.HasRecentFraudReports = hasFraud;
        dealer.FraudCheckAt = DateTime.UtcNow;
        await _dealerRepository.UpdateAsync(dealer);

        return new DealerBadgeEvaluation(
            DealerId: dealerId,
            IsRncVerified: isRncVerified,
            DgiiTaxpayerStatus: dgiiStatus,
            IsWhatsAppVerified: isWhatsAppVerified,
            ModeratedListingsCount: moderatedCount,
            HasRecentFraudReports: hasFraud,
            FraudReportCount90Days: fraudCount,
            IsOklaVerified: isVerified,
            Summary: summary);
    }

    // ── Criterion 3: Active moderated listings ─────────────────────────────

    private async Task<int> CountActiveListingsAsync(Guid dealerId, CancellationToken ct)
    {
        try
        {
            var stats = await _vehiclesClient.GetSellerListingStatsAsync(dealerId);
            return stats?.ActiveListings ?? 0;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to count active listings for dealer {DealerId}", dealerId);
            return 0;
        }
    }

    // ── Criterion 4: Fraud reports (90 days) ───────────────────────────────

    private async Task<(bool HasFraud, int Count)> CheckFraudReportsAsync(
        Guid dealerId, CancellationToken ct)
    {
        try
        {
            var response = await _reportsClient.GetAsync(
                $"/api/contentreports/vehicle/{dealerId}/count",
                ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "ReportsService returned {StatusCode} for dealer {DealerId} fraud check",
                    response.StatusCode, dealerId);
                return (false, 0);
            }

            var result = await response.Content
                .ReadFromJsonAsync<FraudCountDto>(cancellationToken: ct);
            var count = result?.Count ?? 0;
            return (count > 0, count);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to check fraud reports for dealer {DealerId}", dealerId);
            return (false, 0);
        }
    }

    private record FraudCountDto(int Count);
}
