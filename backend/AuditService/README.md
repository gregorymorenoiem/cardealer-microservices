# 📊 AuditService

Servicio centralizado de auditoría para registro y tracking de operaciones en el sistema CarDealer.

## 📋 Descripción

Microservicio que captura, almacena y consulta eventos de auditoría de todos los servicios del sistema, proporcionando trazabilidad completa de operaciones.

## 🚀 Características

- **Event Sourcing**: Registro inmutable de eventos
- **Async Processing**: Manejo asíncrono con RabbitMQ
- **Query API**: Búsqueda y filtrado de logs de auditoría
- **Retention Policy**: Políticas configurables de retención
- **Aggregation**: Reportes y estadísticas
- **GDPR Compliance**: Anonimización de datos sensibles
- **Time-Series Storage**: Optimizado para consultas temporales

## 🏗️ Arquitectura

```
AuditService.Api (Puerto 5002)
├── Controllers/
│   └── AuditController.cs
├── AuditService.Application/
│   ├── Commands/
│   │   └── CreateAuditLogCommand
│   ├── Queries/
│   │   ├── GetAuditLogsQuery
│   │   └── GetAuditLogByIdQuery
│   └── Services/
│       └── AuditEventProcessor
├── AuditService.Domain/
│   ├── Entities/
│   │   └── AuditLog
│   ├── Enums/
│   │   ├── AuditAction
│   │   └── AuditSeverity
│   └── ValueObjects/
└── AuditService.Infrastructure/
    ├── Data/
    ├── Repositories/
    ├── MessageBus/
    │   └── AuditEventConsumer
    └── External/
```

## 📦 Dependencias Principales

- **Entity Framework Core 8.0**
- **RabbitMQ.Client 6.8.1** - Message bus
- **MediatR 12.2.0** - CQRS
- **Npgsql** - PostgreSQL
- **Serilog** - Structured logging

## ⚙️ Configuración

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=auditdb;..."
  },
  "RabbitMQ": {
    "Host": "localhost",
    "Port": 5672,
    "Username": "guest",
    "Password": "guest",
    "QueueName": "audit-events"
  },
  "RetentionPolicy": {
    "DaysToKeep": 365,
    "ArchiveAfterDays": 90
  }
}
```

## 🔌 API Endpoints

### Queries
```http
GET /api/audit                 # Listar logs de auditoría
GET /api/audit/{id}            # Obtener log específico
GET /api/audit/user/{userId}   # Logs por usuario
GET /api/audit/entity/{entityType}/{entityId}  # Logs por entidad
GET /api/audit/search          # Búsqueda avanzada
```

### Commands
```http
POST /api/audit                # Crear log de auditoría (manual)
```

### Query Parameters
```http
GET /api/audit?action=CREATE&severity=HIGH&from=2024-01-01&to=2024-12-31
GET /api/audit?userId=123&entityType=Vehicle&page=1&pageSize=50
```

## 📝 Ejemplos de Uso

### Crear Audit Log (Manual)
```bash
curl -X POST http://localhost:5002/api/audit \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "user-123",
    "action": "UPDATE",
    "entityType": "Vehicle",
    "entityId": "vehicle-456",
    "changes": {
      "price": { "old": 15000, "new": 14500 },
      "status": { "old": "Available", "new": "Sold" }
    },
    "ipAddress": "192.168.1.100",
    "userAgent": "Mozilla/5.0..."
  }'
