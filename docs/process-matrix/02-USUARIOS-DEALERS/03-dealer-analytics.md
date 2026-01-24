# 📊 Dealer Analytics Service - Analytics para Dealers - Matriz de Procesos

> **Servicio:** DealerAnalyticsService  
> **Puerto:** 5041  
> **Última actualización:** Enero 23, 2026  
> **Estado:** 🟢 ACTIVO  
> **Estado de Implementación:** ✅ Completado

---

## 📊 Resumen de Implementación

| Componente               | Total | Implementado | Pendiente | Estado  |
| ------------------------ | ----- | ------------ | --------- | ------- |
| **Controllers**          | 9     | 9            | 0         | ✅ 100% |
| **Procesos (ANAL-\*)**   | 6     | 6            | 0         | ✅ 100% |
| **Procesos (DASH-\*)**   | 4     | 4            | 0         | ✅ 100% |
| **Procesos (REPORT-\*)** | 3     | 3            | 0         | ✅ 100% |
| **Tests Unitarios**      | 66    | 66           | 0         | ✅ 100% |

### Controllers Implementados

1. `AlertsController` - Gestión de alertas de dealer
2. `AnalyticsController` - Métricas generales de analytics
3. `BenchmarkController` - Comparativas con el mercado
4. `ConversionFunnelController` - Funnel de conversión de leads
5. `DashboardController` - Dashboard principal
6. `InsightsController` - Insights y recomendaciones
7. `InventoryAnalyticsController` - Analytics de inventario
8. `OverviewController` - Vista general
9. `ReportsController` - Generación de reportes

### Leyenda de Estados

- ✅ **IMPLEMENTADO Y PROBADO**: Código completo con tests
- 🟢 **IMPLEMENTADO**: Código completo, falta testing
- 🟡 **EN PROGRESO**: Implementación parcial
- 🔴 **PENDIENTE**: No implementado

---

## 1. Información General

### 1.1 Descripción

Sistema de analytics en tiempo real para dealers que proporciona métricas de rendimiento, estadísticas de inventario, análisis de leads, conversión de ventas, y comparativas con el mercado. Permite a los dealers tomar decisiones basadas en datos.

### 1.2 Dependencias

| Servicio             | Propósito          |
| -------------------- | ------------------ |
| VehiclesSaleService  | Datos de vehículos |
| LeadScoringService   | Datos de leads     |
| BillingService       | Transacciones      |
| EventTrackingService | Eventos de usuario |
| UserBehaviorService  | Comportamiento     |

### 1.3 Arquitectura

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    Dealer Analytics Architecture                         │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│   Data Sources                     Processing              Dashboard     │
│   ┌────────────────┐              ┌──────────────┐        ┌──────────┐  │
│   │ VehiclesSale   │──┐           │              │        │          │  │
│   │ Service        │  │           │  Real-time   │        │ Dealer   │  │
│   └────────────────┘  │           │  Pipeline    │        │ Dashboard│  │
│   ┌────────────────┐  │           │              │        │          │  │
│   │ EventTracking  │──┼───────────│  - Views     │───────▶│ - KPIs   │  │
│   │ Service        │  │           │  - Contacts  │        │ - Charts │  │
│   └────────────────┘  │           │  - Favorites │        │ - Alerts │  │
│   ┌────────────────┐  │           │  - Searches  │        │          │  │
│   │ LeadScoring    │──┤           │              │        │          │  │
│   │ Service        │  │           └──────────────┘        └──────────┘  │
│   └────────────────┘  │                  │                      │       │
│   ┌────────────────┐  │                  ▼                      │       │
│   │ Billing        │──┘           ┌──────────────┐              │       │
│   │ Service        │              │ Aggregation  │              │       │
│   └────────────────┘              │ Service      │              │       │
│                                   │              │              │       │
│                                   │ - Daily      │              │       │
│                                   │ - Weekly     │              │       │
│                                   │ - Monthly    │              │       │
│                                   └──────┬───────┘              │       │
│                                          │                      │       │
│                                          ▼                      ▼       │
│                                   ┌──────────────────────────────────┐  │
│                                   │        PostgreSQL + Redis        │  │
│                                   │   (Time-series + Aggregates)     │  │
│                                   └──────────────────────────────────┘  │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 2. Endpoints

### 2.1 Dashboard Principal

