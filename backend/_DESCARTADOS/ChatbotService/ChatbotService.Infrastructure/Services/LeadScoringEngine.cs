using Microsoft.Extensions.Logging;
using ChatbotService.Domain.Entities;
using ChatbotService.Domain.Interfaces;

namespace ChatbotService.Infrastructure.Services;

/// <summary>
/// Motor de calificación de leads basado en reglas y ML
/// </summary>
public class LeadScoringEngine : ILeadScoringEngine
{
    private readonly ILogger<LeadScoringEngine> _logger;

    public LeadScoringEngine(ILogger<LeadScoringEngine> logger)
    {
        _logger = logger;
    }

    public async Task<int> CalculateLeadScoreAsync(
        Conversation conversation,
        CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask; // For async signature

        int score = 50; // Base score

        // Detect signals from message content
        var allContent = string.Join(" ", conversation.Messages.Select(m => m.Content?.ToLower() ?? ""));
        
        // Urgency signals (+25)
        bool hasUrgency = allContent.Contains("hoy") || allContent.Contains("today") || 
                          allContent.Contains("ahora") || allContent.Contains("now") ||
                          allContent.Contains("inmediato") || allContent.Contains("urgent") ||
                          allContent.Contains("necesito") || allContent.Contains("need");
        
        if (hasUrgency || conversation.HasUrgency)
        {
            if (allContent.Contains("today") || allContent.Contains("hoy") || 
                allContent.Contains("now") || allContent.Contains("ahora") || 
                allContent.Contains("inmediato"))
            {
                score += 25; // Maximum urgency
            }
            else if (allContent.Contains("semana") || allContent.Contains("week"))
            {
                score += 20;
            }
            else if (allContent.Contains("mes") || allContent.Contains("month"))
            {
                score += 15;
            }
            else
            {
                score += 10;
            }
        }

        // Financial readiness (+20)
        bool hasBudget = allContent.Contains("budget") || allContent.Contains("presupuesto") ||
                         allContent.Contains("ready") || allContent.Contains("listo") ||
                         allContent.Contains("cash") || allContent.Contains("financ");
        
        if (hasBudget || conversation.HasBudget)
        {
            score += 20;
        }

        // Trade-in (+15)
        bool hasTradeIn = allContent.Contains("trade") || allContent.Contains("intercambio") ||
                          allContent.Contains("cambio") || allContent.Contains("actual");
        
        if (hasTradeIn || conversation.HasTradeIn)
        {
            score += 15;
        }

        // Test drive request (+25)
        bool wantsTestDrive = allContent.Contains("test drive") || allContent.Contains("prueba") ||
                              allContent.Contains("probar") || allContent.Contains("manejo");
        
        if (wantsTestDrive || conversation.WantsTestDrive)
        {
            score += 25;
        }

        // Engagement level (message count and length)
        if (conversation.MessageCount > 10)
        {
            score += 10;
        }
        else if (conversation.MessageCount > 5)
        {
            score += 5;
        }

        // Average response length (engagement quality)
        if (conversation.AvgResponseLength > 50)
        {
            score += 5;
        }

        // Buying signals count
        var positiveSignals = conversation.BuyingSignals.Count(s => 
            !s.Contains("just_browsing") && 
            !s.Contains("solo_mirando") &&
            !s.Contains("no_urgency"));

        score += Math.Min(positiveSignals * 3, 15);

        // Negative signals from content
        bool justBrowsing = allContent.Contains("just browsing") || allContent.Contains("solo mirando") ||
                            allContent.Contains("just looking") || allContent.Contains("solo viendo");
        
        if (justBrowsing || conversation.BuyingSignals.Any(s => s.Contains("just_browsing") || s.Contains("solo_mirando")))
        {
            score -= 20; // Strong negative signal
        }

        // Conversation duration (very short = not engaged)
        if (conversation.Duration.TotalMinutes < 2 && conversation.MessageCount > 3)
        {
            score -= 10; // Too quick, not serious
        }

        // Clamp score between 0-100
        score = Math.Max(0, Math.Min(100, score));

        _logger.LogInformation(
            "Lead score calculated for conversation {ConversationId}: {Score} (Messages: {Messages}, Signals: {Signals})",
            conversation.Id, score, conversation.MessageCount, conversation.BuyingSignals.Count);

        return score;
    }

    public LeadTemperature DetermineLeadTemperature(int score)
    {
        return score switch
        {
            >= 85 => LeadTemperature.Hot,
            >= 70 => LeadTemperature.WarmHot,
            >= 50 => LeadTemperature.Warm,
            >= 30 => LeadTemperature.Cold,
            _ => LeadTemperature.Cold
        };
    }

    public string GetRecommendedAction(int score, LeadTemperature temperature)
    {
        return temperature switch
        {
            LeadTemperature.Hot => "🔥 Transferir a WhatsApp INMEDIATAMENTE. Lead listo para cerrar.",
            LeadTemperature.WarmHot => "🟠 Ofrecer test drive. Conectar con vendedor HOY.",
            LeadTemperature.Warm => "🟡 Nutrir con más información. Seguimiento en 24-48h.",
            LeadTemperature.Cold => "🔵 Email de seguimiento automático. Remarketing.",
            _ => "Monitorear y seguimiento regular."
        };
    }

    public bool ShouldTriggerHandoff(Conversation conversation)
    {
        // Trigger handoff if:
        // 1. Lead is HOT (score >= 85)
        // 2. User explicitly wants test drive
        // 3. User asks about financing and has urgency

        if (conversation.LeadScore >= 85)
            return true;

        if (conversation.WantsTestDrive)
            return true;

        if (conversation.HasUrgency && conversation.HasBudget)
            return true;

        // Check for specific high-intent signals
        var highIntentSignals = new[] 
        { 
            "wants_test_drive", 
            "ready_to_buy", 
            "financing_approved",
            "availability_check",
            "asking_for_dealer_contact"
        };

        if (conversation.BuyingSignals.Any(s => highIntentSignals.Any(h => s.Contains(h))))
            return true;

        return false;
    }
}
