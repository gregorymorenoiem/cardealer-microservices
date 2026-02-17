# Tutorial 2: Gestión de Pods y Deployments

**Duración:** 45 minutos  
**Nivel:** Principiante  
**Requisitos:** Tutorial 1 completado

---

## 📋 Resumen

Aprenderás a gestionar el ciclo de vida de pods y deployments en Kubernetes. Desde crear y escalar deployments, hasta reiniciar pods y manejar actualizaciones con rollout.

### Lo que aprenderás:
- Diferencia entre Pod, ReplicaSet y Deployment
- Crear y eliminar pods
- Escalar deployments (scale up/down)
- Reiniciar deployments sin downtime
- Ver historial de rollouts
- Hacer rollback a versiones anteriores

---

## 📑 Índice

1. [Conceptos: Pod vs ReplicaSet vs Deployment](#1-conceptos-pod-vs-replicaset-vs-deployment)
2. [Ver pods en ejecución](#2-ver-pods-en-ejecución)
3. [Eliminar pods (y auto-recreación)](#3-eliminar-pods-y-auto-recreación)
4. [Escalar deployments](#4-escalar-deployments)
5. [Reiniciar deployments](#5-reiniciar-deployments)
6. [Historial de rollouts](#6-historial-de-rollouts)
7. [Rollback a versión anterior](#7-rollback-a-versión-anterior)
8. [Estados de pods y troubleshooting](#8-estados-de-pods-y-troubleshooting)
9. [Cheat Sheet](#9-cheat-sheet)
10. [Ejercicios prácticos](#10-ejercicios-prácticos)

---

## 1. Conceptos: Pod vs ReplicaSet vs Deployment

### ¿Por qué tres conceptos diferentes?

Kubernetes tiene una arquitectura en capas. Cada capa añade funcionalidad sobre la anterior:

```
Deployment (más alto nivel)
    ↓
ReplicaSet (gestión de réplicas)
    ↓
Pod (contenedor corriendo)
```

### 🔷 Pod

**Definición:** La unidad más pequeña en Kubernetes. Uno o más contenedores que comparten red y almacenamiento.

```yaml
apiVersion: v1
kind: Pod
metadata:
  name: nginx-pod
spec:
  containers:
  - name: nginx
    image: nginx:latest
    ports:
    - containerPort: 80
```

**Características:**
- ✅ Corre contenedores Docker
- ❌ Si el pod muere, NO se recrea automáticamente
- ❌ No puede escalarse (crear más copias)
- ❌ No puede actualizarse sin downtime

**¿Cuándo usarlo?** Casi nunca en producción. Solo para pruebas rápidas.

### 🔷 ReplicaSet

**Definición:** Garantiza que un número específico de réplicas de un pod estén corriendo.

```yaml
apiVersion: apps/v1
kind: ReplicaSet
metadata:
  name: nginx-rs
spec:
  replicas: 3
  selector:
    matchLabels:
      app: nginx
  template:
    metadata:
      labels:
        app: nginx
    spec:
      containers:
      - name: nginx
        image: nginx:latest
```

**Características:**
- ✅ Auto-recrea pods si mueren
- ✅ Puede escalarse (3 → 5 réplicas)
- ❌ No gestiona actualizaciones (rolling updates)
- ❌ No tiene historial de versiones

**¿Cuándo usarlo?** Casi nunca directamente. Los Deployments lo usan internamente.

### 🔷 Deployment (LO MÁS USADO)

**Definición:** Gestiona ReplicaSets y proporciona actualizaciones declarativas para pods.

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: nginx-deployment
spec:
  replicas: 3
  selector:
    matchLabels:
      app: nginx
  template:
    metadata:
      labels:
        app: nginx
    spec:
      containers:
      - name: nginx
        image: nginx:1.21
```

**Características:**
- ✅ Todo lo de ReplicaSet
- ✅ Rolling updates (actualiza sin downtime)
- ✅ Rollback a versiones anteriores
- ✅ Historial de despliegues
- ✅ Control de estrategia de actualización

**¿Cuándo usarlo?** SIEMPRE en producción para aplicaciones stateless.

### Relación en el proyecto OKLA

```bash
kubectl get deployments,rs,pods -n okla -l app=gateway
```

**Salida:**
```
NAME                      READY   UP-TO-DATE   AVAILABLE
deployment.apps/gateway   1/1     1            1

NAME                                 DESIRED   CURRENT   READY
replicaset.apps/gateway-77d6c65764   1         1         1

NAME                           READY   STATUS
pod/gateway-77d6c65764-w2mmr   1/1     Running
```

**Lo que vemos:**
1. **Deployment** `gateway` - Configurado para 1 réplica
2. **ReplicaSet** `gateway-77d6c65764` - Creado automáticamente por el Deployment
3. **Pod** `gateway-77d6c65764-w2mmr` - Creado automáticamente por el ReplicaSet

---

## 2. Ver pods en ejecución

### Listar todos los pods del proyecto

```bash
kubectl get pods -n okla
```

**Salida:**
```
NAME                                   READY   STATUS    RESTARTS   AGE
authservice-b5747bbcf-6qf2x            1/1     Running   0          6h
billingservice-d9496cc54-5nnpv         1/1     Running   0          6h
errorservice-c7d4b5f9c-8kl2m           1/1     Running   0          6h
frontend-web-585bd859d8-tn5v4          1/1     Running   0          6h
gateway-77d6c65764-w2mmr               1/1     Running   0          2h
mediaservice-6b8c9d7f5e-4jhk9          1/1     Running   0          6h
notificationservice-8f7e6d5c4b-2nhj3   1/1     Running   0          6h
postgres-0                             1/1     Running   0          6h
rabbitmq-0                             1/1     Running   0          6h
redis-6d9f8c5b4a-9plk6                 1/1     Running   0          6h
roleservice-5c8d7e6f4b-7mkl4           1/1     Running   0          6h
userservice-7f9e8d6c5b-3njk5           1/1     Running   0          6h
vehiclessaleservice-9g8f7e6d5c-4mhj6   1/1     Running   0          6h
```

### Ver pods con más detalle

```bash
kubectl get pods -n okla -o wide
```

**Columnas adicionales:**
- **IP** - Dirección IP interna del pod
- **NODE** - Nodo (servidor) donde corre
- **NOMINATED NODE** - Nodo propuesto para scheduling
- **READINESS GATES** - Checks adicionales de readiness

### Ver pods de un deployment específico

```bash
# Usando label selector
kubectl get pods -n okla -l app=gateway

# Usando grep
kubectl get pods -n okla | grep gateway
```

### Ver pods ordenados por edad

```bash
kubectl get pods -n okla --sort-by=.metadata.creationTimestamp
```

### Watch mode (actualización en tiempo real)

```bash
kubectl get pods -n okla -w
```

**¿Para qué sirve?** Ver en tiempo real cómo se crean, reinician o eliminan pods. Presiona Ctrl+C para salir.

---

## 3. Eliminar pods (y auto-recreación)

### ¿Qué pasa cuando eliminas un pod?

Si el pod es gestionado por un **Deployment** o **ReplicaSet**, Kubernetes lo recrea automáticamente para mantener el número de réplicas configurado.

### Eliminar un pod específico

```bash
# Ver el pod actual del gateway
kubectl get pods -n okla -l app=gateway

# Copiar el nombre del pod y eliminarlo
kubectl delete pod gateway-77d6c65764-w2mmr -n okla
```

**Salida:**
```
pod "gateway-77d6c65764-w2mmr" deleted
```

### Verificar auto-recreación

```bash
# Ver pods inmediatamente después
kubectl get pods -n okla -l app=gateway
```

**Salida:**
```
NAME                       READY   STATUS    RESTARTS   AGE
gateway-77d6c65764-k8n5p   1/1     Running   0          5s
```

**¿Qué pasó?**
1. Eliminaste el pod `gateway-xxx-w2mmr`
2. El ReplicaSet detectó que faltan réplicas (tiene 0, necesita 1)
3. Creó un nuevo pod `gateway-xxx-k8n5p`

**El hash permanece igual** (`77d6c65764`) porque el ReplicaSet no cambió.

### Forzar eliminación (no recomendado)

```bash
# Eliminar con --force y --grace-period=0
kubectl delete pod gateway-77d6c65764-w2mmr -n okla --force --grace-period=0
```

**¿Cuándo usar esto?** Solo si un pod está "stuck" en estado Terminating.

### Eliminar todos los pods de un deployment

```bash
# Esto eliminará todos pero se recrearán inmediatamente
kubectl delete pods -n okla -l app=gateway
```

**Nota:** Esto NO escala a cero. Para eso necesitas escalar el Deployment.

---

## 4. Escalar deployments

### ¿Por qué escalar?

- **Scale up** (aumentar réplicas) - Más tráfico, más capacidad
- **Scale down** (reducir réplicas) - Ahorrar recursos
- **Scale to zero** - Desactivar temporalmente sin eliminar

### Ver réplicas actuales

```bash
kubectl get deployment gateway -n okla
```

**Salida:**
```
NAME      READY   UP-TO-DATE   AVAILABLE   AGE
gateway   1/1     1            1           6h
```

**READY 1/1** significa:
- 1 réplica lista / 1 réplica deseada

### Escalar a 3 réplicas (scale up)

```bash
kubectl scale deployment gateway --replicas=3 -n okla
```

**Salida:**
```
deployment.apps/gateway scaled
```

### Verificar el escalado

```bash
kubectl get pods -n okla -l app=gateway -w
```

**Verás en tiempo real:**
```
NAME                       READY   STATUS              AGE
gateway-77d6c65764-w2mmr   1/1     Running             6h
gateway-77d6c65764-x7k2n   0/1     ContainerCreating   2s
gateway-77d6c65764-m9p4r   0/1     ContainerCreating   2s

... (unos segundos después)

NAME                       READY   STATUS    AGE
gateway-77d6c65764-w2mmr   1/1     Running   6h
gateway-77d6c65764-x7k2n   1/1     Running   15s
gateway-77d6c65764-m9p4r   1/1     Running   15s
```

### Ver el deployment escalado

```bash
kubectl get deployment gateway -n okla
```

**Salida:**
```
NAME      READY   UP-TO-DATE   AVAILABLE   AGE
gateway   3/3     3            3           6h
```

### Escalar de vuelta a 1 réplica (scale down)

```bash
kubectl scale deployment gateway --replicas=1 -n okla
```

**¿Qué pods se eliminan?** Kubernetes elimina los pods más nuevos primero.

### Escalar a cero (desactivar temporalmente)

```bash
kubectl scale deployment gateway --replicas=0 -n okla
```

**¿Para qué sirve?**
- Mantenimiento
- Debugging sin tráfico
- Ahorrar recursos temporalmente

**¿Cómo reactivar?**
```bash
kubectl scale deployment gateway --replicas=1 -n okla
```

### Escalar múltiples deployments

```bash
# Escalar todos los servicios backend a 2 réplicas
kubectl scale deployment -n okla --replicas=2 \
  authservice userservice vehiclessaleservice
```

---

## 5. Reiniciar deployments

### ¿Por qué reiniciar?

- Has actualizado un ConfigMap y necesitas recargar la configuración
- Hay un bug y quieres "refrescar" el servicio
- Necesitas aplicar cambios de variables de entorno

### Reiniciar un deployment (rolling restart)

```bash
kubectl rollout restart deployment gateway -n okla
```

**Salida:**
```
deployment.apps/gateway restarted
```

**¿Qué hace esto?**
1. Crea nuevos pods con la misma imagen
2. Espera a que estén listos (healthy)
3. Termina los pods viejos
4. **Resultado:** CERO DOWNTIME

### Ver el proceso en tiempo real

```bash
# En una terminal, watch del deployment
kubectl get deployment gateway -n okla -w

# En otra terminal, watch de los pods
kubectl get pods -n okla -l app=gateway -w
```

**Verás:**
```
# Pods viejos
gateway-77d6c65764-w2mmr   1/1   Running   6h

# Nuevo ReplicaSet creado
gateway-8k4m9f7e6d-p3n7x   0/1   ContainerCreating   1s

# Nuevo pod listo
gateway-8k4m9f7e6d-p3n7x   1/1   Running   10s

# Pod viejo terminando
gateway-77d6c65764-w2mmr   1/1   Terminating   6h
```

### Reiniciar múltiples deployments

```bash
kubectl rollout restart deployment -n okla \
  gateway authservice userservice
```

### Reiniciar TODOS los deployments del namespace

```bash
kubectl rollout restart deployment -n okla
```

**⚠️ Cuidado:** Esto reiniciará TODOS los servicios, incluyendo bases de datos si están como Deployment (en nuestro caso PostgreSQL es StatefulSet, así que está seguro).

---

## 6. Historial de rollouts

### ¿Por qué guardar historial?

Kubernetes guarda las últimas revisiones de cada deployment. Esto permite:
- Ver qué cambió en cada despliegue
- Hacer rollback a versiones anteriores

### Ver historial de un deployment

```bash
kubectl rollout history deployment gateway -n okla
```

**Salida:**
```
deployment.apps/gateway
REVISION  CHANGE-CAUSE
1         <none>
2         <none>
3         kubectl rollout restart deployment gateway
```

**REVISION** - Número secuencial de cada cambio.

### Ver detalles de una revisión específica

```bash
kubectl rollout history deployment gateway -n okla --revision=3
```

**Salida:**
```
deployment.apps/gateway with revision #3
Pod Template:
  Labels:	app=gateway
  Containers:
   gateway:
    Image:	ghcr.io/gregorymorenoiem/cardealer-gateway:latest
    Port:	8080/TCP
    Environment:
      ASPNETCORE_ENVIRONMENT:	Production
      RABBITMQ_HOST:	rabbitmq
    Mounts:	<none>
  Volumes:	<none>
```

**Verás:**
- Imagen usada
- Variables de entorno
- Configuración del contenedor

### Añadir anotaciones al historial

```bash
# Al hacer cambios, añade --record (deprecado pero útil)
kubectl set image deployment/gateway gateway=ghcr.io/.../gateway:v2.0.0 -n okla \
  --record
```

**Mejor práctica moderna:** Usar `kubernetes.io/change-cause` annotation:

```bash
kubectl annotate deployment gateway -n okla \
  kubernetes.io/change-cause="Updated to version 2.0.0"
```

---

## 7. Rollback a versión anterior

### ¿Cuándo hacer rollback?

- Desplegaste una versión con bugs
- La nueva versión tiene problemas de performance
- Necesitas volver a una versión estable rápidamente

### Rollback a la revisión anterior (undo)

```bash
kubectl rollout undo deployment gateway -n okla
```

**Salida:**
```
deployment.apps/gateway rolled back
```

**¿Qué pasó?**
1. Kubernetes identificó la revisión anterior (ej: revisión 2)
2. Creó un nuevo ReplicaSet con esa configuración
3. Hizo rolling update para volver a esa versión

### Verificar el rollback

```bash
# Ver el historial
kubectl rollout history deployment gateway -n okla

# Ver los pods (nuevo hash)
kubectl get pods -n okla -l app=gateway
```

### Rollback a una revisión específica

```bash
# Volver a la revisión 1
kubectl rollout undo deployment gateway -n okla --to-revision=1
```

### Ver el estado del rollout

```bash
kubectl rollout status deployment gateway -n okla
```

**Salida:**
```
deployment "gateway" successfully rolled out
```

O si aún está en progreso:
```
Waiting for deployment "gateway" rollout to finish: 1 out of 3 new replicas have been updated...
```

---

## 8. Estados de pods y troubleshooting

### Estados comunes de pods

| Estado | Significado | Acción |
|--------|-------------|--------|
| **Pending** | Esperando ser asignado a un nodo | Verificar recursos disponibles |
| **ContainerCreating** | Descargando imagen, iniciando | Normal, esperar unos segundos |
| **Running** | Pod funcionando correctamente | ✅ Todo bien |
| **Succeeded** | Completó su tarea (Jobs) | ✅ Normal para Jobs |
| **Failed** | Error al iniciar o ejecutar | Ver logs |
| **CrashLoopBackOff** | El contenedor inicia y crashea repetidamente | Ver logs |
| **ImagePullBackOff** | No puede descargar la imagen | Verificar imagen existe |
| **Terminating** | Eliminándose | Esperar o forzar eliminación |

### Troubleshooting: CrashLoopBackOff

```bash
# Ver estado
kubectl get pods -n okla

# Salida:
NAME                       READY   STATUS             RESTARTS   AGE
gateway-xxx                0/1     CrashLoopBackOff   5          10m
```

**¿Qué significa?**
- El contenedor inicia
- Falla inmediatamente
- Kubernetes espera y lo reinicia
- Falla de nuevo
- Espera más tiempo (backoff)
- Ciclo infinito

**Cómo debuggear:**

```bash
# Ver logs del pod actual
kubectl logs gateway-xxx -n okla

# Ver logs del contenedor anterior (antes del crash)
kubectl logs gateway-xxx -n okla --previous

# Describir el pod para ver eventos
kubectl describe pod gateway-xxx -n okla
```

**Causas comunes:**
- Error en el código (NullReferenceException, etc.)
- Falta variable de entorno
- No puede conectar a base de datos
- Puerto ya en uso
- ConfigMap o Secret faltante

### Troubleshooting: ImagePullBackOff

```bash
kubectl get pods -n okla

# Salida:
NAME                       READY   STATUS              RESTARTS   AGE
gateway-xxx                0/1     ImagePullBackOff    0          2m
```

**Cómo debuggear:**

```bash
kubectl describe pod gateway-xxx -n okla
```

**Eventos típicos:**
```
Events:
  Type     Reason     Message
  ----     ------     -------
  Warning  Failed     Failed to pull image "ghcr.io/.../gateway:wrongtag": pull access denied
  Warning  Failed     Error: ErrImagePull
  Normal   BackOff    Back-off pulling image "ghcr.io/.../gateway:wrongtag"
```

**Causas comunes:**
- Tag de imagen incorrecto
- Imagen no existe en el registry
- Falta credenciales para registry privado
- Typo en el nombre de la imagen

**Solución:**

```bash
# Verificar imagen en deployment
kubectl get deployment gateway -n okla -o jsonpath='{.spec.template.spec.containers[0].image}'

# Corregir la imagen
kubectl set image deployment/gateway gateway=ghcr.io/.../gateway:latest -n okla
```

### Troubleshooting: Pending

```bash
kubectl get pods -n okla

# Salida:
NAME                       READY   STATUS    RESTARTS   AGE
gateway-xxx                0/1     Pending   0          5m
```

**Cómo debuggear:**

```bash
kubectl describe pod gateway-xxx -n okla
```

**Eventos típicos:**
```
Events:
  Type     Reason            Message
  ----     ------            -------
  Warning  FailedScheduling  0/3 nodes are available: insufficient memory
```

**Causas comunes:**
- No hay recursos suficientes (CPU/RAM) en el cluster
- No hay nodos disponibles
- PersistentVolumeClaim no está bound
- Node selector/affinity no cumplido

**Solución:**
- Escalar el cluster (añadir nodos)
- Reducir requests de recursos
- Verificar PVCs

---

## 9. Cheat Sheet

### Comandos de Pods

| Comando | Descripción |
|---------|-------------|
| `kubectl get pods -n okla` | Listar pods |
| `kubectl get pods -n okla -o wide` | Listar con más detalle |
| `kubectl get pods -n okla -w` | Watch mode |
| `kubectl delete pod <nombre> -n okla` | Eliminar pod |
| `kubectl describe pod <nombre> -n okla` | Detalles del pod |

### Comandos de Deployments

| Comando | Descripción |
|---------|-------------|
| `kubectl get deployments -n okla` | Listar deployments |
| `kubectl scale deployment <nombre> --replicas=3 -n okla` | Escalar |
| `kubectl rollout restart deployment <nombre> -n okla` | Reiniciar |
| `kubectl rollout status deployment <nombre> -n okla` | Ver estado |
| `kubectl rollout history deployment <nombre> -n okla` | Ver historial |
| `kubectl rollout undo deployment <nombre> -n okla` | Rollback |

### Comandos de ReplicaSets

| Comando | Descripción |
|---------|-------------|
| `kubectl get rs -n okla` | Listar ReplicaSets |
| `kubectl describe rs <nombre> -n okla` | Detalles del RS |

### Flags útiles

| Flag | Función |
|------|---------|
| `-w` | Watch mode (actualización en tiempo real) |
| `-o wide` | Más columnas |
| `-o yaml` | YAML completo |
| `--previous` | Logs del contenedor anterior |
| `--sort-by` | Ordenar resultados |

---

## 10. Ejercicios prácticos

### Ejercicio 1: Gestión básica de pods

1. Lista todos los pods del namespace `okla`
2. Elimina el pod del servicio `authservice`
3. Verifica que se recreó automáticamente (distinto nombre)

<details>
<summary>Solución</summary>

```bash
# 1
kubectl get pods -n okla

# 2 (copiar el nombre exacto del pod)
kubectl delete pod authservice-b5747bbcf-6qf2x -n okla

# 3
kubectl get pods -n okla -l app=authservice
# Verás un pod nuevo con diferente sufijo aleatorio
```
</details>

### Ejercicio 2: Escalar un deployment

1. Escala el deployment `vehiclessaleservice` a 3 réplicas
2. Verifica que hay 3 pods corriendo
3. Escálalo de vuelta a 1 réplica

<details>
<summary>Solución</summary>

```bash
# 1
kubectl scale deployment vehiclessaleservice --replicas=3 -n okla

# 2
kubectl get pods -n okla -l app=vehiclessaleservice

# 3
kubectl scale deployment vehiclessaleservice --replicas=1 -n okla
```
</details>

### Ejercicio 3: Reiniciar y verificar rollout

1. Reinicia el deployment `gateway`
2. Observa en tiempo real cómo se crean nuevos pods
3. Verifica que el rollout completó exitosamente

<details>
<summary>Solución</summary>

```bash
# 1
kubectl rollout restart deployment gateway -n okla

# 2
kubectl get pods -n okla -l app=gateway -w
# Presiona Ctrl+C para salir

# 3
kubectl rollout status deployment gateway -n okla
```
</details>

### Ejercicio 4: Historial y rollback

1. Ve el historial de rollouts del deployment `userservice`
2. Haz un rollback a la revisión anterior
3. Verifica el nuevo estado

<details>
<summary>Solución</summary>

```bash
# 1
kubectl rollout history deployment userservice -n okla

# 2
kubectl rollout undo deployment userservice -n okla

# 3
kubectl rollout status deployment userservice -n okla
kubectl get pods -n okla -l app=userservice
```
</details>

### Ejercicio 5: Troubleshooting simulado

1. Escala `mediaservice` a 0 réplicas (simular servicio caído)
2. Describe el deployment y nota que READY es 0/0
3. Escálalo de vuelta a 1 y verifica que vuelve a Running

<details>
<summary>Solución</summary>

```bash
# 1
kubectl scale deployment mediaservice --replicas=0 -n okla

# 2
kubectl get deployment mediaservice -n okla
# READY será 0/0
kubectl describe deployment mediaservice -n okla

# 3
kubectl scale deployment mediaservice --replicas=1 -n okla
kubectl get pods -n okla -l app=mediaservice -w
```
</details>

---

## 🎉 ¡Felicidades!

Has completado el Tutorial 2. Ahora sabes:
- ✅ Diferencia entre Pod, ReplicaSet y Deployment
- ✅ Eliminar y ver auto-recreación de pods
- ✅ Escalar deployments (up/down/zero)
- ✅ Reiniciar deployments sin downtime
- ✅ Ver historial y hacer rollback
- ✅ Identificar y troubleshootear estados de pods

---

**Anterior:** [01 - kubectl Básico](./01-kubectl-basico.md)  
**Siguiente:** [03 - ConfigMaps y Secrets](./03-configmaps-secrets.md)
