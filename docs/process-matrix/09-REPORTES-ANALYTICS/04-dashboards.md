# 📊 Dashboards Service - Dashboards Ejecutivos - Matriz de Procesos

> **Servicio:** ReportsService / DashboardsModule  
> **Puerto:** 5020  
> **Última actualización:** Enero 21, 2026  
> **Estado:** 🟢 ACTIVO

---

## 📊 Resumen de Implementación

| Componente     | Total | Implementado | Pendiente | Estado |
| -------------- | ----- | ------------ | --------- | ------ |
| Controllers    | 2     | 0            | 2         | 🔴     |
| DASH-EXEC-\*   | 5     | 0            | 5         | 🔴     |
| DASH-OPS-\*    | 4     | 0            | 4         | 🔴     |
| DASH-DEAL-\*   | 4     | 0            | 4         | 🔴     |
| DASH-WIDGET-\* | 6     | 0            | 6         | 🔴     |
| Tests          | 0     | 0            | 15        | 🔴     |

**Leyenda:** ✅ Implementado + Tested | 🟢 Implementado | 🟡 En Progreso | 🔴 Pendiente

---

## 1. Información General

### 1.1 Descripción

Sistema de dashboards ejecutivos para visualización de métricas de negocio en tiempo real. Proporciona paneles personalizables para diferentes roles: CEO, Operaciones, Ventas, Finanzas, Compliance, y Dealers.

### 1.2 Tipos de Dashboards

| Dashboard      | Audiencia   | Refresh | KPIs Principales              |
| -------------- | ----------- | ------- | ----------------------------- |
| **Executive**  | C-Level     | 5 min   | Revenue, GMV, Users, Growth   |
| **Operations** | Ops Team    | 1 min   | Listings, Moderation, Support |
| **Sales**      | Sales Team  | 5 min   | Leads, Conversions, Pipeline  |
| **Finance**    | Finance     | 15 min  | Revenue, Subs, Payments       |
| **Compliance** | Compliance  | 1 hora  | Reports, Alerts, KYC          |
| **Dealer**     | Each Dealer | 5 min   | Their metrics                 |

### 1.3 Arquitectura

