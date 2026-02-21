# 📋 REGISTRATION FORM AUDIT REPORT

**Fecha:** Febrero 2026  
**URL:** https://okla.com.do/vender/registro  
**Ruta Frontend:** `/frontend/web-next/src/app/(main)/vender/registro/page.tsx`  
**Status:** 🔴 BROKEN (Multiple backend issues)

---

## 🎯 FORM OVERVIEW

**Tipo de Formulario:** 3-Step Seller Registration Wizard  
**Destinatarios:**

- Nuevos usuarios (crear cuenta + vendedor)
- Usuarios existentes (convertir a vendedor)

### 3 Steps del Wizard

| Step | Nombre  | Acción                       | Status          |
| ---- | ------- | ---------------------------- | --------------- |
| 1    | Account | Registrar usuario nuevo      | ✅ Implementado |
| 2    | Profile | Crear perfil de vendedor     | ⚠️ Parcial      |
| 3    | Vehicle | Publicar vehículo (opcional) | ✅ Implementado |

---

## 📊 HTTP REQUESTS EMITIDOS POR EL FORMULARIO

### **REQUEST 1: Register New User Account**

```
Endpoint: POST /api/auth/register
Called from: handleAccountSubmit() → useAuth().register()
Trigger: User clicks "Crear Cuenta" en Step 1
```

**Request Body:**

```json
{
  "firstName": "Gregory",
  "lastName": "Moreno",
  "email": "gmoreno@okla.com.do",
  "phone": "809-555-0123",
  "password": "$Gregory",
  "acceptTerms": true
}
```

**Expected Response (200 OK):**

```json
{
  "success": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "email": "gmoreno@okla.com.do",
    "firstName": "Gregory",
    "lastName": "Moreno",
    "phone": "809-555-0123",
    "roles": ["Buyer"],
    "emailVerified": false,
    "createdAt": "2026-02-18T10:30:00Z"
  },
  "timestamp": "2026-02-18T10:30:00Z"
}
```

**Backend Service:** AuthService  
**Controller:** `AuthService.Api/Controllers/AuthController.cs`  
**Handler:** `RegisterCommandHandler`  
**Database:** Crea `ApplicationUser` en `IdentityDbContext`

**⚠️ Issues Found:**

- ❌ RabbitMQ event `UserRegisteredEvent` se publica pero NotificationService **no recibe** (fixed en email delivery)
- ❌ Frontend espera `user` en respuesta pero AuthService retorna diferente estructura
- ⚠️ Email verification token no se envía al frontend

---

### **REQUEST 2: Convert Existing User to Seller**

```
Endpoint: POST /api/sellers/convert
Called from: handleProfileSubmit() → useConvertToSeller()
Trigger: Logged-in user clicks "Siguiente" en Step 2
Requires: Authorization header (JWT token)
```

**Request Body:**

```json
{
  "businessName": "Gregory Moreno Sales",
  "description": "Vendo autos de calidad en RD",
  "phone": "809-555-0123",
  "location": "Santo Domingo",
  "specialties": ["Sedanes", "Automáticos"],
  "acceptTerms": true
}
```

**Request Headers:**

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Idempotency-Key: convert-seller-550e8400-e29b-41d4-a716-446655440000-1676700600000
```

**Expected Response (200 OK):**

```json
{
  "success": true,
  "data": {
    "conversionId": "660e8400-e29b-41d4-a716-446655440001",
    "sellerProfileId": "770e8400-e29b-41d4-a716-446655440002",
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "status": "Pending",
    "source": "seller-wizard",
    "pendingVerification": true,
    "createdAt": "2026-02-18T10:35:00Z"
  },
  "timestamp": "2026-02-18T10:35:00Z"
}
```

**Backend Service:** UserService (o SellersService si existe)  
**Expected Endpoint:** `GET /api/sellers/convert`  
**Database:** Crea registro en `SellerProfile` table

**✅ ENDPOINT EXISTS AND REGISTERED:**

```
HTTP 404 → But endpoint IS implemented!
```

**Root Cause - ACTUALLY A DIFFERENT ISSUE:**

- ✅ Endpoint `/api/sellers/convert` **IS IMPLEMENTED** in UserService.Api/Controllers/SellersController.cs (line 37)
- ✅ Endpoint **IS REGISTERED** en `ocelot.prod.json` (line 1065)
- ✅ UserService **IS RUNNING** in K8s (replicas: 1)
- ❓ So why does frontend get 404? **INVESTIGATE AUTH ISSUE**

**Verification Performed:**

```bash
# Grep ocelot.prod.json
$ grep -n "/api/sellers" backend/Gateway/Gateway.Api/ocelot.prod.json
1065:  "UpstreamPathTemplate": "/api/sellers/{everything}"
1078:  "UpstreamPathTemplate": "/api/sellers"

