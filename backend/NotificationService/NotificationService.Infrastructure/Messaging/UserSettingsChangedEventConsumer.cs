using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using CarDealer.Contracts.Events.User;
using NotificationService.Application.Interfaces;

namespace NotificationService.Infrastructure.Messaging;

/// <summary>
/// Consumer that listens to user.settings.changed events from UserService
/// and creates an in-app confirmation notification for the user.
///
/// Flow:
/// 1. User saves settings in /cuenta/configuracion
/// 2. UserService publishes UserSettingsChangedEvent (user.settings.changed)
/// 3. This consumer creates a UserNotification confirming the change
/// 4. The notification appears in the header bell and /cuenta/notificaciones
/// </summary>
public class UserSettingsChangedEventConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<UserSettingsChangedEventConsumer> _logger;
    private readonly IConfiguration _configuration;
    private IConnection? _connection;
    private IModel? _channel;

    private const string ExchangeName = "cardealer.events";
    private const string QueueName = "notificationservice.user.settings.changed";
    private const string RoutingKey = "user.settings.changed";
    private const string DeadLetterExchange = "cardealer.events.dlx";
    private const string DeadLetterQueue = "notificationservice.user.settings.changed.dlq";
    private const int MaxRetryAttempts = 3;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public UserSettingsChangedEventConsumer(
        IConfiguration configuration,
        IServiceProvider serviceProvider,
        ILogger<UserSettingsChangedEventConsumer> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var rabbitEnabled = _configuration.GetValue<bool>("RabbitMQ:Enabled");
        if (!rabbitEnabled)
        {
            _logger.LogInformation("RabbitMQ is disabled. UserSettingsChangedEventConsumer will not start.");
            return;
        }

        await Task.Delay(5000, stoppingToken);

        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMQ:Host"] ?? "localhost",
                Port = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672"),
                UserName = _configuration["RabbitMQ:Username"]
                    ?? throw new InvalidOperationException("RabbitMQ:Username is not configured"),
                Password = _configuration["RabbitMQ:Password"]
                    ?? throw new InvalidOperationException("RabbitMQ:Password is not configured"),
                VirtualHost = _configuration["RabbitMQ:VirtualHost"] ?? "/",
                DispatchConsumersAsync = true
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(exchange: DeadLetterExchange, type: ExchangeType.Direct, durable: true);
            _channel.QueueDeclare(queue: DeadLetterQueue, durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind(DeadLetterQueue, DeadLetterExchange, RoutingKey);

            _channel.ExchangeDeclare(exchange: ExchangeName, type: ExchangeType.Topic, durable: true);

            var queueArgs = new Dictionary<string, object>
            {
                { "x-dead-letter-exchange", DeadLetterExchange },
                { "x-dead-letter-routing-key", RoutingKey }
            };

            _channel.QueueDeclare(
                queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: queueArgs);

            _channel.QueueBind(queue: QueueName, exchange: ExchangeName, routingKey: RoutingKey);

            _logger.LogInformation(
                "UserSettingsChangedEventConsumer initialized. Queue: {Queue}, RoutingKey: {RoutingKey}",
                QueueName, RoutingKey);

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.Received += async (model, ea) =>
            {
                var retryCount = GetRetryCount(ea.BasicProperties);

                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var settingsEvent = JsonSerializer.Deserialize<UserSettingsChangedEvent>(message, _jsonOptions);

                    if (settingsEvent == null || settingsEvent.UserId == Guid.Empty)
                    {
                        _logger.LogWarning("UserSettingsChangedEvent deserialization failed or UserId empty. Sending to DLQ.");
                        _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
                        return;
                    }

                    _logger.LogInformation(
                        "Received UserSettingsChangedEvent: UserId={UserId}, ChangeType={ChangeType}, Retry={Retry}",
                        settingsEvent.UserId, settingsEvent.ChangeType, retryCount);

                    await HandleEventAsync(settingsEvent, stoppingToken);

                    _channel.BasicAck(ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing UserSettingsChangedEvent. Attempt {Attempt}/{Max}",
                        retryCount + 1, MaxRetryAttempts);

                    if (retryCount >= MaxRetryAttempts - 1)
                    {
                        _logger.LogWarning("Max retries reached for UserSettingsChangedEvent. Sending to DLQ.");
                        _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
                    }
                    else
                    {
                        var delayMs = (int)Math.Pow(2, retryCount + 1) * 1000;
                        await Task.Delay(delayMs, stoppingToken);
                        _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
                    }
                }
            };

            _channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);
            _logger.LogInformation("UserSettingsChangedEventConsumer started successfully");

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("UserSettingsChangedEventConsumer stopping.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize UserSettingsChangedEventConsumer");
        }
    }

    private async Task HandleEventAsync(UserSettingsChangedEvent settingsEvent, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var userNotifService = scope.ServiceProvider.GetService<IUserNotificationService>();

            if (userNotifService == null)
            {
                _logger.LogWarning(
                    "IUserNotificationService not available for UserSettingsChangedEvent. UserId={UserId}",
                    settingsEvent.UserId);
                return;
            }

            var (title, message, icon) = settingsEvent.ChangeType switch
            {
                "locale" => (
                    "⚙️ Idioma/moneda actualizado",
                    "Has actualizado el idioma y/o moneda de la plataforma. Los cambios se aplican de inmediato.",
                    "⚙️"
                ),
                _ => (
                    "⚙️ Configuración actualizada",
                    "Has actualizado tus preferencias de notificación. Puedes cambiarlas en cualquier momento desde tu perfil.",
                    "⚙️"
                )
            };

            await userNotifService.CreateAsync(
                userId: settingsEvent.UserId,
                type: "system",
                title: title,
                message: message,
                icon: icon,
                link: "/cuenta/configuracion",
                cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Settings-change notification created for UserId={UserId}, ChangeType={ChangeType}",
                settingsEvent.UserId, settingsEvent.ChangeType);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to create settings notification for UserId={UserId}. Non-critical.",
                settingsEvent.UserId);
        }
    }

    private static int GetRetryCount(IBasicProperties? properties)
    {
        if (properties?.Headers == null) return 0;

        if (properties.Headers.TryGetValue("x-death", out var deathHeader))
        {
            if (deathHeader is List<object> deaths && deaths.Count > 0)
            {
                if (deaths[0] is Dictionary<string, object> death &&
                    death.TryGetValue("count", out var count))
                    return Convert.ToInt32(count);
            }
        }

        if (properties.Headers.TryGetValue("x-retry-count", out var retryCount))
            return Convert.ToInt32(retryCount);

        return 0;
    }

    public override void Dispose()
    {
        try { _channel?.Close(); } catch { /* ignore */ }
        try { _connection?.Close(); } catch { /* ignore */ }
        try { _channel?.Dispose(); } catch { /* ignore */ }
        try { _connection?.Dispose(); } catch { /* ignore */ }
        base.Dispose();
    }
}
