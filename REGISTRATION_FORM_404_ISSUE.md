# 🔴 Registration Form 404 Error — Diagnostics & Root Cause

**Issue Date:** February 2026  
**User Report:** Registration page at okla.com.do/vender/registro showing error "Ocurrió un error inesperado..." with multiple 404s

## Endpoints Failing (404)

```
GET /api/configurations/category/general?environment=Development  →  404
GET /api/advertising/rotation/PremiumSpot                         →  404
GET /api/advertising/homepage/brands                              →  404
GET /api/advertising/homepage/categories                          →  404
GET /api/advertising/rotation/FeaturedSpot                        →  404
GET /api/sellers                                                  →  404 (sometimes)
POST /api/auth/me                                                 →  401 (auth failure)
POST /api/auth/refresh-token                                      →  400 (bad request)
```

## Root Cause Analysis

### Issue #1: AdvertisingService Not Deployed

**Observation:**

- `ocelot.prod.json` HAS routes for `/api/advertising/**`
- Routes map to `Host: advertisingservice` on port 8080
- BUT: No `advertisingservice` pod is running in the cluster

**Evidence:**

```
Routing Rule Defined:
{
  "UpstreamPathTemplate": "/api/advertising/rotation/{section}",
  "DownstreamHostAndPorts": [{ "Host": "advertisingservice", "Port": 8080 }]
}

Status:
- Service: AdvertisingService NOT deployed in K8s
- Files: No deployment YAML for advertisingservice found
- Result: All /api/advertising/* endpoints → 503 Service Unavailable → Frontend sees 404
```

### Issue #2: ConfigurationService Endpoint Problem

**Observation:**

- Route `/api/configurations/category/{category}` exists in ocelot.prod.json
- Maps to `Host: configurationservice` on port 8080
- **BUT:** Endpoint requires `AuthenticationProviderKey: Bearer`
- **AND:** Requires `RouteClaimsRequirement: { "account_type": "4, 5" }` (Admin/Staff only)

**Frontend is calling:**

```
GET /api/configurations/category/general?environment=Development
```

**Result:**

- If user is NOT authenticated → 401
- If user is authenticated but NOT admin/staff → 403 Forbidden
- If service unreachable → 503 → Frontend shows 404

### Issue #3: /api/sellers Endpoint

**Observation:**

- Route `/api/sellers` exists and maps to `userservice`
- Requires authentication: `AuthenticationProviderKey: Bearer`
- Results in 401 if not authenticated or 404 if token invalid

### Issue #4: Auth Endpoint Failures

**Observed Errors:**

- `POST /api/auth/me` → 401
- `POST /api/auth/refresh-token` → 400

**Likely Causes:**

1. JWT token is invalid or expired
2. AuthService not returning proper token structure
3. Refresh token endpoint not working correctly
4. Email delivery issue means user profile not created properly (related to previous RabbitMQ fix)

---

## Root Causes Identified

| Endpoint                               | Actual Status Code                    | Issue                                                   |
| -------------------------------------- | ------------------------------------- | ------------------------------------------------------- |
| `/api/advertising/**`                  | 503 (Service Unavailable)             | AdvertisingService NOT deployed in K8s                  |
| `/api/configurations/category/general` | 403 (Forbidden) or 401 (Unauthorized) | Endpoint requires admin claims; user is not admin/staff |
| `/api/sellers`                         | 401 (Unauthorized)                    | User token missing or invalid                           |
| `/api/auth/me`                         | 401 (Unauthorized)                    | JWT token issue or AuthService error                    |
| `/api/auth/refresh-token`              | 400 (Bad Request)                     | Refresh token payload invalid or refresh logic broken   |

---

## Immediate Actions Required

### 1. **Deploy AdvertisingService** (BLOCKING)

```bash
# Check if advertisingservice exists
ls -la backend/AdvertisingService/

# If not, this service needs to be created or restored
# If it exists, create Kubernetes deployment manifest
```

**Status:**

