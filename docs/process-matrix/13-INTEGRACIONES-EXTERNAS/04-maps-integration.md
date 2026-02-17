# 🗺️ Maps Integration - Integración de Mapas - Matriz de Procesos

> **Servicio:** VehiclesSaleService / LocationService  
> **Proveedor:** Google Maps Platform  
> **Última actualización:** Enero 25, 2026  
> **Estado de Implementación:** 🟡 60% Backend | 🟡 70% UI

---

## ⚠️ AUDITORÍA DE ACCESO UI (Enero 25, 2026)

| Proceso             | Backend | UI Access | Observación                 |
| ------------------- | ------- | --------- | --------------------------- |
| Geocoding           | 🟡 70%  | ✅ 80%    | Autocompletado de dirección |
| Places Autocomplete | ✅ 90%  | ✅ 90%    | En formularios              |
| Distance Matrix     | 🔴 0%   | 🔴 0%     | No implementado             |
| Static Maps         | ✅ 100% | ✅ 100%   | Thumbnails de ubicación     |

### Rutas UI Existentes ✅

- `/sell` - Autocompletado de ubicación
- `/vehicles/:slug` - Mapa de ubicación del vehículo
- `/dealer/:id` - Mapa de sucursales del dealer

### Rutas UI Faltantes 🔴

- `/search` - Filtro por distancia ("a 10km de mi ubicación")
- `/dealers/map` - Vista de mapa de todos los dealers

**Verificación:** Google Maps SDK integrado en frontend, backend geocoding parcial.

---

## 📊 Resumen de Implementación

| Componente    | Total | Implementado | Pendiente | Estado |
| ------------- | ----- | ------------ | --------- | ------ |
| Controllers   | 1     | 0            | 1         | 🔴     |
| MAP-GEO-\*    | 4     | 0            | 4         | 🔴     |
| MAP-PLACE-\*  | 3     | 0            | 3         | 🔴     |
| MAP-DIST-\*   | 3     | 0            | 3         | 🔴     |
| MAP-STATIC-\* | 2     | 0            | 2         | 🔴     |
| Tests         | 0     | 0            | 10        | 🔴     |

**Leyenda:** ✅ Implementado + Tested | 🟢 Implementado | 🟡 En Progreso | 🔴 Pendiente

---

## 1. Información General

### 1.1 Descripción

Integración con Google Maps Platform para geolocalización de vehículos, dealers, y funcionalidades de búsqueda por ubicación. Incluye geocoding, Places API, y Distance Matrix.

### 1.2 APIs Utilizadas

| API                     | Propósito                         | Uso en OKLA             |
| ----------------------- | --------------------------------- | ----------------------- |
| **Geocoding**           | Convertir dirección → coordenadas | Ubicación de vehículos  |
| **Reverse Geocoding**   | Coordenadas → dirección           | Auto-detectar ubicación |
| **Places Autocomplete** | Sugerencias de direcciones        | Formularios             |
| **Places Details**      | Detalles de lugar                 | Validar direcciones     |
| **Distance Matrix**     | Distancia entre puntos            | "Cerca de ti"           |
| **Maps JavaScript**     | Mapa en frontend                  | Visualización           |
| **Static Maps**         | Imágenes de mapas                 | Emails/PDFs             |

### 1.3 Arquitectura

