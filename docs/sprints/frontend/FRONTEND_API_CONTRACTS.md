# 🔌 API Contracts - Frontend ↔ Backend

> **Base URL (Development)**: `http://localhost:5000`  
> **Base URL (Production)**: `https://api.cardealer.com`  
> **Gateway URL**: `http://localhost:15095`

---

## 📋 ÍNDICE

1. [Authentication Service](#1-authentication-service)
2. [Vehicle Service](#2-vehicle-service)
3. [Search Service](#3-search-service)
4. [Media Service](#4-media-service)
5. [User Service](#5-user-service)
6. [Contact Service](#6-contact-service)
7. [Notification Service](#7-notification-service)
8. [Admin Service](#8-admin-service)

---

## 1️⃣ AUTHENTICATION SERVICE

Base Path: `/api/auth`

### POST /register
**Description**: Register a new user

**Request**:
```json
{
  "username": "john_doe",
  "email": "john@example.com",
  "password": "SecurePass123!"
}
```

**Response** (201 Created):
```json
{
  "success": true,
  "data": {
    "userId": "uuid-123",
    "username": "john_doe",
    "email": "john@example.com",
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "refresh-token-abc",
    "expiresAt": "2024-12-04T18:30:00Z"
  }
}
```

**Error** (400):
```json
{
  "success": false,
  "error": {
    "code": "EMAIL_ALREADY_EXISTS",
    "message": "Email already registered"
  }
}
```

---

### POST /login
**Description**: User login

**Request**:
```json
{
  "email": "john@example.com",
  "password": "SecurePass123!"
}
```

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "userId": "uuid-123",
    "email": "john@example.com",
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "refresh-token-abc",
    "expiresAt": "2024-12-04T18:30:00Z",
    "requiresTwoFactor": false
  }
}
```

**Response with 2FA** (200 OK):
```json
{
  "success": true,
  "data": {
    "userId": "uuid-123",
    "requiresTwoFactor": true,
    "tempToken": "temp-token-xyz"
  }
}
```

---

### POST /refresh-token
**Description**: Refresh access token

**Request**:
```json
{
  "refreshToken": "refresh-token-abc"
}
```

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "accessToken": "new-access-token",
    "refreshToken": "new-refresh-token",
    "expiresAt": "2024-12-04T19:30:00Z"
  }
}
```

---

### POST /logout
**Description**: Logout user (invalidate tokens)

**Headers**:
```
Authorization: Bearer {accessToken}
```

**Request**:
```json
{
  "refreshToken": "refresh-token-abc"
}
```

**Response** (200 OK):
```json
{
  "success": true,
  "message": "Logged out successfully"
}
```

---

### POST /forgot-password
**Description**: Request password reset email

**Request**:
```json
{
  "email": "john@example.com"
}
```

**Response** (200 OK):
```json
{
  "success": true,
  "message": "Password reset email sent"
}
```

---

### POST /reset-password
**Description**: Reset password with token

**Request**:
```json
{
  "token": "reset-token-xyz",
  "newPassword": "NewSecurePass456!",
  "confirmPassword": "NewSecurePass456!"
}
```

**Response** (200 OK):
```json
{
  "success": true,
  "message": "Password reset successfully"
}
```

---

### POST /verify-email
**Description**: Verify email with token

**Request**:
```json
{
  "token": "verification-token-xyz"
}
```

**Response** (200 OK):
```json
{
  "success": true,
  "message": "Email verified successfully"
}
```

---

## 2️⃣ VEHICLE SERVICE

Base Path: `/api/vehicles`

### GET /vehicles
**Description**: Get list of vehicles with pagination

**Query Parameters**:
```
?page=1
&pageSize=20
&sortBy=price
&sortOrder=asc
```

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "vehicles": [
      {
        "id": "vehicle-uuid-1",
        "brand": "Toyota",
        "model": "Camry",
        "year": 2023,
        "trim": "XLE Premium",
        "price": 35000,
        "mileage": 15000,
        "transmission": "automatic",
        "fuelType": "hybrid",
        "bodyType": "sedan",
        "exteriorColor": "Silver",
        "interiorColor": "Black",
        "images": [
          "https://cdn.cardealer.com/vehicles/image1.jpg",
          "https://cdn.cardealer.com/vehicles/image2.jpg"
        ],
        "primaryImage": "https://cdn.cardealer.com/vehicles/image1.jpg",
        "location": {
          "city": "Los Angeles",
          "state": "CA",
          "zipCode": "90001"
        },
        "sellerId": "seller-uuid-123",
        "sellerName": "John Seller",
        "status": "active",
        "isFeatured": true,
        "isVerified": true,
        "views": 234,
        "favorites": 12,
        "createdAt": "2024-11-01T10:00:00Z",
        "updatedAt": "2024-11-15T14:30:00Z"
      }
    ],
    "pagination": {
      "currentPage": 1,
      "pageSize": 20,
      "totalPages": 50,
      "totalItems": 1000,
      "hasNextPage": true,
      "hasPreviousPage": false
    }
  }
}
```

---

### GET /vehicles/search
**Description**: Search vehicles with filters

**Query Parameters**:
```
?q=toyota+camry
&brands=Toyota,Honda
&minPrice=20000
&maxPrice=50000
&minYear=2020
&maxYear=2024
&fuelTypes=hybrid,electric
&transmission=automatic
&bodyTypes=sedan,suv
&minMileage=0
&maxMileage=50000
&features=leather,sunroof
&location=Los Angeles, CA
&radius=50
&page=1
&pageSize=20
&sortBy=price
&sortOrder=asc
```

**Response**: Same as GET /vehicles

---

### GET /vehicles/:id
**Description**: Get vehicle details by ID

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "id": "vehicle-uuid-1",
    "brand": "Toyota",
    "model": "Camry",
    "year": 2023,
    "trim": "XLE Premium",
    "vin": "1HGBH41JXMN109186",
    "price": 35000,
    "mileage": 15000,
    "transmission": "automatic",
    "fuelType": "hybrid",
    "bodyType": "sedan",
    "doors": 4,
    "seats": 5,
    "exteriorColor": "Silver",
    "interiorColor": "Black",
    "description": "Well-maintained 2023 Toyota Camry with low mileage...",
    "images": [
      {
        "id": "img-1",
        "url": "https://cdn.cardealer.com/vehicles/image1.jpg",
        "isPrimary": true,
        "order": 1
      }
    ],
    "features": [
      "Leather Seats",
      "Sunroof",
      "Navigation System",
      "Backup Camera",
      "Heated Seats",
      "Bluetooth",
      "Cruise Control"
    ],
    "specs": {
      "engine": "2.5L 4-Cylinder",
      "horsepower": 203,
      "torque": "184 lb-ft",
      "drivetrain": "FWD",
      "mpgCity": 28,
      "mpgHighway": 39,
      "fuelCapacity": "14.3 gal"
    },
    "location": {
      "city": "Los Angeles",
      "state": "CA",
      "zipCode": "90001",
      "latitude": 34.0522,
      "longitude": -118.2437
    },
    "seller": {
      "id": "seller-uuid-123",
      "name": "John Seller",
      "avatar": "https://cdn.cardealer.com/avatars/john.jpg",
      "rating": 4.8,
      "reviewCount": 45,
      "memberSince": "2020-01-15",
      "phone": "+1-555-123-4567",
      "isVerified": true,
      "isDealership": false
    },
    "status": "active",
    "condition": "excellent",
    "isFeatured": true,
    "isVerified": true,
    "isCertified": false,
    "views": 234,
    "favorites": 12,
    "inquiries": 8,
    "createdAt": "2024-11-01T10:00:00Z",
    "updatedAt": "2024-11-15T14:30:00Z"
  }
}
```

---

### POST /vehicles
**Description**: Create new vehicle listing (authenticated)

**Headers**:
```
Authorization: Bearer {accessToken}
```

**Request**:
```json
{
  "brand": "Toyota",
  "model": "Camry",
  "year": 2023,
  "trim": "XLE Premium",
  "vin": "1HGBH41JXMN109186",
  "price": 35000,
  "mileage": 15000,
  "transmission": "automatic",
  "fuelType": "hybrid",
  "bodyType": "sedan",
  "doors": 4,
  "seats": 5,
  "exteriorColor": "Silver",
  "interiorColor": "Black",
  "description": "Well-maintained vehicle...",
  "features": ["Leather Seats", "Sunroof"],
  "specs": {
    "engine": "2.5L 4-Cylinder",
    "horsepower": 203
  },
  "location": {
    "city": "Los Angeles",
    "state": "CA",
    "zipCode": "90001"
  },
  "imageIds": ["media-id-1", "media-id-2"],
  "status": "draft"
}
```

**Response** (201 Created):
```json
{
  "success": true,
  "data": {
    "id": "vehicle-uuid-new",
    "status": "draft",
    "message": "Vehicle created successfully. Awaiting approval."
  }
}
```

---

### PUT /vehicles/:id
**Description**: Update vehicle listing

**Headers**:
```
Authorization: Bearer {accessToken}
```

**Request**: Same as POST (partial updates allowed)

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "id": "vehicle-uuid-1",
    "message": "Vehicle updated successfully"
  }
}
```

---

### DELETE /vehicles/:id
**Description**: Delete vehicle listing

**Headers**:
```
Authorization: Bearer {accessToken}
```

**Response** (200 OK):
```json
{
  "success": true,
  "message": "Vehicle deleted successfully"
}
```

---

### POST /vehicles/:id/favorite
**Description**: Toggle favorite

**Headers**:
```
Authorization: Bearer {accessToken}
```

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "isFavorited": true
  }
}
```

---

### GET /vehicles/user/:userId/favorites
**Description**: Get user's favorite vehicles

**Headers**:
```
Authorization: Bearer {accessToken}
```

**Response**: Same structure as GET /vehicles

---

### GET /vehicles/:id/similar
**Description**: Get similar vehicles

**Query Parameters**:
```
?limit=6
```

**Response**: Array of vehicles (same structure as GET /vehicles)

---

### GET /vehicles/filters/brands
**Description**: Get list of available brands

**Response** (200 OK):
```json
{
  "success": true,
  "data": [
    { "name": "Toyota", "count": 234 },
    { "name": "Honda", "count": 189 },
    { "name": "Ford", "count": 156 }
  ]
}
```

---

### GET /vehicles/filters/models
**Description**: Get models for a brand

**Query Parameters**:
```
?brand=Toyota
```

**Response** (200 OK):
```json
{
  "success": true,
  "data": [
    { "name": "Camry", "count": 45 },
    { "name": "Corolla", "count": 38 },
    { "name": "RAV4", "count": 52 }
  ]
}
```

---

### GET /vehicles/market-value
**Description**: Get market value estimation

**Query Parameters**:
```
?brand=Toyota
&model=Camry
&year=2023
&mileage=15000
```

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "low": 32000,
    "average": 35000,
    "high": 38000,
    "suggested": 35000,
    "confidence": "high",
    "sampleSize": 127
  }
}
```

---

## 3️⃣ SEARCH SERVICE

Base Path: `/api/search`

### POST /search/query
**Description**: Execute search query (Elasticsearch)

**Request**:
```json
{
  "queryText": "toyota camry hybrid",
  "indexName": "vehicles",
  "searchType": "fuzzy",
  "fields": ["brand", "model", "description"],
  "filters": {
    "price": { "gte": 20000, "lte": 50000 },
    "year": 2023,
    "status": "active"
  },
  "page": 1,
  "pageSize": 20,
  "sortBy": "price",
  "sortOrder": "ascending",
  "enableHighlighting": true
}
```

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "hits": [
      {
        "id": "vehicle-uuid-1",
        "score": 0.95,
        "source": {
          "brand": "Toyota",
          "model": "Camry",
          "price": 35000
        },
        "highlights": {
          "description": ["...well-maintained <mark>Toyota Camry Hybrid</mark>..."]
        }
      }
    ],
    "total": 127,
    "took": 45,
    "maxScore": 0.95,
    "pagination": {
      "currentPage": 1,
      "pageSize": 20,
      "totalPages": 7
    }
  }
}
```

