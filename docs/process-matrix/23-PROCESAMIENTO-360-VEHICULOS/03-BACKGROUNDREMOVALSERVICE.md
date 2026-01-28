# 🎨 BackgroundRemovalService - Servicio de Eliminación de Fondos

## 📋 Descripción

El **BackgroundRemovalService** es el microservicio especializado en eliminar fondos de imágenes de vehículos, dejando el vehículo con fondo transparente. Utiliza el **Strategy Pattern** para cambiar dinámicamente entre 6 proveedores de IA/ML.

## 🎯 Función Principal

```
INPUT:  1 Imagen con fondo original (JPEG, PNG, WebP)
OUTPUT: 1 Imagen con fondo transparente (PNG)
```

### Ejemplo Visual

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                                                                             │
│   [ANTES]                          [DESPUÉS]                               │
│                                                                             │
│   ┌─────────────────────┐         ┌─────────────────────┐                  │
│   │░░░░░░░░░░░░░░░░░░░░░│         │▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓│                  │
│   │░░░░░░░░░░░░░░░░░░░░░│         │▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓│                  │
│   │░░░░░┌─────────┐░░░░░│         │▓▓▓▓▓┌─────────┐▓▓▓▓▓│                  │
│   │░░░░░│  🚗    │░░░░░│   ───►  │▓▓▓▓▓│  🚗    │▓▓▓▓▓│                  │
│   │░░░░░│ Toyota │░░░░░│         │▓▓▓▓▓│ Toyota │▓▓▓▓▓│                  │
│   │░░░░░└─────────┘░░░░░│         │▓▓▓▓▓└─────────┘▓▓▓▓▓│                  │
│   │░░░FONDO ORIGINAL░░░░│         │▓▓▓▓TRANSPARENTE▓▓▓▓▓│                  │
│   └─────────────────────┘         └─────────────────────┘                  │
│                                                                             │
│   ░░░ = Fondo (parking, calle)    ▓▓▓ = Transparente (alpha)               │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

## 🏗️ Arquitectura Clean Architecture

```
BackgroundRemovalService/
├── BackgroundRemovalService.Domain/          # Entidades y contratos
│   ├── Entities/
│   │   ├── RemovalJob.cs                     # Job de procesamiento
│   │   ├── ProcessedImage.cs                 # Imagen procesada
│   │   ├── ProviderConfiguration.cs          # Config de proveedores
│   │   ├── UsageStatistics.cs                # Estadísticas de uso
│   │   └── UsageRecord.cs                    # Registro para billing
│   ├── Enums/
│   │   ├── BackgroundRemovalProvider.cs      # Tipos de proveedores
│   │   ├── ProcessingStatus.cs               # Estados del job
│   │   ├── ImageFormat.cs                    # Formatos soportados
│   │   └── OutputResolution.cs               # Resoluciones de salida
│   ├── Interfaces/
│   │   ├── IBackgroundRemovalProvider.cs     # Contrato de proveedor
│   │   └── IRemovalJobRepository.cs          # Repositorio de jobs
│   └── Common/
│       └── DomainEvents/
│
├── BackgroundRemovalService.Application/     # Casos de uso (CQRS)
│   ├── DTOs/
│   │   ├── RemovalJobDto.cs
│   │   ├── ProcessedImageDto.cs
│   │   ├── CreateRemovalJobRequest.cs
│   │   └── BatchRemovalRequest.cs
│   ├── Features/
│   │   ├── Commands/
│   │   │   ├── CreateRemovalJobCommand.cs
│   │   │   ├── BatchRemovalCommand.cs        # ⭐ Procesa múltiples imágenes
│   │   │   ├── CancelRemovalJobCommand.cs
│   │   │   └── RetryRemovalJobCommand.cs
│   │   ├── Queries/
│   │   │   ├── GetRemovalJobQuery.cs
│   │   │   └── GetProcessedImagesQuery.cs
│   │   └── Handlers/
│   └── Validators/
│
├── BackgroundRemovalService.Infrastructure/  # Implementaciones
│   ├── Providers/                            # ⭐ 6 Proveedores de IA
│   │   ├── ClipDropProvider.cs               # ClipDrop (DEFAULT)
│   │   ├── RemoveBgProvider.cs               # Remove.bg (Premium)
│   │   ├── PhotoroomProvider.cs              # Photoroom
│   │   ├── SlazzerProvider.cs                # Slazzer (Económico)
│   │   ├── ClippingMagicProvider.cs          # Clipping Magic
│   │   ├── RemovalAIProvider.cs              # Removal.AI
│   │   └── LocalProvider.cs                  # ML Local (GPU)
│   ├── Services/
│   │   ├── BackgroundRemovalOrchestrator.cs  # Orquestador
│   │   ├── ProviderFactory.cs                # Factory pattern
│   │   ├── ProviderSelector.cs               # Strategy selector
│   │   └── ImageStorageService.cs            # S3 storage
│   └── Persistence/
│       ├── BackgroundRemovalDbContext.cs
│       └── RemovalJobRepository.cs
│
├── BackgroundRemovalService.Api/             # REST API
│   ├── Controllers/
│   │   ├── RemovalController.cs
│   │   ├── BatchController.cs
│   │   └── ProvidersController.cs
│   ├── Program.cs
│   └── Dockerfile
│
└── BackgroundRemovalService.Tests/           # Unit Tests
```

