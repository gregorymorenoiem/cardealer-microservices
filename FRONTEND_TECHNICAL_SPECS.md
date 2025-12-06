# 🏗️ Especificaciones Técnicas Frontend - CarDealer

> **Stack**: React 18 + Vite 5 + TypeScript 5.3  
> **Fecha**: Diciembre 2025  
> **Objetivo**: Arquitectura escalable y mantenible

---

## 📦 TECH STACK COMPLETO

### Core Framework
```json
{
  "react": "^18.3.1",
  "react-dom": "^18.3.1",
  "vite": "^5.1.0",
  "typescript": "^5.3.3"
}
```

### Routing & Navigation
```json
{
  "react-router-dom": "^6.22.0"
}
```

### State Management
```json
{
  "zustand": "^4.5.0",              // Cliente state (global)
  "@tanstack/react-query": "^5.20.0" // Server state (cache)
}
```

### HTTP Client
```json
{
  "axios": "^1.6.7"
}
```

### UI & Styling
```json
{
  "tailwindcss": "^3.4.1",
  "clsx": "^2.1.0",
  "@headlessui/react": "^1.7.18",   // Accessible components
  "framer-motion": "^11.0.3"        // Animations
}
```

### Forms & Validation
```json
{
  "react-hook-form": "^7.50.0",
  "zod": "^3.22.4",
  "@hookform/resolvers": "^3.3.4"
}
```

### Image Handling
```json
{
  "react-dropzone": "^14.2.3",
  "browser-image-compression": "^2.0.2",
  "swiper": "^11.0.6",
  "yet-another-react-lightbox": "^3.15.0",
  "react-easy-crop": "^5.0.4"
}
```

### Icons & Assets
```json
{
  "react-icons": "^5.0.1",          // Icon library
  "lucide-react": "^0.316.0"        // Alternative icons
}
```

### Utils & Helpers
```json
{
  "date-fns": "^3.3.1",
  "lodash-es": "^4.17.21",
  "currency.js": "^2.0.4",
  "react-hot-toast": "^2.4.1"       // Notifications
}
```

### Testing
```json
{
  "vitest": "^1.2.2",
  "@testing-library/react": "^14.1.2",
  "@testing-library/jest-dom": "^6.2.0",
  "@testing-library/user-event": "^14.5.2",
  "jsdom": "^24.0.0"
}
```

### Dev Tools
```json
{
  "eslint": "^8.56.0",
  "prettier": "^3.2.5",
  "@typescript-eslint/eslint-plugin": "^6.21.0",
  "@typescript-eslint/parser": "^6.21.0",
  "autoprefixer": "^10.4.17",
  "postcss": "^8.4.35"
}
```

---

## 📁 ESTRUCTURA DE CARPETAS DETALLADA

