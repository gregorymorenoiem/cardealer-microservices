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
// INFRASTRUCTURE COST ALERT CONSUMER — CONTRA #8 FIX
//
// Consumes InfrastructureCostAlertEvent from AdminService via RabbitMQ.
// Routes to IAdminAlertService which sends multi-channel notifications:
//   Warning → Email + Slack (platform team)
//   Critical/Emergency → Email + SMS + Teams + Slack (CTO)
//
// Queue: notification.infra.cost
// Exchange: cardealer.events (topic)
// Routing key: alert.infra.cost_threshold_breached
// DLQ: notification.infra.cost.dlq
// ═══════════════════════════════════════════════════════════════════════════════

public sealed class InfrastructureCostAlertConsumer : BackgroundService
{
    private readonly ILogger<InfrastructureCostAlertConsumer> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private IConnection? _connection;
    private IModel? _channel;

    private const string QueueName = "notification.infra.cost";
    private const string RoutingKey = "alert.infra.cost_threshold_breached";

    public InfrastructureCostAlertConsumer(
        ILogger<InfrastructureCostAlertConsumer> logger,
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
            _logger.LogInformation("[InfraCostConsumer] RabbitMQ disabled, consumer inactive");
            return;
        }

        InitializeRabbitMQ();

        if (_channel == null)
        {
            _logger.LogWarning("[InfraCostConsumer] Failed to initialize RabbitMQ, consumer inactive");
            return;
        }

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (_, ea) =>
        {
            try
            {
                var body = Encoding.UTF8.GetString(ea.Body.ToArray());
                var alertEvent = JsonSerializer.Deserialize<InfrastructureCostAlertEvent>(body,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (alertEvent == null)
                {
                    _logger.LogWarning("[InfraCostConsumer] Failed to deserialize event, sending to DLQ");
                    _channel.BasicNack(ea.DeliveryTag, false, false);
                    return;
                }

                _logger.LogInformation(
                    "[InfraCostConsumer] Received infra cost alert: Period={Period}, " +
                    "Projected=${Projected:F2}, Budget=${Budget:F2}, Severity={Severity}",
                    alertEvent.Period, alertEvent.ProjectedMonthlyCost,
                    alertEvent.BudgetCeiling, alertEvent.Severity);

                await ProcessAlertAsync(alertEvent, stoppingToken);

                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[InfraCostConsumer] Error processing message, sending to DLQ");
                _channel.BasicNack(ea.DeliveryTag, false, false);
            }
        };

        _channel.BasicConsume(
            queue: QueueName,
            autoAck: false,
            consumer: consumer);

        _logger.LogInformation("[InfraCostConsumer] Listening on queue '{Queue}'", QueueName);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task ProcessAlertAsync(InfrastructureCostAlertEvent alertEvent, CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();

        var adminAlertService = scope.ServiceProvider
            .GetService<NotificationService.Domain.Interfaces.IAdminAlertService>();

        if (adminAlertService == null)
        {
            _logger.LogWarning("[InfraCostConsumer] IAdminAlertService not registered, skipping alert");
            return;
        }

        var title = alertEvent.Severity switch
        {
            "Emergency" => $"🚨 EMERGENCY: Cloud cost ${alertEvent.ProjectedMonthlyCost:F0} exceeds ${alertEvent.BudgetCeiling:F0} budget!",
            "Critical" => $"🔴 CRITICAL: Cloud cost ${alertEvent.ProjectedMonthlyCost:F0} at {alertEvent.BudgetUtilization:P0} of ${alertEvent.BudgetCeiling:F0} budget",
            _ => $"⚠️ Cloud cost alert: ${alertEvent.ProjectedMonthlyCost:F0} at {alertEvent.BudgetUtilization:P0} of ${alertEvent.BudgetCeiling:F0} budget",
        };

        var message = BuildAlertMessage(alertEvent);

        var metadata = new Dictionary<string, string>
        {
            ["period"] = alertEvent.Period,
            ["projected_cost"] = alertEvent.ProjectedMonthlyCost.ToString("F2"),
            ["budget_ceiling"] = alertEvent.BudgetCeiling.ToString("F2"),
            ["budget_utilization"] = alertEvent.BudgetUtilization.ToString("P1"),
            ["overage"] = alertEvent.Overage.ToString("F2"),
            ["days_remaining"] = alertEvent.DaysRemaining.ToString(),
            ["runbook_url"] = alertEvent.RunbookUrl,
        };

        await adminAlertService.SendAlertAsync(
            alertType: "infrastructure_cost",
            title: title,
            message: message,
            severity: alertEvent.Severity,
            metadata: metadata,
            ct: ct);

        _logger.LogInformation("[InfraCostConsumer] Alert sent via AdminAlertService for {Period}", alertEvent.Period);
    }

    private static string BuildAlertMessage(InfrastructureCostAlertEvent e)
    {
        var breakdownLines = string.Join("\n",
            e.CostBreakdown.Select(kv => $"  • {kv.Key}: ${kv.Value:F2}"));

        var actionLines = string.Join("\n",
            e.RecommendedActions.Select(a => $"  ✅ {a}"));

        return $"""
            🏗️ Infrastructure Cost Alert — {e.Period}

            Budget Status:
            • Current Spend: ${e.CurrentSpend:F2}
            • Projected Monthly Cost: ${e.ProjectedMonthlyCost:F2}
            • Budget Ceiling: ${e.BudgetCeiling:F2}
            • Budget Utilization: {e.BudgetUtilization:P1}
            • Overage: ${e.Overage:F2}

            Timeline:
            • Day {e.DaysElapsed} of month, {e.DaysRemaining} days remaining
            • Daily cost rate: ${e.DailyCostRate:F2}/day

            Cost Breakdown:
            {breakdownLines}

            Recommended Actions:
            {actionLines}

            📖 Runbook: {e.RunbookUrl}
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

            _connection = factory.CreateConnection($"NotificationService-InfraCostConsumer-{Environment.MachineName}");
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
                "[InfraCostConsumer] RabbitMQ initialized — Queue={Queue}, Exchange={Exchange}, RoutingKey={Key}",
                QueueName, exchangeName, RoutingKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[InfraCostConsumer] Failed to initialize RabbitMQ: {Error}", ex.Message);
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
