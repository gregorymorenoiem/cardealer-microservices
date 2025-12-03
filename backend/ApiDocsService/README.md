# API Documentation Aggregator Service with Versioning and Testing UI

Servicio transversal que centraliza y agrega la documentación OpenAPI de todos los microservicios del ecosistema CarDealer, con soporte completo para versionado de APIs y una interfaz de testing integrada.

## 📋 Descripción

El **ApiDocsService** proporciona un punto único para acceder a la documentación de API de todos los servicios, facilitando:

- 🔍 Descubrimiento de APIs disponibles
- 📚 Agregación de especificaciones OpenAPI
- 🏥 Monitoreo del estado de documentación
- 🔎 Búsqueda de endpoints
- 🔄 **Versionado y comparación de APIs**
- 🧪 **Interfaz de testing integrada**
- 📊 **Análisis de deprecaciones**
- 🚀 **Testing de endpoints en tiempo real**

## 🏗️ Arquitectura

```
ApiDocsService/
├── ApiDocsService.Api/           # Web API
│   ├── Controllers/              # Endpoints REST
│   │   ├── DocsController.cs     # Documentación y agregación
│   │   ├── VersionController.cs  # ✨ Gestión de versiones
│   │   └── TestingController.cs  # ✨ Testing de APIs
│   ├── wwwroot/                  # ✨ Testing UI
│   │   └── testing.html          # Interfaz de testing web
│   └── Program.cs                # Configuración
├── ApiDocsService.Core/          # Lógica de negocio
│   ├── Models/                   # Modelos de datos
│   │   ├── ServiceInfo.cs        # Información de servicios
│   │   └── ApiVersion.cs         # ✨ Modelos de versionado
│   ├── Interfaces/               # Contratos
│   │   ├── IApiAggregatorService.cs
│   │   └── IVersionService.cs    # ✨ Versionado
│   └── Services/                 # Implementaciones
│       ├── ApiAggregatorService.cs
│       └── VersionService.cs     # ✨ Gestión de versiones
├── ApiDocsService.Tests/         # Tests unitarios (19 tests)
├── API_VERSIONING_GUIDE.md       # ✨ Guía de versionado
└── API_TESTING_UI_GUIDE.md       # ✨ Guía de testing UI
```

## 🚀 Endpoints

### Documentación

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/docs/services` | Lista todos los servicios registrados |
| GET | `/api/docs/services/{name}` | Obtiene información de un servicio |
| GET | `/api/docs/services/{name}/spec` | Obtiene especificación OpenAPI de un servicio |
| GET | `/api/docs/aggregated` | Obtiene especificación OpenAPI agregada |
| POST | `/api/docs/refresh` | Actualiza cache de documentación |
| GET | `/api/docs/health` | Estado de salud de todos los servicios |
| GET | `/api/docs/search?query={term}` | Busca endpoints por nombre/descripción |
| GET | `/api/docs/dashboard` | Dashboard con estadísticas |
| GET | `/api/docs/categories` | Lista de categorías |

### ✨ Versionado de APIs

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/version/services` | Lista todos los servicios versionados |
| GET | `/api/version/services/{name}` | Versiones de un servicio específico |
| GET | `/api/version/compare/{name}` | Compara dos versiones (query: fromVersion, toVersion) |
| GET | `/api/version/deprecated` | APIs deprecadas en todos los servicios |
| GET | `/api/version/deprecated/{name}/{version}` | Verifica si una versión está deprecada |
| GET | `/api/version/migration/{name}` | Ruta de migración entre versiones |

### ✨ Testing de APIs

| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | `/api/testing/execute` | Ejecuta un request de prueba |
| POST | `/api/testing/batch` | Ejecuta múltiples requests |
| GET | `/api/testing/collections` | Colecciones de tests guardadas |
| GET | `/testing` | 🎨 Interfaz web de testing |

## ⚙️ Configuración

### appsettings.json

```json
{
  "Services": [
    {
      "Name": "ErrorService",
      "BaseUrl": "http://errorservice:5050",
      "SwaggerEndpoint": "/swagger/v1/swagger.json",
      "Tags": ["infrastructure", "logging"]
    },
    {
      "Name": "AuthService",
      "BaseUrl": "http://authservice:5060",
      "SwaggerEndpoint": "/swagger/v1/swagger.json",
      "Tags": ["security", "auth"]
    }
  ]
}
```

### Variables de Entorno

| Variable | Descripción | Valor por defecto |
|----------|-------------|-------------------|
| `ASPNETCORE_ENVIRONMENT` | Entorno de ejecución | `Development` |
| `Services__0__BaseUrl` | URL base del primer servicio | - |

## 🏃 Ejecución Local

```bash
# Restaurar dependencias
dotnet restore

# Ejecutar en desarrollo
cd ApiDocsService.Api
dotnet run

# Swagger UI disponible en:
# http://localhost:5320/swagger
```

