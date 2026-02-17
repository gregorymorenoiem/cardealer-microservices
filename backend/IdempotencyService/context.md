# IdempotencyService - Context Documentation

## 📋 INFORMACIÓN GENERAL

- **Nombre del Servicio:** IdempotencyService
- **Puerto en Desarrollo:** 5017
- **Estado:** ⚠️ **SOLO DESARROLLO LOCAL**
- **Backend:** Redis
- **Base de Datos:** N/A (usa Redis para tracking)
- **Imagen Docker:** Local only

### Propósito
Servicio de idempotencia para garantizar que operaciones críticas (pagos, creación de recursos) no se ejecuten múltiples veces. Previene duplicados causados por retries, doble-click, etc.

---

## 🏗️ ARQUITECTURA

```
IdempotencyService/
├── IdempotencyService.Api/
│   ├── Controllers/
│   │   └── IdempotencyController.cs
│   └── Program.cs
├── IdempotencyService.Application/
│   └── Services/
│       └── IdempotencyManager.cs
├── IdempotencyService.Domain/
│   ├── Entities/
│   │   └── IdempotencyKey.cs
│   └── Enums/
│       └── IdempotencyStatus.cs
└── IdempotencyService.Infrastructure/
    └── Redis/
        └── RedisIdempotencyStore.cs
```

---

## 📦 ENTIDADES

### IdempotencyKey
```csharp
public class IdempotencyKey
{
    public string Key { get; set; }                 // Unique idempotency key
    public string Operation { get; set; }           // "CreatePayment", "CreateVehicle"
    public IdempotencyStatus Status { get; set; }   // Pending, Completed, Failed
    public string? ResponseData { get; set; }       // Cached response
    public int? StatusCode { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }         // TTL de 24h típicamente
}
```

### IdempotencyStatus Enum
```csharp
public enum IdempotencyStatus
{
    Pending = 0,        // Request en proceso
    Completed = 1,      // Exitoso, response cacheado
    Failed = 2          // Falló, se puede reintentar
}
```

---

## 📡 ENDPOINTS API

#### POST `/api/idempotency/check`
Verificar si una operación ya fue ejecutada.

**Request:**
```json
{
  "idempotencyKey": "pay_abc123",
  "operation": "CreatePayment"
}
```

**Response (200 OK) - Ya ejecutado:**
```json
{
  "status": "Completed",
  "responseData": "{\"paymentId\": \"...\", \"status\": \"Succeeded\"}",
  "statusCode": 201,
  "executedAt": "2026-01-07T10:30:00Z"
}
```

**Response (404 Not Found) - Primera vez:**
```json
{
  "status": "NotFound",
  "message": "Idempotency key not found, proceed with operation"
}
```

#### POST `/api/idempotency/record`
Registrar resultado de operación.

**Request:**
```json
{
  "idempotencyKey": "pay_abc123",
  "operation": "CreatePayment",
  "status": "Completed",
  "responseData": "{\"paymentId\": \"...\"}",
  "statusCode": 201,
  "ttlSeconds": 86400
}
```

#### DELETE `/api/idempotency/{key}`
Eliminar key (para testing o cleanup manual).

---

## 🔄 FLUJO DE USO

### En el Servicio Consumidor

```csharp
public async Task<IActionResult> CreatePayment(
    [FromBody] CreatePaymentRequest request,
    [FromHeader(Name = "Idempotency-Key")] string idempotencyKey)
{
    // 1. Verificar si ya fue ejecutado
    var check = await _idempotencyService.CheckAsync(idempotencyKey, "CreatePayment");
    
    if (check.Status == IdempotencyStatus.Completed)
    {
        // Retornar respuesta cacheada
        return StatusCode(check.StatusCode, check.ResponseData);
    }
    
    if (check.Status == IdempotencyStatus.Pending)
    {
        // Otro request en proceso, esperar
        return StatusCode(409, "Request already in progress");
    }
    
    // 2. Marcar como pending
    await _idempotencyService.RecordAsync(
        idempotencyKey, 
        "CreatePayment", 
        IdempotencyStatus.Pending
    );
    
    try
    {
        // 3. Ejecutar operación
        var payment = await _paymentService.CreateAsync(request);
        
        // 4. Guardar resultado
        await _idempotencyService.RecordAsync(
            idempotencyKey,
            "CreatePayment",
            IdempotencyStatus.Completed,
            JsonSerializer.Serialize(payment),
            statusCode: 201,
            ttlSeconds: 86400
        );
        
        return CreatedAtAction(nameof(GetPayment), new { id = payment.Id }, payment);
    }
    catch (Exception ex)
    {
        // 5. Marcar como fallido
        await _idempotencyService.RecordAsync(
            idempotencyKey,
            "CreatePayment",
            IdempotencyStatus.Failed
        );
        
        throw;
    }
}
```

---

## 💡 CASOS DE USO

### 1. Pagos (Crítico)
Evitar cobrar múltiples veces al usuario.

```http
POST /api/payments
Idempotency-Key: pay_20260107_user123_abc
```

### 2. Creación de Recursos
Evitar crear vehículo duplicado por doble-click.

```http
POST /api/vehicles
Idempotency-Key: vehicle_create_20260107_user123
```

### 3. Webhooks
Evitar procesar mismo webhook múltiples veces.

```http
POST /api/webhooks/stripe
Idempotency-Key: evt_stripe_abc123
```

---

## 🔧 GENERACIÓN DE KEYS

### Por Cliente (Frontend)
```typescript
function generateIdempotencyKey(operation: string): string {
  const uuid = crypto.randomUUID();
  const timestamp = Date.now();
  return `${operation}_${timestamp}_${uuid}`;
}

// Uso
const key = generateIdempotencyKey('create_payment');
// create_payment_1704621600000_abc123-def456-...
```

### Por Servidor
```csharp
public string GenerateIdempotencyKey(string operation, Guid userId)
{
    var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    var random = Guid.NewGuid().ToString("N").Substring(0, 8);
    return $"{operation}_{timestamp}_{userId}_{random}";
}
```

---

## ⚙️ CONFIGURACIÓN

```json
{
  "Redis": {
    "ConnectionString": "localhost:6379",
    "KeyPrefix": "idempotency:"
  },
  "Idempotency": {
    "DefaultTtlSeconds": 86400,
    "MaxTtlSeconds": 604800
  }
}
```

---

## 📝 BEST PRACTICES

### TTL (Time To Live)
- **Pagos**: 7 días
- **Creación de recursos**: 24 horas
- **Webhooks**: 30 días

### Key Format
```
{operation}_{timestamp}_{userId}_{random}
```

### HTTP Header
Usar header estándar: `Idempotency-Key`

### Response Codes
- `200/201`: Primera ejecución exitosa
- `200/201`: Ejecución duplicada (retornar mismo response)
- `409 Conflict`: Request en progreso (otro cliente procesando)
- `422 Unprocessable Entity`: Key inválido

---

## 🚀 ALTERNATIVAS

### Implementación Directa
En lugar de servicio separado, implementar en cada servicio:

```csharp
[ServiceFilter(typeof(IdempotencyFilter))]
public async Task<IActionResult> CreatePayment(...)
{
    // Idempotency handled by filter
}
```

### Stripe Approach
Stripe usa idempotency keys en todos sus endpoints:

```http
POST https://api.stripe.com/v1/charges
Idempotency-Key: abc123
```

---

**Estado:** Solo desarrollo - Implementar directamente en servicios críticos  
**Versión:** 1.0.0
