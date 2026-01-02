# 📸 SPRINT 4 - Media Upload y Gestión de Imágenes

**Fecha:** 2 Enero 2026  
**Duración estimada:** 4-5 horas  
**Tokens estimados:** ~24,000  
**Prioridad:** 🔴 Crítica

---

## 🎯 OBJETIVOS

1. Integrar AWS S3 con MediaService
2. Implementar upload de imágenes con compresión automática
3. Crear sistema de progressive image loading
4. Generar thumbnails automáticamente
5. Implementar drag & drop en frontend
6. Agregar validaciones de tipo y tamaño
7. Conectar uploads con VehicleService

---

## 📋 CHECKLIST DE TAREAS

### Fase 1: AWS S3 Configuration (30 min)

- [ ] 1.1. Crear bucket S3 en AWS
- [ ] 1.2. Configurar CORS en bucket
- [ ] 1.3. Crear IAM user con permisos
- [ ] 1.4. Obtener Access Key y Secret Key
- [ ] 1.5. Configurar bucket policies

### Fase 2: Backend - MediaService Enhancement (2 horas)

- [ ] 2.1. Instalar AWS SDK for .NET
- [ ] 2.2. Crear S3StorageService
- [ ] 2.3. Implementar upload con resize automático
- [ ] 2.4. Generar múltiples tamaños (thumbnail, medium, full)
- [ ] 2.5. Agregar endpoints para vehículos
- [ ] 2.6. Implementar delete de imágenes

### Fase 3: Frontend - Upload Component (1.5 horas)

- [ ] 3.1. Crear ImageUpload component con drag & drop
- [ ] 3.2. Implementar preview instantáneo
- [ ] 3.3. Agregar validaciones (tipo, tamaño)
- [ ] 3.4. Crear progress bar de upload
- [ ] 3.5. Implementar upload múltiple
- [ ] 3.6. Agregar reordenamiento de imágenes

### Fase 4: Frontend - Image Display (1 hora)

- [ ] 4.1. Crear ProgressiveImage component
- [ ] 4.2. Implementar lazy loading
- [ ] 4.3. Agregar image carousel para detalle
- [ ] 4.4. Crear image gallery grid

---

## 📝 IMPLEMENTACIÓN DETALLADA

### 1️⃣ AWS S3 - Crear Bucket

**Pasos en AWS Console:**

1. Ir a **S3 Console** → https://console.aws.amazon.com/s3/
2. Click **Create bucket**
3. Configuración:
   - **Bucket name:** `cardealer-images-{env}` (ej: `cardealer-images-prod`)
   - **Region:** `us-east-1` (o tu región preferida)
   - **Block Public Access:** ❌ **Desmarcar** "Block all public access"
   - ✅ Confirmar que entiendes los riesgos

4. **CORS Configuration:**
   - Ir a bucket → **Permissions** → **CORS**
   - Agregar:

```json
[
    {
        "AllowedHeaders": ["*"],
        "AllowedMethods": ["GET", "PUT", "POST", "DELETE"],
        "AllowedOrigins": [
            "http://localhost:5174",
            "http://localhost:5173",
            "https://yourdomain.com"
        ],
        "ExposeHeaders": ["ETag"]
    }
]
```

5. **Bucket Policy** (para permitir lecturas públicas):
   - Ir a **Permissions** → **Bucket Policy**
   - Agregar:

```json
{
    "Version": "2012-10-17",
    "Statement": [
        {
            "Sid": "PublicReadGetObject",
            "Effect": "Allow",
            "Principal": "*",
            "Action": "s3:GetObject",
            "Resource": "arn:aws:s3:::cardealer-images-prod/*"
        }
    ]
}
```

6. **Crear IAM User:**
   - Ir a **IAM Console** → **Users** → **Add users**
   - Username: `cardealer-s3-uploader`
   - Access type: ✅ **Programmatic access**
   - Permissions: **Attach existing policies directly** → Crear custom policy:

