# 💳 AZUL Payment Service - Matriz de Procesos

> **Servicio:** AzulPaymentService  
> **Puerto:** 5025  
> **Última actualización:** Enero 25, 2026  
> **Estado:** 🟢 ACTIVO  
> **Proveedor:** AZUL (Banco Popular Dominicano)  
> **Estado de Implementación:** ✅ 100% Backend | ✅ 95% UI

---

## ⚠️ AUDITORÍA DE ACCESO UI (Enero 25, 2026)

| Proceso                   | Backend               | UI Access             | Observación      |
| ------------------------- | --------------------- | --------------------- | ---------------- |
| AZUL-PAY-001 Checkout     | ✅ AzulPaymentService | ✅ CheckoutPage       | Formulario Azul  |
| AZUL-PAY-002 Tokenización | ✅ AzulPaymentService | ✅ CheckoutPage       | Tarjetas locales |
| AZUL-SUB-001 Recurrencia  | ✅ AzulPaymentService | ✅ DealerCheckoutPage | Cobros mensuales |
| AZUL-WH-001 Webhooks      | ✅ AzulPaymentService | N/A                   | Backend only     |

### Rutas UI Existentes ✅

- `/checkout` → CheckoutPage (AZUL como método único)
- `/dealer/checkout` → DealerCheckoutPage (suscripción con Azul)
- `/dealer/billing` → BillingHistoryPage (historial)

### Rutas UI Faltantes 🔴

- Ninguna significativa - Azul completamente integrado

**Verificación Backend:** AzulPaymentService existe en `/backend/AzulPaymentService/` ✅

---

## 📊 Resumen de Implementación

| Componente  | Total | Implementado | Pendiente | Estado  |
| ----------- | ----- | ------------ | --------- | ------- |
| Controllers | 2     | 2            | 0         | ✅ 100% |
| AZUL-PAY-\* | 8     | 4            | 4         | 🟡 50%  |
| AZUL-SUB-\* | 4     | 2            | 2         | 🟡 50%  |
| AZUL-WH-\*  | 5     | 1            | 4         | 🟡 20%  |
| Frontend    | 3     | 3            | 0         | ✅ 100% |
| Tests       | 12    | 6            | 6         | 🟡 50%  |

**Leyenda:** ✅ Implementado + Tested | 🟢 Implementado | 🟡 En Progreso | 🔴 Pendiente

> ⚠️ **IMPORTANTE: Modelo de Negocio**
>
> OKLA es el **COMERCIANTE** (Merchant) que recibe pagos.
> Los dealers son **CLIENTES** que pagan a OKLA por suscripciones.
>
> ```
> ┌──────────────────────────────────────────────────────────────────┐
> │                      FLUJO DE PAGO                               │
> ├──────────────────────────────────────────────────────────────────┤
> │                                                                  │
> │   DEALER ───[Paga RD$2,900-14,900/mes]───> OKLA (Merchant)      │
> │      │                                        │                  │
> │      │ Tarjeta Visa/MC                       │ Cuenta Azul      │
> │      │                                        │ de OKLA          │
> │      ▼                                        ▼                  │
> │   ┌─────────────────────────────────────────────────────────┐   │
> │   │              AZUL (Banco Popular)                       │   │
> │   │   - Valida tarjeta                                      │   │
> │   │   - Procesa cobro                                       │   │
> │   │   - Deposita a OKLA (24-48h)                           │   │
> │   └─────────────────────────────────────────────────────────┘   │
> │                                                                  │
> └──────────────────────────────────────────────────────────────────┘
> ```

---

## 1. Información General

### 1.1 Descripción

Integración con la pasarela de pagos AZUL del Banco Popular Dominicano para procesar tarjetas de crédito y débito dominicanas. Es la opción preferida para clientes locales por tener menor comisión y depósitos más rápidos.

### 1.2 Dependencias

