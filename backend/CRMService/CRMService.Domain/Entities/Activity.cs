using CarDealer.Shared.MultiTenancy;

namespace CRMService.Domain.Entities;

/// <summary>
/// Represents an activity/interaction with a lead or deal.
/// </summary>
public class Activity : ITenantEntity
{
    public Guid Id { get; private set; }
    public Guid DealerId { get; set; }

    // Activity Details
    public ActivityType Type { get; private set; }
    public string Subject { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsCompleted { get; private set; }

    // Timing
    public DateTime? DueDate { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public int? DurationMinutes { get; private set; }

    // Relationships
    public Guid? LeadId { get; private set; }
    public Guid? DealId { get; private set; }
    public Guid? ContactId { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public Guid? AssignedToUserId { get; private set; }

    // Result/Outcome
    public string? Outcome { get; private set; }
    public ActivityPriority Priority { get; private set; }

    // Tracking
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Navigation properties
    public Lead? Lead { get; private set; }
    public Deal? Deal { get; private set; }

    private Activity() { } // EF Constructor

    public Activity(
        Guid dealerId,
        ActivityType type,
        string subject,
        Guid createdByUserId,
        Guid? leadId = null,
        Guid? dealId = null,
        Guid? contactId = null)
    {
        Id = Guid.NewGuid();
        DealerId = dealerId;
        Type = type;
        Subject = subject;
        CreatedByUserId = createdByUserId;
        LeadId = leadId;
        DealId = dealId;
        ContactId = contactId;
        Priority = ActivityPriority.Normal;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(string subject, string? description, ActivityPriority priority)
    {
        Subject = subject;
        Description = description;
        Priority = priority;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetDueDate(DateTime dueDate)
    {
        DueDate = dueDate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AssignTo(Guid userId)
    {
        AssignedToUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsCompleted(string? outcome = null, int? durationMinutes = null)
    {
        IsCompleted = true;
        CompletedAt = DateTime.UtcNow;
        Outcome = outcome;
        DurationMinutes = durationMinutes;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reopen()
    {
        IsCompleted = false;
        CompletedAt = null;
        Outcome = null;
        DurationMinutes = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsOverdue => !IsCompleted && DueDate.HasValue && DueDate.Value < DateTime.UtcNow;
}

public enum ActivityType
{
    Call,
    Email,
    Meeting,
    Task,
    Note,
    TestDrive,
    FollowUp,
    Presentation,
    Proposal,
    Other
}

public enum ActivityPriority
{
    Low,
    Normal,
    High,
    Urgent
}
