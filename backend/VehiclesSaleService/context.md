# VehiclesSaleService - Context Documentation

## 📋 INFORMACIÓN GENERAL

- **Nombre del Servicio:** VehiclesSaleService
- **Puerto en Kubernetes:** 8080
- **Puerto en Desarrollo:** 5004
- **Estado:** ✅ **EN PRODUCCIÓN** (Servicio Principal)
- **Base de Datos:** PostgreSQL (`vehiclessaleservice`)
- **Imagen Docker:** ghcr.io/gregorymorenoiem/cardealer-vehiclessaleservice:latest

### Propósito
**Servicio principal de OKLA**. Gestiona el catálogo completo de vehículos en venta, incluyendo CRUD de vehículos, catálogo de marcas/modelos, sistema de homepage sections dinámicas, búsqueda y filtrado. Es el core del marketplace.

---

## 🏗️ ARQUITECTURA

```
VehiclesSaleService/
├── VehiclesSaleService.Api/
│   ├── Controllers/
│   │   ├── VehiclesController.cs           # CRUD de vehículos
│   │   ├── CatalogController.cs            # Marcas/Modelos/Años
│   │   ├── CategoriesController.cs         # Categorías
│   │   └── HomepageSectionsController.cs   # Secciones del homepage
│   ├── Program.cs
│   └── Dockerfile
├── VehiclesSaleService.Application/
│   ├── Features/
│   │   ├── Commands/
│   │   │   ├── CreateVehicleCommand.cs
│   │   │   ├── UpdateVehicleCommand.cs
│   │   │   ├── DeleteVehicleCommand.cs
│   │   │   └── PublishVehicleCommand.cs
│   │   └── Queries/
│   │       ├── GetVehicleByIdQuery.cs
│   │       ├── SearchVehiclesQuery.cs
│   │       ├── GetHomepageSectionsQuery.cs
│   │       └── GetCatalogQuery.cs
│   └── DTOs/
│       ├── VehicleDto.cs
│       ├── VehicleFilterDto.cs
│       ├── CatalogDto.cs
│       └── HomepageSectionDto.cs
├── VehiclesSaleService.Domain/
│   ├── Entities/
│   │   ├── Vehicle.cs                      # Entidad principal (342 líneas)
│   │   ├── Category.cs
│   │   ├── VehicleImage.cs
│   │   ├── VehicleMake.cs                  # Toyota, Honda, Ford
│   │   ├── VehicleModel.cs                 # Camry, Civic, F-150
│   │   ├── VehicleTrim.cs                  # LE, SE, XLE
│   │   ├── HomepageSectionConfig.cs        # Configuración de secciones
│   │   └── VehicleHomepageSection.cs       # Relación vehículo-sección
│   ├── Enums/
│   │   ├── VehicleStatus.cs                # Draft, Active, Sold, etc.
│   │   ├── VehicleType.cs                  # Car, Truck, SUV, etc.
│   │   ├── BodyStyle.cs                    # Sedan, Coupe, SUV, etc.
│   │   ├── FuelType.cs                     # Gasoline, Electric, Hybrid
│   │   ├── TransmissionType.cs             # Automatic, Manual, CVT
│   │   └── HomepageSection.cs              # Flags para secciones
│   └── Interfaces/
│       ├── IVehicleRepository.cs
│       └── IVehicleCatalogRepository.cs
└── VehiclesSaleService.Infrastructure/
    ├── Persistence/
    │   ├── ApplicationDbContext.cs
    │   ├── Migrations/
    │   └── Repositories/
    │       ├── VehicleRepository.cs
    │       └── VehicleCatalogRepository.cs
    └── Messaging/
        └── RabbitMqEventPublisher.cs
```

---

## 📦 ENTIDAD PRINCIPAL: Vehicle

### Propiedades (342 líneas de código)

