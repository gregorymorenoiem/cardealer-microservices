# 🤖 VehicleIntelligenceService - Matriz de Procesos

> **Servicio:** VehicleIntelligenceService  
> **Puerto:** 5056  
> **Última actualización:** Enero 25, 2026  
> **Estado de Implementación:** ✅ Backend 100% | 🟡 UI 40%

---

## ⚠️ AUDITORÍA DE ACCESO UI (Enero 25, 2026)

> **Estado:** Backend implementado. Consumido parcialmente en UI.

| Proceso          | Backend | UI Access | Observación           |
| ---------------- | ------- | --------- | --------------------- |
| Price suggestion | ✅ 100% | 🟡 60%    | Sugerencia en `/sell` |
| Market analysis  | ✅ 100% | 🔴 0%     | Sin dashboard         |
| Demand forecast  | ✅ 100% | 🔴 0%     | Sin gráficos          |
| Deal rating      | ✅ 100% | ✅ 100%   | Badge en listings     |
| Recommendations  | ✅ 100% | 🟡 50%    | "Similar vehicles"    |

### Puntos de Consumo UI ✅

- ✅ Badge Deal Rating en cards
- ✅ "Vehículos similares" en detalle
- ✅ Sugerencia de precio en `/sell`

### Rutas UI Faltantes 🔴

| Ruta Propuesta            | Funcionalidad       | Prioridad |
| ------------------------- | ------------------- | --------- |
| `/dealer/market-analysis` | Análisis de mercado | 🟡 MEDIA  |
| `/admin/ml/dashboard`     | Dashboard de ML     | 🟢 BAJA   |

**Verificación Backend:** VehicleIntelligenceService existe en `/backend/VehicleIntelligenceService/` ✅

---

## 📊 Resumen de Implementación (ACTUALIZADO)

| Componente    | Total | Implementado | Pendiente | Estado  |
| ------------- | ----- | ------------ | --------- | ------- |
| Controllers   | 3     | 3            | 0         | ✅ 100% |
| VINT-PRICE-\* | 5     | 5            | 0         | ✅ 100% |
| VINT-DEM-\*   | 4     | 4            | 0         | ✅ 100% |
| VINT-ML-\*    | 6     | 5            | 1         | 🟡 83%  |
| VINT-REC-\*   | 4     | 4            | 0         | ✅ 100% |
| Tests         | 15    | 12           | 3         | 🟡 80%  |

**Leyenda:** ✅ Implementado + Tested | 🟢 Implementado | 🟡 En Progreso | 🔴 Pendiente

---

## �📋 Información General

| Aspecto           | Detalle                                                                                                                                               |
| ----------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Servicio**      | VehicleIntelligenceService                                                                                                                            |
| **Puerto**        | 5056                                                                                                                                                  |
| **Base de Datos** | PostgreSQL (vehicle_intelligence_db)                                                                                                                  |
| **Tecnología**    | .NET 8, MediatR, ML.NET / Python ML Models                                                                                                            |
| **Mensajería**    | RabbitMQ                                                                                                                                              |
| **Descripción**   | Inteligencia artificial para análisis de precios, predicción de demanda, recomendaciones de compra y sugerencias de pricing para dealers y vendedores |

