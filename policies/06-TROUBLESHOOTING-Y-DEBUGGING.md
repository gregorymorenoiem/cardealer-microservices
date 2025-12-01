# POLÍTICA 06: TROUBLESHOOTING Y DEBUGGING

**Versión**: 1.0  
**Última Actualización**: 2025-11-30  
**Estado**: OBLIGATORIO ✅  
**Responsable**: Equipo de Arquitectura CarDealer

---

## 📋 RESUMEN EJECUTIVO

**POLÍTICA CRÍTICA**: Todos los microservicios deben tener capacidades de troubleshooting integradas: Serilog con TraceId/SpanId, OpenTelemetry, health checks detallados, y scripts de diagnóstico. NO es aceptable "no sé qué pasó" en producción.

**Objetivo**: Detectar, diagnosticar y resolver problemas rápidamente en desarrollo, staging y producción.

**Alcance**: Aplica a TODOS los microservicios del ecosistema CarDealer.

---

## 🎯 COMPONENTES OBLIGATORIOS DE TROUBLESHOOTING

### Matriz de Herramientas

| Componente | Propósito | Cuándo Usar | Obligatorio |
|------------|-----------|-------------|-------------|
| **Serilog + TraceId** | Logs correlacionados | Siempre | ✅ SÍ |
| **OpenTelemetry** | Distributed tracing | Producción | ✅ SÍ |
| **Health Checks** | Estado del servicio | Kubernetes/monitoring | ✅ SÍ |
| **Metrics (Prometheus)** | Performance monitoring | Producción | ✅ SÍ |
| **Exception Middleware** | Captura de errores | Siempre | ✅ SÍ |
| **Debug Scripts** | Diagnóstico rápido | Development | ✅ SÍ |

---

## 📝 SERILOG CON TRACEID Y SPANID

### Configuración Obligatoria

```csharp
// Program.cs
using Serilog;
using Serilog.Enrichers.Span;

var builder = WebApplication.CreateBuilder(args);

// SERILOG CONFIGURATION
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .Enrich.WithProperty("ServiceName", "ErrorService")
    .Enrich.WithSpan()  // ✅ OBLIGATORIO - TraceId y SpanId
    .CreateLogger();

builder.Host.UseSerilog();

// ... resto de configuración

var app = builder.Build();

// LOGGING MIDDLEWARE (debe ser primero)
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = 
        "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000}ms " +
        "TraceId={TraceId} SpanId={SpanId}";
    
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
        diagnosticContext.Set("RemoteIpAddress", httpContext.Connection.RemoteIpAddress);
        
        // TraceId y SpanId de OpenTelemetry
        var activity = Activity.Current;
        if (activity != null)
        {
            diagnosticContext.Set("TraceId", activity.TraceId.ToString());
            diagnosticContext.Set("SpanId", activity.SpanId.ToString());
        }
    };
});

app.Run();
```

---

### appsettings.json - Serilog

```json
{
  "Serilog": {
    "Using": ["Serilog.Sinks.Console", "Serilog.Sinks.File"],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.AspNetCore": "Warning",
        "Microsoft.EntityFrameworkCore": "Information",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] [TraceId={TraceId}] [SpanId={SpanId}] {Message:lj}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/errorservice-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30,
          "outputTemplate": "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] [TraceId={TraceId}] [SpanId={SpanId}] [{SourceContext}] {Message:lj}{NewLine}{Exception}"
        }
      }
    ],
    "Enrich": ["FromLogContext", "WithMachineName", "WithThreadId", "WithSpan"],
    "Properties": {
      "Application": "ErrorService",
      "Environment": "Development"
    }
  }
}
```

---

### Uso Correcto de ILogger

```csharp
// ✅ CORRECTO - Structured Logging con contexto
public class LogErrorCommandHandler : IRequestHandler<LogErrorCommand, Guid>
{
    private readonly IErrorLogRepository _repository;
    private readonly ILogger<LogErrorCommandHandler> _logger;
    
    public LogErrorCommandHandler(
        IErrorLogRepository repository,
        ILogger<LogErrorCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }
    
    public async Task<Guid> Handle(LogErrorCommand request, CancellationToken ct)
    {
        _logger.LogInformation(
            "Processing LogErrorCommand for service {ServiceName}, ExceptionType={ExceptionType}",
            request.ServiceName,
            request.ExceptionType);
        
        try
        {
            var errorLog = new ErrorLog
            {
                Id = Guid.NewGuid(),
                ServiceName = request.ServiceName,
                ExceptionType = request.ExceptionType,
                Message = request.Message,
                StackTrace = request.StackTrace,
                StatusCode = request.StatusCode,
                OccurredAt = DateTime.UtcNow
            };
            
            await _repository.AddAsync(errorLog, ct);
            
            _logger.LogInformation(
                "ErrorLog created successfully with Id={ErrorId} for service {ServiceName}",
                errorLog.Id,
                request.ServiceName);
            
            return errorLog.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to create ErrorLog for service {ServiceName}. ExceptionType={ExceptionType}",
                request.ServiceName,
                request.ExceptionType);
            
            throw;
        }
    }
}
```

