using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using StripePaymentService.Application.DTOs;
using StripePaymentService.Application.Features.Subscription.Commands;
using StripePaymentService.Application.Features.Subscription.Queries;

namespace StripePaymentService.Api.Controllers;

/// <summary>
/// Controlador para Subscripciones
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubscriptionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<SubscriptionsController> _logger;

    public SubscriptionsController(IMediator mediator, ILogger<SubscriptionsController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Crear una nueva subscripción
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<SubscriptionResponseDto>> CreateSubscription(
        [FromBody] CreateSubscriptionRequestDto request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("POST /subscriptions - StripeCustomerId: {StripeCustomerId}", request.StripeCustomerId);

        try
        {
            var command = new CreateSubscriptionCommand(request);
            var result = await _mediator.Send(command, cancellationToken);

            return CreatedAtAction(nameof(GetSubscriptions), new { customerId = request.StripeCustomerId }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creando subscripción");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Listar subscripciones de un customer
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<SubscriptionResponseDto>>> GetSubscriptions(
        [FromQuery] Guid customerId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GET /subscriptions - CustomerId: {CustomerId}, Page: {Page}", customerId, page);

        try
        {
            var query = new ListSubscriptionsQuery(customerId, page, pageSize);
            var result = await _mediator.Send(query, cancellationToken);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listando subscripciones");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Cancelar una subscripción
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> CancelSubscription(
        Guid id,
        [FromQuery] string? reason,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("DELETE /subscriptions/{Id} - Reason: {Reason}", id, reason ?? "No especificada");

        try
        {
            var command = new CancelSubscriptionCommand(id, reason);
            var result = await _mediator.Send(command, cancellationToken);

            return Ok(new { success = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelando subscripción: {Id}", id);
            return BadRequest(new { error = ex.Message });
        }
    }
}
