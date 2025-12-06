using CarDealer.Shared.MultiTenancy;

namespace CRMService.Domain.Entities;

/// <summary>
/// Represents a sales opportunity/deal in the CRM system.
/// </summary>
public class Deal : ITenantEntity
{
    public Guid Id { get; private set; }
    public Guid DealerId { get; set; }

    // Deal Information
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public decimal Value { get; private set; }
    public string Currency { get; private set; } = "MXN";

    // Pipeline & Stage
    public Guid PipelineId { get; private set; }
    public Guid StageId { get; private set; }
    public int StageOrder { get; private set; }

    // Status & Probability
    public DealStatus Status { get; private set; }
    public int Probability { get; private set; } // 0-100%

    // Dates
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? ExpectedCloseDate { get; private set; }
    public DateTime? ActualCloseDate { get; private set; }

    // Relationships
    public Guid? LeadId { get; private set; }
    public Guid? ContactId { get; private set; }
    public Guid? AssignedToUserId { get; private set; }

    // Vehicle/Product (for car dealership)
    public Guid? ProductId { get; private set; }
    public string? VIN { get; private set; }

    // Won/Lost Details
    public string? LostReason { get; private set; }
    public string? WonNotes { get; private set; }

    // Metadata
    public Dictionary<string, object> CustomFields { get; private set; } = new();
    public List<string> Tags { get; private set; } = new();

    // Navigation properties
    public Pipeline? Pipeline { get; private set; }
    public Stage? Stage { get; private set; }
    public Lead? Lead { get; private set; }
    public ICollection<Activity> Activities { get; private set; } = new List<Activity>();

    private Deal() { } // EF Constructor

    public Deal(
        Guid dealerId,
        string title,
        decimal value,
        Guid pipelineId,
        Guid stageId,
        Guid? leadId = null,
        Guid? contactId = null)
    {
        Id = Guid.NewGuid();
        DealerId = dealerId;
        Title = title;
        Value = value;
        PipelineId = pipelineId;
        StageId = stageId;
        LeadId = leadId;
        ContactId = contactId;
        Status = DealStatus.Open;
        Probability = 10;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(string title, string? description, decimal value, string currency)
    {
        Title = title;
        Description = description;
        Value = value;
        Currency = currency;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MoveToStage(Guid stageId, int stageOrder)
    {
        StageId = stageId;
        StageOrder = stageOrder;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateProbability(int probability)
    {
        Probability = Math.Max(0, Math.Min(100, probability));
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetExpectedCloseDate(DateTime date)
    {
        ExpectedCloseDate = date;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AssignTo(Guid userId)
    {
        AssignedToUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetProduct(Guid productId, string? vin = null)
    {
        ProductId = productId;
        VIN = vin;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsWon(string? notes = null)
    {
        if (Status != DealStatus.Open)
            throw new InvalidOperationException("Only open deals can be marked as won");

        Status = DealStatus.Won;
        Probability = 100;
        ActualCloseDate = DateTime.UtcNow;
        WonNotes = notes;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsLost(string reason)
    {
        if (Status != DealStatus.Open)
            throw new InvalidOperationException("Only open deals can be marked as lost");

        Status = DealStatus.Lost;
        Probability = 0;
        ActualCloseDate = DateTime.UtcNow;
        LostReason = reason;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reopen()
    {
        if (Status == DealStatus.Open)
            throw new InvalidOperationException("Deal is already open");

        Status = DealStatus.Open;
        ActualCloseDate = null;
        LostReason = null;
        WonNotes = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddTag(string tag)
    {
        if (!Tags.Contains(tag))
        {
            Tags.Add(tag);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void SetCustomField(string key, object value)
    {
        CustomFields[key] = value;
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum DealStatus
{
    Open,
    Won,
    Lost
}