## 🐳 Docker

```bash
# Construir imagen
docker build -t apidocsservice:latest .

# Ejecutar contenedor
docker run -p 5320:8080 apidocsservice:latest
```

## 🧪 Tests

```bash
# Ejecutar tests
dotnet test ApiDocsService.Tests

# Con cobertura
dotnet test ApiDocsService.Tests --collect:"XPlat Code Coverage"
```

**Cobertura actual: 19/19 tests pasando (100%)**

### Tests Implementados

- ✅ ApiAggregatorServiceTests (12 tests)
  - Obtención de servicios
  - Health checks
  - Especificaciones OpenAPI
  - Dashboard y búsqueda

- ✅ VersionServiceTests (8 tests)
  - Gestión de versiones
  - Comparación de versiones
  - APIs deprecadas
  - Rutas de migración

- ✅ TestingControllerTests (4 tests)
  - Ejecución de requests
  - Batch testing
  - Colecciones de tests

- ✅ DocsControllerTests (existing)
  - Endpoints de documentación

## 📊 Ejemplo de Respuesta

### GET /api/docs/services

```json
[
  {
    "name": "ErrorService",
    "baseUrl": "http://errorservice:5050",
    "swaggerEndpoint": "/swagger/v1/swagger.json",
    "version": "v1",
    "status": "Available",
    "tags": ["infrastructure", "logging"],
    "endpoints": [
      {
        "path": "/api/errors",
        "method": "GET",
        "summary": "Get all errors",
        "tags": ["Errors"]
      }
    ]
  }
]
```

### GET /api/docs/aggregated

```json
{
  "openapi": "3.0.1",
  "info": {
    "title": "CarDealer API Aggregated Documentation",
    "version": "1.0.0"
  },
  "servers": [
    { "url": "http://errorservice:5050" },
    { "url": "http://authservice:5060" }
  ],
  "paths": {
    "/api/errors": { ... },
    "/api/auth/login": { ... }
  }
}
```

## 🔗 Dependencias

- .NET 8.0
- Swashbuckle.AspNetCore 6.5.0
- Serilog 8.0.0
- Microsoft.Extensions.Http
- Microsoft.Extensions.Caching.Memory
- FluentAssertions 6.12.0 (testing)
- Moq 4.20.70 (testing)
- xUnit 2.5.3 (testing)

## ✨ Nuevas Características

### 1. Versionado de APIs

Sistema completo de gestión de versiones:
- Tracking de versiones por servicio
- Comparación entre versiones
- Detección de breaking changes
- Gestión de deprecaciones
- Rutas de migración

📖 **[Ver guía completa de versionado](API_VERSIONING_GUIDE.md)**

### 2. Testing UI

Interfaz web interactiva para testing:
- Constructor visual de requests
- Soporte para todos los métodos HTTP
- Editor de headers, query params y body
- Visor de respuestas con formato
- Batch testing
- Colecciones de tests

📖 **[Ver guía completa de testing UI](API_TESTING_UI_GUIDE.md)**

Acceso: `http://localhost:5320/testing`

### 3. Ejemplos de Uso

#### Comparar Versiones

```bash
curl -X GET "http://localhost:5320/api/version/compare/AuthService?fromVersion=v1&toVersion=v2"
```

#### Testing desde UI Web

1. Navega a `http://localhost:5320/testing`
2. Selecciona un servicio
3. Configura tu request (método, URL, headers, body)
4. Click en "Send Request"
5. Visualiza la respuesta

#### Batch Testing

```bash
curl -X POST http://localhost:5320/api/testing/batch \
  -H "Content-Type: application/json" \
  -d '{
    "tests": [
      {
        "serviceName": "AuthService",
        "path": "/health",
        "method": "GET"
      },
      {
        "serviceName": "VehicleService",
        "path": "/health",
        "method": "GET"
      }
    ]
  }'
```

## 📝 Notas

- El servicio cachea las especificaciones por 5 minutos por defecto
- Los servicios no disponibles se marcan con `status: "Unavailable"`
- El endpoint `/health` proporciona estado detallado de cada servicio
- ✨ **Testing UI** accesible en `/testing` - no requiere herramientas externas
- ✨ **Version tracking** automático para todos los servicios registrados
- ✨ **Batch testing** permite ejecutar múltiples tests en secuencia

## 🎯 Roadmap

- [x] ✅ Agregación de documentación OpenAPI
- [x] ✅ Health checks de servicios
- [x] ✅ Búsqueda de endpoints
- [x] ✅ **Versionado de APIs**
- [x] ✅ **Testing UI integrada**
- [x] ✅ **Comparación de versiones**
- [ ] 🔄 Monitoreo de uso de APIs deprecadas
- [ ] 🔄 Integración con CI/CD para tests automatizados
- [ ] 🔄 Métricas de uso por endpoint
- [ ] 🔄 Rate limiting por servicio
