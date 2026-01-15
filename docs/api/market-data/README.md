# 📈 Market Data APIs

**Categoría:** Market Intelligence  
**APIs:** 2 (Marketcheck, vAuto)  
**Fase:** 2 (Core Features)  
**Impacto:** +50% pricing accuracy, -30% tiempo en mercado para listados bien precificados

---

## 📖 Resumen

Datos de mercado en tiempo real para análisis competitivo, tendencias de precios, y demand intelligence. Permite a dealers priciar correctamente y a compradores validar si un precio es justo.

### Casos de Uso en OKLA

✅ **"¿Es buen precio?"** - Badge verde/amarillo/rojo en cada listado según mercado  
✅ **Pricing recomendado** - Dealer ve "Precio sugerido: $1,850,000 - $1,950,000"  
✅ **Tendencia de precios** - Gráfico de 12 meses mostrando si precios suben o bajan  
✅ **Demand Index** - "Alta demanda" o "Baja demanda" para ese modelo en RD  
✅ **Competencia local** - "5 similares en venta en Santo Domingo desde $1.7M"  
✅ **Days on Market** - Promedio de días que tarda en venderse ese modelo  
✅ **Price to Market** - % arriba/debajo del promedio del mercado

---

## 🔗 Comparativa de APIs

| Aspecto              | **Marketcheck**        | **vAuto**          |
| -------------------- | ---------------------- | ------------------ |
| **Cobertura**        | USA + Internacional    | USA principalmente |
| **Datos históricos** | 5 años                 | 10 años            |
| **Actualización**    | Tiempo real            | Diaria             |
| **Precio**           | $100-500/mes           | $200-1,000/mes     |
| **API REST**         | ✅ Completa            | ✅ Completa        |
| **Datos RD**         | ⚠️ Limitado            | ❌ No disponible   |
| **Mejor para**       | Análisis internacional | Pricing USA        |
| **Recomendado**      | ⭐ PRINCIPAL           | Backup para USA    |

> **Nota:** Para RD, OKLA debe construir dataset propio basado en listados históricos.

---

## 📡 ENDPOINTS

### Marketcheck API

- `GET /search` - Buscar listados activos
- `GET /listing/{id}` - Detalle de listado
- `GET /stats` - Estadísticas de mercado
- `GET /price-analysis` - Análisis de precio
- `GET /market-trends` - Tendencias por marca/modelo
- `GET /demand-score` - Índice de demanda
- `GET /vin/{vin}/history` - Historial de precios del VIN
- `GET /dealer/{id}/inventory` - Inventario de dealer específico

### vAuto API

- `GET /appraisal` - Valoración de vehículo
- `GET /market-report` - Reporte de mercado
- `GET /price-to-market` - Comparación con mercado
- `GET /days-supply` - Días de inventario

---

## 💻 Backend Implementation (C#)

### Service Interface

```csharp
public interface IMarketDataService
{
    Task<MarketAnalysis> GetMarketAnalysisAsync(VehicleQuery query);
    Task<PricingRecommendation> GetPricingRecommendationAsync(string vin, int year, string make, string model, int mileage);
    Task<PriceTrend[]> GetPriceTrendsAsync(string make, string model, int months = 12);
    Task<DemandIndex> GetDemandIndexAsync(string make, string model, string region);
    Task<CompetitorListing[]> GetLocalCompetitorsAsync(string make, string model, string city, int radius = 50);
    Task<MarketStats> GetMarketStatsAsync(string make, string model, int year);
}

public class VehicleQuery
{
    public string Vin { get; set; }
    public string Make { get; set; }
    public string Model { get; set; }
    public int Year { get; set; }
    public int Mileage { get; set; }
    public string Trim { get; set; }
    public string ExteriorColor { get; set; }
    public string Region { get; set; }
}
```

### Domain Models

