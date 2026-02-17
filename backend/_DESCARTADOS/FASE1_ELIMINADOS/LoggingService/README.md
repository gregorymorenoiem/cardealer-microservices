# LoggingService - Centralized Logging Service

## 📋 Descripción

El **LoggingService** es un servicio centralizado de agregación, consulta, **análisis y alerting** de logs para la arquitectura de microservicios. Utiliza **Seq** como plataforma de almacenamiento y análisis de logs estructurados, y **Serilog** como biblioteca de logging.

## ✨ Nuevas Características

### Sistema de Análisis Avanzado
- ✅ **Detección de patrones** automática (errores recurrentes, spikes, fallos de dependencias)
- ✅ **Detección de anomalías** en tiempo real
- ✅ **Métricas de salud** por servicio
- ✅ **Recomendaciones automáticas** basadas en análisis
- ✅ **Análisis de tendencias** y estadísticas avanzadas

### Sistema de Alerting Completo
- ✅ **Reglas de alertas** configurables con múltiples condiciones
- ✅ **Evaluación automática** periódica de reglas
- ✅ **Múltiples acciones** (Email, Webhook, Slack, Teams, PagerDuty, SMS)
- ✅ **Gestión de alertas** (acknowledge, resolve, historial)
- ✅ **Estadísticas de alertas** (MTTR, MTTA, alertas por regla)
- ✅ **Cooldown period** para evitar spam de alertas

## 🏗️ Arquitectura

### Clean Architecture

```
LoggingService/
├── LoggingService.Domain/          # Entidades de dominio
│   ├── LogEntry.cs                 # Entrada de log
│   ├── LogFilter.cs                # Filtro de consulta
│   ├── LogLevel.cs                 # Nivel de log
│   ├── LogStatistics.cs            # Estadísticas de logs
│   ├── LogPattern.cs               # ✨ Patrón detectado
│   ├── LogAnomaly.cs               # ✨ Anomalía detectada
│   ├── LogAnalysis.cs              # ✨ Resultado de análisis
│   ├── AlertRule.cs                # ✨ Regla de alerta
│   └── Alert.cs                    # ✨ Alerta disparada
├── LoggingService.Application/     # Lógica de aplicación (CQRS)
│   ├── Interfaces/
│   │   ├── ILogAggregator.cs      # Interfaz de agregación
│   │   ├── ILogAnalyzer.cs        # ✨ Interfaz de análisis
│   │   └── IAlertingService.cs    # ✨ Interfaz de alerting
│   ├── Queries/
│   │   └── LogQueries.cs          # Queries de MediatR
│   └── Handlers/
│       └── LogQueryHandlers.cs    # Handlers de MediatR
├── LoggingService.Infrastructure/  # Implementación de infraestructura
│   ├── Services/
│   │   ├── SeqLogAggregator.cs   # Cliente de Seq
│   │   ├── LogAnalyzer.cs        # ✨ Servicio de análisis
│   │   └── InMemoryAlertingService.cs # ✨ Servicio de alerting
│   └── DependencyInjection.cs
├── LoggingService.Api/             # API REST
│   ├── Controllers/
│   │   ├── LogsController.cs      # Controlador de logs
│   │   ├── AnalysisController.cs  # ✨ Controlador de análisis
│   │   └── AlertsController.cs    # ✨ Controlador de alertas
│   └── Services/
│       └── AlertEvaluationBackgroundService.cs # ✨ Evaluación automática
└── LoggingService.Tests/           # Tests unitarios
```

### Patrones Implementados

- **Clean Architecture**: Separación de capas por responsabilidad
- **CQRS**: Separación de comandos y consultas usando MediatR
- **Repository Pattern**: Abstracción del acceso a datos
- **Dependency Injection**: Inyección de dependencias

## 🚀 Características

### Core Features

- ✅ **Agregación centralizada** de logs de todos los microservicios
- ✅ **Consulta avanzada** con filtros múltiples
- ✅ **Logging estructurado** con Serilog
- ✅ **Correlación de requests** con RequestId, TraceId, SpanId
- ✅ **Estadísticas en tiempo real** de logs
- ✅ **Búsqueda por texto** en mensajes de log
- ✅ **Filtrado por nivel** de severidad
- ✅ **Paginación** de resultados

### ✨ Análisis Avanzado

