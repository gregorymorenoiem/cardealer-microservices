# 📈 Event Tracking Service - Captura de Eventos - Matriz de Procesos

> **Servicio:** EventTrackingService  
> **Puerto:** 5050  
> **Última actualización:** Enero 21, 2026  
> **Estado:** 🟢 ACTIVO

---

## 1. Información General

### 1.1 Descripción

Sistema de captura de eventos de usuario en tiempo real. Registra todas las interacciones del usuario con la plataforma para alimentar analytics, recomendaciones y lead scoring.

### 1.2 Arquitectura

```
┌─────────────────────────────────────────────────────────────────────────┐
│                     Event Tracking Architecture                          │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│   Frontend (Browser/App)                                                │
│   ┌─────────────────┐                                                   │
│   │   OKLA SDK      │──────┐                                            │
│   │   (JavaScript)  │      │                                            │
│   └─────────────────┘      │                                            │
│                            ▼                                            │
│   ┌─────────────────────────────────────────────────────────────────┐  │
│   │                    Event Collector API                           │  │
│   │                    (EventTrackingService)                        │  │
│   │                         :5050                                    │  │
│   └───────────────────────────┬─────────────────────────────────────┘  │
│                               │                                         │
│              ┌────────────────┼────────────────┐                       │
│              ▼                ▼                ▼                       │
│   ┌──────────────┐  ┌──────────────┐  ┌──────────────┐               │
│   │    Kafka     │  │    Redis     │  │  RabbitMQ    │               │
│   │  (Raw Events)│  │  (Real-time) │  │  (Consumers) │               │
│   └──────────────┘  └──────────────┘  └──────────────┘               │
│              │                │                │                       │
│              ▼                ▼                ▼                       │
│   ┌──────────────┐  ┌──────────────┐  ┌──────────────┐               │
│   │  ClickHouse  │  │  Analytics   │  │  Lead        │               │
│   │  (Storage)   │  │  Service     │  │  Scoring     │               │
│   └──────────────┘  └──────────────┘  └──────────────┘               │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

### 1.3 Dependencias

| Servicio              | Propósito          |
| --------------------- | ------------------ |
| Kafka                 | Stream de eventos  |
| Redis                 | Métricas real-time |
| ClickHouse            | Almacenamiento     |
| AnalyticsService      | Agregaciones       |
| LeadScoringService    | Scoring de leads   |
| RecommendationService | Personalización    |

---

## 2. Endpoints API

### 2.1 EventsController

| Método | Endpoint               | Descripción         | Auth | Roles  |
| ------ | ---------------------- | ------------------- | ---- | ------ |
| `POST` | `/api/events`          | Enviar evento(s)    | ❌   | Public |
| `POST` | `/api/events/batch`    | Enviar batch        | ❌   | Public |
| `POST` | `/api/events/identify` | Identificar usuario | ❌   | Public |
| `POST` | `/api/events/page`     | Page view           | ❌   | Public |
| `POST` | `/api/events/track`    | Custom event        | ❌   | Public |

### 2.2 AdminEventsController

| Método | Endpoint                      | Descripción        | Auth | Roles |
| ------ | ----------------------------- | ------------------ | ---- | ----- |
| `GET`  | `/api/admin/events/live`      | Stream en vivo     | ✅   | Admin |
| `GET`  | `/api/admin/events/stats`     | Estadísticas       | ✅   | Admin |
| `GET`  | `/api/admin/events/user/{id}` | Eventos de usuario | ✅   | Admin |
| `POST` | `/api/admin/events/replay`    | Replay eventos     | ✅   | Admin |

---

## 3. Catálogo de Eventos

### 3.1 Eventos de Navegación

| Evento           | Descripción       | Propiedades                |
| ---------------- | ----------------- | -------------------------- |
| `page_view`      | Vista de página   | url, title, referrer       |
| `page_leave`     | Salida de página  | url, duration, scrollDepth |
| `tab_change`     | Cambio de tab     | fromTab, toTab             |
| `app_open`       | Apertura de app   | version, platform          |
| `app_background` | App en background | duration                   |

### 3.2 Eventos de Vehículos

| Evento                   | Descripción         | Propiedades           |
| ------------------------ | ------------------- | --------------------- |
| `vehicle_view`           | Vista de vehículo   | vehicleId, source     |
| `vehicle_impression`     | Impresión en lista  | vehicleId, position   |
| `vehicle_click`          | Click en vehículo   | vehicleId, source     |
| `vehicle_favorite`       | Agregar a favoritos | vehicleId             |
| `vehicle_unfavorite`     | Quitar de favoritos | vehicleId             |
| `vehicle_share`          | Compartir vehículo  | vehicleId, channel    |
| `vehicle_compare_add`    | Agregar a comparar  | vehicleId             |
| `vehicle_compare_remove` | Quitar de comparar  | vehicleId             |
| `vehicle_gallery_view`   | Ver galería         | vehicleId, imageIndex |
| `vehicle_360_view`       | Ver 360°            | vehicleId             |
| `vehicle_video_play`     | Reproducir video    | vehicleId, duration   |

### 3.3 Eventos de Búsqueda

| Evento                | Descripción        | Propiedades                  |
| --------------------- | ------------------ | ---------------------------- |
| `search`              | Búsqueda realizada | query, filters, resultsCount |
| `search_filter_apply` | Aplicar filtro     | filterType, value            |
| `search_filter_clear` | Limpiar filtros    | -                            |
| `search_sort`         | Ordenar resultados | sortBy, order                |
| `search_pagination`   | Cambiar página     | page, pageSize               |
| `search_save`         | Guardar búsqueda   | searchId, name               |
| `search_no_results`   | Sin resultados     | query, filters               |

### 3.4 Eventos de Lead/Contacto

| Evento              | Descripción          | Propiedades          |
| ------------------- | -------------------- | -------------------- |
| `lead_form_view`    | Ver formulario       | vehicleId, dealerId  |
| `lead_form_start`   | Iniciar formulario   | vehicleId            |
| `lead_form_field`   | Llenar campo         | fieldName            |
| `lead_form_submit`  | Enviar formulario    | vehicleId, leadId    |
| `lead_form_abandon` | Abandonar formulario | vehicleId, lastField |
| `phone_click`       | Click en teléfono    | vehicleId, dealerId  |
| `whatsapp_click`    | Click en WhatsApp    | vehicleId, dealerId  |
| `email_click`       | Click en email       | vehicleId, dealerId  |
| `chat_start`        | Iniciar chat         | vehicleId, channel   |
| `chat_message`      | Mensaje de chat      | conversationId       |

### 3.5 Eventos de Usuario

| Evento                | Descripción         | Propiedades    |
| --------------------- | ------------------- | -------------- |
| `signup_start`        | Iniciar registro    | source         |
| `signup_complete`     | Completar registro  | userId, method |
| `login`               | Iniciar sesión      | userId, method |
| `logout`              | Cerrar sesión       | userId         |
| `profile_update`      | Actualizar perfil   | fields[]       |
| `password_reset`      | Reset de password   | -              |
| `subscription_view`   | Ver planes          | currentPlan    |
| `subscription_start`  | Iniciar suscripción | plan, price    |
| `subscription_cancel` | Cancelar            | plan, reason   |

### 3.6 Eventos de Dealer

| Evento            | Descripción       | Propiedades          |
| ----------------- | ----------------- | -------------------- |
| `vehicle_create`  | Crear vehículo    | vehicleId            |
| `vehicle_edit`    | Editar vehículo   | vehicleId, fields[]  |
| `vehicle_publish` | Publicar vehículo | vehicleId            |
| `vehicle_pause`   | Pausar vehículo   | vehicleId            |
| `vehicle_delete`  | Eliminar vehículo | vehicleId            |
| `lead_view`       | Ver lead          | leadId               |
| `lead_respond`    | Responder lead    | leadId, responseTime |
| `analytics_view`  | Ver analytics     | section              |

---

## 4. SDK JavaScript

### 4.1 Instalación

```html
<!-- Snippet de instalación -->
<script>
  !(function () {
    var o = (window.okla = window.okla || []);
    if (!o.initialize) {
      o.invoked = !0;
      o.methods = ["identify", "track", "page", "reset"];
      o.factory = function (t) {
        return function () {
          var e = Array.prototype.slice.call(arguments);
          e.unshift(t);
          o.push(e);
          return o;
        };
      };
      for (var t = 0; t < o.methods.length; t++) {
        var e = o.methods[t];
        o[e] = o.factory(e);
      }
      o.load = function (t) {
        var e = document.createElement("script");
        e.type = "text/javascript";
        e.async = !0;
        e.src = "https://cdn.okla.com.do/sdk/v1/okla.min.js";
        var n = document.getElementsByTagName("script")[0];
        n.parentNode.insertBefore(e, n);
        o.WRITE_KEY = t;
      };
      o.load("YOUR_WRITE_KEY");
    }
  })();
