# 🛡️ Verificación de Identidad y Vendedor

> **Código:** TRUST-001, TRUST-002  
> **Versión:** 1.0  
> **Última actualización:** Enero 25, 2026  
> **Criticidad:** 🔴 CRÍTICA (Confianza del marketplace)  
> **Estado de Implementación:** ✅ Backend 100% | 🟡 UI 40%

---

## ⚠️ AUDITORÍA DE ACCESO UI (Enero 25, 2026)

> **Estado:** TrustService existe y está funcional. UI parcialmente implementada.

| Proceso              | Backend | UI Access | Observación               |
| -------------------- | ------- | --------- | ------------------------- |
| Subir cédula         | ✅ 100% | ✅ 100%   | En perfil de usuario      |
| Verificar RNC        | ✅ 100% | ✅ 100%   | En registro dealer        |
| Badge vendedor       | ✅ 100% | ✅ 100%   | Visible en listings       |
| Admin verificaciones | ✅ 100% | 🟡 50%    | Parcial en `/admin/users` |
| Historial            | ✅ 100% | 🔴 0%     | Sin UI                    |

### Rutas UI Existentes ✅

- ✅ `/profile/verification` - Subir documentos (implícito)
- ✅ `/dealer/register` - Verificación RNC
- ✅ `/admin/users/:id` - Ver documentos (parcial)

### Rutas UI Faltantes 🔴

| Ruta Propuesta         | Funcionalidad          | Prioridad |
| ---------------------- | ---------------------- | --------- |
| `/admin/verifications` | Cola de verificaciones | 🔴 ALTA   |
| `/profile/trust-score` | Mi nivel de confianza  | 🟡 MEDIA  |

**Verificación Backend:** TrustService existe en `/backend/TrustService/` ✅

---

## 📊 Resumen de Implementación (ACTUALIZADO)

| Componente     | Total | Implementado | Pendiente | Estado  |
| -------------- | ----- | ------------ | --------- | ------- |
| Controllers    | 2     | 2            | 0         | ✅ 100% |
| TRUST-KYC-\*   | 5     | 5            | 0         | ✅ 100% |
| TRUST-VER-\*   | 4     | 4            | 0         | ✅ 100% |
| TRUST-BADGE-\* | 3     | 3            | 0         | ✅ 100% |
| TRUST-HIST-\*  | 3     | 2            | 1         | 🟡 67%  |
| Tests          | 12    | 10           | 2         | 🟡 83%  |

**Leyenda:** ✅ Implementado + Tested | 🟢 Implementado | 🟡 En Progreso | 🔴 Pendiente

---

## �📋 Información General

| Campo             | Valor                                          |
| ----------------- | ---------------------------------------------- |
| **Servicio**      | TrustService                                   |
| **Puerto**        | 5082                                           |
| **Base de Datos** | `trustservice`                                 |
| **Dependencias**  | UserService, MediaService, NotificationService |
| **Integraciones** | JCE (Cédula), DGII (RNC), Jumio/Onfido (KYC)   |

---

## 🎯 Objetivo del Proceso

Generar confianza en el marketplace verificando:

1. Identidad de vendedores (cédula/RNC)
2. Propiedad del vehículo
3. Historial de transacciones
4. Badge de "Vendedor Verificado"

---

