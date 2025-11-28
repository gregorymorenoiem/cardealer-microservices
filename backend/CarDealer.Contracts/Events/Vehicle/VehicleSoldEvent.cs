using CarDealer.Contracts.Abstractions;

namespace CarDealer.Contracts.Events.Vehicle;

/// <summary>
/// Event published when a vehicle is sold.
/// </summary>
public class VehicleSoldEvent : EventBase
{
    public override string EventType => "vehicle.sold";

    public Guid VehicleId { get; set; }
    public Guid CustomerId { get; set; }
    public decimal SalePrice { get; set; }
    public DateTime SoldAt { get; set; }
    public string? SalesPersonId { get; set; }
}
