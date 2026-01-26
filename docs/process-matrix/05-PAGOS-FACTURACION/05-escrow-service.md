# 🔐 Escrow Service - Pagos en Garantía - Matriz de Procesos

> **Servicio:** EscrowService  
> **Puerto:** 5047  
> **Última actualización:** Enero 25, 2026  
> **Estado:** ❌ **DESCARTADO - NO APLICA AL MODELO DE NEGOCIO**  
> **Estado de Implementación:** 🚫 N/A - DESCARTADO

---

## ⚠️ AUDITORÍA DE ACCESO UI (Enero 25, 2026)

| Proceso   | Backend       | UI Access | Observación         |
| --------- | ------------- | --------- | ------------------- |
| ESCROW-\* | 🚫 Descartado | 🚫 N/A    | No aplica al modelo |

### Rutas UI Existentes ✅

- Ninguna - Servicio descartado

### Rutas UI Faltantes 🔴

- Ninguna - OKLA no procesa pagos entre compradores y vendedores

**Verificación Backend:** EscrowService **DESCARTADO** - OKLA es plataforma de publicidad, no marketplace transaccional.

---

## ⚠️ IMPORTANTE: SERVICIO DESCARTADO

### Razón de Descarte

Este servicio fue planificado asumiendo incorrectamente que OKLA sería un **marketplace transaccional** donde la plataforma procesaría pagos de vehículos entre compradores y vendedores.

### Modelo Correcto de OKLA

```
┌────────────────────────────────────────────────────────────────────────┐
│                   MODELO DE NEGOCIO OKLA                               │
├────────────────────────────────────────────────────────────────────────┤
│                                                                        │
│   OKLA ES UNA PLATAFORMA DE PUBLICIDAD, NO UN MARKETPLACE              │
│   ═══════════════════════════════════════════════════════              │
│                                                                        │
│   ✅ Dealers PAGAN a OKLA: Suscripción mensual RD$2,900-14,900        │
│   ✅ Sellers PAGAN a OKLA: Publicación única RD$1,500                  │
│                                                                        │
│   ❌ OKLA NO procesa pagos de vehículos                                │
│   ❌ OKLA NO retiene dinero de compradores                             │
│   ❌ OKLA NO transfiere dinero a vendedores                            │
│   ❌ OKLA NO cobra comisión por ventas                                 │
│                                                                        │
│   LA TRANSACCIÓN DEL VEHÍCULO OCURRE DIRECTAMENTE:                     │
│   Comprador ───[Paga en efectivo/banco]───> Vendedor                  │
│                                                                        │
│   OKLA solo conecta compradores con vendedores (publicidad)            │
│                                                                        │
└────────────────────────────────────────────────────────────────────────┘
```

### Por qué Escrow NO aplica

| Característica de Escrow     | Por qué NO aplica a OKLA                  |
| ---------------------------- | ----------------------------------------- |
| Retener dinero del comprador | OKLA no recibe dinero de compradores      |
| Liberar fondos al vendedor   | OKLA no transfiere dinero a vendedores    |
| Disputas de transacción      | Las disputas son entre comprador-vendedor |
| Comisión por transacción     | OKLA cobra suscripción fija, no comisión  |
| Verificación de entrega      | La entrega es entre comprador-vendedor    |

### Alternativas para Compradores

Si OKLA quisiera ofrecer protección a compradores en el futuro, las opciones serían:

1. **Partnership con servicio de escrow externo** (ej: Escrow.com)
2. **Verificación pre-compra** (inspección mecánica, historial)
3. **Garantía OKLA** (cobertura limitada post-venta)

Pero ninguna de estas implica que OKLA procese pagos de vehículos.

---

## 📊 Resumen de Implementación

