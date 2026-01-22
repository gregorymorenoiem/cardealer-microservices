# 🏠 Propiedades en Venta - Matriz de Procesos

## 📋 Información General

| Aspecto           | Detalle                                                                             |
| ----------------- | ----------------------------------------------------------------------------------- |
| **Servicio**      | PropertiesSaleService                                                               |
| **Puerto**        | 5024                                                                                |
| **Base de Datos** | PostgreSQL (propertiessale_db)                                                      |
| **Tecnología**    | .NET 8, Entity Framework Core                                                       |
| **Multi-tenancy** | Por agencia inmobiliaria (DealerId)                                                 |
| **Descripción**   | Gestión de propiedades inmobiliarias en venta: casas, condos, terrenos, comerciales |

---

## 🎯 Endpoints del Servicio

### PropertiesController

| Método   | Endpoint                            | Descripción                    | Auth | Roles        |
| -------- | ----------------------------------- | ------------------------------ | ---- | ------------ |
| `GET`    | `/api/properties`                   | Buscar propiedades con filtros | ❌   | Público      |
| `GET`    | `/api/properties/{id}`              | Obtener propiedad por ID       | ❌   | Público      |
| `GET`    | `/api/properties/mls/{mlsNumber}`   | Obtener por número MLS         | ❌   | Público      |
| `GET`    | `/api/properties/featured`          | Propiedades destacadas         | ❌   | Público      |
| `GET`    | `/api/properties/seller/{sellerId}` | Propiedades de un vendedor     | ❌   | Público      |
| `GET`    | `/api/properties/dealer/{dealerId}` | Propiedades de una agencia     | ❌   | Público      |
| `POST`   | `/api/properties`                   | Crear propiedad                | ✅   | Agent, Admin |
| `PUT`    | `/api/properties/{id}`              | Actualizar propiedad           | ✅   | Agent, Admin |
| `DELETE` | `/api/properties/{id}`              | Eliminar propiedad (soft)      | ✅   | Agent, Admin |

### CategoriesController

| Método | Endpoint                        | Descripción                 | Auth | Roles   |
| ------ | ------------------------------- | --------------------------- | ---- | ------- |
| `GET`  | `/api/categories`               | Listar todas las categorías | ❌   | Público |
| `GET`  | `/api/categories/root`          | Categorías raíz             | ❌   | Público |
| `GET`  | `/api/categories/{id}`          | Categoría por ID            | ❌   | Público |
| `GET`  | `/api/categories/slug/{slug}`   | Categoría por slug          | ❌   | Público |
| `GET`  | `/api/categories/{id}/children` | Subcategorías               | ❌   | Público |

---

## 📊 Entidades del Dominio

### Property (Propiedad Inmobiliaria)