```csharp
// ❌ PROHIBIDO - String interpolation
_logger.LogInformation($"Processing command for {serviceName}");

// ❌ PROHIBIDO - Sin contexto
_logger.LogError("Error occurred");

// ❌ PROHIBIDO - Exception sin log
try { }
catch { throw; }  // Sin logging!
```

---

## 🔍 OPENTELEMETRY DISTRIBUTED TRACING

### Configuración Completa

```csharp
// Program.cs
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;

var builder = WebApplication.CreateBuilder(args);

// OPENTELEMETRY CONFIGURATION
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService("ErrorService")
        .AddAttributes(new Dictionary<string, object>
        {
            ["deployment.environment"] = builder.Environment.EnvironmentName,
            ["service.version"] = "1.0.0",
            ["service.instance.id"] = Environment.MachineName
        }))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation(options =>
        {
            options.RecordException = true;
            options.EnrichWithHttpRequest = (activity, request) =>
            {
                activity.SetTag("http.request.user_agent", request.Headers.UserAgent.ToString());
                activity.SetTag("http.request.content_length", request.ContentLength);
            };
            options.EnrichWithHttpResponse = (activity, response) =>
            {
                activity.SetTag("http.response.content_length", response.ContentLength);
            };
        })
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation(options =>
        {
            options.SetDbStatementForText = true;
            options.SetDbStatementForStoredProcedure = true;
            options.EnrichWithIDbCommand = (activity, command) =>
            {
                activity.SetTag("db.command.text", command.CommandText);
            };
        })
        .AddSource("ErrorService")
        .SetSampler(new TraceIdRatioBasedSampler(1.0)) // 100% en dev, 0.1 en prod
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri("http://localhost:4318"); // Jaeger
        }))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddPrometheusExporter());

var app = builder.Build();

// Prometheus metrics endpoint
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
    public static class CustomActivitySource
    {
        public static readonly ActivitySource Source = new("ErrorService", "1.0.0");
        
        public static Activity? StartActivity(
            string name,
            ActivityKind kind = ActivityKind.Internal,
            Dictionary<string, object?>? tags = null)
        {
            var activity = Source.StartActivity(name, kind);
            
            if (activity != null && tags != null)
            {
                foreach (var tag in tags)
                {
                    activity.SetTag(tag.Key, tag.Value);
                }
            }
            
            return activity;
        }
    }
}
```

### Uso de Custom Activities

```csharp
// LogErrorCommandHandler.cs con tracing
using CustomActivitySource = ErrorService.Infrastructure.Telemetry.CustomActivitySource;

public class LogErrorCommandHandler : IRequestHandler<LogErrorCommand, Guid>
{
    public async Task<Guid> Handle(LogErrorCommand request, CancellationToken ct)
    {
        // Crear span custom
        using var activity = CustomActivitySource.StartActivity(
            "LogErrorCommandHandler.Handle",
            ActivityKind.Internal,
            new Dictionary<string, object?>
            {
                ["service.name"] = request.ServiceName,
                ["exception.type"] = request.ExceptionType,
                ["http.status_code"] = request.StatusCode
            });
        
        try
        {
            var errorLog = new ErrorLog { /* ... */ };
            await _repository.AddAsync(errorLog, ct);
            
            activity?.SetTag("error.id", errorLog.Id.ToString());
            activity?.SetTag("operation.result", "success");
            
            return errorLog.Id;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.RecordException(ex);
            throw;
        }
    }
}
```

---

## 🏥 HEALTH CHECKS DETALLADOS

### Configuración Completa

