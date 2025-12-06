using CarDealer.Shared.MultiTenancy;

namespace CRMService.Domain.Entities;

/// <summary>
/// Represents a potential customer (lead) in the CRM system.
/// </summary>
public class Lead : ITenantEntity
{
    public Guid Id { get; private set; }
    public Guid DealerId { get; set; }

    // Contact Information
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? Phone { get; private set; }
    public string? Company { get; private set; }
    public string? JobTitle { get; private set; }

    // Lead Details
    public LeadSource Source { get; private set; }
    public LeadStatus Status { get; private set; }
    public int Score { get; private set; }
    public decimal? EstimatedValue { get; private set; }

    // Assignment
    public Guid? AssignedToUserId { get; private set; }

    // Vehicle Interest (for car dealership context)
    public Guid? InterestedProductId { get; private set; }
    public string? InterestedProductNotes { get; private set; }

    // Tracking
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? ConvertedAt { get; private set; }
    public Guid? ConvertedToDealId { get; private set; }

    // Tags and Notes
    public List<string> Tags { get; private set; } = new();
    public string? Notes { get; private set; }

    // Navigation properties
    public ICollection<Activity> Activities { get; private set; } = new List<Activity>();

    private Lead() { } // EF Constructor

    public Lead(
        Guid dealerId,
        string firstName,
        string lastName,
        string email,
        LeadSource source,
        string? phone = null,
        string? company = null)
    {
        Id = Guid.NewGuid();
        DealerId = dealerId;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Phone = phone;
        Company = company;
        Source = source;
        Status = LeadStatus.New;
        Score = 0;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public string FullName => $"{FirstName} {LastName}";

    public void UpdateContactInfo(string firstName, string lastName, string email, string? phone, string? company, string? jobTitle)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Phone = phone;
        Company = company;
        JobTitle = jobTitle;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateStatus(LeadStatus status)
    {
        Status = status;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateScore(int score)
    {
        Score = Math.Max(0, Math.Min(100, score));
        UpdatedAt = DateTime.UtcNow;
    }

    public void AssignTo(Guid userId)
    {
        AssignedToUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetInterest(Guid productId, string? notes = null)
    {
        InterestedProductId = productId;
        InterestedProductNotes = notes;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetEstimatedValue(decimal value)
    {
        EstimatedValue = value;
        UpdatedAt = DateTime.UtcNow;
    }

    public Deal ConvertToDeal(Guid pipelineId, Guid stageId, string dealTitle, decimal value)
    {
        if (Status == LeadStatus.Converted)
            throw new InvalidOperationException("Lead is already converted");

        var deal = new Deal(
            DealerId,
            dealTitle,
            value,
            pipelineId,
            stageId,
            Id);

        Status = LeadStatus.Converted;
        ConvertedAt = DateTime.UtcNow;
        ConvertedToDealId = deal.Id;
        UpdatedAt = DateTime.UtcNow;

        return deal;
    }

    public void AddTag(string tag)
    {
        if (!Tags.Contains(tag))
        {
            Tags.Add(tag);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void RemoveTag(string tag)
    {
        if (Tags.Remove(tag))
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void SetNotes(string? notes)
    {
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum LeadSource
{
    Website,
    Referral,
    SocialMedia,
    Advertisement,
    TradeShow,
    ColdCall,
    Email,
    WalkIn,
    Partner,
    Other
}

public enum LeadStatus
{
    New,
    Contacted,
    Qualified,
    Unqualified,
    Nurturing,
    Converted,
    Lost
}
