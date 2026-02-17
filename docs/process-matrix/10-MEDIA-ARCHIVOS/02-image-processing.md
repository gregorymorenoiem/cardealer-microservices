# 🖼️ Image Processing - Procesamiento de Imágenes - Matriz de Procesos

> **Componente:** ImageProcessingWorker (MediaService.Workers)  
> **Framework:** ImageSharp  
> **Última actualización:** Enero 25, 2026  
> **Estado de Implementación:** ✅ 100% Backend | N/A UI (Worker interno)

---

## ⚠️ AUDITORÍA DE ACCESO UI (Enero 25, 2026)

| Proceso            | Backend | UI Access | Observación                           |
| ------------------ | ------- | --------- | ------------------------------------- |
| Image Upload       | ✅ 100% | ✅ 100%   | Integrado en formularios de vehículos |
| Image Resize       | ✅ 100% | N/A       | Worker automático                     |
| Watermark          | ✅ 100% | N/A       | Aplicado automáticamente              |
| Image Optimization | ✅ 100% | N/A       | WebP/AVIF generados auto              |
| Variant Generation | ✅ 100% | N/A       | Thumbs, medium, large auto            |

### Rutas UI Existentes ✅

- `/sell` y `/dealer/publish` - Upload de imágenes integrado
- `/vehicles/:id/edit` - Editor de imágenes de vehículo
- Imágenes procesadas se sirven vía CDN automáticamente

### Rutas UI Faltantes 🔴

- Ninguna - Este es un worker de backend, no requiere UI directa

**Verificación Backend:** `MediaService.Workers` existe en `/backend/MediaService/MediaService.Workers/` ✅

---

## 📊 Resumen de Implementación

| Componente  | Total | Implementado | Pendiente | Estado |
| ----------- | ----- | ------------ | --------- | ------ |
| Workers     | 1     | 1            | 0         | 🟢     |
| IMG-PROC-\* | 8     | 8            | 0         | 🟢     |
| IMG-VAR-\*  | 5     | 5            | 0         | 🟢     |
| IMG-OPT-\*  | 4     | 4            | 0         | 🟢     |
| Tests       | 10    | 10           | 0         | ✅     |

**Leyenda:** ✅ Implementado + Tested | 🟢 Implementado | 🟡 En Progreso | 🔴 Pendiente

---

## 1. Información General

### 1.1 Descripción

Worker de procesamiento de imágenes que maneja todas las operaciones de transformación, optimización y generación de variantes para las imágenes de la plataforma OKLA.

### 1.2 Operaciones Soportadas

| Operación     | Descripción              |
| ------------- | ------------------------ |
| **Resize**    | Cambiar dimensiones      |
| **Crop**      | Recortar imagen          |
| **Rotate**    | Rotar (90°, 180°, 270°)  |
| **Compress**  | Optimizar tamaño         |
| **Convert**   | Cambiar formato          |
| **Watermark** | Agregar marca de agua    |
| **Blur**      | Aplicar desenfoque       |
| **Sharpen**   | Aumentar nitidez         |
| **Grayscale** | Escala de grises         |
| **Thumbnail** | Generar miniatura        |
| **Blurhash**  | Generar placeholder hash |

### 1.3 Dependencias

| Librería                 | Versión | Uso                         |
| ------------------------ | ------- | --------------------------- |
| SixLabors.ImageSharp     | 3.1.x   | Procesamiento principal     |
| SixLabors.ImageSharp.Web | 3.0.x   | Transformaciones on-the-fly |
| Blurhash.ImageSharp      | 2.0.x   | Generación de blurhash      |
| ImageMagick              | 7.x     | Formatos especiales (HEIC)  |

---

## 2. Pipeline de Procesamiento

