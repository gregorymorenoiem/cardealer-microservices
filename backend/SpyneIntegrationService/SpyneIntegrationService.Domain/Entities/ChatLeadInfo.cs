using SpyneIntegrationService.Domain.Enums;

namespace SpyneIntegrationService.Domain.Entities;

/// <summary>
/// Lead information captured from a chat session
/// </summary>
public class ChatLeadInfo
{
    public Guid Id { get; set; }
    
    /// <summary>Parent chat session</summary>
    public Guid ChatSessionId { get; set; }
    
    /// <summary>Lead's name if provided</summary>
    public string? Name { get; set; }
    
    /// <summary>Lead's email if provided</summary>
    public string? Email { get; set; }
    
    /// <summary>Lead's phone if provided</summary>
    public string? Phone { get; set; }
    
    /// <summary>Type of interest expressed</summary>
    public LeadInterestType InterestType { get; set; }
    
    /// <summary>Preferred method of contact</summary>
    public string? PreferredContactMethod { get; set; }
    
    /// <summary>Lead's budget</summary>
    public decimal? Budget { get; set; }
    
    /// <summary>Interest in financing</summary>
    public bool FinancingInterest { get; set; }
    
    /// <summary>Trade-in information</summary>
    public string? TradeInInfo { get; set; }
    
    /// <summary>Additional notes/comments from lead</summary>
    public string? Notes { get; set; }
    
    /// <summary>Preferred contact time</summary>
    public string? PreferredContactTime { get; set; }
    
    /// <summary>Lead score calculated by AI (0-100)</summary>
    public int LeadScore { get; set; }
    
    /// <summary>Lead quality tier</summary>
    public LeadQualityTier QualityTier { get; set; }
    
    /// <summary>Whether lead was synced to CRM</summary>
    public bool SyncedToCrm { get; set; }
    
    /// <summary>CRM lead ID if synced</summary>
    public string? CrmLeadId { get; set; }
    
    public DateTime CapturedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ChatLeadInfo()
    {
        Id = Guid.NewGuid();
        CapturedAt = DateTime.UtcNow;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Calculate quality tier based on lead score
    /// </summary>
    public void CalculateQualityTier()
    {
        QualityTier = LeadScore switch
        {
            >= 80 => LeadQualityTier.Hot,
            >= 50 => LeadQualityTier.Warm,
            _ => LeadQualityTier.Cold
        };
    }

    public bool HasContactInfo() => !string.IsNullOrEmpty(Email) || !string.IsNullOrEmpty(Phone);
}

/// <summary>
/// Lead quality tier based on engagement and score
/// </summary>
public enum LeadQualityTier
{
    /// <summary>Low priority lead</summary>
    Cold = 0,
    
    /// <summary>Medium priority lead</summary>
    Warm = 1,
    
    /// <summary>High priority lead - follow up immediately</summary>
    Hot = 2
}
