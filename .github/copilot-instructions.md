# 🤖 GitHub Copilot Instructions - CarDealer Microservices

Este documento proporciona contexto completo para que GitHub Copilot pueda asistir efectivamente en el desarrollo de este proyecto de marketplace multi-vertical.

---

## 📋 RESUMEN DEL PROYECTO

**CarDealer** es una plataforma **SaaS multi-tenant** de marketplace para compra y venta de vehículos (extensible a otros verticales como bienes raíces). Implementa una arquitectura de **microservicios** con Clean Architecture.

### Stack Tecnológico Principal

| Capa                  | Tecnología                                  | Versión     |
| --------------------- | ------------------------------------------- | ----------- |
| **Backend**           | .NET 8.0 LTS                                | net8.0      |
| **Frontend Web**      | React 19 + TypeScript + Vite                | ^19.0.0     |
| **Frontend Mobile**   | Flutter + Dart                              | SDK >=3.2.0 |
| **Base de Datos**     | PostgreSQL (principal), SQL Server, Oracle  | 16+         |
| **Cache**             | Redis (StackExchange.Redis)                 | 2.8.22      |
| **Message Broker**    | RabbitMQ                                    | 6.8.1       |
| **API Gateway**       | Ocelot                                      | 22.0.1      |
| **Service Discovery** | Consul                                      | 1.7.14.9    |
| **Observabilidad**    | OpenTelemetry, Serilog, Prometheus, Grafana | 1.14.0      |
| **Contenedores**      | Docker + Docker Compose                     | -           |

---

## 🚀 PLAN DE INTEGRACIÓN FRONTEND-BACKEND (2 Enero 2026)

> **📊 ANÁLISIS COMPLETO:** 8 documentos técnicos en `docs/analysis/`  
> **🎯 OBJETIVO:** Conectar frontend React con backend .NET para eliminar mock data  
> **⚠️ REGLA CRÍTICA:** SIEMPRE consultar este contexto antes de codificar integraciones

### Estado Actual de Integración

| Métrica                     | Valor                | Estado           |
| --------------------------- | -------------------- | ---------------- |
| **Páginas Frontend**        | 59 páginas           | ✅ Creadas       |
| **Microservicios Backend**  | 35 servicios         | ✅ Operacionales |
| **Integración Completa**    | 15 páginas (25.4%)   | 🔴 BAJO          |
| **Usando Mock Data**        | 34 páginas (57.6%)   | 🔴 CRÍTICO       |
| **Servicios Desconectados** | 10 servicios (28.6%) | 🔴 ALTO          |

**Problema Principal:** El backend está funcional pero el frontend NO lo consume (usa mocks).

---

### Decisión Estratégica: NO Crear Nuevos Microservicios

**❌ NO crear servicios nuevos** - Los 35 existentes cubren 100% necesidades

| Evaluación          | Decisión    | Justificación                              |
| ------------------- | ----------- | ------------------------------------------ |
| ReviewService       | ❌ NO crear | Extender ProductService (12-16h vs 40-50h) |
| AnalyticsService    | ❌ NO crear | Extender ReportsService (16-20h vs 50-60h) |
| MessageService      | ❌ NO crear | Ya existe NotificationService              |
| SubscriptionService | ❌ NO crear | Ya existe BillingService                   |
| InventoryService    | ❌ NO crear | Extender ProductService                    |

**Ahorro:** 120-180 horas evitando crear servicios innecesarios

---

### Frontend: Páginas Existentes (59 total)

#### Admin (7 páginas)

- ✅ AdminDashboardPage - **❌ NO conectada** (AdminService existe, 11 endpoints)
- ✅ AdminListingsPage - **⚠️ Parcial** (ProductService)
- ✅ AdminReportsPage - **❌ NO conectada** (ReportsService existe, 10 endpoints)
- ✅ AdminSettingsPage - Mock data
- ✅ CategoriesManagementPage - Mock data
- ✅ PendingApprovalsPage - Mock data
- ✅ UsersManagementPage - **❌ NO conectada** (UserService existe)

#### Auth (2 páginas)

- ✅ LoginPage - **✅ FUNCIONAL** (AuthService 15085)
- ✅ RegisterPage - **✅ FUNCIONAL** (AuthService 15085)

#### Billing (6 páginas)

- ✅ BillingDashboardPage - **⚠️ Parcial** (BillingService 15008)
- ✅ CheckoutPage - **✅ FUNCIONAL** (Stripe integration)
- ✅ InvoicesPage - **❌ NO conectada** (InvoicingService existe, 6 endpoints)
- ✅ PaymentMethodsPage - **⚠️ Parcial** (Stripe)
- ✅ PaymentsPage - **⚠️ Parcial** (BillingService)
- ✅ PlansPage - **✅ FUNCIONAL** (BillingService)

#### Dealer (8 páginas)

- ✅ DealerDashboardPage - **⚠️ Parcial** (stats faltantes)
- ✅ DealerListingsPage - **⚠️ Parcial** (ProductService)
- ✅ CRMPage - **❌ NO conectada** (CRMService existe, 7 endpoints)
- ✅ AnalyticsPage - **❌ NO conectada** (ReportsService existe)
- ✅ CreateListingTestPage - Mock data
- ✅ DealerAnalyticsPage - Mock data
- ✅ PlansComparisonTestPage - Mock data

#### Marketplace (6 páginas)

- ✅ BrowsePage - **⚠️ Parcial** (ProductService search incompleto)
- ✅ ListingFormPage - **⚠️ Parcial** (ProductService)
- ✅ PropertyDetailPage - **❌ NO conectada** (RealEstateService existe, 8 endpoints)
- ✅ VehicleDetailPage - **⚠️ Parcial** (ProductService)
- ✅ FavoritesPage - **❌ NO backend** (endpoint a crear: 4-6h)
- ✅ SellerDashboardPage - Mock data

#### User (4 páginas)

- ✅ ProfilePage - **⚠️ Parcial** (UserService)
- ✅ MessagesPage - **❌ NO backend** (MessageService a crear en NotificationService)
- ✅ UserDashboardPage - **⚠️ Parcial** (stats incompletos)
- ✅ WishlistPage - **❌ NO backend** (endpoint a crear: 4-6h)

#### Vehicles (6 páginas)

- ✅ VehicleDetailPage - **⚠️ Parcial** (ProductService)
- ✅ VehiclesHomePage - **⚠️ Parcial** (ProductService)
- ✅ BrowsePage - **⚠️ Parcial** (search incompleto)
- ✅ ComparePage - **❌ NO backend** (endpoint a crear: 6-8h)
- ✅ MapViewPage - **❌ NO backend** (geolocation a crear: 8-10h)
- ✅ SellYourCarPage - **⚠️ Parcial** (ProductService)

**Leyenda:**

- ✅ FUNCIONAL = Backend conectado, datos reales
- ⚠️ PARCIAL = Algunas features funcionan, otras no
- ❌ NO conectada/NO backend = Usa mock data o backend no existe

---

### Backend: Microservicios Operacionales (35 total)

#### 🟢 Servicios FUNCIONALES y Consumidos (8)

1. **AuthService** (15085) - 15 endpoints - ✅ Login/Register/JWT funcional
2. **ProductService** (15006) - 18 endpoints - ✅ CRUD básico, ⚠️ Faltan: favorites, comparison, geolocation, reviews
3. **BillingService** (15008) - 12 endpoints - ✅ Stripe integration
4. **Gateway** (18443) - Ocelot routing - ✅ 7 rutas configuradas
5. **ErrorService** (15083) - 6 endpoints - ✅ Centralización de errores
6. **CacheService** - 7 endpoints - ✅ Redis operations
7. **MessageBusService** - 17 endpoints - ✅ RabbitMQ abstraction
8. **LoggingService** - 23 endpoints - ✅ Logs centralizados

#### 🟡 Servicios Parcialmente Conectados (5)

9. **UserService** (15100) - 8 endpoints - ⚠️ CRUD básico, faltan stats/activity
10. **RoleService** (15101) - 6 endpoints - ⚠️ Backend OK, sin UI frontend
11. **MediaService** (15090) - 8 endpoints - ⚠️ Upload básico, falta drag & drop
12. **NotificationService** (15084) - 17 endpoints - ⚠️ Email/SMS OK, falta SignalR real-time
13. **ServiceDiscovery** - 10 endpoints - ⚠️ Consul OK, falta UI health

#### 🔴 Servicios Backend OK pero NO Consumidos (10)

14. **CRMService** (15009) - 7 endpoints - ❌ CRMPage existe pero usa mocks
15. **AdminService** (15011) - 11 endpoints - ❌ AdminDashboard parcial
16. **ReportsService** (15010) - 10 endpoints - ❌ Analytics pages con mocks
17. **SchedulerService** (15012) - 6 endpoints - ❌ Sin UI frontend
18. **RealEstateService** (15034) - 8 endpoints - ❌ PropertyDetailPage usa mocks
19. **InvoicingService** (15031) - 6 endpoints - ❌ InvoicesPage usa mocks
20. **FinanceService** (15029) - 8 endpoints - ❌ Sin UI frontend
21. **ContactService** (15030) - 4 endpoints - ❌ Contact form no guarda
22. **AppointmentService** (15032) - 5 endpoints - ❌ Sin UI calendar
23. **AuditService** - 7 endpoints - ❌ Sin UI frontend

#### ⚪ Servicios Infraestructura (12) - NO requieren UI

24-35. Gateway, ErrorService, CacheService, MessageBusService, LoggingService, TracingService, HealthCheckService, ConfigurationService, FeatureToggleService, FileStorageService, BackupDRService, SearchService

---

### Features a Agregar al Backend (48 endpoints nuevos)

#### 🔴 Prioridad ALTA (140-178h)

**ProductService** (40-52h):

- [ ] **Favorites/Wishlist** (4-6h)
  - Tabla: `product_favorites` (user_id, product_id, created_at)
  - Endpoints: `GET/POST/DELETE /products/{id}/favorite`
  - Frontend: FavoritesPage, WishlistPage
- [ ] **Vehicle Comparison** (6-8h)
  - Endpoint: `POST /products/compare` (recibe IDs, retorna comparación)
  - Frontend: ComparePage con tabla lado a lado
- [ ] **Geolocation Search** (8-10h)
  - PostgreSQL: Agregar columna `GEOGRAPHY(POINT, 4326)` a products
  - Endpoint: `GET /products/nearby?lat={lat}&lng={lng}&radius={km}`
  - Query: `ST_Distance(location, ST_MakePoint({lng}, {lat})::geography) <= {radius}`
  - Frontend: MapViewPage con Google Maps
- [ ] **Saved Searches** (10-12h)
  - Tabla: `saved_searches` (user_id, filters_json, name, email_alerts)
  - Endpoints: `GET/POST/DELETE /searches/saved`
  - Email alerts con Hangfire
- [ ] **Reviews & Ratings** (12-16h)
  - Tablas: `product_reviews`, `review_votes`
  - Endpoints: `GET/POST /products/{id}/reviews`, `/reviews/{id}/vote`
  - Frontend: Ratings component con estrellas

**NotificationService** (24-30h):

- [ ] **SignalR Hub** (8-10h backend + 10-12h frontend)
  - C#: `NotificationHub.cs` con `SendNotificationAsync`
  - TypeScript: `@microsoft/signalr`, `HubConnectionBuilder`
  - Eventos: user.notification, listing.new, message.received
- [ ] **Notification Center UI** (6-8h)
  - NotificationBell component (badge count)
  - NotificationsPage con lista paginada
  - Toast notifications con react-hot-toast

**UserService** (14-18h):

- [ ] **Dashboard Stats** (6-8h)
  - Endpoint: `GET /users/{id}/stats` (listings, views, favorites, messages)
  - Agregar queries a UserRepository
- [ ] **Activity Feed** (8-10h)
  - Tabla: `user_activities` (user_id, action, entity_type, entity_id)
  - Endpoint: `GET /users/{id}/activities`
  - Frontend: Timeline component

**AdminService** (18-22h):

- [ ] **System Health Dashboard** (10-12h)
  - Endpoint: `GET /admin/health/all` (health check 35 services)
  - HttpClient checks a cada servicio + DBs
  - Frontend: SystemHealthPage con status cards
- [ ] **Bulk Operations** (8-10h)
  - Endpoints: `POST /admin/listings/bulk-approve`, `/bulk-reject`
  - Procesar arrays de IDs

**ReportsService** (26-34h):

- [ ] **Dashboard Widgets API** (12-16h backend + 14-18h frontend)
  - 8 widget types: SalesOverview, ListingsStats, UserGrowth, RevenueChart, TopProducts, ConversionFunnel, GeographicMap, RealtimeActivity
  - Endpoints: `GET /reports/widgets/{type}?dateRange={range}`
  - Frontend: ResponsiveGrid con Recharts

**MediaService** (18-22h):

- [ ] **Drag & Drop Upload** (8-10h)
  - Frontend: DropZone component con react-dropzone
  - Progress bars, preview thumbnails
- [ ] **Image Processing** (10-12h)
  - Backend: ImageSharp library
  - Resize, watermark, compression, format conversion
  - Endpoints: `POST /media/process`

#### 🟠 Prioridad MEDIA (46-56h)

**CRMService** (12-14h):

- [ ] Activity timeline, interaction tracking

**InvoicingService** (10-12h):

- [ ] PDF generation con QuestPDF

**ContactService** (8-10h):

- [ ] Admin dashboard para messages

**AppointmentService** (16-20h):

- [ ] Calendar UI con FullCalendar, drag-drop

#### 🟡 Prioridad BAJA (26-30h)

**SchedulerService** (12-14h):

- [ ] Jobs management UI (Hangfire dashboard)

**RoleService** (14-16h):

- [ ] Roles & permissions UI completo

**Total Features Backend:** 212-264 horas