## 🏗️ Arquitectura

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                        TrustService Architecture                             │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│   User Actions                       Core Service                            │
│   ┌────────────────┐              ┌─────────────────────────────────────┐   │
│   │ Upload ID      │──┐           │           TrustService              │   │
│   │ (Cédula/RNC)   │  │           │  ┌───────────────────────────────┐  │   │
│   └────────────────┘  │           │  │ Controllers                   │  │   │
│   ┌────────────────┐  │           │  │ • VerificationController      │  │   │
│   │ Selfie with ID │──┼──────────▶│  │ • BadgesController            │  │   │
│   │ (Liveness)     │  │           │  │ • FraudController             │  │   │
│   └────────────────┘  │           │  └───────────────────────────────┘  │   │
│   ┌────────────────┐  │           │  ┌───────────────────────────────┐  │   │
│   │ Report Fraud   │──┘           │  │ Application (CQRS)            │  │   │
│   │ (Suspicious)   │              │  │ • VerifyIdentityCommand       │  │   │
│   └────────────────┘              │  │ • CalculateTrustScoreQuery    │  │   │
│                                   │  │ • AssignBadgeCommand          │  │   │
│   External APIs                   │  └───────────────────────────────┘  │   │
│   ┌────────────────┐              │  ┌───────────────────────────────┐  │   │
│   │ JCE (Cédula)   │─────────────▶│  │ Domain                        │  │   │
│   │ DGII (RNC)     │              │  │ • IdentityVerification        │  │   │
│   │ Jumio/Onfido   │              │  │ • TrustBadge                  │  │   │
│   │ (KYC Provider) │              │  │ • FraudReport, SellerScore    │  │   │
│   └────────────────┘              │  └───────────────────────────────┘  │   │
│                                   └─────────────────────────────────────┘   │
│   Consumers                                        │                        │
│   ┌────────────────┐               ┌───────────────┼───────────────┐        │
│   │ Seller Profile │◀─────────    ▼               ▼               ▼        │
│   │ (Badge Display)│       ┌────────────┐  ┌────────────┐  ┌────────────┐  │
│   └────────────────┘       │ PostgreSQL │  │   Redis    │  │  RabbitMQ  │  │
│                            │ (Verifs,   │  │  (Badge    │  │ (Verified  │  │
│                            │  Scores)   │  │  Cache)    │  │  Events)   │  │
│                            └────────────┘  └────────────┘  └────────────┘  │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 📡 Endpoints

| Método | Endpoint                           | Descripción                       | Auth |
| ------ | ---------------------------------- | --------------------------------- | ---- |
| `POST` | `/api/trust/verify-identity`       | Iniciar verificación de identidad | ✅   |
| `GET`  | `/api/trust/verification-status`   | Estado de mi verificación         | ✅   |
| `POST` | `/api/trust/upload-document`       | Subir documento para verificación | ✅   |
| `GET`  | `/api/trust/badges/{userId}`       | Badges de un usuario              | ❌   |
| `POST` | `/api/trust/report-fraud`          | Reportar fraude/estafa            | ✅   |
| `GET`  | `/api/trust/seller-score/{userId}` | Score de confianza del vendedor   | ❌   |

---

## 🗃️ Entidades

### IdentityVerification

```csharp
public class IdentityVerification
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public VerificationType Type { get; set; }

    // Datos del documento
    public DocumentType DocumentType { get; set; }    // Cedula, RNC, Passport
    public string DocumentNumber { get; set; }        // Número enmascarado
    public string DocumentNumberHash { get; set; }    // Hash para búsqueda
    public string FullName { get; set; }              // Nombre según documento
    public DateTime? DateOfBirth { get; set; }
    public DateTime? DocumentExpiry { get; set; }

    // Archivos
    public string FrontImageUrl { get; set; }         // Foto frontal del documento
    public string BackImageUrl { get; set; }          // Foto trasera
    public string SelfieUrl { get; set; }             // Selfie con documento

    // Verificación
    public VerificationStatus Status { get; set; }
    public VerificationMethod Method { get; set; }     // Manual, Automated, API
    public decimal ConfidenceScore { get; set; }       // 0-100

    // Resultado
    public bool IsVerified { get; set; }
    public string RejectionReason { get; set; }
    public Guid? VerifiedBy { get; set; }              // Admin que verificó (si manual)

    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }           // Verificación expira en 1 año
}

public enum VerificationType
{
    Individual,    // Persona natural con cédula
    Business       // Empresa con RNC
}

public enum DocumentType
{
    Cedula,
    RNC,
    Passport,
    DriversLicense
}

public enum VerificationStatus
{
    Pending,
    InReview,
    Approved,
    Rejected,
    Expired
}

public enum VerificationMethod
{
    Manual,        // Revisión por staff
    Automated,     // OCR + face match
    ApiValidation  // API de JCE/DGII
}
```

### TrustBadge

