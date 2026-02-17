# 🎛️ Vehicle360ProcessingService - Orquestador Central

## 📋 Descripción

El **Vehicle360ProcessingService** es el microservicio orquestador que coordina el flujo completo de procesamiento 360° de vehículos. Actúa como el "director de orquesta" que integra MediaService, Video360Service y BackgroundRemovalService en un solo flujo unificado.

## 🎯 Función Principal

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                                                                             │
│   VEHICLE360PROCESSINGSERVICE - ORQUESTADOR CENTRAL                        │
│                                                                             │
│   INPUT:  1 Video de vehículo girando 360°                                 │
│   OUTPUT: 6 Imágenes HD sin fondo + Metadatos del visor 360°               │
│                                                                             │
│   Coordina:                                                                 │
│   ├── MediaService ──────────► Almacenamiento S3 + CDN                     │
│   ├── Video360Service ───────► Extracción de 6 frames                      │
│   └── BackgroundRemovalService ► Eliminación de fondos (×6)                │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

## 🏗️ Arquitectura Clean Architecture

```
Vehicle360ProcessingService/
├── Vehicle360ProcessingService.Domain/           # Entidades y contratos
│   ├── Entities/
│   │   ├── Vehicle360ProcessingJob.cs            # Job principal
│   │   ├── ProcessingStep.cs                     # Paso individual
│   │   ├── ProcessedFrame.cs                     # Frame procesado
│   │   └── Vehicle360View.cs                     # Vista final
│   ├── Enums/
│   │   ├── ProcessingJobStatus.cs                # Estados del job
│   │   ├── ProcessingStepType.cs                 # Tipos de pasos
│   │   └── ProcessingStepStatus.cs               # Estados de pasos
│   ├── Interfaces/
│   │   ├── IVehicle360ProcessingJobRepository.cs # Repositorio principal
│   │   ├── IMediaServiceClient.cs                # Cliente MediaService
│   │   ├── IVideo360ServiceClient.cs             # Cliente Video360Service
│   │   └── IBackgroundRemovalServiceClient.cs    # Cliente BackgroundRemovalService
│   └── Events/
│       ├── ProcessingStartedEvent.cs
│       ├── ProcessingCompletedEvent.cs
│       └── ProcessingFailedEvent.cs
│
├── Vehicle360ProcessingService.Application/      # Casos de uso (CQRS)
│   ├── DTOs/
│   │   ├── Vehicle360ProcessingJobDto.cs
│   │   ├── Vehicle360ViewDto.cs
│   │   └── CreateVehicle360JobRequest.cs
│   ├── Features/
│   │   ├── Commands/
│   │   │   ├── CreateVehicle360JobCommand.cs     # Crear job
│   │   │   ├── ProcessVehicle360Command.cs       # Ejecutar procesamiento
│   │   │   ├── CancelVehicle360JobCommand.cs     # Cancelar
│   │   │   └── RetryVehicle360JobCommand.cs      # Reintentar
│   │   ├── Queries/
│   │   │   ├── GetVehicle360JobQuery.cs          # Estado del job
│   │   │   ├── GetVehicle360ViewQuery.cs         # Vista para frontend
│   │   │   └── GetProcessingJobsQuery.cs         # Listar jobs
│   │   └── Handlers/
│   └── Validators/
│
├── Vehicle360ProcessingService.Infrastructure/   # Implementaciones
│   ├── ServiceClients/                           # ⭐ Clientes HTTP a otros servicios
│   │   ├── MediaServiceClient.cs                 # Conecta con MediaService
│   │   ├── Video360ServiceClient.cs              # Conecta con Video360Service
│   │   └── BackgroundRemovalServiceClient.cs     # Conecta con BackgroundRemovalService
│   ├── Orchestration/
│   │   ├── Vehicle360Orchestrator.cs             # ⭐ Orquestador principal
│   │   ├── ProcessingPipeline.cs                 # Pipeline de procesamiento
│   │   └── StepExecutor.cs                       # Ejecutor de pasos
│   ├── Resilience/                               # ⭐ Polly para resiliencia
│   │   ├── RetryPolicies.cs                      # Políticas de reintento
│   │   ├── CircuitBreakerPolicies.cs             # Circuit breakers
│   │   └── TimeoutPolicies.cs                    # Timeouts
│   └── Persistence/
│       ├── Vehicle360DbContext.cs
│       └── Vehicle360ProcessingJobRepository.cs
│
├── Vehicle360ProcessingService.Api/              # REST API
│   ├── Controllers/
│   │   ├── Vehicle360Controller.cs               # API principal
│   │   └── HealthController.cs
│   ├── BackgroundServices/
│   │   └── ProcessingWorker.cs                   # Worker para jobs en cola
│   ├── Program.cs
│   └── Dockerfile
│
└── Vehicle360ProcessingService.Tests/            # Unit Tests
```

