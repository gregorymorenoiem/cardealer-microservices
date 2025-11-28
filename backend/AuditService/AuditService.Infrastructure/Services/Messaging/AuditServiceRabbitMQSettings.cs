namespace AuditService.Infrastructure.Services.Messaging;

public class AuditServiceRabbitMQSettings
{
    public string ExchangeName { get; set; } = "audit.events";
    public string QueueName { get; set; } = "audit.service.queue";
    public string RoutingKey { get; set; } = "audit.event.*";
    public string RetryQueueName { get; set; } = "audit.service.queue.retry";
    public int RetryDelayMs { get; set; } = 30000; // 30 segundos
    public int MaxRetryAttempts { get; set; } = 3;
}