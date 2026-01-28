# 🎬 Video360Service - Servicio de Extracción de Frames

## 📋 Descripción

El **Video360Service** es el microservicio responsable de extraer frames equidistantes de un video de vehículo girando 360°. Recibe un video y retorna 6 imágenes de alta calidad que representan los ángulos principales del vehículo.

## 🎯 Función Principal

```
INPUT:  1 Video de vehículo girando (MP4, MOV, AVI, WebM)
OUTPUT: 6 Imágenes equidistantes (JPEG, PNG, WebP)
```

### Ángulos Extraídos

| Frame | Ángulo | Etiqueta        | Descripción                |
| ----- | ------ | --------------- | -------------------------- |
| 0     | 0°     | **Front**       | Vista frontal del vehículo |
| 1     | 60°    | **Front-Right** | Diagonal frontal derecha   |
| 2     | 120°   | **Rear-Right**  | Diagonal trasera derecha   |
| 3     | 180°   | **Rear**        | Vista trasera del vehículo |
| 4     | 240°   | **Rear-Left**   | Diagonal trasera izquierda |
| 5     | 300°   | **Front-Left**  | Diagonal frontal izquierda |

## 🏗️ Arquitectura Clean Architecture

```
Video360Service/
├── Video360Service.Domain/              # Entidades y contratos
│   ├── Entities/
│   │   ├── Video360Job.cs               # Job de procesamiento
│   │   ├── ExtractedFrame.cs            # Frame extraído
│   │   ├── ProviderConfiguration.cs     # Config de proveedores
│   │   └── UsageRecord.cs               # Registro de uso/billing
│   ├── Enums/
│   │   ├── Video360Provider.cs          # Tipos de proveedores
│   │   ├── ProcessingStatus.cs          # Estados del job
│   │   ├── ImageFormat.cs               # Formatos de salida
│   │   └── VideoQuality.cs              # Calidades de video
│   └── Interfaces/
│       ├── IVideo360Provider.cs         # Contrato de proveedor
│       └── IVideo360JobRepository.cs    # Repositorio de jobs
│
├── Video360Service.Application/         # Casos de uso (CQRS)
│   ├── DTOs/
│   ├── Features/
│   │   ├── Commands/
│   │   │   ├── CreateVideo360JobCommand.cs
│   │   │   ├── CancelVideo360JobCommand.cs
│   │   │   └── RetryVideo360JobCommand.cs
│   │   ├── Queries/
│   │   │   ├── GetVideo360JobQuery.cs
│   │   │   └── GetVehicle360ViewQuery.cs
│   │   └── Handlers/
│   └── Validators/
│
├── Video360Service.Infrastructure/      # Implementaciones
│   ├── Providers/                       # ⭐ 5 Proveedores de API
│   │   ├── FfmpegApiProvider.cs         # FFmpeg-API.com (DEFAULT)
│   │   ├── ApyHubProvider.cs            # ApyHub
│   │   ├── CloudinaryProvider.cs        # Cloudinary
│   │   ├── ImgixProvider.cs             # Imgix
│   │   └── ShotstackProvider.cs         # Shotstack
│   ├── Services/
│   │   ├── Video360Orchestrator.cs      # Orquestador interno
│   │   ├── Video360ProviderFactory.cs   # Factory pattern
│   │   └── VideoStorageService.cs       # Almacenamiento S3
│   └── Persistence/
│       ├── Video360DbContext.cs
│       └── Video360JobRepository.cs
│
├── Video360Service.Api/                 # REST API
│   ├── Controllers/
│   │   ├── Video360Controller.cs
│   │   └── ProvidersController.cs
│   ├── Program.cs
│   └── Dockerfile
│
└── Video360Service.Tests/               # Unit Tests
```

## 💰 Tabla de Proveedores y Precios

