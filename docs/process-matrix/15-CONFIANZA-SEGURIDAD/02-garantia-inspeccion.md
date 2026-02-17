# 🛡️ Garantía OKLA e Inspección Certificada

> **Código:** TRUST-003, TRUST-004  
> **Versión:** 1.0  
> **Última actualización:** Enero 25, 2026  
> **Criticidad:** 🔴 ALTA (Diferenciador del marketplace)  
> **Estado de Implementación:** ✅ Backend 100% | 🟡 UI 40%

---

## ⚠️ AUDITORÍA DE ACCESO UI (Enero 25, 2026)

> **Estado:** Backend implementado. UI parcialmente visible.

| Proceso              | Backend | UI Access | Observación                |
| -------------------- | ------- | --------- | -------------------------- |
| Ver garantía         | ✅ 100% | ✅ 80%    | Badge en listings          |
| Solicitar inspección | ✅ 100% | 🔴 0%     | Sin formulario             |
| Reclamar garantía    | ✅ 100% | 🔴 0%     | Sin flujo                  |
| Ver certificación    | ✅ 100% | 🟡 50%    | Badge visible, sin detalle |

### Rutas UI Existentes ✅

- ✅ Badge "OKLA Certified" en listings
- ✅ Badge "Garantía 7 días" visible

### Rutas UI Faltantes 🔴

| Ruta Propuesta          | Funcionalidad         | Prioridad |
| ----------------------- | --------------------- | --------- |
| `/warranty/claim`       | Reclamar garantía     | 🔴 ALTA   |
| `/inspection/request`   | Solicitar inspección  | 🟡 MEDIA  |
| `/certified/:vehicleId` | Detalle certificación | 🟡 MEDIA  |

**Verificación Backend:** WarrantyService existe en `/backend/WarrantyService/` ✅

---

## 📊 Resumen de Implementación (ACTUALIZADO)

| Componente   | Total | Implementado | Pendiente | Estado  |
| ------------ | ----- | ------------ | --------- | ------- |
| Controllers  | 2     | 2            | 0         | ✅ 100% |
| WAR-REQ-\*   | 4     | 4            | 0         | ✅ 100% |
| WAR-INSP-\*  | 5     | 5            | 0         | ✅ 100% |
| WAR-CERT-\*  | 4     | 4            | 0         | ✅ 100% |
| WAR-CLAIM-\* | 4     | 3            | 1         | 🟡 75%  |
| Tests        | 12    | 10           | 2         | 🟡 83%  |

**Leyenda:** ✅ Implementado + Tested | 🟢 Implementado | 🟡 En Progreso | 🔴 Pendiente

---

## �📋 Información General

| Campo             | Valor                                                    |
| ----------------- | -------------------------------------------------------- |
| **Servicio**      | WarrantyService                                          |
| **Puerto**        | 5083                                                     |
| **Base de Datos** | `warrantyservice`                                        |
| **Dependencias**  | VehiclesSaleService, BillingService, NotificationService |
| **Integraciones** | Talleres certificados, Aseguradoras                      |

---

## 🎯 Objetivo del Proceso

1. **Garantía OKLA:** 7 días de garantía de devolución para compras en la plataforma
2. **Inspección Certificada:** Servicio de inspección pre-compra por técnicos certificados
3. **Certificación de Vehículo:** Sello de calidad para vehículos inspeccionados

---

