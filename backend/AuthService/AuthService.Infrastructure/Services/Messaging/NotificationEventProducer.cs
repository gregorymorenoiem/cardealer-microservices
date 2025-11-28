// AuthService.Infrastructure/Services/Messaging/NotificationEventProducer.cs
using AuthService.Infrastructure.Services.Messaging;
using AuthService.Shared.NotificationMessages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text.Json;
using System.Text;
using AuthService.Domain.Interfaces.Services;

namespace AuthService.Infrastructure.Services.Messaging;

public class RabbitMQNotificationProducer : INotificationEventProducer, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly NotificationServiceRabbitMQSettings _settings;
    private readonly ILogger<RabbitMQNotificationProducer> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public RabbitMQNotificationProducer(
        IOptions<RabbitMQSettings> rabbitMqSettings,
        IOptions<NotificationServiceRabbitMQSettings> notificationSettings,
        ILogger<RabbitMQNotificationProducer> logger)
    {
        _settings = notificationSettings.Value;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        var factory = new ConnectionFactory
        {
            HostName = rabbitMqSettings.Value.Host,
            Port = rabbitMqSettings.Value.Port,
            UserName = rabbitMqSettings.Value.Username,
            Password = rabbitMqSettings.Value.Password,
            VirtualHost = rabbitMqSettings.Value.VirtualHost,
            DispatchConsumersAsync = true
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // Declarar exchange para notificaciones
        _channel.ExchangeDeclare(
            exchange: _settings.ExchangeName,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false);

        // Declarar cola principal de notificaciones
        _channel.QueueDeclare(
            queue: _settings.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        _channel.QueueBind(
            queue: _settings.QueueName,
            exchange: _settings.ExchangeName,
            routingKey: _settings.RoutingKey);

        // Cola de retry para fallos
        _channel.QueueDeclare(
            queue: $"{_settings.QueueName}-retry",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object>
            {
                ["x-dead-letter-exchange"] = _settings.ExchangeName,
                ["x-dead-letter-routing-key"] = _settings.RoutingKey,
                ["x-message-ttl"] = 60000 // 1 minuto
            });

        _logger.LogInformation("RabbitMQ Notification Producer initialized");
    }

    public async Task PublishNotificationAsync(NotificationEvent notification)
    {
        try
        {
            var message = JsonSerializer.Serialize(notification, _jsonOptions);
            var body = Encoding.UTF8.GetBytes(message);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.ContentType = "application/json";
            properties.MessageId = notification.Id;
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            properties.Headers = new Dictionary<string, object>
            {
                ["type"] = notification.Type,
                ["template"] = notification.TemplateName
            };

            _channel.BasicPublish(
                exchange: _settings.ExchangeName,
                routingKey: _settings.RoutingKey,
                basicProperties: properties,
                body: body);

            _logger.LogInformation("Notification event published: {Type} to {To}", notification.Type, notification.To);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish notification event: {Type}", notification.Type);
            throw;
        }
    }

    public Task PublishEmailAsync(string to, string subject, string body, Dictionary<string, object>? data = null)
    {
        var notification = new EmailNotificationEvent
        {
            To = to,
            Subject = subject,
            Body = body,
            Data = data ?? new Dictionary<string, object>(),
            TemplateName = "CustomEmail"
        };

        return PublishNotificationAsync(notification);
    }

    public Task PublishSmsAsync(string to, string message, Dictionary<string, object>? data = null)
    {
        var notification = new SmsNotificationEvent
        {
            To = to,
            Body = message,
            Data = data ?? new Dictionary<string, object>(),
            TemplateName = "CustomSMS"
        };

        return PublishNotificationAsync(notification);
    }

    public void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
    }
}
