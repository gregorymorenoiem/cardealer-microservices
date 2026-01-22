# 📁 Document Storage - Almacenamiento de Documentos - Matriz de Procesos

> **Componente:** DocumentStorageService  
> **Última actualización:** Enero 21, 2026  
> **Estado:** 🟢 ACTIVO

---

## 1. Información General

### 1.1 Descripción

Sistema de almacenamiento seguro para documentos sensibles de la plataforma OKLA. Maneja documentos legales, fiscales, de identidad y contractuales con encriptación, auditoría y cumplimiento regulatorio.

### 1.2 Tipos de Documentos

| Categoría        | Descripción                           | Retención |
| ---------------- | ------------------------------------- | --------- |
| **Identidad**    | Cédulas, pasaportes, licencias        | 10 años   |
| **Fiscal**       | NCF, facturas, reportes DGII          | 10 años   |
| **Legal**        | Contratos, acuerdos, T&C firmados     | 10 años   |
| **Verificación** | Documentos de dealer (RNC, licencias) | 10 años   |
| **Financiero**   | Comprobantes de pago, recibos         | 10 años   |
| **Temporal**     | Uploads en proceso                    | 24 horas  |

### 1.3 Dependencias

| Servicio            | Propósito          |
| ------------------- | ------------------ |
| DigitalOcean Spaces | Object storage     |
| AWS KMS             | Key management     |
| PostgreSQL          | Metadata           |
| Redis               | Cache de URLs      |
| AuditService        | Logging de accesos |

---

## 2. Endpoints API

### 2.1 DocumentsController

| Método   | Endpoint                           | Descripción           | Auth | Roles             |
| -------- | ---------------------------------- | --------------------- | ---- | ----------------- |
| `POST`   | `/api/documents/upload`            | Subir documento       | ✅   | User              |
| `GET`    | `/api/documents/{id}`              | Obtener metadata      | ✅   | Owner/Admin       |
| `GET`    | `/api/documents/{id}/download`     | Descargar documento   | ✅   | Owner/Admin       |
| `GET`    | `/api/documents/{id}/preview`      | Vista previa (PDF)    | ✅   | Owner/Admin       |
| `DELETE` | `/api/documents/{id}`              | Eliminar documento    | ✅   | Owner             |
| `GET`    | `/api/documents/user/{userId}`     | Documentos de usuario | ✅   | Self/Admin        |
| `GET`    | `/api/documents/dealer/{dealerId}` | Documentos de dealer  | ✅   | DealerOwner/Admin |

### 2.2 VerificationDocumentsController

| Método | Endpoint                                   | Descripción                     | Auth | Roles  |
| ------ | ------------------------------------------ | ------------------------------- | ---- | ------ |
| `POST` | `/api/documents/verification/upload`       | Subir documento de verificación | ✅   | Dealer |
| `GET`  | `/api/documents/verification/dealer/{id}`  | Documentos pendientes           | ✅   | Admin  |
| `PUT`  | `/api/documents/verification/{id}/approve` | Aprobar documento               | ✅   | Admin  |
| `PUT`  | `/api/documents/verification/{id}/reject`  | Rechazar documento              | ✅   | Admin  |

---

## 3. Entidades y Enums

### 3.1 DocumentType (Enum)

```csharp
public enum DocumentType
{
    // Identidad
    Cedula = 0,
    Passport = 1,
    DriverLicense = 2,

    // Fiscal
    RNCCertificate = 10,
    InvoicePDF = 11,
    TaxReport = 12,
    DGII606 = 13,
    DGII607 = 14,
    DGII608 = 15,

    // Legal
    SalesContract = 20,
    PowerOfAttorney = 21,
    TermsAcceptance = 22,
    PrivacyConsent = 23,

    // Dealer Verification
    BusinessLicense = 30,
    ProofOfAddress = 31,
    BankCertificate = 32,
    InsurancePolicy = 33,

    // Vehicle
    VehicleTitle = 40,
    MatriculaDocument = 41,
    InspectionCertificate = 42,
    ImportDocument = 43,

    // Financial
    PaymentReceipt = 50,
    BankStatement = 51,

    // Other
    Other = 99
}
```

