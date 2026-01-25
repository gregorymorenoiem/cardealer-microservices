# ⚖️ Disputas y Mediación

> **Código:** DISP-001, DISP-002, DISP-003  
> **Versión:** 1.0  
> **Última actualización:** Enero 21, 2026  
> **Criticidad:** 🔴 CRÍTICA (Protección legal)

---

## � Resumen de Implementación

| Componente      | Total | Implementado | Pendiente | Estado |
| --------------- | ----- | ------------ | --------- | ------ |
| Controllers     | 2     | 0            | 2         | 🔴     |
| DISP-CREATE-\*  | 4     | 0            | 4         | 🔴     |
| DISP-MEDIATE-\* | 5     | 0            | 5         | 🔴     |
| DISP-RESOLVE-\* | 4     | 0            | 4         | 🔴     |
| DISP-ESCAL-\*   | 3     | 0            | 3         | 🔴     |
| Tests           | 0     | 0            | 12        | 🔴     |

**Leyenda:** ✅ Implementado + Tested | 🟢 Implementado | 🟡 En Progreso | 🔴 Pendiente

---

## �📋 Información General

| Campo             | Valor                                                          |
| ----------------- | -------------------------------------------------------------- |
| **Servicio**      | DisputeService                                                 |
| **Puerto**        | 5089                                                           |
| **Base de Datos** | `disputeservice`                                               |
| **Dependencias**  | TrustService, BillingService, NotificationService, UserService |

---

## 🎯 Objetivo del Proceso

1. **Resolver conflictos:** Entre comprador y vendedor
2. **Proteger a ambas partes:** Proceso justo e imparcial
3. **Evitar chargebacks:** Resolver antes de escalación bancaria
4. **Documentar todo:** Audit trail completo para legal

---

## 🏗️ Arquitectura

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                      DisputeService Architecture                             │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│   Parties                            Core Service                            │
│   ┌────────────────┐              ┌─────────────────────────────────────┐   │
│   │ Buyer Opens    │──┐           │          DisputeService              │   │
│   │ Dispute        │  │           │  ┌───────────────────────────────┐  │   │
│   └────────────────┘  │           │  │ Controllers                   │  │   │
│   ┌────────────────┐  │           │  │ • DisputesController          │  │   │
│   │ Seller         │──┼──────────▶│  │ • MediationController         │  │   │
│   │ Responds       │  │           │  └───────────────────────────────┘  │   │
│   └────────────────┘  │           │  ┌───────────────────────────────┐  │   │
│   ┌────────────────┐  │           │  │ Mediation Engine              │  │   │
│   │ Evidence       │──┘           │  │ • Evidence collection        │  │   │
│   │ Upload         │              │  │ • Timeline tracking          │  │   │
│   └────────────────┘              │  │ • Resolution proposals        │  │   │
│                                   │  └───────────────────────────────┘  │   │
│   Admin/Mediator                  │  ┌───────────────────────────────┐  │   │
│   ┌────────────────┐              │  │ Domain                        │  │   │
│   │ Review & Rule  │─────────────▶│  │ • Dispute                     │  │   │
│   │ (Impartial)    │              │  │ • Evidence, Message           │  │   │
│   └────────────────┘              │  │ • Resolution, Escalation      │  │   │
│   ┌────────────────┐              │  └───────────────────────────────┘  │   │
│   │ Escalate to    │              └─────────────────────────────────────┘   │
│   │ Legal Team     │                           │                        │
│   └────────────────┘               ┌───────────────┼───────────────┐        │
│                                    ▼               ▼               ▼        │
│                            ┌────────────┐  ┌────────────┐  ┌────────────┐  │
│                            │ PostgreSQL │  │   Redis    │  │  RabbitMQ  │  │
│                            │ (Disputes, │  │  (Case     │  │ (Dispute  │  │
│                            │  Evidence) │  │  Status)   │  │  Events)   │  │
│                            └────────────┘  └────────────┘  └────────────┘  │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 📋 Tipos de Disputas