---

### 🖼️ MIGRACIÓN DE ASSETS: Imágenes Frontend → Backend (CRÍTICO)

#### Estado Actual del Frontend

El frontend React actualmente usa:

1. **Imágenes externas** (Unsplash, Lorem Picsum, placeholders online)
2. **Imágenes hardcodeadas** en componentes con URLs externas
3. **Assets locales** en `/public` o `/src/assets`
4. **Mock data** con URLs de servicios externos

#### Problema

- ⚠️ **Dependencia de servicios externos** (Unsplash API limits, servicios pueden caer)
- ⚠️ **Sin control de calidad/contenido** (imágenes pueden cambiar o desaparecer)
- ⚠️ **Performance issues** (CORS, latencia, sin CDN control)
- ⚠️ **No production-ready** (producción NO puede depender de servicios demo)

#### Solución: Migración Completa a MediaService/FileStorageService

**Sprint 0.5 (Pre-Sprint 1) - 16-20 horas CRÍTICAS**

##### Paso 1: Auditoría de Imágenes (4-5h)

```bash
# Script para identificar todas las URLs de imágenes
grep -r "unsplash.com\|picsum.photos\|placeholder.com\|via.placeholder" frontend/web/src/
grep -r "https://.*\.(jpg|jpeg|png|gif|svg|webp)" frontend/web/src/

# Resultado esperado: Lista de ~200-300 imágenes únicas
```

**Categorías a auditar:**

- Hero images (homepage, landing pages)
- Vehicle thumbnails (listings, cards)
- Property images (real estate)
- User avatars (profiles, comments)
- UI icons/logos
- Background images
- Product gallery images

##### Paso 2: Descargar y Organizar (3-4h)

```bash
# Script automatizado
# scripts/download-frontend-assets.sh

#!/bin/bash
mkdir -p temp-assets/{vehicles,properties,avatars,ui,backgrounds}

# Descargar de Unsplash API
curl "https://api.unsplash.com/photos/..." -o temp-assets/vehicles/car-001.jpg

# Optimizar imágenes
for img in temp-assets/**/*.{jpg,jpeg,png}; do
  convert "$img" -resize 1920x1080\> -quality 85 "$img"
done
```

**Estructura sugerida:**

```
temp-assets/
├── vehicles/
│   ├── cars/
│   ├── trucks/
│   ├── motorcycles/
│   └── boats/
├── properties/
│   ├── houses/
│   ├── apartments/
│   └── commercial/
├── avatars/
│   ├── users/
│   └── dealers/
├── ui/
│   ├── icons/
│   ├── logos/
│   └── patterns/
└── backgrounds/
    ├── hero/
    └── sections/
```

##### Paso 3: Seed Database con Imágenes (6-8h)

**Backend: Crear Migration Script**

```csharp
// backend/MediaService/Scripts/SeedAssets.cs

public class SeedMediaAssets
{
    public async Task ExecuteAsync()
    {
        var assetsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets");

        // 1. Subir a FileStorage (S3/Azure)
        var vehicleImages = Directory.GetFiles(Path.Combine(assetsPath, "vehicles"));
        foreach (var imagePath in vehicleImages)
        {
            var fileUrl = await _fileStorageService.UploadAsync(
                file: File.OpenRead(imagePath),
                fileName: Path.GetFileName(imagePath),
                container: "vehicles",
                isPublic: true
            );

            // 2. Registrar en media_files table
            await _mediaRepository.AddAsync(new MediaFile
            {
                FileName = Path.GetFileName(imagePath),
                FileUrl = fileUrl,
                FileType = "image/jpeg",
                Category = "vehicles",
                Tags = new[] { "car", "sedan", "demo" },
                IsPublic = true,
                UploadedBy = "system"
            });
        }

        // Repetir para properties, avatars, etc.
    }
}
```

**Ejecutar seed:**

```bash
dotnet run --project backend/MediaService/MediaService.Api -- seed-assets
```

##### Paso 4: Actualizar Frontend para Consumir MediaService (3-4h)

**A. Crear servicio de assets**

```typescript
// frontend/web/src/services/assetService.ts

const MEDIA_SERVICE_URL =
  import.meta.env.VITE_MEDIA_SERVICE_URL || "http://localhost:15090";

export const assetService = {
  // Get vehicle image by category and index
  getVehicleImage: (
    category: "car" | "truck" | "motorcycle",
    index: number
  ) => {
    return `${MEDIA_SERVICE_URL}/api/media/vehicles/${category}/${index}`;
  },

  // Get property image
  getPropertyImage: (type: "house" | "apartment", index: number) => {
    return `${MEDIA_SERVICE_URL}/api/media/properties/${type}/${index}`;
  },

  // Get user avatar
  getUserAvatar: (userId?: string) => {
    return userId
      ? `${MEDIA_SERVICE_URL}/api/media/avatars/${userId}`
      : `${MEDIA_SERVICE_URL}/api/media/avatars/default.jpg`;
  },

  // Get UI asset
  getUIAsset: (path: string) => {
    return `${MEDIA_SERVICE_URL}/api/media/ui/${path}`;
  },
};
```

**B. Reemplazar URLs hardcodeadas**

```typescript
// ❌ ANTES
const vehicleImage = "https://images.unsplash.com/photo-1234567890/car.jpg";

// ✅ DESPUÉS
import { assetService } from "@/services/assetService";
const vehicleImage = assetService.getVehicleImage("car", 1);
```

**C. Componente ImageWithFallback**

```typescript
// frontend/web/src/components/ImageWithFallback.tsx

export const ImageWithFallback = ({
  src,
  fallback = "/placeholder.jpg",
  alt,
  ...props
}: ImageProps) => {
  const [imgSrc, setImgSrc] = useState(src);

  const handleError = () => {
    console.error(`Failed to load image: ${src}`);
    setImgSrc(fallback);
  };

  return (
    <img
      src={imgSrc}
      alt={alt}
      onError={handleError}
      loading="lazy"
      {...props}
    />
  );
};

// Uso:
<ImageWithFallback
  src={assetService.getVehicleImage("car", vehicle.id)}
  alt={vehicle.name}
/>;
```

##### Paso 5: Backend Endpoints para Servir Assets

**MediaService: Agregar endpoints de assets públicos**

```csharp
// MediaService.Api/Controllers/AssetsController.cs

[ApiController]
[Route("api/media")]
public class AssetsController : ControllerBase
{
    private readonly IFileStorageService _storage;

    [HttpGet("vehicles/{category}/{index}")]
    [ResponseCache(Duration = 86400)] // Cache 24 horas
    public async Task<IActionResult> GetVehicleImage(string category, int index)
    {
        var fileName = $"{category}-{index:D3}.jpg";
        var fileUrl = await _storage.GetPublicUrlAsync("vehicles", fileName);

        if (string.IsNullOrEmpty(fileUrl))
            return NotFound();

        // Redirect to S3/Azure URL (o stream directo si es local)
        return Redirect(fileUrl);
    }

    [HttpGet("properties/{type}/{index}")]
    public async Task<IActionResult> GetPropertyImage(string type, int index)
    {
        // Similar a vehicles
    }

    [HttpGet("avatars/{userId}")]
    public async Task<IActionResult> GetUserAvatar(string userId)
    {
        // Query database para avatar del user
        var avatar = await _mediaRepository.GetUserAvatarAsync(userId);

        if (avatar == null)
            return Redirect("/api/media/avatars/default.jpg");

        return Redirect(avatar.FileUrl);
    }
}
```

#### Checklist de Migración

**Backend:**

- [ ] Descargar todas las imágenes de Unsplash/placeholders (~200-300 imágenes)
- [ ] Organizar en estructura de carpetas (vehicles/properties/avatars/ui)
- [ ] Optimizar imágenes (resize, compress, format conversion)
- [ ] Crear seed script para MediaService
- [ ] Subir a FileStorageService (S3/Azure)
- [ ] Crear endpoints públicos en MediaService
- [ ] Configurar CDN (CloudFlare/AWS CloudFront) para assets
- [ ] Agregar response caching (24h-7d)

**Frontend:**

- [ ] Audit completo de URLs hardcodeadas
- [ ] Crear assetService.ts
- [ ] Crear ImageWithFallback component
- [ ] Reemplazar TODAS las URLs en componentes:
  - [ ] VehicleCard.tsx
  - [ ] PropertyCard.tsx
  - [ ] UserAvatar.tsx
  - [ ] HeroSection.tsx
  - [ ] Gallery components
  - [ ] Background images en CSS
- [ ] Agregar VITE_MEDIA_SERVICE_URL en .env
- [ ] Testing de carga de imágenes
- [ ] Verificar lazy loading funciona
- [ ] Verificar fallbacks funcionan

**DevOps:**

- [ ] Configurar CORS en MediaService para frontend
- [ ] Configurar CDN con cache policies
- [ ] Agregar monitoring de asset loading (% success rate)
- [ ] Documentar estructura de assets en README

#### Estimación Total: 16-20 horas

| Tarea                 | Esfuerzo   |
| --------------------- | ---------- |
| Auditoría             | 4-5h       |
| Descargar + Organizar | 3-4h       |
| Seed Database         | 6-8h       |
| Frontend Update       | 3-4h       |
| **TOTAL**             | **16-20h** |

#### Integración en Roadmap

**🔴 CRÍTICO: Ejecutar ANTES de Sprint 1**

Este trabajo debe hacerse en **Sprint 0.5** (1 semana) ANTES del roadmap principal porque:

1. Todas las páginas dependen de imágenes
2. Sin esto, producción NO es viable (dependencia de servicios externos)
3. Bloqueante para testing real de UI
4. Necesario para performance benchmarking

**Actualización del Timeline:**

```
Sprint 0.5 (1 semana): Asset Migration (16-20h) 🔴 CRÍTICO
├── Semana 1 ─────────────────────────────────────────┐
Sprint 1 (2 semanas): Favorites + Notifications       │
Sprint 2 (2 semanas): Real Estate + Admin             │← Roadmap original
Sprint 3-12: (continúa...)                            │
└──────────────────────────────────────────────────────┘
```

#### Riesgos y Mitigación

| Riesgo                      | Probabilidad | Impacto | Mitigación                                         |
| --------------------------- | ------------ | ------- | -------------------------------------------------- |
| Imágenes tienen copyright   | Media        | Alto    | Usar solo Unsplash API (libre uso) o comprar stock |
| Tamaño total > 5GB          | Alta         | Medio   | Compress agresivo + WebP format                    |
| CDN costo inesperado        | Baja         | Medio   | Calcular tráfico estimado antes                    |
| Frontend cachea URLs viejas | Media        | Bajo    | Cache busting con query params                     |

#### Assets Recomendados

**Fuentes gratuitas con licencia comercial:**

- Unsplash API (vehículos, propiedades)
- Pexels API (avatares, backgrounds)
- Pixabay (UI elements)
- UI Faces (avatars genéricos)

**Cantidades sugeridas:**

- Vehicles: 100-150 imágenes (10-15 por categoría)
- Properties: 80-100 imágenes (20-25 por tipo)
- Avatars: 30-50 (usuarios demo)
- UI/Backgrounds: 20-30

**Tamaño estimado total:** 2-3 GB (antes compress), 800MB-1.2GB (después)

---

### UI Faltante en Frontend (15 páginas + 32 componentes)

#### 🔴 Páginas Críticas a Crear (118-140h)

1. **Notification Center** (18-22h)

   - NotificationBell component (2-3h)
   - NotificationsPage full (10-12h)
   - SignalR client (6-7h)

2. **Real Estate Management** (24-28h)

   - RealEstateListingsPage (10-12h)
   - PropertyFormPage con location picker (14-16h)

3. **Roles & Permissions Management** (14-16h)

   - RolesManagementPage con PermissionsGrid

4. **Jobs Management Dashboard** (12-14h)

   - JobsManagementPage (Hangfire UI)

5. **Finance Dashboard** (20-24h)

   - FinanceDashboardPage (12-14h)
   - TransactionsPage (8-10h)

6. **Appointment Calendar** (16-20h)

   - CalendarPage con FullCalendar

7. **Contact Messages Admin** (8-10h)

   - ContactMessagesPage

8. **System Health Dashboard** (10-12h)
   - SystemHealthPage (monitor 35 services)

#### 🟠 Páginas Importantes (68-82h)

9. **Messages Center** (16-18h) - Chat UI completo
10. **Audit Logs Viewer** (8-10h)
    11-13. **Settings Pages** (36-42h) - User/Dealer/Admin

#### 🟡 Nice to Have (38-44h)

14. **Reports Builder** (20-24h)
15. **Marketing Campaigns** (18-20h)

#### Componentes Compartidos (32 componentes, 75-95h)

- NotificationBell, NotificationList, NotificationCard
- ChatWindow, MessageBubble, ConversationList
- PropertyCard, PropertyImageGallery, LocationPicker
- RoleCard, PermissionsGrid, JobCard
- SystemHealthCard, ServiceStatusBadge
- TransactionCard, RevenueChart, CalendarWidget
- AuditLogCard, ReportPreview, DataTable (mejorar)
- StatCard, EmptyState, LoadingState, ErrorState
- ConfirmDialog, Drawer

**Total UI Frontend:** 299-361 horas

---

### 📚 Plan de Integración Frontend-Backend (12 Sprints)

> **📍 UBICACIÓN:** Plan completo en `docs/sprints/frontend-backend-integration/`  
> **📊 RESUMEN:** 12 sprints, ~261K tokens, 47-56 horas de implementación  
> **🎯 OBJETIVO:** Integrar frontend React 19 con backend .NET 8 microservicios

---

#### 📋 ÍNDICE DE SPRINTS

Todos los sprints están documentados en detalle en: `docs/sprints/frontend-backend-integration/`

