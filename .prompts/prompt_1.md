# AUDITORÍA — Sprint 6: Seller — Publicar Mi Primer Vehículo

**Fecha:** 2026-03-29 07:09:33
**Fase:** AUDIT
**Ambiente:** LOCAL/TUNNEL (cloudflared forzado: https://ought-feed-shipping-wright.trycloudflare.com)
**Usuario:** Seller (gmoreno@okla.com.do / $Gregory1)
**URL Base:** https://ought-feed-shipping-wright.trycloudflare.com

## Ambiente Local (HTTPS público via cloudflared tunnel)

> Auditoría corriendo contra **https://ought-feed-shipping-wright.trycloudflare.com** (cloudflared tunnel → Caddy → servicios).
> Asegúrate de que la infra esté levantada: `docker compose up -d`
> Frontend: `cd frontend/web-next && pnpm dev`
> Tunnel: `docker compose --profile tunnel up -d cloudflared`
> Caddy redirige: `/api/*` → Gateway, `/*` → Next.js (host:3000)

| Servicio                | URL                                                        |
| ----------------------- | ---------------------------------------------------------- |
| Frontend (tunnel)       | https://ought-feed-shipping-wright.trycloudflare.com       |
| API (tunnel)            | https://ought-feed-shipping-wright.trycloudflare.com/api/* |
| Auth Swagger (local)    | http://localhost:15001/swagger                             |
| Gateway Swagger (local) | http://localhost:18443/swagger                             |

## Instrucciones

Ejecuta TODA la auditoría con las herramientas MCP del browser (`mcp_aisquare-play_browser_*`).
NO uses scripts shell — usa `mcp_aisquare-play_browser_*`. Scripts solo para upload/download de fotos vía MediaService.

⚠️ **AMBIENTE LOCAL:** Todas las URLs apuntan a `https://ought-feed-shipping-wright.trycloudflare.com` en vez de producción.
Verifica que Caddy + infra + cloudflared tunnel estén corriendo antes de empezar.
Diferencias esperadas vs producción: ver `docs/HTTPS-LOCAL-SETUP.md`.

Para cada tarea:

1. Navega con `mcp_aisquare-play_browser_navigate` a la URL indicada
2. Toma screenshot cuando se indique
3. Documenta bugs y discrepancias en la sección 'Hallazgos'
4. Marca la tarea como completada: `- [ ]` → `- [x]`
5. Al terminar TODAS las tareas, agrega `READ` al final

## 🔧 PROTOCOLO DE TROUBLESHOOTING OKLA

> **Ejecutar este protocolo ANTES de cada sprint y cuando cualquier paso falle.**
> El problema más frecuente: containers Docker caídos → toda la UI falla.

### PASO 0 — Verificar Docker Desktop

```bash
docker info > /dev/null 2>&1 || echo "❌ Docker Desktop NO está corriendo — ábrelo primero"
```

Si Docker Desktop no responde → Abrir Docker Desktop app → esperar 30s → reintentar.

### PASO 1 — Health Check Rápido (10 segundos)

```bash
# Ver estado de TODOS los containers
docker compose ps --format "table {{.Name}}\t{{.Status}}\t{{.Ports}}" 2>/dev/null

# Containers críticos que DEBEN estar healthy:
#   postgres_db, redis, pgbouncer, caddy, gateway, authservice, userservice
# Si alguno dice "unhealthy" o "Exit" → ir a PASO 2
```

### PASO 2 — Restart Selectivo (solo lo caído)

```bash
# Identificar containers problemáticos
docker compose ps --status=exited --format "{{.Name}}" 2>/dev/null
docker compose ps --status=unhealthy --format "{{.Name}}" 2>/dev/null

# Restart SOLO los caídos (no reiniciar todo)
docker compose restart <nombre-del-servicio>

# Si es postgres o redis (infra base), restart en orden:
docker compose restart postgres_db && sleep 10
docker compose restart pgbouncer && sleep 5
docker compose restart redis && sleep 5
# Luego los servicios que dependen de ellos:
docker compose restart authservice gateway userservice roleservice errorservice
```

### PASO 3 — Si el restart no funciona → Diagnóstico profundo

```bash
# Ver logs del container problemático (últimas 50 líneas)
docker compose logs --tail=50 <servicio-problematico>

# Problemas comunes y soluciones:
# ┌─────────────────────────────────────┬─────────────────────────────────────────────┐
# │ Error en logs                       │ Solución                                    │
# ├─────────────────────────────────────┼─────────────────────────────────────────────┤
# │ "connection refused" a postgres     │ docker compose restart postgres_db pgbouncer│
# │ "connection refused" a redis        │ docker compose restart redis                │
# │ "connection refused" a rabbitmq     │ docker compose --profile core up -d rabbitmq│
# │ "port already in use"               │ lsof -i :<puerto> | kill PID               │
# │ "no space left on device"           │ docker builder prune -f                     │
# │ "OOM killed" / memory               │ Docker Desktop → Settings → Resources →    │
# │                                     │   subir RAM a 16GB                          │
# │ authservice unhealthy               │ docker compose restart authservice           │
# │                                     │   Si persiste: docker compose logs authserv  │
# │ gateway unhealthy                   │ docker compose restart gateway               │
# │ "certificate expired" / TLS         │ cd infra && ./setup-https-local.sh          │
# │ tunnel no conecta                   │ docker compose --profile tunnel restart      │
# │                                     │   cloudflared                               │
# │ frontend "ECONNREFUSED"             │ Verificar: cd frontend/web-next && pnpm dev │
# │ "rabbitmq not ready"               │ docker compose --profile core up -d rabbitmq│
# │                                     │   && sleep 30 (RabbitMQ tarda en arrancar)  │
# └─────────────────────────────────────┴─────────────────────────────────────────────┘
```

### PASO 4 — Nuclear Reset (solo si PASO 2-3 fallan)

```bash
# Parar TODO y arrancar limpio (NO borra datos, solo reinicia containers)
docker compose down
docker compose up -d                  # infra base
sleep 15                              # esperar postgres + redis
docker compose --profile core up -d   # auth, gateway, user, role, error
sleep 20                              # esperar que arranquen
docker compose ps                     # verificar todo healthy
```

### PASO 5 — Verificar conectividad end-to-end

```bash
# 1. Gateway responde?
curl -s -o /dev/null -w "%{http_code}" http://localhost:18443/health

# 2. Auth responde?
curl -s -o /dev/null -w "%{http_code}" http://localhost:15001/health

# 3. Frontend responde? (si corre con pnpm dev)
curl -s -o /dev/null -w "%{http_code}" http://localhost:3000

# 4. Caddy proxea correctamente?
curl -s -o /dev/null -w "%{http_code}" https://okla.local/api/health

# 5. Tunnel funciona? (si aplica)
# curl -s -o /dev/null -w "%{http_code}" <tunnel-url>/api/health
```

### Servicios y sus puertos (referencia rápida)

| Servicio            | Puerto Local | Health Check            | Perfil               |
| ------------------- | ------------ | ----------------------- | -------------------- |
| postgres_db         | 5433         | pg_isready              | (base)               |
| redis               | 6379         | redis-cli ping          | (base)               |
| pgbouncer           | 6432         | pg_isready              | (base)               |
| caddy               | 443/80       | curl https://okla.local | (base)               |
| consul              | 8500         | /v1/status/leader       | (base)               |
| seq                 | 5341         | /api/health             | (base)               |
| authservice         | 15001        | /health                 | core                 |
| gateway             | 18443        | /health                 | core                 |
| userservice         | 15002        | /health                 | core                 |
| roleservice         | 15101        | /health                 | core                 |
| errorservice        | 5080         | /health                 | core                 |
| vehiclessaleservice | —            | /health                 | vehicles             |
| mediaservice        | —            | /health                 | vehicles             |
| contactservice      | —            | /health                 | vehicles             |
| chatbotservice      | 5060         | /health                 | ai (HOST, no Docker) |
| searchagent         | —            | /health                 | ai                   |
| supportagent        | —            | /health                 | ai                   |
| pricingagent        | —            | /health                 | ai                   |
| billingservice      | —            | /health                 | business             |
| kycservice          | —            | /health                 | business             |
| notificationservice | —            | /health                 | business             |
| cloudflared         | —            | docker logs             | tunnel               |

### Árbol de dependencias (restart en este orden)

```
postgres_db → pgbouncer → redis → consul
    ↓
authservice → roleservice → userservice
    ↓
gateway → (todos los demás servicios)
    ↓
caddy → (proxea todo)
    ↓
cloudflared → (tunnel público)
    ↓
frontend (pnpm dev en host, NO Docker)
```

## Credenciales

| Rol                 | Email                  | Password       |
| ------------------- | ---------------------- | -------------- |
| Admin               | admin@okla.local       | Admin123!@#    |
| Buyer               | buyer002@okla-test.com | BuyerTest2026! |
| Dealer              | nmateo@okla.com.do     | Dealer2026!@#  |
| Vendedor Particular | gmoreno@okla.com.do    | $Gregory1      |

---

## TAREAS

### S6-T01: Wizard de publicación paso a paso

**Pasos:**

- [x] Paso 1: TROUBLESHOOTING: Verifica que vehiclessaleservice esté corriendo si usas perfil vehicles
- [x] Paso 2: Login como seller (gmoreno@okla.com.do / $Gregory1)
- [x] Paso 3: Navega a {BASE_URL}/publicar (o el botón 'Publicar' del navbar)
- [x] Paso 4: Toma screenshot — ¿es un wizard paso a paso?
- [x] Paso 5: Paso 1: Datos básicos (marca, modelo, año, versión)
- [x] Paso 6: ¿Los menús desplegables funcionan?
- [x] Paso 7: ¿Las marcas están en orden alfabético?
- [x] Paso 8: ¿Los modelos se filtran por marca seleccionada?
- [x] Paso 9: Paso 2: Características (km, combustible, transmisión, color)
- [x] Paso 10: ¿Los campos tienen validación?
- [x] Paso 11: ¿Los tipos de combustible están en español?
- [x] Paso 12: Paso 3: Fotos
- [x] Paso 13: ¿Hay zona de drag & drop?
- [x] Paso 14: ¿Indica límites (máx fotos, tamaño)?
- [x] Paso 15: Paso 4: Precio y ubicación
- [x] Paso 16: ¿Puedo poner precio en RD$?
- [x] Paso 17: ¿Las ubicaciones son de RD (Santo Domingo, Santiago, etc.)?
- [x] Paso 18: Paso 5: Preview antes de publicar
- [x] Paso 19: Toma screenshot del preview
- [x] Paso 20: ¿Se ve como lo verá el comprador?
- [x] Paso 21: NO PUBLICAR — solo documentar todo el flujo

**A validar:**

- [x] UF-040: ¿El wizard funciona paso a paso sin errores?
- [x] UF-041: ¿Los dropdowns de marca/modelo se filtran correctamente?
- [x] UF-042: ¿El drag & drop de fotos funciona?
- [x] UF-043: ¿El preview muestra lo que verá el comprador?
- [x] UF-044: ¿Todo está en español incluyendo ubicaciones?

**Hallazgos:**

**PASSES ✅**
- Wizard carga con 3 métodos de entrada: Escanear VIN, Escribir VIN, Llenar manualmente ✅
- 6 pasos en la progress bar: Información → Fotos → Video → Vista 360° → Precio → Revisión ✅
- Paso 1: Formulario completo con 3 secciones (Información Básica, Especificaciones, Condición y Colores) ✅
- Marcas en orden alfabético (Chevrolet, Ford, Honda, Hyundai, Jeep, Kia…) ✅
- Búsqueda de marcas funciona (escribió "Toyo" → filtró a Toyota) ✅
- Modelos filtran por marca seleccionada (Toyota → Camry, Corolla, Fortuner, Hilux, RAV4, 4Runner) ✅
- Tipos de combustible en español: Gasolina, Diésel, Híbrido, Eléctrico, Híbrido Enchufable, GLP/Gas ✅
- Teléfono pre-llenado del perfil del vendedor ✅
- Botón "Guardar borrador" visible en todos los pasos ✅
- Paso 2 Fotos: drag & drop zone con guía de 5 ángulos requeridos (Frente, Trasera, Lado izquierdo, Lado derecho, Frontal izquierda) ✅
- Paso 3 Video: gate premium "Función Premium" con botón "Saltar" para plan Libre ✅
- Paso 4 Vista 360°: gate premium con botón "Omitir este paso" ✅
- Paso 5 Precio: moneda RD$ DOP por defecto (también disponible US$ USD) ✅
- Paso 5: toggles "Precio negociable" y "Acepta intercambios" ✅
- Paso 5: "Auto-generar" descripción con IA ✅
- Paso 5: Widget de comparación de precios de mercado (Precio de Mercado IA) ✅
- Paso 5: Ubicación del vehículo viene del perfil del vendedor: "Santo Domingo, Distrito Nacional" ✅
- Paso 6 Revisión Final: score de calidad del listing (15/100 "Mejorable") con breakdown por categoría ✅
- Paso 6: Card preview con datos del vehículo, precio en RD$, badge Negociable, info de contacto ✅
- Paso 6: Sección "Revisión antes de publicar" con flujo de moderación 24h ✅
- Paso 6: Checkbox de confirmación legal con link a Términos de Servicio ✅
- Paso 6: Botones "Enviar a Revisión" + "Guardar como Borrador" ✅

**BUGS 🐛**
- **BUG-S6-01 [UX]**: Pantalla de bienvenida dice "3 pasos" pero el wizard tiene **6 pasos** — onboarding mismatch
- **BUG-S6-02 [VALIDACIÓN — CRÍTICO]**: El wizard avanza entre pasos sin validar campos requeridos. Dejando vacíos Tipo de Carrocería, Combustible, Transmisión, Kilometraje (Paso 1) y sin fotos (Paso 2), el botón "Siguiente" avanza sin bloquear. La validación solo aparece en el Paso 6 (Revisión) como lista. Debería ser step-by-step.
- **BUG-S6-03 [UX/COPY]**: Paso 2 Fotos muestra hint "Las publicaciones con **8+ fotos** reciben 3× más contactos" pero el plan Libre solo permite máx **5 fotos** — hint engañoso para usuarios en plan gratuito
- **BUG-S6-04 [I18N]**: Paso 6 Revisión muestra `Condición: Used` (inglés) en vez de `Usado` (español) — el valor raw del enum TypeScript/backend filtra hacia la UI

**MENORES**
- 4Runner aparece después de RAV4 en el dropdown de modelos Toyota (alfanumérico incorrecto — números deberían preceder letras)
- Precio de Mercado IA retorna "No hay suficientes datos" cuando falta el campo Año — comportamiento esperado pero la UX podría guiar mejor al usuario a completar el año

---

### S6-T02: Dashboard del vendedor

**Pasos:**

- [ ] Paso 1: Navega a {BASE_URL}/cuenta/mis-vehiculos
- [ ] Paso 2: Toma screenshot — ¿veo mis vehículos publicados?
- [ ] Paso 3: ¿Puedo editar un vehículo existente?
- [ ] Paso 4: ¿Puedo pausar/activar un listado?
- [ ] Paso 5: ¿Veo estadísticas (vistas, contactos)?
- [ ] Paso 6: Navega a {BASE_URL}/cuenta/suscripcion
- [ ] Paso 7: Toma screenshot — ¿veo mi plan actual?
- [ ] Paso 8: ¿Los planes coinciden con lo que vi en /vender como guest?
- [ ] Paso 9: Navega a {BASE_URL}/cuenta/estadisticas (si existe)
- [ ] Paso 10: ¿Hay métricas útiles para el vendedor?
- [ ] Paso 11: Cierra sesión

**A validar:**

- [x] UF-045: ✅ Dashboard muestra sección de vehículos con empty state apropiado
- [x] UF-046: ⚠️ PARCIAL — sin vehículos activos no se pudo probar editar/pausar; UI de tabs y búsqueda presentes
- [x] UF-047: ✅ Planes en /cuenta/suscripcion coinciden con estructura de 3 niveles (Libre/Estándar/Verificado)
- [x] UF-048: ✅ Estadísticas con premium gate apropiado; para plan Libre la página explica qué se obtiene al subir

**Hallazgos:**

**PASSES ✅**
- /cuenta (Dashboard): "Mi Panel de Vendedor" con badge de plan "Libre" ✅
- KPIs en el dashboard: 0 Vehículos Activos, 0 Ventas Completadas, — Calificación, — Tasa de Respuesta ✅
- Banner "Verifica tu identidad para vender" con CTA "Verificar ahora" ✅
- Upsell banner (dismissable ×) con "Actualizar Plan →" ✅
- "Acciones Rápidas": 5 tiles — Mis Vehículos, Consultas, Estadísticas, Pagos, Mi Plan ✅
- /cuenta/mis-vehiculos: Empty state "No tienes vehículos publicados" + CTA "+ Publicar vehículo" ✅
- Tabs de ciclo de vida: Todos, Activos, En Revisión, Rechazados, Pausados, Vendidos ✅
- Buscador "Buscar por título..." ✅
- /cuenta/suscripcion: 3 planes con precios en RD$ (Libre=Gratis, Estándar=RD$579/publicación, Verificado=RD$2,029/mes) ✅
- Plan actual "Libre" marcado como "Plan Actual" ✅
- Medidores de uso: Publicaciones activas 0/1, Fotos por vehículo 0/5 ✅
- /cuenta/estadisticas: Premium gate correcto — "Requiere: Verificado" con CTA "Mejorar Mi Plan" ✅
- Sidebar navegación completa: Dashboard, Mi Perfil, Mi Garage, Estadísticas, Consultas Recibidas, Reseñas, Favoritos, Alertas de Precio, Pagos, Seguridad, Notificaciones, Preferencias, Cerrar Sesión ✅

**BUGS 🐛**
- **BUG-S6-05 [UX]**: "Destacadas este mes: **0/0**" en /cuenta/suscripcion muestra barra roja al 100% — plan Libre no incluye destacadas, debería mostrar "N/A" u ocultar el medidor
- **BUG-S6-06 [I18N/PRECIO]**: Plan Estándar muestra "Renovación de listing: **$6.99**" con símbolo $ en lugar de RD$ — posible mezcla de precio USD con el contexto RD$ del resto de la página
- **BUG-S6-07 [COPY — CRÍTICO]**: Dashboard upsell dice "Hasta **50 fotos** por publicación" pero Suscripción muestra máx 12 fotos (plan Verificado) — copy incorrecto que puede generar expectativas falsas

**PARCIAL**
- UF-046: Sin vehículos publicados no fue posible probar editar/pausar listados — requiere test adicional con vehículo activo

---

## Resultado

- Sprint: 6 — Seller — Publicar Mi Primer Vehículo
- Fase: AUDIT
- Ambiente: LOCAL/TUNNEL (cloudflared forzado: https://ought-feed-shipping-wright.trycloudflare.com)
- URL: https://ought-feed-shipping-wright.trycloudflare.com
- Estado: COMPLETADO
- Bugs encontrados: **7 bugs** (4 en S6-T01, 3 en S6-T02)
  - BUG-S6-01: Wizard dice "3 pasos" pero tiene 6 [UX]
  - BUG-S6-02: Sin validación step-by-step — deferred al Paso 6 [CRÍTICO]
  - BUG-S6-03: Hint "8+ fotos" en plan que permite máx 5 [UX/COPY]
  - BUG-S6-04: Condición "Used" en inglés en Revisión Final [I18N]
  - BUG-S6-05: Medidor "Destacadas 0/0" con barra roja en plan Libre [UX]
  - BUG-S6-06: Renovación listing "$6.99" debería ser "RD$" [I18N/PRECIO]
  - BUG-S6-07: Upsell dice "50 fotos" vs. máx real de 12 [COPY — CRÍTICO]
- Features confirmadas OK: Wizard 6 pasos, dropdowns marca/modelo, combustibles en español, RD$ por defecto, ubicación RD, score de calidad, moderation flow, planes con precios RD$, premium gates con upgrade CTAs

---

_Cuando termines, agrega la palabra READ al final de este archivo._
