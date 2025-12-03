# ❌ ErrorService

Servicio centralizado de gestión y tracking de errores para el sistema CarDealer.

## 📋 Descripción

Microservicio que captura, procesa, almacena y reporta errores de todos los servicios del sistema, proporcionando análisis y alertas en tiempo real.

## 🚀 Características

- **Error Tracking**: Captura de excepciones y errores
- **Aggregation**: Agrupación inteligente de errores similares
- **Alerting**: Notificaciones en tiempo real
- **Stack Trace Analysis**: Análisis de stack traces
- **Error Rate Monitoring**: Detección de anomalías
- **Historical Trends**: Análisis de tendencias
- **Context Capture**: Request headers, user info, environment
- **Integration**: Sentry-like functionality

## 🏗️ Arquitectura

```
ErrorService.Api (Puerto 5001)
├── Controllers/
│   └── ErrorController.cs
├── ErrorService.Application/
│   ├── Commands/
│   │   └── LogErrorCommand
│   ├── Queries/
│   │   ├── GetErrorsQuery
│   │   ├── GetErrorByIdQuery
│   │   └── GetErrorStatsQuery
│   └── Services/
│       ├── ErrorAggregator
│       └── ErrorAnalyzer
├── ErrorService.Domain/
│   ├── Entities/
│   │   ├── ErrorLog
│   │   └── ErrorOccurrence
│   ├── Enums/
│   │   ├── ErrorSeverity
│   │   └── ErrorStatus
│   └── ValueObjects/
└── ErrorService.Infrastructure/
    ├── Data/
    ├── Repositories/
    ├── MessageBus/
    │   └── ErrorEventConsumer
    └── Alerting/
        └── AlertService
```

## 📦 Dependencias Principales

- **Entity Framework Core 8.0**
- **RabbitMQ.Client 6.8.1** - Message bus
- **MediatR 12.2.0** - CQRS
- **Serilog** - Structured logging
- **StackExchange.Redis** - Caching

## ⚙️ Configuración

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=errordb;..."
  },
  "RabbitMQ": {
    "Host": "localhost",
    "QueueName": "error-events"
  },
  "Alerting": {
    "ThresholdErrorsPerMinute": 10,
    "CriticalErrorsNotifyImmediately": true,
    "SlackWebhook": "https://hooks.slack.com/..."
  },
  "Retention": {
    "DaysToKeep": 90,
    "AggregateAfterDays": 30
  }
}
```

## 🔌 API Endpoints

### Logging
```http
POST /api/errors                    # Registrar error
POST /api/errors/batch              # Registrar múltiples errores
```

### Queries
```http
GET /api/errors                     # Listar errores
GET /api/errors/{id}                # Detalle de error
GET /api/errors/search              # Búsqueda avanzada
GET /api/errors/grouped             # Errores agrupados
```

### Statistics
```http
GET /api/errors/stats               # Estadísticas generales
GET /api/errors/stats/service/{serviceName}
GET /api/errors/stats/timeline      # Serie temporal
GET /api/errors/stats/top-errors    # Top errores frecuentes
```

### Management
```http
PUT    /api/errors/{id}/status      # Marcar como resuelto/ignorado
POST   /api/errors/{id}/assign      # Asignar a desarrollador
DELETE /api/errors/{id}             # Eliminar error
```

## 📝 Ejemplos de Uso

### Registrar Error
```bash
curl -X POST http://localhost:5001/api/errors \
  -H "Content-Type: application/json" \
  -d '{
    "message": "Failed to connect to database",
    "exceptionType": "System.Data.SqlClient.SqlException",
    "stackTrace": "at CarDealer.VehicleService...",
    "severity": "High",
    "serviceName": "VehicleService",
    "environment": "Production",
    "userId": "user-123",
    "requestPath": "/api/vehicles",
    "requestMethod": "GET",
    "statusCode": 500,
    "context": {
      "vehicleId": "vehicle-456",
      "action": "GetVehicleDetails"
    }
  }'
```

**Respuesta**:
```json
{
  "errorId": "error-789",
  "message": "Error logged successfully",
  "groupId": "group-abc",
  "isNewError": false,
  "occurrenceCount": 15
}
```

### Buscar Errores
```bash
curl -X GET "http://localhost:5001/api/errors/search?severity=High&serviceName=VehicleService&from=2024-01-01&to=2024-01-31"
```

### Obtener Estadísticas
```bash
curl -X GET http://localhost:5001/api/errors/stats/timeline?period=24h
```

**Respuesta**:
```json
{
  "totalErrors": 1245,
  "errorsByHour": [
    {"hour": "2024-01-15T00:00:00Z", "count": 45},
    {"hour": "2024-01-15T01:00:00Z", "count": 52},
    ...
  ],
  "topErrors": [
    {
      "groupId": "group-abc",
      "message": "Database connection failed",
      "count": 234,
      "firstSeen": "2024-01-14T08:00:00Z",
      "lastSeen": "2024-01-15T23:45:00Z"
    }
  ]
}
```

### Marcar Error como Resuelto
```bash
curl -X PUT http://localhost:5001/api/errors/error-789/status \
  -H "Content-Type: application/json" \
  -d '{
    "status": "Resolved",
    "resolution": "Fixed database connection pool timeout in v2.1.5"
  }'
