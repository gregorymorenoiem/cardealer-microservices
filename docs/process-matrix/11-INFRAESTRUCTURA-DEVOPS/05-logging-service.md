# 📝 Logging Service - Logging Centralizado - Matriz de Procesos

> **Stack:** Serilog + Seq (opcional ELK)  
> **Alternativa:** ELK Stack (Elasticsearch, Logstash, Kibana)  
> **Última actualización:** Enero 25, 2026  
> **Estado de Implementación:** ✅ 100% Backend | 🔴 0% UI (Herramientas externas)

---

## ⚠️ AUDITORÍA DE ACCESO UI (Enero 25, 2026)

| Proceso            | Backend | UI Access | Observación                |
| ------------------ | ------- | --------- | -------------------------- |
| Structured Logging | ✅ 100% | N/A       | Serilog configurado        |
| Log Sinks          | ✅ 100% | N/A       | Console, File, Seq         |
| Log Enrichment     | ✅ 100% | N/A       | CorrelationId, UserId, etc |
| Log Queries        | ✅ 90%  | 🔴 0%     | Seq UI externa             |

### Rutas UI Existentes ✅

- N/A - Logs se acceden vía Seq UI (http://seq:5341) o Grafana
- Los logs se almacenan y son consultables externamente

### Rutas UI Faltantes 🔴

- No se requiere UI interna - se usa Seq/Grafana/Kibana

**Nota:** Logging es 100% funcional. La consulta se hace vía herramientas externas (Seq, Grafana Loki, ELK).

---

## 📊 Resumen de Implementación

| Componente                          | Total | Implementado | Pendiente | Estado       |
| ----------------------------------- | ----- | ------------ | --------- | ------------ |
| **Configuración Serilog**           | 5     | 5            | 0         | ✅ 100%      |
| **LOG-STRUCT-\*** (Estructurado)    | 4     | 4            | 0         | ✅ 100%      |
| **LOG-SINK-\*** (Destinos)          | 4     | 4            | 0         | ✅ 100%      |
| **LOG-ENRICH-\*** (Enriquecimiento) | 3     | 3            | 0         | ✅ 100%      |
| **LOG-QUERY-\*** (Búsquedas)        | 3     | 0            | 3         | 🔴 Pendiente |
| **Tests**                           | 10    | 10           | 0         | ✅ 100%      |
| **TOTAL**                           | 29    | 26           | 3         | 🟢 90%       |

---

## 1. Información General

### 1.1 Descripción

Sistema de logging centralizado para todos los microservicios de OKLA. Captura, almacena, indexa y permite búsqueda de logs estructurados para debugging, auditoría y análisis.

### 1.2 Arquitectura

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    Centralized Logging Architecture                      │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│   Microservices                                                         │
│   ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐                  │
│   │  Auth    │ │ Vehicles │ │ Billing  │ │   ...    │                  │
│   │ Service  │ │ Service  │ │ Service  │ │          │                  │
│   └────┬─────┘ └────┬─────┘ └────┬─────┘ └────┬─────┘                  │
│        │            │            │            │                         │
│        │  Serilog   │  Serilog   │  Serilog   │                        │
│        │            │            │            │                         │
│        └────────────┴────────────┴────────────┘                        │
│                           │                                             │
│                           ▼                                             │
│                  ┌─────────────────┐                                   │
│                  │      Seq        │                                   │
│                  │  (Log Server)   │                                   │
│                  │    :5341        │                                   │
│                  └────────┬────────┘                                   │
│                           │                                             │
│              ┌────────────┼────────────┐                               │
│              ▼            ▼            ▼                               │
│      ┌───────────┐ ┌───────────┐ ┌───────────┐                        │
│      │   Seq UI  │ │  Grafana  │ │  Alerts   │                        │
│      │  :5341    │ │ Dashboard │ │  (Slack)  │                        │
│      └───────────┘ └───────────┘ └───────────┘                        │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

### 1.3 Niveles de Log

| Nivel         | Descripción           | Uso                 |
| ------------- | --------------------- | ------------------- |
| `Verbose`     | Todo detalle          | Solo desarrollo     |
| `Debug`       | Información de debug  | Desarrollo/Staging  |
| `Information` | Eventos normales      | Todos los ambientes |
| `Warning`     | Situaciones inusuales | Todos los ambientes |
| `Error`       | Errores recuperables  | Todos los ambientes |
| `Fatal`       | Errores críticos      | Todos los ambientes |

---

## 2. Configuración de Serilog

### 2.1 Configuración Base

```csharp
// Program.cs
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .Enrich.WithProperty("Service", "VehiclesSaleService")
    .Enrich.WithProperty("Version", "1.0.0")
    .WriteTo.Console(new JsonFormatter())
    .WriteTo.Seq("http://seq:5341", apiKey: seqApiKey)
    .CreateLogger();

builder.Host.UseSerilog();
```

### 2.2 appsettings.json

```json
{
  "Serilog": {
    "Using": ["Serilog.Sinks.Console", "Serilog.Sinks.Seq"],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.EntityFrameworkCore": "Warning",
        "System": "Warning",
        "Grpc": "Warning"
      }
    },
    "Enrich": ["FromLogContext", "WithMachineName", "WithEnvironmentName"],
    "Properties": {
      "Service": "VehiclesSaleService"
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "formatter": "Serilog.Formatting.Json.JsonFormatter, Serilog"
        }
      },
      {
        "Name": "Seq",
        "Args": {
          "serverUrl": "http://seq:5341",
          "apiKey": "${SEQ_API_KEY}"
        }
      }
    ]
  }
}
```

---

## 3. Structured Logging

### 3.1 Formato de Log

```json
{
  "@t": "2026-01-21T10:30:45.1234567Z",
  "@mt": "Vehicle {VehicleId} created by user {UserId}",
  "@l": "Information",
  "@tr": "00000000-0000-0000-0000-000000000001",
  "@sp": "00000000-0000-0000-0000-000000000002",
  "VehicleId": "abc-123",
  "UserId": "user-456",
  "Service": "VehiclesSaleService",
  "Environment": "Production",
  "MachineName": "vehiclessaleservice-7b8c9d-xyz12",
  "RequestId": "0HN1234567890",
  "RequestPath": "/api/vehicles",
  "CorrelationId": "corr-789"
}
```

### 3.2 Ejemplos de Logging

```csharp
// ✅ Correcto - Structured logging con templates
_logger.LogInformation("Vehicle {VehicleId} created by user {UserId}", vehicleId, userId);

// ✅ Correcto - Con propiedades adicionales
using (_logger.BeginScope(new Dictionary<string, object>
{
    ["DealerId"] = dealerId,
    ["Action"] = "CreateVehicle"
}))
{
    _logger.LogInformation("Processing vehicle creation for {VehicleId}", vehicleId);
}

// ✅ Correcto - Error con excepción
try
{
    await ProcessPaymentAsync(payment);
}
catch (PaymentException ex)
{
    _logger.LogError(ex, "Payment failed for order {OrderId}. Amount: {Amount}",
        orderId, payment.Amount);
    throw;
}

// ❌ Incorrecto - String interpolation (pierde estructura)
_logger.LogInformation($"Vehicle {vehicleId} created by user {userId}");

// ❌ Incorrecto - Demasiada información sensible
_logger.LogInformation("User logged in with password {Password}", password);
```

---

## 4. Log Enrichers

### 4.1 Request Logging Middleware

```csharp
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault()
            ?? Guid.NewGuid().ToString();

        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("RequestId", context.TraceIdentifier))
        using (LogContext.PushProperty("RequestPath", context.Request.Path))
        using (LogContext.PushProperty("RequestMethod", context.Request.Method))
        using (LogContext.PushProperty("UserId", context.User?.FindFirst("sub")?.Value))
        {
            var sw = Stopwatch.StartNew();

            try
            {
                await _next(context);
                sw.Stop();

                _logger.LogInformation(
                    "HTTP {Method} {Path} responded {StatusCode} in {Elapsed}ms",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    sw.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex,
                    "HTTP {Method} {Path} failed after {Elapsed}ms",
                    context.Request.Method,
                    context.Request.Path,
                    sw.ElapsedMilliseconds);
                throw;
            }
        }
    }
}
```

### 4.2 Sensitive Data Filter

```csharp
public class SensitiveDataDestructuringPolicy : IDestructuringPolicy
{
    private static readonly HashSet<string> SensitiveProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        "password", "secret", "apikey", "token", "authorization",
        "creditcard", "cardnumber", "cvv", "cedula", "ssn"
    };

    public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory,
        out LogEventPropertyValue result)
    {
        if (value is IDictionary<string, object> dict)
        {
            var sanitized = dict.ToDictionary(
                kvp => kvp.Key,
                kvp => SensitiveProperties.Contains(kvp.Key) ? "[REDACTED]" : kvp.Value);

            result = propertyValueFactory.CreatePropertyValue(sanitized, destructureObjects: true);
            return true;
        }

        result = null;
        return false;
    }
}
```

---

## 5. Procesos de Logging

### 5.1 LOG-001: Flujo de Log Request-Response

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    Request-Response Logging Flow                         │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│   1. Request enters Gateway                                             │
│   ┌─────────────────────────────────────────────────────────────────┐  │
│   │ LOG: HTTP POST /api/vehicles received                            │  │
│   │      CorrelationId: abc-123                                      │  │
│   │      RequestId: 0HN12345                                         │  │
│   └─────────────────────────────────────────────────────────────────┘  │
│                              │                                          │
│                              ▼                                          │
│   2. Request forwarded to VehiclesSaleService                          │
│   ┌─────────────────────────────────────────────────────────────────┐  │
│   │ LOG: Processing CreateVehicle command                            │  │
│   │      VehicleId: veh-456                                          │  │
│   │      DealerId: dlr-789                                           │  │
│   │      CorrelationId: abc-123                                      │  │
│   └─────────────────────────────────────────────────────────────────┘  │
│                              │                                          │
│                              ▼                                          │
│   3. Database operation                                                 │
│   ┌─────────────────────────────────────────────────────────────────┐  │
│   │ LOG: Executing INSERT vehicles                                   │  │
│   │      Duration: 45ms                                              │  │
│   │      CorrelationId: abc-123                                      │  │
│   └─────────────────────────────────────────────────────────────────┘  │
│                              │                                          │
│                              ▼                                          │
│   4. Event published                                                    │
│   ┌─────────────────────────────────────────────────────────────────┐  │
│   │ LOG: Publishing VehicleCreatedEvent to RabbitMQ                  │  │
│   │      Exchange: vehicles.events                                   │  │
│   │      CorrelationId: abc-123                                      │  │
│   └─────────────────────────────────────────────────────────────────┘  │
│                              │                                          │
│                              ▼                                          │
│   5. Response sent                                                      │
│   ┌─────────────────────────────────────────────────────────────────┐  │
│   │ LOG: HTTP POST /api/vehicles responded 201 in 156ms             │  │
│   │      CorrelationId: abc-123                                      │  │
│   └─────────────────────────────────────────────────────────────────┘  │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

### 5.2 LOG-002: Error Logging con Stack Trace

```csharp
public async Task<Result<Payment>> ProcessPaymentAsync(PaymentRequest request)
{
    using (_logger.BeginScope(new Dictionary<string, object>
    {
        ["PaymentId"] = request.PaymentId,
        ["OrderId"] = request.OrderId,
        ["Amount"] = request.Amount,
        ["Gateway"] = request.Gateway
    }))
    {
        try
        {
            _logger.LogInformation("Starting payment processing");

            var result = await _paymentGateway.ChargeAsync(request);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Payment processed successfully. TransactionId: {TransactionId}",
                    result.TransactionId);
            }
            else
            {
                _logger.LogWarning("Payment declined. Reason: {DeclineReason}", result.DeclineReason);
            }

            return result;
        }
        catch (PaymentGatewayException ex)
        {
            _logger.LogError(ex,
                "Payment gateway error. Gateway: {Gateway}, Code: {ErrorCode}",
                request.Gateway, ex.ErrorCode);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during payment processing");
            throw;
        }
    }
}
```

---

## 6. Seq Queries

### 6.1 Queries Comunes

```sql
-- Errores en las últimas 24 horas
@Level = 'Error' and @Timestamp > Now() - 24h

-- Errores por servicio
select count(*) as ErrorCount, Service
from stream
where @Level = 'Error'
group by Service
order by ErrorCount desc

-- Requests lentos (> 1 segundo)
ElapsedMs > 1000 and @MessageTemplate like 'HTTP%responded%'

-- Errores de un usuario específico
UserId = 'user-123' and @Level = 'Error'

-- Flujo de una transacción por CorrelationId
CorrelationId = 'abc-123'

-- Pagos fallidos hoy
Service = 'BillingService' and @Level = 'Error' and @Timestamp > Today()

-- Top 10 errores más frecuentes
select count(*) as Count, @MessageTemplate
from stream
where @Level = 'Error'
group by @MessageTemplate
order by Count desc
limit 10
```

### 6.2 Dashboards en Seq

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         Seq Dashboard - OKLA                             │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  Last 24 Hours Overview                                                 │
│  ─────────────────────                                                  │
│                                                                          │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐ │
│  │    Total     │  │   Errors     │  │  Warnings    │  │   Avg Resp   │ │
│  │   Events     │  │              │  │              │  │    Time      │ │
│  │   1.2M       │  │     234      │  │    1,456     │  │    45ms      │ │
│  │    ↓5%       │  │    ↑12%      │  │    ↓8%       │  │    ↓10%      │ │
│  └──────────────┘  └──────────────┘  └──────────────┘  └──────────────┘ │
│                                                                          │
│  Events by Level (Last Hour)                                            │
│  ┌─────────────────────────────────────────────────────────────────────┐│
│  │ 🟢 Information ████████████████████████████████████████  89%       ││
│  │ 🟡 Warning     ████████                                   8%        ││
│  │ 🔴 Error       ███                                        3%        ││
│  └─────────────────────────────────────────────────────────────────────┘│
│                                                                          │
│  Errors by Service                                                      │
│  ┌─────────────────────────────────────────────────────────────────────┐│
│  │ BillingService      ████████████████                    45          ││
│  │ VehiclesSaleService ██████████                          28          ││
│  │ NotificationService ████████                            22          ││
│  │ AuthService         ██████                              16          ││
│  │ MediaService        ████                                12          ││
│  └─────────────────────────────────────────────────────────────────────┘│
│                                                                          │
│  Recent Errors                                                          │
│  ┌─────────────────────────────────────────────────────────────────────┐│
│  │ 10:30:45 │ BillingService  │ Payment gateway timeout                ││
│  │ 10:28:12 │ AuthService     │ Invalid refresh token                  ││
│  │ 10:25:33 │ MediaService    │ S3 upload failed                       ││
│  │ 10:22:01 │ VehiclesService │ Database connection pool exhausted     ││
│  └─────────────────────────────────────────────────────────────────────┘│
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 7. Alertas Basadas en Logs

### 7.1 Configuración de Alertas en Seq

```json
{
  "alerts": [
    {
      "id": "high-error-rate",
      "title": "High Error Rate",
      "query": "@Level = 'Error' and @Timestamp > Now() - 5m",
      "countThreshold": 50,
      "notificationChannels": ["slack-critical", "pagerduty"]
    },
    {
      "id": "payment-failures",
      "title": "Payment Failures",
      "query": "Service = 'BillingService' and @Level = 'Error' and @MessageTemplate like '%Payment%failed%'",
      "countThreshold": 5,
      "windowMinutes": 10,
      "notificationChannels": ["slack-payments", "pagerduty"]
    },
    {
      "id": "database-connection-errors",
      "title": "Database Connection Issues",
      "query": "@Level = 'Error' and (@MessageTemplate like '%database%' or @MessageTemplate like '%connection%pool%')",
      "countThreshold": 10,
      "windowMinutes": 5,
      "notificationChannels": ["slack-critical", "pagerduty"]
    },
    {
      "id": "slow-requests",
      "title": "Slow Request Detected",
      "query": "ElapsedMs > 5000",
      "countThreshold": 20,
      "windowMinutes": 15,
      "notificationChannels": ["slack-warnings"]
    }
  ]
}
```

---

## 8. Log Retention

### 8.1 Políticas de Retención

| Nivel         | Retención  | Almacenamiento |
| ------------- | ---------- | -------------- |
| `Debug`       | 7 días     | Hot            |
| `Information` | 30 días    | Hot → Warm     |
| `Warning`     | 90 días    | Hot → Warm     |
| `Error`       | 365 días   | Hot → Cold     |
| `Fatal`       | Indefinido | Hot → Archive  |

### 8.2 Archivado

```json
{
  "Retention": {
    "Policies": [
      {
        "Level": "Information",
        "RetentionDays": 30,
        "ArchiveAfterDays": 15,
        "ArchiveStorage": "s3://okla-logs-archive/information/"
      },
      {
        "Level": "Error",
        "RetentionDays": 365,
        "ArchiveAfterDays": 30,
        "ArchiveStorage": "s3://okla-logs-archive/errors/"
      }
    ]
  }
}
```

---

## 9. Performance Considerations

### 9.1 Async Logging

```csharp
// Usar async sinks para no bloquear requests
.WriteTo.Async(a => a.Seq("http://seq:5341"), bufferSize: 10000)
```

### 9.2 Batching

```csharp
.WriteTo.Seq("http://seq:5341",
    batchPostingLimit: 1000,
    period: TimeSpan.FromSeconds(2))
```

### 9.3 Log Sampling

```csharp
// Para logs de alto volumen, samplear
if (Random.Shared.Next(100) < 10) // 10% sampling
{
    _logger.LogDebug("High-frequency event sampled");
}
```

---

## 10. Métricas

```
# Logs por nivel
serilog_events_total{level="..."}

# Logs por servicio
serilog_events_total{service="..."}

# Errores en los últimos 5 minutos
increase(serilog_events_total{level="Error"}[5m])

# Latencia de envío a Seq
serilog_sink_seq_batch_duration_seconds
```

---

## 📚 Referencias

- [Serilog Documentation](https://serilog.net/) - Documentación oficial
- [Seq Documentation](https://docs.datalust.co/docs) - Documentación de Seq
- [04-health-checks.md](04-health-checks.md) - Health checks
- [05-monitoring.md](05-monitoring.md) - Monitoreo
