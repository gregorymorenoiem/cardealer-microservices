using CarDealer.Contracts.Abstractions;

namespace CarDealer.Contracts.Events.Billing;

/// <summary>
/// Evento publicado cuando se crea una nueva suscripción (primer pago exitoso o trial iniciado).
/// Consumido por: NotificationService (email de bienvenida), DealerAnalyticsService (cohort tracking).
/// </summary>
public class SubscriptionCreatedEvent : EventBase
{
    public override string EventType => "billing.subscription.created";

    public Guid DealerId { get; set; }
    public string DealerEmail { get; set; } = string.Empty;
    public string DealerName { get; set; } = string.Empty;

    /// <summary>Plan suscrito.</summary>
    public string Plan { get; set; } = string.Empty;

    /// <summary>Si inició con trial.</summary>
    public bool IsTrial { get; set; }

    /// <summary>Duración del trial en días (0 si no trial).</summary>
    public int TrialDays { get; set; }

    /// <summary>Precio mensual.</summary>
    public decimal MonthlyPrice { get; set; }

    /// <summary>Moneda.</summary>
    public string Currency { get; set; } = "USD";

    /// <summary>ID de Stripe (si aplica).</summary>
    public string? StripeSubscriptionId { get; set; }
}
