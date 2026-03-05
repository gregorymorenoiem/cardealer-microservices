# 🚀 OKLA Platform — Comprehensive Audit & Action Plan

**Date:** March 5, 2026
**Auditor:** Platform Engineering Audit (Automated Deep Analysis)
**Platform:** OKLA Vehicle Marketplace — Dominican Republic

---

## 1. Executive Summary

OKLA is an ambitious vehicle marketplace with a **massive backend footprint** (47+ microservices), a feature-rich **Next.js 16 frontend** (150+ pages), and an **early-stage Flutter mobile app**. The platform has strong architectural foundations — Clean Architecture, CQRS/MediatR, domain events, observability, and a mature CI/CD pipeline.

However, the audit reveals significant gaps between **built infrastructure and production readiness**:

- **15+ lazy-loaded frontend components** are placeholders (never created)
- **30+ TODO/FIXME items** in production code (frontend + backend)
- **Mobile app** has only 2 data sources (auth, vehicles) — needs 10+ more
- **Admin analytics** returns **placeholder/dummy data** instead of real metrics
- **Payment integrations** (Azul, Stripe) have incomplete signature verification
- **Push notifications** are stubbed — `IPushNotificationService` is commented out
- **5 backend services** are missing from K8s manifests (will fail in prod)
- **Exchange rate is hardcoded** (DOP 58.5) instead of live BCRD feed

### Key Metrics

| Area                      | Status                                       | Score  |
| ------------------------- | -------------------------------------------- | ------ |
| Frontend Web (Pages)      | 150+ pages built                             | 🟢 85% |
| Frontend Web (Components) | 15 placeholder components                    | 🟡 70% |
| Backend Services          | 47+ services built                           | 🟢 90% |
| Backend Integration       | TODO items in critical paths                 | 🟡 75% |
| Mobile App                | Scaffold only, 2 data sources                | 🔴 25% |
| CI/CD                     | 10 workflows, reusable templates             | 🟢 85% |
| K8s Infrastructure        | 48 services deployed, 5 missing              | 🟡 80% |
| Testing                   | 16 frontend tests, 40+ backend test projects | 🟡 65% |
| Security                  | CSRF, sanitization, JWT — some gaps          | 🟡 75% |
| Monitoring                | Prometheus + Grafana + OTEL configured       | 🟢 80% |
| SEO                       | Sitemap + robots.txt implemented             | 🟢 80% |
| PWA                       | Service worker exists, manifest missing      | 🟡 60% |

---

## 2. Current Platform Status — What's Working

### 2.1 Frontend Web (Next.js 16)

**Working well:**

- ✅ **150+ pages** built across auth, main, admin, dealer, and messaging groups
- ✅ **Homepage** with dynamic sections, sponsored dealers, vehicle categories
- ✅ **Vehicle listing & detail** with OKLA Score, 360° view, seller contact
- ✅ **Search** with filters (make, model, year, price, location, fuel type)
- ✅ **Vehicle publishing** flow with photo upload and AI import from Facebook
- ✅ **Dealer dashboard** with inventory, leads, analytics, employees, billing
- ✅ **Admin panel** — 30+ pages (users, dealers, KYC, vehicles, logs, etc.)
- ✅ **Authentication** — login, register, OAuth, password reset, email verification
- ✅ **KYC verification** flow with document upload
- ✅ **Comparison page** at `/comparar`
- ✅ **Favorites, history, alerts, saved searches** pages exist
- ✅ **Messaging** system with conversations
- ✅ **Checkout** flow with Azul and Stripe integration
- ✅ **Advertising system** — campaigns, catalogs, tracking, ROI analytics
- ✅ **CRM** integration for dealer leads
- ✅ **Help center** with categories and articles
- ✅ **Legal pages** — terms, privacy, cookies
- ✅ **SEO** — `sitemap.ts`, `robots.ts`, metadata in layout
- ✅ **PWA** — `sw.js` exists, `service-worker-provider.tsx` component
- ✅ **Offline page** at `/offline`
- ✅ **47 service clients** in `src/services/` covering all backend APIs
- ✅ **Security** — CSRF (`csrf.tsx`), sanitization (`sanitize.ts`), rate limiting (`rate-limit.ts`)

### 2.2 Backend (.NET 8 Microservices)

**Working well:**