### 2.1 Flujo Estándar

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    Image Processing Pipeline                             │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│   ┌────────────┐                                                        │
│   │  Original  │                                                        │
│   │   Image    │                                                        │
│   └─────┬──────┘                                                        │
│         │                                                               │
│         ▼                                                               │
│   ┌────────────────────────────────────────────────────────────────┐   │
│   │                        VALIDATION                               │   │
│   │  • Check dimensions (800x600 min, 8000x8000 max)               │   │
│   │  • Check file size (max 10MB)                                  │   │
│   │  • Detect format (JPEG, PNG, WebP, HEIC)                       │   │
│   │  • Check for corruption                                        │   │
│   └─────────────────────────────┬──────────────────────────────────┘   │
│                                 │                                       │
│                                 ▼                                       │
│   ┌────────────────────────────────────────────────────────────────┐   │
│   │                     PRE-PROCESSING                              │   │
│   │  • EXIF orientation fix                                        │   │
│   │  • Color profile conversion (sRGB)                             │   │
│   │  • Strip unnecessary metadata                                  │   │
│   │  • Convert HEIC to JPEG (if needed)                            │   │
│   └─────────────────────────────┬──────────────────────────────────┘   │
│                                 │                                       │
│         ┌───────────────────────┼───────────────────────┐              │
│         │                       │                       │              │
│         ▼                       ▼                       ▼              │
│   ┌───────────┐          ┌───────────┐          ┌───────────┐         │
│   │   Large   │          │  Medium   │          │   Small   │         │
│   │ 1920x1440 │          │  800x600  │          │  400x300  │         │
│   │   WebP    │          │   WebP    │          │   WebP    │         │
│   └───────────┘          └───────────┘          └───────────┘         │
│                                                                          │
│         ┌───────────────────────┼───────────────────────┐              │
│         │                       │                       │              │
│         ▼                       ▼                       ▼              │
│   ┌───────────┐          ┌───────────┐          ┌───────────┐         │
│   │ Thumbnail │          │  Blurhash │          │  Original │         │
│   │  150x112  │          │   20x15   │          │  Optimized│         │
│   │   WebP    │          │   Hash    │          │   JPEG    │         │
│   └───────────┘          └───────────┘          └───────────┘         │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 3. Especificaciones de Variantes

### 3.1 Variantes para Vehículos

| Variante    | Dimensiones | Formato | Calidad | Uso                |
| ----------- | ----------- | ------- | ------- | ------------------ |
| `original`  | Original    | JPEG    | 90%     | Descarga           |
| `large`     | 1920 × 1440 | WebP    | 85%     | Galería fullscreen |
| `medium`    | 800 × 600   | WebP    | 80%     | Card principal     |
| `small`     | 400 × 300   | WebP    | 75%     | Grid/Lista         |
| `thumbnail` | 150 × 112   | WebP    | 70%     | Miniaturas         |
| `blur`      | 20 × 15     | WebP    | 60%     | Placeholder        |

### 3.2 Variantes para Avatares

| Variante   | Dimensiones | Formato | Calidad |
| ---------- | ----------- | ------- | ------- |
| `original` | Original    | JPEG    | 90%     |
| `large`    | 200 × 200   | WebP    | 85%     |
| `medium`   | 100 × 100   | WebP    | 80%     |
| `small`    | 50 × 50     | WebP    | 75%     |
| `tiny`     | 24 × 24     | WebP    | 70%     |

### 3.3 Variantes para Logos

| Variante   | Dimensiones | Formato | Calidad |
| ---------- | ----------- | ------- | ------- |
| `original` | Original    | PNG     | 100%    |
| `large`    | 400 × auto  | PNG     | 100%    |
| `medium`   | 200 × auto  | PNG     | 100%    |
| `small`    | 100 × auto  | PNG     | 100%    |

---

## 4. Procesos Detallados

### 4.1 IMG-001: Resize con Aspect Ratio

| Campo       | Valor                          |
| ----------- | ------------------------------ |
| **ID**      | IMG-001                        |
| **Nombre**  | Resize Preserving Aspect Ratio |
| **Actor**   | Worker                         |
| **Trigger** | Job en cola                    |

