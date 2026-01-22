using ChatbotService.Domain.Entities;

namespace ChatbotService.Application.DTOs;

public record ConversationDto(
    Guid Id,
    Guid UserId,
    string? UserName,
    string? UserEmail,
    string? UserPhone,
    Guid? VehicleId,
    string? VehicleTitle,
    decimal? VehiclePrice,
    Guid? DealerId,
    string? DealerName,
    ConversationStatus Status,
    int LeadScore,
    LeadTemperature LeadTemperature,
    List<string> BuyingSignals,
    bool IsHandedOff,
    DateTime StartedAt,
    DateTime? EndedAt,
    int MessageCount,
    TimeSpan Duration
);

public record MessageDto(
    Guid Id,
    Guid ConversationId,
    MessageRole Role,
    string Content,
    MessageType Type,
    DateTime Timestamp,
    string? DetectedIntent,
    List<string>? ExtractedSignals,
    double? SentimentScore
);

public record IntentAnalysisDto(
    Guid Id,
    IntentType IntentType,
    double Confidence,
    List<BuyingSignal> BuyingSignals,
    string? ExtractedUrgency,
    string? ExtractedBudget,
    bool? HasTradeIn,
    int PartialScore,
    DateTime AnalyzedAt
);

public record WhatsAppHandoffDto(
    Guid Id,
    Guid ConversationId,
    string UserName,
    string UserPhone,
    int LeadScore,
    LeadTemperature LeadTemperature,
    string ConversationSummary,
    List<string> BuyingSignals,
    string DealerWhatsAppNumber,
    WhatsAppStatus Status,
    DateTime InitiatedAt,
    DateTime? SentAt,
    DateTime? DeliveredAt
);

public record ChatbotStatsDto(
    int TotalConversations,
    int ActiveConversations,
    int HandedOffConversations,
    int HotLeads,
    int WarmLeads,
    int ColdLeads,
    double AverageLeadScore,
    double ConversionRate,
    TimeSpan AverageConversationDuration,
    Dictionary<string, int> TopBuyingSignals
);

// Request DTOs
public record StartConversationRequest(
    Guid UserId,
    string? UserName,
    string? UserEmail,
    string? UserPhone,
    Guid? VehicleId
);

public record SendMessageRequest(
    Guid ConversationId,
    string Message
);

public record HandoffToWhatsAppRequest(
    Guid ConversationId,
    string? CustomMessage
);

// Response DTOs
public record ChatbotResponseDto(
    string Response,
    MessageDto Message,
    ConversationDto UpdatedConversation,
    bool ShouldHandoff,
    string? HandoffReason
);

// Additional DTOs needed for API
public record CreateConversationDto(
    Guid UserId,
    string? SessionId,
    Guid? VehicleId,
    string? UserEmail,
    string? UserName,
    string? UserPhone,
    VehicleContextDto? VehicleContext
);

public record VehicleContextDto(
    Guid VehicleId,
    string? Make,
    string? Model,
    int? Year,
    decimal? Price,
    string? Condition,
    string? ImageUrl
);

public record SendMessageDto(
    Guid ConversationId,
    string Content,
    VehicleContextDto? VehicleContext
);

public record SendMessageResponseDto(
    MessageDto UserMessage,
    MessageDto BotMessage,
    ConversationDto Conversation,
    bool ShouldHandoff,
    string? HandoffReason,
    int LeadScore
);

public record ConversationSummaryDto(
    Guid Id,
    Guid UserId,
    string? UserName,
    Guid? VehicleId,
    string? VehicleTitle,
    int MessageCount,
    LeadTemperature LeadTemperature,
    DateTime StartedAt,
    string? LastMessage
);

public record ChatAnalyticsDto(
    int TotalConversations,
    int ActiveConversations,
    int HandedOffConversations,
    int HotLeads,
    int WarmLeads,
    int ColdLeads,
    double AverageLeadScore,
    double ConversionRate,
    TimeSpan AverageConversationDuration,
    Dictionary<string, int> TopIntents,
    Dictionary<string, int> TopBuyingSignals,
    List<ConversationSummaryDto> RecentHotLeads
);

// SignalR DTOs
public record TypingIndicatorDto(
    Guid ConversationId,
    bool IsTyping
);
