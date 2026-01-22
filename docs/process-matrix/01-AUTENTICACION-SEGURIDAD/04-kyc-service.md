# 🔍 KYC Service - Matriz de Procesos

> **Servicio:** KYCService  
> **Puerto:** 5020  
> **Última actualización:** Enero 21, 2026  
> **Estado:** 🟢 ACTIVO  
> **Regulación:** Ley 155-17 (Prevención Lavado de Activos)

---

## 1. Información General

### 1.1 Descripción

Sistema de Know Your Customer (KYC) para verificación de identidad de usuarios y dealers en OKLA. Cumple con la Ley 155-17 de Prevención de Lavado de Activos de República Dominicana y normativas de la Unidad de Análisis Financiero (UAF).

### 1.2 Dependencias

| Servicio            | Propósito                    |
| ------------------- | ---------------------------- |
| UserService         | Información de usuarios      |
| ComplianceService   | Verificaciones de compliance |
| MediaService        | Almacenamiento de documentos |
| NotificationService | Alertas de estado KYC        |

### 1.3 Componentes

- **KYCProfilesController**: Gestión de perfiles KYC
- **KYCDocumentsController**: Gestión de documentos

---

## 2. Endpoints API

### 2.1 KYCProfilesController

| Método | Endpoint                                     | Descripción                 | Auth | Roles               |
| ------ | -------------------------------------------- | --------------------------- | ---- | ------------------- |
| `GET`  | `/api/kycprofiles`                           | Listar perfiles con filtros | ✅   | Admin, Compliance   |
| `GET`  | `/api/kycprofiles/{id}`                      | Obtener perfil por ID       | ✅   | User (owner), Admin |
| `GET`  | `/api/kycprofiles/user/{userId}`             | Obtener por User ID         | ✅   | User (owner), Admin |
| `GET`  | `/api/kycprofiles/document/{documentNumber}` | Buscar por documento        | ✅   | Admin, Compliance   |
| `POST` | `/api/kycprofiles`                           | Crear perfil KYC            | ✅   | User                |
| `PUT`  | `/api/kycprofiles/{id}`                      | Actualizar perfil           | ✅   | User (owner), Admin |
| `POST` | `/api/kycprofiles/{id}/approve`              | Aprobar perfil              | ✅   | Admin, Compliance   |
| `POST` | `/api/kycprofiles/{id}/reject`               | Rechazar perfil             | ✅   | Admin, Compliance   |
| `GET`  | `/api/kycprofiles/pending`                   | Perfiles pendientes         | ✅   | Admin, Compliance   |
| `GET`  | `/api/kycprofiles/expiring`                  | Perfiles próximos a expirar | ✅   | Admin, Compliance   |
| `GET`  | `/api/kycprofiles/statistics`                | Estadísticas KYC            | ✅   | Admin, Compliance   |

### 2.2 KYCDocumentsController

| Método   | Endpoint                                | Descripción             | Auth | Roles               |
| -------- | --------------------------------------- | ----------------------- | ---- | ------------------- |
| `GET`    | `/api/kycdocuments/profile/{profileId}` | Documentos de un perfil | ✅   | User, Admin         |
| `POST`   | `/api/kycdocuments`                     | Subir documento         | ✅   | User                |
| `PUT`    | `/api/kycdocuments/{id}/verify`         | Verificar documento     | ✅   | Admin, Compliance   |
| `DELETE` | `/api/kycdocuments/{id}`                | Eliminar documento      | ✅   | User (owner), Admin |

---

## 3. Entidades y Enums

### 3.1 KYCStatus (Enum)

```csharp
public enum KYCStatus
{
    NotStarted = 0,        // Usuario no ha iniciado KYC
    InProgress = 1,        // Documentos en proceso de subida
    PendingReview = 2,     // Esperando revisión de compliance
    UnderReview = 3,       // En revisión activa
    Approved = 4,          // KYC aprobado
    Rejected = 5,          // KYC rechazado
    Expired = 6,           // KYC expirado (requiere renovación)
    Suspended = 7          // Suspendido por investigación
}
```

### 3.2 RiskLevel (Enum)

