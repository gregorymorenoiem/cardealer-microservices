# 🚀 Plan de Sprints - Mejoras Frontend CarDealer

## 📊 Configuración del Modelo AI

| Parámetro | Valor |
|-----------|-------|
| **Modelo** | Claude Opus 4.5 |
| **Context Window (Input)** | 128,000 tokens |
| **Max Output** | 16,000 tokens |
| **Multiplier** | 1x |
| **Tokens Disponibles por Sesión** | ~110,000 tokens útiles (reservando 18k para sistema/instrucciones) |

---

## 📈 Metodología de Estimación de Tokens

### Fórmulas Utilizadas

```
Tokens de Lectura = (Líneas de código × 4) + (Archivos × 500)
Tokens de Escritura = (Líneas nuevas/modificadas × 5)
Tokens de Contexto = Instrucciones + Historial (~8,000 base)
Buffer de Seguridad = 15%

Total Estimado = (Lectura + Escritura + Contexto) × 1.15
```

### Factores de Complejidad

| Nivel | Multiplicador | Descripción |
|-------|--------------|-------------|
| Simple | 1.0x | Cambios menores, archivos pequeños |
| Medio | 1.3x | Múltiples archivos, lógica moderada |
| Complejo | 1.6x | Refactoring, nuevos patterns |
| Muy Complejo | 2.0x | Arquitectura, múltiples sistemas |

---

## 🎯 SPRINT 1: Corrección de Versiones y Dependencias
**Duración:** 1 día | **Prioridad:** 🔴 CRÍTICA

### Tarea 1.1: Auditar y Corregir package.json Principal

| Métrica | Valor |
|---------|-------|
| **Archivos a leer** | 1 (package.json) |
| **Líneas a leer** | ~70 |
| **Líneas a modificar** | ~15 |
| **Complejidad** | Simple (1.0x) |

**Estimación de Tokens:**
```
Lectura: 70 × 4 + 1 × 500 = 780 tokens
Escritura: 15 × 5 = 75 tokens
Contexto: 8,000 tokens
Total: (780 + 75 + 8,000) × 1.15 = 10,184 tokens
```

**Cambios Específicos:**
```json
// Correcciones necesarias:
"@tanstack/react-query": "^5.62.8",  // Era: ^5.90.12 (no existe)
"lucide-react": "^0.469.0",          // Era: ^0.556.0 (no existe)
"i18next": "^24.2.0",                // Era: ^25.7.1 (no existe)
"react-i18next": "^15.1.3",          // Era: ^16.4.0 (no existe)
"framer-motion": "^11.15.0"          // Era: ^12.23.25 (verificar)
```

| ✅ Cabe en 1 sesión | Tokens: ~10,200 |
|---------------------|-----------------|

---

### Tarea 1.2: Sincronizar package.json de OKLA

| Métrica | Valor |
|---------|-------|
| **Archivos a leer** | 2 (package.json principal + okla) |
| **Líneas a leer** | ~140 |
| **Líneas a modificar** | ~15 |
| **Complejidad** | Simple (1.0x) |

**Estimación de Tokens:**
```
Lectura: 140 × 4 + 2 × 500 = 1,560 tokens
Escritura: 15 × 5 = 75 tokens
Contexto: 8,000 tokens
Total: (1,560 + 75 + 8,000) × 1.15 = 11,081 tokens
```

| ✅ Cabe en 1 sesión | Tokens: ~11,100 |
|---------------------|-----------------|

---

### Tarea 1.3: Sincronizar package.json de CarDealer App

| Métrica | Valor |
|---------|-------|
| **Archivos a leer** | 2 (package.json principal + cardealer) |
| **Líneas a leer** | ~140 |
| **Líneas a modificar** | ~15 |
| **Complejidad** | Simple (1.0x) |

