using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using PaymentService.Application.DTOs;
using CarDealer.Shared.Idempotency.Attributes;
using PaymentService.Application.Features.Subscription.Commands;

namespace PaymentService.Api.Controllers;

/// <summary>
/// Controller para operaciones de suscripción recurrente
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
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Crea una suscripción recurrente
    /// </summary>
    /// <param name="request">Datos de la suscripción</param>
    /// <response code="201">Suscripción creada</response>
    /// <response code="400">Datos inválidos</response>
    /// <response code="401">No autenticado</response>
    [HttpPost]
    [Idempotent]
    [ProducesResponseType(typeof(SubscriptionResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateSubscription(
        [FromBody] SubscriptionRequestDto request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creando suscripción para usuario {UserId}", request.UserId);

        try
        {
            var command = new CreateSubscriptionCommand(request);
            var result = await _mediator.Send(command, cancellationToken);

            return CreatedAtAction(nameof(GetSubscription), new { subscriptionId = result.SubscriptionId }, result);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validación fallida: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear suscripción");
            return StatusCode(500, new { message = "Error creando suscripción" });
        }
    }

    /// <summary>
    /// Obtiene detalles de una suscripción
    /// </summary>
    /// <param name="subscriptionId">ID de la suscripción</param>
    /// <response code="200">Suscripción encontrada</response>
    /// <response code="404">Suscripción no encontrada</response>
    [HttpGet("{subscriptionId}")]
    [ProducesResponseType(typeof(SubscriptionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetSubscription(
        Guid subscriptionId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Obteniendo suscripción {SubscriptionId}", subscriptionId);

        try
        {
            // TODO: Implementar query
            return Ok(new { message = "Not yet implemented" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener suscripción");
            return StatusCode(500, new { message = "Error obteniendo suscripción" });
        }
    }

    /// <summary>
    /// Cancela una suscripción
    /// </summary>
    /// <param name="subscriptionId">ID de la suscripción</param>
    /// <param name="reason">Razón de la cancelación</param>
    /// <response code="200">Suscripción cancelada</response>
    /// <response code="404">Suscripción no encontrada</response>
    [HttpDelete("{subscriptionId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult CancelSubscription(
        Guid subscriptionId,
        [FromQuery] string? reason,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cancelando suscripción {SubscriptionId}", subscriptionId);

        try
        {
            // TODO: Implementar command
            return Ok(new { message = "Subscription cancelled" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cancelar suscripción");
            return StatusCode(500, new { message = "Error cancelando suscripción" });
        }
    }

    /// <summary>
    /// Health check del servicio
    /// </summary>
    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult HealthCheck()
    {
        return Ok(new { status = "healthy", service = "AzulPaymentService" });
    }
}