## 💰 Tabla de Proveedores y Precios

| Proveedor          | Precio/Imagen | Precio × 6 Frames | Calidad                | Prioridad | Tipo        |
| ------------------ | ------------- | ----------------- | ---------------------- | --------: | ----------- |
| **Slazzer**        | **$0.02**     | **$0.12**         | ⭐⭐⭐⭐ Buena         |        90 | Económico   |
| **ClipDrop**       | **$0.05**     | **$0.30**         | ⭐⭐⭐⭐⭐ Excelente   |       100 | DEFAULT     |
| **Photoroom**      | **$0.05**     | **$0.30**         | ⭐⭐⭐⭐ Muy Buena     |        80 | Alternativo |
| **Removal.AI**     | **$0.08**     | **$0.48**         | ⭐⭐⭐⭐ Buena         |        60 | Backup      |
| **Clipping Magic** | **$0.10**     | **$0.60**         | ⭐⭐⭐⭐⭐ Excelente   |        70 | Premium     |
| **Remove.bg**      | **$0.20**     | **$1.20**         | ⭐⭐⭐⭐⭐ Profesional |        50 | Premium     |
| **Local (ML)**     | **$0.00**     | **$0.00**         | ⭐⭐⭐ Variable        |         0 | Sin costo   |

### Cálculo para Procesamiento 360° Completo

```
Un vehículo 360° requiere procesar 6 imágenes:

Opción Económica (Slazzer):     6 × $0.02 = $0.12/vehículo
Opción Recomendada (ClipDrop):  6 × $0.05 = $0.30/vehículo
Opción Premium (Remove.bg):     6 × $0.20 = $1.20/vehículo
Opción Gratuita (Local):        6 × $0.00 = $0.00/vehículo
```

### Comparativa por 1,000 Vehículos/Mes

| Proveedor      | Costo Mensual | Ahorro vs Remove.bg |
| -------------- | ------------- | ------------------- |
| Local (ML)     | $0            | $1,200 (100%)       |
| Slazzer        | $120          | $1,080 (90%)        |
| ClipDrop       | $300          | $900 (75%)          |
| Photoroom      | $300          | $900 (75%)          |
| Clipping Magic | $600          | $600 (50%)          |
| Remove.bg      | $1,200        | -                   |

## 📡 API Endpoints

### POST /api/removal/jobs

Crea un job de eliminación de fondo para una imagen.

**Request:**

```json
{
  "vehicleId": "550e8400-e29b-41d4-a716-446655440000",
  "imageUrl": "https://cdn.okla.com.do/vehicles/123/frame_00.jpg",
  "preferredProvider": "ClipDrop",
  "outputFormat": "Png",
  "outputResolution": "Original"
}
```

**Response:**

```json
{
  "jobId": "660e8400-e29b-41d4-a716-446655440000",
  "vehicleId": "550e8400-e29b-41d4-a716-446655440000",
  "status": "Pending",
  "createdAt": "2026-01-27T10:35:00Z"
}
```

