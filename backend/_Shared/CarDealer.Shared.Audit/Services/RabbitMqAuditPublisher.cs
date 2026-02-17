using System.Text;
using System.Text.Json;
using CarDealer.Shared.Audit.Configuration;
using CarDealer.Shared.Audit.Interfaces;
using CarDealer.Shared.Audit.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace CarDealer.Shared.Audit.Services;

/// <summary>
/// Publisher de eventos de auditoría vía RabbitMQ
/// </summary>
public class RabbitMqAuditPublisher : IAuditPublisher, IDisposable
{
    private readonly ILogger<RabbitMqAuditPublisher> _logger;
    private readonly AuditOptions _options;
    private readonly JsonSerializerOptions _jsonOptions;
    private IConnection? _connection;
    private IModel? _channel;
    private readonly object _lock = new();
    private bool _disposed;

    public RabbitMqAuditPublisher(
        ILogger<RabbitMqAuditPublisher> logger,
        IOptions<AuditOptions> options)
    {
        _logger = logger;
        _options = options.Value;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        if (_options.Enabled)
        {
            InitializeConnection();
        }
    }

    private void InitializeConnection()
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _options.RabbitMq.Host,
                Port = _options.RabbitMq.Port,
                UserName = _options.RabbitMq.Username,
                Password = _options.RabbitMq.Password,
                VirtualHost = _options.RabbitMq.VirtualHost,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declarar exchange
            _channel.ExchangeDeclare(
                exchange: _options.ExchangeName,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false);

            _logger.LogInformation(
                "Audit publisher connected to RabbitMQ at {Host}:{Port}",
                _options.RabbitMq.Host,
                _options.RabbitMq.Port);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to RabbitMQ for audit publishing");
        }
    }

    public Task PublishAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            _logger.LogDebug("Audit publishing is disabled");
            return Task.CompletedTask;
        }

        // Enriquecer evento con información del servicio
        auditEvent.Source = _options.ServiceName;

        Publish(auditEvent);
        return Task.CompletedTask;
    }

    public void Publish(AuditEvent auditEvent)
    {
        if (!_options.Enabled || _channel == null || !_channel.IsOpen)
        {
            _logger.LogWarning("Cannot publish audit event - channel not available");
            return;
        }

        try
        {
            // Enriquecer evento
            auditEvent.Source = _options.ServiceName;

            var json = JsonSerializer.Serialize(auditEvent, _jsonOptions);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.ContentType = "application/json";
            properties.MessageId = auditEvent.Id.ToString();
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            properties.Headers = new Dictionary<string, object>
            {
                ["event-type"] = auditEvent.EventType,
                ["source"] = _options.ServiceName,
                ["severity"] = auditEvent.Severity.ToString()
            };

            lock (_lock)
            {
                _channel.BasicPublish(
                    exchange: _options.ExchangeName,
                    routingKey: $"{_options.RoutingKey}.{auditEvent.EventType.ToLower()}",
                    basicProperties: properties,
                    body: body);
            }

            _logger.LogDebug(
                "Published audit event {EventType} for resource {ResourceType}/{ResourceId}",
                auditEvent.EventType,
                auditEvent.ResourceType,
                auditEvent.ResourceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish audit event {EventType}", auditEvent.EventType);
        }
    }

    public async Task PublishAsync(
        string eventType,
        string action,
        string? resourceType = null,
        string? resourceId = null,
        object? metadata = null,
        CancellationToken cancellationToken = default)
    {
        var auditEvent = new AuditEvent
        {
            EventType = eventType,
            Action = action,
            ResourceType = resourceType,
            ResourceId = resourceId,
            Metadata = metadata != null
                ? JsonSerializer.Deserialize<Dictionary<string, object>>(
                    JsonSerializer.Serialize(metadata, _jsonOptions), _jsonOptions)
                : null
        };

        await PublishAsync(auditEvent, cancellationToken);
    }

    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true;
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
    }
}