```csharp
public class Vehicle : ITenantEntity
{
    // IDENTIFICACIÓN
    public Guid Id { get; set; }
    public Guid DealerId { get; set; }          // Multi-tenant
    public string? VIN { get; set; }            // Vehicle Identification Number
    public string? StockNumber { get; set; }    // Número de inventario
    
    // INFORMACIÓN BÁSICA
    public string Title { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public VehicleStatus Status { get; set; }   // Draft, Active, Sold
    
    // VENDEDOR
    public Guid SellerId { get; set; }
    public string SellerName { get; set; }
    public SellerType SellerType { get; set; }  // Individual, Dealer
    public string? SellerPhone { get; set; }
    public string? SellerEmail { get; set; }
    public string? SellerWhatsApp { get; set; }
    public bool SellerVerified { get; set; }
    public decimal? SellerRating { get; set; }
    
    // MARCA/MODELO/AÑO
    public Guid? MakeId { get; set; }
    public string Make { get; set; }            // Toyota, Honda, Ford
    public Guid? ModelId { get; set; }
    public string Model { get; set; }           // Camry, Civic, F-150
    public string? Trim { get; set; }           // LE, SE, XLE
    public int Year { get; set; }
    public string? Generation { get; set; }
    
    // TIPO Y CARROCERÍA
    public VehicleType VehicleType { get; set; }  // Car, Truck, SUV
    public BodyStyle BodyStyle { get; set; }      // Sedan, Coupe, SUV
    public int Doors { get; set; } = 4;
    public int Seats { get; set; } = 5;
    
    // MOTOR Y TRANSMISIÓN
    public FuelType FuelType { get; set; }        // Gasoline, Electric, Hybrid
    public string? EngineSize { get; set; }       // 2.5L, 3.0L
    public int? Horsepower { get; set; }
    public int? Torque { get; set; }
    public TransmissionType Transmission { get; set; }  // Automatic, Manual
    public DriveType DriveType { get; set; }      // FWD, RWD, AWD, 4WD
    public int? Cylinders { get; set; }
    
    // KILOMETRAJE Y CONDICIÓN
    public int Mileage { get; set; }
    public MileageUnit MileageUnit { get; set; }  // Miles, Kilometers
    public VehicleCondition Condition { get; set; }  // New, Used, CPO
    public int? PreviousOwners { get; set; }
    public bool AccidentHistory { get; set; }
    public bool HasCleanTitle { get; set; }
    
    // APARIENCIA
    public string? ExteriorColor { get; set; }
    public string? InteriorColor { get; set; }
    public string? InteriorMaterial { get; set; }  // Leather, Cloth
    
    // ECONOMÍA DE COMBUSTIBLE
    public int? MpgCity { get; set; }
    public int? MpgHighway { get; set; }
    public int? MpgCombined { get; set; }
    
    // UBICACIÓN
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? Country { get; set; } = "USA";
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    
    // HISTORIAL Y CERTIFICACIONES
    public bool IsCertified { get; set; }
    public string? CertificationProgram { get; set; }
    public string? CarfaxReportUrl { get; set; }
    public DateTime? LastServiceDate { get; set; }
    public string? ServiceHistoryNotes { get; set; }
    public string? WarrantyInfo { get; set; }
    
    // CARACTERÍSTICAS (JSON Arrays)
    public string FeaturesJson { get; set; } = "[]";
    // Ejemplo: ["Sunroof", "Navigation", "Leather Seats", "Backup Camera"]
    public string PackagesJson { get; set; } = "[]";
    // Ejemplo: ["Technology Package", "Premium Package"]
    
    // MÉTRICAS
    public int ViewCount { get; set; }
    public int FavoriteCount { get; set; }
    public int InquiryCount { get; set; }
    
    // HOMEPAGE SECTIONS (Flags Enum)
    public HomepageSection HomepageSections { get; set; }
    
    // METADATOS
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime? SoldAt { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsFeatured { get; set; }
    
    // NAVEGACIÓN
    public Guid? CategoryId { get; set; }
    public Category? Category { get; set; }
    public ICollection<VehicleImage> Images { get; set; }
    public ICollection<VehicleHomepageSection> HomepageSectionAssignments { get; set; }
}
```

