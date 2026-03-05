using System.Text;
using System.Text.Json;
using CarDealer.Contracts.Events.Auth;
using DealerManagementService.Domain.Entities;
using DealerManagementService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DealerManagementService.Infrastructure.Services.Messaging;

/// <summary>
/// Consumer que escucha eventos UserRegisteredEvent de AuthService.
/// Cuando un usuario se registra con AccountType = Dealer, 
/// automáticamente crea el perfil de dealer asociado.
/// </summary>
public class UserRegisteredEventConsumer : BackgroundService
{
    private readonly ILogger<UserRegisteredEventConsumer> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IModel? _channel;
    private const string QueueName = "dealermanagement.userregistered";
    private const string ExchangeName = "cardealer.events";
    private readonly JsonSerializerOptions _jsonOptions;

    public UserRegisteredEventConsumer(
        ILogger<UserRegisteredEventConsumer> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        try
        {
            // Configurar RabbitMQ
            var factory = new ConnectionFactory
            {
                HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost",
                Port = int.Parse(Environment.GetEnvironmentVariable("RABBITMQ_PORT") ?? "5672"),
                UserName = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? throw new InvalidOperationException("RABBITMQ_USER environment variable is not configured"),
                Password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? throw new InvalidOperationException("RABBITMQ_PASSWORD environment variable is not configured"),
                DispatchConsumersAsync = true
            };

            var connection = factory.CreateConnection();
            _channel = connection.CreateModel();

            // Declarar exchange y queue
            _channel.ExchangeDeclare(ExchangeName, ExchangeType.Topic, durable: true);
            _channel.QueueDeclare(QueueName, durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind(QueueName, ExchangeName, "auth.user.registered");

            _logger.LogInformation("✅ RabbitMQ channel configured for DealerManagementService");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ RabbitMQ not available. UserRegisteredEventConsumer will not start");
            _channel = null;
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_channel == null)
        {
            _logger.LogWarning("RabbitMQ channel not available, UserRegisteredEventConsumer will not start");
            return;
        }

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.Received += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                _logger.LogInformation("📨 Received UserRegisteredEvent: {Message}", message);

                var @event = JsonSerializer.Deserialize<UserRegisteredEvent>(message, _jsonOptions);

                if (@event != null)
                {
                    _logger.LogInformation("🔍 Deserializado correctamente - UserId: {UserId}, Email: {Email}", @event.UserId, @event.Email);
                    await ProcessUserRegisteredEvent(@event);
                    _logger.LogInformation("✅ Successfully processed UserRegisteredEvent for user {UserId}", @event.UserId);
                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                else
                {
                    _logger.LogWarning("⚠️ Failed to deserialize UserRegisteredEvent");
                    _channel.BasicNack(ea.DeliveryTag, false, false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error processing UserRegisteredEvent - Message: {Message}, StackTrace: {StackTrace}", 
                    ex.Message, ex.StackTrace);
                _channel.BasicNack(ea.DeliveryTag, false, true); // Requeue on error
            }
        };

        _channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);

        _logger.LogInformation("🎧 UserRegisteredEventConsumer started, listening on queue: {Queue}", QueueName);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task ProcessUserRegisteredEvent(UserRegisteredEvent @event)
    {
        _logger.LogInformation("🔄 Starting ProcessUserRegisteredEvent for UserId: {UserId}", @event.UserId);
        
        // Verificar si el usuario es dealer por metadata
        var isDealer = false;
        if (@event.Metadata != null && 
            @event.Metadata.TryGetValue("AccountType", out var accountTypeStr))
        {
            // AccountType.Dealer = 2 in the platform enum.
            // Support both integer string ("2") and name string ("Dealer"/"dealer").
            if (int.TryParse(accountTypeStr, out var accountType))
            {
                isDealer = accountType == 2; // 2 = Dealer (NOT 1 which is Buyer)
            }
            else
            {
                isDealer = accountTypeStr.Equals("Dealer", StringComparison.OrdinalIgnoreCase)
                        || accountTypeStr.Equals("dealer", StringComparison.OrdinalIgnoreCase);
            }
            _logger.LogInformation("✓ AccountType encontrado: {AccountType}, Es Dealer: {IsDealer}", accountTypeStr, isDealer);
        }
        else
        {
            _logger.LogWarning("⚠️ Metadata o AccountType no encontrado. Metadata: {Metadata}", 
                @event.Metadata != null ? string.Join(", ", @event.Metadata.Select(kv => $"{kv.Key}={kv.Value}")) : "null");
        }

        if (!isDealer)
        {
            _logger.LogInformation("ℹ️ User {UserId} is not a Dealer, skipping dealer creation", @event.UserId);
            return;
        }

        _logger.LogInformation("🏢 Creating scope and getting DbContext...");
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DealerDbContext>();

        try
        {
            _logger.LogInformation("🔍 Checking if dealer already exists for UserId: {UserId}", @event.UserId);
            
            // Verificar si ya existe un dealer con ese UserId
            var userId = @event.UserId; // Guid
            var existingDealer = await dbContext.Dealers
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (existingDealer != null)
            {
                _logger.LogInformation("ℹ️ Dealer profile already exists for UserId {UserId}, skipping", @event.UserId);
                return;
            }

            _logger.LogInformation("➕ Creating new dealer profile...");
            
            // Generar RNC temporal único (prefix TEMP- + primeros 8 chars del UserId)
            var tempRnc = $"TEMP-{@event.UserId.ToString().Substring(0, 8).ToUpper()}";
            
            // Crear dealer automáticamente con datos básicos
            var dealer = new Dealer
            {
                Id = Guid.NewGuid(),
                UserId = @event.UserId, // ← CRÍTICO: Usar el UserId del evento directamente (Guid)
                BusinessName = @event.FullName ?? @event.Email.Split('@')[0],
                RNC = tempRnc, // RNC temporal hasta que complete el perfil
                LegalName = @event.FullName ?? @event.Email.Split('@')[0],
                Email = @event.Email,
                Phone = string.Empty, // Requerido
                Address = string.Empty, // Requerido
                City = string.Empty, // Requerido
                Province = string.Empty, // Requerido
                Status = DealerStatus.Pending, // Requiere completar perfil
                VerificationStatus = VerificationStatus.NotVerified,
                CurrentPlan = DealerPlan.Free, // Plan inicial gratuito
                MaxActiveListings = 3, // Límite del plan Free
                CurrentActiveListings = 0,
                IsSubscriptionActive = true, // Free plan está activo por defecto
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _logger.LogInformation("💾 Adding dealer to DbContext...");
            dbContext.Dealers.Add(dealer);
            
            _logger.LogInformation("💿 Saving changes to database...");
            await dbContext.SaveChangesAsync();

            _logger.LogInformation(
                "✅ Auto-created dealer profile for user {UserId} ({Email}) with plan {Plan}", 
                @event.UserId, @event.Email, dealer.CurrentPlan);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error creating dealer profile for user {UserId} - Message: {Message}, StackTrace: {StackTrace}", 
                @event.UserId, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public override void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        base.Dispose();
    }
}