### POST /api/removal/batch ⭐ (Para 360°)

Procesa múltiples imágenes en batch (ideal para los 6 frames).

**Request:**

```json
{
  "vehicleId": "550e8400-e29b-41d4-a716-446655440000",
  "images": [
    { "url": "https://cdn.okla.com.do/vehicles/123/frame_00.jpg", "index": 0 },
    { "url": "https://cdn.okla.com.do/vehicles/123/frame_01.jpg", "index": 1 },
    { "url": "https://cdn.okla.com.do/vehicles/123/frame_02.jpg", "index": 2 },
    { "url": "https://cdn.okla.com.do/vehicles/123/frame_03.jpg", "index": 3 },
    { "url": "https://cdn.okla.com.do/vehicles/123/frame_04.jpg", "index": 4 },
    { "url": "https://cdn.okla.com.do/vehicles/123/frame_05.jpg", "index": 5 }
  ],
  "preferredProvider": "ClipDrop",
  "outputFormat": "Png"
}
```

**Response:**

```json
{
  "batchId": "770e8400-e29b-41d4-a716-446655440000",
  "vehicleId": "550e8400-e29b-41d4-a716-446655440000",
  "status": "Processing",
  "totalImages": 6,
  "processedCount": 0,
  "estimatedProcessingSeconds": 90
}
```

### GET /api/removal/jobs/{id}

Obtiene el estado de un job.

**Response (Completed):**

```json
{
  "jobId": "660e8400-e29b-41d4-a716-446655440000",
  "vehicleId": "550e8400-e29b-41d4-a716-446655440000",
  "status": "Completed",
  "provider": "ClipDrop",
  "processingTimeMs": 2500,
  "costUsd": 0.05,
  "completedAt": "2026-01-27T10:35:03Z",
  "processedImage": {
    "originalUrl": "https://cdn.okla.com.do/vehicles/123/frame_00.jpg",
    "processedUrl": "https://cdn.okla.com.do/vehicles/123/nobg/frame_00.png",
    "thumbnailUrl": "https://cdn.okla.com.do/vehicles/123/nobg/thumb_00.png",
    "originalSizeBytes": 156000,
    "processedSizeBytes": 89000,
    "width": 1920,
    "height": 1080
  }
}
```

### GET /api/removal/batch/{batchId}

Obtiene el estado de un batch.

**Response:**

```json
{
  "batchId": "770e8400-e29b-41d4-a716-446655440000",
  "vehicleId": "550e8400-e29b-41d4-a716-446655440000",
  "status": "Completed",
  "totalImages": 6,
  "processedCount": 6,
  "successCount": 6,
  "failedCount": 0,
  "totalCostUsd": 0.3,
  "totalProcessingTimeMs": 15000,
  "processedImages": [
    {
      "index": 0,
      "status": "Completed",
      "originalUrl": "https://cdn.okla.com.do/vehicles/123/frame_00.jpg",
      "processedUrl": "https://cdn.okla.com.do/vehicles/123/nobg/frame_00.png"
    }
    // ... 5 imágenes más
  ]
}
```

### GET /api/providers

Lista los proveedores disponibles.

**Response:**

```json
{
  "providers": [
    {
      "name": "ClipDrop",
      "displayName": "ClipDrop by Stability AI",
      "isEnabled": true,
      "isDefault": true,
      "priority": 100,
      "costPerImageUsd": 0.05,
      "dailyLimit": 10000,
      "dailyUsageCount": 145,
      "features": ["high-quality", "fast", "vehicle-optimized"],
      "isAvailable": true
    },
    {
      "name": "Slazzer",
      "displayName": "Slazzer AI",
      "isEnabled": true,
      "isDefault": false,
      "priority": 90,
      "costPerImageUsd": 0.02,
      "features": ["budget-friendly", "good-quality"]
    }
    // ... otros proveedores
  ]
}
```

## 🔄 Flujo de Procesamiento Interno

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    BACKGROUNDREMOVALSERVICE - FLUJO INTERNO                  │
└─────────────────────────────────────────────────────────────────────────────┘

