using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AzulPaymentService.Application.DTOs;
using AzulPaymentService.Application.Features.Charge.Commands;
using AzulPaymentService.Application.Features.Refund.Commands;
using AzulPaymentService.Application.Features.Transaction.Queries;

namespace AzulPaymentService.Api.Controllers;

/// <summary>
/// Controller para operaciones de pago
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(IMediator mediator, ILogger<PaymentsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Procesa un pago/cobro
    /// </summary>
    /// <param name="request">Datos del cobro</param>
    /// <response code="200">Cobro procesado exitosamente</response>
    /// <response code="400">Datos inválidos</response>
    /// <response code="401">No autenticado</response>
    /// <response code="500">Error del servidor</response>
    [HttpPost("charge")]
    [ProducesResponseType(typeof(ChargeResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ProcessCharge(
        [FromBody] ChargeRequestDto request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Procesando cobro para usuario {UserId}", request.UserId);

        try
        {
            var command = new ChargeCommand(request);
            var result = await _mediator.Send(command, cancellationToken);

            return Ok(result);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validación fallida: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar cobro");
            return StatusCode(500, new { message = "Error procesando cobro" });
        }
    }

    /// <summary>
    /// Obtiene detalles de una transacción
    /// </summary>
    /// <param name="transactionId">ID de la transacción</param>
    /// <response code="200">Transacción encontrada</response>
    /// <response code="404">Transacción no encontrada</response>
    [HttpGet("{transactionId}")]
    [ProducesResponseType(typeof(ChargeResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTransaction(
        Guid transactionId,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetTransactionByIdQuery(transactionId);
            var result = await _mediator.Send(query, cancellationToken);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Transacción no encontrada: {Id}", transactionId);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener transacción");
            return StatusCode(500, new { message = "Error obteniendo transacción" });
        }
    }

    /// <summary>
    /// Procesa un reembolso
    /// </summary>
    /// <param name="request">Datos del reembolso</param>
    /// <response code="200">Reembolso procesado</response>
    /// <response code="400">Datos inválidos</response>
    [HttpPost("refund")]
    [ProducesResponseType(typeof(ChargeResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ProcessRefund(
        [FromBody] RefundRequestDto request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Procesando reembolso para transacción {TransactionId}", request.TransactionId);

        try
        {
            var command = new RefundCommand(request);
            var result = await _mediator.Send(command, cancellationToken);

            return Ok(result);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validación fallida: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Error de operación: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar reembolso");
            return StatusCode(500, new { message = "Error procesando reembolso" });
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
