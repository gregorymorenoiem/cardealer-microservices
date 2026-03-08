# 🎨 OKLA — Auditoría UX, Conversión y Accesibilidad del Frontend

**Fecha:** 2026-03-06
**Autor:** CPSO (Copilot)
**Scope:** Next.js 16 frontend — 130+ rutas, 45 servicios, 27 feature components

---

## 📊 Resumen Ejecutivo

El frontend de OKLA es **técnicamente sólido** (Next.js 16, SSR/ISR, JSON-LD, shadcn/ui) pero tiene **brechas críticas en conversión y accesibilidad**:

- 🔴 La página de búsqueda (conversion page #1) es 100% client-rendered → lenta en mobile RD
- 🔴 No hay skip-to-content ni `<main>` landmark → WCAG Level A failure
- 🟠 No hay flujo guest-to-contact para compradores no autenticados
- 🟠 No hay progress indicator para sellers en proceso de onboarding
- 🟡 No hay landing page dedicada para dealers (usuarios de mayor revenue)

---

## 1. Mapa de Rutas (130+ URLs)

### Buyer-facing

| Grupo            | Rutas                         | Estado                |
| ---------------- | ----------------------------- | --------------------- |
| Homepage         | `/`                           | ✅ SSR, hero search   |
| Búsqueda         | `/buscar`, `/vehiculos`       | ⚠️ `/buscar` CSR only |
| Detalle Vehículo | `/vehiculos/[slug]`           | ✅ ISR 300s, JSON-LD  |
| Comparar         | `/comparar`                   | ⚠️ CSR only           |
| Marcas           | `/marcas/[marca]`             | ✅ SSR                |
| Dealers          | `/dealers`, `/dealers/[slug]` | ✅ SSR + Dynamic meta |

### Seller-facing

| Grupo          | Rutas                           | Estado             |
| -------------- | ------------------------------- | ------------------ |
| Vender Landing | `/vender`                       | ✅ SSR             |
| Publicar       | `/publicar`, `/vender/publicar` | ⚠️ CSR (KYC-gated) |
| Dashboard      | `/cuenta/mis-vehiculos`         | ⚠️ CSR             |

### Dealer-facing

| Grupo           | Rutas (15)                                | Estado |
| --------------- | ----------------------------------------- | ------ |
| Dashboard       | `/cuenta` (role-aware)                    | ✅     |
| Inventario      | `/dealer/inventario`                      | ⚠️ CSR |
| Leads/Analytics | `/dealer/leads`, `/dealer/analytics`      | ⚠️ CSR |
| Perfil/Config   | `/dealer/perfil`, `/dealer/configuracion` | ⚠️ CSR |

### Admin (35 rutas)

Comprehensive admin panel: usuarios, vehículos, KYC, planes, facturación, soporte, compliance, etc.

### Auth (9 rutas)

Login, registro, recuperar contraseña, callback OAuth, verificar email, crear contraseña.

### Estáticas (15 rutas)

FAQ, contacto, nosotros, blog, guías, términos, privacidad, empleos, prensa, etc.

---

## 2. Matriz de Calidad de Páginas

| Página              | SSR/CSR         | Metadata           | Loading     | Error     | A11y                   |
| ------------------- | --------------- | ------------------ | ----------- | --------- | ---------------------- |
| `/` (Home)          | ✅ SSR→Client   | ✅ Full            | ✅ Global   | ✅ Global | ⚠️                     |
| `/vehiculos`        | ✅ SSR+ISR      | ✅ OG/Twitter      | ✅ Skeleton | ✅ Global | ⚠️                     |
| `/vehiculos/[slug]` | ✅ SSR+ISR 300s | ✅ Dynamic+JSON-LD | ✅ Skeleton | ✅ Custom | ✅ Good                |
| **`/buscar`**       | **❌ CSR only** | ✅ Layout          | ✅ Skeleton | **❌**    | **⚠️ No live regions** |
| `/login`            | ❌ CSR          | ⚠️ noindex         | ❌          | ❌        | ⚠️ No aria-live        |
| `/registro`         | ❌ CSR          | ⚠️ noindex         | ❌          | ❌        | ✅ role=alert          |
| `/publicar`         | ❌ CSR          | ❌                 | ✅          | ❌        | ⚠️                     |
| `/checkout`         | ❌ CSR          | ✅ Layout          | ✅          | ✅        | ⚠️                     |
| `/precios`          | ✅ SSR          | ✅ Static          | ❌          | ❌        | ⚠️                     |
| `/vender`           | ✅ SSR          | ✅ Full OG         | ✅          | ✅        | ⚠️                     |
| `/admin/*`          | ❌ CSR          | ✅ Layout          | ✅          | ❌        | ⚠️                     |

**Estadísticas:** 22 loading.tsx, 8 error.tsx, 42 metadata exports, **0 skip-to-content**

---

## 3. Análisis de Funnel de Conversión

### 🛒 Buyer Funnel

```
Homepage (/) → Search (/buscar) → Detail (/vehiculos/[slug]) → Contact → Message
    ✅ Fast         ⚠️ SLOW CSR       ✅ Fast ISR          🔴 LOGIN WALL    ⚠️ Context switch
```

**Gap 1: `/buscar` es CSR** — Usuarios mobile en RD (10-15 Mbps promedio) ven página en blanco mientras carga JavaScript. Esta es la página de conversión #1 y no tiene SSR.

**Gap 2: Login wall para contactar** — No hay modal inline de "registro rápido" cuando un guest quiere contactar. Debe navegar a `/login` → pierde contexto del vehículo.

**Impacto estimado:** 15-25% de bounce rate adicional en `/buscar` por lentitud. 30-40% de abandono en el paso de contacto por fricción de login.

### 🏷️ Seller Funnel

```
Landing (/vender) → Register → KYC (/cuenta/verificacion) → Publish (/publicar) → Manage
    ✅                 ⚠️ Subtle       🔴 1199 lines, no persist  ✅ Smart wizard    ✅
```

**Gap 3: KYC es un monstruo** — 1,199 líneas en un solo Client Component. Si el usuario cierra el browser mid-verificación, pierde progreso (solo hook de localStorage).

**Gap 4: Sin progress indicator** — El seller no sabe cuántos pasos le faltan: Registrar → Verificar → Publicar.

### 🏢 Dealer Funnel

```
Pricing (/precios) → Register → Subscribe (/checkout) → Setup (/dealer/perfil) → Dashboard
    ✅                 🔴 No CTA dedicado  ✅                  ⚠️ Only logged-in    ✅
```

**Gap 5: Sin landing page dedicada para dealers** — Los dealers son los usuarios de mayor revenue ($49-$299/mes) pero no tienen una página de aterrizaje optimizada para conversión.

---

## 4. Hallazgos de Accesibilidad

### 🔴 Críticos (WCAG Level A)

1. **No skip-to-content link** — 0 instancias encontradas. Los usuarios de screen reader no pueden saltar la navegación. **Violación WCAG 2.4.1**.

2. **Sin landmark `<main>`** — El layout usa `<header>` y `<footer>` pero no envuelve el contenido en `<main>`. Screen readers no pueden identificar la región de contenido principal.

3. **Login sin aria-live para errores** — Los errores de login se muestran visualmente pero no se anuncian a screen readers. El registro SÍ usa `role="alert"` (inconsistencia).

4. **Búsqueda sin live region para resultados** — Cuando cambian los resultados, no hay `aria-live` que anuncie "15 resultados encontrados".

### 🟡 Moderados

5. **Loading skeletons sin anuncio** — 22 archivos loading.tsx usan skeletons visuales pero solo el root incluye `role="status"` + `aria-live`.

6. **Contraste de color potencial** — Clases como `text-primary/80` reducen opacidad. Green (#00A870) con texto blanco al 80% podría fallar ratio 4.5:1.

7. **Solo 12 instancias de `onKeyDown`** — Elementos interactivos complejos (galería, filtros, comparador) pueden no ser navegables por teclado.

8. **Sin gestión de focus en cambios de ruta** — Después de navegar, el focus no se mueve al contenido principal.

### ✅ Positivos

- Vehicle cards: buen `alt` en imágenes, `aria-label` en favoritos
- Star ratings: `role="group"`, `aria-label`, keyboard handlers
- Cookie consent: full ARIA con `role="dialog"`
- VIN input: `aria-busy`, `aria-live` para estado de decode
- shadcn/ui components: accesibilidad built-in desde Radix primitives

---

## 5. Top 5 Mejoras UX (Priorizadas)

### 1. 🚀 SSR para `/buscar` (Search) — Impacto: MUY ALTO

**Problema:** Página #1 de conversión es 100% CSR. Usuarios mobile ven blank page.
**Solución:** Refactorizar a SSR con client islands para filtros/interactividad. Similar a como `/vehiculos` ya funciona.
**Revenue impact:** Reducción estimada de 15-25% en bounce rate = más leads para sellers/dealers.

### 2. 🔐 Guest-to-Contact Flow — Impacto: ALTO

**Problema:** No hay conversión inline guest→registered cuando quieren contactar seller.
**Solución:** AuthPromptDialog con quick register preservando contexto del vehículo en callback URL.
**Revenue impact:** Estimado 20-30% más contactos → más conversiones.

### 3. ♿ Skip-to-Content + `<main>` Landmark — Impacto: ALTO (Legal/Compliance)

**Problema:** WCAG Level A failures. Riesgo legal y 15% de usuarios con discapacidades excluidos.
**Solución:** Skip link en root layout, envolver content en `<main>`.
**Esfuerzo:** 2 horas.

### 4. 📊 Seller Onboarding Progress — Impacto: MEDIO

**Problema:** Sellers abandonan KYC porque no ven progreso.
**Solución:** Progress bar persistente en `/cuenta` layout mostrando Registrar → Verificar → Publicar.
**Revenue impact:** Reducción de 15-20% en abandono de KYC.

### 5. 🏪 Dealer Landing Page — Impacto: MEDIO

**Problema:** Dealers (mayor revenue) no tienen landing page optimizada con CTA claro.
**Solución:** Crear `/para-dealers` con pricing, testimonials, "Empezar prueba gratis".
**Revenue impact:** Estimado 10-15% más dealer signups.

---

## 6. Integración con Sprints Futuros

### Sprint 19 (CPSO)

- 19.1: Crear skip-to-content + `<main>` landmark (2h)
- 19.2: Agregar `aria-live` a búsqueda y login
- 19.3: Diseñar dealer landing page

### Sprint 20 (Developer)

- 20.1: Refactorizar `/buscar` a SSR + client islands
- 20.2: Implementar AuthPromptDialog guest-to-contact
- 20.3: Agregar seller onboarding progress indicator

### Sprint 21 (CPSO + Developer)

- 21.1: Implementar dealer landing page
- 21.2: Fix contraste de colores
- 21.3: Audit de keyboard navigation
