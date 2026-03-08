# 🧪 OKLA E2E Production Test Suite Design

> **Version:** 1.0  
> **Date:** 2026-03-07  
> **Target:** .NET 8, xUnit, Gateway at `https://okla.com.do/api` (prod) or `http://localhost:5000/api` (local)  
> **Total Tests:** 95 tests across 8 critical flows

---

## Table of Contents

1. [Architecture & Infrastructure](#1-architecture--infrastructure)
2. [Flow 1: Auth Flow (15 tests)](#2-flow-1-auth-flow)
3. [Flow 2: Vehicle Listing Flow (14 tests)](#3-flow-2-vehicle-listing-flow)
4. [Flow 3: Contact Flow (10 tests)](#4-flow-3-contact-flow)
5. [Flow 4: Dealer Flow (10 tests)](#5-flow-4-dealer-flow)
6. [Flow 5: Billing Flow (10 tests)](#6-flow-5-billing-flow)
7. [Flow 6: Health Check Flow (22 tests)](#7-flow-6-health-check-flow)
8. [Flow 7: Gateway Routing Flow (8 tests)](#8-flow-7-gateway-routing-flow)
9. [Flow 8: Error Logging Flow (6 tests)](#9-flow-8-error-logging-flow)
10. [Shared Test State & Execution Order](#10-shared-test-state--execution-order)
11. [Implementation Notes](#11-implementation-notes)

---

## 1. Architecture & Infrastructure

### 1.1 Existing Test Infrastructure

The codebase already has **per-service unit/integration test projects** (34 `.csproj` files). Some E2E tests exist in `AuthService.Tests/E2E/` using `WebApplicationFactory`. However, **no cross-service E2E test project exists** that tests the full production API surface through the Gateway.

### 1.2 Recommended Project Structure

```
backend/
  E2ETests/
    E2ETests.csproj                    # xUnit + HttpClient
    Configuration/
      TestSettings.cs                  # Base URL, timeouts, test user creds
      TestSettings.json                # Environment-specific config
    Fixtures/
      AuthFixture.cs                   # Shared auth state (tokens, userId)
      VehicleFixture.cs                # Shared vehicle state (vehicleId, slug)
      DealerFixture.cs                 # Shared dealer state (dealerId)
      ContactFixture.cs                # Shared contact state (requestId)
    Helpers/
      HttpClientExtensions.cs          # Auth header helpers, response parsing
      TestDataGenerator.cs             # Bogus/Faker for realistic DR test data
      ApiResponseParser.cs             # Handle ApiResponse<T> and ProblemDetails
    Flows/
      T01_AuthFlowTests.cs
      T02_VehicleListingFlowTests.cs
      T03_ContactFlowTests.cs
      T04_DealerFlowTests.cs
      T05_BillingFlowTests.cs
      T06_HealthCheckFlowTests.cs
      T07_GatewayRoutingFlowTests.cs
      T08_ErrorLoggingFlowTests.cs
    Collections/
      SequentialFlowCollection.cs      # xUnit collection for ordered execution
```

### 1.3 Key Configuration

```json
{
  "E2ETests": {
    "BaseUrl": "https://okla.com.do/api",
    "TimeoutSeconds": 30,
    "TestUser": {
      "Email": "e2e-buyer@test.okla.com",
      "Password": "E2eTest!2026#Secure",
      "UserName": "e2e_buyer_test"
    },
    "TestSeller": {
      "Email": "e2e-seller@test.okla.com",
      "Password": "E2eTest!2026#Secure",
      "UserName": "e2e_seller_test"
    },
    "TestDealer": {
      "Email": "e2e-dealer@test.okla.com",
      "Password": "E2eTest!2026#Secure",
      "UserName": "e2e_dealer_test"
    },
    "AdminToken": "{{FROM_ENV_VAR}}"
  }
}
```

### 1.4 Response Envelope Types

All services wrap responses in one of two shapes:

**Success — `ApiResponse<T>`:**

```json
{
  "success": true,
  "data": { ... },
  "error": null
}
```

**Error — RFC 7807 ProblemDetails:**

```json
{
  "type": "https://okla.com/errors/...",
  "title": "Error Title",
  "status": 400,
  "detail": "Human-readable message"
}
```

---

## 2. Flow 1: Auth Flow

> **Full lifecycle:** Register → Verify Email → Login → Get Profile → Refresh Token → Logout

### T01_01 — Register New Buyer

| Field               | Value                                          |
| ------------------- | ---------------------------------------------- |
| **Description**     | Register a new buyer account with unique email |
| **Method / URL**    | `POST /api/auth/register`                      |
| **Auth**            | None                                           |
| **Expected Status** | `200 OK`                                       |
| **Dependencies**    | None (first test in flow)                      |

**Request Body:**

```json
{
  "userName": "e2e_buyer_{uuid8}",
  "email": "e2e_{uuid8}@test.okla.com",
  "password": "E2eTest!2026#Secure",
  "firstName": "E2E",
  "lastName": "Buyer",
  "phone": "+18095551234",
  "acceptTerms": true,
  "accountType": "buyer",
  "userIntent": "buy"
}
```

**Expected Response Shape:**

```json
{
  "success": true,
  "data": {
    "accessToken": "string (JWT)",
    "refreshToken": "string",
    "expiresAt": "datetime",
    "userId": "guid",
    "email": "string",
    "emailConfirmed": false
  }
}
```

**Assertions:**

- `data.accessToken` is a valid JWT
- `data.userId` is a non-empty GUID
- `data.email` matches input
- Store `accessToken`, `refreshToken`, `userId` in `AuthFixture`

---

### T01_02 — Register Duplicate Email Returns Error

| Field               | Value                                                  |
| ------------------- | ------------------------------------------------------ |
| **Description**     | Attempting to register with the same email should fail |
| **Method / URL**    | `POST /api/auth/register`                              |
| **Auth**            | None                                                   |
| **Expected Status** | `400 Bad Request`                                      |
| **Dependencies**    | T01_01                                                 |

**Request Body:** Same email as T01_01

**Expected Response:** ProblemDetails or `{ "success": false, "error": "..." }`

---

### T01_03 — Register With Invalid Password Fails Validation

| Field               | Value                                                     |
| ------------------- | --------------------------------------------------------- |
| **Description**     | Password missing uppercase/digit/special char should fail |
| **Method / URL**    | `POST /api/auth/register`                                 |
| **Auth**            | None                                                      |
| **Expected Status** | `400 Bad Request`                                         |
| **Dependencies**    | None                                                      |

**Request Body:**

```json
{
  "userName": "e2e_weak_{uuid8}",
  "email": "weak_{uuid8}@test.okla.com",
  "password": "weak",
  "acceptTerms": true
}
```

---

### T01_04 — Verify Email With Token

| Field               | Value                                                                           |
| ------------------- | ------------------------------------------------------------------------------- |
| **Description**     | Verify the registered user's email (requires test email intercept or DB bypass) |
| **Method / URL**    | `POST /api/auth/verify-email`                                                   |
| **Auth**            | None                                                                            |
| **Expected Status** | `200 OK`                                                                        |
| **Dependencies**    | T01_01                                                                          |

**Request Body:**

```json
{
  "token": "{{verificationToken from email or DB}}"
}
```

**Note:** In E2E against production, this requires either: (a) a test endpoint to auto-verify, (b) reading the verification token from the database, or (c) intercepting the email via a test mailbox. **Recommended:** Add a test-only `/api/auth/test/auto-verify/{email}` endpoint behind a feature flag.

---

### T01_05 — Login With Valid Credentials

| Field               | Value                                                 |
| ------------------- | ----------------------------------------------------- |
| **Description**     | Login with the registered (and ideally verified) user |
| **Method / URL**    | `POST /api/auth/login`                                |
| **Auth**            | None                                                  |
| **Expected Status** | `200 OK`                                              |
| **Dependencies**    | T01_01 (or T01_04 if verification required)           |

**Request Body:**

```json
{
  "email": "{{registered_email}}",
  "password": "E2eTest!2026#Secure",
  "captchaToken": null
}
```

**Expected Response Shape:**

```json
{
  "success": true,
  "data": {
    "accessToken": "string",
    "refreshToken": "string",
    "expiresAt": "datetime",
    "requiresTwoFactor": false,
    "userId": "guid"
  }
}
```

**Assertions:**

- `data.accessToken` is a valid JWT with claims (`sub`, `email`, `role`)
- `data.requiresTwoFactor` is `false`
- Update `AuthFixture` tokens

---

### T01_06 — Login With Wrong Password

| Field               | Value                                     |
| ------------------- | ----------------------------------------- |
| **Description**     | Login with incorrect password should fail |
| **Method / URL**    | `POST /api/auth/login`                    |
| **Auth**            | None                                      |
| **Expected Status** | `400 Bad Request` or `401 Unauthorized`   |
| **Dependencies**    | T01_01                                    |

---

### T01_07 — Login With Non-Existent Email

| Field               | Value                                   |
| ------------------- | --------------------------------------- |
| **Description**     | Login with an email that doesn't exist  |
| **Method / URL**    | `POST /api/auth/login`                  |
| **Auth**            | None                                    |
| **Expected Status** | `400 Bad Request` or `401 Unauthorized` |
| **Dependencies**    | None                                    |

---

### T01_08 — Get Current User Profile

| Field               | Value                                 |
| ------------------- | ------------------------------------- |
| **Description**     | Retrieve authenticated user's profile |
| **Method / URL**    | `GET /api/auth/me`                    |
| **Auth**            | `Bearer {{accessToken}}`              |
| **Expected Status** | `200 OK`                              |
| **Dependencies**    | T01_05                                |

**Expected Response Shape:**

```json
{
  "success": true,
  "data": {
    "id": "guid",
    "email": "string",
    "firstName": "string",
    "lastName": "string",
    "accountType": "buyer",
    "isVerified": true,
    "isEmailVerified": true
  }
}
```

---

### T01_09 — Get Profile Without Token Returns 401

| Field               | Value                                                |
| ------------------- | ---------------------------------------------------- |
| **Description**     | Accessing /me without auth header should be rejected |
| **Method / URL**    | `GET /api/auth/me`                                   |
| **Auth**            | None                                                 |
| **Expected Status** | `401 Unauthorized`                                   |
| **Dependencies**    | None                                                 |

---

### T01_10 — Refresh Token

| Field               | Value                                         |
| ------------------- | --------------------------------------------- |
| **Description**     | Exchange refresh token for a new access token |
| **Method / URL**    | `POST /api/auth/refresh-token`                |
| **Auth**            | None (refresh token in body)                  |
| **Expected Status** | `200 OK`                                      |
| **Dependencies**    | T01_05                                        |

**Request Body:**

```json
{
  "refreshToken": "{{refreshToken from login}}"
}
```

**Expected Response Shape:**

```json
{
  "success": true,
  "data": {
    "accessToken": "string (new JWT)",
    "refreshToken": "string (rotated)",
    "expiresAt": "datetime"
  }
}
```

**Assertions:**

- New `accessToken` differs from old one
- New `refreshToken` differs from old one (token rotation)
- Update `AuthFixture`

---

### T01_11 — Refresh With Invalid Token

| Field               | Value                                      |
| ------------------- | ------------------------------------------ |
| **Description**     | Using an invalid refresh token should fail |
| **Method / URL**    | `POST /api/auth/refresh-token`             |
| **Auth**            | None                                       |
| **Expected Status** | `400 Bad Request` or `401 Unauthorized`    |
| **Dependencies**    | None                                       |

**Request Body:**

```json
{
  "refreshToken": "invalid-token-value"
}
```

---

### T01_12 — Forgot Password (Always Returns OK)

| Field               | Value                                                                              |
| ------------------- | ---------------------------------------------------------------------------------- |
| **Description**     | Forgot password returns 200 regardless of email existence (security best practice) |
| **Method / URL**    | `POST /api/auth/forgot-password`                                                   |
| **Auth**            | None                                                                               |
| **Expected Status** | `200 OK`                                                                           |
| **Dependencies**    | None                                                                               |

**Request Body:**

```json
{
  "email": "nonexistent@test.okla.com"
}
```

---

### T01_13 — Resend Verification Email

| Field               | Value                                         |
| ------------------- | --------------------------------------------- |
| **Description**     | Resend verification email for registered user |
| **Method / URL**    | `POST /api/auth/resend-verification`          |
| **Auth**            | None                                          |
| **Expected Status** | `200 OK`                                      |
| **Dependencies**    | T01_01                                        |

**Request Body:**

```json
{
  "email": "{{registered_email}}"
}
```

---

### T01_14 — Register Seller Account

| Field               | Value                                                    |
| ------------------- | -------------------------------------------------------- |
| **Description**     | Register a seller account for use in later vehicle flows |
| **Method / URL**    | `POST /api/auth/register`                                |
| **Auth**            | None                                                     |
| **Expected Status** | `200 OK`                                                 |
| **Dependencies**    | None                                                     |

**Request Body:**

```json
{
  "userName": "e2e_seller_{uuid8}",
  "email": "e2e_seller_{uuid8}@test.okla.com",
  "password": "E2eTest!2026#Secure",
  "firstName": "E2E",
  "lastName": "Seller",
  "acceptTerms": true,
  "accountType": "seller",
  "userIntent": "sell"
}
```

**Store:** `sellerAccessToken`, `sellerUserId` in `AuthFixture`

---

### T01_15 — Logout Revokes Refresh Token

| Field               | Value                                      |
| ------------------- | ------------------------------------------ |
| **Description**     | Logout should invalidate the refresh token |
| **Method / URL**    | `POST /api/auth/logout`                    |
| **Auth**            | `Bearer {{accessToken}}`                   |
| **Expected Status** | `200 OK`                                   |
| **Dependencies**    | T01_05                                     |

**Request Body:**

```json
{
  "refreshToken": "{{refreshToken}}"
}
```

**Post-condition:** Using the old refresh token in T01_10 should now fail.

---

## 3. Flow 2: Vehicle Listing Flow

> **Full lifecycle:** Get Settings → Search (empty) → Create Draft → Upload Photo → Publish → Search → View Details → View by Slug → Similar → Featured → Favorite → Update → Delete

### T02_01 — Get Vehicle Settings (Public)

| Field               | Value                                       |
| ------------------- | ------------------------------------------- |
| **Description**     | Fetch configurable vehicle listing settings |
| **Method / URL**    | `GET /api/vehicles/settings`                |
| **Auth**            | None (public)                               |
| **Expected Status** | `200 OK`                                    |
| **Dependencies**    | None                                        |

**Expected Response Shape:**

```json
{
  "maxImagesPerListing": 30,
  "listingExpirationDays": 90,
  "featuredDurationDays": 30,
  "maxPriceDop": 100000000,
  "paginationDefault": 20,
  "requireKycToSell": true
}
```

---

### T02_02 — Search Vehicles (Public, No Filters)

| Field               | Value                                                 |
| ------------------- | ----------------------------------------------------- |
| **Description**     | Search all published vehicles with default pagination |
| **Method / URL**    | `GET /api/vehicles?page=1&pageSize=10`                |
| **Auth**            | None (public)                                         |
| **Expected Status** | `200 OK`                                              |
| **Dependencies**    | None                                                  |

**Expected Response Shape:**

```json
{
  "vehicles": [...],
  "totalCount": 0,
  "page": 1,
  "pageSize": 10,
  "totalPages": 0
}
```

**Assertions:**

- Response contains `vehicles` array
- `page` equals 1
- `pageSize` equals 10

---

### T02_03 — Search Vehicles With Filters

| Field               | Value                                                                           |
| ------------------- | ------------------------------------------------------------------------------- |
| **Description**     | Search with make, year, price range filters                                     |
| **Method / URL**    | `GET /api/vehicles?make=Toyota&minYear=2020&maxPrice=2000000&page=1&pageSize=5` |
| **Auth**            | None (public)                                                                   |
| **Expected Status** | `200 OK`                                                                        |
| **Dependencies**    | None                                                                            |

---

### T02_04 — Create Vehicle Draft (Authenticated Seller)

| Field               | Value                                        |
| ------------------- | -------------------------------------------- |
| **Description**     | Create a new vehicle listing in Draft status |
| **Method / URL**    | `POST /api/vehicles`                         |
| **Auth**            | `Bearer {{sellerAccessToken}}`               |
| **Expected Status** | `201 Created`                                |
| **Dependencies**    | T01_14 (seller registered)                   |

**Request Body:**

```json
{
  "make": "Toyota",
  "model": "Corolla",
  "year": 2023,
  "price": 1250000,
  "currency": "DOP",
  "description": "E2E Test Vehicle - Excellent condition, single owner",
  "mileage": 15000,
  "mileageUnit": "Kilometers",
  "vehicleType": "Car",
  "bodyStyle": "Sedan",
  "fuelType": "Gasoline",
  "transmission": "Automatic",
  "driveType": "FWD",
  "doors": 4,
  "seats": 5,
  "exteriorColor": "White",
  "interiorColor": "Black",
  "condition": "Used",
  "isCertified": false,
  "hasCleanTitle": true,
  "city": "Santo Domingo",
  "state": "Distrito Nacional",
  "country": "DO",
  "vin": "E2ETEST{random8hex}",
  "images": ["https://placehold.co/800x600/png?text=E2E+Test+Vehicle"],
  "features": ["Air Conditioning", "Bluetooth", "Backup Camera"]
}
```

**Expected Response Shape:**

```json
{
  "id": "guid",
  "slug": "2023-toyota-corolla-{shortId}",
  "status": "Draft",
  "message": "Vehicle created successfully. Call /publish to make it visible."
}
```

**Store:** `vehicleId`, `vehicleSlug` in `VehicleFixture`

---

### T02_05 — Get Vehicle By ID

| Field               | Value                             |
| ------------------- | --------------------------------- |
| **Description**     | Fetch created vehicle by its GUID |
| **Method / URL**    | `GET /api/vehicles/{vehicleId}`   |
| **Auth**            | None (public)                     |
| **Expected Status** | `200 OK`                          |
| **Dependencies**    | T02_04                            |

**Assertions:**

- `id` matches `vehicleId`
- `make` is "Toyota"
- `model` is "Corolla"
- `status` is "Draft"
- `sellerId` matches `sellerUserId`

---

### T02_06 — Get Vehicle By Slug

| Field               | Value                                  |
| ------------------- | -------------------------------------- |
| **Description**     | Fetch vehicle using SEO-friendly slug  |
| **Method / URL**    | `GET /api/vehicles/slug/{vehicleSlug}` |
| **Auth**            | None (public)                          |
| **Expected Status** | `200 OK`                               |
| **Dependencies**    | T02_04                                 |

---

### T02_07 — Get Vehicle By VIN

| Field               | Value                          |
| ------------------- | ------------------------------ |
| **Description**     | Fetch vehicle using VIN lookup |
| **Method / URL**    | `GET /api/vehicles/vin/{vin}`  |
| **Auth**            | None (public)                  |
| **Expected Status** | `200 OK`                       |
| **Dependencies**    | T02_04                         |

---

### T02_08 — Upload Vehicle Image (via MediaService)

| Field               | Value                                                     |
| ------------------- | --------------------------------------------------------- |
| **Description**     | Upload an image for the vehicle through the media service |
| **Method / URL**    | `POST /api/media/upload`                                  |
| **Auth**            | `Bearer {{sellerAccessToken}}`                            |
| **Expected Status** | `200 OK` or `201 Created`                                 |
| **Dependencies**    | T01_14, T02_04                                            |

**Request:** `multipart/form-data` with:

- `file`: JPEG image (≤10MB)
- `vehicleId`: `{{vehicleId}}`
- `purpose`: `"vehicle"`

**Expected Response Shape:**

```json
{
  "url": "https://cdn.okla.com.do/...",
  "thumbnailUrl": "https://cdn.okla.com.do/.../thumb_...",
  "mediaId": "guid"
}
```

---

### T02_09 — Publish Vehicle

| Field               | Value                                         |
| ------------------- | --------------------------------------------- |
| **Description**     | Change vehicle status from Draft to Published |
| **Method / URL**    | `POST /api/vehicles/{vehicleId}/publish`      |
| **Auth**            | `Bearer {{sellerAccessToken}}`                |
| **Expected Status** | `200 OK`                                      |
| **Dependencies**    | T02_04                                        |

**Expected:** Vehicle status changes to `Published`

---

### T02_10 — Track Vehicle View (Public)

| Field               | Value                                   |
| ------------------- | --------------------------------------- |
| **Description**     | Record an anonymous view on the vehicle |
| **Method / URL**    | `POST /api/vehicles/{vehicleId}/views`  |
| **Auth**            | None (public)                           |
| **Expected Status** | `200 OK`                                |
| **Dependencies**    | T02_09                                  |

---

### T02_11 — Get Similar Vehicles

| Field               | Value                                   |
| ------------------- | --------------------------------------- |
| **Description**     | Get vehicles similar to the created one |
| **Method / URL**    | `GET /api/vehicles/{vehicleId}/similar` |
| **Auth**            | None (public, 60s cache)                |
| **Expected Status** | `200 OK`                                |
| **Dependencies**    | T02_09                                  |

---

### T02_12 — Get Featured Vehicles

| Field               | Value                                             |
| ------------------- | ------------------------------------------------- |
| **Description**     | Get featured vehicles list (public homepage data) |
| **Method / URL**    | `GET /api/vehicles/featured`                      |
| **Auth**            | None (public, 60s cache)                          |
| **Expected Status** | `200 OK`                                          |
| **Dependencies**    | None                                              |

---

### T02_13 — Add Vehicle to Favorites

| Field               | Value                                           |
| ------------------- | ----------------------------------------------- |
| **Description**     | Authenticated buyer adds a vehicle to favorites |
| **Method / URL**    | `POST /api/favorites/{vehicleId}`               |
| **Auth**            | `Bearer {{buyerAccessToken}}`                   |
| **Expected Status** | `200 OK` or `201 Created`                       |
| **Dependencies**    | T01_05, T02_09                                  |

**Post-condition:** `GET /api/favorites` returns the vehicle

---

### T02_14 — Delete Test Vehicle (Cleanup)

| Field               | Value                               |
| ------------------- | ----------------------------------- |
| **Description**     | Delete the test vehicle to clean up |
| **Method / URL**    | `DELETE /api/vehicles/{vehicleId}`  |
| **Auth**            | `Bearer {{sellerAccessToken}}`      |
| **Expected Status** | `200 OK` or `204 No Content`        |
| **Dependencies**    | T02_04                              |

---

## 4. Flow 3: Contact Flow

> **Full lifecycle:** Create Contact Request → Get by Buyer → Get by Seller → View Detail → Reply → Mark Read → Archive → Delete

### T03_01 — Create Contact Request

| Field               | Value                                                       |
| ------------------- | ----------------------------------------------------------- |
| **Description**     | Buyer sends inquiry about a vehicle to the seller           |
| **Method / URL**    | `POST /api/contactrequests`                                 |
| **Auth**            | `Bearer {{buyerAccessToken}}`                               |
| **Expected Status** | `201 Created`                                               |
| **Dependencies**    | T01_05 (buyer), T01_14 (seller), T02_09 (published vehicle) |

**Request Body:**

```json
{
  "vehicleId": "{{vehicleId}}",
  "sellerId": "{{sellerUserId}}",
  "subject": "E2E Test Inquiry - 2023 Toyota Corolla",
  "buyerName": "E2E Buyer",
  "buyerEmail": "e2e_buyer@test.okla.com",
  "buyerPhone": "+18095551234",
  "message": "Hola, me interesa este vehículo. ¿Está disponible para verlo?"
}
```

**Expected Response Shape:**

```json
{
  "success": true,
  "data": {
    "id": "guid",
    "vehicleId": "guid",
    "status": "New"
  }
}
```

**Store:** `contactRequestId` in `ContactFixture`

---

### T03_02 — Get My Inquiries (Buyer Perspective)

| Field               | Value                                   |
| ------------------- | --------------------------------------- |
| **Description**     | Buyer retrieves their sent inquiries    |
| **Method / URL**    | `GET /api/contactrequests/my-inquiries` |
| **Auth**            | `Bearer {{buyerAccessToken}}`           |
| **Expected Status** | `200 OK`                                |
| **Dependencies**    | T03_01                                  |

**Assertions:**

- Response contains at least 1 inquiry
- One of them matches `contactRequestId`

---

### T03_03 — Get Received Inquiries (Seller Perspective)

| Field               | Value                               |
| ------------------- | ----------------------------------- |
| **Description**     | Seller retrieves received inquiries |
| **Method / URL**    | `GET /api/contactrequests/received` |
| **Auth**            | `Bearer {{sellerAccessToken}}`      |
| **Expected Status** | `200 OK`                            |
| **Dependencies**    | T03_01                              |

---

### T03_04 — Get Contact Request Detail

| Field               | Value                                         |
| ------------------- | --------------------------------------------- |
| **Description**     | Get the full contact request with messages    |
| **Method / URL**    | `GET /api/contactrequests/{contactRequestId}` |
| **Auth**            | `Bearer {{buyerAccessToken}}`                 |
| **Expected Status** | `200 OK`                                      |
| **Dependencies**    | T03_01                                        |

---

### T03_05 — Reply to Contact Request (Seller)

| Field               | Value                                                |
| ------------------- | ---------------------------------------------------- |
| **Description**     | Seller replies to the buyer's inquiry                |
| **Method / URL**    | `POST /api/contactrequests/{contactRequestId}/reply` |
| **Auth**            | `Bearer {{sellerAccessToken}}`                       |
| **Expected Status** | `201 Created`                                        |
| **Dependencies**    | T03_01                                               |

**Request Body:**

```json
{
  "message": "Sí, el vehículo está disponible. ¿Cuándo puede venir a verlo?"
}
```

---

### T03_06 — Reply to Contact Request (Buyer)

| Field               | Value                                                |
| ------------------- | ---------------------------------------------------- |
| **Description**     | Buyer replies back in the thread                     |
| **Method / URL**    | `POST /api/contactrequests/{contactRequestId}/reply` |
| **Auth**            | `Bearer {{buyerAccessToken}}`                        |
| **Expected Status** | `201 Created`                                        |
| **Dependencies**    | T03_05                                               |

**Request Body:**

```json
{
  "message": "Perfecto, ¿está disponible este sábado a las 10am?"
}
```

---

### T03_07 — Mark Contact Request as Read

| Field               | Value                                                |
| ------------------- | ---------------------------------------------------- |
| **Description**     | Seller marks the conversation as read                |
| **Method / URL**    | `PATCH /api/contactrequests/{contactRequestId}/read` |
| **Auth**            | `Bearer {{sellerAccessToken}}`                       |
| **Expected Status** | `204 No Content`                                     |
| **Dependencies**    | T03_01                                               |

---

### T03_08 — Archive Contact Request

| Field               | Value                                                   |
| ------------------- | ------------------------------------------------------- |
| **Description**     | Seller archives the conversation                        |
| **Method / URL**    | `PATCH /api/contactrequests/{contactRequestId}/archive` |
| **Auth**            | `Bearer {{sellerAccessToken}}`                          |
| **Expected Status** | `204 No Content`                                        |
| **Dependencies**    | T03_01                                                  |

---

### T03_09 — Unauthorized Access to Contact Request

| Field               | Value                                                         |
| ------------------- | ------------------------------------------------------------- |
| **Description**     | A third user should not access another user's contact request |
| **Method / URL**    | `GET /api/contactrequests/{contactRequestId}`                 |
| **Auth**            | None or a different user's token                              |
| **Expected Status** | `401 Unauthorized` or `403 Forbidden`                         |
| **Dependencies**    | T03_01                                                        |

---

### T03_10 — Delete Contact Request

| Field               | Value                                            |
| ------------------- | ------------------------------------------------ |
| **Description**     | Delete the test contact request                  |
| **Method / URL**    | `DELETE /api/contactrequests/{contactRequestId}` |
| **Auth**            | `Bearer {{buyerAccessToken}}`                    |
| **Expected Status** | `204 No Content`                                 |
| **Dependencies**    | T03_01                                           |

---

## 5. Flow 4: Dealer Flow

> **Full lifecycle:** Register Dealer User → Create Dealer Profile → Get Dealer → Set DealerId on Auth → Update Dealer → Get Onboarding → Seller Convert

### T04_01 — Register Dealer User Account

| Field               | Value                                        |
| ------------------- | -------------------------------------------- |
| **Description**     | Register a new user with dealer account type |
| **Method / URL**    | `POST /api/auth/register`                    |
| **Auth**            | None                                         |
| **Expected Status** | `200 OK`                                     |
| **Dependencies**    | None                                         |

**Request Body:**

```json
{
  "userName": "e2e_dealer_{uuid8}",
  "email": "e2e_dealer_{uuid8}@test.okla.com",
  "password": "E2eTest!2026#Secure",
  "firstName": "E2E",
  "lastName": "Dealer",
  "acceptTerms": true,
  "accountType": "dealer",
  "userIntent": "sell"
}
```

**Store:** `dealerAccessToken`, `dealerUserId`

---

### T04_02 — Create Dealer Profile

| Field               | Value                                                   |
| ------------------- | ------------------------------------------------------- |
| **Description**     | Create a dealer (company) registration. Status: Pending |
| **Method / URL**    | `POST /api/dealers`                                     |
| **Auth**            | `Bearer {{dealerAccessToken}}`                          |
| **Expected Status** | `201 Created`                                           |
| **Dependencies**    | T04_01                                                  |

**Request Body:**

```json
{
  "businessName": "E2E Test Motors",
  "businessType": "Dealer",
  "rnc": "123456789",
  "email": "e2e_dealer@test.okla.com",
  "phone": "+18095559999",
  "address": "Av. Winston Churchill, Plaza Test Local 101",
  "city": "Santo Domingo",
  "province": "Distrito Nacional",
  "description": "E2E Test Dealer — created by automated test suite"
}
```

**Expected Response Shape:**

```json
{
  "id": "guid",
  "businessName": "E2E Test Motors",
  "status": "Pending",
  "ownerUserId": "guid"
}
```

**Store:** `dealerId` in `DealerFixture`

---

### T04_03 — Get Dealer Profile by ID

| Field               | Value                                  |
| ------------------- | -------------------------------------- |
| **Description**     | Fetch the created dealer profile       |
| **Method / URL**    | `GET /api/dealers/{dealerId}`          |
| **Auth**            | None or `Bearer {{dealerAccessToken}}` |
| **Expected Status** | `200 OK`                               |
| **Dependencies**    | T04_02                                 |

---

### T04_04 — Get Dealer by Owner

| Field               | Value                                   |
| ------------------- | --------------------------------------- |
| **Description**     | Fetch dealer by the owner user ID       |
| **Method / URL**    | `GET /api/dealers/owner/{dealerUserId}` |
| **Auth**            | `Bearer {{dealerAccessToken}}`          |
| **Expected Status** | `200 OK`                                |
| **Dependencies**    | T04_02                                  |

---

### T04_05 — Get My Dealer (Shortcut)

| Field               | Value                                           |
| ------------------- | ----------------------------------------------- |
| **Description**     | Fetch dealer profile for the authenticated user |
| **Method / URL**    | `GET /api/dealers/me`                           |
| **Auth**            | `Bearer {{dealerAccessToken}}`                  |
| **Expected Status** | `200 OK`                                        |
| **Dependencies**    | T04_02                                          |

---

### T04_06 — Set Dealer ID on Auth User

| Field               | Value                                                   |
| ------------------- | ------------------------------------------------------- |
| **Description**     | Link the dealer profile to the auth user for JWT claims |
| **Method / URL**    | `POST /api/auth/set-dealer-id`                          |
| **Auth**            | `Bearer {{dealerAccessToken}}`                          |
| **Expected Status** | `200 OK`                                                |
| **Dependencies**    | T04_02                                                  |

**Request Body:**

```json
{
  "dealerId": "{{dealerId}}"
}
```

---

### T04_07 — Duplicate Dealer Registration Fails

| Field               | Value                                                  |
| ------------------- | ------------------------------------------------------ |
| **Description**     | Same user trying to create a second dealer should fail |
| **Method / URL**    | `POST /api/dealers`                                    |
| **Auth**            | `Bearer {{dealerAccessToken}}`                         |
| **Expected Status** | `400 Bad Request`                                      |
| **Dependencies**    | T04_02                                                 |

**Expected Response:** ProblemDetails with `errorCode: "ALREADY_DEALER"`

---

### T04_08 — Convert Buyer to Seller

| Field               | Value                             |
| ------------------- | --------------------------------- |
| **Description**     | Convert a buyer account to seller |
| **Method / URL**    | `POST /api/sellers/convert`       |
| **Auth**            | `Bearer {{buyerAccessToken}}`     |
| **Expected Status** | `200 OK`                          |
| **Dependencies**    | T01_05                            |

---

### T04_09 — Get Seller Profile by User

| Field               | Value                              |
| ------------------- | ---------------------------------- |
| **Description**     | Retrieve seller profile for a user |
| **Method / URL**    | `GET /api/sellers/user/{userId}`   |
| **Auth**            | `Bearer {{accessToken}}`           |
| **Expected Status** | `200 OK` or `404 Not Found`        |
| **Dependencies**    | T04_08                             |

---

### T04_10 — Update Dealer Profile

| Field               | Value                              |
| ------------------- | ---------------------------------- |
| **Description**     | Update dealer business information |
| **Method / URL**    | `PUT /api/dealers/{dealerId}`      |
| **Auth**            | `Bearer {{dealerAccessToken}}`     |
| **Expected Status** | `200 OK`                           |
| **Dependencies**    | T04_02                             |

**Request Body:**

```json
{
  "businessName": "E2E Test Motors Updated",
  "phone": "+18095558888",
  "description": "Updated description from E2E test"
}
```

---

## 6. Flow 5: Billing Flow

> **Full lifecycle:** Get Plans → Create Customer → Attach Payment Method → Create Subscription → Get Subscription → Webhook Simulation → Invoice

### T05_01 — Get Public Plans

| Field               | Value                                         |
| ------------------- | --------------------------------------------- |
| **Description**     | Get all available subscription plans (public) |
| **Method / URL**    | `GET /api/billing/plans`                      |
| **Auth**            | None (AllowAnonymous)                         |
| **Expected Status** | `200 OK`                                      |
| **Dependencies**    | None                                          |

**Expected Response Shape:**

```json
[
  {
    "planName": "string",
    "monthlyPrice": 0.00,
    "annualPrice": 0.00,
    "maxVehicles": 0,
    "maxUsers": 0,
    "features": [...]
  }
]
```

---

### T05_02 — Get Dealer Billing Plans

| Field               | Value                                       |
| ------------------- | ------------------------------------------- |
| **Description**     | Get subscription plans designed for dealers |
| **Method / URL**    | `GET /api/dealer-billing/plans`             |
| **Auth**            | None (public)                               |
| **Expected Status** | `200 OK`                                    |
| **Dependencies**    | None                                        |

---

### T05_03 — Get Subscription Plans

| Field               | Value                                    |
| ------------------- | ---------------------------------------- |
| **Description**     | Get plans via the subscriptions endpoint |
| **Method / URL**    | `GET /api/subscriptions/plans`           |
| **Auth**            | None (public)                            |
| **Expected Status** | `200 OK`                                 |
| **Dependencies**    | None                                     |

---

### T05_04 — Create Stripe Customer

| Field               | Value                                                |
| ------------------- | ---------------------------------------------------- |
| **Description**     | Create a Stripe customer for the dealer              |
| **Method / URL**    | `POST /api/billing/customers`                        |
| **Auth**            | `Bearer {{dealerAccessToken}}` (with dealerId claim) |
| **Expected Status** | `200 OK`                                             |
| **Dependencies**    | T04_06                                               |

**Request Body:**

```json
{
  "dealerId": "{{dealerId}}",
  "email": "e2e_dealer@test.okla.com",
  "name": "E2E Test Motors"
}
```

**Note:** This test should only run against staging/test Stripe keys, never prod.

---

### T05_05 — Get Stripe Customer

| Field               | Value                                     |
| ------------------- | ----------------------------------------- |
| **Description**     | Retrieve the Stripe customer by dealer ID |
| **Method / URL**    | `GET /api/billing/customers/{dealerId}`   |
| **Auth**            | `Bearer {{dealerAccessToken}}`            |
| **Expected Status** | `200 OK`                                  |
| **Dependencies**    | T05_04                                    |

---

### T05_06 — Create Subscription

| Field               | Value                                |
| ------------------- | ------------------------------------ |
| **Description**     | Create a subscription for the dealer |
| **Method / URL**    | `POST /api/subscriptions`            |
| **Auth**            | `Bearer {{dealerAccessToken}}`       |
| **Expected Status** | `201 Created`                        |
| **Dependencies**    | T05_04                               |

**Request Body:**

```json
{
  "plan": "Professional",
  "cycle": "Monthly",
  "pricePerCycle": 149.0,
  "maxUsers": 5,
  "maxVehicles": 100,
  "trialDays": 14,
  "features": "AI Processing,Priority Support"
}
```

**Store:** `subscriptionId`

---

### T05_07 — Get Subscription by Dealer

| Field               | Value                                      |
| ------------------- | ------------------------------------------ |
| **Description**     | Retrieve the active subscription           |
| **Method / URL**    | `GET /api/subscriptions/dealer/{dealerId}` |
| **Auth**            | `Bearer {{dealerAccessToken}}`             |
| **Expected Status** | `200 OK`                                   |
| **Dependencies**    | T05_06                                     |

---

### T05_08 — Get My Subscription (JWT-based)

| Field               | Value                                                 |
| ------------------- | ----------------------------------------------------- |
| **Description**     | Get subscription using the authenticated dealer's JWT |
| **Method / URL**    | `GET /api/billing/subscriptions`                      |
| **Auth**            | `Bearer {{dealerAccessToken}}` (with dealerId claim)  |
| **Expected Status** | `200 OK`                                              |
| **Dependencies**    | T05_06                                                |

---

### T05_09 — Stripe Webhook Simulation (Signature Validation)

| Field               | Value                                                              |
| ------------------- | ------------------------------------------------------------------ |
| **Description**     | Send a Stripe webhook without valid signature — should be rejected |
| **Method / URL**    | `POST /api/stripewebhooks`                                         |
| **Auth**            | None (AllowAnonymous, verified by Stripe signature)                |
| **Expected Status** | `400 Bad Request`                                                  |
| **Dependencies**    | None                                                               |

**Request Body:** Raw JSON Stripe event
**Headers:** `Stripe-Signature: invalid_signature`

**Purpose:** Validates that the webhook endpoint rejects unsigned/tampered events.

---

### T05_10 — Get OklaCoins Packages (Public)

| Field               | Value                             |
| ------------------- | --------------------------------- |
| **Description**     | Get available OKLA Coins packages |
| **Method / URL**    | `GET /api/okla-coins/packages`    |
| **Auth**            | None                              |
| **Expected Status** | `200 OK`                          |
| **Dependencies**    | None                              |

---

## 7. Flow 6: Health Check Flow

> **All 22 service health endpoints must return 200**

Each test follows the same pattern:

| Field               | Value                           |
| ------------------- | ------------------------------- |
| **Method**          | `GET`                           |
| **Auth**            | None                            |
| **Expected Status** | `200 OK`                        |
| **Expected Body**   | `Healthy` or JSON health report |
| **Timeout**         | 10 seconds                      |
| **Dependencies**    | None                            |

### Health Check Endpoints

| #      | Test Name                         | URL                                   | Downstream Service              |
| ------ | --------------------------------- | ------------------------------------- | ------------------------------- |
| T06_01 | `HealthCheck_AIProcessing`        | `GET /api/ai/health`                  | aiprocessingservice:8080        |
| T06_02 | `HealthCheck_VehiclesSale`        | `GET /api/inventory/health`           | vehiclessaleservice:8080        |
| T06_03 | `HealthCheck_Errors`              | `GET /api/errors/health`              | errorservice:8080               |
| T06_04 | `HealthCheck_Auth`                | `GET /api/auth/health`                | authservice:8080                |
| T06_05 | `HealthCheck_Notifications`       | `GET /api/notifications/health`       | notificationservice:8080        |
| T06_06 | `HealthCheck_Products`            | `GET /api/products/health`            | vehiclessaleservice:8080        |
| T06_07 | `HealthCheck_Media`               | `GET /api/media/health`               | mediaservice:8080               |
| T06_08 | `HealthCheck_Billing`             | `GET /api/billing/health`             | billingservice:8080             |
| T06_09 | `HealthCheck_Users`               | `GET /api/users/health`               | userservice:8080                |
| T06_10 | `HealthCheck_Dealers`             | `GET /api/dealers/health`             | adminservice:8080               |
| T06_11 | `HealthCheck_Sellers`             | `GET /api/sellers/health`             | userservice:8080                |
| T06_12 | `HealthCheck_Roles`               | `GET /api/roles/health`               | roleservice:8080                |
| T06_13 | `HealthCheck_Admin`               | `GET /api/admin/health`               | adminservice:8080               |
| T06_14 | `HealthCheck_CRM`                 | `GET /api/crm/health`                 | crmservice:8080                 |
| T06_15 | `HealthCheck_Reports`             | `GET /api/reports/health`             | reportsservice:8080             |
| T06_16 | `HealthCheck_Contact`             | `GET /api/contactrequests/health`     | contactservice:8080             |
| T06_17 | `HealthCheck_Comparison`          | `GET /api/vehiclecomparisons/health`  | comparisonservice:8080          |
| T06_18 | `HealthCheck_VehicleIntelligence` | `GET /api/vehicleintelligence/health` | vehicleintelligenceservice:8080 |
| T06_19 | `HealthCheck_Reviews`             | `GET /api/reviews/health`             | reviewservice:8080              |
| T06_20 | `HealthCheck_Chatbot`             | `GET /api/chatbot/health`             | chatbotservice:8080             |
| T06_21 | `HealthCheck_KYC`                 | `GET /api/kyc/health`                 | kycservice:8080                 |
| T06_22 | `HealthCheck_Audit`               | `GET /api/audit/health`               | auditservice:8080               |

### Additional Health Endpoints (Agent Services)

| #      | Test Name                  | URL                            | Downstream Service |
| ------ | -------------------------- | ------------------------------ | ------------------ |
| T06_23 | `HealthCheck_SearchAgent`  | `GET /api/search-agent/health` | searchagent:8080   |
| T06_24 | `HealthCheck_RecoAgent`    | `GET /api/reco-agent/health`   | recoagent:8080     |
| T06_25 | `HealthCheck_SupportAgent` | `GET /api/support/health`      | supportagent:8080  |
| T06_26 | `HealthCheck_Vehicle360`   | `GET /api/vehicle360/health`   | mediaservice:8080  |

**Implementation as `[Theory]` with `[MemberData]`:**

```csharp
[Theory]
[MemberData(nameof(HealthEndpoints))]
public async Task HealthCheck_ServiceResponds200(string name, string url)
{
    var response = await _client.GetAsync(url);
    response.StatusCode.Should().Be(HttpStatusCode.OK,
        because: $"Service {name} health endpoint should be reachable");
}
```

---

## 8. Flow 7: Gateway Routing Flow

> Validates that Gateway correctly routes to all downstream services

### T07_01 — Gateway Returns 404 for Unknown Route

| Field               | Value                                           |
| ------------------- | ----------------------------------------------- |
| **Description**     | A non-existent route should return 404, not 500 |
| **Method / URL**    | `GET /api/nonexistent-service/test`             |
| **Auth**            | None                                            |
| **Expected Status** | `404 Not Found`                                 |
| **Dependencies**    | None                                            |

---

### T07_02 — Authenticated Route Without Token Returns 401

| Field               | Value                                                                             |
| ------------------- | --------------------------------------------------------------------------------- |
| **Description**     | Routes with `AuthenticationProviderKey: "Bearer"` reject unauthenticated requests |
| **Method / URL**    | `GET /api/users`                                                                  |
| **Auth**            | None                                                                              |
| **Expected Status** | `401 Unauthorized`                                                                |
| **Dependencies**    | None                                                                              |

---

### T07_03 — Authenticated Route With Valid Token Returns 200

| Field               | Value                                 |
| ------------------- | ------------------------------------- |
| **Description**     | Authenticated route accepts valid JWT |
| **Method / URL**    | `GET /api/auth/me`                    |
| **Auth**            | `Bearer {{accessToken}}`              |
| **Expected Status** | `200 OK`                              |
| **Dependencies**    | T01_05                                |

---

### T07_04 — Public Route Returns Data Without Auth

| Field               | Value                                              |
| ------------------- | -------------------------------------------------- |
| **Description**     | Public vehicle search works without authentication |
| **Method / URL**    | `GET /api/vehicles?page=1&pageSize=1`              |
| **Auth**            | None                                               |
| **Expected Status** | `200 OK`                                           |
| **Dependencies**    | None                                               |

---

### T07_05 — Public Catalog Route (Cached, 5min)

| Field               | Value                                                    |
| ------------------- | -------------------------------------------------------- |
| **Description**     | Catalog (makes/models) is publicly accessible with cache |
| **Method / URL**    | `GET /api/catalog`                                       |
| **Auth**            | None                                                     |
| **Expected Status** | `200 OK`                                                 |
| **Dependencies**    | None                                                     |

---

### T07_06 — Public Categories Route

| Field               | Value                                      |
| ------------------- | ------------------------------------------ |
| **Description**     | Vehicle categories are publicly accessible |
| **Method / URL**    | `GET /api/categories`                      |
| **Auth**            | None                                       |
| **Expected Status** | `200 OK`                                   |
| **Dependencies**    | None                                       |

---

### T07_07 — Admin Route Without Admin Role Returns 403

| Field               | Value                                                          |
| ------------------- | -------------------------------------------------------------- |
| **Description**     | Admin endpoints require Admin/SuperAdmin/Compliance role claim |
| **Method / URL**    | `GET /api/admin/dashboard`                                     |
| **Auth**            | `Bearer {{buyerAccessToken}}` (no admin role)                  |
| **Expected Status** | `403 Forbidden`                                                |
| **Dependencies**    | T01_05                                                         |

---

### T07_08 — QoS Timeout Respected

| Field               | Value                                                                            |
| ------------------- | -------------------------------------------------------------------------------- |
| **Description**     | Gateway returns timeout error if downstream is unreachable within timeout window |
| **Method / URL**    | Any authenticated endpoint                                                       |
| **Auth**            | `Bearer {{accessToken}}`                                                         |
| **Expected Status** | Response received within 30s                                                     |
| **Dependencies**    | T01_05                                                                           |

**Assertions:** Response time < 30000ms (QoS TimeoutValue)

---

## 9. Flow 8: Error Logging Flow

> **Full lifecycle:** Log Error → Query Errors → Get by ID → Get Stats → Get Services

### T08_01 — Log Error

| Field               | Value                                                      |
| ------------------- | ---------------------------------------------------------- |
| **Description**     | Submit an error log entry to ErrorService                  |
| **Method / URL**    | `POST /api/errors`                                         |
| **Auth**            | `Bearer {{adminOrServiceToken}}` (ErrorServiceRead policy) |
| **Expected Status** | `200 OK`                                                   |
| **Dependencies**    | Valid auth token with ErrorServiceRead policy              |

**Request Body:**

```json
{
  "serviceName": "E2ETestSuite",
  "message": "E2E Test Error — this is an automated test, please ignore",
  "stackTrace": "at E2ETests.ErrorLogging.T08_01()\n   at System.Runtime...",
  "level": "Error",
  "exceptionType": "System.InvalidOperationException",
  "correlationId": "e2e-test-{{uuid}}",
  "environment": "production-e2e-test",
  "additionalData": {
    "testSuite": "E2E",
    "runId": "{{uuid}}"
  }
}
```

**Expected Response Shape:**

```json
{
  "success": true,
  "data": {
    "id": "guid",
    "serviceName": "E2ETestSuite",
    "createdAt": "datetime"
  }
}
```

**Store:** `errorId`

---

### T08_02 — Query Errors with Filter

| Field               | Value                                                         |
| ------------------- | ------------------------------------------------------------- |
| **Description**     | Query errors filtered by service name                         |
| **Method / URL**    | `GET /api/errors?serviceName=E2ETestSuite&page=1&pageSize=10` |
| **Auth**            | `Bearer {{adminOrServiceToken}}`                              |
| **Expected Status** | `200 OK`                                                      |
| **Dependencies**    | T08_01                                                        |

**Assertions:**

- Response contains the error from T08_01
- Pagination fields present

---

### T08_03 — Get Error by ID

| Field               | Value                                    |
| ------------------- | ---------------------------------------- |
| **Description**     | Fetch a specific error entry by its GUID |
| **Method / URL**    | `GET /api/errors/{errorId}`              |
| **Auth**            | `Bearer {{adminOrServiceToken}}`         |
| **Expected Status** | `200 OK`                                 |
| **Dependencies**    | T08_01                                   |

**Assertions:**

- `data.id` matches `errorId`
- `data.serviceName` is "E2ETestSuite"

---

### T08_04 — Get Error Stats

| Field               | Value                            |
| ------------------- | -------------------------------- |
| **Description**     | Get aggregate error statistics   |
| **Method / URL**    | `GET /api/errors/stats`          |
| **Auth**            | `Bearer {{adminOrServiceToken}}` |
| **Expected Status** | `200 OK`                         |
| **Dependencies**    | None                             |

**Expected Response Shape:**

```json
{
  "success": true,
  "data": {
    "totalErrors": 0,
    "errorsByService": {...},
    "errorsByLevel": {...}
  }
}
```

---

### T08_05 — Get Service Names

| Field               | Value                                            |
| ------------------- | ------------------------------------------------ |
| **Description**     | Get list of all services that have logged errors |
| **Method / URL**    | `GET /api/errors/services`                       |
| **Auth**            | `Bearer {{adminOrServiceToken}}`                 |
| **Expected Status** | `200 OK`                                         |
| **Dependencies**    | T08_01                                           |

**Assertions:**

- Response contains "E2ETestSuite" in the list

---

### T08_06 — Get Non-Existent Error Returns 404

| Field               | Value                                    |
| ------------------- | ---------------------------------------- |
| **Description**     | Fetching a random GUID should return 404 |
| **Method / URL**    | `GET /api/errors/{random-guid}`          |
| **Auth**            | `Bearer {{adminOrServiceToken}}`         |
| **Expected Status** | `404 Not Found`                          |
| **Dependencies**    | None                                     |

---

## 10. Shared Test State & Execution Order

### 10.1 xUnit Collection for Ordered Execution

```csharp
[CollectionDefinition("E2E Sequential Flow")]
public class SequentialFlowCollection : ICollectionFixture<SharedTestState> { }

public class SharedTestState : IAsyncLifetime
{
    // Auth
    public string BuyerAccessToken { get; set; }
    public string BuyerRefreshToken { get; set; }
    public string BuyerUserId { get; set; }
    public string BuyerEmail { get; set; }

    public string SellerAccessToken { get; set; }
    public string SellerUserId { get; set; }

    public string DealerAccessToken { get; set; }
    public string DealerUserId { get; set; }

    // Entities
    public string VehicleId { get; set; }
    public string VehicleSlug { get; set; }
    public string ContactRequestId { get; set; }
    public string DealerId { get; set; }
    public string SubscriptionId { get; set; }
    public string ErrorId { get; set; }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => Task.CompletedTask; // Cleanup test data
}
```

### 10.2 Execution Order

```
Phase 0: Health Checks (T06_xx) — parallel, no dependencies
Phase 1: Gateway Routing (T07_xx) — parallel, no dependencies
Phase 2: Auth Flow (T01_xx) — sequential, produces tokens
Phase 3: Vehicle Flow (T02_xx) — sequential, needs seller token
Phase 4: Contact Flow (T03_xx) — sequential, needs buyer+seller+vehicle
Phase 5: Dealer Flow (T04_xx) — sequential, needs dealer token
Phase 6: Billing Flow (T05_xx) — sequential, needs dealer
Phase 7: Error Logging (T08_xx) — sequential, needs admin token
Phase 8: Cleanup — delete test vehicles, contacts, users
```

### 10.3 Dependency Graph

```
T01_01 (Register Buyer) ──┬── T01_05 (Login) ──┬── T01_08 (Get Profile)
                           │                     ├── T01_10 (Refresh Token)
                           │                     ├── T02_13 (Add Favorite)
                           │                     ├── T03_01 (Create Contact)
                           │                     └── T07_03 (Authed Route)
                           │
T01_14 (Register Seller) ──┬── T02_04 (Create Vehicle)
                            ├── T02_08 (Upload Image)
                            ├── T02_09 (Publish)
                            └── T03_05 (Reply to Contact)

T04_01 (Register Dealer) ──── T04_02 (Create Dealer) ──┬── T04_06 (Set DealerId)
                                                         ├── T05_04 (Create Stripe Customer)
                                                         └── T05_06 (Create Subscription)
```

---

## 11. Implementation Notes

### 11.1 NuGet Dependencies

```xml
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.*" />
<PackageReference Include="xunit" Version="2.*" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.*" />
<PackageReference Include="FluentAssertions" Version="7.*" />
<PackageReference Include="Bogus" Version="35.*" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="8.*" />
```

### 11.2 Test Data Conventions

- All test emails use `@test.okla.com` domain
- All test usernames are prefixed with `e2e_`
- All test VINs start with `E2ETEST`
- All test error service names use `E2ETestSuite`
- Test data includes `correlationId` with `e2e-test-` prefix for easy identification and cleanup

### 11.3 Cleanup Strategy

```csharp
public async Task DisposeAsync()
{
    // Delete test vehicle
    if (!string.IsNullOrEmpty(VehicleId))
        await _client.DeleteAsync($"/api/vehicles/{VehicleId}");

    // Delete test contact request
    if (!string.IsNullOrEmpty(ContactRequestId))
        await _client.DeleteAsync($"/api/contactrequests/{ContactRequestId}");

    // Note: User/Dealer cleanup requires admin API or DB access
}
```

### 11.4 Environment-Specific Behavior

| Behavior           | Local/Docker                  | Staging       | Production                            |
| ------------------ | ----------------------------- | ------------- | ------------------------------------- |
| Email verification | Auto-verify via test endpoint | Test mailbox  | **Skip or use pre-verified accounts** |
| Stripe operations  | Test API keys                 | Test API keys | **Skip billing tests**                |
| Error logging      | Full tests                    | Full tests    | **Use dedicated test service name**   |
| Vehicle creation   | Full CRUD                     | Full CRUD     | **Create + immediate delete**         |
| Data cleanup       | Automatic                     | Automatic     | **Mandatory**                         |

### 11.5 CI/CD Integration

```yaml
# .github/workflows/e2e-tests.yml
name: E2E Tests
on:
  schedule:
    - cron: "0 6 * * *" # Daily at 6 AM UTC
  workflow_dispatch:

jobs:
  e2e:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: "8.0.x"
      - name: Run E2E Tests
        env:
          E2E_BASE_URL: ${{ secrets.E2E_BASE_URL }}
          E2E_ADMIN_TOKEN: ${{ secrets.E2E_ADMIN_TOKEN }}
        run: |
          dotnet test backend/E2ETests/E2ETests.csproj \
            --logger "trx;LogFileName=e2e-results.trx" \
            --results-directory ./TestResults
      - uses: actions/upload-artifact@v4
        if: always()
        with:
          name: e2e-test-results
          path: ./TestResults/
```

### 11.6 Postman Collection Export

All tests in this document are designed to be exportable to a Postman Collection v2.1. Key variables:

| Variable                | Description                  |
| ----------------------- | ---------------------------- |
| `{{baseUrl}}`           | Gateway base URL             |
| `{{buyerAccessToken}}`  | JWT from buyer login         |
| `{{sellerAccessToken}}` | JWT from seller login        |
| `{{dealerAccessToken}}` | JWT from dealer login        |
| `{{vehicleId}}`         | Created vehicle GUID         |
| `{{vehicleSlug}}`       | Created vehicle SEO slug     |
| `{{contactRequestId}}`  | Created contact request GUID |
| `{{dealerId}}`          | Created dealer GUID          |
| `{{errorId}}`           | Logged error GUID            |

---

## Summary

| Flow                 | Test Count | Critical Level | Auth Required |
| -------------------- | ---------- | -------------- | ------------- |
| Auth Flow            | 15         | 🔴 Critical    | Mixed         |
| Vehicle Listing Flow | 14         | 🔴 Critical    | Mixed         |
| Contact Flow         | 10         | 🟡 High        | Yes           |
| Dealer Flow          | 10         | 🟡 High        | Yes           |
| Billing Flow         | 10         | 🟡 High        | Mixed         |
| Health Check Flow    | 26         | 🔴 Critical    | No            |
| Gateway Routing Flow | 8          | 🔴 Critical    | Mixed         |
| Error Logging Flow   | 6          | 🟢 Medium      | Yes           |
| **Total**            | **99**     |                |               |

> **Estimated implementation time:** 3–4 days for a senior developer familiar with xUnit and the OKLA codebase.
