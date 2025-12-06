using Microsoft.AspNetCore.Mvc;
using BillingService.Application.DTOs;
using BillingService.Domain.Entities;
using BillingService.Domain.Interfaces;

namespace BillingService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubscriptionsController : ControllerBase
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
        var subscription = await _subscriptionRepository.GetByDealerIdAsync(dealerId, cancellationToken);
        if (subscription == null)
            return NotFound();

        return Ok(MapToDto(subscription));
    }

    [HttpGet("status/{status}")]
    public async Task<ActionResult<IEnumerable<SubscriptionDto>>> GetByStatus(SubscriptionStatus status, CancellationToken cancellationToken)
    {
        var subscriptions = await _subscriptionRepository.GetByStatusAsync(status, cancellationToken);
        return Ok(subscriptions.Select(MapToDto));
    }

    [HttpGet("plan/{plan}")]
    public async Task<ActionResult<IEnumerable<SubscriptionDto>>> GetByPlan(SubscriptionPlan plan, CancellationToken cancellationToken)
    {
        var subscriptions = await _subscriptionRepository.GetByPlanAsync(plan, cancellationToken);
        return Ok(subscriptions.Select(MapToDto));
    }

    [HttpGet("expiring-trials/{days:int}")]
    public async Task<ActionResult<IEnumerable<SubscriptionDto>>> GetExpiringTrials(int days, CancellationToken cancellationToken)
    {
        var subscriptions = await _subscriptionRepository.GetExpiringTrialsAsync(days, cancellationToken);
        return Ok(subscriptions.Select(MapToDto));
    }

    [HttpGet("due-billings")]
    public async Task<ActionResult<IEnumerable<SubscriptionDto>>> GetDueBillings(CancellationToken cancellationToken)
    {
        var subscriptions = await _subscriptionRepository.GetDueBillingsAsync(cancellationToken);
        return Ok(subscriptions.Select(MapToDto));
    }

    [HttpPost]
    public async Task<ActionResult<SubscriptionDto>> Create(
        [FromBody] CreateSubscriptionRequest request,
        [FromHeader(Name = "X-Dealer-Id")] Guid dealerId,
        CancellationToken cancellationToken)
    {
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
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var exists = await _subscriptionRepository.ExistsAsync(id, cancellationToken);
        if (!exists)
            return NotFound();

        await _subscriptionRepository.DeleteAsync(id, cancellationToken);
        _logger.LogInformation("Subscription {SubscriptionId} deleted", id);

        return NoContent();
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
