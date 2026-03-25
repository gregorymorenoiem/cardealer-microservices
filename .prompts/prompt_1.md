# RE-AUDITORÍA (Verificación de fixes, intento 1/3) — Sprint 7: Flujo Completo del Dealer
**Fecha:** 2026-03-25 18:15:00
**Fase:** REAUDIT
**Usuario:** Dealer (nmateo@okla.com.do / Dealer2026!@#)
**URL:** https://okla.com.do

## Instrucciones — RE-AUDITORÍA (Verificación de Fixes)
Esta es la re-verificación del Sprint 7 (intento 1/3).
Re-ejecuta las mismas tareas de auditoría con Chrome para verificar que los fixes funcionan.

- Si TODOS los bugs están corregidos → agrega `READ` al final
- Si ALGÚN bug persiste → documenta cuáles persisten en 'Hallazgos'
  y agrega `READ` igualmente. El script enviará otra ronda de fixes.

IMPORTANTE: Usa Chrome como un humano. NO scripts.

## Credenciales
| Rol | Email | Password |
|-----|-------|----------|
| Admin | admin@okla.local | Admin123!@# |
| Buyer | buyer002@okla-test.com | BuyerTest2026! |
| Dealer | nmateo@okla.com.do | Dealer2026!@# |
| Vendedor Particular | gmoreno@okla.com.do | $Gregory1 |

---

## TAREAS

### S7-T01: Proceso: Dealer accede a dashboard y revisa inventario

**Pasos:**
- [ ] Paso 1: Abre Chrome y navega a https://okla.com.do/login
- [ ] Paso 2: Ingresa email: nmateo@okla.com.do / contraseña: Dealer2026!@#
- [ ] Paso 3: Haz clic en 'Iniciar sesión' y espera 3 segundos
- [ ] Paso 4: Toma screenshot
- [ ] Paso 5: Navega a https://okla.com.do/cuenta
- [ ] Paso 6: Toma screenshot del dashboard del dealer
- [ ] Paso 7: Verifica: inventario, leads, ventas, analytics
- [ ] Paso 8: Verifica el plan actual del dealer con opción de upgrade
- [ ] Paso 9: Navega a https://okla.com.do/cuenta/mis-vehiculos
- [ ] Paso 10: Toma screenshot del inventario del dealer
- [ ] Paso 11: Verifica conteo de vehículos vs lo que muestra la página pública del dealer

**A validar:**
- [ ] FRONTEND-127: ¿Dashboard dealer con inventario, leads, ventas?
- [ ] FRONTEND-128: ¿Plan actual visible con upgrade?

**Hallazgos:**
_(documentar aquí lo encontrado)_

---

### S7-T02: Proceso: Dealer revisa suscripción y planes

**Pasos:**
- [ ] Paso 1: Navega a https://okla.com.do/cuenta/suscripcion
- [ ] Paso 2: Toma screenshot de los planes de dealer
- [ ] Paso 3: Documenta los planes que ve: ¿son los 6 de /dealers o los 4 del backend?
- [ ] Paso 4: Verifica si los precios coinciden con /dealers
- [ ] Paso 5: Haz clic en 'Upgrade' o 'Mejorar plan' y toma screenshot del checkout
- [ ] Paso 6: Verifica si Stripe está integrado — ¿aparece formulario de pago?
- [ ] Paso 7: NO COMPLETES NINGÚN PAGO
- [ ] Paso 8: Regresa y navega a https://okla.com.do/cuenta/pagos
- [ ] Paso 9: Toma screenshot del historial de pagos

**A validar:**
- [ ] FRONTEND-130: ¿Muestra los 6 planes?
- [ ] FRONTEND-131: ¿Precios coinciden con /dealers?
- [ ] FRONTEND-132: ¿Upgrade/downgrade funciona?
- [ ] FRONTEND-133: ¿Stripe checkout integrado?
- [ ] PLAN-017: ¿Stripe checkout funcional?
- [ ] PLAN-018: ¿Stripe maneja DOP?

**Hallazgos:**
_(documentar aquí lo encontrado)_

---

### S7-T03: Proceso: Dealer publica y gestiona vehículos

**Pasos:**
- [ ] Paso 1: Navega a https://okla.com.do/vender/publicar
- [ ] Paso 2: Toma screenshot del formulario — ¿permite más fotos que seller según plan?
- [ ] Paso 3: NO PUBLIQUES — solo documenta
- [ ] Paso 4: Navega a https://okla.com.do/vender/importar
- [ ] Paso 5: Toma screenshot — ¿importación bulk disponible?
- [ ] Paso 6: Navega a https://okla.com.do/vender/leads
- [ ] Paso 7: Toma screenshot — ¿gestión de leads por vehículo?
- [ ] Paso 8: Navega a https://okla.com.do/vender/publicidad
- [ ] Paso 9: Toma screenshot — ¿gestión de campañas?

**A validar:**
- [ ] FRONTEND-134: ¿Más fotos según plan?
- [ ] FRONTEND-135: ¿Importación bulk?
- [ ] FRONTEND-136: ¿Gestión de leads?
- [ ] FRONTEND-137: ¿Campañas publicitarias?

**Hallazgos:**
_(documentar aquí lo encontrado)_

---

### S7-T04: Proceso: Dealer verifica página pública

**Pasos:**
- [ ] Paso 1: Navega a la página pública del dealer (buscar en /dealers el dealer de nmateo)
- [ ] Paso 2: Toma screenshot de la página pública
- [ ] Paso 3: Verifica inventario vs lo que muestra el dashboard
- [ ] Paso 4: Verifica badge de verificado
- [ ] Paso 5: Cierra sesión

**A validar:**
- [ ] FRONTEND-139: ¿Página pública con inventario completo?

**Hallazgos:**
_(documentar aquí lo encontrado)_

---

## Resultado
- Sprint: 7 — Flujo Completo del Dealer
- Fase: REAUDIT
- Estado: EN PROGRESO
- Bugs encontrados: _(completar)_

---

_Cuando termines, agrega la palabra READ al final de este archivo._
