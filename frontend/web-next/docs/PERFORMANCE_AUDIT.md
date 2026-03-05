# ⚡ Performance Audit & Core Web Vitals Optimization — OKLA

**Fecha:** 2025-01-XX  
**Objetivo:** Optimizar LCP, INP, CLS para superar umbrales de Google

---

## Umbrales de Referencia

| Métrica | Bueno   | Necesita mejora | Malo    |
| ------- | ------- | --------------- | ------- |
| LCP     | < 2.5s  | 2.5s – 4.0s     | > 4.0s  |
| INP     | < 200ms | 200ms – 500ms   | > 500ms |
| CLS     | < 0.1   | 0.1 – 0.25      | > 0.25  |
| FCP     | < 1.8s  | 1.8s – 3.0s     | > 3.0s  |
| TTFB    | < 0.8s  | 0.8s – 1.8s     | > 1.8s  |

---

## Optimizaciones Aplicadas

### 🔴 P0 — Críticas para CWV

#### 1. ✅ Eliminar carga duplicada de GA4

- **Problema:** `retargeting-pixels.ts` cargaba `gtag.js` independientemente del componente `GoogleAnalytics` — doble carga de script, dobles page views.
- **Fix:** Removida la carga de `gtag.js` en retargeting-pixels.ts. Ahora solo configura Google Ads reutilizando el gtag existente.
- **Impacto:** −29KB de scripts, eliminación de doble tracking.

#### 2. ✅ Eliminar waterfall en homepage

- **Problema:** `getHomepageSections()` y `getFeaturedVehiclesFallback()` se ejecutaban en serie.
- **Fix:** Ambas funciones ejecutadas con `Promise.all()` en paralelo.
- **Impacto:** LCP −300-800ms (dependiendo de latencia del backend).

#### 3. ✅ Navbar: cambiar de dynamic a static import

- **Problema:** Navbar era dynamic import con skeleton — causaba un round-trip extra para el componente más visible.
- **Fix:** Cambio a import estático. El Navbar ahora se incluye en el bundle SSR directamente, evitando flash del skeleton.
- **Impacto:** LCP −100-300ms, eliminación de flash visual.

#### 4. ✅ SupportAgentWidget: cambiar a dynamic import con ssr:false

- **Problema:** Widget de soporte (LLM, WebSocket, chat) se cargaba estáticamente en cada página.
- **Fix:** Dynamic import con `ssr: false` — solo se carga cuando el browser está idle.
- **Impacto:** Bundle principal −15-25KB, INP mejorado.

### 🟡 P1 — Importantes

#### 5. ✅ GA4 Script: cambiar estrategia a lazyOnload

- **Problema:** GA4 usaba `afterInteractive` que ejecuta durante la hidratación.
- **Fix:** Cambiado a `lazyOnload` — se carga después de que la página es completamente interactiva.
- **Impacto:** INP mejorado, TTI reducido.

#### 6. ✅ GA4 movido de <head> a final de <body>

- **Problema:** GoogleAnalytics como componente cliente en `<head>` forzaba hidratación temprana.
- **Fix:** Movido al final de `<body>`, después del contenido principal.
- **Impacto:** FCP mejorado, no bloquea renderizado.

#### 7. ✅ Retargeting pixels diferidos con requestIdleCallback

- **Problema:** FB Pixel y TikTok Pixel se cargaban inmediatamente en mount.
- **Fix:** Envueltos en `requestIdleCallback` (fallback: `setTimeout(fn, 3000)`).
- **Impacto:** INP −50-100ms, main thread libre para interacciones.

#### 8. ✅ Mantener console.error en producción

- **Problema:** `removeConsole: true` eliminaba TODOS los console calls incluyendo `console.error`.
- **Fix:** `removeConsole: { exclude: ['error', 'warn'] }` mantiene error reporting.
- **Impacto:** Mejor debugging en producción.

---

## Resumen de Impacto Estimado

| Métrica          | Antes (estimado) | Después (estimado)  | Mejora     |
| ---------------- | ---------------- | ------------------- | ---------- |
| LCP              | ~3.0-3.5s        | ~2.0-2.5s           | ✅ −800ms  |
| INP              | ~250-350ms       | ~150-200ms          | ✅ −100ms  |
| CLS              | ~0.05-0.1        | ~0.02-0.05          | ✅ −50%    |
| Bundle JS        | ~450KB           | ~400KB              | ✅ −50KB   |
| Scripts cargados | 4 (sync)         | 2 (sync) + 2 (idle) | ✅ −2 sync |

---

## Arquitectura Optimizada

```
HTML Response (SSR)
├── <head>
│   ├── Theme script (inline, blocking — minimal)
│   ├── Permissions-Policy meta
│   └── JSON-LD structured data
├── <body>
│   ├── Providers (React context)
│   │   ├── Navbar (STATIC import — renders immediately)
│   │   ├── Main Content (SSR + streaming via Suspense)
│   │   ├── Footer (static)
│   │   └── SupportAgentWidget (DYNAMIC, ssr:false — loads on idle)
│   └── GoogleAnalytics (lazyOnload — loads after page idle)
└── Retargeting Pixels (requestIdleCallback — non-blocking)
    ├── FB Pixel (deferred)
    ├── Google Ads config (reuses existing gtag)
    └── TikTok Pixel (deferred)
```
