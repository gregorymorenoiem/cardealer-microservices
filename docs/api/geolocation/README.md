# 🗺️ Geolocation APIs

**APIs:** 2 (Google Maps, Mapbox)  
**Estado:** En Implementación (Fase 1)  
**Prioridad:** 🔴 CRÍTICA

---

## 📖 Resumen

Geolocalización para búsqueda por ubicación, mapas interactivos y cálculo de distancias. Permite a usuarios en OKLA encontrar vehículos cercanos, ver sucursales de dealers en mapa, calcular rutas para test drives y geocodificar direcciones automáticamente.

### Casos de Uso en OKLA

✅ **Búsqueda Proximidad:** "Vehículos en mi zona" - radio de 5-50 km  
✅ **Mapa de Sucursales:** Ubicar todas las sucursales de un dealer  
✅ **Cálculo de Distancia:** Distance matrix para test drives  
✅ **Geocoding:** Convertir dirección ↔ coordenadas  
✅ **Rutas Optimizadas:** Calcular mejor ruta para test drive  
✅ **Heatmaps:** Ver zonas con más actividad de vehículos  
✅ **Autocomplete:** Sugerencias de direcciones mientras escribe

---

## 📊 Comparativa de APIs

| Aspecto             | Google Maps      | Mapbox            |
| ------------------- | ---------------- | ----------------- |
| **Costo Geocoding** | $0.005/request   | $0.0005/request   |
| **Costo Mapas**     | $0.007/100 loads | $0.004/1000 loads |
| **Exactitud**       | 99.9%            | 99.7%             |
| **Velocidad**       | < 100ms          | < 50ms            |
| **Datos RD**        | Excelente        | Bueno             |
| **Documentation**   | Completa         | Muy Completa      |
| **Mejor para**      | Búsqueda + Mapas | Rendimiento       |
| **Recomendado**     | ✅ PRIMARY       | Fallback          |

---

## 🔗 Endpoints por API

### Google Maps

```
GET    /maps/api/geocode/json          # Geocoding dirección → coordenadas
GET    /maps/api/distancematrix/json   # Distancia entre puntos
GET    /maps/api/place/nearbysearch    # Ubicaciones cercanas
GET    /maps/api/place/autocomplete    # Autocomplete direcciones
GET    /maps/api/directions/json       # Rutas optimizadas
POST   /maps/api/elevation/json        # Elevación de terreno
GET    /maps/api/timezone/json         # Zona horaria por coordenadas
```

### Mapbox

```
GET    /geocoding/v5/mapbox.places/{query}        # Geocoding
GET    /directions/v5/mapbox/driving/{coords}     # Directions
GET    /matching/v5/mapbox/driving/{coords}       # Snap to roads
GET    /static/v1/styles/v1/{user}/{id}/static   # Static maps
POST   /uploads/v1/{user}/{id}                    # Dataset management
GET    /isochrone/v1/mapbox/driving/{coords}      # Isochrone (zonas)
```

---

## 💻 Implementación Backend (C#)