</script>
```

### 4.2 Uso Básico

```javascript
// Identificar usuario
okla.identify("user-123", {
  email: "juan@email.com",
  name: "Juan Pérez",
  createdAt: "2025-03-15",
});

// Track page view (automático)
okla.page();

// Track custom event
okla.track("vehicle_view", {
  vehicleId: "abc-123",
  source: "search_results",
  position: 3,
});

// Track con callback
okla.track(
  "lead_form_submit",
  {
    vehicleId: "abc-123",
    dealerId: "dealer-456",
  },
  function () {
    console.log("Event sent");
  },
);

// Reset (logout)
okla.reset();
```

### 4.3 Auto-tracking

El SDK captura automáticamente:

```javascript
// Auto-tracked events
- page_view (on navigation)
- page_leave (on unload)
- click (on buttons/links with data-track)
- form_submit (on forms with data-track-form)
- scroll (depth milestones: 25%, 50%, 75%, 100%)
- visibility (time on page)
- errors (JavaScript errors)
```

---

## 5. Estructura de Eventos

### 5.1 Schema Base

```json
{
  "event": "vehicle_view",
  "timestamp": "2026-01-21T10:30:45.123Z",
  "messageId": "uuid-v4",

  "context": {
    "page": {
      "url": "https://okla.com.do/vehiculo/toyota-rav4-2024",
      "title": "Toyota RAV4 XLE 2024 - OKLA",
      "referrer": "https://google.com"
    },
    "userAgent": "Mozilla/5.0...",
    "ip": "192.168.1.1",
    "locale": "es-DO",
    "timezone": "America/Santo_Domingo",
    "screen": {
      "width": 1920,
      "height": 1080
    },
    "device": {
      "type": "desktop",
      "manufacturer": null,
      "model": null
    },
    "os": {
      "name": "Windows",
      "version": "11"
    },
    "browser": {
      "name": "Chrome",
      "version": "120.0.0"
    },
    "campaign": {
      "source": "google",
      "medium": "cpc",
      "campaign": "q1-suv",
      "term": "toyota rav4",
      "content": "ad-variant-a"
    }
  },

  "userId": "user-123",
  "anonymousId": "anon-456",
  "sessionId": "session-789",

  "properties": {
    "vehicleId": "vehicle-abc",
    "source": "search_results",
    "position": 3
  }
}
```

### 5.2 Identificación de Usuario

```json
{
  "event": "identify",
  "timestamp": "2026-01-21T10:30:45.123Z",

  "userId": "user-123",
  "anonymousId": "anon-456",

  "traits": {
    "email": "juan@email.com",
    "name": "Juan Pérez",
    "phone": "+18295550100",
    "createdAt": "2025-03-15T08:00:00Z",
    "accountType": "Buyer",
    "plan": null,
    "location": {
      "city": "Santo Domingo",
      "country": "DO"
    }
  }
}
```

---

## 6. Procesos Detallados

### 6.1 EVT-001: Ingestión de Eventos

| Campo       | Valor                    |
| ----------- | ------------------------ |
| **ID**      | EVT-001                  |
| **Nombre**  | Event Ingestion Pipeline |
| **Actor**   | SDK/Sistema              |
| **Trigger** | POST /api/events         |

#### Flujo del Proceso

| Paso | Acción                | Sistema       | Validación           |
| ---- | --------------------- | ------------- | -------------------- |
| 1    | SDK captura evento    | Frontend      | Buffer local         |
| 2    | Batch cada 5 segundos | SDK           | Max 100 eventos      |
| 3    | POST a /api/events    | HTTP          | GZIP compressed      |
| 4    | Validar API key       | EventService  | Write key            |
| 5    | Validar schema        | EventService  | JSON Schema          |
| 6    | Enriquecer evento     | EventService  | GeoIP, User-Agent    |
| 7    | Deduplicar            | Redis         | messageId            |
| 8    | Escribir a Kafka      | Producer      | Partición por userId |
| 9    | ACK al cliente        | Response      | 202 Accepted         |
| 10   | Consumer procesa      | Worker        | Async                |
| 11   | Escribir a ClickHouse | DataWarehouse | Insert               |
| 12   | Actualizar Redis      | Cache         | Métricas RT          |
| 13   | Publicar a RabbitMQ   | MessageBus    | Para consumers       |

---

### 6.2 EVT-002: Session Stitching

| Campo       | Valor                        |
| ----------- | ---------------------------- |
| **ID**      | EVT-002                      |
| **Nombre**  | Vincular Sesiones de Usuario |
| **Actor**   | Sistema                      |
| **Trigger** | Evento identify              |

#### Flujo del Proceso

| Paso | Acción                   | Sistema      | Validación          |
| ---- | ------------------------ | ------------ | ------------------- |
| 1    | Recibir identify         | EventService | Con userId          |
| 2    | Obtener anonymousId      | Request      | Cookie/localStorage |
| 3    | Buscar eventos previos   | ClickHouse   | Por anonymousId     |
| 4    | Si hay eventos           | EventService | Actualizar          |
| 5    | UPDATE userId en eventos | ClickHouse   | Batch               |
| 6    | Crear alias              | Database     | anon -> userId      |
| 7    | Guardar en Redis         | Cache        | user_aliases        |
| 8    | Notificar servicios      | RabbitMQ     | user.identified     |

---

### 6.3 EVT-003: Real-time Metrics

| Campo       | Valor                  |
| ----------- | ---------------------- |
| **ID**      | EVT-003                |
| **Nombre**  | Actualizar Métricas RT |
| **Actor**   | Sistema                |
| **Trigger** | Cada evento            |

#### Métricas Actualizadas

```
# Por evento vehicle_view
INCR analytics:views:vehicle:{id}:today
PFADD analytics:unique:vehicle:{id}:today {userId}
ZADD analytics:top:vehicles:today {vehicleId} 1

