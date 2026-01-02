# 🚀 Plan de Sprints - Frontend CarDealer

> **Stack**: React + Vite + TypeScript  
> **Duración Total**: ~10-12 semanas (10 sprints de 1 semana)  
> **Team Size**: 2-3 desarrolladores frontend

---

## 📊 RESUMEN EJECUTIVO

| Sprint | Duración | Enfoque | Componentes |
|--------|----------|---------|-------------|
| **Sprint 0** | 1 semana | Setup & Arquitectura | Proyecto base, design system |
| **Sprint 1** | 1 semana | Autenticación | Login, Register, Profile |
| **Sprint 2** | 1 semana | Home & Navigation | Landing, Navbar, Footer |
| **Sprint 3** | 1.5 semanas | Vehicle Catalog | Search, Filters, Cards |
| **Sprint 4** | 1.5 semanas | Vehicle Details | Gallery, Specs, Contact |
| **Sprint 5** | 1.5 semanas | Sell Vehicle | Multi-step form, Upload |
| **Sprint 6** | 1 semana | User Dashboard | My Listings, Favorites |
| **Sprint 7** | 1 semana | Messages & Contact | Chat, Inquiries |
| **Sprint 8** | 1 semana | Admin Panel | Approval, Moderation |
| **Sprint 9** | 1 semana | Polish & Testing | UX refinement, tests |
| **Sprint 10** | 1 semana | Production & Deploy | Build, Docker, CI/CD |

**Total**: 11-13 semanas

---

## 🔧 SPRINT 0: Setup, Arquitectura & CI/CD
**Duración**: 1 semana (5 días)  
**Objetivo**: Proyecto configurado, CI/CD pipeline, design system básico, Docker setup

### 📋 Tareas

#### Día 1: Inicialización del Proyecto
```bash
# Crear proyecto
npm create vite@latest cardealer-frontend -- --template react-ts
cd cardealer-frontend

# Instalar dependencias core
npm install react-router-dom zustand @tanstack/react-query axios
npm install -D tailwindcss postcss autoprefixer
npm install -D @types/node

# Configurar Tailwind
npx tailwindcss init -p
```

**Checklist**:
- [ ] Proyecto Vite + React + TypeScript creado
- [ ] Git repository inicializado
- [ ] `.gitignore` configurado (node_modules, dist, .env)
- [ ] ESLint + Prettier configurados
- [ ] Tailwind CSS configurado
- [ ] Path aliases configurados (`@/components`, `@/utils`)
- [ ] Husky + lint-staged (pre-commit hooks)
- [ ] Conventional commits setup

**Entregable**: Proyecto ejecutándose en `localhost:5173`

---

#### Día 2: Estructura de Carpetas & Arquitectura

```
cardealer-frontend/
├── public/
│   ├── favicon.ico
│   └── images/
├── src/
│   ├── assets/          # Imágenes, iconos estáticos
│   ├── components/      # Componentes reutilizables
│   │   ├── atoms/       # Button, Input, Badge, etc.
│   │   ├── molecules/   # FormField, SearchBar, etc.
│   │   ├── organisms/   # Navbar, VehicleCard, etc.
│   │   └── templates/   # Layout wrappers
│   ├── features/        # Feature-based modules
│   │   ├── auth/
│   │   │   ├── components/
│   │   │   ├── hooks/
│   │   │   ├── services/
│   │   │   └── types/
│   │   ├── vehicles/
│   │   ├── user/
│   │   └── admin/
│   ├── hooks/           # Custom hooks globales
│   ├── layouts/         # App layouts (MainLayout, AuthLayout)
│   ├── pages/           # Page components
│   ├── services/        # API clients
│   │   ├── api.ts       # Axios instance
│   │   └── endpoints/
│   ├── store/           # Zustand stores
│   ├── styles/          # Global CSS, Tailwind config
│   ├── types/           # TypeScript types/interfaces
│   ├── utils/           # Helper functions
│   ├── App.tsx
│   ├── main.tsx
│   └── router.tsx
├── .env.example
├── .env.development
├── tailwind.config.js
├── tsconfig.json
├── vite.config.ts
└── package.json
```

**Checklist**:
- [ ] Estructura de carpetas creada
- [ ] Archivo `.env.example` con variables
- [ ] `router.tsx` con React Router v6
- [ ] `api.ts` con Axios configurado
- [ ] Interceptores de autenticación
- [ ] Error handling global

**Entregable**: Arquitectura documentada en `README.md`

---

#### Día 3: Design System - Atoms

**Componentes a crear**:

1. **Button.tsx**
```tsx
// src/components/atoms/Button.tsx
type ButtonVariant = 'primary' | 'secondary' | 'outline' | 'ghost';
type ButtonSize = 'sm' | 'md' | 'lg';

interface ButtonProps {
  variant?: ButtonVariant;
  size?: ButtonSize;
  loading?: boolean;
  disabled?: boolean;
  children: React.ReactNode;
  onClick?: () => void;
}
```

2. **Input.tsx**
3. **Badge.tsx**
4. **Icon.tsx** (wrapper para react-icons)
5. **Spinner.tsx**
6. **Avatar.tsx**

**Checklist**:
- [ ] Button con todas las variantes
- [ ] Input con estados (error, disabled, loading)
- [ ] Badge con colores
- [ ] Spinner animado
- [ ] Avatar con fallback
- [ ] Storybook setup (opcional)

**Entregable**: 6 componentes atoms documentados

---

#### Día 4: Design System - Molecules

**Componentes a crear**:

1. **FormField.tsx**
```tsx
interface FormFieldProps {
  label: string;
  error?: string;
  required?: boolean;
  children: React.ReactNode;
}
```

2. **SearchBar.tsx**
3. **PriceDisplay.tsx**
4. **Rating.tsx** (stars component)
5. **VehicleSpecs.tsx** (icon + label + value)
6. **ImageUploader.tsx**

**Checklist**:
- [ ] FormField con label + error
- [ ] SearchBar con sugerencias
- [ ] PriceDisplay formateado
- [ ] Rating interactivo
- [ ] VehicleSpecs con iconos
- [ ] ImageUploader drag & drop

**Entregable**: 6 componentes molecules funcionales

---

#### Día 5: Layouts & Theme

**Componentes a crear**:

1. **MainLayout.tsx**
```tsx
// Navbar + Content + Footer
<MainLayout>
  <Navbar />
  <main className="min-h-screen">
    {children}
  </main>
  <Footer />
</MainLayout>
```

2. **AuthLayout.tsx** (centrado, sin navbar)
3. **DashboardLayout.tsx** (con sidebar)

**Theme Configuration**:
```ts
// src/styles/theme.ts
export const theme = {
  colors: {
    primary: '#00539F',
    secondary: '#0089FF',
    accent: '#FF6B35',
    // ...
  },
  spacing: {
    // 8px grid system
  },
  typography: {
    // Font sizes, weights
  }
};
```

**Checklist**:
- [ ] MainLayout con Navbar placeholder
- [ ] AuthLayout minimalista
- [ ] DashboardLayout con sidebar
- [ ] Theme configurado en Tailwind
- [ ] Dark mode toggle (opcional)
- [ ] Responsive breakpoints validados

**Entregable**: 3 layouts + sistema de theming

---

### 📦 Librerías a Instalar (Sprint 0)

```json
{
  "dependencies": {
    "react": "^18.3.1",
    "react-dom": "^18.3.1",
    "react-router-dom": "^6.22.0",
    "zustand": "^4.5.0",
    "@tanstack/react-query": "^5.20.0",
    "axios": "^1.6.7",
    "clsx": "^2.1.0",
    "date-fns": "^3.3.1",
    "react-icons": "^5.0.1"
  },
  "devDependencies": {
    "@types/react": "^18.2.55",
    "@types/react-dom": "^18.2.19",
    "@typescript-eslint/eslint-plugin": "^6.21.0",
    "@typescript-eslint/parser": "^6.21.0",
    "@vitejs/plugin-react": "^4.2.1",
    "autoprefixer": "^10.4.17",
    "eslint": "^8.56.0",
    "postcss": "^8.4.35",
    "prettier": "^3.2.5",
    "tailwindcss": "^3.4.1",
    "typescript": "^5.3.3",
    "vite": "^5.1.0"
  }
}
```

---

## 🔐 SPRINT 1: Autenticación
**Duración**: 1 semana (5 días)  
**Objetivo**: Sistema de login/registro completo con JWT

### 📋 Tareas

#### Día 1-2: Login & Register UI

**Páginas a crear**:

1. **LoginPage.tsx** (`/login`)
```tsx
// Features:
- Email/password form
- "Remember me" checkbox
- "Forgot password?" link
- Social login buttons (Google, Facebook)
- Link to register
- Form validation (react-hook-form + zod)
```

