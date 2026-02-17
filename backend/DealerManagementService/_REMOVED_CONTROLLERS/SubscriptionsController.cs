using DealerManagementService.Application.DTOs;
using DealerManagementService.Domain.Entities;
using DealerManagementService.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DealerManagementService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubscriptionsController : ControllerBase
{
    private readonly IDealerRepository _dealerRepository;
    private readonly ILogger<SubscriptionsController> _logger;

    public SubscriptionsController(
        IDealerRepository dealerRepository,
        ILogger<SubscriptionsController> logger)
    {
        _dealerRepository = dealerRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get all available plans with features and pricing
    /// </summary>
    [HttpGet("plans")]
    [ProducesResponseType(typeof(AllPlansResponse), StatusCodes.Status200OK)]
    public ActionResult<AllPlansResponse> GetAllPlans([FromQuery] Guid? dealerId = null)
    {
        var currentPlan = "Free";
        
        var plans = new List<PlanInfoDto>
        {
            new PlanInfoDto(
                Plan: "Free",
                DisplayName: "Gratis",
                MonthlyPrice: 0,
                AnnualPrice: null,
                Description: "Perfecto para comenzar a vender",
                Features: new List<string>
                {
                    "Hasta 3 publicaciones activas",
                    "5 imágenes por vehículo",
                    "Perfil básico de dealer",
                    "Contacto directo con compradores"
                },
                NotIncluded: new List<string>
                {
                    "Analytics",
                    "Carga masiva",
                    "Publicaciones destacadas",
                    "CRM de leads",
                    "Soporte prioritario"
                },
                IsPopular: false,
                MaxListings: 3,
                MaxImages: 5,
                MaxFeaturedListings: 0
            ),
            new PlanInfoDto(
                Plan: "Basic",
                DisplayName: "Básico",
                MonthlyPrice: 49,
                AnnualPrice: 470, // 20% discount
                Description: "Ideal para dealers pequeños",
                Features: new List<string>
                {
                    "Hasta 50 publicaciones activas",
                    "10 imágenes por vehículo",
                    "3 publicaciones destacadas",
                    "Dashboard de analytics básico",
                    "Carga masiva CSV/Excel",
                    "Gestión de leads básica",
                    "Perfil público personalizado"
                },
                NotIncluded: new List<string>
                {
                    "Soporte prioritario",
                    "Branding personalizado",
                    "Email automation",
                    "Análisis de precios de mercado"
                },
                IsPopular: false,
                MaxListings: 50,
                MaxImages: 10,
                MaxFeaturedListings: 3
            ),
            new PlanInfoDto(
                Plan: "Pro",
                DisplayName: "Profesional",
                MonthlyPrice: 129,
                AnnualPrice: 1238, // 20% discount
                Description: "Para dealers en crecimiento",
                Features: new List<string>
                {
                    "Hasta 200 publicaciones activas",
                    "20 imágenes por vehículo",
                    "10 publicaciones destacadas",
                    "Analytics avanzados",
                    "Carga masiva CSV/Excel",
                    "CRM completo de leads",
                    "Email automation",
                    "Análisis de precios de mercado",
                    "Reportes avanzados",
                    "Branding personalizado",
                    "Soporte prioritario"
                },
                NotIncluded: new List<string>
                {
                    "API access",
                    "White label",
                    "Account manager dedicado"
                },
                IsPopular: true,
                MaxListings: 200,
                MaxImages: 20,
                MaxFeaturedListings: 10
            ),
            new PlanInfoDto(
                Plan: "Enterprise",
                DisplayName: "Empresarial",
                MonthlyPrice: 299,
                AnnualPrice: 2870, // 20% discount
                Description: "Para grandes concesionarios",
                Features: new List<string>
                {
                    "Publicaciones ILIMITADAS",
                    "30 imágenes por vehículo",
                    "Destacados ILIMITADOS",
                    "Todas las features incluidas",
                    "API access completo",
                    "White label",
                    "Account manager dedicado",
                    "Integraciones personalizadas",
                    "Reportes ejecutivos",
                    "Capacitación del equipo"
                },
                NotIncluded: new List<string>(),
                IsPopular: false,
                MaxListings: 999999,
                MaxImages: 30,
                MaxFeaturedListings: 999999
            )
        };

        return Ok(new AllPlansResponse(
            Plans: plans,
            CurrentPlan: currentPlan,
            RecommendedPlan: "Pro"
        ));
    }

    /// <summary>
    /// Get dealer subscription details with limits and usage
    /// </summary>
    [HttpGet("dealer/{dealerId:guid}")]
    [ProducesResponseType(typeof(DealerSubscriptionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DealerSubscriptionDto>> GetDealerSubscription(Guid dealerId)
    {
        var dealer = await _dealerRepository.GetByIdAsync(dealerId);
        
        if (dealer == null)
        {
            return NotFound(new { message = $"Dealer with ID {dealerId} not found" });
        }

        var subscription = MapToSubscriptionDto(dealer);
        return Ok(subscription);
    }

    /// <summary>
    /// Get subscription by user ID
    /// </summary>
    [HttpGet("user/{userId:guid}")]
    [ProducesResponseType(typeof(DealerSubscriptionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DealerSubscriptionDto>> GetSubscriptionByUserId(Guid userId)
    {
        var dealer = await _dealerRepository.GetByUserIdAsync(userId);
        
        if (dealer == null)
        {
            return NotFound(new { message = $"No dealer found for user {userId}" });
        }

        var subscription = MapToSubscriptionDto(dealer);
        return Ok(subscription);
    }

    /// <summary>
    /// Change dealer plan (upgrade/downgrade)
    /// </summary>
    [HttpPost("dealer/{dealerId:guid}/change-plan")]
    [Authorize]
    [ProducesResponseType(typeof(ChangePlanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ChangePlanResponse>> ChangePlan(
        Guid dealerId,
        [FromBody] ChangePlanRequest request)
    {
        var dealer = await _dealerRepository.GetByIdAsync(dealerId);
        
        if (dealer == null)
        {
            return NotFound(new { message = $"Dealer with ID {dealerId} not found" });
        }

        // Parse new plan
        if (!Enum.TryParse<DealerPlan>(request.NewPlan, true, out var newPlan))
        {
            return BadRequest(new { message = $"Invalid plan: {request.NewPlan}. Valid plans: Free, Basic, Pro, Enterprise" });
        }

        var previousPlan = dealer.CurrentPlan.ToString();
        var planFeatures = DealerPlanLimits.GetFeatures(newPlan);

        // Check if downgrade would exceed new limits
        if (dealer.CurrentActiveListings > planFeatures.MaxListings)
        {
            return BadRequest(new
            {
                message = $"No puedes cambiar al plan {request.NewPlan} porque tienes {dealer.CurrentActiveListings} publicaciones activas y el límite es {planFeatures.MaxListings}. Primero desactiva algunas publicaciones."
            });
        }

        // Update dealer plan
        dealer.CurrentPlan = newPlan;
        dealer.MaxActiveListings = planFeatures.MaxListings;
        dealer.IsSubscriptionActive = true;
        dealer.SubscriptionStartDate = DateTime.UtcNow;
        dealer.SubscriptionEndDate = newPlan == DealerPlan.Free 
            ? null 
            : DateTime.UtcNow.AddMonths(1);
        dealer.UpdatedAt = DateTime.UtcNow;

        await _dealerRepository.UpdateAsync(dealer);

        _logger.LogInformation(
            "Dealer {DealerId} changed plan from {PreviousPlan} to {NewPlan}",
            dealerId, previousPlan, newPlan);

        var subscription = MapToSubscriptionDto(dealer);

        return Ok(new ChangePlanResponse(
            Success: true,
            Message: $"Plan cambiado exitosamente a {DealerPlanLimits.GetPlanDisplayName(newPlan)}",
            PreviousPlan: previousPlan,
            NewPlan: newPlan.ToString(),
            Subscription: subscription
        ));
    }

    /// <summary>
    /// Check if dealer can perform an action based on limits
    /// </summary>
    [HttpGet("dealer/{dealerId:guid}/can-action")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> CanPerformAction(
        Guid dealerId,
        [FromQuery] string action)
    {
        var dealer = await _dealerRepository.GetByIdAsync(dealerId);
        
        if (dealer == null)
        {
            return NotFound(new { message = $"Dealer with ID {dealerId} not found" });
        }

        var features = DealerPlanLimits.GetFeatures(dealer.CurrentPlan);
        
        var result = action.ToLower() switch
        {
            "add_listing" => new
            {
                allowed = dealer.CurrentActiveListings < features.MaxListings,
                reason = dealer.CurrentActiveListings >= features.MaxListings
                    ? $"Has alcanzado el límite de {features.MaxListings} publicaciones para tu plan {dealer.CurrentPlan}"
                    : null,
                current = dealer.CurrentActiveListings,
                limit = features.MaxListings,
                remaining = Math.Max(0, features.MaxListings - dealer.CurrentActiveListings)
            },
            "add_featured" => new
            {
                allowed = features.MaxFeaturedListings > 0,
                reason = features.MaxFeaturedListings == 0
                    ? "Tu plan no incluye publicaciones destacadas. Actualiza a Básico o superior."
                    : null,
                current = 0, // TODO: Track featured usage
                limit = features.MaxFeaturedListings,
                remaining = features.MaxFeaturedListings
            },
            "bulk_upload" => new
            {
                allowed = features.BulkUpload,
                reason = !features.BulkUpload
                    ? "La carga masiva no está disponible en tu plan. Actualiza a Básico o superior."
                    : null,
                current = 0,
                limit = 0,
                remaining = 0
            },
            "analytics" => new
            {
                allowed = features.AnalyticsAccess,
                reason = !features.AnalyticsAccess
                    ? "Analytics no está disponible en tu plan. Actualiza a Básico o superior."
                    : null,
                current = 0,
                limit = 0,
                remaining = 0
            },
            _ => new
            {
                allowed = true,
                reason = (string?)null,
                current = 0,
                limit = 0,
                remaining = 0
            }
        };

        return Ok(result);
    }

    /// <summary>
    /// Increment listing count for a dealer
    /// </summary>
    [HttpPost("dealer/{dealerId:guid}/increment-listings")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> IncrementListings(Guid dealerId)
    {
        var dealer = await _dealerRepository.GetByIdAsync(dealerId);
        
        if (dealer == null)
        {
            return NotFound(new { message = $"Dealer with ID {dealerId} not found" });
        }

        if (!dealer.CanAddListing())
        {
            var features = DealerPlanLimits.GetFeatures(dealer.CurrentPlan);
            return BadRequest(new
            {
                message = $"Has alcanzado el límite de {features.MaxListings} publicaciones",
                limit = features.MaxListings,
                current = dealer.CurrentActiveListings,
                upgradeTo = GetNextPlan(dealer.CurrentPlan)
            });
        }

        dealer.CurrentActiveListings++;
        dealer.UpdatedAt = DateTime.UtcNow;
        await _dealerRepository.UpdateAsync(dealer);

        return Ok(new
        {
            currentListings = dealer.CurrentActiveListings,
            maxListings = dealer.MaxActiveListings,
            remaining = dealer.GetRemainingListings()
        });
    }

    /// <summary>
    /// Decrement listing count for a dealer
    /// </summary>
    [HttpPost("dealer/{dealerId:guid}/decrement-listings")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> DecrementListings(Guid dealerId)
    {
        var dealer = await _dealerRepository.GetByIdAsync(dealerId);
        
        if (dealer == null)
        {
            return NotFound(new { message = $"Dealer with ID {dealerId} not found" });
        }

        if (dealer.CurrentActiveListings > 0)
        {
            dealer.CurrentActiveListings--;
            dealer.UpdatedAt = DateTime.UtcNow;
            await _dealerRepository.UpdateAsync(dealer);
        }

        return Ok(new
        {
            currentListings = dealer.CurrentActiveListings,
            maxListings = dealer.MaxActiveListings,
            remaining = dealer.GetRemainingListings()
        });
    }

    // ============================================
    // Private Helper Methods
    // ============================================

    private DealerSubscriptionDto MapToSubscriptionDto(Dealer dealer)
    {
        var plan = dealer.CurrentPlan;
        var features = DealerPlanLimits.GetFeatures(plan);
        
        var daysRemaining = dealer.SubscriptionEndDate.HasValue
            ? Math.Max(0, (dealer.SubscriptionEndDate.Value - DateTime.UtcNow).Days)
            : -1; // -1 = unlimited (free plan or lifetime)

        var usage = new DealerUsageDto(
            CurrentListings: dealer.CurrentActiveListings,
            FeaturedListings: 0, // TODO: Track featured
            ImagesUsed: 0,       // TODO: Track images
            LeadsThisMonth: 0,   // TODO: Track leads
            EmailsSentThisMonth: 0 // TODO: Track emails
        );

        var limits = new DealerLimitsDto(
            MaxListings: features.MaxListings,
            MaxFeaturedListings: features.MaxFeaturedListings,
            MaxImages: features.MaxImages,
            MaxLeadsPerMonth: plan == DealerPlan.Enterprise ? 999999 : plan == DealerPlan.Pro ? 500 : plan == DealerPlan.Basic ? 100 : 10,
            MaxEmailsPerMonth: plan == DealerPlan.Enterprise ? 999999 : plan == DealerPlan.Pro ? 2000 : plan == DealerPlan.Basic ? 500 : 0,
            HasReachedListingLimit: dealer.CurrentActiveListings >= features.MaxListings,
            HasReachedFeaturedLimit: false, // TODO: Track
            RemainingListings: Math.Max(0, features.MaxListings - dealer.CurrentActiveListings),
            RemainingFeatured: features.MaxFeaturedListings,
            ListingsUsagePercent: features.MaxListings > 0 
                ? Math.Min(100, (double)dealer.CurrentActiveListings / features.MaxListings * 100) 
                : 0,
            FeaturedUsagePercent: 0 // TODO: Track
        );

        var featuresDto = new DealerPlanFeaturesDto(
            MaxListings: features.MaxListings,
            MaxImages: features.MaxImages,
            MaxFeaturedListings: features.MaxFeaturedListings,
            AnalyticsAccess: features.AnalyticsAccess,
            BulkUpload: features.BulkUpload,
            PrioritySupport: features.PrioritySupport,
            CustomBranding: features.CustomBranding,
            ApiAccess: features.ApiAccess,
            LeadManagement: features.LeadManagement,
            EmailAutomation: features.EmailAutomation,
            MarketPriceAnalysis: features.MarketPriceAnalysis,
            AdvancedReporting: features.AdvancedReporting,
            WhiteLabel: features.WhiteLabel,
            DedicatedAccountManager: features.DedicatedAccountManager
        );

        var nextPlan = GetNextPlan(plan);

        return new DealerSubscriptionDto(
            DealerId: dealer.Id,
            Plan: plan.ToString(),
            PlanDisplayName: DealerPlanLimits.GetPlanDisplayName(plan),
            MonthlyPrice: DealerPlanLimits.GetMonthlyPrice(plan),
            IsActive: dealer.IsSubscriptionActive,
            StartDate: dealer.SubscriptionStartDate,
            EndDate: dealer.SubscriptionEndDate,
            DaysRemaining: daysRemaining,
            Features: featuresDto,
            Usage: usage,
            Limits: limits,
            CanUpgrade: plan != DealerPlan.Enterprise,
            NextPlan: nextPlan
        );
    }

    private string? GetNextPlan(DealerPlan currentPlan)
    {
        return currentPlan switch
        {
            DealerPlan.Free => "Basic",
            DealerPlan.Basic => "Pro",
            DealerPlan.Pro => "Enterprise",
            DealerPlan.Enterprise => null,
            _ => "Basic"
        };
    }
}
