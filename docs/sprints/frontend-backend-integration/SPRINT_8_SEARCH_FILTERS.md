# 🔍 SPRINT 8 - Search & Advanced Filters

**Fecha:** 2 Enero 2026  
**Duración estimada:** 4-5 horas  
**Tokens estimados:** ~24,000  
**Prioridad:** 🟠 Alta

---

## 🎯 OBJETIVOS

1. Implementar búsqueda avanzada con múltiples filtros
2. Agregar Elasticsearch para búsqueda full-text
3. Crear componentes de filtros dinámicos
4. Implementar faceted search
5. Agregar geolocalización y búsqueda por radio
6. Optimizar performance de búsquedas
7. Crear URL params para compartir búsquedas

---

## 📋 CHECKLIST DE TAREAS

### Fase 1: Backend - Elasticsearch Integration (1.5 horas)

- [ ] 1.1. Configurar Elasticsearch container
- [ ] 1.2. Crear VehicleDocument para indexación
- [ ] 1.3. Implementar ElasticsearchService
- [ ] 1.4. Indexar vehículos existentes
- [ ] 1.5. Configurar auto-sync con DB

### Fase 2: Backend - Search Endpoints (1 hora)

- [ ] 2.1. Crear SearchController
- [ ] 2.2. Implementar búsqueda full-text
- [ ] 2.3. Agregar filtros combinados
- [ ] 2.4. Implementar geolocalización
- [ ] 2.5. Crear facets/aggregations

### Fase 3: Frontend - Filter Components (1.5 horas)

- [ ] 3.1. Crear FilterSidebar component
- [ ] 3.2. Implementar range sliders (precio, año)
- [ ] 3.3. Crear select con autocomplete
- [ ] 3.4. Agregar chips de filtros activos
- [ ] 3.5. Implementar "Limpiar filtros"

### Fase 4: Frontend - Search Integration (1 hora)

- [ ] 4.1. Integrar filtros con URL params
- [ ] 4.2. Implementar debounce en búsquedas
- [ ] 4.3. Agregar loading states
- [ ] 4.4. Crear empty states
- [ ] 4.5. Implementar infinite scroll

---

## 📝 IMPLEMENTACIÓN DETALLADA

### 1️⃣ Backend - Elasticsearch Configuration

**Archivo:** `compose.yaml`

Agregar Elasticsearch:

```yaml
elasticsearch:
  image: docker.elastic.co/elasticsearch/elasticsearch:8.11.0
  container_name: elasticsearch
  environment:
    - discovery.type=single-node
    - xpack.security.enabled=false
    - "ES_JAVA_OPTS=-Xms512m -Xmx512m"
  ports:
    - "9200:9200"
  volumes:
    - elasticsearch_data:/usr/share/elasticsearch/data
  networks:
    - cargurus-net
  deploy:
    resources:
      limits:
        cpus: '0.5'
        memory: 1G
      reservations:
        memory: 512M

volumes:
  elasticsearch_data:
```

---

### 2️⃣ Backend - Elasticsearch Service

**Archivo:** `backend/SearchService/SearchService.Infrastructure/Services/ElasticsearchService.cs`

