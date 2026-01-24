# ❌ ErrorService - Matriz de Procesos

> **Servicio:** ErrorService  
> **Puerto:** 15101  
> **Base de Datos:** error_db  
> **Última actualización:** Enero 21, 2026  
> **Estado de Implementación:** ✅ 100% Completo

---

## 📊 Resumen de Implementación

| Componente            | Total | Implementado | Pendiente | Estado  |
| --------------------- | ----- | ------------ | --------- | ------- |
| **Controllers**       | 2     | 2            | 0         | ✅ 100% |
| **Procesos (ERR-\*)** | 6     | 6            | 0         | ✅ 100% |
| **Procesos (DLQ-\*)** | 3     | 3            | 0         | ✅ 100% |
| **Tests Unitarios**   | 15    | 15           | 0         | ✅ 100% |

### Leyenda de Estados

- ✅ **IMPLEMENTADO Y PROBADO**: Código completo con tests
- 🟢 **IMPLEMENTADO**: Código completo, falta testing
- 🟡 **EN PROGRESO**: Implementación parcial
- 🔴 **PENDIENTE**: No implementado

---

## 1. Información General

### 1.1 Descripción

El ErrorService centraliza el registro, almacenamiento, análisis y monitoreo de errores de todos los microservicios de OKLA. Implementa Dead Letter Queue (DLQ) para errores de mensajería, estadísticas agregadas, y alertas automáticas.

### 1.2 Responsabilidades

| Función               | Descripción                              |
| --------------------- | ---------------------------------------- |
| **Error Logging**     | Recibir y almacenar errores de servicios |
| **Error Aggregation** | Agrupar errores similares                |
| **Statistics**        | Generar estadísticas por servicio/tiempo |
| **Dead Letter Queue** | Procesar mensajes fallidos               |
| **Alerting**          | Notificar errores críticos               |
| **Retention**         | Limpiar errores antiguos                 |

### 1.3 Fuentes de Errores

| Fuente            | Mecanismo             | Prioridad |
| ----------------- | --------------------- | --------- |
| HTTP Direct       | POST /api/errors      | Alta      |
| RabbitMQ          | error.events exchange | Media     |
| Dead Letter Queue | DLQ consumer          | Alta      |
| Gateway           | Error forwarding      | Alta      |

### 1.4 Dependencias

| Servicio            | Propósito             |
| ------------------- | --------------------- |
| RabbitMQ            | Recibir errores async |
| NotificationService | Alertas críticas      |
| Seq                 | Logging centralizado  |
| Prometheus          | Métricas              |

---

## 2. Endpoints API

### 2.1 ErrorsController

| Método | Endpoint               | Descripción           | Auth | Rate Limit |
| ------ | ---------------------- | --------------------- | ---- | ---------- |
| POST   | `/api/errors`          | Registrar error       | ✅   | 200/min    |
| GET    | `/api/errors`          | Listar errores        | ✅   | 150/min    |
| GET    | `/api/errors/{id}`     | Obtener error         | ✅   | 200/min    |
| GET    | `/api/errors/stats`    | Estadísticas          | ✅   | 100/min    |
| GET    | `/api/errors/services` | Servicios con errores | ✅   | 150/min    |

### 2.2 HealthController

| Método | Endpoint        | Descripción  | Auth |
| ------ | --------------- | ------------ | ---- |
| GET    | `/health`       | Health check | ❌   |
| GET    | `/health/ready` | Readiness    | ❌   |
| GET    | `/health/live`  | Liveness     | ❌   |

---

## 3. Entidades

### 3.1 ErrorRecord

| Campo         | Tipo                        | Descripción                      |
| ------------- | --------------------------- | -------------------------------- |
| Id            | Guid                        | ID único del error               |
| ServiceName   | string                      | Nombre del servicio origen       |
| ErrorType     | string                      | Tipo de excepción                |
| Message       | string                      | Mensaje de error                 |
| StackTrace    | string?                     | Stack trace completo             |
| Severity      | ErrorSeverity               | Info, Warning, Error, Critical   |
| Context       | Dictionary<string, object>? | Contexto adicional               |
| UserId        | string?                     | Usuario afectado                 |
| RequestPath   | string?                     | Path del request                 |
| RequestMethod | string?                     | Método HTTP                      |
| TraceId       | string?                     | Trace ID distribuido             |
| CorrelationId | string?                     | Correlation ID                   |
| Environment   | string                      | Development, Staging, Production |
| MachineName   | string                      | Servidor/Pod                     |
| OccurredAt    | DateTime                    | Fecha del error                  |
| CreatedAt     | DateTime                    | Fecha de registro                |

