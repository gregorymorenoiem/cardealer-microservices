# 📸 Photography 3D APIs

**Categoría:** Visual Content & Media  
**APIs:** 4 (Spyne.ai, Spectrum, PhotoUp, AutoUncle)  
**Fase:** 2 (Diferenciación)  
**Impacto:** Listados con fotos 360° tienen 3x más vistas y 2x más contactos

---

## 📖 Resumen

Transformación automática de fotos de vehículos, tours virtuales 360°, detección de daños con IA, y mejora de calidad de imágenes. Estas APIs permiten que dealers sin equipo profesional puedan tener listados con calidad premium.

### Casos de Uso en OKLA

✅ **Fotos 360° automáticas** - Dealer toma 10-15 fotos con celular, API genera tour virtual interactivo  
✅ **Mejora automática de calidad** - Elimina fondos, corrige iluminación, ajusta colores  
✅ **Detección de daños con IA** - Sistema detecta rayones, abolladuras, óxido en fotos  
✅ **Badge "Tour Virtual Disponible"** - Listados con 360° se destacan (65% más engagement)  
✅ **Reportes visuales de condición** - Informe automático de estado del vehículo basado en fotos  
✅ **Integración con historial** - Fotos se guardan como evidencia de condición al momento de venta

---

## 🔗 Comparativa de APIs

| Aspecto               | **Spyne.ai**         | **Spectrum**           | **PhotoUp**      | **AutoUncle**    |
| --------------------- | -------------------- | ---------------------- | ---------------- | ---------------- |
| **Costo**             | $50-500/mes          | $100-1000/mes          | $30-300/mes      | $20-200/mes      |
| **360° Tours**        | ✅ Automático        | ✅ Manual + Auto       | ⚠️ Limitado      | ❌ No            |
| **Background Remove** | ✅ AI-powered        | ✅ Premium             | ✅ Básico        | ✅ Básico        |
| **Damage Detection**  | ✅ Avanzado          | ⚠️ Básico              | ❌ No            | ❌ No            |
| **Calidad Output**    | 4K                   | 8K                     | 2K               | 2K               |
| **Velocidad**         | 30-60 segundos       | 2-5 minutos            | 1-2 minutos      | 1-2 minutos      |
| **API REST**          | ✅ Completa          | ✅ Completa            | ⚠️ Limitada      | ⚠️ Limitada      |
| **Mejor para**        | Dealers alto volumen | Concesionarios premium | Dealers pequeños | Presupuesto bajo |
| **Recomendado**       | ⭐ PRINCIPAL         | ⭐ Premium             | ⭐ Starter       | Backup           |

---

## 📡 ENDPOINTS

### Spyne.ai API

- `POST /api/v1/images/upload` - Subir imágenes (max 50 por batch)
- `POST /api/v1/images/process` - Procesar y mejorar imágenes
- `POST /api/v1/tours/create` - Generar tour 360°
- `GET /api/v1/tours/{tourId}` - Obtener tour generado
- `POST /api/v1/damage/detect` - Detectar daños con IA
- `GET /api/v1/damage/{reportId}` - Obtener reporte de daños
- `DELETE /api/v1/images/{imageId}` - Eliminar imagen

### Spectrum API

- `POST /images/upload` - Subir imágenes alta resolución
- `POST /images/enhance` - Mejora de calidad premium
- `POST /tours/generate` - Generar tour 360° manual
- `GET /tours/{id}/embed` - Código embed para web

### PhotoUp API

- `POST /process` - Procesar imagen (background removal)
- `POST /enhance` - Mejora de iluminación
- `GET /download/{jobId}` - Descargar imagen procesada

---

## 💻 Backend Implementation (C#)

### Service Interface

