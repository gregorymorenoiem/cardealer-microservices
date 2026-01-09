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
