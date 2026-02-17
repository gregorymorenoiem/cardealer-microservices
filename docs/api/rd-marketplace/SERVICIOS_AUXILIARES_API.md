# 🔧 Servicios Auxiliares API

**Servicios:** Inspección Pre-Compra, Grúas, Talleres Certificados  
**Uso:** Servicios de valor agregado para compradores  
**Prioridad:** ⭐⭐⭐ MEDIA (Diferenciación competitiva)

---

## 📋 Información General

Este documento agrupa servicios auxiliares que complementan la experiencia del marketplace de vehículos:

| Servicio | Descripción | Partners Potenciales |
|----------|-------------|---------------------|
| **Inspección Pre-Compra** | Evaluación técnica profesional | INTRANT, talleres certificados |
| **Servicios de Grúa** | Transporte de vehículos | Grúas RD, Asistencia Vial |
| **Talleres Certificados** | Reparaciones y mantenimiento | Red de talleres partner |
| **Historial de Servicios** | Registro de mantenimientos | Talleres + dealers |

---

## 🔍 1. Inspección Pre-Compra

### Proveedores Sugeridos

| Proveedor | Tipo | Cobertura | Precio Est. |
|-----------|------|-----------|-------------|
| **INTRANT** | Gobierno | Nacional | RD$1,500 |
| **AutoCheck RD** | Privado | SD, Santiago | RD$3,500 |
| **Dekra RD** | Internacional | SD | RD$5,000 |
| **Talleres OKLA** | Partner | Nacional | RD$2,500 |

### Puntos de Inspección

```yaml
Inspección Básica (15 puntos):
  - Motor: Compresión, fugas, ruidos
  - Transmisión: Cambios, vibraciones
  - Frenos: Pastillas, discos, líneas
  - Suspensión: Amortiguadores, rótulas
  - Dirección: Juego, alineación
  - Eléctrico: Batería, alternador, luces
  - Carrocería: Golpes, óxido, pintura
  - Interior: Tablero, A/C, asientos
  - Neumáticos: Desgaste, alineación
  - Documentos: VIN, placa, matrícula

Inspección Completa (50 puntos):
  - Todo lo anterior +
  - Diagnóstico computarizado OBD-II
  - Prueba de compresión detallada
  - Análisis de fluidos (aceite, refrigerante)
  - Inspección de chasis (levantado)
  - Test de emisiones
  - Prueba de manejo (15 km)
  - Historial de accidentes (carfax local)
  - Valuación de mercado
```

### Modelos C#

```csharp
namespace AuxiliaryService.Domain.Entities;

/// <summary>
/// Solicitud de inspección
/// </summary>
public record InspectionRequest(
    Guid Id,
    Guid VehicleId,
    Guid BuyerId,
    Guid? SellerId,
    InspectionType Type,
    InspectionProvider Provider,
    string ScheduledLocation,
    DateTime ScheduledDate,
    TimeSlot TimeSlot,
    InspectionStatus Status,
    decimal Price,
    bool IsPaidByBuyer,
    DateTime CreatedAt
);

public enum InspectionType
{
    Basic,      // 15 puntos - RD$2,500
    Complete,   // 50 puntos - RD$5,000
    Premium     // Completa + Historial + Valuación - RD$8,000
}

public enum InspectionProvider
{
    OklaPartner,
    Intrant,
    AutoCheckRd,
    Dekra,
    IndependentMechanic
}

public enum InspectionStatus
{
    Pending,
    Confirmed,
    InProgress,
    Completed,
    Cancelled,
    Failed
}

public record TimeSlot(
    TimeOnly StartTime,
    TimeOnly EndTime
);

/// <summary>
/// Resultado de inspección
/// </summary>
public record InspectionResult(
    Guid InspectionId,
    DateTime InspectedAt,
    string InspectorName,
    string InspectorCertification,
    int TotalPoints,
    int PassedPoints,
    decimal OverallScore,
    InspectionVerdict Verdict,
    List<InspectionCategory> Categories,
    List<InspectionIssue> Issues,
    List<string> PhotoUrls,
    string? OdbReportUrl,
    decimal? EstimatedRepairCost,
    decimal? MarketValue,
    string Summary,
    string Recommendations
);

public enum InspectionVerdict
{
    Excellent,    // 90-100%
    Good,         // 75-89%
    Fair,         // 60-74%
    Poor,         // 40-59%
    Failed        // <40%
}

public record InspectionCategory(
    string Name,
    int MaxPoints,
    int ScoredPoints,
    List<InspectionItem> Items
);

public record InspectionItem(
    string Name,
    bool Passed,
    string? Notes,
    string? PhotoUrl,
    IssueSeverity? Severity
);

public record InspectionIssue(
    string Component,
    string Description,
    IssueSeverity Severity,
    decimal? EstimatedRepairCost,
    bool IsUrgent,
    string? PhotoUrl
);

public enum IssueSeverity
{
    Minor,      // Cosmético, no afecta funcionamiento
    Moderate,   // Requiere atención pronto
    Major,      // Afecta seguridad o funcionamiento
    Critical    // No recomendado comprar sin reparar
}
```

