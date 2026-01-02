# 🏪 Marketplace Multi-Vertical - Guía de Diseño

## 📋 Resumen Ejecutivo

Transformación de tienda de dealer único (vehículos) a **Marketplace Multi-Vertical** que incluye:
- 🚗 **Vehículos** (existente)
- 🏠 **Casas** (nuevo)
- 🏢 **Apartamentos** (nuevo)

### Principios de Diseño

| Principio | Descripción | Ejemplo |
|-----------|-------------|---------|
| **Category-First** | Usuario elige vertical primero | Pills de categoría en hero |
| **Progressive Disclosure** | Información gradual | "Ver más" en descripciones |
| **Visual Hierarchy** | Espacios blancos, tipografía clara | Cards con breathing room |
| **Unified Experience** | Mismo patrón en todas las verticales | Filtros consistentes |
| **Mobile-First** | Diseñado para mobile primero | Bottom nav, swipe gestures |

---

## 🎨 Sistema de Diseño

### Paleta de Colores por Vertical

```scss
// Vehículos (existente)
$vehicles-primary: #2563eb;    // Azul confianza
$vehicles-accent: #3b82f6;

// Bienes Raíces
$realestate-primary: #059669;  // Verde hogar
$realestate-accent: #10b981;

// Marketplace General
$marketplace-neutral: #1f2937; // Gris oscuro
$marketplace-bg: #f9fafb;      // Gris muy claro
```

### Iconografía por Categoría

```
🚗 Vehículos    → Car icon (heroicons/car)
🏠 Casas        → Home icon (heroicons/home)
🏢 Apartamentos → Building icon (heroicons/building-office)
🏞️ Terrenos     → Map icon (heroicons/map)
🏪 Locales      → Storefront icon (heroicons/building-storefront)
```

---

## 🖼️ Wireframes

### 1. Homepage - Hero Section

```
┌─────────────────────────────────────────────────────────────────────┐
│                                                                     │
│  ╔═══════════════════════════════════════════════════════════════╗  │
│  ║                                                               ║  │
│  ║           Encuentra lo que buscas                             ║  │
│  ║                                                               ║  │
│  ║   ┌──────────────┐  ┌──────────────┐  ┌──────────────┐       ║  │
│  ║   │  🚗          │  │  🏠          │  │  🏢          │       ║  │
│  ║   │  Vehículos   │  │  Casas       │  │  Apartamentos│       ║  │
│  ║   │  ──────────  │  │              │  │              │       ║  │
│  ║   │  (selected)  │  │              │  │              │       ║  │
│  ║   └──────────────┘  └──────────────┘  └──────────────┘       ║  │
│  ║                                                               ║  │
│  ║   ┌─────────────────────────────────────────────────────────┐ ║  │
│  ║   │  🔍  Buscar vehículos por marca, modelo...              │ ║  │
│  ║   │     ┌─────────┐ ┌─────────┐ ┌─────────┐ [Buscar]       │ ║  │
│  ║   │     │ Marca ▼ │ │ Precio ▼│ │ Año ▼   │                │ ║  │
│  ║   │     └─────────┘ └─────────┘ └─────────┘                │ ║  │
│  ║   └─────────────────────────────────────────────────────────┘ ║  │
│  ║                                                               ║  │
│  ╚═══════════════════════════════════════════════════════════════╝  │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

**Comportamiento:**
- Al hacer click en una categoría, la SearchBar cambia sus filtros
- Animación suave de transición entre categorías
- El botón seleccionado tiene borde inferior y fondo sutil

---

### 2. Category Selector Component

```tsx
// CategorySelector.tsx
interface Category {
  id: 'vehicles' | 'houses' | 'apartments';
  label: string;
  icon: React.ComponentType;
  color: string;
  placeholder: string;
}

