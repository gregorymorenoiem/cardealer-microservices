# 🛡️ Insurance APIs

**Categoría:** Financial Services  
**APIs:** 4 (Seguros Reservas, Colonial, Mapfre, Jerry.ai)  
**Fase:** 3 (Premium Features)  
**Impacto:** +25% ingresos por referido cuando comprador contrata seguro

---

## 📖 Resumen

Integración con aseguradoras dominicanas y AI para cotizar y contratar seguros de vehículos directamente desde OKLA. El comprador puede comparar coberturas y precios sin salir del marketplace.

### Casos de Uso en OKLA

✅ **Cotización instantánea** - Comprador ve precio de seguro junto al vehículo  
✅ **Comparador de aseguradoras** - "Reservas: $2,500/mes | Colonial: $2,800/mes"  
✅ **Bundle con financiamiento** - Seguro incluido en la cuota mensual  
✅ **Compra directa de póliza** - Sin salir de OKLA, póliza emitida en minutos  
✅ **Renovación automática** - Recordatorios y renovación sin fricción  
✅ **Seguro para dealers** - Cobertura de inventario con tarifa preferencial

---

## 🔗 Comparativa de APIs

| Aspecto                | **Seguros Reservas** | **Colonial**  | **Mapfre**         | **Jerry.ai**   |
| ---------------------- | -------------------- | ------------- | ------------------ | -------------- |
| **Cobertura mínima**   | RD$1,200/mes         | RD$1,500/mes  | RD$1,800/mes       | Varía          |
| **Cobertura integral** | RD$3,500/mes         | RD$3,200/mes  | RD$3,800/mes       | Varía          |
| **Deducible**          | RD$15,000            | RD$20,000     | RD$18,000          | Varía          |
| **API disponible**     | ✅ REST              | ⚠️ SOAP       | ⚠️ Manual          | ✅ REST AI     |
| **Tiempo de emisión**  | 5 minutos            | 15 minutos    | 24 horas           | 3 minutos      |
| **Mejor para**         | Vehículos nuevos     | Flotas        | Premium/Importados | Comparación AI |
| **Recomendado**        | ⭐ PRINCIPAL         | ⭐ Secundario | Backup             | ⭐ Comparador  |

---

## 📡 ENDPOINTS

### Seguros Reservas API

- `POST /api/quotes` - Cotizar seguro
- `GET /api/quotes/{id}` - Obtener cotización
- `POST /api/policies` - Emitir póliza
- `GET /api/policies/{id}` - Consultar póliza
- `POST /api/policies/{id}/renew` - Renovar póliza
- `GET /api/coverages` - Tipos de cobertura disponibles

### Jerry.ai API

- `POST /api/compare` - Comparar múltiples aseguradoras con AI
- `GET /api/recommendations` - Recomendaciones personalizadas
- `POST /api/bind` - Contratar póliza seleccionada

---

## 💻 Backend Implementation (C#)

### Service Interface

```csharp
public interface IInsuranceService
{
    Task<InsuranceQuote> GetQuoteAsync(QuoteRequest request);
    Task<InsuranceQuote[]> CompareInsurersAsync(QuoteRequest request);
    Task<Policy> PurchasePolicyAsync(PurchaseRequest request);
    Task<Policy> GetPolicyAsync(string policyId);
    Task<Policy> RenewPolicyAsync(string policyId);
    Task<Coverage[]> GetCoverageOptionsAsync(string vehicleType);
}

public class QuoteRequest
{
    public string Make { get; set; }
    public string Model { get; set; }
    public int Year { get; set; }
    public int Mileage { get; set; }
    public string VehicleUse { get; set; } // Personal, Commercial, Rideshare
    public string DriverCedula { get; set; }
    public DateTime DriverBirthDate { get; set; }
    public CoverageType CoverageType { get; set; }
}

public enum CoverageType
{
    Liability,        // Solo responsabilidad civil
    Collision,        // Colisión
    Comprehensive,    // Todo riesgo
    FullCoverage      // Cobertura integral
}
```

### Domain Models

