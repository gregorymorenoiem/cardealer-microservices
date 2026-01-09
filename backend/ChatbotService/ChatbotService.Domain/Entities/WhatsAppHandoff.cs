namespace ChatbotService.Domain.Entities;

/// <summary>
/// Registro de transferencia de lead a WhatsApp
/// </summary>
public class WhatsAppHandoff
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ConversationId { get; set; }
    public Guid UserId { get; set; }
    public Guid DealerId { get; set; }
    
    // Lead info
    public string UserName { get; set; } = string.Empty;
    public string UserPhone { get; set; } = string.Empty;
    public int LeadScore { get; set; }
    public LeadTemperature LeadTemperature { get; set; }
    
    // Contexto de la conversación
    public string ConversationSummary { get; set; } = string.Empty;
    public List<string> BuyingSignals { get; set; } = new();
    public string? VehicleDetails { get; set; }
    
    // WhatsApp
    public string DealerWhatsAppNumber { get; set; } = string.Empty;
    public string? WhatsAppMessageId { get; set; }
    public WhatsAppStatus Status { get; set; } = WhatsAppStatus.Pending;
    public string? ErrorMessage { get; set; }
    
    // Timestamps
    public DateTime InitiatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SentAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? ReadAt { get; set; }
    
    // Navigation
    public Conversation? Conversation { get; set; }
}

public enum WhatsAppStatus
{
    Pending,
    Sent,
    Delivered,
    Read,
    Failed
}
