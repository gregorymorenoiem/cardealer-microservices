# 📊 ONE - Oficina Nacional de Estadística API

**Proveedor:** Oficina Nacional de Estadística (ONE)  
**Website:** [one.gob.do](https://one.gob.do)  
**Uso:** Datos demográficos, estadísticas de mercado, precios  
**Prioridad:** ⭐⭐⭐ MEDIA (Inteligencia de mercado)

---

## 📋 Información General

| Campo | Valor |
|-------|-------|
| **Institución** | ONE - República Dominicana |
| **Tipo de Datos** | Públicos / OpenData |
| **Formato** | CSV, JSON, Excel |
| **Costo** | GRATUITO |
| **Actualización** | Mensual/Trimestral/Anual |

---

## 📈 Datasets Relevantes para OKLA

| Dataset | Frecuencia | Uso en Marketplace |
|---------|------------|-------------------|
| **IPC Vehículos** | Mensual | Índice de precios de vehículos |
| **Importaciones Vehículos** | Trimestral | Tendencias de mercado |
| **Parque Vehicular** | Anual | Tamaño del mercado |
| **Población por Provincia** | Anual | Segmentación geográfica |
| **Ingresos Hogares** | Anual | Poder adquisitivo |
| **Tasa de Cambio** | Diario | Conversión USD/DOP |
| **Desempleo** | Trimestral | Indicador económico |

---

## 🌐 API Endpoints / Fuentes

### Portal OpenData ONE

```http
# Base URL
https://datos.one.gob.do/api/3/action/

# Buscar datasets
GET /package_search?q=vehiculos

# Response
{
  "success": true,
  "result": {
    "count": 15,
    "results": [
      {
        "id": "parque-vehicular-2024",
        "title": "Parque Vehicular por Tipo y Provincia",
        "notes": "Cantidad de vehículos registrados...",
        "resources": [
          {
            "id": "res-001",
            "name": "Parque Vehicular 2024.csv",
            "format": "CSV",
            "url": "https://datos.one.gob.do/dataset/xxx/parque_vehicular_2024.csv"
          }
        ]
      }
    ]
  }
}

# Obtener recurso específico
GET /resource_show?id=res-001

# Descargar datos
GET /datastore_search?resource_id=res-001&limit=100

# Response
{
  "success": true,
  "result": {
    "records": [
      {
        "provincia": "Distrito Nacional",
        "tipo_vehiculo": "Automóvil",
        "cantidad": 245678,
        "año": 2024
      }
    ],
    "total": 32
  }
}
```

### IPC - Índice de Precios al Consumidor

```http
# Datos mensuales IPC (Web Scraping desde ONE)
# URL: https://one.gob.do/datos-y-estadisticas/temas/precios/ipc

# Estructura de datos extraídos
{
  "fecha": "2026-01",
  "ipc_general": 148.52,
  "ipc_transporte": 152.34,
  "ipc_vehiculos": 145.67,
  "variacion_mensual": 0.35,
  "variacion_anual": 3.87,
  "variacion_acumulada": 0.35
}
```

### Importaciones de Vehículos (DGA vía ONE)

```http
# Datos trimestrales de importaciones
# Fuente: Dirección General de Aduanas / ONE

{
  "trimestre": "2025-Q4",
  "importaciones_vehiculos": {
    "total_unidades": 28450,
    "por_tipo": {
      "automoviles": 18200,
      "jeepetas_suv": 6500,
      "camionetas": 2800,
      "motocicletas": 950
    },
    "por_origen": {
      "estados_unidos": 12500,
      "japon": 8200,
      "corea_sur": 4100,
      "china": 2650,
      "otros": 1000
    },
    "por_condicion": {
      "nuevos": 9800,
      "usados": 18650
    },
    "valor_cif_usd": 385000000
  }
}
```

### Parque Vehicular por Provincia

```http
# Datos anuales del parque vehicular
{
  "año": 2024,
  "total_vehiculos": 4250000,
  "por_provincia": [
    { "provincia": "Distrito Nacional", "cantidad": 845000, "porcentaje": 19.88 },
    { "provincia": "Santo Domingo", "cantidad": 1250000, "porcentaje": 29.41 },
    { "provincia": "Santiago", "cantidad": 385000, "porcentaje": 9.06 },
    { "provincia": "La Vega", "cantidad": 125000, "porcentaje": 2.94 },
    { "provincia": "San Cristóbal", "cantidad": 115000, "porcentaje": 2.71 }
  ],
  "por_tipo": [
    { "tipo": "Automóviles", "cantidad": 2100000, "porcentaje": 49.41 },
    { "tipo": "Jeepetas/SUV", "cantidad": 850000, "porcentaje": 20.00 },
    { "tipo": "Camionetas", "cantidad": 520000, "porcentaje": 12.24 },
    { "tipo": "Motocicletas", "cantidad": 680000, "porcentaje": 16.00 },
    { "tipo": "Otros", "cantidad": 100000, "porcentaje": 2.35 }
  ]
}
```

---

## 💻 Modelos C#

```csharp
namespace DataService.Domain.Entities;

/// <summary>
/// Índice de precios al consumidor
/// </summary>
public record ConsumerPriceIndex(
    int Year,
    int Month,
    decimal GeneralIndex,
    decimal TransportIndex,
    decimal VehicleIndex,
    decimal MonthlyVariation,
    decimal AnnualVariation,
    decimal AccumulatedVariation,
    DateTime UpdatedAt
);

/// <summary>
/// Estadísticas de importación de vehículos
/// </summary>
public record VehicleImportStats(
    int Year,
    int Quarter,
    int TotalUnits,
    Dictionary<string, int> ByType,
    Dictionary<string, int> ByOrigin,
    int NewVehicles,
    int UsedVehicles,
    decimal TotalValueUsd,
    DateTime UpdatedAt
);

/// <summary>
/// Parque vehicular por provincia
/// </summary>
public record VehicleFleetStats(
    int Year,
    int TotalVehicles,
    List<ProvinceVehicleCount> ByProvince,
    List<VehicleTypeCount> ByType,
    decimal GrowthRate,
    DateTime UpdatedAt
);

public record ProvinceVehicleCount(
    string ProvinceCode,
    string ProvinceName,
    int Count,
    decimal Percentage,
    int? PreviousYearCount,
    decimal? GrowthRate
);

public record VehicleTypeCount(
    string Type,
    int Count,
    decimal Percentage
);

/// <summary>
/// Demografía provincial
/// </summary>
public record ProvinceDemographics(
    string ProvinceCode,
    string ProvinceName,
    int Population,
    decimal MedianIncome,
    decimal UnemploymentRate,
    int TotalHouseholds,
    int VehiclesPerThousand,
    decimal UrbanizationRate,
    int Year
);

/// <summary>
/// Tasa de cambio
/// </summary>
public record ExchangeRate(
    DateTime Date,
    decimal BuyRate,
    decimal SellRate,
    decimal AverageRate,
    string Currency
);

/// <summary>
/// Índice de mercado de vehículos (agregado)
/// </summary>
public record VehicleMarketIndex(
    DateTime Date,
    decimal PriceIndex,
    decimal DemandIndex,
    decimal SupplyIndex,
    decimal AffordabilityIndex,
    string Trend,
    List<string> Insights
);
```

---

## 🔧 Service Interface

```csharp
namespace DataService.Domain.Interfaces;

public interface IMarketDataService
{
    /// <summary>
    /// Obtiene IPC actual y histórico
    /// </summary>
    Task<List<ConsumerPriceIndex>> GetCpiHistoryAsync(
        int months = 12);

    /// <summary>
    /// Obtiene estadísticas de importación
    /// </summary>
    Task<VehicleImportStats?> GetLatestImportStatsAsync();

    /// <summary>
    /// Obtiene parque vehicular por provincia
    /// </summary>
    Task<VehicleFleetStats?> GetVehicleFleetStatsAsync(int? year = null);

    /// <summary>
    /// Obtiene demografía provincial
    /// </summary>
    Task<List<ProvinceDemographics>> GetProvinceDemographicsAsync();

    /// <summary>
    /// Obtiene tasa de cambio actual
    /// </summary>
    Task<ExchangeRate?> GetCurrentExchangeRateAsync(
        string currency = "USD");

    /// <summary>
    /// Obtiene historial de tasas de cambio
    /// </summary>
    Task<List<ExchangeRate>> GetExchangeRateHistoryAsync(
        string currency = "USD",
        int days = 30);

    /// <summary>
    /// Calcula índice de mercado de vehículos
    /// </summary>
    Task<VehicleMarketIndex> CalculateMarketIndexAsync();

    /// <summary>
    /// Obtiene provincias con mayor demanda
    /// </summary>
    Task<List<ProvinceMarketDemand>> GetTopDemandProvincesAsync(
        int top = 10);

    /// <summary>
    /// Ajusta precio por inflación
    /// </summary>
    Task<decimal> AdjustPriceForInflationAsync(
        decimal price,
        DateTime fromDate,
        DateTime toDate);
}

public record ProvinceMarketDemand(
    string ProvinceCode,
    string ProvinceName,
    int Population,
    int VehicleCount,
    decimal VehiclesPerCapita,
    decimal EstimatedDemand,
    string DemandLevel // Alto, Medio, Bajo
);
```

---

## 🏗️ Service Implementation

```csharp
namespace DataService.Infrastructure.Services;

public class OneMarketDataService : IMarketDataService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OneMarketDataService> _logger;
    private readonly IMemoryCache _cache;
    private readonly IMarketDataRepository _repository;

    private const string ONE_DATA_URL = "https://datos.one.gob.do/api/3/action/";

    public OneMarketDataService(
        HttpClient httpClient,
        ILogger<OneMarketDataService> logger,
        IMemoryCache cache,
        IMarketDataRepository repository)
    {
        _httpClient = httpClient;
        _logger = logger;
        _cache = cache;
        _repository = repository;

        _httpClient.BaseAddress = new Uri(ONE_DATA_URL);
    }

    public async Task<List<ConsumerPriceIndex>> GetCpiHistoryAsync(int months = 12)
    {
        var cacheKey = $"cpi_history_{months}";
        
        if (_cache.TryGetValue(cacheKey, out List<ConsumerPriceIndex>? cached))
            return cached!;

        // Intentar obtener de base de datos primero
        var history = await _repository.GetCpiHistoryAsync(months);
        
        if (!history.Any() || history.First().UpdatedAt < DateTime.UtcNow.AddDays(-1))
        {
            // Actualizar desde ONE
            history = await FetchCpiFromOneAsync(months);
            await _repository.SaveCpiHistoryAsync(history);
        }

        _cache.Set(cacheKey, history, TimeSpan.FromHours(24));
        return history;
    }

    private async Task<List<ConsumerPriceIndex>> FetchCpiFromOneAsync(int months)
    {
        // Web scraping de ONE (los datos IPC no están en API)
        var url = "https://one.gob.do/datos-y-estadisticas/temas/precios/ipc";
        
        try
        {
            var html = await _httpClient.GetStringAsync(url);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var result = new List<ConsumerPriceIndex>();
            
            // Parsear tabla de IPC
            var table = doc.DocumentNode.SelectSingleNode("//table[@class='ipc-table']");
            if (table == null)
            {
                _logger.LogWarning("IPC table not found on ONE website");
                return result;
            }

            var rows = table.SelectNodes(".//tr").Skip(1); // Skip header
            foreach (var row in rows.Take(months))
            {
                var cells = row.SelectNodes(".//td");
                if (cells?.Count >= 6)
                {
                    var dateText = cells[0].InnerText.Trim();
                    var parts = dateText.Split('/');
                    
                    result.Add(new ConsumerPriceIndex(
                        Year: int.Parse(parts[1]),
                        Month: int.Parse(parts[0]),
                        GeneralIndex: ParseDecimal(cells[1].InnerText),
                        TransportIndex: ParseDecimal(cells[2].InnerText),
                        VehicleIndex: ParseDecimal(cells[3].InnerText),
                        MonthlyVariation: ParseDecimal(cells[4].InnerText),
                        AnnualVariation: ParseDecimal(cells[5].InnerText),
                        AccumulatedVariation: cells.Count > 6 ? ParseDecimal(cells[6].InnerText) : 0,
                        UpdatedAt: DateTime.UtcNow
                    ));
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch CPI from ONE");
            return new List<ConsumerPriceIndex>();
        }
    }

    public async Task<VehicleImportStats?> GetLatestImportStatsAsync()
    {
        var cacheKey = "vehicle_import_stats";
        
        if (_cache.TryGetValue(cacheKey, out VehicleImportStats? cached))
            return cached;

        var stats = await _repository.GetLatestImportStatsAsync();
        
        if (stats != null)
        {
            _cache.Set(cacheKey, stats, TimeSpan.FromDays(7));
        }

        return stats;
    }

    public async Task<VehicleFleetStats?> GetVehicleFleetStatsAsync(int? year = null)
    {
        year ??= DateTime.Now.Year - 1; // Año anterior (más reciente disponible)
        var cacheKey = $"vehicle_fleet_{year}";
        
        if (_cache.TryGetValue(cacheKey, out VehicleFleetStats? cached))
            return cached;

        // Consultar dataset de ONE
        var resourceId = "parque-vehicular-provincial"; // ID del recurso
        var url = $"datastore_search?resource_id={resourceId}&filters={{\"año\":{year}}}";

        try
        {
            var response = await _httpClient.GetFromJsonAsync<OneDataResponse>(url);
            
            if (response?.Success != true)
                return null;

            var records = response.Result.Records;
            var byProvince = records
                .GroupBy(r => r.Provincia)
                .Select(g => new ProvinceVehicleCount(
                    ProvinceCode: GetProvinceCode(g.Key),
                    ProvinceName: g.Key,
                    Count: g.Sum(r => r.Cantidad),
                    Percentage: 0, // Calcular después
                    PreviousYearCount: null,
                    GrowthRate: null
                ))
                .ToList();

            var total = byProvince.Sum(p => p.Count);
            
            // Calcular porcentajes
            byProvince = byProvince.Select(p => p with
            {
                Percentage = Math.Round((decimal)p.Count / total * 100, 2)
            }).OrderByDescending(p => p.Count).ToList();

            var byType = records
                .GroupBy(r => r.TipoVehiculo)
                .Select(g => new VehicleTypeCount(
                    Type: g.Key,
                    Count: g.Sum(r => r.Cantidad),
                    Percentage: Math.Round((decimal)g.Sum(r => r.Cantidad) / total * 100, 2)
                ))
                .OrderByDescending(t => t.Count)
                .ToList();

            var stats = new VehicleFleetStats(
                Year: year.Value,
                TotalVehicles: total,
                ByProvince: byProvince,
                ByType: byType,
                GrowthRate: 3.5m, // TODO: Calcular vs año anterior
                UpdatedAt: DateTime.UtcNow
            );

            _cache.Set(cacheKey, stats, TimeSpan.FromDays(30));
            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch vehicle fleet stats from ONE");
            return await _repository.GetVehicleFleetStatsAsync(year.Value);
        }
    }

    public async Task<List<ProvinceDemographics>> GetProvinceDemographicsAsync()
    {
        var cacheKey = "province_demographics";
        
        if (_cache.TryGetValue(cacheKey, out List<ProvinceDemographics>? cached))
            return cached!;

        var demographics = await _repository.GetProvinceDemographicsAsync();
        
        if (demographics.Any())
        {
            _cache.Set(cacheKey, demographics, TimeSpan.FromDays(90));
        }

        return demographics;
    }

    public async Task<ExchangeRate?> GetCurrentExchangeRateAsync(string currency = "USD")
    {
        var cacheKey = $"exchange_rate_{currency}";
        
        if (_cache.TryGetValue(cacheKey, out ExchangeRate? cached))
            return cached;

        // Obtener de Banco Central RD (más preciso que ONE)
        try
        {
            var url = "https://api.bancentral.gov.do/api/tasas/ultimatasa";
            var response = await _httpClient.GetFromJsonAsync<BancoCentralResponse>(url);

            if (response == null)
                return null;

            var rate = new ExchangeRate(
                Date: DateTime.Today,
                BuyRate: response.TasaCompra,
                SellRate: response.TasaVenta,
                AverageRate: (response.TasaCompra + response.TasaVenta) / 2,
                Currency: currency
            );

            _cache.Set(cacheKey, rate, TimeSpan.FromHours(4));
            return rate;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch exchange rate");
            return await _repository.GetLatestExchangeRateAsync(currency);
        }
    }

    public async Task<List<ExchangeRate>> GetExchangeRateHistoryAsync(
        string currency = "USD",
        int days = 30)
    {
        return await _repository.GetExchangeRateHistoryAsync(currency, days);
    }

    public async Task<VehicleMarketIndex> CalculateMarketIndexAsync()
    {
        var cacheKey = "vehicle_market_index";
        
        if (_cache.TryGetValue(cacheKey, out VehicleMarketIndex? cached))
            return cached!;

        // Obtener datos para cálculo
        var cpi = await GetCpiHistoryAsync(12);
        var imports = await GetLatestImportStatsAsync();
        var exchangeRate = await GetCurrentExchangeRateAsync();
        var fleet = await GetVehicleFleetStatsAsync();

        // Calcular índices
        var latestCpi = cpi.FirstOrDefault();
        var priceIndex = latestCpi?.VehicleIndex ?? 100m;
        
        // Índice de demanda (basado en importaciones y crecimiento parque)
        var demandIndex = 100m;
        if (imports != null)
        {
            demandIndex = (imports.TotalUnits / 25000m) * 100; // Baseline 25K trimestral
        }

        // Índice de oferta (basado en publicaciones y inventario)
        var supplyIndex = 100m; // TODO: Integrar con nuestros datos

        // Índice de accesibilidad (inverso a tasa de cambio y IPC)
        var affordabilityIndex = 100m;
        if (exchangeRate != null && latestCpi != null)
        {
            var baselineRate = 55m; // Tasa histórica promedio
            var baselineCpi = 140m;
            affordabilityIndex = (baselineRate / exchangeRate.AverageRate) * 
                                 (baselineCpi / latestCpi.GeneralIndex) * 100;
        }

        // Determinar tendencia
        var trend = "Estable";
        if (cpi.Count >= 3)
        {
            var avgVariation = cpi.Take(3).Average(c => c.MonthlyVariation);
            if (avgVariation > 0.5m) trend = "Alcista";
            else if (avgVariation < -0.3m) trend = "Bajista";
        }

        // Generar insights
        var insights = new List<string>();
        
        if (latestCpi?.AnnualVariation > 5)
            insights.Add($"Inflación anual de vehículos en {latestCpi.AnnualVariation:F1}% - Precios en aumento");
        
        if (exchangeRate?.AverageRate > 58)
            insights.Add($"Tasa de cambio alta (RD${exchangeRate.AverageRate:F2}/USD) - Importados más caros");
        
        if (imports?.UsedVehicles > imports?.NewVehicles * 1.5m)
            insights.Add("Alta demanda de vehículos usados vs nuevos");

        var index = new VehicleMarketIndex(
            Date: DateTime.UtcNow,
            PriceIndex: Math.Round(priceIndex, 2),
            DemandIndex: Math.Round(demandIndex, 2),
            SupplyIndex: Math.Round(supplyIndex, 2),
            AffordabilityIndex: Math.Round(affordabilityIndex, 2),
            Trend: trend,
            Insights: insights
        );

        _cache.Set(cacheKey, index, TimeSpan.FromHours(12));
        return index;
    }

    public async Task<List<ProvinceMarketDemand>> GetTopDemandProvincesAsync(int top = 10)
    {
        var demographics = await GetProvinceDemographicsAsync();
        var fleet = await GetVehicleFleetStatsAsync();

        if (!demographics.Any() || fleet == null)
            return new List<ProvinceMarketDemand>();

        return demographics
            .Select(d =>
            {
                var vehicleCount = fleet.ByProvince
                    .FirstOrDefault(p => p.ProvinceCode == d.ProvinceCode)?.Count ?? 0;
                
                var vehiclesPerCapita = d.Population > 0 
                    ? (decimal)vehicleCount / d.Population 
                    : 0;

                // Estimar demanda basada en:
                // - Población
                // - Ingreso mediano
                // - Vehículos per cápita actual (bajo = más demanda potencial)
                var demandScore = (d.Population / 100000m) * 
                                  (d.MedianIncome / 30000m) * 
                                  (1 - Math.Min(vehiclesPerCapita * 5, 0.9m));

                return new ProvinceMarketDemand(
                    ProvinceCode: d.ProvinceCode,
                    ProvinceName: d.ProvinceName,
                    Population: d.Population,
                    VehicleCount: vehicleCount,
                    VehiclesPerCapita: Math.Round(vehiclesPerCapita * 1000, 2),
                    EstimatedDemand: Math.Round(demandScore, 2),
                    DemandLevel: demandScore > 50 ? "Alto" : 
                                 demandScore > 25 ? "Medio" : "Bajo"
                );
            })
            .OrderByDescending(p => p.EstimatedDemand)
            .Take(top)
            .ToList();
    }

    public async Task<decimal> AdjustPriceForInflationAsync(
        decimal price,
        DateTime fromDate,
        DateTime toDate)
    {
        var cpiHistory = await GetCpiHistoryAsync(24);
        
        var fromCpi = cpiHistory
            .FirstOrDefault(c => c.Year == fromDate.Year && c.Month == fromDate.Month);
        var toCpi = cpiHistory
            .FirstOrDefault(c => c.Year == toDate.Year && c.Month == toDate.Month);

        if (fromCpi == null || toCpi == null)
            return price;

        // Ajustar por inflación: precio * (CPI_to / CPI_from)
        return Math.Round(price * (toCpi.VehicleIndex / fromCpi.VehicleIndex), 2);
    }

    private static decimal ParseDecimal(string text)
    {
        var cleaned = text.Replace(",", "").Replace("%", "").Trim();
        return decimal.TryParse(cleaned, out var value) ? value : 0;
    }

    private static string GetProvinceCode(string provinceName)
    {
        // Mapeo de nombres a códigos ISO
        return provinceName switch
        {
            "Distrito Nacional" => "DO-01",
            "Santo Domingo" => "DO-32",
            "Santiago" => "DO-25",
            "La Vega" => "DO-13",
            "San Cristóbal" => "DO-21",
            "Puerto Plata" => "DO-18",
            "Duarte" => "DO-06",
            "La Romana" => "DO-12",
            "San Pedro de Macorís" => "DO-23",
            "La Altagracia" => "DO-11",
            _ => provinceName.Substring(0, Math.Min(5, provinceName.Length)).ToUpper()
        };
    }
}

// DTOs para API de ONE
internal record OneDataResponse(
    bool Success,
    OneDataResult Result
);

internal record OneDataResult(
    List<VehicleRecord> Records,
    int Total
);

internal record VehicleRecord(
    string Provincia,
    string TipoVehiculo,
    int Cantidad,
    int Año
);

internal record BancoCentralResponse(
    decimal TasaCompra,
    decimal TasaVenta,
    string Fecha
);
```

---

## ⚛️ React Component

```tsx
// components/MarketInsights.tsx
import { useQuery } from '@tanstack/react-query';
import { marketDataService } from '@/services/marketDataService';
import { TrendingUp, TrendingDown, Minus, Info, DollarSign, Car, MapPin } from 'lucide-react';

export function MarketInsights() {
  const indexQuery = useQuery({
    queryKey: ['market-index'],
    queryFn: () => marketDataService.getMarketIndex(),
    staleTime: 1000 * 60 * 60, // 1 hora
  });

  const exchangeQuery = useQuery({
    queryKey: ['exchange-rate'],
    queryFn: () => marketDataService.getExchangeRate(),
    staleTime: 1000 * 60 * 30, // 30 minutos
  });

  const topProvincesQuery = useQuery({
    queryKey: ['top-provinces'],
    queryFn: () => marketDataService.getTopDemandProvinces(5),
    staleTime: 1000 * 60 * 60 * 24, // 24 horas
  });

  if (indexQuery.isLoading) {
    return <div className="animate-pulse h-64 bg-gray-100 rounded-xl" />;
  }

  const index = indexQuery.data;
  const exchangeRate = exchangeQuery.data;

  return (
    <div className="bg-white rounded-xl shadow-lg p-6">
      <h2 className="text-xl font-bold mb-6 flex items-center gap-2">
        <Info className="w-5 h-5" />
        Análisis de Mercado
      </h2>

      {/* Market Index Cards */}
      <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-6">
        <IndexCard 
          label="Índice de Precios" 
          value={index?.priceIndex ?? 100}
          baseline={100}
          icon={<DollarSign className="w-5 h-5" />}
        />
        <IndexCard 
          label="Demanda" 
          value={index?.demandIndex ?? 100}
          baseline={100}
          icon={<TrendingUp className="w-5 h-5" />}
        />
        <IndexCard 
          label="Oferta" 
          value={index?.supplyIndex ?? 100}
          baseline={100}
          icon={<Car className="w-5 h-5" />}
        />
        <IndexCard 
          label="Accesibilidad" 
          value={index?.affordabilityIndex ?? 100}
          baseline={100}
          icon={<DollarSign className="w-5 h-5" />}
          invertColors
        />
      </div>

      {/* Trend */}
      <div className="flex items-center gap-2 mb-6">
        <span className="text-gray-600">Tendencia del mercado:</span>
        <span className={`flex items-center gap-1 font-medium ${
          index?.trend === 'Alcista' ? 'text-red-600' :
          index?.trend === 'Bajista' ? 'text-green-600' :
          'text-gray-600'
        }`}>
          {index?.trend === 'Alcista' && <TrendingUp className="w-4 h-4" />}
          {index?.trend === 'Bajista' && <TrendingDown className="w-4 h-4" />}
          {index?.trend === 'Estable' && <Minus className="w-4 h-4" />}
          {index?.trend}
        </span>
      </div>

      {/* Exchange Rate */}
      {exchangeRate && (
        <div className="bg-blue-50 p-4 rounded-lg mb-6">
          <div className="flex items-center justify-between">
            <span className="text-gray-600">Tasa de Cambio USD/DOP</span>
            <div className="text-right">
              <span className="text-2xl font-bold text-blue-600">
                RD$ {exchangeRate.averageRate.toFixed(2)}
              </span>
              <p className="text-sm text-gray-500">
                Compra: {exchangeRate.buyRate.toFixed(2)} | 
                Venta: {exchangeRate.sellRate.toFixed(2)}
              </p>
            </div>
          </div>
        </div>
      )}

      {/* Insights */}
      {index?.insights && index.insights.length > 0 && (
        <div className="space-y-2 mb-6">
          <h3 className="font-medium text-gray-700">💡 Insights</h3>
          {index.insights.map((insight, i) => (
            <p key={i} className="text-sm text-gray-600 pl-4 border-l-2 border-blue-300">
              {insight}
            </p>
          ))}
        </div>
      )}

      {/* Top Demand Provinces */}
      {topProvincesQuery.data && (
        <div>
          <h3 className="font-medium text-gray-700 mb-3 flex items-center gap-2">
            <MapPin className="w-4 h-4" />
            Provincias con Mayor Demanda
          </h3>
          <div className="space-y-2">
            {topProvincesQuery.data.map((province, i) => (
              <div key={province.provinceCode} className="flex items-center gap-3">
                <span className={`w-6 h-6 rounded-full flex items-center justify-center text-xs font-bold
                  ${i === 0 ? 'bg-yellow-400 text-yellow-900' :
                    i === 1 ? 'bg-gray-300 text-gray-700' :
                    i === 2 ? 'bg-orange-300 text-orange-900' :
                    'bg-gray-100 text-gray-600'}`}
                >
                  {i + 1}
                </span>
                <div className="flex-1">
                  <span className="font-medium">{province.provinceName}</span>
                  <span className={`ml-2 text-xs px-2 py-0.5 rounded-full
                    ${province.demandLevel === 'Alto' ? 'bg-green-100 text-green-700' :
                      province.demandLevel === 'Medio' ? 'bg-yellow-100 text-yellow-700' :
                      'bg-gray-100 text-gray-600'}`}
                  >
                    {province.demandLevel}
                  </span>
                </div>
                <span className="text-sm text-gray-500">
                  {province.vehicleCount.toLocaleString()} vehículos
                </span>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}

function IndexCard({ 
  label, 
  value, 
  baseline, 
  icon, 
  invertColors = false 
}: {
  label: string;
  value: number;
  baseline: number;
  icon: React.ReactNode;
  invertColors?: boolean;
}) {
  const diff = ((value - baseline) / baseline) * 100;
  const isPositive = invertColors ? diff < 0 : diff > 0;
  
  return (
    <div className="bg-gray-50 rounded-lg p-4">
      <div className="flex items-center gap-2 text-gray-500 mb-2">
        {icon}
        <span className="text-sm">{label}</span>
      </div>
      <div className="flex items-end gap-2">
        <span className="text-2xl font-bold">{value.toFixed(1)}</span>
        {diff !== 0 && (
          <span className={`text-sm ${isPositive ? 'text-green-600' : 'text-red-600'}`}>
            {diff > 0 ? '+' : ''}{diff.toFixed(1)}%
          </span>
        )}
      </div>
    </div>
  );
}
```

---

## 🎯 Controller

```csharp
namespace DataService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MarketDataController : ControllerBase
{
    private readonly IMarketDataService _marketDataService;

    public MarketDataController(IMarketDataService marketDataService)
    {
        _marketDataService = marketDataService;
    }

    /// <summary>
    /// Obtiene índice de mercado de vehículos
    /// </summary>
    [HttpGet("index")]
    [ResponseCache(Duration = 3600)] // 1 hora
    public async Task<ActionResult<VehicleMarketIndex>> GetMarketIndex()
    {
        var index = await _marketDataService.CalculateMarketIndexAsync();
        return Ok(index);
    }

    /// <summary>
    /// Obtiene tasa de cambio actual
    /// </summary>
    [HttpGet("exchange-rate")]
    [ResponseCache(Duration = 1800)] // 30 minutos
    public async Task<ActionResult<ExchangeRate>> GetExchangeRate(
        [FromQuery] string currency = "USD")
    {
        var rate = await _marketDataService.GetCurrentExchangeRateAsync(currency);
        if (rate == null)
            return NotFound("Exchange rate not available");
        return Ok(rate);
    }

    /// <summary>
    /// Obtiene provincias con mayor demanda
    /// </summary>
    [HttpGet("demand/provinces")]
    [ResponseCache(Duration = 86400)] // 24 horas
    public async Task<ActionResult<List<ProvinceMarketDemand>>> GetTopDemandProvinces(
        [FromQuery] int top = 10)
    {
        var provinces = await _marketDataService.GetTopDemandProvincesAsync(top);
        return Ok(provinces);
    }

    /// <summary>
    /// Obtiene historial de IPC
    /// </summary>
    [HttpGet("cpi/history")]
    public async Task<ActionResult<List<ConsumerPriceIndex>>> GetCpiHistory(
        [FromQuery] int months = 12)
    {
        var history = await _marketDataService.GetCpiHistoryAsync(months);
        return Ok(history);
    }

    /// <summary>
    /// Ajusta precio por inflación
    /// </summary>
    [HttpGet("adjust-price")]
    public async Task<ActionResult<PriceAdjustmentResult>> AdjustPrice(
        [FromQuery] decimal price,
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime? toDate = null)
    {
        var adjustedPrice = await _marketDataService.AdjustPriceForInflationAsync(
            price,
            fromDate,
            toDate ?? DateTime.Today
        );

        return Ok(new PriceAdjustmentResult(
            OriginalPrice: price,
            AdjustedPrice: adjustedPrice,
            FromDate: fromDate,
            ToDate: toDate ?? DateTime.Today,
            InflationAdjustment: Math.Round((adjustedPrice - price) / price * 100, 2)
        ));
    }
}

public record PriceAdjustmentResult(
    decimal OriginalPrice,
    decimal AdjustedPrice,
    DateTime FromDate,
    DateTime ToDate,
    decimal InflationAdjustment
);
```

---

## ⚙️ Configuración

```json
{
  "MarketData": {
    "OneDataUrl": "https://datos.one.gob.do/api/3/action/",
    "BancoCentralUrl": "https://api.bancentral.gov.do/api/",
    "CacheHours": {
      "ExchangeRate": 4,
      "CPI": 24,
      "Demographics": 720
    },
    "UpdateSchedule": {
      "ExchangeRate": "0 */4 * * *",
      "CPI": "0 6 1 * *",
      "VehicleFleet": "0 0 1 1 *"
    }
  }
}
```

---

## 📞 Recursos

| Recurso | URL |
|---------|-----|
| Portal ONE | [one.gob.do](https://one.gob.do) |
| Datos Abiertos ONE | [datos.one.gob.do](https://datos.one.gob.do) |
| Banco Central RD | [bancentral.gov.do](https://bancentral.gov.do) |
| Contacto ONE | one@one.gob.do |

---

**Anterior:** [GOOGLE_MAPS_API.md](./GOOGLE_MAPS_API.md)  
**Siguiente:** [SERVICIOS_AUXILIARES_API.md](./SERVICIOS_AUXILIARES_API.md)
