# ⚙️ System Configuration - Configuración del Sistema - Matriz de Procesos

> **Servicio:** AdminService / ConfigService  
> **Base de datos:** PostgreSQL (adminservice)  
> **Última actualización:** Enero 21, 2026  
> **Estado:** 🟢 ACTIVO

---

## 📊 Resumen de Implementación

| Componente                     | Total | Implementado | Pendiente | Estado         |
| ------------------------------ | ----- | ------------ | --------- | -------------- |
| **Controllers**                | 1     | 0            | 1         | 🔴 Pendiente   |
| **CFG-BIZ-\*** (Negocio)       | 5     | 0            | 5         | 🔴 Pendiente   |
| **CFG-INT-\*** (Integraciones) | 4     | 0            | 4         | 🔴 Pendiente   |
| **CFG-UI-\*** (Interfaz)       | 3     | 0            | 3         | 🔴 Pendiente   |
| **CFG-SEC-\*** (Seguridad)     | 4     | 0            | 4         | 🔴 Pendiente   |
| **Tests**                      | 0     | 0            | 18        | 🔴 Pendiente   |
| **TOTAL**                      | 17    | 0            | 17        | 🔴 0% Completo |

---

## 1. Información General

### 1.1 Descripción

Sistema de configuración centralizada que permite modificar parámetros del sistema sin necesidad de redesplegar. Incluye configuraciones de negocio, límites, integraciones y parámetros operativos.

### 1.2 Categorías de Configuración

| Categoría         | Descripción         | Ejemplo              |
| ----------------- | ------------------- | -------------------- |
| **Business**      | Reglas de negocio   | Comisiones, límites  |
| **Features**      | Feature flags       | Módulos habilitados  |
| **Integrations**  | APIs externas       | Stripe, Azul, Twilio |
| **UI**            | Configuración de UI | Colores, textos      |
| **Notifications** | Templates y canales | Email, SMS, Push     |
| **Security**      | Seguridad           | Rate limits, 2FA     |
| **Maintenance**   | Mantenimiento       | Downtime programado  |

### 1.3 Dependencias

| Servicio     | Propósito               |
| ------------ | ----------------------- |
| Redis        | Cache de configuración  |
| RabbitMQ     | Notificación de cambios |
| AuditService | Log de cambios          |

---

## 2. Endpoints

| Método   | Endpoint                               | Descripción                      | Rol Requerido |
| -------- | -------------------------------------- | -------------------------------- | ------------- |
| `GET`    | `/api/config`                          | Listar todas las configuraciones | Admin         |
| `GET`    | `/api/config/{key}`                    | Obtener configuración            | Admin         |
| `GET`    | `/api/config/category/{category}`      | Configuraciones por categoría    | Admin         |
| `PUT`    | `/api/config/{key}`                    | Actualizar configuración         | SuperAdmin    |
| `POST`   | `/api/config`                          | Crear configuración              | SuperAdmin    |
| `DELETE` | `/api/config/{key}`                    | Eliminar configuración           | SuperAdmin    |
| `GET`    | `/api/config/{key}/history`            | Historial de cambios             | SuperAdmin    |
| `POST`   | `/api/config/{key}/rollback/{version}` | Rollback a versión               | SuperAdmin    |
| `GET`    | `/api/config/export`                   | Exportar configuración           | SuperAdmin    |
| `POST`   | `/api/config/import`                   | Importar configuración           | SuperAdmin    |

---

## 3. Entidades

### 3.1 SystemConfiguration

```csharp
public class SystemConfiguration
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string ValueType { get; set; } = "string"; // string, int, bool, json, decimal

    public string Category { get; set; } = string.Empty;
    public string? SubCategory { get; set; }
    public string Description { get; set; } = string.Empty;

    public bool IsEncrypted { get; set; }
    public bool IsReadOnly { get; set; }
    public bool RequiresRestart { get; set; }

    public string? ValidationRegex { get; set; }
    public string? MinValue { get; set; }
    public string? MaxValue { get; set; }
    public string? DefaultValue { get; set; }

    public int Version { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }

    // Audit
    public ICollection<ConfigurationHistory> History { get; set; } = new List<ConfigurationHistory>();
}
```

### 3.2 ConfigurationHistory