```
┌─────────────────────────────────────────────────────────────────────────┐
│                     Maps Integration Architecture                        │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│   Frontend                                                              │
│   ┌──────────────────────────────────────────────────────────────────┐  │
│   │                                                                   │  │
│   │  ┌─────────────────┐     ┌─────────────────┐                     │  │
│   │  │  Maps JS SDK    │     │ Places          │                     │  │
│   │  │  (Interactive)  │     │ Autocomplete    │                     │  │
│   │  └────────┬────────┘     └────────┬────────┘                     │  │
│   │           │                       │                              │  │
│   │           └───────────┬───────────┘                              │  │
│   │                       │                                          │  │
│   │                       ▼                                          │  │
│   │               Google Maps API                                    │  │
│   │                                                                   │  │
│   └──────────────────────────────────────────────────────────────────┘  │
│                                                                          │
│   Backend                                                               │
│   ┌──────────────────────────────────────────────────────────────────┐  │
│   │                                                                   │  │
│   │  ┌─────────────────┐     ┌─────────────────┐                     │  │
│   │  │ LocationService │     │  VehiclesSale   │                     │  │
│   │  │                 │     │    Service      │                     │  │
│   │  │  - Geocoding    │     │                 │                     │  │
│   │  │  - Distance     │     │  - Nearby search│                     │  │
│   │  │  - Validation   │     │  - Location     │                     │  │
│   │  └────────┬────────┘     └────────┬────────┘                     │  │
│   │           │                       │                              │  │
│   │           └───────────┬───────────┘                              │  │
│   │                       │                                          │  │
│   │                       ▼                                          │  │
│   │           ┌─────────────────────┐                                │  │
│   │           │  Google Maps API    │                                │  │
│   │           │  (Server-side)      │                                │  │
│   │           └─────────────────────┘                                │  │
│   │                                                                   │  │
│   └──────────────────────────────────────────────────────────────────┘  │
│                                                                          │
│   Caching Layer                                                         │
│   ┌──────────────────────────────────────────────────────────────────┐  │
│   │  Redis Cache                                                      │  │
│   │  - Geocoding results (TTL: 30 days)                               │  │
│   │  - Place details (TTL: 7 days)                                    │  │
│   │  - Distance calculations (TTL: 1 hour)                            │  │
│   └──────────────────────────────────────────────────────────────────┘  │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 2. Endpoints

### 2.1 Geocoding

| Método | Endpoint                        | Descripción             | Auth |
| ------ | ------------------------------- | ----------------------- | ---- |
| `POST` | `/api/location/geocode`         | Dirección → Coordenadas | User |
| `POST` | `/api/location/reverse-geocode` | Coordenadas → Dirección | User |
| `GET`  | `/api/location/validate`        | Validar dirección       | User |

### 2.2 Places

| Método | Endpoint                        | Descripción                | Auth |
| ------ | ------------------------------- | -------------------------- | ---- |
| `GET`  | `/api/location/autocomplete`    | Sugerencias de direcciones | User |
| `GET`  | `/api/location/place/{placeId}` | Detalles de lugar          | User |

### 2.3 Distance

| Método | Endpoint                 | Descripción            | Auth   |
| ------ | ------------------------ | ---------------------- | ------ |
| `POST` | `/api/location/distance` | Distancia entre puntos | User   |
| `GET`  | `/api/vehicles/nearby`   | Vehículos cercanos     | Public |

### 2.4 Dealers

| Método | Endpoint                      | Descripción            | Auth   |
| ------ | ----------------------------- | ---------------------- | ------ |
| `GET`  | `/api/dealers/nearby`         | Dealers cercanos       | Public |
| `GET`  | `/api/dealers/{id}/locations` | Ubicaciones del dealer | Public |

---

## 3. Entidades

### 3.1 Location

```csharp
public class Location
{
    public Guid Id { get; set; }

    // Address components
    public string FormattedAddress { get; set; } = string.Empty;
    public string? StreetNumber { get; set; }
    public string? StreetName { get; set; }
    public string? City { get; set; }
    public string? Province { get; set; }
    public string? Municipality { get; set; }
    public string? Sector { get; set; }
    public string? PostalCode { get; set; }
    public string Country { get; set; } = "DO";

    // Coordinates
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    // Google Place
    public string? PlaceId { get; set; }
    public string? PlusCode { get; set; }