```csharp
public class Property : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid DealerId { get; set; }          // Multi-tenancy

    // ========================================
    // INFORMACIÓN BÁSICA
    // ========================================
    public string Title { get; set; }           // "Beautiful 3BR House in Miami"
    public string Description { get; set; }
    public decimal Price { get; set; }          // 450000.00
    public string Currency { get; set; }        // "USD"
    public PropertyStatus Status { get; set; }

    // ========================================
    // IDENTIFICACIÓN
    // ========================================
    public string? MLSNumber { get; set; }      // "MLS123456"
    public string? ParcelNumber { get; set; }   // Tax parcel
    public string? PropertyId { get; set; }     // County ID

    // ========================================
    // TIPO DE PROPIEDAD
    // ========================================
    public PropertyType PropertyType { get; set; }
    public PropertySubType PropertySubType { get; set; }
    public OwnershipType OwnershipType { get; set; }

    // ========================================
    // TAMAÑO Y DIMENSIONES
    // ========================================
    public int? SquareFeet { get; set; }        // 2400
    public int? LotSizeSquareFeet { get; set; } // 8500
    public decimal? LotSizeAcres { get; set; }  // 0.19
    public int? Stories { get; set; }           // 2
    public int? YearBuilt { get; set; }         // 2015
    public int? YearRenovated { get; set; }

    // ========================================
    // HABITACIONES
    // ========================================
    public int Bedrooms { get; set; }           // 3
    public int Bathrooms { get; set; }          // 2
    public int? HalfBathrooms { get; set; }     // 1
    public int? RoomsTotal { get; set; }        // 8

    // ========================================
    // ESTACIONAMIENTO
    // ========================================
    public int? GarageSpaces { get; set; }      // 2
    public GarageType GarageType { get; set; }
    public int? ParkingSpaces { get; set; }
    public ParkingType ParkingType { get; set; }

    // ========================================
    // CONSTRUCCIÓN
    // ========================================
    public string? ConstructionType { get; set; }   // "Wood Frame", "Concrete"
    public string? RoofType { get; set; }           // "Shingle", "Tile"
    public string? ExteriorType { get; set; }       // "Brick", "Stucco"
    public string? FoundationType { get; set; }
    public ArchitecturalStyle ArchitecturalStyle { get; set; }

    // ========================================
    // SISTEMAS
    // ========================================
    public HeatingType HeatingType { get; set; }
    public CoolingType CoolingType { get; set; }
    public string? HeatingFuel { get; set; }        // "Gas", "Electric"
    public string? WaterSource { get; set; }        // "Municipal", "Well"
    public string? SewerType { get; set; }          // "Municipal", "Septic"

    // ========================================
    // UBICACIÓN
    // ========================================
    public string StreetAddress { get; set; }
    public string? UnitNumber { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string ZipCode { get; set; }
    public string? County { get; set; }
    public string Country { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Neighborhood { get; set; }
    public string? Subdivision { get; set; }

    // ========================================
    // INFORMACIÓN FINANCIERA
    // ========================================
    public decimal? TaxesYearly { get; set; }       // 5200.00
    public int? TaxYear { get; set; }
    public decimal? HOAFeesMonthly { get; set; }    // 150.00
    public string? HOAName { get; set; }
    public decimal? AssessedValue { get; set; }
    public decimal? PricePerSquareFoot { get; set; } // 187.50

    // ========================================
    // CARACTERÍSTICAS (JSON Arrays)
    // ========================================
    public string InteriorFeaturesJson { get; set; }    // ["Hardwood Floors", "Granite"]
    public string ExteriorFeaturesJson { get; set; }    // ["Pool", "Patio", "Fence"]
    public string CommunityAmenitiesJson { get; set; }  // ["Clubhouse", "Tennis"]
    public string AppliancesJson { get; set; }          // ["Refrigerator", "Dishwasher"]

    // ========================================
    // PISCINA Y EXTRAS
    // ========================================
    public bool HasPool { get; set; }
    public PoolType PoolType { get; set; }
    public bool HasSpa { get; set; }
    public bool HasFireplace { get; set; }
    public int? FireplaceCount { get; set; }
    public bool HasBasement { get; set; }
    public BasementType BasementType { get; set; }

    // ========================================
    // ESCUELAS
    // ========================================
    public string? ElementarySchool { get; set; }
    public string? MiddleSchool { get; set; }
    public string? HighSchool { get; set; }
    public string? SchoolDistrict { get; set; }

    // ========================================
    // INFORMACIÓN DE VENTA
    // ========================================
    public bool IsNewConstruction { get; set; }
    public bool IsForeclosure { get; set; }
    public bool IsShortSale { get; set; }
    public bool VirtualTourAvailable { get; set; }
    public string? VirtualTourUrl { get; set; }
    public DateTime? OpenHouseDate { get; set; }
    public DateTime? ListingDate { get; set; }
    public DateTime? ContractDate { get; set; }
    public DateTime? ClosingDate { get; set; }

    // ========================================
    // HISTORIAL
    // ========================================
    public decimal? OriginalPrice { get; set; }
    public int? DaysOnMarket { get; set; }
    public int PriceChanges { get; set; }

    // ========================================
    // ENGAGEMENT
    // ========================================
    public int ViewCount { get; set; }
    public int SavedCount { get; set; }
    public int InquiryCount { get; set; }
    public int TourRequestCount { get; set; }

    // Navigation
    public Guid? CategoryId { get; set; }
    public Category? Category { get; set; }
    public ICollection<PropertyImage> Images { get; set; }
}
```