```csharp
using Nest;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SearchService.Infrastructure.Services;

public interface IElasticsearchService
{
    Task<SearchResponse<VehicleDocument>> SearchAsync(SearchRequest request);
    Task IndexVehicleAsync(VehicleDocument vehicle);
    Task DeleteVehicleAsync(string id);
    Task<AggregationsResponse> GetAggregationsAsync(SearchRequest request);
}

public class ElasticsearchService : IElasticsearchService
{
    private readonly IElasticClient _client;
    private readonly ILogger<ElasticsearchService> _logger;
    private const string IndexName = "vehicles";

    public ElasticsearchService(IConfiguration configuration, ILogger<ElasticsearchService> logger)
    {
        _logger = logger;

        var settings = new ConnectionSettings(new Uri(configuration["Elasticsearch:Url"] ?? "http://localhost:9200"))
            .DefaultIndex(IndexName)
            .EnableDebugMode()
            .PrettyJson();

        _client = new ElasticClient(settings);

        CreateIndexIfNotExists();
    }

    private void CreateIndexIfNotExists()
    {
        var existsResponse = _client.Indices.Exists(IndexName);
        
        if (!existsResponse.Exists)
        {
            var createResponse = _client.Indices.Create(IndexName, c => c
                .Map<VehicleDocument>(m => m
                    .AutoMap()
                    .Properties(p => p
                        .Text(t => t.Name(n => n.Title).Analyzer("standard"))
                        .Text(t => t.Name(n => n.Make).Analyzer("keyword"))
                        .Text(t => t.Name(n => n.Model).Analyzer("keyword"))
                        .Text(t => t.Name(n => n.Description).Analyzer("standard"))
                        .Keyword(k => k.Name(n => n.FuelType))
                        .Keyword(k => k.Name(n => n.Transmission))
                        .Keyword(k => k.Name(n => n.BodyType))
                        .Number(n => n.Name(nn => nn.Price).Type(NumberType.Double))
                        .Number(n => n.Name(nn => nn.Year).Type(NumberType.Integer))
                        .Number(n => n.Name(nn => nn.Mileage).Type(NumberType.Integer))
                        .GeoPoint(g => g.Name(n => n.Location))
                    )
                )
            );

            if (!createResponse.IsValid)
            {
                _logger.LogError("Failed to create index: {Error}", createResponse.ServerError);
            }
        }
    }

    public async Task<SearchResponse<VehicleDocument>> SearchAsync(SearchRequest request)
    {
        var searchDescriptor = new SearchDescriptor<VehicleDocument>()
            .From((request.Page - 1) * request.PageSize)
            .Size(request.PageSize);

        // Full-text search
        if (!string.IsNullOrEmpty(request.Query))
        {
            searchDescriptor.Query(q => q
                .MultiMatch(mm => mm
                    .Query(request.Query)
                    .Fields(f => f
                        .Field(ff => ff.Title, boost: 3)
                        .Field(ff => ff.Make, boost: 2)
                        .Field(ff => ff.Model, boost: 2)
                        .Field(ff => ff.Description)
                    )
                    .Type(TextQueryType.BestFields)
                    .Fuzziness(Fuzziness.Auto)
                )
            );
        }

        // Filters
        var filters = new List<Func<QueryContainerDescriptor<VehicleDocument>, QueryContainer>>();

        if (!string.IsNullOrEmpty(request.Make))
        {
            filters.Add(f => f.Term(t => t.Field(ff => ff.Make).Value(request.Make)));
        }

        if (!string.IsNullOrEmpty(request.Model))
        {
            filters.Add(f => f.Term(t => t.Field(ff => ff.Model).Value(request.Model)));
        }

        if (request.MinPrice.HasValue || request.MaxPrice.HasValue)
        {
            filters.Add(f => f.Range(r =>
            {
                var range = r.Field(ff => ff.Price);
                if (request.MinPrice.HasValue) range.GreaterThanOrEquals(request.MinPrice.Value);
                if (request.MaxPrice.HasValue) range.LessThanOrEquals(request.MaxPrice.Value);
                return range;
            }));
        }

        if (request.MinYear.HasValue || request.MaxYear.HasValue)
        {
            filters.Add(f => f.Range(r =>
            {
                var range = r.Field(ff => ff.Year);
                if (request.MinYear.HasValue) range.GreaterThanOrEquals(request.MinYear.Value);
                if (request.MaxYear.HasValue) range.LessThanOrEquals(request.MaxYear.Value);
                return range;
            }));
        }

        // Geographic search
        if (request.Latitude.HasValue && request.Longitude.HasValue && request.Radius.HasValue)
        {
            filters.Add(f => f.GeoDistance(g => g
                .Field(ff => ff.Location)
                .Location(request.Latitude.Value, request.Longitude.Value)
                .Distance($"{request.Radius.Value}km")
            ));
        }

        if (filters.Any())
        {
            searchDescriptor.Query(q => q.Bool(b => b.Filter(filters)));
        }

        // Sorting
        if (!string.IsNullOrEmpty(request.SortBy))
        {
            searchDescriptor.Sort(s =>
            {
                return request.SortBy.ToLower() switch
                {
                    "price" => request.SortDescending
                        ? s.Descending(f => f.Price)
                        : s.Ascending(f => f.Price),
                    "year" => request.SortDescending
                        ? s.Descending(f => f.Year)
                        : s.Ascending(f => f.Year),
                    "mileage" => request.SortDescending
                        ? s.Descending(f => f.Mileage)
                        : s.Ascending(f => f.Mileage),
                    _ => s.Descending(f => f.CreatedAt)
                };
            });
        }

        var response = await _client.SearchAsync<VehicleDocument>(searchDescriptor);

        if (!response.IsValid)
        {
            _logger.LogError("Search failed: {Error}", response.ServerError);
        }

        return new SearchResponse<VehicleDocument>
        {
            Items = response.Documents.ToList(),
            Total = response.Total,
            Page = request.Page,
            PageSize = request.PageSize,
            Took = response.Took
        };
    }

    public async Task<AggregationsResponse> GetAggregationsAsync(SearchRequest request)
    {
        var searchDescriptor = new SearchDescriptor<VehicleDocument>()
            .Size(0)
            .Aggregations(a => a
                .Terms("makes", t => t.Field(f => f.Make).Size(50))
                .Terms("fuel_types", t => t.Field(f => f.FuelType).Size(20))
                .Terms("transmissions", t => t.Field(f => f.Transmission).Size(10))
                .Terms("body_types", t => t.Field(f => f.BodyType).Size(20))
                .Range("price_ranges", r => r
                    .Field(f => f.Price)
                    .Ranges(
                        rr => rr.To(10000),
                        rr => rr.From(10000).To(20000),
                        rr => rr.From(20000).To(30000),
                        rr => rr.From(30000).To(50000),
                        rr => rr.From(50000)
                    )
                )
            );

        var response = await _client.SearchAsync<VehicleDocument>(searchDescriptor);

        return new AggregationsResponse
        {
            Makes = response.Aggregations.Terms("makes").Buckets
                .Select(b => new FacetItem { Value = b.Key, Count = b.DocCount ?? 0 })
                .ToList(),
            FuelTypes = response.Aggregations.Terms("fuel_types").Buckets
                .Select(b => new FacetItem { Value = b.Key, Count = b.DocCount ?? 0 })
                .ToList(),
            Transmissions = response.Aggregations.Terms("transmissions").Buckets
                .Select(b => new FacetItem { Value = b.Key, Count = b.DocCount ?? 0 })
                .ToList(),
            BodyTypes = response.Aggregations.Terms("body_types").Buckets
                .Select(b => new FacetItem { Value = b.Key, Count = b.DocCount ?? 0 })
                .ToList()
        };
    }

    public async Task IndexVehicleAsync(VehicleDocument vehicle)
    {
        var response = await _client.IndexAsync(vehicle, idx => idx.Id(vehicle.Id));
        
        if (!response.IsValid)
        {
            _logger.LogError("Failed to index vehicle: {Error}", response.ServerError);
        }
    }

    public async Task DeleteVehicleAsync(string id)
    {
        await _client.DeleteAsync<VehicleDocument>(id);
    }
}

public class VehicleDocument
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal Price { get; set; }
    public int Mileage { get; set; }
    public string FuelType { get; set; } = string.Empty;
    public string Transmission { get; set; } = string.Empty;
    public string BodyType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public GeoLocation? Location { get; set; }
    public List<string> ImageUrls { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class SearchRequest
{
    public string? Query { get; set; }
    public string? Make { get; set; }
    public string? Model { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int? MinYear { get; set; }
    public int? MaxYear { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? Radius { get; set; }
    public string? SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}

public class SearchResponse<T>
{
    public List<T> Items { get; set; } = new();
    public long Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public long Took { get; set; }
}
```