## 🏗️ Arquitectura

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                       WarrantyService Architecture                           │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│   User Flow                          Core Service                            │
│   ┌────────────────┐              ┌─────────────────────────────────────┐   │
│   │ Request        │──┐           │          WarrantyService            │   │
│   │ Inspection     │  │           │  ┌───────────────────────────────┐  │   │
│   └────────────────┘  │           │  │ Controllers                   │  │   │
│   ┌────────────────┐  │           │  │ • InspectionController        │  │   │
│   │ Schedule at    │──┼──────────▶│  │ • WarrantyController          │  │   │
│   │ Certified Shop │  │           │  │ • ClaimsController            │  │   │
│   └────────────────┘  │           │  └───────────────────────────────┘  │   │
│   ┌────────────────┐  │           │  ┌───────────────────────────────┐  │   │
│   │ File Warranty  │──┘           │  │ Application (CQRS)            │  │   │
│   │ Claim          │              │  │ • RequestInspectionCmd        │  │   │
│   └────────────────┘              │  │ • SubmitResultsCommand        │  │   │
│                                   │  │ • FileClaimCommand            │  │   │
│   Inspector Flow                  │  └───────────────────────────────┘  │   │
│   ┌────────────────┐              │  ┌───────────────────────────────┐  │   │
│   │ Mobile App     │─────────────▶│  │ Domain                        │  │   │
│   │ (150-point     │              │  │ • VehicleInspection           │  │   │
│   │  Checklist)    │              │  │ • WarrantyClaim               │  │   │
│   └────────────────┘              │  │ • Certification, Inspector    │  │   │
│                                   │  └───────────────────────────────┘  │   │
│   Partners                        └─────────────────────────────────────┘   │
│   ┌────────────────┐                           │                        │
│   │ Certified      │               ┌───────────────┼───────────────┐        │
│   │ Shops Network  │               ▼               ▼               ▼        │
│   └────────────────┘       ┌────────────┐  ┌────────────┐  ┌────────────┐  │
│                            │ PostgreSQL │  │   Redis    │  │  RabbitMQ  │  │
│                            │ (Inspects, │  │  (Badge    │  │ (Cert.    │  │
│                            │  Claims)   │  │  Cache)    │  │  Events)   │  │
│                            └────────────┘  └────────────┘  └────────────┘  │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 📡 Endpoints

| Método | Endpoint                              | Descripción               | Auth |
| ------ | ------------------------------------- | ------------------------- | ---- |
| `POST` | `/api/warranty/request-inspection`    | Solicitar inspección      | ✅   |
| `GET`  | `/api/warranty/inspections`           | Mis inspecciones          | ✅   |
| `GET`  | `/api/warranty/inspections/{id}`      | Detalle de inspección     | ✅   |
| `POST` | `/api/warranty/claims`                | Crear reclamo de garantía | ✅   |
| `GET`  | `/api/warranty/claims/{id}`           | Estado del reclamo        | ✅   |
| `GET`  | `/api/warranty/certified/{vehicleId}` | Ver certificación         | ❌   |

---

## 🗃️ Entidades

### VehicleInspection

```csharp
public class VehicleInspection
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public Guid RequestedBy { get; set; }            // Usuario que solicita
    public Guid? DealerId { get; set; }              // Si es dealer

    // Datos de la inspección
    public Guid InspectorId { get; set; }            // Técnico asignado
    public Guid InspectionCenterId { get; set; }     // Taller/ubicación
    public DateTime ScheduledDate { get; set; }
    public DateTime? CompletedDate { get; set; }

    // Resultado
    public InspectionStatus Status { get; set; }
    public InspectionResult Result { get; set; }
    public int OverallScore { get; set; }            // 0-100

    // Componentes evaluados
    public List<InspectionItem> Items { get; set; }

    // Documentos
    public string ReportPdfUrl { get; set; }
    public List<string> PhotoUrls { get; set; }
    public string VideoUrl { get; set; }

    // Certificación
    public bool IsCertified { get; set; }
    public DateTime? CertificationExpiry { get; set; }
    public string CertificationCode { get; set; }

    // Pago
    public decimal InspectionFee { get; set; }
    public Guid? PaymentId { get; set; }

    public DateTime CreatedAt { get; set; }
}

public enum InspectionStatus
{
    Requested,
    Scheduled,
    InProgress,
    Completed,
    Cancelled
}

public enum InspectionResult
{
    Excellent,    // 90-100: Certificable
    Good,         // 70-89: Certificable con observaciones
    Fair,         // 50-69: No certificable, reparaciones menores
    Poor,         // 30-49: No certificable, reparaciones mayores
    Failed        // 0-29: No apto para venta
}
```

### InspectionItem

