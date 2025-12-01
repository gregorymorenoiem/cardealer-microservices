namespace ConfigurationService.Domain.Entities;

public class ConfigurationItem
{
    public Guid Id { get; set; }
    public required string Key { get; set; }
    public required string Value { get; set; }
    public required string Environment { get; set; } // Dev, Staging, Prod
    public string? Description { get; set; }
    public string? TenantId { get; set; } // For multi-tenant support
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsActive { get; set; } = true;

    // For versioning and auditing
    public int Version { get; set; } = 1;
}
