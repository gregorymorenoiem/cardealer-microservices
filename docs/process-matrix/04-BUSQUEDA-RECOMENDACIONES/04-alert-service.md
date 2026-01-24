# 🔔 Alert Service - Matriz de Procesos

> **Servicio:** AlertService  
> **Puerto:** 5056  
> **Última actualización:** Enero 21, 2026  
> **Estado:** 🟢 ACTIVO  
> **Estado de Implementación:** ✅ 100% Completo

---

## 📊 Resumen de Implementación

| Componente              | Total | Implementado | Pendiente | Estado  |
| ----------------------- | ----- | ------------ | --------- | ------- |
| **Controllers**         | 2     | 2            | 0         | ✅ 100% |
| **Procesos (ALERT-\*)** | 5     | 5            | 0         | ✅ 100% |
| **Procesos (SAVED-\*)** | 4     | 4            | 0         | ✅ 100% |
| **Tests Unitarios**     | 12    | 12           | 0         | ✅ 100% |

### Leyenda de Estados

- ✅ **IMPLEMENTADO Y PROBADO**: Código completo con tests
- 🟢 **IMPLEMENTADO**: Código completo, falta testing
- 🟡 **EN PROGRESO**: Implementación parcial
- 🔴 **PENDIENTE**: No implementado

---

## 1. Información General

### 1.1 Descripción

Sistema de alertas personalizadas para OKLA. Permite a los usuarios crear alertas de precio para vehículos específicos y guardar búsquedas para recibir notificaciones cuando aparezcan nuevos vehículos que coincidan con sus criterios.

### 1.2 Dependencias

| Servicio            | Propósito                          |
| ------------------- | ---------------------------------- |
| VehiclesSaleService | Información de vehículos y precios |
| NotificationService | Envío de alertas (Email/Push)      |
| SearchService       | Ejecución de búsquedas guardadas   |
| UserService         | Información del usuario            |

### 1.3 Componentes

- **PriceAlertsController**: Alertas de cambio de precio
- **SavedSearchesController**: Búsquedas guardadas con notificaciones

---

## 2. Endpoints API

### 2.1 PriceAlertsController

| Método   | Endpoint                             | Descripción                  | Auth | Roles        |
| -------- | ------------------------------------ | ---------------------------- | ---- | ------------ |
| `GET`    | `/api/pricealerts`                   | Listar mis alertas de precio | ✅   | User         |
| `GET`    | `/api/pricealerts/{id}`              | Obtener alerta específica    | ✅   | User (owner) |
| `POST`   | `/api/pricealerts`                   | Crear alerta de precio       | ✅   | User         |
| `PUT`    | `/api/pricealerts/{id}/target-price` | Actualizar precio objetivo   | ✅   | User (owner) |
| `POST`   | `/api/pricealerts/{id}/activate`     | Activar alerta               | ✅   | User (owner) |
| `POST`   | `/api/pricealerts/{id}/deactivate`   | Desactivar alerta            | ✅   | User (owner) |
| `POST`   | `/api/pricealerts/{id}/reset`        | Resetear alerta disparada    | ✅   | User (owner) |
| `DELETE` | `/api/pricealerts/{id}`              | Eliminar alerta              | ✅   | User (owner) |

### 2.2 SavedSearchesController

| Método   | Endpoint                                | Descripción                 | Auth | Roles        |
| -------- | --------------------------------------- | --------------------------- | ---- | ------------ |
| `GET`    | `/api/savedsearches`                    | Listar búsquedas guardadas  | ✅   | User         |
| `GET`    | `/api/savedsearches/{id}`               | Obtener búsqueda específica | ✅   | User (owner) |
| `POST`   | `/api/savedsearches`                    | Crear búsqueda guardada     | ✅   | User         |
| `PUT`    | `/api/savedsearches/{id}/name`          | Actualizar nombre           | ✅   | User (owner) |
| `PUT`    | `/api/savedsearches/{id}/criteria`      | Actualizar criterios        | ✅   | User (owner) |
| `PUT`    | `/api/savedsearches/{id}/notifications` | Config notificaciones       | ✅   | User (owner) |
| `POST`   | `/api/savedsearches/{id}/activate`      | Activar búsqueda            | ✅   | User (owner) |
| `POST`   | `/api/savedsearches/{id}/deactivate`    | Desactivar búsqueda         | ✅   | User (owner) |
| `DELETE` | `/api/savedsearches/{id}`               | Eliminar búsqueda           | ✅   | User (owner) |

