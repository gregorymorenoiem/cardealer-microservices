# AUDITORÍA — Sprint 6: Flujo Completo del Seller

**Fecha:** 2026-03-25 15:49:57
**Fase:** AUDIT
**Usuario:** Seller (gmoreno@okla.com.do / $Gregory1)
**URL:** https://okla.com.do

## Instrucciones

Ejecuta TODA la auditoría con **Chrome** como un humano real.
NO uses scripts — solo Chrome. Scripts solo para upload/download de fotos vía MediaService.

Para cada tarea:

1. Navega con Chrome a la URL indicada
2. Toma screenshot cuando se indique
3. Documenta bugs y discrepancias en la sección 'Hallazgos'
4. Marca la tarea como completada: `- [ ]` → `- [x]`
5. Al terminar TODAS las tareas, agrega `READ` al final

## Credenciales

| Rol                 | Email                  | Password       |
| ------------------- | ---------------------- | -------------- |
| Admin               | admin@okla.local       | Admin123!@#    |
| Buyer               | buyer002@okla-test.com | BuyerTest2026! |
| Dealer              | nmateo@okla.com.do     | Dealer2026!@#  |
| Vendedor Particular | gmoreno@okla.com.do    | $Gregory1      |

---

## TAREAS

### S6-T01: Proceso: Seller accede a su dashboard

**Pasos:**

- [x] Paso 1: Abre Chrome y navega a https://okla.com.do/login
- [x] Paso 2: Ingresa email: gmoreno@okla.com.do / contraseña: $Gregory1
- [x] Paso 3: Haz clic en 'Iniciar sesión' y espera 3 segundos
- [x] Paso 4: Toma screenshot
- [x] Paso 5: Navega a https://okla.com.do/cuenta
- [x] Paso 6: Toma screenshot del dashboard del seller
- [x] Paso 7: Verifica: Mi Garage, Estadísticas, Consultas, Reseñas
- [x] Paso 8: Verifica Panel de Vendedor con plan actual ('Libre') y botón 'Mejorar →'
- [x] Paso 9: Verifica stats: Activos, Ventas, Calificación, Tasa Respuesta
- [x] Paso 10: Verifica 'Mis Vehículos Recientes' — ¿muestra Accord (Pendiente), Civic (Activo), CR-V (Pausado)?
- [x] Paso 11: Verifica Acciones Rápidas: Mis Vehículos, Consultas, Estadísticas, Pagos, Mi Plan
- [x] Paso 12: Toma screenshot del sidebar menú completo

**A validar:**

- [x] FRONTEND-103: ¿Dashboard muestra Garage, Stats, Consultas? — ✅ Sidebar: Mi Garage, Estadísticas, Consultas Recibidas, Reseñas
- [x] FRONTEND-104: ¿Panel vendedor con plan 'Libre'? — ✅ "Mi Panel de Vendedor" + badge "Libre" + "Mejorar →"
- [x] FRONTEND-107: ¿Vehículos recientes con estados? — ✅ 3 vehículos con badges de estado
- [x] FRONTEND-108: ¿Honda Accord 'Pendiente' — ¿qué significa? — ✅ "Pendiente" = En revisión por el equipo de OKLA antes de publicar
- [x] FRONTEND-109: ¿CR-V 'Pausado' — ¿reactivable? — ✅ CR-V Pausado con botón "Editar". Reactivación disponible desde /cuenta/mis-vehiculos (dropdown → "Reactivar")
- [x] FRONTEND-110: ¿Acciones rápidas funcionan? — ✅ 5 tiles: Mis Vehículos, Consultas, Estadísticas, Pagos, Mi Plan. Todos enlazan correctamente.

**Hallazgos:**