| Método | Endpoint                         | Descripción      | Auth   |
| ------ | -------------------------------- | ---------------- | ------ |
| `GET`  | `/api/dealer-analytics/overview` | Resumen general  | Dealer |
| `GET`  | `/api/dealer-analytics/kpis`     | KPIs principales | Dealer |
| `GET`  | `/api/dealer-analytics/snapshot` | Snapshot actual  | Dealer |

### 2.2 Métricas de Inventario

| Método | Endpoint                                      | Descripción              | Auth   |
| ------ | --------------------------------------------- | ------------------------ | ------ |
| `GET`  | `/api/dealer-analytics/inventory/stats`       | Stats de inventario      | Dealer |
| `GET`  | `/api/dealer-analytics/inventory/aging`       | Antigüedad de stock      | Dealer |
| `GET`  | `/api/dealer-analytics/inventory/turnover`    | Rotación                 | Dealer |
| `GET`  | `/api/dealer-analytics/inventory/performance` | Rendimiento por vehículo | Dealer |

### 2.3 Métricas de Leads/Conversión

| Método | Endpoint                                | Descripción          | Auth   |
| ------ | --------------------------------------- | -------------------- | ------ |
| `GET`  | `/api/dealer-analytics/leads/stats`     | Stats de leads       | Dealer |
| `GET`  | `/api/dealer-analytics/leads/funnel`    | Funnel de conversión | Dealer |
| `GET`  | `/api/dealer-analytics/leads/sources`   | Fuentes de leads     | Dealer |
| `GET`  | `/api/dealer-analytics/conversion-rate` | Tasa de conversión   | Dealer |

### 2.4 Métricas de Engagement

| Método | Endpoint                          | Descripción              | Auth   |
| ------ | --------------------------------- | ------------------------ | ------ |
| `GET`  | `/api/dealer-analytics/views`     | Vistas de vehículos      | Dealer |
| `GET`  | `/api/dealer-analytics/contacts`  | Contactos recibidos      | Dealer |
| `GET`  | `/api/dealer-analytics/favorites` | Favoritos                | Dealer |
| `GET`  | `/api/dealer-analytics/searches`  | Apariciones en búsquedas | Dealer |

### 2.5 Comparativas y Benchmarks

| Método | Endpoint                          | Descripción             | Auth   |
| ------ | --------------------------------- | ----------------------- | ------ |
| `GET`  | `/api/dealer-analytics/benchmark` | Comparación con mercado | Dealer |
| `GET`  | `/api/dealer-analytics/ranking`   | Ranking entre dealers   | Dealer |
| `GET`  | `/api/dealer-analytics/trends`    | Tendencias del mercado  | Dealer |

### 2.6 Reportes

| Método | Endpoint                                        | Descripción           | Auth   |
| ------ | ----------------------------------------------- | --------------------- | ------ |
| `GET`  | `/api/dealer-analytics/reports/daily`           | Reporte diario        | Dealer |
| `GET`  | `/api/dealer-analytics/reports/weekly`          | Reporte semanal       | Dealer |
| `GET`  | `/api/dealer-analytics/reports/monthly`         | Reporte mensual       | Dealer |
| `POST` | `/api/dealer-analytics/reports/custom`          | Reporte personalizado | Dealer |
| `GET`  | `/api/dealer-analytics/reports/export/{format}` | Exportar PDF/Excel    | Dealer |

---

## 3. Entidades

### 3.1 DealerSnapshot

```csharp
public class DealerSnapshot
{
    public Guid Id { get; set; }
    public Guid DealerId { get; set; }
    public DateTime SnapshotDate { get; set; }

    // Inventory Metrics
    public int TotalVehicles { get; set; }
    public int ActiveVehicles { get; set; }
    public int SoldVehicles { get; set; }
    public decimal TotalInventoryValue { get; set; }
    public decimal AvgVehiclePrice { get; set; }
    public double AvgDaysOnMarket { get; set; }

    // Engagement Metrics
    public int TotalViews { get; set; }
    public int UniqueViews { get; set; }
    public int TotalContacts { get; set; }
    public int TotalFavorites { get; set; }
    public int SearchImpressions { get; set; }

    // Lead Metrics
    public int NewLeads { get; set; }
    public int QualifiedLeads { get; set; }
    public int ConvertedLeads { get; set; }
    public double LeadConversionRate { get; set; }

    // Revenue Metrics
    public decimal TotalRevenue { get; set; }
    public decimal AvgTransactionValue { get; set; }

    public DateTime CreatedAt { get; set; }
}
```

### 3.2 VehiclePerformance