## 💰 Costo Total por Vehículo

El orquestador calcula el costo total combinando Video360Service + BackgroundRemovalService:

### Opción Económica

```
Video360:            ApyHub        $0.009
Background Removal:  Slazzer × 6   $0.02 × 6 = $0.12
────────────────────────────────────────────────
TOTAL ECONÓMICO:                  $0.129/vehículo
```

### Opción Recomendada (Balance Calidad/Precio)

```
Video360:            FFmpeg-API    $0.011
Background Removal:  ClipDrop × 6  $0.05 × 6 = $0.30
────────────────────────────────────────────────
TOTAL RECOMENDADO:                $0.311/vehículo
```

### Opción Premium

```
Video360:            Shotstack     $0.05
Background Removal:  Remove.bg × 6 $0.20 × 6 = $1.20
────────────────────────────────────────────────
TOTAL PREMIUM:                    $1.25/vehículo
```

### Comparativa por Volumen

| Volumen Mensual | Económico | Recomendado | Premium   |
| --------------- | --------- | ----------- | --------- |
| 100 vehículos   | $12.90    | $31.10      | $125.00   |
| 500 vehículos   | $64.50    | $155.50     | $625.00   |
| 1,000 vehículos | $129.00   | $311.00     | $1,250.00 |
| 5,000 vehículos | $645.00   | $1,555.00   | $6,250.00 |

## 📡 API Endpoints

### POST /api/vehicle360/process ⭐ (Endpoint Principal)

Inicia el procesamiento completo 360° de un vehículo.

**Request:**

```json
{
  "vehicleId": "550e8400-e29b-41d4-a716-446655440000",
  "videoUrl": "https://cdn.okla.com.do/uploads/vehicle123/raw_video.mp4",
  "options": {
    "frameCount": 6,
    "imageFormat": "Jpeg",
    "removeBackground": true,
    "video360Provider": "FfmpegApi",
    "backgroundRemovalProvider": "ClipDrop",
    "generateThumbnails": true,
    "thumbnailSize": { "width": 400, "height": 300 }
  },
  "callbackUrl": "https://api.okla.com.do/webhooks/vehicle360/complete"
}
```

**Response:**

```json
{
  "jobId": "880e8400-e29b-41d4-a716-446655440000",
  "vehicleId": "550e8400-e29b-41d4-a716-446655440000",
  "status": "Pending",
  "createdAt": "2026-01-27T10:00:00Z",
  "steps": [
    { "step": "UploadToS3", "status": "Pending" },
    { "step": "ExtractFrames", "status": "Pending" },
    { "step": "RemoveBackgrounds", "status": "Pending" },
    { "step": "GenerateThumbnails", "status": "Pending" },
    { "step": "CreateView", "status": "Pending" }
  ],
  "estimatedProcessingSeconds": 180
}
```

### POST /api/vehicle360/upload-and-process

Sube video directamente y procesa.

**Request:** (multipart/form-data)

```
video: [archivo.mp4]
vehicleId: 550e8400-e29b-41d4-a716-446655440000
removeBackground: true
video360Provider: FfmpegApi
backgroundRemovalProvider: ClipDrop
```

### GET /api/vehicle360/jobs/{id}

Obtiene el estado detallado de un job.

**Response (Processing):**

```json
{
  "jobId": "880e8400-e29b-41d4-a716-446655440000",
  "vehicleId": "550e8400-e29b-41d4-a716-446655440000",
  "status": "Processing",
  "progress": 65,
  "currentStep": "RemoveBackgrounds",
  "steps": [
    { "step": "UploadToS3", "status": "Completed", "durationMs": 5000 },
    {
      "step": "ExtractFrames",
      "status": "Completed",
      "durationMs": 45000,
      "cost": 0.011
    },
    {
      "step": "RemoveBackgrounds",
      "status": "Processing",
      "progress": 4,
      "total": 6
    },
    { "step": "GenerateThumbnails", "status": "Pending" },
    { "step": "CreateView", "status": "Pending" }
  ],
  "startedAt": "2026-01-27T10:00:05Z"
}
```

