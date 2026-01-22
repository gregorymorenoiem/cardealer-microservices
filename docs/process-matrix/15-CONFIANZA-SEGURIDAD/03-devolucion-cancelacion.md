# 🔙 Devolución y Cancelación

> **Código:** TRUST-005, TRUST-006  
> **Versión:** 1.0  
> **Última actualización:** Enero 21, 2026  
> **Criticidad:** 🟡 ALTA (Confianza del consumidor)

---

## 📋 Información General

| Campo             | Valor                                                    |
| ----------------- | -------------------------------------------------------- |
| **Servicio**      | TrustService                                             |
| **Puerto**        | 5082                                                     |
| **Base de Datos** | `trustservice`                                           |
| **Dependencias**  | BillingService, NotificationService, VehiclesSaleService |

---

## 🎯 Objetivo del Proceso

1. **Garantía de satisfacción:** 7 días para devolver vehículo (si aplica)
2. **Cancelación de transacciones:** Antes de entrega
3. **Reembolsos:** Proceso claro y rápido
4. **Protección al consumidor:** Cumplir con ley 358-05

---

## 🛡️ Política de Devolución OKLA

### Aplica Para:

| Tipo de Venta             | Devolución   | Condiciones               |
| ------------------------- | ------------ | ------------------------- |
| **Dealer verificado**     | ✅ 7 días    | Máximo 300 km recorridos  |
| **Venta individual**      | ❌ No aplica | Solo mediación de disputa |
| **Garantía OKLA Protect** | ✅ 30 días   | Defectos ocultos          |

### No Aplica Si:

- Vehículo tiene daños nuevos (accidente)
- Más de 300 km recorridos desde entrega
- Modificaciones realizadas
- Venta entre particulares (sin OKLA Protect)

---

## 📡 Endpoints

| Método | Endpoint                          | Descripción                  | Auth |
| ------ | --------------------------------- | ---------------------------- | ---- |
| `POST` | `/api/trust/returns`              | Solicitar devolución         | ✅   |
| `GET`  | `/api/trust/returns/{id}`         | Estado de devolución         | ✅   |
| `PUT`  | `/api/trust/returns/{id}/approve` | Aprobar devolución (dealer)  | ✅   |
| `PUT`  | `/api/trust/returns/{id}/reject`  | Rechazar devolución (dealer) | ✅   |
| `POST` | `/api/trust/cancellations`        | Cancelar antes de entrega    | ✅   |
| `GET`  | `/api/trust/refunds`              | Mis reembolsos               | ✅   |

---

## 🗃️ Entidades

### ReturnRequest

```csharp
public class ReturnRequest
{
    public Guid Id { get; set; }
    public Guid BuyerId { get; set; }
    public Guid SellerId { get; set; }
    public Guid OrderId { get; set; }
    public Guid VehicleId { get; set; }

    // Tipo
    public ReturnType Type { get; set; }

    // Motivo
    public ReturnReason Reason { get; set; }
    public string ReasonDetails { get; set; }

    // Evidencia
    public List<string> PhotoUrls { get; set; }
    public List<string> VideoUrls { get; set; }
    public string OdometerPhoto { get; set; }
    public int CurrentOdometer { get; set; }
    public int DeliveryOdometer { get; set; }

    // Status
    public ReturnStatus Status { get; set; }

    // Timeline
    public DateTime DeliveryDate { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int DaysSinceDelivery { get; set; }

    // Resolución
    public string Resolution { get; set; }
    public Guid? RefundId { get; set; }
    public string RejectionReason { get; set; }

    // Inspección
    public bool RequiresInspection { get; set; }
    public Guid? InspectionId { get; set; }

    // Dealer response
    public Guid? ReviewedBy { get; set; }
    public string ReviewNotes { get; set; }
}

public enum ReturnType
{
    Satisfaction,        // 7 días satisfacción garantizada
    DefectClaim,         // Defecto oculto
    MisrepresentedTitle  // Título no coincide
}

public enum ReturnReason
{
    NotAsDescribed,
    MechanicalIssue,
    CosmeticIssue,
    ChangedMind,
    FoundBetterDeal,
    FinancingFailed,
    TitleIssue,
    Other
}

public enum ReturnStatus
{
    Submitted,
    UnderReview,
    InspectionScheduled,
    InspectionCompleted,
    Approved,
    Rejected,
    VehicleReturned,
    RefundProcessing,
    Completed,
    Disputed
}
```

### Cancellation