- ✅ **Detección automática de patrones**
  - Errores recurrentes
  - Spikes de errores
  - Fallos de dependencias
  - Problemas de configuración
  
- ✅ **Detección de anomalías**
  - Tasa de error anormal
  - Patrones de error inusuales
  - Servicios no disponibles
  - Anomalías de recursos

- ✅ **Métricas de salud por servicio**
  - Estado de salud (Healthy, Degraded, Unhealthy, Critical)
  - Tasa de errores
  - Conteo de requests
  - Porcentaje de disponibilidad
  - Problemas actuales

- ✅ **Recomendaciones automáticas**
  - Basadas en patrones detectados
  - Prioridad (Low, Medium, High, Critical)
  - Acciones sugeridas
  - Enlaces a documentación

### ✨ Sistema de Alerting

- ✅ **Reglas de alerta configurables**
  - Múltiples tipos de condiciones
  - Umbrales personalizables
  - Ventanas de evaluación
  - Períodos de cooldown

- ✅ **Tipos de condiciones**
  - Error count (conteo de errores)
  - Error rate (porcentaje de errores)
  - Specific error (error específico)
  - Service down (servicio caído)
  - Performance degradation
  - Anomaly detected
  - Pattern match

- ✅ **Acciones múltiples**
  - Email notifications
  - Webhooks
  - Slack messages
  - Microsoft Teams
  - PagerDuty incidents
  - SMS
  - Ticket creation
  - Auto-scaling triggers

- ✅ **Gestión de alertas**
  - Acknowledge (reconocer)
  - Resolve (resolver)
  - Historial completo
  - Estados (Open, Acknowledged, Resolved)

- ✅ **Estadísticas de alerting**
  - MTTA (Mean Time To Acknowledge)
  - MTTR (Mean Time To Resolve)
  - Alertas por regla
  - Alertas por severidad
  - Reglas más disparadas

### Log Levels

```csharp
public enum LogLevel
{
    Trace = 0,         // Información de debugging muy detallada
    Debug = 1,         // Información de debugging
    Information = 2,   // Mensajes informativos generales
    Warning = 3,       // Advertencias
    Error = 4,         // Errores que no detienen la aplicación
    Critical = 5       // Errores críticos que requieren atención inmediata
}
```

### Correlation IDs

El servicio soporta tres tipos de IDs de correlación para rastreo distribuido:

- **RequestId**: Identificador único de la petición HTTP
- **TraceId**: Identificador de la traza distribuida (OpenTelemetry compatible)
- **SpanId**: Identificador del span dentro de la traza

## 📡 API Endpoints

### Log Management

#### GET /api/logs

Obtener logs con filtros opcionales.

**Query Parameters:**
- `startDate` (DateTime?): Fecha de inicio
- `endDate` (DateTime?): Fecha de fin
- `minLevel` (LogLevel?): Nivel mínimo de log
- `serviceName` (string?): Nombre del servicio
- `requestId` (string?): ID de request
- `traceId` (string?): ID de trace
- `userId` (string?): ID de usuario
- `searchText` (string?): Texto a buscar
- `hasException` (bool?): Filtrar logs con excepción
- `pageNumber` (int): Número de página (default: 1)
- `pageSize` (int): Tamaño de página (default: 100, max: 1000)

#### GET /api/logs/{id}

Obtener un log específico por ID.

#### GET /api/logs/statistics

Obtener estadísticas de logs.

---

### ✨ Analysis Endpoints

#### POST /api/analysis/analyze

Analizar logs dentro de un rango de tiempo.

**Query Parameters:**
- `startTime` (DateTime?): Fecha de inicio (default: last 24h)
- `endTime` (DateTime?): Fecha de fin (default: now)

**Response:**
```json
{
  "id": "analysis-123",
  "startTime": "2024-01-01T00:00:00Z",
  "endTime": "2024-01-02T00:00:00Z",
  "statistics": { /* LogStatistics */ },
  "detectedPatterns": [ /* LogPattern[] */ ],
  "detectedAnomalies": [ /* LogAnomaly[] */ ],
  "serviceHealth": {
    "AuthService": {
      "serviceName": "AuthService",
      "status": "Healthy",
      "errorRate": 2.5,
      "requestCount": 1000,
      "errorCount": 25
    }
  },
  "recommendations": [ /* Recommendation[] */ ],
  "summary": {
    "totalLogsAnalyzed": 50000,
    "criticalIssuesFound": 2,
    "warningsFound": 100,
    "patternsDetected": 5,
    "anomaliesDetected": 3,
    "overallSystemHealth": 85.5
  }
}
```

