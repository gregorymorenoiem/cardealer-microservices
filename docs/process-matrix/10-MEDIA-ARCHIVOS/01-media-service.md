# 🖼️ MediaService - Matriz de Procesos

> **Servicio:** MediaService  
> **Puerto:** 15104  
> **Base de Datos:** media_db  
> **Storage:** Digital Ocean Spaces (S3-compatible)  
> **CDN:** Digital Ocean CDN  
> **Última actualización:** Enero 21, 2026

---

## 1. Información General

### 1.1 Descripción

El MediaService gestiona todos los archivos multimedia de OKLA: imágenes de vehículos, documentos de verificación, videos, y archivos de usuario. Implementa upload presigned a S3, procesamiento asíncrono de imágenes (resize, watermark, optimización), y distribución via CDN.

### 1.2 Tipos de Media

| Tipo         | Prefijo ID | Formatos             | Max Size |
| ------------ | ---------- | -------------------- | -------- |
| **Image**    | `img_`     | JPG, PNG, WebP, HEIC | 10 MB    |
| **Video**    | `vid_`     | MP4, MOV, AVI        | 100 MB   |
| **Document** | `doc_`     | PDF, DOC, DOCX       | 25 MB    |
| **Audio**    | `aud_`     | MP3, WAV             | 20 MB    |

### 1.3 Arquitectura de Storage

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                            ARQUITECTURA MEDIA                               │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  Frontend                                                                   │
│     │                                                                       │
│     │ 1. POST /upload/init                                                  │
│     ▼                                                                       │
│  MediaService API ─────► DB (metadata)                                      │
│     │                                                                       │
│     │ 2. Return presigned URL                                              │
│     ▼                                                                       │
│  Frontend ─────────────► Digital Ocean Spaces (S3)                         │
│     │     PUT file                                                          │
│     │                                                                       │
│     │ 3. POST /upload/finalize                                             │
│     ▼                                                                       │
│  MediaService API                                                           │
│     │                                                                       │
│     │ 4. Publish: media.uploaded                                           │
│     ▼                                                                       │
│  RabbitMQ ─────────────► MediaWorker                                       │
│                              │                                              │
│                              │ 5. Process (resize, optimize)               │
│                              ▼                                              │
│                          Spaces + CDN                                       │
│                              │                                              │
│                              │ 6. Publish: media.processed                 │
│                              ▼                                              │
│                          Ready to serve                                     │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 1.4 Dependencias

| Servicio       | Propósito                  |
| -------------- | -------------------------- |
| VehicleService | Imágenes de vehículos      |
| UserService    | Fotos de perfil            |
| DealerService  | Logos, documentos          |
| KYCService     | Documentos de verificación |

### 1.5 Controllers

| Controller       | Archivo             | Endpoints |
| ---------------- | ------------------- | --------- |
| MediaController  | MediaController.cs  | 8         |
| HealthController | HealthController.cs | 1         |

---

## 2. Endpoints API

### 2.1 MediaController

| Método | Endpoint                               | Descripción                    | Auth     |
| ------ | -------------------------------------- | ------------------------------ | -------- |
| POST   | `/api/media/upload/init`               | Iniciar upload (presigned URL) | ✅       |
| POST   | `/api/media/upload/finalize/{mediaId}` | Confirmar upload               | ✅       |
| GET    | `/api/media/{mediaId}`                 | Obtener media info             | ✅       |
| GET    | `/api/media/owner/{ownerId}`           | Listar por owner               | ✅       |
| GET    | `/api/media/{mediaId}/variants`        | Obtener variantes              | ✅       |
| DELETE | `/api/media/{mediaId}`                 | Eliminar media                 | ✅       |
| POST   | `/api/media/{mediaId}/process`         | Procesar manualmente           | ✅       |
| GET    | `/api/media`                           | Listar (admin)                 | ✅ Admin |

---

## 3. Entidades

### 3.1 MediaAsset