#### Algoritmo

```csharp
public async Task<Image> ResizeAsync(
    Image source,
    int targetWidth,
    int targetHeight,
    ResizeMode mode)
{
    var options = new ResizeOptions
    {
        Size = new Size(targetWidth, targetHeight),
        Mode = mode,
        Sampler = KnownResamplers.Lanczos3,
        Compand = true
    };

    source.Mutate(x => x.Resize(options));
    return source;
}
```

#### Modos de Resize

| Modo      | Descripción                   | Uso               |
| --------- | ----------------------------- | ----------------- |
| `Crop`    | Llena el área, recorta exceso | Cards, thumbnails |
| `Pad`     | Ajusta dentro, agrega padding | Logos             |
| `BoxPad`  | Pad con aspect ratio fijo     | Galería           |
| `Max`     | Ajusta al máximo sin exceder  | General           |
| `Min`     | Ajusta al mínimo cubriendo    | Backgrounds       |
| `Stretch` | Estira para llenar            | No recomendado    |

---

### 4.2 IMG-002: Optimización de Compresión

| Campo       | Valor             |
| ----------- | ----------------- |
| **ID**      | IMG-002           |
| **Nombre**  | Smart Compression |
| **Actor**   | Worker            |
| **Trigger** | Después de resize |

#### Estrategia de Compresión

| Formato | Encoder | Configuración                     |
| ------- | ------- | --------------------------------- |
| WebP    | LibWebP | Quality: 75-85, Method: 4         |
| JPEG    | MozJpeg | Quality: 80-90, Progressive: true |
| PNG     | Oxipng  | Compression: 3, Strip: true       |

#### Algoritmo

```csharp
public async Task<Stream> CompressAsync(Image image, OutputFormat format, int quality)
{
    var stream = new MemoryStream();

    switch (format)
    {
        case OutputFormat.WebP:
            await image.SaveAsWebpAsync(stream, new WebpEncoder
            {
                Quality = quality,
                Method = WebpEncodingMethod.BestQuality,
                NearLossless = false
            });
            break;

        case OutputFormat.Jpeg:
            await image.SaveAsJpegAsync(stream, new JpegEncoder
            {
                Quality = quality,
                ColorType = JpegEncodingColor.YCbCrRatio420
            });
            break;

        case OutputFormat.Png:
            await image.SaveAsPngAsync(stream, new PngEncoder
            {
                CompressionLevel = PngCompressionLevel.BestCompression,
                ColorType = PngColorType.RgbWithAlpha
            });
            break;
    }

    stream.Position = 0;
    return stream;
}
```

---

### 4.3 IMG-003: Generación de Blurhash

| Campo       | Valor                          |
| ----------- | ------------------------------ |
| **ID**      | IMG-003                        |
| **Nombre**  | Blurhash Generation            |
| **Actor**   | Worker                         |
| **Trigger** | Después de crear variante blur |

#### Descripción

Blurhash es un algoritmo que genera una cadena corta representando un placeholder blur de una imagen. Perfecto para lazy loading.

#### Implementación

```csharp
public string GenerateBlurhash(Image<Rgba32> image, int componentsX = 4, int componentsY = 3)
{
    // Resize a 20x15 para procesamiento
    image.Mutate(x => x.Resize(20, 15));

    var pixels = new Rgba32[20 * 15];
    image.CopyPixelDataTo(pixels);

    return Blurhash.Core.Encode(pixels, 20, 15, componentsX, componentsY);
}
```

#### Ejemplo de Blurhash

```
"LEHV6nWB2yk8pyo0adR*.7kCMdnj"
```

#### Uso en Frontend

```html
<!-- Placeholder blur mientras carga imagen real -->
<div style="background: url(data:image/svg+xml,${blurhashToSvg(hash)})">
  <img src="real-image.webp" loading="lazy" />
</div>
```

---

### 4.4 IMG-004: Watermark de Dealer