### Service Implementation

```csharp
namespace AuxiliaryService.Infrastructure.Services;

public class InspectionService : IInspectionService
{
    private readonly IInspectionRepository _repository;
    private readonly INotificationService _notificationService;
    private readonly IPaymentService _paymentService;
    private readonly ILogger<InspectionService> _logger;

    public async Task<InspectionRequest> RequestInspectionAsync(
        CreateInspectionRequest request)
    {
        // Verificar disponibilidad del proveedor
        var isAvailable = await CheckProviderAvailabilityAsync(
            request.Provider,
            request.Location,
            request.ScheduledDate,
            request.TimeSlot);

        if (!isAvailable)
            throw new InvalidOperationException("Time slot not available");

        var price = CalculatePrice(request.Type, request.Provider);

        var inspection = new InspectionRequest(
            Id: Guid.NewGuid(),
            VehicleId: request.VehicleId,
            BuyerId: request.BuyerId,
            SellerId: request.SellerId,
            Type: request.Type,
            Provider: request.Provider,
            ScheduledLocation: request.Location,
            ScheduledDate: request.ScheduledDate,
            TimeSlot: request.TimeSlot,
            Status: InspectionStatus.Pending,
            Price: price,
            IsPaidByBuyer: true,
            CreatedAt: DateTime.UtcNow
        );

        await _repository.SaveAsync(inspection);

        // Notificar a las partes
        await _notificationService.SendInspectionRequestedAsync(inspection);

        // Si el vendedor ofrece pagar, notificarle
        if (request.RequestSellerPayment)
        {
            await _notificationService.SendPaymentRequestToSellerAsync(inspection);
        }

        return inspection;
    }

    public async Task<InspectionResult> SubmitInspectionResultAsync(
        Guid inspectionId,
        InspectionResultInput input)
    {
        var inspection = await _repository.GetByIdAsync(inspectionId)
            ?? throw new NotFoundException("Inspection not found");

        // Calcular puntaje
        var totalPassed = input.Categories.Sum(c => c.Items.Count(i => i.Passed));
        var totalItems = input.Categories.Sum(c => c.Items.Count);
        var score = (decimal)totalPassed / totalItems * 100;

        // Determinar veredicto
        var verdict = score switch
        {
            >= 90 => InspectionVerdict.Excellent,
            >= 75 => InspectionVerdict.Good,
            >= 60 => InspectionVerdict.Fair,
            >= 40 => InspectionVerdict.Poor,
            _ => InspectionVerdict.Failed
        };

        // Identificar problemas
        var issues = input.Categories
            .SelectMany(c => c.Items)
            .Where(i => !i.Passed && i.Severity.HasValue)
            .Select(i => new InspectionIssue(
                Component: i.Name,
                Description: i.Notes ?? "Issue identified",
                Severity: i.Severity!.Value,
                EstimatedRepairCost: i.EstimatedCost,
                IsUrgent: i.Severity >= IssueSeverity.Major,
                PhotoUrl: i.PhotoUrl
            ))
            .ToList();

        var result = new InspectionResult(
            InspectionId: inspectionId,
            InspectedAt: DateTime.UtcNow,
            InspectorName: input.InspectorName,
            InspectorCertification: input.InspectorCertification,
            TotalPoints: totalItems,
            PassedPoints: totalPassed,
            OverallScore: Math.Round(score, 1),
            Verdict: verdict,
            Categories: input.Categories.Select(c => new InspectionCategory(
                Name: c.Name,
                MaxPoints: c.Items.Count,
                ScoredPoints: c.Items.Count(i => i.Passed),
                Items: c.Items
            )).ToList(),
            Issues: issues,
            PhotoUrls: input.PhotoUrls,
            OdbReportUrl: input.OdbReportUrl,
            EstimatedRepairCost: issues.Sum(i => i.EstimatedRepairCost ?? 0),
            MarketValue: input.MarketValue,
            Summary: GenerateSummary(verdict, issues),
            Recommendations: GenerateRecommendations(verdict, issues)
        );

        await _repository.SaveResultAsync(result);

        // Actualizar estado
        await _repository.UpdateStatusAsync(inspectionId, InspectionStatus.Completed);

        // Notificar al comprador
        await _notificationService.SendInspectionCompletedAsync(result);

        return result;
    }

    private decimal CalculatePrice(InspectionType type, InspectionProvider provider)
    {
        var basePrice = type switch
        {
            InspectionType.Basic => 2500m,
            InspectionType.Complete => 5000m,
            InspectionType.Premium => 8000m,
            _ => 2500m
        };

        var multiplier = provider switch
        {
            InspectionProvider.OklaPartner => 1.0m,
            InspectionProvider.Intrant => 0.6m,
            InspectionProvider.AutoCheckRd => 1.4m,
            InspectionProvider.Dekra => 2.0m,
            InspectionProvider.IndependentMechanic => 0.8m,
            _ => 1.0m
        };

        return basePrice * multiplier;
    }

    private string GenerateSummary(InspectionVerdict verdict, List<InspectionIssue> issues)
    {
        var criticalCount = issues.Count(i => i.Severity == IssueSeverity.Critical);
        var majorCount = issues.Count(i => i.Severity == IssueSeverity.Major);

        return verdict switch
        {
            InspectionVerdict.Excellent => "Vehículo en excelentes condiciones. Recomendado para compra.",
            InspectionVerdict.Good => $"Vehículo en buenas condiciones. {(issues.Any() ? $"Se identificaron {issues.Count} puntos menores." : "")}",
            InspectionVerdict.Fair => $"Vehículo en condiciones aceptables. {majorCount} problemas moderados identificados.",
            InspectionVerdict.Poor => $"Vehículo requiere reparaciones significativas. {criticalCount + majorCount} problemas importantes.",
            InspectionVerdict.Failed => "No se recomienda la compra sin reparaciones mayores.",
            _ => "Inspección completada."
        };
    }

    private string GenerateRecommendations(InspectionVerdict verdict, List<InspectionIssue> issues)
    {
        var recommendations = new List<string>();

        if (verdict == InspectionVerdict.Failed)
        {
            recommendations.Add("❌ No recomendamos proceder con la compra en las condiciones actuales.");
            recommendations.Add("💰 Solicitar al vendedor que realice las reparaciones críticas.");
        }
        else if (issues.Any(i => i.Severity >= IssueSeverity.Major))
        {
            recommendations.Add("⚠️ Negociar descuento basado en costo estimado de reparaciones.");
            var repairCost = issues.Sum(i => i.EstimatedRepairCost ?? 0);
            if (repairCost > 0)
                recommendations.Add($"💵 Costo estimado de reparaciones: RD${repairCost:N0}");
        }
        else if (verdict >= InspectionVerdict.Good)
        {
            recommendations.Add("✅ Vehículo en buenas condiciones para compra.");
            recommendations.Add("📋 Verificar documentación antes de finalizar.");
        }

        return string.Join("\n", recommendations);
    }
}
```

