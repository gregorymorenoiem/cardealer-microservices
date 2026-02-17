# 🎬 Multimedia Processing - Procesamiento Multimedia - Matriz de Procesos

> **Servicio:** MediaService  
> **Puerto:** 5016  
> **Última actualización:** Enero 25, 2026  
> **Estado de Implementación:** ✅ 95% Backend | 🟡 75% UI

---

## ⚠️ AUDITORÍA DE ACCESO UI (Enero 25, 2026)

| Proceso             | Backend | UI Access | Observación                     |
| ------------------- | ------- | --------- | ------------------------------- |
| Image Processing    | ✅ 100% | ✅ 100%   | Upload en todos los formularios |
| Video Upload        | ✅ 95%  | ✅ 80%    | Soportado en dealer listings    |
| Video Transcoding   | ✅ 90%  | N/A       | FFmpeg worker automático        |
| 360° Viewer         | 🟡 70%  | 🟡 60%    | Viewer básico implementado      |
| Document Processing | ✅ 90%  | ✅ 70%    | PDF preview, OCR parcial        |

### Rutas UI Existentes ✅

- `/sell` y `/dealer/publish` - Upload multimedia completo
- `/vehicles/:slug` - Galería con viewer 360 básico
- `/vehicles/:id/edit` - Editor de multimedia
- CDN CloudFront para delivery

### Rutas UI Faltantes 🔴

- `/media/editor` - Editor avanzado de imágenes
- `/media/360/builder` - Constructor de tours 360
- `/admin/media/stats` - Estadísticas de uso de almacenamiento

**Verificación Backend:** `MediaService` existe en `/backend/MediaService/` ✅

---

## 📊 Resumen de Implementación

| Componente   | Total | Implementado | Pendiente | Estado |
| ------------ | ----- | ------------ | --------- | ------ |
| Controllers  | 2     | 0            | 2         | 🔴     |
| MEDIA-IMG-\* | 6     | 0            | 6         | 🔴     |
| MEDIA-VID-\* | 5     | 0            | 5         | 🔴     |
| MEDIA-DOC-\* | 3     | 0            | 3         | 🔴     |
| MEDIA-360-\* | 4     | 0            | 4         | 🔴     |
| Tests        | 0     | 0            | 12        | 🔴     |

**Leyenda:** ✅ Implementado + Tested | 🟢 Implementado | 🟡 En Progreso | 🔴 Pendiente

---

## 1. Información General

### 1.1 Descripción

Sistema de procesamiento de archivos multimedia para OKLA. Maneja procesamiento de imágenes (resize, watermark, compression), videos (transcoding, thumbnails), y documentos PDF. Integración con AWS S3 para almacenamiento y CloudFront para CDN.

### 1.2 Tipos de Procesamiento

| Tipo            | Descripción                          | Formatos              |
| --------------- | ------------------------------------ | --------------------- |
| **Images**      | Resize, crop, watermark, compression | JPG, PNG, WebP        |
| **Videos**      | Transcoding, thumbnails, streaming   | MP4, MOV, WebM        |
| **Documents**   | PDF preview, thumbnails              | PDF                   |
| **360° Photos** | Panoramic processing                 | JPG (equirectangular) |

### 1.3 Arquitectura

