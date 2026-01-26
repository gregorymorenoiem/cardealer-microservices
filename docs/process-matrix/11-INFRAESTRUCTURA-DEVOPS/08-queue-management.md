# 📬 Queue Management - Gestión de Colas RabbitMQ - Matriz de Procesos

> **Tecnología:** RabbitMQ 3.12+  
> **Librería:** MassTransit  
> **Última actualización:** Enero 25, 2026  
> **Estado de Implementación:** ✅ 85% Backend | N/A UI (RabbitMQ Management)

---

## ⚠️ AUDITORÍA DE ACCESO UI (Enero 25, 2026)

| Proceso           | Backend | UI Access | Observación                            |
| ----------------- | ------- | --------- | -------------------------------------- |
| Publishers        | ✅ 100% | N/A       | MassTransit integrado                  |
| Subscribers       | ✅ 100% | N/A       | Consumers en servicios                 |
| Dead Letter Queue | ✅ 100% | N/A       | ErrorService integrado                 |
| Retry Policies    | ✅ 100% | N/A       | Exponential backoff                    |
| Sagas             | 🔴 0%   | N/A       | Pendiente para transacciones complejas |

### Rutas UI Existentes ✅

- RabbitMQ Management UI: http://rabbitmq:15672 (admin)
- Grafana dashboards para métricas de colas

### Rutas UI Faltantes 🔴

- Ninguna requerida - RabbitMQ Management es suficiente

**Verificación Backend:** RabbitMQ + MassTransit en todos los servicios ✅

---

## 📊 Resumen de Implementación

| Componente                       | Total | Implementado | Pendiente | Estado       |
| -------------------------------- | ----- | ------------ | --------- | ------------ |
| **MQ-PUB-\*** (Publishers)       | 5     | 5            | 0         | ✅ 100%      |
| **MQ-SUB-\*** (Subscribers)      | 5     | 5            | 0         | ✅ 100%      |
| **MQ-DLQ-\*** (Dead Letter)      | 4     | 4            | 0         | ✅ 100%      |
| **MQ-RETRY-\*** (Retry Policies) | 3     | 3            | 0         | ✅ 100%      |
| **MQ-SAGA-\*** (Sagas)           | 4     | 0            | 4         | 🔴 Pendiente |
| **Tests**                        | 20    | 17           | 3         | 🟢 85%       |
| **TOTAL**                        | 41    | 34           | 7         | 🟢 83%       |

---

## 1. Información General

### 1.1 Descripción

Sistema de mensajería asíncrona basado en RabbitMQ que maneja la comunicación entre microservicios mediante eventos de dominio y comandos. Utiliza MassTransit como abstracción de alto nivel.

### 1.2 Arquitectura

