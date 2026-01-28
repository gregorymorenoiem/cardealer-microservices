using ChatbotService.Domain.Entities;

namespace ChatbotService.Domain.Interfaces;

/// <summary>
/// Servicio para integración con OpenAI GPT-4o-mini
/// </summary>
public interface IOpenAIService
{
    /// <summary>
    /// Genera respuesta del chatbot con contexto de vehículo
    /// </summary>
    Task<string> GenerateResponseAsync(
        List<Message> conversationHistory, 
        string vehicleContext,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Analiza intención del mensaje del usuario
    /// </summary>
    Task<IntentAnalysis> AnalyzeIntentAsync(
        string message, 
        List<Message> conversationHistory,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Genera resumen de conversación para handoff
    /// </summary>
    Task<string> GenerateConversationSummaryAsync(
        List<Message> messages,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Extrae señales de compra del mensaje
    /// </summary>
    Task<List<BuyingSignal>> ExtractBuyingSignalsAsync(
        string message,
        CancellationToken cancellationToken = default);
}
