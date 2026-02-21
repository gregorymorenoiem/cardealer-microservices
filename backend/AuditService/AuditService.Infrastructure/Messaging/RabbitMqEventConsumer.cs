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
                    // Negative acknowledgment - requeue the message
                    _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                }
            };

            _channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);

            _logger.LogInformation("AuditService consumer started. Listening for ALL events on queue: {QueueName}", QueueName);

            // Keep the service running
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in RabbitMQ consumer");
        }
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
        _channel?.Close();
        _connection?.Close();
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
