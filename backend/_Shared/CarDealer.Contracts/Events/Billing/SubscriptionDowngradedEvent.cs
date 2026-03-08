using CarDealer.Contracts.Abstractions;

namespace CarDealer.Contracts.Events.Billing;

/// <summary>
/// Evento publicado cuando un dealer baja de plan (ej: Pro → Visible).
/// Consumido por: NotificationService, DealerAnalyticsService, AdminService.
/// </summary>
public class SubscriptionDowngradedEvent : EventBase
{
    public override string EventType => "billing.subscription.downgraded";

    public Guid DealerId { get; set; }
    public string DealerEmail { get; set; } = string.Empty;
    public string DealerName { get; set; } = string.Empty;

    /// <summary>Plan anterior (ej: "Pro").</summary>
    public string OldPlan { get; set; } = string.Empty;

    /// <summary>Plan nuevo (ej: "Visible").</summary>
    public string NewPlan { get; set; } = string.Empty;

    /// <summary>Razón del downgrade.</summary>
    public string? Reason { get; set; }

    /// <summary>Diferencia de precio mensual (negativa).</summary>
    public decimal PriceDifference { get; set; }

    /// <summary>Fecha efectiva del cambio.</summary>
    public DateTime EffectiveAt { get; set; }
}
