# 📋 Template: Eventos de Dominio y RabbitMQ

Guía para crear eventos de dominio y comunicación asíncrona entre servicios.

---

## 1. Definir Evento en CarDealer.Contracts

Los eventos se definen en `backend/_Shared/CarDealer.Contracts/Events/`:

```csharp
// CarDealer.Contracts/Events/{Category}/{Entity}CreatedEvent.cs
using CarDealer.Contracts.Abstractions;

namespace CarDealer.Contracts.Events.{Category};

/// <summary>
/// Evento publicado cuando se crea un nuevo {Entity}.
/// </summary>
public class {Entity}CreatedEvent : EventBase
{
    public override string EventType => "{category}.{entity}.created";

    /// <summary>
    /// ID único del {Entity} creado.
    /// </summary>
    public Guid {Entity}Id { get; set; }

    /// <summary>
    /// Nombre del {Entity}.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// ID del tenant (dealer).
    /// </summary>
    public Guid DealerId { get; set; }

    /// <summary>
    /// Timestamp de creación.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Metadata adicional.
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
}
```

### Estructura de Carpetas

```
CarDealer.Contracts/
├── Abstractions/
│   ├── EventBase.cs
│   └── IEvent.cs
├── Events/
│   ├── Auth/
│   │   ├── UserRegisteredEvent.cs
│   │   ├── UserLoggedInEvent.cs
│   │   └── PasswordChangedEvent.cs
│   ├── Vehicle/
│   │   ├── VehicleCreatedEvent.cs
│   │   ├── VehicleUpdatedEvent.cs
│   │   └── VehicleSoldEvent.cs
│   ├── Media/
│   │   ├── MediaUploadedEvent.cs
│   │   └── MediaDeletedEvent.cs
│   ├── Notification/
│   │   └── NotificationSentEvent.cs
│   └── Audit/
│       └── AuditLogCreatedEvent.cs
└── DTOs/
    └── Common/
```

---

## 2. Clase Base de Eventos

```csharp
// CarDealer.Contracts/Abstractions/IEvent.cs
namespace CarDealer.Contracts.Abstractions;

/// <summary>
/// Interface base para todos los eventos de dominio.
/// </summary>
public interface IEvent
{
    /// <summary>
    /// ID único del evento.
    /// </summary>
    Guid EventId { get; set; }

    /// <summary>
    /// Timestamp cuando ocurrió el evento.
    /// </summary>
    DateTime OccurredAt { get; set; }

    /// <summary>
    /// Tipo del evento (e.g., "auth.user.registered").
    /// </summary>
    string EventType { get; }
}

// CarDealer.Contracts/Abstractions/EventBase.cs
namespace CarDealer.Contracts.Abstractions;

/// <summary>
/// Clase base para todos los eventos de dominio.
/// </summary>
public abstract class EventBase : IEvent
{
    public Guid EventId { get; set; } = Guid.NewGuid();
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public abstract string EventType { get; }
}
```

---

## 3. Event Publisher (RabbitMQ)

```csharp
// {ServiceName}.Infrastructure/Messaging/RabbitMqEventPublisher.cs
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using CarDealer.Contracts.Abstractions;

namespace {ServiceName}.Infrastructure.Messaging;

public interface IEventPublisher
{
    Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) 
        where T : IEvent;
}

public class RabbitMqEventPublisher : IEventPublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMqEventPublisher> _logger;
    private const string ExchangeName = "cardealer.events";

    public RabbitMqEventPublisher(
        IConfiguration configuration, 
        ILogger<RabbitMqEventPublisher> logger)
    {
        _logger = logger;

        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMQ:Host"] ?? "localhost",
            Port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672"),
            UserName = configuration["RabbitMQ:Username"] ?? "guest",
            Password = configuration["RabbitMQ:Password"] ?? "guest",
            VirtualHost = configuration["RabbitMQ:VirtualHost"] ?? "/"
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // Declarar exchange de tipo topic
        _channel.ExchangeDeclare(
            exchange: ExchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false
        );

        _logger.LogInformation("RabbitMQ connection established");
    }

    public Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) 
        where T : IEvent
    {
        try
        {
            var routingKey = @event.EventType; // e.g., "auth.user.registered"
            var message = JsonSerializer.Serialize(@event);
            var body = Encoding.UTF8.GetBytes(message);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.MessageId = @event.EventId.ToString();
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            properties.ContentType = "application/json";
            properties.Headers = new Dictionary<string, object>
            {
                ["event_type"] = @event.EventType
            };

            _channel.BasicPublish(
                exchange: ExchangeName,
                routingKey: routingKey,
                basicProperties: properties,
                body: body
            );

            _logger.LogInformation(
                "Published event {EventType} with ID {EventId}",
                @event.EventType,
                @event.EventId);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish event {EventType}", @event.EventType);
            throw;
        }
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
```

---

## 4. Event Consumer (Subscriber)

