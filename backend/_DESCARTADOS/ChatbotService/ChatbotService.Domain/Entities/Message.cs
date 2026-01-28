namespace ChatbotService.Domain.Entities;

public class Message
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ConversationId { get; set; }
    public MessageRole Role { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    // Metadata
    public MessageType Type { get; set; } = MessageType.Text;
    public Dictionary<string, object> Metadata { get; set; } = new();
    
    // Intent analysis (populated by AI)
    public string? DetectedIntent { get; set; }
    public List<string>? ExtractedSignals { get; set; }
    public double? SentimentScore { get; set; } // -1.0 to 1.0
    
    // Navigation
    public Conversation? Conversation { get; set; }
}

public enum MessageRole
{
    System,
    User,
    Assistant
}

public enum MessageType
{
    Text,
    Image,
    Document,
    VehicleCard,
    HandoffNotification,
    SystemNotification
}
