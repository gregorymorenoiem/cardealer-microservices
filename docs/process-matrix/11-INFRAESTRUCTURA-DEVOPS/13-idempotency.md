# 🔐 Idempotency Service - Idempotencia - Matriz de Procesos

> **Servicio:** SharedLibrary / Middleware  
> **Ubicación:** CarDealer.Shared.Idempotency  
> **Última actualización:** Enero 25, 2026  
> **Estado de Implementación:** ✅ 100% Backend | N/A UI

---

## ⚠️ AUDITORÍA DE ACCESO UI (Enero 25, 2026)

| Proceso          | Backend | UI Access | Observación              |
| ---------------- | ------- | --------- | ------------------------ |
| Idempotency Keys | ✅ 100% | N/A       | Header X-Idempotency-Key |
| Key Storage      | ✅ 100% | N/A       | Redis TTL 24h            |
| Response Caching | ✅ 100% | N/A       | Respuestas cacheadas     |
| Webhook Dedup    | ✅ 100% | N/A       | Stripe/Azul webhooks     |

### Rutas UI Existentes ✅

- N/A - Funcionalidad transparente de backend
- El frontend envía header X-Idempotency-Key automáticamente

### Rutas UI Faltantes 🔴

- Ninguna requerida - es infraestructura transparente

**Verificación Backend:** `CarDealer.Shared` implementa IdempotencyMiddleware ✅

---

## 📊 Resumen de Implementación

| Componente                        | Total | Implementado | Pendiente | Estado  |
| --------------------------------- | ----- | ------------ | --------- | ------- |
| **IDEMP-HEADER-\*** (Headers)     | 3     | 3            | 0         | ✅ 100% |
| **IDEMP-STORE-\*** (Storage)      | 4     | 4            | 0         | ✅ 100% |
| **IDEMP-CHECK-\*** (Verificación) | 3     | 3            | 0         | ✅ 100% |
| **IDEMP-WEBHOOK-\*** (Webhooks)   | 3     | 3            | 0         | ✅ 100% |
| **IDEMP-CLEANUP-\*** (Limpieza)   | 2     | 2            | 0         | ✅ 100% |
| **Tests**                         | 15    | 15           | 0         | ✅ 100% |
| **TOTAL**                         | 30    | 30           | 0         | ✅ 100% |

---

## 1. Información General

### 1.1 Descripción

Sistema de idempotencia para garantizar que operaciones críticas (pagos, creación de recursos, etc.) puedan ser reintentadas de forma segura sin efectos duplicados. Utiliza Redis para almacenar claves de idempotencia con TTL configurable.

### 1.2 Casos de Uso

| Operación               | Riesgo sin Idempotencia | Solución                  |
| ----------------------- | ----------------------- | ------------------------- |
| **Procesar pago**       | Cobro duplicado         | Idempotency-Key en header |
| **Crear listing**       | Vehículos duplicados    | Hash de contenido         |
| **Enviar notificación** | Spam de mensajes        | Event deduplication       |
| **Webhook processing**  | Acciones duplicadas     | Webhook ID check          |

### 1.3 Arquitectura