```csharp
// Program.cs
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy("Service is running"))
    .AddNpgSql(
        connectionString: builder.Configuration.GetConnectionString("DefaultConnection")!,
        name: "postgresql",
        failureStatus: HealthStatus.Unhealthy,
        tags: new[] { "db", "postgresql" })
    .AddRabbitMQ(
        rabbitConnectionString: builder.Configuration["RabbitMQ:ConnectionString"]!,
        name: "rabbitmq",
        failureStatus: HealthStatus.Unhealthy,
        tags: new[] { "messaging", "rabbitmq" })
    .AddRedis(
        redisConnectionString: builder.Configuration["Redis:ConnectionString"]!,
        name: "redis",
        failureStatus: HealthStatus.Degraded,
        tags: new[] { "cache", "redis" });

var app = builder.Build();

// Health check endpoints
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        
        var result = JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            timestamp = DateTime.UtcNow,
            duration = report.TotalDuration,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                duration = e.Value.Duration,
                description = e.Value.Description,
                exception = e.Value.Exception?.Message,
                data = e.Value.Data
            })
        });
        
        await context.Response.WriteAsync(result);
    }
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("db") || check.Tags.Contains("messaging")
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false // Solo check del servicio mismo
});
```

---

### Custom Health Check

```csharp
// CircuitBreakerHealthCheck.cs
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Polly.CircuitBreaker;

namespace ErrorService.Infrastructure.HealthChecks
{
    public class CircuitBreakerHealthCheck : IHealthCheck
    {
        private readonly CircuitBreakerPolicy _circuitBreaker;
        
        public CircuitBreakerHealthCheck(CircuitBreakerPolicy circuitBreaker)
        {
            _circuitBreaker = circuitBreaker;
        }
        
        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            var state = _circuitBreaker.CircuitState;
            
            return state switch
            {
                CircuitState.Closed => Task.FromResult(
                    HealthCheckResult.Healthy("Circuit breaker is closed")),
                
                CircuitState.HalfOpen => Task.FromResult(
                    HealthCheckResult.Degraded("Circuit breaker is half-open")),
                
                CircuitState.Open => Task.FromResult(
                    HealthCheckResult.Unhealthy("Circuit breaker is open")),
                
                CircuitState.Isolated => Task.FromResult(
                    HealthCheckResult.Unhealthy("Circuit breaker is isolated")),
                
                _ => Task.FromResult(
                    HealthCheckResult.Unhealthy($"Unknown circuit state: {state}"))
            };
        }
    }
}
```

---

## 🐛 GLOBAL EXCEPTION MIDDLEWARE

### Implementación Obligatoria

```csharp
// GlobalExceptionMiddleware.cs
using System.Diagnostics;
using System.Net;
using System.Text.Json;

namespace ErrorService.Api.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        
        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }
        
        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Obtener TraceId y SpanId
            var activity = Activity.Current;
            var traceId = activity?.TraceId.ToString() ?? Guid.NewGuid().ToString();
            var spanId = activity?.SpanId.ToString() ?? "";
            
            // Log completo con contexto
            _logger.LogError(exception,
                "Unhandled exception in request {RequestMethod} {RequestPath}. " +
                "TraceId={TraceId}, SpanId={SpanId}, RemoteIP={RemoteIP}",
                context.Request.Method,
                context.Request.Path,
                traceId,
                spanId,
                context.Connection.RemoteIpAddress);
            
            // Determinar status code
            var statusCode = exception switch
            {
                ValidationException => HttpStatusCode.BadRequest,
                UnauthorizedAccessException => HttpStatusCode.Unauthorized,
                KeyNotFoundException => HttpStatusCode.NotFound,
                _ => HttpStatusCode.InternalServerError
            };
            
            // Response estándar
            var response = new
            {
                error = new
                {
                    message = exception.Message,
                    type = exception.GetType().Name,
                    traceId = traceId,
                    spanId = spanId,
                    timestamp = DateTime.UtcNow,
                    path = context.Request.Path.Value
                }
            };
            
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;
            
            var json = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(json);
            
            // Registrar en OpenTelemetry
            if (activity != null)
            {
                activity.SetStatus(ActivityStatusCode.Error, exception.Message);
                activity.RecordException(exception);
            }
        }
    }
}
```

```csharp
// Program.cs - Registrar middleware
app.UseMiddleware<GlobalExceptionMiddleware>();
```

---

## 📊 DIAGNÓSTICO CON POWERSHELL SCRIPTS

### CHECK-SERVICE-HEALTH.ps1

