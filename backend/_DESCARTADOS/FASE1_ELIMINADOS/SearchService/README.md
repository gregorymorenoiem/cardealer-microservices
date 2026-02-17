# 🔍 SearchService - Full-Text Search with Elasticsearch

**Estado**: ✅ Producción Ready  
**Versión**: 1.0.0  
**Puerto**: 15093  
**Stack**: ASP.NET Core 8.0, Elasticsearch 8.11, NEST 7.17, MediatR

---

## 📋 Descripción

SearchService es un servicio de búsqueda de texto completo basado en Elasticsearch que proporciona capacidades avanzadas de búsqueda para toda la plataforma CarDealer. Implementa Clean Architecture con 4 capas bien separadas.

### Características Principales

- ✅ **Full-Text Search** - Búsqueda de texto completo con análisis y tokenización
- ✅ **Fuzzy Search** - Búsqueda tolerante a errores de escritura
- ✅ **Exact Match** - Coincidencias exactas de términos
- ✅ **Wildcard Search** - Búsqueda con comodines (* y ?)
- ✅ **Prefix Search** - Autocompletado basado en prefijos
- ✅ **Highlighting** - Resaltado de resultados en fragmentos de texto
- ✅ **Pagination** - Paginación eficiente de resultados
- ✅ **Sorting** - Ordenamiento por relevancia o campos específicos
- ✅ **Bulk Indexing** - Indexación masiva de documentos
- ✅ **Index Management** - Creación, eliminación y gestión de índices
- ✅ **Statistics** - Métricas y estadísticas de índices

---

## 🏗️ Arquitectura

```
SearchService/
├── SearchService.Domain/         # Entidades, enums, interfaces
│   ├── Entities/                 # SearchDocument, IndexMetadata
│   ├── ValueObjects/             # SearchQuery, SearchResult
│   ├── Enums/                    # SearchType, SortOrder, IndexStatus
│   └── Interfaces/               # ISearchRepository, IIndexManager
├── SearchService.Application/    # Lógica de negocio (CQRS)
│   ├── Commands/                 # IndexDocument, DeleteDocument, BulkIndex
│   ├── Queries/                  # ExecuteSearch, GetDocument, GetIndexMetadata
│   └── Handlers/                 # Command & Query handlers con MediatR
├── SearchService.Infrastructure/ # Implementación Elasticsearch
│   ├── Repositories/             # ElasticsearchRepository (NEST)
│   ├── Services/                 # ElasticsearchIndexManager
│   └── Configuration/            # ElasticsearchOptions
└── SearchService.Api/            # REST API
    └── Controllers/              # SearchController, IndexController, StatsController
```

---

## 🚀 Endpoints API

### 1. **SearchController**

#### `POST /api/search/query`
Ejecuta una búsqueda en Elasticsearch.

**Request Body**:
```json
{
  "queryText": "toyota camry 2023",
  "indexName": "vehicles",
  "searchType": 0,
  "fields": ["brand", "model", "year", "description"],
  "filters": {
    "status": "active",
    "category": "sedan"
  },
  "page": 1,
  "pageSize": 10,
  "sortBy": "_score",
  "sortOrder": 1,
  "enableHighlighting": true,
  "fuzziness": "AUTO",
  "minScore": 0.5
}
```

**SearchType Enum**:
- `0` = FullText
- `1` = Fuzzy
- `2` = Exact
- `3` = Wildcard
- `4` = Prefix

**Response**:
```json
{
  "totalCount": 42,
  "documents": [
    {
      "id": "doc123",
      "indexName": "vehicles",
      "content": "{...}",
      "score": 4.2,
      "highlights": {
        "description": ["<mark>toyota</mark> <mark>camry</mark>"]
      }
    }
  ],
  "currentPage": 1,
  "pageSize": 10,
  "executionTimeMs": 125,
  "timedOut": false,
  "maxScore": 4.5
}
```

#### `GET /api/search/{indexName}/{documentId}`
Obtiene un documento específico por ID.