```json
{
    "Version": "2012-10-17",
    "Statement": [
        {
            "Effect": "Allow",
            "Action": [
                "s3:PutObject",
                "s3:GetObject",
                "s3:DeleteObject",
                "s3:ListBucket"
            ],
            "Resource": [
                "arn:aws:s3:::cardealer-images-prod",
                "arn:aws:s3:::cardealer-images-prod/*"
            ]
        }
    ]
}
```

7. **Guardar credenciales:**
   - Access Key ID
   - Secret Access Key
   - ⚠️ **IMPORTANTE:** Guardar en `compose.secrets.yaml`

---

### 2️⃣ Backend - S3 Storage Service

**Archivo:** `backend/MediaService/MediaService.Infrastructure/Services/S3StorageService.cs`

```csharp
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace MediaService.Infrastructure.Services;

public interface IS3StorageService
{
    Task<string> UploadImageAsync(Stream imageStream, string fileName, string contentType);
    Task<List<string>> UploadVehicleImagesAsync(List<Stream> imageStreams, string vehicleId);
    Task DeleteImageAsync(string fileUrl);
}

public class S3StorageService : IS3StorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly string _region;
    private readonly ILogger<S3StorageService> _logger;

    public S3StorageService(IConfiguration configuration, ILogger<S3StorageService> logger)
    {
        _logger = logger;
        
        var accessKey = configuration["AWS:AccessKey"];
        var secretKey = configuration["AWS:SecretKey"];
        _bucketName = configuration["AWS:BucketName"] ?? "cardealer-images-prod";
        _region = configuration["AWS:Region"] ?? "us-east-1";

        var config = new AmazonS3Config
        {
            RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(_region)
        };

        _s3Client = new AmazonS3Client(accessKey, secretKey, config);
    }

    public async Task<string> UploadImageAsync(Stream imageStream, string fileName, string contentType)
    {
        try
        {
            // Generate unique filename
            var extension = Path.GetExtension(fileName);
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var key = $"vehicles/{uniqueFileName}";

            // Compress and resize image
            using var image = await Image.LoadAsync(imageStream);
            
            // Original size (max 1920x1080)
            if (image.Width > 1920 || image.Height > 1080)
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(1920, 1080),
                    Mode = ResizeMode.Max
                }));
            }

            // Save as JPEG with compression
            using var outputStream = new MemoryStream();
            await image.SaveAsync(outputStream, new JpegEncoder { Quality = 85 });
            outputStream.Position = 0;

            // Upload to S3
            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = outputStream,
                Key = key,
                BucketName = _bucketName,
                ContentType = "image/jpeg",
                CannedACL = S3CannedACL.PublicRead
            };

            var transferUtility = new TransferUtility(_s3Client);
            await transferUtility.UploadAsync(uploadRequest);

            var imageUrl = $"https://{_bucketName}.s3.{_region}.amazonaws.com/{key}";
            
            _logger.LogInformation("Image uploaded successfully: {Url}", imageUrl);

            // Generate thumbnail
            await GenerateThumbnailAsync(image, key.Replace(extension, $"-thumb{extension}"));

            return imageUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload image");
            throw;
        }
    }

    public async Task<List<string>> UploadVehicleImagesAsync(List<Stream> imageStreams, string vehicleId)
    {
        var imageUrls = new List<string>();

        foreach (var stream in imageStreams)
        {
            var fileName = $"vehicle-{vehicleId}-{Guid.NewGuid()}.jpg";
            var url = await UploadImageAsync(stream, fileName, "image/jpeg");
            imageUrls.Add(url);
        }

        return imageUrls;
    }

    public async Task DeleteImageAsync(string fileUrl)
    {
        try
        {
            // Extract key from URL
            var uri = new Uri(fileUrl);
            var key = uri.AbsolutePath.TrimStart('/');

            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = key
            };

            await _s3Client.DeleteObjectAsync(deleteRequest);

            // Delete thumbnail too
            var thumbKey = key.Replace(".jpg", "-thumb.jpg");
            await _s3Client.DeleteObjectAsync(new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = thumbKey
            });

            _logger.LogInformation("Image deleted: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete image from S3");
            throw;
        }
    }

    private async Task GenerateThumbnailAsync(Image image, string key)
    {
        try
        {
            var thumbnail = image.Clone(ctx => ctx.Resize(new ResizeOptions
            {
                Size = new Size(300, 200),
                Mode = ResizeMode.Crop
            }));

            using var thumbStream = new MemoryStream();
            await thumbnail.SaveAsync(thumbStream, new JpegEncoder { Quality = 80 });
            thumbStream.Position = 0;

            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = thumbStream,
                Key = key,
                BucketName = _bucketName,
                ContentType = "image/jpeg",
                CannedACL = S3CannedACL.PublicRead
            };

            var transferUtility = new TransferUtility(_s3Client);
            await transferUtility.UploadAsync(uploadRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate thumbnail");
        }
    }
}
```