| Servicio            | Propósito                 |
| ------------------- | ------------------------- |
| BillingService      | Gestión de facturación    |
| UserService         | Información del cliente   |
| NotificationService | Confirmaciones de pago    |
| AuditService        | Registro de transacciones |

### 1.3 Características AZUL

- **Comisión:** ~2.5% (competitiva en el mercado)
- **Depósito:** 24-48 horas (más rápido)
- **Moneda:** DOP (Pesos Dominicanos)
- **Tarjetas:** Visa, MasterCard, American Express
- **3D Secure:** Soportado

---

## 2. Endpoints API

### 2.1 PaymentsController

| Método | Endpoint                        | Descripción         | Auth | Roles        |
| ------ | ------------------------------- | ------------------- | ---- | ------------ |
| `POST` | `/api/payments/charge`          | Procesar cobro      | ✅   | User         |
| `GET`  | `/api/payments/{transactionId}` | Obtener transacción | ✅   | User (owner) |
| `POST` | `/api/payments/refund`          | Procesar reembolso  | ✅   | Admin        |
| `GET`  | `/api/payments/health`          | Health check        | ❌   | -            |

### 2.2 SubscriptionsController

| Método | Endpoint                             | Descripción         | Auth | Roles        |
| ------ | ------------------------------------ | ------------------- | ---- | ------------ |
| `POST` | `/api/subscriptions`                 | Crear suscripción   | ✅   | User         |
| `GET`  | `/api/subscriptions/{id}`            | Obtener suscripción | ✅   | User (owner) |
| `POST` | `/api/subscriptions/{id}/cancel`     | Cancelar            | ✅   | User (owner) |
| `POST` | `/api/subscriptions/{id}/reactivate` | Reactivar           | ✅   | User (owner) |

### 2.3 WebhooksController

| Método | Endpoint             | Descripción     | Auth | Roles |
| ------ | -------------------- | --------------- | ---- | ----- |
| `POST` | `/api/webhooks/azul` | Webhook de AZUL | ❌   | -     |

---

## 3. Entidades y Enums

### 3.0 Diagrama de Arquitectura AZUL

```
┌─────────────────────────────────────────────────────────────────────────────────────┐
│                 ARQUITECTURA AZUL (Banco Popular) - OKLA                   │
├─────────────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ┌───────────────────────────────────────────────────────────────────────────────┐  │
│  │                              DEALER/SELLER                               │  │
│  │  ┌─────────────────────────┐   Paga con tarjeta   ┌────────────────────────┐  │  │
│  │  │  Suscripción Dealer   │   dominicana       │  Publicación Individual │  │  │
│  │  │  $49-299/mes         │─────────────────▶│         $29              │  │  │
│  │  └─────────────────────────┘                   └────────────────────────┘  │  │
│  └───────────────────────────────────────────────────────────────────────────────┘  │
│                              │                                              │
│                              ▼                                              │
│  ┌───────────────────────────────────────────────────────────────────────────────┐  │
│  │                    AZULPAYMENTSERVICE (:5025)                            │  │
│  │                                                                         │  │
│  │  ┌─────────────────────────┐   ┌─────────────────────────────────────┐    │  │
│  │  │  PaymentsController     │   │  WebhooksController                   │    │  │
│  │  │  • POST /charge          │   │  • POST /callback/approved           │    │  │
│  │  │  • POST /refund          │   │  • POST /callback/declined           │    │  │
│  │  │  • GET /transaction/{id} │   │  • Validación por firma HMAC         │    │  │
│  │  └────────────┬────────────┘   └──────────────┬──────────────────────┘    │  │
│  │               │                              │                          │  │
│  │  ┌────────────┴─────────────────────────────┴───────────────────────┐    │  │
│  │  │                         AZUL HTTP CLIENT                          │    │  │
│  │  │  • Credenciales: MerchantId, AuthHash                              │    │  │
│  │  │  • Endpoint: https://pagos.azul.com.do/webservices/JSON           │    │  │
│  │  │  • Moneda: DOP (Pesos Dominicanos)                                │    │  │
│  │  └─────────────────────────────────────────────────────────────────┘    │  │
│  └───────────────────────────────────────────────────────────────────────────────┘  │
│                              │                                              │
│                              ▼                                              │
│  ┌───────────────────────────────────────────────────────────────────────────────┐  │
│  │                      AZUL - BANCO POPULAR DOMINICANO                    │  │
│  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  ┌───────────────┐    │  │
│  │  │ Autorización │  │Tokenización │  │  3D Secure  │  │  Depósito OKLA │    │  │
│  │  │   de cobro  │  │  de tarjeta │  │  (si req.)  │  │   24-48 hrs   │    │  │
│  │  └─────────────┘  └─────────────┘  └─────────────┘  └───────────────┘    │  │
│  └───────────────────────────────────────────────────────────────────────────────┘  │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────────────┘
```

