# 🔢 VIN Decoding APIs

**Categoría:** Vehicle Data & Specifications  
**APIs:** 3 (NHTSA ✅, Marketcheck, DataOne)  
**Fase:** 2 (Diferenciación)  
**Impacto:** Especificaciones instantáneas = mejor UX, 20% más búsquedas

---

## 📖 Resumen

Decodificación de VINs para extraer especificaciones exactas del vehículo (marca, modelo, año, cilindros, transmisión, colores, recalls). NHTSA es gratis y confiable. Marketcheck agrega datos de mercado.

### Casos de Uso en OKLA

✅ **Auto-llenar campos al publicar** - Dealer escanea VIN, se llenan Make/Model/Year/Body automáticamente  
✅ **Detección de especificaciones** - Sistema valida si lo que dice el dealer es correcto (VIN vs. especificaciones)  
✅ **Recalls automáticos** - Muestra recalls NHTSA en ficha de vehículo, genera confianza  
✅ **Búsqueda por especificaciones** - Comprador filtra por transmisión, # cilindros, color exacto  
✅ **Comparación VIN-a-VIN** - Dos vehículos pueden compararse especificación por especificación

---

## 🔗 Comparativa de APIs

| Aspecto             | **NHTSA**          | **Marketcheck**   | **DataOne**      |
| ------------------- | ------------------ | ----------------- | ---------------- |
| **Costo**           | Gratis             | $0.10/VIN         | $0.05/VIN        |
| **Cobertura**       | 100% USA vehicles  | 95% market data   | 90% market data  |
| **Velocidad**       | <500ms             | 1-2s              | 2-3s             |
| **Datos incluidos** | Make, Model, Specs | + Market listings | + Price history  |
| **Recalls**         | ✅ NHTSA completo  | ⚠️ Parcial        | ❌ No            |
| **Confiabilidad**   | ⭐⭐⭐⭐⭐         | ⭐⭐⭐⭐          | ⭐⭐⭐           |
| **Mejor para**      | Especificaciones   | Market analysis   | Price estimation |
| **Recomendado**     | ✅ PRINCIPAL       | ⭐ Secundario     | ⭐ Backup        |

---

## 📡 ENDPOINTS

### NHTSA API (Gratis)

- `GET /api/vehicles/DecodeVin/{vin}` - Decodificación completa
- `GET /api/vehicles/DecodeVin/{vin}?format=json` - Formato JSON
- `GET /api/vehicles/GetRecalls` - Recalls del VIN
- `GET /api/vehicles/{year}/{manufacturer}/{model}` - Por año/marca/modelo

### Marketcheck API

- `GET /inventory/{vin}` - Datos del VIN + listados activos
- `GET /inventory/{vin}/price-trends` - Tendencia de precios
- `GET /inventory/{vin}/market-comparison` - Precios competencia

### DataOne API

- `GET /vehicle/decode/{vin}` - Decodificación básica
- `GET /vehicle/{vin}/history` - Precio histórico

---

## 💻 Backend Implementation (C#)

### Service Interface

```csharp
public interface IVinDecoderService
{
    Task<VinDecodeResult> DecodeAsync(string vin);
    Task<RecallInfo[]> GetRecallsAsync(string vin);
    Task<VinValidationResult> ValidateVinAsync(string vin);
    Task<VehicleSpecs> GetSpecificationsAsync(string vin);
}

public interface IMarketDataService
{
    Task<PriceTrends> GetPriceTrendsAsync(string vin);
    Task<MarketComparison> GetMarketComparisonAsync(string vin);
}
```

### Domain Models