---

## 🚛 2. Servicios de Grúa

### Proveedores Sugeridos

| Proveedor | Cobertura | Precio Base | Por Km |
|-----------|-----------|-------------|--------|
| **Grúas RD** | Nacional | RD$2,000 | RD$50 |
| **Asistencia Vial Popular** | SD, Santiago | RD$1,500 | RD$40 |
| **Seguros (incluido)** | Según póliza | Gratis | Gratis |
| **OKLA Partners** | Nacional | RD$1,800 | RD$45 |

### Modelos C#

```csharp
namespace AuxiliaryService.Domain.Entities;

/// <summary>
/// Solicitud de grúa
/// </summary>
public record TowRequest(
    Guid Id,
    Guid? VehicleId,
    Guid RequesterId,
    TowType Type,
    TowProvider Provider,
    Location PickupLocation,
    Location DestinationLocation,
    decimal EstimatedDistance,
    decimal EstimatedPrice,
    decimal? FinalPrice,
    TowStatus Status,
    DateTime RequestedAt,
    DateTime? ScheduledPickupTime,
    DateTime? ActualPickupTime,
    DateTime? DeliveryTime,
    string? DriverName,
    string? DriverPhone,
    string? TruckPlate,
    string? Notes
);

public record Location(
    double Latitude,
    double Longitude,
    string Address,
    string? Reference
);

public enum TowType
{
    Standard,       // Grúa plataforma normal
    Flatbed,        // Plataforma plana (vehículos bajos)
    HeavyDuty,      // Para camiones/vehículos pesados
    Motorcycle,     // Para motos
    Emergency       // Asistencia en carretera
}

public enum TowProvider
{
    OklaPartner,
    GruasRd,
    AsistenciaVialPopular,
    InsuranceCoverage,
    Independent
}

public enum TowStatus
{
    Pending,
    Confirmed,
    DriverAssigned,
    EnRoute,
    AtPickup,
    InTransit,
    Delivered,
    Cancelled
}

/// <summary>
/// Tracking de grúa en tiempo real
/// </summary>
public record TowTracking(
    Guid TowRequestId,
    Location CurrentLocation,
    decimal DistanceToPickup,
    int EstimatedMinutesToPickup,
    decimal DistanceToDestination,
    int EstimatedMinutesToDestination,
    DateTime UpdatedAt
);
```

### Service Implementation

