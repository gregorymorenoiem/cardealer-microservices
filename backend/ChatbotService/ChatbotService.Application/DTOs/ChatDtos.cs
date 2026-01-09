using ChatbotService.Domain.Entities;

namespace ChatbotService.Application.DTOs;

// ============================================
// Conversation DTOs
// ============================================

public record ConversationDto(
    Guid Id,
    Guid? UserId,
    string? SessionId,
    Guid? VehicleId,
    string Status,
    string? UserEmail,
    string? UserName,
    int MessageCount,
    string LeadQualification,
    double? LeadScore,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? EndedAt,
    List<MessageDto> Messages
);

public record ConversationSummaryDto(
    Guid Id,
    Guid? VehicleId,
    string Status,
    int MessageCount,
    string LeadQualification,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CreateConversationDto(
    Guid? UserId,
    string? SessionId,
    Guid? VehicleId,
    string? UserEmail,
    string? UserName,
    string? UserPhone,
    VehicleContextDto? VehicleContext
);

public record VehicleContextDto(
    Guid VehicleId,
    string Make,
    string Model,
    int Year,
    decimal Price,
    int Mileage,
    string? Transmission,
    string? FuelType,
    string? Color,
    string? Description,
    string? SellerName,
    string? Location
);

// ============================================
// Message DTOs
// ============================================

public record MessageDto(
    Guid Id,
    string Role,
    string Content,
    string Type,
    DateTime CreatedAt,
    TimeSpan? ResponseTime,
    List<QuickReplyDto>? SuggestedReplies
);

public record SendMessageDto(
    Guid ConversationId,
    string Content,
    VehicleContextDto? VehicleContext
);

public record SendMessageResponseDto(
    MessageDto UserMessage,
    MessageDto AssistantMessage,
    List<QuickReplyDto> SuggestedReplies,
    bool ShouldTransferToAgent,
    string? TransferReason
);

public record QuickReplyDto(
    string Id,
    string Label,
    string Value,
    string? Icon
);

// ============================================
// Chat Widget DTOs (for SignalR)
// ============================================

public record ChatWidgetStateDto(
    bool IsOpen,
    Guid? ActiveConversationId,
    bool IsTyping,
    int UnreadCount
);

public record TypingIndicatorDto(
    Guid ConversationId,
    bool IsTyping
);

// ============================================
// Analytics DTOs
// ============================================

public record ChatAnalyticsDto(
    int TotalConversations,
    int ActiveConversations,
    int TotalMessages,
    double AverageMessagesPerConversation,
    double AverageResponseTimeMs,
    int TotalTokensUsed,
    decimal TotalCost,
    Dictionary<string, int> ConversationsByStatus,
    Dictionary<string, int> LeadsByQualification
);

// ============================================
// Mapping Extensions
// ============================================

public static class ChatDtoMappings
{
    public static ConversationDto ToDto(this ChatConversation conversation)
    {
        return new ConversationDto(
            conversation.Id,
            conversation.UserId,
            conversation.SessionId,
            conversation.VehicleId,
            conversation.Status.ToString(),
            conversation.UserEmail,
            conversation.UserName,
            conversation.MessageCount,
            conversation.LeadQualification.ToString(),
            conversation.LeadScore,
            conversation.CreatedAt,
            conversation.UpdatedAt,
            conversation.EndedAt,
            conversation.Messages.Select(m => m.ToDto()).ToList()
        );
    }

    public static ConversationSummaryDto ToSummaryDto(this ChatConversation conversation)
    {
        return new ConversationSummaryDto(
            conversation.Id,
            conversation.VehicleId,
            conversation.Status.ToString(),
            conversation.MessageCount,
            conversation.LeadQualification.ToString(),
            conversation.CreatedAt,
            conversation.UpdatedAt
        );
    }

    public static MessageDto ToDto(this ChatMessage message)
    {
        return new MessageDto(
            message.Id,
            message.Role.ToString(),
            message.Content,
            message.Type.ToString(),
            message.CreatedAt,
            message.ResponseTime,
            null
        );
    }

    public static QuickReplyDto ToDto(this QuickReply reply)
    {
        return new QuickReplyDto(reply.Id, reply.Label, reply.Value, reply.Icon);
    }
}
