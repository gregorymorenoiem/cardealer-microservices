# 📦 Logistics APIs

**Categoría:** Vehicle Transport  
**APIs:** 2 (uShip, Montway)  
**Fase:** 3 (Premium Features)  
**Impacto:** +20% conversiones de compradores fuera de la ciudad del vendedor

---

## 📖 Resumen

Servicios de transporte de vehículos puerta a puerta. Permite a compradores comprar vehículos en cualquier parte del país y recibirlos en su casa con cotización instantánea y seguimiento en tiempo real.

### Casos de Uso en OKLA

✅ **Cotización instantánea** - Comprador ve "$850 envío a Santiago" en la ficha  
✅ **Booking integrado** - Reservar transporte sin salir de OKLA  
✅ **Tracking en tiempo real** - "Tu vehículo está en X, llega mañana"  
✅ **Seguro de transporte** - Cobertura incluida hasta $50K  
✅ **Documentación digital** - Bill of Lading firmado electrónicamente  
✅ **Para dealers** - Envío de lotes de vehículos con descuento

---

## 🔗 Comparativa de APIs

| Aspecto               | **uShip**             | **Montway**         |
| --------------------- | --------------------- | ------------------- |
| **Modelo**            | Marketplace (ofertas) | Precio fijo         |
| **Cobertura USA**     | ✅ Nacional           | ✅ Nacional         |
| **Cobertura RD**      | ⚠️ Limitada           | ❌ No               |
| **Tiempo cotización** | 24-48h (subastas)     | Instantáneo         |
| **Precio promedio**   | $700-1,500            | $800-1,200          |
| **Seguimiento**       | ✅ GPS                | ✅ GPS              |
| **Seguro incluido**   | Hasta $100K           | Hasta $50K          |
| **API disponible**    | ✅ REST               | ✅ REST             |
| **Mejor para**        | Precio competitivo    | Rapidez y confianza |
| **Recomendado**       | ⭐ Para RD (flexible) | ⭐ Para USA         |

---

## 📡 ENDPOINTS

### uShip API

- `POST /quotes` - Solicitar cotizaciones
- `GET /quotes/{id}` - Ver cotizaciones recibidas
- `POST /shipments` - Crear envío
- `GET /shipments/{id}` - Estado del envío
- `GET /shipments/{id}/tracking` - Tracking GPS
- `POST /shipments/{id}/confirm-pickup` - Confirmar recogida
- `POST /shipments/{id}/confirm-delivery` - Confirmar entrega

### Montway API

- `POST /api/quotes` - Cotización instantánea
- `POST /api/orders` - Crear orden de transporte
- `GET /api/orders/{id}` - Estado de orden
- `GET /api/orders/{id}/tracking` - Seguimiento
- `PUT /api/orders/{id}/documents` - Subir documentos

---

## 💻 Backend Implementation (C#)

### Service Interface

```csharp
public interface ILogisticsService
{
    Task<ShippingQuote> GetQuoteAsync(ShippingRequest request);
    Task<ShippingQuote[]> GetMultipleQuotesAsync(ShippingRequest request); // Para uShip
    Task<Shipment> CreateShipmentAsync(CreateShipmentRequest request);
    Task<Shipment> GetShipmentAsync(string shipmentId);
    Task<TrackingInfo> GetTrackingAsync(string shipmentId);
    Task ConfirmPickupAsync(string shipmentId, PickupConfirmation confirmation);
    Task ConfirmDeliveryAsync(string shipmentId, DeliveryConfirmation confirmation);
}

public class ShippingRequest
{
    public ShippingLocation Origin { get; set; }
    public ShippingLocation Destination { get; set; }
    public VehicleDetails Vehicle { get; set; }
    public ShippingType Type { get; set; }
    public DateTime PreferredPickupDate { get; set; }
    public bool IncludeInsurance { get; set; }
}

public class ShippingLocation
{
    public string Address { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string ZipCode { get; set; }
    public string Country { get; set; }
    public string ContactName { get; set; }
    public string ContactPhone { get; set; }
}

public class VehicleDetails
{
    public string Vin { get; set; }
    public string Make { get; set; }
    public string Model { get; set; }
    public int Year { get; set; }
    public VehicleCondition Condition { get; set; } // Running, Non-Running
    public bool IsModified { get; set; }
}

public enum ShippingType
{
    OpenTransport,   // Más económico, expuesto al clima
    EnclosedTransport // Más caro, protegido
}

public enum VehicleCondition
{
    Running,    // Rueda normalmente
    NonRunning, // Necesita grúa
    Inoperable  // Daño significativo
}
```

