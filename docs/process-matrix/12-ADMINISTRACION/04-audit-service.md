# 📋 AuditService - Matriz de Procesos

> **Servicio:** AuditService  
> **Puerto:** 5045  
> **Base de Datos:** PostgreSQL (audit_db) / Elasticsearch (opcional)  
> **Tecnología:** .NET 8, MediatR, Entity Framework Core  
> **Última actualización:** Enero 23, 2026  
> **Estado de Implementación:** 🟢 Implementado

---

## 📊 Resumen de Implementación

| Componente              | Total | Implementado | Pendiente | Estado  |
| ----------------------- | ----- | ------------ | --------- | ------- |
| **Controllers**         | 2     | 2            | 0         | ✅ 100% |
| **Procesos (AUDIT-\*)** | 5     | 5            | 0         | ✅ 100% |
| **Consumers RabbitMQ**  | 3     | 3            | 0         | ✅ 100% |
| **Tests Unitarios**     | 12    | 10           | 2         | 🟡 83%  |

### Leyenda de Estados

- ✅ **IMPLEMENTADO Y PROBADO**: Código completo con tests
- 🟢 **IMPLEMENTADO**: Código completo, falta testing
- 🟡 **EN PROGRESO**: Implementación parcial
- 🔴 **PENDIENTE**: No implementado

---

## 📋 Información General

| Aspecto           | Detalle                                                                                                                             |
| ----------------- | ----------------------------------------------------------------------------------------------------------------------------------- |
| **Servicio**      | AuditService                                                                                                                        |
| **Puerto**        | 5045                                                                                                                                |
| **Base de Datos** | PostgreSQL (audit_db) / Elasticsearch (opcional)                                                                                    |
| **Tecnología**    | .NET 8, MediatR, Entity Framework Core                                                                                              |
| **Mensajería**    | RabbitMQ (consumidor)                                                                                                               |
| **Descripción**   | Sistema centralizado de auditoría para registrar acciones de usuarios, eventos del sistema y cambios de datos en toda la plataforma |

### Arquitectura

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                        AuditService Architecture                             │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│   Event Sources                      Core Service                            │
│   ┌────────────────┐                ┌────────────────────────────────┐      │
│   │ All Services   │──┐             │           AuditService           │      │
│   │ (via RabbitMQ) │  │             │  ┌──────────────────────────┐   │      │
│   └────────────────┘  │             │  │ Controllers              │   │      │
│   ┌────────────────┐  │             │  │ • AuditController        │   │      │
│   │ AuthService    │──┼────────────▶│  │ • StatisticsController   │   │      │
│   │ (Login Events) │  │             │  └──────────────────────────┘   │      │
│   └────────────────┘  │             │  ┌──────────────────────────┐   │      │
│   ┌────────────────┐  │             │  │ Consumers (RabbitMQ)     │   │      │
│   │ UserService    │──┤             │  │ • UserActionConsumer     │   │      │
│   │ (Profile Chg.) │  │             │  │ • DataChangeConsumer     │   │      │
│   └────────────────┘  │             │  │ • SystemEventConsumer    │   │      │
│   ┌────────────────┐  │             │  └──────────────────────────┘   │      │
│   │ BillingService │──┘             │  ┌──────────────────────────┐   │      │
│   │ (Payments)     │               │  │ Domain                   │   │      │
│   └────────────────┘               │  │ • AuditLog, EventType    │   │      │
│                                    │  │ • Actor, Resource        │   │      │
│   Consumers                        │  └──────────────────────────┘   │      │
│   ┌────────────────┐               └────────────────────────────────┘      │
│   │ Admin Panel    │◀────────────────────         │                        │
│   │ (Audit Viewer) │               ┌───────────┼───────────┐                │
│   └────────────────┘               ▼           ▼           ▼                │
│   ┌────────────────┐       ┌────────────┐ ┌────────────┐ ┌────────────┐   │
│   │ Security Team  │◀───── │ PostgreSQL │ │ Elastic    │ │  RabbitMQ  │   │
│   │ (Reports)      │       │ (Audit     │ │ (Search,   │ │ (Events    │   │
│   └────────────────┘       │  Logs)     │ │  Optional) │ │  Consumer) │   │
│                           └────────────┘ └────────────┘ └────────────┘   │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 🎯 Endpoints del Servicio