- ✅ **47+ microservices** with Clean Architecture (Api, Application, Domain, Infrastructure)
- ✅ **Gateway** with Ocelot — 285 routes to 34 downstream services
- ✅ **AuthService** — full auth with JWT, OAuth, 2FA, phone verification
- ✅ **UserService** — users, dealers, sellers, roles, privacy
- ✅ **VehiclesSaleService** — CRUD, catalog, favorites, history, leads, homepage sections
- ✅ **MediaService** — upload, processing
- ✅ **BillingService** — Stripe, Azul, subscriptions, invoices, OKLA coins
- ✅ **NotificationService** — email, push, templates, scheduling, webhooks
- ✅ **AdminService** — dashboard, analytics, dealers, content, moderation
- ✅ **KYCService** — profiles, documents, identity verification, STR, watchlist
- ✅ **AdvertisingService** — campaigns, catalog, rotation, tracking, reports
- ✅ **CRMService** — leads, deals, activities, pipelines
- ✅ **ReviewService** — reviews CRUD
- ✅ **ChatbotService** — chat, WhatsApp, configuration
- ✅ **ComparisonService** — vehicle comparisons
- ✅ **AlertService** — price alerts, saved searches
- ✅ **DealerAnalyticsService** — analytics, benchmarks, conversion funnel, insights
- ✅ **ReportsService** — reports, dashboards, schedules
- ✅ **All services have Dockerfiles** (50 Dockerfiles found)
- ✅ **All core services have test projects** (40+ test directories)

### 2.3 Infrastructure

- ✅ **48 K8s service definitions** in `k8s/services.yaml`
- ✅ **K8s manifests** — deployments, HPA, PDB, RBAC, network policies, topology
- ✅ **MVP deployment** set in `k8s/mvp/`
- ✅ **10 GitHub Actions workflows** including reusable templates
- ✅ **Prometheus + Grafana + OTEL** monitoring stack
- ✅ **Data seeding** with comprehensive plan and JSON data

### 2.4 Mobile App (Flutter)

- ✅ **Clean Architecture** scaffold (core, data, domain, presentation)
- ✅ **18 pages** — home, search, detail, favorites, messages, auth, etc.
- ✅ **BLoC state management** for auth, vehicles, search, favorites, etc.
- ✅ **Firebase integration** — messaging, analytics, crashlytics
- ✅ **Localization** support (`flutter_localizations`, `intl`)
- ✅ **GoRouter** navigation with bottom nav shell

---

## 3. Feature Gap Analysis

### 🔴 P0 — Critical (Blocks Launch / Revenue)

