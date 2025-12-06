using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;
using UserService.Infrastructure.Persistence;

namespace UserService.Api.Controllers;

/// <summary>
/// Controller para onboarding y registro de dealers
/// Integra con BillingService para crear Customer de Stripe automáticamente
/// </summary>
[ApiController]
[Route("api/dealers")]
public class DealerOnboardingController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<DealerOnboardingController> _logger;
    private readonly IConfiguration _configuration;

    public DealerOnboardingController(
        ApplicationDbContext context,
        IHttpClientFactory httpClientFactory,
        ILogger<DealerOnboardingController> logger,
        IConfiguration configuration)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Registra un nuevo dealer y crea su Customer de Stripe
    /// Este endpoint es llamado después del registro de usuario
    /// </summary>
    [HttpPost("register")]
    [Authorize]
    public async Task<IActionResult> RegisterDealer([FromBody] RegisterDealerRequest request)
    {
        try
        {
            // 1. Verificar que el dealer no exista ya
            var existingSubscription = await _context.DealerSubscriptions
                .FirstOrDefaultAsync(s => s.DealerId == request.DealerId);

            if (existingSubscription != null)
            {
                _logger.LogWarning("Dealer {DealerId} already has a subscription", request.DealerId);
                return Conflict(new { error = "Dealer already registered" });
            }

            // 2. Crear la suscripción local con plan Free
            var subscription = new DealerSubscription
            {
                Id = Guid.NewGuid(),
                DealerId = request.DealerId,
                Plan = DealerPlan.Free,
                Status = SubscriptionStatus.Active,
                StartDate = DateTime.UtcNow,
                EndDate = null, // Free no expira
                CurrentListings = 0,
                ListingsThisMonth = 0,
                FeaturedUsed = 0,
                CreatedAt = DateTime.UtcNow
            };

            _context.DealerSubscriptions.Add(subscription);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created local subscription for dealer {DealerId}", request.DealerId);

            // 3. Intentar crear Customer en Stripe (async, no bloquea si falla)
            string? stripeCustomerId = null;
            try
            {
                stripeCustomerId = await CreateStripeCustomerAsync(request);

                if (!string.IsNullOrEmpty(stripeCustomerId))
                {
                    subscription.StripeCustomerId = stripeCustomerId;
                    await _context.SaveChangesAsync();
                    _logger.LogInformation(
                        "Created Stripe customer {StripeCustomerId} for dealer {DealerId}",
                        stripeCustomerId, request.DealerId);
                }
            }
            catch (Exception ex)
            {
                // No fallar el registro si Stripe falla - se puede crear después
                _logger.LogWarning(ex,
                    "Failed to create Stripe customer for dealer {DealerId}, will retry later",
                    request.DealerId);
            }

            return Ok(new RegisterDealerResponse(
                subscription.Id,
                request.DealerId,
                subscription.Plan.ToString(),
                subscription.Status.ToString(),
                stripeCustomerId,
                subscription.CreatedAt
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering dealer {DealerId}", request.DealerId);
            return StatusCode(500, new { error = "Failed to register dealer" });
        }
    }

    /// <summary>
    /// Obtiene la suscripción de un dealer
    /// </summary>
    [HttpGet("{dealerId:guid}/subscription")]
    [Authorize]
    public async Task<IActionResult> GetDealerSubscription(Guid dealerId)
    {
        var subscription = await _context.DealerSubscriptions
            .FirstOrDefaultAsync(s => s.DealerId == dealerId);

        if (subscription == null)
        {
            return NotFound(new { error = $"Subscription not found for dealer {dealerId}" });
        }

        var features = DealerPlanLimits.GetFeatures(subscription.Plan);

        return Ok(new DealerSubscriptionResponse(
            subscription.Id,
            subscription.DealerId,
            subscription.Plan.ToString(),
            subscription.Status.ToString(),
            subscription.StripeCustomerId,
            subscription.StripeSubscriptionId,
            subscription.StartDate,
            subscription.EndDate,
            subscription.TrialEndDate,
            new DealerLimitsResponse(
                features.MaxListings,
                features.MaxImages,
                features.FeaturedListings,
                features.MaxInvoicesPerMonth
            ),
            new DealerFeaturesResponse(
                features.CRMAdvanced,
                features.InvoicingCFDI,
                features.FinanceAdvanced,
                features.MarketingAutomation,
                features.WhatsappIntegration,
                features.AdvancedReports
            ),
            subscription.CreatedAt
        ));
    }

    /// <summary>
    /// Actualiza el StripeCustomerId de un dealer (llamado por webhook o sync)
    /// </summary>
    [HttpPatch("{dealerId:guid}/stripe-customer")]
    [Authorize]
    public async Task<IActionResult> UpdateStripeCustomer(
        Guid dealerId,
        [FromBody] UpdateStripeCustomerRequest request)
    {
        var subscription = await _context.DealerSubscriptions
            .FirstOrDefaultAsync(s => s.DealerId == dealerId);

        if (subscription == null)
        {
            return NotFound(new { error = $"Subscription not found for dealer {dealerId}" });
        }

        subscription.StripeCustomerId = request.StripeCustomerId;
        subscription.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Updated Stripe customer {StripeCustomerId} for dealer {DealerId}",
            request.StripeCustomerId, dealerId);

        return Ok(new { success = true });
    }

    /// <summary>
    /// Actualiza la suscripción de Stripe de un dealer (llamado por webhook)
    /// </summary>
    [HttpPatch("{dealerId:guid}/stripe-subscription")]
    [Authorize]
    public async Task<IActionResult> UpdateStripeSubscription(
        Guid dealerId,
        [FromBody] UpdateStripeSubscriptionRequest request)
    {
        var subscription = await _context.DealerSubscriptions
            .FirstOrDefaultAsync(s => s.DealerId == dealerId);

        if (subscription == null)
        {
            return NotFound(new { error = $"Subscription not found for dealer {dealerId}" });
        }

        subscription.StripeSubscriptionId = request.StripeSubscriptionId;

        // Mapear plan de Stripe a local
        subscription.Plan = MapStripePlanToLocal(request.Plan);
        subscription.Status = MapStripeStatusToLocal(request.Status);

        if (request.TrialEndDate.HasValue)
        {
            subscription.TrialEndDate = request.TrialEndDate;
        }

        if (request.CurrentPeriodEnd.HasValue)
        {
            subscription.EndDate = request.CurrentPeriodEnd;
        }

        subscription.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Updated subscription for dealer {DealerId}: Plan={Plan}, Status={Status}",
            dealerId, subscription.Plan, subscription.Status);

        return Ok(new { success = true });
    }

    /// <summary>
    /// Sincroniza manualmente con Stripe (para dealers que no tienen StripeCustomerId)
    /// </summary>
    [HttpPost("{dealerId:guid}/sync-stripe")]
    [Authorize]
    public async Task<IActionResult> SyncWithStripe(Guid dealerId, [FromBody] SyncStripeRequest request)
    {
        var subscription = await _context.DealerSubscriptions
            .FirstOrDefaultAsync(s => s.DealerId == dealerId);

        if (subscription == null)
        {
            return NotFound(new { error = $"Subscription not found for dealer {dealerId}" });
        }

        if (!string.IsNullOrEmpty(subscription.StripeCustomerId))
        {
            return Ok(new
            {
                message = "Dealer already has Stripe customer",
                stripeCustomerId = subscription.StripeCustomerId
            });
        }

        try
        {
            var registerRequest = new RegisterDealerRequest(
                dealerId,
                request.Email,
                request.BusinessName,
                request.Phone
            );

            var stripeCustomerId = await CreateStripeCustomerAsync(registerRequest);

            if (string.IsNullOrEmpty(stripeCustomerId))
            {
                return StatusCode(502, new { error = "Failed to create Stripe customer" });
            }

            subscription.StripeCustomerId = stripeCustomerId;
            subscription.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                stripeCustomerId = stripeCustomerId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sync dealer {DealerId} with Stripe", dealerId);
            return StatusCode(500, new { error = "Failed to sync with Stripe" });
        }
    }

    // ========================================
    // PRIVATE METHODS
    // ========================================

    private async Task<string?> CreateStripeCustomerAsync(RegisterDealerRequest request)
    {
        var billingServiceUrl = _configuration["Services:BillingService"]
            ?? "http://localhost:5070";

        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(billingServiceUrl);

        // Propagar token de autorización si existe
        if (Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            client.DefaultRequestHeaders.Add("Authorization", authHeader.ToString());
        }

        var createRequest = new
        {
            DealerId = request.DealerId,
            Email = request.Email,
            Name = request.BusinessName,
            Phone = request.Phone,
            Metadata = new Dictionary<string, string>
            {
                { "dealer_id", request.DealerId.ToString() },
                { "source", "user-service-onboarding" }
            }
        };

        var response = await client.PostAsJsonAsync("/api/billing/customers", createRequest);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning(
                "BillingService returned {StatusCode}: {Error}",
                response.StatusCode, error);
            return null;
        }

        var result = await response.Content.ReadFromJsonAsync<StripeCustomerResult>();
        return result?.StripeCustomerId;
    }

    private static DealerPlan MapStripePlanToLocal(string plan)
    {
        return plan.ToLowerInvariant() switch
        {
            "free" => DealerPlan.Free,
            "basic" => DealerPlan.Basic,
            "professional" or "pro" => DealerPlan.Pro,
            "enterprise" => DealerPlan.Enterprise,
            _ => DealerPlan.Free
        };
    }

    private static SubscriptionStatus MapStripeStatusToLocal(string status)
    {
        return status.ToLowerInvariant() switch
        {
            "active" => SubscriptionStatus.Active,
            "trialing" => SubscriptionStatus.Trial,
            "canceled" or "cancelled" => SubscriptionStatus.Canceled,
            "past_due" or "unpaid" => SubscriptionStatus.Active, // Mantener activo pero marcar
            _ => SubscriptionStatus.Active
        };
    }
}