**Estimación de Tokens:**
```
Lectura: 140 × 4 + 2 × 500 = 1,560 tokens
Escritura: 15 × 5 = 75 tokens
Contexto: 8,000 tokens
Total: (1,560 + 75 + 8,000) × 1.15 = 11,081 tokens
```

| ✅ Cabe en 1 sesión | Tokens: ~11,100 |
|---------------------|-----------------|

---

### 📋 Resumen Sprint 1

| Tarea | Tokens Est. | Sesiones | Prioridad |
|-------|-------------|----------|-----------|
| 1.1 Corregir package.json principal | 10,200 | 1 | 🔴 |
| 1.2 Sincronizar OKLA package.json | 11,100 | 1 | 🔴 |
| 1.3 Sincronizar CarDealer package.json | 11,100 | 1 | 🔴 |
| **TOTAL SPRINT 1** | **32,400** | **3** | - |

**💡 Optimización:** Las 3 tareas pueden combinarse en 1-2 sesiones ya que comparten contexto.

---

## 🎯 SPRINT 2: Integración de Error Tracking (Sentry)
**Duración:** 2-3 días | **Prioridad:** 🔴 ALTA

### Tarea 2.1: Instalar y Configurar Sentry

| Métrica | Valor |
|---------|-------|
| **Archivos a crear** | 2 (sentry.ts, ErrorBoundary mejorado) |
| **Archivos a modificar** | 3 (main.tsx, App.tsx, vite.config.ts) |
| **Líneas a leer** | ~400 |
| **Líneas a escribir** | ~150 |
| **Complejidad** | Medio (1.3x) |

**Estimación de Tokens:**
```
Lectura: 400 × 4 + 5 × 500 = 4,100 tokens
Escritura: 150 × 5 = 750 tokens
Contexto: 8,000 tokens
Total: (4,100 + 750 + 8,000) × 1.15 × 1.3 = 19,217 tokens
```

**Código a Generar:**

```typescript
// src/lib/sentry.ts (~80 líneas)
import * as Sentry from '@sentry/react';

export const initSentry = () => {
  Sentry.init({
    dsn: import.meta.env.VITE_SENTRY_DSN,
    environment: import.meta.env.MODE,
    integrations: [
      Sentry.browserTracingIntegration(),
      Sentry.replayIntegration(),
    ],
    tracesSampleRate: import.meta.env.PROD ? 0.1 : 1.0,
    replaysSessionSampleRate: 0.1,
    replaysOnErrorSampleRate: 1.0,
  });
};
```

| ✅ Cabe en 1 sesión | Tokens: ~19,200 |
|---------------------|-----------------|

---

### Tarea 2.2: Integrar Sentry en Componentes Críticos

| Métrica | Valor |
|---------|-------|
| **Archivos a modificar** | 5 (api.ts, hooks, error boundaries) |
| **Líneas a leer** | ~600 |
| **Líneas a modificar** | ~100 |
| **Complejidad** | Medio (1.3x) |

**Estimación de Tokens:**
```
Lectura: 600 × 4 + 5 × 500 = 4,900 tokens
Escritura: 100 × 5 = 500 tokens
Contexto: 8,000 tokens
Total: (4,900 + 500 + 8,000) × 1.15 × 1.3 = 20,033 tokens
```

| ✅ Cabe en 1 sesión | Tokens: ~20,000 |
|---------------------|-----------------|

---

### Tarea 2.3: Configurar Source Maps y Release Tracking

| Métrica | Valor |
|---------|-------|
| **Archivos a modificar** | 2 (vite.config.ts, package.json scripts) |
| **Líneas a leer** | ~200 |
| **Líneas a modificar** | ~50 |
| **Complejidad** | Medio (1.3x) |

**Estimación de Tokens:**
```
Lectura: 200 × 4 + 2 × 500 = 1,800 tokens
Escritura: 50 × 5 = 250 tokens
Contexto: 8,000 tokens
Total: (1,800 + 250 + 8,000) × 1.15 × 1.3 = 15,029 tokens
```

