using LeadScoringService.Domain.Entities;

namespace LeadScoringService.Domain.Interfaces;

/// <summary>
/// Motor de cálculo de lead scoring
/// </summary>
public interface ILeadScoringEngine
{
    /// <summary>
    /// Calcula el score total de un lead basado en sus acciones y comportamiento
    /// </summary>
    Task<int> CalculateLeadScoreAsync(Lead lead, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Calcula el score de engagement (0-40 puntos)
    /// Basado en: vistas, favoritos, comparaciones, shares
    /// </summary>
    int CalculateEngagementScore(Lead lead);
    
    /// <summary>
    /// Calcula el score de recency (0-30 puntos)
    /// Basado en: tiempo desde última interacción
    /// </summary>
    int CalculateRecencyScore(Lead lead);
    
    /// <summary>
    /// Calcula el score de intent (0-30 puntos)
    /// Basado en: contactos, test drive, financiamiento, ofertas
    /// </summary>
    int CalculateIntentScore(Lead lead);
    
    /// <summary>
    /// Determina la temperatura del lead según su score
    /// </summary>
    LeadTemperature DetermineTemperature(int score);
    
    /// <summary>
    /// Calcula la probabilidad de conversión (0-100%)
    /// </summary>
    decimal CalculateConversionProbability(Lead lead);
    
    /// <summary>
    /// Recalcula el score de todos los leads (batch)
    /// </summary>
    Task RecalculateAllScoresAsync(CancellationToken cancellationToken = default);
}
