# ✅ OKLA Certified Pre-Owned

> **Código:** CERT-001  
> **Versión:** 1.0  
> **Última actualización:** Enero 21, 2026  
> **Criticidad:** 🔴 ALTA (Diferenciación premium)  
> **Origen:** AutoTrader, Cars.com CPO Programs

---

## � Resumen de Implementación

| Componente    | Total | Implementado | Pendiente | Estado |
| ------------- | ----- | ------------ | --------- | ------ |
| Controllers   | 2     | 0            | 2         | 🔴     |
| CERT-APPLY-\* | 4     | 0            | 4         | 🔴     |
| CERT-INSP-\*  | 5     | 0            | 5         | 🔴     |
| CERT-BADGE-\* | 3     | 0            | 3         | 🔴     |
| CERT-RENEW-\* | 3     | 0            | 3         | 🔴     |
| Tests         | 0     | 0            | 12        | 🔴     |

**Leyenda:** ✅ Implementado + Tested | 🟢 Implementado | 🟡 En Progreso | 🔴 Pendiente

---

## �📋 Información General

| Campo             | Valor                                                                            |
| ----------------- | -------------------------------------------------------------------------------- |
| **Servicio**      | CertificationService (NUEVO) o TrustService (extender)                           |
| **Puerto**        | 5092 (nuevo) o 5082 (existente)                                                  |
| **Base de Datos** | `certificationservice`                                                           |
| **Dependencias**  | VehiclesSaleService, InspectionService, WarrantyService, DealerManagementService |

---

## 🎯 Objetivo del Proceso

1. **Confianza:** Badge visible = vehículo confiable
2. **Diferenciación:** Solo vehículos que cumplen criterios estrictos
3. **Premium:** Justificar precio más alto con garantía
4. **Conversión:** Compradores prefieren Certified

---

## 🏆 Criterios de Certificación OKLA

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    OKLA CERTIFIED PRE-OWNED                             │
│                    Criterios de Elegibilidad                            │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  ✅ REQUISITOS DEL VEHÍCULO                                            │
│  ─────────────────────────────                                         │
│  • Año: 2019 o más nuevo (máximo 7 años)                               │
│  • Kilometraje: Menos de 100,000 km                                    │
│  • Título limpio: Sin salvage, rebuilt o flood                         │
│  • Sin accidentes estructurales reportados                             │
│  • Historial de mantenimiento verificable                              │
│                                                                         │
│  ✅ REQUISITOS DE INSPECCIÓN                                           │
│  ─────────────────────────────────                                     │
│  • Inspección de 150+ puntos completada                                │
│  • Realizada por mecánico certificado                                  │
│  • Fecha de inspección: últimos 30 días                                │
│  • Score mínimo: 85/100                                                │
│  • Sin fallas críticas pendientes                                      │
│                                                                         │
│  ✅ REQUISITOS DEL DEALER                                              │
│  ─────────────────────────────                                         │
│  • Dealer verificado en OKLA                                           │
│  • Rating mínimo: 4.0 estrellas                                        │
│  • Miembro del programa OKLA Certified                                 │
│  • Sin violaciones de políticas activas                                │
│                                                                         │
│  ✅ BENEFICIOS INCLUIDOS                                               │
│  ────────────────────────                                              │
│  • Garantía OKLA: 6 meses o 10,000 km                                  │
│  • Reporte de historial completo                                       │
│  • 7 días de devolución                                                │
│  • Asistencia en carretera 24/7 (3 meses)                              │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 📡 Endpoints

| Método | Endpoint                                     | Descripción                 | Auth      |
| ------ | -------------------------------------------- | --------------------------- | --------- |
| `GET`  | `/api/certification/eligibility/{vehicleId}` | Verificar elegibilidad      | ✅ Dealer |
| `POST` | `/api/certification/apply/{vehicleId}`       | Solicitar certificación     | ✅ Dealer |
| `GET`  | `/api/certification/{vehicleId}`             | Ver estado de certificación | ❌        |
| `GET`  | `/api/certification/vehicles`                | Listar vehículos Certified  | ❌        |
| `POST` | `/api/certification/{id}/inspection`         | Subir inspección            | ✅ Dealer |
| `POST` | `/api/certification/{id}/approve`            | Aprobar certificación       | ✅ Admin  |
| `POST` | `/api/certification/{id}/revoke`             | Revocar certificación       | ✅ Admin  |
| `GET`  | `/api/certification/dealers/enrolled`        | Dealers en el programa      | ❌        |