| Campo            | Tipo               | Descripción                             |
| ---------------- | ------------------ | --------------------------------------- |
| Id               | string             | Prefixed ID (img_xxx, vid_xxx)          |
| DealerId         | Guid               | Multi-tenant (dealer owner)             |
| OwnerId          | string             | ID del recurso dueño (vehicle, user)    |
| Context          | string?            | Contexto (profile, vehicle, document)   |
| Type             | MediaType          | Image, Video, Document, Audio           |
| OriginalFileName | string             | Nombre original del archivo             |
| ContentType      | string             | MIME type                               |
| SizeBytes        | long               | Tamaño en bytes                         |
| Status           | MediaStatus        | Uploaded, Processing, Processed, Failed |
| ProcessingError  | string?            | Error si fallo procesamiento            |
| StorageKey       | string             | Path en S3                              |
| CdnUrl           | string?            | URL pública en CDN                      |
| ProcessedAt      | DateTime?          | Fecha de procesamiento                  |
| Variants         | List<MediaVariant> | Variantes generadas                     |

### 3.2 MediaVariant

| Campo        | Tipo   | Descripción                    |
| ------------ | ------ | ------------------------------ |
| Id           | Guid   | ID de la variante              |
| MediaAssetId | string | FK al asset original           |
| VariantType  | string | thumbnail, medium, large, webp |
| StorageKey   | string | Path en S3                     |
| CdnUrl       | string | URL pública                    |
| Width        | int    | Ancho en px                    |
| Height       | int    | Alto en px                     |
| SizeBytes    | long   | Tamaño de la variante          |

### 3.3 MediaStatus (Enum)

| Valor      | Descripción                   |
| ---------- | ----------------------------- |
| Uploaded   | Subido, pendiente de procesar |
| Processing | En proceso                    |
| Processed  | Listo y disponible            |
| Failed     | Error en procesamiento        |
| Deleted    | Marcado para eliminar         |

---

## 4. Procesos Detallados

### MEDIA-UPLOAD-001: Subir Imagen de Vehículo

| Campo          | Valor            |
| -------------- | ---------------- |
| **ID**         | MEDIA-UPLOAD-001 |
| **Nombre**     | Upload de Imagen |
| **Actor**      | Seller/Dealer    |
| **Criticidad** | 🔴 CRÍTICO       |
| **Estado**     | 🟢 ACTIVO        |

#### Precondiciones

- [ ] Usuario autenticado
- [ ] DealerId válido en JWT
- [ ] Archivo < 10 MB
- [ ] Formato soportado (JPG, PNG, WebP, HEIC)

#### Paso 1: Iniciar Upload

**Request (Init)**

```json
{
  "ownerId": "vehicle_abc123",
  "context": "vehicle-gallery",
  "fileName": "toyota-camry-front.jpg",
  "contentType": "image/jpeg",
  "fileSize": 2457600
}
```

**Flujo Init**

| Paso | Acción                               | Servicio   | Validación                         |
| ---- | ------------------------------------ | ---------- | ---------------------------------- |
| 1    | Extraer dealerId del JWT             | Controller | Claim existe                       |
| 2    | Validar tamaño permitido             | Handler    | < 10 MB                            |
| 3    | Validar content-type                 | Handler    | Formato permitido                  |
| 4    | Determinar MediaType                 | Handler    | Según content-type                 |
| 5    | Generar ID prefixado                 | Domain     | img_xxx                            |
| 6    | Generar StorageKey                   | Handler    | {dealer}/{context}/{id}/{filename} |
| 7    | Crear presigned PUT URL              | S3Service  | Expira en 15 min                   |
| 8    | Crear MediaAsset (status: Uploading) | Repository | INSERT                             |
| 9    | Retornar URL y mediaId               | Controller | 200 OK                             |

**Response Init (200)**

```json
{
  "success": true,
  "data": {
    "mediaId": "img_a1b2c3d4e5f6",
    "uploadUrl": "https://okla-media.nyc3.digitaloceanspaces.com/...",
    "expiresAt": "2026-01-21T10:45:00Z",
    "headers": {
      "Content-Type": "image/jpeg",
      "x-amz-acl": "private"
    }
  }
}
```

#### Paso 2: Upload Directo a S3

**Frontend ejecuta:**

```javascript
const response = await fetch(uploadUrl, {
  method: "PUT",
  headers: {
    "Content-Type": "image/jpeg",
    "x-amz-acl": "private",
  },
  body: imageFile,
});
```

#### Paso 3: Finalizar Upload

