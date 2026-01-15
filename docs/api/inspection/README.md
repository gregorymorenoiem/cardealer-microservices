# 🔍 Inspection APIs

**Categoría:** Quality Assurance  
**APIs:** 2 (Lemon Squad, Certify My Ride)  
**Fase:** 2-3 (Trust & Safety)  
**Impacto:** +40% confianza del comprador cuando vehículo tiene inspección certificada

---

## 📖 Resumen

Servicios de inspección mecánica profesional para vehículos usados. Compradores pueden solicitar inspección antes de comprar, vendedores pueden incluir certificación para aumentar confianza.

### Casos de Uso en OKLA

✅ **Inspección pre-compra** - Comprador paga $100, inspector revisa vehículo in-situ  
✅ **Inspección para vendedor** - Vendedor certifica su vehículo para vender más rápido  
✅ **Badge "OKLA Certified"** - Vehículos inspeccionados se destacan en búsqueda  
✅ **Reporte detallado** - 150+ puntos de inspección con fotos y video  
✅ **Garantía de condición** - Si inspector falla algo, OKLA cubre hasta $1,000  
✅ **Inspección virtual** - Videollamada donde inspector guía al vendedor remotamente

---

## 🔗 Comparativa de APIs

| Aspecto               | **Lemon Squad**    | **Certify My Ride**  |
| --------------------- | ------------------ | -------------------- |
| **Precio base**       | $135-200           | $80-150              |
| **Puntos inspección** | 186                | 150                  |
| **Tiempo reporte**    | 24 horas           | 48 horas             |
| **Fotos incluidas**   | 30+                | 20+                  |
| **Video incluido**    | ✅ 5 minutos       | ❌                   |
| **Cobertura USA**     | ✅ Nacional        | ✅ Nacional          |
| **Cobertura RD**      | ⚠️ Limitada        | ⚠️ Limitada          |
| **API disponible**    | ✅ REST            | ⚠️ Webhook           |
| **Mejor para**        | Compradores serios | Inspecciones básicas |
| **Recomendado**       | ⭐ PRINCIPAL       | Backup               |

---

## 📡 ENDPOINTS

### Lemon Squad API

- `POST /api/inspections` - Solicitar inspección
- `GET /api/inspections/{id}` - Estado de inspección
- `GET /api/inspections/{id}/report` - Reporte completo
- `GET /api/inspections/{id}/photos` - Fotos de la inspección
- `POST /api/inspections/{id}/reschedule` - Reprogramar
- `DELETE /api/inspections/{id}` - Cancelar

### Certify My Ride API

- `POST /api/bookings` - Agendar inspección
- `GET /api/bookings/{id}` - Estado de reserva
- `GET /api/reports/{vin}` - Reporte por VIN
- Webhook: `inspection.completed` - Notifica cuando termina

---

## 💻 Backend Implementation (C#)

### Service Interface

```csharp
public interface IInspectionService
{
    Task<InspectionRequest> RequestInspectionAsync(InspectionRequestDto request);
    Task<InspectionStatus> GetStatusAsync(string inspectionId);
    Task<InspectionReport> GetReportAsync(string inspectionId);
    Task<InspectionPhoto[]> GetPhotosAsync(string inspectionId);
    Task RescheduleAsync(string inspectionId, DateTime newDate);
    Task CancelAsync(string inspectionId);
}

public class InspectionRequestDto
{
    public string VehicleId { get; set; }
    public string Vin { get; set; }
    public string Make { get; set; }
    public string Model { get; set; }
    public int Year { get; set; }
    public InspectionType Type { get; set; }
    public InspectionLocation Location { get; set; }
    public DateTime PreferredDate { get; set; }
    public TimeSlot PreferredTime { get; set; }
    public string ContactName { get; set; }
    public string ContactPhone { get; set; }
    public string Notes { get; set; }
}

public enum InspectionType
{
    Basic,          // 50 puntos - $80
    Standard,       // 100 puntos - $120
    Comprehensive,  // 150+ puntos - $180
    PrePurchase     // 186 puntos + video - $200
}

public class InspectionLocation
{
    public string Address { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string ZipCode { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}

public enum TimeSlot
{
    Morning,    // 8am - 12pm
    Afternoon,  // 12pm - 5pm
    Evening     // 5pm - 8pm
}
```

### Domain Models