### 3.0.1 Diagrama de Flujo de Pago AZUL

```
┌─────────────────────────────────────────────────────────────────────────────────────┐
│                    FLUJO DE PAGO AZUL (DOP)                                 │
├─────────────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│     CLIENTE           FRONTEND         BACKEND            AZUL             │
│    ─────────          ─────────         ─────────          ────────          │
│        │                 │                │                 │               │
│        │  1. Click Pagar │                │                 │               │
│        │──────────────▶│                │                 │               │
│        │                 │                │                 │               │
│        │  2. Ingresar    │                │                 │               │
│        │     tarjeta     │                │                 │               │
│        │──────────────▶│                │                 │               │
│        │                 │                │                 │               │
│        │                 │ 3. Tokenizar   │                 │               │
│        │                 │    tarjeta     │                 │               │
│        │                 │─────────────────────────────────▶│               │
│        │                 │◀─── token_tarjeta ───────────────────┤               │
│        │                 │                │                 │               │
│        │                 │ 4. POST /charge│                 │               │
│        │                 │──────────────▶│                 │               │
│        │                 │                │                 │               │
│        │                 │                │ 5. Verificar    │               │
│        │                 │                │    idempotencia │               │
│        │                 │                │                 │               │
│        │                 │                │ 6. Enviar cobro │               │
│        │                 │                │───────────────▶│               │
│        │                 │                │                 │               │
│        │                 │                │     ┌───────────────────┐       │
│        │                 │                │     │ 7. Validar tarjeta │       │
│        │                 │                │     │    con banco emisor │      │
│        │                 │                │     └─────────┬─────────┘       │
│        │                 │                │               │               │
│        │                 │                │◀── ResponseCode ─┘               │
│        │                 │                │    + AuthCode                  │
│        │                 │                │    + RRN                       │
│        │                 │                │                                │
│        │                 │     ┌───────────┴───────────────┐                    │
│        │                 │     │ 8. ResponseCode = "00"? │                    │
│        │                 │     └──────────┬───────────────┘                    │
│        │                 │              │                                  │
│        │              ┌──┼──────────────┴────────────────┐                   │
│        │              ▼ Sí                               ▼ No               │
│        │       ┌───────────────────┐              ┌───────────────────┐      │
│        │       │ 9. ✅ APROBADO     │              │ 9. ❌ RECHAZADO    │      │
│        │       │ - Guardar TX      │              │ - Registrar error │      │
│        │       │ - Generar NCF     │              │ - Sugerir acción  │      │
│        │       │ - Publicar evento │              └─────────┬─────────┘      │
│        │       │ - Enviar email    │                        │              │
│        │       └─────────┬─────────┘                        │              │
│        │               │                                  │              │
│        │◀──── 10. Resultado ──────────────────────────────────────┘              │
│        │                 │                │                                │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────────────┘
```

### 3.1 TransactionStatus (Enum)

