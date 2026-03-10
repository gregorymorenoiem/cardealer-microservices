using System.Text;
using System.Text.Json;
using CarDealer.Contracts.Events.Auth;
using ContactService.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ContactService.Infrastructure.Messaging;

// ═══════════════════════════════════════════════════════════════════════════════
// USER DATA DELETION CONSUMER — LEY 172-13 CASCADE DELETION
//
// Consumes UserDeletedEvent from UserService (AccountDeletionWorker) via RabbitMQ.
// ANONYMIZES (not deletes) contact data to preserve seller history:
//   • Contact requests: PII fields → "[SUPRIMIDO]"
//   • Contact messages: buyer messages → "[MENSAJE SUPRIMIDO — Ley 172-13]"
//
// Anonymization is preferred over hard deletion because:
//   1. Sellers need to keep their inquiry history for business continuity
//   2. Vehicle listing analytics depend on inquiry counts
//   3. Ley 172-13 Art. 5 only requires PII removal, not record destruction
//
// Queue: contact.user.deletion
// Exchange: cardealer.events (topic)
// Routing key: auth.user.deleted
// DLQ: contact.user.deletion.dlq
// ═══════════════════════════════════════════════════════════════════════════════

public sealed class UserDataDeletionConsumer : BackgroundService
{
    private readonly ILogger<UserDataDeletionConsumer> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private IConnection? _connection;
    private IModel? _channel;

    private const string QueueName = "contact.user.deletion";
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

    private async Task ProcessUserDeletionAsync(UserDeletedEvent deletionEvent, CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();

        var contactRequestRepo = scope.ServiceProvider.GetRequiredService<IContactRequestRepository>();
        var contactMessageRepo = scope.ServiceProvider.GetRequiredService<IContactMessageRepository>();

        var userId = deletionEvent.UserId;

        // Step 1: Anonymize buyer messages (replace content with suppression notice)
        var messagesAnonymized = await contactMessageRepo.AnonymizeByUserIdAsync(userId, ct);

        // Step 2: Anonymize contact requests (replace PII fields)
        var requestsAnonymized = await contactRequestRepo.AnonymizeByBuyerIdAsync(userId, ct);

        _logger.LogInformation(
            "[UserDeletion] Completed Ley 172-13 anonymization for user {UserId}: " +
            "{Requests} contact requests, {Messages} messages anonymized",
            userId, requestsAnonymized, messagesAnonymized);
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

            _connection = factory.CreateConnection($"ContactService-UserDeletion-{Environment.MachineName}");
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
