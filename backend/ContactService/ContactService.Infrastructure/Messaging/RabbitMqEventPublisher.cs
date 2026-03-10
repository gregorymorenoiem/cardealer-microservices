using System.Text;
using System.Text.Json;
using CarDealer.Contracts.Abstractions;
using ContactService.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using RabbitMQ.Client;

namespace ContactService.Infrastructure.Messaging;

/// <summary>
/// Publishes domain events to RabbitMQ topic exchange.
/// Uses Polly circuit breaker for resilience.
/// Follows the same pattern as AuthService, MediaService, etc.
/// </summary>
public class RabbitMqEventPublisher : IEventPublisher, IDisposable
{
    private readonly ILogger<RabbitMqEventPublisher> _logger;
    private readonly IConfiguration _configuration;
    private IConnection? _connection;
    private IModel? _channel;
    private readonly object _lock = new();
    private bool _disposed;

    private const string ExchangeName = "cardealer.events";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private readonly ResiliencePipeline _circuitBreaker;

    public RabbitMqEventPublisher(
        ILogger<RabbitMqEventPublisher> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;

        _circuitBreaker = new ResiliencePipelineBuilder()
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = 0.5,
                SamplingDuration = TimeSpan.FromSeconds(30),
                MinimumThroughput = 5,
                BreakDuration = TimeSpan.FromSeconds(30),
                OnOpened = args =>
                {
                    _logger.LogWarning("Circuit breaker OPENED for RabbitMQ publisher in ContactService");
                    return ValueTask.CompletedTask;
                },
                OnClosed = args =>
                {
                    _logger.LogInformation("Circuit breaker CLOSED for RabbitMQ publisher in ContactService");
                    return ValueTask.CompletedTask;
                }
            })
            .Build();
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IEvent
    {
        var routingKey = @event.EventType;

        await _circuitBreaker.ExecuteAsync(async token =>
        {
            EnsureConnection();

            var json = JsonSerializer.Serialize(@event, JsonOptions);
            var body = Encoding.UTF8.GetBytes(json);

            lock (_lock)
            {
                if (_channel == null)
                    throw new InvalidOperationException("RabbitMQ channel is not available");

                var properties = _channel.CreateBasicProperties();
                properties.Persistent = true;
                properties.ContentType = "application/json";
                properties.MessageId = @event.EventId.ToString();
                properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                properties.Headers = new Dictionary<string, object>
                {
                    ["event_type"] = @event.EventType,
                    ["source_service"] = "ContactService",
                    ["schema_version"] = "1"
                };

                _channel.BasicPublish(
                    exchange: ExchangeName,
                    routingKey: routingKey,
                    basicProperties: properties,
                    body: body);
            }

            _logger.LogInformation(
                "Published {EventType} to exchange {Exchange} with routing key {RoutingKey}. EventId={EventId}",
                typeof(TEvent).Name, ExchangeName, routingKey, @event.EventId);

        }, cancellationToken);
    }

    private void EnsureConnection()
    {
        lock (_lock)
        {
            if (_connection is { IsOpen: true } && _channel is { IsOpen: true })
                return;

            _channel?.Dispose();
            _connection?.Dispose();

            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMQ:Host"] ?? _configuration["RabbitMQ:HostName"] ?? "localhost",
                Port = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672"),
                UserName = _configuration["RabbitMQ:Username"] ?? _configuration["RabbitMQ:UserName"] ?? "guest",
                Password = _configuration["RabbitMQ:Password"] ?? "guest",
                VirtualHost = _configuration["RabbitMQ:VirtualHost"] ?? "/",
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            _connection = factory.CreateConnection($"ContactService-Publisher-{Environment.MachineName}");
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(
                exchange: ExchangeName,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false);

            _channel.ConfirmSelect();

            _logger.LogInformation("RabbitMQ connection established for ContactService publisher");
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        try
        {
            _channel?.Close();
            _connection?.Close();
            _channel?.Dispose();
            _connection?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disposing RabbitMQ connection in ContactService publisher");
        }

        GC.SuppressFinalize(this);
    }
}
