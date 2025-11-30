using CarDealer.Contracts.Abstractions;
using ErrorService.Domain.Interfaces;
using ErrorService.Application.Metrics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Polly;
using Polly.CircuitBreaker;

namespace ErrorService.Infrastructure.Messaging;

/// <summary>
/// RabbitMQ implementation of IEventPublisher with Circuit Breaker pattern.
/// Publishes events to RabbitMQ topic exchanges for consumption by other microservices.
/// Implements resilience with Polly Circuit Breaker to handle RabbitMQ unavailability.
/// </summary>
public class RabbitMqEventPublisher : IEventPublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMqEventPublisher> _logger;
    private readonly ErrorServiceMetrics _metrics;
    private readonly string _exchangeName;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ResiliencePipeline _resiliencePipeline;

    public RabbitMqEventPublisher(
        IConfiguration configuration,
        ILogger<RabbitMqEventPublisher> logger,
        ErrorServiceMetrics metrics)
    {
        _logger = logger;
        _metrics = metrics;
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

        // Configure Circuit Breaker with Polly v8
        _resiliencePipeline = new ResiliencePipelineBuilder()
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = 0.5, // Open circuit if 50% of requests fail
                SamplingDuration = TimeSpan.FromSeconds(30), // Sample window
                MinimumThroughput = 3, // Minimum requests before breaking
                BreakDuration = TimeSpan.FromSeconds(30), // Circuit stays open for 30s
                OnOpened = args =>
                {
                    _metrics.SetCircuitBreakerState(CircuitBreakerState.Open);
                    _logger.LogWarning(
                        "🔴 Circuit Breaker OPEN: RabbitMQ unavailable for {Duration}s. " +
                        "Events will be logged but not published.",
                        args.BreakDuration.TotalSeconds);
                    return ValueTask.CompletedTask;
                },
                OnClosed = args =>
                {
                    _metrics.SetCircuitBreakerState(CircuitBreakerState.Closed);
                    _logger.LogInformation(
                        "🟢 Circuit Breaker CLOSED: RabbitMQ connection restored. " +
                        "Resuming event publishing.");
                    return ValueTask.CompletedTask;
                },
                OnHalfOpened = args =>
                {
                    _metrics.SetCircuitBreakerState(CircuitBreakerState.HalfOpen);
                    _logger.LogInformation(
                        "🟡 Circuit Breaker HALF-OPEN: Testing RabbitMQ connection...");
                    return ValueTask.CompletedTask;
                }
            })
            .Build();

        _logger.LogInformation(
            "RabbitMQ Event Publisher initialized with Circuit Breaker. " +
            "Exchange: {Exchange}, Host: {Host}",
            _exchangeName, factory.HostName);
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IEvent
    {
        try
        {
            // Execute with Circuit Breaker protection
            await _resiliencePipeline.ExecuteAsync(async ct =>
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

                return ValueTask.CompletedTask;
            }, cancellationToken);
        }
        catch (BrokenCircuitException ex)
        {
            // Circuit is open, log the event but don't fail the request
            _logger.LogWarning(ex,
                "⚠️ Circuit Breaker OPEN: Cannot publish event {EventType} with ID {EventId}. " +
                "Event logged locally but not sent to RabbitMQ.",
                @event.EventType, @event.EventId);

            // TODO: Consider implementing a local queue or dead-letter storage
            // for events that couldn't be published during circuit open state
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "❌ Failed to publish event {EventType} with ID {EventId}",
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