**Request (Finalize)**

```
POST /api/media/upload/finalize/img_a1b2c3d4e5f6
```

**Flujo Finalize**

| Paso | Acción                      | Servicio   | Validación                |
| ---- | --------------------------- | ---------- | ------------------------- |
| 1    | Buscar MediaAsset           | Repository | Existe y status=Uploading |
| 2    | Verificar archivo en S3     | S3Service  | HEAD request              |
| 3    | Validar tamaño real         | S3Service  | Matches expected          |
| 4    | Actualizar status: Uploaded | Repository | UPDATE                    |
| 5    | Publicar evento             | RabbitMQ   | media.uploaded            |
| 6    | Retornar media info         | Controller | 200 OK                    |

**Response Finalize (200)**

```json
{
  "success": true,
  "data": {
    "mediaId": "img_a1b2c3d4e5f6",
    "status": "Uploaded",
    "originalFileName": "toyota-camry-front.jpg",
    "sizeBytes": 2457600,
    "contentType": "image/jpeg",
    "processingStarted": true,
    "estimatedProcessingTime": "5-10 seconds"
  }
}
```

#### Paso 4: Procesamiento Asíncrono (Worker)

| Paso | Acción                       | Servicio   | Output            |
| ---- | ---------------------------- | ---------- | ----------------- |
| 1    | Consumir evento              | RabbitMQ   | media.uploaded    |
| 2    | Descargar original           | S3Service  | Temp file         |
| 3    | Validar imagen válida        | ImageSharp | No corrupta       |
| 4    | Auto-orientar (EXIF)         | ImageSharp | Rotación correcta |
| 5    | Generar thumbnail            | ImageSharp | 150x150           |
| 6    | Generar medium               | ImageSharp | 800x600           |
| 7    | Generar large                | ImageSharp | 1920x1080         |
| 8    | Convertir a WebP             | ImageSharp | Optimizado        |
| 9    | Agregar watermark            | ImageSharp | Logo OKLA         |
| 10   | Subir variantes a S3         | S3Service  | Public-read       |
| 11   | Configurar CDN URLs          | Handler    | cdn.okla.com.do   |
| 12   | Crear MediaVariants          | Repository | INSERT batch      |
| 13   | Actualizar status: Processed | Repository | UPDATE            |
| 14   | Publicar evento              | RabbitMQ   | media.processed   |
| 15   | Limpiar temp files           | Handler    | Delete            |

---

### MEDIA-VARIANTS-001: Variantes de Imagen

| Campo          | Valor                 |
| -------------- | --------------------- |
| **ID**         | MEDIA-VARIANTS-001    |
| **Nombre**     | Generar Variantes     |
| **Actor**      | MediaWorker (sistema) |
| **Criticidad** | 🟠 ALTO               |
| **Estado**     | 🟢 ACTIVO             |

#### Variantes Generadas

| Variante        | Dimensiones | Uso            | Watermark |
| --------------- | ----------- | -------------- | --------- |
| `thumbnail`     | 150x150     | Grid listings  | ❌ No     |
| `small`         | 320x240     | Mobile cards   | ❌ No     |
| `medium`        | 800x600     | Vehicle detail | ✅ Sí     |
| `large`         | 1920x1080   | Full screen    | ✅ Sí     |
| `original_webp` | Original    | Optimizado     | ❌ No     |

#### Configuración de Procesamiento

```json
{
  "image": {
    "quality": 85,
    "formats": ["webp", "jpeg"],
    "variants": {
      "thumbnail": { "width": 150, "height": 150, "crop": true },
      "small": { "width": 320, "height": 240, "crop": true },
      "medium": { "width": 800, "height": 600, "crop": false },
      "large": { "width": 1920, "height": 1080, "crop": false }
    },
    "watermark": {
      "enabled": true,
      "image": "/assets/watermark.png",
      "position": "bottom-right",
      "opacity": 0.5,
      "minWidth": 600
    }
  }
}
```

---

### MEDIA-DELETE-001: Eliminar Media

| Campo          | Valor            |
| -------------- | ---------------- |
| **ID**         | MEDIA-DELETE-001 |
| **Nombre**     | Eliminar Media   |
| **Actor**      | Seller/Dealer    |
| **Criticidad** | 🟠 ALTO          |
| **Estado**     | 🟢 ACTIVO        |

