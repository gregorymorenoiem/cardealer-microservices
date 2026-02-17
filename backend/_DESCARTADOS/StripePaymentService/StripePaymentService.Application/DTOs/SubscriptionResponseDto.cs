namespace StripePaymentService.Application.DTOs;

/// <summary>
/// DTO para respuesta de suscripción
/// </summary>
public class SubscriptionResponseDto
{
    /// <summary>
    /// ID interno en OKLA
    /// </summary>
    public Guid SubscriptionId { get; set; }

    /// <summary>
    /// ID de la suscripción en Stripe
    /// </summary>
    public string StripeSubscriptionId { get; set; } = string.Empty;

    /// <summary>
    /// ID del Customer
    /// </summary>
    public string StripeCustomerId { get; set; } = string.Empty;

    /// <summary>
    /// Estado
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Monto
    /// </summary>
    public long Amount { get; set; }

    /// <summary>
    /// Moneda
    /// </summary>
    public string Currency { get; set; } = "usd";

    /// <summary>
    /// Intervalo de facturación
    /// </summary>
    public string BillingInterval { get; set; } = string.Empty;

    /// <summary>
    /// Próxima fecha de facturación
    /// </summary>
    public DateTime NextBillingDate { get; set; }

    /// <summary>
    /// Fecha de inicio
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Fecha de cancelación
    /// </summary>
    public DateTime? CancelledAt { get; set; }

    /// <summary>
    /// Total pagado
    /// </summary>
    public long TotalPaid { get; set; }

    /// <summary>
    /// Si fue exitosa
    /// </summary>
    public bool IsSuccessful => Status == "active";
}