| Sprint        | Documento                    | Tokens | Horas | Prioridad  | Estado       |
| ------------- | ---------------------------- | ------ | ----- | ---------- | ------------ |
| **Sprint 0**  | SPRINT_0_SETUP_INICIAL.md    | ~18K   | 2-3h  | 🔴 CRÍTICO | ⚪ Pendiente |
| **Sprint 1**  | SPRINT_1_CUENTAS_TERCEROS.md | ~22K   | 3-4h  | 🔴 CRÍTICO | ⚪ Pendiente |
| **Sprint 2**  | SPRINT_2_AUTH_INTEGRATION.md | ~25K   | 4-5h  | 🔴 CRÍTICO | ⚪ Pendiente |
| **Sprint 3**  | SPRINT_3_VEHICLE_SERVICE.md  | ~30K   | 5-6h  | 🟠 Alta    | ⚪ Pendiente |
| **Sprint 4**  | SPRINT_4_MEDIA_UPLOAD.md     | ~27K   | 4-5h  | 🟠 Alta    | ⚪ Pendiente |
| **Sprint 5**  | SPRINT_5_BILLING_PAYMENTS.md | ~30K   | 5-6h  | 🟠 Alta    | ⚪ Pendiente |
| **Sprint 6**  | SPRINT_6_NOTIFICATIONS.md    | ~25K   | 3-4h  | 🟡 Media   | ⚪ Pendiente |
| **Sprint 7**  | SPRINT_7_MESSAGING_CRM.md    | ~20K   | 3-4h  | 🟡 Media   | ⚪ Pendiente |
| **Sprint 8**  | SPRINT_8_SEARCH_FILTERS.md   | ~24K   | 4-5h  | 🟡 Media   | ⚪ Pendiente |
| **Sprint 9**  | SPRINT_9_SAVED_SEARCHES.md   | ~18K   | 2-3h  | 🟢 Baja    | ⚪ Pendiente |
| **Sprint 10** | SPRINT_10_ADMIN_PANEL.md     | ~22K   | 4h    | 🟡 Media   | ⚪ Pendiente |
| **Sprint 11** | SPRINT_11_TESTING_QA.md      | ~20K   | 3-4h  | 🟠 Alta    | ⚪ Pendiente |

**Total:** ~261K tokens | 47-56 horas + **28-36h adicionales (assets + catálogo)** = **75-92h total**

---

#### 🚨 TAREAS CRÍTICAS ADICIONALES

**⚠️ Estas tareas son BLOQUEANTES para producción y DEBEN ejecutarse:**

1. **Sprint 0 - Migración de Assets (16-20h)** 🔴 CRÍTICO

   - Frontend usa ~200-300 imágenes/videos de URLs externas (Unsplash, placeholders)
   - **Sin esto:** Producción tendrá broken images, dependencia de servicios externos
   - **Hacer:** Auditar URLs, descargar assets, optimizar, subir a MediaService, actualizar frontend
   - **Resultado:** Frontend 100% self-sufficient, sin dependencias externas

2. **Sprint 3 - Seed Catálogo de Vehículos (12-16h)** 🔴 CRÍTICO
   - Frontend necesita catálogo completo (marcas, modelos, años, specs) como CarGurus
   - **Sin esto:** Páginas de búsqueda/filtros vacías, dropdowns sin datos
   - **Hacer:** Diseñar tablas (makes, models, trims, specs), obtener data (NHTSA API/Kaggle), seed DB
   - **Resultado:** ~50-100 marcas, ~500-1000 modelos, filtros funcionan con datos reales

---

#### 📖 DOCUMENTOS PRINCIPALES

**Guías de referencia en `docs/sprints/frontend-backend-integration/`:**

1. **README.md** - Cómo usar los documentos de sprints
2. **PLAN_MAESTRO_INTEGRACION.md** - Visión general y arquitectura
3. **PROGRESS_TRACKER.md** - Dashboard de seguimiento de progreso
4. **RESUMEN_FINAL.md** - Resumen ejecutivo completo (este doc completo incluye todo el plan)

---

#### 🎯 OBJETIVOS POR FASE

##### Fase 0: Setup (2 sprints, ~40K tokens, 5-7h) 🔴 CRÍTICO

**Sprint 0: Setup Inicial**

- Variables de entorno (.env)
- CORS y configuración JWT
- Docker Compose ajustes
- Testing del stack completo

**Sprint 1: Cuentas de Terceros**

- Google OAuth2 (Cloud Console)
- Firebase (Push notifications)
- Stripe (Pagos)
- SendGrid (Email)
- Twilio (SMS)
- AWS S3 (Almacenamiento)
- Sentry (Error tracking)

##### Fase 1: Core Features (4 sprints, ~112K tokens, 21-25h) 🔴 CRÍTICO

**Sprint 2: Auth Integration**

- Zustand auth store
- JWT refresh automático
- OAuth2 Google login
- PrivateRoute component
- User profile integration

**Sprint 3: Vehicle Service**

- Backend: CRUD completo de vehículos
- Frontend: TanStack Query hooks
- Filtros avanzados
- Paginación y ordenamiento
- Custom fields JSON

**Sprint 4: Media Upload**

- S3 storage integration
- ImageSharp backend processing
- Drag & drop frontend
- Preview y progress bars
- Optimización de imágenes

**Sprint 5: Billing & Payments**

- Stripe integration completa
- 3 planes de suscripción
- Webhooks para eventos
- Gestión de payment methods
- Invoicing history

##### Fase 2: Notificaciones (2 sprints, ~45K tokens, 7-8h) 🟠 ALTA

**Sprint 6: Notifications**

- Email con SendGrid
- SMS con Twilio
- Push con Firebase Cloud Messaging
- NotificationCenter UI component
- Preferencias de usuario

**Sprint 7: Messaging & CRM**

- SignalR Hub para chat real-time
- ChatHub backend
- ChatWindow frontend component
- MessageList con paginación
- CRM básico para leads

##### Fase 3: Avanzadas (2 sprints, ~42K tokens, 7-8h) 🟡 MEDIA

**Sprint 8: Search & Filters**

- Elasticsearch integration
- Full-text search
- FilterSidebar component avanzado
- Geolocalización con Google Maps
- Autocompletado de búsquedas

**Sprint 9: Saved Searches**

- SavedSearch entity
- Email alerts para nuevos matches
- Hangfire background jobs
- SavedSearchCard component
- Toggle de alertas activas

##### Fase 4: Admin & QA (2 sprints, ~42K tokens, 7-8h) 🟢 MEDIA-CRÍTICA

**Sprint 10: Admin Panel**

- AdminService backend
- Verificación de dealers
- Aprobación de vehículos
- Moderación de contenido
- Dashboard de métricas

**Sprint 11: Testing & QA**

- Unit tests (xUnit + Jest)
- Integration tests (Testcontainers)
- E2E tests (Playwright)
- Cobertura >= 80% backend
- Cobertura >= 70% frontend
- CI/CD pipeline

---

#### 🔢 ORDEN DE EJECUCIÓN

**⚠️ IMPORTANTE:** Los sprints DEBEN ejecutarse secuencialmente. Cada uno depende de los anteriores.

```
Sprint 0 → Sprint 1 → Sprint 2 → Sprint 3 → Sprint 4 → Sprint 5
    ↓         ↓          ↓          ↓          ↓          ↓
 (Setup)  (Cuentas)   (Auth)   (Vehicles)  (Media)  (Billing)
                                                          ↓
Sprint 6 → Sprint 7 → Sprint 8 → Sprint 9 → Sprint 10 → Sprint 11
    ↓          ↓          ↓          ↓          ↓           ↓
(Notif.)   (Chat)    (Search)  (Saved)    (Admin)     (Testing)
```

**Dependencias críticas:**

- Sprint 2 requiere Sprint 0, 1 (API keys)
- Sprint 3 requiere Sprint 2 (autenticación)
- Sprint 4 requiere Sprint 3 (upload de imágenes de vehículos)
- Sprint 5 requiere Sprint 3 (subscripciones por dealer)
- Sprint 6-11 requieren Sprint 2-5 (funcionalidad core)

---

#### 🌿 FLUJO DE TRABAJO GIT POR SPRINT

**⚠️ REGLA OBLIGATORIA:** Cada sprint se desarrolla en su propia rama (branch) para mantener `main` estable.

**Proceso:**

1. **Crear branch por sprint:**

   ```bash
   git checkout main
   git pull origin main
   git checkout -b sprint-0-setup-inicial
   # O cualquier sprint: sprint-1-cuentas-terceros, sprint-2-auth-integration, etc.
   ```

2. **Desarrollo del sprint:**

   - Hacer commits incrementales durante el desarrollo
   - Probar cada feature antes de commit
   - Mantener commits pequeños y atómicos

3. **Testing completo:**

   - Validar que TODAS las features del sprint funcionen
   - Ejecutar tests unitarios e integración
   - Verificar health checks de servicios
   - Probar endpoints en Postman/cURL
   - Validar frontend integrado (si aplica)

4. **Merge a main solo cuando TODO funcione:**

   ```bash
   # Asegurarse que main esté actualizado
   git checkout main
   git pull origin main

   # Merge del sprint
   git merge sprint-0-setup-inicial --no-ff

   # Push a remoto
   git push origin main

   # Eliminar branch local (opcional)
   git branch -d sprint-0-setup-inicial
   ```

**Convención de nombres de branches:**

| Sprint    | Nombre de Branch            |
| --------- | --------------------------- |
| Sprint 0  | `sprint-0-setup-inicial`    |
| Sprint 1  | `sprint-1-cuentas-terceros` |
| Sprint 2  | `sprint-2-auth-integration` |
| Sprint 3  | `sprint-3-vehicle-service`  |
| Sprint 4  | `sprint-4-media-upload`     |
| Sprint 5  | `sprint-5-billing-payments` |
| Sprint 6  | `sprint-6-notifications`    |
| Sprint 7  | `sprint-7-messaging-crm`    |
| Sprint 8  | `sprint-8-search-filters`   |
| Sprint 9  | `sprint-9-saved-searches`   |
| Sprint 10 | `sprint-10-admin-panel`     |
| Sprint 11 | `sprint-11-testing-qa`      |

**Ventajas de este flujo:**

- ✅ `main` siempre está estable y deployable
- ✅ Permite rollback fácil si algo falla
- ✅ Facilita revisión de código por sprint
- ✅ Commits más organizados y trazables
- ✅ Posibilidad de trabajar en múltiples sprints en paralelo (si es necesario)

**Comandos útiles:**

```bash
# Ver rama actual
git branch --show-current

# Ver todas las ramas
git branch -a

# Cambiar de rama
git checkout sprint-X-nombre

# Ver diferencias entre rama y main
git diff main..sprint-X-nombre

# Ver estado del merge
git log --oneline --graph --all

# Si necesitas deshacer un merge (ANTES de push)
git reset --hard HEAD~1
```

---

#### 💰 COSTOS ESTIMADOS (Mensual)

| Servicio      | Tier                     | Costo         |
| ------------- | ------------------------ | ------------- |
| AWS S3        | 50GB + 10K requests      | ~$2           |
| Stripe        | 2.9% + $0.30/transacción | Variable      |
| SendGrid      | 100 emails/día           | Free          |
| Twilio        | Pay-as-you-go            | ~$1/100 SMS   |
| Firebase      | Spark plan               | Free          |
| Google Maps   | $200 crédito/mes         | Free-$10      |
| Sentry        | 5K eventos/mes           | Free          |
| Elasticsearch | Self-hosted (Docker)     | Free          |
| **TOTAL**     | -                        | **$5-15/mes** |

---

#### 🚀 CÓMO EMPEZAR

**1. Leer documentación base:**

```bash
# Abrir estos 3 archivos primero
docs/sprints/frontend-backend-integration/README.md
docs/sprints/frontend-backend-integration/PLAN_MAESTRO_INTEGRACION.md
docs/sprints/frontend-backend-integration/PROGRESS_TRACKER.md
```

**2. Iniciar Sprint 0:**

```bash
# Abrir y seguir paso a paso
docs/sprints/frontend-backend-integration/SPRINT_0_SETUP_INICIAL.md
```

**3. Avisar a GitHub Copilot:**

```
"Quiero empezar el Sprint 0 - Setup Inicial del plan de integración"
```

**4. Al completar cada sprint:**

```
"Completé el Sprint X, quiero continuar con el Sprint Y"
```

---

#### 📊 SEGUIMIENTO DE PROGRESO

Para ver el estado actual y actualizar el progreso, consultar:

```
docs/sprints/frontend-backend-integration/PROGRESS_TRACKER.md
```

Este archivo mantiene un dashboard visual con:

- Estado de cada sprint (⚪ Pendiente, 🟡 En progreso, ✅ Completo)
- Checklist de tareas por sprint
- Porcentaje de completitud
- Estimaciones vs tiempo real

---

#### ✅ CRITERIOS DE ÉXITO

Al completar los 12 sprints, el proyecto debe tener:

- ✅ Autenticación completa (JWT + OAuth2)
- ✅ CRUD de vehículos con imágenes
- ✅ Sistema de pagos funcional
- ✅ Notificaciones multi-canal
- ✅ Chat en tiempo real
- ✅ Búsqueda avanzada con Elasticsearch
- ✅ Panel de administración
- ✅ Tests >= 80% cobertura backend
- ✅ Tests >= 70% cobertura frontend
- ✅ E2E tests de flujos críticos
- ✅ CI/CD pipeline operacional

---

#### 📝 NOTAS IMPORTANTES

1. **NO saltar sprints** - Las dependencias causan errores
2. **Leer documento completo** antes de empezar cada sprint
3. **Actualizar PROGRESS_TRACKER.md** al terminar tareas
4. **Validar con tests** antes de marcar completo
5. **Consultar Troubleshooting** en cada documento si hay problemas