### Enums Principales

#### VehicleStatus
```csharp
public enum VehicleStatus
{
    Draft = 0,          // Borrador
    PendingReview = 1,  // Pendiente de revisión
    Active = 2,         // Publicado y visible
    Reserved = 3,       // Reservado
    Sold = 4,           // Vendido
    Archived = 5,       // Archivado
    Rejected = 6        // Rechazado
}
```

#### VehicleType
```csharp
public enum VehicleType
{
    Car, Truck, SUV, Van, Motorcycle, RV, Boat, ATV, Commercial, Other
}
```

#### BodyStyle
```csharp
public enum BodyStyle
{
    Sedan, Coupe, Hatchback, Wagon, SUV, Crossover, Pickup, Van, 
    Minivan, Convertible, SportsCar, Other
}
```

#### FuelType
```csharp
public enum FuelType
{
    Gasoline, Diesel, Electric, Hybrid, PlugInHybrid, Hydrogen, 
    FlexFuel, NaturalGas, Other
}
```

#### HomepageSection (Flags)
```csharp
[Flags]
public enum HomepageSection
{
    None = 0,
    Carousel = 1,
    Sedanes = 2,
    SUVs = 4,
    Camionetas = 8,
    Deportivos = 16,
    Destacados = 32,
    Lujo = 64,
    All = Carousel | Sedanes | SUVs | Camionetas | Deportivos | Destacados | Lujo
}
```

---

## 🏠 SISTEMA DE HOMEPAGE SECTIONS

### Arquitectura

El sistema permite mostrar vehículos en secciones dinámicas del homepage (okla.com.do).

```
Frontend (React) 
    ↓ GET /api/homepagesections/homepage
Gateway (Ocelot) 
    ↓ → vehiclessaleservice:8080
VehiclesSaleService
    ↓ Query a PostgreSQL
Tablas: homepage_section_configs, vehicle_homepage_sections, vehicles
```

### Tablas

#### homepage_section_configs
Configuración de cada sección del homepage.

```sql
CREATE TABLE homepage_section_configs (
    "Id" uuid PRIMARY KEY,
    "Name" varchar(100) NOT NULL,              -- "Sedanes", "SUVs", "Destacados"
    "Slug" varchar(100) NOT NULL UNIQUE,       -- "sedanes", "suvs"
    "DisplayOrder" integer NOT NULL,           -- Orden de aparición (1, 2, 3...)
    "MaxItems" integer NOT NULL DEFAULT 10,    -- Máximo de vehículos a mostrar
    "IsActive" boolean NOT NULL DEFAULT true,
    "Subtitle" varchar(200),                   -- Descripción corta
    "AccentColor" varchar(50),                 -- "blue", "amber", "green"
    "ViewAllHref" varchar(200),                -- Link "Ver todos"
    "CreatedAt" timestamp NOT NULL,
    "UpdatedAt" timestamp
);
```

#### vehicle_homepage_sections
Relación muchos-a-muchos entre vehículos y secciones.

```sql
CREATE TABLE vehicle_homepage_sections (
    "VehicleId" uuid NOT NULL,
    "HomepageSectionConfigId" uuid NOT NULL,
    "SortOrder" integer NOT NULL DEFAULT 0,    -- Orden dentro de la sección
    "IsPinned" boolean NOT NULL DEFAULT false, -- Fijado al inicio
    "StartDate" timestamp,                     -- Fecha inicio (opcional)
    "EndDate" timestamp,                       -- Fecha fin (opcional)
    PRIMARY KEY ("VehicleId", "HomepageSectionConfigId"),
    FOREIGN KEY ("VehicleId") REFERENCES vehicles("Id"),
    FOREIGN KEY ("HomepageSectionConfigId") REFERENCES homepage_section_configs("Id")
);
```