```csharp
public interface IPhotoProcessingService
{
    Task<ProcessedImage[]> ProcessImagesAsync(List<byte[]> images, ProcessingOptions options);
    Task<Tour360> Generate360TourAsync(string vehicleId, List<string> imageUrls);
    Task<DamageReport> DetectDamageAsync(List<string> imageUrls);
    Task<string> RemoveBackgroundAsync(byte[] image);
    Task<EnhancedImage> EnhanceImageAsync(byte[] image, EnhanceOptions options);
}

public class ProcessingOptions
{
    public bool RemoveBackground { get; set; } = true;
    public bool EnhanceLighting { get; set; } = true;
    public bool CorrectColors { get; set; } = true;
    public string OutputFormat { get; set; } = "jpg"; // jpg, png, webp
    public int Quality { get; set; } = 90;
}
```

### Domain Models

```csharp
public class ProcessedImage
{
    public string Id { get; set; }
    public string OriginalUrl { get; set; }
    public string ProcessedUrl { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public long FileSizeBytes { get; set; }
    public DateTime ProcessedAt { get; set; }
}

public class Tour360
{
    public string TourId { get; set; }
    public string VehicleId { get; set; }
    public string EmbedUrl { get; set; }
    public string ThumbnailUrl { get; set; }
    public int ImageCount { get; set; }
    public string[] Hotspots { get; set; }
    public DateTime CreatedAt { get; set; }
    public TourStatus Status { get; set; }
}

public enum TourStatus { Processing, Ready, Failed }

public class DamageReport
{
    public string ReportId { get; set; }
    public string VehicleId { get; set; }
    public DamageItem[] DetectedDamages { get; set; }
    public string OverallCondition { get; set; } // "Excellent", "Good", "Fair", "Poor"
    public decimal ConditionScore { get; set; } // 0-100
    public DateTime AnalyzedAt { get; set; }
}

public class DamageItem
{
    public string Type { get; set; } // "Scratch", "Dent", "Rust", "Crack"
    public string Location { get; set; } // "Front Bumper", "Driver Door"
    public string Severity { get; set; } // "Minor", "Moderate", "Severe"
    public decimal EstimatedRepairCost { get; set; }
    public string ImageUrl { get; set; }
}
```

### Service Implementation

