# 🎯 Deal Rating - Calificación de Precio

> **Código:** PRICE-001  
> **Versión:** 1.0  
> **Última actualización:** Enero 21, 2026  
> **Criticidad:** 🔴 CRÍTICA (Diferenciador competitivo #1)  
> **Origen:** CarGurus

---

## 📋 Información General

| Campo             | Valor                                 |
| ----------------- | ------------------------------------- |
| **Servicio**      | PricingIntelligenceService (NUEVO)    |
| **Puerto**        | 5090                                  |
| **Base de Datos** | `pricingintelligenceservice`          |
| **Dependencias**  | VehiclesSaleService, AnalyticsService |

---

## 🎯 Objetivo del Proceso

1. **Transparencia:** Mostrar si un precio es justo o no
2. **Confianza:** Usuarios confían más en la plataforma
3. **Diferenciación:** Feature clave que SuperCarros NO tiene
4. **Conversión:** Usuarios compran más rápido con información clara

---

## 📊 Sistema de Calificación

```
┌─────────────────────────────────────────────────────────────────────────┐
│                     DEAL RATING - ESCALA DE PRECIOS                     │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  🟢 EXCELENTE PRECIO   │  15%+ por debajo del mercado                  │
│  ────────────────────  │  "Este vehículo tiene un precio excepcional"  │
│                                                                         │
│  🟢 BUEN PRECIO        │  5-15% por debajo del mercado                 │
│  ────────────────────  │  "Este vehículo tiene buen precio"            │
│                                                                         │
│  🟡 PRECIO JUSTO       │  Dentro del rango de mercado (±5%)            │
│  ────────────────────  │  "Este vehículo tiene precio de mercado"      │
│                                                                         │
│  🟠 PRECIO ALTO        │  5-15% por encima del mercado                 │
│  ────────────────────  │  "Este vehículo está por encima del mercado"  │
│                                                                         │
│  🔴 SOBREPRECIADO      │  15%+ por encima del mercado                  │
│  ────────────────────  │  "Este vehículo está muy por encima"          │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 📡 Endpoints

| Método | Endpoint                                | Descripción                    | Auth        |
| ------ | --------------------------------------- | ------------------------------ | ----------- |
| `GET`  | `/api/pricing/deal-rating/{vehicleId}`  | Obtener rating de un vehículo  | ❌          |
| `GET`  | `/api/pricing/market-value/{vehicleId}` | Valor de mercado estimado      | ❌          |
| `GET`  | `/api/pricing/similar-prices`           | Precios de vehículos similares | ❌          |
| `POST` | `/api/pricing/calculate`                | Calcular rating (internal)     | ✅ Internal |
| `GET`  | `/api/pricing/factors/{vehicleId}`      | Factores que afectan el precio | ❌          |

---

## 🗃️ Entidades

### DealRating

```csharp
public class DealRating
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }

    // Rating
    public DealRatingLevel Rating { get; set; }
    public string RatingLabel { get; set; }
    public string RatingDescription { get; set; }
    public string RatingColor { get; set; }  // #22c55e, #eab308, etc.

    // Valores
    public decimal ListedPrice { get; set; }
    public decimal MarketValue { get; set; }
    public decimal PriceDifference { get; set; }
    public decimal PriceDifferencePercent { get; set; }

    // Contexto
    public int SimilarVehiclesCount { get; set; }
    public decimal SimilarVehiclesAvgPrice { get; set; }
    public decimal SimilarVehiclesMinPrice { get; set; }
    public decimal SimilarVehiclesMaxPrice { get; set; }

    // Factores
    public List<PricingFactor> Factors { get; set; }

    // Metadata
    public DateTime CalculatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }  // Recalcular cada 24h
    public int ConfidenceScore { get; set; }  // 0-100
}

public enum DealRatingLevel
{
    GreatDeal = 1,      // 🟢 Excelente Precio
    GoodDeal = 2,       // 🟢 Buen Precio
    FairDeal = 3,       // 🟡 Precio Justo
    HighPrice = 4,      // 🟠 Precio Alto
    Overpriced = 5      // 🔴 Sobrepreciado
}

public class PricingFactor
{
    public string Name { get; set; }
    public string Description { get; set; }
    public FactorImpact Impact { get; set; }  // Positive, Negative, Neutral
    public decimal ImpactAmount { get; set; }
}

// Ejemplos de factores:
// - "Bajo kilometraje" → Positive, +$50,000
// - "Alto kilometraje" → Negative, -$80,000
// - "Color popular" → Positive, +$20,000
// - "Versión full" → Positive, +$100,000
// - "Historial de accidentes" → Negative, -$150,000
```

### MarketValueEstimate

```csharp
public class MarketValueEstimate
{
    public Guid Id { get; set; }

