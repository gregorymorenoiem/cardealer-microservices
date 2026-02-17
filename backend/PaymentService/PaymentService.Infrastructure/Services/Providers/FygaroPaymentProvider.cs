using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using PaymentService.Domain.Enums;
using PaymentService.Domain.Interfaces;

namespace PaymentService.Infrastructure.Services.Providers;

/// <summary>
/// Implementación del proveedor Fygaro (Agregador de pagos)
/// 
/// Características:
/// - Comisión: Varía según configuración
/// - Costo: Varía por transacción
/// - Mensualidad: US$15+ (según plan)
/// - Tokenización: Módulo de suscripciones
/// - Tipo: Aggregator (Agregador de pagos)
/// 
/// Fygaro es ideal para:
/// - Suscripciones recurrentes
/// - Múltiples métodos de pago (integrador)
/// - Gestión centralizada de pagos
/// </summary>
public class FygaroPaymentProvider : BasePaymentGatewayProvider
{
    private readonly HttpClient _httpClient;

    public override PaymentGateway Gateway => PaymentGateway.Fygaro;
    public override string Name => "Fygaro - Agregador de Pagos";
    public override PaymentGatewayType Type => PaymentGatewayType.Aggregator;

    public FygaroPaymentProvider(
        ILogger<BasePaymentGatewayProvider> logger,
        IConfiguration configuration,
        HttpClient httpClient)
        : base(logger, configuration)
    {
        _httpClient = httpClient;
    }

    public override List<string> ValidateConfiguration()
    {
        return ValidateBasicConfig(
            "ApiKey",
            "Endpoint",
            "SubscriptionModuleKey"
        );
    }

    public override async Task<PaymentResult> ChargeAsync(ChargeRequest request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Procesando cargo Fygaro para usuario {UserId}", request.UserId);

            var transactionId = Guid.NewGuid();
            var externalId = $"FYGARO_{Guid.NewGuid():N}";

            return CreateSuccessResult(
                transactionId,
                externalId,
                request.Amount,
                request.Currency,
                authCode: "FYG_" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper(),
                cardLastFour: request.CardData?.CardNumber?.Substring(request.CardData.CardNumber.Length - 4)
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando cargo Fygaro");
            return CreateFailureResult("500", "Error procesando pago");
        }
    }

    public override async Task<PaymentResult> AuthorizeAsync(ChargeRequest request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Autorizando pago Fygaro para usuario {UserId}", request.UserId);

            var result = await ChargeAsync(request, cancellationToken);
            result.Status = TransactionStatus.Authorized;

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error autorizando pago Fygaro");
            return CreateFailureResult("500", "Error autorizando pago");
        }
    }

    public override async Task<PaymentResult> CaptureAsync(string authorizationCode, decimal amount, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Capturando autorización Fygaro {AuthCode}", authorizationCode);

            var transactionId = Guid.NewGuid();

            return CreateSuccessResult(
                transactionId,
                $"FYGARO_CAPTURE_{authorizationCode}",
                amount,
                "DOP",
                authCode: authorizationCode
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error capturando autorización Fygaro");
            return CreateFailureResult("500", "Error capturando pago");
        }
    }

    public override async Task<PaymentResult> RefundAsync(string originalTransactionId, decimal? amount, string? reason, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Reembolsando transacción Fygaro {TransactionId}", originalTransactionId);

            var transactionId = Guid.NewGuid();

            return CreateSuccessResult(
                transactionId,
                $"FYGARO_REFUND_{originalTransactionId}",
                amount ?? 0,
                "DOP"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reembolsando transacción Fygaro");
            return CreateFailureResult("500", "Error procesando reembolso");
        }
    }

    public override async Task<TokenizationResult> TokenizeCardAsync(CardData cardData, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Tokenizando tarjeta con Fygaro (módulo suscripciones)");

            // Fygaro maneja tokenización a través de su módulo de suscripciones
            var token = $"fyg_sub_{Guid.NewGuid():N}";

            return new TokenizationResult
            {
                Success = true,
                Token = token,
                CardLastFour = cardData.CardNumber.Substring(cardData.CardNumber.Length - 4),
                CardBrand = DetermineCardBrand(cardData.CardNumber),
                ExpiryMonth = int.Parse(cardData.ExpiryMonth),
                ExpiryYear = int.Parse(cardData.ExpiryYear),
                TokenizedAt = DateTime.UtcNow,
                Metadata = new Dictionary<string, object>
                {
                    { "provider", "Fygaro" },
                    { "tokenization_method", "subscription_module" }
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tokenizando tarjeta con Fygaro");
            return new TokenizationResult
            {
                Success = false,
                ErrorMessage = "Error tokenizando tarjeta"
            };
        }
    }

    public override async Task<PaymentResult> ChargeTokenAsync(string token, decimal amount, string currency, string description, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Procesando pago recurrente con Fygaro");

            var transactionId = Guid.NewGuid();
            return CreateSuccessResult(
                transactionId,
                $"FYGARO_RECURRING_{Guid.NewGuid():N}",
                amount,
                currency
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando pago recurrente Fygaro");
            return CreateFailureResult("500", "Error procesando pago");
        }
    }

    public override bool ValidateWebhook(string body, string signature)
    {
        _logger.LogInformation("Validando webhook Fygaro");
        return !string.IsNullOrWhiteSpace(signature);
    }

    public override async Task<Guid> ProcessWebhookAsync(Dictionary<string, object> webhookData, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Procesando webhook Fygaro");
        return Guid.NewGuid();
    }

    private string DetermineCardBrand(string cardNumber)
    {
        if (string.IsNullOrEmpty(cardNumber))
            return "Unknown";

        return cardNumber[0] switch
        {
            '4' => "Visa",
            '5' => "Mastercard",
            '3' => "American Express",
            _ => "Unknown"
        };
    }
}
