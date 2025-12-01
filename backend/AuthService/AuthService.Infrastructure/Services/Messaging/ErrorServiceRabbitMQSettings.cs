
namespace AuthService.Infrastructure.Services.Messaging;

public class ErrorServiceRabbitMQSettings
{
    public bool EnableRabbitMQ { get; set; } = true;
    public string QueueName { get; set; } = "error-queue";
    public string ExchangeName { get; set; } = "error-exchange";
    public string RoutingKey { get; set; } = "error.routing.key";
}
