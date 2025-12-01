using MessageBusService.Domain.Enums;

namespace MessageBusService.Domain.Entities;

public class MessageBatch
{
    public Guid Id { get; set; }
    public string BatchName { get; set; } = string.Empty;
    public List<Guid> MessageIds { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public MessageStatus Status { get; set; }
    public int TotalMessages { get; set; }
    public int ProcessedMessages { get; set; }
    public int FailedMessages { get; set; }
}