    // Metadata
    public LocationType Type { get; set; }
    public bool IsVerified { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public enum LocationType
{
    Vehicle,
    Dealer,
    DealerBranch,
    User,
    Event
}
```

### 3.2 GeocodingResult (DTO)

```csharp
public record GeocodingResult
{
    public string FormattedAddress { get; init; } = string.Empty;
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public string? PlaceId { get; init; }
    public AddressComponents Components { get; init; } = new();
    public GeocodeAccuracy Accuracy { get; init; }
}

public record AddressComponents
{
    public string? StreetNumber { get; init; }
    public string? Route { get; init; }
    public string? Locality { get; init; } // Ciudad
    public string? AdminArea1 { get; init; } // Provincia
    public string? AdminArea2 { get; init; } // Municipio
    public string? Neighborhood { get; init; } // Sector
    public string? PostalCode { get; init; }
    public string? Country { get; init; }
}

public enum GeocodeAccuracy
{
    Rooftop,      // Exacto
    RangeInterpolated, // Aproximado
    GeometricCenter, // Centro de área
    Approximate    // Aproximado (ciudad/provincia)
}
```

### 3.3 NearbySearchRequest (DTO)

```csharp
public record NearbySearchRequest
{
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public int RadiusKm { get; init; } = 25;
    public int? MaxResults { get; init; } = 20;

    // Filters
    public List<string>? Makes { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public int? MinYear { get; init; }
    public int? MaxYear { get; init; }
}
```

---

## 4. Provincias y Ciudades de RD

### 4.1 Datos de Referencia

```csharp
public static class DominicanLocations
{
    public static readonly Dictionary<string, List<string>> ProvinciaMunicipios = new()
    {
        ["Distrito Nacional"] = new() { "Santo Domingo de Guzmán" },
        ["Santo Domingo"] = new()
        {
            "Santo Domingo Este", "Santo Domingo Norte", "Santo Domingo Oeste",
            "Boca Chica", "San Antonio de Guerra", "Los Alcarrizos", "Pedro Brand"
        },
        ["Santiago"] = new()
        {
            "Santiago de los Caballeros", "Bisonó", "Jánico", "Licey al Medio",
            "Puñal", "Sabana Iglesia", "San José de las Matas", "Tamboril", "Villa González"
        },
        ["La Vega"] = new() { "Concepción de La Vega", "Constanza", "Jarabacoa", "Jima Abajo" },
        ["Puerto Plata"] = new() { "San Felipe de Puerto Plata", "Sosúa", "Cabarete", "Imbert" },
        ["San Cristóbal"] = new() { "San Cristóbal", "Bajos de Haina", "Villa Altagracia" },
        ["La Romana"] = new() { "La Romana", "Guaymate", "Villa Hermosa" },
        ["San Pedro de Macorís"] = new() { "San Pedro de Macorís", "Consuelo", "Quisqueya" },
        // ... más provincias
    };

    public static readonly (double Lat, double Lng) CenterOfRD = (18.7357, -70.1627);
    public static readonly (double SouthWest, double NorthEast) BoundsRD =
        ((17.4, -72.0), (19.95, -68.3));
}
```

---

## 5. Procesos Detallados

### 5.1 MAP-001: Geocodificar Dirección de Vehículo

| Paso | Acción                        | Sistema             | Validación         |
| ---- | ----------------------------- | ------------------- | ------------------ |
| 1    | Usuario ingresa dirección     | Frontend            | Dirección no vacía |
| 2    | Places Autocomplete sugiere   | Google Maps         | Suggestions shown  |
| 3    | Usuario selecciona sugerencia | Frontend            | Place selected     |
| 4    | Obtener Place Details         | Google Maps         | Details obtained   |
| 5    | Enviar PlaceId al backend     | Frontend            | PlaceId válido     |
| 6    | Verificar cache               | Redis               | Cache hit/miss     |
| 7    | Si miss, llamar Geocoding API | Google Maps         | Response OK        |
| 8    | Guardar en cache              | Redis               | TTL 30 días        |
| 9    | Validar está en RD            | LocationService     | Dentro de bounds   |
| 10   | Guardar ubicación             | LocationService     | Location saved     |
| 11   | Actualizar vehículo           | VehiclesSaleService | Vehicle updated    |

```csharp
public class LocationService : ILocationService
{
    private readonly IGoogleMapsClient _mapsClient;
    private readonly IDistributedCache _cache;

    public async Task<GeocodingResult?> GeocodeAsync(
        string address,
        CancellationToken ct = default)
    {
        // 1. Check cache
        var cacheKey = $"geocode:{ComputeHash(address.ToLower())}";
        var cached = await _cache.GetStringAsync(cacheKey, ct);

        if (!string.IsNullOrEmpty(cached))
        {
            return JsonSerializer.Deserialize<GeocodingResult>(cached);
        }

        // 2. Call Google Geocoding API
        var response = await _mapsClient.GeocodeAsync(new GeocodeRequest
        {
            Address = address,
            Region = "do", // Prefer Dominican Republic
            Bounds = new Bounds
            {
                SouthWest = new LatLng(17.4, -72.0),
                NorthEast = new LatLng(19.95, -68.3)
            }
        }, ct);

        if (response.Status != "OK" || response.Results.Length == 0)
        {
            return null;
        }

        var result = response.Results[0];

        // 3. Parse result
        var geocodeResult = new GeocodingResult
        {
            FormattedAddress = result.FormattedAddress,
            Latitude = result.Geometry.Location.Lat,
            Longitude = result.Geometry.Location.Lng,
            PlaceId = result.PlaceId,
            Components = ParseAddressComponents(result.AddressComponents),
            Accuracy = MapLocationType(result.Geometry.LocationType)
        };

        // 4. Validate is in Dominican Republic
        if (!IsInDominicanRepublic(geocodeResult.Latitude, geocodeResult.Longitude))
        {
            throw new InvalidLocationException("Location must be in Dominican Republic");
        }

        // 5. Cache result (30 days)
        await _cache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(geocodeResult),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30)
            },
            ct);

        return geocodeResult;
    }

