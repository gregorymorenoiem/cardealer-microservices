# 📸 Media 360° y Video Tour

> **Código:** VEH-006, VEH-007  
> **Versión:** 1.0  
> **Última actualización:** Enero 25, 2026  
> **Criticidad:** 🟡 ALTA (Diferenciador de UX)  
> **Estado de Implementación:** � En desarrollo Backend | ✅ 100% UI

---

## ⚠️ AUDITORÍA DE ACCESO UI (Enero 25, 2026)

| Proceso                     | Backend        | UI Access             | Observación              |
| --------------------------- | -------------- | --------------------- | ------------------------ |
| M360-UPLOAD-001 Subida 360° | 🟡 En progreso | ✅ Form disponible    | Feature planificada Q2   |
| M360-VIEW-001 Visualizador  | 🟡 En progreso | ✅ Media360ViewerPage | Visor interactivo creado |
| VIDEO-UPLOAD-001 Video Tour | 🟡 En progreso | ✅ Form disponible    | Upload video disponible  |
| VIDEO-STREAM-001 Streaming  | 🟡 En progreso | ✅ VideoTourPage      | Player de video creado   |

### Rutas UI Existentes ✅

- `/vehicles/:id` → VehicleDetailPage (fotos estáticas + links a 360°/video)
- `/vehicles/:slug/360` → Visor 360° interactivo (Media360ViewerPage)
- `/vehicles/:slug/video` → Video tour completo (VideoTourPage)

### Rutas UI para Upload (Dealer) ✅

- `/dealer/inventory/:id/edit` → Incluye sección de media 360° y video

**Verificación Backend:** MediaService existe, extensión 360°/Video en desarrollo para Q2 2026.

> ℹ️ **NOTA:** Frontend UI completado. Backend en desarrollo para streaming/processing.

---

## 📊 Resumen de Implementación

| Componente                       | Total | Implementado | Pendiente | Estado         |
| -------------------------------- | ----- | ------------ | --------- | -------------- |
| **Controllers**                  | 2     | 0            | 2         | 🔴 Pendiente   |
| **M360-UPLOAD-\*** (Subida)      | 3     | 0            | 3         | 🔴 Pendiente   |
| **M360-PROCESS-\*** (Procesado)  | 4     | 0            | 4         | 🔴 Pendiente   |
| **M360-VIEW-\*** (Visualización) | 3     | 0            | 3         | 🔴 Pendiente   |
| **VIDEO-UPLOAD-\*** (Videos)     | 3     | 0            | 3         | 🔴 Pendiente   |
| **VIDEO-STREAM-\*** (Streaming)  | 3     | 0            | 3         | 🔴 Pendiente   |
| **Tests**                        | 0     | 0            | 18        | 🔴 Pendiente   |
| **TOTAL**                        | 18    | 0            | 18        | 🔴 0% Completo |

---

## 📋 Información General

| Campo             | Valor                          |
| ----------------- | ------------------------------ |
| **Servicio**      | MediaService (extendido)       |
| **Puerto**        | 5007                           |
| **Base de Datos** | `mediaservice`                 |
| **Dependencias**  | VehiclesSaleService, MLService |
| **Storage**       | AWS S3, CloudFront CDN         |

---

## 🎯 Objetivo del Proceso

1. **Vista 360°:** Experiencia interactiva de rotación del vehículo
2. **Video Tour:** Video de walkaround grabado por el vendedor
3. **Interior Panorámico:** Vista 360° del interior
4. **Hotspots:** Puntos de interés marcados en las imágenes

---

