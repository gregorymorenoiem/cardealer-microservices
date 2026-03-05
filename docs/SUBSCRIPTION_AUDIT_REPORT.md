# 📊 Subscription Plan Enforcement Audit Report

**Date:** 2026-03-05  
**Auditor:** GitHub Copilot (Claude)  
**Scope:** All subscription tiers for Dealers and Sellers in OKLA  

---

## 1. Plan Overview

### Dealer Plans
| Feature | LIBRE ($0) | VISIBLE ($29) | PRO ($89) | ELITE ($199) |
|---------|-----------|---------------|-----------|--------------|
| maxListings | 3 | 10 | 25 | Unlimited |
| maxImages | 10 | 20 | 30 | 40 |
| maxVideos | 0 | 0 | 1 | 3 |
| featuredListings | 0 | 1 | 3 | 10 |
| view360Available | ❌ | ❌ | ✅ | ✅ |
| bulkUpload | ❌ | ❌ | ✅ | ✅ |
| analyticsAccess | basic | standard | advanced | premium |
| leadManagement | ❌ | ✅ | ✅ | ✅ |
| whatsappIntegration | ❌ | ❌ | ✅ | ✅ |
| customBranding | ❌ | ❌ | ❌ | ✅ |
| searchPriority | 0 | 1 | 2 | 3 |
| listingDuration | 30 | 45 | 60 | 90 |
| supportLevel | community | email | priority | dedicated |
| verifiedBadge | ❌ | ✅ | ✅ | ✅ |
| oklaCoins | 0 | 50 | 150 | 500 |

### Seller Plans
| Feature | GRATIS | PREMIUM | PRO |
|---------|--------|---------|-----|
| maxListings | 1 | 3 | 5 |
| maxImages | 5 | 15 | 25 |
| maxVideos | 0 | 1 | 2 |
| view360Available | ❌ | ❌ | ✅ |
| bulkUpload | ❌ | ❌ | ❌ |

---

## 2. Findings Summary

| Severity | Count | Description |
|----------|-------|-------------|
| 🔴 P0 Critical | 6 | Features with no enforcement (bypassable) |
| 🟠 P1 High | 6 | Backend enforcement missing |
| 🟡 P2 Medium | 12 | Features defined but not built |
| 🟢 P3 Low | 8 | Properly enforced |

---

## 3. P0 Critical Findings (Fixed in This Session)

### 3.1 ✅ FIXED — maxImages Not Per-Plan (Frontend)
- **Before:** Hardcoded `{individual: {min:3, max:20}, dealer: {min:5, max:50}}` in photo upload page
- **After:** Reads `maxImages` from `DEALER_PLAN_LIMITS[plan]` / `SELLER_PLAN_LIMITS[plan]`
- **File:** `src/app/(main)/publicar/fotos/page.tsx`
- **⚠️ Backend still not enforced** — MediaService upload endpoint doesn't validate per-plan limits

### 3.2 ✅ FIXED — view360Available Hardcoded to `true`
- **Before:** `const is360Available = true;` in view360-step.tsx
- **After:** Checks `view360Available` from user's plan config
- **File:** `src/components/vehicles/smart-publish/view360-step.tsx`

### 3.3 ✅ FIXED — bulkUpload (CSV Import) Not Gated
- **Before:** CSV import wizard accessible to all users regardless of plan
- **After:** Shows upgrade CTA when `bulkUpload` is `false` in plan config
- **File:** `src/components/vehicles/smart-publish/csv-import-wizard.tsx`

### 3.4 ⚠️ NOT FIXED — maxListings Not Checked at Vehicle Creation
- **Issue:** Backend `VehiclesController` doesn't check current listing count vs plan limit
- **Impact:** Users can bypass frontend limits via direct API calls
- **Fix Required:** Backend middleware/filter in VehiclesSaleService to validate `maxListings`

### 3.5 ⚠️ NOT FIXED — featuredListings Enforcement Removed
- **Issue:** `SubscriptionsController` is in `_REMOVED_CONTROLLERS/` directory
- **Impact:** No active API endpoint to manage subscription features
- **Fix Required:** Restore and update SubscriptionsController, or create enforcement middleware

### 3.6 ⚠️ NOT FIXED — searchPriority Not Enforced in Backend
- **Issue:** Vehicle search doesn't factor in dealer plan's `searchPriority` value
- **Impact:** All listings treated equally regardless of plan tier
- **Fix Required:** Backend search query in VehiclesSaleService needs to boost by plan priority

---

## 4. P1 High Findings (Backend Required)

