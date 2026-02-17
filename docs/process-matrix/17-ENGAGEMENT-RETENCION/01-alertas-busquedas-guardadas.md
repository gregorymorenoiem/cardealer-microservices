# 🔔 Alertas y Búsquedas Guardadas

> **Código:** HIST-001, HIST-002, ALERT-001  
> **Versión:** 1.0  
> **Última actualización:** Enero 25, 2026  
> **Criticidad:** 🟡 MEDIA (Engagement y retención)  
> **Estado de Implementación:** ✅ Backend 100% | ✅ UI 90%

---

## ✅ AUDITORÍA DE ACCESO UI (Enero 25, 2026)

> **Estado:** ✅ CASI COMPLETO - Backend y UI implementados.

| Proceso             | Backend | UI Access | Observación               |
| ------------------- | ------- | --------- | ------------------------- |
| Alertas de precio   | ✅ 100% | ✅ 100%   | `/alerts` existe          |
| Búsquedas guardadas | ✅ 100% | ✅ 100%   | `/alerts` (tab)           |
| Favoritos           | ✅ 100% | ✅ 100%   | `/favorites`              |
| Historial vistos    | ✅ 100% | 🟡 60%    | Parcial en vehicle detail |

### Rutas UI Existentes ✅

- ✅ `/alerts` - Centro de alertas (precio + búsquedas)
- ✅ `/favorites` - Lista de favoritos
- ✅ `/comparison` - Comparador
- ✅ `/dealer/alerts` - Alertas para dealers

**Verificación Backend:** AlertService existe en `/backend/AlertService/` ✅

---

## 📊 Resumen de Implementación (ACTUALIZADO)

| Componente     | Total | Implementado | Pendiente | Estado  |
| -------------- | ----- | ------------ | --------- | ------- |
| Controllers    | 2     | 2            | 0         | ✅ 100% |
| ALERT-PRICE-\* | 4     | 4            | 0         | ✅ 100% |
| ALERT-SAVED-\* | 4     | 4            | 0         | ✅ 100% |
| ALERT-HIST-\*  | 3     | 2            | 1         | 🟡 67%  |
| ALERT-NOTIF-\* | 3     | 3            | 0         | ✅ 100% |
| Tests          | 10    | 8            | 2         | 🟡 80%  |

**Leyenda:** ✅ Implementado + Tested | 🟢 Implementado | 🟡 En Progreso | 🔴 Pendiente

---

## �📋 Información General

| Campo             | Valor                                                 |
| ----------------- | ----------------------------------------------------- |
| **Servicio**      | AlertService                                          |
| **Puerto**        | 5067                                                  |
| **Base de Datos** | `alertservice`                                        |
| **Dependencias**  | VehiclesSaleService, NotificationService, UserService |

---

## 🎯 Objetivo del Proceso

1. **Historial de Vehículos Vistos:** Recordar vehículos visitados
2. **Búsquedas Guardadas:** Guardar criterios de búsqueda para re-ejecutar
3. **Alertas de Precio:** Notificar cuando un vehículo baje de precio
4. **Alertas de Nuevos Listings:** Notificar cuando aparezcan vehículos que coincidan

---

## 🏗️ Arquitectura

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                       AlertService Architecture                             │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│   User Actions                       Core Service                            │
│   ┌────────────────┐              ┌─────────────────────────────────────┐   │
│   │ View Vehicle   │──┐           │           AlertService               │   │
│   │ (Track View)   │  │           │  ┌───────────────────────────────┐  │   │
│   └────────────────┘  │           │  │ Controllers                   │  │   │
│   ┌────────────────┐  │           │  │ • RecentlyViewedController    │  │   │
│   │ Save Search    │──┼──────────▶│  │ • SavedSearchesController     │  │   │
│   │ Criteria       │  │           │  │ • PriceAlertsController       │  │   │
│   └────────────────┘  │           │  └───────────────────────────────┘  │   │
│   ┌────────────────┐  │           │  ┌───────────────────────────────┐  │   │
│   │ Create Price   │──┘           │  │ Alert Engine                  │  │   │
│   │ Alert          │              │  │ • Price change detection      │  │   │
│   └────────────────┘              │  │ • New listing matching        │  │   │
│                                   │  │ • Notification triggers       │  │   │
│   Background Jobs                 │  └───────────────────────────────┘  │   │
│   ┌────────────────┐              │  ┌───────────────────────────────┐  │   │
│   │ Search Matcher │─────────────▶│  │ Domain                        │  │   │
│   │ (New Listings) │              │  │ • RecentlyViewed              │  │   │
│   └────────────────┘              │  │ • SavedSearch, PriceAlert     │  │   │
│   ┌────────────────┐              │  └───────────────────────────────┘  │   │
│   │ Price Checker  │─────────────▶│                                   │   │
│   │ (Every 1h)     │              └─────────────────────────────────────┘   │
│   └────────────────┘                           │                        │
│                                    ┌───────────────┼───────────────┐        │
│   Output                           ▼               ▼               ▼        │
│   ┌────────────────┐       ┌────────────┐  ┌────────────┐  ┌────────────┐  │
│   │ Notification   │       │ PostgreSQL │  │   Redis    │  │  RabbitMQ  │  │
│   │ Service        │◀─────│ (Alerts,   │  │  (Recent   │  │ (Alert    │  │
│   │ (Email/Push)   │       │  Searches) │  │  Views)    │  │  Events)   │  │
│   └────────────────┘       └────────────┘  └────────────┘  └────────────┘  │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 📡 Endpoints

