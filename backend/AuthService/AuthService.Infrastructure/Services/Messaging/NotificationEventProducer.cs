// AuthService.Infrastructure/Services/Messaging/NotificationEventProducer.cs
using AuthService.Infrastructure.Services.Messaging;
using AuthService.Shared.NotificationMessages;
using AuthService.Shared.Messaging;
using AuthService.Domain.Interfaces;
using AuthService.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text.Json;
using System.Text;
using Polly;
using Polly.CircuitBreaker;

namespace AuthService.Infrastructure.Services.Messaging;

public class RabbitMQNotificationProducer : INotificationEventProducer, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly NotificationServiceRabbitMQSettings _settings;
    private readonly ILogger<RabbitMQNotificationProducer> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly IDeadLetterQueue? _deadLetterQueue;
    private readonly ResiliencePipeline _resiliencePipeline;

    public RabbitMQNotificationProducer(
        IOptions<RabbitMQSettings> rabbitMqSettings,
        IOptions<NotificationServiceRabbitMQSettings> notificationSettings,
        ILogger<RabbitMQNotificationProducer> logger,
        IDeadLetterQueue? deadLetterQueue = null)
    {
        _settings = notificationSettings.Value;
        _logger = logger;
        _deadLetterQueue = deadLetterQueue;
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
            DispatchConsumersAsync = true,
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
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

        // Configure Circuit Breaker
        _resiliencePipeline = new ResiliencePipelineBuilder()
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = 0.5,
                SamplingDuration = TimeSpan.FromSeconds(30),
                MinimumThroughput = 3,
                BreakDuration = TimeSpan.FromSeconds(30),
                OnOpened = args =>
                {
                    _logger.LogWarning("🔴 Notification Producer Circuit Breaker OPEN: NotificationService unavailable");
                    return ValueTask.CompletedTask;
                },
                OnClosed = args =>
                {
                    _logger.LogInformation("🟢 Notification Producer Circuit Breaker CLOSED: NotificationService restored");
                    return ValueTask.CompletedTask;
                }
            })
            .Build();

        _logger.LogInformation("RabbitMQ Notification Producer initialized with Circuit Breaker");
    }

    public async Task PublishNotificationAsync(NotificationEvent notification)
    {
        try
        {
            await _resiliencePipeline.ExecuteAsync(async ct =>
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
                return ValueTask.CompletedTask;
            }, CancellationToken.None);
        }
        catch (BrokenCircuitException ex)
        {
            _logger.LogWarning(ex, "Circuit Breaker OPEN: Cannot publish notification {Type}. Queuing to DLQ.", notification.Type);

            if (_deadLetterQueue != null)
            {
                var failedEvent = new FailedEvent
                {
                    EventType = "notification.event",
                    EventJson = JsonSerializer.Serialize(notification, _jsonOptions),
                    FailedAt = DateTime.UtcNow,
                    RetryCount = 0
                };
                failedEvent.ScheduleNextRetry();
                _deadLetterQueue.Enqueue(failedEvent);
            }
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
