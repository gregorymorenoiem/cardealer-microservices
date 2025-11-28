using AuditService.Shared.AuditMessages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace AuditService.Infrastructure.Services.Messaging;

public class RabbitMQAuditProducer : IAuditEventProducer, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly AuditServiceRabbitMQSettings _settings;
    private readonly ILogger<RabbitMQAuditProducer> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public RabbitMQAuditProducer(
        IOptions<RabbitMQSettings> rabbitMqSettings,
        IOptions<AuditServiceRabbitMQSettings> auditSettings,
        ILogger<RabbitMQAuditProducer> logger)
    {
        _settings = auditSettings.Value;
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
            VirtualHost = rabbitMqSettings.Value.VirtualHost
        };

        try
        {
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            ConfigureMessagingTopology();
            _logger.LogInformation("RabbitMQ Audit Producer initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize RabbitMQ Audit Producer");
            throw;
        }
    }

    private void ConfigureMessagingTopology()
    {
        _channel.ExchangeDeclare(
            exchange: _settings.ExchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false);

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
    }

    public Task PublishAuditEventAsync(AuditEvent auditEvent)
    {
        try
        {
            var message = JsonSerializer.Serialize(auditEvent, _jsonOptions);
            var body = Encoding.UTF8.GetBytes(message);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.ContentType = "application/json";
            properties.MessageId = auditEvent.Id.ToString();
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            _channel.BasicPublish(
                exchange: _settings.ExchangeName,
                routingKey: GetRoutingKey(auditEvent),
                basicProperties: properties,
                body: body);

            _logger.LogInformation(
                "Audit event published: {Action} for user {UserId}",
                auditEvent.Action, auditEvent.UserId);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish audit event: {Action}", auditEvent.Action);
            throw;
        }
    }

    private string GetRoutingKey(AuditEvent auditEvent)
    {
        return $"audit.event.{auditEvent.Action.ToLower()}";
    }

    public void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
        GC.SuppressFinalize(this);
    }
}