# Vehicle 360 Processing Service - Orchestrator

Microservicio orquestador que coordina el procesamiento completo de vistas 360° de vehículos.

## 📋 Descripción

Este servicio es el **punto único de entrada** para el frontend cuando se desea procesar un video 360° de un vehículo. Coordina la comunicación con:

1. **MediaService** - Almacenamiento de video e imágenes en S3
2. **Video360Service** - Extracción de 6 frames del video
3. **BackgroundRemovalService** - Remoción de fondos de las imágenes

## 🏗️ Arquitectura

```
┌─────────────────┐
│    Frontend     │
│  (React/Flutter)│
└────────┬────────┘
         │ POST /api/vehicle360processing/process
         ▼
┌─────────────────────────────────────────────────┐
│        Vehicle360ProcessingService              │
│              (Orchestrator)                      │
│                                                  │
│  ┌─────────────────────────────────────────┐   │
│  │         Polly Resilience                 │   │
│  │  • Retry (exponential backoff)           │   │
│  │  • Circuit Breaker                       │   │
│  │  • Timeout                               │   │
│  └─────────────────────────────────────────┘   │
└───────┬──────────────┬──────────────┬──────────┘
        │              │              │
        ▼              ▼              ▼
┌───────────────┐ ┌───────────┐ ┌──────────────────┐
│ MediaService  │ │ Video360  │ │BackgroundRemoval │
│  (S3 Upload)  │ │  Service  │ │    Service       │
└───────────────┘ └───────────┘ └──────────────────┘
```

## 🚀 Flujo de Procesamiento

1. **Frontend** envía video + vehicleId
2. **Orchestrator** sube video a S3 (MediaService)
3. **Orchestrator** envía URL del video a Video360Service
4. **Video360Service** extrae 6 frames y los retorna
5. **Orchestrator** envía cada frame a BackgroundRemovalService
6. **BackgroundRemovalService** procesa y retorna imágenes sin fondo
7. **Orchestrator** sube imágenes finales a S3 (MediaService)
8. **Frontend** recibe URLs de las 6 imágenes procesadas

## 📡 API Endpoints

### POST /api/vehicle360processing/process

Inicia el procesamiento de un video 360°.

**Request (multipart/form-data):**

```
video: [File] - Video del vehículo (MP4, MOV, AVI, WebM)
vehicleId: [Guid] - ID del vehículo
frameCount: [int] - Número de frames (default: 6)
outputWidth: [int] - Ancho de salida (default: 1920)
outputHeight: [int] - Alto de salida (default: 1080)
outputFormat: [string] - Formato (png, jpg, webp)
smartFrameSelection: [bool] - Selección inteligente de frames
autoCorrectExposure: [bool] - Corrección automática de exposición
generateThumbnails: [bool] - Generar thumbnails
backgroundColor: [string] - Color de fondo (transparent, white, etc.)
```

**Response:**

```json
{
  "jobId": "550e8400-e29b-41d4-a716-446655440000",
  "status": "Queued",
  "message": "Video uploaded successfully. Processing queued.",
  "queuePosition": 1,
  "estimatedWaitSeconds": 120
}
```

### GET /api/vehicle360processing/jobs/{jobId}/status

Obtiene el estado de un job.

**Response:**

```json
{
  "jobId": "550e8400-e29b-41d4-a716-446655440000",
  "vehicleId": "660e8400-e29b-41d4-a716-446655440000",
  "status": "RemovingBackgrounds",
  "progress": 65,
  "isComplete": false,
  "isFailed": false,
  "currentStep": "Removing backgrounds"
}
```

### GET /api/vehicle360processing/viewer/{vehicleId}

Obtiene los datos del visor 360° para un vehículo.

**Response:**

```json
{
  "vehicleId": "660e8400-e29b-41d4-a716-446655440000",
  "isReady": true,
  "totalFrames": 6,
  "primaryImageUrl": "https://s3.../frame_01.png",
  "frames": [
    {
      "index": 0,
      "angle": 0,
      "name": "Front",
      "imageUrl": "https://s3.../frame_01.png",
      "thumbnailUrl": "https://s3.../thumb_01.png"
    },
    ...
  ],
  "config": {
    "autoRotate": true,
    "autoRotateSpeed": 5000,
    "allowDrag": true,
    "showThumbnails": true,
    "hasTransparentBackground": true
  }
}
```

## 🛡️ Resiliencia (Polly)

