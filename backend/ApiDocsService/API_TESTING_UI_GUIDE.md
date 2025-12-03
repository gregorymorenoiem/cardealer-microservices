# API Testing UI Guide - ApiDocsService

## 📋 Overview

The **API Testing Interface** provides a comprehensive web-based UI for testing and exploring all CarDealer microservices APIs without requiring external tools like Postman or Insomnia.

## 🎨 Features

### 1. **Interactive Request Builder**
- Visual interface for constructing API requests
- Support for all HTTP methods (GET, POST, PUT, DELETE, PATCH)
- Dynamic service and endpoint selection

### 2. **Request Configuration**
- **Headers**: Add custom headers with key-value pairs
- **Query Parameters**: Define URL query parameters
- **Request Body**: JSON editor for request payloads
- **Authorization**: Bearer token and other auth methods

### 3. **Response Viewer**
- **Status Code**: Visual indicator (success/error)
- **Response Time**: Request duration in milliseconds
- **Response Body**: Formatted JSON with syntax highlighting
- **Headers**: View response headers

### 4. **Batch Testing**
- Execute multiple requests sequentially
- Collection-based test organization
- Success/failure reporting

## 🚀 Accessing the Testing UI

### Web Interface

```
http://localhost:5320/testing
```

### From Documentation Portal

Navigate to the main portal and click "Testing UI" link.

## 📖 Usage Guide

### Basic Request

#### Step 1: Select Service

1. Choose a service from the sidebar or dropdown
2. Services are grouped by category
3. Health status indicators show availability

#### Step 2: Configure Request

```http
Method: GET
URL: /api/errors
```

#### Step 3: Add Headers (Optional)

```
Content-Type: application/json
X-Request-ID: test-12345
```

#### Step 4: Add Query Parameters (Optional)

```
page: 1
pageSize: 10
status: active
```

#### Step 5: Send Request

Click **"Send Request"** button and view the response.

### POST Request with Body

```json
{
  "method": "POST",
  "url": "/api/auth/login",
  "headers": {
    "Content-Type": "application/json"
  },
  "body": {
    "username": "admin",
    "password": "password123"
  }
}
```

### Authenticated Request

```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

## 🔧 API Endpoints

### Execute Test Request

```http
POST /api/testing/execute
Content-Type: application/json

{
  "serviceName": "AuthService",
  "path": "/api/auth/login",
  "method": "POST",
  "headers": {
    "Content-Type": "application/json"
  },
  "queryParameters": {},
  "body": {
    "username": "admin",
    "password": "password123"
  },
  "authorization": null
}
```

**Response:**
```json
{
  "requestId": "550e8400-e29b-41d4-a716-446655440000",
  "success": true,
  "statusCode": 200,
  "responseTimeMs": 145,
  "responseBody": "{\"token\": \"eyJhbGc...\", \"expiresIn\": 3600}",
  "responseHeaders": {
    "content-type": "application/json",
    "x-request-id": "req-12345"
  },
  "errorMessage": null,
  "timestamp": "2024-12-03T10:30:00Z"
}
```

### Execute Batch Tests

```http
POST /api/testing/batch
Content-Type: application/json

{
  "tests": [
    {
      "serviceName": "AuthService",
      "path": "/api/auth/login",
      "method": "POST",
      "body": {
        "username": "user1",
        "password": "pass1"
      }
    },
    {
      "serviceName": "VehicleService",
      "path": "/api/vehicles",
      "method": "GET",
      "queryParameters": {
        "status": "available"
      }
    }
  ],
  "stopOnFailure": false
}
```

**Response:**
```json
{
  "batchId": "batch-12345",
  "totalTests": 2,
  "successCount": 2,
  "failureCount": 0,
  "results": [
    {
      "requestId": "req-001",
      "success": true,
      "statusCode": 200,
      "responseTimeMs": 120
    },
    {
      "requestId": "req-002",
      "success": true,
      "statusCode": 200,
      "responseTimeMs": 85
    }
  ],
  "timestamp": "2024-12-03T10:30:00Z"
}
```

### Get Test Collections

```http
GET /api/testing/collections
```

Returns pre-configured test collections.

## 💡 Testing Examples

### Example 1: Health Check

```javascript
// Request
{
  "serviceName": "ErrorService",
  "path": "/health",
  "method": "GET"
}

// Expected Response
Status: 200 OK
Body: { "status": "Healthy" }
```

### Example 2: Create Resource

```javascript
// Request
{
  "serviceName": "VehicleService",
  "path": "/api/vehicles",
  "method": "POST",
  "headers": {
    "Authorization": "Bearer token123",
    "Content-Type": "application/json"
  },
  "body": {
    "make": "Toyota",
    "model": "Camry",
    "year": 2024,
    "price": 35000
  }
}

