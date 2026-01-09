namespace ChatbotService.Domain.Entities;

/// <summary>
/// Represents a single message in a chat conversation
/// </summary>
public class ChatMessage
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public MessageRole Role { get; set; }
    public string Content { get; set; } = string.Empty;
    public MessageType Type { get; set; } = MessageType.Text;
    public int? TokenCount { get; set; }
    public TimeSpan? ResponseTime { get; set; }
    public string? IntentDetected { get; set; }
    public double? SentimentScore { get; set; }
    public string? Metadata { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public virtual ChatConversation? Conversation { get; set; }

    // Factory methods
    public static ChatMessage CreateUserMessage(Guid conversationId, string content)
    {
        return new ChatMessage
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,
            Role = MessageRole.User,
            Content = content,
            Type = MessageType.Text,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static ChatMessage CreateAssistantMessage(Guid conversationId, string content, int tokenCount, TimeSpan responseTime)
    {
        return new ChatMessage
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,
            Role = MessageRole.Assistant,
            Content = content,
            Type = MessageType.Text,
            TokenCount = tokenCount,
            ResponseTime = responseTime,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static ChatMessage CreateSystemMessage(Guid conversationId, string content)
    {
        return new ChatMessage
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,
            Role = MessageRole.System,
            Content = content,
            Type = MessageType.System,
            CreatedAt = DateTime.UtcNow
        };
    }
}

public enum MessageRole
{
    User,
    Assistant,
    System
}

public enum MessageType
{
    Text,
    Image,
    System,
    Action,
    QuickReply
}
