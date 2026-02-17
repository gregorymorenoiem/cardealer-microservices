# 🚗 DGII - Consulta de Vehículos por Placa

**Entidad:** Dirección General de Impuestos Internos  
**Tipo:** Web Scraping (no hay API oficial)  
**Uso:** Verificar estado fiscal de vehículos  
**Prioridad:** ⭐⭐⭐⭐⭐ CRÍTICA

---

## 📋 Información General

| Campo | Valor |
|-------|-------|
| **Website** | [dgii.gov.do](https://dgii.gov.do) |
| **Consulta Web** | https://dgii.gov.do/app/WebApps/ConsultasWeb/consultas/vehiculos.aspx |
| **Método** | Web Scraping (HtmlAgilityPack) |
| **Datos Requeridos** | Placa del vehículo |
| **Caching** | Recomendado 24 horas |

---

## 📊 Datos Obtenibles

| Dato | Descripción | Uso en OKLA |
|------|-------------|-------------|
| **Placa** | Número de placa | Identificador |
| **Marca** | Fabricante | Validar datos del listing |
| **Modelo** | Modelo del vehículo | Validar datos del listing |
| **Año** | Año de fabricación | Validar datos del listing |
| **Color** | Color registrado | Validar datos del listing |
| **Tipo** | Automóvil, Jeepeta, etc. | Categorización |
| **Estado Fiscal** | Al día / Pendiente | Badge de verificación |
| **Monto Adeudado** | Deuda de impuestos | Alerta al comprador |
| **Último Pago Marbete** | Fecha | Verificar vigencia |
| **Propietario** | Nombre (parcial) | Verificar vendedor |

---

## 🌐 Endpoint de Consulta

```http
# Página de consulta DGII
GET https://dgii.gov.do/app/WebApps/ConsultasWeb/consultas/vehiculos.aspx

# Parámetros POST del formulario
__VIEWSTATE: {valor del formulario}
__EVENTVALIDATION: {valor del formulario}
txtPlaca: A123456
btnBuscar: Buscar
```

---

## 💻 Modelos C#

```csharp
namespace VehicleVerificationService.Domain.Entities;

/// <summary>
/// Información fiscal de un vehículo según DGII
/// </summary>
public record VehicleFiscalInfo(
    string Placa,
    string Marca,
    string Modelo,
    int Ano,
    string Color,
    VehicleType Tipo,
    FiscalStatus EstadoFiscal,
    decimal MontoAdeudado,
    DateTime? UltimoPagoMarbete,
    string? PropietarioParcial,
    DateTime ConsultadoEn
);

public enum VehicleType
{
    Automovil,
    Jeepeta,
    Camioneta,
    Motocicleta,
    Camion,
    Autobus,
    Otro
}

public enum FiscalStatus
{
    AlDia,
    Pendiente,
    Exento,
    NoEncontrado
}

/// <summary>
/// Resultado de verificación completa del vehículo
/// </summary>
public record VehicleVerificationResult(
    bool Success,
    VehicleFiscalInfo? FiscalInfo,
    bool DatosCoinciden,
    List<string> Discrepancias,
    VerificationBadge Badge
);

public enum VerificationBadge
{
    Verified,      // ✓ Todo al día
    Warning,       // ⚠ Tiene deudas menores
    Alert,         // ⚠ Deudas significativas
    NotFound,      // ✗ No encontrado
    Mismatch       // ✗ Datos no coinciden
}
```

---

## 🔧 Service Interface

```csharp
namespace VehicleVerificationService.Domain.Interfaces;

public interface IDgiiVehicleService
{
    /// <summary>
    /// Consulta información fiscal de un vehículo por placa
    /// </summary>
    Task<VehicleFiscalInfo?> GetByPlacaAsync(string placa);

    /// <summary>
    /// Verifica si los datos del listing coinciden con DGII
    /// </summary>
    Task<VehicleVerificationResult> VerifyVehicleAsync(
        string placa,
        string marcaEsperada,
        string modeloEsperado,
        int anoEsperado
    );

    /// <summary>
    /// Obtiene el badge de verificación para mostrar en UI
    /// </summary>
    Task<VerificationBadge> GetVerificationBadgeAsync(string placa);
}
```

---

## 🏗️ Service Implementation

```csharp
namespace VehicleVerificationService.Infrastructure.Services;

using HtmlAgilityPack;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

public class DgiiVehicleService : IDgiiVehicleService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<DgiiVehicleService> _logger;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;

    private const string DGII_URL = 
        "https://dgii.gov.do/app/WebApps/ConsultasWeb/consultas/vehiculos.aspx";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);

    public DgiiVehicleService(
        HttpClient httpClient,
        IMemoryCache cache,
        ILogger<DgiiVehicleService> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _logger = logger;

        // Configurar retry policy con Polly
        _retryPolicy = Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(r => !r.IsSuccessStatusCode)
            .WaitAndRetryAsync(3, attempt => 
                TimeSpan.FromSeconds(Math.Pow(2, attempt)));
    }

    public async Task<VehicleFiscalInfo?> GetByPlacaAsync(string placa)
    {
        // Normalizar placa
        placa = NormalizePlaca(placa);
        var cacheKey = $"dgii_vehicle_{placa}";

        // Verificar cache
        if (_cache.TryGetValue(cacheKey, out VehicleFiscalInfo? cached))
        {
            _logger.LogDebug("Cache hit for placa {Placa}", placa);
            return cached;
        }

        try
        {
            // 1. Obtener página inicial para extraer ViewState
            var initialResponse = await _httpClient.GetAsync(DGII_URL);
            var initialHtml = await initialResponse.Content.ReadAsStringAsync();

            var doc = new HtmlDocument();
            doc.LoadHtml(initialHtml);

            var viewState = doc.DocumentNode
                .SelectSingleNode("//input[@id='__VIEWSTATE']")
                ?.GetAttributeValue("value", "");
            var eventValidation = doc.DocumentNode
                .SelectSingleNode("//input[@id='__EVENTVALIDATION']")
                ?.GetAttributeValue("value", "");

            // 2. Enviar consulta
            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("__VIEWSTATE", viewState ?? ""),
                new KeyValuePair<string, string>("__EVENTVALIDATION", 
                    eventValidation ?? ""),
                new KeyValuePair<string, string>("txtPlaca", placa),
                new KeyValuePair<string, string>("btnBuscar", "Buscar")
            });

            var response = await _retryPolicy.ExecuteAsync(() =>
                _httpClient.PostAsync(DGII_URL, formData));

            var html = await response.Content.ReadAsStringAsync();

            // 3. Parsear resultados
            var result = ParseVehicleInfo(html, placa);

            if (result != null)
            {
                _cache.Set(cacheKey, result, CacheDuration);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error consultando DGII para placa {Placa}", placa);
            return null;
        }
    }

    public async Task<VehicleVerificationResult> VerifyVehicleAsync(
        string placa,
        string marcaEsperada,
        string modeloEsperado,
        int anoEsperado)
    {
        var fiscalInfo = await GetByPlacaAsync(placa);

        if (fiscalInfo == null)
        {
            return new VehicleVerificationResult(
                Success: false,
                FiscalInfo: null,
                DatosCoinciden: false,
                Discrepancias: new List<string> { "Vehículo no encontrado en DGII" },
                Badge: VerificationBadge.NotFound
            );
        }

        var discrepancias = new List<string>();

        // Comparar marca
        if (!CompareStrings(fiscalInfo.Marca, marcaEsperada))
        {
            discrepancias.Add(
                $"Marca: DGII='{fiscalInfo.Marca}', Listing='{marcaEsperada}'");
        }

        // Comparar modelo
        if (!CompareStrings(fiscalInfo.Modelo, modeloEsperado))
        {
            discrepancias.Add(
                $"Modelo: DGII='{fiscalInfo.Modelo}', Listing='{modeloEsperado}'");
        }

        // Comparar año
        if (fiscalInfo.Ano != anoEsperado)
        {
            discrepancias.Add(
                $"Año: DGII={fiscalInfo.Ano}, Listing={anoEsperado}");
        }

        // Determinar badge
        var badge = DetermineBadge(fiscalInfo, discrepancias);

        return new VehicleVerificationResult(
            Success: true,
            FiscalInfo: fiscalInfo,
            DatosCoinciden: discrepancias.Count == 0,
            Discrepancias: discrepancias,
            Badge: badge
        );
    }

    public async Task<VerificationBadge> GetVerificationBadgeAsync(string placa)
    {
        var info = await GetByPlacaAsync(placa);

        if (info == null)
            return VerificationBadge.NotFound;

        if (info.EstadoFiscal == FiscalStatus.AlDia && info.MontoAdeudado == 0)
            return VerificationBadge.Verified;

        if (info.MontoAdeudado > 0 && info.MontoAdeudado < 5000)
            return VerificationBadge.Warning;

        return VerificationBadge.Alert;
    }

    private VehicleFiscalInfo? ParseVehicleInfo(string html, string placa)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        // Buscar tabla de resultados
        var table = doc.DocumentNode.SelectSingleNode(
            "//table[@id='grdResultados']//tr[2]");

        if (table == null)
            return null;

        var cells = table.SelectNodes("td");
        if (cells == null || cells.Count < 8)
            return null;

        return new VehicleFiscalInfo(
            Placa: placa,
            Marca: cells[1].InnerText.Trim(),
            Modelo: cells[2].InnerText.Trim(),
            Ano: int.TryParse(cells[3].InnerText.Trim(), out var ano) ? ano : 0,
            Color: cells[4].InnerText.Trim(),
            Tipo: ParseVehicleType(cells[5].InnerText.Trim()),
            EstadoFiscal: ParseFiscalStatus(cells[6].InnerText.Trim()),
            MontoAdeudado: ParseMonto(cells[7].InnerText.Trim()),
            UltimoPagoMarbete: ParseDate(cells[8]?.InnerText.Trim()),
            PropietarioParcial: cells.Count > 9 ? cells[9].InnerText.Trim() : null,
            ConsultadoEn: DateTime.UtcNow
        );
    }

    private static string NormalizePlaca(string placa)
    {
        return placa.ToUpperInvariant()
            .Replace("-", "")
            .Replace(" ", "")
            .Trim();
    }

    private static bool CompareStrings(string a, string b)
    {
        return string.Equals(
            a?.Trim(), 
            b?.Trim(), 
            StringComparison.OrdinalIgnoreCase);
    }

    private static VehicleType ParseVehicleType(string tipo)
    {
        return tipo.ToUpperInvariant() switch
        {
            "AUTOMOVIL" => VehicleType.Automovil,
            "JEEPETA" => VehicleType.Jeepeta,
            "CAMIONETA" => VehicleType.Camioneta,
            "MOTOCICLETA" => VehicleType.Motocicleta,
            "CAMION" => VehicleType.Camion,
            "AUTOBUS" => VehicleType.Autobus,
            _ => VehicleType.Otro
        };
    }

    private static FiscalStatus ParseFiscalStatus(string status)
    {
        return status.ToUpperInvariant() switch
        {
            "AL DIA" or "AL DÍA" => FiscalStatus.AlDia,
            "PENDIENTE" => FiscalStatus.Pendiente,
            "EXENTO" => FiscalStatus.Exento,
            _ => FiscalStatus.NoEncontrado
        };
    }

    private static decimal ParseMonto(string monto)
    {
        var cleaned = monto.Replace("RD$", "")
            .Replace(",", "")
            .Replace(" ", "")
            .Trim();
        return decimal.TryParse(cleaned, out var result) ? result : 0;
    }

    private static DateTime? ParseDate(string? date)
    {
        if (string.IsNullOrEmpty(date))
            return null;
        return DateTime.TryParse(date, out var result) ? result : null;
    }

    private static VerificationBadge DetermineBadge(
        VehicleFiscalInfo info,
        List<string> discrepancias)
    {
        if (discrepancias.Count > 0)
            return VerificationBadge.Mismatch;

        if (info.EstadoFiscal == FiscalStatus.AlDia && info.MontoAdeudado == 0)
            return VerificationBadge.Verified;

        if (info.MontoAdeudado > 0 && info.MontoAdeudado < 5000)
            return VerificationBadge.Warning;

        return VerificationBadge.Alert;
    }
}
```

---

## ⚛️ React Component

```tsx
// components/VehicleVerificationBadge.tsx
import { useQuery } from '@tanstack/react-query';
import { vehicleVerificationService } from '@/services/vehicleVerificationService';
import { CheckCircle, AlertTriangle, XCircle, HelpCircle } from 'lucide-react';

interface Props {
  placa: string;
  showDetails?: boolean;
}

export function VehicleVerificationBadge({ placa, showDetails = false }: Props) {
  const { data: verification, isLoading } = useQuery({
    queryKey: ['vehicle-verification', placa],
    queryFn: () => vehicleVerificationService.verifyByPlaca(placa),
    staleTime: 24 * 60 * 60 * 1000, // 24 horas
  });

  if (isLoading) {
    return (
      <div className="animate-pulse bg-gray-200 rounded-full px-3 py-1 w-24 h-6" />
    );
  }

  const badges = {
    Verified: {
      icon: CheckCircle,
      text: 'Verificado DGII',
      color: 'bg-green-100 text-green-800 border-green-200',
      iconColor: 'text-green-600',
    },
    Warning: {
      icon: AlertTriangle,
      text: 'Deudas Menores',
      color: 'bg-yellow-100 text-yellow-800 border-yellow-200',
      iconColor: 'text-yellow-600',
    },
    Alert: {
      icon: XCircle,
      text: 'Revisar Estado',
      color: 'bg-red-100 text-red-800 border-red-200',
      iconColor: 'text-red-600',
    },
    NotFound: {
      icon: HelpCircle,
      text: 'No Verificado',
      color: 'bg-gray-100 text-gray-800 border-gray-200',
      iconColor: 'text-gray-600',
    },
    Mismatch: {
      icon: XCircle,
      text: 'Datos Incorrectos',
      color: 'bg-red-100 text-red-800 border-red-200',
      iconColor: 'text-red-600',
    },
  };

  const badge = badges[verification?.badge || 'NotFound'];
  const Icon = badge.icon;

  return (
    <div className="space-y-2">
      <div className={`inline-flex items-center gap-1.5 px-3 py-1 rounded-full 
                       border ${badge.color}`}>
        <Icon className={`w-4 h-4 ${badge.iconColor}`} />
        <span className="text-sm font-medium">{badge.text}</span>
      </div>

      {showDetails && verification?.fiscalInfo && (
        <div className="bg-gray-50 rounded-lg p-4 space-y-2 text-sm">
          <div className="grid grid-cols-2 gap-2">
            <div>
              <span className="text-gray-500">Estado Fiscal:</span>
              <span className={`ml-2 font-medium ${
                verification.fiscalInfo.estadoFiscal === 'AlDia' 
                  ? 'text-green-600' : 'text-red-600'
              }`}>
                {verification.fiscalInfo.estadoFiscal === 'AlDia' 
                  ? 'Al Día ✓' : 'Pendiente'}
              </span>
            </div>
            
            {verification.fiscalInfo.montoAdeudado > 0 && (
              <div>
                <span className="text-gray-500">Adeudado:</span>
                <span className="ml-2 font-medium text-red-600">
                  RD$ {verification.fiscalInfo.montoAdeudado.toLocaleString()}
                </span>
              </div>
            )}
            
            {verification.fiscalInfo.ultimoPagoMarbete && (
              <div>
                <span className="text-gray-500">Último Marbete:</span>
                <span className="ml-2 font-medium">
                  {new Date(verification.fiscalInfo.ultimoPagoMarbete)
                    .toLocaleDateString('es-DO')}
                </span>
              </div>
            )}
          </div>

          {verification.discrepancias.length > 0 && (
            <div className="mt-2 p-2 bg-red-50 rounded border border-red-200">
              <p className="text-red-700 font-medium">⚠️ Discrepancias:</p>
              <ul className="list-disc list-inside text-red-600">
                {verification.discrepancias.map((d, i) => (
                  <li key={i}>{d}</li>
                ))}
              </ul>
            </div>
          )}

          <p className="text-xs text-gray-400 mt-2">
            Consultado: {new Date(verification.fiscalInfo.consultadoEn)
              .toLocaleString('es-DO')}
          </p>
        </div>
      )}
    </div>
  );
}
```

---

## 🔌 API Controller

```csharp
namespace VehicleVerificationService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VehicleVerificationController : ControllerBase
{
    private readonly IDgiiVehicleService _dgiiService;

    public VehicleVerificationController(IDgiiVehicleService dgiiService)
    {
        _dgiiService = dgiiService;
    }

    /// <summary>
    /// Obtiene información fiscal de un vehículo por placa
    /// </summary>
    [HttpGet("placa/{placa}")]
    public async Task<ActionResult<VehicleFiscalInfo>> GetByPlaca(string placa)
    {
        var result = await _dgiiService.GetByPlacaAsync(placa);

        if (result == null)
            return NotFound(new { message = "Vehículo no encontrado en DGII" });

        return Ok(result);
    }

    /// <summary>
    /// Verifica un vehículo contra los datos del listing
    /// </summary>
    [HttpPost("verify")]
    public async Task<ActionResult<VehicleVerificationResult>> Verify(
        [FromBody] VerifyVehicleRequest request)
    {
        var result = await _dgiiService.VerifyVehicleAsync(
            request.Placa,
            request.Marca,
            request.Modelo,
            request.Ano
        );

        return Ok(result);
    }

    /// <summary>
    /// Obtiene solo el badge de verificación
    /// </summary>
    [HttpGet("badge/{placa}")]
    public async Task<ActionResult<VerificationBadge>> GetBadge(string placa)
    {
        var badge = await _dgiiService.GetVerificationBadgeAsync(placa);
        return Ok(new { badge = badge.ToString() });
    }
}

public record VerifyVehicleRequest(
    string Placa,
    string Marca,
    string Modelo,
    int Ano
);
```

---

## 🧪 Tests

```csharp
namespace VehicleVerificationService.Tests;

public class DgiiVehicleServiceTests
{
    private readonly Mock<HttpMessageHandler> _httpHandler;
    private readonly Mock<IMemoryCache> _cache;
    private readonly Mock<ILogger<DgiiVehicleService>> _logger;
    private readonly DgiiVehicleService _service;

    public DgiiVehicleServiceTests()
    {
        _httpHandler = new Mock<HttpMessageHandler>();
        _cache = new Mock<IMemoryCache>();
        _logger = new Mock<ILogger<DgiiVehicleService>>();

        var httpClient = new HttpClient(_httpHandler.Object);
        _service = new DgiiVehicleService(httpClient, _cache.Object, _logger.Object);
    }

    [Fact]
    public async Task GetByPlacaAsync_ValidPlaca_ReturnsInfo()
    {
        // Arrange
        var html = @"
            <table id='grdResultados'>
                <tr><th>Headers</th></tr>
                <tr>
                    <td>A123456</td>
                    <td>TOYOTA</td>
                    <td>COROLLA</td>
                    <td>2022</td>
                    <td>BLANCO</td>
                    <td>AUTOMOVIL</td>
                    <td>AL DIA</td>
                    <td>RD$0.00</td>
                    <td>15/01/2026</td>
                </tr>
            </table>";

        SetupHttpResponse(html);

        // Act
        var result = await _service.GetByPlacaAsync("A123456");

        // Assert
        result.Should().NotBeNull();
        result!.Marca.Should().Be("TOYOTA");
        result.Modelo.Should().Be("COROLLA");
        result.Ano.Should().Be(2022);
        result.EstadoFiscal.Should().Be(FiscalStatus.AlDia);
    }

    [Fact]
    public async Task VerifyVehicleAsync_DataMatches_ReturnsVerified()
    {
        // Arrange
        SetupHttpResponseWithVehicle("TOYOTA", "COROLLA", 2022, FiscalStatus.AlDia);

        // Act
        var result = await _service.VerifyVehicleAsync(
            "A123456", "Toyota", "Corolla", 2022);

        // Assert
        result.Success.Should().BeTrue();
        result.DatosCoinciden.Should().BeTrue();
        result.Badge.Should().Be(VerificationBadge.Verified);
    }

    [Fact]
    public async Task VerifyVehicleAsync_DataMismatch_ReturnsMismatch()
    {
        // Arrange
        SetupHttpResponseWithVehicle("TOYOTA", "COROLLA", 2022, FiscalStatus.AlDia);

        // Act - Enviamos datos diferentes
        var result = await _service.VerifyVehicleAsync(
            "A123456", "Honda", "Civic", 2023);

        // Assert
        result.DatosCoinciden.Should().BeFalse();
        result.Discrepancias.Should().HaveCountGreaterThan(0);
        result.Badge.Should().Be(VerificationBadge.Mismatch);
    }

    [Fact]
    public async Task GetBadge_VehicleWithDebt_ReturnsWarningOrAlert()
    {
        // Arrange
        SetupHttpResponseWithVehicle("TOYOTA", "COROLLA", 2022, 
            FiscalStatus.Pendiente, montoAdeudado: 3000);

        // Act
        var badge = await _service.GetVerificationBadgeAsync("A123456");

        // Assert
        badge.Should().Be(VerificationBadge.Warning);
    }
}
```

---

## ⚙️ Configuración

```json
// appsettings.json
{
  "DgiiVehicle": {
    "BaseUrl": "https://dgii.gov.do/app/WebApps/ConsultasWeb/consultas/vehiculos.aspx",
    "CacheHours": 24,
    "TimeoutSeconds": 30,
    "MaxRetries": 3
  }
}
```

```csharp
// Program.cs
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<IDgiiVehicleService, DgiiVehicleService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("User-Agent", "OKLA-Marketplace/1.0");
});
```

---

## 🚨 Consideraciones

### Rate Limiting
- DGII puede bloquear IPs con muchas consultas
- Implementar delays entre consultas (500ms mínimo)
- Usar cache agresivo (24 horas)

### Disponibilidad
- El sitio de DGII puede tener caídas
- Implementar circuit breaker con Polly
- Tener fallback (mostrar "Verificación no disponible")

### Legal
- Scraping puede violar términos de uso
- Contactar DGII para convenio oficial
- Teléfono: 809-689-3444

---

**Siguiente:** [INTRANT_API.md](./INTRANT_API.md)
