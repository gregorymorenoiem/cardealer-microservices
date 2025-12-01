namespace MessageBusService.Domain.Entities;

public class Subscription
{
    public Guid Id { get; set; }
    public string Topic { get; set; } = string.Empty;
    public string ConsumerName { get; set; } = string.Empty;
    public string QueueName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastActivityAt { get; set; }
    public int MessagesConsumed { get; set; }
    public Dictionary<string, string>? Configuration { get; set; }
}