2. **RegisterPage.tsx** (`/register`)
```tsx
// Features:
- Username, email, password fields
- Password strength indicator
- Terms & conditions checkbox
- Email verification notice
- Link to login
```

**Checklist**:
- [ ] LoginPage UI completa
- [ ] RegisterPage UI completa
- [ ] Form validation con Zod
- [ ] Error messages user-friendly
- [ ] Loading states
- [ ] Responsive design

---

#### Día 3: Auth Service & Store

**Service**:
```ts
// src/features/auth/services/authService.ts
export const authService = {
  login: (email: string, password: string) => 
    api.post('/auth/login', { email, password }),
  
  register: (data: RegisterData) => 
    api.post('/auth/register', data),
  
  logout: (refreshToken: string) => 
    api.post('/auth/logout', { refreshToken }),
  
  refreshToken: (token: string) => 
    api.post('/auth/refresh-token', { refreshToken: token }),
  
  forgotPassword: (email: string) => 
    api.post('/auth/forgot-password', { email }),
  
  resetPassword: (token: string, password: string) => 
    api.post('/auth/reset-password', { token, newPassword: password })
};
```

**Store**:
```ts
// src/store/authStore.ts
interface AuthState {
  user: User | null;
  accessToken: string | null;
  refreshToken: string | null;
  isAuthenticated: boolean;
  login: (email: string, password: string) => Promise<void>;
  logout: () => void;
  register: (data: RegisterData) => Promise<void>;
}
```

**Checklist**:
- [ ] authService completo
- [ ] authStore con Zustand
- [ ] localStorage persistence
- [ ] Token refresh logic
- [ ] Axios interceptors

---

#### Día 4: Protected Routes & Profile

**Router Protection**:
```tsx
// src/components/ProtectedRoute.tsx
const ProtectedRoute = ({ children }: { children: React.ReactNode }) => {
  const { isAuthenticated } = useAuthStore();
  return isAuthenticated ? children : <Navigate to="/login" />;
};
```

**ProfilePage.tsx** (`/profile`)
```tsx
// Features:
- View user info
- Edit profile form
- Change password
- Upload avatar
- Delete account
```

**Checklist**:
- [ ] ProtectedRoute component
- [ ] Redirect logic
- [ ] ProfilePage UI
- [ ] Edit profile functionality
- [ ] Avatar upload
- [ ] Change password flow

---

#### Día 5: Password Reset & Email Verification

**ForgotPasswordPage.tsx** (`/forgot-password`)
**ResetPasswordPage.tsx** (`/reset-password/:token`)
**VerifyEmailPage.tsx** (`/verify-email/:token`)

**Checklist**:
- [ ] Forgot password flow
- [ ] Reset password page
- [ ] Email verification page
- [ ] Success/error messages
- [ ] Auto-redirect after verification

---

### ✅ Sprint 1 Entregables

- [ ] Login page funcional
- [ ] Register page funcional
- [ ] JWT authentication working
- [ ] Protected routes
- [ ] Profile management
- [ ] Password reset flow
- [ ] Email verification

**Tests**: 15+ unit tests (forms, store, services)

---

## 🏠 SPRINT 2: Home & Navigation
**Duración**: 1 semana (5 días)  
**Objetivo**: Landing page, navbar, footer, navegación completa

### 📋 Tareas

#### Día 1-2: Navbar Component

**Navbar.tsx**
```tsx
// Desktop:
┌──────────────────────────────────────────────────────┐
│ [Logo] [Buscar] [Comprar] [Vender] [Login/Avatar ▼]│
└──────────────────────────────────────────────────────┘

// Mobile:
┌──────────────────────────────────────────────────────┐
│ [☰] [Logo]                               [🔍] [👤] │
└──────────────────────────────────────────────────────┘

// Features:
- Sticky on scroll
- Transparent on hero, solid on scroll
- Search bar (expandable on mobile)
- User dropdown menu
- Mobile hamburger menu
- Notifications badge (opcional)
```

**Checklist**:
- [ ] Navbar desktop responsive
- [ ] Navbar mobile con drawer
- [ ] Search bar funcional
- [ ] User dropdown
- [ ] Scroll behavior
- [ ] Active link highlighting

---

#### Día 3: Homepage / Landing

**HomePage.tsx** (`/`)

**Secciones**:
```tsx
1. Hero Section
   - Headline: "Encuentra el auto de tus sueños"
   - Smart Search Bar
   - Background image (car)
   - CTA buttons

2. Featured Vehicles (Grid)
   - 8-12 vehicle cards
   - "Ver todos" button

3. Browse by Category
   - [SUV] [Sedan] [Coupe] [Van] [Truck] [Electric]
   - Icon + count per category

4. How It Works (3 steps)
   - 1️⃣ Search
   - 2️⃣ Compare
   - 3️⃣ Buy

5. Trust Badges
   - [✓ Verified] [🛡️ Safe] [💰 Best Price]

6. Testimonials
   - Carousel with user reviews

7. Stats Counter
   - 50,000+ Vehicles
   - 10,000+ Sellers
   - 4.8★ Rating
```

**Checklist**:
- [ ] Hero section responsive
- [ ] Search bar integration
- [ ] Featured vehicles from API
- [ ] Category navigation
- [ ] How it works section
- [ ] Testimonials carousel
- [ ] CTA buttons functional

---

#### Día 4: Footer Component

**Footer.tsx**
```tsx
┌─────────────────────────────────────────────────────┐
│  [Logo]                                             │
│                                                     │
│  Company        Support        Legal                │
│  ├─ About      ├─ Help Center  ├─ Terms            │
│  ├─ Blog       ├─ Contact      ├─ Privacy          │
│  ├─ Careers    └─ FAQ          └─ Cookies          │
│  └─ Press                                           │
│                                                     │
│  Follow Us: [Facebook] [Twitter] [Instagram]       │
│                                                     │
│  © 2025 CarDealer. All rights reserved.            │
└─────────────────────────────────────────────────────┘
```

**Checklist**:
- [ ] Footer links organized
- [ ] Social media icons
- [ ] Newsletter signup (opcional)
- [ ] Responsive columns
- [ ] Accessibility

---

#### Día 5: Search Bar Global

**GlobalSearchBar.tsx**
```tsx
// Features:
- Autocomplete suggestions
- Recent searches
- Quick filters (marca, modelo)
- Search on Enter
- Debounced API calls
- Loading state
```

**Checklist**:
- [ ] Autocomplete working
- [ ] Debounce implemented (300ms)
- [ ] Recent searches stored
- [ ] Keyboard navigation (↑↓ Enter)
- [ ] Mobile optimized
- [ ] Search results preview

---

### ✅ Sprint 2 Entregables

- [ ] Navbar responsive funcional
- [ ] Homepage/Landing completa
- [ ] Footer con links
- [ ] Global search bar
- [ ] Mobile navigation drawer
- [ ] SEO meta tags

**Tests**: 10+ component tests

---

## 🚗 SPRINT 3: Vehicle Catalog & Search
**Duración**: 1.5 semanas (7-8 días)  
**Objetivo**: Búsqueda de vehículos, filtros, paginación

### 📋 Tareas

#### Día 1-2: Vehicle Search Page Layout

**VehiclesPage.tsx** (`/vehicles`)

```tsx
// Desktop Layout:
┌─────────────────────────────────────────────────────┐
│  🔍 "Toyota Camry 2023" [X]  [💾 Guardar búsqueda] │
├──────────┬──────────────────────────────────────────┤
│ FILTERS  │  RESULTS GRID (234 vehicles)            │
│ (25%)    │  [Sort: Featured ▼] [View: Grid/List]   │
│          │                                          │
│ Precio   │  ┌────┐ ┌────┐ ┌────┐ ┌────┐          │
│ Marca    │  │Card│ │Card│ │Card│ │Card│          │
│ Año      │  └────┘ └────┘ └────┘ └────┘          │
│ Tipo     │  ┌────┐ ┌────┐ ┌────┐ ┌────┐          │
│ Combustible│ │Card│ │Card│ │Card│ │Card│          │
│          │  └────┘ └────┘ └────┘ └────┘          │
│ [Reset]  │                                          │
│          │  [← Prev] [1][2][3]...[10] [Next →]   │
└──────────┴──────────────────────────────────────────┘
```

**Checklist**:
- [ ] Layout responsivo (sidebar collapse en mobile)
- [ ] Grid/List view toggle
- [ ] Sort dropdown (Featured, Price↑, Price↓, Date)
- [ ] Results count display
- [ ] Empty state
- [ ] Loading skeleton

---

