using System.Text;
using System.Text.Json;
using CarDealer.Contracts.Events.Auth;
using ChatbotService.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ChatbotService.Infrastructure.Messaging;

// ═══════════════════════════════════════════════════════════════════════════════
// USER DATA DELETION CONSUMER — LEY 172-13 CASCADE DELETION
//
// Consumes UserDeletedEvent from UserService (AccountDeletionWorker) via RabbitMQ.
// Deletes ALL user data in ChatbotService:
//   • Chat sessions (and messages via cascade)
//   • Chat leads (generated from conversations)
//   • Interaction usage records
//
// This ensures compliance with Dominican Republic Ley 172-13 (Protección de
// Datos Personales) — Art. 5 Derecho de Supresión.
//
// Queue: chatbot.user.deletion
// Exchange: cardealer.events (topic)
// Routing key: auth.user.deleted
// DLQ: chatbot.user.deletion.dlq
// ═══════════════════════════════════════════════════════════════════════════════

public sealed class UserDataDeletionConsumer : BackgroundService
{
    private readonly ILogger<UserDataDeletionConsumer> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private IConnection? _connection;
    private IModel? _channel;

    private const string QueueName = "chatbot.user.deletion";
    private const string RoutingKey = "auth.user.deleted";

    public UserDataDeletionConsumer(
        ILogger<UserDataDeletionConsumer> logger,
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var rabbitEnabled = _configuration.GetValue<bool>("RabbitMQ:Enabled", false);
        if (!rabbitEnabled)
        {
            _logger.LogInformation("[UserDeletion] RabbitMQ disabled, consumer inactive");
            return;
        }

        InitializeRabbitMQ();

        if (_channel == null)
        {
            _logger.LogWarning("[UserDeletion] Failed to initialize RabbitMQ, consumer inactive");
            return;
        }

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (_, ea) =>
        {
            try
            {
                var body = Encoding.UTF8.GetString(ea.Body.ToArray());
                var deletionEvent = JsonSerializer.Deserialize<UserDeletedEvent>(body,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (deletionEvent == null || deletionEvent.UserId == Guid.Empty)
                {
                    _logger.LogWarning("[UserDeletion] Failed to deserialize event or empty UserId, sending to DLQ");
                    _channel.BasicNack(ea.DeliveryTag, false, false);
                    return;
                }

                _logger.LogInformation(
                    "[UserDeletion] Received user deletion event: UserId={UserId}, Reason={Reason}",
                    deletionEvent.UserId, deletionEvent.Reason);

                await ProcessUserDeletionAsync(deletionEvent, stoppingToken);

                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UserDeletion] Error processing message, sending to DLQ");
                _channel.BasicNack(ea.DeliveryTag, false, false);
            }
        };

        _channel.BasicConsume(
            queue: QueueName,
            autoAck: false,
            consumer: consumer);

        _logger.LogInformation("[UserDeletion] Listening on queue '{Queue}' for user deletion events", QueueName);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    /// <summary>
    /// Deletes all ChatbotService data for a user:
    /// 1. Messages (by session IDs — explicit before session cascade)
    /// 2. Leads (by session IDs)
    /// 3. Sessions (by user ID)
    /// 4. Interaction usage (by user ID)
    /// </summary>
    private async Task ProcessUserDeletionAsync(UserDeletedEvent deletionEvent, CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();

        var sessionRepo = scope.ServiceProvider.GetRequiredService<IChatSessionRepository>();
        var messageRepo = scope.ServiceProvider.GetRequiredService<IChatMessageRepository>();
        var leadRepo = scope.ServiceProvider.GetRequiredService<IChatLeadRepository>();
        var usageRepo = scope.ServiceProvider.GetRequiredService<IInteractionUsageRepository>();

        var userId = deletionEvent.UserId;

        // Step 1: Get session IDs BEFORE deletion (needed for child cleanup)
        var sessions = await sessionRepo.GetByUserIdAsync(userId, ct);
        var userSessionIds = sessions.Select(s => s.Id).ToList();

        // Step 2: Delete messages (explicit — before session cascade)
        var messagesDeleted = 0;
        if (userSessionIds.Count > 0)
        {
            messagesDeleted = await messageRepo.DeleteBySessionIdsAsync(userSessionIds, ct);
        }

        // Step 3: Delete leads
        var leadsDeleted = 0;
        if (userSessionIds.Count > 0)
        {
            leadsDeleted = await leadRepo.DeleteBySessionIdsAsync(userSessionIds, ct);
        }

        // Step 4: Delete sessions
        var deletedSessionIds = await sessionRepo.DeleteAllByUserIdAsync(userId, ct);

        // Step 5: Delete interaction usage
        var usageDeleted = await usageRepo.DeleteByUserIdAsync(userId, ct);

        _logger.LogInformation(
            "[UserDeletion] Completed Ley 172-13 cascade for user {UserId}: " +
            "{Sessions} sessions, {Messages} messages, {Leads} leads, {Usage} usage records deleted",
            userId, deletedSessionIds.Count, messagesDeleted, leadsDeleted, usageDeleted);
    }

    private void InitializeRabbitMQ()
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMQ:Host"] ?? "localhost",
                Port = int.TryParse(_configuration["RabbitMQ:Port"], out var port) ? port : 5672,
                UserName = _configuration["RabbitMQ:Username"] ?? "guest",
                Password = _configuration["RabbitMQ:Password"] ?? "guest",
                VirtualHost = _configuration["RabbitMQ:VirtualHost"] ?? "/",
                DispatchConsumersAsync = true,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
            };

            var exchangeName = _configuration["RabbitMQ:ExchangeName"] ?? "cardealer.events";
            var dlxExchange = $"{exchangeName}.dlx";
            var dlqQueue = $"{QueueName}.dlq";

            _connection = factory.CreateConnection($"ChatbotService-UserDeletion-{Environment.MachineName}");
            _channel = _connection.CreateModel();

            // Declare main exchange
            _channel.ExchangeDeclare(exchangeName, ExchangeType.Topic, durable: true, autoDelete: false);

            // Declare DLX + DLQ
            _channel.ExchangeDeclare(dlxExchange, ExchangeType.Direct, durable: true, autoDelete: false);
            _channel.QueueDeclare(dlqQueue, durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind(dlqQueue, dlxExchange, RoutingKey);

            // Declare main queue with DLX args
            var args = new Dictionary<string, object>
            {
                ["x-dead-letter-exchange"] = dlxExchange,
                ["x-dead-letter-routing-key"] = RoutingKey,
            };
            _channel.QueueDeclare(QueueName, durable: true, exclusive: false, autoDelete: false, arguments: args);
            _channel.QueueBind(QueueName, exchangeName, RoutingKey);

            // Prefetch 1 message at a time — deletion is heavy, no parallel processing
            _channel.BasicQos(0, 1, false);

            _logger.LogInformation(
                "[UserDeletion] RabbitMQ initialized — Queue={Queue}, Exchange={Exchange}, RoutingKey={Key}",
                QueueName, exchangeName, RoutingKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[UserDeletion] Failed to initialize RabbitMQ: {Error}", ex.Message);
            _channel = null;
            _connection = null;
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
