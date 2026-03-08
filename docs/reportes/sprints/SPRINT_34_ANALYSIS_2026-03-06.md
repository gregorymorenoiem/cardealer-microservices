# Sprint 34 — Codebase Analysis & Task Identification

**Fecha:** 2026-03-06  
**Tipo:** Pre-Sprint Analysis  
**Analizado por:** CPSO

---

## 1. Service Inventory (25 Backend Services)

| #   | Service                        | Serilog | Observability | ErrorHandling | Audit | Health Triple | ResponseCompression | FluentValidation Pipeline | [Authorize] | try/catch/finally |
| --- | ------------------------------ | ------- | ------------- | ------------- | ----- | ------------- | ------------------- | ------------------------- | ----------- | ----------------- |
| 1   | AdminService                   | ✅      | ✅            | ✅            | ✅    | ✅            | ❌                  | ✅                        | ✅          | ✅                |
| 2   | AIProcessingService            | ✅      | ✅            | ✅            | ✅    | ✅            | ❌                  | ✅                        | ✅          | ✅                |
| 3   | AuditService                   | ✅      | ✅            | ✅            | ✅    | ✅            | ❌                  | ✅                        | ✅          | ✅                |
| 4   | AuthService                    | ✅      | ✅            | ✅            | ✅    | ✅            | ❌                  | ✅                        | ✅          | ✅                |
| 5   | BillingService                 | ✅      | ✅            | ✅            | ✅    | ✅            | ❌                  | ✅                        | ✅          | ✅                |
| 6   | ChatbotService                 | ✅      | ✅            | ✅            | ✅    | ✅            | ❌                  | ✅                        | ⚠️          | ✅                |
| 7   | **ComparisonService**          | ❌      | ❌            | ❌            | ❌    | ❌            | ❌                  | ❌                        | ✅          | ❌                |
| 8   | ContactService                 | ✅      | ✅            | ✅            | ✅    | ✅            | ❌                  | ✅                        | ✅          | ✅                |
| 9   | CRMService                     | ✅      | ✅            | ✅            | ✅    | ✅            | ❌                  | ✅                        | ✅          | ✅                |
| 10  | DealerAnalyticsService         | ✅      | ✅            | ✅            | ✅    | ✅            | ❌                  | ✅                        | ✅          | ✅                |
| 11  | ErrorService                   | ✅      | ✅            | ✅            | ✅    | ✅            | ❌                  | ✅                        | ✅          | ✅                |
| 12  | Gateway                        | ✅      | ✅            | ✅            | ✅    | ✅            | ✅                  | N/A                       | N/A         | ✅                |
| 13  | KYCService                     | ✅      | ✅            | ✅            | ✅    | ✅            | ❌                  | ✅                        | ✅          | ✅                |
| 14  | MediaService                   | ✅      | ✅            | ✅            | ✅    | ✅            | ❌                  | ✅                        | ✅          | ✅                |
| 15  | NotificationService            | ✅      | ✅            | ✅            | ✅    | ✅            | ❌                  | ✅                        | ⚠️          | ✅                |
| 16  | RecoAgent                      | ✅      | ✅            | ✅            | ✅    | ✅            | ❌                  | ✅                        | ✅          | ✅                |
| 17  | RecommendationService          | ✅      | ✅            | ✅            | ✅    | ✅            | ❌                  | ✅                        | ✅          | ✅                |
| 18  | **ReportsService**             | ✅      | ✅            | ✅            | ✅    | ✅            | ❌                  | ❌                        | 🔴          | ✅                |
| 19  | ReviewService                  | ✅      | ✅            | ✅            | ✅    | ✅            | ❌                  | ✅                        | ⚠️          | ✅                |
| 20  | RoleService                    | ✅      | ✅            | ✅            | ✅    | ✅            | ❌                  | ✅                        | ✅          | ✅                |
| 21  | SearchAgent                    | ✅      | ✅            | ✅            | ✅    | ✅            | ❌                  | ✅                        | ✅          | ✅                |
| 22  | SupportAgent                   | ✅      | ✅            | ✅            | ✅    | ✅            | ❌                  | ✅                        | ⚠️          | ✅                |
| 23  | UserService                    | ✅      | ✅            | ✅            | ✅    | ✅            | ❌                  | ✅                        | ✅          | ✅                |
| 24  | **VehicleIntelligenceService** | ❌      | ❌            | ❌            | ❌    | ❌            | ❌                  | ✅                        | ✅          | ❌                |
| 25  | VehiclesSaleService            | ✅      | ✅            | ✅            | ✅    | ✅            | ✅                  | ✅                        | ✅          | ✅                |

**Legend:** 🔴 = Critical missing | ⚠️ = Partial/Issues | ❌ = Missing | ✅ = Present

---

## 2. CRITICAL Security Findings

