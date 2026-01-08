# ErrorService - Context Documentation

## 📋 INFORMACIÓN GENERAL

- **Nombre del Servicio:** ErrorService
- **Puerto en Kubernetes:** 8080
- **Puerto en Desarrollo:** 5008
- **Estado:** ✅ **EN PRODUCCIÓN**
- **Base de Datos:** PostgreSQL (`errorservice`)
- **Imagen Docker:** ghcr.io/gregorymorenoiem/cardealer-errorservice:latest

### Propósito
Servicio centralizado de registro y gestión de errores y excepciones. Captura errores de todos los microservicios, los almacena, agrupa, notifica y proporciona dashboards para monitoreo y debugging.

---

## 🏗️ ARQUITECTURA

```
ErrorService/
├── ErrorService.Api/
│   ├── Controllers/
│   │   ├── ErrorsController.cs
│   │   ├── ErrorGroupsController.cs
│   │   └── ErrorReportsController.cs
│   └── Program.cs
├── ErrorService.Application/
│   ├── Features/
│   │   ├── Commands/
│   │   │   ├── LogErrorCommand.cs
│   │   │   ├── MarkErrorResolvedCommand.cs
│   │   │   └── CreateErrorGroupCommand.cs
│   │   └── Queries/
│   │       ├── GetErrorByIdQuery.cs
│   │       ├── SearchErrorsQuery.cs
│   │       └── GetErrorStatisticsQuery.cs
│   └── DTOs/
├── ErrorService.Domain/
│   ├── Entities/
│   │   ├── Error.cs
│   │   ├── ErrorGroup.cs
│   │   ├── ErrorOccurrence.cs
│   │   └── ErrorAlert.cs
│   ├── Enums/
│   │   ├── ErrorSeverity.cs
│   │   └── ErrorStatus.cs
│   └── Interfaces/
│       ├── IErrorRepository.cs
│       └── IErrorGroupingService.cs
└── ErrorService.Infrastructure/
    ├── Services/
    │   ├── ErrorGroupingService.cs
    │   ├── ErrorNotificationService.cs
    │   └── ErrorAnalyticsService.cs
    └── BackgroundServices/
        └── ErrorAlertWorker.cs
```

---

## 📦 ENTIDADES

### Error
```csharp
public class Error
{
    public Guid Id { get; set; }
    
    // Origen del error
    public string ServiceName { get; set; }         // "VehiclesSaleService", "AuthService"
    public string Environment { get; set; }         // "Production", "Development"
    public string? HostName { get; set; }
    public string? IpAddress { get; set; }
    
    // Detalles del error
    public string Message { get; set; }
    public string? ExceptionType { get; set; }      // "NullReferenceException"
    public string? StackTrace { get; set; }
    public string? InnerException { get; set; }
    
    // Contexto
    public string? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string? RequestPath { get; set; }
    public string? HttpMethod { get; set; }
    public int? StatusCode { get; set; }
    public string? UserAgent { get; set; }
    
    // Severidad y Estado
    public ErrorSeverity Severity { get; set; }     // Critical, Error, Warning, Info
    public ErrorStatus Status { get; set; }         // New, InProgress, Resolved, Ignored
    
    // Metadata
    public string? AdditionalData { get; set; }     // JSON con datos extra
    public DateTime OccurredAt { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Agrupación
    public Guid? ErrorGroupId { get; set; }
    public ErrorGroup? ErrorGroup { get; set; }
    
    // Resolución
    public DateTime? ResolvedAt { get; set; }
    public Guid? ResolvedBy { get; set; }
    public string? ResolutionNotes { get; set; }
}
```

### ErrorGroup
```csharp
public class ErrorGroup
{
    public Guid Id { get; set; }
    
    // Identificación
    public string GroupHash { get; set; }           // Hash para agrupar errores similares
    public string Title { get; set; }               // Resumen del error
    public string? ExceptionType { get; set; }
    
    // Estadísticas
    public int OccurrenceCount { get; set; }
    public int AffectedUsersCount { get; set; }
    public DateTime FirstOccurrence { get; set; }
    public DateTime LastOccurrence { get; set; }
    
    // Estado
    public ErrorStatus Status { get; set; }
    public ErrorSeverity MaxSeverity { get; set; }
    
    // Resolución
    public DateTime? ResolvedAt { get; set; }
    public Guid? AssignedTo { get; set; }
    public string? ResolutionNotes { get; set; }
    
    // Navegación
    public ICollection<Error> Errors { get; set; }
}
```

