# 🔔 Notificación de Vehículo Vendido

> **Código:** NOTIF-001, NOTIF-002  
> **Versión:** 1.0  
> **Última actualización:** Enero 21, 2026  
> **Criticidad:** 🟢 MEDIA (UX y engagement)

---

## 📊 Resumen de Implementación

| Componente                     | Total | Implementado | Pendiente | Estado         |
| ------------------------------ | ----- | ------------ | --------- | -------------- |
| **SOLD-DETECT-\*** (Detección) | 3     | 0            | 3         | 🔴 Pendiente   |
| **SOLD-NOTIFY-\*** (Notificar) | 4     | 0            | 4         | 🔴 Pendiente   |
| **SOLD-ALT-\*** (Alternativas) | 4     | 0            | 4         | 🔴 Pendiente   |
| **SOLD-TRACK-\*** (Tracking)   | 3     | 0            | 3         | 🔴 Pendiente   |
| **Tests**                      | 0     | 0            | 12        | 🔴 Pendiente   |
| **TOTAL**                      | 14    | 0            | 14        | 🔴 0% Completo |

---

## 📋 Información General

| Campo             | Valor                                                    |
| ----------------- | -------------------------------------------------------- |
| **Servicio**      | NotificationService (extendido)                          |
| **Puerto**        | 5008                                                     |
| **Base de Datos** | `notificationservice`                                    |
| **Dependencias**  | VehiclesSaleService, AlertService, RecommendationService |

---

## 🎯 Objetivo del Proceso

1. **Informar rápido:** Usuario sabe que vehículo ya no está disponible
2. **Reducir frustración:** Evitar intentos de contacto a vehículos vendidos
3. **Re-engagement:** Ofrecer alternativas similares inmediatamente
4. **Data:** Saber qué vehículos generan más interés

---

## 🏗️ Arquitectura

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    Vehicle Sold Notification Flow                            │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│   Event Source                       Core Service                            │
│   ┌────────────────┐              ┌─────────────────────────────────────┐   │
│   │ VehiclesSale   │──┐           │    NotificationService (extended)   │   │
│   │ Status->Sold   │  │           │  ┌───────────────────────────────┐  │   │
│   └────────────────┘  │           │  │ Vehicle Sold Handler            │  │   │
│                      │           │  │ • Detect status change          │  │   │
│   Users to Notify    │           │  │ • Find interested users         │  │   │
│   ┌────────────────┐  │           │  │ • Prepare notifications         │  │   │
│   │ Favorites      │──┼──────────▶│  └───────────────────────────────┘  │   │
│   │ Users          │  │           │  ┌───────────────────────────────┐  │   │
│   └────────────────┘  │           │  │ RecommendationService           │  │   │
│   ┌────────────────┐  │           │  │ • Find similar vehicles         │  │   │
│   │ Price Alert    │──┘           │  │ • Same make/model/year          │  │   │
│   │ Users          │              │  │ • Same price range              │  │   │
│   └────────────────┘              │  └───────────────────────────────┘  │   │
│   ┌────────────────┐              └─────────────────────────────────────┘   │
│   │ Contact        │                           │                        │
│   │ Inquirers      │               ┌───────────────┼───────────────┐        │
│   └────────────────┘               ▼               ▼               ▼        │
│   Output                     ┌────────────┐  ┌────────────┐  ┌────────────┐  │
│   ┌────────────────┐       │ PostgreSQL │  │   Redis    │  │  RabbitMQ  │  │
│   │ Email: Sold   │       │ (Sold      │  │  (Recent   │  │ (Sold     │  │
│   │ + Alternativas│◀─────│  Notifs)   │  │  Similar)  │  │  Events)   │  │
│   └────────────────┘       └────────────┘  └────────────┘  └────────────┘  │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 📡 Endpoints

| Método | Endpoint                                  | Descripción                     | Auth        |
| ------ | ----------------------------------------- | ------------------------------- | ----------- |
| `POST` | `/api/notifications/vehicle-sold`         | Trigger notificación de vendido | ✅ Internal |
| `GET`  | `/api/notifications/sold-alerts`          | Mis alertas de vendido          | ✅          |
| `PUT`  | `/api/notifications/sold-alerts/settings` | Configurar alertas              | ✅          |

---

## 🗃️ Entidades

### VehicleSoldNotification