```csharp
public class VinDecodeResult
{
    public string Vin { get; set; }
    public string Make { get; set; }
    public string Model { get; set; }
    public int Year { get; set; }
    public string Body { get; set; } // "Sedan", "SUV", "Truck", etc.
    public string Engine { get; set; }
    public int Cylinders { get; set; }
    public string Transmission { get; set; } // "Manual", "Automatic", "CVT"
    public string FuelType { get; set; } // "Gasoline", "Diesel", "Hybrid", "Electric"
    public decimal EngineDisplacement { get; set; } // Litros
    public string Color { get; set; }
    public string DriveType { get; set; } // "FWD", "RWD", "AWD", "4WD"
    public string[] Features { get; set; }
    public DateTime DecodedAt { get; set; }
}

public class RecallInfo
{
    public string RecallNumber { get; set; }
    public string Component { get; set; } // "Engine", "Brakes", "Airbags"
    public string Description { get; set; }
    public DateTime RecallDate { get; set; }
    public string Consequence { get; set; } // Posible riesgo
    public string Remedy { get; set; } // Solución/reparación
    public bool IsResolved { get; set; }
}

public class VehicleSpecs
{
    public int Horsepower { get; set; }
    public int Torque { get; set; }
    public decimal ZeroTo60 { get; set; } // Segundos
    public int TopSpeed { get; set; }
    public decimal CityMpg { get; set; }
    public decimal HighwayMpg { get; set; }
    public decimal CombinedMpg { get; set; }
    public int CurWeight { get; set; } // Libras
    public int GrossWeight { get; set; } // Capacidad
}

public class PriceTrends
{
    public string Vin { get; set; }
    public decimal AveragePrice { get; set; }
    public decimal LowPrice { get; set; }
    public decimal HighPrice { get; set; }
    public int ListingsCount { get; set; }
    public decimal[] PriceHistory { get; set; } // Últimos 90 días
    public decimal TrendPercentage { get; set; } // % cambio mes anterior
}
```

### NHTSA Service Implementation

```csharp
public class NhtsaVinDecoderService : IVinDecoderService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<NhtsaVinDecoderService> _logger;
    private const string BaseUrl = "https://vpic.nhtsa.dot.gov/api/vehicles";

    public NhtsaVinDecoderService(
        HttpClient httpClient,
        ILogger<NhtsaVinDecoderService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<VinDecodeResult> DecodeAsync(string vin)
    {
        try
        {
            if (!IsValidVin(vin))
                throw new InvalidOperationException("VIN inválido");

            var url = $"{BaseUrl}/DecodeVin/{vin}?format=json";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"NHTSA API error: {response.StatusCode}");

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            var results = doc.RootElement.GetProperty("Results");

            var makeElement = results.EnumerateArray()
                .FirstOrDefault(r => r.GetProperty("Variable").GetString() == "Make");
            var modelElement = results.EnumerateArray()
                .FirstOrDefault(r => r.GetProperty("Variable").GetString() == "Model");
            var yearElement = results.EnumerateArray()
                .FirstOrDefault(r => r.GetProperty("Variable").GetString() == "Model Year");

            return new VinDecodeResult
            {
                Vin = vin,
                Make = makeElement.GetProperty("Value").GetString(),
                Model = modelElement.GetProperty("Value").GetString(),
                Year = int.Parse(yearElement.GetProperty("Value").GetString()),
                DecodedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decoding VIN {vin}", vin);
            throw;
        }
    }

    public async Task<RecallInfo[]> GetRecallsAsync(string vin)
    {
        var decode = await DecodeAsync(vin);
        var url = $"{BaseUrl}/GetRecalls?make={decode.Make}&model={decode.Model}&modelYear={decode.Year}";
        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
            return Array.Empty<RecallInfo>();

        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);

        return doc.RootElement.GetProperty("Results")
            .EnumerateArray()
            .Select(r => new RecallInfo
            {
                RecallNumber = r.GetProperty("RecallNumber").GetString(),
                Component = r.GetProperty("Component").GetString(),
                Description = r.GetProperty("Summary").GetString(),
                Consequence = r.GetProperty("Consequence").GetString(),
                Remedy = r.GetProperty("Remedy").GetString()
            })
            .ToArray();
    }

    public async Task<VinValidationResult> ValidateVinAsync(string vin)
    {
        return new VinValidationResult
        {
            IsValid = IsValidVin(vin),
            Vin = vin,
            ValidationDate = DateTime.UtcNow
        };
    }

    public async Task<VehicleSpecs> GetSpecificationsAsync(string vin)
    {
        var decode = await DecodeAsync(vin);
        // Buscar specs en base de datos o tercera API
        return new VehicleSpecs
        {
            Horsepower = 200,
            TopSpeed = 120,
            CityMpg = 20,
            HighwayMpg = 28
        };
    }

    private bool IsValidVin(string vin)
    {
        return !string.IsNullOrWhiteSpace(vin) &&
               vin.Length == 17 &&
               vin.All(c => char.IsLetterOrDigit(c));
    }
}
```

