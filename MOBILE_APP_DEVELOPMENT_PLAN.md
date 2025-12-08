# 📱 Plan de Desarrollo - CarDealer Mobile App (Flutter)

**Versión:** 1.0  
**Fecha:** Diciembre 2025  
**Plataformas:** iOS y Android  
**Framework:** Flutter 3.x  
**Ubicación:** `frontend/mobile/cardealer/`  

---

## ✅ Estado del Proyecto

### Sprint 0: COMPLETADO (100%) ✅
**Infraestructura y Configuración Base**  
15/15 tareas completadas. Ver: `frontend/mobile/cardealer/SPRINT0_COMPLETION_REPORT.md`

### Sprint 1: COMPLETADO (100%) ✅  
**Design System y Componentes Base**  
19 componentes creados, 85+ tests, 4,850+ líneas de código. Ver: `frontend/mobile/cardealer/SPRINT1_COMPLETION_REPORT.md`

### Sprint 2: EN PROGRESO (60%) 🔄  
**Autenticación y Onboarding**  
- ✅ Mock Data Layer (100%)
- ✅ Domain Layer (100%)  
- ✅ BLoC Layer (100%)
- ✅ LoginPage (100%)
- ⏸️ RegisterPage (0%)
- ⏸️ Onboarding (0%)
- ⏸️ Profile Setup (0%)  

Ver: `frontend/mobile/cardealer/SPRINT2_PROGRESS_REPORT.md` y `frontend/mobile/cardealer/MOCK_DATA_STRATEGY.md`  

---

## 📋 Tabla de Contenidos