### Domain Models

```csharp
public class ShippingQuote
{
    public string QuoteId { get; set; }
    public string CarrierId { get; set; }
    public string CarrierName { get; set; }
    public decimal Price { get; set; }
    public decimal InsuranceCost { get; set; }
    public decimal TotalCost { get; set; }
    public ShippingType Type { get; set; }
    public int EstimatedDays { get; set; }
    public DateTime EstimatedPickup { get; set; }
    public DateTime EstimatedDelivery { get; set; }
    public decimal InsuranceCoverage { get; set; }
    public string[] Includes { get; set; }
    public DateTime ValidUntil { get; set; }
    public decimal CarrierRating { get; set; } // 1-5 estrellas
    public int CarrierReviews { get; set; }
}

public class Shipment
{
    public string ShipmentId { get; set; }
    public string OrderNumber { get; set; }
    public ShipmentStatus Status { get; set; }
    public ShippingLocation Origin { get; set; }
    public ShippingLocation Destination { get; set; }
    public VehicleDetails Vehicle { get; set; }
    public string CarrierId { get; set; }
    public string CarrierName { get; set; }
    public string DriverName { get; set; }
    public string DriverPhone { get; set; }
    public decimal TotalCost { get; set; }
    public DateTime ScheduledPickup { get; set; }
    public DateTime? ActualPickup { get; set; }
    public DateTime EstimatedDelivery { get; set; }
    public DateTime? ActualDelivery { get; set; }
    public string BillOfLadingUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}

public enum ShipmentStatus
{
    Pending,        // Esperando confirmación
    Confirmed,      // Carrier asignado
    PickupScheduled,// Fecha de recogida confirmada
    PickedUp,       // Vehículo recogido
    InTransit,      // En camino
    OutForDelivery, // Llegando hoy
    Delivered,      // Entregado
    Cancelled       // Cancelado
}

public class TrackingInfo
{
    public string ShipmentId { get; set; }
    public ShipmentStatus Status { get; set; }
    public TrackingLocation CurrentLocation { get; set; }
    public DateTime LastUpdate { get; set; }
    public DateTime EstimatedArrival { get; set; }
    public TrackingEvent[] Events { get; set; }
}

public class TrackingLocation
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string City { get; set; }
    public string State { get; set; }
}

public class TrackingEvent
{
    public DateTime Timestamp { get; set; }
    public string Event { get; set; }      // "Vehicle picked up", "In transit"
    public string Location { get; set; }   // "Miami, FL"
    public string Description { get; set; }
}

public class PickupConfirmation
{
    public DateTime PickupTime { get; set; }
    public string DriverSignature { get; set; } // Base64
    public string SellerSignature { get; set; } // Base64
    public string[] VehiclePhotos { get; set; } // URLs de fotos pre-transporte
    public string Notes { get; set; }
    public VehicleConditionReport ConditionReport { get; set; }
}

public class DeliveryConfirmation
{
    public DateTime DeliveryTime { get; set; }
    public string DriverSignature { get; set; }
    public string BuyerSignature { get; set; }
    public string[] VehiclePhotos { get; set; }
    public bool VehicleReceivedInGoodCondition { get; set; }
    public string DamageNotes { get; set; }
}

public class VehicleConditionReport
{
    public bool EngineStarts { get; set; }
    public bool DrivesTested { get; set; }
    public int FuelLevel { get; set; } // 0-100%
    public string[] PreExistingDamage { get; set; }
    public int Odometer { get; set; }
}
```

### Service Implementation

