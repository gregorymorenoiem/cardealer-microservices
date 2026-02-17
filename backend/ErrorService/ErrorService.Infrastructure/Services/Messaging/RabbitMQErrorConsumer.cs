using ErrorService.Domain.Entities;
using ErrorService.Domain.Interfaces;
using ErrorService.Application.Helpers;
using ErrorService.Shared.ErrorMessages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace ErrorService.Infrastructure.Services.Messaging;

public class RabbitMQSettings
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
}

public class ErrorServiceRabbitMQSettings
{
    public string QueueName { get; set; } = "error-queue";
    public string ExchangeName { get; set; } = "error-exchange";
    public string RoutingKey { get; set; } = "error.routing.key";
}

public class RabbitMQErrorConsumer : BackgroundService
{
    private const int MaxRetryCount = 5;
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RabbitMQErrorConsumer> _logger;
    private readonly ErrorServiceRabbitMQSettings _settings;
    private readonly JsonSerializerOptions _jsonOptions;

    public RabbitMQErrorConsumer(
        IOptions<RabbitMQSettings> rabbitMqSettings,
        IOptions<ErrorServiceRabbitMQSettings> errorServiceSettings,
        IServiceProvider serviceProvider,
        ILogger<RabbitMQErrorConsumer> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _settings = errorServiceSettings.Value;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        try
        {
            var factory = new ConnectionFactory
            {
                HostName = rabbitMqSettings.Value.HostName,
                Port = rabbitMqSettings.Value.Port,
                UserName = rabbitMqSettings.Value.UserName,
                Password = rabbitMqSettings.Value.Password,
                VirtualHost = rabbitMqSettings.Value.VirtualHost,
                DispatchConsumersAsync = true,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declarar exchange y queue (mismo que el productor)
            _channel.ExchangeDeclare(
                exchange: _settings.ExchangeName,
                type: ExchangeType.Direct,
                durable: true,
                autoDelete: false);

            // Dead Letter Exchange for persistent DLQ (CRIT-01: replaces InMemoryDeadLetterQueue)
            var dlxExchange = $"{_settings.ExchangeName}.dlx";
            var dlqQueue = $"{_settings.QueueName}.dlq";
            _channel.ExchangeDeclare(dlxExchange, ExchangeType.Direct, durable: true, autoDelete: false);
            _channel.QueueDeclare(dlqQueue, durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind(dlqQueue, dlxExchange, _settings.RoutingKey);

            // Main queue with DLX arguments — rejected messages routed to DLQ automatically
            var queueArgs = new Dictionary<string, object>
            {
                { "x-dead-letter-exchange", dlxExchange },
                { "x-dead-letter-routing-key", _settings.RoutingKey }
            };
            _channel.QueueDeclare(
                queue: _settings.QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: queueArgs);

            _channel.QueueBind(
                queue: _settings.QueueName,
                exchange: _settings.ExchangeName,
                routingKey: _settings.RoutingKey);

            _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            _logger.LogInformation("RabbitMQ Error Consumer initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize RabbitMQ Error Consumer");
            throw;
        }
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            try
            {
                await ProcessMessageAsync(message, ea.DeliveryTag);
            }
            catch (Exception ex)
            {
                var retryCount = GetRetryCount(ea.BasicProperties);
                if (retryCount >= MaxRetryCount)
                {
                    _logger.LogError(ex, "Max retries ({MaxRetries}) exceeded for message. Sending to DLQ", MaxRetryCount);
                    _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
                }
                else
                {
                    _logger.LogWarning(ex, "Error processing message (attempt {Attempt}/{MaxRetries}). Requeueing", retryCount + 1, MaxRetryCount);
                    _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
                }
            }
        };

        _channel.BasicConsume(
            queue: _settings.QueueName,
            autoAck: false,
            consumer: consumer);

        return Task.CompletedTask;
    }

    private async Task ProcessMessageAsync(string message, ulong deliveryTag)
    {
        try
        {
            var errorEvent = JsonSerializer.Deserialize<RabbitMQErrorEvent>(message, _jsonOptions);
            if (errorEvent == null)
            {
                _logger.LogWarning("Received null or invalid error event from RabbitMQ");
                _channel.BasicAck(deliveryTag, multiple: false);
                return;
            }

            // Validate required fields
            if (string.IsNullOrWhiteSpace(errorEvent.ServiceName) || string.IsNullOrWhiteSpace(errorEvent.ErrorMessage))
            {
                _logger.LogWarning("Error event missing required fields (ServiceName or ErrorMessage). Discarding");
                _channel.BasicAck(deliveryTag, multiple: false);
                return;
            }

            if (!Guid.TryParse(errorEvent.Id, out var errorId))
            {
                _logger.LogWarning("Invalid GUID format for error event Id: {Id}. Generating new Id", errorEvent.Id);
                errorId = Guid.NewGuid();
            }

            using var scope = _serviceProvider.CreateScope();
            var errorLogRepository = scope.ServiceProvider.GetRequiredService<IErrorLogRepository>();

            // Convertir RabbitMQErrorEvent a ErrorLog entity
            var errorLog = new ErrorLog
            {
                Id = errorId,
                ServiceName = errorEvent.ServiceName,
                ExceptionType = errorEvent.ErrorCode,
                Message = errorEvent.ErrorMessage,
                  StackTrace = StackTraceSanitizer.Sanitize(errorEvent.StackTrace),
                OccurredAt = errorEvent.Timestamp,
                Endpoint = errorEvent.Endpoint,
                HttpMethod = errorEvent.HttpMethod,
                StatusCode = errorEvent.StatusCode,
                UserId = errorEvent.UserId,
                Metadata = errorEvent.Metadata ?? new Dictionary<string, object>()
            };

            await errorLogRepository.AddAsync(errorLog);

            _logger.LogInformation("Successfully processed error event from {ServiceName}: {ErrorCode}",
                errorEvent.ServiceName, errorEvent.ErrorCode);

            // Acknowledge the message
            _channel.BasicAck(deliveryTag, multiple: false);
        }
        catch (JsonException jsonEx)
        {
            _logger.LogError(jsonEx, "Failed to deserialize RabbitMQ message");
            // Reject and don't requeue malformed messages
            _channel.BasicNack(deliveryTag, multiple: false, requeue: false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing RabbitMQ message");
            // Reject and requeue for transient errors
            _channel.BasicNack(deliveryTag, multiple: false, requeue: true);
        }
    }

    private static int GetRetryCount(IBasicProperties? properties)
    {
        if (properties?.Headers == null) return 0;
        if (!properties.Headers.TryGetValue("x-death", out var xDeath)) return 0;

        if (xDeath is List<object> deathList && deathList.Count > 0)
        {
            if (deathList[0] is Dictionary<string, object> deathInfo &&
                deathInfo.TryGetValue("count", out var count))
            {
                return Convert.ToInt32(count);
            }
        }

        return 0;
    }

    public override void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
        base.Dispose();
    }
}