### AuditController

| Método | Endpoint                   | Descripción                           | Auth | Roles                |
| ------ | -------------------------- | ------------------------------------- | ---- | -------------------- |
| `GET`  | `/api/audit`               | Listar logs de auditoría con filtros  | ✅   | Admin, Auditor       |
| `GET`  | `/api/audit/{id}`          | Obtener log de auditoría por ID       | ✅   | Admin, Auditor       |
| `POST` | `/api/audit`               | Crear entrada de auditoría manual     | ✅   | System, Admin        |
| `GET`  | `/api/audit/stats`         | Obtener estadísticas de auditoría     | ✅   | Admin, Auditor       |
| `GET`  | `/api/audit/user/{userId}` | Obtener logs de un usuario específico | ✅   | Admin, Auditor, Self |

---

## 📊 Entidades del Dominio

### AuditLog (Entidad Principal)

```csharp
public class AuditLog
{
    public Guid Id { get; set; }

    // Identificación del Actor
    public string UserId { get; set; }              // ID del usuario que realizó la acción
    public string UserIp { get; set; }              // Dirección IP del usuario
    public string UserAgent { get; set; }           // User-Agent del navegador/cliente

    // Acción y Recurso
    public string Action { get; set; }              // Acción realizada (LOGIN, CREATE, UPDATE, DELETE, etc.)
    public string Resource { get; set; }            // Recurso afectado (User, Vehicle, Payment, etc.)

    // Contexto del Servicio
    public string ServiceName { get; set; }         // Nombre del microservicio origen
    public string? CorrelationId { get; set; }      // ID para correlacionar requests

    // Resultado de la Operación
    public bool Success { get; set; }               // Si la operación fue exitosa
    public string? ErrorMessage { get; set; }       // Mensaje de error si falló
    public long? DurationMs { get; set; }           // Duración de la operación en ms

    // Severidad
    public AuditSeverity Severity { get; set; }     // Nivel de severidad del evento

    // Datos Adicionales
    public string AdditionalDataJson { get; set; } = "{}";  // Datos extra serializados

    // Auditoría
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

### Enumeraciones

```csharp
public enum AuditSeverity
{
    Debug = 1,          // Información de desarrollo/troubleshooting
    Information = 2,    // Operaciones normales del sistema
    Warning = 3,        // Eventos que podrían indicar problemas
    Error = 4,          // Errores que no rompen el sistema
    Critical = 5        // Eventos críticos que requieren atención inmediata
}
```

---

## 🔄 Procesos Detallados

### PROCESO 1: Consultar Logs de Auditoría

#### Endpoint: `GET /api/audit`

| Paso | Actor         | Acción                      | Sistema              | Resultado                    |
| ---- | ------------- | --------------------------- | -------------------- | ---------------------------- |
| 1    | Admin/Auditor | Accede a panel de auditoría | HTTP GET con filtros | Request recibido             |
| 2    | API           | Valida token JWT            | Authorization check  | Usuario autenticado          |
| 3    | API           | Valida rol Admin o Auditor  | Role check           | Autorizado                   |
| 4    | Handler       | Parsea query parameters     | Query binding        | Filtros extraídos            |
| 5    | Handler       | Construye query dinámico    | LINQ builder         | Query preparado              |
| 6    | Repository    | Ejecuta query paginado      | SELECT con WHERE     | Resultados obtenidos         |
| 7    | Handler       | Mapea a DTOs                | AutoMapper           | DTOs creados                 |
| 8    | API           | Retorna lista paginada      | HTTP 200             | PaginatedResult<AuditLogDto> |

#### Query Parameters

| Parámetro        | Tipo      | Default | Descripción                  |
| ---------------- | --------- | ------- | ---------------------------- |
| `userId`         | string    | null    | Filtrar por ID de usuario    |
| `action`         | string    | null    | Filtrar por tipo de acción   |
| `resource`       | string    | null    | Filtrar por recurso afectado |
| `serviceName`    | string    | null    | Filtrar por servicio origen  |
| `severity`       | string    | null    | Filtrar por severidad        |
| `success`        | bool?     | null    | Filtrar por éxito/fallo      |
| `fromDate`       | DateTime? | null    | Fecha inicial del rango      |
| `toDate`         | DateTime? | null    | Fecha final del rango        |
| `page`           | int       | 1       | Número de página             |
| `pageSize`       | int       | 50      | Tamaño de página             |
| `sortBy`         | string    | null    | Campo para ordenar           |
| `sortDescending` | bool      | true    | Orden descendente            |
| `searchText`     | string    | null    | Búsqueda en texto libre      |

#### Response (200 OK)

```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "audit-log-uuid",
        "userId": "user-uuid",
        "action": "LOGIN",
        "resource": "AuthService",
        "userIp": "192.168.1.100",
        "userAgent": "Mozilla/5.0 ...",
        "success": true,
        "durationMs": 245,
        "serviceName": "AuthService",
        "severity": "Information",
        "correlationId": "corr-123",
        "additionalData": {
          "method": "POST",
          "endpoint": "/api/auth/login"
        },
        "createdAt": "2026-01-09T10:30:00Z"
      }
    ],
    "totalCount": 1500,
    "page": 1,
    "pageSize": 50,
    "totalPages": 30
  },
  "timestamp": "2026-01-09T10:30:05Z"
}
```

---

### PROCESO 2: Obtener Log de Auditoría por ID

#### Endpoint: `GET /api/audit/{id}`

| Paso | Actor   | Acción                      | Sistema                   | Resultado             |
| ---- | ------- | --------------------------- | ------------------------- | --------------------- |
| 1    | Admin   | Solicita detalle de log     | HTTP GET                  | Request recibido      |
| 2    | API     | Valida autenticación y rol  | JWT + Role check          | Autorizado            |
| 3    | Handler | Busca log por ID            | Repository.GetByIdAsync() | Log encontrado o null |
| 4    | Handler | Si no existe, retorna error | ApiResponse.Fail()        | Error 404             |
| 5    | Handler | Mapea a DTO                 | AutoMapper                | DTO creado            |
| 6    | API     | Retorna detalle completo    | HTTP 200                  | AuditLogDto           |

#### Response (200 OK)

```json
{
  "success": true,
  "data": {
    "id": "audit-log-uuid",
    "userId": "user-uuid",
    "action": "UPDATE",
    "resource": "Vehicle",
    "resourceId": "vehicle-uuid",
    "userIp": "192.168.1.100",
    "userAgent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64)...",
    "success": true,
    "durationMs": 125,
    "serviceName": "VehiclesSaleService",
    "severity": "Information",
    "correlationId": "corr-456",
    "additionalData": {
      "method": "PUT",
      "endpoint": "/api/vehicles/vehicle-uuid",
      "changedFields": ["price", "description"],
      "oldValues": {
        "price": 1500000,
        "description": "Original description"
      },
      "newValues": {
        "price": 1450000,
        "description": "Updated description"
      }
    },
    "createdAt": "2026-01-09T14:22:35Z"
  },
  "timestamp": "2026-01-09T14:23:00Z"
}
```

---

### PROCESO 3: Crear Entrada de Auditoría

#### Endpoint: `POST /api/audit`

| Paso | Actor         | Acción                    | Sistema                  | Resultado        |
| ---- | ------------- | ------------------------- | ------------------------ | ---------------- |
| 1    | Microservicio | Envía evento de auditoría | HTTP POST                | Request recibido |
| 2    | API           | Valida token de servicio  | Service JWT              | Autorizado       |
| 3    | Validador     | Valida campos requeridos  | FluentValidation         | Validación OK    |
| 4    | Handler       | Crea entidad AuditLog     | AuditLog.CreateSuccess() | Entidad creada   |
| 5    | Handler       | Serializa AdditionalData  | JsonSerializer           | JSON almacenado  |
| 6    | Repository    | Persiste en base de datos | INSERT audit_logs        | Log guardado     |
| 7    | API           | Retorna ID creado         | HTTP 201                 | String ID        |

#### Request Body

```json
{
  "userId": "user-uuid",
  "action": "CREATE",
  "resource": "Vehicle",
  "resourceId": "vehicle-uuid",
  "userIp": "192.168.1.100",
  "userAgent": "Mozilla/5.0...",
  "success": true,
  "durationMs": 450,
  "serviceName": "VehiclesSaleService",
  "severity": "Information",
  "correlationId": "corr-789",
  "additionalData": {
    "method": "POST",
    "endpoint": "/api/vehicles",
    "vehicleTitle": "Toyota Corolla 2024",
    "dealerId": "dealer-uuid"
  }
}
```

#### Response (201 Created)

```json
{
  "success": true,
  "data": "audit-log-uuid-created",
  "timestamp": "2026-01-09T10:30:00Z"
}
```

---

### PROCESO 4: Obtener Estadísticas de Auditoría

#### Endpoint: `GET /api/audit/stats`

| Paso | Actor      | Acción                        | Sistema              | Resultado               |
| ---- | ---------- | ----------------------------- | -------------------- | ----------------------- |
| 1    | Admin      | Solicita estadísticas         | HTTP GET con filtros | Request recibido        |
| 2    | API        | Valida autenticación          | JWT check            | Autorizado              |
| 3    | Handler    | Parsea filtros de fecha       | Query binding        | Filtros extraídos       |
| 4    | Repository | Ejecuta queries de agregación | GROUP BY queries     | Agregaciones calculadas |
| 5    | Handler    | Construye DTO de estadísticas | AuditStatsDto        | Stats compilados        |
| 6    | API        | Retorna estadísticas          | HTTP 200             | AuditStatsDto           |

#### Query Parameters

| Parámetro     | Tipo      | Default      | Descripción                |
| ------------- | --------- | ------------ | -------------------------- |
| `fromDate`    | DateTime? | 7 días atrás | Inicio del período         |
| `toDate`      | DateTime? | ahora        | Fin del período            |
| `serviceName` | string    | null         | Filtrar por servicio       |
| `userId`      | string    | null         | Filtrar por usuario        |
| `action`      | string    | null         | Filtrar por tipo de acción |

#### Response (200 OK)

```json
{
  "success": true,
  "data": {
    "totalEvents": 15420,
    "successfulEvents": 14850,
    "failedEvents": 570,
    "successRate": 96.3,
    "averageDurationMs": 185.5,
    "eventsByAction": {
      "LOGIN": 3500,
      "CREATE": 2800,
      "UPDATE": 4200,
      "DELETE": 920,
      "VIEW": 4000
    },
    "eventsByService": {
      "AuthService": 3500,
      "VehiclesSaleService": 5200,
      "UserService": 2100,
      "BillingService": 1800,
      "MediaService": 2820
    },
    "eventsBySeverity": {
      "Debug": 500,
      "Information": 13500,
      "Warning": 850,
      "Error": 520,
      "Critical": 50
    },
    "eventsOverTime": [
      { "date": "2026-01-03", "count": 2100 },
      { "date": "2026-01-04", "count": 2250 },
      { "date": "2026-01-05", "count": 1980 },
      { "date": "2026-01-06", "count": 2300 },
      { "date": "2026-01-07", "count": 2450 },
      { "date": "2026-01-08", "count": 2340 },
      { "date": "2026-01-09", "count": 2000 }
    ],
    "topUsers": [
      { "userId": "admin-uuid", "eventCount": 1500 },
      { "userId": "user-uuid-1", "eventCount": 850 },
      { "userId": "user-uuid-2", "eventCount": 720 }
    ],
    "recentFailures": [
      {
        "id": "log-uuid",
        "action": "CREATE",
        "resource": "Vehicle",
        "errorMessage": "Validation failed: Price is required",
        "createdAt": "2026-01-09T10:15:00Z"
      }
    ]
  },
  "timestamp": "2026-01-09T10:30:00Z"
}
```

---

### PROCESO 5: Obtener Logs de Usuario Específico

#### Endpoint: `GET /api/audit/user/{userId}`

| Paso | Actor         | Acción                       | Sistema                | Resultado        |
| ---- | ------------- | ---------------------------- | ---------------------- | ---------------- |
| 1    | Admin/Usuario | Solicita logs de usuario     | HTTP GET               | Request recibido |
| 2    | API           | Valida token JWT             | Authorization          | Autenticado      |
| 3    | API           | Valida acceso (Admin o Self) | Role/Owner check       | Autorizado       |
| 4    | Handler       | Construye query con userId   | GetAuditLogsQuery      | Query preparado  |
| 5    | Repository    | Filtra por userId            | WHERE UserId = @userId | Logs del usuario |
| 6    | Handler       | Aplica paginación            | Skip/Take              | Página actual    |
| 7    | API           | Retorna lista paginada       | HTTP 200               | PaginatedResult  |

#### Response (200 OK)

```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "log-uuid-1",
        "action": "LOGIN",
        "resource": "AuthService",
        "success": true,
        "userIp": "192.168.1.100",
        "createdAt": "2026-01-09T08:00:00Z"
      },
      {
        "id": "log-uuid-2",
        "action": "UPDATE",
        "resource": "User",
        "success": true,
        "userIp": "192.168.1.100",
        "createdAt": "2026-01-09T08:15:00Z"
      },
      {
        "id": "log-uuid-3",
        "action": "CREATE",
        "resource": "Vehicle",
        "success": true,
        "userIp": "192.168.1.100",
        "createdAt": "2026-01-09T09:30:00Z"
      }
    ],
    "totalCount": 250,
    "page": 1,
    "pageSize": 50,
    "totalPages": 5
  },
  "timestamp": "2026-01-09T10:30:00Z"
}
```

---

## 🔔 Consumo de Eventos (RabbitMQ)

El AuditService es principalmente un **consumidor** de eventos de otros servicios.

### Eventos Consumidos

| Evento                  | Origen                  | Acción Generada  | Severidad   |
| ----------------------- | ----------------------- | ---------------- | ----------- |
| `UserLoggedInEvent`     | AuthService             | LOGIN            | Information |
| `UserLoggedOutEvent`    | AuthService             | LOGOUT           | Information |
| `LoginFailedEvent`      | AuthService             | LOGIN_FAILED     | Warning     |
| `UserCreatedEvent`      | UserService             | CREATE           | Information |
| `UserUpdatedEvent`      | UserService             | UPDATE           | Information |
| `UserDeletedEvent`      | UserService             | DELETE           | Information |
| `VehicleCreatedEvent`   | VehiclesSaleService     | CREATE           | Information |
| `VehicleUpdatedEvent`   | VehiclesSaleService     | UPDATE           | Information |
| `VehicleDeletedEvent`   | VehiclesSaleService     | DELETE           | Information |
| `VehiclePublishedEvent` | VehiclesSaleService     | PUBLISH          | Information |
| `PaymentCompletedEvent` | BillingService          | PAYMENT          | Information |
| `PaymentFailedEvent`    | BillingService          | PAYMENT_FAILED   | Error       |
| `DealerVerifiedEvent`   | DealerManagementService | VERIFY           | Information |
| `DealerSuspendedEvent`  | DealerManagementService | SUSPEND          | Warning     |
| `ComplianceAlertEvent`  | ComplianceService       | COMPLIANCE_ALERT | Critical    |
| `FileUploadedEvent`     | MediaService            | UPLOAD           | Information |
| `FileDeletedEvent`      | MediaService            | DELETE           | Information |
| `ErrorLoggedEvent`      | ErrorService            | ERROR            | Error       |
| `CriticalErrorEvent`    | ErrorService            | CRITICAL_ERROR   | Critical    |

### Consumer Handler

```csharp
public class AuditEventConsumer : IConsumer<AuditEventMessage>
{
    public async Task Consume(ConsumeContext<AuditEventMessage> context)
    {
        var message = context.Message;

        var auditLog = new AuditLog(
            userId: message.UserId,
            action: message.Action,
            resource: message.Resource,
            userIp: message.UserIp ?? "system",
            userAgent: message.UserAgent ?? "service",
            additionalData: message.AdditionalData ?? new(),
            success: message.Success,
            errorMessage: message.ErrorMessage,
            durationMs: message.DurationMs,
            correlationId: message.CorrelationId,
            serviceName: message.ServiceName,
            severity: message.Severity
        );

        await _repository.AddAsync(auditLog);
    }
}
```

---

## 📊 Acciones Estándar de Auditoría

| Acción             | Descripción                 | Ejemplo de Recurso    |
| ------------------ | --------------------------- | --------------------- |
| `LOGIN`            | Usuario inició sesión       | AuthService           |
| `LOGOUT`           | Usuario cerró sesión        | AuthService           |
| `LOGIN_FAILED`     | Intento de login fallido    | AuthService           |
| `CREATE`           | Creación de recurso         | User, Vehicle, Dealer |
| `UPDATE`           | Actualización de recurso    | User, Vehicle, Dealer |
| `DELETE`           | Eliminación de recurso      | User, Vehicle, Dealer |
| `VIEW`             | Visualización de recurso    | Vehicle, Report       |
| `DOWNLOAD`         | Descarga de archivo/reporte | Report, Invoice       |
| `PUBLISH`          | Publicación de contenido    | Vehicle               |
| `UNPUBLISH`        | Despublicación de contenido | Vehicle               |
| `APPROVE`          | Aprobación (admin)          | Dealer, Vehicle       |
| `REJECT`           | Rechazo (admin)             | Dealer, Vehicle       |
| `VERIFY`           | Verificación                | Dealer                |
| `SUSPEND`          | Suspensión                  | User, Dealer          |
| `ACTIVATE`         | Activación                  | User, Dealer          |
| `PAYMENT`          | Pago realizado              | Subscription, Invoice |
| `PAYMENT_FAILED`   | Pago fallido                | Subscription          |
| `EXPORT`           | Exportación de datos        | Report, CSV           |
| `IMPORT`           | Importación de datos        | Vehicles, CSV         |
| `COMPLIANCE_ALERT` | Alerta de compliance        | Transaction           |
| `CONFIG_CHANGE`    | Cambio de configuración     | Settings              |

---

## ⚠️ Reglas de Negocio

### Retención de Datos

| Tipo de Log           | Retención | Justificación            |
| --------------------- | --------- | ------------------------ |
| Logs de Compliance    | 10 años   | Ley 155-17 (AML RD)      |
| Logs Financieros      | 7 años    | Código Tributario RD     |
| Logs de Usuarios      | 5 años    | LGPD/Protección de datos |
| Logs Técnicos (Debug) | 30 días   | Solo troubleshooting     |
| Logs de Errores       | 1 año     | Análisis de incidentes   |

### Campos Obligatorios

| Campo         | Requerido | Validación        |
| ------------- | --------- | ----------------- |
| `userId`      | ✅ Sí     | No vacío          |
| `action`      | ✅ Sí     | No vacío          |
| `resource`    | ✅ Sí     | No vacío          |
| `userIp`      | ✅ Sí     | Formato IP válido |
| `userAgent`   | ✅ Sí     | No vacío          |
| `serviceName` | ✅ Sí     | Servicio conocido |

### Seguridad

- **No almacenar datos sensibles** en AdditionalData (passwords, tarjetas, etc.)
- **Enmascarar PII** cuando sea necesario (email parcial, phone parcial)
- **Inmutabilidad**: Los logs no pueden ser modificados después de crearse
- **Acceso restringido**: Solo Admin y Auditor pueden consultar todos los logs

---

## ❌ Códigos de Error

| Código      | HTTP Status | Mensaje                           | Causa                             |
| ----------- | ----------- | --------------------------------- | --------------------------------- |
| `AUDIT_001` | 400         | Campos requeridos faltantes       | Falta userId, action, resource    |
| `AUDIT_002` | 400         | Formato de fecha inválido         | fromDate/toDate mal formateados   |
| `AUDIT_003` | 404         | Log de auditoría no encontrado    | ID no existe                      |
| `AUDIT_004` | 403         | No autorizado para ver estos logs | Usuario no es Admin/Auditor/Owner |
| `AUDIT_005` | 400         | Rango de fechas inválido          | toDate anterior a fromDate        |
| `AUDIT_006` | 400         | PageSize excede límite            | pageSize > 200                    |

---

## ⚙️ Configuración del Servicio

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=postgres;Database=audit_db;Username=postgres;Password=xxx",
    "Elasticsearch": "http://elasticsearch:9200"
  },
  "AuditSettings": {
    "MaxPageSize": 200,
    "DefaultPageSize": 50,
    "EnableElasticsearch": false,
    "RetentionPolicy": {
      "ComplianceDays": 3650,
      "FinancialDays": 2555,
      "UserDays": 1825,
      "DebugDays": 30,
      "ErrorDays": 365
    },
    "PiiMasking": {
      "Enabled": true,
      "MaskEmail": true,
      "MaskPhone": true
    }
  },
  "RabbitMQ": {
    "Host": "rabbitmq",
    "Port": 5672,
    "Username": "guest",
    "Password": "guest",
    "VirtualHost": "/",
    "Queues": {
      "AuditEvents": "audit.events"
    }
  }
}
```

