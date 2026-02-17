# ⚙️ Configuration Service - Servicio de Configuración - Matriz de Procesos

> **Servicio:** ConfigurationService (no implementado)  
> **Puerto:** 5070  
> **Última actualización:** Enero 25, 2026  
> **Estado de Implementación:** 🔴 0% Backend | 🔴 0% UI (Usando appsettings.json)

---

## ⚠️ AUDITORÍA DE ACCESO UI (Enero 25, 2026)

| Proceso        | Backend | UI Access | Observación                   |
| -------------- | ------- | --------- | ----------------------------- |
| Config Reading | ✅ 100% | N/A       | Via IConfiguration nativo     |
| Config Writing | 🔴 0%   | 🔴 0%     | No implementado dinámicamente |
| Config Caching | ✅ 100% | N/A       | In-memory config              |
| Config Refresh | 🔴 0%   | 🔴 0%     | Requiere restart              |

### Rutas UI Existentes ✅

- N/A - Configuración vía archivos appsettings.json y variables de entorno K8s

### Rutas UI Faltantes 🔴

- `/admin/config` - Editor de configuración dinámica (nice-to-have)

**Nota:** Actualmente usando `appsettings.json` + K8s ConfigMaps. Un servicio de configuración dinámica es opcional para fase 2.

---

## 📊 Resumen de Implementación

| Componente                       | Total | Implementado | Pendiente | Estado         |
| -------------------------------- | ----- | ------------ | --------- | -------------- |
| **Controllers**                  | 1     | 0            | 1         | 🔴 Pendiente   |
| **CFG-GET-\*** (Lectura)         | 4     | 0            | 4         | 🔴 Pendiente   |
| **CFG-SET-\*** (Escritura)       | 4     | 0            | 4         | 🔴 Pendiente   |
| **CFG-CACHE-\*** (Caché)         | 3     | 0            | 3         | 🔴 Pendiente   |
| **CFG-NOTIFY-\*** (Notificación) | 3     | 0            | 3         | 🔴 Pendiente   |
| **Tests**                        | 0     | 0            | 15        | 🔴 Pendiente   |
| **TOTAL**                        | 15    | 0            | 15        | 🔴 0% Completo |

---

## 1. Información General

### 1.1 Descripción

Servicio centralizado de configuración para la plataforma OKLA. Gestiona configuraciones dinámicas que pueden cambiar sin redeployment, incluyendo feature flags, parámetros de negocio, límites de rate limiting, y configuraciones de integraciones externas.

### 1.2 Tipos de Configuración

| Tipo               | Descripción               | Ejemplo                    |
| ------------------ | ------------------------- | -------------------------- |
| **Feature Flags**  | On/Off de funcionalidades | `earlybird.enabled = true` |
| **Business Rules** | Parámetros de negocio     | `listing.maxPhotos = 20`   |
| **Rate Limits**    | Límites por endpoint      | `api.rateLimit = 100/min`  |
| **Integration**    | Keys y endpoints externos | `stripe.mode = live`       |
| **UI Config**      | Configuración de frontend | `banner.message = "..."`   |

### 1.3 Arquitectura

