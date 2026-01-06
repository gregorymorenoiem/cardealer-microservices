using CarDealer.Contracts.Abstractions;

namespace CarDealer.Contracts.Events.Billing;

/// <summary>
/// Evento publicado cuando se completa un pago exitosamente.
/// </summary>
public class PaymentCompletedEvent : EventBase
{
    public override string EventType => "billing.payment.completed";

    /// <summary>
    /// ID único del pago.
    /// </summary>
    public Guid PaymentId { get; set; }

    /// <summary>
    /// ID del usuario que realizó el pago.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Email del usuario.
    /// </summary>
    public string UserEmail { get; set; } = string.Empty;

    /// <summary>
    /// Nombre del usuario.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Monto del pago.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Moneda del pago (USD, EUR, etc).
    /// </summary>
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// ID de la transacción de Stripe.
    /// </summary>
    public string StripePaymentIntentId { get; set; } = string.Empty;

    /// <summary>
    /// Descripción del pago.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Plan de suscripción (si aplica).
    /// </summary>
    public string? SubscriptionPlan { get; set; }

    /// <summary>
    /// Timestamp del pago.
    /// </summary>
    public DateTime PaidAt { get; set; }
}