    // Vehículo
    public string Make { get; set; }
    public string Model { get; set; }
    public int Year { get; set; }
    public string Trim { get; set; }
    public int Mileage { get; set; }
    public string Condition { get; set; }

    // Valores
    public decimal EstimatedValue { get; set; }
    public decimal ValueRangeLow { get; set; }
    public decimal ValueRangeHigh { get; set; }

    // Comparables
    public int ComparablesCount { get; set; }
    public List<ComparableVehicle> Comparables { get; set; }

    // Tendencia
    public decimal PriceChange30Days { get; set; }
    public decimal PriceChange90Days { get; set; }
    public PriceTrend Trend { get; set; }

    public DateTime CalculatedAt { get; set; }
}

public class ComparableVehicle
{
    public Guid VehicleId { get; set; }
    public string Title { get; set; }
    public decimal Price { get; set; }
    public int Mileage { get; set; }
    public int Year { get; set; }
    public string Location { get; set; }
    public int DaysOnMarket { get; set; }
    public decimal SimilarityScore { get; set; }  // 0-1
}
```

---

## 📊 Proceso PRICE-001: Calcular Deal Rating

```
┌─────────────────────────────────────────────────────────────────────────┐
│ PROCESO: PRICE-001 - Calcular Deal Rating de un Vehículo               │
├─────────────────────────────────────────────────────────────────────────┤
│ Actor Iniciador: Sistema (evento) o Usuario (request)                  │
│ Sistemas: PricingIntelligenceService, VehiclesSaleService              │
│ Triggers: Nuevo listing, Cambio de precio, Request de usuario          │
│ Criticidad: ALTA                                                        │
└─────────────────────────────────────────────────────────────────────────┘
```

| Paso | Subpaso | Acción                                     | Sistema                    | Actor   | Evidencia           | Código    |
| ---- | ------- | ------------------------------------------ | -------------------------- | ------- | ------------------- | --------- |
| 1    | 1.1     | **Trigger: Nuevo vehículo o request**      | RabbitMQ/API               | Sistema | Event/Request       | EVD-LOG   |
| 1    | 1.2     | Obtener datos del vehículo                 | VehiclesSaleService        | Sistema | Vehicle data        | EVD-LOG   |
| 2    | 2.1     | **Buscar vehículos comparables**           | VehiclesSaleService        | Sistema | **Query ejecutado** | EVD-LOG   |
| 2    | 2.2     | Criterios: Marca, Modelo, ±2 años, ±30% km | Sistema                    | Sistema | Filters applied     | EVD-LOG   |
| 2    | 2.3     | Mínimo 5 comparables requeridos            | Sistema                    | Sistema | Validation          | EVD-LOG   |
| 3    | 3.1     | **Calcular valor de mercado**              | PricingIntelligenceService | Sistema | **Market value**    | EVD-AUDIT |
| 3    | 3.2     | Promedio ponderado por similaridad         | Sistema                    | Sistema | Weighted avg        | EVD-LOG   |
| 3    | 3.3     | Ajustar por factores (km, condición, etc.) | Sistema                    | Sistema | Adjustments         | EVD-LOG   |
| 4    | 4.1     | **Calcular diferencia de precio**          | PricingIntelligenceService | Sistema | **Difference**      | EVD-LOG   |
| 4    | 4.2     | (ListedPrice - MarketValue) / MarketValue  | Sistema                    | Sistema | Percentage          | EVD-LOG   |
| 5    | 5.1     | **Asignar Deal Rating**                    | PricingIntelligenceService | Sistema | **Rating assigned** | EVD-AUDIT |
| 5    | 5.2     | Basado en porcentaje de diferencia         | Sistema                    | Sistema | Logic applied       | EVD-LOG   |
| 6    | 6.1     | **Identificar factores de precio**         | PricingIntelligenceService | Sistema | **Factors**         | EVD-LOG   |
| 6    | 6.2     | Km, condición, features, ubicación         | Sistema                    | Sistema | Analysis            | EVD-LOG   |
| 7    | 7.1     | **Calcular confidence score**              | PricingIntelligenceService | Sistema | **Confidence**      | EVD-LOG   |
| 7    | 7.2     | Basado en cantidad de comparables          | Sistema                    | Sistema | Score calc          | EVD-LOG   |
| 8    | 8.1     | **Guardar DealRating**                     | PricingIntelligenceService | Sistema | **Record saved**    | EVD-AUDIT |
| 8    | 8.2     | TTL: 24 horas (recalcular diario)          | Sistema                    | Sistema | Expiration set      | EVD-LOG   |
| 9    | 9.1     | **Actualizar índice de búsqueda**          | SearchService              | Sistema | **Index updated**   | EVD-LOG   |
| 9    | 9.2     | Para filtro por Deal Rating                | Sistema                    | Sistema | Filter ready        | EVD-LOG   |
| 10   | 10.1    | **Audit trail**                            | AuditService               | Sistema | Complete audit      | EVD-AUDIT |

### Evidencia de Cálculo

```json
{
  "processCode": "PRICE-001",
  "dealRating": {
    "vehicleId": "veh-67890",
    "vehicle": {
      "title": "Toyota Corolla 2023 - Excelente Estado",
      "make": "Toyota",
      "model": "Corolla",
      "year": 2023,
      "mileage": 25000,
      "listedPrice": 1250000
    },
    "rating": {
      "level": "GoodDeal",
      "label": "Buen Precio",
      "description": "Este vehículo tiene buen precio, está 8% por debajo del mercado",
      "color": "#22c55e"
    },
    "marketAnalysis": {
      "estimatedValue": 1358000,
      "priceDifference": -108000,
      "priceDifferencePercent": -7.95,
      "valueRangeLow": 1200000,
      "valueRangeHigh": 1500000
    },
    "comparables": {
      "count": 23,
      "avgPrice": 1345000,
      "minPrice": 1150000,
      "maxPrice": 1550000,
      "samples": [
        {
          "id": "veh-111",
          "title": "Toyota Corolla 2023",
          "price": 1320000,
          "mileage": 28000,
          "similarity": 0.95
        },
        {
          "id": "veh-222",
          "title": "Toyota Corolla 2022",
          "price": 1280000,
          "mileage": 22000,
          "similarity": 0.88
        }
      ]
    },
    "factors": [
      {
        "name": "Bajo kilometraje",
        "description": "25,000 km está por debajo del promedio para un 2023",
        "impact": "Positive",
        "impactAmount": 50000
      },
      {
        "name": "Condición excelente",
        "description": "El vendedor indica condición excelente",
        "impact": "Positive",
        "impactAmount": 30000
      },
      {
        "name": "Ubicación Santo Domingo",
        "description": "Mayor demanda en la capital",
        "impact": "Positive",
        "impactAmount": 20000
      }
    ],
    "confidence": {
      "score": 87,
      "level": "High",
      "reason": "23 vehículos comparables encontrados"
    },
    "calculatedAt": "2026-01-21T10:00:00Z",
    "expiresAt": "2026-01-22T10:00:00Z"
  }
}
```

---

## 🧮 Algoritmo de Cálculo

### 1. Selección de Comparables

```csharp
public async Task<List<ComparableVehicle>> FindComparables(Vehicle vehicle)
{
    var query = _context.Vehicles
        .Where(v => v.Id != vehicle.Id)
        .Where(v => v.Status == "Active")
        .Where(v => v.Make == vehicle.Make)
        .Where(v => v.Model == vehicle.Model)
        .Where(v => v.Year >= vehicle.Year - 2 && v.Year <= vehicle.Year + 2)
        .Where(v => v.Mileage >= vehicle.Mileage * 0.7m && v.Mileage <= vehicle.Mileage * 1.3m);

    var comparables = await query
        .OrderByDescending(v => CalculateSimilarity(vehicle, v))
        .Take(50)
        .ToListAsync();

    return comparables;
}

