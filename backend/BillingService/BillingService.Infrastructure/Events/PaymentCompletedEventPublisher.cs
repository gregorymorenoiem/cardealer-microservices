using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CarDealer.Contracts.Events.Billing;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace BillingService.Infrastructure.Events;

/// <summary>
/// Publisher para eventos de pagos completados
/// </summary>
public class PaymentCompletedEventPublisher : IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<PaymentCompletedEventPublisher> _logger;
    private const string ExchangeName = "cardealer.events";
    private const string RoutingKey = "payment.completed";

    public PaymentCompletedEventPublisher(
        IConnection connection,
        ILogger<PaymentCompletedEventPublisher> logger)
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

    public Task PublishAsync(PaymentCompletedEvent @event, CancellationToken cancellationToken = default)
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
                "Published PaymentCompletedEvent for user {Email} - Amount: {Amount} {Currency} - PaymentId: {PaymentId}",
                @event.UserEmail,
                @event.Amount,
                @event.Currency,
                @event.PaymentId);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish PaymentCompletedEvent for payment {PaymentId}", @event.PaymentId);
            throw;
        }
    }

    public void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
    }
}