| Método   | Endpoint                          | Descripción                    | Auth |
| -------- | --------------------------------- | ------------------------------ | ---- |
| `GET`    | `/api/alerts/recently-viewed`     | Vehículos vistos recientemente | ✅   |
| `POST`   | `/api/alerts/saved-searches`      | Guardar búsqueda               | ✅   |
| `GET`    | `/api/alerts/saved-searches`      | Mis búsquedas guardadas        | ✅   |
| `DELETE` | `/api/alerts/saved-searches/{id}` | Eliminar búsqueda guardada     | ✅   |
| `POST`   | `/api/alerts/price-alerts`        | Crear alerta de precio         | ✅   |
| `GET`    | `/api/alerts/price-alerts`        | Mis alertas de precio          | ✅   |
| `PUT`    | `/api/alerts/price-alerts/{id}`   | Actualizar alerta              | ✅   |
| `DELETE` | `/api/alerts/price-alerts/{id}`   | Eliminar alerta                | ✅   |

---

## 🗃️ Entidades

### RecentlyViewed

```csharp
public class RecentlyViewed
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid VehicleId { get; set; }

    // Tracking
    public int ViewCount { get; set; }
    public DateTime FirstViewedAt { get; set; }
    public DateTime LastViewedAt { get; set; }
    public int TotalSecondsViewed { get; set; }

    // Estado del vehículo al momento
    public decimal PriceAtFirstView { get; set; }
    public decimal PriceAtLastView { get; set; }
    public decimal? PriceChange { get; set; }

    // Acciones tomadas
    public bool AddedToFavorites { get; set; }
    public bool ContactedSeller { get; set; }
    public bool SharedListing { get; set; }
}
```

### SavedSearch

```csharp
public class SavedSearch
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    // Identificación
    public string Name { get; set; }                  // "Mi búsqueda de SUVs"
    public string Description { get; set; }

    // Criterios de búsqueda
    public SearchCriteria Criteria { get; set; }

    // Notificaciones
    public bool NotifyOnNewListings { get; set; }
    public NotificationFrequency Frequency { get; set; }
    public DateTime? LastNotifiedAt { get; set; }

    // Estado
    public bool IsActive { get; set; }
    public int MatchingVehiclesCount { get; set; }
    public DateTime LastExecutedAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class SearchCriteria
{
    // Vehículo
    public List<string> Makes { get; set; }
    public List<string> Models { get; set; }
    public int? YearMin { get; set; }
    public int? YearMax { get; set; }
    public List<string> BodyTypes { get; set; }

    // Precio
    public decimal? PriceMin { get; set; }
    public decimal? PriceMax { get; set; }

    // Kilometraje
    public int? MileageMin { get; set; }
    public int? MileageMax { get; set; }

    // Ubicación
    public List<string> Provinces { get; set; }
    public List<string> Cities { get; set; }

    // Características
    public List<string> FuelTypes { get; set; }
    public List<string> Transmissions { get; set; }
    public List<string> Colors { get; set; }
    public List<string> Features { get; set; }

    // Vendedor
    public SellerType? SellerType { get; set; }
    public bool? VerifiedSellersOnly { get; set; }

    // Otros
    public string Keywords { get; set; }
    public bool? HasVideo { get; set; }
    public bool? Has360View { get; set; }
}

public enum NotificationFrequency
{
    Instant,        // Inmediatamente
    Daily,          // Una vez al día
    Weekly,         // Una vez a la semana
    Never           // Sin notificaciones
}
```