```csharp
public class VehiclePerformance
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public Guid DealerId { get; set; }
    public DateTime Date { get; set; }

    // Daily Metrics
    public int Views { get; set; }
    public int UniqueViews { get; set; }
    public int Contacts { get; set; }
    public int Favorites { get; set; }
    public int SearchImpressions { get; set; }
    public int SearchClicks { get; set; }

    // Calculated
    public double ClickThroughRate => SearchImpressions > 0
        ? (double)SearchClicks / SearchImpressions * 100
        : 0;

    public double ContactRate => Views > 0
        ? (double)Contacts / Views * 100
        : 0;
}
```

### 3.3 LeadFunnelMetrics

```csharp
public class LeadFunnelMetrics
{
    public Guid DealerId { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }

    // Funnel Stages
    public int Impressions { get; set; }  // Vistas búsqueda
    public int Views { get; set; }         // Vistas detalle
    public int Contacts { get; set; }      // Contactos iniciados
    public int Qualified { get; set; }     // Leads calificados
    public int Negotiation { get; set; }   // En negociación
    public int Converted { get; set; }     // Ventas cerradas

    // Conversion Rates
    public double ImpressionsToViews =>
        Impressions > 0 ? (double)Views / Impressions * 100 : 0;
    public double ViewsToContacts =>
        Views > 0 ? (double)Contacts / Views * 100 : 0;
    public double ContactsToQualified =>
        Contacts > 0 ? (double)Qualified / Contacts * 100 : 0;
    public double QualifiedToConverted =>
        Qualified > 0 ? (double)Converted / Qualified * 100 : 0;
    public double OverallConversion =>
        Impressions > 0 ? (double)Converted / Impressions * 100 : 0;
}
```

### 3.4 DealerBenchmark

```csharp
public class DealerBenchmark
{
    public Guid DealerId { get; set; }
    public DateTime Date { get; set; }

    // Dealer's Metrics
    public double AvgDaysOnMarket { get; set; }
    public double ConversionRate { get; set; }
    public double AvgResponseTime { get; set; } // Minutes
    public double CustomerSatisfaction { get; set; } // 0-5

    // Market Averages
    public double MarketAvgDaysOnMarket { get; set; }
    public double MarketAvgConversionRate { get; set; }
    public double MarketAvgResponseTime { get; set; }
    public double MarketAvgSatisfaction { get; set; }

    // Percentile Ranking
    public int DaysOnMarketPercentile { get; set; }
    public int ConversionRatePercentile { get; set; }
    public int ResponseTimePercentile { get; set; }
    public int SatisfactionPercentile { get; set; }

    // Overall Rank
    public int OverallRank { get; set; }
    public int TotalDealers { get; set; }
}
```

---

## 4. Procesos Detallados

### 4.1 ANALYTICS-001: Obtener Dashboard Overview

| Paso | Acción                               | Sistema         | Validación         |
| ---- | ------------------------------------ | --------------- | ------------------ |
| 1    | Dealer accede al dashboard           | Frontend        | Dealer autenticado |
| 2    | Request GET /overview                | Frontend        | X-Dealer-Id header |
| 3    | Validar dealer activo                | DealerAnalytics | Suscripción activa |
| 4    | Obtener snapshot del día             | DealerAnalytics | Cache o DB         |
| 5    | Calcular cambios vs período anterior | DealerAnalytics | % change           |
| 6    | Obtener KPIs principales             | DealerAnalytics | Aggregated data    |
| 7    | Obtener alertas activas              | DealerAnalytics | Alerts check       |
| 8    | Retornar overview                    | API             | Response 200       |

