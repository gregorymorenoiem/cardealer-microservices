using LeadScoringService.Domain.Entities;
using LeadScoringService.Domain.Interfaces;

namespace LeadScoringService.Infrastructure.Services;

/// <summary>
/// Motor de cálculo de lead scoring con algoritmo basado en reglas
/// </summary>
public class LeadScoringEngine : ILeadScoringEngine
{
    private readonly ILeadRepository _leadRepository;

    public LeadScoringEngine(ILeadRepository leadRepository)
    {
        _leadRepository = leadRepository;
    }

    public async Task<int> CalculateLeadScoreAsync(Lead lead, CancellationToken cancellationToken = default)
    {
        // Calcular componentes del score
        lead.EngagementScore = CalculateEngagementScore(lead);
        lead.RecencyScore = CalculateRecencyScore(lead);
        lead.IntentScore = CalculateIntentScore(lead);

        // Score total (0-100)
        var totalScore = lead.EngagementScore + lead.RecencyScore + lead.IntentScore;
        
        // Asegurar que esté en el rango 0-100
        totalScore = Math.Max(0, Math.Min(100, totalScore));

        // Determinar temperatura
        lead.Temperature = DetermineTemperature(totalScore);

        // Calcular probabilidad de conversión
        lead.ConversionProbability = CalculateConversionProbability(lead);

        return totalScore;
    }

    public int CalculateEngagementScore(Lead lead)
    {
        // Engagement Score: 0-40 puntos
        // Basado en: vistas, favoritos, comparaciones, shares
        
        var score = 0;

        // Vistas (máximo 10 puntos)
        // 1-3 views = 3pts, 4-6 = 6pts, 7-10 = 8pts, 10+ = 10pts
        score += lead.ViewCount switch
        {
            >= 10 => 10,
            >= 7 => 8,
            >= 4 => 6,
            >= 1 => 3,
            _ => 0
        };

        // Favoritos (10 puntos si lo agregó)
        if (lead.FavoriteCount > 0)
        {
            score += 10;
        }

        // Comparaciones (8 puntos si comparó)
        if (lead.ComparisonCount > 0)
        {
            score += 8;
        }

        // Shares (6 puntos si compartió)
        if (lead.ShareCount > 0)
        {
            score += 6;
        }

        // Tiempo invertido (máximo 6 puntos)
        // 1-2 min = 2pts, 3-5 min = 4pts, 5+ min = 6pts
        var minutes = lead.TotalTimeSpentSeconds / 60;
        score += minutes switch
        {
            >= 5 => 6,
            >= 3 => 4,
            >= 1 => 2,
            _ => 0
        };

        return Math.Min(40, score);
    }

    public int CalculateRecencyScore(Lead lead)
    {
        // Recency Score: 0-30 puntos
        // Basado en: tiempo desde última interacción
        
        var hoursSinceLastInteraction = (DateTime.UtcNow - lead.LastInteractionAt).TotalHours;

        return hoursSinceLastInteraction switch
        {
            // Menos de 1 hora: SUPER HOT (30 pts)
            < 1 => 30,
            
            // 1-6 horas: muy reciente (25 pts)
            < 6 => 25,
            
            // 6-24 horas: reciente (20 pts)
            < 24 => 20,
            
            // 1-3 días: medio reciente (15 pts)
            < 72 => 15,
            
            // 3-7 días: frío (10 pts)
            < 168 => 10,
            
            // 7-14 días: muy frío (5 pts)
            < 336 => 5,
            
            // 14+ días: abandonado (0 pts)
            _ => 0
        };
    }

    public int CalculateIntentScore(Lead lead)
    {
        // Intent Score: 0-30 puntos
        // Basado en: acciones de alta intención (contacto, test drive, financing)
        
        var score = 0;

        // Test drive programado: MÁXIMA INTENCIÓN (15 puntos)
        if (lead.HasScheduledTestDrive)
        {
            score += 15;
        }

        // Financiamiento solicitado: ALTA INTENCIÓN (12 puntos)
        if (lead.HasRequestedFinancing)
        {
            score += 12;
        }

        // Contactos realizados (máximo 10 puntos)
        // 1 contacto = 4pts, 2-3 = 7pts, 4+ = 10pts
        score += lead.ContactCount switch
        {
            >= 4 => 10,
            >= 2 => 7,
            >= 1 => 4,
            _ => 0
        };

        return Math.Min(30, score);
    }

    public LeadTemperature DetermineTemperature(int score)
    {
        return score switch
        {
            >= 70 => LeadTemperature.Hot,      // 70-100: HOT 🔥
            >= 40 => LeadTemperature.Warm,     // 40-69: WARM 🟡
            _ => LeadTemperature.Cold          // 0-39: COLD ❄️
        };
    }

    public decimal CalculateConversionProbability(Lead lead)
    {
        // Modelo simplificado de probabilidad de conversión
        // En producción, esto sería un modelo ML entrenado
        
        var probability = 0m;

        // Base de score (50% del peso)
        probability += (decimal)lead.Score * 0.5m;

        // Factores adicionales
        
        // Test drive = +20% de probabilidad
        if (lead.HasScheduledTestDrive)
        {
            probability += 20m;
        }

        // Financing request = +15% de probabilidad
        if (lead.HasRequestedFinancing)
        {
            probability += 15m;
        }

        // Múltiples contactos = +10% de probabilidad
        if (lead.ContactCount >= 3)
        {
            probability += 10m;
        }

        // Engagement alto = +5% de probabilidad
        if (lead.EngagementScore >= 30)
        {
            probability += 5m;
        }

        // Recency alta = +5% de probabilidad
        if (lead.RecencyScore >= 25)
        {
            probability += 5m;
        }

        // Asegurar que esté en el rango 0-100
        return Math.Max(0, Math.Min(100, probability));
    }

    public async Task RecalculateAllScoresAsync(CancellationToken cancellationToken = default)
    {
        // Obtener todos los leads que no están convertidos o perdidos
        var leads = await _leadRepository.GetAllAsync(cancellationToken);
        
        var activeLeads = leads.Where(l => 
            l.Status != LeadStatus.Converted && 
            l.Status != LeadStatus.Lost &&
            l.Status != LeadStatus.Archived).ToList();

        foreach (var lead in activeLeads)
        {
            await CalculateLeadScoreAsync(lead, cancellationToken);
            await _leadRepository.UpdateAsync(lead, cancellationToken);
        }
    }
}