#### Día 3-4: Filter Sidebar Component

**FilterSidebar.tsx**

**Filtros a implementar**:
```tsx
1. Price Range (Slider)
   [$0 ────────●─────● $100k+]

2. Brand (Checkbox list)
   ☑ Toyota (145)
   ☐ Honda (98)
   ☐ Ford (76)
   ... [Show more]

3. Year Range (Dual slider)
   [2015 ──●────────●── 2024]

4. Mileage
   ○ Under 10k miles
   ○ 10k - 50k
   ○ 50k - 100k
   ○ 100k+

5. Fuel Type
   ☐ Gasoline
   ☐ Diesel
   ☐ Hybrid
   ☐ Electric

6. Transmission
   ○ Automatic
   ○ Manual

7. Body Type
   ☐ Sedan
   ☐ SUV
   ☐ Truck
   ☐ Coupe

8. Features (expandable)
   ☐ Leather Seats
   ☐ Sunroof
   ☐ Navigation
   ... [+12 more]
```

**Checklist**:
- [ ] Price range slider (rc-slider o react-range)
- [ ] Brand checkbox list (scrollable)
- [ ] Year range dual slider
- [ ] Mileage radio buttons
- [ ] Fuel type checkboxes
- [ ] Transmission radio
- [ ] Body type checkboxes
- [ ] Clear all filters button
- [ ] Active filters count badge
- [ ] Mobile: Bottom sheet drawer

---

#### Día 5-6: Vehicle Card Component

**VehicleCard.tsx** (⭐ COMPONENTE CRÍTICO)

```tsx
interface VehicleCardProps {
  vehicle: Vehicle;
  viewMode: 'grid' | 'list';
  onFavorite?: (id: string) => void;
  onCompare?: (id: string) => void;
}

// Grid View:
┌─────────────────────────┐
│  [Image Carousel]       │
│  [❤️] [🔄]  [NUEVO]    │
├─────────────────────────┤
│  Toyota Camry 2023      │
│  XLE Premium            │
│  ⭐⭐⭐⭐⭐ (45)         │
│                         │
│  $35,000                │
│  15,000 mi • Los Angeles│
│                         │
│  🚗 Auto • ⛽ Hybrid   │
│                         │
│  [View Details →]       │
└─────────────────────────┘

// List View: Horizontal layout
```

**Features**:
- Image carousel (swiper)
- Favorite toggle
- Compare checkbox
- Badge (NEW, CERTIFIED, FEATURED)
- Price formatting
- Specs icons
- Hover effects
- Lazy loading images

**Checklist**:
- [ ] Grid view layout
- [ ] List view layout
- [ ] Image carousel functional
- [ ] Favorite button (optimistic update)
- [ ] Compare toggle
- [ ] Badge system
- [ ] Responsive (mobile adapta)
- [ ] Performance optimized (memo)

---

#### Día 7-8: Pagination & Infinite Scroll

**Pagination Component**:
```tsx
// Opciones:
1. Traditional: [← Prev] [1][2][3]...[10] [Next →]
2. Infinite scroll (react-infinite-scroll-component)
3. Load more button
```

**API Integration**:
```ts
// src/features/vehicles/hooks/useVehicles.ts
const useVehicles = (filters: VehicleFilters, page: number) => {
  return useQuery({
    queryKey: ['vehicles', filters, page],
    queryFn: () => vehicleService.search(filters, page),
    keepPreviousData: true,
  });
};
```

**Checklist**:
- [ ] Pagination component
- [ ] Page state management
- [ ] URL query params sync
- [ ] Scroll to top on page change
- [ ] Loading states
- [ ] Error handling
- [ ] Cache strategy (React Query)

---

### 🔌 API Integration (Sprint 3)

**Endpoints a consumir**:
```ts
GET /api/vehicles?page=1&pageSize=20
GET /api/vehicles/search?q=toyota&minPrice=20000&maxPrice=50000
GET /api/vehicles/filters/brands
GET /api/vehicles/filters/models?brand=toyota
POST /api/vehicles/{id}/favorite
GET /api/vehicles/user/{userId}/favorites
```

**Types**:
```ts
interface Vehicle {
  id: string;
  brand: string;
  model: string;
  year: number;
  price: number;
  mileage: number;
  transmission: 'automatic' | 'manual';
  fuelType: 'gasoline' | 'diesel' | 'hybrid' | 'electric';
  bodyType: string;
  color: string;
  images: string[];
  location: string;
  sellerId: string;
  status: 'active' | 'sold' | 'pending';
  features: string[];
  createdAt: string;
}

interface VehicleFilters {
  query?: string;
  brands?: string[];
  minPrice?: number;
  maxPrice?: number;
  minYear?: number;
  maxYear?: number;
  fuelTypes?: string[];
  transmission?: string;
  bodyTypes?: string[];
  minMileage?: number;
  maxMileage?: number;
  features?: string[];
  location?: string;
}
```

---

### ✅ Sprint 3 Entregables

- [ ] Vehicle search page funcional
- [ ] Filter sidebar completo
- [ ] Vehicle card en grid/list
- [ ] Pagination/infinite scroll
- [ ] URL query params
- [ ] Favorite functionality
- [ ] Compare functionality
- [ ] Responsive mobile

**Tests**: 20+ tests (componentes, hooks, filtros)

---

## 📄 SPRINT 4: Vehicle Detail Page
**Duración**: 1.5 semanas (7-8 días)  
**Objetivo**: Página de detalle completa con galería, specs, contacto

### 📋 Tareas

#### Día 1-3: Image Gallery Component

**VehicleGallery.tsx** (⭐ COMPONENTE CRUCIAL)

```tsx
// Features requeridos:
- Main image large display
- Thumbnail strip (scrollable)
- Lightbox/fullscreen mode
- Zoom on hover
- 360° viewer (si disponible)
- Lazy loading
- Keyboard navigation (← →)
- Touch gestures (swipe)
```

**Librerías**:
```json
{
  "swiper": "^11.0.6",
  "yet-another-react-lightbox": "^3.15.0",
  "react-medium-image-zoom": "^5.1.8"
}
```

**Checklist**:
- [ ] Swiper carousel configurado
- [ ] Thumbnail navigation
- [ ] Lightbox modal
- [ ] Zoom on hover/click
- [ ] Touch swipe gestures
- [ ] Keyboard arrows
- [ ] Loading placeholders
- [ ] Error fallback image

---

#### Día 4-5: Vehicle Detail Page Layout

**VehicleDetailPage.tsx** (`/vehicles/:id`)

```tsx
// Secciones:
1. Gallery (above fold)
2. Overview Card (sticky sidebar)
3. Description
4. Key Features
5. Specifications (accordion)
6. Vehicle History (Carfax placeholder)
7. Seller Info Card
8. Reviews & Ratings
9. Similar Vehicles
10. Contact Form
```

**Layout**:
```tsx
<div className="container mx-auto">
  {/* Gallery */}
  <VehicleGallery images={vehicle.images} />
  
  <div className="grid lg:grid-cols-3 gap-8">
    {/* Left: Main content (2 cols) */}
    <div className="lg:col-span-2">
      <VehicleOverview />
      <VehicleDescription />
      <VehicleFeatures />
      <VehicleSpecs />
      <VehicleHistory />
      <ReviewsSection />
    </div>
    
    {/* Right: Sidebar (1 col, sticky) */}
    <div className="lg:col-span-1">
      <PriceCard /> {/* Sticky */}
      <SellerCard />
      <TrustBadges />
    </div>
  </div>
  
  <SimilarVehicles />
</div>
```

**Checklist**:
- [ ] Responsive layout
- [ ] Sticky sidebar
- [ ] Breadcrumbs navigation
- [ ] Share buttons
- [ ] Print view
- [ ] SEO meta tags (dynamic)

---

#### Día 6: Price Card & Contact

**PriceCard.tsx** (Sticky sidebar)
```tsx
┌─────────────────────────┐
│  $35,000               │
│  💰 Est: $650/mo       │
│                        │
│  [💬 Contact Seller]   │
│  [📞 (555) 123-4567]   │
│  [💰 Get Financing]    │
│                        │
│  ❤️ Save to Favorites  │
│  🔄 Add to Compare     │
│                        │
│  📅 Schedule Test Drive│
│  📊 Check Availability │
└─────────────────────────┘
```

**ContactModal.tsx**
```tsx
// Modal form:
- Name
- Email
- Phone
- Message (pre-filled: "I'm interested in...")
- [Send Message]
```

**Checklist**:
- [ ] Price display con formateo
- [ ] Monthly payment calculator
- [ ] Contact seller button → modal
- [ ] Call button (tel: link)
- [ ] Financing link (placeholder)
- [ ] Favorite toggle
- [ ] Compare toggle
- [ ] Schedule test drive (modal)

