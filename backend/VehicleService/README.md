# 🚗 VehicleService

Servicio principal de gestión de inventario de vehículos para el sistema CarDealer.

## 📋 Descripción

Microservicio core responsable de la gestión completa del inventario de vehículos, incluyendo CRUD, búsquedas, reservas y gestión de imágenes.

## 🚀 Características

- **Gestión de Inventario**: CRUD completo de vehículos
- **Búsqueda Avanzada**: Filtros por marca, modelo, año, precio, etc.
- **Gestión de Imágenes**: Upload y gestión de fotos de vehículos
- **Reservas**: Sistema de reserva temporal de vehículos
- **Historial**: Tracking de cambios y actualizaciones
- **Validaciones**: Business rules para integridad de datos
- **Clean Architecture**: Separación en capas Domain, Application, Infrastructure, API
- **CQRS**: Commands y Queries con MediatR
- **Event Sourcing**: Publicación de eventos de dominio

## 🏗️ Arquitectura

```
VehicleService.Api (Puerto 5009)
├── Controllers/
│   ├── VehiclesController.cs
│   ├── VehicleImagesController.cs
│   └── VehicleReservationsController.cs
├── VehicleService.Application/
│   ├── Commands/
│   │   ├── CreateVehicleCommand
│   │   ├── UpdateVehicleCommand
│   │   ├── DeleteVehicleCommand
│   │   └── ReserveVehicleCommand
│   ├── Queries/
│   │   ├── GetVehicleQuery
│   │   ├── GetVehiclesQuery
│   │   └── SearchVehiclesQuery
│   ├── Validators/
│   │   └── CreateVehicleCommandValidator
│   └── Services/
│       ├── VehicleManager
│       └── ReservationManager
├── VehicleService.Domain/
│   ├── Entities/
│   │   ├── Vehicle
│   │   ├── VehicleImage
│   │   └── VehicleReservation
│   ├── Enums/
│   │   ├── VehicleStatus
│   │   ├── FuelType
│   │   ├── Transmission
│   │   └── BodyType
│   ├── ValueObjects/
│   │   ├── VehicleSpecs
│   │   └── Price
│   └── Events/
│       ├── VehicleCreatedEvent
│       └── VehicleReservedEvent
└── VehicleService.Infrastructure/
    ├── Data/
    │   └── VehicleDbContext
    ├── Repositories/
    │   └── VehicleRepository
    ├── External/
    │   ├── MediaServiceClient
    │   └── SearchServiceClient
    └── MessageBus/
```

## 📦 Dependencias Principales

- **Entity Framework Core 8.0**
- **MediatR 12.2.0** - CQRS
- **FluentValidation 11.8.0**
- **AutoMapper 12.0.1**
- **Npgsql** - PostgreSQL provider
- **RabbitMQ.Client 6.8.1** - Event publishing
- **Serilog** - Logging

## ⚙️ Configuración

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=vehicledb;Username=admin;Password=***"
  },
  "ServiceUrls": {
    "MediaService": "http://localhost:5004",
    "SearchService": "http://localhost:5023",
    "NotificationService": "http://localhost:5003"
  },
  "RabbitMQ": {
    "Host": "localhost",
    "Port": 5672,
    "Username": "guest",
    "Password": "guest"
  },
  "Reservation": {
    "DefaultDurationMinutes": 30,
    "MaxDurationMinutes": 1440
  }
}
```

### Variables de Entorno
```bash
ASPNETCORE_ENVIRONMENT=Development
ConnectionStrings__DefaultConnection=Host=postgres;Database=vehicledb;...
MediaService__BaseUrl=http://mediaservice
SearchService__BaseUrl=http://searchservice
RabbitMQ__Host=rabbitmq
```

## 🔌 API Endpoints

### Vehículos
```http
GET    /api/vehicles                # Listar vehículos (paginado)
GET    /api/vehicles/{id}           # Obtener vehículo por ID
POST   /api/vehicles                # Crear vehículo
PUT    /api/vehicles/{id}           # Actualizar vehículo
DELETE /api/vehicles/{id}           # Eliminar vehículo (soft delete)
PATCH  /api/vehicles/{id}/status    # Cambiar estado
```

### Búsqueda y Filtros
```http
GET /api/vehicles/search?brand=BMW&yearFrom=2020&priceMax=50000
GET /api/vehicles/featured           # Vehículos destacados
GET /api/vehicles/similar/{id}       # Vehículos similares
```

### Imágenes
```http
GET    /api/vehicles/{id}/images    # Obtener imágenes de vehículo
POST   /api/vehicles/{id}/images    # Subir imagen
DELETE /api/vehicles/{id}/images/{imageId}  # Eliminar imagen
PUT    /api/vehicles/{id}/images/{imageId}/primary  # Marcar como principal
```

### Reservas
```http
GET    /api/vehicles/{id}/reservations        # Ver reservas
POST   /api/vehicles/{id}/reserve              # Reservar vehículo
DELETE /api/vehicles/{id}/reservations/{resId} # Cancelar reserva
```

### Estadísticas
```http
GET /api/vehicles/stats              # Estadísticas generales
GET /api/vehicles/stats/brands       # Distribución por marca
GET /api/vehicles/stats/inventory    # Estado del inventario
```

## 📝 Ejemplos de Uso

### Crear Vehículo
```bash
curl -X POST http://localhost:5009/api/vehicles \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{
    "brand": "BMW",
    "model": "X5",
    "year": 2024,
    "price": 75000,
    "currency": "USD",
    "mileage": 0,
    "fuelType": "Diesel",
    "transmission": "Automatic",
    "bodyType": "SUV",
    "color": "Black",
    "vin": "WBA12345678901234",
    "description": "Luxury SUV with premium features",
    "features": [
      "Leather Seats",
      "Panoramic Roof",
      "Navigation System",
      "Adaptive Cruise Control"
    ],
    "specifications": {
      "engine": "3.0L Inline-6 Turbo",
      "horsepower": 375,
      "transmission": "8-Speed Automatic",
      "drivetrain": "AWD",
      "fuelEconomy": "21/26 MPG"
    },
    "status": "Available"
  }'
