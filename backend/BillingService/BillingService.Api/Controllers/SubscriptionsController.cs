using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BillingService.Application.DTOs;
using BillingService.Domain.Entities;
using BillingService.Domain.Interfaces;

namespace BillingService.Api.Controllers;

// Plan enforcement DTOs — used by other microservices
public record PlanEnforcementResponse(
    bool CanPublish,
    int CurrentCount,
    int MaxAllowed,
    int Remaining,
    string Plan,
    string Message
);

public record PlanFeaturesResponse(
    string Plan,
    int MaxVehicles,
    int MaxUsers,
    int MaxPhotosPerListing,
    int FeaturedListingsPerMonth,
    int OklaCoinsMonthly,
    int ChatAgentConversations,
    bool VideoTourEnabled,
    bool VerifiedBadge,
    bool WhatsAppIntegration
);

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubscriptionsController : BillingBaseController
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly ILogger<SubscriptionsController> _logger;

    public SubscriptionsController(
        ISubscriptionRepository subscriptionRepository,
        ILogger<SubscriptionsController> logger)
    {
        _subscriptionRepository = subscriptionRepository;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<IEnumerable<SubscriptionDto>>> GetAll(CancellationToken cancellationToken)
    {
        var subscriptions = await _subscriptionRepository.GetAllAsync(cancellationToken);
        return Ok(subscriptions.Select(MapToDto));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SubscriptionDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var subscription = await _subscriptionRepository.GetByIdAsync(id, cancellationToken);
        if (subscription == null)
            return NotFound();

        return Ok(MapToDto(subscription));
    }

    [HttpGet("dealer/{dealerId:guid}")]
    public async Task<ActionResult<SubscriptionDto>> GetByDealerId(Guid dealerId, CancellationToken cancellationToken)
    {
        dealerId = GetDealerIdOrOverride(dealerId);
        var subscription = await _subscriptionRepository.GetByDealerIdAsync(dealerId, cancellationToken);
        if (subscription == null)
            return NotFound();

        return Ok(MapToDto(subscription));
    }

    [HttpGet("status/{status}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<IEnumerable<SubscriptionDto>>> GetByStatus(SubscriptionStatus status, CancellationToken cancellationToken)
    {
        var subscriptions = await _subscriptionRepository.GetByStatusAsync(status, cancellationToken);
        return Ok(subscriptions.Select(MapToDto));
    }

    [HttpGet("plan/{plan}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<IEnumerable<SubscriptionDto>>> GetByPlan(SubscriptionPlan plan, CancellationToken cancellationToken)
    {
        var subscriptions = await _subscriptionRepository.GetByPlanAsync(plan, cancellationToken);
        return Ok(subscriptions.Select(MapToDto));
    }

    [HttpGet("expiring-trials/{days:int}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<IEnumerable<SubscriptionDto>>> GetExpiringTrials(int days, CancellationToken cancellationToken)
    {
        var subscriptions = await _subscriptionRepository.GetExpiringTrialsAsync(days, cancellationToken);
        return Ok(subscriptions.Select(MapToDto));
    }

    [HttpGet("due-billings")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<IEnumerable<SubscriptionDto>>> GetDueBillings(CancellationToken cancellationToken)
    {
        var subscriptions = await _subscriptionRepository.GetDueBillingsAsync(cancellationToken);
        return Ok(subscriptions.Select(MapToDto));
    }

    [HttpPost]
    public async Task<ActionResult<SubscriptionDto>> Create(
        [FromBody] CreateSubscriptionRequest request,
        CancellationToken cancellationToken)
    {
        var dealerId = GetDealerIdFromJwt();
        if (!Enum.TryParse<SubscriptionPlan>(request.Plan, true, out var plan))
            return BadRequest("Invalid subscription plan");

        if (!Enum.TryParse<BillingCycle>(request.Cycle, true, out var cycle))
            return BadRequest("Invalid billing cycle");

        // Check if dealer already has a subscription
        var existing = await _subscriptionRepository.GetByDealerIdAsync(dealerId, cancellationToken);
        if (existing != null)
            return Conflict("Dealer already has an active subscription");

        var subscription = new Subscription(
            dealerId,
            plan,
            cycle,
            request.PricePerCycle,
            request.MaxUsers,
            request.MaxVehicles,
            request.TrialDays);

        if (!string.IsNullOrEmpty(request.Features))
            subscription.SetFeatures(request.Features);

        var created = await _subscriptionRepository.AddAsync(subscription, cancellationToken);
        _logger.LogInformation("Subscription {SubscriptionId} created for dealer {DealerId}", created.Id, dealerId);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToDto(created));
    }

    [HttpPost("{id:guid}/activate")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<SubscriptionDto>> Activate(Guid id, CancellationToken cancellationToken)
    {
        var subscription = await _subscriptionRepository.GetByIdAsync(id, cancellationToken);
        if (subscription == null)
            return NotFound();

        subscription.Activate();
        await _subscriptionRepository.UpdateAsync(subscription, cancellationToken);
        _logger.LogInformation("Subscription {SubscriptionId} activated", id);

        return Ok(MapToDto(subscription));
    }

    [HttpPost("{id:guid}/suspend")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<SubscriptionDto>> Suspend(Guid id, CancellationToken cancellationToken)
    {
        var subscription = await _subscriptionRepository.GetByIdAsync(id, cancellationToken);
        if (subscription == null)
            return NotFound();

        subscription.Suspend();
        await _subscriptionRepository.UpdateAsync(subscription, cancellationToken);
        _logger.LogInformation("Subscription {SubscriptionId} suspended", id);

        return Ok(MapToDto(subscription));
    }

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<SubscriptionDto>> Cancel(Guid id, [FromBody] string reason, CancellationToken cancellationToken)
    {
        var subscription = await _subscriptionRepository.GetByIdAsync(id, cancellationToken);
        if (subscription == null)
            return NotFound();

        subscription.Cancel(reason);
        await _subscriptionRepository.UpdateAsync(subscription, cancellationToken);
        _logger.LogInformation("Subscription {SubscriptionId} cancelled", id);

        return Ok(MapToDto(subscription));
    }

    [HttpPost("{id:guid}/upgrade")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<SubscriptionDto>> Upgrade(Guid id, [FromBody] UpgradeSubscriptionRequest request, CancellationToken cancellationToken)
    {
        var subscription = await _subscriptionRepository.GetByIdAsync(id, cancellationToken);
        if (subscription == null)
            return NotFound();

        if (!Enum.TryParse<SubscriptionPlan>(request.Plan, true, out var newPlan))
            return BadRequest("Invalid subscription plan");

        subscription.Upgrade(newPlan, request.NewPrice, request.NewMaxUsers, request.NewMaxVehicles);
        await _subscriptionRepository.UpdateAsync(subscription, cancellationToken);
        _logger.LogInformation("Subscription {SubscriptionId} upgraded to {Plan}", id, request.Plan);

        return Ok(MapToDto(subscription));
    }

    [HttpPost("{id:guid}/change-billing-cycle")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<SubscriptionDto>> ChangeBillingCycle(Guid id, [FromBody] ChangeBillingCycleRequest request, CancellationToken cancellationToken)
    {
        var subscription = await _subscriptionRepository.GetByIdAsync(id, cancellationToken);
        if (subscription == null)
            return NotFound();

        if (!Enum.TryParse<BillingCycle>(request.Cycle, true, out var newCycle))
            return BadRequest("Invalid billing cycle");

        subscription.ChangeBillingCycle(newCycle, request.NewPrice);
        await _subscriptionRepository.UpdateAsync(subscription, cancellationToken);
        _logger.LogInformation("Subscription {SubscriptionId} billing cycle changed to {Cycle}", id, request.Cycle);

        return Ok(MapToDto(subscription));
    }

    [HttpPost("{id:guid}/renew")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<SubscriptionDto>> Renew(Guid id, CancellationToken cancellationToken)
    {
        var subscription = await _subscriptionRepository.GetByIdAsync(id, cancellationToken);
        if (subscription == null)
            return NotFound();

        subscription.RenewBilling();
        await _subscriptionRepository.UpdateAsync(subscription, cancellationToken);
        _logger.LogInformation("Subscription {SubscriptionId} renewed", id);

        return Ok(MapToDto(subscription));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var exists = await _subscriptionRepository.ExistsAsync(id, cancellationToken);
        if (!exists)
            return NotFound();

        await _subscriptionRepository.DeleteAsync(id, cancellationToken);
        _logger.LogInformation("Subscription {SubscriptionId} deleted", id);

        return NoContent();
    }

    // ========================================
    // PLAN ENFORCEMENT API
    // Used by VehiclesSaleService to check if a dealer can publish more vehicles
    // ========================================

    /// <summary>
    /// Checks if a dealer can perform an action based on their current plan limits.
    /// Called by other microservices before allowing resource creation.
    /// </summary>
    [HttpGet("dealer/{dealerId:guid}/can-publish")]
    [AllowAnonymous] // Internal service-to-service call (protected by network policy)
    public async Task<ActionResult<PlanEnforcementResponse>> CanPublish(
        Guid dealerId,
        [FromQuery] int currentVehicleCount,
        CancellationToken cancellationToken)
    {
        var subscription = await _subscriptionRepository.GetByDealerIdAsync(dealerId, cancellationToken);

        // OKLA Plan Table: ALL plans have ∞ UNLIMITED vehicle listings
        // The differentiation is via premium features (photos, search priority, ChatAgent, etc.)
        var maxVehicles = -1; // All plans = unlimited
        var planName = "Free";

        if (subscription != null && subscription.Status == SubscriptionStatus.Active)
        {
            maxVehicles = subscription.MaxVehicles; // Should be -1 for all plans
            planName = subscription.Plan.ToString();
        }

        // -1 = unlimited — with the OKLA Freemium model, everyone can publish
        var canPublish = maxVehicles == -1 || currentVehicleCount < maxVehicles;
        var remaining = maxVehicles == -1 ? -1 : Math.Max(0, maxVehicles - currentVehicleCount);

        return Ok(new PlanEnforcementResponse(
            CanPublish: canPublish,
            CurrentCount: currentVehicleCount,
            MaxAllowed: maxVehicles,
            Remaining: remaining,
            Plan: planName,
            Message: canPublish
                ? "Puedes publicar. Las publicaciones son ilimitadas en todos los planes OKLA."
                : $"Has alcanzado el límite de {maxVehicles} publicaciones de tu plan {planName}. Contacta soporte."
        ));
    }

    /// <summary>
    /// Returns the plan features for a dealer (used by other services for feature gating).
    /// </summary>
    [HttpGet("dealer/{dealerId:guid}/plan-features")]
    [AllowAnonymous] // Internal service-to-service call
    public async Task<ActionResult<PlanFeaturesResponse>> GetPlanFeatures(
        Guid dealerId,
        CancellationToken cancellationToken)
    {
        var subscription = await _subscriptionRepository.GetByDealerIdAsync(dealerId, cancellationToken);

        if (subscription == null || subscription.Status != SubscriptionStatus.Active)
        {
            return Ok(new PlanFeaturesResponse(
                Plan: "Free",
                MaxVehicles: -1, // ALL plans have unlimited listings
                MaxUsers: 1,
                MaxPhotosPerListing: 10,
                FeaturedListingsPerMonth: 0,
                OklaCoinsMonthly: 0,
                ChatAgentConversations: 0,
                VideoTourEnabled: false,
                VerifiedBadge: false,
                WhatsAppIntegration: false
            ));
        }

        var limits = StripePriceMapping.GetLimits(subscription.Plan);
        return Ok(new PlanFeaturesResponse(
            Plan: subscription.Plan.ToString(),
            MaxVehicles: limits.MaxVehicles,
            MaxUsers: limits.MaxUsers,
            MaxPhotosPerListing: subscription.Plan switch
            {
                SubscriptionPlan.Free => 10,
                SubscriptionPlan.Basic => 20,
                SubscriptionPlan.Professional => 30,
                SubscriptionPlan.Enterprise => 40,
                _ => 10
            },
            FeaturedListingsPerMonth: subscription.Plan switch
            {
                SubscriptionPlan.Free => 0,
                SubscriptionPlan.Basic => 3,
                SubscriptionPlan.Professional => 10,
                SubscriptionPlan.Enterprise => 25,
                _ => 0
            },
            OklaCoinsMonthly: subscription.Plan switch
            {
                SubscriptionPlan.Free => 0,
                SubscriptionPlan.Basic => 15,
                SubscriptionPlan.Professional => 45,
                SubscriptionPlan.Enterprise => 120,
                _ => 0
            },
            ChatAgentConversations: subscription.Plan switch
            {
                SubscriptionPlan.Free => 0,
                SubscriptionPlan.Basic => 0,
                SubscriptionPlan.Professional => 500,
                SubscriptionPlan.Enterprise => -1,
                _ => 0
            },
            VideoTourEnabled: subscription.Plan == SubscriptionPlan.Enterprise,
            VerifiedBadge: subscription.Plan >= SubscriptionPlan.Basic,
            WhatsAppIntegration: subscription.Plan >= SubscriptionPlan.Professional
        ));
    }

    private static SubscriptionDto MapToDto(Subscription subscription) => new(
        subscription.Id,
        subscription.DealerId,
        subscription.Plan.ToString(),
        subscription.Status.ToString(),
        subscription.Cycle.ToString(),
        subscription.PricePerCycle,
        subscription.Currency,
        subscription.StartDate,
        subscription.EndDate,
        subscription.TrialEndDate,
        subscription.NextBillingDate,
        subscription.StripeCustomerId,
        subscription.StripeSubscriptionId,
        subscription.MaxUsers,
        subscription.MaxVehicles,
        subscription.Features,
        subscription.CreatedAt
    );
}