---

#### Día 7-8: Specifications & Reviews

**SpecificationsAccordion.tsx**
```tsx
// Expandable sections:
1. Engine & Performance
   - Engine: 2.5L 4-Cylinder
   - Horsepower: 203 hp
   - Transmission: 8-Speed Automatic
   
2. Exterior
   - Color: Silver
   - Body Type: Sedan
   - Doors: 4
   
3. Interior
   - Seats: 5
   - Upholstery: Leather
   - Interior Color: Black
   
4. Features & Options
   - Sunroof
   - Navigation System
   - Backup Camera
   - ...
   
5. Safety
   - Airbags: 8
   - ABS Brakes
   - Stability Control
   - ...
```

**ReviewsSection.tsx**
```tsx
// Features:
- Overall rating ⭐⭐⭐⭐⭐ 4.8/5 (45 reviews)
- Rating breakdown (5★: 80%, 4★: 15%, ...)
- Sort by: Most helpful, Recent, Rating
- Individual reviews with:
  - User avatar
  - Rating
  - Date
  - Verified badge
  - Helpful count [👍 12]
  - Review text
  
// For logged-in users:
- [Write a Review] button
```

**Checklist**:
- [ ] Accordion component reutilizable
- [ ] All specs displayed
- [ ] Reviews listing
- [ ] Rating breakdown chart
- [ ] Sort/filter reviews
- [ ] Write review modal
- [ ] Helpful vote toggle

---

### 🔌 API Integration (Sprint 4)

**Endpoints**:
```ts
GET /api/vehicles/:id
GET /api/vehicles/:id/similar
GET /api/vehicles/:id/reviews
POST /api/vehicles/:id/reviews
POST /api/vehicles/:id/contact
GET /api/media/:vehicleId/images
```

---

### ✅ Sprint 4 Entregables

- [ ] Vehicle detail page completa
- [ ] Image gallery profesional
- [ ] Price card sticky funcional
- [ ] Contact seller modal
- [ ] Specifications accordion
- [ ] Reviews section
- [ ] Similar vehicles carousel
- [ ] SEO optimizado
- [ ] Share functionality

**Tests**: 15+ tests

---

## 📝 SPRINT 5: Sell Vehicle (Upload Flow)
**Duración**: 1.5 semanas (7-8 días)  
**Objetivo**: Multi-step form para publicar vehículos

### 📋 Tareas

#### Día 1-2: Multi-Step Form Structure

**SellVehiclePage.tsx** (`/sell`)

```tsx
// Progress Stepper:
[●━━●━━●━━○━━○━━○] Step 3 of 6

// Steps:
1. Basic Information
2. Vehicle Details
3. Photos Upload ⭐
4. Features & Options
5. Pricing
6. Review & Publish
```

**FormWizard.tsx** (reusable)
```tsx
interface Step {
  id: number;
  title: string;
  component: React.ComponentType;
  validation: ZodSchema;
}

const SellVehicleWizard = () => {
  const [currentStep, setCurrentStep] = useState(1);
  const [formData, setFormData] = useState({});
  
  const steps: Step[] = [
    { id: 1, title: 'Basic Info', component: BasicInfoStep, ... },
    { id: 2, title: 'Details', component: DetailsStep, ... },
    // ...
  ];
  
  return (
    <div>
      <ProgressBar current={currentStep} total={steps.length} />
      <StepComponent {...steps[currentStep-1]} />
      <Navigation onPrev={prev} onNext={next} />
    </div>
  );
};
```

**Checklist**:
- [ ] Progress stepper component
- [ ] Form wizard navigation
- [ ] State management (Zustand o Context)
- [ ] LocalStorage persistence
- [ ] Validation per step
- [ ] Back/Next logic

---

#### Día 3: Step 1 & 2 - Basic Info & Details

**Step 1: Basic Information**
```tsx
- VIN Decoder (API call to decode VIN)
- Brand [Select con búsqueda]
- Model [Dependiente de Brand]
- Year [Select 2000-2025]
- Trim [Select opcional]

[Autocomplete de datos si VIN es válido]
```

**Step 2: Vehicle Details**
```tsx
- Mileage [Number input con validación]
- Transmission [Radio: Automatic / Manual]
- Fuel Type [Radio: Gas / Diesel / Hybrid / Electric]
- Exterior Color [Color picker o select]
- Interior Color [Color picker o select]
- Doors [Select: 2 / 4 / 5]
- Seats [Select: 2-7]
- Body Type [Select: Sedan / SUV / ...]
```

**Checklist**:
- [ ] VIN decoder integration
- [ ] Brand/Model cascade selects
- [ ] Year validation (not future)
- [ ] Mileage formatting (commas)
- [ ] Color pickers functional
- [ ] Form validation (Zod)

---

#### Día 4-5: Step 3 - Photo Upload (⭐ CRÍTICO)

**PhotoUploadStep.tsx**

```tsx
// Upload Zone:
┌─────────────────────────────────────┐
│                                     │
│    📸 Drag & Drop Photos Here      │
│    or Click to Browse               │
│                                     │
│    • Upload up to 20 photos         │
│    • Max 10MB per photo             │
│    • JPG, PNG, WebP                 │
│                                     │
└─────────────────────────────────────┘

// Tips Sidebar:
📷 Photo Tips:
✓ Take photos in good lighting
✓ Clean your car first
✓ Show all angles (360°)
✓ Include:
  • Front, rear, both sides
  • Interior dashboard
  • Seats (front & back)
  • Trunk/cargo area
  • Engine bay
  • Any damage or wear

// Uploaded Photos Grid:
┌────┐ ┌────┐ ┌────┐ ┌────┐
│⭐  │ │ 2  │ │ 3  │ │ 4  │
│[x] │ │[x] │ │[x] │ │[x] │
└────┘ └────┘ └────┘ └────┘
Main    Edit    Edit    Edit

[Drag to reorder]
```

**Features**:
- Drag & drop (react-dropzone)
- Multiple upload
- Image compression (browser-image-compression)
- Preview thumbnails
- Set primary image (star icon)
- Reorder (drag to reorder)
- Crop/rotate editor (react-easy-crop)
- Progress bars
- Error handling (size, format)
- Delete confirmation

**Checklist**:
- [ ] Dropzone configurado
- [ ] Image compression working
- [ ] Preview thumbnails grid
- [ ] Set primary image
- [ ] Drag to reorder (dnd-kit)
- [ ] Image editor modal (crop/rotate)
- [ ] Upload progress per image
- [ ] Validation (max size, format)
- [ ] Delete with confirmation
- [ ] Mobile camera capture

---

#### Día 6: Step 4 & 5 - Features & Pricing

**Step 4: Features & Options**
```tsx
// Checkboxes organized by category:

🪑 Comfort
☑ Leather Seats
☑ Heated Seats
☐ Ventilated Seats
☐ Power Seats

🎵 Entertainment
☑ Premium Sound System
☑ Navigation
☐ DVD Player
☐ WiFi Hotspot

🛡️ Safety
☑ Backup Camera
☑ Blind Spot Monitor
☐ Lane Departure Warning
☐ Adaptive Cruise Control

🔧 Convenience
☑ Keyless Entry
☑ Remote Start
☐ Power Liftgate
☐ Sunroof

[+ Add Custom Feature]
```

**Step 5: Pricing**
```tsx
┌─────────────────────────────────┐
│  Your Asking Price              │
│  $__________                    │
│                                 │
│  💡 Market Value Analysis       │
│  Based on similar vehicles:     │
│  Low: $32,000                   │
│  Avg: $35,000  ← Suggested     │
│  High: $38,000                  │
│                                 │
│  [Use Suggested Price]          │
│                                 │
│  ☐ Negotiable                   │
│  ☐ Open to trade-ins            │
└─────────────────────────────────┘
```

**Checklist**:
- [ ] Features checklist organized
- [ ] Custom feature input
- [ ] Price validation
- [ ] Market value estimation (mock)
- [ ] Negotiable toggle
- [ ] Trade-in option

---

#### Día 7-8: Step 6 - Review & Publish

**ReviewStep.tsx**
```tsx
// Preview Card (como se verá publicado)
┌─────────────────────────────────┐
│  [Image Carousel Preview]       │
│                                 │
│  Toyota Camry 2023 XLE          │
│  $35,000 • 15,000 mi            │
│                                 │
│  📍 Los Angeles, CA             │
│  🚗 Automatic • ⛽ Hybrid      │
│                                 │
│  ✨ Features:                   │
│  • Leather Seats                │
│  • Sunroof                      │
│  • Navigation                   │
│  ...                            │
│                                 │
│  📝 Description                 │
│  [Your description here]        │
└─────────────────────────────────┘

[Edit] [Save as Draft] [Publish Listing →]

⚠️ Important:
• Listing will be reviewed by our team
• Approval typically takes 24-48 hours
• You'll receive an email when approved
```

