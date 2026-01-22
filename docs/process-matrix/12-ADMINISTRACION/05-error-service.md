# ⚠️ Error Service - Servicio de Errores - Matriz de Procesos

> **Servicio:** ErrorService  
> **Puerto:** 5018  
> **Última actualización:** Enero 21, 2026  
> **Estado:** 🟢 ACTIVO

---

## 1. Información General

### 1.1 Descripción

Servicio centralizado para captura, análisis y gestión de errores en la plataforma OKLA. Recibe excepciones de todos los microservicios, las agrupa, notifica al equipo, y proporciona un dashboard para análisis y resolución.

### 1.2 Funcionalidades

| Funcionalidad     | Descripción                           |
| ----------------- | ------------------------------------- |
| **Error Capture** | Recibe errores de todos los servicios |
| **Grouping**      | Agrupa errores similares              |
| **Alerting**      | Notifica cuando hay nuevos errores    |
| **Dashboard**     | UI para análisis de errores           |
| **Dead Letter**   | Gestión de mensajes fallidos          |
| **Trends**        | Análisis de tendencias                |

### 1.3 Arquitectura

```
┌─────────────────────────────────────────────────────────────────────────┐
│                      Error Service Architecture                          │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│   Microservices                                                         │
│   ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐                  │
│   │  Auth    │ │  Users   │ │ Vehicles │ │ Billing  │  ...             │
│   │ Service  │ │ Service  │ │ Service  │ │ Service  │                  │
│   └────┬─────┘ └────┬─────┘ └────┬─────┘ └────┬─────┘                  │
│        │            │            │            │                         │
│        └────────────┴────────────┴────────────┘                         │
│                         │                                                │
│                         │ POST /api/errors                              │
│                         │ RabbitMQ: error.events                        │
│                         ▼                                                │
│   ┌─────────────────────────────────────────────────────────────────┐   │
│   │                      ErrorService API                            │   │
│   │                                                                   │   │
│   │   ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐ │   │
│   │   │ Error Ingestion │  │ Error Grouping  │  │ Alert Engine    │ │   │
│   │   │                 │  │                 │  │                 │ │   │
│   │   │ - Deduplicate   │  │ - Fingerprint   │  │ - Threshold     │ │   │
│   │   │ - Enrich        │  │ - Similar stack │  │ - Notification  │ │   │
│   │   │ - Store         │  │ - Same service  │  │ - Escalation    │ │   │
│   │   └─────────────────┘  └─────────────────┘  └─────────────────┘ │   │
│   │                                                                   │   │
│   │   ┌─────────────────────────────────────────────────────────┐   │   │
│   │   │                   Dead Letter Queue                      │   │   │
│   │   │                                                          │   │   │
│   │   │   RabbitMQ messages that failed processing:             │   │   │
│   │   │   - View message content                                │   │   │
│   │   │   - Retry processing                                    │   │   │
│   │   │   - Move to archive                                     │   │   │
│   │   │   - Delete permanently                                  │   │   │
│   │   └─────────────────────────────────────────────────────────┘   │   │
│   └───────────────────────────────┬─────────────────────────────────┘   │
│                                   │                                      │
│              ┌────────────────────┼────────────────────┐                │
│              │                    │                    │                 │
│              ▼                    ▼                    ▼                 │
│   ┌──────────────────┐ ┌──────────────────┐ ┌──────────────────┐        │
│   │   PostgreSQL     │ │  Notifications   │ │   Admin Panel    │        │
│   │   (Errors DB)    │ │  (Teams/Email)   │ │   (Dashboard)    │        │
│   └──────────────────┘ └──────────────────┘ └──────────────────┘        │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 2. Endpoints

### 2.1 Error Ingestion

| Método | Endpoint            | Descripción        | Auth    |
| ------ | ------------------- | ------------------ | ------- |
| `POST` | `/api/errors`       | Reportar error     | Service |
| `POST` | `/api/errors/batch` | Reportar múltiples | Service |

### 2.2 Error Query

| Método | Endpoint                  | Descripción       | Auth  |
| ------ | ------------------------- | ----------------- | ----- |
| `GET`  | `/api/errors`             | Listar errores    | Admin |
| `GET`  | `/api/errors/{id}`        | Detalle de error  | Admin |
| `GET`  | `/api/errors/groups`      | Errores agrupados | Admin |
| `GET`  | `/api/errors/groups/{id}` | Grupo específico  | Admin |

### 2.3 Error Management

| Método | Endpoint                   | Descripción     | Auth  |
| ------ | -------------------------- | --------------- | ----- |
| `PUT`  | `/api/errors/{id}/status`  | Cambiar estado  | Admin |
| `POST` | `/api/errors/{id}/assign`  | Asignar a dev   | Admin |
| `POST` | `/api/errors/{id}/resolve` | Marcar resuelto | Admin |
| `POST` | `/api/errors/{id}/ignore`  | Ignorar error   | Admin |

### 2.4 Dead Letter Queue

| Método   | Endpoint                | Descripción         | Auth       |
| -------- | ----------------------- | ------------------- | ---------- |
| `GET`    | `/api/dlq`              | Listar mensajes DLQ | Admin      |
| `GET`    | `/api/dlq/{id}`         | Detalle de mensaje  | Admin      |
| `POST`   | `/api/dlq/{id}/retry`   | Reintentar mensaje  | Admin      |
| `POST`   | `/api/dlq/{id}/archive` | Archivar mensaje    | Admin      |
| `DELETE` | `/api/dlq/{id}`         | Eliminar mensaje    | SuperAdmin |
| `POST`   | `/api/dlq/retry-all`    | Reintentar todos    | SuperAdmin |

### 2.5 Analytics

| Método | Endpoint                 | Descripción            | Auth  |
| ------ | ------------------------ | ---------------------- | ----- |
| `GET`  | `/api/errors/stats`      | Estadísticas generales | Admin |
| `GET`  | `/api/errors/trends`     | Tendencias             | Admin |
| `GET`  | `/api/errors/by-service` | Errores por servicio   | Admin |

---

## 3. Entidades

### 3.1 ErrorEntry

```csharp
public class ErrorEntry
{
    public Guid Id { get; set; }
    public string Fingerprint { get; set; } = string.Empty; // Hash para agrupación
    public Guid? GroupId { get; set; }