### 3.2 ErrorSeverity (Enum)

| Valor    | Descripción   | Alerting     |
| -------- | ------------- | ------------ |
| Info     | Informativo   | ❌           |
| Warning  | Advertencia   | ❌           |
| Error    | Error normal  | Rate-based   |
| Critical | Error crítico | ✅ Inmediato |

### 3.3 AggregatedError

| Campo       | Tipo                  | Descripción                   |
| ----------- | --------------------- | ----------------------------- |
| Fingerprint | string                | Hash del error (para agrupar) |
| ServiceName | string                | Servicio                      |
| ErrorType   | string                | Tipo                          |
| Message     | string                | Mensaje (sanitizado)          |
| FirstSeen   | DateTime              | Primera ocurrencia            |
| LastSeen    | DateTime              | Última ocurrencia             |
| Count       | int                   | Total ocurrencias             |
| Status      | AggregatedErrorStatus | New, Acknowledged, Resolved   |

---

## 4. Procesos Detallados

### ERR-LOG-001: Registrar Error

| Campo          | Valor           |
| -------------- | --------------- |
| **ID**         | ERR-LOG-001     |
| **Nombre**     | Registrar Error |
| **Actor**      | Microservicio   |
| **Criticidad** | 🔴 CRÍTICO      |
| **Estado**     | 🟢 ACTIVO       |

#### Request Body

```json
{
  "serviceName": "VehiclesSaleService",
  "errorType": "System.NullReferenceException",
  "message": "Object reference not set to an instance of an object",
  "stackTrace": "at VehiclesSaleService.Application.Handlers...",
  "severity": "Error",
  "context": {
    "vehicleId": "abc123",
    "operation": "CreateVehicle",
    "userId": "user-456"
  },
  "userId": "user-456",
  "requestPath": "/api/vehicles",
  "requestMethod": "POST",
  "traceId": "00-abc123-def456-01",
  "correlationId": "corr-789"
}
```

#### Flujo Paso a Paso

| Paso | Acción                 | Servicio            | Validación           |
| ---- | ---------------------- | ------------------- | -------------------- |
| 1    | Recibir request        | Controller          | Schema válido        |
| 2    | Rate limit check       | Middleware          | < 200/min            |
| 3    | Validar authorization  | AuthPolicy          | ErrorServiceRead     |
| 4    | Sanitizar datos        | Handler             | Remove PII           |
| 5    | Generar fingerprint    | Handler             | Hash de tipo+mensaje |
| 6    | Buscar error agregado  | Repository          | By fingerprint       |
| 7    | Si existe: incrementar | Repository          | UPDATE count         |
| 8    | Si no: crear nuevo     | Repository          | INSERT               |
| 9    | Guardar error completo | Repository          | INSERT               |
| 10   | Check severity         | Handler             | Critical?            |
| 11   | Si Critical: alertar   | NotificationService | Async                |
| 12   | Publicar métrica       | Prometheus          | error_count++        |
| 13   | Retornar ID            | Controller          | 200 OK               |

#### Response Success (200)

```json
{
  "success": true,
  "data": {
    "errorId": "guid",
    "aggregatedErrorId": "guid",
    "fingerprint": "abc123...",
    "isNew": false,
    "occurrenceNumber": 47
  }
}
```

---

### ERR-STATS-001: Obtener Estadísticas

| Campo          | Valor                   |
| -------------- | ----------------------- |
| **ID**         | ERR-STATS-001           |
| **Nombre**     | Estadísticas de Errores |
| **Actor**      | Admin/Dashboard         |
| **Criticidad** | 🟢 MEDIO                |
| **Estado**     | 🟢 ACTIVO               |

#### Request

```
GET /api/errors/stats?from=2026-01-14&to=2026-01-21
```

#### Response (200)