---

### GET /search/autocomplete
**Description**: Autocomplete suggestions

**Query Parameters**:
```
?q=toyo
&field=brand
&limit=5
```

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "suggestions": [
      { "text": "Toyota", "count": 234 },
      { "text": "Toyota Camry", "count": 45 },
      { "text": "Toyota Corolla", "count": 38 }
    ]
  }
}
```

---

## 4️⃣ MEDIA SERVICE

Base Path: `/api/media`

### POST /media/upload/init
**Description**: Initialize image upload

**Headers**:
```
Authorization: Bearer {accessToken}
```

**Request**:
```json
{
  "ownerId": "user-uuid-123",
  "context": "vehicle",
  "fileName": "car-front.jpg",
  "contentType": "image/jpeg",
  "fileSize": 2048576
}
```

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "mediaId": "media-uuid-1",
    "uploadUrl": "https://s3.amazonaws.com/bucket/presigned-url",
    "expiresAt": "2024-12-04T17:00:00Z",
    "uploadHeaders": {
      "Content-Type": "image/jpeg"
    },
    "storageKey": "users/user-123/vehicles/car-front.jpg"
  }
}
```

---

### POST /media/upload/finalize/:mediaId
**Description**: Finalize upload after client uploads to presigned URL

**Headers**:
```
Authorization: Bearer {accessToken}
```

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "mediaId": "media-uuid-1",
    "status": "processed",
    "cdnUrl": "https://cdn.cardealer.com/media/car-front.jpg",
    "processedAt": "2024-12-04T16:35:00Z"
  }
}
```

---

### GET /media/:mediaId
**Description**: Get media details

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "id": "media-uuid-1",
    "ownerId": "user-uuid-123",
    "context": "vehicle",
    "fileName": "car-front.jpg",
    "contentType": "image/jpeg",
    "fileSize": 2048576,
    "cdnUrl": "https://cdn.cardealer.com/media/car-front.jpg",
    "thumbnailUrl": "https://cdn.cardealer.com/media/car-front-thumb.jpg",
    "status": "processed",
    "createdAt": "2024-12-04T16:30:00Z"
  }
}
```

