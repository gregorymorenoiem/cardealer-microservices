using System.Text;
using System.Text.Json;
using CarDealer.Contracts.Events.Auth;
using AuthService.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis;

namespace AuthService.Infrastructure.Messaging;

// ═══════════════════════════════════════════════════════════════════════════════
// USER DATA DELETION CONSUMER — LEY 172-13 CASCADE DELETION (AUTH-LOCAL)
//
// Consumes UserDeletedEvent published by UserService's AccountDeletionWorker.
// HARD-DELETES all auth-related data for the deleted user:
//   • Refresh tokens (all — active, expired, revoked)
//   • User sessions (all)
//   • Login history (all)
//   • Trusted devices (all)
//   • Verification tokens (by email)
//   • Two-factor auth configuration
//   • Redis cache keys (session cache, token blacklist)
//
// Hard deletion is required because:
//   1. Auth data has no business value after user deletion
//   2. Keeping session/token data is a security risk
//   3. Ley 172-13 Art. 5 requires complete PII removal
//
// Queue: auth.user.deletion
// Exchange: cardealer.events (topic)
// Routing key: auth.user.deleted
// DLQ: auth.user.deletion.dlq
// ═══════════════════════════════════════════════════════════════════════════════

public sealed class UserDataDeletionConsumer : BackgroundService
{
    private readonly ILogger<UserDataDeletionConsumer> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private IConnection? _connection;
    private IModel? _channel;

    private const string QueueName = "auth.user.deletion";
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
            _logger.LogInformation("[UserDeletion-Auth] RabbitMQ disabled, consumer inactive");
            return;
        }

        InitializeRabbitMQ();

        if (_channel == null)
        {
            _logger.LogWarning("[UserDeletion-Auth] Failed to initialize RabbitMQ, consumer inactive");
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
                    _logger.LogWarning("[UserDeletion-Auth] Invalid event payload, sending to DLQ");
                    _channel.BasicNack(ea.DeliveryTag, false, false);
                    return;
                }

                _logger.LogInformation(
                    "[UserDeletion-Auth] Processing deletion for UserId={UserId}, Email={Email}",
                    deletionEvent.UserId, deletionEvent.Email);

                await ProcessUserDeletionAsync(deletionEvent, stoppingToken);

                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UserDeletion-Auth] Error processing message, sending to DLQ");
                _channel.BasicNack(ea.DeliveryTag, false, false);
            }
        };

        _channel.BasicConsume(
            queue: QueueName,
            autoAck: false,
            consumer: consumer);

        _logger.LogInformation("[UserDeletion-Auth] Listening on queue '{Queue}' for user deletion events", QueueName);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task ProcessUserDeletionAsync(UserDeletedEvent deletionEvent, CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();

        var refreshTokenRepo = scope.ServiceProvider.GetRequiredService<IRefreshTokenRepository>();
        var userSessionRepo = scope.ServiceProvider.GetRequiredService<IUserSessionRepository>();
        var loginHistoryRepo = scope.ServiceProvider.GetRequiredService<ILoginHistoryRepository>();
        var trustedDeviceRepo = scope.ServiceProvider.GetRequiredService<ITrustedDeviceRepository>();
        var verificationTokenRepo = scope.ServiceProvider.GetRequiredService<IVerificationTokenRepository>();
        var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        // AuthService uses string UserId (ASP.NET Identity), event carries Guid
        var userId = deletionEvent.UserId.ToString();
        var email = deletionEvent.Email;

        // ── Step 1: Delete refresh tokens ──
        var refreshTokensDeleted = await refreshTokenRepo.DeleteAllByUserIdAsync(userId, ct);

        // ── Step 2: Delete user sessions ──
        var sessionsDeleted = await userSessionRepo.DeleteAllByUserIdAsync(userId, ct);

        // ── Step 3: Delete login history ──
        var loginHistoryDeleted = await loginHistoryRepo.DeleteAllByUserIdAsync(userId, ct);

        // ── Step 4: Delete trusted devices ──
        var trustedDevicesDeleted = await trustedDeviceRepo.DeleteAllByUserIdAsync(userId, ct);

        // ── Step 5: Delete verification tokens (by email) ──
        var verificationTokensDeleted = 0;
        if (!string.IsNullOrEmpty(email))
        {
            verificationTokensDeleted = await verificationTokenRepo.DeleteByEmailAsync(email, ct);
        }

        // ── Step 6: Delete 2FA configuration ──
        try
        {
            await userRepo.RemoveTwoFactorAuthAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[UserDeletion-Auth] 2FA removal failed for {UserId} (may not exist)", userId);
        }

        // ── Step 7: Invalidate Redis cache ──
        await InvalidateRedisCacheAsync(userId, email);

        _logger.LogInformation(
            "[UserDeletion-Auth] Completed Ley 172-13 cascade deletion for user {UserId}: " +
            "RefreshTokens={RefreshTokens}, Sessions={Sessions}, LoginHistory={LoginHistory}, " +
            "TrustedDevices={Devices}, VerificationTokens={VerifTokens}",
            userId, refreshTokensDeleted, sessionsDeleted, loginHistoryDeleted,
            trustedDevicesDeleted, verificationTokensDeleted);
    }

    private async Task InvalidateRedisCacheAsync(string userId, string? email)
    {
        try
        {
            var redis = _serviceProvider.GetService<IConnectionMultiplexer>();
            if (redis == null)
            {
                _logger.LogDebug("[UserDeletion-Auth] Redis not available, skipping cache invalidation");
                return;
            }

            var db = redis.GetDatabase();
            var server = redis.GetServer(redis.GetEndPoints().First());

            // Scan for keys with AuthService_ prefix matching this user
            var keysDeleted = 0;
            var patterns = new[]
            {
                $"AuthService_*{userId}*",
                $"AuthService_*{email}*"
            };

            foreach (var pattern in patterns)
            {
                await foreach (var key in server.KeysAsync(pattern: pattern))
                {
                    await db.KeyDeleteAsync(key);
                    keysDeleted++;
                }
            }

            _logger.LogInformation(
                "[UserDeletion-Auth] Redis cache invalidated for user {UserId}: {Keys} keys deleted",
                userId, keysDeleted);
        }
        catch (Exception ex)
        {
            // Redis failures should not fail the deletion — data will expire naturally via TTL
            _logger.LogWarning(ex, "[UserDeletion-Auth] Redis cache invalidation failed for user {UserId}", userId);
        }
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

            _connection = factory.CreateConnection($"AuthService-UserDeletion-{Environment.MachineName}");
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
                "[UserDeletion-Auth] RabbitMQ initialized — Queue={Queue}, Exchange={Exchange}",
                QueueName, exchangeName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[UserDeletion-Auth] Failed to initialize RabbitMQ: {Error}", ex.Message);
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
