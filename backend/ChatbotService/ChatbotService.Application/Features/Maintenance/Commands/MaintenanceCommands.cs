using MediatR;
using ChatbotService.Application.DTOs;
using ChatbotService.Domain.Enums;

namespace ChatbotService.Application.Features.Maintenance.Commands;

/// <summary>
/// Comando para ejecutar tarea de mantenimiento manualmente
/// </summary>
public record RunMaintenanceTaskCommand(
    Guid TaskId,
    bool Force = false,
    string? Reason = null
) : IRequest<MaintenanceTaskResultDto>;

/// <summary>
/// Comando para sincronizar inventario
/// </summary>
public record SyncInventoryCommand(Guid ConfigurationId) : IRequest<InventorySyncResultDto>;

public record InventorySyncResultDto(
    bool Success,
    int TotalVehicles,
    int AddedVehicles,
    int UpdatedVehicles,
    int RemovedVehicles,
    int FailedVehicles,
    List<string> Errors,
    int ExecutionTimeMs
);

/// <summary>
/// Comando para ejecutar análisis de aprendizaje automático
/// </summary>
public record RunAutoLearningCommand(Guid ConfigurationId) : IRequest<AutoLearningResultDto>;

public record AutoLearningResultDto(
    int ConversationsAnalyzed,
    int NewUnansweredQuestions,
    int ExistingQuestionsUpdated,
    int SuggestionsGenerated,
    int ExecutionTimeMs
);

/// <summary>
/// Comando para generar reporte
/// </summary>
public record GenerateReportCommand(
    Guid ConfigurationId,
    ReportType ReportType,
    DateTime? Date = null,
    bool SendByEmail = false,
    List<string>? Recipients = null
) : IRequest<GenerateReportResultDto>;

public enum ReportType
{
    Daily,
    Weekly,
    Monthly
}

public record GenerateReportResultDto(
    bool Success,
    string ReportType,
    string? ReportContent,
    string? ReportUrl,
    bool EmailSent,
    int ExecutionTimeMs
);

/// <summary>
/// Comando para crear/actualizar configuración del chatbot
/// </summary>
public record CreateOrUpdateConfigurationCommand(
    Guid? Id,
    Guid? DealerId,
    string Name,
    string LlmServerUrl,
    string LlmModelId,
    string LlmLanguageCode,
    string? LlmSystemPrompt,
    int MaxInteractionsPerSession,
    int MaxInteractionsPerUserPerDay,
    int MaxInteractionsPerUserPerMonth,
    int MaxGlobalInteractionsPerDay,
    int MaxGlobalInteractionsPerMonth,
    string BotName,
    string? BotAvatarUrl,
    string WelcomeMessage,
    string? QuickRepliesJson,
    bool EnableAutoInventorySync,
    int InventorySyncIntervalMinutes,
    bool EnableAutoReports,
    bool EnableAutoLearning,
    bool EnableHealthMonitoring
) : IRequest<ChatbotConfigurationDto>;

/// <summary>
/// Comando para crear respuesta rápida
/// </summary>
public record CreateQuickResponseCommand(
    Guid ConfigurationId,
    string Category,
    string Name,
    List<string> Triggers,
    string Response,
    List<QuickReplyDto>? QuickReplies,
    int Priority,
    bool BypassLlm
) : IRequest<QuickResponseDto>;