```csharp
public class MarketAnalysis
{
    public string Vin { get; set; }
    public string Make { get; set; }
    public string Model { get; set; }
    public int Year { get; set; }
    public PriceRange MarketPriceRange { get; set; }
    public decimal AveragePrice { get; set; }
    public decimal MedianPrice { get; set; }
    public decimal LowestPrice { get; set; }
    public decimal HighestPrice { get; set; }
    public int TotalListings { get; set; }
    public int AverageDaysOnMarket { get; set; }
    public DemandLevel DemandLevel { get; set; }
    public PriceTrend PriceTrend { get; set; }
    public DateTime AnalyzedAt { get; set; }
}

public class PriceRange
{
    public decimal Low { get; set; }       // Percentil 25
    public decimal Fair { get; set; }      // Mediana
    public decimal High { get; set; }      // Percentil 75
    public decimal Excellent { get; set; } // Percentil 90
}

public enum DemandLevel
{
    VeryLow,    // < 20 percentil
    Low,        // 20-40 percentil
    Moderate,   // 40-60 percentil
    High,       // 60-80 percentil
    VeryHigh    // > 80 percentil
}

public class PricingRecommendation
{
    public decimal RecommendedPrice { get; set; }
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
    public PriceRating CurrentPriceRating { get; set; } // Great, Good, Fair, High, Overpriced
    public string Explanation { get; set; }
    public int EstimatedDaysToSell { get; set; }
    public AdjustmentFactor[] Adjustments { get; set; }
}

public enum PriceRating
{
    GreatDeal,  // >15% debajo del mercado
    GoodDeal,   // 5-15% debajo
    FairPrice,  // +/- 5%
    SlightlyHigh, // 5-10% arriba
    Overpriced  // >10% arriba
}

public class AdjustmentFactor
{
    public string Factor { get; set; }      // "High mileage", "Premium color"
    public decimal Adjustment { get; set; } // -50,000 o +25,000
    public string Direction { get; set; }   // "increase" o "decrease"
}

public class PriceTrend
{
    public string Period { get; set; }      // "Jan 2025"
    public decimal AveragePrice { get; set; }
    public int TotalListings { get; set; }
    public decimal ChangePercent { get; set; }
}

public class DemandIndex
{
    public string Make { get; set; }
    public string Model { get; set; }
    public string Region { get; set; }
    public DemandLevel Level { get; set; }
    public int Score { get; set; }          // 0-100
    public int SearchVolume { get; set; }   // Búsquedas en OKLA
    public int ActiveListings { get; set; }
    public decimal SupplyDemandRatio { get; set; } // <1 = más demanda que oferta
    public string Insight { get; set; }     // "Alta demanda, pocos listados disponibles"
}

public class CompetitorListing
{
    public string ListingId { get; set; }
    public string DealerName { get; set; }
    public string City { get; set; }
    public decimal Price { get; set; }
    public int Mileage { get; set; }
    public string Trim { get; set; }
    public string ExteriorColor { get; set; }
    public int DaysOnMarket { get; set; }
    public string ListingUrl { get; set; }
}

public class MarketStats
{
    public string Make { get; set; }
    public string Model { get; set; }
    public int Year { get; set; }
    public decimal AveragePrice { get; set; }
    public decimal AverageMileage { get; set; }
    public int AverageDaysOnMarket { get; set; }
    public int TotalActiveListings { get; set; }
    public int SoldLast30Days { get; set; }
    public decimal SellThroughRate { get; set; } // % vendidos
    public string[] PopularColors { get; set; }
    public string[] PopularTrims { get; set; }
}
```

### Service Implementation