```
┌─────────────────────────────────────────────────────────────────────────┐
│                      Idempotency Architecture                            │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│   Client Request                                                        │
│   Headers: Idempotency-Key: "abc-123-xyz"                               │
│        │                                                                │
│        ▼                                                                │
│   ┌─────────────────────────────────────────────────────────────────┐   │
│   │                    API Gateway                                   │   │
│   │                                                                   │   │
│   │   ┌─────────────────────────────────────────────────────────┐   │   │
│   │   │              IdempotencyMiddleware                       │   │   │
│   │   │                                                          │   │   │
│   │   │   1. Extract Idempotency-Key from headers                │   │   │
│   │   │   2. Check Redis: idempotency:{key}                      │   │   │
│   │   │                                                          │   │   │
│   │   │   IF key exists:                                         │   │   │
│   │   │     3a. Return cached response (200 OK)                  │   │   │
│   │   │     3b. Include X-Idempotent-Replayed: true              │   │   │
│   │   │                                                          │   │   │
│   │   │   IF key not exists:                                     │   │   │
│   │   │     3c. Acquire lock: idempotency:{key}:lock             │   │   │
│   │   │     4. Continue to controller                            │   │   │
│   │   │     5. Store response in Redis                           │   │   │
│   │   │     6. Release lock                                      │   │   │
│   │   └─────────────────────────────────────────────────────────┘   │   │
│   └───────────────────────────────┬─────────────────────────────────┘   │
│                                   │                                      │
│                                   ▼                                      │
│   ┌─────────────────────────────────────────────────────────────────┐   │
│   │                          Redis                                   │   │
│   │                                                                   │   │
│   │   idempotency:{key} = {                                         │   │
│   │     "statusCode": 201,                                          │   │
│   │     "headers": {...},                                           │   │
│   │     "body": "{...}",                                            │   │
│   │     "createdAt": "2026-01-21T10:30:00Z",                        │   │
│   │     "requestHash": "sha256:..."                                 │   │
│   │   }                                                              │   │
│   │                                                                   │   │
│   │   TTL: 24 hours (configurable per operation)                    │   │
│   └─────────────────────────────────────────────────────────────────┘   │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 2. Implementación

### 2.1 Middleware de Idempotencia

```csharp
public class IdempotencyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IDistributedCache _cache;
    private readonly ILogger<IdempotencyMiddleware> _logger;
    private readonly IdempotencyOptions _options;

    public async Task InvokeAsync(HttpContext context)
    {
        // Only apply to POST, PUT, DELETE (state-changing operations)
        if (!IsIdempotentMethod(context.Request.Method))
        {
            await _next(context);
            return;
        }

        // Check for Idempotency-Key header
        var idempotencyKey = context.Request.Headers["Idempotency-Key"].FirstOrDefault();

        if (string.IsNullOrEmpty(idempotencyKey))
        {
            // Some endpoints require idempotency key
            if (RequiresIdempotencyKey(context.Request.Path))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "IDEMPOTENCY_KEY_REQUIRED",
                    message = "Idempotency-Key header is required for this operation"
                });
                return;
            }

            await _next(context);
            return;
        }

        var cacheKey = $"idempotency:{idempotencyKey}";
        var lockKey = $"{cacheKey}:lock";

        // 1. Check if response already cached
        var cachedResponse = await _cache.GetStringAsync(cacheKey);

        if (!string.IsNullOrEmpty(cachedResponse))
        {
            var stored = JsonSerializer.Deserialize<StoredResponse>(cachedResponse)!;

            // Verify request hash matches (prevent key reuse for different requests)
            var currentHash = await ComputeRequestHashAsync(context.Request);

            if (stored.RequestHash != currentHash)
            {
                context.Response.StatusCode = 422;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "IDEMPOTENCY_KEY_REUSED",
                    message = "Idempotency key was already used for a different request"
                });
                return;
            }

            // Return cached response
            _logger.LogInformation("Returning idempotent response for key {Key}", idempotencyKey);

            context.Response.StatusCode = stored.StatusCode;
            context.Response.Headers["X-Idempotent-Replayed"] = "true";

            foreach (var header in stored.Headers)
            {
                context.Response.Headers[header.Key] = header.Value;
            }

            await context.Response.WriteAsync(stored.Body);
            return;
        }

        // 2. Acquire distributed lock
        var lockAcquired = await TryAcquireLockAsync(lockKey, _options.LockTimeout);

        if (!lockAcquired)
        {
            // Request in progress, ask client to retry
            context.Response.StatusCode = 409;
            context.Response.Headers["Retry-After"] = "1";
            await context.Response.WriteAsJsonAsync(new
            {
                error = "IDEMPOTENCY_CONFLICT",
                message = "A request with this idempotency key is already in progress"
            });
            return;
        }

        try
        {
            // 3. Capture response
            var originalBody = context.Response.Body;
            using var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            await _next(context);

            // 4. Store response in cache
            memoryStream.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(memoryStream).ReadToEndAsync();

            var storedResponse = new StoredResponse
            {
                StatusCode = context.Response.StatusCode,
                Headers = context.Response.Headers
                    .Where(h => !h.Key.StartsWith("X-"))
                    .ToDictionary(h => h.Key, h => h.Value.ToString()),
                Body = responseBody,
                RequestHash = await ComputeRequestHashAsync(context.Request),
                CreatedAt = DateTime.UtcNow
            };

            var ttl = GetTtlForPath(context.Request.Path);

            await _cache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(storedResponse),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = ttl
                });

            // 5. Write response to client
            memoryStream.Seek(0, SeekOrigin.Begin);
            await memoryStream.CopyToAsync(originalBody);
            context.Response.Body = originalBody;
        }
        finally
        {
            await ReleaseLockAsync(lockKey);
        }
    }

    private async Task<string> ComputeRequestHashAsync(HttpRequest request)
    {
        // Hash of: method + path + body
        request.EnableBuffering();
        request.Body.Position = 0;

        using var reader = new StreamReader(request.Body, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        request.Body.Position = 0;

        var input = $"{request.Method}:{request.Path}:{body}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));

        return $"sha256:{Convert.ToBase64String(hash)}";
    }

    private TimeSpan GetTtlForPath(PathString path)
    {
        // Different TTLs for different operations
        if (path.StartsWithSegments("/api/billing/payments"))
            return TimeSpan.FromHours(48); // Payments: 48h

        if (path.StartsWithSegments("/api/vehicles"))
            return TimeSpan.FromHours(24); // Vehicles: 24h

        return TimeSpan.FromHours(1); // Default: 1h
    }
}

