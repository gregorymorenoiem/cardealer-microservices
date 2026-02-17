# 🚔 AMET - Autoridad Metropolitana de Transporte

**Entidad:** Autoridad Metropolitana de Transporte (AMET)  
**Website:** [amet.gob.do](https://amet.gob.do)  
**Uso:** Historial de multas y accidentes de tránsito  
**Prioridad:** ⭐⭐⭐⭐ ALTA

---

## 📋 Información General

| Campo | Valor |
|-------|-------|
| **Website** | [amet.gob.do](https://amet.gob.do) |
| **Consulta Multas** | https://amet.gob.do/servicios/consulta-multas |
| **Cobertura** | Gran Santo Domingo principalmente |
| **Método** | Web Scraping / Convenio |

---

## 📊 Datos Obtenibles

| Dato | Descripción | Uso en OKLA |
|------|-------------|-------------|
| **Multas Pendientes** | Cantidad y monto | Alertar deudas ocultas |
| **Historial de Multas** | Todas las multas del vehículo | Historial completo |
| **Tipo de Infracción** | Velocidad, estacionamiento, etc. | Patrón de uso |
| **Accidentes Reportados** | Choques registrados | CRÍTICO para compradores |
| **Estado de Multas** | Pagada/Pendiente/Disputa | Estado actual |

---

## 💻 Modelos C#

```csharp
namespace VehicleVerificationService.Domain.Entities;

/// <summary>
/// Historial completo AMET de un vehículo
/// </summary>
public record AmetVehicleHistory(
    string Placa,
    int TotalMultas,
    int MultasPendientes,
    decimal MontoTotalAdeudado,
    int AccidentesReportados,
    List<AmetMulta> Multas,
    List<AmetAccident> Accidentes,
    AmetRiskLevel NivelRiesgo,
    DateTime ConsultadoEn
);

public record AmetMulta(
    string NumeroMulta,
    DateTime Fecha,
    string Infraccion,
    InfraccionTipo Tipo,
    string Ubicacion,
    decimal MontoOriginal,
    decimal Recargo,
    decimal MontoTotal,
    AmetMultaStatus Estado,
    DateTime? FechaPago
);

public enum InfraccionTipo
{
    Velocidad,
    Estacionamiento,
    SemaforoRojo,
    Documentos,
    CinturonSeguridad,
    UsoCelular,
    Alcoholemia,
    Otras
}

public enum AmetMultaStatus
{
    Pendiente,
    Pagada,
    EnRecargo,     // Con recargo por mora
    EnDisputa,
    Anulada
}

public record AmetAccident(
    string NumeroReporte,
    DateTime Fecha,
    string Ubicacion,
    AccidentSeverity Severidad,
    string Descripcion,
    bool FueResponsable,
    List<string> OtrosVehiculosInvolucrados,
    bool TuvoHeridos,
    bool TuvoFatalidades
);

public enum AccidentSeverity
{
    Leve,          // Solo daños materiales menores
    Moderado,      // Daños materiales significativos
    Grave,         // Con heridos
    MuyGrave       // Con fatalidades
}

public enum AmetRiskLevel
{
    Bajo,          // 0 multas, 0 accidentes
    Normal,        // Pocas multas menores
    Elevado,       // Múltiples multas o 1 accidente
    Alto           // Accidentes graves o muchas multas
}
```

---

## 🔧 Service Interface

```csharp
namespace VehicleVerificationService.Domain.Interfaces;

public interface IAmetService
{
    /// <summary>
    /// Obtiene historial completo de multas y accidentes
    /// </summary>
    Task<AmetVehicleHistory> GetVehicleHistoryAsync(string placa);

    /// <summary>
    /// Obtiene solo las multas pendientes
    /// </summary>
    Task<List<AmetMulta>> GetPendingMultasAsync(string placa);

    /// <summary>
    /// Verifica si el vehículo tiene accidentes reportados
    /// </summary>
    Task<bool> HasAccidentsAsync(string placa);

    /// <summary>
    /// Calcula el nivel de riesgo basado en historial
    /// </summary>
    Task<AmetRiskLevel> CalculateRiskLevelAsync(string placa);

    /// <summary>
    /// Obtiene total de deuda de multas
    /// </summary>
    Task<decimal> GetTotalDebtAsync(string placa);
}
```

---

## 🏗️ Service Implementation

```csharp
namespace VehicleVerificationService.Infrastructure.Services;

public class AmetService : IAmetService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<AmetService> _logger;

    private const string AMET_URL = "https://amet.gob.do/servicios/consulta-multas";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(6);

    public AmetService(
        HttpClient httpClient,
        IMemoryCache cache,
        ILogger<AmetService> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _logger = logger;
    }

    public async Task<AmetVehicleHistory> GetVehicleHistoryAsync(string placa)
    {
        var cacheKey = $"amet_history_{placa}";

        if (_cache.TryGetValue(cacheKey, out AmetVehicleHistory? cached))
            return cached!;

        try
        {
            // 1. Obtener página inicial
            var initialResponse = await _httpClient.GetAsync(AMET_URL);
            var initialHtml = await initialResponse.Content.ReadAsStringAsync();

            var doc = new HtmlDocument();
            doc.LoadHtml(initialHtml);

            // Extraer tokens del formulario
            var viewState = ExtractFormValue(doc, "__VIEWSTATE");
            var eventValidation = ExtractFormValue(doc, "__EVENTVALIDATION");

            // 2. Enviar consulta
            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("__VIEWSTATE", viewState),
                new KeyValuePair<string, string>("__EVENTVALIDATION", eventValidation),
                new KeyValuePair<string, string>("txtPlaca", placa.ToUpperInvariant()),
                new KeyValuePair<string, string>("btnConsultar", "Consultar")
            });

            var response = await _httpClient.PostAsync(AMET_URL, formData);
            var html = await response.Content.ReadAsStringAsync();

            // 3. Parsear resultados
            var multas = ParseMultas(html);
            var accidentes = ParseAccidentes(html);
            var riskLevel = CalculateRiskLevel(multas, accidentes);

            var result = new AmetVehicleHistory(
                Placa: placa,
                TotalMultas: multas.Count,
                MultasPendientes: multas.Count(m => m.Estado == AmetMultaStatus.Pendiente 
                    || m.Estado == AmetMultaStatus.EnRecargo),
                MontoTotalAdeudado: multas
                    .Where(m => m.Estado != AmetMultaStatus.Pagada 
                        && m.Estado != AmetMultaStatus.Anulada)
                    .Sum(m => m.MontoTotal),
                AccidentesReportados: accidentes.Count,
                Multas: multas,
                Accidentes: accidentes,
                NivelRiesgo: riskLevel,
                ConsultadoEn: DateTime.UtcNow
            );

            _cache.Set(cacheKey, result, CacheDuration);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error consultando AMET para {Placa}", placa);

            return new AmetVehicleHistory(
                Placa: placa,
                TotalMultas: 0,
                MultasPendientes: 0,
                MontoTotalAdeudado: 0,
                AccidentesReportados: 0,
                Multas: new(),
                Accidentes: new(),
                NivelRiesgo: AmetRiskLevel.Bajo,
                ConsultadoEn: DateTime.UtcNow
            );
        }
    }

    public async Task<List<AmetMulta>> GetPendingMultasAsync(string placa)
    {
        var history = await GetVehicleHistoryAsync(placa);
        return history.Multas
            .Where(m => m.Estado == AmetMultaStatus.Pendiente 
                || m.Estado == AmetMultaStatus.EnRecargo)
            .ToList();
    }

    public async Task<bool> HasAccidentsAsync(string placa)
    {
        var history = await GetVehicleHistoryAsync(placa);
        return history.AccidentesReportados > 0;
    }

    public async Task<AmetRiskLevel> CalculateRiskLevelAsync(string placa)
    {
        var history = await GetVehicleHistoryAsync(placa);
        return history.NivelRiesgo;
    }

    public async Task<decimal> GetTotalDebtAsync(string placa)
    {
        var history = await GetVehicleHistoryAsync(placa);
        return history.MontoTotalAdeudado;
    }

    private static AmetRiskLevel CalculateRiskLevel(
        List<AmetMulta> multas, 
        List<AmetAccident> accidentes)
    {
        var score = 0;

        // Puntos por multas pendientes
        score += multas.Count(m => m.Estado == AmetMultaStatus.Pendiente) * 2;
        score += multas.Count(m => m.Estado == AmetMultaStatus.EnRecargo) * 3;

        // Puntos por tipo de infracción
        score += multas.Count(m => m.Tipo == InfraccionTipo.Alcoholemia) * 10;
        score += multas.Count(m => m.Tipo == InfraccionTipo.Velocidad) * 3;

        // Puntos por accidentes
        foreach (var acc in accidentes)
        {
            score += acc.Severidad switch
            {
                AccidentSeverity.Leve => 5,
                AccidentSeverity.Moderado => 10,
                AccidentSeverity.Grave => 20,
                AccidentSeverity.MuyGrave => 50,
                _ => 0
            };

            if (acc.FueResponsable)
                score += 10;
        }

        return score switch
        {
            0 => AmetRiskLevel.Bajo,
            <= 10 => AmetRiskLevel.Normal,
            <= 25 => AmetRiskLevel.Elevado,
            _ => AmetRiskLevel.Alto
        };
    }

    private List<AmetMulta> ParseMultas(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var multas = new List<AmetMulta>();
        var rows = doc.DocumentNode.SelectNodes("//table[@id='tblMultas']//tr");

        if (rows == null) return multas;

        foreach (var row in rows.Skip(1)) // Skip header
        {
            var cells = row.SelectNodes("td");
            if (cells == null || cells.Count < 7) continue;

            multas.Add(new AmetMulta(
                NumeroMulta: cells[0].InnerText.Trim(),
                Fecha: ParseDate(cells[1].InnerText.Trim()),
                Infraccion: cells[2].InnerText.Trim(),
                Tipo: ParseInfraccionTipo(cells[2].InnerText.Trim()),
                Ubicacion: cells[3].InnerText.Trim(),
                MontoOriginal: ParseMonto(cells[4].InnerText.Trim()),
                Recargo: ParseMonto(cells[5].InnerText.Trim()),
                MontoTotal: ParseMonto(cells[6].InnerText.Trim()),
                Estado: ParseMultaStatus(cells[7]?.InnerText.Trim()),
                FechaPago: cells.Count > 8 ? ParseNullableDate(cells[8].InnerText) : null
            ));
        }

        return multas;
    }

    private List<AmetAccident> ParseAccidentes(string html)
    {
        // Similar parsing para accidentes
        return new List<AmetAccident>();
    }

    private static InfraccionTipo ParseInfraccionTipo(string descripcion)
    {
        var desc = descripcion.ToUpperInvariant();
        
        if (desc.Contains("VELOCIDAD") || desc.Contains("EXCESO"))
            return InfraccionTipo.Velocidad;
        if (desc.Contains("ESTACION"))
            return InfraccionTipo.Estacionamiento;
        if (desc.Contains("SEMAFORO") || desc.Contains("LUZ ROJA"))
            return InfraccionTipo.SemaforoRojo;
        if (desc.Contains("LICENCIA") || desc.Contains("DOCUMENTO"))
            return InfraccionTipo.Documentos;
        if (desc.Contains("CINTUR"))
            return InfraccionTipo.CinturonSeguridad;
        if (desc.Contains("CELULAR") || desc.Contains("TELEFONO"))
            return InfraccionTipo.UsoCelular;
        if (desc.Contains("ALCOHOL") || desc.Contains("EMBRIAGUEZ"))
            return InfraccionTipo.Alcoholemia;
        
        return InfraccionTipo.Otras;
    }
}
```

---

## ⚛️ React Component

```tsx
// components/AmetHistoryBadge.tsx
import { useQuery } from '@tanstack/react-query';
import { ametService } from '@/services/ametService';
import { 
  CheckCircle, AlertTriangle, XCircle, Car, FileWarning 
} from 'lucide-react';

interface Props {
  placa: string;
  showDetails?: boolean;
}

export function AmetHistoryBadge({ placa, showDetails = false }: Props) {
  const { data: history, isLoading } = useQuery({
    queryKey: ['amet-history', placa],
    queryFn: () => ametService.getVehicleHistory(placa),
    staleTime: 6 * 60 * 60 * 1000, // 6 horas
  });

  if (isLoading) {
    return <div className="animate-pulse bg-gray-200 rounded w-36 h-6" />;
  }

  const riskConfig = {
    Bajo: {
      icon: CheckCircle,
      text: 'Sin Incidentes',
      color: 'bg-green-100 text-green-800',
      iconColor: 'text-green-600',
    },
    Normal: {
      icon: CheckCircle,
      text: 'Historial Normal',
      color: 'bg-blue-100 text-blue-800',
      iconColor: 'text-blue-600',
    },
    Elevado: {
      icon: AlertTriangle,
      text: 'Revisar Historial',
      color: 'bg-yellow-100 text-yellow-800',
      iconColor: 'text-yellow-600',
    },
    Alto: {
      icon: XCircle,
      text: 'Riesgo Alto',
      color: 'bg-red-100 text-red-800',
      iconColor: 'text-red-600',
    },
  };

  const config = riskConfig[history?.nivelRiesgo || 'Bajo'];
  const Icon = config.icon;

  return (
    <div className="space-y-3">
      <div className={`inline-flex items-center gap-2 px-3 py-1.5 
                       rounded-full ${config.color}`}>
        <Icon className={`w-4 h-4 ${config.iconColor}`} />
        <span className="text-sm font-medium">{config.text}</span>
      </div>

      {/* Alertas importantes */}
      {history?.accidentesReportados > 0 && (
        <div className="flex items-center gap-2 text-red-600 text-sm">
          <Car className="w-4 h-4" />
          <span className="font-medium">
            {history.accidentesReportados} accidente(s) reportado(s)
          </span>
        </div>
      )}

      {history?.multasPendientes > 0 && (
        <div className="flex items-center gap-2 text-yellow-600 text-sm">
          <FileWarning className="w-4 h-4" />
          <span>
            {history.multasPendientes} multa(s) pendiente(s) - 
            RD$ {history.montoTotalAdeudado.toLocaleString()}
          </span>
        </div>
      )}

      {showDetails && history && (
        <div className="bg-gray-50 rounded-lg p-4 space-y-4">
          {/* Resumen */}
          <div className="grid grid-cols-3 gap-4 text-center">
            <div>
              <p className="text-2xl font-bold text-gray-900">
                {history.totalMultas}
              </p>
              <p className="text-xs text-gray-500">Total Multas</p>
            </div>
            <div>
              <p className={`text-2xl font-bold ${
                history.multasPendientes > 0 ? 'text-red-600' : 'text-green-600'
              }`}>
                {history.multasPendientes}
              </p>
              <p className="text-xs text-gray-500">Pendientes</p>
            </div>
            <div>
              <p className={`text-2xl font-bold ${
                history.accidentesReportados > 0 ? 'text-red-600' : 'text-green-600'
              }`}>
                {history.accidentesReportados}
              </p>
              <p className="text-xs text-gray-500">Accidentes</p>
            </div>
          </div>

          {/* Lista de multas recientes */}
          {history.multas.length > 0 && (
            <div>
              <h4 className="font-medium text-sm mb-2">Multas Recientes</h4>
              <div className="space-y-2">
                {history.multas.slice(0, 3).map((multa, i) => (
                  <div key={i} className="flex justify-between text-sm 
                                          p-2 bg-white rounded">
                    <div>
                      <p className="font-medium">{multa.infraccion}</p>
                      <p className="text-xs text-gray-500">
                        {new Date(multa.fecha).toLocaleDateString('es-DO')}
                      </p>
                    </div>
                    <div className="text-right">
                      <p className={`font-medium ${
                        multa.estado === 'Pagada' 
                          ? 'text-green-600' : 'text-red-600'
                      }`}>
                        RD$ {multa.montoTotal.toLocaleString()}
                      </p>
                      <p className="text-xs">{multa.estado}</p>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          )}

          {/* Accidentes */}
          {history.accidentes.length > 0 && (
            <div className="p-3 bg-red-50 rounded border border-red-200">
              <h4 className="font-medium text-red-800 mb-2">
                ⚠️ Accidentes Reportados
              </h4>
              {history.accidentes.map((acc, i) => (
                <div key={i} className="text-sm text-red-700">
                  <p className="font-medium">{acc.descripcion}</p>
                  <p className="text-xs">
                    {new Date(acc.fecha).toLocaleDateString('es-DO')} - 
                    {acc.ubicacion}
                  </p>
                </div>
              ))}
            </div>
          )}
        </div>
      )}
    </div>
  );
}
```

---

## 🔌 API Controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class AmetController : ControllerBase
{
    private readonly IAmetService _ametService;

    public AmetController(IAmetService ametService)
    {
        _ametService = ametService;
    }

    [HttpGet("history/{placa}")]
    public async Task<ActionResult<AmetVehicleHistory>> GetHistory(string placa)
    {
        var result = await _ametService.GetVehicleHistoryAsync(placa);
        return Ok(result);
    }

    [HttpGet("multas/{placa}")]
    public async Task<ActionResult<List<AmetMulta>>> GetMultas(string placa)
    {
        var result = await _ametService.GetPendingMultasAsync(placa);
        return Ok(result);
    }

    [HttpGet("accidents/{placa}")]
    public async Task<ActionResult<bool>> HasAccidents(string placa)
    {
        var result = await _ametService.HasAccidentsAsync(placa);
        return Ok(new { hasAccidents = result });
    }

    [HttpGet("risk/{placa}")]
    public async Task<ActionResult> GetRiskLevel(string placa)
    {
        var result = await _ametService.CalculateRiskLevelAsync(placa);
        return Ok(new { riskLevel = result.ToString() });
    }

    [HttpGet("debt/{placa}")]
    public async Task<ActionResult> GetTotalDebt(string placa)
    {
        var result = await _ametService.GetTotalDebtAsync(placa);
        return Ok(new { totalDebt = result });
    }
}
```

---

## 🧪 Tests

```csharp
public class AmetServiceTests
{
    [Theory]
    [InlineData(0, 0, AmetRiskLevel.Bajo)]
    [InlineData(5, 0, AmetRiskLevel.Normal)]
    [InlineData(15, 1, AmetRiskLevel.Elevado)]
    [InlineData(30, 2, AmetRiskLevel.Alto)]
    public void CalculateRiskLevel_ReturnsCorrectLevel(
        int multasPendientes, int accidentes, AmetRiskLevel expected)
    {
        var multas = Enumerable.Range(0, multasPendientes)
            .Select(_ => new AmetMulta(
                "M001", DateTime.Now, "Velocidad", InfraccionTipo.Velocidad,
                "Ubicación", 2000, 0, 2000, AmetMultaStatus.Pendiente, null
            )).ToList();

        var accs = Enumerable.Range(0, accidentes)
            .Select(_ => new AmetAccident(
                "A001", DateTime.Now, "Ubicación", AccidentSeverity.Moderado,
                "Descripción", false, new(), false, false
            )).ToList();

        var result = AmetService.CalculateRiskLevel(multas, accs);

        result.Should().Be(expected);
    }

    [Fact]
    public void ParseInfraccionTipo_Alcoholemia_ReturnsCorrectType()
    {
        var result = AmetService.ParseInfraccionTipo("CONDUCIR EN ESTADO DE EMBRIAGUEZ");
        result.Should().Be(InfraccionTipo.Alcoholemia);
    }
}
```

---

## 📞 Contacto AMET

| Departamento | Teléfono |
|--------------|----------|
| Consultas | 809-686-4646 |
| Emergencias | 911 |

---

**Anterior:** [INTRANT_API.md](./INTRANT_API.md)  
**Siguiente:** [JCE_CEDULA_API.md](./JCE_CEDULA_API.md)
