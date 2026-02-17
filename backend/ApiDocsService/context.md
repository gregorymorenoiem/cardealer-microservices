# ApiDocsService - Context Documentation

## 📋 INFORMACIÓN GENERAL

- **Nombre del Servicio:** ApiDocsService
- **Puerto en Desarrollo:** 5033
- **Estado:** 🚧 **EN DESARROLLO - NO DESPLEGADO**
- **Base de Datos:** PostgreSQL (`apidocsservice`)
- **Imagen Docker:** Local only

### Propósito
Servicio de documentación de APIs. Genera y mantiene documentación automática usando Swagger/OpenAPI, incluyendo guías, ejemplos y playground interactivo.

---

## 🏗️ ARQUITECTURA

```
ApiDocsService/
├── ApiDocsService.Api/
│   ├── Controllers/
│   │   ├── SwaggerController.cs
│   │   └── GuidesController.cs
│   └── Program.cs
├── ApiDocsService.Application/
├── ApiDocsService.Domain/
│   ├── Entities/
│   │   ├── ApiEndpoint.cs
│   │   ├── Guide.cs
│   │   └── CodeExample.cs
│   └── Enums/
│       └── HttpMethodType.cs
└── ApiDocsService.Infrastructure/
```

---

## 📦 ENTIDADES PRINCIPALES

### ApiEndpoint
```csharp
public class ApiEndpoint
{
    public Guid Id { get; set; }
    
    // Endpoint
    public string ServiceName { get; set; }        // "VehiclesSaleService"
    public string Path { get; set; }               // "/api/vehicles/{id}"
    public HttpMethodType Method { get; set; }     // GET, POST, PUT, DELETE
    
    // Documentación
    public string Title { get; set; }
    public string Description { get; set; }
    public string? DetailedDescription { get; set; }
    
    // Request
    public string? RequestBodySchema { get; set; } // JSON Schema
    public string? RequestExample { get; set; }    // JSON example
    
    // Response
    public string? ResponseSchema { get; set; }
    public string? ResponseExample { get; set; }
    
    // Parámetros
    public List<EndpointParameter> Parameters { get; set; }
    
    // Autenticación
    public bool RequiresAuth { get; set; }
    public List<string>? RequiredPermissions { get; set; }
    
    // Metadata
    public bool IsDeprecated { get; set; }
    public string? DeprecationNote { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class EndpointParameter
{
    public string Name { get; set; }
    public string Location { get; set; }           // "path", "query", "header", "body"
    public string Type { get; set; }               // "string", "integer", "boolean"
    public bool Required { get; set; }
    public string? Description { get; set; }
    public string? DefaultValue { get; set; }
}
```

### Guide
```csharp
public class Guide
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Slug { get; set; }               // URL-friendly
    public string Category { get; set; }           // "GettingStarted", "Authentication", "Vehicles"
    
    // Contenido (Markdown)
    public string Content { get; set; }
    
    // Orden
    public int DisplayOrder { get; set; }
    
    // Metadata
    public Guid AuthorId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdatedAt { get; set; }
    public bool IsPublished { get; set; }
}
```

### CodeExample
```csharp
public class CodeExample
{
    public Guid Id { get; set; }
    public Guid? ApiEndpointId { get; set; }
    
    // Ejemplo
    public string Title { get; set; }
    public string Description { get; set; }
    public string Language { get; set; }           // "javascript", "python", "csharp", "curl"
    public string Code { get; set; }
    
    // Categoría
    public string Category { get; set; }           // "Authentication", "CRUD", "Search"
    
    public DateTime CreatedAt { get; set; }
}
```

---

## 📡 ENDPOINTS (Propuestos)

### Swagger/OpenAPI
- `GET /api/docs/swagger.json` - OpenAPI spec completa
- `GET /api/docs/swagger/{serviceName}` - Spec de un servicio específico
- `GET /api/docs/ui` - Swagger UI (interfaz web)

### Endpoints Documentation
- `GET /api/docs/endpoints` - Listar todos los endpoints
- `GET /api/docs/endpoints/{serviceName}` - Endpoints de un servicio
- `GET /api/docs/endpoints/{id}` - Detalle de endpoint específico

### Guides
- `GET /api/docs/guides` - Listar guías
- `GET /api/docs/guides/{slug}` - Ver guía
- `POST /api/docs/guides` - Crear guía (admin)
- `PUT /api/docs/guides/{id}` - Actualizar guía

### Code Examples
- `GET /api/docs/examples` - Listar ejemplos
- `GET /api/docs/examples/{language}` - Ejemplos en un lenguaje específico

---

## 💡 FUNCIONALIDADES PLANEADAS