### 🔴 FINDING S-01: ReportsService — ALL 4 controllers have ZERO [Authorize] (P0)

**Severity:** CRITICAL  
**Impact:** Any anonymous user can access reports, dashboards, schedules, and content reports.

| Controller                | File                                                                         | Endpoints Exposed       |
| ------------------------- | ---------------------------------------------------------------------------- | ----------------------- |
| ReportsController         | `ReportsService/ReportsService.Api/Controllers/ReportsController.cs`         | CRUD on reports         |
| DashboardsController      | `ReportsService/ReportsService.Api/Controllers/DashboardsController.cs`      | CRUD on dashboards      |
| ReportSchedulesController | `ReportsService/ReportsService.Api/Controllers/ReportSchedulesController.cs` | CRUD on schedules       |
| ContentReportsController  | `ReportsService/ReportsService.Api/Controllers/ContentReportsController.cs`  | CRUD on content reports |

**Fix:** Add `[Authorize]` to all 4 controller classes. Admin-only operations need `[Authorize(Roles = "Admin")]`.

### 🔴 FINDING S-02: ReportsService — NO MediatR pipeline = FluentValidation BYPASSED (P0)

**Severity:** CRITICAL  
**Impact:** Despite having `SecurityValidators.cs` and FluentValidation in the project, the controllers directly call repositories. There is NO MediatR, NO `ValidationBehavior`, and NO `AddValidatorsFromAssembly`. SQL injection and XSS validators are **completely dead code**.

**File:** `ReportsService/ReportsService.Api/Program.cs` — No `AddMediatR`, No `AddValidatorsFromAssembly`, No `ValidationBehavior` registration.

**Fix:** Either refactor to CQRS+MediatR pattern or add manual validation in controllers.

### 🟠 FINDING S-03: ChatbotService ChatController — No class-level [Authorize] (P1)

**Severity:** HIGH  
**Impact:** 8 endpoints (StartSession, SendMessage, EndSession, TransferToAgent, GetSession, GetSessionMessages, GetActiveSessionCount) have NO auth. Only `health` ([AllowAnonymous]) and `handoff/*` ([Authorize]) have explicit decorators.

**File:** `ChatbotService/ChatbotService.Api/Controllers/ChatController.cs:13`

**Fix:** Add `[Authorize]` at class level, then `[AllowAnonymous]` on health and public chat start endpoints.

### 🟠 FINDING S-04: SupportAgent — All endpoints [AllowAnonymous] (P1)

**Severity:** HIGH (by design, but needs review)  
**Impact:** The SupportAgentController has only `[AllowAnonymous]` on all 3 endpoints. While this may be intentional for a public support chatbot, it means anyone can flood the support system.

**File:** `SupportAgent/SupportAgent.Api/Controllers/SupportAgentController.cs`

**Note:** Has rate limiting per IP, but no authentication. Review if `message` endpoint should require auth for non-anonymous flows.

### 🟡 FINDING S-05: NotificationService deferred findings (from Sprint 32)

**Severity:** MEDIUM  
**Impact:** 11 findings deferred from Sprint 32:

- S-04: InternalNotificationsController needs API key defense-in-depth
- S-07: ScheduledNotifications needs admin-only or user scoping
- S-06: 6 non-MediatR controllers missing FluentValidation
- S-02/S-05/S-09: IDOR issues on notification status, price alerts, saved searches
- S-08: CronExpression input not validated
- S-12: Template content stored XSS risk
- S-13: Webhook signature verification degrades instead of failing closed

---

## 3. Standardization Findings

### 🔴 FINDING STD-01: ComparisonService — Completely unstandardized (P0)

**Severity:** CRITICAL  
**File:** `ComparisonService/ComparisonService.Api/Program.cs`

Missing ALL standard patterns:

- ❌ NO `UseStandardSerilog` — uses built-in ILogger only
- ❌ NO `AddStandardObservability` — no OpenTelemetry/Jaeger
- ❌ NO `AddStandardErrorHandling` — no ProblemDetails middleware
- ❌ NO `AddAuditPublisher` — no audit trail
- ❌ NO health triple — only single `app.MapHealthChecks("/health")`
- ❌ NO `try/catch/finally` + `Log.CloseAndFlush()` pattern
- ❌ NO `AddResponseCompression`
- ❌ NO MediatR or FluentValidation pipeline — no input validation
- ❌ NO `MicroserviceSecretsConfiguration.GetJwtConfig()` — uses raw `Jwt:Secret` config
- ❌ `app.UseHttpsRedirection()` unconditional (should be `!IsProduction`)
- ❌ No `UseRequestLogging()`
- ❌ No `UseAuditMiddleware()`
- ❌ Only references `CarDealer.Shared` (missing Logging, Observability, ErrorHandling, Audit project references)