---

## 3. Entidades y Enums

### 3.1 AlertCondition (Enum)

```csharp
public enum AlertCondition
{
    Below = 0,           // Notificar cuando precio < objetivo
    Above = 1,           // Notificar cuando precio > objetivo (raro)
    Equals = 2,          // Notificar cuando precio == objetivo
    AnyChange = 3        // Notificar cualquier cambio de precio
}
```

### 3.2 AlertStatus (Enum)

```csharp
public enum AlertStatus
{
    Active = 0,          // Monitoreando activamente
    Inactive = 1,        // Pausada por usuario
    Triggered = 2,       // Ya se disparó (requiere reset)
    Expired = 3          // Vehículo ya no disponible
}
```

### 3.3 NotificationFrequency (Enum)

```csharp
public enum NotificationFrequency
{
    Instant = 0,         // Inmediato cuando hay match
    Daily = 1,           // Resumen diario (9 AM)
    Weekly = 2,          // Resumen semanal (Lunes 9 AM)
    Never = 3            // Sin notificaciones email
}
```

### 3.4 PriceAlert (Entidad)

```csharp
public class PriceAlert
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid VehicleId { get; set; }
    public decimal TargetPrice { get; set; }
    public AlertCondition Condition { get; set; }
    public AlertStatus Status { get; set; }

    // Tracking
    public decimal OriginalPrice { get; set; }       // Precio cuando se creó
    public decimal CurrentPrice { get; set; }        // Precio actual
    public decimal? TriggeredAtPrice { get; set; }   // Precio cuando disparó

    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime? TriggeredAt { get; set; }
    public DateTime? LastCheckedAt { get; set; }
    public DateTime? DeactivatedAt { get; set; }

    // Métodos
    public void Activate();
    public void Deactivate();
    public void Reset();
    public void UpdateTargetPrice(decimal newPrice);
    public bool ShouldTrigger(decimal newPrice);
}
```

### 3.5 SavedSearch (Entidad)

```csharp
public class SavedSearch
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; }                 // "Mi búsqueda de RAV4"
    public string SearchCriteria { get; set; }       // JSON con filtros
    public bool SendEmailNotifications { get; set; }
    public NotificationFrequency Frequency { get; set; }
    public bool IsActive { get; set; }

    // Resultados
    public int LastResultCount { get; set; }
    public DateTime? LastExecutedAt { get; set; }
    public int NewVehiclesSinceLastCheck { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Métodos
    public void UpdateName(string name);
    public void UpdateSearchCriteria(string criteria);
    public void UpdateNotificationSettings(bool send, NotificationFrequency freq);
    public void Activate();
    public void Deactivate();
}
```

### 3.6 SearchCriteria (JSON Schema)

```json
{
  "makes": ["Toyota", "Honda"],
  "models": ["RAV4", "CR-V"],
  "yearMin": 2020,
  "yearMax": 2026,
  "priceMin": 800000,
  "priceMax": 2000000,
  "mileageMax": 50000,
  "bodyTypes": ["SUV"],
  "fuelTypes": ["Gasoline", "Hybrid"],
  "transmissions": ["Automatic"],
  "cities": ["Santo Domingo"],
  "features": ["Sunroof", "Leather"],
  "condition": "Used",
  "sellerTypes": ["Dealer", "Individual"]
}
```

---

## 4. Procesos Detallados

### 4.1 ALERT-PRICE-001: Crear Alerta de Precio

| Campo       | Valor                  |
| ----------- | ---------------------- |
| **ID**      | ALERT-PRICE-001        |
| **Nombre**  | Crear Alerta de Precio |
| **Actor**   | Usuario autenticado    |
| **Trigger** | POST /api/pricealerts  |

#### Flujo del Proceso

| Paso | Acción                        | Sistema             | Validación                 |
| ---- | ----------------------------- | ------------------- | -------------------------- |
| 1    | Usuario quiere ser notificado | Frontend            | En página de vehículo      |
| 2    | Ingresar precio objetivo      | Frontend            | Validar formato            |
| 3    | Enviar request                | API                 | Token JWT                  |
| 4    | Verificar vehículo existe     | VehiclesSaleService | Status = Active            |
| 5    | Verificar no existe alerta    | AlertService        | UserId + VehicleId único   |
| 6    | Obtener precio actual         | VehiclesSaleService | Para comparación           |
| 7    | Validar target price          | AlertService        | < precio actual (si Below) |
| 8    | Crear alerta                  | Database            | Status = Active            |
| 9    | Publicar evento               | RabbitMQ            | alert.price.created        |
| 10   | Retornar confirmación         | Response            | Con detalles               |