```csharp
public class UShipLogisticsService : ILogisticsService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<UShipLogisticsService> _logger;
    private readonly string _apiKey;
    private const string BaseUrl = "https://api.uship.com/v2";

    public UShipLogisticsService(HttpClient httpClient, IConfiguration config, ILogger<UShipLogisticsService> logger)
    {
        _httpClient = httpClient;
        _config = config;
        _logger = logger;
        _apiKey = config["UShip:ApiKey"];
    }

    public async Task<ShippingQuote> GetQuoteAsync(ShippingRequest request)
    {
        var quotes = await GetMultipleQuotesAsync(request);
        return quotes.OrderBy(q => q.TotalCost).FirstOrDefault();
    }

    public async Task<ShippingQuote[]> GetMultipleQuotesAsync(ShippingRequest request)
    {
        try
        {
            var payload = new
            {
                origin = new
                {
                    address = request.Origin.Address,
                    city = request.Origin.City,
                    state = request.Origin.State,
                    zip = request.Origin.ZipCode,
                    country = request.Origin.Country ?? "US"
                },
                destination = new
                {
                    address = request.Destination.Address,
                    city = request.Destination.City,
                    state = request.Destination.State,
                    zip = request.Destination.ZipCode,
                    country = request.Destination.Country ?? "US"
                },
                vehicle = new
                {
                    vin = request.Vehicle.Vin,
                    make = request.Vehicle.Make,
                    model = request.Vehicle.Model,
                    year = request.Vehicle.Year,
                    condition = request.Vehicle.Condition.ToString().ToLower(),
                    is_modified = request.Vehicle.IsModified
                },
                transport_type = request.Type == ShippingType.OpenTransport ? "open" : "enclosed",
                pickup_date = request.PreferredPickupDate.ToString("yyyy-MM-dd"),
                include_insurance = request.IncludeInsurance
            };

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/quotes");
            httpRequest.Headers.Add("Authorization", $"Bearer {_apiKey}");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(httpRequest);
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var quotes = new List<ShippingQuote>();
            foreach (var item in doc.RootElement.GetProperty("quotes").EnumerateArray())
            {
                quotes.Add(new ShippingQuote
                {
                    QuoteId = item.GetProperty("quote_id").GetString(),
                    CarrierId = item.GetProperty("carrier_id").GetString(),
                    CarrierName = item.GetProperty("carrier_name").GetString(),
                    Price = item.GetProperty("price").GetDecimal(),
                    InsuranceCost = item.TryGetProperty("insurance_cost", out var ins) ? ins.GetDecimal() : 0,
                    TotalCost = item.GetProperty("total").GetDecimal(),
                    Type = request.Type,
                    EstimatedDays = item.GetProperty("estimated_days").GetInt32(),
                    EstimatedPickup = DateTime.Parse(item.GetProperty("pickup_date").GetString()),
                    EstimatedDelivery = DateTime.Parse(item.GetProperty("delivery_date").GetString()),
                    InsuranceCoverage = item.GetProperty("insurance_coverage").GetDecimal(),
                    CarrierRating = item.GetProperty("carrier_rating").GetDecimal(),
                    CarrierReviews = item.GetProperty("carrier_reviews").GetInt32(),
                    ValidUntil = DateTime.UtcNow.AddDays(3)
                });
            }

            return quotes.OrderByDescending(q => q.CarrierRating).ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting shipping quotes from uShip");
            throw;
        }
    }

    public async Task<Shipment> CreateShipmentAsync(CreateShipmentRequest request)
    {
        var payload = new
        {
            quote_id = request.QuoteId,
            contact = new
            {
                origin_name = request.Origin.ContactName,
                origin_phone = request.Origin.ContactPhone,
                destination_name = request.Destination.ContactName,
                destination_phone = request.Destination.ContactPhone
            },
            payment = new
            {
                method = request.PaymentMethod,
                token = request.PaymentToken
            }
        };

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/shipments");
        httpRequest.Headers.Add("Authorization", $"Bearer {_apiKey}");
        httpRequest.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(httpRequest);
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        return new Shipment
        {
            ShipmentId = root.GetProperty("shipment_id").GetString(),
            OrderNumber = root.GetProperty("order_number").GetString(),
            Status = ShipmentStatus.Pending,
            CarrierId = root.GetProperty("carrier_id").GetString(),
            CarrierName = root.GetProperty("carrier_name").GetString(),
            TotalCost = root.GetProperty("total").GetDecimal(),
            ScheduledPickup = DateTime.Parse(root.GetProperty("scheduled_pickup").GetString()),
            EstimatedDelivery = DateTime.Parse(root.GetProperty("estimated_delivery").GetString()),
            CreatedAt = DateTime.UtcNow
        };
    }

    public async Task<Shipment> GetShipmentAsync(string shipmentId)
    {
        var response = await _httpClient.GetAsync($"{BaseUrl}/shipments/{shipmentId}");
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Shipment>(json);
    }

    public async Task<TrackingInfo> GetTrackingAsync(string shipmentId)
    {
        var response = await _httpClient.GetAsync($"{BaseUrl}/shipments/{shipmentId}/tracking");
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        return new TrackingInfo
        {
            ShipmentId = shipmentId,
            Status = Enum.Parse<ShipmentStatus>(root.GetProperty("status").GetString(), true),
            CurrentLocation = new TrackingLocation
            {
                Latitude = root.GetProperty("location").GetProperty("lat").GetDouble(),
                Longitude = root.GetProperty("location").GetProperty("lng").GetDouble(),
                City = root.GetProperty("location").GetProperty("city").GetString(),
                State = root.GetProperty("location").GetProperty("state").GetString()
            },
            LastUpdate = DateTime.Parse(root.GetProperty("updated_at").GetString()),
            EstimatedArrival = DateTime.Parse(root.GetProperty("eta").GetString()),
            Events = ParseTrackingEvents(root.GetProperty("events"))
        };
    }

    private TrackingEvent[] ParseTrackingEvents(JsonElement eventsElement)
    {
        var events = new List<TrackingEvent>();
        foreach (var item in eventsElement.EnumerateArray())
        {
            events.Add(new TrackingEvent
            {
                Timestamp = DateTime.Parse(item.GetProperty("timestamp").GetString()),
                Event = item.GetProperty("event").GetString(),
                Location = item.GetProperty("location").GetString(),
                Description = item.TryGetProperty("description", out var desc) ? desc.GetString() : null
            });
        }
        return events.OrderByDescending(e => e.Timestamp).ToArray();
    }
}
```