Para más detalles, ver: `docs/sprints/frontend-backend-integration/RESUMEN_FINAL.md`

---

### ⚠️ REGLAS CRÍTICAS PARA CODIFICACIÓN

#### 1. NO Crear Nuevos Microservicios

```csharp
// ❌ NUNCA hacer esto
public class VehicleReviewService { }  // Ya existe ProductService

// ✅ SIEMPRE hacer esto
// Extender ProductService.Application/Features/Reviews/
```

#### 2. SIEMPRE Usar Gateway

```typescript
// ❌ NUNCA llamar directamente
const response = await fetch("http://authservice:15085/api/auth/login");

// ✅ SIEMPRE via Gateway
const response = await fetch("http://localhost:18443/api/auth/login");
```

#### 3. Verificar Servicio Existe ANTES de Codificar

```bash
# ANTES de crear endpoint para favorites:
# 1. Verificar ProductService existe y funciona
curl http://localhost:15006/health

# 2. Verificar ruta en Gateway
# Ver: backend/Gateway/Gateway.Api/ocelot.dev.json

# 3. Solo entonces crear nuevo endpoint
```

#### 4. Consultar Análisis Técnico

Antes de cualquier integración, consultar:

- `docs/analysis/EXECUTIVE_SUMMARY.md` - Decisiones estratégicas
- `docs/analysis/SECCION_2_BACKEND_ACTUAL.md` - Endpoints disponibles
- `docs/analysis/SECCION_5_FEATURES_AGREGAR.md` - Specs de nuevos endpoints
- `docs/analysis/SECCION_6_VISTAS_FALTANTES.md` - UI components a crear

#### 5. Frontend: Eliminar Mocks SOLO Cuando Backend Esté Listo

```typescript
// ❌ NO eliminar mocks prematuramente
// const vehicles = mockVehicles; // Eliminar sin backend = app rota

// ✅ Verificar backend primero
const healthCheck = await fetch("http://localhost:15006/health");
if (healthCheck.ok) {
  // AHORA sí reemplazar
  const vehicles = await vehicleService.getAll();
}
```

#### 6. Testing Obligatorio

```csharp
// Cada nuevo endpoint DEBE tener:
// 1. Unit test del handler
[Fact]
public async Task Handle_AddToFavorites_Success() { }

// 2. Integration test de la API
[Fact]
public async Task POST_Favorites_Returns201() { }
```

---

### Quick Reference: ¿Qué Servicio Usar?

| Feature Frontend | Servicio Backend    | Puerto | Estado                     |
| ---------------- | ------------------- | ------ | -------------------------- |
| Login/Register   | AuthService         | 15085  | ✅ Funcional               |
| User Profile     | UserService         | 15100  | ⚠️ Parcial                 |
| Vehicle CRUD     | ProductService      | 15006  | ⚠️ Parcial                 |
| Favorites        | ProductService      | 15006  | ❌ Crear endpoint (4-6h)   |
| Comparison       | ProductService      | 15006  | ❌ Crear endpoint (6-8h)   |
| Reviews          | ProductService      | 15006  | ❌ Crear endpoint (12-16h) |
| Notifications    | NotificationService | 15084  | ⚠️ Falta SignalR (24-30h)  |
| Real Estate      | RealEstateService   | 15034  | ❌ NO consumido            |
| CRM              | CRMService          | 15009  | ❌ NO consumido            |
| Analytics        | ReportsService      | 15010  | ❌ NO consumido            |
| Invoices         | InvoicingService    | 15031  | ❌ NO consumido            |
| Payments         | BillingService      | 15008  | ✅ Funcional               |
| Appointments     | AppointmentService  | 15032  | ❌ Sin UI (16-20h)         |
| Jobs             | SchedulerService    | 15012  | ❌ Sin UI (12-14h)         |
| Roles            | RoleService         | 15101  | ❌ Sin UI (14-16h)         |
| Contact          | ContactService      | 15030  | ❌ NO consumido            |
| Media Upload     | MediaService        | 15090  | ⚠️ Mejorar (18-22h)        |

---

### Documentación Técnica Completa

**Análisis en `docs/analysis/`:**

1. **EXECUTIVE_SUMMARY.md** - 2 páginas para stakeholders
2. **ANALISIS_FRONTEND_BACKEND_INDEX.md** - Índice master
3. **SECCION_1_FRONTEND_ACTUAL.md** - 59 páginas inventariadas
4. **SECCION_2_BACKEND_ACTUAL.md** - 35 microservicios detallados
5. **SECCION_3_GAP_ANALYSIS.md** - 47 gaps identificados
6. **SECCION_4_MICROSERVICIOS_NUEVOS.md** - 0 necesarios (extender)
7. **SECCION_5_FEATURES_AGREGAR.md** - 48 endpoints especificados
8. **SECCION_6_VISTAS_FALTANTES.md** - 15 páginas + 32 componentes
9. **SECCION_7_PLAN_ACCION.md** - Roadmap de 12 sprints

**Sprints en `docs/sprints/frontend-backend-integration/`:**

1. **README.md** - Guía de uso de los sprints
2. **PLAN_MAESTRO_INTEGRACION.md** - Arquitectura y visión general
3. **PROGRESS_TRACKER.md** - Dashboard de seguimiento
4. **RESUMEN_FINAL.md** - Resumen ejecutivo completo
5. **SPRINT_0_SETUP_INICIAL.md** - Variables de entorno y configuración
6. **SPRINT_1_CUENTAS_TERCEROS.md** - Google, Firebase, Stripe, etc.
7. **SPRINT_2_AUTH_INTEGRATION.md** - JWT, OAuth2, Zustand
8. **SPRINT_3_VEHICLE_SERVICE.md** - CRUD vehículos
9. **SPRINT_4_MEDIA_UPLOAD.md** - S3, ImageSharp
10. **SPRINT_5_BILLING_PAYMENTS.md** - Stripe subscriptions
11. **SPRINT_6_NOTIFICATIONS.md** - Email, SMS, Push
12. **SPRINT_7_MESSAGING_CRM.md** - SignalR chat
13. **SPRINT_8_SEARCH_FILTERS.md** - Elasticsearch
14. **SPRINT_9_SAVED_SEARCHES.md** - Email alerts
15. **SPRINT_10_ADMIN_PANEL.md** - Admin dashboard
16. **SPRINT_11_TESTING_QA.md** - Tests y CI/CD

**Total:** 12 sprints, ~261K tokens, 47-56 horas

---

## 🤖 CONFIGURACIÓN DEL MODELO AI Y ESTIMACIÓN DE TOKENS

### Parámetros del Modelo

| Parámetro                    | Valor                                                       |
| ---------------------------- | ----------------------------------------------------------- |
| **Modelo**                   | Claude Opus 4.5                                             |
| **Context Window (Input)**   | 128,000 tokens                                              |
| **Max Output**               | 16,000 tokens                                               |
| **Multiplier**               | 1x                                                          |
| **Tokens Útiles por Sesión** | ~110,000 tokens (reservando 18k para sistema/instrucciones) |

### ⚠️ REGLA OBLIGATORIA: Estimación Antes de Ejecutar

**ANTES de ejecutar cualquier tarea, SIEMPRE debes:**

1. **Estimar tokens de entrada** (código a leer + contexto)
2. **Estimar tokens de salida** (código a generar)
3. **Determinar si cabe en una sesión** (total < 110,000 input + 16,000 output)
4. **Dividir en subtareas si excede los límites**

### Fórmulas de Estimación

```
Tokens de Lectura = (Líneas de código × 4) + (Archivos × 500)
Tokens de Escritura = (Líneas nuevas/modificadas × 5)
Tokens de Contexto = Instrucciones + Historial (~8,000 base)
Buffer de Seguridad = 15%

Total Estimado = (Lectura + Escritura + Contexto) × 1.15
```

### Factores de Complejidad

| Nivel        | Multiplicador | Descripción                         |
| ------------ | ------------- | ----------------------------------- |
| Simple       | 1.0x          | Cambios menores, archivos pequeños  |
| Medio        | 1.3x          | Múltiples archivos, lógica moderada |
| Complejo     | 1.6x          | Refactoring, nuevos patterns        |
| Muy Complejo | 2.0x          | Arquitectura, múltiples sistemas    |

### Tabla de Referencia Rápida

| Tipo de Tarea                       | Tokens Est. | ¿Cabe en 1 sesión?          |
| ----------------------------------- | ----------- | --------------------------- |
| Actualizar 1 paquete                | ~5,000      | ✅ Sí                       |
| Crear 1 archivo nuevo (~100 líneas) | ~8,000      | ✅ Sí                       |
| Modificar 3-5 archivos relacionados | ~15,000     | ✅ Sí                       |
| Crear feature CQRS completa         | ~25,000     | ✅ Sí                       |
| Refactoring de módulo completo      | ~45,000     | ✅ Sí (límite)              |
| Breaking change (ej: Firebase 2→3)  | ~85,000     | ⚠️ Dividir en 4-5 subtareas |
| Nuevo microservicio completo        | ~120,000    | 🔴 Dividir en 6-8 subtareas |

### Proceso de División de Tareas

Si una tarea excede **80,000 tokens** (margen de seguridad), dividir así:

```
Tarea Grande (120k tokens)
├── Subtarea 1: Estructura base (~20k)
├── Subtarea 2: Entities y Models (~18k)
├── Subtarea 3: Repositories (~18k)
├── Subtarea 4: Use Cases (~20k)
├── Subtarea 5: Controllers/API (~22k)
└── Subtarea 6: Tests (~22k)
```

### Ejemplo de Estimación

```markdown
## Tarea: Implementar VehicleRemoteDataSource

**Estimación:**

- Archivos a leer: 5 (~400 líneas)
- Archivos a crear/modificar: 3 (~200 líneas)
- Complejidad: Medio (1.3x)

**Cálculo:**
Lectura: 400 × 4 + 5 × 500 = 4,100 tokens
Escritura: 200 × 5 = 1,000 tokens
Contexto: 8,000 tokens
Total: (4,100 + 1,000 + 8,000) × 1.15 × 1.3 = ~19,600 tokens

**Decisión:** ✅ Cabe en 1 sesión
```

### Planes de Sprint Disponibles

Los planes detallados con estimaciones de tokens están en:

| Documento                             | Tokens Totales | Sesiones Est. |
| ------------------------------------- | -------------- | ------------- |
| `BACKEND_IMPROVEMENT_SPRINT_PLAN.md`  | ~482,000       | 25-26         |
| `FRONTEND_IMPROVEMENT_SPRINT_PLAN.md` | ~371,300       | 20            |
| `MOBILE_IMPROVEMENT_SPRINT_PLAN.md`   | ~463,000       | 25-30         |

---

## 🏗️ ESTRUCTURA DEL PROYECTO

> **Estado Docker (31 Dic 2025):** ✅ **Todos los 35 microservicios tienen Dockerfile Y están en docker-compose.yml**  
> **✅ AuthService FUNCIONAL:** Dockerfile.dev corregido (dotnet build + dotnet run), variables de entorno Database\_\_\* configuradas  
> **✅ Credenciales de Prueba:** `test@example.com` / `Admin123!` (email confirmado, login funcional)

```
cardealer-microservices/
├── backend/                          # Microservicios .NET 8 (35 servicios)
│   ├── _Shared/                      # Librerías compartidas
│   │   ├── CarDealer.Contracts/      # DTOs y Events para comunicación
│   │   └── CarDealer.Shared/         # Utilidades y Multi-tenancy
│   ├── _Tests/IntegrationTests/      # Tests de integración
│   ├── Gateway/                      # API Gateway (Ocelot) ✅
│   ├── ServiceDiscovery/             # Consul integration ✅
│   ├── AuthService/                  # Autenticación y autorización ✅🟢 FUNCIONAL
│   ├── UserService/                  # Gestión de usuarios ✅
│   ├── RoleService/                  # Gestión de roles y permisos ✅
│   ├── ProductService/               # Productos genéricos marketplace ✅
│   ├── MediaService/                 # Gestión de archivos multimedia ✅
│   ├── NotificationService/          # Email, SMS, Push notifications ✅
│   ├── BillingService/               # Facturación y pagos ✅
│   ├── CRMService/                   # Gestión de clientes ✅
│   ├── ErrorService/                 # Centralización de errores ✅
│   ├── AuditService/                 # Auditoría y compliance ✅
│   ├── CacheService/                 # Cache distribuido ✅
│   ├── MessageBusService/            # RabbitMQ abstraction ✅
│   ├── SchedulerService/             # Jobs con Hangfire ✅
│   ├── SearchService/                # Búsqueda (Elasticsearch) ✅
│   ├── ReportsService/               # Reportes y analytics ✅
│   ├── HealthCheckService/           # Health monitoring ✅
│   ├── LoggingService/               # Logging centralizado ✅
│   ├── TracingService/               # Distributed tracing ✅
│   ├── ConfigurationService/         # Configuración dinámica ✅
│   ├── FeatureToggleService/         # Feature flags ✅
│   ├── FileStorageService/           # S3/Azure Blob storage ✅
│   ├── BackupDRService/              # Backup y Disaster Recovery ✅
│   ├── MarketingService/             # Campañas marketing ✅
│   ├── IntegrationService/           # Integraciones externas ✅
│   ├── FinanceService/               # Finanzas y contabilidad ✅
│   ├── InvoicingService/             # Facturación electrónica ✅
│   ├── ContactService/               # Gestión de contactos ✅
│   ├── AppointmentService/           # Citas y agenda ✅
│   ├── AdminService/                 # Panel de administración ✅
│   ├── ApiDocsService/               # Documentación API ✅
│   ├── RateLimitingService/          # Rate limiting ✅
│   ├── IdempotencyService/           # Idempotencia ✅
│   ├── RealEstateService/            # Vertical inmobiliario ✅
│   ├── observability/                # Configs OpenTelemetry
│   └── monitoring/                   # Prometheus/Grafana configs
│   # ✅ = En docker-compose.yml (35/35 servicios) | 🟢 = Probado y funcional
│
├── frontend/
│   ├── web/                          # React 19 + Vite + TailwindCSS
│   │   ├── src/
│   │   ├── cardealer/                # App CarDealer
│   │   └── okla/                     # App alternativa OKLA
│   ├── mobile/cardealer/             # Flutter app
│   │   ├── lib/
│   │   │   ├── core/                 # Core utilities
│   │   │   ├── data/                 # Data layer (repos, datasources)
│   │   │   ├── domain/               # Domain layer (entities, usecases)
│   │   │   └── presentation/         # UI (pages, widgets, blocs)
│   │   ├── android/
│   │   └── ios/
│   └── shared/                       # Componentes compartidos
│
├── policies/                         # Políticas de seguridad
├── scripts/                          # Scripts de utilidad
├── compose.yaml                      # Docker Compose principal
└── cardealer.sln                     # Solución .NET
```

