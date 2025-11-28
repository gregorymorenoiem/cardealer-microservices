using CarDealer.Contracts.Abstractions;

namespace CarDealer.Contracts.Events.Vehicle;

/// <summary>
/// Event published when a vehicle is deleted from inventory.
/// </summary>
public class VehicleDeletedEvent : EventBase
{
    public override string EventType => "vehicle.deleted";

    public Guid VehicleId { get; set; }
    public string? Reason { get; set; }
    public Guid DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }
}