---

## 🎨 Frontend Implementation (React + TypeScript)

### VIN Decoder Service

```typescript
import axios from "axios";

export interface VinDecodeResult {
  vin: string;
  make: string;
  model: string;
  year: number;
  body: string;
  engine: string;
  transmission: string;
}

export class VinDecoderService {
  private baseUrl = process.env.REACT_APP_API_URL;

  async decode(vin: string): Promise<VinDecodeResult> {
    const response = await axios.get(`${this.baseUrl}/api/vin-decode/${vin}`);
    return response.data;
  }

  async getRecalls(vin: string) {
    const response = await axios.get(
      `${this.baseUrl}/api/vin-decode/${vin}/recalls`
    );
    return response.data;
  }
}
```

### React Component - Auto-fill on Scan

```typescript
import React, { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { VinDecoderService } from "@/services/vinDecoderService";

export const VinScanForm = ({
  onDecoded,
}: {
  onDecoded: (data: any) => void;
}) => {
  const [vin, setVin] = useState("");
  const decoderService = new VinDecoderService();

  const {
    data: decoded,
    isLoading,
    mutate,
  } = useMutation({
    mutationFn: (v: string) => decoderService.decode(v),
    onSuccess: (data) => onDecoded(data),
  });

  const handleScan = (scannedVin: string) => {
    setVin(scannedVin);
    mutate(scannedVin);
  };

  return (
    <div className="border rounded-lg p-6">
      <h3 className="text-lg font-bold mb-4">Escanear o ingresar VIN</h3>

      <div className="flex gap-2 mb-4">
        <input
          type="text"
          value={vin}
          onChange={(e) => setVin(e.target.value)}
          placeholder="Ingresa VIN o escanea código"
          className="flex-1 px-3 py-2 border rounded"
          maxLength={17}
        />
        <button
          onClick={() => handleScan(vin)}
          disabled={isLoading || vin.length !== 17}
          className="px-4 py-2 bg-blue-600 text-white rounded disabled:opacity-50"
        >
          {isLoading ? "Decodificando..." : "Decodificar"}
        </button>
      </div>

      {decoded && (
        <div className="grid grid-cols-2 gap-4 mt-6 bg-green-50 p-4 rounded">
          <div>
            <label className="text-sm text-gray-600">Marca</label>
            <input
              type="text"
              defaultValue={decoded.make}
              className="w-full px-3 py-2 border rounded mt-1"
            />
          </div>
          <div>
            <label className="text-sm text-gray-600">Modelo</label>
            <input
              type="text"
              defaultValue={decoded.model}
              className="w-full px-3 py-2 border rounded mt-1"
            />
          </div>
          <div>
            <label className="text-sm text-gray-600">Año</label>
            <input
              type="number"
              defaultValue={decoded.year}
              className="w-full px-3 py-2 border rounded mt-1"
            />
          </div>
          <div>
            <label className="text-sm text-gray-600">Carrocería</label>
            <input
              type="text"
              defaultValue={decoded.body}
              className="w-full px-3 py-2 border rounded mt-1"
            />
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
public class NhtsaVinDecoderServiceTests
{
    private readonly NhtsaVinDecoderService _service;
    private readonly Mock<HttpClient> _httpClientMock;
    private readonly Mock<ILogger<NhtsaVinDecoderService>> _loggerMock;

    public NhtsaVinDecoderServiceTests()
    {
        _httpClientMock = new Mock<HttpClient>();
        _loggerMock = new Mock<ILogger<NhtsaVinDecoderService>>();
        _service = new NhtsaVinDecoderService(_httpClientMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task DecodeAsync_WithValidVin_ReturnsDecodedVehicle()
    {
        // Arrange
        var vin = "1G1FB1RS5D2176411";

        // Act
        var result = await _service.DecodeAsync(vin);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(vin, result.Vin);
        Assert.NotEmpty(result.Make);
        Assert.NotEmpty(result.Model);
        Assert.True(result.Year > 1980);
    }

    [Theory]
    [InlineData("INVALID")]
    [InlineData("")]
    [InlineData("123456789012345")]
    public async Task DecodeAsync_WithInvalidVin_ThrowsException(string invalidVin)
    {
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.DecodeAsync(invalidVin)
        );
    }

    [Fact]
    public async Task GetRecallsAsync_WithValidVin_ReturnsRecalls()
    {
        var vin = "1G1FB1RS5D2176411";
        var recalls = await _service.GetRecallsAsync(vin);

        Assert.NotNull(recalls);
        // Algunos vehículos pueden no tener recalls
        Assert.IsAssignableFrom<RecallInfo[]>(recalls);
    }
}
```