```csharp
public enum RiskLevel
{
    Low = 0,               // Bajo riesgo - verificación estándar
    Medium = 1,            // Riesgo medio - revisión adicional
    High = 2,              // Alto riesgo - due diligence reforzada
    Critical = 3           // Crítico - requiere escalamiento
}
```

### 3.3 DocumentType (Enum)

```csharp
public enum DocumentType
{
    // Documentos de identidad
    Cedula = 0,            // Cédula dominicana
    Passport = 1,          // Pasaporte
    DriverLicense = 2,     // Licencia de conducir

    // Comprobantes de dirección
    UtilityBill = 10,      // Factura de servicios
    BankStatement = 11,    // Estado de cuenta bancario
    LeaseAgreement = 12,   // Contrato de alquiler

    // Documentos de negocio (Dealers)
    RNC = 20,              // Registro Nacional Contribuyente
    MercantileRegistry = 21,// Registro Mercantil
    BusinessLicense = 22,  // Licencia comercial
    TaxCertificate = 23,   // Certificación DGII

    // Documentos financieros
    IncomeProof = 30,      // Comprobante de ingresos
    TaxReturn = 31,        // Declaración de impuestos

    // Selfie/Verificación
    Selfie = 40,           // Foto selfie
    SelfieWithDocument = 41 // Selfie con documento
}
```

### 3.4 KYCProfile (Entidad Principal)

```csharp
public class KYCProfile
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    // Información Personal
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string DocumentNumber { get; set; }      // Cédula/Pasaporte
    public DocumentType DocumentType { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Nationality { get; set; }

    // Información de Contacto
    public string Address { get; set; }
    public string City { get; set; }
    public string Province { get; set; }
    public string PhoneNumber { get; set; }

    // Estado y Riesgo
    public KYCStatus Status { get; set; }
    public RiskLevel RiskLevel { get; set; }
    public decimal RiskScore { get; set; }          // 0-100

    // PEP (Persona Expuesta Políticamente)
    public bool IsPEP { get; set; }
    public string? PEPPosition { get; set; }
    public string? PEPRelationship { get; set; }

    // Fuente de Fondos
    public string SourceOfFunds { get; set; }
    public string Occupation { get; set; }
    public decimal? ExpectedMonthlyTransaction { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string? RejectionReason { get; set; }

    // Documentos
    public List<KYCDocument> Documents { get; set; }
}
```

---

## 4. Procesos Detallados

### 4.1 KYC-PROF-001: Crear Perfil KYC

| Campo       | Valor                    |
| ----------- | ------------------------ |
| **ID**      | KYC-PROF-001             |
| **Nombre**  | Iniciar Verificación KYC |
| **Actor**   | Usuario registrado       |
| **Trigger** | POST /api/kycprofiles    |

#### Flujo del Proceso

| Paso | Acción                     | Sistema           | Validación              |
| ---- | -------------------------- | ----------------- | ----------------------- |
| 1    | Usuario inicia KYC         | Frontend          | Usuario autenticado     |
| 2    | Verificar perfil no existe | KYCService        | UserId único            |
| 3    | Validar datos personales   | KYCService        | Formato cédula RD       |
| 4    | Verificar cédula en JCE    | External API      | Opcional, si disponible |
| 5    | Calcular RiskScore inicial | KYCService        | Algoritmo interno       |
| 6    | Verificar lista PEP        | ComplianceService | Contra base datos UAF   |
| 7    | Verificar sanciones        | ComplianceService | OFAC, UN, EU lists      |
| 8    | Crear perfil               | Database          | Status = InProgress     |
| 9    | Publicar evento            | RabbitMQ          | KYCProfileCreated       |

#### Request

```json
{
  "userId": "uuid",
  "firstName": "Juan",
  "lastName": "Pérez",
  "documentNumber": "001-0000000-0",
  "documentType": "Cedula",
  "dateOfBirth": "1985-06-15",
  "nationality": "Dominicana",
  "address": "Calle Principal #123",
  "city": "Santo Domingo",
  "province": "Distrito Nacional",
  "phoneNumber": "+1 809-555-1234",
  "sourceOfFunds": "Salary",
  "occupation": "Engineer",
  "expectedMonthlyTransaction": 50000.0
}
```

#### Response

