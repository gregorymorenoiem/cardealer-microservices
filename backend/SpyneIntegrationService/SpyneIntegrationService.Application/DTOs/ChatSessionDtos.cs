using SpyneIntegrationService.Domain.Entities;
using SpyneIntegrationService.Domain.Enums;

namespace SpyneIntegrationService.Application.DTOs;

/// <summary>
/// DTO for chat session (Fase 4 - Backend only, NOT consumed in frontend)
/// </summary>
public record ChatSessionDto
{
    public Guid Id { get; init; }
    public Guid? VehicleId { get; init; }
    public Guid? UserId { get; init; }
    public Guid? DealerId { get; init; }
    public string? SessionIdentifier { get; init; }
    public string? SpyneChatId { get; init; }
    public string Language { get; init; } = "es";
    public ChatSessionStatus Status { get; init; }
    public bool IsQualifiedLead { get; init; }
    public int? UserRating { get; init; }
    public int MessageCount { get; init; }
    public DateTime? LastMessageAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime StartedAt { get; init; }
    public DateTime? EndedAt { get; init; }
    public DateTime? ClosedAt { get; init; }
    public DateTime LastActivityAt { get; init; }
    public TimeSpan Duration { get; init; }
    public ChatLeadInfoDto? CapturedLead { get; init; }
    public ChatLeadInfoDto? LeadInfo { get; init; }
    public List<ChatMessageDto> Messages { get; init; } = new();
}

/// <summary>
/// DTO for chat message
/// </summary>
public record ChatMessageDto
{
    public Guid Id { get; init; }
    public Guid ChatSessionId { get; init; }
    public string Content { get; init; } = string.Empty;
    public ChatMessageRole Role { get; init; }
    public DateTime Timestamp { get; init; }
    public DateTime CreatedAt { get; init; }
    public ChatMessageMetadataDto? Metadata { get; init; }
}

/// <summary>
/// DTO for chat message metadata
/// </summary>
public record ChatMessageMetadataDto
{
    public double? Confidence { get; init; }
    public List<string> SuggestedQuestions { get; init; } = new();
    public bool LeadInfoDetected { get; init; }
    public string? DetectedIntent { get; init; }
    public string? Sentiment { get; init; }
}

/// <summary>
/// DTO for captured lead info
/// </summary>
public record ChatLeadInfoDto
{
    public Guid Id { get; init; }
    public string? Name { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? PreferredContactMethod { get; init; }
    public decimal? Budget { get; init; }
    public LeadInterestType InterestType { get; init; }
    public string? Notes { get; init; }
    public int LeadScore { get; init; }
    public LeadQualityTier QualityTier { get; init; }
    public DateTime CapturedAt { get; init; }
}

/// <summary>
/// Request to start a chat session
/// </summary>
public record StartChatSessionRequest
{
    public Guid? VehicleId { get; init; }
    public Guid? UserId { get; init; }
    public Guid? DealerId { get; init; }
    public string VisitorFingerprint { get; init; } = string.Empty;
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
    public string? ReferrerUrl { get; init; }
    public string? PageUrl { get; init; }
}

/// <summary>
/// Response when starting a chat session
/// </summary>
public record StartChatSessionResponse
{
    public Guid SessionId { get; init; }
    public string WelcomeMessage { get; init; } = string.Empty;
    public List<string> SuggestedQuestions { get; init; } = new();
}

/// <summary>
/// Request to send a chat message
/// </summary>
public record SendChatMessageRequest
{
    public Guid SessionId { get; init; }
    public string Message { get; init; } = string.Empty;
}

/// <summary>
/// Response from sending a chat message
/// </summary>
public record SendChatMessageResponse
{
    public ChatMessageDto UserMessage { get; init; } = null!;
    public ChatMessageDto AssistantMessage { get; init; } = null!;
    public bool LeadDetected { get; init; }
    public ChatLeadInfoDto? DetectedLead { get; init; }
}

/// <summary>
/// Response DTO for chat message (used by handlers)
/// </summary>
public record ChatMessageResponseDto
{
    public Guid SessionId { get; init; }
    public ChatMessageDto UserMessage { get; init; } = null!;
    public ChatMessageDto AssistantMessage { get; init; } = null!;
    public ChatMessageDto AssistantResponse { get; init; } = null!;
    public bool LeadDetected { get; init; }
    public bool IsQualifiedLead { get; init; }
    public ChatLeadInfoDto? CapturedLead { get; init; }
    public List<string> SuggestedFollowUps { get; init; } = new();
    public List<string> SuggestedActions { get; init; } = new();
}

/// <summary>
/// Summary DTO for chat session history
/// </summary>
public record ChatSessionSummaryDto
{
    public Guid Id { get; init; }
    public Guid SessionId { get; init; }
    public Guid? VehicleId { get; init; }
    public Guid? UserId { get; init; }
    public Guid? DealerId { get; init; }
    public ChatSessionStatus Status { get; init; }
    public int MessageCount { get; init; }
    public bool HasLead { get; init; }
    public bool IsQualifiedLead { get; init; }
    public int? LeadScore { get; init; }
    public int? UserRating { get; init; }
    public DateTime StartedAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? EndedAt { get; init; }
    public DateTime? ClosedAt { get; init; }
    public TimeSpan Duration { get; init; }
    public string? LastMessagePreview { get; init; }
    public ChatLeadInfoDto? LeadInfo { get; init; }
}

/// <summary>
/// Vehicle context DTO for chat sessions
/// </summary>
public record VehicleContextDto
{
    public Guid VehicleId { get; init; }
    public string Make { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public int Year { get; init; }
    public decimal Price { get; init; }
    public string? Currency { get; init; } = "DOP";
    public string? Color { get; init; }
    public int? Mileage { get; init; }
    public string? FuelType { get; init; }
    public string? Transmission { get; init; }
    public string? VehicleUrl { get; init; }
    public List<string> ImageUrls { get; init; } = new();
    public List<string> Features { get; init; } = new();
}

/// <summary>
/// Spyne lead info response from API
/// </summary>
public record SpyneLeadInfoResponse
{
    public bool Detected { get; init; }
    public string? Name { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Intent { get; init; }
    public int Score { get; init; }
    public string? Notes { get; init; }
}