```csharp
public class TrustBadge
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public BadgeType Type { get; set; }

    public string Name { get; set; }
    public string Description { get; set; }
    public string IconUrl { get; set; }
    public string Color { get; set; }

    public DateTime EarnedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; }
}

public enum BadgeType
{
    VerifiedSeller,         // Identidad verificada
    VerifiedDealer,         // Dealer con documentos completos
    TrustedSeller,          // 5+ ventas exitosas
    TopRatedSeller,         // 4.5+ estrellas con 10+ reviews
    FastResponder,          // Responde en < 1 hora
    FoundingMember,         // Early bird
    PremiumDealer,          // Plan Enterprise
    SafeTransaction         // Usa pagos seguros
}
```

### SellerTrustScore

```csharp
public class SellerTrustScore
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    // Score general
    public int TrustScore { get; set; }               // 0-100
    public TrustLevel Level { get; set; }

    // Componentes del score
    public int IdentityScore { get; set; }            // Verificación de identidad
    public int TransactionScore { get; set; }         // Historial de ventas
    public int ResponseScore { get; set; }            // Tiempo de respuesta
    public int ReviewScore { get; set; }              // Calificaciones
    public int ListingQualityScore { get; set; }      // Calidad de listings

    // Estadísticas
    public int TotalSales { get; set; }
    public int TotalListings { get; set; }
    public decimal AverageResponseTimeMinutes { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }

    // Alertas
    public int FraudReportsReceived { get; set; }
    public int FraudReportsConfirmed { get; set; }
    public bool HasWarnings { get; set; }

    public DateTime CalculatedAt { get; set; }
}

public enum TrustLevel
{
    New = 0,          // Sin historial
    Bronze = 1,       // Score 1-39
    Silver = 2,       // Score 40-69
    Gold = 3,         // Score 70-89
    Platinum = 4      // Score 90-100
}
```

### FraudReport

```csharp
public class FraudReport
{
    public Guid Id { get; set; }

    // Reportador
    public Guid ReporterId { get; set; }
    public string ReporterEmail { get; set; }

    // Reportado
    public Guid? ReportedUserId { get; set; }
    public Guid? ReportedVehicleId { get; set; }
    public Guid? ReportedDealerId { get; set; }

    // Detalles
    public FraudType Type { get; set; }
    public string Description { get; set; }
    public List<string> EvidenceUrls { get; set; }    // Screenshots, conversaciones

    // Estado
    public FraudReportStatus Status { get; set; }
    public string Resolution { get; set; }
    public Guid? ResolvedBy { get; set; }

    // Acción tomada
    public FraudAction? ActionTaken { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

public enum FraudType
{
    FakeVehicle,           // Vehículo no existe
    WrongPhotos,           // Fotos de otro vehículo
    PriceScam,             // Precio irreal para estafar
    IdentityTheft,         // Se hace pasar por otro
    NonDelivery,           // Pagó y no entregó
    Counterfeit,           // Documentos falsos
    Other
}

public enum FraudReportStatus
{
    Pending,
    UnderReview,
    Confirmed,
    Dismissed,
    Resolved
}

public enum FraudAction
{
    Warning,               // Advertencia al usuario
    ListingRemoved,        // Listing eliminado
    TemporarySuspension,   // Suspensión temporal
    PermanentBan,          // Ban permanente
    LegalReferral          // Referido a autoridades
}
```

---

## 📊 Proceso TRUST-001: Verificación de Identidad

```
┌─────────────────────────────────────────────────────────────────────────┐
│ PROCESO: TRUST-001 - Verificación de Identidad                         │
├─────────────────────────────────────────────────────────────────────────┤
│ Actor Iniciador: USR-SELLER, DLR-ADMIN                                 │
│ Sistemas: TrustService, MediaService, NotificationService              │
│ Duración: Automático 1-5 min, Manual 24h                               │
│ Criticidad: CRÍTICA                                                    │
└─────────────────────────────────────────────────────────────────────────┘
```

