# Tutorial 15: Deploy Completo (Masterclass)

**Duración:** 120 minutos  
**Nivel:** Experto  
**Requisitos:** Tutoriales 1-14 completados

---

## 📋 Resumen

Este es el tutorial definitivo que integra TODOS los conceptos aprendidos. Realizarás un deploy completo de OKLA desde cero hasta producción en Digital Ocean Kubernetes (DOKS), configurando infraestructura, CI/CD, networking, SSL, y monitoreo.

### Lo que aprenderás:
- Setup completo de cluster DOKS
- Configuración de infraestructura (PostgreSQL, Redis, RabbitMQ)
- Deploy de todos los microservicios
- Configuración de Ocelot Gateway
- Setup de DNS y SSL (Let's Encrypt)
- CI/CD con GitHub Actions
- Verificación y monitoreo
- Troubleshooting end-to-end

---

## 📑 Índice

1. [Arquitectura final](#1-arquitectura-final)
2. [Preparación del cluster](#2-preparación-del-cluster)
3. [Infraestructura base](#3-infraestructura-base)
4. [Deploy de servicios](#4-deploy-de-servicios)
5. [Configuración de Gateway](#5-configuración-de-gateway)
6. [Networking y SSL](#6-networking-y-ssl)
7. [CI/CD setup](#7-cicd-setup)
8. [Verificación completa](#8-verificación-completa)
9. [Troubleshooting](#9-troubleshooting)
10. [Checklist final](#10-checklist-final)

---

## 1. Arquitectura final

### Stack completo

```
┌─────────────────────────────────────────────────────────────┐
│                     Internet                                │
└───────────────────────┬─────────────────────────────────────┘
                        │
        ┌───────────────┴───────────────┐
        │                               │
        ▼                               ▼
┌───────────────┐              ┌────────────────┐
│ okla.com.do   │              │ api.okla.com.do│
│  (Frontend)   │              │   (Gateway)    │
└───────┬───────┘              └────────┬───────┘
        │                               │
        ▼                               ▼
┌─────────────────────────────────────────────────────────────┐
│              Digital Ocean LoadBalancer                     │
│                  IP: 146.190.199.0                          │
└───────────────────────┬─────────────────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────────────────┐
│         DOKS Cluster: okla-cluster (NYC1)                   │
│         Namespace: okla                                     │
│                                                             │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐     │
│  │ Ingress      │  │ cert-manager │  │ Secrets      │     │
│  │ (nginx)      │  │ (Let's       │  │              │     │
│  └──────┬───────┘  │  Encrypt)    │  └──────────────┘     │
│         │          └──────────────┘                        │
│         ▼                                                   │
│  ┌─────────────────────────────────────────────────┐       │
│  │            Frontend Web (React 19)              │       │
│  │            Port: 8080                            │       │
│  └─────────────────────────────────────────────────┘       │
│         │                                                   │
│         ▼                                                   │
│  ┌─────────────────────────────────────────────────┐       │
│  │         Gateway (Ocelot API Gateway)            │       │
│  │         Port: 8080                               │       │
│  │         ocelot.json via ConfigMap                │       │
│  └────────┬────────────────────────────────────────┘       │
│           │                                                 │
│  ┌────────┴────────────────────────────────────────┐       │
│  │                                                  │       │
│  ▼                    ▼                    ▼        ▼       │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐  │
│  │ Auth     │  │ User     │  │ Vehicles │  │ Media    │  │
│  │ Service  │  │ Service  │  │ Sale     │  │ Service  │  │
│  │ :8080    │  │ :8080    │  │ Service  │  │ :8080    │  │
│  └────┬─────┘  └────┬─────┘  │ :8080    │  └────┬─────┘  │
│       │             │         └────┬─────┘       │         │
│       │             │              │             │         │
│       └─────────────┴──────────────┴─────────────┘         │
│                          │                                  │
│                          ▼                                  │
│  ┌───────────────────────────────────────────────────┐     │
│  │           Infrastructure Layer                    │     │
│  │                                                    │     │
│  │  ┌──────────────┐  ┌────────────┐  ┌──────────┐  │     │
│  │  │ PostgreSQL   │  │ Redis      │  │ RabbitMQ │  │     │
│  │  │ StatefulSet  │  │ Deployment │  │ Deploy   │  │     │
│  │  │ PVC: 10Gi    │  │ Port: 6379 │  │ 5672/    │  │     │
│  │  │ Port: 5432   │  └────────────┘  │ 15672    │  │     │
│  │  └──────────────┘                  └──────────┘  │     │
│  └───────────────────────────────────────────────────┘     │
└─────────────────────────────────────────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────────────────┐
│          GitHub Container Registry (ghcr.io)                │
│          - cardealer-gateway:latest                         │
│          - cardealer-authservice:latest                     │
│          - cardealer-userservice:latest                     │
│          - cardealer-vehiclessaleservice:latest             │
│          - cardealer-web:latest                             │
└─────────────────────────────────────────────────────────────┘
```

### Tecnologías

| Componente | Tecnología | Versión |
|------------|------------|---------|
| **Kubernetes** | Digital Ocean DOKS | 1.28+ |
| **Container Registry** | GitHub Container Registry | - |
| **Backend Services** | .NET 8 | 8.0.x |
| **Frontend** | React 19 + Vite | 19.x |
| **API Gateway** | Ocelot | 22.0.1 |
| **Database** | PostgreSQL | 16 |
| **Cache** | Redis | 7 |
| **Message Broker** | RabbitMQ | 3.12 |
| **Ingress** | nginx-ingress | latest |
| **SSL** | cert-manager + Let's Encrypt | v1.13 |
| **CI/CD** | GitHub Actions | - |
| **DNS** | Digital Ocean DNS | - |

---

## 2. Preparación del cluster

### Paso 1: Crear cluster DOKS

```bash
# Instalar doctl si no lo tienes
brew install doctl

# Autenticar
doctl auth init
# Pegar token de Digital Ocean

# Crear cluster
doctl kubernetes cluster create okla-cluster \
  --region nyc1 \
  --size s-2vcpu-4gb \
  --count 2 \
  --auto-upgrade=true \
  --surge-upgrade=true

# Esperar ~5 minutos
```

**Output:**
```
Notice: Cluster is provisioning, waiting for cluster to be running
.............................
Notice: Cluster created
ID                                      Name           Region    Version        Auto Upgrade    Status
abc123-def456-ghi789-jkl012-mno345      okla-cluster   nyc1      1.28.2-do.0    true            running
```

### Paso 2: Conectar a cluster

```bash
# Obtener kubeconfig
doctl kubernetes cluster kubeconfig save okla-cluster

# Verificar conexión
kubectl cluster-info

# Verificar nodes
kubectl get nodes
```

**Output esperado:**
```
NAME                   STATUS   ROLES    AGE   VERSION
pool-okla-default-1    Ready    <none>   5m    v1.28.2
pool-okla-default-2    Ready    <none>   5m    v1.28.2
```

### Paso 3: Crear namespace

```bash
kubectl create namespace okla

# Verificar
kubectl get namespaces | grep okla
```

### Paso 4: Configurar ImagePullSecrets

```bash
# Crear secret para GHCR
kubectl create secret docker-registry ghcr-secret \
  --docker-server=ghcr.io \
  --docker-username=YOUR_GITHUB_USERNAME \
  --docker-password=YOUR_GITHUB_PAT \
  --docker-email=YOUR_EMAIL \
  -n okla

# Verificar
kubectl get secrets -n okla | grep ghcr
```

---

## 3. Infraestructura base

### Paso 1: Deploy PostgreSQL

```yaml
# postgres.yaml
apiVersion: v1
kind: PersistentVolumeClaim
metadata:
  name: postgres-pvc
  namespace: okla
spec:
  accessModes:
  - ReadWriteOnce
  resources:
    requests:
      storage: 10Gi
  storageClassName: do-block-storage
---
apiVersion: apps/v1
kind: StatefulSet
metadata:
  name: postgres
  namespace: okla
spec:
  serviceName: postgres
  replicas: 1
  selector:
    matchLabels:
      app: postgres
  template:
    metadata:
      labels:
        app: postgres
    spec:
      containers:
      - name: postgres
        image: postgres:16
        ports:
        - containerPort: 5432
        env:
        - name: POSTGRES_PASSWORD
          value: "YourSecurePassword123!"
        - name: POSTGRES_DB
          value: "okla"
        volumeMounts:
        - name: postgres-storage
          mountPath: /var/lib/postgresql/data
      volumes:
      - name: postgres-storage
        persistentVolumeClaim:
          claimName: postgres-pvc
---
apiVersion: v1
kind: Service
metadata:
  name: postgres
  namespace: okla
spec:
  selector:
    app: postgres
  ports:
  - port: 5432
    targetPort: 5432
  type: ClusterIP
```

```bash
kubectl apply -f postgres.yaml

# Verificar
kubectl get statefulset postgres -n okla
kubectl get pvc -n okla
kubectl get svc postgres -n okla
```

### Paso 2: Deploy Redis

```yaml
# redis.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: redis
  namespace: okla
spec:
  replicas: 1
  selector:
    matchLabels:
      app: redis
  template:
    metadata:
      labels:
        app: redis
    spec:
      containers:
      - name: redis
        image: redis:7-alpine
        ports:
        - containerPort: 6379
        command: ["redis-server"]
        args: ["--appendonly", "yes"]
---
apiVersion: v1
kind: Service
metadata:
  name: redis
  namespace: okla
spec:
  selector:
    app: redis
  ports:
  - port: 6379
    targetPort: 6379
  type: ClusterIP
```

```bash
kubectl apply -f redis.yaml

# Verificar
kubectl get deployment redis -n okla
kubectl get svc redis -n okla
```

### Paso 3: Deploy RabbitMQ

```yaml
# rabbitmq.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: rabbitmq
  namespace: okla
spec:
  replicas: 1
  selector:
    matchLabels:
      app: rabbitmq
  template:
    metadata:
      labels:
        app: rabbitmq
    spec:
      containers:
      - name: rabbitmq
        image: rabbitmq:3.12-management-alpine
        ports:
        - containerPort: 5672
          name: amqp
        - containerPort: 15672
          name: management
        env:
        - name: RABBITMQ_DEFAULT_USER
          value: "admin"
        - name: RABBITMQ_DEFAULT_PASS
          value: "YourSecurePassword123!"
---
apiVersion: v1
kind: Service
metadata:
  name: rabbitmq
  namespace: okla
spec:
  selector:
    app: rabbitmq
  ports:
  - port: 5672
    targetPort: 5672
    name: amqp
  - port: 15672
    targetPort: 15672
    name: management
  type: ClusterIP
```

```bash
kubectl apply -f rabbitmq.yaml

# Verificar
kubectl get deployment rabbitmq -n okla
kubectl get svc rabbitmq -n okla
```

### Verificación de infraestructura

```bash
# Verificar que todo está Running
kubectl get pods -n okla

# Debe mostrar:
# postgres-0               1/1     Running
# redis-xxx                1/1     Running
# rabbitmq-xxx             1/1     Running
```

---

## 4. Deploy de servicios

### Paso 1: Crear ConfigMap de Gateway

```bash
kubectl create configmap gateway-config -n okla \
  --from-file=ocelot.json=backend/Gateway/Gateway.Api/ocelot.prod.json
```

### Paso 2: Deploy Gateway

```yaml
# gateway.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: gateway
  namespace: okla
spec:
  replicas: 2
  selector:
    matchLabels:
      app: gateway
  template:
    metadata:
      labels:
        app: gateway
    spec:
      imagePullSecrets:
      - name: ghcr-secret
      containers:
      - name: gateway
        image: ghcr.io/gregorymorenoiem/cardealer-gateway:latest
        ports:
        - containerPort: 8080
        volumeMounts:
        - name: ocelot-config
          mountPath: /app/ocelot.json
          subPath: ocelot.json
        readinessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 15
          periodSeconds: 10
      volumes:
      - name: ocelot-config
        configMap:
          name: gateway-config
---
apiVersion: v1
kind: Service
metadata:
  name: gateway
  namespace: okla
spec:
  selector:
    app: gateway
  ports:
  - port: 8080
    targetPort: 8080
  type: ClusterIP
```

```bash
kubectl apply -f gateway.yaml
```

### Paso 3: Deploy microservicios

```yaml
# services.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: authservice
  namespace: okla
spec:
  replicas: 2
  selector:
    matchLabels:
      app: authservice
  template:
    metadata:
      labels:
        app: authservice
    spec:
      imagePullSecrets:
      - name: ghcr-secret
      containers:
      - name: authservice
        image: ghcr.io/gregorymorenoiem/cardealer-authservice:latest
        ports:
        - containerPort: 8080
        env:
        - name: ConnectionStrings__DefaultConnection
          value: "Host=postgres;Database=authservice;Username=postgres;Password=YourSecurePassword123!"
        - name: Redis__ConnectionString
          value: "redis:6379"
---
apiVersion: v1
kind: Service
metadata:
  name: authservice
  namespace: okla
spec:
  selector:
    app: authservice
  ports:
  - port: 8080
    targetPort: 8080
---
# Repetir para userservice, vehiclessaleservice, etc.
```

```bash
kubectl apply -f services.yaml
```

### Paso 4: Deploy Frontend

```yaml
# frontend.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: frontend-web
  namespace: okla
spec:
  replicas: 2
  selector:
    matchLabels:
      app: frontend-web
  template:
    metadata:
      labels:
        app: frontend-web
    spec:
      imagePullSecrets:
      - name: ghcr-secret
      containers:
      - name: frontend-web
        image: ghcr.io/gregorymorenoiem/cardealer-web:latest
        ports:
        - containerPort: 8080
        env:
        - name: RUNTIME_API_URL
          value: "https://api.okla.com.do"
---
apiVersion: v1
kind: Service
metadata:
  name: frontend-web
  namespace: okla
spec:
  selector:
    app: frontend-web
  ports:
  - port: 8080
    targetPort: 8080
```

```bash
kubectl apply -f frontend.yaml
```

---

## 5. Configuración de Gateway

### Verificar ocelot.json

```bash
# Ver configuración
kubectl exec deployment/gateway -n okla -- cat /app/ocelot.json | head -50

# Verificar rutas
kubectl exec deployment/gateway -n okla -- cat /app/ocelot.json | grep "UpstreamPathTemplate"
```

### Rutas críticas

Verificar que estas rutas existen:
- `/api/auth/{everything}` → authservice
- `/api/users/{everything}` → userservice
- `/api/vehicles/{everything}` → vehiclessaleservice
- `/api/homepagesections/{everything}` → vehiclessaleservice

---

## 6. Networking y SSL

### Paso 1: Instalar ingress-nginx

```bash
kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.8.2/deploy/static/provider/cloud/deploy.yaml

# Verificar
kubectl get svc -n ingress-nginx
```

**Esperar ~2 minutos** hasta que el LoadBalancer tenga EXTERNAL-IP.

```bash
kubectl get svc ingress-nginx-controller -n ingress-nginx -w
```

**Anotar la IP del LoadBalancer** (ej: 146.190.199.0)

### Paso 2: Configurar DNS

1. Ve a Digital Ocean → Networking → Domains
2. Agrega dominio: `okla.com.do`
3. Crear registros A:

```
Type: A
Hostname: @
Value: 146.190.199.0  (LoadBalancer IP)
TTL: 300

Type: A
Hostname: api
Value: 146.190.199.0
TTL: 300

Type: A
Hostname: www
Value: 146.190.199.0
TTL: 300
```

4. Verificar DNS:

```bash
dig okla.com.do +short
dig api.okla.com.do +short
# Ambos deben retornar 146.190.199.0
```

### Paso 3: Instalar cert-manager

```bash
kubectl apply -f https://github.com/cert-manager/cert-manager/releases/download/v1.13.0/cert-manager.yaml

# Verificar
kubectl get pods -n cert-manager
```

### Paso 4: Crear ClusterIssuer

```yaml
# cluster-issuer.yaml
apiVersion: cert-manager.io/v1
kind: ClusterIssuer
metadata:
  name: letsencrypt-prod
spec:
  acme:
    server: https://acme-v02.api.letsencrypt.org/directory
    email: your-email@example.com
    privateKeySecretRef:
      name: letsencrypt-prod
    solvers:
    - http01:
        ingress:
          class: nginx
```

```bash
kubectl apply -f cluster-issuer.yaml
```

### Paso 5: Crear Ingress

```yaml
# ingress.yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: okla-ingress
  namespace: okla
  annotations:
    cert-manager.io/cluster-issuer: "letsencrypt-prod"
    nginx.ingress.kubernetes.io/ssl-redirect: "true"
spec:
  ingressClassName: nginx
  tls:
  - hosts:
    - okla.com.do
    - www.okla.com.do
    - api.okla.com.do
    secretName: okla-tls
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

```bash
kubectl apply -f ingress.yaml
```

### Paso 6: Verificar certificado

```bash
# Ver estado del certificado
kubectl get certificate -n okla

# Debe mostrar:
# NAME        READY   SECRET      AGE
# okla-tls    True    okla-tls    2m
```

Si `READY` es `False`, ver logs:

```bash
kubectl describe certificate okla-tls -n okla
kubectl logs -n cert-manager -l app=cert-manager
```

---

## 7. CI/CD setup

### Paso 1: Configurar secrets en GitHub

1. Ve a repo → Settings → Secrets and variables → Actions
2. Crear secrets:

**DIGITALOCEAN_ACCESS_TOKEN:**
```bash
# Obtener token de https://cloud.digitalocean.com/account/api/tokens
```

**KUBE_CONFIG:**
```bash
cat ~/.kube/config | base64 > kubeconfig-base64.txt
cat kubeconfig-base64.txt
# Copiar y pegar en GitHub Secret
```

**GHCR_TOKEN:**
```bash
# Crear PAT en https://github.com/settings/tokens
# Scopes: write:packages, read:packages
```

### Paso 2: Verificar workflows

```bash
# Ver workflows
ls .github/workflows

# Debe mostrar:
# smart-cicd.yml
# deploy-digitalocean.yml
# pr-checks.yml
```

### Paso 3: Trigger manual deploy

1. Ve a Actions en GitHub
2. Selecciona "Deploy to Digital Ocean"
3. Run workflow → main branch
4. Esperar ~10 minutos

---

## 8. Verificación completa

### Checklist de verificación

```bash
#!/bin/bash
# verify-deployment.sh

echo "=== OKLA Deployment Verification ==="
echo ""

# 1. Cluster
echo "1. Cluster status..."
kubectl cluster-info | head -1

# 2. Namespace
echo "2. Namespace..."
kubectl get namespace okla

# 3. Pods
echo "3. Pods status..."
kubectl get pods -n okla | grep -v "Running" || echo "✅ All pods Running"

# 4. Services
echo "4. Services..."
kubectl get svc -n okla | wc -l
echo "services found"

# 5. Ingress
echo "5. Ingress..."
kubectl get ingress -n okla

# 6. Certificate
echo "6. SSL Certificate..."
kubectl get certificate okla-tls -n okla | grep True && echo "✅ SSL Ready"

# 7. Frontend
echo "7. Frontend health..."
STATUS=$(curl -s -o /dev/null -w "%{http_code}" https://okla.com.do)
[ "$STATUS" -eq 200 ] && echo "✅ Frontend OK (HTTP $STATUS)" || echo "❌ Frontend Failed (HTTP $STATUS)"

# 8. API Gateway
echo "8. API Gateway health..."
STATUS=$(curl -s -o /dev/null -w "%{http_code}" https://api.okla.com.do/health)
[ "$STATUS" -eq 200 ] && echo "✅ Gateway OK (HTTP $STATUS)" || echo "❌ Gateway Failed (HTTP $STATUS)"

# 9. Vehicles API
echo "9. Vehicles API..."
STATUS=$(curl -s -o /dev/null -w "%{http_code}" https://api.okla.com.do/api/vehicles)
[ "$STATUS" -eq 200 ] && echo "✅ Vehicles API OK" || echo "❌ Vehicles API Failed"

# 10. Database
echo "10. Database connectivity..."
kubectl exec -it statefulset/postgres -n okla -- psql -U postgres -c "SELECT version();" | grep PostgreSQL && echo "✅ Database OK"

echo ""
echo "=== Verification Complete ==="
```

```bash
chmod +x verify-deployment.sh
./verify-deployment.sh
```

---

## 9. Troubleshooting

### Problema 1: Pod en CrashLoopBackOff

```bash
# Ver logs
kubectl logs pod-name -n okla --previous

# Ver eventos
kubectl describe pod pod-name -n okla

# Causas comunes:
# - Image no existe
# - Environment variables incorrectas
# - Conexión a DB fallida
```

### Problema 2: 404 Not Found

```bash
# Verificar ruta en ocelot.json
kubectl exec deployment/gateway -n okla -- cat /app/ocelot.json | grep -A10 "ruta-problema"

# Si falta, agregar y actualizar ConfigMap
kubectl delete cm gateway-config -n okla
kubectl create cm gateway-config -n okla --from-file=ocelot.json=backend/Gateway/Gateway.Api/ocelot.prod.json
kubectl rollout restart deployment/gateway -n okla
```

### Problema 3: 503 Service Unavailable

```bash
# Verificar Service y Endpoints
kubectl get svc service-name -n okla
kubectl get endpoints service-name -n okla

# Si endpoints es <none>, verificar selector
kubectl get svc service-name -n okla -o yaml | grep -A3 selector
kubectl get pods -n okla --show-labels | grep service-name
```

### Problema 4: SSL no funciona

```bash
# Ver estado del certificado
kubectl describe certificate okla-tls -n okla

# Ver logs de cert-manager
kubectl logs -n cert-manager -l app=cert-manager

# Verificar DNS
dig api.okla.com.do +short
# Debe retornar IP del LoadBalancer
```

---

## 10. Checklist final

### ✅ Infraestructura

- [ ] Cluster DOKS creado
- [ ] Namespace `okla` existe
- [ ] ImagePullSecret configurado
- [ ] PostgreSQL deployed con PVC
- [ ] Redis deployed
- [ ] RabbitMQ deployed

### ✅ Servicios

- [ ] Gateway deployed (2 réplicas)
- [ ] AuthService deployed
- [ ] UserService deployed
- [ ] VehiclesSaleService deployed
- [ ] MediaService deployed
- [ ] NotificationService deployed
- [ ] BillingService deployed
- [ ] ErrorService deployed
- [ ] RoleService deployed
- [ ] Frontend-web deployed

### ✅ Networking

- [ ] ingress-nginx instalado
- [ ] LoadBalancer tiene IP externa
- [ ] DNS configurado (okla.com.do → LoadBalancer IP)
- [ ] DNS configurado (api.okla.com.do → LoadBalancer IP)
- [ ] cert-manager instalado
- [ ] ClusterIssuer creado
- [ ] Ingress creado
- [ ] Certificado SSL emitido (READY=True)

### ✅ Gateway

- [ ] ocelot.json montado via ConfigMap
- [ ] Rutas para auth, users, vehicles configuradas
- [ ] Gateway responde en /health

### ✅ CI/CD

- [ ] Secrets configurados en GitHub
- [ ] Workflows existen (.github/workflows/)
- [ ] GitHub Actions ejecutado exitosamente
- [ ] Imágenes en GHCR

### ✅ Verificación

- [ ] https://okla.com.do retorna 200
- [ ] https://api.okla.com.do/health retorna 200
- [ ] https://api.okla.com.do/api/vehicles retorna datos
- [ ] SSL funciona (🔒 en browser)
- [ ] Todos los pods en estado Running
- [ ] No hay eventos de tipo Warning

### 🎉 Deploy Completo

```bash
curl -I https://okla.com.do
curl -I https://api.okla.com.do/health
curl https://api.okla.com.do/api/vehicles | jq
```

**Output esperado:**
```
HTTP/2 200 OK
content-type: application/json
...
```

---

## 🎊 ¡FELICIDADES! 🎊

Has completado el deploy completo de OKLA a producción. Ahora tienes:

- ✅ Cluster Kubernetes en Digital Ocean
- ✅ 13 microservicios desplegados
- ✅ Frontend React 19 en producción
- ✅ API Gateway con Ocelot
- ✅ PostgreSQL con persistencia
- ✅ SSL con Let's Encrypt
- ✅ DNS configurado
- ✅ CI/CD automático
- ✅ Zero-downtime deployments
- ✅ Monitoreo básico

### Próximos pasos (opcional)

1. **Monitoreo avanzado:** Prometheus + Grafana
2. **Logging:** ELK Stack o Loki
3. **Backups automatizados:** Velero
4. **Auto-scaling:** HPA (Horizontal Pod Autoscaler)
5. **Service Mesh:** Istio o Linkerd
6. **Seguridad:** NetworkPolicies, Pod Security Standards

---

**Anterior:** [14 - Monitoreo Básico](./14-monitoreo-basico.md)  
**Inicio:** [01 - kubectl básico](./01-kubectl-basico.md)