#### GET /api/analysis/patterns

Obtener patrones detectados.

**Response:**
```json
[
  {
    "id": "pattern-123",
    "name": "Recurring Error: Database timeout",
    "pattern": "Database connection timeout",
    "type": "RecurringError",
    "occurrenceCount": 50,
    "firstSeen": "2024-01-01T10:00:00Z",
    "lastSeen": "2024-01-01T18:00:00Z",
    "affectedServices": ["AuthService", "MediaService"]
  }
]
```

#### GET /api/analysis/anomalies

Obtener anomalías detectadas.

**Response:**
```json
[
  {
    "id": "anomaly-123",
    "title": "High Error Rate Detected",
    "description": "Error rate is 15.5%, which exceeds threshold",
    "type": "ErrorRateSpike",
    "severity": "High",
    "confidence": 95,
    "serviceName": "AuthService",
    "detectedAt": "2024-01-01T12:00:00Z",
    "isResolved": false
  }
]
```

#### GET /api/analysis/service-health

Obtener métricas de salud de servicios.

#### GET /api/analysis/recommendations

Obtener recomendaciones basadas en análisis.

#### GET /api/analysis/summary

Obtener resumen de análisis.

---

### ✨ Alerting Endpoints

#### Alert Rules Management

##### POST /api/alerts/rules

Crear una nueva regla de alerta.

**Request Body:**
```json
{
  "name": "High Error Rate Alert",
  "description": "Alert when error rate exceeds 10%",
  "isEnabled": true,
  "condition": {
    "type": "ErrorRate",
    "serviceName": "AuthService",
    "minLevel": "Error",
    "errorRateThreshold": 10.0
  },
  "actions": [
    {
      "type": "Email",
      "priority": 5,
      "configuration": {
        "Recipients": "ops@company.com",
        "Subject": "High Error Rate Detected"
      }
    },
    {
      "type": "Slack",
      "priority": 4,
      "configuration": {
        "Channel": "#alerts",
        "WebhookUrl": "https://hooks.slack.com/..."
      }
    }
  ],
  "evaluationWindow": "00:05:00",
  "cooldownPeriod": "00:15:00"
}
```

##### GET /api/alerts/rules

Obtener todas las reglas de alerta.

##### GET /api/alerts/rules/{id}

Obtener una regla específica.

##### PUT /api/alerts/rules/{id}

Actualizar una regla.

##### DELETE /api/alerts/rules/{id}

Eliminar una regla.

##### POST /api/alerts/rules/{id}/enable

Habilitar una regla.

##### POST /api/alerts/rules/{id}/disable

Deshabilitar una regla.

##### POST /api/alerts/rules/{id}/evaluate

Evaluar una regla manualmente.

##### POST /api/alerts/evaluate-all

Evaluar todas las reglas (útil para testing).

#### Alert Management

##### GET /api/alerts

Obtener alertas con filtros opcionales.

**Query Parameters:**
- `status` (AlertStatus?): Open, Acknowledged, Resolved
- `since` (DateTime?): Alertas desde esta fecha

##### GET /api/alerts/{id}

Obtener una alerta específica.

##### POST /api/alerts/{id}/acknowledge

Reconocer una alerta.

**Request Body:**
```json
{
  "userId": "user-123"
}
```

##### POST /api/alerts/{id}/resolve

Resolver una alerta.

**Request Body:**
```json
{
  "userId": "user-123",
  "notes": "Fixed by restarting the service"
}
```

##### GET /api/alerts/statistics

Obtener estadísticas de alertas.

**Response:**
```json
{
  "totalAlerts": 150,
  "openAlerts": 10,
  "acknowledgedAlerts": 20,
  "resolvedAlerts": 120,
  "alertsByRule": {
    "High Error Rate Alert": 50,
    "Service Down Alert": 30
  },
  "alertsBySeverity": {
    "Critical": 10,
    "Error": 40,
    "Warning": 80,
    "Info": 20
  },
  "averageTimeToAcknowledge": 5.2,
  "averageTimeToResolve": 25.8,
  "mostTriggeredRules": [
    "High Error Rate Alert",
    "Service Down Alert"
  ]
}
```

