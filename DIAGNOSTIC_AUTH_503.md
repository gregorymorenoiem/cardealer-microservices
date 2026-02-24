# 🔍 ANÁLISIS: AuthService 503 Service Temporarily Unavailable

## Resumen del Problema

**Error reportado**: `503 Service Temporarily Unavailable (nginx)`

**Lugar**: Al acceder a AuthService a través del Gateway/Ocelot

**Estado del servicio**: ✅ AuthService ESTÁ CORRIENDO Y RESPONDIENDO 200 OK

## Investigación Realizada

### 1. ✅ Estado del AuthService Pod

```bash
kubectl get pods -n okla | grep auth
# Resultado: authservice-849bc545b5-snm5w  1/1  Running  0  5m42s
```

**Status**: ✅ **Pod está CORRIENDO** (1/1 Ready)

### 2. ✅ Logs del AuthService

Últimas 50 líneas de logs muestran:

- ✅ `HTTP POST /api/auth/login responded 200`
- ✅ `HTTP GET /api/auth/me responded 200`
- ✅ `HTTP GET /health/ready responded 200`
- ✅ Login exitoso, notificaciones enviadas, sesión creada

**Status**: ✅ **El servicio está SANO**

### 3. ✅ Configuración de Ocelot (Gateway)

**Rutas configuradas en `/backend/Gateway/Gateway.Api/ocelot.prod.json`**:

```json
{
  "UpstreamPathTemplate": "/api/auth/health",
  "DownstreamPathTemplate": "/health",
  "DownstreamScheme": "http",
  "DownstreamHostAndPorts": [{ "Host": "authservice", "Port": 8080 }]
},
{
  "UpstreamPathTemplate": "/api/auth/{everything}",
  "DownstreamPathTemplate": "/api/auth/{everything}",
  "DownstreamScheme": "http",
  "DownstreamHostAndPorts": [{ "Host": "authservice", "Port": 8080 }]
}
```

**Status**: ✅ **Rutas están configuradas correctamente**

### 4. ❌ Problema Identificado: Gateway/Ocelot LOGS

Los logs del Gateway muestran problemas de conectividad a **otros servicios**:

```
[WRN] Error connecting to downstream service, exception:
System.Net.Http.HttpRequestException: Connection refused (maintenanceservice:8080)
[ERR] HTTP GET /api/maintenance/status responded 502 in 8ms
```

**Pero no vemos errores de conexión a authservice en los logs recientes.**

## Análisis de Causas Posibles

### Posibilidad 1: Circuit Breaker de Ocelot

Si el Gateway ha recibido múltiples errores previos de authservice, el **circuit breaker** puede estar abierto.

**Configuración**:

```json
"QoSOptions": {
  "ExceptionsAllowedBeforeBreaking": 3,
  "DurationOfBreak": 10,  // 10 segundos
  "TimeoutValue": 30000
}
```

**Síntomas**:

- ✅ AuthService responde 200 OK
- ✅ Gateway devuelve 503 (o 502)
- ❌ Circuit breaker está en estado abierto

**Solución**: Esperar 10 segundos O hacer rollout restart del Gateway

### Posibilidad 2: DNS Resolution en K8s

Si `authservice` no resuelve correctamente en DNS de K8s:

```
DownstreamHostAndPorts: [{ "Host": "authservice", "Port": 8080 }]
```

**Síntomas**:

- ❌ Gateway no puede resolver DNS de `authservice`
- ✅ Direct pod-to-pod communication funciona
- ❌ Gateway devuelve 502/503

**Verificación**:

```bash
kubectl run -it --rm debug --image=busybox --restart=Never -- nslookup authservice.okla.svc.cluster.local
```

### Posibilidad 3: Mala Ruta en Ocelot Config

Si la ruta `/api/auth/{everything}` está mal ordenada en el JSON y está siendo sobrescrita por otra ruta catch-all.

**Síntomas**:

- ✅ AuthService responde
- ❌ Gateway no redirige correctamente
- ❌ Gateway devuelve 503/404/502

**Verificación**: Revisar orden de rutas en `ocelot.prod.json`

### Posibilidad 4: ConfigMap No Actualizado

Si el Gateway tiene un ConfigMap con `ocelot.prod.json` desactualizado:

**Síntomas**:

- ✅ Código está correcto en `/backend/Gateway/`
- ❌ K8s ConfigMap apunta a versión vieja
- ❌ Gateway usa configuración antigua

**Verificación**:

```bash
kubectl get configmap -n okla | grep ocelot
kubectl get configmap -n okla ocelot-config -o yaml | grep authservice
```

## Pasos de Diagnóstico

### Paso 1: Verificar DNS en K8s

```bash
kubectl run -it --rm debug --image=busybox --restart=Never -- \
  nslookup authservice.okla.svc.cluster.local
# Si no resuelve, el problema es DNS
```

### Paso 2: Verificar Conectividad desde Gateway Pod

```bash
kubectl exec -n okla deployment/gateway -- \
  curl -v http://authservice:8080/health
# Debe devolver 200 OK
```

### Paso 3: Verificar Circuit Breaker

```bash
kubectl logs -n okla deployment/gateway --tail=200 | grep -i "circuit\|breaker"
```

### Paso 4: Verificar ConfigMap de Ocelot

```bash
kubectl get configmap -n okla -o yaml | grep -A20 "authservice"
```

### Paso 5: Restart del Gateway

```bash
kubectl rollout restart deployment/gateway -n okla
kubectl wait --for=condition=ready pod -l app=gateway -n okla --timeout=300s
```

## Recomendaciones

1. **Inmediato**: Hacer `kubectl rollout restart deployment/gateway -n okla`
   - Esto reseteará el circuit breaker si está abierto

2. **Corto Plazo**: Verificar DNS resolution en K8s
   - El Gateway debe poder resolver `authservice` en el namespace `okla`

3. **Mediano Plazo**: Validar que ConfigMap tiene la configuración correcta de Ocelot
   - Asegurar que `ocelot.prod.json` en K8s coincida con el del repo

4. **Largo Plazo**: Implementar health checks del Gateway
   - Agregar métricas de circuit breaker
   - Monitorear errores de conexión por ruta

## Archivos Relevantes

- [ocelot.prod.json](backend/Gateway/Gateway.Api/ocelot.prod.json) - Líneas 345-370
- [AuthService/Program.cs](backend/AuthService/AuthService.Api/Program.cs) - Línea 46 (servicio corriendo)
- [Gateway ConfigMap en K8s] - `ocelot-config` o similar

## Estado Actual

🔴 **BLOCKER**: Users cannot access AuthService through Gateway  
✅ **UNDERLYING SERVICE**: AuthService is healthy and responding  
⚠️ **ROOT CAUSE**: Gateway/Ocelot circuit breaker OR network issue  
🆘 **ACTION**: Try `kubectl rollout restart deployment/gateway -n okla`