```csharp
public class TowService : ITowService
{
    private readonly ITowRepository _repository;
    private readonly ILocationService _locationService;
    private readonly INotificationService _notificationService;
    private readonly List<ITowProviderAdapter> _providers;

    public async Task<TowRequest> RequestTowAsync(CreateTowRequest request)
    {
        // Calcular distancia
        var distance = await _locationService.GetDistancesAsync(
            new GeoCoordinates(request.PickupLocation.Latitude, request.PickupLocation.Longitude),
            new List<GeoCoordinates> { 
                new(request.DestinationLocation.Latitude, request.DestinationLocation.Longitude) 
            });

        var distanceKm = distance.First().DistanceMeters / 1000m;

        // Obtener cotizaciones de proveedores
        var quotes = await GetQuotesAsync(request.Type, distanceKm);
        var selectedQuote = quotes.OrderBy(q => q.Price).First();

        var towRequest = new TowRequest(
            Id: Guid.NewGuid(),
            VehicleId: request.VehicleId,
            RequesterId: request.RequesterId,
            Type: request.Type,
            Provider: selectedQuote.Provider,
            PickupLocation: request.PickupLocation,
            DestinationLocation: request.DestinationLocation,
            EstimatedDistance: distanceKm,
            EstimatedPrice: selectedQuote.Price,
            FinalPrice: null,
            Status: TowStatus.Pending,
            RequestedAt: DateTime.UtcNow,
            ScheduledPickupTime: request.ScheduledTime,
            ActualPickupTime: null,
            DeliveryTime: null,
            DriverName: null,
            DriverPhone: null,
            TruckPlate: null,
            Notes: request.Notes
        );

        await _repository.SaveAsync(towRequest);

        // Enviar a proveedor
        var provider = _providers.First(p => p.Provider == selectedQuote.Provider);
        await provider.SubmitRequestAsync(towRequest);

        // Notificar al usuario
        await _notificationService.SendTowRequestConfirmedAsync(towRequest);

        return towRequest;
    }

    public async Task<TowTracking?> GetTrackingAsync(Guid towRequestId)
    {
        var request = await _repository.GetByIdAsync(towRequestId);
        if (request == null || request.Status < TowStatus.DriverAssigned)
            return null;

        var provider = _providers.First(p => p.Provider == request.Provider);
        return await provider.GetTrackingAsync(towRequestId);
    }

    private async Task<List<TowQuote>> GetQuotesAsync(TowType type, decimal distanceKm)
    {
        var quotes = new List<TowQuote>();

        foreach (var provider in _providers)
        {
            try
            {
                var quote = await provider.GetQuoteAsync(type, distanceKm);
                quotes.Add(quote);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get quote from {Provider}", provider.Provider);
            }
        }

        // Si no hay cotizaciones, usar precio por defecto
        if (!quotes.Any())
        {
            quotes.Add(new TowQuote(
                Provider: TowProvider.OklaPartner,
                Price: CalculateDefaultPrice(type, distanceKm),
                EstimatedMinutes: (int)(distanceKm * 2) + 30
            ));
        }

        return quotes;
    }

    private decimal CalculateDefaultPrice(TowType type, decimal distanceKm)
    {
        var basePrice = type switch
        {
            TowType.Standard => 2000m,
            TowType.Flatbed => 2500m,
            TowType.HeavyDuty => 4000m,
            TowType.Motorcycle => 1500m,
            TowType.Emergency => 3000m,
            _ => 2000m
        };

        return basePrice + (distanceKm * 50m);
    }
}

public record TowQuote(
    TowProvider Provider,
    decimal Price,
    int EstimatedMinutes
);
```

---

## 🔧 3. Talleres Certificados

### Red de Talleres Partner

```yaml
Requisitos para ser Taller OKLA:
  - Licencia vigente del INTRANT
  - Mínimo 5 años de experiencia
  - Certificación en marcas específicas
  - Garantía mínima 6 meses en trabajos
  - Sistema de facturación digital
  - Capacidad de reportar a OKLA

Beneficios para Talleres:
  - Referidos desde la plataforma
  - Badge de "Taller Certificado OKLA"
  - Acceso a clientes pre-calificados
  - Sistema de citas integrado
  - Comisión: 5% por referido

Categorías:
  - Mecánica General
  - Electricidad Automotriz
  - Transmisiones
  - Aire Acondicionado
  - Carrocería y Pintura
  - Especialistas por Marca
```

### Modelos C#

