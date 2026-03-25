# RE-AUDITORÍA (Verificación de fixes, intento 1/3) — Sprint 2: Búsqueda & Filtros de Vehículos (Guest)

**Fecha:** 2026-03-25 10:21:52
**Fase:** REAUDIT
**Usuario:** Guest (sin login)
**URL:** https://okla.com.do

## Instrucciones — RE-AUDITORÍA (Verificación de Fixes)

Esta es la re-verificación del Sprint 2 (intento 1/3).
Re-ejecuta las mismas tareas de auditoría con Chrome para verificar que los fixes funcionan.

- Si TODOS los bugs están corregidos → agrega `READ` al final
- Si ALGÚN bug persiste → documenta cuáles persisten en 'Hallazgos'
  y agrega `READ` igualmente. El script enviará otra ronda de fixes.

IMPORTANTE: Usa Chrome como un humano. NO scripts.

## Credenciales

| Rol                 | Email                  | Password       |
| ------------------- | ---------------------- | -------------- |
| Admin               | admin@okla.local       | Admin123!@#    |
| Buyer               | buyer002@okla-test.com | BuyerTest2026! |
| Dealer              | nmateo@okla.com.do     | Dealer2026!@#  |
| Vendedor Particular | gmoreno@okla.com.do    | $Gregory1      |

---

## TAREAS

### S2-T01: Auditar listado y filtros de /vehiculos

**Pasos:**

- [x] Paso 1: Abre Chrome y navega a https://okla.com.do/vehiculos
- [x] Paso 2: Toma una screenshot de la página completa
- [x] Paso 3: Verifica que dice '149 vehículos encontrados' (o el conteo actual)
- [x] Paso 4: Verifica la trust bar: 'Vendedores verificados · +2,400 vehículos activos'
- [x] Paso 5: Verifica que los filtros laterales existen: Condición (Nuevo/Usado), Marca, Modelo, Precio, Año, Carrocería, Ubicación
- [x] Paso 6: Haz clic en el filtro de precio '< 1M' y toma screenshot de los resultados
- [x] Paso 7: Verifica que los resultados se actualizan con vehículos bajo RD$1,000,000
- [x] Paso 8: Limpia los filtros y haz clic en 'SUV' en carrocería
- [x] Paso 9: Toma screenshot y verifica que solo muestra SUVs
- [x] Paso 10: Verifica que cada vehicle card muestra: imagen, badge, año, km, combustible, ubicación, precio RD$ + ≈USD

**A validar:**

- [x] FRONTEND-018: ¿Combustible en inglés en algunos vehículos? → **PARCIALMENTE CORREGIDO** — Muestra "Gasolina" en orden por defecto, pero al aplicar sort=price_asc revierte a "gasoline". Inconsistencia de traducción según parámetros de URL.
- [x] FRONTEND-019: ¿Filtros de precio actualizan resultados? → **CORREGIDO** ✅ — Filtro "<1M" muestra correctamente 21 vehículos bajo RD$1,000,000.
- [x] FRONTEND-020: ¿Conversión RD$/USD correcta (tasa ≈60.5)? → **CORREGIDO** ✅ — Tasa ~60.5 verificada (RD$16,800,000 ≈ $277,686).
- [x] FRONTEND-026: ¿Ordenamiento funciona? → **CORREGIDO** ✅ — "Precio: Menor a mayor" ordena correctamente (RD$1,150,000 → RD$1,200,000 → RD$1,250,000).
- [x] FRONTEND-029: ¿Vehicle card muestra '0 km' para nuevos? → **CONFUSO** — Vehículos "Nuevo" muestran km altos (29,799 km, 24,141 km, 33,447 km), lo que sugiere un problema de clasificación de datos, no de display.

**Hallazgos:**
- Trust bar verificada: "Vendedores verificados · +2,400 vehículos activos · Contacto directo · Alertas gratis"
- 149 vehículos encontrados en listado completo
- Todos los filtros existen: Condición (Todos/Nuevo/Usado), Marca y Modelo, Precio, Año, Carrocería (Sedán, SUV, Crossover, Pickup, Hatchback, Coupé, Deportivo, Convertible, Van, Minivan), Ubicación
- Vehicle cards muestran: imagen, badge (PARTICULAR/DEALER), año, km, combustible, ubicación, precio RD$ + ≈USD
- Filtro SUV: 38 resultados, todos correctos
- FRONTEND-018 necesita fix adicional: la traducción depende del sort parameter usado

---

### S2-T02: Auditar paginación y vehículos patrocinados

**Pasos:**

- [x] Paso 1: Navega a https://okla.com.do/vehiculos
- [x] Paso 2: Scroll hasta el final de la primera página de resultados
- [x] Paso 3: Toma screenshot de la paginación (debe tener ~15 páginas)
- [x] Paso 4: Haz clic en 'Página 2' y verifica que carga nuevos vehículos manteniendo los filtros
- [x] Paso 5: Regresa a página 1
- [x] Paso 6: Busca los bloques de 'Vehículos Patrocinados (Publicidad)' intercalados en los resultados
- [x] Paso 7: Toma screenshot de un bloque de patrocinados
- [x] Paso 8: Verifica si los vehículos patrocinados repiten los mismos 3 (RAV4, CR-V, Tucson) — BUG conocido P0-010
- [x] Paso 9: Verifica que los patrocinados tienen badge visual diferente a los orgánicos

