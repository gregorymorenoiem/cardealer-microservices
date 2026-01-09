namespace ChatbotService.Domain.Entities;

public class Conversation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string? UserName { get; set; }
    public string? UserEmail { get; set; }
    public string? UserPhone { get; set; }
    
    // Contexto del vehículo
    public Guid? VehicleId { get; set; }
    public string? VehicleTitle { get; set; }
    public decimal? VehiclePrice { get; set; }
    
    // Dealer info
    public Guid? DealerId { get; set; }
    public string? DealerName { get; set; }
    public string? DealerPhone { get; set; }
    public string? DealerWhatsApp { get; set; }
    
    // Conversación
    public List<Message> Messages { get; set; } = new();
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EndedAt { get; set; }
    public ConversationStatus Status { get; set; } = ConversationStatus.Active;
    
    // Lead Scoring
    public int LeadScore { get; set; } = 0; // 0-100
    public LeadTemperature LeadTemperature { get; set; } = LeadTemperature.Unknown;
    public List<string> BuyingSignals { get; set; } = new();
    public bool HasUrgency { get; set; }
    public bool HasBudget { get; set; }
    public bool HasTradeIn { get; set; }
    public bool WantsTestDrive { get; set; }
    public string? PurchaseTimeframe { get; set; }
    
    // Handoff
    public bool IsHandedOff { get; set; }
    public DateTime? HandedOffAt { get; set; }
    public HandoffMethod? HandoffMethod { get; set; }
    public string? HandoffNotes { get; set; }
    
    // Analytics
    public int MessageCount => Messages.Count;
    public TimeSpan Duration => (EndedAt ?? DateTime.UtcNow) - StartedAt;
    public double AvgResponseLength => Messages.Any() 
        ? Messages.Where(m => m.Role == MessageRole.User).Average(m => m.Content.Length) 
        : 0;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public enum ConversationStatus
{
    Active,
    Waiting,
    HandedOff,
    Ended,
    Abandoned
}

public enum LeadTemperature
{
    Unknown,
    Cold,     // 0-49
    Warm,     // 50-69
    WarmHot,  // 70-84
    Hot       // 85-100
}

public enum HandoffMethod
{
    WhatsApp,
    Phone,
    Email,
    InPlatform
}
