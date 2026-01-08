# Tutorial 6: LoadBalancer y Networking

**Duración:** 50 minutos  
**Nivel:** Intermedio  
**Requisitos:** Tutorial 5 completado

---

## 📋 Resumen

Aprenderás cómo funciona el networking en Kubernetes, los tipos de servicios disponibles, y cómo configurar un LoadBalancer e Ingress para exponer tu aplicación al mundo exterior.

### Lo que aprenderás:
- Tipos de servicios en Kubernetes (ClusterIP, NodePort, LoadBalancer)
- Cómo funciona el networking entre pods
- Configurar un LoadBalancer en Digital Ocean
- Crear y configurar Ingress
- Ruteo basado en hostname y path
- Networking troubleshooting

---

## 📑 Índice

1. [Conceptos: Networking en Kubernetes](#1-conceptos-networking-en-kubernetes)
2. [Tipos de Services](#2-tipos-de-services)
3. [ClusterIP (interno)](#3-clusterip-interno)
4. [LoadBalancer (externo)](#4-loadbalancer-externo)
5. [Ingress Controller](#5-ingress-controller)
6. [Configurar Ingress](#6-configurar-ingress)
7. [Ruteo avanzado](#7-ruteo-avanzado)
8. [Network Policies (opcional)](#8-network-policies-opcional)
9. [Cheat Sheet](#9-cheat-sheet)
10. [Ejercicios prácticos](#10-ejercicios-prácticos)

---

## 1. Conceptos: Networking en Kubernetes

### Modelo de red de Kubernetes

Kubernetes sigue estos principios:
1. **Cada pod tiene su propia IP** única en el cluster
2. **Todos los pods pueden comunicarse** entre sí sin NAT
3. **Los servicios proporcionan IPs estables** para grupos de pods

```
┌─────────────────────────────────────────────────────────┐
│                      Internet                           │
└──────────────────────┬──────────────────────────────────┘
                       │
                       ▼
┌──────────────────────────────────────────────────────────┐
│              LoadBalancer (146.190.199.0)                │
└──────────────────────┬──────────────────────────────────┘
                       │
                       ▼
┌──────────────────────────────────────────────────────────┐
│                    Ingress Controller                    │
│          (rutea por hostname: okla.com.do)               │
└──────────────────────┬──────────────────────────────────┘
                       │
        ┌──────────────┴──────────────┐
        ▼                             ▼
┌───────────────┐           ┌─────────────────┐
│  Service:     │           │  Service:       │
│  frontend-web │           │  gateway        │
│  ClusterIP    │           │  ClusterIP      │
└───────┬───────┘           └────────┬────────┘
        │                            │
        ▼                            ▼
┌───────────────┐           ┌─────────────────┐
│  Pod:         │           │  Pod:           │
│  frontend-web │           │  gateway        │
│  10.244.1.5   │           │  10.244.1.6     │
└───────────────┘           └─────────────────┘
```

### Componentes de networking

| Componente | Función |
|------------|---------|
| **Pod Network** | Red interna donde cada pod tiene su IP |
| **Service** | IP estable que balancea tráfico entre pods |
| **Ingress** | Ruteo HTTP/HTTPS basado en reglas |
| **LoadBalancer** | IP pública para acceso desde internet |
| **NetworkPolicy** | Firewall rules entre pods |

---

## 2. Tipos de Services

Kubernetes tiene 4 tipos de servicios:

### 🔹 ClusterIP (default)

**Descripción:** IP interna accesible solo dentro del cluster.

```yaml
apiVersion: v1
kind: Service
metadata:
  name: vehiclessaleservice
spec:
  type: ClusterIP
  selector:
    app: vehiclessaleservice
  ports:
  - port: 8080
    targetPort: 8080
```

**¿Cuándo usar?**
- Comunicación interna entre microservicios
- Bases de datos
- Servicios que NO necesitan acceso externo

**Ejemplo en OKLA:** Todos los microservicios backend usan ClusterIP.

### 🔹 NodePort

**Descripción:** Expone el servicio en un puerto de cada nodo (30000-32767).

```yaml
apiVersion: v1
kind: Service
metadata:
  name: my-service
spec:
  type: NodePort
  selector:
    app: my-app
  ports:
  - port: 8080
    targetPort: 8080
    nodePort: 30080  # Puerto en los nodos
```

**¿Cuándo usar?**
- Testing rápido
- Clusters on-premise sin LoadBalancer

**NO recomendado para producción** en clouds que soportan LoadBalancer.

### 🔹 LoadBalancer

**Descripción:** Crea un LoadBalancer externo (en cloud provider) con IP pública.

```yaml
apiVersion: v1
kind: Service
metadata:
  name: nginx-lb
spec:
  type: LoadBalancer
  selector:
    app: nginx
  ports:
  - port: 80
    targetPort: 80
```

**¿Cuándo usar?**
- Exponer servicios a internet
- Cuando necesitas IP pública
- En cloud providers (AWS, GCP, Azure, DO)

**Costo:** Cada LoadBalancer tiene costo mensual (~$10-20/mes en DO).

### 🔹 ExternalName

**Descripción:** Mapea un servicio a un DNS externo.

```yaml
apiVersion: v1
kind: Service
metadata:
  name: external-api
spec:
  type: ExternalName
  externalName: api.external-service.com
```

**¿Cuándo usar?**
- Referenciar servicios externos como si fueran internos
- Migración gradual de servicios

---

## 3. ClusterIP (interno)

### Ver servicios ClusterIP en OKLA

```bash
kubectl get svc -n okla
```

**Salida:**
```
NAME                  TYPE        CLUSTER-IP      EXTERNAL-IP   PORT(S)
authservice           ClusterIP   10.116.54.128   <none>        8080/TCP
billingservice        ClusterIP   10.116.58.241   <none>        8080/TCP
frontend-web          ClusterIP   10.116.58.183   <none>        8080/TCP
gateway               ClusterIP   10.116.51.214   <none>        8080/TCP
vehiclessaleservice   ClusterIP   10.116.49.128   <none>        8080/TCP
postgres              ClusterIP   10.116.45.161   <none>        5432/TCP
rabbitmq              ClusterIP   10.116.52.179   <none>        5672/TCP, 15672/TCP
redis                 ClusterIP   10.116.53.194   <none>        6379/TCP
```

**EXTERNAL-IP `<none>`** significa que son solo accesibles internamente.

### Anatomía de un Service ClusterIP

```yaml
apiVersion: v1
kind: Service
metadata:
  name: vehiclessaleservice
  namespace: okla
spec:
  type: ClusterIP
  selector:
    app: vehiclessaleservice  # Selecciona pods con este label
  ports:
  - name: http
    port: 8080              # Puerto del Service
    targetPort: 8080        # Puerto del contenedor
    protocol: TCP
```

**¿Qué hace?**
1. Crea una IP estable (10.116.49.128)
2. Encuentra pods con label `app=vehiclessaleservice`
3. Balancea tráfico entre esos pods

### Ver endpoints (pods detrás del service)

```bash
kubectl get endpoints vehiclessaleservice -n okla
```

**Salida:**
```
NAME                  ENDPOINTS           AGE
vehiclessaleservice   10.244.1.23:8080    6h
```

**10.244.1.23** es la IP del pod de vehiclessaleservice.

### Probar conectividad interna

```bash
# Desde el gateway, hacer request a vehiclessaleservice
kubectl exec -it deployment/gateway -n okla -- sh

# Instalar wget si no existe
apk add wget

# Request al servicio
wget -qO- http://vehiclessaleservice:8080/health

# Request usando IP del service
wget -qO- http://10.116.49.128:8080/health

# Request usando FQDN
wget -qO- http://vehiclessaleservice.okla.svc.cluster.local:8080/health
```

Todos deberían funcionar.

---

## 4. LoadBalancer (externo)

### ¿Cómo funciona en Digital Ocean?

Cuando creas un Service tipo LoadBalancer en DOKS:
1. Kubernetes le pide a Digital Ocean crear un LoadBalancer
2. DO crea el LB y asigna una IP pública
3. DO configura el LB para rutear tráfico al cluster
4. Kubernetes actualiza el Service con la EXTERNAL-IP

**Costo:** $10-15/mes por LoadBalancer en Digital Ocean.

### Ver LoadBalancer de ingress-nginx

```bash
kubectl get svc -n ingress-nginx
```

**Salida:**
```
NAME                                 TYPE           EXTERNAL-IP      PORT(S)
ingress-nginx-controller             LoadBalancer   146.190.199.0    80:31234/TCP,443:31567/TCP
```

**EXTERNAL-IP `146.190.199.0`** es la IP pública que apunta a tu cluster.

### Anatomía de un Service LoadBalancer

```yaml
apiVersion: v1
kind: Service
metadata:
  name: ingress-nginx-controller
  namespace: ingress-nginx
spec:
  type: LoadBalancer
  selector:
    app.kubernetes.io/name: ingress-nginx
  ports:
  - name: http
    port: 80
    targetPort: http
    protocol: TCP
  - name: https
    port: 443
    targetPort: https
    protocol: TCP
```

**¿Qué hace?**
1. Expone puertos 80 (HTTP) y 443 (HTTPS)
2. Redirige tráfico al Ingress Controller
3. El Ingress Controller rutea según reglas de Ingress

### Ver LoadBalancer en Digital Ocean Console

1. Ve a https://cloud.digitalocean.com/networking/load_balancers
2. Verás un LB con la IP 146.190.199.0
3. Configuración:
   - **Forwarding Rules:** 80 → 31234, 443 → 31567 (NodePorts)
   - **Health Check:** TCP en puerto 10256
   - **Droplets:** Tus nodos del cluster

### Crear un LoadBalancer para un servicio específico

**⚠️ No recomendado** - Usa Ingress en su lugar para ahorrar costos.

```yaml
apiVersion: v1
kind: Service
metadata:
  name: my-app-lb
spec:
  type: LoadBalancer
  selector:
    app: my-app
  ports:
  - port: 80
    targetPort: 8080
```

Esto crearía un SEGUNDO LoadBalancer ($10 extra/mes).

**Mejor práctica:** Un solo LoadBalancer (ingress-nginx) + múltiples Ingress rules.

---

## 5. Ingress Controller

### ¿Qué es un Ingress Controller?

Un **Ingress Controller** es un pod especial que:
- Recibe tráfico del LoadBalancer
- Lee las reglas de Ingress
- Rutea requests a los servicios correctos

**Analogía:** Es como un "reverse proxy" inteligente.

### Ingress Controller más populares

| Nombre | Cloud | Características |
|--------|-------|-----------------|
| **NGINX Ingress** | Universal | Simple, confiable, usado en OKLA |
| **Traefik** | Universal | Auto-discovery, dashboard |
| **AWS ALB** | AWS | Integrado con ALB |
| **GCE Ingress** | GCP | Integrado con GCP LB |

**En OKLA usamos NGINX Ingress Controller.**

### Verificar que ingress-nginx está instalado

```bash
kubectl get pods -n ingress-nginx
```

**Salida:**
```
NAME                                       READY   STATUS    RESTARTS   AGE
ingress-nginx-controller-xxx               1/1     Running   0          7d
```

### Ver configuración del Ingress Controller

```bash
kubectl get deployment ingress-nginx-controller -n ingress-nginx -o yaml
```

**Configuraciones clave:**
- `--enable-ssl-passthrough` - Para SSL
- `--default-backend-service` - Servicio por defecto (404)
- `--publish-service` - LoadBalancer service

### Ver logs del Ingress Controller

```bash
kubectl logs -f deployment/ingress-nginx-controller -n ingress-nginx
```

**Verás:**
- Requests HTTP/HTTPS
- Upstreams (servicios backend)
- Errores de ruteo
- SSL/TLS handshakes

---

## 6. Configurar Ingress

### ¿Qué es un Ingress?

Un **Ingress** es un recurso que define reglas de ruteo HTTP/HTTPS.

```
Cliente → LoadBalancer → Ingress Controller → [Ingress Rules] → Service → Pod
```

### Ingress básico (single service)

```yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: simple-ingress
  namespace: okla
spec:
  ingressClassName: nginx
  rules:
  - host: okla.com.do
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: frontend-web
            port:
              number: 8080
```

**¿Qué hace?**
- Requests a `okla.com.do/*` van a `frontend-web:8080`

### Ingress con múltiples hosts

```yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: okla-ingress
  namespace: okla
spec:
  ingressClassName: nginx
  rules:
  - host: okla.com.do
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: frontend-web
            port:
              number: 8080
  - host: api.okla.com.do
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: gateway
            port:
              number: 8080
```

**¿Qué hace?**
- `okla.com.do` → frontend-web
- `api.okla.com.do` → gateway

### Aplicar Ingress

```bash
kubectl apply -f k8s/ingress.yaml
```

### Ver Ingress

```bash
kubectl get ingress -n okla
```

**Salida:**
```
NAME           CLASS   HOSTS                             ADDRESS          PORTS     AGE
okla-ingress   nginx   okla.com.do,api.okla.com.do      146.190.199.0    80, 443   6h
```

**ADDRESS** debe mostrar la IP del LoadBalancer.

### Ver detalles

```bash
kubectl describe ingress okla-ingress -n okla
```

**Salida:**
```
Name:             okla-ingress
Namespace:        okla
Address:          146.190.199.0
Ingress Class:    nginx
Rules:
  Host             Path  Backends
  ----             ----  --------
  okla.com.do
                   /   frontend-web:8080 (10.244.1.23:8080)
  api.okla.com.do
                   /   gateway:8080 (10.244.1.24:8080)
```

---

## 7. Ruteo avanzado

### Ruteo por path (path-based routing)

```yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: path-based-ingress
spec:
  ingressClassName: nginx
  rules:
  - host: okla.com.do
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: frontend-web
            port:
              number: 8080
      - path: /api
        pathType: Prefix
        backend:
          service:
            name: gateway
            port:
              number: 8080
```

**¿Qué hace?**
- `okla.com.do/` → frontend-web
- `okla.com.do/api/*` → gateway

### PathType options

| PathType | Comportamiento |
|----------|----------------|
| **Exact** | Match exacto: `/api` pero NO `/api/` |
| **Prefix** | Match prefijo: `/api` y `/api/*` |
| **ImplementationSpecific** | Depende del Ingress Controller |

### Rewrite paths (quitar prefijo)

```yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: rewrite-ingress
  annotations:
    nginx.ingress.kubernetes.io/rewrite-target: /$2
spec:
  ingressClassName: nginx
  rules:
  - host: okla.com.do
    http:
      paths:
      - path: /api(/|$)(.*)
        pathType: ImplementationSpecific
        backend:
          service:
            name: gateway
            port:
              number: 8080
```

**¿Qué hace?**
- Request: `okla.com.do/api/vehicles`
- Rewrite: `gateway:8080/vehicles` (quita `/api`)

### Redirección permanente

```yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: redirect-ingress
  annotations:
    nginx.ingress.kubernetes.io/permanent-redirect: https://okla.com.do
spec:
  ingressClassName: nginx
  rules:
  - host: www.okla.com.do
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: frontend-web
            port:
              number: 8080
```

**¿Qué hace?**
- `www.okla.com.do` → redirige a `okla.com.do`

### CORS headers

```yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: cors-ingress
  annotations:
    nginx.ingress.kubernetes.io/enable-cors: "true"
    nginx.ingress.kubernetes.io/cors-allow-origin: "https://okla.com.do"
    nginx.ingress.kubernetes.io/cors-allow-methods: "GET, POST, PUT, DELETE, OPTIONS"
```

### Rate limiting

```yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: rate-limit-ingress
  annotations:
    nginx.ingress.kubernetes.io/limit-rps: "10"  # 10 requests/segundo
```

---

## 8. Network Policies (opcional)

### ¿Qué son Network Policies?

**Network Policies** son como firewalls para pods. Controlan:
- Qué pods pueden recibir tráfico (Ingress)
- Qué pods pueden enviar tráfico (Egress)

**⚠️ Nota:** Requiere CNI plugin que soporte NetworkPolicy (Calico, Cilium). DOKS usa Cilium.

### Ejemplo: Solo el Gateway puede acceder a VehiclesSaleService

```yaml
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata:
  name: vehiclessaleservice-policy
  namespace: okla
spec:
  podSelector:
    matchLabels:
      app: vehiclessaleservice
  policyTypes:
  - Ingress
  ingress:
  - from:
    - podSelector:
        matchLabels:
          app: gateway
    ports:
    - protocol: TCP
      port: 8080
```

**¿Qué hace?**
- Solo pods con label `app=gateway` pueden hacer requests a `vehiclessaleservice:8080`
- Todos los demás pods son bloqueados

### Ejemplo: Permitir todo (default)

Por defecto, Kubernetes permite todo el tráfico entre pods.

Para permitir explícitamente todo:

```yaml
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata:
  name: allow-all
  namespace: okla
spec:
  podSelector: {}
  policyTypes:
  - Ingress
  - Egress
  ingress:
  - {}
  egress:
  - {}
```

---

## 9. Cheat Sheet

### Comandos de Services

| Comando | Descripción |
|---------|-------------|
| `kubectl get svc -n okla` | Listar servicios |
| `kubectl describe svc <nombre> -n okla` | Detalles del servicio |
| `kubectl get endpoints <nombre> -n okla` | Ver pods detrás del servicio |
| `kubectl expose deployment <nombre> --port=8080 -n okla` | Crear servicio ClusterIP |

### Comandos de Ingress

| Comando | Descripción |
|---------|-------------|
| `kubectl get ingress -n okla` | Listar Ingress |
| `kubectl describe ingress <nombre> -n okla` | Detalles del Ingress |
| `kubectl get ingressclass` | Ver Ingress Classes |
| `kubectl logs -f deployment/ingress-nginx-controller -n ingress-nginx` | Logs del Ingress Controller |

### Debugging de networking

```bash
# Ver IP del LoadBalancer
kubectl get svc -n ingress-nginx | grep LoadBalancer

# Probar conectividad desde un pod
kubectl exec -it deployment/gateway -n okla -- wget -qO- http://vehiclessaleservice:8080/health

# Ver configuración del Ingress Controller
kubectl exec -it deployment/ingress-nginx-controller -n ingress-nginx -- cat /etc/nginx/nginx.conf
```

---

## 10. Ejercicios prácticos

### Ejercicio 1: Explorar Services

1. Lista todos los servicios en namespace okla
2. Ve los endpoints del servicio `authservice`
3. Describe el servicio `gateway`

<details>
<summary>Solución</summary>

```bash
# 1
kubectl get svc -n okla

# 2
kubectl get endpoints authservice -n okla

# 3
kubectl describe svc gateway -n okla
```
</details>

### Ejercicio 2: Verificar LoadBalancer

1. Ve el servicio del Ingress Controller en namespace ingress-nginx
2. Obtén la EXTERNAL-IP
3. Verifica que esa IP responde con curl

<details>
<summary>Solución</summary>

```bash
# 1 y 2
kubectl get svc -n ingress-nginx

# 3 (reemplaza con tu IP)
curl -I http://146.190.199.0
```
</details>

### Ejercicio 3: Inspeccionar Ingress

1. Lista los Ingress en namespace okla
2. Ve los detalles del Ingress principal
3. Identifica qué hosts están configurados

<details>
<summary>Solución</summary>

```bash
# 1
kubectl get ingress -n okla

# 2
kubectl describe ingress okla-ingress -n okla

# 3
kubectl get ingress okla-ingress -n okla -o yaml | grep -A2 "rules:"
```
</details>

### Ejercicio 4: Probar conectividad interna

1. Entra al pod del gateway
2. Instala wget
3. Haz un request a `vehiclessaleservice:8080/health`

<details>
<summary>Solución</summary>

```bash
# 1
kubectl exec -it deployment/gateway -n okla -- sh

# 2
apk add wget

# 3
wget -qO- http://vehiclessaleservice:8080/health

exit
```
</details>

### Ejercicio 5: Ver logs del Ingress Controller

1. Muestra los últimos 50 logs del Ingress Controller
2. Busca requests a `/api/vehicles`

<details>
<summary>Solución</summary>

```bash
# 1
kubectl logs deployment/ingress-nginx-controller -n ingress-nginx --tail=50

# 2
kubectl logs deployment/ingress-nginx-controller -n ingress-nginx | grep "/api/vehicles"
```
</details>

---

## 🎉 ¡Felicidades!

Has completado el Tutorial 6. Ahora sabes:
- ✅ Tipos de servicios (ClusterIP, NodePort, LoadBalancer)
- ✅ Cómo funciona el networking en Kubernetes
- ✅ Configurar LoadBalancer en Digital Ocean
- ✅ Crear y configurar Ingress
- ✅ Ruteo avanzado (hosts, paths, rewrites)
- ✅ Troubleshooting de networking

---

**Anterior:** [05 - DNS y SSL](./05-dns-ssl.md)  
**Siguiente:** [07 - GitHub Container Registry](./07-registries-ghcr.md)