#### Request

```json
{
  "vehicleId": "uuid",
  "targetPrice": 1500000,
  "condition": "Below"
}
```

#### Response

```json
{
  "id": "uuid",
  "vehicleId": "uuid",
  "targetPrice": 1500000,
  "condition": "Below",
  "status": "Active",
  "originalPrice": 1800000,
  "currentPrice": 1800000,
  "priceDifference": 300000,
  "percentageToTarget": 16.67,
  "vehicle": {
    "title": "Toyota RAV4 2023",
    "image": "https://..."
  },
  "createdAt": "2026-01-21T10:00:00Z"
}
```

---

### 4.2 ALERT-PRICE-002: Verificación de Precios (Job)

| Campo       | Valor                  |
| ----------- | ---------------------- |
| **ID**      | ALERT-PRICE-002        |
| **Nombre**  | Check Price Alerts Job |
| **Actor**   | Sistema (Scheduled)    |
| **Trigger** | Cron: cada 15 minutos  |

#### Flujo del Proceso

| Paso | Acción                   | Sistema             | Validación               |
| ---- | ------------------------ | ------------------- | ------------------------ |
| 1    | Job scheduled inicia     | SchedulerService    | Cron expression          |
| 2    | Obtener alertas activas  | Database            | Status = Active          |
| 3    | Agrupar por VehicleId    | AlertService        | Batch para eficiencia    |
| 4    | Obtener precios actuales | VehiclesSaleService | Batch request            |
| 5    | Por cada alerta          | Loop                | Verificar condición      |
| 6    | Si precio cambió         | Check               | CurrentPrice != anterior |
| 7    | Actualizar CurrentPrice  | Database            | Guardar nuevo precio     |
| 8    | Si cumple condición      | Check               | ShouldTrigger()          |
| 9    | Marcar como Triggered    | Database            | Status = Triggered       |
| 10   | Enviar notificación      | NotificationService | Email + Push             |
| 11   | Publicar evento          | RabbitMQ            | alert.price.triggered    |

#### Lógica de Trigger

```csharp
public bool ShouldTrigger(decimal newPrice)
{
    return Condition switch
    {
        AlertCondition.Below => newPrice <= TargetPrice,
        AlertCondition.Above => newPrice >= TargetPrice,
        AlertCondition.Equals => newPrice == TargetPrice,
        AlertCondition.AnyChange => newPrice != CurrentPrice,
        _ => false
    };
}
```

---

### 4.3 ALERT-SEARCH-001: Crear Búsqueda Guardada

| Campo       | Valor                   |
| ----------- | ----------------------- |
| **ID**      | ALERT-SEARCH-001        |
| **Nombre**  | Guardar Búsqueda        |
| **Actor**   | Usuario autenticado     |
| **Trigger** | POST /api/savedsearches |

#### Flujo del Proceso

| Paso | Acción                      | Sistema       | Validación             |
| ---- | --------------------------- | ------------- | ---------------------- |
| 1    | Usuario realiza búsqueda    | Frontend      | Con filtros aplicados  |
| 2    | Click en "Guardar búsqueda" | Frontend      | Modal de configuración |
| 3    | Ingresar nombre             | Frontend      | Obligatorio            |
| 4    | Configurar notificaciones   | Frontend      | Frecuencia, email      |
| 5    | Enviar request              | API           | Token JWT              |
| 6    | Validar nombre único        | AlertService  | Para este usuario      |
| 7    | Validar criterios válidos   | AlertService  | JSON schema            |
| 8    | Ejecutar búsqueda inicial   | SearchService | Para obtener count     |
| 9    | Crear búsqueda guardada     | Database      | Con lastResultCount    |
| 10   | Publicar evento             | RabbitMQ      | search.saved.created   |

#### Request