```csharp
public class Cancellation
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid OrderId { get; set; }
    public Guid VehicleId { get; set; }

    // Tipo
    public CancellationType Type { get; set; }
    public CancellationReason Reason { get; set; }
    public string ReasonDetails { get; set; }

    // Status del pedido al cancelar
    public string OrderStatus { get; set; }

    // Financiero
    public decimal AmountPaid { get; set; }
    public decimal RefundAmount { get; set; }
    public decimal? CancellationFee { get; set; }
    public string RefundMethod { get; set; }

    // Reembolso
    public Guid? RefundId { get; set; }
    public DateTime? RefundedAt { get; set; }

    public DateTime RequestedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public CancellationStatus Status { get; set; }
}

public enum CancellationType
{
    BuyerInitiated,
    SellerInitiated,
    SystemAutomatic,      // Pago falló
    AdminForced
}

public enum CancellationReason
{
    ChangedMind,
    FoundBetterDeal,
    FinancingDenied,
    VehicleSoldToOther,
    VehicleNotAvailable,
    PricingError,
    FraudSuspected,
    Other
}

public enum CancellationStatus
{
    Requested,
    Approved,
    RefundPending,
    RefundProcessed,
    Completed,
    Disputed
}
```

### Refund

```csharp
public class Refund
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    // Origen
    public RefundSource Source { get; set; }
    public Guid SourceId { get; set; }              // ReturnId or CancellationId

    // Montos
    public decimal OriginalAmount { get; set; }
    public decimal RefundAmount { get; set; }
    public decimal? DeductedFees { get; set; }
    public string FeeReason { get; set; }

    // Método
    public RefundMethod Method { get; set; }
    public string PaymentReference { get; set; }

    // Status
    public RefundStatus Status { get; set; }

    // Timeline
    public DateTime RequestedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int EstimatedDays { get; set; }

    // Pago
    public string TransactionId { get; set; }
    public string ReceiptUrl { get; set; }
}

public enum RefundSource
{
    Return,
    Cancellation,
    Dispute,
    WarrantyClaim,
    AdminCredit
}

public enum RefundMethod
{
    OriginalPaymentMethod,
    BankTransfer,
    AccountCredit,
    Check
}

public enum RefundStatus
{
    Pending,
    Approved,
    Processing,
    Completed,
    Failed,
    Disputed
}
```

---

## 📊 Proceso TRUST-005: Solicitar Devolución

```
┌─────────────────────────────────────────────────────────────────────────┐
│ PROCESO: TRUST-005 - Solicitar Devolución de Vehículo                  │
├─────────────────────────────────────────────────────────────────────────┤
│ Actor Iniciador: USR-BUYER                                             │
│ Sistemas: TrustService, BillingService, NotificationService            │
│ Duración: 1-7 días                                                     │
│ Criticidad: ALTA                                                        │
└─────────────────────────────────────────────────────────────────────────┘
```

| Paso | Subpaso | Acción                                | Sistema             | Actor     | Evidencia           | Código     |
| ---- | ------- | ------------------------------------- | ------------------- | --------- | ------------------- | ---------- |
| 1    | 1.1     | Comprador accede a "Mis Compras"      | Frontend            | USR-BUYER | Page accessed       | EVD-LOG    |
| 1    | 1.2     | Click "Solicitar Devolución"          | Frontend            | USR-BUYER | CTA clicked         | EVD-LOG    |
| 2    | 2.1     | Verificar elegibilidad                | TrustService        | Sistema   | Eligibility check   | EVD-LOG    |
| 2    | 2.2     | Verificar días desde entrega (≤7)     | TrustService        | Sistema   | Days check          | EVD-LOG    |
| 2    | 2.3     | Verificar dealer soporta devoluciones | TrustService        | Sistema   | Dealer check        | EVD-LOG    |
| 3    | 3.1     | Formulario de devolución              | Frontend            | USR-BUYER | Form displayed      | EVD-SCREEN |
| 3    | 3.2     | Seleccionar motivo                    | Frontend            | USR-BUYER | Reason selected     | EVD-LOG    |
| 3    | 3.3     | Describir el problema                 | Frontend            | USR-BUYER | Details input       | EVD-LOG    |
| 3    | 3.4     | **Subir fotos del vehículo actual**   | MediaService        | USR-BUYER | **Photos uploaded** | EVD-FILE   |
| 3    | 3.5     | **Subir foto del odómetro**           | MediaService        | USR-BUYER | **Odometer photo**  | EVD-FILE   |
| 4    | 4.1     | POST /api/trust/returns               | Gateway             | USR-BUYER | **Request**         | EVD-AUDIT  |
| 4    | 4.2     | Validar datos                         | TrustService        | Sistema   | Validation          | EVD-LOG    |
| 4    | 4.3     | **Crear ReturnRequest**               | TrustService        | Sistema   | **Return created**  | EVD-AUDIT  |
| 5    | 5.1     | **Notificar al dealer**               | NotificationService | SYS-NOTIF | **Dealer notified** | EVD-COMM   |
| 5    | 5.2     | Email con detalles completos          | Email               | SYS-NOTIF | Email sent          | EVD-COMM   |
| 5    | 5.3     | Push notification                     | Push                | SYS-NOTIF | Push sent           | EVD-COMM   |
| 6    | 6.1     | Confirmar al comprador                | Frontend            | USR-BUYER | Confirmation        | EVD-SCREEN |
| 6    | 6.2     | Email de confirmación                 | Email               | SYS-NOTIF | **Receipt sent**    | EVD-COMM   |
| 7    | 7.1     | **Audit trail**                       | AuditService        | Sistema   | Complete audit      | EVD-AUDIT  |