const categories: Category[] = [
  { 
    id: 'vehicles', 
    label: 'Vehículos', 
    icon: CarIcon, 
    color: 'blue',
    placeholder: 'Buscar por marca, modelo, año...'
  },
  { 
    id: 'houses', 
    label: 'Casas', 
    icon: HomeIcon, 
    color: 'green',
    placeholder: 'Buscar casas por ubicación, precio...'
  },
  { 
    id: 'apartments', 
    label: 'Apartamentos', 
    icon: BuildingIcon, 
    color: 'green',
    placeholder: 'Buscar apartamentos por zona...'
  },
];
```

---

### 3. Featured Sections Layout

```
┌─────────────────────────────────────────────────────────────────────┐
│                                                                     │
│  🚗 Vehículos Destacados                            [Ver todos →]   │
│  ───────────────────────────────────────────────────────────────    │
│                                                                     │
│  ┌────────────┐  ┌────────────┐  ┌────────────┐  ┌────────────┐    │
│  │  [IMAGE]   │  │  [IMAGE]   │  │  [IMAGE]   │  │  [IMAGE]   │    │
│  │ ────────── │  │ ────────── │  │ ────────── │  │ ────────── │    │
│  │ BMW X5     │  │ Tesla M3   │  │ Audi Q7    │  │ Mercedes   │    │
│  │ $850,000   │  │ $720,000   │  │ $920,000   │  │ $680,000   │    │
│  │ 2023 • 15K │  │ 2024 • 5K  │  │ 2022 • 30K │  │ 2023 • 20K │    │
│  └────────────┘  └────────────┘  └────────────┘  └────────────┘    │
│                           ← ● ● ○ ○ →                               │
│                                                                     │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  🏠 Propiedades Destacadas                          [Ver todos →]   │
│  ───────────────────────────────────────────────────────────────    │
│                                                                     │
│  ┌────────────┐  ┌────────────┐  ┌────────────┐  ┌────────────┐    │
│  │            │  │            │  │            │  │            │    │
│  │  [IMAGE]   │  │  [IMAGE]   │  │  [IMAGE]   │  │  [IMAGE]   │    │
│  │  $2.5M     │  │  $1.8M     │  │  $3.2M     │  │  $950K     │    │
│  │ ────────── │  │ ────────── │  │ ────────── │  │ ────────── │    │
│  │ Casa       │  │ Depto      │  │ Casa       │  │ Depto      │    │
│  │ Polanco    │  │ Roma Norte │  │ Lomas      │  │ Condesa    │    │
│  │ 🛏3 🚿2 📐180m²│ │ 🛏2 🚿1 📐95m² │ │ 🛏4 🚿3 📐250m²│ │ 🛏1 🚿1 📐65m² │    │
│  └────────────┘  └────────────┘  └────────────┘  └────────────┘    │
│                           ← ● ● ○ ○ →                               │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

---

### 4. Property Card Design

```
┌──────────────────────────────────────┐
│                                      │
│         ┌────────────────────┐       │
│         │                    │       │
│         │   IMAGEN HERO      │       │
│         │                    │       │
│         │    ┌─────────┐     │       │
│         │    │ $2.5M   │     │  ❤    │
│         │    └─────────┘     │       │
│         │                    │       │
│         └────────────────────┘       │
│                                      │
│  Casa en Venta                       │
│  Polanco, Ciudad de México           │
│                                      │
│  ┌────┐  ┌────┐  ┌─────┐  ┌────┐    │
│  │🛏 3│  │🚿 2│  │📐180│  │🚗 2│    │
│  └────┘  └────┘  └─────┘  └────┘    │
│                                      │
│  Publicado hace 2 días               │
│                                      │
└──────────────────────────────────────┘
```

**Características:**
- Imagen ocupa 60% del card
- Precio superpuesto en esquina inferior izquierda
- Botón de favorito en esquina superior derecha
- Badges de specs en fila horizontal
- Sin bordes innecesarios, sombra sutil

---

### 5. Browse Page - Filters Sidebar