### Enums de Propiedad

```csharp
public enum PropertyStatus
{
    Draft = 0,           // Borrador
    PendingReview = 1,   // Pendiente revisión
    Active = 2,          // Activo (publicado)
    UnderContract = 3,   // Bajo contrato
    Pending = 4,         // Pendiente de cierre
    Sold = 5,            // Vendido
    Closed = 6,          // Cerrado
    Withdrawn = 7,       // Retirado
    Expired = 8,         // Expirado
    Archived = 9         // Archivado
}

public enum PropertyType
{
    House = 0,           // Casa
    Condo = 1,           // Condominio
    Townhouse = 2,       // Casa adosada
    MultiFamily = 3,     // Multi-familiar
    Apartment = 4,       // Apartamento
    Land = 5,            // Terreno
    Commercial = 6,      // Comercial
    Industrial = 7,      // Industrial
    Farm = 8,            // Finca
    MobileHome = 9,      // Casa móvil
    Other = 99
}

public enum PropertySubType
{
    SingleFamily = 0,    // Unifamiliar
    Duplex = 1,
    Triplex = 2,
    Fourplex = 3,
    Condo = 4,
    Loft = 5,
    Penthouse = 6,
    Studio = 7,
    Attached = 8,
    Detached = 9,
    VacantLand = 10,
    Office = 11,
    Retail = 12,
    Warehouse = 13,
    Other = 99
}

public enum ArchitecturalStyle
{
    Colonial = 0,
    Contemporary = 1,
    Craftsman = 2,
    Mediterranean = 3,
    MidCenturyModern = 4,
    Ranch = 5,
    Tudor = 6,
    Victorian = 7,
    Traditional = 8,
    Modern = 9,
    Farmhouse = 10,
    Cape = 11,
    Other = 99
}

public enum HeatingType { None, Forced, Radiant, Baseboard, HeatPump, Geothermal, Other = 99 }
public enum CoolingType { None, Central, Window, Split, Evaporative, Geothermal, Other = 99 }
public enum GarageType { None, Attached, Detached, Carport, Other = 99 }
public enum ParkingType { None, Driveway, Street, Lot, Covered, Underground, Other = 99 }
public enum PoolType { None, InGround, AboveGround, Indoor, Community, Other = 99 }
public enum BasementType { None, Full, Partial, Finished, Unfinished, Crawl, WalkOut, Other = 99 }
public enum OwnershipType { Fee, Leasehold, Coop, Timeshare, Other = 99 }
```

### Category (Categorías)

```csharp
public class Category : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid DealerId { get; set; }
    public Guid? ParentId { get; set; }         // Para jerarquía

    public string Name { get; set; }            // "Casas", "Condominios"
    public string Slug { get; set; }            // "casas", "condominios"
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public string? ImageUrl { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public bool IsSystem { get; set; }          // Categorías predefinidas

    public Category? Parent { get; set; }
    public ICollection<Category> Children { get; set; }
    public ICollection<Property> Properties { get; set; }
}
```

---

## 🔄 Procesos Detallados

### PROCESO 1: Búsqueda de Propiedades

#### Endpoint: `GET /api/properties`

| Paso | Actor      | Acción                             | Sistema               | Resultado              |
| ---- | ---------- | ---------------------------------- | --------------------- | ---------------------- |
| 1    | Usuario    | Aplica filtros de búsqueda         | HTTP GET              | Request recibido       |
| 2    | Controller | Construye PropertySearchParameters | Map request → params  | Parámetros construidos |
| 3    | Repository | Construye query base               | IQueryable<Property>  | Query inicial          |
| 4    | Repository | Aplica filtro SearchTerm           | WHERE Title/Desc LIKE | Filtro texto           |
| 5    | Repository | Aplica filtro precio               | WHERE Price BETWEEN   | Filtro rango           |
| 6    | Repository | Aplica filtro tipo                 | WHERE PropertyType =  | Filtro enum            |
| 7    | Repository | Aplica filtro habitaciones         | WHERE Bedrooms >=     | Filtro numérico        |
| 8    | Repository | Aplica filtro ubicación            | WHERE State/City =    | Filtro geográfico      |
| 9    | Repository | Aplica filtro características      | WHERE HasPool = true  | Filtro booleano        |
| 10   | Repository | Ordena resultados                  | ORDER BY              | Ordenado               |
| 11   | Repository | Pagina resultados                  | SKIP/TAKE             | Paginado               |
| 12   | Repository | Cuenta total                       | COUNT(\*)             | TotalCount             |
| 13   | API        | Retorna PropertySearchResult       | HTTP 200              | Respuesta              |