### Arquitectura

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                  VehicleIntelligenceService Architecture                     │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│   Data Sources                       Core Service                            │
│   ┌────────────────┐                ┌────────────────────────────────┐      │
│   │ VehiclesSale   │──┐             │     VehicleIntelligenceService   │      │
│   │ Service        │  │             │  ┌──────────────────────────┐   │      │
│   └────────────────┘  │             │  │ Controllers              │   │      │
│   ┌────────────────┐  │             │  │ • PricingController      │   │      │
│   │ FeatureStore   │──┼────────────▶│  │ • DemandController       │   │      │
│   │ Service        │  │             │  │ • IntelligenceController │   │      │
│   └────────────────┘  │             │  └──────────────────────────┘   │      │
│   ┌────────────────┐  │             │  ┌──────────────────────────┐   │      │
│   │ Market Data    │──┤             │  │ ML Models (ML.NET)       │   │      │
│   │ (External)     │  │             │  │ • Price Prediction       │   │      │
│   └────────────────┘  │             │  │ • Demand Forecast        │   │      │
│   ┌────────────────┐  │             │  │ • Market Analysis        │   │      │
│   │ EventTracking  │──┘             │  └──────────────────────────┘   │      │
│   │ Service        │               │  ┌──────────────────────────┐   │      │
│   └────────────────┘               │  │ Domain                   │   │      │
│                                    │  │ • PriceAnalysis          │   │      │
│   Consumers                        │  │ • DemandPrediction       │   │      │
│   ┌────────────────┐               │  │ • MarketInsight          │   │      │
│   │ Publish Wizard │◀─────────────│  └──────────────────────────┘   │      │
│   │ (Price Suggest)│               └────────────────────────────────┘      │
│   └────────────────┘                           │                        │
│   ┌────────────────┐               ┌───────────┼───────────┐                │
│   │ Dealer Dash    │◀────────────▼           ▼           ▼                │
│   │ (Demand View)  │       ┌────────────┐ ┌────────────┐ ┌────────────┐   │
│   └────────────────┘       │ PostgreSQL │ │   Redis    │ │  RabbitMQ  │   │
│                           │ (Analysis, │ │ (Cached    │ │ (Analysis  │   │
│                           │  Insights) │ │  Results)  │ │  Events)   │   │
│                           └────────────┘ └────────────┘ └────────────┘   │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 🎯 Endpoints del Servicio

### PricingController

| Método | Endpoint                                  | Descripción                    | Auth | Roles         |
| ------ | ----------------------------------------- | ------------------------------ | ---- | ------------- |
| `POST` | `/api/pricing/analyze`                    | Analizar precio de un vehículo | ✅   | Dealer/Seller |
| `GET`  | `/api/pricing/vehicle/{vehicleId}/latest` | Último análisis de un vehículo | ✅   | Owner         |
| `GET`  | `/api/pricing/{id}`                       | Obtener análisis por ID        | ✅   | Owner         |

### VehicleIntelligenceController

| Método | Endpoint                                     | Descripción                               | Auth | Roles         |
| ------ | -------------------------------------------- | ----------------------------------------- | ---- | ------------- |
| `POST` | `/api/vehicleintelligence/price-suggestion`  | Sugerencia de precio (wizard publicación) | ✅   | Dealer/Seller |
| `GET`  | `/api/vehicleintelligence/demand/categories` | Demanda por categoría (dashboard dealer)  | ✅   | Dealer        |

### DemandController

| Método | Endpoint                            | Descripción                       | Auth | Roles   |
| ------ | ----------------------------------- | --------------------------------- | ---- | ------- |
| `POST` | `/api/demand/predict`               | Predecir demanda para un vehículo | ✅   | Dealer  |
| `GET`  | `/api/demand/{make}/{model}/{year}` | Obtener predicción de demanda     | ❌   | Público |

---

## 📊 Entidades del Dominio

### PriceAnalysis (Análisis de Precio)

```csharp
public class PriceAnalysis
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public decimal CurrentPrice { get; set; }

    // Precio sugerido
    public decimal SuggestedPrice { get; set; }
    public decimal SuggestedPriceMin { get; set; }
    public decimal SuggestedPriceMax { get; set; }

    // Comparación con mercado
    public decimal MarketAvgPrice { get; set; }
    public decimal PriceVsMarket { get; set; }        // 1.05 = 5% arriba del mercado
    public string PricePosition { get; set; }         // "Above Market", "Below Market", "Fair"

    // Predicción de tiempo de venta
    public int PredictedDaysToSaleAtCurrentPrice { get; set; }
    public int PredictedDaysToSaleAtSuggestedPrice { get; set; }

    // Metadata
    public DateTime AnalysisDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Confianza del modelo
    public decimal ConfidenceScore { get; set; }      // 0-100

    // Factores que influyen en el precio (JSON)
    public string InfluencingFactors { get; set; }
}
```

