using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StripePaymentService.Infrastructure.Services;

namespace StripePaymentService.Api.Controllers;

/// <summary>
/// Controlador para Webhooks de Stripe
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class WebhooksController : ControllerBase
{
    private readonly StripeWebhookValidationService _webhookService;
    private readonly ILogger<WebhooksController> _logger;
    private readonly IConfiguration _configuration;

    public WebhooksController(
        StripeWebhookValidationService webhookService,
        ILogger<WebhooksController> logger,
        IConfiguration configuration)
    {
        _webhookService = webhookService ?? throw new ArgumentNullException(nameof(webhookService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <summary>
    /// Recibir eventos de webhook de Stripe
    /// </summary>
    [HttpPost("stripe")]
    public async Task<IActionResult> ReceiveStripeWebhook()
    {
        try
        {
            using var reader = new StreamReader(Request.Body);
            var payload = await reader.ReadToEndAsync();
            var signatureHeader = Request.Headers["Stripe-Signature"].ToString();
            var webhookSecret = _configuration["Stripe:WebhookSecret"] ?? "";

            _logger.LogInformation("Webhook recibido de Stripe");

            if (!_webhookService.ValidateWebhookSignature(payload, signatureHeader, webhookSecret))
            {
                _logger.LogWarning("Validación de webhook fallida");
                return Unauthorized();
            }

            // TODO: Procesar evento de webhook
            // - payment_intent.succeeded
            // - payment_intent.payment_failed
            // - customer.subscription.updated
            // - customer.subscription.deleted
            // - invoice.payment_succeeded
            // - invoice.payment_failed

            _logger.LogInformation("Webhook procesado exitosamente");
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando webhook");
            return StatusCode(500, new { error = "Error procesando webhook" });
        }
    }

    /// <summary>
    /// Health check
    /// </summary>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "Webhooks Service is running" });
    }
}
