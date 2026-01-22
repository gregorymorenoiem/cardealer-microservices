# 📊 Analytics Service - Métricas y Reportes - Matriz de Procesos

> **Servicio:** AnalyticsService  
> **Puerto:** 5070  
> **Última actualización:** Enero 21, 2026  
> **Estado:** 🟢 ACTIVO

---

## 1. Información General

### 1.1 Descripción

Sistema centralizado de analytics y reportes para la plataforma OKLA. Proporciona métricas en tiempo real, dashboards para diferentes roles, y reportes automatizados para la toma de decisiones.

### 1.2 Tipos de Analytics

| Tipo                   | Descripción                        | Usuarios            |
| ---------------------- | ---------------------------------- | ------------------- |
| **Platform Analytics** | Métricas globales de la plataforma | Admins, Executives  |
| **Dealer Analytics**   | Rendimiento de dealers             | Dealers             |
| **Vehicle Analytics**  | Performance de vehículos           | Dealers, Admins     |
| **User Analytics**     | Comportamiento de usuarios         | Product, Marketing  |
| **Revenue Analytics**  | Ingresos y facturación             | Finance, Executives |

### 1.3 Dependencias

| Servicio             | Propósito          |
| -------------------- | ------------------ |
| EventTrackingService | Eventos de usuario |
| VehiclesSaleService  | Datos de vehículos |
| BillingService       | Datos de pagos     |
| LeadService          | Datos de leads     |
| UserService          | Datos de usuarios  |
| Redis                | Caché de métricas  |
| ClickHouse           | Data warehouse     |

---

## 2. Endpoints API

### 2.1 PlatformAnalyticsController

| Método | Endpoint                              | Descripción           | Auth | Roles |
| ------ | ------------------------------------- | --------------------- | ---- | ----- |
| `GET`  | `/api/analytics/platform/overview`    | Resumen general       | ✅   | Admin |
| `GET`  | `/api/analytics/platform/traffic`     | Tráfico del sitio     | ✅   | Admin |
| `GET`  | `/api/analytics/platform/conversions` | Embudos de conversión | ✅   | Admin |
| `GET`  | `/api/analytics/platform/users`       | Métricas de usuarios  | ✅   | Admin |
| `GET`  | `/api/analytics/platform/revenue`     | Métricas de ingresos  | ✅   | Admin |

### 2.2 DealerAnalyticsController

| Método | Endpoint                            | Descripción           | Auth | Roles  |
| ------ | ----------------------------------- | --------------------- | ---- | ------ |
| `GET`  | `/api/analytics/dealer/dashboard`   | Dashboard del dealer  | ✅   | Dealer |
| `GET`  | `/api/analytics/dealer/vehicles`    | Rendimiento vehículos | ✅   | Dealer |
| `GET`  | `/api/analytics/dealer/leads`       | Métricas de leads     | ✅   | Dealer |
| `GET`  | `/api/analytics/dealer/competitors` | Comparación mercado   | ✅   | Dealer |
| `GET`  | `/api/analytics/dealer/trends`      | Tendencias            | ✅   | Dealer |

### 2.3 ReportsController

| Método | Endpoint                               | Descripción         | Auth | Roles |
| ------ | -------------------------------------- | ------------------- | ---- | ----- |
| `GET`  | `/api/analytics/reports`               | Listar reportes     | ✅   | User  |
| `POST` | `/api/analytics/reports/generate`      | Generar reporte     | ✅   | User  |
| `GET`  | `/api/analytics/reports/{id}`          | Obtener reporte     | ✅   | Owner |
| `GET`  | `/api/analytics/reports/{id}/download` | Descargar PDF/Excel | ✅   | Owner |
| `POST` | `/api/analytics/reports/schedule`      | Programar reporte   | ✅   | User  |

---

## 3. Métricas Clave (KPIs)

### 3.1 Métricas de Plataforma

| Métrica               | Descripción          | Fórmula                   |
| --------------------- | -------------------- | ------------------------- |
| **MAU**               | Monthly Active Users | Usuarios únicos/mes       |
| **DAU**               | Daily Active Users   | Usuarios únicos/día       |
| **Session Duration**  | Duración promedio    | Tiempo total / Sesiones   |
| **Bounce Rate**       | Tasa de rebote       | Sesiones 1 página / Total |
| **Pages per Session** | Páginas por sesión   | Total páginas / Sesiones  |

