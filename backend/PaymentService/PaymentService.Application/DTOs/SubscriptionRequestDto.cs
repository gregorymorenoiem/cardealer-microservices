namespace PaymentService.Application.DTOs;

/// <summary>
/// DTO para crear/actualizar suscripción recurrente
/// </summary>
public class SubscriptionRequestDto
{
    /// <summary>
    /// ID del usuario suscriptor
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Monto a cobrar periódicamente
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Moneda
    /// </summary>
    public string Currency { get; set; } = "DOP";

    /// <summary>
    /// Frecuencia de cobro (Daily, Weekly, Monthly, etc.)
    /// </summary>
    public string Frequency { get; set; } = "Monthly";

    /// <summary>
    /// Fecha de inicio
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Fecha de finalización (null = indefinida)
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Token de tarjeta para pagos recurrentes
    /// </summary>
    public string? CardToken { get; set; }

    /// <summary>
    /// Número de tarjeta (si no se usa token)
    /// </summary>
    public string? CardNumber { get; set; }

    /// <summary>
    /// Mes de expiración
    /// </summary>
    public string? CardExpiryMonth { get; set; }

    /// <summary>
    /// Año de expiración
    /// </summary>
    public string? CardExpiryYear { get; set; }

    /// <summary>
    /// CVV
    /// </summary>
    public string? CardCVV { get; set; }

    /// <summary>
    /// Nombre del titular
    /// </summary>
    public string? CardholderName { get; set; }

    /// <summary>
    /// Email del cliente
    /// </summary>
    public string? CustomerEmail { get; set; }

    /// <summary>
    /// Teléfono del cliente
    /// </summary>
    public string? CustomerPhone { get; set; }

    /// <summary>
    /// Nombre del plan/producto
    /// </summary>
    public string? PlanName { get; set; }

    /// <summary>
    /// Referencia de invoice
    /// </summary>
    public string? InvoiceReference { get; set; }

    /// <summary>
    /// Método de pago
    /// </summary>
    public string PaymentMethod { get; set; } = "CreditCard";
}
