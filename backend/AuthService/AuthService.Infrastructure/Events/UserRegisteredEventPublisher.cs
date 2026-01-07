using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CarDealer.Contracts.Events.Auth;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace AuthService.Infrastructure.Events;

/// <summary>
/// Publisher para eventos de registro de usuarios
/// </summary>
public class UserRegisteredEventPublisher : IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<UserRegisteredEventPublisher> _logger;
    private const string ExchangeName = "cardealer.events";
    private const string RoutingKey = "auth.user.registered";

    public UserRegisteredEventPublisher(
        IConnection connection,
        ILogger<UserRegisteredEventPublisher> logger)
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

    public Task PublishAsync(UserRegisteredEvent @event, CancellationToken cancellationToken = default)
    {
        if (@event == null)
            throw new ArgumentNullException(nameof(@event));

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
                "Published UserRegisteredEvent for user {Email} with ID {UserId}",
                @event.Email,
                @event.UserId);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish UserRegisteredEvent for user {Email}", @event.Email);
            throw;
        }
    }

    public void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
    }
}