| Proveedor          | Costo Mensual | Costo por Vehículo | Calidad                | Prioridad | Estado  |
| ------------------ | ------------- | ------------------ | ---------------------- | --------: | ------- |
| **FFmpeg-API.com** | $11/mes       | **$0.011**         | ⭐⭐⭐⭐⭐ Excelente   |       100 | DEFAULT |
| **ApyHub**         | $9/mes        | **$0.009**         | ⭐⭐⭐⭐ Muy Buena     |        90 | Activo  |
| **Cloudinary**     | $12/mes       | **$0.012**         | ⭐⭐⭐⭐ Buena         |        70 | Activo  |
| **Imgix**          | $18/mes       | **$0.018**         | ⭐⭐⭐⭐⭐ Excelente   |        80 | Activo  |
| **Shotstack**      | $50/mes       | **$0.05**          | ⭐⭐⭐⭐⭐ Profesional |        50 | Activo  |

### Cálculo de Costo por Vehículo

```
Fórmula: (Costo mensual del plan) / (Vehículos incluidos en el plan)

FFmpeg-API Starter:  $11 / 1000 videos = $0.011/vehículo
ApyHub Basic:        $9 / 1000 videos  = $0.009/vehículo
Cloudinary:          $12 / 1000 videos = $0.012/vehículo
Imgix:               $18 / 1000 videos = $0.018/vehículo
Shotstack Pro:       $50 / 1000 videos = $0.05/vehículo
```

## 📡 API Endpoints

### POST /api/video360/jobs

Crea un nuevo job de extracción de frames.

**Request:**

```json
{
  "vehicleId": "550e8400-e29b-41d4-a716-446655440000",
  "videoUrl": "https://cdn.okla.com.do/videos/vehicle123/original.mp4",
  "frameCount": 6,
  "imageFormat": "Jpeg",
  "videoQuality": "High",
  "preferredProvider": "FfmpegApi"
}
```

**Response:**

```json
{
  "jobId": "660e8400-e29b-41d4-a716-446655440000",
  "vehicleId": "550e8400-e29b-41d4-a716-446655440000",
  "status": "Pending",
  "createdAt": "2026-01-27T10:30:00Z",
  "estimatedProcessingSeconds": 60
}
```

### POST /api/video360/jobs/upload

Sube un video directamente y crea el job.

**Request:** (multipart/form-data)

```
video: [archivo.mp4]
vehicleId: "550e8400-e29b-41d4-a716-446655440000"
frameCount: 6
imageFormat: Jpeg
videoQuality: High
```

### GET /api/video360/jobs/{id}

Obtiene el estado de un job.

**Response (Processing):**

```json
{
  "jobId": "660e8400-e29b-41d4-a716-446655440000",
  "vehicleId": "550e8400-e29b-41d4-a716-446655440000",
  "status": "Processing",
  "progress": 50,
  "provider": "FfmpegApi",
  "startedAt": "2026-01-27T10:30:05Z"
}
```

**Response (Completed):**

```json
{
  "jobId": "660e8400-e29b-41d4-a716-446655440000",
  "vehicleId": "550e8400-e29b-41d4-a716-446655440000",
  "status": "Completed",
  "provider": "FfmpegApi",
  "processingTimeMs": 45000,
  "costUsd": 0.011,
  "completedAt": "2026-01-27T10:30:50Z",
  "frames": [
    {
      "index": 0,
      "angleDegrees": 0,
      "angleLabel": "Front",
      "imageUrl": "https://cdn.okla.com.do/vehicles/123/360/frame_00.jpg",
      "thumbnailUrl": "https://cdn.okla.com.do/vehicles/123/360/thumb_00.jpg",
      "fileSizeBytes": 156000,
      "width": 1920,
      "height": 1080
    }
    // ... 5 frames más
  ]
}
```

### GET /api/video360/vehicles/{vehicleId}/view

Obtiene los datos del visor 360° de un vehículo.

**Response:**

