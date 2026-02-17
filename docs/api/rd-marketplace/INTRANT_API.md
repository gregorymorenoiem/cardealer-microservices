# 🚦 INTRANT - Instituto Nacional de Tránsito

**Entidad:** Instituto Nacional de Tránsito y Transporte Terrestre  
**Website:** [intrant.gob.do](https://intrant.gob.do)  
**Uso:** Historial de inspecciones, licencias, revisión técnica  
**Prioridad:** ⭐⭐⭐⭐⭐ CRÍTICA

---

## 📋 Información General

| Campo                   | Valor                                                        |
| ----------------------- | ------------------------------------------------------------ |
| **Website**             | [intrant.gob.do](https://intrant.gob.do)                     |
| **Portal de Servicios** | [servicios.intrant.gob.do](https://servicios.intrant.gob.do) |
| **Método**              | Web Scraping / Convenio                                      |
| **Datos Principales**   | Inspecciones, licencias, multas                              |

---

## 📊 Datos Obtenibles

### Por Placa del Vehículo

| Dato                        | Descripción              | Uso en OKLA                  |
| --------------------------- | ------------------------ | ---------------------------- |
| **Revisión Técnica**        | Estado y vigencia        | Badge "Inspección Vigente ✓" |
| **Fecha Última Inspección** | Cuándo fue inspeccionado | Historial del vehículo       |
| **Próxima Inspección**      | Cuándo vence             | Alertar al comprador         |
| **Centro de Inspección**    | Dónde fue inspeccionado  | Referencia                   |
| **Resultado**               | Aprobado/Rechazado       | Historial                    |
| **Observaciones**           | Detalles técnicos        | Información adicional        |

### Por Licencia de Conductor

| Dato          | Descripción                | Uso en OKLA                  |
| ------------- | -------------------------- | ---------------------------- |
| **Vigencia**  | Fecha de vencimiento       | Verificar vendedor/comprador |
| **Categoría** | Tipo de licencia           | Validar que puede conducir   |
| **Estado**    | Vigente/Vencida/Suspendida | Verificación                 |
| **Puntos**    | Puntos acumulados          | Historial del conductor      |

---

## 🌐 Endpoints de Consulta

```http
# Consulta de Revisión Técnica
GET https://servicios.intrant.gob.do/revisiontecnica/consulta
POST ?placa=A123456

# Consulta de Licencia
GET https://servicios.intrant.gob.do/licencias/validar
POST ?cedula=00100000001

# Consulta de Multas INTRANT
GET https://servicios.intrant.gob.do/multas/consulta
POST ?placa=A123456
```

---

## 💻 Modelos C#

```csharp
namespace VehicleVerificationService.Domain.Entities;

/// <summary>
/// Información de revisión técnica vehicular
/// </summary>
public record TechnicalInspection(
    string Placa,
    InspectionStatus Estado,
    DateTime FechaInspeccion,
    DateTime FechaVencimiento,
    string CentroInspeccion,
    InspectionResult Resultado,
    string? Observaciones,
    List<InspectionItem> ItemsRevisados
);

public enum InspectionStatus
{
    Vigente,
    PorVencer,    // Vence en 30 días
    Vencida,
    NuncaInspeccionado
}

public enum InspectionResult
{
    Aprobado,
    AprobadoConObservaciones,
    Rechazado,
    Pendiente
}

public record InspectionItem(
    string Categoria,     // "Frenos", "Luces", "Emisiones"
    string Item,          // "Freno de mano"
    ItemResult Resultado, // Bueno, Regular, Malo
    string? Observacion
);

public enum ItemResult
{
    Bueno,
    Regular,
    Malo,
    NoAplica
}

/// <summary>
/// Información de licencia de conducir
/// </summary>
public record DriverLicense(
    string Cedula,
    string NombreCompleto,
    LicenseCategory Categoria,
    LicenseStatus Estado,
    DateTime FechaEmision,
    DateTime FechaVencimiento,
    int PuntosAcumulados,
    int PuntosDisponibles,
    List<LicenseViolation> Violaciones
);

public enum LicenseCategory
{
    Primera,    // Motocicletas
    Segunda,    // Vehículos livianos
    Tercera,    // Vehículos pesados
    Cuarta,     // Transporte público
    Quinta      // Vehículos articulados
}

public enum LicenseStatus
{
    Vigente,
    Vencida,
    Suspendida,
    Cancelada
}

public record LicenseViolation(
    DateTime Fecha,
    string Descripcion,
    int PuntosDescontados,
    decimal MultaRD
);

/// <summary>
/// Historial completo del vehículo
/// </summary>
public record VehicleIntrantHistory(
    string Placa,
    TechnicalInspection? InspeccionActual,
    List<TechnicalInspection> HistorialInspecciones,
    List<IntrantMulta> Multas,
    DateTime ConsultadoEn
);

public record IntrantMulta(
    string NumeroMulta,
    DateTime Fecha,
    string Infraccion,
    decimal Monto,
    MultaStatus Estado
);

public enum MultaStatus
{
    Pendiente,
    Pagada,
    EnDisputa,
    Prescrita
}
```

---

## 🔧 Service Interface

```csharp
namespace VehicleVerificationService.Domain.Interfaces;

public interface IIntrantService
{
    // Vehículos
    Task<TechnicalInspection?> GetCurrentInspectionAsync(string placa);
    Task<List<TechnicalInspection>> GetInspectionHistoryAsync(string placa);
    Task<VehicleIntrantHistory> GetFullVehicleHistoryAsync(string placa);

    // Licencias
    Task<DriverLicense?> ValidateLicenseAsync(string cedula);
    Task<bool> IsLicenseValidAsync(string cedula);

    // Multas
    Task<List<IntrantMulta>> GetMultasAsync(string placa);
    Task<decimal> GetTotalDeudaMultasAsync(string placa);
}
```

---

## 🏗️ Service Implementation

```csharp
namespace VehicleVerificationService.Infrastructure.Services;

public class IntrantService : IIntrantService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<IntrantService> _logger;

    private const string BASE_URL = "https://servicios.intrant.gob.do";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(12);

    public IntrantService(
        HttpClient httpClient,
        IMemoryCache cache,
        ILogger<IntrantService> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _logger = logger;
    }

    public async Task<TechnicalInspection?> GetCurrentInspectionAsync(string placa)
    {
        var cacheKey = $"intrant_inspection_{placa}";

        if (_cache.TryGetValue(cacheKey, out TechnicalInspection? cached))
            return cached;

        try
        {
            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("placa", placa.ToUpperInvariant())
            });

            var response = await _httpClient.PostAsync(
                $"{BASE_URL}/revisiontecnica/consulta", formData);

            var html = await response.Content.ReadAsStringAsync();
            var result = ParseInspectionResult(html, placa);

            if (result != null)
                _cache.Set(cacheKey, result, CacheDuration);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error consultando INTRANT para {Placa}", placa);
            return null;
        }
    }

    public async Task<VehicleIntrantHistory> GetFullVehicleHistoryAsync(string placa)
    {
        var inspection = await GetCurrentInspectionAsync(placa);
        var history = await GetInspectionHistoryAsync(placa);
        var multas = await GetMultasAsync(placa);

        return new VehicleIntrantHistory(
            Placa: placa,
            InspeccionActual: inspection,
            HistorialInspecciones: history,
            Multas: multas,
            ConsultadoEn: DateTime.UtcNow
        );
    }

    public async Task<DriverLicense?> ValidateLicenseAsync(string cedula)
    {
        var cacheKey = $"intrant_license_{cedula}";

        if (_cache.TryGetValue(cacheKey, out DriverLicense? cached))
            return cached;

        try
        {
            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("cedula", cedula)
            });

            var response = await _httpClient.PostAsync(
                $"{BASE_URL}/licencias/validar", formData);

            var html = await response.Content.ReadAsStringAsync();
            var result = ParseLicenseResult(html, cedula);

            if (result != null)
                _cache.Set(cacheKey, result, CacheDuration);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validando licencia para {Cedula}", cedula);
            return null;
        }
    }

    public async Task<bool> IsLicenseValidAsync(string cedula)
    {
        var license = await ValidateLicenseAsync(cedula);
        return license?.Estado == LicenseStatus.Vigente;
    }

    public async Task<List<IntrantMulta>> GetMultasAsync(string placa)
    {
        var cacheKey = $"intrant_multas_{placa}";

        if (_cache.TryGetValue(cacheKey, out List<IntrantMulta>? cached))
            return cached ?? new();

        try
        {
            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("placa", placa.ToUpperInvariant())
            });

            var response = await _httpClient.PostAsync(
                $"{BASE_URL}/multas/consulta", formData);

            var html = await response.Content.ReadAsStringAsync();
            var result = ParseMultasResult(html);

            _cache.Set(cacheKey, result, CacheDuration);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error consultando multas INTRANT {Placa}", placa);
            return new();
        }
    }

    public async Task<decimal> GetTotalDeudaMultasAsync(string placa)
    {
        var multas = await GetMultasAsync(placa);
        return multas
            .Where(m => m.Estado == MultaStatus.Pendiente)
            .Sum(m => m.Monto);
    }

    private TechnicalInspection? ParseInspectionResult(string html, string placa)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        // Buscar div de resultado
        var resultDiv = doc.DocumentNode.SelectSingleNode("//div[@class='resultado']");
        if (resultDiv == null)
            return null;

        // Parsear datos
        var fechaInsp = ExtractDate(resultDiv, "fechaInspeccion");
        var fechaVenc = ExtractDate(resultDiv, "fechaVencimiento");
        var centro = ExtractText(resultDiv, "centro");
        var resultado = ExtractText(resultDiv, "resultado");

        if (!fechaInsp.HasValue)
            return null;

        var estado = DetermineInspectionStatus(fechaVenc);

        return new TechnicalInspection(
            Placa: placa,
            Estado: estado,
            FechaInspeccion: fechaInsp.Value,
            FechaVencimiento: fechaVenc ?? fechaInsp.Value.AddYears(1),
            CentroInspeccion: centro ?? "No especificado",
            Resultado: ParseInspectionResult(resultado),
            Observaciones: ExtractText(resultDiv, "observaciones"),
            ItemsRevisados: ParseInspectionItems(resultDiv)
        );
    }

    private static InspectionStatus DetermineInspectionStatus(DateTime? fechaVencimiento)
    {
        if (!fechaVencimiento.HasValue)
            return InspectionStatus.NuncaInspeccionado;

        var diasRestantes = (fechaVencimiento.Value - DateTime.Today).Days;

        return diasRestantes switch
        {
            < 0 => InspectionStatus.Vencida,
            <= 30 => InspectionStatus.PorVencer,
            _ => InspectionStatus.Vigente
        };
    }

    // Métodos auxiliares de parsing...
}
```

---

## ⚛️ React Component

```tsx
// components/VehicleInspectionBadge.tsx
import { useQuery } from "@tanstack/react-query";
import { intrantService } from "@/services/intrantService";
import { Shield, AlertTriangle, XCircle, Clock } from "lucide-react";

interface Props {
  placa: string;
  showHistory?: boolean;
}

export function VehicleInspectionBadge({ placa, showHistory = false }: Props) {
  const { data: inspection, isLoading } = useQuery({
    queryKey: ["vehicle-inspection", placa],
    queryFn: () => intrantService.getCurrentInspection(placa),
    staleTime: 12 * 60 * 60 * 1000, // 12 horas
  });

  if (isLoading) {
    return <div className="animate-pulse bg-gray-200 rounded w-32 h-6" />;
  }

  const statusConfig = {
    Vigente: {
      icon: Shield,
      text: "Inspección Vigente",
      color: "bg-green-100 text-green-800",
      iconColor: "text-green-600",
    },
    PorVencer: {
      icon: Clock,
      text: "Por Vencer",
      color: "bg-yellow-100 text-yellow-800",
      iconColor: "text-yellow-600",
    },
    Vencida: {
      icon: XCircle,
      text: "Inspección Vencida",
      color: "bg-red-100 text-red-800",
      iconColor: "text-red-600",
    },
    NuncaInspeccionado: {
      icon: AlertTriangle,
      text: "Sin Inspección",
      color: "bg-gray-100 text-gray-800",
      iconColor: "text-gray-600",
    },
  };

  const config = statusConfig[inspection?.estado || "NuncaInspeccionado"];
  const Icon = config.icon;

  const diasRestantes = inspection?.fechaVencimiento
    ? Math.ceil(
        (new Date(inspection.fechaVencimiento).getTime() - Date.now()) /
          (1000 * 60 * 60 * 24)
      )
    : null;

  return (
    <div className="space-y-3">
      <div
        className={`inline-flex items-center gap-2 px-3 py-1.5 rounded-full 
                       ${config.color}`}
      >
        <Icon className={`w-4 h-4 ${config.iconColor}`} />
        <span className="text-sm font-medium">{config.text}</span>
        {diasRestantes !== null && diasRestantes > 0 && diasRestantes <= 60 && (
          <span className="text-xs">({diasRestantes} días)</span>
        )}
      </div>

      {showHistory && inspection && (
        <div className="bg-gray-50 rounded-lg p-4 space-y-2 text-sm">
          <div className="grid grid-cols-2 gap-4">
            <div>
              <p className="text-gray-500">Última Inspección</p>
              <p className="font-medium">
                {new Date(inspection.fechaInspeccion).toLocaleDateString(
                  "es-DO"
                )}
              </p>
            </div>
            <div>
              <p className="text-gray-500">Vencimiento</p>
              <p
                className={`font-medium ${
                  inspection.estado === "Vencida" ? "text-red-600" : ""
                }`}
              >
                {new Date(inspection.fechaVencimiento).toLocaleDateString(
                  "es-DO"
                )}
              </p>
            </div>
            <div>
              <p className="text-gray-500">Centro</p>
              <p className="font-medium">{inspection.centroInspeccion}</p>
            </div>
            <div>
              <p className="text-gray-500">Resultado</p>
              <p
                className={`font-medium ${
                  inspection.resultado === "Aprobado"
                    ? "text-green-600"
                    : "text-red-600"
                }`}
              >
                {inspection.resultado}
              </p>
            </div>
          </div>

          {inspection.observaciones && (
            <div className="mt-2 p-2 bg-yellow-50 rounded">
              <p className="text-yellow-800 text-xs">
                <strong>Observaciones:</strong> {inspection.observaciones}
              </p>
            </div>
          )}
        </div>
      )}
    </div>
  );
}

// components/DriverLicenseVerification.tsx
export function DriverLicenseVerification({ cedula }: { cedula: string }) {
  const { data: license, isLoading } = useQuery({
    queryKey: ["driver-license", cedula],
    queryFn: () => intrantService.validateLicense(cedula),
  });

  if (isLoading) return <p>Verificando licencia...</p>;

  if (!license) {
    return (
      <div className="flex items-center gap-2 text-gray-500">
        <AlertTriangle className="w-4 h-4" />
        <span>No se pudo verificar licencia</span>
      </div>
    );
  }

  return (
    <div
      className={`p-3 rounded-lg ${
        license.estado === "Vigente"
          ? "bg-green-50 border border-green-200"
          : "bg-red-50 border border-red-200"
      }`}
    >
      <div className="flex items-center justify-between">
        <div>
          <p className="font-medium">{license.nombreCompleto}</p>
          <p className="text-sm text-gray-600">
            Licencia {license.categoria} - {license.estado}
          </p>
        </div>
        <div className="text-right">
          <p className="text-sm">
            Vence:{" "}
            {new Date(license.fechaVencimiento).toLocaleDateString("es-DO")}
          </p>
          <p className="text-xs text-gray-500">
            Puntos: {license.puntosDisponibles}/30
          </p>
        </div>
      </div>
    </div>
  );
}
```

---

## 🔌 API Controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class IntrantController : ControllerBase
{
    private readonly IIntrantService _intrantService;

    public IntrantController(IIntrantService intrantService)
    {
        _intrantService = intrantService;
    }

    [HttpGet("inspection/{placa}")]
    public async Task<ActionResult<TechnicalInspection>> GetInspection(string placa)
    {
        var result = await _intrantService.GetCurrentInspectionAsync(placa);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    [HttpGet("history/{placa}")]
    public async Task<ActionResult<VehicleIntrantHistory>> GetHistory(string placa)
    {
        var result = await _intrantService.GetFullVehicleHistoryAsync(placa);
        return Ok(result);
    }

    [HttpGet("license/{cedula}")]
    public async Task<ActionResult<DriverLicense>> ValidateLicense(string cedula)
    {
        var result = await _intrantService.ValidateLicenseAsync(cedula);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    [HttpGet("multas/{placa}")]
    public async Task<ActionResult<List<IntrantMulta>>> GetMultas(string placa)
    {
        var result = await _intrantService.GetMultasAsync(placa);
        return Ok(result);
    }
}
```

---

## 🧪 Tests

```csharp
public class IntrantServiceTests
{
    [Fact]
    public void DetermineInspectionStatus_Vigente_WhenMoreThan30Days()
    {
        var fechaVencimiento = DateTime.Today.AddDays(60);
        var status = IntrantService.DetermineInspectionStatus(fechaVencimiento);
        status.Should().Be(InspectionStatus.Vigente);
    }

    [Fact]
    public void DetermineInspectionStatus_PorVencer_WhenLessThan30Days()
    {
        var fechaVencimiento = DateTime.Today.AddDays(15);
        var status = IntrantService.DetermineInspectionStatus(fechaVencimiento);
        status.Should().Be(InspectionStatus.PorVencer);
    }

    [Fact]
    public void DetermineInspectionStatus_Vencida_WhenPastDate()
    {
        var fechaVencimiento = DateTime.Today.AddDays(-10);
        var status = IntrantService.DetermineInspectionStatus(fechaVencimiento);
        status.Should().Be(InspectionStatus.Vencida);
    }
}
```

---

## 📞 Contacto INTRANT

| Departamento        | Teléfono     | Email                          |
| ------------------- | ------------ | ------------------------------ |
| Servicios Digitales | 809-920-0065 | servicios@intrant.gob.do       |
| Revisión Técnica    | 809-920-0066 | revisiontecnica@intrant.gob.do |

---

**Anterior:** [DGII_VEHICULOS_API.md](./DGII_VEHICULOS_API.md)  
**Siguiente:** [AMET_API.md](./AMET_API.md)