```
┌─────────────────────────────────────────────────────────────────────────┐
│                      Dashboards Architecture                             │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│   Frontend Dashboards                                                   │
│   ┌─────────────────────────────────────────────────────────────────┐   │
│   │                                                                   │   │
│   │   ┌──────────────┐  ┌──────────────┐  ┌──────────────┐          │   │
│   │   │  Executive   │  │  Operations  │  │    Sales     │          │   │
│   │   │  Dashboard   │  │  Dashboard   │  │  Dashboard   │          │   │
│   │   └──────────────┘  └──────────────┘  └──────────────┘          │   │
│   │   ┌──────────────┐  ┌──────────────┐  ┌──────────────┐          │   │
│   │   │   Finance    │  │  Compliance  │  │   Dealer     │          │   │
│   │   │  Dashboard   │  │  Dashboard   │  │  Dashboard   │          │   │
│   │   └──────────────┘  └──────────────┘  └──────────────┘          │   │
│   │                                                                   │   │
│   │   Components: Charts (Recharts), KPI Cards, Tables, Maps         │   │
│   └───────────────────────────────┬─────────────────────────────────┘   │
│                                   │                                      │
│                                   │ WebSocket + REST                     │
│                                   ▼                                      │
│   ┌─────────────────────────────────────────────────────────────────┐   │
│   │                     Dashboards API                               │   │
│   │                                                                   │   │
│   │   ┌────────────────────────────────────────────────────────┐    │   │
│   │   │              Data Aggregation Layer                     │    │   │
│   │   │                                                          │    │   │
│   │   │   - Pre-computed aggregates (hourly, daily, monthly)    │    │   │
│   │   │   - Real-time metrics (Redis)                           │    │   │
│   │   │   - Historical trends (PostgreSQL)                      │    │   │
│   │   │   - Comparative analysis (YoY, MoM, WoW)                │    │   │
│   │   └────────────────────────────────────────────────────────┘    │   │
│   │                                                                   │   │
│   └───────────────────────────────┬─────────────────────────────────┘   │
│                                   │                                      │
│              ┌────────────────────┼────────────────────┐                │
│              │                    │                    │                 │
│              ▼                    ▼                    ▼                 │
│   ┌──────────────────┐ ┌──────────────────┐ ┌──────────────────┐        │
│   │     Redis        │ │   PostgreSQL     │ │  Other Services  │        │
│   │  (Real-time)     │ │  (Historical)    │ │  (Source data)   │        │
│   └──────────────────┘ └──────────────────┘ └──────────────────┘        │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 2. Endpoints

### 2.1 Dashboard Data

| Método | Endpoint                            | Descripción           | Auth              |
| ------ | ----------------------------------- | --------------------- | ----------------- |
| `GET`  | `/api/dashboards/executive`         | Dashboard ejecutivo   | C-Level           |
| `GET`  | `/api/dashboards/operations`        | Dashboard operaciones | OpsTeam           |
| `GET`  | `/api/dashboards/sales`             | Dashboard ventas      | SalesTeam         |
| `GET`  | `/api/dashboards/finance`           | Dashboard finanzas    | FinanceTeam       |
| `GET`  | `/api/dashboards/compliance`        | Dashboard compliance  | ComplianceOfficer |
| `GET`  | `/api/dashboards/dealer/{dealerId}` | Dashboard dealer      | Dealer            |

### 2.2 Widgets y Gráficos

| Método | Endpoint                              | Descripción                | Auth |
| ------ | ------------------------------------- | -------------------------- | ---- |
| `GET`  | `/api/dashboards/widgets/{type}`      | Datos de widget específico | Auth |
| `GET`  | `/api/dashboards/charts/timeseries`   | Serie de tiempo            | Auth |
| `GET`  | `/api/dashboards/charts/distribution` | Distribución               | Auth |
| `GET`  | `/api/dashboards/charts/geographic`   | Datos geográficos          | Auth |

### 2.3 Personalización

| Método | Endpoint                         | Descripción               | Auth  |
| ------ | -------------------------------- | ------------------------- | ----- |
| `GET`  | `/api/dashboards/config`         | Configuración del usuario | User  |
| `PUT`  | `/api/dashboards/config`         | Guardar configuración     | User  |
| `POST` | `/api/dashboards/widgets/custom` | Crear widget custom       | Admin |

### 2.4 Export

| Método | Endpoint                          | Descripción      | Auth |
| ------ | --------------------------------- | ---------------- | ---- |
| `POST` | `/api/dashboards/export/pdf`      | Exportar a PDF   | Auth |
| `POST` | `/api/dashboards/export/excel`    | Exportar a Excel | Auth |
| `POST` | `/api/dashboards/schedule-report` | Programar envío  | Auth |

---

## 3. Entidades

### 3.1 DashboardConfig

```csharp
public class DashboardConfig
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DashboardType Type { get; set; }

    // Layout
    public List<WidgetConfig> Widgets { get; set; } = new();
    public string Layout { get; set; } = "default"; // grid, freeform

    // Preferences
    public string Theme { get; set; } = "light";
    public string DateRange { get; set; } = "last30days";
    public bool AutoRefresh { get; set; } = true;
    public int RefreshIntervalSeconds { get; set; } = 300;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class WidgetConfig
{
    public string WidgetId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Row { get; set; }
    public int Col { get; set; }
    public int Width { get; set; } = 1;
    public int Height { get; set; } = 1;
    public Dictionary<string, object> Settings { get; set; } = new();
}

public enum DashboardType
{
    Executive,
    Operations,
    Sales,
    Finance,
    Compliance,
    Dealer,
    Custom
}
```

### 3.2 DashboardMetrics

```csharp
public class ExecutiveDashboardMetrics
{
    public DateTime Timestamp { get; set; }
    public DateRange Period { get; set; }

    // Revenue
    public RevenueMetrics Revenue { get; set; } = new();

    // Users
    public UserMetrics Users { get; set; } = new();

    // Marketplace
    public MarketplaceMetrics Marketplace { get; set; } = new();

    // Growth
    public GrowthMetrics Growth { get; set; } = new();
}

public class RevenueMetrics
{
    public decimal TotalRevenue { get; set; }
    public decimal MRR { get; set; } // Monthly Recurring Revenue
    public decimal ARR { get; set; } // Annual Recurring Revenue
    public decimal GMV { get; set; } // Gross Merchandise Value
    public decimal AvgRevenuePerDealer { get; set; }
    public decimal RevenueChangePercent { get; set; } // vs previous period

    public List<TimeSeriesPoint> RevenueTrend { get; set; } = new();
    public Dictionary<string, decimal> RevenueBySource { get; set; } = new(); // Subs, Listings, Ads
}

public class UserMetrics
{
    public int TotalUsers { get; set; }
    public int ActiveUsersDAU { get; set; }
    public int ActiveUsersMAU { get; set; }
    public int NewUsers { get; set; }
    public double ChurnRate { get; set; }
    public double RetentionRate { get; set; }