## 🏗️ Arquitectura

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                     Media 360° & Video Architecture                          │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│   Upload Flow                        Processing                Delivery     │
│   ┌────────────────┐              ┌─────────────────┐     ┌────────────┐   │
│   │ Dealer App     │──┐           │  Media Workers  │     │ CDN        │   │
│   │ (360° Camera)  │  │           │                 │     │ (CloudFront│   │
│   └────────────────┘  │           │ ┌─────────────┐ │     │  /Spaces)  │   │
│   ┌────────────────┐  │           │ │ 360° Worker │ │     └────────────┘   │
│   │ Mobile App     │──┼──────────▶│ │ • Stitch    │ │            │         │
│   │ (Video Tour)   │  │           │ │ • Optimize  │ │────────────│         │
│   └────────────────┘  │           │ │ • Hotspots  │ │            │         │
│   ┌────────────────┐  │           │ └─────────────┘ │            ▼         │
│   │ Web Upload     │──┘           │ ┌─────────────┐ │     ┌────────────┐   │
│   │ (Multiple IMG) │              │ │Video Worker │ │     │ Frontend   │   │
│   └────────────────┘              │ │ • Transcode │ │     │            │   │
│                                   │ │ • HLS/DASH  │ │────▶│ 360 Viewer │   │
│   MediaService API                │ │ • Thumbs    │ │     │ (Three.js) │   │
│   ┌────────────────┐              │ └─────────────┘ │     │            │   │
│   │ POST /upload   │─────────────▶│ ┌─────────────┐ │     │ Video.js   │   │
│   │ GET /360       │              │ │ Quality     │ │     │ (HLS)      │   │
│   │ GET /video     │◀─────────────│ │ Analysis    │ │     └────────────┘   │
│   └────────────────┘              │ │ (ML)        │ │                      │
│                                   │ └─────────────┘ │                      │
│                                   └─────────────────┘                      │
│                                            │                               │
│                                ┌───────────┼───────────┐                   │
│                                ▼           ▼           ▼                   │
│                        ┌────────────┐ ┌────────────┐ ┌────────────┐       │
│                        │ PostgreSQL │ │ S3/Spaces  │ │  RabbitMQ  │       │
│                        │ (Metadata, │ │ (Files,    │ │ (Process   │       │
│                        │  Hotspots) │ │  Variants) │ │  Queue)    │       │
│                        └────────────┘ └────────────┘ └────────────┘       │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 📡 Endpoints

| Método | Endpoint                              | Descripción              | Auth |
| ------ | ------------------------------------- | ------------------------ | ---- |
| `POST` | `/api/media/360/upload`               | Subir imágenes para 360° | ✅   |
| `GET`  | `/api/media/360/{vehicleId}`          | Obtener vista 360°       | ❌   |
| `POST` | `/api/media/video/upload`             | Subir video tour         | ✅   |
| `GET`  | `/api/media/video/{vehicleId}`        | Obtener video tour       | ❌   |
| `POST` | `/api/media/hotspots`                 | Agregar hotspots         | ✅   |
| `GET`  | `/api/media/interior-360/{vehicleId}` | Vista 360° interior      | ❌   |

---

## 🗃️ Entidades

### Vehicle360View

```csharp
public class Vehicle360View
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }

    // Configuración
    public View360Type Type { get; set; }
    public int ImageCount { get; set; }             // 24, 36, 72 imágenes
    public int DegreesPerImage { get; set; }        // 360 / ImageCount

    // Imágenes
    public List<View360Image> Images { get; set; }
    public string PreviewImageUrl { get; set; }     // Imagen estática para preview

    // Estado
    public View360Status Status { get; set; }
    public bool IsProcessed { get; set; }
    public DateTime? ProcessedAt { get; set; }

    // Metadata
    public string BackgroundColor { get; set; }      // Auto-removed o color
    public bool HasHotspots { get; set; }
    public List<Hotspot> Hotspots { get; set; }

    // Quality
    public int QualityScore { get; set; }            // 0-100
    public List<string> QualityIssues { get; set; }  // Blur, lighting, etc.

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public enum View360Type
{
    Exterior,           // Rotación exterior
    Interior,           // Panorama interior
    EngineCompartment,  // Motor
    Trunk               // Baúl
}

public enum View360Status
{
    Uploading,
    Processing,
    Ready,
    Failed
}
```

### View360Image

