# MediaService - Context Documentation

## 📋 INFORMACIÓN GENERAL

- **Nombre del Servicio:** MediaService
- **Puerto en Kubernetes:** 8080
- **Puerto en Desarrollo:** 5005
- **Estado:** ✅ **EN PRODUCCIÓN**
- **Base de Datos:** PostgreSQL (`mediaservice`)
- **Almacenamiento:** AWS S3 / Digital Ocean Spaces
- **Imagen Docker:** ghcr.io/gregorymorenoiem/cardealer-mediaservice:latest

### Propósito
Servicio de gestión de archivos y medios (imágenes, videos, documentos). Maneja upload a S3, procesamiento de imágenes (resize, thumbnails), almacenamiento de metadata y generación de URLs firmadas.

---

## 🏗️ ARQUITECTURA

```
MediaService/
├── MediaService.Api/
│   ├── Controllers/
│   │   ├── MediaController.cs          # Upload/Download
│   │   └── ImagesController.cs         # Operaciones de imágenes
│   └── Program.cs
├── MediaService.Application/
│   ├── Features/
│   │   ├── Commands/
│   │   │   ├── UploadFileCommand.cs
│   │   │   ├── DeleteFileCommand.cs
│   │   │   └── GeneratePresignedUrlCommand.cs
│   │   └── Queries/
│   │       ├── GetFileByIdQuery.cs
│   │       └── GetFilesByEntityQuery.cs
│   └── DTOs/
│       ├── FileUploadDto.cs
│       └── MediaFileDto.cs
├── MediaService.Domain/
│   ├── Entities/
│   │   ├── MediaFile.cs
│   │   └── ImageMetadata.cs
│   ├── Enums/
│   │   ├── FileType.cs                 # Image, Video, Document
│   │   └── StorageProvider.cs          # S3, Spaces
│   └── Interfaces/
│       ├── IMediaRepository.cs
│       └── IStorageService.cs
├── MediaService.Infrastructure/
│   ├── Services/
│   │   ├── S3StorageService.cs
│   │   ├── ImageProcessingService.cs
│   │   └── ThumbnailGenerator.cs
│   └── Persistence/
└── MediaService.Workers/
    └── ImageOptimizationWorker.cs      # Background processing
```

---

## 📦 ENTIDADES

### MediaFile
```csharp
public class MediaFile
{
    public Guid Id { get; set; }
    
    // Archivo
    public string FileName { get; set; }
    public string OriginalFileName { get; set; }
    public string ContentType { get; set; }         // image/jpeg, application/pdf
    public long FileSizeBytes { get; set; }
    public FileType FileType { get; set; }          // Image, Video, Document
    
    // Almacenamiento
    public StorageProvider StorageProvider { get; set; }
    public string BucketName { get; set; }
    public string StorageKey { get; set; }          // Path en S3
    public string PublicUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    
    // Entidad relacionada
    public string EntityType { get; set; }          // "Vehicle", "User", "Dealer"
    public Guid EntityId { get; set; }
    public string? EntityField { get; set; }        // "avatar", "primary_image"
    
    // Metadata
    public Guid UploadedBy { get; set; }
    public DateTime UploadedAt { get; set; }
    public bool IsPublic { get; set; }
    public bool IsDeleted { get; set; }
    
    // Metadata de imagen (si FileType == Image)
    public int? Width { get; set; }
    public int? Height { get; set; }
    public string? Format { get; set; }             // JPEG, PNG, WEBP
    public bool? HasThumbnail { get; set; }
}
```

### FileType Enum
```csharp
public enum FileType
{
    Image = 0,
    Video = 1,
    Document = 2,
    Audio = 3,
    Other = 99
}
```

---

## 📡 ENDPOINTS API

### Upload de Archivos

#### POST `/api/media/upload`
Subir archivo (multipart/form-data).

**Request (Form Data):**
- `file`: Archivo (IFormFile)
- `entityType`: Tipo de entidad ("Vehicle", "User")
- `entityId`: ID de la entidad
- `entityField`: Campo (opcional, ej: "avatar")
- `isPublic`: Si el archivo es público (default: true)

