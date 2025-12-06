using AuthService.Shared.ErrorMessages;
using AuthService.Shared.Messaging;
using AuthService.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Polly;
using Polly.CircuitBreaker;

namespace AuthService.Infrastructure.Services.Messaging;


public interface IErrorEventProducer
{
    Task PublishErrorAsync(RabbitMQErrorEvent errorEvent);
    Task PublishErrorAsync(string errorCode, string errorMessage, string? stackTrace = null, string? userId = null, Dictionary<string, object>? metadata = null);
}

public class RabbitMQErrorProducer : IErrorEventProducer, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ErrorServiceRabbitMQSettings _settings;
    private readonly ILogger<RabbitMQErrorProducer> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly IDeadLetterQueue? _deadLetterQueue;
    private readonly ResiliencePipeline _resiliencePipeline;

    public RabbitMQErrorProducer(
        IOptions<RabbitMQSettings> rabbitMqSettings,
        IOptions<ErrorServiceRabbitMQSettings> errorServiceSettings,
        ILogger<RabbitMQErrorProducer> logger,
        IDeadLetterQueue? deadLetterQueue = null)
    {
        _settings = errorServiceSettings.Value;
        _logger = logger;
        _deadLetterQueue = deadLetterQueue;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        try
        {
            var factory = new ConnectionFactory
            {
                HostName = rabbitMqSettings.Value.Host,
                Port = rabbitMqSettings.Value.Port,
                UserName = rabbitMqSettings.Value.Username,
                Password = rabbitMqSettings.Value.Password,
                VirtualHost = rabbitMqSettings.Value.VirtualHost,
                DispatchConsumersAsync = true,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declarar exchange y queue
            _channel.ExchangeDeclare(
                exchange: _settings.ExchangeName,
                type: ExchangeType.Direct,
                durable: true,
                autoDelete: false);

            _channel.QueueDeclare(
                queue: _settings.QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            _channel.QueueBind(
                queue: _settings.QueueName,
                exchange: _settings.ExchangeName,
                routingKey: _settings.RoutingKey);

            // Configure Circuit Breaker
            _resiliencePipeline = new ResiliencePipelineBuilder()
                .AddCircuitBreaker(new CircuitBreakerStrategyOptions
                {
                    FailureRatio = 0.5,
                    SamplingDuration = TimeSpan.FromSeconds(30),
                    MinimumThroughput = 3,
                    BreakDuration = TimeSpan.FromSeconds(30),
                    OnOpened = args =>
                    {
                        _logger.LogWarning("🔴 Error Producer Circuit Breaker OPEN: ErrorService unavailable");
                        return ValueTask.CompletedTask;
                    },
                    OnClosed = args =>
                    {
                        _logger.LogInformation("🟢 Error Producer Circuit Breaker CLOSED: ErrorService restored");
                        return ValueTask.CompletedTask;
                    }
                })
                .Build();

            _logger.LogInformation("RabbitMQ Error Producer initialized successfully with Circuit Breaker");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize RabbitMQ Error Producer");
            throw;
        }
    }

    public async Task PublishErrorAsync(RabbitMQErrorEvent errorEvent)
    {
        if (!_settings.EnableRabbitMQ)
        {
            _logger.LogWarning("RabbitMQ error publishing is disabled");
            return;
        }

        try
        {
            await _resiliencePipeline.ExecuteAsync(async ct =>
            {
                var message = JsonSerializer.Serialize(errorEvent, _jsonOptions);
                var body = Encoding.UTF8.GetBytes(message);

                var properties = _channel.CreateBasicProperties();
                properties.Persistent = true;
                properties.ContentType = "application/json";
                properties.MessageId = errorEvent.Id;
                properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

                _channel.BasicPublish(
                    exchange: _settings.ExchangeName,
                    routingKey: _settings.RoutingKey,
                    basicProperties: properties,
                    body: body);

                _logger.LogInformation("Error event published to RabbitMQ: {ErrorCode}", errorEvent.ErrorCode);
                await Task.CompletedTask;
            }, CancellationToken.None);
        }
        catch (BrokenCircuitException ex)
        {
            _logger.LogWarning(ex, "Circuit Breaker OPEN: Cannot publish error event {ErrorCode}. Queuing to DLQ.", errorEvent.ErrorCode);

            if (_deadLetterQueue != null)
            {
                var failedEvent = new FailedEvent
                {
                    EventType = "error.event",
                    EventJson = JsonSerializer.Serialize(errorEvent, _jsonOptions),
                    FailedAt = DateTime.UtcNow,
                    RetryCount = 0
                };
                failedEvent.ScheduleNextRetry();
                _deadLetterQueue.Enqueue(failedEvent);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish error event to RabbitMQ: {ErrorCode}", errorEvent.ErrorCode);
            // No throw - we don't want to break the main flow if error reporting fails
        }
    }

    public Task PublishErrorAsync(string errorCode, string errorMessage, string? stackTrace = null, string? userId = null, Dictionary<string, object>? metadata = null)
    {
        var errorEvent = new RabbitMQErrorEvent(errorCode, errorMessage, stackTrace, userId)
        {
            Metadata = metadata ?? new Dictionary<string, object>()
        };

        return PublishErrorAsync(errorEvent);
    }

    public void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
    }
}