```csharp
public enum TransactionStatus
{
    Pending = 0,         // Esperando procesamiento
    Approved = 1,        // Aprobada
    Declined = 2,        // Rechazada por banco
    Error = 3,           // Error técnico
    Voided = 4,          // Anulada
    Refunded = 5,        // Reembolsada
    PartialRefund = 6    // Reembolso parcial
}
```

### 3.2 PaymentMethod (Enum)

```csharp
public enum PaymentMethod
{
    CreditCard = 0,      // Tarjeta de crédito
    DebitCard = 1,       // Tarjeta de débito
    TokenizedCard = 2    // Tarjeta guardada
}
```

### 3.3 AzulTransaction (Entidad)

```csharp
public class AzulTransaction
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? DealerId { get; set; }

    // AZUL Response
    public string AzulOrderId { get; set; }
    public string RRN { get; set; }              // Reference Retrieval Number
    public string AuthorizationCode { get; set; }
    public string ResponseCode { get; set; }
    public string ResponseMessage { get; set; }

    // Monto
    public decimal Amount { get; set; }          // En DOP
    public decimal? RefundedAmount { get; set; }

    // Tarjeta (enmascarada)
    public string CardBrand { get; set; }        // Visa, MC, Amex
    public string CardLast4 { get; set; }        // Últimos 4 dígitos
    public string CardHolderName { get; set; }

    // Estado
    public TransactionStatus Status { get; set; }
    public PaymentMethod Method { get; set; }

    // Metadata
    public string Description { get; set; }
    public string CustomerEmail { get; set; }
    public string CustomerPhone { get; set; }
    public string IpAddress { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? RefundedAt { get; set; }
}
```

---

## 4. Procesos Detallados

### 4.1 AZUL-PAY-001: Procesar Cobro

| Campo       | Valor                     |
| ----------- | ------------------------- |
| **ID**      | AZUL-PAY-001              |
| **Nombre**  | Procesar Pago con AZUL    |
| **Actor**   | Usuario autenticado       |
| **Trigger** | POST /api/payments/charge |

#### Flujo del Proceso

| Paso | Acción                           | Sistema             | Validación             |
| ---- | -------------------------------- | ------------------- | ---------------------- |
| 1    | Usuario ingresa datos de tarjeta | Frontend            | Validación JS          |
| 2    | Tokenizar tarjeta en frontend    | AZUL SDK            | Token temporal         |
| 3    | Enviar request de cobro          | API                 | Con idempotency key    |
| 4    | Verificar idempotencia           | IdempotencyService  | Evitar duplicados      |
| 5    | Validar monto                    | AzulPaymentService  | > 0, formato correcto  |
| 6    | Construir request AZUL           | AzulPaymentService  | Con credenciales       |
| 7    | Llamar API AZUL                  | HTTP Client         | POST /webservices/JSON |
| 8    | Parsear respuesta                | AzulPaymentService  | ResponseCode           |
| 9    | Si approved                      | Check               | ResponseCode = "00"    |
| 10   | Guardar transacción              | Database            | Status = Approved      |
| 11   | Publicar evento                  | RabbitMQ            | payment.completed      |
| 12   | Enviar confirmación              | NotificationService | Email + SMS            |
| 13   | Retornar resultado               | Response            | Con detalles           |

#### Request

```json
{
  "userId": "uuid",
  "amount": 1500.0,
  "currency": "DOP",
  "cardToken": "azul_tok_xxxxx",
  "description": "Publicación vehículo - OKLA",
  "customerEmail": "cliente@email.com",
  "customerPhone": "+18095551234",
  "metadata": {
    "vehicleId": "uuid",
    "listingType": "Featured"
  }
}
```

#### Response (Exitoso)

```json
{
  "success": true,
  "data": {
    "transactionId": "uuid",
    "azulOrderId": "OKLA-20260121-001",
    "rrn": "123456789012",
    "authorizationCode": "123456",
    "status": "Approved",
    "amount": 1500.0,
    "currency": "DOP",
    "cardBrand": "Visa",
    "cardLast4": "4242",
    "message": "Transacción aprobada"
  }
}
```

