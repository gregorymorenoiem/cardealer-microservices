# 📊 Vehicle History APIs

**Categoría:** Vehicle Data & History  
**APIs:** 3 (Carfax, AutoCheck, VINAudit)  
**Fase:** 2 (Diferenciación)  
**Impacto:** Genera confianza en compradores, reduce fraude

---

## 📖 Resumen

Acceso a históricos de vehículos para validar condición, accidentes previos, mantenimiento y detectar vehículos con problemas graves. Es crítico para la confianza del comprador y prevención de fraude.

### Casos de Uso en OKLA

✅ **Reporte completo en ficha del vehículo** - Comprador ve historial de accidentes, reparaciones, títulos salvage  
✅ **Detección automática de salvage titles** - Sistema rechaza vehículos con títulos problemáticos  
✅ **Badge "Historial limpio"** - Los dealer con historial limpio se destacan (40% más clics)  
✅ **Alerta de fraude** - Sistema marca si hay discrepancias (e.g., odómetro anómalo)  
✅ **Integración con pricing** - Vehículos con historial limpio se valúan más alto

---

## 🔗 Comparativa de APIs

| Aspecto             | **Carfax**        | **AutoCheck**    | **VINAudit**        |
| ------------------- | ----------------- | ---------------- | ------------------- |
| **Costo**           | $15-30/reporte    | $10-20/reporte   | $5-10/reporte       |
| **Cobertura**       | 98% USA           | 95% USA          | 90% USA             |
| **Datos**           | Accidentes, Maint | Salvage, Títulos | Básico + Recall     |
| **Velocidad API**   | 2-5 segundos      | 1-3 segundos     | <1 segundo          |
| **Actualizaciones** | Real-time         | Daily            | Weekly              |
| **Mejor para**      | Históricos detall | Legal/Compliance | Verificación rápida |
| **Recomendado**     | ⭐⭐⭐⭐⭐        | ⭐⭐⭐⭐         | ⭐⭐⭐              |

---

## 📡 ENDPOINTS

### Carfax API

- `GET /reports/{vin}` - Reporte completo del vehículo
- `GET /reports/{vin}/accident-summary` - Solo accidentes
- `GET /reports/{vin}/service-records` - Historial de servicio
- `GET /reports/{vin}/ownership` - Historial de propietarios
- `POST /bulk-reports` - Múltiples reportes en batch

### AutoCheck API

- `GET /vehiclehistory/{vin}` - Reporte con títulos
- `GET /vehiclehistory/{vin}/accidents` - Accidentes registrados
- `GET /vehiclehistory/{vin}/brands` - Titles y marcas negativas
- `GET /alerts/{vin}` - Alertas de fraude

### VINAudit API

- `GET /decode/{vin}` - Información básica + recalls
- `GET /history/{vin}` - Historial simple
- `GET /recalls/{vin}` - NHTSA recalls solo

---

## 💻 Backend Implementation (C#)

### Service Interface

```csharp
public interface IVehicleHistoryService
{
    Task<VehicleHistoryResult> GetFullHistoryAsync(string vin);
    Task<AccidentSummary> GetAccidentSummaryAsync(string vin);
    Task<bool> HasSalvageTitleAsync(string vin);
    Task<FraudAlert[]> CheckForFraudAsync(string vin);
    Task<HistoryReport> GetCarfaxReportAsync(string vin);
}
```

### Domain Models

```csharp
public class VehicleHistoryResult
{
    public string Vin { get; set; }
    public int AccidentCount { get; set; }
    public DateTime FirstAccident { get; set; }
    public string[] ClaimTypes { get; set; } // "Collision", "Flood", "Fire"
    public int OwnershipCount { get; set; }
    public bool HasSalvageTitle { get; set; }
    public ServiceRecord[] ServiceRecords { get; set; }
    public string OverallGrade { get; set; } // "Excellent", "Good", "Fair", "Poor"
    public DateTime ReportGeneratedDate { get; set; }
}

public class AccidentSummary
{
    public int TotalAccidents { get; set; }
    public AccidentRecord[] Accidents { get; set; }
    public int TotalClaimValue { get; set; }
    public bool IsSevere { get; set; }
}

public class AccidentRecord
{
    public string Type { get; set; } // "Collision", "Structural Damage", etc.
    public DateTime Date { get; set; }
    public int EstimatedRepairCost { get; set; }
    public string Source { get; set; } // "Insurance Claim", "Police Report"
}

public class FraudAlert
{
    public string AlertType { get; set; } // "Odometer Discrepancy", "Title Washing", "Flood Damage"
    public string Severity { get; set; } // "Critical", "High", "Medium"
    public string Description { get; set; }
    public DateTime DetectedDate { get; set; }
}

public class ServiceRecord
{
    public DateTime Date { get; set; }
    public string Mileage { get; set; }
    public string ServiceType { get; set; }
    public string Provider { get; set; }
}
```

