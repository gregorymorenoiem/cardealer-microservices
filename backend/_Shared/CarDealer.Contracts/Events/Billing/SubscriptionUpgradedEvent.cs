using CarDealer.Contracts.Abstractions;

namespace CarDealer.Contracts.Events.Billing;

/// <summary>
/// Evento publicado cuando un dealer sube de plan (ej: Visible → Pro).
/// Consumido por: NotificationService (email de bienvenida al plan), DealerAnalyticsService.
/// </summary>
public class SubscriptionUpgradedEvent : EventBase
{
    public override string EventType => "billing.subscription.upgraded";

    public Guid DealerId { get; set; }
    public string DealerEmail { get; set; } = string.Empty;
    public string DealerName { get; set; } = string.Empty;

    /// <summary>Plan anterior (ej: "Libre").</summary>
    public string OldPlan { get; set; } = string.Empty;

    /// <summary>Plan nuevo (ej: "Pro").</summary>
    public string NewPlan { get; set; } = string.Empty;

    /// <summary>Diferencia de precio mensual (positiva).</summary>
    public decimal PriceDifference { get; set; }

    /// <summary>Fecha efectiva del cambio.</summary>
    public DateTime EffectiveAt { get; set; }
}