```
┌─────────────────────────────────────────────────────────────────────────┐
│                      Message Queue Architecture                          │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│   Publishers                        RabbitMQ                             │
│   ┌──────────┐                    ┌─────────────────────────────────┐   │
│   │ Vehicles │───▶ VehicleCreated │                                 │   │
│   │ Service  │    VehicleUpdated  │   Exchanges        Queues       │   │
│   └──────────┘                    │   ┌───────┐      ┌────────┐    │   │
│   ┌──────────┐                    │   │vehicle│──────│vehicle │    │   │
│   │ Billing  │───▶ PaymentSuccess │   │events │      │.created│    │   │
│   │ Service  │    SubscriptionNew │   └───────┘      └────────┘    │   │
│   └──────────┘                    │   ┌───────┐      ┌────────┐    │   │
│   ┌──────────┐                    │   │billing│──────│billing │    │   │
│   │  Auth    │───▶ UserRegistered │   │events │      │.payment│    │   │
│   │ Service  │    UserLoggedIn    │   └───────┘      └────────┘    │   │
│   └──────────┘                    │   ┌───────┐      ┌────────┐    │   │
│                                   │   │ DLX   │──────│error_  │    │   │
│   Consumers                       │   │       │      │queue   │    │   │
│   ┌──────────┐                    │   └───────┘      └────────┘    │   │
│   │Notificat.│◀─── VehicleCreated │                                 │   │
│   │ Service  │     PaymentSuccess │   Management UI: :15672          │   │
│   └──────────┘                    └─────────────────────────────────┘   │
│   ┌──────────┐                                                          │
│   │Analytics │◀─── All Events                                           │
│   │ Service  │                                                          │
│   └──────────┘                                                          │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

### 1.3 Dependencias

| Componente  | Versión | Propósito        |
| ----------- | ------- | ---------------- |
| RabbitMQ    | 3.12+   | Message Broker   |
| MassTransit | 8.1+    | Abstracción .NET |
| Erlang/OTP  | 26+     | Runtime          |

---

## 2. Configuración

### 2.1 Connection String

```json
{
  "RabbitMQ": {
    "Host": "rabbitmq.okla.svc.cluster.local",
    "Port": 5672,
    "ManagementPort": 15672,
    "Username": "${RABBITMQ_USER}",
    "Password": "${RABBITMQ_PASSWORD}",
    "VirtualHost": "/okla",
    "PrefetchCount": 16,
    "ConcurrentMessageLimit": 8
  }
}
```

### 2.2 MassTransit Configuration

```csharp
// Program.cs
builder.Services.AddMassTransit(x =>
{
    // Registrar consumers automáticamente
    x.AddConsumers(typeof(Program).Assembly);

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitConfig = context.GetRequiredService<IOptions<RabbitMQSettings>>().Value;

        cfg.Host(rabbitConfig.Host, rabbitConfig.Port, rabbitConfig.VirtualHost, h =>
        {
            h.Username(rabbitConfig.Username);
            h.Password(rabbitConfig.Password);
        });

        // Configuración global
        cfg.PrefetchCount = rabbitConfig.PrefetchCount;
        cfg.ConcurrentMessageLimit = rabbitConfig.ConcurrentMessageLimit;

        // Retry policy
        cfg.UseMessageRetry(r => r.Exponential(
            retryLimit: 5,
            minInterval: TimeSpan.FromSeconds(1),
            maxInterval: TimeSpan.FromMinutes(5),
            intervalDelta: TimeSpan.FromSeconds(2)));

        // Dead letter queue
        cfg.UseDelayedRedelivery(r => r.Intervals(
            TimeSpan.FromMinutes(5),
            TimeSpan.FromMinutes(15),
            TimeSpan.FromMinutes(30)));

        // Circuit breaker
        cfg.UseCircuitBreaker(cb =>
        {
            cb.TrackingPeriod = TimeSpan.FromMinutes(1);
            cb.TripThreshold = 15;
            cb.ActiveThreshold = 10;
            cb.ResetInterval = TimeSpan.FromMinutes(5);
        });

        cfg.ConfigureEndpoints(context);
    });
});
```

---

## 3. Eventos de Dominio

### 3.1 Vehicle Events

```csharp
namespace CarDealer.Contracts.Events;

// Vehículo creado
public record VehicleCreatedEvent
{
    public Guid VehicleId { get; init; }
    public Guid DealerId { get; init; }
    public string Make { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public int Year { get; init; }
    public decimal Price { get; init; }
    public string Status { get; init; } = "Pending";
    public DateTime CreatedAt { get; init; }
    public Guid CorrelationId { get; init; }
}

// Vehículo actualizado
public record VehicleUpdatedEvent
{
    public Guid VehicleId { get; init; }
    public Guid DealerId { get; init; }
    public Dictionary<string, object> Changes { get; init; } = new();
    public DateTime UpdatedAt { get; init; }
    public Guid UpdatedBy { get; init; }
}

// Vehículo eliminado
public record VehicleDeletedEvent
{
    public Guid VehicleId { get; init; }
    public Guid DealerId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public DateTime DeletedAt { get; init; }
    public Guid DeletedBy { get; init; }
}

// Vehículo vendido
public record VehicleSoldEvent
{
    public Guid VehicleId { get; init; }
    public Guid DealerId { get; init; }
    public Guid BuyerId { get; init; }
    public decimal SalePrice { get; init; }
    public DateTime SoldAt { get; init; }
}

// Estado de vehículo cambiado
public record VehicleStatusChangedEvent
{
    public Guid VehicleId { get; init; }
    public string OldStatus { get; init; } = string.Empty;
    public string NewStatus { get; init; } = string.Empty;
    public string? Reason { get; init; }
    public DateTime ChangedAt { get; init; }
}
```

### 3.2 User Events

```csharp
// Usuario registrado
public record UserRegisteredEvent
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string AccountType { get; init; } = "Individual";
    public DateTime RegisteredAt { get; init; }
    public string? ReferralCode { get; init; }
}