---

## 🎨 Frontend Implementation (React + TypeScript)

### Logistics Service

```typescript
import axios from "axios";

export interface ShippingQuote {
  quoteId: string;
  carrierName: string;
  price: number;
  insuranceCost: number;
  totalCost: number;
  estimatedDays: number;
  estimatedPickup: string;
  estimatedDelivery: string;
  carrierRating: number;
  carrierReviews: number;
}

export interface Shipment {
  shipmentId: string;
  orderNumber: string;
  status: string;
  carrierName: string;
  driverName?: string;
  driverPhone?: string;
  scheduledPickup: string;
  estimatedDelivery: string;
  totalCost: number;
}

export interface TrackingInfo {
  status: string;
  currentLocation: { city: string; state: string; lat: number; lng: number };
  estimatedArrival: string;
  events: TrackingEvent[];
}

export class LogisticsService {
  private baseUrl = process.env.REACT_APP_API_URL;

  async getQuote(
    origin: Location,
    destination: Location,
    vehicle: VehicleDetails
  ): Promise<ShippingQuote[]> {
    const response = await axios.post(`${this.baseUrl}/api/logistics/quotes`, {
      origin,
      destination,
      vehicle,
    });
    return response.data;
  }

  async createShipment(
    quoteId: string,
    paymentInfo: PaymentInfo
  ): Promise<Shipment> {
    const response = await axios.post(
      `${this.baseUrl}/api/logistics/shipments`,
      {
        quoteId,
        paymentInfo,
      }
    );
    return response.data;
  }

  async getTracking(shipmentId: string): Promise<TrackingInfo> {
    const response = await axios.get(
      `${this.baseUrl}/api/logistics/shipments/${shipmentId}/tracking`
    );
    return response.data;
  }
}
```

