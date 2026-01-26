# 🚀 Deployment - Estrategia de Despliegue - Matriz de Procesos

> **Plataforma:** Digital Ocean Kubernetes (DOKS)  
> **CI/CD:** GitHub Actions  
> **Registry:** GitHub Container Registry (ghcr.io)  
> **Última actualización:** Enero 25, 2026  
> **Estado de Implementación:** ✅ 75% Backend | N/A UI (GitHub/K8s dashboards)

---

## ⚠️ AUDITORÍA DE ACCESO UI (Enero 25, 2026)

| Proceso           | Backend | UI Access | Observación             |
| ----------------- | ------- | --------- | ----------------------- |
| CI/CD Pipelines   | ✅ 100% | N/A       | GitHub Actions UI       |
| Kubernetes Deploy | ✅ 100% | N/A       | kubectl + K8s Dashboard |
| Rolling Updates   | ✅ 100% | N/A       | Nativo de K8s           |
| Blue/Green        | 🔴 0%   | N/A       | Pendiente               |
| Canary Releases   | 🔴 0%   | N/A       | Pendiente               |

### Rutas UI Existentes ✅

- GitHub Actions: github.com/gregorymorenoiem/cardealer-microservices/actions
- Kubernetes Dashboard (externo)

### Rutas UI Faltantes 🔴

- Ninguna interna requerida - herramientas externas suficientes

**Verificación:** Workflows en `.github/workflows/` + manifests en `/k8s/` ✅

---

## 📊 Resumen de Implementación

| Componente                           | Total | Implementado | Pendiente | Estado       |
| ------------------------------------ | ----- | ------------ | --------- | ------------ |
| **DEPLOY-CICD-\*** (CI/CD)           | 5     | 5            | 0         | ✅ 100%      |
| **DEPLOY-K8S-\*** (Kubernetes)       | 6     | 6            | 0         | ✅ 100%      |
| **DEPLOY-ROLL-\*** (Rolling)         | 4     | 4            | 0         | ✅ 100%      |
| **DEPLOY-BLUEGREEN-\*** (Blue/Green) | 3     | 0            | 3         | 🔴 Pendiente |
| **DEPLOY-CANARY-\*** (Canary)        | 3     | 0            | 3         | 🔴 Pendiente |
| **Tests**                            | 15    | 12           | 3         | 🟢 80%       |
| **TOTAL**                            | 36    | 27           | 9         | 🟢 75%       |

---

## 1. Información General

### 1.1 Descripción

Sistema de despliegue automatizado para la plataforma OKLA utilizando GitOps con GitHub Actions para CI/CD, imágenes Docker en GitHub Container Registry, y orquestación en Kubernetes (DOKS).

### 1.2 Arquitectura de Deployment

```
┌─────────────────────────────────────────────────────────────────────────┐
│                        Deployment Pipeline                               │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│   Developer                  GitHub                    Production        │
│   ┌───────┐              ┌─────────────┐           ┌───────────────┐   │
│   │  git  │──── push ────│  Repository │           │    DOKS       │   │
│   │ push  │              │             │           │   Cluster     │   │
│   └───────┘              └──────┬──────┘           │               │   │
│                                 │                   │  ┌─────────┐ │   │
│                         GitHub Actions              │  │ Gateway │ │   │
│                     ┌───────────┴───────────┐      │  └─────────┘ │   │
│                     │                       │      │  ┌─────────┐ │   │
│                     ▼                       ▼      │  │Services │ │   │
│               ┌──────────┐           ┌──────────┐ │  └─────────┘ │   │
│               │  Build   │           │  Deploy  │ │  ┌─────────┐ │   │
│               │  & Test  │──────────▶│  to K8s  │─┼─▶│ Postgres│ │   │
│               └──────────┘           └──────────┘ │  └─────────┘ │   │
│                     │                              │  ┌─────────┐ │   │
│                     ▼                              │  │  Redis  │ │   │
│               ┌──────────┐                         │  └─────────┘ │   │
│               │   Push   │                         └───────────────┘   │
│               │ to GHCR  │                                              │
│               └──────────┘                                              │
│                                                                          │
│   Environments:                                                         │
│   ├── development (local docker-compose)                                │
│   ├── staging (future - DOKS staging namespace)                        │
│   └── production (DOKS okla namespace)                                  │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 2. Ambientes

### 2.1 Development (Local)

```bash
# Levantar todos los servicios localmente
docker-compose up -d

