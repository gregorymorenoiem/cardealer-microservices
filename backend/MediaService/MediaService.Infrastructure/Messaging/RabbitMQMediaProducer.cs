using System.Text;
using System.Text.Json;
using MediaService.Domain.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace MediaService.Infrastructure.Messaging;

public interface IRabbitMQMediaProducer
{
    Task PublishDomainEventAsync(DomainEvent domainEvent);
    Task PublishProcessMediaCommandAsync(string mediaId, string? processingType = null);
    bool IsConnected { get; }
}

public class RabbitMQMediaProducer : IRabbitMQMediaProducer, IDisposable
{
    private readonly RabbitMQSettings _settings;
    private readonly ILogger<RabbitMQMediaProducer> _logger;
    private IConnection? _connection;
    private IModel? _channel;
    private bool _disposed = false;

    public bool IsConnected => _connection?.IsOpen == true && _channel?.IsOpen == true;

    public RabbitMQMediaProducer(IOptions<RabbitMQSettings> settings, ILogger<RabbitMQMediaProducer> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        InitializeConnection();
    }

    private void InitializeConnection()
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                Port = _settings.Port,
                UserName = _settings.UserName,
                Password = _settings.Password,
                VirtualHost = _settings.VirtualHost,
                DispatchConsumersAsync = true
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declare exchanges
            _channel.ExchangeDeclare(
                exchange: _settings.MediaEventsExchange,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false);

            _channel.ExchangeDeclare(
                exchange: _settings.MediaCommandsExchange,
                type: ExchangeType.Direct,
                durable: true,
                autoDelete: false);

            // Declare queues for events
            _channel.QueueDeclare(
                queue: _settings.MediaUploadedQueue,
                durable: true,
                exclusive: false,
                autoDelete: false);

            _channel.QueueDeclare(
                queue: _settings.MediaProcessedQueue,
                durable: true,
                exclusive: false,
                autoDelete: false);

            _channel.QueueDeclare(
                queue: _settings.MediaDeletedQueue,
                durable: true,
                exclusive: false,
                autoDelete: false);

            _channel.QueueDeclare(
                queue: _settings.ProcessMediaQueue,
                durable: true,
                exclusive: false,
                autoDelete: false);

            // Bind queues to exchanges
            _channel.QueueBind(
                queue: _settings.MediaUploadedQueue,
                exchange: _settings.MediaEventsExchange,
                routingKey: _settings.MediaUploadedRoutingKey);

            _channel.QueueBind(
                queue: _settings.MediaProcessedQueue,
                exchange: _settings.MediaEventsExchange,
                routingKey: _settings.MediaProcessedRoutingKey);

            _channel.QueueBind(
                queue: _settings.MediaDeletedQueue,
                exchange: _settings.MediaEventsExchange,
                routingKey: _settings.MediaDeletedRoutingKey);

            _channel.QueueBind(
                queue: _settings.ProcessMediaQueue,
                exchange: _settings.MediaCommandsExchange,
                routingKey: _settings.ProcessMediaRoutingKey);

            _logger.LogInformation("RabbitMQ Media Producer initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize RabbitMQ connection");
            throw;
        }
    }

    public Task PublishDomainEventAsync(DomainEvent domainEvent)
    {
        if (!IsConnected)
        {
            throw new InvalidOperationException("RabbitMQ connection is not available");
        }

        try
        {
            var routingKey = GetRoutingKeyForDomainEvent(domainEvent);
            var exchange = _settings.MediaEventsExchange;

            var message = JsonSerializer.Serialize(domainEvent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });

            var body = Encoding.UTF8.GetBytes(message);

            var properties = _channel!.CreateBasicProperties();
            properties.Persistent = true;
            properties.ContentType = "application/json";
            properties.MessageId = domainEvent.EventId;
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            properties.Headers = new Dictionary<string, object>
            {
                ["EventType"] = domainEvent.GetType().Name,
                ["ServiceName"] = domainEvent.ServiceName,
                ["OccurredOn"] = domainEvent.OccurredOn.ToString("O")
            };

            if (!string.IsNullOrEmpty(domainEvent.CorrelationId))
            {
                properties.CorrelationId = domainEvent.CorrelationId;
            }

            if (!string.IsNullOrEmpty(domainEvent.UserId))
            {
                properties.Headers["UserId"] = domainEvent.UserId;
            }

            _channel.BasicPublish(
                exchange: exchange,
                routingKey: routingKey,
                basicProperties: properties,
                body: body);

            _logger.LogDebug("Published domain event {EventType} with ID {EventId} to {Exchange}/{RoutingKey}",
                domainEvent.GetType().Name, domainEvent.EventId, exchange, routingKey);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish domain event {EventId}", domainEvent.EventId);
            throw;
        }
    }

    public Task PublishProcessMediaCommandAsync(string mediaId, string? processingType = null)
    {
        if (!IsConnected)
        {
            throw new InvalidOperationException("RabbitMQ connection is not available");
        }

        try
        {
            var command = new
            {
                MediaId = mediaId,
                ProcessingType = processingType,
                Timestamp = DateTime.UtcNow,
                CommandId = Guid.NewGuid().ToString()
            };

            var message = JsonSerializer.Serialize(command, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });

            var body = Encoding.UTF8.GetBytes(message);

            var properties = _channel!.CreateBasicProperties();
            properties.Persistent = true;
            properties.ContentType = "application/json";
            properties.MessageId = command.CommandId;
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            _channel.BasicPublish(
                exchange: _settings.MediaCommandsExchange,
                routingKey: _settings.ProcessMediaRoutingKey,
                basicProperties: properties,
                body: body);

            _logger.LogInformation("Published process media command for {MediaId}", mediaId);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish process media command for {MediaId}", mediaId);
            throw;
        }
    }

    private string GetRoutingKeyForDomainEvent(DomainEvent domainEvent)
    {
        return domainEvent switch
        {
            Domain.Events.MediaUploadedEvent => _settings.MediaUploadedRoutingKey,
            Domain.Events.MediaProcessedEvent => _settings.MediaProcessedRoutingKey,
            Domain.Events.MediaDeletedEvent => _settings.MediaDeletedRoutingKey,
            _ => $"media.unknown.{domainEvent.GetType().Name.ToLower()}"
        };
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _channel?.Close();
            _connection?.Close();
            _channel?.Dispose();
            _connection?.Dispose();
            _disposed = true;
        }
    }
}