// Expected Response
Status: 201 Created
Body: { "id": "veh-123", "make": "Toyota", ... }
```

### Example 3: Query with Filters

```javascript
// Request
{
  "serviceName": "VehicleService",
  "path": "/api/vehicles",
  "method": "GET",
  "queryParameters": {
    "make": "Toyota",
    "minYear": "2020",
    "maxPrice": "40000",
    "page": "1",
    "pageSize": "20"
  }
}

// Expected Response
Status: 200 OK
Body: {
  "items": [...],
  "totalCount": 15,
  "page": 1,
  "pageSize": 20
}
```

### Example 4: Update Resource

```javascript
// Request
{
  "serviceName": "VehicleService",
  "path": "/api/vehicles/veh-123",
  "method": "PUT",
  "headers": {
    "Authorization": "Bearer token123"
  },
  "body": {
    "price": 32000,
    "status": "sold"
  }
}

// Expected Response
Status: 200 OK
Body: { "id": "veh-123", "price": 32000, "status": "sold" }
```

### Example 5: Delete Resource

```javascript
// Request
{
  "serviceName": "VehicleService",
  "path": "/api/vehicles/veh-123",
  "method": "DELETE",
  "headers": {
    "Authorization": "Bearer token123"
  }
}

// Expected Response
Status: 204 No Content
```

## 🎯 Test Collections

### Health Checks Collection

```json
{
  "name": "Health Checks",
  "description": "Test health endpoints for all services",
  "tests": [
    { "serviceName": "AuthService", "path": "/health", "method": "GET" },
    { "serviceName": "VehicleService", "path": "/health", "method": "GET" },
    { "serviceName": "ErrorService", "path": "/health", "method": "GET" }
  ]
}
```

### Authentication Tests Collection

```json
{
  "name": "Authentication Tests",
  "description": "Test authentication flows",
  "tests": [
    {
      "serviceName": "AuthService",
      "path": "/api/auth/login",
      "method": "POST",
      "body": { "username": "testuser", "password": "testpass" }
    },
    {
      "serviceName": "AuthService",
      "path": "/api/auth/refresh",
      "method": "POST",
      "body": { "refreshToken": "refresh-token-here" }
    }
  ]
}
```

## 🛠️ Advanced Features

### Request Chaining

Execute requests in sequence and use responses in subsequent requests:

```javascript
// 1. Login to get token
const loginResponse = await executeTest({
  serviceName: "AuthService",
  path: "/api/auth/login",
  method: "POST",
  body: { username: "admin", password: "pass" }
});

const token = JSON.parse(loginResponse.responseBody).token;

// 2. Use token in next request
const vehiclesResponse = await executeTest({
  serviceName: "VehicleService",
  path: "/api/vehicles",
  method: "GET",
  authorization: `Bearer ${token}`
});
```

### Environment Variables

Store common values for reuse:

```javascript
const environments = {
  development: {
    authService: "http://localhost:5060",
    vehicleService: "http://localhost:5070"
  },
  production: {
    authService: "https://auth.cardealer.com",
    vehicleService: "https://vehicles.cardealer.com"
  }
};
```

## 📊 Response Analysis

### Success Indicators

- ✅ **2xx Status**: Request successful
- ⚡ **Response Time < 200ms**: Excellent performance
- 📦 **Valid JSON**: Proper response format

### Error Indicators

- ❌ **4xx Status**: Client error (bad request, auth, etc.)
- ❌ **5xx Status**: Server error
- ⚠️ **Timeout**: Service unreachable
- ⚠️ **Invalid JSON**: Malformed response

## 🔍 Debugging Tips

### 1. Check Request Headers

Ensure proper content type and authorization:
```
Content-Type: application/json
Authorization: Bearer <token>
```

### 2. Validate JSON Body

Use a JSON validator to ensure proper format.

### 3. Monitor Response Time

High response times may indicate:
- Network latency
- Database slow queries
- Heavy processing

### 4. Analyze Error Messages

Error responses should include:
- Error code
- Error message
- Request ID (for tracking)

## 🚀 Integration with CI/CD

Export test collections for automated testing:

```bash
# Run tests via API
curl -X POST http://localhost:5320/api/testing/batch \
  -H "Content-Type: application/json" \
  -d @tests/health-checks.json
```

## 📚 Additional Resources

- [REST API Best Practices](https://restfulapi.net/)
- [HTTP Status Codes](https://httpstatuses.com/)
- [JWT Authentication](https://jwt.io/)