**Response**: `SearchDocument`

#### `GET /api/search/indices`
Lista todos los índices disponibles.

**Response**: `["vehicles", "users", "contacts"]`

---

### 2. **IndexController**

#### `POST /api/index/{indexName}/document`
Indexa un nuevo documento (genera ID automático).

**Request Body**: Cualquier JSON válido
```json
{
  "brand": "Toyota",
  "model": "Camry",
  "year": 2023,
  "price": 28000,
  "description": "Reliable sedan with excellent fuel economy"
}
```

**Response**:
```json
{
  "id": "generated-uuid",
  "indexName": "vehicles"
}
```

#### `PUT /api/index/{indexName}/document/{documentId}`
Indexa un documento con ID específico.

#### `PATCH /api/index/{indexName}/document/{documentId}`
Actualiza un documento existente.

#### `DELETE /api/index/{indexName}/document/{documentId}`
Elimina un documento del índice.

#### `POST /api/index/{indexName}/bulk`
Indexa múltiples documentos en batch.

**Request Body**:
```json
[
  {
    "id": "doc1",
    "document": { "brand": "Toyota", "model": "Camry" }
  },
  {
    "id": "doc2",
    "document": { "brand": "Honda", "model": "Accord" }
  }
]
```

**Response**:
```json
{
  "indexName": "vehicles",
  "successful": 2,
  "failed": 0,
  "total": 2
}
```

#### `POST /api/index/{indexName}`
Crea un nuevo índice con configuración opcional.

**Request Body** (opcional):
```json
{
  "mappings": {},
  "settings": {
    "number_of_shards": 1,
    "number_of_replicas": 1
  }
}
```

#### `POST /api/index/initialize/properties`
Inicializa el índice de **propiedades inmobiliarias** con mappings optimizados.

Este endpoint crea el índice `properties` con:
- Campos de texto con analyzer español para título, descripción, dirección
- Campos numéricos para precio, área, recámaras, baños
- Campos booleanos para amenidades (alberca, jardín, gimnasio, etc.)
- Campo `geo_point` para búsqueda por ubicación geográfica
- Multi-tenant con `dealerId`

**Response**:
```json
{
  "indexName": "properties",
  "initialized": true
}
```

#### `DELETE /api/index/{indexName}`
Elimina un índice completo.

---

### 3. **StatsController**

#### `GET /api/stats/{indexName}`
Obtiene metadatos y estadísticas de un índice.

**Response**:
```json
{
  "name": "vehicles",
  "status": 0,
  "documentCount": 15234,
  "sizeInBytes": 52428800,
  "primaryShards": 1,
  "replicaCount": 1,
  "createdAt": "2025-12-02T10:00:00Z",
  "updatedAt": "2025-12-02T15:30:00Z"
}
```

#### `GET /api/stats`
Lista todos los índices con estadísticas básicas.

---

## ⚙️ Configuración

### appsettings.json

```json
{
  "Elasticsearch": {
    "Url": "http://elasticsearch:9200",
    "Username": "",
    "Password": "",
    "IndexPrefix": "cardealer",
    "TimeoutSeconds": 30,
    "MaxRetries": 3,
    "EnableDebugMode": false,
    "DefaultShards": 1,
    "DefaultReplicas": 1
  }
}
```

### Variables de Entorno

- `Elasticsearch__Url` - URL del cluster Elasticsearch
- `Elasticsearch__Username` - Usuario para autenticación (opcional)
- `Elasticsearch__Password` - Contraseña para autenticación (opcional)
- `Elasticsearch__IndexPrefix` - Prefijo para nombres de índices
- `Elasticsearch__DefaultShards` - Número de shards por defecto
- `Elasticsearch__DefaultReplicas` - Número de réplicas por defecto

---

## 🐳 Docker

### Dockerfile