```json
{
  "id": "uuid",
  "userId": "uuid",
  "status": "InProgress",
  "riskLevel": "Low",
  "riskScore": 25.5,
  "isPEP": false,
  "requiredDocuments": ["Cedula", "UtilityBill", "SelfieWithDocument"],
  "createdAt": "2026-01-21T10:00:00Z"
}
```

---

### 4.2 KYC-DOC-001: Subir Documento KYC

| Campo       | Valor                           |
| ----------- | ------------------------------- |
| **ID**      | KYC-DOC-001                     |
| **Nombre**  | Subir Documento de Verificación |
| **Actor**   | Usuario con perfil KYC          |
| **Trigger** | POST /api/kycdocuments          |

#### Flujo del Proceso

| Paso | Acción                     | Sistema      | Validación            |
| ---- | -------------------------- | ------------ | --------------------- |
| 1    | Usuario sube documento     | Frontend     | Imagen/PDF            |
| 2    | Validar tipo de archivo    | KYCService   | jpg, png, pdf         |
| 3    | Validar tamaño             | KYCService   | Max 10MB              |
| 4    | Escanear malware           | MediaService | ClamAV                |
| 5    | Verificar calidad imagen   | KYCService   | Min 300 DPI           |
| 6    | OCR extracción datos       | KYCService   | Tesseract/Azure       |
| 7    | Validar datos vs perfil    | KYCService   | Nombre, cédula match  |
| 8    | Almacenar encriptado       | MediaService | S3 + encryption       |
| 9    | Actualizar perfil          | Database     | Documento agregado    |
| 10   | Verificar completitud      | KYCService   | Todos docs requeridos |
| 11   | Cambiar status si completo | Database     | PendingReview         |

#### Request (multipart/form-data)

```
profileId: uuid
documentType: Cedula
file: [binary]
side: Front  // Front, Back (para cédula)
```

---

### 4.3 KYC-REV-001: Aprobar Perfil KYC

| Campo       | Valor                              |
| ----------- | ---------------------------------- |
| **ID**      | KYC-REV-001                        |
| **Nombre**  | Aprobar Verificación KYC           |
| **Actor**   | Oficial de Compliance              |
| **Trigger** | POST /api/kycprofiles/{id}/approve |

#### Flujo del Proceso

| Paso | Acción                        | Sistema             | Validación                |
| ---- | ----------------------------- | ------------------- | ------------------------- |
| 1    | Compliance revisa perfil      | Dashboard           | Documentos visibles       |
| 2    | Verificar todos documentos OK | KYCService          | Cada doc verificado       |
| 3    | Revisar alertas PEP/Sanciones | ComplianceService   | Ninguna pendiente         |
| 4    | Aprobar perfil                | KYCService          | Con comentarios           |
| 5    | Calcular fecha expiración     | KYCService          | +1 año para Low risk      |
| 6    | Actualizar status             | Database            | Approved                  |
| 7    | Actualizar UserService        | HTTP                | user.IsKYCVerified = true |
| 8    | Publicar evento               | RabbitMQ            | KYCApproved               |
| 9    | Notificar usuario             | NotificationService | Email + Push              |

#### Request

```json
{
  "id": "uuid",
  "approvedBy": "compliance@okla.com.do",
  "comments": "All documents verified. Identity confirmed.",
  "expiresAt": "2027-01-21T00:00:00Z"
}
```

---

### 4.4 KYC-REV-002: Rechazar Perfil KYC

| Campo       | Valor                             |
| ----------- | --------------------------------- |
| **ID**      | KYC-REV-002                       |
| **Nombre**  | Rechazar Verificación KYC         |
| **Actor**   | Oficial de Compliance             |
| **Trigger** | POST /api/kycprofiles/{id}/reject |

#### Flujo del Proceso

| Paso | Acción                         | Sistema             | Validación               |
| ---- | ------------------------------ | ------------------- | ------------------------ |
| 1    | Compliance identifica problema | Dashboard           | Documento inválido, etc. |
| 2    | Seleccionar razón de rechazo   | Frontend            | Lista predefinida        |
| 3    | Agregar comentarios            | Frontend            | Detalles específicos     |
| 4    | Rechazar perfil                | KYCService          | Con razón obligatoria    |
| 5    | Actualizar status              | Database            | Rejected                 |
| 6    | Publicar evento                | RabbitMQ            | KYCRejected              |
| 7    | Notificar usuario              | NotificationService | Con razón y pasos        |