| Componente                  | Total | Implementado | Pendiente | Estado                |
| --------------------------- | ----- | ------------ | --------- | --------------------- |
| **Controllers**             | 1     | 0            | 0         | ❌ DESCARTADO         |
| **ESC-CREATE-\*** (Crear)   | 4     | 0            | 0         | ❌ DESCARTADO         |
| **ESC-FUND-\*** (Fondos)    | 4     | 0            | 0         | ❌ DESCARTADO         |
| **ESC-REL-\*** (Liberar)    | 4     | 0            | 0         | ❌ DESCARTADO         |
| **ESC-DISP-\*** (Disputas)  | 4     | 0            | 0         | ❌ DESCARTADO         |
| **ESC-REF-\*** (Reembolsos) | 3     | 0            | 0         | ❌ DESCARTADO         |
| **TOTAL**                   | 20    | 0            | 0         | ❌ NO SE IMPLEMENTARÁ |

---

## 📚 Documentación Histórica (Solo Referencia)

> **NOTA:** El contenido a continuación se mantiene solo como referencia histórica.
> Este servicio NO se implementará.

---

## 1. Información General (DESCARTADO)

### 1.1 Descripción

Sistema de pagos en garantía (escrow) para transacciones de alto valor entre compradores y vendedores. El dinero se retiene hasta que ambas partes confirman la transacción satisfactoria, protegiendo tanto al comprador como al vendedor.

### 1.2 Flujo de Escrow

```
┌─────────────────────────────────────────────────────────────────────────┐
│                      FLUJO DE ESCROW                                     │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│   COMPRADOR                 OKLA (Escrow)              VENDEDOR          │
│   ─────────                ─────────────               ────────          │
│                                                                          │
│   1. Inicia compra                                                       │
│      └──────────────> 2. Crea Escrow Account                            │
│                              │                                           │
│   3. Deposita fondos         │                                           │
│      └──────────────> 4. Retiene dinero                                 │
│                              │                                           │
│                       5. Notifica pago ────────────>                    │
│                              │                                           │
│                              │        6. Entrega vehículo               │
│                              │  <────────────────────                    │
│                              │                                           │
│   7. Confirma recepción      │                                           │
│      └──────────────> 8. Verifica ambas partes                          │
│                              │                                           │
│                       9. Libera fondos ────────────>  10. Recibe pago   │
│                                                                          │
│   ═══════════════════════════════════════════════════════════════════   │
│                                                                          │
│   PROTECCIÓN:                                                            │
│   • Comprador: Dinero seguro hasta recibir vehículo                     │
│   • Vendedor: Pago garantizado una vez entregado                        │
│   • Disputas: OKLA media y decide                                       │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

### 1.3 Dependencias

| Servicio            | Propósito                 |
| ------------------- | ------------------------- |
| BillingService      | Procesamiento de pagos    |
| UserService         | Verificación de identidad |
| VehiclesSaleService | Datos del vehículo        |
| NotificationService | Comunicaciones            |
| InvoicingService    | Facturación               |
| LegalService        | Documentos legales        |

---

## 2. Endpoints API

### 2.1 EscrowController

| Método | Endpoint                            | Descripción         | Auth | Roles       |
| ------ | ----------------------------------- | ------------------- | ---- | ----------- |
| `POST` | `/api/escrow`                       | Crear escrow        | ✅   | User        |
| `GET`  | `/api/escrow/{id}`                  | Obtener escrow      | ✅   | Participant |
| `GET`  | `/api/escrow/my`                    | Mis escrows         | ✅   | User        |
| `POST` | `/api/escrow/{id}/fund`             | Depositar fondos    | ✅   | Buyer       |
| `POST` | `/api/escrow/{id}/confirm-delivery` | Confirmar entrega   | ✅   | Seller      |
| `POST` | `/api/escrow/{id}/confirm-receipt`  | Confirmar recepción | ✅   | Buyer       |
| `POST` | `/api/escrow/{id}/release`          | Liberar fondos      | ✅   | System      |
| `POST` | `/api/escrow/{id}/dispute`          | Abrir disputa       | ✅   | Participant |
| `POST` | `/api/escrow/{id}/cancel`           | Cancelar            | ✅   | Participant |

### 2.2 DisputesController

| Método | Endpoint                             | Descripción       | Auth | Roles              |
| ------ | ------------------------------------ | ----------------- | ---- | ------------------ |
| `GET`  | `/api/escrow/disputes`               | Listar disputas   | ✅   | Admin              |
| `GET`  | `/api/escrow/disputes/{id}`          | Ver disputa       | ✅   | Participant, Admin |
| `POST` | `/api/escrow/disputes/{id}/evidence` | Agregar evidencia | ✅   | Participant        |
| `POST` | `/api/escrow/disputes/{id}/resolve`  | Resolver disputa  | ✅   | Admin              |

---

## 3. Entidades y Enums

### 3.1 EscrowStatus (Enum)

```csharp
public enum EscrowStatus
{
    Created = 0,              // Escrow creado, esperando fondos
    Funded = 1,               // Fondos depositados
    InTransit = 2,            // Vehículo en proceso de entrega
    DeliveryConfirmed = 3,    // Vendedor confirmó entrega
    ReceiptConfirmed = 4,     // Comprador confirmó recepción
    Released = 5,             // Fondos liberados al vendedor
    Disputed = 6,             // En disputa
    Refunded = 7,             // Fondos devueltos al comprador
    Cancelled = 8,            // Cancelado
    Expired = 9               // Expiró sin completar
}
```

### 3.2 DisputeReason (Enum)

```csharp
public enum DisputeReason
{
    VehicleNotAsDescribed = 0,    // Vehículo diferente a descripción
    VehicleNotReceived = 1,       // No recibió el vehículo
    MechanicalIssues = 2,         // Problemas mecánicos ocultos
    DocumentationIssues = 3,      // Problemas con documentos
    SellerUnresponsive = 4,       // Vendedor no responde
    BuyerNotPaying = 5,           // Comprador no completa pago
    Other = 99                    // Otro
}
```

### 3.3 EscrowTransaction (Entidad)

```csharp
public class EscrowTransaction
{
    public Guid Id { get; set; }
    public string EscrowNumber { get; set; }      // ESC-2026-00001
    public EscrowStatus Status { get; set; }