// Usuario verificado
public record UserVerifiedEvent
{
    public Guid UserId { get; init; }
    public string VerificationType { get; init; } = string.Empty; // Email, Phone, Identity
    public DateTime VerifiedAt { get; init; }
}

// Usuario logueado
public record UserLoggedInEvent
{
    public Guid UserId { get; init; }
    public string IpAddress { get; init; } = string.Empty;
    public string UserAgent { get; init; } = string.Empty;
    public DateTime LoggedInAt { get; init; }
}
```

### 3.3 Billing Events

```csharp
// Pago exitoso
public record PaymentSuccessEvent
{
    public Guid PaymentId { get; init; }
    public Guid UserId { get; init; }
    public Guid? DealerId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "DOP";
    public string PaymentType { get; init; } = string.Empty; // Subscription, Listing, Featured
    public string Gateway { get; init; } = string.Empty; // Stripe, Azul
    public string TransactionId { get; init; } = string.Empty;
    public DateTime ProcessedAt { get; init; }
}

// Pago fallido
public record PaymentFailedEvent
{
    public Guid PaymentId { get; init; }
    public Guid UserId { get; init; }
    public decimal Amount { get; init; }
    public string FailureReason { get; init; } = string.Empty;
    public string Gateway { get; init; } = string.Empty;
    public DateTime FailedAt { get; init; }
}

// Suscripción creada
public record SubscriptionCreatedEvent
{
    public Guid SubscriptionId { get; init; }
    public Guid DealerId { get; init; }
    public string PlanId { get; init; } = string.Empty;
    public string PlanName { get; init; } = string.Empty;
    public decimal MonthlyPrice { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}

// Suscripción cancelada
public record SubscriptionCancelledEvent
{
    public Guid SubscriptionId { get; init; }
    public Guid DealerId { get; init; }
    public string CancellationReason { get; init; } = string.Empty;
    public DateTime CancelledAt { get; init; }
    public DateTime EffectiveEndDate { get; init; }
}
```

### 3.4 Lead Events

```csharp
// Lead creado
public record LeadCreatedEvent
{
    public Guid LeadId { get; init; }
    public Guid VehicleId { get; init; }
    public Guid DealerId { get; init; }
    public Guid? BuyerId { get; init; }
    public string ContactName { get; init; } = string.Empty;
    public string ContactPhone { get; init; } = string.Empty;
    public string? ContactEmail { get; init; }
    public string Source { get; init; } = string.Empty; // Web, WhatsApp, Phone
    public DateTime CreatedAt { get; init; }
}

// Lead actualizado
public record LeadStatusUpdatedEvent
{
    public Guid LeadId { get; init; }
    public string OldStatus { get; init; } = string.Empty;
    public string NewStatus { get; init; } = string.Empty;
    public string? Notes { get; init; }
    public DateTime UpdatedAt { get; init; }
}
```

---

## 4. Publishers

### 4.1 Event Publisher Service

```csharp
public class DomainEventPublisher : IDomainEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<DomainEventPublisher> _logger;

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class
    {
        try
        {
            await _publishEndpoint.Publish(@event, cancellationToken);

            _logger.LogInformation("Published event {EventType} with CorrelationId {CorrelationId}",
                typeof(TEvent).Name,
                (@event as dynamic)?.CorrelationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish event {EventType}", typeof(TEvent).Name);
            throw;
        }
    }

    public async Task PublishBatchAsync<TEvent>(IEnumerable<TEvent> events,
        CancellationToken cancellationToken = default) where TEvent : class
    {
        foreach (var @event in events)
        {
            await PublishAsync(@event, cancellationToken);
        }
    }
}
```

### 4.2 Uso en Handlers

```csharp
public class CreateVehicleHandler : IRequestHandler<CreateVehicleCommand, Result<VehicleDto>>
{
    private readonly IVehicleRepository _repository;
    private readonly IDomainEventPublisher _eventPublisher;