#### Parámetros de Búsqueda Disponibles

| Categoría           | Parámetros                                           |
| ------------------- | ---------------------------------------------------- |
| **Texto**           | Search, Neighborhood, City, State, ZipCode           |
| **Precio**          | MinPrice, MaxPrice                                   |
| **Tipo**            | PropertyType, PropertySubType                        |
| **Habitaciones**    | MinBedrooms, MaxBedrooms, MinBathrooms, MaxBathrooms |
| **Tamaño**          | MinSquareFeet, MaxSquareFeet                         |
| **Año**             | MinYearBuilt, MaxYearBuilt                           |
| **Características** | HasPool, HasGarage, HasBasement, HasFireplace        |
| **Sistemas**        | HeatingType, CoolingType                             |
| **Paginación**      | Page, PageSize, SortBy, SortDescending               |

---

### PROCESO 2: Crear Propiedad para Venta

#### Endpoint: `POST /api/properties`

| Paso | Actor      | Acción                   | Sistema                    | Resultado             |
| ---- | ---------- | ------------------------ | -------------------------- | --------------------- |
| 1    | Agente     | Envía datos de propiedad | HTTP POST                  | Request recibido      |
| 2    | Controller | Valida categoría existe  | CategoryRepository.GetById | Categoría validada    |
| 3    | Controller | Si categoría no existe   | HTTP 400                   | "Category not found"  |
| 4    | Controller | Crea entidad Property    | new Property()             | Entidad creada        |
| 5    | Controller | Mapea campos básicos     | Title, Description, Price  | Campos mapeados       |
| 6    | Controller | Mapea tipo y tamaño      | PropertyType, SquareFeet   | Campos mapeados       |
| 7    | Controller | Mapea habitaciones       | Bedrooms, Bathrooms        | Campos mapeados       |
| 8    | Controller | Mapea ubicación          | Address, City, State       | Campos mapeados       |
| 9    | Controller | Mapea características    | HasPool, HasBasement       | Campos mapeados       |
| 10   | Controller | Procesa imágenes         | Loop images                | PropertyImage creadas |
| 11   | Controller | Primera imagen = Primary | IsPrimary = sortOrder == 0 | Imagen principal      |
| 12   | Repository | Persiste propiedad       | INSERT property            | Guardado              |
| 13   | Logger     | Registra creación        | ILogger                    | Log creado            |
| 14   | API        | Retorna 201 Created      | CreatedAtAction            | Propiedad creada      |

#### Request Body Completo

```json
{
  "title": "Beautiful 3BR House in Miami",
  "description": "Spacious single-family home with pool...",
  "price": 450000,
  "currency": "USD",
  "mlsNumber": "MLS123456",
  "propertyType": 0,
  "propertySubType": 0,
  "bedrooms": 3,
  "bathrooms": 2.5,
  "halfBathrooms": 1,
  "squareFeet": 2400,
  "lotSize": 8500,
  "lotSizeUnit": "sqft",
  "yearBuilt": 2015,
  "stories": 2,
  "garageSpaces": 2,
  "hasPool": true,
  "hasBasement": false,
  "hasFireplace": true,
  "heatingType": 1,
  "coolingType": 1,
  "streetAddress": "123 Palm Beach Blvd",
  "city": "Miami",
  "state": "FL",
  "zipCode": "33101",
  "country": "USA",
  "neighborhood": "Coral Gables",
  "latitude": 25.7617,
  "longitude": -80.1918,
  "sellerId": "seller-uuid",
  "sellerName": "John Smith",
  "dealerId": "dealer-uuid",
  "categoryId": "category-uuid",
  "images": [
    "https://cdn.okla.com.do/properties/1/front.jpg",
    "https://cdn.okla.com.do/properties/1/living.jpg",
    "https://cdn.okla.com.do/properties/1/kitchen.jpg"
  ]
}
```