**Response (Completed):**

```json
{
  "jobId": "880e8400-e29b-41d4-a716-446655440000",
  "vehicleId": "550e8400-e29b-41d4-a716-446655440000",
  "status": "Completed",
  "progress": 100,
  "completedAt": "2026-01-27T10:03:25Z",
  "totalProcessingTimeMs": 200000,
  "totalCostUsd": 0.311,
  "costBreakdown": {
    "video360": 0.011,
    "backgroundRemoval": 0.3
  },
  "steps": [
    { "step": "UploadToS3", "status": "Completed", "durationMs": 5000 },
    { "step": "ExtractFrames", "status": "Completed", "durationMs": 45000 },
    { "step": "RemoveBackgrounds", "status": "Completed", "durationMs": 90000 },
    {
      "step": "GenerateThumbnails",
      "status": "Completed",
      "durationMs": 10000
    },
    { "step": "CreateView", "status": "Completed", "durationMs": 2000 }
  ],
  "result": {
    "viewId": "990e8400-e29b-41d4-a716-446655440000",
    "viewUrl": "/api/vehicle360/views/550e8400-e29b-41d4-a716-446655440000"
  }
}
```

### GET /api/vehicle360/views/{vehicleId} ⭐ (Para Frontend)

Obtiene los datos para renderizar el visor 360°.

**Response:**

```json
{
  "vehicleId": "550e8400-e29b-41d4-a716-446655440000",
  "isReady": true,
  "createdAt": "2026-01-27T10:03:25Z",
  "frames": [
    {
      "index": 0,
      "angle": 0,
      "name": "Front",
      "imageUrl": "https://cdn.okla.com.do/vehicles/123/360/nobg/frame_00.png",
      "thumbnailUrl": "https://cdn.okla.com.do/vehicles/123/360/thumbs/frame_00.png",
      "width": 1920,
      "height": 1080
    },
    {
      "index": 1,
      "angle": 60,
      "name": "Front-Right",
      "imageUrl": "https://cdn.okla.com.do/vehicles/123/360/nobg/frame_01.png",
      "thumbnailUrl": "https://cdn.okla.com.do/vehicles/123/360/thumbs/frame_01.png"
    },
    {
      "index": 2,
      "angle": 120,
      "name": "Rear-Right",
      "imageUrl": "https://cdn.okla.com.do/vehicles/123/360/nobg/frame_02.png",
      "thumbnailUrl": "https://cdn.okla.com.do/vehicles/123/360/thumbs/frame_02.png"
    },
    {
      "index": 3,
      "angle": 180,
      "name": "Rear",
      "imageUrl": "https://cdn.okla.com.do/vehicles/123/360/nobg/frame_03.png",
      "thumbnailUrl": "https://cdn.okla.com.do/vehicles/123/360/thumbs/frame_03.png"
    },
    {
      "index": 4,
      "angle": 240,
      "name": "Rear-Left",
      "imageUrl": "https://cdn.okla.com.do/vehicles/123/360/nobg/frame_04.png",
      "thumbnailUrl": "https://cdn.okla.com.do/vehicles/123/360/thumbs/frame_04.png"
    },
    {
      "index": 5,
      "angle": 300,
      "name": "Front-Left",
      "imageUrl": "https://cdn.okla.com.do/vehicles/123/360/nobg/frame_05.png",
      "thumbnailUrl": "https://cdn.okla.com.do/vehicles/123/360/thumbs/frame_05.png"
    }
  ],
  "config": {
    "autoRotate": true,
    "autoRotateSpeed": 5000,
    "allowDrag": true,
    "allowZoom": true,
    "showAngleIndicator": true,
    "preloadAll": true
  }
}
```

## 🔄 Flujo de Orquestación Detallado

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    VEHICLE360PROCESSINGSERVICE - PIPELINE                    │
└─────────────────────────────────────────────────────────────────────────────┘