```powershell
# CHECK-SERVICE-HEALTH.ps1
# Script para diagnóstico rápido de salud del servicio
param(
    [Parameter(Mandatory=$false)]
    [string]$BaseUrl = "http://localhost:5000"
)

$ErrorActionPreference = "Stop"

Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host "  SERVICE HEALTH DIAGNOSTICS" -ForegroundColor Cyan
Write-Host "  URL: $BaseUrl" -ForegroundColor Cyan
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host ""

# 1. Basic Connectivity
Write-Host "[CHECK 1] Basic Connectivity..." -NoNewline
try {
    $response = Invoke-WebRequest -Uri "$BaseUrl/health" -Method GET -TimeoutSec 5
    Write-Host " ✅ OK (HTTP $($response.StatusCode))" -ForegroundColor Green
}
catch {
    Write-Host " ❌ FAILED" -ForegroundColor Red
    Write-Host "  Error: $_" -ForegroundColor Red
    exit 1
}

# 2. Detailed Health Check
Write-Host "[CHECK 2] Detailed Health Status..." -NoNewline
try {
    $healthResponse = Invoke-RestMethod -Uri "$BaseUrl/health" -Method GET -TimeoutSec 5
    $status = $healthResponse.status
    
    if ($status -eq "Healthy") {
        Write-Host " ✅ HEALTHY" -ForegroundColor Green
    }
    elseif ($status -eq "Degraded") {
        Write-Host " ⚠️ DEGRADED" -ForegroundColor Yellow
    }
    else {
        Write-Host " ❌ UNHEALTHY" -ForegroundColor Red
    }
    
    Write-Host ""
    Write-Host "  Timestamp: $($healthResponse.timestamp)"
    Write-Host "  Duration: $($healthResponse.duration)"
    Write-Host ""
    
    # Mostrar checks individuales
    foreach ($check in $healthResponse.checks) {
        $symbol = switch ($check.status) {
            "Healthy" { "✅" }
            "Degraded" { "⚠️" }
            "Unhealthy" { "❌" }
            default { "❓" }
        }
        
        Write-Host "  $symbol $($check.name): $($check.status) ($($check.duration))"
        
        if ($check.exception) {
            Write-Host "    Error: $($check.exception)" -ForegroundColor Red
        }
    }
}
catch {
    Write-Host " ❌ FAILED" -ForegroundColor Red
    Write-Host "  Error: $_" -ForegroundColor Red
}

Write-Host ""

# 3. Readiness Check
Write-Host "[CHECK 3] Readiness (DB + RabbitMQ)..." -NoNewline
try {
    $readyResponse = Invoke-RestMethod -Uri "$BaseUrl/health/ready" -Method GET -TimeoutSec 5
    if ($readyResponse.status -eq "Healthy") {
        Write-Host " ✅ READY" -ForegroundColor Green
    }
    else {
        Write-Host " ❌ NOT READY" -ForegroundColor Red
    }
}
catch {
    Write-Host " ❌ FAILED" -ForegroundColor Red
    Write-Host "  Error: $_" -ForegroundColor Red
}

# 4. Liveness Check
Write-Host "[CHECK 4] Liveness..." -NoNewline
try {
    $liveResponse = Invoke-RestMethod -Uri "$BaseUrl/health/live" -Method GET -TimeoutSec 5
    if ($liveResponse.status -eq "Healthy") {
        Write-Host " ✅ ALIVE" -ForegroundColor Green
    }
    else {
        Write-Host " ❌ NOT ALIVE" -ForegroundColor Red
    }
}
catch {
    Write-Host " ❌ FAILED" -ForegroundColor Red
    Write-Host "  Error: $_" -ForegroundColor Red
}

Write-Host ""
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host "Diagnosis complete." -ForegroundColor Cyan
Write-Host "=====================================================" -ForegroundColor Cyan
```

---

### ANALYZE-LOGS.ps1