```json
{
  "success": true,
  "data": {
    "totalErrors": 1247,
    "criticalErrors": 12,
    "errorsByService": [
      {
        "serviceName": "VehiclesSaleService",
        "count": 456,
        "percentage": 36.6
      },
      { "serviceName": "AuthService", "count": 312, "percentage": 25.0 },
      { "serviceName": "BillingService", "count": 189, "percentage": 15.2 }
    ],
    "errorsBySeverity": [
      { "severity": "Info", "count": 89 },
      { "severity": "Warning", "count": 234 },
      { "severity": "Error", "count": 912 },
      { "severity": "Critical", "count": 12 }
    ],
    "errorsByDay": [
      { "date": "2026-01-14", "count": 178 },
      { "date": "2026-01-15", "count": 156 },
      { "date": "2026-01-16", "count": 203 }
    ],
    "topErrors": [
      {
        "fingerprint": "abc123",
        "message": "Connection timeout",
        "count": 89,
        "lastSeen": "2026-01-21T10:25:00Z"
      }
    ],
    "from": "2026-01-14T00:00:00Z",
    "to": "2026-01-21T23:59:59Z"
  }
}
```

---

### ERR-DLQ-001: Procesar Dead Letter Queue

| Campo          | Valor              |
| -------------- | ------------------ |
| **ID**         | ERR-DLQ-001        |
| **Nombre**     | Dead Letter Queue  |
| **Actor**      | Sistema (Consumer) |
| **Criticidad** | 🔴 CRÍTICO         |
| **Estado**     | 🟢 ACTIVO          |

#### Descripción

Cuando un mensaje falla el procesamiento después de N reintentos, va al DLQ. El ErrorService consume estos mensajes para análisis.

#### Flujo de DLQ

```
┌──────────────────────────────────────────────────────────────────────────────┐
│                           DEAD LETTER QUEUE FLOW                             │
├──────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  Servicio                                                                    │
│     │                                                                        │
│     │ Publish mensaje                                                        │
│     ▼                                                                        │
│  RabbitMQ Exchange ────► Queue                                              │
│                              │                                               │
│                              │ Consumer intenta procesar                     │
│                              ▼                                               │
│                          ¿Éxito?                                            │
│                          /     \                                             │
│                        Sí       No                                           │
│                        │         │                                           │
│                        │         ▼                                           │
│                        │    Retry (3x)                                       │
│                        │         │                                           │
│                        │         │ Falla 3 veces                             │
│                        │         ▼                                           │
│                        │    DLQ Exchange ────► DLQ Queue                    │
│                        │                            │                        │
│                        │                            ▼                        │
│                        │                     ErrorService                    │
│                        │                     DLQ Consumer                    │
│                        │                            │                        │
│                        │                            │ Guardar + Alertar      │
│                        │                            ▼                        │
│                        │                     error_db                        │
│                        │                                                     │
│                        ▼                                                     │
│                   ACK mensaje                                                │
│                                                                              │
└──────────────────────────────────────────────────────────────────────────────┘
```

#### DLQ Message Structure

```json
{
  "originalExchange": "vehicle.events",
  "originalRoutingKey": "vehicle.created",
  "originalMessage": {
    "vehicleId": "abc123",
    "dealerId": "dealer-456"
  },
  "error": "System.TimeoutException: Database timeout",
  "retryCount": 3,
  "firstFailure": "2026-01-21T10:25:00Z",
  "lastFailure": "2026-01-21T10:27:30Z",
  "consumerService": "NotificationService"
}
```

---

### ERR-ALERT-001: Alertas de Errores Críticos

| Campo          | Valor                 |
| -------------- | --------------------- |
| **ID**         | ERR-ALERT-001         |
| **Nombre**     | Alertar Error Crítico |
| **Actor**      | Sistema               |
| **Criticidad** | 🔴 CRÍTICO            |
| **Estado**     | 🟢 ACTIVO             |

#### Condiciones de Alerta

| Condición           | Threshold         | Canal         |
| ------------------- | ----------------- | ------------- |
| Error Critical      | Cualquiera        | Slack + Email |
| Error rate spike    | 5x normal en 5min | Slack         |
| Nuevo tipo de error | First occurrence  | Email         |
| Service degradation | 50 errors/min     | PagerDuty     |
| DLQ message         | Cualquiera        | Slack         |

#### Flujo de Alerta

| Paso | Acción                | Servicio            | Output       |
| ---- | --------------------- | ------------------- | ------------ |
| 1    | Detectar condición    | AlertService        |              |
| 2    | Verificar throttling  | Cache               | No spam      |
| 3    | Formatear mensaje     | Handler             | Template     |
| 4    | Enviar a Slack        | SlackService        | Webhook      |
| 5    | Enviar email          | NotificationService | Admin emails |
| 6    | Si PagerDuty: escalar | PagerDutyService    | On-call      |
| 7    | Registrar alerta      | Repository          | INSERT       |

#### Slack Message Format

