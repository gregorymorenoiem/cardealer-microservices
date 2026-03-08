namespace BillingService.Domain.Entities;

/// <summary>
/// Razones estructuradas de cancelación de suscripción.
/// Permite analíticas y respuestas de retención automatizadas.
/// </summary>
public enum CancellationReasonType
{
    /// <summary>El plan es muy costoso para el dealer.</summary>
    TooExpensive = 1,

    /// <summary>No percibe suficiente valor/ROI del plan.</summary>
    NotEnoughValue = 2,

    /// <summary>Se cambió a un competidor (CoroMotors, SuperCarros, etc.).</summary>
    SwitchedCompetitor = 3,

    /// <summary>Problemas técnicos recurrentes con la plataforma.</summary>
    TechnicalIssues = 4,

    /// <summary>El negocio/dealer cerró operaciones.</summary>
    BusinessClosed = 5,

    /// <summary>No estaba usando la plataforma lo suficiente.</summary>
    NotUsingEnough = 6,

    /// <summary>Temporada baja — planea regresar después.</summary>
    SeasonalPause = 7,

    /// <summary>Falta de soporte/atención al cliente.</summary>
    PoorSupport = 8,

    /// <summary>Faltan funcionalidades que necesita.</summary>
    MissingFeatures = 9,

    /// <summary>Otra razón (ver detalles en texto libre).</summary>
    Other = 99,
}

/// <summary>
/// Dirección del cambio de plan.
/// </summary>
public enum PlanChangeDirection
{
    Upgrade = 1,
    Downgrade = 2,
    Cancel = 3,
    Reactivate = 4,
    TrialToActive = 5,
}

/// <summary>
/// Historial de cambios de suscripción de un dealer.
/// Permite tracking completo de la vida de la suscripción para analytics de churn.
/// </summary>
public class SubscriptionChangeHistory
{
    public Guid Id { get; private set; }
    public Guid SubscriptionId { get; private set; }
    public Guid DealerId { get; private set; }

    /// <summary>Plan anterior.</summary>
    public SubscriptionPlan OldPlan { get; private set; }

    /// <summary>Plan nuevo.</summary>
    public SubscriptionPlan NewPlan { get; private set; }

    /// <summary>Status anterior.</summary>
    public SubscriptionStatus OldStatus { get; private set; }

    /// <summary>Status nuevo.</summary>
    public SubscriptionStatus NewStatus { get; private set; }

    /// <summary>Dirección del cambio.</summary>
    public PlanChangeDirection Direction { get; private set; }

    /// <summary>Razón estructurada del cambio.</summary>
    public CancellationReasonType? ReasonType { get; private set; }

    /// <summary>Detalle de la razón en texto libre.</summary>
    public string? ReasonDetails { get; private set; }

    /// <summary>Precio anterior por ciclo.</summary>
    public decimal OldPrice { get; private set; }

    /// <summary>Precio nuevo por ciclo.</summary>
    public decimal NewPrice { get; private set; }

    /// <summary>Moneda.</summary>
    public string Currency { get; private set; } = "USD";

    /// <summary>Fecha del cambio.</summary>
    public DateTime ChangedAt { get; private set; }

    /// <summary>Quién realizó el cambio (dealer, admin, sistema).</summary>
    public string ChangedBy { get; private set; } = "system";

    /// <summary>ID del evento de Stripe asociado (si aplica).</summary>
    public string? StripeEventId { get; private set; }

    // Navigation
    public Subscription? Subscription { get; private set; }

    private SubscriptionChangeHistory() { }

    public SubscriptionChangeHistory(
        Guid subscriptionId,
        Guid dealerId,
        SubscriptionPlan oldPlan,
        SubscriptionPlan newPlan,
        SubscriptionStatus oldStatus,
        SubscriptionStatus newStatus,
        PlanChangeDirection direction,
        decimal oldPrice,
        decimal newPrice,
        string? reasonDetails = null,
        CancellationReasonType? reasonType = null,
        string changedBy = "system",
        string? stripeEventId = null)
    {
        Id = Guid.NewGuid();
        SubscriptionId = subscriptionId;
        DealerId = dealerId;
        OldPlan = oldPlan;
        NewPlan = newPlan;
        OldStatus = oldStatus;
        NewStatus = newStatus;
        Direction = direction;
        OldPrice = oldPrice;
        NewPrice = newPrice;
        ReasonDetails = reasonDetails;
        ReasonType = reasonType;
        ChangedBy = changedBy;
        StripeEventId = stripeEventId;
        Currency = "USD";
        ChangedAt = DateTime.UtcNow;
    }
}
