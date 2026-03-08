
namespace AuthService.Infrastructure.Services.Messaging;

public class ErrorServiceRabbitMQSettings
{
    public bool EnableRabbitMQ { get; set; } = true;
    public string QueueName { get; set; } = "errors.queue";
    public string ExchangeName { get; set; } = "errors.exchange";
    public string RoutingKey { get; set; } = "error.created";
}