```csharp
public class InspectionItem
{
    public Guid Id { get; set; }
    public Guid InspectionId { get; set; }

    public InspectionCategory Category { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    public ItemCondition Condition { get; set; }
    public int Score { get; set; }                   // 0-10
    public string Notes { get; set; }
    public List<string> PhotoUrls { get; set; }

    public bool RequiresRepair { get; set; }
    public decimal? EstimatedRepairCost { get; set; }
}

public enum InspectionCategory
{
    Exterior,      // Carrocería, pintura, vidrios
    Interior,      // Tapicería, tablero, controles
    Engine,        // Motor, transmisión, sistema de escape
    Electrical,    // Luces, batería, sistema eléctrico
    Suspension,    // Amortiguadores, dirección
    Brakes,        // Frenos, discos, pastillas
    Tires,         // Neumáticos, alineación
    UnderBody,     // Chasis, óxido, daños
    Documents      // Matrícula, inspección técnica
}

public enum ItemCondition
{
    Excellent = 10,
    Good = 8,
    Fair = 6,
    Poor = 4,
    Failed = 0
}
```

### WarrantyClaim

```csharp
public class WarrantyClaim
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public Guid BuyerId { get; set; }
    public Guid SellerId { get; set; }
    public Guid TransactionId { get; set; }          // Compra original

    // Detalles del reclamo
    public ClaimType Type { get; set; }
    public string Description { get; set; }
    public List<string> EvidenceUrls { get; set; }

    // Fechas
    public DateTime PurchaseDate { get; set; }
    public DateTime ClaimDate { get; set; }
    public int DaysSincePurchase { get; set; }

    // Estado
    public ClaimStatus Status { get; set; }
    public string Resolution { get; set; }
    public Guid? ResolvedBy { get; set; }

    // Resolución financiera
    public ClaimResolutionType? ResolutionType { get; set; }
    public decimal? RefundAmount { get; set; }
    public decimal? RepairCostCovered { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

public enum ClaimType
{
    MechanicalDefect,     // Defecto mecánico no revelado
    UndisclosedDamage,    // Daño no declarado
    OdometerFraud,        // Kilometraje alterado
    DocumentIssue,        // Problema con documentos
    DescriptionMismatch,  // No coincide con descripción
    Other
}

public enum ClaimStatus
{
    Submitted,
    UnderReview,
    InspectionRequired,
    Approved,
    PartiallyApproved,
    Rejected,
    Resolved
}

public enum ClaimResolutionType
{
    FullRefund,           // Devolución completa
    PartialRefund,        // Devolución parcial
    RepairCoverage,       // Cubrir reparación
    Replacement,          // Reemplazo de vehículo
    NoAction              // Sin acción (reclamo rechazado)
}
```

### InspectionCenter

```csharp
public class InspectionCenter
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string Province { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public string Phone { get; set; }
    public string Email { get; set; }

    public List<DayOfWeek> OperatingDays { get; set; }
    public TimeSpan OpenTime { get; set; }
    public TimeSpan CloseTime { get; set; }

    public bool IsActive { get; set; }
    public bool IsCertified { get; set; }
    public decimal InspectionFee { get; set; }
    public int AverageInspectionTimeMinutes { get; set; }

    public decimal Rating { get; set; }
    public int TotalInspections { get; set; }
}
```

---

## 📊 Proceso TRUST-003: Solicitar Inspección Certificada

```
┌─────────────────────────────────────────────────────────────────────────┐
│ PROCESO: TRUST-003 - Solicitar Inspección Certificada                  │
├─────────────────────────────────────────────────────────────────────────┤
│ Actor Iniciador: USR-REG (Comprador interesado)                        │
│ Sistemas: WarrantyService, BillingService, NotificationService         │
│ Duración: 24-72 horas                                                  │
│ Criticidad: ALTA                                                        │
└─────────────────────────────────────────────────────────────────────────┘
```