### React Component - Shipping Quote Calculator

```typescript
import React, { useState } from "react";
import { useMutation } from "@tanstack/react-query";
import { LogisticsService, ShippingQuote } from "@/services/logisticsService";
import { Truck, Shield, Star, Clock } from "lucide-react";

interface Props {
  vehicle: { vin: string; make: string; model: string; year: number };
  originZip: string;
}

export const ShippingQuoteCalculator = ({ vehicle, originZip }: Props) => {
  const [destinationZip, setDestinationZip] = useState("");
  const [selectedQuote, setSelectedQuote] = useState<string | null>(null);
  const logisticsService = new LogisticsService();

  const {
    mutate: getQuotes,
    data: quotes,
    isLoading,
  } = useMutation({
    mutationFn: () =>
      logisticsService.getQuote(
        { zipCode: originZip },
        { zipCode: destinationZip },
        vehicle
      ),
  });

  const formatCurrency = (value: number) =>
    new Intl.NumberFormat("en-US", {
      style: "currency",
      currency: "USD",
    }).format(value);

  return (
    <div className="border rounded-xl p-6 bg-white shadow-sm">
      <h3 className="text-xl font-bold flex items-center gap-2 mb-6">
        <Truck className="h-6 w-6 text-blue-600" />
        Calcular Envío
      </h3>

      <div className="flex gap-4 mb-6">
        <div className="flex-1">
          <label className="text-sm text-gray-600 block mb-1">Desde</label>
          <input
            value={originZip}
            disabled
            className="w-full border rounded-lg p-3 bg-gray-100"
          />
        </div>
        <div className="flex-1">
          <label className="text-sm text-gray-600 block mb-1">
            Hasta (ZIP)
          </label>
          <input
            value={destinationZip}
            onChange={(e) => setDestinationZip(e.target.value)}
            placeholder="10001"
            className="w-full border rounded-lg p-3"
          />
        </div>
        <button
          onClick={() => getQuotes()}
          disabled={!destinationZip || isLoading}
          className="self-end px-6 py-3 bg-blue-600 text-white rounded-lg font-semibold disabled:bg-gray-400"
        >
          {isLoading ? "Calculando..." : "Cotizar"}
        </button>
      </div>

      {/* Quotes */}
      {quotes && quotes.length > 0 && (
        <div className="space-y-4">
          <h4 className="font-semibold text-gray-700">
            Opciones de Transporte:
          </h4>

          {quotes.map((quote) => (
            <div
              key={quote.quoteId}
              onClick={() => setSelectedQuote(quote.quoteId)}
              className={`border-2 rounded-xl p-4 cursor-pointer transition-all ${
                selectedQuote === quote.quoteId
                  ? "border-blue-600 bg-blue-50"
                  : "border-gray-200 hover:border-blue-300"
              }`}
            >
              <div className="flex justify-between items-start">
                <div>
                  <h5 className="font-semibold">{quote.carrierName}</h5>
                  <div className="flex items-center gap-1 text-sm text-yellow-600">
                    <Star className="h-4 w-4 fill-current" />
                    <span>{quote.carrierRating.toFixed(1)}</span>
                    <span className="text-gray-400">
                      ({quote.carrierReviews} reviews)
                    </span>
                  </div>
                </div>
                <div className="text-right">
                  <p className="text-2xl font-bold text-blue-600">
                    {formatCurrency(quote.totalCost)}
                  </p>
                  <p className="text-sm text-gray-500">
                    <Clock className="h-3 w-3 inline mr-1" />
                    {quote.estimatedDays} días
                  </p>
                </div>
              </div>

              <div className="mt-3 flex gap-4 text-sm text-gray-600">
                <span className="flex items-center gap-1">
                  <Shield className="h-4 w-4 text-green-500" />
                  Seguro hasta $50,000
                </span>
                <span>
                  Recogida:{" "}
                  {new Date(quote.estimatedPickup).toLocaleDateString("es-DO")}
                </span>
              </div>
            </div>
          ))}

          {selectedQuote && (
            <button className="w-full py-3 bg-green-600 text-white rounded-lg font-semibold hover:bg-green-700">
              Reservar Transporte
            </button>
          )}
        </div>
      )}

      <p className="text-xs text-gray-500 mt-4">
        * Precios incluyen seguro básico. Transporte abierto (expuesto al
        clima).
      </p>
    </div>
  );
};
```

