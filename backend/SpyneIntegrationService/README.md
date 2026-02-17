# 🎨 SpyneIntegrationService

**Microservicio de integración con Spyne AI** para transformación profesional de imágenes de vehículos, generación de 360° Spins, videos tours, y chat AI.

**Puerto:** 5070  
**Estado:** ✅ COMPLETO (Fases 1-4)

---

## 📋 Descripción

SpyneIntegrationService proporciona integración completa con la plataforma [Spyne AI](https://spyne.ai) para:

| Fase       | Feature                    | Estado      | Frontend           |
| ---------- | -------------------------- | ----------- | ------------------ |
| **Fase 1** | Transformación de Imágenes | ✅ Completo | ✅ Consumible      |
| **Fase 2** | 360° Spins                 | ✅ Completo | ✅ Consumible      |
| **Fase 3** | Video Tours                | ✅ Completo | ✅ Consumible      |
| **Fase 4** | Chat AI (Vini)             | ✅ Completo | ⚠️ **NO CONSUMIR** |

> ⚠️ **IMPORTANTE:** La Fase 4 (Chat AI) está completamente implementada en el backend pero **NO debe ser consumida por el frontend** en esta versión.

---

## 🏗️ Arquitectura

Clean Architecture con las siguientes capas:

```
SpyneIntegrationService/
├── SpyneIntegrationService.Domain/         # Entidades, Enums, Interfaces
├── SpyneIntegrationService.Application/    # DTOs, Commands, Queries, Validators
├── SpyneIntegrationService.Infrastructure/ # DbContext, Repositories, SpyneApiClient
├── SpyneIntegrationService.Api/            # Controllers, Program.cs, Config
└── SpyneIntegrationService.Tests/          # Unit Tests
```

### Stack Tecnológico

| Componente      | Tecnología                           |
| --------------- | ------------------------------------ |
| Framework       | .NET 8.0                             |
| CQRS            | MediatR 12.2.0                       |
| Validación      | FluentValidation 11.9.0              |
| ORM             | Entity Framework Core 8.0.0          |
| Base de Datos   | PostgreSQL (Npgsql)                  |
| HTTP Resilience | Microsoft.Extensions.Http.Resilience |
| Testing         | xUnit + FluentAssertions + Moq       |

---

## 📡 API Endpoints

### 🖼️ Images (Fase 1)

| Método | Endpoint                          | Descripción         |
| ------ | --------------------------------- | ------------------- |
| `POST` | `/api/images/transform`           | Transformar imagen  |
| `GET`  | `/api/images/{id}/status`         | Verificar estado    |
| `GET`  | `/api/images/vehicle/{vehicleId}` | Listar por vehículo |
| `POST` | `/api/images/batch`               | Batch processing    |

### 🔄 Spins 360° (Fase 2)

| Método | Endpoint                         | Descripción               |
| ------ | -------------------------------- | ------------------------- |
| `POST` | `/api/spins/generate`            | Generar spin 360°         |
| `GET`  | `/api/spins/{id}/status`         | Verificar estado          |
| `GET`  | `/api/spins/vehicle/{vehicleId}` | Obtener spin del vehículo |

### 🎬 Videos (Fase 3)

| Método | Endpoint                          | Descripción                |
| ------ | --------------------------------- | -------------------------- |
| `POST` | `/api/videos/generate`            | Generar video tour         |
| `GET`  | `/api/videos/{id}/status`         | Verificar estado           |
| `GET`  | `/api/videos/vehicle/{vehicleId}` | Listar videos del vehículo |

### 💬 Chat AI (Fase 4) - ⚠️ NO CONSUMIR EN FRONTEND

| Método | Endpoint                                  | Descripción            |
| ------ | ----------------------------------------- | ---------------------- |
| `POST` | `/api/chat/sessions/start`                | Iniciar sesión         |
| `POST` | `/api/chat/sessions/{sessionId}/messages` | Enviar mensaje         |
| `POST` | `/api/chat/sessions/{sessionId}/end`      | Cerrar sesión          |
| `GET`  | `/api/chat/sessions/{sessionId}`          | Obtener sesión         |
| `GET`  | `/api/chat/vehicle/{vehicleId}/history`   | Historial del vehículo |

### ⚙️ Webhooks

| Método | Endpoint              | Descripción               |
| ------ | --------------------- | ------------------------- |
| `POST` | `/api/webhooks/spyne` | Recibir webhooks de Spyne |

### 🎨 Presets

| Método | Endpoint                   | Descripción             |
| ------ | -------------------------- | ----------------------- |
| `GET`  | `/api/presets/backgrounds` | Listar presets de fondo |

---

## 🔧 Configuración

### Variables de Entorno

```env
# Base de datos
ConnectionStrings__SpyneDb=Host=postgres;Database=spyneintegration;Username=postgres;Password=xxx

# Spyne AI API
Spyne__ApiKey=your-spyne-api-key
Spyne__BaseUrl=https://api.spyne.ai/v2
Spyne__WebhookSecret=your-webhook-secret

# JWT (para autenticación)
JwtSettings__Secret=your-jwt-secret
JwtSettings__Issuer=cardealer-microservices
JwtSettings__Audience=cardealer-clients
```

### appsettings.json

```json
{
  "Spyne": {
    "BaseUrl": "https://api.spyne.ai/v2",
    "ApiKey": "YOUR_API_KEY",
    "WebhookSecret": "YOUR_WEBHOOK_SECRET",
    "TimeoutSeconds": 120,
    "RetryCount": 3
  }
}
```

---

## 🚀 Ejecutar Localmente

```bash
# Navegar al servicio
cd backend/SpyneIntegrationService

# Restaurar dependencias
dotnet restore

# Compilar
dotnet build

# Ejecutar tests
dotnet test

# Ejecutar API
dotnet run --project SpyneIntegrationService.Api
```

El servicio estará disponible en `http://localhost:5070`.

### Health Check

```bash
curl http://localhost:5070/health
```

---

## 🐳 Docker

### Build

```bash
docker build -t cardealer-spyneintegrationservice:latest .
```

### Run

```bash
docker run -d \
  -p 5070:8080 \
  -e ConnectionStrings__SpyneDb="Host=host.docker.internal;Database=spyneintegration;..." \
  -e Spyne__ApiKey="your-key" \
  cardealer-spyneintegrationservice:latest
```

---

## 📊 Tipos de Transformación

### BackgroundPresets

| Preset        | Descripción                     |
| ------------- | ------------------------------- |
| `Transparent` | Fondo transparente (PNG)        |
| `White`       | Fondo blanco sólido             |
| `Studio`      | Estudio fotográfico profesional |
| `Showroom`    | Showroom de dealer              |
| `Outdoor`     | Exterior natural                |
| `Urban`       | Ciudad/urbano                   |
| `Custom`      | Color personalizado (hex)       |

### TransformationTypes

| Tipo                    | Descripción          |
| ----------------------- | -------------------- |
| `BackgroundRemoval`     | Eliminar fondo       |
| `BackgroundReplacement` | Reemplazar fondo     |
| `Enhancement`           | Mejora de imagen     |
| `PlateMasking`          | Ocultar placas       |
| `Full`                  | Todos los anteriores |

### VideoStyles

| Estilo      | Descripción                    |
| ----------- | ------------------------------ |
| `Cinematic` | Estilo cinematográfico         |
| `Dynamic`   | Transiciones dinámicas         |
| `Showcase`  | Exhibición simple              |
| `Social`    | Optimizado para redes sociales |
| `Premium`   | Alta calidad premium           |

---

## 🧪 Tests

El proyecto incluye tests unitarios para:

- **Domain Tests:** Entidades y lógica de negocio
- **Validator Tests:** Validación de comandos
- **Handler Tests:** Lógica de handlers (próximamente)

```bash
# Ejecutar todos los tests
dotnet test

# Con coverage
dotnet test /p:CollectCoverage=true
```

---

## 📈 Métricas y Observabilidad

### Endpoints de Monitoreo

- `/health` - Health check
- `/metrics` - Métricas Prometheus (si habilitado)

### Logging

El servicio usa Serilog con:

- Console sink (desarrollo)
- JSON formatting (producción)
- Request logging automático

---

## 🔗 Integración con Gateway

Agregar en `ocelot.prod.json`:

```json
{
  "Routes": [
    {
      "DownstreamPathTemplate": "/api/images/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        { "Host": "spyneintegrationservice", "Port": 8080 }
      ],
      "UpstreamPathTemplate": "/api/spyne/images/{everything}",
      "UpstreamHttpMethod": ["GET", "POST"]
    },
    {
      "DownstreamPathTemplate": "/api/spins/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        { "Host": "spyneintegrationservice", "Port": 8080 }
      ],
      "UpstreamPathTemplate": "/api/spyne/spins/{everything}",
      "UpstreamHttpMethod": ["GET", "POST"]
    },
    {
      "DownstreamPathTemplate": "/api/videos/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        { "Host": "spyneintegrationservice", "Port": 8080 }
      ],
      "UpstreamPathTemplate": "/api/spyne/videos/{everything}",
      "UpstreamHttpMethod": ["GET", "POST"]
    }
  ]
}
```

> ⚠️ **Nota:** El endpoint `/api/chat/*` NO debe exponerse en el Gateway para esta versión.

---

## 📝 Changelog

### v1.0.0 (Enero 2026)

- ✅ Fase 1: Image Transformation completa
- ✅ Fase 2: 360° Spin Generation completa
- ✅ Fase 3: Video Tour Generation completa
- ✅ Fase 4: Chat AI (Vini) - Backend only, no frontend consumption

---

## 🤝 Contribuir

1. Crear branch: `feature/spyne-nueva-feature`
2. Implementar cambios
3. Crear tests
4. PR a `development`

---

**Desarrollado para OKLA Marketplace** 🚗  
_Powered by Spyne AI_