```csharp
namespace AuxiliaryService.Domain.Entities;

/// <summary>
/// Taller certificado
/// </summary>
public record CertifiedWorkshop(
    Guid Id,
    string Name,
    string LegalName,
    string Rnc,
    Location Location,
    string Phone,
    string Email,
    string? Website,
    List<WorkshopSpecialty> Specialties,
    List<string> CertifiedBrands,
    decimal Rating,
    int TotalReviews,
    int YearsInBusiness,
    bool IsVerified,
    WorkshopTier Tier,
    WorkingHours Hours,
    List<string> PhotoUrls,
    DateTime JoinedAt
);

public enum WorkshopSpecialty
{
    GeneralMechanics,
    Electrical,
    Transmission,
    AirConditioning,
    BodyAndPaint,
    Tires,
    Diagnostics,
    Performance
}

public enum WorkshopTier
{
    Standard,       // Básico
    Preferred,      // Preferido (más referidos)
    Premium         // Premium (top listing)
}

public record WorkingHours(
    TimeOnly WeekdayOpen,
    TimeOnly WeekdayClose,
    TimeOnly? SaturdayOpen,
    TimeOnly? SaturdayClose,
    bool SundayClosed
);

/// <summary>
/// Cita en taller
/// </summary>
public record WorkshopAppointment(
    Guid Id,
    Guid WorkshopId,
    Guid UserId,
    Guid? VehicleId,
    string VehicleDescription,
    ServiceType ServiceType,
    string ServiceDescription,
    DateTime ScheduledDate,
    TimeSlot TimeSlot,
    AppointmentStatus Status,
    decimal? EstimatedCost,
    decimal? FinalCost,
    string? DiagnosticNotes,
    DateTime CreatedAt
);

public enum ServiceType
{
    Inspection,
    Maintenance,
    Repair,
    Diagnostic,
    BodyWork,
    Other
}

public enum AppointmentStatus
{
    Pending,
    Confirmed,
    InProgress,
    WaitingApproval,    // Esperando aprobación de costo
    Completed,
    Cancelled
}

/// <summary>
/// Reseña de taller
/// </summary>
public record WorkshopReview(
    Guid Id,
    Guid WorkshopId,
    Guid UserId,
    Guid? AppointmentId,
    int Rating,
    string? Comment,
    List<string>? PhotoUrls,
    int QualityRating,
    int PriceRating,
    int ServiceRating,
    bool IsVerifiedPurchase,
    DateTime CreatedAt
);
```

---

## ⚛️ React Components

### Componente de Inspección