```csharp
public class InspectionRequest
{
    public string InspectionId { get; set; }
    public string VehicleId { get; set; }
    public string Vin { get; set; }
    public InspectionType Type { get; set; }
    public InspectionRequestStatus Status { get; set; }
    public DateTime ScheduledDate { get; set; }
    public string InspectorName { get; set; }
    public string InspectorPhone { get; set; }
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }
}

public enum InspectionRequestStatus
{
    Pending,
    Confirmed,
    InProgress,
    Completed,
    Cancelled,
    Rescheduled
}

public class InspectionReport
{
    public string ReportId { get; set; }
    public string InspectionId { get; set; }
    public string Vin { get; set; }
    public DateTime InspectedAt { get; set; }
    public string InspectorName { get; set; }
    public OverallGrade OverallGrade { get; set; } // A, B, C, D, F
    public int Score { get; set; } // 0-100
    public CategoryReport[] Categories { get; set; }
    public InspectionIssue[] Issues { get; set; }
    public string Summary { get; set; }
    public string RecommendedActions { get; set; }
    public decimal EstimatedRepairCost { get; set; }
    public string PdfUrl { get; set; }
    public string VideoUrl { get; set; }
}

public enum OverallGrade
{
    A,  // 90-100: Excelente condición
    B,  // 80-89: Buena condición
    C,  // 70-79: Condición aceptable
    D,  // 60-69: Necesita reparaciones
    F   // <60: No recomendado
}

public class CategoryReport
{
    public string CategoryName { get; set; } // "Engine", "Transmission", "Body"
    public string Grade { get; set; }
    public int Score { get; set; }
    public CheckItem[] Items { get; set; }
}

public class CheckItem
{
    public string Name { get; set; }        // "Oil Level"
    public CheckResult Result { get; set; } // Pass, Fail, Warning
    public string Notes { get; set; }
    public string PhotoUrl { get; set; }
}

public enum CheckResult
{
    Pass,
    Fail,
    Warning,
    NotApplicable
}

public class InspectionIssue
{
    public IssueSeverity Severity { get; set; }
    public string Category { get; set; }
    public string Description { get; set; }
    public decimal EstimatedCost { get; set; }
    public string PhotoUrl { get; set; }
}

public enum IssueSeverity
{
    Critical,   // Seguridad comprometida
    Major,      // Reparación necesaria pronto
    Minor,      // Cosmético o menor
    Informational
}

public class InspectionPhoto
{
    public string PhotoId { get; set; }
    public string Url { get; set; }
    public string ThumbnailUrl { get; set; }
    public string Category { get; set; }
    public string Caption { get; set; }
}
```

### Service Implementation