```csharp
public class VehicleSoldNotification
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }

    // Datos del vehículo
    public string VehicleTitle { get; set; }
    public string VehicleImage { get; set; }
    public decimal VehiclePrice { get; set; }
    public string VehicleSlug { get; set; }

    // Vendedor
    public Guid SellerId { get; set; }
    public string SellerName { get; set; }

    // Stats del vehículo
    public int TotalViews { get; set; }
    public int TotalFavorites { get; set; }
    public int TotalContacts { get; set; }
    public int DaysListed { get; set; }

    // Notificaciones enviadas
    public List<SoldNotificationRecipient> Recipients { get; set; }
    public int TotalRecipients { get; set; }

    public DateTime SoldAt { get; set; }
    public DateTime NotifiedAt { get; set; }
}

public class SoldNotificationRecipient
{
    public Guid Id { get; set; }
    public Guid NotificationId { get; set; }
    public Guid UserId { get; set; }

    // Relación con el vehículo
    public RecipientRelation Relation { get; set; }

    // Envío
    public List<NotificationChannel> ChannelsSent { get; set; }
    public DateTime SentAt { get; set; }
    public DateTime? ViewedAt { get; set; }

    // Engagement con alternativas
    public bool ViewedAlternatives { get; set; }
    public int AlternativesClicked { get; set; }
}

public enum RecipientRelation
{
    Favorited,           // Lo tenía en favoritos
    Contacted,           // Contactó al vendedor
    Compared,            // Lo tenía en comparación
    AlertMatch,          // Coincidía con alerta de búsqueda
    RecentView           // Lo vio en últimos 7 días
}
```

### SoldAlertSettings

```csharp
public class SoldAlertSettings
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    // Qué notificar
    public bool NotifyFavorites { get; set; }
    public bool NotifyContacted { get; set; }
    public bool NotifyCompared { get; set; }
    public bool NotifyAlertMatches { get; set; }
    public bool NotifyRecentViews { get; set; }

    // Cómo notificar
    public bool EmailEnabled { get; set; }
    public bool PushEnabled { get; set; }
    public bool InAppEnabled { get; set; }

    // Alternativas
    public bool ShowAlternatives { get; set; }
    public int MaxAlternatives { get; set; }

    public DateTime UpdatedAt { get; set; }
}
```

---

## 📊 Proceso NOTIF-001: Notificar Vehículo Vendido

```
┌─────────────────────────────────────────────────────────────────────────┐
│ PROCESO: NOTIF-001 - Notificar Vehículo Vendido a Interesados          │
├─────────────────────────────────────────────────────────────────────────┤
│ Actor Iniciador: Sistema (evento)                                      │
│ Sistemas: NotificationService, VehiclesSaleService, RecommendationService│
│ Triggers: Vehículo marcado como vendido                                │
│ Criticidad: MEDIA                                                       │
└─────────────────────────────────────────────────────────────────────────┘
```

| Paso | Subpaso | Acción                                                  | Sistema               | Actor     | Evidencia               | Código    |
| ---- | ------- | ------------------------------------------------------- | --------------------- | --------- | ----------------------- | --------- |
| 1    | 1.1     | **Evento: VehicleSold**                                 | RabbitMQ              | Sistema   | **Event received**      | EVD-EVENT |
| 1    | 1.2     | Extraer VehicleId                                       | NotificationService   | Sistema   | VehicleId extracted     | EVD-LOG   |
| 2    | 2.1     | Obtener datos del vehículo                              | VehiclesSaleService   | Sistema   | Vehicle data            | EVD-LOG   |
| 2    | 2.2     | Obtener estadísticas                                    | VehiclesSaleService   | Sistema   | Stats fetched           | EVD-LOG   |
| 3    | 3.1     | **Buscar usuarios que lo favoriteron**                  | FavoritesDB           | Sistema   | **Favorites found**     | EVD-LOG   |
| 3    | 3.2     | **Buscar usuarios que contactaron**                     | ContactsDB            | Sistema   | **Contacts found**      | EVD-LOG   |
| 3    | 3.3     | **Buscar usuarios que compararon**                      | ComparisonsDB         | Sistema   | **Comparisons found**   | EVD-LOG   |
| 3    | 3.4     | **Buscar alertas que coincidían**                       | AlertsDB              | Sistema   | **Alerts found**        | EVD-LOG   |
| 3    | 3.5     | **Buscar vistas recientes (7 días)**                    | AnalyticsDB           | Sistema   | **Views found**         | EVD-LOG   |
| 4    | 4.1     | Deduplicar usuarios                                     | NotificationService   | Sistema   | Deduplication           | EVD-LOG   |
| 4    | 4.2     | Verificar settings de cada usuario                      | NotificationService   | Sistema   | Settings checked        | EVD-LOG   |
| 5    | 5.1     | **Buscar vehículos alternativos**                       | RecommendationService | Sistema   | **Alternatives found**  | EVD-LOG   |
| 5    | 5.2     | Basado en: misma marca, precio similar, características | RecommendationService | Sistema   | Filtering               | EVD-LOG   |
| 6    | 6.1     | **Crear VehicleSoldNotification**                       | NotificationService   | Sistema   | **Record created**      | EVD-AUDIT |
| 7    | 7.1     | Por cada usuario elegible:                              | NotificationService   | Sistema   | Loop start              | EVD-LOG   |
| 7    | 7.2     | **Enviar push notification**                            | NotificationService   | SYS-NOTIF | **Push sent**           | EVD-COMM  |
| 7    | 7.3     | **Enviar email** (si habilitado)                        | NotificationService   | SYS-NOTIF | **Email sent**          | EVD-COMM  |
| 7    | 7.4     | **Crear in-app notification**                           | NotificationService   | Sistema   | **In-app created**      | EVD-LOG   |
| 7    | 7.5     | Incluir alternativas personalizadas                     | NotificationService   | Sistema   | Alternatives included   | EVD-LOG   |
| 8    | 8.1     | **Actualizar favoritos** (quitar vehículo)              | VehiclesSaleService   | Sistema   | **Favorites updated**   | EVD-LOG   |
| 8    | 8.2     | **Actualizar comparaciones** (quitar)                   | VehiclesSaleService   | Sistema   | **Comparisons updated** | EVD-LOG   |
| 9    | 9.1     | **Audit trail**                                         | AuditService          | Sistema   | Complete audit          | EVD-AUDIT |