    private bool IsInDominicanRepublic(double lat, double lng)
    {
        // Bounding box of Dominican Republic
        return lat >= 17.4 && lat <= 19.95 && lng >= -72.0 && lng <= -68.3;
    }
}
```

### 5.2 MAP-002: Búsqueda de Vehículos Cercanos

| Paso | Acción                           | Sistema             | Validación            |
| ---- | -------------------------------- | ------------------- | --------------------- |
| 1    | Usuario permite geolocalización  | Browser             | Permission granted    |
| 2    | Obtener coordenadas actuales     | Browser API         | Coords obtained       |
| 3    | Enviar request con coords        | Frontend            | Request válido        |
| 4    | Buscar vehículos en radio        | VehiclesSaleService | Query ejecutado       |
| 5    | Calcular distancia para cada uno | LocationService     | Distancias calculadas |
| 6    | Ordenar por distancia            | VehiclesSaleService | Lista ordenada        |
| 7    | Retornar resultados              | API                 | Response sent         |

```csharp
public class VehicleSearchService
{
    public async Task<PagedResult<VehicleWithDistance>> SearchNearbyAsync(
        NearbySearchRequest request,
        CancellationToken ct = default)
    {
        // 1. Build spatial query using PostGIS
        var radiusMeters = request.RadiusKm * 1000;

        var query = _context.Vehicles
            .Where(v => v.Status == VehicleStatus.Active)
            .Where(v => v.Location != null);

        // Apply filters
        if (request.Makes?.Any() == true)
            query = query.Where(v => request.Makes.Contains(v.Make));

        if (request.MinPrice.HasValue)
            query = query.Where(v => v.Price >= request.MinPrice.Value);

        if (request.MaxPrice.HasValue)
            query = query.Where(v => v.Price <= request.MaxPrice.Value);

        // 2. Calculate distance using Haversine formula (PostGIS)
        var userPoint = new Point(request.Longitude, request.Latitude) { SRID = 4326 };

        var vehiclesWithDistance = await query
            .Select(v => new
            {
                Vehicle = v,
                Distance = v.Location!.Coordinates.Distance(userPoint)
            })
            .Where(x => x.Distance <= radiusMeters)
            .OrderBy(x => x.Distance)
            .Take(request.MaxResults ?? 20)
            .ToListAsync(ct);

        // 3. Map to DTOs
        var results = vehiclesWithDistance.Select(x => new VehicleWithDistance
        {
            Vehicle = _mapper.Map<VehicleDto>(x.Vehicle),
            DistanceKm = Math.Round(x.Distance / 1000, 1)
        }).ToList();

        return new PagedResult<VehicleWithDistance>
        {
            Items = results,
            TotalCount = results.Count
        };
    }
}
```

### 5.3 MAP-003: Places Autocomplete

```
┌─────────────────────────────────────────────────────────────────────────┐
│                     Places Autocomplete Flow                             │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│   User types: "Av. 27"                                                  │
│        │                                                                │
│        ▼                                                                │
│   ┌───────────────────────────────────────────────────────────────────┐ │
│   │ Google Places Autocomplete (debounced 300ms)                      │ │
│   │ Session token (for billing)                                       │ │
│   └───────────────────────────────────────────────────────────────────┘ │
│        │                                                                │
│        ▼                                                                │
│   Suggestions:                                                          │
│   ┌───────────────────────────────────────────────────────────────────┐ │
│   │ 📍 Av. 27 de Febrero, Santo Domingo                               │ │
│   │ 📍 Av. 27 de Febrero, Santiago de los Caballeros                  │ │
│   │ 📍 Av. 27 de Febrero #123, Piantini, Santo Domingo                │ │
│   └───────────────────────────────────────────────────────────────────┘ │
│        │                                                                │
│        │ User selects                                                   │
│        ▼                                                                │
│   ┌───────────────────────────────────────────────────────────────────┐ │
│   │ Places Details API                                                 │ │
│   │ - Full address                                                     │ │
│   │ - Coordinates                                                      │ │
│   │ - Place ID                                                         │ │
│   └───────────────────────────────────────────────────────────────────┘ │
│        │                                                                │
│        ▼                                                                │
│   Form auto-filled:                                                     │
│   - Address: Av. 27 de Febrero #123                                    │
│   - City: Santo Domingo                                                │
│   - Sector: Piantini                                                   │
│   - Province: Distrito Nacional                                        │
│   - Lat/Lng: 18.4682, -69.9293                                         │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

