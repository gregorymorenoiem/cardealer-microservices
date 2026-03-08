using CarDealer.Contracts.Events.Auth;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using UserService.Infrastructure.Persistence;

namespace UserService.Infrastructure.Services.Messaging;

/// <summary>
/// Consumer for UserLoggedInEvent from AuthService.
/// Updates LastLoginAt field on the User record when a user logs in.
/// </summary>
public class UserLoggedInEventConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<UserLoggedInEventConsumer> _logger;
    private readonly RabbitMQSettings _rabbitMQSettings;
    private IConnection? _connection;
    private IModel? _channel;
    private readonly JsonSerializerOptions _jsonOptions;

    private const string ExchangeName = "cardealer.events";
    private const string QueueName = "userservice.user.loggedin";
    private const string RoutingKey = "auth.user.loggedin";

    public UserLoggedInEventConsumer(
        IOptions<RabbitMQSettings> rabbitMqSettings,
        IServiceProvider serviceProvider,
        ILogger<UserLoggedInEventConsumer> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _rabbitMQSettings = rabbitMqSettings.Value;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Wait a bit for RabbitMQ to be ready
        await Task.Delay(5000, stoppingToken);

        // Retry loop — matches UserRegisteredEventConsumer pattern
        const int maxRetries = 10;
        for (var attempt = 1; attempt <= maxRetries; attempt++)
        {
            if (stoppingToken.IsCancellationRequested) return;

            InitializeRabbitMQ();
            if (_channel != null) break;

            var delay = TimeSpan.FromSeconds(Math.Min(attempt * 5, 60));
            _logger.LogWarning(
                "UserLoggedInEventConsumer: RabbitMQ not available, retry {Attempt}/{MaxRetries} in {Delay}s",
                attempt, maxRetries, delay.TotalSeconds);
            await Task.Delay(delay, stoppingToken);
        }

        try
        {
            if (_channel == null)
            {
                _logger.LogError("RabbitMQ channel not available after {MaxRetries} retries, UserLoggedInEventConsumer will not start", maxRetries);
                return;
            }

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);

                    _logger.LogInformation("Received UserLoggedInEvent: {Message}", message);

                    var @event = JsonSerializer.Deserialize<UserLoggedInEvent>(message, _jsonOptions);

                    if (@event != null)
                    {
                        await ProcessEventAsync(@event, stoppingToken);
                        _channel.BasicAck(ea.DeliveryTag, false);
                        _logger.LogInformation("Successfully processed UserLoggedInEvent for user {UserId}", @event.UserId);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to deserialize UserLoggedInEvent");
                        _channel.BasicNack(ea.DeliveryTag, false, false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing UserLoggedInEvent");
                    _channel.BasicNack(ea.DeliveryTag, false, true); // Requeue
                }
            };

            _channel.BasicConsume(
                queue: QueueName,
                autoAck: false,
                consumer: consumer);

            _logger.LogInformation("UserLoggedInEventConsumer started, listening on queue: {Queue}", QueueName);

            // Keep running until cancellation
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UserLoggedInEventConsumer");
        }
    }

    private void InitializeRabbitMQ()
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _rabbitMQSettings.Host,
                Port = _rabbitMQSettings.Port,
                UserName = _rabbitMQSettings.Username,
                Password = _rabbitMQSettings.Password,
                VirtualHost = _rabbitMQSettings.VirtualHost,
                DispatchConsumersAsync = true
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declare exchange (topic type for event routing)
            _channel.ExchangeDeclare(
                exchange: ExchangeName,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false);

            // Declare queue
            _channel.QueueDeclare(
                queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            // Bind queue to exchange with routing key
            _channel.QueueBind(
                queue: QueueName,
                exchange: ExchangeName,
                routingKey: RoutingKey);

            _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            _logger.LogInformation("RabbitMQ UserLoggedInEventConsumer initialized - Exchange: {Exchange}, Queue: {Queue}, RoutingKey: {RoutingKey}",
                ExchangeName, QueueName, RoutingKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize RabbitMQ for UserLoggedInEventConsumer");
        }
    }

    private async Task ProcessEventAsync(UserLoggedInEvent @event, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var user = await dbContext.Users.FirstOrDefaultAsync(
            u => u.Id == @event.UserId, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found in UserService, cannot update LastLoginAt", @event.UserId);
            return;
        }

        user.LastLoginAt = @event.LoggedInAt;
        user.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated LastLoginAt for user {UserId} ({Email}) to {LoggedInAt}",
            @event.UserId, @event.Email, @event.LoggedInAt);
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}