```json
{
  "name": "Toyota RAV4 en Santo Domingo",
  "searchCriteria": {
    "makes": ["Toyota"],
    "models": ["RAV4"],
    "yearMin": 2020,
    "priceMax": 2000000,
    "cities": ["Santo Domingo"]
  },
  "sendEmailNotifications": true,
  "frequency": "Daily"
}
```

---

### 4.4 ALERT-SEARCH-002: Ejecutar Búsquedas Guardadas (Job)

| Campo       | Valor                      |
| ----------- | -------------------------- |
| **ID**      | ALERT-SEARCH-002           |
| **Nombre**  | Execute Saved Searches Job |
| **Actor**   | Sistema (Scheduled)        |
| **Trigger** | Cron: cada 1 hora          |

#### Flujo del Proceso

| Paso | Acción                       | Sistema             | Validación           |
| ---- | ---------------------------- | ------------------- | -------------------- |
| 1    | Job scheduled inicia         | SchedulerService    | Cron expression      |
| 2    | Obtener búsquedas activas    | Database            | IsActive = true      |
| 3    | Filtrar por frecuencia       | AlertService        | Solo las que aplican |
| 4    | Por cada búsqueda            | Loop                | Ejecutar             |
| 5    | Ejecutar búsqueda            | SearchService       | Con criterios        |
| 6    | Comparar resultados          | AlertService        | vs LastResultCount   |
| 7    | Identificar nuevos vehículos | AlertService        | Diff de IDs          |
| 8    | Actualizar LastResultCount   | Database            | Nuevo total          |
| 9    | Si hay nuevos                | Check               | NewVehicles > 0      |
| 10   | Enviar notificación          | NotificationService | Con lista de nuevos  |
| 11   | Actualizar LastExecutedAt    | Database            | Timestamp            |

#### Frecuencia de Ejecución

| Frequency | Cuándo ejecutar |
| --------- | --------------- |
| Instant   | Cada hora       |
| Daily     | 9:00 AM         |
| Weekly    | Lunes 9:00 AM   |
| Never     | No ejecutar     |

---

### 4.5 ALERT-NOTIF-001: Enviar Notificación de Alerta

| Campo       | Valor                         |
| ----------- | ----------------------------- |
| **ID**      | ALERT-NOTIF-001               |
| **Nombre**  | Enviar Notificación de Alerta |
| **Actor**   | Sistema                       |
| **Trigger** | Evento alert.triggered        |

#### Flujo del Proceso

| Paso | Acción               | Sistema             | Validación                 |
| ---- | -------------------- | ------------------- | -------------------------- |
| 1    | Recibir evento       | RabbitMQ Consumer   | alert.\*                   |
| 2    | Obtener usuario      | UserService         | UserId                     |
| 3    | Obtener preferencias | NotificationService | Canales habilitados        |
| 4    | Preparar template    | NotificationService | price_alert o search_alert |
| 5    | Renderizar contenido | TemplateEngine      | Con datos del vehículo     |
| 6    | Enviar Email         | SMTP                | Si habilitado              |
| 7    | Enviar Push          | Firebase            | Si habilitado              |
| 8    | Registrar envío      | Database            | NotificationLog            |

---

## 5. Reglas de Negocio

### 5.1 Límites por Usuario

| Plan          | Alertas Precio | Búsquedas Guardadas |
| ------------- | -------------- | ------------------- |
| Free          | 3              | 2                   |
| Early Bird    | 10             | 5                   |
| Buyer Premium | 25             | 15                  |
| Unlimited     | Ilimitado      | Ilimitado           |

### 5.2 Expiración de Alertas

| Condición             | Acción                          |
| --------------------- | ------------------------------- |
| Vehículo vendido      | Status = Expired                |
| Vehículo eliminado    | Status = Expired                |
| Sin actividad 90 días | Notificar, luego desactivar     |
| Alerta triggered      | Mantener 30 días, luego limpiar |

### 5.3 Validaciones

| Regla                | Validación                |
| -------------------- | ------------------------- |
| Target price (Below) | Debe ser < precio actual  |
| Target price (Above) | Debe ser > precio actual  |
| Nombre búsqueda      | 3-100 caracteres          |
| Criterios            | Al menos 1 filtro         |
| Frecuencia Instant   | Max 50 notificaciones/día |

---

## 6. Manejo de Errores