```csharp
public class LemonSquadInspectionService : IInspectionService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<LemonSquadInspectionService> _logger;
    private readonly string _apiKey;
    private const string BaseUrl = "https://api.lemonsquad.com/v1";

    public LemonSquadInspectionService(HttpClient httpClient, IConfiguration config, ILogger<LemonSquadInspectionService> logger)
    {
        _httpClient = httpClient;
        _config = config;
        _logger = logger;
        _apiKey = config["LemonSquad:ApiKey"];
    }

    public async Task<InspectionRequest> RequestInspectionAsync(InspectionRequestDto request)
    {
        try
        {
            var payload = new
            {
                vin = request.Vin,
                make = request.Make,
                model = request.Model,
                year = request.Year,
                inspection_type = MapInspectionType(request.Type),
                location = new
                {
                    address = request.Location.Address,
                    city = request.Location.City,
                    state = request.Location.State,
                    zip = request.Location.ZipCode
                },
                preferred_date = request.PreferredDate.ToString("yyyy-MM-dd"),
                time_slot = request.PreferredTime.ToString().ToLower(),
                contact = new
                {
                    name = request.ContactName,
                    phone = request.ContactPhone
                },
                notes = request.Notes
            };

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/inspections");
            httpRequest.Headers.Add("Authorization", $"Bearer {_apiKey}");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(httpRequest);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            return new InspectionRequest
            {
                InspectionId = root.GetProperty("inspection_id").GetString(),
                VehicleId = request.VehicleId,
                Vin = request.Vin,
                Type = request.Type,
                Status = InspectionRequestStatus.Pending,
                ScheduledDate = DateTime.Parse(root.GetProperty("scheduled_date").GetString()),
                InspectorName = root.TryGetProperty("inspector_name", out var inspector) ? inspector.GetString() : null,
                Price = root.GetProperty("price").GetDecimal(),
                CreatedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting inspection from Lemon Squad");
            throw;
        }
    }

    public async Task<InspectionStatus> GetStatusAsync(string inspectionId)
    {
        var response = await _httpClient.GetAsync($"{BaseUrl}/inspections/{inspectionId}");
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        var statusStr = doc.RootElement.GetProperty("status").GetString();
        return Enum.Parse<InspectionRequestStatus>(statusStr, true);
    }

    public async Task<InspectionReport> GetReportAsync(string inspectionId)
    {
        var response = await _httpClient.GetAsync($"{BaseUrl}/inspections/{inspectionId}/report");
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        return new InspectionReport
        {
            ReportId = root.GetProperty("report_id").GetString(),
            InspectionId = inspectionId,
            Vin = root.GetProperty("vin").GetString(),
            InspectedAt = DateTime.Parse(root.GetProperty("inspected_at").GetString()),
            InspectorName = root.GetProperty("inspector_name").GetString(),
            OverallGrade = Enum.Parse<OverallGrade>(root.GetProperty("overall_grade").GetString()),
            Score = root.GetProperty("score").GetInt32(),
            Summary = root.GetProperty("summary").GetString(),
            EstimatedRepairCost = root.GetProperty("estimated_repair_cost").GetDecimal(),
            PdfUrl = root.GetProperty("pdf_url").GetString(),
            VideoUrl = root.TryGetProperty("video_url", out var video) ? video.GetString() : null,
            Categories = ParseCategories(root.GetProperty("categories")),
            Issues = ParseIssues(root.GetProperty("issues"))
        };
    }

    public async Task<InspectionPhoto[]> GetPhotosAsync(string inspectionId)
    {
        var response = await _httpClient.GetAsync($"{BaseUrl}/inspections/{inspectionId}/photos");
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<InspectionPhoto[]>(json);
    }

    private string MapInspectionType(InspectionType type) => type switch
    {
        InspectionType.Basic => "basic",
        InspectionType.Standard => "standard",
        InspectionType.Comprehensive => "comprehensive",
        InspectionType.PrePurchase => "pre_purchase",
        _ => "standard"
    };

    private CategoryReport[] ParseCategories(JsonElement element)
    {
        var categories = new List<CategoryReport>();
        foreach (var item in element.EnumerateArray())
        {
            categories.Add(new CategoryReport
            {
                CategoryName = item.GetProperty("name").GetString(),
                Grade = item.GetProperty("grade").GetString(),
                Score = item.GetProperty("score").GetInt32()
            });
        }
        return categories.ToArray();
    }

    private InspectionIssue[] ParseIssues(JsonElement element)
    {
        var issues = new List<InspectionIssue>();
        foreach (var item in element.EnumerateArray())
        {
            issues.Add(new InspectionIssue
            {
                Severity = Enum.Parse<IssueSeverity>(item.GetProperty("severity").GetString(), true),
                Category = item.GetProperty("category").GetString(),
                Description = item.GetProperty("description").GetString(),
                EstimatedCost = item.GetProperty("estimated_cost").GetDecimal(),
                PhotoUrl = item.TryGetProperty("photo_url", out var photo) ? photo.GetString() : null
            });
        }
        return issues.ToArray();
    }
}
```

---

## 🎨 Frontend Implementation (React + TypeScript)

### Inspection Service

```typescript
import axios from "axios";

export interface InspectionRequest {
  inspectionId: string;
  status: string;
  scheduledDate: string;
  inspectorName?: string;
  price: number;
}

export interface InspectionReport {
  reportId: string;
  overallGrade: "A" | "B" | "C" | "D" | "F";
  score: number;
  summary: string;
  estimatedRepairCost: number;
  pdfUrl: string;
  videoUrl?: string;
  categories: CategoryReport[];
  issues: InspectionIssue[];
}

export class InspectionService {
  private baseUrl = process.env.REACT_APP_API_URL;

  async requestInspection(
    data: InspectionRequestData
  ): Promise<InspectionRequest> {
    const response = await axios.post(`${this.baseUrl}/api/inspections`, data);
    return response.data;
  }

  async getStatus(inspectionId: string): Promise<string> {
    const response = await axios.get(
      `${this.baseUrl}/api/inspections/${inspectionId}`
    );
    return response.data.status;
  }

  async getReport(inspectionId: string): Promise<InspectionReport> {
    const response = await axios.get(
      `${this.baseUrl}/api/inspections/${inspectionId}/report`
    );
    return response.data;
  }

  async getPhotos(inspectionId: string): Promise<InspectionPhoto[]> {
    const response = await axios.get(
      `${this.baseUrl}/api/inspections/${inspectionId}/photos`
    );
    return response.data;
  }
}
```