1. [Resumen Ejecutivo](#resumen-ejecutivo)
2. [Análisis de la Web Actual](#análisis-de-la-web-actual)
3. [Arquitectura Móvil](#arquitectura-móvil)
4. [Sistema de Diseño](#sistema-de-diseño)
5. [Sprints y Tareas](#sprints-y-tareas)
6. [Stack Tecnológico](#stack-tecnológico)
7. [Consideraciones de Performance](#consideraciones-de-performance)

---

## 🎯 Resumen Ejecutivo

### Objetivo
Desarrollar una aplicación móvil nativa para iOS y Android que replique la funcionalidad completa de **CarDealer Web** (frontend/web/cardealer), optimizada para dispositivos móviles con experiencia de usuario superior.

### Alcance
- **Marketplace de vehículos** (cars-only, sin properties/lodging/rentals)
- **7 secciones de monetización** del HomePage
- **Sistema de autenticación** completo (Individual, Dealer, Admin)
- **Panel de Dealer** móvil optimizado
- **Sistema de favoritos y mensajes**
- **Búsqueda y filtros avanzados**
- **Integración con backend existente**

### Filosofía de Diseño
> "mostrando vehiculo porque eso es dinero"

- Máxima densidad de vehículos sin comprometer UX móvil
- Scrolls verticales y horizontales optimizados para touch
- Imágenes optimizadas para mobile (WebP, lazy loading)
- 40% featured ratio mantenido del algoritmo de ranking

---

## 🔍 Análisis de la Web Actual

### Estructura de Páginas Web

#### **Public Routes (No autenticado)**
```
/                        → HomePage (7 secciones de vehículos)
/browse                  → VehicleBrowsePage (lista + filtros)
/compare                 → VehicleComparePage
/sell-your-car           → SellYourCarPage
/listing/:id             → VehicleDetailPage
/map                     → VehicleMapViewPage
```

#### **User Routes (Autenticado)**
```
/wishlist                → WishlistPage
/messages                → MessagesPage
/dashboard               → UserDashboardPage
/profile                 → ProfilePage
```

#### **Dealer Routes (Panel de Dealer)**
```
/dealer/dashboard        → DealerDashboardPage
/dealer/listings         → DealerListingsPage
/dealer/crm              → CRMPage
/dealer/analytics        → AnalyticsPage
```

#### **Admin Routes (Panel Admin)**
```
/admin/dashboard         → AdminDashboardPage
/admin/approvals         → PendingApprovalsPage
/admin/users             → UsersManagementPage
/admin/listings          → AdminListingsPage
/admin/reports           → AdminReportsPage
/admin/settings          → AdminSettingsPage
```

#### **Billing Routes**
```
/billing                 → BillingDashboardPage
/billing/plans           → PlansPage
/billing/invoices        → InvoicesPage
/billing/payments        → PaymentsPage
/billing/checkout        → CheckoutPage
```

#### **Auth Routes**
```
/login                   → LoginPage
/register                → RegisterPage
```

#### **Common Pages**
```
/about                   → AboutPage
/how-it-works           → HowItWorksPage
/pricing                → PricingPage
/faq                    → FAQPage
/contact                → ContactPage
/help                   → HelpCenterPage
/terms                  → TermsPage
/privacy                → PrivacyPage
```

### HomePage - 7 Secciones de Monetización

```typescript
// 71 vehículos totales mostrados en HomePage
1. Hero Carousel (5 vehículos) - Full screen
2. Vehículos Destacados (6 vehículos) - Grid 3x2
3. Destacados de la Semana (10 vehículos) - Scroll horizontal
4. Ofertas del Día (10 vehículos) - Scroll horizontal
5. SUVs y Camionetas (10 vehículos) - Scroll horizontal
6. Vehículos Premium (10 vehículos) - Scroll horizontal
7. Eléctricos e Híbridos (10 vehículos) - Scroll horizontal

// Secciones adicionales
- Features (4 cards)
- How It Works (3 steps)
- CTA Section
```

### Componentes Clave Existentes

```
HeroCarousel           → Carousel principal full-screen
FeaturedListingGrid    → Grid de vehículos destacados
GlobalSearch           → Búsqueda global con dropdown
LanguageSwitcher       → Switch ES/EN
Navbar                 → Navigation bar responsive
DealerSidebar          → Sidebar panel dealer
mockVehicles           → 11 vehículos mock data
rankingAlgorithm       → Algoritmo 40% featured
```

### Sistema de Tipos de Usuario

```typescript
enum AccountType {
  GUEST              // No autenticado
  INDIVIDUAL         // Usuario regular
  DEALER             // Vendedor/Agencia
  DEALER_EMPLOYEE    // Empleado de dealer
  ADMIN              // Administrador
  PLATFORM_EMPLOYEE  // Empleado plataforma
}

enum DealerPlan {
  FREE      // 5 listings, 1 featured
  BASIC     // 20 listings, 3 featured
  PRO       // 200 listings, 10 featured
  ENTERPRISE // Unlimited
}
```

### Colores del Tema

```dart
// Basado en Tailwind CSS de cardealer web
primary: Color(0xFF2563EB),        // blue-600
primaryDark: Color(0xFF1E40AF),    // blue-700
secondary: Color(0xFF10B981),      // emerald-500
accent: Color(0xFFF59E0B),         // amber-500
error: Color(0xFFEF4444),          // red-500
success: Color(0xFF22C55E),        // green-500
warning: Color(0xFFF59E0B),        // amber-500
background: Color(0xFFF9FAFB),     // gray-50
surface: Color(0xFFFFFFFF),        // white
textPrimary: Color(0xFF111827),    // gray-900
textSecondary: Color(0xFF6B7280),  // gray-500
border: Color(0xFFE5E7EB),         // gray-200
```

---

## 🏗️ Arquitectura Móvil

### Clean Architecture + BLoC Pattern

```
lib/
├── core/
│   ├── theme/
│   │   ├── app_theme.dart
│   │   ├── colors.dart
│   │   ├── typography.dart
│   │   └── spacing.dart
│   ├── constants/
│   │   ├── api_constants.dart
│   │   └── app_constants.dart
│   ├── network/
│   │   ├── api_client.dart
│   │   ├── interceptors.dart
│   │   └── error_handler.dart
│   ├── utils/
│   │   ├── formatters.dart
│   │   ├── validators.dart
│   │   └── helpers.dart
│   └── errors/
│       └── failures.dart
├── data/
│   ├── models/
│   │   ├── vehicle_model.dart
│   │   ├── user_model.dart
│   │   ├── dealer_model.dart
│   │   └── subscription_model.dart
│   ├── repositories/
│   │   ├── vehicle_repository_impl.dart
│   │   ├── auth_repository_impl.dart
│   │   └── dealer_repository_impl.dart
│   └── datasources/
│       ├── remote/
│       │   ├── vehicle_remote_datasource.dart
│       │   └── auth_remote_datasource.dart
│       └── local/
│           ├── vehicle_local_datasource.dart (Hive)
│           └── auth_local_datasource.dart (Secure Storage)
├── domain/
│   ├── entities/
│   │   ├── vehicle.dart
│   │   ├── user.dart
│   │   ├── dealer.dart
│   │   └── subscription.dart
│   ├── repositories/
│   │   ├── vehicle_repository.dart
│   │   ├── auth_repository.dart
│   │   └── dealer_repository.dart
│   └── usecases/
│       ├── vehicles/
│       │   ├── get_featured_vehicles.dart
│       │   ├── get_vehicle_details.dart
│       │   ├── search_vehicles.dart
│       │   └── filter_vehicles.dart
│       ├── auth/
│       │   ├── login.dart
│       │   ├── register.dart
│       │   └── logout.dart
│       └── dealer/
│           ├── get_dealer_stats.dart
│           ├── manage_listings.dart
│           └── get_crm_data.dart
├── presentation/
│   ├── bloc/
│   │   ├── auth/
│   │   │   ├── auth_bloc.dart
│   │   │   ├── auth_event.dart
│   │   │   └── auth_state.dart
│   │   ├── vehicles/
│   │   │   ├── vehicles_bloc.dart
│   │   │   ├── vehicles_event.dart
│   │   │   └── vehicles_state.dart
│   │   ├── dealer/
│   │   │   ├── dealer_bloc.dart
│   │   │   ├── dealer_event.dart
│   │   │   └── dealer_state.dart
│   │   └── favorites/
│   │       ├── favorites_bloc.dart
│   │       ├── favorites_event.dart
│   │       └── favorites_state.dart
│   ├── pages/
│   │   ├── home/
│   │   │   ├── home_page.dart
│   │   │   └── widgets/
│   │   │       ├── hero_carousel.dart
│   │   │       ├── featured_grid.dart
│   │   │       ├── featured_section.dart
│   │   │       ├── features_section.dart
│   │   │       └── how_it_works_section.dart
│   │   ├── browse/
│   │   │   ├── browse_page.dart
│   │   │   └── widgets/
│   │   │       ├── filter_bottom_sheet.dart
│   │   │       ├── vehicle_list_item.dart
│   │   │       └── sort_dropdown.dart
│   │   ├── detail/
│   │   │   ├── vehicle_detail_page.dart
│   │   │   └── widgets/
│   │   │       ├── image_gallery.dart
│   │   │       ├── specs_section.dart
│   │   │       ├── seller_info.dart
│   │   │       └── contact_actions.dart
│   │   ├── dealer/
│   │   │   ├── dealer_dashboard_page.dart
│   │   │   ├── dealer_listings_page.dart
│   │   │   ├── dealer_crm_page.dart
│   │   │   └── dealer_analytics_page.dart
│   │   ├── auth/
│   │   │   ├── login_page.dart
│   │   │   └── register_page.dart
│   │   └── profile/
│   │       ├── profile_page.dart
│   │       ├── wishlist_page.dart
│   │       └── messages_page.dart
│   └── widgets/
│       ├── vehicle_card.dart
│       ├── custom_app_bar.dart
│       ├── loading_indicator.dart
│       ├── error_widget.dart
│       ├── empty_state.dart
│       └── bottom_nav_bar.dart
├── l10n/
│   ├── app_en.arb
│   └── app_es.arb
└── main.dart
```

---

## 🎨 Sistema de Diseño

### Paleta de Colores (Material Theme)

```dart
// lib/core/theme/colors.dart
class AppColors {
  // Primary - Blue theme (cars)
  static const primary = Color(0xFF2563EB);        // blue-600
  static const primaryDark = Color(0xFF1E40AF);    // blue-700
  static const primaryLight = Color(0xFF3B82F6);   // blue-500
  
  // Secondary - Emerald (success states)
  static const secondary = Color(0xFF10B981);      // emerald-500
  static const secondaryDark = Color(0xFF059669);  // emerald-600
  
  // Accent - Amber (featured, highlights)
  static const accent = Color(0xFFF59E0B);         // amber-500
  static const accentDark = Color(0xFFD97706);     // amber-600
  
  // Semantic colors
  static const error = Color(0xFFEF4444);          // red-500
  static const success = Color(0xFF22C55E);        // green-500
  static const warning = Color(0xFFF59E0B);        // amber-500
  static const info = Color(0xFF3B82F6);           // blue-500
  
  // Neutrals
  static const background = Color(0xFFF9FAFB);     // gray-50
  static const surface = Color(0xFFFFFFFF);        // white
  static const surfaceVariant = Color(0xFFF3F4F6); // gray-100
  
  // Text colors
  static const textPrimary = Color(0xFF111827);    // gray-900
  static const textSecondary = Color(0xFF6B7280);  // gray-500
  static const textTertiary = Color(0xFF9CA3AF);   // gray-400
  static const textDisabled = Color(0xFFD1D5DB);   // gray-300
  
  // Borders
  static const border = Color(0xFFE5E7EB);         // gray-200
  static const divider = Color(0xFFF3F4F6);        // gray-100
  
  // Dealer plan badges
  static const planFree = Color(0xFFD1D5DB);       // gray-300
  static const planBasic = Color(0xFF34D399);      // emerald-400
  static const planPro = Color(0xFF3B82F6);        // blue-500
  static const planEnterprise = Color(0xFF9333EA); // purple-600
}
```

### Tipografía

```dart
// lib/core/theme/typography.dart
class AppTypography {
  static const fontFamily = 'Inter'; // or 'SF Pro' for iOS
  
  // Headings
  static const h1 = TextStyle(
    fontSize: 32,
    fontWeight: FontWeight.w700,
    height: 1.2,
    letterSpacing: -0.5,
  );
  
  static const h2 = TextStyle(
    fontSize: 24,
    fontWeight: FontWeight.w700,
    height: 1.3,
  );
  
  static const h3 = TextStyle(
    fontSize: 20,
    fontWeight: FontWeight.w600,
    height: 1.4,
  );
  
  static const h4 = TextStyle(
    fontSize: 18,
    fontWeight: FontWeight.w600,
    height: 1.4,
  );
  
  // Body
  static const bodyLarge = TextStyle(
    fontSize: 16,
    fontWeight: FontWeight.w400,
    height: 1.5,
  );
  
  static const bodyMedium = TextStyle(
    fontSize: 14,
    fontWeight: FontWeight.w400,
    height: 1.5,
  );
  
  static const bodySmall = TextStyle(
    fontSize: 12,
    fontWeight: FontWeight.w400,
    height: 1.4,
  );
  
  // Labels
  static const labelLarge = TextStyle(
    fontSize: 14,
    fontWeight: FontWeight.w600,
    height: 1.4,
  );
  
  static const labelMedium = TextStyle(
    fontSize: 12,
    fontWeight: FontWeight.w600,
    height: 1.4,
  );
}
```

### Espaciado

```dart
// lib/core/theme/spacing.dart
class AppSpacing {
  static const xs = 4.0;
  static const sm = 8.0;
  static const md = 16.0;
  static const lg = 24.0;
  static const xl = 32.0;
  static const xxl = 48.0;
  
  // Card spacing
  static const cardPadding = 16.0;
  static const cardMargin = 12.0;
  static const cardRadius = 12.0;
  
  // Section spacing (Amazon-style compact)
  static const sectionVertical = 24.0; // py-6 in web
  static const sectionHorizontal = 16.0;
}
```

---

## 📅 Sprints y Tareas

### **SPRINT 0: Setup y Fundamentos** (1 semana) ✅ COMPLETADO 100%

#### Objetivos
- ✅ Configurar proyecto Flutter
- ✅ Establecer arquitectura base
- ✅ Configurar CI/CD pipeline

#### Tareas

**Setup del Proyecto**
- [x] Crear proyecto Flutter con estructura Clean Architecture
- [x] Configurar pubspec.yaml con dependencias base
- [x] Setup Flutter Flavors (dev, staging, prod) - AppConfig + main_*.dart
- [x] Configurar Android (build.gradle, AndroidManifest) - Product Flavors + Permisos
- [x] Configurar iOS (Info.plist, Podfile) - Permisos + Deep Links
- [x] Setup Firebase (Analytics, Crashlytics, Remote Config) - FirebaseService implementado

**Arquitectura Base**
- [x] Implementar core/network (Dio + Interceptors)
- [x] Implementar core/storage (Hive + Secure Storage)
- [x] Setup dependency injection (get_it)
- [x] Implementar error handling global
- [x] Crear base repository pattern
- [x] Implementar logging system

**CI/CD**
- [x] Setup GitHub Actions workflow - flutter-ci-cd.yml
- [x] Configurar Fastlane para iOS - 7 lanes configuradas
- [x] Configurar Fastlane para Android - 6 lanes configuradas
- [x] Configurar distribución TestFlight/Firebase App Distribution

**Deliverables**
- ✅ Proyecto base corriendo en iOS y Android
- ✅ CI/CD pipeline funcional
- ✅ Documentación de setup (4 guías completas)

**Archivos creados:** 25 archivos (~2,170 líneas)
**Documentación:** SPRINT0_COMPLETION_REPORT.md, FIREBASE_SETUP.md, IOS_FLAVORS_CONFIG.md, FASTLANE_SETUP.md

---

### **SPRINT 1: Sistema de Diseño y Componentes Base** (2 semanas) ✅ COMPLETADO 100%

#### Objetivos
- ✅ Implementar sistema de diseño completo
- ✅ Crear componentes reutilizables
- ✅ Preparar assets y recursos

#### Tareas

**Theme System** (5/5) ✅
- [x] Implementar AppTheme con light/dark mode (231 lines)
- [x] Crear AppColors con toda la paleta (110 lines)
- [x] Implementar AppTypography (156 lines)
- [x] Configurar AppSpacing constants (27 lines)
- [x] Configurar AppRadius constants (27 lines)

**Assets y Recursos** (5/5) ✅
- [x] AppIcons constants (40 lines)
- [x] AppImages constants (35 lines)
- [x] Illustrations directory preparado
- [x] Fonts configurados en pubspec
- [x] Assets registrados en pubspec

**Componentes Base** (11/11) ✅
- [x] CustomButton (filled, outlined, text) - 273 lines
- [x] CustomTextField (filled, outlined, underlined) - 267 lines
- [x] CustomLoadingIndicator (circular, linear, custom) - 169 lines
- [x] CustomCard - 139 lines
- [x] CustomBottomSheet (modal, persistent) - 165 lines
- [x] CustomDialog (info, success, warning, error) - 207 lines
- [x] CustomEmptyState - 143 lines
- [x] CustomBottomNavBar con badges - 148 lines
- [x] CustomSnackBar/Toast (4 tipos) - 299 lines
- [x] CustomChip/Badge/Tag (3 variantes) - 306 lines
- [x] CustomAvatar con AvatarGroup - 323 lines

**Card Components** (4/6) ✅
- [x] VehicleCard (list view) - 315 lines
- [x] VehicleGridCard (grid view) - 202 lines
- [x] VehicleDetailCard (detail view) - 298 lines
- [x] PriceTag (4 variantes) - 355 lines
- [ ] RatingStars widget - DEFERRED to Sprint 3
- [ ] LocationChip widget - DEFERRED to Sprint 3

**Utilities** (7/7) ✅
- [x] Validators (email, phone, required, etc.) - 90 lines
- [x] Formatters (currency, date, phone, distance) - 83 lines
- [x] String extensions - 45 lines
- [x] Date extensions - 38 lines
- [x] Context extensions - 57 lines
- [x] App constants - 50 lines
- [x] Assets constants - 75 lines

**Testing** ✅
- [x] Widget tests para todos los componentes (85+ tests)
- [x] Test coverage >90%

**Deliverables** ✅
- [x] Widgetbook con todos los componentes (260 lines)
- [x] Documentación completa de componentes
- [x] Sprint 1 Completion Report (SPRINT1_MOBILE_COMPLETION_REPORT.md)

**Totals:**
- **Files Created**: 30+ files
- **Lines of Code**: ~4,850 lines
- **Components**: 19 total (15 base + 4 vehicle cards)
- **Test Cases**: 85+ tests
- **Status**: ✅ **100% COMPLETE**

---

### **SPRINT 2: Autenticación y Onboarding** (2 semanas)

#### Objetivos
- Implementar flujo completo de auth
- Crear onboarding experience
- Integrar con backend de auth

#### Tareas

**Domain Layer - Auth**
- [ ] Crear User entity
- [ ] Crear AccountType enum
- [ ] Definir AuthRepository interface
- [ ] Implementar Login use case
- [ ] Implementar Register use case
- [ ] Implementar Logout use case
- [ ] Implementar CheckAuthStatus use case

**Data Layer - Auth**
- [ ] Implementar AuthRemoteDataSource
- [ ] Implementar AuthLocalDataSource (tokens)
- [ ] Crear UserModel + JSON serialization
- [ ] Implementar AuthRepositoryImpl
- [ ] Configurar secure storage para tokens
- [ ] Implementar token refresh logic

**Presentation Layer - Auth**
- [ ] Crear AuthBloc con estados (authenticated, unauthenticated, loading)
- [ ] Implementar LoginPage UI
- [ ] Implementar RegisterPage UI
- [ ] Crear AccountTypeSelector widget
- [ ] Implementar ForgotPasswordPage
- [ ] Crear SocialAuthButtons (Google, Apple, Facebook)

**Onboarding**
- [ ] Crear OnboardingPage con PageView
- [ ] Diseñar 3 screens onboarding (cars-focused)
- [ ] Implementar skip/next logic
- [ ] Guardar onboarding completion en local storage
- [ ] Crear SplashScreen con logo animation

**Navigation Guards**
- [ ] Implementar AuthGuard middleware
- [ ] Crear route generator con auth checks
- [ ] Setup deep linking básico

**Deliverables**
- ✅ Auth flow completo funcional
- ✅ Onboarding experience
- ✅ Tests unitarios para auth

---

### **SPRINT 3: HomePage - Secciones de Monetización** (3 semanas)

#### Objetivos
- Implementar HomePage con 7 secciones
- Replicar exactamente estructura web
- Optimizar performance para móvil

#### Tareas

**Domain Layer - Vehicles**
- [ ] Crear Vehicle entity (completa con todos los campos)
- [ ] Crear VehicleRepository interface
- [ ] Implementar GetFeaturedVehicles use case
- [ ] Implementar GetVehiclesByCategory use case
- [ ] Crear RankingAlgorithm utility (40% featured)

**Data Layer - Vehicles**
- [ ] Crear VehicleModel con JSON serialization
- [ ] Implementar VehicleRemoteDataSource
- [ ] Implementar VehicleLocalDataSource (cache con Hive)
- [ ] Implementar VehicleRepositoryImpl
- [ ] Setup cache strategy (stale-while-revalidate)

**Presentation Layer - HomePage**
- [ ] Crear VehiclesBloc
- [ ] Implementar HomePage scaffold

**Section 1: Hero Carousel** (5 vehículos)
- [ ] Crear HeroCarouselWidget
- [ ] Implementar PageView con auto-play
- [ ] Agregar dot indicators
- [ ] Implementar swipe gestures
- [ ] Optimizar imágenes (cached_network_image)

**Section 2: Featured Grid** (6 vehículos - Grid 2 columnas)
- [ ] Implementar FeaturedGridSection
- [ ] Crear VehicleGridCard widget
- [ ] Implementar featured badge overlay
- [ ] Agregar "Ver todo" navigation

**Sections 3-7: Horizontal Scrollable** (10 vehículos c/u)
- [ ] Crear FeaturedSectionWidget reusable
- [ ] Implementar horizontal ListView.builder
- [ ] Crear VehicleCardHorizontal
- [ ] Agregar scroll physics optimizado
- [ ] Implementar lazy loading de imágenes

**Additional Sections**
- [ ] FeaturesSection (4 cards)
- [ ] HowItWorksSection (3 steps)
- [ ] CTASection

**Performance Optimizations**
- [ ] Implementar image caching strategy
- [ ] Setup lazy loading para todas las sections
- [ ] Optimizar scroll performance
- [ ] Implementar skeleton loaders
- [ ] Setup analytics tracking por section

**Deliverables**
- ✅ HomePage completo con 71 vehículos
- ✅ Performance 60fps en scroll
- ✅ Tests de integración

---

### **SPRINT 4: Browse y Filtros** (2 semanas)

#### Objetivos
- Implementar página de browse con filtros
- Sistema de búsqueda avanzada
- Sort y pagination

#### Tareas

**Domain Layer**
- [ ] Crear FilterCriteria entity
- [ ] Implementar SearchVehicles use case
- [ ] Implementar FilterVehicles use case
- [ ] Implementar SortVehicles use case

**Data Layer**
- [ ] Implementar query builder para filtros
- [ ] Setup pagination (cursor-based)
- [ ] Implementar debounce para search

**Presentation Layer - Browse**
- [ ] Crear FilterBloc
- [ ] Implementar BrowsePage UI
- [ ] Crear VehicleListView (vertical)
- [ ] Implementar pull-to-refresh
- [ ] Setup infinite scroll

**Filter System**
- [ ] Crear FilterBottomSheet modal
- [ ] Implementar PriceRangeSlider
- [ ] Crear YearRangePicker
- [ ] Implementar MakeModelSelector (hierarchical)
- [ ] Crear BodyTypeChips
- [ ] Implementar FuelTypeSelector
- [ ] Crear TransmissionSelector
- [ ] Implementar LocationFilter
- [ ] Agregar "Limpiar filtros" button
- [ ] Implementar filter chip display en lista

**Search**
- [ ] Crear SearchBar con autocomplete
- [ ] Implementar RecentSearches local storage
- [ ] Crear SearchSuggestions dropdown
- [ ] Agregar voice search (speech_to_text)

**Sort Options**
- [ ] Implementar SortDropdown
- [ ] Opciones: Relevancia, Precio (asc/desc), Año, Km
- [ ] Guardar last sort preference

**Deliverables**
- ✅ Browse page completa
- ✅ Sistema de filtros funcional
- ✅ Search con autocomplete

---

### **SPRINT 5: Vehicle Detail Page** (2 semanas)

#### Objetivos
- Página de detalle completa
- Galería de imágenes optimizada
- Información de seller y contacto

#### Tareas

**Domain Layer**
- [ ] Implementar GetVehicleDetail use case
- [ ] Crear ContactSeller use case
- [ ] Implementar AddToFavorites use case

**Presentation Layer - Detail**
- [ ] Crear VehicleDetailBloc
- [ ] Implementar VehicleDetailPage scaffold

**Image Gallery**
- [ ] Crear ImageGalleryWidget con PageView
- [ ] Implementar pinch-to-zoom
- [ ] Agregar fullscreen mode
- [ ] Implementar dot indicators
- [ ] Setup hero animation desde lista

**Vehicle Info Sections**
- [ ] Header con precio y título
- [ ] Specs section (grid de specs)
- [ ] Features list (checkmarks)
- [ ] Description expandable
- [ ] Location map preview
- [ ] Vehicle history (si aplica)

**Seller Info**
- [ ] Seller card widget
- [ ] Rating stars display
- [ ] Dealer badge (si aplica)
- [ ] Verification badge
- [ ] "View profile" link

**Contact Actions**
- [ ] Sticky bottom bar con acciones
- [ ] Call button (url_launcher)
- [ ] WhatsApp button
- [ ] Message button
- [ ] Share button (share_plus)
- [ ] Favorite toggle button

**Similar Vehicles**
- [ ] Sección "Vehículos similares"
- [ ] Horizontal scroll de 10 vehicles
- [ ] Based on make/model/price range

**Deliverables**
- ✅ Detail page completa
- ✅ Hero animations
- ✅ Contact funcional

---

### **SPRINT 6: User Profile y Favoritos** (2 semanas)

#### Objetivos
- Perfil de usuario editable
- Sistema de favoritos
- Historial de búsquedas

#### Tareas

**Domain Layer - User**
- [ ] Implementar UpdateProfile use case
- [ ] Implementar GetFavorites use case
- [ ] Implementar ToggleFavorite use case
- [ ] Implementar GetSearchHistory use case

**Data Layer**
- [ ] Implementar UserRemoteDataSource
- [ ] Implementar FavoritesLocalDataSource
- [ ] Setup sync strategy (online/offline)

**Presentation Layer - Profile**
- [ ] Crear ProfileBloc
- [ ] Implementar ProfilePage UI
- [ ] Crear EditProfilePage
- [ ] Implementar avatar upload (image_picker)
- [ ] Crear settings page

**Favorites**
- [ ] Crear FavoritesBloc
- [ ] Implementar FavoritesPage (grid view)
- [ ] Agregar swipe-to-delete
- [ ] Implementar empty state
- [ ] Setup offline favorites sync

**Wishlist Features**
- [ ] Agregar price alerts
- [ ] Notificaciones de price drop
- [ ] Compartir favoritos

**Search History**
- [ ] Implementar SearchHistoryPage
- [ ] Guardar búsquedas recientes
- [ ] Clear history option

**Deliverables**
- ✅ Profile completo editable
- ✅ Favoritos con sync
- ✅ Search history funcional

---

### **SPRINT 7: Mensajería y Notificaciones** (2 semanas)

#### Objetivos
- Sistema de mensajería in-app
- Push notifications
- Real-time chat

#### Tareas

**Domain Layer - Messaging**
- [ ] Crear Message entity
- [ ] Crear Conversation entity
- [ ] Implementar GetConversations use case
- [ ] Implementar SendMessage use case
- [ ] Implementar GetMessages use case

**Data Layer**
- [ ] Implementar MessagingRemoteDataSource
- [ ] Setup WebSocket connection
- [ ] Implementar message caching local
- [ ] Setup Firebase Cloud Messaging

**Presentation Layer - Messages**
- [ ] Crear MessagingBloc
- [ ] Implementar ConversationsPage (lista)
- [ ] Implementar ChatPage (1-on-1)
- [ ] Crear MessageBubble widget
- [ ] Implementar typing indicator
- [ ] Agregar read receipts

**Real-time Features**
- [ ] Setup WebSocket listener
- [ ] Implementar presence system (online/offline)
- [ ] Agregar typing indicator real-time
- [ ] Implementar message delivery status

**Push Notifications**
- [ ] Setup FCM en Android
- [ ] Setup APNs en iOS
- [ ] Implementar notification handler
- [ ] Crear notification permission request
- [ ] Setup deep linking desde notificación
- [ ] Implementar notification badges

**Media Sharing**
- [ ] Image sharing en chat
- [ ] Video sharing
- [ ] Document sharing
- [ ] Location sharing

**Deliverables**
- ✅ Chat real-time funcional
- ✅ Push notifications
- ✅ Media sharing

---

### **SPRINT 8: Dealer Panel Mobile** (3 semanas)

#### Objetivos
- Panel de dealer optimizado para móvil
- Dashboard con métricas
- Gestión de listings

#### Tareas

**Domain Layer - Dealer**
- [ ] Crear DealerStats entity
- [ ] Implementar GetDealerStats use case
- [ ] Implementar GetDealerListings use case
- [ ] Implementar ManageListing use case (create/edit/delete)
- [ ] Implementar GetCRMData use case

**Data Layer**
- [ ] Implementar DealerRemoteDataSource
- [ ] Setup dealer-specific endpoints

**Presentation Layer - Dealer Dashboard**
- [ ] Crear DealerBloc
- [ ] Implementar DealerDashboardPage

**Dashboard Widgets**
- [ ] Stats cards (views, leads, conversions)
- [ ] Revenue chart (fl_chart)
- [ ] Recent activity feed
- [ ] Plan usage indicators
- [ ] Quick actions grid

**Dealer Listings Page**
- [ ] Lista de listings activos
- [ ] Filtro por status (active, pending, sold)
- [ ] Swipe actions (edit, delete, duplicate)
- [ ] Bulk actions
- [ ] Sort options

**Create/Edit Listing**
- [ ] Multi-step form
- [ ] Image upload (multi-select)
- [ ] Specs form con validators
- [ ] Pricing setup
- [ ] Location picker
- [ ] Preview before publish
- [ ] Draft system

**CRM Mobile**
- [ ] Leads pipeline view
- [ ] Lead detail page
- [ ] Quick actions (call, message, schedule)
- [ ] Notes system
- [ ] Lead status update

**Analytics Mobile**
- [ ] Simplified charts
- [ ] Key metrics cards
- [ ] Date range picker
- [ ] Export option

**Deliverables**
- ✅ Dealer panel completo
- ✅ Listing management
- ✅ Mobile CRM básico

---

### **SPRINT 9: Maps y Geolocation** (1 semana)

#### Objetivos
- Integrar mapas con listings
- Búsqueda por ubicación
- Filtro geográfico

#### Tareas

**Maps Integration**
- [ ] Setup Google Maps SDK (Android/iOS)
- [ ] Implementar MapViewPage
- [ ] Crear VehicleMapMarker custom
- [ ] Cluster markers optimization
- [ ] Implementar info window custom

**Geolocation**
- [ ] Setup location permissions
- [ ] Implementar GetCurrentLocation
- [ ] Crear LocationPicker widget
- [ ] Implementar address autocomplete
- [ ] Reverse geocoding

**Map Features**
- [ ] Filter by radius
- [ ] Draw circle/polygon filters
- [ ] Show user location
- [ ] Directions to seller
- [ ] Street View integration

**Deliverables**
- ✅ Map view funcional
- ✅ Location-based search
- ✅ Geofencing

---

### **SPRINT 10: Offline Support y Sync** (1 semana)

#### Objetivos
- Soporte offline completo
- Sync automático
- Cache inteligente

#### Tareas

**Offline Architecture**
- [ ] Implementar connectivity_plus listener
- [ ] Setup offline-first strategy
- [ ] Implementar sync queue
- [ ] Crear offline indicator UI

**Data Sync**
- [ ] Implementar background sync
- [ ] Setup conflict resolution
- [ ] Crear sync status indicators
- [ ] Implementar manual sync trigger

**Offline Features**
- [ ] Cache de vehículos vistos
- [ ] Offline favorites
- [ ] Draft messages queue
- [ ] Search history offline

**Deliverables**
- ✅ App funcional offline
- ✅ Auto-sync cuando vuelve online

---

### **SPRINT 11: Payments y Billing** (2 semanas)

#### Objetivos
- Integrar sistema de pagos
- Gestión de suscripciones
- In-app purchases

#### Tareas

**Payment Integration**
- [ ] Setup Stripe SDK
- [ ] Implementar payment flow
- [ ] Crear PaymentMethodsPage
- [ ] Implementar card input
- [ ] Setup 3D Secure

**Subscriptions**
- [ ] Implementar PlansPage móvil
- [ ] Crear PlanComparisonWidget
- [ ] Setup in-app subscriptions (iOS/Android)
- [ ] Implementar upgrade/downgrade flow
- [ ] Crear billing history page

**Invoice Management**
- [ ] Lista de facturas
- [ ] PDF viewer/download
- [ ] Payment status tracking

**Deliverables**
- ✅ Pagos funcionales
- ✅ Subscriptions activas

---

### **SPRINT 12: Performance y Optimización** (1 semana)

#### Objetivos
- Optimizar performance general
- Reducir tamaño de app
- Mejorar tiempo de carga

#### Tareas

**Performance Audit**
- [ ] Flutter DevTools profiling
- [ ] Identificar bottlenecks
- [ ] Memory leak detection
- [ ] Network calls optimization

**Image Optimization**
- [ ] Implementar WebP format
- [ ] Setup progressive loading
- [ ] Thumbnail generation
- [ ] LazyLoad optimization

**App Size Optimization**
- [ ] Code splitting
- [ ] Asset optimization
- [ ] Remove unused dependencies
- [ ] Enable Dart obfuscation

**Loading Performance**
- [ ] Optimize app startup time
- [ ] Reduce time-to-interactive
- [ ] Improve scroll performance
- [ ] Optimize animations (60fps)

**Deliverables**
- ✅ App < 50MB
- ✅ Startup < 3s
- ✅ 60fps scrolls

---

### **SPRINT 13: Testing y QA** (2 semanas)

#### Objetivos
- Cobertura de tests completa
- Testing en devices reales
- Bug fixing

#### Tareas

**Unit Tests**
- [ ] Tests para todos los use cases
- [ ] Tests para repositories
- [ ] Tests para BLoCs
- [ ] Target: 80% coverage

**Widget Tests**
- [ ] Tests para componentes base
- [ ] Tests para páginas principales
- [ ] Golden tests para UI consistency

**Integration Tests**
- [ ] E2E test flows principales
- [ ] Auth flow test
- [ ] Browse y detail flow
- [ ] Dealer panel flow

**Device Testing**
- [ ] Test en iOS (iPhone 12+, iPad)
- [ ] Test en Android (Samsung, Pixel, Xiaomi)
- [ ] Test diferentes tamaños de pantalla
- [ ] Test en tablets

**Bug Fixing**
- [ ] Fix critical bugs
- [ ] Fix UI inconsistencies
- [ ] Performance issues
- [ ] Crash fixes

**Deliverables**
- ✅ 80% test coverage
- ✅ 0 critical bugs
- ✅ QA approval

---

### **SPRINT 14: Deploy y Monitoring** (1 semana)

#### Objetivos
- Publicar en App Store y Play Store
- Setup monitoring y analytics
- Documentación final

#### Tareas

**App Store Preparation**
- [ ] Crear App Store Connect listing
- [ ] Screenshots y preview videos
- [ ] App description (ES/EN)
- [ ] Privacy policy y términos
- [ ] Submit para review

**Play Store Preparation**
- [ ] Crear Play Console listing
- [ ] Store assets (screenshots, videos)
- [ ] App description (ES/EN)
- [ ] Submit para review

**Monitoring Setup**
- [ ] Firebase Analytics completo
- [ ] Crashlytics configurado
- [ ] Custom events tracking
- [ ] Performance monitoring
- [ ] Setup alerts

**Documentation**
- [ ] README completo
- [ ] Architecture documentation
- [ ] API documentation
- [ ] Deployment guide
- [ ] Troubleshooting guide

**Deliverables**
- ✅ App en stores
- ✅ Monitoring activo
- ✅ Docs completas

---

## 🛠️ Stack Tecnológico

### Core Dependencies

```yaml
dependencies:
  flutter: sdk: flutter
  
  # State Management
  flutter_bloc: ^8.1.3
  equatable: ^2.0.5
  
  # Dependency Injection
  get_it: ^7.6.4
  injectable: ^2.3.2
  
  # Navigation
  go_router: ^12.1.1
  
  # Network
  dio: ^5.4.0
  connectivity_plus: ^5.0.2
  pretty_dio_logger: ^1.3.1
  
  # Local Storage
  hive: ^2.2.3
  hive_flutter: ^1.1.0
  flutter_secure_storage: ^9.0.0
  shared_preferences: ^2.2.2
  
  # Image Handling
  cached_network_image: ^3.3.0
  image_picker: ^1.0.5
  flutter_cache_manager: ^3.3.1
  
  # UI Components
  shimmer: ^3.0.0
  flutter_svg: ^2.0.9
  lottie: ^2.7.0
  
  # Maps
  google_maps_flutter: ^2.5.0
  geolocator: ^10.1.0
  geocoding: ^2.1.1
  
  # Internationalization
  flutter_localizations: sdk: flutter
  intl: ^0.18.1
  
  # Firebase
  firebase_core: ^2.24.2
  firebase_analytics: ^10.7.4
  firebase_crashlytics: ^3.4.8
  firebase_messaging: ^14.7.9
  firebase_remote_config: ^4.3.8
  
  # Utilities
  url_launcher: ^6.2.2
  share_plus: ^7.2.1
  package_info_plus: ^5.0.1
  device_info_plus: ^9.1.1
  permission_handler: ^11.1.0
  
  # Date & Time
  intl: ^0.18.1
  timeago: ^3.6.0
  
  # Charts
  fl_chart: ^0.65.0
  
  # Video Player
  video_player: ^2.8.1
  
  # WebView
  webview_flutter: ^4.4.2
  
  # Payments
  flutter_stripe: ^10.1.0
  
  # Biometrics
  local_auth: ^2.1.7

dev_dependencies:
  flutter_test: sdk: flutter
  flutter_lints: ^3.0.1
  
  # Code Generation
  build_runner: ^2.4.7
  injectable_generator: ^2.4.1
  hive_generator: ^2.0.1
  
  # Testing
  mocktail: ^1.0.1
  bloc_test: ^9.1.5
  
  # Icons
  flutter_launcher_icons: ^0.13.1
```

### Folder Structure Summary

```
cardealer_mobile/
├── android/           # Android native code
├── ios/              # iOS native code
├── lib/
│   ├── core/         # Core utilities, theme, network
│   ├── data/         # Data layer (models, repositories impl)
│   ├── domain/       # Domain layer (entities, use cases)
│   ├── presentation/ # UI layer (pages, widgets, BLoCs)
│   ├── l10n/         # Internationalization
│   └── main.dart     # Entry point
├── test/             # Unit & widget tests
├── integration_test/ # E2E tests
├── assets/           # Images, fonts, etc
└── pubspec.yaml      # Dependencies
```

---

## 🚀 Consideraciones de Performance

### Image Optimization

```dart
// Usar CachedNetworkImage con placeholder y error widgets
CachedNetworkImage(
  imageUrl: vehicle.imageUrl,
  placeholder: (context, url) => ShimmerWidget(),
  errorWidget: (context, url, error) => Icon(Icons.error),
  memCacheHeight: 400, // Limit memory usage
  memCacheWidth: 600,
  maxHeightDiskCache: 800,
  maxWidthDiskCache: 1200,
)
```

### List Performance

```dart
// Usar ListView.builder para listas largas
ListView.builder(
  itemCount: vehicles.length,
  itemBuilder: (context, index) {
    return VehicleCard(vehicle: vehicles[index]);
  },
  cacheExtent: 1000, // Cache items fuera de viewport
)
```

### Network Optimization

```dart
// Implementar debounce en search
final debouncer = Debouncer(milliseconds: 500);
debouncer.run(() => _performSearch(query));

// Cache strategy
final cacheOptions = CacheOptions(
  store: HiveCacheStore(path),
  policy: CachePolicy.refreshForceCache,
  maxStale: Duration(days: 7),
);
```

### State Management Best Practices

```dart
// Usar Equatable para comparación eficiente
class VehiclesState extends Equatable {
  final List<Vehicle> vehicles;
  final bool isLoading;
  
  @override
  List<Object?> get props => [vehicles, isLoading];
}

// Avoid rebuilds innecesarios con BlocBuilder selector
BlocBuilder<VehiclesBloc, VehiclesState>(
  buildWhen: (previous, current) => 
    previous.vehicles != current.vehicles,
  builder: (context, state) => VehicleList(state.vehicles),
)
```

---

## 📊 Métricas de Éxito

### Performance Targets

- **App Size:** < 50MB
- **Startup Time:** < 3s (cold start)
- **Time to Interactive:** < 2s
- **Frame Rate:** 60fps (consistent)
- **API Response:** < 500ms (p95)
- **Image Load:** < 1s (cached)

### Quality Targets

- **Test Coverage:** > 80%
- **Crash-free Rate:** > 99.5%
- **ANR Rate:** < 0.1%
- **Network Success Rate:** > 98%

### User Experience Targets

- **User Retention (D1):** > 40%
- **User Retention (D7):** > 20%
- **Session Duration:** > 5 min
- **Conversion Rate:** > 3%

---

## 📝 Notas Finales

### Prioridades

1. **Performance First:** La app debe ser más rápida que la web
2. **Offline Support:** Usuario debe poder ver favoritos/cache offline
3. **Push Notifications:** Critical para engagement
4. **Native Feel:** Debe sentirse nativa, no como web wrapper

### Fases de Rollout

**Fase 1: Beta Cerrada** (100 usuarios)
- TestFlight (iOS) + Firebase App Distribution (Android)
- Recoger feedback inicial
- Fix bugs críticos

**Fase 2: Beta Abierta** (1,000 usuarios)
- Expandir a más usuarios
- A/B testing de features
- Performance monitoring

**Fase 3: Launch Público**
- Full launch en stores
- Marketing campaign
- Monitor analytics closely

### Equipo Sugerido

- **1 iOS Developer** (Flutter + native iOS)
- **1 Android Developer** (Flutter + native Android)
- **1 Backend Developer** (adaptar APIs si necesario)
- **1 QA Engineer** (testing)
- **1 UI/UX Designer** (mobile-specific designs)
- **1 Project Manager**

### Timeframe Total

**14 Sprints × 1-2 semanas = ~5-6 meses**

---

## 🎯 Conclusión

Esta planificación cubre el desarrollo completo de la app móvil CarDealer para iOS y Android usando Flutter, replicando toda la funcionalidad de `frontend/web/cardealer` con optimizaciones específicas para móvil.

La app mantendrá la filosofía "mostrando vehiculo porque eso es dinero" maximizando la densidad de vehículos mientras provee una experiencia móvil superior con scrolls optimizados, imágenes lazy-loaded, y offline-first architecture.

**¿Listo para empezar con el Sprint 0?** 🚀