### ErrorAlert
```csharp
public class ErrorAlert
{
    public Guid Id { get; set; }
    
    // Regla de alerta
    public string Name { get; set; }
    public string? Description { get; set; }
    
    // Condiciones
    public string? ServiceName { get; set; }        // Null = todos los servicios
    public ErrorSeverity? MinSeverity { get; set; }
    public int? OccurrenceThreshold { get; set; }   // Alertar después de N ocurrencias
    public TimeSpan? TimeWindow { get; set; }       // En los últimos X minutos
    
    // Notificaciones
    public bool SendEmail { get; set; }
    public string? EmailRecipients { get; set; }    // Comma-separated
    public bool SendSlack { get; set; }
    public string? SlackWebhook { get; set; }
    
    // Estado
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastTriggeredAt { get; set; }
}
```

---

## 📡 ENDPOINTS API

### Registro de Errores

#### POST `/api/errors`
Registrar nuevo error.

**Request:**
```json
{
  "serviceName": "VehiclesSaleService",
  "environment": "Production",
  "message": "Object reference not set to an instance of an object",
  "exceptionType": "NullReferenceException",
  "stackTrace": "   at VehiclesSaleService.Controllers...",
  "severity": "Error",
  "requestPath": "/api/vehicles/123",
  "httpMethod": "GET",
  "statusCode": 500,
  "userId": "...",
  "additionalData": "{\"vehicleId\": \"123\"}"
}
```

**Response (201 Created):**
```json
{
  "errorId": "...",
  "errorGroupId": "...",
  "status": "New",
  "occurrenceCount": 15,
  "message": "Error registrado exitosamente"
}
```

#### POST `/api/errors/batch`
Registrar múltiples errores (para batch processing).

### Consulta de Errores

#### GET `/api/errors`
Buscar errores con filtros.

**Query Parameters:**
- `serviceName`: Filtrar por servicio
- `severity`: Filtrar por severidad
- `status`: Filtrar por estado
- `from`: Fecha desde
- `to`: Fecha hasta
- `search`: Búsqueda en mensaje
- `page`: Número de página
- `pageSize`: Tamaño de página

**Response (200 OK):**
```json
{
  "items": [
    {
      "id": "...",
      "serviceName": "VehiclesSaleService",
      "message": "Object reference not set...",
      "exceptionType": "NullReferenceException",
      "severity": "Error",
      "status": "New",
      "occurrenceCount": 15,
      "firstOccurrence": "2026-01-07T08:00:00Z",
      "lastOccurrence": "2026-01-07T10:30:00Z"
    }
  ],
  "totalCount": 150,
  "page": 1,
  "pageSize": 20
}
```

#### GET `/api/errors/{id}`
Obtener detalle completo de un error.

**Response (200 OK):**
```json
{
  "id": "...",
  "serviceName": "VehiclesSaleService",
  "environment": "Production",
  "message": "Object reference not set...",
  "exceptionType": "NullReferenceException",
  "stackTrace": "...",
  "severity": "Error",
  "status": "New",
  "requestPath": "/api/vehicles/123",
  "userId": "...",
  "userEmail": "usuario@ejemplo.com",
  "occurredAt": "2026-01-07T10:30:00Z",
  "errorGroupId": "...",
  "occurrenceCount": 15
}
```

### Gestión de Errores

#### PUT `/api/errors/{id}/resolve`
Marcar error como resuelto.

**Request:**
```json
{
  "resolutionNotes": "Fixed NullReferenceException in VehicleController line 45"
}
```

#### PUT `/api/errors/{id}/ignore`
Ignorar error (no alertar más).

#### PUT `/api/errors/{id}/assign`
Asignar error a un desarrollador.

**Request:**
```json
{
  "assignedTo": "dev-user-id"
}
```

### Error Groups

#### GET `/api/errorgroups`
Listar grupos de errores.

**Response (200 OK):**
```json
{
  "items": [
    {
      "id": "...",
      "title": "NullReferenceException in VehicleController",
      "exceptionType": "NullReferenceException",
      "occurrenceCount": 45,
      "affectedUsersCount": 12,
      "maxSeverity": "Error",
      "status": "InProgress",
      "firstOccurrence": "2026-01-05T00:00:00Z",
      "lastOccurrence": "2026-01-07T10:30:00Z"
    }
  ]
}
```

#### GET `/api/errorgroups/{id}`
Detalle de un grupo de errores.

### Estadísticas

#### GET `/api/errors/statistics`
Obtener estadísticas de errores.

**Query Parameters:**
- `serviceName`: Filtrar por servicio
- `from`: Fecha desde
- `to`: Fecha hasta

**Response (200 OK):**
```json
{
  "totalErrors": 1250,
  "newErrors": 85,
  "resolvedErrors": 920,
  "criticalErrors": 15,
  "errorsByService": {
    "VehiclesSaleService": 450,
    "AuthService": 120,
    "UserService": 80
  },
  "errorsBySeverity": {
    "Critical": 15,
    "Error": 890,
    "Warning": 300,
    "Info": 45
  },
  "errorTrend": [
    { "date": "2026-01-01", "count": 95 },
    { "date": "2026-01-02", "count": 120 }
  ]
}
```