**Response (201 Created):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "fileName": "abc123.jpg",
  "originalFileName": "mi-carro.jpg",
  "contentType": "image/jpeg",
  "fileSizeBytes": 1024000,
  "publicUrl": "https://okla-media.s3.amazonaws.com/vehicles/abc123.jpg",
  "thumbnailUrl": "https://okla-media.s3.amazonaws.com/vehicles/abc123_thumb.jpg",
  "width": 1920,
  "height": 1080,
  "uploadedAt": "2026-01-07T10:30:00Z"
}
```

#### POST `/api/media/upload-multiple`
Subir múltiples archivos.

**Request:** Array de archivos en form-data

**Response (201 Created):**
```json
{
  "uploadedFiles": [
    { "id": "...", "publicUrl": "..." },
    { "id": "...", "publicUrl": "..." }
  ],
  "totalCount": 5,
  "successCount": 5,
  "failedCount": 0
}
```

### Gestión de Archivos

#### GET `/api/media/{id}`
Obtener metadata de un archivo.

**Response (200 OK):**
```json
{
  "id": "...",
  "fileName": "abc123.jpg",
  "contentType": "image/jpeg",
  "publicUrl": "...",
  "entityType": "Vehicle",
  "entityId": "...",
  "uploadedAt": "..."
}
```

#### GET `/api/media/entity/{entityType}/{entityId}`
Obtener todos los archivos de una entidad.

**Query Parameters:**
- `fileType`: Filtrar por tipo (Image, Document, etc.)

**Response (200 OK):**
```json
{
  "entityType": "Vehicle",
  "entityId": "...",
  "files": [
    { "id": "...", "publicUrl": "...", "fileType": "Image" }
  ],
  "totalCount": 8
}
```

#### DELETE `/api/media/{id}`
Eliminar archivo (soft delete + eliminar de S3).

**Response (204 No Content)**

### Operaciones de Imágenes

#### POST `/api/images/{id}/thumbnail`
Generar thumbnail de una imagen.

**Request:**
```json
{
  "width": 300,
  "height": 200,
  "format": "WEBP"
}
```

#### POST `/api/images/{id}/resize`
Redimensionar imagen.

**Request:**
```json
{
  "width": 1920,
  "height": 1080,
  "maintainAspectRatio": true
}
```

#### GET `/api/media/{id}/presigned-url`
Generar URL firmada (para acceso temporal a archivos privados).

**Query Parameters:**
- `expiresInMinutes`: Expiración (default: 60)

**Response (200 OK):**
```json
{
  "presignedUrl": "https://s3.amazonaws.com/...?signature=...",
  "expiresAt": "2026-01-07T11:30:00Z"
}
```

---

## 🔧 TECNOLOGÍAS Y DEPENDENCIAS

```xml
<PackageReference Include="AWSSDK.S3" Version="3.7.305" />
<PackageReference Include="SixLabors.ImageSharp" Version="3.1.0" />
<PackageReference Include="MediatR" Version="12.2.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />
<PackageReference Include="RabbitMQ.Client" Version="6.8.1" />
```

### Servicios Externos
- **AWS S3 / Digital Ocean Spaces**: Almacenamiento de archivos
- **ImageSharp**: Procesamiento de imágenes
- **PostgreSQL**: Metadata
- **RabbitMQ**: Eventos de upload/delete

---

## ⚙️ CONFIGURACIÓN

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=${DB_HOST};Database=mediaservice;..."
  },
  "AWS": {
    "Region": "us-east-1",
    "AccessKeyId": "${AWS_ACCESS_KEY_ID}",
    "SecretAccessKey": "${AWS_SECRET_ACCESS_KEY}",
    "S3": {
      "BucketName": "okla-media",
      "PublicBucketUrl": "https://okla-media.s3.amazonaws.com"
    }
  },
  "ImageProcessing": {
    "MaxFileSizeMB": 10,
    "AllowedFormats": ["JPEG", "PNG", "WEBP", "GIF"],
    "ThumbnailWidth": 300,
    "ThumbnailHeight": 200,
    "MaxWidth": 2048,
    "MaxHeight": 2048,
    "Quality": 85
  },
  "RabbitMQ": {
    "Host": "rabbitmq",
    "Port": 5672
  }
}
```

---

## 🔄 EVENTOS PUBLICADOS

### FileUploadedEvent
```csharp
public record FileUploadedEvent(
    Guid FileId,
    string FileName,
    FileType FileType,
    string EntityType,
    Guid EntityId,
    string PublicUrl,
    DateTime UploadedAt
);
```

**Exchange:** `media.events`  
**Routing Key:** `file.uploaded`  
**Consumidores:**
- **VehiclesSaleService**: Actualizar URLs de imágenes
- **UserService**: Actualizar avatar
- **AuditService**: Registro de uploads

### FileDeletedEvent
Cuando se elimina un archivo.

---

## 📝 REGLAS DE NEGOCIO

### Upload de Imágenes
1. **Tamaño máximo**: 10 MB por imagen
2. **Formatos permitidos**: JPEG, PNG, WEBP, GIF
3. **Auto-optimización**: Se redimensiona a máximo 2048x2048
4. **Thumbnails automáticos**: 300x200 para imágenes
5. **Nombres únicos**: GUID + extensión original

### Eliminación
1. **Soft delete**: Se marca como `IsDeleted = true`
2. **Eliminación física de S3**: Proceso asíncrono
3. **Cascada**: Si se elimina entidad, se eliminan sus archivos

### URLs Públicas vs Privadas
- **Públicas**: Acceso directo desde S3 (vehículos, perfiles)
- **Privadas**: Requieren presigned URL (documentos, facturas)

---

## 🔗 RELACIONES CON OTROS SERVICIOS

### Consultado Por:
- **VehiclesSaleService**: Imágenes de vehículos
- **UserService**: Avatares
- **DealerService**: Logos y banners

### Publica Eventos A:
- **VehiclesSaleService**: Sincronización de imágenes
- **AuditService**: Registro de uploads

---

## 🚀 DESPLIEGUE

### Kubernetes
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: mediaservice
  namespace: okla
spec:
  replicas: 2
  template:
    spec:
      containers:
      - name: mediaservice
        image: ghcr.io/gregorymorenoiem/cardealer-mediaservice:latest
        ports:
        - containerPort: 8080
        env:
        - name: AWS__AccessKeyId
          valueFrom:
            secretKeyRef:
              name: aws-credentials
              key: access-key-id
        - name: AWS__SecretAccessKey
          valueFrom:
            secretKeyRef:
              name: aws-credentials
              key: secret-access-key
```

---

## 📅 ÚLTIMA ACTUALIZACIÓN

**Fecha:** Enero 7, 2026  
**Versión:** 1.0.0  
**Estado:** Producción en DOKS
