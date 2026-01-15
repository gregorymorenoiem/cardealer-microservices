# 🔌 Mapeo Completo: Endpoints → Test Data

**Fecha:** Enero 15, 2026  
**Propósito:** Identificar exactamente qué datos debe generar el seeding script

---

## 📡 MICROSERVICIOS Y SUS ENDPOINTS

### 1️⃣ VehiclesSaleService (`/api/vehicles`)

#### GET /api/vehicles/search

```
Parámetros:
  - query: string (opcional)
  - make: string (opcional)
  - model: string (opcional)
  - yearFrom: number (opcional)
  - yearTo: number (opcional)
  - priceMin: number (opcional)
  - priceMax: number (opcional)
  - mileageMax: number (opcional)
  - page: number (default: 1)
  - pageSize: number (default: 12)

Datos Necesarios:
  ✅ 150 vehículos distribuidos:
     - 10 marcas diferentes
     - 7 body styles (Sedan, SUV, Truck, Coupe, etc.)
     - 5 fuel types (Gasoline, Diesel, Hybrid, Electric, LPG)
     - 3 transmissions (Manual, Automatic, CVT)
     - Años 2010-2025
     - Precios variados (5M - 500M DOP)
     - Millaje variado (0 - 300K km)
  ✅ 30 dealers activos
  ✅ Mínimo 1 imagen por vehículo
```

#### GET /api/vehicles/{vehicleId}

```
Datos Necesarios:
  ✅ Detalle completo del vehículo:
     - Title, Description
     - All specs (engine, horsepower, torque)
     - 5-10 imágenes por vehículo
     - Features list (8-15 características)
     - Dealer info completo
     - Status (Active/Inactive/Sold)
     - Timestamps (createdAt, updatedAt)
```

#### GET /api/catalog/makes

```
Datos Necesarios:
  ✅ 10 marcas:
     ["Toyota", "Honda", "Nissan", "BMW", "Mercedes-Benz",
      "Porsche", "Tesla", "Hyundai", "Ford", "Chevrolet"]
```

#### GET /api/catalog/models/{makeId}

```
Por marca:
  Toyota: ["Corolla", "Camry", "RAV4", "Highlander", "4Runner", "Yaris"]
  Honda: ["Civic", "Accord", "CR-V", "Pilot", "Fit"]
  BMW: ["3 Series", "5 Series", "X5", "X3", "M440i", "M850i"]
  Mercedes: ["C-Class", "E-Class", "GLA", "GLC", "S-Class", "AMG"]
  Porsche: ["911", "Cayenne", "Panamera", "Tacan"]
  Tesla: ["Model 3", "Model Y", "Model S", "Model X"]
  Nissan: ["Altima", "Maxima", "Pathfinder", "Rogue", "GT-R"]
  Hyundai: ["Elantra", "Sonata", "Tucson", "Santa Fe", "Kona"]
  Ford: ["Mustang", "F-150", "Edge", "Explorer"]
  Chevrolet: ["Silverado", "Tahoe", "Camaro", "Equinox"]
```

#### GET /api/catalog/years

```
Datos Necesarios:
  ✅ [2010, 2011, ..., 2024, 2025]
  ✅ Mínimo 15 años
```

#### GET /api/homepagesections/homepage

```
Datos Necesarios:
  ✅ 8 secciones activas:
     1. Carousel Principal (5 vehículos)
     2. Sedanes (10 vehículos)
     3. SUVs (10 vehículos)
     4. Camionetas (10 vehículos)
     5. Deportivos (10 vehículos)
     6. Destacados (9 vehículos)
     7. Vehículos de Lujo (10 vehículos)
     8. Eléctricos (10 vehículos)

  ✅ 90 vehículos total distribuidos en secciones
  ✅ Cada vehículo con:
     - name, make, model, year
     - price, mileage
     - fuelType, transmission
     - exteriorColor, bodyStyle
     - imageUrl
     - isPinned, sortOrder
```

#### POST /api/favorites

```
Auth: Required
Body:
  { vehicleId: string }

Datos Necesarios:
  ✅ 5+ usuarios buyers
  ✅ Cada usuario con 5-15 vehículos en favoritos
  ✅ Total: 50+ favoritos almacenados
```

#### GET /api/favorites

```
Auth: Required
Datos Necesarios:
  ✅ Acceso a registros de favorites del usuario autenticado
```

#### DELETE /api/favorites/{vehicleId}

