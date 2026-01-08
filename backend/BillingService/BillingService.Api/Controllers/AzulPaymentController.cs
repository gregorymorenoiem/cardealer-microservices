using BillingService.Application.DTOs.Azul;
using BillingService.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace BillingService.Api.Controllers;

[ApiController]
[Route("api/payment/azul")]
public class AzulPaymentController : ControllerBase
{
    private readonly IAzulPaymentService _azulService;
    private readonly ILogger<AzulPaymentController> _logger;

    public AzulPaymentController(
        IAzulPaymentService azulService,
        ILogger<AzulPaymentController> logger)
    {
        _azulService = azulService;
        _logger = logger;
    }

    [HttpPost("initiate")]
    public IActionResult InitiatePayment([FromBody] InitiateAzulPaymentRequest request)
    {
        try
        {
            var paymentRequest = _azulService.CreatePaymentRequest(
                request.Amount,
                request.ITBIS,
                request.OrderNumber);

            var paymentPageUrl = _azulService.GetPaymentPageUrl();
            var formFields = _azulService.GetPaymentFormFields(paymentRequest);

            return Ok(new
            {
                PaymentPageUrl = paymentPageUrl,
                FormFields = formFields
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error iniciando pago AZUL para orden {OrderNumber}", request.OrderNumber);
            return StatusCode(500, new { Error = "Error al iniciar el pago" });
        }
    }
}

public record InitiateAzulPaymentRequest(
    decimal Amount,
    decimal ITBIS,
    string OrderNumber);
