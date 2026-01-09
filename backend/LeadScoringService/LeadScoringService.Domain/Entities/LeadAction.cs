namespace LeadScoringService.Domain.Entities;

/// <summary>
/// Representa una acción específica realizada por un lead
/// </summary>
public class LeadAction
{
    public Guid Id { get; set; }
    
    // Lead asociado
    public Guid LeadId { get; set; }
    public Lead Lead { get; set; } = null!;
    
    // Tipo de acción
    public LeadActionType ActionType { get; set; }
    
    // Descripción de la acción
    public string Description { get; set; } = string.Empty;
    
    // Metadata adicional (JSON)
    public string? Metadata { get; set; }
    
    // Impacto en el score (puede ser negativo)
    public int ScoreImpact { get; set; }
    
    // Timestamp
    public DateTime OccurredAt { get; set; }
    
    // IP y User Agent (para análisis)
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}

/// <summary>
/// Tipos de acciones que puede realizar un lead
/// </summary>
public enum LeadActionType
{
    // Visualización
    ViewListing = 0,           // Ver listing (+2 score)
    ViewImages = 1,            // Ver galería de imágenes (+1 score)
    ViewSellerProfile = 2,     // Ver perfil del vendedor (+3 score)
    
    // Engagement
    ClickPhone = 10,           // Click en teléfono (+5 score)
    ClickEmail = 11,           // Click en email (+5 score)
    ClickWhatsApp = 12,        // Click en WhatsApp (+7 score)
    SendMessage = 13,          // Enviar mensaje (+10 score)
    
    // Interés
    AddToFavorites = 20,       // Agregar a favoritos (+8 score)
    RemoveFromFavorites = 21,  // Quitar de favoritos (-5 score)
    ShareListing = 22,         // Compartir listing (+6 score)
    AddToComparison = 23,      // Agregar a comparación (+10 score)
    
    // Intención alta
    ScheduleTestDrive = 30,    // Agendar test drive (+20 score)
    RequestFinancing = 31,     // Solicitar financiamiento (+25 score)
    MakeOffer = 32,            // Hacer oferta (+30 score)
    
    // Conversión
    PurchaseCompleted = 40,    // Compra completada (+100 score)
    
    // Negativas
    ReportListing = 50,        // Reportar listing (-10 score)
    BlockSeller = 51,          // Bloquear vendedor (-50 score)
    
    // Sistema
    ScoreRecalculated = 99     // Score recalculado (0 score)
}
