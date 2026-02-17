# Tutorial 11: Zero-Downtime Deployments

**Duración:** 50 minutos  
**Nivel:** Avanzado  
**Requisitos:** Tutorial 10 completado

---

## 📋 Resumen

Aprenderás a realizar deployments sin downtime utilizando rolling updates, readiness/liveness probes, y estrategias de deployment avanzadas. Entenderás cómo Kubernetes maneja actualizaciones de aplicaciones sin interrumpir el servicio.

### Lo que aprenderás:
- Estrategias de deployment (Rolling, Recreate, Blue-Green)
- Readiness y Liveness probes
- Rolling update parameters (maxSurge, maxUnavailable)
- Rollback de deployments
- Pre-stop hooks para graceful shutdown
- Health checks en producción

---

## 📑 Índice

1. [Estrategias de deployment](#1-estrategias-de-deployment)
2. [Rolling Updates explicado](#2-rolling-updates-explicado)
3. [Readiness Probe](#3-readiness-probe)
4. [Liveness Probe](#4-liveness-probe)
5. [Configurar probes](#5-configurar-probes)
6. [Rolling update parameters](#6-rolling-update-parameters)
7. [Rollback de deployments](#7-rollback-de-deployments)
8. [Graceful shutdown](#8-graceful-shutdown)
9. [Cheat Sheet](#9-cheat-sheet)
10. [Ejercicios prácticos](#10-ejercicios-prácticos)

---

## 1. Estrategias de deployment

### Rolling Update (Default)

Actualiza pods **gradualmente**, uno por uno (o en batches).

**Ventaja:** Zero-downtime.

**Flujo:**
```
Estado inicial:
Pod-1 (v1) ✅
Pod-2 (v1) ✅
Pod-3 (v1) ✅

Paso 1: Crear nuevo pod
Pod-1 (v1) ✅
Pod-2 (v1) ✅
Pod-3 (v1) ✅
Pod-4 (v2) 🔄 (Starting)

Paso 2: Terminar pod viejo
Pod-2 (v1) ✅
Pod-3 (v1) ✅
Pod-4 (v2) ✅
Pod-1 (v1) ❌ (Terminating)

...repetir hasta que todos sean v2
```

**Kubernetes garantiza:**
- Siempre hay pods disponibles
- Los nuevos pods pasan health checks antes de recibir tráfico
- Los viejos pods se terminan gracefully

### Recreate

Termina **todos** los pods viejos, luego crea los nuevos.

**Ventaja:** Deployment más simple.  
**Desventaja:** **Downtime garantizado**.

**Flujo:**
```
Estado inicial:
Pod-1 (v1) ✅
Pod-2 (v1) ✅

Paso 1: Terminar todos
Pod-1 (v1) ❌
Pod-2 (v1) ❌
⚠️ SERVICIO NO DISPONIBLE

Paso 2: Crear todos nuevos
Pod-1 (v2) ✅
Pod-2 (v2) ✅
✅ Servicio disponible de nuevo
```

**Cuándo usar:**
- Aplicaciones que no pueden tener múltiples versiones corriendo simultáneamente
- Migraciones de esquema de DB complejas
- Ambientes de dev/staging

### Blue-Green

Mantiene **dos ambientes** completos: Blue (actual) y Green (nuevo).

**Flujo:**
```
Blue (v1) - En producción
┌───────────┐
│ Pod-1 (v1) │
│ Pod-2 (v1) │
└───────────┘
      ↑
   Service (apunta a Blue)

Green (v2) - Nuevo
┌───────────┐
│ Pod-3 (v2) │  ← Deploy y test
│ Pod-4 (v2) │
└───────────┘

Switch: Service ahora apunta a Green
      ↓
Green (v2) - En producción
Service apunta aquí

Blue (v1) - En standby (rollback rápido)
```

**Ventaja:**
- Rollback instantáneo (cambiar label selector)
- Testing completo antes del switch

**Desventaja:**
- Requiere 2x recursos durante el deployment

### Canary

Similar a Blue-Green, pero con **tráfico gradual**.

```
v1 (90% tráfico)
├── Pod-1 (v1)
├── Pod-2 (v1)
├── Pod-3 (v1)

v2 (10% tráfico)
└── Pod-4 (v2)  ← Canary

Si v2 está OK:
  Incrementar a 50%, luego 100%
```

**Ventaja:** Minimiza impacto de bugs.

---

## 2. Rolling Updates explicado

### Comando para aplicar rolling update

```bash
# Método 1: kubectl set image
kubectl set image deployment/vehiclessaleservice \
  vehiclessaleservice=ghcr.io/gregorymorenoiem/cardealer-vehiclessaleservice:v2 \
  -n okla

# Método 2: kubectl apply (preferido)
kubectl apply -f k8s/deployments.yaml -n okla
```

### Ver progreso del rollout

```bash
kubectl rollout status deployment/vehiclessaleservice -n okla
```

**Output:**
```
Waiting for deployment "vehiclessaleservice" rollout to finish: 1 out of 3 new replicas have been updated...
Waiting for deployment "vehiclessaleservice" rollout to finish: 1 old replicas are pending termination...
deployment "vehiclessaleservice" successfully rolled out
```

### Ver historial de rollouts

```bash
kubectl rollout history deployment/vehiclessaleservice -n okla
```

**Output:**
```
REVISION  CHANGE-CAUSE
1         <none>
2         kubectl set image deployment/vehiclessaleservice vehiclessaleservice=v2
3         kubectl apply -f deployments.yaml
```

### Pausar un rollout

```bash
kubectl rollout pause deployment/vehiclessaleservice -n okla
```

**Uso típico:** Pausar a mitad de rollout para verificar métricas.

### Reanudar rollout

```bash
kubectl rollout resume deployment/vehiclessaleservice -n okla
```

---

## 3. Readiness Probe

### ¿Qué es?

**Readiness Probe** determina si un pod está **listo para recibir tráfico**.

**Comportamiento:**
- Si falla → Kubernetes **remueve el pod del Service**
- No recibe tráfico hasta que pase el probe
- El pod NO se reinicia

**Uso típico:**
- Aplicación se está iniciando (loading config, conectando a DB)
- Aplicación está temporalmente sobrecargada
- Dependencia externa no disponible

### Tipos de Readiness Probes

#### HTTP GET

```yaml
readinessProbe:
  httpGet:
    path: /health
    port: 8080
  initialDelaySeconds: 10
  periodSeconds: 5
  failureThreshold: 3
```

**Comportamiento:**
- Kubernetes hace `GET http://pod-ip:8080/health`
- Si retorna `200-399` → Healthy
- Si retorna `400+` o timeout → Unhealthy

#### TCP Socket

```yaml
readinessProbe:
  tcpSocket:
    port: 8080
  initialDelaySeconds: 5
  periodSeconds: 5
```

**Comportamiento:**
- Kubernetes intenta conectar TCP al puerto
- Si conecta → Healthy
- Si no conecta → Unhealthy

**Uso típico:** Bases de datos (PostgreSQL, Redis).

#### Exec Command

```yaml
readinessProbe:
  exec:
    command:
    - cat
    - /tmp/healthy
  initialDelaySeconds: 5
  periodSeconds: 5
```

**Comportamiento:**
- Kubernetes ejecuta el comando dentro del container
- Exit code 0 → Healthy
- Exit code != 0 → Unhealthy

### Configuración recomendada

```yaml
readinessProbe:
  httpGet:
    path: /health
    port: 8080
  initialDelaySeconds: 10   # Espera 10s antes del primer probe
  periodSeconds: 5          # Probe cada 5 segundos
  timeoutSeconds: 3         # Timeout de 3 segundos
  successThreshold: 1       # 1 success = Healthy
  failureThreshold: 3       # 3 failures = Unhealthy
```

### Ejemplo en OKLA

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: vehiclessaleservice
  namespace: okla
spec:
  replicas: 2
  template:
    spec:
      containers:
      - name: vehiclessaleservice
        image: ghcr.io/gregorymorenoiem/cardealer-vehiclessaleservice:latest
        ports:
        - containerPort: 8080
        readinessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 15
          periodSeconds: 10
          failureThreshold: 3
```

### Verificar readiness

```bash
kubectl get pods -n okla -o wide
```

**Output:**
```
NAME                                   READY   STATUS    RESTARTS   AGE
vehiclessaleservice-7f8d6b9c5-abcde   1/1     Running   0          2m
```

`READY 1/1` → **Readiness probe pasando**.

`READY 0/1` → **Readiness probe fallando** (pod no recibe tráfico).

---

## 4. Liveness Probe

### ¿Qué es?

**Liveness Probe** determina si un pod está **vivo**.

**Comportamiento:**
- Si falla → Kubernetes **reinicia el pod**
- No afecta el Service (el pod sigue sin recibir tráfico si readiness falla)

**Uso típico:**
- Aplicación en deadlock
- Aplicación consumiendo memoria infinitamente
- Aplicación no responde a requests (pero el proceso sigue corriendo)

### Diferencia con Readiness

| Probe | Falla → Acción | Uso |
|-------|----------------|-----|
| **Readiness** | Remueve del Service | "¿Está listo para tráfico?" |
| **Liveness** | Reinicia el pod | "¿Está vivo?" |

**Ejemplo:**
- Readiness falla: App conectando a DB (temporal, no reiniciar)
- Liveness falla: App en deadlock (reiniciar)

### Configuración

```yaml
livenessProbe:
  httpGet:
    path: /health
    port: 8080
  initialDelaySeconds: 30   # Espera más que readiness
  periodSeconds: 10
  timeoutSeconds: 5
  failureThreshold: 3       # 3 failures = Reinicia
```

**⚠️ IMPORTANTE:**
- `initialDelaySeconds` debe ser **mayor** que el tiempo de startup de la app
- Si es muy agresivo, Kubernetes reiniciará pods sanos

### Ejemplo completo (OKLA)

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: vehiclessaleservice
  namespace: okla
spec:
  replicas: 2
  template:
    spec:
      containers:
      - name: vehiclessaleservice
        image: ghcr.io/gregorymorenoiem/cardealer-vehiclessaleservice:latest
        ports:
        - containerPort: 8080
        readinessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 15
          periodSeconds: 10
          failureThreshold: 3
        livenessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 30
          periodSeconds: 15
          failureThreshold: 3
```

---

## 5. Configurar probes

### Endpoint /health en .NET

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// ...

var app = builder.Build();

// Health check endpoint
app.MapGet("/health", () => Results.Ok("Healthy"));

app.Run();
```

**Respuesta esperada:**
```
GET /health
200 OK
"Healthy"
```

### Health check avanzado

```csharp
// Program.cs
using Microsoft.Extensions.Diagnostics.HealthChecks;

builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString, name: "postgres")
    .AddRedis(redisConnectionString, name: "redis");

var app = builder.Build();

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = _ => true,  // Todas las dependencias
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false  // Sin dependencias (solo app viva)
});
```

**Endpoints:**
- `/health/ready` → Readiness probe (verifica DB, Redis)
- `/health/live` → Liveness probe (solo verifica app)

### Deployment con health checks avanzados

```yaml
readinessProbe:
  httpGet:
    path: /health/ready  # Verifica dependencias
    port: 8080
  initialDelaySeconds: 15
  periodSeconds: 10

livenessProbe:
  httpGet:
    path: /health/live   # Solo verifica app
    port: 8080
  initialDelaySeconds: 30
  periodSeconds: 15
```

---

## 6. Rolling update parameters

### maxSurge y maxUnavailable

Control fino del rolling update.

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: vehiclessaleservice
  namespace: okla
spec:
  replicas: 4
  strategy:
    type: RollingUpdate
    rollingUpdate:
      maxSurge: 1         # Máximo 1 pod extra durante update
      maxUnavailable: 1   # Máximo 1 pod puede estar no disponible
  template:
    spec:
      containers:
      - name: vehiclessaleservice
        image: ghcr.io/gregorymorenoiem/cardealer-vehiclessaleservice:v2
```

### maxSurge

**Definición:** Máximo número de pods **adicionales** durante el update.

**Ejemplo con 4 réplicas:**

```
maxSurge: 1

Estado inicial: 4 pods (v1)
Pod-1, Pod-2, Pod-3, Pod-4

Durante update: 5 pods (4 + maxSurge)
Pod-1 (v1), Pod-2 (v1), Pod-3 (v1), Pod-4 (v1), Pod-5 (v2)
                                                    ↑ extra

Luego: Termina Pod-1 (v1)
Pod-2 (v1), Pod-3 (v1), Pod-4 (v1), Pod-5 (v2)

Crea Pod-6 (v2)
Pod-2 (v1), Pod-3 (v1), Pod-4 (v1), Pod-5 (v2), Pod-6 (v2)
                                                    ↑ extra

...hasta que todos sean v2
```

**Valores típicos:**
- `maxSurge: 1` → Conservador (1 pod extra)
- `maxSurge: 25%` → Balanceado (25% de réplicas como extra)
- `maxSurge: 100%` → Agresivo (doble de pods temporalmente)

### maxUnavailable

**Definición:** Máximo número de pods que pueden estar **no disponibles** durante el update.

**Ejemplo con 4 réplicas:**

```
maxUnavailable: 1

Estado inicial: 4 pods (v1)
Pod-1, Pod-2, Pod-3, Pod-4

Durante update: Mínimo 3 disponibles (4 - maxUnavailable)
Pod-2 (v1), Pod-3 (v1), Pod-4 (v1), Pod-5 (v2)
Pod-1 (v1) ❌ Terminating

Garantía: Siempre ≥ 3 pods disponibles
```

**Valores típicos:**
- `maxUnavailable: 0` → Súper conservador (siempre todas las réplicas)
- `maxUnavailable: 1` → Balanceado
- `maxUnavailable: 25%` → Agresivo

### Combinaciones comunes

#### Conservador (zero-downtime garantizado)

```yaml
strategy:
  rollingUpdate:
    maxSurge: 1
    maxUnavailable: 0
```

**Comportamiento:**
- Crea 1 pod nuevo
- Espera que pase readiness probe
- Termina 1 pod viejo
- Repite

**Ventaja:** Siempre hay capacidad completa.  
**Desventaja:** Más lento, usa más recursos temporalmente.

#### Balanceado

```yaml
strategy:
  rollingUpdate:
    maxSurge: 25%
    maxUnavailable: 25%
```

**Comportamiento (4 réplicas):**
- Puede haber hasta 5 pods (4 + 1)
- Puede haber hasta 3 pods no disponibles (4 - 1)

**Ventaja:** Balance entre velocidad y disponibilidad.

#### Agresivo (más rápido)

```yaml
strategy:
  rollingUpdate:
    maxSurge: 100%
    maxUnavailable: 0
```

**Comportamiento (4 réplicas):**
- Crea 4 pods nuevos (doble capacidad temporalmente)
- Cuando todos estén Ready, termina los 4 viejos

**Ventaja:** Update muy rápido.  
**Desventaja:** Usa el doble de recursos temporalmente.

---

## 7. Rollback de deployments

### Ver historial de rollouts

```bash
kubectl rollout history deployment/vehiclessaleservice -n okla
```

**Output:**
```
REVISION  CHANGE-CAUSE
1         Initial deployment
2         Update to v1.2.0
3         Update to v1.3.0 (current)
```

### Ver detalles de una revisión

```bash
kubectl rollout history deployment/vehiclessaleservice -n okla --revision=2
```

**Output:**
```
deployment.apps/vehiclessaleservice with revision #2
Pod Template:
  Labels:       app=vehiclessaleservice
  Containers:
   vehiclessaleservice:
    Image:      ghcr.io/gregorymorenoiem/cardealer-vehiclessaleservice:v1.2.0
    ...
```

### Rollback a la revisión anterior

```bash
kubectl rollout undo deployment/vehiclessaleservice -n okla
```

**Comportamiento:**
- Revierte a la revisión anterior (REVISION 2 en el ejemplo)
- Usa rolling update (zero-downtime)

### Rollback a una revisión específica

```bash
kubectl rollout undo deployment/vehiclessaleservice -n okla --to-revision=1
```

### Verificar rollback

```bash
kubectl rollout status deployment/vehiclessaleservice -n okla
```

**Output:**
```
deployment "vehiclessaleservice" successfully rolled out
```

```bash
kubectl get pods -n okla -o wide | grep vehiclessaleservice
```

Verifica que la imagen es la anterior.

### Agregar CHANGE-CAUSE

```bash
# Método 1: Anotar manualmente
kubectl annotate deployment/vehiclessaleservice -n okla \
  kubernetes.io/change-cause="Update to v1.3.0 - Fix critical bug"

# Método 2: Usar --record (deprecated pero útil)
kubectl apply -f deployments.yaml -n okla --record
```

**Resultado:**
```
REVISION  CHANGE-CAUSE
1         Initial deployment
2         Update to v1.2.0
3         Update to v1.3.0 - Fix critical bug
```

---

## 8. Graceful shutdown

### ¿Qué es?

**Graceful shutdown** permite que la app termine requests en progreso antes de apagarse.

**Sin graceful shutdown:**
```
Kubernetes: "Pod, termínate"
Pod: ❌ Termina inmediatamente
Requests en progreso: ❌ Error 502
```

**Con graceful shutdown:**
```
Kubernetes: "Pod, termínate en 30 segundos"
Pod: ✅ Termina requests en progreso
      ✅ Cierra conexiones DB
      ✅ Flush logs
      ✅ Termina gracefully
```

### terminationGracePeriodSeconds

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: vehiclessaleservice
  namespace: okla
spec:
  template:
    spec:
      terminationGracePeriodSeconds: 30  # Default
      containers:
      - name: vehiclessaleservice
        image: ghcr.io/gregorymorenoiem/cardealer-vehiclessaleservice:latest
```

**Comportamiento:**
1. Kubernetes envía `SIGTERM` al proceso
2. Espera hasta 30 segundos
3. Si el proceso no terminó, envía `SIGKILL` (forzado)

### PreStop hook

Ejecuta un comando **antes** de enviar `SIGTERM`.

```yaml
lifecycle:
  preStop:
    exec:
      command: ["/bin/sh", "-c", "sleep 5"]
```

**Uso típico:**
- Dar tiempo a que el Service remueva el pod de endpoints
- Evitar "race condition" donde el pod recibe tráfico después de SIGTERM

### Configuración recomendada

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: vehiclessaleservice
  namespace: okla
spec:
  template:
    spec:
      terminationGracePeriodSeconds: 30
      containers:
      - name: vehiclessaleservice
        image: ghcr.io/gregorymorenoiem/cardealer-vehiclessaleservice:latest
        lifecycle:
          preStop:
            exec:
              command: ["/bin/sh", "-c", "sleep 10"]
        readinessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 15
          periodSeconds: 10
```

**Flujo de shutdown:**
1. Kubernetes marca pod como Terminating
2. Ejecuta preStop hook (`sleep 10`)
3. Readiness probe falla → Pod removido del Service
4. Envía SIGTERM al proceso
5. Espera hasta 30 segundos
6. Si no terminó, SIGKILL

### Graceful shutdown en .NET

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

// Configurar shutdown timeout
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();

lifetime.ApplicationStopping.Register(() =>
{
    Console.WriteLine("Application is stopping...");
    // Flush logs, cerrar conexiones, etc.
});

lifetime.ApplicationStopped.Register(() =>
{
    Console.WriteLine("Application stopped.");
});

app.Run();
```

**ASP.NET Core maneja SIGTERM automáticamente:**
- Para de aceptar nuevos requests
- Completa requests en progreso
- Cierra conexiones

---

## 9. Cheat Sheet

### Rolling Update

```bash
# Aplicar deployment (rolling update automático)
kubectl apply -f deployments.yaml -n okla

# Ver progreso
kubectl rollout status deployment/vehiclessaleservice -n okla

# Pausar rollout
kubectl rollout pause deployment/vehiclessaleservice -n okla

# Reanudar rollout
kubectl rollout resume deployment/vehiclessaleservice -n okla

# Ver historial
kubectl rollout history deployment/vehiclessaleservice -n okla

# Rollback a anterior
kubectl rollout undo deployment/vehiclessaleservice -n okla

# Rollback a revisión específica
kubectl rollout undo deployment/vehiclessaleservice -n okla --to-revision=2
```

### Probes

```yaml
# Readiness Probe (¿Listo para tráfico?)
readinessProbe:
  httpGet:
    path: /health/ready
    port: 8080
  initialDelaySeconds: 15
  periodSeconds: 10
  failureThreshold: 3

# Liveness Probe (¿Está vivo?)
livenessProbe:
  httpGet:
    path: /health/live
    port: 8080
  initialDelaySeconds: 30
  periodSeconds: 15
  failureThreshold: 3
```

### Rolling Update Strategy

```yaml
strategy:
  type: RollingUpdate
  rollingUpdate:
    maxSurge: 1        # Pods extra
    maxUnavailable: 0  # Pods no disponibles
```

### Graceful Shutdown

```yaml
terminationGracePeriodSeconds: 30
containers:
- name: app
  lifecycle:
    preStop:
      exec:
        command: ["/bin/sh", "-c", "sleep 10"]
```

---

## 10. Ejercicios prácticos

### Ejercicio 1: Ver estrategia de deployment

1. Ve la configuración de strategy de `vehiclessaleservice`
2. Identifica el tipo de strategy
3. Identifica maxSurge y maxUnavailable

<details>
<summary>Solución</summary>

```bash
# 1
kubectl get deployment vehiclessaleservice -n okla -o yaml | grep -A10 "strategy"

# 2 y 3 - Output esperado:
# strategy:
#   type: RollingUpdate
#   rollingUpdate:
#     maxSurge: 1
#     maxUnavailable: 1
```
</details>

### Ejercicio 2: Verificar probes

1. Ve la configuración de probes de `gateway`
2. Identifica el path del readiness probe
3. Identifica el initialDelaySeconds

<details>
<summary>Solución</summary>

```bash
# 1
kubectl get deployment gateway -n okla -o yaml | grep -A10 "readinessProbe"

# 2 y 3 aparecerán en el output
```
</details>

### Ejercicio 3: Ver historial de rollouts

1. Ve el historial de rollouts de `vehiclessaleservice`
2. Cuenta cuántas revisiones hay

<details>
<summary>Solución</summary>

```bash
# 1
kubectl rollout history deployment/vehiclessaleservice -n okla

# 2
# Cuenta las líneas (excluyendo header)
```
</details>

### Ejercicio 4: Simular rolling update

1. Anota el deployment con un cambio
2. Ve el progreso del rollout
3. Verifica que completó

<details>
<summary>Solución</summary>

```bash
# 1
kubectl annotate deployment/vehiclessaleservice -n okla \
  kubernetes.io/change-cause="Test rolling update"

# 2
kubectl rollout status deployment/vehiclessaleservice -n okla

# 3
kubectl rollout history deployment/vehiclessaleservice -n okla
# Debe aparecer una nueva revisión
```
</details>

### Ejercicio 5: Probar graceful shutdown

1. Ve el terminationGracePeriodSeconds de `gateway`
2. Elimina un pod del gateway
3. Observa cuánto tarda en terminar

<details>
<summary>Solución</summary>

```bash
# 1
kubectl get deployment gateway -n okla -o yaml | grep terminationGracePeriodSeconds

# 2
POD=$(kubectl get pods -n okla -l app=gateway -o name | head -1)
kubectl delete $POD -n okla

# 3
kubectl get pods -n okla -l app=gateway -w
# Observa el estado "Terminating"
# Debe tardar aproximadamente el tiempo del terminationGracePeriodSeconds
```
</details>

---

## 🎉 ¡Felicidades!

Has completado el Tutorial 11. Ahora sabes:
- ✅ Estrategias de deployment (Rolling, Recreate, Blue-Green)
- ✅ Diferencia entre Readiness y Liveness probes
- ✅ Configurar probes en .NET
- ✅ Controlar rolling updates con maxSurge/maxUnavailable
- ✅ Hacer rollback de deployments
- ✅ Implementar graceful shutdown
- ✅ Realizar deployments sin downtime

---

**Anterior:** [10 - Troubleshooting 404/503](./10-troubleshooting-errors.md)  
**Siguiente:** [12 - GitHub Actions Setup](./12-github-actions-setup.md)
