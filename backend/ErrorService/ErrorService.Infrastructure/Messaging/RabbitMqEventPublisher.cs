using CarDealer.Contracts.Abstractions;
using ErrorService.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace ErrorService.Infrastructure.Messaging;

/// <summary>
/// RabbitMQ implementation of IEventPublisher.
/// Publishes events to RabbitMQ topic exchanges for consumption by other microservices.
/// </summary>
public class RabbitMqEventPublisher : IEventPublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMqEventPublisher> _logger;
    private readonly string _exchangeName;
    private readonly JsonSerializerOptions _jsonOptions;

    public RabbitMqEventPublisher(
        IConfiguration configuration,
        ILogger<RabbitMqEventPublisher> logger)
    {
        _logger = logger;
        _exchangeName = configuration["RabbitMQ:ExchangeName"] ?? "cardealer.events";

        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMQ:HostName"] ?? "localhost",
            Port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672"),
            UserName = configuration["RabbitMQ:UserName"] ?? "guest",
            Password = configuration["RabbitMQ:Password"] ?? "guest",
            VirtualHost = configuration["RabbitMQ:VirtualHost"] ?? "/",
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // Declare topic exchange (idempotent operation)
        _channel.ExchangeDeclare(
            exchange: _exchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false);

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        _logger.LogInformation(
            "RabbitMQ Event Publisher initialized. Exchange: {Exchange}, Host: {Host}",
            _exchangeName, factory.HostName);
    }

    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IEvent
    {
        try
        {
            var routingKey = @event.EventType; // e.g., "error.critical", "error.logged"
            var messageBody = JsonSerializer.Serialize(@event, _jsonOptions);
            var body = Encoding.UTF8.GetBytes(messageBody);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true; // Make messages durable
            properties.ContentType = "application/json";
            properties.MessageId = @event.EventId.ToString();
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            properties.Type = @event.EventType;

            _channel.BasicPublish(
                exchange: _exchangeName,
                routingKey: routingKey,
                basicProperties: properties,
                body: body);

            _logger.LogInformation(
                "Published event {EventType} with ID {EventId} to exchange {Exchange}",
                @event.EventType, @event.EventId, _exchangeName);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to publish event {EventType} with ID {EventId}",
                @event.EventType, @event.EventId);
            throw;
        }
    }

    public void Dispose()
    {
        try
        {
            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
            
            _logger.LogInformation("RabbitMQ Event Publisher disposed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disposing RabbitMQ Event Publisher");
        }
    }
}