| ✅ Cabe en 1 sesión | Tokens: ~15,000 |
|---------------------|-----------------|

---

### 📋 Resumen Sprint 2

| Tarea | Tokens Est. | Sesiones | Prioridad |
|-------|-------------|----------|-----------|
| 2.1 Configurar Sentry base | 19,200 | 1 | 🔴 |
| 2.2 Integrar en componentes | 20,000 | 1 | 🔴 |
| 2.3 Source maps y releases | 15,000 | 1 | 🟡 |
| **TOTAL SPRINT 2** | **54,200** | **3** | - |

---

## 🎯 SPRINT 3: Configuración de Testing Coverage
**Duración:** 1-2 días | **Prioridad:** 🟡 MEDIA

### Tarea 3.1: Configurar Vitest Coverage v8

| Métrica | Valor |
|---------|-------|
| **Archivos a modificar** | 3 (package.json, vitest.config.ts, scripts) |
| **Líneas a leer** | ~150 |
| **Líneas a modificar** | ~40 |
| **Complejidad** | Simple (1.0x) |

**Estimación de Tokens:**
```
Lectura: 150 × 4 + 3 × 500 = 2,100 tokens
Escritura: 40 × 5 = 200 tokens
Contexto: 8,000 tokens
Total: (2,100 + 200 + 8,000) × 1.15 = 11,845 tokens
```

**Configuración a Agregar:**

```typescript
// vitest.config.ts
export default defineConfig({
  test: {
    coverage: {
      provider: 'v8',
      reporter: ['text', 'html', 'lcov'],
      exclude: [
        'node_modules/',
        'src/test/',
        '**/*.d.ts',
        'src/mocks/**',
      ],
      thresholds: {
        global: {
          branches: 70,
          functions: 70,
          lines: 70,
          statements: 70,
        },
      },
    },
  },
});
```

| ✅ Cabe en 1 sesión | Tokens: ~11,900 |
|---------------------|-----------------|

---

### Tarea 3.2: Agregar MSW para API Mocking

| Métrica | Valor |
|---------|-------|
| **Archivos a crear** | 4 (handlers.ts, browser.ts, server.ts, mocks/index.ts) |
| **Archivos a modificar** | 2 (test/setup.ts, package.json) |
| **Líneas a leer** | ~100 |
| **Líneas a escribir** | ~200 |
| **Complejidad** | Medio (1.3x) |

**Estimación de Tokens:**
```
Lectura: 100 × 4 + 6 × 500 = 3,400 tokens
Escritura: 200 × 5 = 1,000 tokens
Contexto: 8,000 tokens
Total: (3,400 + 1,000 + 8,000) × 1.15 × 1.3 = 18,551 tokens
```

| ✅ Cabe en 1 sesión | Tokens: ~18,600 |
|---------------------|-----------------|

---

### 📋 Resumen Sprint 3

| Tarea | Tokens Est. | Sesiones | Prioridad |
|-------|-------------|----------|-----------|
| 3.1 Configurar coverage | 11,900 | 1 | 🟡 |
| 3.2 Agregar MSW | 18,600 | 1 | 🟡 |
| **TOTAL SPRINT 3** | **30,500** | **2** | - |

---

## 🎯 SPRINT 4: Implementar Storybook
**Duración:** 3-4 días | **Prioridad:** 🟡 MEDIA

### Tarea 4.1: Inicializar Storybook con Vite

| Métrica | Valor |
|---------|-------|
| **Archivos a crear** | 4 (.storybook/main.ts, preview.ts, etc.) |
| **Archivos a modificar** | 1 (package.json) |
| **Líneas a leer** | ~50 |
| **Líneas a escribir** | ~120 |
| **Complejidad** | Medio (1.3x) |

**Estimación de Tokens:**
```
Lectura: 50 × 4 + 5 × 500 = 2,700 tokens
Escritura: 120 × 5 = 600 tokens
Contexto: 8,000 tokens
Total: (2,700 + 600 + 8,000) × 1.15 × 1.3 = 16,900 tokens
```