### DemandPrediction (Predicción de Demanda)

```csharp
public class DemandPrediction
{
    public Guid Id { get; set; }
    public string Make { get; set; }
    public string Model { get; set; }
    public int Year { get; set; }

    // Demanda actual
    public DemandLevel CurrentDemand { get; set; }
    public decimal DemandScore { get; set; }          // 0-100

    // Tendencia
    public TrendDirection Trend { get; set; }
    public decimal TrendStrength { get; set; }        // 0-1

    // Predicción futura
    public DemandLevel PredictedDemand30Days { get; set; }
    public DemandLevel PredictedDemand90Days { get; set; }

    // Estadísticas de mercado
    public int SearchesPerDay { get; set; }
    public int AvailableInventory { get; set; }
    public decimal AvgDaysToSale { get; set; }

    // Recomendación de compra para dealers
    public BuyRecommendation BuyRecommendation { get; set; }
    public string BuyRecommendationReason { get; set; }

    // Insights como JSON array
    public string Insights { get; set; }

    public DateTime PredictionDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

### Enumeraciones

```csharp
public enum DemandLevel
{
    VeryHigh,   // Se venden en menos de 15 días
    High,       // Se venden en 15-30 días
    Medium,     // Se venden en 30-60 días
    Low,        // Se venden en 60-90 días
    VeryLow     // Difícil de vender, >90 días
}

public enum TrendDirection
{
    Rising,     // Demanda subiendo
    Stable,     // Demanda estable
    Falling     // Demanda bajando
}

