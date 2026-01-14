namespace AzulPaymentService.Application.DTOs;

/// <summary>
/// DTO para solicitud de cobro/transacción
/// </summary>
public class ChargeRequestDto
{
    /// <summary>
    /// ID del usuario que realiza el pago
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Monto a cobrar
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Moneda (default: DOP)
    /// </summary>
    public string Currency { get; set; } = "DOP";

    /// <summary>
    /// Descripción de la transacción
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Número de tarjeta (si no se usa token)
    /// </summary>
    public string? CardNumber { get; set; }

    /// <summary>
    /// Mes de expiración (MM)
    /// </summary>
    public string? CardExpiryMonth { get; set; }

    /// <summary>
    /// Año de expiración (YYYY)
    /// </summary>
    public string? CardExpiryYear { get; set; }

    /// <summary>
    /// CVV de la tarjeta
    /// </summary>
    public string? CardCVV { get; set; }

    /// <summary>
    /// Nombre del titular
    /// </summary>
    public string? CardholderName { get; set; }

    /// <summary>
    /// Token de tarjeta (para pagos recurrentes)
    /// </summary>
    public string? CardToken { get; set; }

    /// <summary>
    /// Método de pago (tarjeta, móvil, ACH, etc.)
    /// </summary>
    public string PaymentMethod { get; set; } = "CreditCard";

    /// <summary>
    /// Email del cliente
    /// </summary>
    public string? CustomerEmail { get; set; }

    /// <summary>
    /// Teléfono del cliente
    /// </summary>
    public string? CustomerPhone { get; set; }

    /// <summary>
    /// IP del cliente
    /// </summary>
    public string? CustomerIpAddress { get; set; }

    /// <summary>
    /// Referencia de la orden/invoice
    /// </summary>
    public string? InvoiceReference { get; set; }

    /// <summary>
    /// Tipo de transacción (Sale, Authorize)
    /// </summary>
    public string TransactionType { get; set; } = "Sale";

    /// <summary>
    /// Si es una transacción recurrente
    /// </summary>
    public bool IsRecurring { get; set; }

    /// <summary>
    /// ID de suscripción (si aplica)
    /// </summary>
    public Guid? SubscriptionId { get; set; }
}