### Secciones Actuales en Producción

| # | Sección | MaxItems | Vehículos Asignados |
|---|---------|----------|---------------------|
| 1 | Carousel Principal | 5 | 10 |
| 2 | Sedanes | 10 | 10 |
| 3 | SUVs | 10 | 10 |
| 4 | Camionetas | 10 | 10 |
| 5 | Deportivos | 10 | 10 |
| 6 | Destacados | 9 | 10 |
| 7 | Lujo | 10 | 10 |
| 8 | Vehículos Eléctricos | 10 | 15 |
| 9 | Eficiencia Total | 10 | 10 |
| 10 | Muscle & Performance | 10 | 10 |

### Endpoint Principal

#### GET `/api/homepagesections/homepage`
Retorna todas las secciones activas del homepage con sus vehículos.

**Lógica:**
1. Obtiene secciones activas ordenadas por `DisplayOrder`
2. Para cada sección, obtiene vehículos con `Status = 'Active'`
3. Limita a `MaxItems` vehículos por sección
4. Ordena por `SortOrder` y `IsPinned`

**Response (200 OK):**
```json
[
  {
    "name": "Carousel Principal",
    "slug": "carousel-principal",
    "subtitle": "Los mejores vehículos del mes",
    "accentColor": "blue",
    "vehicles": [
      {
        "id": "...",
        "title": "2024 Toyota Camry XLE",
        "make": "Toyota",
        "model": "Camry",
        "year": 2024,
        "price": 28500,
        "currency": "USD",
        "mileage": 15000,
        "images": [...]
      }
    ]
  },
  {
    "name": "Sedanes",
    "slug": "sedanes",
    "vehicles": [...]
  }
]
```

### Flujo Completo

1. **Usuario accede a okla.com.do**
2. **Frontend carga VehiclesOnlyHomePage.tsx**
3. **useHomepageSections() hook** hace fetch a `/api/homepagesections/homepage`
4. **Gateway** rutea a `vehiclessaleservice:8080/api/homepagesections/homepage`
5. **HomepageSectionsController** consulta PostgreSQL:
   - Secciones activas ordenadas por `DisplayOrder`
   - Vehículos con `Status='Active'`
   - Limita a `MaxItems`
6. **API retorna JSON** con secciones y vehículos
7. **Frontend renderiza**:
   - `HeroCarousel` con Carousel Principal
   - `FeaturedListingGrid` con Destacados (maxItems=9)
   - `FeaturedSection` para cada categoría

---

## 📡 ENDPOINTS API

### Vehículos

#### GET `/api/vehicles`
Listar vehículos con filtros y paginación.

**Query Parameters:**
- `page`: Número de página (default: 1)
- `pageSize`: Tamaño de página (default: 10, max: 100)
- `status`: Filtrar por estado (Active, Sold, etc.)
- `make`: Filtrar por marca
- `model`: Filtrar por modelo
- `year`: Filtrar por año
- `minPrice`: Precio mínimo
- `maxPrice`: Precio máximo
- `vehicleType`: Tipo de vehículo
- `fuelType`: Tipo de combustible
- `transmission`: Tipo de transmisión
- `city`: Ciudad
- `state`: Estado

**Response (200 OK):**
```json
{
  "items": [
    {
      "id": "...",
      "title": "2024 Toyota Camry XLE",
      "description": "...",
      "price": 28500,
      "currency": "USD",
      "make": "Toyota",
      "model": "Camry",
      "year": 2024,
      "mileage": 15000,
      "status": "Active",
      "images": [
        {
          "url": "https://s3.amazonaws.com/...",
          "isPrimary": true
        }
      ],
      "sellerName": "ABC Motors",
      "city": "Santo Domingo"
    }
  ],
  "totalCount": 250,
  "page": 1,
  "pageSize": 10,
  "totalPages": 25
}
```