### PriceAlert

```csharp
public class PriceAlert
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid VehicleId { get; set; }

    // Configuración
    public AlertType Type { get; set; }
    public decimal? TargetPrice { get; set; }          // Notificar si baja a este precio
    public decimal? PercentageDropThreshold { get; set; }  // Notificar si baja X%

    // Estado actual
    public decimal OriginalPrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal PriceChange { get; set; }
    public decimal PercentageChange { get; set; }

    // Notificaciones
    public bool IsActive { get; set; }
    public bool HasTriggered { get; set; }
    public DateTime? TriggeredAt { get; set; }
    public int NotificationsSent { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public enum AlertType
{
    PriceDrop,          // Cualquier baja de precio
    TargetPrice,        // Precio objetivo específico
    PercentageDrop,     // Baja de X%
    PriceIncrease       // Subida de precio (para vendedores)
}
```

### AlertNotification

```csharp
public class AlertNotification
{
    public Guid Id { get; set; }
    public Guid AlertId { get; set; }
    public Guid UserId { get; set; }

    public AlertNotificationType Type { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public string VehicleTitle { get; set; }
    public string VehicleImageUrl { get; set; }

    public decimal? OldPrice { get; set; }
    public decimal? NewPrice { get; set; }
    public decimal? Savings { get; set; }

    public bool IsRead { get; set; }
    public bool IsClicked { get; set; }

    public DateTime SentAt { get; set; }
    public NotificationChannel Channel { get; set; }
}

public enum AlertNotificationType
{
    PriceDropped,
    TargetPriceReached,
    NewMatchingListing,
    ListingSoldNotification,
    ListingExpiring
}

public enum NotificationChannel
{
    InApp,
    Email,
    Push,
    SMS,
    WhatsApp
}
```

---

## 📊 Proceso HIST-001: Registrar Vehículo Visto

```
┌─────────────────────────────────────────────────────────────────────────┐
│ PROCESO: HIST-001 - Registrar Vehículo Visto                           │
├─────────────────────────────────────────────────────────────────────────┤
│ Actor Iniciador: USR-REG, USR-ANON                                     │
│ Sistemas: AlertService, VehiclesSaleService                            │
│ Duración: Instantáneo (async)                                          │
│ Criticidad: BAJA                                                        │
└─────────────────────────────────────────────────────────────────────────┘
```

| Paso | Subpaso | Acción                             | Sistema              | Actor   | Evidencia          | Código    |
| ---- | ------- | ---------------------------------- | -------------------- | ------- | ------------------ | --------- |
| 1    | 1.1     | Usuario ve listing                 | Frontend             | USR-REG | Page view          | EVD-LOG   |
| 1    | 1.2     | Tracking script activo             | Frontend             | Sistema | Tracking active    | EVD-LOG   |
| 2    | 2.1     | Evento "vehicle_viewed"            | EventTrackingService | Sistema | **Event tracked**  | EVD-EVENT |
| 2    | 2.2     | Payload incluye vehicleId, userId  | EventTrackingService | Sistema | Event data         | EVD-LOG   |
| 3    | 3.1     | Verificar si existe registro       | AlertService         | Sistema | Record check       | EVD-LOG   |
| 3    | 3.2     | Si nuevo: crear RecentlyViewed     | AlertService         | Sistema | **Record created** | EVD-AUDIT |
| 3    | 3.3     | Si existe: actualizar ViewCount    | AlertService         | Sistema | Record updated     | EVD-LOG   |
| 4    | 4.1     | Capturar precio actual             | AlertService         | Sistema | Price captured     | EVD-LOG   |
| 4    | 4.2     | Calcular price change              | AlertService         | Sistema | Change calculated  | EVD-LOG   |
| 5    | 5.1     | Actualizar tiempo de visualización | AlertService         | Sistema | Time updated       | EVD-LOG   |
| 6    | 6.1     | Mantener solo últimos 50           | AlertService         | Sistema | Cleanup            | EVD-LOG   |

### Evidencia