```

**Respuesta**:
```json
{
  "id": "vehicle-123",
  "brand": "BMW",
  "model": "X5",
  "year": 2024,
  "price": 75000,
  "currency": "USD",
  "status": "Available",
  "createdAt": "2024-01-15T10:30:00Z",
  "imageUrl": null
}
```

### Buscar Vehículos
```bash
curl -X GET "http://localhost:5009/api/vehicles/search?brand=BMW&yearFrom=2020&yearTo=2024&priceMin=50000&priceMax=100000&transmission=Automatic&page=1&pageSize=20"
```

**Respuesta**:
```json
{
  "data": [
    {
      "id": "vehicle-123",
      "brand": "BMW",
      "model": "X5",
      "year": 2024,
      "price": 75000,
      "mileage": 0,
      "status": "Available",
      "imageUrl": "https://cdn.cardealer.com/vehicles/vehicle-123/main.jpg",
      "createdAt": "2024-01-15T10:30:00Z"
    }
  ],
  "totalCount": 45,
  "page": 1,
  "pageSize": 20,
  "totalPages": 3
}
```

### Actualizar Vehículo
```bash
curl -X PUT http://localhost:5009/api/vehicles/vehicle-123 \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{
    "price": 72000,
    "mileage": 150,
    "status": "Available"
  }'
```

### Subir Imagen
```bash
curl -X POST http://localhost:5009/api/vehicles/vehicle-123/images \
  -H "Authorization: Bearer <token>" \
  -F "file=@vehicle-photo.jpg" \
  -F "isPrimary=true"
```

### Reservar Vehículo
```bash
curl -X POST http://localhost:5009/api/vehicles/vehicle-123/reserve \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{
    "customerId": "customer-456",
    "durationMinutes": 60,
    "notes": "Test drive scheduled for tomorrow"
  }'
```

**Respuesta**:
```json
{
  "reservationId": "res-789",
  "vehicleId": "vehicle-123",
  "customerId": "customer-456",
  "reservedAt": "2024-01-15T10:30:00Z",
  "expiresAt": "2024-01-15T11:30:00Z",
  "status": "Active"
}
```

## 📊 Modelo de Datos

### Vehicle Entity
```csharp
public class Vehicle
{
    // Identity
    public Guid Id { get; set; }
    public string Vin { get; set; }  // Vehicle Identification Number
    
    // Basic Info
    public string Brand { get; set; }
    public string Model { get; set; }
    public int Year { get; set; }
    public string Color { get; set; }
    
    // Pricing
    public decimal Price { get; set; }
    public string Currency { get; set; }
    
    // Status & Availability
    public VehicleStatus Status { get; set; }  // Available, Reserved, Sold, Maintenance
    public int Mileage { get; set; }
    
    // Technical Specs
    public FuelType FuelType { get; set; }  // Gasoline, Diesel, Electric, Hybrid
    public Transmission Transmission { get; set; }  // Manual, Automatic, CVT
    public BodyType BodyType { get; set; }  // Sedan, SUV, Coupe, Truck, Van
    
    // Description
    public string Description { get; set; }
    public string Features { get; set; }  // JSON array
    public string Specifications { get; set; }  // JSON object
    
    // Relations
    public List<VehicleImage> Images { get; set; }
    public List<VehicleReservation> Reservations { get; set; }
    
