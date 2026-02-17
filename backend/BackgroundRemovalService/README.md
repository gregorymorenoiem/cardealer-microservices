# 🖼️ Background Removal Service

Microservicio escalable para **remoción de fondos de imágenes** con soporte para múltiples proveedores de API.

## 🎯 Características

- ✅ **ClipDrop por Defecto**: Usa ClipDrop (Stability AI) como proveedor principal
- ✅ **Multi-Proveedor**: Soporta ClipDrop, Remove.bg, Photoroom, Slazzer, y fácilmente extensible
- ✅ **Configurable**: El proveedor por defecto se configura en appsettings.json
- ✅ **Strategy Pattern**: Arquitectura limpia para agregar nuevos proveedores
- ✅ **Fallback Automático**: Si un proveedor falla, usa el siguiente disponible
- ✅ **Circuit Breaker**: Protección contra proveedores con problemas
- ✅ **Rate Limiting**: Control de uso por proveedor
- ✅ **Tracking de Uso**: Historial y estadísticas para billing
- ✅ **Almacenamiento**: S3 o local para imágenes procesadas
- ✅ **Health Checks**: Monitoreo de disponibilidad de proveedores

## 🏗️ Arquitectura

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                          BACKGROUND REMOVAL SERVICE                          │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ┌─────────────────┐    ┌────────────────────┐    ┌──────────────────────┐ │
│  │   API Layer     │───▶│  Application Layer │───▶│  Infrastructure      │ │
│  │  (Controllers)  │    │  (Orchestrator)    │    │  (Providers)         │ │
│  └─────────────────┘    └────────────────────┘    └──────────────────────┘ │
│                                   │                         │              │
│                                   │                         ▼              │
│                                   │              ┌──────────────────────┐  │
│                                   │              │  Provider Factory    │  │
│                                   │              │  (Strategy Pattern)  │  │
│                                   │              └──────────────────────┘  │
│                                   │                         │              │
│                                   ▼                         ▼              │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │                         PROVIDERS                                    │   │
│  │  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌────────┐ │   │
│  │  │ ClipDrop │  │Remove.bg │  │Photoroom │  │ Slazzer  │  │ Local  │ │   │
│  │  │  $0.05   │  │  $0.20   │  │  $0.05   │  │  $0.02   │  │  Free  │ │   │
│  │  │ DEFAULT⭐│  │          │  │          │  │          │  │        │ │   │
│  │  └──────────┘  └──────────┘  └──────────┘  └──────────┘  └────────┘ │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

## 📁 Estructura del Proyecto

```
BackgroundRemovalService/
├── BackgroundRemovalService.Domain/           # Entidades y contratos
│   ├── Entities/
│   │   ├── BackgroundRemovalJob.cs           # Job de procesamiento
│   │   ├── ProviderConfiguration.cs          # Config de proveedores
│   │   └── UsageRecord.cs                    # Registro de uso
│   ├── Enums/
│   │   ├── BackgroundRemovalProvider.cs      # Tipos de proveedores
│   │   ├── ProcessingStatus.cs               # Estados de procesamiento
│   │   ├── OutputFormat.cs                   # Formatos de salida
│   │   └── ImageSize.cs                      # Tamaños de imagen
│   └── Interfaces/
│       ├── IBackgroundRemovalProvider.cs     # Contrato de proveedor
│       ├── IBackgroundRemovalJobRepository.cs
│       ├── IProviderConfigurationRepository.cs
│       └── IUsageRecordRepository.cs
│
├── BackgroundRemovalService.Application/      # Lógica de aplicación
│   ├── DTOs/
│   │   └── BackgroundRemovalDtos.cs          # Request/Response DTOs
│   ├── Interfaces/
│   │   ├── IBackgroundRemovalProviderFactory.cs
│   │   ├── IBackgroundRemovalOrchestrator.cs
│   │   └── IImageStorageService.cs
│   ├── Features/
│   │   ├── Commands/
│   │   ├── Queries/
│   │   └── Handlers/
│   └── Validators/
│       └── CreateRemovalJobRequestValidator.cs
│
├── BackgroundRemovalService.Infrastructure/   # Implementaciones
│   ├── Persistence/
│   │   ├── BackgroundRemovalDbContext.cs
│   │   └── Repositories/
│   ├── Providers/                             # ⭐ Proveedores de API
│   │   ├── ClipDropProvider.cs               # ClipDrop (DEFAULT)
│   │   ├── RemoveBgProvider.cs               # Remove.bg
│   │   ├── PhotoroomProvider.cs              # Photoroom
│   │   ├── SlazzerProvider.cs                # Slazzer
│   │   └── BackgroundRemovalProviderFactory.cs
│   └── Services/
│       ├── BackgroundRemovalOrchestrator.cs  # Orquestador principal
│       └── ImageStorageService.cs            # S3/Local storage
│
├── BackgroundRemovalService.Api/              # API REST
│   ├── Controllers/
│   │   └── BackgroundRemovalController.cs
│   ├── Program.cs
│   ├── appsettings.json
│   └── Dockerfile
│
└── BackgroundRemovalService.sln
```

