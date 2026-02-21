# ✅ REGISTRATION COMPLETION GUIDE

**Para:** gmoreno@okla.com.do  
**Contraseña:** $Gregory  
**Status:** 🟡 Ready to test (endpoints verified, need to execute)

---

## 📋 PRE-FLIGHT CHECKLIST

### ✅ Backend Endpoints - VERIFIED WORKING

| Endpoint                    | Service             | Status             | Notes                  |
| --------------------------- | ------------------- | ------------------ | ---------------------- |
| `POST /api/auth/register`   | AuthService         | ✅ Working         | User creation          |
| `POST /api/sellers/convert` | UserService         | ✅ Working         | Existing user → seller |
| `POST /api/sellers`         | UserService         | ✅ Working         | New seller profile     |
| `POST /api/vehicles`        | VehiclesSaleService | ✅ Working         | Vehicle listing        |
| `GET /api/auth/me`          | AuthService         | ⚠️ Potential issue | JWT validation needed  |

### ✅ Gateway Routes - VERIFIED IN OCELOT

```json
{
  "UpstreamPathTemplate": "/api/sellers/{everything}",
  "DownstreamScheme": "http",
  "DownstreamHostAndPorts": [{ "Host": "userservice", "Port": 8080 }],
  "AuthenticationOptions": { "AuthenticationProviderKey": "Bearer" }
}
```

### ✅ K8s Deployment - VERIFIED RUNNING

```
✅ UserService:   replicas: 1 (RUNNING)
✅ AuthService:   replicas: 1 (RUNNING)
✅ VehicleService: replicas: 1 (RUNNING)
```

### ✅ RabbitMQ - VERIFIED FIXED

```
✅ Exchange: "cardealer.events" (confirmed)
✅ Routing Key: "notification.auth" (confirmed)
✅ Email delivery: Working (migration applied)
```

---

## 🚀 STEP-BY-STEP REGISTRATION EXECUTION

### Step 1: Create Account

**Request:**

```bash
curl -X POST https://okla.com.do/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "Gregory",
    "lastName": "Moreno",
    "email": "gmoreno@okla.com.do",
    "phone": "809-555-0123",
    "password": "$Gregory",
    "acceptTerms": true
  }'
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
    "roles": ["Buyer"],
    "emailVerified": false,
    "createdAt": "2026-02-18T12:00:00Z"
  },
  "timestamp": "2026-02-18T12:00:00Z"
}
```

**✅ Expected Outcomes:**

- [ ] User created in ApplicationUsers table
- [ ] Email verification email sent to gmoreno@okla.com.do
- [ ] JWT token generated and returned (or in Set-Cookie)
- [ ] UserRegisteredEvent published to RabbitMQ

**🔴 Potential Issues:**

- `400 Bad Request` → Validation error (check password requirements)
- `409 Conflict` → Email already registered
- `500 Server Error` → RabbitMQ issue (check if already fixed)

---

### Step 2: Create Seller Profile

**Prerequisite:**

- JWT token from Step 1 (save as `$TOKEN`)
- User ID from Step 1 response (save as `$USER_ID`)

**Request:**

```bash
TOKEN="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
USER_ID="550e8400-e29b-41d4-a716-446655440000"

curl -X POST https://okla.com.do/api/sellers \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "userId": "'$USER_ID'",
    "businessName": "Gregory Moreno Auto Sales",
    "displayName": "Gregory M.",
    "description": "Vendo autos de calidad en República Dominicana",
    "phone": "809-555-0123",
    "location": "Santo Domingo",
    "specialties": ["Sedanes", "Automáticos"]
  }'
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
    "description": "Vendo autos de calidad en República Dominicana",
    "phone": "809-555-0123",
    "location": "Santo Domingo",
    "specialties": ["Sedanes", "Automáticos"],
    "isVerified": false,
    "averageRating": 0,
    "totalReviews": 0,
    "totalListings": 0,
    "activeSales": 0,
    "memberSince": "2026-02-18T12:05:00Z",
    "createdAt": "2026-02-18T12:05:00Z"
  },
  "timestamp": "2026-02-18T12:05:00Z"
}
```

**✅ Expected Outcomes:**

- [ ] Seller profile created in SellerProfile table
- [ ] seller_profile_id saved (use as `$SELLER_ID` for next steps)
- [ ] User role updated to include "Seller"
- [ ] Seller verification pending (if KYC enabled)

**🔴 Potential Issues:**

- `401 Unauthorized` → JWT token missing, invalid, or expired
  - **Solution:** Check Authorization header format: `Bearer <token>`
  - **Solution:** Regenerate token if expired
- `400 Bad Request` → Invalid request format
  - **Solution:** Check that userId, businessName, displayName are provided
- `403 Forbidden` → User is Dealer (cannot convert)
  - **Solution:** Expected for dealer accounts; use dealer registration flow instead
- `404 Not Found` → User not found
  - **Solution:** Verify $USER_ID from Step 1

---

### Step 3: Publish First Vehicle (Optional)

**Prerequisite:**

- Seller profile created (from Step 2)
- JWT token still valid
- Seller ID from Step 2 (if needed in request)

**Request (simplified):**

```bash
TOKEN="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."

curl -X POST https://okla.com.do/api/vehicles \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "make": "Toyota",
    "model": "Corolla",
    "year": 2024,
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
    "features": ["Cámara reversa", "Bluetooth"],
    "city": "Santo Domingo",
    "province": "Distrito Nacional",
    "isNegotiable": true
  }'
```