| Paso | Subpaso | Acción                          | Sistema             | Actor      | Evidencia                | Código    |
| ---- | ------- | ------------------------------- | ------------------- | ---------- | ------------------------ | --------- |
| 1    | 1.1     | Usuario inicia verificación     | Frontend            | USR-SELLER | **Verification started** | EVD-AUDIT |
| 1    | 1.2     | Selecciona tipo de documento    | Frontend            | USR-SELLER | Doc type selected        | EVD-LOG   |
| 2    | 2.1     | Capturar foto frontal documento | Mobile/Web          | USR-SELLER | Image captured           | EVD-LOG   |
| 2    | 2.2     | Upload foto frontal             | MediaService        | USR-SELLER | **Front uploaded**       | EVD-FILE  |
| 2    | 3.1     | Capturar foto trasera           | Mobile/Web          | USR-SELLER | Image captured           | EVD-LOG   |
| 2    | 3.2     | Upload foto trasera             | MediaService        | USR-SELLER | **Back uploaded**        | EVD-FILE  |
| 3    | 4.1     | Capturar selfie con documento   | Mobile/Web          | USR-SELLER | Selfie captured          | EVD-LOG   |
| 3    | 4.2     | Upload selfie                   | MediaService        | USR-SELLER | **Selfie uploaded**      | EVD-FILE  |
| 4    | 4.1     | POST /api/trust/verify-identity | Gateway             | USR-SELLER | **Request**              | EVD-AUDIT |
| 4    | 4.2     | Validar imágenes                | TrustService        | Sistema    | Image validation         | EVD-LOG   |
| 5    | 5.1     | **OCR para extraer datos**      | MLService           | Sistema    | **OCR result**           | EVD-AUDIT |
| 5    | 5.2     | Extraer número de cédula/RNC    | MLService           | Sistema    | Number extracted         | EVD-LOG   |
| 5    | 5.3     | Extraer nombre completo         | MLService           | Sistema    | Name extracted           | EVD-LOG   |
| 6    | 6.1     | **Face matching**               | MLService           | Sistema    | **Face match score**     | EVD-AUDIT |
| 6    | 6.2     | Comparar selfie vs documento    | MLService           | Sistema    | Match result             | EVD-LOG   |
| 7    | 7.1     | **Validar con JCE/DGII**        | TrustService        | Sistema    | **API validation**       | EVD-AUDIT |
| 7    | 7.2     | Verificar cédula activa         | TrustService        | Sistema    | Cedula status            | EVD-LOG   |
| 8    | 8.1     | Calcular confidence score       | TrustService        | Sistema    | Score calculated         | EVD-LOG   |
| 8    | 8.2     | Si score > 85: auto-aprobar     | TrustService        | Sistema    | **Auto-approved**        | EVD-AUDIT |
| 8    | 8.3     | Si score < 85: enviar a manual  | TrustService        | Sistema    | **Manual review**        | EVD-AUDIT |
| 9    | 9.1     | **Crear IdentityVerification**  | TrustService        | Sistema    | **Record created**       | EVD-AUDIT |
| 9    | 9.2     | Hash de documento guardado      | TrustService        | Sistema    | Hash stored              | EVD-HASH  |
| 10   | 10.1    | Si aprobado: asignar badge      | TrustService        | Sistema    | **Badge assigned**       | EVD-EVENT |
| 10   | 10.2    | Actualizar perfil de usuario    | UserService         | Sistema    | Profile updated          | EVD-LOG   |
| 11   | 11.1    | **Notificar resultado**         | NotificationService | SYS-NOTIF  | **Result notification**  | EVD-COMM  |
| 12   | 12.1    | **Audit trail completo**        | AuditService        | Sistema    | Complete audit           | EVD-AUDIT |

### Evidencia de Verificación

