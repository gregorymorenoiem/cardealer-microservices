# 🚀 PLAN DE SEEDING ACTUALIZADO v2.0

**Fecha:** Enero 15, 2026  
**Basado en:** Análisis completo del frontend y sus datos reales  
**Objetivo:** Generar exactamente los datos necesarios para probar todas las vistas

---

## 📋 RESUMEN EJECUTIVO

El análisis del frontend reveló que necesitamos datos **MÁS ESPECÍFICOS Y DISTRIBUIDOS** que lo que teníamos en v1.0:

### Comparación v1.0 vs v2.0

| Aspecto            | v1.0        | v2.0                    | Cambio                              |
| ------------------ | ----------- | ----------------------- | ----------------------------------- |
| Vehículos          | 150 básicos | 150 especificados       | 🔄 Specs completos                  |
| Dealers            | 30 simples  | 30 con locations        | ✅ +Locations y ratings             |
| Usuarios           | 20          | 40                      | ✅ +Admin users                     |
| Imágenes           | 7,500 URLs  | 750 de verdad + info    | 🔄 Mejor distribución               |
| Secciones Homepage | 8           | 8 + vehículos asignados | ✅ +Mappings                        |
| Relaciones         | 0           | 100+                    | ✅ NEW: Favorites, Alerts, Messages |
| Catálogos          | Stub        | Completo                | ✅ NEW: Makes, Models, Years        |

---

## 🎯 FASE 0: CATÁLOGOS (NUEVAS)

### Make/Model Catalog

```csharp
var makesAndModels = new Dictionary<string, List<string>>
{
    ["Toyota"] = new()
    {
        "Corolla", "Camry", "RAV4", "Highlander", "4Runner", "Yaris", "Prius"
    },
    ["Honda"] = new()
    {
        "Civic", "Accord", "CR-V", "Pilot", "Fit", "HR-V"
    },
    ["Nissan"] = new()
    {
        "Altima", "Maxima", "Pathfinder", "Rogue", "GT-R", "Murano"
    },
    ["BMW"] = new()
    {
        "3 Series", "5 Series", "X5", "X3", "M440i", "M850i", "Z4"
    },
    ["Mercedes-Benz"] = new()
    {
        "C-Class", "E-Class", "GLA", "GLC", "S-Class", "AMG G63"
    },
    ["Porsche"] = new()
    {
        "911", "Cayenne", "Panamera", "Tacan", "Boxster"
    },
    ["Tesla"] = new()
    {
        "Model 3", "Model Y", "Model S", "Model X"
    },
    ["Hyundai"] = new()
    {
        "Elantra", "Sonata", "Tucson", "Santa Fe", "Kona"
    },
    ["Ford"] = new()
    {
        "Mustang", "F-150", "Edge", "Explorer", "Ranger"
    },
    ["Chevrolet"] = new()
    {
        "Silverado", "Tahoe", "Camaro", "Equinox", "Malibu"
    }
};
```

**Datos a Generar:**

- [ ] 10 Makes
- [ ] ~60 Models totales
- [ ] 15 Years (2010-2025)
- [ ] 7 Body Styles
- [ ] 5 Fuel Types
- [ ] 3 Transmissions
- [ ] 20+ Colors

---

## 🎯 FASE 1: USUARIOS (MEJORADO)

### Buyers (10 usuarios)

```json
{
  "accountType": "Individual",
  "role": "Buyer",
  "emails": ["buyer1@okla.local", "buyer2@okla.local", ..., "buyer10@okla.local"],
  "password": "Test@123",
  "isVerified": true,
  "isActive": true
}
```

### Sellers (10 usuarios)

```json
{
  "accountType": "Seller",
  "role": "Seller",
  "emails": ["seller1@okla.local", ..., "seller10@okla.local"],
  "password": "Test@123",
  "isVerified": true,
  "isActive": true
}
```

### Dealers (30 usuarios - dealers reales)