---

## 5️⃣ USER SERVICE

Base Path: `/api/users`

### GET /users/me
**Description**: Get current user profile

**Headers**:
```
Authorization: Bearer {accessToken}
```

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "id": "user-uuid-123",
    "username": "john_doe",
    "email": "john@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "avatar": "https://cdn.cardealer.com/avatars/john.jpg",
    "phone": "+1-555-123-4567",
    "location": {
      "city": "Los Angeles",
      "state": "CA"
    },
    "bio": "Car enthusiast and collector",
    "role": "seller",
    "isVerified": true,
    "memberSince": "2020-01-15",
    "stats": {
      "totalListings": 3,
      "activeListings": 2,
      "soldVehicles": 15,
      "rating": 4.8,
      "reviewCount": 45
    }
  }
}
```

---

### PUT /users/me
**Description**: Update current user profile

**Headers**:
```
Authorization: Bearer {accessToken}
```

**Request**:
```json
{
  "firstName": "John",
  "lastName": "Doe",
  "phone": "+1-555-123-4567",
  "location": {
    "city": "Los Angeles",
    "state": "CA"
  },
  "bio": "Updated bio"
}
```

**Response** (200 OK):
```json
{
  "success": true,
  "message": "Profile updated successfully"
}
```

---

### GET /users/me/listings
**Description**: Get user's vehicle listings

**Headers**:
```
Authorization: Bearer {accessToken}
```

**Query Parameters**:
```
?status=active
&page=1
&pageSize=20
```

**Response**: Same structure as GET /vehicles

---

### GET /users/me/stats
**Description**: Get user statistics

**Headers**:
```
Authorization: Bearer {accessToken}
```

**Query Parameters**:
```
?period=30d
```

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "period": "30d",
    "views": {
      "total": 1234,
      "trend": 15.5,
      "dailyAverage": 41
    },
    "favorites": {
      "total": 45,
      "trend": 8.2
    },
    "inquiries": {
      "total": 12,
      "trend": -3.1
    },
    "topListing": {
      "id": "vehicle-uuid-1",
      "brand": "Toyota",
      "model": "Camry",
      "views": 456
    },
    "viewsChart": [
      { "date": "2024-11-05", "views": 38 },
      { "date": "2024-11-06", "views": 42 }
    ]
  }
}
```

