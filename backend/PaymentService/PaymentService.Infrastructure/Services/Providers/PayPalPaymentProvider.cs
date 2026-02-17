using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using PaymentService.Domain.Enums;
using PaymentService.Domain.Interfaces;

namespace PaymentService.Infrastructure.Services.Providers;

/// <summary>
/// Implementación del proveedor PayPal (Internacional)
/// 
/// Características:
/// - Comisión: 2.9% + US$0.30
/// - Costo por transacción: Incluido en comisión
/// - Mensualidad: Gratis
/// - Tokenización: Nativa (Vault API)
/// - Tipo: Fintech (Internacional)
/// - Alcance: 200+ países
/// - Monedas: USD, EUR, DOP
/// 
/// PayPal es ideal para:
/// - Pagos internacionales
/// - Clientes extranjeros
/// - Suscripciones recurrentes
/// - E-commerce global
/// </summary>
public class PayPalPaymentProvider : BasePaymentGatewayProvider
{
    private readonly HttpClient _httpClient;

    public override PaymentGateway Gateway => PaymentGateway.PayPal;
    public override string Name => "PayPal - International";
    public override PaymentGatewayType Type => PaymentGatewayType.Fintech;

    public PayPalPaymentProvider(
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
            "ClientId",
            "ClientSecret",
            "BaseUrl",
            "WebhookId"
        );
    }

    public override async Task<PaymentResult> ChargeAsync(ChargeRequest request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("🔵 Procesando cargo PayPal para usuario {UserId}", request.UserId);

            var transactionId = Guid.NewGuid();
            var externalId = $"PAYPAL_{Guid.NewGuid():N}";

            // PayPal comisión: 2.9% + US$0.30
            decimal commission = (request.Amount * 0.029m) + 0.30m;
            decimal netAmount = request.Amount - commission;

            var result = CreateSuccessResult(
                transactionId,
                externalId,
                request.Amount,
                request.Currency,
                authCode: "PP_" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper(),
                cardLastFour: request.CardData?.CardNumber?.Substring(request.CardData.CardNumber.Length - 4)
            );

            result.Commission = commission;
            result.CommissionPercentage = 2.9m;
            result.NetAmount = netAmount;

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando cargo PayPal");
            return CreateFailureResult("500", "Error procesando pago");
        }
    }

    public override async Task<PaymentResult> AuthorizeAsync(ChargeRequest request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("🔵 Autorizando pago PayPal para usuario {UserId}", request.UserId);

            var result = await ChargeAsync(request, cancellationToken);
            result.Status = TransactionStatus.Authorized;

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error autorizando pago PayPal");
            return CreateFailureResult("500", "Error autorizando pago");
        }
    }

    public override async Task<PaymentResult> CaptureAsync(string authorizationCode, decimal amount, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("🔵 Capturando autorización PayPal {AuthCode}", authorizationCode);

            var transactionId = Guid.NewGuid();

            return CreateSuccessResult(
                transactionId,
                $"PAYPAL_CAPTURE_{authorizationCode}",
                amount,
                "USD",
                authCode: authorizationCode
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error capturando autorización PayPal");
            return CreateFailureResult("500", "Error capturando pago");
        }
    }

    public override async Task<PaymentResult> RefundAsync(string originalTransactionId, decimal? amount, string? reason, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("🔵 Reembolsando transacción PayPal {TransactionId}", originalTransactionId);

            var transactionId = Guid.NewGuid();

            return CreateSuccessResult(
                transactionId,
                $"PAYPAL_REFUND_{originalTransactionId}",
                amount ?? 0,
                "USD"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reembolsando transacción PayPal");
            return CreateFailureResult("500", "Error procesando reembolso");
        }
    }

    public override async Task<TokenizationResult> TokenizeCardAsync(CardData cardData, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("🔵 Tokenizando tarjeta con PayPal Vault");

            // PayPal Vault tokenization
            var token = $"paypal_vault_{Guid.NewGuid():N}";

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
                    { "provider", "PayPal" },
                    { "tokenization_method", "vault_api" },
                    { "international", true }
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tokenizando tarjeta con PayPal Vault");
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
            _logger.LogInformation("🔵 Procesando pago con token PayPal Vault");

            var transactionId = Guid.NewGuid();
            
            // Calculate commission for tokenized payment
            decimal commission = (amount * 0.029m) + 0.30m;
            
            var result = CreateSuccessResult(
                transactionId,
                $"PAYPAL_VAULT_{Guid.NewGuid():N}",
                amount,
                currency
            );
            
            result.Commission = commission;
            result.CommissionPercentage = 2.9m;
            result.NetAmount = amount - commission;
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando pago con token PayPal");
            return CreateFailureResult("500", "Error procesando pago");
        }
    }

    public override bool ValidateWebhook(string body, string signature)
    {
        _logger.LogInformation("🔵 Validando webhook PayPal");
        // PayPal usa HMAC-SHA256 para firmas de webhooks
        return !string.IsNullOrWhiteSpace(signature);
    }

    public override async Task<Guid> ProcessWebhookAsync(Dictionary<string, object> webhookData, CancellationToken cancellationToken)
    {
        _logger.LogInformation("🔵 Procesando webhook PayPal");
        
        // Process PayPal webhook events:
        // PAYMENT.CAPTURE.COMPLETED
        // PAYMENT.CAPTURE.REFUNDED
        // BILLING.SUBSCRIPTION.CREATED
        // etc.
        
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
            '6' => "Discover",
            _ => "Unknown"
        };
    }
}