| ✅ Cabe en 1 sesión | Tokens: ~16,900 |
|---------------------|-----------------|

---

### Tarea 4.2: Crear Stories para Atoms

| Métrica | Valor |
|---------|-------|
| **Archivos a leer** | 7 (Button, Input, Label, Spinner, etc.) |
| **Archivos a crear** | 7 (*.stories.tsx) |
| **Líneas a leer** | ~400 |
| **Líneas a escribir** | ~350 |
| **Complejidad** | Medio (1.3x) |

**Estimación de Tokens:**
```
Lectura: 400 × 4 + 7 × 500 = 5,100 tokens
Escritura: 350 × 5 = 1,750 tokens
Contexto: 8,000 tokens
Total: (5,100 + 1,750 + 8,000) × 1.15 × 1.3 = 22,226 tokens
```

| ✅ Cabe en 1 sesión | Tokens: ~22,200 |
|---------------------|-----------------|

---

### Tarea 4.3: Crear Stories para Molecules

| Métrica | Valor |
|---------|-------|
| **Archivos a leer** | 10 (componentes molecules) |
| **Archivos a crear** | 10 (*.stories.tsx) |
| **Líneas a leer** | ~800 |
| **Líneas a escribir** | ~500 |
| **Complejidad** | Medio (1.3x) |

**Estimación de Tokens:**
```
Lectura: 800 × 4 + 10 × 500 = 8,200 tokens
Escritura: 500 × 5 = 2,500 tokens
Contexto: 8,000 tokens
Total: (8,200 + 2,500 + 8,000) × 1.15 × 1.3 = 27,965 tokens
```

| ✅ Cabe en 1 sesión | Tokens: ~28,000 |
|---------------------|-----------------|

---

### Tarea 4.4: Crear Stories para Organisms (Parte 1)

| Métrica | Valor |
|---------|-------|
| **Archivos a leer** | 8 (Navbar, Footer, etc.) |
| **Archivos a crear** | 8 (*.stories.tsx) |
| **Líneas a leer** | ~1,200 |
| **Líneas a escribir** | ~600 |
| **Complejidad** | Complejo (1.6x) |

**Estimación de Tokens:**
```
Lectura: 1,200 × 4 + 8 × 500 = 8,800 tokens
Escritura: 600 × 5 = 3,000 tokens
Contexto: 8,000 tokens
Total: (8,800 + 3,000 + 8,000) × 1.15 × 1.6 = 36,432 tokens
```

| ✅ Cabe en 1 sesión | Tokens: ~36,400 |
|---------------------|-----------------|

---

### Tarea 4.5: Crear Stories para Organisms (Parte 2)

| Métrica | Valor |
|---------|-------|
| **Archivos a leer** | 8 (VehicleCard, FilterSidebar, etc.) |
| **Archivos a crear** | 8 (*.stories.tsx) |
| **Líneas a leer** | ~1,200 |
| **Líneas a escribir** | ~600 |
| **Complejidad** | Complejo (1.6x) |

**Estimación de Tokens:**
```
Lectura: 1,200 × 4 + 8 × 500 = 8,800 tokens
Escritura: 600 × 5 = 3,000 tokens
Contexto: 8,000 tokens
Total: (8,800 + 3,000 + 8,000) × 1.15 × 1.6 = 36,432 tokens
```

| ✅ Cabe en 1 sesión | Tokens: ~36,400 |
|---------------------|-----------------|

---

### 📋 Resumen Sprint 4

| Tarea | Tokens Est. | Sesiones | Prioridad |
|-------|-------------|----------|-----------|
| 4.1 Inicializar Storybook | 16,900 | 1 | 🟡 |
| 4.2 Stories Atoms | 22,200 | 1 | 🟡 |
| 4.3 Stories Molecules | 28,000 | 1 | 🟡 |
| 4.4 Stories Organisms P1 | 36,400 | 1 | 🟡 |
| 4.5 Stories Organisms P2 | 36,400 | 1 | 🟡 |
| **TOTAL SPRINT 4** | **139,900** | **5** | - |