```csharp
public class View360Image
{
    public Guid Id { get; set; }
    public Guid View360Id { get; set; }

    public int SequenceNumber { get; set; }          // 1-72
    public int Degrees { get; set; }                 // 0-360

    public string OriginalUrl { get; set; }          // Original uploaded
    public string ProcessedUrl { get; set; }         // Processed/optimized
    public string ThumbnailUrl { get; set; }         // For timeline

    public int Width { get; set; }
    public int Height { get; set; }
    public long FileSizeBytes { get; set; }

    public bool IsKeyFrame { get; set; }             // Frames importantes
}
```

### VideoTour

```csharp
public class VideoTour
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }

    // Video
    public string OriginalVideoUrl { get; set; }
    public string ProcessedVideoUrl { get; set; }
    public string ThumbnailUrl { get; set; }
    public string PreviewGifUrl { get; set; }        // GIF de preview

    // Metadata
    public int DurationSeconds { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public long FileSizeBytes { get; set; }
    public string Format { get; set; }               // MP4, WebM

    // Estado
    public VideoStatus Status { get; set; }
    public bool IsTranscoded { get; set; }
    public DateTime? TranscodedAt { get; set; }

    // Versiones transcodificadas
    public List<VideoVariant> Variants { get; set; }

    // Calidad
    public int QualityScore { get; set; }
    public bool HasAudio { get; set; }
    public bool IsStabilized { get; set; }

    // Chapters (puntos de interés)
    public List<VideoChapter> Chapters { get; set; }

    public DateTime CreatedAt { get; set; }
}

public enum VideoStatus
{
    Uploading,
    Transcoding,
    Ready,
    Failed
}
```

### VideoVariant

```csharp
public class VideoVariant
{
    public string Quality { get; set; }              // 1080p, 720p, 480p
    public string Url { get; set; }
    public long FileSizeBytes { get; set; }
    public int Bitrate { get; set; }
}
```

### VideoChapter

```csharp
public class VideoChapter
{
    public int StartSecond { get; set; }
    public string Title { get; set; }               // "Exterior Frontal", "Interior", etc.
    public string ThumbnailUrl { get; set; }
}
```

### Hotspot

```csharp
public class Hotspot
{
    public Guid Id { get; set; }
    public Guid View360Id { get; set; }

    // Posición
    public int Degrees { get; set; }                 // En qué ángulo aparece
    public decimal XPercent { get; set; }            // Posición X (0-100%)
    public decimal YPercent { get; set; }            // Posición Y (0-100%)

    // Contenido
    public HotspotType Type { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string IconUrl { get; set; }

    // Datos adicionales
    public string DetailImageUrl { get; set; }       // Imagen de detalle
    public string LinkUrl { get; set; }              // Link externo
}

public enum HotspotType
{
    Feature,            // Característica destacada
    Damage,             // Daño o imperfección
    Upgrade,            // Mejora/accesorio
    Info                // Información general
}
```

---

## 📊 Proceso VEH-006: Crear Vista 360°

```
┌─────────────────────────────────────────────────────────────────────────┐
│ PROCESO: VEH-006 - Crear Vista 360° del Vehículo                       │
├─────────────────────────────────────────────────────────────────────────┤
│ Actor Iniciador: USR-SELLER, DLR-ADMIN                                 │
│ Sistemas: MediaService, MLService, VehiclesSaleService                 │
│ Duración: 5-15 minutos (captura) + 2-5 min (procesamiento)             │
│ Criticidad: ALTA                                                        │
└─────────────────────────────────────────────────────────────────────────┘
```

