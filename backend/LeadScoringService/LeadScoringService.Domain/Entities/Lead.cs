namespace LeadScoringService.Domain.Entities;

/// <summary>
/// Representa un lead (usuario interesado en un vehículo)
/// con su score calculado y clasificación HOT/WARM/COLD
/// </summary>
public class Lead
{
    public Guid Id { get; set; }
    
    // Usuario del lead
    public Guid UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string UserFullName { get; set; } = string.Empty;
    public string? UserPhone { get; set; }
    
    // Vehículo de interés
    public Guid VehicleId { get; set; }
    public string VehicleTitle { get; set; } = string.Empty;
    public decimal VehiclePrice { get; set; }
    
    // Dealer propietario del vehículo
    public Guid DealerId { get; set; }
    public string DealerName { get; set; } = string.Empty;
    
    // Lead Score (0-100)
    public int Score { get; set; }
    
    // Clasificación del lead
    public LeadTemperature Temperature { get; set; }
    
    // Probabilidad de conversión (0-100%)
    public decimal ConversionProbability { get; set; }
    
    // Componentes del score
    public int EngagementScore { get; set; } // 0-40 puntos
    public int RecencyScore { get; set; } // 0-30 puntos
    public int IntentScore { get; set; } // 0-30 puntos
    
    // Acciones realizadas
    public int ViewCount { get; set; }
    public int ContactCount { get; set; }
    public int FavoriteCount { get; set; }
    public int ShareCount { get; set; }
    public int ComparisonCount { get; set; }
    public bool HasScheduledTestDrive { get; set; }
    public bool HasRequestedFinancing { get; set; }
    
    // Tiempo invertido
    public int TotalTimeSpentSeconds { get; set; }
    public int AverageSessionDurationSeconds { get; set; }
    
    // Estado del lead
    public LeadStatus Status { get; set; }
    public LeadSource Source { get; set; }
    
    // Fechas importantes
    public DateTime FirstInteractionAt { get; set; }
    public DateTime LastInteractionAt { get; set; }
    public DateTime? LastContactedAt { get; set; }
    public DateTime? ConvertedAt { get; set; }
    
    // Notas del dealer
    public string? DealerNotes { get; set; }
    
    // Metadata
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navegación
    public List<LeadAction> Actions { get; set; } = new();
    public List<LeadScoreHistory> ScoreHistory { get; set; } = new();
}

/// <summary>
/// Temperatura del lead basada en su score
/// </summary>
public enum LeadTemperature
{
    Cold = 0,   // Score 0-39 (baja probabilidad)
    Warm = 1,   // Score 40-69 (probabilidad media)
    Hot = 2     // Score 70-100 (alta probabilidad)
}

/// <summary>
/// Estado actual del lead en el funnel
/// </summary>
public enum LeadStatus
{
    New = 0,           // Recién creado
    Contacted = 1,     // Dealer ha contactado
    Qualified = 2,     // Lead verificado como válido
    Nurturing = 3,     // En proceso de seguimiento
    Negotiating = 4,   // Negociando precio/términos
    Converted = 5,     // Venta cerrada
    Lost = 6,          // Lead perdido
    Archived = 7       // Archivado
}

/// <summary>
/// Fuente de origen del lead
/// </summary>
public enum LeadSource
{
    OrganicSearch = 0,     // Búsqueda en el sitio
    DirectListing = 1,     // Acceso directo al listing
    EmailCampaign = 2,     // Campaña de email
    SocialMedia = 3,       // Redes sociales
    Referral = 4,          // Referido
    PaidAd = 5,            // Publicidad pagada
    Retargeting = 6,       // Retargeting
    Unknown = 99           // Desconocido
}