```typescript
// Frontend implementation
const PlacesAutocomplete: React.FC<Props> = ({ onSelect }) => {
  const [inputValue, setInputValue] = useState('');
  const [suggestions, setSuggestions] = useState<PlaceSuggestion[]>([]);
  const sessionToken = useRef(new google.maps.places.AutocompleteSessionToken());

  // Debounced search
  const debouncedSearch = useDebouncedCallback(async (value: string) => {
    if (value.length < 3) return;

    const service = new google.maps.places.AutocompleteService();
    const results = await service.getPlacePredictions({
      input: value,
      sessionToken: sessionToken.current,
      componentRestrictions: { country: 'do' },
      types: ['address']
    });

    setSuggestions(results.predictions);
  }, 300);

  const handleSelect = async (placeId: string) => {
    const service = new google.maps.places.PlacesService(document.createElement('div'));

    service.getDetails(
      { placeId, fields: ['formatted_address', 'geometry', 'address_components'] },
      (place, status) => {
        if (status === 'OK' && place) {
          onSelect({
            formattedAddress: place.formatted_address,
            latitude: place.geometry?.location?.lat(),
            longitude: place.geometry?.location?.lng(),
            placeId: placeId,
            components: parseAddressComponents(place.address_components)
          });

          // Reset session token after selection (billing optimization)
          sessionToken.current = new google.maps.places.AutocompleteSessionToken();
        }
      }
    );
  };

  return (
    <Combobox value={inputValue} onChange={setInputValue}>
      <Combobox.Input />
      <Combobox.Options>
        {suggestions.map(s => (
          <Combobox.Option key={s.place_id} value={s.place_id}>
            {s.description}
          </Combobox.Option>
        ))}
      </Combobox.Options>
    </Combobox>
  );
};
```