### Flujo de Aprobación del Dealer

| Paso | Subpaso | Acción                              | Sistema             | Actor      | Evidencia            | Código    |
| ---- | ------- | ----------------------------------- | ------------------- | ---------- | -------------------- | --------- |
| 8    | 8.1     | Dealer revisa solicitud             | Dashboard           | USR-DEALER | Request viewed       | EVD-AUDIT |
| 8    | 8.2     | Revisar fotos y odómetro            | Dashboard           | USR-DEALER | Evidence reviewed    | EVD-LOG   |
| 9    | 9.1     | Si aprueba: programar recogida      | Dashboard           | USR-DEALER | Pickup scheduled     | EVD-LOG   |
| 9    | 9.2     | PUT /api/trust/returns/{id}/approve | Gateway             | USR-DEALER | **Approved**         | EVD-AUDIT |
| 10   | 10.1    | Si requiere inspección              | TrustService        | Sistema    | Inspection required  | EVD-LOG   |
| 10   | 10.2    | Programar inspección                | TrustService        | Sistema    | Inspection scheduled | EVD-LOG   |
| 11   | 11.1    | **Vehículo devuelto**               | TrustService        | Sistema    | **Vehicle returned** | EVD-AUDIT |
| 11   | 11.2    | Actualizar inventario dealer        | VehiclesSaleService | Sistema    | Inventory updated    | EVD-LOG   |
| 12   | 12.1    | **Procesar reembolso**              | BillingService      | Sistema    | **Refund initiated** | EVD-AUDIT |
| 13   | 13.1    | **Notificar reembolso completado**  | NotificationService | SYS-NOTIF  | **Refund confirmed** | EVD-COMM  |

### Evidencia de Devolución

```json
{
  "processCode": "TRUST-005",
  "return": {
    "id": "return-12345",
    "orderId": "order-67890",
    "vehicle": {
      "id": "veh-11111",
      "make": "Honda",
      "model": "Civic",
      "year": 2022,
      "vin": "1HGBH41JXMN109186"
    },
    "buyer": {
      "id": "user-buyer",
      "name": "Juan Comprador",
      "email": "juan@email.com"
    },
    "seller": {
      "id": "dealer-001",
      "name": "AutoMax RD",
      "type": "DEALER"
    },
    "reason": {
      "type": "NOT_AS_DESCRIBED",
      "details": "El aire acondicionado no enfría correctamente como se indicó en el anuncio"
    },
    "evidence": {
      "photos": [
        "s3://returns/return-12345/vehicle-front.jpg",
        "s3://returns/return-12345/vehicle-back.jpg",
        "s3://returns/return-12345/ac-display.jpg"
      ],
      "odometer": {
        "photo": "s3://returns/return-12345/odometer.jpg",
        "atDelivery": 45230,
        "current": 45412,
        "kmDriven": 182
      }
    },
    "timeline": {
      "delivered": "2026-01-15T14:00:00Z",
      "requested": "2026-01-21T10:30:00Z",
      "daysSinceDelivery": 6,
      "eligibleForReturn": true
    },
    "status": "SUBMITTED",
    "sla": {
      "dealerResponseDue": "2026-01-23T10:30:00Z",
      "hoursRemaining": 48
    }
  }
}
```

---

## 📊 Proceso TRUST-006: Cancelar Antes de Entrega

