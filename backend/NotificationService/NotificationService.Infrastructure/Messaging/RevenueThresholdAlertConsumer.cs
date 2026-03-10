using System.Text;
using System.Text.Json;
using CarDealer.Contracts.Events.Alert;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NotificationService.Infrastructure.Messaging;

// ═══════════════════════════════════════════════════════════════════════════════
// REVENUE THRESHOLD ALERT CONSUMER — CONTRA #7 FIX
//
// Consumes RevenueThresholdAlertEvent from AdminService via RabbitMQ.
// Routes to IAdminAlertService which sends multi-channel notifications:
//   Email + SMS + Teams + Slack → Founder
//
// Queue: notification.revenue.threshold
// Exchange: cardealer.events (topic)
// Routing key: alert.revenue.threshold_breached
// DLQ: notification.revenue.threshold.dlq
// ═══════════════════════════════════════════════════════════════════════════════

public sealed class RevenueThresholdAlertConsumer : BackgroundService
{
    private readonly ILogger<RevenueThresholdAlertConsumer> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private IConnection? _connection;
    private IModel? _channel;

    private const string QueueName = "notification.revenue.threshold";
    private const string RoutingKey = "alert.revenue.threshold_breached";

    public RevenueThresholdAlertConsumer(
        ILogger<RevenueThresholdAlertConsumer> logger,
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
            _logger.LogInformation("[RevenueAlertConsumer] RabbitMQ disabled, consumer inactive");
            return;
        }

        InitializeRabbitMQ();

        if (_channel == null)
        {
            _logger.LogWarning("[RevenueAlertConsumer] Failed to initialize RabbitMQ, consumer inactive");
            return;
        }

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (_, ea) =>
        {
            try
            {
                var body = Encoding.UTF8.GetString(ea.Body.ToArray());
                var alertEvent = JsonSerializer.Deserialize<RevenueThresholdAlertEvent>(body,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (alertEvent == null)
                {
                    _logger.LogWarning("[RevenueAlertConsumer] Failed to deserialize event, sending to DLQ");
                    _channel.BasicNack(ea.DeliveryTag, false, false);
                    return;
                }

                _logger.LogInformation(
                    "[RevenueAlertConsumer] Received revenue alert: Period={Period}, " +
                    "Projected=${Projected:F2}, OPEX=${Opex:F2}, Shortfall=${Shortfall:F2}, Severity={Severity}",
                    alertEvent.Period, alertEvent.ProjectedMonthlyRevenue,
                    alertEvent.OpexThreshold, alertEvent.Shortfall, alertEvent.Severity);

                await ProcessAlertAsync(alertEvent, stoppingToken);

                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RevenueAlertConsumer] Error processing message, sending to DLQ");
                _channel.BasicNack(ea.DeliveryTag, false, false);
            }
        };

        _channel.BasicConsume(
            queue: QueueName,
            autoAck: false,
            consumer: consumer);

        _logger.LogInformation("[RevenueAlertConsumer] Listening on queue '{Queue}'", QueueName);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task ProcessAlertAsync(RevenueThresholdAlertEvent alertEvent, CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();

        // Resolve IAdminAlertService to send multi-channel notification
        var adminAlertService = scope.ServiceProvider
            .GetService<NotificationService.Domain.Interfaces.IAdminAlertService>();

        if (adminAlertService == null)
        {
            _logger.LogWarning("[RevenueAlertConsumer] IAdminAlertService not registered, skipping alert");
            return;
        }

        var title = alertEvent.Severity == "Critical"
            ? $"🚨 CRITICAL: Revenue projected ${alertEvent.ProjectedMonthlyRevenue:F0} — below OPEX ${alertEvent.OpexThreshold:F0}"
            : $"⚠️ Revenue alert: Projected ${alertEvent.ProjectedMonthlyRevenue:F0} vs OPEX ${alertEvent.OpexThreshold:F0}";

        var message = BuildAlertMessage(alertEvent);

        var metadata = new Dictionary<string, string>
        {
            ["period"] = alertEvent.Period,
            ["projected_revenue"] = alertEvent.ProjectedMonthlyRevenue.ToString("F2"),
            ["opex_threshold"] = alertEvent.OpexThreshold.ToString("F2"),
            ["shortfall"] = alertEvent.Shortfall.ToString("F2"),
            ["shortfall_percent"] = alertEvent.ShortfallPercent.ToString("P1"),
            ["days_remaining"] = alertEvent.DaysRemaining.ToString(),
            ["mrr"] = alertEvent.CurrentMrr.ToString("F2"),
        };

        await adminAlertService.SendAlertAsync(
            alertType: "revenue_threshold",
            title: title,
            message: message,
            severity: alertEvent.Severity,
            metadata: metadata,
            ct: ct);

        _logger.LogInformation("[RevenueAlertConsumer] Alert sent via AdminAlertService for {Period}", alertEvent.Period);
    }

    private static string BuildAlertMessage(RevenueThresholdAlertEvent e)
    {
        return $"""
            📊 Revenue Threshold Alert — {e.Period}
            
            Current Status:
            • Accumulated Revenue: ${e.AccumulatedRevenue:F2}
            • Projected Monthly Revenue: ${e.ProjectedMonthlyRevenue:F2}
            • Monthly OPEX Threshold: ${e.OpexThreshold:F2}
            • Shortfall: ${e.Shortfall:F2} ({e.ShortfallPercent:P1} below OPEX)
            
            Timeline:
            • Day {e.DaysElapsed} of month, {e.DaysRemaining} days remaining
            • Daily revenue rate: ${e.DailyRevenueRate:F2}/day
            • Required daily revenue: ${e.RequiredDailyRevenue:F2}/day to reach OPEX
            
            Revenue Breakdown:
            • MRR (Subscriptions): ${e.CurrentMrr:F2}
            • Additional (Overage + Ads): ${e.AdditionalRevenue:F2}
            
            {e.SuggestedAction}
            """;
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

            _connection = factory.CreateConnection($"NotificationService-RevenueAlertConsumer-{Environment.MachineName}");
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

            // Prefetch 1 message at a time
            _channel.BasicQos(0, 1, false);

            _logger.LogInformation(
                "[RevenueAlertConsumer] RabbitMQ initialized — Queue={Queue}, Exchange={Exchange}, RoutingKey={Key}",
                QueueName, exchangeName, RoutingKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[RevenueAlertConsumer] Failed to initialize RabbitMQ: {Error}", ex.Message);
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