#### Response (Rechazado)

```json
{
  "success": false,
  "data": {
    "transactionId": "uuid",
    "status": "Declined",
    "responseCode": "05",
    "message": "Tarjeta rechazada por el banco emisor",
    "suggestedAction": "Intente con otra tarjeta o contacte a su banco"
  }
}
```

---

### 4.2 AZUL-PAY-002: Procesar Reembolso

| Campo       | Valor                     |
| ----------- | ------------------------- |
| **ID**      | AZUL-PAY-002              |
| **Nombre**  | Procesar Reembolso        |
| **Actor**   | Admin/Sistema             |
| **Trigger** | POST /api/payments/refund |

#### Flujo del Proceso

| Paso | Acción                       | Sistema             | Validación               |
| ---- | ---------------------------- | ------------------- | ------------------------ |
| 1    | Admin solicita reembolso     | Dashboard           | TransactionId            |
| 2    | Obtener transacción original | Database            | Status = Approved        |
| 3    | Validar monto reembolso      | AzulPaymentService  | <= amount - refunded     |
| 4    | Construir request reverso    | AzulPaymentService  | Con RRN original         |
| 5    | Llamar API AZUL              | HTTP Client         | POST /webservices/JSON   |
| 6    | Si aprobado                  | Check               | ResponseCode = "00"      |
| 7    | Actualizar transacción       | Database            | RefundedAmount += amount |
| 8    | Publicar evento              | RabbitMQ            | payment.refunded         |
| 9    | Notificar cliente            | NotificationService | Email                    |
| 10   | Registrar auditoría          | AuditService        | Quién, cuándo, motivo    |

#### Request

```json
{
  "transactionId": "uuid",
  "amount": 1500.0,
  "reason": "Cancelación de publicación por solicitud del cliente"
}
```

---

### 4.3 AZUL-SUB-001: Crear Suscripción Recurrente

| Campo       | Valor                       |
| ----------- | --------------------------- |
| **ID**      | AZUL-SUB-001                |
| **Nombre**  | Crear Suscripción de Dealer |
| **Actor**   | Dealer                      |
| **Trigger** | POST /api/subscriptions     |

#### Flujo del Proceso

| Paso | Acción                    | Sistema            | Validación             |
| ---- | ------------------------- | ------------------ | ---------------------- |
| 1    | Dealer selecciona plan    | Frontend           | Starter/Pro/Enterprise |
| 2    | Ingresar datos de tarjeta | Frontend           | AZUL SDK tokenización  |
| 3    | Enviar request            | API                | Con plan seleccionado  |
| 4    | Tokenizar tarjeta         | AzulPaymentService | Para cobros futuros    |
| 5    | Procesar primer cobro     | AzulPaymentService | Cobro inicial          |
| 6    | Si exitoso                | Check              | Approved               |
| 7    | Crear suscripción         | Database           | Status = Active        |
| 8    | Calcular próximo cobro    | AzulPaymentService | +1 mes                 |
| 9    | Actualizar BillingService | HTTP               | Sincronizar plan       |
| 10   | Publicar evento           | RabbitMQ           | subscription.created   |

---

### 4.4 AZUL-WEBHOOK-001: Procesar Webhook

| Campo       | Valor                      |
| ----------- | -------------------------- |
| **ID**      | AZUL-WEBHOOK-001           |
| **Nombre**  | Procesar Notificación AZUL |
| **Actor**   | AZUL (Sistema externo)     |
| **Trigger** | POST /api/webhooks/azul    |

#### Flujo del Proceso