### Service Implementation

```csharp
public class CarfaxVehicleHistoryService : IVehicleHistoryService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<CarfaxVehicleHistoryService> _logger;
    private string _apiKey;
    private string _baseUrl = "https://api.carfaxonline.com/api";

    public CarfaxVehicleHistoryService(
        HttpClient httpClient,
        IConfiguration config,
        ILogger<CarfaxVehicleHistoryService> logger)
    {
        _httpClient = httpClient;
        _config = config;
        _logger = logger;
        _apiKey = config["Carfax:ApiKey"];
    }

    public async Task<VehicleHistoryResult> GetFullHistoryAsync(string vin)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get,
                $"{_baseUrl}/reports/{vin}");
            request.Headers.Add("Authorization", $"Bearer {_apiKey}");
            request.Headers.Add("X-Request-ID", Guid.NewGuid().ToString());

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                throw new Exception($"Carfax API error: {response.StatusCode}");

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            return new VehicleHistoryResult
            {
                Vin = vin,
                AccidentCount = root.GetProperty("accidentCount").GetInt32(),
                HasSalvageTitle = root.GetProperty("titles")
                    .EnumerateArray()
                    .Any(t => t.GetProperty("type").GetString() == "Salvage"),
                OverallGrade = root.GetProperty("overallGrade").GetString(),
                ReportGeneratedDate = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Carfax history for VIN {vin}", vin);
            throw;
        }
    }

    public async Task<AccidentSummary> GetAccidentSummaryAsync(string vin)
    {
        var request = new HttpRequestMessage(HttpMethod.Get,
            $"{_baseUrl}/reports/{vin}/accidents");
        request.Headers.Add("Authorization", $"Bearer {_apiKey}");

        var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);

        var accidents = doc.RootElement.GetProperty("accidents")
            .EnumerateArray()
            .Select(a => new AccidentRecord
            {
                Type = a.GetProperty("type").GetString(),
                Date = DateTime.Parse(a.GetProperty("date").GetString()),
                EstimatedRepairCost = a.GetProperty("estimatedCost").GetInt32()
            })
            .ToArray();

        return new AccidentSummary
        {
            TotalAccidents = accidents.Length,
            Accidents = accidents,
            IsSevere = accidents.Any(a => a.EstimatedRepairCost > 10000)
        };
    }

    public async Task<bool> HasSalvageTitleAsync(string vin)
    {
        var history = await GetFullHistoryAsync(vin);
        return history.HasSalvageTitle;
    }

    public async Task<FraudAlert[]> CheckForFraudAsync(string vin)
    {
        var request = new HttpRequestMessage(HttpMethod.Get,
            $"{_baseUrl}/reports/{vin}/fraud-check");
        request.Headers.Add("Authorization", $"Bearer {_apiKey}");

        var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);

        return doc.RootElement.GetProperty("alerts")
            .EnumerateArray()
            .Select(alert => new FraudAlert
            {
                AlertType = alert.GetProperty("type").GetString(),
                Severity = alert.GetProperty("severity").GetString(),
                Description = alert.GetProperty("description").GetString(),
                DetectedDate = DateTime.Parse(alert.GetProperty("detectedAt").GetString())
            })
            .ToArray();
    }

    public async Task<HistoryReport> GetCarfaxReportAsync(string vin)
    {
        var history = await GetFullHistoryAsync(vin);
        var accidents = await GetAccidentSummaryAsync(vin);
        var fraud = await CheckForFraudAsync(vin);

        return new HistoryReport
        {
            Vin = vin,
            History = history,
            Accidents = accidents,
            FraudAlerts = fraud,
            GeneratedAt = DateTime.UtcNow
        };
    }
}
```

### CQRS Commands

```csharp
public class GetVehicleHistoryCommand : IRequest<Result<VehicleHistoryResult>>
{
    public string Vin { get; set; }
}

public class GetVehicleHistoryCommandHandler : IRequestHandler<GetVehicleHistoryCommand, Result<VehicleHistoryResult>>
{
    private readonly IVehicleHistoryService _historyService;

    public GetVehicleHistoryCommandHandler(IVehicleHistoryService historyService)
    {
        _historyService = historyService;
    }

    public async Task<Result<VehicleHistoryResult>> Handle(GetVehicleHistoryCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Vin) || request.Vin.Length != 17)
            return Result<VehicleHistoryResult>.Failure("VIN inválido");

        var history = await _historyService.GetFullHistoryAsync(request.Vin);
        return Result<VehicleHistoryResult>.Success(history);
    }
}
```

