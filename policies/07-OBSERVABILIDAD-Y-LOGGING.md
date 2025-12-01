# POLÍTICA 07: OBSERVABILIDAD Y LOGGING

**Versión**: 1.0  
**Última Actualización**: 2025-11-30  
**Estado**: OBLIGATORIO ✅  
**Responsable**: Equipo de Arquitectura CarDealer

---

## 📋 RESUMEN EJECUTIVO

**POLÍTICA CRÍTICA**: Todos los microservicios deben implementar los 3 pilares de observabilidad: Logs (Serilog), Metrics (Prometheus), Traces (Jaeger). Sin observabilidad completa, NO es posible hacer troubleshooting efectivo en producción.

**Objetivo**: Tener visibilidad completa del comportamiento del sistema en tiempo real y capacidad de análisis histórico.

**Alcance**: Aplica a TODOS los microservicios del ecosistema CarDealer.

---

## 🎯 LOS 3 PILARES DE OBSERVABILIDAD

### Matriz de Pilares

| Pilar | Herramienta | Propósito | Cuándo Usar |
|-------|-------------|-----------|-------------|
| **📝 Logs** | Serilog + Seq/ELK | Eventos discretos, errores, auditoría | Debugging, troubleshooting |
| **📊 Metrics** | Prometheus + Grafana | Agregaciones numéricas, performance | Alerting, capacity planning |
| **🔍 Traces** | Jaeger (OpenTelemetry) | Request flow, latency distribution | Performance analysis |

**REGLA**: Los 3 pilares son OBLIGATORIOS. No es aceptable implementar solo 1 o 2.

---

## 📝 PILAR 1: LOGS (SERILOG)

### Niveles de Logging Obligatorios

```csharp
// appsettings.json - Niveles por ambiente
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",  // Development: Debug, Production: Information
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.AspNetCore": "Warning",
        "Microsoft.EntityFrameworkCore": "Information",  // Ver queries SQL
        "Microsoft.EntityFrameworkCore.Database.Command": "Information",
        "System": "Warning",
        "System.Net.Http.HttpClient": "Warning"
      }
    }
  }
}
```

---

### Structured Logging Pattern (OBLIGATORIO)

```csharp
// ✅ CORRECTO - Structured Logging
_logger.LogInformation(
    "User {UserId} created order {OrderId} for vehicle {VehicleId} with total {Total:C}",
    userId,
    orderId,
    vehicleId,
    totalAmount);

// ✅ CORRECTO - Exception con contexto
_logger.LogError(ex,
    "Failed to process payment for order {OrderId}. PaymentMethod={PaymentMethod}, Amount={Amount:C}",
    orderId,
    paymentMethod,
    amount);

// ❌ PROHIBIDO - String interpolation
_logger.LogInformation($"User {userId} created order {orderId}");

// ❌ PROHIBIDO - Sin contexto
_logger.LogError("Payment failed");
```

---

### Log Enrichment (OBLIGATORIO)

```csharp
// Program.cs
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .Enrich.WithThreadId()
    .Enrich.WithProperty("ServiceName", "ErrorService")
    .Enrich.WithProperty("ServiceVersion", "1.0.0")
    .Enrich.WithSpan()  // TraceId y SpanId
    .CreateLogger();
```

---

### Sinks Obligatorios por Ambiente

```json
{
  "Serilog": {
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss.fff}] [{Level:u3}] [TraceId={TraceId}] {Message:lj}{NewLine}{Exception}",
          "theme": "Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme::Code, Serilog.Sinks.Console"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/errorservice-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30,
          "fileSizeLimitBytes": 104857600,
          "outputTemplate": "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] [TraceId={TraceId}] [SpanId={SpanId}] [{SourceContext}] {Message:lj}{NewLine}{Exception}"
        }
      },
      {
        "Name": "Seq",
        "Args": {
          "serverUrl": "http://localhost:5341",
          "apiKey": "your-api-key-here"
        }
      }
    ]
  }
}
```

---

### Request Logging Middleware