```csharp
public class DealerOverviewQuery : IRequest<DealerOverviewDto>
{
    public Guid DealerId { get; init; }
    public DateRange DateRange { get; init; } = DateRange.Last30Days;
}

public class DealerOverviewQueryHandler : IRequestHandler<DealerOverviewQuery, DealerOverviewDto>
{
    public async Task<DealerOverviewDto> Handle(
        DealerOverviewQuery request,
        CancellationToken ct)
    {
        // 1. Get current snapshot
        var currentSnapshot = await _snapshotRepository
            .GetLatestAsync(request.DealerId, ct);

        // 2. Get comparison period snapshot
        var compareDate = request.DateRange.Start.AddDays(
            -(request.DateRange.End - request.DateRange.Start).Days);

        var previousSnapshot = await _snapshotRepository
            .GetByDateAsync(request.DealerId, compareDate, ct);

        // 3. Calculate changes
        var kpis = new DealerKpisDto
        {
            TotalViews = currentSnapshot.TotalViews,
            ViewsChange = CalculateChange(
                previousSnapshot?.TotalViews ?? 0,
                currentSnapshot.TotalViews),

            TotalContacts = currentSnapshot.TotalContacts,
            ContactsChange = CalculateChange(
                previousSnapshot?.TotalContacts ?? 0,
                currentSnapshot.TotalContacts),

            ConversionRate = currentSnapshot.LeadConversionRate,
            ConversionChange = currentSnapshot.LeadConversionRate -
                (previousSnapshot?.LeadConversionRate ?? 0),

            AvgDaysOnMarket = currentSnapshot.AvgDaysOnMarket,
            DaysOnMarketChange = currentSnapshot.AvgDaysOnMarket -
                (previousSnapshot?.AvgDaysOnMarket ?? 0),

            ActiveListings = currentSnapshot.ActiveVehicles,
            TotalRevenue = currentSnapshot.TotalRevenue
        };

        // 4. Get top performers
        var topPerformers = await _vehiclePerformanceRepository
            .GetTopPerformersAsync(request.DealerId, 5, ct);

        // 5. Get alerts
        var alerts = await GetActiveAlertsAsync(request.DealerId, ct);

        return new DealerOverviewDto
        {
            Kpis = kpis,
            TopPerformers = topPerformers,
            Alerts = alerts,
            LastUpdated = currentSnapshot.CreatedAt
        };
    }

    private double CalculateChange(decimal previous, decimal current)
    {
        if (previous == 0) return current > 0 ? 100 : 0;
        return (double)((current - previous) / previous * 100);
    }
}
```

### 4.2 ANALYTICS-002: Análisis de Funnel de Conversión

| Paso | Acción                          | Sistema         | Validación       |
| ---- | ------------------------------- | --------------- | ---------------- |
| 1    | Request GET /leads/funnel       | Frontend        | Dealer auth      |
| 2    | Definir período de análisis     | DealerAnalytics | DateRange válido |
| 3    | Agregar impresiones de búsqueda | DealerAnalytics | Search events    |
| 4    | Agregar vistas de detalle       | DealerAnalytics | View events      |
| 5    | Agregar contactos               | DealerAnalytics | Contact events   |
| 6    | Agregar leads calificados       | DealerAnalytics | Lead status      |
| 7    | Agregar conversiones            | DealerAnalytics | Sales            |
| 8    | Calcular tasas de conversión    | DealerAnalytics | Percentages      |
| 9    | Comparar con benchmark          | DealerAnalytics | Market data      |
| 10   | Retornar funnel                 | API             | Response 200     |

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         Conversion Funnel                                │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│   ┌─────────────────────────────────────────────────────────────────┐   │
│   │ IMPRESIONES (Búsquedas)                        15,420 (100%)    │   │
│   └──────────────────────────────────────────────────────┬──────────┘   │
│                                                          │ 18.2%       │
│   ┌──────────────────────────────────────────────────────▼──────────┐   │
│   │ VISTAS (Detalle)                                2,810 (18.2%)   │   │
│   └──────────────────────────────────────────────────────┬──────────┘   │
│                                                          │ 8.5%        │
│   ┌──────────────────────────────────────────────────────▼──────────┐   │
│   │ CONTACTOS                                         239 (8.5%)    │   │
│   └──────────────────────────────────────────────────────┬──────────┘   │
│                                                          │ 42.3%       │
│   ┌──────────────────────────────────────────────────────▼──────────┐   │
│   │ LEADS CALIFICADOS                                 101 (42.3%)   │   │
│   └──────────────────────────────────────────────────────┬──────────┘   │
│                                                          │ 28.7%       │
│   ┌──────────────────────────────────────────────────────▼──────────┐   │
│   │ VENTAS CERRADAS                                    29 (28.7%)   │   │
│   └─────────────────────────────────────────────────────────────────┘   │
│                                                                          │
│   Tasa de Conversión General: 0.19%                                     │
│   Benchmark del Mercado: 0.15%                                          │
│   Posición: Top 25% de dealers                                          │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

### 4.3 ANALYTICS-003: Generar Snapshot Diario (Job)