---

## 🗃️ Entidades

### VehicleCertification

```csharp
public class VehicleCertification
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public Guid DealerId { get; set; }

    // Estado
    public CertificationStatus Status { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public Guid? ApprovedBy { get; set; }

    // Elegibilidad
    public EligibilityCheck Eligibility { get; set; }

    // Inspección
    public CertificationInspection Inspection { get; set; }

    // Garantía incluida
    public Guid? WarrantyId { get; set; }
    public WarrantyCoverage IncludedWarranty { get; set; }

    // Beneficios
    public bool ReturnPolicyEnabled { get; set; }
    public int ReturnDays { get; set; }
    public bool RoadsideAssistanceEnabled { get; set; }
    public int RoadsideAssistanceMonths { get; set; }

    // Historial
    public string VehicleHistoryReportUrl { get; set; }

    // Revocación
    public bool IsRevoked { get; set; }
    public string RevocationReason { get; set; }
    public DateTime? RevokedAt { get; set; }

    public DateTime CreatedAt { get; set; }
}

public enum CertificationStatus
{
    Pending,             // Solicitud en revisión
    EligibilityFailed,   // No cumple criterios
    AwaitingInspection,  // Esperando inspección
    InspectionFailed,    // Inspección no pasó
    Approved,            // ✅ Certificado
    Expired,             // Expiró
    Revoked              // Revocado
}

public class EligibilityCheck
{
    public bool VehicleAgeOk { get; set; }
    public bool MileageOk { get; set; }
    public bool TitleClean { get; set; }
    public bool NoStructuralDamage { get; set; }
    public bool ServiceHistoryOk { get; set; }
    public bool DealerVerified { get; set; }
    public bool DealerRatingOk { get; set; }
    public bool DealerEnrolled { get; set; }

    public bool AllPassed => VehicleAgeOk && MileageOk && TitleClean &&
                             NoStructuralDamage && ServiceHistoryOk &&
                             DealerVerified && DealerRatingOk && DealerEnrolled;

    public List<string> FailedCriteria { get; set; }
    public DateTime CheckedAt { get; set; }
}

public class CertificationInspection
{
    public Guid Id { get; set; }
    public Guid CertificationId { get; set; }

    // Mecánico
    public string MechanicName { get; set; }
    public string MechanicCertification { get; set; }
    public string ShopName { get; set; }

    // Resultados
    public int TotalPoints { get; set; }          // 150+
    public int PointsPassed { get; set; }
    public int Score { get; set; }                // 0-100
    public bool Passed { get; set; }              // Score >= 85

    // Categorías
    public List<InspectionCategory> Categories { get; set; }

    // Documentos
    public string InspectionReportUrl { get; set; }
    public List<string> PhotoUrls { get; set; }

    // Issues
    public List<InspectionIssue> Issues { get; set; }
    public bool HasCriticalIssues { get; set; }

    public DateTime InspectedAt { get; set; }
    public DateTime UploadedAt { get; set; }
}

public class InspectionCategory
{
    public string Name { get; set; }  // Motor, Transmisión, Frenos, etc.
    public int Points { get; set; }
    public int PointsPassed { get; set; }
    public int Score { get; set; }
    public List<InspectionItem> Items { get; set; }
}

public class InspectionIssue
{
    public string Category { get; set; }
    public string Description { get; set; }
    public IssueSeverity Severity { get; set; }
    public bool Resolved { get; set; }
}

public enum IssueSeverity
{
    Minor,      // Cosmético, no afecta
    Moderate,   // Debe atenderse
    Critical    // Falla inmediata de certificación
}
```

### DealerCertificationEnrollment

```csharp
public class DealerCertificationEnrollment
{
    public Guid Id { get; set; }
    public Guid DealerId { get; set; }

    // Estado
    public bool IsEnrolled { get; set; }
    public DateTime EnrolledAt { get; set; }

    // Requisitos
    public bool DealerVerified { get; set; }
    public decimal DealerRating { get; set; }
    public bool TrainingCompleted { get; set; }
    public bool AgreementSigned { get; set; }

    // Estadísticas
    public int TotalCertifiedVehicles { get; set; }
    public int ActiveCertifiedVehicles { get; set; }
    public decimal SuccessRate { get; set; }

    // Fees
    public decimal FeePerCertification { get; set; }  // RD$5,000 por vehículo

    public DateTime LastCertificationAt { get; set; }
}
```