```json
{
  "accountType": "Dealer",
  "role": "Dealer",
  "dealerType": "Mixed",
  "dealerTypes": {
    "Independent": 10,
    "Chain": 8,
    "MultipleStore": 7,
    "Franchise": 5
  },
  "password": "Test@123",
  "isVerified": true
}
```

### Admins (2 usuarios)

```json
{
  "accountType": "Admin",
  "role": "Admin",
  "emails": ["admin1@okla.local", "admin2@okla.local"],
  "password": "Test@123",
  "isVerified": true
}
```

**Total: 40 + 2 = 42 usuarios**

---

## 🎯 FASE 2: DEALERS (MEJORADO)

### Distribución de Dealers

```
10 Independent
  ├─ 7 Verified (70%)
  └─ 3 Pending (30%)

8 Chain
  └─ 6 Verified, 2 Pending

7 MultipleStore
  └─ 5 Verified, 2 Pending

5 Franchise
  └─ 3 Verified, 2 Pending
```

### Estructura de Dealer

```csharp
var dealer = new Dealer
{
    // Basic
    BusinessName = faker.Company.CompanyName(),
    RNC = GenerateRNC(),  // 9-11 dígitos único
    LegalName = faker.Company.CompanyName(),
    DealerType = "Independent|Chain|MultipleStore|Franchise",

    // Contact
    Email = $"dealer{n}@okla.local",
    Phone = faker.Phone.PhoneNumber("809-####-####"),
    Website = faker.Internet.Url(),

    // Location
    Address = faker.Address.StreetAddress(),
    City = faker.PickRandom(["Santo Domingo", "Santiago", "La Romana", "Puerto Plata", "San Cristóbal"]),
    Province = "Province",

    // Business
    EstablishedDate = faker.Date.PastDateOnly(30),
    EmployeeCount = faker.Random.Int(5, 50),
    Description = faker.Lorem.Paragraph(3),

    // Status
    Status = "Active",
    CurrentPlan = "Pro",  // Starter, Pro, Enterprise
    VerificationStatus = faker.PickRandom(["Verified", "NotVerified"]),
    MaxActiveListings = 50,
    IsSubscriptionActive = true,

    // Locations (2-3 per dealer)
    Locations = GenerateDealerLocations(2-3)
};
```

---

## 🎯 FASE 3: VEHÍCULOS (ESPECIFICADO)

### Distribución por Marca (150 total)

```
Toyota:        45 vehículos
├─ Corolla:    15 (Sedanes, Used, 2015-2023)
├─ Camry:      12 (Sedanes, Used, 2018-2024)
├─ RAV4:       10 (SUVs, Used, 2019-2024)
├─ Highlander: 5 (SUVs, New, 2024-2025)
├─ 4Runner:    3 (Trucks, Used)

Honda:         16 vehículos
├─ Civic:      8 (Sedanes, Used, 2016-2023)
├─ Accord:     5 (Sedanes, Used, 2019-2024)
├─ CR-V:       3 (SUVs)

Nissan:        22 vehículos
├─ Altima:     10 (Sedanes)
├─ Rogue:      7 (SUVs)
├─ GT-R:       5 (Deportivos)

Ford:          22 vehículos
├─ F-150:      15 (Trucks, Used)
├─ Edge:       7 (SUVs)

BMW:           15 vehículos
├─ 3 Series:   8 (Sedanes, Luxury)
├─ X5:         5 (SUVs, Luxury)
├─ M440i:      2 (Coupe, Luxury)

Mercedes:      15 vehículos
├─ C-Class:    8 (Sedanes, Luxury)
├─ GLC:        5 (SUVs, Luxury)
├─ AMG G63:    2 (SUV, Ultra-Luxury)

Tesla:         12 vehículos
├─ Model 3:    5 (Sedan, Electric)
├─ Model Y:    5 (SUV, Electric)
├─ Model S:    2 (Sedan, Electric, Luxury)

Hyundai:       15 vehículos
├─ Elantra:    8 (Sedanes, Budget)
├─ Tucson:     7 (SUVs, Budget)

Porsche:       10 vehículos
├─ 911:        5 (Deportivos, Ultra-Luxury)
├─ Cayenne:    5 (SUV, Luxury)

Chevrolet:     8 vehículos
└─ Silverado:  8 (Trucks, Used)
```

