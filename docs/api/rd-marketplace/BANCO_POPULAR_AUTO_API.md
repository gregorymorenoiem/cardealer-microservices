# 🏦 Banco Popular Auto - Financiamiento Vehicular

**Entidad:** Banco Popular Dominicano  
**Website:** [popularenlinea.com](https://www.popularenlinea.com)  
**Producto:** Préstamo Auto Popular  
**Prioridad:** ⭐⭐⭐⭐⭐ CRÍTICA (💰 MONETIZACIÓN)

---

## 📋 Información General

| Campo | Valor |
|-------|-------|
| **Website** | [popularenlinea.com/auto](https://www.popularenlinea.com/prestamos/auto) |
| **API** | Requiere convenio comercial con Dealers |
| **Tipo** | REST API con OAuth 2.0 |
| **Comisión OKLA** | 0.5% - 1.0% del monto financiado |

---

## 📊 Características del Producto

| Característica | Valor |
|----------------|-------|
| **Monto Mínimo** | RD$ 200,000 |
| **Monto Máximo** | RD$ 10,000,000 |
| **Plazo** | 12 - 72 meses |
| **Tasa** | 9.5% - 18% según perfil |
| **Inicial** | Desde 10% |
| **Vehículos** | Nuevos y usados (hasta 10 años) |

---

## 🌐 API Endpoints

```http
# Base URL (Sandbox)
https://sandbox-api.bpdapi.com.do/v1/auto/

# Base URL (Producción)
https://api.bpdapi.com.do/v1/auto/

# Autenticación OAuth 2.0
POST /oauth/token
Content-Type: application/x-www-form-urlencoded

grant_type=client_credentials
&client_id={DEALER_CLIENT_ID}
&client_secret={DEALER_SECRET}
&scope=auto-financing

# Simular Financiamiento
POST /simulations
Authorization: Bearer {access_token}
{
  "vehicleType": "NEW",           // NEW | USED
  "vehicleValue": 1500000,        // Valor del vehículo
  "downPayment": 300000,          // Inicial
  "termMonths": 48,               // Plazo
  "customerRNC": "00100000001"    // Cédula (opcional)
}

# Response
{
  "simulationId": "SIM-2026-ABC123",
  "approved": true,
  "amountFinanced": 1200000,
  "interestRate": 11.5,
  "monthlyPayment": 31450,
  "totalPayment": 1509600,
  "totalInterest": 309600,
  "firstPaymentDate": "2026-03-01",
  "insuranceRequired": true,
  "insuranceMonthly": 2500,
  "paymentWithInsurance": 33950,
  "conditions": [
    "Seguro vehicular obligatorio",
    "Antigüedad laboral mínima 6 meses"
  ]
}

# Iniciar Solicitud
POST /applications
Authorization: Bearer {access_token}
{
  "simulationId": "SIM-2026-ABC123",
  "dealerId": "DEALER-001",
  "customer": {
    "cedula": "00100000001",
    "firstName": "Juan",
    "lastName": "Pérez",
    "email": "juan@email.com",
    "phone": "8091234567",
    "monthlyIncome": 85000
  },
  "vehicle": {
    "make": "Toyota",
    "model": "Corolla",
    "year": 2024,
    "vin": "JTDKN3DU5A0123456",
    "color": "Blanco"
  },
  "documents": {
    "cedulaFront": "base64...",
    "cedulaBack": "base64...",
    "incomeProof": "base64..."
  }
}

# Response
{
  "applicationId": "APP-2026-XYZ789",
  "status": "PENDING_REVIEW",
  "estimatedResponse": "2026-01-16T18:00:00Z",
  "requiredDocuments": [],
  "nextSteps": [
    "Revisión de documentos (24h)",
    "Aprobación final",
    "Firma de contrato"
  ]
}

# Consultar Estado
GET /applications/{applicationId}
Authorization: Bearer {access_token}

# Response
{
  "applicationId": "APP-2026-XYZ789",
  "status": "APPROVED",
  "approvedAmount": 1200000,
  "approvedRate": 11.5,
  "monthlyPayment": 31450,
  "contractUrl": "https://...",
  "disbursementDate": "2026-01-20"
}

# Webhook de Cambio de Estado
POST {dealerWebhookUrl}
{
  "event": "application.status_changed",
  "applicationId": "APP-2026-XYZ789",
  "previousStatus": "PENDING_REVIEW",
  "newStatus": "APPROVED",
  "timestamp": "2026-01-16T14:30:00Z",
  "data": {
    "approvedAmount": 1200000,
    "approvedRate": 11.5
  }
}
```

---

## 💻 Modelos C#

```csharp
namespace FinancingService.Domain.Entities;

/// <summary>
/// Solicitud de financiamiento Banco Popular
/// </summary>
public record BpAutoApplication(
    string ApplicationId,
    ApplicationStatus Status,
    decimal AmountRequested,
    decimal? ApprovedAmount,
    decimal InterestRate,
    int TermMonths,
    decimal MonthlyPayment,
    CustomerInfo Customer,
    VehicleInfo Vehicle,
    List<string> RequiredDocuments,
    List<string> NextSteps,
    DateTime CreatedAt,
    DateTime? ApprovedAt,
    DateTime? DisbursementDate
);

public enum ApplicationStatus
{
    PendingDocuments,
    PendingReview,
    UnderAnalysis,
    Approved,
    ConditionallyApproved,
    Rejected,
    Cancelled,
    Disbursed
}

public record CustomerInfo(
    string Cedula,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    decimal MonthlyIncome,
    string? EmployerName,
    int? EmploymentMonths
);

public record VehicleInfo(
    string Make,
    string Model,
    int Year,
    string? Vin,
    string? Color,
    bool IsNew,
    decimal Value
);

/// <summary>
/// Simulación de financiamiento
/// </summary>
public record FinancingSimulation(
    string SimulationId,
    bool IsApproved,
    decimal VehicleValue,
    decimal DownPayment,
    decimal AmountFinanced,
    decimal InterestRate,
    int TermMonths,
    decimal MonthlyPayment,
    decimal TotalPayment,
    decimal TotalInterest,
    bool InsuranceRequired,
    decimal? InsuranceMonthly,
    decimal PaymentWithInsurance,
    DateTime FirstPaymentDate,
    List<string> Conditions,
    DateTime ValidUntil
);

/// <summary>
/// Request para simulación
/// </summary>
public record SimulationRequest(
    VehicleType VehicleType,
    decimal VehicleValue,
    decimal DownPayment,
    int TermMonths,
    string? CustomerCedula = null
);

public enum VehicleType
{
    New,
    Used
}
```

---

## 🔧 Service Interface

```csharp
namespace FinancingService.Domain.Interfaces;

public interface IBancoPopularAutoService
{
    /// <summary>
    /// Simula financiamiento (no requiere datos personales)
    /// </summary>
    Task<FinancingSimulation> SimulateAsync(SimulationRequest request);

    /// <summary>
    /// Crea solicitud de financiamiento
    /// </summary>
    Task<BpAutoApplication> CreateApplicationAsync(
        string simulationId,
        CustomerInfo customer,
        VehicleInfo vehicle,
        Dictionary<string, byte[]> documents);

    /// <summary>
    /// Consulta estado de solicitud
    /// </summary>
    Task<BpAutoApplication?> GetApplicationAsync(string applicationId);

    /// <summary>
    /// Lista solicitudes del dealer
    /// </summary>
    Task<List<BpAutoApplication>> GetDealerApplicationsAsync(
        string dealerId,
        ApplicationStatus? status = null,
        DateTime? fromDate = null);

    /// <summary>
    /// Cancela solicitud
    /// </summary>
    Task<bool> CancelApplicationAsync(string applicationId, string reason);
}
```

---

## 🏗️ Service Implementation

```csharp
namespace FinancingService.Infrastructure.Services;

public class BancoPopularAutoService : IBancoPopularAutoService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<BancoPopularAutoService> _logger;

    private string? _accessToken;
    private DateTime _tokenExpiry = DateTime.MinValue;

    public BancoPopularAutoService(
        HttpClient httpClient,
        IConfiguration config,
        ILogger<BancoPopularAutoService> logger)
    {
        _httpClient = httpClient;
        var baseUrl = config["BancoPopular:BaseUrl"] 
            ?? "https://api.bpdapi.com.do/v1/auto/";
        _httpClient.BaseAddress = new Uri(baseUrl);
        _config = config;
        _logger = logger;
    }

    public async Task<FinancingSimulation> SimulateAsync(SimulationRequest request)
    {
        await EnsureAuthenticatedAsync();

        var apiRequest = new
        {
            vehicleType = request.VehicleType.ToString().ToUpper(),
            vehicleValue = request.VehicleValue,
            downPayment = request.DownPayment,
            termMonths = request.TermMonths,
            customerRNC = request.CustomerCedula
        };

        var response = await _httpClient.PostAsJsonAsync("simulations", apiRequest);
        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<BpSimulationResponse>();

        return MapToSimulation(result!, request);
    }

    public async Task<BpAutoApplication> CreateApplicationAsync(
        string simulationId,
        CustomerInfo customer,
        VehicleInfo vehicle,
        Dictionary<string, byte[]> documents)
    {
        await EnsureAuthenticatedAsync();

        var dealerId = _config["BancoPopular:DealerId"];

        var docBase64 = documents.ToDictionary(
            kvp => kvp.Key,
            kvp => Convert.ToBase64String(kvp.Value)
        );

        var request = new
        {
            simulationId,
            dealerId,
            customer = new
            {
                cedula = customer.Cedula,
                firstName = customer.FirstName,
                lastName = customer.LastName,
                email = customer.Email,
                phone = customer.Phone,
                monthlyIncome = customer.MonthlyIncome
            },
            vehicle = new
            {
                make = vehicle.Make,
                model = vehicle.Model,
                year = vehicle.Year,
                vin = vehicle.Vin,
                color = vehicle.Color
            },
            documents = docBase64
        };

        var response = await _httpClient.PostAsJsonAsync("applications", request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<BpApplicationResponse>();

        _logger.LogInformation(
            "Created financing application {AppId} for {Cedula}",
            result!.ApplicationId, MaskCedula(customer.Cedula));

        return MapToApplication(result, customer, vehicle);
    }

    public async Task<BpAutoApplication?> GetApplicationAsync(string applicationId)
    {
        await EnsureAuthenticatedAsync();

        var response = await _httpClient.GetAsync($"applications/{applicationId}");
        
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<BpApplicationResponse>();

        return MapToApplication(result!, null, null);
    }

    public async Task<List<BpAutoApplication>> GetDealerApplicationsAsync(
        string dealerId,
        ApplicationStatus? status = null,
        DateTime? fromDate = null)
    {
        await EnsureAuthenticatedAsync();

        var queryParams = new List<string> { $"dealerId={dealerId}" };
        
        if (status.HasValue)
            queryParams.Add($"status={status}");
        if (fromDate.HasValue)
            queryParams.Add($"fromDate={fromDate:yyyy-MM-dd}");

        var url = $"applications?{string.Join("&", queryParams)}";
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var results = await response.Content
            .ReadFromJsonAsync<List<BpApplicationResponse>>();

        return results?.Select(r => MapToApplication(r, null, null)).ToList() 
            ?? new();
    }

    public async Task<bool> CancelApplicationAsync(
        string applicationId, string reason)
    {
        await EnsureAuthenticatedAsync();

        var request = new { reason };
        var response = await _httpClient.PostAsJsonAsync(
            $"applications/{applicationId}/cancel", request);

        return response.IsSuccessStatusCode;
    }

    private async Task EnsureAuthenticatedAsync()
    {
        if (_accessToken != null && DateTime.UtcNow < _tokenExpiry)
            return;

        var tokenRequest = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("client_id", 
                _config["BancoPopular:ClientId"]!),
            new KeyValuePair<string, string>("client_secret", 
                _config["BancoPopular:ClientSecret"]!),
            new KeyValuePair<string, string>("scope", "auto-financing")
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

    private static FinancingSimulation MapToSimulation(
        BpSimulationResponse response, 
        SimulationRequest request)
    {
        return new FinancingSimulation(
            SimulationId: response.SimulationId,
            IsApproved: response.Approved,
            VehicleValue: request.VehicleValue,
            DownPayment: request.DownPayment,
            AmountFinanced: response.AmountFinanced,
            InterestRate: response.InterestRate,
            TermMonths: request.TermMonths,
            MonthlyPayment: response.MonthlyPayment,
            TotalPayment: response.TotalPayment,
            TotalInterest: response.TotalInterest,
            InsuranceRequired: response.InsuranceRequired,
            InsuranceMonthly: response.InsuranceMonthly,
            PaymentWithInsurance: response.PaymentWithInsurance,
            FirstPaymentDate: DateTime.Parse(response.FirstPaymentDate),
            Conditions: response.Conditions,
            ValidUntil: DateTime.UtcNow.AddHours(24)
        );
    }

    private static BpAutoApplication MapToApplication(
        BpApplicationResponse response,
        CustomerInfo? customer,
        VehicleInfo? vehicle)
    {
        return new BpAutoApplication(
            ApplicationId: response.ApplicationId,
            Status: Enum.Parse<ApplicationStatus>(
                response.Status.Replace("_", ""), ignoreCase: true),
            AmountRequested: response.AmountRequested ?? 0,
            ApprovedAmount: response.ApprovedAmount,
            InterestRate: response.ApprovedRate ?? 0,
            TermMonths: response.TermMonths ?? 0,
            MonthlyPayment: response.MonthlyPayment ?? 0,
            Customer: customer!,
            Vehicle: vehicle!,
            RequiredDocuments: response.RequiredDocuments ?? new(),
            NextSteps: response.NextSteps ?? new(),
            CreatedAt: response.CreatedAt ?? DateTime.UtcNow,
            ApprovedAt: response.ApprovedAt,
            DisbursementDate: response.DisbursementDate
        );
    }

    private static string MaskCedula(string cedula)
        => cedula.Length >= 11 ? $"{cedula[..3]}****{cedula[^4..]}" : "***";
}

// DTOs internos
internal record BpSimulationResponse(
    string SimulationId,
    bool Approved,
    decimal AmountFinanced,
    decimal InterestRate,
    decimal MonthlyPayment,
    decimal TotalPayment,
    decimal TotalInterest,
    string FirstPaymentDate,
    bool InsuranceRequired,
    decimal? InsuranceMonthly,
    decimal PaymentWithInsurance,
    List<string> Conditions
);

internal record BpApplicationResponse(
    string ApplicationId,
    string Status,
    decimal? AmountRequested,
    decimal? ApprovedAmount,
    decimal? ApprovedRate,
    int? TermMonths,
    decimal? MonthlyPayment,
    List<string>? RequiredDocuments,
    List<string>? NextSteps,
    DateTime? CreatedAt,
    DateTime? ApprovedAt,
    DateTime? DisbursementDate
);
```

---

## ⚛️ React Component

```tsx
// components/BancoPopularFinancing.tsx
import { useState } from 'react';
import { useMutation } from '@tanstack/react-query';
import { financingService } from '@/services/financingService';
import { Building, Car, FileText, CheckCircle, Clock } from 'lucide-react';

interface Props {
  vehiclePrice: number;
  vehicleMake: string;
  vehicleModel: string;
  vehicleYear: number;
  isNew: boolean;
  onApplicationSubmitted?: (appId: string) => void;
}

export function BancoPopularFinancing({
  vehiclePrice,
  vehicleMake,
  vehicleModel,
  vehicleYear,
  isNew,
  onApplicationSubmitted,
}: Props) {
  const [step, setStep] = useState<'simulate' | 'apply' | 'submitted'>('simulate');
  const [simulation, setSimulation] = useState<FinancingSimulation | null>(null);
  
  // Formulario de simulación
  const [downPayment, setDownPayment] = useState(vehiclePrice * 0.2);
  const [termMonths, setTermMonths] = useState(48);

  // Formulario de solicitud
  const [customerData, setCustomerData] = useState({
    cedula: '',
    firstName: '',
    lastName: '',
    email: '',
    phone: '',
    monthlyIncome: 0,
  });

  const simulationMutation = useMutation({
    mutationFn: () => financingService.simulateBancoPopular({
      vehicleType: isNew ? 'NEW' : 'USED',
      vehicleValue: vehiclePrice,
      downPayment,
      termMonths,
    }),
    onSuccess: (result) => {
      setSimulation(result);
      if (result.isApproved) {
        setStep('apply');
      }
    },
  });

  const applicationMutation = useMutation({
    mutationFn: () => financingService.applyBancoPopular({
      simulationId: simulation!.simulationId,
      customer: customerData,
      vehicle: {
        make: vehicleMake,
        model: vehicleModel,
        year: vehicleYear,
        isNew,
        value: vehiclePrice,
      },
    }),
    onSuccess: (result) => {
      setStep('submitted');
      onApplicationSubmitted?.(result.applicationId);
    },
  });

  return (
    <div className="bg-white rounded-xl shadow-lg overflow-hidden">
      {/* Header con logo Banco Popular */}
      <div className="bg-[#00529B] text-white p-4">
        <div className="flex items-center gap-3">
          <Building className="w-8 h-8" />
          <div>
            <h3 className="font-bold text-lg">Préstamo Auto Popular</h3>
            <p className="text-blue-200 text-sm">
              Financiamiento hasta 72 meses
            </p>
          </div>
        </div>
      </div>

      <div className="p-6">
        {step === 'simulate' && (
          <SimulationForm
            vehiclePrice={vehiclePrice}
            downPayment={downPayment}
            setDownPayment={setDownPayment}
            termMonths={termMonths}
            setTermMonths={setTermMonths}
            onSimulate={() => simulationMutation.mutate()}
            isLoading={simulationMutation.isPending}
          />
        )}

        {step === 'apply' && simulation && (
          <ApplicationForm
            simulation={simulation}
            customerData={customerData}
            setCustomerData={setCustomerData}
            onSubmit={() => applicationMutation.mutate()}
            onBack={() => setStep('simulate')}
            isLoading={applicationMutation.isPending}
          />
        )}

        {step === 'submitted' && (
          <SubmittedConfirmation 
            applicationId={applicationMutation.data?.applicationId} 
          />
        )}
      </div>
    </div>
  );
}

function SimulationForm({ 
  vehiclePrice, 
  downPayment, 
  setDownPayment, 
  termMonths, 
  setTermMonths,
  onSimulate,
  isLoading,
}: any) {
  const minDownPayment = vehiclePrice * 0.1;
  const maxDownPayment = vehiclePrice * 0.5;
  const financing = vehiclePrice - downPayment;

  return (
    <div className="space-y-6">
      <div className="bg-blue-50 rounded-lg p-4">
        <div className="flex justify-between mb-2">
          <span className="text-gray-600">Precio del Vehículo</span>
          <span className="font-bold">RD$ {vehiclePrice.toLocaleString()}</span>
        </div>
        <div className="flex justify-between">
          <span className="text-gray-600">A Financiar</span>
          <span className="font-bold text-blue-600">
            RD$ {financing.toLocaleString()}
          </span>
        </div>
      </div>

      {/* Inicial */}
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-2">
          Inicial (mínimo 10%)
        </label>
        <input
          type="range"
          min={minDownPayment}
          max={maxDownPayment}
          step={10000}
          value={downPayment}
          onChange={(e) => setDownPayment(Number(e.target.value))}
          className="w-full"
        />
        <div className="flex justify-between text-sm text-gray-500">
          <span>RD$ {downPayment.toLocaleString()}</span>
          <span>{Math.round((downPayment / vehiclePrice) * 100)}%</span>
        </div>
      </div>

      {/* Plazo */}
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-2">
          Plazo
        </label>
        <div className="grid grid-cols-4 gap-2">
          {[36, 48, 60, 72].map((months) => (
            <button
              key={months}
              onClick={() => setTermMonths(months)}
              className={`py-2 rounded-lg border text-sm ${
                termMonths === months
                  ? 'bg-[#00529B] text-white border-[#00529B]'
                  : 'bg-white text-gray-700 border-gray-300'
              }`}
            >
              {months} meses
            </button>
          ))}
        </div>
      </div>

      <button
        onClick={onSimulate}
        disabled={isLoading}
        className="w-full py-3 bg-[#00529B] text-white rounded-lg font-medium
                   hover:bg-[#003d73] disabled:opacity-50"
      >
        {isLoading ? 'Calculando...' : 'Simular Financiamiento'}
      </button>

      <p className="text-xs text-gray-500 text-center">
        La simulación no afecta tu historial crediticio
      </p>
    </div>
  );
}

function ApplicationForm({
  simulation,
  customerData,
  setCustomerData,
  onSubmit,
  onBack,
  isLoading,
}: any) {
  return (
    <div className="space-y-6">
      {/* Resumen de simulación */}
      <div className="bg-green-50 border border-green-200 rounded-lg p-4">
        <div className="flex items-center gap-2 text-green-700 mb-2">
          <CheckCircle className="w-5 h-5" />
          <span className="font-medium">Simulación Aprobada</span>
        </div>
        <div className="grid grid-cols-2 gap-4 text-sm">
          <div>
            <span className="text-gray-600">Cuota Mensual</span>
            <p className="font-bold text-lg">
              RD$ {simulation.monthlyPayment.toLocaleString()}
            </p>
          </div>
          <div>
            <span className="text-gray-600">Tasa</span>
            <p className="font-bold text-lg">{simulation.interestRate}%</p>
          </div>
        </div>
      </div>

      {/* Formulario de datos */}
      <div className="grid grid-cols-2 gap-4">
        <div>
          <label className="block text-sm text-gray-600 mb-1">Cédula</label>
          <input
            type="text"
            value={customerData.cedula}
            onChange={(e) => setCustomerData({
              ...customerData,
              cedula: e.target.value.replace(/\D/g, ''),
            })}
            placeholder="00000000000"
            maxLength={11}
            className="w-full px-3 py-2 border rounded-lg"
          />
        </div>
        <div>
          <label className="block text-sm text-gray-600 mb-1">Teléfono</label>
          <input
            type="tel"
            value={customerData.phone}
            onChange={(e) => setCustomerData({
              ...customerData,
              phone: e.target.value,
            })}
            placeholder="809-000-0000"
            className="w-full px-3 py-2 border rounded-lg"
          />
        </div>
        <div>
          <label className="block text-sm text-gray-600 mb-1">Nombre</label>
          <input
            type="text"
            value={customerData.firstName}
            onChange={(e) => setCustomerData({
              ...customerData,
              firstName: e.target.value,
            })}
            className="w-full px-3 py-2 border rounded-lg"
          />
        </div>
        <div>
          <label className="block text-sm text-gray-600 mb-1">Apellido</label>
          <input
            type="text"
            value={customerData.lastName}
            onChange={(e) => setCustomerData({
              ...customerData,
              lastName: e.target.value,
            })}
            className="w-full px-3 py-2 border rounded-lg"
          />
        </div>
        <div className="col-span-2">
          <label className="block text-sm text-gray-600 mb-1">Email</label>
          <input
            type="email"
            value={customerData.email}
            onChange={(e) => setCustomerData({
              ...customerData,
              email: e.target.value,
            })}
            className="w-full px-3 py-2 border rounded-lg"
          />
        </div>
        <div className="col-span-2">
          <label className="block text-sm text-gray-600 mb-1">
            Ingreso Mensual (RD$)
          </label>
          <input
            type="number"
            value={customerData.monthlyIncome || ''}
            onChange={(e) => setCustomerData({
              ...customerData,
              monthlyIncome: Number(e.target.value),
            })}
            className="w-full px-3 py-2 border rounded-lg"
          />
        </div>
      </div>

      <div className="flex gap-4">
        <button
          onClick={onBack}
          className="flex-1 py-3 border border-gray-300 rounded-lg"
        >
          Volver
        </button>
        <button
          onClick={onSubmit}
          disabled={isLoading}
          className="flex-1 py-3 bg-[#00529B] text-white rounded-lg font-medium
                     disabled:opacity-50"
        >
          {isLoading ? 'Enviando...' : 'Solicitar Financiamiento'}
        </button>
      </div>
    </div>
  );
}

function SubmittedConfirmation({ applicationId }: { applicationId?: string }) {
  return (
    <div className="text-center py-8">
      <div className="w-16 h-16 bg-green-100 rounded-full flex items-center 
                      justify-center mx-auto mb-4">
        <CheckCircle className="w-8 h-8 text-green-600" />
      </div>
      <h3 className="text-xl font-bold mb-2">¡Solicitud Enviada!</h3>
      <p className="text-gray-600 mb-4">
        Tu solicitud #{applicationId} está siendo procesada.
      </p>
      <div className="flex items-center justify-center gap-2 text-sm text-gray-500">
        <Clock className="w-4 h-4" />
        <span>Respuesta estimada: 24-48 horas</span>
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
public class BancoPopularController : ControllerBase
{
    private readonly IBancoPopularAutoService _bpService;

    public BancoPopularController(IBancoPopularAutoService bpService)
    {
        _bpService = bpService;
    }

    [HttpPost("simulate")]
    public async Task<ActionResult<FinancingSimulation>> Simulate(
        [FromBody] SimulationRequest request)
    {
        var result = await _bpService.SimulateAsync(request);
        return Ok(result);
    }

    [HttpPost("apply")]
    [Authorize]
    public async Task<ActionResult<BpAutoApplication>> Apply(
        [FromBody] BpApplicationRequest request)
    {
        var result = await _bpService.CreateApplicationAsync(
            request.SimulationId,
            request.Customer,
            request.Vehicle,
            new Dictionary<string, byte[]>() // Documents would come from form
        );
        return Ok(result);
    }

    [HttpGet("applications/{id}")]
    [Authorize]
    public async Task<ActionResult<BpAutoApplication>> GetApplication(string id)
    {
        var result = await _bpService.GetApplicationAsync(id);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook([FromBody] BpWebhookPayload payload)
    {
        // Procesar webhook de cambio de estado
        // Notificar al dealer y cliente
        return Ok();
    }
}

public record BpApplicationRequest(
    string SimulationId,
    CustomerInfo Customer,
    VehicleInfo Vehicle
);
```

---

## ⚙️ Configuración

```json
{
  "BancoPopular": {
    "BaseUrl": "https://api.bpdapi.com.do/v1/auto/",
    "ClientId": "okla-dealer-001",
    "ClientSecret": "xxxxx",
    "DealerId": "DEALER-OKLA-001",
    "WebhookSecret": "whsec_xxxxx",
    "Commission": 0.75
  }
}
```

---

## 📞 Contacto Banco Popular

| Departamento | Teléfono | Email |
|--------------|----------|-------|
| Banca Empresa (Convenios) | 809-544-5500 ext. 2500 | empresas@bpd.com.do |
| Préstamos Auto | 809-544-5500 ext. 3000 | autofinancing@bpd.com.do |
| API Support | 809-544-5500 ext. 4000 | apidev@bpd.com.do |

---

**Anterior:** [DATACREDITO_API.md](./DATACREDITO_API.md)  
**Siguiente:** [ASOCIACIONES_AHORROS_API.md](./ASOCIACIONES_AHORROS_API.md)