```json
{
  "processCode": "HIST-001",
  "recentlyViewed": {
    "id": "rv-12345",
    "userId": "user-001",
    "vehicleId": "veh-67890",
    "vehicle": {
      "title": "Toyota Corolla 2023",
      "imageUrl": "cdn.okla.com.do/vehicles/67890/1.jpg"
    },
    "tracking": {
      "viewCount": 3,
      "firstViewedAt": "2026-01-19T10:30:00Z",
      "lastViewedAt": "2026-01-21T14:20:00Z",
      "totalSecondsViewed": 245
    },
    "pricing": {
      "priceAtFirstView": 1300000,
      "priceAtLastView": 1250000,
      "priceChange": -50000,
      "percentageChange": -3.85
    },
    "actions": {
      "addedToFavorites": true,
      "contactedSeller": false,
      "sharedListing": false
    }
  }
}
```

---

## 📊 Proceso HIST-002: Guardar Búsqueda

```
┌─────────────────────────────────────────────────────────────────────────┐
│ PROCESO: HIST-002 - Guardar Búsqueda                                   │
├─────────────────────────────────────────────────────────────────────────┤
│ Actor Iniciador: USR-REG                                               │
│ Sistemas: AlertService, VehiclesSaleService                            │
│ Duración: Instantáneo                                                  │
│ Criticidad: MEDIA                                                       │
└─────────────────────────────────────────────────────────────────────────┘
```

| Paso | Subpaso | Acción                          | Sistema             | Actor   | Evidencia          | Código     |
| ---- | ------- | ------------------------------- | ------------------- | ------- | ------------------ | ---------- |
| 1    | 1.1     | Usuario realiza búsqueda        | Frontend            | USR-REG | Search executed    | EVD-LOG    |
| 1    | 1.2     | Click "Guardar esta búsqueda"   | Frontend            | USR-REG | CTA clicked        | EVD-LOG    |
| 2    | 2.1     | Modal: nombrar búsqueda         | Frontend            | USR-REG | Name input         | EVD-LOG    |
| 2    | 2.2     | Seleccionar frecuencia notif    | Frontend            | USR-REG | Frequency selected | EVD-LOG    |
| 3    | 3.1     | POST /api/alerts/saved-searches | Gateway             | USR-REG | **Request**        | EVD-AUDIT  |
| 3    | 3.2     | Validar criterios               | AlertService        | Sistema | Validation         | EVD-LOG    |
| 4    | 4.1     | **Crear SavedSearch**           | AlertService        | Sistema | **Search saved**   | EVD-AUDIT  |
| 4    | 4.2     | Ejecutar búsqueda inicial       | VehiclesSaleService | Sistema | Initial count      | EVD-LOG    |
| 4    | 4.3     | Guardar MatchingVehiclesCount   | AlertService        | Sistema | Count stored       | EVD-LOG    |
| 5    | 5.1     | Confirmar al usuario            | Frontend            | USR-REG | Confirmation       | EVD-SCREEN |
| 6    | 6.1     | **Audit trail**                 | AuditService        | Sistema | Complete audit     | EVD-AUDIT  |

### [Proceso de Notificación - Scheduler]

| Paso | Subpaso | Acción                               | Sistema             | Actor     | Evidencia             | Código   |
| ---- | ------- | ------------------------------------ | ------------------- | --------- | --------------------- | -------- |
| 7    | 7.1     | Scheduler ejecuta cada hora          | SYS-SCHEDULER       | Sistema   | Cron executed         | EVD-LOG  |
| 7    | 7.2     | Obtener searches con notificación    | AlertService        | Sistema   | Searches fetched      | EVD-LOG  |
| 8    | 8.1     | Para cada search: ejecutar criterios | VehiclesSaleService | Sistema   | Search executed       | EVD-LOG  |
| 8    | 8.2     | Comparar con count anterior          | AlertService        | Sistema   | Comparison            | EVD-LOG  |
| 9    | 9.1     | Si hay nuevos: preparar notificación | AlertService        | Sistema   | Notification prep     | EVD-LOG  |
| 9    | 9.2     | **Enviar notificación**              | NotificationService | SYS-NOTIF | **Notification sent** | EVD-COMM |
| 10   | 10.1    | Actualizar LastNotifiedAt            | AlertService        | Sistema   | Timestamp updated     | EVD-LOG  |
| 10   | 10.2    | Actualizar MatchingVehiclesCount     | AlertService        | Sistema   | Count updated         | EVD-LOG  |

### Evidencia de Búsqueda Guardada

