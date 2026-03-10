using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using VehiclesSaleService.Infrastructure.Persistence;

namespace VehiclesSaleService.Infrastructure.Messaging;

/// <summary>
/// Consumes review events from RabbitMQ to keep SellerRating and SellerReviewCount
/// synchronized on Vehicle entities. This is critical for the OKLA Platform Score
/// D4 (Seller Reputation) to reflect real-time review data.
///
/// Routing keys consumed:
///   reviews.review.created  → update seller rating on all their vehicles
///   reviews.review.updated  → recalculate seller rating
///
/// SWITCHING COST: By keeping review data synced to vehicles, the buyer sees
/// real-time reputation data that is exclusive to OKLA.
/// </summary>
public class ReviewEventsConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ReviewEventsConsumer> _logger;
    private IConnection? _connection;
    private IModel? _channel;

    private const string ExchangeName = "cardealer.events";
    private const string QueueName = "vehiclessaleservice.review-sync";

    private static readonly string[] RoutingKeys =
    {
        "reviews.review.created",
        "reviews.review.updated"
    };

    public ReviewEventsConsumer(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<ReviewEventsConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var rabbitEnabled = _configuration.GetValue<bool>("RabbitMQ:Enabled");
        if (!rabbitEnabled)
        {
            _logger.LogInformation("ReviewEventsConsumer disabled — RabbitMQ not enabled");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ConnectAndConsumeAsync(stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogWarning(ex, "ReviewEventsConsumer connection lost. Reconnecting in 10s...");
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }

    private async Task ConnectAndConsumeAsync(CancellationToken stoppingToken)
    {
        var host = _configuration.GetValue<string>("RabbitMQ:Host") ?? "localhost";
        var port = _configuration.GetValue<int>("RabbitMQ:Port", 5672);
        var username = _configuration.GetValue<string>("RabbitMQ:Username") ?? "guest";
        var password = _configuration.GetValue<string>("RabbitMQ:Password") ?? "guest";

        var factory = new ConnectionFactory
        {
            HostName = host,
            Port = port,
            UserName = username,
            Password = password,
            DispatchConsumersAsync = true,
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
        };

        _connection = factory.CreateConnection("VehiclesSaleService.ReviewSync");
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(ExchangeName, ExchangeType.Topic, durable: true);
        _channel.QueueDeclare(QueueName, durable: true, exclusive: false, autoDelete: false);

        foreach (var key in RoutingKeys)
        {
            _channel.QueueBind(QueueName, ExchangeName, key);
        }

        _channel.BasicQos(prefetchSize: 0, prefetchCount: 10, global: false);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (_, ea) =>
        {
            try
            {
                var body = Encoding.UTF8.GetString(ea.Body.ToArray());
                var routingKey = ea.RoutingKey;

                _logger.LogDebug("ReviewEventsConsumer received: {RoutingKey}", routingKey);

                await ProcessReviewEventAsync(routingKey, body);

                _channel.BasicAck(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing review event: {RoutingKey}", ea.RoutingKey);
                _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
            }
        };

        _channel.BasicConsume(QueueName, autoAck: false, consumer: consumer);

        _logger.LogInformation("✅ ReviewEventsConsumer started — listening for review events");

        // Wait until cancellation
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task ProcessReviewEventAsync(string routingKey, string body)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var json = JsonDocument.Parse(body);
        var root = json.RootElement;

        if (!root.TryGetProperty("SellerId", out var sellerIdProp) &&
            !root.TryGetProperty("sellerId", out sellerIdProp))
        {
            _logger.LogWarning("Review event missing SellerId property");
            return;
        }

        if (!Guid.TryParse(sellerIdProp.GetString(), out var sellerId))
        {
            _logger.LogWarning("Review event has invalid SellerId");
            return;
        }

        // Extract the new aggregated values from the event
        decimal newAverageRating = 0;
        int newTotalReviews = 0;

        if (root.TryGetProperty("NewAverageRating", out var ratingProp) ||
            root.TryGetProperty("newAverageRating", out ratingProp))
        {
            newAverageRating = ratingProp.GetDecimal();
        }

        if (root.TryGetProperty("NewTotalReviews", out var reviewsProp) ||
            root.TryGetProperty("newTotalReviews", out reviewsProp))
        {
            newTotalReviews = reviewsProp.GetInt32();
        }

        // Update all vehicles belonging to this seller
        var vehiclesUpdated = await context.Vehicles
            .Where(v => v.SellerId == sellerId && !v.IsDeleted)
            .ExecuteUpdateAsync(setter => setter
                .SetProperty(v => v.SellerRating, newAverageRating)
                .SetProperty(v => v.SellerReviewCount, newTotalReviews)
                .SetProperty(v => v.UpdatedAt, DateTime.UtcNow));

        _logger.LogInformation(
            "OKLA Platform Score: Synced seller {SellerId} rating → {Rating:F1}★ ({Reviews} reviews) on {Count} vehicles",
            sellerId, newAverageRating, newTotalReviews, vehiclesUpdated);
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
