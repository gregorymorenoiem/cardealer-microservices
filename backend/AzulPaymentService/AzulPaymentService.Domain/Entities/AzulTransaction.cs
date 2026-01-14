namespace AzulPaymentService.Domain.Entities;

/// <summary>
/// Representa una transacción de pago procesada por AZUL
/// </summary>
public class AzulTransaction
{
    /// <summary>
    /// ID único de la transacción en OKLA
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID de transacción en AZUL (referencia externa)
    /// </summary>
    public string AzulTransactionId { get; set; } = string.Empty;

    /// <summary>
    /// ID del usuario que realiza la transacción
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Monto de la transacción (en DOP o USD)
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Moneda (DOP, USD, etc.)
    /// </summary>
    public string Currency { get; set; } = "DOP";

    /// <summary>
    /// Descripción de la transacción
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Método de pago utilizado
    /// </summary>
    public TransactionStatus Status { get; set; }

    /// <summary>
    /// Tipo de método de pago
    /// </summary>
    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>
    /// Últimos 4 dígitos de la tarjeta (si aplica)
    /// </summary>
    public string? CardLastFour { get; set; }

    /// <summary>
    /// Tipo de transacción (Sale, Authorize, etc.)
    /// </summary>
    public string TransactionType { get; set; } = "Sale"; // Sale, Authorize, Capture, Void, Refund

    /// <summary>
    /// Código de respuesta de AZUL
    /// </summary>
    public string? ResponseCode { get; set; }

    /// <summary>
    /// Mensaje de respuesta de AZUL
    /// </summary>
    public string? ResponseMessage { get; set; }

    /// <summary>
    /// Token de la tarjeta (si se tokenizó)
    /// </summary>
    public string? CardToken { get; set; }

    /// <summary>
    /// Número de aprobación de AZUL
    /// </summary>
    public string? AuthorizationCode { get; set; }

    /// <summary>
    /// Si es una transacción recurrente
    /// </summary>
    public bool IsRecurring { get; set; }

    /// <summary>
    /// ID de suscripción asociada (si aplica)
    /// </summary>
    public Guid? SubscriptionId { get; set; }

    /// <summary>
    /// Referencia de la orden/invoice
    /// </summary>
    public string? InvoiceReference { get; set; }

    /// <summary>
    /// Email del cliente
    /// </summary>
    public string? CustomerEmail { get; set; }

    /// <summary>
    /// Teléfono del cliente
    /// </summary>
    public string? CustomerPhone { get; set; }

    /// <summary>
    /// IP del cliente que realizó la transacción
    /// </summary>
    public string? CustomerIpAddress { get; set; }

    /// <summary>
    /// Timestamp de creación
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp de última actualización
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Timestamp de completación
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Razón del fallo (si aplica)
    /// </summary>
    public string? FailureReason { get; set; }

    /// <summary>
    /// Trazabilidad: Raw response de AZUL
    /// </summary>
    public string? RawAzulResponse { get; set; }
}