```csharp
// Program.cs
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = 
        "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000}ms " +
        "TraceId={TraceId} SpanId={SpanId} UserAgent={UserAgent}";
    
    options.GetLevel = (httpContext, elapsed, ex) =>
    {
        if (ex != null) return LogEventLevel.Error;
        if (httpContext.Response.StatusCode >= 500) return LogEventLevel.Error;
        if (httpContext.Response.StatusCode >= 400) return LogEventLevel.Warning;
        if (elapsed > 1000) return LogEventLevel.Warning;  // Slow request
        return LogEventLevel.Information;
    };
    
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
        diagnosticContext.Set("RemoteIpAddress", httpContext.Connection.RemoteIpAddress?.ToString());
        diagnosticContext.Set("ContentLength", httpContext.Request.ContentLength);
        
        // TraceId y SpanId de OpenTelemetry
        var activity = Activity.Current;
        if (activity != null)
        {
            diagnosticContext.Set("TraceId", activity.TraceId.ToString());
            diagnosticContext.Set("SpanId", activity.SpanId.ToString());
        }
        
        // Claims del usuario autenticado
        if (httpContext.User.Identity?.IsAuthenticated == true)
        {
            diagnosticContext.Set("UserId", httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            diagnosticContext.Set("UserName", httpContext.User.Identity.Name);
        }
    };
});
```

---

### Log Scopes para Correlación

```csharp
// Uso de LogContext para agregar propiedades al scope
using Serilog.Context;

public class LogErrorCommandHandler : IRequestHandler<LogErrorCommand, Guid>
{
    public async Task<Guid> Handle(LogErrorCommand request, CancellationToken ct)
    {
        // Crear scope con propiedades que se agregarán a TODOS los logs
        using (LogContext.PushProperty("ServiceName", request.ServiceName))
        using (LogContext.PushProperty("ExceptionType", request.ExceptionType))
        {
            _logger.LogInformation("Starting error logging process");
            
            // ... lógica
            
            _logger.LogInformation("Error logged successfully with Id={ErrorId}", errorLog.Id);
            // Ambos logs tendrán ServiceName y ExceptionType
        }
        
        return errorLog.Id;
    }
}
```

---

## 📊 PILAR 2: METRICS (PROMETHEUS)

### Configuración de Prometheus

```csharp
// Program.cs
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

// ... otros servicios

var app = builder.Build();

// Metrics middleware (ANTES de otros middlewares)
app.UseHttpMetrics(options =>
{
    options.AddCustomLabel("service", context => "ErrorService");
    options.AddCustomLabel("environment", context => builder.Environment.EnvironmentName);
});

// Prometheus scraping endpoint
app.MapMetrics();  // Expone /metrics

app.Run();
```

---

### Custom Metrics (OBLIGATORIOS)

```csharp
// CustomMetrics.cs
using Prometheus;

namespace ErrorService.Infrastructure.Metrics
{
    public static class CustomMetrics
    {
        // Counter: Incrementa siempre (nunca decrece)
        public static readonly Counter ErrorsLogged = Metrics.CreateCounter(
            "errorservice_errors_logged_total",
            "Total number of errors logged",
            new CounterConfiguration
            {
                LabelNames = new[] { "service_name", "exception_type", "status_code" }
            });
        
        // Gauge: Valor que sube y baja
        public static readonly Gauge ActiveConnections = Metrics.CreateGauge(
            "errorservice_active_connections",
            "Number of active database connections");
        
        // Histogram: Distribución de valores (latency, size, etc)
        public static readonly Histogram RequestDuration = Metrics.CreateHistogram(
            "errorservice_request_duration_seconds",
            "Duration of HTTP requests in seconds",
            new HistogramConfiguration
            {
                LabelNames = new[] { "method", "endpoint", "status_code" },
                Buckets = Histogram.ExponentialBuckets(0.001, 2, 10)  // 1ms, 2ms, 4ms, ..., 512ms
            });
        
        // Summary: Similar a Histogram pero con percentiles
        public static readonly Summary ErrorProcessingTime = Metrics.CreateSummary(
            "errorservice_error_processing_seconds",
            "Time spent processing error logs",
            new SummaryConfiguration
            {
                LabelNames = new[] { "service_name" },
                Objectives = new[]
                {
                    new QuantileEpsilonPair(0.5, 0.05),   // p50 (median)
                    new QuantileEpsilonPair(0.9, 0.01),   // p90
                    new QuantileEpsilonPair(0.99, 0.001)  // p99
                }
            });
    }
}
```

---

### Uso de Custom Metrics