```csharp
public class SpynePhotoService : IPhotoProcessingService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<SpynePhotoService> _logger;
    private readonly string _apiKey;
    private const string BaseUrl = "https://api.spyne.ai/api/v1";

    public SpynePhotoService(HttpClient httpClient, IConfiguration config, ILogger<SpynePhotoService> logger)
    {
        _httpClient = httpClient;
        _config = config;
        _logger = logger;
        _apiKey = config["Spyne:ApiKey"];
    }

    public async Task<ProcessedImage[]> ProcessImagesAsync(List<byte[]> images, ProcessingOptions options)
    {
        try
        {
            var content = new MultipartFormDataContent();

            for (int i = 0; i < images.Count; i++)
            {
                content.Add(new ByteArrayContent(images[i]), "images", $"image_{i}.jpg");
            }

            content.Add(new StringContent(options.RemoveBackground.ToString()), "remove_background");
            content.Add(new StringContent(options.EnhanceLighting.ToString()), "enhance_lighting");
            content.Add(new StringContent(options.Quality.ToString()), "quality");

            var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/images/process");
            request.Headers.Add("X-API-Key", _apiKey);
            request.Content = content;

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Spyne API error: {response.StatusCode}");

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            return doc.RootElement.GetProperty("images")
                .EnumerateArray()
                .Select(img => new ProcessedImage
                {
                    Id = img.GetProperty("id").GetString(),
                    OriginalUrl = img.GetProperty("original_url").GetString(),
                    ProcessedUrl = img.GetProperty("processed_url").GetString(),
                    Width = img.GetProperty("width").GetInt32(),
                    Height = img.GetProperty("height").GetInt32(),
                    ProcessedAt = DateTime.UtcNow
                })
                .ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing {count} images with Spyne", images.Count);
            throw;
        }
    }

    public async Task<Tour360> Generate360TourAsync(string vehicleId, List<string> imageUrls)
    {
        var payload = new { vehicle_id = vehicleId, image_urls = imageUrls, auto_hotspots = true };

        var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/tours/create");
        request.Headers.Add("X-API-Key", _apiKey);
        request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        return new Tour360
        {
            TourId = doc.RootElement.GetProperty("tour_id").GetString(),
            VehicleId = vehicleId,
            EmbedUrl = doc.RootElement.GetProperty("embed_url").GetString(),
            ThumbnailUrl = doc.RootElement.GetProperty("thumbnail_url").GetString(),
            ImageCount = imageUrls.Count,
            Status = TourStatus.Processing,
            CreatedAt = DateTime.UtcNow
        };
    }

    public async Task<DamageReport> DetectDamageAsync(List<string> imageUrls)
    {
        var payload = new { image_urls = imageUrls, detection_mode = "comprehensive" };

        var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/damage/detect");
        request.Headers.Add("X-API-Key", _apiKey);
        request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var damages = root.GetProperty("damages")
            .EnumerateArray()
            .Select(d => new DamageItem
            {
                Type = d.GetProperty("type").GetString(),
                Location = d.GetProperty("location").GetString(),
                Severity = d.GetProperty("severity").GetString(),
                EstimatedRepairCost = d.GetProperty("repair_cost").GetDecimal(),
                ImageUrl = d.GetProperty("annotated_image").GetString()
            })
            .ToArray();

        return new DamageReport
        {
            ReportId = root.GetProperty("report_id").GetString(),
            DetectedDamages = damages,
            OverallCondition = root.GetProperty("overall_condition").GetString(),
            ConditionScore = root.GetProperty("condition_score").GetDecimal(),
            AnalyzedAt = DateTime.UtcNow
        };
    }

    public async Task<string> RemoveBackgroundAsync(byte[] image)
    {
        var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(image), "image", "image.jpg");

        var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/images/remove-background");
        request.Headers.Add("X-API-Key", _apiKey);
        request.Content = content;

        var response = await _httpClient.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        return doc.RootElement.GetProperty("processed_url").GetString();
    }

    public async Task<EnhancedImage> EnhanceImageAsync(byte[] image, EnhanceOptions options)
    {
        var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(image), "image", "image.jpg");

        var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/images/enhance");
        request.Headers.Add("X-API-Key", _apiKey);
        request.Content = content;

        var response = await _httpClient.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        return new EnhancedImage
        {
            Url = doc.RootElement.GetProperty("enhanced_url").GetString(),
            OriginalSize = doc.RootElement.GetProperty("original_size").GetInt64(),
            EnhancedSize = doc.RootElement.GetProperty("enhanced_size").GetInt64()
        };
    }
}
```

### CQRS Commands

```csharp
public class ProcessVehicleImagesCommand : IRequest<Result<ProcessedImage[]>>
{
    public string VehicleId { get; set; }
    public List<IFormFile> Images { get; set; }
    public bool GenerateTour { get; set; } = true;
    public bool DetectDamage { get; set; } = false;
}

public class ProcessVehicleImagesCommandHandler
    : IRequestHandler<ProcessVehicleImagesCommand, Result<ProcessedImage[]>>
{
    private readonly IPhotoProcessingService _photoService;
    private readonly IMediaRepository _mediaRepo;
    private readonly IBus _bus;

    public async Task<Result<ProcessedImage[]>> Handle(ProcessVehicleImagesCommand request, CancellationToken ct)
    {
        var imageBytes = new List<byte[]>();
        foreach (var file in request.Images)
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms, ct);
            imageBytes.Add(ms.ToArray());
        }

        var options = new ProcessingOptions { RemoveBackground = true, EnhanceLighting = true, Quality = 90 };
        var processed = await _photoService.ProcessImagesAsync(imageBytes, options);

        foreach (var img in processed)
            await _mediaRepo.SaveImageAsync(request.VehicleId, img);

        if (request.GenerateTour && processed.Length >= 8)
        {
            var imageUrls = processed.Select(p => p.ProcessedUrl).ToList();
            var tour = await _photoService.Generate360TourAsync(request.VehicleId, imageUrls);
            await _mediaRepo.SaveTourAsync(request.VehicleId, tour);
        }

        await _bus.Publish(new VehicleImagesProcessedEvent { VehicleId = request.VehicleId, ImageCount = processed.Length });
        return Result<ProcessedImage[]>.Success(processed);
    }
}
```

