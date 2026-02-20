using CarDealer.Contracts.Abstractions;

namespace CarDealer.Contracts.Events.Vehicle;

/// <summary>
/// Evento emitido cuando un vehículo es publicado (cambia a estado activo).
/// </summary>
public class VehiclePublishedEvent : EventBase
{
    public override string EventType => "vehicles.vehicle.published";

    public Guid VehicleId { get; set; }
    public Guid SellerId { get; set; }
    public Guid? DealerId { get; set; }
    public string SellerType { get; set; } = string.Empty; // "Individual" or "Dealer"
    public string Title { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "DOP";
    public bool IsFeatured { get; set; }
    public DateTime PublishedAt { get; set; }
}
