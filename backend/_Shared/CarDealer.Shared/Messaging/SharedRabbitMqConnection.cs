using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace CarDealer.Shared.Messaging;

/// <summary>
/// Singleton RabbitMQ connection shared across all publishers/consumers within a service.
/// Prevents connection exhaustion when auto-scaling (each pod uses 1 connection instead of N per class).
/// 
/// RabbitMQ best practice: share one IConnection per process, create multiple IModel (channels) as needed.
/// IConnection is thread-safe; IModel is NOT thread-safe.
/// </summary>
public sealed class SharedRabbitMqConnection : IDisposable
{
    private readonly ILogger<SharedRabbitMqConnection> _logger;
    private readonly ConnectionFactory _factory;
    private readonly object _lock = new();
    private IConnection? _connection;
    private bool _disposed;

    public SharedRabbitMqConnection(
        IConfiguration configuration,
        ILogger<SharedRabbitMqConnection> logger)
    {
        _logger = logger;

        // Support both naming conventions used across services
        var host = configuration["RabbitMQ:Host"]
                ?? configuration["RabbitMQ:HostName"]
                ?? "localhost";
        var port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672");
        var userName = configuration["RabbitMQ:Username"]
                    ?? configuration["RabbitMQ:UserName"]
                    ?? "guest";
        var password = configuration["RabbitMQ:Password"] ?? "guest";
        var virtualHost = configuration["RabbitMQ:VirtualHost"] ?? "/";

        _factory = new ConnectionFactory
        {
            HostName = host,
            Port = port,
            UserName = userName,
            Password = password,
            VirtualHost = virtualHost,
            DispatchConsumersAsync = true,
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
            TopologyRecoveryEnabled = true,
            RequestedHeartbeat = TimeSpan.FromSeconds(30),
            // Connection name helps identify the service in RabbitMQ Management UI
            ClientProvidedName = $"{Environment.GetEnvironmentVariable("SERVICE_NAME") ?? "okla-service"}-{Environment.MachineName}"
        };

        _logger.LogInformation(
            "SharedRabbitMqConnection configured: Host={Host}, Port={Port}, VHost={VHost}",
            host, port, virtualHost);
    }

    /// <summary>
    /// Gets the shared connection, creating it if necessary.
    /// Thread-safe — only one connection is created per pod/process.
    /// </summary>
    public IConnection GetConnection()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SharedRabbitMqConnection));

        if (_connection is { IsOpen: true })
            return _connection;

        lock (_lock)
        {
            if (_connection is { IsOpen: true })
                return _connection;

            try
            {
                _connection?.Dispose();
                _connection = _factory.CreateConnection();
                _logger.LogInformation(
                    "✅ SharedRabbitMqConnection established (ClientName={ClientName})",
                    _factory.ClientProvidedName);
                return _connection;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to create shared RabbitMQ connection");
                throw;
            }
        }
    }

    /// <summary>
    /// Creates a new channel (IModel) from the shared connection.
    /// Each publisher/consumer should create its own channel.
    /// Channels are lightweight — creating many is fine.
    /// </summary>
    public IModel CreateChannel()
    {
        var connection = GetConnection();
        return connection.CreateModel();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        try
        {
            _connection?.Close(TimeSpan.FromSeconds(5));
            _connection?.Dispose();
            _logger.LogInformation("SharedRabbitMqConnection disposed");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error disposing SharedRabbitMqConnection");
        }
    }
}
