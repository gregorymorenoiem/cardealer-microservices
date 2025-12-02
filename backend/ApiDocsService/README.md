# API Documentation Aggregator Service

Servicio transversal que centraliza y agrega la documentación OpenAPI de todos los microservicios del ecosistema CarDealer.

## 📋 Descripción

El **ApiDocsService** proporciona un punto único para acceder a la documentación de API de todos los servicios, facilitando:

- 🔍 Descubrimiento de APIs disponibles
- 📚 Agregación de especificaciones OpenAPI
- 🏥 Monitoreo del estado de documentación
- 🔎 Búsqueda de endpoints

## 🏗️ Arquitectura

```
ApiDocsService/
├── ApiDocsService.Api/           # Web API
│   ├── Controllers/              # Endpoints REST
│   └── Program.cs                # Configuración
├── ApiDocsService.Core/          # Lógica de negocio
│   ├── Models/                   # Modelos de datos
│   ├── Interfaces/               # Contratos
│   └── Services/                 # Implementaciones
└── ApiDocsService.Tests/         # Tests unitarios
```

## 🚀 Endpoints

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/docs/services` | Lista todos los servicios registrados |
| GET | `/api/docs/services/{name}` | Obtiene información de un servicio |
| GET | `/api/docs/services/{name}/spec` | Obtiene especificación OpenAPI de un servicio |
| GET | `/api/docs/aggregated` | Obtiene especificación OpenAPI agregada |
| POST | `/api/docs/refresh` | Actualiza cache de documentación |
| GET | `/api/docs/health` | Estado de salud de todos los servicios |
| GET | `/api/docs/search?query={term}` | Busca endpoints por nombre/descripción |

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

## 📝 Notas

- El servicio cachea las especificaciones por 5 minutos por defecto
- Los servicios no disponibles se marcan con `status: "Unavailable"`
- El endpoint `/health` proporciona estado detallado de cada servicio
