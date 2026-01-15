# ☁️ API Amazon S3 (DigitalOcean Spaces)

**Proveedor:** DigitalOcean Spaces (compatible con S3)  
**Documentación oficial:** https://docs.digitalocean.com/products/spaces/  
**Versión:** AWS Signature V4  
**Actualizado:** Enero 2026

---

## 📋 Tabla de Contenidos

1. [Autenticación](#autenticación)
2. [Endpoints principales](#endpoints-principales)
3. [Operaciones básicas](#operaciones-básicas)
4. [Políticas de acceso](#políticas-de-acceso)
5. [CORS Configuration](#cors-configuration)
6. [CDN Integration](#cdn-integration)
7. [Manejo de errores](#manejo-de-errores)
8. [Ejemplos de código](#ejemplos-de-código)

---

## 🔑 Autenticación

### Credenciales Requeridas

Obten tus credenciales en:

- **Control Panel:** https://cloud.digitalocean.com/spaces
- **Region:** Seleccionar región más cercana (nyc3, ams3, sgp1, etc.)

### Parámetros de Autenticación

```
Access Key ID      (String, 20 chars)  - Identificador público
Secret Access Key  (String, 40 chars)  - Clave privada
Region             (String)             - Región del bucket (ej: nyc3)
Endpoint           (URL)                - https://{region}.digitaloceanspaces.com
Bucket Name        (String)             - Nombre del bucket creado
```

### AWS Signature Version 4

S3 utiliza AWS Signature V4 para autenticación. La firma se calcula:

1. Crear canonical request
2. Crear string to sign
3. Calcular signing key
4. Calcular signature

**C# con AWS SDK:**

```csharp
using Amazon.S3;
using Amazon.S3.Model;

var config = new AmazonS3Config
{
    ServiceURL = "https://nyc3.digitaloceanspaces.com",
    ForcePathStyle = true
};

var client = new AmazonS3Client(
    accessKeyId: "YOUR_ACCESS_KEY",
    secretAccessKey: "YOUR_SECRET_KEY",
    config
);
```

---

## 🌐 Endpoints Principales

### URL Base

```
https://{bucket-name}.{region}.digitaloceanspaces.com
```

**Ejemplo:**

```
https://okla-media.nyc3.digitaloceanspaces.com
```

### URL del CDN (opcional)

```
https://{bucket-name}.{region}.cdn.digitaloceanspaces.com
```

**Ejemplo:**

```
https://okla-media.nyc3.cdn.digitaloceanspaces.com
```

### Health Check

```bash
curl -I https://okla-media.nyc3.digitaloceanspaces.com
# Retorna 403 si bucket privado (OK)
# Retorna 200 si bucket público
```

---

## 📦 Operaciones Básicas

### 1. Upload de Archivo (PutObject)

**Endpoint:** `PUT /{bucket}/{key}`

**Ejemplo con AWS SDK:**

```csharp
public async Task<string> UploadFileAsync(Stream fileStream, string fileName)
{
    var putRequest = new PutObjectRequest
    {
        BucketName = "okla-media",
        Key = $"vehicles/{Guid.NewGuid()}/{fileName}",
        InputStream = fileStream,
        ContentType = GetContentType(fileName),
        CannedACL = S3CannedACL.PublicRead // o Private
    };

    var response = await _s3Client.PutObjectAsync(putRequest);

    var fileUrl = $"https://okla-media.nyc3.digitaloceanspaces.com/{putRequest.Key}";
    return fileUrl;
}
```

**Headers requeridos:**

```
Content-Type: image/jpeg (o tipo apropiado)
x-amz-acl: public-read (opcional, para archivos públicos)
x-amz-storage-class: STANDARD (default)
```

---

### 2. Download de Archivo (GetObject)

**Endpoint:** `GET /{bucket}/{key}`

**Ejemplo con AWS SDK:**

```csharp
public async Task<Stream> DownloadFileAsync(string fileKey)
{
    var getRequest = new GetObjectRequest
    {
        BucketName = "okla-media",
        Key = fileKey
    };

    var response = await _s3Client.GetObjectAsync(getRequest);
    return response.ResponseStream;
}
```

**URL pública:**

```
https://okla-media.nyc3.digitaloceanspaces.com/vehicles/123/photo.jpg
```

---

### 3. Eliminar Archivo (DeleteObject)

**Endpoint:** `DELETE /{bucket}/{key}`

**Ejemplo con AWS SDK:**

```csharp
public async Task<bool> DeleteFileAsync(string fileKey)
{
    var deleteRequest = new DeleteObjectRequest
    {
        BucketName = "okla-media",
        Key = fileKey
    };

    var response = await _s3Client.DeleteObjectAsync(deleteRequest);
    return response.HttpStatusCode == System.Net.HttpStatusCode.NoContent;
}
```

---

### 4. Listar Archivos (ListObjects)

**Endpoint:** `GET /{bucket}?list-type=2`

**Ejemplo con AWS SDK:**

```csharp
public async Task<List<string>> ListFilesAsync(string prefix = "")
{
    var listRequest = new ListObjectsV2Request
    {
        BucketName = "okla-media",
        Prefix = prefix,
        MaxKeys = 1000
    };

    var response = await _s3Client.ListObjectsV2Async(listRequest);

    return response.S3Objects.Select(o => o.Key).ToList();
}
```

---

### 5. Generar URL Pre-firmada (Presigned URL)

Para acceso temporal a archivos privados:

```csharp
public string GeneratePresignedUrl(string fileKey, int expirationMinutes = 60)
{
    var request = new GetPreSignedUrlRequest
    {
        BucketName = "okla-media",
        Key = fileKey,
        Expires = DateTime.UtcNow.AddMinutes(expirationMinutes)
    };

    return _s3Client.GetPreSignedURL(request);
}
```

**URL generada:**

```
https://okla-media.nyc3.digitaloceanspaces.com/vehicles/123/photo.jpg?
  AWSAccessKeyId=XXX&
  Expires=1736960400&
  Signature=YYY
```

---

### 6. Copy Object (duplicar archivo)

**Endpoint:** `PUT /{bucket}/{new-key}`

```csharp
public async Task<bool> CopyFileAsync(string sourceKey, string destinationKey)
{
    var copyRequest = new CopyObjectRequest
    {
        SourceBucket = "okla-media",
        SourceKey = sourceKey,
        DestinationBucket = "okla-media",
        DestinationKey = destinationKey
    };

    var response = await _s3Client.CopyObjectAsync(copyRequest);
    return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
}
```

---

## 🔐 Políticas de Acceso (Bucket Policy)

### Bucket Público para Lectura

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Sid": "PublicReadGetObject",
      "Effect": "Allow",
      "Principal": "*",
      "Action": "s3:GetObject",
      "Resource": "arn:aws:s3:::okla-media/*"
    }
  ]
}
```

### Bucket Privado (solo acceso con credenciales)

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Sid": "DenyPublicAccess",
      "Effect": "Deny",
      "Principal": "*",
      "Action": "s3:*",
      "Resource": "arn:aws:s3:::okla-media/*"
    }
  ]
}
```

---

## 🌍 CORS Configuration

Para permitir uploads desde el navegador:

```json
{
  "CORSRules": [
    {
      "AllowedOrigins": ["https://okla.com.do", "https://www.okla.com.do"],
      "AllowedMethods": ["GET", "PUT", "POST", "DELETE"],
      "AllowedHeaders": ["*"],
      "MaxAgeSeconds": 3600,
      "ExposeHeaders": ["ETag"]
    }
  ]
}
```

**Configurar via AWS CLI:**

```bash
aws s3api put-bucket-cors \
  --bucket okla-media \
  --cors-configuration file://cors.json \
  --endpoint https://nyc3.digitaloceanspaces.com
```

---

## 🚀 CDN Integration (DigitalOcean Spaces CDN)

### Habilitar CDN

1. En DigitalOcean Control Panel
2. Seleccionar el Space
3. Habilitar "CDN"
4. Esperar propagación (5-10 minutos)

### Usar URL del CDN

**Sin CDN:**

```
https://okla-media.nyc3.digitaloceanspaces.com/vehicles/123/photo.jpg
```

**Con CDN:**

```
https://okla-media.nyc3.cdn.digitaloceanspaces.com/vehicles/123/photo.jpg
```

### Beneficios del CDN

- ⚡ Latencia reducida (edge locations)
- 💰 Menor costo de egress
- 🌍 Global distribution
- 🔒 HTTPS incluido

---

## ❌ Manejo de Errores

### Códigos de Error Comunes

| Código | Error                   | Descripción                          |
| ------ | ----------------------- | ------------------------------------ |
| 403    | AccessDenied            | Sin permisos o firma inválida        |
| 404    | NoSuchKey               | Archivo no existe                    |
| 409    | BucketAlreadyOwnedByYou | Bucket ya existe con ese nombre      |
| 411    | MissingContentLength    | Falta header Content-Length          |
| 416    | InvalidRange            | Rango solicitado inválido            |
| 500    | InternalError           | Error interno de S3                  |
| 503    | ServiceUnavailable      | Servicio temporalmente no disponible |

### Respuesta de Error

**XML:**

```xml
<?xml version="1.0" encoding="UTF-8"?>
<Error>
  <Code>NoSuchKey</Code>
  <Message>The specified key does not exist.</Message>
  <Key>vehicles/999/photo.jpg</Key>
  <RequestId>tx000000000000000000001-0063...</RequestId>
</Error>
```

**Manejo en C#:**

```csharp
try
{
    var response = await _s3Client.GetObjectAsync(request);
}
catch (AmazonS3Exception ex)
{
    if (ex.ErrorCode == "NoSuchKey")
    {
        _logger.LogWarning($"File not found: {request.Key}");
        return null;
    }
    else if (ex.ErrorCode == "AccessDenied")
    {
        _logger.LogError($"Access denied to file: {request.Key}");
        throw;
    }
    else
    {
        _logger.LogError(ex, "S3 error");
        throw;
    }
}
```

---

## 🔧 Ejemplos de Código C#

### MediaService - S3StorageProvider.cs

```csharp
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;

public class S3StorageProvider : IStorageProvider
{
    private readonly IAmazonS3 _s3Client;
    private readonly S3Settings _settings;
    private readonly ILogger<S3StorageProvider> _logger;

    public S3StorageProvider(
        IAmazonS3 s3Client,
        IOptions<S3Settings> settings,
        ILogger<S3StorageProvider> logger)
    {
        _s3Client = s3Client;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<UploadResult> UploadAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        string folder = "")
    {
        try
        {
            var key = GenerateKey(fileName, folder);

            var putRequest = new PutObjectRequest
            {
                BucketName = _settings.BucketName,
                Key = key,
                InputStream = fileStream,
                ContentType = contentType,
                CannedACL = S3CannedACL.PublicRead,
                Metadata =
                {
                    ["x-amz-meta-uploaded-by"] = "okla-mediaservice",
                    ["x-amz-meta-upload-date"] = DateTime.UtcNow.ToString("O")
                }
            };

            var response = await _s3Client.PutObjectAsync(putRequest);

            if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception($"S3 upload failed: {response.HttpStatusCode}");
            }

            var fileUrl = _settings.UseCdn
                ? $"https://{_settings.CdnDomain}/{key}"
                : $"https://{_settings.BucketName}.{_settings.Region}.digitaloceanspaces.com/{key}";

            _logger.LogInformation($"File uploaded successfully: {key}");

            return new UploadResult
            {
                Success = true,
                FileUrl = fileUrl,
                FileKey = key,
                FileSize = fileStream.Length,
                ContentType = contentType
            };
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "S3 error during upload");
            return new UploadResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<bool> DeleteAsync(string fileKey)
    {
        try
        {
            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = _settings.BucketName,
                Key = fileKey
            };

            var response = await _s3Client.DeleteObjectAsync(deleteRequest);

            _logger.LogInformation($"File deleted: {fileKey}");
            return response.HttpStatusCode == System.Net.HttpStatusCode.NoContent;
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, $"Error deleting file: {fileKey}");
            return false;
        }
    }

    public async Task<Stream> DownloadAsync(string fileKey)
    {
        var getRequest = new GetObjectRequest
        {
            BucketName = _settings.BucketName,
            Key = fileKey
        };

        var response = await _s3Client.GetObjectAsync(getRequest);
        return response.ResponseStream;
    }

    public string GeneratePresignedUrl(string fileKey, int expirationMinutes = 60)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _settings.BucketName,
            Key = fileKey,
            Expires = DateTime.UtcNow.AddMinutes(expirationMinutes)
        };

        return _s3Client.GetPreSignedURL(request);
    }

    private string GenerateKey(string fileName, string folder)
    {
        var sanitizedFileName = Path.GetFileName(fileName);
        var uniqueId = Guid.NewGuid().ToString("N");
        var extension = Path.GetExtension(sanitizedFileName);

        var key = string.IsNullOrEmpty(folder)
            ? $"{uniqueId}{extension}"
            : $"{folder}/{uniqueId}{extension}";

        return key;
    }
}

public class S3Settings
{
    public string BucketName { get; set; } = "okla-media";
    public string Region { get; set; } = "nyc3";
    public string AccessKey { get; set; } = "";
    public string SecretKey { get; set; } = "";
    public bool UseCdn { get; set; } = true;
    public string CdnDomain { get; set; } = "okla-media.nyc3.cdn.digitaloceanspaces.com";
}
```

### appsettings.json

```json
{
  "S3Settings": {
    "BucketName": "okla-media",
    "Region": "nyc3",
    "AccessKey": "DO00ABC123...",
    "SecretKey": "xyz789...",
    "UseCdn": true,
    "CdnDomain": "okla-media.nyc3.cdn.digitaloceanspaces.com"
  }
}
```

### Program.cs Registration

```csharp
// appsettings
builder.Services.Configure<S3Settings>(
    builder.Configuration.GetSection("S3Settings"));

// AWS S3 Client
var s3Settings = builder.Configuration.GetSection("S3Settings").Get<S3Settings>();
builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    var config = new AmazonS3Config
    {
        ServiceURL = $"https://{s3Settings.Region}.digitaloceanspaces.com",
        ForcePathStyle = true
    };

    return new AmazonS3Client(
        s3Settings.AccessKey,
        s3Settings.SecretKey,
        config
    );
});

// Storage provider
builder.Services.AddScoped<IStorageProvider, S3StorageProvider>();
```

---

## 📊 Estructura de Archivos en OKLA

```
okla-media/
├── vehicles/
│   ├── {guid}/
│   │   ├── photo-1.jpg
│   │   ├── photo-2.jpg
│   │   └── photo-3.jpg
├── users/
│   ├── avatars/
│   │   └── {guid}.jpg
│   └── documents/
│       └── {guid}/
│           ├── rrnc.pdf
│           └── license.pdf
├── dealers/
│   ├── logos/
│   │   └── {guid}.png
│   └── documents/
│       └── {guid}/
│           └── business-license.pdf
└── temp/
    └── {guid}/
        └── upload.tmp (se limpia cada 24h)
```

---

## 🧪 Testing

### Test con cURL

**Upload:**

```bash
curl -X PUT "https://okla-media.nyc3.digitaloceanspaces.com/test.txt" \
  -H "Content-Type: text/plain" \
  -H "x-amz-acl: public-read" \
  --aws-sigv4 "aws:amz:nyc3:s3" \
  --user "ACCESS_KEY:SECRET_KEY" \
  -d "Hello OKLA"
```

**Download:**

```bash
curl "https://okla-media.nyc3.digitaloceanspaces.com/test.txt"
```

**Delete:**

```bash
curl -X DELETE "https://okla-media.nyc3.digitaloceanspaces.com/test.txt" \
  --aws-sigv4 "aws:amz:nyc3:s3" \
  --user "ACCESS_KEY:SECRET_KEY"
```

---

## 💰 Costos (DigitalOcean Spaces)

| Concepto      | Precio                     |
| ------------- | -------------------------- |
| **Storage**   | $5/mes por 250 GB          |
| **Bandwidth** | 1 TB incluido gratis/mes   |
| **Exceso**    | $0.01/GB adicional         |
| **Requests**  | Ilimitados (gratis)        |
| **CDN**       | Incluido (sin costo extra) |

**Ejemplo para OKLA:**

- Storage: 100 GB de imágenes = $5/mes
- Bandwidth: 500 GB/mes (dentro del límite) = $0
- **Total:** $5/mes

---

## 🔐 Seguridad - Buenas Prácticas

1. **NUNCA exponer Secret Key en frontend**
2. **Usar presigned URLs** para uploads desde navegador
3. **Validar Content-Type** en uploads
4. **Limitar tamaño de archivo** (ej: 10MB max)
5. **Scan de virus** en archivos subidos
6. **Expirar URLs** después de un tiempo
7. **Backup regular** del bucket
8. **Lifecycle policies** para limpieza automática

---

## 📚 Referencias

- [DigitalOcean Spaces Documentation](https://docs.digitalocean.com/products/spaces/)
- [AWS S3 API Reference](https://docs.aws.amazon.com/s3/)
- [AWS SDK for .NET](https://aws.amazon.com/sdk-for-net/)
- [AWSSDK.S3 NuGet Package](https://www.nuget.org/packages/AWSSDK.S3/)

---

**Implementado en:** MediaService  
**Versión:** 1.0  
**Última actualización:** Enero 15, 2026