```csharp
public class ConfigurationHistory
{
    public Guid Id { get; set; }
    public Guid ConfigurationId { get; set; }

    public string OldValue { get; set; } = string.Empty;
    public string NewValue { get; set; } = string.Empty;
    public int Version { get; set; }

    public string? ChangeReason { get; set; }
    public Guid ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; }

    // Navigation
    public SystemConfiguration Configuration { get; set; } = null!;
}
```

---

## 4. Configuraciones del Sistema

### 4.1 Business - Comisiones y Fees

```json
{
  "Business.Commission.ListingFee.Individual": {
    "value": "1450.00",
    "type": "decimal",
    "description": "Fee por publicación individual (DOP)"
  },
  "Business.Commission.ListingFee.Featured": {
    "value": "2900.00",
    "type": "decimal",
    "description": "Fee por publicación destacada (DOP)"
  },
  "Business.Commission.PaymentGateway.Stripe": {
    "value": "3.5",
    "type": "decimal",
    "description": "Comisión Stripe (%)"
  },
  "Business.Commission.PaymentGateway.Azul": {
    "value": "2.5",
    "type": "decimal",
    "description": "Comisión Azul (%)"
  }
}
```

### 4.2 Business - Límites de Vehículos por Plan

```json
{
  "Business.Dealer.Plan.Starter.MaxListings": {
    "value": "15",
    "type": "int",
    "description": "Límite de vehículos para plan Starter"
  },
  "Business.Dealer.Plan.Pro.MaxListings": {
    "value": "50",
    "type": "int",
    "description": "Límite de vehículos para plan Pro"
  },
  "Business.Dealer.Plan.Enterprise.MaxListings": {
    "value": "-1",
    "type": "int",
    "description": "Límite de vehículos para plan Enterprise (-1 = ilimitado)"
  }
}
```

### 4.3 Business - Precios de Suscripción

```json
{
  "Business.Subscription.Starter.MonthlyPrice": {
    "value": "2499.00",
    "type": "decimal",
    "description": "Precio mensual plan Starter (DOP)"
  },
  "Business.Subscription.Pro.MonthlyPrice": {
    "value": "6499.00",
    "type": "decimal",
    "description": "Precio mensual plan Pro (DOP)"
  },
  "Business.Subscription.Enterprise.MonthlyPrice": {
    "value": "14999.00",
    "type": "decimal",
    "description": "Precio mensual plan Enterprise (DOP)"
  }
}
```

### 4.4 Integrations - Payment Gateways

```json
{
  "Integration.Stripe.Enabled": {
    "value": "true",
    "type": "bool",
    "description": "Habilitar Stripe"
  },
  "Integration.Stripe.WebhookSecret": {
    "value": "encrypted:xxxx",
    "type": "string",
    "encrypted": true,
    "description": "Stripe webhook secret"
  },
  "Integration.Azul.Enabled": {
    "value": "true",
    "type": "bool",
    "description": "Habilitar Azul"
  },
  "Integration.Azul.MerchantId": {
    "value": "encrypted:xxxx",
    "type": "string",
    "encrypted": true,
    "description": "Azul merchant ID"
  }
}
```

### 4.5 Integrations - Comunicaciones

```json
{
  "Integration.Twilio.Enabled": {
    "value": "true",
    "type": "bool",
    "description": "Habilitar Twilio (SMS/WhatsApp)"
  },
  "Integration.SendGrid.Enabled": {
    "value": "true",
    "type": "bool",
    "description": "Habilitar SendGrid (Email)"
  },
  "Integration.Firebase.Enabled": {
    "value": "true",
    "type": "bool",
    "description": "Habilitar Firebase (Push)"
  }
}
```

### 4.6 Security - Rate Limiting

```json
{
  "Security.RateLimit.Login.MaxAttempts": {
    "value": "5",
    "type": "int",
    "description": "Máximo intentos de login por minuto"
  },
  "Security.RateLimit.API.RequestsPerMinute": {
    "value": "100",
    "type": "int",
    "description": "Requests por minuto por IP"
  },
  "Security.RateLimit.Upload.MaxPerHour": {
    "value": "50",
    "type": "int",
    "description": "Uploads por hora por usuario"
  }
}
```

### 4.7 Notifications - Templates