### Specs Completos por Vehículo

```csharp
var vehicle = new Vehicle
{
    // Identification
    Id = Guid.NewGuid(),
    Title = $"{year} {make} {model}",

    // Make/Model/Year
    Make = faker.PickRandom(makes),
    Model = GetModelForMake(make),
    Year = faker.Random.Int(2010, 2025),

    // Condition (60% Used, 30% New, 10% Certified)
    Condition = faker.Random.Double() switch
    {
        < 0.6 => "Used",
        < 0.9 => "New",
        _ => "Certified"
    },

    // Pricing
    Price = CalculatePrice(year, condition, make, model),
    OriginalPrice = condition == "New" ? Price * 1.05 : null,

    // Mileage
    Mileage = condition switch
    {
        "New" => faker.Random.Int(0, 100),
        "Used" => faker.Random.Int(10000, 300000),
        "Certified" => faker.Random.Int(30000, 150000),
        _ => 0
    },

    // Physical Properties
    BodyStyle = GetBodyStyleForModel(model),  // Sedan, SUV, Truck, Coupe, etc.
    ExteriorColor = faker.PickRandom(colors),
    InteriorColor = faker.PickRandom(["Black", "Beige", "Gray", "Brown"]),

    // Engine/Performance
    FuelType = faker.PickRandom(["Gasoline", "Diesel", "Hybrid", "Electric"]),
    Transmission = faker.PickRandom(["Manual", "Automatic", "CVT"]),
    Engine = GetEngineForModel(model),  // "2.0L V4", "3.5L V6", etc.
    Horsepower = GenerateHorsepowerForModel(model),
    Torque = GenerateTorqueForModel(model),

    // Features (8-15 características)
    Features = GenerateFeatures(make, model, year, 8, 15),

    // Description
    Description = faker.Lorem.Paragraph(2),

    // Dealer
    DealerId = faker.PickRandom(dealerIds),

    // Status
    Status = "Active",
    IsFeatured = faker.Random.Bool(0.3),  // 30% destacados

    // Images (1-10)
    Images = GenerateImages(10)
};
```

### Generador de Features

```csharp
var possibleFeatures = new[]
{
    // Exterior
    "Alloy Wheels", "Sunroof", "Roof Rack", "Fog Lights", "LED Headlights",
    "Panoramic Sunroof", "Spoiler", "Tinted Windows",

    // Interior
    "Leather Seats", "Memory Seats", "Heated Seats", "Ventilated Seats",
    "Power Steering", "Power Windows", "Power Locks", "Power Mirrors",
    "Navigation System", "Backup Camera", "Bluetooth", "USB Ports",
    "Wireless Charging", "Premium Audio System", "Ambient Lighting",

    // Safety
    "ABS Brakes", "Stability Control", "Traction Control", "Lane Departure Warning",
    "Blind Spot Monitoring", "Forward Collision Warning", "Automatic Emergency Braking",
    "Multiple Airbags", "Electronic Stability Program",

    // Convenience
    "Climate Control", "Dual Zone Climate", "Auto Headlights", "Rain Sensing Wipers",
    "Adaptive Cruise Control", "Cruise Control", "Automatic Transmission",
    "Keyless Entry", "Push Start",

    // Performance
    "Turbo Engine", "Sport Suspension", "Performance Brakes", "All-Wheel Drive",
    "Four-Wheel Drive", "Traction Control"
};

// Para cada vehículo, seleccionar 8-15 features aleatorios
var vehicleFeatures = faker.Random.Shuffle(possibleFeatures).Take(faker.Random.Int(8, 15)).ToList();
```

---

## 🎯 FASE 4: HOMEPAGE SECTIONS (NUEVO)

### Secciones a Configurar