### 3.2 DocumentStatus (Enum)

```csharp
public enum DocumentStatus
{
    Uploading = 0,
    Processing = 1,
    Active = 2,
    PendingVerification = 3,
    Verified = 4,
    Rejected = 5,
    Expired = 6,
    Archived = 7,
    Deleted = 8
}
```

### 3.3 Document (Entidad)

```csharp
public class Document
{
    public Guid Id { get; set; }
    public string FileName { get; set; }
    public string OriginalFileName { get; set; }
    public string ContentType { get; set; }
    public long SizeBytes { get; set; }

    // Clasificación
    public DocumentType Type { get; set; }
    public DocumentStatus Status { get; set; }
    public bool IsConfidential { get; set; }

    // Almacenamiento
    public string StorageKey { get; set; }          // Path en S3
    public string EncryptionKeyId { get; set; }     // KMS Key ID
    public string Checksum { get; set; }            // SHA-256

    // Ownership
    public Guid? UserId { get; set; }
    public Guid? DealerId { get; set; }
    public Guid? VehicleId { get; set; }
    public Guid? TransactionId { get; set; }

    // Verificación (si aplica)
    public bool RequiresVerification { get; set; }
    public Guid? VerifiedById { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string? VerificationNotes { get; set; }
    public string? RejectionReason { get; set; }

    // Expiración
    public DateTime? ExpiresAt { get; set; }
    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt < DateTime.UtcNow;

    // Metadata
    public Dictionary<string, string> Metadata { get; set; }

    // Retención
    public DateTime RetentionUntil { get; set; }
    public bool CanBeDeleted => DateTime.UtcNow > RetentionUntil;

    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public bool IsDeleted { get; set; }
}
```

### 3.4 DocumentAccess (Entidad - Auditoría)

```csharp
public class DocumentAccess
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public Guid AccessedById { get; set; }
    public DocumentAccessType AccessType { get; set; }  // View, Download, Delete
    public string IpAddress { get; set; }
    public string UserAgent { get; set; }
    public DateTime AccessedAt { get; set; }
}
```

---

## 4. Procesos Detallados

### 4.1 DOC-001: Upload de Documento de Identidad

| Campo       | Valor                      |
| ----------- | -------------------------- |
| **ID**      | DOC-001                    |
| **Nombre**  | Identity Document Upload   |
| **Actor**   | Usuario/Dealer             |
| **Trigger** | POST /api/documents/upload |

#### Flujo del Proceso

| Paso | Acción                   | Sistema             | Validación                   |
| ---- | ------------------------ | ------------------- | ---------------------------- |
| 1    | Seleccionar documento    | Frontend            | File picker                  |
| 2    | Validar client-side      | Frontend            | PDF/imagen, < 5MB            |
| 3    | Mostrar preview          | Frontend            | Local                        |
| 4    | POST multipart/form-data | API                 | Con tipo                     |
| 5    | Validar autenticación    | AuthMiddleware      | JWT                          |
| 6    | Validar archivo          | DocumentService     | Ver tabla                    |
| 7    | Generar checksum         | SHA-256             | Integridad                   |
| 8    | Cifrar documento         | AES-256             | Con KMS key                  |
| 9    | Generar storage key      | DocumentService     | Path seguro                  |
| 10   | Subir a S3               | S3Client            | Server-side encryption       |
| 11   | Crear registro en DB     | Database            | Status = Active              |
| 12   | Log de auditoría         | AuditService        | Upload event                 |
| 13   | Si requiere verificación | Marcar              | Status = PendingVerification |
| 14   | Notificar admins         | NotificationService | Si es verificación           |
| 15   | Response                 | API                 | 201 Created                  |

#### Validaciones

| Validación             | Valor         |
| ---------------------- | ------------- |
| Formatos permitidos    | PDF, JPG, PNG |
| Tamaño máximo          | 5 MB          |
| Documentos por usuario | Max 20        |
| Nombre de archivo      | Sanitizado    |
| Virus scan             | ClamAV        |