El servicio utiliza multi-stage build para optimizar el tamaño de la imagen:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
# ... build stage ...

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
# ... runtime stage ...
```

### docker-compose.yml

```yaml
elasticsearch:
  image: docker.elastic.co/elasticsearch/elasticsearch:8.11.0
  environment:
    - discovery.type=single-node
    - xpack.security.enabled=false
  ports:
    - "9200:9200"
    - "9300:9300"
  volumes:
    - elasticsearch_data:/usr/share/elasticsearch/data

searchservice:
  build:
    context: ./SearchService
    dockerfile: Dockerfile
  environment:
    Elasticsearch__Url: "http://elasticsearch:9200"
  ports:
    - "15093:80"
  depends_on:
    - elasticsearch
```

### Ejecutar con Docker

```powershell
# Levantar solo Elasticsearch y SearchService
docker-compose up -d elasticsearch searchservice

# Ver logs
docker-compose logs -f searchservice

# Verificar health
curl http://localhost:15093/health
```

---

## 🧪 Testing

El proyecto incluye 14 tests unitarios cubriendo:

- ✅ Validación de SearchQuery
- ✅ Cálculo de paginación y offsets
- ✅ Lógica de SearchResult
- ✅ Formateo de tamaños de índices
- ✅ Estados de salud de índices

**Ejecutar tests**:
```powershell
cd backend/SearchService.Tests
dotnet test
```

---

## 📊 Casos de Uso

### 1. Búsqueda de Vehículos

```csharp
// Buscar vehículos Toyota con fuzzy search
var query = new SearchQuery
{
    QueryText = "toyata camri",  // Typos tolerados
    IndexName = "vehicles",
    SearchType = SearchType.Fuzzy,
    Fields = ["brand", "model"],
    Page = 1,
    PageSize = 20
};

POST /api/search/query
```

### 2. Autocompletado

```csharp
// Autocompletar marcas de vehículos
var query = new SearchQuery
{
    QueryText = "to",
    IndexName = "vehicles",
    SearchType = SearchType.Prefix,
    Fields = ["brand"],
    PageSize = 5
};
```

### 3. Búsqueda Avanzada con Filtros

```csharp
var query = new SearchQuery
{
    QueryText = "luxury sedan",
    IndexName = "vehicles",
    SearchType = SearchType.FullText,
    Filters = new Dictionary<string, object>
    {
        { "price", new { gte = 40000, lte = 100000 } },
        { "year", 2023 },
        { "status", "active" }
    },
    SortBy = "price",
    SortOrder = SortOrder.Descending
};
```

### 4. Indexación Masiva

```csharp
// Indexar 1000 vehículos en batch
var vehicles = LoadVehiclesFromDatabase(); // 1000 items

var bulkRequest = vehicles.Select(v => new BulkIndexRequest
{
    Id = v.Id.ToString(),
    Document = v
}).ToList();

POST /api/index/vehicles/bulk
```

---

## 🔧 Troubleshooting

### Elasticsearch no conecta

**Síntoma**: `Connection refused on http://elasticsearch:9200`

**Solución**:
1. Verificar que Elasticsearch esté corriendo: `docker ps | grep elasticsearch`
2. Esperar a que Elasticsearch esté healthy (puede tardar 60s en arrancar)
3. Verificar logs: `docker logs elasticsearch`

### Búsquedas lentas

**Síntoma**: `ExecutionTimeMs > 5000ms`

**Soluciones**:
- Reducir `PageSize` (máximo 100)
- Usar filtros en lugar de búsqueda de texto completo cuando sea posible
- Aumentar shards del índice para grandes volúmenes
- Optimizar mappings con tipos de datos específicos

### Error "Index not found"

**Solución**:
```powershell
# Crear índice manualmente
POST /api/index/vehicles
```

### Documentos no aparecen inmediatamente

**Causa**: Elasticsearch tiene refresh interval de 1s por defecto

**Solución**:
- Esperar 1-2 segundos después de indexar
- O usar `Refresh.WaitFor` en indexación (ya implementado)

---

## 📈 Performance