---

## 📊 Proceso CERT-001: Certificar Vehículo

```
┌─────────────────────────────────────────────────────────────────────────┐
│ PROCESO: CERT-001 - Certificar Vehículo como OKLA Certified            │
├─────────────────────────────────────────────────────────────────────────┤
│ Actor Iniciador: USR-DEALER                                           │
│ Sistemas: CertificationService, InspectionService, WarrantyService    │
│ Duración: 1-3 días (incluye inspección)                                │
│ Criticidad: ALTA                                                        │
└─────────────────────────────────────────────────────────────────────────┘
```

| Paso | Subpaso | Acción                                             | Sistema              | Actor      | Evidencia              | Código     |
| ---- | ------- | -------------------------------------------------- | -------------------- | ---------- | ---------------------- | ---------- |
| 1    | 1.1     | Dealer selecciona vehículo                         | Dashboard            | USR-DEALER | Vehicle selected       | EVD-LOG    |
| 1    | 1.2     | Click "Certificar como OKLA Certified"             | Dashboard            | USR-DEALER | CTA clicked            | EVD-LOG    |
| 2    | 2.1     | **GET /api/certification/eligibility/{vehicleId}** | Gateway              | USR-DEALER | **Request**            | EVD-LOG    |
| 2    | 2.2     | **Verificar elegibilidad del vehículo**            | CertificationService | Sistema    | **Eligibility check**  | EVD-AUDIT  |
| 2    | 2.3     | Año >= 2019?                                       | Sistema              | Sistema    | Year check             | EVD-LOG    |
| 2    | 2.4     | Km < 100,000?                                      | Sistema              | Sistema    | Mileage check          | EVD-LOG    |
| 2    | 2.5     | Título limpio?                                     | Sistema              | Sistema    | Title check            | EVD-LOG    |
| 2    | 2.6     | Sin daño estructural?                              | Sistema              | Sistema    | Damage check           | EVD-LOG    |
| 2    | 2.7     | **Verificar elegibilidad del dealer**              | CertificationService | Sistema    | **Dealer check**       | EVD-LOG    |
| 2    | 2.8     | Dealer verificado? Rating >= 4.0?                  | Sistema              | Sistema    | Dealer validation      | EVD-LOG    |
| 3    | 3.1     | Si no elegible: Mostrar razones                    | Frontend             | Sistema    | Reasons shown          | EVD-SCREEN |
| 3    | 3.2     | Si elegible: Continuar                             | Frontend             | Sistema    | Continue               | EVD-LOG    |
| 4    | 4.1     | **POST /api/certification/apply/{vehicleId}**      | Gateway              | USR-DEALER | **Application**        | EVD-AUDIT  |
| 4    | 4.2     | **Crear VehicleCertification**                     | CertificationService | Sistema    | **Cert created**       | EVD-AUDIT  |
| 4    | 4.3     | Status = AwaitingInspection                        | CertificationService | Sistema    | Status set             | EVD-LOG    |
| 5    | 5.1     | Dealer realiza inspección de 150 puntos            | Físico               | USR-DEALER | Inspection done        | EVD-LOG    |
| 5    | 5.2     | Con mecánico certificado                           | Físico               | Mecánico   | Mechanic involved      | EVD-LOG    |
| 6    | 6.1     | **POST /api/certification/{id}/inspection**        | Gateway              | USR-DEALER | **Inspection upload**  | EVD-AUDIT  |
| 6    | 6.2     | Subir reporte + fotos                              | CertificationService | USR-DEALER | Files uploaded         | EVD-FILE   |
| 6    | 6.3     | **Calcular score**                                 | CertificationService | Sistema    | **Score calculated**   | EVD-LOG    |
| 7    | 7.1     | Si score < 85: Status = InspectionFailed           | CertificationService | Sistema    | Failed                 | EVD-LOG    |
| 7    | 7.2     | Si issues críticos: Status = InspectionFailed      | CertificationService | Sistema    | Critical fail          | EVD-LOG    |
| 7    | 7.3     | Si score >= 85 y sin críticos: Continuar           | CertificationService | Sistema    | Passed                 | EVD-LOG    |
| 8    | 8.1     | Notificar a Admin para revisión                    | NotificationService  | SYS-NOTIF  | Admin notified         | EVD-COMM   |
| 8    | 8.2     | Admin revisa solicitud                             | Admin Panel          | USR-ADMIN  | Review                 | EVD-LOG    |
| 9    | 9.1     | **POST /api/certification/{id}/approve**           | Gateway              | USR-ADMIN  | **Approval**           | EVD-AUDIT  |
| 9    | 9.2     | **Status = Approved**                              | CertificationService | Sistema    | **Approved**           | EVD-AUDIT  |
| 9    | 9.3     | Fecha de expiración = +12 meses                    | CertificationService | Sistema    | Expiry set             | EVD-LOG    |
| 10   | 10.1    | **Crear garantía incluida**                        | WarrantyService      | Sistema    | **Warranty created**   | EVD-AUDIT  |
| 10   | 10.2    | 6 meses o 10,000 km                                | WarrantyService      | Sistema    | Coverage set           | EVD-LOG    |
| 11   | 11.1    | **Activar beneficios**                             | CertificationService | Sistema    | **Benefits activated** | EVD-LOG    |
| 11   | 11.2    | Return policy 7 días                               | CertificationService | Sistema    | Return enabled         | EVD-LOG    |
| 11   | 11.3    | Roadside assistance 3 meses                        | CertificationService | Sistema    | Roadside enabled       | EVD-LOG    |
| 12   | 12.1    | **Agregar badge al listing**                       | VehiclesSaleService  | Sistema    | **Badge added**        | EVD-LOG    |
| 12   | 12.2    | Incluir en filtro "OKLA Certified"                 | SearchService        | Sistema    | Filter updated         | EVD-LOG    |
| 13   | 13.1    | Cobrar fee al dealer                               | BillingService       | Sistema    | Fee charged            | EVD-AUDIT  |
| 13   | 13.2    | RD$5,000 por certificación                         | BillingService       | Sistema    | Amount                 | EVD-LOG    |
| 14   | 14.1    | **Notificar al dealer**                            | NotificationService  | SYS-NOTIF  | **Dealer notified**    | EVD-COMM   |
| 15   | 15.1    | **Audit trail**                                    | AuditService         | Sistema    | Complete audit         | EVD-AUDIT  |

