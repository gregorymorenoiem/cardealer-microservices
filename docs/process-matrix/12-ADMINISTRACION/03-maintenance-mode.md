# 🔧 Maintenance Mode - Modo Mantenimiento - Matriz de Procesos

> **Servicio:** MaintenanceService  
> **Puerto:** 5061  
> **Última actualización:** Enero 21, 2026  
> **Estado:** 🟢 ACTIVO

---

## 📊 Resumen de Implementación

| Componente                     | Total | Implementado | Pendiente | Estado         |
| ------------------------------ | ----- | ------------ | --------- | -------------- |
| **Controllers**                | 1     | 0            | 1         | 🔴 Pendiente   |
| **MAINT-SCHED-\*** (Programar) | 4     | 0            | 4         | 🔴 Pendiente   |
| **MAINT-ACT-\*** (Activar)     | 4     | 0            | 4         | 🔴 Pendiente   |
| **MAINT-BANNER-\*** (Banners)  | 3     | 0            | 3         | 🔴 Pendiente   |
| **MAINT-MON-\*** (Monitoreo)   | 3     | 0            | 3         | 🔴 Pendiente   |
| **Tests**                      | 0     | 0            | 15        | 🔴 Pendiente   |
| **TOTAL**                      | 15    | 0            | 15        | 🔴 0% Completo |

---

## 1. Información General

### 1.1 Descripción

Sistema para gestionar ventanas de mantenimiento programadas y emergencias. Permite mostrar banners informativos a los usuarios, bloquear acceso a funcionalidades específicas, y comunicar el estado del sistema de manera efectiva.

### 1.2 Casos de Uso

| Tipo              | Descripción                 | Ejemplo               |
| ----------------- | --------------------------- | --------------------- |
| **Scheduled**     | Mantenimiento programado    | Actualización de BD   |
| **Emergency**     | Mantenimiento de emergencia | Hotfix crítico        |
| **Partial**       | Funcionalidad específica    | PaymentService down   |
| **Informational** | Solo aviso                  | Próximo mantenimiento |

### 1.3 Arquitectura

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    Maintenance Mode Architecture                         │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│   Admin Panel                                                           │
│   ┌─────────────────────────────────────────────────────────────────┐   │
│   │  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐           │   │
│   │  │ Schedule     │  │ Activate     │  │ Monitor      │           │   │
│   │  │ Maintenance  │  │ Emergency    │  │ Status       │           │   │
│   │  └──────────────┘  └──────────────┘  └──────────────┘           │   │
│   └──────────────────────────────┬──────────────────────────────────┘   │
│                                  │                                       │
│                                  ▼                                       │
│   ┌─────────────────────────────────────────────────────────────────┐   │
│   │                    MaintenanceService API                        │   │
│   │                                                                   │   │
│   │   ┌────────────────────────────────────────────────────────┐    │   │
│   │   │                  Maintenance State                      │    │   │
│   │   │  - Current mode (active/inactive)                       │    │   │
│   │   │  - Affected services                                    │    │   │
│   │   │  - Banner message                                       │    │   │
│   │   │  - Estimated end time                                   │    │   │
│   │   └────────────────────────────────────────────────────────┘    │   │
│   └──────────────────────────────┬──────────────────────────────────┘   │
│                                  │                                       │
│              ┌───────────────────┼───────────────────┐                  │
│              │                   │                   │                   │
│              ▼                   ▼                   ▼                   │
│   ┌──────────────────┐ ┌──────────────────┐ ┌──────────────────┐        │
│   │   API Gateway    │ │   Frontend       │ │   Other          │        │
│   │   (Block routes) │ │   (Show banner)  │ │   Services       │        │
│   └──────────────────┘ └──────────────────┘ └──────────────────┘        │
│                                                                          │
│   ┌─────────────────────────────────────────────────────────────────┐   │
│   │                      Redis (State Cache)                         │   │
│   │   maintenance:current → {status, message, endTime, services}    │   │
│   └─────────────────────────────────────────────────────────────────┘   │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 2. Endpoints

### 2.1 Consulta de Estado

| Método | Endpoint                    | Descripción                 | Auth   |
| ------ | --------------------------- | --------------------------- | ------ |
| `GET`  | `/api/maintenance/current`  | Estado actual               | Public |
| `GET`  | `/api/maintenance/upcoming` | Próximos mantenimientos     | Public |
| `GET`  | `/api/maintenance/history`  | Historial de mantenimientos | Admin  |

### 2.2 Gestión de Mantenimiento

