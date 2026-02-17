# 🗺️ Google Maps API - Documentación Técnica

**API Provider:** Google Cloud  
**Versión:** v3  
**Tipo:** Geolocation & Maps Service  
**Status en OKLA:** 🚧 En Configuración (Q1 2026)  
**Última actualización:** Enero 15, 2026

---

## 📋 Descripción General

**Google Maps API** se utiliza para:

- Mostrar ubicación de vehículos en mapa
- Búsqueda de dealers cercanos
- Rutas/direcciones a dealerships
- Geolocalización del usuario
- Autocomplete de direcciones

**¿Por qué Google Maps?**

- ✅ **Mejor cobertura global**
- ✅ **UI familiar** para usuarios
- ✅ **Direcciones y rutas** precisas
- ✅ **Autocompletar direcciones**
- ✅ **Geolocation nativa**
- ✅ **Street View** para dealers

---

## 🔑 Autenticación

### Crear API Key en Google Cloud Console

1. Ir a [Google Cloud Console](https://console.cloud.google.com/)
2. Crear nuevo proyecto o usar existente
3. Ir a **APIs & Services** → **Credentials**
4. Crear nueva **API Key**
5. Restringir a Maps JavaScript API

### En appsettings.json

```json
{
  "GoogleMaps": {
    "ApiKey": "${GOOGLE_MAPS_API_KEY}",
    "ProjectId": "okla-marketplace"
  }
}
```

### En Frontend (.env)

```
VITE_GOOGLE_MAPS_KEY=AIzaSyxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
```

---

## 🔌 APIs Necesarias

### Maps JavaScript API

```html
<script src="https://maps.googleapis.com/maps/api/js?key=YOUR_API_KEY&libraries=places,geometry"></script>
```

### Endpoints REST

#### Geocoding (Dirección → Coordenadas)

```
GET https://maps.googleapis.com/maps/api/geocode/json?address={address}&key={API_KEY}
```

**Response:**

```json
{
  "results": [
    {
      "formatted_address": "123 Main St, New York, NY 10001, USA",
      "geometry": {
        "location": {
          "lat": 40.7128,
          "lng": -74.006
        },
        "location_type": "ROOFTOP"
      },
      "place_id": "ChIJD7fiBh9u5kcRYJSMaMOCCwQ"
    }
  ],
  "status": "OK"
}
```

#### Reverse Geocoding (Coordenadas → Dirección)

```
GET https://maps.googleapis.com/maps/api/geocode/json?latlng={lat},{lng}&key={API_KEY}
```

#### Places Autocomplete

```
GET https://maps.googleapis.com/maps/api/place/autocomplete/json?input={input}&key={API_KEY}
```

**Response:**

```json
{
  "predictions": [
    {
      "place_id": "ChIJD7fiBh9u5kcRYJSMaMOCCwQ",
      "description": "123 Main St, New York, NY, USA",
      "matched_substrings": [
        {
          "length": 3,
          "offset": 0
        }
      ]
    }
  ],
  "status": "OK"
}
```

#### Distance Matrix (Distancia entre puntos)

```
GET https://maps.googleapis.com/maps/api/distancematrix/json?origins={lat},{lng}&destinations={lat2},{lng2}&key={API_KEY}
```

---

## 💻 Implementación en C#/.NET

### Instalación del paquete

```bash
dotnet add package GoogleApi
```

### GoogleMapsService.cs

```csharp
using Google.Apis.Customsearch.v1;
using Google.Maps;
using Google.Maps.Geocoding;
using Google.Maps.DistanceMatrix;
using Microsoft.Extensions.Logging;

namespace VehiclesSaleService.Infrastructure.Services;

public class GoogleMapsService : IGeoLocationService
{
    private readonly string _apiKey;
    private readonly ILogger<GoogleMapsService> _logger;

    public GoogleMapsService(string apiKey, ILogger<GoogleMapsService> logger)
    {
        _apiKey = apiKey;
        _logger = logger;
        GoogleSigned.AssignAllKeysFromEnvironment();
    }

    // ✅ Geocoding: Dirección → Coordenadas
    public async Task<Result<GeoLocation>> GeocodeAddressAsync(
        string address,
        CancellationToken ct = default)
    {
        try
        {
            var request = new GeocodingRequest
            {
                Address = address
            };

            var response = await request.GetResponseAsync();

            if (response.Status != ServiceResponseStatus.Ok)
            {
                return Result<GeoLocation>.Failure($"Geocoding failed: {response.Status}");
            }

            var result = response.Results.FirstOrDefault();
            if (result == null)
            {
                return Result<GeoLocation>.Failure("Address not found");
            }

            var location = new GeoLocation
            {
                Latitude = result.Geometry.Location.Latitude,
                Longitude = result.Geometry.Location.Longitude,
                FormattedAddress = result.FormattedAddress,
                PlaceId = result.PlaceId
            };

            _logger.LogInformation($"Geocoded {address} to {location.Latitude}, {location.Longitude}");
            return Result<GeoLocation>.Success(location);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception geocoding address");
            return Result<GeoLocation>.Failure($"Error: {ex.Message}");
        }
    }

    // ✅ Reverse Geocoding: Coordenadas → Dirección
    public async Task<Result<string>> ReverseGeocodeAsync(
        double latitude,
        double longitude,
        CancellationToken ct = default)
    {
        try
        {
            var request = new GeocodingRequest
            {
                Location = new Location(latitude, longitude)
            };

            var response = await request.GetResponseAsync();

            if (response.Status != ServiceResponseStatus.Ok)
            {
                return Result<string>.Failure($"Reverse geocoding failed: {response.Status}");
            }

            var address = response.Results.FirstOrDefault()?.FormattedAddress ?? "Unknown";

            _logger.LogInformation($"Reverse geocoded {latitude}, {longitude} to {address}");
            return Result<string>.Success(address);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception reverse geocoding");
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    // ✅ Calcular distancia entre puntos
    public async Task<Result<DistanceInfo>> GetDistanceAsync(
        double originLat,
        double originLng,
        double destinationLat,
        double destinationLng,
        CancellationToken ct = default)
    {
        try
        {
            var request = new DistanceMatrixRequest
            {
                Origins = new[] { new Location(originLat, originLng) },
                Destinations = new[] { new Location(destinationLat, destinationLng) }
            };

            var response = await request.GetResponseAsync();

            if (response.Status != ServiceResponseStatus.Ok)
            {
                return Result<DistanceInfo>.Failure($"Distance calculation failed: {response.Status}");
            }

            var row = response.Rows.FirstOrDefault();
            var element = row?.Elements.FirstOrDefault();

            if (element == null || element.Status != ServiceResponseStatus.Ok)
            {
                return Result<DistanceInfo>.Failure("Could not calculate distance");
            }

            var distanceInfo = new DistanceInfo
            {
                DistanceMeters = element.Distance.Value,
                DistanceKm = element.Distance.Value / 1000.0,
                DurationSeconds = element.Duration.Value,
                DurationMinutes = element.Duration.Value / 60,
                DurationText = element.Duration.Text
            };

            _logger.LogInformation($"Distance: {distanceInfo.DistanceKm}km in {distanceInfo.DurationText}");
            return Result<DistanceInfo>.Success(distanceInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception calculating distance");
            return Result<DistanceInfo>.Failure($"Error: {ex.Message}");
        }
    }

    // ✅ Encontrar dealers cercanos
    public async Task<Result<List<NearbyLocation>>> FindNearbyAsync(
        double latitude,
        double longitude,
        int radiusMeters = 5000,
        CancellationToken ct = default)
    {
        try
        {
            // Usar Places API para buscar dealers cercanos
            // Este es un ejemplo - requiere configuración adicional

            var nearbyLocations = new List<NearbyLocation>();

            _logger.LogInformation($"Found {nearbyLocations.Count} dealers within {radiusMeters}m");
            return Result<List<NearbyLocation>>.Success(nearbyLocations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception finding nearby dealers");
            return Result<List<NearbyLocation>>.Failure($"Error: {ex.Message}");
        }
    }
}

// DTOs
public class GeoLocation
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string FormattedAddress { get; set; }
    public string PlaceId { get; set; }
}

public class DistanceInfo
{
    public int DistanceMeters { get; set; }
    public double DistanceKm => DistanceMeters / 1000.0;
    public int DurationSeconds { get; set; }
    public int DurationMinutes => DurationSeconds / 60;
    public string DurationText { get; set; }
}

public class NearbyLocation
{
    public string Name { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double Distance { get; set; } // en km
    public string Address { get; set; }
}
```

---

## 🎯 Casos de Uso en OKLA

### 1. Mostrar Mapa de Ubicación del Vehículo

```csharp
var vehicle = await _vehicleService.GetAsync(vehicleId);
var location = await _geoService.GeocodeAddressAsync(vehicle.Location);

return new VehicleDetailDto
{
    // ...
    Location = vehicle.Location,
    Latitude = location.Data.Latitude,
    Longitude = location.Data.Longitude,
    MapUrl = $"https://maps.google.com/?q={location.Data.Latitude},{location.Data.Longitude}"
};
```

### 2. Buscar Dealers Cercanos

```csharp
var userLocation = await _geoService.GeocodeAddressAsync(userAddress);
var nearbyDealers = await _geoService.FindNearbyAsync(
    userLocation.Data.Latitude,
    userLocation.Data.Longitude,
    radiusMeters: 10000); // 10km

return nearbyDealers.Data;
```

### 3. Calcular Distancia a Dealer

```csharp
var distanceResult = await _geoService.GetDistanceAsync(
    originLat: buyerLocation.Latitude,
    originLng: buyerLocation.Longitude,
    destinationLat: dealerLocation.Latitude,
    destinationLng: dealerLocation.Longitude);

return new DealerProximityDto
{
    DealerName = dealer.Name,
    DistanceKm = distanceResult.Data.DistanceKm,
    DurationText = distanceResult.Data.DurationText
};
```

---

## 🔐 Seguridad y Best Practices

### ✅ Do's

- ✅ **Restringir API key** a Maps JavaScript API
- ✅ **Usar referrer restrictions** (okla.com.do)
- ✅ **Monitorear uso** en Cloud Console
- ✅ **Cachear resultados** en Redis

### ❌ Don'ts

- ❌ **NO usar API key en frontend** sin restricciones
- ❌ **NO dejar API key en código**
- ❌ **NO hacer requests directos** desde navegador
- ❌ **NO ignorar quotas** (2.5K gratuitos/día)

---

## 💰 Costos

| Feature             | Costo                   | Uso OKLA |
| ------------------- | ----------------------- | -------- |
| **Maps Embed**      | Free                    | Sí       |
| **Maps JavaScript** | Free (primeros 28K/mes) | Sí       |
| **Geocoding**       | $0.005/request          | Sí       |
| **Distance Matrix** | $0.005/element          | Sí       |
| **Places**          | $0.017                  | Sí       |

**Costo OKLA (Enero 2026):** $0 (bajo volumen, dentro de free tier)

---

**Mantenido por:** Frontend Team  
**Última revisión:** Enero 15, 2026
