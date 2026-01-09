namespace ChatbotService.Domain.Entities;

/// <summary>
/// Represents a chat conversation between a user and the AI chatbot
/// </summary>
public class ChatConversation
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string? SessionId { get; set; }
    public Guid? VehicleId { get; set; }
    public string? VehicleContext { get; set; }
    public ConversationStatus Status { get; set; } = ConversationStatus.Active;
    public string? UserEmail { get; set; }
    public string? UserName { get; set; }
    public string? UserPhone { get; set; }
    public int MessageCount { get; set; }
    public int TotalTokensUsed { get; set; }
    public decimal EstimatedCost { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EndedAt { get; set; }
    public string? EndReason { get; set; }
    public LeadQualification LeadQualification { get; set; } = LeadQualification.Unknown;
    public double? LeadScore { get; set; }
    public string? Metadata { get; set; }

    // Navigation properties
    public virtual ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();

    // Domain methods
    public void AddMessage(ChatMessage message)
    {
        Messages.Add(message);
        MessageCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    public void EndConversation(string reason)
    {
        Status = ConversationStatus.Ended;
        EndedAt = DateTime.UtcNow;
        EndReason = reason;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateLeadScore(double score, LeadQualification qualification)
    {
        LeadScore = score;
        LeadQualification = qualification;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddTokenUsage(int tokens, decimal cost)
    {
        TotalTokensUsed += tokens;
        EstimatedCost += cost;
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum ConversationStatus
{
    Active,
    Paused,
    Ended,
    TransferredToAgent
}

public enum LeadQualification
{
    Unknown,
    Cold,
    Warm,
    Hot
}
