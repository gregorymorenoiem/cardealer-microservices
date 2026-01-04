# 🚀 Guía de Setup: CI/CD con Digital Ocean Kubernetes

## 📋 Resumen de Arquitectura

```
┌─────────────────────────────────────────────────────────────────────────┐
│                        GitHub Actions CI/CD                             │
├─────────────────────────────────────────────────────────────────────────┤
│  PR → pr-checks.yml (3-5 min)                                          │
│   └─> Lint, Type Check, Unit Tests                                      │
│                                                                         │
│  Push Main → smart-cicd.yml (5-15 min)                                 │
│   └─> Detect Changes → Build Changed Services → Docker Push             │
│                                                                         │
│  CI Success → deploy-digitalocean.yml                                   │
│   └─> Configure kubectl → Apply K8s manifests → Health Check           │
└─────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                    Digital Ocean Kubernetes (DOKS)                      │
├─────────────────────────────────────────────────────────────────────────┤
│  ┌─────────────┐    ┌─────────────┐    ┌─────────────────────────────┐ │
│  │   Ingress   │───▶│   Gateway   │───▶│  Microservices (10+)        │ │
│  │ (nginx LB)  │    │  (Ocelot)   │    │  AuthService, UserService   │ │
│  └─────────────┘    └─────────────┘    │  VehiclesSaleService, etc.  │ │
│         │                              └─────────────────────────────┘ │
│         ▼                                           │                   │
│  ┌─────────────┐                       ┌────────────▼────────────────┐ │
│  │  Frontend   │                       │   DO Managed PostgreSQL     │ │
│  │   (React)   │                       │   DO Managed Redis          │ │
│  └─────────────┘                       └─────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 💰 Costos Estimados Digital Ocean

### Opción 1: Desarrollo/Staging (Mínimo) - ~$60/mes

| Recurso | Especificación | Costo/mes |
|---------|----------------|-----------|
| **DOKS Cluster** | Basic, 2 nodes (2GB RAM, 1 vCPU) | $24 |
| **Managed PostgreSQL** | Basic, 1GB RAM | $15 |
| **Managed Redis** | Basic, 1GB RAM | $15 |
| **Load Balancer** | Basic | $12 |
| **Spaces (S3)** | 250GB storage | $5 |
| **Total** | | **~$71/mes** |

### Opción 2: Producción (Recomendado) - ~$150/mes

| Recurso | Especificación | Costo/mes |
|---------|----------------|-----------|
| **DOKS Cluster** | Professional, 3 nodes (4GB RAM, 2 vCPU) | $72 |
| **Managed PostgreSQL** | Standard, 4GB RAM, HA | $40 |
| **Managed Redis** | Standard, 2GB RAM | $20 |
| **Load Balancer** | Standard | $12 |
| **Spaces (S3)** | 500GB + CDN | $10 |
| **Total** | | **~$154/mes** |

### Opción 3: Escala (Alto tráfico) - ~$400/mes

| Recurso | Especificación | Costo/mes |
|---------|----------------|-----------|
| **DOKS Cluster** | Professional, 5 nodes (8GB RAM, 4 vCPU) | $200 |
| **Managed PostgreSQL** | Standard, 8GB RAM, HA, Read Replicas | $100 |
| **Managed Redis** | Standard, 4GB RAM, Cluster | $40 |
| **Load Balancer** | Professional | $24 |
| **Spaces (S3)** | 1TB + CDN | $20 |
| **Total** | | **~$384/mes** |

---

## 🔧 Setup Paso a Paso

### 1. Crear Cluster en Digital Ocean

```bash
# Instalar doctl CLI
brew install doctl

# Autenticar
doctl auth init

# Crear cluster Kubernetes
doctl kubernetes cluster create cardealer-cluster \
  --region nyc1 \
  --size s-2vcpu-4gb \
  --count 3 \
  --tag cardealer

# Configurar kubectl
doctl kubernetes cluster kubeconfig save cardealer-cluster
```

### 2. Instalar NGINX Ingress Controller

```bash
# Agregar repo helm
helm repo add ingress-nginx https://kubernetes.github.io/ingress-nginx
helm repo update

# Instalar
helm install ingress-nginx ingress-nginx/ingress-nginx \
  --namespace ingress-nginx \
  --create-namespace \
  --set controller.service.annotations."service\.beta\.kubernetes\.io/do-loadbalancer-name"="cardealer-lb"
```

### 3. Instalar Cert-Manager (SSL automático)

```bash
# Agregar repo
helm repo add jetstack https://charts.jetstack.io
helm repo update

# Instalar
helm install cert-manager jetstack/cert-manager \
  --namespace cert-manager \
  --create-namespace \
  --set installCRDs=true
```

### 4. Crear Managed PostgreSQL

```bash
doctl databases create cardealer-db \
  --engine pg \
  --version 16 \
  --size db-s-1vcpu-1gb \
  --region nyc1 \
  --num-nodes 1
```

### 5. Crear Managed Redis

```bash
doctl databases create cardealer-redis \
  --engine redis \
  --version 7 \
  --size db-s-1vcpu-1gb \
  --region nyc1 \
  --num-nodes 1
