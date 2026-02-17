# FileStorageService - Context Documentation

## 📋 INFORMACIÓN GENERAL

- **Nombre del Servicio:** FileStorageService
- **Puerto en Desarrollo:** 5035
- **Estado:** 🚧 **EN DESARROLLO - NO DESPLEGADO**
- **Base de Datos:** PostgreSQL (`filestorageservice`)
- **Almacenamiento:** AWS S3 / Digital Ocean Spaces
- **Imagen Docker:** Local only

### Propósito
Servicio especializado de gestión de archivos y almacenamiento en la nube. Maneja uploads, procesamiento, organización, versionado y optimización de archivos. Complementa al MediaService con funcionalidades avanzadas.

---

## 🏗️ ARQUITECTURA

```
FileStorageService/
├── FileStorageService.Api/
│   ├── Controllers/
│   │   ├── FilesController.cs
│   │   ├── FoldersController.cs
│   │   └── VersionsController.cs
│   └── Program.cs
├── FileStorageService.Application/
│   └── Services/
│       ├── S3Service.cs
│       ├── FileProcessorService.cs
│       └── VirusScanner.cs
├── FileStorageService.Domain/
│   ├── Entities/
│   │   ├── StoredFile.cs
│   │   ├── Folder.cs
│   │   └── FileVersion.cs
│   └── Enums/
│       ├── FileType.cs
│       └── ScanStatus.cs
└── FileStorageService.Infrastructure/
```

---

## 📦 ENTIDADES PRINCIPALES

### StoredFile
```csharp
public class StoredFile
{
    public Guid Id { get; set; }
    public string FileName { get; set; }
    public string OriginalFileName { get; set; }
    
    // Ubicación
    public Guid? FolderId { get; set; }
    public Folder? Folder { get; set; }
    public string S3Key { get; set; }              // Path en S3/Spaces
    public string BucketName { get; set; }
    
    // Metadata
    public FileType Type { get; set; }             // Document, Image, Video, Archive
    public string MimeType { get; set; }
    public long SizeInBytes { get; set; }
    public string? Extension { get; set; }
    
    // Imagen (si aplica)
    public int? Width { get; set; }
    public int? Height { get; set; }
    public string? ThumbnailUrl { get; set; }
    
    // Seguridad
    public ScanStatus VirusScanStatus { get; set; } // Pending, Clean, Infected, Error
    public DateTime? ScannedAt { get; set; }
    public string? Hash { get; set; }              // SHA256 para detectar duplicados
    
    // Propietario
    public Guid UploadedByUserId { get; set; }
    public string UploadedByUserName { get; set; }
    
    // Acceso
    public bool IsPublic { get; set; }
    public string? PresignedUrl { get; set; }
    public DateTime? PresignedUrlExpiresAt { get; set; }
    
    // Versionado
    public int Version { get; set; } = 1;
    public bool IsLatestVersion { get; set; } = true;
    
    // Lifecycle
    public DateTime UploadedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public bool IsDeleted { get; set; }
    
    // Navegación
    public ICollection<FileVersion> Versions { get; set; }
}
```

### Folder
```csharp
public class Folder
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Path { get; set; }               // "/documents/contracts/2026"
    
    // Jerarquía
    public Guid? ParentFolderId { get; set; }
    public Folder? ParentFolder { get; set; }
    public ICollection<Folder> SubFolders { get; set; }
    
    // Permisos
    public Guid OwnerId { get; set; }
    public bool IsPublic { get; set; }
    public List<Guid>? SharedWithUserIds { get; set; }
    
    // Metadata
    public DateTime CreatedAt { get; set; }
    public DateTime LastModifiedAt { get; set; }
    
    // Navegación
    public ICollection<StoredFile> Files { get; set; }
}
```

### FileVersion
```csharp
public class FileVersion
{
    public Guid Id { get; set; }
    public Guid FileId { get; set; }
    public int VersionNumber { get; set; }
    
    // Ubicación en S3
    public string S3Key { get; set; }
    public long SizeInBytes { get; set; }
    
    // Cambios
    public string? ChangeDescription { get; set; }
    public Guid UploadedByUserId { get; set; }
    public DateTime UploadedAt { get; set; }
    
    // Navegación
    public StoredFile File { get; set; }
}
```

---

## 📡 ENDPOINTS (Propuestos)

### Files
- `POST /api/files/upload` - Upload archivo
  ```
  Multipart form-data:
  - file: <binary>
  - folderId: <uuid> (opcional)
  - isPublic: <bool>
  ```
- `GET /api/files/{id}` - Metadata del archivo
- `GET /api/files/{id}/download` - Descargar archivo
- `DELETE /api/files/{id}` - Eliminar (soft delete)
- `POST /api/files/{id}/restore` - Restaurar archivo eliminado
- `GET /api/files` - Listar archivos (con filtros)

### Folders
- `POST /api/folders` - Crear folder
- `GET /api/folders` - Listar folders
- `GET /api/folders/{id}` - Contenido del folder
- `PUT /api/folders/{id}/move` - Mover folder
- `DELETE /api/folders/{id}` - Eliminar folder