```json
{
  "processCode": "HIST-002",
  "savedSearch": {
    "id": "search-12345",
    "userId": "user-001",
    "name": "SUVs Toyota en Santo Domingo",
    "criteria": {
      "makes": ["Toyota"],
      "bodyTypes": ["SUV"],
      "yearMin": 2020,
      "yearMax": 2026,
      "priceMin": 1000000,
      "priceMax": 3000000,
      "provinces": ["Santo Domingo"],
      "fuelTypes": ["Gasolina", "Híbrido"],
      "verifiedSellersOnly": true
    },
    "notification": {
      "enabled": true,
      "frequency": "DAILY",
      "channels": ["EMAIL", "PUSH"]
    },
    "stats": {
      "matchingVehicles": 23,
      "lastExecutedAt": "2026-01-21T10:00:00Z",
      "lastNotifiedAt": "2026-01-20T08:00:00Z",
      "newSinceLastNotification": 2
    },
    "createdAt": "2026-01-15T12:00:00Z"
  }
}
```

---

## 📊 Proceso ALERT-001: Alerta de Precio

```
┌─────────────────────────────────────────────────────────────────────────┐
│ PROCESO: ALERT-001 - Crear Alerta de Precio                           │
├─────────────────────────────────────────────────────────────────────────┤
│ Actor Iniciador: USR-REG                                               │
│ Sistemas: AlertService, VehiclesSaleService, NotificationService       │
│ Duración: Instantáneo + monitoreo continuo                             │
│ Criticidad: MEDIA                                                       │
└─────────────────────────────────────────────────────────────────────────┘
```

| Paso | Subpaso | Acción                              | Sistema      | Actor   | Evidencia         | Código     |
| ---- | ------- | ----------------------------------- | ------------ | ------- | ----------------- | ---------- |
| 1    | 1.1     | Usuario ve listing                  | Frontend     | USR-REG | Listing viewed    | EVD-LOG    |
| 1    | 1.2     | Click "Alertarme si baja"           | Frontend     | USR-REG | CTA clicked       | EVD-LOG    |
| 2    | 2.1     | Modal: configurar alerta            | Frontend     | USR-REG | Config modal      | EVD-SCREEN |
| 2    | 2.2     | Seleccionar tipo de alerta          | Frontend     | USR-REG | Type selected     | EVD-LOG    |
| 2    | 2.3     | Ingresar precio objetivo (opcional) | Frontend     | USR-REG | Target input      | EVD-LOG    |
| 3    | 3.1     | POST /api/alerts/price-alerts       | Gateway      | USR-REG | **Request**       | EVD-AUDIT  |
| 3    | 3.2     | Validar vehículo activo             | AlertService | Sistema | Validation        | EVD-LOG    |
| 4    | 4.1     | **Crear PriceAlert**                | AlertService | Sistema | **Alert created** | EVD-AUDIT  |
| 4    | 4.2     | Capturar precio actual              | AlertService | Sistema | Price captured    | EVD-LOG    |
| 5    | 5.1     | Confirmar al usuario                | Frontend     | USR-REG | Confirmation      | EVD-SCREEN |
| 6    | 6.1     | **Audit trail**                     | AuditService | Sistema | Complete audit    | EVD-AUDIT  |

### [Trigger de Alerta - Event Driven]

| Paso | Subpaso | Acción                             | Sistema             | Actor      | Evidencia         | Código    |
| ---- | ------- | ---------------------------------- | ------------------- | ---------- | ----------------- | --------- |
| 7    | 7.1     | Vendedor actualiza precio          | VehiclesSaleService | USR-SELLER | Price updated     | EVD-EVENT |
| 7    | 7.2     | Evento "vehicle.price.changed"     | RabbitMQ            | Sistema    | Event published   | EVD-EVENT |
| 8    | 8.1     | AlertService recibe evento         | AlertService        | Sistema    | Event received    | EVD-LOG   |
| 8    | 8.2     | Buscar alertas para este vehículo  | AlertService        | Sistema    | Alerts fetched    | EVD-LOG   |
| 9    | 9.1     | Evaluar condiciones de cada alerta | AlertService        | Sistema    | **Evaluation**    | EVD-AUDIT |
| 9    | 9.2     | Si cumple: marcar HasTriggered     | AlertService        | Sistema    | Alert triggered   | EVD-LOG   |
| 10   | 10.1    | **Preparar notificación**          | AlertService        | Sistema    | Notification prep | EVD-LOG   |
| 10   | 10.2    | **Enviar notificación**            | NotificationService | SYS-NOTIF  | **Alert sent**    | EVD-COMM  |
| 11   | 11.1    | Actualizar stats de alerta         | AlertService        | Sistema    | Stats updated     | EVD-LOG   |
| 11   | 11.2    | Incrementar NotificationsSent      | AlertService        | Sistema    | Counter updated   | EVD-LOG   |