    public List<TimeSeriesPoint> UserGrowthTrend { get; set; } = new();
    public Dictionary<string, int> UsersByProvince { get; set; } = new();
}

public class MarketplaceMetrics
{
    public int TotalListings { get; set; }
    public int ActiveListings { get; set; }
    public int NewListings { get; set; }
    public int SoldVehicles { get; set; }
    public decimal TotalInventoryValue { get; set; }
    public double AvgDaysOnMarket { get; set; }
    public double ConversionRate { get; set; }

    public List<TimeSeriesPoint> ListingsTrend { get; set; } = new();
    public Dictionary<string, int> ListingsByCategory { get; set; } = new();
}

public class GrowthMetrics
{
    public double UserGrowthRate { get; set; }
    public double RevenueGrowthRate { get; set; }
    public double DealerGrowthRate { get; set; }
    public double ListingsGrowthRate { get; set; }

    // Comparisons
    public double YoYGrowth { get; set; }
    public double MoMGrowth { get; set; }
    public double WoWGrowth { get; set; }
}

public class TimeSeriesPoint
{
    public DateTime Date { get; set; }
    public decimal Value { get; set; }
}
```

---

## 4. Widgets Disponibles

### 4.1 KPI Cards

| Widget ID             | Descripción        | Datos              |
| --------------------- | ------------------ | ------------------ |
| `kpi-revenue-total`   | Ingreso total      | Revenue + change % |
| `kpi-mrr`             | MRR actual         | MRR + change %     |
| `kpi-gmv`             | GMV total          | GMV + change %     |
| `kpi-active-users`    | Usuarios activos   | DAU/MAU            |
| `kpi-active-listings` | Listings activos   | Count + change     |
| `kpi-conversion-rate` | Tasa de conversión | % + trend          |
| `kpi-avg-dom`         | Días en mercado    | Avg days           |
| `kpi-open-tickets`    | Tickets abiertos   | Count              |

### 4.2 Charts

| Widget ID               | Tipo             | Descripción                |
| ----------------------- | ---------------- | -------------------------- |
| `chart-revenue-trend`   | Line             | Tendencia de ingresos      |
| `chart-users-growth`    | Area             | Crecimiento de usuarios    |
| `chart-listings-funnel` | Funnel           | Funnel de publicaciones    |
| `chart-revenue-sources` | Pie              | Distribución de ingresos   |
| `chart-geographic-dist` | Map              | Distribución geográfica    |
| `chart-category-dist`   | Bar              | Distribución por categoría |
| `chart-dealer-ranking`  | Bar (horizontal) | Top dealers                |
| `chart-lead-sources`    | Donut            | Fuentes de leads           |

### 4.3 Tables

| Widget ID                  | Descripción             |
| -------------------------- | ----------------------- |
| `table-top-vehicles`       | Vehículos más vistos    |
| `table-recent-sales`       | Ventas recientes        |
| `table-pending-approvals`  | Aprobaciones pendientes |
| `table-support-tickets`    | Tickets de soporte      |
| `table-dealer-performance` | Performance de dealers  |

---

## 5. Procesos Detallados

### 5.1 DASH-001: Cargar Dashboard Ejecutivo

| Paso | Acción                     | Sistema       | Validación        |
| ---- | -------------------------- | ------------- | ----------------- |
| 1    | Usuario abre dashboard     | Frontend      | C-Level auth      |
| 2    | Request GET /executive     | Frontend      | Auth header       |
| 3    | Verificar permisos         | DashboardsAPI | Role check        |
| 4    | Obtener config del usuario | DashboardsAPI | User config       |
| 5    | Para cada widget:          | DashboardsAPI | Parallel          |
| 6    | - Check Redis cache        | Redis         | Cache hit?        |
| 7    | - Si miss: calcular        | DashboardsAPI | Aggregation       |
| 8    | - Guardar en cache         | Redis         | TTL by widget     |
| 9    | Ensamblar response         | DashboardsAPI | All widgets       |
| 10   | Iniciar WebSocket          | Frontend      | Real-time updates |

```csharp
public class ExecutiveDashboardQueryHandler : IRequestHandler<ExecutiveDashboardQuery, ExecutiveDashboardMetrics>
{
    public async Task<ExecutiveDashboardMetrics> Handle(
        ExecutiveDashboardQuery request,
        CancellationToken ct)
    {
        var period = ParseDateRange(request.DateRange);
        var cacheKey = $"dashboard:executive:{period.Start:yyyyMMdd}:{period.End:yyyyMMdd}";

        // Try cache first
        var cached = await _cache.GetStringAsync(cacheKey, ct);
        if (!string.IsNullOrEmpty(cached))
        {
            return JsonSerializer.Deserialize<ExecutiveDashboardMetrics>(cached)!;
        }

        // Calculate metrics in parallel
        var revenueTask = CalculateRevenueMetricsAsync(period, ct);
        var usersTask = CalculateUserMetricsAsync(period, ct);
        var marketplaceTask = CalculateMarketplaceMetricsAsync(period, ct);
        var growthTask = CalculateGrowthMetricsAsync(period, ct);

        await Task.WhenAll(revenueTask, usersTask, marketplaceTask, growthTask);

        var metrics = new ExecutiveDashboardMetrics
        {
            Timestamp = DateTime.UtcNow,
            Period = period,
            Revenue = await revenueTask,
            Users = await usersTask,
            Marketplace = await marketplaceTask,
            Growth = await growthTask
        };

        // Cache for 5 minutes
        await _cache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(metrics),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            },
            ct);

        return metrics;
    }

    private async Task<RevenueMetrics> CalculateRevenueMetricsAsync(DateRange period, CancellationToken ct)
    {
        // Get from BillingService or pre-aggregated table
        var currentRevenue = await _billingRepo.GetTotalRevenueAsync(period, ct);

        // Previous period for comparison
        var previousPeriod = GetPreviousPeriod(period);
        var previousRevenue = await _billingRepo.GetTotalRevenueAsync(previousPeriod, ct);

        var mrr = await _billingRepo.GetMRRAsync(ct);
        var gmv = await _vehiclesRepo.GetGMVAsync(period, ct);

        return new RevenueMetrics
        {
            TotalRevenue = currentRevenue,
            MRR = mrr,
            ARR = mrr * 12,
            GMV = gmv,
            RevenueChangePercent = CalculateChangePercent(previousRevenue, currentRevenue),
            RevenueTrend = await _billingRepo.GetRevenueTrendAsync(period, ct),
            RevenueBySource = await _billingRepo.GetRevenueBySourceAsync(period, ct)
        };
    }
}
```

### 5.2 DASH-002: Real-time Updates via WebSocket

```
┌─────────────────────────────────────────────────────────────────────────┐
│                     WebSocket Real-time Updates                          │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│   Client                           Server                               │
│     │                                │                                   │
│     │ 1. Connect WebSocket           │                                   │
│     │──────────────────────────────▶│                                   │
│     │                                │                                   │
│     │ 2. Subscribe: executive        │                                   │
│     │──────────────────────────────▶│                                   │
│     │                                │                                   │
│     │                                │ 3. Add to group: dashboard_exec  │
│     │                                │                                   │
│     │                                │                                   │
│     │◀──────────────────────────────│ 4. Push: kpi-revenue-total       │
│     │    {type: "kpi", id: "...",   │    (on transaction)               │
│     │     value: 1234567}           │                                   │
│     │                                │                                   │
│     │◀──────────────────────────────│ 5. Push: kpi-active-listings     │
│     │                                │    (on new listing)              │
│     │                                │                                   │
│     │ 6. Unsubscribe / Disconnect   │                                   │
│     │──────────────────────────────▶│                                   │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