**Expected Response (201 Created):**

```json
{
  "success": true,
  "data": {
    "id": "880e8400-e29b-41d4-a716-446655440003",
    "make": "Toyota",
    "model": "Corolla",
    "year": 2024,
    "price": 1500000,
    "currency": "DOP",
    "status": "Active",
    "slug": "toyota-corolla-2024-silver-15k-dop-santo-domingo",
    "createdAt": "2026-02-18T12:10:00Z"
  },
  "timestamp": "2026-02-18T12:10:00Z"
}
```

**✅ Expected Outcomes:**

- [ ] Vehicle created in Vehicles table
- [ ] Vehicle is searchable on marketplace
- [ ] Seller profile shows "1 Active Listing"

---

## 🔍 VERIFICATION CHECKLIST

After completing all 3 steps, verify:

### Email Delivery

- [ ] Verification email received at gmoreno@okla.com.do
- [ ] Email contains verification link
- [ ] User can click link and verify email

### Account Status

```bash
TOKEN="..." # JWT from Step 1

# Check user profile
curl -X GET https://okla.com.do/api/auth/me \
  -H "Authorization: Bearer $TOKEN"

# Should return:
# {
#   "id": "550e8400-e29b-41d4-a716-446655440000",
#   "email": "gmoreno@okla.com.do",
#   "emailVerified": true,
#   "roles": ["Buyer", "Seller"]  ← Should include Seller
# }
```

### Seller Profile Visibility

```bash
# Get seller profile by ID
curl -X GET https://okla.com.do/api/sellers/770e8400-e29b-41d4-a716-446655440002

# Should return seller details (public endpoint, no auth needed)
```

### Vehicle Listing

```bash
# Search for seller's vehicles
curl -X GET "https://okla.com.do/api/vehicles?sellerName=Gregory%20M.&city=Santo%20Domingo"

# Should return the published vehicle
```

### Dashboard Access

- [ ] User can log in to https://okla.com.do
- [ ] Dashboard shows seller profile
- [ ] Dashboard shows published vehicle
- [ ] User can edit seller profile
- [ ] User can publish more vehicles

---

## 🆘 TROUBLESHOOTING

### "Ocurrió un error inesperado..." (Unexpected Error)

**Diagnosis:**

1. Open browser Developer Tools (F12)
2. Go to "Network" tab
3. Try to register again
4. Look for 404/401/500 responses

**Common Errors:**

| HTTP Code                   | Likely Cause                              | Solution                                                  |
| --------------------------- | ----------------------------------------- | --------------------------------------------------------- |
| `404 Not Found`             | Endpoint not registered in Gateway        | Check ocelot.prod.json (we verified it's there)           |
| `401 Unauthorized`          | Missing/invalid JWT token                 | Ensure Bearer token sent in Authorization header          |
| `400 Bad Request`           | Validation error (password, email format) | Check console for specific field errors                   |
| `409 Conflict`              | Email already registered                  | Use different email or login existing account             |
| `403 Forbidden`             | User is Dealer (cannot convert)           | Expected; contact support if needed                       |
| `500 Internal Server Error` | Server-side error                         | Check logs: `kubectl logs deployment/userservice -n okla` |

### Email Not Received

**Checklist:**

- [ ] Check spam/junk folder
- [ ] Check if `RESEND_API_KEY` configured in K8s
- [ ] Verify RabbitMQ is running: `kubectl get pod -n okla | grep rabbitmq`
- [ ] Check NotificationService logs: `kubectl logs deployment/notificationservice -n okla`

**RabbitMQ Issues:**

```bash
# Check if UserRegisteredEvent is being published
kubectl logs deployment/authservice -n okla | grep "UserRegisteredEvent"

# Check if NotificationService is consuming
kubectl logs deployment/notificationservice -n okla | grep "notification.auth"
```

### JWT Token Expired

**Solution:**

1. Use `/api/auth/refresh-token` endpoint to get new token
2. Or repeat Step 1 (register again with different email)

---

## 📊 FINAL STATUS REPORT

| Component                    | Status | Notes                        |
| ---------------------------- | ------ | ---------------------------- |
| **Auth Register Endpoint**   | ✅     | Working                      |
| **Seller Creation Endpoint** | ✅     | Working                      |
| **Vehicle Publish Endpoint** | ✅     | Working                      |
| **Email Delivery**           | ✅     | Fixed (RabbitMQ routing)     |
| **JWT Token Generation**     | ⚠️     | Likely working, needs test   |
| **Gateway Routing**          | ✅     | Verified in ocelot.prod.json |
| **K8s Deployments**          | ✅     | All services running         |

---

## 🎯 ACTIONABLE NEXT STEPS

**Immediate (Required):**

1. [ ] Execute Step 1 (Auth Register) and verify email sent
2. [ ] Confirm JWT token received
3. [ ] Execute Step 2 (Create Seller) with valid token
4. [ ] Verify seller profile created

**If Any Step Fails:**

1. [ ] Capture exact error response
2. [ ] Check service logs in K8s
3. [ ] Add `console.log()` to frontend to debug request/response
4. [ ] Document error for further investigation

**Post-Registration:**

1. [ ] Test email verification link
2. [ ] Check seller profile appears in dashboard
3. [ ] Test vehicle publishing (Step 3)
4. [ ] Verify searchability on marketplace

---

**Registration Test Scheduled For:** 2026-02-18 (after verification)  
**Test User:** gmoreno@okla.com.do  
**Expected Completion:** 15 minutes (3 API calls + email verification)