```csharp
// {ServiceName}.Infrastructure/Messaging/{Event}Consumer.cs
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using CarDealer.Contracts.Events.Auth;

namespace {ServiceName}.Infrastructure.Messaging;

public class UserRegisteredEventConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<UserRegisteredEventConsumer> _logger;
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private const string ExchangeName = "cardealer.events";
    private const string QueueName = "{servicename}.user.registered";
    private const string RoutingKey = "auth.user.registered";

    public UserRegisteredEventConsumer(
        IConfiguration configuration,
        IServiceProvider serviceProvider,
        ILogger<UserRegisteredEventConsumer> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMQ:Host"] ?? "localhost",
            Port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672"),
            UserName = configuration["RabbitMQ:Username"] ?? "guest",
            Password = configuration["RabbitMQ:Password"] ?? "guest"
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // Declarar exchange
        _channel.ExchangeDeclare(
            exchange: ExchangeName,
            type: ExchangeType.Topic,
            durable: true
        );

        // Declarar queue
        _channel.QueueDeclare(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false
        );

        // Bind queue al exchange con routing key
        _channel.QueueBind(
            queue: QueueName,
            exchange: ExchangeName,
            routingKey: RoutingKey
        );

        _logger.LogInformation("Consumer initialized for queue: {Queue}", QueueName);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var @event = JsonSerializer.Deserialize<UserRegisteredEvent>(message);

                if (@event != null)
                {
                    _logger.LogInformation(
                        "Received UserRegisteredEvent: UserId={UserId}, Email={Email}",
                        @event.UserId,
                        @event.Email);

                    await HandleEventAsync(@event, stoppingToken);

                    _channel.BasicAck(ea.DeliveryTag, multiple: false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message");
                
                // Requeue si falla
                _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
            }
        };

        _channel.BasicConsume(
            queue: QueueName,
            autoAck: false,
            consumer: consumer
        );

        return Task.CompletedTask;
    }

    private async Task HandleEventAsync(
        UserRegisteredEvent @event, 
        CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        
        // Obtener servicios del scope
        var handler = scope.ServiceProvider.GetRequiredService<IUserRegisteredEventHandler>();
        
        await handler.HandleAsync(@event, cancellationToken);
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}
```

---

## 5. Event Handler

```csharp
// {ServiceName}.Application/EventHandlers/UserRegisteredEventHandler.cs
using Microsoft.Extensions.Logging;
using CarDealer.Contracts.Events.Auth;

namespace {ServiceName}.Application.EventHandlers;

public interface IUserRegisteredEventHandler
{
    Task HandleAsync(UserRegisteredEvent @event, CancellationToken cancellationToken);
}

public class UserRegisteredEventHandler : IUserRegisteredEventHandler
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<UserRegisteredEventHandler> _logger;

    public UserRegisteredEventHandler(
        INotificationService notificationService,
        ILogger<UserRegisteredEventHandler> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task HandleAsync(
        UserRegisteredEvent @event, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling UserRegisteredEvent for user {Email}", 
            @event.Email);

        // Ejemplo: Enviar email de bienvenida
        await _notificationService.SendWelcomeEmailAsync(
            @event.Email,
            @event.FullName,
            cancellationToken);

        _logger.LogInformation(
            "Welcome email sent to {Email}", 
            @event.Email);
    }
}
```

---

## 6. Registro en DI

```csharp
// {ServiceName}.Infrastructure/Extensions/MessagingExtensions.cs
using Microsoft.Extensions.DependencyInjection;
using {ServiceName}.Infrastructure.Messaging;
using {ServiceName}.Application.EventHandlers;

namespace {ServiceName}.Infrastructure.Extensions;

public static class MessagingExtensions
{
    public static IServiceCollection AddMessaging(this IServiceCollection services)
    {
        // Event Publisher
        services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();

        // Event Handlers
        services.AddScoped<IUserRegisteredEventHandler, UserRegisteredEventHandler>();

        // Event Consumers (Background Services)
        services.AddHostedService<UserRegisteredEventConsumer>();

        return services;
    }
}
```

---

## 7. Configuración RabbitMQ

```json
// appsettings.json
{
  "RabbitMQ": {
    "Host": "localhost",
    "Port": 5672,
    "Username": "guest",
    "Password": "guest",
    "VirtualHost": "/"
  }
}
```

---

## 8. Dead Letter Queue (DLQ)

```csharp
// Configuración de DLQ para mensajes fallidos
_channel.ExchangeDeclare(
    exchange: "cardealer.dlx",  // Dead Letter Exchange
    type: ExchangeType.Fanout,
    durable: true
);

_channel.QueueDeclare(
    queue: "cardealer.dlq",
    durable: true,
    exclusive: false,
    autoDelete: false
);

_channel.QueueBind(
    queue: "cardealer.dlq",
    exchange: "cardealer.dlx",
    routingKey: ""
);

// Queue principal con DLQ configurada
var args = new Dictionary<string, object>
{
    ["x-dead-letter-exchange"] = "cardealer.dlx",
    ["x-message-ttl"] = 86400000  // 24 horas
};

_channel.QueueDeclare(
    queue: QueueName,
    durable: true,
    exclusive: false,
    autoDelete: false,
    arguments: args
);
```

---

## 9. Mapa de Eventos por Servicio

| Servicio | Eventos que PUBLICA | Eventos que CONSUME |
|----------|---------------------|---------------------|
| **AuthService** | `auth.user.registered`, `auth.user.logged_in`, `auth.password.changed` | - |
| **NotificationService** | `notification.sent` | `auth.user.registered`, `vehicle.created`, `vehicle.sold` |
| **AuditService** | `audit.log.created` | Todos (wildcard `*.*.#`) |
| **ErrorService** | `error.logged` | `*.*.error` |
| **ProductService** | `vehicle.created`, `vehicle.updated`, `vehicle.sold` | - |
| **MediaService** | `media.uploaded`, `media.deleted` | `vehicle.created` |