---

## 🎨 Frontend Implementation (React + TypeScript)

### Photo Service

```typescript
import axios from "axios";

export interface ProcessedImage {
  id: string;
  originalUrl: string;
  processedUrl: string;
  width: number;
  height: number;
}

export interface Tour360 {
  tourId: string;
  embedUrl: string;
  thumbnailUrl: string;
  imageCount: number;
  status: "Processing" | "Ready" | "Failed";
}

export class PhotoService {
  private baseUrl = process.env.REACT_APP_API_URL;

  async uploadAndProcess(
    vehicleId: string,
    files: File[]
  ): Promise<ProcessedImage[]> {
    const formData = new FormData();
    files.forEach((file) => formData.append("images", file));
    formData.append("vehicleId", vehicleId);
    formData.append("generateTour", "true");

    const response = await axios.post(
      `${this.baseUrl}/api/photos/process`,
      formData,
      {
        headers: { "Content-Type": "multipart/form-data" },
      }
    );
    return response.data;
  }

  async get360Tour(vehicleId: string): Promise<Tour360> {
    const response = await axios.get(
      `${this.baseUrl}/api/photos/${vehicleId}/tour`
    );
    return response.data;
  }
}
```

### React Component - Image Uploader

```typescript
import React, { useState, useCallback } from "react";
import { useDropzone } from "react-dropzone";
import { useMutation } from "@tanstack/react-query";
import { PhotoService } from "@/services/photoService";

export const VehicleImageUploader = ({ vehicleId, onUploadComplete }) => {
  const [previews, setPreviews] = useState<string[]>([]);
  const [files, setFiles] = useState<File[]>([]);
  const photoService = new PhotoService();

  const { mutate: uploadImages, isLoading } = useMutation({
    mutationFn: (files: File[]) =>
      photoService.uploadAndProcess(vehicleId, files),
    onSuccess: (data) => {
      onUploadComplete(data);
      setPreviews([]);
      setFiles([]);
    },
  });

  const onDrop = useCallback((acceptedFiles: File[]) => {
    setFiles((prev) => [...prev, ...acceptedFiles]);
    acceptedFiles.forEach((file) => {
      const reader = new FileReader();
      reader.onload = () =>
        setPreviews((prev) => [...prev, reader.result as string]);
      reader.readAsDataURL(file);
    });
  }, []);

  const { getRootProps, getInputProps, isDragActive } = useDropzone({
    onDrop,
    accept: { "image/*": [".jpg", ".jpeg", ".png", ".webp"] },
    maxFiles: 20,
    maxSize: 10 * 1024 * 1024,
  });

  return (
    <div className="space-y-6">
      <div
        {...getRootProps()}
        className={`border-2 border-dashed rounded-xl p-8 text-center cursor-pointer ${
          isDragActive ? "border-blue-500 bg-blue-50" : "border-gray-300"
        }`}
      >
        <input {...getInputProps()} />
        <p className="text-lg font-medium">
          {isDragActive
            ? "Suelta las imágenes aquí"
            : "Arrastra fotos o haz clic para seleccionar"}
        </p>
        <p className="text-sm text-gray-500 mt-2">
          Mínimo 8 fotos para tour 360° | Máximo 20 fotos
        </p>
      </div>

      {previews.length > 0 && (
        <div>
          <div className="flex justify-between items-center mb-4">
            <h3>{previews.length} fotos seleccionadas</h3>
            <button
              onClick={() => uploadImages(files)}
              disabled={isLoading}
              className="px-6 py-2 bg-blue-600 text-white rounded-lg disabled:opacity-50"
            >
              {isLoading ? "Procesando..." : "Subir y Procesar"}
            </button>
          </div>
          <div className="grid grid-cols-4 gap-4">
            {previews.map((preview, i) => (
              <img
                key={i}
                src={preview}
                alt={`Preview ${i + 1}`}
                className="w-full h-32 object-cover rounded-lg"
              />
            ))}
          </div>
        </div>
      )}

      {previews.length >= 8 && (
        <div className="bg-green-50 border border-green-200 rounded-lg p-4">
          ✓ ¡Suficientes fotos para generar tour 360°!
        </div>
      )}
    </div>
  );
};
```