---

### PROCESO 3: Flujo de Estados de Propiedad

```
┌─────────┐   Publicar    ┌─────────────┐   Aprobar    ┌────────┐
│  Draft  ├──────────────►│PendingReview├─────────────►│ Active │
└─────────┘               └─────────────┘              └────┬───┘
                                │                          │
                                │ Rechazar                 │ Recibir oferta
                                ▼                          ▼
                          ┌──────────┐              ┌─────────────┐
                          │ Archived │              │UnderContract│
                          └──────────┘              └──────┬──────┘
                                                          │
                    ┌─────────────┬───────────────────────┼─────────────┐
                    │             │                       │             │
                    ▼             ▼                       ▼             ▼
              ┌─────────┐   ┌─────────┐            ┌──────────┐   ┌─────────┐
              │Withdrawn│   │ Expired │            │  Pending │   │  Sold   │
              └─────────┘   └─────────┘            └────┬─────┘   └────┬────┘
                                                        │              │
                                                        ▼              ▼
                                                   ┌────────┐    ┌────────┐
                                                   │ Closed │    │Archived│
                                                   └────────┘    └────────┘
```

#### Transiciones Válidas

| Estado Actual | Estados Permitidos                | Condición                           |
| ------------- | --------------------------------- | ----------------------------------- |
| Draft         | PendingReview, Archived           | Completar datos mínimos             |
| PendingReview | Active, Archived                  | Revisión de admin                   |
| Active        | UnderContract, Withdrawn, Expired | Oferta recibida, retiro, expiración |
| UnderContract | Pending, Active, Sold             | Contingencias, cierre, venta        |
| Pending       | Closed, Active                    | Cierre exitoso o fallido            |
| Sold          | Archived                          | Proceso completado                  |
| Closed        | Archived                          | Archivo final                       |

---

### PROCESO 4: Obtener Categorías con Jerarquía

#### Endpoint: `GET /api/categories/root` + `GET /api/categories/{id}/children`

| Paso | Actor    | Acción                  | Sistema                       | Resultado             |
| ---- | -------- | ----------------------- | ----------------------------- | --------------------- |
| 1    | Frontend | Obtiene categorías raíz | GET /categories/root          | Categorías nivel 0    |
| 2    | API      | Filtra ParentId = null  | WHERE ParentId IS NULL        | Solo raíz             |
| 3    | Frontend | Para cada categoría     | Loop                          | Expandir si necesario |
| 4    | Frontend | Obtiene subcategorías   | GET /categories/{id}/children | Hijos                 |
| 5    | API      | Filtra por ParentId     | WHERE ParentId = id           | Subcategorías         |
| 6    | Frontend | Construye árbol         | Recursivo                     | Árbol completo        |

#### Estructura de Categorías

```
├── Houses (casas)
│   ├── Single Family
│   ├── Townhouses
│   └── Multi-Family
├── Condos (condominios)
│   ├── High Rise
│   ├── Mid Rise
│   └── Garden Style
├── Land (terrenos)
│   ├── Residential Lots
│   ├── Commercial Lots
│   └── Farms
├── Commercial (comercial)
│   ├── Office
│   ├── Retail
│   └── Industrial
└── Other
```

---

## 🔔 Eventos de Dominio (RabbitMQ)

### Eventos Publicados