```json
{
  "vehicleId": "550e8400-e29b-41d4-a716-446655440000",
  "isReady": true,
  "totalFrames": 6,
  "frames": [
    {
      "index": 0,
      "angle": 0,
      "name": "Front",
      "imageUrl": "https://cdn.okla.com.do/...",
      "thumbnailUrl": "https://cdn.okla.com.do/..."
    }
    // ...
  ],
  "config": {
    "autoRotate": true,
    "autoRotateSpeed": 5000,
    "allowDrag": true
  }
}
```

### GET /api/providers

Lista los proveedores disponibles.

**Response:**

```json
{
  "providers": [
    {
      "name": "FfmpegApi",
      "displayName": "FFmpeg-API.com",
      "isEnabled": true,
      "isDefault": true,
      "priority": 100,
      "costPerVideoUsd": 0.011,
      "dailyLimit": 1000,
      "dailyUsageCount": 45,
      "isAvailable": true
    }
    // ... otros proveedores
  ]
}
```

## 🔄 Flujo de Procesamiento Interno

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         VIDEO360SERVICE - FLUJO INTERNO                      │
└─────────────────────────────────────────────────────────────────────────────┘

1. REQUEST RECIBIDO
   ├── Validar formato de video (mp4, mov, avi, webm)
   ├── Validar tamaño (<500MB)
   ├── Validar duración (<120 segundos)
   └── Crear Video360Job (status: Pending)
           │
           ▼
2. SELECCIÓN DE PROVEEDOR (Video360ProviderFactory)
   ├── Verificar preferredProvider del request
   ├── Si no hay preferido, usar prioridad:
   │   ┌──────────────────────────────────────────┐
   │   │ 1. FfmpegApi  (priority: 100) ← DEFAULT │
   │   │ 2. ApyHub     (priority: 90)            │
   │   │ 3. Imgix      (priority: 80)            │
   │   │ 4. Cloudinary (priority: 70)            │
   │   │ 5. Shotstack  (priority: 50)            │
   │   └──────────────────────────────────────────┘
   ├── Verificar disponibilidad:
   │   ├── IsEnabled == true?
   │   ├── DailyUsageCount < DailyLimit?
   │   └── CircuitBreaker closed?
   └── Retornar proveedor seleccionado
           │
           ▼
3. ENVÍO A PROVEEDOR EXTERNO
   ├── Actualizar status: Processing
   ├── Registrar StartedAt
   ├── Llamar al proveedor externo:
   │   ┌────────────────────────────────────────────────────────────┐
   │   │ FfmpegApiProvider.ExtractFramesAsync(videoUrl, options)   │
   │   │   ├── POST https://api.ffmpeg-api.com/v1/video/extract   │
   │   │   ├── Payload: { video_url, frame_count: 6, format: jpg } │
   │   │   └── Response: { frames: [...urls] }                     │
   │   └────────────────────────────────────────────────────────────┘
   └── Timeout: 300 segundos
           │
           ▼
4. PROCESAMIENTO DE RESULTADO
   ├── Si exitoso:
   │   ├── Descargar frames del proveedor
   │   ├── Guardar en S3 (MediaService)
   │   ├── Crear ExtractedFrame entities
   │   ├── Calcular timestamps y ángulos
   │   ├── Actualizar status: Completed
   │   └── Registrar costUsd y processingTimeMs
   │
   └── Si fallido:
       ├── Registrar error
       ├── Si hay reintentos disponibles:
       │   ├── IncrementRetryCount()
       │   └── Intentar con siguiente proveedor (fallback)
       └── Si no hay más reintentos:
           └── Actualizar status: Failed
           │
           ▼
5. REGISTRO DE USO
   ├── Crear UsageRecord en DB
   ├── Actualizar DailyUsageCount del proveedor
   └── Emitir evento para billing (si aplica)
```

## 🛡️ Resiliencia y Fallback

### Política de Reintentos

```csharp
// Configuración de Polly
services.AddHttpClient<IVideo360Provider>()
    .AddRetryPolicy(options =>
    {
        options.MaxRetries = 3;
        options.BackoffType = DelayBackoffType.Exponential;
        options.Delay = TimeSpan.FromSeconds(2); // 2s, 4s, 8s
    });