| Paso | Acción                  | Sistema            | Validación            |
| ---- | ----------------------- | ------------------ | --------------------- |
| 1    | AZUL envía notificación | Webhook            | Firma válida          |
| 2    | Verificar firma HMAC    | AzulPaymentService | SHA256 signature      |
| 3    | Parsear evento          | AzulPaymentService | JSON payload          |
| 4    | Identificar tipo evento | AzulPaymentService | payment, refund, etc. |
| 5    | Buscar transacción      | Database           | Por AzulOrderId       |
| 6    | Actualizar estado       | Database           | Según evento          |
| 7    | Publicar evento interno | RabbitMQ           | Correspondiente       |
| 8    | Retornar 200 OK         | Response           | Acknowledge           |

---

## 5. Códigos de Respuesta AZUL

### 5.1 Códigos Exitosos

| Código | Significado          |
| ------ | -------------------- |
| 00     | Transacción aprobada |

### 5.2 Códigos de Rechazo

| Código | Significado              | Acción Sugerida        |
| ------ | ------------------------ | ---------------------- |
| 05     | No autorizada            | Contactar banco emisor |
| 12     | Transacción inválida     | Verificar datos        |
| 14     | Tarjeta inválida         | Verificar número       |
| 41     | Tarjeta extraviada       | Contactar banco        |
| 43     | Tarjeta robada           | Contactar banco        |
| 51     | Fondos insuficientes     | Usar otra tarjeta      |
| 54     | Tarjeta expirada         | Actualizar tarjeta     |
| 55     | PIN incorrecto           | Reintentar             |
| 57     | Transacción no permitida | Contactar banco        |
| 61     | Excede límite            | Reducir monto          |
| 65     | Excede frecuencia        | Esperar e intentar     |
| 91     | Banco no disponible      | Reintentar más tarde   |
| 96     | Error de sistema         | Reintentar más tarde   |

---

## 6. Integración con AZUL API

### 6.1 Credenciales

```json
{
  "MerchantId": "39038540035",
  "MerchantName": "OKLA SRL",
  "MerchantType": "E-Commerce",
  "Channel": "EC",
  "Store": "39038540035",
  "Terminal": "1"
}
```

### 6.2 Request de Cobro AZUL

```json
{
  "Channel": "EC",
  "Store": "39038540035",
  "CardNumber": "tokenizado",
  "Expiration": "202812",
  "CVC": "tokenizado",
  "PosInputMode": "E-Commerce",
  "TrxType": "Sale",
  "Amount": "150000",
  "Itbis": "27000",
  "CurrencyPosCode": "$",
  "CustomerServicePhone": "8095551234",
  "OrderNumber": "OKLA-20260121-001",
  "ECommerceUrl": "https://okla.com.do",
  "CustomOrderId": "uuid"
}
```

### 6.3 URL Endpoints AZUL

| Ambiente   | URL                                                       |
| ---------- | --------------------------------------------------------- |
| Sandbox    | https://pruebas.azul.com.do/webservices/JSON/Default.aspx |
| Producción | https://pagos.azul.com.do/webservices/JSON/Default.aspx   |

---

## 7. Reglas de Negocio

### 7.1 Límites de Transacción

| Concepto                  | Límite        |
| ------------------------- | ------------- |
| Monto mínimo              | RD$ 100       |
| Monto máximo              | RD$ 500,000   |
| Transacciones/día/tarjeta | 10            |
| Monto/día/tarjeta         | RD$ 1,000,000 |

### 7.2 Política de Reembolsos

| Regla                | Valor                  |
| -------------------- | ---------------------- |
| Plazo máximo         | 30 días desde el cobro |
| Reembolso parcial    | Permitido              |
| Múltiples reembolsos | Hasta el monto total   |
| Tiempo procesamiento | 5-10 días hábiles      |

### 7.3 Reintentos

| Escenario           | Política                      |
| ------------------- | ----------------------------- |
| Error de red        | 3 reintentos con backoff      |
| Banco no disponible | Reintentar en 5 minutos       |
| Tarjeta rechazada   | No reintentar automáticamente |

---

## 8. Seguridad

### 8.1 Cumplimiento PCI DSS