```
cardealer-frontend/
├── public/
│   ├── favicon.ico
│   ├── images/
│   │   ├── logo.svg
│   │   ├── placeholder-car.jpg
│   │   └── hero-background.jpg
│   └── locales/              # i18n (futuro)
│       ├── en.json
│       └── es.json
│
├── src/
│   ├── assets/               # Imágenes, iconos importados
│   │   ├── images/
│   │   └── icons/
│   │
│   ├── components/           # Componentes reutilizables
│   │   ├── atoms/            # Componentes básicos
│   │   │   ├── Button/
│   │   │   │   ├── Button.tsx
│   │   │   │   ├── Button.test.tsx
│   │   │   │   └── index.ts
│   │   │   ├── Input/
│   │   │   ├── Badge/
│   │   │   ├── Spinner/
│   │   │   ├── Avatar/
│   │   │   └── Icon/
│   │   │
│   │   ├── molecules/        # Combinaciones de atoms
│   │   │   ├── FormField/
│   │   │   ├── SearchBar/
│   │   │   ├── PriceDisplay/
│   │   │   ├── Rating/
│   │   │   ├── VehicleSpecs/
│   │   │   └── ImageUploader/
│   │   │
│   │   ├── organisms/        # Componentes complejos
│   │   │   ├── Navbar/
│   │   │   ├── Footer/
│   │   │   ├── VehicleCard/
│   │   │   ├── VehicleGallery/
│   │   │   ├── FilterSidebar/
│   │   │   ├── ContactForm/
│   │   │   └── ReviewsList/
│   │   │
│   │   └── templates/        # Layout wrappers
│   │       ├── PageContainer/
│   │       └── Section/
│   │
│   ├── features/             # Feature-based modules
│   │   ├── auth/
│   │   │   ├── components/   # Feature-specific components
│   │   │   │   ├── LoginForm.tsx
│   │   │   │   ├── RegisterForm.tsx
│   │   │   │   └── ProtectedRoute.tsx
│   │   │   ├── hooks/        # Custom hooks
│   │   │   │   ├── useAuth.ts
│   │   │   │   └── useLogin.ts
│   │   │   ├── services/     # API calls
│   │   │   │   └── authService.ts
│   │   │   ├── store/        # Zustand slices
│   │   │   │   └── authStore.ts
│   │   │   ├── types/        # TypeScript types
│   │   │   │   └── auth.types.ts
│   │   │   └── utils/        # Helpers
│   │   │       └── validation.ts
│   │   │
│   │   ├── vehicles/
│   │   │   ├── components/
│   │   │   │   ├── VehicleList/
│   │   │   │   ├── VehicleDetail/
│   │   │   │   └── VehicleForm/
│   │   │   ├── hooks/
│   │   │   │   ├── useVehicles.ts
│   │   │   │   ├── useVehicleDetail.ts
│   │   │   │   └── useVehicleFilters.ts
│   │   │   ├── services/
│   │   │   │   └── vehicleService.ts
│   │   │   ├── store/
│   │   │   │   └── vehicleStore.ts
│   │   │   └── types/
│   │   │       └── vehicle.types.ts
│   │   │
│   │   ├── user/
│   │   │   ├── components/
│   │   │   │   ├── Dashboard/
│   │   │   │   ├── Profile/
│   │   │   │   └── Favorites/
│   │   │   ├── hooks/
│   │   │   ├── services/
│   │   │   └── types/
│   │   │
│   │   ├── messages/
│   │   ├── admin/
│   │   └── search/
│   │
│   ├── hooks/                # Global custom hooks
│   │   ├── useDebounce.ts
│   │   ├── useLocalStorage.ts
│   │   ├── useMediaQuery.ts
│   │   ├── useIntersectionObserver.ts
│   │   └── useScrollLock.ts
│   │
│   ├── layouts/              # App layouts
│   │   ├── MainLayout/
│   │   │   ├── MainLayout.tsx
│   │   │   ├── Header.tsx
│   │   │   └── Footer.tsx
│   │   ├── AuthLayout/
│   │   │   └── AuthLayout.tsx
│   │   └── DashboardLayout/
│   │       ├── DashboardLayout.tsx
│   │       └── Sidebar.tsx
│   │
│   ├── pages/                # Page components (route-based)
│   │   ├── HomePage.tsx
│   │   ├── LoginPage.tsx
│   │   ├── RegisterPage.tsx
│   │   ├── VehiclesPage.tsx
│   │   ├── VehicleDetailPage.tsx
│   │   ├── SellVehiclePage.tsx
│   │   ├── DashboardPage.tsx
│   │   ├── ProfilePage.tsx
│   │   ├── AdminPage.tsx
│   │   └── NotFoundPage.tsx
│   │
│   ├── services/             # API services
│   │   ├── api.ts            # Axios instance + interceptors
│   │   └── endpoints/
│   │       ├── authEndpoints.ts
│   │       ├── vehicleEndpoints.ts
│   │       ├── userEndpoints.ts
│   │       └── messageEndpoints.ts
│   │
│   ├── store/                # Global Zustand stores
│   │   ├── index.ts          # Store composition
│   │   ├── authStore.ts
│   │   ├── uiStore.ts        # UI state (modals, drawers)
│   │   └── notificationStore.ts
│   │
│   ├── styles/               # Global styles
│   │   ├── index.css         # Tailwind imports
│   │   ├── theme.css         # CSS variables
│   │   └── animations.css    # Custom animations
│   │
│   ├── types/                # Global TypeScript types
│   │   ├── index.ts
│   │   ├── api.types.ts
│   │   └── common.types.ts
│   │
│   ├── utils/                # Helper functions
│   │   ├── api.utils.ts
│   │   ├── date.utils.ts
│   │   ├── format.utils.ts
│   │   ├── validation.utils.ts
│   │   └── constants.ts
│   │
│   ├── config/               # App configuration
│   │   ├── env.ts            # Environment variables
│   │   └── routes.ts         # Route constants
│   │
│   ├── App.tsx               # Root component
│   ├── main.tsx              # Entry point
│   └── router.tsx            # React Router config
│
├── .env.example              # Env template
├── .env.development          # Dev environment
├── .env.production           # Prod environment
├── .eslintrc.json            # ESLint config
├── .prettierrc               # Prettier config
├── .gitignore
├── index.html                # HTML template
├── package.json
├── postcss.config.js
├── tailwind.config.js
├── tsconfig.json             # TypeScript config
├── tsconfig.node.json
├── vite.config.ts            # Vite config
└── vitest.config.ts          # Vitest config
```