# Ver logs
docker-compose logs -f gateway vehiclessaleservice

# Reconstruir un servicio específico
docker-compose build --no-cache vehiclessaleservice
docker-compose up -d vehiclessaleservice
```

### 2.2 Production (DOKS)

| Recurso              | Valor          |
| -------------------- | -------------- |
| **Cluster**          | okla-cluster   |
| **Namespace**        | okla           |
| **Region**           | nyc1           |
| **Node Pool**        | 3x s-2vcpu-4gb |
| **Load Balancer IP** | 146.190.199.0  |

### 2.3 URLs de Producción

| Servicio     | URL                            |
| ------------ | ------------------------------ |
| Frontend     | https://okla.com.do            |
| API Gateway  | https://api.okla.com.do        |
| Health Check | https://api.okla.com.do/health |

---

## 3. GitHub Actions Workflows

### 3.1 Smart CI/CD Workflow

```yaml
# .github/workflows/smart-cicd.yml
name: Smart CI/CD

on:
  push:
    branches: [main, development]
  pull_request:
    branches: [main]

env:
  REGISTRY: ghcr.io
  SERVICES: "frontend-web,gateway,authservice,userservice,vehiclessaleservice,mediaservice,notificationservice,billingservice,errorservice,roleservice"

jobs:
  detect-changes:
    runs-on: ubuntu-latest
    outputs:
      services: ${{ steps.changes.outputs.services }}
    steps:
      - uses: actions/checkout@v4
        with:
          fetch-depth: 0

      - name: Detect changed services
        id: changes
        run: |
          CHANGED_SERVICES=""

          # Check each service for changes
          for service in $(echo $SERVICES | tr ',' ' '); do
            case $service in
              frontend-web)
                path="frontend/web"
                ;;
              gateway)
                path="backend/Gateway"
                ;;
              *)
                # Convert service name to path
                service_pascal=$(echo $service | sed -r 's/(^|-)(\w)/\U\2/g')
                path="backend/${service_pascal}"
                ;;
            esac
            
            if git diff --name-only ${{ github.event.before }} ${{ github.sha }} | grep -q "^$path/"; then
              CHANGED_SERVICES="$CHANGED_SERVICES,$service"
            fi
          done

          echo "services=${CHANGED_SERVICES#,}" >> $GITHUB_OUTPUT

  build-and-push:
    needs: detect-changes
    if: needs.detect-changes.outputs.services != ''
    runs-on: ubuntu-latest
    strategy:
      matrix:
        service: ${{ fromJson(needs.detect-changes.outputs.services) }}

    steps:
      - uses: actions/checkout@v4

      - name: Set up Docker Buildx
        uses: docker/setup-buildx-action@v3

      - name: Login to GHCR
        uses: docker/login-action@v3
        with:
          registry: ${{ env.REGISTRY }}
          username: ${{ github.actor }}
          password: ${{ secrets.GITHUB_TOKEN }}

      - name: Build and push
        uses: docker/build-push-action@v5
        with:
          context: ${{ matrix.service == 'frontend-web' && 'frontend/web' || format('backend/{0}', matrix.service) }}
          push: ${{ github.event_name != 'pull_request' }}
          tags: |
            ${{ env.REGISTRY }}/${{ github.repository_owner }}/cardealer-${{ matrix.service }}:latest
            ${{ env.REGISTRY }}/${{ github.repository_owner }}/cardealer-${{ matrix.service }}:${{ github.sha }}
          cache-from: type=gha
          cache-to: type=gha,mode=max

  deploy:
    needs: build-and-push
    if: github.ref == 'refs/heads/main'
    runs-on: ubuntu-latest

    steps:
      - uses: actions/checkout@v4

      - name: Install doctl
        uses: digitalocean/action-doctl@v2
        with:
          token: ${{ secrets.DIGITALOCEAN_ACCESS_TOKEN }}

      - name: Configure kubectl
        run: doctl kubernetes cluster kubeconfig save okla-cluster

      - name: Update deployments
        run: |
          for service in $(echo "${{ needs.detect-changes.outputs.services }}" | tr ',' ' '); do
            kubectl set image deployment/$service \
              $service=${{ env.REGISTRY }}/${{ github.repository_owner }}/cardealer-$service:${{ github.sha }} \
              -n okla
          done

      - name: Wait for rollout
        run: |
          for service in $(echo "${{ needs.detect-changes.outputs.services }}" | tr ',' ' '); do
            kubectl rollout status deployment/$service -n okla --timeout=300s
          done
