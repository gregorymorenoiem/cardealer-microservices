using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using PaymentService.Infrastructure.Services;
using PaymentService.Domain.Interfaces;
using PaymentService.Domain.Enums;

namespace PaymentService.Api.Controllers;

/// <summary>
/// Controller para procesar webhooks de todos los proveedores de pago
/// Soporta: AZUL, CardNET, PixelPay, Fygaro, PayPal
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class WebhooksController : ControllerBase
{
    private readonly AzulWebhookValidationService _webhookValidationService;
    private readonly IPaymentGatewayRegistry _gatewayRegistry;
    private readonly ILogger<WebhooksController> _logger;

    public WebhooksController(
        AzulWebhookValidationService webhookValidationService,
        IPaymentGatewayRegistry gatewayRegistry,
        ILogger<WebhooksController> logger)
    {
        _webhookValidationService = webhookValidationService;
        _gatewayRegistry = gatewayRegistry;
        _logger = logger;
    }

    // ==================== WEBHOOK AZUL ====================

    /// <summary>
    /// Recibe eventos de webhook desde AZUL
    /// </summary>
    [HttpPost("azul")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> HandleAzulWebhook(
        [FromHeader(Name = "X-Signature")] string? signature,
        [FromBody] string rawPayload,
        CancellationToken cancellationToken)
    {
        return await ProcessProviderWebhook(PaymentGateway.Azul, rawPayload, signature, cancellationToken);
    }

    // ==================== WEBHOOK CARDNET ====================

    /// <summary>
    /// Recibe eventos de webhook desde CardNET
    /// </summary>
    [HttpPost("cardnet")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> HandleCardNETWebhook(
        [FromHeader(Name = "X-CardNET-Signature")] string? signature,
        [FromBody] string rawPayload,
        CancellationToken cancellationToken)
    {
        return await ProcessProviderWebhook(PaymentGateway.CardNET, rawPayload, signature, cancellationToken);
    }

    // ==================== WEBHOOK PIXELPAY ====================

    /// <summary>
    /// Recibe eventos de webhook desde PixelPay
    /// </summary>
    [HttpPost("pixelpay")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> HandlePixelPayWebhook(
        [FromHeader(Name = "X-PixelPay-Signature")] string? signature,
        [FromBody] string rawPayload,
        CancellationToken cancellationToken)
    {
        return await ProcessProviderWebhook(PaymentGateway.PixelPay, rawPayload, signature, cancellationToken);
    }

    // ==================== WEBHOOK FYGARO ====================

    /// <summary>
    /// Recibe eventos de webhook desde Fygaro
    /// </summary>
    [HttpPost("fygaro")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> HandleFygaroWebhook(
        [FromHeader(Name = "X-Fygaro-Signature")] string? signature,
        [FromBody] string rawPayload,
        CancellationToken cancellationToken)
    {
        return await ProcessProviderWebhook(PaymentGateway.Fygaro, rawPayload, signature, cancellationToken);
    }

    // ==================== WEBHOOK PAYPAL ====================

    /// <summary>
    /// Recibe eventos de webhook desde PayPal
    /// </summary>
    [HttpPost("paypal")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> HandlePayPalWebhook(
        [FromHeader(Name = "PAYPAL-TRANSMISSION-SIG")] string? signature,
        [FromBody] string rawPayload,
        CancellationToken cancellationToken)
    {
        return await ProcessProviderWebhook(PaymentGateway.PayPal, rawPayload, signature, cancellationToken);
    }

    // ==================== LEGACY ENDPOINT (backward compatibility) ====================

    /// <summary>
    /// Recibe eventos de webhook desde AZUL (legacy endpoint)
    /// </summary>
    [HttpPost("event")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> HandleWebhookEvent(
        [FromHeader(Name = "X-Signature")] string? signature,
        [FromBody] string rawPayload,
        CancellationToken cancellationToken)
    {
        // Legacy: redirigir a AZUL por defecto
        return await HandleAzulWebhook(signature, rawPayload, cancellationToken);
    }

    // ==================== PROCESAMIENTO GENÉRICO ====================

    /// <summary>
    /// Procesa un webhook de cualquier proveedor
    /// </summary>
    private async Task<IActionResult> ProcessProviderWebhook(
        PaymentGateway gateway,
        string rawPayload,
        string? signature,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("📩 Webhook recibido de {Gateway}", gateway);

        try
        {
            // Obtener el proveedor
            var provider = _gatewayRegistry.Get(gateway);
            if (provider == null)
            {
                _logger.LogWarning("Proveedor {Gateway} no está registrado", gateway);
                return BadRequest(new { 
                    message = $"Provider {gateway} is not registered",
                    gateway = gateway.ToString()
                });
            }

            // Validar firma
            if (string.IsNullOrEmpty(signature))
            {
                _logger.LogWarning("Webhook de {Gateway} sin firma", gateway);
                return BadRequest(new { 
                    message = "Missing signature header",
                    gateway = gateway.ToString()
                });
            }

            if (!provider.ValidateWebhook(rawPayload, signature))
            {
                _logger.LogWarning("Firma de webhook de {Gateway} inválida", gateway);
                return BadRequest(new { 
                    message = "Invalid webhook signature",
                    gateway = gateway.ToString()
                });
            }

            // Parsear datos del webhook
            var webhookData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(rawPayload)
                ?? new Dictionary<string, object>();

            // Procesar el webhook a través del proveedor
            var transactionId = await provider.ProcessWebhookAsync(webhookData, cancellationToken);

            _logger.LogInformation(
                "✅ Webhook de {Gateway} procesado correctamente. TransactionId: {TransactionId}", 
                gateway, transactionId);

            return Ok(new { 
                message = "Webhook processed successfully",
                gateway = gateway.ToString(),
                transactionId = transactionId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error procesando webhook de {Gateway}", gateway);
            return StatusCode(500, new { 
                message = "Error processing webhook",
                gateway = gateway.ToString(),
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Health check del servicio de webhooks
    /// </summary>
    [HttpGet("health")]
    public IActionResult HealthCheck()
    {
        var providers = _gatewayRegistry.GetAll();
        return Ok(new { 
            status = "healthy", 
            service = "PaymentService.Webhooks",
            supportedGateways = providers.Select(p => p.Gateway.ToString()).ToList(),
            endpoints = new[]
            {
                "/api/webhooks/azul",
                "/api/webhooks/cardnet",
                "/api/webhooks/pixelpay",
                "/api/webhooks/fygaro",
                "/api/webhooks/paypal",
                "/api/webhooks/event (legacy)"
            }
        });
    }
}