    // Participantes
    public Guid BuyerId { get; set; }
    public Guid SellerId { get; set; }
    public Guid VehicleId { get; set; }

    // Montos
    public decimal VehiclePrice { get; set; }
    public decimal EscrowFee { get; set; }        // 1.5% del precio
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; }

    // Estado de fondos
    public bool FundsPending { get; set; }
    public bool FundsReceived { get; set; }
    public DateTime? FundsReceivedAt { get; set; }
    public bool FundsReleased { get; set; }
    public DateTime? FundsReleasedAt { get; set; }

    // Confirmaciones
    public bool SellerConfirmedDelivery { get; set; }
    public DateTime? DeliveryConfirmedAt { get; set; }
    public bool BuyerConfirmedReceipt { get; set; }
    public DateTime? ReceiptConfirmedAt { get; set; }

    // Disputa
    public Guid? DisputeId { get; set; }

    // Cuenta bancaria de liberación
    public string? SellerBankAccount { get; set; }
    public string? SellerBankName { get; set; }

    // Tiempos
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }       // 7 días para completar
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
}
```

### 3.4 EscrowDispute (Entidad)

```csharp
public class EscrowDispute
{
    public Guid Id { get; set; }
    public Guid EscrowId { get; set; }
    public Guid InitiatedBy { get; set; }         // Buyer o Seller
    public DisputeReason Reason { get; set; }
    public string Description { get; set; }

    // Estado
    public DisputeStatus Status { get; set; }

    // Evidencia
    public List<DisputeEvidence> Evidence { get; set; }