---

## 6️⃣ CONTACT SERVICE

Base Path: `/api/contact`

### POST /contact/seller
**Description**: Send message to seller

**Headers**:
```
Authorization: Bearer {accessToken}
```

**Request**:
```json
{
  "vehicleId": "vehicle-uuid-1",
  "sellerId": "seller-uuid-123",
  "name": "Jane Buyer",
  "email": "jane@example.com",
  "phone": "+1-555-987-6543",
  "message": "Hi, I'm interested in this vehicle. Is it still available?"
}
```

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "conversationId": "conversation-uuid-1",
    "message": "Message sent successfully"
  }
}
```

---

### GET /contact/conversations
**Description**: Get user's conversations

**Headers**:
```
Authorization: Bearer {accessToken}
```

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "conversations": [
      {
        "id": "conversation-uuid-1",
        "vehicleId": "vehicle-uuid-1",
        "vehicle": {
          "brand": "Toyota",
          "model": "Camry",
          "year": 2023,
          "primaryImage": "https://cdn.cardealer.com/..."
        },
        "otherUser": {
          "id": "user-uuid-456",
          "name": "Jane Buyer",
          "avatar": "https://cdn.cardealer.com/avatars/jane.jpg"
        },
        "lastMessage": {
          "text": "Yes, still available!",
          "sentAt": "2024-12-04T09:15:00Z",
          "sentBy": "user-uuid-123"
        },
        "unreadCount": 2,
        "createdAt": "2024-12-03T14:30:00Z"
      }
    ]
  }
}
```