### 🔴 FINDING STD-02: VehicleIntelligenceService — Completely unstandardized (P0)

**Severity:** CRITICAL  
**File:** `VehicleIntelligenceService/VehicleIntelligenceService.Api/Program.cs`

Missing ALL standard patterns:

- ❌ NO `UseStandardSerilog` — uses `Console.WriteLine`
- ❌ NO `AddStandardObservability` — no OpenTelemetry/Jaeger
- ❌ NO `AddStandardErrorHandling` — no ProblemDetails middleware
- ❌ NO `AddAuditPublisher` — no audit trail
- ❌ NO health triple — only single `app.MapHealthChecks("/health")`
- ❌ NO `try/catch/finally` + `Log.CloseAndFlush()` pattern
- ❌ NO `AddResponseCompression`
- ❌ NO `MicroserviceSecretsConfiguration.GetJwtConfig()` — uses raw `Jwt:Key` config
- ❌ Hardcoded fallback connection string: `"Host=postgres_db;Port=5432;Database=vehicleintelligenceservice;Username=postgres;Password=postgres"`
- ❌ No `UseRequestLogging()`
- ❌ No `UseAuditMiddleware()`
- ❌ Only references `CarDealer.Shared` (missing Logging, Observability, ErrorHandling, Audit project references)
- ❌ Missing `public partial class Program { }` for test accessibility

### 🟡 FINDING STD-03: Response Compression missing in 23/25 services (P2)

**Severity:** MEDIUM  
**Impact:** Only Gateway and VehiclesSaleService have `AddResponseCompression`. All other services return uncompressed JSON, increasing bandwidth costs and response times.

**Services missing response compression:** AdminService, AIProcessingService, AuditService, AuthService, BillingService, ChatbotService, ComparisonService, ContactService, CRMService, DealerAnalyticsService, ErrorService, KYCService, MediaService, NotificationService, RecoAgent, RecommendationService, ReportsService, ReviewService, RoleService, SearchAgent, SupportAgent, UserService, VehicleIntelligenceService

---

## 4. Dockerfile Findings

### ✅ All 25 Dockerfiles use EXPOSE 8080 — No port 80 issues

### 🟠 FINDING DF-01: CRMService Dockerfile — Missing shared lib csproj COPY for restore layer (P2)

**File:** `CRMService/Dockerfile`  
**Issue:** The Dockerfile only copies `CarDealer.Shared` and `CarDealer.Contracts` csproj files before `dotnet restore`, but `CRMService.Api.csproj` references `CarDealer.Shared.Audit`, `CarDealer.Logging`, `CarDealer.Observability`, `CarDealer.ErrorHandling`. The restore step may fail because those csproj files aren't present yet. The later `COPY . .` adds them but runs AFTER restore.

**Fix:** Add COPY commands for all referenced shared lib csproj files before the restore step.

### 🟠 FINDING DF-02: ReportsService Dockerfile — Same issue as CRMService (P2)

**File:** `ReportsService/Dockerfile`  
**Same pattern** — only copies `CarDealer.Shared` and `CarDealer.Contracts` but the service references additional shared libs via its csproj.

---

## 5. Test Coverage Findings

### Shared Libraries: ✅ COMPLETE

- `CarDealer.Shared.Tests` — 171 tests covering 16 libraries (ApiVersioning, Audit, Caching, ErrorHandling, FeatureFlags, HealthChecks, Idempotency, Logging, Messaging, Observability, Persistence, RateLimiting, Resilience, Sagas, Secrets, ServiceDiscovery)
- `CarDealer.Contracts.Tests` — Separate test project ✅

### 🟡 FINDING T-01: 5 services WITHOUT in-service test projects (P2)

| Service                    | In-Service Tests | \_Tests/ Legacy                                |
| -------------------------- | ---------------- | ---------------------------------------------- |
| AIProcessingService        | ❌               | ❌                                             |
| DealerAnalyticsService     | ❌               | ✅ (`_Tests/DealerAnalyticsService.Tests`)     |
| RecommendationService      | ❌               | ✅ (`_Tests/RecommendationService.Tests`)      |
| ReviewService              | ❌               | ✅ (`_Tests/ReviewService.Tests`)              |
| VehicleIntelligenceService | ❌               | ✅ (`_Tests/VehicleIntelligenceService.Tests`) |

---

## 6. Frontend State

### ✅ Good baseline:

- Next.js App Router with proper route groups: `(auth)`, `(main)`, `(admin)`, `(messaging)`
- 16 unit/integration test files (Vitest)
- 10+ E2E test specs (Playwright)
- Vitest config with coverage settings
- pnpm ✅ (not npm/yarn)
- 28+ component folders organized by feature
- OG image generation (`opengraph-image.tsx`)
- SEO files: `sitemap.ts`, `robots.ts`
- Error boundaries: `error.tsx`, `global-error.tsx`, `not-found.tsx`