| Campo       | Valor                        |
| ----------- | ---------------------------- |
| **ID**      | IMG-004                      |
| **Nombre**  | Dealer Watermark Application |
| **Actor**   | Worker                       |
| **Trigger** | Request o batch              |

#### Flujo del Proceso

| Paso | Acción                  | Sistema    | Detalle         |
| ---- | ----------------------- | ---------- | --------------- |
| 1    | Obtener imagen original | S3         | Download        |
| 2    | Obtener logo dealer     | S3         | Cache local     |
| 3    | Resize logo             | ImageSharp | Max 200px ancho |
| 4    | Aplicar opacidad        | ImageSharp | 30-50%          |
| 5    | Calcular posición       | Algorithm  | Según config    |
| 6    | Overlay logo            | ImageSharp | DrawImage       |
| 7    | Guardar variante        | S3         | `_watermarked`  |

#### Posiciones Disponibles

```
┌─────────────────────────────────────────┐
│  TL                                 TR  │
│                                         │
│                                         │
│                  CENTER                 │
│                                         │
│                                         │
│  BL          BOTTOM-BANNER          BR  │
└─────────────────────────────────────────┘
```

#### Implementación

```csharp
public async Task ApplyWatermarkAsync(
    Image image,
    Image watermark,
    WatermarkPosition position,
    float opacity)
{
    // Resize watermark to max 15% of image width
    var maxWidth = (int)(image.Width * 0.15);
    if (watermark.Width > maxWidth)
    {
        var ratio = (float)maxWidth / watermark.Width;
        watermark.Mutate(x => x.Resize((int)(watermark.Width * ratio), 0));
    }

    // Calculate position
    var point = CalculatePosition(image.Size, watermark.Size, position);

    // Apply with opacity
    image.Mutate(x => x.DrawImage(watermark, point, opacity));
}
```

---

### 4.5 IMG-005: Detección de Contenido Inapropiado

| Campo       | Valor                   |
| ----------- | ----------------------- |
| **ID**      | IMG-005                 |
| **Nombre**  | NSFW Content Detection  |
| **Actor**   | Worker                  |
| **Trigger** | Antes de aprobar imagen |

#### Flujo del Proceso

| Paso | Acción             | Sistema    | Detalle           |
| ---- | ------------------ | ---------- | ----------------- |
| 1    | Recibir imagen     | Worker     | Nueva subida      |
| 2    | Enviar a modelo ML | ML Service | AWS Rekognition   |
| 3    | Obtener score      | Response   | 0-100             |
| 4    | Si score > 80      | Rechazar   | Status = Rejected |
| 5    | Si score 50-80     | Review     | Manual review     |
| 6    | Si score < 50      | Aprobar    | Status = Approved |
| 7    | Log resultado      | Database   | Para auditoría    |

#### Categorías Detectadas

| Categoría       | Umbral Rechazo | Umbral Review |
| --------------- | -------------- | ------------- |
| Explicit Nudity | 80%            | 50%           |
| Violence        | 90%            | 70%           |
| Drugs           | 90%            | 70%           |
| Hate Symbols    | 95%            | 80%           |
| Gambling        | 85%            | 60%           |

---

### 4.6 IMG-006: Corrección de Orientación EXIF

| Campo       | Valor                |
| ----------- | -------------------- |
| **ID**      | IMG-006              |
| **Nombre**  | EXIF Orientation Fix |
| **Actor**   | Worker               |
| **Trigger** | Pre-procesamiento    |

#### Problema

Las fotos tomadas con móviles pueden tener orientación EXIF diferente a la visual.

#### Valores de Orientación EXIF

| Valor | Transformación                  |
| ----- | ------------------------------- |
| 1     | Normal                          |
| 2     | Flip horizontal                 |
| 3     | Rotar 180°                      |
| 4     | Flip vertical                   |
| 5     | Rotar 90° CW + Flip horizontal  |
| 6     | Rotar 90° CW                    |
| 7     | Rotar 90° CCW + Flip horizontal |
| 8     | Rotar 90° CCW                   |

