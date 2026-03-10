using CarDealer.Contracts.Abstractions;

namespace CarDealer.Contracts.Events.Vehicle;

/// <summary>
/// Evento emitido cuando un comprador crea un lead (contacta al vendedor).
/// </summary>
public class LeadCreatedEvent : EventBase
{
    public override string EventType => "vehicles.lead.created";

    public Guid LeadId { get; set; }
    public Guid VehicleId { get; set; }
    public Guid SellerId { get; set; }
    public Guid? DealerId { get; set; }
    public Guid? BuyerId { get; set; }
    public string BuyerName { get; set; } = string.Empty;
    public string BuyerEmail { get; set; } = string.Empty;
    public string? BuyerPhone { get; set; }
    public string Message { get; set; } = string.Empty;
    public string VehicleTitle { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// True when this is the very first inquiry a dealer receives on OKLA.
    /// Used to trigger a celebration notification flow.
    /// </summary>
    public bool IsFirstInquiry { get; set; }
}