```tsx
// components/InspectionScheduler.tsx
import { useState } from 'react';
import { useMutation, useQuery } from '@tanstack/react-query';
import { inspectionService } from '@/services/inspectionService';
import { Calendar, Clock, MapPin, Shield, DollarSign } from 'lucide-react';
import { DatePicker } from '@/components/ui/DatePicker';

interface Props {
  vehicleId: string;
  sellerId?: string;
  onScheduled: (inspectionId: string) => void;
}

const INSPECTION_TYPES = [
  { 
    type: 'Basic', 
    name: 'Inspección Básica', 
    points: 15, 
    price: 2500,
    description: 'Motor, frenos, suspensión, carrocería'
  },
  { 
    type: 'Complete', 
    name: 'Inspección Completa', 
    points: 50, 
    price: 5000,
    description: 'Todo + diagnóstico OBD-II + prueba de manejo',
    recommended: true
  },
  { 
    type: 'Premium', 
    name: 'Inspección Premium', 
    points: 50, 
    price: 8000,
    description: 'Completa + historial + valuación de mercado'
  },
];

export function InspectionScheduler({ vehicleId, sellerId, onScheduled }: Props) {
  const [selectedType, setSelectedType] = useState<string>('Complete');
  const [selectedDate, setSelectedDate] = useState<Date | null>(null);
  const [selectedTime, setSelectedTime] = useState<string | null>(null);
  const [location, setLocation] = useState<string>('');

  const timeSlotsQuery = useQuery({
    queryKey: ['inspection-slots', selectedDate],
    queryFn: () => inspectionService.getAvailableSlots(selectedDate!),
    enabled: !!selectedDate,
  });

  const scheduleMutation = useMutation({
    mutationFn: () => inspectionService.scheduleInspection({
      vehicleId,
      sellerId,
      type: selectedType,
      scheduledDate: selectedDate!,
      timeSlot: selectedTime!,
      location,
    }),
    onSuccess: (data) => onScheduled(data.id),
  });

  return (
    <div className="max-w-2xl mx-auto p-6 bg-white rounded-xl shadow-lg">
      <h2 className="text-2xl font-bold mb-6 flex items-center gap-2">
        <Shield className="w-6 h-6 text-blue-600" />
        Programar Inspección Pre-Compra
      </h2>

      {/* Tipo de Inspección */}
      <div className="mb-6">
        <label className="block text-sm font-medium mb-3">
          Tipo de Inspección
        </label>
        <div className="space-y-3">
          {INSPECTION_TYPES.map((insp) => (
            <div
              key={insp.type}
              onClick={() => setSelectedType(insp.type)}
              className={`p-4 border-2 rounded-lg cursor-pointer transition-all
                ${selectedType === insp.type 
                  ? 'border-blue-500 bg-blue-50' 
                  : 'border-gray-200 hover:border-gray-300'}`}
            >
              <div className="flex items-center justify-between">
                <div>
                  <div className="flex items-center gap-2">
                    <span className="font-medium">{insp.name}</span>
                    {insp.recommended && (
                      <span className="px-2 py-0.5 bg-green-100 text-green-700 text-xs rounded-full">
                        Recomendado
                      </span>
                    )}
                  </div>
                  <p className="text-sm text-gray-500">{insp.description}</p>
                  <p className="text-xs text-gray-400 mt-1">
                    {insp.points} puntos de inspección
                  </p>
                </div>
                <div className="text-right">
                  <span className="text-xl font-bold text-blue-600">
                    RD${insp.price.toLocaleString()}
                  </span>
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* Fecha */}
      <div className="mb-6">
        <label className="block text-sm font-medium mb-2">
          <Calendar className="w-4 h-4 inline mr-2" />
          Fecha
        </label>
        <DatePicker
          value={selectedDate}
          onChange={setSelectedDate}
          minDate={new Date()}
          maxDate={new Date(Date.now() + 30 * 24 * 60 * 60 * 1000)}
        />
      </div>

      {/* Horario */}
      {selectedDate && (
        <div className="mb-6">
          <label className="block text-sm font-medium mb-2">
            <Clock className="w-4 h-4 inline mr-2" />
            Horario Disponible
          </label>
          <div className="grid grid-cols-3 gap-2">
            {timeSlotsQuery.data?.map((slot) => (
              <button
                key={slot}
                onClick={() => setSelectedTime(slot)}
                disabled={!slot.available}
                className={`p-2 rounded-lg text-sm
                  ${selectedTime === slot.time
                    ? 'bg-blue-600 text-white'
                    : slot.available
                      ? 'bg-gray-100 hover:bg-gray-200'
                      : 'bg-gray-50 text-gray-300 cursor-not-allowed'}`}
              >
                {slot.time}
              </button>
            ))}
          </div>
        </div>
      )}

      {/* Ubicación */}
      <div className="mb-6">
        <label className="block text-sm font-medium mb-2">
          <MapPin className="w-4 h-4 inline mr-2" />
          Ubicación del Vehículo
        </label>
        <input
          type="text"
          value={location}
          onChange={(e) => setLocation(e.target.value)}
          placeholder="Dirección donde se encuentra el vehículo"
          className="w-full px-4 py-2 border rounded-lg"
        />
      </div>

      {/* Resumen y Pago */}
      <div className="bg-gray-50 rounded-lg p-4 mb-6">
        <h3 className="font-medium mb-2">Resumen</h3>
        <div className="space-y-1 text-sm">
          <div className="flex justify-between">
            <span>Inspección {selectedType}</span>
            <span>RD${INSPECTION_TYPES.find(t => t.type === selectedType)?.price.toLocaleString()}</span>
          </div>
          {selectedDate && (
            <div className="flex justify-between text-gray-500">
              <span>Fecha</span>
              <span>{selectedDate.toLocaleDateString('es-DO')}</span>
            </div>
          )}
          {selectedTime && (
            <div className="flex justify-between text-gray-500">
              <span>Hora</span>
              <span>{selectedTime}</span>
            </div>
          )}
        </div>
      </div>

      {/* Botón */}
      <button
        onClick={() => scheduleMutation.mutate()}
        disabled={!selectedDate || !selectedTime || !location || scheduleMutation.isPending}
        className="w-full py-3 bg-blue-600 text-white rounded-lg font-medium
          hover:bg-blue-700 disabled:opacity-50"
      >
        {scheduleMutation.isPending ? 'Procesando...' : 'Programar y Pagar'}
      </button>
    </div>
  );
}
```

### Resultado de Inspección