| Requisito                | Implementación         |
| ------------------------ | ---------------------- |
| Tokenización             | AZUL SDK en frontend   |
| No almacenar CVV         | Solo token temporal    |
| Encriptación en tránsito | TLS 1.3                |
| Enmascaramiento          | Solo últimos 4 dígitos |
| Logs sanitizados         | Sin datos sensibles    |

### 8.2 Prevención de Fraude

| Control            | Implementación           |
| ------------------ | ------------------------ |
| Velocity checks    | Max transacciones por IP |
| 3D Secure          | Obligatorio > RD$ 5,000  |
| Device fingerprint | AZUL Risk Engine         |
| Geolocalización    | Validar país tarjeta     |

---

## 9. Eventos RabbitMQ

| Evento                      | Exchange         | Descripción        | Payload                           |
| --------------------------- | ---------------- | ------------------ | --------------------------------- |
| `payment.azul.initiated`    | `payment.events` | Pago iniciado      | `{ transactionId, amount }`       |
| `payment.azul.completed`    | `payment.events` | Pago completado    | `{ transactionId, status }`       |
| `payment.azul.declined`     | `payment.events` | Pago rechazado     | `{ transactionId, reason }`       |
| `payment.azul.refunded`     | `payment.events` | Reembolso          | `{ transactionId, refundAmount }` |
| `subscription.azul.created` | `payment.events` | Suscripción creada | `{ subscriptionId, plan }`        |
| `subscription.azul.renewed` | `payment.events` | Renovación exitosa | `{ subscriptionId }`              |
| `subscription.azul.failed`  | `payment.events` | Renovación fallida | `{ subscriptionId, reason }`      |

---

## 10. Métricas y Monitoreo

### 10.1 Prometheus Metrics

```
# Transacciones
azul_transactions_total{status="approved|declined|error"}

# Monto procesado
azul_amount_processed_total{currency="DOP"}

# Latencia API AZUL
azul_api_latency_seconds{quantile="0.5|0.95|0.99"}

# Tasa de aprobación
azul_approval_rate_ratio

# Reembolsos
azul_refunds_total
azul_refund_amount_total
```

### 10.2 Alertas

| Alerta          | Condición         | Severidad |
| --------------- | ----------------- | --------- |
| HighDeclineRate | >20% rechazos     | Warning   |
| AzulAPIDown     | No response 1 min | Critical  |
| HighRefundRate  | >5% reembolsos    | Warning   |
| FraudAlert      | Pattern detectado | Critical  |

---

## 11. Configuración

### 11.1 appsettings.json

```json
{
  "Azul": {
    "MerchantId": "${AZUL_MERCHANT_ID}",
    "MerchantName": "OKLA SRL",
    "AuthKey1": "${AZUL_AUTH_KEY1}",
    "AuthKey2": "${AZUL_AUTH_KEY2}",
    "Channel": "EC",
    "Store": "${AZUL_STORE}",
    "Terminal": "1",
    "BaseUrl": "https://pagos.azul.com.do/webservices/JSON/Default.aspx",
    "TimeoutSeconds": 30,
    "EnableTestMode": false
  },
  "Limits": {
    "MinAmount": 100,
    "MaxAmount": 500000,
    "MaxTransactionsPerDay": 10
  }
}
```

### 11.2 Secrets Requeridos

| Secret                | Descripción                    |
| --------------------- | ------------------------------ |
| `AZUL_MERCHANT_ID`    | ID del comercio                |
| `AZUL_AUTH_KEY1`      | Primera clave de autenticación |
| `AZUL_AUTH_KEY2`      | Segunda clave de autenticación |
| `AZUL_STORE`          | Código de tienda               |
| `AZUL_WEBHOOK_SECRET` | Secreto para validar webhooks  |

---

## 12. Integración con Dealer Onboarding

### 12.1 Uso Principal: Suscripciones de Dealers