```
Auth: Required
Datos Necesarios:
  ✅ Capacidad de eliminar favoritos
```

---

### 2️⃣ DealerManagementService (`/api/dealers`)

#### GET /api/dealers

```
Parámetros:
  - page: number (default: 1)
  - pageSize: number (default: 20)
  - status: string (optional) - Pending, Active, Suspended, Rejected
  - verificationStatus: string (optional) - NotVerified, Verified, Rejected
  - searchTerm: string (optional) - businessName, RNC, email, city

Datos Necesarios:
  ✅ 30 dealers totales:
     - 10 Independent dealers (70% Verified, 30% Pending)
     - 8 Chain dealers
     - 7 MultipleStore dealers
     - 5 Franchise dealers

  ✅ Cada dealer con:
     - businessName, RNC, legalName
     - email, phone, website
     - address, city, province
     - establishedDate
     - employeeCount
     - description
     - currentPlan (Starter/Pro/Enterprise)
     - status (Pending/Active/Suspended)
     - verificationStatus (NotVerified/Verified)
     - 2-3 locations (branches)
     - maxActiveListings (según plan)
```

#### GET /api/dealers/{dealerId}

```
Datos Necesarios:
  ✅ Perfil público del dealer:
     - Información básica
     - Estadísticas (activeListings, totalReviews, averageRating)
     - Featured vehicles (3-5)
     - Reviews (5+)
     - Locations
```

#### GET /api/dealers/user/{userId}

```
Auth: Required
Datos Necesarios:
  ✅ Obtener dealer asociado al usuario
  ✅ 10 usuarios con cuentas de dealer
```

#### GET /api/dealers/{dealerId}/statistics

```
Datos Necesarios:
  ✅ activeVehicles: number
  ✅ viewsThisMonth: number
  ✅ inquiries: number
  ✅ revenue: number
```

#### POST /api/dealers

```
Auth: Required
Body:
  {
    businessName: string,
    rnc: string,
    legalName: string,
    dealerType: string,
    email: string,
    phone: string,
    mobilePhone: string,
    website: string,
    address: string,
    city: string,
    province: string,
    establishedDate: string,
    employeeCount: number,
    description: string
  }

Datos Necesarios:
  ✅ Capacidad de crear nuevos dealers vía API
```

---

### 3️⃣ UserService (`/api/users`)

#### GET /api/users/me

```
Auth: Required
Datos Necesarios:
  ✅ Usuario autenticado con perfil completo:
     - id, email, firstName, lastName
     - phoneNumber, address
     - accountType (Individual, Seller, Dealer, Admin)
     - isVerified, isActive
     - profileImageUrl
```

#### GET /api/users/me/stats

```
Auth: Required
Parámetros:
  - period: "7d" | "30d" | "90d"

Datos Necesarios:
  ✅ Usuario con historial de actividad:
     - views: total, trend, dailyAverage
     - favorites: total, trend
     - inquiries: total, trend
     - topListing (vehículo más visto)
     - viewsChart (datos históricos)
```

#### PUT /api/users/{userId}

```
Auth: Required
Datos Necesarios:
  ✅ Poder actualizar perfil del usuario
```

---

### 4️⃣ AuthService (`/api/auth`)

#### POST /api/auth/register

```
Body:
  {
    email: string,
    password: string,
    firstName: string,
    lastName: string,
    phoneNumber?: string,
    accountType: "Individual" | "Seller" | "Dealer" | "Admin"
  }

Datos Necesarios:
  ✅ Capacidad de registrar nuevos usuarios
  ✅ El seeding debe crear 40+ usuarios:
     - 10 buyers
     - 10 sellers
     - 30 dealers (en realidad registrados como usuarios primero)
```

#### POST /api/auth/login

```
Body:
  {
    email: string,
    password: string
  }

Response:
  {
    accessToken: string,
    refreshToken: string,
    user: UserDto
  }

Datos Necesarios:
  ✅ Todos los usuarios deben poder hacer login
  ✅ Contraseñas conocidas para testing
```

---

### 5️⃣ MediaService (`/api/media`)

#### POST /api/media/upload

```
Auth: Required
Body: FormData (multipart/form-data)
  - file: File
  - vehicleId?: string
  - documentType?: string

Datos Necesarios:
  ✅ No requiere seeding pre-generado
  ✅ Las imágenes se generan por Picsum URLs
```

#### GET /api/media/{mediaId}

```
Datos Necesarios:
  ✅ Acceso a URLs de imágenes almacenadas
```

---