**Checklist**:
- [ ] Preview card exacto al VehicleCard
- [ ] Edit step navigation
- [ ] Save as draft
- [ ] Publish confirmation modal
- [ ] Loading state on publish
- [ ] Success page redirect
- [ ] Error handling

---

### 🔌 API Integration (Sprint 5)

**Endpoints**:
```ts
// VIN Decoder
GET /api/vehicles/vin/:vin

// Image Upload
POST /api/media/upload/init
POST /api/media/upload/finalize/:mediaId

// Vehicle Creation
POST /api/vehicles (draft)
POST /api/vehicles/:id/publish

// Market Value
GET /api/vehicles/market-value?brand=toyota&model=camry&year=2023
```

**Upload Flow**:
```ts
// 1. Init upload para cada imagen
const { mediaId, uploadUrl } = await initUpload(file);

// 2. Upload directo a S3/Storage
await fetch(uploadUrl, { method: 'PUT', body: file });

// 3. Finalize para confirmar
await finalizeUpload(mediaId);

// 4. Guardar mediaIds en el formulario
```

---

### ✅ Sprint 5 Entregables

- [ ] Multi-step form completo
- [ ] Step 1-2: Basic info & details
- [ ] Step 3: Photo upload profesional
- [ ] Step 4-5: Features & pricing
- [ ] Step 6: Review & publish
- [ ] Draft save functionality
- [ ] Form persistence (localStorage)
- [ ] Validation per step
- [ ] Image upload working

**Tests**: 25+ tests (cada step, upload, validación)

---

## 👤 SPRINT 6: User Dashboard
**Duración**: 1 semana (5 días)  
**Objetivo**: Panel de usuario con listings, favoritos, mensajes

### 📋 Tareas

#### Día 1-2: Dashboard Layout & My Listings

**DashboardPage.tsx** (`/dashboard`)

```tsx
// Tabs:
[My Listings] [Favorites] [Messages] [Profile]

// My Listings Tab:
┌─────────────────────────────────────────┐
│  📊 Stats                               │
│  [3 Active] [1 Pending] [2 Sold]       │
├─────────────────────────────────────────┤
│  ACTIVE LISTINGS (3)                    │
│                                         │
│  ┌───────────────────────────────────┐ │
│  │ [IMG] Toyota Camry 2023           │ │
│  │ $35,000 • PUBLISHED               │ │
│  │ 👁️ 234 views • ❤️ 12 favorites   │ │
│  │ 💬 3 inquiries                     │ │
│  │                                   │ │
│  │ [Edit] [Renew] [Boost] [Delete]  │ │
│  └───────────────────────────────────┘ │
│                                         │
│  [+ Add New Listing]                    │
└─────────────────────────────────────────┘
```

**Checklist**:
- [ ] Dashboard layout con tabs
- [ ] Stats cards
- [ ] My Listings list
- [ ] Edit listing button
- [ ] Delete confirmation
- [ ] Mark as sold
- [ ] Boost/renew (placeholder)
- [ ] Empty state

---

#### Día 3: Favorites Tab

**FavoritesTab.tsx**

```tsx
// Grid de vehicles favoritos
┌─────────────────────────────────────────┐
│  SAVED VEHICLES (8)                     │
│                                         │
│  ┌────┐ ┌────┐ ┌────┐ ┌────┐          │
│  │    │ │    │ │    │ │    │          │
│  │ ❤️ │ │ ❤️ │ │ ❤️ │ │ ❤️ │          │
│  └────┘ └────┘ └────┘ └────┘          │
│                                         │
│  [Clear All] [Export List]              │
└─────────────────────────────────────────┘
```

**Features**:
- Grid de VehicleCards
- Remove favorite
- Clear all (confirmation)
- Export list (CSV/PDF)
- Filters/sort
- Empty state

**Checklist**:
- [ ] Favorites grid
- [ ] Remove from favorites
- [ ] Clear all with confirmation
- [ ] Empty state design
- [ ] Loading skeleton

---

#### Día 4: Profile Settings

**ProfileTab.tsx**

```tsx
// Secciones:
1. Profile Photo
   [Avatar] [Upload New Photo] [Remove]

2. Personal Information
   - Name: [__________]
   - Email: [__________] (verified ✓)
   - Phone: [__________]
   - Location: [__________]
   - Bio: [Text area]

3. Account Settings
   - Change Password [Button]
   - Email Preferences [Button]
   - Delete Account [Button - danger]

4. Business Information (for dealers)
   - Business Name
   - License Number
   - Website
```

**Checklist**:
- [ ] Profile form
- [ ] Avatar upload
- [ ] Email verification badge
- [ ] Change password modal
- [ ] Email preferences modal
- [ ] Delete account flow
- [ ] Validation (Zod)
- [ ] Success toasts

---

#### Día 5: Statistics & Analytics

**StatsOverview.tsx**

```tsx
┌─────────────────────────────────────────┐
│  📊 YOUR STATISTICS (Last 30 days)      │
├─────────────────────────────────────────┤
│  ┌─────────┐ ┌─────────┐ ┌─────────┐  │
│  │ 👁️ 1,234│ │ ❤️ 45   │ │ 💬 12   │  │
│  │ Views   │ │ Saves   │ │ Messages │  │
│  │ +15% ↗  │ │ +8% ↗   │ │ -3% ↘    │  │
│  └─────────┘ └─────────┘ └─────────┘  │
│                                         │
│  📈 VIEWS CHART (7 days)                │
│  [Line chart showing daily views]       │
│                                         │
│  🔝 TOP PERFORMING LISTING              │
│  [VehicleCard] - 456 views             │
└─────────────────────────────────────────┘
```

**Checklist**:
- [ ] Stats cards
- [ ] Views chart (recharts o Chart.js)
- [ ] Top listing highlight
- [ ] Date range selector
- [ ] Export report

---

### ✅ Sprint 6 Entregables

- [ ] Dashboard con tabs
- [ ] My Listings management
- [ ] Favorites grid
- [ ] Profile settings
- [ ] Stats & analytics
- [ ] Edit/delete listings
- [ ] Empty states

**Tests**: 15+ tests

---

## 💬 SPRINT 7: Messages & Contact
**Duración**: 1 semana (5 días)  
**Objetivo**: Sistema de mensajería entre compradores y vendedores

### 📋 Tareas

#### Día 1-2: Messages Inbox

**MessagesTab.tsx** (`/dashboard/messages`)

```tsx
// Layout:
┌────────────┬─────────────────────────────┐
│ INBOX      │  CONVERSATION               │
│            │                             │
│ [Search]   │  John Seller                │
│            │  ────────────────────       │
│ ● Active   │  Hey! Is this car still... │
│ John S.    │  Yesterday 2:30 PM          │
│ About: ... │                             │
│            │  Yes, still available!      │
│ Sarah M.   │  Today 9:15 AM              │
│ About: ... │                             │
│            │  [Type message...] [Send]   │
└────────────┴─────────────────────────────┘
```

**Features**:
- Inbox list (conversations)
- Unread badge
- Search conversations
- Selected conversation view
- Real-time updates (WebSocket o polling)
- Message timestamps
- Vehicle context card

**Checklist**:
- [ ] Inbox list component
- [ ] Conversation view
- [ ] Message input
- [ ] Send message
- [ ] Real-time updates
- [ ] Unread count
- [ ] Search functionality
- [ ] Empty state

---

#### Día 3: Contact Seller Flow

**ContactSellerModal.tsx**

```tsx
// Trigger: Desde VehicleDetailPage
┌─────────────────────────────────────┐
│  Contact Seller                     │
│                                     │
│  About: Toyota Camry 2023           │
│  [Vehicle thumbnail]                │
│                                     │
│  Your Name                          │
│  [___________]                      │
│                                     │
│  Email                              │
│  [___________]                      │
│                                     │
│  Phone (optional)                   │
│  [___________]                      │
│                                     │
│  Message                            │
│  [I'm interested in this vehicle...│
│   When would be a good time to...] │
│                                     │
│  [Send Message]                     │
└─────────────────────────────────────┘
```

**Features**:
- Pre-filled message template
- Vehicle context
- Phone optional
- Send creates conversation
- Email notification to seller
- Success confirmation

**Checklist**:
- [ ] Contact modal UI
- [ ] Form validation
- [ ] Send message API
- [ ] Success toast
- [ ] Error handling
- [ ] Loading state