#### Request

```
DELETE /api/media/img_a1b2c3d4e5f6
```

#### Flujo Paso a Paso

| Paso | Acción                  | Servicio   | Validación          |
| ---- | ----------------------- | ---------- | ------------------- |
| 1    | Buscar MediaAsset       | Repository | Existe              |
| 2    | Validar ownership       | Handler    | DealerId matches    |
| 3    | Marcar como Deleted     | Repository | Soft delete         |
| 4    | Publicar evento         | RabbitMQ   | media.deleted       |
| 5    | (Worker) Eliminar de S3 | S3Service  | Original + variants |
| 6    | (Worker) Invalidar CDN  | CDNService | Cache purge         |
| 7    | (Worker) Hard delete    | Repository | DELETE              |

#### Response (200)

```json
{
  "success": true,
  "message": "Media scheduled for deletion"
}
```

---

### MEDIA-DOC-001: Upload de Documento KYC

| Campo          | Valor                         |
| -------------- | ----------------------------- |
| **ID**         | MEDIA-DOC-001                 |
| **Nombre**     | Upload Documento Verificación |
| **Actor**      | Dealer                        |
| **Criticidad** | 🔴 CRÍTICO                    |
| **Estado**     | 🟢 ACTIVO                     |

#### Precondiciones

- [ ] Dealer registrado
- [ ] Documento < 25 MB
- [ ] Formato PDF, DOC, DOCX, o imagen

#### Request (Init)

```json
{
  "ownerId": "dealer_xyz789",
  "context": "kyc-documents",
  "fileName": "RNC-certificado.pdf",
  "contentType": "application/pdf",
  "fileSize": 1234567
}
```

#### Procesamiento Especial Documentos

| Paso | Acción           | Descripción             |
| ---- | ---------------- | ----------------------- |
| 1    | Validar PDF      | Verificar no corrupto   |
| 2    | Escaneo virus    | ClamAV integration      |
| 3    | Generar preview  | Primera página como PNG |
| 4    | Encriptar en S3  | SSE-S3 encryption       |
| 5    | No hacer público | Sin CDN URL             |

#### Response

```json
{
  "success": true,
  "data": {
    "mediaId": "doc_x1y2z3",
    "status": "Processed",
    "previewUrl": "https://api.okla.com.do/api/media/doc_x1y2z3/preview",
    "isEncrypted": true,
    "virusScanResult": "clean"
  }
}
```

---

### MEDIA-VIDEO-001: Upload de Video

| Campo          | Valor                 |
| -------------- | --------------------- |
| **ID**         | MEDIA-VIDEO-001       |
| **Nombre**     | Upload Video Vehículo |
| **Actor**      | Dealer                |
| **Criticidad** | 🟠 ALTO               |
| **Estado**     | 🟢 ACTIVO             |

#### Límites Video

| Aspecto           | Límite        |
| ----------------- | ------------- |
| Tamaño máximo     | 100 MB        |
| Duración máxima   | 2 minutos     |
| Formatos          | MP4, MOV, AVI |
| Resolución máxima | 4K            |

#### Procesamiento Video

| Paso | Acción                | Output               |
| ---- | --------------------- | -------------------- |
| 1    | Validar formato       | FFprobe              |
| 2    | Extraer metadata      | Duration, resolution |
| 3    | Generar thumbnail     | Frame en 0:01        |
| 4    | Transcodificar a MP4  | H.264, AAC           |
| 5    | Generar versión 720p  | Para mobile          |
| 6    | Generar versión 1080p | Para web             |
| 7    | Agregar watermark     | Logo animado         |
| 8    | Generar HLS playlist  | Streaming            |

#### Variantes Video

| Variante    | Resolución | Bitrate  | Uso           |
| ----------- | ---------- | -------- | ------------- |
| `thumbnail` | 320x180    | -        | Preview image |
| `mobile`    | 720p       | 2 Mbps   | Mobile app    |
| `web`       | 1080p      | 5 Mbps   | Desktop       |
| `original`  | Original   | Original | Backup        |

---

## 5. Storage Structure

### 5.1 Estructura de Buckets