### Evidencia de Alerta de Precio

```json
{
  "processCode": "ALERT-001",
  "priceAlert": {
    "id": "alert-12345",
    "userId": "user-001",
    "vehicle": {
      "id": "veh-67890",
      "title": "Honda Accord 2022",
      "imageUrl": "cdn.okla.com.do/vehicles/67890/1.jpg",
      "currentUrl": "okla.com.do/vehicles/honda-accord-2022-123"
    },
    "configuration": {
      "type": "TARGET_PRICE",
      "targetPrice": 1100000,
      "originalPrice": 1250000,
      "percentageDropThreshold": null
    },
    "status": {
      "isActive": true,
      "hasTriggered": true,
      "triggeredAt": "2026-01-21T15:30:00Z"
    },
    "priceHistory": [
      { "date": "2026-01-15", "price": 1250000 },
      { "date": "2026-01-18", "price": 1200000 },
      { "date": "2026-01-21", "price": 1100000 }
    ],
    "notification": {
      "sent": true,
      "sentAt": "2026-01-21T15:30:15Z",
      "channels": ["EMAIL", "PUSH"],
      "message": "¡Buenas noticias! El Honda Accord 2022 que sigues ahora cuesta RD$1,100,000 - ¡RD$150,000 menos que tu alerta!"
    },
    "createdAt": "2026-01-15T12:00:00Z",
    "expiresAt": "2026-04-15T12:00:00Z"
  }
}
```

---

## 📧 Templates de Notificación

### Alerta de Baja de Precio

```
📉 ¡El precio bajó!

Hola {nombre},

El {vehicleTitle} que estás siguiendo ahora tiene un precio de
RD${newPrice} - ¡RD${savings} menos que antes!

💰 Precio anterior: RD${oldPrice}
💰 Precio actual: RD${newPrice}
📉 Ahorro: RD${savings} ({percentageChange}%)

[Ver vehículo ahora]

Este vehículo tiene {otherInterestedCount} personas interesadas.
No dejes pasar esta oportunidad.

---
Para dejar de recibir alertas: [Desuscribir]
```

### Nuevos Vehículos que Coinciden

```
🚗 ¡Nuevos vehículos para ti!

Hola {nombre},

Encontramos {newCount} vehículos nuevos que coinciden con tu
búsqueda guardada "{searchName}":

1. {vehicle1Title} - RD${vehicle1Price}
   📍 {vehicle1Location}
   [Ver →]

2. {vehicle2Title} - RD${vehicle2Price}
   📍 {vehicle2Location}
   [Ver →]

3. {vehicle3Title} - RD${vehicle3Price}
   📍 {vehicle3Location}
   [Ver →]

[Ver todos los {totalMatching} vehículos]

---
Frecuencia actual: {frequency}
[Cambiar frecuencia] | [Eliminar búsqueda]
```

---

## 📊 Métricas Prometheus

```yaml
# Recently Viewed
alerts_recently_viewed_total
alerts_recently_viewed_returning_rate  # % que vuelve a ver

# Saved Searches
alerts_saved_searches_total
alerts_saved_searches_active
alerts_saved_search_notification_rate

# Price Alerts
alerts_price_alerts_total
alerts_price_alerts_triggered_total
alerts_price_alert_conversion_rate  # % que contacta después

# Notifications
alerts_notifications_sent_total{type, channel}
alerts_notifications_clicked_rate
alerts_notifications_unsubscribe_rate
```

---

## 🔗 Referencias

- [03-VEHICULOS-INVENTARIO/01-vehicles-sale-service.md](../03-VEHICULOS-INVENTARIO/01-vehicles-sale-service.md)
- [06-NOTIFICACIONES/01-notification-service.md](../06-NOTIFICACIONES/01-notification-service.md)
- [02-USUARIOS-DEALERS/01-user-service.md](../02-USUARIOS-DEALERS/01-user-service.md)
