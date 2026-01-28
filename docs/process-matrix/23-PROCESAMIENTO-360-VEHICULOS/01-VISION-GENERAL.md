# 🎯 Visión General - Sistema de Procesamiento 360° de Vehículos

## 📋 Descripción del Sistema

El sistema de procesamiento 360° de vehículos de OKLA permite a los usuarios subir un video de un vehículo girando y obtener automáticamente una experiencia interactiva 360° en la plataforma.

### ¿Qué problema resuelve?

1. **UX Mejorada**: Los compradores pueden ver el vehículo desde todos los ángulos
2. **Automatización**: No requiere edición manual de fotos
3. **Consistencia**: Todas las vistas 360° tienen el mismo formato profesional
4. **Escalabilidad**: Procesa cientos de vehículos automáticamente

## 🏗️ Microservicios Involucrados

| Microservicio                   | Puerto | Función                        |
| ------------------------------- | ------ | ------------------------------ |
| **Vehicle360ProcessingService** | 8080   | Orquestador principal          |
| **Video360Service**             | 8080   | Extracción de frames del video |
| **BackgroundRemovalService**    | 8080   | Remoción de fondos de imágenes |
| **MediaService**                | 8080   | Almacenamiento en S3/CDN       |

## 📊 Arquitectura Detallada

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                                 FRONTEND                                     │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ┌─────────────────────────┐     ┌─────────────────────────────────────┐   │
│  │     React Web App       │     │       Flutter Mobile App            │   │
│  │  • VehicleDetail page   │     │  • Vehicle360Viewer widget          │   │
│  │  • Upload360Video comp  │     │  • Upload360Video screen            │   │
│  │  • Vehicle360Viewer     │     │  • Processing status indicator      │   │
│  └─────────────────────────┘     └─────────────────────────────────────┘   │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
                                      │
                                      │ HTTPS / JWT Auth
                                      ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                              API GATEWAY                                     │
│                              (Ocelot)                                        │
│  • Rate Limiting            • Authentication           • Load Balancing     │
└─────────────────────────────────────────────────────────────────────────────┘
                                      │
                 ┌────────────────────┼────────────────────┐
                 ▼                    ▼                    ▼
┌────────────────────────┐ ┌──────────────────┐ ┌─────────────────────────┐
│ Vehicle360Processing   │ │   Video360       │ │  BackgroundRemoval      │
│ Service                │ │   Service        │ │  Service                │
│ ─────────────────────  │ │ ────────────     │ │ ─────────────────────   │
│ • Orquesta el flujo    │ │ • Extrae frames  │ │ • Remueve fondos        │
│ • Tracking de estado   │ │ • 5 proveedores  │ │ • 6 proveedores         │
│ • Polly resilience     │ │ • Fallback auto  │ │ • Fallback auto         │
│ • Notificaciones       │ │ • 6 frames       │ │ • Strategy pattern      │
└────────────────────────┘ └──────────────────┘ └─────────────────────────┘
         │                         │                        │
         └─────────────────────────┼────────────────────────┘
                                   ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                              MediaService                                    │
│                         (S3 Storage + CDN)                                   │
│                                                                             │
│  Buckets:                                                                   │
│  • okla-videos/ → Videos originales                                        │
│  • okla-images/ → Frames extraídos                                         │
│  • okla-processed/ → Imágenes sin fondo                                    │
│                                                                             │
│  CDN: https://cdn.okla.com.do/                                             │
└─────────────────────────────────────────────────────────────────────────────┘
                                   │
                                   ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                              PostgreSQL                                      │
│                                                                             │
│  Databases:                                                                 │
│  • vehicle360processingservice → Jobs de orquestación                      │
│  • video360service → Jobs de extracción de frames                          │
│  • backgroundremovalservice → Jobs de remoción de fondos                   │
└─────────────────────────────────────────────────────────────────────────────┘
```

## 🔄 Flujo de Procesamiento Paso a Paso

### Paso 1: Usuario Sube Video

```
Usuario (Frontend)
    │
    │ 1. Selecciona archivo de video (MP4, MOV, AVI, WebM)
    │ 2. Click "Subir Video 360°"
    │
    ▼