---

## 🎨 Frontend Implementation (React + TypeScript)

### Service API

```typescript
import axios from "axios";

export interface VehicleHistory {
  vin: string;
  accidentCount: number;
  hasSalvageTitle: boolean;
  overallGrade: "Excellent" | "Good" | "Fair" | "Poor";
  serviceRecords: ServiceRecord[];
}

export interface ServiceRecord {
  date: string;
  mileage: string;
  serviceType: string;
  provider: string;
}

export class VehicleHistoryService {
  private baseUrl = process.env.REACT_APP_API_URL;

  async getHistory(vin: string): Promise<VehicleHistory> {
    const response = await axios.get(
      `${this.baseUrl}/api/vehicle-history/${vin}`,
      {
        headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
      }
    );
    return response.data;
  }

  async getAccidentSummary(vin: string) {
    const response = await axios.get(
      `${this.baseUrl}/api/vehicle-history/${vin}/accidents`
    );
    return response.data;
  }

  async checkFraud(vin: string) {
    const response = await axios.get(
      `${this.baseUrl}/api/vehicle-history/${vin}/fraud-check`
    );
    return response.data;
  }
}
```

### React Component

```typescript
import React, { useEffect, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { VehicleHistoryService } from "@/services/vehicleHistoryService";

export const VehicleHistoryCard = ({ vin }: { vin: string }) => {
  const historyService = new VehicleHistoryService();
  const {
    data: history,
    isLoading,
    error,
  } = useQuery({
    queryKey: ["vehicleHistory", vin],
    queryFn: () => historyService.getHistory(vin),
  });

  if (isLoading) return <div>Cargando historial...</div>;
  if (error) return <div>Error cargando historial</div>;

  const getGradeColor = (grade: string) => {
    const colors: Record<string, string> = {
      Excellent: "text-green-600",
      Good: "text-blue-600",
      Fair: "text-yellow-600",
      Poor: "text-red-600",
    };
    return colors[grade] || "text-gray-600";
  };

  return (
    <div className="border rounded-lg p-6 bg-white shadow-sm">
      <h3 className="text-xl font-bold mb-4">Historial del Vehículo</h3>

      <div className="grid grid-cols-3 gap-4 mb-6">
        <div>
          <p className="text-sm text-gray-600">Calificación</p>
          <p
            className={`text-2xl font-bold ${getGradeColor(
              history.overallGrade
            )}`}
          >
            {history.overallGrade}
          </p>
        </div>
        <div>
          <p className="text-sm text-gray-600">Accidentes</p>
          <p className="text-2xl font-bold">{history.accidentCount}</p>
        </div>
        <div>
          <p className="text-sm text-gray-600">Título Salvage</p>
          <p
            className={`text-lg font-bold ${
              history.hasSalvageTitle ? "text-red-600" : "text-green-600"
            }`}
          >
            {history.hasSalvageTitle ? "⚠️ Sí" : "✅ No"}
          </p>
        </div>
      </div>

      {history.serviceRecords.length > 0 && (
        <div>
          <h4 className="font-semibold mb-3">Historial de Servicio</h4>
          <div className="space-y-2">
            {history.serviceRecords.map((record, i) => (
              <div key={i} className="text-sm bg-gray-50 p-3 rounded">
                <div className="flex justify-between">
                  <span>{record.serviceType}</span>
                  <span className="text-gray-600">{record.date}</span>
                </div>
                <div className="text-gray-600">{record.provider}</div>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
};
```

---

## ✅ Testing (xUnit)

```csharp
public class CarfaxVehicleHistoryServiceTests
{
    private readonly Mock<HttpClient> _httpClientMock;
    private readonly Mock<IConfiguration> _configMock;
    private readonly Mock<ILogger<CarfaxVehicleHistoryService>> _loggerMock;
    private CarfaxVehicleHistoryService _service;

    public CarfaxVehicleHistoryServiceTests()
    {
        _httpClientMock = new Mock<HttpClient>();
        _configMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<CarfaxVehicleHistoryService>>();
    }

    [Fact]
    public async Task GetFullHistoryAsync_WithValidVin_ReturnsHistory()
    {
        // Arrange
        var vin = "1HGCM41JXMA109186";
        var mockResponse = new HttpResponseMessage
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            Content = new StringContent("{\"accidentCount\": 2, \"overallGrade\": \"Good\"}")
        };
        // Act
        var result = await _service.GetFullHistoryAsync(vin);
        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.AccidentCount);
        Assert.Equal("Good", result.OverallGrade);
    }

    [Fact]
    public async Task HasSalvageTitleAsync_WithSalvageVehicle_ReturnsTrue()
    {
        var vin = "SALVAGED123456789";
        var result = await _service.HasSalvageTitleAsync(vin);
        Assert.True(result);
    }

    [Fact]
    public async Task CheckForFraudAsync_WithFraudVehicle_ReturnsAlerts()
    {
        var vin = "FRAUD1234567890AB";
        var alerts = await _service.CheckForFraudAsync(vin);
        Assert.NotEmpty(alerts);
        Assert.Contains(alerts, a => a.AlertType == "Odometer Discrepancy");
    }
}
```