```json
{
  "Notification.Template.WelcomeEmail": {
    "value": {
      "subject": "¡Bienvenido a OKLA!",
      "templateId": "d-abc123"
    },
    "type": "json",
    "description": "Template de email de bienvenida"
  },
  "Notification.Template.VehicleApproved": {
    "value": {
      "subject": "Tu vehículo ha sido aprobado",
      "templateId": "d-def456"
    },
    "type": "json",
    "description": "Template de vehículo aprobado"
  }
}
```

### 4.8 UI - Configuración Visual

```json
{
  "UI.Theme.PrimaryColor": {
    "value": "#3B82F6",
    "type": "string",
    "description": "Color primario de la marca"
  },
  "UI.Homepage.FeaturedSections": {
    "value": ["sedanes", "suvs", "camionetas", "deportivos"],
    "type": "json",
    "description": "Secciones del homepage"
  },
  "UI.Footer.SocialLinks": {
    "value": {
      "instagram": "https://instagram.com/okla_rd",
      "facebook": "https://facebook.com/oklard",
      "twitter": "https://twitter.com/okla_rd"
    },
    "type": "json",
    "description": "Links de redes sociales"
  }
}
```

---

## 5. Procesos Detallados

### 5.1 CFG-001: Actualizar Configuración

| Paso | Acción                              | Sistema             | Validación         |
| ---- | ----------------------------------- | ------------------- | ------------------ |
| 1    | Admin selecciona configuración      | Frontend Admin      | Config existe      |
| 2    | Ingresa nuevo valor                 | Frontend Admin      | Formato válido     |
| 3    | Verificar permisos                  | AdminService        | Es SuperAdmin      |
| 4    | Validar valor contra reglas         | ConfigService       | Validación OK      |
| 5    | Guardar valor anterior en historial | ConfigService       | Historial creado   |
| 6    | Actualizar configuración            | ConfigService       | Config actualizada |
| 7    | Incrementar versión                 | ConfigService       | Version++          |
| 8    | Invalidar cache                     | Redis               | Cache invalidado   |
| 9    | Publicar evento de cambio           | RabbitMQ            | Evento publicado   |
| 10   | Notificar a servicios               | Todos los servicios | Recargar config    |

