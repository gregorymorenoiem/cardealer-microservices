using CarDealer.Contracts.Abstractions;

namespace CarDealer.Contracts.Events.Billing;

/// <summary>
/// Evento emitido al cierre del mes cuando un dealer ÉLITE ha excedido
/// el límite de 2,000 conversaciones. BillingService lo consume para
/// crear un line‐item de overage en la factura del mes siguiente.
///
/// Cálculo:  (TotalConversations - IncludedLimit) × OverageUnitCost
/// Ejemplo:  (2,340 - 2,000) × $0.08 = $27.20
///
/// CONTRA #5 / OVERAGE BILLING FIX
/// </summary>
public class ConversationOverageBillingEvent : EventBase
{
    public override string EventType => "billing.conversation.overage";

    /// <summary>Dealer que superó el límite.</summary>
    public Guid DealerId { get; set; }

    /// <summary>Plan del dealer al momento del cierre ("elite").</summary>
    public string DealerPlan { get; set; } = string.Empty;

    /// <summary>Periodo facturado: YYYY-MM</summary>
    public string BillingPeriod { get; set; } = string.Empty;

    /// <summary>Total de conversaciones del mes.</summary>
    public int TotalConversations { get; set; }

    /// <summary>Conversaciones incluidas en el plan (2,000 para ÉLITE).</summary>
    public int IncludedLimit { get; set; }

    /// <summary>Conversaciones de overage (TotalConversations − IncludedLimit).</summary>
    public int OverageCount { get; set; }

    /// <summary>Costo unitario por conversación extra ($0.08).</summary>
    public decimal OverageUnitCost { get; set; }

    /// <summary>Monto total del overage: OverageCount × OverageUnitCost.</summary>
    public decimal OverageTotalAmount { get; set; }

    /// <summary>Moneda (siempre USD para el cálculo interno).</summary>
    public string Currency { get; set; } = "USD";
}
