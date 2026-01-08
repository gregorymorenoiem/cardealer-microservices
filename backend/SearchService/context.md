# SearchService - Context Documentation

## 📋 INFORMACIÓN GENERAL

- **Nombre del Servicio:** SearchService
- **Puerto en Desarrollo:** 5027
- **Estado:** 🚧 **EN DESARROLLO - NO DESPLEGADO**
- **Base de Datos:** PostgreSQL (`searchservice`) + Elasticsearch
- **Imagen Docker:** Local only

### Propósito
Servicio de búsqueda avanzada y filtrado. Implementa full-text search, búsqueda semántica, faceted search y autocomplete para vehículos y propiedades.

---

## 🏗️ ARQUITECTURA

```
SearchService/
├── SearchService.Api/
│   ├── Controllers/
│   │   ├── SearchController.cs
│   │   ├── AutocompleteController.cs
│   │   └── FiltersController.cs
│   └── Program.cs
├── SearchService.Application/
│   └── Services/
│       ├── ElasticsearchService.cs
│       └── SearchRankingService.cs
├── SearchService.Domain/
│   ├── Models/
│   │   ├── SearchQuery.cs
│   │   ├── SearchResult.cs
│   │   └── SearchFacet.cs
│   └── Enums/
│       └── SortOrder.cs
└── SearchService.Infrastructure/
    └── Elasticsearch/
```

---

## 📦 MODELOS PRINCIPALES

### SearchQuery
```csharp
public class SearchQuery
{
    // Búsqueda básica
    public string? Keywords { get; set; }
    public string EntityType { get; set; }         // "Vehicle", "Property"
    
    // Filtros de vehículos
    public List<string>? Makes { get; set; }
    public List<string>? Models { get; set; }
    public int? YearMin { get; set; }
    public int? YearMax { get; set; }
    public decimal? PriceMin { get; set; }
    public decimal? PriceMax { get; set; }
    public int? MileageMax { get; set; }
    public List<string>? BodyTypes { get; set; }
    public List<string>? FuelTypes { get; set; }
    public List<string>? Transmissions { get; set; }
    public List<string>? Colors { get; set; }
    
    // Filtros de propiedades
    public List<string>? PropertyTypes { get; set; }
    public int? BedroomsMin { get; set; }
    public int? BathroomsMin { get; set; }
    public decimal? SquareMetersMin { get; set; }
    public decimal? SquareMetersMax { get; set; }
    
    // Ubicación (propiedades)
    public string? City { get; set; }
    public string? State { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? RadiusKm { get; set; }
    
    // Paginación y ordenamiento
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; }            // "price", "year", "mileage", "relevance"
    public SortOrder SortOrder { get; set; } = SortOrder.Ascending;
    
    // Facets
    public bool IncludeFacets { get; set; } = true;
}
```

### SearchResult
```csharp
public class SearchResult<T>
{
    // Resultados
    public List<T> Items { get; set; }
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    
    // Facets (para filtros)
    public Dictionary<string, List<FacetValue>> Facets { get; set; }
    
    // Metadata
    public double QueryTimeMs { get; set; }
    public string? SuggestedQuery { get; set; }    // "Did you mean: Toyota Corolla?"
}

public class FacetValue
{
    public string Value { get; set; }
    public int Count { get; set; }
}
```

### SearchDocument (Elasticsearch)
```csharp
// Documento indexado en Elasticsearch
public class VehicleSearchDocument
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    
    // Datos principales
    public string Make { get; set; }
    public string Model { get; set; }
    public int Year { get; set; }
    public decimal Price { get; set; }
    public int Mileage { get; set; }
    
    // Categorización
    public string BodyType { get; set; }
    public string FuelType { get; set; }
    public string Transmission { get; set; }
    public string Color { get; set; }
    public string Condition { get; set; }
    
    // Ubicación
    public string City { get; set; }
    public string State { get; set; }
    
    // Seller
    public string SellerName { get; set; }
    public string SellerType { get; set; }
    
    // Metadata
    public string Status { get; set; }
    public DateTime PublishedAt { get; set; }
    public int ViewCount { get; set; }
    
    // Boost de ranking
    public double QualityScore { get; set; }       // 0-100
    public bool IsFeatured { get; set; }
    public bool IsCertified { get; set; }
}
```

---

## 📡 ENDPOINTS (Propuestos)

### Búsqueda Principal
- `POST /api/search` - Búsqueda avanzada
  ```json
  Request:
  {
    "keywords": "Toyota Corolla",
    "entityType": "Vehicle",
    "yearMin": 2018,
    "priceMax": 20000,
    "mileageMax": 50000,
    "transmissions": ["Automatic"],
    "page": 1,
    "pageSize": 20,
    "sortBy": "price",
    "sortOrder": "Ascending"
  }
  
  Response:
  {
    "items": [...],
    "totalCount": 45,
    "page": 1,
    "pageSize": 20,
    "totalPages": 3,
    "facets": {
      "make": [
        { "value": "Toyota", "count": 45 },
        { "value": "Honda", "count": 23 }
      ],
      "year": [
        { "value": "2020", "count": 12 },
        { "value": "2019", "count": 18 }
      ]
    },
    "queryTimeMs": 42.5
  }
  ```