```csharp
// LogErrorCommandHandler.cs
using ErrorService.Infrastructure.Metrics;

public class LogErrorCommandHandler : IRequestHandler<LogErrorCommand, Guid>
{
    public async Task<Guid> Handle(LogErrorCommand request, CancellationToken ct)
    {
        // Medir duración con Histogram
        using (CustomMetrics.ErrorProcessingTime
            .WithLabels(request.ServiceName)
            .NewTimer())
        {
            var errorLog = new ErrorLog { /* ... */ };
            await _repository.AddAsync(errorLog, ct);
            
            // Incrementar counter
            CustomMetrics.ErrorsLogged
                .WithLabels(
                    request.ServiceName,
                    request.ExceptionType,
                    request.StatusCode.ToString())
                .Inc();
            
            return errorLog.Id;
        }
    }
}
```

```csharp
// DatabaseConnectionPool.cs
public class DatabaseConnectionPool
{
    private int _activeConnections = 0;
    
    public async Task<DbConnection> GetConnectionAsync()
    {
        var connection = await CreateConnectionAsync();
        
        Interlocked.Increment(ref _activeConnections);
        CustomMetrics.ActiveConnections.Set(_activeConnections);
        
        return connection;
    }
    
    public void ReleaseConnection(DbConnection connection)
    {
        connection.Dispose();
        
        Interlocked.Decrement(ref _activeConnections);
        CustomMetrics.ActiveConnections.Set(_activeConnections);
    }
}
```

---

### Prometheus Queries Útiles

```promql
# Error rate por servicio
rate(errorservice_errors_logged_total[5m])

# P99 latency de requests
histogram_quantile(0.99, rate(errorservice_request_duration_seconds_bucket[5m]))

# Número de errores 5xx en última hora
increase(errorservice_errors_logged_total{status_code=~"5.."}[1h])

# Active connections promedio
avg_over_time(errorservice_active_connections[5m])

# Request throughput (requests/sec)
rate(http_requests_received_total{service="ErrorService"}[1m])
```

---

## 🔍 PILAR 3: TRACES (JAEGER + OPENTELEMETRY)

### Configuración Completa de OpenTelemetry

```csharp
// Program.cs
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using OpenTelemetry.Logs;

var builder = WebApplication.CreateBuilder(args);

// OPENTELEMETRY
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(
            serviceName: "ErrorService",
            serviceVersion: "1.0.0",
            serviceInstanceId: Environment.MachineName)
        .AddAttributes(new Dictionary<string, object>
        {
            ["deployment.environment"] = builder.Environment.EnvironmentName,
            ["host.name"] = Environment.MachineName,
            ["service.namespace"] = "CarDealer"
        }))
    
    // TRACING
    .WithTracing(tracing => tracing
        .AddSource("ErrorService")
        .AddSource("CarDealer.*")  // Todos los servicios CarDealer
        
        // ASP.NET Core instrumentation
        .AddAspNetCoreInstrumentation(options =>
        {
            options.RecordException = true;
            
            options.Filter = (httpContext) =>
            {
                // No trazar health checks (reduce noise)
                return !httpContext.Request.Path.StartsWithSegments("/health");
            };
            
            options.EnrichWithHttpRequest = (activity, request) =>
            {
                activity.SetTag("http.request.user_agent", request.Headers.UserAgent.ToString());
                activity.SetTag("http.request.content_type", request.ContentType);
                activity.SetTag("http.request.content_length", request.ContentLength);
                
                // Claims del usuario
                if (request.HttpContext.User.Identity?.IsAuthenticated == true)
                {
                    activity.SetTag("user.id", request.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                }
            };
            
            options.EnrichWithHttpResponse = (activity, response) =>
            {
                activity.SetTag("http.response.content_type", response.ContentType);
                activity.SetTag("http.response.content_length", response.ContentLength);
            };
            
            options.EnrichWithException = (activity, exception) =>
            {
                activity.SetTag("exception.type", exception.GetType().Name);
                activity.SetTag("exception.message", exception.Message);
                activity.SetTag("exception.stacktrace", exception.StackTrace);
            };
        })
        
        // HTTP Client instrumentation
        .AddHttpClientInstrumentation(options =>
        {
            options.RecordException = true;
            
            options.EnrichWithHttpRequestMessage = (activity, request) =>
            {
                activity.SetTag("http.request.method", request.Method.ToString());
                activity.SetTag("http.request.uri", request.RequestUri?.ToString());
            };
            
            options.EnrichWithHttpResponseMessage = (activity, response) =>
            {
                activity.SetTag("http.response.status_code", (int)response.StatusCode);
            };
        })
        
        // Entity Framework Core instrumentation
        .AddEntityFrameworkCoreInstrumentation(options =>
        {
            options.SetDbStatementForText = true;
            options.SetDbStatementForStoredProcedure = true;
            
            options.EnrichWithIDbCommand = (activity, command) =>
            {
                activity.SetTag("db.command.text", command.CommandText);
                activity.SetTag("db.command.timeout", command.CommandTimeout);
            };
        })
        
        // Sampling
        .SetSampler(new TraceIdRatioBasedSampler(
            builder.Environment.IsProduction() ? 0.1 : 1.0))  // 10% en prod, 100% en dev
        
        // Exporter
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(builder.Configuration["OpenTelemetry:JaegerEndpoint"]!);
            options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
        }))
    
    // METRICS
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddProcessInstrumentation()
        .AddPrometheusExporter());

var app = builder.Build();

app.MapPrometheusScrapingEndpoint();
app.Run();
```