---

## ⚙️ CONFIGURACIÓN VITE

**vite.config.ts**:
```ts
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import path from 'path';

export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
      '@components': path.resolve(__dirname, './src/components'),
      '@features': path.resolve(__dirname, './src/features'),
      '@hooks': path.resolve(__dirname, './src/hooks'),
      '@utils': path.resolve(__dirname, './src/utils'),
      '@types': path.resolve(__dirname, './src/types'),
      '@services': path.resolve(__dirname, './src/services'),
      '@store': path.resolve(__dirname, './src/store'),
      '@assets': path.resolve(__dirname, './src/assets'),
    },
  },
  server: {
    port: 3000,
    proxy: {
      '/api': {
        target: process.env.VITE_API_URL || 'http://localhost:5000',
        changeOrigin: true,
      },
    },
  },
  build: {
    outDir: 'dist',
    sourcemap: true,
    rollupOptions: {
      output: {
        manualChunks: {
          vendor: ['react', 'react-dom', 'react-router-dom'],
          ui: ['@headlessui/react', 'framer-motion'],
        },
      },
    },
  },
});
```

---

## 🎨 CONFIGURACIÓN TAILWIND

**tailwind.config.js**:
```js
/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        primary: {
          50: '#e6f0ff',
          100: '#b3d1ff',
          200: '#80b3ff',
          300: '#4d94ff',
          400: '#1a75ff',
          500: '#00539F', // Main
          600: '#004080',
          700: '#003060',
          800: '#002040',
          900: '#001020',
        },
        secondary: {
          500: '#0089FF',
        },
        accent: {
          500: '#FF6B35',
        },
      },
      fontFamily: {
        sans: ['Inter', 'system-ui', 'sans-serif'],
        heading: ['Poppins', 'sans-serif'],
      },
      spacing: {
        '128': '32rem',
        '144': '36rem',
      },
      borderRadius: {
        '4xl': '2rem',
      },
      boxShadow: {
        'card': '0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06)',
        'card-hover': '0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -2px rgba(0, 0, 0, 0.05)',
      },
    },
  },
  plugins: [
    require('@tailwindcss/forms'),
    require('@tailwindcss/aspect-ratio'),
  ],
};
```

---

## 🔌 AXIOS CONFIGURATION

**src/services/api.ts**:
```ts
import axios, { AxiosError, AxiosInstance } from 'axios';
import { useAuthStore } from '@store/authStore';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000';

// Create axios instance
const api: AxiosInstance = axios.create({
  baseURL: API_BASE_URL,
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor - Add auth token
api.interceptors.request.use(
  (config) => {
    const { accessToken } = useAuthStore.getState();
    if (accessToken) {
      config.headers.Authorization = `Bearer ${accessToken}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Response interceptor - Handle errors & token refresh