```tsx
// components/InspectionReport.tsx
import { CheckCircle, XCircle, AlertTriangle, Info } from 'lucide-react';

interface Props {
  result: InspectionResult;
}

export function InspectionReport({ result }: Props) {
  const verdictConfig = {
    Excellent: { color: 'green', icon: CheckCircle, label: 'Excelente' },
    Good: { color: 'blue', icon: CheckCircle, label: 'Bueno' },
    Fair: { color: 'yellow', icon: AlertTriangle, label: 'Regular' },
    Poor: { color: 'orange', icon: AlertTriangle, label: 'Deficiente' },
    Failed: { color: 'red', icon: XCircle, label: 'No Recomendado' },
  };

  const config = verdictConfig[result.verdict];
  const VerdictIcon = config.icon;

  return (
    <div className="bg-white rounded-xl shadow-lg overflow-hidden">
      {/* Header con Veredicto */}
      <div className={`p-6 bg-${config.color}-50`}>
        <div className="flex items-center gap-4">
          <div className={`p-4 rounded-full bg-${config.color}-100`}>
            <VerdictIcon className={`w-8 h-8 text-${config.color}-600`} />
          </div>
          <div>
            <h2 className={`text-2xl font-bold text-${config.color}-700`}>
              {config.label}
            </h2>
            <p className="text-gray-600">
              Puntuación: {result.overallScore}% ({result.passedPoints}/{result.totalPoints} puntos)
            </p>
          </div>
          <div className="ml-auto text-right">
            <div className="text-4xl font-bold text-gray-800">
              {result.overallScore}%
            </div>
          </div>
        </div>
      </div>

      {/* Resumen */}
      <div className="p-6 border-b">
        <h3 className="font-medium mb-2">Resumen</h3>
        <p className="text-gray-600">{result.summary}</p>
      </div>

      {/* Problemas Identificados */}
      {result.issues.length > 0 && (
        <div className="p-6 border-b">
          <h3 className="font-medium mb-4">
            Problemas Identificados ({result.issues.length})
          </h3>
          <div className="space-y-3">
            {result.issues.map((issue, i) => (
              <div key={i} className={`p-3 rounded-lg border-l-4 
                ${issue.severity === 'Critical' ? 'border-red-500 bg-red-50' :
                  issue.severity === 'Major' ? 'border-orange-500 bg-orange-50' :
                  issue.severity === 'Moderate' ? 'border-yellow-500 bg-yellow-50' :
                  'border-gray-300 bg-gray-50'}`}
              >
                <div className="flex items-start justify-between">
                  <div>
                    <span className="font-medium">{issue.component}</span>
                    <p className="text-sm text-gray-600">{issue.description}</p>
                  </div>
                  {issue.estimatedRepairCost && (
                    <span className="text-sm text-gray-500">
                      ~RD${issue.estimatedRepairCost.toLocaleString()}
                    </span>
                  )}
                </div>
              </div>
            ))}
          </div>

          {result.estimatedRepairCost > 0 && (
            <div className="mt-4 p-3 bg-gray-100 rounded-lg flex justify-between items-center">
              <span className="font-medium">Costo Estimado de Reparaciones</span>
              <span className="text-xl font-bold">
                RD${result.estimatedRepairCost.toLocaleString()}
              </span>
            </div>
          )}
        </div>
      )}

      {/* Categorías Detalladas */}
      <div className="p-6 border-b">
        <h3 className="font-medium mb-4">Detalle por Categoría</h3>
        <div className="space-y-4">
          {result.categories.map((category) => (
            <div key={category.name}>
              <div className="flex items-center justify-between mb-1">
                <span className="font-medium">{category.name}</span>
                <span className="text-sm text-gray-500">
                  {category.scoredPoints}/{category.maxPoints}
                </span>
              </div>
              <div className="h-2 bg-gray-200 rounded-full overflow-hidden">
                <div 
                  className={`h-full ${
                    category.scoredPoints / category.maxPoints >= 0.8 ? 'bg-green-500' :
                    category.scoredPoints / category.maxPoints >= 0.6 ? 'bg-yellow-500' :
                    'bg-red-500'
                  }`}
                  style={{ width: `${(category.scoredPoints / category.maxPoints) * 100}%` }}
                />
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* Recomendaciones */}
      <div className="p-6 bg-blue-50">
        <h3 className="font-medium mb-2 flex items-center gap-2">
          <Info className="w-5 h-5 text-blue-600" />
          Recomendaciones
        </h3>
        <div className="whitespace-pre-line text-gray-700">
          {result.recommendations}
        </div>
      </div>

      {/* Footer */}
      <div className="p-4 bg-gray-50 text-sm text-gray-500 flex justify-between">
        <span>Inspector: {result.inspectorName}</span>
        <span>Cert: {result.inspectorCertification}</span>
        <span>{new Date(result.inspectedAt).toLocaleDateString('es-DO')}</span>
      </div>
    </div>
  );
}
```

---

## 🎯 Controller