---

### Custom Activity Source

```csharp
// CustomActivitySource.cs
using System.Diagnostics;

namespace ErrorService.Infrastructure.Telemetry
{
    public static class ErrorServiceActivitySource
    {
        public static readonly ActivitySource Source = new("ErrorService", "1.0.0");
        
        public static Activity? StartActivity(
            string name,
            ActivityKind kind = ActivityKind.Internal,
            ActivityContext parentContext = default,
            Dictionary<string, object?>? tags = null,
            IEnumerable<ActivityLink>? links = null,
            DateTimeOffset startTime = default)
        {
            var activity = Source.StartActivity(
                name,
                kind,
                parentContext,
                tags,
                links,
                startTime);
            
            return activity;
        }
    }
}
```

---

### Crear Spans Personalizados

```csharp
// LogErrorCommandHandler.cs
using ErrorService.Infrastructure.Telemetry;

public class LogErrorCommandHandler : IRequestHandler<LogErrorCommand, Guid>
{
    public async Task<Guid> Handle(LogErrorCommand request, CancellationToken ct)
    {
        // Crear span para toda la operación
        using var activity = ErrorServiceActivitySource.StartActivity(
            "LogError.Process",
            ActivityKind.Internal,
            tags: new Dictionary<string, object?>
            {
                ["service.name"] = request.ServiceName,
                ["exception.type"] = request.ExceptionType,
                ["error.status_code"] = request.StatusCode
            });
        
        try
        {
            // Sub-span para validación
            using (var validationActivity = ErrorServiceActivitySource.StartActivity(
                "LogError.Validate",
                ActivityKind.Internal))
            {
                // ... validación
                validationActivity?.SetTag("validation.result", "success");
            }
            
            // Sub-span para persistencia
            using (var dbActivity = ErrorServiceActivitySource.StartActivity(
                "LogError.SaveToDatabase",
                ActivityKind.Internal))
            {
                var errorLog = new ErrorLog { /* ... */ };
                await _repository.AddAsync(errorLog, ct);
                
                dbActivity?.SetTag("db.operation", "INSERT");
                dbActivity?.SetTag("db.table", "ErrorLogs");
                dbActivity?.SetTag("error.id", errorLog.Id.ToString());
                
                activity?.SetTag("error.id", errorLog.Id.ToString());
                activity?.SetTag("operation.result", "success");
                
                return errorLog.Id;
            }
        }
        catch (Exception ex)
        {
            // Marcar span como error
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.RecordException(ex);
            
            throw;
        }
    }
}
```

---

### Propagación de Trace Context

```csharp
// HttpClient con trace context propagation (automático con OpenTelemetry)
public class ExternalServiceClient
{
    private readonly HttpClient _httpClient;
    
    public ExternalServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<string> CallExternalServiceAsync()
    {
        // TraceId y SpanId se propagan automáticamente en headers
        // W3C Trace Context: traceparent, tracestate
        var response = await _httpClient.GetAsync("https://external-service/api/data");
        
        return await response.Content.ReadAsStringAsync();
    }
}
```

---