| Paso | Subpaso | Acción                             | Sistema             | Actor      | Evidencia              | Código     |
| ---- | ------- | ---------------------------------- | ------------------- | ---------- | ---------------------- | ---------- |
| 1    | 1.1     | Usuario edita listing              | Frontend            | USR-SELLER | Edit accessed          | EVD-LOG    |
| 1    | 1.2     | Click "Agregar Vista 360°"         | Frontend            | USR-SELLER | CTA clicked            | EVD-LOG    |
| 2    | 2.1     | Mostrar guía de captura            | Frontend            | USR-SELLER | Guide shown            | EVD-SCREEN |
| 2    | 2.2     | Explicar requisitos (24-72 fotos)  | Frontend            | USR-SELLER | Instructions           | EVD-LOG    |
| 3    | 3.1     | Usuario toma fotos en secuencia    | Mobile App          | USR-SELLER | Photos captured        | EVD-LOG    |
| 3    | 3.2     | App guía rotación (indicador 360°) | Mobile App          | USR-SELLER | Rotation guide         | EVD-LOG    |
| 4    | 4.1     | POST /api/media/360/upload         | Gateway             | USR-SELLER | **Request**            | EVD-AUDIT  |
| 4    | 4.2     | Validar cantidad de imágenes       | MediaService        | Sistema    | Validation             | EVD-LOG    |
| 4    | 4.3     | Validar calidad mínima             | MediaService        | Sistema    | Quality check          | EVD-LOG    |
| 5    | 5.1     | **Upload a S3**                    | MediaService        | Sistema    | **Files uploaded**     | EVD-FILE   |
| 5    | 5.2     | Crear Vehicle360View               | MediaService        | Sistema    | Record created         | EVD-AUDIT  |
| 6    | 6.1     | **Procesar imágenes**              | MLService           | Sistema    | **Processing started** | EVD-AUDIT  |
| 6    | 6.2     | Detectar y remover fondo           | MLService           | Sistema    | Background removed     | EVD-LOG    |
| 6    | 6.3     | Normalizar iluminación             | MLService           | Sistema    | Lighting normalized    | EVD-LOG    |
| 6    | 6.4     | Alinear secuencia                  | MLService           | Sistema    | Sequence aligned       | EVD-LOG    |
| 7    | 7.1     | Generar thumbnails                 | MediaService        | Sistema    | Thumbnails created     | EVD-LOG    |
| 7    | 7.2     | Generar preview image              | MediaService        | Sistema    | Preview created        | EVD-LOG    |
| 8    | 8.1     | Calcular quality score             | MLService           | Sistema    | Score calculated       | EVD-LOG    |
| 8    | 8.2     | Identificar issues                 | MLService           | Sistema    | Issues identified      | EVD-LOG    |
| 9    | 9.1     | Actualizar status → Ready          | MediaService        | Sistema    | Status updated         | EVD-LOG    |
| 9    | 9.2     | **Actualizar listing**             | VehiclesSaleService | Sistema    | **Listing updated**    | EVD-EVENT  |
| 9    | 9.3     | Agregar badge "360°"               | VehiclesSaleService | Sistema    | Badge added            | EVD-LOG    |
| 10   | 10.1    | **Notificar completado**           | NotificationService | SYS-NOTIF  | **Confirmation**       | EVD-COMM   |
| 11   | 11.1    | **Audit trail**                    | AuditService        | Sistema    | Complete audit         | EVD-AUDIT  |

### Evidencia de Vista 360°

```json
{
  "processCode": "VEH-006",
  "view360": {
    "id": "360-12345",
    "vehicleId": "veh-67890",
    "type": "EXTERIOR",
    "configuration": {
      "imageCount": 36,
      "degreesPerImage": 10,
      "format": "JPG",
      "resolution": "2048x1536"
    },
    "processing": {
      "startedAt": "2026-01-21T10:30:00Z",
      "completedAt": "2026-01-21T10:33:45Z",
      "duration": "3m 45s",
      "steps": [
        { "step": "upload", "status": "completed", "duration": "45s" },
        {
          "step": "background_removal",
          "status": "completed",
          "duration": "90s"
        },
        {
          "step": "lighting_normalization",
          "status": "completed",
          "duration": "30s"
        },
        {
          "step": "sequence_alignment",
          "status": "completed",
          "duration": "20s"
        },
        {
          "step": "thumbnail_generation",
          "status": "completed",
          "duration": "15s"
        }
      ]
    },
    "quality": {
      "overallScore": 88,
      "factors": {
        "resolution": 95,
        "lighting": 85,
        "focus": 90,
        "sequenceSmooth": 82
      },
      "issues": [
        {
          "type": "LIGHTING_VARIANCE",
          "severity": "MINOR",
          "affectedFrames": [12, 13, 14],
          "description": "Slight shadow variation on left side"
        }
      ]
    },
    "output": {
      "previewUrl": "cdn.okla.com.do/360/12345/preview.jpg",
      "playerUrl": "cdn.okla.com.do/360/12345/viewer.html",
      "images": [
        { "degree": 0, "url": "cdn.okla.com.do/360/12345/frame-001.jpg" },
        { "degree": 10, "url": "cdn.okla.com.do/360/12345/frame-002.jpg" }
      ],
      "thumbnailStrip": "cdn.okla.com.do/360/12345/strip.jpg",
      "totalSizeBytes": 15728640
    },
    "hotspots": []
  }
}
```

