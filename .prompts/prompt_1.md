# RE-AUDITORÍA (Verificación de fixes, intento 1/3) — Sprint 4: Login & Registro (Todos los Usuarios)
**Fecha:** 2026-03-25 13:41:25
**Fase:** REAUDIT
**Usuario:** Guest → Buyer, Seller, Dealer
**URL:** https://okla.com.do

## Instrucciones — RE-AUDITORÍA (Verificación de Fixes)
Esta es la re-verificación del Sprint 4 (intento 1/3).
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

### S4-T01: Auditar página de Login

**Pasos:**
- [ ] Paso 1: Abre Chrome y navega a https://okla.com.do/login
- [ ] Paso 2: Toma screenshot completa de la página de login
- [ ] Paso 3: Verifica layout: imagen izquierda + form derecha
- [ ] Paso 4: Verifica stats: 10,000+ Vehículos, 500+ Dealers, 50,000+ Usuarios
- [ ] Paso 5: Verifica botones social login: Google, Apple
- [ ] Paso 6: Verifica campos: Email, Contraseña, Recordarme, ¿Olvidaste tu contraseña?
- [ ] Paso 7: Verifica CTA: 'Iniciar sesión' + '¿No tienes cuenta? Regístrate gratis'
- [ ] Paso 8: Intenta hacer login con credenciales INCORRECTAS (test@test.com / wrongpass)
- [ ] Paso 9: Toma screenshot del error — ¿dice 'credenciales inválidas' sin revelar si el email existe?
- [ ] Paso 10: Haz clic en '¿Olvidaste tu contraseña?' y verifica a dónde redirige

**A validar:**
- [ ] FRONTEND-051: ¿'Olvidaste contraseña' lleva a /recuperar-contrasena?
- [ ] FRONTEND-053: ¿Error NO revela si email existe?
- [ ] FRONTEND-055: ¿CSRF protection?

**Hallazgos:**
_(documentar aquí lo encontrado)_

---

### S4-T02: Auditar Login como BUYER

**Pasos:**
- [ ] Paso 1: Navega a https://okla.com.do/login
- [ ] Paso 2: Ingresa email: buyer002@okla-test.com
- [ ] Paso 3: Ingresa contraseña: BuyerTest2026!
- [ ] Paso 4: Haz clic en 'Iniciar sesión'
- [ ] Paso 5: Espera 3 segundos y toma screenshot
- [ ] Paso 6: Verifica que redirige al homepage o al dashboard del buyer
- [ ] Paso 7: Verifica que el navbar muestra el nombre del buyer y avatar
- [ ] Paso 8: Verifica que 'Ingresar' cambió a menú de usuario
- [ ] Paso 9: Verifica si aparece icono de notificaciones con badge
- [ ] Paso 10: Toma screenshot del navbar después del login
- [ ] Paso 11: Cierra sesión (clic en avatar → Cerrar sesión)

**A validar:**
- [ ] FRONTEND-076: ¿Navbar muestra nombre del buyer?
- [ ] FRONTEND-077: ¿'Ingresar' cambia a menú de usuario?
- [ ] FRONTEND-078: ¿Icono de notificaciones con badge?

**Hallazgos:**
_(documentar aquí lo encontrado)_

---

### S4-T03: Auditar Login como SELLER

**Pasos:**
- [ ] Paso 1: Navega a https://okla.com.do/login
- [ ] Paso 2: Ingresa email: gmoreno@okla.com.do
- [ ] Paso 3: Ingresa contraseña: $Gregory1
- [ ] Paso 4: Haz clic en 'Iniciar sesión'
- [ ] Paso 5: Espera 3 segundos y toma screenshot
- [ ] Paso 6: Verifica que redirige correctamente
- [ ] Paso 7: Verifica que el navbar muestra 'Gregory' + 'Vendedor Particular'
- [ ] Paso 8: Verifica el badge de notificaciones (¿73 notificaciones?)
- [ ] Paso 9: Toma screenshot del navbar del seller
- [ ] Paso 10: Cierra sesión

**A validar:**
- [ ] FRONTEND-098: ¿Navbar muestra 'Gregory' + 'Vendedor Particular'?
- [ ] FRONTEND-099: ¿Badge '73' notificaciones es real o stale?

**Hallazgos:**
_(documentar aquí lo encontrado)_

---

### S4-T04: Auditar Login como DEALER

**Pasos:**
- [ ] Paso 1: Navega a https://okla.com.do/login
- [ ] Paso 2: Ingresa email: nmateo@okla.com.do
- [ ] Paso 3: Ingresa contraseña: Dealer2026!@#
- [ ] Paso 4: Haz clic en 'Iniciar sesión'
- [ ] Paso 5: Espera 3 segundos y toma screenshot
- [ ] Paso 6: Verifica que redirige correctamente
- [ ] Paso 7: Verifica que el navbar muestra nombre del dealer + badge verificado
- [ ] Paso 8: Toma screenshot del navbar del dealer
- [ ] Paso 9: Cierra sesión

**A validar:**
- [ ] FRONTEND-125: ¿Navbar muestra nombre + badge verificado?

**Hallazgos:**
_(documentar aquí lo encontrado)_

---

### S4-T05: Auditar página de Registro

**Pasos:**
- [ ] Paso 1: Navega a https://okla.com.do/registro
- [ ] Paso 2: Toma screenshot completa
- [ ] Paso 3: Verifica botones social: Google, Apple
- [ ] Paso 4: Verifica selector de intent: Comprar / Vender
- [ ] Paso 5: Verifica campos: Nombre, Apellido, Email, Teléfono (opcional), Contraseña, Confirmar
- [ ] Paso 6: Verifica checkboxes: Términos, Mayor de 18, Transferencia datos Art. 27 Ley 172-13
- [ ] Paso 7: NO CREAR CUENTA — solo documentar la UI
- [ ] Paso 8: Verifica que el link '¿Ya tienes cuenta? Inicia sesión' funciona
- [ ] Paso 9: Navega a https://okla.com.do/registro/dealer y toma screenshot — ¿existe registro de dealer separado?

**A validar:**
- [ ] FRONTEND-056: ¿Consent Ley 172-13 Art. 27 obligatorio?
- [ ] FRONTEND-060: ¿Comprar/Vender mapea a UserIntent?
- [ ] FRONTEND-062: ¿Registro dealer separado?
- [ ] FRONTEND-063: ¿Protección anti-bot?
- [ ] LEGAL-003: ¿Art. 27 Ley 172-13 consentimiento?
- [ ] LEGAL-011: ¿Verificación 18+?

**Hallazgos:**
_(documentar aquí lo encontrado)_

---

## Resultado
- Sprint: 4 — Login & Registro (Todos los Usuarios)
- Fase: REAUDIT
- Estado: EN PROGRESO
- Bugs encontrados: _(completar)_

---

_Cuando termines, agrega la palabra READ al final de este archivo._
