using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using CarDealer.Contracts.Events.Vehicle;
using NotificationService.Domain.Interfaces;
using NotificationService.Application.DTOs;

namespace NotificationService.Infrastructure.Messaging;

/// <summary>
/// Background service que escucha eventos de vehículos creados
/// y envía notificaciones a los dealers
/// </summary>
public class VehicleCreatedNotificationConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<VehicleCreatedNotificationConsumer> _logger;
    private readonly IConfiguration _configuration;
    private IConnection? _connection;
    private IModel? _channel;
    private const string ExchangeName = "cardealer.events";
    private const string QueueName = "notificationservice.vehicle.created";
    private const string RoutingKey = "vehicle.created";

    public VehicleCreatedNotificationConsumer(
        IServiceProvider serviceProvider,
        ILogger<VehicleCreatedNotificationConsumer> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Verificar si RabbitMQ está habilitado
        var rabbitMQEnabled = _configuration.GetValue<bool>("RabbitMQ:Enabled");
        if (!rabbitMQEnabled)
        {
            _logger.LogInformation("RabbitMQ is disabled. VehicleCreatedNotificationConsumer will not start.");
            return;
        }

        try
        {
            InitializeRabbitMQ();

            if (_channel == null)
            {
                _logger.LogError("Failed to initialize RabbitMQ channel");
                return;
            }

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                try
                {
                    var vehicleCreatedEvent = JsonSerializer.Deserialize<VehicleCreatedEvent>(message);

                    if (vehicleCreatedEvent != null)
                    {
                        _logger.LogInformation(
                            "Received VehicleCreatedEvent: VehicleId={VehicleId}, Make={Make}, Model={Model}, Year={Year}",
                            vehicleCreatedEvent.VehicleId,
                            vehicleCreatedEvent.Make,
                            vehicleCreatedEvent.Model,
                            vehicleCreatedEvent.Year);

                        await HandleVehicleCreatedEventAsync(vehicleCreatedEvent, stoppingToken);

                        // Acknowledge message
                        _channel.BasicAck(ea.DeliveryTag, multiple: false);
                        _logger.LogDebug("Message acknowledged: {MessageId}", ea.BasicProperties.MessageId);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to deserialize VehicleCreatedEvent");
                        _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing VehicleCreatedEvent");

                    // Requeue message for retry
                    _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
                }
            };

            _channel.BasicConsume(
                queue: QueueName,
                autoAck: false,
                consumer: consumer);

            _logger.LogInformation("VehicleCreatedNotificationConsumer started listening on queue: {Queue}", QueueName);

            // Keep the service running
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in VehicleCreatedNotificationConsumer");
        }
    }

    private void InitializeRabbitMQ()
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMQ:Host"] ?? "localhost",
                Port = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672"),
                UserName = _configuration["RabbitMQ:Username"] ?? "guest",
                Password = _configuration["RabbitMQ:Password"] ?? "guest",
                VirtualHost = _configuration["RabbitMQ:VirtualHost"] ?? "/",
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declarar exchange
            _channel.ExchangeDeclare(
                exchange: ExchangeName,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false);

            // Declarar queue
            _channel.QueueDeclare(
                queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            // Bind queue al exchange
            _channel.QueueBind(
                queue: QueueName,
                exchange: ExchangeName,
                routingKey: RoutingKey);

            // Set prefetch count
            _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            _logger.LogInformation("RabbitMQ initialized successfully for VehicleCreatedNotificationConsumer");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize RabbitMQ connection");
            throw;
        }
    }

    private async Task HandleVehicleCreatedEventAsync(
        VehicleCreatedEvent eventData,
        CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        try
        {
            // Obtener información del dealer (en producción, esto vendría de UserService)
            // Por ahora usamos datos del evento
            var dealerEmail = "dealer@example.com"; // TODO: Obtener email real del dealer

            var subject = $"Nuevo Vehículo Publicado: {eventData.Year} {eventData.Make} {eventData.Model}";

            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2 style='color: #2c3e50;'>🚗 Nuevo Vehículo Publicado</h2>
                    <p>Se ha publicado un nuevo vehículo en tu cuenta:</p>
                    
                    <div style='background-color: #f8f9fa; padding: 20px; border-radius: 5px; margin: 20px 0;'>
                        <h3 style='margin-top: 0; color: #007bff;'>{eventData.Year} {eventData.Make} {eventData.Model}</h3>
                        <p><strong>VIN:</strong> {eventData.VIN}</p>
                        <p><strong>Precio:</strong> ${eventData.Price:N2}</p>
                        <p><strong>ID del Vehículo:</strong> {eventData.VehicleId}</p>
                        <p><strong>Fecha de Publicación:</strong> {eventData.CreatedAt:dd/MM/yyyy HH:mm}</p>
                    </div>
                    
                    <p>Tu vehículo ahora está visible para los compradores en la plataforma.</p>
                    
                    <p style='margin-top: 30px;'>
                        <a href='https://cardealer.com/vehicles/{eventData.VehicleId}' 
                           style='background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>
                            Ver Vehículo
                        </a>
                    </p>
                    
                    <hr style='margin: 30px 0; border: none; border-top: 1px solid #dee2e6;'>
                    
                    <p style='color: #6c757d; font-size: 12px;'>
                        Este es un mensaje automático de CarDealer. No responder a este correo.
                    </p>
                </body>
                </html>
            ";

            await emailService.SendEmailAsync(
                to: dealerEmail,
                subject: subject,
                body: body,
                isHtml: true);

            _logger.LogInformation(
                "Vehicle creation notification sent to dealer for VehicleId: {VehicleId}",
                eventData.VehicleId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send vehicle creation notification for VehicleId: {VehicleId}",
                eventData.VehicleId);
            throw;
        }
    }

    public override void Dispose()
    {
        try
        {
            _channel?.Close();
            _connection?.Close();
            _channel?.Dispose();
            _connection?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disposing RabbitMQ connection");
        }

        base.Dispose();
    }
}
