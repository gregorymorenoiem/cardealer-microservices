using System.Text;
using System.Text.Json;
using CarDealer.Shared.ErrorHandling.Interfaces;
using CarDealer.Shared.ErrorHandling.Models;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace CarDealer.Shared.ErrorHandling.Services;

/// <summary>
/// Publishes errors to ErrorService via RabbitMQ
/// </summary>
public class RabbitMQErrorPublisher : IErrorPublisher, IDisposable
{
    private readonly ErrorHandlingOptions _options;
    private readonly ILogger<RabbitMQErrorPublisher> _logger;
    private IConnection? _connection;
    private IModel? _channel;
    private readonly object _lock = new();
    private bool _disposed;
    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public RabbitMQErrorPublisher(
        IOptions<ErrorHandlingOptions> options,
        ILogger<RabbitMQErrorPublisher> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public Task PublishAsync(ErrorEvent errorEvent, CancellationToken cancellationToken = default)
    {
        if (!_options.RabbitMQ.Enabled)
        {
            _logger.LogDebug("RabbitMQ error publishing is disabled");
            return Task.CompletedTask;
        }

        try
        {
            PublishToRabbitMQ(errorEvent);
        }
        catch (Exception ex)
        {
            // Log but don't throw - we don't want error publishing to break the app
            _logger.LogWarning(ex, "Failed to publish error to RabbitMQ: {Message}", ex.Message);
        }

        return Task.CompletedTask;
    }

    public async Task PublishExceptionAsync(
        Exception exception,
        ErrorContext? context = null,
        CancellationToken cancellationToken = default)
    {
        var errorEvent = CreateErrorEvent(exception, context);
        await PublishAsync(errorEvent, cancellationToken);
    }

    private ErrorEvent CreateErrorEvent(Exception exception, ErrorContext? context)
    {
        var category = DetermineCategory(exception);
        var severity = DetermineSeverity(exception);

        return new ErrorEvent
        {
            ServiceName = _options.ServiceName,
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
            Severity = context?.Severity ?? severity,
            Category = context?.Category ?? category,
            ExceptionType = exception.GetType().FullName ?? exception.GetType().Name,
            Message = exception.Message,
            StackTrace = _options.IncludeStackTrace ? exception.StackTrace : null,
            InnerException = exception.InnerException?.Message,
            RequestPath = context?.RequestPath,
            RequestMethod = context?.RequestMethod,
            UserId = context?.UserId,
            CorrelationId = context?.CorrelationId,
            TraceId = context?.TraceId,
            SpanId = context?.SpanId,
            ClientIp = context?.ClientIp,
            UserAgent = context?.UserAgent,
            AdditionalData = context?.AdditionalData
        };
    }

    private static ErrorCategory DetermineCategory(Exception exception)
    {
        return exception switch
        {
            ValidationException => ErrorCategory.Validation,
            UnauthorizedAccessException => ErrorCategory.Authorization,
            KeyNotFoundException => ErrorCategory.NotFound,
            InvalidOperationException => ErrorCategory.Business,
            TimeoutException => ErrorCategory.Timeout,
            HttpRequestException => ErrorCategory.ExternalService,
            _ => ErrorCategory.Unhandled
        };
    }

    private static ErrorSeverity DetermineSeverity(Exception exception)
    {
        return exception switch
        {
            ValidationException => ErrorSeverity.Warning,
            UnauthorizedAccessException => ErrorSeverity.Warning,
            KeyNotFoundException => ErrorSeverity.Warning,
            InvalidOperationException => ErrorSeverity.Error,
            OutOfMemoryException => ErrorSeverity.Fatal,
            StackOverflowException => ErrorSeverity.Fatal,
            _ => ErrorSeverity.Error
        };
    }

    private void PublishToRabbitMQ(ErrorEvent errorEvent)
    {
        EnsureConnection();

        if (_channel == null)
        {
            _logger.LogWarning("RabbitMQ channel is not available");
            return;
        }

        var json = JsonSerializer.Serialize(errorEvent, s_jsonOptions);
        var body = Encoding.UTF8.GetBytes(json);

        var properties = _channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.ContentType = "application/json";
        properties.MessageId = errorEvent.Id.ToString();
        properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        properties.Headers = new Dictionary<string, object>
        {
            ["service"] = _options.ServiceName,
            ["severity"] = errorEvent.Severity.ToString(),
            ["category"] = errorEvent.Category.ToString()
        };

        _channel.BasicPublish(
            exchange: _options.RabbitMQ.Exchange,
            routingKey: _options.RabbitMQ.RoutingKey,
            basicProperties: properties,
            body: body);

        _logger.LogDebug("Published error event {ErrorId} to RabbitMQ", errorEvent.Id);
    }

    private void EnsureConnection()
    {
        if (_connection?.IsOpen == true && _channel?.IsOpen == true)
            return;

        lock (_lock)
        {
            if (_connection?.IsOpen == true && _channel?.IsOpen == true)
                return;

            try
            {
                _channel?.Dispose();
                _connection?.Dispose();

                var factory = new ConnectionFactory
                {
                    HostName = _options.RabbitMQ.Hostname,
                    Port = _options.RabbitMQ.Port,
                    UserName = _options.RabbitMQ.Username,
                    Password = _options.RabbitMQ.Password,
                    AutomaticRecoveryEnabled = true,
                    NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
                };

                _connection = factory.CreateConnection($"{_options.ServiceName}-ErrorPublisher");
                _channel = _connection.CreateModel();

                // Declare exchange
                _channel.ExchangeDeclare(
                    exchange: _options.RabbitMQ.Exchange,
                    type: ExchangeType.Topic,
                    durable: true,
                    autoDelete: false);

                // Declare queue with DLX arguments to match ErrorService's queue declaration.
                // ErrorService owns the queue and declares it with x-dead-letter-exchange.
                // All publishers MUST use the same arguments to avoid PRECONDITION_FAILED.
                var dlxExchange = $"{_options.RabbitMQ.Exchange}.dlx";
                var queueArgs = new Dictionary<string, object>
                {
                    { "x-dead-letter-exchange", dlxExchange },
                    { "x-dead-letter-routing-key", _options.RabbitMQ.RoutingKey }
                };

                // Ensure DLX exchange exists before declaring the queue with it
                _channel.ExchangeDeclare(dlxExchange, ExchangeType.Direct, durable: true, autoDelete: false);

                _channel.QueueDeclare(
                    queue: _options.RabbitMQ.Queue,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: queueArgs);

                // Bind queue to exchange
                _channel.QueueBind(
                    queue: _options.RabbitMQ.Queue,
                    exchange: _options.RabbitMQ.Exchange,
                    routingKey: _options.RabbitMQ.RoutingKey);

                _logger.LogInformation("Connected to RabbitMQ for error publishing");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to connect to RabbitMQ: {Message}", ex.Message);
            }
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        _channel?.Dispose();
        _connection?.Dispose();
        _disposed = true;
    }
}