POST /api/vehicle360processing/process
    Headers: Authorization: Bearer {jwt_token}
    Body: multipart/form-data
    ├── video: [archivo.mp4]
    ├── vehicleId: "uuid"
    ├── frameCount: 6
    ├── outputFormat: "png"
    └── backgroundColor: "transparent"
```

### Paso 2: Orquestador Recibe y Valida

```
Vehicle360ProcessingService
    │
    │ 1. Valida formato de video (mp4, mov, avi, webm)
    │ 2. Valida tamaño (<500MB)
    │ 3. Valida duración (<60 segundos)
    │ 4. Crea Vehicle360Job en DB (status: Queued)
    │
    ▼
Response: {
    "jobId": "uuid",
    "status": "Queued",
    "queuePosition": 1,
    "estimatedWaitSeconds": 120
}
```

### Paso 3: Upload a S3

```
Orquestador → MediaService
    │
    │ POST /api/media/upload
    │ Body: multipart/form-data
    │ ├── file: [video bytes]
    │ └── path: "videos/{vehicleId}/original.mp4"
    │
    │ Retry Policy: 3 intentos, backoff 2s, 4s, 8s
    │ Timeout: 120 segundos
    │
    ▼
Response: {
    "url": "https://s3.amazonaws.com/okla-videos/...",
    "cdnUrl": "https://cdn.okla.com.do/videos/..."
}

Job Status Update: "Uploading" → "VideoUploaded"
```

### Paso 4: Extracción de Frames

```
Orquestador → Video360Service
    │
    │ POST /api/video360/jobs
    │ Body: {
    │     "vehicleId": "uuid",
    │     "videoUrl": "https://cdn.okla.com.do/videos/...",
    │     "frameCount": 6,
    │     "imageFormat": "Jpeg",
    │     "videoQuality": "High"
    │ }
    │
    │ Retry Policy: 3 intentos
    │ Timeout: 300 segundos (5 min)
    │
    ▼
Video360Service internamente:
    │
    │ 1. Selecciona proveedor (por prioridad y disponibilidad)
    │    FFmpeg-API → ApyHub → Cloudinary → Imgix → Shotstack
    │
    │ 2. Envía video al proveedor seleccionado
    │
    │ 3. Extrae 6 frames equidistantes:
    │    ┌─────────────────────────────────────────────┐
    │    │ Frame 0: 0°   → Frente                     │
    │    │ Frame 1: 60°  → Frente-Derecha             │
    │    │ Frame 2: 120° → Atrás-Derecha              │
    │    │ Frame 3: 180° → Atrás                      │
    │    │ Frame 4: 240° → Atrás-Izquierda            │
    │    │ Frame 5: 300° → Frente-Izquierda           │
    │    └─────────────────────────────────────────────┘
    │
    │ 4. Guarda frames en S3
    │
    ▼
Response: {
    "jobId": "uuid",
    "status": "Completed",
    "frames": [
        { "index": 0, "angle": 0, "imageUrl": "https://..." },
        { "index": 1, "angle": 60, "imageUrl": "https://..." },
        ...
    ]
}

Job Status Update: "ExtractingFrames" → "FramesExtracted"
```

### Paso 5: Remoción de Fondos

```
Orquestador → BackgroundRemovalService (×6 frames)
    │
    │ Para cada frame (0-5):
    │ POST /api/backgroundremoval/remove
    │ Body: {
    │     "imageUrl": "https://cdn.okla.com.do/images/frame_0.jpg",
    │     "outputFormat": 1,  // PNG
    │     "objectType": "car"
    │ }
    │
    │ Procesamiento paralelo: hasta 3 frames simultáneos
    │ Retry Policy: 3 intentos por frame
    │ Timeout: 180 segundos (3 min) por frame
    │
    ▼
BackgroundRemovalService internamente:
    │
    │ 1. Selecciona proveedor (por prioridad):
    │    ClipDrop → Remove.bg → Photoroom → Slazzer
    │
    │ 2. Envía imagen al proveedor
    │
    │ 3. Recibe imagen sin fondo (PNG transparente)
    │
    │ 4. Retorna URL de imagen procesada
    │
    ▼