#### Request

```json
{
  "id": "uuid",
  "rejectionReason": "DocumentExpired",
  "comments": "La cédula presentada está expirada. Por favor suba un documento vigente.",
  "canRetry": true
}
```

---

### 4.5 KYC-MON-001: Monitoreo de Expiración

| Campo       | Valor                         |
| ----------- | ----------------------------- |
| **ID**      | KYC-MON-001                   |
| **Nombre**  | Monitoreo de KYC por Expirar  |
| **Actor**   | Sistema (Scheduled Job)       |
| **Trigger** | GET /api/kycprofiles/expiring |

#### Flujo del Proceso

| Paso | Acción                      | Sistema             | Validación                |
| ---- | --------------------------- | ------------------- | ------------------------- |
| 1    | Job diario ejecuta          | SchedulerService    | 6:00 AM                   |
| 2    | Buscar perfiles por expirar | KYCService          | ExpiresAt < Now + 30 días |
| 3    | Por cada perfil             | Loop                | Procesar                  |
| 4    | Enviar recordatorio         | NotificationService | Email + Push              |
| 5    | Si expirado                 | KYCService          | Status = Expired          |
| 6    | Restringir funcionalidades  | UserService         | Limitar transacciones     |
| 7    | Generar reporte             | ReportingService    | Para compliance           |

---

## 5. Reglas de Negocio

### 5.1 Documentos Requeridos por Tipo de Usuario

| Tipo Usuario           | Documentos Requeridos                                                         |
| ---------------------- | ----------------------------------------------------------------------------- |
| Individual (Comprador) | Cédula, UtilityBill                                                           |
| Individual (Vendedor)  | Cédula, UtilityBill, SelfieWithDocument                                       |
| Dealer                 | RNC, MercantileRegistry, BusinessLicense, TaxCertificate, Cédula (rep. legal) |

### 5.2 Cálculo de RiskScore

| Factor                  | Peso | Descripción                    |
| ----------------------- | ---- | ------------------------------ |
| Nacionalidad            | 20%  | RD=bajo, otros=variable        |
| Ocupación               | 15%  | Alto riesgo: cambista, casino  |
| PEP                     | 25%  | +50 puntos si es PEP           |
| Fuente de fondos        | 20%  | Salary=bajo, Investments=medio |
| Transacciones esperadas | 20%  | >$100K/mes = alto              |

### 5.3 Vigencia KYC

| Risk Level | Vigencia | Renovación    |
| ---------- | -------- | ------------- |
| Low        | 2 años   | 30 días antes |
| Medium     | 1 año    | 45 días antes |
| High       | 6 meses  | 60 días antes |
| Critical   | 3 meses  | 90 días antes |

### 5.4 Límites por Estado KYC

| KYC Status      | Límite Transacción | Funcionalidades |
| --------------- | ------------------ | --------------- |
| NotStarted      | $0                 | Solo navegación |
| InProgress      | $0                 | Solo navegación |
| Approved (Low)  | $500,000/mes       | Todas           |
| Approved (High) | $100,000/mes       | Con monitoreo   |
| Expired         | $0                 | Bloqueado       |

---

## 6. Manejo de Errores

| Código | Error           | Mensaje                        | Acción                |
| ------ | --------------- | ------------------------------ | --------------------- |
| 400    | InvalidCedula   | "Formato de cédula inválido"   | Verificar formato     |
| 400    | DocumentExpired | "El documento está expirado"   | Subir vigente         |
| 400    | LowQualityImage | "Imagen de baja calidad"       | Tomar mejor foto      |
| 400    | DataMismatch    | "Los datos no coinciden"       | Verificar información |
| 404    | ProfileNotFound | "Perfil KYC no encontrado"     | Crear perfil primero  |
| 409    | ProfileExists   | "Ya existe un perfil KYC"      | Usar existente        |
| 409    | DocumentExists  | "Este documento ya fue subido" | No duplicar           |

---