╔══════════════════════════════════════════════════════════════════════════════╗
║  PASO 1: UPLOAD TO S3 (MediaService)                          ~5-30 segundos ║
╠══════════════════════════════════════════════════════════════════════════════╣
║                                                                              ║
║  1.1 Recibir video del request                                               ║
║      └── Validar formato: mp4, mov, avi, webm                                ║
║      └── Validar tamaño: < 500MB                                             ║
║      └── Validar duración: < 120 segundos                                    ║
║                                                                              ║
║  1.2 Llamar MediaService                                                     ║
║      ┌────────────────────────────────────────────────────────────────────┐  ║
║      │ POST http://mediaservice:8080/api/upload                           │  ║
║      │ Body: { file: video.mp4, folder: "vehicles/{id}/360" }             │  ║
║      │                                                                    │  ║
║      │ Response: {                                                        │  ║
║      │   "fileId": "abc123",                                              │  ║
║      │   "url": "https://cdn.okla.com.do/vehicles/123/360/original.mp4"   │  ║
║      │ }                                                                  │  ║
║      └────────────────────────────────────────────────────────────────────┘  ║
║                                                                              ║
║  1.3 Registrar Step: UploadToS3 = Completed                                  ║
║                                                                              ║
╚══════════════════════════════════════════════════════════════════════════════╝
                                    │
                                    ▼
╔══════════════════════════════════════════════════════════════════════════════╗
║  PASO 2: EXTRACT FRAMES (Video360Service)                    ~30-90 segundos ║
╠══════════════════════════════════════════════════════════════════════════════╣
║                                                                              ║
║  2.1 Llamar Video360Service                                                  ║
║      ┌────────────────────────────────────────────────────────────────────┐  ║
║      │ POST http://video360service:8080/api/video360/jobs                 │  ║
║      │ Body: {                                                            │  ║
║      │   "vehicleId": "550e8400...",                                      │  ║
║      │   "videoUrl": "https://cdn.okla.com.do/.../original.mp4",          │  ║
║      │   "frameCount": 6,                                                 │  ║
║      │   "imageFormat": "Jpeg",                                           │  ║
║      │   "preferredProvider": "FfmpegApi"                                 │  ║
║      │ }                                                                  │  ║
║      │                                                                    │  ║
║      │ Response: { "jobId": "660e8400...", "status": "Pending" }          │  ║
║      └────────────────────────────────────────────────────────────────────┘  ║
║                                                                              ║
║  2.2 Poll status hasta completado (máx 300s timeout)                         ║
║      ┌────────────────────────────────────────────────────────────────────┐  ║
║      │ GET http://video360service:8080/api/video360/jobs/{jobId}          │  ║
║      │                                                                    │  ║
║      │ while (status != "Completed" && status != "Failed") {              │  ║
║      │     await Task.Delay(2000);  // Poll cada 2 segundos              │  ║
║      │     status = await GetJobStatus(jobId);                            │  ║
║      │ }                                                                  │  ║
║      │                                                                    │  ║
║      │ Response final: {                                                  │  ║
║      │   "status": "Completed",                                           │  ║
║      │   "frames": [                                                      │  ║
║      │     { "index": 0, "imageUrl": ".../frame_00.jpg" },                │  ║
║      │     { "index": 1, "imageUrl": ".../frame_01.jpg" },                │  ║
║      │     ...6 frames total                                              │  ║
║      │   ],                                                               │  ║
║      │   "costUsd": 0.011                                                 │  ║
║      │ }                                                                  │  ║
║      └────────────────────────────────────────────────────────────────────┘  ║
║                                                                              ║
║  2.3 Registrar Step: ExtractFrames = Completed                               ║
║      └── Guardar URLs de los 6 frames extraídos                              ║
║      └── Acumular costo: $0.011                                              ║
║                                                                              ║
╚══════════════════════════════════════════════════════════════════════════════╝
                                    │
                                    ▼