---

## 🔧 Troubleshooting

| Problema                     | Causa                           | Solución                                    |
| ---------------------------- | ------------------------------- | ------------------------------------------- |
| API retorna 401 Unauthorized | API key inválida o expirada     | Verificar en `.env`: `Carfax:ApiKey`        |
| VIN no encontrado (404)      | VIN no existe en base de Carfax | Validar VIN con NHTSA primero               |
| Timeout > 5 segundos         | API de Carfax lenta o red lenta | Implementar caché de 24 horas               |
| Datos inconsistentes         | VIN con múltiples registros     | Usar fecha más reciente de reportes         |
| Rate limit (429)             | Más de 100 requests/minuto      | Implementar queue con backoff exponencial   |
| Salvage title no detectado   | Dato no en base de Carfax       | Verificar también con AutoCheck como backup |
| Accidentes sin detalles      | API response incompleta         | Hacer segundo request a `/accidents`        |
| Service records vacíos       | Vehículo nuevo sin historial    | Mostrar mensaje "Sin historial de servicio" |

---

## 🔗 Integración con OKLA

### 1. **Crear VehicleHistoryService en programa principal**

```csharp
// Program.cs
services.AddHttpClient<IVehicleHistoryService, CarfaxVehicleHistoryService>()
    .ConfigureHttpClient(client => client.Timeout = TimeSpan.FromSeconds(10))
    .AddPolicyHandler(GetRetryPolicy());
```

### 2. **Agregar rutas en Gateway (ocelot.prod.json)**

```json
{
  "DownstreamPathTemplate": "/api/vehicle-history/{vin}",
  "DownstreamScheme": "http",
  "DownstreamHostAndPorts": [{ "Host": "vehiclessaleservice", "Port": 8080 }],
  "UpstreamPathTemplate": "/api/vehicle-history/{vin}",
  "UpstreamHttpMethod": ["GET"]
}
```

### 3. **Integrar en VehicleDetailPage (Frontend)**

```tsx
import { VehicleHistoryCard } from "@/components/VehicleHistoryCard";

export const VehicleDetailPage = ({ vehicleId }) => {
  const { data: vehicle } = useVehicle(vehicleId);

  return (
    <div>
      <VehicleImageGallery images={vehicle.images} />
      <VehicleHistoryCard vin={vehicle.vin} />
      <PricingCard vehicle={vehicle} />
    </div>
  );
};
```

### 4. **Publicar evento en RabbitMQ cuando se solicite historial**

```csharp
public class VehicleHistoryRequestedEvent
{
    public string Vin { get; set; }
    public string RequestedBy { get; set; }
    public DateTime RequestedAt { get; set; }
}

await _bus.Publish(new VehicleHistoryRequestedEvent { Vin = vin });
```

### 5. **Caché de 24 horas en Redis**

```csharp
var cacheKey = $"vehicle:history:{vin}";
var cached = await _cache.GetAsync<VehicleHistory>(cacheKey);
if (cached != null) return cached;

var history = await _carfaxService.GetFullHistoryAsync(vin);
await _cache.SetAsync(cacheKey, history, TimeSpan.FromHours(24));
return history;
```

### 6. **Analytics: Registrar cuando se ve historial**

```csharp
await _analytics.TrackEvent(new {
    EventName = "VehicleHistoryViewed",
    VehicleId = vehicleId,
    Grade = history.OverallGrade,
    HasAccidents = history.AccidentCount > 0
});
```

---

## 💰 Costos Estimados

| API       | Costo/Reporte | Vol. Mensual | Total Mensual | Anual        |
| --------- | ------------- | ------------ | ------------- | ------------ |
| Carfax    | $20           | 2,000        | $40,000       | $480,000     |
| AutoCheck | $15           | 1,000        | $15,000       | $180,000     |
| VINAudit  | $7            | 500          | $3,500        | $42,000      |
| **TOTAL** |               |              | **$58,500**   | **$702,000** |

⚠️ **Nota:** Con 5,000 dealers activos, cada uno generando 2-3 reportes/mes = 10,000-15,000 reportes.
Optimizar: Generar reportes solo cuando se vende el vehículo (no en listing inicial)
}
}

```

---

**Versión:** 1.0 | **Actualizado:** Enero 15, 2026
```
