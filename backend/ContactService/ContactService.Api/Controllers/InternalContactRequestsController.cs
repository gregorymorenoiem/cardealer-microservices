using ContactService.Application.Features.ContactRequests.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ContactService.Api.Controllers;

/// <summary>
/// Internal API endpoints for inter-service communication.
/// These endpoints do NOT require JWT authentication (service-to-service calls).
/// Secured by network policy (only reachable within Docker overlay network / k8s cluster).
/// </summary>
[ApiController]
[Route("api/internal/contact-requests")]
public class InternalContactRequestsController : ControllerBase
{
    private readonly IMediator _mediator;

    public InternalContactRequestsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Returns the lead (contact request) count for a seller within a date range.
    /// Used by NotificationService's upsell worker to detect LIBRE-plan dealers with 5+ leads.
    /// </summary>
    /// <param name="sellerId">The seller/dealer user ID</param>
    /// <param name="from">Start of the period (inclusive, ISO 8601)</param>
    /// <param name="to">End of the period (inclusive, ISO 8601)</param>
    [HttpGet("seller/{sellerId:guid}/count")]
    public async Task<IActionResult> GetLeadCountForSeller(
        Guid sellerId,
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        CancellationToken cancellationToken)
    {
        if (from >= to)
            return BadRequest(new { error = "Parameter 'from' must be earlier than 'to'." });

        var count = await _mediator.Send(new GetLeadCountBySellerInPeriodQuery
        {
            SellerId = sellerId,
            From = from,
            To = to
        }, cancellationToken);

        return Ok(new { sellerId, from, to, count });
    }

    /// <summary>
    /// Checks if a buyer has ever contacted a specific seller.
    /// Used by ReviewService to verify that only buyers who had real contact
    /// with a dealer/seller can leave a review — prevents fake reviews.
    /// </summary>
    [HttpGet("buyer/{buyerId:guid}/has-contacted/{sellerId:guid}")]
    public async Task<IActionResult> HasBuyerContactedSeller(
        Guid buyerId,
        Guid sellerId,
        CancellationToken cancellationToken)
    {
        var hasContacted = await _mediator.Send(new HasBuyerContactedSellerQuery
        {
            BuyerId = buyerId,
            SellerId = sellerId
        }, cancellationToken);

        return Ok(new { buyerId, sellerId, hasContacted });
    }
}
