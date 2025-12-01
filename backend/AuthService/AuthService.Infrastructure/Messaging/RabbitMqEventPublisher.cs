using System.Text;
using System.Text.Json;
using CarDealer.Contracts.Abstractions;
using AuthService.Domain.Interfaces;
using AuthService.Shared.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Polly;
using Polly.CircuitBreaker;

namespace AuthService.Infrastructure.Messaging;

/// <summary>
/// RabbitMQ implementation of IEventPublisher for AuthService.
/// Publishes authentication events to cardealer.events exchange with Circuit Breaker pattern.
/// </summary>
public class RabbitMqEventPublisher : IEventPublisher, IDisposable
{
    private readonly ILogger<RabbitMqEventPublisher> _logger;
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _exchangeName;
    private readonly IDeadLetterQueue? _deadLetterQueue;
    private readonly ResiliencePipeline _resiliencePipeline;
    private readonly JsonSerializerOptions _jsonOptions;

    public RabbitMqEventPublisher(
        IConfiguration configuration,
        ILogger<RabbitMqEventPublisher> logger,
        IDeadLetterQueue? deadLetterQueue = null)
    {
        _logger = logger;
        _deadLetterQueue = deadLetterQueue;

        var hostName = configuration["RabbitMQ:HostName"] ?? "localhost";
        var port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672");
        var userName = configuration["RabbitMQ:UserName"] ?? "guest";
        var password = configuration["RabbitMQ:Password"] ?? "guest";
        _exchangeName = configuration["RabbitMQ:ExchangeName"] ?? "cardealer.events";

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        _logger.LogInformation(
            "Initializing RabbitMQ Event Publisher with Circuit Breaker: Host={Host}, Port={Port}, Exchange={Exchange}",
            hostName, port, _exchangeName);

        try
        {
            var factory = new ConnectionFactory
            {
                HostName = hostName,
                Port = port,
                UserName = userName,
                Password = password,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declare topic exchange (idempotent)
            _channel.ExchangeDeclare(
                exchange: _exchangeName,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false);

            // Configure Circuit Breaker with Polly v8
            _resiliencePipeline = new ResiliencePipelineBuilder()
                .AddCircuitBreaker(new CircuitBreakerStrategyOptions
                {
                    FailureRatio = 0.5,              // Open circuit if 50% of requests fail
                    SamplingDuration = TimeSpan.FromSeconds(30),
                    MinimumThroughput = 3,           // Minimum requests before breaking
                    BreakDuration = TimeSpan.FromSeconds(30),
                    OnOpened = args =>
                    {
                        _logger.LogWarning(
                            "🔴 Circuit Breaker OPEN: RabbitMQ unavailable for {Duration}s. Events will be queued to DLQ.",
                            args.BreakDuration.TotalSeconds);
                        return ValueTask.CompletedTask;
                    },
                    OnClosed = args =>
                    {
                        _logger.LogInformation("🟢 Circuit Breaker CLOSED: RabbitMQ connection restored.");
                        return ValueTask.CompletedTask;
                    },
                    OnHalfOpened = args =>
                    {
                        _logger.LogInformation("🟡 Circuit Breaker HALF-OPEN: Testing RabbitMQ connection...");
                        return ValueTask.CompletedTask;
                    }
                })
                .Build();

            _logger.LogInformation("RabbitMQ Event Publisher initialized successfully with Circuit Breaker");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize RabbitMQ Event Publisher");
            throw;
        }
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IEvent
    {
        try
        {
            await _resiliencePipeline.ExecuteAsync(async ct =>
            {
                var routingKey = @event.EventType; // e.g., "auth.user.registered"
                var message = JsonSerializer.Serialize(@event, _jsonOptions);
                var body = Encoding.UTF8.GetBytes(message);

                var properties = _channel.CreateBasicProperties();
                properties.Persistent = true;
                properties.MessageId = @event.EventId.ToString();
                properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                properties.Type = @event.EventType;
                properties.ContentType = "application/json";

                _channel.BasicPublish(
                    exchange: _exchangeName,
                    routingKey: routingKey,
                    basicProperties: properties,
                    body: body);

                _logger.LogInformation(
                    "Published event {EventType} with EventId={EventId} to exchange={Exchange}, routingKey={RoutingKey}",
                    @event.EventType, @event.EventId, _exchangeName, routingKey);

                return ValueTask.CompletedTask;
            }, cancellationToken);
        }
        catch (BrokenCircuitException ex)
        {
            _logger.LogWarning(ex,
                "⚠️ Circuit Breaker OPEN: Cannot publish event {EventType} with ID {EventId}. Queuing to DLQ.",
                @event.EventType, @event.EventId);

            if (_deadLetterQueue != null)
            {
                var failedEvent = new FailedEvent
                {
                    EventType = @event.EventType,
                    EventJson = JsonSerializer.Serialize(@event, _jsonOptions),
                    FailedAt = DateTime.UtcNow,
                    RetryCount = 0
                };
                failedEvent.ScheduleNextRetry();
                _deadLetterQueue.Enqueue(failedEvent);

                _logger.LogInformation(
                    "📮 Event {EventId} queued to DLQ for retry in {Minutes} minute(s)",
                    failedEvent.Id, 1);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "❌ Failed to publish event {EventType} with EventId={EventId}",
                @event.EventType, @event.EventId);
            throw;
        }
    }

    public void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();

        _logger.LogInformation("RabbitMQ Event Publisher disposed");
    }
}