```csharp
public class MarketcheckMarketDataService : IMarketDataService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<MarketcheckMarketDataService> _logger;
    private readonly IMemoryCache _cache;
    private readonly string _apiKey;
    private const string BaseUrl = "https://api.marketcheck.com/v2";

    public MarketcheckMarketDataService(HttpClient httpClient, IConfiguration config, ILogger<MarketcheckMarketDataService> logger, IMemoryCache cache)
    {
        _httpClient = httpClient;
        _config = config;
        _logger = logger;
        _cache = cache;
        _apiKey = config["Marketcheck:ApiKey"];
    }

    public async Task<MarketAnalysis> GetMarketAnalysisAsync(VehicleQuery query)
    {
        var cacheKey = $"market-analysis-{query.Make}-{query.Model}-{query.Year}";

        if (_cache.TryGetValue(cacheKey, out MarketAnalysis cached))
            return cached;

        try
        {
            var url = $"{BaseUrl}/stats?api_key={_apiKey}&year={query.Year}&make={query.Make}&model={query.Model}";

            var response = await _httpClient.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var analysis = new MarketAnalysis
            {
                Make = query.Make,
                Model = query.Model,
                Year = query.Year,
                AveragePrice = root.GetProperty("mean_price").GetDecimal(),
                MedianPrice = root.GetProperty("median_price").GetDecimal(),
                LowestPrice = root.GetProperty("min_price").GetDecimal(),
                HighestPrice = root.GetProperty("max_price").GetDecimal(),
                TotalListings = root.GetProperty("count").GetInt32(),
                AverageDaysOnMarket = root.GetProperty("avg_dom").GetInt32(),
                MarketPriceRange = new PriceRange
                {
                    Low = root.GetProperty("price_25th").GetDecimal(),
                    Fair = root.GetProperty("median_price").GetDecimal(),
                    High = root.GetProperty("price_75th").GetDecimal(),
                    Excellent = root.GetProperty("price_90th").GetDecimal()
                },
                AnalyzedAt = DateTime.UtcNow
            };

            // Determinar nivel de demanda basado en supply/demand
            var supplyDays = root.GetProperty("days_supply").GetInt32();
            analysis.DemandLevel = supplyDays switch
            {
                < 20 => DemandLevel.VeryHigh,
                < 40 => DemandLevel.High,
                < 60 => DemandLevel.Moderate,
                < 90 => DemandLevel.Low,
                _ => DemandLevel.VeryLow
            };

            _cache.Set(cacheKey, analysis, TimeSpan.FromHours(1));
            return analysis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting market analysis from Marketcheck");
            throw;
        }
    }

    public async Task<PricingRecommendation> GetPricingRecommendationAsync(string vin, int year, string make, string model, int mileage)
    {
        var analysis = await GetMarketAnalysisAsync(new VehicleQuery { Year = year, Make = make, Model = model });

        // Ajustar por mileaje
        var avgMileageForYear = (DateTime.Now.Year - year) * 12000; // 12K/año promedio
        var mileageAdjustment = (avgMileageForYear - mileage) * 0.05m; // $0.05/milla diferencia

        var recommendedPrice = analysis.MedianPrice + mileageAdjustment;

        var adjustments = new List<AdjustmentFactor>();

        if (mileage > avgMileageForYear)
        {
            adjustments.Add(new AdjustmentFactor
            {
                Factor = "Alto kilometraje",
                Adjustment = mileageAdjustment,
                Direction = "decrease"
            });
        }
        else if (mileage < avgMileageForYear * 0.7m)
        {
            adjustments.Add(new AdjustmentFactor
            {
                Factor = "Bajo kilometraje",
                Adjustment = mileageAdjustment,
                Direction = "increase"
            });
        }

        return new PricingRecommendation
        {
            RecommendedPrice = recommendedPrice,
            MinPrice = analysis.MarketPriceRange.Low,
            MaxPrice = analysis.MarketPriceRange.High,
            CurrentPriceRating = PriceRating.FairPrice,
            Explanation = $"Basado en {analysis.TotalListings} listados similares con promedio de {analysis.AverageDaysOnMarket} días en mercado.",
            EstimatedDaysToSell = analysis.AverageDaysOnMarket,
            Adjustments = adjustments.ToArray()
        };
    }

    public async Task<PriceTrend[]> GetPriceTrendsAsync(string make, string model, int months = 12)
    {
        var url = $"{BaseUrl}/market-trends?api_key={_apiKey}&make={make}&model={model}&months={months}";

        var response = await _httpClient.GetAsync(url);
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        var trends = new List<PriceTrend>();
        foreach (var item in doc.RootElement.GetProperty("trends").EnumerateArray())
        {
            trends.Add(new PriceTrend
            {
                Period = item.GetProperty("period").GetString(),
                AveragePrice = item.GetProperty("avg_price").GetDecimal(),
                TotalListings = item.GetProperty("count").GetInt32(),
                ChangePercent = item.GetProperty("change_pct").GetDecimal()
            });
        }

        return trends.ToArray();
    }

    public async Task<DemandIndex> GetDemandIndexAsync(string make, string model, string region)
    {
        var url = $"{BaseUrl}/demand-score?api_key={_apiKey}&make={make}&model={model}&region={region}";

        var response = await _httpClient.GetAsync(url);
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var score = root.GetProperty("demand_score").GetInt32();

        return new DemandIndex
        {
            Make = make,
            Model = model,
            Region = region,
            Score = score,
            Level = score switch
            {
                >= 80 => DemandLevel.VeryHigh,
                >= 60 => DemandLevel.High,
                >= 40 => DemandLevel.Moderate,
                >= 20 => DemandLevel.Low,
                _ => DemandLevel.VeryLow
            },
            SearchVolume = root.GetProperty("search_volume").GetInt32(),
            ActiveListings = root.GetProperty("active_listings").GetInt32(),
            SupplyDemandRatio = root.GetProperty("supply_demand_ratio").GetDecimal(),
            Insight = root.GetProperty("insight").GetString()
        };
    }

    public async Task<CompetitorListing[]> GetLocalCompetitorsAsync(string make, string model, string city, int radius = 50)
    {
        var url = $"{BaseUrl}/search?api_key={_apiKey}&make={make}&model={model}&city={city}&radius={radius}";

        var response = await _httpClient.GetAsync(url);
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        var listings = new List<CompetitorListing>();
        foreach (var item in doc.RootElement.GetProperty("listings").EnumerateArray())
        {
            listings.Add(new CompetitorListing
            {
                ListingId = item.GetProperty("id").GetString(),
                DealerName = item.GetProperty("dealer").GetProperty("name").GetString(),
                City = item.GetProperty("city").GetString(),
                Price = item.GetProperty("price").GetDecimal(),
                Mileage = item.GetProperty("miles").GetInt32(),
                Trim = item.GetProperty("trim").GetString(),
                ExteriorColor = item.GetProperty("exterior_color").GetString(),
                DaysOnMarket = item.GetProperty("dom").GetInt32(),
                ListingUrl = item.GetProperty("vdp_url").GetString()
            });
        }

        return listings.OrderBy(l => l.Price).ToArray();
    }
}
```

