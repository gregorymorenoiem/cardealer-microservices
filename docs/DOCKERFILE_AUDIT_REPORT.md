# 🐳 Dockerfile Audit Report — OKLA Microservices Platform

**Date:** 2026-03-07  
**Scope:** 25 service-level Dockerfiles + 1 Python LLM server + compose.yaml  
**Auditor:** Copilot (Claude)

---

## Executive Summary

| Metric                           | Value                               |
| -------------------------------- | ----------------------------------- |
| Total Dockerfiles audited        | 26 (25 .NET + 1 Python)             |
| Critical findings                | 4                                   |
| High findings                    | 6                                   |
| Medium findings                  | 8                                   |
| Low / Informational              | 5                                   |
| Services with all best practices | 0 (every file has at least 1 issue) |

---

## 🔴 CRITICAL — Immediate Fix Required

### C1. Broken HEALTHCHECK — `wget` not installed (3 services)

The following services define a `HEALTHCHECK` using `wget` but **never install it** in the Alpine runtime image. This means the healthcheck will ALWAYS FAIL, causing Docker/K8s to consider the container unhealthy.

| Service                 | Dockerfile                              | Line                        |
| ----------------------- | --------------------------------------- | --------------------------- |
| **AdminService**        | `AdminService/Dockerfile` L53–54        | Uses `wget` — not installed |
| **AuthService**         | `AuthService/Dockerfile` L53–54         | Uses `wget` — not installed |
| **ErrorService**        | `ErrorService/Dockerfile` L56–57        | Uses `wget` — not installed |
| **NotificationService** | `NotificationService/Dockerfile` L59–60 | Uses `wget` — not installed |

**Impact:** Containers will be killed/restarted continuously by K8s liveness probes when Docker's native HEALTHCHECK is consulted.

**Fix:** Add `RUN apk add --no-cache wget` before `USER appuser` in each Dockerfile.

---

### C2. `COPY . .` Copies Entire Backend Directory (19 services)

Most Dockerfiles use `COPY . .` inside the build stage. Since `compose.yaml` sets `context: ./backend`, this copies ALL services' source code, all test projects, READMEs, Dockerfiles, scripts, and documentation into every single build context.

**Affected services (19/25):** AIProcessingService, AdminService, AuditService, AuthService, BillingService, CRMService, ComparisonService, ContactService, DealerAnalyticsService, ErrorService, Gateway, KYCService, MediaService, NotificationService, RecoAgent, RoleService, SearchAgent, SupportAgent, UserService, VehiclesSaleService.

**Services with selective copy (better pattern):** ChatbotService, RecommendationService, ReportsService, ReviewService, VehicleIntelligenceService.

**Impact:**

- Bloated build context (~hundreds of MB extra sent to Docker daemon)
- Cache invalidation: ANY change in ANY service triggers rebuild of ALL services
- Test projects, `.md` files, other Dockerfiles end up in the build image

**Fix:** Either use selective COPY (like ChatbotService does) or ensure the `.dockerignore` is comprehensive. The existing `backend/.dockerignore` does exclude `**/bin/`, `**/obj/`, `**/*.md`, `**/TestResults/` which helps, but doesn't exclude other services' source code.

---

### C3. Gateway Missing `/run/secrets` Ownership

In `Gateway/Dockerfile` L44–50, the secrets directory is created:

```dockerfile
RUN mkdir -p /run/secrets
# ...
RUN addgroup -g 1000 appuser && \
    adduser -D -u 1000 -G appuser appuser && \
    chown -R appuser:appuser /app   # ← /run/secrets NOT included
```

The `appuser` cannot write to `/run/secrets` because ownership is only set on `/app`.

**Fix:** Change to `chown -R appuser:appuser /app /run/secrets`.

---

### C4. MediaService.Workers Dockerfile is Empty

`MediaService/MediaService.Workers/Dockerfile` exists but contains **no content**. If referenced in a build pipeline, it will fail.