### React Component - Request Inspection

```typescript
import React, { useState } from "react";
import { useMutation } from "@tanstack/react-query";
import { InspectionService } from "@/services/inspectionService";
import { Calendar, Clock, MapPin, CheckCircle2, XCircle } from "lucide-react";

interface Props {
  vehicle: {
    id: string;
    vin: string;
    make: string;
    model: string;
    year: number;
  };
}

export const RequestInspectionModal = ({ vehicle }: Props) => {
  const [date, setDate] = useState("");
  const [timeSlot, setTimeSlot] = useState<"morning" | "afternoon" | "evening">(
    "morning"
  );
  const [location, setLocation] = useState({
    address: "",
    city: "",
    state: "",
    zipCode: "",
  });

  const inspectionService = new InspectionService();

  const { mutate, isLoading, isSuccess, data } = useMutation({
    mutationFn: () =>
      inspectionService.requestInspection({
        vehicleId: vehicle.id,
        vin: vehicle.vin,
        make: vehicle.make,
        model: vehicle.model,
        year: vehicle.year,
        type: "pre_purchase",
        preferredDate: date,
        preferredTime: timeSlot,
        location,
      }),
  });

  if (isSuccess) {
    return (
      <div className="text-center py-8">
        <CheckCircle2 className="h-16 w-16 text-green-500 mx-auto mb-4" />
        <h3 className="text-xl font-bold text-green-700">
          ¡Inspección Programada!
        </h3>
        <p className="text-gray-600 mt-2">
          ID: {data.inspectionId}
          <br />
          Fecha: {new Date(data.scheduledDate).toLocaleDateString("es-DO")}
        </p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <h3 className="text-xl font-bold">🔍 Solicitar Inspección Pre-Compra</h3>

      <div className="bg-blue-50 p-4 rounded-lg">
        <p className="text-sm text-blue-700">
          <strong>Inspección incluye:</strong> 186 puntos de revisión, 30+
          fotos, video de 5 min, reporte PDF detallado
        </p>
      </div>

      {/* Date Selection */}
      <div>
        <label className="block text-sm font-medium mb-2">
          <Calendar className="h-4 w-4 inline mr-2" />
          Fecha preferida
        </label>
        <input
          type="date"
          min={new Date().toISOString().split("T")[0]}
          value={date}
          onChange={(e) => setDate(e.target.value)}
          className="w-full border rounded-lg p-3"
        />
      </div>

      {/* Time Slot */}
      <div>
        <label className="block text-sm font-medium mb-2">
          <Clock className="h-4 w-4 inline mr-2" />
          Horario
        </label>
        <div className="grid grid-cols-3 gap-2">
          {(["morning", "afternoon", "evening"] as const).map((slot) => (
            <button
              key={slot}
              onClick={() => setTimeSlot(slot)}
              className={`py-2 px-4 rounded-lg text-sm ${
                timeSlot === slot ? "bg-blue-600 text-white" : "bg-gray-100"
              }`}
            >
              {slot === "morning"
                ? "Mañana (8-12)"
                : slot === "afternoon"
                ? "Tarde (12-5)"
                : "Noche (5-8)"}
            </button>
          ))}
        </div>
      </div>

      {/* Location */}
      <div>
        <label className="block text-sm font-medium mb-2">
          <MapPin className="h-4 w-4 inline mr-2" />
          Ubicación del vehículo
        </label>
        <input
          type="text"
          placeholder="Dirección"
          value={location.address}
          onChange={(e) =>
            setLocation((l) => ({ ...l, address: e.target.value }))
          }
          className="w-full border rounded-lg p-3 mb-2"
        />
        <div className="grid grid-cols-3 gap-2">
          <input
            type="text"
            placeholder="Ciudad"
            value={location.city}
            onChange={(e) =>
              setLocation((l) => ({ ...l, city: e.target.value }))
            }
            className="border rounded-lg p-3"
          />
          <input
            type="text"
            placeholder="Estado"
            value={location.state}
            onChange={(e) =>
              setLocation((l) => ({ ...l, state: e.target.value }))
            }
            className="border rounded-lg p-3"
          />
          <input
            type="text"
            placeholder="ZIP"
            value={location.zipCode}
            onChange={(e) =>
              setLocation((l) => ({ ...l, zipCode: e.target.value }))
            }
            className="border rounded-lg p-3"
          />
        </div>
      </div>

      {/* Price */}
      <div className="border-t pt-4">
        <div className="flex justify-between text-lg">
          <span>Inspección Pre-Compra (186 puntos)</span>
          <span className="font-bold">$200 USD</span>
        </div>
      </div>

      <button
        onClick={() => mutate()}
        disabled={isLoading || !date || !location.address}
        className="w-full py-3 bg-green-600 text-white rounded-lg font-semibold hover:bg-green-700 disabled:bg-gray-400"
      >
        {isLoading ? "Procesando..." : "Programar Inspección"}
      </button>
    </div>
  );
};
```