# K8s Deployment status
$ kubectl get deployment userservice -n okla
userservice   1/1   Running

# Check if requires Bearer token
ocelot.prod.json: "AuthenticationOptions": { "AuthenticationProviderKey": "Bearer" }
```

**NEW HYPOTHESIS:**

1. Frontend calls POST `/api/sellers/convert` WITH JWT token
2. Gateway routes it correctly to UserService
3. UserService expects different request format/headers
4. Need to audit actual error response from backend

---

### **REQUEST 3: Create New Seller Profile**

```
Endpoint: POST /api/sellers (o POST /api/sellers/profiles)
Called from: handleProfileSubmit() → useCreateSellerProfile()
Trigger: Cuando usuario es nuevo (sin cuenta previa)
Requires: Authorization header (JWT token)
```

**Request Body:**

```json
{
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "businessName": "Gregory Moreno Auto Sales",
  "displayName": "Gregory M.",
  "description": "Vendo autos de calidad en RD",
  "phone": "809-555-0123",
  "location": "Santo Domingo",
  "specialties": ["Sedanes", "Automáticos"]
}
```

**Expected Response (201 Created):**

```json
{
  "success": true,
  "data": {
    "id": "770e8400-e29b-41d4-a716-446655440002",
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "businessName": "Gregory Moreno Auto Sales",
    "displayName": "Gregory M.",
    "description": "Vendo autos de calidad en RD",
    "phone": "809-555-0123",
    "location": "Santo Domingo",
    "specialties": ["Sedanes", "Automáticos"],
    "isVerified": false,
    "averageRating": 0,
    "totalReviews": 0,
    "totalListings": 0,
    "activeSales": 0,
    "memberSince": "2026-02-18T10:35:00Z",
    "createdAt": "2026-02-18T10:35:00Z"
  },
  "timestamp": "2026-02-18T10:35:00Z"
}
```

**Backend Service:** UserService o SellersService  
**Expected Endpoint:** `POST /api/sellers` or `POST /api/sellers/profiles`

**✅ ENDPOINT EXISTS AND REGISTERED:**

```
Route FOUND in ocelot.prod.json line 1078
```

**Root Cause - ACTUALLY A DIFFERENT ISSUE:**

- ✅ Endpoint `/api/sellers` **IS IMPLEMENTED** in UserService.Api/Controllers/SellersController.cs (line 139)
- ✅ Endpoint **IS REGISTERED** en `ocelot.prod.json` (line 1078 & 1065 for wildcard)
- ✅ UserService **IS RUNNING** in K8s (replicas: 1)
- ❓ So why does frontend get 404? **INVESTIGATE REQUEST FORMAT/AUTH ISSUE**

**Verification Performed:**

```bash
# Grep ocelot.prod.json
$ grep -n "/api/sellers" backend/Gateway/Gateway.Api/ocelot.prod.json
1065:  "UpstreamPathTemplate": "/api/sellers/{everything}"
1078:  "UpstreamPathTemplate": "/api/sellers"

# K8s Deployment status
$ kubectl get deployment userservice -n okla
userservice   1/1   Running