| Método   | Endpoint                      | Descripción             | Auth       |
| -------- | ----------------------------- | ----------------------- | ---------- |
| `POST`   | `/api/maintenance/schedule`   | Programar mantenimiento | Admin      |
| `POST`   | `/api/maintenance/activate`   | Activar inmediatamente  | SuperAdmin |
| `POST`   | `/api/maintenance/deactivate` | Desactivar              | SuperAdmin |
| `PUT`    | `/api/maintenance/{id}`       | Actualizar programado   | Admin      |
| `DELETE` | `/api/maintenance/{id}`       | Cancelar programado     | Admin      |

### 2.3 Banners

| Método   | Endpoint                        | Descripción     | Auth   |
| -------- | ------------------------------- | --------------- | ------ |
| `GET`    | `/api/maintenance/banners`      | Banners activos | Public |
| `POST`   | `/api/maintenance/banners`      | Crear banner    | Admin  |
| `DELETE` | `/api/maintenance/banners/{id}` | Eliminar banner | Admin  |

---

## 3. Entidades

### 3.1 MaintenanceWindow

```csharp
public class MaintenanceWindow
{
    public Guid Id { get; set; }
    public MaintenanceType Type { get; set; }
    public MaintenanceStatus Status { get; set; }
    public MaintenanceSeverity Severity { get; set; }

    // Timing
    public DateTime ScheduledStartTime { get; set; }
    public DateTime ScheduledEndTime { get; set; }
    public DateTime? ActualStartTime { get; set; }
    public DateTime? ActualEndTime { get; set; }

    // Scope
    public List<string> AffectedServices { get; set; } = new();
    public List<string> BlockedRoutes { get; set; } = new();
    public bool IsFullMaintenance { get; set; }

    // Messages
    public string TitleEs { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;
    public string MessageEs { get; set; } = string.Empty;
    public string MessageEn { get; set; } = string.Empty;

    // Audit
    public Guid CreatedById { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? ActivatedById { get; set; }
    public DateTime? ActivatedAt { get; set; }

    // Notes
    public string? InternalNotes { get; set; }
    public string? PostMortem { get; set; }
}

public enum MaintenanceType
{
    Scheduled,      // Programado con anticipación
    Emergency,      // Emergencia no programada
    PartialOutage,  // Outage parcial
    Informational   // Solo informativo
}

public enum MaintenanceStatus
{
    Scheduled,      // Programado, no activo
    Active,         // En progreso
    Completed,      // Completado
    Cancelled,      // Cancelado
    Extended        // Extendido
}

public enum MaintenanceSeverity
{
    Info,           // Informativo
    Warning,        // Advertencia
    Error,          // Afecta funcionalidad
    Critical        // Sistema no disponible
}
```

### 3.2 MaintenanceBanner

```csharp
public class MaintenanceBanner
{
    public Guid Id { get; set; }
    public BannerType Type { get; set; }
    public MaintenanceSeverity Severity { get; set; }

    // Content
    public string TitleEs { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;
    public string MessageEs { get; set; } = string.Empty;
    public string MessageEn { get; set; } = string.Empty;
    public string? LinkUrl { get; set; }
    public string? LinkText { get; set; }

    // Display
    public bool IsDismissible { get; set; } = true;
    public bool ShowCountdown { get; set; }
    public DateTime? CountdownTarget { get; set; }

    // Timing
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public bool IsActive { get; set; } = true;

    // Targeting
    public List<string> TargetPages { get; set; } = new(); // Empty = all
    public List<string> TargetRoles { get; set; } = new(); // Empty = all

    public DateTime CreatedAt { get; set; }
}

public enum BannerType
{
    TopBar,         // Banner fijo arriba
    Modal,          // Modal popup
    Toast,          // Notificación toast
    FullPage        // Página completa de mantenimiento
}
```

### 3.3 MaintenanceState (Redis)

```csharp
public class MaintenanceState
{
    public bool IsActive { get; set; }
    public bool IsFullMaintenance { get; set; }
    public MaintenanceSeverity Severity { get; set; }

    public string TitleEs { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;
    public string MessageEs { get; set; } = string.Empty;
    public string MessageEn { get; set; } = string.Empty;

    public DateTime? EstimatedEndTime { get; set; }

    public List<string> AffectedServices { get; set; } = new();
    public List<string> BlockedRoutes { get; set; } = new();

    public DateTime LastUpdated { get; set; }
}
```

---

## 4. Procesos Detallados

### 4.1 MAINT-001: Programar Mantenimiento