```csharp
var sections = new[]
{
    new HomepageSectionConfig
    {
        Name = "Carousel Principal",
        Slug = "carousel",
        DisplayOrder = 1,
        MaxItems = 5,
        IsActive = true,
        Subtitle = "Los vehículos más buscados ahora",
        AccentColor = "blue",
        LayoutType = "carousel",
        Vehicles = SelectAndAssignVehicles(
            criteria: vehicle => vehicle.IsFeatured && vehicle.Status == "Active",
            count: 5,
            sortBy: "popularity"
        )
    },
    new HomepageSectionConfig
    {
        Name = "Sedanes",
        Slug = "sedanes",
        DisplayOrder = 2,
        MaxItems = 10,
        IsActive = true,
        Vehicles = SelectAndAssignVehicles(
            criteria: v => v.BodyStyle == "Sedan",
            count: 10
        )
    },
    new HomepageSectionConfig
    {
        Name = "SUVs",
        Slug = "suvs",
        DisplayOrder = 3,
        MaxItems = 10,
        IsActive = true,
        Vehicles = SelectAndAssignVehicles(
            criteria: v => v.BodyStyle == "SUV",
            count: 10
        )
    },
    // ... 5 secciones más (Camionetas, Deportivos, Destacados, Lujo, Eléctricos)
};
```

### Vehículos por Sección

| Sección    | Count  | Criteria                                                       |
| ---------- | ------ | -------------------------------------------------------------- |
| Carousel   | 5      | `IsFeatured = true`                                            |
| Sedanes    | 10     | `BodyStyle = "Sedan"`                                          |
| SUVs       | 10     | `BodyStyle = "SUV"`                                            |
| Camionetas | 10     | `BodyStyle = "Truck"`                                          |
| Deportivos | 10     | `BodyStyle = "Coupe" OR FuelType = "Gasoline" AND Price > 50M` |
| Destacados | 9      | `IsFeatured = true AND Status = "Active"`                      |
| Lujo       | 10     | `Make IN (BMW, Mercedes, Porsche) AND Price > 80M`             |
| Eléctricos | 10     | `FuelType = "Electric"`                                        |
| **TOTAL**  | **90** | Distribuidos en 8 secciones                                    |

---

## 🎯 FASE 5: IMÁGENES (MEJORADO)

### URL Generation Strategy

```csharp
// Usar Picsum Photos con vehicle ID como seed
// Garantiza URLs consistentes y diferentes para cada vehículo

var imageTypes = new[]
{
    ("Exterior Front", 0),
    ("Exterior Back", 1),
    ("Exterior Left", 2),
    ("Exterior Right", 3),
    ("Exterior Detail", 4),
    ("Interior Cabin", 10),
    ("Interior Dashboard", 11),
    ("Interior Seats", 12),
    ("Engine Bay", 20),
    ("Engine Detail", 21)
};

for (int i = 0; i < 10; i++)
{
    var imageUrl = $"https://picsum.photos/800/600?random={vehicleId.GetHashCode() + i}";
    var image = new VehicleImage
    {
        Id = Guid.NewGuid(),
        VehicleId = vehicleId,
        Url = imageUrl,
        ThumbnailUrl = $"https://picsum.photos/400/300?random={vehicleId.GetHashCode() + i}",
        Caption = imageTypes[i].Item1,
        ImageType = imageTypes[i].Item1,
        IsPrimary = i == 0,  // Primera imagen es la principal
        SortOrder = i,
        MimeType = "image/jpeg",
        CreatedAt = DateTime.UtcNow
    };
}
```

### Distribución de Imágenes

```
Total Imágenes: 1,500 (10 por vehículo × 150 vehículos)

Por tipo:
├─ Exterior Front:    150 (10%)
├─ Exterior Back:     150 (10%)
├─ Exterior Sides:    300 (20%)
├─ Interior:          300 (20%)
├─ Engine:            300 (20%)
├─ Details:           300 (20%)

Primarias: 150 (1 por vehículo)
Secundarias: 1,350 (9 por vehículo)
```

---

## 🎯 FASE 6: RELACIONES Y DATOS TRANSACCIONALES (NUEVO)

### Favorites