### Retry Policy

- 3 reintentos con backoff exponencial
- Delays: 2s, 4s, 8s

### Circuit Breaker

- Se abre después de 5 fallos consecutivos
- Duración del corte: 30 segundos
- Estados: Closed → Open → Half-Open → Closed

### Timeout

- MediaService: 120s
- Video360Service: 300s (5 min)
- BackgroundRemovalService: 180s (3 min)

## 🔧 Configuración

```json
{
  "Services": {
    "MediaService": {
      "BaseUrl": "http://mediaservice:8080",
      "TimeoutSeconds": 120,
      "RetryCount": 3,
      "CircuitBreakerThreshold": 5,
      "CircuitBreakerDurationSeconds": 30
    },
    "Video360Service": {
      "BaseUrl": "http://video360service:8080",
      "TimeoutSeconds": 300,
      "MaxProcessingMinutes": 10,
      "PollIntervalSeconds": 5
    },
    "BackgroundRemovalService": {
      "BaseUrl": "http://backgroundremovalservice:8080",
      "TimeoutSeconds": 180,
      "MaxProcessingMinutes": 5,
      "PollIntervalSeconds": 2
    }
  }
}
```

## 🐳 Docker

```bash
# Build
docker build -t vehicle360processing:latest .

# Run
docker run -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="Host=postgres;..." \
  -e Services__MediaService__BaseUrl="http://mediaservice:8080" \
  vehicle360processing:latest
```

## 🧪 Tests

```bash
cd Vehicle360ProcessingService.Tests
dotnet test --verbosity normal
```

## 📊 Health Checks

- `/health` - Estado general
- `/health/ready` - Listo para recibir tráfico (DB)
- `/health/live` - Liveness probe
- `/api/vehicle360processing/health/services` - Estado de servicios dependientes

## 📁 Estructura del Proyecto

```
Vehicle360ProcessingService/
├── Vehicle360ProcessingService.Api/
│   ├── Controllers/
│   │   └── Vehicle360ProcessingController.cs
│   ├── Program.cs
│   └── appsettings.json
├── Vehicle360ProcessingService.Application/
│   ├── DTOs/
│   ├── Features/
│   │   ├── Commands/
│   │   ├── Queries/
│   │   └── Handlers/
│   └── Validators/
├── Vehicle360ProcessingService.Domain/
│   ├── Entities/
│   │   └── Vehicle360Job.cs
│   └── Interfaces/
│       ├── IVehicle360JobRepository.cs
│       ├── IMediaServiceClient.cs
│       ├── IVideo360ServiceClient.cs
│       └── IBackgroundRemovalClient.cs
├── Vehicle360ProcessingService.Infrastructure/
│   ├── HttpClients/
│   │   ├── MediaServiceHttpClient.cs
│   │   ├── Video360ServiceHttpClient.cs
│   │   └── BackgroundRemovalHttpClient.cs
│   ├── Persistence/
│   │   ├── Vehicle360ProcessingDbContext.cs
│   │   └── Vehicle360JobRepository.cs
│   └── DependencyInjection.cs
├── Vehicle360ProcessingService.Tests/
├── Dockerfile
└── README.md
```

## 🗄️ Base de Datos

**Tabla: vehicle_360_jobs**

| Columna            | Tipo          | Descripción                |
| ------------------ | ------------- | -------------------------- |
| id                 | UUID          | Primary key                |
| vehicle_id         | UUID          | ID del vehículo            |
| user_id            | UUID          | ID del usuario             |
| status             | VARCHAR(50)   | Estado del job             |
| progress           | INT           | Progreso 0-100             |
| error_message      | VARCHAR(2000) | Mensaje de error           |
| frame_count        | INT           | Número de frames           |
| original_video_url | VARCHAR(2000) | URL del video original     |
| options            | JSONB         | Opciones de procesamiento  |
| processed_frames   | JSONB         | Array de frames procesados |
| created_at         | TIMESTAMP     | Fecha de creación          |
| completed_at       | TIMESTAMP     | Fecha de completado        |

## 📈 Métricas

El servicio expone métricas compatibles con Prometheus en `/metrics`:

- `vehicle360_jobs_total` - Total de jobs procesados
- `vehicle360_jobs_duration_seconds` - Duración del procesamiento
- `vehicle360_service_requests_total` - Requests a servicios externos
- `vehicle360_circuit_breaker_state` - Estado de circuit breakers