---

## 🔒 Seguridad

### Autenticación y Autorización

| Endpoint                     | Auth | Roles Permitidos            |
| ---------------------------- | ---- | --------------------------- |
| GET /api/audit               | ✅   | Admin, Auditor              |
| GET /api/audit/{id}          | ✅   | Admin, Auditor              |
| POST /api/audit              | ✅   | System (service-to-service) |
| GET /api/audit/stats         | ✅   | Admin, Auditor              |
| GET /api/audit/user/{userId} | ✅   | Admin, Auditor, Self        |

### Consideraciones de Privacidad

1. **Enmascaramiento de PII**: Emails, teléfonos y datos sensibles
2. **Logs de acceso**: Quién consultó qué logs (meta-auditoría)
3. **Cifrado**: Datos en tránsito (TLS) y reposo (encryption at rest)
4. **Derecho al olvido**: Proceso para eliminar logs de usuarios (GDPR/LGPD)

---

## 📈 Métricas y Observabilidad

### Métricas Prometheus

| Métrica                        | Tipo      | Labels                   | Descripción                   |
| ------------------------------ | --------- | ------------------------ | ----------------------------- |
| `audit_events_total`           | Counter   | action, service, success | Total de eventos de auditoría |
| `audit_events_by_severity`     | Counter   | severity                 | Eventos por severidad         |
| `audit_query_duration_seconds` | Histogram | endpoint                 | Duración de queries           |
| `audit_storage_size_bytes`     | Gauge     | -                        | Tamaño del almacenamiento     |

---

## 📚 Referencias

- [AuditController](../../backend/AuditService/AuditService.Api/Controllers/AuditController.cs)
- [AuditLog Entity](../../backend/AuditService/AuditService.Domain/Entities/AuditLog.cs)
- [AuditSeverity Enum](../../backend/AuditService/AuditService.Shared/Enums/AuditSeverity.cs)

---

**Última actualización:** Enero 9, 2026  
**Autor:** Sistema de Documentación Automatizado  
**Versión:** 1.0.0
