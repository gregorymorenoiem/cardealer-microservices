# IMPLEMENTATION PLAN: AuthService Compliance

## 📋 Gap Analysis

Based on the audit against the 15 Architectural Policies, the following gaps were identified in `backend\AuthService`:

| Policy | Requirement | Status | Gap Description |
|--------|-------------|--------|-----------------|
| **06** | Health Checks | ⚠️ Partial | Missing `/health/ready` and `/health/live` endpoints. |
| **07** | Observability | ❌ Missing | OpenTelemetry (Tracing/Metrics) is not installed or configured. |
| **09** | Resilience | ❌ Missing | Polly (Circuit Breaker, Retry) is not configured for HTTP Clients. |
| **10** | Documentation | ❌ Missing | Missing `README.md`, `CHANGELOG.md`, `ARCHITECTURE.md`, `TROUBLESHOOTING.md`. XML comments not enabled. |

---

## 🛠️ Implementation Steps

### Step 1: Observability (Policy 07)
**Goal**: Enable OpenTelemetry for Tracing and Metrics.

1.  **Add NuGet Packages** (`AuthService.Api.csproj`):
    *   `OpenTelemetry.Extensions.Hosting`
    *   `OpenTelemetry.Instrumentation.AspNetCore`
    *   `OpenTelemetry.Instrumentation.EntityFrameworkCore`
    *   `OpenTelemetry.Instrumentation.Http`
    *   `OpenTelemetry.Exporter.OpenTelemetryProtocol`
2.  **Configure OpenTelemetry** (`Program.cs`):
    *   Configure Tracing (AspNetCore, EF Core, HttpClient).
    *   Configure Metrics (AspNetCore, Runtime).
    *   Configure OTLP Exporter (endpoint from `appsettings.json`).

### Step 2: Resilience (Policy 09)
**Goal**: Implement Circuit Breaker and Retry patterns using Polly.

1.  **Add NuGet Packages** (`AuthService.Infrastructure.csproj`):
    *   `Microsoft.Extensions.Http.Polly`
2.  **Configure Polly Policies** (`ServiceCollectionExtensions.cs`):
    *   Define `GetRetryPolicy()` (Exponential Backoff).
    *   Define `GetCircuitBreakerPolicy()`.
    *   Apply policies to `NotificationServiceClient`, `ExternalTokenValidator`, and `ExternalServiceHealthCheck`.

### Step 3: Health Checks (Policy 06)
**Goal**: Implement Liveness and Readiness probes.

1.  **Update Middleware** (`Program.cs`):
    *   Map `/health/live` (Predicate: `_ => false` or basic check).
    *   Map `/health/ready` (Predicate: `check => check.Tags.Contains("ready")`).
    *   Keep `/health` for general status.

### Step 4: Documentation (Policy 10)
**Goal**: Complete project documentation.

1.  **Enable XML Comments** (`AuthService.Api.csproj`):
    *   Add `<GenerateDocumentationFile>true</GenerateDocumentationFile>`.
    *   Suppress warning 1591.
2.  **Configure Swagger** (`Program.cs`):
    *   Include XML comments in SwaggerGen.
3.  **Create Documentation Files**:
    *   `README.md`
    *   `CHANGELOG.md`
    *   `ARCHITECTURE.md`
    *   `TROUBLESHOOTING.md`

---

## ✅ Verification Plan

1.  **Build**: `dotnet build` must pass without errors.
2.  **Health**: `curl http://localhost:5000/health/live` returns 200 OK.
3.  **Swagger**: `http://localhost:5000/swagger` shows XML comments.
4.  **Resilience**: Disconnect `NotificationService` and verify Retry/Circuit Breaker logs.
5.  **Observability**: Verify traces appear in Jaeger/Aspire Dashboard.