#### Para Vehículos:
```
┌──────────────────────┐
│ Filtros              │
│ ──────────────────── │
│                      │
│ Marca                │
│ [Todas las marcas ▼] │
│                      │
│ Modelo               │
│ [Todos los modelos▼] │
│                      │
│ Precio               │
│ $[____] - $[____]    │
│ ═══════●═══════════  │
│                      │
│ Año                  │
│ [2020] - [2024]      │
│                      │
│ Kilometraje          │
│ ○ < 30,000 km        │
│ ○ 30K - 60K km       │
│ ○ 60K - 100K km      │
│ ○ > 100,000 km       │
│                      │
│ Transmisión          │
│ ☑ Automática         │
│ ☑ Manual             │
│                      │
│ Combustible          │
│ ☑ Gasolina           │
│ ☑ Diesel             │
│ ☑ Híbrido            │
│ ☑ Eléctrico          │
│                      │
│ [Aplicar Filtros]    │
│ [Limpiar]            │
└──────────────────────┘
```

#### Para Propiedades:
```
┌──────────────────────┐
│ Filtros              │
│ ──────────────────── │
│                      │
│ Tipo de Operación    │
│ ● Comprar  ○ Rentar  │
│                      │
│ Tipo de Propiedad    │
│ ☑ Casa               │
│ ☑ Apartamento        │
│ ☐ Terreno            │
│ ☐ Local Comercial    │
│                      │
│ Precio               │
│ $[____] - $[____]    │
│ ═══════●═══════════  │
│                      │
│ Recámaras            │
│ [1] [2] [3] [4] [5+] │
│                      │
│ Baños                │
│ [1] [2] [3] [4+]     │
│                      │
│ Superficie (m²)      │
│ [50] - [500]         │
│                      │
│ Amenidades           │
│ ☐ Alberca            │
│ ☐ Gimnasio           │
│ ☐ Seguridad 24h      │
│ ☐ Estacionamiento    │
│ ☐ Roof Garden        │
│ ☐ Pet Friendly       │
│                      │
│ [Aplicar Filtros]    │
│ [Limpiar]            │
└──────────────────────┘
```

---

### 6. Property Detail Page