---

### 4.2 DOC-002: Descarga Segura de Documento

| Campo       | Valor                            |
| ----------- | -------------------------------- |
| **ID**      | DOC-002                          |
| **Nombre**  | Secure Document Download         |
| **Actor**   | Usuario autorizado               |
| **Trigger** | GET /api/documents/{id}/download |

#### Flujo del Proceso

| Paso | Acción                  | Sistema         | Validación       |
| ---- | ----------------------- | --------------- | ---------------- |
| 1    | Request download        | API             | GET              |
| 2    | Validar autenticación   | AuthMiddleware  | JWT              |
| 3    | Obtener documento       | Database        | Por ID           |
| 4    | Verificar ownership     | DocumentService | Es dueño o Admin |
| 5    | Verificar status        | DocumentService | Status = Active  |
| 6    | Log de auditoría        | AuditService    | Download event   |
| 7    | Generar URL pre-firmada | S3Client        | Expiry 5 min     |
| 8    | Redirect a URL          | Response        | 302              |
| 9    | O stream directo        | Response        | Binary data      |

#### Headers de Seguridad

```http
Content-Disposition: attachment; filename="documento.pdf"
Content-Type: application/pdf
X-Content-Type-Options: nosniff
Cache-Control: private, no-store
```

---

### 4.3 DOC-003: Verificación de Documentos de Dealer

| Campo       | Valor                        |
| ----------- | ---------------------------- |
| **ID**      | DOC-003                      |
| **Nombre**  | Dealer Document Verification |
| **Actor**   | Admin                        |
| **Trigger** | Documento subido por Dealer  |

#### Flujo del Proceso

| Paso | Acción                       | Sistema             | Validación           |
| ---- | ---------------------------- | ------------------- | -------------------- |
| 1    | Dealer sube documentos       | API                 | RNC, licencia, etc.  |
| 2    | Status = PendingVerification | Database            | Automático           |
| 3    | Notificar admins             | NotificationService | Email + dashboard    |
| 4    | Admin accede a panel         | AdminUI             | /admin/verifications |
| 5    | Ver documento                | DocumentService     | Preview seguro       |
| 6    | Validar legibilidad          | Admin               | Manual               |
| 7    | Validar datos                | Admin               | Nombre, RNC, fechas  |
| 8    | Si válido                    | Aprobar             | Status = Verified    |
| 9    | Si inválido                  | Rechazar            | Con razón            |
| 10   | Actualizar dealer            | DealerService       | VerificationStatus   |
| 11   | Notificar dealer             | NotificationService | Resultado            |
| 12   | Log de auditoría             | AuditService        | Decisión             |

#### Criterios de Verificación

| Documento            | Criterios                                |
| -------------------- | ---------------------------------------- |
| RNC Certificate      | RNC coincide, vigente, nombre correcto   |
| Business License     | Vigente, actividad correcta              |
| ID del Representante | Legible, vigente, foto clara             |
| Proof of Address     | Reciente (< 3 meses), dirección correcta |

---

### 4.4 DOC-004: Generación de Contrato Digital

| Campo       | Valor                       |
| ----------- | --------------------------- |
| **ID**      | DOC-004                     |
| **Nombre**  | Digital Contract Generation |
| **Actor**   | Sistema                     |
| **Trigger** | Transacción de venta        |

#### Flujo del Proceso

| Paso | Acción                     | Sistema             | Validación              |
| ---- | -------------------------- | ------------------- | ----------------------- |
| 1    | Transacción iniciada       | BillingService      | Nuevo pago              |
| 2    | Obtener datos              | Múltiples servicios | Buyer, Seller, Vehicle  |
| 3    | Seleccionar template       | ContractService     | Por tipo de transacción |
| 4    | Merge data en template     | ContractService     | Variables               |
| 5    | Generar PDF                | PdfService          | Con sellos              |
| 6    | Agregar QR de verificación | PdfService          | Link único              |
| 7    | Cifrar PDF                 | DocumentService     | AES-256                 |
| 8    | Guardar en S3              | S3Client            | Con SSE                 |
| 9    | Crear registro             | Database            | Type = SalesContract    |
| 10   | Enviar a partes            | NotificationService | Email con link          |
| 11   | Solicitar firma digital    | SignatureService    | Si aplica               |

