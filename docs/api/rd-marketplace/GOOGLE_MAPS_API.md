# 🗺️ Google Maps Platform API

**Proveedor:** Google Cloud Platform  
**Website:** [cloud.google.com/maps-platform](https://cloud.google.com/maps-platform)  
**Uso:** Geolocalización, mapas, direcciones, dealers cercanos  
**Prioridad:** ⭐⭐⭐⭐⭐ CRÍTICA

---

## 📋 Información General

| API | Uso | Costo (por 1000) |
|-----|-----|-----------------|
| **Maps JavaScript** | Mapa interactivo | $7.00 |
| **Geocoding** | Dirección ↔ Coordenadas | $5.00 |
| **Places** | Búsqueda de lugares | $17.00 |
| **Distance Matrix** | Distancias/tiempos | $5.00 |
| **Directions** | Rutas | $5.00 |
| **Static Maps** | Imágenes estáticas | $2.00 |

**Crédito Mensual:** $200 USD gratis

---

## 🌐 API Endpoints

### Geocoding (Dirección → Coordenadas)

```http
GET https://maps.googleapis.com/maps/api/geocode/json
  ?address=Av.+Winston+Churchill+esq.+Roberto+Pastoriza,+Santo+Domingo
  &key=YOUR_API_KEY

# Response
{
  "results": [{
    "formatted_address": "Av. Winston Churchill, Santo Domingo, República Dominicana",
    "geometry": {
      "location": {
        "lat": 18.4663,
        "lng": -69.9398
      },
      "location_type": "ROOFTOP",
      "viewport": {
        "northeast": { "lat": 18.4676, "lng": -69.9384 },
        "southwest": { "lat": 18.4649, "lng": -69.9411 }
      }
    },
    "place_id": "ChIJxxxxxxxxxxxxxx",
    "address_components": [
      { "long_name": "Winston Churchill", "short_name": "W Churchill", "types": ["route"] },
      { "long_name": "Distrito Nacional", "short_name": "DN", "types": ["administrative_area_level_2"] },
      { "long_name": "Santo Domingo", "short_name": "SD", "types": ["locality"] }
    ]
  }],
  "status": "OK"
}
```

### Reverse Geocoding (Coordenadas → Dirección)

```http
GET https://maps.googleapis.com/maps/api/geocode/json
  ?latlng=18.4663,-69.9398
  &key=YOUR_API_KEY

# Response
{
  "results": [{
    "formatted_address": "Av. Winston Churchill 1015, Santo Domingo 10148, República Dominicana",
    "place_id": "ChIJxxxxxx"
  }],
  "status": "OK"
}
```

### Places Search (Buscar Dealers/Talleres)

```http
# Nearby Search
GET https://maps.googleapis.com/maps/api/place/nearbysearch/json
  ?location=18.4663,-69.9398
  &radius=5000
  &type=car_dealer
  &key=YOUR_API_KEY

# Response
{
  "results": [
    {
      "name": "Toyota de Santo Domingo",
      "place_id": "ChIJxxxxxx",
      "geometry": {
        "location": { "lat": 18.4700, "lng": -69.9350 }
      },
      "vicinity": "Av. 27 de Febrero esq. Tiradentes",
      "rating": 4.3,
      "user_ratings_total": 245,
      "opening_hours": { "open_now": true },
      "photos": [{ "photo_reference": "ATtYBwJxxxxxxx" }]
    }
  ],
  "status": "OK"
}

# Text Search
GET https://maps.googleapis.com/maps/api/place/textsearch/json
  ?query=dealers+de+autos+usados+en+Santiago+Republica+Dominicana
  &key=YOUR_API_KEY
```

### Place Details

```http
GET https://maps.googleapis.com/maps/api/place/details/json
  ?place_id=ChIJxxxxxx
  &fields=name,formatted_address,formatted_phone_number,website,opening_hours,rating,reviews
  &key=YOUR_API_KEY

# Response
{
  "result": {
    "name": "Toyota de Santo Domingo",
    "formatted_address": "Av. 27 de Febrero #123, Santo Domingo",
    "formatted_phone_number": "(809) 566-1234",
    "website": "https://toyota.com.do",
    "rating": 4.3,
    "opening_hours": {
      "weekday_text": [
        "Monday: 8:00 AM – 6:00 PM",
        "Tuesday: 8:00 AM – 6:00 PM"
      ]
    },
    "reviews": [
      {
        "author_name": "Juan Pérez",
        "rating": 5,
        "text": "Excelente servicio, muy profesionales",
        "time": 1704067200
      }
    ]
  },
  "status": "OK"
}
```

### Distance Matrix (Distancias entre puntos)

```http
GET https://maps.googleapis.com/maps/api/distancematrix/json
  ?origins=18.4663,-69.9398
  &destinations=18.5000,-69.9000|19.4500,-70.6900
  &mode=driving
  &key=YOUR_API_KEY

# Response
{
  "rows": [{
    "elements": [
      {
        "distance": { "text": "8.5 km", "value": 8500 },
        "duration": { "text": "18 mins", "value": 1080 },
        "duration_in_traffic": { "text": "25 mins", "value": 1500 },
        "status": "OK"
      },
      {
        "distance": { "text": "155 km", "value": 155000 },
        "duration": { "text": "2 hours 30 mins", "value": 9000 },
        "status": "OK"
      }
    ]
  }],
  "status": "OK"
}
```

### Directions (Rutas)

```http
GET https://maps.googleapis.com/maps/api/directions/json
  ?origin=18.4663,-69.9398
  &destination=18.5000,-69.9000
  &mode=driving
  &alternatives=true
  &key=YOUR_API_KEY

# Response
{
  "routes": [{
    "summary": "Av. 27 de Febrero",
    "legs": [{
      "distance": { "text": "8.5 km", "value": 8500 },
      "duration": { "text": "18 mins", "value": 1080 },
      "start_address": "Av. Winston Churchill, Santo Domingo",
      "end_address": "Av. Abraham Lincoln, Santo Domingo",
      "steps": [
        {
          "html_instructions": "Dirígete hacia el <b>este</b> por <b>Av. Winston Churchill</b>",
          "distance": { "text": "0.3 km", "value": 300 },
          "duration": { "text": "1 min", "value": 60 }
        }
      ]
    }],
    "overview_polyline": {
      "points": "q~iiDnlvbNcBgC..."
    }
  }],
  "status": "OK"
}
```

### Place Autocomplete

```http
GET https://maps.googleapis.com/maps/api/place/autocomplete/json
  ?input=av+winston+churchill+santo
  &components=country:do
  &types=address
  &key=YOUR_API_KEY

# Response
{
  "predictions": [
    {
      "description": "Av. Winston Churchill, Santo Domingo, República Dominicana",
      "place_id": "ChIJxxxxxx",
      "structured_formatting": {
        "main_text": "Av. Winston Churchill",
        "secondary_text": "Santo Domingo, República Dominicana"
      }
    }
  ],
  "status": "OK"
}
```

---

## 💻 Modelos C#

```csharp
namespace LocationService.Domain.Entities;

/// <summary>
/// Coordenadas geográficas
/// </summary>
public record GeoCoordinates(
    double Latitude,
    double Longitude
);

/// <summary>
/// Dirección geocodificada
/// </summary>
public record GeocodedAddress(
    string FormattedAddress,
    GeoCoordinates Coordinates,
    string PlaceId,
    AddressComponents Components,
    string? PlusCode
);

public record AddressComponents(
    string? StreetNumber,
    string? Route,
    string? Neighborhood,
    string? City,
    string? Province,
    string? Country,
    string? PostalCode
);

/// <summary>
/// Lugar encontrado
/// </summary>
public record Place(
    string PlaceId,
    string Name,
    string Address,
    GeoCoordinates Coordinates,
    decimal? Rating,
    int? TotalReviews,
    bool? IsOpenNow,
    string? PhoneNumber,
    string? Website,
    List<string>? PhotoReferences,
    PlaceType Type
);

public enum PlaceType
{
    CarDealer,
    CarRepair,
    CarWash,
    GasStation,
    ParkingLot,
    InsuranceAgency,
    Bank,
    Other
}

/// <summary>
/// Distancia/Tiempo entre puntos
/// </summary>
public record DistanceInfo(
    GeoCoordinates Origin,
    GeoCoordinates Destination,
    int DistanceMeters,
    string DistanceText,
    int DurationSeconds,
    string DurationText,
    int? DurationInTrafficSeconds,
    string? DurationInTrafficText
);

/// <summary>
/// Ruta calculada
/// </summary>
public record Route(
    string Summary,
    int TotalDistanceMeters,
    int TotalDurationSeconds,
    GeoCoordinates StartLocation,
    GeoCoordinates EndLocation,
    string StartAddress,
    string EndAddress,
    List<RouteStep> Steps,
    string EncodedPolyline
);

public record RouteStep(
    string Instruction,
    int DistanceMeters,
    int DurationSeconds,
    GeoCoordinates StartLocation,
    GeoCoordinates EndLocation
);

/// <summary>
/// Sugerencia de autocompletado
/// </summary>
public record AddressSuggestion(
    string PlaceId,
    string Description,
    string MainText,
    string SecondaryText
);

/// <summary>
/// Dealer cercano con distancia
/// </summary>
public record NearbyDealer(
    Guid DealerId,
    string DealerName,
    string Address,
    GeoCoordinates Coordinates,
    DistanceInfo Distance,
    decimal? Rating,
    int VehicleCount,
    bool IsVerified
);
```

---

## 🔧 Service Interface

```csharp
namespace LocationService.Domain.Interfaces;

public interface ILocationService
{
    /// <summary>
    /// Geocodifica dirección a coordenadas
    /// </summary>
    Task<GeocodedAddress?> GeocodeAddressAsync(string address);

    /// <summary>
    /// Reverse geocoding: coordenadas a dirección
    /// </summary>
    Task<GeocodedAddress?> ReverseGeocodeAsync(GeoCoordinates coordinates);

    /// <summary>
    /// Busca lugares cercanos por tipo
    /// </summary>
    Task<List<Place>> SearchNearbyPlacesAsync(
        GeoCoordinates location,
        int radiusMeters,
        PlaceType type);

    /// <summary>
    /// Búsqueda de texto libre
    /// </summary>
    Task<List<Place>> SearchPlacesByTextAsync(string query);

    /// <summary>
    /// Obtiene detalles de un lugar
    /// </summary>
    Task<Place?> GetPlaceDetailsAsync(string placeId);

    /// <summary>
    /// Calcula distancias desde un origen a múltiples destinos
    /// </summary>
    Task<List<DistanceInfo>> GetDistancesAsync(
        GeoCoordinates origin,
        List<GeoCoordinates> destinations);

    /// <summary>
    /// Obtiene ruta entre dos puntos
    /// </summary>
    Task<Route?> GetDirectionsAsync(
        GeoCoordinates origin,
        GeoCoordinates destination,
        bool alternatives = false);

    /// <summary>
    /// Autocompletado de direcciones
    /// </summary>
    Task<List<AddressSuggestion>> AutocompleteAddressAsync(
        string input,
        string? sessionToken = null);

    /// <summary>
    /// Busca dealers cercanos con distancia
    /// </summary>
    Task<List<NearbyDealer>> GetNearbyDealersAsync(
        GeoCoordinates userLocation,
        int radiusKm = 50,
        int maxResults = 20);

    /// <summary>
    /// Obtiene URL de imagen de mapa estático
    /// </summary>
    string GetStaticMapUrl(
        GeoCoordinates center,
        int zoom = 15,
        int width = 600,
        int height = 400,
        List<GeoCoordinates>? markers = null);
}
```

---

## 🏗️ Service Implementation

```csharp
namespace LocationService.Infrastructure.Services;

public class GoogleMapsService : ILocationService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<GoogleMapsService> _logger;
    private readonly IMemoryCache _cache;
    private readonly IDealerRepository _dealerRepo;

    private readonly string _apiKey;

    public GoogleMapsService(
        HttpClient httpClient,
        IConfiguration config,
        ILogger<GoogleMapsService> logger,
        IMemoryCache cache,
        IDealerRepository dealerRepo)
    {
        _httpClient = httpClient;
        _config = config;
        _logger = logger;
        _cache = cache;
        _dealerRepo = dealerRepo;

        _apiKey = config["GoogleMaps:ApiKey"]!;
        _httpClient.BaseAddress = new Uri("https://maps.googleapis.com/maps/api/");
    }

    public async Task<GeocodedAddress?> GeocodeAddressAsync(string address)
    {
        var cacheKey = $"geocode:{address}";
        
        if (_cache.TryGetValue(cacheKey, out GeocodedAddress? cached))
            return cached;

        var url = $"geocode/json?address={Uri.EscapeDataString(address)}&key={_apiKey}";
        var response = await _httpClient.GetFromJsonAsync<GeocodeResponse>(url);

        if (response?.Status != "OK" || !response.Results.Any())
            return null;

        var result = response.Results.First();
        var geocoded = new GeocodedAddress(
            FormattedAddress: result.FormattedAddress,
            Coordinates: new GeoCoordinates(
                result.Geometry.Location.Lat,
                result.Geometry.Location.Lng),
            PlaceId: result.PlaceId,
            Components: ParseAddressComponents(result.AddressComponents),
            PlusCode: result.PlusCode?.GlobalCode
        );

        _cache.Set(cacheKey, geocoded, TimeSpan.FromDays(30));
        return geocoded;
    }

    public async Task<GeocodedAddress?> ReverseGeocodeAsync(GeoCoordinates coordinates)
    {
        var cacheKey = $"reverse:{coordinates.Latitude},{coordinates.Longitude}";
        
        if (_cache.TryGetValue(cacheKey, out GeocodedAddress? cached))
            return cached;

        var url = $"geocode/json?latlng={coordinates.Latitude},{coordinates.Longitude}&key={_apiKey}";
        var response = await _httpClient.GetFromJsonAsync<GeocodeResponse>(url);

        if (response?.Status != "OK" || !response.Results.Any())
            return null;

        var result = response.Results.First();
        var geocoded = new GeocodedAddress(
            FormattedAddress: result.FormattedAddress,
            Coordinates: coordinates,
            PlaceId: result.PlaceId,
            Components: ParseAddressComponents(result.AddressComponents),
            PlusCode: null
        );

        _cache.Set(cacheKey, geocoded, TimeSpan.FromDays(30));
        return geocoded;
    }

    public async Task<List<Place>> SearchNearbyPlacesAsync(
        GeoCoordinates location,
        int radiusMeters,
        PlaceType type)
    {
        var googleType = type switch
        {
            PlaceType.CarDealer => "car_dealer",
            PlaceType.CarRepair => "car_repair",
            PlaceType.CarWash => "car_wash",
            PlaceType.GasStation => "gas_station",
            PlaceType.ParkingLot => "parking",
            PlaceType.InsuranceAgency => "insurance_agency",
            PlaceType.Bank => "bank",
            _ => "establishment"
        };

        var url = $"place/nearbysearch/json?location={location.Latitude},{location.Longitude}" +
                  $"&radius={radiusMeters}&type={googleType}&key={_apiKey}";

        var response = await _httpClient.GetFromJsonAsync<PlaceSearchResponse>(url);

        if (response?.Status != "OK")
            return new List<Place>();

        return response.Results.Select(r => new Place(
            PlaceId: r.PlaceId,
            Name: r.Name,
            Address: r.Vicinity ?? r.FormattedAddress ?? "",
            Coordinates: new GeoCoordinates(r.Geometry.Location.Lat, r.Geometry.Location.Lng),
            Rating: r.Rating,
            TotalReviews: r.UserRatingsTotal,
            IsOpenNow: r.OpeningHours?.OpenNow,
            PhoneNumber: null,
            Website: null,
            PhotoReferences: r.Photos?.Select(p => p.PhotoReference).ToList(),
            Type: type
        )).ToList();
    }

    public async Task<List<Place>> SearchPlacesByTextAsync(string query)
    {
        var url = $"place/textsearch/json?query={Uri.EscapeDataString(query)}&key={_apiKey}";
        var response = await _httpClient.GetFromJsonAsync<PlaceSearchResponse>(url);

        if (response?.Status != "OK")
            return new List<Place>();

        return response.Results.Select(r => new Place(
            PlaceId: r.PlaceId,
            Name: r.Name,
            Address: r.FormattedAddress ?? "",
            Coordinates: new GeoCoordinates(r.Geometry.Location.Lat, r.Geometry.Location.Lng),
            Rating: r.Rating,
            TotalReviews: r.UserRatingsTotal,
            IsOpenNow: r.OpeningHours?.OpenNow,
            PhoneNumber: null,
            Website: null,
            PhotoReferences: r.Photos?.Select(p => p.PhotoReference).ToList(),
            Type: PlaceType.Other
        )).ToList();
    }

    public async Task<Place?> GetPlaceDetailsAsync(string placeId)
    {
        var cacheKey = $"place:{placeId}";
        
        if (_cache.TryGetValue(cacheKey, out Place? cached))
            return cached;

        var fields = "name,formatted_address,geometry,formatted_phone_number,website,rating,user_ratings_total,opening_hours,photos";
        var url = $"place/details/json?place_id={placeId}&fields={fields}&key={_apiKey}";

        var response = await _httpClient.GetFromJsonAsync<PlaceDetailsResponse>(url);

        if (response?.Status != "OK" || response.Result == null)
            return null;

        var r = response.Result;
        var place = new Place(
            PlaceId: placeId,
            Name: r.Name,
            Address: r.FormattedAddress ?? "",
            Coordinates: new GeoCoordinates(
                r.Geometry?.Location.Lat ?? 0,
                r.Geometry?.Location.Lng ?? 0),
            Rating: r.Rating,
            TotalReviews: r.UserRatingsTotal,
            IsOpenNow: r.OpeningHours?.OpenNow,
            PhoneNumber: r.FormattedPhoneNumber,
            Website: r.Website,
            PhotoReferences: r.Photos?.Select(p => p.PhotoReference).ToList(),
            Type: PlaceType.Other
        );

        _cache.Set(cacheKey, place, TimeSpan.FromDays(7));
        return place;
    }

    public async Task<List<DistanceInfo>> GetDistancesAsync(
        GeoCoordinates origin,
        List<GeoCoordinates> destinations)
    {
        var destStr = string.Join("|", destinations.Select(d => $"{d.Latitude},{d.Longitude}"));
        var url = $"distancematrix/json?origins={origin.Latitude},{origin.Longitude}" +
                  $"&destinations={destStr}&mode=driving&departure_time=now&key={_apiKey}";

        var response = await _httpClient.GetFromJsonAsync<DistanceMatrixResponse>(url);

        if (response?.Status != "OK" || !response.Rows.Any())
            return new List<DistanceInfo>();

        var elements = response.Rows[0].Elements;
        var distances = new List<DistanceInfo>();

        for (int i = 0; i < elements.Count && i < destinations.Count; i++)
        {
            var el = elements[i];
            if (el.Status == "OK")
            {
                distances.Add(new DistanceInfo(
                    Origin: origin,
                    Destination: destinations[i],
                    DistanceMeters: el.Distance.Value,
                    DistanceText: el.Distance.Text,
                    DurationSeconds: el.Duration.Value,
                    DurationText: el.Duration.Text,
                    DurationInTrafficSeconds: el.DurationInTraffic?.Value,
                    DurationInTrafficText: el.DurationInTraffic?.Text
                ));
            }
        }

        return distances;
    }

    public async Task<Route?> GetDirectionsAsync(
        GeoCoordinates origin,
        GeoCoordinates destination,
        bool alternatives = false)
    {
        var url = $"directions/json?origin={origin.Latitude},{origin.Longitude}" +
                  $"&destination={destination.Latitude},{destination.Longitude}" +
                  $"&mode=driving&alternatives={alternatives}&key={_apiKey}";

        var response = await _httpClient.GetFromJsonAsync<DirectionsResponse>(url);

        if (response?.Status != "OK" || !response.Routes.Any())
            return null;

        var r = response.Routes.First();
        var leg = r.Legs.First();

        return new Route(
            Summary: r.Summary,
            TotalDistanceMeters: leg.Distance.Value,
            TotalDurationSeconds: leg.Duration.Value,
            StartLocation: new GeoCoordinates(
                leg.StartLocation.Lat, leg.StartLocation.Lng),
            EndLocation: new GeoCoordinates(
                leg.EndLocation.Lat, leg.EndLocation.Lng),
            StartAddress: leg.StartAddress,
            EndAddress: leg.EndAddress,
            Steps: leg.Steps.Select(s => new RouteStep(
                Instruction: StripHtml(s.HtmlInstructions),
                DistanceMeters: s.Distance.Value,
                DurationSeconds: s.Duration.Value,
                StartLocation: new GeoCoordinates(s.StartLocation.Lat, s.StartLocation.Lng),
                EndLocation: new GeoCoordinates(s.EndLocation.Lat, s.EndLocation.Lng)
            )).ToList(),
            EncodedPolyline: r.OverviewPolyline.Points
        );
    }

    public async Task<List<AddressSuggestion>> AutocompleteAddressAsync(
        string input,
        string? sessionToken = null)
    {
        var url = $"place/autocomplete/json?input={Uri.EscapeDataString(input)}" +
                  $"&components=country:do&types=address&key={_apiKey}";

        if (!string.IsNullOrEmpty(sessionToken))
            url += $"&sessiontoken={sessionToken}";

        var response = await _httpClient.GetFromJsonAsync<AutocompleteResponse>(url);

        if (response?.Status != "OK")
            return new List<AddressSuggestion>();

        return response.Predictions.Select(p => new AddressSuggestion(
            PlaceId: p.PlaceId,
            Description: p.Description,
            MainText: p.StructuredFormatting?.MainText ?? p.Description,
            SecondaryText: p.StructuredFormatting?.SecondaryText ?? ""
        )).ToList();
    }

    public async Task<List<NearbyDealer>> GetNearbyDealersAsync(
        GeoCoordinates userLocation,
        int radiusKm = 50,
        int maxResults = 20)
    {
        // Obtener dealers de nuestra base de datos
        var dealers = await _dealerRepo.GetActiveWithCoordinatesAsync();

        // Filtrar por radio
        var nearbyDealers = dealers
            .Where(d => d.Latitude.HasValue && d.Longitude.HasValue)
            .Select(d => new
            {
                Dealer = d,
                Distance = CalculateDistance(
                    userLocation,
                    new GeoCoordinates(d.Latitude!.Value, d.Longitude!.Value))
            })
            .Where(x => x.Distance <= radiusKm * 1000) // Metros
            .OrderBy(x => x.Distance)
            .Take(maxResults)
            .ToList();

        if (!nearbyDealers.Any())
            return new List<NearbyDealer>();

        // Obtener distancias reales con Google Maps
        var destinations = nearbyDealers
            .Select(x => new GeoCoordinates(
                x.Dealer.Latitude!.Value, 
                x.Dealer.Longitude!.Value))
            .ToList();

        var distances = await GetDistancesAsync(userLocation, destinations);

        return nearbyDealers.Select((x, i) => new NearbyDealer(
            DealerId: x.Dealer.Id,
            DealerName: x.Dealer.BusinessName,
            Address: x.Dealer.Address ?? "",
            Coordinates: new GeoCoordinates(
                x.Dealer.Latitude!.Value, 
                x.Dealer.Longitude!.Value),
            Distance: i < distances.Count ? distances[i] : new DistanceInfo(
                userLocation,
                new GeoCoordinates(x.Dealer.Latitude!.Value, x.Dealer.Longitude!.Value),
                (int)x.Distance,
                $"{x.Distance / 1000:F1} km",
                0, "N/A", null, null),
            Rating: x.Dealer.Rating,
            VehicleCount: x.Dealer.ActiveVehicleCount,
            IsVerified: x.Dealer.IsVerified
        )).ToList();
    }

    public string GetStaticMapUrl(
        GeoCoordinates center,
        int zoom = 15,
        int width = 600,
        int height = 400,
        List<GeoCoordinates>? markers = null)
    {
        var url = $"https://maps.googleapis.com/maps/api/staticmap" +
                  $"?center={center.Latitude},{center.Longitude}" +
                  $"&zoom={zoom}&size={width}x{height}&maptype=roadmap&key={_apiKey}";

        if (markers != null && markers.Any())
        {
            var markerStr = string.Join("|", markers.Select(m => $"{m.Latitude},{m.Longitude}"));
            url += $"&markers=color:red|{markerStr}";
        }

        return url;
    }

    private static AddressComponents ParseAddressComponents(
        List<AddressComponentDto> components)
    {
        string? Get(params string[] types) => components
            .FirstOrDefault(c => types.Any(t => c.Types.Contains(t)))?.LongName;

        return new AddressComponents(
            StreetNumber: Get("street_number"),
            Route: Get("route"),
            Neighborhood: Get("neighborhood", "sublocality"),
            City: Get("locality", "administrative_area_level_2"),
            Province: Get("administrative_area_level_1"),
            Country: Get("country"),
            PostalCode: Get("postal_code")
        );
    }

    private static double CalculateDistance(
        GeoCoordinates point1, 
        GeoCoordinates point2)
    {
        // Haversine formula
        const double R = 6371000; // Radio de la Tierra en metros
        var lat1 = point1.Latitude * Math.PI / 180;
        var lat2 = point2.Latitude * Math.PI / 180;
        var deltaLat = (point2.Latitude - point1.Latitude) * Math.PI / 180;
        var deltaLng = (point2.Longitude - point1.Longitude) * Math.PI / 180;

        var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                Math.Cos(lat1) * Math.Cos(lat2) *
                Math.Sin(deltaLng / 2) * Math.Sin(deltaLng / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return R * c;
    }

    private static string StripHtml(string html)
    {
        return System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", "");
    }
}

// DTOs para respuestas de Google
internal record GeocodeResponse(string Status, List<GeocodeResult> Results);
internal record GeocodeResult(
    string FormattedAddress,
    string PlaceId,
    GeometryDto Geometry,
    List<AddressComponentDto> AddressComponents,
    PlusCodeDto? PlusCode
);
internal record GeometryDto(LocationDto Location);
internal record LocationDto(double Lat, double Lng);
internal record AddressComponentDto(string LongName, string ShortName, List<string> Types);
internal record PlusCodeDto(string GlobalCode);

internal record PlaceSearchResponse(string Status, List<PlaceResult> Results);
internal record PlaceResult(
    string PlaceId,
    string Name,
    string? Vicinity,
    string? FormattedAddress,
    GeometryDto Geometry,
    decimal? Rating,
    int? UserRatingsTotal,
    OpeningHoursDto? OpeningHours,
    List<PhotoDto>? Photos
);
internal record OpeningHoursDto(bool? OpenNow);
internal record PhotoDto(string PhotoReference);

internal record PlaceDetailsResponse(string Status, PlaceDetailResult? Result);
internal record PlaceDetailResult(
    string Name,
    string? FormattedAddress,
    GeometryDto? Geometry,
    string? FormattedPhoneNumber,
    string? Website,
    decimal? Rating,
    int? UserRatingsTotal,
    OpeningHoursDto? OpeningHours,
    List<PhotoDto>? Photos
);

internal record DistanceMatrixResponse(string Status, List<DistanceRow> Rows);
internal record DistanceRow(List<DistanceElement> Elements);
internal record DistanceElement(
    string Status,
    ValueTextDto Distance,
    ValueTextDto Duration,
    ValueTextDto? DurationInTraffic
);
internal record ValueTextDto(int Value, string Text);

internal record DirectionsResponse(string Status, List<RouteDto> Routes);
internal record RouteDto(string Summary, List<LegDto> Legs, PolylineDto OverviewPolyline);
internal record LegDto(
    ValueTextDto Distance,
    ValueTextDto Duration,
    string StartAddress,
    string EndAddress,
    LocationDto StartLocation,
    LocationDto EndLocation,
    List<StepDto> Steps
);
internal record StepDto(
    string HtmlInstructions,
    ValueTextDto Distance,
    ValueTextDto Duration,
    LocationDto StartLocation,
    LocationDto EndLocation
);
internal record PolylineDto(string Points);

internal record AutocompleteResponse(string Status, List<PredictionDto> Predictions);
internal record PredictionDto(
    string PlaceId,
    string Description,
    StructuredFormattingDto? StructuredFormatting
);
internal record StructuredFormattingDto(string MainText, string SecondaryText);
```

---

## ⚛️ React Components

### Mapa con Dealers

```tsx
// components/DealerMap.tsx
import { useState, useCallback } from 'react';
import { GoogleMap, useJsApiLoader, Marker, InfoWindow } from '@react-google-maps/api';
import { useQuery } from '@tanstack/react-query';
import { locationService } from '@/services/locationService';
import { MapPin, Star, Car, Phone } from 'lucide-react';

interface Props {
  userLocation?: { lat: number; lng: number };
  onDealerSelect?: (dealerId: string) => void;
}

const mapContainerStyle = {
  width: '100%',
  height: '500px',
};

const defaultCenter = { lat: 18.4861, lng: -69.9312 }; // Santo Domingo

export function DealerMap({ userLocation, onDealerSelect }: Props) {
  const [selectedDealer, setSelectedDealer] = useState<NearbyDealer | null>(null);

  const { isLoaded } = useJsApiLoader({
    googleMapsApiKey: import.meta.env.VITE_GOOGLE_MAPS_KEY,
  });

  const dealersQuery = useQuery({
    queryKey: ['nearby-dealers', userLocation],
    queryFn: () => locationService.getNearbyDealers(
      userLocation || defaultCenter,
      50
    ),
    enabled: isLoaded,
  });

  const handleMarkerClick = useCallback((dealer: NearbyDealer) => {
    setSelectedDealer(dealer);
  }, []);

  if (!isLoaded) {
    return <div className="h-[500px] bg-gray-100 animate-pulse rounded-xl" />;
  }

  return (
    <GoogleMap
      mapContainerStyle={mapContainerStyle}
      center={userLocation || defaultCenter}
      zoom={12}
      options={{
        styles: mapStyles,
        disableDefaultUI: false,
        zoomControl: true,
        streetViewControl: false,
        mapTypeControl: false,
      }}
    >
      {/* User location marker */}
      {userLocation && (
        <Marker
          position={userLocation}
          icon={{
            url: '/icons/user-location.svg',
            scaledSize: new google.maps.Size(40, 40),
          }}
          title="Tu ubicación"
        />
      )}

      {/* Dealer markers */}
      {dealersQuery.data?.map((dealer) => (
        <Marker
          key={dealer.dealerId}
          position={{
            lat: dealer.coordinates.latitude,
            lng: dealer.coordinates.longitude,
          }}
          icon={{
            url: dealer.isVerified 
              ? '/icons/dealer-verified.svg' 
              : '/icons/dealer.svg',
            scaledSize: new google.maps.Size(35, 45),
          }}
          onClick={() => handleMarkerClick(dealer)}
        />
      ))}

      {/* Info window */}
      {selectedDealer && (
        <InfoWindow
          position={{
            lat: selectedDealer.coordinates.latitude,
            lng: selectedDealer.coordinates.longitude,
          }}
          onCloseClick={() => setSelectedDealer(null)}
        >
          <div className="p-2 max-w-[250px]">
            <h3 className="font-bold text-lg mb-1">
              {selectedDealer.dealerName}
              {selectedDealer.isVerified && (
                <span className="ml-1 text-blue-600">✓</span>
              )}
            </h3>
            
            <p className="text-gray-600 text-sm mb-2">
              <MapPin className="w-4 h-4 inline mr-1" />
              {selectedDealer.address}
            </p>

            <div className="flex items-center gap-4 text-sm mb-2">
              {selectedDealer.rating && (
                <span className="flex items-center">
                  <Star className="w-4 h-4 text-yellow-500 fill-yellow-500 mr-1" />
                  {selectedDealer.rating.toFixed(1)}
                </span>
              )}
              <span className="flex items-center">
                <Car className="w-4 h-4 mr-1" />
                {selectedDealer.vehicleCount} vehículos
              </span>
            </div>

            <p className="text-sm text-gray-500 mb-3">
              📍 {selectedDealer.distance.distanceText} 
              ({selectedDealer.distance.durationText})
            </p>

            <button
              onClick={() => onDealerSelect?.(selectedDealer.dealerId)}
              className="w-full px-4 py-2 bg-blue-600 text-white rounded-lg
                hover:bg-blue-700 text-sm"
            >
              Ver Inventario
            </button>
          </div>
        </InfoWindow>
      )}
    </GoogleMap>
  );
}

const mapStyles = [
  {
    featureType: 'poi',
    elementType: 'labels',
    stylers: [{ visibility: 'off' }],
  },
];
```

### Autocompletado de Direcciones

```tsx
// components/AddressAutocomplete.tsx
import { useState, useRef } from 'react';
import { useMutation, useQuery } from '@tanstack/react-query';
import { locationService } from '@/services/locationService';
import { MapPin, Loader2, X } from 'lucide-react';
import { useDebounce } from '@/hooks/useDebounce';

interface Props {
  value: string;
  onChange: (value: string) => void;
  onSelect: (address: GeocodedAddress) => void;
  placeholder?: string;
}

export function AddressAutocomplete({ value, onChange, onSelect, placeholder }: Props) {
  const [isOpen, setIsOpen] = useState(false);
  const [sessionToken] = useState(() => crypto.randomUUID());
  const debouncedValue = useDebounce(value, 300);
  const inputRef = useRef<HTMLInputElement>(null);

  const suggestionsQuery = useQuery({
    queryKey: ['address-suggestions', debouncedValue],
    queryFn: () => locationService.autocompleteAddress(debouncedValue, sessionToken),
    enabled: debouncedValue.length >= 3,
  });

  const geocodeMutation = useMutation({
    mutationFn: (placeId: string) => locationService.getPlaceDetails(placeId),
    onSuccess: (address) => {
      if (address) {
        onChange(address.formattedAddress);
        onSelect(address);
        setIsOpen(false);
      }
    },
  });

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    onChange(e.target.value);
    setIsOpen(true);
  };

  const handleSuggestionClick = (suggestion: AddressSuggestion) => {
    geocodeMutation.mutate(suggestion.placeId);
  };

  const handleClear = () => {
    onChange('');
    inputRef.current?.focus();
  };

  return (
    <div className="relative">
      <div className="relative">
        <MapPin className="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
        <input
          ref={inputRef}
          type="text"
          value={value}
          onChange={handleInputChange}
          onFocus={() => setIsOpen(true)}
          placeholder={placeholder || "Ingresa una dirección..."}
          className="w-full pl-10 pr-10 py-3 border rounded-lg focus:ring-2 focus:ring-blue-500"
        />
        {value && (
          <button
            onClick={handleClear}
            className="absolute right-3 top-1/2 -translate-y-1/2"
          >
            <X className="w-5 h-5 text-gray-400 hover:text-gray-600" />
          </button>
        )}
      </div>

      {/* Dropdown */}
      {isOpen && debouncedValue.length >= 3 && (
        <div className="absolute z-50 w-full mt-1 bg-white border rounded-lg shadow-lg max-h-60 overflow-auto">
          {suggestionsQuery.isLoading && (
            <div className="p-4 text-center text-gray-500">
              <Loader2 className="w-5 h-5 animate-spin mx-auto" />
            </div>
          )}

          {suggestionsQuery.data?.map((suggestion) => (
            <button
              key={suggestion.placeId}
              onClick={() => handleSuggestionClick(suggestion)}
              className="w-full px-4 py-3 text-left hover:bg-gray-50 flex items-start gap-3"
            >
              <MapPin className="w-5 h-5 text-gray-400 mt-0.5 flex-shrink-0" />
              <div>
                <p className="font-medium">{suggestion.mainText}</p>
                <p className="text-sm text-gray-500">{suggestion.secondaryText}</p>
              </div>
            </button>
          ))}

          {suggestionsQuery.data?.length === 0 && (
            <p className="p-4 text-center text-gray-500">
              No se encontraron direcciones
            </p>
          )}
        </div>
      )}

      {/* Overlay to close dropdown */}
      {isOpen && (
        <div 
          className="fixed inset-0 z-40" 
          onClick={() => setIsOpen(false)} 
        />
      )}
    </div>
  );
}
```

---

## 🎯 Controller

```csharp
namespace LocationService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationController : ControllerBase
{
    private readonly ILocationService _locationService;

    public LocationController(ILocationService locationService)
    {
        _locationService = locationService;
    }

    /// <summary>
    /// Geocodifica dirección a coordenadas
    /// </summary>
    [HttpGet("geocode")]
    public async Task<ActionResult<GeocodedAddress>> Geocode([FromQuery] string address)
    {
        var result = await _locationService.GeocodeAddressAsync(address);
        if (result == null)
            return NotFound("No se pudo geocodificar la dirección");
        return Ok(result);
    }

    /// <summary>
    /// Autocompletado de direcciones
    /// </summary>
    [HttpGet("autocomplete")]
    public async Task<ActionResult<List<AddressSuggestion>>> Autocomplete(
        [FromQuery] string input,
        [FromQuery] string? sessionToken = null)
    {
        var suggestions = await _locationService.AutocompleteAddressAsync(input, sessionToken);
        return Ok(suggestions);
    }

    /// <summary>
    /// Obtiene dealers cercanos
    /// </summary>
    [HttpGet("dealers/nearby")]
    public async Task<ActionResult<List<NearbyDealer>>> GetNearbyDealers(
        [FromQuery] double lat,
        [FromQuery] double lng,
        [FromQuery] int radiusKm = 50)
    {
        var location = new GeoCoordinates(lat, lng);
        var dealers = await _locationService.GetNearbyDealersAsync(location, radiusKm);
        return Ok(dealers);
    }

    /// <summary>
    /// Calcula distancia entre puntos
    /// </summary>
    [HttpPost("distance")]
    public async Task<ActionResult<List<DistanceInfo>>> GetDistances(
        [FromBody] DistanceRequest request)
    {
        var distances = await _locationService.GetDistancesAsync(
            request.Origin,
            request.Destinations
        );
        return Ok(distances);
    }

    /// <summary>
    /// Obtiene ruta entre dos puntos
    /// </summary>
    [HttpGet("directions")]
    public async Task<ActionResult<Route>> GetDirections(
        [FromQuery] double originLat,
        [FromQuery] double originLng,
        [FromQuery] double destLat,
        [FromQuery] double destLng)
    {
        var origin = new GeoCoordinates(originLat, originLng);
        var destination = new GeoCoordinates(destLat, destLng);
        
        var route = await _locationService.GetDirectionsAsync(origin, destination);
        if (route == null)
            return NotFound("No se pudo calcular la ruta");
        return Ok(route);
    }
}

public record DistanceRequest(
    GeoCoordinates Origin,
    List<GeoCoordinates> Destinations
);
```

---

## ⚙️ Configuración

```json
{
  "GoogleMaps": {
    "ApiKey": "AIzaSyxxxxxxxxxxxxxxxxxxxxxxxxxx",
    "Restrictions": {
      "AllowedDomains": ["okla.com.do", "*.okla.com.do"],
      "AllowedIPs": ["146.190.199.0"]
    }
  }
}
```

---

## 📞 Recursos

| Recurso | URL |
|---------|-----|
| Google Cloud Console | [console.cloud.google.com](https://console.cloud.google.com) |
| Maps JavaScript API | [developers.google.com/maps/documentation/javascript](https://developers.google.com/maps/documentation/javascript) |
| Precios | [cloud.google.com/maps-platform/pricing](https://cloud.google.com/maps-platform/pricing) |
| Soporte | [cloud.google.com/support](https://cloud.google.com/support) |

---

**Anterior:** [SMS_GATEWAYS_API.md](./SMS_GATEWAYS_API.md)  
**Siguiente:** [ONE_ESTADISTICAS_API.md](./ONE_ESTADISTICAS_API.md)
