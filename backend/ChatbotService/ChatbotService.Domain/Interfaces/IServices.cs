using ChatbotService.Domain.Entities;
using ChatbotService.Domain.Models;

namespace ChatbotService.Domain.Interfaces;

/// <summary>
/// Servicio de integración con Dialogflow ES
/// </summary>
public interface IDialogflowService
{
    /// <summary>
    /// Detecta la intención del usuario en base al mensaje
    /// </summary>
    Task<DialogflowDetectionResult> DetectIntentAsync(string sessionId, string text, string? languageCode = null, CancellationToken ct = default);
    
    /// <summary>
    /// Obtiene información del agente de Dialogflow
    /// </summary>
    Task<DialogflowAgentInfo> GetAgentInfoAsync(CancellationToken ct = default);
    
    /// <summary>
    /// Entrena el agente con los cambios
    /// </summary>
    Task<bool> TrainAgentAsync(CancellationToken ct = default);
    
    /// <summary>
    /// Crea un nuevo intent en Dialogflow
    /// </summary>
    Task<bool> CreateIntentAsync(SuggestedIntent intent, CancellationToken ct = default);
    
    /// <summary>
    /// Agrega frases de entrenamiento a un intent existente
    /// </summary>
    Task<bool> AddTrainingPhrasesAsync(string intentName, IEnumerable<string> phrases, CancellationToken ct = default);
    
    /// <summary>
    /// Verifica conectividad con Dialogflow
    /// </summary>
    Task<bool> TestConnectivityAsync(CancellationToken ct = default);
    
    /// <summary>
    /// Obtiene el estado de salud del servicio Dialogflow
    /// </summary>
    Task<DialogflowHealthStatus> GetHealthStatusAsync(CancellationToken ct = default);
}

/// <summary>
/// Servicio de sincronización de inventario de vehículos
/// </summary>
public interface IInventorySyncService
{
    /// <summary>
    /// Sincroniza vehículos desde VehiclesSaleService
    /// </summary>
    Task<InventorySyncResult> SyncVehiclesAsync(Guid configurationId, CancellationToken ct = default);
    
    /// <summary>
    /// Obtiene la cantidad de vehículos activos
    /// </summary>
    Task<int> GetActiveVehicleCountAsync(Guid configurationId, CancellationToken ct = default);
    
    /// <summary>
    /// Busca vehículos por texto
    /// </summary>
    Task<IEnumerable<ChatbotVehicle>> SearchVehiclesAsync(Guid configurationId, string query, int limit = 5, CancellationToken ct = default);
    
    /// <summary>
    /// Obtiene vehículos destacados
    /// </summary>
    Task<IEnumerable<ChatbotVehicle>> GetFeaturedVehiclesAsync(Guid configurationId, int limit = 5, CancellationToken ct = default);
    
    /// <summary>
    /// Obtiene vehículos por rango de precio
    /// </summary>
    Task<IEnumerable<ChatbotVehicle>> GetVehiclesByPriceRangeAsync(Guid configurationId, decimal minPrice, decimal maxPrice, int limit = 10, CancellationToken ct = default);
}

/// <summary>
/// Servicio de auto-aprendizaje del chatbot
/// </summary>
public interface IAutoLearningService
{
    /// <summary>
    /// Analiza conversaciones y sugiere mejoras
    /// </summary>
    Task<AutoLearningAnalysisResult> AnalyzeAndSuggestAsync(Guid configurationId, CancellationToken ct = default);
    
    /// <summary>
    /// Aplica una sugerencia de auto-aprendizaje
    /// </summary>
    Task<bool> ApplySuggestionAsync(AutoLearningSuggestion suggestion, CancellationToken ct = default);
    
    /// <summary>
    /// Aplica automáticamente sugerencias con alta confianza
    /// </summary>
    Task<int> AutoApplyHighConfidenceSuggestionsAsync(Guid configurationId, float minConfidence = 0.85f, CancellationToken ct = default);
    
    /// <summary>
    /// Obtiene intents sugeridos
    /// </summary>
    Task<IEnumerable<SuggestedIntent>> GetSuggestedIntentsAsync(Guid configurationId, int limit = 10, CancellationToken ct = default);
    
    /// <summary>
    /// Registra un fallback para análisis
    /// </summary>
    Task RecordFallbackAsync(Guid configurationId, string userMessage, string? attemptedIntent, float? confidence, CancellationToken ct = default);
}

/// <summary>
/// Servicio de monitoreo de salud del chatbot
/// </summary>
public interface IHealthMonitoringService
{
    /// <summary>
    /// Genera un reporte de salud completo
    /// </summary>
    Task<ChatbotHealthReport> GenerateHealthReportAsync(Guid configurationId, CancellationToken ct = default);
    
    /// <summary>
    /// Verifica la salud de Dialogflow
    /// </summary>
    Task<bool> CheckDialogflowHealthAsync(CancellationToken ct = default);
    
    /// <summary>
    /// Verifica la salud de la base de datos
    /// </summary>
    Task<bool> CheckDatabaseHealthAsync(CancellationToken ct = default);
    
    /// <summary>
    /// Obtiene métricas del sistema
    /// </summary>
    Task<SystemMetrics> GetSystemMetricsAsync(CancellationToken ct = default);
    
    /// <summary>
    /// Obtiene alertas activas
    /// </summary>
    Task<IEnumerable<HealthAlert>> GetActiveAlertsAsync(Guid configurationId, CancellationToken ct = default);
}

/// <summary>
/// Servicio de reportes y análisis de costos
/// </summary>
public interface IReportingService
{
    /// <summary>
    /// Genera reporte de análisis de costos
    /// </summary>
    Task<CostAnalysisReport> GenerateCostReportAsync(Guid configurationId, DateTime startDate, DateTime endDate, CancellationToken ct = default);
    
    /// <summary>
    /// Envía reporte por email
    /// </summary>
    Task<bool> SendReportByEmailAsync(CostAnalysisReport report, string recipientEmail, CancellationToken ct = default);
    
    /// <summary>
    /// Genera datos para dashboard
    /// </summary>
    Task<object> GenerateDashboardDataAsync(Guid configurationId, CancellationToken ct = default);
    
    /// <summary>
    /// Obtiene tendencias de uso
    /// </summary>
    Task<object> GetUsageTrendsAsync(Guid configurationId, int days = 30, CancellationToken ct = default);
}