```

### 3.2 Deploy to Digital Ocean Workflow

```yaml
# .github/workflows/deploy-digitalocean.yml
name: Deploy to Digital Ocean

on:
  workflow_dispatch:
    inputs:
      services:
        description: 'Services to deploy (comma-separated or "all")'
        required: true
        default: "all"

jobs:
  deploy:
    runs-on: ubuntu-latest

    steps:
      - uses: actions/checkout@v4

      - name: Install doctl
        uses: digitalocean/action-doctl@v2
        with:
          token: ${{ secrets.DIGITALOCEAN_ACCESS_TOKEN }}

      - name: Configure kubectl
        run: doctl kubernetes cluster kubeconfig save okla-cluster

      - name: Apply Kubernetes manifests
        run: |
          kubectl apply -f k8s/namespace.yaml
          kubectl apply -f k8s/configmaps.yaml
          kubectl apply -f k8s/secrets.yaml
          kubectl apply -f k8s/deployments.yaml
          kubectl apply -f k8s/services.yaml
          kubectl apply -f k8s/ingress.yaml

      - name: Rollout restart
        run: |
          if [ "${{ github.event.inputs.services }}" = "all" ]; then
            kubectl rollout restart deployment -n okla
          else
            for service in $(echo "${{ github.event.inputs.services }}" | tr ',' ' '); do
              kubectl rollout restart deployment/$service -n okla
            done
          fi

      - name: Wait for rollout
        run: |
          kubectl rollout status deployment -n okla --timeout=600s

      - name: Verify health
        run: |
          sleep 30
          curl -f https://api.okla.com.do/health || exit 1
```

---

## 4. Kubernetes Manifests

### 4.1 Namespace

```yaml
# k8s/namespace.yaml
apiVersion: v1
kind: Namespace
metadata:
  name: okla
  labels:
    name: okla
    environment: production
```

### 4.2 Deployment Template

```yaml
# k8s/deployments.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: vehiclessaleservice
  namespace: okla
  labels:
    app: vehiclessaleservice
    version: v1
spec:
  replicas: 2
  selector:
    matchLabels:
      app: vehiclessaleservice
  strategy:
    type: RollingUpdate
    rollingUpdate:
      maxSurge: 1
      maxUnavailable: 0
  template:
    metadata:
      labels:
        app: vehiclessaleservice
        version: v1
    spec:
      containers:
        - name: vehiclessaleservice
          image: ghcr.io/gregorymorenoiem/cardealer-vehiclessaleservice:latest
          imagePullPolicy: Always
          ports:
            - containerPort: 8080
              protocol: TCP
          env:
            - name: ASPNETCORE_ENVIRONMENT
              value: "Production"
            - name: ConnectionStrings__DefaultConnection
              valueFrom:
                secretKeyRef:
                  name: database-secrets
                  key: vehiclessaleservice-connection
            - name: Redis__ConnectionString
              valueFrom:
                secretKeyRef:
                  name: redis-secrets
                  key: connection-string
            - name: RabbitMQ__Host
              value: "rabbitmq.okla.svc.cluster.local"
          resources:
            requests:
              memory: "256Mi"
              cpu: "100m"
            limits:
              memory: "512Mi"
              cpu: "500m"
          startupProbe:
            httpGet:
              path: /health/startup
              port: 8080
            initialDelaySeconds: 10
            periodSeconds: 10
            failureThreshold: 30
          livenessProbe:
            httpGet:
              path: /health/live
              port: 8080
            periodSeconds: 10
            failureThreshold: 3
          readinessProbe:
            httpGet:
              path: /health/ready
              port: 8080
            periodSeconds: 5
            failureThreshold: 3
      imagePullSecrets:
        - name: ghcr-secret
```

### 4.3 Service

```yaml
# k8s/services.yaml
apiVersion: v1
kind: Service
metadata:
  name: vehiclessaleservice
  namespace: okla
spec:
  type: ClusterIP
  selector:
    app: vehiclessaleservice
  ports:
    - port: 8080
      targetPort: 8080
      protocol: TCP
