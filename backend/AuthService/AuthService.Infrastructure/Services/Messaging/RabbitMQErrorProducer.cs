using AuthService.Shared.ErrorMessages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
// Agregar estos alias para evitar conflictos
using RabbitMQConnection = RabbitMQ.Client.IConnection;
using RabbitMQChannel = RabbitMQ.Client.IModel;
using System.Text;
using System.Text.Json;

namespace AuthService.Infrastructure.Services.Messaging;


public interface IErrorEventProducer
{
    Task PublishErrorAsync(RabbitMQErrorEvent errorEvent);
    Task PublishErrorAsync(string errorCode, string errorMessage, string? stackTrace = null, string? userId = null, Dictionary<string, object>? metadata = null);
}

public class RabbitMQErrorProducer : IErrorEventProducer, IDisposable
{
    private readonly RabbitMQConnection _connection;
    private readonly RabbitMQChannel _channel;
    private readonly ErrorServiceRabbitMQSettings _settings;
    private readonly ILogger<RabbitMQErrorProducer> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public RabbitMQErrorProducer(
        IOptions<RabbitMQSettings> rabbitMqSettings,
        IOptions<ErrorServiceRabbitMQSettings> errorServiceSettings,
        ILogger<RabbitMQErrorProducer> logger)
    {
        _settings = errorServiceSettings.Value;
        _logger = logger;

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
                DispatchConsumersAsync = true
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

            _logger.LogInformation("RabbitMQ Error Producer initialized successfully");
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
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish error event to RabbitMQ: {ErrorCode}", errorEvent.ErrorCode);
            // No throw - we don't want to break the main flow if error reporting fails
        }
    }

    public async Task PublishErrorAsync(string errorCode, string errorMessage, string? stackTrace = null, string? userId = null, Dictionary<string, object>? metadata = null)
    {
        var errorEvent = new RabbitMQErrorEvent(errorCode, errorMessage, stackTrace, userId)
        {
            Metadata = metadata ?? new Dictionary<string, object>()
        };

        await PublishErrorAsync(errorEvent);
    }

    public void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
    }
}