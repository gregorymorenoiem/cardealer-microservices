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

    public WebhooksController(
        StripeWebhookValidationService webhookService,
        ILogger<WebhooksController> logger)
    {
        _webhookService = webhookService ?? throw new ArgumentNullException(nameof(webhookService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

            _logger.Information("Webhook recibido de Stripe");

            if (!_webhookService.ValidateWebhookSignature(payload, signatureHeader))
            {
                _logger.Warning("Validación de webhook fallida");
                return Unauthorized();
            }

            // TODO: Procesar evento de webhook
            // - payment_intent.succeeded
            // - payment_intent.payment_failed
            // - customer.subscription.updated
            // - customer.subscription.deleted
            // - invoice.payment_succeeded
            // - invoice.payment_failed

            _logger.Information("Webhook procesado exitosamente");
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error procesando webhook");
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