private decimal CalculateSimilarity(Vehicle target, Vehicle comparable)
{
    decimal score = 1.0m;

    // Año (peso: 30%)
    int yearDiff = Math.Abs(target.Year - comparable.Year);
    score -= yearDiff * 0.10m;

    // Kilometraje (peso: 25%)
    decimal mileageDiff = Math.Abs(target.Mileage - comparable.Mileage) / (decimal)target.Mileage;
    score -= mileageDiff * 0.25m;

    // Trim/Versión (peso: 20%)
    if (target.Trim == comparable.Trim) score += 0.10m;

    // Ubicación (peso: 15%)
    if (target.City == comparable.City) score += 0.08m;
    else if (target.Province == comparable.Province) score += 0.04m;

    // Condición (peso: 10%)
    if (target.Condition == comparable.Condition) score += 0.05m;

    return Math.Max(0, Math.Min(1, score));
}
```

### 2. Cálculo de Valor de Mercado

```csharp
public decimal CalculateMarketValue(List<ComparableVehicle> comparables, Vehicle target)
{
    // Promedio ponderado por similaridad
    decimal weightedSum = 0;
    decimal totalWeight = 0;

    foreach (var comp in comparables)
    {
        weightedSum += comp.Price * comp.SimilarityScore;
        totalWeight += comp.SimilarityScore;
    }

    decimal baseValue = weightedSum / totalWeight;

    // Ajustes por factores
    decimal adjustments = 0;

    // Ajuste por kilometraje
    decimal avgMileage = comparables.Average(c => c.Mileage);
    decimal mileageDiff = avgMileage - target.Mileage;
    adjustments += mileageDiff * 0.50m;  // RD$0.50 por km de diferencia

    // Ajuste por condición
    adjustments += GetConditionAdjustment(target.Condition);

    // Ajuste por features premium
    adjustments += GetFeaturesAdjustment(target.Features);

    return baseValue + adjustments;
}
```

### 3. Asignación de Rating

```csharp
public DealRatingLevel AssignRating(decimal listedPrice, decimal marketValue)
{
    decimal diffPercent = (listedPrice - marketValue) / marketValue * 100;

    return diffPercent switch
    {
        <= -15 => DealRatingLevel.GreatDeal,    // 🟢 Excelente
        <= -5 => DealRatingLevel.GoodDeal,       // 🟢 Bueno
        <= 5 => DealRatingLevel.FairDeal,        // 🟡 Justo
        <= 15 => DealRatingLevel.HighPrice,      // 🟠 Alto
        _ => DealRatingLevel.Overpriced          // 🔴 Sobrepreciado
    };
}
```

---

## 📱 UI Mockups

### Badge en Card de Vehículo

```
┌─────────────────────────────────────────────────────────────────────────┐
│ ┌────────────────────────────────────────────────────────────────────┐ │
│ │                                                        ┌─────────┐ │ │
│ │  [IMAGEN DEL VEHÍCULO]                                │ 🟢 BUEN │ │ │
│ │                                                        │ PRECIO  │ │ │
│ │                                                        └─────────┘ │ │
│ └────────────────────────────────────────────────────────────────────┘ │
│                                                                         │
│  Toyota Corolla 2023                                                   │
│  25,000 km · Santo Domingo                                             │
│                                                                         │
│  RD$ 1,250,000                                                         │
│  ───────────────                                                       │
│  💡 RD$108,000 por debajo del mercado                                  │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### Sección en Vehicle Detail