## 📈 GRAFANA DASHBOARDS (OBLIGATORIOS)

### Dashboard 1: Service Overview

```json
{
  "dashboard": {
    "title": "ErrorService - Overview",
    "panels": [
      {
        "title": "Request Rate",
        "targets": [
          {
            "expr": "rate(http_requests_received_total{service=\"ErrorService\"}[5m])"
          }
        ]
      },
      {
        "title": "Error Rate",
        "targets": [
          {
            "expr": "rate(errorservice_errors_logged_total[5m])"
          }
        ]
      },
      {
        "title": "P99 Latency",
        "targets": [
          {
            "expr": "histogram_quantile(0.99, rate(http_request_duration_seconds_bucket{service=\"ErrorService\"}[5m]))"
          }
        ]
      },
      {
        "title": "Active Connections",
        "targets": [
          {
            "expr": "errorservice_active_connections"
          }
        ]
      }
    ]
  }
}
```

---

### Dashboard 2: Error Analysis

```json
{
  "dashboard": {
    "title": "ErrorService - Error Analysis",
    "panels": [
      {
        "title": "Errors by Service",
        "targets": [
          {
            "expr": "sum by (service_name) (rate(errorservice_errors_logged_total[5m]))"
          }
        ]
      },
      {
        "title": "Errors by Exception Type",
        "targets": [
          {
            "expr": "sum by (exception_type) (rate(errorservice_errors_logged_total[5m]))"
          }
        ]
      },
      {
        "title": "HTTP Status Codes",
        "targets": [
          {
            "expr": "sum by (status_code) (rate(http_requests_received_total{service=\"ErrorService\"}[5m]))"
          }
        ]
      }
    ]
  }
}
```

---

## 🚨 ALERTING (PROMETHEUS ALERTMANAGER)

### Reglas de Alerting Obligatorias

```yaml
# prometheus-alerts.yml
groups:
  - name: ErrorService
    interval: 30s
    rules:
      # Alert: High Error Rate
      - alert: ErrorServiceHighErrorRate
        expr: |
          rate(errorservice_errors_logged_total{status_code=~"5.."}[5m]) > 10
        for: 2m
        labels:
          severity: critical
          service: ErrorService
        annotations:
          summary: "High error rate in ErrorService"
          description: "ErrorService is logging {{ $value }} errors/sec (threshold: 10)"
      
      # Alert: High Latency
      - alert: ErrorServiceHighLatency
        expr: |
          histogram_quantile(0.99, rate(http_request_duration_seconds_bucket{service="ErrorService"}[5m])) > 1
        for: 5m
        labels:
          severity: warning
          service: ErrorService
        annotations:
          summary: "High latency in ErrorService"
          description: "P99 latency is {{ $value }}s (threshold: 1s)"
      
      # Alert: Service Down
      - alert: ErrorServiceDown
        expr: |
          up{job="errorservice"} == 0
        for: 1m
        labels:
          severity: critical
          service: ErrorService
        annotations:
          summary: "ErrorService is down"
          description: "ErrorService has been down for 1 minute"
      
      # Alert: Database Connection Issues
      - alert: ErrorServiceDatabaseConnectionIssues
        expr: |
          errorservice_active_connections == 0
        for: 2m
        labels:
          severity: critical
          service: ErrorService
        annotations:
          summary: "ErrorService has no active database connections"
          description: "Database connectivity issue detected"
      
      # Alert: Memory Usage High
      - alert: ErrorServiceHighMemoryUsage
        expr: |
          process_resident_memory_bytes{service="ErrorService"} > 1073741824
        for: 5m
        labels:
          severity: warning
          service: ErrorService
        annotations:
          summary: "ErrorService memory usage is high"
          description: "Memory usage: {{ $value | humanize }}B (threshold: 1GB)"
```

---

## 🔔 NOTIFICATION CHANNELS

### Configuración de Alertmanager