```powershell
# ANALYZE-LOGS.ps1
# Analiza logs de Serilog buscando patrones de errores
param(
    [Parameter(Mandatory=$false)]
    [string]$LogPath = "logs",
    
    [Parameter(Mandatory=$false)]
    [int]$LastMinutes = 60,
    
    [Parameter(Mandatory=$false)]
    [string]$Level = "Error"
)

$ErrorActionPreference = "Continue"

Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host "  LOG ANALYSIS" -ForegroundColor Cyan
Write-Host "  Path: $LogPath" -ForegroundColor Cyan
Write-Host "  Last: $LastMinutes minutes" -ForegroundColor Cyan
Write-Host "  Level: $Level" -ForegroundColor Cyan
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host ""

# Encontrar archivo de log más reciente
$logFiles = Get-ChildItem -Path $LogPath -Filter "*.log" | Sort-Object LastWriteTime -Descending

if ($logFiles.Count -eq 0) {
    Write-Host "❌ No log files found in $LogPath" -ForegroundColor Red
    exit 1
}

$latestLog = $logFiles[0]
Write-Host "Analyzing: $($latestLog.Name)" -ForegroundColor Yellow
Write-Host ""

# Leer contenido
$content = Get-Content -Path $latestLog.FullName -Tail 10000

# Filtrar por tiempo
$cutoffTime = (Get-Date).AddMinutes(-$LastMinutes)
$recentLines = $content | Where-Object {
    if ($_ -match '\[([\d\-\s:\.]+)') {
        $timestamp = [DateTime]::Parse($matches[1])
        return $timestamp -ge $cutoffTime
    }
    return $false
}

# Filtrar por nivel
$filteredLines = $recentLines | Where-Object { $_ -match "\[$Level\]" }

Write-Host "Total $Level entries: $($filteredLines.Count)" -ForegroundColor Yellow
Write-Host ""

if ($filteredLines.Count -eq 0) {
    Write-Host "✅ No $Level entries found in last $LastMinutes minutes" -ForegroundColor Green
    exit 0
}

# Agrupar por TraceId
$traceIdPattern = 'TraceId=([a-f0-9]+)'
$errors = @{}

foreach ($line in $filteredLines) {
    if ($line -match $traceIdPattern) {
        $traceId = $matches[1]
        
        if (-not $errors.ContainsKey($traceId)) {
            $errors[$traceId] = @()
        }
        
        $errors[$traceId] += $line
    }
}

# Mostrar resumen
Write-Host "Unique TraceIds with errors: $($errors.Count)" -ForegroundColor Red
Write-Host ""

# Mostrar top 10 errores
$counter = 1
foreach ($traceId in ($errors.Keys | Select-Object -First 10)) {
    Write-Host "[$counter] TraceId: $traceId" -ForegroundColor Cyan
    
    foreach ($line in $errors[$traceId]) {
        Write-Host "  $line" -ForegroundColor Gray
    }
    
    Write-Host ""
    $counter++
}

if ($errors.Count -gt 10) {
    Write-Host "... and $($errors.Count - 10) more TraceIds with errors" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host "Analysis complete." -ForegroundColor Cyan
Write-Host "=====================================================" -ForegroundColor Cyan
```

---

### TRACE-REQUEST.ps1

```powershell
# TRACE-REQUEST.ps1
# Busca todos los logs relacionados con un TraceId específico
param(
    [Parameter(Mandatory=$true)]
    [string]$TraceId,
    
    [Parameter(Mandatory=$false)]
    [string]$LogPath = "logs"
)

Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host "  TRACE REQUEST ANALYSIS" -ForegroundColor Cyan
Write-Host "  TraceId: $TraceId" -ForegroundColor Cyan
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host ""

# Buscar en todos los logs
$logFiles = Get-ChildItem -Path $LogPath -Filter "*.log" | Sort-Object LastWriteTime -Descending

$found = $false

foreach ($logFile in $logFiles) {
    $matches = Get-Content -Path $logFile.FullName | Where-Object { $_ -match $TraceId }
    
    if ($matches.Count -gt 0) {
        $found = $true
        Write-Host "Found in: $($logFile.Name)" -ForegroundColor Yellow
        Write-Host ""
        
        foreach ($line in $matches) {
            # Colorear por nivel
            if ($line -match '\[ERR\]' -or $line -match '\[Error\]') {
                Write-Host $line -ForegroundColor Red
            }
            elseif ($line -match '\[WRN\]' -or $line -match '\[Warning\]') {
                Write-Host $line -ForegroundColor Yellow
            }
            elseif ($line -match '\[INF\]' -or $line -match '\[Information\]') {
                Write-Host $line -ForegroundColor Green
            }
            else {
                Write-Host $line -ForegroundColor Gray
            }
        }
        
        Write-Host ""
    }
}

if (-not $found) {
    Write-Host "❌ No logs found for TraceId: $TraceId" -ForegroundColor Red
}

Write-Host ""
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host "Trace analysis complete." -ForegroundColor Cyan
Write-Host "=====================================================" -ForegroundColor Cyan
```

---

## 🔍 QUERY JAEGER PARA TROUBLESHOOTING

