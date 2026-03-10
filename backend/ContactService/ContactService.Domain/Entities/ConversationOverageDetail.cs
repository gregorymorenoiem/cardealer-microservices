namespace ContactService.Domain.Entities;

/// <summary>
/// Registra cada conversación individual que excede el límite del plan.
/// Permite al dealer descargar un detalle de fecha/hora de cada
/// conversación de overage.
///
/// Redis key for monthly overage list:
///   okla:contact:overage:{dealerId}:{YYYY-MM}  → Sorted set (score=timestamp)
///
/// CONTRA #5 / OVERAGE BILLING FIX
/// </summary>
public class ConversationOverageDetail
{
    /// <summary>Unique identifier.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Dealer al que pertenece el overage.</summary>
    public Guid DealerId { get; set; }

    /// <summary>Contact request que generó la conversación de overage.</summary>
    public Guid ContactRequestId { get; set; }

    /// <summary>Buyer que inició la conversación.</summary>
    public Guid BuyerId { get; set; }

    /// <summary>Vehicle relacionado (si aplica).</summary>
    public Guid? VehicleId { get; set; }

    /// <summary>Subject / título de la conversación.</summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>Plan del dealer al momento del overage.</summary>
    public string DealerPlan { get; set; } = string.Empty;

    /// <summary>Periodo de facturación: YYYY-MM.</summary>
    public string BillingPeriod { get; set; } = string.Empty;

    /// <summary>Número de conversación en el mes (2001, 2002, etc.).</summary>
    public int ConversationNumber { get; set; }

    /// <summary>Límite incluido en el plan (2000).</summary>
    public int PlanLimit { get; set; }

    /// <summary>Costo unitario de esta conversación ($0.08).</summary>
    public decimal UnitCost { get; set; }

    /// <summary>Fecha y hora exacta de la conversación (UTC).</summary>
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>Fecha de creación del registro.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