---

## 🎨 Frontend Implementation (React + TypeScript)

### Market Data Service

```typescript
import axios from "axios";

export interface MarketAnalysis {
  make: string;
  model: string;
  year: number;
  averagePrice: number;
  medianPrice: number;
  totalListings: number;
  averageDaysOnMarket: number;
  demandLevel: "VeryLow" | "Low" | "Moderate" | "High" | "VeryHigh";
  marketPriceRange: {
    low: number;
    fair: number;
    high: number;
    excellent: number;
  };
}

export interface PricingRecommendation {
  recommendedPrice: number;
  minPrice: number;
  maxPrice: number;
  currentPriceRating: string;
  explanation: string;
  estimatedDaysToSell: number;
}

export class MarketDataService {
  private baseUrl = process.env.REACT_APP_API_URL;

  async getMarketAnalysis(
    make: string,
    model: string,
    year: number
  ): Promise<MarketAnalysis> {
    const response = await axios.get(
      `${this.baseUrl}/api/market-data/analysis`,
      {
        params: { make, model, year },
      }
    );
    return response.data;
  }

  async getPricingRecommendation(
    vin: string,
    mileage: number
  ): Promise<PricingRecommendation> {
    const response = await axios.get(
      `${this.baseUrl}/api/market-data/pricing`,
      {
        params: { vin, mileage },
      }
    );
    return response.data;
  }

  async getPriceTrends(make: string, model: string): Promise<PriceTrend[]> {
    const response = await axios.get(`${this.baseUrl}/api/market-data/trends`, {
      params: { make, model },
    });
    return response.data;
  }
}
```

### React Component - Price Rating Badge

