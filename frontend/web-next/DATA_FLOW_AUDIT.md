# 🔍 Deep Data Flow Audit — OKLA Next.js Frontend

**Audited:** 2026-03-03  
**Scope:** `/frontend/web-next/src/`  
**Auditor:** Copilot (Claude Opus 4.6)

---

## Executive Summary

| Data Flow                 | Status                                 | Severity                                                                                               |
| ------------------------- | -------------------------------------- | ------------------------------------------------------------------------------------------------------ |
| 1. OKLA Score             | ⚠️ Partially Connected                 | **HIGH** — VIN History (D1, 25% of score) always scores default because no VinAudit integration exists |
| 2. Advertising            | ⚠️ Dual-path (Backend + Demo Fallback) | **MEDIUM** — All sponsored slots fall back to hardcoded demo data when backend is unavailable          |
| 3. Analytics/Tracking     | 🔴 In-memory only                      | **CRITICAL** — All tracked events live in a Node.js `Map`, lost on every server restart/redeploy       |
| 4. Auth Token Flow        | ✅ Solid                               | **LOW** — Well-implemented HttpOnly cookie flow with proper refresh logic                              |
| 5. Broken Links/Dead Ends | ⚠️ Multiple found                      | **MEDIUM** — Several demo data fallbacks silently replace real data without UI indication              |

---

## 1. OKLA Score Data Flow

### 1.1 End-to-End Trace

```
User enters VIN
  → useCalculateScore() hook (src/hooks/use-okla-score.ts:89)
    → POST /api/score/calculate (src/app/api/score/calculate/route.ts)
      → Step 1: GET /api/score/vin-decode (→ NHTSA vPIC free API ✅)
      → Step 2: Parallel:
          GET /api/score/recalls (→ NHTSA Recalls API ✅)
          GET /api/score/safety  (→ NHTSA Safety API ✅)
      → Step 3: calculateOklaScore() in okla-score-engine.ts
    → OklaScoreReport returned
      → ScoreReport component renders gauge, dimensions, alerts
      → ScoreBadge rendered on VehicleCard
```

### 1.2 Issues Found

#### 🔴 CRITICAL — D1 VIN History (25% of score) ALWAYS uses default score