╔══════════════════════════════════════════════════════════════════════════════╗
║  PASO 3: REMOVE BACKGROUNDS (BackgroundRemovalService)       ~60-180 segundos║
╠══════════════════════════════════════════════════════════════════════════════╣
║                                                                              ║
║  3.1 Llamar BackgroundRemovalService con batch de 6 imágenes                 ║
║      ┌────────────────────────────────────────────────────────────────────┐  ║
║      │ POST http://backgroundremovalservice:8080/api/removal/batch        │  ║
║      │ Body: {                                                            │  ║
║      │   "vehicleId": "550e8400...",                                      │  ║
║      │   "images": [                                                      │  ║
║      │     { "url": ".../frame_00.jpg", "index": 0 },                     │  ║
║      │     { "url": ".../frame_01.jpg", "index": 1 },                     │  ║
║      │     { "url": ".../frame_02.jpg", "index": 2 },                     │  ║
║      │     { "url": ".../frame_03.jpg", "index": 3 },                     │  ║
║      │     { "url": ".../frame_04.jpg", "index": 4 },                     │  ║
║      │     { "url": ".../frame_05.jpg", "index": 5 }                      │  ║
║      │   ],                                                               │  ║
║      │   "preferredProvider": "ClipDrop",                                 │  ║
║      │   "outputFormat": "Png"                                            │  ║
║      │ }                                                                  │  ║
║      │                                                                    │  ║
║      │ Response: { "batchId": "770e8400...", "status": "Processing" }     │  ║
║      └────────────────────────────────────────────────────────────────────┘  ║
║                                                                              ║
║  3.2 Poll batch status hasta completado (máx 180s timeout)                   ║
║      ┌────────────────────────────────────────────────────────────────────┐  ║
║      │ GET http://backgroundremovalservice:8080/api/removal/batch/{id}    │  ║
║      │                                                                    │  ║
║      │ Response final: {                                                  │  ║
║      │   "status": "Completed",                                           │  ║
║      │   "totalCostUsd": 0.30,                                            │  ║
║      │   "processedImages": [                                             │  ║
║      │     { "index": 0, "processedUrl": ".../nobg/frame_00.png" },       │  ║
║      │     { "index": 1, "processedUrl": ".../nobg/frame_01.png" },       │  ║
║      │     ...6 imágenes sin fondo                                        │  ║
║      │   ]                                                                │  ║
║      │ }                                                                  │  ║
║      └────────────────────────────────────────────────────────────────────┘  ║
║                                                                              ║
║  3.3 Registrar Step: RemoveBackgrounds = Completed                           ║
║      └── Guardar URLs de las 6 imágenes sin fondo                            ║
║      └── Acumular costo: $0.30 ($0.05 × 6)                                   ║
║                                                                              ║
╚══════════════════════════════════════════════════════════════════════════════╝
                                    │
                                    ▼
╔══════════════════════════════════════════════════════════════════════════════╗
║  PASO 4: GENERATE THUMBNAILS (MediaService)                   ~5-15 segundos ║
╠══════════════════════════════════════════════════════════════════════════════╣
║                                                                              ║
║  4.1 Para cada imagen sin fondo, generar thumbnail                           ║
║      ┌────────────────────────────────────────────────────────────────────┐  ║
║      │ POST http://mediaservice:8080/api/resize                           │  ║
║      │ Body: {                                                            │  ║
║      │   "sourceUrl": ".../nobg/frame_00.png",                            │  ║
║      │   "width": 400,                                                    │  ║
║      │   "height": 300,                                                   │  ║
║      │   "outputFolder": "vehicles/{id}/360/thumbs"                       │  ║
║      │ }                                                                  │  ║
║      │                                                                    │  ║
║      │ Response: { "thumbnailUrl": ".../thumbs/frame_00.png" }            │  ║
║      └────────────────────────────────────────────────────────────────────┘  ║
║                                                                              ║
║  4.2 Procesar las 6 imágenes en paralelo                                     ║
║                                                                              ║
║  4.3 Registrar Step: GenerateThumbnails = Completed                          ║
║                                                                              ║
╚══════════════════════════════════════════════════════════════════════════════╝
                                    │
                                    ▼