| Paso | Acción                            | Sistema             | Validación                 |
| ---- | --------------------------------- | ------------------- | -------------------------- |
| 1    | Job scheduler dispara a las 00:00 | Scheduler           | Cron daily                 |
| 2    | Obtener lista de dealers activos  | DealerAnalytics     | Active dealers             |
| 3    | Para cada dealer:                 | DealerAnalytics     | Loop                       |
| 4    | - Agregar métricas de inventario  | VehiclesSaleService | Query                      |
| 5    | - Agregar métricas de engagement  | EventTracking       | Query                      |
| 6    | - Agregar métricas de leads       | LeadScoringService  | Query                      |
| 7    | - Agregar métricas de revenue     | BillingService      | Query                      |
| 8    | - Guardar snapshot                | DealerAnalytics     | Insert                     |
| 9    | Calcular benchmarks de mercado    | DealerAnalytics     | Aggregation                |
| 10   | Actualizar rankings               | DealerAnalytics     | Update ranks               |
| 11   | Publicar evento                   | RabbitMQ            | DealerSnapshotCreatedEvent |

```csharp
public class DailySnapshotJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var yesterday = DateTime.UtcNow.Date.AddDays(-1);
        var dealers = await _dealerRepository.GetActiveDealersAsync();

        var snapshots = new List<DealerSnapshot>();

        foreach (var dealer in dealers)
        {
            var snapshot = await CreateSnapshotAsync(dealer.Id, yesterday);
            snapshots.Add(snapshot);
        }

        // Bulk insert snapshots
        await _snapshotRepository.BulkInsertAsync(snapshots);

        // Calculate market benchmarks
        await CalculateMarketBenchmarksAsync(snapshots, yesterday);

        // Update rankings
        await UpdateDealerRankingsAsync(snapshots, yesterday);

        // Publish event
        await _eventBus.PublishAsync(new DailySnapshotsCreatedEvent
        {
            Date = yesterday,
            DealerCount = snapshots.Count
        });

        _logger.LogInformation(
            "Created {Count} daily snapshots for {Date}",
            snapshots.Count, yesterday);
    }

    private async Task<DealerSnapshot> CreateSnapshotAsync(Guid dealerId, DateTime date)
    {
        // Get inventory metrics
        var inventoryStats = await _vehicleService.GetInventoryStatsAsync(dealerId, date);

        // Get engagement metrics
        var engagementStats = await _eventTrackingService.GetDealerEngagementAsync(
            dealerId, date, date.AddDays(1));

        // Get lead metrics
        var leadStats = await _leadService.GetDealerLeadStatsAsync(dealerId, date);

        // Get revenue metrics
        var revenueStats = await _billingService.GetDealerRevenueAsync(dealerId, date);

        return new DealerSnapshot
        {
            DealerId = dealerId,
            SnapshotDate = date,

            // Inventory
            TotalVehicles = inventoryStats.Total,
            ActiveVehicles = inventoryStats.Active,
            SoldVehicles = inventoryStats.SoldToday,
            TotalInventoryValue = inventoryStats.TotalValue,
            AvgVehiclePrice = inventoryStats.AvgPrice,
            AvgDaysOnMarket = inventoryStats.AvgDaysOnMarket,

            // Engagement
            TotalViews = engagementStats.Views,
            UniqueViews = engagementStats.UniqueViews,
            TotalContacts = engagementStats.Contacts,
            TotalFavorites = engagementStats.Favorites,
            SearchImpressions = engagementStats.SearchImpressions,

            // Leads
            NewLeads = leadStats.NewLeads,
            QualifiedLeads = leadStats.Qualified,
            ConvertedLeads = leadStats.Converted,
            LeadConversionRate = leadStats.ConversionRate,

            // Revenue
            TotalRevenue = revenueStats.TotalRevenue,
            AvgTransactionValue = revenueStats.AvgTransactionValue,

            CreatedAt = DateTime.UtcNow
        };
    }
}
```

---

## 5. KPIs y Métricas Disponibles

### 5.1 Métricas de Inventario

| Métrica         | Descripción                | Fórmula                |
| --------------- | -------------------------- | ---------------------- |
| Active Listings | Vehículos activos          | Count(status='Active') |
| Inventory Value | Valor total del inventario | Sum(price)             |
| Avg Price       | Precio promedio            | Avg(price)             |
| Days on Market  | Días en el mercado         | Avg(daysActive)        |
| Aging Rate      | % con más de 60 días       | Count(days>60)/Total   |
| Turnover Rate   | Rotación mensual           | Sold/AvgInventory      |

### 5.2 Métricas de Engagement

