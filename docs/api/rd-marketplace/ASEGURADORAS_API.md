# 🛡️ Multi-Aseguradoras - Comparador de Seguros

**Entidades:** Universal, Mapfre, Sura, La Colonial, Humano, etc.  
**Modelo:** Agregador/Comparador Multi-Carrier  
**Prioridad:** ⭐⭐⭐⭐ ALTA (Mayor conversión por opciones)

---

## 📋 Aseguradoras Principales

| Aseguradora | Website | Especialidad |
|-------------|---------|--------------|
| **Universal** | [universal.com.do](https://www.universal.com.do) | Líder del mercado |
| **Mapfre BHD** | [mapfre.com.do](https://www.mapfre.com.do) | Internacional |
| **Sura** | [segurossura.com.do](https://www.segurossura.com.do) | Empresarial |
| **La Colonial** | [lacolonial.com.do](https://www.lacolonial.com.do) | Tradicional |
| **Humano** | [humano.com.do](https://www.humano.com.do) | Digital |
| **Banreservas Seguros** | [segurosbanreservas.com](https://www.segurosbanreservas.com) | Banco |

---

## 📊 Comparativa de Características

| Característica | Universal | Mapfre | Sura | La Colonial |
|----------------|-----------|--------|------|-------------|
| **API Disponible** | ✅ | ✅ | ❌ | ❌ |
| **Cotización Online** | ✅ | ✅ | ✅ | ✅ |
| **Emisión Inmediata** | ✅ | ✅ | ❌ | ❌ |
| **Asistencia Vial** | ✅ | ✅ | ✅ | ✅ |
| **App Móvil** | ✅ | ✅ | ❌ | ❌ |
| **Cobertura RD Completa** | ✅ | ✅ | ✅ | ✅ |
| **Descuento OKLA** | 10% | 8% | 5% | 7% |

---

## 🌐 API Arquitectura (Agregador)

```
┌─────────────────────────────────────────────────────────────┐
│                    OKLA INSURANCE AGGREGATOR                │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│   [Frontend]  →  [InsuranceService API]  →  [Adapters]      │
│                                                             │
│                          │                                  │
│            ┌─────────────┼─────────────┐                    │
│            ▼             ▼             ▼                    │
│     ┌───────────┐ ┌───────────┐ ┌───────────┐              │
│     │ Universal │ │  Mapfre   │ │  Humano   │              │
│     │  Adapter  │ │  Adapter  │ │  Adapter  │              │
│     └─────┬─────┘ └─────┬─────┘ └─────┬─────┘              │
│           │             │             │                     │
│           ▼             ▼             ▼                     │
│     ┌───────────┐ ┌───────────┐ ┌───────────┐              │
│     │ Universal │ │  Mapfre   │ │  Humano   │              │
│     │    API    │ │    API    │ │  Scraper  │              │
│     └───────────┘ └───────────┘ └───────────┘              │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## 💻 Modelos C#

```csharp
namespace InsuranceService.Domain.Entities;

/// <summary>
/// Aseguradoras soportadas
/// </summary>
public enum InsuranceCarrier
{
    SegurosReservas,
    Universal,
    Mapfre,
    Sura,
    LaColonial,
    Humano,
    BanreservasSeguros
}

/// <summary>
/// Cotización de múltiples aseguradoras
/// </summary>
public record MultiCarrierQuote(
    string ComparisonId,
    VehicleInsuranceInfo Vehicle,
    DriverInfo Driver,
    List<CarrierQuote> Quotes,
    CarrierQuote? BestByPrice,
    CarrierQuote? BestByCoverage,
    CarrierQuote? Recommended,
    DateTime GeneratedAt,
    DateTime ValidUntil
);

public record CarrierQuote(
    InsuranceCarrier Carrier,
    string CarrierName,
    string? QuoteId,
    bool IsAvailable,
    string? ErrorMessage,
    decimal? AnnualPremium,
    decimal? MonthlyPremium,
    List<CoverageDetail> Coverages,
    decimal? Deductible,
    List<string> Benefits,
    int Rating,              // 1-5 estrellas
    int ClaimProcessDays,    // Días promedio para procesar siniestro
    bool HasMobileApp,
    bool Has24x7Support,
    decimal? OklaDiscount,   // Descuento exclusivo por OKLA
    string? SpecialOffer
);

/// <summary>
/// Request para comparación multi-carrier
/// </summary>
public record MultiCarrierQuoteRequest(
    VehicleInsuranceInfo Vehicle,
    DriverInfo Driver,
    List<CoverageType> RequiredCoverages,
    DeductibleLevel PreferredDeductible,
    List<InsuranceCarrier>? PreferredCarriers = null
);

/// <summary>
/// Información de aseguradora para display
/// </summary>
public record CarrierInfo(
    InsuranceCarrier Carrier,
    string Name,
    string LogoUrl,
    string Website,
    string Phone,
    int YearsInMarket,
    decimal MarketShare,
    int CustomerRating,
    List<string> Strengths,
    List<string> Weaknesses
);
```

---

## 🔧 Service Interface

```csharp
namespace InsuranceService.Domain.Interfaces;

/// <summary>
/// Agregador de múltiples aseguradoras
/// </summary>
public interface IInsuranceAggregatorService
{
    /// <summary>
    /// Obtiene cotizaciones de todas las aseguradoras disponibles
    /// </summary>
    Task<MultiCarrierQuote> GetMultiCarrierQuotesAsync(
        MultiCarrierQuoteRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Obtiene cotización de una aseguradora específica
    /// </summary>
    Task<CarrierQuote> GetQuoteFromCarrierAsync(
        InsuranceCarrier carrier,
        QuoteRequest request);

    /// <summary>
    /// Lista aseguradoras disponibles con info
    /// </summary>
    IEnumerable<CarrierInfo> GetAvailableCarriers();

    /// <summary>
    /// Emite póliza con la aseguradora seleccionada
    /// </summary>
    Task<InsurancePolicy> IssuePolicyAsync(
        InsuranceCarrier carrier,
        string quoteId,
        PaymentDetails payment);
}

/// <summary>
/// Interface para adaptadores de cada aseguradora
/// </summary>
public interface IInsuranceCarrierAdapter
{
    InsuranceCarrier Carrier { get; }
    Task<CarrierQuote> GetQuoteAsync(QuoteRequest request);
    Task<InsurancePolicy> IssuePolicyAsync(string quoteId, PaymentDetails payment);
    Task<InsurancePolicy?> GetPolicyAsync(string policyNumber);
}
```

---

## 🏗️ Service Implementation

```csharp
namespace InsuranceService.Infrastructure.Services;

public class InsuranceAggregatorService : IInsuranceAggregatorService
{
    private readonly IEnumerable<IInsuranceCarrierAdapter> _adapters;
    private readonly IMemoryCache _cache;
    private readonly ILogger<InsuranceAggregatorService> _logger;

    private readonly Dictionary<InsuranceCarrier, CarrierInfo> _carrierInfo;

    public InsuranceAggregatorService(
        IEnumerable<IInsuranceCarrierAdapter> adapters,
        IMemoryCache cache,
        ILogger<InsuranceAggregatorService> logger)
    {
        _adapters = adapters;
        _cache = cache;
        _logger = logger;
        _carrierInfo = InitializeCarrierInfo();
    }

    public async Task<MultiCarrierQuote> GetMultiCarrierQuotesAsync(
        MultiCarrierQuoteRequest request,
        CancellationToken ct = default)
    {
        var comparisonId = $"CMP-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..24];

        // Filtrar adapters si hay preferidos
        var adaptersToUse = request.PreferredCarriers?.Any() == true
            ? _adapters.Where(a => request.PreferredCarriers.Contains(a.Carrier))
            : _adapters;

        // Consultar todas las aseguradoras en paralelo
        var quoteRequest = new QuoteRequest(
            Vehicle: request.Vehicle,
            Driver: request.Driver,
            Coverages: request.RequiredCoverages,
            Deductible: request.PreferredDeductible
        );

        var tasks = adaptersToUse.Select(async adapter =>
        {
            try
            {
                return await adapter.GetQuoteAsync(quoteRequest);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, 
                    "Error getting quote from {Carrier}", adapter.Carrier);
                
                return new CarrierQuote(
                    Carrier: adapter.Carrier,
                    CarrierName: _carrierInfo[adapter.Carrier].Name,
                    QuoteId: null,
                    IsAvailable: false,
                    ErrorMessage: "No disponible en este momento",
                    AnnualPremium: null,
                    MonthlyPremium: null,
                    Coverages: new(),
                    Deductible: null,
                    Benefits: new(),
                    Rating: 0,
                    ClaimProcessDays: 0,
                    HasMobileApp: false,
                    Has24x7Support: false,
                    OklaDiscount: null,
                    SpecialOffer: null
                );
            }
        });

        var quotes = (await Task.WhenAll(tasks)).ToList();

        // Ordenar y seleccionar mejores opciones
        var availableQuotes = quotes
            .Where(q => q.IsAvailable && q.AnnualPremium.HasValue)
            .OrderBy(q => q.AnnualPremium)
            .ToList();

        var bestByPrice = availableQuotes.FirstOrDefault();
        
        var bestByCoverage = availableQuotes
            .OrderByDescending(q => q.Coverages.Sum(c => c.Limit))
            .ThenBy(q => q.AnnualPremium)
            .FirstOrDefault();

        // Recomendado: balance precio/cobertura/rating
        var recommended = availableQuotes
            .OrderByDescending(q => CalculateScore(q))
            .FirstOrDefault();

        return new MultiCarrierQuote(
            ComparisonId: comparisonId,
            Vehicle: request.Vehicle,
            Driver: request.Driver,
            Quotes: quotes,
            BestByPrice: bestByPrice,
            BestByCoverage: bestByCoverage,
            Recommended: recommended,
            GeneratedAt: DateTime.UtcNow,
            ValidUntil: DateTime.UtcNow.AddHours(24)
        );
    }

    public async Task<CarrierQuote> GetQuoteFromCarrierAsync(
        InsuranceCarrier carrier,
        QuoteRequest request)
    {
        var adapter = _adapters.FirstOrDefault(a => a.Carrier == carrier)
            ?? throw new ArgumentException($"Carrier {carrier} not supported");

        return await adapter.GetQuoteAsync(request);
    }

    public IEnumerable<CarrierInfo> GetAvailableCarriers()
    {
        return _carrierInfo.Values;
    }

    public async Task<InsurancePolicy> IssuePolicyAsync(
        InsuranceCarrier carrier,
        string quoteId,
        PaymentDetails payment)
    {
        var adapter = _adapters.FirstOrDefault(a => a.Carrier == carrier)
            ?? throw new ArgumentException($"Carrier {carrier} not supported");

        return await adapter.IssuePolicyAsync(quoteId, payment);
    }

    private static decimal CalculateScore(CarrierQuote quote)
    {
        // Score basado en múltiples factores (0-100)
        var priceScore = 100 - (quote.AnnualPremium / 1000 ?? 100); // Menor precio = mejor
        var coverageScore = quote.Coverages.Count * 10;
        var ratingScore = quote.Rating * 10;
        var benefitScore = (quote.Has24x7Support ? 10 : 0) + (quote.HasMobileApp ? 5 : 0);
        var discountScore = (quote.OklaDiscount ?? 0) * 2;

        return priceScore * 0.3m + 
               coverageScore * 0.25m + 
               ratingScore * 0.25m + 
               benefitScore * 0.1m +
               discountScore * 0.1m;
    }

    private static Dictionary<InsuranceCarrier, CarrierInfo> InitializeCarrierInfo()
    {
        return new Dictionary<InsuranceCarrier, CarrierInfo>
        {
            [InsuranceCarrier.SegurosReservas] = new(
                Carrier: InsuranceCarrier.SegurosReservas,
                Name: "Seguros Reservas",
                LogoUrl: "/images/insurers/seguros-reservas.png",
                Website: "https://www.segurosreservas.com",
                Phone: "809-960-0000",
                YearsInMarket: 25,
                MarketShare: 22.5m,
                CustomerRating: 4,
                Strengths: new() { "Respaldo Grupo Popular", "Amplia red", "App móvil" },
                Weaknesses: new() { "Proceso de siniestros lento" }
            ),
            [InsuranceCarrier.Universal] = new(
                Carrier: InsuranceCarrier.Universal,
                Name: "Universal Seguros",
                LogoUrl: "/images/insurers/universal.png",
                Website: "https://www.universal.com.do",
                Phone: "809-544-7111",
                YearsInMarket: 60,
                MarketShare: 28.0m,
                CustomerRating: 5,
                Strengths: new() { "Líder del mercado", "Mejor servicio al cliente", "Siniestros rápidos" },
                Weaknesses: new() { "Precios ligeramente más altos" }
            ),
            [InsuranceCarrier.Mapfre] = new(
                Carrier: InsuranceCarrier.Mapfre,
                Name: "Mapfre BHD",
                LogoUrl: "/images/insurers/mapfre.png",
                Website: "https://www.mapfre.com.do",
                Phone: "809-549-5050",
                YearsInMarket: 30,
                MarketShare: 18.0m,
                CustomerRating: 4,
                Strengths: new() { "Respaldo internacional", "Cobertura en el extranjero" },
                Weaknesses: new() { "Menos sucursales" }
            ),
            [InsuranceCarrier.Sura] = new(
                Carrier: InsuranceCarrier.Sura,
                Name: "Seguros Sura",
                LogoUrl: "/images/insurers/sura.png",
                Website: "https://www.segurossura.com.do",
                Phone: "809-567-8000",
                YearsInMarket: 15,
                MarketShare: 12.0m,
                CustomerRating: 4,
                Strengths: new() { "Excelente para empresas", "Servicio personalizado" },
                Weaknesses: new() { "Menos conocida" }
            ),
            [InsuranceCarrier.LaColonial] = new(
                Carrier: InsuranceCarrier.LaColonial,
                Name: "La Colonial",
                LogoUrl: "/images/insurers/la-colonial.png",
                Website: "https://www.lacolonial.com.do",
                Phone: "809-562-5555",
                YearsInMarket: 50,
                MarketShare: 10.0m,
                CustomerRating: 3,
                Strengths: new() { "Tradicional", "Precios competitivos" },
                Weaknesses: new() { "Tecnología limitada", "Sin app" }
            ),
            [InsuranceCarrier.Humano] = new(
                Carrier: InsuranceCarrier.Humano,
                Name: "Humano Seguros",
                LogoUrl: "/images/insurers/humano.png",
                Website: "https://www.humano.com.do",
                Phone: "809-541-4600",
                YearsInMarket: 10,
                MarketShare: 5.0m,
                CustomerRating: 4,
                Strengths: new() { "Digital first", "Procesos ágiles" },
                Weaknesses: new() { "Red limitada" }
            )
        };
    }
}

/// <summary>
/// Adapter para Universal Seguros (ejemplo)
/// </summary>
public class UniversalInsuranceAdapter : IInsuranceCarrierAdapter
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public InsuranceCarrier Carrier => InsuranceCarrier.Universal;

    public UniversalInsuranceAdapter(
        HttpClient httpClient, 
        IConfiguration config)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(config["Insurance:Universal:BaseUrl"]!);
        _config = config;
    }

    public async Task<CarrierQuote> GetQuoteAsync(QuoteRequest request)
    {
        // Implementación específica para Universal
        var apiRequest = new
        {
            vehiculo = new
            {
                marca = request.Vehicle.Make,
                modelo = request.Vehicle.Model,
                ano = request.Vehicle.Year,
                valor = request.Vehicle.InsuredValue
            },
            conductor = new
            {
                cedula = request.Driver.Cedula,
                fechaNacimiento = request.Driver.DateOfBirth.ToString("yyyy-MM-dd")
            }
        };

        var response = await _httpClient.PostAsJsonAsync("cotizar", apiRequest);
        
        if (!response.IsSuccessStatusCode)
        {
            return new CarrierQuote(
                Carrier: Carrier,
                CarrierName: "Universal Seguros",
                QuoteId: null,
                IsAvailable: false,
                ErrorMessage: "Error al consultar",
                AnnualPremium: null,
                MonthlyPremium: null,
                Coverages: new(),
                Deductible: null,
                Benefits: new(),
                Rating: 5,
                ClaimProcessDays: 5,
                HasMobileApp: true,
                Has24x7Support: true,
                OklaDiscount: null,
                SpecialOffer: null
            );
        }

        var result = await response.Content.ReadFromJsonAsync<UniversalQuoteResponse>();

        return new CarrierQuote(
            Carrier: Carrier,
            CarrierName: "Universal Seguros",
            QuoteId: result!.CotizacionId,
            IsAvailable: true,
            ErrorMessage: null,
            AnnualPremium: result.PrimaAnual,
            MonthlyPremium: result.PrimaMensual,
            Coverages: result.Coberturas.Select(c => new CoverageDetail(
                c.Codigo, c.Nombre, c.Limite, c.Deducible, c.Prima, c.Obligatoria
            )).ToList(),
            Deductible: result.DeducibleGeneral,
            Benefits: result.Beneficios,
            Rating: 5,
            ClaimProcessDays: 5,
            HasMobileApp: true,
            Has24x7Support: true,
            OklaDiscount: 10, // 10% descuento exclusivo OKLA
            SpecialOffer: "Primer mes gratis con OKLA"
        );
    }

    public async Task<InsurancePolicy> IssuePolicyAsync(
        string quoteId, PaymentDetails payment)
    {
        // Implementación para emitir póliza
        throw new NotImplementedException();
    }

    public async Task<InsurancePolicy?> GetPolicyAsync(string policyNumber)
    {
        throw new NotImplementedException();
    }
}

internal record UniversalQuoteResponse(
    string CotizacionId,
    decimal PrimaAnual,
    decimal PrimaMensual,
    List<UniversalCoverage> Coberturas,
    decimal DeducibleGeneral,
    List<string> Beneficios
);

internal record UniversalCoverage(
    string Codigo,
    string Nombre,
    decimal Limite,
    decimal? Deducible,
    decimal Prima,
    bool Obligatoria
);
```

---

## ⚛️ React Component

```tsx
// components/InsuranceComparator.tsx
import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { insuranceService } from '@/services/insuranceService';
import { 
  Shield, Star, Clock, Smartphone, Phone, 
  CheckCircle, XCircle, TrendingDown, Award
} from 'lucide-react';

interface Props {
  vehicleMake: string;
  vehicleModel: string;
  vehicleYear: number;
  vehicleValue: number;
  driverCedula: string;
  driverBirthDate: string;
  onSelect?: (quote: CarrierQuote) => void;
}

export function InsuranceComparator({
  vehicleMake,
  vehicleModel,
  vehicleYear,
  vehicleValue,
  driverCedula,
  driverBirthDate,
  onSelect,
}: Props) {
  const [selectedCoverages, setSelectedCoverages] = useState([
    'RESPONSABILIDAD_CIVIL',
    'COLISION',
    'ROBO_TOTAL',
  ]);
  const [sortBy, setSortBy] = useState<'price' | 'rating' | 'coverage'>('price');

  const compareQuery = useQuery({
    queryKey: ['insurance-compare', vehicleMake, vehicleModel, vehicleYear, vehicleValue, selectedCoverages],
    queryFn: () => insuranceService.compareInsurers({
      vehicle: {
        make: vehicleMake,
        model: vehicleModel,
        year: vehicleYear,
        type: 'SEDAN',
        use: 'PARTICULAR',
        insuredValue: vehicleValue,
      },
      driver: {
        cedula: driverCedula,
        dateOfBirth: driverBirthDate,
        gender: 'M',
        yearsOfExperience: 5,
      },
      requiredCoverages: selectedCoverages,
      preferredDeductible: 'ESTANDAR',
    }),
  });

  const sortedQuotes = compareQuery.data?.quotes
    .filter(q => q.isAvailable)
    .sort((a, b) => {
      switch (sortBy) {
        case 'price':
          return (a.annualPremium ?? Infinity) - (b.annualPremium ?? Infinity);
        case 'rating':
          return b.rating - a.rating;
        case 'coverage':
          return b.coverages.length - a.coverages.length;
        default:
          return 0;
      }
    }) ?? [];

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="bg-gradient-to-r from-indigo-600 to-purple-600 
                      text-white rounded-xl p-6">
        <div className="flex items-center gap-3 mb-2">
          <Shield className="w-8 h-8" />
          <h2 className="text-2xl font-bold">Comparador de Seguros</h2>
        </div>
        <p className="text-indigo-200">
          Compara las mejores aseguradoras de RD en segundos
        </p>
      </div>

      {/* Sorting */}
      <div className="flex gap-2">
        <span className="text-sm text-gray-500 self-center">Ordenar por:</span>
        {[
          { value: 'price', label: 'Precio', icon: TrendingDown },
          { value: 'rating', label: 'Calificación', icon: Star },
          { value: 'coverage', label: 'Cobertura', icon: Shield },
        ].map((option) => (
          <button
            key={option.value}
            onClick={() => setSortBy(option.value as any)}
            className={`flex items-center gap-1 px-3 py-1 rounded-full text-sm ${
              sortBy === option.value
                ? 'bg-indigo-600 text-white'
                : 'bg-gray-100 text-gray-700'
            }`}
          >
            <option.icon className="w-4 h-4" />
            {option.label}
          </button>
        ))}
      </div>

      {/* Loading */}
      {compareQuery.isLoading && (
        <div className="text-center py-12">
          <div className="animate-spin w-12 h-12 border-4 border-indigo-600 
                          border-t-transparent rounded-full mx-auto" />
          <p className="mt-4 text-gray-600">
            Consultando 6 aseguradoras...
          </p>
        </div>
      )}

      {/* Results Grid */}
      {compareQuery.data && (
        <div className="space-y-4">
          {/* Best options highlights */}
          <div className="grid grid-cols-3 gap-4">
            {compareQuery.data.bestByPrice && (
              <HighlightCard
                title="Mejor Precio"
                quote={compareQuery.data.bestByPrice}
                color="green"
                icon={TrendingDown}
              />
            )}
            {compareQuery.data.recommended && (
              <HighlightCard
                title="Recomendado"
                quote={compareQuery.data.recommended}
                color="indigo"
                icon={Award}
              />
            )}
            {compareQuery.data.bestByCoverage && (
              <HighlightCard
                title="Mejor Cobertura"
                quote={compareQuery.data.bestByCoverage}
                color="blue"
                icon={Shield}
              />
            )}
          </div>

          {/* All quotes */}
          <div className="space-y-4">
            {sortedQuotes.map((quote) => (
              <QuoteCard
                key={quote.carrier}
                quote={quote}
                isRecommended={quote.quoteId === compareQuery.data.recommended?.quoteId}
                onSelect={() => onSelect?.(quote)}
              />
            ))}
          </div>

          {/* Unavailable carriers */}
          {compareQuery.data.quotes.filter(q => !q.isAvailable).length > 0 && (
            <div className="border-t pt-4">
              <p className="text-sm text-gray-500 mb-2">No disponibles:</p>
              <div className="flex gap-2">
                {compareQuery.data.quotes
                  .filter(q => !q.isAvailable)
                  .map((quote) => (
                    <span key={quote.carrier} 
                          className="text-xs bg-gray-100 text-gray-500 
                                     px-2 py-1 rounded">
                      {quote.carrierName}
                    </span>
                  ))}
              </div>
            </div>
          )}
        </div>
      )}
    </div>
  );
}

function HighlightCard({
  title,
  quote,
  color,
  icon: Icon,
}: {
  title: string;
  quote: CarrierQuote;
  color: 'green' | 'indigo' | 'blue';
  icon: any;
}) {
  const colors = {
    green: 'bg-green-50 border-green-200 text-green-700',
    indigo: 'bg-indigo-50 border-indigo-200 text-indigo-700',
    blue: 'bg-blue-50 border-blue-200 text-blue-700',
  };

  return (
    <div className={`rounded-lg border p-4 ${colors[color]}`}>
      <div className="flex items-center gap-2 mb-2">
        <Icon className="w-5 h-5" />
        <span className="text-sm font-medium">{title}</span>
      </div>
      <p className="font-bold">{quote.carrierName}</p>
      <p className="text-lg font-bold">
        RD$ {quote.annualPremium?.toLocaleString()}/año
      </p>
    </div>
  );
}

function QuoteCard({
  quote,
  isRecommended,
  onSelect,
}: {
  quote: CarrierQuote;
  isRecommended: boolean;
  onSelect: () => void;
}) {
  return (
    <div 
      className={`bg-white rounded-xl shadow-md overflow-hidden border-2 ${
        isRecommended ? 'border-indigo-500' : 'border-transparent'
      }`}
    >
      {isRecommended && (
        <div className="bg-indigo-600 text-white text-center text-sm py-1">
          ⭐ RECOMENDADO PARA TI
        </div>
      )}

      <div className="p-4">
        <div className="flex justify-between items-start">
          {/* Carrier info */}
          <div className="flex items-center gap-3">
            <div className="w-16 h-16 bg-gray-100 rounded-lg flex items-center 
                            justify-center">
              <Shield className="w-8 h-8 text-gray-400" />
            </div>
            <div>
              <h3 className="font-bold text-lg">{quote.carrierName}</h3>
              <div className="flex items-center gap-1">
                {[...Array(5)].map((_, i) => (
                  <Star
                    key={i}
                    className={`w-4 h-4 ${
                      i < quote.rating
                        ? 'text-yellow-400 fill-yellow-400'
                        : 'text-gray-300'
                    }`}
                  />
                ))}
                <span className="text-sm text-gray-500 ml-1">
                  ({quote.rating}/5)
                </span>
              </div>
            </div>
          </div>

          {/* Price */}
          <div className="text-right">
            <p className="text-3xl font-bold">
              RD$ {quote.annualPremium?.toLocaleString()}
            </p>
            <p className="text-sm text-gray-500">por año</p>
            <p className="text-lg text-indigo-600 font-medium">
              RD$ {quote.monthlyPremium?.toLocaleString()}/mes
            </p>
          </div>
        </div>

        {/* Features */}
        <div className="grid grid-cols-4 gap-4 mt-4 py-4 border-t border-b">
          <Feature
            icon={Clock}
            label="Siniestros"
            value={`${quote.claimProcessDays} días`}
          />
          <Feature
            icon={Smartphone}
            label="App Móvil"
            value={quote.hasMobileApp}
          />
          <Feature
            icon={Phone}
            label="24/7 Soporte"
            value={quote.has24x7Support}
          />
          <Feature
            icon={Shield}
            label="Coberturas"
            value={`${quote.coverages.length}`}
          />
        </div>

        {/* OKLA discount */}
        {quote.oklaDiscount && (
          <div className="mt-3 bg-green-50 text-green-700 rounded-lg p-3 
                          flex items-center gap-2">
            <CheckCircle className="w-5 h-5" />
            <div>
              <span className="font-medium">
                {quote.oklaDiscount}% descuento exclusivo OKLA
              </span>
              {quote.specialOffer && (
                <span className="block text-sm">{quote.specialOffer}</span>
              )}
            </div>
          </div>
        )}

        {/* CTA */}
        <button
          onClick={onSelect}
          className={`w-full mt-4 py-3 rounded-lg font-medium ${
            isRecommended
              ? 'bg-indigo-600 text-white hover:bg-indigo-700'
              : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
          }`}
        >
          Seleccionar
        </button>
      </div>
    </div>
  );
}

function Feature({
  icon: Icon,
  label,
  value,
}: {
  icon: any;
  label: string;
  value: string | boolean;
}) {
  return (
    <div className="text-center">
      <Icon className="w-5 h-5 mx-auto text-gray-400 mb-1" />
      <p className="text-xs text-gray-500">{label}</p>
      {typeof value === 'boolean' ? (
        value ? (
          <CheckCircle className="w-5 h-5 text-green-500 mx-auto" />
        ) : (
          <XCircle className="w-5 h-5 text-gray-300 mx-auto" />
        )
      ) : (
        <p className="font-medium">{value}</p>
      )}
    </div>
  );
}
```

---

## 🔌 API Controller

```csharp
[ApiController]
[Route("api/insurance")]
public class InsuranceAggregatorController : ControllerBase
{
    private readonly IInsuranceAggregatorService _service;

    public InsuranceAggregatorController(IInsuranceAggregatorService service)
    {
        _service = service;
    }

    [HttpPost("compare")]
    public async Task<ActionResult<MultiCarrierQuote>> CompareQuotes(
        [FromBody] MultiCarrierQuoteRequest request,
        CancellationToken ct)
    {
        var result = await _service.GetMultiCarrierQuotesAsync(request, ct);
        return Ok(result);
    }

    [HttpGet("carriers")]
    public ActionResult<IEnumerable<CarrierInfo>> GetCarriers()
    {
        return Ok(_service.GetAvailableCarriers());
    }

    [HttpPost("{carrier}/quote")]
    public async Task<ActionResult<CarrierQuote>> GetQuote(
        InsuranceCarrier carrier,
        [FromBody] QuoteRequest request)
    {
        var result = await _service.GetQuoteFromCarrierAsync(carrier, request);
        return Ok(result);
    }

    [HttpPost("{carrier}/issue")]
    [Authorize]
    public async Task<ActionResult<InsurancePolicy>> IssuePolicy(
        InsuranceCarrier carrier,
        [FromBody] IssuePolicyRequest request)
    {
        var result = await _service.IssuePolicyAsync(
            carrier, request.QuoteId, request.Payment);
        return Ok(result);
    }
}
```

---

## ⚙️ Configuración

```json
{
  "Insurance": {
    "Universal": {
      "BaseUrl": "https://api.universal.com.do/v1/",
      "ClientId": "OKLA-001",
      "ClientSecret": "xxxxx",
      "Commission": 0.15
    },
    "Mapfre": {
      "BaseUrl": "https://api.mapfre.com.do/v1/",
      "ClientId": "OKLA-001",
      "ClientSecret": "xxxxx",
      "Commission": 0.12
    },
    "Sura": {
      "BaseUrl": "https://api.segurossura.com.do/v1/",
      "ClientId": "OKLA-001",
      "ClientSecret": "xxxxx",
      "Commission": 0.10
    }
  }
}
```

---

## 📞 Contactos

| Aseguradora | Teléfono Comercial | Email Convenios |
|-------------|--------------------| ----------------|
| Universal | 809-544-7111 | comercial@universal.com.do |
| Mapfre BHD | 809-549-5050 | empresas@mapfre.com.do |
| Sura | 809-567-8000 | convenios@segurossura.com.do |
| La Colonial | 809-562-5555 | ventas@lacolonial.com.do |
| Humano | 809-541-4600 | digital@humano.com.do |

---

**Anterior:** [SEGUROS_RESERVAS_API.md](./SEGUROS_RESERVAS_API.md)  
**Siguiente:** [WHATSAPP_BUSINESS_API.md](./WHATSAPP_BUSINESS_API.md)