```

### Circuit Breaker

```csharp
// Se activa después de 5 fallos consecutivos
.AddCircuitBreakerPolicy(options =>
{
    options.FailureThreshold = 5;
    options.SamplingDuration = TimeSpan.FromMinutes(1);
    options.BreakDuration = TimeSpan.FromSeconds(30);
});
```

### Fallback Automático

```
Si FFmpeg-API falla → Intentar con ApyHub
Si ApyHub falla → Intentar con Imgix
Si Imgix falla → Intentar con Cloudinary
Si Cloudinary falla → Intentar con Shotstack
Si Shotstack falla → Marcar job como Failed
```

## 📊 Entidades de Dominio

### Video360Job

```csharp
public class Video360Job
{
    public Guid Id { get; set; }
    public Guid? VehicleId { get; set; }
    public Guid? UserId { get; set; }
    public string? TenantId { get; set; }

    // Video info
    public string SourceVideoUrl { get; set; }
    public string? VideoUrl { get; set; }
    public double VideoDurationSeconds { get; set; }
    public int? VideoWidth { get; set; }
    public int? VideoHeight { get; set; }

    // Processing config
    public Video360Provider Provider { get; set; }
    public ProcessingStatus Status { get; set; }
    public ImageFormat ImageFormat { get; set; }
    public VideoQuality VideoQuality { get; set; }
    public int FrameCount { get; set; } = 6;

    // Results
    public List<ExtractedFrame> ExtractedFrames { get; set; }
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

### ExtractedFrame

```csharp
public class ExtractedFrame
{
    public Guid Id { get; set; }
    public Guid Video360JobId { get; set; }

    public int FrameIndex { get; set; }      // 0-5
    public int AngleDegrees { get; set; }    // 0, 60, 120, 180, 240, 300
    public string? AngleLabel { get; set; }  // "Front", "Rear", etc.

    public string ImageUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public long FileSizeBytes { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}
```

## 🔧 Configuración

### appsettings.json

```json
{
  "Providers": {
    "FfmpegApi": {
      "BaseUrl": "https://api.ffmpeg-api.com",
      "ApiKey": "${FFMPEG_API_KEY}",
      "IsEnabled": true,
      "Priority": 100,
      "CostPerVideoUsd": 0.011,
      "TimeoutSeconds": 300,
      "DailyLimit": 1000
    },
    "ApyHub": {
      "BaseUrl": "https://api.apyhub.com",
      "ApiToken": "${APYHUB_API_TOKEN}",
      "IsEnabled": true,
      "Priority": 90,
      "CostPerVideoUsd": 0.009
    }
    // ... otros proveedores
  },
  "Storage": {
    "S3": {
      "BucketName": "okla-video360",
      "Region": "us-east-1",
      "CdnBaseUrl": "https://cdn.okla.com.do"
    }
  }
}
```

### Variables de Entorno

```bash
# Database
ConnectionStrings__DefaultConnection=Host=postgres;Database=video360service;...

# Providers
FFMPEG_API_KEY=your-api-key
APYHUB_API_TOKEN=your-token
CLOUDINARY_CLOUD_NAME=your-cloud
CLOUDINARY_API_KEY=your-key
CLOUDINARY_API_SECRET=your-secret
IMGIX_API_KEY=your-key
SHOTSTACK_API_KEY=your-key

# S3
AWS_ACCESS_KEY_ID=your-access-key
AWS_SECRET_ACCESS_KEY=your-secret-key
```

## 🧪 Tests

```bash
cd Video360Service.Tests
dotnet test

# Resultados esperados:
# Passed!  - Failed: 0, Passed: 50, Skipped: 0
```

---

**Anterior:** [01-VISION-GENERAL.md](./01-VISION-GENERAL.md)  
**Siguiente:** [03-BACKGROUNDREMOVALSERVICE.md](./03-BACKGROUNDREMOVALSERVICE.md)
