using SpyneIntegrationService.Domain.Enums;

namespace SpyneIntegrationService.Domain.Entities;

/// <summary>
/// Represents a single message in a chat session
/// </summary>
public class ChatMessage
{
    public Guid Id { get; set; }
    
    /// <summary>Parent chat session</summary>
    public Guid ChatSessionId { get; set; }
    
    /// <summary>Message content</summary>
    public string Content { get; set; } = string.Empty;
    
    /// <summary>Who sent this message</summary>
    public ChatMessageRole Role { get; set; }
    
    /// <summary>Spyne message ID from API</summary>
    public string? SpyneMessageId { get; set; }
    
    /// <summary>When the message was sent</summary>
    public DateTime Timestamp { get; set; }
    
    /// <summary>Alias for Timestamp - when the message was created</summary>
    public DateTime CreatedAt 
    { 
        get => Timestamp; 
        set => Timestamp = value; 
    }
    
    /// <summary>Additional metadata from AI response</summary>
    public ChatMessageMetadata? Metadata { get; set; }

    public ChatMessage()
    {
        Id = Guid.NewGuid();
        Timestamp = DateTime.UtcNow;
    }

    public static ChatMessage FromUser(Guid sessionId, string content)
    {
        return new ChatMessage
        {
            ChatSessionId = sessionId,
            Content = content,
            Role = ChatMessageRole.User
        };
    }

    public static ChatMessage FromAssistant(Guid sessionId, string content, ChatMessageMetadata? metadata = null)
    {
        return new ChatMessage
        {
            ChatSessionId = sessionId,
            Content = content,
            Role = ChatMessageRole.Assistant,
            Metadata = metadata
        };
    }

    public static ChatMessage System(Guid sessionId, string content)
    {
        return new ChatMessage
        {
            ChatSessionId = sessionId,
            Content = content,
            Role = ChatMessageRole.System
        };
    }
}

/// <summary>
/// Metadata attached to AI assistant messages
/// </summary>
public class ChatMessageMetadata
{
    /// <summary>Confidence score of the response (0-1)</summary>
    public double? Confidence { get; set; }
    
    /// <summary>Suggested follow-up questions</summary>
    public List<string> SuggestedQuestions { get; set; } = new();
    
    /// <summary>Whether lead info was detected in conversation</summary>
    public bool LeadInfoDetected { get; set; }
    
    /// <summary>Intent detected from user message</summary>
    public string? DetectedIntent { get; set; }
    
    /// <summary>Sentiment of the conversation</summary>
    public string? Sentiment { get; set; }
    
    /// <summary>Vehicle features mentioned</summary>
    public List<string> MentionedFeatures { get; set; } = new();
}
