# 🔍 Elasticsearch API - Documentación Completa

**API:** Elasticsearch  
**Versión:** 8.x  
**Proveedor:** Elastic NV  
**Estado:** ✅ En Producción  
**Última actualización:** Enero 15, 2026

---

## 📋 Tabla de Contenidos

1. [Introducción](#introducción)
2. [Características Principales](#características-principales)
3. [Casos de Uso en OKLA](#casos-de-uso-en-okla)
4. [Configuración e Instalación](#configuración-e-instalación)
5. [Autenticación](#autenticación)
6. [Arquitectura de Integración](#arquitectura-de-integración)
7. [Esquema de Índices](#esquema-de-índices)
8. [Operaciones Principales](#operaciones-principales)
9. [Ejemplos de Código](#ejemplos-de-código)
10. [Monitoreo y Salud](#monitoreo-y-salud)
11. [Manejo de Errores](#manejo-de-errores)
12. [Mejores Prácticas](#mejores-prácticas)
13. [Troubleshooting](#troubleshooting)
14. [Costos y Límites](#costos-y-límites)
15. [Recursos Adicionales](#recursos-adicionales)

---

## 🎯 Introducción

**Elasticsearch** es un motor de búsqueda y análisis distribuido basado en Apache Lucene. En OKLA, se utiliza para indexación y búsqueda avanzada de logs de error, permitiendo análisis en tiempo real y troubleshooting eficiente.

### ¿Por qué Elasticsearch en OKLA?

- ✅ **Búsqueda Full-Text:** Búsqueda rápida en millones de logs
- ✅ **Análisis en Tiempo Real:** Agregaciones y métricas instantáneas
- ✅ **Escalabilidad:** Crece con el volumen de datos
- ✅ **Integración Nativa:** SDK oficial para .NET
- ✅ **Visualización:** Compatible con Kibana para dashboards

---

## ✨ Características Principales

### Capacidades Core

| Característica         | Descripción                   | Uso en OKLA                              |
| ---------------------- | ----------------------------- | ---------------------------------------- |
| **Full-Text Search**   | Búsqueda avanzada con scoring | Búsqueda de errores por palabra clave    |
| **Aggregations**       | Análisis estadístico de datos | Errores por servicio, frecuencia, trends |
| **Real-Time Indexing** | Indexación inmediata          | Logs disponibles al instante             |
| **Distributed**        | Arquitectura distribuida      | Alta disponibilidad y performance        |
| **RESTful API**        | API HTTP estándar             | Fácil integración con .NET               |
| **Schema-Free**        | JSON dinámico                 | Flexibilidad en estructura de logs       |

### Ventajas vs Alternativas

| Aspecto                | Elasticsearch | SQL Database | Log Files  |
| ---------------------- | ------------- | ------------ | ---------- |
| **Búsqueda Full-Text** | ⭐⭐⭐⭐⭐    | ⭐⭐         | ⭐         |
| **Performance**        | ⭐⭐⭐⭐⭐    | ⭐⭐⭐       | ⭐         |
| **Análisis Real-Time** | ⭐⭐⭐⭐⭐    | ⭐⭐⭐       | ⭐         |
| **Escalabilidad**      | ⭐⭐⭐⭐⭐    | ⭐⭐⭐       | ⭐⭐       |
| **Costo**              | ⭐⭐⭐        | ⭐⭐⭐⭐     | ⭐⭐⭐⭐⭐ |

---

## 🎨 Casos de Uso en OKLA

### 1. Indexación de Logs de Error (RoleService)

**Ubicación:** `backend/RoleService/RoleService.Infrastructure/External/ElasticSearchService.cs`

```
ErrorService (captura errores)
         ↓
RoleService (procesa y valida)
         ↓
ElasticSearchService (indexa en Elasticsearch)
         ↓
Kibana Dashboard (visualización)
```

**Funcionalidades:**

- ✅ Indexar logs de error con metadata completa
- ✅ Incluir stack traces, user context, request details
- ✅ Timestamp UTC para correlación temporal
- ✅ Tags y categorización automática

### 2. Búsqueda Avanzada de Errores

**Escenarios:**

- 🔍 Buscar errores por mensaje: `"NullReferenceException"`
- 🔍 Filtrar por servicio: `service:"RoleService"`
- 🔍 Errores en rango temporal: `timestamp:[now-1h TO now]`
- 🔍 Búsqueda fuzzy: `mesage~` (typo tolerance)

### 3. Análisis y Métricas

**Agregaciones:**

- 📊 Top 10 errores más frecuentes
- 📊 Errores por hora/día/semana
- 📊 Distribución por severidad (Error, Warning, Info)
- 📊 Errores por usuario/endpoint

### 4. Troubleshooting en Producción

**Workflow:**

1. Usuario reporta error → ID de correlación
2. Admin busca en Kibana: `correlationId:"abc-123"`
3. Ve contexto completo: request, user, estado
4. Identifica causa raíz y soluciona

---

## ⚙️ Configuración e Instalación

### Paso 1: Instalar Elasticsearch en DOKS

#### Opción A: Helm Chart (Recomendado)

```bash
# Agregar repositorio de Elastic
helm repo add elastic https://helm.elastic.co
helm repo update

# Crear namespace
kubectl create namespace elastic-system

# Instalar Elasticsearch
helm install elasticsearch elastic/elasticsearch \
  --namespace elastic-system \
  --set replicas=3 \
  --set minimumMasterNodes=2 \
  --set resources.requests.memory="2Gi" \
  --set resources.requests.cpu="1000m" \
  --set persistence.enabled=true \
  --set volumeClaimTemplate.resources.requests.storage="30Gi"

# Verificar instalación
kubectl get pods -n elastic-system
```

#### Opción B: Managed Service (DigitalOcean Marketplace)

```bash
# Opción más simple pero con costo adicional
# Deploy desde DigitalOcean Marketplace
# https://marketplace.digitalocean.com/apps/elasticsearch
```

### Paso 2: Instalar Paquete NuGet

```bash
cd backend/RoleService/RoleService.Infrastructure
dotnet add package Elastic.Clients.Elasticsearch --version 8.11.0
```

**Packages requeridos:**

```xml
<PackageReference Include="Elastic.Clients.Elasticsearch" Version="8.11.0" />
<PackageReference Include="Elastic.Transport" Version="0.4.16" />
```

### Paso 3: Configurar appsettings.json

**Development (`appsettings.Development.json`):**

```json
{
  "ElasticSearch": {
    "Enabled": true,
    "ConnectionString": "http://localhost:9200",
    "Username": "elastic",
    "Password": "changeme",
    "DefaultIndex": "okla-errors-dev",
    "CertificateFingerprint": null,
    "RequestTimeout": "00:01:00"
  }
}
```

**Production (`appsettings.json`):**

```json
{
  "ElasticSearch": {
    "Enabled": true,
    "ConnectionString": "https://elasticsearch.elastic-system.svc.cluster.local:9200",
    "Username": "${ELASTICSEARCH_USERNAME}",
    "Password": "${ELASTICSEARCH_PASSWORD}",
    "DefaultIndex": "okla-errors-prod",
    "CertificateFingerprint": "${ELASTICSEARCH_CERT_FINGERPRINT}",
    "RequestTimeout": "00:01:00"
  }
}
```

**Kubernetes Secret:**

```bash
# Crear secret para credenciales
kubectl create secret generic elasticsearch-credentials \
  --from-literal=username=elastic \
  --from-literal=password='your-secure-password' \
  --from-literal=cert-fingerprint='AA:BB:CC:DD...' \
  -n okla
```

### Paso 4: Variables de Entorno en Deployment

```yaml
# k8s/deployments.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: roleservice
  namespace: okla
spec:
  template:
    spec:
      containers:
        - name: roleservice
          image: ghcr.io/gregorymorenoiem/cardealer-roleservice:latest
          env:
            - name: ELASTICSEARCH_USERNAME
              valueFrom:
                secretKeyRef:
                  name: elasticsearch-credentials
                  key: username
            - name: ELASTICSEARCH_PASSWORD
              valueFrom:
                secretKeyRef:
                  name: elasticsearch-credentials
                  key: password
            - name: ELASTICSEARCH_CERT_FINGERPRINT
              valueFrom:
                secretKeyRef:
                  name: elasticsearch-credentials
                  key: cert-fingerprint
```

---

## 🔐 Autenticación

### Métodos Soportados

#### 1. Basic Authentication (Desarrollo)

```csharp
var settings = new ElasticsearchClientSettings(new Uri(connectionString))
    .Authentication(new BasicAuthentication(username, password));

var client = new ElasticsearchClient(settings);
```

#### 2. API Key (Producción - Recomendado)

```csharp
var settings = new ElasticsearchClientSettings(new Uri(connectionString))
    .Authentication(new ApiKey("your-api-key-id", "your-api-key"));

var client = new ElasticsearchClient(settings);
```

**Crear API Key en Elasticsearch:**

```bash
# Conectar al pod de Elasticsearch
kubectl exec -it elasticsearch-0 -n elastic-system -- bash

# Crear API key
curl -X POST "localhost:9200/_security/api_key" \
  -H "Content-Type: application/json" \
  -u elastic:password \
  -d '{
    "name": "okla-roleservice",
    "role_descriptors": {
      "role-a": {
        "cluster": ["all"],
        "index": [
          {
            "names": ["okla-errors-*"],
            "privileges": ["all"]
          }
        ]
      }
    }
  }'
```

#### 3. Certificate-Based (TLS)

```csharp
var settings = new ElasticsearchClientSettings(new Uri(connectionString))
    .CertificateFingerprint(certificateFingerprint)
    .Authentication(new BasicAuthentication(username, password));

var client = new ElasticsearchClient(settings);
```

---

## 🏗️ Arquitectura de Integración

### Flujo Completo de Error Logging

```
┌─────────────────────────────────────────────────────────────────────┐
│                    OKLA Microservices                                │
├─────────────────────────────────────────────────────────────────────┤
│                                                                       │
│  1️⃣ Error Occurrence                                                 │
│  ├─> VehiclesSaleService throws exception                           │
│  ├─> Global Exception Handler catches                               │
│  └─> Publishes to RabbitMQ: ErrorOccurred event                     │
│                                                                       │
│  2️⃣ Error Processing (ErrorService)                                  │
│  ├─> Consumes ErrorOccurred from RabbitMQ                           │
│  ├─> Saves to PostgreSQL (errors table)                             │
│  ├─> Publishes ErrorProcessed event                                 │
│  └─> Returns Error ID                                                │
│                                                                       │
│  3️⃣ Error Indexing (RoleService)                                     │
│  ├─> Consumes ErrorProcessed event                                   │
│  ├─> Enriches with additional metadata                               │
│  ├─> ElasticSearchService.IndexErrorAsync()                          │
│  │   ├─> Serializes to JSON                                          │
│  │   ├─> POST to Elasticsearch: /okla-errors-prod/_doc/{id}         │
│  │   └─> Verifies indexing success                                   │
│  └─> Log: "Successfully indexed error {ErrorId}"                     │
│                                                                       │
│  4️⃣ Search & Analysis                                                │
│  ├─> Admin accesses Kibana dashboard                                 │
│  ├─> Searches: service:"VehiclesSaleService" AND level:"Error"      │
│  ├─> Views aggregations: errors per hour                             │
│  └─> Drills down to specific error for debugging                    │
│                                                                       │
└─────────────────────────────────────────────────────────────────────┘
```

### Componentes Clave

**ElasticSearchService.cs:**

```csharp
public class ElasticSearchService
{
    private readonly ElasticsearchClient? _client;
    private readonly ElasticSearchSettings _settings;
    private readonly ILogger<ElasticSearchService> _logger;

    // Constructor
    // IndexErrorAsync() - Indexar error
    // SearchAsync() - Buscar errores
    // GetByIdAsync() - Obtener error específico
}
```

**ElasticSearchSettings.cs:**

```csharp
public class ElasticSearchSettings
{
    public bool Enabled { get; set; }
    public string ConnectionString { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string DefaultIndex { get; set; } = "okla-errors";
    public string? CertificateFingerprint { get; set; }
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromMinutes(1);
}
```

---

## 📊 Esquema de Índices

### Índice: `okla-errors-{env}`

**Naming Convention:**

- Development: `okla-errors-dev`
- Staging: `okla-errors-staging`
- Production: `okla-errors-prod`

### Mapping (Esquema)

```json
{
  "mappings": {
    "properties": {
      "id": { "type": "keyword" },
      "timestamp": { "type": "date" },
      "level": { "type": "keyword" },
      "message": {
        "type": "text",
        "fields": {
          "keyword": { "type": "keyword" }
        }
      },
      "exception": {
        "properties": {
          "type": { "type": "keyword" },
          "message": { "type": "text" },
          "stackTrace": { "type": "text" },
          "innerException": { "type": "text" }
        }
      },
      "context": {
        "properties": {
          "service": { "type": "keyword" },
          "environment": { "type": "keyword" },
          "userId": { "type": "keyword" },
          "correlationId": { "type": "keyword" },
          "endpoint": { "type": "keyword" },
          "method": { "type": "keyword" },
          "statusCode": { "type": "integer" }
        }
      },
      "request": {
        "properties": {
          "path": { "type": "keyword" },
          "query": { "type": "text" },
          "headers": { "type": "object", "enabled": false },
          "body": { "type": "text" }
        }
      },
      "tags": { "type": "keyword" },
      "metadata": { "type": "object", "enabled": false }
    }
  }
}
```

### Crear Índice con Mapping

```bash
# Conectar a Elasticsearch pod
kubectl exec -it elasticsearch-0 -n elastic-system -- bash

# Crear índice
curl -X PUT "localhost:9200/okla-errors-prod" \
  -H "Content-Type: application/json" \
  -u elastic:password \
  -d @- <<'EOF'
{
  "settings": {
    "number_of_shards": 3,
    "number_of_replicas": 2,
    "index": {
      "lifecycle": {
        "name": "okla-errors-policy",
        "rollover_alias": "okla-errors"
      }
    }
  },
  "mappings": {
    // ... (mapping completo arriba)
  }
}
EOF
```

### Index Lifecycle Management (ILM)

```json
{
  "policy": {
    "phases": {
      "hot": {
        "actions": {
          "rollover": {
            "max_size": "50gb",
            "max_age": "30d"
          }
        }
      },
      "warm": {
        "min_age": "30d",
        "actions": {
          "shrink": {
            "number_of_shards": 1
          },
          "forcemerge": {
            "max_num_segments": 1
          }
        }
      },
      "delete": {
        "min_age": "90d",
        "actions": {
          "delete": {}
        }
      }
    }
  }
}
```

---

## 🔧 Operaciones Principales

### 1. Indexar Error (Create)

**Endpoint:** `POST /{index}/_doc/{id}`

**Request:**

```http
POST /okla-errors-prod/_doc/550e8400-e29b-41d4-a716-446655440000
Content-Type: application/json

{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "timestamp": "2026-01-15T14:30:00Z",
  "level": "Error",
  "message": "Failed to update vehicle price",
  "exception": {
    "type": "ArgumentNullException",
    "message": "Value cannot be null. (Parameter 'price')",
    "stackTrace": "at VehiclesSaleService.Application...",
    "innerException": null
  },
  "context": {
    "service": "VehiclesSaleService",
    "environment": "Production",
    "userId": "user-123",
    "correlationId": "abc-def-123",
    "endpoint": "/api/vehicles/123",
    "method": "PUT",
    "statusCode": 500
  },
  "tags": ["vehicles", "pricing", "critical"]
}
```

**Response:**

```json
{
  "_index": "okla-errors-prod",
  "_id": "550e8400-e29b-41d4-a716-446655440000",
  "_version": 1,
  "result": "created",
  "_shards": {
    "total": 2,
    "successful": 2,
    "failed": 0
  }
}
```

### 2. Buscar Errores (Search)

**Endpoint:** `GET /{index}/_search`

**Simple Search:**

```http
GET /okla-errors-prod/_search
Content-Type: application/json

{
  "query": {
    "match": {
      "message": "NullReferenceException"
    }
  },
  "size": 20,
  "sort": [
    { "timestamp": "desc" }
  ]
}
```

**Advanced Search con Filtros:**

```http
GET /okla-errors-prod/_search
Content-Type: application/json

{
  "query": {
    "bool": {
      "must": [
        { "match": { "context.service": "VehiclesSaleService" }},
        { "term": { "level": "Error" }}
      ],
      "filter": [
        { "range": {
          "timestamp": {
            "gte": "now-1h",
            "lte": "now"
          }
        }}
      ]
    }
  },
  "aggs": {
    "errors_per_endpoint": {
      "terms": {
        "field": "context.endpoint",
        "size": 10
      }
    }
  }
}
```

### 3. Obtener Error por ID (Get)

**Endpoint:** `GET /{index}/_doc/{id}`

```http
GET /okla-errors-prod/_doc/550e8400-e29b-41d4-a716-446655440000
```

**Response:**

```json
{
  "_index": "okla-errors-prod",
  "_id": "550e8400-e29b-41d4-a716-446655440000",
  "_version": 1,
  "_seq_no": 12345,
  "_primary_term": 1,
  "found": true,
  "_source": {
    // ... documento completo
  }
}
```

### 4. Agregaciones (Análisis)

**Top 10 Errores Más Frecuentes:**

```http
GET /okla-errors-prod/_search
Content-Type: application/json

{
  "size": 0,
  "aggs": {
    "top_errors": {
      "terms": {
        "field": "exception.type",
        "size": 10
      },
      "aggs": {
        "recent_example": {
          "top_hits": {
            "size": 1,
            "sort": [{ "timestamp": "desc" }],
            "_source": ["message", "timestamp"]
          }
        }
      }
    }
  }
}
```

**Errores por Hora (Timeline):**

```http
GET /okla-errors-prod/_search
Content-Type: application/json

{
  "size": 0,
  "aggs": {
    "errors_over_time": {
      "date_histogram": {
        "field": "timestamp",
        "fixed_interval": "1h"
      }
    }
  }
}
```

---

## 💻 Ejemplos de Código

### ElasticSearchService Completo

**`ElasticSearchService.cs`:**

```csharp
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Elastic.Transport;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace RoleService.Infrastructure.External;

public class ElasticSearchSettings
{
    public bool Enabled { get; set; } = false;
    public string ConnectionString { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string DefaultIndex { get; set; } = "okla-errors";
    public string? CertificateFingerprint { get; set; }
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromMinutes(1);
}

public class ElasticSearchService
{
    private readonly ElasticsearchClient? _client;
    private readonly ILogger<ElasticSearchService> _logger;
    private readonly ElasticSearchSettings _settings;

    public ElasticSearchService(
        IOptions<ElasticSearchSettings> settings,
        ILogger<ElasticSearchService> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        if (!_settings.Enabled)
        {
            _logger.LogInformation("Elasticsearch is disabled.");
            return;
        }

        try
        {
            var config = new ElasticsearchClientSettings(new Uri(_settings.ConnectionString))
                .DefaultIndex(_settings.DefaultIndex)
                .RequestTimeout(_settings.RequestTimeout);

            // Authentication
            if (!string.IsNullOrEmpty(_settings.Username) && !string.IsNullOrEmpty(_settings.Password))
            {
                config = config.Authentication(new BasicAuthentication(_settings.Username, _settings.Password));
            }

            // Certificate fingerprint (production)
            if (!string.IsNullOrEmpty(_settings.CertificateFingerprint))
            {
                config = config.CertificateFingerprint(_settings.CertificateFingerprint);
            }

            _client = new ElasticsearchClient(config);
            _logger.LogInformation("Elasticsearch client initialized successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Elasticsearch client");
        }
    }

    public async Task<bool> IndexErrorAsync<T>(T document, string? id = null) where T : class
    {
        if (_client == null)
        {
            _logger.LogDebug("Elasticsearch is disabled, skipping error indexing.");
            return false;
        }

        try
        {
            var response = id != null
                ? await _client.IndexAsync(document, idx => idx.Id(id))
                : await _client.IndexAsync(document);

            if (!response.IsValidResponse)
            {
                _logger.LogWarning(
                    "Failed to index document in Elasticsearch: {DebugInfo}",
                    response.DebugInformation);
                return false;
            }

            _logger.LogInformation(
                "Successfully indexed document {Id} in Elasticsearch",
                response.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error indexing document in Elasticsearch");
            return false;
        }
    }

    public async Task<IEnumerable<T>> SearchAsync<T>(string query, int size = 20) where T : class
    {
        if (_client == null)
        {
            _logger.LogDebug("Elasticsearch is disabled, returning empty search results.");
            return Enumerable.Empty<T>();
        }

        try
        {
            var response = await _client.SearchAsync<T>(s => s
                .Query(q => q
                    .MultiMatch(m => m
                        .Query(query)
                        .Fields(new[] { "message^2", "exception.message", "tags" })
                        .Fuzziness(new Fuzziness("AUTO"))
                    )
                )
                .Size(size)
                .Sort(sort => sort.Field("timestamp", new FieldSort { Order = SortOrder.Desc }))
            );

            if (!response.IsValidResponse)
            {
                _logger.LogWarning(
                    "Elasticsearch search failed: {DebugInfo}",
                    response.DebugInformation);
                return Enumerable.Empty<T>();
            }

            return response.Documents;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching documents in Elasticsearch");
            return Enumerable.Empty<T>();
        }
    }

    public async Task<T?> GetByIdAsync<T>(string id) where T : class
    {
        if (_client == null)
        {
            _logger.LogDebug("Elasticsearch is disabled.");
            return null;
        }

        try
        {
            var response = await _client.GetAsync<T>(id);

            if (!response.IsValidResponse || !response.Found)
            {
                _logger.LogDebug("Document {Id} not found in Elasticsearch", id);
                return null;
            }

            return response.Source;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting document {Id} from Elasticsearch", id);
            return null;
        }
    }

    public async Task<Dictionary<string, long>> GetErrorCountsByServiceAsync(DateTime from, DateTime to)
    {
        if (_client == null)
        {
            return new Dictionary<string, long>();
        }

        try
        {
            var response = await _client.SearchAsync<object>(s => s
                .Query(q => q
                    .DateRange(r => r
                        .Field("timestamp")
                        .Gte(from)
                        .Lte(to)
                    )
                )
                .Size(0)
                .Aggregations(a => a
                    .Terms("services", t => t
                        .Field("context.service")
                        .Size(50)
                    )
                )
            );

            if (!response.IsValidResponse)
            {
                _logger.LogWarning("Failed to get aggregations from Elasticsearch");
                return new Dictionary<string, long>();
            }

            var termsAgg = response.Aggregations?.GetTerms("services");
            if (termsAgg == null)
            {
                return new Dictionary<string, long>();
            }

            return termsAgg.Buckets
                .ToDictionary(
                    b => b.Key.ToString() ?? "Unknown",
                    b => b.DocCount ?? 0
                );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting error counts by service from Elasticsearch");
            return new Dictionary<string, long>();
        }
    }

    public async Task<bool> IsHealthyAsync()
    {
        if (_client == null)
        {
            return false;
        }

        try
        {
            var response = await _client.PingAsync();
            return response.IsValidResponse;
        }
        catch
        {
            return false;
        }
    }
}
```

### Registro en Program.cs

```csharp
// Program.cs
using RoleService.Infrastructure.External;

var builder = WebApplication.CreateBuilder(args);

// Configurar ElasticSearch
builder.Services.Configure<ElasticSearchSettings>(
    builder.Configuration.GetSection("ElasticSearch"));

builder.Services.AddSingleton<ElasticSearchService>();

var app = builder.Build();

// Health check de Elasticsearch
app.MapGet("/health/elasticsearch", async (ElasticSearchService elasticSearch) =>
{
    var isHealthy = await elasticSearch.IsHealthyAsync();
    return isHealthy
        ? Results.Ok(new { status = "healthy" })
        : Results.ServiceUnavailable();
});

app.Run();
```

### Uso en Controller

```csharp
using Microsoft.AspNetCore.Mvc;
using RoleService.Infrastructure.External;

namespace RoleService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ErrorLogsController : ControllerBase
{
    private readonly ElasticSearchService _elasticSearch;
    private readonly ILogger<ErrorLogsController> _logger;

    public ErrorLogsController(
        ElasticSearchService elasticSearch,
        ILogger<ErrorLogsController> logger)
    {
        _elasticSearch = elasticSearch;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> IndexError([FromBody] ErrorLogDto error)
    {
        var success = await _elasticSearch.IndexErrorAsync(error, error.Id.ToString());

        if (!success)
        {
            return StatusCode(500, "Failed to index error in Elasticsearch");
        }

        return Ok(new { indexed = true, id = error.Id });
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string query, [FromQuery] int size = 20)
    {
        var results = await _elasticSearch.SearchAsync<ErrorLogDto>(query, size);
        return Ok(results);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var error = await _elasticSearch.GetByIdAsync<ErrorLogDto>(id);

        if (error == null)
        {
            return NotFound();
        }

        return Ok(error);
    }

    [HttpGet("stats/by-service")]
    public async Task<IActionResult> GetErrorCountsByService(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to)
    {
        var stats = await _elasticSearch.GetErrorCountsByServiceAsync(from, to);
        return Ok(stats);
    }
}
```

### DTO Example

```csharp
namespace RoleService.Application.DTOs;

public class ErrorLogDto
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string Level { get; set; } = "Error";
    public string Message { get; set; } = string.Empty;
    public ExceptionDto? Exception { get; set; }
    public ContextDto Context { get; set; } = new();
    public RequestDto? Request { get; set; }
    public List<string> Tags { get; set; } = new();
    public Dictionary<string, object>? Metadata { get; set; }
}

public class ExceptionDto
{
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public string? InnerException { get; set; }
}

public class ContextDto
{
    public string Service { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string? CorrelationId { get; set; }
    public string? Endpoint { get; set; }
    public string? Method { get; set; }
    public int? StatusCode { get; set; }
}

public class RequestDto
{
    public string Path { get; set; } = string.Empty;
    public string? Query { get; set; }
    public Dictionary<string, string>? Headers { get; set; }
    public string? Body { get; set; }
}
```

---

## 🏥 Monitoreo y Salud

### Health Endpoint

**GET `/health/elasticsearch`**

```csharp
app.MapGet("/health/elasticsearch", async (ElasticSearchService elasticSearch) =>
{
    try
    {
        var isHealthy = await elasticSearch.IsHealthyAsync();

        if (!isHealthy)
        {
            return Results.Json(
                new { status = "unhealthy", message = "Elasticsearch ping failed" },
                statusCode: 503
            );
        }

        return Results.Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            elasticsearch = new
            {
                connected = true,
                version = "8.x"
            }
        });
    }
    catch (Exception ex)
    {
        return Results.Json(
            new { status = "unhealthy", error = ex.Message },
            statusCode: 503
        );
    }
});
```

### Métricas Clave

| Métrica            | Descripción            | Umbral     |
| ------------------ | ---------------------- | ---------- |
| **Indexing Rate**  | Docs/segundo indexados | > 100/s OK |
| **Search Latency** | Tiempo de respuesta    | < 100ms OK |
| **Disk Usage**     | Espacio usado          | < 80% OK   |
| **Heap Usage**     | Memoria JVM            | < 75% OK   |
| **CPU Usage**      | Uso de CPU             | < 70% OK   |

### Kibana Dashboards

**Acceso:**

```bash
# Port-forward Kibana
kubectl port-forward svc/kibana-kibana 5601:5601 -n elastic-system

# Abrir en navegador
open http://localhost:5601
```

**Dashboards Recomendados:**

1. **OKLA Errors Overview** - Resumen de errores
2. **Error Trends** - Gráficos de tendencias
3. **Service Health** - Salud por servicio
4. **User Impact** - Errores por usuario

---

## ⚠️ Manejo de Errores

### Estrategias de Resiliencia

#### 1. Graceful Degradation

```csharp
public async Task<bool> IndexErrorAsync<T>(T document) where T : class
{
    if (_client == null)
    {
        _logger.LogDebug("Elasticsearch disabled, skipping indexing");
        return false; // No throw, continúa ejecución
    }

    try
    {
        var response = await _client.IndexAsync(document);
        return response.IsValidResponse;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to index to Elasticsearch");
        // No throw - logging error no debe romper app
        return false;
    }
}
```

#### 2. Circuit Breaker

```csharp
// Implementar con Polly
var circuitBreakerPolicy = Policy
    .HandleResult<bool>(r => !r)
    .CircuitBreakerAsync(
        handledEventsAllowedBeforeBreaking: 3,
        durationOfBreak: TimeSpan.FromMinutes(1),
        onBreak: (result, duration) =>
        {
            _logger.LogWarning("Circuit breaker opened for {Duration}", duration);
        },
        onReset: () =>
        {
            _logger.LogInformation("Circuit breaker reset");
        }
    );
```

#### 3. Retry con Backoff

```csharp
var retryPolicy = Policy
    .Handle<ElasticsearchClientException>()
    .WaitAndRetryAsync(
        retryCount: 3,
        sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
        onRetry: (exception, timeSpan, retryCount, context) =>
        {
            _logger.LogWarning(
                "Elasticsearch retry {RetryCount} after {Delay}",
                retryCount,
                timeSpan
            );
        }
    );
```

### Códigos de Error Comunes

| Status  | Error               | Causa                  | Solución                    |
| ------- | ------------------- | ---------------------- | --------------------------- |
| **401** | Unauthorized        | Credenciales inválidas | Verificar username/password |
| **403** | Forbidden           | Sin permisos           | Revisar API key permisos    |
| **404** | Not Found           | Índice no existe       | Crear índice primero        |
| **429** | Too Many Requests   | Rate limit             | Implementar backoff         |
| **503** | Service Unavailable | Elasticsearch down     | Verificar cluster health    |

---

## 🎯 Mejores Prácticas

### 1. Indexación

✅ **DO:**

- Usar IDs únicos y predecibles (UUID)
- Incluir timestamp UTC en todos los documentos
- Agregar tags para categorización
- Usar bulk indexing para volumen alto

❌ **DON'T:**

- No indexar información sensible (passwords, tokens)
- No crear índices sin mapping
- No usar campos dinámicos sin control

### 2. Búsquedas

✅ **DO:**

- Usar paginación (`from`, `size`)
- Agregar filtros de rango temporal
- Limitar fields retornados con `_source`
- Usar query cache cuando sea posible

❌ **DON'T:**

- No hacer deep pagination (> 10,000 docs)
- No buscar sin filtros temporales
- No hacer wildcard queries sin prefijo

### 3. Performance

✅ **DO:**

- Configurar ILM para rotación de índices
- Usar replicas para redundancia
- Monitorear heap y disk usage
- Implementar warming queries

❌ **DON'T:**

- No almacenar documentos > 100MB
- No sobrecargar un solo shard
- No ignorar cluster health warnings

### 4. Seguridad

✅ **DO:**

- Usar API keys en producción
- Habilitar TLS/SSL
- Rotar credenciales regularmente
- Limitar permisos por servicio

❌ **DON'T:**

- No usar basic auth en prod
- No exponer Elasticsearch públicamente
- No compartir credenciales entre servicios

---

## 🔧 Troubleshooting

### Problema: Elasticsearch Connection Refused

**Síntomas:**

```
Failed to initialize Elasticsearch client
System.Net.Http.HttpRequestException: Connection refused
```

**Solución:**

```bash
# 1. Verificar que Elasticsearch está corriendo
kubectl get pods -n elastic-system

# 2. Verificar servicio
kubectl get svc elasticsearch-master -n elastic-system

# 3. Port-forward para debugging
kubectl port-forward svc/elasticsearch-master 9200:9200 -n elastic-system

# 4. Test manual
curl http://localhost:9200
```

### Problema: Authentication Failed (401)

**Síntomas:**

```
Elasticsearch search failed: 401 Unauthorized
```

**Solución:**

```bash
# 1. Verificar credenciales en secret
kubectl get secret elasticsearch-credentials -n okla -o yaml

# 2. Decodificar password
echo "base64-encoded-password" | base64 -d

# 3. Test credenciales
curl -u elastic:password http://localhost:9200

# 4. Regenerar API key si es necesario
```

### Problema: Index Not Found (404)

**Síntomas:**

```
index_not_found_exception: no such index [okla-errors-prod]
```

**Solución:**

```bash
# Crear índice manualmente
kubectl exec -it elasticsearch-0 -n elastic-system -- bash

curl -X PUT "localhost:9200/okla-errors-prod" \
  -H "Content-Type: application/json" \
  -u elastic:password \
  -d '{
    "settings": {
      "number_of_shards": 3,
      "number_of_replicas": 2
    }
  }'
```

### Problema: Too Many Requests (429)

**Síntomas:**

```
circuit_breaking_exception: Data too large
```

**Solución:**

```bash
# 1. Verificar heap usage
curl -u elastic:password "http://localhost:9200/_cat/nodes?v&h=name,heap.percent"

# 2. Aumentar heap si necesario (max 50% de RAM)
# En k8s/deployments.yaml:
resources:
  limits:
    memory: "4Gi"
  requests:
    memory: "2Gi"
env:
  - name: ES_JAVA_OPTS
    value: "-Xms2g -Xmx2g"

# 3. Implementar rate limiting en app
```

### Problema: Slow Queries

**Síntomas:**

```
Search taking > 5 seconds
```

**Solución:**

```bash
# 1. Ver slow logs
kubectl logs elasticsearch-0 -n elastic-system | grep "took"

# 2. Analizar query performance
curl -u elastic:password "http://localhost:9200/okla-errors-prod/_search?explain=true" \
  -H "Content-Type: application/json" \
  -d '{query...}'

# 3. Agregar índices a campos frecuentes
# 4. Limitar resultados con filtros
# 5. Usar query cache
```

---

## 💰 Costos y Límites

### Costos en DigitalOcean Kubernetes

| Configuración         | Recursos          | Costo Mensual | Uso Recomendado |
| --------------------- | ----------------- | ------------- | --------------- |
| **Dev/Test**          | 1 node, 2GB RAM   | ~$12/mes      | Desarrollo      |
| **Small**             | 3 nodes, 4GB RAM  | ~$72/mes      | Staging         |
| **Production**        | 3 nodes, 8GB RAM  | ~$144/mes     | Producción      |
| **High Availability** | 5 nodes, 16GB RAM | ~$360/mes     | Alta carga      |

### Límites por Plan

| Límite            | Dev    | Small   | Production | HA     |
| ----------------- | ------ | ------- | ---------- | ------ |
| **Docs/Index**    | 1M     | 10M     | 100M       | 1B+    |
| **Indexing Rate** | 100/s  | 1K/s    | 10K/s      | 100K/s |
| **Search QPS**    | 10     | 100     | 1K         | 10K    |
| **Storage**       | 20GB   | 100GB   | 500GB      | 2TB+   |
| **Retention**     | 7 días | 30 días | 90 días    | 1 año  |

### Optimización de Costos

✅ **Reducir costos:**

- Implementar ILM para eliminar logs antiguos
- Usar warm/cold tiers para datos históricos
- Comprimir índices con force merge
- Ajustar replicas según necesidad

---

## 📚 Recursos Adicionales

### Documentación Oficial

- **Elasticsearch Docs:** https://www.elastic.co/guide/en/elasticsearch/reference/current/index.html
- **.NET Client:** https://www.elastic.co/guide/en/elasticsearch/client/net-api/current/index.html
- **Kibana Guide:** https://www.elastic.co/guide/en/kibana/current/index.html

### Tutoriales

- **Getting Started:** https://www.elastic.co/guide/en/elasticsearch/reference/current/getting-started.html
- **Query DSL:** https://www.elastic.co/guide/en/elasticsearch/reference/current/query-dsl.html
- **Aggregations:** https://www.elastic.co/guide/en/elasticsearch/reference/current/search-aggregations.html

### Herramientas

- **Elasticsearch Head:** GUI para administración
- **Dejavu:** Web UI para Elasticsearch
- **Cerebro:** Cluster monitoring tool

### Comunidad

- **Forum:** https://discuss.elastic.co/
- **Stack Overflow:** Tag `elasticsearch`
- **GitHub:** https://github.com/elastic/elasticsearch

---

## 📝 Changelog

| Versión   | Fecha          | Cambios                        |
| --------- | -------------- | ------------------------------ |
| **1.0.0** | Enero 15, 2026 | Documentación inicial completa |

---

## 👥 Contacto y Soporte

**Equipo de Desarrollo OKLA:**

- **Email:** dev@okla.com.do
- **Slack:** #elasticsearch-support
- **GitHub:** gregorymorenoiem/cardealer-microservices

**Elastic Support:**

- **Community:** https://discuss.elastic.co/
- **Commercial:** https://www.elastic.co/support

---

**✅ Documentación completada - Ready for Production**

_Esta documentación es parte del proyecto OKLA Marketplace. Para más información sobre otras APIs, consulta [API_MASTER_INDEX.md](../API_MASTER_INDEX.md)._