- **Búsqueda simple**: < 50ms
- **Búsqueda con highlighting**: < 150ms
- **Bulk indexing (100 docs)**: < 500ms
- **Índice < 1M documentos**: < 100ms average
- **Índice < 10M documentos**: < 300ms average

---

## 🔐 Seguridad

### Autenticación Elasticsearch

Para producción, configurar autenticación:

```json
{
  "Elasticsearch": {
    "Url": "https://elastic-cluster:9200",
    "Username": "elastic",
    "Password": "your-secure-password"
  }
}
```

### CORS

Por defecto permite todos los orígenes en desarrollo. Para producción:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("Production", policy =>
    {
        policy.WithOrigins("https://app.cardealer.com")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
```

---

## 🚦 Health Checks

```bash
# Health check del servicio
curl http://localhost:15093/health

# Health check de Elasticsearch
curl http://localhost:9200/_cluster/health
```

---

## 🏠 Índice de Propiedades (Real Estate)

### Inicialización

El índice `properties` está optimizado para búsqueda de bienes raíces con mappings específicos:

```bash
# Inicializar índice de propiedades
POST /api/index/initialize/properties
```

### Estructura del Documento

```json
{
  "id": "prop-123",
  "dealerId": "dealer-uuid",
  "title": "Casa en Polanco con Jardín",
  "description": "Hermosa casa de 200m² con 4 recámaras...",
  "propertyType": "house",
  "listingType": "sale",
  "status": "active",
  "price": 5000000,
  "currency": "MXN",
  "pricePerSqMeter": 25000,
  "totalArea": 200,
  "bedrooms": 4,
  "bathrooms": 3,
  "parkingSpaces": 2,
  "hasPool": true,
  "hasGarden": true,
  "hasGym": false,
  "hasSecurity": true,
  "amenities": ["pool", "garden", "rooftop"],
  "location": {
    "address": "Calle Horacio 123",
    "city": "Ciudad de México",
    "state": "CDMX",
    "neighborhood": "Polanco",
    "coordinates": { "lat": 19.4326, "lon": -99.1332 }
  },
  "seller": {
    "id": "seller-123",
    "name": "Inmobiliaria ABC",
    "isVerified": true,
    "isDealership": true
  },
  "isFeatured": true,
  "createdAt": "2025-12-06T10:00:00Z"
}
```

### Búsqueda de Propiedades

```json
POST /api/search/query
{
  "queryText": "casa polanco 4 recamaras",
  "indexName": "properties",
  "searchType": 0,
  "fields": ["title", "description", "location.neighborhood"],
  "filters": {
    "propertyType": "house",
    "bedrooms": 4,
    "hasPool": true
  },
  "page": 1,
  "pageSize": 20
}
```

### Campos Indexados

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `title` | text (spanish) | Título con análisis full-text |
| `description` | text (spanish) | Descripción con análisis full-text |
| `propertyType` | keyword | house, apartment, condo, land, commercial |
| `listingType` | keyword | sale, rent, sale-or-rent |
| `price` | double | Para rangos de precio |
| `bedrooms` | integer | Número de recámaras |
| `bathrooms` | integer | Número de baños |
| `totalArea` | double | Superficie en m² |
| `hasPool`, `hasGarden`, etc. | boolean | Filtros de amenidades |
| `location.coordinates` | geo_point | Búsqueda por ubicación |

---

## 📚 Recursos

- [Elasticsearch Documentation](https://www.elastic.co/guide/en/elasticsearch/reference/current/index.html)
- [NEST Client Documentation](https://www.elastic.co/guide/en/elasticsearch/client/net-api/current/index.html)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)

---

## 🎯 Próximos Pasos

- [ ] Implementar sugerencias de búsqueda (did you mean?)
- [ ] Agregar faceted search para filtros dinámicos
- [ ] Implementar sinónimos en búsquedas
- [ ] Dashboard de analytics de búsquedas
- [ ] Reindexación zero-downtime
- [ ] Machine learning ranking (Learning to Rank)

---

**Desarrollado por**: CarDealer Team  
**Última actualización**: 2 diciembre 2025