```
okla-media/
├── vehicles/
│   ├── {dealerId}/
│   │   ├── {vehicleId}/
│   │   │   ├── original/
│   │   │   │   └── img_abc123_original.jpg
│   │   │   ├── thumbnail/
│   │   │   │   └── img_abc123_thumb.webp
│   │   │   ├── medium/
│   │   │   │   └── img_abc123_medium.webp
│   │   │   └── large/
│   │   │       └── img_abc123_large.webp
├── profiles/
│   ├── {userId}/
│   │   └── avatar/
│   │       └── img_xyz789.webp
├── dealers/
│   ├── {dealerId}/
│   │   ├── logo/
│   │   └── banner/
└── kyc/
    └── {dealerId}/
        ├── rnc/
        ├── license/
        └── id/
```

### 5.2 URLs y CDN

| Tipo             | URL Pattern                                                       |
| ---------------- | ----------------------------------------------------------------- |
| CDN público      | `https://cdn.okla.com.do/{path}`                                  |
| Presigned (temp) | `https://okla-media.nyc3.digitaloceanspaces.com/{path}?X-Amz-...` |
| API proxy        | `https://api.okla.com.do/api/media/{mediaId}`                     |
| KYC (private)    | Solo via API con auth                                             |

---

## 6. Reglas de Negocio

### 6.1 Límites por Plan

| Plan              | Max Imágenes/Vehículo | Max Videos | Total Storage |
| ----------------- | --------------------- | ---------- | ------------- |
| Individual Seller | 10                    | 0          | 50 MB         |
| Starter           | 15                    | 1          | 500 MB        |
| Pro               | 25                    | 2          | 2 GB          |
| Enterprise        | 50                    | 5          | Ilimitado     |

### 6.2 Retención

| Contexto           | Retención              |
| ------------------ | ---------------------- |
| Vehículo activo    | Indefinido             |
| Vehículo vendido   | 30 días                |
| Vehículo eliminado | 7 días                 |
| KYC documentos     | 7 años (regulación)    |
| Profile photos     | Mientras cuenta activa |

### 6.3 Moderación

| Regla                         | Acción                  |
| ----------------------------- | ----------------------- |
| Contenido inapropiado         | Auto-detect (AI) + flag |
| Watermark de terceros         | Rechazar                |
| Imagen genérica stock         | Warning                 |
| Múltiples vehículos en imagen | Warning                 |

---

## 7. Manejo de Errores

### 7.1 Códigos de Error

| Código   | Nombre            | HTTP | Descripción            |
| -------- | ----------------- | ---- | ---------------------- |
| MEDIA001 | FILE_TOO_LARGE    | 400  | Excede tamaño máximo   |
| MEDIA002 | INVALID_FORMAT    | 400  | Formato no soportado   |
| MEDIA003 | UPLOAD_EXPIRED    | 400  | Presigned URL expiró   |
| MEDIA004 | MEDIA_NOT_FOUND   | 404  | Media no existe        |
| MEDIA005 | PROCESSING_FAILED | 500  | Error en procesamiento |
| MEDIA006 | QUOTA_EXCEEDED    | 403  | Límite de storage      |
| MEDIA007 | ACCESS_DENIED     | 403  | No tiene permiso       |
| MEDIA008 | VIRUS_DETECTED    | 400  | Archivo con virus      |
| MEDIA009 | CORRUPT_FILE      | 400  | Archivo corrupto       |
| MEDIA010 | DUPLICATE_FILE    | 409  | Hash duplicado         |

---

## 8. Eventos RabbitMQ

### 8.1 Eventos Publicados

| Evento          | Exchange     | Routing Key     | Payload                  |
| --------------- | ------------ | --------------- | ------------------------ |
| media.uploaded  | media.events | media.uploaded  | {mediaId, ownerId, type} |
| media.processed | media.events | media.processed | {mediaId, variants[]}    |
| media.failed    | media.events | media.failed    | {mediaId, error}         |
| media.deleted   | media.events | media.deleted   | {mediaId, ownerId}       |

### 8.2 Eventos Consumidos

