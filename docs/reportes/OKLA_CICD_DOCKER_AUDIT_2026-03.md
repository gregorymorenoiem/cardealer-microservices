# 🐳 OKLA — Auditoría CI/CD & Docker

**Fecha:** 2026-03-06
**Autor:** CPSO (Copilot)
**Scope:** GitHub Actions workflows, Dockerfiles (12 servicios), Docker Compose (5 archivos)

---

## 📊 Resumen Ejecutivo

| Área           | Estado General           | Hallazgos Críticos                                      |
| -------------- | ------------------------ | ------------------------------------------------------- |
| GitHub Actions | ⚠️ Funcional con riesgos | 4 críticos, 10 warnings                                 |
| Dockerfiles    | ✅ Bien estandarizados   | 3 críticos (LlmServer, tags flotantes, contexto amplio) |
| Docker Compose | ⚠️ Inconsistencias       | 6 warnings (puertos dev vs prod, archivos stale)        |

---

## 1. Matriz de Calidad de Dockerfiles

| Servicio            | Multi-stage | Alpine            | Non-root        | HEALTHCHECK | Port 8080 | Score    |
| ------------------- | ----------- | ----------------- | --------------- | ----------- | --------- | -------- |
| AuthService         | ✅ 3-stage  | ✅                | ✅ appuser:1000 | ✅ wget     | ✅        | **Good** |
| Gateway             | ✅ 3-stage  | ✅                | ✅ appuser:1000 | ✅ wget     | ✅        | **Good** |
| AdminService        | ✅ 3-stage  | ✅                | ✅ appuser:1000 | ✅ wget     | ✅        | **Good** |
| MediaService        | ✅ 3-stage  | ✅                | ✅ appuser:1000 | ✅ wget     | ✅        | **Good** |
| ErrorService        | ✅ 3-stage  | ✅                | ✅ appuser:1000 | ✅ wget     | ✅        | **Good** |
| NotificationService | ✅ 3-stage  | ✅                | ✅ appuser:1000 | ✅ wget     | ✅        | **Good** |
| ContactService      | ✅ 3-stage  | ✅                | ✅ appuser:1000 | ✅ wget     | ✅        | **Good** |
| VehiclesSaleService | ✅ 3-stage  | ✅                | ✅ appuser:1000 | ✅ wget     | ✅        | **Good** |
| SearchAgent         | ✅ 3-stage  | ✅                | ✅ appuser:1000 | ✅ wget     | ✅        | **Good** |
| ChatbotService      | ✅ 3-stage  | ✅                | ✅ appuser:1000 | ⚠️ curl     | ✅        | **Fair** |
| LlmServer (Python)  | ❌ Single   | ❌ slim           | ❌ Root         | ✅ curl     | ⚠️ 8000   | **Poor** |
| Frontend (Next.js)  | ✅ 3-stage  | ✅ node:20-alpine | ✅ nextjs:1001  | ✅ wget     | ✅        | **Good** |

---

## 2. Hallazgos CI/CD

### 🔴 Críticos

1. **Todas las imágenes tagged con `:latest` como tag primario** — No reproducible, rollbacks imprecisos. Solo `smart-cicd.yml` agrega SHA como tag secundario. Recomendación: SHA como tag principal.

2. **No hay image digest pinning en deployments K8s** — `kubectl rollout restart` fuerza re-pull de `:latest`. Sin SHA, no se sabe qué código corre.

3. **Single point of failure: Runner macOS ARM64 self-hosted** — Si la Mac no está disponible, toda la CI/CD se detiene. Sin fallback a GitHub-hosted runners.

4. **`docker/login-action` conflicto con macOS keychain** — En `deploy-staging.yml` se usa esta action que interfiere con el keychain locked. Otros workflows usan inyección manual de `auths{}`.

### 🟡 Warnings

5. **Trivy scan con `continue-on-error: true`** — Vulnerabilidades críticas no bloquean el pipeline.
6. **Frontend Docker build procede incluso si tests fallan** — Condición `if: always() && !cancelled()`.
7. **No hay firma de imágenes (Cosign/Notary)** — Supply chain sin verificación.
8. **NuGet vulnerability scan no bloquea en HIGH** — Solo bloquea en CRITICAL directos.
9. **ChatbotService tiene CI duplicada** — Se construye tanto en `smart-cicd.yml` como en su pipeline dedicada.
10. **Docker buildx cache local sin límite de tamaño** — Crece indefinidamente en disco del runner.

