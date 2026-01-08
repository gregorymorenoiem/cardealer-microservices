# AuditService - Context Documentation

## 📋 INFORMACIÓN GENERAL

- **Nombre del Servicio:** AuditService
- **Puerto en Desarrollo:** 5032
- **Estado:** 🚧 **EN DESARROLLO - NO DESPLEGADO**
- **Base de Datos:** PostgreSQL (`auditservice`)
- **Imagen Docker:** Local only

### Propósito
Servicio de auditoría y compliance. Registra todas las operaciones críticas del sistema para trazabilidad, seguridad y cumplimiento regulatorio.

---

## 🏗️ ARQUITECTURA

```
AuditService/
├── AuditService.Api/
│   ├── Controllers/
│   │   ├── AuditLogsController.cs
│   │   └── ComplianceController.cs
│   └── Program.cs
├── AuditService.Application/
├── AuditService.Domain/
│   ├── Entities/
│   │   ├── AuditLog.cs
│   │   ├── SecurityEvent.cs
│   │   └── DataAccess.cs
│   └── Enums/
│       ├── AuditEventType.cs
│       └── SeverityLevel.cs
└── AuditService.Infrastructure/
```

---

## 📦 ENTIDADES PRINCIPALES

### AuditLog
```csharp
public class AuditLog
{
    public Guid Id { get; set; }
    
    // Evento
    public AuditEventType EventType { get; set; }  // Create, Update, Delete, Login, Logout, AccessDenied
    public string Action { get; set; }             // "User.Created", "Vehicle.Updated", "Payment.Processed"
    public string? Description { get; set; }
    
    // Usuario
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public string? UserEmail { get; set; }
    
    // Recurso afectado
    public string? EntityType { get; set; }        // "User", "Vehicle", "Payment"
    public Guid? EntityId { get; set; }
    public string? EntityIdentifier { get; set; }  // Email, VIN, invoice number
    
    // Cambios realizados (para Update)
    public string? OldValue { get; set; }          // JSON del estado anterior
    public string? NewValue { get; set; }          // JSON del estado nuevo
    
    // Contexto técnico
    public string IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? RequestPath { get; set; }
    public string? HttpMethod { get; set; }
    
    // Resultado
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public int? HttpStatusCode { get; set; }
    
    // Metadata
    public DateTime Timestamp { get; set; }
    public string ServiceName { get; set; }        // "VehiclesSaleService", "AuthService"
    public string? TraceId { get; set; }           // Para correlation con logs
}
```

### SecurityEvent
```csharp
public class SecurityEvent
{
    public Guid Id { get; set; }
    
    // Tipo de evento de seguridad
    public SecurityEventType Type { get; set; }
    // LoginSuccess, LoginFailed, PasswordReset, AccountLocked,
    // UnauthorizedAccess, SuspiciousActivity, DataBreach
    
    public SeverityLevel Severity { get; set; }    // Info, Warning, Critical
    
    // Usuario involucrado
    public Guid? UserId { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    
    // Detalles
    public string Description { get; set; }
    public string? Details { get; set; }           // JSON con info adicional
    
    // Contexto
    public string IpAddress { get; set; }
    public string? Location { get; set; }          // Geolocalización aproximada
    public string? Device { get; set; }
    public string? UserAgent { get; set; }
    
    // Respuesta
    public bool RequiresAction { get; set; }
    public bool IsResolved { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolutionNotes { get; set; }
    
    public DateTime Timestamp { get; set; }
}
```

### DataAccess (GDPR/Privacy compliance)
```csharp
public class DataAccess
{
    public Guid Id { get; set; }
    
    // Quién accedió
    public Guid AccessorUserId { get; set; }
    public string AccessorName { get; set; }
    public string AccessorRole { get; set; }
    
    // A qué datos
    public string DataType { get; set; }           // "PersonalInfo", "FinancialData", "HealthData"
    public Guid SubjectUserId { get; set; }        // Usuario cuyos datos se accedieron
    public string SubjectName { get; set; }
    
    // Propósito del acceso
    public string Purpose { get; set; }            // "CustomerSupport", "Investigation", "ReportGeneration"
    public string? Justification { get; set; }
    
    // Campos accedidos
    public List<string> FieldsAccessed { get; set; } // ["email", "phone", "address"]
    
    // Contexto
    public DateTime AccessedAt { get; set; }
    public string IpAddress { get; set; }
    public string ServiceName { get; set; }
}
```

---

## 📡 ENDPOINTS (Propuestos)

### Audit Logs
- `POST /api/audit` - Registrar evento de auditoría
  ```json
  {
    "eventType": "Update",
    "action": "Vehicle.PriceChanged",
    "userId": "uuid",
    "entityType": "Vehicle",
    "entityId": "vehicle-uuid",
    "oldValue": "{\"price\": 15000}",
    "newValue": "{\"price\": 14500}",
    "ipAddress": "192.168.1.100",
    "serviceName": "VehiclesSaleService"
  }
  ```
