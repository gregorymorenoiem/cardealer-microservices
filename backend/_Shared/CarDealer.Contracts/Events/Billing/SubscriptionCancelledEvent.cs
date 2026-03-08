using CarDealer.Contracts.Abstractions;

namespace CarDealer.Contracts.Events.Billing;

/// <summary>
/// Evento publicado cuando un dealer cancela su suscripción.
/// Consumido por: NotificationService (email de confirmación), DealerAnalyticsService, AdminService, CRMService.
/// </summary>
public class SubscriptionCancelledEvent : EventBase
{
    public override string EventType => "billing.subscription.cancelled";

    /// <summary>ID del dealer que canceló.</summary>
    public Guid DealerId { get; set; }

    /// <summary>Email del dealer.</summary>
    public string DealerEmail { get; set; } = string.Empty;

    /// <summary>Nombre del dealer.</summary>
    public string DealerName { get; set; } = string.Empty;

    /// <summary>Plan que tenía antes de cancelar (Visible, Pro, Élite).</summary>
    public string PreviousPlan { get; set; } = string.Empty;

    /// <summary>Razón estructurada de cancelación.</summary>
    public string CancellationReasonType { get; set; } = "Other";

    /// <summary>Detalle libre de la razón de cancelación.</summary>
    public string? CancellationReasonDetails { get; set; }

    /// <summary>Si la cancelación es al final del período o inmediata.</summary>
    public bool CancelAtPeriodEnd { get; set; }

    /// <summary>Fecha efectiva de cancelación.</summary>
    public DateTime EffectiveAt { get; set; }

    /// <summary>Tiempo que estuvo suscrito al plan (en días).</summary>
    public int DaysOnPlan { get; set; }

    /// <summary>Monto mensual que pagaba (USD).</summary>
    public decimal MonthlyAmount { get; set; }
}