---

## 6. Reglas de Negocio

| Código  | Regla                                    | Validación                     |
| ------- | ---------------------------------------- | ------------------------------ |
| MAP-R01 | Solo ubicaciones en RD                   | IsInDominicanRepublic()        |
| MAP-R02 | Precisión mínima: ciudad                 | Accuracy != Approximate (país) |
| MAP-R03 | Cache geocoding 30 días                  | Reduce API calls               |
| MAP-R04 | Radio máximo búsqueda: 100km             | RadiusKm <= 100                |
| MAP-R05 | Session tokens para Places               | Billing optimization           |
| MAP-R06 | Dealers deben tener ubicación verificada | IsVerified == true             |

---

## 7. Códigos de Error

| Código    | HTTP | Mensaje                             | Causa                       |
| --------- | ---- | ----------------------------------- | --------------------------- |
| `MAP_001` | 400  | Invalid address                     | Dirección no geocodificable |
| `MAP_002` | 400  | Location outside Dominican Republic | Fuera de RD                 |
| `MAP_003` | 400  | Invalid coordinates                 | Lat/Lng inválidos           |
| `MAP_004` | 429  | API quota exceeded                  | Límite de Google Maps       |
| `MAP_005` | 500  | Geocoding service error             | Error de Google             |
| `MAP_006` | 400  | Radius too large                    | Radio > 100km               |

---

## 8. Configuración

```json
{
  "GoogleMaps": {
    "ApiKey": "${GOOGLE_MAPS_API_KEY}",
    "ServerApiKey": "${GOOGLE_MAPS_SERVER_KEY}",
    "DefaultCountry": "DO",
    "DefaultLanguage": "es",
    "Bounds": {
      "SouthWest": { "Lat": 17.4, "Lng": -72.0 },
      "NorthEast": { "Lat": 19.95, "Lng": -68.3 }
    },
    "Cache": {
      "GeocodingTTLDays": 30,
      "PlaceDetailsTTLDays": 7,
      "DistanceTTLMinutes": 60
    },
    "Quotas": {
      "GeocodingPerDay": 10000,
      "PlacesPerDay": 5000,
      "DistanceMatrixPerDay": 2500
    }
  }
}
```

---

## 9. Métricas Prometheus

```
# Geocoding requests
maps_geocoding_requests_total{status="success|error|cached"}

# Places API requests
maps_places_requests_total{type="autocomplete|details"}

# Distance calculations
maps_distance_calculations_total

# API quota usage
maps_quota_used{api="geocoding|places|distance"}

# Cache hit rate
maps_cache_hit_rate
```

---

## 10. Costos de Google Maps

| API                 | Precio               | Free Tier         |
| ------------------- | -------------------- | ----------------- |
| Geocoding           | $5/1000 requests     | 200/day free      |
| Places Autocomplete | $2.83/1000 (session) | 200/day free      |
| Places Details      | $17/1000             | 200/day free      |
| Distance Matrix     | $5/1000 elements     | 200/day free      |
| Maps JavaScript     | $7/1000 loads        | 28,000/month free |

**Optimizaciones:**

- Session tokens para Places (reduce costo 60%)
- Cache agresivo de geocoding (30 días)
- Lazy loading de mapas
- Static maps para emails

---

## 📚 Referencias

- [Google Maps Platform](https://developers.google.com/maps) - Documentación
- [Places API](https://developers.google.com/maps/documentation/places/web-service) - Places
- [Geocoding API](https://developers.google.com/maps/documentation/geocoding) - Geocoding
- [PostGIS](https://postgis.net/) - Spatial queries
