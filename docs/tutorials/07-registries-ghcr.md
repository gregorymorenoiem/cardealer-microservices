# Tutorial 7: GitHub Container Registry (GHCR)

**Duración:** 40 minutos  
**Nivel:** Intermedio  
**Requisitos:** Tutorial 6 completado, cuenta GitHub

---

## 📋 Resumen

Aprenderás a usar GitHub Container Registry para almacenar imágenes Docker de tus microservicios. Configurarás GitHub Actions para build automático y Kubernetes para pull de imágenes privadas.

### Lo que aprenderás:
- Qué es un Container Registry
- Configurar GitHub Container Registry (ghcr.io)
- Build y push de imágenes Docker
- Autenticación con GHCR desde Kubernetes
- Pull de imágenes privadas en deployments
- Gestión de tags y versionado

---

## 📑 Índice

1. [Conceptos: Container Registry](#1-conceptos-container-registry)
2. [Configurar GHCR](#2-configurar-ghcr)
3. [Crear Personal Access Token](#3-crear-personal-access-token)
4. [Build y push manual](#4-build-y-push-manual)
5. [Pull de imágenes en Kubernetes](#5-pull-de-imágenes-en-kubernetes)
6. [Crear ImagePullSecret](#6-crear-imagepullsecret)
7. [Automatización con GitHub Actions](#7-automatización-con-github-actions)
8. [Gestión de tags](#8-gestión-de-tags)
9. [Cheat Sheet](#9-cheat-sheet)
10. [Ejercicios prácticos](#10-ejercicios-prácticos)

---

## 1. Conceptos: Container Registry

### ¿Qué es un Container Registry?

Un **Container Registry** es un repositorio centralizado para almacenar imágenes Docker/OCI.

**Analogía:** Como GitHub para código, pero para imágenes Docker.

```
Docker Build → Docker Push → Registry → Kubernetes Pull → Deploy
```

### Registries más populares

| Registry | URL | Características |
|----------|-----|-----------------|
| **Docker Hub** | hub.docker.com | Gratis (públicas), límites en pulls |
| **GitHub Container Registry** | ghcr.io | Gratis ilimitado, integrado con GitHub |
| **Google Container Registry** | gcr.io | Integrado con GCP |
| **AWS Elastic Container Registry** | ecr.aws | Integrado con AWS |
| **Azure Container Registry** | azurecr.io | Integrado con Azure |
| **DigitalOcean Container Registry** | registry.digitalocean.com | $5/mes en DO |

**En OKLA usamos GHCR** (ghcr.io).

### Ventajas de GHCR

- ✅ **Gratis** - Sin límites de storage o bandwidth
- ✅ **Integrado con GitHub** - Mismo login, mismos permisos
- ✅ **Privado por defecto** - Imágenes privadas incluidas
- ✅ **CI/CD nativo** - Fácil con GitHub Actions
- ✅ **Multi-arquitectura** - Soporta AMD64, ARM64

### Estructura de una imagen en GHCR

```
ghcr.io/<username>/<image-name>:<tag>

Ejemplos:
ghcr.io/gregorymorenoiem/cardealer-gateway:latest
ghcr.io/gregorymorenoiem/cardealer-gateway:v1.2.0
ghcr.io/gregorymorenoiem/cardealer-vehiclessaleservice:2026-01-07-abc123
```

---

## 2. Configurar GHCR

### Paso 1: Habilitar GitHub Container Registry

GHCR ya está habilitado por defecto en GitHub (desde 2021).

### Paso 2: Ver paquetes existentes

1. Ve a tu perfil GitHub
2. Click en "Packages"
3. Verás las imágenes ya publicadas

**URL directa:**
```
https://github.com/yourusername?tab=packages
```

**Para OKLA:**
```
https://github.com/gregorymorenoiem?tab=packages
```

Verás:
- cardealer-gateway
- cardealer-authservice
- cardealer-vehiclessaleservice
- ... (todos los microservicios)

### Paso 3: Visibilidad de paquetes

Por defecto, las imágenes son **privadas** (solo tu puedes acceder).

**Para hacer una imagen pública:**
1. Ve a Package settings
2. Scroll hasta "Danger Zone"
3. Click "Change visibility" → Public

**⚠️ En producción, mantén las imágenes privadas.**

---

## 3. Crear Personal Access Token

### ¿Por qué necesitas un token?

Para push/pull imágenes a GHCR, necesitas autenticarte. GitHub usa **Personal Access Tokens** (PAT).

### Crear token en GitHub

1. Ve a: https://github.com/settings/tokens
2. Click "Generate new token" → "Generate new token (classic)"
3. Configuración:
   - **Note:** `GHCR Access for OKLA`
   - **Expiration:** 1 year (o No expiration si prefieres)
   - **Scopes:** ✅ `write:packages`, ✅ `read:packages`, ✅ `delete:packages`
4. Click "Generate token"
5. **¡COPIA EL TOKEN!** No podrás verlo de nuevo.

**Ejemplo de token:**
```
ghp_abcdefghijklmnopqrstuvwxyz1234567890
```

### Guardar token de forma segura

```bash
# Guarda el token en variable de entorno (temporal)
export GHCR_TOKEN=ghp_abcdefghijklmnopqrstuvwxyz1234567890

# O guárdalo en archivo (NO commits)
echo $GHCR_TOKEN > ~/.ghcr-token
chmod 600 ~/.ghcr-token
```

---

## 4. Build y push manual

### Paso 1: Login en GHCR con Docker

```bash
# Usando token guardado en variable
echo $GHCR_TOKEN | docker login ghcr.io -u yourusername --password-stdin

# O manualmente
docker login ghcr.io -u yourusername
# Password: (pega tu token)
```

**Salida exitosa:**
```
Login Succeeded
```

**Credenciales guardadas en:** `~/.docker/config.json`

### Paso 2: Build de imagen Docker

```bash
cd backend/Gateway/Gateway.Api

docker build -t ghcr.io/gregorymorenoiem/cardealer-gateway:latest .
```

**¿Qué hace?**
1. Lee el `Dockerfile` en el directorio
2. Ejecuta las instrucciones (FROM, COPY, RUN, etc.)
3. Crea imagen con el tag especificado

**Salida típica:**
```
[+] Building 45.2s (15/15) FINISHED
 => [internal] load build definition from Dockerfile
 => => transferring dockerfile: 543B
 => [internal] load .dockerignore
 => [1/9] FROM mcr.microsoft.com/dotnet/aspnet:8.0
 => [2/9] WORKDIR /app
 => [3/9] COPY bin/Release/net8.0/publish .
 ...
 => exporting to image
 => => naming to ghcr.io/gregorymorenoiem/cardealer-gateway:latest
```

### Paso 3: Verificar imagen local

```bash
docker images | grep gateway
```

**Salida:**
```
ghcr.io/gregorymorenoiem/cardealer-gateway   latest   a1b2c3d4e5f6   2 minutes ago   250MB
```

### Paso 4: Push a GHCR

```bash
docker push ghcr.io/gregorymorenoiem/cardealer-gateway:latest
```

**Salida:**
```
The push refers to repository [ghcr.io/gregorymorenoiem/cardealer-gateway]
abc123: Pushed
def456: Pushed
...
latest: digest: sha256:abc123... size: 2415
```

### Paso 5: Verificar en GitHub

```
https://github.com/yourusername?tab=packages
```

Deberías ver `cardealer-gateway` con el tag `latest`.

### Build y push de múltiples servicios

```bash
# Script para build y push de todos los servicios
SERVICES="gateway authservice vehiclessaleservice"

for SERVICE in $SERVICES; do
  echo "Building $SERVICE..."
  cd backend/${SERVICE^}/${SERVICE^}.Api
  docker build -t ghcr.io/gregorymorenoiem/cardealer-$SERVICE:latest .
  docker push ghcr.io/gregorymorenoiem/cardealer-$SERVICE:latest
  cd ../../..
done
```

---

## 5. Pull de imágenes en Kubernetes

### Imágenes públicas (sin autenticación)

Si la imagen es pública en GHCR:

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: gateway
spec:
  template:
    spec:
      containers:
      - name: gateway
        image: ghcr.io/gregorymorenoiem/cardealer-gateway:latest
```

Kubernetes descargará la imagen automáticamente.

### Imágenes privadas (requiere ImagePullSecret)

Si la imagen es privada:

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: gateway
spec:
  template:
    spec:
      containers:
      - name: gateway
        image: ghcr.io/gregorymorenoiem/cardealer-gateway:latest
      imagePullSecrets:
      - name: ghcr-secret  # Secret con credenciales
```

**Error sin ImagePullSecret:**
```
Failed to pull image "ghcr.io/...": pull access denied
```

---

## 6. Crear ImagePullSecret

### ¿Qué es un ImagePullSecret?

Un **ImagePullSecret** es un Secret tipo `kubernetes.io/dockerconfigjson` que contiene credenciales para pull de imágenes privadas.

### Crear ImagePullSecret desde línea de comandos

```bash
kubectl create secret docker-registry ghcr-secret \
  --docker-server=ghcr.io \
  --docker-username=yourusername \
  --docker-password=$GHCR_TOKEN \
  --docker-email=your-email@example.com \
  -n okla
```

**Salida:**
```
secret/ghcr-secret created
```

### Verificar Secret

```bash
kubectl get secret ghcr-secret -n okla
```

**Salida:**
```
NAME          TYPE                             DATA   AGE
ghcr-secret   kubernetes.io/dockerconfigjson   1      30s
```

### Ver contenido del Secret (base64)

```bash
kubectl get secret ghcr-secret -n okla -o yaml
```

**Salida:**
```yaml
apiVersion: v1
kind: Secret
metadata:
  name: ghcr-secret
  namespace: okla
type: kubernetes.io/dockerconfigjson
data:
  .dockerconfigjson: eyJhdXRocyI6eyJnaGNyLmlvIjp7InVzZXJuYW1lIjoiLi4uIiwicGFzc3dvcmQiOiIuLi4ifX19
```

### Crear ImagePullSecret desde archivo YAML

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: ghcr-secret
  namespace: okla
type: kubernetes.io/dockerconfigjson
stringData:
  .dockerconfigjson: |
    {
      "auths": {
        "ghcr.io": {
          "username": "yourusername",
          "password": "ghp_your_token_here",
          "email": "your-email@example.com",
          "auth": "base64(username:password)"
        }
      }
    }
```

**Generar auth (base64):**
```bash
echo -n "yourusername:ghp_your_token" | base64
```

### Usar el Secret en un Deployment

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: gateway
  namespace: okla
spec:
  replicas: 1
  selector:
    matchLabels:
      app: gateway
  template:
    metadata:
      labels:
        app: gateway
    spec:
      imagePullSecrets:
      - name: ghcr-secret  # ← Aquí
      containers:
      - name: gateway
        image: ghcr.io/gregorymorenoiem/cardealer-gateway:latest
        ports:
        - containerPort: 8080
```

### Aplicar deployment

```bash
kubectl apply -f k8s/deployments.yaml
```

### Verificar pull exitoso

```bash
kubectl get pods -n okla | grep gateway
```

**Salida:**
```
gateway-xxx   1/1   Running   0   30s
```

**Status Running** significa que la imagen se descargó correctamente.

---

## 7. Automatización con GitHub Actions

### Workflow de CI/CD con GHCR

El proyecto OKLA ya tiene automatización en `.github/workflows/smart-cicd.yml`.

**Flujo:**
1. Push a `main` branch
2. GitHub Actions se activa
3. Build de imágenes Docker
4. Push a GHCR
5. Deploy a Kubernetes (opcional)

### Extracto del workflow

```yaml
name: Smart CI/CD

on:
  push:
    branches: [main]

env:
  REGISTRY: ghcr.io
  IMAGE_PREFIX: cardealer

jobs:
  build-and-push:
    runs-on: ubuntu-latest
    permissions:
      contents: read
      packages: write
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Log in to GHCR
      uses: docker/login-action@v3
      with:
        registry: ghcr.io
        username: ${{ github.actor }}
        password: ${{ secrets.GITHUB_TOKEN }}
    
    - name: Build and push Gateway
      run: |
        cd backend/Gateway/Gateway.Api
        docker build -t ghcr.io/${{ github.repository_owner }}/cardealer-gateway:latest .
        docker push ghcr.io/${{ github.repository_owner }}/cardealer-gateway:latest
```

**¿Qué hace?**
1. Checkout del código
2. Login en GHCR con `GITHUB_TOKEN` (automático)
3. Build y push de imagen

### Ver workflow en GitHub

1. Ve a tu repositorio en GitHub
2. Click en "Actions"
3. Verás los workflows ejecutándose

**URL directa:**
```
https://github.com/yourusername/cardealer-microservices/actions
```

### Secrets requeridos en GitHub

| Secret | Descripción |
|--------|-------------|
| `GITHUB_TOKEN` | Automático, no necesitas crearlo |
| `GHCR_TOKEN` | (Opcional) Si usas token personal |

**GITHUB_TOKEN es automático** - GitHub lo proporciona en cada workflow.

---

## 8. Gestión de tags

### Estrategias de tagging

| Estrategia | Ejemplo | Uso |
|------------|---------|-----|
| **latest** | `gateway:latest` | Desarrollo, siempre la última versión |
| **Semantic Version** | `gateway:v1.2.3` | Producción, versionado claro |
| **Git SHA** | `gateway:abc123` | Trazabilidad exacta del commit |
| **Fecha + SHA** | `gateway:2026-01-07-abc123` | Combinación de fecha y commit |
| **Branch name** | `gateway:feature-payments` | Testing de features |

### Tag con semantic versioning

```bash
docker build -t ghcr.io/gregorymorenoiem/cardealer-gateway:v1.2.0 .
docker push ghcr.io/gregorymorenoiem/cardealer-gateway:v1.2.0

# También mantener latest
docker tag ghcr.io/gregorymorenoiem/cardealer-gateway:v1.2.0 \
           ghcr.io/gregorymorenoiem/cardealer-gateway:latest
docker push ghcr.io/gregorymorenoiem/cardealer-gateway:latest
```

### Tag con Git SHA

```bash
GIT_SHA=$(git rev-parse --short HEAD)
docker build -t ghcr.io/gregorymorenoiem/cardealer-gateway:$GIT_SHA .
docker push ghcr.io/gregorymorenoiem/cardealer-gateway:$GIT_SHA
```

### Multi-tag en GitHub Actions

```yaml
- name: Build and push with multiple tags
  uses: docker/build-push-action@v5
  with:
    context: backend/Gateway/Gateway.Api
    push: true
    tags: |
      ghcr.io/${{ github.repository_owner }}/cardealer-gateway:latest
      ghcr.io/${{ github.repository_owner }}/cardealer-gateway:${{ github.sha }}
      ghcr.io/${{ github.repository_owner }}/cardealer-gateway:v1.2.0
```

### Ver tags de una imagen en GHCR

```
https://github.com/users/yourusername/packages/container/cardealer-gateway/versions
```

### Eliminar tags antiguos

```bash
# Usando API de GitHub
curl -X DELETE \
  -H "Authorization: Bearer $GHCR_TOKEN" \
  https://api.github.com/user/packages/container/cardealer-gateway/versions/12345
```

**Mejor práctica:** Automatizar limpieza de tags antiguos con GitHub Actions.

---

## 9. Cheat Sheet

### Comandos Docker con GHCR

| Comando | Descripción |
|---------|-------------|
| `docker login ghcr.io -u username --password-stdin` | Login en GHCR |
| `docker build -t ghcr.io/user/image:tag .` | Build imagen |
| `docker push ghcr.io/user/image:tag` | Push a GHCR |
| `docker pull ghcr.io/user/image:tag` | Pull de GHCR |
| `docker tag source-image ghcr.io/user/image:tag` | Crear nuevo tag |
| `docker images | grep ghcr.io` | Ver imágenes GHCR locales |

### Comandos Kubernetes con ImagePullSecret

| Comando | Descripción |
|---------|-------------|
| `kubectl create secret docker-registry <name> --docker-server=ghcr.io ...` | Crear secret |
| `kubectl get secrets -n okla` | Ver secrets |
| `kubectl describe secret <name> -n okla` | Detalles del secret |
| `kubectl delete secret <name> -n okla` | Eliminar secret |

### Build y push rápido

```bash
# Build y push en un comando
cd backend/Gateway/Gateway.Api
docker build -t ghcr.io/gregorymorenoiem/cardealer-gateway:latest . && \
docker push ghcr.io/gregorymorenoiem/cardealer-gateway:latest

# Build multi-arch (ARM + AMD)
docker buildx build --platform linux/amd64,linux/arm64 \
  -t ghcr.io/gregorymorenoiem/cardealer-gateway:latest \
  --push .
```

---

## 10. Ejercicios prácticos

### Ejercicio 1: Login en GHCR

1. Crea un Personal Access Token en GitHub con scope `write:packages`
2. Haz login en GHCR con Docker
3. Verifica que el login fue exitoso

<details>
<summary>Solución</summary>

```bash
# 1 - Ve a https://github.com/settings/tokens y crea el token

# 2
export GHCR_TOKEN=ghp_tu_token_aqui
echo $GHCR_TOKEN | docker login ghcr.io -u yourusername --password-stdin

# 3
cat ~/.docker/config.json | grep ghcr.io
```
</details>

### Ejercicio 2: Build local

1. Navega a `backend/Gateway/Gateway.Api`
2. Build una imagen con tag `test`
3. Ve la imagen localmente

<details>
<summary>Solución</summary>

```bash
# 1
cd backend/Gateway/Gateway.Api

# 2
docker build -t ghcr.io/yourusername/cardealer-gateway:test .

# 3
docker images | grep gateway
```
</details>

### Ejercicio 3: Crear ImagePullSecret

1. Crea un ImagePullSecret llamado `ghcr-test-secret` en namespace okla
2. Verifica que se creó correctamente
3. Elimínalo

<details>
<summary>Solución</summary>

```bash
# 1
kubectl create secret docker-registry ghcr-test-secret \
  --docker-server=ghcr.io \
  --docker-username=yourusername \
  --docker-password=$GHCR_TOKEN \
  --docker-email=your-email@example.com \
  -n okla

# 2
kubectl get secret ghcr-test-secret -n okla

# 3
kubectl delete secret ghcr-test-secret -n okla
```
</details>

### Ejercicio 4: Verificar imagen en deployment

1. Ve la configuración del deployment `gateway`
2. Identifica qué imagen está usando
3. Verifica que tenga `imagePullSecrets`

<details>
<summary>Solución</summary>

```bash
# 1 y 2
kubectl get deployment gateway -n okla -o yaml | grep image:

# 3
kubectl get deployment gateway -n okla -o yaml | grep imagePullSecrets -A2
```
</details>

### Ejercicio 5: Ver packages en GitHub

1. Ve a tu perfil de GitHub → Packages
2. Identifica cuántos paquetes (imágenes) tienes
3. Click en uno y ve los tags disponibles

<details>
<summary>Solución</summary>

```
1. https://github.com/yourusername?tab=packages
2. Cuenta manual
3. Click en cualquier paquete para ver versiones/tags
```
</details>

---

## 🎉 ¡Felicidades!

Has completado el Tutorial 7. Ahora sabes:
- ✅ Qué es un Container Registry y por qué GHCR
- ✅ Configurar GitHub Container Registry
- ✅ Crear Personal Access Tokens
- ✅ Build y push de imágenes Docker
- ✅ Crear ImagePullSecrets en Kubernetes
- ✅ Pull de imágenes privadas en deployments
- ✅ Automatización con GitHub Actions
- ✅ Estrategias de tagging

---

**Anterior:** [06 - LoadBalancer y Networking](./06-loadbalancer-networking.md)  
**Siguiente:** [08 - PostgreSQL StatefulSet](./08-postgresql-statefulset.md)
