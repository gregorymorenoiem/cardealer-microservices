using UserService.Domain.Entities;
using UserService.Domain.Interfaces;
using UserService.Shared.ErrorMessages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace UserService.Infrastructure.Services.Messaging;

public class RabbitMQSettings
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
}

public class UserServiceRabbitMQSettings
{
    public string QueueName { get; set; } = "error-queue";
    public string ExchangeName { get; set; } = "error-exchange";
    public string RoutingKey { get; set; } = "error.routing.key";
}

public class RabbitMQErrorConsumer : BackgroundService
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RabbitMQErrorConsumer> _logger;
    private readonly UserServiceRabbitMQSettings _settings;
    private readonly JsonSerializerOptions _jsonOptions;

    public RabbitMQErrorConsumer(
        IOptions<RabbitMQSettings> rabbitMqSettings,
        IOptions<UserServiceRabbitMQSettings> UserServiceSettings,
        IServiceProvider serviceProvider,
        ILogger<RabbitMQErrorConsumer> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _settings = UserServiceSettings.Value;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
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

            // Declarar exchange y queue (mismo que el productor)
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
                _logger.LogError(ex, "Error processing RabbitMQ message");
                // Reject and requeue the message
                _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
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

            using var scope = _serviceProvider.CreateScope();
            var RoleRepository = scope.ServiceProvider.GetRequiredService<IRoleRepository>();

            // Convertir RabbitMQErrorEvent a Role entity
            var Role = new Role
            {
                Id = Guid.Parse(errorEvent.Id),
                ServiceName = errorEvent.ServiceName,
                ExceptionType = errorEvent.ErrorCode,
                Message = errorEvent.ErrorMessage,
                StackTrace = errorEvent.StackTrace,
                OccurredAt = errorEvent.Timestamp,
                Endpoint = errorEvent.Endpoint,
                HttpMethod = errorEvent.HttpMethod,
                StatusCode = errorEvent.StatusCode,
                UserId = errorEvent.UserId,
                Metadata = errorEvent.Metadata ?? new Dictionary<string, object>()
            };

            await RoleRepository.AddAsync(Role);

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

    public override void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
        base.Dispose();
    }
}