| Tipo                     | Iniciador | Ejemplos                           |
| ------------------------ | --------- | ---------------------------------- |
| **Producto no recibido** | Comprador | Pagó pero no le entregaron         |
| **No como se describe**  | Comprador | Diferencias vs anuncio             |
| **Defecto oculto**       | Comprador | Problema mecánico no revelado      |
| **Título/Documentos**    | Comprador | Problemas de transferencia         |
| **Pago no recibido**     | Vendedor  | Comprador tomó vehículo sin pagar  |
| **Cancelación injusta**  | Vendedor  | Comprador canceló sin razón válida |
| **Daños al vehículo**    | Vendedor  | Daños durante test drive           |
| **Fraude**               | Ambos     | Suplantación, estafa               |

---

## 📡 Endpoints

| Método | Endpoint                      | Descripción             | Auth     |
| ------ | ----------------------------- | ----------------------- | -------- |
| `POST` | `/api/disputes`               | Abrir disputa           | ✅       |
| `GET`  | `/api/disputes`               | Mis disputas            | ✅       |
| `GET`  | `/api/disputes/{id}`          | Detalle de disputa      | ✅       |
| `POST` | `/api/disputes/{id}/evidence` | Agregar evidencia       | ✅       |
| `POST` | `/api/disputes/{id}/messages` | Enviar mensaje          | ✅       |
| `POST` | `/api/disputes/{id}/respond`  | Responder (contraparte) | ✅       |
| `POST` | `/api/disputes/{id}/accept`   | Aceptar resolución      | ✅       |
| `POST` | `/api/disputes/{id}/reject`   | Rechazar resolución     | ✅       |
| `PUT`  | `/api/disputes/{id}/resolve`  | Resolver (mediador)     | ✅ Admin |
| `PUT`  | `/api/disputes/{id}/escalate` | Escalar a legal         | ✅ Admin |

---

## 🗃️ Entidades

### Dispute

```csharp
public class Dispute
{
    public Guid Id { get; set; }
    public string CaseNumber { get; set; }           // OKLA-D-2026-00001

    // Partes
    public Guid InitiatorId { get; set; }
    public string InitiatorName { get; set; }
    public DisputePartyType InitiatorType { get; set; }

    public Guid RespondentId { get; set; }
    public string RespondentName { get; set; }
    public DisputePartyType RespondentType { get; set; }

    // Contexto
    public Guid? OrderId { get; set; }
    public Guid? VehicleId { get; set; }
    public Guid? TransactionId { get; set; }

    // Clasificación
    public DisputeType Type { get; set; }
    public DisputeCategory Category { get; set; }
    public DisputePriority Priority { get; set; }

    // Contenido
    public string Title { get; set; }
    public string Description { get; set; }
    public decimal DisputedAmount { get; set; }
    public string Currency { get; set; }

    // Evidencia
    public List<DisputeEvidence> Evidence { get; set; }

    // Comunicación
    public List<DisputeMessage> Messages { get; set; }

    // Resolución
    public DisputeStatus Status { get; set; }
    public DisputeResolution? Resolution { get; set; }

    // Asignación
    public Guid? MediatorId { get; set; }
    public string MediatorName { get; set; }

    // Timeline
    public DateTime CreatedAt { get; set; }
    public DateTime? RespondentDeadline { get; set; }  // 72 horas
    public DateTime? RespondedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime? ClosedAt { get; set; }

    // Escalación
    public bool IsEscalated { get; set; }
    public DateTime? EscalatedAt { get; set; }
    public string EscalationReason { get; set; }
}

public enum DisputePartyType
{
    Buyer,
    Seller,
    Dealer
}

public enum DisputeType
{
    ItemNotReceived,
    NotAsDescribed,
    HiddenDefect,
    DocumentationIssue,
    PaymentNotReceived,
    UnfairCancellation,
    PropertyDamage,
    Fraud
}

public enum DisputeCategory
{
    Vehicle,
    Payment,
    Documentation,
    Service,
    Warranty
}

public enum DisputePriority
{
    Low,
    Medium,
    High,
    Critical
}

public enum DisputeStatus
{
    Open,                   // Recién abierta
    AwaitingResponse,       // Esperando respuesta del demandado
    UnderReview,            // Mediador revisando
    NegotiationPhase,       // Partes negociando
    ResolutionProposed,     // Resolución propuesta
    ResolutionAccepted,     // Ambos aceptaron
    ResolutionRejected,     // Alguien rechazó
    EscalatedToLegal,       // Pasó a legal
    Closed,                 // Resuelta
    Withdrawn               // Retirada por iniciador
}
```