---

## 🎯 ARQUITECTURA POR MICROSERVICIO

Cada microservicio sigue **Clean Architecture**:

```
{ServiceName}/
├── {ServiceName}.Api/                # Capa de presentación
│   ├── Controllers/                  # REST Controllers
│   ├── Middleware/                   # Custom middleware
│   ├── Program.cs                    # Entry point
│   └── appsettings.json
├── {ServiceName}.Application/        # Capa de aplicación
│   ├── Features/                     # CQRS con MediatR
│   │   ├── Commands/
│   │   └── Queries/
│   ├── DTOs/
│   ├── Validators/                   # FluentValidation
│   └── Common/Behaviours/            # Pipeline behaviors
├── {ServiceName}.Domain/             # Capa de dominio
│   ├── Entities/                     # Entidades de dominio
│   ├── ValueObjects/
│   ├── Events/                       # Domain events
│   ├── Interfaces/
│   ├── Enums/
│   └── Exceptions/
├── {ServiceName}.Infrastructure/     # Capa de infraestructura
│   ├── Persistence/                  # DbContext, Repositories
│   ├── Services/                     # Implementaciones externas
│   ├── Messaging/                    # RabbitMQ publishers/consumers
│   └── Extensions/                   # DI extensions
├── {ServiceName}.Shared/             # DTOs compartidos (opcional)
├── {ServiceName}.Tests/              # Unit tests
├── Dockerfile
└── {ServiceName}.sln
```

---

## 📦 PATRONES Y CONVENCIONES

### 1. CQRS con MediatR

```csharp
// Command
public record CreateUserCommand(string Email, string Password) : IRequest<Result<UserDto>>;

// Handler
public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<UserDto>>
{
    public async Task<Result<UserDto>> Handle(CreateUserCommand request, CancellationToken ct)
    {
        // Implementación
    }
}
```

### 2. Domain Events (Event-Driven)

```csharp
// Definir evento en CarDealer.Contracts
public class UserRegisteredEvent : EventBase
{
    public override string EventType => "auth.user.registered";
    public Guid UserId { get; set; }
    public string Email { get; set; }
}

// Publicar via RabbitMQ
await _eventPublisher.PublishAsync(new UserRegisteredEvent { ... });
```

### 3. Multi-Tenancy

Todas las entidades multi-tenant implementan `ITenantEntity`:

```csharp
public class Product : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid DealerId { get; set; }  // Tenant ID
    // ...
}
```

### 4. Result Pattern

```csharp
public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }

    public static Result<T> Success(T value) => new(value, null, true);
    public static Result<T> Failure(string error) => new(default, error, false);
}
```

### 5. Repository Pattern

```csharp
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);
    Task<T> AddAsync(T entity, CancellationToken ct = default);
    Task UpdateAsync(T entity, CancellationToken ct = default);
    Task DeleteAsync(T entity, CancellationToken ct = default);
}
```

---

## 🔐 AUTENTICACIÓN Y AUTORIZACIÓN

- **JWT Bearer Tokens** con refresh tokens
- **ASP.NET Core Identity** para gestión de usuarios
- **2FA** con TOTP (Otp.NET + QRCoder)
- **OAuth2** con Google y Microsoft
- **Rate Limiting** por IP y usuario
- **CORS** configurado por entorno

### Claims estándar

```csharp
public static class ClaimTypes
{
    public const string UserId = "sub";
    public const string Email = "email";
    public const string DealerId = "dealer_id";    // Tenant
    public const string Role = "role";
    public const string Permissions = "permissions";
}
```

---

## 📡 COMUNICACIÓN ENTRE SERVICIOS

### ❌ NO hacer (Anti-pattern)

```csharp
// NUNCA llamar directamente entre servicios
var response = await _httpClient.GetAsync("http://authservice/api/users/123");
```

### ✅ SÍ hacer

```csharp
// 1. Via Gateway (para clientes externos)
// Cliente → Gateway → Servicio

// 2. Via RabbitMQ (entre servicios)
await _eventPublisher.PublishAsync(new UserRegisteredEvent { UserId = user.Id });
```

### Exchanges y Queues (RabbitMQ)

| Exchange             | Tipo   | Descripción          |
| -------------------- | ------ | -------------------- |
| `cardealer.events`   | topic  | Eventos de dominio   |
| `cardealer.commands` | direct | Comandos directos    |
| `cardealer.dlx`      | fanout | Dead Letter Exchange |

---

## 🗄️ BASE DE DATOS

### Configuración Multi-Provider

```json
// appsettings.json
{
  "Database": {
    "Provider": "PostgreSQL", // PostgreSQL, SqlServer, Oracle
    "Host": "localhost",
    "Port": 5432,
    "Database": "authservice",
    "Username": "postgres",
    "Password": "password",
    "AutoMigrate": true
  }
}
```

```csharp
// Program.cs
builder.Services.AddDatabaseProvider<ApplicationDbContext>(builder.Configuration);
```

### Migraciones

```powershell
# Crear migración
dotnet ef migrations add InitialCreate -p AuthService.Infrastructure -s AuthService.Api

# Aplicar migración
dotnet ef database update -p AuthService.Infrastructure -s AuthService.Api
```

---

## 🔄 PROCESO DE COMPILACIÓN Y TESTING DE MICROSERVICIOS

### ⚠️ FLUJO OBLIGATORIO - Orden de Ejecución

**SIEMPRE seguir este proceso en orden para cada microservicio:**

### 1️⃣ COMPILACIÓN LOCAL (Capa por Capa)

**ANTES de crear o probar contenedores Docker, SIEMPRE compilar localmente:**

```powershell
# Navegar al servicio
cd backend/{ServiceName}

# Compilar cada capa en orden de dependencias
dotnet build {ServiceName}.Domain/{ServiceName}.Domain.csproj
dotnet build {ServiceName}.Application/{ServiceName}.Application.csproj
dotnet build {ServiceName}.Infrastructure/{ServiceName}.Infrastructure.csproj
dotnet build {ServiceName}.Api/{ServiceName}.Api.csproj

# O compilar todo el servicio (más rápido si todas las capas están OK)
dotnet build {ServiceName}.sln
```

**✅ Validación Exitosa:**

- `Build succeeded. 0 Error(s)`
- **Solo si NO hay errores**, proceder al siguiente paso

**❌ Si hay errores:**

- **NO crear Docker images**
- **NO levantar contenedores**
- Corregir errores en el código primero
- Repetir compilación local hasta 0 errores

### 2️⃣ CREACIÓN DE IMAGEN DOCKER

**Solo después de compilación local exitosa:**

```powershell
# Build de imagen Docker
docker build -t cardealer-microservices-{servicename}:latest \
  -f backend/{ServiceName}/{ServiceName}.Api/Dockerfile.dev \
  backend

# Verificar imagen creada
docker images | Select-String "{servicename}"
```

### 3️⃣ PRUEBA DE COMPILACIÓN EN DOCKER

**Levantar contenedor y verificar compilación dentro de Docker:**

```powershell
# Iniciar contenedor
docker-compose up -d {servicename}

# O manualmente si no está en compose.yaml
docker run -d --name {servicename} \
  --network cardealer-microservices_cargurus-net \
  -p {port}:80 \
  -e ASPNETCORE_ENVIRONMENT=Development \
  -e "ConnectionStrings__DefaultConnection=Host={servicename}-db;..." \
  cardealer-microservices-{servicename}:latest

# Esperar compilación (60-100 segundos sin watch mode)
Start-Sleep -Seconds 90

# Verificar logs - NO debe haber errores
docker logs {servicename} --tail 50
```

**🔍 Signos de Compilación Exitosa:**

- Logs muestran: `"Now listening on: http://[::]:80"`
- No hay excepciones de tipo `System.*Exception`
- No hay errores de DI (Dependency Injection)
- No hay errores de EF Core migrations

**❌ Si hay errores en Docker:**

- Revisar logs completos: `docker logs {servicename}`
- Problema común: `dotnet watch` puede congelarse → Usar `dotnet run` en Dockerfile
- Verificar variables de entorno y conexión a DB
- Si persiste: Bajar contenedor, corregir código local, recompilar, rebuild Docker

### 4️⃣ PRUEBAS DE API (Solo cuando servicio esté UP)

**Solo después de que el contenedor esté HEALTHY:**

```powershell
# Verificar health check
Invoke-WebRequest "http://localhost:{port}/health" -UseBasicParsing

# Verificar Swagger UI
Invoke-WebRequest "http://localhost:{port}/swagger" -UseBasicParsing

# Contar endpoints disponibles
$swagger = Invoke-WebRequest "http://localhost:{port}/swagger/v1/swagger.json" -UseBasicParsing
$json = $swagger.Content | ConvertFrom-Json
$endpointCount = ($json.paths.PSObject.Properties | ForEach-Object {
  $_.Value.PSObject.Properties.Count
} | Measure-Object -Sum).Sum
Write-Host "✅ $endpointCount endpoints operacionales"

# Probar endpoint específico (ejemplo)
Invoke-WebRequest "http://localhost:{port}/api/{resource}?page=1&pageSize=5" -UseBasicParsing
```

---

### 🎯 OPTIMIZACIÓN DE RECURSOS - Gestión de Contenedores

**⚠️ IMPORTANTE:** PC con recursos limitados (~8GB RAM) - NO correr todos los contenedores simultáneamente.

#### Estrategia: Levantar Solo lo Necesario

**Antes de iniciar un test, identificar dependencias:**

```powershell
# 1. Listar todos los contenedores corriendo
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"

# 2. Identificar servicios NO necesarios para la prueba actual
# Ejemplo: Si vas a probar CRMService, NO necesitas:
# - ProductService, NotificationService, SearchService, etc.

# 3. Bajar servicios innecesarios
docker stop productservice notificationservice searchservice
# O usar docker-compose
docker-compose stop productservice notificationservice searchservice

# 4. Verificar liberación de recursos
docker stats --no-stream --format "table {{.Name}}\t{{.CPUPerc}}\t{{.MemUsage}}"
```

#### Perfiles de Contenedores por Tipo de Prueba

**A. Prueba de Servicio Individual (Mínimo):**

```powershell
# Solo levantar:
# - Servicio a probar
# - Su base de datos
# - Redis (si el servicio usa cache)
# - RabbitMQ (si usa mensajería)

# Ejemplo para CRMService:
docker-compose up -d redis rabbitmq crmservice-db crmservice

# Bajar todo lo demás:
docker-compose stop $(docker-compose ps --services | Where-Object { $_ -notmatch "redis|rabbitmq|crmservice" })
```

**B. Prueba de Integración entre 2-3 Servicios:**

```powershell
# Ejemplo: AuthService + ErrorService + Gateway
docker-compose up -d redis rabbitmq \
  authservice-db authservice \
  errorservice-db errorservice \
  gateway
```

**C. Prueba Completa del Sistema (Requiere más RAM):**

```powershell
# Solo para validación final - consume ~14GB RAM
docker-compose up -d
```

#### Limpieza Post-Prueba

```powershell
# Bajar servicios probados (libera RAM inmediatamente)
docker-compose stop {servicename}

# Ver recursos liberados
docker stats --no-stream

# Limpiar contenedores detenidos (opcional)
docker container prune -f

# Limpiar imágenes antiguas (opcional)
docker image prune -f
```

---

### 🐛 TROUBLESHOOTING - Problemas Comunes

| Problema                            | Causa                             | Solución                                         |
| ----------------------------------- | --------------------------------- | ------------------------------------------------ |
| Compilación local OK, Docker FAIL   | `dotnet watch` se congela         | Cambiar ENTRYPOINT a `dotnet run` en Dockerfile  |
| "Column does not exist" en API      | Migraciones EF desincronizadas    | Regenerar migraciones desde cero                 |
| Servicio no responde después de 60s | Aún compilando o error silencioso | Esperar 30s más o revisar `docker logs`          |
| `IHttpClientFactory` no registrado  | Falta `AddHttpClient()` en DI     | Agregar en Program.cs                            |
| Contenedor crashea al inicio        | Middleware requiere DB migrada    | Comentar middleware o agregar auto-migration     |
| Docker consume toda la RAM          | Demasiados contenedores activos   | Bajar servicios innecesarios (ver perfil mínimo) |

---

### ✅ CHECKLIST DE VALIDACIÓN

Antes de considerar un microservicio "listo":

- [ ] ✅ Compilación local sin errores (capa por capa)
- [ ] ✅ Imagen Docker creada exitosamente
- [ ] ✅ Contenedor inicia sin errores en logs
- [ ] ✅ Health check responde 200 OK
- [ ] ✅ Swagger UI accesible
- [ ] ✅ Endpoints cuentan correctamente (>0)
- [ ] ✅ API responde a requests (aunque sea 401 Unauthorized)
- [ ] ✅ Base de datos conectada y con tablas migradas
- [ ] ✅ No hay errores en logs después de 2 minutos de ejecución

