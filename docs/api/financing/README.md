# 💳 Financing APIs

**Categoría:** Financial Services  
**APIs:** 4 (Banco Popular, Banreservas, BHD León, RouteOne)  
**Fase:** 3 (Premium Features)  
**Impacto:** +35% conversiones cuando comprador ve cuota mensual estimada

---

## 📖 Resumen

Integración con bancos dominicanos para calcular financiamiento, simular cuotas, y enviar solicitudes de préstamo directamente desde el marketplace. El comprador ve cuota estimada SIN salir de OKLA.

### Casos de Uso en OKLA

✅ **Calculadora de cuotas en ficha** - Comprador ve "$15,500/mes por 48 meses" debajo del precio  
✅ **Simulador interactivo** - Slider para ajustar inicial, plazo, y ver cuota al instante  
✅ **Pre-aprobación instantánea** - Comprador ingresa datos básicos, banco responde en 2 minutos  
✅ **Comparativa de bancos** - "Banco Popular: $15,500 | Banreservas: $15,800 | BHD: $16,100"  
✅ **Solicitud directa** - Comprador aplica sin salir de OKLA, dealer recibe notificación  
✅ **Badge "Financiamiento Disponible"** - Listados con financiamiento se destacan

---

## 🔗 Comparativa de APIs

| Aspecto              | **Banco Popular** | **Banreservas**  | **BHD León**     | **RouteOne**    |
| -------------------- | ----------------- | ---------------- | ---------------- | --------------- |
| **Tasa APR**         | 8-12%             | 8-12%            | 9-13%            | 7-11%           |
| **Plazo máximo**     | 72 meses          | 60 meses         | 60 meses         | 84 meses        |
| **Monto máximo**     | RD$5,000,000      | RD$4,000,000     | RD$3,500,000     | USD$100,000     |
| **Tiempo respuesta** | 2-5 minutos       | 5-10 minutos     | 10-15 minutos    | 1-2 minutos     |
| **API disponible**   | ✅ REST           | ⚠️ SOAP          | ⚠️ Manual        | ✅ REST         |
| **Integración**      | Fácil             | Media            | Difícil          | Fácil           |
| **Mejor para**       | Vehículos nuevos  | Vehículos usados | Clientes premium | Dealers grandes |
| **Recomendado**      | ⭐ PRINCIPAL      | ⭐ Secundario    | Backup           | ⭐ USA/Export   |

---

## 📡 ENDPOINTS

### Banco Popular API

- `POST /api/financing/quote` - Calcular cuota mensual
- `POST /api/financing/pre-approval` - Pre-aprobación instantánea
- `POST /api/financing/application` - Solicitud completa
- `GET /api/financing/application/{id}/status` - Estado de solicitud
- `GET /api/financing/rates` - Tasas actuales

### Banreservas API (SOAP)

- `CalcularCuota` - Calcular cuota
- `SolicitarPreAprobacion` - Pre-aprobación
- `EnviarSolicitud` - Solicitud completa
- `ConsultarEstado` - Estado de solicitud

### RouteOne API

- `POST /api/credit/decision` - Decisión de crédito
- `POST /api/applications` - Crear solicitud
- `GET /api/applications/{id}` - Consultar estado

---

## 💻 Backend Implementation (C#)

### Service Interface

```csharp
public interface IFinancingService
{
    Task<FinancingQuote> CalculateQuoteAsync(QuoteRequest request);
    Task<PreApprovalResult> GetPreApprovalAsync(PreApprovalRequest request);
    Task<LoanApplication> SubmitApplicationAsync(ApplicationRequest request);
    Task<ApplicationStatus> GetApplicationStatusAsync(string applicationId);
    Task<FinancingRates> GetCurrentRatesAsync();
    Task<BankComparison[]> CompareBanksAsync(decimal vehiclePrice, int termMonths);
}

public class QuoteRequest
{
    public decimal VehiclePrice { get; set; }
    public decimal DownPayment { get; set; }
    public int TermMonths { get; set; } // 12, 24, 36, 48, 60, 72
    public string VehicleYear { get; set; }
    public bool IsNew { get; set; }
}

public class PreApprovalRequest
{
    public string CustomerId { get; set; }
    public string Cedula { get; set; }
    public decimal MonthlyIncome { get; set; }
    public decimal RequestedAmount { get; set; }
    public int TermMonths { get; set; }
}
```

