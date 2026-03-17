using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BillingService.Domain.Entities;
using BillingService.Infrastructure.Persistence;

namespace BillingService.Api.Controllers;

/// <summary>
/// Manages which payment gateways a dealer has enabled.
/// Dealers can toggle on/off the gateways they want to use for subscription payments.
/// </summary>
[ApiController]
[Route("api/billing/dealers")]
[Authorize]
public class DealerPaymentSettingsController : BillingBaseController
{
    // All platform gateways in display order
    private static readonly GatewayInfo[] AllGateways =
    [
        new("Azul",     "Azul (Banco Popular)",              "Tarjetas dominicanas — Visa, Mastercard, Discover",        true,  true),
        new("CardNET",  "CardNET",                           "Red de pagos Visa/MasterCard local",                       false, true),
        new("PixelPay", "PixelPay",                          "Fintech regional — Centroamérica y Caribe",                false, true),
        new("Fygaro",   "Fygaro",                            "Crédito y débito local dominicano",                        false, true),
        new("PayPal",   "PayPal",                            "Tarjetas internacionales y cuenta PayPal",                 true,  false),
        new("Stripe",   "Stripe",                            "Apple Pay, Google Pay y tarjetas internacionales",         false, false),
    ];

    private readonly BillingDbContext _db;
    private readonly ILogger<DealerPaymentSettingsController> _logger;

    public DealerPaymentSettingsController(BillingDbContext db, ILogger<DealerPaymentSettingsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/billing/dealers/me/payment-gateways
    /// Returns all gateways with enabled/disabled status for the authenticated dealer.
    /// If no preferences saved, all gateways are enabled by default.
    /// </summary>
    [HttpGet("me/payment-gateways")]
    public async Task<IActionResult> GetMyGateways(CancellationToken ct)
    {
        var dealerId = GetDealerIdFromJwt();
        var prefs = await _db.DealerGatewayPreferences
            .FirstOrDefaultAsync(p => p.DealerId == dealerId, ct);

        var enabledIds = prefs?.GetEnabledGateways();
        // null/empty means "all enabled" (default)
        var allEnabled = enabledIds == null || enabledIds.Count == 0;

        var result = AllGateways.Select(g => new GatewayStatusDto(
            Id: g.Id,
            Name: g.Name,
            Description: g.Description,
            Recommended: g.Recommended,
            IsLocal: g.IsLocal,
            IsEnabled: allEnabled || enabledIds!.Contains(g.Id)
        )).ToList();

        return Ok(new { gateways = result });
    }

    /// <summary>
    /// PUT /api/billing/dealers/me/payment-gateways
    /// Updates which gateways the dealer has enabled.
    /// Body: { "enabledGatewayIds": ["Azul", "PayPal"] }
    /// </summary>
    [HttpPut("me/payment-gateways")]
    public async Task<IActionResult> UpdateMyGateways(
        [FromBody] UpdateGatewaySettingsRequest request,
        CancellationToken ct)
    {
        if (request.EnabledGatewayIds is null || request.EnabledGatewayIds.Length == 0)
            return BadRequest(new { error = "At least one payment gateway must be enabled." });

        // Validate all IDs are known gateways
        var validIds = AllGateways.Select(g => g.Id).ToHashSet();
        var invalid = request.EnabledGatewayIds.Where(id => !validIds.Contains(id)).ToList();
        if (invalid.Count > 0)
            return BadRequest(new { error = $"Unknown gateway IDs: {string.Join(", ", invalid)}" });

        var dealerId = GetDealerIdFromJwt();
        var prefs = await _db.DealerGatewayPreferences
            .FirstOrDefaultAsync(p => p.DealerId == dealerId, ct);

        if (prefs is null)
        {
            prefs = new DealerGatewayPreferences(dealerId, request.EnabledGatewayIds);
            _db.DealerGatewayPreferences.Add(prefs);
        }
        else
        {
            prefs.Update(request.EnabledGatewayIds);
        }

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation(
            "Dealer {DealerId} updated gateway preferences: {Gateways}",
            dealerId, string.Join(",", request.EnabledGatewayIds));

        return Ok(new { message = "Pasarelas actualizadas correctamente." });
    }
}

// ─── DTOs ────────────────────────────────────────────────────────────────────

public record GatewayInfo(
    string Id,
    string Name,
    string Description,
    bool Recommended,
    bool IsLocal
);

public record GatewayStatusDto(
    string Id,
    string Name,
    string Description,
    bool Recommended,
    bool IsLocal,
    bool IsEnabled
);

public record UpdateGatewaySettingsRequest(
    string[] EnabledGatewayIds
);
