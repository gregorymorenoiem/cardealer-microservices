using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using AzulPaymentService.Infrastructure.Services;

namespace AzulPaymentService.Api.Controllers;

/// <summary>
/// Controller para procesar webhooks de AZUL
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class WebhooksController : ControllerBase
{
    private readonly AzulWebhookValidationService _webhookValidationService;
    private readonly ILogger<WebhooksController> _logger;

    public WebhooksController(
        AzulWebhookValidationService webhookValidationService,
        ILogger<WebhooksController> logger)
    {
        _webhookValidationService = webhookValidationService;
        _logger = logger;
    }

    /// <summary>
    /// Recibe eventos de webhook desde AZUL
    /// </summary>
    /// <remarks>
    /// AZUL envía webhooks para eventos como:
    /// - transaction.approved
    /// - transaction.declined
    /// - subscription.charged
    /// - subscription.failed
    /// - subscription.cancelled
    /// </remarks>
    /// <response code="200">Webhook procesado correctamente</response>
    /// <response code="400">Firma inválida o datos inválidos</response>
    /// <response code="500">Error del servidor</response>
    [HttpPost("event")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> HandleWebhookEvent(
        [FromHeader(Name = "X-Signature")] string? signature,
        [FromBody] string rawPayload,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Webhook recibido de AZUL");

        try
        {
            if (string.IsNullOrEmpty(signature))
            {
                _logger.LogWarning("Webhook sin firma");
                return BadRequest(new { message = "Missing X-Signature header" });
            }

            // Validar firma
            if (!_webhookValidationService.ValidateWebhookSignature(rawPayload, signature))
            {
                _logger.LogWarning("Firma de webhook inválida");
                return BadRequest(new { message = "Invalid signature" });
            }

            // Extraer datos del webhook
            var webhookData = _webhookValidationService.ExtractWebhookData(rawPayload);

            // TODO: Procesar el evento de webhook
            // Guardar en base de datos
            // Publicar evento de dominio
            // Ejecutar lógica de negocio

            _logger.LogInformation("Webhook procesado correctamente");

            return Ok(new { message = "Webhook received" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando webhook");
            return StatusCode(500, new { message = "Error processing webhook" });
        }
    }

    /// <summary>
    /// Health check del servicio
    /// </summary>
    [HttpGet("health")]
    public IActionResult HealthCheck()
    {
        return Ok(new { status = "healthy", service = "AzulPaymentService" });
    }
}