```

### 6. Configurar GitHub Secrets

En tu repositorio de GitHub, ve a **Settings > Secrets and variables > Actions** y agrega:

| Secret | Descripción |
|--------|-------------|
| `DIGITALOCEAN_ACCESS_TOKEN` | Token de API de Digital Ocean |
| `DO_CLUSTER_NAME` | Nombre del cluster (ej: cardealer-cluster) |
| `POSTGRES_HOST` | Host de PostgreSQL (de DO dashboard) |
| `POSTGRES_PORT` | 25060 (default DO) |
| `POSTGRES_USER` | doadmin (default) |
| `POSTGRES_PASSWORD` | Password de PostgreSQL |
| `JWT_SECRET_KEY` | Clave secreta para JWT (64+ chars) |
| `STRIPE_SECRET_KEY` | Clave de Stripe |
| `SENDGRID_API_KEY` | API key de SendGrid |
| `AWS_ACCESS_KEY_ID` | Para S3 storage |
| `AWS_SECRET_ACCESS_KEY` | Para S3 storage |

### 7. Desplegar

```bash
# Aplicar manifiestos manualmente (primera vez)
kubectl apply -f k8s/namespace.yaml
kubectl apply -f k8s/configmaps.yaml
kubectl apply -f k8s/secrets.yaml
kubectl apply -f k8s/deployments.yaml
kubectl apply -f k8s/services.yaml
kubectl apply -f k8s/ingress.yaml
kubectl apply -f k8s/hpa.yaml
kubectl apply -f k8s/pdb.yaml

# O usar GitHub Actions (automático)
# Push a main y el workflow se ejecuta automáticamente
```

---

## 📁 Estructura de Archivos Creados

```
.github/workflows/
├── _reusable-dotnet-service.yml  # Template reutilizable .NET
├── _reusable-frontend.yml        # Template reutilizable Frontend
├── smart-cicd.yml                # CI/CD con Smart Triggers
├── pr-checks.yml                 # Validación de PRs (optimizado)
├── deploy-digitalocean.yml       # Deploy a DOKS
└── test.yml                      # (existente)

k8s/
├── namespace.yaml                # Namespace cardealer
├── configmaps.yaml               # Configuración de apps
├── secrets.yaml                  # Template de secretos
├── deployments.yaml              # Deployments de servicios
├── services.yaml                 # ClusterIP services
├── ingress.yaml                  # NGINX Ingress + TLS
├── hpa.yaml                      # Horizontal Pod Autoscaler
└── pdb.yaml                      # Pod Disruption Budget
```

---

## 📊 Ahorro de Costos CI/CD

| Estrategia | Sin Optimizar | Con Smart Triggers |
|------------|---------------|-------------------|
| **Minutos/push** | ~180 min | ~20 min |
| **Pushes/mes** | 40 | 40 |
| **Total minutos** | 7,200 min | 800 min |
| **Costo (exceso)** | $41.60 | **$0** (dentro del gratis) |

### Técnicas Implementadas:

1. ✅ **Smart Triggers** - Solo ejecuta CI para servicios que cambiaron
2. ✅ **Cache agresivo** - NuGet y npm packages cacheados
3. ✅ **Parallel jobs** - Frontend y Backend en paralelo
4. ✅ **Conditional Docker** - Solo push en main
5. ✅ **PR checks ligeros** - Solo lint + unit tests
6. ✅ **Concurrency groups** - Cancela ejecuciones duplicadas

---

## 🔐 Seguridad

1. **Secrets Management**
   - Usar GitHub Encrypted Secrets
   - En producción: Considerar HashiCorp Vault o DO Secrets

2. **Container Security**
   - Trivy scanner incluido en CI
   - Imágenes base oficiales de Microsoft

3. **Network Policies**
   - Ingress solo permite tráfico a gateway
   - Services solo accesibles internamente

4. **RBAC**
   - Service accounts específicos por deployment
   - Mínimos privilegios

---

## 🚨 Comandos Útiles

```bash
# Ver pods
kubectl get pods -n cardealer

# Ver logs de un pod
kubectl logs -f deployment/authservice -n cardealer

# Ejecutar comando en pod
kubectl exec -it deployment/authservice -n cardealer -- sh

# Ver recursos
kubectl top pods -n cardealer

# Rollback
kubectl rollout undo deployment/authservice -n cardealer

# Escalar manualmente
kubectl scale deployment/authservice --replicas=3 -n cardealer

# Ver eventos
kubectl get events -n cardealer --sort-by='.lastTimestamp'

# Port forward para debug
kubectl port-forward svc/authservice 8080:80 -n cardealer
```

---

## ✅ Checklist de Deploy

- [ ] Cluster DOKS creado
- [ ] NGINX Ingress instalado
- [ ] Cert-Manager instalado
- [ ] PostgreSQL managed creado
- [ ] Redis managed creado
- [ ] DNS configurado (cardealer.do → Load Balancer IP)
- [ ] GitHub Secrets configurados
- [ ] Primera build de imágenes Docker
- [ ] Manifiestos aplicados
- [ ] SSL certificado emitido
- [ ] Health checks pasando