#### Implementación

```csharp
public void FixOrientation(Image image)
{
    image.Mutate(x => x.AutoOrient());
}
```

---

## 5. Configuración de Quality Profiles

### 5.1 Perfiles Predefinidos

```json
{
  "QualityProfiles": {
    "vehicle_photos": {
      "format": "webp",
      "quality": 85,
      "variants": ["large", "medium", "small", "thumbnail", "blur"],
      "watermark": true
    },
    "dealer_logo": {
      "format": "png",
      "quality": 100,
      "variants": ["large", "medium", "small"],
      "watermark": false,
      "preserveTransparency": true
    },
    "user_avatar": {
      "format": "webp",
      "quality": 80,
      "variants": ["large", "medium", "small", "tiny"],
      "watermark": false,
      "crop": "circle"
    },
    "document": {
      "format": "original",
      "quality": 100,
      "variants": ["original"],
      "watermark": false,
      "optimize": false
    }
  }
}
```

---

## 6. Performance y Optimización

### 6.1 Concurrent Processing

```csharp
public async Task ProcessBatchAsync(IEnumerable<ProcessingJob> jobs)
{
    var options = new ParallelOptions
    {
        MaxDegreeOfParallelism = Environment.ProcessorCount
    };

    await Parallel.ForEachAsync(jobs, options, async (job, ct) =>
    {
        await ProcessSingleAsync(job, ct);
    });
}
```

### 6.2 Memory Management

| Estrategia | Implementación                      |
| ---------- | ----------------------------------- |
| Streaming  | Procesar sin cargar todo en memoria |
| Dispose    | Usar `using` statements             |
| Pool       | Reutilizar buffers con ArrayPool    |
| Limit      | Max 4 imágenes grandes simultáneas  |

### 6.3 Benchmarks

| Operación                   | Tiempo Promedio |
| --------------------------- | --------------- |
| Resize 4000x3000 → 800x600  | 45ms            |
| WebP encode (quality 80)    | 120ms           |
| Blurhash generation         | 15ms            |
| Full pipeline (6 variantes) | 650ms           |
| Watermark application       | 80ms            |

---

## 7. Eventos RabbitMQ

| Evento                       | Exchange       | Payload                        |
| ---------------------------- | -------------- | ------------------------------ |
| `image.processing.started`   | `media.events` | `{ mediaId, variants[] }`      |
| `image.processing.completed` | `media.events` | `{ mediaId, results[] }`       |
| `image.processing.failed`    | `media.events` | `{ mediaId, error }`           |
| `image.moderation.flagged`   | `media.events` | `{ mediaId, category, score }` |

---

## 8. Métricas

```
# Processing
image_processing_total{operation="...", format="..."}
image_processing_duration_seconds{operation="..."}
image_processing_failures_total{reason="..."}

# Quality
image_compression_ratio
image_original_size_bytes
image_processed_size_bytes

# Moderation
image_moderation_flagged_total{category="..."}
image_moderation_approved_total
```

---

## 9. Códigos de Error

| Código    | Mensaje               | Causa                 |
| --------- | --------------------- | --------------------- |
| `IMG_001` | Invalid format        | Formato no soportado  |
| `IMG_002` | Dimensions too small  | < 800x600             |
| `IMG_003` | Dimensions too large  | > 8000x8000           |
| `IMG_004` | Corrupt image         | No se puede procesar  |
| `IMG_005` | Processing timeout    | > 30 segundos         |
| `IMG_006` | Memory exceeded       | Imagen muy grande     |
| `IMG_007` | NSFW content detected | Contenido inapropiado |

---

## 📚 Referencias

- [01-media-service.md](01-media-service.md) - Servicio principal de media
- [ImageSharp Docs](https://docs.sixlabors.com/api/ImageSharp/) - Documentación oficial
- [Blurhash Algorithm](https://blurha.sh/) - Especificación