### DisputeEvidence

```csharp
public class DisputeEvidence
{
    public Guid Id { get; set; }
    public Guid DisputeId { get; set; }
    public Guid SubmittedBy { get; set; }
    public string SubmittedByName { get; set; }

    // Contenido
    public EvidenceType Type { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string FileUrl { get; set; }
    public string MimeType { get; set; }
    public long FileSizeBytes { get; set; }

    // Metadata
    public DateTime SubmittedAt { get; set; }
    public bool IsVerified { get; set; }
    public DateTime? VerifiedAt { get; set; }
}

public enum EvidenceType
{
    Photo,
    Video,
    Document,
    Screenshot,
    Chat,
    Receipt,
    Contract,
    InspectionReport,
    PoliceReport
}
```

### DisputeMessage

```csharp
public class DisputeMessage
{
    public Guid Id { get; set; }
    public Guid DisputeId { get; set; }

    public Guid SenderId { get; set; }
    public string SenderName { get; set; }
    public MessageSenderType SenderType { get; set; }

    public string Content { get; set; }
    public List<string> AttachmentUrls { get; set; }

    public bool IsInternal { get; set; }             // Solo staff
    public DateTime SentAt { get; set; }
    public DateTime? ReadAt { get; set; }
}

public enum MessageSenderType
{
    Initiator,
    Respondent,
    Mediator,
    System
}
```

### DisputeResolution

```csharp
public class DisputeResolution
{
    public Guid Id { get; set; }
    public Guid DisputeId { get; set; }
    public Guid ResolvedBy { get; set; }

    // Decisión
    public ResolutionOutcome Outcome { get; set; }
    public string Explanation { get; set; }

    // Acciones
    public List<ResolutionAction> Actions { get; set; }

    // Aceptación
    public bool InitiatorAccepted { get; set; }
    public DateTime? InitiatorAcceptedAt { get; set; }
    public bool RespondentAccepted { get; set; }
    public DateTime? RespondentAcceptedAt { get; set; }

    public DateTime ProposedAt { get; set; }
    public DateTime? FinalizedAt { get; set; }
}

public enum ResolutionOutcome
{
    FavorInitiator,         // A favor del iniciador
    FavorRespondent,        // A favor del demandado
    PartialBoth,            // Compromiso
    Dismissed,              // Desestimada
    Withdrawn               // Retirada
}

public class ResolutionAction
{
    public ActionType Type { get; set; }
    public Guid TargetUserId { get; set; }
    public decimal? Amount { get; set; }
    public string Description { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public enum ActionType
{
    FullRefund,
    PartialRefund,
    ReturnVehicle,
    CompleteDelivery,
    ProvideDocumentation,
    RepairVehicle,
    AccountWarning,
    AccountSuspension,
    AccountBan
}
```

---

## 📊 Proceso DISP-001: Abrir Disputa

```
┌─────────────────────────────────────────────────────────────────────────┐
│ PROCESO: DISP-001 - Abrir Disputa                                      │
├─────────────────────────────────────────────────────────────────────────┤
│ Actor Iniciador: USR-BUYER, USR-SELLER, USR-DEALER                     │
│ Sistemas: DisputeService, NotificationService                          │
│ Duración: Instantáneo                                                  │
│ Criticidad: ALTA                                                        │
└─────────────────────────────────────────────────────────────────────────┘
```