| Paso | Acción                                | Sistema             | Validación         |
| ---- | ------------------------------------- | ------------------- | ------------------ |
| 1    | Admin accede a panel de mantenimiento | Frontend            | Admin auth         |
| 2    | Selecciona fecha y hora               | Frontend            | Future date        |
| 3    | Define servicios afectados            | Frontend            | Services valid     |
| 4    | Escribe mensajes (ES/EN)              | Frontend            | Messages not empty |
| 5    | Submit                                | Frontend            | Form valid         |
| 6    | Crear MaintenanceWindow               | MaintenanceService  | Window saved       |
| 7    | Programar jobs                        | Scheduler           | StartJob + EndJob  |
| 8    | Enviar notificación a equipo          | NotificationService | Email/Teams        |
| 9    | Crear banner informativo              | MaintenanceService  | Banner created     |

```csharp
public class ScheduleMaintenanceCommandHandler : IRequestHandler<ScheduleMaintenanceCommand, MaintenanceWindow>
{
    public async Task<MaintenanceWindow> Handle(ScheduleMaintenanceCommand request, CancellationToken ct)
    {
        // 1. Validate timing
        if (request.StartTime <= DateTime.UtcNow)
            throw new ValidationException("Start time must be in the future");

        if (request.EndTime <= request.StartTime)
            throw new ValidationException("End time must be after start time");

        // 2. Check for conflicts
        var conflicts = await _repository.GetOverlappingAsync(
            request.StartTime, request.EndTime, ct);

        if (conflicts.Any())
            throw new ValidationException("Conflicts with existing maintenance window");

        // 3. Create maintenance window
        var window = new MaintenanceWindow
        {
            Type = MaintenanceType.Scheduled,
            Status = MaintenanceStatus.Scheduled,
            Severity = request.Severity,
            ScheduledStartTime = request.StartTime,
            ScheduledEndTime = request.EndTime,
            AffectedServices = request.AffectedServices,
            BlockedRoutes = request.BlockedRoutes,
            IsFullMaintenance = request.IsFullMaintenance,
            TitleEs = request.TitleEs,
            TitleEn = request.TitleEn,
            MessageEs = request.MessageEs,
            MessageEn = request.MessageEn,
            CreatedById = request.AdminId,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(window, ct);

        // 4. Schedule activation job
        await _scheduler.ScheduleAsync(new ActivateMaintenanceJob
        {
            MaintenanceWindowId = window.Id
        }, window.ScheduledStartTime);

        // 5. Schedule deactivation job
        await _scheduler.ScheduleAsync(new DeactivateMaintenanceJob
        {
            MaintenanceWindowId = window.Id
        }, window.ScheduledEndTime);

        // 6. Notify team
        await _notificationService.NotifyTeamAsync(new MaintenanceScheduledNotification
        {
            Window = window
        });

        // 7. Create informational banner
        if (window.ScheduledStartTime <= DateTime.UtcNow.AddHours(24))
        {
            await CreateUpcomingBannerAsync(window, ct);
        }

        return window;
    }
}
```

### 4.2 MAINT-002: Activar Mantenimiento de Emergencia

| Paso | Acción                          | Sistema             | Validación       |
| ---- | ------------------------------- | ------------------- | ---------------- |
| 1    | Incidente detectado             | Monitoring          | Alert triggered  |
| 2    | On-call activa modo emergencia  | Admin Panel         | SuperAdmin auth  |
| 3    | Selecciona servicios afectados  | Frontend            | Services list    |
| 4    | Define mensaje de emergencia    | Frontend            | Message required |
| 5    | Activar inmediatamente          | MaintenanceService  | Status = Active  |
| 6    | Actualizar Redis state          | Redis               | State cached     |
| 7    | Gateway bloquea rutas           | Gateway             | Routes blocked   |
| 8    | Frontend muestra banner crítico | Frontend            | Banner shown     |
| 9    | Notificar a usuarios activos    | NotificationService | Push/Email       |
| 10   | Notificar equipo via Teams      | NotificationService | Teams alert      |

