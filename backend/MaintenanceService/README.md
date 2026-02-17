# 🛠️ MaintenanceService

**Puerto**: 5061  
**Estado**: ✅ Implementado (Sprint 1)  
**Base de Datos**: PostgreSQL (maintenanceservice)

## 📝 Descripción

Servicio para gestionar ventanas de mantenimiento programables del sistema. Permite activar/desactivar el modo mantenimiento, programar ventanas futuras, y notificar a usuarios.

## 🎯 Features

- ✅ Crear ventanas de mantenimiento programadas
- ✅ Modo mantenimiento de emergencia
- ✅ Iniciar/completar/cancelar mantenimiento
- ✅ Notificaciones a usuarios
- ✅ Historial de mantenimientos
- ✅ API pública para verificar estado

## 🔌 API Endpoints

### Público

```
GET /api/maintenance/status      # Verificar si hay mantenimiento activo
GET /api/maintenance/upcoming    # Ver mantenimientos próximos (7 días)
```

### Admin (Requiere autenticación)

```
GET    /api/maintenance                  # Listar todas las ventanas
GET    /api/maintenance/{id}             # Obtener ventana específica
POST   /api/maintenance                  # Crear ventana
POST   /api/maintenance/{id}/start       # Iniciar mantenimiento
POST   /api/maintenance/{id}/complete    # Completar mantenimiento
POST   /api/maintenance/{id}/cancel      # Cancelar mantenimiento
PUT    /api/maintenance/{id}/schedule    # Reprogramar
PUT    /api/maintenance/{id}/notes       # Actualizar notas
DELETE /api/maintenance/{id}             # Eliminar ventana
```

## 📊 Modelo de Datos

```csharp
public class MaintenanceWindow
{
    Guid Id
    string Title
    string Description
    MaintenanceType Type      // Scheduled, Emergency, Database, Deployment
    MaintenanceStatus Status  // Scheduled, InProgress, Completed, Cancelled
    DateTime ScheduledStart
    DateTime ScheduledEnd
    DateTime? ActualStart
    DateTime? ActualEnd
    string CreatedBy
    bool NotifyUsers
    int NotifyMinutesBefore
    List<string> AffectedServices
}
```

## 🚀 Ejemplo de Uso

### Verificar estado (Público)

```bash
curl http://localhost:5061/api/maintenance/status
```

Respuesta:

```json
{
  "isMaintenanceMode": true,
  "maintenanceWindow": {
    "id": "...",
    "title": "Database Migration",
    "description": "Migrating PostgreSQL to version 16",
    "type": "Database",
    "status": "InProgress",
    "scheduledStart": "2026-01-10T02:00:00Z",
    "scheduledEnd": "2026-01-10T04:00:00Z",
    "actualStart": "2026-01-10T02:05:00Z",
    "isActive": true
  }
}
```

### Crear ventana (Admin)

```bash
curl -X POST http://localhost:5061/api/maintenance \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Scheduled Maintenance",
    "description": "Server updates and security patches",
    "type": "Scheduled",
    "scheduledStart": "2026-01-15T02:00:00Z",
    "scheduledEnd": "2026-01-15T04:00:00Z",
    "notifyUsers": true,
    "notifyMinutesBefore": 60,
    "affectedServices": ["gateway", "vehiclessaleservice"]
  }'
```

### Iniciar mantenimiento (Admin)

```bash
curl -X POST http://localhost:5061/api/maintenance/{id}/start \
  -H "Authorization: Bearer YOUR_TOKEN"
```

## 🔧 Configuración

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=maintenanceservice;Username=postgres;Password=postgres"
  },
  "Jwt": {
    "Secret": "your-secret-key",
    "Issuer": "CarDealerAuth",
    "Audience": "CarDealerApi"
  }
}
```

### Docker

```bash
# Build
docker build -t maintenanceservice:latest -f backend/MaintenanceService/MaintenanceService.Api/Dockerfile .

# Run
docker run -p 5061:8080 \
  -e ConnectionStrings__DefaultConnection="Host=postgres;Port=5432;Database=maintenanceservice;Username=postgres;Password=postgres123" \
  maintenanceservice:latest
```

## 🌐 Integración Frontend

```typescript
// services/maintenanceService.ts
export const checkMaintenanceStatus = async () => {
  const response = await fetch(
    "https://api.okla.com.do/api/maintenance/status"
  );
  return await response.json();
};

// App.tsx - Mostrar banner de mantenimiento
const { isMaintenanceMode, maintenanceWindow } = await checkMaintenanceStatus();

if (isMaintenanceMode) {
  return <MaintenancePage window={maintenanceWindow} />;
}
```

## 📦 Dependencias

- .NET 8.0
- Entity Framework Core 8.0
- PostgreSQL (Npgsql)
- ASP.NET Core Authentication (JWT)
- Swashbuckle (Swagger)

## 🔄 Estado del Servicio

- ✅ Domain layer
- ✅ Infrastructure layer
- ✅ API Controllers
- ✅ Database context
- ✅ Migrations
- ✅ Dockerfile
- ⏳ Frontend components (pendiente)
- ⏳ Notificaciones (integración con NotificationService)

---

**Sprint**: 1  
**Prioridad**: 🔴 CRÍTICO  
**Completitud**: 80% (backend completo, falta frontend)