```csharp
// 5 buyers, cada uno con 5-15 favoritos

foreach (var buyerId in buyerIds)
{
    var favoriteCount = faker.Random.Int(5, 15);
    var randomVehicles = faker.Random.Shuffle(vehicleIds).Take(favoriteCount);

    foreach (var vehicleId in randomVehicles)
    {
        new Favorite
        {
            Id = Guid.NewGuid(),
            UserId = buyerId,
            VehicleId = vehicleId,
            Note = faker.Lorem.Sentence(),
            SavedAt = faker.Date.PastDateOnly(90),
            IsNotificationEnabled = faker.Random.Bool(0.6)
        };
    }
}
```

**Total: 50+ favorites**

### Price Alerts

```csharp
// 3+ buyers con alertas activas

foreach (var buyerId in buyers.Take(3).Select(x => x.Id))
{
    var alertCount = faker.Random.Int(5, 10);
    var randomVehicles = faker.Random.Shuffle(vehicleIds).Take(alertCount);

    foreach (var vehicleId in randomVehicles)
    {
        var vehicle = vehicleDict[vehicleId];
        new PriceAlert
        {
            Id = Guid.NewGuid(),
            UserId = buyerId,
            VehicleId = vehicleId,
            TargetPrice = vehicle.Price * faker.Random.Double(0.8, 0.95),
            IsActive = true,
            CreatedAt = faker.Date.PastDateOnly(60)
        };
    }
}
```

**Total: 15+ alerts**

### Comparisons

```csharp
// 3+ buyers con comparaciones

foreach (var buyerId in buyers.Take(3).Select(x => x.Id))
{
    var comparison = new Comparison
    {
        Id = Guid.NewGuid(),
        UserId = buyerId,
        Name = faker.Lorem.Sentence(3),
        VehicleIds = faker.Random.Shuffle(vehicleIds).Take(faker.Random.Int(2, 3)).ToList(),
        CreatedAt = faker.Date.PastDateOnly(30),
        IsShared = faker.Random.Bool(0.3)
    };
}
```

**Total: 5+ comparisons**

### Messages/Conversations

```csharp
// Buyers y Sellers intercambiando mensajes

var participantPairs = GenerateRandomPairs(buyerIds, sellerIds, count: 15);

foreach (var (buyerId, sellerId) in participantPairs)
{
    var conversation = new Conversation
    {
        Id = Guid.NewGuid(),
        InitiatorId = buyerId,
        ParticipantId = sellerId,
        VehicleId = faker.Random.Element(vehicleIds),  // About a vehicle
        CreatedAt = faker.Date.PastDateOnly(30)
    };

    // 5-20 mensajes por conversación
    for (int i = 0; i < faker.Random.Int(5, 20); i++)
    {
        new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation.Id,
            SenderId = i % 2 == 0 ? buyerId : sellerId,  // Alternate
            Content = faker.Lorem.Sentences(1),
            SentAt = faker.Date.Between(conversation.CreatedAt, DateTime.UtcNow),
            IsRead = faker.Random.Bool(0.8)
        };
    }
}
```

**Total: 100+ messages, 15+ conversations**

### Activity Logs

```csharp
// Registrar actividades para admin dashboard

var activities = new[]
{
    "user_created", "user_verified", "listing_created", "listing_published",
    "listing_sold", "payment_processed", "dealer_verified", "review_posted",
    "favorite_added", "message_sent", "inquiry_created", "document_uploaded"
};

foreach (var activity in GenerateMixedActivities(count: 100))
{
    new ActivityLog
    {
        Id = Guid.NewGuid(),
        UserId = faker.Random.Element(allUserIds),
        Action = faker.PickRandom(activities),
        EntityType = "Vehicle|Dealer|User|Payment",
        EntityId = faker.Random.Element(referencIds),
        Timestamp = faker.Date.PastDateOnly(90),
        Details = faker.Lorem.Sentence()
    };
}
```

**Total: 100+ logs**

---

## 🎯 FASE 7: DEALERS RATINGS Y REVIEWS (NUEVO)

