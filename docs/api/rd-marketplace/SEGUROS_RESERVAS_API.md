# 🛡️ Seguros Reservas - Seguro Vehicular

**Entidad:** Seguros Reservas (Grupo Popular)  
**Website:** [segurosreservas.com](https://www.segurosreservas.com)  
**Producto:** Seguro de Vehículos (Ley 146-02)  
**Prioridad:** ⭐⭐⭐⭐⭐ CRÍTICA (Obligatorio para financiamiento)

---

## 📋 Información General

| Campo | Valor |
|-------|-------|
| **Website** | [segurosreservas.com](https://www.segurosreservas.com/vehiculos) |
| **API** | REST con convenio de intermediario |
| **Regulador** | Superintendencia de Seguros |
| **Comisión OKLA** | 15-20% de la prima |

---

## 📊 Coberturas Disponibles

| Cobertura | Descripción | Obligatoria |
|-----------|-------------|-------------|
| **Responsabilidad Civil** | Daños a terceros | ✅ Ley 146-02 |
| **Colisión** | Daños al vehículo propio | ❌ |
| **Robo Total** | Pérdida por robo | ❌ |
| **Incendio** | Daños por fuego | ❌ |
| **Todo Riesgo** | Cobertura completa | ❌ |
| **Asistencia Vial** | Grúa, cerrajería, etc. | ❌ |

---

## 🌐 API Endpoints

```http
# Base URL
https://api.segurosreservas.com/v1/vehiculos/

# Autenticación
POST /oauth/token
{
  "grant_type": "client_credentials",
  "client_id": "BROKER-OKLA",
  "client_secret": "xxxxx"
}

# Cotizar Seguro
POST /cotizar
Authorization: Bearer {token}
{
  "vehiculo": {
    "marca": "Toyota",
    "modelo": "Corolla",
    "año": 2024,
    "tipo": "SEDAN",
    "uso": "PARTICULAR",
    "valorAsegurado": 1500000,
    "placa": "A123456"  // Opcional para cotización
  },
  "conductor": {
    "cedula": "00100000001",
    "fechaNacimiento": "1990-05-15",
    "genero": "M",
    "añosExperiencia": 5
  },
  "coberturas": ["RESPONSABILIDAD_CIVIL", "COLISION", "ROBO_TOTAL"],
  "deducible": "ESTANDAR"  // MINIMO | ESTANDAR | ALTO
}

# Response
{
  "cotizacionId": "COT-SR-2026-ABC123",
  "primaAnual": 45000,
  "primaMensual": 4125,
  "coberturas": [
    {
      "codigo": "RESPONSABILIDAD_CIVIL",
      "nombre": "Responsabilidad Civil",
      "limite": 1000000,
      "primaParcial": 8500,
      "obligatoria": true
    },
    {
      "codigo": "COLISION",
      "nombre": "Daños por Colisión",
      "limite": 1500000,
      "deducible": 15000,
      "primaParcial": 22000,
      "obligatoria": false
    },
    {
      "codigo": "ROBO_TOTAL",
      "nombre": "Robo Total",
      "limite": 1500000,
      "deducible": 50000,
      "primaParcial": 12000,
      "obligatoria": false
    }
  ],
  "descuentos": [
    {
      "tipo": "BUEN_CONDUCTOR",
      "porcentaje": 10,
      "monto": 4500
    },
    {
      "tipo": "PAGO_ANUAL",
      "porcentaje": 5,
      "monto": 2025
    }
  ],
  "primaFinal": 38475,
  "impuestos": 6917,
  "totalPagar": 45392,
  "validaHasta": "2026-01-22T23:59:59Z",
  "beneficiosAdicionales": [
    "Asistencia vial 24/7",
    "Auto sustituto por 5 días",
    "Defensa legal incluida"
  ]
}

# Emitir Póliza
POST /emitir
{
  "cotizacionId": "COT-SR-2026-ABC123",
  "formaPago": "MENSUAL",      // ANUAL | SEMESTRAL | MENSUAL
  "metodoPago": "TARJETA",     // TARJETA | TRANSFERENCIA | EFECTIVO
  "tarjeta": {
    "numero": "4000000000000001",
    "expiracion": "12/28",
    "cvv": "123",
    "nombre": "JUAN PEREZ"
  },
  "documentos": {
    "cedulaFrente": "base64...",
    "cedulaReverso": "base64...",
    "matriculaVehiculo": "base64..."
  }
}

# Response
{
  "polizaId": "POL-SR-2026-123456",
  "numeroPoliza": "AUT-2026-123456",
  "estado": "ACTIVA",
  "vigenciaDesde": "2026-01-15",
  "vigenciaHasta": "2027-01-15",
  "documentoPdf": "https://...",
  "tarjetaCirculacion": "https://...",
  "proximoPago": "2026-02-15",
  "montoPago": 3782
}

# Consultar Póliza
GET /polizas/{numeroPoliza}

# Renovar Póliza
POST /polizas/{numeroPoliza}/renovar

# Reportar Siniestro
POST /siniestros
{
  "polizaId": "POL-SR-2026-123456",
  "fechaSiniestro": "2026-03-20T14:30:00",
  "tipoSiniestro": "COLISION",
  "descripcion": "Colisión trasera en semáforo",
  "ubicacion": {
    "direccion": "Av. Winston Churchill esq. Gustavo Mejía Ricart",
    "latitud": 18.4720,
    "longitud": -69.9473
  },
  "fotos": ["base64...", "base64..."],
  "terceroInvolucrado": true
}
```

---

## 💻 Modelos C#

```csharp
namespace InsuranceService.Domain.Entities;

/// <summary>
/// Cotización de seguro vehicular
/// </summary>
public record InsuranceQuote(
    string QuoteId,
    decimal AnnualPremium,
    decimal MonthlyPremium,
    List<CoverageDetail> Coverages,
    List<Discount> Discounts,
    decimal FinalPremium,
    decimal Taxes,
    decimal TotalAmount,
    DateTime ValidUntil,
    List<string> AdditionalBenefits
);

public record CoverageDetail(
    string Code,
    string Name,
    decimal Limit,
    decimal? Deductible,
    decimal PartialPremium,
    bool IsRequired
);

public record Discount(
    string Type,
    decimal Percentage,
    decimal Amount
);

/// <summary>
/// Póliza emitida
/// </summary>
public record InsurancePolicy(
    string PolicyId,
    string PolicyNumber,
    PolicyStatus Status,
    DateTime EffectiveFrom,
    DateTime EffectiveTo,
    decimal TotalPremium,
    PaymentFrequency PaymentFrequency,
    decimal PaymentAmount,
    DateTime? NextPaymentDate,
    string PolicyDocumentUrl,
    string CirculationCardUrl,
    VehicleInsuranceInfo Vehicle,
    DriverInfo Driver
);

public enum PolicyStatus
{
    Active,
    Pending,
    Cancelled,
    Expired,
    Suspended
}

public enum PaymentFrequency
{
    Annual,
    SemiAnnual,
    Monthly
}

/// <summary>
/// Request para cotización
/// </summary>
public record QuoteRequest(
    VehicleInsuranceInfo Vehicle,
    DriverInfo Driver,
    List<CoverageType> Coverages,
    DeductibleLevel Deductible = DeductibleLevel.Standard
);

public record VehicleInsuranceInfo(
    string Make,
    string Model,
    int Year,
    VehicleCategory Type,
    VehicleUse Use,
    decimal InsuredValue,
    string? Plate
);

public record DriverInfo(
    string Cedula,
    DateTime DateOfBirth,
    Gender Gender,
    int YearsOfExperience
);

public enum CoverageType
{
    ResponsabilidadCivil,
    Colision,
    RoboTotal,
    Incendio,
    TodoRiesgo,
    AsistenciaVial
}

public enum DeductibleLevel
{
    Minimum,
    Standard,
    High
}

public enum VehicleCategory
{
    Sedan,
    SUV,
    Pickup,
    Motorcycle,
    Commercial
}

public enum VehicleUse
{
    Particular,
    Commercial,
    PublicTransport
}

public enum Gender
{
    Male,
    Female
}
```

---

## 🔧 Service Interface

```csharp
namespace InsuranceService.Domain.Interfaces;

public interface ISegurosReservasService
{
    /// <summary>
    /// Genera cotización de seguro
    /// </summary>
    Task<InsuranceQuote> GetQuoteAsync(QuoteRequest request);

    /// <summary>
    /// Emite póliza de seguro
    /// </summary>
    Task<InsurancePolicy> IssuePolicyAsync(
        string quoteId,
        PaymentFrequency frequency,
        PaymentMethod method,
        PaymentDetails? paymentDetails,
        Dictionary<string, byte[]> documents);

    /// <summary>
    /// Obtiene póliza por número
    /// </summary>
    Task<InsurancePolicy?> GetPolicyAsync(string policyNumber);

    /// <summary>
    /// Renueva póliza existente
    /// </summary>
    Task<InsurancePolicy> RenewPolicyAsync(string policyNumber);

    /// <summary>
    /// Reporta siniestro
    /// </summary>
    Task<ClaimReport> ReportClaimAsync(ClaimRequest claim);

    /// <summary>
    /// Calcula prima estimada rápida (sin datos completos)
    /// </summary>
    decimal EstimatePremium(
        decimal vehicleValue, 
        int vehicleYear, 
        List<CoverageType> coverages);
}
```

---

## 🏗️ Service Implementation

```csharp
namespace InsuranceService.Infrastructure.Services;

public class SegurosReservasService : ISegurosReservasService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly IMemoryCache _cache;
    private readonly ILogger<SegurosReservasService> _logger;

    private string? _accessToken;
    private DateTime _tokenExpiry = DateTime.MinValue;

    public SegurosReservasService(
        HttpClient httpClient,
        IConfiguration config,
        IMemoryCache cache,
        ILogger<SegurosReservasService> logger)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(
            config["SegurosReservas:BaseUrl"]!);
        _config = config;
        _cache = cache;
        _logger = logger;
    }

    public async Task<InsuranceQuote> GetQuoteAsync(QuoteRequest request)
    {
        await EnsureAuthenticatedAsync();

        var apiRequest = new
        {
            vehiculo = new
            {
                marca = request.Vehicle.Make,
                modelo = request.Vehicle.Model,
                año = request.Vehicle.Year,
                tipo = request.Vehicle.Type.ToString().ToUpper(),
                uso = request.Vehicle.Use.ToString().ToUpper(),
                valorAsegurado = request.Vehicle.InsuredValue,
                placa = request.Vehicle.Plate
            },
            conductor = new
            {
                cedula = request.Driver.Cedula,
                fechaNacimiento = request.Driver.DateOfBirth.ToString("yyyy-MM-dd"),
                genero = request.Driver.Gender == Gender.Male ? "M" : "F",
                añosExperiencia = request.Driver.YearsOfExperience
            },
            coberturas = request.Coverages.Select(c => c.ToString().ToUpper()),
            deducible = request.Deductible.ToString().ToUpper()
        };

        var response = await _httpClient.PostAsJsonAsync("cotizar", apiRequest);
        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<QuoteResponse>();

        return MapToQuote(result!);
    }

    public async Task<InsurancePolicy> IssuePolicyAsync(
        string quoteId,
        PaymentFrequency frequency,
        PaymentMethod method,
        PaymentDetails? paymentDetails,
        Dictionary<string, byte[]> documents)
    {
        await EnsureAuthenticatedAsync();

        var docBase64 = documents.ToDictionary(
            kvp => kvp.Key,
            kvp => Convert.ToBase64String(kvp.Value)
        );

        var request = new
        {
            cotizacionId = quoteId,
            formaPago = frequency.ToString().ToUpper(),
            metodoPago = method.ToString().ToUpper(),
            tarjeta = paymentDetails != null ? new
            {
                numero = paymentDetails.CardNumber,
                expiracion = paymentDetails.Expiration,
                cvv = paymentDetails.Cvv,
                nombre = paymentDetails.CardholderName
            } : null,
            documentos = docBase64
        };

        var response = await _httpClient.PostAsJsonAsync("emitir", request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<PolicyResponse>();

        _logger.LogInformation(
            "Policy {PolicyNumber} issued for quote {QuoteId}",
            result!.NumeroPoliza, quoteId);

        return MapToPolicy(result);
    }

    public async Task<InsurancePolicy?> GetPolicyAsync(string policyNumber)
    {
        await EnsureAuthenticatedAsync();

        var response = await _httpClient.GetAsync($"polizas/{policyNumber}");
        
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<PolicyResponse>();

        return MapToPolicy(result!);
    }

    public async Task<InsurancePolicy> RenewPolicyAsync(string policyNumber)
    {
        await EnsureAuthenticatedAsync();

        var response = await _httpClient.PostAsync(
            $"polizas/{policyNumber}/renovar", null);
        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<PolicyResponse>();

        return MapToPolicy(result!);
    }

    public async Task<ClaimReport> ReportClaimAsync(ClaimRequest claim)
    {
        await EnsureAuthenticatedAsync();

        var request = new
        {
            polizaId = claim.PolicyId,
            fechaSiniestro = claim.IncidentDate,
            tipoSiniestro = claim.ClaimType.ToString().ToUpper(),
            descripcion = claim.Description,
            ubicacion = new
            {
                direccion = claim.Location.Address,
                latitud = claim.Location.Latitude,
                longitud = claim.Location.Longitude
            },
            fotos = claim.Photos?.Select(Convert.ToBase64String),
            terceroInvolucrado = claim.ThirdPartyInvolved
        };

        var response = await _httpClient.PostAsJsonAsync("siniestros", request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<ClaimResponse>();

        return new ClaimReport(
            ClaimId: result!.SiniestroId,
            ClaimNumber: result.NumeroSiniestro,
            Status: ClaimStatus.Reported,
            ReportedAt: DateTime.UtcNow,
            NextSteps: result.SiguientesPasos
        );
    }

    public decimal EstimatePremium(
        decimal vehicleValue,
        int vehicleYear,
        List<CoverageType> coverages)
    {
        // Estimación rápida basada en factores conocidos
        var basePremium = vehicleValue * 0.03m; // 3% base

        // Ajuste por antigüedad
        var age = DateTime.Now.Year - vehicleYear;
        var ageFactor = age switch
        {
            0 => 1.0m,
            1 => 0.95m,
            2 => 0.90m,
            <= 5 => 0.85m,
            <= 10 => 0.80m,
            _ => 0.75m
        };

        // Ajuste por coberturas
        var coverageFactor = 1.0m;
        if (coverages.Contains(CoverageType.Colision)) coverageFactor += 0.3m;
        if (coverages.Contains(CoverageType.RoboTotal)) coverageFactor += 0.2m;
        if (coverages.Contains(CoverageType.TodoRiesgo)) coverageFactor += 0.5m;

        return Math.Round(basePremium * ageFactor * coverageFactor, 2);
    }

    private async Task EnsureAuthenticatedAsync()
    {
        if (_accessToken != null && DateTime.UtcNow < _tokenExpiry)
            return;

        var tokenRequest = new
        {
            grant_type = "client_credentials",
            client_id = _config["SegurosReservas:ClientId"],
            client_secret = _config["SegurosReservas:ClientSecret"]
        };

        var response = await _httpClient.PostAsJsonAsync("oauth/token", tokenRequest);
        response.EnsureSuccessStatusCode();

        var tokenResponse = await response.Content
            .ReadFromJsonAsync<TokenResponse>();

        _accessToken = tokenResponse!.AccessToken;
        _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 60);

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _accessToken);
    }

    private static InsuranceQuote MapToQuote(QuoteResponse response)
    {
        return new InsuranceQuote(
            QuoteId: response.CotizacionId,
            AnnualPremium: response.PrimaAnual,
            MonthlyPremium: response.PrimaMensual,
            Coverages: response.Coberturas.Select(c => new CoverageDetail(
                Code: c.Codigo,
                Name: c.Nombre,
                Limit: c.Limite,
                Deductible: c.Deducible,
                PartialPremium: c.PrimaParcial,
                IsRequired: c.Obligatoria
            )).ToList(),
            Discounts: response.Descuentos.Select(d => new Discount(
                Type: d.Tipo,
                Percentage: d.Porcentaje,
                Amount: d.Monto
            )).ToList(),
            FinalPremium: response.PrimaFinal,
            Taxes: response.Impuestos,
            TotalAmount: response.TotalPagar,
            ValidUntil: response.ValidaHasta,
            AdditionalBenefits: response.BeneficiosAdicionales
        );
    }

    private static InsurancePolicy MapToPolicy(PolicyResponse response)
    {
        return new InsurancePolicy(
            PolicyId: response.PolizaId,
            PolicyNumber: response.NumeroPoliza,
            Status: Enum.Parse<PolicyStatus>(response.Estado, ignoreCase: true),
            EffectiveFrom: DateTime.Parse(response.VigenciaDesde),
            EffectiveTo: DateTime.Parse(response.VigenciaHasta),
            TotalPremium: response.PrimaTotal ?? 0,
            PaymentFrequency: PaymentFrequency.Monthly,
            PaymentAmount: response.MontoPago ?? 0,
            NextPaymentDate: string.IsNullOrEmpty(response.ProximoPago) 
                ? null : DateTime.Parse(response.ProximoPago),
            PolicyDocumentUrl: response.DocumentoPdf ?? "",
            CirculationCardUrl: response.TarjetaCirculacion ?? "",
            Vehicle: null!,
            Driver: null!
        );
    }
}

// DTOs internos
internal record QuoteResponse(
    string CotizacionId,
    decimal PrimaAnual,
    decimal PrimaMensual,
    List<CoverageResponse> Coberturas,
    List<DiscountResponse> Descuentos,
    decimal PrimaFinal,
    decimal Impuestos,
    decimal TotalPagar,
    DateTime ValidaHasta,
    List<string> BeneficiosAdicionales
);

internal record CoverageResponse(
    string Codigo,
    string Nombre,
    decimal Limite,
    decimal? Deducible,
    decimal PrimaParcial,
    bool Obligatoria
);

internal record DiscountResponse(
    string Tipo,
    decimal Porcentaje,
    decimal Monto
);

internal record PolicyResponse(
    string PolizaId,
    string NumeroPoliza,
    string Estado,
    string VigenciaDesde,
    string VigenciaHasta,
    decimal? PrimaTotal,
    string? DocumentoPdf,
    string? TarjetaCirculacion,
    string? ProximoPago,
    decimal? MontoPago
);

internal record ClaimResponse(
    string SiniestroId,
    string NumeroSiniestro,
    List<string> SiguientesPasos
);
```

---

## ⚛️ React Component

```tsx
// components/InsuranceQuote.tsx
import { useState } from 'react';
import { useMutation } from '@tanstack/react-query';
import { insuranceService } from '@/services/insuranceService';
import { Shield, Check, AlertCircle, Car } from 'lucide-react';

interface Props {
  vehicleMake: string;
  vehicleModel: string;
  vehicleYear: number;
  vehicleValue: number;
  vehicleType: 'SEDAN' | 'SUV' | 'PICKUP';
  onQuoteReceived?: (quote: InsuranceQuote) => void;
}

const COVERAGES = [
  { 
    id: 'RESPONSABILIDAD_CIVIL', 
    name: 'Responsabilidad Civil', 
    required: true,
    description: 'Obligatoria por Ley 146-02'
  },
  { 
    id: 'COLISION', 
    name: 'Daños por Colisión',
    description: 'Cubre daños a tu vehículo en accidentes'
  },
  { 
    id: 'ROBO_TOTAL', 
    name: 'Robo Total',
    description: 'Protección contra robo del vehículo'
  },
  { 
    id: 'INCENDIO', 
    name: 'Incendio',
    description: 'Daños por fuego'
  },
  { 
    id: 'TODO_RIESGO', 
    name: 'Todo Riesgo',
    description: 'Cobertura completa (incluye todas las anteriores)'
  },
];

export function InsuranceQuote({
  vehicleMake,
  vehicleModel,
  vehicleYear,
  vehicleValue,
  vehicleType,
  onQuoteReceived,
}: Props) {
  const [selectedCoverages, setSelectedCoverages] = useState<string[]>([
    'RESPONSABILIDAD_CIVIL',
    'COLISION',
  ]);
  const [driverInfo, setDriverInfo] = useState({
    cedula: '',
    birthDate: '',
    gender: 'M',
    experience: 5,
  });
  const [deductible, setDeductible] = useState<'MINIMO' | 'ESTANDAR' | 'ALTO'>('ESTANDAR');

  const quoteMutation = useMutation({
    mutationFn: () => insuranceService.getQuote({
      vehicle: {
        make: vehicleMake,
        model: vehicleModel,
        year: vehicleYear,
        type: vehicleType,
        use: 'PARTICULAR',
        insuredValue: vehicleValue,
      },
      driver: {
        cedula: driverInfo.cedula,
        dateOfBirth: driverInfo.birthDate,
        gender: driverInfo.gender,
        yearsOfExperience: driverInfo.experience,
      },
      coverages: selectedCoverages,
      deductible,
    }),
    onSuccess: (quote) => {
      onQuoteReceived?.(quote);
    },
  });

  const toggleCoverage = (id: string) => {
    if (id === 'RESPONSABILIDAD_CIVIL') return; // No se puede quitar
    if (id === 'TODO_RIESGO') {
      // Todo riesgo reemplaza las demás
      setSelectedCoverages(['RESPONSABILIDAD_CIVIL', 'TODO_RIESGO']);
      return;
    }
    setSelectedCoverages((prev) =>
      prev.includes(id)
        ? prev.filter((c) => c !== id && c !== 'TODO_RIESGO')
        : [...prev.filter((c) => c !== 'TODO_RIESGO'), id]
    );
  };

  return (
    <div className="bg-white rounded-xl shadow-lg overflow-hidden">
      {/* Header */}
      <div className="bg-gradient-to-r from-blue-700 to-blue-800 text-white p-4">
        <div className="flex items-center gap-3">
          <Shield className="w-8 h-8" />
          <div>
            <h3 className="font-bold text-lg">Seguros Reservas</h3>
            <p className="text-blue-200 text-sm">Cotiza tu seguro vehicular</p>
          </div>
        </div>
      </div>

      <div className="p-6 space-y-6">
        {/* Vehículo */}
        <div className="bg-gray-50 rounded-lg p-4 flex items-center gap-4">
          <Car className="w-10 h-10 text-gray-400" />
          <div>
            <p className="font-medium">{vehicleMake} {vehicleModel} {vehicleYear}</p>
            <p className="text-sm text-gray-500">
              Valor asegurado: RD$ {vehicleValue.toLocaleString()}
            </p>
          </div>
        </div>

        {/* Coberturas */}
        <div>
          <h4 className="font-medium mb-3">Selecciona tus coberturas</h4>
          <div className="space-y-2">
            {COVERAGES.map((coverage) => (
              <label
                key={coverage.id}
                className={`flex items-center justify-between p-3 rounded-lg border 
                  cursor-pointer transition-colors ${
                    selectedCoverages.includes(coverage.id)
                      ? 'border-blue-500 bg-blue-50'
                      : 'border-gray-200 hover:border-blue-200'
                  } ${coverage.required ? 'cursor-not-allowed' : ''}`}
              >
                <div className="flex items-center gap-3">
                  <input
                    type="checkbox"
                    checked={selectedCoverages.includes(coverage.id)}
                    onChange={() => toggleCoverage(coverage.id)}
                    disabled={coverage.required}
                    className="w-5 h-5 text-blue-600"
                  />
                  <div>
                    <span className="font-medium">{coverage.name}</span>
                    {coverage.required && (
                      <span className="ml-2 text-xs bg-red-100 text-red-700 
                                       px-2 py-0.5 rounded">
                        Obligatorio
                      </span>
                    )}
                    <p className="text-sm text-gray-500">{coverage.description}</p>
                  </div>
                </div>
              </label>
            ))}
          </div>
        </div>

        {/* Deducible */}
        <div>
          <h4 className="font-medium mb-3">Nivel de deducible</h4>
          <div className="grid grid-cols-3 gap-2">
            {[
              { value: 'MINIMO', label: 'Mínimo', desc: 'Menor deducible, mayor prima' },
              { value: 'ESTANDAR', label: 'Estándar', desc: 'Balance recomendado' },
              { value: 'ALTO', label: 'Alto', desc: 'Mayor deducible, menor prima' },
            ].map((opt) => (
              <button
                key={opt.value}
                onClick={() => setDeductible(opt.value as any)}
                className={`p-3 rounded-lg border text-left ${
                  deductible === opt.value
                    ? 'border-blue-500 bg-blue-50'
                    : 'border-gray-200'
                }`}
              >
                <span className="font-medium">{opt.label}</span>
                <p className="text-xs text-gray-500">{opt.desc}</p>
              </button>
            ))}
          </div>
        </div>

        {/* Datos del conductor */}
        <div>
          <h4 className="font-medium mb-3">Datos del conductor</h4>
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm text-gray-600 mb-1">Cédula</label>
              <input
                type="text"
                value={driverInfo.cedula}
                onChange={(e) => setDriverInfo({
                  ...driverInfo,
                  cedula: e.target.value.replace(/\D/g, ''),
                })}
                maxLength={11}
                placeholder="00000000000"
                className="w-full px-3 py-2 border rounded-lg"
              />
            </div>
            <div>
              <label className="block text-sm text-gray-600 mb-1">
                Fecha Nacimiento
              </label>
              <input
                type="date"
                value={driverInfo.birthDate}
                onChange={(e) => setDriverInfo({
                  ...driverInfo,
                  birthDate: e.target.value,
                })}
                className="w-full px-3 py-2 border rounded-lg"
              />
            </div>
            <div>
              <label className="block text-sm text-gray-600 mb-1">Género</label>
              <select
                value={driverInfo.gender}
                onChange={(e) => setDriverInfo({
                  ...driverInfo,
                  gender: e.target.value,
                })}
                className="w-full px-3 py-2 border rounded-lg"
              >
                <option value="M">Masculino</option>
                <option value="F">Femenino</option>
              </select>
            </div>
            <div>
              <label className="block text-sm text-gray-600 mb-1">
                Años Experiencia
              </label>
              <input
                type="number"
                value={driverInfo.experience}
                onChange={(e) => setDriverInfo({
                  ...driverInfo,
                  experience: Number(e.target.value),
                })}
                min={0}
                max={50}
                className="w-full px-3 py-2 border rounded-lg"
              />
            </div>
          </div>
        </div>

        <button
          onClick={() => quoteMutation.mutate()}
          disabled={quoteMutation.isPending || !driverInfo.cedula || !driverInfo.birthDate}
          className="w-full py-3 bg-blue-700 text-white rounded-lg font-medium
                     hover:bg-blue-800 disabled:opacity-50 flex items-center 
                     justify-center gap-2"
        >
          {quoteMutation.isPending ? (
            <>
              <div className="animate-spin w-5 h-5 border-2 border-white 
                              border-t-transparent rounded-full" />
              Cotizando...
            </>
          ) : (
            <>
              <Shield className="w-5 h-5" />
              Obtener Cotización
            </>
          )}
        </button>

        {/* Resultado */}
        {quoteMutation.data && (
          <QuoteResult quote={quoteMutation.data} />
        )}
      </div>
    </div>
  );
}

function QuoteResult({ quote }: { quote: InsuranceQuote }) {
  return (
    <div className="border-t pt-6 mt-6 space-y-4">
      <div className="flex items-center gap-2 text-green-700">
        <Check className="w-5 h-5" />
        <span className="font-medium">Cotización generada</span>
      </div>

      <div className="bg-blue-50 rounded-lg p-4">
        <div className="flex justify-between items-center mb-4">
          <div>
            <p className="text-sm text-gray-600">Prima Anual</p>
            <p className="text-2xl font-bold">
              RD$ {quote.totalAmount.toLocaleString()}
            </p>
          </div>
          <div className="text-right">
            <p className="text-sm text-gray-600">o mensual</p>
            <p className="text-lg font-medium">
              RD$ {quote.monthlyPremium.toLocaleString()}/mes
            </p>
          </div>
        </div>

        {/* Descuentos */}
        {quote.discounts.length > 0 && (
          <div className="text-sm text-green-700 mb-2">
            {quote.discounts.map((d) => (
              <p key={d.type}>
                ✓ {d.type}: -{d.percentage}% (RD$ {d.amount.toLocaleString()})
              </p>
            ))}
          </div>
        )}

        {/* Coberturas */}
        <div className="border-t pt-3 mt-3">
          <p className="text-sm font-medium mb-2">Coberturas incluidas:</p>
          {quote.coverages.map((c) => (
            <div key={c.code} className="flex justify-between text-sm py-1">
              <span>{c.name}</span>
              <span>RD$ {c.limit.toLocaleString()}</span>
            </div>
          ))}
        </div>

        {/* Beneficios */}
        <div className="border-t pt-3 mt-3">
          <p className="text-sm font-medium mb-2">Beneficios adicionales:</p>
          <ul className="text-sm text-gray-600">
            {quote.additionalBenefits.map((b, i) => (
              <li key={i}>✓ {b}</li>
            ))}
          </ul>
        </div>
      </div>

      <p className="text-xs text-gray-500 text-center">
        Cotización válida hasta: {new Date(quote.validUntil).toLocaleDateString('es-DO')}
      </p>

      <button className="w-full py-3 bg-green-600 text-white rounded-lg font-medium
                         hover:bg-green-700">
        Contratar Ahora
      </button>
    </div>
  );
}
```

---

## 🔌 API Controller

```csharp
[ApiController]
[Route("api/insurance")]
public class InsuranceController : ControllerBase
{
    private readonly ISegurosReservasService _service;

    public InsuranceController(ISegurosReservasService service)
    {
        _service = service;
    }

    [HttpPost("quote")]
    public async Task<ActionResult<InsuranceQuote>> GetQuote(
        [FromBody] QuoteRequest request)
    {
        var result = await _service.GetQuoteAsync(request);
        return Ok(result);
    }

    [HttpPost("issue")]
    [Authorize]
    public async Task<ActionResult<InsurancePolicy>> IssuePolicy(
        [FromBody] IssuePolicyRequest request)
    {
        var result = await _service.IssuePolicyAsync(
            request.QuoteId,
            request.PaymentFrequency,
            request.PaymentMethod,
            request.PaymentDetails,
            new Dictionary<string, byte[]>()
        );
        return Ok(result);
    }

    [HttpGet("policy/{policyNumber}")]
    [Authorize]
    public async Task<ActionResult<InsurancePolicy>> GetPolicy(string policyNumber)
    {
        var result = await _service.GetPolicyAsync(policyNumber);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    [HttpPost("estimate")]
    public ActionResult EstimatePremium([FromBody] EstimateRequest request)
    {
        var estimate = _service.EstimatePremium(
            request.VehicleValue,
            request.VehicleYear,
            request.Coverages
        );
        return Ok(new { estimatedAnnualPremium = estimate });
    }

    [HttpPost("claims")]
    [Authorize]
    public async Task<ActionResult<ClaimReport>> ReportClaim(
        [FromBody] ClaimRequest request)
    {
        var result = await _service.ReportClaimAsync(request);
        return Ok(result);
    }
}
```

---

## ⚙️ Configuración

```json
{
  "SegurosReservas": {
    "BaseUrl": "https://api.segurosreservas.com/v1/vehiculos/",
    "ClientId": "BROKER-OKLA",
    "ClientSecret": "xxxxx",
    "BrokerId": "OKLA-001",
    "Commission": 0.18
  }
}
```

---

## 📞 Contacto Seguros Reservas

| Departamento | Teléfono | Email |
|--------------|----------|-------|
| Ventas Corporativas | 809-960-0000 ext. 5000 | corporativo@segurosreservas.com |
| Intermediarios | 809-960-0000 ext. 5100 | intermediarios@segurosreservas.com |
| Siniestros | 809-960-0000 ext. 3000 | siniestros@segurosreservas.com |
| API Support | 809-960-0000 ext. 4500 | apidev@segurosreservas.com |

---

**Anterior:** [ASOCIACIONES_AHORROS_API.md](./ASOCIACIONES_AHORROS_API.md)  
**Siguiente:** [ASEGURADORAS_API.md](./ASEGURADORAS_API.md)
