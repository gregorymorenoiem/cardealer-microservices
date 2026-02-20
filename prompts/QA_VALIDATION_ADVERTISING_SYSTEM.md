# 🎯 PROMPT DE VALIDACIÓN QA Y DEPLOY — Sistema de Publicidad de Homepage OKLA

**Versión:** 1.0  
**Fecha:** Febrero 20, 2026  
**Ambiente:** Staging (DOKS - Digital Ocean Kubernetes)  
**Responsable:** Equipo QA + DevOps  
**Criterios de Aceptación:** 19 (Todos deben cumplirse para PASAR)  
**Estado Requerido:** ✅ 19/19 PASSING antes de producción

---

## 📋 TABLA DE CONTENIDOS

1. [Objetivo General](#objetivo-general)
2. [Alcance del Proyecto](#alcance-del-proyecto)
3. [Ambiente y Configuración Previa](#ambiente-y-configuración-previa)
4. [19 Criterios de Aceptación — Procedimiento de Validación](#19-criterios-de-aceptación--procedimiento-de-validación)
5. [Plan de Pruebas QA Detallado](#plan-de-pruebas-qa-detallado)
6. [Validación Técnica Backend](#validación-técnica-backend)
7. [Validación Técnica Frontend](#validación-técnica-frontend)
8. [Validación de Integración](#validación-de-integración)
9. [Procedimiento de Escalación de Errores](#procedimiento-de-escalación-de-errores)
10. [Reporte Final de Validación](#reporte-final-de-validación)
11. [Checklist de Deploy](#checklist-de-deploy)

---

## Objetivo General

Validar que el **Sistema de Publicidad de Homepage (AdvertisingService)** funciona correctamente en ambiente staging, cumpliendo íntegramente los **19 criterios de aceptación** técnicos y funcionales. Solo después de que TODOS los criterios pasen, se autoriza el deploy a producción.

**Objetivo Secundario:** Identificar, documentar y corregir cualquier defecto antes de que llegue a producción.

---

## Alcance del Proyecto

### ✅ INCLUIDO EN VALIDACIÓN

- **Backend:** AdvertisingService (.NET 8, CQRS/MediatR, EF Core)
- **Frontend:** Homepage OKLA, Dashboard Admin, Portal Dealer, Pages de Boost
- **Integración:** RabbitMQ, Redis, PostgreSQL, Notification Service
- **Deployment:** Kubernetes (DOKS) + GitHub Actions CI/CD
- **Performance:** Health checks, Rate limiting, Graceful degradation
- **Testing:** Unit tests, Integration tests, End-to-End tests

### ❌ NO INCLUIDO

- Testing de carga (LoadTesting) — requiere ambiente dedicado
- Penetration Testing — requiere equipo de seguridad separado
- Migración de datos — no aplica (datos nuevos)

---

## Ambiente y Configuración Previa

### 1. Verificar Estado del Cluster DOKS

```bash
# Conectar a cluster
doctl kubernetes cluster kubeconfig save okla-cluster

# Verificar que el cluster está online
kubectl cluster-info
kubectl get nodes

# Resultado esperado:
# NAME                           STATUS   ROLES    AGE
# okla-node-1 (or similar)       Ready    <none>   XXd
# okla-node-2 (or similar)       Ready    <none>   XXd
```

**Criterio de Éxito:** 2 nodos en estado `Ready`

### 2. Verificar Namespace y Recursos

```bash
# Ver namespace okla
kubectl get namespace okla
kubectl get pods -n okla

# Resultado esperado: namespace EXISTS y pods están Running/Ready
```

**Criterio de Éxito:** Namespace `okla` existe y tiene al menos 10 pods Running

### 3. Verificar Servicios Dependientes

```bash
# PostgreSQL
kubectl get pod -n okla -l app=postgres
kubectl exec -it deployment/postgres -n okla -- \
  psql -U okla_admin -d postgres -c "SELECT version();"

# Redis
kubectl get pod -n okla -l app=redis
kubectl exec -it deployment/redis -n okla -- redis-cli ping

# RabbitMQ
kubectl get pod -n okla -l app=rabbitmq
kubectl port-forward svc/rabbitmq 15672:15672 -n okla &
# Acceder a http://localhost:15672 (credenciales en secret)

# Gateway
kubectl get pod -n okla -l app=gateway
kubectl logs -f deployment/gateway -n okla | head -20
```

**Criterio de Éxito:** Todos los servicios responden correctamente

### 4. Verificar Base de Datos

```bash
# Conectar a PostgreSQL
kubectl exec -it deployment/postgres -n okla -- \
  psql -U okla_admin -d postgres -c "\l"

# Verificar si database 'advertising_db' existe
# Si NO existe, crear:
kubectl exec -it deployment/postgres -n okla -- \
  psql -U okla_admin -d postgres -c "CREATE DATABASE advertising_db;"

# Verificar tablas después del deploy:
kubectl logs -f deployment/advertisingservice -n okla | grep "EF Core"
```

**Criterio de Éxito:** Database `advertising_db` existe y tablas están creadas (migrations aplicadas)

### 5. Verificar Secretos K8s

```bash
# Ver si existen secrets requeridos
kubectl get secret -n okla | grep -i advertising

# Si NO existen, crear (con valores reales):
kubectl create secret generic advertising-secrets \
  --namespace=okla \
  --from-literal=JWT_KEY='...' \
  --from-literal=RABBITMQ_HOSTNAME='rabbitmq' \
  --from-literal=REDIS_CONNECTION='redis:6379'
```

**Criterio de Éxito:** Todos los secrets están presentes

---

## 19 Criterios de Aceptación — Procedimiento de Validación

### ✅ **CRITERIO #1: Homepage muestra "Destacados" con datos reales o fallback**

**Descripción:** El homepage debe mostrar una sección "⭐ Destacados" con vehículos de campañas activas. Si no hay campañas, usa fallback hardcodeado.

**Procedimiento de Validación:**

1. **Acceder a homepage:**

   ```bash
   # Obtener IP del LoadBalancer
   kubectl get ingress -n okla
   # Abrir https://okla.com.do o http://localhost:3000 (local)
   ```

2. **Verificar sección "Destacados":**
   - [ ] Sección visible en el homepage (top area, después del nav)
   - [ ] Contiene mínimo 3 vehículos mostrados en carrusel
   - [ ] Cada vehículo muestra: foto, título, precio, badge "Destacado"
   - [ ] Carrusel tiene botones next/prev funcionales
   - [ ] Se puede hacer scroll horizontal (móvil)

3. **Caso A: Con campañas activas:**
   - [ ] Datos provienen de endpoint `/api/advertising/rotation/homepage`
   - [ ] Los vehículos tienen IDs y datos que existen en VehicleService
   - [ ] Badge "⭐ Destacado" es visible

4. **Caso B: Sin campañas activas (fallback):**

   ```javascript
   // Verificar en DevTools → Console
   console.log("featured vehicles (fallback):", window.__FALLBACK_VEHICLES__);
   ```

   - [ ] Se usan vehículos hardcodeados como fallback
   - [ ] Fallback carga sin error de red
   - [ ] Transición suave (no salto abrupto)

5. **Test API directa:**
   ```bash
   curl -s https://okla.com.do/api/advertising/rotation/homepage | jq .
   # Esperado: { "success": true, "data": { "vehicles": [...], "algorithm": "..." } }
   ```

**Criterio de Éxito:**

- ✅ Sección visible y funcional
- ✅ Datos reales O fallback trabajando sin errores

---

### ✅ **CRITERIO #2: Carrusel de marcas carga dinámicos y mantiene scroll infinito**

**Descripción:** El carrusel de 12 marcas debe cargar datos del AdvertisingService (nombre, logo, contador de vehículos) y permitir scroll infinito horizontal.

**Procedimiento de Validación:**

1. **Verificar carga de datos:**

   ```bash
   # API call
   curl -s https://okla.com.do/api/advertising/config/brands | jq '.data | length'
   # Esperado: 12 (o el número de marcas activas)
   ```

2. **Verificar en el navegador:**
   - [ ] Sección "Marcas" visible (carrusel horizontal)
   - [ ] Muestra 12 marcas (Toyota, Honda, Hyundai, Kia, Nissan, Mazda, Ford, Chevrolet, BMW, Mercedes, Audi, Volkswagen)
   - [ ] Cada marca muestra: Logo o iniciales, nombre, contador de vehículos
   - [ ] Contador >= 0 (puede estar en 0 si no hay vehículos)

3. **Verificar scroll infinito:**
   - [ ] Hacer scroll horizontal hasta el final
   - [ ] Carrusel vuelve al inicio (loop infinito)
   - [ ] No hay salto abrupto, transición suave
   - [ ] Mobile: swipe left/right funciona

4. **Verificar datos dinámicos:**
   - [ ] Cambiar contador de vehículos en admin
   - [ ] Refrescar página — contador debe actualizarse
   - [ ] Si se desactiva una marca (IsVisible=false), no debe aparecer

5. **DevTools Console:**
   ```javascript
   // Verificar que los datos vienen del API, no hardcodeados
   const brands = document.querySelector('[data-testid="brand-slider"]');
   console.log(brands.getAttribute("data-source")); // Debe decir "api"
   ```

**Criterio de Éxito:**

- ✅ 12 marcas visibles
- ✅ Datos cargan desde API (no hardcodeado)
- ✅ Scroll infinito funcionando

---

### ✅ **CRITERIO #3: Las 6 categorías muestran imágenes/gradientes configuradas**

**Descripción:** Las 6 categorías (SUV, Sedán, Camioneta, Deportivo, Eléctrico, Híbrido) muestran imágenes y gradientes configurados desde admin.

**Procedimiento de Validación:**

1. **Verificar API de categorías:**

   ```bash
   curl -s https://okla.com.do/api/advertising/config/categories | jq '.data'
   # Esperado: Array de 6 categorías con ImageUrl + Gradient
   ```

2. **Verificar en el navegador:**
   - [ ] Sección "Categorías" visible (6 cards grid)
   - [ ] Cada card muestra: imagen de fondo (o gradient si no hay imagen), título, descripción
   - [ ] 6 categorías presentes: SUV, Sedán, Camioneta, Deportivo, Eléctrico, Híbrido
   - [ ] Gradientes correctos si no hay imagen URL
     - SUV: blue-600 to blue-800
     - Sedán: primary to primary/90
     - Camioneta: amber-600 to amber-800
     - Deportivo: red-600 to red-800
     - Eléctrico: green-600 to green-800
     - Híbrido: teal-600 to teal-800

3. **Verificar imágenes:** (si admin subió URLs)
   - [ ] Imagen carga correctamente (no broken image)
   - [ ] Imagen es responsive (no se distorsiona en mobile)
   - [ ] Performance: imagen cargada < 2 segundos

4. **Verificar cambios desde admin:**
   - [ ] Admin cambia imagen de "SUV"
   - [ ] Refrescar homepage — nueva imagen debe aparecer
   - [ ] Cache invalidado correctamente

5. **Test responsive:**
   - [ ] Desktop (1920px): 6 cards en grid 3x2
   - [ ] Tablet (768px): 2 cards por fila
   - [ ] Mobile (375px): 1 card por fila, scrollable vertical

**Criterio de Éxito:**

- ✅ 6 categorías visibles con imágenes/gradientes correctos
- ✅ Datos sincronizados con admin
- ✅ Responsive en todos los breakpoints

---

### ✅ **CRITERIO #4: Vendedor puede ir a `/mis-vehiculos/{id}/boost/` y completar flujo de pago**

**Descripción:** Un vendedor puede seleccionar un vehículo, ir a la página de boost, seleccionar plan y completar el pago.

**Procedimiento de Validación:**

1. **Pre-requisitos:**
   - [ ] Usuario logeado como vendedor (role: "Individual")
   - [ ] Usuario tiene al menos 1 vehículo publicado
   - [ ] BillingService está funcionando (pagos)

2. **Navegar a página de boost:**

   ```bash
   # Como vendedor logueado:
   # Ir a: https://okla.com.do/mis-vehiculos
   # Hacer click en vehículo → botón "Boost" (si existe)
   # URL debe ser: /mis-vehiculos/{id}/boost
   ```

3. **Verificar página de boost:**
   - [ ] Página carga sin error 404
   - [ ] Muestra datos del vehículo: foto, título, año, precio actual
   - [ ] Muestra 3 opciones de plan:
     - [ ] Opción 1: "FeaturedSpot 7 días" — RD$X
     - [ ] Opción 2: "PremiumSpot 15 días" — RD$Y
     - [ ] Opción 3: "PremiumSpot 30 días" — RD$Z
   - [ ] Precios son los correctos (según pricing model)
   - [ ] Se puede seleccionar un plan (radio button o card)
   - [ ] Botón "Ir a Pago" o "Comprar" visible

4. **Completar flujo de pago:**
   - [ ] Click en botón "Comprar" → redirige a `/checkout?campaign={id}`
   - [ ] En checkout aparece el monto correcto
   - [ ] Se puede seleccionar método de pago (tarjeta, etc.)
   - [ ] **IMPORTANTE:** No hacer pago real — usar sandbox de Azul/PixelPay
   - [ ] Después de pago exitoso (mock), redirige a `/mis-vehiculos` con confirmación

5. **Verificar en backend:**

   ```bash
   # Ver que la campaña fue creada en BD
   kubectl exec -it deployment/advertisingservice -n okla -- \
     psql -U okla_admin -d advertising_db -c \
     "SELECT id, title, status, vehicle_id FROM ad_campaigns WHERE vehicle_id = '{vehicle_id}' LIMIT 1;"

   # Esperado: Campaign con status = "Pending" (esperando pago)
   ```

**Criterio de Éxito:**

- ✅ Página de boost accesible
- ✅ Planes mostrados correctamente
- ✅ Flujo de pago completable
- ✅ Campaña creada en BD después del pago

---

### ✅ **CRITERIO #5: Campaña se activa automáticamente al recibir `billing.payment.completed`**

**Descripción:** Cuando el evento `billing.payment.completed` llega de RabbitMQ, el AdvertisingService debe consumirlo y activar la campaña (cambiar estado a "Active").

**Procedimiento de Validación:**

1. **Monitorear RabbitMQ:**

   ```bash
   # Acceder a RabbitMQ Admin
   kubectl port-forward svc/rabbitmq 15672:15672 -n okla &
   # Ir a http://localhost:15672 (credenciales: guest/guest o desde secret)
   # Buscar exchange: billing.payment
   # Buscar queue: advertising.billing.payment.queue
   ```

2. **Completar pago (desde Criterio #4):**
   - [ ] Usuario completa pago exitoso
   - [ ] BillingService genera evento `billing.payment.completed`
   - [ ] Evento se publica en RabbitMQ

3. **Verificar consumo del evento:**

   ```bash
   # Ver logs del AdvertisingService
   kubectl logs -f deployment/advertisingservice -n okla | grep -i "payment\|activated"

   # Esperado: "BillingPaymentCompletedEvent consumed: campaign_id={id}"
   # O similar
   ```

4. **Verificar estado de la campaña:**

   ```bash
   # En BD, verificar que status cambió
   kubectl exec -it deployment/advertisingservice -n okla -- \
     psql -U okla_admin -d advertising_db -c \
     "SELECT id, status, activated_at FROM ad_campaigns WHERE id = '{campaign_id}';"

   # Esperado: status = 'Active', activated_at = timestamp reciente
   ```

5. **Verificar en homepage:**
   - [ ] Vehículo ahora aparece en sección "Destacados"
   - [ ] Transición suave (puede tomar 30 segundos hasta siguiente refresh)

6. **Test de resiliencia:**
   - [ ] Simular que el AdvertisingService está caído
   - [ ] Enviar evento de RabbitMQ
   - [ ] Cuando AdvertisingService se levanta, debe procesar el evento
   - [ ] Dead Letter Queue debe estar funcionando si hay error

**Criterio de Éxito:**

- ✅ Evento RabbitMQ consumido correctamente
- ✅ Campaña activada en BD
- ✅ Vehículo aparece en homepage "Destacados"

---

### ✅ **CRITERIO #6: Impresiones registradas (1 por campaña por sesión, deduplicado en Redis)**

**Descripción:** Cuando un usuario ve una campaña en el homepage, se registra 1 impresión. Si recarga la página en la misma sesión, NO se registra nuevamente (deduplicado en Redis).

**Procedimiento de Validación:**

1. **Setup:**
   - [ ] Tener campaña activa visible en homepage
   - [ ] Redis está funcionando

2. **Primer acceso a homepage:**
   - [ ] Abrir homepage
   - [ ] Esperar 3 segundos (tiempo para tracking)
   - [ ] DevTools Network → buscar request a `/api/advertising/tracking/impression`
   - [ ] Request enviado con: `campaignId`, `userId` (o sessionId), `timestamp`

3. **Verificar impresión registrada:**

   ```bash
   # Ver en BD
   kubectl exec -it deployment/advertisingservice -n okla -- \
     psql -U okla_admin -d advertising_db -c \
     "SELECT id, campaign_id, user_id, timestamp FROM ad_impressions ORDER BY timestamp DESC LIMIT 5;"

   # Esperado: 1 registro nuevo
   ```

4. **Verificar Redis deduplication:**

   ```bash
   # Conectar a Redis
   kubectl exec -it deployment/redis -n okla -- redis-cli

   # Ver keys de deduplication
   > KEYS impression:*
   # Esperado: impression:{campaign_id}:{user_id}:day_{date}

   # Ver TTL
   > TTL impression:{campaign_id}:{user_id}:day_{date}
   # Esperado: < 86400 (24 horas en segundos)
   ```

5. **Recargar página (mismo navegador, misma sesión):**
   - [ ] F5 o Command+R para recargar
   - [ ] Verificar que NO hay nuevo request de impresión
   - [ ] O si lo hay, verifica que Redis lo bloqueó (SETNX retorna 0)

6. **Verificar en BD:**

   ```bash
   # Debe haber solo 1 impresión, no 2
   SELECT COUNT(*) FROM ad_impressions WHERE campaign_id = '{id}' AND user_id = '{user_id}';
   # Esperado: 1
   ```

7. **Cambiar sesión (nueva ventana privada):**
   - [ ] Abrir homepage en ventana privada (nueva sesión)
   - [ ] Debería registrar OTRA impresión
   - [ ] Total ahora = 2

**Criterio de Éxito:**

- ✅ Impresiones registradas en BD
- ✅ Redis deduplication funcionando (1 por sesión)
- ✅ TTL de 24h en Redis

---

### ✅ **CRITERIO #7: Clicks registrados y budget actualizado si `pricingModel = PerView`**

**Descripción:** Cuando usuario hace click en un vehículo destacado y el modelo de precios es "PerView", se registra el click y se deduce del budget.

**Procedimiento de Validación:**

1. **Setup:**
   - [ ] Crear campaña con `pricingModel = "PerView"`
   - [ ] Asignar budget inicial: RD$5,000
   - [ ] Precio por view: RD$50
   - [ ] Campaña debe estar activa en homepage

2. **Hacer click en vehículo destacado:**
   - [ ] Hacer click en la foto o título del vehículo
   - [ ] Debe navegar a `/vehiculos/{slug}` (detalle del vehículo)
   - [ ] DevTools Network → verificar request a `/api/advertising/tracking/click`
   - [ ] Request contiene: `campaignId`, `userId`, `timestamp`

3. **Verificar click registrado:**

   ```bash
   kubectl exec -it deployment/advertisingservice -n okla -- \
     psql -U okla_admin -d advertising_db -c \
     "SELECT id, campaign_id, user_id, timestamp FROM ad_clicks ORDER BY timestamp DESC LIMIT 1;"

   # Esperado: 1 registro nuevo
   ```

4. **Verificar budget deducido:**

   ```bash
   # Ver estado de la campaña
   kubectl exec -it deployment/advertisingservice -n okla -- \
     psql -U okla_admin -d advertising_db -c \
     "SELECT id, budget_allocated, budget_consumed, budget_remaining FROM ad_campaigns WHERE id = '{campaign_id}';"

   # Esperado:
   # budget_allocated: 5000
   # budget_consumed: 50 (aumentó)
   # budget_remaining: 4950 (disminuyó)
   ```

5. **Hacer múltiples clicks:**
   - [ ] Click nuevamente 50 veces
   - [ ] Budget debe decrementarse RD$50 × 50 = RD$2,500
   - [ ] budget_remaining = 2,500

6. **Agotar presupuesto:**
   - [ ] Hacer 100 clicks (100 × RD$50 = RD$5,000)
   - [ ] budget_remaining debe ser 0
   - [ ] Campaña debe cambiar estado a "Completed" o "Exhausted"
   - [ ] Vehículo no debe aparecer más en "Destacados"

7. **Verificar con `pricingModel = "FixedMonthly"`:**
   - [ ] Si modelo es FixedMonthly, budget NO se deduce por click
   - [ ] Clicks siguen registrándose (trazabilidad)

**Criterio de Éxito:**

- ✅ Clicks registrados en BD
- ✅ Budget deducido correctamente (PerView)
- ✅ Campaña se marca como Completed cuando budget = 0
- ✅ FixedMonthly no deduce por click

---

### ✅ **CRITERIO #8: Rotación se refresca automáticamente según intervalo en BD**

**Descripción:** El `RotationRefreshJob` debe ejecutarse periódicamente (cada N minutos según `RefreshIntervalMinutes` en BD) y recalcular los "Destacados".

**Procedimiento de Validación:**

1. **Verificar configuración:**

   ```bash
   kubectl exec -it deployment/advertisingservice -n okla -- \
     psql -U okla_admin -d advertising_db -c \
     "SELECT id, algorithm_type, refresh_interval_minutes FROM rotation_configs WHERE is_active = true LIMIT 1;"

   # Esperado: refresh_interval_minutes = 30 (o valor configurado)
   ```

2. **Ver logs del job:**

   ```bash
   kubectl logs -f deployment/advertisingservice -n okla | grep -i "RotationRefreshJob"

   # Esperado cada 30 minutos (o intervalo configurado):
   # "RotationRefreshJob running..."
   # "Rotation refreshed with 10 vehicles"
   ```

3. **Cambiar intervalo y verificar:**

   ```bash
   # Cambiar intervalo a 1 minuto (para testing)
   kubectl exec -it deployment/advertisingservice -n okla -- \
     psql -U okla_admin -d advertising_db -c \
     "UPDATE rotation_configs SET refresh_interval_minutes = 1 WHERE is_active = true;"
   ```

4. **Esperar 1 minuto y verificar:**
   - [ ] Logs del pod deben mostrar ejecución del job
   - [ ] Timestamp del último refresh debe ser reciente
   - [ ] Vehículos en "Destacados" pueden haber cambiado (rotación)

5. **Verificar que no es hardcodeado:**
   - [ ] Cambiar intervalo a 60 minutos
   - [ ] Esperar > 2 minutos sin cambios
   - [ ] Vehículos en "Destacados" NO deben haber cambiado
   - [ ] Confirmando que lee del DB, no hardcodeado

6. **Simular fallo y recuperación:**
   - [ ] Detener AdvertisingService pod
   - [ ] Cambiar intervalo a 5 minutos
   - [ ] Levantar pod nuevamente
   - [ ] Debe leer el nuevo intervalo de BD
   - [ ] Próxima ejecución debe ser en 5 minutos

**Criterio de Éxito:**

- ✅ Job se ejecuta cada N minutos (desde BD)
- ✅ Intervalo se puede cambiar en BD
- ✅ Job respeta el nuevo intervalo después de restart

---

### ✅ **CRITERIO #9: Admin puede acceder a `/dashboard/publicidad/algoritmo` y cambiar tipo + pesos**

**Descripción:** Admin debe poder ir a dashboard, acceder a la página de algoritmo, ver tipo actual y cambiar algoritmo + pesos.

**Procedimiento de Validación:**

1. **Login como admin:**
   - [ ] Usuario logeado como admin (role: "Admin")

2. **Navegar a página:**

   ```bash
   # URL: https://okla.com.do/dashboard/publicidad/algoritmo
   # Verificar acceso (no 403 Forbidden)
   ```

3. **Verificar página carga correctamente:**
   - [ ] Página visible sin error
   - [ ] Muestra algoritmo actual (ej. "WeightedRandom")
   - [ ] Muestra estado actual de los pesos

4. **Cambiar algoritmo:**
   - [ ] Dropdown/Select con opciones: "WeightedRandom", "RoundRobin", "CTROptimized", "BudgetPriority"
   - [ ] Seleccionar otro algoritmo (ej. "RoundRobin")
   - [ ] Pesos deben desaparecer (RoundRobin no usa pesos)
   - [ ] O si es WeightedRandom, mostrar inputs de pesos

5. **Cambiar pesos (si algoritmo es WeightedRandom):**
   - [ ] Mostrar 4 sliders:
     - [ ] "Calidad" (0-100)
     - [ ] "CTR" (0-100)
     - [ ] "Presupuesto" (0-100)
     - [ ] "Novedad" (0-100)
   - [ ] Cambiar valores: Calidad=40, CTR=30, Presupuesto=20, Novedad=10
   - [ ] Total debe ser 100 (validación en tiempo real)
   - [ ] Botón "Guardar" disponible

6. **Guardar cambios:**
   - [ ] Click en "Guardar"
   - [ ] Esperar confirmación (toast o modal)
   - [ ] Verificar que no hay error 400

7. **Verificar cambios en BD:**

   ```bash
   kubectl exec -it deployment/advertisingservice -n okla -- \
     psql -U okla_admin -d advertising_db -c \
     "SELECT algorithm_type, quality_weight, ctr_weight, budget_weight, novelty_weight FROM rotation_configs WHERE is_active = true LIMIT 1;"

   # Esperado:
   # algorithm_type = RoundRobin (u otro seleccionado)
   # O si WeightedRandom: pesos deben ser 40, 30, 20, 10
   ```

8. **Verificar que cambios afectan rotación:**
   - [ ] Esperar siguiente refresh del job
   - [ ] Vehículos en "Destacados" deben cambiar según nuevo algoritmo/pesos
   - [ ] Confirmar que el algoritmo está siendo usado

**Criterio de Éxito:**

- ✅ Página accesible (admin only)
- ✅ Algoritmo se puede cambiar
- ✅ Pesos se pueden actualizar
- ✅ Cambios se guardan en BD

---

### ✅ **CRITERIO #10: Pesos validados (suma = 100%) antes de enviar**

**Descripción:** Cuando admin intenta guardar pesos que NO suman 100%, debe haber validación en cliente y servidor.

**Procedimiento de Validación:**

1. **Validación en Cliente (Frontend):**

   ```javascript
   // Abrir DevTools → Console
   // En página de algoritmo, cambiar valores:
   // Calidad=40, CTR=30, Presupuesto=20, Novedad=5 (total = 95)

   // Intentar guardar
   // Resultado esperado:
   // - Toast de error: "Los pesos deben sumar 100%. Actual: 95%"
   // - Botón "Guardar" deshabilitado (disabled)
   // - POST request NO se envía
   ```

2. **Validación en Servidor (Backend):**

   ```bash
   # Simular bypass de validación cliente (curl):
   curl -X PUT https://okla.com.do/api/advertising/config/rotation \
     -H "Authorization: Bearer {token}" \
     -H "Content-Type: application/json" \
     -d '{
       "algorithmType": "WeightedRandom",
       "qualityWeight": 40,
       "ctrWeight": 30,
       "budgetWeight": 20,
       "noveltyWeight": 5
     }'

   # Resultado esperado: 400 Bad Request
   # Respuesta: {"error": "Weights must sum to 100%. Current: 95%"}
   ```

3. **Test con rango tolerancia (±1%):**
   - [ ] Enviar pesos que suman 99%: debe aceptar (tolerancia = 1%)
   - [ ] Enviar pesos que suman 101%: debe aceptar (tolerancia = 1%)
   - [ ] Enviar pesos que suman 98%: debe rechazar (fuera de rango)

4. **Test de precisión decimal:**

   ```bash
   # Enviar con decimales que suman exactamente 100:
   # Calidad=33.33, CTR=33.33, Presupuesto=16.67, Novedad=16.67
   # Total = 100.00

   # Resultado esperado: 200 OK (aceptado)
   ```

5. **Verificar en BD que NO se guardó con suma incorrecta:**
   ```bash
   # Si intentamos guardar 95%, no debe estar en BD
   SELECT SUM(quality_weight + ctr_weight + budget_weight + novelty_weight) as total FROM rotation_configs;
   # Esperado: 100 (o muy cercano, ±0.01)
   ```

**Criterio de Éxito:**

- ✅ Cliente rechaza pesos si no suman 100%
- ✅ Servidor rechaza pesos si no suman 100% (±1% tolerancia)
- ✅ BD solo contiene configuraciones válidas

---

### ✅ **CRITERIO #11: Admin puede subir imagen para cada categoría**

**Descripción:** Admin va a `/dashboard/contenido/categorias` y puede subir una imagen para cada categoría (SUV, Sedán, etc.).

**Procedimiento de Validación:**

1. **Navegar a página:**

   ```bash
   # URL: https://okla.com.do/dashboard/contenido/categorias
   # Verificar acceso (no 403)
   ```

2. **Verificar página carga:**
   - [ ] Página visible sin error
   - [ ] Muestra 6 categorías (SUV, Sedán, Camioneta, Deportivo, Eléctrico, Híbrido)
   - [ ] Cada categoría tiene:
     - [ ] Nombre
     - [ ] Descripción actual (editable)
     - [ ] Imagen actual (si existe)
     - [ ] Botón "Cambiar imagen" o file input

3. **Subir imagen para categoría "SUV":**
   - [ ] Click en "Cambiar imagen" para SUV
   - [ ] Seleccionar archivo de imagen (JPG/PNG, < 5MB)
   - [ ] Esperar upload
   - [ ] Verificar preview de la imagen

4. **Guardar cambios:**
   - [ ] Click en "Guardar" o similar
   - [ ] Esperar confirmación

5. **Verificar en BD:**

   ```bash
   kubectl exec -it deployment/advertisingservice -n okla -- \
     psql -U okla_admin -d advertising_db -c \
     "SELECT category_key, image_url FROM category_image_configs WHERE category_key = 'suv';"

   # Esperado: image_url debe ser URL de la imagen subida
   ```

6. **Verificar en homepage:**
   - [ ] Ir a homepage
   - [ ] Sección de categorías debe mostrar la nueva imagen para SUV
   - [ ] Transición suave (puede haber 1-2 segundos de delay por cache)

7. **Test de límites:**
   - [ ] Intentar subir imagen > 5MB: debe rechazar
   - [ ] Intentar subir archivo no-imagen (.txt): debe rechazar
   - [ ] Dejar campo vacío y guardar: debe mantener imagen anterior (o usar gradient)

**Criterio de Éxito:**

- ✅ Admin puede subir imágenes
- ✅ Imágenes se guardan en BD o MediaService
- ✅ Imágenes aparecen en homepage
- ✅ Validación de tamaño/tipo de archivo

---

### ✅ **CRITERIO #12: Admin puede subir logo para cada marca**

**Descripción:** Admin va a `/dashboard/contenido/marcas` y puede subir un logo para cada una de las 12 marcas.

**Procedimiento de Validación:**

1. **Navegar a página:**

   ```bash
   # URL: https://okla.com.do/dashboard/contenido/marcas
   ```

2. **Verificar página carga:**
   - [ ] Página visible
   - [ ] Muestra 12 marcas en tabla o grid
   - [ ] Cada marca tiene: nombre, logo actual (si existe), botón "Cambiar logo"

3. **Subir logo para "Toyota":**
   - [ ] Click en "Cambiar logo"
   - [ ] Seleccionar archivo (PNG con fondo transparente recomendado)
   - [ ] Esperar upload
   - [ ] Preview visible

4. **Guardar cambios:**
   - [ ] Click "Guardar"
   - [ ] Confirmación

5. **Verificar en BD:**

   ```bash
   kubectl exec -it deployment/advertisingservice -n okla -- \
     psql -U okla_admin -d advertising_db -c \
     "SELECT brand_key, logo_url FROM brand_configs WHERE brand_key = 'toyota';"

   # Esperado: logo_url actualizado
   ```

6. **Verificar en homepage:**
   - [ ] Ir a carrusel de marcas
   - [ ] Toyota debe mostrar el nuevo logo (no solo iniciales "TO")

7. **Test para todas las marcas:**
   - [ ] Subir logos para al menos 3 marcas más
   - [ ] Verificar que cada una se actualiza correctamente

**Criterio de Éxito:**

- ✅ Admin puede subir logos
- ✅ Logos se guardan en BD o MediaService
- ✅ Logos aparecen en carrusel de marcas

---

### ✅ **CRITERIO #13: Reporte diario llega por email a las 8:00 AM RD a owners con campañas**

**Descripción:** El `DailyAdReportJob` debe ejecutarse diariamente a las 8:00 AM RD y enviar email a todos los owners (vendedores y dealers) que tienen campañas activas.

**Procedimiento de Validación:**

1. **Verificar job está configurado:**

   ```bash
   kubectl logs -f deployment/advertisingservice -n okla | grep -i "DailyAdReportJob"

   # Esperado en logs:
   # "DailyAdReportJob scheduled for 08:00 AM RD"
   ```

2. **Verificar zona horaria:**

   ```bash
   # Verificar que usa zona horaria RD (Atlantic/Santo_Domingo)
   kubectl exec -it deployment/advertisingservice -n okla -- date

   # O ver en código:
   # var ryZone = TimeZoneInfo.FindSystemTimeZoneById("Atlantic/Santo_Domingo");
   ```

3. **Forzar ejecución del job (para testing):**
   - [ ] Cambiar schedule a unos minutos desde ahora (ej. en 2 minutos)
   - [ ] O ejecutar endpoint de admin manualmente (si existe)

4. **Esperar y verificar email:**
   - [ ] Buscar en mailbox del propietario de campaña
   - [ ] Email debe contener:
     - [ ] Asunto: "Tu reporte de publicidad — OKLA"
     - [ ] Período: "Hoy" o fecha del día anterior
     - [ ] Métricas: Vistas, Clicks, CTR, Presupuesto gastado
     - [ ] Tabla con campañas activas
     - [ ] Link a portal: "Ver reporte completo"

5. **Verificar contenido del email (HTML):**
   - [ ] Header con logo OKLA
   - [ ] Greeting personalizado: "¡Hola {ownerName}!"
   - [ ] 3 métrica boxes: Vistas | Clicks | CTR%
   - [ ] Tabla de campañas con foto del vehículo
   - [ ] Footer estándar

6. **Verificar que solo llega a owners con campañas:**
   - [ ] Owner SIN campañas activas: NO recibe email
   - [ ] Owner CON campaña activa: SÍ recibe email
   - [ ] Owner CON varias campañas: Email lista todas

7. **Verificar horario exacto:**
   - [ ] Cambiar sistema a zona horaria RD
   - [ ] Esperar a las 8:00 AM RD
   - [ ] Email debe llegar en ±5 minutos (exactitud razonable)

8. **Test de resiliencia:**
   - [ ] Detener NotificationService (simular fallo)
   - [ ] Ejecutar job
   - [ ] Email no llega (o se retry)
   - [ ] Cuando NotificationService se levanta, email se envía

**Criterio de Éxito:**

- ✅ Job se ejecuta a las 8:00 AM RD
- ✅ Email llega a owners con campañas
- ✅ Contenido está correcto y formateado
- ✅ Solo owners con campañas activas reciben email

---

### ✅ **CRITERIO #14: Dealer puede ver dashboard en `/dealer/publicidad` con métricas reales**

**Descripción:** Un dealer (cuenta Dealer, no Individual) puede ver un dashboard de publicidad con sus métricas de campañas activas.

**Procedimiento de Validación:**

1. **Login como dealer:**
   - [ ] Usuario logeado con role "Dealer"

2. **Navegar a dashboard:**

   ```bash
   # URL: https://okla.com.do/dealer/publicidad
   ```

3. **Verificar página carga:**
   - [ ] Página visible sin error 404
   - [ ] Muestra sección "Mi Publicidad" o similar

4. **Verificar métricas principales:**
   - [ ] Total de campañas activas (número)
   - [ ] Total de vehículos con campaña (número)
   - [ ] Impresiones totales (hoy/semana/mes)
   - [ ] Clicks totales
   - [ ] CTR promedio (%)
   - [ ] Presupuesto gastado (RD$)
   - [ ] Presupuesto restante (RD$)

5. **Verificar gráficas:**
   - [ ] Gráfica de impresiones por día (últimos 30 días)
   - [ ] Gráfica de clicks por día
   - [ ] Ambas deben tener datos reales (no hardcodeado)

6. **Verificar tabla de campañas:**
   - [ ] Lista las campañas del dealer
   - [ ] Columnas: Vehículo, Estado, Impresiones, Clicks, Presupuesto, Acciones
   - [ ] Botón "Ver detalles" o "Editar"

7. **Verificar acceso:**
   - [ ] Solo el dealer propietario puede ver SUS campañas
   - [ ] Otro dealer NO puede ver campañas de este dealer
   - [ ] Vendedor (Individual) NO tiene acceso a esta página (403 Forbidden)

8. **Test de datos en tiempo real:**
   - [ ] Con dealer logeado, otro usuario hace click en vehículo del dealer
   - [ ] Refrescar dashboard — Clicks debe aumentar en 1 ó 2
   - [ ] Confirma que datos son en tiempo real

**Criterio de Éxito:**

- ✅ Dashboard accesible por dealers
- ✅ Métricas reales sincronizadas con BD
- ✅ Gráficas muestran datos correctos
- ✅ Acceso restringido (solo dealer propietario)

---

### ✅ **CRITERIO #15: DI container resuelve todos los servicios (test `Application_DI_Container_Resolves_All_Services` pasa)**

**Descripción:** Existe un test de integración que valida que el DI container de AdvertisingService puede resolver todos los servicios sin errores.

**Procedimiento de Validación:**

1. **Localizar el test:**

   ```bash
   find /Users/gregorymoreno/Developer/Web/Backend/cardealer-microservices/backend/AdvertisingService \
     -name "*StartupTests.cs" -o -name "*DITests.cs"

   # Esperado: archivo como:
   # AdvertisingService.Tests/DI/StartupTests.cs
   # O: AdvertisingService.Tests/Application_DI_Container_Resolves_All_Services.cs
   ```

2. **Verificar que el test existe y NO está skipped:**

   ```csharp
   // El test debe verse así (NO tener [Fact(Skip="...")])
   [Fact]
   public async Task Application_DI_Container_Resolves_All_Services()
   {
       // Implementación
   }
   ```

3. **Ejecutar el test:**

   ```bash
   cd /Users/gregorymoreno/Developer/Web/Backend/cardealer-microservices/backend/AdvertisingService

   dotnet test --filter "StartupTests"
   # O:
   dotnet test --filter "Application_DI_Container_Resolves_All_Services"

   # Resultado esperado:
   # ✅ 1 passed, 0 failed, 0 skipped
   ```

4. **Si el test falla:**

   ```
   Indicar error específico:
   - "Unable to resolve service for type 'IXxx'" → Agregar al DI container
   - Otros errores → Investigar y corregir
   ```

5. **Ejecutar test completo de DI:**

   ```bash
   dotnet test AdvertisingService.sln --filter "DI or Startup"

   # Todos los tests deben pasar
   ```

**Criterio de Éxito:**

- ✅ Test existe y NO está skipped
- ✅ Test pasa al ejecutar: `dotnet test --filter "Application_DI_Container_Resolves_All_Services"`
- ✅ Resultado: 1 passed, 0 failed

---

### ✅ **CRITERIO #16: Health checks `/health`, `/health/ready`, `/health/live` responden sin bloquear**

**Descripción:** Los 3 endpoints de health check responden correctamente sin timeout o bloqueos.

**Procedimiento de Validación:**

1. **Health check `/health` (overview):**

   ```bash
   # Desde fuera del cluster (via Gateway)
   curl -s https://okla.com.do/api/advertising/health | jq .

   # Desde dentro del cluster
   kubectl exec -it deployment/advertisingservice -n okla -- \
     curl -s http://localhost:8080/health

   # Resultado esperado: 200 OK
   # {"status": "Healthy", "checks": {"database": "Healthy", "redis": "Healthy", ...}}
   ```

2. **Health check `/health/ready` (dependency check):**

   ```bash
   curl -s http://localhost:8080/health/ready

   # Resultado esperado: 200 OK si todas las dependencias están listas
   # {"status": "Healthy", "checks": {"database": "ready", "rabbitmq": "ready"}}
   ```

3. **Health check `/health/live` (liveness, solo proceso):**

   ```bash
   curl -s http://localhost:8080/health/live

   # Resultado esperado: 200 OK siempre que el proceso esté vivo
   # NO ejecuta checks de dependencias
   # {"status": "Healthy"} (simple)
   ```

4. **Medir tiempo de respuesta:**

   ```bash
   time curl -s http://localhost:8080/health > /dev/null

   # Esperado:
   # - /health: < 2 segundos
   # - /health/ready: < 5 segundos
   # - /health/live: < 100ms (muy rápido)
   ```

5. **Test de bloqueo:**
   - [ ] Detener PostgreSQL — `/health` debe fallar, pero NO bloquearse
   - [ ] Detener Redis — `/health` debe fallar, pero NO bloquearse
   - [ ] Simular timeout de una dependencia — respuesta debe ser rápida (<2s)

6. **Verificar K8s usa los endpoints correctamente:**

   ```bash
   kubectl get deployment advertisingservice -n okla -o yaml | grep -A 10 "livenessProbe\|readinessProbe"

   # Esperado:
   # livenessProbe: httpGet path=/health/live
   # readinessProbe: httpGet path=/health/ready
   ```

7. **Simular Kubernetes restart:**
   - [ ] Verificar que pods NO entran en CrashLoopBackOff
   - [ ] Ver logs: `kubectl logs -f deployment/advertisingservice -n okla`

**Criterio de Éxito:**

- ✅ `/health` responde en < 2s
- ✅ `/health/ready` responde en < 5s
- ✅ `/health/live` responde en < 100ms
- ✅ Ninguno bloquea al cluster

---

### ✅ **CRITERIO #17: Servicio está en CI/CD y se construye/despliega**

**Descripción:** El AdvertisingService debe estar incluido en el workflow CI/CD (`smart-cicd.yml`) y construirse/desplegarse automáticamente.

**Procedimiento de Validación:**

1. **Verificar en archivo CI/CD:**

   ```bash
   cat .github/workflows/smart-cicd.yml | grep -i "advertisingservice"

   # Esperado: "advertisingservice" debe estar en la lista de SERVICES
   ```

2. **Verificar Dockerfile:**

   ```bash
   ls -la backend/AdvertisingService/Dockerfile

   # Debe existir y ser válido
   file backend/AdvertisingService/Dockerfile
   ```

3. **Simular build:**

   ```bash
   # Hacer commit y push a rama develop o feature
   git add -A && git commit -m "test: verify ci/cd"
   git push origin develop

   # Ir a GitHub → Actions → Ver workflow ejecutándose
   # Buscar "AdvertisingService" en los logs
   ```

4. **Verificar imagen Docker en GHCR:**

   ```bash
   # Después del build exitoso, debe existir imagen
   docker pull ghcr.io/gregorymorenoiem/advertisingservice:latest

   # Resultado esperado: imagen descargada correctamente
   ```

5. **Verificar deploy automático:**

   ```bash
   # Si workflow tiene deploy automático
   kubectl get deployment -n okla advertisingservice

   # Esperado: deployment existe y está actualizado
   ```

6. **Ver logs del workflow:**
   ```bash
   # En GitHub Actions → Click en workflow run
   # Buscar:
   # ✅ Build AdvertisingService — SUCCESS
   # ✅ Push to GHCR — SUCCESS
   # ✅ Deploy to DOKS — SUCCESS (si aplica)
   ```

**Criterio de Éxito:**

- ✅ Archivo CI/CD contiene "advertisingservice"
- ✅ Dockerfile existe y valida
- ✅ Build exitoso en GitHub Actions
- ✅ Imagen disponible en GHCR
- ✅ Deploy automático funciona

---

### ✅ **CRITERIO #18: Endpoints públicos de tracking tienen rate limiting**

**Descripción:** Los endpoints `/api/advertising/tracking/*` deben tener rate limiting para proteger contra abuso.

**Procedimiento de Validación:**

1. **Identificar endpoints de tracking:**

   ```bash
   # En código del servicio, buscar:
   # - POST /api/advertising/tracking/impression
   # - POST /api/advertising/tracking/click
   # - GET /api/advertising/tracking/...
   ```

2. **Verificar rate limiting configurado:**

   ```bash
   # Ver configuración en appsettings.json o código
   cat backend/AdvertisingService/AdvertisingService.Api/appsettings.json | grep -i "ratelimit"

   # Esperado: sección de rate limiting
   # Ejemplo: 100 requests por minuto por IP/usuario
   ```

3. **Test rate limiting manualmente:**

   ```bash
   # Hacer 150 requests rápidamente al endpoint
   for i in {1..150}; do
     curl -s -X POST https://okla.com.do/api/advertising/tracking/impression \
       -H "Content-Type: application/json" \
       -d '{"campaignId": "test", "userId": "test"}' \
       -w "Status: %{http_code}\n"
   done

   # Primeros ~100: 200 OK
   # Resto: 429 Too Many Requests
   ```

4. **Verificar que rate limiting es por IP:**

   ```bash
   # Hacer requests desde múltiples IPs (o simular)
   # Cada IP debe tener su propio límite de 100 req/min
   ```

5. **Verificar que no bloquea IPs internas:**
   - [ ] Requests desde dentro del cluster (microservicios) NO están rate-limited
   - [ ] Solo clientes externos (browser) están limitados

6. **Verificar headers de rate limiting:**

   ```bash
   curl -i https://okla.com.do/api/advertising/tracking/impression | grep -i "x-ratelimit"

   # Esperado headers:
   # X-RateLimit-Limit: 100
   # X-RateLimit-Remaining: 99
   # X-RateLimit-Reset: <unix_timestamp>
   ```

**Criterio de Éxito:**

- ✅ Rate limiting configurado en endpoints de tracking
- ✅ Límite aplicado correctamente (~100 req/min)
- ✅ Respuesta 429 después del límite
- ✅ Headers informativos en respuesta

---

### ✅ **CRITERIO #19: Si AdvertisingService está caído, homepage degrada con fallback**

**Descripción:** Si el AdvertisingService está caído/inaccesible, el homepage debe mostrar datos hardcodeados como fallback sin error.

**Procedimiento de Validación:**

1. **Simular AdvertisingService caído:**

   ```bash
   # Opción A: Detener el pod
   kubectl scale deployment/advertisingservice --replicas=0 -n okla

   # Opción B: Cambiar servicio a una URL inválida (simular timeout)
   kubectl set env deployment/advertisingservice \
     ADVERTISING_SERVICE_URL="http://localhost:9999" -n okla
   ```

2. **Acceder a homepage:**

   ```bash
   # Ir a https://okla.com.do
   # O: http://localhost:3000 (local)
   ```

3. **Verificar degradación elegante:**
   - [ ] Página carga completamente (no error 500)
   - [ ] Sección "Destacados" visible con vehículos fallback
   - [ ] Carrusel de marcas visible (con datos hardcodeados)
   - [ ] 6 categorías visibles (con gradientes)
   - [ ] No hay mensaje de error visible al usuario
   - [ ] Console de browser NO muestra errores críticos

4. **Verificar en DevTools:**

   ```javascript
   // En console, verificar estado:
   console.log("Is fallback?", window.__USING_FALLBACK__ === true);
   // Esperado: true
   ```

5. **Verificar logs del frontend:**
   - [ ] Debe haber log: "AdvertisingService unavailable, using fallback"
   - [ ] No debe bloquearse en error (retry automático)

6. **Recuperación automática:**
   - [ ] Levantar AdvertisingService nuevamente
   - [ ] Refrescar homepage
   - [ ] Datos en vivo deben reaparecer
   - [ ] Fallback debe desaparecer

7. **Test completo de resiliencia:**

   ```bash
   # Ciclo: On → Off → On
   kubectl scale deployment/advertisingservice --replicas=1 -n okla
   sleep 30 && # Esperar a que se levante

   # Verificar: data en vivo nuevamente
   curl -s https://okla.com.do/api/advertising/rotation/homepage | jq .
   ```

**Criterio de Éxito:**

- ✅ Homepage carga sin error cuando servicio está caído
- ✅ Fallback data es visible
- ✅ Sin mensajes de error visible al usuario
- ✅ Recuperación automática cuando servicio vuelve

---

## Plan de Pruebas QA Detallado

### Fase 1: Pre-Validación (4 horas)

| Tarea              | Responsable | Duración | Descripción                             |
| ------------------ | ----------- | -------- | --------------------------------------- |
| Verificar ambiente | DevOps      | 30min    | Cluster, servicios, DB, Redis, RabbitMQ |
| Build backend      | Backend QA  | 1h       | `dotnet build` — 0 errores              |
| Tests unitarios    | Backend QA  | 1h       | `dotnet test` — todos pasan             |
| Build frontend     | Frontend QA | 30min    | `pnpm build` — 0 errores                |
| TypeScript check   | Frontend QA | 30min    | `tsc --noEmit` — 0 errores              |

**Criterio de Éxito:** ✅ Todos los builds y tests pasen

### Fase 2: Validación Funcional (16 horas)

| Criterio               | Responsable | Duración | Prioridad |
| ---------------------- | ----------- | -------- | --------- |
| #1 Destacados          | Frontend QA | 1h       | CRÍTICA   |
| #2 Marcas              | Frontend QA | 1h       | CRÍTICA   |
| #3 Categorías          | Frontend QA | 1h       | CRÍTICA   |
| #4 Seller Boost        | Seller QA   | 1.5h     | CRÍTICA   |
| #5 Activación RabbitMQ | Backend QA  | 1h       | CRÍTICA   |
| #6 Impresiones + Redis | Backend QA  | 1h       | CRÍTICA   |
| #7 Clicks + Budget     | Backend QA  | 1h       | CRÍTICA   |
| #8 Rotación automática | Backend QA  | 1.5h     | ALTA      |
| #9 Config algoritmo    | Admin QA    | 1h       | ALTA      |
| #10 Validación pesos   | Backend QA  | 1h       | ALTA      |
| #11 Subir categoría    | Admin QA    | 1h       | MEDIA     |
| #12 Subir marca        | Admin QA    | 1h       | MEDIA     |
| #13 Email diario       | Backend QA  | 2h       | MEDIA     |
| #14 Dealer dashboard   | Dealer QA   | 1.5h     | MEDIA     |
| #15 DI test            | Backend QA  | 30min    | CRÍTICA   |
| #16 Health checks      | DevOps QA   | 1h       | ALTA      |
| #17 CI/CD              | DevOps QA   | 1h       | ALTA      |
| #18 Rate limiting      | Security QA | 1h       | MEDIA     |
| #19 Fallback           | Frontend QA | 1h       | CRÍTICA   |

**Total:** ~24 horas de QA testing

### Fase 3: Validación de Integración (8 horas)

- Test end-to-end de flujo completo: Seller → Crear campaña → Pagar → Aparecer en homepage
- Test de load/stress (opcional): 100 usuarios simultáneos en homepage
- Test de recuperación ante fallos
- Test de seguridad básica (SQL injection, XSS en tracking)

### Fase 4: Documentación de Resultados (2 horas)

- Completar reporte de validación
- Documentar defectos encontrados
- Validación final de criterios

---

## Validación Técnica Backend

### Checklist de Construcción

```bash
cd /Users/gregorymoreno/Developer/Web/Backend/cardealer-microservices/backend/AdvertisingService

# ✅ 1. Compilar sin errores
dotnet build AdvertisingService.sln
# Esperado: "Build succeeded" (0 errors)

# ✅ 2. Tests unitarios
dotnet test AdvertisingService.sln
# Esperado: All tests passed

# ✅ 3. Linting / Code analysis
dotnet tool install -g dotnet-codeanalysis
dotnet codeanalysis AdvertisingService.sln
# Esperado: 0 severe issues

# ✅ 4. Docker build
docker build -f Dockerfile -t advertisingservice:test .
# Esperado: Successfully built

# ✅ 5. Ver imágenes
docker images | grep advertisingservice
```

### Verificaciones de Runtime

```bash
# ✅ 1. Iniciar en Docker
docker run -p 8080:8080 advertisingservice:test

# ✅ 2. Verificar logs (sin errores)
# Esperado: "AdvertisingService started on port 8080"

# ✅ 3. Health check
curl -s http://localhost:8080/health
# Esperado: 200 OK, {"status": "Healthy"}

# ✅ 4. Swagger docs
curl -s http://localhost:8080/swagger/v1/swagger.json | jq '.paths' | wc -l
# Esperado: > 10 (múltiples endpoints)
```

---

## Validación Técnica Frontend

### Checklist de Construcción

```bash
cd /Users/gregorymoreno/Developer/Web/Backend/cardealer-microservices/frontend/web-next

# ✅ 1. Install dependencies
pnpm install
# Esperado: 0 errors

# ✅ 2. TypeScript check
pnpm exec tsc --noEmit
# Esperado: 0 errors

# ✅ 3. Linting
pnpm lint
# Esperado: 0 errors (o solo warnings)

# ✅ 4. Build
pnpm build
# Esperado: "successfully generated"

# ✅ 5. Test
pnpm test
# Esperado: All tests passed
```

### Verificaciones de Runtime

```bash
# ✅ 1. Iniciar dev server
pnpm dev

# ✅ 2. Abrir navegador
open http://localhost:3000

# ✅ 3. Verificar homepage sin errores
# Ver console (F12 → Console)
# Esperado: 0 errors, 0 warnings

# ✅ 4. Realizar 5 navegaciones
# - Homepage
# - Buscar
# - Detalle vehículo
# - Mi cuenta
# - Mis vehículos
# Esperado: Sin errores de navegación
```

---

## Validación de Integración

### Test End-to-End: Completo Flujo

**Escenario:** Vendedor destaca vehículo → Paga → Aparece en homepage

```
[Vendedor Login] ──→ [Ir a /mis-vehiculos] ──→ [Seleccionar vehículo]
        ↓
[Click "Boost"] ──→ [/mis-vehiculos/{id}/boost] ──→ [Seleccionar plan]
        ↓
[Click "Pagar"] ──→ [Checkout] ──→ [Pagar con mock Azul]
        ↓
[Simulación] ──→ [RabbitMQ: billing.payment.completed] ──→ [AdvertisingService consume]
        ↓
[Campaña activa] ──→ [Siguiente refresh] ──→ [Homepage muestra "Destacados"]
        ↓
[Vendedor ve vehículo] ──→ [Click en vehículo] ──→ [Click registrado]
        ↓
[Budget decrementado] ──→ [Reporte email a las 8 AM] ──→ [Email con métricas]
```

**Verificación en cada paso:**

1. [ ] Transición exitosa a siguiente paso
2. [ ] Datos correctos en BD
3. [ ] No hay errores en logs
4. [ ] Métricas actualizadas en tiempo real

---

## Procedimiento de Escalación de Errores

### Si falla un criterio:

1. **Documentar defecto:**

   ```
   Criterio #X: [Nombre]
   Descripción: [Qué pasó]
   Paso: [En qué paso falló]
   Esperado: [Qué debería ocurrir]
   Actual: [Qué pasó en lugar de eso]
   Logs: [Stderr/Stdout relevante]
   Reproducción: [Pasos exactos]
   Severidad: CRÍTICA / ALTA / MEDIA / BAJA
   ```

2. **Asignar a desarrollo:**
   - Severidad CRÍTICA → Fix inmediato
   - Severidad ALTA → Fix dentro de 2 horas
   - Severidad MEDIA/BAJA → Fix antes de siguiente release

3. **Re-test después de fix:**
   - Verificar que el criterio ahora pasa
   - Verificar que no rompió otros criterios
   - Ejecutar full test suite nuevamente

4. **Bloquea producción si:**
   - Cualquier criterio #1-7, #15, #16, #17, #19 falla
   - Si falla, NO SE AUTORIZA DEPLOY

---

## Reporte Final de Validación

### Formato de reporte a compartir:

```markdown
# ✅ REPORTE DE VALIDACIÓN QA — AdvertisingService

**Fecha de Validación:** Febrero 20, 2026
**Ambiente:** Staging (DOKS)
**Testeador:** [Nombre]
**Duración:** 48 horas
**Resultado Final:** ✅ APROBADO / ❌ RECHAZADO

---

## Resumen Ejecutivo

- **Total Criterios:** 19
- **Aprobados:** 19 ✅
- **Rechazados:** 0 ❌
- **En Progreso:** 0 ⏳

---

## Criterios Individuales

| #   | Criterio            | Estado | Notas                                |
| --- | ------------------- | ------ | ------------------------------------ |
| 1   | Destacados          | ✅     | Datos en vivo + fallback funcionando |
| 2   | Carrusel marcas     | ✅     | 12 marcas, scroll infinito           |
| 3   | Categorías          | ✅     | 6 categorías con gradientes          |
| 4   | Seller boost        | ✅     | Flujo completo funciona              |
| 5   | Activación RabbitMQ | ✅     | Campaña activada automáticamente     |
| 6   | Impresiones         | ✅     | Deduplicado en Redis                 |
| 7   | Clicks + Budget     | ✅     | Budget deducido correctamente        |
| 8   | Rotación automática | ✅     | Job se ejecuta cada N minutos        |
| 9   | Config algoritmo    | ✅     | Admin puede cambiar                  |
| 10  | Validación pesos    | ✅     | Suma validada 100% ± 1%              |
| 11  | Subir categoría     | ✅     | Imágenes guardadas                   |
| 12  | Subir marca         | ✅     | Logos guardados                      |
| 13  | Email diario        | ✅     | Llega a las 8:00 AM RD               |
| 14  | Dealer dashboard    | ✅     | Métricas reales sincronizadas        |
| 15  | DI test             | ✅     | 42/42 tests pasando                  |
| 16  | Health checks       | ✅     | < 2s, sin bloqueos                   |
| 17  | CI/CD               | ✅     | Build + deploy automático            |
| 18  | Rate limiting       | ✅     | 100 req/min, 429 después             |
| 19  | Fallback            | ✅     | Degrada elegantemente                |

---

## Defectos Encontrados

**Total:** 0 ❌

(Si hubiera defectos, listarlos aquí con severidad)

---

## Conclusión

✅ **APROBADO PARA DEPLOY A PRODUCCIÓN**

Todos los 19 criterios de aceptación han sido validados exitosamente en ambiente staging. El sistema está listo para deploy a producción.

**Autorizado por:** [QA Lead]
**Fecha:** Febrero 20, 2026
```

---

## Checklist de Deploy

Solo después de que el reporte final tenga **19/19 APROBADOS**:

- [ ] Todos los tests pasando (backend + frontend)
- [ ] Compilación sin errores
- [ ] Reporte QA aprobado (19/19)
- [ ] Backend review hecho
- [ ] Frontend review hecho
- [ ] Seguridad review hecho
- [ ] Performance metrics aceptables
- [ ] Logs y monitoring configurados
- [ ] Rollback plan documentado
- [ ] Equipo de soporte notificado
- [ ] Mensaje de release preparado

**Autorizado para deploy:** ✅ Solo si TODOS los checkboxes están marcados

---

_Prompt de Validación QA — Sistema de Publicidad OKLA_
_Febrero 20, 2026_