| Paso | Subpaso | Acción                              | Sistema             | Actor     | Evidencia             | Código     |
| ---- | ------- | ----------------------------------- | ------------------- | --------- | --------------------- | ---------- |
| 1    | 1.1     | Usuario accede a transacción/orden  | Frontend            | USR-REG   | Order accessed        | EVD-LOG    |
| 1    | 1.2     | Click "Reportar Problema"           | Frontend            | USR-REG   | CTA clicked           | EVD-LOG    |
| 2    | 2.1     | Verificar que puede abrir disputa   | DisputeService      | Sistema   | Eligibility check     | EVD-LOG    |
| 2    | 2.2     | No hay otra disputa abierta         | DisputeService      | Sistema   | Duplicate check       | EVD-LOG    |
| 3    | 3.1     | Formulario de disputa               | Frontend            | USR-REG   | Form displayed        | EVD-SCREEN |
| 3    | 3.2     | Seleccionar tipo de disputa         | Frontend            | USR-REG   | Type selected         | EVD-LOG    |
| 3    | 3.3     | Describir el problema               | Frontend            | USR-REG   | Description input     | EVD-LOG    |
| 3    | 3.4     | Indicar monto en disputa            | Frontend            | USR-REG   | Amount input          | EVD-LOG    |
| 3    | 3.5     | **Subir evidencia**                 | MediaService        | USR-REG   | **Evidence uploaded** | EVD-FILE   |
| 4    | 4.1     | POST /api/disputes                  | Gateway             | USR-REG   | **Request**           | EVD-AUDIT  |
| 4    | 4.2     | Validar datos                       | DisputeService      | Sistema   | Validation            | EVD-LOG    |
| 4    | 4.3     | Asignar prioridad automática        | DisputeService      | Sistema   | Priority set          | EVD-LOG    |
| 4    | 4.4     | **Crear Dispute**                   | DisputeService      | Sistema   | **Dispute created**   | EVD-AUDIT  |
| 4    | 4.5     | Generar CaseNumber                  | DisputeService      | Sistema   | Number generated      | EVD-LOG    |
| 5    | 5.1     | **Notificar a contraparte**         | NotificationService | SYS-NOTIF | **Notification**      | EVD-COMM   |
| 5    | 5.2     | "Tienes 72 horas para responder"    | Email               | SYS-NOTIF | Email sent            | EVD-COMM   |
| 5    | 5.3     | Si alta prioridad: asignar mediador | DisputeService      | Sistema   | Mediator assigned     | EVD-LOG    |
| 6    | 6.1     | Confirmar al iniciador              | Frontend            | USR-REG   | Confirmation          | EVD-SCREEN |
| 7    | 7.1     | **Audit trail**                     | AuditService        | Sistema   | Complete audit        | EVD-AUDIT  |

### Evidencia de Disputa Abierta

```json
{
  "processCode": "DISP-001",
  "dispute": {
    "id": "dispute-12345",
    "caseNumber": "OKLA-D-2026-00001",
    "parties": {
      "initiator": {
        "id": "user-buyer",
        "name": "Juan Comprador",
        "type": "BUYER"
      },
      "respondent": {
        "id": "dealer-001",
        "name": "AutoMax RD",
        "type": "DEALER"
      }
    },
    "context": {
      "orderId": "order-67890",
      "vehicleId": "veh-11111",
      "vehicle": "Honda Civic 2022"
    },
    "classification": {
      "type": "NOT_AS_DESCRIBED",
      "category": "VEHICLE",
      "priority": "HIGH"
    },
    "content": {
      "title": "Vehículo con daño en chasis no revelado",
      "description": "Al llevar el vehículo a inspección descubrí que tiene un daño estructural en el chasis que no fue mencionado en el anuncio ni durante la venta.",
      "disputedAmount": 1250000
    },
    "evidence": [
      {
        "id": "evd-001",
        "type": "INSPECTION_REPORT",
        "title": "Reporte de inspección ABC",
        "fileUrl": "s3://disputes/dispute-12345/inspection-report.pdf"
      },
      {
        "id": "evd-002",
        "type": "PHOTO",
        "title": "Foto del daño",
        "fileUrl": "s3://disputes/dispute-12345/damage-photo.jpg"
      }
    ],
    "timeline": {
      "createdAt": "2026-01-21T10:30:00Z",
      "respondentDeadline": "2026-01-24T10:30:00Z",
      "hoursToRespond": 72
    },
    "status": "AWAITING_RESPONSE"
  }
}
```

---

## 📊 Proceso DISP-002: Responder a Disputa

