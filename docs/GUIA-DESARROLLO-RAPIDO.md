# Guía: Desarrollo Rápido sin Probar en Producción — OKLA

> **Problema actual:** Push a `main` → CI/CD 90 min → Producción → Se descubren bugs.  
> **Objetivo:** Código probado localmente en minutos → PR → rama `staging` (QA branch, sin servidor) → QA local con `--profile business` → `main` → Producción.

---

## Índice

1. [El flujo correcto](#1-el-flujo-correcto)
2. [Paridad de producción local — setup único](#2-paridad-de-producción-local--setup-único)
3. [Ambiente local en segundos](#3-ambiente-local-en-segundos)
4. [Pruebas rápidas antes del push](#4-pruebas-rápidas-antes-del-push)
5. [Rama staging: checkpoint de QA sin servidor](#5-rama-staging-checkpoint-de-qa-sin-servidor)
6. [Optimizar el CI/CD para que vuele](#6-optimizar-el-cicd-para-que-vuele)
7. [Comandos de referencia rápida](#7-comandos-de-referencia-rápida)
8. [Checklist antes de cada push](#8-checklist-antes-de-cada-push)

---

## 2. Paridad de producción local — setup único

> Esta sección explica cómo replicar producción (DigitalOcean K8s) localmente en tu Mac M5.

### 2.0 Diagnóstico de tu máquina vs. producción

| Recurso              | Tu Mac M5       | Docker Desktop (actual) | Producción K8s  |
| -------------------- | --------------- | ----------------------- | --------------- |
| CPU cores            | 10 físicos      | **4 (bajo)**            | ~11 cores total |
| RAM                  | 24 GB           | **7.7 GB (bajo)**       | ~13 GB requests |
| Arquitectura         | ARM64 (aarch64) | ARM64 ✅                | amd64 ⚠️ diffs  |
| Servicios en prod    | —               | —                       | 47 deployments  |
| Stack completo local | —               | —                       | ~17 GB (33 svc) |

**El problema raíz:** Docker Desktop solo tiene 7.7 GB asignados pero el stack completo necesita ~17 GB. Al intentar `docker compose up -d` sin perfiles, todos los servicios se matan entre sí por OOM.

---

### 2.1 Paso 1: Darle más RAM a Docker Desktop (HACER UNA VEZ)

1. Abrir **Docker Desktop** → ⚙️ Settings → **Resources**
2. Configurar:
   - **CPU**: `8` (de tus 10 disponibles)
   - **Memory**: `18 GB` (de tus 24 GB)
   - **Swap**: `2 GB`
   - **Disk image size**: lo que tengas libre (tienes 480 GB)
3. Clic en **Apply & Restart**

> Dejas 6 GB para macOS + apps del sistema. Con 18 GB en Docker puedes correr cualquier tier.

---

### 2.2 Paso 2: Variables de entorno (HACER UNA VEZ)

```bash
# Desde la raíz del repo
cp .env.local.example .env
# Editar .env con tus valores reales (AWS keys, OAuth, etc.)
# .env ya está en .gitignore — nunca se va a commitear
```

Las variables en `.env` usan los mismos nombres que los K8s ConfigMaps/Secrets de producción. Así el mismo `appsettings.json` funciona en ambos lados.

---

### 2.3 Los 5 tiers — qué levantar según lo que estés desarrollando

El `compose.yaml` usa [Docker Compose profiles](https://docs.docker.com/compose/profiles/) para segmentar los 33 servicios en grupos de uso. Esto reemplaza tener que recordar qué servicios levantar.

```
TIER          PERFIL        RAM        CONTENIDO
────────────────────────────────────────────────────────────────
Infra         (ninguno)     ~2.4 GB    postgres + redis + rabbitmq
                                       + consul + seq + jaeger
Core          core          +3.2 GB    gateway + authservice
            ─────────────  ─────────  + userservice + roleservice
            Acumulado:      5.6 GB     + errorservice
Vehículos     vehicles      +2.3 GB    vehiclessaleservice
            ─────────────  ─────────  + mediaservice + contactservice
            Acumulado:      7.9 GB     + notificationservice
IA/Chatbot    ai            +2.3 GB    vehicleintelligenceservice
            ─────────────  ─────────  + chatbotservice + recommendation
            Acumulado:      10.1 GB    + dealeranalytics
Negocio       business      +3.1 GB    billing + crm + admin + reviews
            ─────────────  ─────────  + audit + reports + comparison
            Acumulado:      13.2 GB    + kyc + azulpayment + spyne
Frontend      frontend      +4.0 GB    frontend-next (Next.js)
            ─────────────  ─────────  + frontend-web (Vite)
            Acumulado:      17.2 GB    STACK COMPLETO
```

**Regla práctica:** levanta solo el tier del feature en el que estás trabajando.

---

### 2.4 Comandos por escenario de trabajo

```bash
# ── Escenario A: Desarrollar AuthService / usuarios ────────────────────────
docker compose --profile core up -d
# → 5.6 GB, levanta en ~3 min
# Swagger: http://localhost:15001/swagger  (AuthService)
#          http://localhost:15002/swagger  (UserService)
#          http://localhost:18443/swagger  (Gateway)

# ── Escenario B: Desarrollar features de listado/vehículos ─────────────────
docker compose --profile core --profile vehicles up -d
# → 7.9 GB, levanta en ~4 min
# Swagger: http://localhost:15010/swagger  (VehiclesSaleService)
#          http://localhost:15020/swagger  (MediaService)

# ── Escenario C: Desarrollar Chatbot / IA ──────────────────────────────────
docker compose --profile core --profile vehicles --profile ai up -d
# → 10.1 GB

# ── Escenario D: Validar flujo completo antes de merge a staging ───────────
docker compose --profile core --profile vehicles --profile ai --profile business up -d
# → 13.2 GB (sin frontend — puedes correr pnpm dev por separado)

# ── Escenario E: Stack 100% equivalente a producción ──────────────────────
docker compose --profile core --profile vehicles --profile ai --profile business --profile frontend up -d
# → 17.2 GB — requiere haber configurado Docker Desktop a 18 GB (paso 2.1)

# ── Solo infraestructura (para desarrollar con dotnet watch run) ────────────
docker compose up -d
# → 2.4 GB — el mínimo para que los servicios .NET se conecten a la BD
```

---

### 2.5 Modo hot-reload: la forma más rápida de iterar

Levantar el servicio que estás desarrollando **fuera de Docker** con `dotnet watch run` y el resto en `docker compose`. Es **10× más rápido** que rebuilds de Docker.

```bash
# Terminal 1 — infraestructura (2.4 GB, siempre corriendo)
docker compose up -d

# Terminal 2 — el servicio que estás modificando (ejemplo: AuthService)
cd backend/AuthService
dotnet watch run --project AuthService.Api/AuthService.Api.csproj
# → Cambias un archivo → recarga en 2 segundos. Sin rebuild. Sin Docker.
```

El servicio se conecta a postgres/rabbitmq/redis que están en Docker. La connection string en `appsettings.Development.json` apunta a `localhost:5433` (PostgreSQL local).

---

### 2.6 Verificar que el ambiente está corriendo correctamente

```bash
# Ver estado de todos los servicios (healthy / starting / unhealthy)
docker compose ps

# Logs de un servicio específico
docker compose logs -f authservice

# Ver consumo de RAM/CPU por contenedor
docker stats --no-stream

# Probar el gateway (equivalente al ingress de producción)
curl http://localhost:18443/health
```

---

### 2.7 Diferencias conocidas local vs. producción

| Aspecto                  | Local (OKLA dev)                | Producción (K8s)                |
| ------------------------ | ------------------------------- | ------------------------------- |
| `ASPNETCORE_ENVIRONMENT` | `Development` (Swagger visible) | `Production`                    |
| TLS/HTTPS                | HTTP plano (localhost)          | HTTPS + cert Let's Encrypt      |
| Arch. Docker             | ARM64 (M5)                      | amd64 (DOKS nodes)              |
| Secretos                 | `.env` en texto plano           | K8s Secrets cifrados            |
| Replicas                 | 1 instancia por servicio        | HPA: 1-10 pods por servicio     |
| Base de datos            | Shared PostgreSQL (1 instancia) | StatefulSet por servicio en K8s |
| RabbitMQ                 | 1 nodo (sin clustering)         | Cluster 3 nodos                 |

**Lo que NO cambia (paridad real):**

- Mismas variables de entorno (mismos nombres que K8s ConfigMaps)
- Mismas imágenes Docker (ARM64 compila de los mismos Dockerfiles)
- Mismo flujo de datos: frontend → Gateway → Services → PostgreSQL
- Misma configuración de Ocelot (rutas del Gateway idénticas)
- Mismas migraciones de EF Core (se aplican al iniciar)

---

## 1. El flujo correcto

### Flujo actual (PROBLEMÁTICO)

```
Código → push main → CI 90min → Producción → "Oops, bug" → Fix → push main → CI 90min...
```

### Flujo correcto (SOLUCIÓN)

> ⚠️ **No hay servidor staging.** `staging` es una rama de control de calidad.  
> El QA de integración se ejecuta localmente con `--profile business` (13.2 GB).

```
feature/mi-feature
    │
    ├─ dotnet watch run / pnpm dev      ← PROBAR AQUÍ (segundos)
    ├─ dotnet test (unit only)          ← UNIT TESTS (< 30 seg)
    │
    ▼
Pull Request → PR Checks (build/lint/unit tests) → Aprobar
    │
    ▼
Merge a staging (rama QA — SIN servidor, no despliega)
    │
    ├─ QA LOCAL: docker compose --profile business up -d  (13.2 GB)
    ├─ Smoke tests manuales · Playwright E2E local
    └─ Si todo OK → PR: staging → main
    │
    ▼
Merge staging → main → GitHub Actions → Deploy a DOKS (Producción)
```

**Regla de oro:** Si tocas `main`, el código ya pasó QA local con el profile `business`. Producción es la última etapa, no el laboratorio de pruebas.

---

## 3. Ambiente local en segundos

### 3.1 Backend — Hot Reload sin Docker

El método más rápido para iterar en un microservicio:

```bash
# En una terminal — infraestructura (PostgreSQL, Redis, RabbitMQ)
docker compose -f compose.yaml up postgres_db redis rabbitmq -d

# En otra terminal — el servicio que estás desarrollando (ejemplo: AuthService)
cd backend/AuthService
dotnet watch run --project AuthService.Api/AuthService.Api.csproj
# ✅ Cambias un archivo → se recarga en ~2 segundos. No build de Docker.
```

**Tiempo:** 10 segundos desde cambio hasta ver resultado. Comparado con 20-30 min del CI.

### 3.2 Frontend — Hot Reload sin Docker

```bash
cd frontend/web-next
pnpm dev
# ✅ Fast Refresh de Next.js — cambios visibles en milisegundos.
```

> **NO uses `pnpm build` para iterar.** Solo úsalo justo antes de hacer el PR.

### 3.3 Servicio completo + dependencias (Docker Compose local)

Cuando necesitas probar integración real entre servicios:

```bash
# Solo los servicios críticos para frontend (definidos en compose.yaml)
docker compose up -d

# O solo los servicios que necesitas
docker compose up -d postgres_db redis rabbitmq authservice gateway

# Ver logs en tiempo real
docker compose logs -f authservice
```

### 3.4 Variables de entorno para desarrollo local

Crea un archivo `.env.local` en la raíz (ya está en `.gitignore`):

```bash
# .env.local — NO commitear
POSTGRES_PASSWORD=postgres
REDIS_PASSWORD=
JWT_SECRET_KEY=dev-secret-key-for-local-only-not-production
ASPNETCORE_ENVIRONMENT=Development
```

### 3.5 Swagger UI — Probar endpoints sin Postman

Con el servicio corriendo localmente:

- **AuthService:** http://localhost:15001/swagger
- **Gateway:** http://localhost:18443/swagger

---

## 4. Pruebas rápidas antes del push

Ejecuta esto **antes de cada `git push`**. Tarda menos de 2 minutos.

### 4.1 Script local pre-push (Backend)

```bash
# Desde la raíz del repo
# Solo el servicio que modificaste, no todo el monorepo
SERVICE="AuthService"  # Cambiar según lo que modificaste

cd backend/$SERVICE
dotnet build --configuration Release /p:TreatWarningsAsErrors=true && \
dotnet test --no-build \
  --filter "Category!=Integration&Category!=E2E" \
  --logger "console;verbosity=minimal" \
  --blame-hang-timeout 2min
```

### 4.2 Script local pre-push (Frontend)

```bash
cd frontend/web-next
pnpm typecheck && \
pnpm lint && \
CI=true pnpm test -- --run
# Si todo pasa en < 60 segundos, listo para push
```

### 4.3 Instalar el hook de pre-push

Ejecuta una sola vez:

```bash
cat > .git/hooks/pre-push << 'EOF'
#!/bin/bash
echo "🔍 [pre-push] Ejecutando chequeos rápidos..."

# Detectar qué cambió
CHANGED_BACKEND=$(git diff --name-only origin/main...HEAD 2>/dev/null | grep "^backend/" | cut -d'/' -f2 | sort -u | head -3)
CHANGED_FRONTEND=$(git diff --name-only origin/main...HEAD 2>/dev/null | grep "^frontend/")

# Frontend checks
if [ -n "$CHANGED_FRONTEND" ]; then
  echo "  → Frontend detectado, ejecutando typecheck + lint + tests..."
  cd frontend/web-next
  pnpm typecheck && pnpm lint && CI=true pnpm test -- --run
  if [ $? -ne 0 ]; then
    echo "❌ Frontend checks fallaron. Push bloqueado."
    exit 1
  fi
  cd ../..
fi

# Backend checks (solo servicios modificados)
for SVC in $CHANGED_BACKEND; do
  if [ -d "backend/$SVC" ] && [ -f "backend/$SVC/${SVC}.sln" ]; then
    echo "  → $SVC detectado, ejecutando build + tests unitarios..."
    cd "backend/$SVC"
    dotnet build --configuration Release /p:TreatWarningsAsErrors=true --no-restore -q && \
    dotnet test --no-build \
      --filter "Category!=Integration&Category!=E2E" \
      --logger "console;verbosity=minimal" \
      --blame-hang-timeout 2min 2>&1 | tail -5
    if [ $? -ne 0 ]; then
      echo "❌ $SVC tests fallaron. Push bloqueado."
      exit 1
    fi
    cd ../..
  fi
done

echo "✅ [pre-push] Todo OK. Procediendo con el push."
exit 0
EOF
chmod +x .git/hooks/pre-push
```

---

## 5. Rama staging: checkpoint de QA sin servidor

> **Contexto OKLA:** No existe un servidor staging separado. La rama `staging` actúa como  
> checkpoint de código QA-aprobado. El QA real se ejecuta localmente con `--profile business`.

### 5.1 Crear la rama staging (una sola vez)

```bash
git checkout -b staging
git push -u origin staging
```

### 5.2 Workflow de trabajo diario

```bash
# 1. Crear feature branch desde staging (no desde main)
git checkout staging
git pull origin staging
git checkout -b feature/nombre-de-tu-feature

# 2. Desarrollar con hot-reload (ver sección 3)
# Terminal 1: docker compose up -d
# Terminal 2: dotnet watch run / pnpm dev

# 3. Gate pre-commit → Push → PR: feature/... → staging
git push origin feature/nombre-de-tu-feature
# Abrir PR: feature/... → staging  (NUNCA hacia main)
```

### 5.3 QA local con stack completo (antes de PR staging → main)

Antes de mergear `staging → main`, levanta el stack de negocio completo localmente:

```bash
# Levantar flujo completo sin frontend (~13.2 GB)
docker compose --profile core --profile vehicles --profile business up -d

# Verificar que todos los servicios estén healthy
docker compose ps

# Correr smoke tests manuales o Playwright E2E
pnpm --prefix frontend/web-next exec playwright test --project=chromium

# Al terminar el QA, apagar para recuperar RAM
docker compose --profile core --profile vehicles --profile business down
```

> Solo si el QA local pasa sin errores se abre el PR `staging → main`.

### 5.4 Reglas de protección de ramas

Ve a **Settings → Branches → Branch protection rules** y aplica esto:

| Rama      | Regla                                                                  | Deploy          |
| --------- | ---------------------------------------------------------------------- | --------------- |
| `main`    | Require PR from `staging` only · Require 1 approval · No direct pushes | ✅ Auto → DOKS  |
| `staging` | Require PR · PR Checks must pass · No direct pushes                    | ❌ No despliega |

> **Resultado:** Nadie puede hacer push directo a `main`. Solo llega código que pasó QA local con `--profile business`.

---

## 6. Optimizar el CI/CD para que vuele

El CI actual tarda hasta 90 minutos. Con estos cambios debería bajar a 8-15 minutos.

### 6.1 Problema #1: Build Docker sin cache

**Actual:** Cada push rebuilds todo desde cero → 20-40 min por servicio.

**Solución — agregar cache en el step de `docker/build-push-action`:**

En `.github/workflows/smart-cicd.yml`, en cada step de build Docker, agregar:

```yaml
- name: "🐳 Build & Push AuthService"
  if: steps.filter.outputs.authservice == 'true' && github.ref == 'refs/heads/main'
  uses: docker/build-push-action@v5
  with:
    context: ./backend
    file: ./backend/AuthService/Dockerfile
    push: true
    tags: ghcr.io/${{ github.repository_owner }}/authservice:sha-${{ github.sha }}
    # ✅ AGREGAR ESTAS LÍNEAS:
    cache-from: type=gha,scope=authservice
    cache-to: type=gha,mode=max,scope=authservice
    platforms: linux/amd64
```

**Ahorro estimado:** De 20-40 min a 2-5 min por servicio cuando solo cambió código (no dependencias).

### 6.2 Problema #2: `dotnet restore` sin cache entre runs

En el job de CI backend, el cache de NuGet ya está configurado pero falta el cache del objeto de build:

```yaml
# Agregar después del cache de NuGet existente:
- name: "📦 Cache .NET build output"
  uses: actions/cache@v5
  with:
    path: |
      backend/**/bin
      backend/**/obj
    key: ${{ runner.os }}-dotnet-build-${{ hashFiles('**/*.csproj') }}-${{ github.sha }}
    restore-keys: |
      ${{ runner.os }}-dotnet-build-${{ hashFiles('**/*.csproj') }}-
      ${{ runner.os }}-dotnet-build-
```

### 6.3 Problema #3: No usar `--no-restore`

En el workflow, después de `dotnet restore`, asegurarse de pasar `--no-restore` en build y test:

```yaml
# ✅ Correcto
- run: dotnet restore
- run: dotnet build --no-restore /p:TreatWarningsAsErrors=true
- run: dotnet test --no-build --no-restore ...
```

### 6.4 Problema #4: Timeout de 90 minutos innecesario

El timeout actual es 90 min. Para el CI deberían bastar 20 min:

```yaml
jobs:
  ci:
    timeout-minutes: 20 # Reducir de 90 a 20
```

Si el CI tarda más de 20 minutos hay un problema que hay que investigar, no esperar.

### 6.5 Problema #5: Tests de integración mezclados con unitarios

Los tests de integración (que requieren PostgreSQL+RabbitMQ) **no deben correr en el CI principal**. Ya tienen el filtro pero verificar que todos los proyectos de tests etiqueten correctamente:

```csharp
// En cada test de integración, agregar el atributo:
[Trait("Category", "Integration")]
public class MiIntegrationTest { ... }

// En cada test E2E:
[Trait("Category", "E2E")]
public class MiE2ETest { ... }
```

### 6.6 Reducir el tiempo del frontend (pnpm cache mejorado)

En el workflow `pr-checks.yml`, el cache de pnpm ya existe. Agregar también cache del Next.js build:

```yaml
- name: "📦 Cache Next.js build"
  uses: actions/cache@v5
  with:
    path: frontend/web-next/.next/cache
    key: ${{ runner.os }}-nextjs-${{ hashFiles('frontend/web-next/pnpm-lock.yaml') }}-${{ hashFiles('frontend/web-next/src/**/*.{ts,tsx}') }}
    restore-keys: |
      ${{ runner.os }}-nextjs-${{ hashFiles('frontend/web-next/pnpm-lock.yaml') }}-
```

**Ahorro:** `pnpm build` baja de 4-8 min a 1-2 min en cambios menores.

---

## 7. Comandos de referencia rápida

### Desarrollo local (el 90% del tiempo)

```bash
# Levantar infraestructura + servicio en desarrollo
docker compose up -d postgres_db redis rabbitmq
cd backend/AuthService && dotnet watch run --project AuthService.Api

# Frontend
cd frontend/web-next && pnpm dev

# Ver todos los logs del sistema local
docker compose logs -f --tail=50

# Parar todo
docker compose down
```

### Antes de hacer un PR

```bash
# Backend (solo el servicio modificado)
cd backend/MiServicio
dotnet build /p:TreatWarningsAsErrors=true
dotnet test --filter "Category!=Integration&Category!=E2E" --no-build

# Frontend
cd frontend/web-next
pnpm typecheck && pnpm lint && CI=true pnpm test -- --run && pnpm build

# Si todo pasa → abre PR hacia staging
```

### Probar integración real antes de PR (opcional, 2-3 min)

```bash
# Levantar el servicio modificado en modo Docker local
docker compose -f compose.yaml up -d --build authservice
docker compose logs -f authservice

# Correr los tests de integración manualmente
cd backend/AuthService
dotnet test --filter "Category=Integration" \
  -e "ConnectionStrings__DefaultConnection=Host=localhost;Port=5433;..."
```

### Revisar qué está tardando el CI

```bash
# Ver el tiempo de cada job en el último workflow
gh run list --workflow=smart-cicd.yml --limit=5
gh run view <run-id> --log | grep "^[0-9]"
```

---

## 8. Checklist antes de cada push

Antes de `git push`, confirmar mentalmente estos puntos:

```
[ ] ¿Probé el feature localmente con `dotnet watch run` o `pnpm dev`?
[ ] ¿Corrí los unit tests del servicio? (< 30 seg)
[ ] ¿No hay errores de TypeScript o warnings de .NET?
[ ] ¿El PR apunta a `staging`, no a `main`?
[ ] ¿El PR tiene una descripción de qué cambié y cómo probarlo?
[ ] ¿Verifiqué los logs después del deploy a staging?
[ ] ¿Solo después de confirmar que funciona en staging, mergeo a `main`?
```

---

## Resumen del ahorro de tiempo

| Actividad                    | Antes                  | Después                    |
| ---------------------------- | ---------------------- | -------------------------- |
| Iterar en código (backend)   | 20-40 min (CI)         | 2 seg (hot reload)         |
| Iterar en código (frontend)  | 10-20 min (CI)         | ~0 (Fast Refresh)          |
| Descubrir un bug             | Al deployar a prod     | Antes de hacer push        |
| PR Checks                    | 10-15 min              | 3-5 min                    |
| CI completo (smart-cicd.yml) | 60-90 min              | 15-25 min                  |
| Deploy a staging             | — (no existía)         | 5-10 min                   |
| Deploy a producción          | 90 min + debug en prod | 15 min (código ya probado) |

---

> **Regla OKLA:** Producción no es un ambiente de pruebas. Si necesitas "ver si funciona", úsalo localmente o en staging. **Main es sagrado.**