```

## 📊 Modelo de Datos

### ErrorLog Entity
```csharp
public class ErrorLog
{
    public Guid Id { get; set; }
    public string GroupId { get; set; }  // Agrupación de errores similares
    public string Message { get; set; }
    public string ExceptionType { get; set; }
    public string StackTrace { get; set; }
    public ErrorSeverity Severity { get; set; }  // Low, Medium, High, Critical
    public string ServiceName { get; set; }
    public string Environment { get; set; }  // Dev, Staging, Production
    public DateTime OccurredAt { get; set; }
    
    // Request Context
    public string RequestPath { get; set; }
    public string RequestMethod { get; set; }
    public int? StatusCode { get; set; }
    public string UserId { get; set; }
    public string IpAddress { get; set; }
    public string UserAgent { get; set; }
    
    // Additional Context (JSON)
    public string Context { get; set; }
    
    // Management
    public ErrorStatus Status { get; set; }  // New, InProgress, Resolved, Ignored
    public string AssignedTo { get; set; }
    public string Resolution { get; set; }
}
```

### Error Grouping
Los errores se agrupan por:
- ExceptionType
- Message (similar al 80%)
- StackTrace (primeras 5 líneas)
- ServiceName

## 🔔 Alerting

### Slack Integration
```json
{
  "text": "🚨 CRITICAL ERROR in VehicleService",
  "attachments": [
    {
      "color": "danger",
      "fields": [
        {"title": "Error", "value": "NullReferenceException", "short": true},
        {"title": "Count", "value": "15 occurrences in 5 minutes", "short": true},
        {"title": "Environment", "value": "Production", "short": true}
      ]
    }
  ]
}
```

### Email Alerts
- **Critical errors**: Inmediato
- **High severity**: Cada 15 minutos (digest)
- **Error rate spike**: Cuando supera threshold

### Alert Rules
```yaml
rules:
  - name: high_error_rate
    condition: errors_per_minute > 10
    severity: warning
    
  - name: critical_error
    condition: severity == "Critical"
    severity: critical
    notify: immediately
    
  - name: new_error_type
    condition: is_new_error == true && severity >= "High"
    severity: info
```

## 🧪 Testing

```bash
# Tests unitarios
dotnet test ErrorService.Tests/

# Tests de integración
dotnet test --filter "Category=Integration"
```

## 🐳 Docker

```bash
# Build
docker build -t errorservice:latest .

# Run
docker run -d -p 5001:80 \
  -e ConnectionStrings__DefaultConnection="..." \
  -e RabbitMQ__Host="rabbitmq" \
  --name errorservice \
  errorservice:latest
```

## 📊 Base de Datos

### Tablas
- `ErrorLogs` - Registro de errores
- `ErrorOccurrences` - Ocurrencias individuales (agregadas)
- `ErrorAssignments` - Asignaciones a desarrolladores
- `ErrorResolutions` - Resoluciones

### Índices
```sql
CREATE INDEX IX_ErrorLogs_GroupId ON ErrorLogs(GroupId);
CREATE INDEX IX_ErrorLogs_ServiceName_Severity ON ErrorLogs(ServiceName, Severity);
CREATE INDEX IX_ErrorLogs_OccurredAt ON ErrorLogs(OccurredAt DESC);
CREATE INDEX IX_ErrorLogs_Status ON ErrorLogs(Status);
```

### Partitioning
```sql
-- Particionar por mes para performance
CREATE TABLE ErrorLogs_2024_01 PARTITION OF ErrorLogs
FOR VALUES FROM ('2024-01-01') TO ('2024-02-01');
```

## 📈 Monitoreo

### Métricas
- `errors_logged_total` - Total errores registrados
- `errors_by_severity` - Errores por severidad
- `error_rate_per_minute` - Tasa de errores
- `error_groups_active` - Grupos de errores activos
- `errors_resolved_total` - Errores resueltos

### Dashboards
- Error rate timeline (últimas 24h)
- Top 10 errores frecuentes
- Errores por servicio
- Errores por severidad
- MTTR (Mean Time To Resolution)

## 🔄 Event Processing

### RabbitMQ Consumer
```csharp
// Otros servicios publican errores al bus
try
{
    // ... código
}
catch (Exception ex)
{
    await _messageBus.PublishAsync("error-events", new ErrorEvent
    {
        Message = ex.Message,
        ExceptionType = ex.GetType().FullName,
        StackTrace = ex.StackTrace,
        ServiceName = "VehicleService"
    });
}
```

## 🎯 Client Library

### NuGet Package: CarDealer.ErrorService.Client
```csharp
// Configuración en Startup
services.AddErrorServiceClient(options =>
{
    options.BaseUrl = "http://errorservice";
    options.ServiceName = "VehicleService";
    options.Environment = "Production";
});

// Uso global con middleware
app.UseErrorLogging();

// Uso manual
public class MyService
{
    private readonly IErrorServiceClient _errorClient;
    
    public async Task DoSomething()
    {
        try
        {
            // ... código
        }
        catch (Exception ex)
        {
            await _errorClient.LogErrorAsync(ex);
            throw;
        }
    }
}
```

## 🚦 Estado

- ✅ **Build**: OK
- ✅ **Tests**: 100% pasando
- ✅ **Docker**: Configurado
- ✅ **Alerting**: Implementado

---

**Puerto**: 5001  
**Base de Datos**: PostgreSQL (errordb)  
**Message Queue**: RabbitMQ (error-events)  
**Estado**: ✅ Production Ready