- **File:** [okla-score-engine.ts](src/lib/okla-score-engine.ts#L55-L64)
- **Problem:** `calculateD1(history?)` receives `undefined` for `history` because `/api/score/calculate` **never fetches VIN history data**. The `ScoreInput.history` field is always `undefined`.
- **Impact:** D1 (VIN History) is the **heaviest dimension at 25%** and always returns `rawScore=100` with factor "No history available". This means:
  - Salvage titles are never detected
  - Flood/frame damage is never flagged
  - Odometer fraud is never caught
  - Stolen/cloned VINs pass through
- **Root Cause:** There is no VinAudit API integration. The engine code references VinAudit but no API call is made in the calculate route.
- **Line:** [calculate/route.ts](src/app/api/score/calculate/route.ts#L78) — `history` is never assigned.

#### 🟡 WARNING — Hardcoded exchange rate

- **File:** [calculate/route.ts](src/app/api/score/calculate/route.ts#L80)
- **Problem:** `exchangeRate: 58.5` is hardcoded. The comment says `// TODO: fetch live from BCRD or ExchangeRate-API`
- **Impact:** D4 (Price vs Market, 17%) calculations use a stale exchange rate. DOP/USD fluctuations could make fair price analyses incorrect.

#### 🟡 WARNING — No market price data

- **File:** [okla-score-engine.ts](src/lib/okla-score-engine.ts#L356-L362)
- **Problem:** `ScoreInput.marketPriceDOP` and `marketPriceUSD` are never populated by the calculate route. D4 returns neutral `rawScore=85` with "No market data".
- **Impact:** D4 (17% weight) is always neutral — buyers get no price comparison.

#### 🟢 INFO — Type alignment is correct

- `OklaScoreReport` → `ScoreReport` component: types match perfectly.
- `ScoreLookupRequest` used by `useCalculateScore()` maps cleanly to the POST body expected by the route.
- `ScoreBadge` in `VehicleCard` uses `vehicle.oklaScore` (number) which is compatible with `ScoreBadge({ score: number })`.

### 1.3 Data That Actually Works

| Dimension            | Weight | Data Source                  | Status                          |
| -------------------- | ------ | ---------------------------- | ------------------------------- |
| D1 VIN History       | 25%    | VinAudit (not integrated)    | ❌ Always default 100pts        |
| D2 Mechanical        | 20%    | NHTSA vPIC VIN decode        | ✅ Real data                    |
| D3 Mileage           | 18%    | Seller declaration           | ⚠️ No verification (no history) |
| D4 Price vs Market   | 17%    | MarketCheck (not integrated) | ❌ Always neutral               |
| D5 Safety & Recalls  | 10%    | NHTSA APIs                   | ✅ Real data                    |
| D6 Depreciation      | 6%     | From VIN decode year         | ✅ Real data                    |
| D7 Seller Reputation | 4%     | Not passed from backend      | ❌ Always default               |

**Conclusion:** Only **34% of score weight** uses real external data (D2+D5+D6). The remaining **66%** is default/neutral values.

---

## 2. Advertising Data Flow

### 2.1 End-to-End Trace

```
Campaign Lifecycle:
  Dealer creates campaign
    → useCreateCampaign() hook (src/hooks/use-advertising.ts:113)
      → createCampaign() service (src/services/advertising.ts:33)
        → apiClient.post('/api/advertising/campaigns')
          → BFF route (src/app/api/advertising/campaigns/route.ts)
            → Gateway → AdvertisingService (backend, exists in ocelot.prod.json ✅)

Ad Serving:
  Homepage loads
    → GET /api/advertising/sponsored?slot=FeaturedSpot
      → BFF route (src/app/api/advertising/sponsored/route.ts)
        → Try: Gateway → AdvertisingService /api/advertising/rotation/{slot}
        → Fallback: generateSponsoredVehiclesForSlot() → HARDCODED DEMO DATA

Impression/Click Tracking:
  Ad displayed → useRecordImpression()
    → recordImpression() service → apiClient.post('/api/advertising/tracking/impression')
      → BFF route (src/app/api/advertising/tracking/route.ts)
        → Gateway → AdvertisingService

  Ad clicked → useRecordClick()
    → recordClick() service → apiClient.post('/api/advertising/tracking/click')
      → Same BFF route → Gateway → AdvertisingService
```

### 2.2 Issues Found

#### 🔴 CRITICAL — Sponsored ads always serve demo data when backend is down

- **File:** [sponsored/route.ts](src/app/api/advertising/sponsored/route.ts#L48-L64)
- **Problem:** When the AdvertisingService backend is unavailable, the route falls back to `generateSponsoredVehiclesForSlot()` which returns **hardcoded mock vehicles** (Toyota RAV4, Honda CR-V, etc.) from [ad-engine.ts](src/lib/ad-engine.ts#L541-L713).
- **Impact:** Users see fake sponsored listings that link to non-existent vehicle pages. Click tracking URLs (`/api/ads/click?id=sp-001`) point to a route that **does not exist**.
- **Silent failure:** The response includes `source: 'demo'` in `meta`, but components don't check this flag.

#### 🔴 CRITICAL — Demo sponsored vehicles have broken click tracking URLs

- **File:** [ad-engine.ts](src/lib/ad-engine.ts#L580)
- **Problem:** Demo `SponsoredVehicle` objects set `clickTrackingUrl: '/api/ads/click?id=sp-001'`. This API route `/api/ads/click` does **not exist** anywhere in the app.
- **Impact:** Click tracking for demo ads silently 404s.

#### 🟡 WARNING — Two separate tracking paths that don't converge

- **Path A:** `services/advertising.ts` → `apiClient.post('/api/advertising/tracking/impression')` (goes through axios apiClient)
- **Path B:** `app/api/advertising/tracking/route.ts` → `fetch(${API_URL}/api/advertising/tracking/click)` (BFF direct fetch)
- The `services/advertising.ts` functions use the **axios apiClient** (with auth cookies, CSRF), but the BFF route forwards to the backend **without cookies** — only the `Authorization` header if present.
- **Impact:** If an unauthenticated user triggers an impression via the service layer, it works; but if the component directly calls the BFF route with different request shape, data could be lost.

#### 🟡 WARNING — `ad-engine.ts` GSP auction algorithm runs only client-side

- **File:** [ad-engine.ts](src/lib/ad-engine.ts#L237-L290)
- **Problem:** `runGspAuction()`, `calculatePurchaseIntentScore()`, `calculatePacingRate()` etc. are sophisticated algorithms but are **never called** by any BFF route or component. The `generateSponsoredVehiclesForSlot()` demo function skips the auction entirely.
- **Impact:** All the ad-engine science (quality scores, second-price auctions, frequency capping, IVT detection) is dead code in the frontend.

#### 🟢 INFO — Backend AdvertisingService routes exist in Ocelot

The Gateway has routes for `/api/advertising/*` pointing to `advertisingservice:8080` — the backend service exists. The frontend correctly proxies through BFF routes.

### 2.3 Live Dashboard — Demo Data Fallback

- **File:** [live-dashboard/route.ts](src/app/api/advertising/live-dashboard/route.ts#L42)
- When backend is unavailable, `generateDemoLiveData()` creates fake campaign metrics with `Math.random()`. Components consuming this will display random-looking metrics that refresh with different values each time.

---

## 3. Analytics/Tracking Data Flow

### 3.1 End-to-End Trace

```
Page loads
  → TrackingProvider (src/components/providers/tracking-provider.tsx)
    → getDeviceInfo(), getSessionId(), getAnonymousId() from device-fingerprint.ts
    → Enqueues 'session_start' event
    → On route change: enqueues 'page_view' event
    → Every 10s or on page hide: flushEvents()
      → navigator.sendBeacon('/api/analytics/track') or fetch() fallback

/api/analytics/track (src/app/api/analytics/track/route.ts):
  → Stores events in IN-MEMORY Map (eventStore, sessionStore)
  → Fire-and-forget forwardToBackend() to ${API_URL}/api/analytics/events
    → Backend endpoint likely DOES NOT EXIST (no gateway route found)

/api/analytics/leads (src/app/api/analytics/leads/route.ts):
  → Reads from in-memory eventStore via /api/analytics/track?type=all-sessions
  → Scores leads using rule-based model
  → Falls back to generateDemoLeads() (575+ lines of mock data)
```

### 3.2 Issues Found

#### 🔴 CRITICAL — All analytics data is stored in-memory and lost on restart

- **File:** [track/route.ts](src/app/api/analytics/track/route.ts#L17-L18)
- **Problem:** `eventStore` and `sessionStore` are plain `Map<>` objects in the Node.js process. Every deployment, restart, or scale event **wipes all data**.
- **Impact:**
  - Lead scoring has no persistence — the leads dashboard shows demo data most of the time
  - Session tracking is meaningless across deployments
  - The `MAX_VISITORS = 10_000` cap means data is also lost under high traffic

#### 🔴 CRITICAL — Backend forwarding silently fails with no analytics endpoint

- **File:** [track/route.ts](src/app/api/analytics/track/route.ts#L274-L283)
- **Problem:** `forwardToBackend()` sends events to `${API_URL}/api/analytics/events`. There is **no Ocelot route** for `/api/analytics/*` in the Gateway configuration, and no AnalyticsService exists in the backend.
- **Evidence:** grep for `analytics` in Gateway ocelot configs returned no matches.
- **Impact:** The `forwardToBackend()` call always fails silently (caught with `/* Backend may not have analytics endpoint yet — that's OK */`). Data never persists.

#### 🔴 CRITICAL — Lead scoring falls back to demo data in 3 of 3 code paths

- **File:** [leads/route.ts](src/app/api/analytics/leads/route.ts#L26-L74)
- **Problem:** The leads endpoint has THREE fallback paths to `generateDemoLeads()`:
  1. If `trackRes` fails → demo leads
  2. If real leads array is empty → demo leads
  3. If any error → demo leads
- **Impact:** Admin dashboard always shows fake leads (María Rodríguez, Carlos Pérez, etc.) because the in-memory store is typically empty.

#### 🟡 WARNING — Device fingerprint generates non-persistent IDs

- **File:** [device-fingerprint.ts](src/lib/device-fingerprint.ts)
- `getSessionId()` and `getAnonymousId()` likely use `sessionStorage` or generate UUIDs — these don't persist across browser sessions, making cross-session tracking impossible.

#### 🟡 WARNING — Retargeting pixels fire without configuration

- **File:** [retargeting-pixels.ts](src/lib/retargeting-pixels.ts#L22-L24)
- `FB_PIXEL_ID`, `GA_MEASUREMENT_ID`, `GOOGLE_ADS_ID`, `TIKTOK_PIXEL_ID` all default to `''` (empty string).
- When empty, the initialization code early-returns, but the `TrackingProvider` still calls `pixelPageView()`, `pixelVehicleView()`, etc. — these are **no-ops** when pixels aren't initialized, but the code runs on every navigation.

---

## 4. Auth Token Flow

### 4.1 End-to-End Trace

```
Login:
  LoginForm → serverLogin() Server Action (src/actions/auth.ts:233)
    → internalFetch('/api/auth/login') → Gateway → AuthService
    → AuthService returns: { accessToken, refreshToken }
    → setAuthCookiesFromServerAction() sets HttpOnly cookies:
        okla_access_token (24h, httpOnly, secure in prod)
        okla_refresh_token (30d, httpOnly, secure in prod)
    → Also fetches /api/auth/me server-side for user profile

Subsequent API calls:
  apiClient (axios, withCredentials:true)
    → Browser automatically sends HttpOnly cookies
    → CSRF token added via X-CSRF-Token header for mutations
    → 401 response → interceptor tries POST /api/auth/refresh-token
      → On refresh success: retry original request
      → On refresh failure: clear tokens, dispatch 'auth:logout' event

Route protection:
  middleware.ts reads okla_access_token from cookies
    → Decodes JWT payload (no signature verification — done by backend)
    → Checks expiry, roles
    → Redirects to /login if unauthenticated
    → Redirects to /403 if unauthorized
```

### 4.2 Issues Found

#### 🟢 SOLID — HttpOnly cookie implementation is well done

- Tokens are never stored in localStorage (legacy code exists but only for cleanup).
- `sameSite: 'lax'`, `secure: true` in production.
- CSRF double-submit cookie pattern implemented.
- Server Actions hide auth endpoints from browser DevTools.

#### 🟡 WARNING — Middleware does NOT verify JWT signature

- **File:** [middleware.ts](src/middleware.ts#L175-L207)
- **Problem:** `decodeToken()` only does base64 decoding of the JWT payload. It does NOT verify the signature (HMAC/RSA). The comment explains this is intentional (Edge Runtime limitation), but it means:
  - A crafted JWT with `role: 'admin'` would pass middleware routing checks
  - Backend still verifies signatures, so API calls would fail
- **Impact:** LOW — this is a UX-only check. Actual security is enforced by the Gateway.

#### 🟡 WARNING — Legacy token cleanup code scattered throughout

- **File:** [api-client.ts](src/lib/api-client.ts#L39-L46)
- The request interceptor still checks `localStorage.getItem(ACCESS_TOKEN_KEY)` and sends it as `Authorization: Bearer`. This is a migration artifact that could cause issues if old tokens exist.

#### 🟡 WARNING — BFF routes forward `Authorization` header inconsistently

- [campaigns/route.ts](src/app/api/advertising/campaigns/route.ts#L24) checks `request.headers.get('authorization')` — but with HttpOnly cookies, the auth token is in cookies, not the Authorization header. The BFF should read the cookie and forward it.
- This affects: `campaigns/route.ts`, `reports/route.ts`, `live-dashboard/route.ts`.

---

## 5. Broken Links & Dead Data Connections

### 5.1 API Routes Returning Demo/Mock Data

| Route                                | When Demo                      | Indicator                    | Component Checks? |
| ------------------------------------ | ------------------------------ | ---------------------------- | ----------------- |
| `/api/advertising/sponsored`         | Backend unavailable            | `meta.source: 'demo'`        | ❌ No             |
| `/api/advertising/live-dashboard`    | Backend unavailable            | `source: 'demo'` in response | ❌ No             |
| `/api/advertising/targeted`          | Backend unavailable            | `source: 'demo'` in response | ❌ No             |
| `/api/advertising/advertiser-report` | Backend unavailable            | `source: 'demo'` in response | ❌ No             |
| `/api/analytics/leads`               | Always (empty in-memory store) | `source: 'demo'` in response | ❌ No             |

**Impact:** Components display demo data indistinguishable from real data. Admins and dealers see fake metrics.

### 5.2 Non-Existent Endpoints Called

| Caller                                 | Endpoint                                               | Exists?                                       |
| -------------------------------------- | ------------------------------------------------------ | --------------------------------------------- |
| `forwardToBackend()` in track/route.ts | `${API_URL}/api/analytics/events`                      | ❌ No gateway route                           |
| Demo SponsoredVehicle objects          | `/api/ads/click?id=sp-001`                             | ❌ No route handler                           |
| `services/advertising.ts` L97-101      | `/api/advertising/tracking/impression` (via apiClient) | ⚠️ Uses apiClient but BFF expects direct call |

### 5.3 Data Transformations That Could Fail Silently

1. **[track/route.ts L17](src/app/api/analytics/track/route.ts#L43):** `SessionData.vehicleViews` is a `Set<string>`, but `serializeSession()` converts it to an array. The leads route reads it back and treats it as an array — if serialization changes, `vehicleViews.length` would break.

2. **[auth.ts](src/services/auth.ts):** `BackendActiveSession` supports both PascalCase and camelCase fields (dual-mapping). If the backend changes casing, the normalization code in `getActiveSessions()` must be updated in two places.

3. **[advertising/campaigns route](src/app/api/advertising/campaigns/route.ts#L15-L21):** The BFF route forwards `request.headers.get('authorization')` to the backend, but with HttpOnly cookies this header is **empty**. Campaign creation in production would fail with 401 if the backend requires auth.

### 5.4 Missing Error Boundaries

| Component/Hook                               | Error Handling                | Risk                              |
| -------------------------------------------- | ----------------------------- | --------------------------------- |
| `useCalculateScore()`                        | Throws Error — no fallback UI | Score page would show React error |
| `useHomepageRotation()`                      | Returns `null` — graceful     | ✅ Good                           |
| `TrackingProvider`                           | No-op on error — silent       | ✅ Acceptable for tracking        |
| `useRecordImpression()` / `useRecordClick()` | No onError handler            | Silent data loss                  |
| `flushEvents()` in TrackingProvider          | `.catch(() => {})`            | Silent — acceptable               |

---

## 6. Recommendations

### Priority 1 — CRITICAL (Data Loss / Misleading Data)

| #   | Issue                                  | Fix                                                                                                              | Files                                  |
| --- | -------------------------------------- | ---------------------------------------------------------------------------------------------------------------- | -------------------------------------- |
| 1   | Analytics events lost on restart       | Implement Redis-backed event store or forward to a real backend analytics service (PostHog, Mixpanel, or custom) | `src/app/api/analytics/track/route.ts` |
| 2   | VIN History (D1, 25%) always defaults  | Integrate VinAudit API in `/api/score/calculate` or create a `/api/score/history` route                          | `src/app/api/score/calculate/route.ts` |
| 3   | Demo data served without UI indication | Add a `[DEMO]` badge or banner when `source === 'demo'` in API responses                                         | All consuming components               |

### Priority 2 — HIGH (Incorrect Calculations / Silent Failures)

| #   | Issue                                                              | Fix                                                                                  | Files                                                                                       |
| --- | ------------------------------------------------------------------ | ------------------------------------------------------------------------------------ | ------------------------------------------------------------------------------------------- |
| 4   | Market price data (D4, 17%) always neutral                         | Integrate pricing API or pass market data from VehiclesSaleService                   | `src/app/api/score/calculate/route.ts`                                                      |
| 5   | BFF routes forward empty Authorization header                      | Read `okla_access_token` from cookies and forward as `Authorization: Bearer <token>` | `src/app/api/advertising/campaigns/route.ts`, `reports/route.ts`, `live-dashboard/route.ts` |
| 6   | Demo click tracking URLs point to `/api/ads/click` (doesn't exist) | Either create the route or use the existing `/api/advertising/tracking` route        | `src/lib/ad-engine.ts:580`                                                                  |
| 7   | Exchange rate hardcoded at 58.5                                    | Fetch from BCRD API or cache externally                                              | `src/app/api/score/calculate/route.ts:80`                                                   |

### Priority 3 — MEDIUM (Code Quality / Dead Code)

| #   | Issue                                                     | Fix                                                              | Files                           |
| --- | --------------------------------------------------------- | ---------------------------------------------------------------- | ------------------------------- |
| 8   | GSP auction engine is dead code in frontend               | Move to backend AdvertisingService or wire up in sponsored route | `src/lib/ad-engine.ts`          |
| 9   | Legacy localStorage token code                            | Remove after migration period                                    | `src/lib/api-client.ts:39-46`   |
| 10  | No analytics backend service                              | Create an AnalyticsService or integrate PostHog/Mixpanel         | Gateway ocelot config           |
| 11  | Retargeting pixels fire empty with no env vars configured | Add validation or remove calls when IDs are empty                | `src/lib/retargeting-pixels.ts` |

---

## 7. Data Flow Diagrams Summary

### What's Connected (Real Data) ✅

```
VIN Input → NHTSA vPIC (decode) → Score Engine D2, D5, D6 → ScoreReport UI
Auth Form → Server Action → Gateway → AuthService → HttpOnly Cookies → All API calls
Campaign CRUD → BFF → Gateway → AdvertisingService
```

### What's Disconnected (Dead Ends) ❌

```
TrackingProvider → /api/analytics/track → IN-MEMORY MAP → ☠️ (lost on restart)
                                        → forwardToBackend → ☠️ (no analytics endpoint)
Ad Engine (GSP, PIS, IVT) → ☠️ (never called)
VIN History (VinAudit) → ☠️ (not integrated)
Market Price (MarketCheck) → ☠️ (not integrated)
Lead Scoring → reads empty in-memory store → falls back to demo data
```

### What's Half-Working (Demo Fallback) ⚠️

```
Sponsored Ads → try backend → fail → DEMO hardcoded vehicles
Live Dashboard → try backend → fail → DEMO random metrics
Lead Predictions → try in-memory → empty → DEMO fake leads
Advertiser Reports → try backend → fail → DEMO fake report
```