### 3.2 Métricas de Vehículos

| Métrica             | Descripción        | Fórmula                    |
| ------------------- | ------------------ | -------------------------- |
| **Views**           | Visualizaciones    | Count de page views        |
| **CTR**             | Click-through rate | Clicks / Impressions × 100 |
| **Lead Rate**       | Tasa de leads      | Leads / Views × 100        |
| **Time on Listing** | Tiempo en listado  | Promedio segundos          |
| **Favorites Rate**  | Tasa de favoritos  | Favorites / Views × 100    |

### 3.3 Métricas de Conversión

| Métrica                  | Descripción      | Fórmula                |
| ------------------------ | ---------------- | ---------------------- |
| **View to Lead**         | Vista a lead     | Leads / Views × 100    |
| **Lead to Contact**      | Lead a contacto  | Contacts / Leads × 100 |
| **Contact to Sale**      | Contacto a venta | Sales / Contacts × 100 |
| **Overall Conversion**   | Conversión total | Sales / Views × 100    |
| **Average Time to Sale** | Tiempo a venta   | Días promedio          |

### 3.4 Métricas de Revenue

| Métrica        | Descripción               | Fórmula                 |
| -------------- | ------------------------- | ----------------------- |
| **MRR**        | Monthly Recurring Revenue | Sum(subscriptions)      |
| **ARR**        | Annual Recurring Revenue  | MRR × 12                |
| **ARPU**       | Average Revenue Per User  | Revenue / Active Users  |
| **LTV**        | Lifetime Value            | ARPU × Avg Lifespan     |
| **Churn Rate** | Tasa de cancelación       | Cancelled / Total × 100 |

---

## 4. Dashboards

### 4.1 Executive Dashboard

```
┌─────────────────────────────────────────────────────────────────────────┐
│                        OKLA Executive Dashboard                         │
│                           Enero 2026                                     │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐ │
│  │     MRR      │  │     MAU      │  │   Vehículos  │  │    Dealers   │ │
│  │  $485,000    │  │    45,230    │  │    12,450    │  │      156     │ │
│  │    ↑ 12%     │  │    ↑ 8%      │  │    ↑ 5%      │  │    ↑ 15%     │ │
│  └──────────────┘  └──────────────┘  └──────────────┘  └──────────────┘ │
│                                                                          │
│  ┌─────────────────────────────────────────────────────────────────────┐ │
│  │                    Revenue Trend (Last 12 Months)                   │ │
│  │                                                                      │ │
│  │    500K │                                              ┌───         │ │
│  │    400K │                                    ┌───┬───┬─┤            │ │
│  │    300K │              ┌───┬───┬───┬───┬───┬─┤                      │ │
│  │    200K │    ┌───┬───┬─┤                                            │ │
│  │    100K │ ───┤                                                      │ │
│  │         └────┴───┴───┴───┴───┴───┴───┴───┴───┴───┴───┴───          │ │
│  │           Feb Mar Apr May Jun Jul Aug Sep Oct Nov Dec Jan           │ │
│  └─────────────────────────────────────────────────────────────────────┘ │
│                                                                          │
│  ┌──────────────────────────────┐  ┌───────────────────────────────────┐│
│  │    Subscription Breakdown    │  │      Top Performing Dealers       ││
│  │                              │  │                                    ││
│  │  Enterprise ████████  32%   │  │  1. Autos Premium      $45,000    ││
│  │  Pro        ██████████ 45%  │  │  2. MultiAutos         $38,500    ││
│  │  Starter    ███████   23%   │  │  3. Caribe Motors      $32,100    ││
│  │                              │  │  4. Super Autos        $28,750    ││
│  │                              │  │  5. Auto Express       $25,200    ││
│  └──────────────────────────────┘  └───────────────────────────────────┘│
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

### 4.2 Dealer Dashboard

```
┌─────────────────────────────────────────────────────────────────────────┐
│                      Autos del Caribe - Dashboard                        │
│                         Enero 2026                                       │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐ │
│  │  Vehículos   │  │    Vistas    │  │    Leads     │  │    Ventas    │ │
│  │     38/50    │  │    8,450     │  │     156      │  │      12      │ │
│  │   Activos    │  │   Este mes   │  │  Este mes    │  │   Este mes   │ │
│  └──────────────┘  └──────────────┘  └──────────────┘  └──────────────┘ │
│                                                                          │
│  ┌─────────────────────────────────────────────────────────────────────┐ │
│  │                     Top 5 Vehículos por Vistas                      │ │
│  │                                                                      │ │
│  │  1. Toyota RAV4 2024        ████████████████████  1,245 views      │ │
│  │  2. Honda CR-V 2023         ███████████████       980 views        │ │
│  │  3. Hyundai Tucson 2024     ████████████          750 views        │ │
│  │  4. Nissan Rogue 2023       ██████████            620 views        │ │
│  │  5. Toyota Corolla 2024     ████████              485 views        │ │
│  └─────────────────────────────────────────────────────────────────────┘ │
│                                                                          │
│  ┌──────────────────────────────┐  ┌───────────────────────────────────┐│
│  │     Lead Funnel              │  │      Price Recommendations        ││
│  │                              │  │                                    ││
│  │  Vistas    █████████ 8,450  │  │  RAV4 2024: Precio actual $2.3M   ││
│  │  Leads     ████      156    │  │  💡 Recomendado: $2.1M-$2.2M     ││
│  │  Contactos ██        48     │  │     (-5% para mayor conversión)   ││
│  │  Ventas    █         12     │  │                                    ││
│  │                              │  │  CR-V 2023: ✅ Precio óptimo      ││
│  │  Conversión: 0.14%           │  │                                    ││
│  └──────────────────────────────┘  └───────────────────────────────────┘│
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 5. Procesos Detallados