### React Component - Shipment Tracker

```typescript
import React from "react";
import { useQuery } from "@tanstack/react-query";
import { LogisticsService } from "@/services/logisticsService";
import { MapPin, Truck, CheckCircle, Clock } from "lucide-react";

interface Props {
  shipmentId: string;
}

export const ShipmentTracker = ({ shipmentId }: Props) => {
  const logisticsService = new LogisticsService();

  const { data: tracking, isLoading } = useQuery({
    queryKey: ["shipment-tracking", shipmentId],
    queryFn: () => logisticsService.getTracking(shipmentId),
    refetchInterval: 60000, // Actualizar cada minuto
  });

  const statusSteps = [
    { key: "Confirmed", label: "Confirmado", icon: CheckCircle },
    { key: "PickedUp", label: "Recogido", icon: Truck },
    { key: "InTransit", label: "En Tránsito", icon: Truck },
    { key: "Delivered", label: "Entregado", icon: MapPin },
  ];

  if (isLoading)
    return <div className="h-48 bg-gray-100 animate-pulse rounded-xl"></div>;
  if (!tracking) return null;

  const currentStepIndex = statusSteps.findIndex(
    (s) => s.key === tracking.status
  );

  return (
    <div className="bg-white rounded-xl p-6 shadow-sm">
      <h3 className="text-xl font-bold mb-6">📦 Seguimiento de Envío</h3>

      {/* Progress Steps */}
      <div className="flex justify-between mb-8">
        {statusSteps.map((step, index) => {
          const isComplete = index <= currentStepIndex;
          const isCurrent = index === currentStepIndex;

          return (
            <div key={step.key} className="flex flex-col items-center flex-1">
              <div
                className={`w-10 h-10 rounded-full flex items-center justify-center ${
                  isComplete ? "bg-green-500" : "bg-gray-200"
                }`}
              >
                <step.icon
                  className={`h-5 w-5 ${
                    isComplete ? "text-white" : "text-gray-400"
                  }`}
                />
              </div>
              <span
                className={`text-sm mt-2 ${
                  isCurrent ? "font-semibold text-green-600" : "text-gray-500"
                }`}
              >
                {step.label}
              </span>
              {index < statusSteps.length - 1 && (
                <div
                  className={`h-0.5 w-full absolute top-5 ${
                    index < currentStepIndex ? "bg-green-500" : "bg-gray-200"
                  }`}
                />
              )}
            </div>
          );
        })}
      </div>

      {/* Current Location */}
      {tracking.currentLocation && (
        <div className="bg-blue-50 rounded-lg p-4 mb-6">
          <div className="flex items-center gap-3">
            <MapPin className="h-6 w-6 text-blue-600" />
            <div>
              <p className="font-semibold">Ubicación Actual</p>
              <p className="text-gray-600">
                {tracking.currentLocation.city},{" "}
                {tracking.currentLocation.state}
              </p>
            </div>
          </div>
          <p className="text-sm text-gray-500 mt-2">
            <Clock className="h-4 w-4 inline mr-1" />
            Llegada estimada: {new Date(
              tracking.estimatedArrival
            ).toLocaleDateString("es-DO", {
              weekday: "long",
              month: "short",
              day: "numeric",
            })}
          </p>
        </div>
      )}

      {/* Event Timeline */}
      <div className="space-y-4">
        <h4 className="font-semibold text-gray-700">Historial</h4>
        {tracking.events.map((event, idx) => (
          <div key={idx} className="flex gap-4">
            <div className="w-24 text-xs text-gray-500">
              {new Date(event.timestamp).toLocaleString("es-DO", {
                month: "short",
                day: "numeric",
                hour: "2-digit",
                minute: "2-digit",
              })}
            </div>
            <div className="flex-1">
              <p className="font-medium">{event.event}</p>
              <p className="text-sm text-gray-500">{event.location}</p>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};
```