---

## 🎯 SPRINT 5: Configurar Monorepo con Workspaces
**Duración:** 2-3 días | **Prioridad:** 🟡 MEDIA

### Tarea 5.1: Configurar npm Workspaces

| Métrica | Valor |
|---------|-------|
| **Archivos a crear** | 1 (package.json raíz workspace) |
| **Archivos a modificar** | 4 (package.json de cada app) |
| **Líneas a leer** | ~300 |
| **Líneas a modificar** | ~100 |
| **Complejidad** | Complejo (1.6x) |

**Estimación de Tokens:**
```
Lectura: 300 × 4 + 5 × 500 = 3,700 tokens
Escritura: 100 × 5 = 500 tokens
Contexto: 8,000 tokens
Total: (3,700 + 500 + 8,000) × 1.15 × 1.6 = 22,448 tokens
```

**Estructura Propuesta:**
```json
// frontend/package.json (nuevo)
{
  "name": "cardealer-frontend",
  "private": true,
  "workspaces": [
    "web",
    "web/okla",
    "web/cardealer",
    "shared"
  ],
  "scripts": {
    "dev": "npm run dev --workspace=web",
    "dev:okla": "npm run dev --workspace=web/okla",
    "build:all": "npm run build --workspaces",
    "test:all": "npm run test --workspaces"
  }
}
```

| ✅ Cabe en 1 sesión | Tokens: ~22,400 |
|---------------------|-----------------|

---

### Tarea 5.2: Extraer Dependencias Compartidas

| Métrica | Valor |
|---------|-------|
| **Archivos a modificar** | 4 (package.json de cada app) |
| **Líneas a leer** | ~280 |
| **Líneas a modificar** | ~150 |
| **Complejidad** | Medio (1.3x) |

**Estimación de Tokens:**
```
Lectura: 280 × 4 + 4 × 500 = 3,120 tokens
Escritura: 150 × 5 = 750 tokens
Contexto: 8,000 tokens
Total: (3,120 + 750 + 8,000) × 1.15 × 1.3 = 17,751 tokens
```

| ✅ Cabe en 1 sesión | Tokens: ~17,800 |
|---------------------|-----------------|

---

### Tarea 5.3: Configurar Shared Package

| Métrica | Valor |
|---------|-------|
| **Archivos a crear** | 3 (package.json, tsconfig.json, index.ts) |
| **Archivos a modificar** | 2 (imports en apps) |
| **Líneas a leer** | ~100 |
| **Líneas a escribir** | ~80 |
| **Complejidad** | Medio (1.3x) |

**Estimación de Tokens:**
```
Lectura: 100 × 4 + 5 × 500 = 2,900 tokens
Escritura: 80 × 5 = 400 tokens
Contexto: 8,000 tokens
Total: (2,900 + 400 + 8,000) × 1.15 × 1.3 = 16,894 tokens
```

| ✅ Cabe en 1 sesión | Tokens: ~16,900 |
|---------------------|-----------------|

---

### 📋 Resumen Sprint 5

| Tarea | Tokens Est. | Sesiones | Prioridad |
|-------|-------------|----------|-----------|
| 5.1 Configurar workspaces | 22,400 | 1 | 🟡 |
| 5.2 Extraer dependencias | 17,800 | 1 | 🟡 |
| 5.3 Configurar shared pkg | 16,900 | 1 | 🟡 |
| **TOTAL SPRINT 5** | **57,100** | **3** | - |

---

## 🎯 SPRINT 6: Pre-commit Hooks y Code Quality
**Duración:** 1 día | **Prioridad:** 🟢 BAJA

### Tarea 6.1: Configurar Husky + lint-staged

