using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using CarDealer.Contracts.Events.Auth;
using NotificationService.Domain.Interfaces;

namespace NotificationService.Infrastructure.Messaging;

/// <summary>
/// Consumer que escucha eventos de registro de usuarios desde AuthService
/// y envía email de bienvenida automáticamente
/// </summary>
public class UserRegisteredNotificationConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<UserRegisteredNotificationConsumer> _logger;
    private readonly IConfiguration _configuration;
    private IConnection? _connection;
    private IModel? _channel;
    private const string ExchangeName = "cardealer.events";
    private const string QueueName = "notificationservice.user.registered";
    private const string RoutingKey = "auth.user.registered";

    public UserRegisteredNotificationConsumer(
        IConfiguration configuration,
        IServiceProvider serviceProvider,
        ILogger<UserRegisteredNotificationConsumer> logger)
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
            _logger.LogInformation("RabbitMQ is disabled. UserRegisteredNotificationConsumer will not start.");
            return;
        }

        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMQ:Host"] ?? "localhost",
                Port = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672"),
                UserName = _configuration["RabbitMQ:Username"] ?? "guest",
                Password = _configuration["RabbitMQ:Password"] ?? "guest",
                VirtualHost = _configuration["RabbitMQ:VirtualHost"] ?? "/",
                DispatchConsumersAsync = true
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declarar exchange
            _channel.ExchangeDeclare(
                exchange: ExchangeName,
                type: ExchangeType.Topic,
                durable: true
            );

            // Declarar queue
            _channel.QueueDeclare(
                queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false
            );

            // Bind queue al exchange con routing key
            _channel.QueueBind(
                queue: QueueName,
                exchange: ExchangeName,
                routingKey: RoutingKey
            );

            _logger.LogInformation("UserRegisteredNotificationConsumer initialized for queue: {Queue}", QueueName);

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var userRegisteredEvent = JsonSerializer.Deserialize<UserRegisteredEvent>(message);

                    if (userRegisteredEvent != null)
                    {
                        _logger.LogInformation(
                            "Received UserRegisteredEvent: UserId={UserId}, Email={Email}",
                            userRegisteredEvent.UserId,
                            userRegisteredEvent.Email);

                        await HandleEventAsync(userRegisteredEvent, stoppingToken);

                        _channel.BasicAck(ea.DeliveryTag, multiple: false);
                        _logger.LogInformation("Successfully processed UserRegisteredEvent for {Email}", userRegisteredEvent.Email);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing UserRegisteredEvent");

                    // Requeue si falla
                    _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
                }
            };

            _channel.BasicConsume(
                queue: QueueName,
                autoAck: false,
                consumer: consumer
            );

            _logger.LogInformation("UserRegisteredNotificationConsumer started successfully");

            // Mantener el consumer activo
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize UserRegisteredNotificationConsumer");
        }
    }

    private async Task HandleEventAsync(UserRegisteredEvent eventData, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        try
        {
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            // Enviar email de bienvenida
            var subject = "¡Bienvenido a CarDealer!";
            var body = $@"
                <h1>¡Hola {eventData.FullName}!</h1>
                <p>Gracias por registrarte en CarDealer.</p>
                <p>Tu cuenta ha sido creada exitosamente con el email: <strong>{eventData.Email}</strong></p>
                <p>Ahora puedes empezar a publicar tus vehículos o buscar el auto de tus sueños.</p>
                <br>
                <p>¡Bienvenido a bordo!</p>
                <p><em>El equipo de CarDealer</em></p>
            ";

            await emailService.SendEmailAsync(eventData.Email, subject, body);

            _logger.LogInformation("Welcome email sent to {Email}", eventData.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send welcome email to {Email}", eventData.Email);
            throw; // Re-throw para que RabbitMQ requeue el mensaje
        }
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
