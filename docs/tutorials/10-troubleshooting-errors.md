# Tutorial 10: Troubleshooting 404/503

**Duración:** 45 minutos  
**Nivel:** Intermedio-Avanzado  
**Requisitos:** Tutorial 9 completado

---

## 📋 Resumen

Aprenderás a diagnosticar y resolver errores HTTP comunes en Kubernetes: 404 Not Found y 503 Service Unavailable. Desarrollarás un proceso sistemático de debugging para identificar la causa raíz y aplicar soluciones efectivas.

### Lo que aprenderás:
- Entender la causa de errores 404 vs 503
- Proceso sistemático de debugging
- Verificar configuración de Gateway (Ocelot)
- Debugging de DNS y conectividad
- Herramientas de troubleshooting
- Casos reales de OKLA y soluciones

---

## 📑 Índice

1. [Tipos de errores HTTP](#1-tipos-de-errores-http)
2. [Error 404: Diagnóstico](#2-error-404-diagnóstico)
3. [Error 404: Soluciones](#3-error-404-soluciones)
4. [Error 503: Diagnóstico](#4-error-503-diagnóstico)
5. [Error 503: Soluciones](#5-error-503-soluciones)
6. [Proceso sistemático](#6-proceso-sistemático)
7. [Herramientas de debugging](#7-herramientas-de-debugging)
8. [Casos reales](#8-casos-reales)
9. [Cheat Sheet](#9-cheat-sheet)
10. [Ejercicios prácticos](#10-ejercicios-prácticos)

---

## 1. Tipos de errores HTTP

### HTTP Status Codes: Resumen

| Código | Nombre | Significado | Ubicación del problema |
|--------|--------|-------------|----------------------|
| **200** | OK | Request exitoso | No hay error |
| **404** | Not Found | **Ruta no encontrada** | Gateway/Ingress |
| **500** | Internal Server Error | Error en servicio | Backend service |
| **502** | Bad Gateway | Gateway no puede conectar | Gateway → Service |
| **503** | Service Unavailable | **Servicio no disponible** | Service/Pods |
| **504** | Gateway Timeout | Timeout esperando respuesta | Service lento |

### 404 vs 503: Diferencia clave

#### 404 Not Found

**Qué significa:**  
El Gateway recibió el request, pero **no tiene una ruta configurada** para ese path.

**Flujo:**
```
Cliente → Gateway → ❌ "No conozco esa ruta"
```

**Causa típica:**
- Ruta no existe en `ocelot.json`
- UpstreamPathTemplate incorrecto
- Typo en la URL

#### 503 Service Unavailable

**Qué significa:**  
El Gateway conoce la ruta, pero **no puede conectar con el servicio backend**.

**Flujo:**
```
Cliente → Gateway → ❌ (intenta conectar a service)
                    ↓
                Servicio no responde
```

**Causa típica:**
- Servicio no existe
- Pods no están Running
- Puerto incorrecto
- Service sin Endpoints

---

## 2. Error 404: Diagnóstico

### Síntoma

```bash
curl https://api.okla.com.do/api/vehicles
# HTTP/1.1 404 Not Found
```

### Checklist de diagnóstico

#### 1. Verificar que el request llegó al Gateway

```bash
kubectl logs -f deployment/gateway -n okla --tail=30
```

**Buscar:**
```
warn: Ocelot.Errors.Middleware.ExceptionHandlerMiddleware[0]
      requestId: ..., previousRequestId: ... message: DownstreamRouteFinderMiddleware setting pipeline errors. IDownstreamRouteProvider returned no routes
```

Si ves este mensaje → **El Gateway no encontró una ruta**.

#### 2. Verificar rutas en ocelot.json

```bash
# Ver todas las rutas
kubectl exec deployment/gateway -n okla -- cat /app/ocelot.json | grep "UpstreamPathTemplate"

# Buscar ruta específica
kubectl exec deployment/gateway -n okla -- cat /app/ocelot.json | grep -i "vehicles"
```

**¿Aparece la ruta?**
- **NO** → La ruta no está configurada
- **SÍ** → Revisar el template

#### 3. Verificar coincidencia del path

```bash
# Ver configuración completa de la ruta
kubectl exec deployment/gateway -n okla -- cat /app/ocelot.json | grep -A15 "vehicles"
```

**Ejemplo de output:**
```json
{
  "UpstreamPathTemplate": "/api/vehicles/{everything}",
  "UpstreamHttpMethod": ["GET", "POST"],
  "DownstreamPathTemplate": "/api/{everything}",
  ...
}
```

**Preguntas:**
- ¿El `UpstreamPathTemplate` coincide con tu request?
- ¿El método HTTP está permitido? (`GET`, `POST`, etc.)

### Casos comunes de 404

#### Caso 1: Template no coincide

```json
// ocelot.json
"UpstreamPathTemplate": "/vehicles/{everything}"

// Request del cliente
GET /api/vehicles  ❌ NO MATCH (falta "api")
```

**Solución:** Cambiar a `/api/vehicles/{everything}`

#### Caso 2: Método no permitido

```json
"UpstreamHttpMethod": ["GET"]
```

```bash
# Cliente hace POST
curl -X POST https://api.okla.com.do/api/vehicles
# 404 Not Found ❌
```

**Solución:** Agregar `"POST"` al array.

#### Caso 3: Typo en la URL

```bash
# Cliente escribe "vehicels" (typo)
curl https://api.okla.com.do/api/vehicels
# 404 Not Found ❌
```

**Solución:** Corregir typo en cliente.

---

## 3. Error 404: Soluciones

### Solución 1: Agregar ruta faltante

Si la ruta no existe en `ocelot.json`:

```bash
# 1. Editar archivo local
nano backend/Gateway/Gateway.Api/ocelot.prod.json
```

**Agregar:**
```json
{
  "UpstreamPathTemplate": "/api/catalog/{everything}",
  "UpstreamHttpMethod": ["GET"],
  "DownstreamPathTemplate": "/api/{everything}",
  "DownstreamScheme": "http",
  "DownstreamHostAndPorts": [
    {
      "Host": "vehiclessaleservice",
      "Port": 8080
    }
  ]
}
```

```bash
# 2. Recrear ConfigMap
kubectl delete configmap gateway-config -n okla
kubectl create configmap gateway-config -n okla \
  --from-file=ocelot.json=backend/Gateway/Gateway.Api/ocelot.prod.json

# 3. Reiniciar Gateway
kubectl rollout restart deployment gateway -n okla

# 4. Esperar rollout
kubectl rollout status deployment gateway -n okla

# 5. Probar
curl https://api.okla.com.do/api/catalog/makes
```

### Solución 2: Corregir UpstreamPathTemplate

Si el template está mal:

```json
// Antes
"UpstreamPathTemplate": "/vehicles/{everything}"

// Después
"UpstreamPathTemplate": "/api/vehicles/{everything}"
```

Luego actualizar ConfigMap y reiniciar Gateway.

### Solución 3: Agregar método HTTP

```json
// Antes
"UpstreamHttpMethod": ["GET"]

// Después
"UpstreamHttpMethod": ["GET", "POST", "PUT", "DELETE"]
```

### Verificación final

```bash
# 1. Ver logs del Gateway (no debe haber errores "DownstreamRouteFinderMiddleware")
kubectl logs deployment/gateway -n okla --tail=20

# 2. Hacer request exitoso
curl -I https://api.okla.com.do/api/vehicles
# HTTP/1.1 200 OK ✅
```

---

## 4. Error 503: Diagnóstico

### Síntoma

```bash
curl https://api.okla.com.do/api/vehicles
# HTTP/1.1 503 Service Unavailable
```

### Checklist de diagnóstico

#### 1. Verificar logs del Gateway

```bash
kubectl logs -f deployment/gateway -n okla --tail=30
```

**Buscar:**
```
fail: Ocelot.Requester.HttpClientHttpRequester[0]
      Error making http request, exception: System.Net.Http.HttpRequestException: Connection refused
```

Si ves `Connection refused` → **El servicio no está respondiendo**.

#### 2. Verificar que el servicio existe

```bash
kubectl get svc -n okla | grep vehiclessaleservice
```

**Output esperado:**
```
vehiclessaleservice   ClusterIP   10.245.x.x   <none>   8080/TCP   5d
```

Si no aparece → **El Service no existe**.

#### 3. Verificar que el Service tiene Endpoints

```bash
kubectl get endpoints vehiclessaleservice -n okla
```

**Output esperado (✅):**
```
NAME                  ENDPOINTS         AGE
vehiclessaleservice   10.244.0.5:8080   5d
```

**Output problemático (❌):**
```
NAME                  ENDPOINTS   AGE
vehiclessaleservice   <none>      5d
```

`<none>` significa **no hay pods detrás del Service**.

#### 4. Verificar que los Pods están Running

```bash
kubectl get pods -n okla | grep vehiclessaleservice
```

**Output esperado:**
```
vehiclessaleservice-7f8d6b9c5-abcde   1/1     Running   0   5d
```

**Output problemático:**
```
vehiclessaleservice-7f8d6b9c5-abcde   0/1     CrashLoopBackOff   10   5d
```

#### 5. Verificar puerto en ocelot.json

```bash
kubectl exec deployment/gateway -n okla -- cat /app/ocelot.json | grep -A5 "vehiclessaleservice"
```

**Debe ser:**
```json
"DownstreamHostAndPorts": [
  {
    "Host": "vehiclessaleservice",
    "Port": 8080  ✅
  }
]
```

**NO debe ser:**
```json
"Port": 80  ❌ INCORRECTO EN KUBERNETES
```

---

## 5. Error 503: Soluciones

### Solución 1: Crear el Service faltante

Si el Service no existe:

```yaml
# service.yaml
apiVersion: v1
kind: Service
metadata:
  name: vehiclessaleservice
  namespace: okla
spec:
  selector:
    app: vehiclessaleservice
  ports:
  - port: 8080
    targetPort: 8080
  type: ClusterIP
```

```bash
kubectl apply -f service.yaml
```

### Solución 2: Corregir selector del Service

Si el Service no tiene Endpoints:

```bash
# 1. Ver selector del Service
kubectl get svc vehiclessaleservice -n okla -o yaml | grep -A3 selector

# Ejemplo:
# selector:
#   app: vehiclessaleservice

# 2. Ver labels de los Pods
kubectl get pods -n okla --show-labels | grep vehiclessale

# 3. Verificar que coinciden
```

**Si no coinciden:**

```yaml
# Editar Service
kubectl edit svc vehiclessaleservice -n okla

# Cambiar selector para que coincida con los labels de los Pods
spec:
  selector:
    app: vehiclessaleservice  # Debe coincidir con label del Pod
```

### Solución 3: Reparar Pods en CrashLoopBackOff

```bash
# 1. Ver logs del pod
kubectl logs vehiclessaleservice-7f8d6b9c5-abcde -n okla --previous

# 2. Ver eventos
kubectl describe pod vehiclessaleservice-7f8d6b9c5-abcde -n okla

# 3. Verificar errores comunes:
# - Image no existe
# - Environment variables faltantes
# - Conexión a DB fallida
```

**Solución típica:** Corregir Deployment y aplicar.

### Solución 4: Corregir puerto en ocelot.json

Si el puerto está incorrecto:

```bash
# 1. Editar archivo local
nano backend/Gateway/Gateway.Api/ocelot.prod.json
```

**Cambiar:**
```json
// Antes
"Port": 80

// Después
"Port": 8080
```

```bash
# 2. Recrear ConfigMap
kubectl delete configmap gateway-config -n okla
kubectl create configmap gateway-config -n okla \
  --from-file=ocelot.json=backend/Gateway/Gateway.Api/ocelot.prod.json

# 3. Reiniciar Gateway
kubectl rollout restart deployment gateway -n okla
```

### Solución 5: Probar conectividad desde Gateway

```bash
# Entrar al pod del Gateway
kubectl exec -it deployment/gateway -n okla -- sh

# Instalar wget
apk add wget

# Probar conectividad
wget -qO- http://vehiclessaleservice:8080/health

# Si responde: ✅ El servicio está OK, problema en configuración
# Si no responde: ❌ El servicio tiene un problema

exit
```

### Verificación final

```bash
# 1. Service existe y tiene Endpoints
kubectl get svc vehiclessaleservice -n okla
kubectl get endpoints vehiclessaleservice -n okla

# 2. Pods están Running
kubectl get pods -n okla | grep vehiclessaleservice

# 3. Request exitoso
curl -I https://api.okla.com.do/api/vehicles
# HTTP/1.1 200 OK ✅
```

---

## 6. Proceso sistemático

### Flowchart de debugging

```
┌─────────────────────────┐
│  Error 404 o 503?       │
└───────┬─────────────────┘
        │
        ├──── 404 ───────────────────────────────┐
        │                                        │
        │  1. Ver logs del Gateway               │
        │  2. Verificar ocelot.json tiene ruta  │
        │  3. Verificar UpstreamPathTemplate    │
        │  4. Verificar método HTTP             │
        │  5. Actualizar ConfigMap si necesario │
        └────────────────────────────────────────┘
        │
        ├──── 503 ───────────────────────────────┐
        │                                        │
        │  1. Verificar Service existe           │
        │  2. Verificar Service tiene Endpoints  │
        │  3. Verificar Pods están Running       │
        │  4. Verificar puerto en ocelot.json    │
        │  5. Probar conectividad desde Gateway  │
        └────────────────────────────────────────┘
```

### Comandos paso a paso

```bash
# === PASO 1: IDENTIFICAR ERROR ===
curl -I https://api.okla.com.do/api/vehicles
# Ver código: 404 o 503

# === PASO 2: VER LOGS ===
kubectl logs deployment/gateway -n okla --tail=50

# === PASO 3A: SI ES 404 ===
kubectl exec deployment/gateway -n okla -- cat /app/ocelot.json | grep -i "vehicles"

# === PASO 3B: SI ES 503 ===
kubectl get svc vehiclessaleservice -n okla
kubectl get endpoints vehiclessaleservice -n okla
kubectl get pods -n okla | grep vehiclessaleservice

# === PASO 4: APLICAR SOLUCIÓN ===
# (Dependiendo del diagnóstico)

# === PASO 5: VERIFICAR ===
curl -I https://api.okla.com.do/api/vehicles
```

---

## 7. Herramientas de debugging

### kubectl logs

```bash
# Logs en tiempo real
kubectl logs -f deployment/gateway -n okla

# Últimas 100 líneas
kubectl logs deployment/gateway -n okla --tail=100

# Logs del contenedor anterior (si crasheó)
kubectl logs pod/gateway-abc123 -n okla --previous

# Logs con timestamp
kubectl logs deployment/gateway -n okla --timestamps
```

### kubectl exec

```bash
# Shell interactivo
kubectl exec -it deployment/gateway -n okla -- sh

# Comando directo
kubectl exec deployment/gateway -n okla -- cat /app/ocelot.json

# Probar conectividad
kubectl exec deployment/gateway -n okla -- wget -qO- http://service:8080/health
```

### kubectl describe

```bash
# Describe pod (ver eventos)
kubectl describe pod vehiclessaleservice-abc123 -n okla

# Describe service
kubectl describe svc vehiclessaleservice -n okla

# Describe deployment
kubectl describe deployment vehiclessaleservice -n okla
```

### kubectl get endpoints

```bash
# Ver endpoints de un servicio
kubectl get endpoints vehiclessaleservice -n okla

# Ver todos los endpoints
kubectl get endpoints -n okla
```

### curl con verbose

```bash
# Ver headers completos
curl -I https://api.okla.com.do/api/vehicles

# Verbose (ver handshake SSL, redirects)
curl -v https://api.okla.com.do/api/vehicles

# Seguir redirects
curl -L https://api.okla.com.do/api/vehicles
```

### nslookup / dig

```bash
# Resolver DNS del servicio
kubectl exec -it deployment/gateway -n okla -- sh
nslookup vehiclessaleservice
# Output: 10.245.x.x (ClusterIP)

# Resolver DNS externo
nslookup api.okla.com.do
# Output: 146.190.199.0 (LoadBalancer)
```

---

## 8. Casos reales

### Caso 1: Homepage sections retorna 404

**Problema:**
```bash
curl https://api.okla.com.do/api/homepagesections/homepage
# 404 Not Found
```

**Diagnóstico:**
```bash
kubectl exec deployment/gateway -n okla -- cat /app/ocelot.json | grep -i "homepage"
# (no output) ❌
```

La ruta no existe en `ocelot.json`.

**Solución:**

Agregar en `backend/Gateway/Gateway.Api/ocelot.prod.json`:

```json
{
  "UpstreamPathTemplate": "/api/homepagesections/{everything}",
  "UpstreamHttpMethod": ["GET"],
  "DownstreamPathTemplate": "/api/{everything}",
  "DownstreamScheme": "http",
  "DownstreamHostAndPorts": [
    {
      "Host": "vehiclessaleservice",
      "Port": 8080
    }
  ]
}
```

```bash
kubectl delete cm gateway-config -n okla
kubectl create cm gateway-config -n okla --from-file=ocelot.json=backend/Gateway/Gateway.Api/ocelot.prod.json
kubectl rollout restart deployment gateway -n okla
```

### Caso 2: VehiclesSaleService retorna 503

**Problema:**
```bash
curl https://api.okla.com.do/api/vehicles
# 503 Service Unavailable
```

**Diagnóstico:**
```bash
kubectl get endpoints vehiclessaleservice -n okla
# ENDPOINTS: <none> ❌
```

El Service no tiene Endpoints.

```bash
kubectl get svc vehiclessaleservice -n okla -o yaml | grep -A3 selector
# selector:
#   app: vehiclessale  ← Typo!

kubectl get pods -n okla --show-labels | grep vehiclessale
# app=vehiclessaleservice  ← Label correcto
```

**Causa:** Selector del Service tiene typo.

**Solución:**

```bash
kubectl edit svc vehiclessaleservice -n okla
```

Cambiar:
```yaml
selector:
  app: vehiclessaleservice  # Corregir typo
```

Guardar y salir.

**Verificar:**
```bash
kubectl get endpoints vehiclessaleservice -n okla
# ENDPOINTS: 10.244.0.5:8080 ✅
```

### Caso 3: Catalog endpoint retorna 503 tras deploy

**Problema:**
```bash
curl https://api.okla.com.do/api/catalog/makes
# 503 Service Unavailable
```

**Diagnóstico:**
```bash
kubectl logs deployment/gateway -n okla --tail=20
```

**Output:**
```
fail: Ocelot.Requester.HttpClientHttpRequester[0]
      Error making http request, exception: System.Net.Http.HttpRequestException: Connection refused (vehiclessaleservice:80)
```

`vehiclessaleservice:80` → **Está usando puerto 80 (incorrecto)**.

**Verificar:**
```bash
kubectl exec deployment/gateway -n okla -- cat /app/ocelot.json | grep -A5 "catalog"
```

**Output:**
```json
"DownstreamHostAndPorts": [
  {
    "Host": "vehiclessaleservice",
    "Port": 80  ❌
  }
]
```

**Solución:**

Editar `backend/Gateway/Gateway.Api/ocelot.prod.json`:

```json
"Port": 8080  ✅
```

```bash
kubectl delete cm gateway-config -n okla
kubectl create cm gateway-config -n okla --from-file=ocelot.json=backend/Gateway/Gateway.Api/ocelot.prod.json
kubectl rollout restart deployment gateway -n okla
```

---

## 9. Cheat Sheet

### Debugging 404

```bash
# 1. Ver logs del Gateway
kubectl logs deployment/gateway -n okla --tail=50 | grep -i "error\|warn"

# 2. Verificar rutas en ocelot.json
kubectl exec deployment/gateway -n okla -- cat /app/ocelot.json | grep "UpstreamPathTemplate"

# 3. Buscar ruta específica
kubectl exec deployment/gateway -n okla -- cat /app/ocelot.json | grep -A15 "vehicles"

# 4. Actualizar ConfigMap
kubectl delete cm gateway-config -n okla
kubectl create cm gateway-config -n okla --from-file=ocelot.json=backend/Gateway/Gateway.Api/ocelot.prod.json
kubectl rollout restart deployment gateway -n okla
```

### Debugging 503

```bash
# 1. Verificar Service
kubectl get svc servicename -n okla

# 2. Verificar Endpoints
kubectl get endpoints servicename -n okla

# 3. Verificar Pods
kubectl get pods -n okla | grep servicename

# 4. Ver logs del pod
kubectl logs deployment/servicename -n okla --tail=50

# 5. Probar conectividad desde Gateway
kubectl exec -it deployment/gateway -n okla -- wget -qO- http://servicename:8080/health

# 6. Verificar puerto en ocelot.json
kubectl exec deployment/gateway -n okla -- cat /app/ocelot.json | grep -A5 "servicename"
```

### Comandos útiles

```bash
# Ver estado completo de un servicio
kubectl get all -n okla -l app=vehiclessaleservice

# Ver eventos recientes
kubectl get events -n okla --sort-by='.lastTimestamp' | tail -20

# Restart rápido del Gateway
kubectl rollout restart deployment gateway -n okla && kubectl rollout status deployment gateway -n okla

# Test completo de API
curl -I https://api.okla.com.do/health && \
curl -I https://api.okla.com.do/api/vehicles && \
curl -I https://api.okla.com.do/api/auth/me
```

---

## 10. Ejercicios prácticos

### Ejercicio 1: Diagnosticar 404

Simula un error 404 accediendo a una ruta inexistente.

1. Intenta acceder a `/api/nonexistent`
2. Ve el código HTTP
3. Ve los logs del Gateway

<details>
<summary>Solución</summary>

```bash
# 1 y 2
curl -I https://api.okla.com.do/api/nonexistent
# HTTP/1.1 404 Not Found

# 3
kubectl logs deployment/gateway -n okla --tail=20
# Buscar "DownstreamRouteFinderMiddleware"
```
</details>

### Ejercicio 2: Verificar Endpoints

1. Lista todos los Services en namespace okla
2. Verifica los Endpoints de `vehiclessaleservice`
3. Cuenta cuántos Endpoints tiene

<details>
<summary>Solución</summary>

```bash
# 1
kubectl get svc -n okla

# 2
kubectl get endpoints vehiclessaleservice -n okla

# 3
# El output muestra las IPs, ejemplo:
# 10.244.0.5:8080,10.244.0.6:8080
# = 2 endpoints (2 réplicas)
```
</details>

### Ejercicio 3: Probar conectividad interna

1. Entra al pod del Gateway
2. Prueba conectividad a `vehiclessaleservice:8080/health`
3. Verifica que retorna 200 OK

<details>
<summary>Solución</summary>

```bash
# 1
kubectl exec -it deployment/gateway -n okla -- sh

# 2 y 3
apk add wget
wget -qO- http://vehiclessaleservice:8080/health
# Output: "Healthy" (o similar)

exit
```
</details>

### Ejercicio 4: Simular cambio de puerto

1. Ve el puerto actual en ocelot.json para `vehiclessaleservice`
2. (NO EJECUTAR, solo visualizar) ¿Qué pasaría si cambias el puerto a 80?

<details>
<summary>Solución</summary>

```bash
# 1
kubectl exec deployment/gateway -n okla -- cat /app/ocelot.json | grep -A5 "vehiclessaleservice"
# Puerto debe ser 8080

# 2
# Si cambias a 80:
# - El Gateway intentará conectar a vehiclessaleservice:80
# - El Service escucha en 8080
# - Error 503 Service Unavailable
```
</details>

### Ejercicio 5: Debug completo

Ejecuta un troubleshooting completo del endpoint `/api/vehicles`.

1. Hacer request y ver código HTTP
2. Ver logs del Gateway
3. Verificar ruta en ocelot.json
4. Verificar Service y Endpoints
5. Verificar Pods
6. Probar conectividad desde Gateway

<details>
<summary>Solución</summary>

```bash
# 1
curl -I https://api.okla.com.do/api/vehicles

# 2
kubectl logs deployment/gateway -n okla --tail=30

# 3
kubectl exec deployment/gateway -n okla -- cat /app/ocelot.json | grep -A15 "vehicles"

# 4
kubectl get svc vehiclessaleservice -n okla
kubectl get endpoints vehiclessaleservice -n okla

# 5
kubectl get pods -n okla | grep vehiclessaleservice

# 6
kubectl exec -it deployment/gateway -n okla -- sh
apk add wget
wget -qO- http://vehiclessaleservice:8080/health
exit
```
</details>

---

## 🎉 ¡Felicidades!

Has completado el Tutorial 10. Ahora sabes:
- ✅ Diferencia entre 404 y 503
- ✅ Diagnosticar errores HTTP en Kubernetes
- ✅ Verificar configuración de Gateway
- ✅ Debugging de Services y Endpoints
- ✅ Proceso sistemático de troubleshooting
- ✅ Herramientas de debugging
- ✅ Resolver casos reales de producción

---

**Anterior:** [09 - Ocelot Gateway](./09-ocelot-gateway.md)  
**Siguiente:** [11 - Zero-downtime Deployments](./11-zero-downtime-deployments.md)