```json
{
  "blocks": [
    {
      "type": "header",
      "text": {
        "type": "plain_text",
        "text": "🚨 Critical Error - VehiclesSaleService"
      }
    },
    {
      "type": "section",
      "fields": [
        { "type": "mrkdwn", "text": "*Type:* NullReferenceException" },
        { "type": "mrkdwn", "text": "*Count:* 47 in last hour" },
        { "type": "mrkdwn", "text": "*First Seen:* 10:25 AM" },
        { "type": "mrkdwn", "text": "*Trace ID:* abc-123-def" }
      ]
    },
    {
      "type": "section",
      "text": {
        "type": "mrkdwn",
        "text": "*Message:* Object reference not set to an instance of an object"
      }
    },
    {
      "type": "actions",
      "elements": [
        {
          "type": "button",
          "text": { "type": "plain_text", "text": "View in Seq" },
          "url": "https://seq.okla.com.do/events?filter=..."
        },
        {
          "type": "button",
          "text": { "type": "plain_text", "text": "View in Jaeger" },
          "url": "https://jaeger.okla.com.do/trace/abc-123"
        }
      ]
    }
  ]
}
```

---

### ERR-RETENTION-001: Limpieza de Errores Antiguos

| Campo          | Valor                  |
| -------------- | ---------------------- |
| **ID**         | ERR-RETENTION-001      |
| **Nombre**     | Error Retention Policy |
| **Actor**      | Scheduled Job          |
| **Criticidad** | 🟢 MEDIO               |
| **Estado**     | 🟢 ACTIVO              |

#### Políticas de Retención

| Severity   | Retención | Razón        |
| ---------- | --------- | ------------ |
| Info       | 7 días    | Low value    |
| Warning    | 14 días   | Medium value |
| Error      | 30 días   | Standard     |
| Critical   | 90 días   | Important    |
| Aggregated | 1 año     | Statistics   |

#### Cron Schedule

```
0 3 * * * (Diario a las 3am)
```

#### Flujo

| Paso | Acción                | Query                                                        |
| ---- | --------------------- | ------------------------------------------------------------ |
| 1    | Delete Info > 7d      | `DELETE WHERE severity='Info' AND createdAt < NOW()-7d`      |
| 2    | Delete Warning > 14d  | `DELETE WHERE severity='Warning' AND createdAt < NOW()-14d`  |
| 3    | Delete Error > 30d    | `DELETE WHERE severity='Error' AND createdAt < NOW()-30d`    |
| 4    | Delete Critical > 90d | `DELETE WHERE severity='Critical' AND createdAt < NOW()-90d` |
| 5    | Update aggregated     | `UPDATE SET status='Archived' WHERE lastSeen < NOW()-1y`     |
| 6    | Log cleanup stats     | Seq                                                          |

---

## 5. Fingerprinting

### 5.1 Algoritmo de Fingerprint

```csharp
public string GenerateFingerprint(ErrorRecord error)
{
    var normalized = $"{error.ServiceName}|{error.ErrorType}|{NormalizeMessage(error.Message)}";
    return ComputeSha256Hash(normalized);
}

private string NormalizeMessage(string message)
{
    // Remove dynamic values
    var normalized = Regex.Replace(message, @"'\w{8}-\w{4}-\w{4}-\w{4}-\w{12}'", "'[GUID]'");
    normalized = Regex.Replace(normalized, @"'?\d+'?", "[NUMBER]");
    normalized = Regex.Replace(normalized, @"@\w+\.\w+", "[EMAIL]");
    return normalized;
}
```

### 5.2 Ejemplos

| Original                      | Normalizado               | Fingerprint |
| ----------------------------- | ------------------------- | ----------- |
| `User 'abc-123' not found`    | `User '[GUID]' not found` | `a1b2c3...` |
| `Invalid price: 15000`        | `Invalid price: [NUMBER]` | `d4e5f6...` |
| `Email john@test.com invalid` | `Email [EMAIL] invalid`   | `g7h8i9...` |

---

## 6. Integración con Servicios

### 6.1 Publicar Error desde Servicio

```csharp
// En cualquier servicio, usar el cliente de ErrorService

public class ErrorPublisher
{
    public async Task PublishError(Exception ex, string serviceName, Dictionary<string, object>? context = null)
    {
        var message = new ErrorMessage
        {
            ServiceName = serviceName,
            ErrorType = ex.GetType().FullName,
            Message = ex.Message,
            StackTrace = ex.StackTrace,
            Severity = DetermineSeverity(ex),
            Context = context,
            TraceId = Activity.Current?.TraceId.ToString(),
            OccurredAt = DateTime.UtcNow
        };

        // Via RabbitMQ (async)
        await _rabbitMQPublisher.PublishAsync("error.events", "error.logged", message);

        // O via HTTP (sync)
        await _httpClient.PostAsync("/api/errors", message);
    }
}
```

