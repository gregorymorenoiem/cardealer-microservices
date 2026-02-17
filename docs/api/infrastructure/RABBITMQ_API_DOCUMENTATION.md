# 🐰 API RabbitMQ - Message Broker

**Proveedor:** RabbitMQ (Open Source)  
**Documentación oficial:** https://www.rabbitmq.com/docs  
**Versión:** 3.12+  
**Actualizado:** Enero 2026

---

## 📋 Tabla de Contenidos

1. [Introducción](#introducción)
2. [Configuración](#configuración)
3. [Conceptos clave](#conceptos-clave)
4. [Exchanges y queues](#exchanges-y-queues)
5. [Publicar mensajes](#publicar-mensajes)
6. [Consumir mensajes](#consumir-mensajes)
7. [Patrones de mensajería](#patrones-de-mensajería)
8. [Manejo de errores](#manejo-de-errores)
9. [Ejemplos de código](#ejemplos-de-código)

---

## 🎯 Introducción

RabbitMQ es el message broker usado en OKLA para:

- Comunicación asíncrona entre microservicios
- Event-driven architecture
- Procesamiento en background
- Desacoplamiento de servicios

### Casos de Uso en OKLA

| Servicio                   | Usa RabbitMQ Para                                         |
| -------------------------- | --------------------------------------------------------- |
| **VehiclesSaleService**    | Publicar eventos de vehículos (created, updated, deleted) |
| **MediaService**           | Jobs de procesamiento de imágenes                         |
| **NotificationService**    | Cola de emails, SMS, push notifications                   |
| **ErrorService**           | Recibir errores de todos los servicios                    |
| **BillingService**         | Webhooks de Stripe/AZUL                                   |
| **DealerAnalyticsService** | Eventos de tracking para analytics                        |

---

## 🔧 Configuración

### Conexión en Docker Compose

```yaml
rabbitmq:
  image: rabbitmq:3.12-management
  container_name: rabbitmq
  ports:
    - "5672:5672" # AMQP protocol
    - "15672:15672" # Management UI
  environment:
    RABBITMQ_DEFAULT_USER: guest
    RABBITMQ_DEFAULT_PASS: guest
  volumes:
    - rabbitmq_data:/var/lib/rabbitmq
  healthcheck:
    test: ["CMD", "rabbitmqctl", "status"]
    interval: 10s
    timeout: 5s
    retries: 5
```

### Management UI

- **URL:** http://localhost:15672
- **User:** guest
- **Pass:** guest

---

## 📚 Conceptos Clave

### 1. Producer (Publicador)

Servicio que **envía mensajes** a un exchange.

### 2. Exchange

Recibe mensajes de producers y los rutea a queues basado en reglas.

**Tipos de Exchange:**

- **Direct:** Ruteo por routing key exacta
- **Topic:** Ruteo por patrón (ej: vehicle.\*.created)
- **Fanout:** Broadcast a todas las queues
- **Headers:** Ruteo por headers del mensaje

### 3. Queue (Cola)

Almacena mensajes hasta que un consumer los procesa.

### 4. Consumer (Consumidor)

Servicio que **recibe y procesa mensajes** de una queue.

### 5. Binding

Conexión entre Exchange y Queue con reglas de ruteo.

### 6. Routing Key

String que el exchange usa para decidir a qué queue enviar el mensaje.

---

## 🔀 Exchanges y Queues en OKLA

### Exchange: `vehicles.events` (Topic)

Publica eventos de vehículos.

**Queues vinculadas:**

- `vehicles.created.queue` → `vehicle.created`
- `vehicles.updated.queue` → `vehicle.updated`
- `vehicles.deleted.queue` → `vehicle.deleted`
- `analytics.all.queue` → `vehicle.*` (todas)

### Exchange: `media.jobs` (Direct)

Jobs de procesamiento de imágenes.

**Queues vinculadas:**

- `media.thumbnail.queue` → `thumbnail`
- `media.resize.queue` → `resize`
- `media.watermark.queue` → `watermark`

### Exchange: `notifications.events` (Direct)

Notificaciones a enviar.

**Queues vinculadas:**

- `notifications.email.queue` → `email`
- `notifications.sms.queue` → `sms`
- `notifications.push.queue` → `push`

### Exchange: `errors.events` (Fanout)

Errores centralizados (broadcast a todos los consumers).

**Queues vinculadas:**

- `errors.storage.queue`
- `errors.logging.queue`
- `errors.monitoring.queue`

### Exchange: `billing.events` (Topic)

Eventos de pagos y suscripciones.

**Queues vinculadas:**

- `billing.payment.success.queue` → `payment.success`
- `billing.payment.failed.queue` → `payment.failed`
- `billing.subscription.updated.queue` → `subscription.*`

---

## 📤 Publicar Mensajes

### Instalación del Cliente

```bash
dotnet add package RabbitMQ.Client --version 6.8.1
```

### Ejemplo: Publicar Evento de Vehículo Creado

```csharp
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

public class VehicleEventPublisher : IVehicleEventPublisher
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<VehicleEventPublisher> _logger;

    public VehicleEventPublisher(
        IConnectionFactory connectionFactory,
        ILogger<VehicleEventPublisher> logger)
    {
        _connection = connectionFactory.CreateConnection();
        _channel = _connection.CreateModel();
        _logger = logger;

        // Declarar exchange (idempotente)
        _channel.ExchangeDeclare(
            exchange: "vehicles.events",
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false
        );
    }

    public async Task PublishVehicleCreatedAsync(VehicleCreatedEvent @event)
    {
        try
        {
            var message = JsonSerializer.Serialize(@event);
            var body = Encoding.UTF8.GetBytes(message);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true; // Sobrevive restart de RabbitMQ
            properties.ContentType = "application/json";
            properties.MessageId = Guid.NewGuid().ToString();
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            _channel.BasicPublish(
                exchange: "vehicles.events",
                routingKey: "vehicle.created",
                basicProperties: properties,
                body: body
            );

            _logger.LogInformation($"Published vehicle.created event: {@event.VehicleId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing vehicle.created event");
            throw;
        }
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }
}

// Evento
public record VehicleCreatedEvent(
    Guid VehicleId,
    string Make,
    string Model,
    int Year,
    decimal Price,
    Guid UserId,
    DateTime CreatedAt
);
```

### Configuración en Program.cs

```csharp
// RabbitMQ Connection Factory
builder.Services.AddSingleton<IConnectionFactory>(sp =>
{
    return new ConnectionFactory
    {
        HostName = builder.Configuration["RabbitMQ:HostName"] ?? "localhost",
        Port = int.Parse(builder.Configuration["RabbitMQ:Port"] ?? "5672"),
        UserName = builder.Configuration["RabbitMQ:UserName"] ?? "guest",
        Password = builder.Configuration["RabbitMQ:Password"] ?? "guest",
        VirtualHost = "/",
        AutomaticRecoveryEnabled = true,
        NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
    };
});

builder.Services.AddSingleton<IVehicleEventPublisher, VehicleEventPublisher>();
```

---

## 📥 Consumir Mensajes

### Ejemplo: Consumer de Vehículos Creados

```csharp
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

public class VehicleCreatedConsumer : BackgroundService
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<VehicleCreatedConsumer> _logger;
    private readonly IServiceProvider _serviceProvider;

    public VehicleCreatedConsumer(
        IConnectionFactory connectionFactory,
        ILogger<VehicleCreatedConsumer> logger,
        IServiceProvider serviceProvider)
    {
        _connection = connectionFactory.CreateConnection();
        _channel = _connection.CreateModel();
        _logger = logger;
        _serviceProvider = serviceProvider;

        // Declarar exchange y queue
        _channel.ExchangeDeclare(
            exchange: "vehicles.events",
            type: ExchangeType.Topic,
            durable: true
        );

        _channel.QueueDeclare(
            queue: "analytics.vehicles.created.queue",
            durable: true,
            exclusive: false,
            autoDelete: false
        );

        _channel.QueueBind(
            queue: "analytics.vehicles.created.queue",
            exchange: "vehicles.events",
            routingKey: "vehicle.created"
        );

        // QoS: Procesar 1 mensaje a la vez
        _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            try
            {
                var @event = JsonSerializer.Deserialize<VehicleCreatedEvent>(message);

                _logger.LogInformation($"Processing vehicle.created event: {@event.VehicleId}");

                // Procesar evento (con scoped services)
                using var scope = _serviceProvider.CreateScope();
                var analyticsService = scope.ServiceProvider.GetRequiredService<IAnalyticsService>();
                await analyticsService.TrackVehicleCreatedAsync(@event);

                // ACK (confirmar procesamiento exitoso)
                _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);

                _logger.LogInformation($"Processed vehicle.created event: {@event.VehicleId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing vehicle.created event");

                // NACK (rechazar y reencolar)
                _channel.BasicNack(
                    deliveryTag: ea.DeliveryTag,
                    multiple: false,
                    requeue: true // Reintentar
                );
            }
        };

        _channel.BasicConsume(
            queue: "analytics.vehicles.created.queue",
            autoAck: false, // Manual ACK
            consumer: consumer
        );

        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}
```

### Registrar Consumer en Program.cs

```csharp
builder.Services.AddHostedService<VehicleCreatedConsumer>();
```

---

## 🔄 Patrones de Mensajería

### 1. Publish/Subscribe (Fanout)

Un mensaje se envía a **múltiples consumers**.

**Ejemplo:** ErrorService publica error, todos los consumers lo procesan (log, storage, monitoring).

```csharp
_channel.ExchangeDeclare("errors.events", ExchangeType.Fanout, durable: true);

// No routing key necesaria
_channel.BasicPublish(
    exchange: "errors.events",
    routingKey: "",
    body: body
);
```

### 2. Work Queue (Direct)

Múltiples workers procesan mensajes de la misma queue (load balancing).

**Ejemplo:** NotificationService tiene 3 workers procesando emails.

```csharp
_channel.QueueDeclare("notifications.email.queue", durable: true, ...);
_channel.BasicQos(prefetchCount: 1); // 1 mensaje por worker
```

### 3. Routing (Direct)

Mensajes se rutean por routing key exacta.

**Ejemplo:** Notificaciones por tipo (email, sms, push).

```csharp
_channel.BasicPublish(
    exchange: "notifications.events",
    routingKey: "email", // o "sms" o "push"
    body: body
);
```

### 4. Topics

Mensajes se rutean por patrón.

**Ejemplo:** `vehicle.*` captura `vehicle.created`, `vehicle.updated`, `vehicle.deleted`.

```csharp
_channel.QueueBind(
    queue: "analytics.all.queue",
    exchange: "vehicles.events",
    routingKey: "vehicle.*" // Wildcard
);
```

---

## ❌ Manejo de Errores

### 1. Dead Letter Queue (DLQ)

Mensajes que fallan después de N reintentos van a DLQ.

```csharp
var args = new Dictionary<string, object>
{
    { "x-dead-letter-exchange", "dlx.exchange" },
    { "x-dead-letter-routing-key", "dlq.vehicle.created" },
    { "x-message-ttl", 3600000 } // 1 hora
};

_channel.QueueDeclare(
    queue: "vehicles.created.queue",
    durable: true,
    arguments: args
);
```

### 2. Retry Policy

Reintentar con backoff exponencial.

```csharp
int retries = 0;
const int maxRetries = 3;

while (retries < maxRetries)
{
    try
    {
        await ProcessMessageAsync(message);
        _channel.BasicAck(ea.DeliveryTag, false);
        break;
    }
    catch (Exception ex)
    {
        retries++;
        if (retries >= maxRetries)
        {
            _logger.LogError("Max retries reached, sending to DLQ");
            _channel.BasicNack(ea.DeliveryTag, false, requeue: false);
        }
        else
        {
            await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retries)));
        }
    }
}
```

### 3. Circuit Breaker

Si un consumer falla mucho, pausar procesamiento temporalmente.

```csharp
// Usar Polly library
var policy = Policy
    .Handle<Exception>()
    .CircuitBreakerAsync(
        exceptionsAllowedBeforeBreaking: 5,
        durationOfBreak: TimeSpan.FromMinutes(1)
    );

await policy.ExecuteAsync(async () =>
{
    await ProcessMessageAsync(message);
});
```

---

## 📊 Monitoreo

### Management UI Metrics

- **Queues:** Mensajes pending, rate, consumers
- **Exchanges:** Messages in/out
- **Connections:** Active connections
- **Nodes:** Memory, disk usage

### Alertas Recomendadas

| Métrica              | Threshold | Acción            |
| -------------------- | --------- | ----------------- |
| **Queue depth**      | >1000     | Escalar consumers |
| **Consumer lag**     | >5 min    | Investigar        |
| **Memory usage**     | >80%      | Upgrade RAM       |
| **Unacked messages** | >500      | Revisar consumers |

---

## 🔐 Seguridad

### 1. Credenciales

```bash
# Cambiar password de guest
docker exec rabbitmq rabbitmqctl change_password guest NEW_PASSWORD

# Crear usuario para producción
docker exec rabbitmq rabbitmqctl add_user okla_user strong_password
docker exec rabbitmq rabbitmqctl set_permissions -p / okla_user ".*" ".*" ".*"
```

### 2. TLS/SSL

```yaml
rabbitmq:
  environment:
    RABBITMQ_SSL_CERTFILE: /etc/rabbitmq/cert.pem
    RABBITMQ_SSL_KEYFILE: /etc/rabbitmq/key.pem
    RABBITMQ_SSL_CACERTFILE: /etc/rabbitmq/ca.pem
```

### 3. VHosts (Virtual Hosts)

Aislar environments (dev, staging, prod).

```bash
docker exec rabbitmq rabbitmqctl add_vhost /prod
docker exec rabbitmq rabbitmqctl set_permissions -p /prod okla_user ".*" ".*" ".*"
```

---

## 📚 Referencias

- [RabbitMQ Documentation](https://www.rabbitmq.com/docs)
- [RabbitMQ.Client NuGet](https://www.nuget.org/packages/RabbitMQ.Client/)
- [AMQP Protocol](https://www.amqp.org/)
- [CloudAMQP Guide](https://www.cloudamqp.com/blog/)

---

**Implementado en:** Todos los microservicios  
**Versión:** 3.12  
**Última actualización:** Enero 15, 2026