Response: {
    "jobId": "uuid",
    "status": "Completed",
    "outputUrl": "https://cdn.okla.com.do/processed/..."
}

Job Status Update: "RemovingBackgrounds" (progress: 0% → 100%)
```

### Paso 6: Almacenamiento Final

```
Orquestador → MediaService
    │
    │ Las imágenes ya están en S3 desde BackgroundRemovalService
    │ Orquestador actualiza Vehicle360Job con URLs finales
    │
    ▼
Job Status Update: "Completed"

Final Data:
{
    "vehicleId": "uuid",
    "frames": [
        {
            "index": 0,
            "angle": 0,
            "name": "Front",
            "imageUrl": "https://cdn.okla.com.do/vehicles/{id}/360/frame_01.png",
            "thumbnailUrl": "https://cdn.okla.com.do/vehicles/{id}/360/thumb_01.png"
        },
        ...
    ]
}
```

### Paso 7: Frontend Muestra Vista 360°

```
Frontend
    │
    │ GET /api/vehicle360processing/viewer/{vehicleId}
    │
    ▼
Response: {
    "vehicleId": "uuid",
    "isReady": true,
    "totalFrames": 6,
    "primaryImageUrl": "https://cdn.okla.com.do/vehicles/.../frame_01.png",
    "frames": [...],
    "config": {
        "autoRotate": true,
        "autoRotateSpeed": 5000,
        "allowDrag": true,
        "showThumbnails": true,
        "hasTransparentBackground": true
    }
}
    │
    ▼
Componente Vehicle360Viewer renderiza:
    ┌─────────────────────────────────────────────┐
    │                                             │
    │     ◄──────  [IMAGEN 360°]  ──────►        │
    │              Drag to rotate                 │
    │                                             │
    │     ○ ○ ○ ● ○ ○  (indicadores de posición) │
    │                                             │
    └─────────────────────────────────────────────┘
```

## ⏱️ Tiempos de Procesamiento

| Etapa                   | Tiempo Estimado  | Timeout |
| ----------------------- | ---------------- | ------- |
| Upload a S3             | 5-30 segundos    | 120s    |
| Extracción de Frames    | 30-120 segundos  | 300s    |
| Remoción de Fondos (×6) | 60-180 segundos  | 180s ×6 |
| **Total**               | **~2-5 minutos** | -       |

## 🔒 Seguridad

- **Autenticación**: JWT Bearer token requerido
- **Autorización**: Usuario solo puede procesar sus vehículos
- **Rate Limiting**: 10 videos por hora por usuario
- **Validación**: Tamaño máximo 500MB, duración máxima 60s
- **Virus Scan**: MediaService escanea archivos subidos

## 📊 Métricas y Monitoreo

| Métrica                              | Descripción                     |
| ------------------------------------ | ------------------------------- |
| `vehicle360_jobs_total`              | Total de jobs procesados        |
| `vehicle360_jobs_duration_seconds`   | Duración del procesamiento      |
| `vehicle360_provider_requests_total` | Requests a proveedores externos |
| `vehicle360_provider_errors_total`   | Errores por proveedor           |
| `vehicle360_circuit_breaker_state`   | Estado de circuit breakers      |

## 🚨 Manejo de Errores

### Errores Recuperables (con retry)

- Timeout de proveedor externo
- Rate limit temporal
- Error de red transitorio

### Errores No Recuperables

- Video corrupto o formato inválido
- Créditos agotados en todos los proveedores
- Video muy largo o muy grande

### Fallback Automático

```
Si FFmpeg-API falla:
    ├── Intento 1: FFmpeg-API (falló)
    ├── Intento 2: ApyHub (disponible? usar)
    ├── Intento 3: Cloudinary (disponible? usar)
    ├── Intento 4: Imgix (disponible? usar)
    └── Intento 5: Shotstack (último recurso)
```

---

**Siguiente:** [02-VIDEO360SERVICE.md](./02-VIDEO360SERVICE.md)
