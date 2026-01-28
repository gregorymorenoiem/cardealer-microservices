namespace ChatbotService.Domain.Models;

/// <summary>
/// Resultado de la detección de intent en Dialogflow
/// </summary>
public class DialogflowDetectionResult
{
    public string? DetectedIntent { get; set; }
    public float ConfidenceScore { get; set; }
    public string? FulfillmentText { get; set; }
    public Dictionary<string, string> Parameters { get; set; } = new();
    public bool IsFallback { get; set; }
    public string? ResponseId { get; set; }
    public string? LanguageCode { get; set; }
    public string? QueryText { get; set; }
    public List<string>? OutputContexts { get; set; }
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Información del agente de Dialogflow
/// </summary>
public class DialogflowAgentInfo
{
    public string? DisplayName { get; set; }
    public string? DefaultLanguageCode { get; set; }
    public List<string> SupportedLanguageCodes { get; set; } = new();
    public string? TimeZone { get; set; }
    public string? Description { get; set; }
    public DateTime? LastTrainedAt { get; set; }
    public int IntentCount { get; set; }
    public int EntityTypeCount { get; set; }
    public bool IsHealthy { get; set; }
    public string? HealthMessage { get; set; }
}

/// <summary>
/// Reporte de salud del chatbot
/// </summary>
public class ChatbotHealthReport
{
    public Guid ChatbotConfigurationId { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public ChatbotHealthStatus OverallStatus { get; set; }
    public DialogflowHealthStatus DialogflowStatus { get; set; } = new();
    public DatabaseHealthStatus DatabaseStatus { get; set; } = new();
    public SystemMetrics SystemMetrics { get; set; } = new();
    public List<HealthAlert> Alerts { get; set; } = new();
    public Dictionary<string, object> AdditionalMetrics { get; set; } = new();
}

public enum ChatbotHealthStatus
{
    Healthy,
    Degraded,
    Unhealthy,
    Unknown
}

public class DialogflowHealthStatus
{
    public bool IsConnected { get; set; }
    public double ResponseTimeMs { get; set; }
    public DateTime? LastSuccessfulCall { get; set; }
    public int FailedCallsLast24h { get; set; }
    public string? LastError { get; set; }
    public DialogflowAgentInfo? AgentInfo { get; set; }
}

public class DatabaseHealthStatus
{
    public bool IsConnected { get; set; }
    public double ResponseTimeMs { get; set; }
    public long TotalSessions { get; set; }
    public long TotalMessages { get; set; }
    public long ActiveSessions { get; set; }
}

public class SystemMetrics
{
    public double CpuUsagePercent { get; set; }
    public double MemoryUsageMb { get; set; }
    public double AvailableMemoryMb { get; set; }
    public TimeSpan Uptime { get; set; }
    public int ActiveConnections { get; set; }
    public double RequestsPerMinute { get; set; }
    public double AverageResponseTimeMs { get; set; }
}

public class HealthAlert
{
    public string AlertType { get; set; } = string.Empty;
    public string Severity { get; set; } = "Info"; // Info, Warning, Error, Critical
    public string Message { get; set; } = string.Empty;
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, string> Details { get; set; } = new();
}

/// <summary>
/// Sugerencia de intent para auto-aprendizaje
/// </summary>
public class SuggestedIntent
{
    public string IntentName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<string> TrainingPhrases { get; set; } = new();
    public List<string> SuggestedResponses { get; set; } = new();
    public string Category { get; set; } = string.Empty;
    public int OccurrenceCount { get; set; }
    public float ConfidenceScore { get; set; }
    public bool RequiresReview { get; set; } = true;
    public string Source { get; set; } = "auto-learning"; // auto-learning, fallback-analysis, user-suggestion
}

/// <summary>
/// Sugerencia de auto-aprendizaje
/// </summary>
public class AutoLearningSuggestion
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ChatbotConfigurationId { get; set; }
    public SuggestionType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public object? Data { get; set; }
    public int Priority { get; set; } // 1-10, higher is more important
    public float ConfidenceScore { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsApplied { get; set; }
    public DateTime? AppliedAt { get; set; }
    public string? AppliedBy { get; set; }
}

public enum SuggestionType
{
    NewIntent,
    TrainingPhraseAddition,
    ResponseImprovement,
    EntityAddition,
    QuickResponseCreation,
    FallbackImprovement
}

/// <summary>
/// Reporte de análisis de costos
/// </summary>
public class CostAnalysisReport
{
    public Guid ChatbotConfigurationId { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    
    // Métricas de uso
    public int TotalInteractions { get; set; }
    public int FreeInteractionsUsed { get; set; }
    public int PaidInteractions { get; set; }
    public int QuickResponseInteractions { get; set; }
    public int DialogflowInteractions { get; set; }
    
    // Costos
    public decimal TotalCost { get; set; }
    public decimal CostPerInteraction { get; set; }
    public decimal ProjectedMonthlyCost { get; set; }
    public decimal CostSavingsFromQuickResponses { get; set; }
    public decimal CostSavingsFromAutoLearning { get; set; }
    
    // Comparaciones
    public decimal PreviousPeriodCost { get; set; }
    public decimal CostChangePercent { get; set; }
    
    // Desglose por categoría
    public Dictionary<string, int> InteractionsByCategory { get; set; } = new();
    public Dictionary<string, decimal> CostsByCategory { get; set; } = new();
    
    // Recomendaciones
    public List<CostRecommendation> Recommendations { get; set; } = new();
}

public class CostRecommendation
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal EstimatedSavings { get; set; }
    public string Priority { get; set; } = "Medium"; // Low, Medium, High, Critical
    public string ActionRequired { get; set; } = string.Empty;
}

/// <summary>
/// Resultado de sincronización de inventario
/// </summary>
public class InventorySyncResult
{
    public Guid ChatbotConfigurationId { get; set; }
    public DateTime SyncedAt { get; set; } = DateTime.UtcNow;
    public int TotalVehiclesProcessed { get; set; }
    public int NewVehiclesAdded { get; set; }
    public int VehiclesUpdated { get; set; }
    public int VehiclesRemoved { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public TimeSpan Duration { get; set; }
    public Dictionary<string, int> VehiclesByMake { get; set; } = new();
    public Dictionary<string, int> VehiclesByBodyType { get; set; } = new();
}

/// <summary>
/// Resultado de análisis de auto-aprendizaje
/// </summary>
public class AutoLearningAnalysisResult
{
    public Guid ChatbotConfigurationId { get; set; }
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
    public int ConversationsAnalyzed { get; set; }
    public int FallbacksAnalyzed { get; set; }
    public List<SuggestedIntent> SuggestedIntents { get; set; } = new();
    public List<AutoLearningSuggestion> Suggestions { get; set; } = new();
    public int AutoAppliedCount { get; set; }
    public int PendingReviewCount { get; set; }
    public decimal EstimatedCostSavings { get; set; }
}