```

### 4.4 Ingress

```yaml
# k8s/ingress.yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: okla-ingress
  namespace: okla
  annotations:
    kubernetes.io/ingress.class: nginx
    cert-manager.io/cluster-issuer: letsencrypt-prod
    nginx.ingress.kubernetes.io/ssl-redirect: "true"
    nginx.ingress.kubernetes.io/proxy-body-size: "100m"
spec:
  tls:
    - hosts:
        - okla.com.do
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

---

## 5. Rolling Update Strategy

### 5.1 Zero-Downtime Deployment

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    Rolling Update Process                                │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│   Step 1: Initial State (2 replicas)                                   │
│   ┌─────────────┐  ┌─────────────┐                                     │
│   │   Pod v1    │  │   Pod v1    │                                     │
│   │  (healthy)  │  │  (healthy)  │                                     │
│   └─────────────┘  └─────────────┘                                     │
│         ▲                ▲                                              │
│         └────────────────┘                                              │
│               Service                                                   │
│                                                                          │
│   Step 2: Create new pod                                                │
│   ┌─────────────┐  ┌─────────────┐  ┌─────────────┐                    │
│   │   Pod v1    │  │   Pod v1    │  │   Pod v2    │                    │
│   │  (healthy)  │  │  (healthy)  │  │  (starting) │                    │
│   └─────────────┘  └─────────────┘  └─────────────┘                    │
│         ▲                ▲                                              │
│         └────────────────┘                                              │
│               Service (still v1 only)                                   │
│                                                                          │
│   Step 3: New pod ready, traffic shifted                               │
│   ┌─────────────┐  ┌─────────────┐  ┌─────────────┐                    │
│   │   Pod v1    │  │   Pod v1    │  │   Pod v2    │                    │
│   │ (draining) │  │  (healthy)  │  │  (healthy)  │                    │
│   └─────────────┘  └─────────────┘  └─────────────┘                    │
│                          ▲                ▲                             │
│                          └────────────────┘                             │
│                               Service                                   │
│                                                                          │
│   Step 4: Old pod terminated, repeat                                   │
│   ┌─────────────┐  ┌─────────────┐  ┌─────────────┐                    │
│   │   Pod v1    │  │   Pod v2    │  │   Pod v2    │                    │
│   │ (draining) │  │  (healthy)  │  │  (starting) │                    │
│   └─────────────┘  └─────────────┘  └─────────────┘                    │
│                          ▲                                              │
│                          │                                              │
│                      Service                                            │
│                                                                          │
│   Step 5: Complete                                                      │
│   ┌─────────────┐  ┌─────────────┐                                     │
│   │   Pod v2    │  │   Pod v2    │                                     │
│   │  (healthy)  │  │  (healthy)  │                                     │
│   └─────────────┘  └─────────────┘                                     │
│         ▲                ▲                                              │
│         └────────────────┘                                              │
│               Service                                                   │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

### 5.2 PodDisruptionBudget

```yaml
apiVersion: policy/v1
kind: PodDisruptionBudget
metadata:
  name: vehiclessaleservice-pdb
  namespace: okla
spec:
  minAvailable: 1
  selector:
    matchLabels:
      app: vehiclessaleservice
```

---

## 6. Comandos de Deployment

### 6.1 Comandos Frecuentes

```bash
# Conectar a cluster
doctl kubernetes cluster kubeconfig save okla-cluster

# Ver pods
kubectl get pods -n okla

# Ver logs de un servicio
kubectl logs -f deployment/gateway -n okla

# Reiniciar un deployment
kubectl rollout restart deployment/vehiclessaleservice -n okla

# Ver historial de rollouts
kubectl rollout history deployment/vehiclessaleservice -n okla

# Rollback a versión anterior
kubectl rollout undo deployment/vehiclessaleservice -n okla

# Rollback a revisión específica
kubectl rollout undo deployment/vehiclessaleservice -n okla --to-revision=2

# Ver estado del rollout
kubectl rollout status deployment/vehiclessaleservice -n okla

# Escalar replicas
kubectl scale deployment/vehiclessaleservice --replicas=3 -n okla

# Actualizar imagen manualmente
kubectl set image deployment/vehiclessaleservice \
  vehiclessaleservice=ghcr.io/gregorymorenoiem/cardealer-vehiclessaleservice:v1.2.3 \
  -n okla
```

### 6.2 Gateway ConfigMap Update