```
┌─────────────────────────────────────────────────────────────────────────┐
│ PROCESO: TRUST-006 - Cancelar Pedido Antes de Entrega                  │
├─────────────────────────────────────────────────────────────────────────┤
│ Actor Iniciador: USR-BUYER                                             │
│ Sistemas: TrustService, BillingService                                 │
│ Duración: Instantáneo - 5 días                                         │
│ Criticidad: MEDIA                                                       │
└─────────────────────────────────────────────────────────────────────────┘
```

| Paso | Subpaso | Acción                          | Sistema             | Actor     | Evidencia                | Código     |
| ---- | ------- | ------------------------------- | ------------------- | --------- | ------------------------ | ---------- |
| 1    | 1.1     | Comprador accede a pedido       | Frontend            | USR-BUYER | Order viewed             | EVD-LOG    |
| 1    | 1.2     | Verificar estado (no entregado) | TrustService        | Sistema   | State check              | EVD-LOG    |
| 1    | 1.3     | Click "Cancelar Pedido"         | Frontend            | USR-BUYER | CTA clicked              | EVD-LOG    |
| 2    | 2.1     | Mostrar política de cancelación | Frontend            | USR-BUYER | Policy shown             | EVD-SCREEN |
| 2    | 2.2     | Mostrar cargo si aplica         | Frontend            | USR-BUYER | Fee shown                | EVD-LOG    |
| 3    | 3.1     | Seleccionar motivo              | Frontend            | USR-BUYER | Reason selected          | EVD-LOG    |
| 3    | 3.2     | Confirmar cancelación           | Frontend            | USR-BUYER | Confirmation             | EVD-LOG    |
| 4    | 4.1     | POST /api/trust/cancellations   | Gateway             | USR-BUYER | **Request**              | EVD-AUDIT  |
| 4    | 4.2     | Verificar estado del pedido     | TrustService        | Sistema   | Status check             | EVD-LOG    |
| 4    | 4.3     | Calcular reembolso              | TrustService        | Sistema   | Refund calc              | EVD-LOG    |
| 4    | 4.4     | **Crear Cancellation**          | TrustService        | Sistema   | **Cancellation created** | EVD-AUDIT  |
| 5    | 5.1     | **Cancelar pedido**             | VehiclesSaleService | Sistema   | **Order cancelled**      | EVD-AUDIT  |
| 5    | 5.2     | Liberar vehículo para venta     | VehiclesSaleService | Sistema   | Vehicle released         | EVD-LOG    |
| 6    | 6.1     | **Procesar reembolso**          | BillingService      | Sistema   | **Refund initiated**     | EVD-AUDIT  |
| 6    | 6.2     | Aplicar fee si corresponde      | BillingService      | Sistema   | Fee applied              | EVD-LOG    |
| 7    | 7.1     | **Notificar al vendedor**       | NotificationService | SYS-NOTIF | **Seller notified**      | EVD-COMM   |
| 7    | 7.2     | **Confirmar al comprador**      | NotificationService | SYS-NOTIF | **Confirmation**         | EVD-COMM   |
| 8    | 8.1     | **Audit trail**                 | AuditService        | Sistema   | Complete audit           | EVD-AUDIT  |

### Política de Fees de Cancelación

| Estado del Pedido             | Fee de Cancelación | Reembolso         |
| ----------------------------- | ------------------ | ----------------- |
| Reservado (sin pago completo) | 0%                 | 100% del depósito |
| Pagado (antes de preparación) | 3%                 | 97%               |
| En preparación                | 5%                 | 95%               |
| Listo para entrega            | 10%                | 90%               |
| En tránsito                   | No cancelable      | -                 |

---

## 📊 Métricas Prometheus

```yaml
# Devoluciones
trust_returns_requested_total{reason}
trust_returns_approved_total
trust_returns_rejected_total
trust_returns_processing_time_hours

# Cancelaciones
trust_cancellations_total{reason, order_status}
trust_cancellation_value_total

# Reembolsos
trust_refunds_total{method, source}
trust_refunds_amount_total
trust_refund_processing_time_hours

# Satisfacción
trust_return_satisfaction_rating
trust_dealer_return_rate{dealer_id}
```

---

## 🔗 Referencias

- [15-CONFIANZA-SEGURIDAD/02-garantia-inspeccion.md](02-garantia-inspeccion.md)
- [05-PAGOS-FACTURACION/01-billing-service.md](../05-PAGOS-FACTURACION/01-billing-service.md)
- [Ley 358-05 de Protección al Consumidor](https://proconsumidor.gob.do)