    // Metadata
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string CreatedBy { get; set; }
    public bool IsDeleted { get; set; }
}
```

### VehicleImage Entity
```csharp
public class VehicleImage
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public string Url { get; set; }
    public bool IsPrimary { get; set; }
    public int Order { get; set; }
    public DateTime UploadedAt { get; set; }
}
```

### VehicleReservation Entity
```csharp
public class VehicleReservation
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public string CustomerId { get; set; }
    public DateTime ReservedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public ReservationStatus Status { get; set; }  // Active, Expired, Cancelled, Completed
    public string Notes { get; set; }
}
```

## 🎯 Business Rules

### Vehicle Creation
- VIN debe ser único
- Año debe estar entre 1900 y año actual + 1
- Precio debe ser mayor a 0
- Status inicial debe ser "Available"

### Vehicle Reservation
- Solo vehículos con status "Available" pueden ser reservados
- Una reserva activa por vehículo a la vez
- Duración máxima: 24 horas
- Reservas expiradas se limpian automáticamente

### Price Updates
- Cambios de precio mayores al 20% requieren aprobación
- Historial de precios se mantiene para auditoría

## 🔄 Event Publishing

### Domain Events
```csharp
// Al crear vehículo
await _messageBus.PublishAsync(new VehicleCreatedEvent
{
    VehicleId = vehicle.Id,
    Brand = vehicle.Brand,
    Model = vehicle.Model,
    Price = vehicle.Price
});

// SearchService lo indexa
// NotificationService notifica a interesados
// AuditService registra la operación
```

### Events Disponibles
- `VehicleCreatedEvent`
- `VehicleUpdatedEvent`
- `VehicleDeletedEvent`
- `VehicleReservedEvent`
- `VehicleSoldEvent`
- `PriceChangedEvent`

## 🧪 Testing

```bash
# Tests unitarios
dotnet test VehicleService.Tests/

# Tests de integración
dotnet test VehicleService.Tests/ --filter "Category=Integration"

# Con cobertura
dotnet test /p:CollectCoverage=true
```

**Nota**: Este servicio necesita tests completos (MEDIA-3 en el plan).

## 🐳 Docker

```bash
# Build
docker build -t vehicleservice:latest .

# Run
docker run -d -p 5009:80 \
  -e ConnectionStrings__DefaultConnection="..." \
  -e RabbitMQ__Host="rabbitmq" \
  --name vehicleservice \
  vehicleservice:latest
```

## 📊 Base de Datos

### Tablas
- `Vehicles` - Información de vehículos
- `VehicleImages` - Imágenes asociadas
- `VehicleReservations` - Reservas
- `VehiclePriceHistory` - Historial de precios
- `VehicleViewHistory` - Historial de visualizaciones

### Índices
```sql
CREATE UNIQUE INDEX IX_Vehicles_Vin ON Vehicles(Vin);
CREATE INDEX IX_Vehicles_Brand_Model ON Vehicles(Brand, Model);
CREATE INDEX IX_Vehicles_Status ON Vehicles(Status);
CREATE INDEX IX_Vehicles_Price ON Vehicles(Price);
CREATE INDEX IX_Vehicles_Year ON Vehicles(Year);
CREATE INDEX IX_VehicleImages_VehicleId ON VehicleImages(VehicleId);
CREATE INDEX IX_VehicleReservations_VehicleId ON VehicleReservations(VehicleId);
CREATE INDEX IX_VehicleReservations_Status ON VehicleReservations(Status);
```

## 📈 Monitoreo

### Métricas
- `vehicles_created_total` - Vehículos creados
- `vehicles_sold_total` - Vehículos vendidos
- `vehicles_reserved_total` - Reservas realizadas
- `vehicles_views_total` - Visualizaciones
- `inventory_value_total` - Valor total del inventario

### KPIs
- Inventory turnover rate
- Average days in inventory
- Price trends by brand/model
- Reservation conversion rate

## 🔄 Integraciones

### MediaService
- Upload de imágenes de vehículos
- Optimización y resize automático
- CDN distribution

### SearchService
- Indexación automática al crear/actualizar
- Full-text search
- Faceted search

### NotificationService
- Alertas de nuevos vehículos
- Notificaciones de cambios de precio
- Recordatorios de reservas

### AuditService
- Tracking de todas las operaciones CRUD
- Historial de cambios

## 🔐 Seguridad

- **Autenticación**: JWT tokens requeridos para operaciones de escritura
- **Autorización**: 
  - `Admin`: CRUD completo
  - `Dealer`: Crear y modificar vehículos propios
  - `Customer`: Solo lectura
- **Validación**: FluentValidation en todos los comandos
- **Sanitización**: XSS protection en descripciones

## 📱 Query Optimization

### Paginación
```csharp
var vehicles = await _dbContext.Vehicles
    .Where(v => !v.IsDeleted && v.Status == VehicleStatus.Available)
    .OrderByDescending(v => v.CreatedAt)
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync();
```

### Caching
- Vehicle details: Cache 5 minutos
- Search results: Cache 2 minutos
- Statistics: Cache 10 minutos

## 🚦 Estado

- ✅ **Build**: OK
- ⚠️ **Tests**: Pendiente (MEDIA-3)
- ✅ **Docker**: Configurado
- ✅ **Events**: RabbitMQ integrado

---

**Puerto**: 5009  
**Base de Datos**: PostgreSQL (vehicledb)  
**Message Queue**: RabbitMQ  
**Estado**: ⚠️ Tests Pendientes