```
┌─────────────────────────────────────────────────────────────────────┐
│  ← Volver                                           ❤ Guardar  📤   │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │                                                             │   │
│  │                                                             │   │
│  │                    GALERÍA DE FOTOS                         │   │
│  │                    (Lightbox al click)                      │   │
│  │                                                             │   │
│  │  [1/15]                                       [Ver todas]   │   │
│  └─────────────────────────────────────────────────────────────┘   │
│  ┌─────┐ ┌─────┐ ┌─────┐ ┌─────┐ ┌─────┐                          │
│  │ [1] │ │ [2] │ │ [3] │ │ [4] │ │ [5] │  ...                     │
│  └─────┘ └─────┘ └─────┘ └─────┘ └─────┘                          │
│                                                                     │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  Casa en Venta                                                      │
│  ════════════════════════════════════════════                       │
│                                                                     │
│  $2,500,000 MXN                                                     │
│  📍 Polanco, Miguel Hidalgo, CDMX                                   │
│                                                                     │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌─────────────┐   │
│  │   🛏 3      │ │   🚿 2.5    │ │  📐 180m²   │ │   🚗 2      │   │
│  │ Recámaras  │ │   Baños     │ │ Superficie  │ │ Estac.      │   │
│  └─────────────┘ └─────────────┘ └─────────────┘ └─────────────┘   │
│                                                                     │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  Descripción                                                        │
│  ────────────                                                       │
│  Hermosa casa en una de las mejores zonas de la ciudad. Cuenta      │
│  con acabados de primera calidad, amplios espacios iluminados       │
│  naturalmente y un diseño arquitectónico moderno...                 │
│                                                       [Ver más ▼]   │
│                                                                     │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  Características                                                    │
│  ────────────────                                                   │
│                                                                     │
│  ┌─────────────────────┐  ┌─────────────────────┐                  │
│  │ ✓ Cocina integral   │  │ ✓ Cuarto de servicio│                  │
│  │ ✓ Closets vestidor  │  │ ✓ Área de lavado    │                  │
│  │ ✓ Pisos de madera   │  │ ✓ Jardín privado    │                  │
│  │ ✓ Doble altura      │  │ ✓ Terraza           │                  │
│  └─────────────────────┘  └─────────────────────┘                  │
│                                                                     │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  Amenidades del Desarrollo                                          │
│  ──────────────────────────                                         │
│                                                                     │
│  ┌──────┐ ┌──────┐ ┌──────┐ ┌──────┐ ┌──────┐ ┌──────┐            │
│  │  🏊  │ │  🏋  │ │  🛡  │ │  🌳  │ │  🐕  │ │  👶  │            │
│  │Alberca│ │ Gym  │ │Segur.│ │Jardín│ │ Pet  │ │Kids │            │
│  └──────┘ └──────┘ └──────┘ └──────┘ └──────┘ └──────┘            │
│                                                                     │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  Ubicación                                                          │
│  ─────────                                                          │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │                                                             │   │
│  │                     [MAPA INTERACTIVO]                      │   │
│  │                                                             │   │
│  │                         📍                                  │   │
│  │                                                             │   │
│  └─────────────────────────────────────────────────────────────┘   │
│                                                                     │
│  Cerca de: 🏫 Escuelas  🏥 Hospitales  🛒 Supermercados            │
│                                                                     │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  Propiedades Similares                                              │
│  ─────────────────────                                              │
│                                                                     │
│  ┌─────────┐ ┌─────────┐ ┌─────────┐ ┌─────────┐                   │
│  │ [Card]  │ │ [Card]  │ │ [Card]  │ │ [Card]  │                   │
│  └─────────┘ └─────────┘ └─────────┘ └─────────┘                   │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────┐
│  STICKY CONTACT BAR (Mobile)                                        │
│  ┌────────────────────────┐  ┌──────────────────────────────────┐  │
│  │ 📞 Llamar              │  │ 💬 Enviar Mensaje                │  │
│  └────────────────────────┘  └──────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────┘
```

---

### 7. Mega Menu Navigation

```
┌─────────────────────────────────────────────────────────────────────┐
│  [LOGO]    Vehículos ▼   Inmuebles ▼   🔍 Buscar    [Login]        │
└─────────────────────────────────────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────────────────────────────────┐
│                                                                     │
│   🚗 Vehículos                    │   Popular                       │
│   ─────────────                   │   ────────                      │
│                                   │                                 │
│   Por Tipo                        │   ⭐ SUVs 2024                  │
│   • Sedanes                       │   ⭐ Autos Eléctricos           │
│   • SUVs                          │   ⭐ Pickups                    │
│   • Pickups                       │                                 │
│   • Deportivos                    │   Ofertas                       │
│   • Eléctricos                    │   ────────                      │
│   • Híbridos                      │   🔥 Menos de $300K             │
│                                   │   🔥 Seminuevos 2023            │
│   Por Marca                       │                                 │
│   • Toyota                        │   ┌───────────────────────┐     │
│   • Honda                         │   │ [BANNER PROMOCIONAL]  │     │
│   • BMW                           │   │ Vende tu auto hoy     │     │
│   • Mercedes                      │   │ [Comenzar →]          │     │
│   • Ver todas →                   │   └───────────────────────┘     │
│                                   │                                 │
│   [Ver todos los vehículos →]     │                                 │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

---

### 8. Mobile Bottom Navigation

```
┌─────────────────────────────────────────────────────────────────────┐
│                                                                     │
│                        [CONTENIDO]                                  │
│                                                                     │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│   ┌─────────┐  ┌─────────┐  ┌─────────┐  ┌─────────┐  ┌─────────┐ │
│   │   🏠    │  │   🚗    │  │   🏠    │  │   ❤️    │  │   👤    │ │
│   │  Inicio │  │Vehículos│  │Inmuebles│  │Favoritos│  │  Perfil │ │
│   │  ─────  │  │         │  │         │  │         │  │         │ │
│   └─────────┘  └─────────┘  └─────────┘  └─────────┘  └─────────┘ │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 🗂️ Nueva Estructura de Archivos

