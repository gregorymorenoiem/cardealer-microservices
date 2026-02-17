using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using PaymentService.Domain.Enums;
using PaymentService.Domain.Interfaces;

namespace PaymentService.Infrastructure.Services.Providers;

/// <summary>
/// Implementación del proveedor AZUL (Banco Popular RD)
/// 
/// Características:
/// - Comisión: 2.9% - 4.5%
/// - Costo fijo: RD$5 - 10 por transacción
/// - Mensualidad: US$30 - 50
/// - Tokenización: Incluida (Cybersource)
/// - Tipo: Banking (Bancaria tradicional)
/// </summary>
public class AzulPaymentProvider : BasePaymentGatewayProvider
{
    private readonly HttpClient _httpClient;

    public override PaymentGateway Gateway => PaymentGateway.Azul;
    public override string Name => "AZUL - Banco Popular RD";
    public override PaymentGatewayType Type => PaymentGatewayType.Banking;

    public AzulPaymentProvider(
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
            "MerchantId",
            "AuthKey",
            "CyberSourceSecretKey",
            "Endpoint"
        );
    }

    public override async Task<PaymentResult> ChargeAsync(ChargeRequest request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Procesando cargo AZUL para usuario {UserId}", request.UserId);

            // Aquí iría la llamada a la API de AZUL
            // Por ahora retorna un resultado simulado
            var transactionId = Guid.NewGuid();
            var externalId = $"AZUL_{DateTime.UtcNow.Ticks}";

            return CreateSuccessResult(
                transactionId,
                externalId,
                request.Amount,
                request.Currency,
                authCode: "123456",
                cardLastFour: request.CardData?.CardNumber?.Substring(request.CardData.CardNumber.Length - 4)
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando cargo AZUL");
            return CreateFailureResult("500", "Error procesando pago");
        }
    }

    public override async Task<PaymentResult> AuthorizeAsync(ChargeRequest request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Autorizando pago AZUL para usuario {UserId}", request.UserId);

            var transactionId = Guid.NewGuid();
            var result = await ChargeAsync(request, cancellationToken);
            result.Status = TransactionStatus.Authorized;

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error autorizando pago AZUL");
            return CreateFailureResult("500", "Error autorizando pago");
        }
    }

    public override async Task<PaymentResult> CaptureAsync(string authorizationCode, decimal amount, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Capturando autorización AZUL {AuthCode}", authorizationCode);

            // Aquí iría la lógica de captura
            var transactionId = Guid.NewGuid();

            return CreateSuccessResult(
                transactionId,
                $"AZUL_CAPTURE_{authorizationCode}",
                amount,
                "DOP",
                authCode: authorizationCode
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error capturando autorización AZUL");
            return CreateFailureResult("500", "Error capturando pago");
        }
    }

    public override async Task<PaymentResult> RefundAsync(string originalTransactionId, decimal? amount, string? reason, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Reembolsando transacción AZUL {TransactionId}", originalTransactionId);

            var transactionId = Guid.NewGuid();

            return CreateSuccessResult(
                transactionId,
                $"AZUL_REFUND_{originalTransactionId}",
                amount ?? 0,
                "DOP"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reembolsando transacción AZUL");
            return CreateFailureResult("500", "Error procesando reembolso");
        }
    }

    public override async Task<TokenizationResult> TokenizeCardAsync(CardData cardData, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Tokenizando tarjeta con AZUL (Cybersource)");

            var token = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(
                $"{cardData.CardNumber}|{cardData.ExpiryMonth}|{cardData.ExpiryYear}"));

            return new TokenizationResult
            {
                Success = true,
                Token = token,
                CardLastFour = cardData.CardNumber.Substring(cardData.CardNumber.Length - 4),
                CardBrand = DetermineCardBrand(cardData.CardNumber),
                ExpiryMonth = int.Parse(cardData.ExpiryMonth),
                ExpiryYear = int.Parse(cardData.ExpiryYear),
                TokenizedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tokenizando tarjeta con AZUL");
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
            _logger.LogInformation("Procesando pago con token AZUL");

            var transactionId = Guid.NewGuid();
            return CreateSuccessResult(
                transactionId,
                $"AZUL_TOKEN_{DateTime.UtcNow.Ticks}",
                amount,
                currency
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando pago con token AZUL");
            return CreateFailureResult("500", "Error procesando pago");
        }
    }

    public override bool ValidateWebhook(string body, string signature)
    {
        // Implementar validación de firma de AZUL
        _logger.LogInformation("Validando webhook AZUL");
        return !string.IsNullOrWhiteSpace(signature);
    }

    public override async Task<Guid> ProcessWebhookAsync(Dictionary<string, object> webhookData, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Procesando webhook AZUL");
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