---

#### Día 4: Notifications System

**NotificationDropdown.tsx**

```tsx
// Navbar:
[🔔 3]  ← Badge with count

// Dropdown:
┌─────────────────────────────────────┐
│  NOTIFICATIONS (3)                  │
├─────────────────────────────────────┤
│  💬 New message about Toyota Camry  │
│  2 minutes ago                      │
├─────────────────────────────────────┤
│  ❤️ Someone saved your listing      │
│  1 hour ago                         │
├─────────────────────────────────────┤
│  ✅ Your listing was approved        │
│  Yesterday                          │
├─────────────────────────────────────┤
│  [Mark all as read] [View All]      │
└─────────────────────────────────────┘
```

**Notification Types**:
- New message
- Listing approved/rejected
- Someone saved your listing
- Price drop on favorite
- New review on your listing

**Checklist**:
- [ ] Notification dropdown
- [ ] Badge count
- [ ] Mark as read
- [ ] Mark all as read
- [ ] View all page
- [ ] Real-time updates (polling)
- [ ] Sound/desktop notifications (opcional)

---

#### Día 5: Email Preferences

**EmailPreferencesModal.tsx**

```tsx
┌─────────────────────────────────────┐
│  Email Notifications                │
│                                     │
│  ☑ New messages                     │
│  ☑ Listing updates                  │
│  ☐ Price drops on favorites         │
│  ☑ Weekly digest                    │
│  ☐ Marketing emails                 │
│                                     │
│  [Save Preferences]                 │
└─────────────────────────────────────┘
```

**Checklist**:
- [ ] Preferences form
- [ ] Save API call
- [ ] Success toast
- [ ] Unsubscribe link handling

---

### 🔌 API Integration (Sprint 7)

**Endpoints**:
```ts
GET /api/messages/conversations
GET /api/messages/conversation/:id
POST /api/messages/send
POST /api/contact/seller
GET /api/notifications
PUT /api/notifications/:id/read
PUT /api/notifications/read-all
PUT /api/user/email-preferences
```

---

### ✅ Sprint 7 Entregables

- [ ] Messages inbox funcional
- [ ] Conversation view
- [ ] Contact seller modal
- [ ] Notifications dropdown
- [ ] Email preferences
- [ ] Real-time updates
- [ ] Empty states

**Tests**: 12+ tests

---

## 🛡️ SPRINT 8: Admin Panel
**Duración**: 1 semana (5 días)  
**Objetivo**: Panel de administración para moderar listings

### 📋 Tareas

#### Día 1-2: Admin Dashboard Layout

**AdminDashboardPage.tsx** (`/admin`)

```tsx
// Protección: Solo role='admin'
// Sidebar:
- Dashboard
- Pending Approvals
- All Listings
- Users
- Reports
- Settings

// Main:
┌─────────────────────────────────────┐
│  📊 ADMIN OVERVIEW                  │
│                                     │
│  ┌─────────┐ ┌─────────┐ ┌─────────┐
│  │ 12      │ │ 1,234   │ │ 567     │
│  │ Pending │ │ Active  │ │ Users   │
│  └─────────┘ └─────────┘ └─────────┘
│                                     │
│  📈 ACTIVITY (Last 7 days)          │
│  [Chart]                            │
│                                     │
│  🚨 RECENT REPORTS (3)              │
│  [List]                             │
└─────────────────────────────────────┘
```

**Checklist**:
- [ ] Admin layout with sidebar
- [ ] Role-based protection
- [ ] Stats overview
- [ ] Activity chart
- [ ] Recent reports

---

#### Día 3-4: Pending Approvals

**PendingApprovalsPage.tsx**

```tsx
┌─────────────────────────────────────┐
│  PENDING APPROVALS (12)             │
│                                     │
│  ┌───────────────────────────────┐ │
│  │ [IMG] Toyota Camry 2023       │ │
│  │ Posted by: John Doe           │ │
│  │ Date: 2024-12-01              │ │
│  │                               │ │
│  │ [View Full Listing]           │ │
│  │                               │ │
│  │ ✅ [Approve]  ❌ [Reject]     │ │
│  │                               │ │
│  │ Reason (if reject):           │ │
│  │ [_________________________]   │ │
│  └───────────────────────────────┘ │
└─────────────────────────────────────┘
```

**Features**:
- List pending vehicles
- Full detail preview
- Approve button → status='active'
- Reject with reason → email to seller
- Bulk actions

**Checklist**:
- [ ] Pending list
- [ ] Detail preview modal
- [ ] Approve functionality
- [ ] Reject with reason
- [ ] Email notification
- [ ] Bulk approve/reject

---

#### Día 5: User Management

**UsersManagementPage.tsx**

```tsx
// Table:
┌─────────────────────────────────────────────────┐
│  USERS (567)                   [Search]         │
├─────────────────────────────────────────────────┤
│ Name        Email          Role    Status       │
├─────────────────────────────────────────────────┤
│ John Doe    john@...       Seller  Active  [...│
│ Jane Smith  jane@...       Admin   Active  [...│
│ Bob J.      bob@...        Buyer   Banned  [...│
└─────────────────────────────────────────────────┘

// Actions dropdown:
[...] → [View Profile] [Ban] [Delete]
```

**Checklist**:
- [ ] Users table
- [ ] Search/filter
- [ ] View user profile
- [ ] Ban/unban user
- [ ] Delete user (confirmation)
- [ ] Pagination

---

### ✅ Sprint 8 Entregables

- [ ] Admin dashboard
- [ ] Pending approvals flow
- [ ] User management
- [ ] Reports list
- [ ] Bulk actions
- [ ] Role-based access

**Tests**: 10+ tests

---

## ✨ SPRINT 9: Polish & Testing
**Duración**: 1 semana (5 días)  
**Objetivo**: Refinamiento UX, tests, optimización

### 📋 Tareas

#### Día 1: Performance Optimization

**Checklist**:
- [ ] Lazy loading de imágenes
- [ ] Code splitting (React.lazy)
- [ ] Bundle size optimization
- [ ] Lighthouse audit (score >90)
- [ ] Image compression
- [ ] Caching strategy (React Query)
- [ ] Debounce en búsquedas

---

#### Día 2: Accessibility (a11y)

**Checklist**:
- [ ] Semantic HTML review
- [ ] ARIA labels
- [ ] Keyboard navigation
- [ ] Focus management
- [ ] Color contrast (WCAG AA)
- [ ] Screen reader testing
- [ ] Alt texts en imágenes

---

#### Día 3-4: Testing

**Test Coverage**:
- Unit tests: 70%+
- Integration tests: 50%+
- E2E tests: Critical flows

**Vitest + React Testing Library**:
```bash
npm install -D vitest @testing-library/react @testing-library/jest-dom
```

**Tests a escribir**:
- [ ] Auth flows (login, register)
- [ ] Vehicle search & filters
- [ ] Upload vehicle flow
- [ ] Contact seller
- [ ] Favorites toggle
- [ ] Admin approval

---

#### Día 5: UX Refinements

**Checklist**:
- [ ] Loading states everywhere
- [ ] Empty states design
- [ ] Error messages helpful
- [ ] Success feedback (toasts)
- [ ] Hover states polished
- [ ] Animations suaves
- [ ] Mobile UX review
- [ ] User testing feedback

---

### ✅ Sprint 9 Entregables

- [ ] Performance optimizado
- [ ] Accessibility compliant
- [ ] 50+ tests escritos
- [ ] UX refinado
- [ ] Bug fixes
- [ ] Documentation

---

## 🚀 SPRINT 10: Production Build & Deployment
**Duración**: 1 semana (5 días)  
**Objetivo**: Deploy completo a producción con monitoreo

### 📋 Tareas

#### Día 1: Production Build Optimization

**Vite Configuration** (`vite.config.ts`):

```typescript
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import { visualizer } from 'rollup-plugin-visualizer'
import compression from 'vite-plugin-compression'

export default defineConfig({
  plugins: [
    react(),
    compression({ algorithm: 'gzip' }),
    compression({ algorithm: 'brotliCompress', ext: '.br' }),
    visualizer({ open: false, filename: 'dist/stats.html' })
  ],
  build: {
    target: 'es2015',
    outDir: 'dist',
    assetsDir: 'assets',
    sourcemap: false, // true for staging
    minify: 'terser',
    terserOptions: {
      compress: {
        drop_console: true,
        drop_debugger: true
      }
    },
    rollupOptions: {
      output: {
        manualChunks: {
          'react-vendor': ['react', 'react-dom', 'react-router-dom'],
          'ui-vendor': ['@headlessui/react', 'framer-motion'],
          'form-vendor': ['react-hook-form', 'zod'],
          'query-vendor': ['@tanstack/react-query', 'axios']
        }
      }
    },
    chunkSizeWarningLimit: 500
  },
  optimizeDeps: {
    include: ['react', 'react-dom', 'react-router-dom']
  }
})
```