public enum BuyRecommendation
{
    StrongBuy,  // Excelente oportunidad de compra
    Buy,        // Buena oportunidad
    Hold,       // Esperar mejores condiciones
    Avoid       // No recomendado comprar
}
```

---

## 🔄 Procesos Detallados

### PROCESO 1: Analizar Precio de Vehículo

#### Endpoint: `POST /api/pricing/analyze`

| Paso | Actor         | Acción                              | Sistema                               | Resultado                 |
| ---- | ------------- | ----------------------------------- | ------------------------------------- | ------------------------- |
| 1    | Dealer/Seller | Solicita análisis de precio         | HTTP POST                             | Request recibido          |
| 2    | API           | Valida autenticación                | JWT check                             | Autorizado                |
| 3    | Validador     | Valida datos del vehículo           | FluentValidation                      | Datos válidos             |
| 4    | Handler       | Obtiene datos del vehículo          | VehicleId, Make, Model, Year, Mileage | Datos obtenidos           |
| 5    | ML Engine     | Busca comparables en el mercado     | Search similar vehicles               | Comparables encontrados   |
| 6    | ML Engine     | Calcula precio promedio de mercado  | AVG(comparable prices)                | MarketAvgPrice calculado  |
| 7    | ML Engine     | Aplica modelo de pricing            | ML.NET / Python model                 | SuggestedPrice generado   |
| 8    | ML Engine     | Calcula rango de precio             | Min/Max basado en condición           | Rango calculado           |
| 9    | ML Engine     | Predice días hasta venta            | Regression model                      | PredictedDays calculado   |
| 10   | ML Engine     | Genera recomendaciones              | Rule-based + ML                       | Recommendations generadas |
| 11   | Handler       | Crea entidad PriceAnalysis          | new PriceAnalysis()                   | Análisis creado           |
| 12   | Repository    | Persiste análisis                   | INSERT price_analyses                 | Guardado en BD            |
| 13   | EventBus      | Publica PriceAnalysisCompletedEvent | RabbitMQ                              | Evento emitido            |
| 14   | API           | Retorna análisis completo           | HTTP 200                              | PriceAnalysisDto          |

#### Request Body

```json
{
  "vehicleId": "vehicle-uuid",
  "make": "Toyota",
  "model": "Camry",
  "year": 2022,
  "mileage": 25000,
  "condition": "Good",
  "fuelType": "Gasoline",
  "transmission": "Automatic",
  "currentPrice": 1250000,
  "photoCount": 15,
  "viewCount": 125,
  "daysListed": 14
}
```

#### Response (200 OK)

```json
{
  "id": "analysis-uuid",
  "vehicleId": "vehicle-uuid",
  "currentPrice": 1250000,
  "suggestedPrice": 1180000,
  "suggestedPriceMin": 1100000,
  "suggestedPriceMax": 1250000,
  "marketAvgPrice": 1150000,
  "priceVsMarket": 1.087,
  "pricePosition": "Above Market",
  "predictedDaysToSaleAtCurrentPrice": 45,
  "predictedDaysToSaleAtSuggestedPrice": 21,
  "confidenceScore": 87.5,
  "analysisDate": "2026-01-21T10:30:00Z",
  "recommendations": [
    {
      "id": "rec-uuid-1",
      "type": "Price Reduction",
      "reason": "El precio actual está 8.7% por encima del mercado",
      "suggestedValue": 1180000,
      "impactDescription": "Reducir precio acortaría tiempo de venta en ~24 días",
      "priority": 1
    },
    {
      "id": "rec-uuid-2",
      "type": "Add Photos",
      "reason": "Vehículos con 20+ fotos venden 15% más rápido",
      "suggestedValue": null,
      "impactDescription": "Agregar 5 fotos más del interior",
      "priority": 2
    }
  ],
  "comparables": [
    {
      "id": "comp-uuid-1",
      "source": "OKLA Marketplace",
      "make": "Toyota",
      "model": "Camry",
      "year": 2022,
      "mileage": 28000,
      "price": 1150000,
      "similarityScore": 0.92,
      "status": "Sold",
      "daysOnMarket": 18,
      "externalUrl": null
    },
    {
      "id": "comp-uuid-2",
      "source": "OKLA Marketplace",
      "make": "Toyota",
      "model": "Camry",
      "year": 2022,
      "mileage": 22000,
      "price": 1200000,
      "similarityScore": 0.89,
      "status": "Active",
      "daysOnMarket": 7,
      "externalUrl": null
    }
  ]
}
```

---

### PROCESO 2: Sugerencia de Precio (Wizard de Publicación)

#### Endpoint: `POST /api/vehicleintelligence/price-suggestion`

| Paso | Actor     | Acción                                  | Sistema                                   | Resultado                     |
| ---- | --------- | --------------------------------------- | ----------------------------------------- | ----------------------------- |
| 1    | Seller    | Solicita sugerencia durante publicación | HTTP POST                                 | Request recibido              |
| 2    | API       | Valida autenticación                    | JWT check                                 | Autorizado                    |
| 3    | Validador | Valida campos requeridos                | PriceSuggestionRequestValidator           | Validación OK                 |
| 4    | Handler   | Consulta modelo de pricing              | GetPriceSuggestionQuery                   | Query enviado                 |
| 5    | ML Engine | Analiza mercado local                   | Location-based analysis                   | Mercado analizado             |
| 6    | ML Engine | Calcula precio de mercado               | Market price model                        | MarketPrice calculado         |
| 7    | ML Engine | Genera precio sugerido                  | Optimization model                        | SuggestedPrice generado       |
| 8    | ML Engine | Calcula diferencia                      | (AskingPrice - MarketPrice) / MarketPrice | DeltaPercent calculado        |
| 9    | ML Engine | Evalúa demanda                          | Demand model                              | DemandScore calculado         |
| 10   | ML Engine | Estima días de venta                    | Time-to-sale model                        | EstimatedDaysToSell calculado |
| 11   | ML Engine | Genera tips personalizados              | Rule engine                               | Tips generados                |
| 12   | API       | Retorna sugerencia                      | HTTP 200                                  | PriceSuggestionDto            |

#### Request Body

```json
{
  "make": "Honda",
  "model": "Civic",
  "year": 2023,
  "mileage": 15000,
  "bodyType": "Sedan",
  "location": "Santo Domingo",
  "askingPrice": 950000
}
```

#### Response (200 OK)

```json
{
  "marketPrice": 920000,
  "suggestedPrice": 910000,
  "deltaPercent": 3.26,
  "demandScore": 78,
  "estimatedDaysToSell": 25,
  "confidence": 0.85,
  "modelVersion": "pricing-model-v2.3.1",
  "tips": [
    "Tu precio está 3.3% por encima del mercado",
    "Honda Civic 2023 tiene alta demanda en Santo Domingo",
    "Considera agregar fotos del motor para destacar",
    "Los vehículos con video venden 20% más rápido"
  ]
}
```

---

### PROCESO 3: Predecir Demanda de Vehículo

#### Endpoint: `POST /api/demand/predict`

| Paso | Actor      | Acción                         | Sistema                   | Resultado                |
| ---- | ---------- | ------------------------------ | ------------------------- | ------------------------ |
| 1    | Dealer     | Solicita predicción de demanda | HTTP POST                 | Request recibido         |
| 2    | Handler    | Consulta datos históricos      | Search events, sales data | Datos históricos         |
| 3    | ML Engine  | Analiza búsquedas recientes    | Search volume analysis    | SearchesPerDay calculado |
| 4    | ML Engine  | Cuenta inventario disponible   | Active listings count     | AvailableInventory       |
| 5    | ML Engine  | Calcula promedio de venta      | AVG(days to sale)         | AvgDaysToSale            |
| 6    | ML Engine  | Determina nivel de demanda     | Classification model      | CurrentDemand asignado   |
| 7    | ML Engine  | Calcula score de demanda       | Scoring model             | DemandScore (0-100)      |
| 8    | ML Engine  | Analiza tendencia              | Time series analysis      | Trend + TrendStrength    |
| 9    | ML Engine  | Predice demanda futura         | Forecasting model         | PredictedDemand30/90Days |
| 10   | ML Engine  | Genera recomendación de compra | Decision model            | BuyRecommendation        |
| 11   | ML Engine  | Genera insights                | NLP-based insights        | Insights array           |
| 12   | Repository | Persiste predicción            | INSERT demand_predictions | Guardado                 |
| 13   | API        | Retorna predicción             | HTTP 200                  | DemandPredictionDto      |

#### Request Body

```json
{
  "make": "Toyota",
  "model": "RAV4",
  "year": 2022,
  "fuelType": "Hybrid",
  "transmission": "Automatic"
}
```

#### Response (200 OK)

```json
{
  "id": "prediction-uuid",
  "make": "Toyota",
  "model": "RAV4",
  "year": 2022,
  "currentDemand": "VeryHigh",
  "demandScore": 92,
  "trend": "Rising",
  "trendStrength": 0.75,
  "predictedDemand30Days": "VeryHigh",
  "predictedDemand90Days": "High",
  "searchesPerDay": 145,
  "availableInventory": 12,
  "avgDaysToSale": 11.5,
  "buyRecommendation": "StrongBuy",
  "buyRecommendationReason": "Alta demanda, bajo inventario, tendencia alcista. Excelente oportunidad de compra.",
  "insights": [
    "RAV4 Hybrid es el SUV más buscado en RD",
    "El inventario actual no cubre la demanda",
    "Precio promedio subió 5% en los últimos 30 días",
    "Tiempo de venta promedio: 11.5 días (muy rápido)",
    "Se espera que la demanda se mantenga alta en Q1 2026"
  ],
  "predictionDate": "2026-01-21T10:30:00Z"
}
```

---

### PROCESO 4: Obtener Demanda por Categoría

#### Endpoint: `GET /api/vehicleintelligence/demand/categories`

| Paso | Actor      | Acción                         | Sistema                   | Resultado           |
| ---- | ---------- | ------------------------------ | ------------------------- | ------------------- |
| 1    | Dealer     | Solicita demanda por categoría | HTTP GET                  | Request recibido    |
| 2    | API        | Valida autenticación           | JWT check                 | Autorizado          |
| 3    | Handler    | Consulta demanda por categoría | GetDemandByCategoryQuery  | Query ejecutado     |
| 4    | Cache      | Verifica cache                 | Redis check               | Cache hit/miss      |
| 5    | Repository | Si cache miss, consulta BD     | SELECT demand by category | Datos obtenidos     |
| 6    | Handler    | Formatea respuesta             | List<CategoryDemandDto>   | Lista formateada    |
| 7    | API        | Retorna lista                  | HTTP 200                  | CategoryDemandDto[] |

#### Response (200 OK)

```json
[
  {
    "category": "SUVs",
    "demandScore": 88,
    "trend": "Rising",
    "updatedAt": "2026-01-21T08:00:00Z"
  },
  {
    "category": "Sedanes",
    "demandScore": 75,
    "trend": "Stable",
    "updatedAt": "2026-01-21T08:00:00Z"
  },
  {
    "category": "Camionetas",
    "demandScore": 82,
    "trend": "Rising",
    "updatedAt": "2026-01-21T08:00:00Z"
  },
  {
    "category": "Eléctricos",
    "demandScore": 65,
    "trend": "Rising",
    "updatedAt": "2026-01-21T08:00:00Z"
  },
  {
    "category": "Deportivos",
    "demandScore": 45,
    "trend": "Falling",
    "updatedAt": "2026-01-21T08:00:00Z"
  },
  {
    "category": "Lujo",
    "demandScore": 52,
    "trend": "Stable",
    "updatedAt": "2026-01-21T08:00:00Z"
  }
]
```

---

## 🔔 Eventos de Dominio (RabbitMQ)

### Eventos Publicados

| Evento                           | Exchange                     | Routing Key                   | Payload                                    |
| -------------------------------- | ---------------------------- | ----------------------------- | ------------------------------------------ |
| `PriceAnalysisCompletedEvent`    | `vehicleintelligence.events` | `pricing.analysis_completed`  | VehicleId, SuggestedPrice, ConfidenceScore |
| `DemandPredictionCompletedEvent` | `vehicleintelligence.events` | `demand.prediction_completed` | Make, Model, Year, DemandLevel             |
| `PriceAlertTriggeredEvent`       | `vehicleintelligence.events` | `pricing.alert_triggered`     | VehicleId, AlertType, Message              |
| `MarketTrendChangedEvent`        | `vehicleintelligence.events` | `market.trend_changed`        | Category, OldTrend, NewTrend               |

### Eventos Consumidos

| Evento                     | Origen              | Acción                           |
| -------------------------- | ------------------- | -------------------------------- |
| `VehicleCreatedEvent`      | VehiclesSaleService | Auto-generar análisis de precio  |
| `VehiclePriceChangedEvent` | VehiclesSaleService | Re-analizar posición en mercado  |
| `VehicleSoldEvent`         | SalesService        | Actualizar modelos de predicción |
| `SearchPerformedEvent`     | SearchService       | Actualizar datos de demanda      |

---

## 🧠 Modelos de Machine Learning

### Modelo de Pricing

| Aspecto           | Detalle                                                                                                    |
| ----------------- | ---------------------------------------------------------------------------------------------------------- |
| **Nombre**        | `pricing-model-v2.3.1`                                                                                     |
| **Tipo**          | Gradient Boosting Regression                                                                               |
| **Features**      | Make, Model, Year, Mileage, Condition, FuelType, Transmission, Location, PhotoCount, ViewCount, DaysListed |
| **Target**        | Precio de venta final                                                                                      |
| **Entrenamiento** | Semanal con datos de ventas recientes                                                                      |
| **Accuracy**      | MAE: $45,000 RD, R²: 0.89                                                                                  |

### Modelo de Demanda

| Aspecto           | Detalle                                                                             |
| ----------------- | ----------------------------------------------------------------------------------- |
| **Nombre**        | `demand-model-v1.5.0`                                                               |
| **Tipo**          | Time Series + Classification                                                        |
| **Features**      | Search volume, Inventory levels, Historical sales, Seasonality, Economic indicators |
| **Target**        | DemandLevel (clasificación)                                                         |
| **Entrenamiento** | Diario                                                                              |
| **Accuracy**      | F1 Score: 0.87                                                                      |

### Modelo de Tiempo de Venta

| Aspecto           | Detalle                                |
| ----------------- | -------------------------------------- |
| **Nombre**        | `time-to-sale-v1.2.0`                  |
| **Tipo**          | Survival Analysis                      |
| **Features**      | All pricing features + Price vs Market |
| **Target**        | Days until sale                        |
| **Entrenamiento** | Semanal                                |
| **Accuracy**      | C-Index: 0.82                          |

---

## ⚠️ Reglas de Negocio

### Análisis de Precios

| #   | Regla                 | Descripción                                                     |
| --- | --------------------- | --------------------------------------------------------------- |
| 1   | Mínimo 3 comparables  | Se requieren mínimo 3 vehículos similares para generar análisis |
| 2   | Confidence mínimo 60% | Si confidence < 60%, mostrar warning al usuario                 |
| 3   | Rango de precio       | SuggestedPriceMax - SuggestedPriceMin <= 20% del precio         |
| 4   | Actualización semanal | Re-analizar automáticamente si análisis tiene > 7 días          |
| 5   | Datos frescos         | Solo usar comparables de los últimos 90 días                    |

### Predicción de Demanda

| #   | Regla                   | Descripción                                           |
| --- | ----------------------- | ----------------------------------------------------- |
| 1   | DemandScore actualizado | Recalcular cada 24 horas                              |
| 2   | Trending threshold      | Trend = Rising si score aumentó > 5% en 7 días        |
| 3   | Inventory ratio         | Low inventory + High searches = VeryHigh demand       |
| 4   | BuyRecommendation       | StrongBuy solo si DemandScore > 85 AND Trend = Rising |
| 5   | Insights máximo         | Generar máximo 5 insights por predicción              |

### Thresholds de DemandLevel

| DemandLevel | DemandScore | AvgDaysToSale |
| ----------- | ----------- | ------------- |
| VeryHigh    | 85-100      | < 15 días     |
| High        | 70-84       | 15-30 días    |
| Medium      | 50-69       | 30-60 días    |
| Low         | 30-49       | 60-90 días    |
| VeryLow     | 0-29        | > 90 días     |

---

## ❌ Códigos de Error

| Código   | HTTP Status | Mensaje                     | Causa                                     |
| -------- | ----------- | --------------------------- | ----------------------------------------- |
| `VI_001` | 400         | Validation failed           | Campos requeridos faltantes               |
| `VI_002` | 404         | No price analysis found     | No existe análisis para el vehículo       |
| `VI_003` | 404         | No demand prediction found  | No existe predicción para make/model/year |
| `VI_004` | 422         | Insufficient data           | Menos de 3 comparables disponibles        |
| `VI_005` | 503         | ML model unavailable        | Servicio de ML no disponible              |
| `VI_006` | 500         | Error generating suggestion | Error interno del modelo                  |

---

## ⚙️ Configuración del Servicio

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=postgres;Database=vehicle_intelligence_db;Username=postgres;Password=xxx"
  },
  "MLSettings": {
    "PricingModelPath": "/models/pricing-model-v2.3.1.onnx",
    "DemandModelPath": "/models/demand-model-v1.5.0.onnx",
    "TimeToSaleModelPath": "/models/time-to-sale-v1.2.0.onnx",
    "MinComparables": 3,
    "MaxComparableAge": 90,
    "MinConfidenceScore": 60,
    "ReCacheInterval": "24:00:00"
  },
  "DemandSettings": {
    "UpdateIntervalHours": 24,
    "TrendThresholdPercent": 5,
    "MaxInsights": 5
  },
  "ServiceUrls": {
    "VehiclesSaleService": "http://vehiclessaleservice:8080",
    "SearchService": "http://searchservice:8080"
  },
  "RabbitMQ": {
    "Host": "rabbitmq",
    "Port": 5672,
    "Exchange": "vehicleintelligence.events"
  },
  "Redis": {
    "ConnectionString": "redis:6379",
    "DemandCacheTTL": "01:00:00"
  }
}
```