### 6️⃣ NotificationService (`/api/notifications`)

#### GET /api/notifications

```
Auth: Required
Datos Necesarios:
  ✅ 5+ usuarios con notificaciones pendientes
  ✅ 10+ notificaciones por usuario
```

#### POST /api/notifications/{notificationId}/read

```
Auth: Required
Datos Necesarios:
  ✅ Marcar notificaciones como leídas
```

---

### 7️⃣ BillingService (`/api/billing`)

#### GET /api/billing/plans

```
Datos Necesarios:
  ✅ 3 planes de suscripción:
     {
       id: "plan-starter",
       name: "Starter",
       price: 49,
       period: "month",
       features: [...6 features...],
       maxListings: 15
     },
     {
       id: "plan-pro",
       name: "Pro",
       price: 129,
       period: "month",
       features: [...8 features...],
       maxListings: 50
     },
     {
       id: "plan-enterprise",
       name: "Enterprise",
       price: 299,
       period: "month",
       features: [...10 features...],
       maxListings: unlimited
     }
```

#### POST /api/billing/subscriptions

```
Auth: Required
Body:
  {
    planId: string,
    dealerId: string,
    paymentMethodId?: string
  }

Datos Necesarios:
  ✅ 10+ subscriptions activas para dealers
```

#### GET /api/billing/subscriptions

```
Auth: Required
Datos Necesarios:
  ✅ Acceso a subscripciones del dealer
```

---

### 8️⃣ ComparisonService (`/api/comparisons`)

#### GET /api/comparisons

```
Auth: Required
Datos Necesarios:
  ✅ 3+ usuarios con comparaciones guardadas
  ✅ 1-2 comparaciones por usuario
```

#### POST /api/comparisons

```
Auth: Required
Body:
  {
    name: string,
    vehicleIds: string[] (max 3)
  }

Datos Necesarios:
  ✅ Vehículos con specs completos para comparación
```

---

### 9️⃣ AlertService (`/api/alerts`)

#### GET /api/alerts/price-alerts

```
Auth: Required
Datos Necesarios:
  ✅ 3+ usuarios con alerts activas
  ✅ 5+ alerts por usuario
```

#### POST /api/alerts/price-alerts

```
Auth: Required
Body:
  {
    vehicleId: string,
    targetPrice: number,
    isActive: boolean
  }

Datos Necesarios:
  ✅ Capacidad de crear nuevas alerts
```

#### GET /api/alerts/saved-searches

```
Auth: Required
Datos Necesarios:
  ✅ 2+ usuarios con búsquedas guardadas
  ✅ 3+ búsquedas por usuario
```

---

### 🔟 AdminService (`/api/admin`)

#### GET /api/admin/dashboard/stats

```
Auth: Required (Admin only)
Datos Necesarios:
  ✅ totalUsers: 40+
  ✅ activeListings: 150+
  ✅ pendingApprovals: 5-10
  ✅ revenue: calculado
  ✅ todayViews: agregado
```

#### GET /api/admin/activity-logs

```
Auth: Required (Admin only)
Parámetros:
  - page: number
  - pageSize: number
  - userId?: string
  - entityType?: string
  - startDate?: string
  - endDate?: string

Datos Necesarios:
  ✅ 100+ activity logs distribuidos
  ✅ Tipos de actividad: user_created, listing_created, payment_processed, etc.
```

#### GET /api/admin/users

```
Auth: Required (Admin only)
Datos Necesarios:
  ✅ Acceso a lista de todos los usuarios
  ✅ Filtros por role, isVerified, isActive
```

#### GET /api/admin/pending-approvals

```
Auth: Required (Admin only)
Datos Necesarios:
  ✅ 5-10 vehículos pendientes de aprobación
  ✅ Cada uno con documentación completa
```

---

## 📊 MATRIZ DE DATOS CONSOLIDADA