    // Source
    public string ServiceName { get; set; } = string.Empty;
    public string ServiceVersion { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string Hostname { get; set; } = string.Empty;

    // Error details
    public string Type { get; set; } = string.Empty; // NullReferenceException
    public string Message { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public string? InnerException { get; set; }

    // Context
    public string? RequestPath { get; set; }
    public string? RequestMethod { get; set; }
    public string? UserId { get; set; }
    public string? DealerId { get; set; }
    public string? CorrelationId { get; set; }
    public Dictionary<string, string> Tags { get; set; } = new();
    public Dictionary<string, object> Extra { get; set; } = new();

    // Status
    public ErrorStatus Status { get; set; }
    public Guid? AssignedToId { get; set; }
    public string? Resolution { get; set; }

    public DateTime OccurredAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public enum ErrorStatus
{
    New,
    Acknowledged,
    InProgress,
    Resolved,
    Ignored,
    Recurring
}
```

### 3.2 ErrorGroup

```csharp
public class ErrorGroup
{
    public Guid Id { get; set; }
    public string Fingerprint { get; set; } = string.Empty;

    public string ServiceName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    // Counts
    public int TotalOccurrences { get; set; }
    public int OccurrencesLast24h { get; set; }
    public int OccurrencesLast7d { get; set; }

    // Timing
    public DateTime FirstSeen { get; set; }
    public DateTime LastSeen { get; set; }

    // Status
    public ErrorStatus Status { get; set; }
    public Guid? AssignedToId { get; set; }

    // Affected
    public int UniqueUsersAffected { get; set; }
    public List<string> AffectedEndpoints { get; set; } = new();
}
```

### 3.3 DeadLetterMessage

```csharp
public class DeadLetterMessage
{
    public Guid Id { get; set; }

    // Original message
    public string Exchange { get; set; } = string.Empty;
    public string RoutingKey { get; set; } = string.Empty;
    public string Queue { get; set; } = string.Empty;
    public string MessageType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public Dictionary<string, string> Headers { get; set; } = new();

    // Failure info
    public string ErrorMessage { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public int RetryCount { get; set; }
    public DateTime LastRetryAt { get; set; }

    // Status
    public DlqStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? ProcessingResult { get; set; }
}

public enum DlqStatus
{
    Pending,
    Retrying,
    Resolved,
    Archived,
    Deleted
}
```

---

## 4. Procesos Detallados

### 4.1 ERR-001: Capturar y Agrupar Error

| Paso | Acción                        | Sistema             | Validación       |
| ---- | ----------------------------- | ------------------- | ---------------- |
| 1    | Excepción ocurre en servicio  | VehiclesSvc         | Exception thrown |
| 2    | Middleware captura excepción  | ErrorMiddleware     | Catch block      |
| 3    | Serializar error con contexto | ErrorMiddleware     | ErrorPayload     |
| 4    | POST /api/errors              | ErrorService        | Request received |
| 5    | Calcular fingerprint          | ErrorService        | Hash generated   |
| 6    | Buscar grupo existente        | PostgreSQL          | Group query      |
| 7    | Si nuevo: crear grupo         | PostgreSQL          | Group created    |
| 8    | Guardar ErrorEntry            | PostgreSQL          | Entry saved      |
| 9    | Incrementar contadores        | PostgreSQL          | Counters++       |
| 10   | Evaluar alerta                | AlertEngine         | Threshold check  |
| 11   | Si threshold: notificar       | NotificationService | Alert sent       |

```csharp
public class ErrorIngestionHandler : IRequestHandler<IngestErrorCommand, ErrorEntry>
{
    public async Task<ErrorEntry> Handle(IngestErrorCommand request, CancellationToken ct)
    {
        // 1. Calculate fingerprint for grouping
        var fingerprint = CalculateFingerprint(request);

        // 2. Find or create error group
        var group = await _groupRepository.GetByFingerprintAsync(fingerprint, ct);

        if (group == null)
        {
            group = new ErrorGroup
            {
                Fingerprint = fingerprint,
                ServiceName = request.ServiceName,
                Type = request.Type,
                Message = TruncateMessage(request.Message, 500),
                FirstSeen = DateTime.UtcNow,
                LastSeen = DateTime.UtcNow,
                Status = ErrorStatus.New
            };

            await _groupRepository.AddAsync(group, ct);

            // New error group - alert immediately
            await AlertNewErrorGroupAsync(group, ct);
        }
        else
        {
            group.TotalOccurrences++;
            group.OccurrencesLast24h++;
            group.LastSeen = DateTime.UtcNow;

            if (group.Status == ErrorStatus.Resolved)
            {
                group.Status = ErrorStatus.Recurring;
                await AlertRecurringErrorAsync(group, ct);
            }

            await _groupRepository.UpdateAsync(group, ct);
        }

        // 3. Create error entry
        var entry = new ErrorEntry
        {
            Fingerprint = fingerprint,
            GroupId = group.Id,
            ServiceName = request.ServiceName,
            ServiceVersion = request.ServiceVersion,
            Environment = request.Environment,
            Hostname = request.Hostname,
            Type = request.Type,
            Message = request.Message,
            StackTrace = request.StackTrace,
            RequestPath = request.RequestPath,
            RequestMethod = request.RequestMethod,
            UserId = request.UserId,
            CorrelationId = request.CorrelationId,
            Tags = request.Tags,
            Extra = request.Extra,
            Status = ErrorStatus.New,
            OccurredAt = request.OccurredAt,
            CreatedAt = DateTime.UtcNow
        };

        await _errorRepository.AddAsync(entry, ct);

        // 4. Check alert thresholds
        await EvaluateAlertsAsync(group, ct);

        return entry;
    }

    private string CalculateFingerprint(IngestErrorCommand request)
    {
        // Group by: service + exception type + first 3 stack frames
        var stackFrames = ParseStackFrames(request.StackTrace)
            .Take(3)
            .Select(f => $"{f.Method}:{f.Line}");

        var input = $"{request.ServiceName}:{request.Type}:{string.Join("|", stackFrames)}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));

        return Convert.ToBase64String(hash)[..16];
    }

    private async Task EvaluateAlertsAsync(ErrorGroup group, CancellationToken ct)
    {
        // Alert if > 10 occurrences in last hour
        if (group.OccurrencesLast24h > 10)
        {
            var recentCount = await _errorRepository.CountRecentAsync(
                group.Fingerprint,
                TimeSpan.FromHours(1),
                ct);

            if (recentCount > 10)
            {
                await _alertService.SendHighFrequencyAlertAsync(group, recentCount, ct);
            }
        }
    }
}
```

### 4.2 ERR-002: Gestionar Dead Letter Queue

| Paso | Acción                      | Sistema      | Validación       |
| ---- | --------------------------- | ------------ | ---------------- |
| 1    | Mensaje falla procesamiento | Consumer     | Exception        |
| 2    | RabbitMQ mueve a DLQ        | RabbitMQ     | x-dead-letter    |
| 3    | DLQ consumer recibe         | ErrorService | Message received |
| 4    | Guardar DeadLetterMessage   | PostgreSQL   | DLQ entry        |
| 5    | Admin revisa en dashboard   | Frontend     | List view        |
| 6    | Admin decide acción         | Frontend     | Retry/Archive    |
| 7    | Si retry: republish         | RabbitMQ     | Original queue   |
| 8    | Actualizar status           | PostgreSQL   | Status updated   |

```csharp
public class DlqConsumer : IConsumer<DeadLetterEnvelope>
{
    public async Task Consume(ConsumeContext<DeadLetterEnvelope> context)
    {
        var envelope = context.Message;

        // Extract original message info from headers
        var headers = context.Headers;
        var originalExchange = headers.Get<string>("x-first-death-exchange");
        var originalQueue = headers.Get<string>("x-first-death-queue");
        var deathReason = headers.Get<string>("x-first-death-reason");

        var dlqMessage = new DeadLetterMessage
        {
            Exchange = originalExchange ?? "unknown",
            RoutingKey = envelope.RoutingKey,
            Queue = originalQueue ?? "unknown",
            MessageType = envelope.MessageType,
            Payload = JsonSerializer.Serialize(envelope.Body),
            Headers = context.Headers.ToDictionary(),
            ErrorMessage = envelope.ErrorMessage,
            StackTrace = envelope.StackTrace,
            RetryCount = 0,
            Status = DlqStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _dlqRepository.AddAsync(dlqMessage);

        // Alert if too many DLQ messages
        var pendingCount = await _dlqRepository.CountPendingAsync();

        if (pendingCount > 50)
        {
            await _alertService.SendDlqAlertAsync(pendingCount);
        }
    }
}

// Retry handler
public class RetryDlqMessageHandler : IRequestHandler<RetryDlqMessageCommand, bool>
{
    public async Task<bool> Handle(RetryDlqMessageCommand request, CancellationToken ct)
    {
        var message = await _dlqRepository.GetByIdAsync(request.MessageId, ct);
        if (message == null) return false;

        message.Status = DlqStatus.Retrying;
        message.RetryCount++;
        message.LastRetryAt = DateTime.UtcNow;

        await _dlqRepository.UpdateAsync(message, ct);

        try
        {
            // Republish to original queue
            var body = JsonSerializer.Deserialize<object>(message.Payload);

            await _bus.Publish(body, context =>
            {
                context.Headers.Set("x-retry-count", message.RetryCount.ToString());
                context.Headers.Set("x-original-dlq-id", message.Id.ToString());
            }, ct);

            message.Status = DlqStatus.Resolved;
            message.ProcessedAt = DateTime.UtcNow;
            message.ProcessingResult = "Republished successfully";
        }
        catch (Exception ex)
        {
            message.Status = DlqStatus.Pending;
            message.ProcessingResult = $"Retry failed: {ex.Message}";
        }

        await _dlqRepository.UpdateAsync(message, ct);

        return message.Status == DlqStatus.Resolved;
    }
}
```

---

## 5. Error Dashboard

```
┌─────────────────────────────────────────────────────────────────────────┐
│  🚨 Error Dashboard                                         Last 24h    │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  Summary                                                                │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐ │
│  │   Errors     │  │    New       │  │  Resolved    │  │    DLQ       │ │
│  │   1,234      │  │     45       │  │     890      │  │     12       │ │
│  │   ↑ 5%       │  │   ↑ 12       │  │   ↓ 30%      │  │   ↓ 3        │ │
│  └──────────────┘  └──────────────┘  └──────────────┘  └──────────────┘ │
│                                                                          │
│  Error Groups (by frequency)                                            │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │ # │ Service          │ Error Type              │ Count │ Status   │  │
│  ├───┼──────────────────┼─────────────────────────┼───────┼──────────┤  │
│  │ 1 │ VehiclesSaleService │ NullReferenceException   │   156 │ 🔴 New    │  │
│  │ 2 │ BillingService   │ StripeException         │    89 │ 🟡 InProg │  │
│  │ 3 │ AuthService      │ TokenExpiredException   │    45 │ 🟢 Resolved│  │
│  │ 4 │ MediaService     │ S3UploadException       │    23 │ 🔴 New    │  │
│  │ 5 │ NotificationSvc  │ TwilioException         │    12 │ 🟡 InProg │  │
│  └───────────────────────────────────────────────────────────────────┘  │
│                                                                          │
│  Error Trend (last 7 days)                                              │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │  500│    *                                                         │  │
│  │     │   * *        *                                              │  │
│  │  250│  *   *      * *                                             │  │
│  │     │ *     *    *   *   *                                        │  │
│  │    0│*       ****     ***  *                                      │  │
│  │     └────────────────────────────────────────────────────────────  │  │
│  │       Mon    Tue    Wed    Thu    Fri    Sat    Sun               │  │
│  └───────────────────────────────────────────────────────────────────┘  │
│                                                                          │
│  Dead Letter Queue                                                      │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │ Queue: payment.completed │ Messages: 5 │ [Retry All] [Archive]    │  │
│  │ Queue: notification.send │ Messages: 7 │ [Retry All] [Archive]    │  │
│  └───────────────────────────────────────────────────────────────────┘  │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 6. SDK para Servicios

```csharp
// Middleware para capturar errores
public class ErrorReportingMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await ReportErrorAsync(context, ex);
            throw; // Re-throw to let normal error handling work
        }
    }

    private async Task ReportErrorAsync(HttpContext context, Exception ex)
    {
        var error = new IngestErrorCommand
        {
            ServiceName = _serviceName,
            ServiceVersion = _serviceVersion,
            Environment = _environment,
            Hostname = Environment.MachineName,
            Type = ex.GetType().Name,
            Message = ex.Message,
            StackTrace = ex.StackTrace,
            InnerException = ex.InnerException?.Message,
            RequestPath = context.Request.Path,
            RequestMethod = context.Request.Method,
            UserId = context.User.FindFirst("sub")?.Value,
            CorrelationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault(),
            OccurredAt = DateTime.UtcNow,
            Extra = new Dictionary<string, object>
            {
                ["Query"] = context.Request.QueryString.ToString(),
                ["UserAgent"] = context.Request.Headers["User-Agent"].ToString()
            }
        };

        await _errorClient.ReportAsync(error);
    }
}

// Extension
builder.Services.AddErrorReporting(options =>
{
    options.ServiceUrl = "http://errorservice:8080";
    options.ServiceName = "VehiclesSaleService";
    options.Environment = builder.Environment.EnvironmentName;
});
```

---

## 7. Reglas de Negocio

| Código  | Regla                        | Validación      |
| ------- | ---------------------------- | --------------- |
| ERR-R01 | Agrupar por fingerprint      | Hash match      |
| ERR-R02 | Alertar si >10 errores/hora  | Threshold check |
| ERR-R03 | Escalar si no resuelto en 4h | Timer job       |
| ERR-R04 | DLQ retry max 3 veces        | RetryCount < 3  |
| ERR-R05 | Retención 30 días errores    | Cleanup job     |
| ERR-R06 | PII redactado en logs        | Sanitizer       |

---

## 8. Métricas Prometheus

```
# Errors
errors_ingested_total{service="...", type="..."}
errors_by_status{status="new|resolved|ignored"}
error_groups_total{status="..."}

# DLQ
dlq_messages_total{queue="...", status="..."}
dlq_retry_success_rate
dlq_oldest_message_age_seconds

# Alerts
error_alerts_sent_total{type="new|recurring|threshold"}
```

---

## 📚 Referencias

- [05-logging-service.md](../11-INFRAESTRUCTURA-DEVOPS/05-logging-service.md) - Logging centralizado
- [10-monitoring.md](../11-INFRAESTRUCTURA-DEVOPS/10-monitoring.md) - Monitoreo
- [08-queue-management.md](../11-INFRAESTRUCTURA-DEVOPS/08-queue-management.md) - RabbitMQ
