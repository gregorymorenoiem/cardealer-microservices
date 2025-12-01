namespace ConfigurationService.Domain.Entities;

public class FeatureFlag
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Key { get; set; }
    public string? Description { get; set; }
    public bool IsEnabled { get; set; }
    public string? Environment { get; set; }
    public string? TenantId { get; set; }

    // For gradual rollout
    public int RolloutPercentage { get; set; } = 100; // 0-100
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}