### React Component - Inspection Report Viewer

```typescript
import React from "react";
import { useQuery } from "@tanstack/react-query";
import { InspectionService } from "@/services/inspectionService";

interface Props {
  inspectionId: string;
}

export const InspectionReportViewer = ({ inspectionId }: Props) => {
  const inspectionService = new InspectionService();

  const { data: report, isLoading } = useQuery({
    queryKey: ["inspection-report", inspectionId],
    queryFn: () => inspectionService.getReport(inspectionId),
  });

  const gradeColors = {
    A: "bg-green-500",
    B: "bg-green-400",
    C: "bg-yellow-400",
    D: "bg-orange-500",
    F: "bg-red-500",
  };

  if (isLoading)
    return <div className="animate-pulse h-96 bg-gray-200 rounded-xl"></div>;
  if (!report) return null;

  return (
    <div className="space-y-6">
      {/* Overall Grade */}
      <div className="flex items-center gap-6 p-6 bg-white rounded-xl shadow-sm">
        <div
          className={`w-20 h-20 rounded-full ${
            gradeColors[report.overallGrade]
          } flex items-center justify-center`}
        >
          <span className="text-4xl font-bold text-white">
            {report.overallGrade}
          </span>
        </div>
        <div>
          <h3 className="text-2xl font-bold">Puntuación: {report.score}/100</h3>
          <p className="text-gray-600">{report.summary}</p>
        </div>
      </div>

      {/* Categories */}
      <div className="grid grid-cols-2 md:grid-cols-3 gap-4">
        {report.categories.map((cat) => (
          <div key={cat.categoryName} className="p-4 border rounded-lg">
            <div className="flex justify-between items-center">
              <span className="font-medium">{cat.categoryName}</span>
              <span
                className={`w-8 h-8 rounded-full ${
                  gradeColors[cat.grade as keyof typeof gradeColors]
                } flex items-center justify-center text-white text-sm font-bold`}
              >
                {cat.grade}
              </span>
            </div>
            <div className="mt-2 w-full bg-gray-200 rounded-full h-2">
              <div
                className="bg-blue-600 h-2 rounded-full"
                style={{ width: `${cat.score}%` }}
              ></div>
            </div>
          </div>
        ))}
      </div>

      {/* Issues */}
      {report.issues.length > 0 && (
        <div className="p-4 border border-orange-200 bg-orange-50 rounded-xl">
          <h4 className="font-bold text-orange-800 mb-3">
            Problemas Detectados
          </h4>
          {report.issues.map((issue, idx) => (
            <div
              key={idx}
              className="flex justify-between py-2 border-b border-orange-200 last:border-0"
            >
              <div>
                <span
                  className={`text-xs px-2 py-1 rounded ${
                    issue.severity === "Critical"
                      ? "bg-red-200 text-red-800"
                      : issue.severity === "Major"
                      ? "bg-orange-200 text-orange-800"
                      : "bg-yellow-200 text-yellow-800"
                  }`}
                >
                  {issue.severity}
                </span>
                <p className="mt-1">{issue.description}</p>
              </div>
              <span className="font-semibold">${issue.estimatedCost}</span>
            </div>
          ))}
          <div className="mt-4 pt-4 border-t border-orange-300 flex justify-between font-bold">
            <span>Costo estimado de reparaciones:</span>
            <span>${report.estimatedRepairCost.toLocaleString()}</span>
          </div>
        </div>
      )}

      {/* Actions */}
      <div className="flex gap-4">
        <a
          href={report.pdfUrl}
          target="_blank"
          className="flex-1 py-3 text-center bg-blue-600 text-white rounded-lg font-semibold"
        >
          📄 Descargar PDF
        </a>
        {report.videoUrl && (
          <a
            href={report.videoUrl}
            target="_blank"
            className="flex-1 py-3 text-center bg-gray-800 text-white rounded-lg font-semibold"
          >
            🎥 Ver Video
          </a>
        )}
      </div>
    </div>
  );
};
```