```
frontend/web/src/
├── components/
│   ├── atoms/
│   │   ├── CategoryPill.tsx          # Pill de categoría
│   │   ├── PriceBadge.tsx            # Badge de precio overlay
│   │   └── FeatureIcon.tsx           # Icono + label (🛏 3)
│   ├── molecules/
│   │   ├── CategorySelector.tsx      # Selector de categoría hero
│   │   ├── SearchBarContextual.tsx   # SearchBar que cambia por categoría
│   │   └── QuickFilters.tsx          # Filtros rápidos inline
│   ├── organisms/
│   │   ├── VehicleCard.tsx           # (existente)
│   │   ├── PropertyCard.tsx          # Nuevo - Card de propiedad
│   │   ├── ListingCard.tsx           # Card genérico (factory pattern)
│   │   ├── AdvancedFilters.tsx       # (refactorizar para multi-vertical)
│   │   ├── MegaMenu.tsx              # Navegación expandida
│   │   ├── FeaturedCarousel.tsx      # Carrusel de destacados
│   │   ├── PropertyGallery.tsx       # Galería de fotos inmueble
│   │   └── AmenitiesGrid.tsx         # Grid de amenidades
│   └── templates/
│       ├── MarketplaceHomeTemplate.tsx
│       └── BrowseTemplate.tsx
├── pages/
│   ├── HomePage.tsx                   # Refactorizar a Marketplace
│   ├── vehicles/
│   │   ├── BrowseVehiclesPage.tsx
│   │   └── VehicleDetailPage.tsx
│   ├── properties/
│   │   ├── BrowsePropertiesPage.tsx
│   │   ├── PropertyDetailPage.tsx
│   │   ├── HousesPage.tsx
│   │   └── ApartmentsPage.tsx
│   └── dealer/
│       ├── ModulesMarketplacePage.tsx # Marketplace de módulos
│       └── SubscriptionPage.tsx       # Gestión de suscripción
├── types/
│   ├── index.ts
│   ├── vehicle.ts                     # Tipos de vehículo
│   ├── property.ts                    # Tipos de propiedad (NUEVO)
│   └── listing.ts                     # Tipo base Listing (NUEVO)
├── services/
│   ├── vehicleService.ts              # (existente)
│   ├── propertyService.ts             # (NUEVO)
│   └── listingService.ts              # Servicio unificado (NUEVO)
└── hooks/
    ├── useCategory.ts                 # Estado de categoría activa
    ├── useListing.ts                  # Hook genérico para listings
    └── useModuleAccess.ts             # Verificar acceso a módulos
```

---

## 🔧 Tipos TypeScript

### Listing Base (Compartido)

```typescript
// types/listing.ts
export type ListingCategory = 'vehicles' | 'properties';
export type ListingStatus = 'active' | 'pending' | 'sold' | 'inactive';

export interface ListingBase {
  id: string;
  category: ListingCategory;
  dealerId: string;
  title: string;
  description: string;
  price: number;
  currency: string;
  images: ListingImage[];
  primaryImage: string;
  location: Location;
  status: ListingStatus;
  isFeatured: boolean;
  isVerified: boolean;
  views: number;
  favorites: number;
  createdAt: string;
  updatedAt: string;
}

export interface ListingImage {
  id: string;
  url: string;
  thumbnailUrl: string;
  alt: string;
  order: number;
}

export interface Location {
  address: string;
  city: string;
  state: string;
  country: string;
  postalCode: string;
  coordinates?: {
    lat: number;
    lng: number;
  };
}
```

### Property Types (Nuevo)