**Solo después de cumplir TODOS los puntos, el servicio se considera operacional.**

---

## 🧪 TESTING

### Stack de Testing

| Tipo        | Framework                        |
| ----------- | -------------------------------- |
| Unit Tests  | xUnit 2.7+                       |
| Mocking     | Moq 4.20+                        |
| Assertions  | FluentAssertions 6.12+           |
| Integration | Testcontainers 3.9+              |
| API Testing | Microsoft.AspNetCore.Mvc.Testing |
| Fake Data   | Bogus 35.5+                      |

### Estructura de Tests

```csharp
public class CreateUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo;
    private readonly CreateUserCommandHandler _handler;

    [Fact]
    public async Task Handle_ValidUser_ReturnsSuccess()
    {
        // Arrange
        var command = new CreateUserCommand("test@email.com", "Password123!");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }
}
```

---

## 🐳 DOCKER

### Comandos frecuentes

```powershell
# Levantar todo el stack
docker-compose up -d

# Solo backend
docker-compose -f backend/docker-compose.yml up -d

# Con observabilidad
docker-compose -f backend/observability/docker-compose.observability.yml up -d

# Logs de un servicio
docker logs -f authservice
```

### Puertos importantes

| Servicio            | Puerto           | Estado              |
| ------------------- | ---------------- | ------------------- |
| Gateway             | 18443            | ✅                  |
| AuthService         | 15085            | ✅                  |
| ErrorService        | 15083            | ✅                  |
| NotificationService | 15084            | ✅                  |
| ProductService      | 15006            | ✅                  |
| UserService         | 15100            | ❌ Falta en compose |
| RoleService         | 15101            | ❌ Falta en compose |
| Redis               | 6379             | ✅                  |
| RabbitMQ            | 5672, 15672 (UI) | ✅                  |
| PostgreSQL          | 25432-25446      | ✅                  |
| Consul              | 8500             | ✅                  |
| Prometheus          | 9090             | ⚪                  |
| Grafana             | 3000             | ⚪                  |
| Jaeger              | 16686            | ⚪                  |

> ✅ = Configurado y funcionando | ❌ = Falta en docker-compose | ⚪ = No desplegado

### ⚠️ LÍMITES DE RECURSOS OBLIGATORIOS PARA DOCKER

**CONTEXTO:** El entorno de desarrollo tiene recursos limitados (~8GB RAM, 8 CPUs). Todos los 35 microservicios deben poder correr simultáneamente en Docker para pruebas de integración. Por lo tanto, **SIEMPRE** que se cree o modifique un servicio en `compose.yaml`, se DEBEN incluir límites de recursos.

#### Límites Estándar por Tipo de Servicio

| Tipo de Servicio             | CPU Límite | RAM Límite | RAM Reservada |
| ---------------------------- | ---------- | ---------- | ------------- |
| **PostgreSQL DB**            | 0.25       | 256M       | 128M          |
| **Redis**                    | 0.1        | 128M       | 64M           |
| **RabbitMQ**                 | 0.25       | 256M       | 128M          |
| **API .NET (Microservicio)** | 0.5        | 384M       | 256M          |
| **Gateway**                  | 0.25       | 256M       | 128M          |
| **Elasticsearch**            | 0.5        | 512M       | 256M          |
| **Consul**                   | 0.1        | 128M       | 64M           |

#### 📋 Recursos por Microservicio (35 servicios)

Basado en la complejidad del código (archivos .cs) y funcionalidad, cada servicio tiene asignados recursos específicos:

##### 🔴 Servicios CORE (Alta prioridad, más recursos)

| Servicio                | Archivos | CPU  | RAM  | RAM Res. |  Requiere DB  | Descripción                         |
| ----------------------- | :------: | :--: | :--: | :------: | :-----------: | ----------------------------------- |
| **AuthService**         |   283    | 0.5  | 384M |   256M   | ✅ PostgreSQL | Autenticación, JWT, Identity, 2FA   |
| **Gateway**             |    39    | 0.25 | 256M |   128M   |      ❌       | API Gateway con Ocelot              |
| **UserService**         |   143    | 0.5  | 384M |   256M   | ✅ PostgreSQL | Gestión de usuarios                 |
| **RoleService**         |   170    | 0.5  | 384M |   256M   | ✅ PostgreSQL | Roles y permisos                    |
| **ProductService**      |    53    | 0.5  | 384M |   256M   | ✅ PostgreSQL | Productos del marketplace           |
| **NotificationService** |   163    | 0.5  | 384M |   256M   | ✅ PostgreSQL | Email, SMS, Push (SendGrid, Twilio) |
| **ErrorService**        |   112    | 0.5  | 384M |   256M   | ✅ PostgreSQL | Centralización de errores           |

##### 🟡 Servicios SECUNDARIOS (Uso moderado)

| Servicio              | Archivos | CPU | RAM  | RAM Res. |  Requiere DB  | Descripción                   |
| --------------------- | :------: | :-: | :--: | :------: | :-----------: | ----------------------------- |
| **MediaService**      |   146    | 0.4 | 320M |   192M   | ✅ PostgreSQL | Archivos multimedia, S3/Azure |
| **AdminService**      |   119    | 0.4 | 320M |   192M   | ✅ PostgreSQL | Panel de administración       |
| **AuditService**      |    86    | 0.3 | 256M |   128M   | ✅ PostgreSQL | Auditoría y compliance        |
| **BillingService**    |    51    | 0.3 | 256M |   128M   | ✅ PostgreSQL | Facturación, Stripe           |
| **ContactService**    |    83    | 0.3 | 256M |   128M   | ✅ PostgreSQL | Gestión de contactos          |
| **CRMService**        |    40    | 0.3 | 256M |   128M   | ✅ PostgreSQL | CRM básico                    |
| **MessageBusService** |    85    | 0.3 | 256M |   128M   |      ❌       | Abstracción RabbitMQ          |

##### 🟢 Servicios LIGEROS (Poco consumo)

| Servicio                 | Archivos | CPU  | RAM  | RAM Res. |  Requiere DB  | Descripción          |
| ------------------------ | :------: | :--: | :--: | :------: | :-----------: | -------------------- |
| **ConfigurationService** |    61    | 0.2  | 192M |   96M    | ✅ PostgreSQL | Config dinámica      |
| **FeatureToggleService** |    71    | 0.2  | 192M |   96M    | ✅ PostgreSQL | Feature flags        |
| **HealthCheckService**   |    36    | 0.15 | 128M |   64M    |      ❌       | Health monitoring    |
| **LoggingService**       |    55    | 0.2  | 192M |   96M    | ✅ PostgreSQL | Logging centralizado |
| **TracingService**       |    32    | 0.15 | 128M |   64M    |      ❌       | Distributed tracing  |
| **CacheService**         |    49    | 0.15 | 128M |   64M    |      ❌       | Proxy a Redis        |
| **IdempotencyService**   |    36    | 0.15 | 128M |   64M    | ✅ PostgreSQL | Idempotencia         |
| **RateLimitingService**  |    53    | 0.2  | 192M |   96M    |      ❌       | Rate limiting        |
| **ApiDocsService**       |    25    | 0.1  | 128M |   64M    |      ❌       | Documentación API    |
| **ServiceDiscovery**     |    48    | 0.15 | 128M |   64M    |      ❌       | Integración Consul   |

##### 🔵 Servicios ESPECIALIZADOS

| Servicio             | Archivos | CPU | RAM  | RAM Res. |  Requiere DB  | Descripción                |
| -------------------- | :------: | :-: | :--: | :------: | :-----------: | -------------------------- |
| **SearchService**    |    50    | 0.4 | 320M |   192M   | ❌ (Elastic)  | Búsqueda con Elasticsearch |
| **SchedulerService** |    54    | 0.3 | 256M |   128M   | ✅ PostgreSQL | Jobs con Hangfire          |
| **BackupDRService**  |    76    | 0.3 | 256M |   128M   | ✅ PostgreSQL | Backup y Disaster Recovery |
| **ReportsService**   |    36    | 0.3 | 256M |   128M   | ✅ PostgreSQL | Reportes y analytics       |

##### ⚪ Servicios VERTICALES/NEGOCIO

| Servicio               | Archivos | CPU  | RAM  | RAM Res. |  Requiere DB  | Descripción             |
| ---------------------- | :------: | :--: | :--: | :------: | :-----------: | ----------------------- |
| **RealEstateService**  |    25    | 0.25 | 192M |   96M    | ✅ PostgreSQL | Vertical inmobiliario   |
| **FinanceService**     |    42    | 0.25 | 192M |   96M    | ✅ PostgreSQL | Finanzas y contabilidad |
| **InvoicingService**   |    43    | 0.25 | 192M |   96M    | ✅ PostgreSQL | Facturación electrónica |
| **AppointmentService** |    30    | 0.2  | 192M |   96M    | ✅ PostgreSQL | Citas y agenda          |
| **MarketingService**   |    36    | 0.2  | 192M |   96M    | ✅ PostgreSQL | Campañas marketing      |
| **IntegrationService** |    36    | 0.2  | 192M |   96M    | ✅ PostgreSQL | Integraciones externas  |
| **FileStorageService** |    36    | 0.2  | 192M |   96M    | ✅ PostgreSQL | S3/Azure Blob storage   |

#### 📊 Resumen de Recursos Totales

| Categoría         | Servicios | CPU Total | RAM Total  |
| ----------------- | :-------: | :-------: | :--------: |
| 🔴 Core           |     7     |   3.25    |   2.4GB    |
| 🟡 Secundarios    |     7     |    2.2    |   1.8GB    |
| 🟢 Ligeros        |    10     |   1.65    |   1.5GB    |
| 🔵 Especializados |     4     |    1.3    |   1.1GB    |
| ⚪ Verticales     |     7     |   1.55    |   1.3GB    |
| **APIs Total**    |  **35**   | **10.0**  | **~8.1GB** |
| PostgreSQL (×20)  |    20     |    5.0    |   5.0GB    |
| Redis             |     1     |    0.1    |    128M    |
| RabbitMQ          |     1     |   0.25    |    256M    |
| Elasticsearch     |     1     |    0.5    |    512M    |
| Consul            |     1     |    0.1    |    128M    |
| **TOTAL MÁXIMO**  |     -     |  **~16**  | **~14GB**  |

#### ⚠️ Estrategia para PC con 8GB RAM

**NO es posible correr todos los servicios simultáneamente.** Usar perfiles:

```powershell
# Perfil MÍNIMO (~2GB RAM) - Solo auth y errores
docker-compose up -d redis rabbitmq authservice-db authservice errorservice-db errorservice gateway

# Perfil BÁSICO (~3.5GB RAM) - Core funcional
docker-compose up -d redis rabbitmq \
  authservice-db authservice \
  errorservice-db errorservice \
  userservice-db userservice \
  notificationservice-db notificationservice \
  gateway

# Perfil DESARROLLO (~5GB RAM) - Con productos y media
docker-compose up -d redis rabbitmq \
  authservice-db authservice \
  errorservice-db errorservice \
  userservice-db userservice \
  productservice-db productservice \
  mediaservice-db mediaservice \
  notificationservice-db notificationservice \
  gateway
```

#### Template para compose.yaml

```yaml
# Para bases de datos PostgreSQL
service-db:
  image: postgres:16
  container_name: service-db
  deploy:
    resources:
      limits:
        cpus: "0.25"
        memory: 256M
      reservations:
        memory: 128M
  # ... resto de configuración

# Para APIs .NET
servicename:
  build:
    context: ./backend
    dockerfile: ServiceName/ServiceName.Api/Dockerfile.dev
  container_name: servicename
  deploy:
    resources:
      limits:
        cpus: "0.5"
        memory: 384M
      reservations:
        memory: 256M
  # ... resto de configuración

# Para Redis
redis:
  image: redis:7-alpine
  container_name: redis
  deploy:
    resources:
      limits:
        cpus: "0.1"
        memory: 128M
      reservations:
        memory: 64M
  # ... resto de configuración

# Para RabbitMQ
rabbitmq:
  image: rabbitmq:3.12-management
  container_name: rabbitmq
  deploy:
    resources:
      limits:
        cpus: "0.25"
        memory: 256M
      reservations:
        memory: 128M
  # ... resto de configuración
```

#### Monitoreo de Recursos

```powershell
# Ver uso de recursos de todos los contenedores
docker stats --format "table {{.Name}}\t{{.CPUPerc}}\t{{.MemUsage}}\t{{.MemPerc}}"

# Ver contenedores que exceden límites
docker stats --no-stream --format "{{.Name}}: {{.MemPerc}}" | findstr /V "0.00%"
```

---

## 📱 FRONTEND WEB (React)

### Estructura

```
frontend/web/src/
├── components/           # Componentes reutilizables
├── pages/               # Páginas/Rutas
├── hooks/               # Custom hooks
├── services/            # API clients (axios)
├── stores/              # Zustand stores
├── utils/               # Utilidades
├── types/               # TypeScript types
└── i18n/                # Internacionalización
```

### Tech Stack

- **React 19** + TypeScript 5.6
- **Vite 7** para bundling
- **TailwindCSS 3.4** para estilos
- **Zustand 5** para state management
- **TanStack Query 5** para server state
- **React Router 7** para routing
- **React Hook Form + Zod** para forms
- **i18next** para i18n (ES/EN)

### Comandos

```powershell
cd frontend/web
npm install
npm run dev      # Development server
npm run build    # Production build
npm run test     # Vitest
```

### ⚠️ NOTAS CRÍTICAS TYPESCRIPT/MONOREPO

1. **Estructura Monorepo**: El proyecto usa npm workspaces. `node_modules` está en `frontend/` NO en `frontend/web/`

