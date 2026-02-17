# Tutorial 9: Ocelot API Gateway

**Duración:** 50 minutos  
**Nivel:** Avanzado  
**Requisitos:** Tutorial 8 completado

---

## 📋 Resumen

Aprenderás a configurar y gestionar Ocelot como API Gateway en Kubernetes. Entenderás cómo rutear requests, transformar paths, y troubleshootear problemas comunes de configuración.

### Lo que aprenderás:
- Qué es un API Gateway y por qué Ocelot
- Estructura del archivo ocelot.json
- Configurar rutas (Routes) en Ocelot
- Montar configuración desde ConfigMap
- Troubleshooting de rutas 404/503
- Rate limiting y CORS en Gateway

---

## 📑 Índice

1. [Conceptos: API Gateway](#1-conceptos-api-gateway)
2. [¿Por qué Ocelot?](#2-por-qué-ocelot)
3. [Estructura de ocelot.json](#3-estructura-de-ocelotjson)
4. [Configurar rutas](#4-configurar-rutas)
5. [Montar desde ConfigMap](#5-montar-desde-configmap)
6. [Actualizar configuración](#6-actualizar-configuración)
7. [Troubleshooting 404/503](#7-troubleshooting-404503)
8. [Features avanzadas](#8-features-avanzadas)
9. [Cheat Sheet](#9-cheat-sheet)
10. [Ejercicios prácticos](#10-ejercicios-prácticos)

---

## 1. Conceptos: API Gateway

### ¿Qué es un API Gateway?

Un **API Gateway** es un punto de entrada único para todos los microservicios.

```
┌─────────────────────────────────────────────────┐
│              Cliente (Browser/App)              │
└────────────────────┬────────────────────────────┘
                     │
                     ▼
         ┌───────────────────────┐
         │    API Gateway        │
         │   (Ocelot en K8s)     │
         │  api.okla.com.do      │
         └───────┬───────────────┘
                 │
     ┌───────────┼───────────────────┐
     │           │                   │
     ▼           ▼                   ▼
┌─────────┐ ┌────────────┐ ┌──────────────────┐
│  Auth   │ │  User      │ │ VehiclesSale     │
│ Service │ │  Service   │ │    Service       │
└─────────┘ └────────────┘ └──────────────────┘
```

### Ventajas de un API Gateway

| Ventaja | Descripción |
|---------|-------------|
| **Single endpoint** | Un solo dominio para todos los servicios |
| **Ruteo centralizado** | Todas las reglas de ruteo en un lugar |
| **Autenticación** | JWT validation centralizada |
| **Rate limiting** | Control de tráfico por servicio |
| **Load balancing** | Distribuye tráfico entre réplicas |
| **SSL termination** | HTTPS solo en gateway, HTTP internamente |
| **Logging** | Logs centralizados de todos los requests |

### Sin API Gateway (problemático)

```
Cliente → authservice.okla.com.do/login
Cliente → userservice.okla.com.do/profile
Cliente → vehiclessaleservice.okla.com.do/vehicles
```

**Problemas:**
- Múltiples dominios
- Múltiples LoadBalancers ($$$)
- CORS complejo
- No hay punto central de control

### Con API Gateway (OKLA)

```
Cliente → api.okla.com.do/api/auth/login        → authservice
Cliente → api.okla.com.do/api/users/profile     → userservice
Cliente → api.okla.com.do/api/vehicles          → vehiclessaleservice
```

**Ventajas:**
- Un solo dominio
- Un LoadBalancer
- CORS centralizado
- Ruteo simple

---

## 2. ¿Por qué Ocelot?

### Ocelot Overview

**Ocelot** es un API Gateway open-source para .NET.

**Repositorio:** https://github.com/ThreeMammals/Ocelot

### Alternativas

| Gateway | Lenguaje | Características |
|---------|----------|-----------------|
| **Ocelot** | .NET | Simple, ligero, configuración JSON |
| **Kong** | Lua/Go | Enterprise, plugins, admin UI |
| **NGINX** | C | Ultra rápido, reverse proxy |
| **Traefik** | Go | Auto-discovery, dashboard |
| **AWS API Gateway** | Managed | Integrado con AWS |
| **Azure API Management** | Managed | Integrado con Azure |

**En OKLA elegimos Ocelot porque:**
- ✅ Mismo stack que microservicios (.NET 8)
- ✅ Configuración declarativa (ocelot.json)
- ✅ Fácil integración con Kubernetes
- ✅ Soporta features avanzadas (auth, rate limiting)

### Arquitectura de Ocelot en OKLA

```
Internet → LoadBalancer → Ingress (api.okla.com.do)
                             ↓
                        Gateway Pod (Ocelot)
                             ↓
                   ┌─────────┴─────────┐
                   ▼                   ▼
            ocelot.json         Microservicios
         (mounted via ConfigMap)
```

---

## 3. Estructura de ocelot.json

### Archivo básico

```json
{
  "Routes": [
    {
      "UpstreamPathTemplate": "/api/vehicles/{everything}",
      "UpstreamHttpMethod": ["GET", "POST", "PUT", "DELETE"],
      "DownstreamPathTemplate": "/api/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        {
          "Host": "vehiclessaleservice",
          "Port": 8080
        }
      ]
    }
  ],
  "GlobalConfiguration": {
    "BaseUrl": "https://api.okla.com.do"
  }
}
```

### Componentes clave

#### 1. Routes (Rutas)

Array de configuraciones de ruteo.

#### 2. UpstreamPathTemplate

Ruta que el **cliente** usa.

```
UpstreamPathTemplate: "/api/vehicles/{everything}"
```

**Match:**
- `/api/vehicles` ✅
- `/api/vehicles/123` ✅
- `/api/vehicles/search?brand=toyota` ✅

#### 3. DownstreamPathTemplate

Ruta que se envía al **microservicio**.

```
DownstreamPathTemplate: "/api/{everything}"
```

**Transformación:**
```
Cliente:      /api/vehicles/123
Gateway:      /api/123         (remueve "vehicles")
```

**Para mantener el path completo:**
```json
"DownstreamPathTemplate": "/api/vehicles/{everything}"
```

#### 4. DownstreamHostAndPorts

Servicio de destino en Kubernetes.

```json
"DownstreamHostAndPorts": [
  {
    "Host": "vehiclessaleservice",
    "Port": 8080
  }
]
```

**⚠️ IMPORTANTE EN KUBERNETES:** El puerto DEBE ser 8080, NO 80.

#### 5. UpstreamHttpMethod

Métodos HTTP permitidos.

```json
"UpstreamHttpMethod": ["GET", "POST", "PUT", "DELETE"]
```

O para todos los métodos:
```json
"UpstreamHttpMethod": []
```

---

## 4. Configurar rutas

### Ruta completa de ejemplo (OKLA)

```json
{
  "UpstreamPathTemplate": "/api/vehicles/{everything}",
  "UpstreamHttpMethod": ["GET", "POST", "PUT", "DELETE", "PATCH"],
  "DownstreamPathTemplate": "/api/{everything}",
  "DownstreamScheme": "http",
  "DownstreamHostAndPorts": [
    {
      "Host": "vehiclessaleservice",
      "Port": 8080
    }
  ],
  "RateLimitOptions": {
    "ClientWhitelist": [],
    "EnableRateLimiting": true,
    "Period": "1m",
    "PeriodTimespan": 60,
    "Limit": 100
  }
}
```

### Múltiples servicios

```json
{
  "Routes": [
    {
      "UpstreamPathTemplate": "/api/auth/{everything}",
      "DownstreamPathTemplate": "/api/{everything}",
      "DownstreamHostAndPorts": [
        { "Host": "authservice", "Port": 8080 }
      ]
    },
    {
      "UpstreamPathTemplate": "/api/users/{everything}",
      "DownstreamPathTemplate": "/api/{everything}",
      "DownstreamHostAndPorts": [
        { "Host": "userservice", "Port": 8080 }
      ]
    },
    {
      "UpstreamPathTemplate": "/api/vehicles/{everything}",
      "DownstreamPathTemplate": "/api/{everything}",
      "DownstreamHostAndPorts": [
        { "Host": "vehiclessaleservice", "Port": 8080 }
      ]
    }
  ]
}
```

### Rutas específicas vs catch-all

**Específica (recomendada):**
```json
"UpstreamPathTemplate": "/api/vehicles/{everything}"
```

Match: `/api/vehicles/*` solamente.

**Catch-all (no recomendada):**
```json
"UpstreamPathTemplate": "/{everything}"
```

Match: TODO (puede causar conflictos).

### Prioridad de rutas

Ocelot evalúa rutas **en orden de arriba hacia abajo**.

**Problema:**
```json
[
  { "UpstreamPathTemplate": "/{everything}" },  // Match TODO
  { "UpstreamPathTemplate": "/api/vehicles" }   // NUNCA se alcanza
]
```

**Solución:** Rutas más específicas primero:
```json
[
  { "UpstreamPathTemplate": "/api/vehicles/{everything}" },
  { "UpstreamPathTemplate": "/api/auth/{everything}" },
  { "UpstreamPathTemplate": "/{everything}" }  // Fallback
]
```

---

## 5. Montar desde ConfigMap

### Crear ConfigMap con ocelot.json

```bash
kubectl create configmap gateway-config -n okla \
  --from-file=ocelot.json=backend/Gateway/Gateway.Api/ocelot.prod.json
```

**Salida:**
```
configmap/gateway-config created
```

### Ver ConfigMap

```bash
kubectl get cm gateway-config -n okla -o yaml | head -40
```

**Extracto:**
```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: gateway-config
  namespace: okla
data:
  ocelot.json: |
    {
      "Routes": [
        {
          "UpstreamPathTemplate": "/api/vehicles/{everything}",
          ...
        }
      ]
    }
```

### Montar en el Deployment del Gateway

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: gateway
  namespace: okla
spec:
  template:
    spec:
      containers:
      - name: gateway
        image: ghcr.io/gregorymorenoiem/cardealer-gateway:latest
        volumeMounts:
        - name: ocelot-config
          mountPath: /app/ocelot.json
          subPath: ocelot.json  # Solo el archivo, no el directorio
      volumes:
      - name: ocelot-config
        configMap:
          name: gateway-config
```

**¿Qué hace `subPath`?**

Sin `subPath`, Kubernetes reemplaza TODO el directorio `/app` con el ConfigMap.

Con `subPath`, solo reemplaza el archivo `/app/ocelot.json`.

### Verificar montaje

```bash
# Entrar al pod del gateway
kubectl exec -it deployment/gateway -n okla -- sh

# Ver el archivo
cat /app/ocelot.json | head -20

# Buscar una ruta específica
grep "vehicles" /app/ocelot.json

exit
```

---

## 6. Actualizar configuración

### Flujo de actualización

1. Editar archivo local `backend/Gateway/Gateway.Api/ocelot.prod.json`
2. Recrear ConfigMap
3. Reiniciar Gateway

### Paso 1: Editar ocelot.prod.json

```bash
# Abrir con tu editor favorito
code backend/Gateway/Gateway.Api/ocelot.prod.json

# O editar con nano
nano backend/Gateway/Gateway.Api/ocelot.prod.json
```

**Ejemplo de cambio:** Agregar nueva ruta para `/api/catalog`.

### Paso 2: Recrear ConfigMap

```bash
# Eliminar ConfigMap anterior
kubectl delete configmap gateway-config -n okla

# Crear nuevo ConfigMap con archivo actualizado
kubectl create configmap gateway-config -n okla \
  --from-file=ocelot.json=backend/Gateway/Gateway.Api/ocelot.prod.json
```

**Salida:**
```
configmap "gateway-config" deleted
configmap/gateway-config created
```

### Paso 3: Reiniciar Gateway

```bash
kubectl rollout restart deployment gateway -n okla
```

**Salida:**
```
deployment.apps/gateway restarted
```

### Verificar que el Gateway cargó la nueva configuración

```bash
# Ver logs del gateway
kubectl logs -f deployment/gateway -n okla --tail=20

# Buscar mensaje de startup
kubectl logs deployment/gateway -n okla | grep -i "started"
```

**Log esperado:**
```
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

### Probar nueva ruta

```bash
curl https://api.okla.com.do/api/catalog/makes
```

---

## 7. Troubleshooting 404/503

### Error 404 Not Found

**Síntoma:**
```bash
curl https://api.okla.com.do/api/vehicles
# 404 Not Found
```

**Causas comunes:**

#### 1. Ruta no existe en ocelot.json

```bash
# Verificar si la ruta está configurada
kubectl exec deployment/gateway -n okla -- cat /app/ocelot.json | grep -i "vehicles"
```

Si no encuentra nada, **la ruta no está configurada**.

**Solución:** Agregar ruta al ocelot.json.

#### 2. UpstreamPathTemplate no coincide

```json
// ocelot.json tiene:
"UpstreamPathTemplate": "/vehicles/{everything}"

// Cliente hace request a:
/api/vehicles  ❌ NO MATCH
```

**Solución:** Ajustar template:
```json
"UpstreamPathTemplate": "/api/vehicles/{everything}"
```

#### 3. Typo en el path

```
Cliente:  /api/vehicels  (typo: vehicels)
Gateway:  /api/vehicles
```

**Solución:** Corregir typo en cliente.

### Error 503 Service Unavailable

**Síntoma:**
```bash
curl https://api.okla.com.do/api/vehicles
# 503 Service Unavailable
```

**Causas comunes:**

#### 1. Servicio downstream no existe

```bash
# Verificar que el servicio existe
kubectl get svc vehiclessaleservice -n okla
```

Si no existe, error 503.

**Solución:** Crear el servicio.

#### 2. Servicio usa puerto incorrecto

```json
// ocelot.json tiene:
"Port": 80  ❌ INCORRECTO EN KUBERNETES

// Debe ser:
"Port": 8080  ✅ CORRECTO
```

**Solución:** Cambiar puerto a 8080 y actualizar ConfigMap.

#### 3. Pod no está Running

```bash
kubectl get pods -n okla | grep vehiclessaleservice
```

Si el pod está en `CrashLoopBackOff` o `Error`, no puede responder.

**Solución:** Debugging del pod (ver logs, describe).

#### 4. Service no tiene endpoints

```bash
kubectl get endpoints vehiclessaleservice -n okla
```

**Salida problemática:**
```
NAME                  ENDPOINTS   AGE
vehiclessaleservice   <none>      10m
```

`<none>` significa que no hay pods detrás del servicio.

**Causas:**
- Label selector incorrecto
- Pods no están Running
- Pods no están Ready

**Solución:**
```bash
# Ver label del servicio
kubectl get svc vehiclessaleservice -n okla -o yaml | grep -A3 selector

# Ver labels de los pods
kubectl get pods -n okla --show-labels | grep vehiclessale
```

Asegúrate de que coincidan.

### Debugging paso a paso

```bash
# 1. Verificar Gateway está Running
kubectl get pods -n okla | grep gateway

# 2. Ver logs del Gateway
kubectl logs -f deployment/gateway -n okla --tail=50

# 3. Verificar ruta en ocelot.json
kubectl exec deployment/gateway -n okla -- cat /app/ocelot.json | grep -A10 "vehicles"

# 4. Verificar servicio downstream
kubectl get svc vehiclessaleservice -n okla

# 5. Verificar endpoints
kubectl get endpoints vehiclessaleservice -n okla

# 6. Probar conectividad desde Gateway
kubectl exec -it deployment/gateway -n okla -- sh
wget -qO- http://vehiclessaleservice:8080/health
exit

# 7. Probar desde exterior
curl -I https://api.okla.com.do/api/vehicles
```

---

## 8. Features avanzadas

### Rate Limiting

```json
{
  "UpstreamPathTemplate": "/api/vehicles/{everything}",
  "DownstreamPathTemplate": "/api/{everything}",
  "DownstreamHostAndPorts": [
    { "Host": "vehiclessaleservice", "Port": 8080 }
  ],
  "RateLimitOptions": {
    "ClientWhitelist": [],
    "EnableRateLimiting": true,
    "Period": "1m",
    "PeriodTimespan": 60,
    "Limit": 100
  }
}
```

**Significado:**
- **Limit:** 100 requests
- **Period:** En 1 minuto
- **PeriodTimespan:** Duración del período (60 segundos)

### CORS

```json
{
  "GlobalConfiguration": {
    "BaseUrl": "https://api.okla.com.do",
    "AllowedOrigins": [
      "https://okla.com.do",
      "https://www.okla.com.do"
    ],
    "AllowedMethods": ["GET", "POST", "PUT", "DELETE"],
    "AllowedHeaders": ["Content-Type", "Authorization"]
  }
}
```

### Load Balancing (múltiples instancias)

```json
{
  "DownstreamHostAndPorts": [
    { "Host": "vehiclessaleservice", "Port": 8080 },
    { "Host": "vehiclessaleservice-replica", "Port": 8080 }
  ],
  "LoadBalancerOptions": {
    "Type": "RoundRobin"
  }
}
```

**Tipos:**
- **RoundRobin** - Distribuye uniformemente
- **LeastConnection** - Al pod con menos conexiones
- **NoLoadBalancer** - Usa el primero

**En Kubernetes:** Kubernetes ya hace load balancing a nivel de Service. No necesitas múltiples hosts.

### Authentication

```json
{
  "AuthenticationOptions": {
    "AuthenticationProviderKey": "Bearer",
    "AllowedScopes": []
  }
}
```

**Requiere configurar JWT bearer en Program.cs del Gateway.**

---

## 9. Cheat Sheet

### Comandos de ConfigMap

```bash
# Crear ConfigMap desde ocelot.json
kubectl create cm gateway-config -n okla --from-file=ocelot.json=path/to/ocelot.prod.json

# Ver ConfigMap
kubectl get cm gateway-config -n okla -o yaml

# Actualizar ConfigMap
kubectl delete cm gateway-config -n okla
kubectl create cm gateway-config -n okla --from-file=ocelot.json=path/to/ocelot.prod.json

# Reiniciar Gateway
kubectl rollout restart deployment gateway -n okla
```

### Debugging de rutas

```bash
# Ver ocelot.json en el pod
kubectl exec deployment/gateway -n okla -- cat /app/ocelot.json

# Buscar ruta específica
kubectl exec deployment/gateway -n okla -- cat /app/ocelot.json | grep -A10 "vehicles"

# Ver logs en tiempo real
kubectl logs -f deployment/gateway -n okla

# Probar conectividad desde Gateway
kubectl exec -it deployment/gateway -n okla -- wget -qO- http://service:8080/health

# Probar desde exterior
curl -I https://api.okla.com.do/api/vehicles
```

### Flujo completo de actualización

```bash
# 1. Editar archivo local
nano backend/Gateway/Gateway.Api/ocelot.prod.json

# 2. Recrear ConfigMap
kubectl delete cm gateway-config -n okla
kubectl create cm gateway-config -n okla --from-file=ocelot.json=backend/Gateway/Gateway.Api/ocelot.prod.json

# 3. Reiniciar Gateway
kubectl rollout restart deployment gateway -n okla

# 4. Verificar rollout
kubectl rollout status deployment gateway -n okla

# 5. Probar
curl https://api.okla.com.do/health
```

---

## 10. Ejercicios prácticos

### Ejercicio 1: Verificar ocelot.json en el pod

1. Entra al pod del Gateway
2. Ve el contenido de /app/ocelot.json
3. Cuenta cuántas rutas hay

<details>
<summary>Solución</summary>

```bash
# 1
kubectl exec -it deployment/gateway -n okla -- sh

# 2
cat /app/ocelot.json | head -50

# 3
cat /app/ocelot.json | grep -c "UpstreamPathTemplate"

exit
```
</details>

### Ejercicio 2: Verificar ruta de vehículos

1. Busca la ruta de vehicles en ocelot.json
2. Verifica el Host downstream
3. Verifica el Port downstream

<details>
<summary>Solución</summary>

```bash
# 1
kubectl exec deployment/gateway -n okla -- cat /app/ocelot.json | grep -A15 "vehicles"

# 2 y 3 - Aparecerán en el output del comando anterior
# Host: vehiclessaleservice
# Port: 8080
```
</details>

### Ejercicio 3: Probar conectividad

1. Desde el Gateway, prueba conectividad a `vehiclessaleservice:8080/health`
2. Desde exterior, prueba `api.okla.com.do/api/vehicles`

<details>
<summary>Solución</summary>

```bash
# 1
kubectl exec -it deployment/gateway -n okla -- sh
apk add wget
wget -qO- http://vehiclessaleservice:8080/health
exit

# 2
curl https://api.okla.com.do/api/vehicles
```
</details>

### Ejercicio 4: Simular actualización de configuración

1. Ve el ConfigMap actual del gateway
2. Simula una recreación del ConfigMap (puedes usar el mismo archivo)
3. Reinicia el Gateway

<details>
<summary>Solución</summary>

```bash
# 1
kubectl get cm gateway-config -n okla -o yaml | head -20

# 2
kubectl delete cm gateway-config -n okla
kubectl create cm gateway-config -n okla --from-file=ocelot.json=backend/Gateway/Gateway.Api/ocelot.prod.json

# 3
kubectl rollout restart deployment gateway -n okla
kubectl rollout status deployment gateway -n okla
```
</details>

### Ejercicio 5: Debugging de ruta no encontrada

1. Intenta acceder a una ruta que NO existe: `/api/nonexistent`
2. Ve el código de respuesta HTTP
3. Ve los logs del Gateway

<details>
<summary>Solución</summary>

```bash
# 1 y 2
curl -I https://api.okla.com.do/api/nonexistent
# Debería retornar 404

# 3
kubectl logs deployment/gateway -n okla --tail=20 | grep "nonexistent"
```
</details>

---

## 🎉 ¡Felicidades!

Has completado el Tutorial 9. Ahora sabes:
- ✅ Qué es un API Gateway y ventajas
- ✅ Por qué Ocelot para .NET
- ✅ Estructura de ocelot.json
- ✅ Configurar rutas y transformaciones
- ✅ Montar configuración desde ConfigMap
- ✅ Actualizar configuración en producción
- ✅ Troubleshooting de 404/503
- ✅ Features avanzadas (rate limiting, CORS)

---

**Anterior:** [08 - PostgreSQL StatefulSet](./08-postgresql-statefulset.md)  
**Siguiente:** [10 - Troubleshooting 404/503](./10-troubleshooting-errors.md)
