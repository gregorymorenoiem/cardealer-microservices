# Tutorial 13: Smart CI/CD

**Duración:** 60 minutos  
**Nivel:** Avanzado  
**Requisitos:** Tutorial 12 completado

---

## 📋 Resumen

Aprenderás a implementar un pipeline CI/CD completo para OKLA. Entenderás el workflow `smart-cicd.yml`, cómo detectar cambios en servicios, build de imágenes Docker, push a GHCR, y deploy automático a Kubernetes.

### Lo que aprenderás:
- Arquitectura del pipeline completo
- Detección inteligente de cambios
- Build optimizado con cache
- Multi-service matrix builds
- Push automático a GHCR
- Deploy automático a DOKS
- Verificación post-deploy

---

## 📑 Índice

1. [Arquitectura del pipeline](#1-arquitectura-del-pipeline)
2. [smart-cicd.yml explicado](#2-smart-cicdyml-explicado)
3. [Detección de cambios](#3-detección-de-cambios)
4. [Build de imágenes](#4-build-de-imágenes)
5. [Push a GHCR](#5-push-a-ghcr)
6. [Deploy a DOKS](#6-deploy-a-doks)
7. [Verificación post-deploy](#7-verificación-post-deploy)
8. [Optimizaciones](#8-optimizaciones)
9. [Cheat Sheet](#9-cheat-sheet)
10. [Ejercicios prácticos](#10-ejercicios-prácticos)

---

## 1. Arquitectura del pipeline

### Flujo completo

```
┌─────────────────────────────────────────────────────────┐
│  Developer                                              │
│  $ git push origin main                                 │
└────────────────────┬────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────┐
│  GitHub                                                 │
│  Trigger: push to main                                  │
└────────────────────┬────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────┐
│  GitHub Actions: smart-cicd.yml                         │
│                                                         │
│  Job 1: detect-changes                                  │
│    - Detecta qué servicios cambiaron                    │
│    - Output: ["gateway", "authservice"]                 │
│                                                         │
│  Job 2: build (matrix)                                  │
│    needs: detect-changes                                │
│    strategy:                                            │
│      matrix:                                            │
│        service: ${{ needs.detect-changes.outputs }}     │
│    steps:                                               │
│      - Checkout code                                    │
│      - Setup Docker Buildx                              │
│      - Build image con cache                            │
│      - Push a ghcr.io                                   │
│                                                         │
│  Job 3: deploy                                          │
│    needs: build                                         │
│    steps:                                               │
│      - Setup kubectl                                    │
│      - Restart deployments                              │
│      - Verificar rollout                                │
└────────────────────┬────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────┐
│  GHCR (ghcr.io)                                         │
│  cardealer-gateway:latest                               │
│  cardealer-authservice:latest                           │
└────────────────────┬────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────┐
│  DOKS Cluster (okla-cluster)                            │
│  Namespace: okla                                        │
│  Deployments reiniciados automáticamente                │
│  Pull de imágenes nuevas                                │
└─────────────────────────────────────────────────────────┘
```

### Ventajas del pipeline

| Ventaja | Descripción |
|---------|-------------|
| **Automático** | Push → Build → Deploy sin intervención |
| **Incremental** | Solo build servicios modificados |
| **Cache** | Build más rápidos con Docker layer cache |
| **Paralelo** | Múltiples servicios en paralelo (matrix) |
| **Verificado** | Verificación post-deploy automática |
| **Rollback fácil** | Git revert → deploy automático |

---

## 2. smart-cicd.yml explicado

### Archivo completo (simplificado)

```yaml
name: Smart CI/CD

on:
  push:
    branches: [main]
    paths:
    - 'backend/**'
    - 'frontend/**'
    - '.github/workflows/**'

env:
  REGISTRY: ghcr.io
  IMAGE_PREFIX: gregorymorenoiem/cardealer
  SERVICES: "frontend-web,gateway,authservice,userservice,vehiclessaleservice,mediaservice,notificationservice,billingservice,errorservice,roleservice"

jobs:
  detect-changes:
    runs-on: ubuntu-latest
    outputs:
      services: ${{ steps.changes.outputs.services }}
    steps:
    - uses: actions/checkout@v4
      with:
        fetch-depth: 2
    
    - name: Detect changed services
      id: changes
      run: |
        CHANGED_FILES=$(git diff --name-only HEAD^ HEAD)
        CHANGED_SERVICES=""
        
        for SERVICE in ${SERVICES//,/ }; do
          if echo "$CHANGED_FILES" | grep -q "backend/${SERVICE}"; then
            CHANGED_SERVICES="$CHANGED_SERVICES,$SERVICE"
          fi
        done
        
        echo "services=${CHANGED_SERVICES#,}" >> $GITHUB_OUTPUT

  build:
    needs: detect-changes
    if: needs.detect-changes.outputs.services != ''
    runs-on: ubuntu-latest
    permissions:
      contents: read
      packages: write
    strategy:
      matrix:
        service: ${{ fromJSON(needs.detect-changes.outputs.services) }}
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Log in to GHCR
      uses: docker/login-action@v3
      with:
        registry: ${{ env.REGISTRY }}
        username: ${{ github.actor }}
        password: ${{ secrets.GITHUB_TOKEN }}
    
    - name: Build and push ${{ matrix.service }}
      uses: docker/build-push-action@v5
      with:
        context: .
        file: backend/${{ matrix.service }}/Dockerfile
        push: true
        tags: ${{ env.REGISTRY }}/${{ env.IMAGE_PREFIX }}-${{ matrix.service }}:latest
        cache-from: type=registry,ref=${{ env.REGISTRY }}/${{ env.IMAGE_PREFIX }}-${{ matrix.service }}:buildcache
        cache-to: type=registry,ref=${{ env.REGISTRY }}/${{ env.IMAGE_PREFIX }}-${{ matrix.service }}:buildcache,mode=max

  deploy:
    needs: build
    runs-on: ubuntu-latest
    steps:
    - uses: digitalocean/action-doctl@v2
      with:
        token: ${{ secrets.DIGITALOCEAN_ACCESS_TOKEN }}
    
    - name: Setup kubectl
      run: doctl kubernetes cluster kubeconfig save okla-cluster
    
    - name: Restart deployments
      run: |
        for SERVICE in ${SERVICES//,/ }; do
          kubectl rollout restart deployment/$SERVICE -n okla || true
        done
    
    - name: Verify rollout
      run: |
        for SERVICE in ${SERVICES//,/ }; do
          kubectl rollout status deployment/$SERVICE -n okla --timeout=5m || true
        done
```

### Componentes clave

#### 1. Trigger con path filter

```yaml
on:
  push:
    branches: [main]
    paths:
    - 'backend/**'
    - 'frontend/**'
```

**Solo ejecuta cuando** hay cambios en `backend/` o `frontend/`.

**NO ejecuta cuando** cambias:
- README.md
- docs/
- .gitignore

#### 2. Variables globales

```yaml
env:
  REGISTRY: ghcr.io
  IMAGE_PREFIX: gregorymorenoiem/cardealer
  SERVICES: "frontend-web,gateway,authservice,..."
```

Compartidas por todos los jobs.

#### 3. Job outputs

```yaml
jobs:
  detect-changes:
    outputs:
      services: ${{ steps.changes.outputs.services }}
```

Pasa datos entre jobs.

---

## 3. Detección de cambios

### ¿Por qué?

**Problema:** No queremos build TODOS los servicios en cada push.

**Solución:** Detectar qué servicios cambiaron.

### Script de detección

```bash
# Ver archivos modificados
CHANGED_FILES=$(git diff --name-only HEAD^ HEAD)

# Ejemplo de output:
# backend/AuthService/AuthService.Api/Controllers/AuthController.cs
# backend/Gateway/Gateway.Api/ocelot.prod.json
# docs/README.md

# Iterar sobre servicios
CHANGED_SERVICES=""
for SERVICE in gateway authservice userservice; do
  if echo "$CHANGED_FILES" | grep -q "backend/${SERVICE}"; then
    CHANGED_SERVICES="$CHANGED_SERVICES,$SERVICE"
  fi
done

# Output: "gateway,authservice"
```

### Paso completo en workflow

```yaml
- name: Detect changed services
  id: changes
  run: |
    CHANGED_FILES=$(git diff --name-only HEAD^ HEAD)
    CHANGED_SERVICES=""
    
    for SERVICE in ${SERVICES//,/ }; do
      if echo "$CHANGED_FILES" | grep -q "backend/${SERVICE}\|frontend/${SERVICE}"; then
        CHANGED_SERVICES="$CHANGED_SERVICES,$SERVICE"
      fi
    done
    
    # Remover coma inicial
    CHANGED_SERVICES=${CHANGED_SERVICES#,}
    
    # Si hay cambios, output como JSON array
    if [ -n "$CHANGED_SERVICES" ]; then
      JSON_ARRAY=$(echo "$CHANGED_SERVICES" | jq -R -s -c 'split(",") | map(select(length > 0))')
      echo "services=$JSON_ARRAY" >> $GITHUB_OUTPUT
    else
      echo "services=[]" >> $GITHUB_OUTPUT
    fi
```

### Usar el output

```yaml
build:
  needs: detect-changes
  if: needs.detect-changes.outputs.services != '[]'
  strategy:
    matrix:
      service: ${{ fromJSON(needs.detect-changes.outputs.services) }}
```

**`fromJSON()`** convierte el string JSON en array para matrix.

---

## 4. Build de imágenes

### Docker Buildx

```yaml
- name: Set up Docker Buildx
  uses: docker/setup-buildx-action@v3
```

**Buildx** permite:
- Builds multi-platform (amd64, arm64)
- Cache avanzado
- Build en paralelo

### Build con cache

```yaml
- name: Build and push ${{ matrix.service }}
  uses: docker/build-push-action@v5
  with:
    context: .
    file: backend/${{ matrix.service }}/Dockerfile
    push: true
    tags: ${{ env.REGISTRY }}/${{ env.IMAGE_PREFIX }}-${{ matrix.service }}:latest
    cache-from: type=registry,ref=${{ env.REGISTRY }}/${{ env.IMAGE_PREFIX }}-${{ matrix.service }}:buildcache
    cache-to: type=registry,ref=${{ env.REGISTRY }}/${{ env.IMAGE_PREFIX }}-${{ matrix.service }}:buildcache,mode=max
```

### Cache layers

**Sin cache:**
```
Build #1: 10 minutos
Build #2: 10 minutos
Build #3: 10 minutos
```

**Con cache:**
```
Build #1: 10 minutos (primera vez)
Build #2: 2 minutos  (usa cache)
Build #3: 2 minutos  (usa cache)
```

**¿Qué se cachea?**
- Capas de Docker (FROM, RUN, COPY)
- Restauración de paquetes NuGet
- npm install (frontend)

### Multi-stage Dockerfile

```dockerfile
# backend/AuthService/AuthService.Api/Dockerfile

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar solo .csproj (cacheable si no cambian)
COPY ["backend/AuthService/AuthService.Api/AuthService.Api.csproj", "AuthService.Api/"]
COPY ["backend/_Shared/CarDealer.Shared/CarDealer.Shared.csproj", "CarDealer.Shared/"]

# Restore (cacheable)
RUN dotnet restore "AuthService.Api/AuthService.Api.csproj"

# Copiar código fuente
COPY backend/AuthService/ AuthService/
COPY backend/_Shared/ _Shared/

# Build
WORKDIR "/src/AuthService/AuthService.Api"
RUN dotnet build -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AuthService.Api.dll"]
```

**Optimización:**
- Stage 1: Solo rebuida si cambian .csproj o código
- Stage 2: Solo re-publica si cambió build
- Stage 3: Imagen final (solo runtime, más pequeña)

---

## 5. Push a GHCR

### Login

```yaml
- name: Log in to GHCR
  uses: docker/login-action@v3
  with:
    registry: ghcr.io
    username: ${{ github.actor }}
    password: ${{ secrets.GITHUB_TOKEN }}
```

**`GITHUB_TOKEN`** es un secret automático con permisos para:
- Leer código del repo
- Escribir a GitHub Packages (GHCR)

### Permisos del job

```yaml
permissions:
  contents: read
  packages: write
```

Sin estos permisos, el push falla.

### Tags

```yaml
tags: ghcr.io/gregorymorenoiem/cardealer-${{ matrix.service }}:latest
```

**Ejemplo:**
- `ghcr.io/gregorymorenoiem/cardealer-gateway:latest`
- `ghcr.io/gregorymorenoiem/cardealer-authservice:latest`

**Estrategias de tagging:**

#### Solo latest (OKLA)

```yaml
tags: ${{ env.REGISTRY }}/${{ env.IMAGE_PREFIX }}-${{ matrix.service }}:latest
```

**Ventaja:** Simple, siempre la última versión.  
**Desventaja:** No hay versiones intermedias.

#### Semver + latest

```yaml
tags: |
  ${{ env.REGISTRY }}/${{ env.IMAGE_PREFIX }}-${{ matrix.service }}:latest
  ${{ env.REGISTRY }}/${{ env.IMAGE_PREFIX }}-${{ matrix.service }}:${{ github.sha }}
  ${{ env.REGISTRY }}/${{ env.IMAGE_PREFIX }}-${{ matrix.service }}:v1.2.3
```

**Ventaja:** Múltiples versiones disponibles.

---

## 6. Deploy a DOKS

### Setup kubectl

```yaml
- name: Setup doctl
  uses: digitalocean/action-doctl@v2
  with:
    token: ${{ secrets.DIGITALOCEAN_ACCESS_TOKEN }}

- name: Connect to cluster
  run: doctl kubernetes cluster kubeconfig save okla-cluster
```

### Restart deployments

```yaml
- name: Restart deployments
  run: |
    kubectl rollout restart deployment/gateway -n okla
    kubectl rollout restart deployment/authservice -n okla
```

**⚠️ IMPORTANTE:** Los pods pull la imagen `latest` al reiniciar.

### Loop para múltiples servicios

```yaml
- name: Restart all deployments
  run: |
    SERVICES="gateway,authservice,userservice,vehiclessaleservice"
    for SERVICE in ${SERVICES//,/ }; do
      echo "Restarting $SERVICE..."
      kubectl rollout restart deployment/$SERVICE -n okla || echo "⚠️ $SERVICE not found"
    done
```

### Verificar rollout

```yaml
- name: Verify rollout
  run: |
    for SERVICE in gateway authservice userservice; do
      echo "Verifying $SERVICE..."
      kubectl rollout status deployment/$SERVICE -n okla --timeout=5m
    done
```

**Output esperado:**
```
Verifying gateway...
deployment "gateway" successfully rolled out
Verifying authservice...
deployment "authservice" successfully rolled out
```

---

## 7. Verificación post-deploy

### Health check del API

```yaml
- name: Health check
  run: |
    echo "Waiting for API to be ready..."
    sleep 30
    
    RESPONSE=$(curl -s -o /dev/null -w "%{http_code}" https://api.okla.com.do/health)
    if [ "$RESPONSE" -eq 200 ]; then
      echo "✅ API is healthy"
    else
      echo "❌ API health check failed (HTTP $RESPONSE)"
      exit 1
    fi
```

### Smoke tests

```yaml
- name: Smoke tests
  run: |
    # Test Gateway
    curl -f https://api.okla.com.do/health || exit 1
    
    # Test Vehicles endpoint
    curl -f https://api.okla.com.do/api/vehicles?page=1&pageSize=10 || exit 1
    
    # Test Auth endpoint
    curl -f https://api.okla.com.do/api/auth/health || exit 1
    
    echo "✅ All smoke tests passed"
```

### Rollback automático en fallo

```yaml
- name: Verify deployment
  id: verify
  run: |
    kubectl rollout status deployment/gateway -n okla --timeout=5m
  continue-on-error: true

- name: Rollback on failure
  if: steps.verify.outcome == 'failure'
  run: |
    echo "❌ Deployment failed, rolling back..."
    kubectl rollout undo deployment/gateway -n okla
```

---

## 8. Optimizaciones

### 1. Condicionales para skip jobs

```yaml
jobs:
  build:
    if: |
      !contains(github.event.head_commit.message, '[skip ci]') &&
      !contains(github.event.head_commit.message, '[ci skip]')
```

**Uso:**
```bash
git commit -m "Update README [skip ci]"
git push
# CI no se ejecuta
```

### 2. Cache de dependencias

```yaml
- name: Cache NuGet packages
  uses: actions/cache@v4
  with:
    path: ~/.nuget/packages
    key: ${{ runner.os }}-nuget-${{ hashFiles('**/*.csproj') }}
    restore-keys: |
      ${{ runner.os }}-nuget-
```

### 3. Artifacts para compartir entre jobs

```yaml
jobs:
  test:
    steps:
    - run: dotnet test --logger trx
    - uses: actions/upload-artifact@v4
      with:
        name: test-results
        path: '**/*.trx'
  
  report:
    needs: test
    steps:
    - uses: actions/download-artifact@v4
      with:
        name: test-results
```

### 4. Parallel matrix builds

```yaml
strategy:
  matrix:
    service: [gateway, authservice, userservice]
  max-parallel: 10  # Build hasta 10 servicios en paralelo
```

### 5. Fail-fast vs continue-on-error

**Fail-fast (default):**
```yaml
strategy:
  fail-fast: true  # Si un servicio falla, cancela todos
```

**Continue on error:**
```yaml
strategy:
  fail-fast: false  # Continúa incluso si un servicio falla
```

---

## 9. Cheat Sheet

### Workflow completo (template)

```yaml
name: CI/CD

on:
  push:
    branches: [main]

env:
  REGISTRY: ghcr.io

jobs:
  build:
    runs-on: ubuntu-latest
    permissions:
      contents: read
      packages: write
    
    steps:
    - uses: actions/checkout@v4
    
    - uses: docker/login-action@v3
      with:
        registry: ${{ env.REGISTRY }}
        username: ${{ github.actor }}
        password: ${{ secrets.GITHUB_TOKEN }}
    
    - uses: docker/build-push-action@v5
      with:
        context: .
        file: Dockerfile
        push: true
        tags: ${{ env.REGISTRY }}/myimage:latest
        cache-from: type=registry,ref=${{ env.REGISTRY }}/myimage:buildcache
        cache-to: type=registry,ref=${{ env.REGISTRY }}/myimage:buildcache

  deploy:
    needs: build
    runs-on: ubuntu-latest
    steps:
    - uses: digitalocean/action-doctl@v2
      with:
        token: ${{ secrets.DIGITALOCEAN_ACCESS_TOKEN }}
    
    - run: doctl kubernetes cluster kubeconfig save my-cluster
    
    - run: kubectl rollout restart deployment/myapp -n default
    
    - run: kubectl rollout status deployment/myapp -n default --timeout=5m
```

### Comandos útiles

```bash
# Ver workflows
gh workflow list

# Ver runs
gh run list --limit 10

# Ver logs de un run
gh run view <run-id> --log

# Watch un run en tiempo real
gh run watch <run-id>

# Re-ejecutar workflow fallido
gh run rerun <run-id>

# Cancelar run
gh run cancel <run-id>
```

---

## 10. Ejercicios prácticos

### Ejercicio 1: Ver workflow actual

1. Ve el contenido de `.github/workflows/smart-cicd.yml`
2. Identifica los jobs
3. Identifica la estrategia de matrix

<details>
<summary>Solución</summary>

```bash
# 1
cat .github/workflows/smart-cicd.yml

# 2
# Jobs: detect-changes, build, deploy

# 3
# strategy:
#   matrix:
#     service: ${{ fromJSON(needs.detect-changes.outputs.services) }}
```
</details>

### Ejercicio 2: Ver últimas ejecuciones

1. Ve a Actions en GitHub
2. Selecciona workflow "Smart CI/CD"
3. Ve las últimas 5 ejecuciones
4. Identifica cuáles fueron exitosas

<details>
<summary>Solución</summary>

En GitHub UI:
- Actions → Smart CI/CD
- Lista de runs con status (✓ o ✗)
- Verde = Éxito, Rojo = Fallo
</details>

### Ejercicio 3: Ver imágenes en GHCR

1. Ve a github.com/USUARIO/REPO/pkgs/container
2. Lista las imágenes
3. Identifica las tags

<details>
<summary>Solución</summary>

Imágenes esperadas:
- cardealer-gateway:latest
- cardealer-authservice:latest
- cardealer-userservice:latest
- ...
</details>

### Ejercicio 4: Ver cache usage

1. Ve a Settings → Actions → Caches en GitHub
2. Lista los caches
3. Verifica el tamaño

<details>
<summary>Solución</summary>

Caches esperados:
- cardealer-gateway:buildcache
- cardealer-authservice:buildcache
- ...
</details>

### Ejercicio 5: Simular detección de cambios

Localmente, simula el script de detección.

1. Ve qué archivos cambiaron en el último commit
2. Determina qué servicios afectaron

<details>
<summary>Solución</summary>

```bash
# 1
git diff --name-only HEAD^ HEAD

# 2
CHANGED_FILES=$(git diff --name-only HEAD^ HEAD)
SERVICES="gateway,authservice,userservice"

for SERVICE in ${SERVICES//,/ }; do
  if echo "$CHANGED_FILES" | grep -q "backend/${SERVICE}"; then
    echo "Changed: $SERVICE"
  fi
done
```
</details>

---

## 🎉 ¡Felicidades!

Has completado el Tutorial 13. Ahora sabes:
- ✅ Arquitectura completa del pipeline CI/CD
- ✅ Detección inteligente de cambios
- ✅ Build optimizado con Docker cache
- ✅ Matrix builds para paralelización
- ✅ Push automático a GHCR
- ✅ Deploy automático a DOKS
- ✅ Verificación post-deploy
- ✅ Optimizaciones avanzadas

---

**Anterior:** [12 - GitHub Actions Setup](./12-github-actions-setup.md)  
**Siguiente:** [14 - Monitoreo Básico](./14-monitoreo-basico.md)