| Evento          | Exchange       | Acción                   |
| --------------- | -------------- | ------------------------ |
| vehicle.deleted | vehicle.events | Cleanup media            |
| dealer.deleted  | user.events    | Cleanup all dealer media |
| user.deleted    | user.events    | Cleanup profile media    |

---

## 9. Seguridad

### 9.1 Acceso

| Contexto       | Acceso            | Método                |
| -------------- | ----------------- | --------------------- |
| Vehicle images | Público (via CDN) | Directo               |
| Profile photos | Público (via CDN) | Directo               |
| KYC documents  | Privado           | Presigned URL (5 min) |
| Dealer logos   | Público (via CDN) | Directo               |

### 9.2 Encriptación

| Tipo                | Encriptación |
| ------------------- | ------------ |
| En tránsito         | TLS 1.3      |
| En reposo (público) | SSE-S3       |
| En reposo (KYC)     | SSE-KMS      |

### 9.3 Content Validation

| Check            | Herramienta |
| ---------------- | ----------- |
| MIME type real   | Magic bytes |
| Virus scan       | ClamAV      |
| Image corruption | ImageSharp  |
| EXIF stripping   | Privacy     |

---

## 10. Configuración

### 10.1 appsettings.json

```json
{
  "Storage": {
    "Provider": "DigitalOcean",
    "Bucket": "okla-media",
    "Region": "nyc3",
    "AccessKey": "[DO_SPACES_KEY]",
    "SecretKey": "[DO_SPACES_SECRET]",
    "Endpoint": "https://nyc3.digitaloceanspaces.com"
  },
  "CDN": {
    "Enabled": true,
    "BaseUrl": "https://cdn.okla.com.do",
    "CacheMaxAge": 31536000
  },
  "Processing": {
    "MaxConcurrentJobs": 10,
    "TimeoutSeconds": 120,
    "RetryCount": 3,
    "TempDirectory": "/tmp/media-processing"
  },
  "Limits": {
    "MaxImageSizeMB": 10,
    "MaxVideoSizeMB": 100,
    "MaxDocumentSizeMB": 25,
    "PresignedUrlExpirationMinutes": 15
  },
  "Watermark": {
    "Enabled": true,
    "ImagePath": "/assets/watermark.png",
    "Position": "BottomRight",
    "Opacity": 0.5,
    "MinImageWidth": 600
  }
}
```

---

## 11. API Response Examples

### 11.1 Get Media

```json
{
  "success": true,
  "data": {
    "id": "img_a1b2c3d4e5f6",
    "ownerId": "vehicle_xyz789",
    "context": "vehicle-gallery",
    "type": "Image",
    "originalFileName": "toyota-camry-front.jpg",
    "contentType": "image/jpeg",
    "sizeBytes": 2457600,
    "status": "Processed",
    "cdnUrl": "https://cdn.okla.com.do/vehicles/dealer123/vehicle_xyz789/large/img_a1b2c3d4e5f6.webp",
    "variants": [
      {
        "type": "thumbnail",
        "url": "https://cdn.okla.com.do/.../thumb.webp",
        "width": 150,
        "height": 150,
        "sizeBytes": 8500
      },
      {
        "type": "medium",
        "url": "https://cdn.okla.com.do/.../medium.webp",
        "width": 800,
        "height": 600,
        "sizeBytes": 125000
      },
      {
        "type": "large",
        "url": "https://cdn.okla.com.do/.../large.webp",
        "width": 1920,
        "height": 1080,
        "sizeBytes": 450000
      }
    ],
    "metadata": {
      "width": 4032,
      "height": 3024,
      "hasWatermark": true
    },
    "createdAt": "2026-01-21T10:30:00Z",
    "processedAt": "2026-01-21T10:30:08Z"
  }
}
```

### 11.2 List by Owner

```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "img_001",
        "thumbnailUrl": "https://cdn.okla.com.do/.../thumb.webp",
        "status": "Processed",
        "isPrimary": true
      },
      {
        "id": "img_002",
        "thumbnailUrl": "https://cdn.okla.com.do/.../thumb.webp",
        "status": "Processed",
        "isPrimary": false
      }
    ],
    "total": 2,
    "primaryImageId": "img_001"
  }
}
```

---

**Documento generado:** Enero 21, 2026  
**Versión:** 1.0.0  
**Autor:** Equipo OKLA
