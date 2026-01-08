# Tutorial 12: GitHub Actions Setup

**Duración:** 45 minutos  
**Nivel:** Intermedio  
**Requisitos:** Tutorial 11 completado

---

## 📋 Resumen

Aprenderás a configurar GitHub Actions para CI/CD. Entenderás la estructura de workflows, cómo usar secrets, y cómo automatizar builds y deployments.

### Lo que aprenderás:
- Fundamentos de GitHub Actions
- Estructura de archivos YAML de workflows
- GitHub Secrets y tokens
- Triggers (push, pull_request, manual)
- Jobs, steps y actions
- Contextos y variables de entorno
- Matrix builds para múltiples servicios

---

## 📑 Índice

1. [Conceptos básicos](#1-conceptos-básicos)
2. [Estructura de un workflow](#2-estructura-de-un-workflow)
3. [GitHub Secrets](#3-github-secrets)
4. [Triggers de workflows](#4-triggers-de-workflows)
5. [Jobs y Steps](#5-jobs-y-steps)
6. [Actions del marketplace](#6-actions-del-marketplace)
7. [Variables de entorno](#7-variables-de-entorno)
8. [Matrix builds](#8-matrix-builds)
9. [Cheat Sheet](#9-cheat-sheet)
10. [Ejercicios prácticos](#10-ejercicios-prácticos)

---

## 1. Conceptos básicos

### ¿Qué es GitHub Actions?

**GitHub Actions** es un sistema CI/CD integrado en GitHub.

**Ventajas:**
- ✅ Integración nativa con repos de GitHub
- ✅ Gratis para repos públicos (2,000 minutos/mes en privados)
- ✅ Marketplace con miles de actions reutilizables
- ✅ No requiere infraestructura externa

### Componentes clave

```
Workflow
  ↓
  Job 1          Job 2 (depends on Job 1)
  ↓              ↓
  Step 1         Step 1
  Step 2         Step 2
  Step 3         Step 3
```

| Componente | Descripción |
|------------|-------------|
| **Workflow** | Archivo YAML que define el proceso completo |
| **Job** | Grupo de steps que se ejecutan en el mismo runner |
| **Step** | Tarea individual (ejecutar comando, usar action) |
| **Action** | Aplicación reutilizable que hace una tarea |
| **Runner** | Máquina virtual que ejecuta los jobs (Ubuntu, Windows, macOS) |
| **Event** | Trigger que inicia el workflow (push, pull_request, etc.) |

### Arquitectura de GitHub Actions

```
┌────────────────────────────────────────────────┐
│          GitHub Repository                     │
│                                                │
│  .github/workflows/                            │
│    ├── smart-cicd.yml         ← Workflow       │
│    └── deploy-digitalocean.yml                 │
└────────────┬───────────────────────────────────┘
             │
             │ (Event: push to main)
             ▼
┌────────────────────────────────────────────────┐
│         GitHub Actions Runner                  │
│         (Ubuntu 22.04 VM)                      │
│                                                │
│  Job: build                                    │
│    Step 1: Checkout code                       │
│    Step 2: Setup .NET                          │
│    Step 3: Build image                         │
│    Step 4: Push to GHCR                        │
└────────────┬───────────────────────────────────┘
             │
             ▼
┌────────────────────────────────────────────────┐
│  GitHub Container Registry (ghcr.io)           │
│    cardealer-gateway:latest                    │
│    cardealer-authservice:latest                │
│    ...                                         │
└────────────────────────────────────────────────┘
```

---

## 2. Estructura de un workflow

### Ubicación

```
.github/
└── workflows/
    ├── smart-cicd.yml
    ├── deploy-digitalocean.yml
    └── pr-checks.yml
```

**Kubernetes NO ejecuta estos archivos.** Son para CI/CD en GitHub.

### Workflow básico

```yaml
name: CI Build

on:
  push:
    branches: [main]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
    
    - name: Say hello
      run: echo "Hello, World!"
```

**Explicación:**
- `name`: Nombre del workflow (aparece en UI)
- `on`: Evento que lo dispara
- `jobs`: Lista de jobs
- `runs-on`: Runner donde se ejecuta
- `steps`: Lista de pasos del job

### Workflow de OKLA (simple)

```yaml
name: Build Services

on:
  push:
    branches: [main]

env:
  REGISTRY: ghcr.io
  IMAGE_PREFIX: gregorymorenoiem/cardealer

jobs:
  build:
    runs-on: ubuntu-latest
    permissions:
      contents: read
      packages: write
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
    
    - name: Log in to GHCR
      uses: docker/login-action@v3
      with:
        registry: ${{ env.REGISTRY }}
        username: ${{ github.actor }}
        password: ${{ secrets.GITHUB_TOKEN }}
    
    - name: Build and push Gateway
      run: |
        docker build -t ${{ env.REGISTRY }}/${{ env.IMAGE_PREFIX }}-gateway:latest \
          -f backend/Gateway/Gateway.Api/Dockerfile .
        docker push ${{ env.REGISTRY }}/${{ env.IMAGE_PREFIX }}-gateway:latest
```

---

## 3. GitHub Secrets

### ¿Qué son?

**Secrets** son variables encriptadas para almacenar información sensible.

**Ejemplos:**
- Tokens de API
- Contraseñas
- SSH keys
- Kubernetes kubeconfig

### Crear un Secret

1. Ve al repo en GitHub
2. Settings → Secrets and variables → Actions
3. Click "New repository secret"
4. Name: `DIGITALOCEAN_ACCESS_TOKEN`
5. Value: (pegar token)
6. Add secret

### Secrets de OKLA

| Secret | Descripción | Uso |
|--------|-------------|-----|
| `GITHUB_TOKEN` | Token automático de GitHub | Push a GHCR |
| `GHCR_TOKEN` | Personal Access Token | Login a GHCR |
| `DIGITALOCEAN_ACCESS_TOKEN` | Token de Digital Ocean | Deploy a DOKS |
| `KUBE_CONFIG` | Kubeconfig en base64 | Deploy a Kubernetes |

### Usar secrets en workflow

```yaml
steps:
- name: Login to Digital Ocean
  uses: digitalocean/action-doctl@v2
  with:
    token: ${{ secrets.DIGITALOCEAN_ACCESS_TOKEN }}

- name: Configure kubectl
  run: |
    echo "${{ secrets.KUBE_CONFIG }}" | base64 -d > kubeconfig
    export KUBECONFIG=kubeconfig
```

### ⚠️ Seguridad

**NUNCA:**
- Commitear secrets en código
- Hacer echo de secrets en logs
- Usar secrets en repos públicos (se pueden leer en forks)

**SÍ:**
- Usar GitHub Secrets
- Usar environment secrets (production, staging)
- Rotar secrets regularmente

### Obtener kubeconfig para secret

```bash
# 1. Obtener kubeconfig
doctl kubernetes cluster kubeconfig save okla-cluster

# 2. Ver ruta del kubeconfig
echo $HOME/.kube/config

# 3. Convertir a base64
cat $HOME/.kube/config | base64 > kubeconfig-base64.txt

# 4. Copiar contenido y crear secret en GitHub
cat kubeconfig-base64.txt
```

---

## 4. Triggers de workflows

### on: push

Ejecuta cuando hay push a ramas específicas.

```yaml
on:
  push:
    branches:
    - main
    - develop
```

**Ejecuta solo cuando:**
- Push a `main` o `develop`

**NO ejecuta cuando:**
- Push a otra rama (ej: `feature/new-thing`)
- Pull request

### on: pull_request

Ejecuta cuando se abre/actualiza un PR.

```yaml
on:
  pull_request:
    branches:
    - main
```

**Ejecuta cuando:**
- PR abierto hacia `main`
- Push a rama de PR existente

### on: workflow_dispatch

Permite ejecutar el workflow manualmente.

```yaml
on:
  workflow_dispatch:
    inputs:
      environment:
        description: 'Environment to deploy'
        required: true
        default: 'production'
        type: choice
        options:
        - production
        - staging
```

**Uso:** Ir a Actions → Workflow → Run workflow → Seleccionar input.

### on: schedule

Ejecuta en un horario (cron).

```yaml
on:
  schedule:
  - cron: '0 2 * * *'  # Todos los días a las 2 AM UTC
```

**Ejemplos de cron:**
- `0 * * * *` - Cada hora
- `0 0 * * 0` - Cada domingo a medianoche
- `0 12 * * 1-5` - Lunes a viernes al mediodía

### Múltiples triggers

```yaml
on:
  push:
    branches: [main]
  pull_request:
    branches: [main]
  workflow_dispatch:
```

**Ejecuta cuando:**
- Push a main
- PR hacia main
- Manual

### paths (filtrado)

```yaml
on:
  push:
    branches: [main]
    paths:
    - 'backend/**'
    - '.github/workflows/**'
```

**Ejecuta solo cuando** los cambios están en:
- `backend/` directory
- `.github/workflows/` directory

**Uso típico:** No ejecutar CI si solo cambió README.

---

## 5. Jobs y Steps

### Jobs

```yaml
jobs:
  test:
    runs-on: ubuntu-latest
    steps:
    - run: echo "Running tests"
  
  build:
    runs-on: ubuntu-latest
    needs: test  # Espera a que test termine
    steps:
    - run: echo "Building"
  
  deploy:
    runs-on: ubuntu-latest
    needs: build
    steps:
    - run: echo "Deploying"
```

**Flujo:**
```
test → build → deploy (secuencial)
```

### Jobs en paralelo

```yaml
jobs:
  test-backend:
    runs-on: ubuntu-latest
    steps:
    - run: echo "Test backend"
  
  test-frontend:
    runs-on: ubuntu-latest
    steps:
    - run: echo "Test frontend"
```

**Flujo:**
```
test-backend  ┐
              ├─ En paralelo
test-frontend ┘
```

### Steps

```yaml
steps:
- name: Checkout code
  uses: actions/checkout@v4

- name: Setup .NET
  uses: actions/setup-dotnet@v4
  with:
    dotnet-version: '8.0.x'

- name: Restore dependencies
  run: dotnet restore

- name: Build
  run: dotnet build --configuration Release

- name: Test
  run: dotnet test --no-build
```

**Tipos de steps:**
- `uses`: Usa una action del marketplace
- `run`: Ejecuta comando shell

### Condicionales

```yaml
steps:
- name: Deploy to production
  if: github.ref == 'refs/heads/main'
  run: echo "Deploying to prod"

- name: Deploy to staging
  if: github.ref == 'refs/heads/develop'
  run: echo "Deploying to staging"
```

---

## 6. Actions del marketplace

### Repositorio de actions

https://github.com/marketplace?type=actions

### Actions comunes

#### actions/checkout

Clona el repo.

```yaml
- uses: actions/checkout@v4
```

**Versiones:**
- `@v4` - Versión específica (recomendado)
- `@main` - Última versión (puede romper)

#### actions/setup-dotnet

Instala .NET SDK.

```yaml
- uses: actions/setup-dotnet@v4
  with:
    dotnet-version: '8.0.x'
```

#### docker/login-action

Login a Docker registry.

```yaml
- uses: docker/login-action@v3
  with:
    registry: ghcr.io
    username: ${{ github.actor }}
    password: ${{ secrets.GITHUB_TOKEN }}
```

#### docker/build-push-action

Build y push de imágenes Docker.

```yaml
- uses: docker/build-push-action@v5
  with:
    context: .
    file: backend/Gateway/Gateway.Api/Dockerfile
    push: true
    tags: ghcr.io/gregorymorenoiem/cardealer-gateway:latest
```

#### digitalocean/action-doctl

Instala `doctl` CLI de Digital Ocean.

```yaml
- uses: digitalocean/action-doctl@v2
  with:
    token: ${{ secrets.DIGITALOCEAN_ACCESS_TOKEN }}

- name: Connect to cluster
  run: doctl kubernetes cluster kubeconfig save okla-cluster
```

### Crear tu propia action

```yaml
# .github/actions/build-dotnet/action.yml
name: Build .NET Service
description: Builds a .NET service
inputs:
  service-name:
    description: Name of the service
    required: true
runs:
  using: composite
  steps:
  - run: |
      cd backend/${{ inputs.service-name }}
      dotnet build
    shell: bash
```

**Uso:**
```yaml
- uses: ./.github/actions/build-dotnet
  with:
    service-name: AuthService
```

---

## 7. Variables de entorno

### env a nivel de workflow

```yaml
name: CI

env:
  DOTNET_VERSION: '8.0.x'
  REGISTRY: ghcr.io

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
    - run: echo "Registry: $REGISTRY"
```

### env a nivel de job

```yaml
jobs:
  build:
    runs-on: ubuntu-latest
    env:
      BUILD_CONFIG: Release
    steps:
    - run: dotnet build --configuration $BUILD_CONFIG
```

### env a nivel de step

```yaml
steps:
- name: Build
  env:
    MY_VAR: hello
  run: echo $MY_VAR
```

### Contextos de GitHub

```yaml
steps:
- name: Print context
  run: |
    echo "Actor: ${{ github.actor }}"
    echo "Ref: ${{ github.ref }}"
    echo "SHA: ${{ github.sha }}"
    echo "Repo: ${{ github.repository }}"
```

**Contextos disponibles:**
- `github.*` - Información del evento
- `env.*` - Variables de entorno
- `secrets.*` - Secrets
- `runner.*` - Información del runner
- `matrix.*` - Variables de matrix

---

## 8. Matrix builds

### ¿Qué es?

**Matrix** ejecuta el mismo job múltiples veces con diferentes parámetros.

**Uso típico:**
- Probar en múltiples versiones (.NET 6, 7, 8)
- Probar en múltiples OS (Ubuntu, Windows, macOS)
- **Build de múltiples servicios** (OKLA)

### Matrix simple

```yaml
jobs:
  test:
    runs-on: ${{ matrix.os }}
    strategy:
      matrix:
        os: [ubuntu-latest, windows-latest, macos-latest]
        dotnet: ['6.0.x', '7.0.x', '8.0.x']
    steps:
    - uses: actions/checkout@v4
    - uses: actions/setup-dotnet@v4
      with:
        dotnet-version: ${{ matrix.dotnet }}
    - run: dotnet test
```

**Resultado:** 9 jobs (3 OS × 3 .NET versions).

### Matrix para servicios (OKLA)

```yaml
jobs:
  build:
    runs-on: ubuntu-latest
    strategy:
      matrix:
        service:
        - gateway
        - authservice
        - userservice
        - vehiclessaleservice
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Build ${{ matrix.service }}
      run: |
        docker build \
          -t ghcr.io/gregorymorenoiem/cardealer-${{ matrix.service }}:latest \
          -f backend/${{ matrix.service }}/Dockerfile .
    
    - name: Push ${{ matrix.service }}
      run: |
        docker push ghcr.io/gregorymorenoiem/cardealer-${{ matrix.service }}:latest
```

**Resultado:** 4 jobs en paralelo.

### Matrix con include

```yaml
strategy:
  matrix:
    service:
    - name: gateway
      path: Gateway/Gateway.Api
    - name: authservice
      path: AuthService/AuthService.Api
    - name: userservice
      path: UserService/UserService.Api

steps:
- name: Build ${{ matrix.service.name }}
  run: |
    docker build -t my-image -f backend/${{ matrix.service.path }}/Dockerfile .
```

### Matrix con exclude

```yaml
strategy:
  matrix:
    os: [ubuntu, windows, macos]
    dotnet: ['6.0', '7.0', '8.0']
    exclude:
    - os: macos
      dotnet: '6.0'  # No probar .NET 6 en macOS
```

---

## 9. Cheat Sheet

### Estructura básica

```yaml
name: My Workflow

on:
  push:
    branches: [main]

env:
  MY_VAR: value

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v4
    - run: echo "Hello"
```

### Secrets

```yaml
steps:
- name: Use secret
  run: echo "Token: ${{ secrets.MY_TOKEN }}"
```

### Triggers

```yaml
on:
  push:
    branches: [main]
  pull_request:
    branches: [main]
  workflow_dispatch:
  schedule:
  - cron: '0 0 * * *'
```

### Jobs con dependencias

```yaml
jobs:
  test:
    runs-on: ubuntu-latest
    steps:
    - run: echo "Test"
  
  deploy:
    needs: test
    runs-on: ubuntu-latest
    steps:
    - run: echo "Deploy"
```

### Matrix

```yaml
strategy:
  matrix:
    service: [gateway, authservice, userservice]
steps:
- run: echo "Building ${{ matrix.service }}"
```

### Contextos

```yaml
- run: |
    echo "Actor: ${{ github.actor }}"
    echo "Ref: ${{ github.ref }}"
    echo "Repo: ${{ github.repository }}"
```

---

## 10. Ejercicios prácticos

### Ejercicio 1: Ver workflows del proyecto

1. Lista los archivos en `.github/workflows`
2. Cuenta cuántos workflows hay

<details>
<summary>Solución</summary>

```bash
# 1
ls .github/workflows

# 2
ls .github/workflows | wc -l
# Output: 3 (smart-cicd.yml, deploy-digitalocean.yml, pr-checks.yml)
```
</details>

### Ejercicio 2: Ver un workflow

1. Ve el contenido de `smart-cicd.yml`
2. Identifica el trigger
3. Identifica cuántos jobs tiene

<details>
<summary>Solución</summary>

```bash
# 1
cat .github/workflows/smart-cicd.yml | head -30

# 2
# on: push (branches: main)

# 3
# grep "^  [a-z]" para ver jobs
cat .github/workflows/smart-cicd.yml | grep -E "^  [a-z]"
```
</details>

### Ejercicio 3: Ver secrets del repo

1. Ve a Settings → Secrets and variables → Actions en GitHub
2. Lista los secrets configurados
3. (NO REVELAR valores)

<details>
<summary>Solución</summary>

Secrets esperados:
- GITHUB_TOKEN (automático)
- DIGITALOCEAN_ACCESS_TOKEN
- KUBE_CONFIG
- GHCR_TOKEN
</details>

### Ejercicio 4: Ver historial de workflows

1. Ve a la pestaña "Actions" en GitHub
2. Selecciona el workflow "Smart CI/CD"
3. Ve los últimos 5 runs

<details>
<summary>Solución</summary>

En GitHub UI:
- Actions tab
- Smart CI/CD workflow
- Lista de runs con fecha, commit, status
</details>

### Ejercicio 5: Simular trigger manual

(NO EJECUTAR si estás en producción)

1. Ve al workflow que tiene `workflow_dispatch`
2. Identifica qué inputs acepta
3. (Opcional) Ejecuta manualmente si es seguro

<details>
<summary>Solución</summary>

```bash
# Ver workflow con workflow_dispatch
cat .github/workflows/deploy-digitalocean.yml | grep -A10 "workflow_dispatch"

# Inputs típicos:
# - environment (production/staging)
# - service (gateway, authservice, etc.)
```
</details>

---

## 🎉 ¡Felicidades!

Has completado el Tutorial 12. Ahora sabes:
- ✅ Fundamentos de GitHub Actions
- ✅ Estructura de workflows (YAML)
- ✅ Configurar y usar Secrets
- ✅ Triggers (push, PR, manual, schedule)
- ✅ Jobs, steps y dependencies
- ✅ Actions del marketplace
- ✅ Variables de entorno y contextos
- ✅ Matrix builds para múltiples servicios

---

**Anterior:** [11 - Zero-downtime Deployments](./11-zero-downtime-deployments.md)  
**Siguiente:** [13 - Smart CI/CD](./13-smart-cicd.md)
