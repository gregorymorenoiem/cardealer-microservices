using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using PaymentService.Domain.Enums;
using PaymentService.Domain.Interfaces;
using PaymentService.Domain.Models;

namespace PaymentService.Infrastructure.Services.Providers;

/// <summary>
/// Clase base abstracta para proveedores de pago
/// Contiene lógica común a todos los proveedores
/// </summary>
public abstract class BasePaymentGatewayProvider : IPaymentGatewayProvider
{
    protected readonly ILogger<BasePaymentGatewayProvider> _logger;
    protected readonly IConfiguration _configuration;

    public abstract PaymentGateway Gateway { get; }
    public abstract string Name { get; }
    public abstract PaymentGatewayType Type { get; }

    protected BasePaymentGatewayProvider(
        ILogger<BasePaymentGatewayProvider> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public virtual async Task<bool> IsAvailableAsync(CancellationToken cancellationToken)
    {
        try
        {
            var errors = ValidateConfiguration();
            if (errors.Count > 0)
            {
                _logger.LogWarning("Pasarela {Gateway} no disponible: {Errors}", 
                    Gateway, string.Join(", ", errors));
                return false;
            }

            _logger.LogInformation("Pasarela {Gateway} está disponible", Gateway);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verificando disponibilidad de {Gateway}", Gateway);
            return false;
        }
    }

    public abstract List<string> ValidateConfiguration();

    public abstract Task<PaymentResult> ChargeAsync(ChargeRequest request, CancellationToken cancellationToken);

    public abstract Task<PaymentResult> AuthorizeAsync(ChargeRequest request, CancellationToken cancellationToken);

    public abstract Task<PaymentResult> CaptureAsync(string authorizationCode, decimal amount, CancellationToken cancellationToken);

    public abstract Task<PaymentResult> RefundAsync(string originalTransactionId, decimal? amount, string? reason, CancellationToken cancellationToken);

    /// <summary>
    /// Reembolso usando RefundRequest - Implementación por defecto que delega al método legacy
    /// </summary>
    public virtual async Task<RefundResult> RefundAsync(RefundRequest request, CancellationToken cancellationToken)
    {
        var legacyResult = await RefundAsync(
            request.OriginalTransactionId, 
            request.Amount, 
            request.Reason, 
            cancellationToken);
        
        return new RefundResult
        {
            Success = legacyResult.Success,
            TransactionId = legacyResult.ExternalTransactionId,
            ResponseCode = legacyResult.ResponseCode,
            Message = legacyResult.ResponseMessage ?? (legacyResult.Success ? "Reembolso exitoso" : "Error en reembolso"),
            Amount = legacyResult.Amount,
            ProcessedAt = legacyResult.ProcessedAt,
            RawResponse = legacyResult.Metadata?.ContainsKey("RawResponse") == true 
                ? legacyResult.Metadata["RawResponse"]?.ToString() 
                : null
        };
    }

    public abstract Task<TokenizationResult> TokenizeCardAsync(CardData cardData, CancellationToken cancellationToken);

    public abstract Task<PaymentResult> ChargeTokenAsync(string token, decimal amount, string currency, string description, CancellationToken cancellationToken);

    public abstract bool ValidateWebhook(string body, string signature);

    public abstract Task<Guid> ProcessWebhookAsync(Dictionary<string, object> webhookData, CancellationToken cancellationToken);

    /// <summary>
    /// Helper para validar configuración básica
    /// </summary>
    protected List<string> ValidateBasicConfig(params string[] requiredKeys)
    {
        var errors = new List<string>();

        foreach (var key in requiredKeys)
        {
            var value = _configuration[$"PaymentGateway:{Gateway}:{key}"];
            if (string.IsNullOrWhiteSpace(value))
            {
                errors.Add($"Configuración requerida no encontrada: PaymentGateway:{Gateway}:{key}");
            }
        }

        return errors;
    }

    /// <summary>
    /// Helper para obtener valor de configuración
    /// </summary>
    protected string? GetConfigValue(string key)
    {
        return _configuration[$"PaymentGateway:{Gateway}:{key}"];
    }

    /// <summary>
    /// Helper para crear un PaymentResult exitoso
    /// </summary>
    protected PaymentResult CreateSuccessResult(
        Guid transactionId,
        string externalTransactionId,
        decimal amount,
        string currency,
        string? authCode = null,
        string? cardToken = null,
        string? cardLastFour = null,
        Dictionary<string, object>? metadata = null)
    {
        return new PaymentResult
        {
            Success = true,
            TransactionId = transactionId,
            ExternalTransactionId = externalTransactionId,
            AuthorizationCode = authCode,
            Amount = amount,
            Currency = currency,
            Status = TransactionStatus.Approved,
            CardToken = cardToken,
            CardLastFour = cardLastFour,
            ResponseCode = "00",
            ProcessedAt = DateTime.UtcNow,
            Metadata = metadata ?? new Dictionary<string, object>()
        };
    }

    /// <summary>
    /// Helper para crear un PaymentResult fallido
    /// </summary>
    protected PaymentResult CreateFailureResult(
        string responseCode,
        string? responseMessage = null,
        Dictionary<string, object>? metadata = null)
    {
        return new PaymentResult
        {
            Success = false,
            Status = TransactionStatus.Error,
            ResponseCode = responseCode,
            ResponseMessage = responseMessage,
            ProcessedAt = DateTime.UtcNow,
            Metadata = metadata ?? new Dictionary<string, object>()
        };
    }
}