```csharp
namespace AuxiliaryService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuxiliaryController : ControllerBase
{
    private readonly IInspectionService _inspectionService;
    private readonly ITowService _towService;
    private readonly IWorkshopService _workshopService;

    // === INSPECCIONES ===

    [HttpPost("inspections")]
    [Authorize]
    public async Task<ActionResult<InspectionRequest>> RequestInspection(
        [FromBody] CreateInspectionRequest request)
    {
        var userId = User.GetUserId();
        var inspection = await _inspectionService.RequestInspectionAsync(
            request with { BuyerId = userId });
        return CreatedAtAction(nameof(GetInspection), new { id = inspection.Id }, inspection);
    }

    [HttpGet("inspections/{id}")]
    public async Task<ActionResult<InspectionRequest>> GetInspection(Guid id)
    {
        var inspection = await _inspectionService.GetByIdAsync(id);
        if (inspection == null) return NotFound();
        return Ok(inspection);
    }

    [HttpGet("inspections/{id}/result")]
    public async Task<ActionResult<InspectionResult>> GetInspectionResult(Guid id)
    {
        var result = await _inspectionService.GetResultAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("inspections/availability")]
    public async Task<ActionResult<List<TimeSlotAvailability>>> GetAvailability(
        [FromQuery] DateTime date,
        [FromQuery] string? location = null)
    {
        var slots = await _inspectionService.GetAvailableSlotsAsync(date, location);
        return Ok(slots);
    }

    // === GRÚAS ===

    [HttpPost("tow")]
    [Authorize]
    public async Task<ActionResult<TowRequest>> RequestTow(
        [FromBody] CreateTowRequest request)
    {
        var userId = User.GetUserId();
        var tow = await _towService.RequestTowAsync(request with { RequesterId = userId });
        return CreatedAtAction(nameof(GetTowRequest), new { id = tow.Id }, tow);
    }

    [HttpGet("tow/{id}")]
    public async Task<ActionResult<TowRequest>> GetTowRequest(Guid id)
    {
        var tow = await _towService.GetByIdAsync(id);
        if (tow == null) return NotFound();
        return Ok(tow);
    }

    [HttpGet("tow/{id}/tracking")]
    public async Task<ActionResult<TowTracking>> GetTowTracking(Guid id)
    {
        var tracking = await _towService.GetTrackingAsync(id);
        if (tracking == null) return NotFound();
        return Ok(tracking);
    }

    [HttpGet("tow/quote")]
    public async Task<ActionResult<List<TowQuote>>> GetTowQuotes(
        [FromQuery] double pickupLat,
        [FromQuery] double pickupLng,
        [FromQuery] double destLat,
        [FromQuery] double destLng,
        [FromQuery] TowType type = TowType.Standard)
    {
        var quotes = await _towService.GetQuotesAsync(
            new Location(pickupLat, pickupLng, "", null),
            new Location(destLat, destLng, "", null),
            type);
        return Ok(quotes);
    }

    // === TALLERES ===

    [HttpGet("workshops")]
    public async Task<ActionResult<List<CertifiedWorkshop>>> GetWorkshops(
        [FromQuery] double? lat,
        [FromQuery] double? lng,
        [FromQuery] int radiusKm = 20,
        [FromQuery] WorkshopSpecialty? specialty = null)
    {
        var workshops = await _workshopService.SearchAsync(
            lat.HasValue ? new GeoCoordinates(lat.Value, lng!.Value) : null,
            radiusKm,
            specialty);
        return Ok(workshops);
    }

    [HttpGet("workshops/{id}")]
    public async Task<ActionResult<CertifiedWorkshop>> GetWorkshop(Guid id)
    {
        var workshop = await _workshopService.GetByIdAsync(id);
        if (workshop == null) return NotFound();
        return Ok(workshop);
    }

    [HttpPost("workshops/{id}/appointments")]
    [Authorize]
    public async Task<ActionResult<WorkshopAppointment>> ScheduleAppointment(
        Guid id,
        [FromBody] CreateAppointmentRequest request)
    {
        var userId = User.GetUserId();
        var appointment = await _workshopService.ScheduleAppointmentAsync(
            id, userId, request);
        return CreatedAtAction(nameof(GetAppointment), new { id = appointment.Id }, appointment);
    }

    [HttpGet("appointments/{id}")]
    public async Task<ActionResult<WorkshopAppointment>> GetAppointment(Guid id)
    {
        var appointment = await _workshopService.GetAppointmentAsync(id);
        if (appointment == null) return NotFound();
        return Ok(appointment);
    }
}
```

---

## ⚙️ Configuración

```json
{
  "AuxiliaryServices": {
    "Inspection": {
      "Providers": {
        "OklaPartner": {
          "Enabled": true,
          "ApiUrl": "https://api.oklapartners.do/inspections",
          "ApiKey": "xxx"
        },
        "AutoCheckRd": {
          "Enabled": true,
          "ApiUrl": "https://api.autocheckrd.com/v1",
          "ApiKey": "xxx"
        }
      },
      "DefaultPrices": {
        "Basic": 2500,
        "Complete": 5000,
        "Premium": 8000
      }
    },
    "Tow": {
      "Providers": {
        "GruasRd": {
          "Enabled": true,
          "ApiUrl": "https://api.gruasrd.com/v1",
          "ApiKey": "xxx"
        }
      },
      "BasePrice": 2000,
      "PricePerKm": 50
    },
    "Workshops": {
      "CommissionPercent": 5,
      "MinimumRatingForListing": 3.5
    }
  }
}
```

---

## 📞 Contactos Partners Potenciales

| Servicio | Empresa | Contacto |
|----------|---------|----------|
| Inspección | INTRANT | inspecciones@intrant.gob.do |
| Inspección | Dekra RD | rd@dekra.com |
| Grúas | Grúas RD | ventas@gruasrd.com |
| Grúas | Asistencia Vial | corporativo@asistenciavial.do |
| Talleres | ADOAUTO | info@adoauto.org.do |

---

**Anterior:** [ONE_ESTADISTICAS_API.md](./ONE_ESTADISTICAS_API.md)  
**Índice:** [README.md](./README.md)