| Paso | Subpaso | Acción                                | Sistema             | Actor     | Evidencia              | Código    |
| ---- | ------- | ------------------------------------- | ------------------- | --------- | ---------------------- | --------- |
| 1    | 1.1     | Usuario ve vehículo                   | Frontend            | USR-REG   | Vehicle viewed         | EVD-LOG   |
| 1    | 1.2     | Click "Solicitar Inspección"          | Frontend            | USR-REG   | CTA clicked            | EVD-LOG   |
| 2    | 2.1     | Seleccionar centro de inspección      | Frontend            | USR-REG   | Center selected        | EVD-LOG   |
| 2    | 2.2     | Seleccionar fecha/hora                | Frontend            | USR-REG   | DateTime selected      | EVD-LOG   |
| 3    | 3.1     | POST /api/warranty/request-inspection | Gateway             | USR-REG   | **Request**            | EVD-AUDIT |
| 3    | 3.2     | Verificar disponibilidad              | WarrantyService     | Sistema   | Availability check     | EVD-LOG   |
| 4    | 4.1     | Calcular precio                       | WarrantyService     | Sistema   | Price calculated       | EVD-LOG   |
| 4    | 4.2     | Crear payment intent                  | BillingService      | Sistema   | Payment intent         | EVD-LOG   |
| 5    | 5.1     | Usuario paga inspección               | Stripe/Azul         | USR-REG   | **Payment**            | EVD-AUDIT |
| 5    | 5.2     | Confirmar pago                        | BillingService      | Sistema   | Payment confirmed      | EVD-EVENT |
| 6    | 6.1     | **Crear VehicleInspection**           | WarrantyService     | Sistema   | **Inspection created** | EVD-AUDIT |
| 6    | 6.2     | Asignar inspector                     | WarrantyService     | Sistema   | Inspector assigned     | EVD-LOG   |
| 7    | 7.1     | Notificar al comprador                | NotificationService | SYS-NOTIF | **Buyer notified**     | EVD-COMM  |
| 7    | 7.2     | Notificar al vendedor                 | NotificationService | SYS-NOTIF | **Seller notified**    | EVD-COMM  |
| 7    | 7.3     | Notificar al centro                   | NotificationService | SYS-NOTIF | Center notified        | EVD-COMM  |
| 8    | 8.1     | **Audit trail**                       | AuditService        | Sistema   | Complete audit         | EVD-AUDIT |

### [Día de la Inspección]

| Paso | Subpaso | Acción                         | Sistema             | Actor     | Evidencia              | Código    |
| ---- | ------- | ------------------------------ | ------------------- | --------- | ---------------------- | --------- |
| 9    | 9.1     | Vehículo llega al centro       | InspectionCenter    | Vendedor  | Check-in               | EVD-LOG   |
| 9    | 9.2     | Inspector inicia inspección    | Mobile App          | Inspector | **Inspection started** | EVD-AUDIT |
| 10   | 10.1    | Evaluar cada categoría         | Mobile App          | Inspector | Category scores        | EVD-AUDIT |
| 10   | 10.2    | Tomar fotos de cada punto      | Mobile App          | Inspector | **Photos taken**       | EVD-FILE  |
| 10   | 10.3    | Grabar video general           | Mobile App          | Inspector | **Video recorded**     | EVD-FILE  |
| 11   | 11.1    | Calcular score general         | WarrantyService     | Sistema   | Overall score          | EVD-LOG   |
| 11   | 11.2    | Determinar resultado           | WarrantyService     | Sistema   | Result determined      | EVD-LOG   |
| 12   | 12.1    | **Generar reporte PDF**        | WarrantyService     | Sistema   | **Report PDF**         | EVD-DOC   |
| 12   | 12.2    | Si >= 70: emitir certificación | WarrantyService     | Sistema   | **Certification**      | EVD-DOC   |
| 13   | 13.1    | **Notificar resultado**        | NotificationService | SYS-NOTIF | **Results sent**       | EVD-COMM  |
| 13   | 13.2    | Actualizar listing con badge   | VehiclesSaleService | Sistema   | Badge added            | EVD-EVENT |

### Evidencia de Inspección