2. **Configuración de typeRoots**: Los tsconfig deben apuntar al directorio padre:

   ```json
   // tsconfig.app.json y tsconfig.node.json
   "typeRoots": ["../node_modules/@types"]
   ```

3. **Tipos de Vite y Node**: Usar triple-slash directives en lugar de `types` en tsconfig:

   ```typescript
   // src/vite-env.d.ts (DEBE existir)
   /// <reference types="vite/client" />

   // vite.config.ts (al inicio del archivo)
   /// <reference types="node" />
   ```

4. **verbatimModuleSyntax**: TypeScript 5.6 requiere imports de tipo explícitos:

   ```typescript
   // ❌ Incorrecto
   import { ReactNode, ErrorInfo } from "react";

   // ✅ Correcto - usar 'import type' para tipos
   import type { ReactNode, ErrorInfo } from "react";
   import { Component } from "react"; // solo valores
   ```

5. **Dos archivos de tipos User**: Existen diferencias entre:

   - `src/types/index.ts` - Tipos locales simplificados
   - `src/shared/types/index.ts` - Tipos compartidos completos
   - **Importante**: User tiene `subscription` directamente, NO `dealer.subscription`

6. **AccountType**: Debe incluir `'guest'` como valor válido:

   ```typescript
   type AccountType =
     | "guest"
     | "individual"
     | "dealer"
     | "dealer_employee"
     | "admin"
     | "platform_employee";
   ```

7. **Sentry browserTracingIntegration**: No usar `tracePropagationTargets` dentro del integration:

   ```typescript
   // ❌ Deprecated
   Sentry.browserTracingIntegration({
     tracePropagationTargets: [...]  // NO
   })

   // ✅ Correcto
   Sentry.browserTracingIntegration()
   // tracePropagationTargets va en Sentry.init() directamente
   ```

---

## 📱 FRONTEND MOBILE (Flutter)

### Arquitectura

- **Clean Architecture** con capas separadas
- **BLoC Pattern** para state management
- **GetIt + Injectable** para DI
- **Dio + Retrofit** para networking
- **Hive** para local storage

### Estructura

```
lib/
├── core/                # Utilidades, themes, constants
├── data/
│   ├── datasources/     # Remote y local datasources
│   ├── models/          # Data models (JSON serializable)
│   └── repositories/    # Repository implementations
├── domain/
│   ├── entities/        # Business entities
│   ├── repositories/    # Repository contracts
│   └── usecases/        # Use cases
├── presentation/
│   ├── blocs/           # BLoC state management
│   ├── pages/           # Screens
│   └── widgets/         # Reusable widgets
├── l10n/                # Localization
├── main.dart
├── main_dev.dart        # Flavor: development
├── main_staging.dart    # Flavor: staging
└── main_prod.dart       # Flavor: production
```

### Comandos

```powershell
cd frontend/mobile/cardealer
flutter pub get
flutter run                      # Debug
flutter run --flavor dev         # Dev flavor
flutter build apk --release      # Android release
flutter build ios --release      # iOS release
```

### ⚠️ NOTAS CRÍTICAS FLUTTER/DART (APIs que han cambiado)

1. **connectivity_plus**: El listener ahora retorna `ConnectivityResult` (single), NO `List<ConnectivityResult>`:

   ```dart
   // ❌ Incorrecto (API antigua)
   Connectivity().onConnectivityChanged.listen((List<ConnectivityResult> results) {
     final result = results.first;
   });

   // ✅ Correcto (API actual)
   Connectivity().onConnectivityChanged.listen((ConnectivityResult result) {
     // usar result directamente
   });
   ```

2. **fl_chart SideTitleWidget**: Usar `axisSide` en lugar de `meta`:

   ```dart
   // ❌ Incorrecto
   SideTitleWidget(meta: meta, child: Text('...'))

   // ✅ Correcto
   SideTitleWidget(axisSide: meta.axisSide, child: Text('...'))
   ```

3. **Color.withOpacity deprecated**: Usar `withValues(alpha:)`:

   ```dart
   // ❌ Deprecated
   color.withOpacity(0.5)

   // ✅ Correcto
   color.withValues(alpha: 0.5)
   ```

4. **Uso de context después de async**: Siempre verificar `mounted`:

   ```dart
   // ❌ Incorrecto - puede fallar si widget fue desmontado
   final image = await picker.pickImage(source: ImageSource.camera);
   if (image != null) {
     ScaffoldMessenger.of(context).showSnackBar(...);
   }

   // ✅ Correcto - guardar referencias ANTES del await
   final navigator = Navigator.of(context);
   final messenger = ScaffoldMessenger.of(context);
   navigator.pop();
   final image = await picker.pickImage(source: ImageSource.camera);
   if (image != null && mounted) {
     messenger.showSnackBar(...);
   }
   ```

5. **Scripts de utilidad**: Agregar `// ignore_for_file: avoid_print` en archivos tool/:

   ```dart
   // ignore_for_file: avoid_print
   import 'dart:io';

   void main() {
     print('This is allowed in scripts');
   }
   ```

6. **Constantes en widgets**: Usar `const` cuando sea posible para mejor rendimiento:

   ```dart
   // ❌ Sin const - crea nueva instancia cada rebuild
   Icon(Icons.home, color: Colors.blue)

   // ✅ Con const - misma instancia
   const Icon(Icons.home, color: Colors.blue)
   ```

---

## 🔧 CONFIGURACIÓN DE DESARROLLO

### Requisitos

- .NET SDK 8.0+
- Node.js 20+
- Flutter SDK 3.2+
- Docker Desktop
- Visual Studio Code / Rider

### Setup inicial

```powershell
# Clonar repositorio
git clone https://github.com/gregorymorenoiem/cardealer-microservices.git
cd cardealer-microservices

# Backend
cd backend
dotnet restore
docker-compose up -d  # Levantar dependencias

# Frontend Web
cd ../frontend/web
npm install
npm run dev

# Frontend Mobile
cd ../frontend/mobile/cardealer
flutter pub get
flutter run
```

---

## 📝 CONVENCIONES DE CÓDIGO

### C# / .NET

```csharp
// Namespaces: File-scoped
namespace AuthService.Domain.Entities;

// Clases: PascalCase
public class ApplicationUser { }

// Interfaces: I + PascalCase
public interface IUserRepository { }

// Métodos async: sufijo Async
public async Task<User> GetUserAsync(Guid id, CancellationToken ct);

// Records para DTOs inmutables
public record UserDto(Guid Id, string Email, string FullName);

// Primary constructors para DI
public class UserService(IUserRepository repo, ILogger<UserService> logger)
{
    public async Task<User?> GetAsync(Guid id) => await repo.GetByIdAsync(id);
}
```

### TypeScript / React

```typescript
// Interfaces: I prefix opcional, preferir types
type User = {
  id: string;
  email: string;
  fullName: string;
};

// Components: PascalCase, función arrow
export const UserCard = ({ user }: { user: User }) => {
  return <div>{user.fullName}</div>;
};

// Hooks: use prefix
export const useAuth = () => {
  // ...
};

// API calls: sufijo Api o Service
export const userApi = {
  getById: (id: string) => axios.get<User>(`/api/users/${id}`),
};
```

### Dart / Flutter

```dart
// Classes: PascalCase
class UserEntity {
  final String id;
  final String email;

  const UserEntity({required this.id, required this.email});
}

// BLoC naming
class AuthBloc extends Bloc<AuthEvent, AuthState> { }

// Widgets: sufijo Widget o Page
class LoginPage extends StatelessWidget { }
class UserCardWidget extends StatelessWidget { }
```

---

## 🚀 CI/CD

### GitHub Actions

- `.github/workflows/` contiene workflows de CI/CD
- Build y test automáticos en PR
- Deploy a staging/production

### Ambientes

| Ambiente      | Descripción      |
| ------------- | ---------------- |
| `Development` | Local con Docker |
| `Staging`     | Pre-producción   |
| `Production`  | Producción       |

---

## 📚 DOCUMENTACIÓN ADICIONAL

| Documento                              | Descripción                  |
| -------------------------------------- | ---------------------------- |
| `ARQUITECTURA_MICROSERVICIOS.md`       | Diseño de arquitectura       |
| `SECURITY_POLICIES.md`                 | Políticas de seguridad       |
| `VAULT_INTEGRATION_GUIDE.md`           | Gestión de secretos          |
| `CI_CD_MONITORING_GUIDE.md`            | Monitoreo y CI/CD            |
| `GUIA_MULTI_DATABASE_CONFIGURATION.md` | Multi-provider DB            |
| `CONVERSION_A_SISTEMA_MULT-TENANT.md`  | Multi-tenancy                |
| `MICROSERVICES_AUDIT_SPRINT_PLAN.md`   | **Plan de auditoría Docker** |
| `MICROSERVICES_AUDIT_REPORT.md`        | Reporte de auditoría         |

---

## ⚠️ NOTAS IMPORTANTES PARA COPILOT

1. **NO crear referencias cruzadas** entre microservicios
2. **Usar RabbitMQ** para comunicación inter-servicios
3. **Siempre implementar** `ITenantEntity` para entidades multi-tenant
4. **Validar con FluentValidation** antes de procesar commands
5. **Publicar Domain Events** para operaciones importantes
6. **Usar Result Pattern** en lugar de excepciones para flujo de control
7. **Incluir CancellationToken** en métodos async
8. **Documentar** endpoints con XML comments para Swagger
9. **Seguir naming conventions** del proyecto
10. **Tests**: mínimo 80% coverage para nuevas features

---

## 🚨 ESTADO DEL PROYECTO (1 Enero 2026 - 04:00)

### 🎉 FASE 0 COMPLETADA AL 100% (1 Ene 2026 - 04:00)

**Estado:** ✅ **11/11 sprints completados** - Infraestructura lista para FASE 1

**Sprints completados:**

- ✅ Sprint 0.1-0.2: Infraestructura Docker y credenciales de prueba
- ✅ Sprint 0.5.1-0.5.5: Docker Services (5 sprints)
- ✅ Sprint 0.6.1: AuthService Dockerfile Fix
- ✅ Sprint 0.6.2: ProductService Fix
- ✅ Sprint 0.6.3: **Schema Validation** (1 Ene 2026 - 02:00)
- ✅ Sprint 0.7.1: **Gestión de Secretos** (36 secretos reemplazados)
- ✅ Sprint 0.7.2: **Validación de Secretos** (1 Ene 2026 - 04:00)

**Sprint 0.7.2 - Validación de Secretos (Completado):**

- ✅ RabbitMQ audit: 8/8 servicios con configuración correcta
- ✅ TODOS los servicios usan `"Host"` NO `"HostName"` (0 fixes requeridos)
- ✅ Infraestructura validada: Redis, RabbitMQ, Consul operacionales
- ✅ 4/4 servicios core healthy: AuthService, ErrorService, UserService, RoleService
- ✅ Startup incremental exitoso: Infrastructure → DBs → Services
- 📄 Documentación: `SPRINT_0.7.2_SECRETS_VALIDATION_COMPLETION.md`

**Sprint 0.6.3 - Schema Validation (Completado):**

- ✅ Script creado: `scripts/Validate-DatabaseSchemas.ps1` (300+ líneas)
- ✅ Valida C# entities vs PostgreSQL columns automáticamente
- ✅ 4/4 servicios core: 0 desincronizaciones detectadas
- ✅ Herramienta reutilizable para QA continuo
- 📄 Documentación: `SPRINT_0.6.3_SCHEMA_VALIDATION_COMPLETION.md`

**Sprint 0.7.1 - Gestión de Secretos (Completado):**

- ✅ 36 secretos reemplazados con variables de entorno
- ✅ 12 JWT keys: `Jwt__Key: "${JWT__KEY:-default}"`
- ✅ 24 PostgreSQL passwords: `POSTGRES_PASSWORD: "${POSTGRES_PASSWORD:-password}"`
- ✅ Script: `scripts/replace-secrets-clean.ps1` (92 líneas)
- 📄 Documentación: `SPRINT_0.7.1_SECRETS_MANAGEMENT_COMPLETION.md`

**Infraestructura validada:**

- ✅ Redis: UP and healthy
- ✅ RabbitMQ: UP and healthy (8 servicios con configuración correcta)
- ✅ Consul: UP and healthy
- ✅ PostgreSQL: 7/7 DB instances para servicios core
- ✅ 4/4 servicios core operacionales

**Progreso global:** 62.2% (23/37 sprints)

- FASE 0: 11/11 sprints = 100% ✅
- FASE 1: 4/4 sprints = 100% ✅ (AuthService, ErrorService, Gateway, NotificationService)
- FASE 2: 4/4 sprints = 100% ✅ (CacheService, MessageBusService, ConfigurationService, ServiceDiscovery)
- FASE 3: 3/3 sprints = 100% ✅ (LoggingService, TracingService, HealthCheckService)
- FASE 4: 0/15 sprints = 0%

**FASE 1 Completada (1 Ene 2026):**

- ✅ Sprint 1.1: AuthService - 11 endpoints auditados
- ✅ Sprint 1.2: ErrorService - 6 endpoints auditados
- ✅ Sprint 1.3: Gateway - Ocelot routing validado, 7 rutas configuradas
- ✅ Sprint 1.4: NotificationService - 17 endpoints auditados (Email, SMS, Push, Teams)
- 📄 Documentación: `FASE_1_PROGRESS_REPORT.md`

**FASE 2 Completada (1 Ene 2026):**

- ✅ Sprint 2.1: CacheService - 7 endpoints auditados (Redis, Distributed Locks, Statistics)
- ✅ Sprint 2.2: MessageBusService - 17 endpoints auditados (RabbitMQ, Sagas, Dead Letters)
- ✅ Sprint 2.3: ConfigurationService - 7 endpoints auditados (Config dinámica, Feature Flags)
- ✅ Sprint 2.4: ServiceDiscovery - 10 endpoints auditados (Consul, Health Checks)
- 📄 Documentación: `FASE_1_PROGRESS_REPORT.md` (incluye FASE 2)

