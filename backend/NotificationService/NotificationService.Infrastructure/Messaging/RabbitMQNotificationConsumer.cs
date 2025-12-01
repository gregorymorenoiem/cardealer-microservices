using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotificationService.Application.DTOs;
using NotificationService.Application.UseCases.SendEmailNotification;
using NotificationService.Application.UseCases.SendSmsNotification;
using NotificationService.Domain.Entities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace NotificationService.Infrastructure.Messaging;

// TODO: Replace AuthService.Shared.NotificationMessages events with CarDealer.Contracts events
// Temporary DTOs for deserialization (should be in CarDealer.Contracts)
public record EmailNotificationEvent(string To, string Subject, string Body, bool IsHtml, Dictionary<string, string>? Data);
public record SmsNotificationEvent(string To, string Body, Dictionary<string, string>? Data);
public record NotificationEvent(string Type, Dictionary<string, string>? Data);

public class RabbitMQNotificationConsumer : BackgroundService
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RabbitMQNotificationConsumer> _logger;
    private readonly NotificationServiceRabbitMQSettings _settings;
    private readonly JsonSerializerOptions _jsonOptions;

    public RabbitMQNotificationConsumer(
        IServiceProvider serviceProvider,
        IOptions<RabbitMQSettings> rabbitMqSettings,
        IOptions<NotificationServiceRabbitMQSettings> notificationSettings,
        ILogger<RabbitMQNotificationConsumer> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _settings = notificationSettings.Value;

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

            // Configurar exchange y colas (deben coincidir con AuthService)
            SetupRabbitMQ();

            _logger.LogInformation("✅ RabbitMQ Notification Consumer initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to initialize RabbitMQ Notification Consumer");
            throw;
        }
    }

    private void SetupRabbitMQ()
    {
        // Exchange principal
        _channel.ExchangeDeclare(
            exchange: _settings.ExchangeName,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false);

        // Cola general
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

        // Cola específica para emails
        _channel.QueueDeclare(
            queue: _settings.EmailQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        _channel.QueueBind(
            queue: _settings.EmailQueueName,
            exchange: _settings.ExchangeName,
            routingKey: "notification.email");

        // Cola específica para SMS
        _channel.QueueDeclare(
            queue: _settings.SmsQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        _channel.QueueBind(
            queue: _settings.SmsQueueName,
            exchange: _settings.ExchangeName,
            routingKey: "notification.sms");

        // Configurar QoS
        _channel.BasicQos(0, (ushort)_settings.PrefetchCount, _settings.GlobalQos);

        _logger.LogInformation("RabbitMQ setup completed for NotificationService");
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.Register(() =>
        {
            _logger.LogInformation("🛑 RabbitMQ Notification Consumer is stopping");
            _channel?.Close();
            _connection?.Close();
        });

        // Iniciar consumo de todas las colas
        StartConsuming(_settings.QueueName, "general");
        StartConsuming(_settings.EmailQueueName, "email");
        StartConsuming(_settings.SmsQueueName, "sms");

        _logger.LogInformation("RabbitMQ Notification Consumer started successfully");
        return Task.CompletedTask;
    }

    private void StartConsuming(string queueName, string queueType)
    {
        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            try
            {
                _logger.LogInformation("Received message from {QueueName} ({Type})", queueName, queueType);
                await ProcessMessage(queueType, message);
                _channel.BasicAck(ea.DeliveryTag, false);
                _logger.LogInformation("Successfully processed message from {QueueName}", queueName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message from queue {QueueName}", queueName);
                _channel.BasicNack(ea.DeliveryTag, false, false);
            }
        };

        _channel.BasicConsume(
            queue: queueName,
            autoAck: false,
            consumer: consumer);

        _logger.LogInformation("Started consuming from queue: {QueueName} ({Type})", queueName, queueType);
    }

    private async Task ProcessMessage(string queueType, string message)
    {
        using var scope = _serviceProvider.CreateScope();

        try
        {
            switch (queueType)
            {
                case "email":
                    await ProcessEmailMessage(message, scope);
                    break;
                case "sms":
                    await ProcessSmsMessage(message, scope);
                    break;
                case "general":
                    await ProcessGeneralMessage(message, scope);
                    break;
                default:
                    _logger.LogWarning("Unknown queue type: {QueueType}", queueType);
                    break;
            }
        }
        catch (JsonException jsonEx)
        {
            _logger.LogError(jsonEx, "JSON deserialization error for {QueueType} message", queueType);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing {QueueType} message", queueType);
            throw;
        }
    }

    private async Task ProcessEmailMessage(string message, IServiceScope scope)
    {
        var emailEvent = JsonSerializer.Deserialize<EmailNotificationEvent>(message, _jsonOptions);
        if (emailEvent == null)
        {
            _logger.LogWarning("Failed to deserialize email message");
            return;
        }

        // Usar los comandos EXISTENTES de tu aplicación
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        // Crear el request usando tu DTO existente
        var request = new SendEmailNotificationRequest(
            To: emailEvent.To,
            Subject: emailEvent.Subject,
            Body: emailEvent.Body,
            IsHtml: emailEvent.IsHtml,
            Metadata: emailEvent.Data?.ToDictionary(k => k.Key, k => (object)k.Value)
        );

        // Usar el comando EXISTENTE de tu aplicación
        var command = new SendEmailNotificationCommand(request);
        var result = await mediator.Send(command);

        if (result.NotificationId != Guid.Empty)
        {
            _logger.LogInformation("Successfully processed email notification {NotificationId} for {To}",
                result.NotificationId, emailEvent.To);
        }
        else
        {
            _logger.LogWarning("Failed to process email notification for {To}", emailEvent.To);
        }
    }

    private async Task ProcessSmsMessage(string message, IServiceScope scope)
    {
        var smsEvent = JsonSerializer.Deserialize<SmsNotificationEvent>(message, _jsonOptions);
        if (smsEvent == null)
        {
            _logger.LogWarning("Failed to deserialize SMS message");
            return;
        }

        // Usar los comandos EXISTENTES de tu aplicación
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        // Crear el request usando tu DTO existente
        var request = new SendSmsNotificationRequest(
            To: smsEvent.To,
            Message: smsEvent.Body,
            Metadata: smsEvent.Data?.ToDictionary(k => k.Key, k => (object)k.Value)
        );

        // Usar el comando EXISTENTE de tu aplicación
        var command = new SendSmsNotificationCommand(request);
        var result = await mediator.Send(command);

        if (result.NotificationId != Guid.Empty)
        {
            _logger.LogInformation("Successfully processed SMS notification {NotificationId} for {To}",
                result.NotificationId, smsEvent.To);
        }
        else
        {
            _logger.LogWarning("Failed to process SMS notification for {To}", smsEvent.To);
        }
    }

    private async Task ProcessGeneralMessage(string message, IServiceScope scope)
    {
        var notificationEvent = JsonSerializer.Deserialize<NotificationEvent>(message, _jsonOptions);
        if (notificationEvent == null)
        {
            _logger.LogWarning("Failed to deserialize general notification message");
            return;
        }

        // Determinar el tipo basado en el campo Type
        switch (notificationEvent.Type.ToLower())
        {
            case "email":
                await ProcessEmailMessage(message, scope);
                break;
            case "sms":
                await ProcessSmsMessage(message, scope);
                break;
            default:
                _logger.LogWarning("Unsupported notification type in general queue: {Type}", notificationEvent.Type);
                break;
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