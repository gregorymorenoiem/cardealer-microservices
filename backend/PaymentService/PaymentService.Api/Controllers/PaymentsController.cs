using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using PaymentService.Application.DTOs;
using CarDealer.Shared.Idempotency.Attributes;
using PaymentService.Application.Features.Charge.Commands;
using PaymentService.Application.Features.Refund.Commands;
using PaymentService.Application.Features.Transaction.Queries;
using PaymentService.Domain.Interfaces;
using PaymentService.Domain.Enums;

namespace PaymentService.Api.Controllers;

/// <summary>
/// Controller para operaciones de pago multi-proveedor
/// Soporta: AZUL, CardNET, PixelPay, Fygaro, PayPal
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IPaymentGatewayRegistry _gatewayRegistry;
    private readonly IGatewayAvailabilityService _availability;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(
        IMediator mediator, 
        IPaymentGatewayRegistry gatewayRegistry,
        IGatewayAvailabilityService availability,
        ILogger<PaymentsController> logger)
    {
        _mediator = mediator;
        _gatewayRegistry = gatewayRegistry;
        _availability = availability;
        _logger = logger;
    }

    // ==================== ENDPOINTS DE PROVEEDORES ====================

    /// <summary>
    /// Lista todos los proveedores de pago disponibles
    /// </summary>
    /// <response code="200">Lista de proveedores</response>
    [HttpGet("providers")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ProvidersListDto), StatusCodes.Status200OK)]
    public IActionResult GetProviders()
    {
        var providers = _gatewayRegistry.GetAll();
        
        var result = new ProvidersListDto
        {
            TotalProviders = providers.Count,
            Providers = providers.Select(p => new ProviderInfoDto
            {
                Gateway = p.Gateway.ToString(),
                Name = p.Name,
                Type = p.Type.ToString(),
                IsConfigured = p.ValidateConfiguration().Count == 0,
                ConfigurationErrors = p.ValidateConfiguration()
            }).ToList()
        };
        
        return Ok(result);
    }

    /// <summary>
    /// Lista solo los proveedores habilitados para nuevos usuarios (checkout).
    /// Respeta los toggles del admin panel (billing.{provider}_enabled).
    /// Existing users with saved methods on disabled gateways can still be charged.
    /// </summary>
    [HttpGet("providers/available")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ProvidersListDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailableProviders(CancellationToken cancellationToken)
    {
        var enabledGateways = await _availability.GetEnabledGatewaysAsync(cancellationToken);
        var providers = _gatewayRegistry.GetAll()
            .Where(p => enabledGateways.Contains(p.Gateway))
            .ToList();

        var result = new ProvidersListDto
        {
            TotalProviders = providers.Count,
            Providers = providers.Select(p => new ProviderInfoDto
            {
                Gateway = p.Gateway.ToString(),
                Name = p.Name,
                Type = p.Type.ToString(),
                IsConfigured = p.ValidateConfiguration().Count == 0,
                ConfigurationErrors = new List<string>() // Don't expose config errors publicly
            }).ToList()
        };

        return Ok(result);
    }

    /// <summary>
    /// Obtiene información de un proveedor específico
    /// </summary>
    /// <param name="gateway">Nombre del proveedor (Azul, CardNET, PixelPay, Fygaro, PayPal)</param>
    [HttpGet("providers/{gateway}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ProviderInfoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetProvider(string gateway)
    {
        if (!Enum.TryParse<PaymentGateway>(gateway, true, out var gatewayEnum))
        {
            return NotFound(new { message = $"Proveedor '{gateway}' no existe" });
        }
        
        var provider = _gatewayRegistry.Get(gatewayEnum);
        if (provider == null)
        {
            return NotFound(new { message = $"Proveedor '{gateway}' no está registrado" });
        }
        
        return Ok(new ProviderInfoDto
        {
            Gateway = provider.Gateway.ToString(),
            Name = provider.Name,
            Type = provider.Type.ToString(),
            IsConfigured = provider.ValidateConfiguration().Count == 0,
            ConfigurationErrors = provider.ValidateConfiguration()
        });
    }

    /// <summary>
    /// Verifica disponibilidad de un proveedor
    /// </summary>
    [HttpGet("providers/{gateway}/health")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ProviderHealthDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CheckProviderHealth(string gateway, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<PaymentGateway>(gateway, true, out var gatewayEnum))
        {
            return NotFound(new { message = $"Proveedor '{gateway}' no existe" });
        }
        
        var provider = _gatewayRegistry.Get(gatewayEnum);
        if (provider == null)
        {
            return NotFound(new { message = $"Proveedor '{gateway}' no está registrado" });
        }
        
        var isAvailable = await provider.IsAvailableAsync(cancellationToken);
        
        return Ok(new ProviderHealthDto
        {
            Gateway = provider.Gateway.ToString(),
            Name = provider.Name,
            IsAvailable = isAvailable,
            CheckedAt = DateTime.UtcNow
        });
    }

    // ==================== ENDPOINTS DE PAGOS ====================

    /// <summary>
    /// Procesa un pago/cobro
    /// </summary>
    /// <param name="request">Datos del cobro</param>
    /// <response code="200">Cobro procesado exitosamente</response>
    /// <response code="400">Datos inválidos</response>
    /// <response code="401">No autenticado</response>
    /// <response code="500">Error del servidor</response>
    [HttpPost("charge")]
    [Idempotent]
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
    [Idempotent]
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
    /// Health check del servicio de pagos multi-proveedor
    /// </summary>
    [HttpGet("health")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AllProvidersHealthDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> HealthCheck(CancellationToken cancellationToken)
    {
        var providers = _gatewayRegistry.GetAll();
        var providerHealths = new List<ProviderHealthDto>();
        
        foreach (var provider in providers)
        {
            var configErrors = provider.ValidateConfiguration();
            var isConfigured = configErrors.Count == 0;
            var isAvailable = false;
            string status = "unavailable";
            string? message = null;
            long? latencyMs = null;
            
            if (isConfigured)
            {
                try
                {
                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    isAvailable = await provider.IsAvailableAsync(cancellationToken);
                    sw.Stop();
                    latencyMs = sw.ElapsedMilliseconds;
                    status = isAvailable ? "available" : "unavailable";
                }
                catch (Exception ex)
                {
                    status = "error";
                    message = ex.Message;
                    _logger.LogWarning(ex, "Error checking health for provider {Provider}", provider.Gateway);
                }
            }
            else
            {
                status = "not_configured";
                message = $"{configErrors.Count} configuration error(s)";
            }
            
            providerHealths.Add(new ProviderHealthDto
            {
                Gateway = provider.Gateway.ToString(),
                Name = provider.Name,
                IsAvailable = isAvailable,
                IsConfigured = isConfigured,
                Status = status,
                Message = message,
                LatencyMs = latencyMs,
                CheckedAt = DateTime.UtcNow,
                ConfigurationErrors = configErrors
            });
        }
        
        var availableCount = providerHealths.Count(p => p.IsAvailable);
        var unavailableCount = providerHealths.Count - availableCount;
        
        var result = new AllProvidersHealthDto
        {
            OverallStatus = availableCount > 0 ? "healthy" : "degraded",
            Service = "PaymentService",
            TotalProviders = providers.Count,
            AvailableProviders = availableCount,
            UnavailableProviders = unavailableCount,
            DefaultGateway = "Azul", // TODO: Get from config
            Providers = providerHealths,
            CheckedAt = DateTime.UtcNow
        };
        
        return Ok(result);
    }
}