# Por evento search
INCR analytics:searches:total:today
HINCRBY analytics:filters:today {filterName} 1

# Por evento lead_form_submit
INCR analytics:leads:total:today
INCR analytics:leads:vehicle:{id}:today
INCR analytics:leads:dealer:{id}:today

# Usuarios activos
SETEX analytics:active:user:{id} 300 1  // TTL 5 min
```

---

## 7. Configuración

### 7.1 Rate Limiting

| Límite                       | Valor       |
| ---------------------------- | ----------- |
| Eventos por segundo (global) | 10,000      |
| Eventos por segundo (por IP) | 100         |
| Batch size máximo            | 100 eventos |
| Payload máximo               | 32 KB       |

### 7.2 Configuración del Servicio

```json
{
  "EventTracking": {
    "WriteKey": "${EVENT_TRACKING_WRITE_KEY}",
    "BatchSize": 100,
    "FlushInterval": "5s",
    "MaxPayloadSize": 32768,
    "RateLimits": {
      "Global": 10000,
      "PerIP": 100
    },
    "Enrichment": {
      "GeoIP": true,
      "UserAgent": true,
      "Bot Detection": true
    }
  },
  "Kafka": {
    "Brokers": ["kafka-1:9092", "kafka-2:9092"],
    "Topic": "okla.events.raw",
    "Partitions": 12
  },
  "ClickHouse": {
    "Host": "clickhouse.okla.internal",
    "Database": "events",
    "BufferSize": 10000,
    "FlushInterval": "1s"
  }
}
```

---

## 8. Eventos RabbitMQ

| Evento            | Exchange | Payload                             |
| ----------------- | -------- | ----------------------------------- |
| `event.received`  | `events` | `{ eventType, userId, properties }` |
| `user.identified` | `events` | `{ userId, anonymousId, traits }`   |
| `session.started` | `events` | `{ sessionId, userId }`             |
| `session.ended`   | `events` | `{ sessionId, duration }`           |

---

## 9. Métricas del Servicio

```
# Ingestion
events_received_total{type="..."}
events_processed_total
events_failed_total{reason="..."}
events_batch_size
events_processing_time_ms

# Pipeline
kafka_messages_produced_total
kafka_messages_consumed_total
clickhouse_inserts_total
clickhouse_insert_latency_ms

# Rate Limiting
rate_limit_exceeded_total{limit="global|per_ip"}
```

---

## 📚 Referencias

- [02-analytics-service.md](../09-REPORTES-ANALYTICS/02-analytics-service.md) - Analytics
- [03-lead-scoring.md](../06-CRM-LEADS-CONTACTOS/03-lead-scoring.md) - Lead scoring
- [02-recommendation-service.md](../04-BUSQUEDA-RECOMENDACIONES/02-recommendation-service.md) - Recomendaciones