```
┌─────────────────────────────────────────────────────────────────────────┐
│                   Multimedia Processing Architecture                     │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│   Upload Request                                                        │
│        │                                                                │
│        ▼                                                                │
│   ┌─────────────────────────────────────────────────────────────────┐   │
│   │                     MediaService API                             │   │
│   │                                                                   │   │
│   │   1. Validate file (size, type, virus scan)                      │   │
│   │   2. Upload original to S3 (/originals/)                         │   │
│   │   3. Queue processing job                                        │   │
│   │   4. Return media ID + processing status                         │   │
│   └───────────────────────────────┬─────────────────────────────────┘   │
│                                   │                                      │
│                                   ▼                                      │
│   ┌─────────────────────────────────────────────────────────────────┐   │
│   │                     RabbitMQ Queue                               │   │
│   │                                                                   │   │
│   │   media.processing.images                                        │   │
│   │   media.processing.videos                                        │   │
│   │   media.processing.documents                                     │   │
│   └───────────────────────────────┬─────────────────────────────────┘   │
│                                   │                                      │
│              ┌────────────────────┼────────────────────┐                │
│              │                    │                    │                 │
│              ▼                    ▼                    ▼                 │
│   ┌──────────────────┐ ┌──────────────────┐ ┌──────────────────┐        │
│   │  Image Worker    │ │  Video Worker    │ │ Document Worker  │        │
│   │                  │ │                  │ │                  │        │
│   │  - ImageSharp    │ │  - FFmpeg        │ │  - Ghostscript   │        │
│   │  - Resize        │ │  - H.264/HEVC    │ │  - ImageMagick   │        │
│   │  - Watermark     │ │  - HLS/DASH      │ │  - PDF Preview   │        │
│   │  - WebP convert  │ │  - Thumbnails    │ │                  │        │
│   └────────┬─────────┘ └────────┬─────────┘ └────────┬─────────┘        │
│            │                    │                    │                   │
│            └────────────────────┴────────────────────┘                   │
│                                 │                                        │
│                                 ▼                                        │
│   ┌─────────────────────────────────────────────────────────────────┐   │
│   │                          AWS S3                                  │   │
│   │                                                                   │   │
│   │   /originals/      → Archivos originales                        │   │
│   │   /processed/      → Versiones procesadas                       │   │
│   │     /thumbnails/   → Miniaturas (150x150, 300x300)             │   │
│   │     /medium/       → Tamaño medio (800x600)                     │   │
│   │     /large/        → Tamaño grande (1920x1080)                  │   │
│   │     /watermarked/  → Con marca de agua                          │   │
│   │   /videos/         → Videos transcodificados                    │   │
│   │     /hls/          → HLS segments                               │   │
│   └───────────────────────────────┬─────────────────────────────────┘   │
│                                   │                                      │
│                                   ▼                                      │
│   ┌─────────────────────────────────────────────────────────────────┐   │
│   │                        CloudFront CDN                            │   │
│   │                  https://media.okla.com.do                       │   │
│   └─────────────────────────────────────────────────────────────────┘   │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 2. Endpoints

### 2.1 Upload

| Método | Endpoint                                        | Descripción           | Auth |
| ------ | ----------------------------------------------- | --------------------- | ---- |
| `POST` | `/api/media/upload`                             | Upload single file    | User |
| `POST` | `/api/media/upload/bulk`                        | Upload multiple files | User |
| `POST` | `/api/media/upload/chunked/init`                | Init chunked upload   | User |
| `PUT`  | `/api/media/upload/chunked/{uploadId}`          | Upload chunk          | User |
| `POST` | `/api/media/upload/chunked/{uploadId}/complete` | Complete upload       | User |

### 2.2 Processing Status

| Método | Endpoint                   | Descripción       | Auth   |
| ------ | -------------------------- | ----------------- | ------ |
| `GET`  | `/api/media/{id}`          | Get media info    | Public |
| `GET`  | `/api/media/{id}/status`   | Processing status | User   |
| `GET`  | `/api/media/{id}/variants` | Get all variants  | Public |

### 2.3 Operations

| Método   | Endpoint                    | Descripción     | Auth  |
| -------- | --------------------------- | --------------- | ----- |
| `POST`   | `/api/media/{id}/reprocess` | Reprocess media | Admin |
| `DELETE` | `/api/media/{id}`           | Delete media    | User  |
| `POST`   | `/api/media/batch-delete`   | Bulk delete     | User  |

### 2.4 Presigned URLs

| Método | Endpoint              | Descripción              | Auth   |
| ------ | --------------------- | ------------------------ | ------ |
| `GET`  | `/api/media/{id}/url` | Get signed URL           | Public |
| `POST` | `/api/media/presign`  | Get presigned upload URL | User   |

---

## 3. Entidades

### 3.1 MediaFile

```csharp
public class MediaFile
{
    public Guid Id { get; set; }

    // Ownership
    public Guid OwnerId { get; set; }
    public Guid? DealerId { get; set; }
    public MediaOwnerType OwnerType { get; set; }

    // Original file
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string OriginalS3Key { get; set; } = string.Empty;

    // Classification
    public MediaType Type { get; set; }
    public MediaCategory Category { get; set; }

    // Processing
    public ProcessingStatus Status { get; set; }
    public string? ProcessingError { get; set; }
    public DateTime? ProcessedAt { get; set; }

    // Variants
    public List<MediaVariant> Variants { get; set; } = new();

