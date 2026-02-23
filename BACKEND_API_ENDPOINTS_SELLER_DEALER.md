# Backend API Endpoints: Seller & Dealer Registration Flows

**Documento consolidado de todos los endpoints implementados en los microservicios para los flujos de registro de Seller y Dealer**

---

## 📑 Table of Contents

1. [AUTH Service](#auth-service)
2. [USER Service](#user-service)
3. [KYC Service](#kyc-service)
4. [BILLING Service](#billing-service)
5. [DEALER MANAGEMENT Service](#dealer-management-service)

---

## AUTH SERVICE

**Base URL:** `POST /api/auth*`  
**Rate Limiting:** Enabled (AuthPolicy)  
**Default Auth:** Some endpoints require Bearer token

### POST /api/auth/register

**Description:** Register a new user account  
**Auth:** Not required  
**Rate Limited:** Yes (AuthPolicy)

**Request Body:**

```json
{
  "userName": "string",
  "email": "string",
  "password": "string",
  "firstName": "string",
  "lastName": "string",
  "phone": "string",
  "acceptTerms": boolean,
  "accountType": "seller|buyer|dealer",
  "userIntent": "sell|buy|buy_and_sell|browse"
}
```

**Response (201 Created):**

```json
{
  "id": "guid",
  "email": "string",
  "accessToken": "string",
  "refreshToken": "string",
  "expiresAt": "datetime",
  "requiresTwoFactor": boolean
}
```

**HTTP Status Codes:**

- **201 Created:** User registered successfully
- **400 Bad Request:** Email exists, password doesn't meet requirements, validation errors
- **422 Unprocessable Entity:** Validation failed

---

### POST /api/auth/login

**Description:** Authenticate user and get JWT tokens  
**Auth:** Not required  
**Rate Limited:** Yes (AuthPolicy)

**Request Body:**

```json
{
  "email": "string",
  "password": "string",
  "captchaToken": "string (optional)"
}
```

**Response (200 OK):**

```json
{
  "accessToken": "string",
  "refreshToken": "string",
  "expiresAt": "datetime",
  "requiresTwoFactor": boolean,
  "user": {
    "id": "guid",
    "email": "string",
    "firstName": "string",
    "lastName": "string",
    "accountType": "buyer|seller|dealer|admin"
  }
}
```

**HTTP Status Codes:**

- **200 OK:** Login successful
- **401 Unauthorized:** Invalid credentials, user not found
- **403 Forbidden:** Account locked/disabled
- **422 Unprocessable Entity:** Validation failed

**Security Notes:**

- Tokens returned as HttpOnly cookies (not body)
- Cookie names: `okla_access_token`, `okla_refresh_token`
- Secure flag set in production (HTTPS)
- SameSite=Lax

---

### POST /api/auth/refresh-token

**Description:** Refresh expired access token  
**Auth:** Not required (refresh token from cookie or body)  
**Rate Limited:** Yes (Debug level)

**Request Body:**

```json
{
  "refreshToken": "string (optional - can use from cookie)"
}
```

**Response (200 OK):**

```json
{
  "accessToken": "string",
  "refreshToken": "string",
  "expiresAt": "datetime"
}
```

**HTTP Status Codes:**

- **200 OK:** Token refreshed
- **401 Unauthorized:** Invalid refresh token, token expired
- **400 Bad Request:** Refresh token not provided

---

### POST /api/auth/logout

**Description:** Revoke refresh token and logout  
**Auth:** Required (Bearer token)  
**Rate Limited:** Yes (Info level)

**Request Body:**

```json
{
  "refreshToken": "string (optional - can use from cookie)"
}
```

**Response (200 OK):**

```json
{
  "success": true,
  "message": "Logged out successfully"
}
```

**HTTP Status Codes:**

- **200 OK:** Logout successful
- **400 Bad Request:** Refresh token not provided
- **401 Unauthorized:** Invalid access token

**Side Effects:**

- HttpOnly auth cookies cleared
- Refresh token revoked in database

---

### GET /api/auth/me

**Description:** Get current authenticated user profile  
**Auth:** Required (Bearer token)

**Response (200 OK):**

```json
{
  "id": "guid",
  "email": "string",
  "firstName": "string",
  "lastName": "string",
  "avatarUrl": "string (nullable)",
  "phone": "string",
  "accountType": "buyer|seller|dealer|admin",
  "isVerified": boolean,
  "isEmailVerified": boolean,
  "isPhoneVerified": boolean,
  "createdAt": "datetime",
  "lastLoginAt": "datetime"
}
```

**HTTP Status Codes:**

- **200 OK:** User found
- **401 Unauthorized:** Token invalid/expired
- **404 Not Found:** User not found in database

---

### POST /api/auth/set-dealer-id

**Description:** Associate dealer profile ID with user account (called after dealer creation)  
**Auth:** Required (Bearer token)  
**Security:** Only users with Dealer account type can set dealer ID

**Request Body:**

```json
{
  "dealerId": "guid"
}
```

**Response (200 OK):**

```json
{
  "success": true,
  "message": "Dealer ID updated successfully"
}
```

**HTTP Status Codes:**

- **200 OK:** Dealer ID set
- **400 Bad Request:** Invalid dealer ID format, dealer already set
- **401 Unauthorized:** Not authenticated
- **403 Forbidden:** User doesn't have Dealer account type
- **404 Not Found:** User not found

---

### POST /api/auth/forgot-password

**Description:** Initiate password reset flow  
**Auth:** Not required

**Request Body:**

```json
{
  "email": "string"
}
```

**Response (200 OK):**

```json
{
  "success": true,
  "message": "Password reset email sent",
  "resetTokenExpiry": "datetime"
}
```

**HTTP Status Codes:**

- **200 OK:** Reset email sent (even if user not found, for security)
- **400 Bad Request:** Invalid email format

---

### POST /api/auth/reset-password

**Description:** Complete password reset  
**Auth:** Not required

**Request Body:**

```json
{
  "token": "string",
  "newPassword": "string",
  "confirmPassword": "string"
}
```

**Response (200 OK):**

```json
{
  "success": true,
  "message": "Password reset successfully"
}
```

**HTTP Status Codes:**

- **200 OK:** Password reset successful
- **400 Bad Request:** Invalid token, passwords don't match, token expired
- **422 Unprocessable Entity:** Password doesn't meet requirements

---

### POST /api/auth/verify-email

**Description:** Verify email address with token from email link  
**Auth:** Not required

**Request Body:**

```json
{
  "token": "string"
}
```

**Response (200 OK):**

```json
{
  "success": true,
  "message": "Email verified successfully"
}
```

**HTTP Status Codes:**

- **200 OK:** Email verified
- **400 Bad Request:** Invalid/expired token
- **404 Not Found:** User not found

---

### POST /api/auth/resend-verification

**Description:** Resend email verification code  
**Auth:** Not required

**Request Body:**

```json
{
  "email": "string"
}
```

**Response (200 OK):**

```json
{
  "success": true,
  "message": "Verification email sent"
}
```

**HTTP Status Codes:**

- **200 OK:** Email sent
- **404 Not Found:** User not found

---

### GET /api/auth/oauth/{provider}

**Description:** Initiate OAuth login flow (redirects to OAuth provider)  
**Auth:** Not required  
**Providers:** google, apple

**Response (302 Found):**

- Redirect to OAuth provider authorization URL

**HTTP Status Codes:**

- **302 Found:** Redirect to OAuth provider
- **400 Bad Request:** Invalid provider

---

### POST /api/auth/oauth/{provider}/callback

**Description:** Handle OAuth callback and exchange authorization code for tokens  
**Auth:** Not required  
**Providers:** google, apple

**Request Body:**

```json
{
  "code": "string",
  "redirectUri": "string",
  "state": "string (optional)"
}
```

**Response (200 OK):**

```json
{
  "userId": "guid",
  "accessToken": "string",
  "refreshToken": "string",
  "expiresAt": "datetime",
  "isNewUser": boolean,
  "user": {
    "email": "string",
    "firstName": "string",
    "lastName": "string"
  }
}
```

**HTTP Status Codes:**

- **200 OK:** OAuth authentication successful
- **400 Bad Request:** Invalid provider, missing authorization code

---

### POST /api/auth/password/setup-request

**Description:** Request password setup link for OAuth users  
**Auth:** Required (Bearer token)

**Response (200 OK):**

```json
{
  "success": true,
  "message": "Password setup email sent",
  "expiresAt": "datetime"
}
```

**HTTP Status Codes:**

- **200 OK:** Setup link sent
- **400 Bad Request:** User already has password
- **401 Unauthorized:** Not authenticated

---

### GET /api/auth/password/setup-validate

**Description:** Validate password setup token (check if still valid)  
**Auth:** Not required  
**Query Parameters:** `token=string`

**Response (200 OK):**

```json
{
  "valid": boolean,
  "expiresAt": "datetime",
  "userId": "guid"
}
```

**HTTP Status Codes:**

- **200 OK:** Token is valid
- **400 Bad Request:** Token not provided or expired

---

### POST /api/auth/password/setup-complete

**Description:** Complete password setup for OAuth user  
**Auth:** Not required

**Request Body:**

```json
{
  "token": "string",
  "newPassword": "string",
  "confirmPassword": "string"
}
```

**Response (200 OK):**

```json
{
  "success": true,
  "message": "Password set successfully"
}
```

**HTTP Status Codes:**

- **200 OK:** Password set
- **400 Bad Request:** Invalid token, password doesn't meet requirements

---

### POST /api/auth/revoked-device/request-code

**Description:** Request verification code for revoked device login  
**Auth:** Not required

**Request Body:**

```json
{
  "userId": "guid",
  "email": "string",
  "deviceFingerprint": "string"
}
```

**Response (200 OK):**

```json
{
  "requiresVerification": boolean,
  "message": "string",
  "verificationToken": "string (nullable)",
  "codeExpiresAt": "datetime (nullable)"
}
```

**HTTP Status Codes:**

- **200 OK:** Code requested
- **400 Bad Request:** Invalid input

---

### POST /api/auth/revoked-device/verify

**Description:** Verify code and complete login from revoked device  
**Auth:** Not required

**Request Body:**

```json
{
  "verificationToken": "string",
  "code": "string"
}
```

**Response (200 OK):**

```json
{
  "success": boolean,
  "message": "string",
  "deviceCleared": boolean,
  "remainingAttempts": "int (nullable)"
}
```

**HTTP Status Codes:**

- **200 OK:** Device verified
- **400 Bad Request:** Invalid code/token

---

### POST /api/phoneverification/send

**Description:** Send phone verification code via SMS  
**Auth:** Required (Bearer token)

**Request Body:**

```json
{
  "phoneNumber": "string"
}
```

**Response (200 OK):**

```json
{
  "success": true,
  "message": "Verification code sent",
  "expiresAt": "datetime",
  "phoneNumber": "string"
}
```

**HTTP Status Codes:**

- **200 OK:** Code sent
- **400 Bad Request:** Invalid phone number
- **401 Unauthorized:** Not authenticated

---

### POST /api/phoneverification/verify

**Description:** Verify phone number with received code  
**Auth:** Required (Bearer token)

**Request Body:**

```json
{
  "phoneNumber": "string",
  "verificationCode": "string"
}
```

**Response (200 OK):**

```json
{
  "success": true,
  "isVerified": boolean,
  "message": "Phone verified successfully",
  "phoneNumber": "string"
}
```

**HTTP Status Codes:**

- **200 OK:** Phone verified
- **400 Bad Request:** Invalid code
- **401 Unauthorized:** Not authenticated

---

### POST /api/twofactor/enable

**Description:** Enable two-factor authentication  
**Auth:** Required (Bearer token)

**Request Body:**

```json
{
  "type": "sms|totp|email"
}
```

**Response (200 OK):**

```json
{
  "success": true,
  "type": "string",
  "secret": "string (nullable - for TOTP)",
  "qrCode": "string (nullable - base64 image for TOTP)"
}
```

**HTTP Status Codes:**

- **200 OK:** 2FA enabled
- **400 Bad Request:** Invalid type
- **401 Unauthorized:** Not authenticated

---

### POST /api/twofactor/verify

**Description:** Verify 2FA setup with code  
**Auth:** Required (Bearer token)

**Request Body:**

```json
{
  "code": "string",
  "type": "sms|totp|email"
}
```

**Response (200 OK):**

```json
{
  "success": true,
  "message": "2FA verified",
  "recoveryCodes": ["string"]
}
```

**HTTP Status Codes:**

- **200 OK:** 2FA verified
- **400 Bad Request:** Invalid code
- **401 Unauthorized:** Not authenticated

---

### POST /api/twofactor/disable

**Description:** Disable two-factor authentication  
**Auth:** Required (Bearer token)

**Request Body:**

```json
{
  "code": "string (optional - current 2FA code for verification)"
}
```

**Response (200 OK):**

```json
{
  "success": true,
  "message": "2FA disabled"
}
```

**HTTP Status Codes:**

- **200 OK:** 2FA disabled
- **400 Bad Request:** Invalid code
- **401 Unauthorized:** Not authenticated

---

## USER SERVICE

**Base URL:** `/api/{dealers|sellers|onboarding}*`  
**Default Auth:** Varies per endpoint

### POST /api/sellers/convert

**Description:** Convert buyer account to seller account  
**Auth:** Required (Bearer token)  
**Idempotency:** Supports `Idempotency-Key` header

**Request Body:**

```json
{
  "description": "string",
  "shopName": "string",
  "phone": "string",
  "address": "string",
  "city": "string",
  "province": "string",
  "country": "string"
}
```

**Response (201 Created / 200 OK):**

```json
{
  "conversionId": "guid",
  "sellerId": "guid",
  "userId": "guid",
  "status": "string",
  "pendingVerification": boolean,
  "source": "new|existing"
}
```

**HTTP Status Codes:**

- **201 Created:** New seller conversion created
- **200 OK:** Idempotent - existing conversion
- **202 Accepted:** Pending KYC verification
- **400 Bad Request:** Conversion not allowed (Dealer/DealerEmployee), validation errors
- **401 Unauthorized:** Not authenticated
- **404 Not Found:** User not found

**Business Rules:**

- Buyers can convert to Sellers
- Dealers/DealerEmployees CANNOT convert (returns 400 with errorCode: CONVERSION_NOT_ALLOWED)
- If idempotent request, returns existing conversion
- Pending KYC verification returns 202

---

### POST /api/sellers

**Description:** Create new seller profile  
**Auth:** Not required (public endpoint for initial creation)

**Request Body:**

```json
{
  "userId": "guid",
  "shopName": "string",
  "description": "string",
  "phone": "string",
  "address": "string",
  "city": "string",
  "province": "string",
  "country": "string"
}
```

**Response (201 Created):**

```json
{
  "id": "guid",
  "userId": "guid",
  "shopName": "string",
  "description": "string",
  "phone": "string",
  "address": "string",
  "city": "string",
  "province": "string",
  "country": "string",
  "createdAt": "datetime",
  "isActive": boolean
}
```

**HTTP Status Codes:**

- **201 Created:** Seller profile created
- **400 Bad Request:** Validation failed

---

### GET /api/sellers/{sellerId}

**Description:** Get seller profile by ID  
**Auth:** Not required

**Response (200 OK):**

```json
{
  "id": "guid",
  "userId": "guid",
  "shopName": "string",
  "description": "string",
  "phone": "string",
  "address": "string",
  "city": "string",
  "province": "string",
  "country": "string",
  "rating": "number",
  "totalReviews": "int",
  "responseTime": "string",
  "isActive": boolean,
  "createdAt": "datetime"
}
```

**HTTP Status Codes:**

- **200 OK:** Seller found
- **404 Not Found:** Seller not found

---

### GET /api/sellers/user/{userId}

**Description:** Get seller profile by user ID  
**Auth:** Not required

**Response (200 OK):** Same as GET /api/sellers/{sellerId}

**HTTP Status Codes:**

- **200 OK:** Seller found
- **404 Not Found:** Seller not found

---

### PUT /api/sellers/{sellerId}

**Description:** Update seller profile  
**Auth:** Required (Bearer token)

**Request Body:** Same as POST /api/sellers

**Response (200 OK):** Updated seller profile

**HTTP Status Codes:**

- **200 OK:** Profile updated
- **400 Bad Request:** Validation failed
- **401 Unauthorized:** Not authenticated
- **404 Not Found:** Seller not found

---

### POST /api/sellers/{sellerId}/verify

**Description:** Verify/reject seller (admin only)  
**Auth:** Required + Admin role

**Request Body:**

```json
{
  "isVerified": boolean,
  "notes": "string (optional)"
}
```

**Response (200 OK):**

```json
{
  "success": true,
  "message": "Seller verified successfully|Seller verification revoked"
}
```

**HTTP Status Codes:**

- **200 OK:** Verification status updated
- **400 Bad Request:** Invalid request
- **401 Unauthorized:** Not authenticated
- **403 Forbidden:** Not admin
- **404 Not Found:** Seller not found

---

### GET /api/sellers/{sellerId}/stats

**Description:** Get seller statistics  
**Auth:** Not required

**Response (200 OK):**

```json
{
  "totalListings": "int",
  "activeListings": "int",
  "totalViews": "int",
  "averageRating": "number",
  "totalReviews": "int",
  "responseRate": "number",
  "averageResponseTime": "string",
  "joinedDate": "datetime"
}
```

**HTTP Status Codes:**

- **200 OK:** Stats retrieved
- **404 Not Found:** Seller not found

---

### GET /api/sellers/{sellerId}/listings

**Description:** Get seller's vehicle listings (paginated)  
**Auth:** Not required

**Query Parameters:**

- `page` (default: 1)
- `pageSize` (default: 12)
- `status` (optional: active, sold, delisted)

**Response (200 OK):**

```json
{
  "listings": [
    {
      "id": "guid",
      "title": "string",
      "price": "number",
      "currency": "string",
      "mainImageUrl": "string",
      "status": "string",
      "views": "int",
      "createdAt": "datetime"
    }
  ],
  "page": "int",
  "pageSize": "int",
  "totalCount": "int"
}
```

**HTTP Status Codes:**

- **200 OK:** Listings retrieved
- **404 Not Found:** Seller not found

---

### GET /api/sellers/{sellerId}/reviews

**Description:** Get seller reviews (paginated)  
**Auth:** Not required

**Query Parameters:**

- `page` (default: 1)
- `pageSize` (default: 10)
- `rating` (optional: 1-5)

**Response (200 OK):**

```json
{
  "reviews": [
    {
      "id": "guid",
      "rating": "int",
      "comment": "string",
      "reviewer": "string",
      "createdAt": "datetime"
    }
  ],
  "page": "int",
  "pageSize": "int",
  "totalCount": "int",
  "averageRating": "number"
}
```

**HTTP Status Codes:**

- **200 OK:** Reviews retrieved
- **404 Not Found:** Seller not found

---

### GET /api/sellers/{sellerId}/profile

**Description:** Get seller public profile  
**Auth:** Not required

**Response (200 OK):**

```json
{
  "sellerId": "guid",
  "shopName": "string",
  "description": "string",
  "phone": "string",
  "address": "string",
  "rating": "number",
  "reviews": "int",
  "responseTime": "string",
  "activeListings": "int",
  "badges": ["string"],
  "joinedDate": "datetime"
}
```

**HTTP Status Codes:**

- **200 OK:** Profile retrieved
- **404 Not Found:** Seller not found/inactive

---

### GET /api/onboarding/status

**Description:** Get current user's onboarding status  
**Auth:** Required (Bearer token)

**Response (200 OK):**

```json
{
  "userId": "guid",
  "completedSteps": ["string"],
  "isComplete": boolean,
  "progress": "int",
  "nextStep": "string (nullable)"
}
```

**HTTP Status Codes:**

- **200 OK:** Status retrieved
- **401 Unauthorized:** Not authenticated

---

### POST /api/onboarding/complete-step

**Description:** Mark onboarding step as completed  
**Auth:** Required (Bearer token)

**Request Body:**

```json
{
  "step": "string"
}
```

**Response (200 OK):** Updated onboarding status

**HTTP Status Codes:**

- **200 OK:** Step marked complete
- **400 Bad Request:** Invalid step
- **401 Unauthorized:** Not authenticated
- **404 Not Found:** Onboarding not found

---

### POST /api/onboarding/complete

**Description:** Mark entire onboarding as complete  
**Auth:** Required (Bearer token)

**Response (200 OK):** Updated onboarding status

**HTTP Status Codes:**

- **200 OK:** Onboarding completed
- **401 Unauthorized:** Not authenticated
- **404 Not Found:** Onboarding not found

---

### POST /api/onboarding/skip

**Description:** Skip onboarding  
**Auth:** Required (Bearer token)

**Response (200 OK):** Updated onboarding status

**HTTP Status Codes:**

- **200 OK:** Onboarding skipped
- **401 Unauthorized:** Not authenticated
- **404 Not Found:** Onboarding not found

---

### POST /api/dealers

**Description:** Register new dealer (company)  
**Auth:** Required (Bearer token)  
**Business Rule:** User with Dealer account type only

**Request Body:**

```json
{
  "businessName": "string",
  "legalName": "string (optional)",
  "rnc": "string (optional)",
  "dealerType": "Independent|Chain",
  "email": "string",
  "phone": "string",
  "mobilePhone": "string (optional)",
  "website": "string (optional)",
  "address": "string",
  "city": "string",
  "province": "string",
  "establishedDate": "date (optional)",
  "employeeCount": "int (optional)",
  "description": "string (optional)"
}
```

**Response (201 Created):**

```json
{
  "id": "guid",
  "businessName": "string",
  "email": "string",
  "phone": "string",
  "verificationStatus": "Pending",
  "status": "Active",
  "createdAt": "datetime"
}
```

**HTTP Status Codes:**

- **201 Created:** Dealer registered (pending admin approval)
- **400 Bad Request:** Already a dealer (errorCode: ALREADY_DEALER), validation failed
- **401 Unauthorized:** Not authenticated
- **403 Forbidden:** User doesn't have Dealer account type

**Business Rules:**

- User must have Dealer account type
- Dealer status starts as "Pending" (requires admin verification)
- Cannot have multiple dealer profiles per user
- Feature flag check: `Features:DealerRegistration`

---

### GET /api/dealers

**Description:** List all dealers (public, with filters)  
**Auth:** Not required

**Query Parameters:**

- `page` (default: 1)
- `pageSize` (default: 20)
- `status` (optional: Active, Inactive, Pending)
- `verificationStatus` (optional: Verified, Rejected, Pending)
- `searchTerm` (optional: search by business name)

**Response (200 OK):**

```json
{
  "dealers": [
    {
      "id": "guid",
      "businessName": "string",
      "city": "string",
      "phone": "string",
      "verificationStatus": "string",
      "rating": "number",
      "activeListings": "int"
    }
  ],
  "page": "int",
  "pageSize": "int",
  "totalCount": "int"
}
```

**HTTP Status Codes:**

- **200 OK:** Dealers listed

---

### GET /api/dealers/{dealerId}

**Description:** Get dealer profile by ID  
**Auth:** Not required (public)

**Response (200 OK):**

```json
{
  "id": "guid",
  "businessName": "string",
  "legalName": "string",
  "rnc": "string",
  "email": "string",
  "phone": "string",
  "website": "string",
  "address": "string",
  "city": "string",
  "province": "string",
  "description": "string",
  "verificationStatus": "string",
  "rating": "number",
  "activeListings": "int",
  "createdAt": "datetime"
}
```

**HTTP Status Codes:**

- **200 OK:** Dealer found
- **404 Not Found:** Dealer not found

---

### GET /api/dealers/user/{userId}

**Description:** Get dealer profile by user ID  
**Auth:** Not required

**Response (200 OK):** Same as GET /api/dealers/{dealerId}

**HTTP Status Codes:**

- **200 OK:** Dealer found
- **404 Not Found:** Dealer not found

---

### PUT /api/dealers/{dealerId}

**Description:** Update dealer profile  
**Auth:** Required (Bearer token)

**Request Body:** Same fields as POST /api/dealers

**Response (200 OK):** Updated dealer profile

**HTTP Status Codes:**

- **200 OK:** Dealer updated
- **400 Bad Request:** Validation failed
- **401 Unauthorized:** Not authenticated
- **403 Forbidden:** Not owner
- **404 Not Found:** Dealer not found

---

### POST /api/dealers/{dealerId}/verify

**Description:** Verify dealer (admin only)  
**Auth:** Required + Admin role

**Request Body:**

```json
{
  "approved": boolean,
  "rejectionReason": "string (required if approved=false)"
}
```

**Response (200 OK):**

```json
{
  "message": "Dealer verified successfully|Dealer rejected",
  "dealerId": "guid"
}
```

**HTTP Status Codes:**

- **200 OK:** Verification status updated
- **401 Unauthorized:** Not authenticated
- **403 Forbidden:** Not admin
- **404 Not Found:** Dealer not found

---

### GET /api/dealers/{dealerId}/stats

**Description:** Get dealer statistics  
**Auth:** Required (Bearer token)

**Response (200 OK):**

```json
{
  "totalListings": "int",
  "activeListings": "int",
  "totalViews": "int",
  "viewsThisMonth": "int",
  "totalInquiries": "int",
  "inquiriesThisMonth": "int",
  "pendingInquiries": "int",
  "responseRate": "number",
  "averageResponseTimeMinutes": "int",
  "totalRevenue": "number",
  "revenueThisMonth": "number"
}
```

**HTTP Status Codes:**

- **200 OK:** Stats retrieved
- **401 Unauthorized:** Not authenticated
- **404 Not Found:** Dealer not found

---

### GET /api/dealers/public/{slug}

**Description:** Get public dealer profile by slug  
**Auth:** Not required

**Response (200 OK):**

```json
{
  "dealerId": "guid",
  "slug": "string",
  "businessName": "string",
  "description": "string",
  "logo": "string",
  "coverImage": "string",
  "phone": "string",
  "email": "string",
  "website": "string",
  "rating": "number",
  "reviews": "int",
  "verifiedBadge": boolean,
  "activeListings": "int"
}
```

**HTTP Status Codes:**

- **200 OK:** Profile found
- **404 Not Found:** Dealer not found

---

### GET /api/dealers/trusted

**Description:** Get all trusted dealers  
**Auth:** Not required

**Response (200 OK):**

```json
[
  {
    "dealerId": "guid",
    "businessName": "string",
    "rating": "number",
    "activeListings": "int"
  }
]
```

**HTTP Status Codes:**

- **200 OK:** Trusted dealers listed

---

### GET /api/dealers/{dealerId}/profile/completion

**Description:** Get dealer profile completion percentage  
**Auth:** Required (Bearer token)

**Response (200 OK):**

```json
{
  "completionPercentage": "int",
  "missingFields": ["string"],
  "suggestions": ["string"]
}
```

**HTTP Status Codes:**

- **200 OK:** Completion data retrieved
- **401 Unauthorized:** Not authenticated
- **404 Not Found:** Dealer not found

---

### GET /api/admin/dealers

**Description:** List pending dealers (admin only)  
**Auth:** Required + Admin role

**Query Parameters:**

- `status` (optional: Pending, Verified, Rejected)
- `page` (default: 1)
- `pageSize` (default: 20)

**Response (200 OK):**

```json
{
  "dealers": [
    {
      "id": "guid",
      "businessName": "string",
      "email": "string",
      "verificationStatus": "string",
      "submittedAt": "datetime"
    }
  ],
  "page": "int",
  "totalCount": "int"
}
```

**HTTP Status Codes:**

- **200 OK:** Dealers listed
- **401 Unauthorized:** Not authenticated
- **403 Forbidden:** Not admin

---

### GET /api/admin/dealers/pending

**Description:** Get only pending dealer registrations (admin only)  
**Auth:** Required + Admin role

**Response (200 OK):** Same format as GET /api/admin/dealers

---

### GET /api/admin/dealers/{dealerId}

**Description:** Get dealer details (admin view, all fields)  
**Auth:** Required + Admin role

**Response (200 OK):**

```json
{
  "id": "guid",
  "businessName": "string",
  "legalName": "string",
  "rnc": "string",
  "email": "string",
  "phone": "string",
  "website": "string",
  "address": "string",
  "city": "string",
  "province": "string",
  "verificationStatus": "string",
  "submittedAt": "datetime",
  "submittedBy": "guid",
  "verifiedBy": "guid (nullable)",
  "verifiedAt": "datetime (nullable)",
  "rejectionReason": "string (nullable)"
}
```

**HTTP Status Codes:**

- **200 OK:** Dealer found
- **401 Unauthorized:** Not authenticated
- **403 Forbidden:** Not admin
- **404 Not Found:** Dealer not found

---

### POST /api/admin/dealers/{dealerId}/approve

**Description:** Approve pending dealer registration (admin only)  
**Auth:** Required + Admin role

**Request Body:**

```json
{
  "notes": "string (optional)"
}
```

**Response (200 OK):**

```json
{
  "success": true,
  "message": "Dealer approved",
  "dealerId": "guid"
}
```

**HTTP Status Codes:**

- **200 OK:** Dealer approved
- **401 Unauthorized:** Not authenticated
- **403 Forbidden:** Not admin
- **404 Not Found:** Dealer not found

**Side Effects:**

- Dealer verificationStatus updated to "Verified"
- Email sent to dealer with approval notification
- DealerApprovedEvent published

---

### POST /api/admin/dealers/{dealerId}/reject

**Description:** Reject pending dealer registration (admin only)  
**Auth:** Required + Admin role

**Request Body:**

```json
{
  "rejectionReason": "string (required)"
}
```

**Response (200 OK):**

```json
{
  "success": true,
  "message": "Dealer rejected",
  "dealerId": "guid"
}
```

**HTTP Status Codes:**

- **200 OK:** Dealer rejected
- **400 Bad Request:** Rejection reason required
- **401 Unauthorized:** Not authenticated
- **403 Forbidden:** Not admin
- **404 Not Found:** Dealer not found

**Side Effects:**

- Dealer verificationStatus updated to "Rejected"
- Email sent to dealer with rejection reason
- DealerRejectedEvent published

---

## KYC SERVICE

**Base URL:** `/api/kyc*` or `/api/kyc-profiles*`  
**Default Auth:** Varies per endpoint  
**Compliance:** Ley 155-17 Prevención de Lavado de Activos

### GET /api/kyc-profiles/settings

**Description:** Get current KYC verification settings (admin/compliance only)  
**Auth:** Required + AdminOrCompliance role

**Response (200 OK):**

```json
{
  "maxVerificationAttempts": "int",
  "verificationTimeoutMinutes": "int",
  "documentExpirationDays": "int",
  "highConfidenceThreshold": "int",
  "faceMatchThreshold": "int",
  "requireLivenessCheck": "boolean",
  "autoApproveHighConfidence": "boolean"
}
```

**HTTP Status Codes:**

- **200 OK:** Settings retrieved
- **401 Unauthorized:** Not authenticated
- **403 Forbidden:** Not admin/compliance

---

### GET /api/kyc-profiles

**Description:** List KYC profiles with pagination and filters (admin/compliance only)  
**Auth:** Required + AdminOrCompliance role

**Query Parameters:**

- `page` (default: 1)
- `pageSize` (default: 20, max: 100)
- `status` (optional: Pending, UnderReview, Approved, Rejected)
- `riskLevel` (optional: Low, Medium, High, Critical)
- `isPEP` (optional: true/false)

**Response (200 OK):**

```json
{
  "profiles": [
    {
      "id": "guid",
      "userId": "guid",
      "documentNumber": "string",
      "status": "string",
      "riskLevel": "string",
      "isPEP": "boolean",
      "submittedAt": "datetime",
      "approvedAt": "datetime (nullable)"
    }
  ],
  "page": "int",
  "pageSize": "int",
  "totalCount": "int"
}
```

**HTTP Status Codes:**

- **200 OK:** Profiles listed
- **401 Unauthorized:** Not authenticated
- **403 Forbidden:** Not admin/compliance

---

### GET /api/kyc-profiles/{id}

**Description:** Get KYC profile by ID  
**Auth:** Required (Bearer token)  
**Security:** User can only access own profile, admin can access any

**Response (200 OK):**

```json
{
  "id": "guid",
  "userId": "guid",
  "firstName": "string",
  "lastName": "string",
  "documentType": "string",
  "documentNumber": "string",
  "dateOfBirth": "date",
  "gender": "M|F|O",
  "address": "string",
  "city": "string",
  "province": "string",
  "country": "string",
  "status": "Pending|UnderReview|Approved|Rejected",
  "riskLevel": "Low|Medium|High|Critical",
  "isPEP": "boolean",
  "approvedBy": "guid (nullable)",
  "rejectionReason": "string (nullable)",
  "submittedAt": "datetime",
  "approvedAt": "datetime (nullable)",
  "expiresAt": "datetime (nullable)"
}
```

**HTTP Status Codes:**

- **200 OK:** Profile retrieved
- **401 Unauthorized:** Not authenticated
- **403 Forbidden:** No access (IDOR protection)
- **404 Not Found:** Profile not found

---

### GET /api/kyc-profiles/user/{userId}

**Description:** Get KYC profile by user ID  
**Auth:** Not required

**Response (200 OK):** Same as GET /api/kyc-profiles/{id}

**HTTP Status Codes:**

- **200 OK:** Profile found
- **404 Not Found:** Profile not found

---

### GET /api/kyc-profiles/document/{documentNumber}

**Description:** Get KYC profile by document number (admin/compliance only)  
**Auth:** Required + AdminOrCompliance role

**Response (200 OK):** Same as GET /api/kyc-profiles/{id}

**HTTP Status Codes:**

- **200 OK:** Profile found
- **401 Unauthorized:** Not authenticated
- **403 Forbidden:** Not admin/compliance
- **404 Not Found:** Profile not found

---

### POST /api/kyc-profiles

**Description:** Create new KYC profile  
**Auth:** Required (Bearer token)

**Request Body:**

```json
{
  "userId": "guid",
  "firstName": "string",
  "lastName": "string",
  "documentType": "cedula|passport|license",
  "documentNumber": "string",
  "dateOfBirth": "date",
  "gender": "M|F|O",
  "address": "string",
  "city": "string",
  "province": "string",
  "country": "string",
  "nationality": "string"
}
```

**Response (201 Created):**

```json
{
  "id": "guid",
  "userId": "guid",
  "status": "Pending",
  "createdAt": "datetime"
}
```

**HTTP Status Codes:**

- **201 Created:** Profile created
- **400 Bad Request:** Validation failed
- **401 Unauthorized:** Not authenticated

---

### PUT /api/kyc-profiles/{id}

**Description:** Update KYC profile  
**Auth:** Required (Bearer token)

**Request Body:** Same as POST /api/kyc-profiles

**Response (200 OK):** Updated profile

**HTTP Status Codes:**

- **200 OK:** Profile updated
- **400 Bad Request:** Validation failed
- **401 Unauthorized:** Not authenticated
- **404 Not Found:** Profile not found

---

### POST /api/kyc-profiles/{id}/submit

**Description:** Submit KYC profile for review  
**Auth:** Required (Bearer token)

**Response (200 OK):**

```json
{
  "id": "guid",
  "status": "UnderReview",
  "submittedAt": "datetime"
}
```

**HTTP Status Codes:**

- **200 OK:** Profile submitted
- **400 Bad Request:** Invalid status transition
- **401 Unauthorized:** Not authenticated
- **404 Not Found:** Profile not found

---

### POST /api/kyc-profiles/{id}/approve

**Description:** Approve KYC profile (admin/compliance only)  
**Auth:** Required + AdminOrCompliance role

**Request Body:**

```json
{
  "id": "guid",
  "approvedBy": "guid"
}
```

**Response (200 OK):**

```json
{
  "id": "guid",
  "status": "Approved",
  "approvedAt": "datetime"
}
```

**HTTP Status Codes:**

- **200 OK:** Profile approved
- **400 Bad Request:** Invalid status transition
- **401 Unauthorized:** Not authenticated
- **403 Forbidden:** Not admin/compliance
- **404 Not Found:** Profile not found

---

### POST /api/kyc-profiles/{id}/reject

**Description:** Reject KYC profile (admin/compliance only)  
**Auth:** Required + AdminOrCompliance role

**Request Body:**

```json
{
  "id": "guid",
  "rejectionReason": "string"
}
```

**Response (200 OK):**

```json
{
  "id": "guid",
  "status": "Rejected",
  "rejectionReason": "string"
}
```

**HTTP Status Codes:**

- **200 OK:** Profile rejected
- **400 Bad Request:** Invalid status transition
- **401 Unauthorized:** Not authenticated
- **403 Forbidden:** Not admin/compliance
- **404 Not Found:** Profile not found

---

### GET /api/kyc-profiles/pending

**Description:** Get pending KYC profiles (admin/compliance only)  
**Auth:** Required + AdminOrCompliance role

**Query Parameters:**

- `page` (default: 1)
- `pageSize` (default: 20, max: 100)

**Response (200 OK):** Same format as GET /api/kyc-profiles

---

### GET /api/kyc-profiles/expiring

**Description:** Get expiring KYC profiles (admin/compliance only)  
**Auth:** Required + AdminOrCompliance role

**Query Parameters:**

- `daysUntilExpiry` (default: 30)
- `page` (default: 1)
- `pageSize` (default: 20)

**Response (200 OK):** Same format as GET /api/kyc-profiles

---

### GET /api/kyc-profiles/statistics

**Description:** Get KYC statistics (admin/compliance only)  
**Auth:** Required + AdminOrCompliance role

**Response (200 OK):**

```json
{
  "totalProfiles": "int",
  "approved": "int",
  "pending": "int",
  "rejected": "int",
  "averageApprovalTime": "int (minutes)",
  "approvalRate": "number (percentage)",
  "pepCount": "int"
}
```

**HTTP Status Codes:**

- **200 OK:** Statistics retrieved
- **401 Unauthorized:** Not authenticated
- **403 Forbidden:** Not admin/compliance

---

### GET /api/kyc/profiles/{profileId}/documents

**Description:** Get documents for KYC profile  
**Auth:** Required (Bearer token)

**Response (200 OK):**

```json
[
  {
    "id": "guid",
    "profileId": "guid",
    "documentType": "identity|residence|proof_of_income",
    "fileName": "string",
    "uploadedAt": "datetime",
    "status": "Pending|Verified|Rejected",
    "url": "string"
  }
]
```

**HTTP Status Codes:**

- **200 OK:** Documents retrieved
- **401 Unauthorized:** Not authenticated
- **403 Forbidden:** No access (IDOR protection)
- **404 Not Found:** Profile not found

---

### POST /api/kyc/profiles/{profileId}/documents

**Description:** Upload KYC document  
**Auth:** Required (Bearer token)

**Request Body (multipart/form-data):**

- `file`: document file
- `documentType`: string
- `kycProfileId`: guid

**Response (201 Created):**

```json
{
  "id": "guid",
  "profileId": "guid",
  "documentType": "string",
  "fileName": "string",
  "uploadedAt": "datetime",
  "status": "Pending"
}
```

**HTTP Status Codes:**

- **201 Created:** Document uploaded
- **400 Bad Request:** Invalid file or profile ID mismatch
- **401 Unauthorized:** Not authenticated
- **413 Payload Too Large:** File too large (max: 10MB typically)

---

### POST /api/kyc/documents/{documentId}/verify

**Description:** Verify KYC document (admin/compliance only)  
**Auth:** Required + AdminOrCompliance role

**Request Body:**

```json
{
  "id": "guid",
  "approved": "boolean"
}
```

**Response (200 OK):**

```json
{
  "id": "guid",
  "status": "Verified|Rejected",
  "verifiedAt": "datetime"
}
```

**HTTP Status Codes:**

- **200 OK:** Document verified
- **400 Bad Request:** Invalid ID or approval status
- **401 Unauthorized:** Not authenticated
- **403 Forbidden:** Not admin/compliance
- **404 Not Found:** Document not found

---

### GET /api/kyc/documents/{documentId}/url

**Description:** Get pre-signed URL for KYC document (valid 1 hour)  
**Auth:** Required (Bearer token)  
**Security:** IDOR protection - user can only access own documents, admin can access any

**Response (200 OK):**

```json
{
  "url": "string (pre-signed S3/storage URL)",
  "expiresAt": "datetime",
  "documentId": "guid"
}
```

**HTTP Status Codes:**

- **200 OK:** URL retrieved
- **401 Unauthorized:** Not authenticated
- **403 Forbidden:** No access (IDOR protection)
- **404 Not Found:** Document not found
- **500 Internal Server Error:** Failed to generate pre-signed URL

---

### GET /api/kyc/profiles/{profileId}/verifications

**Description:** Get verifications for KYC profile  
**Auth:** Required (Bearer token)

**Response (200 OK):**

```json
[
  {
    "id": "guid",
    "profileId": "guid",
    "verificationType": "identity|address|income|pep|background",
    "passed": "boolean",
    "result": "string",
    "performedAt": "datetime",
    "performedBy": "guid (nullable)"
  }
]
```

**HTTP Status Codes:**

- **200 OK:** Verifications retrieved
- **401 Unauthorized:** Not authenticated
- **403 Forbidden:** No access
- **404 Not Found:** Profile not found

---

### POST /api/kyc/profiles/{profileId}/verifications

**Description:** Create verification record (admin/compliance only)  
**Auth:** Required + AdminOrCompliance role

**Request Body:**

```json
{
  "kycProfileId": "guid",
  "verificationType": "string",
  "passed": "boolean",
  "result": "string"
}
```

**Response (200 OK):**

```json
{
  "id": "guid",
  "profileId": "guid",
  "verificationType": "string",
  "passed": "boolean",
  "performedAt": "datetime"
}
```

**HTTP Status Codes:**

- **200 OK:** Verification created
- **400 Bad Request:** Invalid profile or type
- **401 Unauthorized:** Not authenticated
- **403 Forbidden:** Not admin/compliance
- **404 Not Found:** Profile not found

---

### GET /api/kyc/profiles/{profileId}/risk-assessments

**Description:** Get risk assessments for KYC profile (admin/compliance only)  
**Auth:** Required + AdminOrCompliance role

**Response (200 OK):**

```json
[
  {
    "id": "guid",
    "profileId": "guid",
    "riskLevel": "Low|Medium|High|Critical",
    "previousLevel": "string",
    "factors": ["string"],
    "assessedAt": "datetime",
    "assessedBy": "guid"
  }
]
```

**HTTP Status Codes:**

- **200 OK:** Assessments retrieved
- **401 Unauthorized:** Not authenticated
- **403 Forbidden:** Not admin/compliance
- **404 Not Found:** Profile not found

---

### POST /api/kyc/profiles/{profileId}/risk-assessments

**Description:** Create risk assessment (admin/compliance only)  
**Auth:** Required + AdminOrCompliance role

**Request Body:**

```json
{
  "kycProfileId": "guid",
  "riskLevel": "Low|Medium|High|Critical",
  "factors": ["string"],
  "notes": "string (optional)"
}
```

**Response (200 OK):**

```json
{
  "id": "guid",
  "profileId": "guid",
  "riskLevel": "string",
  "previousLevel": "string",
  "assessedAt": "datetime"
}
```

**HTTP Status Codes:**

- **200 OK:** Assessment created
- **400 Bad Request:** Invalid profile or risk level
- **401 Unauthorized:** Not authenticated
- **403 Forbidden:** Not admin/compliance
- **404 Not Found:** Profile not found

---

## BILLING SERVICE

**Base URL:** `/api/{dealer-billing|subscriptions|payments}*`  
**Default Auth:** Required for most endpoints (except public plans)

### GET /api/dealer-billing/dashboard/{dealerId}

**Description:** Get complete dealer billing dashboard  
**Auth:** Required (Bearer token)

**Response (200 OK):**

```json
{
  "summary": {
    "currentPlan": "string",
    "monthlySpend": "number",
    "nextBillingAmount": "number",
    "nextBillingDate": "datetime"
  },
  "plans": [
    {
      "id": "string",
      "name": "string",
      "description": "string",
      "monthlyPrice": "number",
      "yearlyPrice": "number"
    }
  ],
  "usage": {
    "currentListings": "int",
    "maxListings": "string",
    "currentUsers": "int",
    "maxUsers": "string",
    "storageUsed": "string",
    "storageLimit": "string"
  },
  "stats": {
    "currentPlan": "string",
    "monthlySpend": "number",
    "yearlySpend": "number",
    "outstandingBalance": "number",
    "totalPaid": "number",
    "invoiceCount": "int"
  }
}
```

**HTTP Status Codes:**

- **200 OK:** Dashboard data retrieved
- **400 Bad Request:** Invalid dealer ID
- **401 Unauthorized:** Not authenticated
- **500 Internal Server Error:** Error fetching dashboard

---

### GET /api/dealer-billing/subscription

**Description:** Get dealer's current subscription  
**Auth:** Required (Bearer token)  
**Header:** X-Dealer-Id

**Response (200 OK):**

```json
{
  "id": "guid",
  "dealerId": "guid",
  "plan": "free|basic|professional|enterprise",
  "status": "active|suspended|cancelled",
  "cycle": "monthly|quarterly|yearly",
  "pricePerCycle": "number",
  "currency": "string",
  "startDate": "datetime",
  "nextBillingDate": "datetime (nullable)",
  "maxUsers": "int",
  "maxVehicles": "int",
  "features": {
    "listings": "int",
    "users": "int",
    "storage": "string",
    "analytics": "boolean",
    "api": "boolean"
  }
}
```

**HTTP Status Codes:**

- **200 OK:** Subscription retrieved (or default free subscription if none)
- **400 Bad Request:** Invalid dealer ID
- **401 Unauthorized:** Not authenticated

---

### GET /api/dealer-billing/invoices

**Description:** Get dealer invoices  
**Auth:** Required (Bearer token)  
**Header:** X-Dealer-Id

**Response (200 OK):**

```json
[
  {
    "id": "guid",
    "dealerId": "guid",
    "invoiceNumber": "string",
    "subscriptionId": "guid (nullable)",
    "status": "paid|unpaid|overdue|cancelled",
    "subtotal": "number",
    "taxAmount": "number",
    "totalAmount": "number",
    "paidAmount": "number",
    "currency": "string",
    "issueDate": "datetime",
    "dueDate": "datetime",
    "paidDate": "datetime (nullable)",
    "pdfUrl": "string (nullable)"
  }
]
```

**HTTP Status Codes:**

- **200 OK:** Invoices retrieved
- **401 Unauthorized:** Not authenticated

---

### GET /api/dealer-billing/payments

**Description:** Get dealer payment history  
**Auth:** Required (Bearer token)  
**Header:** X-Dealer-Id

**Response (200 OK):**

```json
[
  {
    "id": "guid",
    "dealerId": "guid",
    "subscriptionId": "guid (nullable)",
    "invoiceId": "guid (nullable)",
    "amount": "number",
    "currency": "string",
    "status": "succeeded|failed|pending",
    "method": "credit_card|bank_transfer|stripe",
    "description": "string (nullable)",
    "receiptUrl": "string (nullable)",
    "refundedAmount": "number",
    "createdAt": "datetime",
    "processedAt": "datetime (nullable)",
    "cardLast4": "string",
    "cardBrand": "string"
  }
]
```

**HTTP Status Codes:**

- **200 OK:** Payments retrieved
- **401 Unauthorized:** Not authenticated

---

### GET /api/dealer-billing/usage

**Description:** Get dealer usage metrics  
**Auth:** Required (Bearer token)  
**Header:** X-Dealer-Id

**Response (200 OK):**

```json
{
  "currentListings": "int",
  "maxListings": "string",
  "currentUsers": "int",
  "maxUsers": "string",
  "storageUsed": "string",
  "storageLimit": "string",
  "apiCalls": "int",
  "apiLimit": "string"
}
```

**HTTP Status Codes:**

- **200 OK:** Usage retrieved
- **401 Unauthorized:** Not authenticated

---

### GET /api/dealer-billing/stats

**Description:** Get dealer billing statistics  
**Auth:** Required (Bearer token)  
**Header:** X-Dealer-Id

**Response (200 OK):**

```json
{
  "currentPlan": "string",
  "monthlySpend": "number",
  "yearlySpend": "number",
  "outstandingBalance": "number",
  "nextBillingAmount": "number",
  "nextBillingDate": "datetime",
  "totalPaid": "number",
  "invoiceCount": "int"
}
```

**HTTP Status Codes:**

- **200 OK:** Stats retrieved
- **401 Unauthorized:** Not authenticated

---

### GET /api/dealer-billing/plans

**Description:** Get available subscription plans (public)  
**Auth:** Not required  
**Header:** X-Dealer-Id (optional)

**Response (200 OK):**

```json
[
  {
    "id": "free|basic|professional|enterprise",
    "name": "string",
    "description": "string",
    "prices": {
      "monthly": "number",
      "quarterly": "number",
      "yearly": "number"
    },
    "features": {
      "listings": "int",
      "users": "int",
      "storage": "string",
      "analytics": "boolean",
      "api": "boolean",
      "customBranding": "boolean"
    },
    "popular": "boolean",
    "enterprise": "boolean"
  }
]
```

**HTTP Status Codes:**

- **200 OK:** Plans retrieved

---

### GET /api/dealer-billing/payment-methods

**Description:** Get dealer payment methods  
**Auth:** Required (Bearer token)  
**Header:** X-Dealer-Id

**Response (200 OK):**

```json
[
  {
    "id": "string",
    "type": "card|bank_account",
    "isDefault": "boolean",
    "card": {
      "brand": "Visa|Mastercard|AmEx",
      "last4": "string",
      "expMonth": "int",
      "expYear": "int"
    },
    "bankAccount": null,
    "createdAt": "datetime"
  }
]
```

**HTTP Status Codes:**

- **200 OK:** Payment methods retrieved
- **401 Unauthorized:** Not authenticated

---

### POST /api/dealer-billing/payment-methods

**Description:** Add new payment method (card or bank account)  
**Auth:** Required (Bearer token)  
**Header:** X-Dealer-Id

**Request Body:**

```json
{
  "type": "card|bank_account",
  "cardToken": "string (for card)",
  "bankToken": "string (for bank)",
  "setAsDefault": "boolean"
}
```

**Response (200 OK):**

```json
{
  "id": "string",
  "type": "string",
  "isDefault": "boolean",
  "card": {},
  "createdAt": "datetime"
}
```

**HTTP Status Codes:**

- **200 OK:** Payment method added
- **400 Bad Request:** Invalid payment data
- **401 Unauthorized:** Not authenticated

---

### PUT /api/dealer-billing/payment-methods/{paymentMethodId}/default

**Description:** Set payment method as default  
**Auth:** Required (Bearer token)  
**Header:** X-Dealer-Id

**Response (200 OK):**

```json
{
  "success": true,
  "message": "Default payment method updated"
}
```

**HTTP Status Codes:**

- **200 OK:** Default updated
- **401 Unauthorized:** Not authenticated
- **404 Not Found:** Payment method not found

---

### DELETE /api/dealer-billing/payment-methods/{paymentMethodId}

**Description:** Remove payment method  
**Auth:** Required (Bearer token)  
**Header:** X-Dealer-Id

**Response (200 OK):**

```json
{
  "success": true,
  "message": "Payment method removed"
}
```

**HTTP Status Codes:**

- **200 OK:** Payment method removed
- **401 Unauthorized:** Not authenticated
- **404 Not Found:** Payment method not found

---

### GET /api/subscriptions

**Description:** Get all subscriptions (admin only)  
**Auth:** Required (Bearer token)

**Response (200 OK):**

```json
[
  {
    "id": "guid",
    "dealerId": "guid",
    "plan": "string",
    "status": "string",
    "cycle": "string",
    "pricePerCycle": "number",
    "startDate": "datetime",
    "nextBillingDate": "datetime (nullable)"
  }
]
```

**HTTP Status Codes:**

- **200 OK:** Subscriptions listed
- **401 Unauthorized:** Not authenticated
- **403 Forbidden:** Not admin

---

### GET /api/subscriptions/{id}

**Description:** Get subscription by ID  
**Auth:** Required (Bearer token)

**Response (200 OK):** Single subscription object

**HTTP Status Codes:**

- **200 OK:** Subscription found
- **401 Unauthorized:** Not authenticated
- **404 Not Found:** Subscription not found

---

### GET /api/subscriptions/dealer/{dealerId}

**Description:** Get subscription by dealer ID  
**Auth:** Required (Bearer token)

**Response (200 OK):** Single subscription object

**HTTP Status Codes:**

- **200 OK:** Subscription found
- **401 Unauthorized:** Not authenticated
- **404 Not Found:** Subscription not found

---

### POST /api/subscriptions

**Description:** Create new subscription  
**Auth:** Required (Bearer token)  
**Header:** X-Dealer-Id

**Request Body:**

```json
{
  "plan": "free|basic|professional|enterprise",
  "cycle": "monthly|quarterly|yearly"
}
```

**Response (201 Created):**

```json
{
  "id": "guid",
  "dealerId": "guid",
  "plan": "string",
  "status": "active",
  "cycle": "string",
  "startDate": "datetime"
}
```

**HTTP Status Codes:**

- **201 Created:** Subscription created
- **400 Bad Request:** Invalid plan/cycle, dealer already has subscription
- **401 Unauthorized:** Not authenticated
- **409 Conflict:** Dealer already has active subscription

---

## DEALER MANAGEMENT SERVICE

**Base URL:** `/api/dealers*`  
**Default Auth:** Varies per endpoint

### GET /api/dealers

**Description:** List all dealers with filters (public)  
**Auth:** Not required

**Query Parameters:**

- `page` (default: 1)
- `pageSize` (default: 20)
- `status` (optional)
- `verificationStatus` (optional)
- `searchTerm` (optional)

**Response (200 OK):**

```json
{
  "dealers": [
    {
      "id": "guid",
      "businessName": "string",
      "city": "string",
      "verificationStatus": "string",
      "rating": "number",
      "activeListings": "int"
    }
  ],
  "totalCount": "int"
}
```

**HTTP Status Codes:**

- **200 OK:** Dealers listed

---

### GET /api/dealers/{id}

**Description:** Get dealer profile by ID (public)  
**Auth:** Not required

**Response (200 OK):**

```json
{
  "id": "guid",
  "businessName": "string",
  "email": "string",
  "phone": "string",
  "address": "string",
  "city": "string",
  "province": "string",
  "verificationStatus": "string",
  "rating": "number",
  "activeListings": "int",
  "currentActiveListings": "int",
  "createdAt": "datetime"
}
```

**HTTP Status Codes:**

- **200 OK:** Dealer found
- **404 Not Found:** Dealer not found

---

### GET /api/dealers/me

**Description:** Get current authenticated dealer profile  
**Auth:** Required (Bearer token)

**Response (200 OK):** Same as GET /api/dealers/{id}

**HTTP Status Codes:**

- **200 OK:** Dealer found
- **401 Unauthorized:** Not authenticated
- **404 Not Found:** No dealer profile found for user

---

### GET /api/dealers/user/{userId}

**Description:** Get dealer by user ID (public)  
**Auth:** Not required

**Response (200 OK):** Same as GET /api/dealers/{id}

**HTTP Status Codes:**

- **200 OK:** Dealer found
- **404 Not Found:** Dealer not found

---

### POST /api/dealers

**Description:** Create new dealer account  
**Auth:** Required (Bearer token)

**Request Body:**

```json
{
  "businessName": "string",
  "legalName": "string",
  "rnc": "string",
  "type": "Independent|Chain",
  "email": "string",
  "phone": "string",
  "website": "string",
  "address": "string",
  "city": "string",
  "province": "string",
  "establishedDate": "date",
  "employeeCount": "int",
  "description": "string"
}
```

**Response (201 Created):**

```json
{
  "id": "guid",
  "businessName": "string",
  "email": "string",
  "phone": "string",
  "createdAt": "datetime"
}
```

**HTTP Status Codes:**

- **201 Created:** Dealer created
- **400 Bad Request:** Validation failed
- **401 Unauthorized:** Not authenticated

---

### PUT /api/dealers/{id}

**Description:** Update dealer information  
**Auth:** Required (Bearer token)

**Request Body:** Same as POST /api/dealers

**Response (200 OK):** Updated dealer profile

**HTTP Status Codes:**

- **200 OK:** Dealer updated
- **400 Bad Request:** Validation failed
- **401 Unauthorized:** Not authenticated
- **404 Not Found:** Dealer not found

---

### POST /api/dealers/{id}/verify

**Description:** Verify dealer (admin only)  
**Auth:** Required + Admin role

**Request Body:**

```json
{
  "approved": "boolean",
  "rejectionReason": "string (required if approved=false)"
}
```

**Response (200 OK):**

```json
{
  "message": "Dealer verified successfully|Dealer rejected",
  "dealerId": "guid"
}
```

**HTTP Status Codes:**

- **200 OK:** Verification status updated
- **401 Unauthorized:** Not authenticated
- **403 Forbidden:** Not admin
- **404 Not Found:** Dealer not found

---

### GET /api/dealers/{id}/stats

**Description:** Get dealer statistics  
**Auth:** Required (Bearer token)

**Response (200 OK):**

```json
{
  "totalListings": "int",
  "activeListings": "int",
  "totalViews": "int",
  "viewsThisMonth": "int",
  "totalInquiries": "int",
  "inquiriesThisMonth": "int",
  "pendingInquiries": "int",
  "responseRate": "number",
  "averageResponseTimeMinutes": "int",
  "totalRevenue": "number",
  "revenueThisMonth": "number"
}
```

**HTTP Status Codes:**

- **200 OK:** Stats retrieved
- **401 Unauthorized:** Not authenticated
- **404 Not Found:** Dealer not found

---

### GET /api/dealers/public/{slug}

**Description:** Get public dealer profile by slug  
**Auth:** Not required

**Response (200 OK):**

```json
{
  "dealerId": "guid",
  "slug": "string",
  "businessName": "string",
  "description": "string",
  "phone": "string",
  "website": "string",
  "rating": "number",
  "reviews": "int",
  "activeListings": "int"
}
```

**HTTP Status Codes:**

- **200 OK:** Profile found
- **404 Not Found:** Dealer not found

---

### GET /api/dealers/trusted

**Description:** Get all trusted/verified dealers  
**Auth:** Not required

**Response (200 OK):**

```json
[
  {
    "dealerId": "guid",
    "businessName": "string",
    "rating": "number",
    "activeListings": "int"
  }
]
```

**HTTP Status Codes:**

- **200 OK:** Trusted dealers listed

---

### PUT /api/dealers/{id}/profile

**Description:** Update dealer public profile  
**Auth:** Required (Bearer token)

**Request Body:**

```json
{
  "shopDescription": "string",
  "logoUrl": "string",
  "coverImageUrl": "string",
  "website": "string"
}
```

**Response (200 OK):**

```json
{
  "dealerId": "guid",
  "slug": "string",
  "businessName": "string",
  "shopDescription": "string"
}
```

**HTTP Status Codes:**

- **200 OK:** Profile updated
- **401 Unauthorized:** Not authenticated
- **404 Not Found:** Dealer not found

---

### GET /api/dealers/{id}/profile/completion

**Description:** Get dealer profile completion percentage  
**Auth:** Required (Bearer token)

**Response (200 OK):**

```json
{
  "completionPercentage": "int",
  "missingFields": ["string"],
  "suggestions": ["string"]
}
```

**HTTP Status Codes:**

- **200 OK:** Completion data retrieved
- **401 Unauthorized:** Not authenticated
- **404 Not Found:** Dealer not found

---

## 📋 Summary Table

| Service               | Count            | Main Endpoints                                                                      |
| --------------------- | ---------------- | ----------------------------------------------------------------------------------- |
| **AUTH**              | 19               | register, login, refresh-token, logout, 2fa, phone-verification, oauth              |
| **USER (Sellers)**    | 8                | convert, create, get, update, verify, stats, listings, reviews                      |
| **USER (Dealers)**    | 9                | register, get, update, verify, stats, public-profile, trusted, completion           |
| **USER (Onboarding)** | 4                | status, complete-step, complete, skip                                               |
| **KYC**               | 15               | create-profile, submit, approve, reject, documents, verifications, risk-assessments |
| **BILLING**           | 11               | dashboard, subscription, invoices, payments, usage, stats, plans, payment-methods   |
| **DEALER MANAGEMENT** | 10               | list, get, create, update, verify, stats, public-profile, profile-completion        |
| **TOTAL**             | **76 endpoints** |                                                                                     |

---

## 🔐 Security Notes

- **All endpoints return RFC 7807 ProblemDetails on error**
- **JWT tokens stored in HttpOnly cookies** (CWE-922 mitigation)
- **Rate limiting enabled on sensitive endpoints** (AuthPolicy)
- **All string inputs validated for SQLi/XSS**
- **IDOR protection on personal data endpoints**
- **Admin endpoints require role-based authorization**
- **Dealer/Seller endpoints require owner verification**

---

## ⚠️ Field Mismatches Between Frontend & Backend

**Currently identified issues:**

1. **DealerType vs Type**: Frontend may send `dealerType`, backend expects `type` in some endpoints
2. **UserId Claim Extraction**: Backend checks multiple claim types (`NameIdentifier`, `sub`, `userId`) - ensure frontend sets consistent claim
3. **DealerId in JWT**: After dealer creation, `/api/auth/set-dealer-id` must be called to update JWT with dealerId claim
4. **Bearer Token vs Cookie**: Auth can accept either header token or HttpOnly cookie refresh token
5. **Idempotency-Key Header**: Seller conversion endpoint supports this for safe retries - frontend should use it

---

**Document Generated:** February 23, 2026  
**Backend Version:** .NET 8 Microservices  
**Framework:** ASP.NET Core 8, MediatR, Clean Architecture