```csharp
public class InsuranceQuote
{
    public string QuoteId { get; set; }
    public string InsurerId { get; set; }
    public string InsurerName { get; set; }
    public string InsurerLogo { get; set; }
    public decimal MonthlyPremium { get; set; }
    public decimal AnnualPremium { get; set; }
    public decimal Deductible { get; set; }
    public CoverageType CoverageType { get; set; }
    public CoverageDetail[] Coverages { get; set; }
    public DateTime ValidUntil { get; set; }
    public bool IsRecommended { get; set; }
}

public class CoverageDetail
{
    public string Name { get; set; }        // "Responsabilidad Civil"
    public string Description { get; set; } // "Cubre daños a terceros hasta RD$500K"
    public decimal Limit { get; set; }      // 500,000
    public bool IsIncluded { get; set; }
}

public class Policy
{
    public string PolicyId { get; set; }
    public string PolicyNumber { get; set; }
    public string CustomerId { get; set; }
    public string VehicleId { get; set; }
    public string InsurerId { get; set; }
    public decimal MonthlyPremium { get; set; }
    public PolicyStatus Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string PolicyDocumentUrl { get; set; }
}

public enum PolicyStatus
{
    Pending,
    Active,
    Expired,
    Cancelled,
    UnderClaim
}
```

### Service Implementation

```csharp
public class SegurosReservasInsuranceService : IInsuranceService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<SegurosReservasInsuranceService> _logger;
    private readonly string _apiKey;
    private const string BaseUrl = "https://api.segurosreservas.com.do";

    public SegurosReservasInsuranceService(HttpClient httpClient, IConfiguration config, ILogger<SegurosReservasInsuranceService> logger)
    {
        _httpClient = httpClient;
        _config = config;
        _logger = logger;
        _apiKey = config["SegurosReservas:ApiKey"];
    }

    public async Task<InsuranceQuote> GetQuoteAsync(QuoteRequest request)
    {
        try
        {
            var payload = new
            {
                make = request.Make,
                model = request.Model,
                year = request.Year,
                mileage = request.Mileage,
                vehicle_use = request.VehicleUse,
                driver_cedula = request.DriverCedula,
                driver_birth_date = request.DriverBirthDate.ToString("yyyy-MM-dd"),
                coverage_type = request.CoverageType.ToString().ToLower()
            };

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/api/quotes");
            httpRequest.Headers.Add("X-API-Key", _apiKey);
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(httpRequest);
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            return new InsuranceQuote
            {
                QuoteId = root.GetProperty("quote_id").GetString(),
                InsurerId = "seguros-reservas",
                InsurerName = "Seguros Reservas",
                InsurerLogo = "https://cdn.okla.com.do/insurers/seguros-reservas.png",
                MonthlyPremium = root.GetProperty("monthly_premium").GetDecimal(),
                AnnualPremium = root.GetProperty("annual_premium").GetDecimal(),
                Deductible = root.GetProperty("deductible").GetDecimal(),
                CoverageType = request.CoverageType,
                ValidUntil = DateTime.UtcNow.AddDays(30),
                Coverages = ParseCoverages(root.GetProperty("coverages"))
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting quote from Seguros Reservas");
            throw;
        }
    }

    public async Task<InsuranceQuote[]> CompareInsurersAsync(QuoteRequest request)
    {
        // Usa Jerry.ai para comparar múltiples aseguradoras
        var jerryClient = new JerryAiService(_httpClient, _config);
        return await jerryClient.CompareAsync(request);
    }

    public async Task<Policy> PurchasePolicyAsync(PurchaseRequest request)
    {
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/api/policies");
        httpRequest.Headers.Add("X-API-Key", _apiKey);
        httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(httpRequest);
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        return new Policy
        {
            PolicyId = root.GetProperty("policy_id").GetString(),
            PolicyNumber = root.GetProperty("policy_number").GetString(),
            InsurerId = "seguros-reservas",
            MonthlyPremium = root.GetProperty("monthly_premium").GetDecimal(),
            Status = PolicyStatus.Active,
            StartDate = DateTime.Parse(root.GetProperty("start_date").GetString()),
            EndDate = DateTime.Parse(root.GetProperty("end_date").GetString()),
            PolicyDocumentUrl = root.GetProperty("document_url").GetString()
        };
    }

    public async Task<Policy> RenewPolicyAsync(string policyId)
    {
        var response = await _httpClient.PostAsync($"{BaseUrl}/api/policies/{policyId}/renew", null);
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Policy>(json);
    }

    private CoverageDetail[] ParseCoverages(JsonElement coveragesElement)
    {
        var coverages = new List<CoverageDetail>();
        foreach (var item in coveragesElement.EnumerateArray())
        {
            coverages.Add(new CoverageDetail
            {
                Name = item.GetProperty("name").GetString(),
                Description = item.GetProperty("description").GetString(),
                Limit = item.GetProperty("limit").GetDecimal(),
                IsIncluded = item.GetProperty("included").GetBoolean()
            });
        }
        return coverages.ToArray();
    }
}
```