// ========================================
// REQUEST/RESPONSE DTOs
// ========================================

public record RegisterDealerRequest(
    Guid DealerId,
    string Email,
    string BusinessName,
    string? Phone = null
);

public record RegisterDealerResponse(
    Guid SubscriptionId,
    Guid DealerId,
    string Plan,
    string Status,
    string? StripeCustomerId,
    DateTime CreatedAt
);

public record DealerSubscriptionResponse(
    Guid Id,
    Guid DealerId,
    string Plan,
    string Status,
    string? StripeCustomerId,
    string? StripeSubscriptionId,
    DateTime StartDate,
    DateTime? EndDate,
    DateTime? TrialEndDate,
    DealerLimitsResponse Limits,
    DealerFeaturesResponse Features,
    DateTime CreatedAt
);

public record DealerLimitsResponse(
    int MaxListings,
    int MaxPhotosPerListing,
    int MaxFeaturedListings,
    int MonthlyListingLimit
);

public record DealerFeaturesResponse(
    bool CRMAdvanced,
    bool InvoicingCFDI,
    bool FinanceAdvanced,
    bool MarketingAutomation,
    bool WhatsappIntegration,
    bool AdvancedReports
);

public record UpdateStripeCustomerRequest(string StripeCustomerId);

public record UpdateStripeSubscriptionRequest(
    string StripeSubscriptionId,
    string Plan,
    string Status,
    DateTime? TrialEndDate = null,
    DateTime? CurrentPeriodEnd = null
);

public record SyncStripeRequest(
    string Email,
    string BusinessName,
    string? Phone = null
);

// Internal DTOs for BillingService response
internal record StripeCustomerResult(
    Guid Id,
    Guid DealerId,
    string StripeCustomerId,
    string Email,
    string Name
);
