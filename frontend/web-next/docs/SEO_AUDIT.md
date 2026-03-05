# 🔍 Auditoría SEO Técnico & Crawlabilidad — OKLA

**Fecha:** 2025-01-XX  
**Objetivo:** Optimizar indexación, crawlabilidad y Core Web Vitals SEO

---

## Correcciones Aplicadas

### P0 — Críticas

| # | Issue | Fix | Archivo |
|---|-------|-----|---------|
| 1 | OG image inexistente (`/og-image.jpg`) | Creado `opengraph-image.tsx` con generación dinámica de OG image (branded, 1200×630) | `src/app/opengraph-image.tsx` |
| 2 | Canonical URL faltante en vehículos | Agregado `alternates.canonical` en `generateMetadata` | `src/app/(main)/vehiculos/[slug]/page.tsx` |
| 3 | Hreflang fantasma (es-DO/en-US sin rutas) | Eliminado `languages` del alternates en root layout | `src/app/layout.tsx` |

### P1 — Importantes

| # | Issue | Fix | Archivo |
|---|-------|-----|---------|
| 4 | `/cuenta/` no bloqueado en robots.txt | Agregado `/cuenta/` a disallow list | `src/app/robots.ts` |
| 5 | Auth pages sin noindex | Creado `template.tsx` con meta noindex | `src/app/(auth)/template.tsx` |
| 6 | Cuenta pages sin noindex | Creado `template.tsx` con metadata robots noindex | `src/app/(main)/cuenta/template.tsx` |
| 7 | Sitemap con páginas estáticas faltantes | Agregadas: /precios, /ayuda, /blog, /guias, /seguridad, /reportar, /comparar | `src/app/sitemap.ts` |
| 8 | OG image referenciando archivo inexistente | Actualizado a `/opengraph-image` (ruta auto-generada) | `src/app/layout.tsx` |

---

## Inventario SEO

### ✅ Lo que está bien
- Root metadata completo (title template, OG, Twitter, robots, keywords)
- Structured data: Organization, WebSite+SearchAction, Vehicle, BreadcrumbList
- Sitemap dinámico con vehículos, dealers, marcas, tipos de carrocería, provincias
- Robots.txt con reglas por bot y bloqueo de bots maliciosos
- Canonical URLs en root layout
- Admin panel con noindex
- 404 page bien implementada
- Redirects configurados (301/307)
- Image optimization (AVIF/WebP)
- Security headers (HSTS, CSP)
- ISR caching en páginas clave

### 📊 Cobertura de Metadata por Página

| Grupo | Total | Metadata | OG | JSON-LD | noindex |
|-------|-------|----------|-----|---------|---------|
| Público | 20 | 18/20 | 3/20 | 2/20 | N/A |
| Auth | 6 | 0/6 | 0/6 | 0/6 | ✅ (template) |
| Cuenta | 8 | 0/8 | 0/8 | 0/8 | ✅ (template) |
| Dealer | 15 | 2/15 | 0/15 | 0/15 | via robots.txt |
| Admin | 10 | 1/10 | 0/10 | 0/10 | ✅ (layout) |
