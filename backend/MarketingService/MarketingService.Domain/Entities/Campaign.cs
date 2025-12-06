using CarDealer.Shared.MultiTenancy;

namespace MarketingService.Domain.Entities;

public enum CampaignType
{
    Email,
    Sms,
    WhatsApp,
    Social,
    Push,
    Mixed
}

public enum CampaignStatus
{
    Draft,
    Scheduled,
    Running,
    Paused,
    Completed,
    Cancelled
}

public class Campaign : ITenantEntity
{
    public Guid Id { get; private set; }
    public Guid DealerId { get; set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    public CampaignType Type { get; private set; }
    public CampaignStatus Status { get; private set; }

    public Guid? AudienceId { get; private set; }
    public Guid? TemplateId { get; private set; }

    public DateTime? ScheduledDate { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    public int TotalRecipients { get; private set; }
    public int SentCount { get; private set; }
    public int DeliveredCount { get; private set; }
    public int OpenedCount { get; private set; }
    public int ClickedCount { get; private set; }
    public int BouncedCount { get; private set; }
    public int UnsubscribedCount { get; private set; }

    public decimal Budget { get; private set; }
    public decimal SpentAmount { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public Guid CreatedBy { get; private set; }

    private Campaign() { }

    public Campaign(
        Guid dealerId,
        string name,
        CampaignType type,
        Guid createdBy,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        Id = Guid.NewGuid();
        DealerId = dealerId;
        Name = name;
        Type = type;
        Status = CampaignStatus.Draft;
        Description = description;
        CreatedBy = createdBy;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(string name, string? description)
    {
        Name = name;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetAudience(Guid audienceId)
    {
        AudienceId = audienceId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetTemplate(Guid templateId)
    {
        TemplateId = templateId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetBudget(decimal budget)
    {
        Budget = budget;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Schedule(DateTime scheduledDate)
    {
        if (Status != CampaignStatus.Draft)
            throw new InvalidOperationException("Only draft campaigns can be scheduled");

        ScheduledDate = scheduledDate;
        Status = CampaignStatus.Scheduled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Start(int recipientCount)
    {
        if (Status != CampaignStatus.Draft && Status != CampaignStatus.Scheduled)
            throw new InvalidOperationException("Only draft or scheduled campaigns can be started");

        Status = CampaignStatus.Running;
        TotalRecipients = recipientCount;
        StartedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Pause()
    {
        if (Status != CampaignStatus.Running)
            throw new InvalidOperationException("Only running campaigns can be paused");

        Status = CampaignStatus.Paused;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Resume()
    {
        if (Status != CampaignStatus.Paused)
            throw new InvalidOperationException("Only paused campaigns can be resumed");

        Status = CampaignStatus.Running;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Complete()
    {
        Status = CampaignStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == CampaignStatus.Completed)
            throw new InvalidOperationException("Cannot cancel a completed campaign");

        Status = CampaignStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordSent(int count)
    {
        SentCount += count;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordDelivered(int count)
    {
        DeliveredCount += count;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordOpened(int count)
    {
        OpenedCount += count;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordClicked(int count)
    {
        ClickedCount += count;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordBounced(int count)
    {
        BouncedCount += count;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordUnsubscribed(int count)
    {
        UnsubscribedCount += count;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordSpending(decimal amount)
    {
        SpentAmount += amount;
        UpdatedAt = DateTime.UtcNow;
    }

    public double OpenRate => SentCount > 0 ? (double)OpenedCount / SentCount * 100 : 0;
    public double ClickRate => OpenedCount > 0 ? (double)ClickedCount / OpenedCount * 100 : 0;
    public double BounceRate => SentCount > 0 ? (double)BouncedCount / SentCount * 100 : 0;
}