El servicio AzulPaymentService se utiliza principalmente para cobrar suscripciones mensuales a los dealers:

| Plan         | Precio/Mes | Precio Early Bird | Límite Vehículos |
| ------------ | ---------- | ----------------- | ---------------- |
| Starter      | RD$2,900   | RD$2,320 (-20%)   | 10               |
| Professional | RD$5,900   | RD$4,720 (-20%)   | 50               |
| Enterprise   | RD$14,900  | RD$11,920 (-20%)  | Ilimitado        |

### 12.2 Flujo de Suscripción

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    FLUJO: Dealer Paga Suscripción                        │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  1. Dealer llega a /dealer/onboarding/payment-setup                     │
│  2. Frontend muestra plan seleccionado (precio en DOP)                  │
│  3. Si Early Bird: aplica 20% descuento + 90 días trial                 │
│  4. Dealer ingresa datos de tarjeta                                     │
│  5. Frontend valida (Luhn, fecha expiración, CVV)                       │
│  6. POST /api/azul-payment/subscriptions                                │
│  7. AzulPaymentService:                                                 │
│     a. Crea registro de suscripción                                     │
│     b. Si Early Bird: startDate = hoy + 90 días                         │
│     c. Llama API AZUL para tokenizar/cobro inicial                      │
│     d. Retorna subscriptionId                                           │
│  8. Frontend guarda IDs en dealer onboarding                            │
│  9. Redirect a status page                                              │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

### 12.3 Request de Suscripción

```json
{
  "userId": "dealer-uuid",
  "amount": 4720,
  "currency": "DOP",
  "frequency": "Monthly",
  "startDate": "2026-04-23T00:00:00Z",
  "planName": "Professional",
  "customerEmail": "dealer@empresa.com.do",
  "customerPhone": "8095550100",
  "invoiceReference": "DEALER-uuid-timestamp",
  "paymentMethod": "CreditCard",
  "cardNumber": "4111111111111111",
  "cardExpiryMonth": "12",
  "cardExpiryYear": "2028",
  "cardCVV": "123",
  "cardholderName": "JUAN CARLOS RODRIGUEZ"
}
```

### 12.4 Response de Suscripción

```json
{
  "subscriptionId": "sub_local_uuid",
  "azulSubscriptionId": "sub_azul_abc123",
  "status": "Active",
  "amount": 4720,
  "currency": "DOP",
  "frequency": "Monthly",
  "nextChargeDate": "2026-04-23T00:00:00Z",
  "startDate": "2026-01-23T00:00:00Z",
  "cardLastFour": "1111",
  "cardBrand": "Visa",
  "planName": "Professional",
  "createdAt": "2026-01-23T10:30:00Z"
}
```

### 12.5 Campos Guardados en DealerOnboarding

Después del pago exitoso, se actualizan estos campos en la entidad `DealerOnboarding`:

```csharp
// El dealer es CLIENTE, no comerciante
public string? AzulCustomerId { get; set; }      // = subscriptionId
public string? AzulSubscriptionId { get; set; }  // = azulSubscriptionId
public string? AzulCardToken { get; set; }       // Token para renovaciones
```

### 12.6 Renovación Automática

El sistema de suscripciones debe manejar renovaciones mensuales:

1. **Scheduler Job:** Ejecuta diariamente a las 6:00 AM
2. **Busca:** Suscripciones con nextChargeDate = hoy
3. **Para cada una:**
   - Intenta cobrar usando cardToken
   - Si éxito: actualiza nextChargeDate + 30 días
   - Si falla: marca como PastDue, envía notificación
4. **Retry:** 3 intentos en 5 días antes de suspender

---

## 📚 Referencias

- [AZUL Developer Portal](https://developer.azul.com.do/)
- [01-billing-service.md](01-billing-service.md) - Facturación principal
- [04-dealer-onboarding.md](../02-USUARIOS-DEALERS/04-dealer-onboarding.md) - Onboarding de dealers