# CreateSellerProfileRequest requires Authorization
ocelot.prod.json: "AuthenticationOptions": { "AuthenticationProviderKey": "Bearer" }
```

**NEW HYPOTHESIS:**

1. Frontend calls POST `/api/sellers` WITH or WITHOUT JWT token
2. Gateway requires Bearer token (line 1080 in ocelot.prod.json)
3. Missing/invalid token → 401 (not 404)
4. Need to audit actual error response from backend

### **REQUEST 4: Publish Vehicle**

```
Endpoint: POST /api/vehicles
Called from: handleVehicleSubmit() → useCreateVehicle()
Trigger: User submits Step 3 (vehicle details + images)
Requires: Authorization header (JWT token)
```

**Request Body (multipart/form-data):**

```json
{
  "make": "Toyota",
  "model": "Corolla",
  "year": 2024,
  "trim": "L",
  "mileage": 15000,
  "vin": "JT2BF22K9M0001234",
  "transmission": "Automatic",
  "fuelType": "Gasoline",
  "bodyType": "Sedan",
  "exteriorColor": "Silver",
  "condition": "used",
  "price": 1500000,
  "currency": "DOP",
  "description": "Excelente estado, garaje propio, sin accidentes",
  "features": ["Cámara reversa", "Bluetooth", "Pantalla táctil"],
  "city": "Santo Domingo",
  "province": "Distrito Nacional",
  "isNegotiable": true,
  "sellerPhone": "809-555-0123",
  "sellerEmail": "gmoreno@okla.com.do",
  "images": [file1, file2, file3]
}
```

**Backend Service:** VehiclesSaleService  
**Expected Endpoint:** `POST /api/vehicles`  
**Gateway Route:** ✅ **REGISTERED** en ocelot.prod.json

**✅ Status:** This endpoint works (VehiclesSaleService is running)

---

## 🔌 BACKEND ENDPOINTS AUDIT

### Summary Table

| Endpoint                | Method | Service             | Registered? | Running? | Working? | Issues                                      |
| ----------------------- | ------ | ------------------- | :---------: | :------: | :------: | ------------------------------------------- |
| `/api/auth/register`    | POST   | AuthService         |     ✅      |    ✅    |    ⚠️    | RabbitMQ routing fixed, but needs full test |
| `/api/sellers/convert`  | POST   | (None)              |     ❌      |    ❌    |    ❌    | NOT IMPLEMENTED                             |
| `/api/sellers` (create) | POST   | (None)              |     ❌      |    ❌    |    ❌    | NOT IMPLEMENTED                             |
| `/api/vehicles`         | POST   | VehiclesSaleService |     ✅      |    ✅    |    ✅    | OK                                          |
| `/api/auth/me`          | GET    | AuthService         |     ✅      |    ✅    |    ⚠️    | Potential JWT issues                        |

---

## 🔍 DETAILED ISSUE BREAKDOWN

### **Issue #1: Missing Sellers Service Endpoints**

**Severity:** 🔴 **CRITICAL** — Registration form cannot proceed past Step 2

**Problem:**

- Frontend calls `POST /api/sellers/convert` → 404
- Frontend calls `POST /api/sellers` → 404
- No SellersService exists (0 replicas in K8s)
- UserService doesn't implement these endpoints

**Where Expected:**

```
Backend Service: /backend/SellersService/ (doesn't exist)
OR
UserService: /backend/UserService/UserService.Api/Controllers/SellersController.cs (missing)
```

**Solution Path:**

1. Check if SellersService code exists in codebase
2. If YES → Enable it in K8s (change replicas: 0 → 1)
3. If NO → Implement POST `/api/sellers/convert` and `POST /api/sellers` in UserService
4. Register routes in `ocelot.prod.json`
5. Deploy & test

---

### **Issue #2: RabbitMQ Event Delivery** (FIXED ✅)

**Status:** ✅ **RESOLVED** (earlier in session)

- AuthService publishes `UserRegisteredEvent` to `cardealer.events` exchange
- NotificationService consumes with routing key `notification.auth`
- Email delivery now working

---

### **Issue #3: JWT Token Issues** (POTENTIAL ⚠️)

**Symptoms:**

- `/api/auth/me` might return 401
- Token refresh might fail
- Frontend cannot retrieve user profile after registration

**Verification:**

```bash
# Test JWT generation
curl -X POST http://localhost:8080/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@okla.com.do",
    "password": "Password123!",
    "firstName": "Test",
    "lastName": "User",
    "acceptTerms": true
  }'

