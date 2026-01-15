# 🔗 Arquitectura de Data Seeding - Diagrama Completo

**Creado:** Enero 15, 2026  
**Basado en:** Análisis del Frontend  
**Versión:** v2.0

---

## 📊 FLUJO GENERAL DE DATOS

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         USUARIO EN FRONTEND                              │
│                                                                          │
│  HomePage → SearchPage → VehicleDetail → DealerProfile → CheckoutPage   │
└──────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                         API GATEWAY (Ocelot)                             │
│                  http://localhost:18443/api/*                            │
└──────────────────────────────────────────────────────────────────────────┘
          │         │            │           │          │
          ▼         ▼            ▼           ▼          ▼
    ┌─────────┐ ┌────────┐ ┌─────────┐ ┌────────┐ ┌──────────┐
    │Vehicles │ │Dealers │ │Users    │ │Billing │ │Admin     │
    │Service  │ │Service │ │Service  │ │Service │ │Service   │
    └─────────┘ └────────┘ └─────────┘ └────────┘ └──────────┘
          │         │            │           │          │
          └─────────┴────────────┴───────────┴──────────┘
                            │
                            ▼
    ┌────────────────────────────────────────────────────────┐
    │              PostgreSQL Database                        │
    │  (vehicles, dealers, users, favorites, alerts, etc.)   │
    └────────────────────────────────────────────────────────┘
                            │
                            ▼
    ┌────────────────────────────────────────────────────────┐
    │         DatabaseSeedingService (v2.0)                  │
    │   Genera TODOS los datos necesarios en 7 fases         │
    └────────────────────────────────────────────────────────┘
```

---

## 🎯 ARQUITECTURA DE SEEDING V2.0

```
DatabaseSeedingService.SeedAllAsync()
│
├─→ FASE 0: CATÁLOGOS
│   ├─ CatalogBuilder.GenerateMakes()           → 10 Makes
│   ├─ CatalogBuilder.GenerateModels()          → 60+ Models
│   ├─ CatalogBuilder.GenerateYears()           → 15 Years
│   ├─ CatalogBuilder.GenerateBodyStyles()      → 7 Styles
│   ├─ CatalogBuilder.GenerateFuelTypes()       → 5 Types
│   └─ CatalogBuilder.GenerateColors()          → 20+ Colors
│
├─→ FASE 1: USUARIOS (42 total)
│   ├─ UserBuilder.GenerateBuyers(10)
│   ├─ UserBuilder.GenerateSellers(10)
│   ├─ UserBuilder.GenerateDealerUsers(30)
│   └─ UserBuilder.GenerateAdmins(2)
│
├─→ FASE 2: DEALERS (30 total)
│   ├─ DealerBuilder.GenerateIndependent(10)
│   │  └─ + 2-3 locations cada uno
│   ├─ DealerBuilder.GenerateChain(8)
│   ├─ DealerBuilder.GenerateMultipleStore(7)
│   └─ DealerBuilder.GenerateFranchise(5)
│
├─→ FASE 3: VEHÍCULOS (150 total)
│   ├─ VehicleBuilder.GenerateToyota(45)
│   ├─ VehicleBuilder.GenerateHonda(16)
│   ├─ VehicleBuilder.GenerateNissan(22)
│   ├─ VehicleBuilder.GenerateFord(22)
│   ├─ VehicleBuilder.GenerateBMW(15)
│   ├─ VehicleBuilder.GenerateMercedes(15)
│   ├─ VehicleBuilder.GenerateTesla(12)
│   ├─ VehicleBuilder.GenerateHyundai(15)
│   ├─ VehicleBuilder.GeneratePorsche(10)
│   └─ VehicleBuilder.GenerateChevrolet(8)
│       ├─ Cada vehículo con:
│       │  ├─ Specs completos (engine, horsepower, features)
│       │  ├─ Dealer asignado
│       │  ├─ Body style y otros atributos
│       │  └─ Status y metadata
│
├─→ FASE 4: HOMEPAGE SECTIONS (8 secciones)
│   ├─ HomepageSectionAssignmentService.CreateSections()
│   │  ├─ Carousel Principal (5 vehículos featured)
│   │  ├─ Sedanes (10 vehículos)
│   │  ├─ SUVs (10 vehículos)
│   │  ├─ Camionetas (10 vehículos)
│   │  ├─ Deportivos (10 vehículos)
│   │  ├─ Destacados (9 vehículos featured)
│   │  ├─ Lujo (10 vehículos BMW/Mercedes/Porsche)
│   │  └─ Eléctricos (10 vehículos Tesla)
│       └─ TOTAL: 90 vehículos asignados
│
├─→ FASE 5: IMÁGENES (1,500 URLs)
│   ├─ ImageBuilder.GenerateBatch()
│   │  └─ 10 imágenes por vehículo
│   │     ├─ 1 primaria (thumbnail)
│   │     └─ 9 secundarias (variados tipos)
│   └─ Picsum Photos URLs con vehicle seed
│       └─ Garantiza URLs únicas y consistentes
│
├─→ FASE 6: RELACIONES TRANSACCIONALES (500+ registros)
│   ├─ FavoriteBuilder.GenerateBatch()
│   │  └─ 50+ favorites distribuidos en 5 buyers
│   ├─ AlertBuilder.GenerateBatch()
│   │  └─ 15+ price alerts en 3 buyers
│   ├─ ComparisonBuilder.GenerateBatch()
│   │  └─ 5+ comparisons en 3 buyers
│   ├─ MessageBuilder.GenerateBatch()
│   │  ├─ 15+ conversations entre buyers y sellers
│   │  └─ 100+ messages distribuidos
│   ├─ ReviewBuilder.GenerateBatch()
│   │  └─ 150+ reviews para dealers (5-15 cada uno)
│   └─ ActivityLogBuilder.GenerateBatch()
│       └─ 100+ activity logs en 90 días
│
└─→ FASE 7: VALIDACIÓN
    ├─ CountVerification() → Verificar cantidades
    ├─ IntegrityCheck() → Verificar FK relationships
    ├─ DistributionCheck() → Verificar distribución
    └─ PrintSummary() → Mostrar estadísticas finales