**Instalar paquetes NuGet:**

```xml
<PackageReference Include="AWSSDK.S3" Version="3.7.400" />
<PackageReference Include="AWSSDK.Core" Version="3.7.400" />
<PackageReference Include="SixLabors.ImageSharp" Version="3.1.5" />
```

---

### 3️⃣ Backend - Media Controller

**Archivo:** `backend/MediaService/MediaService.Api/Controllers/MediaController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediaService.Infrastructure.Services;

namespace MediaService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class MediaController : ControllerBase
{
    private readonly IS3StorageService _s3Service;
    private readonly ILogger<MediaController> _logger;

    public MediaController(IS3StorageService s3Service, ILogger<MediaController> logger)
    {
        _s3Service = s3Service;
        _logger = logger;
    }

    /// <summary>
    /// Upload a single image
    /// </summary>
    [HttpPost("upload")]
    [Authorize]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10MB max
    [ProducesResponseType(typeof(UploadResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "No file uploaded" });

        // Validate file type
        var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType.ToLower()))
            return BadRequest(new { error = "Invalid file type. Only JPEG, PNG, and WebP are allowed." });

        // Validate file size (10MB max)
        if (file.Length > 10 * 1024 * 1024)
            return BadRequest(new { error = "File size exceeds 10MB limit" });

        try
        {
            using var stream = file.OpenReadStream();
            var imageUrl = await _s3Service.UploadImageAsync(stream, file.FileName, file.ContentType);

            return Ok(new UploadResult
            {
                Url = imageUrl,
                FileName = file.FileName,
                Size = file.Length
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload image");
            return StatusCode(500, new { error = "Failed to upload image" });
        }
    }

    /// <summary>
    /// Upload multiple images for a vehicle
    /// </summary>
    [HttpPost("upload/vehicle/{vehicleId}")]
    [Authorize]
    [RequestSizeLimit(50 * 1024 * 1024)] // 50MB total
    [ProducesResponseType(typeof(List<UploadResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UploadVehicleImages(
        string vehicleId,
        [FromForm] List<IFormFile> files)
    {
        if (files == null || files.Count == 0)
            return BadRequest(new { error = "No files uploaded" });

        if (files.Count > 20)
            return BadRequest(new { error = "Maximum 20 images allowed" });

        var results = new List<UploadResult>();

        foreach (var file in files)
        {
            try
            {
                using var stream = file.OpenReadStream();
                var imageUrl = await _s3Service.UploadImageAsync(stream, file.FileName, file.ContentType);

                results.Add(new UploadResult
                {
                    Url = imageUrl,
                    FileName = file.FileName,
                    Size = file.Length
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload image: {FileName}", file.FileName);
                results.Add(new UploadResult
                {
                    FileName = file.FileName,
                    Error = ex.Message
                });
            }
        }

        return Ok(results);
    }

    /// <summary>
    /// Delete image
    /// </summary>
    [HttpDelete]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteImage([FromQuery] string url)
    {
        if (string.IsNullOrEmpty(url))
            return BadRequest(new { error = "URL is required" });

        try
        {
            await _s3Service.DeleteImageAsync(url);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete image");
            return StatusCode(500, new { error = "Failed to delete image" });
        }
    }
}

public class UploadResult
{
    public string? Url { get; set; }
    public string FileName { get; set; } = string.Empty;
    public long Size { get; set; }
    public string? Error { get; set; }
}
```