---

## 📊 Proceso VEH-007: Subir Video Tour

```
┌─────────────────────────────────────────────────────────────────────────┐
│ PROCESO: VEH-007 - Subir Video Tour del Vehículo                       │
├─────────────────────────────────────────────────────────────────────────┤
│ Actor Iniciador: USR-SELLER, DLR-ADMIN                                 │
│ Sistemas: MediaService, TranscodingService                             │
│ Duración: Upload (variable) + 5-15 min transcoding                     │
│ Criticidad: ALTA                                                        │
└─────────────────────────────────────────────────────────────────────────┘
```

| Paso | Subpaso | Acción                                  | Sistema             | Actor      | Evidencia               | Código     |
| ---- | ------- | --------------------------------------- | ------------------- | ---------- | ----------------------- | ---------- |
| 1    | 1.1     | Usuario edita listing                   | Frontend            | USR-SELLER | Edit accessed           | EVD-LOG    |
| 1    | 1.2     | Click "Agregar Video Tour"              | Frontend            | USR-SELLER | CTA clicked             | EVD-LOG    |
| 2    | 2.1     | Mostrar guía de grabación               | Frontend            | USR-SELLER | Guide shown             | EVD-SCREEN |
| 2    | 2.2     | Tips: horizontal, estabilizado, 1-3 min | Frontend            | USR-SELLER | Tips shown              | EVD-LOG    |
| 3    | 3.1     | Usuario graba o selecciona video        | Mobile/Web          | USR-SELLER | Video selected          | EVD-LOG    |
| 4    | 4.1     | POST /api/media/video/upload            | Gateway             | USR-SELLER | **Request**             | EVD-AUDIT  |
| 4    | 4.2     | Validar formato y duración              | MediaService        | Sistema    | Validation              | EVD-LOG    |
| 4    | 4.3     | Validar tamaño (max 500MB)              | MediaService        | Sistema    | Size check              | EVD-LOG    |
| 5    | 5.1     | **Upload chunked a S3**                 | MediaService        | Sistema    | **File uploaded**       | EVD-FILE   |
| 5    | 5.2     | Progreso reportado al cliente           | MediaService        | USR-SELLER | Progress events         | EVD-LOG    |
| 5    | 5.3     | Crear VideoTour record                  | MediaService        | Sistema    | Record created          | EVD-AUDIT  |
| 6    | 6.1     | **Iniciar transcoding**                 | TranscodingService  | Sistema    | **Transcoding started** | EVD-AUDIT  |
| 6    | 6.2     | Generar variante 1080p                  | TranscodingService  | Sistema    | 1080p created           | EVD-LOG    |
| 6    | 6.3     | Generar variante 720p                   | TranscodingService  | Sistema    | 720p created            | EVD-LOG    |
| 6    | 6.4     | Generar variante 480p                   | TranscodingService  | Sistema    | 480p created            | EVD-LOG    |
| 7    | 7.1     | Extraer thumbnail                       | TranscodingService  | Sistema    | Thumbnail extracted     | EVD-LOG    |
| 7    | 7.2     | Generar preview GIF                     | TranscodingService  | Sistema    | GIF created             | EVD-LOG    |
| 8    | 8.1     | **Auto-detectar chapters**              | MLService           | Sistema    | **Chapters detected**   | EVD-AUDIT  |
| 8    | 8.2     | Detectar "Exterior", "Interior", etc.   | MLService           | Sistema    | Scenes detected         | EVD-LOG    |
| 9    | 9.1     | Calcular quality score                  | MLService           | Sistema    | Score calculated        | EVD-LOG    |
| 9    | 9.2     | Detectar estabilización necesaria       | MLService           | Sistema    | Stability check         | EVD-LOG    |
| 10   | 10.1    | Actualizar status → Ready               | MediaService        | Sistema    | Status updated          | EVD-LOG    |
| 10   | 10.2    | **Actualizar listing**                  | VehiclesSaleService | Sistema    | **Listing updated**     | EVD-EVENT  |
| 10   | 10.3    | Agregar badge "VIDEO"                   | VehiclesSaleService | Sistema    | Badge added             | EVD-LOG    |
| 11   | 11.1    | **Notificar completado**                | NotificationService | SYS-NOTIF  | **Confirmation**        | EVD-COMM   |
| 12   | 12.1    | **Audit trail**                         | AuditService        | Sistema    | Complete audit          | EVD-AUDIT  |