```

---

## 🗄️ ESTRUCTURA DE BASE DE DATOS

```
PostgreSQL Database: cardealer
│
├─ CATÁLOGOS
│  ├─ catalog_makes (10 registros)
│  ├─ catalog_models (60+ registros)
│  ├─ catalog_years (15 registros)
│  ├─ catalog_body_styles (7 registros)
│  ├─ catalog_fuel_types (5 registros)
│  └─ catalog_colors (20+ registros)
│
├─ USUARIOS
│  └─ users (42 registros)
│     ├─ 10 buyers
│     ├─ 10 sellers
│     ├─ 30 dealers
│     └─ 2 admins
│
├─ DEALERS
│  ├─ dealers (30 registros)
│  │  ├─ Información básica
│  │  ├─ Plan de suscripción
│  │  ├─ Status y verificación
│  │  └─ Agregado: rating, reviews
│  │
│  └─ dealer_locations (60-90 registros)
│     └─ 2-3 locations por dealer
│
├─ VEHÍCULOS
│  ├─ vehicles (150 registros)
│  │  ├─ Specs completos
│  │  ├─ Make/Model/Year
│  │  ├─ Dealer FK
│  │  ├─ Features
│  │  ├─ Status (Active/Inactive/Sold)
│  │  └─ IsFeatured
│  │
│  ├─ vehicle_images (1,500 registros)
│  │  └─ 10 imágenes por vehículo
│  │
│  ├─ vehicle_features (1,200-2,250 registros)
│  │  └─ 8-15 features por vehículo
│  │
│  └─ vehicle_specifications
│     └─ Engine, horsepower, torque, etc.
│
├─ HOMEPAGE
│  ├─ homepage_section_configs (8 registros)
│  │  └─ Secciones configuradas
│  │
│  └─ vehicle_homepage_sections (90 registros)
│     └─ Mapping vehículos a secciones
│
├─ TRANSACCIONES
│  ├─ favorites (50+ registros)
│  │  └─ User → Vehicle mapping
│  │
│  ├─ price_alerts (15+ registros)
│  │  └─ User alert sobre vehículo
│  │
│  ├─ comparisons (5+ registros)
│  │  └─ Comparación entre vehículos
│  │
│  ├─ conversations (15+ registros)
│  │  └─ Entre buyers y sellers
│  │
│  ├─ messages (100+ registros)
│  │  └─ Dentro de conversations
│  │
│  ├─ dealer_reviews (150+ registros)
│  │  └─ Reviews para dealers
│  │
│  └─ activity_logs (100+ registros)
│     └─ Historial de acciones
│
└─ CONFIGURACIONES
   ├─ subscription_plans (3 registros)
   │  ├─ Starter: $49/mes, 15 listings
   │  ├─ Pro: $129/mes, 50 listings
   │  └─ Enterprise: $299/mes, unlimited
   │
   └─ system_configs
      └─ Early bird, maintenance mode, etc.