```csharp
public class ActivateEmergencyMaintenanceCommandHandler
{
    public async Task<MaintenanceWindow> Handle(
        ActivateEmergencyMaintenanceCommand request,
        CancellationToken ct)
    {
        // 1. Create emergency window
        var window = new MaintenanceWindow
        {
            Type = MaintenanceType.Emergency,
            Status = MaintenanceStatus.Active,
            Severity = MaintenanceSeverity.Critical,
            ScheduledStartTime = DateTime.UtcNow,
            ScheduledEndTime = DateTime.UtcNow.AddHours(2), // Estimate
            ActualStartTime = DateTime.UtcNow,
            AffectedServices = request.AffectedServices,
            BlockedRoutes = request.BlockedRoutes,
            IsFullMaintenance = request.IsFullMaintenance,
            TitleEs = "⚠️ Mantenimiento de Emergencia",
            TitleEn = "⚠️ Emergency Maintenance",
            MessageEs = request.MessageEs,
            MessageEn = request.MessageEn,
            ActivatedById = request.AdminId,
            ActivatedAt = DateTime.UtcNow,
            CreatedById = request.AdminId,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(window, ct);

        // 2. Update Redis state
        var state = new MaintenanceState
        {
            IsActive = true,
            IsFullMaintenance = window.IsFullMaintenance,
            Severity = window.Severity,
            TitleEs = window.TitleEs,
            TitleEn = window.TitleEn,
            MessageEs = window.MessageEs,
            MessageEn = window.MessageEn,
            EstimatedEndTime = window.ScheduledEndTime,
            AffectedServices = window.AffectedServices,
            BlockedRoutes = window.BlockedRoutes,
            LastUpdated = DateTime.UtcNow
        };

        await _cache.SetStringAsync(
            "maintenance:current",
            JsonSerializer.Serialize(state),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
            },
            ct);

        // 3. Publish event for Gateway and other services
        await _eventBus.PublishAsync(new MaintenanceActivatedEvent
        {
            WindowId = window.Id,
            BlockedRoutes = window.BlockedRoutes,
            AffectedServices = window.AffectedServices,
            IsFullMaintenance = window.IsFullMaintenance
        }, ct);

        // 4. Notify all active users
        await _notificationService.BroadcastPushAsync(new PushNotification
        {
            Title = "⚠️ Mantenimiento en Progreso",
            Body = window.MessageEs,
            Type = PushType.MaintenanceAlert
        });

        // 5. Teams critical alert
        await _teamsNotifier.SendCriticalAlertAsync(new TeamsAlert
        {
            Title = "🚨 EMERGENCIA: Mantenimiento Activado",
            Message = window.MessageEs,
            MentionOnCall = true
        });

        return window;
    }
}
```

### 4.3 MAINT-003: Gateway Route Blocking

```
┌─────────────────────────────────────────────────────────────────────────┐
│                     Gateway Maintenance Middleware                       │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│   Request                                                               │
│     │                                                                   │
│     ▼                                                                   │
│   ┌─────────────────────────────────────────────────────────────────┐   │
│   │ MaintenanceMiddleware                                            │   │
│   │                                                                   │   │
│   │   1. Check Redis: maintenance:current                            │   │
│   │                                                                   │   │
│   │   if (maintenanceState.IsActive)                                 │   │
│   │   {                                                              │   │
│   │       2. Check if route is blocked                               │   │
│   │                                                                   │   │
│   │       if (IsBlocked(request.Path))                               │   │
│   │       {                                                          │   │
│   │           3. Return 503 Service Unavailable                      │   │
│   │           + Retry-After header                                   │   │
│   │           + JSON with maintenance info                           │   │
│   │       }                                                          │   │
│   │   }                                                              │   │
│   │                                                                   │   │
│   │   4. Continue to next middleware                                 │   │
│   └─────────────────────────────────────────────────────────────────┘   │
│     │                                                                   │
│     ▼                                                                   │
│   Other Middleware → Service                                            │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

```csharp
public class MaintenanceMiddleware
{
    private readonly IDistributedCache _cache;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var stateJson = await _cache.GetStringAsync("maintenance:current");

        if (!string.IsNullOrEmpty(stateJson))
        {
            var state = JsonSerializer.Deserialize<MaintenanceState>(stateJson)!;

            if (state.IsActive)
            {
                var path = context.Request.Path.Value ?? "";

                // Full maintenance blocks everything except health and maintenance endpoints
                if (state.IsFullMaintenance && !IsExemptPath(path))
                {
                    await ReturnMaintenanceResponse(context, state);
                    return;
                }

                // Partial maintenance checks blocked routes
                if (state.BlockedRoutes.Any(r => path.StartsWith(r)))
                {
                    await ReturnMaintenanceResponse(context, state);
                    return;
                }
            }
        }