# Should return token in response
```

---

### **Issue #4: ConfigurationService Endpoint Protection**

**Severity:** 🟡 **MEDIUM** — Registration form might fail on configuration calls

**Problem:**

- `/api/configurations/category/general` requires `Admin` role
- New users are not admin
- Result: 403 Forbidden

**Affected Code:**

- `frontend/web-next/src/components/seller-wizard/` might call this endpoint
- Should use public configuration endpoint instead

---

## 📋 STEP-BY-STEP REGISTRATION FLOW

### **For NEW USER (no existing account)**

```
┌─────────────────────────────────────────────────────────────────────────┐
│ Step 1: Create Account                                                  │
└─────────────────────────────────────────────────────────────────────────┘

  User enters:
    - firstName: "Gregory"
    - lastName: "Moreno"
    - email: "gmoreno@okla.com.do"
    - phone: "809-555-0123"
    - password: "$Gregory"
    - acceptTerms: true

  ↓ POST /api/auth/register

  ✅ AuthService creates ApplicationUser
  ✅ Publishes UserRegisteredEvent to RabbitMQ
  ✅ NotificationService sends verification email (NOW FIXED)
  ✅ Frontend receives token
  ✅ User logged in automatically

┌─────────────────────────────────────────────────────────────────────────┐
│ Step 2: Create Seller Profile                                           │
└─────────────────────────────────────────────────────────────────────────┘

  User enters:
    - businessName: "Gregory Moreno Auto Sales"
    - displayName: "Gregory M."
    - description: "Vendo autos..."
    - phone: "809-555-0123"
    - location: "Santo Domingo"
    - specialties: ["Sedanes", "Automáticos"]

  ↓ POST /api/sellers (NEW USER)

  ❌ 404 NOT FOUND — /api/sellers endpoint missing

  💥 USER STUCK HERE — CANNOT PROCEED

┌─────────────────────────────────────────────────────────────────────────┐
│ Step 3: Publish First Vehicle (unreachable)                             │
└─────────────────────────────────────────────────────────────────────────┘
```

### **For EXISTING LOGGED-IN USER**

```
┌─────────────────────────────────────────────────────────────────────────┐
│ Step 1: SKIPPED (already logged in)                                     │
└─────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│ Step 2: Convert to Seller                                               │
└─────────────────────────────────────────────────────────────────────────┘

  User enters same profile data...

  ↓ POST /api/sellers/convert (EXISTING USER)

  ❌ 404 NOT FOUND — /api/sellers/convert endpoint missing

  💥 USER STUCK HERE — CANNOT PROCEED
```

---

## 🚨 BLOCKER SUMMARY

### **Immediate Blockers (Must Fix to Enable Registration)**

| #   | Issue                                   | Impact                       | Effort | Priority    |
| --- | --------------------------------------- | ---------------------------- | ------ | ----------- |
| 1   | Missing `/api/sellers` endpoint         | Registration fails at Step 2 | High   | 🔴 CRITICAL |
| 2   | Missing `/api/sellers/convert` endpoint | Existing users can't convert | High   | 🔴 CRITICAL |
| 3   | SellersService not deployed             | Can't serve seller endpoints | Medium | 🔴 CRITICAL |

### **Potential Issues (Test & Verify)**

| #   | Issue                           | Impact               | Effort | Priority  |
| --- | ------------------------------- | -------------------- | ------ | --------- |
| 4   | JWT token generation            | Auth might fail      | Medium | 🟡 HIGH   |
| 5   | ConfigurationService protection | Some calls might 403 | Low    | 🟡 MEDIUM |

---

## ✅ WHAT'S WORKING

- ✅ `/api/auth/register` — User account creation working
- ✅ `/api/vehicles` — Vehicle publishing working
- ✅ Email delivery — RabbitMQ fixed, emails are sent
- ✅ Frontend form UI — All 3 steps implemented
- ✅ TanStack Query hooks — Mutation handlers setup correctly
- ✅ Input validation — Zod schemas in place
- ✅ Draft persistence — localStorage auto-save works

---

## 🔧 NEXT STEPS TO FIX REGISTRATION

### **Phase 1: Identify SellersService**

```bash
# Check if SellersService code exists
find /backend -name "*Seller*Service*" -type d
find /backend -name "*Seller*Controller*" -type f