### Evidencia de Video Tour

```json
{
  "processCode": "VEH-007",
  "videoTour": {
    "id": "video-12345",
    "vehicleId": "veh-67890",
    "upload": {
      "originalFilename": "toyota_camry_tour.mp4",
      "originalSize": 125829120,
      "uploadDuration": "45s",
      "chunks": 12
    },
    "transcoding": {
      "startedAt": "2026-01-21T10:30:00Z",
      "completedAt": "2026-01-21T10:38:00Z",
      "duration": "8m",
      "variants": [
        {
          "quality": "1080p",
          "resolution": "1920x1080",
          "bitrate": 8000,
          "size": 95420000,
          "url": "cdn.okla.com.do/video/12345/1080p.mp4"
        },
        {
          "quality": "720p",
          "resolution": "1280x720",
          "bitrate": 4000,
          "size": 47710000,
          "url": "cdn.okla.com.do/video/12345/720p.mp4"
        },
        {
          "quality": "480p",
          "resolution": "854x480",
          "bitrate": 2000,
          "size": 23855000,
          "url": "cdn.okla.com.do/video/12345/480p.mp4"
        }
      ]
    },
    "metadata": {
      "duration": 127,
      "hasAudio": true,
      "audioLanguage": "es",
      "fps": 30,
      "codec": "H.264"
    },
    "chapters": [
      { "start": 0, "title": "Exterior Frontal", "thumbnail": "chap1.jpg" },
      { "start": 25, "title": "Lateral Derecho", "thumbnail": "chap2.jpg" },
      { "start": 45, "title": "Exterior Trasero", "thumbnail": "chap3.jpg" },
      { "start": 65, "title": "Interior", "thumbnail": "chap4.jpg" },
      { "start": 95, "title": "Motor", "thumbnail": "chap5.jpg" },
      { "start": 115, "title": "Baúl", "thumbnail": "chap6.jpg" }
    ],
    "quality": {
      "overallScore": 85,
      "factors": {
        "resolution": 100,
        "stability": 75,
        "lighting": 88,
        "audio": 90,
        "coverage": 80
      },
      "issues": [
        {
          "type": "SLIGHT_SHAKE",
          "severity": "MINOR",
          "timeRange": "0:45-0:52",
          "description": "Slight camera shake during transition"
        }
      ],
      "suggestions": [
        "Consider using a stabilizer for smoother footage",
        "Add more interior dashboard closeups"
      ]
    },
    "preview": {
      "thumbnail": "cdn.okla.com.do/video/12345/thumb.jpg",
      "gif": "cdn.okla.com.do/video/12345/preview.gif",
      "gifDuration": 5
    }
  }
}
```

