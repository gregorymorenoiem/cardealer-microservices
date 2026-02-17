# 🚀 OKLA CarDealer Microservices - Complete API Reference

**Generated:** January 20, 2026  
**API Gateway:** https://api.okla.com.do  
**Environment:** Production (Digital Ocean Kubernetes)

---

## 📋 Table of Contents

1. [Gateway Configuration Overview](#gateway-configuration-overview)
2. [AuthService API](#1-authservice-api)
3. [UserService API](#2-userservice-api)
4. [VehiclesSaleService API](#3-vehiclessaleservice-api)
5. [MediaService API](#4-mediaservice-api)
6. [BillingService API](#5-billingservice-api)
7. [ContactService API](#6-contactservice-api)
8. [NotificationService API](#7-notificationservice-api)
9. [DealerManagementService API](#8-dealermanagementservice-api)
10. [SearchService API](#9-searchservice-api)
11. [ErrorService API](#10-errorservice-api)
12. [Database Schemas](#database-schemas)
13. [Inter-Service Communication](#inter-service-communication)

---

## Gateway Configuration Overview

All API requests go through the **Ocelot API Gateway**. The gateway handles:

- Request routing to downstream services
- JWT authentication (Bearer token)
- Rate limiting and QoS
- CORS headers

### Base URLs

| Environment     | Base URL                  |
| --------------- | ------------------------- |
| **Production**  | `https://api.okla.com.do` |
| **Development** | `http://localhost:18443`  |

### Gateway Route Patterns

| Upstream Pattern             | Downstream Service         | Port |
| ---------------------------- | -------------------------- | ---- |
| `/api/auth/*`                | authservice                | 8080 |
| `/api/users/*`               | userservice                | 8080 |
| `/api/vehicles/*`            | vehiclessaleservice        | 8080 |
| `/api/products/*`            | vehiclessaleservice        | 8080 |
| `/api/catalog/*`             | vehiclessaleservice        | 8080 |
| `/api/categories/*`          | vehiclessaleservice        | 8080 |
| `/api/homepagesections/*`    | vehiclessaleservice        | 8080 |
| `/api/media/*`               | mediaservice               | 8080 |
| `/api/upload/*`              | mediaservice               | 8080 |
| `/api/billing/*`             | billingservice             | 8080 |
| `/api/notifications/*`       | notificationservice        | 8080 |
| `/api/templates/*`           | notificationservice        | 8080 |
| `/api/contactrequests/*`     | contactservice             | 8080 |
| `/api/dealers/*`             | dealermanagementservice    | 8080 |
| `/api/subscriptions/*`       | dealermanagementservice    | 8080 |
| `/api/sellers/*`             | userservice                | 8080 |
| `/api/roles/*`               | roleservice                | 8080 |
| `/api/errors/*`              | errorservice               | 8080 |
| `/api/admin/*`               | adminservice               | 8080 |
| `/api/crm/*`                 | crmservice                 | 8080 |
| `/api/reports/*`             | reportsservice             | 8080 |
| `/api/reviews/*`             | reviewservice              | 8080 |
| `/api/recommendations/*`     | recommendationservice      | 8080 |
| `/api/chatbot/*`             | chatbotservice             | 8080 |
| `/api/vehiclecomparisons/*`  | comparisonservice          | 8080 |
| `/api/vehicleintelligence/*` | vehicleintelligenceservice | 8080 |
| `/api/userbehavior/*`        | userbehaviorservice        | 8080 |
| `/api/azul-payment/*`        | azulpaymentservice         | 8080 |
| `/api/stripe-payment/*`      | stripepaymentservice       | 8080 |

---

## 1. AuthService API

**Base Route:** `/api/auth`  
**Authentication:** Some endpoints require Bearer token  
**Rate Limiting:** AuthPolicy enabled

### Endpoints

| Method | Route                       | Description                   | Auth Required |
| ------ | --------------------------- | ----------------------------- | ------------- |
| `POST` | `/api/auth/register`        | Register new user             | ❌            |
| `POST` | `/api/auth/login`           | User login (returns JWT)      | ❌            |
| `POST` | `/api/auth/forgot-password` | Request password reset        | ❌            |
| `POST` | `/api/auth/reset-password`  | Reset password with token     | ❌            |
| `POST` | `/api/auth/verify-email`    | Verify email address          | ❌            |
| `POST` | `/api/auth/refresh-token`   | Refresh access token          | ❌            |
| `POST` | `/api/auth/logout`          | Logout (revoke refresh token) | ✅            |
| `GET`  | `/api/auth/health`          | Health check                  | ❌            |

### Request/Response Examples

#### POST /api/auth/register

```json
// Request
{
  "userName": "johndoe",
  "email": "john@example.com",
  "password": "SecurePass123!"
}

// Response
{
  "success": true,
  "data": {
    "userId": "guid",
    "email": "john@example.com",
    "accessToken": "eyJhbG...",
    "refreshToken": "refresh-token-string"
  }
}
```

#### POST /api/auth/login

```json
// Request
{
  "email": "john@example.com",
  "password": "SecurePass123!"
}

// Response
{
  "success": true,
  "data": {
    "accessToken": "eyJhbG...",
    "refreshToken": "refresh-token-string",
    "expiresIn": 86400
  }
}
```

### Database Entities

- **ApplicationUser** (Identity) - User account information
- **RefreshToken** - Refresh tokens for session management
- **VerificationToken** - Email/phone verification tokens
- **TwoFactorAuth** - 2FA configuration

---

## 2. UserService API

**Base Route:** `/api/users`  
**Authentication:** Required (Bearer token)

### Users Endpoints

| Method   | Route                 | Description                | Auth Required |
| -------- | --------------------- | -------------------------- | ------------- |
| `GET`    | `/api/users`          | List users with pagination | ✅            |
| `GET`    | `/api/users/{userId}` | Get user by ID             | ✅            |
| `POST`   | `/api/users`          | Create new user            | ✅            |
| `PUT`    | `/api/users/{userId}` | Update user                | ✅            |
| `DELETE` | `/api/users/{userId}` | Delete user                | ✅            |
| `GET`    | `/api/users/health`   | Health check               | ❌            |

### Sellers Endpoints

| Method | Route                            | Description           | Auth Required |
| ------ | -------------------------------- | --------------------- | ------------- |
| `POST` | `/api/sellers`                   | Create seller profile | ✅            |
| `GET`  | `/api/sellers/{sellerId}`        | Get seller profile    | ✅            |
| `GET`  | `/api/sellers/user/{userId}`     | Get seller by user ID | ✅            |
| `PUT`  | `/api/sellers/{sellerId}`        | Update seller profile | ✅            |
| `POST` | `/api/sellers/{sellerId}/verify` | Verify seller (admin) | ✅ Admin      |
| `GET`  | `/api/sellers/{sellerId}/stats`  | Get seller statistics | ✅            |

### Dealer Employees Endpoints

| Method | Route                                                 | Description           | Auth Required |
| ------ | ----------------------------------------------------- | --------------------- | ------------- |
| `GET`  | `/api/dealers/{dealerId}/employees`                   | List dealer employees | ✅            |
| `GET`  | `/api/dealers/{dealerId}/employees/{employeeId}`      | Get employee details  | ✅            |
| `POST` | `/api/dealers/{dealerId}/employees/invite`            | Invite team member    | ✅            |
| `PUT`  | `/api/dealers/{dealerId}/employees/{employeeId}/role` | Update employee role  | ✅            |

### Database Entities

- **User** - Core user information
- **UserRole** - User-role mappings
- **Role** - Role definitions
- **Dealer** - Dealer accounts
- **SellerProfile** - Individual seller profiles
- **IdentityDocument** - KYC documents
- **DealerEmployee** - Dealer team members
- **PlatformEmployee** - Platform staff
- **DealerEmployeeInvitation** - Team invitations
- **DealerSubscription** - Dealer plan subscriptions
- **SubscriptionHistory** - Subscription changes
- **ModuleAddon** - Optional modules
- **DealerModuleSubscription** - Module subscriptions
- **UserOnboarding** - Onboarding progress

---

## 3. VehiclesSaleService API

**Base Route:** `/api/vehicles`  
**Authentication:** Some endpoints require Bearer token

### Vehicles Endpoints

| Method   | Route                             | Description                  | Auth Required |
| -------- | --------------------------------- | ---------------------------- | ------------- |
| `GET`    | `/api/vehicles`                   | Search vehicles with filters | ❌            |
| `GET`    | `/api/vehicles/{id}`              | Get vehicle by ID            | ❌            |
| `GET`    | `/api/vehicles/vin/{vin}`         | Get vehicle by VIN           | ❌            |
| `GET`    | `/api/vehicles/featured`          | Get featured vehicles        | ❌            |
| `GET`    | `/api/vehicles/dealer/{dealerId}` | Get vehicles by dealer       | ❌            |
| `POST`   | `/api/vehicles`                   | Create vehicle listing       | ✅            |
| `PUT`    | `/api/vehicles/{id}`              | Update vehicle               | ✅            |
| `DELETE` | `/api/vehicles/{id}`              | Delete vehicle (soft)        | ✅            |
| `POST`   | `/api/vehicles/compare`           | Compare multiple vehicles    | ❌            |
| `POST`   | `/api/vehicles/{id}/images`       | Add images to vehicle        | ✅            |
| `POST`   | `/api/vehicles/bulk-images`       | Bulk add images              | ✅            |
| `GET`    | `/api/products/health`            | Health check                 | ❌            |

### Search Parameters

| Parameter        | Type    | Description                     |
| ---------------- | ------- | ------------------------------- |
| `search`         | string  | Search term                     |
| `categoryId`     | Guid    | Filter by category              |
| `minPrice`       | decimal | Minimum price                   |
| `maxPrice`       | decimal | Maximum price                   |
| `make`           | string  | Vehicle make (Toyota, Honda...) |
| `model`          | string  | Vehicle model                   |
| `minYear`        | int     | Minimum year                    |
| `maxYear`        | int     | Maximum year                    |
| `maxMileage`     | int     | Maximum mileage                 |
| `vehicleType`    | enum    | Car, SUV, Truck, etc.           |
| `bodyStyle`      | enum    | Sedan, Coupe, Hatchback...      |
| `fuelType`       | enum    | Gasoline, Diesel, Electric...   |
| `transmission`   | enum    | Automatic, Manual, CVT          |
| `driveType`      | enum    | FWD, RWD, AWD, 4WD              |
| `condition`      | enum    | New, Used, Certified            |
| `exteriorColor`  | string  | Color name                      |
| `state`          | string  | State/Province                  |
| `city`           | string  | City name                       |
| `isCertified`    | bool    | Certified pre-owned             |
| `hasCleanTitle`  | bool    | Clean title vehicles            |
| `page`           | int     | Page number (default: 1)        |
| `pageSize`       | int     | Items per page (default: 20)    |
| `sortBy`         | string  | Sort field                      |
| `sortDescending` | bool    | Sort direction                  |

### Catalog Endpoints

| Method | Route                                                 | Description            | Auth Required |
| ------ | ----------------------------------------------------- | ---------------------- | ------------- |
| `GET`  | `/api/catalog/makes`                                  | Get all makes          | ❌            |
| `GET`  | `/api/catalog/makes/popular`                          | Get popular makes      | ❌            |
| `GET`  | `/api/catalog/makes/search?q={query}`                 | Search makes           | ❌            |
| `GET`  | `/api/catalog/makes/{makeSlug}/models`                | Get models by make     | ❌            |
| `GET`  | `/api/catalog/makes/{makeId}/models/search?q={query}` | Search models          | ❌            |
| `GET`  | `/api/catalog/models/{modelId}/years`                 | Get available years    | ❌            |
| `GET`  | `/api/catalog/models/{modelId}/years/{year}/trims`    | Get trims with specs   | ❌            |
| `GET`  | `/api/catalog/trims/{trimId}`                         | Get trim details       | ❌            |
| `GET`  | `/api/catalog/stats`                                  | Get catalog statistics | ❌            |
| `POST` | `/api/catalog/seed`                                   | Bulk import catalog    | ✅ Admin      |

### Homepage Sections Endpoints

| Method | Route                                 | Description               | Auth Required |
| ------ | ------------------------------------- | ------------------------- | ------------- |
| `GET`  | `/api/homepagesections`               | Get all sections          | ❌            |
| `GET`  | `/api/homepagesections/{slug}`        | Get section with vehicles | ❌            |
| `GET`  | `/api/homepagesections/homepage`      | Get homepage data         | ❌            |
| `POST` | `/api/homepagesections`               | Create section            | ✅ Admin      |
| `PUT`  | `/api/homepagesections/{id}`          | Update section            | ✅ Admin      |
| `POST` | `/api/homepagesections/{id}/vehicles` | Assign vehicles           | ✅ Admin      |

### Favorites Endpoints

| Method   | Route                              | Description           | Auth Required |
| -------- | ---------------------------------- | --------------------- | ------------- |
| `GET`    | `/api/favorites`                   | Get my favorites      | ✅            |
| `GET`    | `/api/favorites/count`             | Get favorites count   | ✅            |
| `GET`    | `/api/favorites/check/{vehicleId}` | Check if favorited    | ✅            |
| `POST`   | `/api/favorites/{vehicleId}`       | Add to favorites      | ✅            |
| `DELETE` | `/api/favorites/{vehicleId}`       | Remove from favorites | ✅            |
| `PUT`    | `/api/favorites/{vehicleId}`       | Update favorite notes | ✅            |

### Database Entities

- **Vehicle** - Vehicle listings (main entity)
- **VehicleImage** - Vehicle images
- **VehicleMake** - Catalog of makes (Toyota, Honda...)
- **VehicleModel** - Catalog of models (Camry, Civic...)
- **VehicleTrim** - Trim levels with specs (LE, SE, XLE...)
- **Category** - Vehicle categories
- **Favorite** - User favorites
- **HomepageSectionConfig** - Homepage section configuration
- **VehicleHomepageSection** - Vehicle-section assignments

---

## 4. MediaService API

**Base Route:** `/api/media`  
**Authentication:** Required (Bearer token with dealerId claim)

### Endpoints

| Method | Route                                  | Description                           | Auth Required |
| ------ | -------------------------------------- | ------------------------------------- | ------------- |
| `POST` | `/api/media/upload/init`               | Initialize upload (get presigned URL) | ✅            |
| `POST` | `/api/media/upload/finalize/{mediaId}` | Finalize upload                       | ✅            |
| `GET`  | `/api/media/{mediaId}`                 | Get media details                     | ✅            |
| `GET`  | `/api/media/health`                    | Health check                          | ❌            |

### Database Entities

- **MediaAsset** - Base media entity (TPH inheritance)
- **MediaVariant** - Image variants (thumbnails, sizes)
- **ImageMedia** - Image-specific properties
- **VideoMedia** - Video-specific properties
- **DocumentMedia** - Document-specific properties

---

## 5. BillingService API

**Base Route:** `/api/billing`  
**Authentication:** Required (Bearer token)

### Plans & Pricing Endpoints

| Method | Route                             | Description         | Auth Required |
| ------ | --------------------------------- | ------------------- | ------------- |
| `GET`  | `/api/billing/plans`              | Get available plans | ❌            |
| `GET`  | `/api/billing/summary/{dealerId}` | Get billing summary | ✅            |

### Customer Endpoints

| Method | Route                                               | Description            | Auth Required |
| ------ | --------------------------------------------------- | ---------------------- | ------------- |
| `POST` | `/api/billing/customers`                            | Create Stripe customer | ✅            |
| `GET`  | `/api/billing/customers/{dealerId}`                 | Get customer info      | ✅            |
| `POST` | `/api/billing/customers/{dealerId}/payment-methods` | Attach payment method  | ✅            |

### Subscription Endpoints

| Method   | Route                                         | Description         | Auth Required |
| -------- | --------------------------------------------- | ------------------- | ------------- |
| `GET`    | `/api/billing/subscriptions/{dealerId}`       | Get subscription    | ✅            |
| `POST`   | `/api/billing/subscriptions`                  | Create subscription | ✅            |
| `PUT`    | `/api/billing/subscriptions/{subscriptionId}` | Update subscription | ✅            |
| `DELETE` | `/api/billing/subscriptions/{subscriptionId}` | Cancel subscription | ✅            |

### Checkout Endpoints

| Method | Route                         | Description                    | Auth Required |
| ------ | ----------------------------- | ------------------------------ | ------------- |
| `POST` | `/api/billing/checkout`       | Create Stripe Checkout session | ✅            |
| `POST` | `/api/billing/billing-portal` | Create billing portal session  | ✅            |

### Subscriptions Controller

| Method   | Route                                          | Description           | Auth Required |
| -------- | ---------------------------------------------- | --------------------- | ------------- |
| `GET`    | `/api/subscriptions`                           | Get all subscriptions | ✅            |
| `GET`    | `/api/subscriptions/{id}`                      | Get by ID             | ✅            |
| `GET`    | `/api/subscriptions/dealer/{dealerId}`         | Get by dealer         | ✅            |
| `GET`    | `/api/subscriptions/status/{status}`           | Get by status         | ✅            |
| `GET`    | `/api/subscriptions/plan/{plan}`               | Get by plan           | ✅            |
| `GET`    | `/api/subscriptions/expiring-trials/{days}`    | Get expiring trials   | ✅            |
| `GET`    | `/api/subscriptions/due-billings`              | Get due billings      | ✅            |
| `POST`   | `/api/subscriptions`                           | Create subscription   | ✅            |
| `POST`   | `/api/subscriptions/{id}/activate`             | Activate              | ✅            |
| `POST`   | `/api/subscriptions/{id}/suspend`              | Suspend               | ✅            |
| `POST`   | `/api/subscriptions/{id}/cancel`               | Cancel                | ✅            |
| `POST`   | `/api/subscriptions/{id}/upgrade`              | Upgrade plan          | ✅            |
| `POST`   | `/api/subscriptions/{id}/change-billing-cycle` | Change cycle          | ✅            |
| `POST`   | `/api/subscriptions/{id}/renew`                | Renew                 | ✅            |
| `DELETE` | `/api/subscriptions/{id}`                      | Delete                | ✅            |

### Payments Endpoints

| Method | Route                                         | Description          | Auth Required |
| ------ | --------------------------------------------- | -------------------- | ------------- |
| `GET`  | `/api/payments`                               | Get all payments     | ✅            |
| `GET`  | `/api/payments/{id}`                          | Get payment by ID    | ✅            |
| `GET`  | `/api/payments/subscription/{subscriptionId}` | Get by subscription  | ✅            |
| `GET`  | `/api/payments/status/{status}`               | Get by status        | ✅            |
| `GET`  | `/api/payments/date-range`                    | Get by date range    | ✅            |
| `GET`  | `/api/payments/pending`                       | Get pending payments | ✅            |
| `GET`  | `/api/payments/failed`                        | Get failed payments  | ✅            |
| `GET`  | `/api/payments/total/{dealerId}`              | Get total by dealer  | ✅            |
| `POST` | `/api/payments`                               | Create payment       | ✅            |
| `POST` | `/api/payments/{id}/process`                  | Mark processing      | ✅            |
| `POST` | `/api/payments/{id}/succeed`                  | Mark succeeded       | ✅            |
| `POST` | `/api/payments/{id}/fail`                     | Mark failed          | ✅            |
| `POST` | `/api/payments/{id}/refund`                   | Refund payment       | ✅            |
| `POST` | `/api/payments/{id}/dispute`                  | Mark disputed        | ✅            |

### Azul Payment Endpoints

| Method | Route                        | Description           | Auth Required |
| ------ | ---------------------------- | --------------------- | ------------- |
| `POST` | `/api/payment/azul/initiate` | Initiate Azul payment | ✅            |

### Database Entities

- **Subscription** - Dealer subscriptions
- **Payment** - Payment records
- **Invoice** - Invoice records
- **StripeCustomer** - Stripe customer mapping
- **EarlyBirdMember** - Early bird benefits
- **AzulTransaction** - Azul payment transactions

---

## 6. ContactService API

**Base Route:** `/api/contactrequests`  
**Authentication:** Required (Bearer token)

### Endpoints

| Method | Route                               | Description                     | Auth Required |
| ------ | ----------------------------------- | ------------------------------- | ------------- |
| `POST` | `/api/contactrequests`              | Create contact request          | ✅            |
| `GET`  | `/api/contactrequests/my-inquiries` | Get my inquiries (buyer)        | ✅            |
| `GET`  | `/api/contactrequests/received`     | Get received inquiries (seller) | ✅            |
| `GET`  | `/api/contactrequests/{id}`         | Get request with messages       | ✅            |
| `POST` | `/api/contactrequests/{id}/reply`   | Reply to request                | ✅            |
| `GET`  | `/api/contactrequests/health`       | Health check                    | ❌            |

### Database Entities

- **ContactRequest** - Contact/inquiry requests
- **ContactMessage** - Messages in a thread

---

## 7. NotificationService API

**Base Route:** `/api/notifications`  
**Authentication:** Required for most endpoints

### Notifications Endpoints

| Method | Route                            | Description             | Auth Required |
| ------ | -------------------------------- | ----------------------- | ------------- |
| `POST` | `/api/notifications/email`       | Send email notification | ✅            |
| `POST` | `/api/notifications/sms`         | Send SMS notification   | ✅            |
| `POST` | `/api/notifications/push`        | Send push notification  | ✅            |
| `GET`  | `/api/notifications/{id}/status` | Get notification status | ✅            |
| `GET`  | `/api/notifications/health`      | Health check            | ❌            |

### Templates Endpoints

| Method | Route                           | Description          | Auth Required |
| ------ | ------------------------------- | -------------------- | ------------- |
| `POST` | `/api/templates`                | Create template      | ✅            |
| `GET`  | `/api/templates/{id}`           | Get template by ID   | ✅            |
| `GET`  | `/api/templates/by-name/{name}` | Get template by name | ✅            |
| `GET`  | `/api/templates`                | Get all templates    | ✅            |
| `PUT`  | `/api/templates/{id}`           | Update template      | ✅            |

### Database Entities

- **Notification** - Notification records
- **NotificationTemplate** - Email/SMS templates
- **NotificationQueue** - Queued notifications
- **NotificationLog** - Delivery logs
- **ScheduledNotification** - Scheduled notifications

---

## 8. DealerManagementService API

**Base Route:** `/api/dealers`  
**Authentication:** Some endpoints require Bearer token

### Dealers Endpoints

| Method | Route                        | Description                 | Auth Required |
| ------ | ---------------------------- | --------------------------- | ------------- |
| `GET`  | `/api/dealers`               | Get all dealers (paginated) | ❌            |
| `GET`  | `/api/dealers/{id}`          | Get dealer by ID            | ❌            |
| `GET`  | `/api/dealers/user/{userId}` | Get dealer by user ID       | ✅            |
| `GET`  | `/api/dealers/public/{slug}` | Get public dealer profile   | ❌            |
| `POST` | `/api/dealers`               | Create dealer account       | ✅            |
| `PUT`  | `/api/dealers/{id}`          | Update dealer               | ✅            |
| `POST` | `/api/dealers/{id}/verify`   | Verify dealer (admin)       | ✅ Admin      |
| `GET`  | `/api/dealers/statistics`    | Get statistics (admin)      | ✅ Admin      |
| `GET`  | `/api/dealers/health`        | Health check                | ❌            |

### Subscriptions Endpoints (DealerManagement)

| Method | Route                                                     | Description                | Auth Required |
| ------ | --------------------------------------------------------- | -------------------------- | ------------- |
| `GET`  | `/api/subscriptions/plans`                                | Get all plans with pricing | ❌            |
| `GET`  | `/api/subscriptions/dealer/{dealerId}`                    | Get dealer subscription    | ✅            |
| `GET`  | `/api/subscriptions/user/{userId}`                        | Get subscription by user   | ✅            |
| `POST` | `/api/subscriptions/dealer/{dealerId}/change-plan`        | Change plan                | ✅            |
| `GET`  | `/api/subscriptions/dealer/{dealerId}/can-action`         | Check action permission    | ✅            |
| `POST` | `/api/subscriptions/dealer/{dealerId}/increment-listings` | Increment listings         | ✅            |
| `POST` | `/api/subscriptions/dealer/{dealerId}/decrement-listings` | Decrement listings         | ✅            |

### Subscription Plans

| Plan           | Monthly Price | Max Listings | Max Images | Features                       |
| -------------- | ------------- | ------------ | ---------- | ------------------------------ |
| **Free**       | $0            | 3            | 5          | Basic profile                  |
| **Basic**      | $49           | 50           | 10         | Analytics, bulk upload, leads  |
| **Pro**        | $129          | 200          | 20         | CRM, automation, branding      |
| **Enterprise** | $299          | Unlimited    | 30         | All features, API, white label |

### Database Entities

- **Dealer** - Dealer account
- **DealerDocument** - Verification documents
- **DealerLocation** - Business locations
- **BusinessHours** - Operating hours

---

## 9. SearchService API

**Base Route:** `/api/search`  
**Authentication:** Not required for public searches

### Endpoints

| Method | Route                                  | Description                  | Auth Required |
| ------ | -------------------------------------- | ---------------------------- | ------------- |
| `POST` | `/api/search/query`                    | Execute Elasticsearch search | ❌            |
| `GET`  | `/api/search/{indexName}/{documentId}` | Get document by ID           | ❌            |
| `GET`  | `/api/search/indices`                  | List available indices       | ❌            |

---

## 10. ErrorService API

**Base Route:** `/api/errors`  
**Authentication:** Required (ErrorServiceRead policy)

### Endpoints

| Method | Route                  | Description             | Auth Required |
| ------ | ---------------------- | ----------------------- | ------------- |
| `POST` | `/api/errors`          | Log new error           | ✅            |
| `GET`  | `/api/errors`          | Get errors with filters | ✅            |
| `GET`  | `/api/errors/{id}`     | Get error details       | ✅            |
| `GET`  | `/api/errors/stats`    | Get error statistics    | ✅            |
| `GET`  | `/api/errors/services` | Get service names       | ✅            |
| `GET`  | `/api/errors/health`   | Health check            | ❌            |

### Database Entities

- **ErrorLog** - Centralized error logs

---

## Database Schemas

### Multi-Tenant Services

These services use the `MultiTenantDbContext` with dealer isolation:

- VehiclesSaleService
- MediaService
- NotificationService
- ContactService

### Shared Database Patterns

All services follow these patterns:

- **PostgreSQL** as the database engine
- **EF Core** for ORM
- **UUID** for primary keys
- **CreatedAt/UpdatedAt** timestamps
- **IsDeleted** soft delete flag (where applicable)
- **DealerId** for multi-tenant isolation

### Key Relationships

```
AuthService.ApplicationUser (1) ──── (1) UserService.User
                                          │
                                          ├── (N) SellerProfile
                                          ├── (N) DealerEmployee
                                          └── (N) Favorite

DealerManagementService.Dealer (1) ──── (N) VehiclesSaleService.Vehicle
                                  │
                                  ├── (N) DealerDocument
                                  ├── (N) DealerLocation ──── (N) BusinessHours
                                  └── (1) BillingService.Subscription
                                          │
                                          ├── (N) Payment
                                          └── (N) Invoice

Vehicle (1) ──── (N) VehicleImage
         │
         ├── (1) Category
         ├── (N) VehicleHomepageSection ──── (N) HomepageSectionConfig
         ├── (N) Favorite
         └── (N) ContactRequest ──── (N) ContactMessage
```

---

## Inter-Service Communication

### Synchronous Communication (HTTP)

| Source Service      | Target Service          | Purpose          |
| ------------------- | ----------------------- | ---------------- |
| Gateway             | All Services            | Request routing  |
| VehiclesSaleService | MediaService            | Image management |
| BillingService      | DealerManagementService | Plan validation  |
| UserService         | AuthService             | Authentication   |

### Asynchronous Communication (RabbitMQ)

| Event                        | Producer                | Consumer(s)                        | Purpose               |
| ---------------------------- | ----------------------- | ---------------------------------- | --------------------- |
| `VehicleCreatedEvent`        | VehiclesSaleService     | SearchService, NotificationService | Index vehicle, notify |
| `VehicleUpdatedEvent`        | VehiclesSaleService     | SearchService                      | Re-index vehicle      |
| `VehicleDeletedEvent`        | VehiclesSaleService     | SearchService                      | Remove from index     |
| `PaymentSucceededEvent`      | BillingService          | NotificationService                | Send receipt          |
| `PaymentFailedEvent`         | BillingService          | NotificationService                | Send alert            |
| `SubscriptionChangedEvent`   | BillingService          | DealerManagementService            | Update limits         |
| `UserRegisteredEvent`        | AuthService             | NotificationService                | Welcome email         |
| `DealerVerifiedEvent`        | DealerManagementService | NotificationService                | Approval email        |
| `ContactRequestCreatedEvent` | ContactService          | NotificationService                | Notify seller         |

### Service Dependencies

```
┌─────────────────────────────────────────────────────────────┐
│                     Frontend (React/Flutter)                 │
└─────────────────────────┬───────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────────┐
│                     API Gateway (Ocelot)                     │
│                                                              │
│  • JWT Validation                                            │
│  • Rate Limiting                                             │
│  • Request Routing                                           │
│  • QoS (Circuit Breaker)                                     │
└─────────────────────────┬───────────────────────────────────┘
                          │
     ┌────────────────────┼────────────────────┐
     ▼                    ▼                    ▼
┌──────────┐       ┌──────────┐        ┌──────────────┐
│ AuthSvc  │◄─────►│ UserSvc  │◄──────►│ DealerMgmt   │
└──────────┘       └────┬─────┘        └──────┬───────┘
                        │                      │
                        ▼                      ▼
                  ┌──────────┐          ┌──────────┐
                  │ Vehicles │◄────────►│ Billing  │
                  │   Sale   │          │ Service  │
                  └────┬─────┘          └────┬─────┘
                       │                     │
          ┌────────────┼──────────┐          │
          ▼            ▼          ▼          ▼
    ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐
    │  Media   │ │  Search  │ │ Contact  │ │ Notific. │
    │ Service  │ │ Service  │ │ Service  │ │ Service  │
    └──────────┘ └──────────┘ └──────────┘ └──────────┘
```

---

## Health Check Endpoints

All services expose a `/health` endpoint:

| Service                 | Health Endpoint               |
| ----------------------- | ----------------------------- |
| Gateway                 | `/health`                     |
| AuthService             | `/api/auth/health`            |
| UserService             | `/api/users/health`           |
| VehiclesSaleService     | `/api/products/health`        |
| MediaService            | `/api/media/health`           |
| BillingService          | `/api/billing/health`         |
| ContactService          | `/api/contactrequests/health` |
| NotificationService     | `/api/notifications/health`   |
| DealerManagementService | `/api/dealers/health`         |
| ErrorService            | `/api/errors/health`          |

---

## Authentication Headers

All authenticated requests require:

```http
Authorization: Bearer <jwt-token>
```

For dealer-specific endpoints:

```http
Authorization: Bearer <jwt-token>
X-Dealer-Id: <dealer-guid>
```

---

## Error Response Format

All services return errors in this format:

```json
{
  "success": false,
  "error": "Error message",
  "details": ["Additional details if any"]
}
```

Or for validation errors:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "fieldName": ["Error message"]
  }
}
```

---

**Last Updated:** January 20, 2026  
**Version:** 1.0.0