---

### 4️⃣ Frontend - Image Upload Component

**Archivo:** `frontend/web/original/src/components/ImageUpload.tsx`

```typescript
import { useState, useCallback, type FC } from 'react';
import { useDropzone } from 'react-dropzone';
import { Upload, X, AlertCircle } from 'lucide-react';
import { uploadService } from '@/services/uploadService';
import toast from 'react-hot-toast';

interface ImageUploadProps {
  onUploadComplete: (urls: string[]) => void;
  maxFiles?: number;
  vehicleId?: string;
}

export const ImageUpload: FC<ImageUploadProps> = ({
  onUploadComplete,
  maxFiles = 20,
  vehicleId
}) => {
  const [uploading, setUploading] = useState(false);
  const [progress, setProgress] = useState(0);
  const [previews, setPreviews] = useState<string[]>([]);

  const onDrop = useCallback(async (acceptedFiles: File[]) => {
    if (acceptedFiles.length === 0) return;

    // Validate
    if (acceptedFiles.length > maxFiles) {
      toast.error(`Máximo ${maxFiles} imágenes permitidas`);
      return;
    }

    // Generate previews
    const previewUrls = acceptedFiles.map(file => URL.createObjectURL(file));
    setPreviews(previewUrls);

    // Upload
    setUploading(true);
    setProgress(0);

    try {
      const uploadPromises = acceptedFiles.map((file, index) =>
        uploadService.uploadImage(file, {
          onProgress: (p) => {
            const totalProgress = ((index + (p / 100)) / acceptedFiles.length) * 100;
            setProgress(Math.round(totalProgress));
          }
        })
      );

      const results = await Promise.all(uploadPromises);
      const urls = results.map(r => r.url);

      onUploadComplete(urls);
      toast.success(`${urls.length} imágenes subidas exitosamente`);
      
      // Cleanup previews
      previewUrls.forEach(url => URL.revokeObjectURL(url));
      setPreviews([]);
    } catch (error: any) {
      toast.error(error.message || 'Error al subir imágenes');
    } finally {
      setUploading(false);
      setProgress(0);
    }
  }, [maxFiles, onUploadComplete]);

  const { getRootProps, getInputProps, isDragActive, fileRejections } = useDropzone({
    onDrop,
    accept: {
      'image/jpeg': ['.jpg', '.jpeg'],
      'image/png': ['.png'],
      'image/webp': ['.webp']
    },
    maxSize: 10 * 1024 * 1024, // 10MB
    maxFiles,
    disabled: uploading
  });

  return (
    <div className="space-y-4">
      {/* Dropzone */}
      <div
        {...getRootProps()}
        className={`
          border-2 border-dashed rounded-lg p-8
          transition-colors cursor-pointer
          ${isDragActive ? 'border-blue-500 bg-blue-50' : 'border-gray-300'}
          ${uploading ? 'opacity-50 cursor-not-allowed' : 'hover:border-gray-400'}
        `}
      >
        <input {...getInputProps()} />
        
        <div className="flex flex-col items-center justify-center text-center">
          <Upload className="w-12 h-12 text-gray-400 mb-4" />
          
          {isDragActive ? (
            <p className="text-blue-600 font-medium">
              Suelta las imágenes aquí...
            </p>
          ) : (
            <>
              <p className="text-gray-700 font-medium mb-1">
                Arrastra imágenes aquí o haz click para seleccionar
              </p>
              <p className="text-sm text-gray-500">
                Máximo {maxFiles} imágenes • JPG, PNG, WebP • Hasta 10MB cada una
              </p>
            </>
          )}
        </div>
      </div>

      {/* Errors */}
      {fileRejections.length > 0 && (
        <div className="bg-red-50 border border-red-200 rounded-lg p-4">
          <div className="flex items-start gap-2">
            <AlertCircle className="w-5 h-5 text-red-600 mt-0.5" />
            <div className="flex-1">
              <p className="text-sm font-medium text-red-800 mb-2">
                Algunos archivos fueron rechazados:
              </p>
              <ul className="text-sm text-red-700 space-y-1">
                {fileRejections.map(({ file, errors }) => (
                  <li key={file.name}>
                    <strong>{file.name}</strong>: {errors[0].message}
                  </li>
                ))}
              </ul>
            </div>
          </div>
        </div>
      )}

      {/* Upload Progress */}
      {uploading && (
        <div className="space-y-2">
          <div className="flex justify-between text-sm text-gray-600">
            <span>Subiendo imágenes...</span>
            <span>{progress}%</span>
          </div>
          <div className="h-2 bg-gray-200 rounded-full overflow-hidden">
            <div
              className="h-full bg-blue-600 transition-all duration-300"
              style={{ width: `${progress}%` }}
            />
          </div>
        </div>
      )}

      {/* Previews */}
      {previews.length > 0 && (
        <div className="grid grid-cols-4 gap-4">
          {previews.map((preview, index) => (
            <div key={index} className="relative aspect-video rounded-lg overflow-hidden">
              <img
                src={preview}
                alt={`Preview ${index + 1}`}
                className="w-full h-full object-cover"
              />
              <div className="absolute inset-0 bg-blue-600 bg-opacity-20 flex items-center justify-center">
                <div className="animate-spin rounded-full h-8 w-8 border-4 border-white border-t-transparent" />
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};
```