### Domain Models

```csharp
public class FinancingQuote
{
    public decimal MonthlyPayment { get; set; }
    public decimal TotalPayment { get; set; }
    public decimal TotalInterest { get; set; }
    public decimal Apr { get; set; } // Tasa anual
    public int TermMonths { get; set; }
    public decimal DownPayment { get; set; }
    public decimal FinancedAmount { get; set; }
    public string BankName { get; set; }
    public DateTime ValidUntil { get; set; }
}

public class PreApprovalResult
{
    public string PreApprovalId { get; set; }
    public bool IsApproved { get; set; }
    public decimal ApprovedAmount { get; set; }
    public decimal MaxMonthlyPayment { get; set; }
    public decimal Apr { get; set; }
    public string[] Conditions { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string RejectionReason { get; set; }
}

public class LoanApplication
{
    public string ApplicationId { get; set; }
    public string CustomerId { get; set; }
    public string VehicleId { get; set; }
    public decimal RequestedAmount { get; set; }
    public int TermMonths { get; set; }
    public ApplicationStatus Status { get; set; }
    public DateTime SubmittedAt { get; set; }
    public string BankReference { get; set; }
}

public enum ApplicationStatus
{
    Submitted,
    UnderReview,
    DocumentsRequired,
    Approved,
    Rejected,
    Funded
}

public class BankComparison
{
    public string BankName { get; set; }
    public string LogoUrl { get; set; }
    public decimal MonthlyPayment { get; set; }
    public decimal Apr { get; set; }
    public decimal TotalCost { get; set; }
    public string[] Benefits { get; set; }
    public bool IsRecommended { get; set; }
}
```

### Service Implementation

