using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StripePaymentService.Application.DTOs;
using StripePaymentService.Application.Features.PaymentIntent.Commands;
using StripePaymentService.Application.Features.PaymentIntent.Queries;

namespace StripePaymentService.Api.Controllers;

/// <summary>
/// Controlador para Payment Intents
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentIntentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PaymentIntentsController> _logger;

    public PaymentIntentsController(IMediator mediator, ILogger<PaymentIntentsController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Crear un nuevo Payment Intent
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<PaymentIntentResponseDto>> CreatePaymentIntent(
        [FromBody] CreatePaymentIntentRequestDto request,
        CancellationToken cancellationToken)
    {
        _logger.Information("POST /paymentintents - Amount: {Amount}", request.Amount);

        try
        {
            var command = new CreatePaymentIntentCommand(request);
            var result = await _mediator.Send(command, cancellationToken);

            return CreatedAtAction(nameof(GetPaymentIntent), new { id = result.PaymentIntentId }, result);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error creando Payment Intent");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtener un Payment Intent por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<PaymentIntentResponseDto>> GetPaymentIntent(
        Guid id,
        CancellationToken cancellationToken)
    {
        _logger.Information("GET /paymentintents/{Id}", id);

        try
        {
            var query = new GetPaymentIntentQuery(id);
            var result = await _mediator.Send(query, cancellationToken);

            if (result == null)
                return NotFound();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error obteniendo Payment Intent: {Id}", id);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Health check
    /// </summary>
    [AllowAnonymous]
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "PaymentIntents Service is running" });
    }
}