### Auto-Generated from Swagger
Consumir Swagger specs de cada microservicio:
```csharp
public async Task UpdateDocumentationAsync()
{
    var services = new[] { 
        "vehiclessaleservice", "authservice", "userservice" 
    };
    
    foreach (var service in services)
    {
        var swaggerUrl = $"http://{service}:8080/swagger/v1/swagger.json";
        var spec = await _httpClient.GetFromJsonAsync<OpenApiSpec>(swaggerUrl);
        
        // Parse y guardar endpoints en BD
        foreach (var path in spec.Paths)
        {
            foreach (var operation in path.Value.Operations)
            {
                var endpoint = new ApiEndpoint
                {
                    ServiceName = service,
                    Path = path.Key,
                    Method = operation.Key,
                    Title = operation.Value.Summary,
                    Description = operation.Value.Description,
                    // ... etc
                };
                
                await _context.ApiEndpoints.AddAsync(endpoint);
            }
        }
    }
    
    await _context.SaveChangesAsync();
}
```

### Interactive Playground (Try It Out)
Swagger UI con capacidad de ejecutar requests directamente desde docs:
- Input de parámetros
- Authorization header auto-filled
- Ver response en tiempo real

### Code Generation
Generar client SDKs en múltiples lenguajes:
```javascript
// JavaScript/TypeScript
import { OklaApiClient } from '@okla/api-client';

const client = new OklaApiClient({ apiKey: 'your-key' });
const vehicles = await client.vehicles.list({ make: 'Toyota' });
```

```python
# Python
from okla_api import OklaClient

client = OklaClient(api_key='your-key')
vehicles = client.vehicles.list(make='Toyota')
```

### Changelog
Documentar cambios en API:
- **2026-01-15:** Added `fuel_efficiency` field to Vehicle model
- **2026-01-10:** Deprecated `/api/v1/vehicles` in favor of `/api/vehicles`

### Postman Collection
Export automático a Postman Collection:
```json
{
  "info": {
    "name": "OKLA API",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "item": [
    {
      "name": "Vehicles",
      "item": [
        {
          "name": "List Vehicles",
          "request": {
            "method": "GET",
            "url": "{{baseUrl}}/api/vehicles"
          }
        }
      ]
    }
  ]
}
```

---

## 📚 GUÍAS INICIALES

### Getting Started
```markdown
# Getting Started with OKLA API

## Authentication
All API requests require authentication using JWT Bearer token.

1. Register for an API key at https://okla.com.do/developers
2. Include in Authorization header: `Bearer YOUR_TOKEN`

## Base URL
Production: `https://api.okla.com.do`
Sandbox: `https://sandbox-api.okla.com.do`

## Rate Limits
- Free tier: 100 requests/hour
- Pro tier: 1000 requests/hour
- Enterprise: Contact sales

## Example Request
```curl
curl -X GET "https://api.okla.com.do/api/vehicles" \
  -H "Authorization: Bearer YOUR_TOKEN"
```
```

### Authentication Guide
```markdown
# Authentication

## Obtaining a Token
```curl
POST /api/auth/login
{
  "email": "user@example.com",
  "password": "password123"
}

Response:
{
  "token": "eyJhbGciOiJIUzI1...",
  "refreshToken": "dGhpcyBpcyBhIHJl...",
  "expiresIn": 86400
}
```

## Using the Token
Include in all subsequent requests:
```
Authorization: Bearer eyJhbGciOiJIUzI1...
```

## Refresh Token
When token expires, use refresh token to obtain new one.
```

### Pagination Guide
```markdown
# Pagination

All list endpoints support pagination using `page` and `pageSize` query params.

## Example
```
GET /api/vehicles?page=1&pageSize=20
```

## Response
```json
{
  "data": [...],
  "pagination": {
    "currentPage": 1,
    "pageSize": 20,
    "totalPages": 15,
    "totalCount": 293
  }
}
```
```

---

## 🔗 INTEGRACIÓN CON OTROS SERVICIOS

### Todos los servicios
- Consumir Swagger specs
- Sincronizar automáticamente cuando se despliega nueva versión

---

## 💡 PORTAL DE DESARROLLADORES

### Developer Portal Features
- **API Key Management:** Crear/revocar API keys
- **Usage Analytics:** Ver requests, rate limits, errors
- **Webhooks:** Configurar webhooks para eventos
- **Sandbox Environment:** Testing sin afectar producción
- **Support:** Ticket system para soporte técnico

---

**Estado:** 🚧 EN DESARROLLO - No desplegado en producción  
**Versión:** 0.1.0  
**Acceso:** https://developers.okla.com.do (futuro)
