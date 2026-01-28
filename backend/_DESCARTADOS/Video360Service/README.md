# Video 360 Service

Microservicio para procesar videos 360 de vehículos y extraer N imágenes equidistantes para crear un visor 360° interactivo.

## 📋 Descripción

Este servicio permite:

- Subir un video 360° de un vehículo (grabado alrededor del carro)
- Extraer automáticamente 6 (o N) frames equidistantes
- Aplicar correcciones automáticas de exposición
- Selección inteligente del mejor frame en cada posición
- Generar miniaturas
- Almacenar las imágenes en S3/MinIO

## 🏗️ Arquitectura

```
┌─────────────────────────────────────────────────────────────────────┐
│                        Video 360 Service                            │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  ┌─────────────────┐        ┌─────────────────────────────────┐    │
│  │  .NET 8 API     │───────▶│  Python Worker (OpenCV)         │    │
│  │  (Controllers)  │        │  - Extrae frames                │    │
│  │                 │◀───────│  - Corrige exposición           │    │
│  │  Port: 8080     │        │  - Selección inteligente        │    │
│  └────────┬────────┘        │                                 │    │
│           │                 │  Port: 8000                     │    │
│           │                 └─────────────────────────────────┘    │
│           │                                                        │
│           ▼                                                        │
│  ┌─────────────────┐        ┌─────────────────────────────────┐    │
│  │  PostgreSQL     │        │  S3 / MinIO                     │    │
│  │  (Jobs, Frames) │        │  (Imágenes, Thumbnails)         │    │
│  └─────────────────┘        └─────────────────────────────────┘    │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

## 🚀 Quick Start

### Desarrollo Local

```bash
# 1. Levantar todos los servicios con Docker Compose
cd backend/Video360Service
docker-compose up -d

# 2. Verificar que están corriendo
docker-compose ps

# 3. Probar el health check
curl http://localhost:5070/health
curl http://localhost:8000/health
```

### Solo el Worker Python

```bash
cd backend/Video360Service/workers

# Instalar dependencias
pip install -r requirements.txt

# Correr el servidor API
python api.py

# O procesar un video directamente
python video360_processor.py input.mp4 ./output '{"frame_count": 6}'
```

## 📡 API Endpoints

### Upload Video

```bash
POST /api/video360/upload
Content-Type: multipart/form-data

# Form Fields:
- file: Video file (mp4, mov, avi, webm, mkv - max 500MB)
- vehicleId: UUID del vehículo
- frameCount: Número de frames (4-12, default 6)
- outputWidth: Ancho de salida (default 1920)
- outputHeight: Alto de salida (default 1080)
- jpegQuality: Calidad JPEG 1-100 (default 90)
- smartFrameSelection: true/false (default true)
- autoCorrectExposure: true/false (default true)
- generateThumbnails: true/false (default true)
```

**Response:**

```json
{
  "jobId": "550e8400-e29b-41d4-a716-446655440000",
  "message": "Video recibido correctamente. Procesamiento en cola.",
  "status": "Queued",
  "queuePosition": 1,
  "estimatedWaitSeconds": 60
}
```

### Get Job Status

```bash
GET /api/video360/jobs/{jobId}/status
```

**Response:**

```json
{
  "jobId": "550e8400-e29b-41d4-a716-446655440000",
  "status": "Completed",
  "statusName": "Completed",
  "progress": 100,
  "isComplete": true
}
```

### Get Vehicle 360 Viewer

```bash
GET /api/video360/vehicles/{vehicleId}/viewer
```

**Response:**

```json
{
  "vehicleId": "...",
  "jobId": "...",
  "totalFrames": 6,
  "primaryImageUrl": "https://media.okla.com.do/vehicles/.../frame_01.jpg",
  "frames": [
    {
      "index": 0,
      "angle": 0,
      "name": "Frente",
      "imageUrl": "https://media.okla.com.do/.../frame_01_frente.jpg",
      "thumbnailUrl": "https://media.okla.com.do/.../thumb_01.jpg"
    },
    {
      "index": 1,
      "angle": 60,
      "name": "Frente-Derecha",
      "imageUrl": "https://media.okla.com.do/.../frame_02_frente_derecha.jpg"
    }
    // ... 4 más
  ]
}
```

## 🔧 Configuración

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=postgres;Database=video360db;Username=postgres;Password=postgres"
  },
  "Video360Processor": {
    "PythonServiceUrl": "http://video360-worker:8000",
    "UseHttpService": true,
    "TimeoutSeconds": 300
  },
  "S3Storage": {
    "BucketName": "okla-media",
    "Region": "us-east-1",
    "CdnBaseUrl": "https://media.okla.com.do"
  }
}
```

## 🖼️ Proceso de Extracción