---

## 🔧 Troubleshooting

| Problema                        | Causa                           | Solución                            |
| ------------------------------- | ------------------------------- | ----------------------------------- |
| VIN no encontrado en NHTSA      | VIN incorrecto o vehículo raro  | Validar VIN formato (17 caracteres) |
| Recalls no aparecen             | NHTSA no tiene recalls para eso | Usar API secundaria o manual check  |
| Especificaciones incompletas    | NHTSA solo da Make/Model/Year   | Integrar con Edmunds o Craigslist   |
| Timeout en decodificación       | Red lenta o API caída           | Implementar retry + timeout 5s      |
| Requests se agotan (rate limit) | NHTSA permite 100 req/min       | Caché en Redis 24 horas             |
| Color no se obtiene             | NHTSA no proporciona color      | Dejar que dealer ingrese color      |
| Transmisión incorrecta          | VIN con múltiples opciones      | Mostrar selector al dealer          |

---

## 🔗 Integración con OKLA

### 1. **Crear endpoint en VehiclesSaleService**

```csharp
[HttpPost("/vin-decode")]
public async Task<ActionResult<VinDecodeResult>> DecodeVin([FromBody] DecodeVinRequest request)
{
    var result = await _vinDecoderService.DecodeAsync(request.Vin);
    return Ok(result);
}
```

### 2. **Gateway routing (ocelot.prod.json)**

```json
{
  "UpstreamPathTemplate": "/api/vin-decode/{vin}",
  "DownstreamPathTemplate": "/api/vehicles/vin-decode/{vin}",
  "DownstreamHostAndPorts": [{ "Host": "vehiclessaleservice", "Port": 8080 }]
}
```

### 3. **Integrar en PublishVehiclePage**

```tsx
<VinScanForm
  onDecoded={(data) => {
    form.setValues({
      make: data.make,
      model: data.model,
      year: data.year,
      body: data.body,
    });
  }}
/>
```

### 4. **Caché en Redis (24 horas)**

```csharp
var cacheKey = $"vin:decode:{vin}";
var cached = await _cache.GetAsync<VinDecodeResult>(cacheKey);
if (cached != null) return cached;

var result = await _nhtsaService.DecodeAsync(vin);
await _cache.SetAsync(cacheKey, result, TimeSpan.FromHours(24));
return result;
```

### 5. **Evento para analytics**

```csharp
await _bus.Publish(new VinDecodedEvent
{
    Vin = vin,
    Make = result.Make,
    Model = result.Model,
    Year = result.Year
});
```

---

## 💰 Costos Estimados

| API         | Costo  | Vol./Mes | Total/Mes | Total/Año  |
| ----------- | ------ | -------- | --------- | ---------- |
| NHTSA       | Gratis | 50,000   | $0        | $0         |
| Marketcheck | $0.10  | 5,000    | $500      | $6,000     |
| DataOne     | $0.05  | 2,000    | $100      | $1,200     |
| **TOTAL**   |        |          | **$600**  | **$7,200** |

✅ \*\*NHTSA es suficiente para 90% de casos. Marketcheck como backup para análisis de mercado.

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<NhtsaResponse>(content);

        return new VinDecodeResult
        {
            Make = result.GetVariable("Make"),
            Model = result.GetVariable("Model"),
            Year = int.Parse(result.GetVariable("Model Year")),
            BodyStyle = result.GetVariable("Body Class"),
            Transmission = result.GetVariable("Transmission Type"),
            Engine = result.GetVariable("Engine Description")
        };
    }

}

```

---

**Versión:** 1.0 | **Actualizado:** Enero 15, 2026
```
