# 🎥 Procesamiento 360° de Vehículos

Este directorio contiene la documentación completa del sistema de procesamiento de vistas 360° de vehículos para OKLA.

## 📁 Contenido

| Archivo                                                                  | Descripción                               |
| ------------------------------------------------------------------------ | ----------------------------------------- |
| [01-VISION-GENERAL.md](./01-VISION-GENERAL.md)                           | Visión general del sistema y arquitectura |
| [02-VIDEO360SERVICE.md](./02-VIDEO360SERVICE.md)                         | Servicio de extracción de frames de video |
| [03-BACKGROUNDREMOVALSERVICE.md](./03-BACKGROUNDREMOVALSERVICE.md)       | Servicio de remoción de fondos            |
| [04-VEHICLE360PROCESSINGSERVICE.md](./04-VEHICLE360PROCESSINGSERVICE.md) | Orquestador principal                     |
| [05-INTEGRACION-FRONTEND.md](./05-INTEGRACION-FRONTEND.md)               | Integración con frontend React/Flutter    |
| [06-TABLA-PROVEEDORES-PRECIOS.md](./06-TABLA-PROVEEDORES-PRECIOS.md)     | Tabla completa de proveedores y precios   |

## 🏗️ Arquitectura de Alto Nivel

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              FRONTEND                                        │
│                        (React Web / Flutter Mobile)                          │
└─────────────────────────────────────────────────────────────────────────────┘
                                     │
                                     │ POST /api/vehicle360processing/process
                                     ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                    Vehicle360ProcessingService                               │
│                         (ORQUESTADOR)                                        │
│                                                                              │
│    ┌──────────────────────────────────────────────────────────────────┐     │
│    │                    Polly Resilience                               │     │
│    │   • Retry (3 intentos, exponential backoff: 2s, 4s, 8s)         │     │
│    │   • Circuit Breaker (5 fallos → abierto 30s)                    │     │
│    │   • Timeout (configurable por servicio)                          │     │
│    └──────────────────────────────────────────────────────────────────┘     │
└─────────────────────────────────────────────────────────────────────────────┘
              │                        │                        │
              ▼                        ▼                        ▼
┌─────────────────────┐  ┌─────────────────────┐  ┌─────────────────────────┐
│    MediaService     │  │   Video360Service   │  │ BackgroundRemovalService│
│    (S3 Storage)     │  │ (Frame Extraction)  │  │   (Background Removal)  │
│                     │  │                     │  │                         │
│ • Upload videos     │  │ • 5 proveedores     │  │ • 6 proveedores         │
│ • Upload images     │  │ • 6 frames output   │  │ • Strategy pattern      │
│ • CDN distribution  │  │ • Fallback auto     │  │ • Fallback auto         │
└─────────────────────┘  └─────────────────────┘  └─────────────────────────┘
```

## 💰 Costo Total por Vehículo

| Escenario         | Video360            | BackgroundRemoval (×6)        | Total      |
| ----------------- | ------------------- | ----------------------------- | ---------- |
| **Más Económico** | $0.009 (ApyHub)     | $0.02 × 6 = $0.12 (Slazzer)   | **$0.129** |
| **Recomendado**   | $0.011 (FFmpeg-API) | $0.05 × 6 = $0.30 (ClipDrop)  | **$0.311** |
| **Premium**       | $0.05 (Shotstack)   | $0.20 × 6 = $1.20 (Remove.bg) | **$1.25**  |

## 📊 Flujo de Datos

```
Usuario sube video MP4
        │
        ▼
┌───────────────────────────────────────────────────────────────────┐
│ 1. UPLOAD A S3                                                     │
│    MediaService guarda video original                              │
│    Output: s3://okla-media/videos/{vehicleId}/original.mp4        │
└───────────────────────────────────────────────────────────────────┘
        │
        ▼
┌───────────────────────────────────────────────────────────────────┐
│ 2. EXTRACCIÓN DE FRAMES                                           │
│    Video360Service extrae 6 frames equidistantes                  │
│    Ángulos: 0°, 60°, 120°, 180°, 240°, 300°                      │
│    Output: 6 imágenes JPEG/PNG                                    │
└───────────────────────────────────────────────────────────────────┘
        │
        ▼
┌───────────────────────────────────────────────────────────────────┐
│ 3. REMOCIÓN DE FONDOS                                             │
│    BackgroundRemovalService procesa cada frame                    │
│    Output: 6 imágenes PNG con fondo transparente                  │
└───────────────────────────────────────────────────────────────────┘
        │
        ▼
┌───────────────────────────────────────────────────────────────────┐
│ 4. ALMACENAMIENTO FINAL                                           │
│    MediaService guarda imágenes procesadas                        │
│    Output: URLs CDN para el frontend                              │
│    https://cdn.okla.com.do/vehicles/{id}/360/frame_01.png        │
└───────────────────────────────────────────────────────────────────┘
        │
        ▼
┌───────────────────────────────────────────────────────────────────┐
│ 5. VISOR 360° EN FRONTEND                                         │
│    Componente interactivo con drag/auto-rotate                    │
│    Muestra las 6 imágenes como carrusel 360°                      │
└───────────────────────────────────────────────────────────────────┘
```

## 🚀 Endpoints Principales

| Servicio              | Endpoint                                           | Descripción                   |
| --------------------- | -------------------------------------------------- | ----------------------------- |
| **Orquestador**       | `POST /api/vehicle360processing/process`           | Inicia procesamiento completo |
| **Orquestador**       | `GET /api/vehicle360processing/jobs/{id}/status`   | Estado del job                |
| **Orquestador**       | `GET /api/vehicle360processing/viewer/{vehicleId}` | Datos para visor 360°         |
| **Video360**          | `POST /api/video360/jobs`                          | Crear job de extracción       |
| **Video360**          | `GET /api/video360/jobs/{id}`                      | Estado del job                |
| **BackgroundRemoval** | `POST /api/backgroundremoval/remove`               | Remover fondo de imagen       |

## 📱 Uso desde Frontend

### React/TypeScript

```typescript
// 1. Iniciar procesamiento
const startProcessing = async (videoFile: File, vehicleId: string) => {
  const formData = new FormData();
  formData.append("video", videoFile);
  formData.append("vehicleId", vehicleId);

  const response = await fetch("/api/vehicle360processing/process", {
    method: "POST",
    headers: { Authorization: `Bearer ${token}` },
    body: formData,
  });

  return response.json(); // { jobId, status, estimatedWaitSeconds }
};

// 2. Obtener datos del visor
const get360ViewData = async (vehicleId: string) => {
  const response = await fetch(`/api/vehicle360processing/viewer/${vehicleId}`);
  return response.json();
};
```

---

**Última actualización:** Enero 27, 2026  
**Autor:** OKLA Team