#### Variables del Contrato

```json
{
  "contractNumber": "OKLA-2026-00001",
  "date": "21 de Enero de 2026",
  "seller": {
    "name": "Juan Pérez",
    "cedula": "001-1234567-8",
    "address": "Av. Churchill #100"
  },
  "buyer": {
    "name": "María García",
    "cedula": "001-7654321-0",
    "address": "Calle El Sol #50"
  },
  "vehicle": {
    "make": "Toyota",
    "model": "RAV4 XLE",
    "year": 2024,
    "vin": "1ABCD23EFGH456789",
    "plate": "A123456",
    "color": "Blanco Perla"
  },
  "price": {
    "amount": 2500000,
    "currency": "DOP",
    "amountInWords": "DOS MILLONES QUINIENTOS MIL PESOS"
  }
}
```

---

### 4.5 DOC-005: Retención y Archivado

| Campo       | Valor                           |
| ----------- | ------------------------------- |
| **ID**      | DOC-005                         |
| **Nombre**  | Document Retention and Archival |
| **Actor**   | Sistema                         |
| **Trigger** | Cron diario                     |

#### Flujo del Proceso

| Paso | Acción                   | Sistema          | Validación             |
| ---- | ------------------------ | ---------------- | ---------------------- |
| 1    | Cron ejecuta job         | SchedulerService | 2:00 AM                |
| 2    | Buscar docs expirados    | Database         | ExpiresAt < today      |
| 3    | Para cada documento      | Loop             | Procesar               |
| 4    | Si retention cumplida    | Check            | CanBeDeleted = true    |
| 5    | Mover a Glacier          | S3Client         | Tier archivado         |
| 6    | Actualizar registro      | Database         | Status = Archived      |
| 7    | Si no retention          | Mantener         | En hot storage         |
| 8    | Buscar docs temporales   | Database         | Type = Temp, Age > 24h |
| 9    | Eliminar permanentemente | S3Client         | Delete                 |
| 10   | Log operaciones          | AuditService     | Registro               |
| 11   | Generar reporte          | ReportService    | Para compliance        |

#### Políticas de Retención

| Tipo de Documento | Retención Mínima                | Acción Post-Retención |
| ----------------- | ------------------------------- | --------------------- |
| Fiscales          | 10 años                         | Archivar              |
| Contratos         | 10 años                         | Archivar              |
| Identidad         | 5 años post-relación            | Eliminar              |
| Verificación      | Mientras dealer activo + 5 años | Archivar              |
| Temporales        | 24 horas                        | Eliminar              |

---

## 5. Estructura de Almacenamiento

### 5.1 Buckets y Paths

```
okla-documents/
├── identity/
│   └── {userId}/
│       └── {documentId}.enc
├── fiscal/
│   ├── invoices/
│   │   └── {year}/{month}/{invoiceId}.pdf.enc
│   └── reports/
│       └── {year}/{reportType}/{reportId}.pdf.enc
├── legal/
│   ├── contracts/
│   │   └── {year}/{contractId}.pdf.enc
│   └── consents/
│       └── {userId}/{consentId}.json.enc
├── verification/
│   └── dealers/
│       └── {dealerId}/
│           └── {documentType}/{documentId}.enc
├── vehicles/
│   └── {vehicleId}/
│       └── {documentType}/{documentId}.enc
└── temp/
    └── uploads/
        └── {uploadId}/
```

