using ChatbotService.Domain.Entities;

namespace ChatbotService.Domain.Interfaces;

/// <summary>
/// Motor de calificación de leads basado en ML
/// </summary>
public interface ILeadScoringEngine
{
    /// <summary>
    /// Calcula score del lead basado en conversación completa
    /// </summary>
    Task<int> CalculateLeadScoreAsync(
        Conversation conversation,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Determina temperatura del lead (HOT/WARM/COLD)
    /// </summary>
    LeadTemperature DetermineLeadTemperature(int score);
    
    /// <summary>
    /// Recomienda acción basada en score
    /// </summary>
    string GetRecommendedAction(int score, LeadTemperature temperature);
    
    /// <summary>
    /// Detecta si el lead está listo para handoff
    /// </summary>
    bool ShouldTriggerHandoff(Conversation conversation);
}