**A validar:**

- [x] FRONTEND-021: ¿Patrocinados se diferencian visualmente? → **CORREGIDO** ✅ — Patrocinados tienen badges "Dealer" + "Verificado", foto count (📷18, 📷22, 📷30), rating (⭐4.5, 4.6, 4.9), estimación mensual (Est. RD$X/mes). Clara diferenciación visual.
- [x] FRONTEND-024: ¿Paginación mantiene filtros? → **CORREGIDO** ✅ — Filtros persisten en URL params (body_type=suv). Paginación muestra correctamente (1 de 7 para 149, 1 de 2 para 38 SUVs).
- [x] FRONTEND-025: ¿Patrocinados repiten los mismos 3? → **BUG PERSISTE** ❌ — Dos bloques "Vehículos Patrocinados" en página 1, AMBOS contienen los mismos 3 vehículos: (1) 2022 Honda CR-V Touring, (2) 2023 Hyundai Tucson SEL, (3) 2024 Toyota Corolla LE CVT. Cambiaron de RAV4/CR-V/Tucson a CR-V/Tucson/Corolla pero siguen repitiendo.

**Hallazgos:**
- Paginación: 7 páginas para 149 vehículos (no 15 como indicado — posiblemente cambiaron el page size)
- Patrocinados: 2 bloques encontrados en página 1, ambos con idénticos 3 vehículos repetidos
- Los patrocinados tienen buena diferenciación visual (badges, fotos, ratings, estimación mensual)
- FRONTEND-025 requiere fix: rotar/randomizar los vehículos patrocinados entre bloques

---

### S2-T03: Auditar búsqueda y alertas sin auth

**Pasos:**

- [x] Paso 1: Navega a https://okla.com.do/vehiculos
- [x] Paso 2: Escribe 'Toyota Corolla' en la barra de búsqueda y presiona Enter
- [x] Paso 3: Toma screenshot de los resultados filtrados
- [x] Paso 4: Verifica que muestra solo Toyota Corolla
- [x] Paso 5: Haz clic en 'Guardar búsqueda' y verifica si pide login o permite guardar anónimamente
- [x] Paso 6: Haz clic en 'Activar alertas' y verifica si pide login
- [x] Paso 7: Haz clic en 'Contactar vendedor' en el primer vehículo y verifica si abre modal de login o permite contacto anónimo

**A validar:**

- [x] FRONTEND-005: ¿Búsqueda rápida funciona? → **CORREGIDO** ✅ — "Toyota Corolla" retorna 7 resultados con interpretación IA: "Toyota Corolla - todos los años, precios y condiciones" al 95% confianza, 3426ms.
- [x] FRONTEND-022: ¿'Guardar búsqueda' pide login? → **CORREGIDO** ✅ — Redirige a `/login?redirect=/vehiculos` mostrando formulario de login completo (email/password, Google, Apple).
- [x] FRONTEND-023: ¿'Activar alertas' pide login? → **CORREGIDO** ✅ — Redirige a `/login?callbackUrl=%2Fcuenta%2Falertas` mostrando formulario de login.
- [x] FRONTEND-030: ¿'Contactar vendedor' sin auth? → **CORREGIDO** ✅ — Botón "Chat en vivo" muestra popup inline: "Inicia sesión para chatear — Necesitas una cuenta para contactar al vendedor por chat. Es gratis y toma menos de un minuto." con botones "Iniciar sesión" y "Crear cuenta gratis". WhatsApp y "Ver teléfono" disponibles sin login.

**Hallazgos:**
- Búsqueda AI funciona correctamente con interpretación semántica (95% confianza)
- "Guardar búsqueda" → redirect a login ✅
- "Activar alertas" → redirect a login ✅
- "Chat en vivo" → popup pide login ✅ (UX excelente con mensaje claro)
- WhatsApp y Ver teléfono disponibles sin login (correcto — contacto directo no requiere cuenta)
- Sidebar tiene bloque "Crea una alerta" con CTA "Activar alertas" + nota "Sin spam · Cancela cuando quieras"

---

## Resultado

- Sprint: 2 — Búsqueda & Filtros de Vehículos (Guest)
- Fase: REAUDIT (intento 1/3)
- Estado: **COMPLETADO**
- Bugs totales verificados: 11
- Bugs corregidos: 8 (FRONTEND-005, 019, 020, 021, 022, 023, 026, 030)
- Bugs parcialmente corregidos: 1 (FRONTEND-018 — combustible en inglés con sort params)
- Bugs persistentes: 1 (FRONTEND-025 — patrocinados repiten mismos 3 vehículos)
- Bugs con datos confusos: 1 (FRONTEND-029 — vehículos "Nuevo" con km altos, posible data issue)

### Resumen de Bugs Pendientes

| Bug ID | Descripción | Estado | Prioridad |
|--------|-------------|--------|-----------|
| FRONTEND-018 | Combustible en inglés al usar sort=price_asc | PARCIAL | P2 |
| FRONTEND-025 | Patrocinados repiten mismos 3 vehículos en ambos bloques | PERSISTE | P1 |
| FRONTEND-029 | Vehículos "Nuevo" muestran km altos (29K-33K) | DATA ISSUE | P2 |

---

_Cuando termines, agrega la palabra READ al final de este archivo._