```yaml
# alertmanager.yml
global:
  resolve_timeout: 5m

route:
  group_by: ['alertname', 'service']
  group_wait: 10s
  group_interval: 10s
  repeat_interval: 12h
  receiver: 'default'
  
  routes:
    - match:
        severity: critical
      receiver: 'pagerduty'
      continue: true
    
    - match:
        severity: warning
      receiver: 'slack'

receivers:
  - name: 'default'
    email_configs:
      - to: 'team@cardealer.com'
        from: 'alerts@cardealer.com'
        smarthost: 'smtp.gmail.com:587'
  
  - name: 'slack'
    slack_configs:
      - api_url: 'https://hooks.slack.com/services/YOUR/WEBHOOK/URL'
        channel: '#alerts'
        title: '{{ .GroupLabels.alertname }}'
        text: '{{ range .Alerts }}{{ .Annotations.description }}{{ end }}'
  
  - name: 'pagerduty'
    pagerduty_configs:
      - service_key: 'YOUR_PAGERDUTY_KEY'
        description: '{{ .GroupLabels.alertname }}'

inhibit_rules:
  - source_match:
      severity: 'critical'
    target_match:
      severity: 'warning'
    equal: ['alertname', 'service']
```

---

## 📋 LOG AGGREGATION (SEQ / ELK)

### Seq Configuration (Recomendado para .NET)

```json
{
  "Serilog": {
    "WriteTo": [
      {
        "Name": "Seq",
        "Args": {
          "serverUrl": "http://localhost:5341",
          "apiKey": "your-api-key-here",
          "restrictedToMinimumLevel": "Information",
          "batchPostingLimit": 100,
          "period": "00:00:02"
        }
      }
    ]
  }
}
```

### Queries Útiles en Seq

```sql
-- Errores en última hora
@Level = 'Error' and @Timestamp > Now() - 1h

-- Requests lentos (> 1s)
RequestPath is not null and Elapsed > 1000

-- Errores por TraceId
TraceId = '4bf92f3577b34da6a3ce929d0e0e4736'

-- Top 10 excepciones más comunes
select ExceptionType, count(*) as Count
from stream
where @Level = 'Error'
group by ExceptionType
order by Count desc
limit 10
```

---

## ✅ CHECKLIST DE OBSERVABILIDAD

### Logs
- [ ] Serilog configurado con structured logging
- [ ] TraceId y SpanId en todos los logs
- [ ] Log enrichment (MachineName, Environment, ServiceName)
- [ ] Request logging middleware configurado
- [ ] Log levels apropiados (Debug en dev, Info en prod)
- [ ] File sink con rotación diaria (retention 30 días)
- [ ] Console sink con formato legible
- [ ] Seq/ELK sink configurado (opcional pero recomendado)

### Metrics
- [ ] Prometheus configurado con /metrics endpoint
- [ ] HTTP metrics automáticos (latency, throughput, status codes)
- [ ] Custom metrics implementados:
  - [ ] Counters para eventos de negocio
  - [ ] Gauges para valores actuales
  - [ ] Histograms para distribuciones
- [ ] Grafana dashboards creados:
  - [ ] Service Overview
  - [ ] Error Analysis
  - [ ] Performance
- [ ] Alerting rules configuradas
- [ ] Notification channels configurados (Slack, Email, PagerDuty)

### Traces
- [ ] OpenTelemetry configurado
- [ ] Jaeger exporter configurado
- [ ] ASP.NET Core instrumentation habilitado
- [ ] Entity Framework instrumentation habilitado
- [ ] HTTP Client instrumentation habilitado
- [ ] Custom Activity Source creado
- [ ] Spans personalizados en operaciones críticas
- [ ] Exception tracking en spans
- [ ] Sampling configurado (100% dev, 10% prod)
- [ ] Trace context propagation funcional

---

## 📚 RECURSOS Y REFERENCIAS

- **Microservicio de Referencia**: `ErrorService/Program.cs`
- **Serilog**: [https://serilog.net/](https://serilog.net/)
- **Prometheus**: [https://prometheus.io/](https://prometheus.io/)
- **Grafana**: [https://grafana.com/](https://grafana.com/)
- **Jaeger**: [https://www.jaegertracing.io/](https://www.jaegertracing.io/)
- **OpenTelemetry**: [https://opentelemetry.io/](https://opentelemetry.io/)
- **Seq**: [https://datalust.co/seq](https://datalust.co/seq)

---

**Fecha de Vigencia**: 2025-11-30  
**Aprobado por**: Equipo de Arquitectura CarDealer  
**Revisión**: Trimestral

**NOTA**: Sin los 3 pilares de observabilidad completos, NO es posible aprobar merge a main. Observabilidad NO es opcional.
