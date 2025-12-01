namespace ConfigurationService.Domain.Entities;

public class EncryptedSecret
{
    public Guid Id { get; set; }
    public required string Key { get; set; }
    public required string EncryptedValue { get; set; }
    public required string Environment { get; set; }
    public string? Description { get; set; }
    public string? TenantId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? ExpiresAt { get; set; }
}