### 5.2 Encriptación

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    Encryption Architecture                               │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│   ┌────────────┐       ┌────────────────┐       ┌────────────────┐     │
│   │  Document  │──────▶│  Encrypt with  │──────▶│  Encrypted     │     │
│   │  (Plain)   │       │  AES-256-GCM   │       │  Document      │     │
│   └────────────┘       └───────┬────────┘       └────────────────┘     │
│                                │                                        │
│                                │ Data Key                               │
│                                ▼                                        │
│                        ┌────────────────┐                              │
│                        │   AWS KMS      │                              │
│                        │  Master Key    │                              │
│                        └────────────────┘                              │
│                                │                                        │
│                                │ Encrypted Data Key                    │
│                                ▼                                        │
│                        ┌────────────────┐                              │
│                        │   Stored in    │                              │
│                        │   S3 Metadata  │                              │
│                        └────────────────┘                              │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 6. Eventos RabbitMQ

| Evento                           | Exchange           | Payload                         |
| -------------------------------- | ------------------ | ------------------------------- |
| `document.uploaded`              | `documents.events` | `{ documentId, type, ownerId }` |
| `document.verification.required` | `documents.events` | `{ documentId, dealerId }`      |
| `document.verified`              | `documents.events` | `{ documentId, verifiedBy }`    |
| `document.rejected`              | `documents.events` | `{ documentId, reason }`        |
| `document.accessed`              | `documents.events` | `{ documentId, accessedBy }`    |
| `document.deleted`               | `documents.events` | `{ documentId, permanently }`   |
| `document.expired`               | `documents.events` | `{ documentId, expiresAt }`     |

---

## 7. Métricas

```
# Uploads
documents_uploaded_total{type="...", category="..."}
documents_upload_size_bytes_total
documents_upload_duration_seconds

# Storage
documents_stored_total{status="..."}
documents_storage_bytes_total{tier="hot|glacier"}

# Verification
documents_pending_verification_total
documents_verification_duration_hours
documents_approved_total
documents_rejected_total{reason="..."}

# Access
documents_accessed_total{action="view|download"}
documents_access_denied_total{reason="..."}

# Retention
documents_archived_total
documents_deleted_total
```

---

## 8. Configuración

```json
{
  "DocumentStorage": {
    "MaxUploadSize": "5MB",
    "AllowedTypes": ["application/pdf", "image/jpeg", "image/png"],
    "TempUploadTTL": "24h",
    "VirusScanEnabled": true
  },
  "Encryption": {
    "Provider": "AWS KMS",
    "MasterKeyId": "arn:aws:kms:us-east-1:123456789:key/abcd-1234",
    "Algorithm": "AES-256-GCM"
  },
  "Retention": {
    "FiscalDocuments": "10y",
    "LegalDocuments": "10y",
    "IdentityDocuments": "5y",
    "TemporaryUploads": "24h"
  },
  "Storage": {
    "Provider": "DigitalOceanSpaces",
    "Bucket": "okla-documents",
    "Region": "nyc3",
    "GlacierEnabled": true,
    "GlacierTransitionDays": 90
  }
}
```

---

## 9. Códigos de Error

| Código    | Mensaje               | Causa             |
| --------- | --------------------- | ----------------- |
| `DOC_001` | Document not found    | No existe         |
| `DOC_002` | Access denied         | Sin permisos      |
| `DOC_003` | Invalid document type | Tipo no permitido |
| `DOC_004` | Document too large    | > 5 MB            |
| `DOC_005` | Virus detected        | Archivo infectado |
| `DOC_006` | Encryption failed     | Error de cifrado  |
| `DOC_007` | Already verified      | Ya fue verificado |
| `DOC_008` | Cannot delete         | Retención activa  |
| `DOC_009` | Document expired      | Expiró            |
| `DOC_010` | Checksum mismatch     | Archivo corrupto  |

---

## 📚 Referencias

- [01-media-service.md](01-media-service.md) - Servicio de media
- [03-dgii-integration.md](../08-COMPLIANCE-LEGAL-RD/03-dgii-integration.md) - Documentos fiscales
- [02-ley-172-13.md](../08-COMPLIANCE-LEGAL-RD/02-ley-172-13.md) - Protección de datos
- [01-ley-155-17.md](../08-COMPLIANCE-LEGAL-RD/01-ley-155-17.md) - AML/DDC