---

## 🎨 Frontend Implementation (React + TypeScript)

### Insurance Service

```typescript
import axios from "axios";

export interface InsuranceQuote {
  quoteId: string;
  insurerName: string;
  insurerLogo: string;
  monthlyPremium: number;
  annualPremium: number;
  deductible: number;
  coverageType: string;
  coverages: CoverageDetail[];
  isRecommended: boolean;
}

export interface CoverageDetail {
  name: string;
  description: string;
  limit: number;
  isIncluded: boolean;
}

export class InsuranceService {
  private baseUrl = process.env.REACT_APP_API_URL;

  async getQuote(vehicleData: VehicleData): Promise<InsuranceQuote> {
    const response = await axios.post(
      `${this.baseUrl}/api/insurance/quotes`,
      vehicleData
    );
    return response.data;
  }

  async compareInsurers(vehicleData: VehicleData): Promise<InsuranceQuote[]> {
    const response = await axios.post(
      `${this.baseUrl}/api/insurance/compare`,
      vehicleData
    );
    return response.data;
  }

  async purchasePolicy(
    quoteId: string,
    paymentInfo: PaymentInfo
  ): Promise<Policy> {
    const response = await axios.post(
      `${this.baseUrl}/api/insurance/policies`,
      {
        quoteId,
        paymentInfo,
      }
    );
    return response.data;
  }
}
```

### React Component - Insurance Comparison

```typescript
import React, { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { InsuranceService, InsuranceQuote } from "@/services/insuranceService";
import { CheckCircle, Shield, AlertTriangle } from "lucide-react";

interface Props {
  vehicle: { make: string; model: string; year: number; mileage: number };
}

export const InsuranceComparison = ({ vehicle }: Props) => {
  const [selectedQuote, setSelectedQuote] = useState<string | null>(null);
  const insuranceService = new InsuranceService();

  const { data: quotes, isLoading } = useQuery({
    queryKey: ["insurance-quotes", vehicle],
    queryFn: () => insuranceService.compareInsurers(vehicle),
  });

  const formatCurrency = (value: number) =>
    new Intl.NumberFormat("es-DO", {
      style: "currency",
      currency: "DOP",
    }).format(value);

  if (isLoading) {
    return (
      <div className="animate-pulse space-y-4">
        {[1, 2, 3].map((i) => (
          <div key={i} className="h-32 bg-gray-200 rounded-xl"></div>
        ))}
      </div>
    );
  }

  return (
    <div className="space-y-4">
      <h3 className="text-xl font-bold flex items-center gap-2">
        <Shield className="h-6 w-6 text-blue-600" />
        Compara Seguros
      </h3>

      {quotes?.map((quote) => (
        <div
          key={quote.quoteId}
          onClick={() => setSelectedQuote(quote.quoteId)}
          className={`border-2 rounded-xl p-4 cursor-pointer transition-all ${
            selectedQuote === quote.quoteId
              ? "border-blue-600 bg-blue-50"
              : "border-gray-200 hover:border-blue-300"
          }`}
        >
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-4">
              <img
                src={quote.insurerLogo}
                alt={quote.insurerName}
                className="h-12 w-auto"
              />
              <div>
                <h4 className="font-semibold">{quote.insurerName}</h4>
                <p className="text-sm text-gray-500">{quote.coverageType}</p>
              </div>
              {quote.isRecommended && (
                <span className="px-2 py-1 bg-green-100 text-green-800 text-xs font-semibold rounded-full">
                  ⭐ Recomendado
                </span>
              )}
            </div>
            <div className="text-right">
              <p className="text-2xl font-bold text-blue-600">
                {formatCurrency(quote.monthlyPremium)}/mes
              </p>
              <p className="text-sm text-gray-500">
                Deducible: {formatCurrency(quote.deductible)}
              </p>
            </div>
          </div>

          {/* Coverage Details */}
          <div className="mt-4 grid grid-cols-2 gap-2">
            {quote.coverages.slice(0, 4).map((coverage, idx) => (
              <div key={idx} className="flex items-center gap-2 text-sm">
                {coverage.isIncluded ? (
                  <CheckCircle className="h-4 w-4 text-green-500" />
                ) : (
                  <AlertTriangle className="h-4 w-4 text-gray-300" />
                )}
                <span
                  className={
                    coverage.isIncluded ? "text-gray-700" : "text-gray-400"
                  }
                >
                  {coverage.name}
                </span>
              </div>
            ))}
          </div>
        </div>
      ))}

      {selectedQuote && (
        <button className="w-full py-3 bg-green-600 text-white rounded-lg font-semibold hover:bg-green-700">
          Contratar Seguro Seleccionado
        </button>
      )}
    </div>
  );
};
```