```json
{
  "processCode": "TRUST-001",
  "verification": {
    "id": "ver-12345",
    "userId": "user-001",
    "type": "INDIVIDUAL",
    "document": {
      "type": "CEDULA",
      "number": "001-*****-8",
      "numberHash": "sha256:abc123...",
      "fullName": "Juan Antonio Pérez García",
      "dateOfBirth": "1985-03-15",
      "expiry": "2030-03-15"
    },
    "files": {
      "front": "s3://trust/ver-12345/front.jpg",
      "back": "s3://trust/ver-12345/back.jpg",
      "selfie": "s3://trust/ver-12345/selfie.jpg"
    },
    "analysis": {
      "ocr": {
        "confidence": 0.95,
        "extractedFields": {
          "number": 0.98,
          "name": 0.94,
          "dob": 0.92
        }
      },
      "faceMatch": {
        "score": 0.89,
        "liveness": true
      },
      "documentAuthenticity": {
        "score": 0.92,
        "hologramDetected": true,
        "tamperingDetected": false
      }
    },
    "externalValidation": {
      "source": "JCE",
      "status": "VALID",
      "lastUpdated": "2025-12-01"
    },
    "result": {
      "overallScore": 91,
      "status": "APPROVED",
      "method": "AUTOMATED",
      "timestamp": "2026-01-21T10:35:00Z"
    },
    "badge": {
      "type": "VERIFIED_SELLER",
      "earnedAt": "2026-01-21T10:35:00Z",
      "expiresAt": "2027-01-21T10:35:00Z"
    }
  }
}
```

---

## 📊 Proceso TRUST-002: Reporte de Fraude

```
┌─────────────────────────────────────────────────────────────────────────┐
│ PROCESO: TRUST-002 - Reporte de Fraude                                 │
├─────────────────────────────────────────────────────────────────────────┤
│ Actor Iniciador: USR-REG (cualquier usuario registrado)                │
│ Sistemas: TrustService, NotificationService, AdminService              │
│ Duración: Instantáneo → 24-48h resolución                              │
│ Criticidad: CRÍTICA                                                    │
└─────────────────────────────────────────────────────────────────────────┘
```

| Paso | Subpaso | Acción                         | Sistema             | Actor     | Evidencia             | Código    |
| ---- | ------- | ------------------------------ | ------------------- | --------- | --------------------- | --------- |
| 1    | 1.1     | Usuario hace clic "Reportar"   | Frontend            | USR-REG   | Report initiated      | EVD-LOG   |
| 1    | 1.2     | Selecciona tipo de fraude      | Frontend            | USR-REG   | Type selected         | EVD-LOG   |
| 1    | 1.3     | Escribe descripción            | Frontend            | USR-REG   | Description input     | EVD-LOG   |
| 1    | 1.4     | Sube evidencias                | MediaService        | USR-REG   | **Evidence uploaded** | EVD-FILE  |
| 2    | 2.1     | POST /api/trust/report-fraud   | Gateway             | USR-REG   | **Request**           | EVD-AUDIT |
| 2    | 2.2     | Validar datos                  | TrustService        | Sistema   | Validation            | EVD-LOG   |
| 3    | 3.1     | **Crear FraudReport**          | TrustService        | Sistema   | **Report created**    | EVD-AUDIT |
| 3    | 3.2     | Snapshot del listing reportado | TrustService        | Sistema   | **Listing snapshot**  | EVD-SNAP  |
| 4    | 4.1     | Verificar reportes previos     | TrustService        | Sistema   | Prior reports check   | EVD-LOG   |
| 4    | 4.2     | Si >3 reportes: auto-suspender | TrustService        | Sistema   | **Auto-suspend**      | EVD-AUDIT |
| 5    | 5.1     | **Notificar a Trust & Safety** | NotificationService | SYS-NOTIF | **Team notified**     | EVD-COMM  |
| 5    | 5.2     | Crear tarea en AdminService    | AdminService        | Sistema   | Task created          | EVD-LOG   |
| 6    | 6.1     | Confirmar recepción al usuario | NotificationService | SYS-NOTIF | **Confirmation**      | EVD-COMM  |
| 7    | 7.1     | **Audit trail**                | AuditService        | Sistema   | Complete audit        | EVD-AUDIT |

### [Resolución por Admin]