**Instalar dependencia:**

```bash
npm install react-dropzone
```

---

### 5️⃣ Frontend - Upload Service

**Archivo:** `frontend/web/original/src/services/uploadService.ts`

```typescript
import { api } from './api';

export interface UploadOptions {
  onProgress?: (progress: number) => void;
}

export interface UploadResult {
  url: string;
  fileName: string;
  size: number;
  error?: string;
}

export const uploadService = {
  /**
   * Upload single image
   */
  async uploadImage(file: File, options?: UploadOptions): Promise<UploadResult> {
    const formData = new FormData();
    formData.append('file', file);

    const response = await api.post<UploadResult>('/media/upload', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
      onUploadProgress: (progressEvent) => {
        if (options?.onProgress && progressEvent.total) {
          const percentCompleted = Math.round(
            (progressEvent.loaded * 100) / progressEvent.total
          );
          options.onProgress(percentCompleted);
        }
      },
    });

    return response.data;
  },

  /**
   * Upload multiple images for a vehicle
   */
  async uploadVehicleImages(
    vehicleId: string,
    files: File[],
    options?: UploadOptions
  ): Promise<UploadResult[]> {
    const formData = new FormData();
    files.forEach((file) => {
      formData.append('files', file);
    });

    const response = await api.post<UploadResult[]>(
      `/media/upload/vehicle/${vehicleId}`,
      formData,
      {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
        onUploadProgress: (progressEvent) => {
          if (options?.onProgress && progressEvent.total) {
            const percentCompleted = Math.round(
              (progressEvent.loaded * 100) / progressEvent.total
            );
            options.onProgress(percentCompleted);
          }
        },
      }
    );

    return response.data;
  },

  /**
   * Delete image
   */
  async deleteImage(url: string): Promise<void> {
    await api.delete('/media', { params: { url } });
  },

  /**
   * Compress image client-side before upload
   */
  async compressImage(file: File, maxWidth: number = 1920, quality: number = 0.8): Promise<File> {
    return new Promise((resolve) => {
      const reader = new FileReader();
      reader.onload = (e) => {
        const img = new Image();
        img.onload = () => {
          const canvas = document.createElement('canvas');
          let { width, height } = img;

          if (width > maxWidth) {
            height = (height * maxWidth) / width;
            width = maxWidth;
          }

          canvas.width = width;
          canvas.height = height;

          const ctx = canvas.getContext('2d')!;
          ctx.drawImage(img, 0, 0, width, height);

          canvas.toBlob(
            (blob) => {
              if (blob) {
                const compressedFile = new File([blob], file.name, {
                  type: 'image/jpeg',
                  lastModified: Date.now(),
                });
                resolve(compressedFile);
              } else {
                resolve(file);
              }
            },
            'image/jpeg',
            quality
          );
        };
        img.src = e.target?.result as string;
      };
      reader.readAsDataURL(file);
    });
  },
};
```