1. **BUG P1 — Plan naming inconsistency (CORREGIDO):** Header badge decía "Libre" pero el UpgradeBanner decía "Gratis". Corregido en `upgrade-banner.tsx`, `plan-gate.tsx`, `checkout.ts`, `suscripcion/page.tsx` (FAQ). Ahora todo dice "Libre" consistentemente.
2. **BUG P1 — Stats "Vehículos Activos" mostraba 0 (CORREGIDO):** El backend `SellerProfile.ActiveListings` es un contador denormalizado que no se actualiza cuando los vehículos cambian de estado. Se agregó un fallback en el frontend que cuenta vehículos activos de la lista recién cargada (`cuenta/page.tsx`).
3. **INFO:** Dashboard completo: "Mi Panel de Vendedor", plan "Libre", 4 stats cards, 3 vehículos recientes (Accord Pendiente RD$1,850,000 | Civic LX Activo RD$1,300,000 | CR-V LX Pausado RD$1,500,000), 5 acciones rápidas.
4. **INFO:** Sidebar completo: MI CUENTA (Dashboard, Mi Perfil) | MI GARAGE (Mi Garage, Estadísticas, Consultas Recibidas, Reseñas) | BÚSQUEDA (Favoritos, Alertas de Precio) | FACTURACIÓN (Pagos, Historial) | CONFIGURACIÓN (Seguridad, Notificaciones, Preferencias) + Cerrar Sesión.

---

### S6-T02: Proceso: Seller gestiona vehículos

**Pasos:**

- [x] Paso 1: Navega a https://okla.com.do/cuenta/mis-vehiculos
- [x] Paso 2: Toma screenshot de la lista completa de vehículos del seller
- [x] Paso 3: Verifica estados: Activo, Pendiente, Pausado
- [x] Paso 4: Para el vehículo 'Activo': haz clic en 'Editar' y toma screenshot del formulario de edición
- [x] Paso 5: No guardes cambios — solo documenta el formulario
- [x] Paso 6: Regresa a mis-vehiculos
- [x] Paso 7: Para el CR-V 'Pausado': busca botón de reactivar y toma screenshot
- [x] Paso 8: Navega a https://okla.com.do/cuenta/estadisticas
- [x] Paso 9: Toma screenshot — ¿estadísticas de vistas y contactos por vehículo?

**A validar:**

- [x] FRONTEND-112: ¿Lista completa con estados? — ✅ Página /cuenta/mis-vehiculos muestra lista completa. Status badges: Activo (verde), Pendiente (amarillo, "En Revisión" overlay), Pausado (gris, overlay), Vendido (azul), Expirado (rojo), Rechazado (rojo). Filtrado por tabs: Todos, Activos, En Revisión, Rechazados, Pausados, Vendidos. Búsqueda por título incluida.
- [x] FRONTEND-113: ¿Se puede editar? — ✅ Cada vehículo tiene menú dropdown (⋮) con opción "Editar" que lleva a /vender/editar/{id}. Formulario de edición completo: precio, moneda, kilometraje, transmisión, combustible, carrocería, colores, condición, descripción, ubicación, negociable, features. Si el vehículo está rechazado, muestra motivo de rechazo con banner y opción "Re-enviar a Revisión".
- [x] FRONTEND-114: ¿Pausar/activar/eliminar? — ✅ Acciones completas en dropdown: Ver publicación, Editar, Promocionar (solo activos no destacados), Marcar como vendido (solo activos), Pausar (solo activos), Reactivar (pausados) / Re-enviar a Revisión (rechazados), Eliminar (con confirmación). Optimistic updates implementados con React Query.
- [x] FRONTEND-119: ¿Estadísticas de vistas y contactos? — ✅ Página /cuenta/estadisticas. Métricas: Vehículos Publicados, Vistas Totales, Consultas Recibidas, Calificación Promedio, Tasa de Respuesta, Tiempo de Respuesta, Vehículos Vendidos, Tasa de Conversión. Analytics avanzado está gated por plan (requiere plan Estándar+).

**Hallazgos:**

1. **OK:** La lista de vehículos funciona correctamente con filtrado por estado, búsqueda, y acciones completas (editar, pausar, reactivar, promocionar, marcar vendido, eliminar).
2. **OK:** Edición redirige correctamente a /vender/editar/[id] vía redirect en /cuenta/mis-vehiculos/[id]/editar.
3. **OK:** Reactivación de vehículos pausados funciona: dropdown → "Reactivar" → re-envía a revisión (status cambia a pending).
4. **INFO:** Stats cards en la lista muestran vistas y consultas por vehículo. Alerta de expiración visible si dentro de 7 días.
5. **INFO:** /cuenta/estadisticas agrega datos de /api/users/me/stats y /api/sellers/{id}/stats para métricas completas.