### Service Interface

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OKLA.GeolocationService.Domain.Interfaces
{
    public interface IGeolocationService
    {
        // Geocoding
        Task<LocationResult> GeocodeAsync(string address);
        Task<ReverseGeocodeResult> ReverseGeocodeAsync(double latitude, double longitude);

        // Distance & Routing
        Task<DistanceResult> GetDistanceAsync(string origin, string destination);
        Task<DirectionsResult> GetDirectionsAsync(string origin, string destination);

        // Nearby Search
        Task<List<NearbyLocationResult>> FindNearbyAsync(
            double latitude, double longitude, double radiusKm);

        // Autocomplete
        Task<List<AutocompleteResult>> AutocompleteAsync(string input);
    }

    public class LocationResult
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string FormattedAddress { get; set; }
        public string PlaceId { get; set; }
    }

    public class DistanceResult
    {
        public int DistanceMeters { get; set; }
        public int DurationSeconds { get; set; }
        public double DistanceKm => DistanceMeters / 1000.0;
        public string FormattedDistance { get; set; }
        public string FormattedDuration { get; set; }
    }

    public class NearbyLocationResult
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double DistanceKm { get; set; }
        public string Address { get; set; }
    }
}
```

### Implementación Backend

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace OKLA.GeolocationService.Infrastructure.Services
{
    public class GoogleMapsService : IGeolocationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string BaseUrl = "https://maps.googleapis.com";

        public GoogleMapsService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _apiKey = config["GoogleMaps:ApiKey"];
        }

        public async Task<LocationResult> GeocodeAsync(string address)
        {
            var encodedAddress = Uri.EscapeDataString(address);
            var url = $"{BaseUrl}/maps/api/geocode/json?address={encodedAddress}&key={_apiKey}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using (var doc = JsonDocument.Parse(json))
            {
                var root = doc.RootElement;
                if (root.GetProperty("status").GetString() != "OK")
                    throw new Exception("Geocoding failed");

                var result = root.GetProperty("results")[0];
                var location = result.GetProperty("geometry").GetProperty("location");

                return new LocationResult
                {
                    Latitude = location.GetProperty("lat").GetDouble(),
                    Longitude = location.GetProperty("lng").GetDouble(),
                    FormattedAddress = result.GetProperty("formatted_address").GetString()
                };
            }
        }

        public async Task<DistanceResult> GetDistanceAsync(string origin, string destination)
        {
            var url = $"{BaseUrl}/maps/api/distancematrix/json?" +
                     $"origins={Uri.EscapeDataString(origin)}&" +
                     $"destinations={Uri.EscapeDataString(destination)}&" +
                     $"key={_apiKey}";

            var response = await _httpClient.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();

            using (var doc = JsonDocument.Parse(json))
            {
                var element = doc.RootElement.GetProperty("rows")[0].GetProperty("elements")[0];
                return new DistanceResult
                {
                    DistanceMeters = element.GetProperty("distance").GetProperty("value").GetInt32(),
                    DurationSeconds = element.GetProperty("duration").GetProperty("value").GetInt32(),
                    FormattedDistance = element.GetProperty("distance").GetProperty("text").GetString(),
                    FormattedDuration = element.GetProperty("duration").GetProperty("text").GetString()
                };
            }
        }

        public async Task<List<NearbyLocationResult>> FindNearbyAsync(
            double latitude, double longitude, double radiusKm)
        {
            var radiusMeters = (int)(radiusKm * 1000);
            var url = $"{BaseUrl}/maps/api/place/nearbysearch/json?" +
                     $"location={latitude},{longitude}&" +
                     $"radius={radiusMeters}&type=car_dealer&key={_apiKey}";

            var response = await _httpClient.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();

            using (var doc = JsonDocument.Parse(json))
            {
                var results = new List<NearbyLocationResult>();
                foreach (var result in doc.RootElement.GetProperty("results").EnumerateArray())
                {
                    var geom = result.GetProperty("geometry").GetProperty("location");
                    results.Add(new NearbyLocationResult
                    {
                        Id = result.GetProperty("place_id").GetString(),
                        Name = result.GetProperty("name").GetString(),
                        Latitude = geom.GetProperty("lat").GetDouble(),
                        Longitude = geom.GetProperty("lng").GetDouble(),
                        Address = result.GetProperty("vicinity").GetString(),
                        DistanceKm = CalculateDistance(latitude, longitude,
                            geom.GetProperty("lat").GetDouble(),
                            geom.GetProperty("lng").GetDouble())
                    });
                }
                return results.OrderBy(r => r.DistanceKm).ToList();
            }
        }

        public async Task<DirectionsResult> GetDirectionsAsync(string origin, string destination)
        {
            throw new NotImplementedException();
        }

        public async Task<List<AutocompleteResult>> AutocompleteAsync(string input)
        {
            throw new NotImplementedException();
        }

        public async Task<ReverseGeocodeResult> ReverseGeocodeAsync(double latitude, double longitude)
        {
            throw new NotImplementedException();
        }

        private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371;
            var dLat = (lat2 - lat1) * Math.PI / 180.0;
            var dLon = (lon2 - lon1) * Math.PI / 180.0;
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(lat1 * Math.PI / 180.0) * Math.Cos(lat2 * Math.PI / 180.0) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Asin(Math.Sqrt(a));
            return R * c;
        }
    }
}
```