- [ ] Verify source code exists
- [ ] Create K8s deployment manifest
- [ ] Build and push Docker image to GHCR
- [ ] Deploy to cluster

### 2. **Fix Registration Form Frontend Logic** (CRITICAL)

```typescript
// Current implementation likely:
// 1. Fetches configurations (403 - user not admin)
// 2. Fetches advertising slots (503 - service missing)
// 3. Fetches sellers (401 - auth issue)
// 4. Generic error shown when ANY of these fail

// Should be fixed to:
// 1. Make these calls in parallel
// 2. Handle individual failures gracefully
// 3. Show only critical errors (auth failures)
// 4. Skip non-critical data (advertising) if unavailable
```

### 3. **Verify Email Delivery Chain** (RELATED)

The email delivery RabbitMQ fix completed, but:

- [ ] New test user registered?
- [ ] Verification email received?
- [ ] User profile fully created in UserService?
- [ ] Auth tokens being issued correctly?

---

## Detailed Endpoint Breakdown

### `/api/configurations/category/general`

**Current Route:**

```json
{
  "UpstreamPathTemplate": "/api/configurations/category/{category}",
  "UpstreamHttpMethod": ["GET"],
  "DownstreamPathTemplate": "/api/configurations/category/{category}",
  "DownstreamScheme": "http",
  "DownstreamHostAndPorts": [{ "Host": "configurationservice", "Port": 8080 }],
  "AuthenticationOptions": { "AuthenticationProviderKey": "Bearer" },
  "RouteClaimsRequirement": { "account_type": "4, 5" }
}
```

**Problem:**

- Requires admin role (`account_type: 4, 5`)
- New user registering is likely `account_type: 0` (Individual Buyer) or `1` (Individual Seller)
- Route **REJECTS** the request with 403

**Solution Options:**

1. Remove role requirement if configurations should be public
2. Add separate public endpoint for general configurations
3. Have frontend handle 403 gracefully (don't fail entire form)

### `/api/advertising/**`

**Problem:**

- AdvertisingService is not deployed
- All routes to this service fail with 503
- Frontend treats 503 as 404

**Solution:**

- Deploy AdvertisingService to K8s
- Or remove these endpoints from registration form

### `/api/auth/me` & `/api/auth/refresh-token`

**Problems:**

- Auth failures suggest JWT token issues
- Could be related to email delivery fix (user not fully registered?)
- Could be token expiration
- Could be AuthService returning malformed tokens

**Solution:**

- Verify user is being created correctly after email fix
- Test JWT token generation
- Check token refresh logic

---

## Configuration Service Architecture

```
ConfigurationService (Active in K8s)
├── /api/configurations                    [Requires: Bearer + Admin role (4,5)]
├── /api/configurations/category/{cat}     [Requires: Bearer + Admin role (4,5)]
├── /api/featureflags                      [Requires: Bearer + Admin role (4,5)]
├── /api/secrets                           [Requires: Bearer + Admin role (4,5)]
└── /api/public/pricing                    [Public, NO auth required]
```

**Issue:** Registration form queries **admin-only** endpoint
**Fix:** Query `/api/public/pricing` instead, or make general configurations public

---

## Next Steps

### Immediate (Today)

1. [ ] Check if AdvertisingService source code exists
2. [ ] Check if ConfigurationService is responding
3. [ ] Verify UserService is running (`/api/sellers` endpoint)
4. [ ] Test AuthService JWT token generation

### Short-term (This sprint)

1. [ ] Deploy AdvertisingService if missing
2. [ ] Update registration form to handle 403/503 gracefully
3. [ ] Use public endpoints instead of admin-only endpoints
4. [ ] Fix auth/refresh token issues if any

### Long-term (Architecture)

1. [ ] Separate public vs authenticated endpoints
2. [ ] Add fallback/graceful degradation for optional features
3. [ ] Implement proper error handling in frontend
4. [ ] Add service health monitoring/reporting

---

**Document Updated:** February 2026  
**Status:** REQUIRES INVESTIGATION + FIXES