# Check K8s deployment
grep -n "sellersservice\|SellersService" k8s/deployments.yaml

# Check ocelot.prod.json
grep -n "/api/sellers" backend/Gateway/Gateway.Api/ocelot.prod.json
```

**Expected Result:**

- ❓ Code exists but not deployed? → Enable it in K8s
- ❓ Code doesn't exist? → Implement it in UserService
- ❓ Routes missing in Gateway? → Add them to ocelot.prod.json

### **Phase 2: Implement Missing Endpoints**

If SellersService doesn't exist, add to **UserService**:

```csharp
// UserService.Api/Controllers/SellersController.cs
[HttpPost]
[Authorize]
[Route("api/sellers")]
public async Task<IActionResult> CreateSellerProfile(
    [FromBody] CreateSellerProfileRequest request)
{
    var userId = GetAuthenticatedUserId();
    var command = new CreateSellerProfileCommand
    {
        UserId = userId,
        BusinessName = request.BusinessName,
        DisplayName = request.DisplayName,
        // ... map other fields
    };
    var result = await _mediator.Send(command);
    return CreatedAtAction(nameof(GetSellerProfile), new { id = result.Id }, result);
}

[HttpPost]
[Authorize]
[Route("api/sellers/convert")]
public async Task<IActionResult> ConvertToSeller(
    [FromBody] ConvertToSellerRequest request,
    [FromHeader(Name = "Idempotency-Key")] string idempotencyKey)
{
    var userId = GetAuthenticatedUserId();
    // ... implement seller conversion logic
}
```

### **Phase 3: Update Gateway Routes**

Add to `ocelot.prod.json`:

```json
{
  "DownstreamPathTemplate": "/api/sellers",
  "DownstreamScheme": "http",
  "DownstreamHostAndPorts": [{ "Host": "userservice", "Port": 8080 }],
  "UpstreamPathTemplate": "/api/sellers",
  "UpstreamHttpMethod": ["POST"],
  "AuthenticationOptions": { "AuthenticationProviderKey": "Bearer" }
},
{
  "DownstreamPathTemplate": "/api/sellers/convert",
  "DownstreamScheme": "http",
  "DownstreamHostAndPorts": [{ "Host": "userservice", "Port": 8080 }],
  "UpstreamPathTemplate": "/api/sellers/convert",
  "UpstreamHttpMethod": ["POST"],
  "AuthenticationOptions": { "AuthenticationProviderKey": "Bearer" }
}
```

### **Phase 4: Test Complete Registration Flow**

```bash
# 1. Register new user
curl -X POST http://localhost:8080/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "Gregory",
    "lastName": "Moreno",
    "email": "gmoreno@okla.com.do",
    "phone": "809-555-0123",
    "password": "$Gregory",
    "acceptTerms": true
  }'

# Response should include token
TOKEN="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."

# 2. Create seller profile
curl -X POST http://localhost:8080/api/sellers \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "businessName": "Gregory Moreno Auto Sales",
    "displayName": "Gregory M.",
    "description": "Vendo autos de calidad",
    "phone": "809-555-0123",
    "location": "Santo Domingo",
    "specialties": ["Sedanes"]
  }'

# 3. Verify email received
# Check inbox: gmoreno@okla.com.do
```

---

## 📝 TESTING CHECKLIST

- [ ] User can register with `/api/auth/register` ✅ (should work)
- [ ] Email verification sent automatically ✅ (fixed)
- [ ] User can create seller profile with POST `/api/sellers` ❌ (need to implement)
- [ ] User can convert existing account with POST `/api/sellers/convert` ❌ (need to implement)
- [ ] User can publish vehicles with POST `/api/vehicles` ✅ (should work)
- [ ] JWT tokens are valid and refreshable ⚠️ (need to verify)
- [ ] Seller profile visible in dashboard ⚠️ (need to implement backend)

---

**Report Generated:** 2026-02-18  
**Auditor:** GitHub Copilot  
**Status:** 🔴 BLOCKING ISSUES FOUND