| Paso | Subpaso | Acción                        | Sistema             | Actor     | Evidencia             | Código    |
| ---- | ------- | ----------------------------- | ------------------- | --------- | --------------------- | --------- |
| 8    | 8.1     | Admin revisa reporte          | Frontend            | ADM-MOD   | Access log            | EVD-AUDIT |
| 8    | 8.2     | Revisa evidencias             | Frontend            | ADM-MOD   | Evidence reviewed     | EVD-LOG   |
| 8    | 8.3     | Contacta al reportado         | TrustService        | ADM-MOD   | Contact log           | EVD-COMM  |
| 9    | 9.1     | **Toma decisión**             | TrustService        | ADM-MOD   | **Decision**          | EVD-AUDIT |
| 9    | 9.2     | Documenta resolución          | TrustService        | ADM-MOD   | Resolution notes      | EVD-AUDIT |
| 10   | 10.1    | Si confirmado: aplicar acción | TrustService        | Sistema   | **Action applied**    | EVD-AUDIT |
| 10   | 10.2    | Notificar al reportado        | NotificationService | SYS-NOTIF | **Reportee notified** | EVD-COMM  |
| 10   | 10.3    | Notificar al reportador       | NotificationService | SYS-NOTIF | **Reporter notified** | EVD-COMM  |
| 11   | 11.1    | Actualizar trust score        | TrustService        | Sistema   | Score updated         | EVD-LOG   |
| 11   | 11.2    | Si ban: cerrar sesiones       | AuthService         | Sistema   | Sessions closed       | EVD-AUDIT |

### Evidencia de Reporte de Fraude

```json
{
  "processCode": "TRUST-002",
  "fraudReport": {
    "id": "fraud-12345",
    "reporter": {
      "userId": "user-001",
      "email": "reporter@email.com",
      "previousReports": 2,
      "accuracy": 0.85
    },
    "reported": {
      "type": "VEHICLE_LISTING",
      "vehicleId": "veh-67890",
      "sellerId": "user-999",
      "dealerId": null
    },
    "details": {
      "type": "WRONG_PHOTOS",
      "description": "Las fotos son de otro vehículo. Fui a ver el carro y era completamente diferente. El año no coincide y tiene daños que no aparecen en las fotos.",
      "evidence": [
        {
          "type": "SCREENSHOT",
          "url": "s3://trust/fraud-12345/evidence-1.jpg",
          "description": "Foto del listing"
        },
        {
          "type": "PHOTO",
          "url": "s3://trust/fraud-12345/evidence-2.jpg",
          "description": "Foto real del vehículo"
        },
        {
          "type": "CHAT_SCREENSHOT",
          "url": "s3://trust/fraud-12345/evidence-3.jpg",
          "description": "Conversación con el vendedor"
        }
      ]
    },
    "listingSnapshot": {
      "capturedAt": "2026-01-21T10:30:00Z",
      "title": "Toyota Corolla 2023",
      "price": 1200000,
      "images": ["img1.jpg", "img2.jpg"],
      "description": "..."
    },
    "status": "CONFIRMED",
    "resolution": {
      "decidedBy": {
        "type": "ADM-MOD",
        "id": "admin-001",
        "name": "Ana Moderadora"
      },
      "decision": "CONFIRMED_FRAUD",
      "actionTaken": "LISTING_REMOVED",
      "additionalActions": ["WARNING_ISSUED"],
      "notes": "Fotos confirmadas de diferente vehículo. Primera ofensa, se emite advertencia.",
      "timestamp": "2026-01-21T14:30:00Z"
    },
    "impact": {
      "sellerTrustScoreChange": -25,
      "sellerNewScore": 45,
      "warningIssued": true
    },
    "notifications": {
      "reporter": {
        "sent": true,
        "message": "Tu reporte fue confirmado. El listing ha sido removido."
      },
      "reported": {
        "sent": true,
        "message": "Tu listing fue removido por violar políticas de fotos."
      }
    }
  }
}
```

---

## 📊 Cálculo del Trust Score