```json
{
  "processCode": "TRUST-003",
  "inspection": {
    "id": "insp-12345",
    "vehicle": {
      "id": "veh-67890",
      "make": "Toyota",
      "model": "Corolla",
      "year": 2022,
      "vin": "1HGBH41JXMN109186"
    },
    "requestedBy": {
      "userId": "user-001",
      "name": "María Compradora"
    },
    "seller": {
      "userId": "user-002",
      "name": "Juan Vendedor"
    },
    "center": {
      "id": "center-001",
      "name": "AutoCheck Santo Domingo",
      "address": "Av. Winston Churchill 123"
    },
    "inspector": {
      "id": "inspector-001",
      "name": "Carlos Técnico",
      "certification": "ASE-Certified"
    },
    "schedule": {
      "requestedDate": "2026-01-21T10:30:00Z",
      "scheduledDate": "2026-01-23T09:00:00Z",
      "completedDate": "2026-01-23T10:45:00Z",
      "duration": "1h 45m"
    },
    "payment": {
      "amount": 3500,
      "currency": "DOP",
      "status": "PAID",
      "transactionId": "pay-12345"
    },
    "results": {
      "overallScore": 85,
      "result": "GOOD",
      "categories": [
        { "category": "EXTERIOR", "score": 90, "items": 12, "passed": 11 },
        { "category": "INTERIOR", "score": 88, "items": 10, "passed": 9 },
        { "category": "ENGINE", "score": 82, "items": 15, "passed": 12 },
        { "category": "ELECTRICAL", "score": 85, "items": 8, "passed": 7 },
        { "category": "SUSPENSION", "score": 80, "items": 6, "passed": 5 },
        { "category": "BRAKES", "score": 90, "items": 6, "passed": 6 },
        { "category": "TIRES", "score": 75, "items": 4, "passed": 3 },
        { "category": "UNDERBODY", "score": 85, "items": 8, "passed": 7 },
        { "category": "DOCUMENTS", "score": 100, "items": 5, "passed": 5 }
      ],
      "issues": [
        {
          "category": "ENGINE",
          "item": "Oil Leak",
          "severity": "MINOR",
          "estimatedRepairCost": 8500,
          "notes": "Pequeña fuga de aceite en junta de válvula"
        },
        {
          "category": "TIRES",
          "item": "Tire Wear",
          "severity": "MODERATE",
          "estimatedRepairCost": 15000,
          "notes": "Neumáticos traseros con 40% de vida útil"
        }
      ],
      "recommendations": [
        "Reemplazar junta de válvula en próximos 5,000 km",
        "Considerar cambio de neumáticos traseros pronto"
      ]
    },
    "certification": {
      "certified": true,
      "code": "OKLA-CERT-2026-12345",
      "validUntil": "2026-04-23",
      "badge": "OKLA_INSPECTED"
    },
    "documents": {
      "reportPdf": "s3://warranty/insp-12345/report.pdf",
      "photos": [
        "s3://warranty/insp-12345/exterior-1.jpg",
        "s3://warranty/insp-12345/engine-1.jpg"
      ],
      "video": "s3://warranty/insp-12345/walkthrough.mp4"
    }
  }
}
```

---

## 📊 Proceso TRUST-004: Reclamo de Garantía

```
┌─────────────────────────────────────────────────────────────────────────┐
│ PROCESO: TRUST-004 - Reclamo de Garantía                               │
├─────────────────────────────────────────────────────────────────────────┤
│ Actor Iniciador: USR-REG (Comprador con garantía activa)               │
│ Sistemas: WarrantyService, BillingService, NotificationService         │
│ Duración: 24-72 horas resolución                                       │
│ Criticidad: CRÍTICA                                                    │
└─────────────────────────────────────────────────────────────────────────┘
```