| Paso | Subpaso | Acción                           | Sistema             | Actor      | Evidencia             | Código    |
| ---- | ------- | -------------------------------- | ------------------- | ---------- | --------------------- | --------- |
| 1    | 1.1     | Demandado recibe notificación    | Email/Push          | USR-SELLER | Notification received | EVD-COMM  |
| 1    | 1.2     | Click en link para ver disputa   | Frontend            | USR-SELLER | Link clicked          | EVD-LOG   |
| 2    | 2.1     | Ver detalles de la disputa       | Frontend            | USR-SELLER | Dispute viewed        | EVD-AUDIT |
| 2    | 2.2     | Ver evidencia del iniciador      | Frontend            | USR-SELLER | Evidence viewed       | EVD-LOG   |
| 3    | 3.1     | Escribir respuesta               | Frontend            | USR-SELLER | Response input        | EVD-LOG   |
| 3    | 3.2     | **Subir contra-evidencia**       | MediaService        | USR-SELLER | **Evidence uploaded** | EVD-FILE  |
| 4    | 4.1     | POST /api/disputes/{id}/respond  | Gateway             | USR-SELLER | **Request**           | EVD-AUDIT |
| 4    | 4.2     | **Registrar respuesta**          | DisputeService      | Sistema    | **Response recorded** | EVD-AUDIT |
| 4    | 4.3     | Actualizar status a UNDER_REVIEW | DisputeService      | Sistema    | Status updated        | EVD-LOG   |
| 5    | 5.1     | **Notificar al iniciador**       | NotificationService | SYS-NOTIF  | **Notification**      | EVD-COMM  |
| 5    | 5.2     | Asignar mediador si no tiene     | DisputeService      | Sistema    | Mediator assigned     | EVD-LOG   |
| 6    | 6.1     | **Audit trail**                  | AuditService        | Sistema    | Complete audit        | EVD-AUDIT |

---

## 📊 Proceso DISP-003: Resolver Disputa (Mediador)

```
┌─────────────────────────────────────────────────────────────────────────┐
│ PROCESO: DISP-003 - Resolver Disputa                                   │
├─────────────────────────────────────────────────────────────────────────┤
│ Actor Iniciador: ADM-MEDIATOR                                          │
│ Sistemas: DisputeService, BillingService, TrustService                 │
│ Duración: 1-14 días                                                    │
│ Criticidad: CRÍTICA                                                     │
└─────────────────────────────────────────────────────────────────────────┘
```

| Paso | Subpaso | Acción                                    | Sistema             | Actor        | Evidencia              | Código    |
| ---- | ------- | ----------------------------------------- | ------------------- | ------------ | ---------------------- | --------- |
| 1    | 1.1     | Mediador revisa caso                      | Dashboard           | ADM-MEDIATOR | Case reviewed          | EVD-AUDIT |
| 1    | 1.2     | Revisar toda la evidencia                 | Dashboard           | ADM-MEDIATOR | Evidence reviewed      | EVD-LOG   |
| 1    | 1.3     | Revisar historial de partes               | Dashboard           | ADM-MEDIATOR | History reviewed       | EVD-LOG   |
| 2    | 2.1     | Solicitar info adicional si necesario     | Dashboard           | ADM-MEDIATOR | Info requested         | EVD-COMM  |
| 2    | 2.2     | Esperar respuesta                         | Sistema             | Sistema      | Wait                   | EVD-LOG   |
| 3    | 3.1     | Tomar decisión                            | Dashboard           | ADM-MEDIATOR | Decision made          | EVD-AUDIT |
| 3    | 3.2     | Definir acciones de resolución            | Dashboard           | ADM-MEDIATOR | Actions defined        | EVD-AUDIT |
| 4    | 4.1     | PUT /api/disputes/{id}/resolve            | Gateway             | ADM-MEDIATOR | **Request**            | EVD-AUDIT |
| 4    | 4.2     | **Crear DisputeResolution**               | DisputeService      | Sistema      | **Resolution created** | EVD-AUDIT |
| 4    | 4.3     | Actualizar status                         | DisputeService      | Sistema      | Status updated         | EVD-LOG   |
| 5    | 5.1     | **Notificar a ambas partes**              | NotificationService | SYS-NOTIF    | **Notifications**      | EVD-COMM  |
| 5    | 5.2     | "Tienes 48 horas para aceptar o rechazar" | Email               | SYS-NOTIF    | Emails sent            | EVD-COMM  |
| 6    | 6.1     | Si ambos aceptan: ejecutar acciones       | DisputeService      | Sistema      | Actions executed       | EVD-AUDIT |
| 6    | 6.2     | **Si refund: procesar**                   | BillingService      | Sistema      | **Refund processed**   | EVD-AUDIT |
| 6    | 6.3     | Si devolución: coordinar                  | TrustService        | Sistema      | Return coordinated     | EVD-LOG   |
| 6    | 6.4     | Si sanción: aplicar                       | UserService         | Sistema      | **Sanction applied**   | EVD-AUDIT |
| 7    | 7.1     | **Cerrar disputa**                        | DisputeService      | Sistema      | **Dispute closed**     | EVD-AUDIT |
| 8    | 8.1     | **Audit trail completo**                  | AuditService        | Sistema      | Complete audit         | EVD-AUDIT |

