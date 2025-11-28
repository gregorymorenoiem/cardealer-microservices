using System.Text;
using System.Text.Json;
using CarDealer.Contracts.Events.Error;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NotificationService.Domain.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NotificationService.Infrastructure.Messaging;

/// <summary>
/// Background service that consumes ErrorCriticalEvent messages from RabbitMQ
/// and sends alerts to Microsoft Teams.
/// </summary>
public class ErrorCriticalEventConsumer : BackgroundService
{
    private readonly ILogger<ErrorCriticalEventConsumer> _logger;
    private readonly ITeamsProvider _teamsProvider;
    private readonly IConfiguration _configuration;
    private IConnection? _connection;
    private IModel? _channel;

    public ErrorCriticalEventConsumer(
        ILogger<ErrorCriticalEventConsumer> logger,
        ITeamsProvider teamsProvider,
        IConfiguration configuration)
    {
        _logger = logger;
        _teamsProvider = teamsProvider;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ErrorCriticalEventConsumer starting...");

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
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);

                    _logger.LogInformation("Received message from RabbitMQ: {Message}", message);

                    var errorEvent = JsonSerializer.Deserialize<ErrorCriticalEvent>(message);

                    if (errorEvent != null)
                    {
                        _logger.LogInformation(
                            "Processing ErrorCriticalEvent: ErrorId={ErrorId}, Service={ServiceName}",
                            errorEvent.ErrorId,
                            errorEvent.ServiceName);

                        var success = await _teamsProvider.SendCriticalErrorAlertAsync(errorEvent, stoppingToken);

                        if (success)
                        {
                            _channel.BasicAck(ea.DeliveryTag, false);
                            _logger.LogInformation(
                                "Successfully processed and acknowledged ErrorCriticalEvent: {ErrorId}",
                                errorEvent.ErrorId);
                        }
                        else
                        {
                            // Rechazar mensaje sin requeue si falla el envío a Teams
                            _channel.BasicNack(ea.DeliveryTag, false, false);
                            _logger.LogWarning(
                                "Failed to send Teams alert, message rejected: {ErrorId}",
                                errorEvent.ErrorId);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Failed to deserialize ErrorCriticalEvent, rejecting message");
                        _channel.BasicNack(ea.DeliveryTag, false, false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing RabbitMQ message");

                    // Rechazar mensaje sin requeue en caso de excepción
                    if (_channel?.IsOpen == true)
                    {
                        _channel.BasicNack(ea.DeliveryTag, false, false);
                    }
                }
            };

            // Subscribir al queue con auto-ack deshabilitado
            _channel.BasicConsume(
                queue: "notification.error.critical",
                autoAck: false,
                consumer: consumer);

            _logger.LogInformation("ErrorCriticalEventConsumer started successfully, listening to queue: notification.error.critical");

            // Mantener el servicio ejecutándose
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("ErrorCriticalEventConsumer stopping...");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in ErrorCriticalEventConsumer");
        }
    }

    private void InitializeRabbitMQ()
    {
        try
        {
            var hostName = _configuration["RabbitMQ:HostName"] ?? "localhost";
            var port = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672");
            var userName = _configuration["RabbitMQ:UserName"] ?? "guest";
            var password = _configuration["RabbitMQ:Password"] ?? "guest";
            var exchangeName = _configuration["RabbitMQ:ExchangeName"] ?? "cardealer.events";

            _logger.LogInformation(
                "Initializing RabbitMQ connection: Host={Host}, Port={Port}, Exchange={Exchange}",
                hostName, port, exchangeName);

            var factory = new ConnectionFactory
            {
                HostName = hostName,
                Port = port,
                UserName = userName,
                Password = password,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declarar el exchange (debe ser el mismo que usa ErrorService)
            _channel.ExchangeDeclare(
                exchange: exchangeName,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false);

            // Declarar queue específico para ErrorCriticalEvent
            var queueName = "notification.error.critical";
            _channel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            // Bind queue al exchange con routing key "error.critical"
            _channel.QueueBind(
                queue: queueName,
                exchange: exchangeName,
                routingKey: "error.critical");

            // Configurar QoS para procesar 1 mensaje a la vez
            _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            _logger.LogInformation(
                "RabbitMQ initialized successfully. Queue={Queue}, RoutingKey={RoutingKey}",
                queueName, "error.critical");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize RabbitMQ connection");
            throw;
        }
    }

    public override void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();

        _logger.LogInformation("ErrorCriticalEventConsumer disposed");

        base.Dispose();
    }
}
