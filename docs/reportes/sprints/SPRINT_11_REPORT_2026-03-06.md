# 📊 Sprint 11 Report — Security, Forms & Design Tokens
**Fecha:** 2026-03-06  
**Commit:** `a541e0cb`  
**Build:** ✅ 213 páginas, 17.3s compilación  

---

## 🎯 Objetivos del Sprint
Reforzar la seguridad de mutaciones HTTP con csrfFetch, modernizar formularios con Zod+RHF, limpiar TypeScript `as any`, y reemplazar colores hardcodeados con design tokens.

---

## ✅ Tareas Completadas

### Task 54: csrfFetch en mutaciones HTTP desprotegidas
**Archivos:** `vender/leads/page.tsx`, `vender/importar/page.tsx`, `vehiculos/vehiculos-client.tsx`  
**Cambios:**
- **vender/leads**: POST `/api/leads/${id}/reply` y PATCH `/api/leads/${id}/status` ahora usan `csrfFetch`
- **vender/importar**: POST `/api/import/marketplace` y POST `/api/import/bulk` ahora usan `csrfFetch`
- **vehiculos-client**: POST `/api/banners/${id}/click` y POST `/api/banners/${id}/view` ahora usan `csrfFetch`
- Total: 6 endpoints protegidos con CSRF token automático

### Task 55: Refactorización de convert-to-seller con Zod + RHF
**Archivo:** `cuenta/convert-to-seller/page.tsx`  
**Cambios:**
- Creado schema Zod `sellerFormSchema` con validaciones: `businessName` (1-150 chars), `description` (max 2000), `location` (max 200), `acceptTerms` (literal true)
- Reemplazado `useState` + `handleInputChange` manual → `useForm` con `zodResolver`
- Agregados mensajes de error por campo (`formErrors.businessName.message`, etc.)
- Checkbox usa `setValue('acceptTerms', checked, { shouldValidate: true })`
- Botón submit deshabilitado vía `watch('acceptTerms')` y `watch('businessName')`

### Task 56: Corrección de TypeScript `as any`
**Archivos:** `vender/registro/page.tsx`, `document-capture.tsx`, `liveness-challenge.tsx`  
**Cambios:**
- **registro**: Eliminado `(validation as any).error?.issues ?? (validation as any).error?.errors ?? []` → `validation.error.issues ?? []` con tipado correcto de Zod SafeParse
- **Webcam (document-capture + liveness-challenge)**: Se intentó tipar `dynamic<React.ComponentProps<typeof ReactWebcam>>()` pero Next.js dynamic() no es compatible con los tipos de react-webcam. Se mantuvo `as any` con comentario ESLint actualizado explicando la incompatibilidad. **Decisión técnica documentada.**

### Task 57: Design tokens — colores hardcodeados → CSS variables
**Archivos:** 5 error.tsx (`dealers/[slug]`, `marcas/[marca]`, `blog/[slug]`, `guias/[slug]`, `checkout`)  
**Cambios:**
- `bg-[#00A870] hover:bg-[#007850]` → `bg-primary hover:bg-primary/85`
- `bg-[#00A870] hover:bg-[#00A870]/90` → `bg-primary hover:bg-primary/90`
- `text-[#00A870]` → `text-primary`
- Los error boundaries ahora respetan el tema centralizado vía CSS custom properties

---

## 📈 Métricas

| Métrica | Valor |
|---|---|
| Archivos modificados | 15 |
| Endpoints protegidos con CSRF | 6 |
| Formularios migrados a Zod+RHF | 1 |
| Colores hardcodeados eliminados | ~12 instancias |
| `as any` eliminados | 1 (registro) |
| `as any` mantenidos (justificado) | 2 (Webcam — incompatibilidad de tipos) |
| Páginas generadas | 213 |
| Tiempo de compilación | 17.3s |

---

## 🔍 Hallazgos y Decisiones Técnicas

1. **react-webcam + Next.js dynamic()**: La firma de tipos de `dynamic()` no acepta `React.ComponentProps<typeof ReactWebcam>` como generic. El patrón `dynamic<any>((() => import('react-webcam')) as any, {...})` es la única forma funcional. Documentado con comentario ESLint.

2. **100+ colores hardcodeados restantes**: Solo se limpiaron los 5 error.tsx en este sprint. Archivos como `mensajes/page.tsx` (~40 instancias) y `vender/vender-cta.tsx` (~20) requieren sprints dedicados.

3. **Zod v4 `error` vs `errorMap`**: Confirmado que Zod v4 usa `error` string directo en `z.literal()`, no `errorMap`.

---

## 🚀 Próximos Pasos (Sprint 12+)
- Limpieza masiva de colores hardcodeados en componentes principales
- Auditoría de formularios restantes sin Zod+RHF
- Testing E2E con Playwright para flujos críticos
- Revisión de performance (bundle size, lazy loading)