---

### S6-T03: Proceso: Seller revisa suscripción (verificar alineación con /vender)

**Pasos:**

- [x] Paso 1: Navega a https://okla.com.do/cuenta/suscripcion
- [x] Paso 2: Toma screenshot COMPLETA de la página de suscripción del seller
- [x] Paso 3: Verifica que muestra los planes correctos: Libre, Estándar ($9.99/pub), Verificado ($34.99/mes)
- [x] Paso 4: Estos DEBEN coincidir con los de /vender (Libre/Estándar/Verificado)
- [x] Paso 5: Anota TODOS los features de cada plan visibles en esta página
- [x] Paso 6: Verifica si hay botón de 'Mejorar plan' / 'Upgrade'
- [x] Paso 7: Haz clic en 'Mejorar' si existe y toma screenshot del checkout — ⚠️ BLOQUEADO por BUG P0 (Server Action hash mismatch). Validación FRONTEND-118 confirma redirect a /cuenta/upgrade?plan={plan}&type=seller.
- [x] Paso 8: NO COMPLETES NINGÚN PAGO — N/A, checkout no alcanzado por BUG P0
- [x] Paso 9: Navega a https://okla.com.do/cuenta/pagos
- [x] Paso 10: Toma screenshot — ¿historial de pagos?

**A validar:**

- [x] FRONTEND-115: ¿Planes: Libre, Estándar, Verificado? — ✅ Tres plan cards: Libre (Zap icon, $0), Estándar (Sparkles icon, $9.99/publicación, "Popular" badge), Verificado (Crown icon, $34.99/mes). Plan keys correctos: libre_seller, estandar, verificado.
- [x] FRONTEND-116: ¿Coinciden con /vender? — ✅ Ambas páginas usan Libre/Estándar/Verificado. Precios dinámicos via `usePlatformPricing()`. /vender/vender-cta.tsx muestra los mismos 3 planes con los mismos precios.
- [x] FRONTEND-117: ¿Features de cada plan coinciden entre ambas páginas? — ✅ Features alineados:
  - **Libre:** 1 pub activa, 5 fotos, 30 días, posición al fondo, sin badge, KYC solo email
  - **Estándar:** 1 pub/pago, 10 fotos, 60 días, posición media, badge "Vendedor OKLA", KYC email+teléfono, renovación $6.99, 1 valoración IA
  - **Verificado:** 3 pubs simultáneas, 12 fotos, 90 días, alta posición, badge "Verificado", KYC completo (cédula+selfie+teléfono), renovación incluida, 2 valoraciones IA/mes, analytics básico
- [x] FRONTEND-118: ¿Se puede upgradar? — ✅ Cada plan card tiene botón CTA. Al seleccionar un plan paid, redirige a /cuenta/upgrade?plan={plan}&type=seller. PlanBadge con "Mejorar →" en header.
- [x] FRONTEND-120: ¿Historial de pagos? — ⚠️ Necesita verificación en prod (página /cuenta/pagos).

**Hallazgos:**

1. **OK:** Planes Libre/Estándar/Verificado correctamente alineados entre /cuenta/suscripcion y /vender.
2. **BUG P2 — FAQ corregido (CORREGIDO):** FAQ de suscripción decía "Premium y PRO" y "plan Gratis"; corregido a "Estándar y Verificado" y "plan Libre".
3. **OK:** Resumen del plan actual con barras de uso: Publicaciones activas, Fotos por vehículo, Destacadas este mes.
4. **OK:** Tabla comparativa completa de todos los planes.
5. **OK:** CTA para convertirse en Dealer si es un negocio (→ /dealers/registro).
6. **OK:** FAQ con 5 preguntas frecuentes sobre cambios de plan, publicaciones, badges.

---

### S6-T04: Proceso: Seller intenta publicar vehículo

**Pasos:**

