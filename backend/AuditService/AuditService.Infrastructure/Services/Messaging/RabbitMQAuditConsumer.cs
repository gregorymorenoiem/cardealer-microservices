using AuditService.Domain.Interfaces.Repositories;
using AuditService.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using AuditService.Shared.AuditMessages;

namespace AuditService.Infrastructure.Services.Messaging;

public class RabbitMQAuditConsumer : BackgroundService
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly AuditServiceRabbitMQSettings _settings;
    private readonly ILogger<RabbitMQAuditConsumer> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly JsonSerializerOptions _jsonOptions;

    public RabbitMQAuditConsumer(
        IOptions<RabbitMQSettings> rabbitMqSettings,
        IOptions<AuditServiceRabbitMQSettings> auditSettings,
        ILogger<RabbitMQAuditConsumer> logger,
        IServiceProvider serviceProvider)
    {
        _settings = auditSettings.Value;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

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

        ConfigureMessagingTopology();
    }

    private void ConfigureMessagingTopology()
    {
        _channel.ExchangeDeclare(
            exchange: _settings.ExchangeName,
            type: ExchangeType.Topic,
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
                await ProcessMessageAsync(message, ea, stoppingToken);
                _channel.BasicAck(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing audit event message");
                _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
            }
        };

        _channel.BasicConsume(
            queue: _settings.QueueName,
            autoAck: false,
            consumer: consumer);

        _logger.LogInformation("RabbitMQ Audit Consumer started listening on queue: {QueueName}", _settings.QueueName);

        return Task.CompletedTask;
    }

    private async Task ProcessMessageAsync(string message, BasicDeliverEventArgs ea, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var auditLogRepository = scope.ServiceProvider.GetRequiredService<IAuditLogRepository>();

        try
        {
            var auditEvent = JsonSerializer.Deserialize<AuditEvent>(message, _jsonOptions);

            if (auditEvent == null)
            {
                _logger.LogWarning("Received null or invalid audit event message");
                return;
            }

            // Crear AuditLog usando los métodos de creación
            AuditLog auditLog;

            if (auditEvent.Success)
            {
                auditLog = AuditLog.CreateSuccess(
                    auditEvent.UserId,
                    auditEvent.Action,
                    auditEvent.Resource,
                    auditEvent.UserIp,
                    auditEvent.UserAgent,
                    auditEvent.AdditionalData,
                    auditEvent.DurationMs,
                    auditEvent.CorrelationId,
                    auditEvent.ServiceName
                );
            }
            else
            {
                auditLog = AuditLog.CreateFailure(
                    auditEvent.UserId,
                    auditEvent.Action,
                    auditEvent.Resource,
                    auditEvent.UserIp,
                    auditEvent.UserAgent,
                    auditEvent.ErrorMessage ?? "Unknown error",
                    auditEvent.AdditionalData,
                    auditEvent.DurationMs,
                    auditEvent.CorrelationId,
                    auditEvent.ServiceName,
                    auditEvent.Severity
                );
            }

            await auditLogRepository.AddAsync(auditLog, cancellationToken);

            _logger.LogInformation(
                "Successfully processed audit event: {Action} for user {UserId}",
                auditEvent.Action, auditEvent.UserId);
        }
        catch (JsonException jsonEx)
        {
            _logger.LogError(jsonEx, "Failed to deserialize audit event message");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process audit event message");
            throw;
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