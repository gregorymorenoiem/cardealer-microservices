using CarDealer.Contracts.Abstractions;

namespace CarDealer.Contracts.Events.Vehicle;

/// <summary>
/// Event published when a vehicle is updated.
/// </summary>
public class VehicleUpdatedEvent : EventBase
{
    public override string EventType => "vehicle.updated";
    
    public Guid VehicleId { get; set; }
    public Dictionary<string, object> Changes { get; set; } = new();
    public Guid UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
}