```json
{
  "endpoints": {
    "vehicles": {
      "count": 150,
      "fields": {
        "id": "UUID",
        "title": "string",
        "make": "string (10 unique makes)",
        "model": "string (50+ models)",
        "year": "number (2010-2025)",
        "price": "number (5M-500M DOP)",
        "mileage": "number (0-300K)",
        "condition": "enum (New, Used, Certified)",
        "fuelType": "enum (5 types)",
        "transmission": "enum (3 types)",
        "bodyStyle": "enum (7 types)",
        "features": "string[] (8-15 items)",
        "images": "URL[] (1-10 per vehicle)",
        "dealerId": "UUID",
        "status": "enum (Active, Inactive, Sold)"
      }
    },
    "dealers": {
      "count": 30,
      "distribution": {
        "Independent": 10,
        "Chain": 8,
        "MultipleStore": 7,
        "Franchise": 5
      },
      "fields": {
        "id": "UUID",
        "businessName": "string",
        "rnc": "string (unique)",
        "dealerType": "enum",
        "email": "string",
        "phone": "string",
        "address": "string",
        "city": "string",
        "currentPlan": "enum (Starter, Pro, Enterprise)",
        "status": "enum (Pending, Active, Suspended)",
        "verificationStatus": "enum",
        "locations": "Location[] (2-3)",
        "rating": "number (0-5)",
        "reviews": "Review[] (5+)"
      }
    },
    "users": {
      "count": 40,
      "breakdown": {
        "buyers": 10,
        "sellers": 10,
        "dealers": 20,
        "admins": 2
      },
      "fields": {
        "id": "UUID",
        "email": "string",
        "firstName": "string",
        "lastName": "string",
        "phoneNumber": "string",
        "accountType": "enum",
        "isVerified": "boolean",
        "isActive": "boolean",
        "createdAt": "datetime"
      }
    },
    "images": {
      "count": 750,
      "distribution": {
        "primary": 150,
        "secondary": 600
      },
      "source": "Picsum Photos API URLs"
    },
    "sections": {
      "count": 8,
      "items": [
        "Carousel Principal (5)",
        "Sedanes (10)",
        "SUVs (10)",
        "Camionetas (10)",
        "Deportivos (10)",
        "Destacados (9)",
        "Lujo (10)",
        "Eléctricos (10)"
      ]
    },
    "catalogs": {
      "makes": 10,
      "models": "50+",
      "years": 15,
      "bodyStyles": 7,
      "fuelTypes": 5,
      "transmissions": 3,
      "colors": "20+"
    },
    "relationships": {
      "favorites": "50+ (5 users × 10+ each)",
      "comparisons": "5+ (3 users × 1-2 each)",
      "alerts": "15+ (3 users × 5+ each)",
      "messages": "100+ messages across 20+ conversations",
      "notifications": "200+ (50+ per user)",
      "subscriptions": "10+",
      "activityLogs": "100+ entries"
    }
  }
}
```

---

## ✅ CHECKLIST PARA SEEDING SCRIPT

### Fase 1: Usuarios

- [ ] 10 buyers (email: buyer{1-10}@okla.local)
- [ ] 10 sellers (email: seller{1-10}@okla.local)
- [ ] 2 admins (email: admin{1-2}@okla.local)
- [ ] Contraseña conocida para todas: Test@123

### Fase 2: Dealers

- [ ] 30 dealers con info completa
- [ ] 10 Independent (70% verified)
- [ ] 8 Chain
- [ ] 7 MultipleStore
- [ ] 5 Franchise
- [ ] Cada uno con 2-3 locations
- [ ] Cada uno con RNC único

### Fase 3: Catálogos

- [ ] 10 Makes
- [ ] 50+ Models (distribuidos por make)
- [ ] 15 Years (2010-2025)
- [ ] 7 Body Styles
- [ ] 5 Fuel Types
- [ ] 3 Transmissions
- [ ] 20+ Colors

### Fase 4: Vehículos

- [ ] 150 vehículos totales
- [ ] 30 vehículos por dealer promedio
- [ ] Distribuidos en:
  - 45 Toyota
  - 30 Hyundai
  - 22 Nissan
  - 22 Ford
  - 15 Mazda
  - 16 Honda
- [ ] Condiciones: 60% Used, 30% New, 10% Certified
- [ ] Todos con specs completos

### Fase 5: Homepage Sections

- [ ] 8 secciones configuradas
- [ ] 90 vehículos asignados a secciones
- [ ] Cada sección con maxItems correcto
- [ ] Carousel con 5 vehículos pinned

### Fase 6: Imágenes

- [ ] 750+ URLs de Picsum
- [ ] 1 primaria por vehículo
- [ ] 4 secundarias promedio por vehículo
- [ ] Tipos variados (Exterior, Interior, Engine, Details)

### Fase 7: Relaciones

- [ ] 50+ favorites almacenados
- [ ] 5+ comparisons
- [ ] 15+ price alerts
- [ ] 100+ activity logs
- [ ] 100+ messages

---

**Este documento es la fuente de verdad para el seeding script actualizado**
