namespace UserBehaviorService.Domain.Entities;

/// <summary>
/// Perfil de comportamiento de un usuario basado en sus acciones
/// </summary>
public class UserBehaviorProfile
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    
    // Segmentación
    public string UserSegment { get; set; } = "Unknown"; // "SeriousBuyer", "Browser", "Researcher", "Dealer", "Tire Kicker"
    public double EngagementScore { get; set; } // 0-100
    public double PurchaseIntentScore { get; set; } // 0-100
    
    // Preferencias Inferidas (desde comportamiento)
    public List<string> PreferredMakes { get; set; } = new(); // ["Toyota", "Honda"]
    public List<string> PreferredModels { get; set; } = new(); // ["Corolla", "Civic"]
    public List<int> PreferredYears { get; set; } = new(); // [2020, 2021, 2022]
    public decimal? PreferredPriceMin { get; set; }
    public decimal? PreferredPriceMax { get; set; }
    public List<string> PreferredBodyTypes { get; set; } = new(); // ["Sedan", "SUV"]
    public List<string> PreferredFuelTypes { get; set; } = new(); // ["Gasoline", "Electric"]
    public List<string> PreferredTransmissions { get; set; } = new(); // ["Automatic"]
    
    // Métricas de Comportamiento
    public int TotalSearches { get; set; }
    public int TotalVehicleViews { get; set; }
    public int TotalContactRequests { get; set; }
    public int TotalFavorites { get; set; }
    public int TotalComparisons { get; set; }
    public int TotalSessions { get; set; }
    public TimeSpan TotalTimeSpent { get; set; }
    
    // Historial Reciente
    public List<string> RecentSearchQueries { get; set; } = new(); // Últimas 10 búsquedas
    public List<Guid> RecentVehicleViews { get; set; } = new(); // Últimos 20 vehículos vistos
    public DateTime? LastActivityAt { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Métodos de negocio
    public bool IsHighIntentBuyer() => PurchaseIntentScore >= 70;
    public bool IsActiveRecently() => LastActivityAt.HasValue && (DateTime.UtcNow - LastActivityAt.Value).TotalDays <= 7;
    public bool HasStrongPreferences() => PreferredMakes.Count >= 2 || PreferredModels.Count >= 2;
}

/// <summary>
/// Acción individual de usuario (para historial detallado)
/// </summary>
public class UserAction
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string ActionType { get; set; } = string.Empty; // "Search", "VehicleView", "Contact", "Favorite", "Share"
    public string ActionDetails { get; set; } = string.Empty; // JSON con detalles
    public Guid? RelatedVehicleId { get; set; }
    public string? SearchQuery { get; set; }
    public DateTime Timestamp { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty; // "Desktop", "Mobile", "Tablet"
}

/// <summary>
/// Segmento de usuario con características comunes
/// </summary>
public class UserSegment
{
    public string SegmentName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double MinEngagementScore { get; set; }
    public double MaxEngagementScore { get; set; }
    public double MinPurchaseIntentScore { get; set; }
    public double MaxPurchaseIntentScore { get; set; }
    public int MinActions { get; set; }
    public string Color { get; set; } = "#gray"; // Para UI
    public string Icon { get; set; } = "user"; // Para UI
}