### Evidencia de Certificación

```json
{
  "processCode": "CERT-001",
  "certification": {
    "id": "cert-12345",
    "vehicle": {
      "id": "veh-67890",
      "title": "Toyota Corolla 2022 - OKLA Certified",
      "make": "Toyota",
      "model": "Corolla",
      "year": 2022,
      "mileage": 35000
    },
    "dealer": {
      "id": "dealer-001",
      "name": "AutoMax RD",
      "rating": 4.3,
      "verified": true
    },
    "eligibility": {
      "vehicleAgeOk": true,
      "mileageOk": true,
      "titleClean": true,
      "noStructuralDamage": true,
      "serviceHistoryOk": true,
      "dealerVerified": true,
      "dealerRatingOk": true,
      "dealerEnrolled": true,
      "allPassed": true,
      "checkedAt": "2026-01-18T10:00:00Z"
    },
    "inspection": {
      "mechanicName": "José Martínez",
      "shopName": "Taller Autorizado Toyota",
      "totalPoints": 156,
      "pointsPassed": 148,
      "score": 95,
      "passed": true,
      "categories": [
        { "name": "Motor", "score": 98 },
        { "name": "Transmisión", "score": 95 },
        { "name": "Frenos", "score": 100 },
        { "name": "Suspensión", "score": 92 },
        { "name": "Eléctrico", "score": 94 },
        { "name": "Exterior", "score": 90 },
        { "name": "Interior", "score": 96 }
      ],
      "issues": [
        {
          "category": "Exterior",
          "description": "Rayón menor en parachoques trasero",
          "severity": "Minor",
          "resolved": true
        }
      ],
      "hasCriticalIssues": false,
      "inspectedAt": "2026-01-19T14:00:00Z"
    },
    "status": "Approved",
    "approvedAt": "2026-01-21T10:00:00Z",
    "expiresAt": "2027-01-21T10:00:00Z",
    "benefits": {
      "warranty": {
        "id": "warranty-12345",
        "duration": "6 meses",
        "coverage": "10,000 km",
        "type": "OKLA Certified Warranty"
      },
      "returnPolicy": {
        "enabled": true,
        "days": 7
      },
      "roadsideAssistance": {
        "enabled": true,
        "months": 3,
        "provider": "Asistencia Vial RD"
      }
    },
    "fee": {
      "amount": 5000,
      "currency": "DOP",
      "charged": true,
      "transactionId": "tx-98765"
    }
  }
}
```