        await next(context);
    }

    private bool IsExemptPath(string path)
    {
        return path.StartsWith("/health") ||
               path.StartsWith("/api/maintenance/current") ||
               path.StartsWith("/api/auth/refresh"); // Allow token refresh
    }

    private async Task ReturnMaintenanceResponse(HttpContext context, MaintenanceState state)
    {
        context.Response.StatusCode = 503;
        context.Response.ContentType = "application/json";

        if (state.EstimatedEndTime.HasValue)
        {
            var retryAfter = (int)(state.EstimatedEndTime.Value - DateTime.UtcNow).TotalSeconds;
            context.Response.Headers["Retry-After"] = Math.Max(60, retryAfter).ToString();
        }

        var lang = context.Request.Headers["Accept-Language"].FirstOrDefault()?.Contains("en") == true ? "en" : "es";

        var response = new
        {
            error = "SERVICE_UNAVAILABLE",
            title = lang == "en" ? state.TitleEn : state.TitleEs,
            message = lang == "en" ? state.MessageEn : state.MessageEs,
            estimatedEndTime = state.EstimatedEndTime,
            affectedServices = state.AffectedServices
        };

        await context.Response.WriteAsJsonAsync(response);
    }
}
```

---

## 5. Frontend Banner Component

```typescript
// MaintenanceBanner.tsx
export const MaintenanceBanner: React.FC = () => {
  const { data: maintenance } = useQuery({
    queryKey: ['maintenance', 'current'],
    queryFn: () => maintenanceApi.getCurrent(),
    refetchInterval: 60000, // Check every minute
  });

  const [dismissed, setDismissed] = useState(false);

  if (!maintenance?.isActive || dismissed) return null;

  const severityColors = {
    info: 'bg-blue-50 border-blue-200 text-blue-800',
    warning: 'bg-yellow-50 border-yellow-300 text-yellow-800',
    error: 'bg-orange-50 border-orange-300 text-orange-800',
    critical: 'bg-red-50 border-red-300 text-red-800',
  };

  return (
    <div className={`border-b px-4 py-3 ${severityColors[maintenance.severity]}`}>
      <div className="max-w-7xl mx-auto flex items-center justify-between">
        <div className="flex items-center gap-3">
          <span className="text-xl">
            {maintenance.severity === 'critical' ? '🚨' : '⚠️'}
          </span>
          <div>
            <p className="font-semibold">{maintenance.title}</p>
            <p className="text-sm">{maintenance.message}</p>
          </div>
        </div>

        <div className="flex items-center gap-4">
          {maintenance.estimatedEndTime && (
            <CountdownTimer target={maintenance.estimatedEndTime} />
          )}

          {maintenance.severity !== 'critical' && (
            <button onClick={() => setDismissed(true)}>
              <X className="w-5 h-5" />
            </button>
          )}
        </div>
      </div>
    </div>
  );
};
```

---

## 6. Reglas de Negocio

| Código    | Regla                                         | Validación            |
| --------- | --------------------------------------------- | --------------------- |
| MAINT-R01 | Solo SuperAdmin puede activar emergencia      | Role check            |
| MAINT-R02 | Mantenimiento programado con 24h anticipación | StartTime > Now + 24h |
| MAINT-R03 | Banner crítico no es dismissible              | IsDismissible = false |
| MAINT-R04 | Máximo 4 horas de mantenimiento programado    | Duration <= 4h        |
| MAINT-R05 | Notificar usuarios 1h antes                   | Notification job      |
| MAINT-R06 | Health endpoints siempre disponibles          | Exempt paths          |

---

## 7. Eventos RabbitMQ

| Evento                        | Exchange             | Descripción |
| ----------------------------- | -------------------- | ----------- |
| `MaintenanceScheduledEvent`   | `maintenance.events` | Programado  |
| `MaintenanceActivatedEvent`   | `maintenance.events` | Activado    |
| `MaintenanceDeactivatedEvent` | `maintenance.events` | Desactivado |
| `MaintenanceExtendedEvent`    | `maintenance.events` | Extendido   |

---

## 8. Configuración

```json
{
  "Maintenance": {
    "CacheKey": "maintenance:current",
    "DefaultDurationHours": 2,
    "MaxDurationHours": 8,
    "MinAdvanceNoticeHours": 24,
    "NotifyBeforeMinutes": [60, 15, 5],
    "ExemptPaths": ["/health", "/api/maintenance/current", "/api/auth/refresh"]
  }
}
```

---

## 9. Métricas Prometheus

```
# Maintenance windows
maintenance_windows_total{type="scheduled|emergency", status="..."}

# Duration
maintenance_duration_seconds{type="..."}

# User impact
maintenance_blocked_requests_total{route="..."}
```

---

## 📚 Referencias

- [01-admin-service.md](01-admin-service.md) - Panel de administración
- [01-gateway-service.md](../11-INFRAESTRUCTURA-DEVOPS/01-gateway-service.md) - API Gateway
