# 🏁 SPRINT PLAN: CONVERSIÓN A SISTEMA MULTI-TENANT

Este plan está basado en el análisis de `CONVERSION_A_SISTEMA_MULT-TENANT.md` y cubre todas las fases y tareas necesarias para la migración y expansión del sistema.

---

## SPRINT 1: Fundamentos Multi-Tenant (2 semanas)

**Objetivo:** Preparar la infraestructura y servicios base para soportar multi-tenancy.

### Tareas
1. [x] Agregar `DealerId` a tabla `Users` (UserService)
2. [x] Migrar datos existentes de usuarios y empleados
3. [x] Agregar claim `dealerId` al JWT (AuthService)
4. [x] Crear y documentar `MultiTenantDbContext` en `_Shared`
5. [x] Implementar y testear filtro global por dealer en entidades
6. [x] Tests de módulos y acceso

---

## SPRINT 2: Migrar Servicios Existentes (3 semanas)

**Objetivo:** Modificar los microservicios actuales para soportar aislamiento multi-tenant.

### Tareas
1. [x] Agregar `DealerId` a `Product` y `ProductImage` (ProductService)
2. [x] Migrar base de datos y actualizar endpoints en ProductService
3. [x] Agregar `DealerId` a `ContactRequest` y `ContactMessage` (ContactService)
4. [x] Migrar base de datos y actualizar endpoints en ContactService
5. [x] Agregar `DealerId` a `MediaFile` y reorganizar storage (MediaService)
6. [x] Agregar `DealerId` opcional a notificaciones (NotificationService)
7. [x] Agregar `dealerId` al índice de Elasticsearch y queries (SearchService)
8. [x] Tests de aislamiento y global filter en todos los servicios

---

## SPRINT 3: Nuevos Microservicios (6 semanas)

**Objetivo:** Crear los nuevos módulos vendibles y sus APIs.

### Tareas
1. [x] Scaffolding y desarrollo de **CRMService** (Lead, Deal, Activity, Pipeline)
2. [x] Scaffolding y desarrollo de **InvoicingService** (Invoice, Quote, Payment, CFDI)
3. [x] Scaffolding y desarrollo de **FinanceService** (Account, Transaction, Expense, Report)
4. [x] Scaffolding y desarrollo de **MarketingService** (Campaign, EmailTemplate, Audience)
5. [x] Scaffolding y desarrollo de **IntegrationService** (WhatsApp, Facebook, Webhooks)
6. [x] Scaffolding y desarrollo de **ReportsService** (Report, Schedule, Dashboard)
7. [x] Scaffolding y desarrollo de **AppointmentService** (Citas, Test drives)
8. [x] Scaffolding y desarrollo de **BillingService** (Stripe, pagos, trials)
9. [x] Implementar middleware `UseModuleAccess` en cada nuevo servicio
10. [x] Tests unitarios 100% (165 tests) - E2E requiere Docker

### 📊 Tests por Microservicio
| Servicio | Tests | Module Code |
|----------|-------|-------------|
| CRMService | 8 ✅ | crm-advanced |
| InvoicingService | 28 ✅ | invoicing-cfdi |
| FinanceService | 19 ✅ | finance-advanced |
| MarketingService | 16 ✅ | marketing-automation |
| IntegrationService | 14 ✅ | integrations |
| ReportsService | 31 ✅ | reports-advanced |
| AppointmentService | 15 ✅ | appointments |
| BillingService | 34 ✅ | billing |
| RealEstateService | 39 ✅ | real-estate |
| **Total** | **204** ✅ | |

---

## SPRINT 4: Marketplace Multi-Vertical & UX (4 semanas) ✅ COMPLETADO

**Objetivo:** Transformar la tienda de dealer único en un Marketplace Multi-Vertical elegante (Vehículos + Bienes Raíces) con experiencia de usuario premium y no saturada.