---

## 📱 UI Mockup - Badge en Listing

```
┌─────────────────────────────────────────────────────────────────────────┐
│ ┌────────────────────────────────────────────────────────────────────┐ │
│ │                                                        ┌─────────┐ │ │
│ │  [IMAGEN DEL VEHÍCULO]                                │ ✅ OKLA │ │ │
│ │                                                        │CERTIFIED│ │ │
│ │                                                        └─────────┘ │ │
│ └────────────────────────────────────────────────────────────────────┘ │
│                                                                         │
│  ✅ OKLA Certified Pre-Owned                                           │
│  Toyota Corolla 2022                                                   │
│  35,000 km · Santo Domingo                                             │
│                                                                         │
│  RD$ 1,350,000                                                         │
│                                                                         │
│  Incluye:                                                              │
│  ✓ Garantía 6 meses   ✓ 7 días devolución   ✓ Inspección 150 pts     │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 📱 UI Mockup - Sección OKLA Certified en Detail

```
┌─────────────────────────────────────────────────────────────────────────┐
│                                                                         │
│  ✅ OKLA CERTIFIED PRE-OWNED                                           │
│  ════════════════════════════════════════════════════════════════════  │
│                                                                         │
│  Este vehículo ha sido certificado y cumple con nuestros              │
│  estándares más altos de calidad.                                      │
│                                                                         │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │                                                                 │   │
│  │  📋 INSPECCIÓN DE 156 PUNTOS                                   │   │
│  │                                                                 │   │
│  │  Score: 95/100 ⭐⭐⭐⭐⭐                                       │   │
│  │                                                                 │   │
│  │  Motor ████████████████████ 98%                                │   │
│  │  Transmisión ███████████████████ 95%                           │   │
│  │  Frenos ████████████████████ 100%                              │   │
│  │  Suspensión ██████████████████ 92%                             │   │
│  │  Eléctrico ███████████████████ 94%                             │   │
│  │  Exterior ██████████████████ 90%                               │   │
│  │  Interior ███████████████████ 96%                              │   │
│  │                                                                 │   │
│  │  Inspeccionado por: José Martínez                              │   │
│  │  Taller: Taller Autorizado Toyota                              │   │
│  │  Fecha: 19 de Enero, 2026                                      │   │
│  │                                                                 │   │
│  │  [Ver Reporte Completo]                                        │   │
│  │                                                                 │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│                                                                         │
│  🛡️ BENEFICIOS INCLUIDOS                                              │
│                                                                         │
│  ┌───────────────┐  ┌───────────────┐  ┌───────────────┐              │
│  │  ✓ Garantía   │  │  ✓ Devolución │  │  ✓ Asistencia │              │
│  │  6 meses o    │  │  7 días si    │  │  vial 24/7    │              │
│  │  10,000 km    │  │  no te gusta  │  │  por 3 meses  │              │
│  └───────────────┘  └───────────────┘  └───────────────┘              │
│                                                                         │
│  📄 Historial del vehículo verificado                                  │
│  [Ver Historial Completo]                                              │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 📊 Métricas Prometheus

```yaml
# Certificaciones
certifications_applied_total
certifications_approved_total
certifications_failed_total{reason}
certifications_revoked_total

# Inspecciones
inspection_score_avg
inspection_pass_rate
inspection_time_days_avg

# Negocio
certified_vehicles_active
certified_premium_price_percent
certified_to_sale_rate
certified_vs_regular_conversion

# Dealers
dealers_enrolled_total
certifications_per_dealer_avg
```

---

## 🔗 Referencias

- [00-ANALISIS-COMPETITIVO.md](../00-ANALISIS-COMPETITIVO.md)
- [15-CONFIANZA-SEGURIDAD/02-garantia-inspeccion.md](02-garantia-inspeccion.md)
- [02-USUARIOS-DEALERS/01-dealer-management.md](../02-USUARIOS-DEALERS/01-dealer-management.md)