---

## 🔒 Seguridad

### Autenticación y Autorización

| Endpoint                                       | Auth | Acceso                             |
| ---------------------------------------------- | ---- | ---------------------------------- |
| POST /api/pricing/analyze                      | ✅   | Dealer/Seller (owner del vehículo) |
| GET /api/pricing/vehicle/{id}/latest           | ✅   | Owner del vehículo                 |
| GET /api/pricing/{id}                          | ✅   | Owner del análisis                 |
| POST /api/vehicleintelligence/price-suggestion | ✅   | Cualquier usuario autenticado      |
| GET /api/vehicleintelligence/demand/categories | ✅   | Dealers                            |
| POST /api/demand/predict                       | ✅   | Dealers                            |
| GET /api/demand/{make}/{model}/{year}          | ❌   | Público                            |

---

## 📈 Métricas y Observabilidad

### Métricas Prometheus

| Métrica                          | Tipo      | Labels                 | Descripción                      |
| -------------------------------- | --------- | ---------------------- | -------------------------------- |
| `vi_price_analyses_total`        | Counter   | -                      | Análisis de precio realizados    |
| `vi_demand_predictions_total`    | Counter   | -                      | Predicciones de demanda          |
| `vi_model_inference_duration_ms` | Histogram | model_name             | Latencia de inferencia           |
| `vi_confidence_score_avg`        | Gauge     | -                      | Confidence score promedio        |
| `vi_comparables_found_avg`       | Gauge     | -                      | Comparables encontrados promedio |
| `vi_model_errors_total`          | Counter   | model_name, error_type | Errores de modelos ML            |

### Health Checks

| Check    | Descripción                 | Crítico |
| -------- | --------------------------- | ------- |
| Database | Conexión a PostgreSQL       | ✅      |
| Redis    | Conexión a cache            | ❌      |
| MLModels | Modelos cargados en memoria | ✅      |
| RabbitMQ | Conexión a message broker   | ❌      |

---

## 📚 Referencias

- [PricingController](../../backend/VehicleIntelligenceService/VehicleIntelligenceService.Api/Controllers/PricingController.cs)
- [DemandController](../../backend/VehicleIntelligenceService/VehicleIntelligenceService.Api/Controllers/DemandController.cs)
- [VehicleIntelligenceController](../../backend/VehicleIntelligenceService/VehicleIntelligenceService.Api/Controllers/VehicleIntelligenceController.cs)
- [PriceAnalysis Entity](../../backend/VehicleIntelligenceService/VehicleIntelligenceService.Domain/Entities/PriceAnalysis.cs)
- [DemandPrediction Entity](../../backend/VehicleIntelligenceService/VehicleIntelligenceService.Domain/Entities/DemandPrediction.cs)

---

**Última actualización:** Enero 21, 2026  
**Autor:** Sistema de Documentación Automatizado  
**Versión:** 1.0.0
