# Saga Orchestration - Example Usage

## 📖 Overview

This document provides examples of how to use the Saga Orchestration pattern implemented in MessageBusService.

## 🎯 Basic Saga Example

### Creating a Simple Saga

```csharp
POST /api/saga/start
Content-Type: application/json

{
  "name": "CreateOrderSaga",
  "description": "Create order with inventory and payment",
  "type": "Orchestration",
  "correlationId": "order-12345",
  "timeout": "00:05:00",
  "steps": [
    {
      "name": "ValidateInventory",
      "serviceName": "InventoryService",
      "actionType": "http.post.inventory",
      "actionPayload": "{\"url\":\"http://inventory-service/api/inventory/validate\",\"body\":\"{\\\"productId\\\":\\\"123\\\",\\\"quantity\\\":2}\"}",
      "compensationActionType": "http.post.inventory",
      "compensationPayload": "{\"url\":\"http://inventory-service/api/inventory/release\",\"body\":\"{\\\"productId\\\":\\\"123\\\",\\\"quantity\\\":2}\"}",
      "maxRetries": 3,
      "timeout": "00:01:00"
    },
    {
      "name": "ProcessPayment",
      "serviceName": "PaymentService",
      "actionType": "http.post.payment",
      "actionPayload": "{\"url\":\"http://payment-service/api/payments/process\",\"body\":\"{\\\"amount\\\":100.00,\\\"currency\\\":\\\"USD\\\"}\"}",
      "compensationActionType": "http.post.payment",
      "compensationPayload": "{\"url\":\"http://payment-service/api/payments/refund\",\"body\":\"{\\\"transactionId\\\":\\\"{{transaction_id}}\\\"}\"}",
      "maxRetries": 3,
      "timeout": "00:02:00"
    },
    {
      "name": "CreateOrder",
      "serviceName": "OrderService",
      "actionType": "http.post.order",
      "actionPayload": "{\"url\":\"http://order-service/api/orders/create\",\"body\":\"{\\\"orderId\\\":\\\"order-12345\\\"}\"}",
      "compensationActionType": "http.delete.order",
      "compensationPayload": "{\"url\":\"http://order-service/api/orders/order-12345\"}",
      "maxRetries": 3,
      "timeout": "00:01:00"
    }
  ]
}
```

### Response

```json
{
  "sagaId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "CreateOrderSaga",
  "status": "Running",
  "correlationId": "order-12345",
  "createdAt": "2025-01-02T10:00:00Z",
  "totalSteps": 3,
  "currentStepIndex": 0,
  "message": "Saga started successfully"
}
```

## 🔄 Saga with RabbitMQ Steps

### Example: Microservice Communication via RabbitMQ

```csharp
POST /api/saga/start
Content-Type: application/json

{
  "name": "UserRegistrationSaga",
  "description": "Register user and send welcome email",
  "type": "Orchestration",
  "steps": [
    {
      "name": "CreateUserAccount",
      "serviceName": "UserService",
      "actionType": "rabbitmq.publish.user-exchange.user.created",
      "actionPayload": "{\"userId\":\"user-123\",\"email\":\"user@example.com\",\"name\":\"John Doe\"}",
      "compensationActionType": "rabbitmq.publish.user-exchange.user.deleted",
      "compensationPayload": "{\"userId\":\"user-123\"}",
      "maxRetries": 3
    },
    {
      "name": "SendWelcomeEmail",
      "serviceName": "NotificationService",
      "actionType": "rabbitmq.publish.notification-exchange.email.send",
      "actionPayload": "{\"to\":\"user@example.com\",\"template\":\"welcome\",\"data\":{\"name\":\"John Doe\"}}",
      "maxRetries": 2
    }
  ]
}
```

## 📊 Monitoring Saga Status

### Get Saga Details

```bash
GET /api/saga/{sagaId}
```

**Response:**

```json
{
  "sagaId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "CreateOrderSaga",
  "description": "Create order with inventory and payment",
  "type": "Orchestration",
  "status": "Running",
  "correlationId": "order-12345",
  "createdAt": "2025-01-02T10:00:00Z",
  "startedAt": "2025-01-02T10:00:01Z",
  "totalSteps": 3,
  "currentStepIndex": 1,
  "steps": [
    {
      "stepId": "step-1",
      "order": 0,
      "name": "ValidateInventory",
      "serviceName": "InventoryService",
      "actionType": "http.post.inventory",
      "status": "Completed",
      "startedAt": "2025-01-02T10:00:01Z",
      "completedAt": "2025-01-02T10:00:02Z",
      "retryAttempts": 0,
      "maxRetries": 3
    },
    {
      "stepId": "step-2",
      "order": 1,
      "name": "ProcessPayment",
      "serviceName": "PaymentService",
      "actionType": "http.post.payment",
      "status": "Running",
      "startedAt": "2025-01-02T10:00:02Z",
      "retryAttempts": 0,
      "maxRetries": 3
    },
    {
      "stepId": "step-3",
      "order": 2,
      "name": "CreateOrder",
      "serviceName": "OrderService",
      "actionType": "http.post.order",
      "status": "Pending",
      "retryAttempts": 0,
      "maxRetries": 3
    }
  ]
}
```

## ⚠️ Handling Failures

### Automatic Compensation

When a step fails, the saga automatically triggers compensation (rollback) of completed steps in reverse order:

1. **Payment fails** → Saga automatically:
   - Compensates "ValidateInventory" (releases inventory)
   - Marks saga as "Compensated"

### Manual Compensation

```bash
POST /api/saga/{sagaId}/compensate
```

## 🔄 Retry Failed Step

```bash
POST /api/saga/{sagaId}/steps/{stepId}/retry
```

## 🛑 Abort Saga

```bash
POST /api/saga/{sagaId}/abort
Content-Type: application/json

{
  "reason": "User cancelled order"
}
```

## 📋 Query Sagas by Status

```bash
GET /api/saga/status/running
GET /api/saga/status/failed
GET /api/saga/status/completed
GET /api/saga/status/compensated
```

## 🎯 Advanced Example: E-Commerce Order Processing

```json
{
  "name": "CompleteOrderSaga",
  "description": "Full order processing with multiple services",
  "type": "Orchestration",
  "correlationId": "order-789",
  "timeout": "00:10:00",
  "context": {
    "orderId": "order-789",
    "customerId": "customer-456",
    "totalAmount": "250.00"
  },
  "steps": [
    {
      "name": "ValidateCustomer",
      "serviceName": "CustomerService",
      "actionType": "http.get.customer",
      "actionPayload": "{\"url\":\"http://customer-service/api/customers/customer-456/validate\"}",
      "maxRetries": 2
    },
    {
      "name": "ReserveInventory",
      "serviceName": "InventoryService",
      "actionType": "http.post.inventory",
      "actionPayload": "{\"url\":\"http://inventory-service/api/inventory/reserve\",\"body\":\"{\\\"items\\\":[{\\\"sku\\\":\\\"ABC123\\\",\\\"quantity\\\":1}]}\"}",
      "compensationActionType": "http.post.inventory",
      "compensationPayload": "{\"url\":\"http://inventory-service/api/inventory/release\",\"body\":\"{\\\"reservationId\\\":\\\"{{reservation_id}}\\\"}\"}",
      "maxRetries": 3
    },
    {
      "name": "ChargePayment",
      "serviceName": "PaymentService",
      "actionType": "http.post.payment",
      "actionPayload": "{\"url\":\"http://payment-service/api/payments/charge\",\"body\":\"{\\\"amount\\\":250.00,\\\"customerId\\\":\\\"customer-456\\\"}\"}",
      "compensationActionType": "http.post.payment",
      "compensationPayload": "{\"url\":\"http://payment-service/api/payments/refund\",\"body\":\"{\\\"transactionId\\\":\\\"{{transaction_id}}\\\"}\"}",
      "maxRetries": 3,
      "timeout": "00:03:00"
    },
    {
      "name": "CreateShipment",
      "serviceName": "ShippingService",
      "actionType": "http.post.shipping",
      "actionPayload": "{\"url\":\"http://shipping-service/api/shipments/create\",\"body\":\"{\\\"orderId\\\":\\\"order-789\\\"}\"}",
      "compensationActionType": "http.delete.shipping",
      "compensationPayload": "{\"url\":\"http://shipping-service/api/shipments/{{shipment_id}}\"}",
      "maxRetries": 2
    },
    {
      "name": "SendOrderConfirmation",
      "serviceName": "NotificationService",
      "actionType": "rabbitmq.publish.notification-exchange.email.send",
      "actionPayload": "{\"to\":\"customer@example.com\",\"template\":\"order-confirmation\",\"data\":{\"orderId\":\"order-789\"}}",
      "maxRetries": 2
    },
    {
      "name": "UpdateOrderStatus",
      "serviceName": "OrderService",
      "actionType": "http.put.order",
      "actionPayload": "{\"url\":\"http://order-service/api/orders/order-789/status\",\"body\":\"{\\\"status\\\":\\\"confirmed\\\"}\"}",
      "maxRetries": 3
    }
  ]
}
```

## 🔧 Supported Action Types

### HTTP Actions
- `http.get.{serviceName}` - HTTP GET request
- `http.post.{serviceName}` - HTTP POST request
- `http.put.{serviceName}` - HTTP PUT request
- `http.delete.{serviceName}` - HTTP DELETE request

### RabbitMQ Actions
- `rabbitmq.publish.{exchange}.{routingKey}` - Publish message to RabbitMQ

## 📚 Saga States

- **Created**: Saga has been created but not started
- **Running**: Saga is executing steps
- **Completed**: All steps completed successfully
- **Compensating**: A step failed, rolling back
- **Compensated**: Successfully rolled back
- **Failed**: Compensation failed
- **Aborted**: Manually aborted

## 🎯 Best Practices

1. **Always define compensation actions** for critical operations
2. **Set appropriate timeouts** for each step
3. **Use correlation IDs** for tracing across services
4. **Monitor saga status** regularly
5. **Handle idempotency** in your services
6. **Log all saga events** for debugging

## 🔍 Troubleshooting

### Check Saga Logs
```bash
GET /api/saga/{sagaId}
```

### View Failed Steps
```json
{
  "steps": [
    {
      "status": "Failed",
      "errorMessage": "Connection timeout to inventory service",
      "retryAttempts": 3,
      "maxRetries": 3
    }
  ]
}
```

### Retry After Fixing Issues
```bash
POST /api/saga/{sagaId}/steps/{stepId}/retry
```
