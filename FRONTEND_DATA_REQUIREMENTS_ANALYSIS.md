# 📊 Análisis de Requerimientos de Datos del Frontend

**Fecha:** Enero 15, 2026  
**Objetivo:** Mapear todas las vistas del frontend y sus requerimientos de datos

---

## 🎯 Resumen Ejecutivo

Este documento analiza **todas las páginas del frontend** para identificar exactamente qué datos deben existir en la base de datos para probar cada vista completamente.

### Vistas Principales Identificadas

```
🏠 PÁGINAS PÚBLICAS
├─ HomePage (Landing)
├─ SearchPage (Búsqueda)
├─ VehicleDetailPage
├─ DealerProfilePage
├─ PricingPage
└─ CatalogPage (Años, marcas, modelos)

👤 PÁGINAS AUTENTICADAS
├─ FavoritesPage
├─ ComparisonPage
├─ AlertsPage
├─ MyInquiriesPage
└─ SellerReviewsPage

🏪 DEALER PAGES
├─ DealerLandingPage
├─ DealerPricingPage
├─ DealerRegistrationPage
├─ DealerDashboard (Mi Panel)
├─ InventoryManagementPage
├─ DealerAnalyticsDashboard
├─ DealerProfileEditorPage
├─ PublicDealerProfilePage
├─ PricingIntelligencePage
└─ LeadsDashboard

📨 MESSAGING
├─ ConversationsPage
├─ ChatPage
└─ NotificationsPage

💳 BILLING
├─ CheckoutPage
├─ AzulPaymentPage
└─ PaymentStatusPages

🛠️ ADMIN
├─ AdminDashboard
├─ PendingApprovalsPage
├─ ReportedContentPage
├─ UserManagementPage
└─ SystemSettingsPage

📊 ANALYTICS
├─ UserBehaviorDashboard
├─ FeatureStoreDashboard
└─ LeadScoringDashboard
```

---

## 📋 1️⃣ PÁGINAS PÚBLICAS

### HomePage (Landing Page)

**Endpoint:** `GET /api/homepagesections/homepage`

**Datos Necesarios:**

```json
{
  "sections": [
    {
      "name": "Carousel Principal",
      "slug": "carousel",
      "displayOrder": 1,
      "maxItems": 5,
      "vehicles": [
        {
          "id": "uuid-1",
          "name": "Toyota Corolla 2023",
          "make": "Toyota",
          "model": "Corolla",
          "year": 2023,
          "price": 850000,
          "mileage": 5000,
          "fuelType": "Hybrid",
          "transmission": "Automatic",
          "exteriorColor": "Silver",
          "bodyStyle": "Sedan",
          "imageUrl": "https://picsum.photos/800/600?random=1",
          "isPinned": true,
          "sortOrder": 1
        }
        // ... 4 vehículos más
      ]
    },
    {
      "name": "Sedanes",
      "slug": "sedanes",
      "displayOrder": 2,
      "maxItems": 10,
      "vehicles": [...10 vehículos sedanes...]
    },
    {
      "name": "SUVs",
      "slug": "suvs",
      "maxItems": 10,
      "vehicles": [...10 vehículos SUV...]
    },
    {
      "name": "Camionetas",
      "slug": "camionetas",
      "maxItems": 10,
      "vehicles": [...10 vehículos pickups...]
    },
    {
      "name": "Deportivos",
      "slug": "deportivos",
      "maxItems": 10,
      "vehicles": [...10 vehículos deportivos...]
    },
    {
      "name": "Destacados",
      "slug": "destacados",
      "maxItems": 9,
      "isFeatured": true,
      "vehicles": [...9 vehículos destacados...]
    },
    {
      "name": "Vehículos de Lujo",
      "slug": "lujo",
      "maxItems": 10,
      "vehicles": [...10 vehículos lujo...]
    },
    {
      "name": "Eléctricos",
      "slug": "electricos",
      "maxItems": 10,
      "vehicles": [...10 vehículos eléctricos...]
    }
  ]
}
```

**Datos de Test Requeridos:**

