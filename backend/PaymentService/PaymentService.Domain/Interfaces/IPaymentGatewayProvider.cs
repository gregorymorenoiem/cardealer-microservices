using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;
using PaymentService.Domain.Models;

namespace PaymentService.Domain.Interfaces;

/// <summary>
/// Interfaz genérica para proveedores de pago
/// Cada proveedor (AZUL, CardNET, PixelPay, Fygaro) debe implementar esta interfaz
/// </summary>
public interface IPaymentGatewayProvider
{
    /// <summary>
    /// Identificador único de la pasarela
    /// </summary>
    PaymentGateway Gateway { get; }

    /// <summary>
    /// Nombre descriptivo de la pasarela
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Tipo de pasarela (Banking, Fintech, Aggregator)
    /// </summary>
    PaymentGatewayType Type { get; }

    /// <summary>
    /// Comprobación de estado de la pasarela
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>True si la pasarela está disponible</returns>
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Valida que la pasarela esté correctamente configurada
    /// </summary>
    /// <returns>Lista de errores de configuración (vacía si ok)</returns>
    List<string> ValidateConfiguration();

    /// <summary>
    /// Procesa un cargo/pago
    /// </summary>
    /// <param name="request">Datos del pago</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado del pago con detalles de la transacción</returns>
    Task<PaymentResult> ChargeAsync(ChargeRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Autoriza un pago sin capturar fondos
    /// </summary>
    /// <param name="request">Datos del pago</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado de la autorización</returns>
    Task<PaymentResult> AuthorizeAsync(ChargeRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Captura fondos de una autorización previa
    /// </summary>
    /// <param name="authorizationCode">Código de autorización</param>
    /// <param name="amount">Monto a capturar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado de la captura</returns>
    Task<PaymentResult> CaptureAsync(string authorizationCode, decimal amount, CancellationToken cancellationToken);

    /// <summary>
    /// Reembolsa una transacción
    /// </summary>
    /// <param name="originalTransactionId">ID original de la transacción</param>
    /// <param name="amount">Monto a reembolsar (null = total)</param>
    /// <param name="reason">Razón del reembolso</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado del reembolso</returns>
    Task<PaymentResult> RefundAsync(string originalTransactionId, decimal? amount, string? reason, CancellationToken cancellationToken);

    /// <summary>
    /// Reembolsa una transacción usando el modelo RefundRequest
    /// </summary>
    /// <param name="request">Solicitud de reembolso con todos los detalles</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado del reembolso</returns>
    Task<RefundResult> RefundAsync(RefundRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Tokeniza una tarjeta para pagos recurrentes
    /// </summary>
    /// <param name="cardData">Datos de la tarjeta</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Token de la tarjeta y detalles</returns>
    Task<TokenizationResult> TokenizeCardAsync(CardData cardData, CancellationToken cancellationToken);

    /// <summary>
    /// Procesa un pago recurrente usando un token
    /// </summary>
    /// <param name="token">Token de la tarjeta</param>
    /// <param name="amount">Monto a cobrar</param>
    /// <param name="currency">Moneda</param>
    /// <param name="description">Descripción del pago</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado del pago</returns>
    Task<PaymentResult> ChargeTokenAsync(string token, decimal amount, string currency, string description, CancellationToken cancellationToken);

    /// <summary>
    /// Valida un webhook recibido de la pasarela
    /// </summary>
    /// <param name="body">Cuerpo del webhook</param>
    /// <param name="signature">Firma del webhook</param>
    /// <returns>True si el webhook es válido</returns>
    bool ValidateWebhook(string body, string signature);

    /// <summary>
    /// Procesa un webhook recibido de la pasarela
    /// </summary>
    /// <param name="webhookData">Datos del webhook</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>ID de la transacción actualizada</returns>
    Task<Guid> ProcessWebhookAsync(Dictionary<string, object> webhookData, CancellationToken cancellationToken);
}

/// <summary>
/// Solicitud de pago genérica
/// </summary>
public class ChargeRequest
{
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "DOP";
    public string Description { get; set; } = string.Empty;
    public PaymentMethod PaymentMethod { get; set; }
    public CardData? CardData { get; set; }
    public string? CardToken { get; set; }
    public string? IdempotencyKey { get; set; }
    
    // Información del cliente
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerIpAddress { get; set; }
    
    // Referencias
    public string? InvoiceReference { get; set; }
    
    // Pagos recurrentes
    public bool IsRecurring { get; set; }
    public Guid? SubscriptionId { get; set; }
}

/// <summary>
/// Datos de la tarjeta
/// </summary>
public class CardData
{
    public string CardNumber { get; set; } = string.Empty;
    public string ExpiryMonth { get; set; } = string.Empty;
    public string ExpiryYear { get; set; } = string.Empty;
    public string CVV { get; set; } = string.Empty;
    public string CardholderName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
}

/// <summary>
/// Resultado de una operación de pago
/// </summary>
public class PaymentResult
{
    public bool Success { get; set; }
    public Guid? TransactionId { get; set; }
    public string ExternalTransactionId { get; set; } = string.Empty;
    public string? AuthorizationCode { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "DOP";
    public TransactionStatus Status { get; set; }
    public string? CardToken { get; set; }
    public string? CardLastFour { get; set; }
    public string ResponseCode { get; set; } = string.Empty;
    public string? ResponseMessage { get; set; }
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object>? Metadata { get; set; }
    
    // Propiedades de comisión del proveedor
    public decimal? Commission { get; set; }
    public decimal? CommissionPercentage { get; set; }
    public decimal? NetAmount { get; set; }
}

/// <summary>
/// Resultado de tokenización
/// </summary>
public class TokenizationResult
{
    public bool Success { get; set; }
    public string Token { get; set; } = string.Empty;
    public string CardLastFour { get; set; } = string.Empty;
    public string CardBrand { get; set; } = string.Empty; // Visa, Mastercard, Amex, etc.
    public int ExpiryMonth { get; set; }
    public int ExpiryYear { get; set; }
    public DateTime TokenizedAt { get; set; } = DateTime.UtcNow;
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}