```bash
# IMPORTANTE: Actualizar config del Gateway después de cambios
kubectl delete configmap gateway-config -n okla
kubectl create configmap gateway-config \
  --from-file=ocelot.json=backend/Gateway/Gateway.Api/ocelot.prod.json \
  -n okla
kubectl rollout restart deployment/gateway -n okla
```

---

## 7. Monitoreo de Deployments

### 7.1 Prometheus Metrics

```
# Deployment replicas
kube_deployment_status_replicas{deployment="...", namespace="okla"}

# Available replicas
kube_deployment_status_replicas_available{deployment="...", namespace="okla"}

# Pod restarts
kube_pod_container_status_restarts_total{namespace="okla"}

# Container CPU/Memory
container_cpu_usage_seconds_total{namespace="okla"}
container_memory_usage_bytes{namespace="okla"}
```

### 7.2 Alertas de Deployment

```yaml
groups:
  - name: deployment-alerts
    rules:
      - alert: DeploymentRolloutStuck
        expr: kube_deployment_status_observed_generation != kube_deployment_metadata_generation
        for: 10m
        labels:
          severity: warning
        annotations:
          summary: "Deployment {{ $labels.deployment }} rollout is stuck"

      - alert: PodCrashLooping
        expr: rate(kube_pod_container_status_restarts_total[15m]) > 0
        for: 5m
        labels:
          severity: critical
        annotations:
          summary: "Pod {{ $labels.pod }} is crash looping"

      - alert: DeploymentReplicasMismatch
        expr: kube_deployment_status_replicas_available != kube_deployment_spec_replicas
        for: 10m
        labels:
          severity: warning
        annotations:
          summary: "Deployment {{ $labels.deployment }} has replica mismatch"
```

---

## 8. Secrets Management

### 8.1 Secrets en Kubernetes

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: database-secrets
  namespace: okla
type: Opaque
stringData:
  vehiclessaleservice-connection: "Host=postgres;Database=vehiclessaleservice;Username=postgres;Password=${DB_PASSWORD}"
  authservice-connection: "Host=postgres;Database=authservice;Username=postgres;Password=${DB_PASSWORD}"
```

### 8.2 GitHub Secrets Requeridos

| Secret                      | Descripción                                 |
| --------------------------- | ------------------------------------------- |
| `DIGITALOCEAN_ACCESS_TOKEN` | Token API de DigitalOcean                   |
| `GHCR_TOKEN`                | Token para ghcr.io (puede ser GITHUB_TOKEN) |
| `KUBE_CONFIG`               | kubeconfig del cluster (base64)             |
| `DB_PASSWORD`               | Password de PostgreSQL                      |
| `REDIS_PASSWORD`            | Password de Redis                           |
| `JWT_SECRET`                | Secret para tokens JWT                      |

---

## 9. Dockerfile Best Practices

### 9.1 Multi-stage Build (.NET)

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY ["VehiclesSaleService.Api/VehiclesSaleService.Api.csproj", "VehiclesSaleService.Api/"]
COPY ["VehiclesSaleService.Application/VehiclesSaleService.Application.csproj", "VehiclesSaleService.Application/"]
COPY ["VehiclesSaleService.Domain/VehiclesSaleService.Domain.csproj", "VehiclesSaleService.Domain/"]
COPY ["VehiclesSaleService.Infrastructure/VehiclesSaleService.Infrastructure.csproj", "VehiclesSaleService.Infrastructure/"]
RUN dotnet restore "VehiclesSaleService.Api/VehiclesSaleService.Api.csproj"

# Copy everything else and build
COPY . .
RUN dotnet publish "VehiclesSaleService.Api/VehiclesSaleService.Api.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create non-root user
RUN adduser --disabled-password --gecos '' appuser
USER appuser

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "VehiclesSaleService.Api.dll"]
```

### 9.2 Frontend (React + Nginx)

```dockerfile
# Build stage
FROM node:20-alpine AS build
WORKDIR /app

COPY package*.json ./
RUN npm ci

COPY . .
RUN npm run build

# Runtime stage
FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/nginx.conf

EXPOSE 8080
CMD ["nginx", "-g", "daemon off;"]
```

---

## 📚 Referencias

- [docs/tutorials/](../../tutorials/) - 15 tutoriales de deployment
- [DOKS Documentation](https://docs.digitalocean.com/products/kubernetes/) - Digital Ocean Kubernetes
- [GitHub Actions](https://docs.github.com/en/actions) - CI/CD