```typescript
import React from "react";
import { useQuery } from "@tanstack/react-query";
import { MarketDataService } from "@/services/marketDataService";
import { TrendingUp, TrendingDown, Minus } from "lucide-react";

interface Props {
  price: number;
  make: string;
  model: string;
  year: number;
}

export const PriceRatingBadge = ({ price, make, model, year }: Props) => {
  const marketDataService = new MarketDataService();

  const { data: analysis, isLoading } = useQuery({
    queryKey: ["market-analysis", make, model, year],
    queryFn: () => marketDataService.getMarketAnalysis(make, model, year),
  });

  if (isLoading || !analysis) return null;

  const { marketPriceRange } = analysis;

  let rating: "great" | "good" | "fair" | "high" | "overpriced";
  let label: string;
  let bgColor: string;
  let Icon: typeof TrendingDown;

  if (price <= marketPriceRange.low) {
    rating = "great";
    label = "Gran Precio";
    bgColor = "bg-green-600";
    Icon = TrendingDown;
  } else if (price <= marketPriceRange.fair) {
    rating = "good";
    label = "Buen Precio";
    bgColor = "bg-green-500";
    Icon = TrendingDown;
  } else if (price <= marketPriceRange.high) {
    rating = "fair";
    label = "Precio Justo";
    bgColor = "bg-yellow-500";
    Icon = Minus;
  } else if (price <= marketPriceRange.excellent * 1.1) {
    rating = "high";
    label = "Algo Alto";
    bgColor = "bg-orange-500";
    Icon = TrendingUp;
  } else {
    rating = "overpriced";
    label = "Sobrepreciado";
    bgColor = "bg-red-500";
    Icon = TrendingUp;
  }

  const percentDiff = (
    ((price - marketPriceRange.fair) / marketPriceRange.fair) *
    100
  ).toFixed(0);
  const diffLabel =
    price > marketPriceRange.fair
      ? `+${percentDiff}% vs mercado`
      : `${percentDiff}% vs mercado`;

  return (
    <div
      className={`${bgColor} text-white px-3 py-1.5 rounded-lg inline-flex items-center gap-2`}
    >
      <Icon className="h-4 w-4" />
      <span className="font-semibold">{label}</span>
      <span className="text-xs opacity-80">({diffLabel})</span>
    </div>
  );
};
```

### React Component - Price Trend Chart

```typescript
import React from "react";
import { useQuery } from "@tanstack/react-query";
import { MarketDataService } from "@/services/marketDataService";
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  Tooltip,
  ResponsiveContainer,
} from "recharts";

interface Props {
  make: string;
  model: string;
}

export const PriceTrendChart = ({ make, model }: Props) => {
  const marketDataService = new MarketDataService();

  const { data: trends, isLoading } = useQuery({
    queryKey: ["price-trends", make, model],
    queryFn: () => marketDataService.getPriceTrends(make, model),
  });

  if (isLoading)
    return <div className="h-64 bg-gray-100 animate-pulse rounded-xl"></div>;
  if (!trends?.length) return null;

  const formatPrice = (value: number) => `$${(value / 1000).toFixed(0)}K`;

  const latestChange = trends[trends.length - 1]?.changePercent || 0;
  const trendDirection =
    latestChange > 0 ? "subiendo" : latestChange < 0 ? "bajando" : "estable";
  const trendColor =
    latestChange > 0
      ? "text-red-600"
      : latestChange < 0
      ? "text-green-600"
      : "text-gray-600";

  return (
    <div className="bg-white p-6 rounded-xl shadow-sm">
      <div className="flex justify-between items-center mb-4">
        <h3 className="text-lg font-bold">Tendencia de Precios</h3>
        <span className={`text-sm font-medium ${trendColor}`}>
          Precios {trendDirection} ({latestChange > 0 ? "+" : ""}
          {latestChange.toFixed(1)}% último mes)
        </span>
      </div>

      <ResponsiveContainer width="100%" height={200}>
        <LineChart data={trends}>
          <XAxis dataKey="period" tick={{ fontSize: 12 }} />
          <YAxis tickFormatter={formatPrice} tick={{ fontSize: 12 }} />
          <Tooltip
            formatter={(value: number) => [
              `$${value.toLocaleString()}`,
              "Precio Promedio",
            ]}
            labelFormatter={(label) => `${label}`}
          />
          <Line
            type="monotone"
            dataKey="averagePrice"
            stroke="#2563eb"
            strokeWidth={2}
            dot={{ r: 3 }}
          />
        </LineChart>
      </ResponsiveContainer>

      <p className="text-xs text-gray-500 mt-2">
        Basado en{" "}
        {trends.reduce((sum, t) => sum + t.totalListings, 0).toLocaleString()}{" "}
        listados analizados
      </p>
    </div>
  );
};
```

