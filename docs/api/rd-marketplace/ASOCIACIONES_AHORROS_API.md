# 🏦 Asociaciones de Ahorros y Préstamos (APAP)

**Entidades:** APAP, ALNAP, La Nacional, etc.  
**Regulador:** CONACOOP / SIB  
**Uso:** Financiamiento vehicular alternativo  
**Prioridad:** ⭐⭐⭐⭐ ALTA (Mayor aprobación para perfiles medios)

---

## 📋 Asociaciones Principales

| Asociación | Website | Características |
|------------|---------|-----------------|
| **APAP** | [apap.com.do](https://www.apap.com.do) | Mayor del país, tasas competitivas |
| **ALNAP** | [alnap.com.do](https://www.alnap.com.do) | Segunda más grande |
| **La Nacional** | [lanacional.com.do](https://www.lanacional.com.do) | Buen servicio |
| **ALNAP** | [coop-crecer.com](https://www.coop-crecer.com) | Cooperativa especializada |

---

## 📊 Comparativa de Productos

| Característica | APAP | ALNAP | La Nacional |
|----------------|------|-------|-------------|
| **Monto Máximo** | RD$ 8M | RD$ 5M | RD$ 6M |
| **Plazo Máximo** | 60 meses | 48 meses | 60 meses |
| **Tasa Mínima** | 9.5% | 10% | 10.5% |
| **Inicial Mínima** | 15% | 20% | 15% |
| **Tiempo Respuesta** | 48-72h | 24-48h | 48h |
| **Requiere Socio** | Sí | Sí | Sí |

---

## 🌐 API Endpoints (APAP)

```http
# Base URL
https://api.apap.com.do/v1/prestamos/

# Autenticación
POST /oauth/token
{
  "grant_type": "client_credentials",
  "client_id": "DEALER-OKLA",
  "client_secret": "xxxxx"
}

# Verificar si es socio
GET /socios/verificar/{cedula}
Authorization: Bearer {token}

# Response
{
  "esSocio": true,
  "socioDesde": "2020-03-15",
  "categoria": "PLATINUM",
  "aportesActuales": 125000,
  "creditoDisponible": 500000,
  "prestamoActivo": false
}

# Simular Préstamo Auto
POST /vehiculos/simular
{
  "cedula": "00100000001",
  "montoVehiculo": 1200000,
  "inicial": 240000,
  "plazoMeses": 48,
  "tipoVehiculo": "NUEVO"  // NUEVO | USADO
}

# Response
{
  "simulacionId": "SIM-APAP-2026-001",
  "aprobado": true,
  "montoFinanciar": 960000,
  "tasaAnual": 10.5,
  "cuotaMensual": 24650,
  "totalPagar": 1183200,
  "totalInteres": 223200,
  "seguroVida": 1200,
  "cuotaConSeguro": 25850,
  "beneficioSocio": true,
  "tasaPreferencial": true,
  "ahorroVsTasaNormal": 35400,
  "requisitos": [
    "Ser socio con mínimo 6 meses",
    "Aportes al día",
    "Carta de trabajo"
  ]
}

# Solicitar Préstamo
POST /vehiculos/solicitar
{
  "simulacionId": "SIM-APAP-2026-001",
  "dealerId": "DEALER-OKLA-001",
  "vehiculo": {
    "marca": "Toyota",
    "modelo": "Corolla",
    "año": 2024,
    "chasis": "JTDKN3DU5A0123456"
  },
  "documentos": {
    "cedulaFrente": "base64...",
    "cedulaReverso": "base64...",
    "comprobanteTrabajo": "base64...",
    "estadoCuenta": "base64..."
  }
}

# Response
{
  "solicitudId": "SOL-APAP-2026-12345",
  "estado": "EN_REVISION",
  "fechaEstimadaRespuesta": "2026-01-18",
  "documentosFaltantes": [],
  "siguientesPasos": [
    "Revisión de documentos (24-48h)",
    "Llamada de verificación",
    "Aprobación y firma de contrato"
  ]
}

# Consultar Estado
GET /vehiculos/solicitudes/{solicitudId}
```

---

## 💻 Modelos C#

```csharp
namespace FinancingService.Domain.Entities;

/// <summary>
/// Información de socio de asociación
/// </summary>
public record MembershipInfo(
    bool IsMember,
    DateTime? MemberSince,
    MemberCategory Category,
    decimal CurrentContributions,
    decimal AvailableCredit,
    bool HasActiveLoan
);

public enum MemberCategory
{
    Regular,
    Silver,
    Gold,
    Platinum
}

/// <summary>
/// Simulación de préstamo asociación
/// </summary>
public record AssociationLoanSimulation(
    string SimulationId,
    string AssociationName,
    bool IsApproved,
    decimal AmountToFinance,
    decimal AnnualRate,
    int TermMonths,
    decimal MonthlyPayment,
    decimal TotalPayment,
    decimal TotalInterest,
    decimal LifeInsurance,
    decimal PaymentWithInsurance,
    bool HasMemberBenefit,
    bool HasPreferentialRate,
    decimal? SavingsVsNormalRate,
    List<string> Requirements,
    DateTime ValidUntil
);

/// <summary>
/// Request para simulación en asociaciones
/// </summary>
public record AssociationLoanRequest(
    string Cedula,
    decimal VehicleValue,
    decimal DownPayment,
    int TermMonths,
    VehicleCondition VehicleCondition
);

public enum VehicleCondition
{
    New,
    Used
}

/// <summary>
/// Comparativa de múltiples asociaciones
/// </summary>
public record AssociationComparison(
    string Cedula,
    decimal VehicleValue,
    List<AssociationLoanSimulation> Options,
    AssociationLoanSimulation? BestOption,
    DateTime GeneratedAt
);
```

---

## 🔧 Service Interface

```csharp
namespace FinancingService.Domain.Interfaces;

public interface IAssociationFinancingService
{
    /// <summary>
    /// Verifica membresía en asociación
    /// </summary>
    Task<MembershipInfo?> VerifyMembershipAsync(
        AssociationType association, 
        string cedula);

    /// <summary>
    /// Simula préstamo en una asociación específica
    /// </summary>
    Task<AssociationLoanSimulation?> SimulateAsync(
        AssociationType association,
        AssociationLoanRequest request);

    /// <summary>
    /// Compara opciones en múltiples asociaciones
    /// </summary>
    Task<AssociationComparison> CompareOptionsAsync(
        AssociationLoanRequest request);

    /// <summary>
    /// Crea solicitud de préstamo
    /// </summary>
    Task<LoanApplication> CreateApplicationAsync(
        AssociationType association,
        string simulationId,
        VehicleInfo vehicle,
        Dictionary<string, byte[]> documents);
}

public enum AssociationType
{
    APAP,
    ALNAP,
    LaNacional,
    CoopCrecer
}
```

---

## 🏗️ Service Implementation

```csharp
namespace FinancingService.Infrastructure.Services;

public class AssociationFinancingService : IAssociationFinancingService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;
    private readonly IMemoryCache _cache;
    private readonly ILogger<AssociationFinancingService> _logger;

    private readonly Dictionary<AssociationType, string> _baseUrls;

    public AssociationFinancingService(
        IHttpClientFactory httpClientFactory,
        IConfiguration config,
        IMemoryCache cache,
        ILogger<AssociationFinancingService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _config = config;
        _cache = cache;
        _logger = logger;

        _baseUrls = new()
        {
            [AssociationType.APAP] = config["Associations:APAP:BaseUrl"]!,
            [AssociationType.ALNAP] = config["Associations:ALNAP:BaseUrl"]!,
            [AssociationType.LaNacional] = config["Associations:LaNacional:BaseUrl"]!,
            [AssociationType.CoopCrecer] = config["Associations:CoopCrecer:BaseUrl"]!
        };
    }

    public async Task<MembershipInfo?> VerifyMembershipAsync(
        AssociationType association, 
        string cedula)
    {
        var cacheKey = $"membership_{association}_{cedula}";
        
        if (_cache.TryGetValue(cacheKey, out MembershipInfo? cached))
            return cached;

        try
        {
            var client = GetClient(association);
            var response = await client.GetAsync($"socios/verificar/{cedula}");

            if (!response.IsSuccessStatusCode)
                return null;

            var result = await response.Content
                .ReadFromJsonAsync<MembershipResponse>();

            var membership = new MembershipInfo(
                IsMember: result!.EsSocio,
                MemberSince: result.SocioDesde,
                Category: ParseCategory(result.Categoria),
                CurrentContributions: result.AportesActuales,
                AvailableCredit: result.CreditoDisponible,
                HasActiveLoan: result.PrestamoActivo
            );

            _cache.Set(cacheKey, membership, TimeSpan.FromHours(24));
            return membership;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, 
                "Error verificando membresía en {Association}", association);
            return null;
        }
    }

    public async Task<AssociationLoanSimulation?> SimulateAsync(
        AssociationType association,
        AssociationLoanRequest request)
    {
        try
        {
            var client = GetClient(association);

            var apiRequest = new
            {
                cedula = request.Cedula,
                montoVehiculo = request.VehicleValue,
                inicial = request.DownPayment,
                plazoMeses = request.TermMonths,
                tipoVehiculo = request.VehicleCondition == VehicleCondition.New 
                    ? "NUEVO" : "USADO"
            };

            var response = await client.PostAsJsonAsync(
                "vehiculos/simular", apiRequest);

            if (!response.IsSuccessStatusCode)
                return null;

            var result = await response.Content
                .ReadFromJsonAsync<AssociationSimulationResponse>();

            return new AssociationLoanSimulation(
                SimulationId: result!.SimulacionId,
                AssociationName: association.ToString(),
                IsApproved: result.Aprobado,
                AmountToFinance: result.MontoFinanciar,
                AnnualRate: result.TasaAnual,
                TermMonths: request.TermMonths,
                MonthlyPayment: result.CuotaMensual,
                TotalPayment: result.TotalPagar,
                TotalInterest: result.TotalInteres,
                LifeInsurance: result.SeguroVida,
                PaymentWithInsurance: result.CuotaConSeguro,
                HasMemberBenefit: result.BeneficioSocio,
                HasPreferentialRate: result.TasaPreferencial,
                SavingsVsNormalRate: result.AhorroVsTasaNormal,
                Requirements: result.Requisitos,
                ValidUntil: DateTime.UtcNow.AddDays(7)
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error simulando en {Association}", association);
            return null;
        }
    }

    public async Task<AssociationComparison> CompareOptionsAsync(
        AssociationLoanRequest request)
    {
        // Consultar todas las asociaciones en paralelo
        var tasks = Enum.GetValues<AssociationType>()
            .Select(async assoc =>
            {
                try
                {
                    return await SimulateAsync(assoc, request);
                }
                catch
                {
                    return null;
                }
            });

        var results = await Task.WhenAll(tasks);
        
        var options = results
            .Where(r => r != null && r.IsApproved)
            .Cast<AssociationLoanSimulation>()
            .OrderBy(r => r.MonthlyPayment)
            .ToList();

        return new AssociationComparison(
            Cedula: MaskCedula(request.Cedula),
            VehicleValue: request.VehicleValue,
            Options: options,
            BestOption: options.FirstOrDefault(),
            GeneratedAt: DateTime.UtcNow
        );
    }

    public async Task<LoanApplication> CreateApplicationAsync(
        AssociationType association,
        string simulationId,
        VehicleInfo vehicle,
        Dictionary<string, byte[]> documents)
    {
        var client = GetClient(association);

        var docBase64 = documents.ToDictionary(
            kvp => kvp.Key,
            kvp => Convert.ToBase64String(kvp.Value)
        );

        var request = new
        {
            simulacionId = simulationId,
            dealerId = _config[$"Associations:{association}:DealerId"],
            vehiculo = new
            {
                marca = vehicle.Make,
                modelo = vehicle.Model,
                año = vehicle.Year,
                chasis = vehicle.Vin
            },
            documentos = docBase64
        };

        var response = await client.PostAsJsonAsync(
            "vehiculos/solicitar", request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<ApplicationResponse>();

        return new LoanApplication(
            ApplicationId: result!.SolicitudId,
            Status: ParseStatus(result.Estado),
            EstimatedResponseDate: DateTime.Parse(result.FechaEstimadaRespuesta),
            MissingDocuments: result.DocumentosFaltantes,
            NextSteps: result.SiguientesPasos
        );
    }

    private HttpClient GetClient(AssociationType association)
    {
        var client = _httpClientFactory.CreateClient(association.ToString());
        client.BaseAddress = new Uri(_baseUrls[association]);
        // Token management would be per-association
        return client;
    }

    private static MemberCategory ParseCategory(string? category)
    {
        return category?.ToUpperInvariant() switch
        {
            "PLATINUM" => MemberCategory.Platinum,
            "GOLD" => MemberCategory.Gold,
            "SILVER" => MemberCategory.Silver,
            _ => MemberCategory.Regular
        };
    }

    private static string MaskCedula(string cedula)
        => cedula.Length >= 11 ? $"{cedula[..3]}****{cedula[^4..]}" : "***";

    private static ApplicationStatus ParseStatus(string status)
    {
        return status switch
        {
            "EN_REVISION" => ApplicationStatus.UnderAnalysis,
            "APROBADO" => ApplicationStatus.Approved,
            "RECHAZADO" => ApplicationStatus.Rejected,
            _ => ApplicationStatus.PendingReview
        };
    }
}

// DTOs internos
internal record MembershipResponse(
    bool EsSocio,
    DateTime? SocioDesde,
    string? Categoria,
    decimal AportesActuales,
    decimal CreditoDisponible,
    bool PrestamoActivo
);

internal record AssociationSimulationResponse(
    string SimulacionId,
    bool Aprobado,
    decimal MontoFinanciar,
    decimal TasaAnual,
    decimal CuotaMensual,
    decimal TotalPagar,
    decimal TotalInteres,
    decimal SeguroVida,
    decimal CuotaConSeguro,
    bool BeneficioSocio,
    bool TasaPreferencial,
    decimal? AhorroVsTasaNormal,
    List<string> Requisitos
);

internal record ApplicationResponse(
    string SolicitudId,
    string Estado,
    string FechaEstimadaRespuesta,
    List<string> DocumentosFaltantes,
    List<string> SiguientesPasos
);
```

---

## ⚛️ React Component

```tsx
// components/AssociationFinancing.tsx
import { useState } from 'react';
import { useQuery, useMutation } from '@tanstack/react-query';
import { financingService } from '@/services/financingService';
import { Users, Building2, TrendingDown, Star, CheckCircle } from 'lucide-react';

interface Props {
  vehiclePrice: number;
  isNew: boolean;
  cedula?: string;
  onOptionSelected?: (option: AssociationLoanSimulation) => void;
}

export function AssociationFinancing({
  vehiclePrice,
  isNew,
  cedula,
  onOptionSelected,
}: Props) {
  const [downPayment, setDownPayment] = useState(vehiclePrice * 0.2);
  const [termMonths, setTermMonths] = useState(48);
  const [inputCedula, setInputCedula] = useState(cedula || '');

  const compareQuery = useQuery({
    queryKey: ['association-compare', inputCedula, vehiclePrice, downPayment, termMonths],
    queryFn: () => financingService.compareAssociations({
      cedula: inputCedula,
      vehicleValue: vehiclePrice,
      downPayment,
      termMonths,
      vehicleCondition: isNew ? 'NEW' : 'USED',
    }),
    enabled: inputCedula.length === 11,
  });

  return (
    <div className="bg-white rounded-xl shadow-lg overflow-hidden">
      {/* Header */}
      <div className="bg-gradient-to-r from-green-600 to-green-700 text-white p-4">
        <div className="flex items-center gap-3">
          <Users className="w-8 h-8" />
          <div>
            <h3 className="font-bold text-lg">Asociaciones de Ahorros</h3>
            <p className="text-green-100 text-sm">
              Tasas preferenciales para socios
            </p>
          </div>
        </div>
      </div>

      <div className="p-6 space-y-6">
        {/* Formulario básico */}
        <div className="grid grid-cols-2 gap-4">
          <div className="col-span-2">
            <label className="block text-sm text-gray-600 mb-1">
              Cédula (para verificar membresía)
            </label>
            <input
              type="text"
              value={inputCedula}
              onChange={(e) => setInputCedula(e.target.value.replace(/\D/g, ''))}
              placeholder="00000000000"
              maxLength={11}
              className="w-full px-4 py-2 border rounded-lg"
            />
          </div>
          <div>
            <label className="block text-sm text-gray-600 mb-1">Inicial</label>
            <input
              type="number"
              value={downPayment}
              onChange={(e) => setDownPayment(Number(e.target.value))}
              className="w-full px-4 py-2 border rounded-lg"
            />
          </div>
          <div>
            <label className="block text-sm text-gray-600 mb-1">Plazo</label>
            <select
              value={termMonths}
              onChange={(e) => setTermMonths(Number(e.target.value))}
              className="w-full px-4 py-2 border rounded-lg"
            >
              <option value={24}>24 meses</option>
              <option value={36}>36 meses</option>
              <option value={48}>48 meses</option>
              <option value={60}>60 meses</option>
            </select>
          </div>
        </div>

        {/* Loading */}
        {compareQuery.isLoading && (
          <div className="text-center py-8">
            <div className="animate-spin w-8 h-8 border-4 border-green-600 
                            border-t-transparent rounded-full mx-auto" />
            <p className="text-sm text-gray-500 mt-2">
              Consultando asociaciones...
            </p>
          </div>
        )}

        {/* Resultados */}
        {compareQuery.data && (
          <div className="space-y-4">
            <h4 className="font-medium flex items-center gap-2">
              <CheckCircle className="w-5 h-5 text-green-600" />
              {compareQuery.data.options.length} opciones encontradas
            </h4>

            {compareQuery.data.options.map((option, index) => (
              <AssociationOption
                key={option.simulationId}
                option={option}
                isBest={index === 0}
                onSelect={() => onOptionSelected?.(option)}
              />
            ))}

            {compareQuery.data.options.length === 0 && (
              <div className="text-center py-8 text-gray-500">
                <Building2 className="w-12 h-12 mx-auto mb-2 opacity-50" />
                <p>No hay opciones disponibles con los criterios actuales.</p>
                <p className="text-sm">
                  Intenta ajustar la inicial o el plazo.
                </p>
              </div>
            )}
          </div>
        )}

        {/* Beneficios de asociaciones */}
        <div className="bg-green-50 rounded-lg p-4 space-y-2">
          <h5 className="font-medium text-green-800">
            Beneficios de las Asociaciones
          </h5>
          <ul className="text-sm text-green-700 space-y-1">
            <li className="flex items-center gap-2">
              <TrendingDown className="w-4 h-4" />
              Tasas más bajas para socios
            </li>
            <li className="flex items-center gap-2">
              <Star className="w-4 h-4" />
              Seguro de vida incluido
            </li>
            <li className="flex items-center gap-2">
              <Users className="w-4 h-4" />
              Acumulas aportes mientras pagas
            </li>
          </ul>
        </div>
      </div>
    </div>
  );
}

function AssociationOption({
  option,
  isBest,
  onSelect,
}: {
  option: AssociationLoanSimulation;
  isBest: boolean;
  onSelect: () => void;
}) {
  return (
    <div 
      className={`border rounded-lg p-4 ${
        isBest 
          ? 'border-green-500 bg-green-50' 
          : 'border-gray-200 hover:border-green-300'
      }`}
    >
      <div className="flex justify-between items-start mb-3">
        <div>
          <div className="flex items-center gap-2">
            <span className="font-bold">{option.associationName}</span>
            {isBest && (
              <span className="text-xs bg-green-600 text-white px-2 py-0.5 
                               rounded-full">
                Mejor Opción
              </span>
            )}
          </div>
          {option.hasMemberBenefit && (
            <span className="text-xs text-green-600">
              ✓ Tasa preferencial de socio
            </span>
          )}
        </div>
        <div className="text-right">
          <p className="text-2xl font-bold">
            RD$ {option.monthlyPayment.toLocaleString()}
          </p>
          <p className="text-sm text-gray-500">por mes</p>
        </div>
      </div>

      <div className="grid grid-cols-3 gap-4 text-sm mb-4">
        <div>
          <span className="text-gray-500">Tasa</span>
          <p className="font-medium">{option.annualRate}%</p>
        </div>
        <div>
          <span className="text-gray-500">Plazo</span>
          <p className="font-medium">{option.termMonths} meses</p>
        </div>
        <div>
          <span className="text-gray-500">Total a Pagar</span>
          <p className="font-medium">
            RD$ {option.totalPayment.toLocaleString()}
          </p>
        </div>
      </div>

      {option.savingsVsNormalRate && option.savingsVsNormalRate > 0 && (
        <div className="bg-yellow-50 rounded p-2 mb-3 text-sm">
          <span className="text-yellow-700">
            💰 Ahorras RD$ {option.savingsVsNormalRate.toLocaleString()} 
            vs tasa normal
          </span>
        </div>
      )}

      <button
        onClick={onSelect}
        className={`w-full py-2 rounded-lg font-medium ${
          isBest
            ? 'bg-green-600 text-white hover:bg-green-700'
            : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
        }`}
      >
        Seleccionar
      </button>
    </div>
  );
}
```

---

## 🔌 API Controller

```csharp
[ApiController]
[Route("api/associations")]
public class AssociationsController : ControllerBase
{
    private readonly IAssociationFinancingService _service;

    public AssociationsController(IAssociationFinancingService service)
    {
        _service = service;
    }

    [HttpGet("membership/{association}/{cedula}")]
    public async Task<ActionResult<MembershipInfo>> VerifyMembership(
        AssociationType association, string cedula)
    {
        var result = await _service.VerifyMembershipAsync(association, cedula);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    [HttpPost("simulate/{association}")]
    public async Task<ActionResult<AssociationLoanSimulation>> Simulate(
        AssociationType association,
        [FromBody] AssociationLoanRequest request)
    {
        var result = await _service.SimulateAsync(association, request);
        if (result == null)
            return BadRequest(new { message = "No se pudo simular" });
        return Ok(result);
    }

    [HttpPost("compare")]
    public async Task<ActionResult<AssociationComparison>> Compare(
        [FromBody] AssociationLoanRequest request)
    {
        var result = await _service.CompareOptionsAsync(request);
        return Ok(result);
    }

    [HttpPost("apply/{association}")]
    [Authorize]
    public async Task<ActionResult<LoanApplication>> Apply(
        AssociationType association,
        [FromBody] AssociationApplicationRequest request)
    {
        var result = await _service.CreateApplicationAsync(
            association,
            request.SimulationId,
            request.Vehicle,
            new Dictionary<string, byte[]>()
        );
        return Ok(result);
    }
}

public record AssociationApplicationRequest(
    string SimulationId,
    VehicleInfo Vehicle
);
```

---

## ⚙️ Configuración

```json
{
  "Associations": {
    "APAP": {
      "BaseUrl": "https://api.apap.com.do/v1/prestamos/",
      "ClientId": "okla-marketplace",
      "ClientSecret": "xxxxx",
      "DealerId": "DEALER-APAP-001"
    },
    "ALNAP": {
      "BaseUrl": "https://api.alnap.com.do/v1/",
      "ClientId": "okla-marketplace",
      "ClientSecret": "xxxxx",
      "DealerId": "DEALER-ALNAP-001"
    },
    "LaNacional": {
      "BaseUrl": "https://api.lanacional.com.do/v1/",
      "ClientId": "okla-marketplace",
      "ClientSecret": "xxxxx",
      "DealerId": "DEALER-LN-001"
    }
  }
}
```

---

## 📞 Contactos

| Asociación | Teléfono | Email Convenios |
|------------|----------|-----------------|
| APAP | 809-549-2272 | convenios@apap.com.do |
| ALNAP | 809-541-7373 | empresas@alnap.com.do |
| La Nacional | 809-472-7272 | comercial@lanacional.com.do |

---

## 💡 Ventaja Competitiva

1. **Mayor aprobación** - Asociaciones aprueban perfiles que bancos rechazan
2. **Tasas competitivas** - Especialmente para socios
3. **Beneficio dual** - Cliente paga préstamo + acumula aportes
4. **Proceso más humano** - Menos burocrático que bancos

---

**Anterior:** [BANCO_POPULAR_AUTO_API.md](./BANCO_POPULAR_AUTO_API.md)  
**Siguiente:** [SEGUROS_RESERVAS_API.md](./SEGUROS_RESERVAS_API.md)