╔══════════════════════════════════════════════════════════════════════════════╗
║  PASO 5: CREATE VIEW (Base de Datos)                           ~1-2 segundos ║
╠══════════════════════════════════════════════════════════════════════════════╣
║                                                                              ║
║  5.1 Crear entidad Vehicle360View                                            ║
║      ┌────────────────────────────────────────────────────────────────────┐  ║
║      │ var view = new Vehicle360View                                      │  ║
║      │ {                                                                  │  ║
║      │     VehicleId = vehicleId,                                         │  ║
║      │     IsReady = true,                                                │  ║
║      │     Frames = processedFrames,  // 6 frames con URLs               │  ║
║      │     Config = defaultConfig                                         │  ║
║      │ };                                                                 │  ║
║      │ await _repository.AddAsync(view);                                  │  ║
║      └────────────────────────────────────────────────────────────────────┘  ║
║                                                                              ║
║  5.2 Registrar Step: CreateView = Completed                                  ║
║      └── Job status = Completed                                              ║
║      └── Calcular costo total: $0.311                                        ║
║                                                                              ║
║  5.3 Enviar webhook de completado (si callbackUrl configurada)               ║
║      ┌────────────────────────────────────────────────────────────────────┐  ║
║      │ POST {callbackUrl}                                                 │  ║
║      │ Body: {                                                            │  ║
║      │   "event": "vehicle360.completed",                                 │  ║
║      │   "jobId": "880e8400...",                                          │  ║
║      │   "vehicleId": "550e8400...",                                      │  ║
║      │   "viewUrl": "/api/vehicle360/views/550e8400...",                  │  ║
║      │   "totalCostUsd": 0.311                                            │  ║
║      │ }                                                                  │  ║
║      └────────────────────────────────────────────────────────────────────┘  ║
║                                                                              ║
╚══════════════════════════════════════════════════════════════════════════════╝
```

## 🛡️ Resiliencia con Polly

El orquestador implementa políticas de resiliencia para cada servicio externo:

### Configuración de Polly

```csharp
// Program.cs
builder.Services.AddHttpClient<IMediaServiceClient, MediaServiceClient>()
    .AddPolicyHandler(GetRetryPolicy())
    .AddPolicyHandler(GetCircuitBreakerPolicy())
    .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(120)));

builder.Services.AddHttpClient<IVideo360ServiceClient, Video360ServiceClient>()
    .AddPolicyHandler(GetRetryPolicy())
    .AddPolicyHandler(GetCircuitBreakerPolicy())
    .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(300)));

builder.Services.AddHttpClient<IBackgroundRemovalServiceClient, BackgroundRemovalServiceClient>()
    .AddPolicyHandler(GetRetryPolicy())
    .AddPolicyHandler(GetCircuitBreakerPolicy())
    .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(180)));