---

## 💡 Ejemplos de Uso

### Análisis de Logs

```csharp
// Análisis completo del último día
var analysis = await _logAnalyzer.AnalyzeLogsAsync(
    DateTime.UtcNow.AddDays(-1),
    DateTime.UtcNow
);

Console.WriteLine($"Logs analizados: {analysis.Summary.TotalLogsAnalyzed}");
Console.WriteLine($"Salud del sistema: {analysis.Summary.OverallSystemHealth}%");
Console.WriteLine($"Patrones detectados: {analysis.DetectedPatterns.Count}");
Console.WriteLine($"Anomalías detectadas: {analysis.DetectedAnomalies.Count}");

// Revisar servicios no saludables
foreach (var (serviceName, health) in analysis.ServiceHealth)
{
    if (health.Status != HealthStatus.Healthy)
    {
        Console.WriteLine($"⚠️ {serviceName}: {health.Status}");
        Console.WriteLine($"   Error Rate: {health.ErrorRate}%");
        Console.WriteLine($"   Issues: {string.Join(", ", health.CurrentIssues)}");
    }
}

// Revisar patrones críticos
var criticalPatterns = analysis.DetectedPatterns
    .Where(p => p.Type == PatternType.ErrorSpike || p.Type == PatternType.RecurringError)
    .ToList();

foreach (var pattern in criticalPatterns)
{
    Console.WriteLine($"🔍 {pattern.Name}");
    Console.WriteLine($"   Ocurrencias: {pattern.OccurrenceCount}");
    Console.WriteLine($"   Frecuencia: {pattern.GetFrequencyPerHour()}/hora");
}

// Aplicar recomendaciones
foreach (var rec in analysis.Recommendations.Where(r => r.Priority >= RecommendationPriority.High))
{
    Console.WriteLine($"💡 {rec.Title} [{rec.Priority}]");
    Console.WriteLine($"   {rec.Description}");
    foreach (var action in rec.ActionItems)
    {
        Console.WriteLine($"   - {action}");
    }
}
```

### Gestión de Reglas de Alerta

```csharp
// 1. Regla para alta tasa de errores
var highErrorRateRule = new AlertRule
{
    Name = "High Error Rate - AuthService",
    Description = "Alert when AuthService error rate exceeds 10%",
    IsEnabled = true,
    Condition = new RuleCondition
    {
        Type = ConditionType.ErrorRate,
        ServiceName = "AuthService",
        MinLevel = LogLevel.Error,
        ErrorRateThreshold = 10.0
    },
    Actions = new List<AlertAction>
    {
        new()
        {
            Type = ActionType.Email,
            Priority = 5,
            Configuration = new Dictionary<string, string>
            {
                ["Recipients"] = "ops-team@company.com",
                ["Subject"] = "🚨 High Error Rate in AuthService"
            }
        },
        new()
        {
            Type = ActionType.Slack,
            Priority = 4,
            Configuration = new Dictionary<string, string>
            {
                ["Channel"] = "#alerts-critical",
                ["WebhookUrl"] = "https://hooks.slack.com/services/YOUR/WEBHOOK/URL"
            }
        }
    },
    EvaluationWindow = TimeSpan.FromMinutes(5),
    CooldownPeriod = TimeSpan.FromMinutes(15)
};

await _alertingService.CreateRuleAsync(highErrorRateRule);

// 2. Regla para servicio caído
var serviceDownRule = new AlertRule
{
    Name = "Service Down - MediaService",
    Description = "Alert when MediaService stops responding",
    IsEnabled = true,
    Condition = new RuleCondition
    {
        Type = ConditionType.ServiceDown,
        ServiceName = "MediaService"
    },
    Actions = new List<AlertAction>
    {
        new()
        {
            Type = ActionType.PagerDuty,
            Priority = 5,
            Configuration = new Dictionary<string, string>
            {
                ["ServiceKey"] = "your-pagerduty-service-key",
                ["IncidentKey"] = "mediaservice-down"
            }
        }
    },
    EvaluationWindow = TimeSpan.FromMinutes(2),
    CooldownPeriod = TimeSpan.FromMinutes(10)
};

await _alertingService.CreateRuleAsync(serviceDownRule);

// 3. Regla para error específico
var specificErrorRule = new AlertRule
{
    Name = "Database Connection Timeout",
    Description = "Alert on database connection timeouts",
    IsEnabled = true,
    Condition = new RuleCondition
    {
        Type = ConditionType.SpecificError,
        MessagePattern = "database.*connection.*timeout",
        MinLevel = LogLevel.Error,
        ErrorCountThreshold = 5
    },
    Actions = new List<AlertAction>
    {
        new()
        {
            Type = ActionType.Teams,
            Priority = 3,
            Configuration = new Dictionary<string, string>
            {
                ["WebhookUrl"] = "https://outlook.office.com/webhook/YOUR/WEBHOOK/URL",
                ["Title"] = "Database Connection Issues"
            }
        }
    },
    EvaluationWindow = TimeSpan.FromMinutes(5),
    CooldownPeriod = TimeSpan.FromMinutes(20)
};

await _alertingService.CreateRuleAsync(specificErrorRule);
```