### ✅ Positivos

- Smart monorepo change detection (`dorny/paths-filter`) — solo construye servicios cambiados
- Reusable workflow templates — evita duplicación
- Security scan workflow existe (Gitleaks, NuGet vulns)
- PR quality checks (size, conventional commits, lint, type check)
- Concurrency groups con `cancel-in-progress: true`

---

## 3. Hallazgos Docker

### 🔴 Críticos

1. **LlmServer corre como root** — Riesgo de escalación de privilegios en container escape.
2. **Base images con tags flotantes** — `sdk:8.0`, `aspnet:8.0-alpine` sin digest pin. Updates silenciosos pueden romper builds.
3. **`COPY . .` copia todo el directorio backend** — Contexto de ~500MB+ por build. Solo se necesita el servicio target + `_Shared/`.

### 🟡 Warnings

4. ChatbotService usa `curl` en Alpine (inconsistente, agrega bloat).
5. VehiclesSaleService tiene comentarios de debug que rompen cache.
6. Patrones de creación de usuario inconsistentes entre servicios.
7. Dockerfiles duplicados (root + .Api/) en algunos servicios.
8. Solo VehiclesSaleService tiene su propio `.dockerignore`.
9. `DOTNET_NUGET_SIGNATURE_VERIFICATION=false` en todos los builds.

---

## 4. Hallazgos Docker Compose

### 🟡 Warnings

1. **Dev compose usa puerto 80, no 8080** — Inconsistente con producción, puede enmascarar bugs.
2. **`compose.docker.yaml` es stale** — Referencias a nombres de servicios antiguos (`productservice`), Dockerfiles incorrectos, puertos `:80`.
3. **Credenciales default hardcodeadas** — `password`, `guest`, `clave-super-secreta-desarrollo-32-caracteres-aaa`.
4. **Sin resource limits en `compose.yaml`** — Un container puede consumir toda la memoria del host.
5. **Inconsistencia de nombres de red** — `compose.yaml` usa `okla-net`, `compose.docker.yaml` usa `cardealer-net`.

### ✅ Positivos

- Health checks en todos los servicios de infraestructura (PostgreSQL, Redis, RabbitMQ)
- PgBouncer para connection pooling
- Backups automatizados de PostgreSQL (retención 7 días)
- Docker Secrets pattern en `compose.secrets.yaml`
- Archivos `.dockerignore` comprensivos

---

## 5. Top 10 Recomendaciones (Priorizadas)

| #   | Recomendación                                                | Impacto                                       | Esfuerzo |
| --- | ------------------------------------------------------------ | --------------------------------------------- | -------- |
| 1   | **Usar commit SHA como tag primario de imagen**              | Deployments reproducibles, rollbacks precisos | Medio    |
| 2   | **Agregar runner de fallback** (GitHub-hosted ubuntu-latest) | Elimina SPOF en CI/CD                         | Medio    |
| 3   | **Fix LlmServer Dockerfile** — non-root user, Alpine Python  | Cierra brecha de seguridad                    | Bajo     |
| 4   | **Hacer Trivy blocking para CRITICAL**                       | Previene CVEs críticos en producción          | Bajo     |
| 5   | **Pin base images con digest** + Dependabot/Renovate         | Builds reproducibles                          | Medio    |
| 6   | **Deduplicar CI de ChatbotService**                          | Ahorra minutos CI, evita confusión            | Bajo     |
| 7   | **Archivar/eliminar compose.docker.yaml stale**              | Reduce confusión para nuevos devs             | Bajo     |
| 8   | **Template estandarizado de Dockerfile** con build args      | Mantenimiento más fácil de 20+ Dockerfiles    | Alto     |
| 9   | **Image signing con Cosign**                                 | Seguridad en supply chain                     | Medio    |
| 10  | **Scopear contexto de Docker build por servicio**            | Builds más rápidos, menos transferencia       | Alto     |

---

## 6. Integración con Sprint 18

Estas recomendaciones deben incorporarse al Sprint 18 como:

**Developer Tasks:**

- 18.1: Cambiar tag strategy a commit SHA en smart-cicd.yml
- 18.2: Fix LlmServer Dockerfile (non-root, Alpine)
- 18.3: Hacer Trivy blocking para CRITICAL

**CPSO Tasks:**

- 18.6: Deduplicar CI de ChatbotService
- 18.7: Limpiar compose.docker.yaml stale
- 18.8: Estandarizar healthcheck pattern (wget) en ChatbotService