**FASE 3 Completada (1 Ene 2026):**

- ✅ Sprint 3.1: LoggingService - 23 endpoints auditados (Logs, Alerts, Analysis)
- ✅ Sprint 3.2: TracingService - 6 endpoints auditados (Traces, Spans, Services)
- ✅ Sprint 3.3: HealthCheckService - 4 endpoints auditados (System Health, Service Health)
- 📄 Documentación: `FASE_1_PROGRESS_REPORT.md` (incluye FASE 3)

**Próximo paso:** FASE 4 - Sprint 4.1 UserService Audit

### ✅ RESUELTO: Migraciones EF Core

**Estado:** ✅ **VERIFICADO** - Las migraciones están correctas.

**Verificación realizada (31 Dic 2025):**

- ✅ AuthService: Todas las columnas existen en la BD (`CreatedAt`, `UpdatedAt`, `DealerId`, `ExternalAuthProvider`, `ExternalUserId`)
- ✅ RefreshTokens: `Id` existe como PK
- ✅ ProductService: DealerId agregado a products, product_images, categories
- ✅ UserService: Users, UserRoles con DealerId
- ✅ RoleService: Roles, Permissions, RolePermissions con DealerId
- ✅ ErrorService: error_logs con DealerId

### ✅ RESUELTO: Servicios en docker-compose.yml

**Estado:** ✅ **COMPLETADO** - Todos los 35 servicios están en docker-compose.yml

**Verificado el 31 Dic 2025:**

- ✅ Todos los servicios tienen configuración en compose.yaml
- ✅ Todos los servicios tienen Dockerfile.dev
- ✅ Todos los servicios tienen bases de datos PostgreSQL configuradas (donde aplica)
- ✅ Variables de entorno `Database__*` configuradas correctamente

### ✅ RESUELTO: AuthService funcional

**Estado:** 🟢 **FUNCIONAL** (31 Dic 2025)

**Correcciones aplicadas:**

1. ✅ Dockerfile.dev cambiado de `dotnet watch` a `dotnet build + dotnet run`
2. ✅ Variables de entorno agregadas en compose.yaml:
   - `Database__Provider: "PostgreSQL"`
   - `Database__Host`, `Database__Port`, `Database__Database`
   - `Database__Username`, `Database__Password`
   - `Database__ConnectionStrings__PostgreSQL` (connection string completo)
   - `Database__AutoMigrate: "true"`
3. ✅ Health check responde 200 OK
4. ✅ Endpoints `/api/auth/register` y `/api/auth/login` funcionales
5. ✅ Tokens JWT generados correctamente

**Credenciales de prueba creadas:**

```
Email: test@example.com
Password: Admin123!
UserName: testuser
AccountType: individual
EmailConfirmed: true
```

---

## 🔐 SECRETOS REQUERIDOS PARA PRODUCCIÓN

Para que los microservicios funcionen, solo se necesita suministrar estos secretos:

| Servicio                | Secreto                  | Variable de Entorno                        | Obligatorio |
| ----------------------- | ------------------------ | ------------------------------------------ | :---------: |
| **AuthService**         | JWT Secret Key           | `JWT__KEY`                                 |     ✅      |
|                         | Google Client ID         | `AUTHENTICATION__GOOGLE__CLIENTID`         |     ⚪      |
|                         | Google Client Secret     | `AUTHENTICATION__GOOGLE__CLIENTSECRET`     |     ⚪      |
|                         | Microsoft Client ID      | `AUTHENTICATION__MICROSOFT__CLIENTID`      |     ⚪      |
|                         | Microsoft Client Secret  | `AUTHENTICATION__MICROSOFT__CLIENTSECRET`  |     ⚪      |
| **NotificationService** | SendGrid API Key         | `NOTIFICATIONSETTINGS__SENDGRID__APIKEY`   |    ⚪\*     |
|                         | Twilio Account SID       | `NOTIFICATIONSETTINGS__TWILIO__ACCOUNTSID` |    ⚪\*     |
|                         | Twilio Auth Token        | `NOTIFICATIONSETTINGS__TWILIO__AUTHTOKEN`  |    ⚪\*     |
|                         | Firebase Service Account | Archivo JSON montado                       |    ⚪\*     |
| **BillingService**      | Stripe Secret Key        | `STRIPE__SECRETKEY`                        |     ✅      |
|                         | Stripe Webhook Secret    | `STRIPE__WEBHOOKSECRET`                    |     ✅      |
| **MediaService**        | AWS Access Key           | `S3STORAGE__ACCESSKEY`                     |   ⚪\*\*    |
|                         | AWS Secret Key           | `S3STORAGE__SECRETKEY`                     |   ⚪\*\*    |
|                         | Azure Connection String  | `AZUREBLOBSTORAGE__CONNECTIONSTRING`       |   ⚪\*\*    |

> ✅ = Obligatorio | ⚪ = Opcional | ⚪\* = Al menos un canal requerido | ⚪\*\* = Según provider

---

## 🔄 VERSIONES DE PAQUETES RECOMENDADAS

### .NET Packages (actualizado 2025)

```xml
<!-- Core -->
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.11" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.11" />

<!-- Auth -->
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.11" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="8.2.1" />

<!-- CQRS -->
<PackageReference Include="MediatR" Version="12.4.1" />
<PackageReference Include="FluentValidation" Version="11.11.0" />

<!-- Messaging -->
<PackageReference Include="RabbitMQ.Client" Version="6.8.1" />

<!-- Resilience -->
<PackageReference Include="Polly" Version="8.5.2" />

<!-- Cache -->
<PackageReference Include="StackExchange.Redis" Version="2.8.22" />

<!-- Service Discovery -->
<PackageReference Include="Consul" Version="1.7.14.9" />

<!-- Scheduler -->
<PackageReference Include="Hangfire.Core" Version="1.8.17" />
<PackageReference Include="Hangfire.AspNetCore" Version="1.8.17" />

<!-- Observability -->
<PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="1.14.0" />
<PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />

<!-- API Docs -->
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.8.1" />

<!-- Testing -->
<PackageReference Include="xunit" Version="2.9.2" />
<PackageReference Include="FluentAssertions" Version="7.0.0" />
<PackageReference Include="Moq" Version="4.20.72" />
```

---

## � CHECKLIST PRE-EJECUCIÓN DE TAREAS

Antes de ejecutar CUALQUIER tarea de los sprint plans, verificar:

### 1. Estimación de Tokens

```
[ ] Calculé tokens de lectura (archivos × líneas × 4)
[ ] Calculé tokens de escritura (líneas nuevas × 5)
[ ] Sumé contexto base (~8,000)
[ ] Apliqué multiplicador de complejidad
[ ] Total < 110,000 tokens de input
[ ] Output esperado < 16,000 tokens
```

### 2. División si Excede Límites

```
[ ] Si total > 80,000: dividir en subtareas
[ ] Cada subtarea debe ser independiente y testeable
[ ] Definir orden de ejecución
[ ] Documentar dependencias entre subtareas
```

### 3. Contexto Necesario

```
[ ] Tengo acceso a todos los archivos requeridos
[ ] Las dependencias están identificadas
[ ] Los tests existentes están considerados
[ ] El breaking change está documentado
```

### 4. Validación Post-Tarea

```
[ ] Código compila sin errores
[ ] Tests pasan (si aplica)
[ ] Lint/format aplicado
[ ] Commit message sigue convención
```

---

## �📁 TEMPLATES Y SAMPLES

Para tareas comunes, consulta los templates en `.github/copilot-samples/`:

| Template                                                                     | Descripción                        |
| ---------------------------------------------------------------------------- | ---------------------------------- |
| [new-microservice-template.md](copilot-samples/new-microservice-template.md) | Crear nuevo microservicio          |
| [cqrs-feature-template.md](copilot-samples/cqrs-feature-template.md)         | Crear Commands/Queries con MediatR |
| [domain-events-template.md](copilot-samples/domain-events-template.md)       | Eventos de dominio y RabbitMQ      |
| [testing-template.md](copilot-samples/testing-template.md)                   | Unit tests e Integration tests     |
| [quick-reference.md](copilot-samples/quick-reference.md)                     | Comandos y endpoints frecuentes    |

---

## 📱 FLUTTER MOBILE - NOTAS CRÍTICAS

### Información del Proyecto

| Aspecto                | Valor                               |
| ---------------------- | ----------------------------------- |
| **Nombre del paquete** | `cardealer_mobile` (NO `cardealer`) |
| **Ruta del proyecto**  | `frontend/mobile/cardealer`         |
| **SDK Flutter**        | >=3.4.0 (stable 3.35.4+)            |
| **SDK Dart**           | >=3.4.0 <4.0.0 (3.9.2+)             |

### ⚠️ ERRORES COMUNES A EVITAR

1. **Imports del paquete**: SIEMPRE usar `package:cardealer_mobile/...` NO `package:cardealer/...`

2. **Dos archivos de Failures con sintaxis diferente**:

   - `core/error/failures.dart` - Usa parámetros NOMBRADOS: `const AuthFailure({required super.message});`
   - `core/errors/failures.dart` - Usa parámetros POSICIONALES: `const AuthFailure(super.message);`
   - Los usecases de Auth importan `core/errors/failures.dart` (posicional)
   - Los usecases de Vehicle importan `core/error/failures.dart` (nombrado)

3. **Testing con mocktail (NO mockito)**:

   - El proyecto usa `mocktail` para mocking - NO requiere code generation
   - NO usar `@GenerateMocks` ni `build_runner`
   - Sintaxis: `class MockRepo extends Mock implements Repo {}`
   - When: `when(() => mock.method()).thenReturn(value)`
   - Any: `any(named: 'param')` en lugar de `anyNamed('param')`
   - Registrar fallback values: `setUpAll(() { registerFallbackValue(UserRole.individual); })`

4. **Use cases sin parámetros**: Usar `.call()` explícito

   ```dart
   // ✅ Correcto
   when(() => mockLogoutUseCase.call()).thenAnswer((_) async => const Right(null));

   // ❌ Incorrecto
   when(() => mockLogoutUseCase()).thenAnswer(...);  // No funciona con mocktail
   ```

5. **AuthBloc estados de registro**: El registro emite `AuthRegistrationSuccess` NO `AuthAuthenticated`

6. **Vehicle entity**: Requiere `createdAt` como parámetro obligatorio
   ```dart
   Vehicle(id: '1', name: 'Test', createdAt: DateTime(2024, 1, 1), ...)
   ```

### 🔧 COMANDOS FLUTTER

```powershell
# ⚠️ CRÍTICO: Los comandos flutter (analyze, test) pueden quedarse esperando input
# SIEMPRE agregar `; echo ""` al final del comando para forzar que termine
# O enviar ENTER manualmente si el proceso se queda colgado

# Análisis - USAR ESTE FORMATO:
flutter analyze --no-fatal-infos --no-fatal-warnings 2>&1; echo ""

# Tests - USAR ESTE FORMATO:
flutter test 2>&1; echo ""
flutter test test/presentation/bloc/ 2>&1; echo ""
flutter test --reporter compact 2>&1; echo ""

# Build runner (si fuera necesario - NO requerido con mocktail)
dart run build_runner build --delete-conflicting-outputs

# Limpiar y reconstruir
flutter clean
flutter pub get
```

### 📁 ESTRUCTURA DE TESTS

```
test/
├── presentation/
│   └── bloc/
│       ├── auth/
│       │   └── auth_bloc_test.dart      # 9 tests - mocktail
│       └── vehicles/
│           └── vehicles_bloc_test.dart  # 16 tests - mocktail
└── ... (otros tests)
```

### 🧪 TEMPLATE DE TEST CON MOCKTAIL

```dart
import 'package:flutter_test/flutter_test.dart';
import 'package:mocktail/mocktail.dart';
import 'package:dartz/dartz.dart';
import 'package:bloc_test/bloc_test.dart';
import 'package:cardealer_mobile/core/errors/failures.dart'; // posicional

// Mock classes - NO code generation needed
class MockMyUseCase extends Mock implements MyUseCase {}

void main() {
  late MyBloc bloc;
  late MockMyUseCase mockUseCase;

  // Register fallback values for non-primitive types
  setUpAll(() {
    registerFallbackValue(UserRole.individual);
  });

  setUp(() {
    mockUseCase = MockMyUseCase();
    bloc = MyBloc(myUseCase: mockUseCase);
  });

  tearDown(() {
    bloc.close();
  });

  blocTest<MyBloc, MyState>(
    'emits [Loading, Success] when successful',
    build: () {
      when(() => mockUseCase.call()).thenAnswer((_) async => const Right(result));
      return bloc;
    },
    act: (bloc) => bloc.add(MyEvent()),
    expect: () => [MyLoading(), MySuccess(result)],
  );
}
```

---

## 🏷️ COMMITS Y BRANCHES

### Convención de Commits

```
<type>(<scope>): <description>

[optional body]

[optional footer]
```

**Tipos:**

- `feat`: Nueva funcionalidad
- `fix`: Corrección de bug
- `docs`: Documentación
- `style`: Formato (no cambia código)
- `refactor`: Refactorización
- `test`: Agregar tests
- `chore`: Tareas de mantenimiento

**Ejemplos:**

```
feat(auth): add 2FA support with TOTP
fix(product): resolve pagination issue with custom fields
docs(readme): update API documentation
test(user): add integration tests for user creation
```

### Convención de Branches

```
<type>/<ticket-id>-<short-description>
```

**Ejemplos:**

```
feature/CD-123-add-2fa-support
bugfix/CD-456-fix-login-error
hotfix/CD-789-security-patch
```

---

_Última actualización: 30 Diciembre 2025_