### React Component - Tour 360° Viewer

```typescript
import React from "react";
import { useQuery } from "@tanstack/react-query";
import { PhotoService } from "@/services/photoService";

export const Tour360Viewer = ({ vehicleId, className = "" }) => {
  const photoService = new PhotoService();
  const {
    data: tour,
    isLoading,
    error,
  } = useQuery({
    queryKey: ["tour360", vehicleId],
    queryFn: () => photoService.get360Tour(vehicleId),
  });

  if (isLoading)
    return (
      <div
        className={`bg-gray-100 rounded-xl flex items-center justify-center ${className}`}
      >
        <p>Cargando tour...</p>
      </div>
    );
  if (error || !tour)
    return (
      <div
        className={`bg-gray-100 rounded-xl flex items-center justify-center ${className}`}
      >
        <p>Tour 360° no disponible</p>
      </div>
    );
  if (tour.status === "Processing")
    return (
      <div
        className={`bg-yellow-50 rounded-xl flex items-center justify-center ${className}`}
      >
        <p>⏳ Tour en proceso...</p>
      </div>
    );

  return (
    <div className={`relative rounded-xl overflow-hidden ${className}`}>
      <iframe
        src={tour.embedUrl}
        width="100%"
        height="100%"
        frameBorder="0"
        allowFullScreen
        title="Tour Virtual 360°"
      />
      <div className="absolute top-4 right-4 bg-black/60 text-white px-3 py-1 rounded-full text-sm">
        🔄 Tour 360° | {tour.imageCount} fotos
      </div>
    </div>
  );
};
```

---

## ✅ Testing (xUnit)

```csharp
public class SpynePhotoServiceTests
{
    private readonly Mock<HttpClient> _httpClientMock;
    private readonly Mock<IConfiguration> _configMock;
    private readonly Mock<ILogger<SpynePhotoService>> _loggerMock;

    [Fact]
    public async Task ProcessImagesAsync_WithValidImages_ReturnsProcessedImages()
    {
        var images = new List<byte[]> { new byte[] { 0xFF, 0xD8, 0xFF }, new byte[] { 0xFF, 0xD8, 0xFF } };
        var options = new ProcessingOptions { RemoveBackground = true };

        var result = await _service.ProcessImagesAsync(images, options);

        Assert.NotNull(result);
        Assert.Equal(2, result.Length);
        Assert.All(result, img => Assert.NotEmpty(img.ProcessedUrl));
    }

    [Fact]
    public async Task Generate360TourAsync_With8PlusImages_ReturnsTour()
    {
        var vehicleId = "VH-12345";
        var imageUrls = Enumerable.Range(1, 10).Select(i => $"https://cdn.okla.com/images/{i}.jpg").ToList();

        var tour = await _service.Generate360TourAsync(vehicleId, imageUrls);

        Assert.NotNull(tour);
        Assert.Equal(vehicleId, tour.VehicleId);
        Assert.NotEmpty(tour.EmbedUrl);
        Assert.Equal(10, tour.ImageCount);
    }

    [Fact]
    public async Task DetectDamageAsync_WithDamagedVehicle_ReturnsReport()
    {
        var imageUrls = new List<string> { "https://cdn.okla.com/images/damaged.jpg" };

        var report = await _service.DetectDamageAsync(imageUrls);

        Assert.NotNull(report);
        Assert.NotEmpty(report.ReportId);
        Assert.True(report.ConditionScore >= 0 && report.ConditionScore <= 100);
    }

    [Fact]
    public async Task RemoveBackgroundAsync_WithValidImage_ReturnsUrl()
    {
        var imageBytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 };

        var processedUrl = await _service.RemoveBackgroundAsync(imageBytes);

        Assert.NotEmpty(processedUrl);
        Assert.StartsWith("https://", processedUrl);
    }
}
```