## 🚀 Inicio Rápido

### 1. Configurar API Keys

El servicio usa **ClipDrop como proveedor por defecto**. Configura en `appsettings.json` o usa variables de entorno:

```json
{
  "BackgroundRemoval": {
    "DefaultProvider": "ClipDrop",
    "FallbackProviders": ["RemoveBg", "Photoroom", "Slazzer"],
    "EnableFallback": true,
    "Providers": {
      "ClipDrop": {
        "ApiKey": "tu-api-key-de-clipdrop",
        "BaseUrl": "https://clipdrop-api.co",
        "TimeoutSeconds": 60,
        "CostPerImageUsd": 0.05,
        "IsDefault": true
      },
      "RemoveBg": {
        "ApiKey": "tu-api-key-de-removebg"
      },
      "Photoroom": {
        "ApiKey": "tu-api-key-de-photoroom"
      },
      "Slazzer": {
        "ApiKey": "tu-api-key-de-slazzer"
      }
    }
  }
}
```

O con variables de entorno:

```bash
export CLIPDROP_API_KEY=tu-api-key    # PRINCIPAL
export REMOVEBG_API_KEY=tu-api-key
export PHOTOROOM_API_KEY=tu-api-key
export SLAZZER_API_KEY=tu-api-key
```

### Cambiar Proveedor por Defecto

Para usar otro proveedor por defecto, cambia `DefaultProvider`:

```json
{
  "BackgroundRemoval": {
    "DefaultProvider": "RemoveBg" // Cambia a Remove.bg
  }
}
```

### 2. Ejecutar

```bash
# Desarrollo
cd backend/BackgroundRemovalService
dotnet run --project BackgroundRemovalService.Api

# Docker
docker build -t background-removal-service -f backend/BackgroundRemovalService/Dockerfile .
docker run -p 5080:8080 -e CLIPDROP_API_KEY=tu-api-key background-removal-service
```

### 3. Probar API

```bash
# Remover fondo con proveedor por defecto (ClipDrop)
curl -X POST http://localhost:5080/api/backgroundremoval/remove \
  -H "Content-Type: application/json" \
  -d '{
    "imageUrl": "https://example.com/car.jpg",
    "outputFormat": 1,
    "objectType": "car"
  }'

# Remover fondo con proveedor específico
curl -X POST http://localhost:5080/api/backgroundremoval/remove \
  -H "Content-Type: application/json" \
  -d '{
    "imageUrl": "https://example.com/car.jpg",
    "preferredProvider": 1,
    "outputFormat": 1
  }'

# Remover fondo (con Base64)
curl -X POST http://localhost:5080/api/backgroundremoval/remove \
  -H "Content-Type: application/json" \
  -d '{
    "imageBase64": "data:image/jpeg;base64,/9j/4AAQ...",
    "outputFormat": 1
  }'

# Ver estado de proveedores
curl http://localhost:5080/api/backgroundremoval/providers/health
```

## 📊 Proveedores Disponibles

| Proveedor | Enum Value | Costo/Imagen | Por Defecto | Documentación                                   |
| --------- | ---------- | ------------ | ----------- | ----------------------------------------------- |
| ClipDrop  | 0          | $0.05        | ✅ Sí       | https://clipdrop.co/apis/docs/remove-background |
| Remove.bg | 1          | $0.20        | ❌ No       | https://www.remove.bg/api                       |
| Photoroom | 2          | $0.05        | ❌ No       | https://www.photoroom.com/api                   |
| Slazzer   | 4          | $0.02        | ❌ No       | https://www.slazzer.com/api                     |

## 📡 API Endpoints

| Método | Endpoint                                  | Descripción            |
| ------ | ----------------------------------------- | ---------------------- |
| `POST` | `/api/backgroundremoval/remove`           | Procesar imagen        |
| `GET`  | `/api/backgroundremoval/jobs/{id}`        | Estado de un job       |
| `GET`  | `/api/backgroundremoval/jobs`             | Lista de jobs (auth)   |
| `POST` | `/api/backgroundremoval/jobs/{id}/retry`  | Reintentar job fallido |
| `POST` | `/api/backgroundremoval/jobs/{id}/cancel` | Cancelar job           |
| `GET`  | `/api/backgroundremoval/providers`        | Info de proveedores    |
| `GET`  | `/api/backgroundremoval/providers/health` | Health check           |
| `GET`  | `/api/backgroundremoval/usage`            | Estadísticas de uso    |

