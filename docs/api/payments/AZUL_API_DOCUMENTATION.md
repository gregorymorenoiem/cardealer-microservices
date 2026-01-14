# 🔐 API AZUL - Banco Popular RD

**Documentación oficial:** https://desarrolladores.azul.com.do  
**Versión:** 2.0  
**Actualizado:** Enero 2026

---

## 📋 Tabla de Contenidos

1. [Autenticación](#autenticación)
2. [Endpoints principales](#endpoints-principales)
3. [Métodos de pago](#métodos-de-pago)
4. [Transacciones](#transacciones)
5. [Webhooks](#webhooks)
6. [Manejo de errores](#manejo-de-errores)
7. [Ejemplos de código](#ejemplos-de-código)

---

## 🔑 Autenticación

### Credenciales Requeridas

Obten tus credenciales en:

- **Portal:** https://desarrolladores.azul.com.do
- **Sandbox:** Para pruebas sin dinero real
- **Production:** Para transacciones reales

### Parámetros de Autenticación

```
StoreId      (String)  - Identificador único de tu comercio
ApiKey       (String)  - Clave secreta de tu cuenta
AuthHash     (String)  - Hash SHA-256 de StoreId+ApiKey+timestamp
```

### Método de Autenticación

**Header HTTP:**

```
Authorization: Bearer {AuthHash}
X-Store-Id: {StoreId}
Content-Type: application/json
```

**Cálculo de AuthHash:**

```
AuthHash = SHA256(StoreId + ApiKey + UnixTimestamp)
```

### Ejemplo en C#

```csharp
using System;
using System.Security.Cryptography;
using System.Text;

public class AzulAuth
{
    public static string GenerateAuthHash(string storeId, string apiKey)
    {
        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        string toHash = $"{storeId}{apiKey}{timestamp}";

        using (var sha256 = SHA256.Create())
        {
            byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(toHash));
            return Convert.ToHexString(hashedBytes).ToLower();
        }
    }
}
```

---

## 🌐 Endpoints Principales

### URL Base

- **Sandbox:** `https://api.azul.com.do/api/1.0/sandbox/`
- **Production:** `https://api.azul.com.do/api/1.0/`

### Health Check

```
GET /transactions/health
Headers:
  - Authorization: Bearer {AuthHash}
  - X-Store-Id: {StoreId}

Response:
{
  "status": "ok",
  "timestamp": "2026-01-14T10:30:00Z",
  "version": "2.0"
}
```

---

## 💳 Métodos de Pago Soportados

| Método              | Código         | Descripción                        |
| ------------------- | -------------- | ---------------------------------- |
| **Tarjeta Crédito** | CREDIT_CARD    | Visa, Mastercard, American Express |
| **Tarjeta Débito**  | DEBIT_CARD     | Tarjetas de débito dominicanas     |
| **ACH Transfer**    | ACH            | Transferencia bancaria             |
| **Mobile Payment**  | MOBILE_PAYMENT | Dinero Móvil (Orange Money, Claro) |
| **E-wallet**        | E_WALLET       | Billeteras electrónicas            |

---

## 📊 Transacciones

### 1. Crear Pago (Sale)

**POST** `/transactions/sale`

**Request:**

```json
{
  "amount": 1999.99,
  "currency": "DOP",
  "reference": "INV-2026-001",
  "description": "Purchase of vehicle parts",
  "cardNumber": "4111111111111111",
  "expirationDate": "12/25",
  "cvv": "123",
  "cardholderName": "JUAN PEREZ",
  "email": "juan@example.com",
  "ipAddress": "192.168.1.1",
  "customData": {
    "dealerId": "dealer-123",
    "vehicleId": "vehicle-456"
  }
}
```

**Response:**

```json
{
  "transactionId": "TXN-2026-001-ABC123",
  "status": "APPROVED",
  "amount": 1999.99,
  "currency": "DOP",
  "reference": "INV-2026-001",
  "authorizationCode": "123456",
  "timestamp": "2026-01-14T10:30:00Z",
  "responseCode": "00",
  "responseMessage": "Transacción aprobada",
  "cardBrand": "VISA",
  "last4": "1111",
  "processingFee": 50.0,
  "netAmount": 1949.99
}
```

### 2. Autorizar Pago (Pre-authorization)

**POST** `/transactions/authorize`

**Request:**

```json
{
  "amount": 2500.0,
  "currency": "DOP",
  "reference": "AUTH-2026-001",
  "description": "Pre-authorization for subscription",
  "cardNumber": "4111111111111111",
  "expirationDate": "12/25",
  "cvv": "123"
}
```

**Response:**

```json
{
  "transactionId": "TXN-2026-002-ABC124",
  "status": "AUTHORIZED",
  "amount": 2500.0,
  "authorizationCode": "654321",
  "responseCode": "00"
}
```

### 3. Capturar Pago Autorizado

**POST** `/transactions/{transactionId}/capture`

**Request:**

```json
{
  "amount": 2500.0,
  "description": "Captura de autorización"
}
```

**Response:**

```json
{
  "transactionId": "TXN-2026-002-ABC124",
  "status": "CAPTURED",
  "amount": 2500.0,
  "responseCode": "00"
}
```

### 4. Revertir (Void) Autorización

**POST** `/transactions/{transactionId}/void`

**Request:**

```json
{
  "reason": "Customer requested cancellation"
}
```

**Response:**

```json
{
  "transactionId": "TXN-2026-002-ABC124",
  "status": "VOIDED",
  "responseCode": "00",
  "responseMessage": "Autorización revocada"
}
```

### 5. Reembolso (Refund)

**POST** `/transactions/{transactionId}/refund`

**Request:**

```json
{
  "amount": 1500.0,
  "reason": "Partial refund - customer request",
  "description": "Reembolso parcial"
}
```

**Response:**

```json
{
  "refundId": "REF-2026-001-ABC125",
  "originalTransactionId": "TXN-2026-001-ABC123",
  "amount": 1500.0,
  "status": "APPROVED",
  "responseCode": "00",
  "timestamp": "2026-01-14T10:35:00Z"
}
```

### 6. Obtener Estado de Transacción

**GET** `/transactions/{transactionId}`

**Response:**

```json
{
  "transactionId": "TXN-2026-001-ABC123",
  "status": "CAPTURED",
  "amount": 1999.99,
  "currency": "DOP",
  "reference": "INV-2026-001",
  "authorizationCode": "123456",
  "cardBrand": "VISA",
  "last4": "1111",
  "timestamp": "2026-01-14T10:30:00Z",
  "responseCode": "00",
  "processedAt": "2026-01-14T10:31:00Z"
}
```

### 7. Listar Transacciones

**GET** `/transactions?filter[status]=APPROVED&filter[startDate]=2026-01-01&filter[endDate]=2026-01-31&page=1&pageSize=50`

**Response:**

```json
{
  "transactions": [
    {
      "transactionId": "TXN-2026-001-ABC123",
      "status": "APPROVED",
      "amount": 1999.99,
      "reference": "INV-2026-001",
      "timestamp": "2026-01-14T10:30:00Z"
    }
  ],
  "pagination": {
    "page": 1,
    "pageSize": 50,
    "total": 150,
    "totalPages": 3
  }
}
```

---

## 🔄 Transacciones Recurrentes (Suscripciones)

### 1. Crear Suscripción

**POST** `/subscriptions`

**Request:**

```json
{
  "customerId": "CUST-2026-001",
  "amount": 500.0,
  "currency": "DOP",
  "frequency": "MONTHLY",
  "startDate": "2026-02-01",
  "endDate": "2026-12-31",
  "description": "Dealer monthly subscription - Pro Plan",
  "cardNumber": "4111111111111111",
  "expirationDate": "12/25",
  "cvv": "123"
}
```

**Frequencies disponibles:**

- `DAILY`
- `WEEKLY`
- `BIWEEKLY`
- `MONTHLY`
- `QUARTERLY`
- `SEMIANNUALLY`
- `ANNUALLY`

**Response:**

```json
{
  "subscriptionId": "SUB-2026-001-ABC126",
  "customerId": "CUST-2026-001",
  "status": "ACTIVE",
  "amount": 500.0,
  "frequency": "MONTHLY",
  "nextChargDate": "2026-02-01",
  "responseCode": "00"
}
```

### 2. Modificar Suscripción

**PUT** `/subscriptions/{subscriptionId}`

**Request:**

```json
{
  "amount": 750.0,
  "frequency": "MONTHLY",
  "status": "ACTIVE"
}
```

### 3. Cancelar Suscripción

**DELETE** `/subscriptions/{subscriptionId}`

**Response:**

```json
{
  "subscriptionId": "SUB-2026-001-ABC126",
  "status": "CANCELLED",
  "cancelledAt": "2026-01-14T10:40:00Z",
  "responseCode": "00"
}
```

---

## 📱 Tokenización de Tarjetas

### 1. Crear Token de Tarjeta

**POST** `/tokens/cards`

**Request:**

```json
{
  "cardNumber": "4111111111111111",
  "expirationDate": "12/25",
  "cvv": "123",
  "cardholderName": "JUAN PEREZ"
}
```

**Response:**

```json
{
  "tokenId": "TOK-2026-001-ABC127",
  "cardBrand": "VISA",
  "last4": "1111",
  "expirationDate": "12/25",
  "status": "ACTIVE",
  "createdAt": "2026-01-14T10:45:00Z"
}
```

### 2. Usar Token en Transacción

**POST** `/transactions/sale`

**Request:**

```json
{
  "amount": 1999.99,
  "currency": "DOP",
  "reference": "INV-2026-002",
  "tokenId": "TOK-2026-001-ABC127"
}
```

---

## 🪝 Webhooks

### Eventos Disponibles

```
transaction.approved
transaction.declined
transaction.pending
transaction.authorized
transaction.captured
transaction.voided
transaction.refunded
subscription.created
subscription.charged
subscription.failed
subscription.cancelled
```

### Registrar Webhook

**POST** `/webhooks`

**Request:**

```json
{
  "url": "https://api.okla.com.do/webhooks/azul",
  "events": [
    "transaction.approved",
    "transaction.declined",
    "subscription.charged"
  ],
  "active": true
}
```

### Recibir Webhook

**POST** (desde Azul a tu servidor)

**Headers:**

```
X-Azul-Signature: SHA256 hash del payload
X-Azul-Timestamp: timestamp del evento
```

**Body:**

```json
{
  "eventId": "EVT-2026-001-ABC128",
  "eventType": "transaction.approved",
  "timestamp": "2026-01-14T10:50:00Z",
  "data": {
    "transactionId": "TXN-2026-001-ABC123",
    "status": "APPROVED",
    "amount": 1999.99,
    "reference": "INV-2026-001"
  }
}
```

### Validar Webhook

```csharp
public bool ValidateWebhookSignature(
    string payload,
    string signature,
    string apiKey)
{
    using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(apiKey)))
    {
        byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        string expectedSignature = Convert.ToHexString(hash).ToLower();

        return signature.Equals(expectedSignature,
            StringComparison.OrdinalIgnoreCase);
    }
}
```

---

## ⚠️ Manejo de Errores

### Códigos de Respuesta

| Código | Significado                      | Acción                             |
| ------ | -------------------------------- | ---------------------------------- |
| `00`   | Aprobado                         | Transacción completada             |
| `01`   | Rechazado - Tarjeta vencida      | Solicitar nueva tarjeta            |
| `02`   | Rechazado - Fondos insuficientes | Solicitar otro método de pago      |
| `03`   | Rechazado - Tarjeta no válida    | Validar datos                      |
| `04`   | Rechazado - Límite excedido      | Intentar monto menor               |
| `05`   | Rechazado - CVV incorrecto       | Reintentar con CVV correcto        |
| `10`   | Error de autenticación           | Verificar credenciales             |
| `20`   | Error de servidor                | Reintentar                         |
| `30`   | Timeout                          | Reintentar con backoff exponencial |

### Estructura de Error

```json
{
  "status": "ERROR",
  "errorCode": "03",
  "errorMessage": "Tarjeta no válida",
  "details": {
    "field": "cardNumber",
    "reason": "Invalid card number"
  },
  "timestamp": "2026-01-14T10:55:00Z"
}
```

---

## 💻 Ejemplos de Código

### C# - Crear Pago

```csharp
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

public class AzulPaymentService
{
    private readonly string _storeId = "your-store-id";
    private readonly string _apiKey = "your-api-key";
    private readonly HttpClient _httpClient;

    public AzulPaymentService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<AzulTransactionResponse> CreatePaymentAsync(
        decimal amount,
        string cardNumber,
        string expirationDate,
        string cvv)
    {
        var request = new
        {
            amount = amount,
            currency = "DOP",
            reference = $"INV-{DateTime.Now:yyyyMMddHHmmss}",
            cardNumber = cardNumber,
            expirationDate = expirationDate,
            cvv = cvv,
            cardholderName = "CUSTOMER NAME"
        };

        var payload = JsonSerializer.Serialize(request);
        var authHash = GenerateAuthHash();

        var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            "https://api.azul.com.do/api/1.0/transactions/sale")
        {
            Content = new StringContent(payload,
                System.Text.Encoding.UTF8,
                "application/json")
        };

        httpRequest.Headers.Add("Authorization", $"Bearer {authHash}");
        httpRequest.Headers.Add("X-Store-Id", _storeId);

        var response = await _httpClient.SendAsync(httpRequest);
        var content = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<AzulTransactionResponse>(content);
    }

    private string GenerateAuthHash()
    {
        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        string toHash = $"{_storeId}{_apiKey}{timestamp}";

        using (var sha256 = System.Security.Cryptography.SHA256.Create())
        {
            byte[] hashedBytes = sha256.ComputeHash(
                System.Text.Encoding.UTF8.GetBytes(toHash));
            return Convert.ToHexString(hashedBytes).ToLower();
        }
    }
}

public class AzulTransactionResponse
{
    public string TransactionId { get; set; }
    public string Status { get; set; }
    public decimal Amount { get; set; }
    public string ResponseCode { get; set; }
    public string AuthorizationCode { get; set; }
    public string ResponseMessage { get; set; }
}
```

### C# - Procesar Webhook

```csharp
[HttpPost("webhooks/azul")]
public async Task<IActionResult> HandleAzulWebhook(
    [FromHeader(Name = "X-Azul-Signature")] string signature,
    [FromBody] JsonElement body)
{
    string payload = body.GetRawText();

    // Validar firma
    if (!ValidateSignature(payload, signature))
    {
        return Unauthorized("Invalid signature");
    }

    var eventData = JsonSerializer.Deserialize<AzulWebhookEvent>(payload);

    // Procesar evento según tipo
    switch (eventData.EventType)
    {
        case "transaction.approved":
            await _paymentService.ProcessApprovedTransaction(eventData);
            break;
        case "transaction.declined":
            await _paymentService.ProcessDeclinedTransaction(eventData);
            break;
        case "subscription.charged":
            await _subscriptionService.ProcessSubscriptionCharge(eventData);
            break;
    }

    return Ok();
}
```

---

## 📚 Límites y Restricciones

| Límite                            | Valor          |
| --------------------------------- | -------------- |
| **Monto máximo por transacción**  | RD$ 999,999.99 |
| **Monto mínimo por transacción**  | RD$ 1.00       |
| **Transacciones por minuto**      | 100            |
| **Transacciones por hora**        | 5,000          |
| **Tiempo de expiración de token** | 30 minutos     |
| **Reintentos automáticos**        | 3 intentos     |

---

## 🔗 Recursos Útiles

- **Portal de Desarrollo:** https://desarrolladores.azul.com.do
- **Dashboard:** https://dashboard.azul.com.do
- **Sandbox API:** https://api.azul.com.do/api/1.0/sandbox/
- **Status Page:** https://status.azul.com.do
- **Support:** developer-support@azul.com.do

---

**Última actualización:** Enero 2026  
**Versión API:** 2.0