api.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config;

    // Token expired - attempt refresh
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;

      try {
        const { refreshToken, setTokens } = useAuthStore.getState();
        const response = await axios.post(`${API_BASE_URL}/api/auth/refresh-token`, {
          refreshToken,
        });

        const { accessToken: newAccessToken, refreshToken: newRefreshToken } = response.data;
        setTokens(newAccessToken, newRefreshToken);

        // Retry original request with new token
        originalRequest.headers.Authorization = `Bearer ${newAccessToken}`;
        return api(originalRequest);
      } catch (refreshError) {
        // Refresh failed - logout user
        useAuthStore.getState().logout();
        window.location.href = '/login';
        return Promise.reject(refreshError);
      }
    }

    return Promise.reject(error);
  }
);

export default api;
```

---

## 🗃️ ZUSTAND STORE PATTERN

**src/store/authStore.ts**:
```ts
import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import { authService } from '@features/auth/services/authService';
import { User, LoginCredentials, RegisterData } from '@features/auth/types/auth.types';

interface AuthState {
  // State
  user: User | null;
  accessToken: string | null;
  refreshToken: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;

  // Actions
  login: (credentials: LoginCredentials) => Promise<void>;
  register: (data: RegisterData) => Promise<void>;
  logout: () => void;
  setUser: (user: User) => void;
  setTokens: (accessToken: string, refreshToken: string) => void;
  clearError: () => void;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      // Initial state
      user: null,
      accessToken: null,
      refreshToken: null,
      isAuthenticated: false,
      isLoading: false,
      error: null,

      // Login action
      login: async (credentials) => {
        set({ isLoading: true, error: null });
        try {
          const response = await authService.login(credentials);
          const { user, accessToken, refreshToken } = response.data;
          
          set({
            user,
            accessToken,
            refreshToken,
            isAuthenticated: true,
            isLoading: false,
          });
        } catch (error: any) {
          set({
            error: error.response?.data?.message || 'Login failed',
            isLoading: false,
          });
          throw error;
        }
      },

      // Register action
      register: async (data) => {
        set({ isLoading: true, error: null });
        try {
          const response = await authService.register(data);
          const { user, accessToken, refreshToken } = response.data;
          
          set({
            user,
            accessToken,
            refreshToken,
            isAuthenticated: true,
            isLoading: false,
          });
        } catch (error: any) {
          set({
            error: error.response?.data?.message || 'Registration failed',
            isLoading: false,
          });
          throw error;
        }
      },

      // Logout action
      logout: () => {
        const { refreshToken } = get();
        if (refreshToken) {
          authService.logout(refreshToken).catch(() => {});
        }
        set({
          user: null,
          accessToken: null,
          refreshToken: null,
          isAuthenticated: false,
          error: null,
        });
      },

      // Setters
      setUser: (user) => set({ user }),
      setTokens: (accessToken, refreshToken) => set({ accessToken, refreshToken }),
      clearError: () => set({ error: null }),
    }),
    {
      name: 'auth-storage',
      partialize: (state) => ({
        user: state.user,
        accessToken: state.accessToken,
        refreshToken: state.refreshToken,
        isAuthenticated: state.isAuthenticated,
      }),
    }
  )
);
```

---

## 🪝 REACT QUERY CONFIGURATION

**src/App.tsx**:
```tsx
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ReactQueryDevtools } from '@tanstack/react-query-devtools';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 5 * 60 * 1000, // 5 minutes
      cacheTime: 10 * 60 * 1000, // 10 minutes
      retry: 1,
      refetchOnWindowFocus: false,
    },
  },
});

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <Router />
      {import.meta.env.DEV && <ReactQueryDevtools initialIsOpen={false} />}
    </QueryClientProvider>
  );
}
```

**Custom Hook Example**:
```ts
// src/features/vehicles/hooks/useVehicles.ts
import { useQuery } from '@tanstack/react-query';
import { vehicleService } from '../services/vehicleService';
import { VehicleFilters } from '../types/vehicle.types';