1. REQUEST BATCH RECIBIDO (6 imágenes de un vehículo 360°)
   ├── Validar formatos de imagen (jpg, png, webp)
   ├── Validar tamaños (<25MB por imagen)
   ├── Crear RemovalJob para cada imagen
   └── Crear BatchJob con referencia a todos los jobs
           │
           ▼
2. SELECCIÓN DE PROVEEDOR (ProviderSelector - Strategy Pattern)
   ┌──────────────────────────────────────────────────────────────────────────┐
   │  interface IBackgroundRemovalProvider                                    │
   │  {                                                                       │
   │      Task<ProcessedImage> RemoveBackgroundAsync(image, options);         │
   │      bool IsAvailable();                                                 │
   │      decimal GetCostPerImage();                                          │
   │  }                                                                       │
   │                                                                          │
   │  Implementaciones:                                                       │
   │  ├── ClipDropProvider      : IBackgroundRemovalProvider  (DEFAULT)       │
   │  ├── RemoveBgProvider      : IBackgroundRemovalProvider                  │
   │  ├── PhotoroomProvider     : IBackgroundRemovalProvider                  │
   │  ├── SlazzerProvider       : IBackgroundRemovalProvider                  │
   │  ├── ClippingMagicProvider : IBackgroundRemovalProvider                  │
   │  ├── RemovalAIProvider     : IBackgroundRemovalProvider                  │
   │  └── LocalProvider         : IBackgroundRemovalProvider  (ML local)      │
   └──────────────────────────────────────────────────────────────────────────┘
   ├── Verificar proveedor preferido
   ├── Si no disponible, usar fallback por prioridad
   └── Retornar proveedor seleccionado
           │
           ▼
3. PROCESAMIENTO PARALELO (6 imágenes simultáneas)
   ┌───────────────────────────────────────────────────────────────────────────┐
   │  Task.WhenAll(                                                           │
   │      ProcessImage(frame_00.jpg) ──► ClipDrop API ──► frame_00_nobg.png   │
   │      ProcessImage(frame_01.jpg) ──► ClipDrop API ──► frame_01_nobg.png   │
   │      ProcessImage(frame_02.jpg) ──► ClipDrop API ──► frame_02_nobg.png   │
   │      ProcessImage(frame_03.jpg) ──► ClipDrop API ──► frame_03_nobg.png   │
   │      ProcessImage(frame_04.jpg) ──► ClipDrop API ──► frame_04_nobg.png   │
   │      ProcessImage(frame_05.jpg) ──► ClipDrop API ──► frame_05_nobg.png   │
   │  );                                                                      │
   │                                                                          │
   │  Cada procesamiento individual:                                          │
   │  1. Descargar imagen original                                            │
   │  2. Llamar API del proveedor                                             │
   │  3. Recibir imagen sin fondo                                             │
   │  4. Guardar en S3 (MediaService)                                         │
   │  5. Crear ProcessedImage entity                                          │
   └───────────────────────────────────────────────────────────────────────────┘
           │
           ▼
4. MANEJO DE ERRORES POR IMAGEN
   ├── Si imagen falla:
   │   ├── Intentar con proveedor fallback
   │   ├── Si todos fallan, marcar imagen como Failed
   │   └── Continuar con otras imágenes (no bloquea batch)
   │
   └── Batch se considera exitoso si ≥1 imagen procesada
           │
           ▼
5. ALMACENAMIENTO FINAL
   ├── Guardar todas las imágenes procesadas en S3:
   │   ├── /vehicles/{id}/nobg/frame_00.png
   │   ├── /vehicles/{id}/nobg/frame_01.png
   │   ├── /vehicles/{id}/nobg/frame_02.png
   │   ├── /vehicles/{id}/nobg/frame_03.png
   │   ├── /vehicles/{id}/nobg/frame_04.png
   │   └── /vehicles/{id}/nobg/frame_05.png
   │
   └── Generar thumbnails (opcional):
       ├── /vehicles/{id}/nobg/thumb_00.png (400x300)
       └── ...
           │
           ▼
6. REGISTRO Y BILLING
   ├── Crear UsageRecord para cada imagen
   ├── Calcular costo total del batch
   ├── Actualizar estadísticas del proveedor
   └── Emitir evento de completado