    public async Task<Result<VehicleDto>> Handle(CreateVehicleCommand request,
        CancellationToken cancellationToken)
    {
        var vehicle = new Vehicle(/* ... */);
        await _repository.AddAsync(vehicle, cancellationToken);

        // Publicar evento después de guardar
        await _eventPublisher.PublishAsync(new VehicleCreatedEvent
        {
            VehicleId = vehicle.Id,
            DealerId = vehicle.DealerId,
            Make = vehicle.Make,
            Model = vehicle.Model,
            Year = vehicle.Year,
            Price = vehicle.Price,
            Status = vehicle.Status,
            CreatedAt = vehicle.CreatedAt,
            CorrelationId = request.CorrelationId
        }, cancellationToken);

        return Result.Success(vehicle.ToDto());
    }
}
```

---

## 5. Consumers

### 5.1 Notification Consumer

```csharp
public class VehicleCreatedConsumer : IConsumer<VehicleCreatedEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<VehicleCreatedConsumer> _logger;

    public async Task Consume(ConsumeContext<VehicleCreatedEvent> context)
    {
        var @event = context.Message;

        _logger.LogInformation("Processing VehicleCreatedEvent for {VehicleId}", @event.VehicleId);

        try
        {
            // Notificar al dealer
            var dealer = await _userRepository.GetDealerAsync(@event.DealerId);
            if (dealer != null)
            {
                await _notificationService.SendAsync(new NotificationRequest
                {
                    UserId = dealer.UserId,
                    Type = "VehicleCreated",
                    Title = "Vehículo Publicado",
                    Body = $"Tu {event.Make} {event.Model} {event.Year} ha sido publicado.",
                    Channels = new[] { "push", "email" },
                    Data = new { VehicleId = @event.VehicleId }
                });
            }

            // Notificar a usuarios con alertas coincidentes
            await NotifyMatchingAlertsAsync(@event);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing VehicleCreatedEvent for {VehicleId}", @event.VehicleId);
            throw; // Will trigger retry
        }
    }

    private async Task NotifyMatchingAlertsAsync(VehicleCreatedEvent @event)
    {
        // Buscar alertas que coincidan con el vehículo
        // Enviar notificaciones a usuarios interesados
    }
}
```

### 5.2 Analytics Consumer

```csharp
public class AnalyticsEventConsumer :
    IConsumer<VehicleCreatedEvent>,
    IConsumer<VehicleSoldEvent>,
    IConsumer<LeadCreatedEvent>,
    IConsumer<PaymentSuccessEvent>
{
    private readonly IEventTrackingService _trackingService;

    public async Task Consume(ConsumeContext<VehicleCreatedEvent> context)
    {
        await _trackingService.TrackEventAsync(new AnalyticsEvent
        {
            EventType = "vehicle.created",
            EntityId = context.Message.VehicleId.ToString(),
            DealerId = context.Message.DealerId.ToString(),
            Properties = new Dictionary<string, object>
            {
                ["make"] = context.Message.Make,
                ["model"] = context.Message.Model,
                ["year"] = context.Message.Year,
                ["price"] = context.Message.Price
            },
            Timestamp = context.Message.CreatedAt
        });
    }

    public async Task Consume(ConsumeContext<VehicleSoldEvent> context)
    {
        await _trackingService.TrackEventAsync(new AnalyticsEvent
        {
            EventType = "vehicle.sold",
            EntityId = context.Message.VehicleId.ToString(),
            DealerId = context.Message.DealerId.ToString(),
            Properties = new Dictionary<string, object>
            {
                ["sale_price"] = context.Message.SalePrice,
                ["buyer_id"] = context.Message.BuyerId
            },
            Timestamp = context.Message.SoldAt
        });
    }

    // Similar para otros eventos...
}
```

---

## 6. Dead Letter Queue

### 6.1 Configuración DLQ

```csharp
cfg.ReceiveEndpoint("vehicle-created", e =>
{
    e.ConfigureConsumer<VehicleCreatedConsumer>(context);

    // Dead letter exchange
    e.BindDeadLetterQueue("dlx", "vehicle-created-dlq");

    // Después de N reintentos, va al DLQ
    e.UseMessageRetry(r => r.Intervals(
        TimeSpan.FromSeconds(10),
        TimeSpan.FromSeconds(30),
        TimeSpan.FromMinutes(1),
        TimeSpan.FromMinutes(5)));
});
```

### 6.2 DLQ Consumer para Análisis

```csharp
public class DeadLetterConsumer : IConsumer<Fault<VehicleCreatedEvent>>
{
    private readonly IErrorService _errorService;
    private readonly ILogger<DeadLetterConsumer> _logger;