**Fix:** Either remove it or populate with proper content.

---

## 🟠 HIGH — Should Fix Before Next Deployment

### H1. Inconsistent HEALTHCHECK Timing Parameters

| Service                        | `--timeout` | `--start-period` | Divergent?              |
| ------------------------------ | ----------- | ---------------- | ----------------------- |
| **BillingService**             | **3s**      | **5s**           | ⚠️ Both too low         |
| **ChatbotService**             | 10s         | **5s**           | ⚠️ Low start-period     |
| **CRMService**                 | 10s         | **5s**           | ⚠️ Low start-period     |
| **ReportsService**             | 10s         | **5s**           | ⚠️ Low start-period     |
| **ComparisonService**          | 10s         | **60s**          | ⚠️ Unusually high start |
| **RecommendationService**      | **3s**      | 40s              | ⚠️ Timeout too low      |
| **ReviewService**              | **3s**      | 40s              | ⚠️ Timeout too low      |
| **VehicleIntelligenceService** | **3s**      | 40s              | ⚠️ Timeout too low      |
| **(Standard — 17 services)**   | 10s         | 40s              | ✅                      |

**Impact:** A 3s timeout is aggressive for services that connect to databases, RabbitMQ, etc. during startup. A 5s start-period gives .NET services almost no time to boot before health checks begin.

**Recommended standard:** `--interval=30s --timeout=10s --start-period=40s --retries=3`

---

### H2. ChatbotService Uses `ASPNETCORE_ENVIRONMENT=Production` Instead of `Docker`

`ChatbotService/Dockerfile` L6:

```dockerfile
ENV ASPNETCORE_ENVIRONMENT=Production
```

All other 24 .NET services use `ASPNETCORE_ENVIRONMENT=Docker`. This means ChatbotService will NOT load `appsettings.Docker.json`, potentially using wrong connection strings, ports, or configurations.

**Fix:** Change to `ENV ASPNETCORE_ENVIRONMENT=Docker`.

---

### H3. ChatbotService Uses `curl` Instead of `wget`

`ChatbotService/Dockerfile` L8 installs `curl` and L56 uses `curl --fail` in HEALTHCHECK, while all other Alpine-based services use `wget`. This:

- Increases attack surface (curl is a larger binary)
- Breaks consistency

**Fix:** Replace with `wget` for consistency:

```dockerfile
RUN apk add --no-cache wget
HEALTHCHECK ... CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health || exit 1
```

---

### H4. Missing `--chown` on COPY in Runtime Stage (5 services)

The following services copy published files **without `--chown`**, meaning files are owned by `root` inside the container, even though the app runs as `appuser`:

| Service                        | Line                                              |
| ------------------------------ | ------------------------------------------------- |
| **CRMService**                 | `COPY --from=publish /app/publish .` (no --chown) |
| **RecommendationService**      | `COPY --from=publish /app/publish .`              |
| **ReportsService**             | `COPY --from=publish /app/publish .`              |
| **ReviewService**              | `COPY --from=publish /app/publish .`              |
| **VehicleIntelligenceService** | `COPY --from=publish /app/publish .`              |

**Impact:** If the app needs to write to its own directory (e.g., temp files, logs), it will fail with permission denied. While the app DLLs are read-only in production, this is a defense-in-depth concern.

**Fix:** Use `COPY --from=publish --chown=appuser:appuser /app/publish .`

---

### H5. RecommendationService & ReviewService Missing `-r linux-x64` in Publish

```dockerfile
# RecommendationService/Dockerfile L43
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false
# ReviewService/Dockerfile L43
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false
# VehicleIntelligenceService/Dockerfile L33
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false
```

All other services specify `-r linux-x64 --self-contained false`. Without the RID, the publish output is portable but may include extra dependencies or have slight runtime differences on DOKS (linux/amd64).

