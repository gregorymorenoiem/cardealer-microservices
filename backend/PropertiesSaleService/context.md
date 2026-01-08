# PropertiesSaleService - Context Documentation

## 📋 INFORMACIÓN GENERAL

- **Nombre del Servicio:** PropertiesSaleService
- **Puerto en Desarrollo:** 5021
- **Estado:** 🚧 **EN DESARROLLO - NO DESPLEGADO**
- **Base de Datos:** PostgreSQL (`propertiessaleservice`)
- **Imagen Docker:** Local only

### Propósito
Servicio de venta de propiedades inmobiliarias (casas, apartamentos, terrenos, locales comerciales). Similar a VehiclesSaleService pero para bienes raíces.

---

## 🏗️ ARQUITECTURA

```
PropertiesSaleService/
├── PropertiesSaleService.Api/
│   ├── Controllers/
│   │   ├── PropertiesController.cs
│   │   ├── PropertyTypesController.cs
│   │   └── PropertyFeaturesController.cs
│   └── Program.cs
├── PropertiesSaleService.Application/
├── PropertiesSaleService.Domain/
│   ├── Entities/
│   │   ├── Property.cs
│   │   ├── PropertyImage.cs
│   │   ├── PropertyFeature.cs
│   │   └── PropertyTour.cs
│   └── Enums/
│       ├── PropertyType.cs
│       └── PropertyStatus.cs
└── PropertiesSaleService.Infrastructure/
```

---

## 📦 ENTIDADES PRINCIPALES

### Property
```csharp
public class Property
{
    public Guid Id { get; set; }
    
    // Básico
    public string Title { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public PropertyStatus Status { get; set; }
    
    // Tipo
    public PropertyType Type { get; set; }          // House, Apartment, Land, Commercial
    public string? Subtype { get; set; }            // Villa, Penthouse, etc.
    
    // Ubicación
    public string Street { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string ZipCode { get; set; }
    public string Country { get; set; } = "DO";
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    
    // Características
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public decimal SquareMeters { get; set; }
    public decimal? LotSizeSquareMeters { get; set; }
    public int? YearBuilt { get; set; }
    public int? Floors { get; set; }
    public int? ParkingSpaces { get; set; }
    
    // Amenidades (JSON)
    public string FeaturesJson { get; set; } = "[]";
    // ["Pool", "Gym", "Security 24/7", "Generator", "Solar Panels"]
    
    // Vendedor
    public Guid SellerId { get; set; }
    public string SellerName { get; set; }
    public string? SellerPhone { get; set; }
    public bool IsAgentListing { get; set; }
    
    // Metadata
    public int ViewCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    
    // Navegación
    public ICollection<PropertyImage> Images { get; set; }
}
```

### PropertyType Enum
```csharp
public enum PropertyType
{
    House = 0,          // Casa
    Apartment = 1,      // Apartamento
    Condo = 2,          // Condominio
    Townhouse = 3,      // Casa adosada
    Land = 4,           // Terreno
    Commercial = 5,     // Local comercial
    Office = 6,         // Oficina
    Warehouse = 7,      // Almacén
    Farm = 8,           // Finca
    Building = 9        // Edificio completo
}
```

---

## 📡 ENDPOINTS (Propuestos)

- `GET /api/properties` - Listar propiedades (con filtros)
- `GET /api/properties/{id}` - Detalle de propiedad
- `POST /api/properties` - Publicar propiedad
- `PUT /api/properties/{id}` - Actualizar propiedad
- `DELETE /api/properties/{id}` - Eliminar propiedad
- `GET /api/properties/search` - Búsqueda avanzada
- `GET /api/properties/nearby` - Propiedades cercanas a ubicación
- `POST /api/properties/{id}/tour-request` - Solicitar tour virtual/físico

---

## 🔍 FILTROS DE BÚSQUEDA

### Ubicación
- Ciudad, estado, código postal
- Radio de búsqueda (propiedades a X km)
- Barrios/sectores

### Precio
- Rango mínimo-máximo
- Precio por m²

### Características
- Número de habitaciones (mín-máx)
- Número de baños (mín-máx)
- Tamaño en m² (mín-máx)

### Tipo
- Tipo de propiedad (casa, apartamento, etc.)
- Condición (nuevo, usado, en construcción)

### Amenidades
- Piscina, gym, parqueos, seguridad, etc.

---

## 💡 FUNCIONALIDADES PLANEADAS

### Tours Virtuales
Integración con Matterport para tours 3D.

### Mapas Interactivos
Visualización de propiedades en mapa (Google Maps).

### Calculadora de Hipoteca
Calcular cuotas mensuales según precio y tasa de interés.

### Comparador
Comparar hasta 4 propiedades lado a lado.

### Alertas
Notificar cuando hay propiedad nueva que coincide con criterios del usuario.

---

**Estado:** 🚧 EN DESARROLLO - No desplegado en producción  
**Versión:** 0.1.0