    // Metadata
    public int? Width { get; set; }
    public int? Height { get; set; }
    public int? DurationSeconds { get; set; } // For videos
    public Dictionary<string, string> ExifData { get; set; } = new();

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class MediaVariant
{
    public Guid Id { get; set; }
    public Guid MediaFileId { get; set; }

    public VariantType Type { get; set; }
    public string S3Key { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;

    public int Width { get; set; }
    public int Height { get; set; }
    public long FileSizeBytes { get; set; }
    public string ContentType { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}

public enum MediaType
{
    Image,
    Video,
    Document,
    Panorama360
}

public enum MediaCategory
{
    VehiclePhoto,
    VehicleVideo,
    ProfilePhoto,
    DealerLogo,
    DealerDocument,
    Other
}

public enum ProcessingStatus
{
    Pending,
    Processing,
    Completed,
    Failed,
    Cancelled
}

public enum VariantType
{
    Thumbnail_150,
    Thumbnail_300,
    Medium_800,
    Large_1920,
    Watermarked,
    WebP_Thumbnail,
    WebP_Medium,
    WebP_Large,
    Video_720p,
    Video_1080p,
    HLS_Playlist,
    PDF_Preview
}
```

### 3.2 ProcessingJob

```csharp
public class ProcessingJob
{
    public Guid Id { get; set; }
    public Guid MediaFileId { get; set; }
    public JobType Type { get; set; }
    public JobStatus Status { get; set; }

    // Configuration
    public Dictionary<string, object> Options { get; set; } = new();

    // Progress
    public int ProgressPercent { get; set; }
    public string? CurrentStep { get; set; }

    // Timing
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Errors
    public int RetryCount { get; set; }
    public string? ErrorMessage { get; set; }
}

public enum JobType
{
    ImageResize,
    ImageWatermark,
    ImageWebPConvert,
    VideoTranscode,
    VideoThumbnail,
    VideoHLS,
    DocumentPreview,
    PanoramaProcess
}

public enum JobStatus
{
    Queued,
    Processing,
    Completed,
    Failed,
    Cancelled
}
```

---

## 4. Procesos Detallados

### 4.1 MEDIA-001: Upload y Procesamiento de Imagen

| Paso | Acción                   | Sistema      | Validación              |
| ---- | ------------------------ | ------------ | ----------------------- |
| 1    | Usuario sube imagen      | Frontend     | File selected           |
| 2    | Validar archivo          | MediaService | Size < 10MB, type valid |
| 3    | Virus scan               | ClamAV       | Clean                   |
| 4    | Upload a S3 /originals/  | S3           | Upload success          |
| 5    | Crear MediaFile record   | PostgreSQL   | Record saved            |
| 6    | Queue processing jobs    | RabbitMQ     | Jobs queued             |
| 7    | Worker recibe job        | Image Worker | Job received            |
| 8    | Descargar original de S3 | Worker       | Downloaded              |
| 9    | Generar thumbnails       | ImageSharp   | 150x150, 300x300        |
| 10   | Generar medium           | ImageSharp   | 800x max                |
| 11   | Generar large            | ImageSharp   | 1920x max               |
| 12   | Convertir a WebP         | ImageSharp   | WebP versions           |
| 13   | Agregar watermark        | ImageSharp   | OKLA watermark          |
| 14   | Upload variants a S3     | S3           | All uploaded            |
| 15   | Actualizar MediaFile     | PostgreSQL   | Status = Completed      |
| 16   | Publicar evento          | RabbitMQ     | MediaProcessedEvent     |

```csharp
public class ImageProcessingWorker : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        await foreach (var job in _queue.ReadAllAsync(ct))
        {
            try
            {
                await ProcessImageAsync(job, ct);
            }
            catch (Exception ex)
            {
                await HandleJobFailureAsync(job, ex, ct);
            }
        }
    }

    private async Task ProcessImageAsync(ProcessingJob job, CancellationToken ct)
    {
        var media = await _mediaRepository.GetByIdAsync(job.MediaFileId, ct);
        if (media == null) return;

        job.Status = JobStatus.Processing;
        job.StartedAt = DateTime.UtcNow;
        await _jobRepository.UpdateAsync(job, ct);

        // 1. Download original from S3
        var originalStream = await _s3Client.GetObjectStreamAsync(media.OriginalS3Key, ct);
        using var image = await Image.LoadAsync(originalStream, ct);

        // Extract metadata
        media.Width = image.Width;
        media.Height = image.Height;
        ExtractExifData(image, media);

        var variants = new List<MediaVariant>();

        // 2. Generate thumbnails
        job.CurrentStep = "Generating thumbnails";
        job.ProgressPercent = 10;
        await _jobRepository.UpdateAsync(job, ct);

        variants.Add(await GenerateVariantAsync(image, VariantType.Thumbnail_150, 150, 150, media.Id, ct));
        variants.Add(await GenerateVariantAsync(image, VariantType.Thumbnail_300, 300, 300, media.Id, ct));

        // 3. Generate medium size
        job.CurrentStep = "Generating medium size";
        job.ProgressPercent = 30;
        await _jobRepository.UpdateAsync(job, ct);

        variants.Add(await GenerateVariantAsync(image, VariantType.Medium_800, 800, 600, media.Id, ct));

        // 4. Generate large size
        job.CurrentStep = "Generating large size";
        job.ProgressPercent = 50;
        await _jobRepository.UpdateAsync(job, ct);

        variants.Add(await GenerateVariantAsync(image, VariantType.Large_1920, 1920, 1080, media.Id, ct));

        // 5. Convert to WebP
        job.CurrentStep = "Converting to WebP";
        job.ProgressPercent = 70;
        await _jobRepository.UpdateAsync(job, ct);

        variants.Add(await GenerateWebPVariantAsync(image, VariantType.WebP_Medium, 800, 600, media.Id, ct));
        variants.Add(await GenerateWebPVariantAsync(image, VariantType.WebP_Large, 1920, 1080, media.Id, ct));

        // 6. Add watermark (for vehicle photos)
        if (media.Category == MediaCategory.VehiclePhoto)
        {
            job.CurrentStep = "Adding watermark";
            job.ProgressPercent = 85;
            await _jobRepository.UpdateAsync(job, ct);

            variants.Add(await GenerateWatermarkedAsync(image, media.Id, ct));
        }

        // 7. Save variants and update media
        media.Variants = variants;
        media.Status = ProcessingStatus.Completed;
        media.ProcessedAt = DateTime.UtcNow;
        await _mediaRepository.UpdateAsync(media, ct);

        job.Status = JobStatus.Completed;
        job.ProgressPercent = 100;
        job.CompletedAt = DateTime.UtcNow;
        await _jobRepository.UpdateAsync(job, ct);

        // 8. Publish event
        await _eventBus.PublishAsync(new MediaProcessedEvent
        {
            MediaId = media.Id,
            OwnerId = media.OwnerId,
            Type = media.Type,
            VariantUrls = variants.ToDictionary(v => v.Type.ToString(), v => v.Url)
        }, ct);
    }

    private async Task<MediaVariant> GenerateVariantAsync(
        Image image, VariantType type, int width, int height, Guid mediaId, CancellationToken ct)
    {
        using var clone = image.Clone(ctx =>
        {
            ctx.Resize(new ResizeOptions
            {
                Size = new Size(width, height),
                Mode = ResizeMode.Max,
                Sampler = KnownResamplers.Lanczos3
            });
        });

        using var ms = new MemoryStream();
        await clone.SaveAsJpegAsync(ms, new JpegEncoder { Quality = 85 }, ct);
        ms.Position = 0;

        var s3Key = $"processed/{type.ToString().ToLower()}/{mediaId}.jpg";
        await _s3Client.UploadAsync(s3Key, ms, "image/jpeg", ct);

        return new MediaVariant
        {
            MediaFileId = mediaId,
            Type = type,
            S3Key = s3Key,
            Url = $"https://media.okla.com.do/{s3Key}",
            Width = clone.Width,
            Height = clone.Height,
            FileSizeBytes = ms.Length,
            ContentType = "image/jpeg"
        };
    }
}
```

### 4.2 MEDIA-002: Procesamiento de Video

| Paso | Acción               | Sistema      | Validación         |
| ---- | -------------------- | ------------ | ------------------ |
| 1    | Usuario sube video   | Frontend     | File < 100MB       |
| 2    | Chunked upload a S3  | S3           | All chunks         |
| 3    | Crear MediaFile      | PostgreSQL   | Record saved       |
| 4    | Queue video job      | RabbitMQ     | Job queued         |
| 5    | Worker recibe job    | Video Worker | Job received       |
| 6    | Analizar video       | FFprobe      | Metadata           |
| 7    | Transcodificar 720p  | FFmpeg       | H.264, AAC         |
| 8    | Transcodificar 1080p | FFmpeg       | H.264, AAC         |
| 9    | Generar thumbnails   | FFmpeg       | 3 frames           |
| 10   | Crear HLS playlist   | FFmpeg       | m3u8 + ts          |
| 11   | Upload variants      | S3           | All uploaded       |
| 12   | Actualizar MediaFile | PostgreSQL   | Status = Completed |

```
┌─────────────────────────────────────────────────────────────────────────┐
│                     Video Processing Pipeline                            │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│   Original Video (1080p, 30fps, H.264)                                  │
│        │                                                                │
│        ├─────────────────┐                                              │
│        │                 │                                              │
│        ▼                 ▼                                              │
│   ┌──────────┐     ┌──────────┐                                        │
│   │ Transcode│     │ Transcode│                                        │
│   │   720p   │     │  1080p   │                                        │
│   │  2Mbps   │     │  5Mbps   │                                        │
│   └────┬─────┘     └────┬─────┘                                        │
│        │                 │                                              │
│        └────────┬────────┘                                              │
│                 │                                                        │
│                 ▼                                                        │
│        ┌───────────────┐                                                │
│        │  Generate HLS │                                                │
│        │               │                                                │
│        │  master.m3u8  │                                                │
│        │  720p/        │                                                │
│        │    index.m3u8 │                                                │
│        │    seg001.ts  │                                                │
│        │    seg002.ts  │                                                │
│        │  1080p/       │                                                │
│        │    index.m3u8 │                                                │
│        │    seg001.ts  │                                                │
│        └───────────────┘                                                │
│                 │                                                        │
│                 ▼                                                        │
│        ┌───────────────┐                                                │
│        │  Thumbnails   │                                                │
│        │               │                                                │
│        │  0:00 - 25%   │                                                │
│        │  50%  - 75%   │                                                │
│        │  100% (final) │                                                │
│        └───────────────┘                                                │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

```csharp
public class VideoProcessingWorker
{
    public async Task ProcessVideoAsync(ProcessingJob job, CancellationToken ct)
    {
        var media = await _mediaRepository.GetByIdAsync(job.MediaFileId, ct);

        // Download original
        var tempPath = Path.Combine(_tempDir, $"{media.Id}.mp4");
        await _s3Client.DownloadToFileAsync(media.OriginalS3Key, tempPath, ct);

        try
        {
            // 1. Analyze video
            var probeResult = await FFProbe.AnalyseAsync(tempPath, null, ct);
            media.Width = probeResult.PrimaryVideoStream?.Width;
            media.Height = probeResult.PrimaryVideoStream?.Height;
            media.DurationSeconds = (int?)probeResult.Duration.TotalSeconds;

            // 2. Transcode to 720p
            var path720p = Path.Combine(_tempDir, $"{media.Id}_720p.mp4");
            await FFMpegArguments
                .FromFileInput(tempPath)
                .OutputToFile(path720p, overwrite: true, options => options
                    .WithVideoCodec("libx264")
                    .WithAudioCodec("aac")
                    .WithVideoBitrate(2000)
                    .Resize(1280, 720)
                    .WithFastStart())
                .ProcessAsynchronously();

            // Upload 720p
            var key720p = $"videos/{media.Id}/720p.mp4";
            await _s3Client.UploadFileAsync(key720p, path720p, "video/mp4", ct);

            // 3. Generate HLS
            var hlsDir = Path.Combine(_tempDir, $"{media.Id}_hls");
            Directory.CreateDirectory(hlsDir);

            await FFMpegArguments
                .FromFileInput(path720p)
                .OutputToFile(Path.Combine(hlsDir, "index.m3u8"), overwrite: true, options => options
                    .ForceFormat("hls")
                    .WithCustomArgument("-hls_time 6")
                    .WithCustomArgument("-hls_playlist_type vod")
                    .WithCustomArgument("-hls_segment_filename " +
                        Path.Combine(hlsDir, "seg%03d.ts")))
                .ProcessAsynchronously();

            // Upload HLS files
            foreach (var file in Directory.GetFiles(hlsDir))
            {
                var hlsKey = $"videos/{media.Id}/hls/{Path.GetFileName(file)}";
                await _s3Client.UploadFileAsync(hlsKey, file,
                    file.EndsWith(".m3u8") ? "application/x-mpegURL" : "video/MP2T", ct);
            }

            // 4. Generate thumbnails
            var thumbnailTimes = new[] { 0.0, 0.25, 0.5, 0.75, 1.0 };
            var duration = probeResult.Duration.TotalSeconds;

            for (int i = 0; i < thumbnailTimes.Length; i++)
            {
                var time = TimeSpan.FromSeconds(duration * thumbnailTimes[i]);
                var thumbPath = Path.Combine(_tempDir, $"{media.Id}_thumb_{i}.jpg");

                await FFMpeg.SnapshotAsync(tempPath, thumbPath, captureTime: time);

                var thumbKey = $"videos/{media.Id}/thumbnails/{i}.jpg";
                await _s3Client.UploadFileAsync(thumbKey, thumbPath, "image/jpeg", ct);
            }

            // Update media record
            media.Status = ProcessingStatus.Completed;
            media.ProcessedAt = DateTime.UtcNow;
            await _mediaRepository.UpdateAsync(media, ct);
        }
        finally
        {
            // Cleanup temp files
            CleanupTempFiles(media.Id);
        }
    }
}
```

---

## 5. Variantes de Imagen

| Variante        | Tamaño        | Formato | Uso             |
| --------------- | ------------- | ------- | --------------- |
| `Thumbnail_150` | 150x150       | JPEG    | Grid lists      |
| `Thumbnail_300` | 300x300       | JPEG    | Cards           |
| `Medium_800`    | 800x600 max   | JPEG    | Detail page     |
| `Large_1920`    | 1920x1080 max | JPEG    | Full screen     |
| `WebP_Medium`   | 800x600 max   | WebP    | Modern browsers |
| `WebP_Large`    | 1920x1080 max | WebP    | Modern browsers |
| `Watermarked`   | 1920x1080 max | JPEG    | Share/download  |

---

## 6. Reglas de Negocio

| Código    | Regla                                   | Validación          |
| --------- | --------------------------------------- | ------------------- |
| MEDIA-R01 | Imagen max 10MB                         | FileSizeBytes check |
| MEDIA-R02 | Video max 100MB (Starter), 500MB (Pro)  | Plan check          |
| MEDIA-R03 | Formatos permitidos: JPG, PNG, MP4, MOV | ContentType         |
| MEDIA-R04 | Watermark obligatorio para vehículos    | Category check      |
| MEDIA-R05 | Virus scan obligatorio                  | ClamAV result       |
| MEDIA-R06 | Retención: 30 días sin uso              | LastAccessedAt      |

---

## 7. Códigos de Error

| Código      | HTTP | Mensaje            | Causa                  |
| ----------- | ---- | ------------------ | ---------------------- |
| `MEDIA_001` | 413  | File too large     | Excede límite          |
| `MEDIA_002` | 415  | Unsupported format | Formato no válido      |
| `MEDIA_003` | 400  | Virus detected     | Archivo infectado      |
| `MEDIA_004` | 500  | Processing failed  | Error de procesamiento |
| `MEDIA_005` | 404  | Media not found    | No existe              |

---

## 8. Eventos RabbitMQ

| Evento                       | Exchange       | Descripción              |
| ---------------------------- | -------------- | ------------------------ |
| `MediaUploadedEvent`         | `media.events` | Archivo subido           |
| `MediaProcessedEvent`        | `media.events` | Procesamiento completado |
| `MediaProcessingFailedEvent` | `media.events` | Error procesamiento      |
| `MediaDeletedEvent`          | `media.events` | Archivo eliminado        |

---

## 9. Configuración

```json
{
  "Media": {
    "S3": {
      "Bucket": "okla-media",
      "Region": "us-east-1",
      "CdnUrl": "https://media.okla.com.do"
    },
    "Limits": {
      "MaxImageSizeMB": 10,
      "MaxVideoSizeMB": {
        "Starter": 100,
        "Pro": 500,
        "Enterprise": 1000
      }
    },
    "Processing": {
      "ImageQuality": 85,
      "VideoCodec": "libx264",
      "VideoBitrates": {
        "720p": 2000,
        "1080p": 5000
      },
      "HlsSegmentSeconds": 6
    },
    "Watermark": {
      "ImagePath": "/assets/watermark.png",
      "Position": "BottomRight",
      "Opacity": 0.5
    }
  }
}
```

---

## 10. Métricas Prometheus

```
# Upload
media_uploads_total{type="image|video|document"}
media_upload_size_bytes{type="..."}

# Processing
media_processing_duration_seconds{type="...", variant="..."}
media_processing_failures_total{type="...", error="..."}
media_processing_queue_size

# Storage
media_storage_bytes_total{type="...", variant="..."}
```

---

## 📚 Referencias

- [01-upload.md](01-upload.md) - Upload de archivos
- [02-images.md](02-images.md) - Procesamiento de imágenes
- [03-storage.md](03-storage.md) - Almacenamiento S3