| Elemento          | Cantidad | Descripción                                                   |
| ----------------- | -------- | ------------------------------------------------------------- |
| Secciones activas | 8        | Las listadas arriba                                           |
| Vehículos total   | 90+      | Distribuidos en secciones                                     |
| Imágenes          | 90+      | Mínimo 1 por vehículo                                         |
| Dealers activos   | 10+      | Para la sección "Destacados"                                  |
| Marcas            | 8+       | Toyota, Honda, Nissan, BMW, Mercedes, Porsche, Tesla, Hyundai |

**Validación de Página:**

```bash
# 1. Verificar que se cargan secciones
curl -s http://localhost:18443/api/homepagesections/homepage | jq '.data | length'
# Debe retornar: 8

# 2. Verificar vehículos en carousel
curl -s http://localhost:18443/api/homepagesections/homepage | jq '.data[0].vehicles | length'
# Debe retornar: 5

# 3. Verificar que tengan imágenes
curl -s http://localhost:18443/api/homepagesections/homepage | jq '.data[0].vehicles[0].imageUrl'
# Debe retornar una URL válida
```

---

### SearchPage (Búsqueda y Filtros)

**Endpoints:**

1. `GET /api/vehicles/search?query=...&page=1&pageSize=12`
2. `GET /api/catalog/makes`
3. `GET /api/catalog/models/{makeId}`
4. `GET /api/catalog/years`

**Datos Necesarios:**

```typescript
interface SearchResult {
  items: Vehicle[];
  totalPages: number;
  currentPage: number;
  pageSize: number;
  total: number;
}

interface Vehicle {
  id: string;
  title: string;
  make: string; // "Toyota"
  model: string; // "Corolla"
  year: number; // 2023
  price: number; // 850000
  mileage: number; // 15000
  imageUrl: string; // Primary image
  bodyStyle: string; // "Sedan", "SUV", "Truck"
  fuelType: string; // "Gasoline", "Diesel", "Hybrid"
  transmission: string; // "Manual", "Automatic"
  exteriorColor: string;
  dealerId: string; // Para mostrar vendedor
}
```

**Catálogos Necesarios:**

```typescript
// MAKES (Marcas)
["Toyota", "Honda", "Nissan", "BMW", "Mercedes-Benz", "Porsche", "Tesla", "Hyundai", "Ford", "Chevrolet"]

// MODELS por Make
Toyota: ["Corolla", "Camry", "RAV4", "Highlander", "4Runner", "Yaris"]
Honda: ["Civic", "Accord", "CR-V", "Pilot", "Fit"]
BMW: ["3 Series", "5 Series", "X5", "X3", "M440i"]
Mercedes: ["C-Class", "E-Class", "GLA", "GLC", "S-Class"]
// ... etc

// YEARS
[2010, 2011, ..., 2024, 2025]

// BODY STYLES
["Sedan", "SUV", "Truck", "Coupe", "Hatchback", "Van", "Station Wagon"]

// FUEL TYPES
["Gasoline", "Diesel", "Hybrid", "Electric", "LPG"]

// TRANSMISSIONS
["Manual", "Automatic", "CVT"]
```

**Datos de Test Requeridos:**

| Elemento      | Cantidad | Notas                         |
| ------------- | -------- | ----------------------------- |
| Vehículos     | 150+     | Distribuidos entre 10+ marcas |
| Marcas        | 10       | Con modelos variados cada una |
| Modelos       | 50+      | 5-10 modelos por marca        |
| Años          | 15       | 2010-2025                     |
| Body Styles   | 7        | Todos representados           |
| Fuel Types    | 5        | Todos representados           |
| Transmissions | 3        | Todos representados           |

**Validación:**

```bash
# Buscar por marca
curl -s "http://localhost:18443/api/vehicles/search?make=Toyota" | jq '.items | length'

# Buscar con filtros
curl -s "http://localhost:18443/api/vehicles/search?make=Toyota&yearFrom=2020&priceMax=1000000" | jq '.items | length'

# Obtener marcas
curl -s "http://localhost:18443/api/catalog/makes" | jq '. | length'
# Debe retornar: 10

# Obtener modelos
curl -s "http://localhost:18443/api/catalog/models/Toyota" | jq '. | length'
# Debe retornar: 5+
```

---

### VehicleDetailPage

**Endpoint:** `GET /api/vehicles/{vehicleId}`

**Datos Necesarios:**