### Autocomplete
- `GET /api/autocomplete` - Sugerencias mientras escribe
  ```json
  Query: ?q=toyot&type=Vehicle
  
  Response:
  {
    "suggestions": [
      { "text": "Toyota Corolla", "count": 156 },
      { "text": "Toyota Camry", "count": 89 },
      { "text": "Toyota RAV4", "count": 67 }
    ]
  }
  ```

### Filtros Dinámicos
- `GET /api/filters/available` - Obtener valores disponibles para filtros
  ```json
  Query: ?entityType=Vehicle
  
  Response:
  {
    "makes": ["Toyota", "Honda", "Nissan", ...],
    "yearRange": { "min": 2010, "max": 2024 },
    "priceRange": { "min": 5000, "max": 150000 },
    "bodyTypes": ["Sedan", "SUV", "Truck", ...]
  }
  ```

### Búsqueda Similar
- `GET /api/search/similar/{id}` - Vehículos/propiedades similares
  ```json
  Response:
  {
    "similarItems": [...],  // Basado en make, model, price range, year
    "count": 8
  }
  ```

### Búsqueda por Ubicación
- `POST /api/search/nearby` - Propiedades cercanas a ubicación
  ```json
  {
    "latitude": 18.4861,
    "longitude": -69.9312,
    "radiusKm": 10,
    "priceMax": 500000
  }
  ```

---

## 💡 FUNCIONALIDADES PLANEADAS

### Full-Text Search
Usando Elasticsearch para búsqueda de texto completo:
```csharp
// Buscar en múltiples campos con diferentes pesos
{
  "multi_match": {
    "query": "toyota corolla 2020",
    "fields": [
      "title^3",           // Peso 3x
      "description^2",     // Peso 2x
      "make^1.5",
      "model^1.5"
    ],
    "type": "best_fields"
  }
}
```

### Fuzzy Search
Tolera errores de tipeo:
- "Toyotta" → "Toyota"
- "Coroolla" → "Corolla"

### Boosting
Priorizar resultados basados en:
- `IsFeatured` (listings destacados) → +50% relevancia
- `IsCertified` (vehículos certificados) → +30%
- `ViewCount` (popularidad) → +10%
- Fecha reciente de publicación → +20%

### Faceted Search
Filtros dinámicos con conteos:
```
Make:
  ☑ Toyota (45)
  ☑ Honda (23)
  ☐ Nissan (12)

Year:
  ☑ 2020 (12)
  ☑ 2019 (18)
  ☐ 2018 (15)
```

### Saved Searches
Permitir guardar criterios de búsqueda:
- Nombre: "SUVs bajo $25K"
- Recibir alertas cuando hay nuevo match

### Search Analytics
Tracking de:
- Términos más buscados
- Búsquedas sin resultados (para mejorar catálogo)
- Click-through rate (CTR)
- Conversión por término de búsqueda

---

## 🔗 INTEGRACIÓN CON OTROS SERVICIOS

### VehiclesSaleService / PropertiesSaleService
- **Indexación automática** cuando se crea/actualiza listing
- Webhook o RabbitMQ event para sincronizar con Elasticsearch

### UserService
- Guardar historial de búsquedas por usuario
- Recomendaciones personalizadas

### NotificationService
- Alertas de saved searches
- "Nuevo vehículo que coincide con tu búsqueda"

---

## 🔄 EVENTOS CONSUMIDOS (RabbitMQ)

### VehicleCreated / VehicleUpdated
```json
{
  "vehicleId": "uuid",
  "action": "index",  // "index", "update", "delete"
  "timestamp": "2026-01-07T10:30:00Z"
}
```
→ Indexar/actualizar en Elasticsearch

### VehicleDeleted
```json
{
  "vehicleId": "uuid",
  "action": "delete",
  "timestamp": "2026-01-07T10:30:00Z"
}
```
→ Remover de índice de Elasticsearch

---

## 🛠️ ELASTICSEARCH MAPPING

```json
{
  "mappings": {
    "properties": {
      "title": { "type": "text", "boost": 3.0 },
      "description": { "type": "text", "boost": 2.0 },
      "make": { "type": "keyword", "fields": { "text": { "type": "text" } } },
      "model": { "type": "keyword", "fields": { "text": { "type": "text" } } },
      "year": { "type": "integer" },
      "price": { "type": "double" },
      "mileage": { "type": "integer" },
      "location": { "type": "geo_point" },
      "publishedAt": { "type": "date" },
      "qualityScore": { "type": "double" }
    }
  }
}
```

---

**Estado:** 🚧 EN DESARROLLO - No desplegado en producción  
**Versión:** 0.1.0  
**Dependencias:** Elasticsearch 8+