### Evidencia de Notificación

```json
{
  "processCode": "NOTIF-001",
  "notification": {
    "id": "sold-notif-12345",
    "vehicle": {
      "id": "veh-67890",
      "title": "Toyota Corolla 2023 - Excelente Estado",
      "image": "cdn.okla.com.do/vehicles/veh-67890/main.jpg",
      "price": 1250000,
      "slug": "toyota-corolla-2023-excelente-estado"
    },
    "seller": {
      "id": "dealer-001",
      "name": "AutoMax RD"
    },
    "stats": {
      "totalViews": 2340,
      "totalFavorites": 45,
      "totalContacts": 12,
      "daysListed": 18
    },
    "recipients": {
      "total": 52,
      "byRelation": {
        "favorited": 45,
        "contacted": 12,
        "compared": 8,
        "alertMatch": 23,
        "recentView": 156
      },
      "afterDedup": 52,
      "notified": 48,
      "optedOut": 4
    },
    "alternatives": [
      {
        "id": "veh-11111",
        "title": "Toyota Corolla 2022",
        "price": 1180000,
        "similarity": 0.92
      },
      {
        "id": "veh-22222",
        "title": "Honda Civic 2023",
        "price": 1320000,
        "similarity": 0.85
      },
      {
        "id": "veh-33333",
        "title": "Hyundai Elantra 2023",
        "price": 1150000,
        "similarity": 0.78
      }
    ],
    "timeline": {
      "soldAt": "2026-01-21T10:00:00Z",
      "notifiedAt": "2026-01-21T10:00:15Z",
      "processingMs": 15000
    }
  }
}
```

---

## 📊 Proceso NOTIF-002: Usuario Recibe Notificación

| Paso | Subpaso | Acción                                | Sistema             | Actor   | Evidencia              | Código     |
| ---- | ------- | ------------------------------------- | ------------------- | ------- | ---------------------- | ---------- |
| 1    | 1.1     | Usuario recibe push                   | Mobile/Browser      | USR-REG | Push received          | EVD-COMM   |
| 1    | 1.2     | "😢 El Toyota Corolla 2023 se vendió" | Push                | Sistema | Displayed              | EVD-LOG    |
| 2    | 2.1     | Usuario hace tap en notificación      | Mobile/Browser      | USR-REG | Tap                    | EVD-LOG    |
| 2    | 2.2     | Abrir página de "Vehículo vendido"    | Frontend            | USR-REG | Page opened            | EVD-SCREEN |
| 3    | 3.1     | Mostrar info del vehículo vendido     | Frontend            | USR-REG | Info shown             | EVD-LOG    |
| 3    | 3.2     | **Mostrar alternativas similares**    | Frontend            | USR-REG | **Alternatives shown** | EVD-SCREEN |
| 4    | 4.1     | Usuario hace clic en alternativa      | Frontend            | USR-REG | Alternative clicked    | EVD-LOG    |
| 4    | 4.2     | Ir a listing de alternativa           | Frontend            | USR-REG | Navigation             | EVD-LOG    |
| 5    | 5.1     | **Trackear engagement**               | Analytics           | Sistema | **Engagement tracked** | EVD-LOG    |
| 5    | 5.2     | Actualizar ViewedAlternatives         | NotificationService | Sistema | Record updated         | EVD-LOG    |