| Métrica       | Descripción         | Fórmula            |
| ------------- | ------------------- | ------------------ |
| Total Views   | Vistas de vehículos | Sum(pageViews)     |
| View Rate     | Tasa de vistas      | Views/Impressions  |
| Contact Rate  | Tasa de contacto    | Contacts/Views     |
| Favorite Rate | Tasa de favoritos   | Favorites/Views    |
| CTR           | Click-through rate  | Clicks/Impressions |

### 5.3 Métricas de Leads

| Métrica            | Descripción          | Fórmula                |
| ------------------ | -------------------- | ---------------------- |
| Lead Volume        | Total de leads       | Count(leads)           |
| Qualification Rate | Tasa de calificación | Qualified/Total        |
| Conversion Rate    | Tasa de conversión   | Converted/Qualified    |
| Response Time      | Tiempo de respuesta  | Avg(firstResponseTime) |
| Lead Quality Score | Score promedio       | Avg(leadScore)         |

---

## 6. Alertas Automáticas

### 6.1 Tipos de Alertas

```csharp
public enum DealerAlertType
{
    // Inventory
    LowInventory,           // < 5 vehículos activos
    AgingInventory,         // Vehículos > 60 días sin vender
    PriceAdjustmentNeeded,  // Precio por encima del mercado

    // Engagement
    ViewsDropping,          // -30% vistas vs semana anterior
    LowContactRate,         // < 1% tasa de contacto

    // Leads
    LeadResponseSlow,       // Tiempo respuesta > 4 horas
    LeadsNotFollowedUp,     // Leads sin seguimiento > 48h
    ConversionDropping,     // -20% conversión vs mes anterior

    // Competitive
    CompetitorPriceLower,   // Competidor con precio menor
    RankingDropped,         // Perdió 5+ posiciones en ranking

    // Performance
    BadReviewReceived,      // Review < 3 estrellas
    GoalNotMet              // Objetivo mensual no alcanzado
}
```

---

## 7. Reglas de Negocio

| Código  | Regla                                   | Validación           |
| ------- | --------------------------------------- | -------------------- |
| ANA-R01 | Solo dealers activos con plan Pro+      | Plan >= Pro          |
| ANA-R02 | Datos de 90 días máximo para plan Pro   | DateRange <= 90 days |
| ANA-R03 | Datos ilimitados para plan Enterprise   | No restriction       |
| ANA-R04 | Benchmark excluye dealers inactivos     | IsActive = true      |
| ANA-R05 | Rankings calculados diariamente         | Daily job            |
| ANA-R06 | Alertas enviadas máximo 1x por tipo/día | Throttling           |

---

## 8. Eventos RabbitMQ

| Evento                       | Exchange           | Descripción            |
| ---------------------------- | ------------------ | ---------------------- |
| `DealerSnapshotCreatedEvent` | `analytics.events` | Snapshot diario creado |
| `DealerAlertTriggeredEvent`  | `analytics.events` | Alerta disparada       |
| `DealerRankingChangedEvent`  | `analytics.events` | Cambio en ranking      |
| `DealerGoalAchievedEvent`    | `analytics.events` | Meta alcanzada         |

---

## 9. Configuración

```json
{
  "DealerAnalytics": {
    "SnapshotCron": "0 0 * * *",
    "RetentionDays": {
      "Starter": 30,
      "Pro": 90,
      "Enterprise": 365
    },
    "Alerts": {
      "LowInventoryThreshold": 5,
      "AgingDaysThreshold": 60,
      "ViewsDropThreshold": 30,
      "ResponseTimeThresholdMinutes": 240,
      "ConversionDropThreshold": 20
    },
    "Cache": {
      "SnapshotTTLMinutes": 5,
      "BenchmarkTTLMinutes": 60
    }
  }
}
```

---

## 10. Métricas Prometheus

```
# Snapshot generation
dealer_snapshots_generated_total{status="success|error"}

# Dashboard requests
dealer_analytics_requests_total{endpoint="...", dealer_plan="..."}

# Alert triggers
dealer_alerts_triggered_total{type="...", severity="..."}

# Cache performance
dealer_analytics_cache_hits_total
dealer_analytics_cache_misses_total
```

---

## 📚 Referencias

- [02-dealer-management.md](02-dealer-management.md) - Gestión de dealers
- [03-event-tracking.md](../09-REPORTES-ANALYTICS/03-event-tracking.md) - Event tracking
- [01-reports-service.md](../09-REPORTES-ANALYTICS/01-reports-service.md) - Reportes