```csharp
public class TrustScoreCalculator
{
    public SellerTrustScore Calculate(Guid userId)
    {
        var score = new SellerTrustScore { UserId = userId };

        // 1. Identity Score (25%)
        var verification = _verificationRepo.GetLatest(userId);
        score.IdentityScore = verification?.IsVerified == true ? 100 : 0;

        // 2. Transaction Score (25%)
        var sales = _salesRepo.GetCompletedSales(userId);
        score.TotalSales = sales.Count;
        score.TransactionScore = Math.Min(100, sales.Count * 10);

        // 3. Response Score (20%)
        var avgResponse = _messageRepo.GetAverageResponseTime(userId);
        score.AverageResponseTimeMinutes = avgResponse;
        score.ResponseScore = avgResponse switch
        {
            < 30 => 100,      // < 30 min = 100
            < 60 => 90,       // < 1 hora = 90
            < 120 => 70,      // < 2 horas = 70
            < 240 => 50,      // < 4 horas = 50
            < 1440 => 30,     // < 24 horas = 30
            _ => 10           // > 24 horas = 10
        };

        // 4. Review Score (20%)
        var reviews = _reviewRepo.GetReviews(userId);
        score.TotalReviews = reviews.Count;
        score.AverageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;
        score.ReviewScore = (int)(score.AverageRating * 20);  // 5 stars = 100

        // 5. Listing Quality Score (10%)
        var listings = _vehicleRepo.GetByUser(userId);
        score.TotalListings = listings.Count;
        var avgImages = listings.Average(l => l.ImageCount);
        var avgDescLength = listings.Average(l => l.Description?.Length ?? 0);
        score.ListingQualityScore = CalculateListingQuality(avgImages, avgDescLength);

        // Penalties
        var fraudReports = _fraudRepo.GetConfirmedAgainst(userId);
        score.FraudReportsConfirmed = fraudReports.Count;
        var penalty = fraudReports.Count * 15;  // -15 por cada fraude confirmado

        // Calculate total
        score.TrustScore = (int)(
            score.IdentityScore * 0.25 +
            score.TransactionScore * 0.25 +
            score.ResponseScore * 0.20 +
            score.ReviewScore * 0.20 +
            score.ListingQualityScore * 0.10
        ) - penalty;

        score.TrustScore = Math.Max(0, Math.Min(100, score.TrustScore));

        // Determine level
        score.Level = score.TrustScore switch
        {
            >= 90 => TrustLevel.Platinum,
            >= 70 => TrustLevel.Gold,
            >= 40 => TrustLevel.Silver,
            > 0 => TrustLevel.Bronze,
            _ => TrustLevel.New
        };

        score.CalculatedAt = DateTime.UtcNow;
        return score;
    }
}
```

---

## 🏅 Badges y sus Criterios

| Badge                | Criterio                    | Icono        | Color    |
| -------------------- | --------------------------- | ------------ | -------- |
| **Verified Seller**  | Identidad verificada        | ✓ Shield     | Azul     |
| **Verified Dealer**  | Dealer con docs completos   | ✓ Building   | Verde    |
| **Trusted Seller**   | 5+ ventas exitosas          | ⭐ Star      | Dorado   |
| **Top Rated**        | 4.5+ estrellas, 10+ reviews | 👑 Crown     | Púrpura  |
| **Fast Responder**   | Responde en < 1 hora avg    | ⚡ Lightning | Amarillo |
| **Founding Member**  | Early Bird inscrito         | 🏆 Trophy    | Bronce   |
| **Premium Dealer**   | Plan Enterprise             | 💎 Diamond   | Platino  |
| **Safe Transaction** | Usa pagos seguros           | 🔒 Lock      | Verde    |

---

## 📊 Métricas Prometheus

```yaml
# Verificaciones
trust_verifications_total{type, status, method}
trust_verification_processing_time_seconds

# Reportes de fraude
trust_fraud_reports_total{type, status}
trust_fraud_report_resolution_time_hours
trust_fraud_confirmed_rate

# Trust Scores
trust_score_distribution{level}
trust_score_average

# Badges
trust_badges_awarded_total{type}
```

---

## 🔗 Referencias

- [02-USUARIOS-DEALERS/01-user-service.md](../02-USUARIOS-DEALERS/01-user-service.md)
- [07-REVIEWS-REPUTACION/01-review-service.md](../07-REVIEWS-REPUTACION/01-review-service.md)
- [12-ADMINISTRACION/01-admin-service.md](../12-ADMINISTRACION/01-admin-service.md)
- [JCE Portal](https://jce.gob.do)
- [DGII Portal](https://dgii.gov.do)
