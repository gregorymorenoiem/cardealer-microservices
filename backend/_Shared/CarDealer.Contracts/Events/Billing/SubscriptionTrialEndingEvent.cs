using CarDealer.Contracts.Abstractions;

namespace CarDealer.Contracts.Events.Billing;

/// <summary>
/// Evento publicado cuando el período de prueba de un dealer está por vencer.
/// Consumido por: NotificationService (email de advertencia), CRMService.
/// Publicado 3 días antes y 1 día antes del vencimiento.
/// </summary>
public class SubscriptionTrialEndingEvent : EventBase
{
    public override string EventType => "billing.subscription.trial_ending";

    public Guid DealerId { get; set; }
    public string DealerEmail { get; set; } = string.Empty;
    public string DealerName { get; set; } = string.Empty;

    /// <summary>Plan al que está en trial.</summary>
    public string TrialPlan { get; set; } = string.Empty;

    /// <summary>Fecha en que expira el trial.</summary>
    public DateTime TrialEndsAt { get; set; }

    /// <summary>Días restantes del trial.</summary>
    public int DaysRemaining { get; set; }

    /// <summary>Precio mensual si convierte.</summary>
    public decimal MonthlyPrice { get; set; }
}
