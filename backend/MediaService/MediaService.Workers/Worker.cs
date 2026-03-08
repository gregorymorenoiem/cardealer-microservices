using MediaService.Workers.Handlers;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace MediaService.Workers;

/// <summary>
/// Background worker that consumes RabbitMQ 'media.process' queue
/// and delegates to ImageProcessingHandler for async variant generation.
/// </summary>
public class ImageProcessingWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ImageProcessingWorker> _logger;
    private readonly IConfiguration _configuration;
    private IConnection? _connection;
    private IModel? _channel;

    private const string QueueName = "media.process";
    private const string ExchangeName = "media.commands";
    private const string RoutingKey = "media.process";
    private const string DlxExchange = "media.commands.dlx";
    private const string DlqName = "media.process.dlq";

    public ImageProcessingWorker(
        IServiceProvider serviceProvider,
        ILogger<ImageProcessingWorker> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ImageProcessingWorker starting — listening on queue '{Queue}'", QueueName);

        try
        {
            SetupRabbitMq();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to RabbitMQ. ImageProcessingWorker will not process messages.");
            // Wait and retry in case RabbitMQ is not ready yet
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            return;
        }

        if (_channel == null) return;

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (_, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            _logger.LogDebug("Received message from queue '{Queue}': {Message}", QueueName, message);

            try
            {
                var command = JsonSerializer.Deserialize<ProcessMediaMessage>(message, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (command?.MediaId is not null)
                {
                    using var scope = _serviceProvider.CreateScope();
                    var handler = scope.ServiceProvider.GetRequiredService<ImageProcessingHandler>();
                    await handler.HandleAsync(command.MediaId, stoppingToken);
                }

                _channel.BasicAck(ea.DeliveryTag, multiple: false);
                _logger.LogInformation("Successfully processed media {MediaId}", command?.MediaId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message from queue '{Queue}'", QueueName);
                // Nack with requeue=false (will go to DLQ via DLX)
                _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
            }
        };

        _channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);

        // Keep alive until cancellation
        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("ImageProcessingWorker stopping");
        }
    }

    private void SetupRabbitMq()
    {
        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMQ:HostName"] ?? "localhost",
            Port = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672"),
            UserName = _configuration["RabbitMQ:UserName"] ?? "guest",
            Password = _configuration["RabbitMQ:Password"] ?? "guest",
            DispatchConsumersAsync = true
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // Declare DLX exchange and DLQ
        _channel.ExchangeDeclare(DlxExchange, ExchangeType.Direct, durable: true);
        _channel.QueueDeclare(DlqName, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(DlqName, DlxExchange, RoutingKey);

        // Declare main exchange and queue with DLX
        _channel.ExchangeDeclare(ExchangeName, ExchangeType.Direct, durable: true);
        _channel.QueueDeclare(QueueName, durable: true, exclusive: false, autoDelete: false,
            arguments: new Dictionary<string, object>
            {
                { "x-dead-letter-exchange", DlxExchange },
                { "x-dead-letter-routing-key", RoutingKey }
            });
        _channel.QueueBind(QueueName, ExchangeName, RoutingKey);

        // Prefetch 1 — process one image at a time (image processing is CPU-intensive)
        _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

        _logger.LogInformation("RabbitMQ connected. Queue: {Queue}, Exchange: {Exchange}", QueueName, ExchangeName);
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

/// <summary>
/// Periodic cleanup worker that runs every 6 hours to clean up
/// stale uploads (>48h) and orphaned media (>7 days).
/// </summary>
public class MediaCleanupWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MediaCleanupWorker> _logger;

    private static readonly TimeSpan CleanupInterval = TimeSpan.FromHours(6);

    public MediaCleanupWorker(
        IServiceProvider serviceProvider,
        ILogger<MediaCleanupWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MediaCleanupWorker starting — cleanup every {Hours}h", CleanupInterval.TotalHours);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<MediaCleanupHandler>();
                await handler.HandleAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during media cleanup cycle");
            }

            try
            {
                await Task.Delay(CleanupInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("MediaCleanupWorker stopping");
            }
        }
    }
}

/// <summary>DTO for deserializing RabbitMQ process media messages</summary>
public record ProcessMediaMessage(string? MediaId, string? ProcessingType = null);
