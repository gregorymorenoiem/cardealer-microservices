namespace NotificationService.Infrastructure.Messaging;

public class RabbitMQSettings
{
    public string Host { get; set; } = "rabbitmq";
    public int Port { get; set; } = 5672;
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    public int ConnectionTimeout { get; set; } = 30000;
    public int RequestedHeartbeat { get; set; } = 60;

    // Propiedades adicionales para resiliencia
    public int RetryCount { get; set; } = 3;
    public int RetryDelayMilliseconds { get; set; } = 2000;
    public bool AutomaticRecoveryEnabled { get; set; } = true;
    public TimeSpan NetworkRecoveryInterval { get; set; } = TimeSpan.FromSeconds(10);
}