public class StoredResponse
{
    public int StatusCode { get; set; }
    public Dictionary<string, string> Headers { get; set; } = new();
    public string Body { get; set; } = string.Empty;
    public string RequestHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
```

### 2.2 Attribute para Controllers

```csharp
[AttributeUsage(AttributeTargets.Method)]
public class IdempotentAttribute : Attribute
{
    public bool Required { get; set; } = false;
    public int TtlHours { get; set; } = 24;
}

// Uso en controller
[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    [HttpPost]
    [Idempotent(Required = true, TtlHours = 48)]
    public async Task<ActionResult<PaymentResult>> ProcessPayment(
        [FromBody] PaymentRequest request)
    {
        // La idempotencia se maneja automáticamente por el middleware
        var result = await _paymentService.ProcessAsync(request);
        return CreatedAtAction(nameof(GetPayment), new { id = result.Id }, result);
    }
}
```

### 2.3 Event Deduplication (para RabbitMQ)

```csharp
public class IdempotentConsumer<TMessage> : IConsumer<TMessage>
    where TMessage : class, IMessage
{
    private readonly IConsumer<TMessage> _inner;
    private readonly IDistributedCache _cache;

    public async Task Consume(ConsumeContext<TMessage> context)
    {
        var messageId = context.MessageId?.ToString()
            ?? context.Message.GetHashCode().ToString();

        var cacheKey = $"event:processed:{typeof(TMessage).Name}:{messageId}";

        // Check if already processed
        var processed = await _cache.GetStringAsync(cacheKey);

        if (!string.IsNullOrEmpty(processed))
        {
            _logger.LogInformation(
                "Skipping duplicate message {MessageId} of type {Type}",
                messageId, typeof(TMessage).Name);
            return;
        }

        // Process message
        await _inner.Consume(context);

        // Mark as processed (TTL: 7 days)
        await _cache.SetStringAsync(
            cacheKey,
            DateTime.UtcNow.ToString("O"),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7)
            });
    }
}
```

### 2.4 Webhook Deduplication

```csharp
public class WebhookIdempotencyService
{
    private readonly IDistributedCache _cache;

    public async Task<bool> IsWebhookProcessedAsync(string webhookId, CancellationToken ct)
    {
        var key = $"webhook:processed:{webhookId}";
        var value = await _cache.GetStringAsync(key, ct);
        return !string.IsNullOrEmpty(value);
    }

    public async Task MarkWebhookProcessedAsync(
        string webhookId,
        string result,
        CancellationToken ct)
    {
        var key = $"webhook:processed:{webhookId}";

        await _cache.SetStringAsync(
            key,
            JsonSerializer.Serialize(new
            {
                ProcessedAt = DateTime.UtcNow,
                Result = result
            }),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30)
            },
            ct);
    }
}

// Uso en webhook controller
[HttpPost("stripe/webhook")]
public async Task<IActionResult> HandleStripeWebhook()
{
    var payload = await new StreamReader(Request.Body).ReadToEndAsync();
    var signature = Request.Headers["Stripe-Signature"];

    var stripeEvent = EventUtility.ConstructEvent(
        payload, signature, _webhookSecret);

    // Check idempotency
    if (await _idempotency.IsWebhookProcessedAsync(stripeEvent.Id, HttpContext.RequestAborted))
    {
        _logger.LogInformation("Webhook {Id} already processed", stripeEvent.Id);
        return Ok(); // Acknowledge duplicate
    }

    // Process webhook
    var result = await _webhookProcessor.ProcessAsync(stripeEvent);

    // Mark as processed
    await _idempotency.MarkWebhookProcessedAsync(
        stripeEvent.Id,
        result,
        HttpContext.RequestAborted);

    return Ok();
}
```

---

## 3. Flujo de Request

```
┌─────────────────────────────────────────────────────────────────────────┐
│                     Idempotency Request Flow                             │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│   First Request                                                         │
│   ─────────────                                                         │
│   POST /api/payments                                                    │
│   Idempotency-Key: pay_abc123                                           │
│        │                                                                │
│        ▼                                                                │
│   1. Check Redis: idempotency:pay_abc123 → NOT FOUND                    │
│        │                                                                │
│        ▼                                                                │
│   2. Acquire lock: idempotency:pay_abc123:lock → SUCCESS                │
│        │                                                                │
│        ▼                                                                │
│   3. Process payment → Stripe charge → Success                          │
│        │                                                                │
│        ▼                                                                │
│   4. Store in Redis: {status: 201, body: {...}}                         │
│        │                                                                │
│        ▼                                                                │
│   5. Release lock                                                       │
│        │                                                                │
│        ▼                                                                │
│   Response: 201 Created, {paymentId: "pi_xxx"}                          │
│                                                                          │
│   ═══════════════════════════════════════════════════════════════════   │
│                                                                          │
│   Retry Request (network failure, timeout)                              │
│   ──────────────                                                        │
│   POST /api/payments                                                    │
│   Idempotency-Key: pay_abc123                                           │
│        │                                                                │
│        ▼                                                                │
│   1. Check Redis: idempotency:pay_abc123 → FOUND                        │
│        │                                                                │
│        ▼                                                                │
│   2. Verify request hash matches → MATCH                                │
│        │                                                                │
│        ▼                                                                │
│   Response: 201 Created (from cache)                                    │
│   X-Idempotent-Replayed: true                                           │
│   {paymentId: "pi_xxx"} ← Same response, NO duplicate charge            │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 4. Endpoints que Requieren Idempotencia