---

### GET /contact/conversations/:id
**Description**: Get conversation messages

**Headers**:
```
Authorization: Bearer {accessToken}
```

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "conversationId": "conversation-uuid-1",
    "messages": [
      {
        "id": "message-uuid-1",
        "text": "Hi, is this car still available?",
        "sentBy": "user-uuid-456",
        "sentAt": "2024-12-03T14:30:00Z",
        "isRead": true
      },
      {
        "id": "message-uuid-2",
        "text": "Yes, still available!",
        "sentBy": "user-uuid-123",
        "sentAt": "2024-12-04T09:15:00Z",
        "isRead": false
      }
    ]
  }
}
```

---

### POST /contact/messages/send
**Description**: Send message in conversation

**Headers**:
```
Authorization: Bearer {accessToken}
```

**Request**:
```json
{
  "conversationId": "conversation-uuid-1",
  "text": "Great! When can I schedule a test drive?"
}
```

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "messageId": "message-uuid-3",
    "sentAt": "2024-12-04T10:00:00Z"
  }
}
```

---

## 7️⃣ NOTIFICATION SERVICE

Base Path: `/api/notifications`

### GET /notifications
**Description**: Get user notifications

**Headers**:
```
Authorization: Bearer {accessToken}
```

**Query Parameters**:
```
?unreadOnly=true
&limit=20
```

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "notifications": [
      {
        "id": "notif-uuid-1",
        "type": "new_message",
        "title": "New message",
        "message": "You have a new message about Toyota Camry",
        "data": {
          "conversationId": "conversation-uuid-1",
          "vehicleId": "vehicle-uuid-1"
        },
        "isRead": false,
        "createdAt": "2024-12-04T09:15:00Z"
      },
      {
        "id": "notif-uuid-2",
        "type": "listing_approved",
        "title": "Listing approved",
        "message": "Your Toyota Camry listing has been approved",
        "data": {
          "vehicleId": "vehicle-uuid-1"
        },
        "isRead": true,
        "createdAt": "2024-12-03T16:00:00Z"
      }
    ],
    "unreadCount": 3
  }
}
```

---

### PUT /notifications/:id/read
**Description**: Mark notification as read

**Headers**:
```
Authorization: Bearer {accessToken}
```

**Response** (200 OK):
```json
{
  "success": true,
  "message": "Notification marked as read"
}
```

---

### PUT /notifications/read-all
**Description**: Mark all notifications as read

**Headers**:
```
Authorization: Bearer {accessToken}
```

**Response** (200 OK):
```json
{
  "success": true,
  "message": "All notifications marked as read"
}
```

---

## 8️⃣ ADMIN SERVICE

Base Path: `/api/admin`

### GET /admin/vehicles/pending
**Description**: Get pending vehicle approvals (admin only)

**Headers**:
```
Authorization: Bearer {accessToken}
```

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "vehicles": [
      {
        "id": "vehicle-uuid-1",
        "brand": "Toyota",
        "model": "Camry",
        "year": 2023,
        "price": 35000,
        "seller": {
          "id": "seller-uuid-123",
          "name": "John Doe",
          "email": "john@example.com"
        },
        "status": "pending",
        "submittedAt": "2024-12-01T10:00:00Z"
      }
    ],
    "total": 12
  }
}
```