```

---

## 🚀 FLUJO DE EJECUCIÓN

### Paso 1: Inicialización

```csharp
var seeder = new DatabaseSeedingService(
    dbContext,
    logger,
    configuration
);

// Ejecutar seeding completo
await seeder.SeedAllAsync(cancellationToken);
```

### Paso 2: Fase 0 - Catálogos

```csharp
// Crear entries de catálogo
await dbContext.Makes.AddRangeAsync(
    CatalogBuilder.GenerateMakes()  // 10 makes
);
await dbContext.Models.AddRangeAsync(
    CatalogBuilder.GenerateModels() // 60+ models
);
// ... resto de catálogos
await dbContext.SaveChangesAsync();
```

### Paso 3: Fase 1 - Usuarios

```csharp
// Crear 42 usuarios
var users = new List<User>();
users.AddRange(UserBuilder.GenerateBuyers(10));
users.AddRange(UserBuilder.GenerateSellers(10));
users.AddRange(UserBuilder.GenerateDealerUsers(30));
users.AddRange(UserBuilder.GenerateAdmins(2));

await dbContext.Users.AddRangeAsync(users);
await dbContext.SaveChangesAsync();
```

### Paso 4: Fase 2 - Dealers

```csharp
// Crear 30 dealers con locations
var dealers = new List<Dealer>();

// 10 Independent
dealers.AddRange(DealerBuilder.GenerateBatch(10, "Independent")
    .Select(d => {
        d.Locations = LocationBuilder.GenerateBatch(2, d.Id);
        return d;
    }));

// ... Chain, MultipleStore, Franchise

await dbContext.Dealers.AddRangeAsync(dealers);
await dbContext.DealerLocations.AddRangeAsync(
    dealers.SelectMany(d => d.Locations)
);
await dbContext.SaveChangesAsync();
```

### Paso 5: Fase 3 - Vehículos

```csharp
// Crear 150 vehículos con specs completos
var vehicles = new List<Vehicle>();

// Por cada marca, generar cantidad específica
foreach (var (make, count) in MakeCounts) // Toyota: 45, Honda: 16, etc.
{
    var makeVehicles = VehicleBuilder.GenerateBatch(
        count: count,
        make: make,
        dealerIds: dealerIds,
        withCompleteSpecs: true,
        withFeatures: 8-15
    );
    vehicles.AddRange(makeVehicles);
}

await dbContext.Vehicles.AddRangeAsync(vehicles);
await dbContext.SaveChangesAsync();
```

### Paso 6: Fase 4 - Homepage Sections

```csharp
// Crear secciones y asignar vehículos
var sectionsService = new HomepageSectionAssignmentService(dbContext);

await sectionsService.CreateAndAssignSections(new()
{
    ("Carousel Principal", 5, v => v.IsFeatured),
    ("Sedanes", 10, v => v.BodyStyle == "Sedan"),
    ("SUVs", 10, v => v.BodyStyle == "SUV"),
    // ... rest
});
```

### Paso 7: Fase 5 - Imágenes

```csharp
// Crear 1,500 imágenes
var images = new List<VehicleImage>();

foreach (var vehicle in vehicles)
{
    images.AddRange(ImageBuilder.GenerateBatchForVehicle(
        vehicle.Id,
        count: 10,  // 10 imágenes por vehículo
        usePicsumPhotosSeeded: vehicle.Id
    ));
}

await dbContext.VehicleImages.AddRangeAsync(images);
await dbContext.SaveChangesAsync();
```

### Paso 8: Fase 6 - Relaciones

```csharp
// Crear favorites, alerts, messages, etc.

// Favorites: 5 buyers × 10+ cada uno = 50+
var favorites = FavoriteBuilder.GenerateBatch(
    buyerIds: buyerIds.Take(5),
    vehicleIds: vehicleIds,
    countPerBuyer: faker.Random.Int(5, 15)
);

// Price Alerts: 3 buyers × 5+ cada uno = 15+
var alerts = AlertBuilder.GenerateBatch(
    buyerIds: buyerIds.Take(3),
    vehicleIds: vehicleIds,
    countPerBuyer: faker.Random.Int(5, 10)
);

