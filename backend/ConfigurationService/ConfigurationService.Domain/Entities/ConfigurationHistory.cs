namespace ConfigurationService.Domain.Entities;

public class ConfigurationHistory
{
    public Guid Id { get; set; }
    public Guid ConfigurationItemId { get; set; }
    public required string Key { get; set; }
    public required string OldValue { get; set; }
    public required string NewValue { get; set; }
    public required string Environment { get; set; }
    public required string ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; }
    public string? ChangeReason { get; set; }
}