| Métrica | Valor |
|---------|-------|
| **Archivos a crear** | 3 (.husky/pre-commit, lint-staged.config.js) |
| **Archivos a modificar** | 1 (package.json) |
| **Líneas a leer** | ~70 |
| **Líneas a escribir** | ~50 |
| **Complejidad** | Simple (1.0x) |

**Estimación de Tokens:**
```
Lectura: 70 × 4 + 4 × 500 = 2,280 tokens
Escritura: 50 × 5 = 250 tokens
Contexto: 8,000 tokens
Total: (2,280 + 250 + 8,000) × 1.15 = 12,110 tokens
```

| ✅ Cabe en 1 sesión | Tokens: ~12,100 |
|---------------------|-----------------|

---

### Tarea 6.2: Configurar Prettier

| Métrica | Valor |
|---------|-------|
| **Archivos a crear** | 2 (.prettierrc, .prettierignore) |
| **Archivos a modificar** | 2 (package.json, eslint.config.js) |
| **Líneas a leer** | ~100 |
| **Líneas a escribir** | ~40 |
| **Complejidad** | Simple (1.0x) |

**Estimación de Tokens:**
```
Lectura: 100 × 4 + 4 × 500 = 2,400 tokens
Escritura: 40 × 5 = 200 tokens
Contexto: 8,000 tokens
Total: (2,400 + 200 + 8,000) × 1.15 = 12,190 tokens
```

| ✅ Cabe en 1 sesión | Tokens: ~12,200 |
|---------------------|-----------------|

---

### 📋 Resumen Sprint 6

| Tarea | Tokens Est. | Sesiones | Prioridad |
|-------|-------------|----------|-----------|
| 6.1 Husky + lint-staged | 12,100 | 1 | 🟢 |
| 6.2 Prettier | 12,200 | 1 | 🟢 |
| **TOTAL SPRINT 6** | **24,300** | **2** | - |

---

## 🎯 SPRINT 7: SEO y Web Vitals
**Duración:** 2 días | **Prioridad:** 🟡 MEDIA

### Tarea 7.1: Integrar React Helmet Async

| Métrica | Valor |
|---------|-------|
| **Archivos a crear** | 2 (SEOHead.tsx, useSEO.ts) |
| **Archivos a modificar** | 5 (páginas principales) |
| **Líneas a leer** | ~400 |
| **Líneas a escribir** | ~200 |
| **Complejidad** | Medio (1.3x) |

**Estimación de Tokens:**
```
Lectura: 400 × 4 + 7 × 500 = 5,100 tokens
Escritura: 200 × 5 = 1,000 tokens
Contexto: 8,000 tokens
Total: (5,100 + 1,000 + 8,000) × 1.15 × 1.3 = 21,099 tokens
```

| ✅ Cabe en 1 sesión | Tokens: ~21,100 |
|---------------------|-----------------|

---

### Tarea 7.2: Integrar Web Vitals Reporting

| Métrica | Valor |
|---------|-------|
| **Archivos a crear** | 1 (webVitals.ts) |
| **Archivos a modificar** | 2 (main.tsx, package.json) |
| **Líneas a leer** | ~100 |
| **Líneas a escribir** | ~80 |
| **Complejidad** | Simple (1.0x) |

**Estimación de Tokens:**
```
Lectura: 100 × 4 + 3 × 500 = 1,900 tokens
Escritura: 80 × 5 = 400 tokens
Contexto: 8,000 tokens
Total: (1,900 + 400 + 8,000) × 1.15 = 11,845 tokens
```

**Código a Generar:**
```typescript
// src/lib/webVitals.ts
import { onCLS, onFID, onFCP, onLCP, onTTFB } from 'web-vitals';

export function reportWebVitals() {
  onCLS(console.log);
  onFID(console.log);
  onFCP(console.log);
  onLCP(console.log);
  onTTFB(console.log);
}
```

| ✅ Cabe en 1 sesión | Tokens: ~11,800 |
|---------------------|-----------------|