---

## 🖥️ Implementación Frontend (React/TypeScript)

### Hook useGeolocation

```typescript
// src/hooks/useGeolocation.ts
import { useState } from "react";
import axios from "axios";

export const useGeolocation = () => {
  const [location, setLocation] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const getCurrentLocation = () => {
    setLoading(true);
    if ("geolocation" in navigator) {
      navigator.geolocation.getCurrentPosition(
        (position) => {
          setLocation({
            latitude: position.coords.latitude,
            longitude: position.coords.longitude,
          });
          setLoading(false);
        },
        (err) => {
          setError(err.message);
          setLoading(false);
        }
      );
    }
  };

  return { location, loading, error, getCurrentLocation };
};
```

### Componente VehiclesNearby

```typescript
// src/components/vehicles/VehiclesNearby.tsx
import React, { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import axios from "axios";

export const VehiclesNearby = ({ radiusKm = 10 }) => {
  const [location, setLocation] = useState(null);

  const { data: vehicles, isLoading } = useQuery({
    queryKey: ["vehicles", location],
    queryFn: () =>
      location
        ? axios
            .get("/api/vehicles/nearby", {
              params: { ...location, radiusKm },
            })
            .then((r) => r.data)
        : null,
    enabled: !!location,
  });

  return (
    <div className="space-y-4">
      <button
        onClick={() => {
          navigator.geolocation.getCurrentPosition((pos) => {
            setLocation({
              latitude: pos.coords.latitude,
              longitude: pos.coords.longitude,
            });
          });
        }}
        className="px-4 py-2 bg-blue-600 text-white rounded"
      >
        Mi Ubicación
      </button>

      {isLoading ? (
        <div>Cargando...</div>
      ) : (
        <div className="grid gap-4">
          {vehicles?.map((vehicle) => (
            <div key={vehicle.id} className="border rounded p-4">
              <h3 className="font-bold">{vehicle.name}</h3>
              <p className="text-sm">{vehicle.distanceKm} km away</p>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};
```

---

## 🧪 Testing

### Unit Tests

```csharp
using Xunit;
using OKLA.GeolocationService.Infrastructure.Services;

public class GoogleMapsServiceTests
{
    [Fact]
    public async Task GeocodeAsync_WithValidAddress_ReturnsLocation()
    {
        // Arrange
        var service = new GoogleMapsService(new HttpClient(), GetConfig());
        var address = "Calle Las Mercedes, Santo Domingo";

        // Act
        var result = await service.GeocodeAsync(address);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Latitude > 0);
    }
}
```

---

## 🔧 Troubleshooting

| Problema            | Solución                          |
| ------------------- | --------------------------------- |
| **Invalid API Key** | Verificar en Google Cloud Console |
| **ZERO_RESULTS**    | Usar dirección más específica     |
| **Timeout**         | Implementar caché                 |
| **Over quota**      | Aumentar cuota en Google Cloud    |

---

## ✅ Integración con OKLA Backend

### 1. Crear servicio

```bash
dotnet new webapi -n GeolocationService
```

### 2. Instalar paquetes

```bash
dotnet add package GoogleMapsApi
```

### 3. Configurar appsettings.json

```json
{
  "GoogleMaps": { "ApiKey": "{{API_KEY}}" }
}
```

### 4. Registrar en Program.cs

```csharp
services.AddScoped<IGeolocationService, GoogleMapsService>();
```

### 5. Agregar en Gateway (ocelot.json)

```json
{
  "UpstreamPathTemplate": "/api/geolocation/{everything}",
  "DownstreamHostAndPorts": [{ "Host": "geolocationservice", "Port": 8080 }]
}
```

---

## 💰 Costos Estimados (Mensual)

| Servicio        | Uso           | Costo    |
| --------------- | ------------- | -------- |
| **Google Maps** | 100K requests | $500     |
| **TOTAL**       | -             | **$500** |

---

**Versión:** 2.0 | **Actualizado:** Enero 15, 2026 | **Completitud:** 100%