### 5.1 ANLT-001: Recopilar Eventos de Usuario

| Campo       | Valor                     |
| ----------- | ------------------------- |
| **ID**      | ANLT-001                  |
| **Nombre**  | Event Collection Pipeline |
| **Actor**   | Sistema                   |
| **Trigger** | Cada acción de usuario    |

#### Flujo del Proceso

| Paso | Acción                  | Sistema         | Validación          |
| ---- | ----------------------- | --------------- | ------------------- |
| 1    | Usuario realiza acción  | Frontend        | Click, view, scroll |
| 2    | Capturar evento         | JS SDK          | Con metadata        |
| 3    | Enqueue evento          | EventQueue      | In-memory buffer    |
| 4    | Batch cada 5 segundos   | EventService    | Batch de eventos    |
| 5    | Enviar a backend        | API             | POST /events        |
| 6    | Validar evento          | EventService    | Schema check        |
| 7    | Enriquecer con contexto | EventService    | User, session       |
| 8    | Escribir a Kafka        | MessageQueue    | Particionado        |
| 9    | Consumer procesa        | AnalyticsWorker | Agregaciones        |
| 10   | Escribir a ClickHouse   | DataWarehouse   | Para queries        |
| 11   | Actualizar cache        | Redis           | Métricas RT         |

#### Eventos Trackeados

| Evento               | Datos                   |
| -------------------- | ----------------------- |
| `page_view`          | url, referrer, duration |
| `vehicle_view`       | vehicleId, source       |
| `vehicle_favorite`   | vehicleId               |
| `vehicle_compare`    | vehicleIds[]            |
| `search`             | query, filters, results |
| `lead_submit`        | vehicleId, dealerId     |
| `chat_start`         | vehicleId, channel      |
| `signup`             | source, campaign        |
| `subscription_start` | plan, price             |

---

### 5.2 ANLT-002: Calcular Métricas en Tiempo Real

| Campo       | Valor                         |
| ----------- | ----------------------------- |
| **ID**      | ANLT-002                      |
| **Nombre**  | Real-time Metrics Calculation |
| **Actor**   | Sistema                       |
| **Trigger** | Streaming de eventos          |

#### Flujo del Proceso