| # | Feature | Status | Notes |
|---|---------|--------|-------|
| 4.1 | maxVideos | Frontend only | MediaService doesn't validate per-plan video count |
| 4.2 | analyticsAccess | Frontend only | AnalyticsService doesn't filter by plan level |
| 4.3 | leadManagement | Frontend only | No backend gating on lead endpoints |
| 4.4 | whatsappIntegration | Frontend only | WhatsApp API calls not plan-gated |
| 4.5 | customBranding | Frontend only | Branding settings accessible to all |
| 4.6 | listingDuration | Not enforced | No auto-expiry based on plan duration |

---

## 5. P2 Medium Findings (Features Not Built)

| # | Feature | Status |
|---|---------|--------|
| 5.1 | oklaCoins | Defined in config, no economy system built |
| 5.2 | chatAgent | Plan flag exists, no chatbot service |
| 5.3 | pricingAgent | Plan flag exists, no pricing AI service |
| 5.4 | autoScheduling | Plan flag exists, not implemented |
| 5.5 | premiumPlacement | Plan flag exists, not implemented |
| 5.6 | socialMediaSharing | Plan flag exists, not implemented |
| 5.7 | exportTools | Plan flag exists, not implemented |
| 5.8 | apiAccess | Plan flag exists, no public API |
| 5.9 | multiUser | Plan flag exists, not implemented |
| 5.10 | inventoryAlerts | Plan flag exists, not implemented |
| 5.11 | comparativeMarket | Plan flag exists, not implemented |
| 5.12 | dealerFinancing | Plan flag exists, not implemented |

---

## 6. P3 Low — Properly Enforced

| # | Feature | Enforcement |
|---|---------|-------------|
| 6.1 | verifiedBadge | Badge UI gated by plan in dealer profile |
| 6.2 | supportLevel | Correctly shown in plan comparison UI |
| 6.3 | Plan pricing display | Accurate in subscription page |
| 6.4 | Plan feature comparison | Table renders correctly from config |
| 6.5 | Photo upload preview | Shows correct limit message |
| 6.6 | Video upload step | Plan-gated with maxVideos (new) |
| 6.7 | 360° step | Plan-gated with view360Available (fixed) |
| 6.8 | CSV import | Plan-gated with bulkUpload (fixed) |

---

## 7. Architecture Concern: Frontend-Only Enforcement

**Critical Issue:** Most plan restrictions are enforced only in the frontend via:
- `useDealerFeatures()` hook
- `<FeatureGate>` component
- Plan config constants

**Risk:** All frontend-only restrictions can be bypassed by:
1. Direct API calls (curl, Postman)
2. Browser DevTools manipulation
3. Modified frontend builds

**Recommendation:** Implement backend middleware that:
1. Reads user's plan from the subscription database
2. Validates each action against plan limits
3. Returns `403 Forbidden` with plan upgrade message when limits exceeded

---

## 8. Recommended Backend Enforcement Architecture

```
┌─────────────────────────────────────────────┐
│              API Gateway (Ocelot)            │
│  ┌─────────────────────────────────────────┐ │
│  │   PlanEnforcementMiddleware (NEW)       │ │
│  │   - Reads JWT claims for userId/planId  │ │
│  │   - Calls SubscriptionService for plan  │ │
│  │   - Validates action vs plan limits     │ │
│  │   - Returns 403 if over limit           │ │
│  └─────────────────────────────────────────┘ │
└─────────────────────────────────────────────┘
```

**Priority Order for Backend Implementation:**
1. `maxListings` — Prevents revenue loss from unlimited free listings
2. `maxImages` / `maxVideos` — Prevents storage abuse
3. `searchPriority` — Core paid feature differentiation
4. `featuredListings` — Revenue feature
5. `listingDuration` — Auto-expiry prevents stale listings

---

## 9. Changes Made in This Session

| File | Change |
|------|--------|
| `view360-step.tsx` | Plan-based 360° gating |
| `publicar/fotos/page.tsx` | Per-plan image limits |
| `csv-import-wizard.tsx` | Bulk upload plan gating |
| `smart-publish-wizard.tsx` | Pass plan props to steps |
| `dealer/publicar/page.tsx` | Pass dealerPlan to wizard |

---

## 10. Next Steps

- [ ] Restore or rebuild SubscriptionsController in AdminService
- [ ] Implement `PlanEnforcementMiddleware` in Gateway
- [ ] Add `maxListings` check to VehiclesSaleService vehicle creation endpoint
- [ ] Add `maxImages` / `maxVideos` validation to MediaService upload
- [ ] Implement `searchPriority` boost in vehicle search queries
- [ ] Add `listingDuration` auto-expiry job (scheduled worker)
- [ ] Build OklaCoins economy system
- [ ] Integration tests for all plan-gated features