| #    | Gap                                                                                                                     | Location                                                                                                                                                                                                                                                                          | Impact                                       | Effort |
| ---- | ----------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------- | ------ |
| P0-1 | **Hardcoded exchange rate** (DOP 58.5) — must fetch live from BCRD                                                      | [route.ts](frontend/web-next/src/app/api/score/calculate/route.ts#L78), [constants.ts](frontend/web-next/src/lib/constants.ts#L8)                                                                                                                                                 | All price calculations wrong when DOP shifts | 2d     |
| P0-2 | **Azul payment signature verification** is TODO — payments can be spoofed                                               | [AzulPaymentPageController.cs](backend/BillingService/BillingService.Api/Controllers/AzulPaymentPageController.cs#L175)                                                                                                                                                           | **Security: fake payment confirmations**     | 3d     |
| P0-3 | **StripePaymentForm & AzulPaymentForm** are placeholder components — checkout UI incomplete                             | [lazy/index.tsx](frontend/web-next/src/components/lazy/index.tsx#L239-L245)                                                                                                                                                                                                       | Users can't complete real payments           | 5d     |
| P0-4 | **Push notifications stubbed** — `IPushNotificationService` commented out                                               | [IPushNotificationService.cs](backend/NotificationService/NotificationService.Domain/Interfaces/IPushNotificationService.cs#L7), [RabbitMQNotificationConsumer.cs](backend/NotificationService/NotificationService.Infrastructure/Messaging/RabbitMQNotificationConsumer.cs#L375) | No push notifications to users/dealers       | 5d     |
| P0-5 | **Admin analytics returns placeholder data** — dashboard useless for business decisions                                 | [AnalyticsHandlers.cs](backend/AdminService/AdminService.Application/UseCases/Analytics/AnalyticsHandlers.cs#L9)                                                                                                                                                                  | Platform operators can't see real metrics    | 3d     |
| P0-6 | **Checkout page TODO** — payment SDK fields not integrated                                                              | [checkout/page.tsx](<frontend/web-next/src/app/(main)/checkout/page.tsx#L236>)                                                                                                                                                                                                    | Incomplete payment flow                      | 5d     |
| P0-7 | **Missing K8s manifests** for 5 services (recoagent, taxcompliance, spyneintegration, eventtracking, complianceservice) | [k8s/services.yaml](k8s/services.yaml), [k8s/deployments.yaml](k8s/deployments.yaml)                                                                                                                                                                                              | Services won't deploy to production          | 2d     |
| P0-8 | **Dealer billing** returns TODO for actual usage tracking                                                               | [DealerBillingController.cs](backend/BillingService/BillingService.Api/Controllers/DealerBillingController.cs#L283)                                                                                                                                                               | Dealers see wrong usage data                 | 3d     |

### 🟠 P1 — High Priority (Core User Experience)

| #     | Gap                                                                       | Location                                                                                                                                                                                                                                                                                                                                                             | Impact                                   | Effort |
| ----- | ------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------- | ------ |
| P1-1  | **Phone reveal analytics** not tracked                                    | [seller-card.tsx](frontend/web-next/src/components/vehicle-detail/seller-card.tsx#L97), [seller-contact-card.tsx](frontend/web-next/src/components/vehicle-detail/seller-contact-card.tsx#L108)                                                                                                                                                                      | Can't measure lead quality               | 1d     |
| P1-2  | **Chat widget analytics** not tracked (3 TODO items)                      | [VehicleChatWidget.tsx](frontend/web-next/src/components/vehicle-detail/VehicleChatWidget.tsx#L34-L40), [ChatWidget.tsx](frontend/web-next/src/components/chat/ChatWidget.tsx#L28-L34)                                                                                                                                                                               | Missing engagement metrics               | 1d     |
| P1-3  | **ContactModal** not implemented — users can't contact sellers from modal | [lazy/index.tsx](frontend/web-next/src/components/lazy/index.tsx#L215)                                                                                                                                                                                                                                                                                               | Friction in contact flow                 | 3d     |
| P1-4  | **ShareModal** not implemented — can't share vehicles on social media     | [lazy/index.tsx](frontend/web-next/src/components/lazy/index.tsx#L221)                                                                                                                                                                                                                                                                                               | Missing viral/organic growth             | 2d     |
| P1-5  | **Lightbox** not implemented — can't zoom vehicle photos                  | [lazy/index.tsx](frontend/web-next/src/components/lazy/index.tsx#L227)                                                                                                                                                                                                                                                                                               | Poor photo viewing experience            | 2d     |
| P1-6  | **ComparisonModal** not implemented — comparison feature incomplete       | [lazy/index.tsx](frontend/web-next/src/components/lazy/index.tsx#L233)                                                                                                                                                                                                                                                                                               | Feature exists but modal missing         | 2d     |
| P1-7  | **Map components** not implemented (Map, DealerMap)                       | [lazy/index.tsx](frontend/web-next/src/components/lazy/index.tsx#L168-L174)                                                                                                                                                                                                                                                                                          | No map-based search or dealer locations  | 5d     |
| P1-8  | **VideoPlayer** not implemented — can't play vehicle videos               | [lazy/index.tsx](frontend/web-next/src/components/lazy/index.tsx#L191)                                                                                                                                                                                                                                                                                               | Missing video showcase capability        | 2d     |
| P1-9  | **Admin promotions page** uses mock data                                  | [admin/promociones/page.tsx](<frontend/web-next/src/app/(admin)/admin/promociones/page.tsx#L88>)                                                                                                                                                                                                                                                                     | Admin can't manage promotions            | 2d     |
| P1-10 | **Admin early-bird page** uses mock data                                  | [admin/early-bird/page.tsx](<frontend/web-next/src/app/(admin)/admin/early-bird/page.tsx#L61>)                                                                                                                                                                                                                                                                       | Can't manage early bird program          | 2d     |
| P1-11 | **Admin transactions page** uses mock data                                | [admin/transacciones/page.tsx](<frontend/web-next/src/app/(admin)/admin/transacciones/page.tsx#L95>)                                                                                                                                                                                                                                                                 | Can't view real transactions             | 2d     |
| P1-12 | **Dealer inventory bulk delete** not implemented                          | [dealer/inventario/page.tsx](<frontend/web-next/src/app/(main)/dealer/inventario/page.tsx#L321>)                                                                                                                                                                                                                                                                     | Dealers can't bulk manage inventory      | 1d     |
| P1-13 | **Stripe webhook notifications** to dealers are TODO                      | [StripeWebhooksController.cs](backend/BillingService/BillingService.Api/Controllers/StripeWebhooksController.cs#L406-L497)                                                                                                                                                                                                                                           | Dealers don't get payment notifications  | 2d     |
| P1-14 | **AuthService event publishing** — 5 TODO items for RabbitMQ events       | [RequestUnlinkCodeCommandHandler.cs](backend/AuthService/AuthService.Application/Features/ExternalAuth/Commands/RequestUnlinkCode/RequestUnlinkCodeCommandHandler.cs#L143), [LinkExternalAccountCommandHandler.cs](backend/AuthService/AuthService.Application/Features/ExternalAuth/Commands/LinkExternalAccount/LinkExternalAccountCommandHandler.cs#L107), others | Missing audit trail for auth actions     | 3d     |
| P1-15 | **AdminService email notifications** for employee actions are TODO        | [PlatformEmployeeHandlers.cs](backend/AdminService/AdminService.Application/UseCases/PlatformEmployees/PlatformEmployeeHandlers.cs#L77)                                                                                                                                                                                                                              | Staff doesn't get onboarding emails      | 1d     |
| P1-16 | **WhatsApp dealer routing** is TODO                                       | [WhatsAppController.cs](backend/ChatbotService/ChatbotService.Api/Controllers/WhatsAppController.cs#L138)                                                                                                                                                                                                                                                            | WhatsApp messages can't route to dealers | 2d     |
| P1-17 | **Dealer statistics aggregation** is TODO                                 | [DealersController.cs](backend/DealerManagementService/DealerManagementService.Api/Controllers/DealersController.cs#L237)                                                                                                                                                                                                                                            | Dealer stats unavailable                 | 2d     |
| P1-18 | **Contact page error toast** not implemented                              | [contacto/page.tsx](<frontend/web-next/src/app/(main)/contacto/page.tsx#L78>)                                                                                                                                                                                                                                                                                        | Silent failures on contact form          | 0.5d   |

### 🟡 P2 — Medium Priority (Growth & Competitive Features)

| #     | Gap                                                                                 | Location                                                                                                                                    | Impact                                     | Effort |
| ----- | ----------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------ | ------ |
| P2-1  | **PWA manifest missing** — `manifest.ts` or `manifest.json` not found in `src/app/` | `src/app/manifest.ts` (file does not exist)                                                                                                 | Can't install as PWA / Add to Home Screen  | 1d     |
| P2-2  | **Multi-language support** — frontend is Spanish-only, no i18n framework            | `package.json` (no `next-intl` or similar)                                                                                                  | Limits market to Spanish speakers only     | 10d    |
| P2-3  | **Structured data (JSON-LD)** for vehicle listings — not implemented                | No `schema.org` or `json-ld` references found                                                                                               | Poor search engine rich results            | 3d     |
| P2-4  | **Vehicle history report** (Carfax-like) — no service exists                        | No VehicleHistoryService found                                                                                                              | Missing competitive differentiator         | 15d    |
| P2-5  | **Financing calculator** — no page or component exists                              | No financing page found                                                                                                                     | Missing revenue/conversion opportunity     | 5d     |
| P2-6  | **Insurance integration** — no service exists                                       | No InsuranceService found                                                                                                                   | Missing partnership revenue                | 10d    |
| P2-7  | **DataTable component** not implemented                                             | [lazy/index.tsx](frontend/web-next/src/components/lazy/index.tsx#L203)                                                                      | Admin tables use inline implementations    | 3d     |
| P2-8  | **RichTextEditor component** not implemented                                        | [lazy/index.tsx](frontend/web-next/src/components/lazy/index.tsx#L197)                                                                      | No rich content editing for descriptions   | 2d     |
| P2-9  | **InventoryTable component** not implemented                                        | [lazy/index.tsx](frontend/web-next/src/components/lazy/index.tsx#L209)                                                                      | Dealer inventory uses basic layout         | 2d     |
| P2-10 | **AnalyticsDashboard component** not implemented                                    | [lazy/index.tsx](frontend/web-next/src/components/lazy/index.tsx#L251)                                                                      | Admin/dealer dashboards use inline charts  | 3d     |
| P2-11 | **DealerAnalytics comparison** calculation is TODO                                  | [AnalyticsQueries.cs](backend/DealerAnalyticsService/DealerAnalyticsService.Application/Features/Analytics/Queries/AnalyticsQueries.cs#L60) | Period comparison shows 0                  | 1d     |
| P2-12 | **Video360 async processing** is TODO                                               | [Video360CommandHandlers.cs](backend/Video360Service/Video360Service.Application/Features/Handlers/Video360CommandHandlers.cs#L129)         | 360° videos processed synchronously (slow) | 3d     |
| P2-13 | **AlertService DLQ** retry for failed publishes is TODO                             | [RabbitMqEventPublisher.cs](backend/AlertService/AlertService.Infrastructure/Messaging/RabbitMqEventPublisher.cs#L98)                       | Failed alerts silently lost                | 2d     |
| P2-14 | **Dealer admin JWT claims** extraction is TODO                                      | [DealersController.cs](backend/DealerManagementService/DealerManagementService.Api/Controllers/DealersController.cs#L169)                   | Admin actions not properly attributed      | 1d     |

### 🟢 P3 — Nice to Have (Scale & Polish)

| #    | Gap                                                                            | Location                                                               | Impact                                 | Effort |
| ---- | ------------------------------------------------------------------------------ | ---------------------------------------------------------------------- | -------------------------------------- | ------ |
| P3-1 | **AdminDashboard component** not implemented                                   | [lazy/index.tsx](frontend/web-next/src/components/lazy/index.tsx#L257) | Admin dashboard uses inline layout     | 3d     |
| P3-2 | **Accessibility audit** — no WCAG compliance review done                       | Frontend-wide                                                          | Legal risk, excludes users             | 10d    |
| P3-3 | **Performance budget** — no bundle size limits or Core Web Vitals targets      | `next.config.*`                                                        | SEO and UX degradation over time       | 2d     |
| P3-4 | **API versioning** — no v1/v2 strategy                                         | Gateway/Ocelot config                                                  | Breaking changes affect mobile clients | 5d     |
| P3-5 | **Rate limiting at Gateway level** — unclear if Ocelot rate limiting is active | Gateway config                                                         | Potential abuse                        | 2d     |
| P3-6 | **Database backup automation** — backup.yaml exists but unclear if scheduled   | [k8s/backup.yaml](k8s/backup.yaml)                                     | Data loss risk                         | 2d     |
| P3-7 | **Canary deployments** — only chatbot has canary config                        | [k8s/chatbot-canary.yaml](k8s/chatbot-canary.yaml)                     | Risky deployments for other services   | 5d     |
| P3-8 | **Frontend test coverage** — only 16 unit tests + 23 E2E specs                 | `frontend/web-next/`                                                   | Low confidence in changes              | 15d    |
| P3-9 | **Mobile app tests** — only 1 test file found                                  | `frontend/mobile/okla_app/test/`                                       | Zero coverage                          | 10d    |

---

## 4. Mobile App — Deep Gap Analysis

The Flutter mobile app (`frontend/mobile/okla_app/`) has a solid **architectural scaffold** but is at **~25% completion**:

### What's Built

- ✅ Clean Architecture layers (core, data, domain, presentation)
- ✅ 18 presentation pages (home, search, detail, favorites, messages, profile, etc.)
- ✅ GoRouter navigation with bottom nav shell
- ✅ BLoC pattern for state management (8 blocs)
- ✅ Firebase integration (messaging, analytics, crashlytics)
- ✅ API client with Dio + response handler
- ✅ Theme, colors, constants
- ✅ Vehicle card widget

### What's Missing

| Area                   | Status                          | Details                                                                                             |
| ---------------------- | ------------------------------- | --------------------------------------------------------------------------------------------------- |
| **Data Sources**       | 🔴 Only 2 (auth, vehicle)       | Need: favorites, messaging, notifications, dealer, search, alerts, reviews, checkout, KYC, settings |
| **Repositories**       | 🔴 Only 2 (auth, vehicle)       | Need: 10+ matching data sources                                                                     |
| **Use Cases**          | 🔴 Empty directory              | Zero use cases implemented                                                                          |
| **Local Data Sources** | 🔴 Empty directory              | No offline caching, no local DB                                                                     |
| **OAuth/Social Login** | 🔴 TODO in code                 | Google/Apple sign-in not implemented                                                                |
| **Push Notifications** | 🟡 Firebase added               | FCM token registration + handling missing                                                           |
| **Deep Linking**       | 🔴 Not implemented              | Can't share vehicle links to app                                                                    |
| **Biometric Auth**     | 🟡 `local_auth` added           | Not integrated into auth flow                                                                       |
| **Offline Mode**       | 🔴 Not implemented              | No offline data caching                                                                             |
| **Image Caching**      | 🟡 `cached_network_image` added | Need proper cache strategy                                                                          |
| **Maps Integration**   | 🟡 `google_maps_flutter` added  | Not integrated into pages                                                                           |
| **App Store Setup**    | 🔴 Not done                     | No screenshots, descriptions, ASO                                                                   |

---

## 5. Technical Debt

### 5.1 Code Quality

- **15 placeholder components** in `lazy/index.tsx` — either implement or remove stubs
- **30+ TODO items** across frontend and backend production code
- **Analytics handler** returns fake data — should aggregate from real services
- **Mobile app** has empty `usecases/` and `datasources/local/` directories

### 5.2 Security Debt

- ❗ **Azul payment signature verification** not implemented (P0-2)
- ❗ **5 AuthService commands** don't publish audit events to RabbitMQ
- ⚠️ **CSRF** implementation exists but coverage unclear across all forms
- ⚠️ **Rate limiting** service exists but integration with Gateway unclear

### 5.3 Infrastructure Debt

- **5 services missing K8s manifests**: RecoAgent, TaxComplianceService, SpyneIntegrationService, EventTrackingService, ComplianceService
- **No canary deployments** for most services (only chatbot)
- **Database backup** schedule unclear
- **No staging environment** clearly documented

### 5.4 Testing Debt

- Frontend: **16 unit tests** for 150+ pages (< 10% coverage)
- Frontend: **23 E2E specs** — good coverage of critical flows but gaps remain
- Mobile: **1 test file** — effectively zero coverage
- Backend: **40+ test projects** exist but test density unknown

---

## 6. Action Plan

### Phase 1: Critical Fixes (Weeks 1–2) — P0 Items

**Goal:** Make payment flow secure and functional, fix data accuracy issues.

| Task                                                   | Priority | Effort                      | Dependencies            |
| ------------------------------------------------------ | -------- | --------------------------- | ----------------------- |
| Implement Azul payment signature verification          | P0-2     | 3d                          | Azul API docs           |
| Implement live DOP/USD exchange rate from BCRD API     | P0-1     | 2d                          | BCRD API access         |
| Create StripePaymentForm component (Stripe Elements)   | P0-3     | 3d                          | Stripe account          |
| Create AzulPaymentForm component (Azul WebPay SDK)     | P0-3     | 3d                          | Azul sandbox            |
| Integrate payment SDK fields into checkout page        | P0-6     | 2d                          | P0-3                    |
| Connect admin analytics to real data sources           | P0-5     | 3d                          | None                    |
| Implement push notification service (uncomment + wire) | P0-4     | 5d                          | Firebase/APNS setup     |
| Add missing K8s manifests for 5 services               | P0-7     | 2d                          | None                    |
| Fix dealer billing usage tracking                      | P0-8     | 3d                          | VehiclesSaleService API |
| **Phase 1 Total**                                      |          | **~26d (2 devs × 2 weeks)** |                         |

### Phase 2: Core Features (Weeks 3–6) — P1 Items

**Goal:** Complete user experience, enable full analytics, fix integration gaps.

| Task                                                       | Priority   | Effort                      | Dependencies           |
| ---------------------------------------------------------- | ---------- | --------------------------- | ---------------------- |
| Implement ContactModal component                           | P1-3       | 3d                          | None                   |
| Implement ShareModal with social sharing                   | P1-4       | 2d                          | None                   |
| Implement Lightbox for vehicle photos                      | P1-5       | 2d                          | None                   |
| Implement ComparisonModal                                  | P1-6       | 2d                          | None                   |
| Implement Map & DealerMap components (Google Maps)         | P1-7       | 5d                          | Google Maps API key    |
| Implement VideoPlayer component                            | P1-8       | 2d                          | None                   |
| Add phone reveal & chat analytics tracking                 | P1-1, P1-2 | 2d                          | EventTrackingService   |
| Connect admin promotions page to real API                  | P1-9       | 2d                          | BillingService         |
| Connect admin early-bird page to real API                  | P1-10      | 2d                          | BillingService         |
| Connect admin transactions page to real API                | P1-11      | 2d                          | PaymentService         |
| Implement dealer inventory bulk delete                     | P1-12      | 1d                          | VehiclesSaleService    |
| Implement Stripe webhook dealer notifications              | P1-13      | 2d                          | NotificationService    |
| Publish AuthService domain events to RabbitMQ (5 handlers) | P1-14      | 3d                          | CarDealer.Contracts    |
| Implement AdminService email notifications                 | P1-15      | 1d                          | NotificationService    |
| Implement WhatsApp dealer routing                          | P1-16      | 2d                          | ChatbotService         |
| Implement dealer statistics aggregation                    | P1-17      | 2d                          | DealerAnalyticsService |
| Add contact page error toast                               | P1-18      | 0.5d                        | None                   |
| **Phase 2 Total**                                          |            | **~35d (2 devs × 4 weeks)** |                        |

### Phase 3: Growth Features (Weeks 7–10) — P2 Items + Mobile

**Goal:** Add competitive features, improve mobile app, strengthen infrastructure.

| Task                                                     | Priority | Effort                      | Dependencies |
| -------------------------------------------------------- | -------- | --------------------------- | ------------ |
| Create PWA manifest.ts with icons & theme                | P2-1     | 1d                          | None         |
| Implement JSON-LD structured data for vehicles & dealers | P2-3     | 3d                          | None         |
| Build financing calculator page & component              | P2-5     | 5d                          | None         |
| Implement DataTable reusable component                   | P2-7     | 3d                          | None         |
| Implement RichTextEditor component                       | P2-8     | 2d                          | None         |
| Implement InventoryTable component                       | P2-9     | 2d                          | None         |
| Implement AnalyticsDashboard component                   | P2-10    | 3d                          | None         |
| Fix DealerAnalytics period comparison                    | P2-11    | 1d                          | None         |
| Implement Video360 async processing                      | P2-12    | 3d                          | RabbitMQ     |
| Fix AlertService DLQ retry                               | P2-13    | 2d                          | None         |
| Fix Dealer admin JWT claims                              | P2-14    | 1d                          | None         |
| **Mobile: Implement 8+ data sources**                    | Mobile   | 10d                         | Backend APIs |
| **Mobile: Implement 8+ repositories**                    | Mobile   | 5d                          | Data sources |
| **Mobile: Add use cases layer**                          | Mobile   | 5d                          | Repositories |
| **Mobile: Implement OAuth (Google/Apple)**               | Mobile   | 3d                          | OAuth setup  |
| **Mobile: Deep linking**                                 | Mobile   | 2d                          | None         |
| **Mobile: Push notification handling**                   | Mobile   | 3d                          | P0-4         |
| **Phase 3 Total**                                        |          | **~54d (3 devs × 4 weeks)** |              |

### Phase 4: Scale & Optimize (Weeks 11–14) — P3 + Polish

**Goal:** Production hardening, compliance, scale testing, app store launch.

| Task                                              | Priority | Effort                        | Dependencies           |
| ------------------------------------------------- | -------- | ----------------------------- | ---------------------- |
| Multi-language support (next-intl)                | P2-2     | 10d                           | Translation files      |
| Vehicle history report service                    | P2-4     | 15d                           | Data partnerships      |
| Insurance integration service                     | P2-6     | 10d                           | Insurance API partners |
| Accessibility audit (WCAG 2.1 AA)                 | P3-2     | 10d                           | None                   |
| Performance budget & Core Web Vitals optimization | P3-3     | 2d                            | None                   |
| API versioning strategy                           | P3-4     | 5d                            | None                   |
| Gateway rate limiting verification                | P3-5     | 2d                            | None                   |
| Database backup automation verification           | P3-6     | 2d                            | None                   |
| Canary deployment for critical services           | P3-7     | 5d                            | None                   |
| Frontend test coverage to 50%+                    | P3-8     | 15d                           | None                   |
| Mobile test coverage                              | P3-9     | 10d                           | None                   |
| **Mobile: Offline mode with local DB**            | Mobile   | 5d                            | SQLite/Hive            |
| **Mobile: Biometric auth integration**            | Mobile   | 2d                            | None                   |
| **Mobile: App Store & Play Store submission**     | Mobile   | 5d                            | Screenshots, ASO       |
| **Phase 4 Total**                                 |          | **~98d (3-4 devs × 4 weeks)** |                        |

---

## 7. Dependency Map

```
P0-3 (Payment Forms) ──► P0-6 (Checkout Integration)
P0-4 (Push Notifications) ──► Mobile Push Handling
P0-7 (K8s Manifests) ──► All missing services deploy
P1-1 (Phone Analytics) ──► EventTrackingService must be in K8s
P1-13 (Stripe Webhooks) ──► NotificationService must support dealer notifications
P1-14 (Auth Events) ──► CarDealer.Contracts event types must exist
P2-1 (PWA Manifest) ──► Mobile web install capability
P2-3 (JSON-LD) ──► SEO improvements visible in search
Mobile Data Sources ──► Backend APIs must be stable
Mobile OAuth ──► AuthService OAuth endpoints verified
Mobile Push ──► P0-4 backend push notifications working
```

---

## 8. Services Inventory Summary

### Active Backend Services (47)

| Category           | Services                                                                                                                                                                                                                                         |
| ------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| **Core**           | AuthService, UserService, RoleService, VehiclesSaleService, Gateway                                                                                                                                                                              |
| **Billing**        | BillingService, PaymentService                                                                                                                                                                                                                   |
| **Communication**  | NotificationService, ContactService, ChatbotService                                                                                                                                                                                              |
| **Dealer**         | DealerManagementService, DealerAnalyticsService, StaffService                                                                                                                                                                                    |
| **Media**          | MediaService, AIProcessingService, BackgroundRemovalService, SpyneIntegrationService, Vehicle360ProcessingService, Video360Service                                                                                                               |
| **Intelligence**   | VehicleIntelligenceService, RecommendationService, RecoAgent, SearchAgent, LeadScoringService                                                                                                                                                    |
| **Operations**     | AdminService, AuditService, ErrorService, ReportsService, MaintenanceService                                                                                                                                                                     |
| **Marketplace**    | AdvertisingService, ComparisonService, AlertService, ReviewService, CRMService, InventoryManagementService                                                                                                                                       |
| **Compliance**     | KYCService, ComplianceService, DataProtectionService, TaxComplianceService                                                                                                                                                                       |
| **Infrastructure** | CacheService, ConfigurationService, IdempotencyService, MessageBusService, RateLimitingService, SchedulerService, ServiceDiscovery, EventTrackingService, IntegrationService, MarketingService, SupportAgent, ApiDocsService, AppointmentService |

### Services Missing from K8s (5)

1. **RecoAgent** — in Gateway routes but not in K8s
2. **TaxComplianceService** — has Dockerfile but no K8s manifest
3. **SpyneIntegrationService** — has Dockerfile but no K8s manifest
4. **EventTrackingService** — has Dockerfile but no K8s manifest
5. **ComplianceService** — has Dockerfile but no K8s manifest

---

## 9. Recommended Team Allocation

| Phase   | Duration | Team Size                                | Focus Areas                              |
| ------- | -------- | ---------------------------------------- | ---------------------------------------- |
| Phase 1 | 2 weeks  | 2 backend + 1 frontend                   | Payments, security, data accuracy        |
| Phase 2 | 4 weeks  | 2 frontend + 1 backend                   | UI components, admin pages, integrations |
| Phase 3 | 4 weeks  | 1 frontend + 1 backend + 1 mobile        | Growth features, mobile app, infra       |
| Phase 4 | 4 weeks  | 1 frontend + 1 backend + 1 mobile + 1 QA | Polish, testing, compliance, app stores  |

---

## 10. Risk Register

| Risk                         | Probability                    | Impact   | Mitigation                                      |
| ---------------------------- | ------------------------------ | -------- | ----------------------------------------------- |
| Azul payment spoofing (P0-2) | High                           | Critical | Implement signature verification immediately    |
| Wrong pricing (P0-1)         | High                           | High     | Integrate BCRD API with fallback to cached rate |
| Push notification failures   | Medium                         | High     | Implement with retry/DLQ pattern                |
| Mobile app delayed           | Medium                         | Medium   | Prioritize core flows (browse, search, detail)  |
| K8s services not deploying   | High if deploying new services | High     | Add manifests in Phase 1                        |
| Test coverage gaps           | High                           | Medium   | Add tests with each feature, not retroactively  |

---

## 11. Quick Wins (< 1 day each)

These can be done immediately with minimal risk:

1. ✅ Add error toast to contact page (P1-18) — 0.5d
2. ✅ Implement dealer inventory bulk delete button (P1-12) — 1d
3. ✅ Fix DealerAnalytics period comparison returning 0 (P2-11) — 1d
4. ✅ Fix Dealer admin JWT claims extraction (P2-14) — 1d
5. ✅ Create PWA manifest.ts (P2-1) — 1d
6. ✅ Add AdminService email notifications for employee events (P1-15) — 1d

---

_Document generated from automated deep audit of the OKLA platform codebase._
_Next review recommended: After Phase 1 completion._