export const useVehicles = (filters: VehicleFilters, page: number = 1) => {
  return useQuery({
    queryKey: ['vehicles', filters, page],
    queryFn: () => vehicleService.getVehicles(filters, page),
    keepPreviousData: true,
    staleTime: 2 * 60 * 1000, // 2 minutes
  });
};

export const useVehicleDetail = (id: string) => {
  return useQuery({
    queryKey: ['vehicle', id],
    queryFn: () => vehicleService.getVehicleById(id),
    enabled: !!id,
  });
};
```

---

## 🔐 ENVIRONMENT VARIABLES

**.env.example**:
```env
# API
VITE_API_URL=http://localhost:5000
VITE_GATEWAY_URL=http://localhost:15095

# Auth
VITE_JWT_SECRET=your-secret-key

# Upload
VITE_MAX_FILE_SIZE=10485760
VITE_MAX_FILES=20

# Google OAuth (optional)
VITE_GOOGLE_CLIENT_ID=your-google-client-id

# Sentry (optional)
VITE_SENTRY_DSN=your-sentry-dsn

# Feature Flags
VITE_ENABLE_ANALYTICS=true
VITE_ENABLE_CHAT=true
```

---

## 🧪 TESTING SETUP

**vitest.config.ts**:
```ts
import { defineConfig } from 'vitest/config';
import react from '@vitejs/plugin-react';
import path from 'path';

export default defineConfig({
  plugins: [react()],
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: './src/tests/setup.ts',
    coverage: {
      provider: 'v8',
      reporter: ['text', 'json', 'html'],
      exclude: [
        'node_modules/',
        'src/tests/',
        '**/*.config.js',
        '**/*.config.ts',
      ],
    },
  },
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
});
```

**src/tests/setup.ts**:
```ts
import '@testing-library/jest-dom';
import { cleanup } from '@testing-library/react';
import { afterEach } from 'vitest';

// Cleanup after each test
afterEach(() => {
  cleanup();
});
```

**Example Test**:
```tsx
// src/components/atoms/Button/Button.test.tsx
import { render, screen, fireEvent } from '@testing-library/react';
import { describe, it, expect, vi } from 'vitest';
import Button from './Button';

describe('Button', () => {
  it('renders with text', () => {
    render(<Button>Click me</Button>);
    expect(screen.getByRole('button')).toHaveTextContent('Click me');
  });

  it('calls onClick when clicked', () => {
    const handleClick = vi.fn();
    render(<Button onClick={handleClick}>Click me</Button>);
    
    fireEvent.click(screen.getByRole('button'));
    expect(handleClick).toHaveBeenCalledTimes(1);
  });

  it('is disabled when loading', () => {
    render(<Button loading>Click me</Button>);
    expect(screen.getByRole('button')).toBeDisabled();
  });
});
```

---

## 🎨 COMPONENT STANDARDS

### Naming Conventions
- **Components**: PascalCase (`VehicleCard.tsx`)
- **Hooks**: camelCase with `use` prefix (`useVehicles.ts`)
- **Utils**: camelCase (`formatPrice.ts`)
- **Types**: PascalCase with `.types.ts` suffix
- **Constants**: UPPERCASE (`API_ENDPOINTS.ts`)

### File Structure (Component)
```
ComponentName/
├── ComponentName.tsx       # Main component
├── ComponentName.test.tsx  # Tests
├── ComponentName.types.ts  # TypeScript types
├── ComponentName.styles.ts # Styled components (si aplica)
└── index.ts                # Export barrel
```

### Component Template
```tsx
// VehicleCard/VehicleCard.tsx
import { FC } from 'react';
import clsx from 'clsx';
import { VehicleCardProps } from './VehicleCard.types';

