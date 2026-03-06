# Sprint 7 Report — SEO Structured Data + UX Micro-fixes

**Fecha**: 6 marzo 2026
**Commit**: `4d943505`
**Build**: ✅ 213 páginas, 14.9s compilación

---

## Resumen Ejecutivo

Sprint enfocado en structured data (JSON-LD) para SEO y correcciones micro de UX. Cuatro tareas S-sized completadas sin regresiones.

---

## Auditoría Previa

Análisis de SEO structured data y WCAG compliance identificó 4 gaps:

1. **Falta JSON-LD en /vehiculos** — la página de listing más importante no tiene BreadcrumbList ni ItemList
2. **alert() nativo en /comparar** — inconsistente con el sistema de toast (sonner) del resto de la app
3. **Hero quick filter pills < 44px** — incumple WCAG 2.5.8 touch target mínimo
4. **Sin breadcrumbs en SERP** — Google no puede mostrar breadcrumbs para /vehiculos

---

## Tareas Completadas

### Task 38: ItemList JSON-LD en /vehiculos

**Archivo**: `src/lib/seo.tsx`

- Nueva función `generateItemListJsonLd()` que acepta array de items con name, url, image, price
- Schema.org ItemList con `numberOfItems` y `itemListElement`
- URLs relativas automáticamente prefijadas con SITE_URL

**Archivo**: `src/app/(main)/vehiculos/page.tsx`

- Convertido a `async` server component
- SSR fetch de primera página de vehículos via `searchVehicles({ page: 1, pageSize: 10 })`
- Try/catch para graceful degradation (API no disponible en build time → no JSON-LD, página sigue funcionando)
- Cada vehículo incluye: nombre (`{year} {make} {model}`), URL (`/vehiculos/{slug}`), imagen

### Task 39: Replace alert()→toast.success() en /comparar

**Archivo**: `src/app/(main)/comparar/page.tsx`

- Removido: `alert('Enlace copiado al portapapeles')` + `// TODO: Show toast notification`
- Agregado: `import { toast } from 'sonner'` + `toast.success('Enlace copiado al portapapeles')`
- Sonner ya está instalado (v2.0.7) y configurado globalmente

### Task 40: Hero Quick Filter Pills — WCAG 2.5.8 Touch Targets

**Archivo**: `src/components/homepage/hero-compact.tsx`

- `QuickFiltersHero` Link elements: `px-4 py-2` → `px-5 py-2.5` + `min-h-[44px]`
- Ahora cumple WCAG 2.5.8 mínimo de 44×44px touch target
- Estilo visual mejorado sin romper el diseño hero

### Task 41: BreadcrumbList JSON-LD en /vehiculos

**Archivo**: `src/app/(main)/vehiculos/page.tsx`

- BreadcrumbList estático: Inicio (/) → Vehículos en Venta (/vehiculos)
- Usa `generateBreadcrumbJsonLd` existente de seo.tsx
- Renderizado como `<script type="application/ld+json">` en head

---

## Impacto SEO

| Métrica                 | Antes         | Después                             |
| ----------------------- | ------------- | ----------------------------------- |
| JSON-LD en /vehiculos   | 0 schemas     | 2 (BreadcrumbList + ItemList)       |
| Google SERP breadcrumbs | No disponible | Inicio → Vehículos en Venta         |
| ItemList vehicles       | N/A           | Top 10 vehículos en structured data |
| alert() usage           | 1 instancia   | 0 (migrado a toast)                 |
| Touch targets < 44px    | Hero pills    | 0 violaciones                       |

---

## Archivos Modificados

| Archivo                                    | Cambio                           |
| ------------------------------------------ | -------------------------------- |
| `src/lib/seo.tsx`                          | +generateItemListJsonLd function |
| `src/app/(main)/vehiculos/page.tsx`        | async + JSON-LD + SSR fetch      |
| `src/app/(main)/comparar/page.tsx`         | alert→toast.success + import     |
| `src/components/homepage/hero-compact.tsx` | pill touch targets 44px          |

---

## Métricas del Sprint

- **Tareas**: 4/4 completadas (100%)
- **Archivos modificados**: 4
- **Líneas cambiadas**: +124 / -37
- **Build**: 213 páginas, 14.9s
- **Regresiones**: 0