```typescript
interface VehicleDetail extends Vehicle {
  // Basic
  id: string;
  title: string;
  description: string;

  // Specs
  make: string;
  model: string;
  year: number;
  condition: "New" | "Used" | "Certified";
  mileage: number;
  vin: string;

  // Visual
  bodyStyle: string;
  exteriorColor: string;
  interiorColor: string;

  // Performance
  fuelType: string;
  transmission: string;
  engine: string; // "2.0L V4"
  horsepower: number;
  torque: number;

  // Features
  features: string[]; // ["Leather Seats", "Sunroof", "Navigation", ...]

  // Pricing
  price: number;
  originalPrice?: number; // Para mostrar descuento

  // Seller Info
  dealerId: string;
  dealerName: string;
  dealerRating: number;
  dealerReviews: number;
  dealerLocation: string;

  // Images
  images: {
    id: string;
    url: string;
    caption?: string;
    type: "Exterior" | "Interior" | "Engine" | "Details";
  }[];

  // Status
  status: "Active" | "Inactive" | "Sold";
  createdAt: string;
  updatedAt: string;
}
```

**Datos de Test Requeridos:**

| Elemento              | Cantidad | Notas                       |
| --------------------- | -------- | --------------------------- |
| Vehículos con detalle | 50+      | Completamente especificados |
| Imágenes por vehículo | 5-10     | Variedad de tipos           |
| Features por vehículo | 8-15     | Diferentes según tipo       |
| Dealers activos       | 15+      | Con ratings                 |

**Validación:**

```bash
curl -s "http://localhost:18443/api/vehicles/{vehicleId}" | jq '{title, make, model, year, price, features, images}'
```

---

### DealerProfilePage

**Endpoint:** `GET /api/dealers/{dealerId}`

**Datos Necesarios:**

```typescript
interface DealerProfile {
  id: string;
  businessName: string;
  rnc: string;
  dealerType: "Independent" | "Chain" | "MultipleStore" | "Franchise";

  // Contact
  email: string;
  phone: string;
  website?: string;

  // Location
  address: string;
  city: string;
  province: string;

  // Profile
  description: string;
  logoUrl?: string;

  // Stats
  totalListings: number;
  activeListings: number;
  soldVehicles: number;
  averageRating: number;
  totalReviews: number;

  // Subscription
  currentPlan: "Starter" | "Pro" | "Enterprise";
  isVerified: boolean;

  // Locations
  locations: Location[];

  // Featured Vehicles
  featuredVehicles: VehiclePreview[];

  // Reviews
  reviews: Review[];
}
```

---

## 👤 2️⃣ PÁGINAS AUTENTICADAS

### FavoritesPage

**Endpoints:**

- `GET /api/favorites` (listar)
- `POST /api/favorites` (agregar)
- `DELETE /api/favorites/{vehicleId}` (eliminar)
- `PUT /api/favorites/{vehicleId}/note` (agregar nota)

**Datos Necesarios:**

```typescript
interface Favorite {
  id: string;
  vehicleId: string;
  vehicleData: {
    id: string;
    title: string;
    price: number;
    imageUrl: string;
    make: string;
    model: string;
    year: number;
  };
  note?: string;
  savedAt: string;
  isFavorite: boolean;
}
```

**Datos de Test:**

| Elemento               | Cantidad |
| ---------------------- | -------- |
| Usuarios con favoritos | 5+       |
| Favoritos por usuario  | 5-15     |

---

### ComparisonPage

**Endpoints:**

- `GET /api/comparisons` (listar)
- `POST /api/comparisons` (crear)
- `POST /api/comparisons/{id}/vehicles/{vehicleId}` (agregar vehículo)
- `DELETE /api/comparisons/{id}/vehicles/{vehicleId}` (remover)

**Datos Necesarios:**

- Mínimo 50 vehículos con specs completos para comparar
- Diferentes marcas y tipos

---

### AlertsPage

**Endpoints:**

- `GET /api/alerts/price-alerts`
- `GET /api/alerts/saved-searches`

**Datos Necesarios:**

```typescript
interface PriceAlert {
  id: string;
  vehicleId: string;
  targetPrice: number;
  currentPrice: number;
  isActive: boolean;
  createdAt: string;
}

interface SavedSearch {
  id: string;
  name: string;
  filters: SearchFilters;
  matchCount: number;
  isActive: boolean;
  createdAt: string;
}
```

