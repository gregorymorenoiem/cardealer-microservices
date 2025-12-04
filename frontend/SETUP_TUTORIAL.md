# 🚀 Tutorial: Configuración Inicial Frontend - Sin Errores

> **Guía paso a paso para configurar un proyecto React + Vite + TypeScript + Tailwind CSS**

---

## 📋 Prerequisitos

Antes de comenzar, asegúrate de tener instalado:

- ✅ **Node.js 20+** → [Descargar](https://nodejs.org/)
- ✅ **npm 10+** (viene con Node.js)
- ✅ **Git** → [Descargar](https://git-scm.com/)
- ✅ **Visual Studio Code** → [Descargar](https://code.visualstudio.com/)

Verifica las versiones:
```powershell
node --version    # Debe ser v20.x o superior
npm --version     # Debe ser v10.x o superior
```

---

## 🎯 PASO 1: Crear Proyecto Base con Vite

### 1.1 Crear carpeta del proyecto

```powershell
# Navega al directorio del repositorio
cd c:\Users\gmoreno\source\repos\cardealer

# Crea el folder frontend
mkdir frontend
cd frontend
```

### 1.2 Inicializar proyecto Vite

```powershell
# Opción A: Instalación interactiva (RECOMENDADO)
npm create vite@latest . -- --template react-ts

# Durante la instalación:
# ✓ Select a framework: › React
# ✓ Select a variant: › TypeScript
# ✓ Install with npm and start now?: › No (selecciona No)
```

**⚠️ IMPORTANTE**: NO inicies el servidor todavía. Primero instalaremos todas las dependencias.

---

## 🎯 PASO 2: Instalar Dependencias Core

### 2.1 Instalar dependencias principales

```powershell
# Asegúrate de estar en la carpeta frontend
cd c:\Users\gmoreno\source\repos\cardealer\frontend

# Instala las dependencias principales
npm install react-router-dom zustand @tanstack/react-query axios clsx
```

**Dependencias instaladas**:
- `react-router-dom` → Routing
- `zustand` → State management
- `@tanstack/react-query` → Server state & caching
- `axios` → HTTP client
- `clsx` → Utility para clases CSS

### 2.2 Instalar dependencias de desarrollo

```powershell
# Instala Tailwind CSS v3 (IMPORTANTE: v3, no v4)
npm install -D tailwindcss@3.4.1 postcss@8.4.35 autoprefixer@10.4.18
```

**⚠️ CRÍTICO**: Usa Tailwind CSS **v3.4.1**, no la v4, para evitar conflictos con PostCSS.

### 2.3 Instalar librerías UI y formularios

```powershell
# UI Components
npm install @headlessui/react framer-motion react-icons

# Formularios y validación
npm install react-hook-form zod @hookform/resolvers
```

---

## 🎯 PASO 3: Configurar Tailwind CSS (CRÍTICO)

### 3.1 Crear archivo de configuración PostCSS

**⚠️ IMPORTANTE**: El archivo DEBE tener extensión `.cjs` (CommonJS), no `.js`

Crea el archivo `postcss.config.cjs`:

```powershell
New-Item -ItemType File -Path postcss.config.cjs
```

**Contenido de `postcss.config.cjs`**:
```javascript
module.exports = {
  plugins: {
    tailwindcss: {},
    autoprefixer: {},
  },
}
```

### 3.2 Crear archivo de configuración Tailwind

**⚠️ IMPORTANTE**: También con extensión `.cjs`

Crea el archivo `tailwind.config.cjs`:

```powershell
New-Item -ItemType File -Path tailwind.config.cjs
```

**Contenido de `tailwind.config.cjs`**:
```javascript
/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        primary: {
          DEFAULT: '#00539F',
          50: '#E6F2FF',
          100: '#CCE5FF',
          200: '#99CBFF',
          300: '#66B0FF',
          400: '#3396FF',
          500: '#00539F',
          600: '#004380',
          700: '#003260',
          800: '#002240',
          900: '#001120',
        },
        secondary: {
          DEFAULT: '#0089FF',
          50: '#E6F5FF',
          100: '#CCEBFF',
          200: '#99D7FF',
          300: '#66C3FF',
          400: '#33AFFF',
          500: '#0089FF',
          600: '#006ECC',
          700: '#005299',
          800: '#003766',
          900: '#001B33',
        },
        accent: {
          DEFAULT: '#FF6B35',
          50: '#FFF2ED',
          100: '#FFE5DB',
          200: '#FFCBB7',
          300: '#FFB193',
          400: '#FF976F',
          500: '#FF6B35',
          600: '#FF4500',
          700: '#CC3700',
          800: '#992900',
          900: '#661C00',
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
  plugins: [],
}
```

### 3.3 Actualizar src/index.css

Reemplaza todo el contenido de `src/index.css` con:

```css
@tailwind base;
@tailwind components;
@tailwind utilities;

@layer base {
  * {
    @apply border-border;
  }
  
  body {
    @apply bg-white text-gray-900 antialiased;
    font-family: 'Inter', system-ui, -apple-system, sans-serif;
  }

  h1, h2, h3, h4, h5, h6 {
    font-family: 'Poppins', sans-serif;
    @apply font-semibold;
  }
}

@layer components {
  /* Button Base Styles */
  .btn {
    @apply inline-flex items-center justify-center rounded-lg px-4 py-2 text-sm font-medium transition-all duration-200 focus:outline-none focus:ring-2 focus:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed;
  }

  .btn-primary {
    @apply bg-primary text-white hover:bg-primary-600 focus:ring-primary-500;
  }

  .btn-secondary {
    @apply bg-secondary text-white hover:bg-secondary-600 focus:ring-secondary-500;
  }

  .btn-outline {
    @apply border border-gray-300 bg-white text-gray-700 hover:bg-gray-50 focus:ring-primary-500;
  }

  /* Input Base Styles */
  .input {
    @apply w-full rounded-lg border border-gray-300 px-4 py-2 text-sm transition-colors focus:border-primary-500 focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-0;
  }

  /* Card Styles */
  .card {
    @apply rounded-lg bg-white shadow-card transition-shadow hover:shadow-card-hover;
  }
}

@layer utilities {
  .text-balance {
    text-wrap: balance;
  }
}
```

---

## 🎯 PASO 4: Configurar Vite

### 4.1 Actualizar vite.config.ts

Reemplaza el contenido de `vite.config.ts`:

```typescript
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import path from 'path'
import { fileURLToPath } from 'url'

const __dirname = path.dirname(fileURLToPath(import.meta.url))

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
      '@components': path.resolve(__dirname, './src/components'),
      '@features': path.resolve(__dirname, './src/features'),
      '@hooks': path.resolve(__dirname, './src/hooks'),
      '@layouts': path.resolve(__dirname, './src/layouts'),
      '@pages': path.resolve(__dirname, './src/pages'),
      '@services': path.resolve(__dirname, './src/services'),
      '@store': path.resolve(__dirname, './src/store'),
      '@styles': path.resolve(__dirname, './src/styles'),
      '@types': path.resolve(__dirname, './src/types'),
      '@utils': path.resolve(__dirname, './src/utils'),
    },
  },
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: 'http://localhost:15095',
        changeOrigin: true,
      },
    },
  },
})
```

---

## 🎯 PASO 5: Crear Estructura de Carpetas

### 5.1 Crear carpetas principales

```powershell
# Asegúrate de estar en src/
cd src

# Crear estructura de carpetas
mkdir components, features, hooks, layouts, pages, services, store, types, utils

# Crear subcarpetas de components (Atomic Design)
cd components
mkdir atoms, molecules, organisms, templates
cd ..

# Crear features modules
cd features
mkdir auth, vehicles, user, admin, messages, search
cd ..

# Crear subcarpeta de services
cd services
mkdir endpoints
cd ..

# Volver a la raíz del frontend
cd ..
```

### 5.2 Estructura final

```
frontend/
├── src/
│   ├── components/
│   │   ├── atoms/
│   │   ├── molecules/
│   │   ├── organisms/
│   │   └── templates/
│   ├── features/
│   │   ├── auth/
│   │   ├── vehicles/
│   │   ├── user/
│   │   ├── admin/
│   │   ├── messages/
│   │   └── search/
│   ├── hooks/
│   ├── layouts/
│   ├── pages/
│   ├── services/
│   │   └── endpoints/
│   ├── store/
│   ├── types/
│   └── utils/
```

---

## 🎯 PASO 6: Configurar Environment Variables

### 6.1 Crear .env.example

```powershell
New-Item -ItemType File -Path .env.example
```

**Contenido de `.env.example`**:
```bash
# Environment Configuration
VITE_API_URL=http://localhost:15095
VITE_GATEWAY_URL=http://localhost:15095
VITE_CDN_URL=http://localhost:15095
VITE_ENVIRONMENT=development
VITE_ENABLE_ANALYTICS=false
VITE_ENABLE_ERROR_TRACKING=false

# Feature Flags
VITE_FEATURE_CHAT=true
VITE_FEATURE_FAVORITES=true
VITE_FEATURE_COMPARE=true

# Optional Services
VITE_SENTRY_DSN=
VITE_GA_TRACKING_ID=
```

### 6.2 Crear .env (desarrollo)

```powershell
Copy-Item .env.example .env
```

---

## 🎯 PASO 7: Crear Archivos Base del Proyecto

### 7.1 Crear src/types/index.ts

```powershell
New-Item -ItemType File -Path src/types/index.ts
```

**Contenido**: [Ver archivo completo en el proyecto]

### 7.2 Crear src/services/api.ts

```powershell
New-Item -ItemType File -Path src/services/api.ts
```

**Contenido básico**:
```typescript
import axios, { type AxiosError, type AxiosInstance, type InternalAxiosRequestConfig } from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:15095';

export const api: AxiosInstance = axios.create({
  baseURL: API_BASE_URL,
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor - Add auth token
api.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    const token = localStorage.getItem('accessToken');
    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error: AxiosError) => {
    return Promise.reject(error);
  }
);

export default api;
```

### 7.3 Crear src/store/authStore.ts

```powershell
New-Item -ItemType File -Path src/store/authStore.ts
```

### 7.4 Actualizar src/App.tsx

Reemplaza el contenido con:

```typescript
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import HomePage from './pages/HomePage';
import './index.css';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 5 * 60 * 1000,
      gcTime: 10 * 60 * 1000,
      retry: 1,
      refetchOnWindowFocus: false,
    },
  },
});

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <Router>
        <Routes>
          <Route path="/" element={<HomePage />} />
          <Route path="*" element={
            <div className="min-h-screen flex items-center justify-center">
              <div className="text-center">
                <h1 className="text-4xl font-bold text-gray-900 mb-4">404</h1>
                <p className="text-gray-600">Página no encontrada</p>
                <a href="/" className="btn btn-primary mt-4">Volver al inicio</a>
              </div>
            </div>
          } />
        </Routes>
      </Router>
    </QueryClientProvider>
  );
}

export default App;
```

### 7.5 Crear src/pages/HomePage.tsx

```powershell
New-Item -ItemType File -Path src/pages/HomePage.tsx
```

**Contenido**: Página de inicio con diseño responsive

---

## 🎯 PASO 8: Iniciar el Servidor de Desarrollo

### 8.1 Verificar que estás en el directorio correcto

```powershell
# IMPORTANTE: Debes estar en la carpeta frontend
cd c:\Users\gmoreno\source\repos\cardealer\frontend

# Verifica la ubicación actual
Get-Location
# Debe mostrar: C:\Users\gmoreno\source\repos\cardealer\frontend
```

### 8.2 Iniciar el servidor

```powershell
npm run dev
```

**Salida esperada**:
```
  VITE v7.2.6  ready in XXX ms

  ➜  Local:   http://localhost:5173/
  ➜  Network: use --host to expose
  ➜  press h + enter to show help
```

### 8.3 Abrir en el navegador

Abre tu navegador y ve a: **http://localhost:5173/**

Deberías ver la página de inicio con estilos de Tailwind CSS aplicados correctamente.

---

## ✅ VERIFICACIÓN FINAL

### Checklist de verificación

- [ ] ✅ Servidor corriendo en `http://localhost:5173/`
- [ ] ✅ No hay errores en la consola del navegador
- [ ] ✅ No hay errores en la terminal
- [ ] ✅ Los estilos de Tailwind CSS se aplican correctamente
- [ ] ✅ Los colores personalizados funcionan (primary, secondary, accent)
- [ ] ✅ Hot Module Replacement (HMR) funciona al editar archivos

### Comandos de verificación

```powershell
# Verificar que no hay errores de TypeScript
npm run lint

# Verificar build de producción
npm run build

# Preview del build
npm run preview
```

---

## ⚠️ SOLUCIÓN DE PROBLEMAS COMUNES

### Problema 1: Error de PostCSS "module is not defined"

**Error**:
```
[postcss] module is not defined in ES module scope
```

**Solución**:
```powershell
# Renombra los archivos de configuración a .cjs
Rename-Item -Path postcss.config.js -NewName postcss.config.cjs
Rename-Item -Path tailwind.config.js -NewName tailwind.config.cjs
```

### Problema 2: Error "tailwindcss directly as a PostCSS plugin"

**Error**:
```
It looks like you're trying to use `tailwindcss` directly as a PostCSS plugin
```

**Solución**:
```powershell
# Desinstala Tailwind v4+ e instala v3.4.1
npm uninstall tailwindcss
npm install -D tailwindcss@3.4.1 postcss@8.4.35 autoprefixer@10.4.18
```

### Problema 3: "Could not read package.json"

**Error**:
```
npm error enoent Could not read package.json
```

**Solución**:
```powershell
# Asegúrate de estar en el directorio correcto
cd c:\Users\gmoreno\source\repos\cardealer\frontend

# Verifica que existe package.json
Test-Path package.json  # Debe devolver True
```

### Problema 4: Import alias no funciona

**Error**:
```
Cannot find module '@/components/...'
```

**Solución**:
Verifica que `vite.config.ts` tenga los alias configurados correctamente (ver Paso 4.1)

---

## 🎉 ¡CONFIGURACIÓN COMPLETADA!

Si llegaste hasta aquí sin errores, ¡felicidades! 🎊

Tu proyecto frontend está completamente configurado y listo para el desarrollo.

### Próximos pasos

1. **Sprint 1**: Implementar autenticación (Login, Register)
2. **Sprint 2**: Crear Home page y navegación
3. **Sprint 3**: Desarrollar catálogo de vehículos
4. **Sprint 4+**: Continuar según el Sprint Plan

### Recursos adicionales

- 📋 [FRONTEND_SPRINT_PLAN.md](../FRONTEND_SPRINT_PLAN.md) - Plan de desarrollo completo
- 🎨 [FRONTEND_DESIGN_ANALYSIS.md](../FRONTEND_DESIGN_ANALYSIS.md) - Guías de diseño
- 🔌 [FRONTEND_API_CONTRACTS.md](../FRONTEND_API_CONTRACTS.md) - Documentación de API
- 📊 [FRONTEND_TECHNICAL_SPECS.md](../FRONTEND_TECHNICAL_SPECS.md) - Especificaciones técnicas

---

## 📝 Resumen de Comandos

```powershell
# 1. Crear proyecto
mkdir frontend && cd frontend
npm create vite@latest . -- --template react-ts

# 2. Instalar dependencias
npm install react-router-dom zustand @tanstack/react-query axios clsx
npm install -D tailwindcss@3.4.1 postcss@8.4.35 autoprefixer@10.4.18
npm install @headlessui/react framer-motion react-icons
npm install react-hook-form zod @hookform/resolvers

# 3. Crear archivos de configuración (.cjs)
New-Item -ItemType File -Path postcss.config.cjs
New-Item -ItemType File -Path tailwind.config.cjs

# 4. Crear estructura de carpetas
mkdir src/components, src/features, src/hooks, src/layouts, src/pages, src/services, src/store, src/types, src/utils

# 5. Iniciar servidor
npm run dev
```

---

**Autor**: GitHub Copilot  
**Fecha**: Diciembre 4, 2025  
**Versión**: 1.0.0  
**Tech Stack**: React 19.2 + Vite 7.2 + TypeScript 5.9 + Tailwind CSS 3.4.1