    public async Task Consume(ConsumeContext<Fault<VehicleCreatedEvent>> context)
    {
        _logger.LogError("Message sent to DLQ. Exceptions: {Exceptions}",
            string.Join(", ", context.Message.Exceptions.Select(e => e.Message)));

        await _errorService.LogDeadLetterAsync(new DeadLetterLog
        {
            MessageId = context.Message.FaultId,
            MessageType = typeof(VehicleCreatedEvent).Name,
            OriginalMessage = JsonSerializer.Serialize(context.Message.Message),
            Exceptions = context.Message.Exceptions.Select(e => e.Message).ToList(),
            FailedAt = context.Message.Timestamp,
            Queue = "vehicle-created-dlq"
        });
    }
}
```

### 6.3 Reprocessing de DLQ

```csharp
public class DeadLetterReprocessor : IDeadLetterReprocessor
{
    private readonly IConnection _connection;
    private readonly IPublishEndpoint _publishEndpoint;

    public async Task ReprocessAsync(string queueName, int maxMessages = 100)
    {
        using var channel = _connection.CreateModel();
        var count = 0;

        while (count < maxMessages)
        {
            var result = channel.BasicGet($"{queueName}-dlq", autoAck: false);
            if (result == null) break;

            try
            {
                var message = DeserializeMessage(result.Body.ToArray(), result.BasicProperties);
                await _publishEndpoint.Publish(message);

                channel.BasicAck(result.DeliveryTag, false);
                count++;
            }
            catch (Exception ex)
            {
                // Dejar en DLQ si falla
                channel.BasicNack(result.DeliveryTag, false, true);
                throw;
            }
        }
    }
}
```

---

## 7. Exchanges y Routing

### 7.1 Exchange Types

| Exchange                | Tipo   | Routing Key                               | Uso                      |
| ----------------------- | ------ | ----------------------------------------- | ------------------------ |
| `vehicle.events`        | Topic  | `vehicle.created`, `vehicle.updated`      | Eventos de vehículos     |
| `user.events`           | Topic  | `user.registered`, `user.verified`        | Eventos de usuarios      |
| `billing.events`        | Topic  | `payment.success`, `subscription.created` | Eventos de pagos         |
| `notification.commands` | Direct | `send.email`, `send.sms`, `send.push`     | Comandos de notificación |
| `dlx`                   | Fanout | N/A                                       | Dead letter exchange     |

### 7.2 Routing Keys

```
vehicle.created
vehicle.updated
vehicle.deleted
vehicle.sold
vehicle.status.changed

user.registered
user.verified
user.login

billing.payment.success
billing.payment.failed
billing.subscription.created
billing.subscription.cancelled

lead.created
lead.status.changed
```

---

## 8. Monitoreo

### 8.1 RabbitMQ Management UI

```
http://rabbitmq.okla.internal:15672

Dashboards:
- Overview: Conexiones, canales, queues
- Queues: Mensajes pendientes, consumers
- Exchanges: Routing, bindings
- Connections: Clientes conectados
```

### 8.2 Prometheus Metrics

```
# Mensajes publicados
rabbitmq_published_total{exchange="...", routing_key="..."}