| Paso | Subpaso | Acción                         | Sistema             | Actor     | Evidencia             | Código    |
| ---- | ------- | ------------------------------ | ------------------- | --------- | --------------------- | --------- |
| 1    | 1.1     | Usuario accede a mis compras   | Frontend            | USR-REG   | Access                | EVD-LOG   |
| 1    | 1.2     | Selecciona compra con garantía | Frontend            | USR-REG   | Purchase selected     | EVD-LOG   |
| 1    | 1.3     | Click "Iniciar Reclamo"        | Frontend            | USR-REG   | CTA clicked           | EVD-LOG   |
| 2    | 2.1     | Verificar garantía vigente     | WarrantyService     | Sistema   | **Warranty check**    | EVD-AUDIT |
| 2    | 2.2     | Calcular días desde compra     | WarrantyService     | Sistema   | Days calculated       | EVD-LOG   |
| 3    | 3.1     | Usuario selecciona tipo        | Frontend            | USR-REG   | Type selected         | EVD-LOG   |
| 3    | 3.2     | Usuario describe problema      | Frontend            | USR-REG   | Description input     | EVD-LOG   |
| 3    | 3.3     | Usuario sube evidencias        | MediaService        | USR-REG   | **Evidence uploaded** | EVD-FILE  |
| 4    | 4.1     | POST /api/warranty/claims      | Gateway             | USR-REG   | **Request**           | EVD-AUDIT |
| 4    | 4.2     | Validar datos                  | WarrantyService     | Sistema   | Validation            | EVD-LOG   |
| 5    | 5.1     | **Crear WarrantyClaim**        | WarrantyService     | Sistema   | **Claim created**     | EVD-AUDIT |
| 5    | 5.2     | Snapshot de compra             | WarrantyService     | Sistema   | **Purchase snapshot** | EVD-SNAP  |
| 6    | 6.1     | **Notificar al equipo**        | NotificationService | SYS-NOTIF | **Team notified**     | EVD-COMM  |
| 6    | 6.2     | Notificar al vendedor          | NotificationService | SYS-NOTIF | **Seller notified**   | EVD-COMM  |
| 6    | 6.3     | Confirmar al comprador         | NotificationService | SYS-NOTIF | **Buyer confirmed**   | EVD-COMM  |
| 7    | 7.1     | **Audit trail**                | AuditService        | Sistema   | Complete audit        | EVD-AUDIT |

### [Resolución del Reclamo]

| Paso | Subpaso | Acción                           | Sistema             | Actor       | Evidencia            | Código    |
| ---- | ------- | -------------------------------- | ------------------- | ----------- | -------------------- | --------- |
| 8    | 8.1     | Admin revisa reclamo             | Frontend            | ADM-SUPPORT | Access log           | EVD-AUDIT |
| 8    | 8.2     | Revisa evidencias                | Frontend            | ADM-SUPPORT | Evidence reviewed    | EVD-LOG   |
| 8    | 8.3     | Contacta a ambas partes          | WarrantyService     | ADM-SUPPORT | **Contact log**      | EVD-COMM  |
| 9    | 9.1     | Si necesario: ordenar inspección | WarrantyService     | ADM-SUPPORT | Inspection ordered   | EVD-AUDIT |
| 10   | 10.1    | **Toma decisión**                | WarrantyService     | ADM-SUPPORT | **Decision**         | EVD-AUDIT |
| 10   | 10.2    | Documenta resolución             | WarrantyService     | ADM-SUPPORT | Resolution notes     | EVD-AUDIT |
| 11   | 11.1    | Si reembolso: procesar           | BillingService      | Sistema     | **Refund processed** | EVD-AUDIT |
| 11   | 11.2    | Si reparación: autorizar         | WarrantyService     | Sistema     | Repair authorized    | EVD-AUDIT |
| 12   | 12.1    | **Notificar resolución**         | NotificationService | SYS-NOTIF   | **Resolution sent**  | EVD-COMM  |
| 12   | 12.2    | Notificar al vendedor            | NotificationService | SYS-NOTIF   | Seller notified      | EVD-COMM  |
| 13   | 13.1    | Actualizar trust scores          | TrustService        | Sistema     | Scores updated       | EVD-LOG   |

### Evidencia de Reclamo de Garantía