### Gestión de Alertas

```csharp
// Obtener alertas abiertas
var openAlerts = await _alertingService.GetAlertsAsync(AlertStatus.Open);

foreach (var alert in openAlerts)
{
    Console.WriteLine($"Alert: {alert.Title} [{alert.Severity}]");
    Console.WriteLine($"  Message: {alert.Message}");
    Console.WriteLine($"  Triggered: {alert.TriggeredAt}");
    Console.WriteLine($"  Age: {alert.GetAge().TotalMinutes:F1} minutes");
}

// Reconocer una alerta
await _alertingService.AcknowledgeAlertAsync(alert.Id, "ops-user-123");

// Resolver una alerta
await _alertingService.ResolveAlertAsync(
    alert.Id,
    "ops-user-123",
    "Issue resolved by restarting the service"
);

// Obtener estadísticas
var stats = await _alertingService.GetAlertStatisticsAsync();
Console.WriteLine($"Total Alerts: {stats.TotalAlerts}");
Console.WriteLine($"Open: {stats.OpenAlerts}");
Console.WriteLine($"MTTA: {stats.AverageTimeToAcknowledge:F1} minutes");
Console.WriteLine($"MTTR: {stats.AverageTimeToResolve:F1} minutes");
Console.WriteLine($"Most Triggered: {string.Join(", ", stats.MostTriggeredRules)}");
```

### Uso de la API REST

```bash
# Análisis completo
curl -X POST "http://localhost:5000/api/analysis/analyze?startTime=2024-01-01T00:00:00Z&endTime=2024-01-02T00:00:00Z"

# Obtener patrones detectados
curl "http://localhost:5000/api/analysis/patterns?startTime=2024-01-01T00:00:00Z"

# Obtener anomalías
curl "http://localhost:5000/api/analysis/anomalies"

# Obtener salud de servicios
curl "http://localhost:5000/api/analysis/service-health"

# Crear regla de alerta
curl -X POST "http://localhost:5000/api/alerts/rules" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "High Error Rate",
    "isEnabled": true,
    "condition": {
      "type": "ErrorRate",
      "errorRateThreshold": 10.0
    },
    "actions": [{
      "type": "Email",
      "priority": 5,
      "configuration": {
        "Recipients": "ops@company.com"
      }
    }],
    "evaluationWindow": "00:05:00",
    "cooldownPeriod": "00:15:00"
  }'

# Obtener todas las alertas
curl "http://localhost:5000/api/alerts"

# Reconocer una alerta
curl -X POST "http://localhost:5000/api/alerts/{alertId}/acknowledge" \
  -H "Content-Type: application/json" \
  -d '{"userId": "user-123"}'

# Obtener estadísticas
curl "http://localhost:5000/api/alerts/statistics"
```

---

## 🔧 Configuración

### appsettings.json

```json
{
  "Seq": {
    "Url": "http://localhost:5341"
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    }
  },
  "Alerting": {
    "EvaluationIntervalMinutes": 5
  }
}
```

### Variables de Entorno