---

## 📱 UI Mockup - Push Notification

```
┌─────────────────────────────────────────────────────────────────────────┐
│ OKLA                                                              Ahora │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  😢 ¡Se vendió!                                                        │
│                                                                         │
│  El Toyota Corolla 2023 que guardaste ya fue vendido.                  │
│  Tenemos 3 alternativas similares para ti.                             │
│                                                                         │
│  [Ver Alternativas]                                                     │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 📱 UI Mockup - Página de Vendido

```
┌─────────────────────────────────────────────────────────────────────────┐
│ ←                   OKLA                                               │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│   ┌───────────────────────────────────────────────────────────────┐    │
│   │                                                               │    │
│   │    ┌────────────┐                                            │    │
│   │    │ [IMAGEN]   │    😢 Este vehículo ya se vendió          │    │
│   │    │  VENDIDO   │                                            │    │
│   │    │            │    Toyota Corolla 2023                     │    │
│   │    └────────────┘    RD$ 1,250,000                           │    │
│   │                                                               │    │
│   │    📊 Este vehículo tuvo:                                    │    │
│   │    • 2,340 vistas                                            │    │
│   │    • 45 usuarios lo guardaron                                │    │
│   │    • Se vendió en 18 días                                    │    │
│   │                                                               │    │
│   └───────────────────────────────────────────────────────────────┘    │
│                                                                         │
│   ✨ ALTERNATIVAS SIMILARES PARA TI                                    │
│                                                                         │
│   ┌─────────────┐  ┌─────────────┐  ┌─────────────┐                   │
│   │ [IMAGEN]    │  │ [IMAGEN]    │  │ [IMAGEN]    │                   │
│   │ Corolla '22 │  │ Civic '23   │  │ Elantra '23 │                   │
│   │ RD$1,180,000│  │ RD$1,320,000│  │ RD$1,150,000│                   │
│   │   92% ★     │  │   85% ★     │  │   78% ★     │                   │
│   └─────────────┘  └─────────────┘  └─────────────┘                   │
│                                                                         │
│   [🔔 Crear Alerta para Vehículos Similares]                           │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 📧 Template de Email

```html
Asunto: 😢 El Toyota Corolla 2023 que te gustaba ya se vendió Hola [Nombre],
Lamentamos informarte que el Toyota Corolla 2023 que guardaste en tus favoritos
ya fue vendido. ¡Pero tenemos buenas noticias! Encontramos 3 vehículos similares
que podrían interesarte: 1. Toyota Corolla 2022 - RD$ 1,180,000 (92% similar) 2.
Honda Civic 2023 - RD$ 1,320,000 (85% similar) 3. Hyundai Elantra 2023 - RD$
1,150,000 (78% similar) [Ver Alternativas] 💡 Consejo: Crea una alerta de
búsqueda para que te notifiquemos inmediatamente cuando haya un vehículo similar
disponible. [Crear Alerta de Búsqueda] El equipo de OKLA
```

---

## 📊 Métricas Prometheus

```yaml
# Notificaciones
vehicle_sold_notifications_sent_total
vehicle_sold_recipients_total{relation}
vehicle_sold_notification_latency_ms

# Engagement
vehicle_sold_notification_opened_total
vehicle_sold_alternatives_viewed_total
vehicle_sold_alternatives_clicked_total
vehicle_sold_to_new_favorite_rate
vehicle_sold_to_contact_rate

# Re-engagement
vehicle_sold_alert_created_after_total
```

---

## 🔗 Referencias

- [07-NOTIFICACIONES/01-notification-service.md](../07-NOTIFICACIONES/01-notification-service.md)
- [03-VEHICULOS-INVENTARIO/01-vehicles-sale-service.md](../03-VEHICULOS-INVENTARIO/01-vehicles-sale-service.md)
- [17-ENGAGEMENT-RETENCION/01-alertas-busquedas-guardadas.md](../17-ENGAGEMENT-RETENCION/01-alertas-busquedas-guardadas.md)