| Servicio            | Endpoint                       | Required    | TTL |
| ------------------- | ------------------------------ | ----------- | --- |
| BillingService      | `POST /api/payments`           | ✅          | 48h |
| BillingService      | `POST /api/subscriptions`      | ✅          | 48h |
| VehiclesSaleService | `POST /api/vehicles`           | ⚠️ Opcional | 24h |
| DealerManagement    | `POST /api/dealers`            | ✅          | 24h |
| NotificationService | `POST /api/notifications/send` | ⚠️ Opcional | 1h  |
| LeadService         | `POST /api/leads`              | ⚠️ Opcional | 6h  |

---

## 5. Reglas de Negocio

| Código   | Regla                             | Validación         |
| -------- | --------------------------------- | ------------------ |
| IDEM-R01 | Key max 255 caracteres            | Length check       |
| IDEM-R02 | Key format: prefijo_uuid          | Regex validation   |
| IDEM-R03 | Mismo key, diferente body = 422   | Hash comparison    |
| IDEM-R04 | Lock timeout: 30 segundos         | Redis SETNX        |
| IDEM-R05 | Response cache max 1MB            | Size check         |
| IDEM-R06 | Pagos DEBEN tener idempotency key | Required attribute |

---

## 6. Códigos de Error

| Código     | HTTP | Mensaje                        | Causa                       |
| ---------- | ---- | ------------------------------ | --------------------------- |
| `IDEM_001` | 400  | Idempotency-Key required       | Falta header                |
| `IDEM_002` | 422  | Idempotency key reused         | Key usada para otro request |
| `IDEM_003` | 409  | Request in progress            | Lock held                   |
| `IDEM_004` | 400  | Invalid idempotency key format | Formato incorrecto          |
| `IDEM_005` | 500  | Failed to store response       | Redis error                 |

---

## 7. Configuración

```json
{
  "Idempotency": {
    "Enabled": true,
    "DefaultTtlHours": 24,
    "LockTimeoutSeconds": 30,
    "MaxResponseSizeBytes": 1048576,
    "RequiredPaths": [
      "/api/billing/payments",
      "/api/billing/subscriptions",
      "/api/dealers"
    ],
    "KeyPattern": "^[a-zA-Z]+_[a-f0-9\\-]+$",
    "RedisKeyPrefix": "idempotency:"
  }
}
```

---

## 8. Métricas Prometheus

```
# Idempotency cache
idempotency_cache_hits_total
idempotency_cache_misses_total
idempotency_cache_hit_ratio

# Conflicts
idempotency_conflicts_total{type="lock|hash_mismatch"}

# Storage
idempotency_stored_responses_total
idempotency_response_size_bytes
```

---

## 9. Cliente JavaScript

```typescript
// API Client con idempotency key automático
class OklaApiClient {
  async processPayment(payment: PaymentRequest): Promise<PaymentResult> {
    const idempotencyKey = `pay_${uuidv4()}`;

    try {
      return await this.post("/api/payments", payment, {
        headers: {
          "Idempotency-Key": idempotencyKey,
        },
      });
    } catch (error) {
      if (error.status === 409) {
        // Request in progress, retry after delay
        await sleep(1000);
        return this.processPayment(payment);
      }
      throw error;
    }
  }
}

// Guardar key para posibles reintentos
function createPayment(payment: PaymentRequest) {
  const idempotencyKey = `pay_${uuidv4()}`;

  // Guardar en localStorage por si la página se recarga
  localStorage.setItem("pending_payment_key", idempotencyKey);

  return api.post("/api/payments", payment, {
    headers: { "Idempotency-Key": idempotencyKey },
  });
}
```

---

## 📚 Referencias

- [01-billing-service.md](../05-PAGOS-FACTURACION/01-billing-service.md) - Servicio de pagos
- [03-webhooks.md](../05-PAGOS-FACTURACION/03-webhooks.md) - Webhooks de pago
- [01-gateway-service.md](01-gateway-service.md) - API Gateway