### Alertas

#### GET `/api/alerts`
Listar reglas de alertas.

#### POST `/api/alerts`
Crear regla de alerta.

**Request:**
```json
{
  "name": "Critical Errors in Production",
  "serviceName": "VehiclesSaleService",
  "minSeverity": "Critical",
  "occurrenceThreshold": 5,
  "timeWindow": "00:15:00",
  "sendEmail": true,
  "emailRecipients": "dev-team@okla.com.do",
  "isActive": true
}
```

---

## 🔧 TECNOLOGÍAS

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />
<PackageReference Include="MediatR" Version="12.2.0" />
<PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
<PackageReference Include="RabbitMQ.Client" Version="6.8.1" />
```

### Servicios Externos
- **PostgreSQL**: Almacenamiento de errores
- **RabbitMQ**: Cola de errores para procesamiento asíncrono
- **Slack API**: Notificaciones de alertas
- **Email Service**: Notificaciones por email

---

## ⚙️ CONFIGURACIÓN

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=${DB_HOST};Database=errorservice;..."
  },
  "ErrorHandling": {
    "MaxStackTraceLength": 5000,
    "RetentionDays": 90,
    "AutoGroupErrors": true,
    "EnableAlerts": true
  },
  "Alerts": {
    "SlackWebhook": "${SLACK_WEBHOOK_URL}",
    "EmailFrom": "errors@okla.com.do"
  }
}
```

---

## 🔄 EVENTOS CONSUMIDOS

ErrorService consume eventos de error de TODOS los servicios:

### ErrorOccurredEvent
```csharp
public record ErrorOccurredEvent(
    string ServiceName,
    string Message,
    string ExceptionType,
    string StackTrace,
    ErrorSeverity Severity,
    DateTime OccurredAt,
    Dictionary<string, object> Context
);
```

**Publicado por:** Todos los microservicios  
**Handler:** `LogErrorHandler`

---

## 📊 AGRUPACIÓN DE ERRORES

### Algoritmo de Grouping

Los errores se agrupan usando un hash generado por:
1. **ExceptionType** (NullReferenceException, etc.)
2. **ServiceName** (VehiclesSaleService, etc.)
3. **Stack Trace (primeras 3 líneas)** - Location del error

Errores con el mismo hash se agrupan en un `ErrorGroup`.

### Beneficios
- **Reducir ruido**: 1000 errores idénticos = 1 grupo
- **Priorización**: Ver qué errores afectan a más usuarios
- **Tracking**: Seguimiento de resolución por grupo

---

## 📝 REGLAS DE NEGOCIO

### Severidad de Errores
- **Critical**: Sistema no funcional, requiere acción inmediata
- **Error**: Funcionalidad rota, afecta usuarios
- **Warning**: Potencial problema, no bloquea funcionalidad
- **Info**: Log informativo, no es error

### Retención de Datos
- **Errores resueltos**: 90 días
- **Errores activos**: Indefinido
- **Stack traces completos**: Solo últimos 30 días (por espacio)

### Alertas
- **Throttling**: Máximo 1 alerta cada 15 minutos por grupo
- **Auto-silence**: Después de 10 alertas en 1 hora

---

## 🔗 RELACIONES

### Consume Eventos De:
- **TODOS los servicios**: Errores y excepciones

### Publica Eventos A:
- **NotificationService**: Alertas de errores críticos
- **Slack**: Webhooks de notificación

### Consultado Por:
- **AdminService**: Dashboard de errores
- **Developers**: Debugging y monitoreo

---

## 🚀 DESPLIEGUE

### Kubernetes
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: errorservice
  namespace: okla
spec:
  replicas: 2
  template:
    spec:
      containers:
      - name: errorservice
        image: ghcr.io/gregorymorenoiem/cardealer-errorservice:latest
        ports:
        - containerPort: 8080
```

---

## 🐛 USO DESDE OTROS SERVICIOS

### Registro Manual de Error

```csharp
// En cualquier microservicio
try
{
    // Código que puede fallar
}
catch (Exception ex)
{
    var errorEvent = new ErrorOccurredEvent(
        ServiceName: "VehiclesSaleService",
        Message: ex.Message,
        ExceptionType: ex.GetType().Name,
        StackTrace: ex.StackTrace,
        Severity: ErrorSeverity.Error,
        OccurredAt: DateTime.UtcNow,
        Context: new Dictionary<string, object>
        {
            ["UserId"] = userId,
            ["VehicleId"] = vehicleId
        }
    );
    
    await _eventPublisher.PublishAsync(errorEvent);
}
```

---

## 📅 ÚLTIMA ACTUALIZACIÓN

**Fecha:** Enero 7, 2026  
**Versión:** 1.0.0  
**Estado:** Producción en DOKS