```typescript
// types/property.ts
import { ListingBase } from './listing';

export type PropertyType = 'house' | 'apartment' | 'land' | 'commercial';
export type ListingType = 'sale' | 'rent';

export interface Property extends ListingBase {
  category: 'properties';
  propertyType: PropertyType;
  listingType: ListingType;
  
  // Dimensiones
  surfaceTotal: number;      // m² totales
  surfaceBuilt: number;      // m² construidos
  surfaceLand?: number;      // m² terreno (casas)
  
  // Espacios
  bedrooms: number;
  bathrooms: number;
  halfBaths?: number;
  parkingSpaces: number;
  floors?: number;
  floorNumber?: number;      // Para apartamentos
  
  // Características
  yearBuilt?: number;
  features: PropertyFeature[];
  amenities: Amenity[];
  
  // Precios adicionales
  pricePerSqm?: number;
  maintenanceFee?: number;   // Cuota de mantenimiento
  
  // Datos legales
  propertyTaxes?: number;
  registryNumber?: string;
}

export interface PropertyFeature {
  id: string;
  name: string;
  icon?: string;
}

export interface Amenity {
  id: string;
  name: string;
  icon: string;
  category: 'building' | 'outdoor' | 'services' | 'security';
}

// Amenidades predefinidas
export const AMENITIES: Amenity[] = [
  { id: 'pool', name: 'Alberca', icon: '🏊', category: 'outdoor' },
  { id: 'gym', name: 'Gimnasio', icon: '🏋️', category: 'building' },
  { id: 'security', name: 'Seguridad 24h', icon: '🛡️', category: 'security' },
  { id: 'garden', name: 'Jardín', icon: '🌳', category: 'outdoor' },
  { id: 'pet-friendly', name: 'Pet Friendly', icon: '🐕', category: 'services' },
  { id: 'kids-area', name: 'Área Infantil', icon: '👶', category: 'outdoor' },
  { id: 'rooftop', name: 'Roof Garden', icon: '🌿', category: 'outdoor' },
  { id: 'concierge', name: 'Concierge', icon: '🛎️', category: 'services' },
  { id: 'elevator', name: 'Elevador', icon: '🛗', category: 'building' },
  { id: 'storage', name: 'Bodega', icon: '📦', category: 'building' },
];
```

---

## 🚀 Plan de Implementación

### Semana 1: Fundamentos
1. Crear RealEstateService en backend
2. Definir tipos TypeScript
3. Crear componentes base (CategorySelector, PropertyCard)

### Semana 2: Homepage y Browse
4. Rediseñar HomePage con categorías
5. Crear BrowsePropertiesPage
6. Implementar filtros contextuales

### Semana 3: Detail Pages y Navegación
7. Crear PropertyDetailPage
8. Implementar MegaMenu
9. Mobile bottom navigation

### Semana 4: Admin y Polish
10. Módulos Marketplace para dealers
11. Admin CRUD de categorías
12. Testing y optimización

---

## 📱 Responsive Breakpoints

```scss
// Tailwind config
screens: {
  'sm': '640px',   // Mobile landscape
  'md': '768px',   // Tablet
  'lg': '1024px',  // Desktop
  'xl': '1280px',  // Large desktop
  '2xl': '1536px', // Extra large
}

// Grid columns por breakpoint
.listing-grid {
  @apply grid gap-6;
  @apply grid-cols-1;           // Mobile: 1 columna
  @apply sm:grid-cols-2;        // Tablet: 2 columnas
  @apply lg:grid-cols-3;        // Desktop: 3 columnas
  @apply xl:grid-cols-4;        // Large: 4 columnas
}
```

---

## ✅ Checklist de Lanzamiento

- [ ] RealEstateService desplegado y funcionando
- [ ] Todos los componentes con tests unitarios
- [ ] E2E tests para flujos principales
- [ ] Lighthouse score > 90
- [ ] Accesibilidad WCAG 2.1 AA
- [ ] SEO meta tags dinámicos
- [ ] Analytics eventos configurados
- [ ] Error tracking (Sentry) configurado
- [ ] Feature flags para rollout gradual