---

## ✅ Testing (xUnit)

```csharp
public class MarketcheckMarketDataServiceTests
{
    [Fact]
    public async Task GetMarketAnalysisAsync_WithValidQuery_ReturnsAnalysis()
    {
        var query = new VehicleQuery { Make = "Toyota", Model = "Corolla", Year = 2020 };

        var analysis = await _service.GetMarketAnalysisAsync(query);

        Assert.NotNull(analysis);
        Assert.True(analysis.AveragePrice > 0);
        Assert.True(analysis.TotalListings > 0);
    }

    [Fact]
    public async Task GetPricingRecommendationAsync_ReturnsRecommendation()
    {
        var recommendation = await _service.GetPricingRecommendationAsync("1HGBH41JXMN109186", 2020, "Honda", "Civic", 35000);

        Assert.NotNull(recommendation);
        Assert.True(recommendation.RecommendedPrice > 0);
        Assert.True(recommendation.MinPrice < recommendation.MaxPrice);
    }

    [Fact]
    public async Task GetPriceTrendsAsync_Returns12MonthsTrends()
    {
        var trends = await _service.GetPriceTrendsAsync("Toyota", "Camry");

        Assert.NotEmpty(trends);
        Assert.True(trends.Length >= 6);
    }

    [Fact]
    public async Task GetDemandIndexAsync_ReturnsValidScore()
    {
        var demand = await _service.GetDemandIndexAsync("Honda", "CR-V", "Santo Domingo");

        Assert.NotNull(demand);
        Assert.True(demand.Score >= 0 && demand.Score <= 100);
    }
}
```

---

## 🔧 Troubleshooting

| Problema                 | Causa                        | Solución                                     |
| ------------------------ | ---------------------------- | -------------------------------------------- |
| Datos desactualizados    | API no tiene data reciente   | Cache con TTL corto + fallback a data propia |
| Make/Model no encontrado | Nomenclatura diferente       | Mapeo de nombres (ej: "Chevy" → "Chevrolet") |
| Precios muy diferentes   | Región diferente (USA vs RD) | Ajustar por factor regional                  |
| Rate limit excedido      | Muchas requests              | Implementar cache agresivo (1 hora mínimo)   |
| Demand index incorrecto  | Pocos datos para esa región  | Usar data nacional como fallback             |
| Gráfico vacío            | No hay data histórica        | Mostrar mensaje "Datos insuficientes"        |

---

## 🔗 Integración con OKLA

### 1. **Crear MarketDataService**

```csharp
services.AddHttpClient<IMarketDataService, MarketcheckMarketDataService>();
services.AddMemoryCache(); // Para caching
```

### 2. **Gateway routing**

```json
{
  "UpstreamPathTemplate": "/api/market-data/{everything}",
  "DownstreamPathTemplate": "/api/market-data/{everything}",
  "DownstreamHostAndPorts": [{ "Host": "marketdataservice", "Port": 8080 }]
}
```

### 3. **Badge en VehicleCard**

```tsx
<PriceRatingBadge
  price={vehicle.price}
  make={vehicle.make}
  model={vehicle.model}
  year={vehicle.year}
/>
```

### 4. **Chart en VehicleDetailPage**

```tsx
<PriceTrendChart make={vehicle.make} model={vehicle.model} />
```

### 5. **En Dealer Dashboard**

```tsx
<DealerPricingAssistant vehicles={dealerInventory} />
```

---

## 💰 Costos Estimados

| API         | Plan           | Costo/mes | Queries/mes |
| ----------- | -------------- | --------- | ----------- |
| Marketcheck | Starter        | $100      | 5,000       |
| Marketcheck | Pro            | $500      | 50,000      |
| vAuto       | Basic (backup) | $200      | 10,000      |
| **TOTAL**   |                | **$300**  | 15,000      |

✅ **ROI:** Listados bien precificados se venden 30% más rápido = más transacciones = más revenue.
