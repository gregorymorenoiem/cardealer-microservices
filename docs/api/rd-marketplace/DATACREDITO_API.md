# 💳 Data Crédito - Buró de Crédito RD

**Entidad:** Data Crédito (TransUnion República Dominicana)  
**Website:** [datacredito.com.do](https://datacredito.com.do)  
**Uso:** Score crediticio, pre-aprobación de financiamiento  
**Prioridad:** ⭐⭐⭐⭐⭐ CRÍTICA (💰 MONETIZACIÓN)

---

## 📋 Información General

| Campo | Valor |
|-------|-------|
| **Website** | [datacredito.com.do](https://datacredito.com.do) |
| **API** | Requiere convenio comercial |
| **Método** | REST API (OAuth 2.0) |
| **Costo** | $0.50 - $1.00 USD por consulta |

---

## 📊 Datos Obtenibles

| Dato | Descripción | Uso en OKLA |
|------|-------------|-------------|
| **Score Crediticio** | 300-850 puntos | Pre-aprobación instantánea |
| **Rango** | Excelente/Bueno/Regular/Deficiente | Elegibilidad financiamiento |
| **Capacidad Endeudamiento** | Monto máximo a financiar | Mostrar límite |
| **Deuda Actual** | Total de deudas vigentes | Calcular cuotas |
| **Historial Morosidad** | Si ha tenido atrasos | Riesgo crediticio |
| **Cuentas Activas** | Cantidad de créditos | Perfil financiero |

---

## 🌐 API Endpoints

```http
# Base URL (requiere convenio)
https://api.datacredito.com.do/v1/

# Autenticación OAuth 2.0
POST /oauth/token
Content-Type: application/x-www-form-urlencoded

grant_type=client_credentials
&client_id={CLIENT_ID}
&client_secret={CLIENT_SECRET}

# Consulta Soft (no afecta score)
POST /credit/soft-inquiry
Authorization: Bearer {access_token}
Content-Type: application/json

{
  "cedula": "00100000001",
  "consentimiento": true,
  "proposito": "PRE_APROBACION_VEHICULO"
}

# Response
{
  "consultaId": "DC-2026-123456",
  "cedula": "001****0001",
  "score": 720,
  "rango": "BUENO",
  "capacidadEndeudamiento": 500000,
  "deudaActual": 150000,
  "disponible": 350000,
  "historialMorosidad": false,
  "cuentasActivas": 3,
  "antiguedadCrediticia": "5 años 3 meses",
  "recomendacion": "APROBADO_CON_CONDICIONES",
  "timestamp": "2026-01-15T10:30:00Z"
}

# Consulta Hard (afecta score - solo con autorización)
POST /credit/hard-inquiry
Authorization: Bearer {access_token}
{
  "cedula": "00100000001",
  "montoSolicitado": 800000,
  "plazoMeses": 60,
  "tipoCredito": "VEHICULO",
  "consentimientoFirmado": true
}
```

---

## 💻 Modelos C#

```csharp
namespace FinancingService.Domain.Entities;

/// <summary>
/// Resultado de consulta crediticia
/// </summary>
public record CreditScore(
    string ConsultaId,
    string CedulaMasked,
    int Score,
    CreditRating Rating,
    decimal CapacidadEndeudamiento,
    decimal DeudaActual,
    decimal Disponible,
    bool TieneHistorialMorosidad,
    int CuentasActivas,
    string AntiguedadCrediticia,
    CreditRecommendation Recomendacion,
    DateTime ConsultadoEn
);

public enum CreditRating
{
    Excelente,   // 750-850
    Bueno,       // 670-749
    Regular,     // 580-669
    Deficiente,  // 300-579
    SinHistorial
}

public enum CreditRecommendation
{
    Aprobado,
    AprobadoConCondiciones,
    RequiereGarantia,
    RequiereCodeudor,
    Rechazado,
    SinDeterminar
}

/// <summary>
/// Resultado de pre-aprobación de financiamiento
/// </summary>
public record PreApprovalResult(
    bool IsEligible,
    CreditScore CreditScore,
    decimal MontoMaximoAprobado,
    decimal TasaEstimada,
    int PlazoMaximoMeses,
    decimal CuotaEstimadaMinima,
    decimal CuotaEstimadaMaxima,
    List<string> Requisitos,
    List<FinancingOption> OpcionesFinanciamiento,
    DateTime ValidoHasta
);

public record FinancingOption(
    string EntidadFinanciera,
    decimal MontoAprobado,
    decimal TasaAnual,
    int PlazoMeses,
    decimal CuotaMensual,
    string? PromocionEspecial
);

/// <summary>
/// Request para pre-aprobación
/// </summary>
public record PreApprovalRequest(
    string Cedula,
    decimal MontoVehiculo,
    decimal Inicial,         // Down payment
    int PlazoDeseadoMeses,
    decimal IngresoMensual,
    bool ConsentimientoConsulta
);
```

---

## 🔧 Service Interface

```csharp
namespace FinancingService.Domain.Interfaces;

public interface IDataCreditoService
{
    /// <summary>
    /// Obtiene score crediticio (soft inquiry)
    /// </summary>
    Task<CreditScore?> GetCreditScoreAsync(string cedula);

    /// <summary>
    /// Calcula pre-aprobación de financiamiento
    /// </summary>
    Task<PreApprovalResult> GetPreApprovalAsync(PreApprovalRequest request);

    /// <summary>
    /// Verifica si usuario es elegible para financiamiento
    /// </summary>
    Task<bool> IsEligibleForFinancingAsync(string cedula, decimal monto);

    /// <summary>
    /// Calcula cuota mensual estimada
    /// </summary>
    decimal CalculateMonthlyPayment(
        decimal principal, 
        decimal annualRate, 
        int months);
}
```

---

## 🏗️ Service Implementation

```csharp
namespace FinancingService.Infrastructure.Services;

public class DataCreditoService : IDataCreditoService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _config;
    private readonly ILogger<DataCreditoService> _logger;

    private string? _accessToken;
    private DateTime _tokenExpiry = DateTime.MinValue;

    public DataCreditoService(
        HttpClient httpClient,
        IMemoryCache cache,
        IConfiguration config,
        ILogger<DataCreditoService> logger)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(config["DataCredito:BaseUrl"]!);
        _cache = cache;
        _config = config;
        _logger = logger;
    }

    public async Task<CreditScore?> GetCreditScoreAsync(string cedula)
    {
        var cacheKey = $"credit_score_{cedula}";

        // Cache por 24 horas (soft inquiry no cambia frecuentemente)
        if (_cache.TryGetValue(cacheKey, out CreditScore? cached))
            return cached;

        try
        {
            await EnsureAuthenticatedAsync();

            var request = new
            {
                cedula = cedula,
                consentimiento = true,
                proposito = "PRE_APROBACION_VEHICULO"
            };

            var response = await _httpClient.PostAsJsonAsync(
                "credit/soft-inquiry", request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "DataCredito returned {Status} for {Cedula}", 
                    response.StatusCode, MaskCedula(cedula));
                return null;
            }

            var result = await response.Content
                .ReadFromJsonAsync<DataCreditoResponse>();

            if (result == null) return null;

            var score = MapToScore(result);
            _cache.Set(cacheKey, score, TimeSpan.FromHours(24));

            return score;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error consultando DataCredito");
            return null;
        }
    }

    public async Task<PreApprovalResult> GetPreApprovalAsync(
        PreApprovalRequest request)
    {
        // 1. Obtener score crediticio
        var score = await GetCreditScoreAsync(request.Cedula);

        if (score == null)
        {
            return new PreApprovalResult(
                IsEligible: false,
                CreditScore: null!,
                MontoMaximoAprobado: 0,
                TasaEstimada: 0,
                PlazoMaximoMeses: 0,
                CuotaEstimadaMinima: 0,
                CuotaEstimadaMaxima: 0,
                Requisitos: new() { "No se pudo verificar historial crediticio" },
                OpcionesFinanciamiento: new(),
                ValidoHasta: DateTime.UtcNow
            );
        }

        // 2. Calcular elegibilidad
        var montoFinanciar = request.MontoVehiculo - request.Inicial;
        var isEligible = score.Rating != CreditRating.Deficiente 
            && montoFinanciar <= score.Disponible;

        // 3. Calcular tasa según score
        var tasaBase = GetTasaBaseByScore(score.Score);

        // 4. Calcular monto máximo aprobado
        var montoMaximo = Math.Min(montoFinanciar, score.Disponible);

        // 5. Validar que cuota no exceda 40% del ingreso
        var cuotaMaximaPermitida = request.IngresoMensual * 0.4m;
        var cuotaCalculada = CalculateMonthlyPayment(
            montoMaximo, tasaBase, request.PlazoDeseadoMeses);

        if (cuotaCalculada > cuotaMaximaPermitida)
        {
            // Ajustar monto o plazo
            montoMaximo = CalculateMaxPrincipal(
                cuotaMaximaPermitida, tasaBase, request.PlazoDeseadoMeses);
        }

        // 6. Generar opciones de financiamiento
        var opciones = GenerateFinancingOptions(
            montoMaximo, tasaBase, request.PlazoDeseadoMeses, score.Rating);

        // 7. Determinar requisitos
        var requisitos = GetRequirements(score);

        return new PreApprovalResult(
            IsEligible: isEligible && montoMaximo > 0,
            CreditScore: score,
            MontoMaximoAprobado: montoMaximo,
            TasaEstimada: tasaBase,
            PlazoMaximoMeses: GetMaxPlazo(score.Rating),
            CuotaEstimadaMinima: CalculateMonthlyPayment(
                montoMaximo, tasaBase, 72), // 72 meses
            CuotaEstimadaMaxima: CalculateMonthlyPayment(
                montoMaximo, tasaBase, 36), // 36 meses
            Requisitos: requisitos,
            OpcionesFinanciamiento: opciones,
            ValidoHasta: DateTime.UtcNow.AddDays(30)
        );
    }

    public async Task<bool> IsEligibleForFinancingAsync(
        string cedula, decimal monto)
    {
        var score = await GetCreditScoreAsync(cedula);
        if (score == null) return false;

        return score.Rating != CreditRating.Deficiente 
            && monto <= score.Disponible;
    }

    public decimal CalculateMonthlyPayment(
        decimal principal, 
        decimal annualRate, 
        int months)
    {
        if (principal <= 0 || months <= 0) return 0;

        var monthlyRate = annualRate / 100 / 12;
        
        if (monthlyRate == 0)
            return principal / months;

        var payment = principal * monthlyRate * 
            (decimal)Math.Pow((double)(1 + monthlyRate), months) /
            ((decimal)Math.Pow((double)(1 + monthlyRate), months) - 1);

        return Math.Round(payment, 2);
    }

    private async Task EnsureAuthenticatedAsync()
    {
        if (_accessToken != null && DateTime.UtcNow < _tokenExpiry)
            return;

        var tokenRequest = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("client_id", 
                _config["DataCredito:ClientId"]!),
            new KeyValuePair<string, string>("client_secret", 
                _config["DataCredito:ClientSecret"]!)
        });

        var response = await _httpClient.PostAsync("oauth/token", tokenRequest);
        response.EnsureSuccessStatusCode();

        var tokenResponse = await response.Content
            .ReadFromJsonAsync<TokenResponse>();

        _accessToken = tokenResponse!.AccessToken;
        _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 60);

        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", _accessToken);
    }

    private static decimal GetTasaBaseByScore(int score)
    {
        return score switch
        {
            >= 750 => 10.0m,  // Excelente
            >= 670 => 12.5m,  // Bueno
            >= 580 => 16.0m,  // Regular
            _ => 22.0m        // Deficiente
        };
    }

    private static int GetMaxPlazo(CreditRating rating)
    {
        return rating switch
        {
            CreditRating.Excelente => 72,
            CreditRating.Bueno => 60,
            CreditRating.Regular => 48,
            _ => 36
        };
    }

    private List<FinancingOption> GenerateFinancingOptions(
        decimal monto, 
        decimal tasaBase, 
        int plazoDeseado,
        CreditRating rating)
    {
        var options = new List<FinancingOption>();

        // Banco Popular
        options.Add(new FinancingOption(
            EntidadFinanciera: "Banco Popular",
            MontoAprobado: monto,
            TasaAnual: tasaBase,
            PlazoMeses: plazoDeseado,
            CuotaMensual: CalculateMonthlyPayment(monto, tasaBase, plazoDeseado),
            PromocionEspecial: rating == CreditRating.Excelente 
                ? "0% inicial por 3 meses" : null
        ));

        // Banreservas
        options.Add(new FinancingOption(
            EntidadFinanciera: "Banreservas",
            MontoAprobado: monto,
            TasaAnual: tasaBase + 0.5m,
            PlazoMeses: plazoDeseado,
            CuotaMensual: CalculateMonthlyPayment(monto, tasaBase + 0.5m, plazoDeseado),
            PromocionEspecial: null
        ));

        // APAP
        options.Add(new FinancingOption(
            EntidadFinanciera: "APAP",
            MontoAprobado: monto * 0.9m, // 90% del monto
            TasaAnual: tasaBase - 0.5m,
            PlazoMeses: Math.Min(plazoDeseado, 48),
            CuotaMensual: CalculateMonthlyPayment(
                monto * 0.9m, tasaBase - 0.5m, Math.Min(plazoDeseado, 48)),
            PromocionEspecial: "Tasa preferencial socios"
        ));

        return options.OrderBy(o => o.CuotaMensual).ToList();
    }

    private static List<string> GetRequirements(CreditScore score)
    {
        var requirements = new List<string>
        {
            "Cédula de identidad",
            "Comprobante de ingresos (últimos 3 meses)",
            "Estados de cuenta bancarios"
        };

        if (score.Rating == CreditRating.Regular)
        {
            requirements.Add("Carta de trabajo");
            requirements.Add("Referencia bancaria");
        }

        if (score.TieneHistorialMorosidad)
        {
            requirements.Add("Carta explicativa de morosidad");
        }

        return requirements;
    }

    private decimal CalculateMaxPrincipal(
        decimal maxPayment, 
        decimal annualRate, 
        int months)
    {
        var monthlyRate = annualRate / 100 / 12;
        if (monthlyRate == 0) return maxPayment * months;

        return maxPayment * 
            ((decimal)Math.Pow((double)(1 + monthlyRate), months) - 1) /
            (monthlyRate * (decimal)Math.Pow((double)(1 + monthlyRate), months));
    }

    private static string MaskCedula(string cedula)
    {
        if (cedula.Length < 11) return "***";
        return $"{cedula[..3]}****{cedula[^4..]}";
    }
}

// DTOs para respuestas de API
internal record DataCreditoResponse(
    string ConsultaId,
    int Score,
    string Rango,
    decimal CapacidadEndeudamiento,
    decimal DeudaActual,
    bool HistorialMorosidad,
    int CuentasActivas,
    string AntiguedadCrediticia,
    string Recomendacion
);

internal record TokenResponse(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("expires_in")] int ExpiresIn
);
```

---

## ⚛️ React Component

```tsx
// components/FinancingCalculator.tsx
import { useState } from 'react';
import { useQuery, useMutation } from '@tanstack/react-query';
import { financingService } from '@/services/financingService';
import { 
  Calculator, DollarSign, TrendingUp, CheckCircle, XCircle 
} from 'lucide-react';

interface Props {
  vehiclePrice: number;
  onPreApproval?: (result: PreApprovalResult) => void;
}

export function FinancingCalculator({ vehiclePrice, onPreApproval }: Props) {
  const [cedula, setCedula] = useState('');
  const [inicial, setInicial] = useState(vehiclePrice * 0.2); // 20% default
  const [plazo, setPlazo] = useState(48);
  const [ingreso, setIngreso] = useState(0);
  const [consent, setConsent] = useState(false);

  const preApprovalMutation = useMutation({
    mutationFn: () => financingService.getPreApproval({
      cedula,
      montoVehiculo: vehiclePrice,
      inicial,
      plazoDeseadoMeses: plazo,
      ingresoMensual: ingreso,
      consentimientoConsulta: consent,
    }),
    onSuccess: (result) => {
      onPreApproval?.(result);
    },
  });

  const montoFinanciar = vehiclePrice - inicial;
  const estimatedPayment = financingService.calculatePayment(
    montoFinanciar, 12.5, plazo
  );

  return (
    <div className="bg-white rounded-xl shadow-lg p-6 space-y-6">
      <div className="flex items-center gap-3">
        <div className="p-3 bg-blue-100 rounded-full">
          <Calculator className="w-6 h-6 text-blue-600" />
        </div>
        <div>
          <h3 className="text-lg font-bold">Calculadora de Financiamiento</h3>
          <p className="text-sm text-gray-500">
            Obtén pre-aprobación en segundos
          </p>
        </div>
      </div>

      {/* Resumen del vehículo */}
      <div className="bg-gray-50 rounded-lg p-4">
        <div className="flex justify-between">
          <span className="text-gray-600">Precio del Vehículo</span>
          <span className="font-bold">
            RD$ {vehiclePrice.toLocaleString()}
          </span>
        </div>
      </div>

      {/* Inicial */}
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-2">
          Inicial (mínimo 20%)
        </label>
        <div className="flex items-center gap-4">
          <input
            type="range"
            min={vehiclePrice * 0.2}
            max={vehiclePrice * 0.5}
            step={10000}
            value={inicial}
            onChange={(e) => setInicial(Number(e.target.value))}
            className="flex-1"
          />
          <span className="w-32 text-right font-medium">
            RD$ {inicial.toLocaleString()}
          </span>
        </div>
        <p className="text-xs text-gray-500 mt-1">
          {Math.round((inicial / vehiclePrice) * 100)}% del precio
        </p>
      </div>

      {/* Plazo */}
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-2">
          Plazo (meses)
        </label>
        <div className="grid grid-cols-4 gap-2">
          {[36, 48, 60, 72].map((months) => (
            <button
              key={months}
              onClick={() => setPlazo(months)}
              className={`py-2 rounded-lg border transition-colors ${
                plazo === months
                  ? 'bg-blue-600 text-white border-blue-600'
                  : 'bg-white text-gray-700 border-gray-300 hover:border-blue-300'
              }`}
            >
              {months} meses
            </button>
          ))}
        </div>
      </div>

      {/* Estimación rápida */}
      <div className="bg-blue-50 rounded-lg p-4">
        <p className="text-sm text-blue-600 mb-1">Cuota Estimada*</p>
        <p className="text-3xl font-bold text-blue-700">
          RD$ {estimatedPayment.toLocaleString()}/mes
        </p>
        <p className="text-xs text-blue-500 mt-1">
          *Estimación con tasa de 12.5%. La tasa final depende de tu score.
        </p>
      </div>

      <hr />

      {/* Formulario de pre-aprobación */}
      <div className="space-y-4">
        <h4 className="font-medium">Solicitar Pre-Aprobación</h4>
        
        <div>
          <label className="block text-sm text-gray-600 mb-1">
            Cédula
          </label>
          <input
            type="text"
            value={cedula}
            onChange={(e) => setCedula(e.target.value.replace(/\D/g, ''))}
            placeholder="000-0000000-0"
            maxLength={11}
            className="w-full px-4 py-2 border rounded-lg"
          />
        </div>

        <div>
          <label className="block text-sm text-gray-600 mb-1">
            Ingreso Mensual (RD$)
          </label>
          <input
            type="number"
            value={ingreso || ''}
            onChange={(e) => setIngreso(Number(e.target.value))}
            placeholder="50,000"
            className="w-full px-4 py-2 border rounded-lg"
          />
        </div>

        <label className="flex items-start gap-2 text-sm">
          <input
            type="checkbox"
            checked={consent}
            onChange={(e) => setConsent(e.target.checked)}
            className="mt-1"
          />
          <span className="text-gray-600">
            Autorizo la consulta de mi historial crediticio en Data Crédito 
            para fines de pre-aprobación de financiamiento.
          </span>
        </label>

        <button
          onClick={() => preApprovalMutation.mutate()}
          disabled={!consent || cedula.length !== 11 || ingreso <= 0 
            || preApprovalMutation.isPending}
          className="w-full py-3 bg-green-600 text-white rounded-lg 
                     font-medium hover:bg-green-700 disabled:opacity-50 
                     disabled:cursor-not-allowed flex items-center 
                     justify-center gap-2"
        >
          {preApprovalMutation.isPending ? (
            <>
              <div className="animate-spin w-5 h-5 border-2 border-white 
                              border-t-transparent rounded-full" />
              Consultando...
            </>
          ) : (
            <>
              <TrendingUp className="w-5 h-5" />
              Obtener Pre-Aprobación
            </>
          )}
        </button>
      </div>

      {/* Resultado */}
      {preApprovalMutation.data && (
        <PreApprovalResult result={preApprovalMutation.data} />
      )}
    </div>
  );
}

function PreApprovalResult({ result }: { result: PreApprovalResult }) {
  if (!result.isEligible) {
    return (
      <div className="bg-red-50 border border-red-200 rounded-lg p-4">
        <div className="flex items-center gap-2 text-red-700 mb-2">
          <XCircle className="w-5 h-5" />
          <span className="font-medium">No elegible para financiamiento</span>
        </div>
        <ul className="list-disc list-inside text-sm text-red-600">
          {result.requisitos.map((req, i) => (
            <li key={i}>{req}</li>
          ))}
        </ul>
      </div>
    );
  }

  return (
    <div className="bg-green-50 border border-green-200 rounded-lg p-4 space-y-4">
      <div className="flex items-center gap-2 text-green-700">
        <CheckCircle className="w-5 h-5" />
        <span className="font-medium">¡Pre-Aprobado!</span>
      </div>

      <div className="grid grid-cols-2 gap-4">
        <div>
          <p className="text-sm text-gray-600">Monto Aprobado</p>
          <p className="text-xl font-bold text-green-700">
            RD$ {result.montoMaximoAprobado.toLocaleString()}
          </p>
        </div>
        <div>
          <p className="text-sm text-gray-600">Tasa Estimada</p>
          <p className="text-xl font-bold">{result.tasaEstimada}%</p>
        </div>
        <div>
          <p className="text-sm text-gray-600">Cuota Desde</p>
          <p className="text-lg font-medium">
            RD$ {result.cuotaEstimadaMinima.toLocaleString()}/mes
          </p>
        </div>
        <div>
          <p className="text-sm text-gray-600">Score Crediticio</p>
          <p className="text-lg font-medium">
            {result.creditScore.score} ({result.creditScore.rating})
          </p>
        </div>
      </div>

      <div>
        <p className="text-sm font-medium text-gray-700 mb-2">
          Opciones de Financiamiento
        </p>
        <div className="space-y-2">
          {result.opcionesFinanciamiento.slice(0, 3).map((opt, i) => (
            <div key={i} className="flex justify-between items-center 
                                    p-2 bg-white rounded">
              <div>
                <p className="font-medium">{opt.entidadFinanciera}</p>
                <p className="text-xs text-gray-500">
                  {opt.plazoMeses} meses @ {opt.tasaAnual}%
                </p>
              </div>
              <p className="font-bold">
                RD$ {opt.cuotaMensual.toLocaleString()}/mes
              </p>
            </div>
          ))}
        </div>
      </div>

      <p className="text-xs text-gray-500">
        Válido hasta: {new Date(result.validoHasta).toLocaleDateString('es-DO')}
      </p>
    </div>
  );
}
```

---

## 🔌 API Controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class FinancingController : ControllerBase
{
    private readonly IDataCreditoService _dataCreditoService;

    public FinancingController(IDataCreditoService dataCreditoService)
    {
        _dataCreditoService = dataCreditoService;
    }

    [HttpGet("score/{cedula}")]
    [Authorize]
    public async Task<ActionResult<CreditScore>> GetScore(string cedula)
    {
        var result = await _dataCreditoService.GetCreditScoreAsync(cedula);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    [HttpPost("pre-approval")]
    [Authorize]
    public async Task<ActionResult<PreApprovalResult>> GetPreApproval(
        [FromBody] PreApprovalRequest request)
    {
        if (!request.ConsentimientoConsulta)
            return BadRequest(new { message = "Se requiere consentimiento" });

        var result = await _dataCreditoService.GetPreApprovalAsync(request);
        return Ok(result);
    }

    [HttpGet("calculate")]
    public ActionResult CalculatePayment(
        [FromQuery] decimal principal,
        [FromQuery] decimal rate,
        [FromQuery] int months)
    {
        var payment = _dataCreditoService.CalculateMonthlyPayment(
            principal, rate, months);
        return Ok(new { monthlyPayment = payment });
    }
}
```

---

## ⚙️ Configuración

```json
{
  "DataCredito": {
    "BaseUrl": "https://api.datacredito.com.do/v1/",
    "ClientId": "okla-marketplace",
    "ClientSecret": "xxxxx",
    "CostPerQuery": 0.75
  }
}
```

---

## 📞 Contacto Data Crédito

| Departamento | Teléfono | Email |
|--------------|----------|-------|
| Comercial | 809-567-4100 | comercial@datacredito.com.do |
| Soporte API | 809-567-4101 | api@datacredito.com.do |

---

**Anterior:** [JCE_CEDULA_API.md](./JCE_CEDULA_API.md)  
**Siguiente:** [BANCO_POPULAR_AUTO_API.md](./BANCO_POPULAR_AUTO_API.md)
