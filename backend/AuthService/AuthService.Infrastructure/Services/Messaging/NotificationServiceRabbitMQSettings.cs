
namespace AuthService.Infrastructure.Services.Messaging;

public class NotificationServiceRabbitMQSettings
{
    public bool EnableRabbitMQ { get; set; } = true;
    public string QueueName { get; set; } = "notification-queue";
    public string ExchangeName { get; set; } = "cardealer.events";
    public string RoutingKey { get; set; } = "notification.auth";

    // Opcional: Configuraciones espec�ficas para tipos de notificaci�n
    public string EmailQueueName { get; set; } = "notification-email-queue";
    public string SmsQueueName { get; set; } = "notification-sms-queue";
    public string PushQueueName { get; set; } = "notification-push-queue";

    public int RetryDelayMs { get; set; } = 60000; // 1 minuto para retry
    public int MaxRetryAttempts { get; set; } = 3;
}