```

## 🛡️ Strategy Pattern - Implementación

### Interfaz Base

```csharp
public interface IBackgroundRemovalProvider
{
    string ProviderName { get; }
    BackgroundRemovalProvider ProviderType { get; }
    decimal CostPerImageUsd { get; }
    int Priority { get; }

    Task<ProcessedImage> RemoveBackgroundAsync(
        string imageUrl,
        RemovalOptions options,
        CancellationToken cancellationToken = default);

    bool IsAvailable();
    Task<HealthStatus> CheckHealthAsync();
}
```

### Implementación de Proveedor (Ejemplo: ClipDrop)

```csharp
public class ClipDropProvider : IBackgroundRemovalProvider
{
    private readonly HttpClient _httpClient;
    private readonly ClipDropOptions _options;

    public string ProviderName => "ClipDrop";
    public BackgroundRemovalProvider ProviderType => BackgroundRemovalProvider.ClipDrop;
    public decimal CostPerImageUsd => 0.05m;
    public int Priority => 100;

    public async Task<ProcessedImage> RemoveBackgroundAsync(
        string imageUrl,
        RemovalOptions options,
        CancellationToken ct)
    {
        // 1. Descargar imagen original
        var imageBytes = await DownloadImageAsync(imageUrl, ct);

        // 2. Llamar API de ClipDrop
        var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(imageBytes), "image_file", "image.jpg");

        var response = await _httpClient.PostAsync(
            "https://clipdrop-api.co/remove-background/v1",
            content,
            ct);

        response.EnsureSuccessStatusCode();

        // 3. Obtener resultado
        var resultBytes = await response.Content.ReadAsByteArrayAsync(ct);

        return new ProcessedImage
        {
            ImageBytes = resultBytes,
            Format = ImageFormat.Png,
            Width = options.OutputWidth,
            Height = options.OutputHeight
        };
    }

    public bool IsAvailable()
    {
        return _options.IsEnabled
            && _options.DailyUsageCount < _options.DailyLimit;
    }
}
```

### Provider Factory

```csharp
public class ProviderFactory : IProviderFactory
{
    private readonly IServiceProvider _serviceProvider;

    public IBackgroundRemovalProvider CreateProvider(BackgroundRemovalProvider type)
    {
        return type switch
        {
            BackgroundRemovalProvider.ClipDrop => _serviceProvider.GetRequiredService<ClipDropProvider>(),
            BackgroundRemovalProvider.RemoveBg => _serviceProvider.GetRequiredService<RemoveBgProvider>(),
            BackgroundRemovalProvider.Photoroom => _serviceProvider.GetRequiredService<PhotoroomProvider>(),
            BackgroundRemovalProvider.Slazzer => _serviceProvider.GetRequiredService<SlazzerProvider>(),
            BackgroundRemovalProvider.ClippingMagic => _serviceProvider.GetRequiredService<ClippingMagicProvider>(),
            BackgroundRemovalProvider.RemovalAI => _serviceProvider.GetRequiredService<RemovalAIProvider>(),
            BackgroundRemovalProvider.Local => _serviceProvider.GetRequiredService<LocalProvider>(),
            _ => throw new NotSupportedException($"Provider {type} not supported")
        };
    }

    public IBackgroundRemovalProvider GetDefaultProvider()
    {
        return CreateProvider(BackgroundRemovalProvider.ClipDrop);
    }

    public IBackgroundRemovalProvider GetNextAvailableProvider(BackgroundRemovalProvider current)
    {
        var providers = GetAllProviders()
            .Where(p => p.ProviderType != current)
            .OrderByDescending(p => p.Priority)
            .ToList();

        return providers.FirstOrDefault(p => p.IsAvailable());
    }
}
```

## 📊 Entidades de Dominio

### RemovalJob

```csharp
public class RemovalJob
{
    public Guid Id { get; set; }
    public Guid? BatchId { get; set; }
    public Guid? VehicleId { get; set; }
    public Guid? UserId { get; set; }
    public string? TenantId { get; set; }

    // Image info
    public string SourceImageUrl { get; set; }
    public long SourceImageSizeBytes { get; set; }
    public int SourceWidth { get; set; }
    public int SourceHeight { get; set; }