// Messages: 15+ conversations
var conversations = MessageBuilder.GenerateConversations(
    buyerIds: buyerIds,
    sellerIds: sellerIds,
    vehicleIds: vehicleIds,
    conversationCount: 15
);

await dbContext.SaveChangesAsync();
```

### Paso 9: Fase 7 - Validación

```csharp
// Verificar que todo se generó correctamente
await seeder.ValidateAsync();

// Output:
// ✅ 10 Makes
// ✅ 60+ Models
// ✅ 150 Vehicles
// ✅ 1,500 Images
// ✅ 42 Users
// ✅ 30 Dealers
// ✅ 50+ Favorites
// ... etc
```

---

## 📊 DISTRIBUCIÓN VISUALIZADA

### Vehículos por Marca

```
Toyota:        ████████████████████████████████████████████ 45 (30%)
Hyundai:       ███████████████ 15 (10%)
Nissan:        ██████████████████████ 22 (15%)
Ford:          ██████████████████████ 22 (15%)
BMW:           ███████████████ 15 (10%)
Mercedes:      ███████████████ 15 (10%)
Tesla:         ████████████ 12 (8%)
Honda:         ████████████████ 16 (11%)
Porsche:       ██████████ 10 (7%)
Chevrolet:     ████████ 8 (5%)
```

### Usuarios por Tipo

```
Dealers:       ████████████████████████████████████████████ 30 (71%)
Buyers:        ███████████ 10 (24%)
Sellers:       ███████████ 10 (24%)
Admins:        ██ 2 (5%)
```

### Vehículos en Homepage

```
Total:         ██████████████████████████████████████████████ 90 (60%)
No asignados:  ████████████████████████ 60 (40%)
```

### Relaciones Transaccionales

```
Messages:      ███████████████████████████████████████████ 100+ (40%)
Reviews:       ████████████████████████████████ 150+ (60%)
Favorites:     ██████████████ 50+ (20%)
Alerts:        █████ 15+ (6%)
Comparisons:   ███ 5+ (2%)
Activity Logs: ████████████████ 100+ (40%)
```

---

## 🔍 VALIDACIÓN POR VISTA

### HomePage ✅

```
GET /api/homepagesections/homepage
├─ 8 sections retornadas
├─ 90 vehículos asignados
├─ Cada sección con maxItems respetado
├─ Imágenes primarias presentes
└─ Featured vehicles marcados
```

### SearchPage ✅

```
GET /api/catalog/makes
├─ 10 makes retornados

GET /api/catalog/models/{makeId}
├─ 5-7 models por make

GET /api/vehicles?make=Toyota
├─ 45 vehículos retornados

GET /api/vehicles?bodyStyle=Sedan
├─ 40+ sedanes retornados
```

### FavoritesPage ✅

```
GET /api/favorites
├─ Usuario buyer1 tiene 10-15 favorites
├─ Cada favorite tiene vehicle data
└─ Notas y timestamps presentes
```

### AdminDashboard ✅

```
GET /api/admin/dashboard/stats
├─ totalUsers: 42
├─ activeListings: 150
├─ pendingApprovals: 5-10
├─ revenue: calculado
└─ activityLogs: 100+
```

---

## 🎯 RESUMEN DE ARQUITECTURA

```
┌──────────────────────────────────────────────────────────┐
│                   FRONTEND VIEWS (27)                    │
│  ────────────────────────────────────────────────────────│
│ HomePage │ Search │ Detail │ Dealer │ Favorites │ Admin │
└──────────────────────────────────────────────────────────┘
                            │
                            ▼
┌──────────────────────────────────────────────────────────┐
│              MICROSERVICES (10 servicios)                 │
│  ────────────────────────────────────────────────────────│
│ Vehicles │ Dealers │ Users │ Auth │ Media │ Billing │ ...│
└──────────────────────────────────────────────────────────┘
                            │
                            ▼
┌──────────────────────────────────────────────────────────┐
│           POSTGRESQL DATABASE (15 tablas)                │
│  ────────────────────────────────────────────────────────│
│ vehicles │ dealers │ users │ favorites │ alerts │ logs │
└──────────────────────────────────────────────────────────┘
                            │
                            ▼
┌──────────────────────────────────────────────────────────┐
│        SEEDING SERVICE (7 FASES, 500+ REGISTROS)         │
│  ────────────────────────────────────────────────────────│
│ Catalogs│ Users │ Dealers │ Vehicles │ Sections│ Images│
└──────────────────────────────────────────────────────────┘
```

---

**Arquitectura v2.0: 100% específica para el frontend**