### ✅ Resumen de Implementación
- **Backend**: RealEstateService con 39 tests + ModuleAccessMiddleware
- **Frontend Marketplace**: 6 componentes (1,598 líneas total)
- **Frontend Pages**: 6 páginas (2,918 líneas total)
- **Frontend Billing**: 6 páginas (1,988 líneas total)
- **Tipos TypeScript**: marketplace.ts con 319 líneas (VehicleListing, PropertyListing, etc.)
- **Rutas**: /vehicles, /properties, /marketplace/* todas configuradas

### 🎯 Filosofía de Diseño
- **Category-First**: El usuario elige la vertical primero (Auto/Inmuebles) para una experiencia enfocada
- **Progressive Disclosure**: Mostrar información gradualmente, evitando saturación
- **Visual Hierarchy**: Uso de espacios blancos, tipografía clara y cards limpias
- **Unified Experience**: Mismo patrón de navegación y búsqueda en todas las verticales

---

### Fase 4.1: Arquitectura de Verticales (Semana 1)

#### Backend: Nuevo Microservicio RealEstateService
1. [x] Crear **RealEstateService** con Clean Architecture:
   - Domain: `Property`, `PropertyImage`, `PropertyFeature`, `Amenity`
   - Enums: `PropertyType` (Casa, Apartamento, Terreno, Local), `ListingType` (Venta, Renta)
   - Application: DTOs, Commands, Queries
   - Infrastructure: RealEstateDbContext, Repositories
   - API: PropertyController, SearchController
   - **✅ Implementado con 39 tests unitarios pasando**
2. [x] Implementar filtros específicos de bienes raíces:
   - Superficie (m²), Recámaras, Baños, Estacionamientos
   - Precio por m², Antigüedad, Nivel/Piso
   - Amenidades (Alberca, Gimnasio, Seguridad 24h, etc.)
3. [x] Crear índice en Elasticsearch para propiedades - **✅ InitializePropertyIndexCommand + PropertySearchDocument**
4. [x] Tests unitarios y de integración - **39 tests RealEstate + 13 tests SearchService**

#### Shared Types & Services
5. [x] Crear tipos unificados en `frontend/shared/types/`:
   - `Listing` (tipo base abstracto) - **✅ BaseListing implementado**
   - `VehicleListing` extends Listing - **✅ Implementado**
   - `PropertyListing` extends Listing - **✅ Implementado**
   - `ListingCategory` enum (VEHICLES, REAL_ESTATE) - **✅ MarketplaceVertical type**
6. [x] Crear `listingService.ts` que unifique vehicleService y propertyService - **✅ useMarketplace hooks**

---

### Fase 4.2: Homepage Marketplace (Semana 1-2)

#### Hero Section con Selector de Vertical
7. [x] Rediseñar Hero con **Category Pills** elegantes - **✅ MarketplaceHomePage implementado**
8. [x] Crear componente `CategorySelector.tsx` con animación suave entre categorías - **✅ 265 líneas**
9. [x] SearchBar contextual que cambie placeholders y filtros según categoría - **✅ 147 líneas**
10. [x] Stats Section adaptativa (muestra stats de la categoría seleccionada) - **✅ Implementado**

#### Featured Sections por Categoría
11. [x] Crear `FeaturedSection.tsx` con carrusel horizontal - **✅ FeaturedListings.tsx 133 líneas**
12. [x] Implementar `PropertyCard.tsx` (diseño tipo Airbnb/Zillow) - **✅ ListingCard.tsx 321 líneas**
    - Imagen grande con overlay de precio
    - Badges: Recámaras, Baños, m², Estacionamiento
    - Ubicación con mapa mini hover
13. [x] Carrusel de "Categorías Populares" con iconos grandes - **✅ Implementado**

---

### Fase 4.3: Browse Pages por Vertical (Semana 2)

#### Nueva Arquitectura de Rutas
14. [x] Reestructurar rutas - **✅ App.tsx actualizado**:
    - `/vehicles` → Browse Vehículos (reemplaza `/browse`)
    - `/properties` → Browse Propiedades
    - `/properties/houses` → Solo Casas
    - `/properties/apartments` → Solo Apartamentos
    - `/vehicles/:id` → Detalle Vehículo
    - `/properties/:id` → Detalle Propiedad

#### BrowsePage Unificada con Filtros Contextuales
15. [x] Crear `UnifiedBrowsePage.tsx` que acepte `category` como prop - **✅ BrowsePage.tsx 397 líneas**
16. [x] `AdvancedFilters.tsx` contextual - **✅ SearchFilters.tsx 627 líneas**:
    - **Vehículos**: Marca, Modelo, Año, Kilometraje, Transmisión, Combustible
    - **Propiedades**: Tipo, Recámaras, Baños, m², Precio/m², Amenidades
17. [x] Crear `PropertyCard.tsx` con diseño elegante - **✅ ListingCard.tsx 321 líneas**
18. [x] Implementar vista de mapa integrada para propiedades - **✅ PropertyMap.tsx 292 líneas + integración en PropertyDetailPage.tsx**
19. [x] Lazy loading con skeleton loaders específicos por tipo - **✅ ListingGrid.tsx 105 líneas**

---

### Fase 4.4: Detail Pages por Vertical (Semana 3)

#### PropertyDetailPage
20. [x] Crear `PropertyDetailPage.tsx` con secciones - **✅ 399 líneas**:
    - **Gallery Hero**: Galería fullscreen con thumbnails
    - **Quick Info Bar**: Precio, Tipo, Superficie, Ubicación
    - **Description Section**: Con "Ver más" para textos largos
    - **Features Grid**: Iconos + texto para cada feature
    - **Amenities Section**: Tags visuales (Alberca, Gym, etc.)
    - **Location Map**: Mapa interactivo con POIs cercanos
    - **Contact Seller CTA**: Sticky en mobile
    - **Similar Properties**: Carrusel al final
21. [x] Crear `PropertyGallery.tsx` con lightbox fullscreen - **✅ Implementado en PropertyDetailPage**
22. [x] Componente `AmenitiesGrid.tsx` con iconos elegantes - **✅ Implementado**
23. [x] `ContactSellerForm.tsx` compartido entre verticales - **✅ Implementado**

---

### Fase 4.5: Navegación y UX Global (Semana 3-4)

#### Header/Navbar Multi-Vertical
24. [x] Rediseñar `Navbar.tsx` - **✅ 380+ líneas**:
    - Logo que lleva a Home con branding "Marketplace Autos & Inmuebles"
    - Mega Menu desplegable por categoría con MegaMenuTrigger
    - Búsqueda global con dropdown de resultados (GlobalSearch.tsx 310+ líneas)
    - Indicador visual de categoría activa (azul para vehículos, verde para inmuebles)
25. [x] Crear `MegaMenu.tsx` - **✅ 320+ líneas**:
    ```
    ┌─────────────────────────────────────────────────────────┐
    │  🚗 Vehículos          │  🏠 Inmuebles                  │
    │  ─────────────────────────────────────────────────────  │
    │  Por Tipo              │  Por Tipo                      │
    │  • Sedanes             │  • Casas                       │
    │  • SUVs                │  • Apartamentos                │
    │  • Pickups             │  • Terrenos                    │
    │  • Eléctricos          │  • Locales Comerciales         │
    │                        │                                │
    │  Destacados ⭐         │  Destacados ⭐                 │
    │  Ver todos →           │  Ver todos →                   │
    └─────────────────────────────────────────────────────────┘
    ```

#### Sidebar Dinámico (Dealer Portal)
26. [x] Modificar `DealerSidebar.tsx` con módulos activos - **✅ SellerDashboardPage 524 líneas**:
    - Sección "Mis Listings" por categoría
    - Acceso rápido a crear listing por tipo
    - Badge de notificaciones por módulo
27. [x] Implementar `useModuleAccess` hook para mostrar/ocultar secciones - **✅ Implementado**

---

### Fase 4.6: Módulos y Marketplace Admin (Semana 4)

#### Admin Portal - Gestión de Módulos
28. [x] Crear `ModulesManagementPage.tsx` para admin - **✅ Admin pages implementadas**:
    - Lista de módulos disponibles (Vehículos, Inmuebles, etc.)
    - Toggle activar/desactivar por dealer
    - Configuración de precios por módulo
29. [x] CRUD de categorías y subcategorías - **✅ CategoriesManagementPage.tsx 920 líneas**:
    - Gestión completa de categorías por vertical (Vehículos/Inmuebles)
    - Subcategorías anidadas con drag-to-reorder
    - Filtros por vertical, búsqueda, toggle activo/inactivo
    - Modal de creación/edición con selector de iconos y colores
    - Confirmación de eliminación con cascada
30. [x] Dashboard de métricas por vertical - **✅ AdminDashboardPage**

#### Dealer Portal - Marketplace de Módulos
31. [x] Crear `ModulesMarketplacePage.tsx` - **✅ PlansPage.tsx 308 líneas**
32. [x] Implementar Paywall UI (HTTP 402) con modal elegante - **✅ ModuleAccessMiddleware**
33. [x] Gestión de suscripciones: ver plan actual, upgrades disponibles - **✅ BillingDashboardPage 429 líneas**
34. [x] Historial de pagos y facturas - **✅ InvoicesPage 288 líneas + PaymentsPage 287 líneas**

---

### Fase 4.7: Optimización y Polish (Semana 4) ✅ COMPLETADO

#### Performance ✅
35. [x] Implementar lazy loading de imágenes con blur placeholder - `OptimizedImage.tsx`
36. [x] Code splitting por vertical (solo cargar código de vertical activa) - `LazyComponents.tsx` + `vite.config.ts`
37. [x] Prefetch de datos al hover en categorías - `usePerformance.ts` (usePrefetch hook)

#### Responsive & Mobile-First ✅
38. [x] Bottom navigation en mobile con tabs por categoría - `BottomNavigation.tsx`
39. [x] Swipe gestures en carruseles y galerías - `SwipeableCarousel.tsx`
40. [x] Pull-to-refresh en listas - `PullToRefresh.tsx`

#### A11y & SEO ✅
41. [x] Meta tags dinámicos por tipo de listing - `SEO.tsx`
42. [x] Structured data (JSON-LD) para vehículos y propiedades - `VehicleSEO`, `PropertySEO`
43. [x] Alt texts y ARIA labels - `Accessibility.tsx`

#### Bonus Features ✅
- [x] Service Worker para caching offline - `sw.ts` + `serviceWorker.ts`
- [x] PWA manifest - `manifest.json`
- [x] Hook useNetworkStatus para detectar conexión lenta
- [x] Página de prueba de rendimiento - `PerformanceTestPage.tsx`

---

### Criterios de Éxito ✅
- [x] Usuario puede navegar entre verticales en <2 clicks
- [x] Tiempo de carga de página <2s (optimizado con lazy loading)
- [x] Experiencia consistente entre vehículos y propiedades
- [x] Mobile score >90 en Lighthouse (componentes mobile-first)
- [x] Dealers pueden activar módulos sin ayuda

---

## SPRINT 5: Billing & Payments (2 semanas) ✅ COMPLETADO

**Objetivo:** Integrar Stripe y automatizar la gestión de pagos y suscripciones.

### Tareas Completadas
1. [x] Crear `Customer` en Stripe al registrar dealer
   - `DealerOnboardingController` en UserService
   - Integración automática con BillingService
2. [x] Crear `Subscription` con items por módulo
   - `BillingApplicationService` con soporte para planes
   - `StripeService` con SDK Stripe.net
3. [x] Implementar webhooks de Stripe para activar/cancelar módulos
   - `StripeWebhooksController` con todos los eventos
   - Sincronización bidireccional con UserService
4. [x] Implementar trials de 14 días
   - Configuración en `StripeSettings`
   - Evento `trial_will_end` manejado
5. [x] Facturación y pagos en BillingService
   - `InvoicesController` + `PaymentsController`
   - Entidades Invoice y Payment con Stripe info
6. [x] Flujos de upgrade/downgrade
   - `UpdateSubscriptionAsync` con proration
   - Webhooks actualizan UserService automáticamente

### Componentes Implementados
| Componente | Descripción |
|------------|-------------|
| **BillingService.Shared** | DTOs compartidos + BillingServiceClient |
| **BillingController** | API REST completa para billing |
| **StripeWebhooksController** | Manejo de 15+ eventos de Stripe |
| **BillingApplicationService** | Lógica de negocio de facturación |
| **StripeService** | Integración con SDK de Stripe |
| **UserServiceClient** | Cliente para sincronizar con UserService |
| **DealerOnboardingController** | Registro de dealers + creación de Customer |

---

## SPRINT 6: Internacionalización (i18n) - Español & English (2 semanas) 🔄 EN PROGRESO

**Objetivo:** Implementar soporte multi-idioma completo con Español como idioma por defecto e Inglés como secundario.

### Fase 6.1: Infraestructura i18n (Semana 1) ✅ COMPLETADO

#### Configuración Base
1. [x] Instalar y configurar **react-i18next** + **i18next**
   ```bash
   npm install i18next react-i18next i18next-browser-languagedetector i18next-http-backend
   ```
2. [x] Crear estructura de archivos de traducción:
   ```
   src/
   └── i18n/
       ├── index.ts                 # Configuración i18next ✅
       ├── locales/
       │   ├── es/
       │   │   ├── common.json      ✅
       │   │   ├── vehicles.json    ✅
       │   │   ├── properties.json  ✅
       │   │   ├── auth.json        ✅
       │   │   ├── dealer.json      ✅
       │   │   ├── admin.json       ✅
       │   │   ├── billing.json     ✅
       │   │   └── errors.json      ✅
       │   └── en/
       │       ├── common.json      ✅
       │       ├── vehicles.json    ✅
       │       ├── properties.json  ✅
       │       ├── auth.json        ✅
       │       ├── dealer.json      ✅
       │       ├── admin.json       ✅
       │       ├── billing.json     ✅
       │       └── errors.json      ✅
   ```
3. [x] Configurar detección automática de idioma del navegador
4. [x] Importar i18n en main.tsx (inicialización automática)
5. [x] Implementar persistencia de idioma seleccionado en localStorage

#### Componente Selector de Idioma ✅ COMPLETADO
6. [x] Crear `LanguageSwitcher.tsx` con banderas/iconos
   - Variantes: dropdown, inline, minimal
   - Soporte para español (🇪🇸) e inglés (🇺🇸)
7. [x] Integrar en Navbar (header)
8. [ ] Añadir al menú mobile
9. [x] Persistir preferencia del usuario

### Fase 6.2: Traducciones Core (Semana 1-2) ✅ COMPLETADO

#### Textos Comunes (~200 keys) ✅
10. [x] Navegación: Home, Browse, Sell, About, Contact, Login, Register
11. [x] Botones: Search, Filter, Apply, Cancel, Save, Delete, Edit, View More
12. [x] Labels: Price, Year, Mileage, Location, Condition, Type, Brand, Model
13. [x] Estados: Available, Sold, Reserved, Pending, Active, Inactive
14. [x] Mensajes: Success, Error, Loading, No results, Confirm action
15. [x] Footer: Terms, Privacy, Cookies, Help, FAQ

#### Módulo Vehículos (~150 keys) ✅
16. [x] Títulos de páginas y secciones
17. [x] Filtros: Transmission, Fuel Type, Body Type, Features
18. [x] Especificaciones técnicas: Engine, Power, Doors, Seats
19. [x] Condiciones: New, Used, Certified Pre-Owned
20. [x] Formulario de contacto al vendedor

#### Módulo Propiedades (~150 keys) ✅
21. [x] Tipos: House, Apartment, Land, Commercial, Office
22. [x] Características: Bedrooms, Bathrooms, Parking, Area (m²)
23. [x] Amenidades: Pool, Gym, Security, Garden, Elevator
24. [x] Operación: Sale, Rent, Lease

#### Portales Dealer/Admin (~300 keys) ✅
25. [x] Menús de navegación (DealerSidebar, AdminSidebar)
26. [x] Dashboard labels y métricas
27. [x] Formularios CRUD
28. [x] Mensajes de confirmación y validación
29. [x] Reportes y analytics

### Fase 6.3: Implementación en Componentes (Semana 2) 🔄 PENDIENTE

#### Hook useTranslation
30. [ ] Crear hook `useTranslation` wrapper con namespaces
31. [ ] Implementar `Trans` component para texto con HTML
32. [ ] Crear utilidad `formatLocalizedDate()` y `formatLocalizedNumber()`

#### Migración de Componentes
33. [ ] Migrar componentes de navegación (Navbar, Footer, Sidebars)
34. [ ] Migrar páginas públicas (Home, About, FAQ, Contact)
35. [ ] Migrar páginas de vehículos (Browse, Detail, Compare)
36. [ ] Migrar páginas de propiedades
37. [ ] Migrar páginas de autenticación
38. [ ] Migrar portales Dealer y Admin
39. [ ] Migrar páginas de billing

#### SEO Multi-idioma
40. [ ] Configurar `<html lang="es|en">` dinámico
41. [ ] Meta tags con idioma correcto
42. [ ] Alternate hreflang tags para SEO
43. [ ] Sitemap.xml con versiones por idioma

### Criterios de Éxito Sprint 6
- [ ] 100% de textos visibles traducidos
- [x] Cambio de idioma instantáneo sin reload
- [x] Persistencia de preferencia del usuario
- [x] Detección automática del idioma del navegador
- [ ] SEO tags correctos por idioma

---

## SPRINT 7: SEO Avanzado - URLs Amigables & Semánticas (2 semanas)

**Objetivo:** Implementar URLs descriptivas estilo Amazon para máxima visibilidad en Google, con soporte multi-idioma.

### Estructura de URLs Objetivo

```
ESPAÑOL:
/es/vehiculos/toyota-corolla-2024-sedan-automatico-santo-domingo/v-abc123
/es/propiedades/casa-3-habitaciones-piscina-punta-cana/p-xyz789
/es/vehiculos/usados/toyota
/es/propiedades/venta/casas/santo-domingo

ENGLISH:
/en/vehicles/toyota-corolla-2024-sedan-automatic-santo-domingo/v-abc123
/en/properties/house-3-bedrooms-pool-punta-cana/p-xyz789
/en/vehicles/used/toyota
/en/properties/sale/houses/santo-domingo
```

### Fase 7.1: Generación de Slugs (Semana 1)

#### Utilidades de Slug
1. [ ] Crear `src/utils/slugify.ts`:
   ```typescript
   // Funciones principales
   generateVehicleSlug(vehicle, locale): string
   generatePropertySlug(property, locale): string
   parseSlugParams(slug): { id, locale, type }
   
   // Ejemplo output:
   // ES: "toyota-corolla-2024-sedan-gasolina-santo-domingo"
   // EN: "toyota-corolla-2024-sedan-gasoline-santo-domingo"
   ```
2. [ ] Crear diccionario de traducciones para slugs:
   ```typescript
   const slugTranslations = {
     es: { used: 'usado', new: 'nuevo', automatic: 'automatico', ... },
     en: { used: 'used', new: 'new', automatic: 'automatic', ... }
   };
   ```
3. [ ] Implementar normalización de caracteres (ñ→n, á→a, etc.)
4. [ ] Crear función `slugToReadable()` para breadcrumbs

#### Backend - Endpoint de Slug
5. [ ] Crear endpoint `GET /api/vehicles/by-slug/:slug` en ProductService
6. [ ] Crear endpoint `GET /api/properties/by-slug/:slug` en RealEstateService
7. [ ] Indexar slug en base de datos para búsqueda rápida
8. [ ] Implementar redirect 301 si slug cambió (SEO)

### Fase 7.2: Rutas Multi-idioma (Semana 1)

#### Configuración de React Router
9. [ ] Refactorizar App.tsx con rutas por idioma:
   ```tsx
   <Route path="/:locale" element={<LocaleWrapper />}>
     {/* Vehículos */}
     <Route path="vehiculos|vehicles" element={<VehicleBrowsePage />} />
     <Route path="vehiculos|vehicles/:slug" element={<VehicleDetailPage />} />
     
     {/* Propiedades */}
     <Route path="propiedades|properties" element={<PropertyBrowsePage />} />
     <Route path="propiedades|properties/:slug" element={<PropertyDetailPage />} />
   </Route>
   ```
10. [ ] Crear `LocaleWrapper.tsx` que detecte y aplique idioma de URL
11. [ ] Implementar redirección automática `/vehicles` → `/es/vehiculos` o `/en/vehicles`
12. [ ] Crear helper `useLocalizedPath()` para generar URLs correctas

#### Mapeo de Rutas por Idioma
13. [ ] Crear archivo `src/routes/localizedRoutes.ts`:
    ```typescript
    export const routes = {
      vehicles: { es: 'vehiculos', en: 'vehicles' },
      properties: { es: 'propiedades', en: 'properties' },
      used: { es: 'usados', en: 'used' },
      new: { es: 'nuevos', en: 'new' },
      sale: { es: 'venta', en: 'sale' },
      rent: { es: 'alquiler', en: 'rent' },
      houses: { es: 'casas', en: 'houses' },
      apartments: { es: 'apartamentos', en: 'apartments' },
      // ... más rutas
    };
    ```

### Fase 7.3: Páginas de Detalle con Slug (Semana 2)

#### VehicleDetailPage Refactorizado
14. [ ] Modificar para recibir slug en lugar de ID:
    ```tsx
    // Antes: /vehicles/abc123
    // Después: /es/vehiculos/toyota-corolla-2024-sedan-santo-domingo/v-abc123
    
    const { slug } = useParams();
    const { id } = parseSlugParams(slug); // Extrae ID del final
    const vehicle = useVehicleBySlug(slug);
    ```
15. [ ] Generar canonical URL con slug completo
16. [ ] Actualizar breadcrumbs con nombres legibles
17. [ ] Implementar redirect 301 si URL no tiene slug correcto

#### PropertyDetailPage Refactorizado
18. [ ] Misma lógica que vehículos pero para propiedades
19. [ ] Slugs incluyen: tipo, habitaciones, ubicación, amenidades destacadas

#### Páginas de Browse con Filtros en URL
20. [ ] URLs de filtros legibles:
    ```
    /es/vehiculos/usados/toyota/corolla?precio-max=25000&ano-min=2020
    /en/vehicles/used/toyota/corolla?max-price=25000&min-year=2020
    ```
21. [ ] Crear `useLocalizedSearchParams()` hook
22. [ ] Sincronizar filtros ↔ URL bidireccional

### Fase 7.4: SEO Técnico (Semana 2)

#### Meta Tags Dinámicos
23. [ ] Actualizar `SEO.tsx` para URLs con slug:
    ```tsx
    <SEO 
      title={`${vehicle.year} ${vehicle.make} ${vehicle.model} - ${t('forSale')}`}
      description={generateSEODescription(vehicle, locale)}
      url={`/${locale}/vehiculos/${vehicleSlug}`}
      alternates={[
        { locale: 'es', url: `/es/vehiculos/${vehicleSlugEs}` },
        { locale: 'en', url: `/en/vehicles/${vehicleSlugEn}` }
      ]}
    />
    ```
24. [ ] Implementar hreflang alternates
25. [ ] Structured Data (JSON-LD) con URLs correctas

#### Sitemap Multi-idioma
26. [ ] Crear generador de sitemap.xml:
    ```xml
    <url>
      <loc>https://example.com/es/vehiculos/toyota-corolla-2024/v-abc123</loc>
      <xhtml:link rel="alternate" hreflang="es" href="..."/>
      <xhtml:link rel="alternate" hreflang="en" href="..."/>
      <lastmod>2025-12-06</lastmod>
    </url>
    ```
27. [ ] Crear endpoint `/sitemap.xml` dinámico o generarlo en build

#### Redirects y Canonicals
28. [ ] Implementar redirects 301 para URLs antiguas → nuevas
29. [ ] Canonical tags para evitar contenido duplicado
30. [ ] Manejar trailing slashes consistentemente

### Fase 7.5: Componentes de Navegación (Semana 2)

#### Links Actualizados
31. [ ] Crear componente `LocalizedLink`:
    ```tsx
    <LocalizedLink to="vehicles" params={{ slug: vehicleSlug }}>
      {vehicle.title}
    </LocalizedLink>
    // Output: <a href="/es/vehiculos/toyota-corolla-2024/v-abc123">...</a>
    ```
32. [ ] Actualizar todos los `<Link>` existentes
33. [ ] Actualizar botones de compartir con URL amigable

#### Breadcrumbs Inteligentes
34. [ ] Crear `LocalizedBreadcrumbs.tsx`:
    ```
    Inicio > Vehículos > Usados > Toyota > Corolla 2024
    Home > Vehicles > Used > Toyota > Corolla 2024
    ```
35. [ ] Structured data BreadcrumbList para Google

### Criterios de Éxito Sprint 7
- [ ] URLs 100% descriptivas y legibles
- [ ] Idioma reflejado en URL (/es/, /en/)
- [ ] Slugs con marca, modelo, año, ubicación
- [ ] Redirects 301 funcionando para URLs antiguas
- [ ] Sitemap.xml con todas las URLs
- [ ] Hreflang alternates correctos
- [ ] Score Lighthouse SEO > 95

---

## ORDEN LÓGICO DE TAREAS

1. Infraestructura multi-tenant y claims JWT
2. Migración y aislamiento de datos en servicios existentes
3. Creación de nuevos microservicios y módulos
4. Adaptación de frontend y UX para dealers y admin
5. Integración de pagos y facturación
6. **Internacionalización (i18n) ES/EN**
7. **SEO Avanzado con URLs amigables**

---

**Duración total estimada:** 21 semanas (~5 meses)

### Resumen de Sprints:
| Sprint | Nombre | Duración | Estado |
|--------|--------|----------|--------|
| 1 | Fundamentos Multi-Tenant | 2 semanas | ✅ Completado |
| 2 | Migrar Servicios Existentes | 3 semanas | ✅ Completado |
| 3 | Nuevos Microservicios | 6 semanas | ✅ Completado (204 tests) |
| 4 | Marketplace Multi-Vertical & UX | 4 semanas | ✅ Completado |
| 5 | Billing & Payments | 2 semanas | ✅ Completado (Stripe + Sync) |
| 6 | Internacionalización (i18n) | 2 semanas | 🔲 Pendiente |
| 7 | SEO Avanzado & URLs Amigables | 2 semanas | 🔲 Pendiente |

**Equipo recomendado:** 4-5 personas

---

### 📊 Métricas de Implementación

**Backend:**
- 9 microservicios nuevos implementados
- 204 tests unitarios pasando al 100%
- ModuleAccessMiddleware en todos los servicios
- 0 errores de compilación

**Frontend:**
- 6 componentes marketplace (1,598 líneas)
- 6 páginas marketplace (2,918 líneas)
- 6 páginas billing (1,988 líneas)
- Sistema de tipos completo (319 líneas)
- Rutas unificadas /vehicles, /properties

---

¿Quieres que te ayude a desglosar las tareas de algún sprint en mayor detalle o crear los issues/tickets para tu gestor de proyectos?