#### GET `/api/vehicles/{id}`
Obtener detalle completo de un vehículo.

**Response (200 OK):** (Objeto Vehicle completo con todas las propiedades)

#### POST `/api/vehicles`
Crear nuevo vehículo (requiere autenticación).

**Request:**
```json
{
  "title": "2024 Toyota Camry XLE",
  "description": "Excelente condición, un solo dueño",
  "price": 28500,
  "make": "Toyota",
  "model": "Camry",
  "year": 2024,
  "mileage": 15000,
  "transmission": "Automatic",
  "fuelType": "Gasoline",
  "bodyStyle": "Sedan",
  "exteriorColor": "White",
  "city": "Santo Domingo",
  "state": "Distrito Nacional"
}
```

**Response (201 Created):**
```json
{
  "id": "...",
  "title": "2024 Toyota Camry XLE",
  "status": "Draft",
  "createdAt": "2026-01-07T10:30:00Z"
}
```

#### PUT `/api/vehicles/{id}`
Actualizar vehículo.

#### DELETE `/api/vehicles/{id}`
Eliminar vehículo (soft delete).

#### POST `/api/vehicles/{id}/publish`
Publicar vehículo (cambia status a Active).

### Catálogo

#### GET `/api/catalog/makes`
Obtener todas las marcas de vehículos.

**Response (200 OK):**
```json
[
  {
    "id": "...",
    "name": "Toyota",
    "slug": "toyota",
    "logoUrl": "...",
    "vehicleCount": 150
  },
  {
    "id": "...",
    "name": "Honda",
    "slug": "honda",
    "logoUrl": "...",
    "vehicleCount": 120
  }
]
```

#### GET `/api/catalog/models/{makeId}`
Obtener modelos de una marca específica.

**Response (200 OK):**
```json
[
  {
    "id": "...",
    "name": "Camry",
    "makeId": "...",
    "makeName": "Toyota",
    "vehicleCount": 45
  },
  {
    "id": "...",
    "name": "Corolla",
    "makeId": "...",
    "makeName": "Toyota",
    "vehicleCount": 38
  }
]
```

#### GET `/api/catalog/years`
Obtener años disponibles.

**Response (200 OK):**
```json
[2025, 2024, 2023, 2022, 2021, 2020, ...]
```

### Categorías

#### GET `/api/categories`
Obtener categorías de vehículos.

**Response (200 OK):**
```json
[
  {
    "id": "...",
    "name": "Sedanes",
    "slug": "sedanes",
    "vehicleCount": 85
  }
]
```

---

## 🔧 TECNOLOGÍAS Y DEPENDENCIAS

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />
<PackageReference Include="MediatR" Version="12.2.0" />
<PackageReference Include="FluentValidation" Version="11.9.0" />
<PackageReference Include="RabbitMQ.Client" Version="6.8.1" />
<PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
<PackageReference Include="OpenTelemetry" Version="1.7.0" />
<PackageReference Include="CarDealer.Shared" />
<PackageReference Include="CarDealer.Contracts" />
```

---

## ⚙️ CONFIGURACIÓN

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=${DB_HOST};Database=vehiclessaleservice;Username=${DB_USER};Password=${DB_PASSWORD}"
  },
  "RabbitMQ": {
    "Host": "rabbitmq",
    "Port": 5672
  }
}
```

---

## 🗃️ BASE DE DATOS

### Tablas Principales
- **vehicles**: Vehículos (columna principal con 50+ campos)
- **vehicle_images**: Imágenes de vehículos
- **categories**: Categorías
- **vehicle_makes**: Marcas (Toyota, Honda, etc.)
- **vehicle_models**: Modelos (Camry, Civic, etc.)
- **vehicle_trims**: Versiones (LE, SE, XLE, etc.)
- **homepage_section_configs**: Configuración de secciones del homepage
- **vehicle_homepage_sections**: Relación vehículo-sección