/**
 * VehicleCard Component
 * Displays vehicle information in a card format
 * 
 * @param vehicle - Vehicle data
 * @param viewMode - Display mode (grid/list)
 * @param onFavorite - Favorite toggle callback
 */
export const VehicleCard: FC<VehicleCardProps> = ({
  vehicle,
  viewMode = 'grid',
  onFavorite,
  className,
}) => {
  return (
    <article 
      className={clsx(
        'vehicle-card',
        viewMode === 'grid' ? 'vehicle-card--grid' : 'vehicle-card--list',
        className
      )}
    >
      {/* Component content */}
    </article>
  );
};

export default VehicleCard;
```

---

## 🚀 PERFORMANCE OPTIMIZATIONS

### Code Splitting
```tsx
// Lazy load routes
import { lazy, Suspense } from 'react';

const VehiclesPage = lazy(() => import('@/pages/VehiclesPage'));
const VehicleDetailPage = lazy(() => import('@/pages/VehicleDetailPage'));

<Suspense fallback={<Loading />}>
  <Routes>
    <Route path="/vehicles" element={<VehiclesPage />} />
    <Route path="/vehicles/:id" element={<VehicleDetailPage />} />
  </Routes>
</Suspense>
```

### Image Optimization
```tsx
// Lazy loading images
<img 
  src={vehicle.image} 
  alt={vehicle.name}
  loading="lazy"
  decoding="async"
/>

// Responsive images
<picture>
  <source srcset="image-large.webp" media="(min-width: 1024px)" />
  <source srcset="image-medium.webp" media="(min-width: 768px)" />
  <img src="image-small.webp" alt="Vehicle" />
</picture>
```

### Memoization
```tsx
import { memo, useMemo, useCallback } from 'react';

export const VehicleCard = memo(({ vehicle, onFavorite }) => {
  const formattedPrice = useMemo(() => 
    formatCurrency(vehicle.price), 
    [vehicle.price]
  );

  const handleFavorite = useCallback(() => {
    onFavorite(vehicle.id);
  }, [vehicle.id, onFavorite]);

  return (
    <div>
      <p>{formattedPrice}</p>
      <button onClick={handleFavorite}>Favorite</button>
    </div>
  );
});
```

---

## 📱 RESPONSIVE DESIGN

### Breakpoints
```ts
export const breakpoints = {
  sm: '640px',   // Mobile landscape
  md: '768px',   // Tablet
  lg: '1024px',  // Desktop
  xl: '1280px',  // Large desktop
  '2xl': '1536px' // Extra large
};
```

### Mobile-First CSS
```css
/* Mobile first */
.vehicle-card {
  width: 100%;
}

/* Tablet */
@media (min-width: 768px) {
  .vehicle-card {
    width: 50%;
  }
}

/* Desktop */
@media (min-width: 1024px) {
  .vehicle-card {
    width: 33.333%;
  }
}
```

---

## ♿ ACCESSIBILITY

### ARIA Labels
```tsx
<button aria-label="Add to favorites">
  <HeartIcon />
</button>

<input 
  type="text" 
  id="search" 
  aria-describedby="search-help"
/>
<span id="search-help">Search for vehicles by brand or model</span>
```

### Keyboard Navigation
```tsx
const handleKeyDown = (e: KeyboardEvent) => {
  if (e.key === 'Enter' || e.key === ' ') {
    handleClick();
  }
};

<div 
  role="button"
  tabIndex={0}
  onKeyDown={handleKeyDown}
  onClick={handleClick}
>
  Clickable
</div>
```

---

## 📦 BUILD & DEPLOYMENT

### Production Build
```bash
npm run build
# Output: dist/
```

### Preview Build
```bash
npm run preview
# Serves dist/ on localhost:4173
```

### Docker (Optional)
```dockerfile
FROM node:20-alpine AS builder
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build

FROM nginx:alpine
COPY --from=builder /app/dist /usr/share/nginx/html
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

---

**FIN DE ESPECIFICACIONES TÉCNICAS** 🏗️