**Checklist**:
- [ ] Code splitting configurado
- [ ] Lazy loading de rutas
- [ ] Bundle size < 500KB (gzip)
- [ ] Tree shaking verificado
- [ ] Dead code eliminado
- [ ] Source maps para staging
- [ ] Compression (gzip + brotli)
- [ ] Assets optimizados

---

#### Día 2: Environment & Secrets Management

**.env.production**:
```bash
VITE_API_URL=https://api.cardealer.com
VITE_GATEWAY_URL=https://gateway.cardealer.com
VITE_CDN_URL=https://cdn.cardealer.com
VITE_SENTRY_DSN=https://your-sentry-dsn
VITE_GA_TRACKING_ID=G-XXXXXXXXXX
VITE_ENVIRONMENT=production
VITE_ENABLE_ANALYTICS=true
VITE_ENABLE_ERROR_TRACKING=true
VITE_VERSION=$CI_COMMIT_SHA
```

**.env.staging**:
```bash
VITE_API_URL=https://staging-api.cardealer.com
VITE_GATEWAY_URL=https://staging-gateway.cardealer.com
VITE_CDN_URL=https://staging-cdn.cardealer.com
VITE_ENVIRONMENT=staging
VITE_ENABLE_ANALYTICS=false
```

**GitHub Secrets** (Settings → Secrets):
```
DOCKER_USERNAME
DOCKER_PASSWORD
PROD_HOST
PROD_USER
PROD_SSH_KEY
SENTRY_AUTH_TOKEN
VERCEL_TOKEN (if using Vercel)
DO_TOKEN (if using Digital Ocean)
```

**Checklist**:
- [ ] .env.production configurado
- [ ] .env.staging configurado
- [ ] Secrets en GitHub Actions
- [ ] API URLs correctas
- [ ] CDN configurado
- [ ] Sentry DSN
- [ ] Google Analytics ID
- [ ] Feature flags (si aplica)

---

#### Día 3: Deployment Strategies

**Opción 1: Digital Ocean + Docker** (Recomendado)

**deploy-do.sh**:
```bash
#!/bin/bash
set -e

echo "🚀 Deploying to Digital Ocean..."

# Variables
IMAGE_NAME="cardealer-frontend"
DOCKER_REGISTRY="registry.digitalocean.com/cardealer"
VERSION=$(git rev-parse --short HEAD)

# Build and push
docker build -t $DOCKER_REGISTRY/$IMAGE_NAME:$VERSION .
docker push $DOCKER_REGISTRY/$IMAGE_NAME:$VERSION
docker tag $DOCKER_REGISTRY/$IMAGE_NAME:$VERSION $DOCKER_REGISTRY/$IMAGE_NAME:latest
docker push $DOCKER_REGISTRY/$IMAGE_NAME:latest

# Deploy to droplet
ssh root@your-droplet-ip << EOF
  cd /opt/cardealer
  docker pull $DOCKER_REGISTRY/$IMAGE_NAME:latest
  docker-compose up -d frontend
  docker image prune -f
EOF

echo "✅ Deployment complete!"
```

**docker-compose.prod.yml**:
```yaml
version: '3.8'
services:
  frontend:
    image: registry.digitalocean.com/cardealer/cardealer-frontend:latest
    restart: always
    ports:
      - "80:80"
      - "443:443"
    environment:
      - NODE_ENV=production
    volumes:
      - ./nginx.conf:/etc/nginx/conf.d/default.conf:ro
      - ./ssl:/etc/nginx/ssl:ro
      - logs:/var/log/nginx
    networks:
      - cardealer-network
    labels:
      - "com.centurylinklabs.watchtower.enable=true"

  watchtower:
    image: containrrr/watchtower
    volumes:
      - /var/run/docker.sock:/var/run/docker.sock
    command: --interval 300 --cleanup
    restart: always

networks:
  cardealer-network:
    external: true

volumes:
  logs:
```

**Opción 2: Vercel** (Alternativa simple):

```bash
# Install Vercel CLI
npm install -g vercel

# Configure
vercel link

# Deploy
vercel --prod
```

**vercel.json**:
```json
{
  "version": 2,
  "builds": [
    {
      "src": "package.json",
      "use": "@vercel/static-build",
      "config": { "distDir": "dist" }
    }
  ],
  "routes": [
    { "handle": "filesystem" },
    { "src": "/(.*)", "dest": "/index.html" }
  ],
  "env": {
    "VITE_API_URL": "@api_url",
    "VITE_GATEWAY_URL": "@gateway_url"
  }
}
```

**Opción 3: Netlify**:

**netlify.toml**:
```toml
[build]
  command = "npm run build"
  publish = "dist"

[[redirects]]
  from = "/*"
  to = "/index.html"
  status = 200

[[headers]]
  for = "/*"
  [headers.values]
    X-Frame-Options = "DENY"
    X-XSS-Protection = "1; mode=block"
    X-Content-Type-Options = "nosniff"
    Referrer-Policy = "strict-origin-when-cross-origin"

[[headers]]
  for = "/assets/*"
  [headers.values]
    Cache-Control = "public, max-age=31536000, immutable"
```

**Checklist**:
- [ ] Deployment strategy elegida
- [ ] Scripts de deploy creados
- [ ] SSL/TLS certificados configurados
- [ ] DNS configurado
- [ ] Health checks configurados
- [ ] Auto-scaling (si aplica)
- [ ] Rollback strategy definida
- [ ] Blue-green deployment (opcional)

---

#### Día 4: Monitoring & Error Tracking

**Sentry Integration** (`src/main.tsx`):

```tsx
import * as Sentry from "@sentry/react";
import { BrowserTracing } from "@sentry/tracing";

if (import.meta.env.VITE_ENABLE_ERROR_TRACKING === 'true') {
  Sentry.init({
    dsn: import.meta.env.VITE_SENTRY_DSN,
    integrations: [
      new BrowserTracing(),
      new Sentry.Replay()
    ],
    environment: import.meta.env.VITE_ENVIRONMENT,
    release: import.meta.env.VITE_VERSION,
    tracesSampleRate: 0.1,
    replaysSessionSampleRate: 0.1,
    replaysOnErrorSampleRate: 1.0,
    beforeSend(event, hint) {
      // Filter out non-error events
      if (event.level === 'warning') return null;
      return event;
    }
  });
}

const root = ReactDOM.createRoot(document.getElementById('root')!);
root.render(
  <Sentry.ErrorBoundary fallback={<ErrorFallback />}>
    <React.StrictMode>
      <App />
    </React.StrictMode>
  </Sentry.ErrorBoundary>
);
```

**Google Analytics** (`src/utils/analytics.ts`):

```typescript
import ReactGA from 'react-ga4';

export const initGA = () => {
  if (import.meta.env.VITE_ENABLE_ANALYTICS === 'true') {
    ReactGA.initialize(import.meta.env.VITE_GA_TRACKING_ID);
  }
};

export const logPageView = (path: string) => {
  ReactGA.send({ hitType: "pageview", page: path });
};

export const logEvent = (category: string, action: string, label?: string) => {
  ReactGA.event({ category, action, label });
};

// Usage
logEvent('Vehicle', 'View', vehicleId);
logEvent('Contact', 'Send', 'Vehicle Inquiry');
```

**Performance Monitoring** (`src/utils/performance.ts`):

```typescript
import { onCLS, onFID, onFCP, onLCP, onTTFB } from 'web-vitals';

export const reportWebVitals = () => {
  onCLS(console.log);
  onFID(console.log);
  onFCP(console.log);
  onLCP(console.log);
  onTTFB(console.log);
};

// Send to analytics
const sendToAnalytics = (metric: any) => {
  const body = JSON.stringify(metric);
  const url = 'https://analytics.example.com/vitals';
  
  if (navigator.sendBeacon) {
    navigator.sendBeacon(url, body);
  } else {
    fetch(url, { body, method: 'POST', keepalive: true });
  }
};
```

**Health Check Endpoint** (`public/health.json`):

```json
{
  "status": "healthy",
  "version": "1.0.0",
  "timestamp": "2025-12-04T00:00:00Z"
}
```

**Uptime Monitoring** (UptimeRobot, Pingdom):
- Check `/health.json` every 5 minutes
- Alert on 3 consecutive failures
- Alert channels: Email, Slack, SMS

