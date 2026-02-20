# Video360Service

Microservicio para procesamiento de video 360° de vehículos. Extrae 6 frames equidistantes de un video para crear una vista interactiva 360° en el frontend.

## 📋 Descripción

Este servicio permite a los usuarios subir un video de un vehículo girando 360° y obtiene automáticamente 6 imágenes de alta calidad que representan los ángulos principales:

| Frame | Ángulo | Etiqueta         |
| ----- | ------ | ---------------- |
| 0     | 0°     | Frente           |
| 1     | 60°    | Frente-Derecha   |
| 2     | 120°   | Atrás-Derecha    |
| 3     | 180°   | Atrás            |
| 4     | 240°   | Atrás-Izquierda  |
| 5     | 300°   | Frente-Izquierda |

## 🚀 Proveedores Soportados

| Proveedor          | Costo Mensual | Costo por Vehículo | Calidad     | Prioridad     |
| ------------------ | ------------- | ------------------ | ----------- | ------------- |
| **FFmpeg-API.com** | $11/mes       | $0.011             | Excelente   | 100 (DEFAULT) |
| **ApyHub**         | $9/mes        | $0.009             | Muy Buena   | 90            |
| **Cloudinary**     | $12/mes       | $0.012             | Buena       | 70            |
| **Imgix**          | $18/mes       | $0.018             | Excelente   | 80            |
| **Shotstack**      | $50/mes       | $0.05              | Profesional | 50            |

El sistema automáticamente selecciona el mejor proveedor disponible basado en prioridad, disponibilidad y límites diarios.

## 🏗️ Arquitectura

```
Video360Service/
├── Video360Service.Api/           # REST API Controllers
├── Video360Service.Application/   # CQRS Commands, Queries, DTOs
├── Video360Service.Domain/        # Entidades, Enums, Interfaces
├── Video360Service.Infrastructure/# Repositories, Providers, Storage
└── Video360Service.Tests/         # Unit Tests
```

### Clean Architecture

- **Domain**: Entidades de negocio puras (Video360Job, ExtractedFrame, ProviderConfiguration, UsageRecord)
- **Application**: Casos de uso con MediatR (CQRS pattern)
- **Infrastructure**: Implementaciones de providers, storage S3, y Entity Framework
- **API**: Controllers REST con autenticación JWT

## 📡 API Endpoints

### Video360 Jobs

```bash
# Crear job desde URL de video
POST /api/video360/jobs
Authorization: Bearer {token}
{
  "vehicleId": "uuid",
  "videoUrl": "https://...",
  "frameCount": 6,
  "imageFormat": "Jpeg",
  "videoQuality": "High"
}

# Subir video directamente
POST /api/video360/jobs/upload
Authorization: Bearer {token}
Content-Type: multipart/form-data
file: [video.mp4]

# Obtener job por ID
GET /api/video360/jobs/{id}

# Listar jobs
GET /api/video360/jobs?vehicleId={uuid}&status=Completed&page=1&pageSize=20

# Obtener vista 360° de un vehículo
GET /api/video360/vehicles/{vehicleId}/view

# Cancelar job
POST /api/video360/jobs/{id}/cancel

# Reintentar job fallido
POST /api/video360/jobs/{id}/retry

# Eliminar job
DELETE /api/video360/jobs/{id}
```

### Proveedores

```bash
# Listar proveedores disponibles
GET /api/providers

# Estadísticas de uso (Admin)
GET /api/providers/usage?startDate=2026-01-01&endDate=2026-01-31
```

## 🔧 Configuración

### Variables de Entorno

```bash
# Database
ConnectionStrings__DefaultConnection=Host=postgres;Database=video360service;Username=postgres;Password=xxx

# JWT
JWT_SECRET_KEY=your-32-char-secret-key

# Providers
FFMPEG_API_KEY=your-ffmpeg-api-key
APYHUB_API_TOKEN=your-apyhub-token
CLOUDINARY_CLOUD_NAME=your-cloud-name
CLOUDINARY_API_KEY=your-key
CLOUDINARY_API_SECRET=your-secret
IMGIX_API_KEY=your-imgix-key
IMGIX_SECURE_URL_TOKEN=your-secure-token
IMGIX_SOURCE_DOMAIN=your-source.imgix.net
SHOTSTACK_API_KEY=your-shotstack-key

# S3 Storage
AWS_ACCESS_KEY_ID=your-access-key
AWS_SECRET_ACCESS_KEY=your-secret-key
S3_BUCKET_NAME=okla-video360
CDN_BASE_URL=https://cdn.okla.com.do
```

### appsettings.json

```json
{
  "Providers": {
    "FfmpegApi": {
      "BaseUrl": "https://api.ffmpeg-api.com",
      "IsEnabled": true,
      "CostPerVideoUsd": 0.011
    }
  },
  "Storage": {
    "S3": {
      "BucketName": "okla-video360",
      "Region": "us-east-1"
    }
  }
}
```

## 🐳 Docker

```bash
# Build
docker build -t video360service:latest .

# Run
docker run -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="Host=postgres..." \
  -e FFMPEG_API_KEY="your-key" \
  video360service:latest
```

## 🧪 Testing

```bash
# Ejecutar todos los tests
cd Video360Service.Tests
dotnet test

# Con coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=lcov
```

### Tests Incluidos

- `Video360JobTests` - Tests de la entidad principal
- `ExtractedFrameTests` - Tests de frames extraídos
- `ProviderConfigurationTests` - Tests de configuración de proveedores
- `EnumsTests` - Tests de enumeraciones

## 📊 Flujo de Procesamiento

```
1. Usuario sube video o proporciona URL
   ↓
2. Se crea Video360Job (status: Pending)
   ↓
3. Video se guarda en S3 (status: Uploading)
   ↓
4. Orchestrator selecciona mejor proveedor
   ↓
5. Proveedor extrae 6 frames (status: Processing)
   ↓
6. Frames se guardan en S3 (status: Saving)
   ↓
7. Job completado con URLs públicas (status: Completed)
   ↓
8. Frontend muestra vista 360° interactiva
```

## 🔄 Fallback de Proveedores

Si un proveedor falla, el sistema automáticamente:

1. Registra el error
2. Incrementa retry count
3. Intenta con el siguiente proveedor disponible (ordenado por prioridad)
4. Continúa hasta agotar todos los proveedores o éxito

## 📈 Métricas

- `/health` - Health check
- `/metrics` - Prometheus metrics

## 🔐 Seguridad

- Autenticación JWT requerida para crear jobs
- CORS configurado para dominios de producción
- Rate limiting por tenant (configurable)
- Límites diarios por proveedor

## 📦 Dependencias

- .NET 8.0
- Entity Framework Core 8.0
- MediatR 12.4
- FluentValidation 11.3
- AWS SDK S3
- Serilog
- OpenTelemetry

---

**Autor:** OKLA Team  
**Versión:** 1.0.0  
**Puerto:** 8080