**Fix:** Add `-r linux-x64 --self-contained false` for consistency.

---

### H6. VehiclesSaleService Dockerfile Has Trailing Debug/Trigger Comments

`VehiclesSaleService/Dockerfile` L59–65:

```dockerfile
# Wed Feb 25 09:48:42 AST 2026
# trigger 1772028282
# network-host fix
# ci-trigger: GITHUB_TOKEN auth fix
```

These are CI trigger artifacts that leaked into the Dockerfile. They're harmless but unprofessional and can cause confusion.

**Fix:** Remove the trailing comments.

---

## 🟡 MEDIUM — Improve When Possible

### M1. Inconsistent User/Group Creation Patterns (5 variants found)

| Pattern                                                                                      | Services                                                                                                           |
| -------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------ | ----------------------------- | --- | ------------------------- | ---------------- |
| `addgroup -g 1000 appuser && adduser -D -u 1000 -G appuser appuser`                          | AIProcessing, Admin, Auth, CRM, Chatbot, Comparison, Error, Gateway, KYC, Notification, Reports, VehiclesSale (12) |
| `adduser -D -u 1000 appuser` (no explicit group)                                             | AuditService, Contact, DealerAnalytics, RecoAgent, RoleService, SearchAgent, SupportAgent, UserService (8)         |
| `addgroup -g 1000 appgroup && adduser -D -u 1000 -G appgroup appuser` (different group name) | BillingService, VehicleIntelligence (2)                                                                            |
| `addgroup -S appgroup && adduser -S -G appgroup appuser` (system user, NO UID)               | Recommendation, ReviewService (2)                                                                                  |
| `getent group                                                                                |                                                                                                                    | addgroup ... && getent passwd |     | adduser ...` (idempotent) | MediaService (1) |

**Impact:** The `-S` (system user) flag in RecommendationService and ReviewService creates a user with a random low UID (typically 100+), which is different from the explicit `1000` UID used everywhere else. This could cause permission issues in mounted volumes.

**Recommendation:** Standardize on:

```dockerfile
RUN addgroup -g 1000 appuser && \
    adduser -D -u 1000 -G appuser appuser
```

---

### M2. Redundant `dotnet build` Before `dotnet publish` (11 services)

`dotnet publish` implicitly builds. Running `dotnet build` first is redundant and adds ~30–60s to build time:

**Affected:** AIProcessingService, AuditService, CRMService, DealerAnalyticsService, RecommendationService, ReportsService, ReviewService, RoleService, UserService, VehicleIntelligenceService, VehiclesSaleService.

**Not affected (correct pattern):** AdminService, AuthService, BillingService, ComparisonService, ContactService, ErrorService, Gateway, KYCService, MediaService, NotificationService, ChatbotService, RecoAgent, SearchAgent, SupportAgent.

**Fix:** Remove the `RUN dotnet build` line; `dotnet publish` handles it.

---

### M3. `.dockerignore` Coverage Gap

Only **2 `.dockerignore` files** exist for the entire backend:

- `backend/.dockerignore` (shared, used when `context: ./backend`)
- `backend/VehiclesSaleService/.dockerignore` (service-specific, but not used since context is `./backend`)

The `backend/.dockerignore` does exclude `**/bin/`, `**/obj/`, `**/*.md`, `**/TestResults/`, `.git/`, `.vs/`, etc. However it does NOT exclude:

- `**/Dockerfile*` (other services' Dockerfiles)
- `**/*.sln` (solution files)
- `**/context.md` (matched by `**/*.md`)
- `**/*.sh` (shell scripts)
- `**/prometheus*.yml` (monitoring configs)

The VehiclesSaleService `.dockerignore` is **unused** because the Docker build context is `./backend`, not `./backend/VehiclesSaleService`.

**Fix:** Enhance `backend/.dockerignore` to exclude more non-essential files.

---

### M4. `COPY . .` in Build Stage Defeats Layer Caching for Multi-Service Changes

While the csproj-first pattern is correctly used for NuGet restore caching, the `COPY . .` that follows copies ALL of `./backend`. This means a change in AuthService's source code **invalidates the build cache for VehiclesSaleService** and vice versa.

**Ideal fix:** Use selective copy like ChatbotService:

```dockerfile
COPY _Shared/ _Shared/
COPY AuthService/ AuthService/
```

---

### M5. Base Stage Pattern Inconsistency

Three patterns are used for the runtime stage:

| Pattern                                                                       | Services                                   |
| ----------------------------------------------------------------------------- | ------------------------------------------ |
| **Linear** (`FROM sdk → FROM aspnet` at the end)                              | 20 services                                |
| **Base alias** (`FROM aspnet AS base` at top, `FROM base AS final` at bottom) | CRMService, ChatbotService, ReportsService |
| **Mixed**                                                                     | —                                          |

The "base alias" pattern installs wget/creates dirs as root at the top, then the final stage inherits. This is fine functionally but inconsistent with the majority.

---

### M6. DealerAnalyticsService HEALTHCHECK Missing `--no-verbose`

`DealerAnalyticsService/Dockerfile` L60:

```dockerfile
CMD wget --spider http://localhost:8080/health || exit 1
```

All other wget-based healthchecks use `--no-verbose --tries=1 --spider`. Missing `--no-verbose` produces noisy logs; missing `--tries=1` means wget may retry (default 20).

---

### M7. AdminService HEALTHCHECK Uses Different wget Flags

`AdminService/Dockerfile` L54:

```dockerfile
CMD wget --no-verbose --tries=1 http://localhost:8080/health -qO- > /dev/null || exit 1
```

All other services use `--spider` (HEAD request only). AdminService downloads the full response body and discards it. This is slightly less efficient.

---

### M8. CRMService and ChatbotService Use `curl --fail` in Base Stage

CRMService installs `wget` in the base stage but ChatbotService installs `curl`. Both install health check tools in a `base` stage that runs as root. After `USER appuser`, if any process needs these tools, they work — but the pattern divergence is notable.

---

## 🔵 LOW / INFORMATIONAL

### L1. Base Image Consistency ✅ GOOD

All 25 .NET services use:

- **Build:** `mcr.microsoft.com/dotnet/sdk:8.0`
- **Runtime:** `mcr.microsoft.com/dotnet/aspnet:8.0-alpine`

This is consistent and correct for .NET 8 on Alpine.

The Python LLM server (`ChatbotService/LlmServer/Dockerfile`) uses `python:3.11-slim` which is appropriate for its use case.

---

### L2. Port Configuration ✅ GOOD

All 25 .NET services correctly:

- `EXPOSE 8080`
- `ENV ASPNETCORE_URLS=http://+:8080`

This matches the OKLA standard (all services on port 8080 internally).

---

### L3. Multi-Stage Build ✅ GOOD

All 25 .NET services use proper multi-stage builds: `build → publish → final`. The SDK image is only used in build stages; the slim Alpine runtime is used for the final image.

---

### L4. No Hardcoded Secrets ✅ GOOD

No Dockerfiles contain hardcoded secrets, API keys, or connection strings. All services use environment variables that are expected to be injected at runtime.

---

### L5. `--platform=$BUILDPLATFORM` Used Correctly ✅ GOOD

All build stages use `FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0`, enabling cross-compilation from macOS ARM64 to linux/amd64. The `-r linux-x64` in publish ensures correct target architecture.

⚠️ Exception: RecommendationService, ReviewService, and VehicleIntelligenceService omit `-r linux-x64` (see H5).

---

## Comparative Matrix

| Service                    | wget | non-root | --chown | HEALTHCHECK  | secrets dir | build step redundant | -r linux-x64 | COPY pattern | Overall |
| -------------------------- | ---- | -------- | ------- | ------------ | ----------- | -------------------- | ------------ | ------------ | ------- |
| AIProcessingService        | ✅   | ✅       | ✅      | ✅           | ✅          | ⚠️                   | ✅           | COPY . .     | 🟡      |
| AdminService               | ❌   | ✅       | ✅      | ❌ broken    | ✅          | ✅                   | ✅           | COPY . .     | 🔴      |
| AuditService               | ✅   | ✅       | ✅      | ✅           | ✅          | ⚠️                   | ✅           | COPY . .     | 🟡      |
| AuthService                | ❌   | ✅       | ✅      | ❌ broken    | ✅          | ✅                   | ✅           | COPY . .     | 🔴      |
| BillingService             | ✅   | ✅       | ✅      | ⚠️ timing    | ✅          | ✅                   | ✅           | COPY . .     | 🟠      |
| CRMService                 | ✅   | ✅       | ❌      | ⚠️ timing    | ✅          | ⚠️                   | ✅           | COPY . .     | 🟠      |
| ChatbotService             | curl | ✅       | ❌      | ⚠️ timing    | ❌          | ⚠️                   | ✅           | selective    | 🟠      |
| ComparisonService          | ✅   | ✅       | ✅      | ⚠️ 60s start | ✅          | ✅                   | ✅           | COPY . .     | 🟡      |
| ContactService             | ✅   | ✅       | ✅      | ✅           | ✅          | ✅                   | ✅           | COPY . .     | ✅      |
| DealerAnalyticsService     | ✅   | ✅       | ✅      | ⚠️ noisy     | ✅          | ⚠️                   | ✅           | COPY . .     | 🟡      |
| ErrorService               | ❌   | ✅       | ✅      | ❌ broken    | ✅          | ✅                   | ✅           | COPY . .     | 🔴      |
| Gateway                    | ✅   | ✅       | ✅      | ✅           | ⚠️ no chown | ✅                   | ✅           | COPY . .     | 🟠      |
| KYCService                 | ✅   | ✅       | ✅      | ✅           | ✅          | ✅                   | ✅           | COPY . .     | ✅      |
| MediaService               | ✅   | ✅       | ✅      | ✅           | ✅          | ✅                   | ✅           | COPY . .     | ✅      |
| NotificationService        | ❌   | ✅       | ✅      | ❌ broken    | ✅          | ✅                   | ✅           | COPY . .     | 🔴      |
| RecoAgent                  | ✅   | ✅       | ✅      | ✅           | ✅          | ✅                   | ✅           | COPY . .     | ✅      |
| RecommendationService      | ✅   | ⚠️ -S    | ❌      | ⚠️ timing    | ✅          | ⚠️                   | ❌           | selective    | 🟠      |
| ReportsService             | ✅   | ✅       | ❌      | ⚠️ timing    | ✅          | ⚠️                   | ✅           | selective    | 🟠      |
| ReviewService              | ✅   | ⚠️ -S    | ❌      | ⚠️ timing    | ✅          | ⚠️                   | ❌           | selective    | 🟠      |
| RoleService                | ✅   | ✅       | ✅      | ✅           | ✅          | ⚠️                   | ✅           | COPY . .     | 🟡      |
| SearchAgent                | ✅   | ✅       | ✅      | ✅           | ✅          | ✅                   | ✅           | COPY . .     | ✅      |
| SupportAgent               | ✅   | ✅       | ✅      | ✅           | ✅          | ✅                   | ✅           | COPY . .     | ✅      |
| UserService                | ✅   | ✅       | ✅      | ✅           | ✅          | ⚠️                   | ✅           | COPY . .     | 🟡      |
| VehicleIntelligenceService | ✅   | ✅       | ❌      | ⚠️ timing    | ✅          | ⚠️                   | ❌           | selective    | 🟠      |
| VehiclesSaleService        | ✅   | ✅       | ✅      | ✅           | ✅          | ⚠️                   | ✅           | COPY . .     | 🟡      |

---

## Recommended Golden Dockerfile Template

```dockerfile
# syntax=docker/dockerfile:1
# Build stage
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1 \
    DOTNET_NOLOGO=1 \
    NUGET_XMLDOC_MODE=skip \
    DOTNET_NUGET_SIGNATURE_VERIFICATION=false

# 1. Copy shared libraries (csproj only for restore caching)
COPY ["_Shared/CarDealer.Shared/CarDealer.Shared.csproj", "_Shared/CarDealer.Shared/"]
COPY ["_Shared/CarDealer.Contracts/CarDealer.Contracts.csproj", "_Shared/CarDealer.Contracts/"]

# 2. Copy service csproj files
COPY ["MyService/MyService.Api/MyService.Api.csproj", "MyService/MyService.Api/"]
COPY ["MyService/MyService.Application/MyService.Application.csproj", "MyService/MyService.Application/"]
COPY ["MyService/MyService.Domain/MyService.Domain.csproj", "MyService/MyService.Domain/"]
COPY ["MyService/MyService.Infrastructure/MyService.Infrastructure.csproj", "MyService/MyService.Infrastructure/"]

# 3. Restore (cached unless csproj changes)
RUN dotnet restore "MyService/MyService.Api/MyService.Api.csproj"

# 4. Copy source code (selective, not COPY . .)
COPY _Shared/ _Shared/
COPY MyService/ MyService/

# 5. Publish (no separate build step needed)
FROM build AS publish
WORKDIR "/src/MyService/MyService.Api"
RUN dotnet publish "MyService.Api.csproj" -c Release -o /app/publish -r linux-x64 --self-contained false /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app
EXPOSE 8080

# Install wget for healthcheck
RUN apk add --no-cache wget

# Create non-root user and secrets directory
RUN addgroup -g 1000 appuser && \
    adduser -D -u 1000 -G appuser appuser && \
    mkdir -p /run/secrets && \
    chown -R appuser:appuser /app /run/secrets

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Docker

USER appuser
COPY --from=publish --chown=appuser:appuser /app/publish .

HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "MyService.Api.dll"]
```

---

## Priority Action Items

| Priority | Action                                                                                             | Effort | Services Affected |
| -------- | -------------------------------------------------------------------------------------------------- | ------ | ----------------- |
| 🔴 P0    | Add `apk add --no-cache wget` to AdminService, AuthService, ErrorService, NotificationService      | 5 min  | 4                 |
| 🔴 P0    | Fix Gateway `/run/secrets` ownership                                                               | 2 min  | 1                 |
| 🟠 P1    | Standardize HEALTHCHECK timings to `10s/40s`                                                       | 15 min | 8                 |
| 🟠 P1    | Fix ChatbotService `ASPNETCORE_ENVIRONMENT=Production` → `Docker`                                  | 2 min  | 1                 |
| 🟠 P1    | Add `--chown=appuser:appuser` to COPY in CRM, Recommendation, Reports, Review, VehicleIntelligence | 10 min | 5                 |
| 🟠 P1    | Add `-r linux-x64 --self-contained false` to Recommendation, Review, VehicleIntelligence publish   | 5 min  | 3                 |
| 🟡 P2    | Remove redundant `dotnet build` from 11 services                                                   | 15 min | 11                |
| 🟡 P2    | Standardize user creation pattern                                                                  | 20 min | 10                |
| 🟡 P2    | Remove trailing CI comments from VehiclesSaleService                                               | 2 min  | 1                 |
| 🟡 P2    | Switch `COPY . .` to selective copy across all services                                            | 2 hr   | 19                |
| 🔵 P3    | Enhance `backend/.dockerignore`                                                                    | 10 min | all               |
| 🔵 P3    | Delete empty MediaService.Workers/Dockerfile                                                       | 1 min  | 1                 |