---

## ✅ Testing (xUnit)

```csharp
public class UShipLogisticsServiceTests
{
    [Fact]
    public async Task GetQuoteAsync_WithValidRequest_ReturnsQuotes()
    {
        var request = new ShippingRequest
        {
            Origin = new ShippingLocation { ZipCode = "33101", City = "Miami", State = "FL" },
            Destination = new ShippingLocation { ZipCode = "10001", City = "New York", State = "NY" },
            Vehicle = new VehicleDetails { Make = "Toyota", Model = "Camry", Year = 2020, Condition = VehicleCondition.Running }
        };

        var quotes = await _service.GetMultipleQuotesAsync(request);

        Assert.NotEmpty(quotes);
        Assert.All(quotes, q => Assert.True(q.TotalCost > 0));
    }

    [Fact]
    public async Task CreateShipmentAsync_WithValidQuote_ReturnsShipment()
    {
        var request = new CreateShipmentRequest { QuoteId = "valid-quote-id" };

        var shipment = await _service.CreateShipmentAsync(request);

        Assert.NotNull(shipment);
        Assert.NotEmpty(shipment.ShipmentId);
        Assert.Equal(ShipmentStatus.Pending, shipment.Status);
    }

    [Fact]
    public async Task GetTrackingAsync_ReturnsTrackingInfo()
    {
        var shipmentId = "active-shipment-id";

        var tracking = await _service.GetTrackingAsync(shipmentId);

        Assert.NotNull(tracking);
        Assert.NotNull(tracking.CurrentLocation);
        Assert.NotEmpty(tracking.Events);
    }
}
```

---

## 🔧 Troubleshooting

| Problema                    | Causa                           | Solución                               |
| --------------------------- | ------------------------------- | -------------------------------------- |
| Cotización muy alta         | Vehículo non-running o modified | Mostrar desglose y opciones            |
| No hay carriers disponibles | Ruta muy remota                 | Ofrecer alternativas o pickup points   |
| Tracking no actualiza       | GPS del carrier intermitente    | Polling cada 5 min + último conocido   |
| Vehículo dañado en tránsito | Accidente o negligencia         | Documentar con fotos + iniciar claim   |
| Pickup cancelado            | Carrier no llegó                | Re-asignar automáticamente + compensar |
| Entrega retrasada           | Clima, tráfico, problemas mec.  | Notificar proactivamente + nuevo ETA   |

---

## 🔗 Integración con OKLA

### 1. **Crear LogisticsService**

```csharp
services.AddHttpClient<ILogisticsService, UShipLogisticsService>();
```

### 2. **Gateway routing**

```json
{
  "UpstreamPathTemplate": "/api/logistics/{everything}",
  "DownstreamPathTemplate": "/api/logistics/{everything}",
  "DownstreamHostAndPorts": [{ "Host": "logisticsservice", "Port": 8080 }]
}
```

### 3. **En VehicleDetailPage**

```tsx
<ShippingQuoteCalculator
  vehicle={vehicle}
  originZip={vehicle.location.zipCode}
/>
```

### 4. **En orden de compra**

```tsx
// Agregar costo de envío al total
const total = vehiclePrice + shippingQuote.totalCost + fees;
```

### 5. **Tracking page para comprador**

```tsx
<Route path="/my-orders/:orderId/tracking" element={<ShipmentTracker />} />
```

---

## 💰 Costos Estimados

| Ruta Ejemplo       | uShip (open) | Montway (open) | Markup OKLA |
| ------------------ | ------------ | -------------- | ----------- |
| Miami → New York   | $800         | $900           | $100        |
| LA → Houston       | $650         | $700           | $80         |
| Chicago → Denver   | $550         | $600           | $70         |
| **100 envíos/mes** | $70,000      | -              | **$9,000**  |

✅ **Revenue adicional:** ~$9,000/mes en markup de logística con 100 envíos.

> **Nota para RD:** uShip tiene cobertura limitada en República Dominicana. Considerar alianza con transporte local (grúas RD) + crear servicio propio de coordinación.