### Versions
- `GET /api/files/{id}/versions` - Historial de versiones
- `POST /api/files/{id}/new-version` - Upload nueva versión
- `POST /api/files/{id}/revert/{versionNumber}` - Revertir a versión anterior

### Bulk Operations
- `POST /api/files/bulk-upload` - Upload múltiples archivos
- `POST /api/files/bulk-delete` - Eliminar múltiples
- `POST /api/files/bulk-move` - Mover múltiples a folder

---

## 💡 FUNCIONALIDADES PLANEADAS

### Virus Scanning
Integración con ClamAV:
```csharp
public async Task<ScanResult> ScanFileAsync(Stream fileStream)
{
    using var client = new ClamClient("clamav-host", 3310);
    var result = await client.SendAndScanFileAsync(fileStream);
    
    return new ScanResult
    {
        IsClean = result.Result == ClamScanResults.Clean,
        VirusName = result.VirusName
    };
}
```

### Duplicate Detection
Detectar archivos duplicados por hash:
```csharp
public async Task<string> CalculateSHA256(Stream stream)
{
    using var sha256 = SHA256.Create();
    var hash = await sha256.ComputeHashAsync(stream);
    return Convert.ToBase64String(hash);
}
```
Si hash ya existe → opción de referenciar archivo existente

### Image Processing
- Resize automático
- Generar thumbnails (150x150, 300x300)
- Convert a WebP para optimización
- EXIF data extraction

### Video Processing
- Generate thumbnail de video
- Convert a streaming-friendly format (HLS)
- Extract metadata (duration, resolution)

### Document Processing
- PDF → Text extraction (OCR si necesario)
- Office docs → Preview/thumbnail
- Virus scanning obligatorio

### Presigned URLs
URLs temporales para acceso sin autenticación:
```csharp
public string GeneratePresignedUrl(string s3Key, int expirationMinutes = 60)
{
    var request = new GetPreSignedUrlRequest
    {
        BucketName = _bucketName,
        Key = s3Key,
        Expires = DateTime.UtcNow.AddMinutes(expirationMinutes)
    };
    
    return _s3Client.GetPreSignedURL(request);
}
```

### Storage Tiers
- **Hot Storage:** Archivos accedidos frecuentemente (S3 Standard)
- **Cool Storage:** Archivos antiguos (S3 Infrequent Access)
- **Archive:** Archivos raramente accedidos (S3 Glacier)

Auto-migration basada en última fecha de acceso.

### Quota Management
Limitar storage por usuario/organización:
```csharp
public class StorageQuota
{
    public Guid UserId { get; set; }
    public long QuotaBytes { get; set; }           // 5GB default
    public long UsedBytes { get; set; }
    public long RemainingBytes => QuotaBytes - UsedBytes;
    public decimal UsagePercent => (decimal)UsedBytes / QuotaBytes * 100;
}
```

### CDN Integration
- CloudFront para servir archivos estáticos
- Cache headers apropiados
- Geolocation-based delivery

---

## 🔗 INTEGRACIÓN CON OTROS SERVICIOS

### MediaService
- Complementar con funcionalidades avanzadas
- Versionado de imágenes de vehículos

### VehiclesSaleService
- Guardar documentos del vehículo (carfax, factura, etc.)

### InvoicingService
- Almacenar PDFs de facturas

### UserService
- Avatar y documentos del usuario (ID, licencia)

### ContractService (futuro)
- Guardar contratos firmados

---

## 🔐 SEGURIDAD

### Validaciones
```csharp
public class FileValidator
{
    private readonly string[] _allowedExtensions = 
        { ".jpg", ".png", ".pdf", ".docx", ".xlsx" };
    private const long MaxFileSizeMB = 50;
    
    public bool ValidateFile(IFormFile file)
    {
        // Extension
        var ext = Path.GetExtension(file.FileName).ToLower();
        if (!_allowedExtensions.Contains(ext))
            return false;
        
        // Size
        if (file.Length > MaxFileSizeMB * 1024 * 1024)
            return false;
        
        // MIME type
        if (!IsValidMimeType(file.ContentType))
            return false;
        
        return true;
    }
}
```

### Encryption at Rest
S3 Server-Side Encryption (SSE-S3 o SSE-KMS).

### Access Control
- Public files: anyone con URL
- Private files: requieren autenticación
- Presigned URLs: acceso temporal

---

## 🔄 EVENTOS PUBLICADOS (RabbitMQ)

### FileUploaded
```json
{
  "fileId": "uuid",
  "fileName": "document.pdf",
  "userId": "uuid",
  "sizeBytes": 1024000,
  "timestamp": "2026-01-07T10:30:00Z"
}
```

### FileDeleted
```json
{
  "fileId": "uuid",
  "deletedBy": "uuid",
  "timestamp": "2026-01-07T10:30:00Z"
}
```

---

**Estado:** 🚧 EN DESARROLLO - No desplegado en producción  
**Versión:** 0.1.0  
**Storage Provider:** AWS S3 / Digital Ocean Spaces