## ➕ Agregar un Nuevo Proveedor

Para agregar un nuevo proveedor (ej: Clipping Magic):

### 1. Agregar al Enum

```csharp
// Domain/Enums/BackgroundRemovalProvider.cs
public enum BackgroundRemovalProvider
{
    ClipDrop = 0,       // ⭐ DEFAULT
    RemoveBg = 1,
    Photoroom = 2,
    ClippingMagic = 3,  // ← Nuevo
    Slazzer = 4,
    // ...
}
```

### 2. Crear la Configuración

```csharp
// Infrastructure/Providers/ClippingMagicProvider.cs
public class ClippingMagicSettings
{
    public const string SectionName = "BackgroundRemoval:Providers:ClippingMagic";
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://clippingmagic.com/api/v1";
    public int TimeoutSeconds { get; set; } = 60;
    public decimal CostPerImageUsd { get; set; } = 0.15m;
}
```

### 3. Implementar el Proveedor

```csharp
public class ClippingMagicProvider : IBackgroundRemovalProvider
{
    public BackgroundRemovalProvider ProviderType => BackgroundRemovalProvider.ClippingMagic;
    public string ProviderName => "Clipping Magic";

    public async Task<BackgroundRemovalResult> RemoveBackgroundAsync(
        byte[] imageBytes,
        BackgroundRemovalOptions options,
        CancellationToken cancellationToken = default)
    {
        // Implementar llamada a la API de Clipping Magic
    }

    // ... otros métodos
}
```

### 4. Registrar en DI

```csharp
// Program.cs
builder.Services.Configure<ClippingMagicSettings>(
    builder.Configuration.GetSection(ClippingMagicSettings.SectionName));

builder.Services.AddHttpClient<ClippingMagicProvider>()
    .AddStandardResilienceHandler();

builder.Services.AddScoped<IBackgroundRemovalProvider, ClippingMagicProvider>();
```

### 5. Configurar

```json
{
  "BackgroundRemoval": {
    "Providers": {
      "ClippingMagic": {
        "ApiKey": "tu-api-key",
        "BaseUrl": "https://clippingmagic.com/api/v1",
        "CostPerImageUsd": 0.15
      }
    }
  }
}
```

## 🔄 Lógica de Fallback

El servicio selecciona automáticamente el mejor proveedor disponible:

1. **Prioridad**: Usa el proveedor configurado con menor prioridad
2. **Disponibilidad**: Salta proveedores en circuit breaker o sin créditos
3. **Fallback**: Si falla, intenta con el siguiente proveedor
4. **Reintentos**: Hasta 3 reintentos por job

```csharp
// Orden de selección:
// 1. PreferredProvider del request (si especificado)
// 2. Proveedor con menor Priority en ProviderConfiguration
// 3. Primer proveedor disponible por IsAvailable()
```

## 💰 Comparación de Proveedores

| Proveedor          | Costo/Imagen | Calidad    | Velocidad | Especial             |
| ------------------ | ------------ | ---------- | --------- | -------------------- |
| **Remove.bg**      | $0.20        | ⭐⭐⭐⭐⭐ | Rápido    | Mejor para autos     |
| **Photoroom**      | $0.05        | ⭐⭐⭐⭐   | Rápido    | Bueno para productos |
| **Slazzer**        | $0.02        | ⭐⭐⭐     | Medio     | Más económico        |
| **Clipping Magic** | $0.15        | ⭐⭐⭐⭐   | Medio     | Edición manual       |
| **Local (rembg)**  | Gratis       | ⭐⭐⭐     | Lento     | Sin costos de API    |

## 🔧 Variables de Entorno

| Variable                               | Descripción               | Default         |
| -------------------------------------- | ------------------------- | --------------- |
| `REMOVEBG_API_KEY`                     | API Key de Remove.bg      | -               |
| `PHOTOROOM_API_KEY`                    | API Key de Photoroom      | -               |
| `SLAZZER_API_KEY`                      | API Key de Slazzer        | -               |
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection     | localhost       |
| `Storage__S3__UseLocalPath`            | Usar almacenamiento local | true            |
| `Storage__S3__BucketName`              | Bucket de S3              | cardealer-media |

## 📊 Monitoreo

- **Health Check**: `GET /health`
- **Provider Health**: `GET /api/backgroundremoval/providers/health`
- **Métricas**: OpenTelemetry + Prometheus ready

## 🧪 Testing

```bash
# Unit tests
dotnet test

# Integration tests (requiere Docker)
docker-compose -f docker-compose.test.yml up -d
dotnet test --filter Category=Integration
```

## 📄 Licencia

Propiedad de OKLA - Uso interno.