---

## 🏪 3️⃣ DEALER PAGES

### DealerLandingPage

**Datos Necesarios:**

- Texto estático (sin API)
- Early Bird banner (config en base de datos)
- Estadísticas de dashboard (opcional)

### DealerDashboard

**Endpoints:**

- `GET /api/dealers/user/{userId}` (obtener dealer del usuario)
- `GET /api/dealers/{dealerId}/statistics` (estadísticas)
- `GET /api/dealers/{dealerId}/inventory` (inventario)

**Datos Necesarios:**

```typescript
interface DealerDashboard {
  dealer: {
    id: string;
    businessName: string;
    rnc: string;
    currentPlan: string;
    status: "Pending" | "Active" | "Suspended";
    maxActiveListings: number;
  };

  statistics: {
    activeVehicles: number;
    viewsThisMonth: number;
    inquiries: number;
    revenue: number;
  };

  recentActivity: Activity[];
}
```

---

## 📨 4️⃣ MESSAGING

### ConversationsPage

**Endpoints:**

- `GET /api/messaging/conversations`
- `GET /api/messaging/conversations/{conversationId}/messages`
- `POST /api/messaging/conversations/{conversationId}/messages`

**Datos Necesarios:**

```typescript
interface Conversation {
  id: string;
  participantId: string;
  participantName: string;
  lastMessage: string;
  lastMessageTime: string;
  unreadCount: number;
}

interface Message {
  id: string;
  conversationId: string;
  senderId: string;
  content: string;
  sentAt: string;
  isRead: boolean;
}
```

**Datos de Test:**

| Elemento                   | Cantidad |
| -------------------------- | -------- |
| Usuarios para mensajería   | 20+      |
| Conversaciones por usuario | 3-10     |
| Mensajes por conversación  | 5-20     |

---

## 💳 5️⃣ BILLING & PAYMENTS

### CheckoutPage

**Endpoints:**

- `POST /api/billing/checkout`
- `GET /api/billing/plans`

**Datos Necesarios:**

```typescript
interface Plan {
  id: string;
  name: "Starter" | "Pro" | "Enterprise";
  price: number;
  period: "month" | "year";
  features: string[];
  maxListings: number;
}

interface CartItem {
  planId: string;
  quantity: 1;
  price: number;
}
```

---

## 🛠️ 6️⃣ ADMIN PAGES

### AdminDashboard

**Endpoints:**

- `GET /api/admin/dashboard/stats`
- `GET /api/admin/activity-logs`

**Datos Necesarios:**

```typescript
interface DashboardStats {
  totalUsers: number;
  activeListings: number;
  pendingApprovals: number;
  revenue: number;
  todayViews: number;
}

interface ActivityLog {
  id: string;
  userId: string;
  action: string;
  entityType: string;
  entityId: string;
  timestamp: string;
}
```

---

## 📊 MATRIZ DE DATOS POR VISTA

### Tabla Resumen

| Vista           | Endpoint                         | Datos Mínimos                | Status         |
| --------------- | -------------------------------- | ---------------------------- | -------------- |
| HomePage        | `/api/homepagesections/homepage` | 90 vehículos en 8 secciones  | ✅ Documentado |
| SearchPage      | `/api/vehicles/search`           | 150 vehículos, catálogos     | ✅ Documentado |
| VehicleDetail   | `/api/vehicles/{id}`             | 50+ vehículos completos      | ✅ Documentado |
| DealerProfile   | `/api/dealers/{id}`              | 30 dealers verificados       | ✅ Documentado |
| Favorites       | `/api/favorites`                 | Requiere auth                | ✅ Documentado |
| Comparison      | `/api/comparisons`               | Requiere auth                | ✅ Documentado |
| Alerts          | `/api/alerts`                    | Requiere auth                | ✅ Documentado |
| DealerDashboard | `/api/dealers/user/{id}`         | 30 dealers + usuarios        | ✅ Documentado |
| Messaging       | `/api/messaging/*`               | 20+ usuarios, conversaciones | ✅ Documentado |
| AdminDash       | `/api/admin/*`                   | Stats agregadas              | ✅ Documentado |

