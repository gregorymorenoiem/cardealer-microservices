# Tutorial 14: Monitoreo Básico

**Duración:** 45 minutos  
**Nivel:** Intermedio  
**Requisitos:** Tutorial 13 completado

---

## 📋 Resumen

Aprenderás a monitorear recursos y health de tu cluster Kubernetes. Entenderás cómo usar `kubectl top`, configurar alertas básicas, y verificar el estado de servicios en producción.

### Lo que aprenderás:
- Health checks de servicios
- Monitoreo de recursos (CPU, memoria)
- kubectl top para pods y nodes
- Eventos del cluster
- Logs agregados
- Alertas básicas (conceptual)

---

## 📑 Índice

1. [Health checks](#1-health-checks)
2. [Monitoreo de recursos](#2-monitoreo-de-recursos)
3. [kubectl top](#3-kubectl-top)
4. [Eventos del cluster](#4-eventos-del-cluster)
5. [Logs agregados](#5-logs-agregados)
6. [Métricas de negocio](#6-métricas-de-negocio)
7. [Alertas conceptuales](#7-alertas-conceptuales)
8. [Dashboard de Digital Ocean](#8-dashboard-de-digital-ocean)
9. [Cheat Sheet](#9-cheat-sheet)
10. [Ejercicios prácticos](#10-ejercicios-prácticos)

---

## 1. Health checks

### Endpoints de health

#### /health (básico)

```bash
curl https://api.okla.com.do/health
```

**Respuesta esperada:**
```
Healthy
```

#### /health con status code

```bash
curl -I https://api.okla.com.do/health
```

**Respuesta esperada:**
```
HTTP/2 200 OK
content-type: text/plain
date: Thu, 07 Jan 2026 12:00:00 GMT
```

### Health checks de servicios individuales

```bash
# Gateway
curl https://api.okla.com.do/health

# Vehicles (a través del Gateway)
curl https://api.okla.com.do/api/vehicles/health

# Auth
curl https://api.okla.com.do/api/auth/health
```

### Health check desde dentro del cluster

```bash
# Entrar a un pod
kubectl exec -it deployment/gateway -n okla -- sh

# Health check de otro servicio
wget -qO- http://vehiclessaleservice:8080/health

# Health check de PostgreSQL (puerto 5432)
nc -zv postgres 5432
# Output: postgres (10.245.x.x:5432) open

exit
```

### Script de monitoreo continuo

```bash
#!/bin/bash
# health-monitor.sh

ENDPOINTS=(
  "https://api.okla.com.do/health"
  "https://api.okla.com.do/api/vehicles"
  "https://okla.com.do"
)

while true; do
  echo "=== Health Check: $(date) ==="
  
  for ENDPOINT in "${ENDPOINTS[@]}"; do
    STATUS=$(curl -s -o /dev/null -w "%{http_code}" "$ENDPOINT")
    
    if [ "$STATUS" -eq 200 ]; then
      echo "✅ $ENDPOINT - OK"
    else
      echo "❌ $ENDPOINT - FAILED (HTTP $STATUS)"
    fi
  done
  
  echo ""
  sleep 60  # Cada 60 segundos
done
```

**Ejecutar:**
```bash
chmod +x health-monitor.sh
./health-monitor.sh
```

---

## 2. Monitoreo de recursos

### Ver recursos de pods

```bash
kubectl get pods -n okla -o custom-columns=\
NAME:.metadata.name,\
CPU-REQUEST:.spec.containers[0].resources.requests.cpu,\
MEM-REQUEST:.spec.containers[0].resources.requests.memory,\
CPU-LIMIT:.spec.containers[0].resources.limits.cpu,\
MEM-LIMIT:.spec.containers[0].resources.limits.memory
```

**Output:**
```
NAME                                   CPU-REQUEST   MEM-REQUEST   CPU-LIMIT   MEM-LIMIT
gateway-7f8d6b9c5-abcde               100m          128Mi         500m        512Mi
authservice-5d9c8b7a4-fghij           100m          128Mi         500m        512Mi
vehiclessaleservice-6e7f9c8d5-klmno   200m          256Mi         1000m       1Gi
```

### Definir recursos en Deployment

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: vehiclessaleservice
  namespace: okla
spec:
  template:
    spec:
      containers:
      - name: vehiclessaleservice
        image: ghcr.io/gregorymorenoiem/cardealer-vehiclessaleservice:latest
        resources:
          requests:
            cpu: 200m      # 0.2 CPU cores
            memory: 256Mi  # 256 MiB
          limits:
            cpu: 1000m     # 1 CPU core
            memory: 1Gi    # 1 GiB
```

**Requests vs Limits:**

| Tipo | Descripción | Comportamiento |
|------|-------------|----------------|
| **requests** | Recursos garantizados | Kubernetes garantiza que el pod tenga estos recursos |
| **limits** | Máximo permitido | Si el pod excede, es throttled (CPU) o killed (memoria) |

### Ver uso total del namespace

```bash
kubectl top pods -n okla --sum
```

**Output:**
```
NAME                                   CPU(cores)   MEMORY(bytes)
gateway-7f8d6b9c5-abcde               50m          120Mi
authservice-5d9c8b7a4-fghij           30m          100Mi
...

SUM                                    500m         2Gi
```

---

## 3. kubectl top

### Prerrequisito: Metrics Server

Kubernetes necesita **Metrics Server** para `kubectl top`.

**Verificar si está instalado:**
```bash
kubectl get deployment metrics-server -n kube-system
```

**Si no está instalado (DOKS lo tiene por default):**
```bash
kubectl apply -f https://github.com/kubernetes-sigs/metrics-server/releases/latest/download/components.yaml
```

### kubectl top pods

```bash
# Todos los pods del namespace
kubectl top pods -n okla

# Pods ordenados por CPU
kubectl top pods -n okla --sort-by=cpu

# Pods ordenados por memoria
kubectl top pods -n okla --sort-by=memory
```

**Output:**
```
NAME                                   CPU(cores)   MEMORY(bytes)
vehiclessaleservice-6e7f9c8d5-klmno   120m         450Mi
gateway-7f8d6b9c5-abcde               80m          200Mi
postgres-0                            50m          800Mi
authservice-5d9c8b7a4-fghij           30m          150Mi
```

**Interpretación:**
- `120m` = 0.12 CPU cores (12%)
- `450Mi` = 450 MiB de memoria

### kubectl top nodes

```bash
kubectl top nodes
```

**Output:**
```
NAME                   CPU(cores)   CPU%   MEMORY(bytes)   MEMORY%
pool-okla-default-1    800m         40%    4Gi             50%
pool-okla-default-2    600m         30%    3Gi             37%
```

**Interpretación:**
- Cluster tiene 2 nodos
- Nodo 1: Usando 40% CPU, 50% memoria
- Nodo 2: Usando 30% CPU, 37% memoria

### Monitoreo continuo

```bash
# Actualiza cada 2 segundos
watch -n 2 'kubectl top pods -n okla --sort-by=cpu'
```

---

## 4. Eventos del cluster

### Ver eventos recientes

```bash
# Todos los eventos del namespace
kubectl get events -n okla

# Últimos 20 eventos
kubectl get events -n okla --sort-by='.lastTimestamp' | tail -20

# Solo warnings
kubectl get events -n okla --field-selector type=Warning
```

**Output:**
```
LAST SEEN   TYPE      REASON              OBJECT                           MESSAGE
2m          Normal    Scheduled           pod/gateway-abc123               Successfully assigned okla/gateway-abc123 to node-1
1m          Normal    Pulling             pod/gateway-abc123               Pulling image "ghcr.io/.../gateway:latest"
1m          Normal    Pulled              pod/gateway-abc123               Successfully pulled image
30s         Warning   BackOff             pod/authservice-xyz789           Back-off restarting failed container
```

### Eventos de un pod específico

```bash
kubectl describe pod gateway-7f8d6b9c5-abcde -n okla | grep -A10 "Events:"
```

**Output:**
```
Events:
  Type    Reason     Age   From               Message
  ----    ------     ----  ----               -------
  Normal  Scheduled  5m    default-scheduler  Successfully assigned okla/gateway-abc to node-1
  Normal  Pulling    5m    kubelet            Pulling image "ghcr.io/gregorymorenoiem/cardealer-gateway:latest"
  Normal  Pulled     4m    kubelet            Successfully pulled image
  Normal  Created    4m    kubelet            Created container gateway
  Normal  Started    4m    kubelet            Started container gateway
```

### Watch eventos en tiempo real

```bash
kubectl get events -n okla --watch
```

**Output (streaming):**
```
LAST SEEN   TYPE     REASON    OBJECT              MESSAGE
0s          Normal   Pulling   pod/gateway-new     Pulling image...
5s          Normal   Pulled    pod/gateway-new     Successfully pulled
10s         Normal   Started   pod/gateway-new     Started container
```

---

## 5. Logs agregados

### Ver logs de múltiples pods

```bash
# Ver logs de todos los pods del gateway
kubectl logs -f deployment/gateway -n okla --all-containers=true

# Ver logs de los últimos 100 líneas
kubectl logs deployment/gateway -n okla --tail=100

# Ver logs desde hace 1 hora
kubectl logs deployment/gateway -n okla --since=1h
```

### Filtrar logs

```bash
# Buscar errores
kubectl logs deployment/gateway -n okla | grep -i error

# Buscar requests a /api/vehicles
kubectl logs deployment/gateway -n okla | grep "/api/vehicles"

# Contar errores
kubectl logs deployment/gateway -n okla | grep -c "ERROR"
```

### Logs con timestamps

```bash
kubectl logs deployment/gateway -n okla --timestamps | tail -50
```

**Output:**
```
2026-01-07T12:00:00.123Z info: Microsoft.Hosting.Lifetime[0] Application started
2026-01-07T12:00:01.456Z info: Ocelot.Middleware[0] Ocelot is using route: /api/vehicles
2026-01-07T12:00:02.789Z warn: Ocelot.Errors[0] DownstreamRouteFinderMiddleware error
```

### Logs del pod anterior (si crasheó)

```bash
kubectl logs pod/gateway-abc123 -n okla --previous
```

### Script para agregar logs

```bash
#!/bin/bash
# aggregate-logs.sh

SERVICES="gateway authservice userservice vehiclessaleservice"
OUTPUT_FILE="logs-$(date +%Y%m%d-%H%M%S).log"

echo "Aggregating logs from services: $SERVICES"
echo "Output: $OUTPUT_FILE"
echo ""

for SERVICE in $SERVICES; do
  echo "=== $SERVICE ===" >> $OUTPUT_FILE
  kubectl logs deployment/$SERVICE -n okla --tail=100 >> $OUTPUT_FILE 2>&1
  echo "" >> $OUTPUT_FILE
done

echo "✅ Logs saved to $OUTPUT_FILE"
```

---

## 6. Métricas de negocio

### Endpoint de métricas custom

```csharp
// VehiclesSaleService.Api/Controllers/MetricsController.cs

[ApiController]
[Route("api/[controller]")]
public class MetricsController : ControllerBase
{
    private readonly VehiclesDbContext _context;

    public MetricsController(VehiclesDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetMetrics()
    {
        var totalVehicles = await _context.Vehicles.CountAsync();
        var activeVehicles = await _context.Vehicles.CountAsync(v => v.Status == "Active");
        var totalUsers = await _context.Users.CountAsync();

        return Ok(new
        {
            totalVehicles,
            activeVehicles,
            totalUsers,
            timestamp = DateTime.UtcNow
        });
    }
}
```

### Consultar métricas

```bash
curl https://api.okla.com.do/api/metrics
```

**Respuesta:**
```json
{
  "totalVehicles": 1250,
  "activeVehicles": 980,
  "totalUsers": 450,
  "timestamp": "2026-01-07T12:00:00Z"
}
```

### Script de monitoreo de métricas

```bash
#!/bin/bash
# business-metrics.sh

while true; do
  echo "=== Business Metrics: $(date) ==="
  
  METRICS=$(curl -s https://api.okla.com.do/api/metrics)
  
  TOTAL=$(echo "$METRICS" | jq -r '.totalVehicles')
  ACTIVE=$(echo "$METRICS" | jq -r '.activeVehicles')
  USERS=$(echo "$METRICS" | jq -r '.totalUsers')
  
  echo "Total Vehicles: $TOTAL"
  echo "Active Vehicles: $ACTIVE"
  echo "Total Users: $USERS"
  echo ""
  
  sleep 300  # Cada 5 minutos
done
```

---

## 7. Alertas conceptuales

### Herramientas de alerting

| Herramienta | Descripción | Complejidad |
|-------------|-------------|-------------|
| **Prometheus + Alertmanager** | Métricas y alertas | Media |
| **Grafana** | Visualización y alertas | Media |
| **Digital Ocean Monitoring** | Built-in (DOKS) | Baja |
| **UptimeRobot** | External monitoring | Muy baja |
| **PagerDuty** | Incident management | Alta |

### Alertas típicas

#### 1. Pod no está Running

```yaml
# Alerta conceptual
IF pod.status != "Running" FOR 5 minutes
THEN send alert "Pod {{ pod.name }} is down"
```

**Verificar manualmente:**
```bash
kubectl get pods -n okla | grep -v Running
```

#### 2. CPU alto

```yaml
IF pod.cpu > 80% FOR 10 minutes
THEN send alert "Pod {{ pod.name }} high CPU usage"
```

**Verificar manualmente:**
```bash
kubectl top pods -n okla --sort-by=cpu
```

#### 3. Memoria alta

```yaml
IF pod.memory > 90% OF limits FOR 5 minutes
THEN send alert "Pod {{ pod.name }} high memory"
```

#### 4. Restart frecuentes

```yaml
IF pod.restarts > 5 IN 1 hour
THEN send alert "Pod {{ pod.name }} restarting frequently"
```

**Verificar manualmente:**
```bash
kubectl get pods -n okla -o custom-columns=NAME:.metadata.name,RESTARTS:.status.containerStatuses[0].restartCount
```

#### 5. Health check fallo

```yaml
IF http.status != 200 FOR /health FOR 3 minutes
THEN send alert "Service health check failing"
```

---

## 8. Dashboard de Digital Ocean

### Acceder al dashboard

1. Ve a [cloud.digitalocean.com](https://cloud.digitalocean.com)
2. Kubernetes → okla-cluster
3. Insights tab

### Métricas disponibles

| Métrica | Descripción |
|---------|-------------|
| **CPU Usage** | Uso de CPU por node |
| **Memory Usage** | Uso de memoria por node |
| **Disk Usage** | Uso de disco persistente |
| **Network I/O** | Tráfico de red |
| **Pod Count** | Número de pods |
| **Node Status** | Estado de los nodes |

### Configurar alertas en DO

1. Cluster → Settings → Alerts
2. Create Alert
3. Configurar:
   - **Metric:** CPU usage
   - **Threshold:** 80%
   - **Duration:** 5 minutes
   - **Action:** Email notification
4. Save

### Visualizar logs en DO

1. Cluster → Logs
2. Filtrar por:
   - Namespace: okla
   - Pod: gateway
   - Time range: Last 1 hour
3. Buscar keywords

---

## 9. Cheat Sheet

### Health checks

```bash
# API health
curl https://api.okla.com.do/health

# Status code only
curl -s -o /dev/null -w "%{http_code}" https://api.okla.com.do/health

# Internal health check
kubectl exec -it deployment/gateway -n okla -- wget -qO- http://vehiclessaleservice:8080/health
```

### Recursos

```bash
# Uso actual de pods
kubectl top pods -n okla

# Uso actual de nodes
kubectl top nodes

# Pods ordenados por CPU
kubectl top pods -n okla --sort-by=cpu

# Pods ordenados por memoria
kubectl top pods -n okla --sort-by=memory
```

### Eventos

```bash
# Todos los eventos
kubectl get events -n okla

# Últimos eventos
kubectl get events -n okla --sort-by='.lastTimestamp' | tail -20

# Solo warnings
kubectl get events -n okla --field-selector type=Warning

# Watch en tiempo real
kubectl get events -n okla --watch
```

### Logs

```bash
# Logs de un deployment
kubectl logs -f deployment/gateway -n okla

# Últimas 100 líneas
kubectl logs deployment/gateway -n okla --tail=100

# Desde hace 1 hora
kubectl logs deployment/gateway -n okla --since=1h

# Con timestamps
kubectl logs deployment/gateway -n okla --timestamps

# Pod anterior (crashed)
kubectl logs pod/gateway-abc -n okla --previous
```

---

## 10. Ejercicios prácticos

### Ejercicio 1: Health check de todos los servicios

1. Haz health check del Gateway
2. Haz health check de VehiclesSaleService
3. Verifica que ambos retornan 200

<details>
<summary>Solución</summary>

```bash
# 1
curl -I https://api.okla.com.do/health
# HTTP/2 200 OK

# 2
curl -I https://api.okla.com.do/api/vehicles/health
# HTTP/2 200 OK

# 3
# Ambos deben retornar 200
```
</details>

### Ejercicio 2: Ver uso de recursos

1. Ve el uso de CPU y memoria de todos los pods
2. Identifica el pod con mayor uso de CPU
3. Identifica el pod con mayor uso de memoria

<details>
<summary>Solución</summary>

```bash
# 1
kubectl top pods -n okla

# 2
kubectl top pods -n okla --sort-by=cpu | head -2
# El primero (después del header) es el mayor

# 3
kubectl top pods -n okla --sort-by=memory | head -2
```
</details>

### Ejercicio 3: Ver eventos recientes

1. Lista todos los eventos del namespace okla
2. Filtra solo warnings
3. Cuenta cuántos warnings hay

<details>
<summary>Solución</summary>

```bash
# 1
kubectl get events -n okla

# 2
kubectl get events -n okla --field-selector type=Warning

# 3
kubectl get events -n okla --field-selector type=Warning | wc -l
```
</details>

### Ejercicio 4: Buscar errores en logs

1. Ve los logs del Gateway
2. Busca líneas con "ERROR"
3. Cuenta cuántos errores hay

<details>
<summary>Solución</summary>

```bash
# 1
kubectl logs deployment/gateway -n okla --tail=200

# 2
kubectl logs deployment/gateway -n okla | grep ERROR

# 3
kubectl logs deployment/gateway -n okla | grep -c ERROR
```
</details>

### Ejercicio 5: Verificar recursos definidos

1. Ve los resources requests de vehiclessaleservice
2. Ve los resources limits
3. Compara con el uso actual (top)

<details>
<summary>Solución</summary>

```bash
# 1 y 2
kubectl get deployment vehiclessaleservice -n okla -o yaml | grep -A10 resources

# 3
kubectl top pods -n okla | grep vehiclessaleservice

# Comparar:
# Requests: 200m CPU, 256Mi MEM
# Uso actual: (depende de la carga)
```
</details>

---

## 🎉 ¡Felicidades!

Has completado el Tutorial 14. Ahora sabes:
- ✅ Hacer health checks de servicios
- ✅ Monitorear recursos con kubectl top
- ✅ Ver eventos del cluster
- ✅ Agregar y filtrar logs
- ✅ Definir métricas de negocio
- ✅ Conceptos de alerting
- ✅ Usar el dashboard de Digital Ocean

---

**Anterior:** [13 - Smart CI/CD](./13-smart-cicd.md)  
**Siguiente:** [15 - Deploy Completo](./15-deploy-completo.md)
