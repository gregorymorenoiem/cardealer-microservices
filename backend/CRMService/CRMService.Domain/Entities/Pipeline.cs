using CarDealer.Shared.MultiTenancy;

namespace CRMService.Domain.Entities;

/// <summary>
/// Represents a sales pipeline containing stages for deal progression.
/// </summary>
public class Pipeline : ITenantEntity
{
    public Guid Id { get; private set; }
    public Guid DealerId { get; set; }

    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsDefault { get; private set; }
    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Navigation properties
    public ICollection<Stage> Stages { get; private set; } = new List<Stage>();
    public ICollection<Deal> Deals { get; private set; } = new List<Deal>();

    private Pipeline() { } // EF Constructor

    public Pipeline(Guid dealerId, string name, string? description = null, bool isDefault = false)
    {
        Id = Guid.NewGuid();
        DealerId = dealerId;
        Name = name;
        Description = description;
        IsDefault = isDefault;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(string name, string? description)
    {
        Name = name;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetAsDefault()
    {
        IsDefault = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UnsetAsDefault()
    {
        IsDefault = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public Stage AddStage(string name, int order, string? color = null)
    {
        var stage = new Stage(Id, DealerId, name, order, color);
        Stages.Add(stage);
        UpdatedAt = DateTime.UtcNow;
        return stage;
    }

    /// <summary>
    /// Creates a default sales pipeline with common stages for car dealerships.
    /// </summary>
    public static Pipeline CreateDefaultPipeline(Guid dealerId)
    {
        var pipeline = new Pipeline(dealerId, "Sales Pipeline", "Default sales pipeline", true);

        pipeline.AddStage("New Lead", 1, "#6366F1");
        pipeline.AddStage("Initial Contact", 2, "#8B5CF6");
        pipeline.AddStage("Test Drive Scheduled", 3, "#EC4899");
        pipeline.AddStage("Negotiation", 4, "#F59E0B");
        pipeline.AddStage("Proposal Sent", 5, "#10B981");
        pipeline.AddStage("Closing", 6, "#3B82F6");

        return pipeline;
    }
}