### Acceder a Jaeger UI

```bash
# URL: http://localhost:16686

# Buscar traces:
# 1. Service: ErrorService
# 2. Operation: HTTP POST /api/errors
# 3. Tags: http.status_code=500
# 4. Min Duration: 1000ms (requests lentos)
```

### Ejemplo de Query con Jaeger API

```powershell
# GET-SLOW-TRACES.ps1
param(
    [Parameter(Mandatory=$false)]
    [string]$JaegerUrl = "http://localhost:16686",
    
    [Parameter(Mandatory=$false)]
    [string]$ServiceName = "ErrorService",
    
    [Parameter(Mandatory=$false)]
    [int]$MinDurationMs = 1000
)

$apiUrl = "$JaegerUrl/api/traces?service=$ServiceName&minDuration=${MinDurationMs}ms&limit=20"

Write-Host "Fetching slow traces (> ${MinDurationMs}ms)..." -ForegroundColor Yellow

$response = Invoke-RestMethod -Uri $apiUrl -Method GET

Write-Host "Found $($response.data.Count) slow traces:" -ForegroundColor Cyan
Write-Host ""

foreach ($trace in $response.data) {
    $traceId = $trace.traceID
    $duration = $trace.spans[0].duration / 1000  # Convert to ms
    $operation = $trace.spans[0].operationName
    
    Write-Host "TraceId: $traceId" -ForegroundColor Yellow
    Write-Host "  Duration: ${duration}ms" -ForegroundColor Red
    Write-Host "  Operation: $operation" -ForegroundColor Gray
    Write-Host "  URL: $JaegerUrl/trace/$traceId" -ForegroundColor Blue
    Write-Host ""
}
```

---

## ✅ CHECKLIST DE TROUBLESHOOTING

- [ ] Serilog configurado con TraceId y SpanId
- [ ] OpenTelemetry con Jaeger configurado
- [ ] Health checks implementados (/health, /health/ready, /health/live)
- [ ] Global Exception Middleware implementado
- [ ] Custom Activity Source para tracing
- [ ] Prometheus metrics endpoint expuesto
- [ ] Structured logging en todos los handlers
- [ ] Exception logging en todos los try-catch
- [ ] Scripts de diagnóstico creados:
  - [ ] CHECK-SERVICE-HEALTH.ps1
  - [ ] ANALYZE-LOGS.ps1
  - [ ] TRACE-REQUEST.ps1
- [ ] Logs rotan diariamente (retention 30 días)
- [ ] TraceId en response headers (para correlación)
- [ ] Alerting configurado en Grafana (opcional)

---

## 🎯 TROUBLESHOOTING COMÚN

### Problema: "No puedo encontrar logs de un request específico"

**Solución**:
1. Obtener TraceId del response header
2. Ejecutar: `.\TRACE-REQUEST.ps1 -TraceId "abc123..."`
3. Revisar logs correlacionados en orden cronológico

---

### Problema: "Servicio responde lento"

**Solución**:
1. Verificar health checks: `.\CHECK-SERVICE-HEALTH.ps1`
2. Buscar traces lentos en Jaeger: `.\GET-SLOW-TRACES.ps1 -MinDurationMs 1000`
3. Revisar métricas en Prometheus: http://localhost:9090
4. Analizar logs: `.\ANALYZE-LOGS.ps1 -Level Warning -LastMinutes 30`

---

### Problema: "Exception no logueada"

**Solución**:
1. Verificar GlobalExceptionMiddleware está registrado
2. Verificar try-catch tiene logging:
   ```csharp
   catch (Exception ex)
   {
       _logger.LogError(ex, "Context here");
       throw;
   }
   ```

---

## 📚 RECURSOS Y REFERENCIAS

- **Microservicio de Referencia**: `ErrorService/Program.cs` (Serilog + OpenTelemetry)
- **Serilog Documentation**: [Serilog](https://serilog.net/)
- **OpenTelemetry .NET**: [OpenTelemetry](https://opentelemetry.io/docs/instrumentation/net/)
- **Health Checks**: [Microsoft Docs](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks)
- **Jaeger UI**: [Jaeger](https://www.jaegertracing.io/)

---

**Fecha de Vigencia**: 2025-11-30  
**Aprobado por**: Equipo de Arquitectura CarDealer  
**Revisión**: Trimestral

**NOTA**: Troubleshooting inadecuado = debugging imposible en producción. TraceId/SpanId son OBLIGATORIOS.