---

## 🎯 DISTRIBUCIÓN RECOMENDADA DE DATA

### Vehículos (150 total)

```
Marcas:
├─ Toyota: 30 (Sedanes 15, SUVs 10, Trucks 5)
├─ Honda: 20 (Sedanes 10, SUVs 8, Compactos 2)
├─ Nissan: 20 (Sedanes 8, SUVs 8, Trucks 4)
├─ BMW: 15 (Sedanes 10, SUVs 5)
├─ Mercedes: 15 (Sedanes 8, SUVs 5, Coupe 2)
├─ Porsche: 10 (Deportivos 10)
├─ Tesla: 12 (Model 3 5, Model Y 5, Model S 2)
├─ Hyundai: 15 (Sedanes 8, SUVs 7)
├─ Ford: 10 (Trucks 7, SUVs 3)
└─ Chevrolet: 8 (Trucks 5, SUVs 3)

Condiciones:
├─ New: 15 (10%)
├─ Certified: 15 (10%)
└─ Used: 120 (80%)

Dealers:
└─ Distribuidos entre 30 dealers (5 vehículos promedio por dealer)
```

### Usuarios (40 total)

```
Buyers: 10
├─ Con favoritos: 5
├─ Con comparaciones: 3
└─ Con alertas: 3

Sellers: 10
├─ Con vehículos publicados: 10
├─ Con mensajes: 8
└─ Con reviews: 5

Dealers: 30
├─ Verified: 21 (70%)
├─ Pending: 9 (30%)
└─ Con inventario activo: 25

Admins: 2
└─ Con permisos completos

Total: 52 usuarios
```

### Imágenes (150+ total)

```
Total: 150+ imágenes
├─ 150 primarias (1 por vehículo)
├─ 300-450 secundarias (2-3 por vehículo)
├─ Tipos:
│  ├─ Exterior Front: 150 (30%)
│  ├─ Exterior Back: 120 (24%)
│  ├─ Interior: 120 (24%)
│  ├─ Engine: 100 (20%)
│  └─ Details: 10 (2%)
└─ Fuente: Picsum Photos URLs
```

---

## ✅ CHECKLIST DE VALIDACIÓN

### Paso 1: Verificar Datos Base

```bash
# Vehículos
curl -s http://localhost:18443/api/vehicles/count | jq .
# Esperado: 150

# Dealers
curl -s http://localhost:18443/api/dealers?pageSize=100 | jq '.data | length'
# Esperado: 30

# Usuarios
psql -h localhost -U postgres -d cardealer -c "SELECT COUNT(*) FROM users;"
# Esperado: 40+
```

### Paso 2: Validar Secciones

```bash
# Homepage sections
curl -s http://localhost:18443/api/homepagesections/homepage | jq '.data | map(.name)'
# Esperado: 8 secciones

# Vehículos por sección
curl -s http://localhost:18443/api/homepagesections/homepage | jq '.data[0].vehicles | length'
# Esperado: 5 (carousel)
```

### Paso 3: Validar Búsqueda

```bash
# Búsqueda simple
curl -s "http://localhost:18443/api/vehicles/search?query=toyota" | jq '.items | length'
# Esperado: 30+ resultados

# Búsqueda con filtros
curl -s "http://localhost:18443/api/vehicles/search?make=Toyota&yearFrom=2020" | jq '.items | length'
# Esperado: 15+
```

### Paso 4: Validar Autenticación

```bash
# Login
TOKEN=$(curl -s -X POST http://localhost:18443/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"buyer1@okla.local","password":"Test@123"}' | jq -r '.data.accessToken')

# Favoritos
curl -s http://localhost:18443/api/favorites \
  -H "Authorization: Bearer $TOKEN" | jq '.data | length'
# Esperado: 5+
```

---

## 🚀 PRÓXIMOS PASOS

1. ✅ **Actualizar DatabaseSeedingService** con esta matriz de datos
2. ✅ **Crear builders más específicos** para cada tipo de dato
3. ✅ **Validar que el seeding genere exactamente** lo necesario
4. ✅ **Crear script de validación** para verificar completitud

---

**Análisis completado: 10 vistas públicas + 6 autenticadas + 8 dealer + 3 admin = 27 vistas mapeadas**