- `SEQ_URL`: URL del servidor Seq (default: http://localhost:5341)
- `ALERTING_EVALUATION_INTERVAL`: Intervalo de evaluación de reglas en minutos (default: 5)

### Docker Compose

```yaml
seq:
  image: datalust/seq:latest
  container_name: seq
  environment:
    ACCEPT_EULA: "Y"
  ports:
    - "5341:80"      # Web UI
    - "5342:5341"    # Ingestion
  volumes:
    - seq_data:/data

loggingservice:
  build:
    context: ./LoggingService
    dockerfile: Dockerfile
  environment:
    Seq__Url: "http://seq:80"
    Alerting__EvaluationIntervalMinutes: "5"
  ports:
    - "5096:80"
  depends_on:
    - seq
```

## 📦 Integración con otros servicios

Para integrar Serilog + Seq en otros microservicios:

### 1. Instalar NuGet Packages

```bash
dotnet add package Serilog.AspNetCore --version 10.0.0
dotnet add package Serilog.Sinks.Seq --version 9.0.0
```

### 2. Configurar Program.cs

```csharp
using Serilog;

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.WithProperty("ServiceName", "YourServiceName")
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Seq(builder.Configuration["Seq:Url"] ?? "http://localhost:5341")
    .CreateLogger();

builder.Host.UseSerilog();

app.UseSerilogRequestLogging();
```

### 3. Usar logging en código

```csharp
public class MyService
{
    private readonly ILogger<MyService> _logger;

    public MyService(ILogger<MyService> logger)
    {
        _logger = logger;
    }

    public async Task DoSomethingAsync()
    {
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["UserId"] = "user-123",
            ["RequestId"] = "req-456"
        }))
        {
            _logger.LogInformation("Starting operation");
            
            try
            {
                // Código...
                _logger.LogInformation("Operation completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Operation failed");
                throw;
            }
        }
    }
}
```

## 🧪 Testing

### Ejecutar Tests

```bash
dotnet test LoggingService.sln
```

### Cobertura de Tests

- ✅ **18 tests unitarios** (100% passing)
- ✅ Tests de dominio (LogEntry, LogFilter, LogStatistics)
- ✅ Tests de validación
- ✅ Tests de cálculos estadísticos

## 📊 Seq Dashboard

### Acceso

Una vez iniciado el contenedor de Seq, acceder a:

```
http://localhost:5341
```

### Búsquedas útiles

```sql
-- Logs de error en las últimas 24 horas
Level = 'Error' OR Level = 'Fatal'

-- Logs de un servicio específico
ServiceName = 'AuthService'

-- Logs con excepción
Exception IS NOT NULL

-- Logs de un usuario específico
UserId = 'user-123'

-- Logs de una traza específica
TraceId = 'trace-456'

-- Logs por request
RequestId = 'req-789'
```

## 🐛 Troubleshooting

### Seq no está disponible

Si los logs no aparecen en Seq:

1. Verificar que el contenedor de Seq esté corriendo:
   ```bash
   docker ps | grep seq
   ```

2. Verificar la configuración de Seq en appsettings.json

3. Verificar logs del contenedor de Seq:
   ```bash
   docker logs seq
   ```

### Logs no se están enviando

1. Verificar que Serilog esté configurado correctamente en Program.cs
2. Verificar que la URL de Seq sea correcta
3. Verificar que el servicio tenga acceso de red a Seq

## 📈 Métricas y Monitoreo

### Health Check

```
GET http://localhost:5096/health
```

### Estadísticas de uso

Usar el endpoint `/api/logs/statistics` para obtener:
- Total de logs por servicio
- Tasa de errores
- Servicio más activo
- Rango de tiempo de logs

## 🔐 Seguridad

### Recomendaciones

- ✅ **No logear información sensible** (passwords, tokens, PII)
- ✅ **Usar Seq API Keys** en producción
- ✅ **Habilitar HTTPS** para Seq
- ✅ **Configurar retención** de logs en Seq
- ✅ **Implementar rate limiting** en el API

### Variables de entorno sensibles

En producción, usar variables de entorno para:
- `Seq__ApiKey`
- `Seq__Url`

## 📝 Roadmap

- [ ] Implementar alertas automáticas
- [ ] Soporte para OpenTelemetry
- [ ] Dashboard personalizado
- [ ] Exportación de logs a diferentes formatos
- [ ] Integración con sistemas de monitoreo (Grafana, Prometheus)

## 📚 Referencias

- [Serilog Documentation](https://serilog.net/)
- [Seq Documentation](https://docs.datalust.co/docs)
- [OpenTelemetry](https://opentelemetry.io/)
- [Structured Logging](https://stackify.com/what-is-structured-logging-and-why-developers-need-it/)

---

**Servicio #4** del roadmap de servicios transversales - ✅ Completado