- `GET /api/audit` - Listar audit logs (filtros: usuario, fecha, tipo, entidad)
- `GET /api/audit/entity/{type}/{id}` - Historial de cambios de una entidad
- `GET /api/audit/user/{userId}` - Actividad de un usuario
- `GET /api/audit/search` - Búsqueda avanzada

### Security Events
- `POST /api/audit/security` - Registrar evento de seguridad
- `GET /api/audit/security` - Listar eventos de seguridad
- `GET /api/audit/security/unresolved` - Eventos críticos sin resolver
- `PUT /api/audit/security/{id}/resolve` - Marcar como resuelto

### Data Access (Privacy/GDPR)
- `POST /api/audit/data-access` - Registrar acceso a datos sensibles
- `GET /api/audit/data-access/user/{userId}` - ¿Quién accedió a los datos de este usuario?
- `GET /api/audit/data-access/accessor/{userId}` - ¿A qué datos accedió este admin?

### Compliance Reports
- `GET /api/audit/compliance/gdpr` - Reporte de accesos para GDPR
- `GET /api/audit/compliance/retention` - Verificar políticas de retención

---

## 💡 FUNCIONALIDADES PLANEADAS

### Automatic Logging via Middleware
```csharp
public class AuditMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        var startTime = DateTime.UtcNow;
        var originalBody = context.Response.Body;
        
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;
        
        await _next(context);
        
        // Log después del request
        var auditLog = new AuditLog
        {
            Action = $"{context.Request.Method} {context.Request.Path}",
            UserId = GetUserId(context),
            IpAddress = context.Connection.RemoteIpAddress?.ToString(),
            HttpMethod = context.Request.Method,
            RequestPath = context.Request.Path,
            HttpStatusCode = context.Response.StatusCode,
            Success = context.Response.StatusCode < 400,
            Timestamp = startTime,
            ServiceName = _serviceName
        };
        
        await _auditService.LogAsync(auditLog);
        
        responseBody.Seek(0, SeekOrigin.Begin);
        await responseBody.CopyToAsync(originalBody);
    }
}
```

### Change Tracking
Detectar qué campos cambiaron:
```csharp
public string DetectChanges(object oldObj, object newObj)
{
    var changes = new Dictionary<string, object>();
    var props = oldObj.GetType().GetProperties();
    
    foreach (var prop in props)
    {
        var oldValue = prop.GetValue(oldObj);
        var newValue = prop.GetValue(newObj);
        
        if (!Equals(oldValue, newValue))
        {
            changes[prop.Name] = new { Old = oldValue, New = newValue };
        }
    }
    
    return JsonSerializer.Serialize(changes);
}
```

### Anomaly Detection
Detectar actividades sospechosas:
- Login desde IP/ubicación inusual
- Múltiples intentos de login fallidos
- Acceso a datos sensibles fuera de horario laboral
- Descarga masiva de datos
- Cambios en configuraciones críticas

### Retention Policy
- Audit logs: retención de 7 años (compliance)
- Security events: retención indefinida
- Data access logs: retención de 5 años (GDPR)
- Auto-archive a cold storage después de 1 año

### Alerting
Alertas automáticas para:
- 5+ login attempts fallidos en 10 min
- Acceso denegado a recursos críticos
- Eliminación de datos en producción
- Cambios en configuración de seguridad

---

## 🔗 INTEGRACIÓN CON OTROS SERVICIOS

### Todos los servicios
Cada servicio publica eventos de auditoría vía RabbitMQ:
```json
{
  "eventType": "Update",
  "action": "Vehicle.StatusChanged",
  "userId": "uuid",
  "entityType": "Vehicle",
  "entityId": "uuid",
  "oldValue": "{\"status\": \"Active\"}",
  "newValue": "{\"status\": \"Sold\"}",
  "serviceName": "VehiclesSaleService",
  "timestamp": "2026-01-07T10:30:00Z"
}
```

### AdminService
- Registrar acciones administrativas
- Dashboard de eventos de seguridad

### NotificationService
- Enviar alertas de eventos críticos

---

## 🎯 COMPLIANCE REQUIREMENTS

### GDPR (si aplica para usuarios EU)
- **Right to Access:** Usuario puede solicitar reporte de qué datos se accedieron
- **Right to be Forgotten:** Log de eliminación de datos
- **Data Breach Notification:** Registro de breaches

### SOX (si empresa es pública)
- **Audit Trail:** Trazabilidad completa de transacciones financieras
- **Segregation of Duties:** Verificar que misma persona no crea y aprueba

### Local (República Dominicana)
- **DGII:** Audit trail de facturas y NCF
- **Protección de Datos Personales:** Log de accesos a datos sensibles

---

## 🔄 EVENTOS CONSUMIDOS (RabbitMQ)

Todos los eventos importantes de otros servicios:
- `*.Created`, `*.Updated`, `*.Deleted`
- `Payment.Processed`, `User.LoggedIn`
- `Vehicle.Sold`, `Contract.Signed`

---

**Estado:** 🚧 EN DESARROLLO - No desplegado en producción  
**Versión:** 0.1.0  
**Retención:** 7 años mínimo
