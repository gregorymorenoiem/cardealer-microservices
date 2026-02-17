using System;

namespace RecommendationService.Domain.Entities;

/// <summary>
/// Interacción de un usuario con un vehículo (view, favorite, contact)
/// </summary>
public class VehicleInteraction
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid VehicleId { get; set; }
    public InteractionType Type { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int DurationSeconds { get; set; } // Tiempo en la página
    public string? Source { get; set; } // Origen: search, recommendation, direct

    public VehicleInteraction() { }

    public VehicleInteraction(Guid userId, Guid vehicleId, InteractionType type, string? source = null)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        VehicleId = vehicleId;
        Type = type;
        Source = source;
        CreatedAt = DateTime.UtcNow;
    }
}

public enum InteractionType
{
    View = 1,
    Favorite = 2,
    Contact = 3,
    Share = 4,
    Compare = 5
}