---

### 6️⃣ Frontend - Progressive Image Component

**Archivo:** `frontend/web/original/src/components/ProgressiveImage.tsx`

```typescript
import { useState, useEffect, type FC } from 'react';

interface ProgressiveImageProps {
  src: string;
  alt: string;
  className?: string;
  placeholderSrc?: string;
}

export const ProgressiveImage: FC<ProgressiveImageProps> = ({
  src,
  alt,
  className = '',
  placeholderSrc
}) => {
  const [currentSrc, setCurrentSrc] = useState(placeholderSrc || getPlaceholder());
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const img = new Image();
    img.src = src;
    
    img.onload = () => {
      setCurrentSrc(src);
      setLoading(false);
    };

    img.onerror = () => {
      setLoading(false);
    };
  }, [src]);

  return (
    <div className={`relative ${className}`}>
      <img
        src={currentSrc}
        alt={alt}
        className={`w-full h-full object-cover transition-all duration-300 ${
          loading ? 'blur-sm scale-105' : 'blur-0 scale-100'
        }`}
      />
      {loading && (
        <div className="absolute inset-0 flex items-center justify-center bg-gray-100">
          <div className="animate-spin rounded-full h-8 w-8 border-4 border-blue-600 border-t-transparent" />
        </div>
      )}
    </div>
  );
};

function getPlaceholder(): string {
  return 'data:image/svg+xml,%3Csvg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 400 300"%3E%3Crect fill="%23f3f4f6" width="400" height="300"/%3E%3C/svg%3E';
}
```

---

## ✅ CRITERIOS DE ACEPTACIÓN

### Test Backend

```bash
# Upload single image
curl -X POST http://localhost:15007/api/media/upload \
  -H "Authorization: Bearer $TOKEN" \
  -F "file=@image.jpg"

# Expected response:
{
  "url": "https://cardealer-images-prod.s3.us-east-1.amazonaws.com/vehicles/xxx.jpg",
  "fileName": "image.jpg",
  "size": 1234567
}
```

### Test Frontend

1. Login como dealer
2. Ir a "Crear Vehículo"
3. Arrastrar 5 imágenes al área de upload
4. Ver previews instantáneos
5. Ver barra de progreso
6. Ver URLs generadas en consola
7. Crear vehículo con imágenes
8. Visitar detalle → Ver imágenes cargadas correctamente

---

## 📊 ESTIMACIÓN DE TOKENS

| Tarea | Tokens |
|-------|--------|
| AWS S3 configuration guide | 3,000 |
| S3StorageService implementation | 5,000 |
| MediaController | 3,500 |
| Frontend ImageUpload component | 4,500 |
| UploadService | 3,000 |
| ProgressiveImage component | 2,000 |
| Docker configuration | 1,500 |
| Testing | 1,500 |
| **TOTAL** | **~24,000** |

**Con buffer 15%:** ~27,500 tokens

---

## ➡️ PRÓXIMO SPRINT

**Sprint 5:** [SPRINT_5_BILLING_PAYMENTS.md](SPRINT_5_BILLING_PAYMENTS.md)

Integración de Stripe para pagos, suscripciones y webhooks.

---

**Estado:** ⚪ Pendiente  
**Última actualización:** 2 Enero 2026