    // Processing config
    public BackgroundRemovalProvider Provider { get; set; }
    public ProcessingStatus Status { get; set; }
    public ImageFormat OutputFormat { get; set; } = ImageFormat.Png;
    public OutputResolution OutputResolution { get; set; } = OutputResolution.Original;

    // Result
    public ProcessedImage? ProcessedImage { get; set; }
    public long? ProcessingTimeMs { get; set; }
    public decimal? CostUsd { get; set; }

    // Error handling
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }
    public int MaxRetries { get; set; } = 3;

    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
```

### ProcessedImage

```csharp
public class ProcessedImage
{
    public Guid Id { get; set; }
    public Guid RemovalJobId { get; set; }

    public string OriginalUrl { get; set; }
    public string ProcessedUrl { get; set; }
    public string? ThumbnailUrl { get; set; }

    public long OriginalSizeBytes { get; set; }
    public long ProcessedSizeBytes { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    public ImageFormat Format { get; set; }
    public bool HasTransparency { get; set; } = true;
}
```

## 🔧 Configuración

### appsettings.json

```json
{
  "Providers": {
    "ClipDrop": {
      "BaseUrl": "https://clipdrop-api.co",
      "ApiKey": "${CLIPDROP_API_KEY}",
      "IsEnabled": true,
      "Priority": 100,
      "CostPerImageUsd": 0.05,
      "TimeoutSeconds": 60,
      "DailyLimit": 10000
    },
    "Slazzer": {
      "BaseUrl": "https://api.slazzer.com",
      "ApiKey": "${SLAZZER_API_KEY}",
      "IsEnabled": true,
      "Priority": 90,
      "CostPerImageUsd": 0.02
    },
    "RemoveBg": {
      "BaseUrl": "https://api.remove.bg",
      "ApiKey": "${REMOVEBG_API_KEY}",
      "IsEnabled": true,
      "Priority": 50,
      "CostPerImageUsd": 0.2
    },
    "Photoroom": {
      "BaseUrl": "https://sdk.photoroom.com",
      "ApiKey": "${PHOTOROOM_API_KEY}",
      "IsEnabled": true,
      "Priority": 80,
      "CostPerImageUsd": 0.05
    },
    "ClippingMagic": {
      "BaseUrl": "https://clippingmagic.com/api/v1",
      "ApiId": "${CLIPPINGMAGIC_API_ID}",
      "ApiSecret": "${CLIPPINGMAGIC_API_SECRET}",
      "IsEnabled": true,
      "Priority": 70,
      "CostPerImageUsd": 0.1
    },
    "RemovalAI": {
      "BaseUrl": "https://api.removal.ai",
      "ApiKey": "${REMOVALAI_API_KEY}",
      "IsEnabled": true,
      "Priority": 60,
      "CostPerImageUsd": 0.08
    },
    "Local": {
      "ModelPath": "/models/u2net.onnx",
      "IsEnabled": false,
      "Priority": 0,
      "CostPerImageUsd": 0.0
    }
  },
  "Batch": {
    "MaxImagesPerBatch": 50,
    "ParallelProcessingLimit": 10,
    "TimeoutPerImageSeconds": 60
  }
}
```

### Variables de Entorno

```bash
# Database
ConnectionStrings__DefaultConnection=Host=postgres;Database=backgroundremovalservice;...

# Providers
CLIPDROP_API_KEY=your-api-key
SLAZZER_API_KEY=your-api-key
REMOVEBG_API_KEY=your-api-key
PHOTOROOM_API_KEY=your-api-key
CLIPPINGMAGIC_API_ID=your-id
CLIPPINGMAGIC_API_SECRET=your-secret
REMOVALAI_API_KEY=your-api-key

# S3
AWS_ACCESS_KEY_ID=your-access-key
AWS_SECRET_ACCESS_KEY=your-secret-key
```

## 🧪 Tests

```bash
cd BackgroundRemovalService.Tests
dotnet test

# Resultados esperados:
# Passed!  - Failed: 0, Passed: 45, Skipped: 0
```

---

**Anterior:** [02-VIDEO360SERVICE.md](./02-VIDEO360SERVICE.md)  
**Siguiente:** [04-VEHICLE360PROCESSINGSERVICE.md](./04-VEHICLE360PROCESSINGSERVICE.md)