```
┌─────────────────────────────────────────────────────────────────────────┐
│                   Configuration Service Architecture                     │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│   Admin Panel                                                           │
│   ┌─────────────────────────────────────────────────────────────────┐   │
│   │  Configuration Manager UI                                        │   │
│   │  - Edit configurations                                           │   │
│   │  - View history                                                  │   │
│   │  - Schedule changes                                              │   │
│   │  - Environment comparison                                        │   │
│   └───────────────────────────────┬─────────────────────────────────┘   │
│                                   │                                      │
│                                   ▼                                      │
│   ┌─────────────────────────────────────────────────────────────────┐   │
│   │                  ConfigurationService API                        │   │
│   │                                                                   │   │
│   │   ┌─────────────────────────────────────────────────────────┐   │   │
│   │   │                Configuration Store                       │   │   │
│   │   │                                                          │   │   │
│   │   │   PostgreSQL (Persistent)                               │   │   │
│   │   │   └── config_entries                                    │   │   │
│   │   │   └── config_history                                    │   │   │
│   │   │   └── scheduled_changes                                 │   │   │
│   │   │                                                          │   │   │
│   │   │   Redis (Cache)                                         │   │   │
│   │   │   └── config:{namespace}:{key}                          │   │   │
│   │   └─────────────────────────────────────────────────────────┘   │   │
│   └───────────────────────────────┬─────────────────────────────────┘   │
│                                   │                                      │
│              ┌────────────────────┼────────────────────┐                │
│              │                    │                    │                 │
│              ▼                    ▼                    ▼                 │
│   ┌──────────────────┐ ┌──────────────────┐ ┌──────────────────┐        │
│   │   AuthService    │ │  VehiclesSvc     │ │   Frontend       │        │
│   │   (SDK Client)   │ │  (SDK Client)    │ │   (HTTP Poll)    │        │
│   └──────────────────┘ └──────────────────┘ └──────────────────┘        │
│                                                                          │
│   Change Propagation:                                                   │
│   1. Config updated in PostgreSQL                                       │
│   2. Redis cache invalidated                                            │
│   3. RabbitMQ event: ConfigChangedEvent                                 │
│   4. Services receive event and reload                                  │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 2. Endpoints

### 2.1 Configuration CRUD

| Método   | Endpoint                        | Descripción              | Auth       |
| -------- | ------------------------------- | ------------------------ | ---------- |
| `GET`    | `/api/config`                   | Listar todas las configs | Admin      |
| `GET`    | `/api/config/{namespace}`       | Configs por namespace    | Service    |
| `GET`    | `/api/config/{namespace}/{key}` | Config específica        | Service    |
| `PUT`    | `/api/config/{namespace}/{key}` | Actualizar config        | Admin      |
| `DELETE` | `/api/config/{namespace}/{key}` | Eliminar config          | SuperAdmin |

### 2.2 Bulk Operations

| Método | Endpoint             | Descripción          | Auth  |
| ------ | -------------------- | -------------------- | ----- |
| `POST` | `/api/config/bulk`   | Actualizar múltiples | Admin |
| `POST` | `/api/config/import` | Importar desde JSON  | Admin |
| `GET`  | `/api/config/export` | Exportar a JSON      | Admin |

### 2.3 History & Scheduling

| Método   | Endpoint                                 | Descripción          | Auth  |
| -------- | ---------------------------------------- | -------------------- | ----- |
| `GET`    | `/api/config/{namespace}/{key}/history`  | Historial de cambios | Admin |
| `POST`   | `/api/config/{namespace}/{key}/rollback` | Rollback a versión   | Admin |
| `POST`   | `/api/config/schedule`                   | Programar cambio     | Admin |
| `GET`    | `/api/config/schedule`                   | Ver programados      | Admin |
| `DELETE` | `/api/config/schedule/{id}`              | Cancelar programado  | Admin |

### 2.4 Client Endpoints

| Método | Endpoint                         | Descripción          | Auth    |
| ------ | -------------------------------- | -------------------- | ------- |
| `GET`  | `/api/config/client/{namespace}` | Batch para servicios | Service |
| `GET`  | `/api/config/frontend`           | Config para UI       | Public  |

---

## 3. Entidades

### 3.1 ConfigEntry

```csharp
public class ConfigEntry
{
    public Guid Id { get; set; }
    public string Namespace { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public ConfigValueType ValueType { get; set; }

    // Metadata
    public string? Description { get; set; }
    public bool IsSecret { get; set; }
    public bool IsEnvironmentSpecific { get; set; }

    // Constraints
    public string? ValidationRegex { get; set; }
    public string? AllowedValues { get; set; } // JSON array
    public string? DefaultValue { get; set; }

    // Audit
    public Guid? LastModifiedById { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int Version { get; set; }
}

public enum ConfigValueType
{
    String,
    Integer,
    Decimal,
    Boolean,
    Json,
    ConnectionString,
    Secret
}
```

### 3.2 ConfigHistory

```csharp
public class ConfigHistory
{
    public Guid Id { get; set; }
    public Guid ConfigEntryId { get; set; }

    public string OldValue { get; set; } = string.Empty;
    public string NewValue { get; set; } = string.Empty;
    public int OldVersion { get; set; }
    public int NewVersion { get; set; }

    public Guid ChangedById { get; set; }
    public string ChangedByName { get; set; } = string.Empty;
    public string? ChangeReason { get; set; }

    public DateTime ChangedAt { get; set; }
}
```

### 3.3 ScheduledConfigChange

```csharp
public class ScheduledConfigChange
{
    public Guid Id { get; set; }
    public Guid ConfigEntryId { get; set; }

    public string NewValue { get; set; } = string.Empty;
    public DateTime ScheduledFor { get; set; }
    public ScheduleStatus Status { get; set; }

    public Guid CreatedById { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExecutedAt { get; set; }
    public string? ExecutionResult { get; set; }
}

public enum ScheduleStatus
{
    Pending,
    Executed,
    Failed,
    Cancelled
}
```

---

## 4. Namespaces de Configuración

### 4.1 Estructura de Namespaces

```yaml
global:
  maintenance.active: false
  maintenance.message: ""
  earlybird.enabled: true
  earlybird.endDate: "2026-01-31"

auth:
  jwt.expiryMinutes: 60
  jwt.refreshDays: 30
  2fa.enabled: true
  passwordReset.expiryMinutes: 15

billing:
  stripe.mode: "live"
  azul.enabled: true
  currency: "DOP"

vehicles:
  listing.maxPhotos: 20
  listing.maxVideoSizeMB: 100
  moderation.autoApprove: false

dealers:
  starter.maxListings: 15
  pro.maxListings: 50
  enterprise.maxListings: 999999

notifications:
  email.enabled: true
  sms.enabled: true
  whatsapp.enabled: true

ratelimits:
  api.default: "100/minute"
  api.auth: "10/minute"
  api.search: "50/minute"

frontend:
  banner.enabled: false
  banner.message: ""
  banner.type: "info"
  theme.primaryColor: "#2563eb"
```

---

## 5. Procesos Detallados

### 5.1 CONFIG-001: Actualizar Configuración

| Paso | Acción                    | Sistema       | Validación         |
| ---- | ------------------------- | ------------- | ------------------ |
| 1    | Admin abre config manager | Frontend      | Admin auth         |
| 2    | Selecciona namespace/key  | Frontend      | Config exists      |
| 3    | Edita valor               | Frontend      | Validation         |
| 4    | Agrega razón del cambio   | Frontend      | Reason optional    |
| 5    | Submit                    | Frontend      | Form valid         |
| 6    | Validar nuevo valor       | ConfigService | Type + regex       |
| 7    | Crear historial           | PostgreSQL    | History saved      |
| 8    | Actualizar config         | PostgreSQL    | Version++          |
| 9    | Invalidar cache Redis     | Redis         | DEL key            |
| 10   | Publicar evento           | RabbitMQ      | ConfigChangedEvent |
| 11   | Servicios recargan        | All services  | Config reloaded    |

```csharp
public class UpdateConfigCommandHandler : IRequestHandler<UpdateConfigCommand, ConfigEntry>
{
    public async Task<ConfigEntry> Handle(UpdateConfigCommand request, CancellationToken ct)
    {
        var config = await _repository.GetAsync(request.Namespace, request.Key, ct);
        if (config == null)
            throw new NotFoundException("Configuration not found");

        // 1. Validate new value
        ValidateValue(config, request.NewValue);

        // 2. Create history entry
        var history = new ConfigHistory
        {
            ConfigEntryId = config.Id,
            OldValue = config.Value,
            NewValue = request.NewValue,
            OldVersion = config.Version,
            NewVersion = config.Version + 1,
            ChangedById = request.AdminId,
            ChangedByName = request.AdminName,
            ChangeReason = request.Reason,
            ChangedAt = DateTime.UtcNow
        };

        await _historyRepository.AddAsync(history, ct);

        // 3. Update config
        config.Value = request.NewValue;
        config.Version++;
        config.UpdatedAt = DateTime.UtcNow;
        config.LastModifiedById = request.AdminId;

        await _repository.UpdateAsync(config, ct);

        // 4. Invalidate Redis cache
        var cacheKey = $"config:{request.Namespace}:{request.Key}";
        await _cache.RemoveAsync(cacheKey, ct);

        // 5. Publish event for all services
        await _eventBus.PublishAsync(new ConfigChangedEvent
        {
            Namespace = request.Namespace,
            Key = request.Key,
            NewValue = config.IsSecret ? "[REDACTED]" : request.NewValue,
            ChangedById = request.AdminId,
            Version = config.Version
        }, ct);

        _logger.LogInformation(
            "Config {Namespace}:{Key} updated to version {Version} by {Admin}",
            request.Namespace, request.Key, config.Version, request.AdminName);

        return config;
    }
}
```

### 5.2 CONFIG-002: Cargar Configuración en Servicio

```csharp
// SDK Client para servicios
public class ConfigurationClient : IConfigurationClient
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _localCache;
    private readonly ILogger _logger;
    private ConcurrentDictionary<string, string> _configs = new();

    public async Task InitializeAsync(string namespace, CancellationToken ct = default)
    {
        // Load all configs for namespace
        var response = await _httpClient.GetAsync($"/api/config/client/{namespace}", ct);
        response.EnsureSuccessStatusCode();

        var configs = await response.Content.ReadFromJsonAsync<List<ConfigDto>>(ct);

        foreach (var config in configs!)
        {
            _configs[config.Key] = config.Value;
        }

        _logger.LogInformation("Loaded {Count} configs for namespace {Namespace}",
            configs.Count, namespace);
    }

    public string Get(string key, string defaultValue = "")
    {
        return _configs.TryGetValue(key, out var value) ? value : defaultValue;
    }

    public T Get<T>(string key, T defaultValue = default!)
    {
        if (!_configs.TryGetValue(key, out var value))
            return defaultValue;

        return typeof(T) switch
        {
            Type t when t == typeof(bool) => (T)(object)bool.Parse(value),
            Type t when t == typeof(int) => (T)(object)int.Parse(value),
            Type t when t == typeof(decimal) => (T)(object)decimal.Parse(value),
            _ => JsonSerializer.Deserialize<T>(value)!
        };
    }

    // Handle config change events
    public void HandleConfigChanged(ConfigChangedEvent evt)
    {
        if (evt.Namespace != _namespace) return;

        _configs[evt.Key] = evt.NewValue;
        _localCache.Remove($"config:{evt.Key}");

        _logger.LogInformation("Config {Key} updated to version {Version}",
            evt.Key, evt.Version);

        // Trigger reload callbacks
        OnConfigChanged?.Invoke(evt.Key, evt.NewValue);
    }

    public event Action<string, string>? OnConfigChanged;
}

// Uso en Program.cs
builder.Services.AddConfigurationClient(options =>
{
    options.ServiceUrl = "http://configservice:8080";
    options.Namespace = "auth";
    options.RefreshInterval = TimeSpan.FromMinutes(5);
});

// Uso en código
public class AuthService
{
    private readonly IConfigurationClient _config;

    public int GetTokenExpiry()
    {
        return _config.Get<int>("jwt.expiryMinutes", 60);
    }
}
```

### 5.3 CONFIG-003: Programar Cambio de Configuración

| Paso | Acción                      | Sistema             | Validación        |
| ---- | --------------------------- | ------------------- | ----------------- |
| 1    | Admin programa cambio       | Frontend            | Future date       |
| 2    | Crear ScheduledConfigChange | ConfigService       | Schedule saved    |
| 3    | Hangfire job programado     | Scheduler           | Job scheduled     |
| 4    | En fecha/hora programada    | Hangfire            | Timer fires       |
| 5    | Ejecutar cambio             | ConfigService       | Apply change      |
| 6    | Actualizar status           | PostgreSQL          | Status = Executed |
| 7    | Notificar equipo            | NotificationService | Email/Teams       |

```
┌─────────────────────────────────────────────────────────────────────────┐
│                     Scheduled Config Change                              │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│   Now: Jan 21, 2026 10:00 AM                                            │
│                                                                          │
│   ┌─────────────────────────────────────────────────────────────────┐   │
│   │ Schedule: End Early Bird Promotion                               │   │
│   │                                                                   │   │
│   │ Config:    global:earlybird.enabled                              │   │
│   │ Current:   true                                                  │   │
│   │ New Value: false                                                 │   │
│   │ Execute:   Jan 31, 2026 11:59 PM                                 │   │
│   │ Status:    ⏳ Pending                                             │   │
│   └─────────────────────────────────────────────────────────────────┘   │
│                                                                          │
│   Timeline:                                                             │
│   ──────────────────────────────────────────────────────────────────    │
│   Jan 21        Jan 25        Jan 31 11:59 PM                          │
│   │             │             │                                         │
│   ●─────────────┼─────────────●                                         │
│   Created       |             Executed                                  │
│                 └──→ Email reminder 24h before                          │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 6. Frontend Configuration

### 6.1 Endpoint Público para UI

```typescript
// GET /api/config/frontend
interface FrontendConfig {
  // Feature Flags
  features: {
    earlyBirdEnabled: boolean;
    earlyBirdEndDate: string;
    newSearchUI: boolean;
    comparison: boolean;
    alerts: boolean;
  };

  // UI Configuration
  ui: {
    banner: {
      enabled: boolean;
      message: string;
      type: 'info' | 'warning' | 'error';
      dismissible: boolean;
    };
    theme: {
      primaryColor: string;
      logo: string;
    };
  };

  // Business Rules (public)
  limits: {
    maxPhotosPerListing: number;
    maxComparisonVehicles: number;
    maxSavedSearches: number;
  };
}

// React hook
function useConfig() {
  const { data: config } = useQuery({
    queryKey: ['frontend-config'],
    queryFn: () => configApi.getFrontendConfig(),
    staleTime: 5 * 60 * 1000, // 5 minutes
    refetchInterval: 5 * 60 * 1000,
  });

  return config;
}

// Uso
function EarlyBirdBanner() {
  const config = useConfig();

  if (!config?.features.earlyBirdEnabled) return null;

  return <Banner endDate={config.features.earlyBirdEndDate} />;
}
```

---

## 7. Reglas de Negocio

| Código  | Regla                                   | Validación         |
| ------- | --------------------------------------- | ------------------ |
| CFG-R01 | Secrets no se exponen en logs/eventos   | IsSecret check     |
| CFG-R02 | Cambios críticos requieren 2FA          | Critical namespace |
| CFG-R03 | Historial se mantiene 90 días           | Retention policy   |
| CFG-R04 | Cambios programados notifican 24h antes | Reminder job       |
| CFG-R05 | Rollback solo últimas 10 versiones      | Version limit      |
| CFG-R06 | Cache local max 5 minutos               | TTL config         |

---

## 8. Códigos de Error

| Código    | HTTP | Mensaje              | Causa             |
| --------- | ---- | -------------------- | ----------------- |
| `CFG_001` | 404  | Config not found     | No existe         |
| `CFG_002` | 400  | Invalid value        | Falla validación  |
| `CFG_003` | 400  | Invalid value type   | Tipo incorrecto   |
| `CFG_004` | 403  | Cannot modify secret | Sin permiso       |
| `CFG_005` | 409  | Version conflict     | Concurrent update |

---

## 9. Eventos RabbitMQ

| Evento                 | Exchange        | Descripción        |
| ---------------------- | --------------- | ------------------ |
| `ConfigChangedEvent`   | `config.events` | Config actualizada |
| `ConfigScheduledEvent` | `config.events` | Cambio programado  |
| `ConfigRollbackEvent`  | `config.events` | Rollback ejecutado |

---

## 10. Configuración

```json
{
  "ConfigurationService": {
    "CacheEnabled": true,
    "CacheTtlSeconds": 300,
    "HistoryRetentionDays": 90,
    "MaxVersionsForRollback": 10,
    "CriticalNamespaces": ["billing", "auth"],
    "PublicNamespaces": ["frontend"],
    "ScheduleReminderHours": 24
  }
}
```

---

## 11. Métricas Prometheus

```
# Config operations
config_updates_total{namespace="..."}
config_reads_total{namespace="...", cache="hit|miss"}

# Cache
config_cache_hit_ratio
config_cache_size

# Scheduled changes
config_scheduled_pending
config_scheduled_executed_total{status="success|failed"}
```

---

## 📚 Referencias

- [04-feature-flags.md](../12-ADMINISTRACION/04-feature-flags.md) - Feature flags
- [02-service-discovery.md](02-service-discovery.md) - Service discovery
- [01-admin-service.md](../12-ADMINISTRACION/01-admin-service.md) - Panel admin