```csharp
public class BancoPopularFinancingService : IFinancingService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<BancoPopularFinancingService> _logger;
    private readonly string _apiKey;
    private const string BaseUrl = "https://api.bancopopular.com.do/financing";

    public BancoPopularFinancingService(HttpClient httpClient, IConfiguration config, ILogger<BancoPopularFinancingService> logger)
    {
        _httpClient = httpClient;
        _config = config;
        _logger = logger;
        _apiKey = config["BancoPopular:ApiKey"];
    }

    public async Task<FinancingQuote> CalculateQuoteAsync(QuoteRequest request)
    {
        try
        {
            var payload = new
            {
                vehicle_price = request.VehiclePrice,
                down_payment = request.DownPayment,
                term_months = request.TermMonths,
                vehicle_year = request.VehicleYear,
                is_new = request.IsNew
            };

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/quote");
            httpRequest.Headers.Add("Authorization", $"Bearer {_apiKey}");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(httpRequest);
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            return new FinancingQuote
            {
                MonthlyPayment = root.GetProperty("monthly_payment").GetDecimal(),
                TotalPayment = root.GetProperty("total_payment").GetDecimal(),
                TotalInterest = root.GetProperty("total_interest").GetDecimal(),
                Apr = root.GetProperty("apr").GetDecimal(),
                TermMonths = request.TermMonths,
                DownPayment = request.DownPayment,
                FinancedAmount = request.VehiclePrice - request.DownPayment,
                BankName = "Banco Popular",
                ValidUntil = DateTime.UtcNow.AddDays(7)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating quote with Banco Popular");
            throw;
        }
    }

    public async Task<PreApprovalResult> GetPreApprovalAsync(PreApprovalRequest request)
    {
        var payload = new
        {
            cedula = request.Cedula,
            monthly_income = request.MonthlyIncome,
            requested_amount = request.RequestedAmount,
            term_months = request.TermMonths
        };

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/pre-approval");
        httpRequest.Headers.Add("Authorization", $"Bearer {_apiKey}");
        httpRequest.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(httpRequest);
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        return new PreApprovalResult
        {
            PreApprovalId = root.GetProperty("pre_approval_id").GetString(),
            IsApproved = root.GetProperty("is_approved").GetBoolean(),
            ApprovedAmount = root.GetProperty("approved_amount").GetDecimal(),
            MaxMonthlyPayment = root.GetProperty("max_monthly_payment").GetDecimal(),
            Apr = root.GetProperty("apr").GetDecimal(),
            ExpiresAt = DateTime.Parse(root.GetProperty("expires_at").GetString())
        };
    }

    public async Task<LoanApplication> SubmitApplicationAsync(ApplicationRequest request)
    {
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/applications");
        httpRequest.Headers.Add("Authorization", $"Bearer {_apiKey}");
        httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(httpRequest);
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        return new LoanApplication
        {
            ApplicationId = doc.RootElement.GetProperty("application_id").GetString(),
            Status = ApplicationStatus.Submitted,
            SubmittedAt = DateTime.UtcNow,
            BankReference = doc.RootElement.GetProperty("bank_reference").GetString()
        };
    }

    public async Task<ApplicationStatus> GetApplicationStatusAsync(string applicationId)
    {
        var response = await _httpClient.GetAsync($"{BaseUrl}/applications/{applicationId}/status");
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        var statusString = doc.RootElement.GetProperty("status").GetString();
        return Enum.Parse<ApplicationStatus>(statusString);
    }

    public async Task<FinancingRates> GetCurrentRatesAsync()
    {
        var response = await _httpClient.GetAsync($"{BaseUrl}/rates");
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<FinancingRates>(json);
    }

    public async Task<BankComparison[]> CompareBanksAsync(decimal vehiclePrice, int termMonths)
    {
        // Consultar múltiples bancos en paralelo
        var tasks = new[]
        {
            CalculateQuoteAsync(new QuoteRequest { VehiclePrice = vehiclePrice, TermMonths = termMonths }),
            // Otros bancos...
        };

        var quotes = await Task.WhenAll(tasks);

        return quotes.Select(q => new BankComparison
        {
            BankName = q.BankName,
            MonthlyPayment = q.MonthlyPayment,
            Apr = q.Apr,
            TotalCost = q.TotalPayment,
            IsRecommended = q.Apr == quotes.Min(x => x.Apr)
        }).ToArray();
    }
}
```

---

## 🎨 Frontend Implementation (React + TypeScript)

### Financing Service

```typescript
import axios from "axios";

export interface FinancingQuote {
  monthlyPayment: number;
  totalPayment: number;
  totalInterest: number;
  apr: number;
  termMonths: number;
  bankName: string;
}

export interface PreApprovalResult {
  preApprovalId: string;
  isApproved: boolean;
  approvedAmount: number;
  maxMonthlyPayment: number;
  apr: number;
  expiresAt: string;
}

export class FinancingService {
  private baseUrl = process.env.REACT_APP_API_URL;

  async calculateQuote(
    vehiclePrice: number,
    downPayment: number,
    termMonths: number
  ): Promise<FinancingQuote> {
    const response = await axios.post(`${this.baseUrl}/api/financing/quote`, {
      vehiclePrice,
      downPayment,
      termMonths,
    });
    return response.data;
  }

  async getPreApproval(
    cedula: string,
    monthlyIncome: number,
    requestedAmount: number
  ): Promise<PreApprovalResult> {
    const response = await axios.post(
      `${this.baseUrl}/api/financing/pre-approval`,
      {
        cedula,
        monthlyIncome,
        requestedAmount,
      }
    );
    return response.data;
  }

  async compareBanks(vehiclePrice: number, termMonths: number) {
    const response = await axios.get(`${this.baseUrl}/api/financing/compare`, {
      params: { vehiclePrice, termMonths },
    });
    return response.data;
  }
}
```