**Instalar paquete NuGet:**

```xml
<PackageReference Include="NEST" Version="7.17.5" />
<PackageReference Include="Elasticsearch.Net" Version="7.17.5" />
```

---

### 3️⃣ Frontend - Filter Sidebar Component

**Archivo:** `frontend/web/original/src/components/FilterSidebar.tsx`

```typescript
import { useState, type FC } from 'react';
import { X, SlidersHorizontal } from 'lucide-react';
import type { VehicleFilters } from '@/types/vehicle';

interface FilterSidebarProps {
  filters: VehicleFilters;
  onChange: (filters: VehicleFilters) => void;
  makes: string[];
  models: string[];
}

export const FilterSidebar: FC<FilterSidebarProps> = ({
  filters,
  onChange,
  makes,
  models
}) => {
  const [localFilters, setLocalFilters] = useState<VehicleFilters>(filters);

  const handleChange = (key: keyof VehicleFilters, value: any) => {
    const newFilters = { ...localFilters, [key]: value };
    setLocalFilters(newFilters);
    onChange(newFilters);
  };

  const clearFilters = () => {
    const empty: VehicleFilters = {};
    setLocalFilters(empty);
    onChange(empty);
  };

  const activeFilterCount = Object.values(localFilters).filter(Boolean).length;

  return (
    <div className="w-80 bg-white border-r border-gray-200 p-6 overflow-y-auto">
      {/* Header */}
      <div className="flex items-center justify-between mb-6">
        <div className="flex items-center gap-2">
          <SlidersHorizontal className="w-5 h-5 text-gray-600" />
          <h3 className="text-lg font-semibold text-gray-900">Filtros</h3>
          {activeFilterCount > 0 && (
            <span className="bg-blue-600 text-white text-xs px-2 py-0.5 rounded-full">
              {activeFilterCount}
            </span>
          )}
        </div>
        {activeFilterCount > 0 && (
          <button
            onClick={clearFilters}
            className="text-sm text-blue-600 hover:text-blue-700"
          >
            Limpiar todo
          </button>
        )}
      </div>

      {/* Make */}
      <div className="mb-6">
        <label className="block text-sm font-medium text-gray-700 mb-2">
          Marca
        </label>
        <select
          value={localFilters.make || ''}
          onChange={(e) => handleChange('make', e.target.value || undefined)}
          className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 
                   focus:ring-blue-600 focus:border-transparent"
        >
          <option value="">Todas las marcas</option>
          {makes.map((make) => (
            <option key={make} value={make}>
              {make}
            </option>
          ))}
        </select>
      </div>

      {/* Model */}
      {localFilters.make && (
        <div className="mb-6">
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Modelo
          </label>
          <select
            value={localFilters.model || ''}
            onChange={(e) => handleChange('model', e.target.value || undefined)}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg"
          >
            <option value="">Todos los modelos</option>
            {models.map((model) => (
              <option key={model} value={model}>
                {model}
              </option>
            ))}
          </select>
        </div>
      )}

      {/* Price Range */}
      <div className="mb-6">
        <label className="block text-sm font-medium text-gray-700 mb-2">
          Precio
        </label>
        <div className="grid grid-cols-2 gap-2">
          <input
            type="number"
            placeholder="Mín"
            value={localFilters.minPrice || ''}
            onChange={(e) => handleChange('minPrice', e.target.value ? Number(e.target.value) : undefined)}
            className="px-3 py-2 border border-gray-300 rounded-lg"
          />
          <input
            type="number"
            placeholder="Máx"
            value={localFilters.maxPrice || ''}
            onChange={(e) => handleChange('maxPrice', e.target.value ? Number(e.target.value) : undefined)}
            className="px-3 py-2 border border-gray-300 rounded-lg"
          />
        </div>
      </div>

      {/* Year Range */}
      <div className="mb-6">
        <label className="block text-sm font-medium text-gray-700 mb-2">
          Año
        </label>
        <div className="grid grid-cols-2 gap-2">
          <input
            type="number"
            placeholder="Desde"
            value={localFilters.minYear || ''}
            onChange={(e) => handleChange('minYear', e.target.value ? Number(e.target.value) : undefined)}
            className="px-3 py-2 border border-gray-300 rounded-lg"
          />
          <input
            type="number"
            placeholder="Hasta"
            value={localFilters.maxYear || ''}
            onChange={(e) => handleChange('maxYear', e.target.value ? Number(e.target.value) : undefined)}
            className="px-3 py-2 border border-gray-300 rounded-lg"
          />
        </div>
      </div>

      {/* Fuel Type */}
      <div className="mb-6">
        <label className="block text-sm font-medium text-gray-700 mb-2">
          Combustible
        </label>
        <select
          value={localFilters.fuelType || ''}
          onChange={(e) => handleChange('fuelType', e.target.value || undefined)}
          className="w-full px-3 py-2 border border-gray-300 rounded-lg"
        >
          <option value="">Todos</option>
          <option value="Gasoline">Gasolina</option>
          <option value="Diesel">Diesel</option>
          <option value="Electric">Eléctrico</option>
          <option value="Hybrid">Híbrido</option>
        </select>
      </div>

      {/* Transmission */}
      <div className="mb-6">
        <label className="block text-sm font-medium text-gray-700 mb-2">
          Transmisión
        </label>
        <select
          value={localFilters.transmission || ''}
          onChange={(e) => handleChange('transmission', e.target.value || undefined)}
          className="w-full px-3 py-2 border border-gray-300 rounded-lg"
        >
          <option value="">Todas</option>
          <option value="Manual">Manual</option>
          <option value="Automatic">Automática</option>
          <option value="SemiAutomatic">Semi-automática</option>
        </select>
      </div>
    </div>
  );
};
```

---

## ✅ CRITERIOS DE ACEPTACIÓN

1. Búsqueda por texto encuentra vehículos relevantes
2. Filtros se aplican correctamente
3. URL params reflejan filtros activos
4. Resultados se actualizan en tiempo real
5. Geolocalización funciona dentro de radio
6. Performance < 500ms en búsquedas

---

## 📊 ESTIMACIÓN DE TOKENS

| Tarea | Tokens |
|-------|--------|
| Elasticsearch configuration | 3,000 |
| ElasticsearchService | 6,000 |
| Search controller | 4,000 |
| FilterSidebar component | 5,000 |
| Search integration | 4,000 |
| Testing | 2,000 |
| **TOTAL** | **~24,000** |

---

## ➡️ PRÓXIMO SPRINT

**Sprint 9:** Saved Searches

---

**Estado:** ⚪ Pendiente  
**Última actualización:** 2 Enero 2026