**Checklist**:
- [ ] Sentry configurado
- [ ] Google Analytics integrado
- [ ] Web Vitals tracking
- [ ] Error boundary global
- [ ] Health check endpoint
- [ ] Uptime monitoring
- [ ] Alerting configurado
- [ ] Dashboard de métricas

---

#### Día 5: Security & Performance Hardening

**Security Headers** (nginx o Netlify):

```nginx
# nginx.conf additions
add_header X-Frame-Options "SAMEORIGIN" always;
add_header X-Content-Type-Options "nosniff" always;
add_header X-XSS-Protection "1; mode=block" always;
add_header Referrer-Policy "strict-origin-when-cross-origin" always;
add_header Permissions-Policy "geolocation=(), microphone=(), camera=()" always;
add_header Content-Security-Policy "default-src 'self'; script-src 'self' 'unsafe-inline' https://www.googletagmanager.com; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; font-src 'self' data:; connect-src 'self' https://api.cardealer.com https://sentry.io;" always;
```

**Performance Optimizations**:

```typescript
// src/App.tsx - Route-based code splitting
const HomePage = lazy(() => import('./pages/HomePage'));
const VehiclesPage = lazy(() => import('./pages/VehiclesPage'));
const VehicleDetailPage = lazy(() => import('./pages/VehicleDetailPage'));

// Image lazy loading component
<img 
  loading="lazy" 
  decoding="async"
  src={imageUrl}
  alt={alt}
/>

// Preconnect to external domains
<link rel="preconnect" href="https://api.cardealer.com" />
<link rel="dns-prefetch" href="https://cdn.cardealer.com" />

// Preload critical resources
<link rel="preload" href="/fonts/inter.woff2" as="font" type="font/woff2" crossorigin />
```

**Lighthouse CI** (`.github/workflows/lighthouse.yml`):

```yaml
name: Lighthouse CI
on: [push]
jobs:
  lighthouse:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
      - run: npm ci && npm run build
      - run: npm install -g @lhci/cli
      - run: lhci autorun
        env:
          LHCI_GITHUB_APP_TOKEN: ${{ secrets.LHCI_GITHUB_APP_TOKEN }}
```

**lighthouserc.json**:

```json
{
  "ci": {
    "collect": {
      "startServerCommand": "npm run preview",
      "url": ["http://localhost:4173"],
      "numberOfRuns": 3
    },
    "assert": {
      "assertions": {
        "categories:performance": ["error", {"minScore": 0.9}],
        "categories:accessibility": ["error", {"minScore": 0.9}],
        "categories:best-practices": ["error", {"minScore": 0.9}],
        "categories:seo": ["error", {"minScore": 0.9}]
      }
    },
    "upload": {
      "target": "temporary-public-storage"
    }
  }
}
```

**Pre-deployment Checklist**:
- [ ] Security headers configurados
- [ ] CSP policy definida
- [ ] HTTPS forzado
- [ ] Rate limiting en API
- [ ] Input sanitization
- [ ] XSS protection
- [ ] CSRF tokens
- [ ] Lighthouse score > 90
- [ ] Bundle size optimizado
- [ ] Images optimizadas (WebP)
- [ ] Fonts preloaded
- [ ] Critical CSS inlined
- [ ] Service Worker (PWA opcional)

---

### ✅ Sprint 10 Entregables

**Infrastructure**:
- [ ] Dockerfile production-ready
- [ ] docker-compose.prod.yml
- [ ] nginx.conf optimizado
- [ ] SSL certificates configurados

**CI/CD**:
- [ ] GitHub Actions workflows
- [ ] Automated testing
- [ ] Docker image builds
- [ ] Automated deployment

**Monitoring**:
- [ ] Sentry error tracking
- [ ] Google Analytics
- [ ] Web Vitals monitoring
- [ ] Uptime monitoring
- [ ] Health checks

**Security**:
- [ ] Security headers
- [ ] CSP policy
- [ ] HTTPS enabled
- [ ] Secrets management

**Performance**:
- [ ] Lighthouse score > 90
- [ ] Bundle optimizado
- [ ] Code splitting
- [ ] Lazy loading
- [ ] Image optimization

**Documentation**:
- [ ] Deployment runbook
- [ ] Rollback procedures
- [ ] Monitoring dashboards
- [ ] Alert runbook

---

## 📦 POST-DEPLOYMENT

### Monitoring Dashboards

**Sentry Dashboard**:
- Error rates
- Performance metrics
- User sessions
- Release tracking

**Google Analytics**:
- User acquisition
- Behavior flow
- Conversion tracking
- Page performance

**Lighthouse CI**:
- Performance scores
- Accessibility issues
- SEO audits
- Best practices

### Incident Response

**Runbook** (`docs/incident-response.md`):

```markdown
# Incident Response Runbook

## Severity Levels
- **P0**: Complete outage → Respond in 15 min
- **P1**: Major feature broken → Respond in 1 hour
- **P2**: Minor issue → Respond in 4 hours
- **P3**: Enhancement → Next sprint

## Rollback Procedure
```bash
# Quick rollback to previous version
ssh root@prod-server
cd /opt/cardealer
docker-compose down frontend
docker tag cardealer-frontend:previous cardealer-frontend:latest
docker-compose up -d frontend
```

## Common Issues
1. **High error rate**: Check Sentry, review recent deploys
2. **Slow performance**: Check CDN, API gateway
3. **Failed deployment**: Review GitHub Actions logs
```

---

## 📊 MÉTRICAS DE ÉXITO

### Performance
- [ ] Lighthouse Score > 90
- [ ] FCP < 1.5s
- [ ] TTI < 3s
- [ ] Bundle size < 500KB

### Quality
- [ ] Test coverage > 70%
- [ ] 0 console errors
- [ ] 0 accessibility violations
- [ ] Mobile-friendly (Google test)

### Features
- [ ] 8 servicios frontend completos
- [ ] 50+ componentes reutilizables
- [ ] Auth completo
- [ ] Upload funcional
- [ ] Admin panel operativo

---

## 🎯 POST-LAUNCH ROADMAP

Después del Sprint 10 (Production):
- **Sprint 11**: SEO & Marketing features (Meta tags, Schema.org, Sitemap)
- **Sprint 12**: Advanced features (Comparador, Financing calculator, Trade-in estimator)
- **Sprint 13**: PWA features (Offline mode, Push notifications, Install prompt)
- **Sprint 14**: Analytics & Optimization (A/B testing, Conversion funnels, UX improvements)

---

## 📊 DEPLOYMENT CHECKLIST FINAL

### Pre-Launch (1 semana antes)
- [ ] Load testing completado
- [ ] Security audit realizado
- [ ] Backup strategy validada
- [ ] Rollback procedure probado
- [ ] Monitoring dashboards configurados
- [ ] Alert rules definidas
- [ ] Documentation completa
- [ ] Stakeholder demo realizada

### Launch Day
- [ ] Deploy a staging completado
- [ ] Smoke tests passed
- [ ] Deploy a production ejecutado
- [ ] DNS propagation verificado
- [ ] SSL certificates válidos
- [ ] Health checks passing
- [ ] Monitoring activo
- [ ] Team on standby (2 hours)

### Post-Launch (primeras 24 hours)
- [ ] Error rates monitoreados
- [ ] Performance metrics revisados
- [ ] User feedback recolectado
- [ ] Hotfixes deployed (si necesario)
- [ ] Post-mortem programado
- [ ] Success metrics reportados

---

## 🏆 MÉTRICAS DE ÉXITO FINALES

### Technical Metrics
- ✅ Lighthouse Performance: > 90
- ✅ Lighthouse Accessibility: > 90
- ✅ Lighthouse Best Practices: > 90
- ✅ Lighthouse SEO: > 90
- ✅ Test Coverage: > 70%
- ✅ Bundle Size (gzip): < 500KB
- ✅ Time to Interactive: < 3s
- ✅ First Contentful Paint: < 1.5s

### Business Metrics
- ✅ 8 microservicios integrados
- ✅ 60+ componentes reutilizables
- ✅ 150+ tests automatizados
- ✅ 100% features core completadas
- ✅ Mobile responsive (100%)
- ✅ Multi-browser compatible
- ✅ Accessibility compliant (WCAG AA)
- ✅ Production-ready deployment

### User Experience
- ✅ < 2s page load time
- ✅ Smooth animations (60fps)
- ✅ Intuitive navigation
- ✅ Professional design
- ✅ Clear error messages
- ✅ Helpful empty states
- ✅ Responsive on all devices
- ✅ Keyboard accessible

---

**FIN DEL PLAN DE SPRINTS FRONTEND** 🚀🎉

**Total Duration**: 11-13 semanas  
**Total Sprints**: 10 sprints + Post-Launch  
**Deliverables**: Production-ready frontend application con deploy completo