| Código | Error              | Mensaje                                       | Acción              |
| ------ | ------------------ | --------------------------------------------- | ------------------- |
| 400    | AlertExists        | "Ya existe una alerta para este vehículo"     | Editar existente    |
| 400    | InvalidTargetPrice | "El precio objetivo debe ser menor al actual" | Ajustar precio      |
| 400    | InvalidCriteria    | "Los criterios de búsqueda son inválidos"     | Verificar JSON      |
| 403    | NotOwner           | "No tienes permiso para esta alerta"          | Verificar propiedad |
| 404    | AlertNotFound      | "Alerta no encontrada"                        | Verificar ID        |
| 429    | LimitReached       | "Has alcanzado el límite de alertas"          | Upgrade plan        |

---

## 7. Eventos RabbitMQ

| Evento                    | Exchange       | Descripción             | Payload                            |
| ------------------------- | -------------- | ----------------------- | ---------------------------------- |
| `alert.price.created`     | `alert.events` | Alerta de precio creada | `{ alertId, userId, vehicleId }`   |
| `alert.price.triggered`   | `alert.events` | Alerta disparada        | `{ alertId, oldPrice, newPrice }`  |
| `alert.price.deactivated` | `alert.events` | Alerta desactivada      | `{ alertId, reason }`              |
| `alert.search.created`    | `alert.events` | Búsqueda guardada       | `{ searchId, userId, criteria }`   |
| `alert.search.matched`    | `alert.events` | Nuevos resultados       | `{ searchId, newCount, vehicles }` |

---

## 8. Templates de Notificación

### 8.1 Email de Price Alert

```html
Asunto: 🎉 ¡El vehículo que sigues bajó de precio! Hola {{userName}}, El precio
del {{vehicleTitle}} ha cambiado: Precio anterior: RD$ {{oldPrice}} Precio
actual: RD$ {{newPrice}} Tu objetivo: RD$ {{targetPrice}} ¡Has ahorrado RD$
{{savings}}! [Ver Vehículo] [Contactar Vendedor]
```

### 8.2 Email de Saved Search

```html
Asunto: 🚗 {{newCount}} nuevos vehículos coinciden con tu búsqueda Hola
{{userName}}, Encontramos {{newCount}} nuevos vehículos para "{{searchName}}":
1. {{vehicle1.title}} - RD$ {{vehicle1.price}} 2. {{vehicle2.title}} - RD$
{{vehicle2.price}} 3. {{vehicle3.title}} - RD$ {{vehicle3.price}} [Ver Todos los
Resultados]
```

---

## 9. Métricas y Monitoreo

### 9.1 Prometheus Metrics

```
# Alertas activas
alert_price_active_total{status="active|triggered|expired"}

# Búsquedas guardadas
alert_search_saved_total

# Notificaciones enviadas
alert_notifications_sent_total{type="price|search", channel="email|push"}

# Job execution
alert_job_execution_seconds{job="price_check|search_execute"}
alert_job_alerts_checked_total
alert_job_alerts_triggered_total
```

### 9.2 Alertas de Monitoreo

| Alerta              | Condición                | Severidad |
| ------------------- | ------------------------ | --------- |
| JobFailure          | Job no ejecuta en 30 min | Critical  |
| HighTriggerRate     | >100 triggers/hora       | Warning   |
| NotificationFailure | >10 fallos/hora          | Warning   |

---

## 10. Configuración

### 10.1 appsettings.json

```json
{
  "Alerts": {
    "PriceCheck": {
      "CronExpression": "*/15 * * * *",
      "BatchSize": 500,
      "MaxConcurrency": 10
    },
    "SavedSearch": {
      "CronExpression": "0 * * * *",
      "InstantCheckMinutes": 60,
      "DailyCheckHour": 9,
      "WeeklyCheckDay": 1
    },
    "Limits": {
      "Free": {
        "PriceAlerts": 3,
        "SavedSearches": 2
      },
      "Premium": {
        "PriceAlerts": 25,
        "SavedSearches": 15
      }
    },
    "Expiration": {
      "InactiveDays": 90,
      "TriggeredRetentionDays": 30
    }
  }
}
```

---

## 📚 Referencias

- [01-notification-service.md](../07-NOTIFICACIONES/01-notification-service.md) - Sistema de notificaciones
- [01-search-service.md](01-search-service.md) - Motor de búsqueda
- [01-vehicles-sale-service.md](../03-VEHICULOS-INVENTARIO/01-vehicles-sale-service.md) - Vehículos