El worker Python (OpenCV) realiza:

1. **Análisis del video**: Obtiene duración, FPS, resolución
2. **División equidistante**: Calcula N posiciones a 360°/N grados
3. **Selección inteligente**: En cada posición, muestrea 5 frames y selecciona el de mayor nitidez
4. **Corrección de exposición**: Aplica CLAHE para mejorar brillo/contraste
5. **Redimensionado**: Ajusta a resolución de salida manteniendo aspecto
6. **Generación de thumbnails**: Crea versiones pequeñas para preview

### Vistas Estándar (6 frames)

| #   | Vista          | Ángulo |
| --- | -------------- | ------ |
| 1   | Frente         | 0°     |
| 2   | Frente-Derecha | 60°    |
| 3   | Derecha        | 120°   |
| 4   | Atrás-Derecha  | 180°   |
| 5   | Atrás          | 240°   |
| 6   | Izquierda      | 300°   |

## 📊 Métricas de Calidad

Cada frame extraído incluye un **Quality Score** (0-100) basado en:

- **Nitidez** (50%): Varianza del Laplaciano
- **Contraste** (30%): Desviación estándar del histograma
- **Brillo** (20%): Proximidad a valor medio ideal (127)

## 🧪 Testing

```bash
# Correr tests
cd backend/Video360Service
dotnet test

# Con coverage
dotnet test /p:CollectCoverage=true
```

## 📁 Estructura del Proyecto

```
Video360Service/
├── Video360Service.Domain/           # Entidades y contratos
│   ├── Entities/
│   │   ├── Video360Job.cs
│   │   ├── ExtractedFrame.cs
│   │   └── ProcessingOptions.cs
│   ├── Enums/
│   │   └── Video360JobStatus.cs
│   └── Interfaces/
│       ├── IVideo360JobRepository.cs
│       ├── IVideo360Processor.cs
│       └── IStorageService.cs
├── Video360Service.Application/      # Lógica de negocio
│   ├── DTOs/
│   ├── Features/
│   │   ├── Commands/
│   │   ├── Queries/
│   │   └── Handlers/
│   └── Validators/
├── Video360Service.Infrastructure/   # Implementaciones
│   ├── Persistence/
│   │   ├── Video360DbContext.cs
│   │   └── Repositories/
│   └── Services/
│       ├── Video360Processor.cs
│       └── S3StorageService.cs
├── Video360Service.Api/              # Controllers REST
│   ├── Controllers/
│   ├── Program.cs
│   └── appsettings.json
├── Video360Service.Tests/            # Tests unitarios
├── workers/                          # Python worker
│   ├── video360_processor.py
│   ├── api.py
│   ├── requirements.txt
│   └── Dockerfile
├── Dockerfile                        # .NET API
├── docker-compose.yml
└── README.md
```

## 🔄 Flujo de Datos

```
1. Cliente sube video → POST /api/video360/upload
                              │
2. API guarda en temp  ◄──────┘
                              │
3. Crea Job en DB      ◄──────┘ (status: Queued)
                              │
4. Worker Python       ◄──────┘
   - Descarga video
   - Extrae frames
   - Corrige exposición
   - Genera thumbnails
                              │
5. Sube a S3           ◄──────┘
                              │
6. Actualiza DB        ◄──────┘ (status: Completed)
                              │
7. Cliente consulta    ─────────▶ GET /vehicles/{id}/viewer
   y obtiene URLs
```

## 🚀 Deploy a Kubernetes

```yaml
# k8s/deployments.yaml (añadir)
---
apiVersion: apps/v1
kind: Deployment
metadata:
  name: video360service
  namespace: okla
spec:
  replicas: 1
  selector:
    matchLabels:
      app: video360service
  template:
    metadata:
      labels:
        app: video360service
    spec:
      containers:
        - name: video360service
          image: ghcr.io/gregorymorenoiem/cardealer-video360service:latest
          ports:
            - containerPort: 8080
          env:
            - name: ConnectionStrings__DefaultConnection
              valueFrom:
                secretKeyRef:
                  name: okla-secrets
                  key: video360-db-connection
---
apiVersion: apps/v1
kind: Deployment
metadata:
  name: video360-worker
  namespace: okla
spec:
  replicas: 2
  selector:
    matchLabels:
      app: video360-worker
  template:
    metadata:
      labels:
        app: video360-worker
    spec:
      containers:
        - name: video360-worker
          image: ghcr.io/gregorymorenoiem/cardealer-video360-worker:latest
          ports:
            - containerPort: 8000
          resources:
            requests:
              memory: "512Mi"
              cpu: "500m"
            limits:
              memory: "2Gi"
              cpu: "2000m"
```

## 📝 Licencia

Propiedad de OKLA - Uso interno