---

## ✅ Testing (xUnit)

```csharp
public class LemonSquadInspectionServiceTests
{
    [Fact]
    public async Task RequestInspectionAsync_WithValidData_ReturnsInspection()
    {
        var request = new InspectionRequestDto
        {
            Vin = "1HGBH41JXMN109186",
            Make = "Toyota",
            Model = "Corolla",
            Year = 2020,
            Type = InspectionType.PrePurchase,
            PreferredDate = DateTime.Today.AddDays(3)
        };

        var result = await _service.RequestInspectionAsync(request);

        Assert.NotNull(result);
        Assert.NotEmpty(result.InspectionId);
        Assert.Equal(InspectionRequestStatus.Pending, result.Status);
    }

    [Fact]
    public async Task GetReportAsync_WithCompletedInspection_ReturnsFullReport()
    {
        var inspectionId = "completed-inspection-id";

        var report = await _service.GetReportAsync(inspectionId);

        Assert.NotNull(report);
        Assert.NotEmpty(report.ReportId);
        Assert.True(report.Score >= 0 && report.Score <= 100);
        Assert.NotEmpty(report.Categories);
    }

    [Fact]
    public async Task GetPhotosAsync_ReturnsPhotos()
    {
        var inspectionId = "inspection-with-photos";

        var photos = await _service.GetPhotosAsync(inspectionId);

        Assert.NotEmpty(photos);
        Assert.All(photos, p => Assert.NotEmpty(p.Url));
    }
}
```

---

## 🔧 Troubleshooting

| Problema                   | Causa                     | Solución                                     |
| -------------------------- | ------------------------- | -------------------------------------------- |
| No hay inspectores en área | Ubicación remota          | Ofrecer inspección virtual o expandir radio  |
| Inspección cancelada       | Vehículo no disponible    | Permitir reprogramar sin cargo extra         |
| Reporte no disponible      | Aún procesando            | Polling cada 30 segundos hasta ready         |
| Fotos no cargan            | CDN lento o timeout       | Usar thumbnails primero, lazy load full size |
| Precio diferente           | Tipo de vehículo especial | Mostrar estimado inicial + precio final      |
| Inspector no llegó         | Error de coordinación     | Soporte 24/7 + re-agendamiento automático    |

---

## 🔗 Integración con OKLA

### 1. **Crear InspectionService**

```csharp
services.AddHttpClient<IInspectionService, LemonSquadInspectionService>();
```

### 2. **Gateway routing**

```json
{
  "UpstreamPathTemplate": "/api/inspections/{everything}",
  "DownstreamPathTemplate": "/api/inspections/{everything}",
  "DownstreamHostAndPorts": [{ "Host": "inspectionservice", "Port": 8080 }]
}
```

### 3. **Botón en VehicleDetailPage**

```tsx
<Button onClick={() => setShowInspectionModal(true)}>
  🔍 Solicitar Inspección
</Button>
```

### 4. **Badge "OKLA Certified"**

```tsx
{
  vehicle.hasInspection && vehicle.inspectionGrade >= "B" && (
    <Badge className="bg-green-600">✓ OKLA Certified</Badge>
  );
}
```

### 5. **Webhook para actualizar estado**

```csharp
[HttpPost("webhook/lemon-squad")]
public async Task<IActionResult> LemonSquadWebhook([FromBody] LemonSquadEvent ev)
{
    await _mediator.Send(new UpdateInspectionStatusCommand(ev.InspectionId, ev.Status));
    return Ok();
}
```

---

## 💰 Costos Estimados

| Tipo             | Costo Lemon Squad | Costo OKLA (markup) | Ganancia   |
| ---------------- | ----------------- | ------------------- | ---------- |
| Básica           | $100              | $120                | $20        |
| Standard         | $135              | $160                | $25        |
| Comprehensive    | $180              | $210                | $30        |
| Pre-Purchase     | $200              | $230                | $30        |
| **100 insp/mes** | $15,000           | $18,500             | **$3,500** |

✅ **Revenue adicional:** ~$3,500/mes con 100 inspecciones. Más si se incluye como requisito para "OKLA Certified".
