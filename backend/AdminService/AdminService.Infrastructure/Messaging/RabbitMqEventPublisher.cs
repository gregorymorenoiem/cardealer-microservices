using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using AdminService.Domain.Interfaces;
using CarDealer.Contracts.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace AdminService.Infrastructure.Messaging;

// ═══════════════════════════════════════════════════════════════════════════════
// RABBITMQ EVENT PUBLISHER — AdminService
//
// Follows the ContactService pattern: lazy connection, topic exchange, Polly.
// Publishes to "cardealer.events" exchange with event-type routing keys.
// ═══════════════════════════════════════════════════════════════════════════════

public sealed class RabbitMqEventPublisher : IEventPublisher, IDisposable
{
    private readonly ILogger<RabbitMqEventPublisher> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _exchange;
    private readonly string _sourceService = "AdminService";

    private IConnection? _connection;
    private IModel? _channel;
    private readonly object _lock = new();
    private bool _disposed;

    public RabbitMqEventPublisher(
        ILogger<RabbitMqEventPublisher> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _exchange = configuration["RabbitMQ:Exchange"] ?? "cardealer.events";
    }

    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IEvent
    {
        try
        {
            EnsureConnection();

            if (_channel == null || _channel.IsClosed)
            {
                _logger.LogWarning("[RabbitMQ-Admin] Channel is closed, skipping publish of {EventType}",
                    @event.EventType);
                return Task.CompletedTask;
            }

            var routingKey = @event.EventType;
            var body = JsonSerializer.SerializeToUtf8Bytes(@event, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.ContentType = "application/json";
            properties.MessageId = @event.EventId.ToString();
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            properties.Headers = new Dictionary<string, object>
            {
                ["event_type"] = @event.EventType,
                ["source_service"] = _sourceService,
                ["schema_version"] = "1"
            };

            _channel.BasicPublish(
                exchange: _exchange,
                routingKey: routingKey,
                mandatory: false,
                basicProperties: properties,
                body: body);

            _logger.LogInformation(
                "[RabbitMQ-Admin] Published {EventType} (ID: {EventId}) to {Exchange}/{RoutingKey}",
                @event.EventType, @event.EventId, _exchange, routingKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "[RabbitMQ-Admin] Failed to publish {EventType}: {Error}",
                @event.EventType, ex.Message);
        }

        return Task.CompletedTask;
    }

    private void EnsureConnection()
    {
        if (_connection is { IsOpen: true } && _channel is { IsOpen: true })
            return;

        lock (_lock)
        {
            if (_connection is { IsOpen: true } && _channel is { IsOpen: true })
                return;

            try
            {
                _channel?.Dispose();
                _connection?.Dispose();

                var factory = new ConnectionFactory
                {
                    HostName = _configuration["RabbitMQ:Host"] ?? "localhost",
                    Port = int.TryParse(_configuration["RabbitMQ:Port"], out var port) ? port : 5672,
                    UserName = _configuration["RabbitMQ:Username"] ?? "guest",
                    Password = _configuration["RabbitMQ:Password"] ?? "guest",
                    VirtualHost = _configuration["RabbitMQ:VirtualHost"] ?? "/",
                    AutomaticRecoveryEnabled = true,
                    NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
                    RequestedHeartbeat = TimeSpan.FromSeconds(30)
                };

                _connection = factory.CreateConnection($"AdminService-Publisher-{Environment.MachineName}");
                _channel = _connection.CreateModel();
                _channel.ExchangeDeclare(_exchange, ExchangeType.Topic, durable: true);

                _logger.LogInformation("[RabbitMQ-Admin] Connected to {Host}:{Port}, exchange={Exchange}",
                    factory.HostName, factory.Port, _exchange);
            }
            catch (Exception ex) when (ex is SocketException or BrokerUnreachableException)
            {
                _logger.LogWarning(ex,
                    "[RabbitMQ-Admin] Unable to connect: {Error}. Events will not be published.",
                    ex.Message);
                _channel = null;
                _connection = null;
            }
        }
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