```
┌─────────────────────────────────────────────────────────────────────────┐
│                                                                         │
│  📊 ANÁLISIS DE PRECIO                                                 │
│  ════════════════════════════════════════════════════════════════════  │
│                                                                         │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │                                                                 │   │
│  │   🟢 BUEN PRECIO                                               │   │
│  │   Este vehículo está 8% por debajo del mercado                 │   │
│  │                                                                 │   │
│  │   Precio listado:    RD$ 1,250,000                             │   │
│  │   Valor de mercado:  RD$ 1,358,000                             │   │
│  │   Ahorro estimado:   RD$ 108,000  ✓                            │   │
│  │                                                                 │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│                                                                         │
│  📈 Rango de precios para Toyota Corolla 2023 similar:                 │
│                                                                         │
│  RD$1.15M ──────●─────────────────────────────── RD$1.55M              │
│            ↑                                                            │
│       Este vehículo                                                    │
│                                                                         │
│  📋 Factores que afectan el precio:                                    │
│                                                                         │
│  ✅ Bajo kilometraje                    +RD$50,000                     │
│  ✅ Condición excelente                 +RD$30,000                     │
│  ✅ Ubicación Santo Domingo             +RD$20,000                     │
│                                                                         │
│  ℹ️ Basado en 23 vehículos similares en el mercado                     │
│  🔄 Actualizado: hace 2 horas                                          │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 📊 Métricas Prometheus

```yaml
# Cálculos
deal_rating_calculations_total
deal_rating_calculation_duration_ms
deal_rating_by_level{level="GreatDeal|GoodDeal|FairDeal|HighPrice|Overpriced"}

# Comparables
comparables_found_avg
comparables_insufficient_total  # Menos de 5

# Confidence
confidence_score_avg
low_confidence_ratings_total  # Score < 50

# Engagement
deal_rating_views_total
deal_rating_click_to_contact_rate
great_deal_conversion_rate

# Performance
cache_hit_rate
recalculation_trigger_total{reason}
```

---

## 🔄 Jobs Programados

```yaml
# Recalcular ratings expirados (cada hora)
- name: RecalculateExpiredRatings
  schedule: "0 * * * *"
  action: Recalcular ratings con ExpiresAt < now()

# Actualizar valores de mercado (diario)
- name: RefreshMarketValues
  schedule: "0 3 * * *"
  action: Recalcular valores de mercado para todos los modelos

# Limpiar ratings de vehículos inactivos
- name: CleanupInactiveRatings
  schedule: "0 4 * * 0"
  action: Eliminar ratings de vehículos vendidos/inactivos
```

---

## 🔗 Referencias

- [00-ANALISIS-COMPETITIVO.md](../00-ANALISIS-COMPETITIVO.md)
- [03-VEHICULOS-INVENTARIO/01-vehicles-sale-service.md](../03-VEHICULOS-INVENTARIO/01-vehicles-sale-service.md)
- [04-BUSQUEDA-FILTROS/03-filtros-avanzados.md](../04-BUSQUEDA-FILTROS/03-filtros-avanzados.md)
