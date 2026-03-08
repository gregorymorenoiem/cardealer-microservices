using AuditService.Domain.Entities;
using AuditService.Domain.Interfaces;
using CarDealer.Contracts.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace AuditService.Infrastructure.Messaging;

/// <summary>
/// Background service that consumes ALL events from RabbitMQ and persists them to audit database.
/// Uses wildcard routing key '#' to capture every event published to the exchange.
/// </summary>
public class RabbitMqEventConsumer : BackgroundService
{
    private readonly ILogger<RabbitMqEventConsumer> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private IConnection? _connection;
    private IModel? _channel;
    private const string ExchangeName = "cardealer.events";
    private const string QueueName = "audit.all-events";
    private const string RoutingKey = "#"; // Wildcard to consume ALL events

    public RabbitMqEventConsumer(
        ILogger<RabbitMqEventConsumer> logger,
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // RELIABILITY: Retry loop for RabbitMQ connection (handles K8s startup races)
        const int maxRetries = 10;
        for (var attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                InitializeRabbitMq();

                if (_channel == null)
                {
                    _logger.LogError("Failed to initialize RabbitMQ channel");
                    return;
                }

                // DispatchConsumersAsync=true requires AsyncEventingBasicConsumer
                var consumer = new AsyncEventingBasicConsumer(_channel);

                consumer.Received += async (model, ea) =>
                {
                    try
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        var routingKey = ea.RoutingKey;

                        _logger.LogInformation("Received event with routing key: {RoutingKey}", routingKey);

                        await ProcessEventAsync(message, routingKey, stoppingToken);

                        // Acknowledge the message
                        _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing event");

                        // RELIABILITY: Track retry count to prevent infinite poison message loop
                        var retryCount = GetRetryCount(ea.BasicProperties);
                        if (retryCount >= 3)
                        {
                            _logger.LogError("Poison message detected after {Retries} retries. Sending to DLX.", retryCount);
                            _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
                        }
                        else
                        {
                            _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                        }
                    }
                };

                _channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);

                _logger.LogInformation("AuditService consumer started. Listening for ALL events on queue: {QueueName}", QueueName);

                // Keep the service running
                await Task.Delay(Timeout.Infinite, stoppingToken);
                return;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return; // Graceful shutdown
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "AuditService RabbitMQ consumer: connection attempt {Attempt}/{Max} failed. Retrying...",
                    attempt, maxRetries);

                if (attempt == maxRetries)
                {
                    _logger.LogError("AuditService RabbitMQ consumer: All {Max} connection attempts exhausted.", maxRetries);
                    return;
                }

                var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
                await Task.Delay(delay, stoppingToken);
            }
        }
    }

    /// <summary>
    /// RELIABILITY: Extract retry count from RabbitMQ x-death headers.
    /// </summary>
    private static int GetRetryCount(RabbitMQ.Client.IBasicProperties? properties)
    {
        if (properties?.Headers == null) return 0;
        if (!properties.Headers.TryGetValue("x-death", out var xDeath)) return 0;
        if (xDeath is IList<object> entries && entries.Count > 0 &&
            entries[0] is IDictionary<string, object> first &&
            first.TryGetValue("count", out var countObj))
            return Convert.ToInt32(countObj);
        return 0;
    }

    private void InitializeRabbitMq()
    {
        try
        {
            var rabbitMqHost = _configuration["RabbitMQ:Host"] ?? _configuration["RabbitMQ:HostName"] ?? "localhost";
            var rabbitMqPort = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672");
            var rabbitMqUser = _configuration["RabbitMQ:UserName"] ?? _configuration["RabbitMQ:Username"] ?? "guest";
            var rabbitMqPassword = _configuration["RabbitMQ:Password"] ?? "guest";

            var factory = new ConnectionFactory
            {
                HostName = rabbitMqHost,
                Port = rabbitMqPort,
                UserName = rabbitMqUser,
                Password = rabbitMqPassword,
                DispatchConsumersAsync = true // Enable async consumers
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declare the exchange (should already exist, but ensures consistency)
            _channel.ExchangeDeclare(
                exchange: ExchangeName,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false);

            // Declare the audit queue
            _channel.QueueDeclare(
                queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            // Bind queue to exchange with wildcard routing key to receive ALL events
            _channel.QueueBind(
                queue: QueueName,
                exchange: ExchangeName,
                routingKey: RoutingKey);

            // Set QoS to process one message at a time
            _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            _logger.LogInformation("RabbitMQ consumer initialized successfully. Queue: {QueueName}, Routing Key: {RoutingKey}", QueueName, RoutingKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize RabbitMQ");
            throw;
        }
    }

    private async Task ProcessEventAsync(string message, string routingKey, CancellationToken cancellationToken)
    {
        try
        {
            // Deserialize the base event to extract common properties
            var eventData = JsonSerializer.Deserialize<BaseEventData>(message);

            if (eventData == null)
            {
                _logger.LogWarning("Failed to deserialize event: {Message}", message);
                return;
            }

            var auditEvent = new AuditEvent
            {
                EventId = eventData.EventId,
                EventType = routingKey,
                Source = DetermineSource(routingKey),
                Payload = message,
                EventTimestamp = eventData.OccurredOn,
                ConsumedAt = DateTime.UtcNow,
                CorrelationId = eventData.CorrelationId
            };

            // Use scoped service to save the audit event
            using var scope = _scopeFactory.CreateScope();
            var auditRepository = scope.ServiceProvider.GetRequiredService<IAuditRepository>();
            await auditRepository.SaveAuditEventAsync(auditEvent, cancellationToken);

            _logger.LogInformation("Audit event persisted successfully. EventId: {EventId}, Type: {EventType}",
                auditEvent.EventId, auditEvent.EventType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing audit event: {Message}", message);
            throw;
        }
    }

    private string DetermineSource(string routingKey)
    {
        // Extract source from routing key pattern (e.g., "user.registered" -> "AuthService")
        if (routingKey.StartsWith("user.") || routingKey.StartsWith("auth."))
            return "AuthService";
        if (routingKey.StartsWith("vehicle."))
            return "VehicleService";
        if (routingKey.StartsWith("media."))
            return "MediaService";
        if (routingKey.StartsWith("error."))
            return "ErrorService";
        if (routingKey.StartsWith("notification."))
            return "NotificationService";
        if (routingKey.StartsWith("contact."))
            return "ContactService";
        if (routingKey.StartsWith("admin."))
            return "AdminService";

        return "Unknown";
    }

    public override void Dispose()
    {
        try { _channel?.Close(); } catch { /* ignore */ }
        try { _connection?.Close(); } catch { /* ignore */ }
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }

    /// <summary>
    /// Minimal class to extract common event properties for audit.
    /// </summary>
    private class BaseEventData
    {
        public Guid EventId { get; set; }
        public DateTime OccurredOn { get; set; }
        public string? CorrelationId { get; set; }
    }
}