    // Resolución
    public Guid? ResolvedBy { get; set; }
    public DisputeResolution? Resolution { get; set; }
    public decimal? RefundAmount { get; set; }
    public decimal? ReleaseAmount { get; set; }
    public string? ResolutionNotes { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}
```

---

## 4. Procesos Detallados

### 4.1 ESC-001: Crear Escrow

| Campo       | Valor                      |
| ----------- | -------------------------- |
| **ID**      | ESC-001                    |
| **Nombre**  | Iniciar Transacción Escrow |
| **Actor**   | Comprador                  |
| **Trigger** | POST /api/escrow           |

#### Flujo del Proceso

| Paso | Acción                     | Sistema             | Validación           |
| ---- | -------------------------- | ------------------- | -------------------- |
| 1    | Comprador ve vehículo      | Frontend            | VehicleDetail        |
| 2    | Click "Comprar con Escrow" | Frontend            | Modal                |
| 3    | Revisar términos           | Frontend            | Aceptar T&C          |
| 4    | Confirmar precio           | Frontend            | Con fee              |
| 5    | Validar comprador KYC      | UserService         | Identidad verificada |
| 6    | Validar vendedor           | UserService         | Cuenta activa        |
| 7    | Calcular fee               | EscrowService       | 1.5%                 |
| 8    | Crear escrow               | Database            | Status = Created     |
| 9    | Notificar vendedor         | NotificationService | Email + Push         |
| 10   | Generar instrucciones      | EscrowService       | Para depósito        |
| 11   | Publicar evento            | RabbitMQ            | escrow.created       |

#### Request

```json
{
  "vehicleId": "uuid",
  "sellerId": "uuid",
  "agreedPrice": 1500000.0,
  "currency": "DOP",
  "deliveryMethod": "InPerson",
  "deliveryLocation": "Sucursal Santo Domingo",
  "buyerNotes": "Disponible para recoger el sábado"
}
```

#### Response

```json
{
  "id": "uuid",
  "escrowNumber": "ESC-2026-00001",
  "status": "Created",
  "vehiclePrice": 1500000.0,
  "escrowFee": 22500.0,
  "totalAmount": 1522500.0,
  "expiresAt": "2026-01-28T12:00:00Z",
  "paymentInstructions": {
    "bankName": "Banco Popular Dominicano",
    "accountNumber": "****5678",
    "accountName": "OKLA Escrow Account",
    "reference": "ESC-2026-00001"
  }
}
```

---

### 4.2 ESC-002: Depositar Fondos

| Campo       | Valor                      |
| ----------- | -------------------------- |
| **ID**      | ESC-002                    |
| **Nombre**  | Depositar Fondos en Escrow |
| **Actor**   | Comprador                  |
| **Trigger** | POST /api/escrow/{id}/fund |

#### Flujo del Proceso

| Paso | Acción                   | Sistema             | Validación           |
| ---- | ------------------------ | ------------------- | -------------------- |
| 1    | Comprador elige método   | Frontend            | Transferencia/Stripe |
| 2    | Si transferencia         | Bank                | ACH/Wire             |
| 3    | Si Stripe/AZUL           | BillingService      | Procesar pago        |
| 4    | Verificar monto completo | EscrowService       | == totalAmount       |
| 5    | Actualizar escrow        | Database            | Status = Funded      |
| 6    | Retener fondos           | Account             | Cuenta escrow        |
| 7    | Notificar vendedor       | NotificationService | "Fondos recibidos"   |
| 8    | Notificar comprador      | NotificationService | Confirmación         |
| 9    | Iniciar timer            | EscrowService       | 7 días para entregar |
| 10   | Publicar evento          | RabbitMQ            | escrow.funded        |

---

### 4.3 ESC-003: Confirmar Entrega

| Campo       | Valor                                  |
| ----------- | -------------------------------------- |
| **ID**      | ESC-003                                |
| **Nombre**  | Vendedor Confirma Entrega              |
| **Actor**   | Vendedor                               |
| **Trigger** | POST /api/escrow/{id}/confirm-delivery |

#### Flujo del Proceso

| Paso | Acción                    | Sistema             | Validación                |
| ---- | ------------------------- | ------------------- | ------------------------- |
| 1    | Vendedor entrega vehículo | Presencial          | Físicamente               |
| 2    | Accede a app              | Mobile/Web          | Autenticado               |
| 3    | Buscar escrow activo      | EscrowService       | Status = Funded           |
| 4    | Click "Confirmar Entrega" | Frontend            | Con foto                  |
| 5    | Subir foto del momento    | MediaService        | Evidencia                 |
| 6    | Firmar digitalmente       | EscrowService       | Timestamp                 |
| 7    | Actualizar escrow         | Database            | DeliveryConfirmed         |
| 8    | Notificar comprador       | NotificationService | "Confirma recepción"      |
| 9    | Iniciar timer             | EscrowService       | 48h para confirmar        |
| 10   | Publicar evento           | RabbitMQ            | escrow.delivery_confirmed |

---

### 4.4 ESC-004: Confirmar Recepción y Liberar

| Campo       | Valor                                 |
| ----------- | ------------------------------------- |
| **ID**      | ESC-004                               |
| **Nombre**  | Comprador Confirma y Libera Fondos    |
| **Actor**   | Comprador                             |
| **Trigger** | POST /api/escrow/{id}/confirm-receipt |

#### Flujo del Proceso

| Paso | Acción                         | Sistema             | Validación                |
| ---- | ------------------------------ | ------------------- | ------------------------- |
| 1    | Comprador recibe vehículo      | Presencial          | Inspección                |
| 2    | Verifica condición             | Comprador           | Vs descripción            |
| 3    | Si satisfecho                  | Frontend            | "Confirmar"               |
| 4    | Confirmar recepción            | EscrowService       | Status = ReceiptConfirmed |
| 5    | Verificar ambas confirmaciones | EscrowService       | Delivery + Receipt        |
| 6    | Calcular monto a liberar       | EscrowService       | Precio - fee              |
| 7    | Iniciar transferencia          | BillingService      | Al vendedor               |
| 8    | Actualizar escrow              | Database            | Status = Released         |
| 9    | Enviar comprobante             | NotificationService | A ambos                   |
| 10   | Marcar vehículo vendido        | VehiclesSaleService | Status = Sold             |
| 11   | Generar factura                | InvoicingService    | Para ambos                |
| 12   | Publicar evento                | RabbitMQ            | escrow.completed          |

---

### 4.5 ESC-005: Abrir Disputa

| Campo       | Valor                         |
| ----------- | ----------------------------- |
| **ID**      | ESC-005                       |
| **Nombre**  | Abrir Disputa de Escrow       |
| **Actor**   | Comprador/Vendedor            |
| **Trigger** | POST /api/escrow/{id}/dispute |

#### Flujo del Proceso

| Paso | Acción                      | Sistema             | Validación          |
| ---- | --------------------------- | ------------------- | ------------------- |
| 1    | Participante inicia disputa | Frontend            | Razón + descripción |
| 2    | Subir evidencia inicial     | MediaService        | Fotos, docs         |
| 3    | Crear disputa               | Database            | Status = Open       |
| 4    | Pausar liberación           | EscrowService       | Fondos retenidos    |
| 5    | Notificar contraparte       | NotificationService | Email urgente       |
| 6    | Notificar admin             | NotificationService | Ticket creado       |
| 7    | Actualizar escrow           | Database            | Status = Disputed   |
| 8    | Dar 48h para responder      | EscrowService       | Timer               |
| 9    | Publicar evento             | RabbitMQ            | escrow.disputed     |

#### Request

```json
{
  "reason": "VehicleNotAsDescribed",
  "description": "El vehículo tiene daños significativos en el motor que no fueron mencionados en la descripción.",
  "evidence": [
    {
      "type": "image",
      "url": "https://...",
      "description": "Foto del motor con daños"
    },
    {
      "type": "document",
      "url": "https://...",
      "description": "Reporte de mecánico"
    }
  ],
  "requestedResolution": "FullRefund"
}
```

---

### 4.6 ESC-006: Resolver Disputa

| Campo       | Valor                                  |
| ----------- | -------------------------------------- |
| **ID**      | ESC-006                                |
| **Nombre**  | Resolver Disputa                       |
| **Actor**   | Admin                                  |
| **Trigger** | POST /api/escrow/disputes/{id}/resolve |

#### Opciones de Resolución

| Resolución      | Descripción                           |
| --------------- | ------------------------------------- |
| `RefundFull`    | 100% al comprador                     |
| `ReleaseFull`   | 100% al vendedor                      |
| `Split`         | Dividir fondos                        |
| `RefundPartial` | Parte al comprador, parte al vendedor |
| `Escalate`      | Escalar a legal                       |

#### Flujo del Proceso

| Paso | Acción                        | Sistema             | Validación              |
| ---- | ----------------------------- | ------------------- | ----------------------- |
| 1    | Admin revisa caso             | Dashboard           | Evidencias              |
| 2    | Contactar partes si necesario | Admin               | Llamadas/emails         |
| 3    | Tomar decisión                | Admin               | Con justificación       |
| 4    | Ingresar resolución           | Dashboard           | Montos + notas          |
| 5    | Ejecutar resolución           | EscrowService       | Transferencias          |
| 6    | Si refund                     | BillingService      | Al comprador            |
| 7    | Si release                    | BillingService      | Al vendedor             |
| 8    | Cerrar disputa                | Database            | Status = Resolved       |
| 9    | Actualizar escrow             | Database            | Refunded/Released       |
| 10   | Notificar partes              | NotificationService | Resultado               |
| 11   | Publicar evento               | RabbitMQ            | escrow.dispute_resolved |

---

## 5. Reglas de Negocio

### 5.1 Fees

| Concepto      | Valor           |
| ------------- | --------------- |
| Fee de escrow | 1.5% del precio |
| Fee mínimo    | RD$ 5,000       |
| Fee máximo    | RD$ 75,000      |

### 5.2 Tiempos

| Evento                    | Tiempo Límite                               |
| ------------------------- | ------------------------------------------- |
| Depósito de fondos        | 48 horas                                    |
| Entrega después de fondos | 7 días                                      |
| Confirmación de recepción | 48 horas                                    |
| Liberación automática     | 48h después de delivery (si no hay disputa) |
| Expiración de escrow      | 14 días total                               |

### 5.3 Requisitos

| Requisito           | Comprador | Vendedor |
| ------------------- | --------- | -------- |
| KYC verificado      | ✅        | ✅       |
| Email verificado    | ✅        | ✅       |
| Teléfono verificado | ✅        | ✅       |
| Cuenta bancaria     | ❌        | ✅       |

---

## 6. Eventos RabbitMQ

| Evento                      | Exchange        | Payload                           |
| --------------------------- | --------------- | --------------------------------- |
| `escrow.created`            | `escrow.events` | `{ escrowId, buyerId, sellerId }` |
| `escrow.funded`             | `escrow.events` | `{ escrowId, amount }`            |
| `escrow.delivery_confirmed` | `escrow.events` | `{ escrowId }`                    |
| `escrow.receipt_confirmed`  | `escrow.events` | `{ escrowId }`                    |
| `escrow.completed`          | `escrow.events` | `{ escrowId, releasedAmount }`    |
| `escrow.disputed`           | `escrow.events` | `{ escrowId, disputeId }`         |
| `escrow.refunded`           | `escrow.events` | `{ escrowId, amount }`            |
| `escrow.expired`            | `escrow.events` | `{ escrowId }`                    |

---

## 7. Métricas

```
# Escrows
escrow_created_total
escrow_completed_total
escrow_disputed_total
escrow_refunded_total
escrow_volume_total{currency="DOP|USD"}

# Tiempos
escrow_time_to_fund_hours
escrow_time_to_delivery_days
escrow_time_to_complete_days

# Disputas
escrow_disputes_total{reason="...", resolution="..."}
escrow_dispute_resolution_time_days
```

---

## 8. Configuración

```json
{
  "Escrow": {
    "FeePercent": 1.5,
    "MinFee": 5000,
    "MaxFee": 75000,
    "FundingTimeoutHours": 48,
    "DeliveryTimeoutDays": 7,
    "ReceiptTimeoutHours": 48,
    "AutoReleaseAfterDeliveryHours": 48,
    "ExpirationDays": 14
  }
}
```

---

## 📚 Referencias

- [01-billing-service.md](01-billing-service.md) - Pagos
- [02-stripe-payment.md](02-stripe-payment.md) - Stripe
- [03-azul-payment.md](03-azul-payment.md) - AZUL
- [04-invoicing-service.md](04-invoicing-service.md) - Facturación
