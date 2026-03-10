using CarDealer.Contracts.Abstractions;

namespace CarDealer.Contracts.Events.Vehicle;

/// <summary>
/// Evento emitido cuando un vehículo es publicado (cambia a estado activo).
/// Incluye campos de detalle para que los consumidores puedan hacer matching
/// contra búsquedas guardadas y alertas de precio sin llamar de vuelta al API.
/// </summary>
public class VehiclePublishedEvent : EventBase
{
    public override string EventType => "vehicles.vehicle.published";

    // ── Identificación ──
    public Guid VehicleId { get; set; }
    public Guid SellerId { get; set; }
    public Guid? DealerId { get; set; }
    public string SellerType { get; set; } = string.Empty; // "Individual" or "Dealer"

    // ── Información básica ──
    public string Title { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "DOP";
    public bool IsFeatured { get; set; }
    public DateTime PublishedAt { get; set; }

    // ── Campos para matching de alertas (SavedSearch criteria) ──
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string? BodyType { get; set; }
    public string? FuelType { get; set; }
    public string? Transmission { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public int Mileage { get; set; }
    public string? Condition { get; set; }
    public string? Slug { get; set; }
    public string? ImageUrl { get; set; }
}