## 7. Eventos RabbitMQ

| Evento                  | Exchange     | Descripción          | Payload                         |
| ----------------------- | ------------ | -------------------- | ------------------------------- |
| `kyc.profile.created`   | `kyc.events` | Perfil creado        | `{ profileId, userId, status }` |
| `kyc.profile.updated`   | `kyc.events` | Perfil actualizado   | `{ profileId, changes }`        |
| `kyc.profile.approved`  | `kyc.events` | Perfil aprobado      | `{ profileId, approvedBy }`     |
| `kyc.profile.rejected`  | `kyc.events` | Perfil rechazado     | `{ profileId, reason }`         |
| `kyc.profile.expired`   | `kyc.events` | Perfil expirado      | `{ profileId, expiresAt }`      |
| `kyc.document.uploaded` | `kyc.events` | Documento subido     | `{ docId, type, profileId }`    |
| `kyc.document.verified` | `kyc.events` | Documento verificado | `{ docId, verifiedBy }`         |
| `kyc.pep.detected`      | `kyc.events` | PEP detectado        | `{ profileId, pepInfo }`        |

---

## 8. Integración con Compliance (Ley 155-17)

### 8.1 Verificaciones Automáticas

```
┌──────────────┐     ┌──────────────┐     ┌───────────────┐
│  KYCService  │────>│ Compliance   │────>│  UAF Listas   │
│              │     │   Service    │     │  PEP/Sanciones│
└──────────────┘     └──────────────┘     └───────────────┘
       │                    │                     │
       │                    ▼                     │
       │            ┌──────────────┐              │
       │            │    OFAC      │<─────────────┤
       │            │  Sanctions   │              │
       │            └──────────────┘              │
       │                                          │
       ▼                                          ▼
┌──────────────┐                         ┌───────────────┐
│  JCE (Cédula)│                         │  World Check  │
│  Validation  │                         │  (optional)   │
└──────────────┘                         └───────────────┘
```

### 8.2 Reportes UAF Requeridos

| Reporte              | Frecuencia | Contenido               |
| -------------------- | ---------- | ----------------------- |
| ROS                  | Inmediato  | Operaciones sospechosas |
| Transacciones > $10K | Mensual    | Todas las transacciones |
| PEP Activos          | Trimestral | Lista de PEPs           |
| Estadísticas KYC     | Mensual    | Aprobados/Rechazados    |

---

## 9. Métricas y Dashboard

### 9.1 KPIs Principales

```
# Perfiles por estado
kyc_profiles_by_status{status="approved|pending|rejected"}

# Tiempo promedio de aprobación
kyc_approval_time_seconds_avg

# Documentos procesados
kyc_documents_processed_total{type="cedula|passport"}

# PEPs detectados
kyc_pep_detected_total

# Verificaciones por día
kyc_verifications_daily
```

### 9.2 Alertas

| Alerta           | Condición           | Severidad |
| ---------------- | ------------------- | --------- |
| HighPendingQueue | >50 pendientes      | Warning   |
| PEPDetected      | Nuevo PEP           | Critical  |
| SanctionMatch    | Match en OFAC       | Critical  |
| ExpiringSoon     | >20 por expirar hoy | Warning   |

---

## 10. Configuración

### 10.1 appsettings.json

```json
{
  "KYC": {
    "ExpirationDays": {
      "Low": 730,
      "Medium": 365,
      "High": 180,
      "Critical": 90
    },
    "ReminderDays": [30, 14, 7, 1],
    "MaxDocumentSizeMB": 10,
    "AllowedFileTypes": ["jpg", "jpeg", "png", "pdf"],
    "OCREnabled": true,
    "AutoApprovalEnabled": false
  },
  "Compliance": {
    "PEPCheckEnabled": true,
    "OFACCheckEnabled": true,
    "JCEValidationEnabled": false
  }
}
```

---

## 📚 Referencias

- [Ley 155-17](https://uaf.gob.do/ley-155-17/) - Prevención Lavado de Activos
- [01-compliance-service.md](../08-COMPLIANCE-LEGAL-RD/01-compliance-service.md) - Compliance general
- [OFAC Sanctions Lists](https://sanctionssearch.ofac.treas.gov/)