---

## 📱 Guía de Captura 360°

### Requisitos Mínimos

| Aspecto        | Mínimo | Recomendado | Óptimo    |
| -------------- | ------ | ----------- | --------- |
| **Fotos**      | 24     | 36          | 72        |
| **Resolución** | 1080p  | 2K          | 4K        |
| **Formato**    | JPG    | JPG         | RAW → JPG |
| **Fondo**      | Limpio | Blanco/Gris | Estudio   |

### Instrucciones para Usuario

```
📸 CÓMO CREAR UNA VISTA 360° PERFECTA

1️⃣ PREPARACIÓN
   • Estaciona el vehículo en un área abierta
   • Asegúrate de buena iluminación (evita sol directo)
   • Limpia el vehículo

2️⃣ POSICIONAMIENTO
   • Mantén la cámara a la altura del centro del vehículo
   • Distancia: ~3-4 metros del vehículo
   • Usa un trípode si es posible

3️⃣ CAPTURA
   • Gira alrededor del vehículo en sentido horario
   • Toma una foto cada 10° (36 fotos total)
   • Mantén la cámara nivelada
   • La app te guiará con indicadores

4️⃣ CONSEJOS PRO
   • Evita sombras proyectadas
   • No incluyas personas en las fotos
   • Verifica que todas las fotos estén enfocadas
```

---

## 🎬 Guía de Video Tour

### Estructura Recomendada (2 minutos)

```
00:00 - 00:20  Exterior Frontal (walk around front)
00:20 - 00:35  Lateral Derecho
00:35 - 00:50  Exterior Trasero
00:50 - 01:05  Lateral Izquierdo
01:05 - 01:25  Interior (asientos, tablero)
01:25 - 01:40  Motor
01:40 - 02:00  Baúl y características especiales
```

### Tips de Grabación

```
🎬 CONSEJOS PARA UN VIDEO TOUR PROFESIONAL

📱 CONFIGURACIÓN
   • Graba en horizontal (landscape)
   • Resolución mínima: 1080p
   • Duración: 1-3 minutos ideal

🎥 TÉCNICA
   • Movimientos lentos y suaves
   • Evita zoom digital
   • Mantén estabilidad (usa ambas manos)

🔊 AUDIO
   • Describe lo que muestras
   • Menciona características destacadas
   • Evita ruido de fondo

💡 ILUMINACIÓN
   • Luz natural es mejor
   • Evita contraluces fuertes
   • Interior: enciende luces del vehículo
```

---

## 📊 Métricas Prometheus

```yaml
# Vista 360
media_360_created_total{type}
media_360_processing_time_seconds
media_360_quality_score_average

# Video
media_video_uploaded_total
media_video_transcoding_time_seconds
media_video_size_bytes{quality}
media_video_views_total

# Engagement
media_360_interactions_total{action}  # rotate, zoom, hotspot_click
media_video_watch_time_seconds
media_video_completion_rate

# Quality
media_360_quality_issues_total{type}
media_video_quality_issues_total{type}
```

---

## 💰 Impacto en Listado

| Característica      | Aumento Vistas | Aumento Contactos | Premium Price |
| ------------------- | -------------- | ----------------- | ------------- |
| **Sin media extra** | Base           | Base              | -             |
| **+ Vista 360°**    | +45%           | +35%              | -             |
| **+ Video Tour**    | +60%           | +50%              | -             |
| **+ Ambos**         | +85%           | +70%              | +5% precio    |
| **+ 360° Interior** | +25% adicional | +20% adicional    | -             |

---

## 🔗 Referencias

- [03-VEHICULOS-INVENTARIO/01-vehicles-sale-service.md](../03-VEHICULOS-INVENTARIO/01-vehicles-sale-service.md)
- [09-MEDIA-ARCHIVOS/01-media-service.md](../09-MEDIA-ARCHIVOS/01-media-service.md)
- [AWS Elemental MediaConvert](https://aws.amazon.com/mediaconvert/)
