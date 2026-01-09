namespace LeadScoringService.Domain.Entities;

/// <summary>
/// Historial de cambios en el score de un lead
/// Para análisis de evolución temporal
/// </summary>
public class LeadScoreHistory
{
    public Guid Id { get; set; }
    
    // Lead asociado
    public Guid LeadId { get; set; }
    public Lead Lead { get; set; } = null!;
    
    // Score anterior y nuevo
    public int PreviousScore { get; set; }
    public int NewScore { get; set; }
    public int ScoreDelta { get; set; }
    
    // Temperatura anterior y nueva
    public LeadTemperature PreviousTemperature { get; set; }
    public LeadTemperature NewTemperature { get; set; }
    
    // Razón del cambio
    public string Reason { get; set; } = string.Empty;
    
    // Acción que causó el cambio (puede ser null si fue recalculación)
    public Guid? TriggeringActionId { get; set; }
    public LeadAction? TriggeringAction { get; set; }
    
    // Timestamp
    public DateTime ChangedAt { get; set; }
}