```json
{
  "processCode": "TRUST-004",
  "claim": {
    "id": "claim-12345",
    "purchase": {
      "transactionId": "tx-67890",
      "vehicleId": "veh-11111",
      "vehicleTitle": "Honda Civic 2021",
      "purchaseDate": "2026-01-15T14:30:00Z",
      "purchasePrice": 1250000,
      "warrantyExpiresAt": "2026-01-22T14:30:00Z"
    },
    "buyer": {
      "userId": "user-001",
      "name": "María Compradora",
      "email": "maria@email.com"
    },
    "seller": {
      "userId": "user-002",
      "name": "Juan Vendedor",
      "type": "INDIVIDUAL"
    },
    "claim": {
      "type": "MECHANICAL_DEFECT",
      "description": "El vehículo presenta una falla en la transmisión que no fue revelada. Al segundo día de uso, el vehículo empezó a hacer ruido al cambiar de marcha y ahora no entra tercera.",
      "evidence": [
        {
          "type": "VIDEO",
          "url": "s3://warranty/claim-12345/transmission-noise.mp4",
          "description": "Video del ruido de transmisión"
        },
        {
          "type": "PHOTO",
          "url": "s3://warranty/claim-12345/dashboard.jpg",
          "description": "Luz de check engine encendida"
        },
        {
          "type": "DOCUMENT",
          "url": "s3://warranty/claim-12345/mechanic-diagnosis.pdf",
          "description": "Diagnóstico del mecánico"
        }
      ],
      "daysSincePurchase": 3,
      "withinWarranty": true
    },
    "resolution": {
      "status": "APPROVED",
      "type": "PARTIAL_REFUND",
      "decision": "Reclamo válido. El defecto de transmisión debió ser revelado.",
      "resolvedBy": {
        "type": "ADM-SUPPORT",
        "id": "admin-001",
        "name": "Carlos Soporte"
      },
      "financial": {
        "refundAmount": 150000,
        "refundReason": "Costo estimado de reparación de transmisión",
        "refundMethod": "ORIGINAL_PAYMENT_METHOD",
        "refundTransactionId": "refund-12345"
      },
      "additionalActions": [
        "Advertencia emitida al vendedor",
        "Trust score del vendedor reducido"
      ],
      "timestamp": "2026-01-18T16:30:00Z"
    },
    "notifications": {
      "buyer": {
        "sent": true,
        "message": "Tu reclamo fue aprobado. Se procesó un reembolso de RD$150,000."
      },
      "seller": {
        "sent": true,
        "message": "El reclamo del comprador fue aprobado. Se emitió una advertencia."
      }
    },
    "impact": {
      "sellerTrustScoreChange": -20,
      "sellerWarningIssued": true
    }
  }
}
```

---

## 💰 Precios de Inspección

| Tipo de Vehículo | Precio (DOP) | Duración      | Puntos Evaluados |
| ---------------- | ------------ | ------------- | ---------------- |
| Sedán/Compacto   | RD$ 3,500    | 1-2 horas     | 74 puntos        |
| SUV/Crossover    | RD$ 4,500    | 1.5-2.5 horas | 80 puntos        |
| Pickup/Camioneta | RD$ 5,000    | 2-3 horas     | 85 puntos        |
| Lujo/Deportivo   | RD$ 7,500    | 2-3 horas     | 90 puntos        |
| Comercial/Pesado | RD$ 10,000   | 3-4 horas     | 100 puntos       |

---

## 🏅 Badge de Certificación

```
┌─────────────────────────────────────────────────────────────────────────┐
│                     🔍 OKLA INSPECTED                                   │
│                                                                         │
│   Este vehículo ha sido inspeccionado por un técnico certificado       │
│   de OKLA y cumple con nuestros estándares de calidad.                 │
│                                                                         │
│   ✓ 74 puntos de inspección                                            │
│   ✓ Score: 85/100                                                      │
│   ✓ Sin defectos mayores                                               │
│                                                                         │
│   Código: OKLA-CERT-2026-12345                                         │
│   Válido hasta: 23 de abril 2026                                       │
│   Ver reporte completo →                                               │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 📊 Métricas Prometheus

```yaml
# Inspecciones
warranty_inspections_total{center, result}
warranty_inspection_processing_time_hours
warranty_certification_rate

# Reclamos
warranty_claims_total{type, status}
warranty_claim_resolution_time_hours
warranty_claim_approval_rate

# Financiero
warranty_refunds_total{reason}
warranty_refund_amount_total
warranty_inspection_revenue_total
```

---

## 🔗 Referencias

- [15-CONFIANZA-SEGURIDAD/01-verificacion-identidad.md](01-verificacion-identidad.md)
- [05-PAGOS-FACTURACION/01-billing-service.md](../05-PAGOS-FACTURACION/01-billing-service.md)
- [03-VEHICULOS-INVENTARIO/01-vehicles-sale-service.md](../03-VEHICULOS-INVENTARIO/01-vehicles-sale-service.md)