---

## 🔧 Troubleshooting

| Problema                       | Causa                              | Solución                                  |
| ------------------------------ | ---------------------------------- | ----------------------------------------- |
| Upload falla con 413           | Imagen muy grande (>10MB)          | Comprimir imagen cliente antes de subir   |
| Tour 360° no se genera         | Menos de 8 imágenes                | Requerir mínimo 8 fotos en UI             |
| Background removal incompleto  | Imagen compleja o mala iluminación | Usar modo "manual" o ajustar tolerancia   |
| Detección de daños incorrecta  | Ángulos malos o baja resolución    | Guía de fotos requeridas en UI            |
| API retorna 429 Rate Limit     | Más de 100 requests/minuto         | Implementar queue con backoff exponencial |
| Imágenes borrosas post-proceso | Compresión excesiva                | Aumentar quality a 95                     |
| Tour no carga en móvil         | JavaScript bloqueado               | Usar embed URL con fallback a galería     |
| Hotspots no aparecen           | Menos de 12 imágenes               | Mínimo 12 para auto-hotspots              |

---

## 🔗 Integración con OKLA

### 1. **Crear PhotoProcessingService en VehiclesSaleService**

```csharp
services.AddHttpClient<IPhotoProcessingService, SpynePhotoService>()
    .ConfigureHttpClient(client => client.Timeout = TimeSpan.FromMinutes(5))
    .AddPolicyHandler(GetRetryPolicy());
```

### 2. **Agregar endpoint en MediaService**

```csharp
[HttpPost("vehicles/{vehicleId}/photos")]
public async Task<ActionResult<ProcessedImage[]>> UploadPhotos(string vehicleId, [FromForm] List<IFormFile> images)
{
    var command = new ProcessVehicleImagesCommand { VehicleId = vehicleId, Images = images, GenerateTour = images.Count >= 8 };
    return await _mediator.Send(command);
}
```

### 3. **Gateway routing (ocelot.prod.json)**

```json
{
  "UpstreamPathTemplate": "/api/vehicles/{vehicleId}/photos",
  "DownstreamPathTemplate": "/api/media/vehicles/{vehicleId}/photos",
  "DownstreamHostAndPorts": [{ "Host": "mediaservice", "Port": 8080 }],
  "UpstreamHttpMethod": ["POST"]
}
```

### 4. **Integrar en PublishVehiclePage**

```tsx
<VehicleImageUploader
  vehicleId={vehicleId}
  onUploadComplete={(images) => setVehicleImages(images)}
/>
```

### 5. **Mostrar tour 360° en VehicleDetailPage**

```tsx
{
  vehicle.hasTour && (
    <Tour360Viewer vehicleId={vehicle.id} className="h-[400px]" />
  );
}
```

### 6. **Evento para analytics**

```csharp
await _bus.Publish(new VehicleImagesProcessedEvent { VehicleId = vehicleId, ImageCount = processed.Length, HasTour = tour != null });
```

---

## 💰 Costos Estimados

| API       | Plan   | Imágenes/mes | Costo/mes | Costo/año  |
| --------- | ------ | ------------ | --------- | ---------- |
| Spyne.ai  | Pro    | 10,000       | $300      | $3,600     |
| Spectrum  | Backup | 1,000        | $150      | $1,800     |
| **TOTAL** |        |              | **$450**  | **$5,400** |

✅ **ROI:** Listados con tour 360° generan 3x más contactos. Con 5,000 dealers y $29/listing promedio, el ROI es positivo desde el primer mes.

        return JsonSerializer.Deserialize<PhotoResult>(
            await response.Content.ReadAsStringAsync());
    }

}

```

---

**Versión:** 1.0 | **Actualizado:** Enero 15, 2026
```