```csharp
// Cada dealer con 5+ reviews y rating

foreach (var dealer in dealers)
{
    var reviewCount = faker.Random.Int(5, 15);
    var ratings = new List<int>();

    for (int i = 0; i < reviewCount; i++)
    {
        var rating = faker.Random.Int(3, 5);  // 3-5 stars
        ratings.Add(rating);

        new DealerReview
        {
            Id = Guid.NewGuid(),
            DealerId = dealer.Id,
            ReviewerId = faker.Random.Element(buyerIds),
            Rating = rating,
            Title = faker.Lorem.Sentence(3),
            Content = faker.Lorem.Paragraphs(2),
            CreatedAt = faker.Date.PastDateOnly(180)
        };
    }

    // Update dealer average rating
    dealer.AverageRating = ratings.Average();
    dealer.TotalReviews = reviewCount;
}
```

**Total: 5-15 reviews × 30 dealers = 150+ reviews**

---

## 📊 RESUMEN DE DATOS v2.0

### Totales por Categoría

```
📊 USUARIOS
├─ Buyers: 10
├─ Sellers: 10
├─ Dealers: 30
├─ Admins: 2
└─ TOTAL: 42

🚗 VEHÍCULOS
├─ Total: 150
├─ Specs completos: 150 (100%)
├─ Con imágenes: 150 (1-10 cada uno)
├─ Distribuidos en 10 marcas: ✅
└─ En secciones: 90 (60%)

🏪 DEALERS
├─ Total: 30
├─ Verified: 21 (70%)
├─ Pending: 9 (30%)
├─ Con locations: 30 (2-3 cada uno)
├─ Con reviews: 30 (5-15 cada uno)
└─ Con ratings: 30

📸 IMÁGENES
├─ Total URLs: 1,500
├─ Por vehículo: 10
├─ Primarias: 150
├─ Secundarias: 1,350

📋 CATÁLOGOS
├─ Makes: 10
├─ Models: 60+
├─ Years: 15 (2010-2025)
├─ Body Styles: 7
├─ Fuel Types: 5
├─ Transmissions: 3
└─ Colors: 20+

🏠 HOMEPAGE SECTIONS
├─ Total: 8
├─ Vehículos asignados: 90
└─ Maxitems respetados: ✅

💙 FAVORITES
├─ Total: 50+
├─ Usuarios: 5
└─ Por usuario: 10+

🔔 PRICE ALERTS
├─ Total: 15+
├─ Usuarios: 3
└─ Por usuario: 5+

📊 COMPARISONS
├─ Total: 5+
├─ Usuarios: 3
└─ Por comparación: 2-3 vehículos

💬 MESSAGES
├─ Conversaciones: 15+
├─ Participantes: Buyers + Sellers
└─ Mensajes: 100+

📝 ACTIVITY LOGS
├─ Total: 100+
├─ Tipos: 12 tipos diferentes
└─ Período: 90 días

⭐ REVIEWS
├─ Total: 150+
├─ Por dealer: 5-15
├─ Rating promedio: 3-5 stars
```

---

## ✅ IMPLEMENTACIÓN

### Recomendación: Actualizar DatabaseSeedingService

El `DatabaseSeedingService.cs` actual debe expandirse para incluir:

1. **Fase de Catálogos** (nueva)

   - CarBuilder para Makes/Models
   - YearBuilder
   - FeatureBuilder

2. **Mejorar VehicleBuilder**

   - Agregar todos los specs (engine, horsepower, features completos)
   - Implementar lógica de distribución por marca
   - Asignar a secciones de homepage

3. **Mejorar DealerBuilder**

   - Agregar locations generator
   - Generar reviews y ratings

4. **Nueva: RelationshipBuilder**

   - FavoritesBuilder
   - AlertsBuilder
   - MessageBuilder
   - ActivityLogBuilder

5. **Nueva: HomepageSectionAssignmentService**
   - Asignar vehículos a secciones correctamente

---

## 🚀 PRÓXIMO PASO

**Actualizar:** `backend/_Shared/CarDealer.DataSeeding/DatabaseSeedingService.cs`

Con la nueva estructura de 7 fases en lugar de 4.