### 🟡 FINDING FE-01: Frontend test coverage is light relative to component count (P3)

- 28+ component folders vs 16 test files
- Most tests are integration-level; few unit tests for individual components
- Services with tests: auth, vehicles, sellers, checkout, messaging, notifications, favorites, history

---

## 7. Sprint 32 Deferred Items

From the Sprint 32 report, these remain incomplete:

- [ ] ContactService full CQRS implementation
- [ ] Frontend vehicle search performance audit

---

## 8. Prioritized Sprint 34 Task List

### 🔴 P0 — Critical Security & Standardization (MUST DO)

| Task     | Description                                                                                                                                                          | Estimated Effort | Files                           |
| -------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------- | ------------------------------- |
| **34.1** | ReportsService: Add `[Authorize]` to all 4 controllers                                                                                                               | 30 min           | 4 controller files              |
| **34.2** | ReportsService: Add MediatR + ValidationBehavior or manual validation                                                                                                | 2 hrs            | Program.cs + controllers        |
| **34.3** | ComparisonService: Full Program.cs standardization (Serilog, Observability, ErrorHandling, Audit, health triple, try/catch/finally, JWT config, ResponseCompression) | 3 hrs            | Program.cs, .csproj, Dockerfile |
| **34.4** | VehicleIntelligenceService: Full Program.cs standardization + remove hardcoded conn string                                                                           | 3 hrs            | Program.cs, .csproj, Dockerfile |

### 🟠 P1 — High Security

| Task     | Description                                                                                          | Estimated Effort | Files                     |
| -------- | ---------------------------------------------------------------------------------------------------- | ---------------- | ------------------------- |
| **34.5** | ChatbotService ChatController: Add class-level `[Authorize]`, `[AllowAnonymous]` on public endpoints | 30 min           | ChatController.cs         |
| **34.6** | SupportAgent: Review auth strategy, add per-user rate limiting or session tokens                     | 1 hr             | SupportAgentController.cs |

### 🟡 P2 — Medium Priority

| Task     | Description                                                                                                                                           | Estimated Effort | Files                     |
| -------- | ----------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------- | ------------------------- |
| **34.7** | Add ResponseCompression (Brotli+Gzip) to remaining high-traffic services (AuthService, KYCService, BillingService, MediaService, NotificationService) | 2 hrs            | 5 Program.cs files        |
| **34.8** | Fix CRMService + ReportsService Dockerfiles: Add missing shared lib csproj COPY commands                                                              | 1 hr             | 2 Dockerfiles             |
| **34.9** | NotificationService: Address deferred S-04, S-06, S-07 findings                                                                                       | 2 hrs            | Multiple controller files |

### 🔵 P3 — Lower Priority / Tech Debt

| Task      | Description                                                                         | Estimated Effort | Files            |
| --------- | ----------------------------------------------------------------------------------- | ---------------- | ---------------- |
| **34.10** | Move 4 legacy test projects from `_Tests/` into their respective services           | 1 hr             | Directory moves  |
| **34.11** | Create test project for AIProcessingService                                         | 2 hrs            | New test project |
| **34.12** | Frontend: Add unit tests for critical components (search, vehicle card, auth forms) | 3 hrs            | New test files   |
| **34.13** | Complete deferred Sprint 32 items (ContactService CQRS, search perf audit)          | Variable         | Multiple         |

---

## 9. Recommended Sprint 34 Scope (1 week)

**Focus:** Security hardening + standardization of the last 2 unstandardized services.

**Selected tasks:** 34.1, 34.2, 34.3, 34.4, 34.5, 34.6, 34.8

**Expected outcomes:**

- 0 controllers without [Authorize] (currently 4 critical)
- 0 unstandardized services (currently 2 of 25)
- 0 bypassed FluentValidation pipelines (currently 1)
- All Dockerfiles structurally correct for shared lib references

**Total estimated effort:** ~12 hours

---

## 10. Metrics Summary

| Metric                                       | Current                | After Sprint 34 |
| -------------------------------------------- | ---------------------- | --------------- |
| Total backend services                       | 25                     | 25              |
| Fully standardized Program.cs                | 23/25 (92%)            | 25/25 (100%)    |
| Services with [Authorize] on all controllers | 21/25                  | 25/25           |
| Services with FluentValidation pipeline      | 22/25                  | 24/25           |
| Services with response compression           | 2/25 (8%)              | 7/25 (28%)      |
| Shared library test coverage                 | 171 tests / 16 libs ✅ | 171+            |
| Frontend test files                          | 16 + 10 E2E            | 16+             |
| Dockerfiles on port 8080                     | 25/25 ✅               | 25/25           |