### Índices Críticos
```sql
CREATE INDEX idx_vehicles_status ON vehicles (status);
CREATE INDEX idx_vehicles_make ON vehicles (make);
CREATE INDEX idx_vehicles_year ON vehicles (year);
CREATE INDEX idx_vehicles_price ON vehicles (price);
CREATE INDEX idx_vehicles_created_at ON vehicles (created_at DESC);
CREATE INDEX idx_vehicles_dealer_id ON vehicles (dealer_id);
```

---

## 🔄 EVENTOS PUBLICADOS

### VehicleCreatedEvent
```csharp
public record VehicleCreatedEvent(
    Guid VehicleId,
    Guid SellerId,
    string Make,
    string Model,
    int Year,
    decimal Price,
    DateTime CreatedAt
);
```

**Exchange:** `vehicle.events`  
**Routing Key:** `vehicle.created`

### VehiclePublishedEvent
Cuando un vehículo se publica (status → Active).

### VehicleSoldEvent
Cuando un vehículo se marca como vendido.

---

## 📝 REGLAS DE NEGOCIO

### Publicación de Vehículos
1. Debe tener al menos 1 imagen
2. Campos obligatorios: Title, Make, Model, Year, Price
3. Status Draft → PendingReview (si requiere moderación) → Active

### Homepage Sections
1. Un vehículo puede estar en múltiples secciones
2. El límite `MaxItems` se aplica en la query (`.Take(MaxItems)`)
3. Solo vehículos con `Status = 'Active'` aparecen
4. Orden: `IsPinned DESC, SortOrder ASC`

### Multi-Tenancy
1. Cada vehículo tiene un `DealerId`
2. Los dealers solo pueden ver/editar sus propios vehículos
3. Admins pueden ver todos los vehículos

---

## 🔗 RELACIONES CON OTROS SERVICIOS

### Publica Eventos A:
- **NotificationService**: Notificaciones de publicación
- **MediaService**: Sincronización de imágenes
- **SearchService**: Indexación para búsqueda

### Consulta A:
- **UserService**: Información del vendedor
- **MediaService**: Upload de imágenes

### Consultado Por:
- **Frontend Web**: okla.com.do
- **Frontend Mobile**: App Flutter
- **BillingService**: Pagos por publicación

---

## 🚀 DESPLIEGUE

### Kubernetes
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: vehiclessaleservice
  namespace: okla
spec:
  replicas: 3  # Servicio principal, más réplicas
  template:
    spec:
      containers:
      - name: vehiclessaleservice
        image: ghcr.io/gregorymorenoiem/cardealer-vehiclessaleservice:latest
        ports:
        - containerPort: 8080
```

---

## 🐛 TROUBLESHOOTING

### Homepage Sections no muestran vehículos

**Verificar:**
```bash
# 1. Verificar configuración de secciones
kubectl exec -it postgres-0 -n okla -- psql -U postgres -d vehiclessaleservice -c \
  'SELECT "Name", "MaxItems", "IsActive" FROM homepage_section_configs ORDER BY "DisplayOrder";'

# 2. Verificar vehículos asignados
kubectl exec -it postgres-0 -n okla -- psql -U postgres -d vehiclessaleservice -c \
  'SELECT hsc."Name", COUNT(vhs."VehicleId") as total
   FROM homepage_section_configs hsc
   LEFT JOIN vehicle_homepage_sections vhs ON hsc."Id" = vhs."HomepageSectionConfigId"
   GROUP BY hsc."Name";'

# 3. Verificar respuesta del API
curl -s "https://api.okla.com.do/api/homepagesections/homepage" | \
  python3 -c "import json,sys; [print(f\"{s['name']}: {len(s['vehicles'])}\") for s in json.load(sys.stdin)]"
```

---

## 📅 ÚLTIMA ACTUALIZACIÓN

**Fecha:** Enero 7, 2026  
**Versión:** 1.0.0  
**Estado:** Producción estable en DOKS (Servicio Principal)
