using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CarDealer.Contracts.Events.Vehicle;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace VehiclesSaleService.Infrastructure.Events;

/// <summary>
/// Publisher para eventos de creación de vehículos
/// </summary>
public class VehicleCreatedEventPublisher : IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<VehicleCreatedEventPublisher> _logger;
    private const string ExchangeName = "cardealer.events";
    private const string RoutingKey = "vehicle.created";

    public VehicleCreatedEventPublisher(
        IConnection connection,
        ILogger<VehicleCreatedEventPublisher> logger)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _channel = _connection.CreateModel();

        // Declarar exchange tipo topic
        _channel.ExchangeDeclare(
            exchange: ExchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false
        );
    }

    public Task PublishAsync(VehicleCreatedEvent @event, CancellationToken cancellationToken = default)
    {
        if (@event == null)
            throw new ArgumentNullException(nameof(@event));

        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var message = JsonSerializer.Serialize(@event);
            var body = Encoding.UTF8.GetBytes(message);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.MessageId = @event.EventId.ToString();
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            properties.ContentType = "application/json";
            properties.Headers = new Dictionary<string, object>
            {
                ["event_type"] = @event.EventType
            };

            _channel.BasicPublish(
                exchange: ExchangeName,
                routingKey: RoutingKey,
                basicProperties: properties,
                body: body
            );

            _logger.LogInformation(
                "Published VehicleCreatedEvent for vehicle {Make} {Model} {Year} with ID {VehicleId}",
                @event.Make,
                @event.Model,
                @event.Year,
                @event.VehicleId);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish VehicleCreatedEvent for vehicle {VehicleId}", @event.VehicleId);
            throw;
        }
    }

    public void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
    }
}