```

### Política de Reintentos

```csharp
private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // 2s, 4s, 8s
            onRetry: (outcome, timespan, retryAttempt, context) =>
            {
                _logger.LogWarning(
                    "Reintento {RetryAttempt} después de {Delay}ms. Razón: {Reason}",
                    retryAttempt,
                    timespan.TotalMilliseconds,
                    outcome.Exception?.Message ?? outcome.Result.StatusCode.ToString());
            });
}
```

### Circuit Breaker

```csharp
private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 5,  // 5 fallos consecutivos
            durationOfBreak: TimeSpan.FromSeconds(30), // 30 segundos abierto
            onBreak: (outcome, breakDuration) =>
            {
                _logger.LogError(
                    "Circuit breaker ABIERTO por {Duration}s. Razón: {Reason}",
                    breakDuration.TotalSeconds,
                    outcome.Exception?.Message);
            },
            onReset: () =>
            {
                _logger.LogInformation("Circuit breaker CERRADO. Servicio recuperado.");
            });
}
```

### Timeouts por Servicio

| Servicio                 | Timeout | Justificación                             |
| ------------------------ | ------- | ----------------------------------------- |
| MediaService             | 120s    | Upload de videos grandes (hasta 500MB)    |
| Video360Service          | 300s    | Procesamiento de video puede tomar tiempo |
| BackgroundRemovalService | 180s    | Procesa 6 imágenes en batch               |

## 📊 Entidades de Dominio

### Vehicle360ProcessingJob

```csharp
public class Vehicle360ProcessingJob
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public Guid? UserId { get; set; }
    public string? TenantId { get; set; }

    // Source video
    public string SourceVideoUrl { get; set; }
    public double VideoDurationSeconds { get; set; }

    // Processing options
    public int FrameCount { get; set; } = 6;
    public string ImageFormat { get; set; } = "Jpeg";
    public bool RemoveBackground { get; set; } = true;
    public string Video360Provider { get; set; } = "FfmpegApi";
    public string BackgroundRemovalProvider { get; set; } = "ClipDrop";
    public bool GenerateThumbnails { get; set; } = true;

    // Status
    public ProcessingJobStatus Status { get; set; }
    public int Progress { get; set; }
    public string? CurrentStep { get; set; }
    public string? ErrorMessage { get; set; }

    // Steps
    public List<ProcessingStep> Steps { get; set; } = new();

    // Results
    public Guid? Vehicle360ViewId { get; set; }
    public Vehicle360View? Vehicle360View { get; set; }
    public decimal? TotalCostUsd { get; set; }
    public long? TotalProcessingTimeMs { get; set; }

    // Callback
    public string? CallbackUrl { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
```

### ProcessingStep

```csharp
public class ProcessingStep
{
    public Guid Id { get; set; }
    public Guid Vehicle360ProcessingJobId { get; set; }

    public ProcessingStepType StepType { get; set; }
    public ProcessingStepStatus Status { get; set; }
    public int Order { get; set; }

    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public long? DurationMs { get; set; }
    public decimal? CostUsd { get; set; }
    public string? ErrorMessage { get; set; }

    // External references
    public string? ExternalJobId { get; set; }  // Job ID del servicio externo
    public string? ResultData { get; set; }     // JSON con resultado
}

public enum ProcessingStepType
{
    UploadToS3 = 0,
    ExtractFrames = 1,
    RemoveBackgrounds = 2,
    GenerateThumbnails = 3,
    CreateView = 4
}
```

### Vehicle360View

```csharp
public class Vehicle360View
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }

    public bool IsReady { get; set; }
    public List<ProcessedFrame> Frames { get; set; } = new();
    public Vehicle360ViewConfig Config { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ProcessedFrame
{
    public int Index { get; set; }
    public int AngleDegrees { get; set; }
    public string AngleName { get; set; }

    public string ImageUrl { get; set; }
    public string ThumbnailUrl { get; set; }

    public int Width { get; set; }
    public int Height { get; set; }
}

public class Vehicle360ViewConfig
{
    public bool AutoRotate { get; set; } = true;
    public int AutoRotateSpeed { get; set; } = 5000;  // ms entre frames
    public bool AllowDrag { get; set; } = true;
    public bool AllowZoom { get; set; } = true;
    public bool ShowAngleIndicator { get; set; } = true;
    public bool PreloadAll { get; set; } = true;
}
```

## 🔧 Configuración

### appsettings.json

```json
{
  "ServiceClients": {
    "MediaService": {
      "BaseUrl": "http://mediaservice:8080",
      "TimeoutSeconds": 120
    },
    "Video360Service": {
      "BaseUrl": "http://video360service:8080",
      "TimeoutSeconds": 300
    },
    "BackgroundRemovalService": {
      "BaseUrl": "http://backgroundremovalservice:8080",
      "TimeoutSeconds": 180
    }
  },
  "Processing": {
    "DefaultFrameCount": 6,
    "DefaultImageFormat": "Jpeg",
    "DefaultRemoveBackground": true,
    "DefaultVideo360Provider": "FfmpegApi",
    "DefaultBackgroundRemovalProvider": "ClipDrop",
    "DefaultGenerateThumbnails": true,
    "ThumbnailWidth": 400,
    "ThumbnailHeight": 300,
    "MaxVideoSizeMb": 500,
    "MaxVideoDurationSeconds": 120
  },
  "Resilience": {
    "RetryCount": 3,
    "RetryDelaySeconds": 2,
    "CircuitBreakerFailures": 5,
    "CircuitBreakerDurationSeconds": 30
  },
  "Worker": {
    "PollingIntervalSeconds": 5,
    "MaxConcurrentJobs": 10
  }
}
```

## 🧪 Tests

```bash
cd Vehicle360ProcessingService.Tests
dotnet test

# Resultados esperados:
# Passed!  - Failed: 0, Passed: 35, Skipped: 0
```

---

**Anterior:** [03-BACKGROUNDREMOVALSERVICE.md](./03-BACKGROUNDREMOVALSERVICE.md)  
**Siguiente:** [05-INTEGRACION-FRONTEND.md](./05-INTEGRACION-FRONTEND.md)