| Paso | Acción                | Sistema         | Validación      |
| ---- | --------------------- | --------------- | --------------- |
| 1    | Recibir evento        | Kafka           | Consumer group  |
| 2    | Deserializar          | AnalyticsWorker | JSON            |
| 3    | Clasificar evento     | Router          | Por tipo        |
| 4    | Actualizar contadores | Redis           | INCR            |
| 5    | Actualizar sets       | Redis           | SADD (unique)   |
| 6    | Calcular rolling avg  | Redis           | Sorted sets     |
| 7    | Verificar alertas     | AlertService    | Thresholds      |
| 8    | Publicar update       | WebSocket       | Para dashboards |
| 9    | Persistir cada minuto | ClickHouse      | Time series     |

#### Estructura Redis

```
# Contadores diarios
analytics:views:vehicle:{id}:2026-01-21 = 245
analytics:views:dealer:{id}:2026-01-21 = 1580

# Usuarios únicos (HyperLogLog)
analytics:users:dau:2026-01-21 = HLL(...)
analytics:users:mau:2026-01 = HLL(...)

# Top vehículos (Sorted Set)
analytics:top:vehicles:2026-01-21 = {
  vehicleId1: 1245,
  vehicleId2: 980,
  ...
}

# Métricas en tiempo real (Hash)
analytics:realtime:platform = {
  activeUsers: 342,
  pageViewsLastHour: 2450,
  leadsLastHour: 23
}
```

---

### 5.3 ANLT-003: Generar Reporte Programado

| Campo       | Valor                       |
| ----------- | --------------------------- |
| **ID**      | ANLT-003                    |
| **Nombre**  | Scheduled Report Generation |
| **Actor**   | Sistema                     |
| **Trigger** | Cron job                    |

#### Flujo del Proceso

| Paso | Acción                       | Sistema             | Validación        |
| ---- | ---------------------------- | ------------------- | ----------------- |
| 1    | Trigger cron                 | SchedulerService    | Por configuración |
| 2    | Obtener reportes programados | Database            | IsActive = true   |
| 3    | Para cada reporte            | Loop                | Procesar          |
| 4    | Query data warehouse         | ClickHouse          | SQL complejo      |
| 5    | Agregar datos                | ReportService       | Cálculos          |
| 6    | Generar visualizaciones      | ChartService        | Charts            |
| 7    | Renderizar PDF/Excel         | ReportService       | Template          |
| 8    | Subir a S3                   | MediaService        | Storage           |
| 9    | Enviar por email             | NotificationService | Con adjunto       |
| 10   | Log ejecución                | Database            | ReportExecution   |

#### Tipos de Reportes

| Reporte                   | Frecuencia    | Destinatarios |
| ------------------------- | ------------- | ------------- |
| Daily Platform Summary    | Diario 6am    | Executives    |
| Weekly Dealer Performance | Lunes 8am     | Dealers       |
| Monthly Revenue Report    | Día 1 del mes | Finance       |
| Quarterly Business Review | Trimestral    | Board         |
| Ad-hoc Custom Reports     | On-demand     | Requested by  |

---

## 6. Data Warehouse Schema

### 6.1 ClickHouse Tables

```sql
-- Eventos de usuario (tabla principal)
CREATE TABLE events (
    event_id UUID,
    event_type String,
    user_id Nullable(UUID),
    session_id UUID,
    timestamp DateTime64(3),

    -- Contexto
    page_url String,
    referrer String,
    device_type String,
    browser String,
    country String,
    city String,

    -- Datos del evento
    properties Map(String, String),

    -- Particionado por fecha
    event_date Date DEFAULT toDate(timestamp)
) ENGINE = MergeTree()
PARTITION BY toYYYYMM(event_date)
ORDER BY (event_type, event_date, user_id);

-- Métricas agregadas diarias
CREATE TABLE daily_metrics (
    date Date,
    metric_type String,
    dimension String,
    dimension_value String,
    value Float64,
    count UInt64
) ENGINE = SummingMergeTree()
PARTITION BY toYYYYMM(date)
ORDER BY (metric_type, dimension, date);

-- Sesiones de usuario
CREATE TABLE sessions (
    session_id UUID,
    user_id Nullable(UUID),
    start_time DateTime64(3),
    end_time DateTime64(3),
    duration_seconds UInt32,
    page_count UInt16,
    events_count UInt16,
    device_type String,
    first_referrer String,
    landing_page String,
    exit_page String,
    is_bounce Bool
) ENGINE = MergeTree()
PARTITION BY toYYYYMM(start_time)
ORDER BY (start_time, session_id);
```

