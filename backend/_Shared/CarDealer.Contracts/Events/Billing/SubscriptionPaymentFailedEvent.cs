using CarDealer.Contracts.Abstractions;

namespace CarDealer.Contracts.Events.Billing;

/// <summary>
/// Evento publicado cuando falla un pago de suscripción.
/// Consumido por: NotificationService (email de pago fallido), AdminService (dashboard MRR at risk).
/// </summary>
public class SubscriptionPaymentFailedEvent : EventBase
{
    public override string EventType => "billing.subscription.payment_failed";

    public Guid DealerId { get; set; }
    public string DealerEmail { get; set; } = string.Empty;
    public string DealerName { get; set; } = string.Empty;

    /// <summary>Plan actual del dealer.</summary>
    public string Plan { get; set; } = string.Empty;

    /// <summary>Monto que falló.</summary>
    public decimal Amount { get; set; }

    /// <summary>Moneda.</summary>
    public string Currency { get; set; } = "USD";

    /// <summary>Número de intento de cobro (1, 2, 3...).</summary>
    public int AttemptNumber { get; set; }

    /// <summary>Fecha del próximo reintento (si hay).</summary>
    public DateTime? NextRetryAt { get; set; }

    /// <summary>Razón del fallo.</summary>
    public string FailureReason { get; set; } = string.Empty;

    /// <summary>ID del invoice de Stripe.</summary>
    public string? StripeInvoiceId { get; set; }
}
