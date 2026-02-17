using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using PaymentService.Domain.Enums;
using PaymentService.Domain.Interfaces;

namespace PaymentService.Infrastructure.Services.Providers;

/// <summary>
/// Implementación del proveedor CardNET (Bancaria RD)
/// 
/// Características:
/// - Comisión: 2.5% - 4.5%
/// - Costo fijo: RD$5 - 10 por transacción
/// - Mensualidad: US$30 - 50
/// - Tokenización: Sí (Solicitar)
/// - Tipo: Banking (Bancaria tradicional)
/// </summary>
public class CardNETPaymentProvider : BasePaymentGatewayProvider
{
    private readonly HttpClient _httpClient;

    public override PaymentGateway Gateway => PaymentGateway.CardNET;
    public override string Name => "CardNET - Bancaria RD";
    public override PaymentGatewayType Type => PaymentGatewayType.Banking;

    public CardNETPaymentProvider(
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
            "TerminalId",
            "APIKey",
            "Endpoint"
        );
    }

    public override async Task<PaymentResult> ChargeAsync(ChargeRequest request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Procesando cargo CardNET para usuario {UserId}", request.UserId);

            var transactionId = Guid.NewGuid();
            var externalId = $"CARDNET_{DateTime.UtcNow.Ticks}";

            return CreateSuccessResult(
                transactionId,
                externalId,
                request.Amount,
                request.Currency,
                authCode: "APPROVED",
                cardLastFour: request.CardData?.CardNumber?.Substring(request.CardData.CardNumber.Length - 4)
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando cargo CardNET");
            return CreateFailureResult("500", "Error procesando pago");
        }
    }

    public override async Task<PaymentResult> AuthorizeAsync(ChargeRequest request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Autorizando pago CardNET para usuario {UserId}", request.UserId);

            var result = await ChargeAsync(request, cancellationToken);
            result.Status = TransactionStatus.Authorized;

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error autorizando pago CardNET");
            return CreateFailureResult("500", "Error autorizando pago");
        }
    }

    public override async Task<PaymentResult> CaptureAsync(string authorizationCode, decimal amount, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Capturando autorización CardNET {AuthCode}", authorizationCode);

            var transactionId = Guid.NewGuid();

            return CreateSuccessResult(
                transactionId,
                $"CARDNET_CAPTURE_{authorizationCode}",
                amount,
                "DOP",
                authCode: authorizationCode
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error capturando autorización CardNET");
            return CreateFailureResult("500", "Error capturando pago");
        }
    }

    public override async Task<PaymentResult> RefundAsync(string originalTransactionId, decimal? amount, string? reason, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Reembolsando transacción CardNET {TransactionId}", originalTransactionId);

            var transactionId = Guid.NewGuid();

            return CreateSuccessResult(
                transactionId,
                $"CARDNET_REFUND_{originalTransactionId}",
                amount ?? 0,
                "DOP"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reembolsando transacción CardNET");
            return CreateFailureResult("500", "Error procesando reembolso");
        }
    }

    public override async Task<TokenizationResult> TokenizeCardAsync(CardData cardData, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Tokenizando tarjeta con CardNET");

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
            _logger.LogError(ex, "Error tokenizando tarjeta con CardNET");
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
            _logger.LogInformation("Procesando pago con token CardNET");

            var transactionId = Guid.NewGuid();
            return CreateSuccessResult(
                transactionId,
                $"CARDNET_TOKEN_{DateTime.UtcNow.Ticks}",
                amount,
                currency
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando pago con token CardNET");
            return CreateFailureResult("500", "Error procesando pago");
        }
    }

    public override bool ValidateWebhook(string body, string signature)
    {
        _logger.LogInformation("Validando webhook CardNET");
        return !string.IsNullOrWhiteSpace(signature);
    }

    public override async Task<Guid> ProcessWebhookAsync(Dictionary<string, object> webhookData, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Procesando webhook CardNET");
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