---

## ✅ Testing (xUnit)

```csharp
public class SegurosReservasInsuranceServiceTests
{
    private readonly Mock<HttpClient> _mockHttpClient;
    private readonly IInsuranceService _service;

    [Fact]
    public async Task GetQuoteAsync_WithValidVehicle_ReturnsQuote()
    {
        var request = new QuoteRequest
        {
            Make = "Toyota",
            Model = "Corolla",
            Year = 2022,
            Mileage = 25000,
            VehicleUse = "Personal",
            CoverageType = CoverageType.Comprehensive
        };

        var quote = await _service.GetQuoteAsync(request);

        Assert.NotNull(quote);
        Assert.True(quote.MonthlyPremium > 0);
        Assert.NotEmpty(quote.Coverages);
    }

    [Fact]
    public async Task CompareInsurersAsync_ReturnsMultipleQuotes()
    {
        var request = new QuoteRequest { Make = "Honda", Model = "Civic", Year = 2023 };

        var quotes = await _service.CompareInsurersAsync(request);

        Assert.True(quotes.Length >= 2);
        Assert.Contains(quotes, q => q.IsRecommended);
    }

    [Fact]
    public async Task PurchasePolicyAsync_WithValidQuote_ReturnsActivePolicy()
    {
        var purchase = new PurchaseRequest { QuoteId = "valid-quote-id" };

        var policy = await _service.PurchasePolicyAsync(purchase);

        Assert.NotNull(policy);
        Assert.Equal(PolicyStatus.Active, policy.Status);
        Assert.NotEmpty(policy.PolicyNumber);
    }
}
```

---

## 🔧 Troubleshooting

| Problema                    | Causa                             | Solución                                   |
| --------------------------- | --------------------------------- | ------------------------------------------ |
| Cotización muy alta         | Vehículo de alto riesgo/deportivo | Ofrecer coberturas alternativas            |
| Póliza no emitida           | Documentos faltantes              | Mostrar checklist de documentos requeridos |
| Aseguradora no responde     | API timeout                       | Usar cache de cotizaciones recientes       |
| Cobertura rechazada         | Vehículo muy viejo (>15 años)     | Mostrar aseguradoras que aceptan usados    |
| Precio diferente a cotizado | Cambios en datos                  | Re-cotizar con datos actualizados          |
| Renovación fallida          | Póliza expirada >30 días          | Iniciar nueva póliza en lugar de renovar   |

---

## 🔗 Integración con OKLA

### 1. **Agregar InsuranceService**

```csharp
services.AddHttpClient<IInsuranceService, SegurosReservasInsuranceService>();
```

### 2. **Gateway routing**

```json
{
  "UpstreamPathTemplate": "/api/insurance/{everything}",
  "DownstreamPathTemplate": "/api/insurance/{everything}",
  "DownstreamHostAndPorts": [{ "Host": "insuranceservice", "Port": 8080 }]
}
```

### 3. **Mostrar en VehicleDetailPage**

```tsx
<InsuranceComparison vehicle={vehicle} />
```

### 4. **Bundle con financiamiento**

```csharp
// Incluir prima de seguro en la cuota mensual
var totalMonthly = loanQuote.MonthlyPayment + insuranceQuote.MonthlyPremium;
```

---

## 💰 Costos Estimados

| Aseguradora      | Costo API | Comisión Referido | Potencial/mes |
| ---------------- | --------- | ----------------- | ------------- |
| Seguros Reservas | Gratis    | 8-10%             | $5,000+       |
| Colonial         | Gratis    | 8-10%             | $3,000+       |
| Jerry.ai         | Gratis    | 5-8%              | $2,000+       |
| **TOTAL**        |           |                   | **$10,000+**  |

✅ **Modelo de Revenue:** OKLA gana comisión de 8-10% por cada póliza vendida a través del marketplace.