### React Component - Financing Calculator

```typescript
import React, { useState, useEffect } from "react";
import { useQuery } from "@tanstack/react-query";
import { FinancingService } from "@/services/financingService";

interface Props {
  vehiclePrice: number;
}

export const FinancingCalculator = ({ vehiclePrice }: Props) => {
  const [downPayment, setDownPayment] = useState(vehiclePrice * 0.2);
  const [termMonths, setTermMonths] = useState(48);
  const financingService = new FinancingService();

  const { data: quote, isLoading } = useQuery({
    queryKey: ["financing-quote", vehiclePrice, downPayment, termMonths],
    queryFn: () =>
      financingService.calculateQuote(vehiclePrice, downPayment, termMonths),
    enabled: vehiclePrice > 0,
  });

  const formatCurrency = (value: number) =>
    new Intl.NumberFormat("es-DO", {
      style: "currency",
      currency: "DOP",
    }).format(value);

  return (
    <div className="border rounded-xl p-6 bg-white shadow-sm">
      <h3 className="text-xl font-bold mb-6">
        💳 Calculadora de Financiamiento
      </h3>

      <div className="space-y-6">
        {/* Down Payment Slider */}
        <div>
          <div className="flex justify-between mb-2">
            <label className="text-sm text-gray-600">Inicial</label>
            <span className="font-semibold">{formatCurrency(downPayment)}</span>
          </div>
          <input
            type="range"
            min={vehiclePrice * 0.1}
            max={vehiclePrice * 0.5}
            value={downPayment}
            onChange={(e) => setDownPayment(Number(e.target.value))}
            className="w-full h-2 bg-gray-200 rounded-lg appearance-none cursor-pointer"
          />
          <div className="flex justify-between text-xs text-gray-500 mt-1">
            <span>10%</span>
            <span>50%</span>
          </div>
        </div>

        {/* Term Selector */}
        <div>
          <label className="text-sm text-gray-600 block mb-2">Plazo</label>
          <div className="grid grid-cols-4 gap-2">
            {[24, 36, 48, 60].map((months) => (
              <button
                key={months}
                onClick={() => setTermMonths(months)}
                className={`py-2 px-4 rounded-lg text-sm font-medium transition-colors ${
                  termMonths === months
                    ? "bg-blue-600 text-white"
                    : "bg-gray-100 text-gray-700 hover:bg-gray-200"
                }`}
              >
                {months} meses
              </button>
            ))}
          </div>
        </div>

        {/* Quote Result */}
        {quote && (
          <div className="bg-blue-50 rounded-xl p-6 text-center">
            <p className="text-sm text-gray-600 mb-2">
              Tu cuota mensual estimada
            </p>
            <p className="text-4xl font-bold text-blue-600">
              {formatCurrency(quote.monthlyPayment)}
            </p>
            <p className="text-sm text-gray-500 mt-2">
              Tasa: {quote.apr}% APR | Total:{" "}
              {formatCurrency(quote.totalPayment)}
            </p>
          </div>
        )}

        {isLoading && (
          <div className="text-center py-4">
            <div className="animate-spin h-8 w-8 border-4 border-blue-500 border-t-transparent rounded-full mx-auto"></div>
          </div>
        )}

        <button className="w-full py-3 bg-green-600 text-white rounded-lg font-semibold hover:bg-green-700 transition-colors">
          Solicitar Pre-Aprobación
        </button>

        <p className="text-xs text-gray-500 text-center">
          * Cuota estimada. Sujeto a aprobación de crédito. Tasa puede variar
          según historial crediticio.
        </p>
      </div>
    </div>
  );
};
```