# Mensajes consumidos
rabbitmq_consumed_total{queue="...", consumer="..."}

# Mensajes en cola
rabbitmq_queue_messages{queue="...", state="ready|unacked"}

# Mensajes en DLQ
rabbitmq_dlq_messages_total{queue="..."}

# Consumer lag
rabbitmq_consumer_lag{queue="...", consumer="..."}
```

### 8.3 Alertas

```yaml
groups:
  - name: rabbitmq-alerts
    rules:
      - alert: QueueBacklog
        expr: rabbitmq_queue_messages{state="ready"} > 10000
        for: 5m
        labels:
          severity: warning
        annotations:
          summary: "Queue {{ $labels.queue }} has high backlog"

      - alert: DLQNotEmpty
        expr: rabbitmq_dlq_messages_total > 0
        for: 5m
        labels:
          severity: warning
        annotations:
          summary: "Dead letter queue has {{ $value }} messages"

      - alert: NoConsumers
        expr: rabbitmq_queue_consumers == 0 and rabbitmq_queue_messages > 0
        for: 5m
        labels:
          severity: critical
        annotations:
          summary: "Queue {{ $labels.queue }} has no consumers"
```

---

## 9. Kubernetes Deployment

### 9.1 RabbitMQ StatefulSet

```yaml
apiVersion: apps/v1
kind: StatefulSet
metadata:
  name: rabbitmq
  namespace: okla
spec:
  serviceName: rabbitmq
  replicas: 1
  selector:
    matchLabels:
      app: rabbitmq
  template:
    metadata:
      labels:
        app: rabbitmq
    spec:
      containers:
        - name: rabbitmq
          image: rabbitmq:3.12-management-alpine
          ports:
            - containerPort: 5672
              name: amqp
            - containerPort: 15672
              name: management
          env:
            - name: RABBITMQ_DEFAULT_USER
              valueFrom:
                secretKeyRef:
                  name: rabbitmq-secrets
                  key: username
            - name: RABBITMQ_DEFAULT_PASS
              valueFrom:
                secretKeyRef:
                  name: rabbitmq-secrets
                  key: password
            - name: RABBITMQ_DEFAULT_VHOST
              value: "/okla"
          volumeMounts:
            - name: rabbitmq-data
              mountPath: /var/lib/rabbitmq
  volumeClaimTemplates:
    - metadata:
        name: rabbitmq-data
      spec:
        accessModes: ["ReadWriteOnce"]
        resources:
          requests:
            storage: 10Gi
```

---

## 10. Best Practices

### 10.1 Idempotencia

```csharp
public class IdempotentConsumer<TMessage> : IConsumer<TMessage> where TMessage : class
{
    private readonly IDistributedCache _cache;
    private readonly IConsumer<TMessage> _innerConsumer;

    public async Task Consume(ConsumeContext<TMessage> context)
    {
        var messageId = context.MessageId?.ToString() ?? Guid.NewGuid().ToString();
        var cacheKey = $"processed:{typeof(TMessage).Name}:{messageId}";

        // Check if already processed
        var exists = await _cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(exists))
        {
            return; // Already processed, skip
        }

        await _innerConsumer.Consume(context);

        // Mark as processed with TTL
        await _cache.SetStringAsync(cacheKey, "1", new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
        });
    }
}
```

### 10.2 Ordering Garantizado

```csharp
// Para eventos que requieren orden, usar partitioning por key
cfg.ReceiveEndpoint("vehicle-events", e =>
{
    e.ConfigureConsumer<VehicleEventConsumer>(context);

    // Mensajes del mismo dealer van al mismo consumer
    e.UsePartitioner(8, context =>
        context.Message is IVehicleEvent ve ? ve.DealerId : default);
});
```

---

## 📚 Referencias

- [MassTransit Documentation](https://masstransit.io/documentation) - Documentación oficial
- [RabbitMQ Documentation](https://www.rabbitmq.com/documentation.html) - Documentación RabbitMQ
- [ErrorService Dead Letter](../backend/ErrorService/DEAD_LETTER_QUEUE_IMPLEMENTATION.md) - Implementación DLQ
