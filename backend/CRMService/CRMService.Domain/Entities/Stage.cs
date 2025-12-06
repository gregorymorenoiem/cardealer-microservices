using CarDealer.Shared.MultiTenancy;

namespace CRMService.Domain.Entities;

/// <summary>
/// Represents a stage within a sales pipeline.
/// </summary>
public class Stage : ITenantEntity
{
    public Guid Id { get; private set; }
    public Guid DealerId { get; set; }
    public Guid PipelineId { get; private set; }

    public string Name { get; private set; } = string.Empty;
    public int Order { get; private set; }
    public string? Color { get; private set; }
    public int DefaultProbability { get; private set; }
    public bool IsWonStage { get; private set; }
    public bool IsLostStage { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Navigation properties
    public Pipeline? Pipeline { get; private set; }
    public ICollection<Deal> Deals { get; private set; } = new List<Deal>();

    private Stage() { } // EF Constructor

    public Stage(Guid pipelineId, Guid dealerId, string name, int order, string? color = null)
    {
        Id = Guid.NewGuid();
        PipelineId = pipelineId;
        DealerId = dealerId;
        Name = name;
        Order = order;
        Color = color;
        DefaultProbability = CalculateDefaultProbability(order);
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    private static int CalculateDefaultProbability(int order)
    {
        // Increase probability based on stage order
        return Math.Min(order * 15, 90);
    }

    public void Update(string name, int order, string? color)
    {
        Name = name;
        Order = order;
        Color = color;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetProbability(int probability)
    {
        DefaultProbability = Math.Max(0, Math.Min(100, probability));
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsWonStage()
    {
        IsWonStage = true;
        IsLostStage = false;
        DefaultProbability = 100;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsLostStage()
    {
        IsLostStage = true;
        IsWonStage = false;
        DefaultProbability = 0;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ClearSpecialStatus()
    {
        IsWonStage = false;
        IsLostStage = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