| Evento                       | Exchange            | Routing Key               | Payload                          |
| ---------------------------- | ------------------- | ------------------------- | -------------------------------- |
| `PropertyCreatedEvent`       | `properties.events` | `property.created`        | PropertyId, Title, Price         |
| `PropertyUpdatedEvent`       | `properties.events` | `property.updated`        | PropertyId, ChangedFields        |
| `PropertyStatusChangedEvent` | `properties.events` | `property.status_changed` | PropertyId, OldStatus, NewStatus |
| `PropertyDeletedEvent`       | `properties.events` | `property.deleted`        | PropertyId                       |
| `PropertyPriceChangedEvent`  | `properties.events` | `property.price_changed`  | PropertyId, OldPrice, NewPrice   |
| `PropertyViewedEvent`        | `properties.events` | `property.viewed`         | PropertyId, UserId               |
| `PropertySavedEvent`         | `properties.events` | `property.saved`          | PropertyId, UserId               |

### Eventos Consumidos

| Evento               | Origen       | Acción                       |
| -------------------- | ------------ | ---------------------------- |
| `MediaUploadedEvent` | MediaService | Asociar imagen a propiedad   |
| `UserDeletedEvent`   | UserService  | Anonimizar datos de vendedor |

---

## ⚠️ Reglas de Negocio

| #   | Regla              | Descripción                              |
| --- | ------------------ | ---------------------------------------- |
| 1   | MLS único          | Número MLS no puede repetirse            |
| 2   | Precio positivo    | Price > 0 obligatorio                    |
| 3   | Ubicación completa | Address, City, State, ZipCode requeridos |
| 4   | Categoría válida   | CategoryId debe existir                  |
| 5   | Imagen principal   | Primera imagen siempre IsPrimary = true  |
| 6   | Días en mercado    | DaysOnMarket calculado desde ListingDate |
| 7   | Precio por pie²    | PricePerSquareFoot = Price / SquareFeet  |
| 8   | Cambios de precio  | Incrementar PriceChanges en cada cambio  |

---

## ❌ Códigos de Error

| Código     | HTTP Status | Mensaje                   | Causa                   |
| ---------- | ----------- | ------------------------- | ----------------------- |
| `PROP_001` | 404         | Property not found        | Propiedad no existe     |
| `PROP_002` | 400         | Category not found        | Categoría inválida      |
| `PROP_003` | 400         | MLS already exists        | MLS duplicado           |
| `PROP_004` | 400         | Invalid price             | Precio <= 0             |
| `PROP_005` | 400         | Invalid status transition | Transición no permitida |
| `CAT_001`  | 404         | Category not found        | Categoría no existe     |

---

## ⚙️ Configuración

### appsettings.json

```json
{
  "PropertiesSaleSettings": {
    "DefaultCurrency": "USD",
    "DefaultCountry": "USA",
    "MaxImagesPerProperty": 50,
    "FeaturedLimit": 10,
    "ExpirationDays": 90,
    "AutoCalculatePricePerSqFt": true
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=propertiessale_db;..."
  }
}
```

---

## 📈 Métricas Prometheus

| Métrica                              | Tipo    | Labels                | Descripción              |
| ------------------------------------ | ------- | --------------------- | ------------------------ |
| `properties_sale_total`              | Gauge   | status, property_type | Total por estado y tipo  |
| `properties_sale_views`              | Counter | property_id           | Vistas de propiedades    |
| `properties_sale_inquiries`          | Counter | property_id           | Consultas recibidas      |
| `properties_sale_avg_days_on_market` | Gauge   | -                     | Promedio días en mercado |
| `properties_sale_avg_price`          | Gauge   | property_type         | Precio promedio por tipo |

---

## 📚 Referencias

- [PropertiesController](../../backend/PropertiesSaleService/PropertiesSaleService.Api/Controllers/PropertiesController.cs)
- [CategoriesController](../../backend/PropertiesSaleService/PropertiesSaleService.Api/Controllers/CategoriesController.cs)
- [Property Entity](../../backend/PropertiesSaleService/PropertiesSaleService.Domain/Entities/Property.cs)
- [Category Entity](../../backend/PropertiesSaleService/PropertiesSaleService.Domain/Entities/Category.cs)

---

**Última actualización:** Enero 21, 2026  
**Versión:** 1.0.0
