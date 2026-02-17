using System;
using System.Collections.Generic;

namespace RecommendationService.Domain.Entities;

/// <summary>
/// Representa una recomendación de vehículo para un usuario
/// </summary>
public class Recommendation
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid VehicleId { get; set; }
    public RecommendationType Type { get; set; }
    public double Score { get; set; } // 0.0 - 1.0
    public string Reason { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ViewedAt { get; set; }
    public DateTime? ClickedAt { get; set; }
    public bool IsRelevant { get; set; } = true;

    public Recommendation() { }

    public Recommendation(Guid userId, Guid vehicleId, RecommendationType type, double score, string reason)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        VehicleId = vehicleId;
        Type = type;
        Score = score;
        Reason = reason;
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkViewed()
    {
        ViewedAt = DateTime.UtcNow;
    }

    public void MarkClicked()
    {
        ClickedAt = DateTime.UtcNow;
    }

    public void MarkNotRelevant()
    {
        IsRelevant = false;
    }
}

public enum RecommendationType
{
    ForYou = 1,          // Personalizado para el usuario
    Similar = 2,         // Similar al vehículo actual
    AlsoViewed = 3,      // Usuarios también vieron
    Popular = 4,         // Popular en general
    Trending = 5,        // Tendencia actual
    RecentlyViewed = 6   // Basado en vistas recientes del usuario
}
