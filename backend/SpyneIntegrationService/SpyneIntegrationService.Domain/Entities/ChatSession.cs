using SpyneIntegrationService.Domain.Enums;

namespace SpyneIntegrationService.Domain.Entities;

/// <summary>
/// Represents a chat session with Spyne Vini AI
/// NOTE: Backend implementation ready but NOT consumed in frontend for this phase
/// </summary>
public class ChatSession
{
    public Guid Id { get; set; }
    
    /// <summary>Optional vehicle context for the chat</summary>
    public Guid? VehicleId { get; set; }
    
    /// <summary>Optional user ID if authenticated</summary>
    public Guid? UserId { get; set; }
    
    /// <summary>Dealer ID for routing leads</summary>
    public Guid? DealerId { get; set; }
    
    /// <summary>Spyne session token</summary>
    public string? SpyneSessionToken { get; set; }
    
    /// <summary>Spyne Chat ID from API</summary>
    public string? SpyneChatId { get; set; }
    
    /// <summary>Session identifier for anonymous users</summary>
    public string SessionIdentifier { get; set; } = string.Empty;
    
    /// <summary>Visitor fingerprint for anonymous users</summary>
    public string VisitorFingerprint { get; set; } = string.Empty;
    
    /// <summary>Current session status</summary>
    public ChatSessionStatus Status { get; set; }
    
    /// <summary>Chat language (es, en)</summary>
    public string Language { get; set; } = "es";
    
    /// <summary>Vehicle context JSON</summary>
    public string? VehicleContextJson { get; set; }
    
    /// <summary>Whether this is a qualified lead</summary>
    public bool IsQualifiedLead { get; set; }
    
    /// <summary>User rating (1-5)</summary>
    public int? UserRating { get; set; }
    
    /// <summary>Chat messages in this session</summary>
    public List<ChatMessage> Messages { get; set; } = new();
    
    /// <summary>Lead information captured from chat</summary>
    public ChatLeadInfo? LeadInfo { get; set; }
    
    /// <summary>Captured lead information if any (alias for LeadInfo)</summary>
    public ChatLeadInfo? CapturedLead 
    { 
        get => LeadInfo; 
        set => LeadInfo = value; 
    }
    
    /// <summary>IP address of visitor</summary>
    public string? IpAddress { get; set; }
    
    /// <summary>User agent string</summary>
    public string? UserAgent { get; set; }
    
    /// <summary>Referrer URL</summary>
    public string? ReferrerUrl { get; set; }
    
    /// <summary>Page URL where chat was initiated</summary>
    public string? PageUrl { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public DateTime LastActivityAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    /// <summary>Reason for closing the session</summary>
    public string? ClosureReason { get; set; }
    
    /// <summary>User feedback text</summary>
    public string? UserFeedback { get; set; }

    public ChatSession()
    {
        Id = Guid.NewGuid();
        SessionIdentifier = Guid.NewGuid().ToString();
        Status = ChatSessionStatus.Active;
        CreatedAt = DateTime.UtcNow;
        StartedAt = DateTime.UtcNow;
        LastActivityAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddMessage(ChatMessage message)
    {
        Messages.Add(message);
        LastActivityAt = DateTime.UtcNow;
    }

    public void End()
    {
        Status = ChatSessionStatus.Ended;
        EndedAt = DateTime.UtcNow;
        ClosedAt = DateTime.UtcNow;
    }

    public void CaptureLead(ChatLeadInfo leadInfo)
    {
        LeadInfo = leadInfo;
        CapturedLead = leadInfo;
        IsQualifiedLead = true;
        Status = ChatSessionStatus.LeadCaptured;
    }

    public void MarkExpired()
    {
        Status = ChatSessionStatus.Expired;
        EndedAt = DateTime.UtcNow;
        ClosedAt = DateTime.UtcNow;
    }

    public bool IsActive() => Status == ChatSessionStatus.Active;

    public TimeSpan GetDuration()
    {
        var endTime = EndedAt ?? DateTime.UtcNow;
        return endTime - StartedAt;
    }
}