---

### POST /admin/vehicles/:id/approve
**Description**: Approve vehicle listing

**Headers**:
```
Authorization: Bearer {accessToken}
```

**Request**:
```json
{
  "approvedBy": "admin-uuid-456",
  "reason": "Meets all requirements"
}
```

**Response** (200 OK):
```json
{
  "success": true,
  "message": "Vehicle approved successfully"
}
```

---

### POST /admin/vehicles/:id/reject
**Description**: Reject vehicle listing

**Headers**:
```
Authorization: Bearer {accessToken}
```

**Request**:
```json
{
  "rejectedBy": "admin-uuid-456",
  "reason": "Incomplete information - missing VIN"
}
```

**Response** (200 OK):
```json
{
  "success": true,
  "message": "Vehicle rejected. Email sent to seller."
}
```

---

## 🔒 AUTHENTICATION

All protected endpoints require JWT token in Authorization header:

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Token Expiration
- Access Token: 15 minutes
- Refresh Token: 7 days

### Token Refresh Flow
1. Access token expires → API returns 401
2. Frontend calls `/api/auth/refresh-token` with refresh token
3. Get new access & refresh tokens
4. Retry original request with new token

---

## ❌ ERROR RESPONSES

### Standard Error Format
```json
{
  "success": false,
  "error": {
    "code": "ERROR_CODE",
    "message": "Human-readable error message",
    "details": {
      "field": "email",
      "reason": "Email already exists"
    }
  }
}
```

### Common Error Codes
- `UNAUTHORIZED` (401): Invalid or expired token
- `FORBIDDEN` (403): Insufficient permissions
- `NOT_FOUND` (404): Resource not found
- `VALIDATION_ERROR` (400): Invalid input
- `CONFLICT` (409): Resource already exists
- `INTERNAL_ERROR` (500): Server error

---

## 🚀 RATE LIMITING

**Limits**:
- Unauthenticated: 100 requests/hour
- Authenticated: 1000 requests/hour
- Admin: Unlimited

**Headers**:
```
X-RateLimit-Limit: 1000
X-RateLimit-Remaining: 950
X-RateLimit-Reset: 1733327400
```

---

## 📝 PAGINATION

**Query Parameters**:
```
?page=1
&pageSize=20
```

**Response Format**:
```json
{
  "data": [...],
  "pagination": {
    "currentPage": 1,
    "pageSize": 20,
    "totalPages": 50,
    "totalItems": 1000,
    "hasNextPage": true,
    "hasPreviousPage": false
  }
}
```

---

**FIN DE API CONTRACTS** 🔌