---

### 📋 Resumen Sprint 7

| Tarea | Tokens Est. | Sesiones | Prioridad |
|-------|-------------|----------|-----------|
| 7.1 React Helmet Async | 21,100 | 1 | 🟡 |
| 7.2 Web Vitals | 11,800 | 1 | 🟡 |
| **TOTAL SPRINT 7** | **32,900** | **2** | - |

---

## 📊 RESUMEN GLOBAL DEL PLAN

### Vista General por Sprint

| Sprint | Nombre | Tokens Total | Sesiones | Días | Prioridad |
|--------|--------|--------------|----------|------|-----------|
| 1 | Corrección de Versiones | 32,400 | 3 | 1 | 🔴 Crítica |
| 2 | Sentry Integration | 54,200 | 3 | 2-3 | 🔴 Alta |
| 3 | Testing Coverage | 30,500 | 2 | 1-2 | 🟡 Media |
| 4 | Storybook | 139,900 | 5 | 3-4 | 🟡 Media |
| 5 | Monorepo Workspaces | 57,100 | 3 | 2-3 | 🟡 Media |
| 6 | Pre-commit Hooks | 24,300 | 2 | 1 | 🟢 Baja |
| 7 | SEO y Web Vitals | 32,900 | 2 | 2 | 🟡 Media |
| **TOTAL** | - | **371,300** | **20** | **12-16** | - |

---

### 📈 Distribución de Tokens por Prioridad

```
🔴 CRÍTICA/ALTA:  86,600 tokens  (23.3%)  → Sprints 1-2
🟡 MEDIA:        260,400 tokens  (70.2%)  → Sprints 3-5, 7
🟢 BAJA:          24,300 tokens  (6.5%)   → Sprint 6
```

---

### ⚡ Optimizaciones Posibles

| Optimización | Ahorro Estimado | Descripción |
|--------------|-----------------|-------------|
| Combinar tareas 1.1-1.3 | ~15,000 tokens | Misma sesión con contexto compartido |
| Batch Stories por tipo | ~20,000 tokens | Menos cambios de contexto |
| Usar templates | ~10,000 tokens | Reutilizar patrones de código |
| **TOTAL AHORRO** | **~45,000 tokens** | 12% de reducción |

---

### 📅 Cronograma Recomendado

```
Semana 1: Sprint 1 + Sprint 2 (Críticos)
         ├── Día 1: Corrección versiones (3 tareas)
         ├── Día 2-3: Sentry setup
         └── Día 4: Sentry integración

Semana 2: Sprint 3 + Sprint 6
         ├── Día 1: Coverage setup
         ├── Día 2: MSW mocking
         └── Día 3: Husky + Prettier

Semana 3-4: Sprint 4 (Storybook)
         ├── Día 1: Setup inicial
         ├── Día 2: Atoms stories
         ├── Día 3: Molecules stories
         └── Día 4-5: Organisms stories

Semana 5: Sprint 5 + Sprint 7
         ├── Día 1-2: Monorepo setup
         ├── Día 3: Shared package
         └── Día 4: SEO + Web Vitals
```

---

### 💰 Costo Estimado en Tokens

**Con Claude Opus 4.5 (1x multiplier):**

| Métrica | Valor |
|---------|-------|
| Tokens de Input totales | ~250,000 |
| Tokens de Output totales | ~121,000 |
| Sesiones totales | 20 |
| Tokens promedio/sesión | ~18,500 |

---

### ✅ Checklist de Validación Pre-Sprint

- [ ] Verificar versiones actuales en npm antes de cada tarea
- [ ] Hacer backup de package.json antes de cambios
- [ ] Ejecutar `npm install` después de cada cambio
- [ ] Ejecutar tests después de cada sprint
- [ ] Verificar build de producción al final de cada sprint

---

*Documento generado el 29 de Diciembre 2025*
*Basado en análisis de frontend CarDealer Microservices*