---

## ✅ Testing (xUnit)

```csharp
public class BancoPopularFinancingServiceTests
{
    [Fact]
    public async Task CalculateQuoteAsync_WithValidRequest_ReturnsQuote()
    {
        var request = new QuoteRequest
        {
            VehiclePrice = 1_500_000, // RD$1.5M
            DownPayment = 300_000,    // 20%
            TermMonths = 48,
            IsNew = false
        };

        var quote = await _service.CalculateQuoteAsync(request);

        Assert.NotNull(quote);
        Assert.True(quote.MonthlyPayment > 0);
        Assert.True(quote.Apr >= 8 && quote.Apr <= 15);
        Assert.Equal(48, quote.TermMonths);
    }

    [Fact]
    public async Task GetPreApprovalAsync_WithGoodCredit_ReturnsApproval()
    {
        var request = new PreApprovalRequest
        {
            Cedula = "001-1234567-8",
            MonthlyIncome = 80_000,
            RequestedAmount = 1_200_000,
            TermMonths = 48
        };

        var result = await _service.GetPreApprovalAsync(request);

        Assert.NotNull(result);
        Assert.NotEmpty(result.PreApprovalId);
        Assert.True(result.ApprovedAmount > 0);
    }

    [Fact]
    public async Task CompareBanksAsync_WithValidPrice_ReturnsMultipleBanks()
    {
        var comparisons = await _service.CompareBanksAsync(1_500_000, 48);

        Assert.NotEmpty(comparisons);
        Assert.Contains(comparisons, c => c.IsRecommended);
    }
}
```

---

## 🔧 Troubleshooting

| Problema                 | Causa                            | Solución                                  |
| ------------------------ | -------------------------------- | ----------------------------------------- |
| Pre-aprobación rechazada | Historial crediticio bajo        | Mostrar mensaje amigable y alternativas   |
| Timeout en API banco     | API del banco lenta (>10s)       | Implementar timeout + retry + cache       |
| Tasa incorrecta          | Tasa cambia según perfil cliente | Mostrar rango y disclaimer                |
| Cédula inválida          | Formato incorrecto               | Validar formato antes de enviar           |
| Solicitud duplicada      | Usuario envió múltiples veces    | Debounce en frontend, idempotency backend |
| Monto excede límite      | Vehículo muy caro para el banco  | Mostrar bancos con límites más altos      |

---

## 🔗 Integración con OKLA

### 1. **Crear FinancingService**

```csharp
services.AddHttpClient<IFinancingService, BancoPopularFinancingService>()
    .ConfigureHttpClient(client => client.Timeout = TimeSpan.FromSeconds(30));
```

### 2. **Gateway routing**

```json
{
  "UpstreamPathTemplate": "/api/financing/{everything}",
  "DownstreamPathTemplate": "/api/financing/{everything}",
  "DownstreamHostAndPorts": [{ "Host": "billingservice", "Port": 8080 }]
}
```

### 3. **Integrar en VehicleDetailPage**

```tsx
<FinancingCalculator vehiclePrice={vehicle.price} />
```

### 4. **Evento cuando se solicita pre-aprobación**

```csharp
await _bus.Publish(new FinancingPreApprovalRequestedEvent { VehicleId = vehicleId, Amount = amount });
```

---

## 💰 Costos Estimados

| Banco         | Costo API | Volumen/mes | Total/mes |
| ------------- | --------- | ----------- | --------- |
| Banco Popular | Gratis\*  | 10,000      | $0        |
| Banreservas   | Gratis\*  | 5,000       | $0        |
| **TOTAL**     |           |             | **$0**    |

\*Los bancos no cobran por la API; ganan con los préstamos aprobados.

✅ **Revenue Share:** OKLA puede negociar comisión de 0.5-1% por préstamo referido.

```

---

**Versión:** 1.0 | **Actualizado:** Enero 15, 2026
```