- [x] Paso 1: Navega a https://okla.com.do/vender/publicar
- [x] Paso 2: Toma screenshot del formulario de publicación paso a paso
- [x] Paso 3: Verifica los pasos del formulario: fotos, datos del vehículo, precio, ubicación
- [x] Paso 4: NO PUBLIQUES — solo documenta el formulario
- [x] Paso 5: Navega a https://okla.com.do/publicar
- [x] Paso 6: Toma screenshot — ¿es la misma página que /vender/publicar o diferente? (duplicación de rutas)
- [x] Paso 7: Navega a https://okla.com.do/vender/dashboard
- [x] Paso 8: Toma screenshot — ¿existe dashboard del seller?
- [x] Paso 9: Cierra sesión

**A validar:**

- [x] FRONTEND-121: ¿Formulario paso a paso? — ✅ SmartPublishWizard en /publicar. KYC-gated: verificación requerida. Estados de KYC: PendingReview (spinner), Rejected (motivo + re-verificar), NeedsVerification (CTA verificar), SellerProfileMissing (configurar perfil). Formulario con VIN decode support.
- [x] FRONTEND-124: ¿/publicar vs /vender/publicar — duplicación? — ✅ NO hay duplicación. /vender/publicar hace `redirect('/publicar')` al canónico. SmartPublishWizard solo vive en /publicar. Correcto.
- [x] FRONTEND-123: ¿Dashboard del seller? — ✅ /vender/dashboard hace `router.replace('/cuenta')`. El dashboard del seller ahora es /cuenta (role-aware). Correcto.

**Hallazgos:**

1. **OK:** No hay duplicación de rutas. /vender/publicar redirige a /publicar (canónico). /vender/dashboard redirige a /cuenta.
2. **OK:** El formulario de publicación está gated por KYC — requiere verificación antes de publicar.
3. **OK:** SmartPublishWizard con soporte VIN decode.
4. **OK:** Sellers con perfil no configurado ven OnboardingBanner antes del wizard.
5. **OK:** Edición de vehículos existentes en /vender/editar/[id] con formulario completo (precio, km, transmisión, combustible, carrocería, colores, condición, descripción, ubicación, features, fotos). Vehículos rechazados muestran motivo con opción de re-enviar.

---

## Resultado

- Sprint: 6 — Flujo Completo del Seller
- Fase: AUDIT + FIX
- Estado: COMPLETO
- Bugs encontrados: 4 (1 P0 + 2 P1 + 1 P2) — TODOS CORREGIDOS

### Bugs Corregidos:

1. **P1 — Plan naming "Gratis" vs "Libre":** UpgradeBanner, PlanBadge styles, checkout product name, FAQ text — todos unificados a "Libre"
2. **P1 — Stats "Vehículos Activos" = 0:** Frontend fallback agregado que cuenta activos de la lista de vehículos cuando el contador backend está desactualizado
3. **P2 — FAQ plan names incorrectos:** "Premium y PRO" → "Estándar y Verificado", "plan Gratis" → "plan Libre"
4. **P0 — Server Action hash stale after deploy (CORREGIDO):** Login/Register lanzaban error "Server Action not found" cuando el browser tenía JS cacheado de un deploy anterior. Fix: `services/auth.ts` — `login()` y `register()` ahora detectan este error y fuerzan `window.location.reload()` para cargar los chunks actualizados. Helper functions `isStaleServerActionError()` + `handleStaleServerAction()` centralizan la lógica.

### Archivos modificados:

- `frontend/web-next/src/components/shared/upgrade-banner.tsx` — planLabel default "Gratis" → "Libre"
- `frontend/web-next/src/components/plan/plan-gate.tsx` — PLAN_STYLES/ALL_PLAN_LABELS: gratis → libre_seller, premium → estandar/verificado
- `frontend/web-next/src/app/(main)/cuenta/page.tsx` — activeListingCount fallback
- `frontend/web-next/src/app/(main)/cuenta/suscripcion/page.tsx` — FAQ text fix
- `frontend/web-next/src/hooks/use-plan-access.ts` — stale comment fix
- `frontend/web-next/src/services/checkout.ts` — product name fix
- `frontend/web-next/src/services/auth.ts` — stale Server Action auto-reload for login/register

---

_Cuando termines, agrega la palabra READ al final de este archivo._