---

## 7. API Responses

### 7.1 Platform Overview

```json
{
  "period": "2026-01-21",
  "metrics": {
    "activeUsers": {
      "dau": 2450,
      "mau": 45230,
      "dauChange": 8.5,
      "mauChange": 12.3
    },
    "traffic": {
      "pageViews": 125000,
      "sessions": 42000,
      "avgSessionDuration": 245,
      "bounceRate": 35.2
    },
    "vehicles": {
      "total": 12450,
      "active": 10280,
      "newThisMonth": 890,
      "avgViewsPerListing": 45
    },
    "leads": {
      "total": 3450,
      "qualified": 1230,
      "conversionRate": 35.6
    },
    "revenue": {
      "mrr": 485000,
      "arr": 5820000,
      "arpu": 103.5,
      "churnRate": 2.3
    }
  },
  "trends": {
    "trafficByHour": [...],
    "leadsByDay": [...],
    "revenueByMonth": [...]
  }
}
```

### 7.2 Dealer Dashboard

```json
{
  "dealerId": "uuid",
  "period": "2026-01",
  "overview": {
    "activeListings": 38,
    "maxListings": 50,
    "totalViews": 8450,
    "totalLeads": 156,
    "salesThisMonth": 12,
    "averageTimeToSale": 21
  },
  "topVehicles": [
    {
      "id": "uuid",
      "title": "Toyota RAV4 XLE 2024",
      "views": 1245,
      "leads": 23,
      "favorites": 45,
      "daysListed": 15
    }
  ],
  "leadFunnel": {
    "views": 8450,
    "leads": 156,
    "contacts": 48,
    "appointments": 28,
    "sales": 12
  },
  "priceInsights": [
    {
      "vehicleId": "uuid",
      "currentPrice": 2300000,
      "recommendedPrice": 2150000,
      "marketAverage": 2180000,
      "recommendation": "Reducir 6% para mayor conversión"
    }
  ],
  "competitorComparison": {
    "avgPriceVsMarket": -5.2,
    "avgDaysOnMarket": 18,
    "marketAvgDays": 25
  }
}
```

---

## 8. Eventos RabbitMQ

| Evento                       | Exchange           | Payload                    |
| ---------------------------- | ------------------ | -------------------------- |
| `analytics.event.received`   | `analytics.events` | `{ eventType, data }`      |
| `analytics.metrics.updated`  | `analytics.events` | `{ metricType, value }`    |
| `analytics.report.generated` | `analytics.events` | `{ reportId, type }`       |
| `analytics.alert.triggered`  | `analytics.alerts` | `{ alertType, threshold }` |

---

## 9. Métricas del Servicio

```
# Pipeline
analytics_events_received_total{type="..."}
analytics_events_processed_total
analytics_events_failed_total
analytics_pipeline_latency_ms

# Queries
analytics_queries_total{type="realtime|historical"}
analytics_query_duration_seconds

# Reports
analytics_reports_generated_total{type="..."}
analytics_reports_failed_total

# Storage
analytics_clickhouse_rows_total
analytics_redis_memory_bytes
```

---

## 10. Configuración

```json
{
  "Analytics": {
    "EventBatchSize": 100,
    "EventFlushInterval": "5s",
    "RealtimeWindow": "1h",
    "RetentionDays": {
      "RawEvents": 90,
      "AggregatedDaily": 730,
      "AggregatedMonthly": 1825
    }
  },
  "ClickHouse": {
    "Host": "clickhouse.okla.internal",
    "Database": "analytics",
    "MaxConnections": 10
  },
  "Reports": {
    "DefaultTimezone": "America/Santo_Domingo",
    "PdfQuality": "high",
    "EmailRetryAttempts": 3
  }
}
```

---

## 📚 Referencias

- [01-event-tracking.md](01-event-tracking.md) - Tracking de eventos
- [03-lead-scoring.md](../06-CRM-LEADS-CONTACTOS/03-lead-scoring.md) - Lead scoring
- [02-recommendation-service.md](../04-BUSQUEDA-RECOMENDACIONES/02-recommendation-service.md) - Recomendaciones