### Evidencia de Resolución

```json
{
  "processCode": "DISP-003",
  "resolution": {
    "disputeId": "dispute-12345",
    "caseNumber": "OKLA-D-2026-00001",
    "mediator": {
      "id": "admin-001",
      "name": "Carlos Mediador"
    },
    "decision": {
      "outcome": "FAVOR_INITIATOR",
      "explanation": "La evidencia demuestra que el daño en el chasis existía previamente y no fue revelado. El vendedor tiene obligación legal de informar defectos ocultos según Ley 358-05.",
      "legalBasis": "Art. 35, Ley 358-05 Protección al Consumidor"
    },
    "actions": [
      {
        "type": "RETURN_VEHICLE",
        "targetUser": "user-buyer",
        "description": "Comprador debe devolver el vehículo",
        "deadline": "2026-01-28T23:59:59Z"
      },
      {
        "type": "FULL_REFUND",
        "targetUser": "user-buyer",
        "amount": 1250000,
        "description": "Reembolso completo al comprador"
      },
      {
        "type": "ACCOUNT_WARNING",
        "targetUser": "dealer-001",
        "description": "Primera advertencia por ocultamiento de información"
      }
    ],
    "acceptance": {
      "initiatorAccepted": true,
      "initiatorAcceptedAt": "2026-01-22T14:00:00Z",
      "respondentAccepted": true,
      "respondentAcceptedAt": "2026-01-22T16:30:00Z"
    },
    "timeline": {
      "proposedAt": "2026-01-22T10:00:00Z",
      "acceptanceDeadline": "2026-01-24T10:00:00Z",
      "finalizedAt": "2026-01-22T16:30:00Z"
    },
    "status": "CLOSED"
  }
}
```

---

## 📊 SLAs de Resolución

| Prioridad | Respuesta Inicial | Resolución | Escalación             |
| --------- | ----------------- | ---------- | ---------------------- |
| CRITICAL  | 4 horas           | 48 horas   | 24 horas sin respuesta |
| HIGH      | 24 horas          | 5 días     | 72 horas sin respuesta |
| MEDIUM    | 48 horas          | 10 días    | 5 días sin respuesta   |
| LOW       | 72 horas          | 14 días    | 7 días sin respuesta   |

---

## 📊 Métricas Prometheus

```yaml
# Disputas
dispute_opened_total{type, initiator_type}
dispute_resolved_total{outcome}
dispute_resolution_time_hours{priority}
dispute_escalated_total

# Por usuario
dispute_per_user{user_id, role}
dispute_win_rate{user_type}

# Financiero
dispute_refund_amount_total
dispute_amount_in_process

# Satisfacción
dispute_resolution_satisfaction_rating
```

---

## 🔗 Referencias

- [15-CONFIANZA-SEGURIDAD/03-devolucion-cancelacion.md](03-devolucion-cancelacion.md)
- [05-PAGOS-FACTURACION/01-billing-service.md](../05-PAGOS-FACTURACION/01-billing-service.md)
- [Ley 358-05 de Protección al Consumidor](https://proconsumidor.gob.do)
