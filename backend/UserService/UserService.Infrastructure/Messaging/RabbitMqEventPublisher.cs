using CarDealer.Contracts.Abstractions;
using UserService.Domain.Interfaces;
using UserService.Application.Metrics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Polly;
using Polly.CircuitBreaker;

namespace UserService.Infrastructure.Messaging;

/// <summary>
/// RabbitMQ implementation of IEventPublisher with Circuit Breaker pattern.
/// Publishes events to RabbitMQ topic exchanges for consumption by other microservices.
/// Implements resilience with Polly Circuit Breaker to handle RabbitMQ unavailability.
/// Uses lazy connection to avoid startup failures when RabbitMQ is not immediately available.
/// </summary>
public class RabbitMqEventPublisher : IEventPublisher, IDisposable
{
    private IConnection? _connection;
    private IModel? _channel;
    private readonly ILogger<RabbitMqEventPublisher> _logger;
    private readonly UserServiceMetrics _metrics;
    private readonly IDeadLetterQueue? _deadLetterQueue;
    private readonly string _exchangeName;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ResiliencePipeline _resiliencePipeline;
    private readonly IConfiguration _configuration;
    private readonly object _connectionLock = new();
    private bool _isConnected;

    public RabbitMqEventPublisher(
        IConfiguration configuration,
        ILogger<RabbitMqEventPublisher> logger,
        UserServiceMetrics metrics,
        IDeadLetterQueue? deadLetterQueue = null)
    {
        _logger = logger;
        _metrics = metrics;
        _deadLetterQueue = deadLetterQueue;
        _configuration = configuration;
        _exchangeName = configuration["RabbitMQ:ExchangeName"] ?? "cardealer.events";

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        // Configure Circuit Breaker with Polly v8
        _resiliencePipeline = new ResiliencePipelineBuilder()
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = 0.5,
                SamplingDuration = TimeSpan.FromSeconds(30),
                MinimumThroughput = 3,
                BreakDuration = TimeSpan.FromSeconds(30),
                OnOpened = args =>
                {
                    _metrics.SetCircuitBreakerState(CircuitBreakerState.Open);
                    _logger.LogWarning(
                        "Circuit Breaker OPEN: RabbitMQ unavailable for {Duration}s. " +
                        "Events will be logged but not published.",
                        args.BreakDuration.TotalSeconds);
                    return ValueTask.CompletedTask;
                },
                OnClosed = args =>
                {
                    _metrics.SetCircuitBreakerState(CircuitBreakerState.Closed);
                    _logger.LogInformation(
                        "Circuit Breaker CLOSED: RabbitMQ connection restored. " +
                        "Resuming event publishing.");
                    return ValueTask.CompletedTask;
                },
                OnHalfOpened = args =>
                {
                    _metrics.SetCircuitBreakerState(CircuitBreakerState.HalfOpen);
                    _logger.LogInformation(
                        "Circuit Breaker HALF-OPEN: Testing RabbitMQ connection...");
                    return ValueTask.CompletedTask;
                }
            })
            .Build();

        _logger.LogInformation(
            "RabbitMQ Event Publisher initialized with Circuit Breaker (lazy connection). " +
            "Exchange: {Exchange}",
            _exchangeName);
    }

    /// <summary>
    /// Ensures RabbitMQ connection is established. Uses lazy initialization.
    /// </summary>
    private bool EnsureConnected()
    {
        if (_isConnected && _channel != null && _channel.IsOpen)
            return true;

        lock (_connectionLock)
        {
            if (_isConnected && _channel != null && _channel.IsOpen)
                return true;

            try
            {
                // Support both naming conventions: RabbitMQ:Host and RabbitMQ:HostName
                var host = _configuration["RabbitMQ:Host"]
                    ?? _configuration["RabbitMQ:HostName"]
                    ?? "localhost";
                var portStr = _configuration["RabbitMQ:Port"] ?? "5672";
                var username = _configuration["RabbitMQ:Username"]
                    ?? _configuration["RabbitMQ:UserName"]
                    ?? "guest";
                var password = _configuration["RabbitMQ:Password"] ?? "guest";
                var virtualHost = _configuration["RabbitMQ:VirtualHost"] ?? "/";

                var factory = new ConnectionFactory
                {
                    HostName = host,
                    Port = int.Parse(portStr),
                    UserName = username,
                    Password = password,
                    VirtualHost = virtualHost,
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

                _isConnected = true;
                _logger.LogInformation(
                    "RabbitMQ connection established to {Host}:{Port}",
                    host, portStr);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Could not connect to RabbitMQ. Events will be queued to DLQ if available.");
                _isConnected = false;
                return false;
            }
        }
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IEvent
    {
        try
        {
            // Execute with Circuit Breaker protection
            await _resiliencePipeline.ExecuteAsync(async ct =>
            {
                if (!EnsureConnected() || _channel == null)
                {
                    throw new InvalidOperationException("RabbitMQ not connected");
                }

                var routingKey = @event.EventType;
                var messageBody = JsonSerializer.Serialize(@event, _jsonOptions);
                var body = Encoding.UTF8.GetBytes(messageBody);

                var properties = _channel.CreateBasicProperties();
                properties.Persistent = true;
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

                await Task.CompletedTask;
            }, cancellationToken);
        }
        catch (BrokenCircuitException ex)
        {
            _logger.LogWarning(ex,
                "Circuit Breaker OPEN: Cannot publish event {EventType} with ID {EventId}. " +
                "Event queued for retry when RabbitMQ recovers.",
                @event.EventType, @event.EventId);

            QueueToDeadLetter(@event);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to publish event {EventType} with ID {EventId}. Queuing to DLQ.",
                @event.EventType, @event.EventId);

            QueueToDeadLetter(@event);
        }
    }

    private void QueueToDeadLetter<TEvent>(TEvent @event) where TEvent : IEvent
    {
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
                "Event {EventId} queued to DLQ for retry",
                @event.EventId);
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