```
┌─────────────────────────────────────────────────────────────────────────┐
│                     Configuration Update Flow                            │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│   SuperAdmin         ConfigService        Redis          RabbitMQ       │
│       │                   │                 │                │          │
│       │  PUT /config/{key}│                 │                │          │
│       │ ─────────────────▶│                 │                │          │
│       │                   │                 │                │          │
│       │                   │ Validar valor   │                │          │
│       │                   │ ───────────────▶│                │          │
│       │                   │                 │                │          │
│       │                   │ Guardar historial                │          │
│       │                   │ ─────────────┐  │                │          │
│       │                   │              │  │                │          │
│       │                   │ ◀────────────┘  │                │          │
│       │                   │                 │                │          │
│       │                   │ Actualizar DB   │                │          │
│       │                   │ ─────────────┐  │                │          │
│       │                   │              │  │                │          │
│       │                   │ ◀────────────┘  │                │          │
│       │                   │                 │                │          │
│       │                   │ DEL cache key   │                │          │
│       │                   │ ───────────────▶│                │          │
│       │                   │                 │                │          │
│       │                   │ Publish event   │                │          │
│       │                   │ ────────────────┼───────────────▶│          │
│       │                   │                 │                │          │
│       │   Config updated  │                 │                │          │
│       │ ◀─────────────────│                 │                │          │
│       │                   │                 │                │          │
│                           │                 │ ConfigChangedEvent       │
│   All Services ◀──────────┼─────────────────┼────────────────│          │
│   reload config           │                 │                │          │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

### 5.2 CFG-002: Rollback de Configuración

| Paso | Acción                            | Sistema        | Validación       |
| ---- | --------------------------------- | -------------- | ---------------- |
| 1    | Admin ve historial de config      | Frontend Admin | Historial existe |
| 2    | Selecciona versión para rollback  | Frontend Admin | Versión válida   |
| 3    | Confirmar rollback                | Frontend Admin | Confirmación     |
| 4    | Guardar valor actual en historial | ConfigService  | Historial creado |
| 5    | Restaurar valor de versión        | ConfigService  | Valor restaurado |
| 6    | Invalidar cache                   | Redis          | Cache invalidado |
| 7    | Publicar evento de cambio         | RabbitMQ       | Evento publicado |

### 5.3 CFG-003: Leer Configuración (Runtime)

```csharp
public class ConfigurationService : IConfigurationService
{
    private readonly IDistributedCache _cache;
    private readonly IConfigurationRepository _repository;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5);

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        // 1. Try cache first
        var cacheKey = $"config:{key}";
        var cached = await _cache.GetStringAsync(cacheKey, ct);

        if (!string.IsNullOrEmpty(cached))
        {
            return JsonSerializer.Deserialize<T>(cached);
        }

        // 2. Get from database
        var config = await _repository.GetByKeyAsync(key, ct);
        if (config == null)
            return default;

        // 3. Decrypt if needed
        var value = config.IsEncrypted
            ? await DecryptValueAsync(config.Value)
            : config.Value;

        // 4. Cache the value
        await _cache.SetStringAsync(cacheKey, value, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _cacheExpiration
        }, ct);

        // 5. Convert and return
        return ConvertValue<T>(value, config.ValueType);
    }
}
```

---

## 6. Reglas de Negocio

| Código  | Regla                                      | Validación                        |
| ------- | ------------------------------------------ | --------------------------------- |
| CFG-R01 | Solo SuperAdmin puede modificar configs    | Role == SuperAdmin                |
| CFG-R02 | Configs de seguridad requieren 2FA         | Action + 2FA verification         |
| CFG-R03 | Valores encriptados no se pueden exportar  | IsEncrypted → hide in export      |
| CFG-R04 | Historial se mantiene por 1 año            | RetentionDays = 365               |
| CFG-R05 | Valores numéricos deben estar en rango     | MinValue <= Value <= MaxValue     |
| CFG-R06 | Algunos configs requieren restart          | RequiresRestart → warn user       |
| CFG-R07 | ReadOnly configs no pueden modificarse     | IsReadOnly → reject update        |
| CFG-R08 | Cambios en producción requieren aprobación | Environment == Prod → 2 approvers |

---

## 7. Códigos de Error

| Código    | HTTP | Mensaje                            | Causa                         |
| --------- | ---- | ---------------------------------- | ----------------------------- |
| `CFG_001` | 404  | Configuration not found            | Key no existe                 |
| `CFG_002` | 400  | Invalid value format               | Formato incorrecto            |
| `CFG_003` | 400  | Value out of range                 | Fuera de min/max              |
| `CFG_004` | 400  | Validation failed                  | Regex no match                |
| `CFG_005` | 403  | Configuration is read-only         | No modificable                |
| `CFG_006` | 404  | Version not found                  | Versión de rollback no existe |
| `CFG_007` | 400  | Encrypted value cannot be exported | Valor encriptado              |
| `CFG_008` | 403  | 2FA required for security config   | Requiere 2FA                  |

---

## 8. Eventos RabbitMQ

| Evento                         | Exchange        | Descripción              |
| ------------------------------ | --------------- | ------------------------ |
| `ConfigurationChangedEvent`    | `config.events` | Configuración modificada |
| `ConfigurationCreatedEvent`    | `config.events` | Nueva configuración      |
| `ConfigurationDeletedEvent`    | `config.events` | Configuración eliminada  |
| `ConfigurationRolledBackEvent` | `config.events` | Rollback ejecutado       |

### 8.1 Consumer en Servicios

```csharp
public class ConfigurationChangedConsumer : IConsumer<ConfigurationChangedEvent>
{
    private readonly IConfigurationService _configService;
    private readonly ILogger<ConfigurationChangedConsumer> _logger;

    public async Task Consume(ConsumeContext<ConfigurationChangedEvent> context)
    {
        var key = context.Message.Key;

        _logger.LogInformation("Configuration changed: {Key}", key);

        // Invalidate local cache
        await _configService.InvalidateCacheAsync(key);

        // Reload if it's a critical config
        if (key.StartsWith("Security.") || key.StartsWith("Integration."))
        {
            await _configService.ReloadAsync(key);
        }
    }
}
```

---

## 9. Métricas Prometheus

```
# Configuraciones totales
system_configurations_total{category="..."}

# Cambios de configuración
configuration_changes_total{key="...", admin="..."}

# Cache hits/misses
configuration_cache_hits_total
configuration_cache_misses_total

# Tiempo de lectura de config
configuration_read_duration_seconds
```

---

## 📚 Referencias

- [02-admin-users.md](02-admin-users.md) - Usuarios admin
- [04-feature-flags.md](04-feature-flags.md) - Feature flags