```

### Buscar Logs
```bash
curl -X GET "http://localhost:5002/api/audit/search?action=DELETE&severity=HIGH"
```

**Respuesta**:
```json
{
  "data": [
    {
      "id": "audit-789",
      "timestamp": "2024-01-15T10:30:00Z",
      "userId": "admin-001",
      "action": "DELETE",
      "entityType": "Vehicle",
      "entityId": "vehicle-999",
      "severity": "HIGH",
      "ipAddress": "10.0.0.5",
      "changes": { ... }
    }
  ],
  "totalCount": 1,
  "page": 1,
  "pageSize": 50
}
```

## 📊 Modelo de Datos

### AuditLog Entity
```csharp
public class AuditLog
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string UserId { get; set; }
    public AuditAction Action { get; set; }  // CREATE, UPDATE, DELETE, READ
    public AuditSeverity Severity { get; set; }  // LOW, MEDIUM, HIGH, CRITICAL
    public string EntityType { get; set; }
    public string EntityId { get; set; }
    public string Changes { get; set; }  // JSON
    public string IpAddress { get; set; }
    public string UserAgent { get; set; }
    public string ServiceName { get; set; }
}
```

### AuditAction Enum
- `CREATE` - Creación de entidad
- `UPDATE` - Actualización
- `DELETE` - Eliminación
- `READ` - Lectura (opcional, alto volumen)
- `LOGIN` - Inicio de sesión
- `LOGOUT` - Cierre de sesión
- `EXPORT` - Exportación de datos

## 🔄 Event Processing

### RabbitMQ Consumer
```csharp
// Los servicios publican eventos
var auditEvent = new AuditEvent
{
    UserId = currentUserId,
    Action = "UPDATE",
    EntityType = "Vehicle",
    EntityId = vehicleId,
    Changes = JsonSerializer.Serialize(changes)
};

await _messageBus.PublishAsync("audit-events", auditEvent);
```

### Async Processing
- **Queue**: `audit-events`
- **Exchange**: `audit-exchange`
- **Routing**: `audit.{serviceName}.{action}`
- **Dead Letter Queue**: `audit-dlq` (mensajes fallidos)

## 🧪 Testing

```bash
# Tests unitarios
dotnet test AuditService.Tests/

# Tests de integración con RabbitMQ
dotnet test AuditService.Tests/ --filter "Category=Integration"
```

## 🐳 Docker

```bash
# Build
docker build -t auditservice:latest .

# Run
docker run -d -p 5002:80 \
  -e ConnectionStrings__DefaultConnection="..." \
  -e RabbitMQ__Host="rabbitmq" \
  --name auditservice \
  auditservice:latest
```

## 📊 Base de Datos

### Tablas
- `AuditLogs` - Registro principal
- `AuditArchive` - Logs archivados (>90 días)

### Índices (Optimización)
```sql
CREATE INDEX IX_AuditLogs_Timestamp ON AuditLogs(Timestamp DESC);
CREATE INDEX IX_AuditLogs_UserId ON AuditLogs(UserId);
CREATE INDEX IX_AuditLogs_EntityType_EntityId ON AuditLogs(EntityType, EntityId);
CREATE INDEX IX_AuditLogs_Action_Severity ON AuditLogs(Action, Severity);
```

### Partitioning (Producción)
```sql
-- Particionar por mes para mejor performance
CREATE TABLE AuditLogs_2024_01 PARTITION OF AuditLogs
FOR VALUES FROM ('2024-01-01') TO ('2024-02-01');
```

## 🔐 Seguridad

- **Immutable Logs**: Los logs no se pueden modificar
- **Access Control**: Solo admins pueden consultar todos los logs
- **Encryption**: Datos sensibles encriptados en reposo
- **GDPR**: Anonimización automática de PII después de retención

## 📈 Monitoreo

### Métricas
- `audit_events_processed_total` - Eventos procesados
- `audit_events_failed_total` - Eventos fallidos
- `audit_query_duration_seconds` - Tiempo de consultas
- `audit_storage_size_bytes` - Tamaño de base de datos

### Retention Jobs
```bash
# Archivar logs antiguos (cron diario)
0 2 * * * docker exec auditservice dotnet AuditService.dll --archive

# Eliminar logs expirados (cron semanal)
0 3 * * 0 docker exec auditservice dotnet AuditService.dll --purge
```

## 📊 Reportes

### Agregaciones Disponibles
```http
GET /api/audit/stats/actions       # Distribución por acción
GET /api/audit/stats/users         # Top usuarios activos
GET /api/audit/stats/entities      # Entidades más modificadas
GET /api/audit/stats/timeline      # Serie temporal de eventos
```

## 🚦 Estado

- ✅ **Build**: OK
- ✅ **Tests**: 100% pasando
- ✅ **Docker**: Configurado
- ✅ **Message Bus**: RabbitMQ integrado

---

**Puerto**: 5002  
**Base de Datos**: PostgreSQL (auditdb)  
**Message Queue**: RabbitMQ (audit-events)  
**Estado**: ✅ Production Ready