```csharp
public class DashboardHub : Hub
{
    public async Task SubscribeToDashboard(string dashboardType)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"dashboard_{dashboardType}");
    }

    public async Task UnsubscribeFromDashboard(string dashboardType)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"dashboard_{dashboardType}");
    }
}

// Event handler that pushes updates
public class PaymentCompletedHandler : IConsumer<PaymentCompletedEvent>
{
    private readonly IHubContext<DashboardHub> _hubContext;

    public async Task Consume(ConsumeContext<PaymentCompletedEvent> context)
    {
        // Calculate new total
        var newTotal = await _billingRepo.GetTodayRevenueAsync();

        // Push to executive dashboard subscribers
        await _hubContext.Clients.Group("dashboard_executive")
            .SendAsync("widgetUpdate", new
            {
                widgetId = "kpi-revenue-total",
                value = newTotal,
                timestamp = DateTime.UtcNow
            });
    }
}
```

---

## 6. Dashboards por Rol

### 6.1 Executive Dashboard

```
┌─────────────────────────────────────────────────────────────────────────┐
│  📊 Executive Dashboard                          📅 Last 30 Days  ⟳    │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐ │
│  │   Revenue    │  │     MRR      │  │     GMV      │  │    Users     │ │
│  │  $245,670    │  │   $52,300    │  │  $3.2M       │  │   15,234     │ │
│  │   ↑ 12.3%    │  │    ↑ 8.5%   │  │   ↑ 15.2%   │  │   ↑ 22.1%   │ │
│  └──────────────┘  └──────────────┘  └──────────────┘  └──────────────┘ │
│                                                                          │
│  ┌────────────────────────────────────┐  ┌───────────────────────────┐  │
│  │     Revenue Trend (30 days)        │  │   Revenue by Source       │  │
│  │  $                                 │  │                           │  │
│  │  │    ╱╲    ╱╲                    │  │   ● Subscriptions  65%    │  │
│  │  │   ╱  ╲  ╱  ╲  ╱╲              │  │   ● Listings       25%    │  │
│  │  │  ╱    ╲╱    ╲╱  ╲             │  │   ● Ads            10%    │  │
│  │  │ ╱                 ╲            │  │                           │  │
│  │  └─────────────────────────────   │  └───────────────────────────┘  │
│  │    Jan 1        Jan 15      Jan 30│                                  │
│  └────────────────────────────────────┘                                  │
│                                                                          │
│  ┌────────────────────────────────────┐  ┌───────────────────────────┐  │
│  │     User Growth                    │  │   Top Dealers             │  │
│  │  Users                             │  │                           │  │
│  │  │    ▄▄▄▄▄▄▄                     │  │   1. Auto San Juan  $45K  │  │
│  │  │   ▄████████▄                   │  │   2. CarMax RD      $38K  │  │
│  │  │  ▄██████████▄▄                 │  │   3. SuperAutos     $32K  │  │
│  │  │ ▄██████████████                │  │   4. TuCarro Pro    $28K  │  │
│  │  └─────────────────────────────   │  │   5. MegaDealer     $25K  │  │
│  └────────────────────────────────────┘  └───────────────────────────┘  │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

### 6.2 Operations Dashboard

**KPIs:** Pending Moderation, Support Queue, Response Time, System Health
**Charts:** Moderation queue trend, Ticket volume, Service availability
**Tables:** Pending approvals, Escalated tickets, Error rates

### 6.3 Dealer Dashboard

**KPIs:** Active Listings, Views, Contacts, Conversion Rate
**Charts:** Views trend, Lead sources, Inventory aging
**Tables:** Top vehicles, Recent leads, Competitor comparison

---

## 7. Reglas de Negocio

| Código   | Regla                           | Validación           |
| -------- | ------------------------------- | -------------------- |
| DASH-R01 | Cache TTL por tipo de widget    | 5min KPIs, 1h trends |
| DASH-R02 | Dealers solo ven sus datos      | DealerId filter      |
| DASH-R03 | Datos financieros solo Finance+ | Role check           |
| DASH-R04 | Export limitado a 1 año         | DateRange validation |
| DASH-R05 | Real-time solo para últimos 24h | Filter applied       |

---

## 8. Eventos RabbitMQ

| Evento                  | Trigger       | Dashboard          |
| ----------------------- | ------------- | ------------------ |
| `PaymentCompletedEvent` | Revenue KPIs  | Executive, Finance |
| `UserRegisteredEvent`   | User KPIs     | Executive          |
| `VehiclePublishedEvent` | Listings KPIs | Operations         |
| `TicketCreatedEvent`    | Support KPIs  | Operations         |
| `LeadCreatedEvent`      | Lead KPIs     | Sales, Dealer      |

---

## 9. Configuración

```json
{
  "Dashboards": {
    "CacheTTL": {
      "KPIs": 300,
      "Trends": 3600,
      "Tables": 60
    },
    "RealTime": {
      "Enabled": true,
      "MaxConnectionsPerUser": 3
    },
    "Export": {
      "MaxRowsExcel": 10000,
      "MaxPagesP

DF": 50
    }
  }
}
```

---

## 10. Métricas Prometheus

```
# Dashboard requests
dashboard_requests_total{type="...", user_role="..."}
dashboard_load_time_ms{type="..."}

# Cache
dashboard_cache_hit_rate{widget="..."}

# WebSocket
dashboard_websocket_connections{type="..."}
dashboard_realtime_updates_total
```

---

## 📚 Referencias

- [01-reports-service.md](01-reports-service.md) - Reportes generales
- [03-dealer-analytics.md](../02-USUARIOS-DEALERS/03-dealer-analytics.md) - Analytics de dealers
- [01-admin-service.md](../12-ADMINISTRACION/01-admin-service.md) - Panel de administración