### 6.2 Global Exception Handler

```csharp
// En cada microservicio

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

        // Publicar a ErrorService
        await _errorPublisher.PublishError(exception, "ServiceName", new
        {
            path = context.Request.Path,
            method = context.Request.Method,
            userId = context.User?.FindFirst("sub")?.Value
        });

        // Retornar error response
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new
        {
            error = "An error occurred",
            traceId = Activity.Current?.TraceId.ToString()
        });
    });
});
```

---

## 7. Métricas Prometheus

### 7.1 Métricas Expuestas

| Métrica                             | Tipo      | Labels                  |
| ----------------------------------- | --------- | ----------------------- |
| `errors_total`                      | Counter   | service, severity, type |
| `errors_by_service_total`           | Counter   | service                 |
| `errors_critical_total`             | Counter   | service                 |
| `dlq_messages_total`                | Counter   | exchange, queue         |
| `error_processing_duration_seconds` | Histogram | operation               |
| `aggregated_errors_active`          | Gauge     | status                  |

### 7.2 Alertas Prometheus

```yaml
groups:
  - name: error-service-alerts
    rules:
      - alert: HighErrorRate
        expr: rate(errors_total{severity="Error"}[5m]) > 10
        for: 2m
        labels:
          severity: warning
        annotations:
          summary: "High error rate detected"

      - alert: CriticalErrorDetected
        expr: increase(errors_critical_total[1m]) > 0
        for: 0m
        labels:
          severity: critical
        annotations:
          summary: "Critical error in {{ $labels.service }}"

      - alert: DLQBacklog
        expr: dlq_messages_total > 100
        for: 5m
        labels:
          severity: warning
        annotations:
          summary: "DLQ backlog growing"
```

---

## 8. Configuración

### 8.1 appsettings.json

```json
{
  "ErrorService": {
    "RetentionDays": {
      "Info": 7,
      "Warning": 14,
      "Error": 30,
      "Critical": 90
    },
    "AlertThresholds": {
      "CriticalImmediate": true,
      "ErrorRateSpike": 5,
      "ErrorRateSpikeWindowMinutes": 5
    },
    "DLQ": {
      "Enabled": true,
      "ExchangeName": "dlq.exchange",
      "QueueName": "error.dlq"
    }
  },
  "Slack": {
    "WebhookUrl": "[SLACK_WEBHOOK]",
    "Channel": "#okla-errors"
  },
  "PagerDuty": {
    "Enabled": false,
    "ServiceKey": "[PAGERDUTY_KEY]"
  }
}
```

---

## 9. Manejo de Errores (Propio)

### 9.1 ¿Qué pasa si ErrorService falla?

| Escenario         | Fallback             |
| ----------------- | -------------------- |
| DB no disponible  | Write to local file  |
| RabbitMQ down     | HTTP direct call     |
| ErrorService down | Services log locally |
| Full disk         | Rotate old logs      |

### 9.2 Circuit Breaker en Clientes

```csharp
// Los servicios usan circuit breaker al llamar a ErrorService
services.AddHttpClient("ErrorService")
    .AddPolicyHandler(Policy
        .Handle<HttpRequestException>()
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 5,
            durationOfBreak: TimeSpan.FromMinutes(1)
        ));
```

---

## 10. Dashboard

### 10.1 Grafana Dashboard

```json
{
  "panels": [
    {
      "title": "Errors per Minute",
      "type": "graph",
      "targets": [{ "expr": "rate(errors_total[1m])" }]
    },
    {
      "title": "Errors by Service",
      "type": "piechart",
      "targets": [{ "expr": "sum by (service) (errors_total)" }]
    },
    {
      "title": "Critical Errors",
      "type": "stat",
      "targets": [{ "expr": "sum(increase(errors_critical_total[24h]))" }]
    },
    {
      "title": "Top Error Types",
      "type": "table",
      "targets": [{ "expr": "topk(10, sum by (type) (errors_total))" }]
    }
  ]
}
```

---

**Documento generado:** Enero 21, 2026  
**Versión:** 1.0.0  
**Autor:** Equipo OKLA
