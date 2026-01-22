# 🚗 Test Drive Scheduling (Complemento)

> **Código:** TESTDRIVE-001, TESTDRIVE-002  
> **Versión:** 1.0  
> **Última actualización:** Enero 21, 2026  
> **Criticidad:** 🔴 CRÍTICA (Conversión)

---

## 📋 Información General

| Campo              | Valor                                                                  |
| ------------------ | ---------------------------------------------------------------------- |
| **Servicio**       | AppointmentService (extendido)                                         |
| **Puerto**         | 5009                                                                   |
| **Base de Datos**  | `appointmentservice`                                                   |
| **Dependencias**   | VehiclesSaleService, UserService, NotificationService, CalendarService |
| **Documento Base** | [05-AGENDAMIENTO/01-appointment-service.md](01-appointment-service.md) |

---

## 🎯 Objetivo del Proceso

Este documento complementa `01-appointment-service.md` con el flujo específico de **Test Drive**, incluyendo:

1. **Agenda del dealer:** Disponibilidad configurable
2. **Preparación del vehículo:** Checklist pre-test drive
3. **Documentación legal:** Formulario de responsabilidad
4. **Seguimiento post-test:** Contacto automatizado

---

## 📡 Endpoints Adicionales

| Método | Endpoint                                   | Descripción                           | Auth |
| ------ | ------------------------------------------ | ------------------------------------- | ---- |
| `GET`  | `/api/testdrives/availability/{vehicleId}` | Slots disponibles                     | ❌   |
| `POST` | `/api/testdrives`                          | Agendar test drive                    | ✅   |
| `GET`  | `/api/testdrives/{id}/waiver`              | Obtener formulario de responsabilidad | ✅   |
| `POST` | `/api/testdrives/{id}/waiver/sign`         | Firmar formulario                     | ✅   |
| `POST` | `/api/testdrives/{id}/checkin`             | Check-in al llegar                    | ✅   |
| `POST` | `/api/testdrives/{id}/checkout`            | Check-out al terminar                 | ✅   |
| `POST` | `/api/testdrives/{id}/feedback`            | Feedback post-test                    | ✅   |

---

## 🗃️ Entidades

### TestDrive (extiende Appointment)

```csharp
public class TestDrive : Appointment
{
    // Vehículo
    public Guid VehicleId { get; set; }
    public string VehicleTitle { get; set; }
    public string VehicleImage { get; set; }
    public string VehicleVIN { get; set; }

    // Conductor
    public string DriverName { get; set; }
    public string DriverCedula { get; set; }
    public string DriverLicenseNumber { get; set; }
    public DateTime DriverLicenseExpiry { get; set; }
    public string DriverLicensePhoto { get; set; }

    // Legal
    public bool WaiverSigned { get; set; }
    public DateTime? WaiverSignedAt { get; set; }
    public string WaiverSignatureUrl { get; set; }
    public string WaiverDocumentUrl { get; set; }

    // Check-in/out
    public bool CheckedIn { get; set; }
    public DateTime? CheckInTime { get; set; }
    public int OdometerAtCheckIn { get; set; }
    public List<string> PreTestPhotos { get; set; }

    public bool CheckedOut { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public int OdometerAtCheckOut { get; set; }
    public int KmDriven { get; set; }
    public List<string> PostTestPhotos { get; set; }

    // Feedback
    public TestDriveFeedback Feedback { get; set; }

    // Resultado
    public TestDriveOutcome? Outcome { get; set; }
}

public class TestDriveFeedback
{
    public Guid Id { get; set; }
    public Guid TestDriveId { get; set; }

    // Ratings (1-5)
    public int OverallRating { get; set; }
    public int ComfortRating { get; set; }
    public int PerformanceRating { get; set; }
    public int ConditionRating { get; set; }

    // Preguntas
    public bool MeetsExpectations { get; set; }
    public bool WouldRecommend { get; set; }
    public bool InterestedInBuying { get; set; }

    // Comentarios
    public string Likes { get; set; }
    public string Dislikes { get; set; }
    public string AdditionalComments { get; set; }

    public DateTime SubmittedAt { get; set; }
}

public enum TestDriveOutcome
{
    NoDecision,          // Aún pensando
    InterestedWillReturn,// Interesado, volverá
    MadeOffer,           // Hizo oferta
    Purchased,           // Compró!
    NotInterested,       // No interesado
    Cancelled            // No se presentó
}
```

### DealerTestDriveSettings

```csharp
public class DealerTestDriveSettings
{
    public Guid Id { get; set; }
    public Guid DealerId { get; set; }

    // Horarios disponibles
    public List<AvailabilitySlot> WeeklySchedule { get; set; }

    // Configuración
    public int MaxTestDrivesPerDay { get; set; }
    public int TestDriveDurationMinutes { get; set; }    // Default: 30
    public int BufferBetweenMinutes { get; set; }        // Default: 15
    public int MaxAdvanceBookingDays { get; set; }       // Default: 14
    public int MinAdvanceBookingHours { get; set; }      // Default: 2

    // Requisitos
    public bool RequireLicensePhoto { get; set; }
    public bool RequireWaiverSignature { get; set; }
    public bool RequireDeposit { get; set; }
    public decimal? DepositAmount { get; set; }

    // Rutas
    public List<TestDriveRoute> ApprovedRoutes { get; set; }

    // Personal
    public bool SalesRepAccompanies { get; set; }
    public List<Guid> AssignedSalesReps { get; set; }
}

public class TestDriveRoute
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal DistanceKm { get; set; }
    public int EstimatedMinutes { get; set; }
    public string MapUrl { get; set; }
    public bool IncludesHighway { get; set; }
}
```

---

## 📊 Proceso TESTDRIVE-001: Agendar Test Drive

```
┌─────────────────────────────────────────────────────────────────────────┐
│ PROCESO: TESTDRIVE-001 - Agendar Test Drive                            │
├─────────────────────────────────────────────────────────────────────────┤
│ Actor Iniciador: USR-REG                                               │
│ Sistemas: AppointmentService, VehiclesSaleService, NotificationService │
│ Duración: 2-5 minutos                                                  │
│ Criticidad: ALTA                                                        │
└─────────────────────────────────────────────────────────────────────────┘
```

| Paso | Subpaso | Acción                                       | Sistema             | Actor     | Evidencia             | Código     |
| ---- | ------- | -------------------------------------------- | ------------------- | --------- | --------------------- | ---------- |
| 1    | 1.1     | Usuario ve listing de vehículo               | Frontend            | USR-REG   | Listing viewed        | EVD-LOG    |
| 1    | 1.2     | Click "Agendar Test Drive"                   | Frontend            | USR-REG   | CTA clicked           | EVD-LOG    |
| 2    | 2.1     | GET /api/testdrives/availability/{vehicleId} | Gateway             | USR-REG   | Request               | EVD-LOG    |
| 2    | 2.2     | Obtener configuración del dealer             | AppointmentService  | Sistema   | Config fetched        | EVD-LOG    |
| 2    | 2.3     | Calcular slots disponibles                   | AppointmentService  | Sistema   | Slots calculated      | EVD-LOG    |
| 3    | 3.1     | Mostrar calendario con disponibilidad        | Frontend            | USR-REG   | Calendar shown        | EVD-SCREEN |
| 3    | 3.2     | Seleccionar fecha                            | Frontend            | USR-REG   | Date selected         | EVD-LOG    |
| 3    | 3.3     | Seleccionar hora                             | Frontend            | USR-REG   | Time selected         | EVD-LOG    |
| 4    | 4.1     | Formulario de datos del conductor            | Frontend            | USR-REG   | Form shown            | EVD-SCREEN |
| 4    | 4.2     | Ingresar número de licencia                  | Frontend            | USR-REG   | License input         | EVD-LOG    |
| 4    | 4.3     | **Subir foto de licencia**                   | MediaService        | USR-REG   | **License uploaded**  | EVD-FILE   |
| 5    | 5.1     | Si requiere depósito: checkout               | BillingService      | USR-REG   | Deposit payment       | EVD-AUDIT  |
| 6    | 6.1     | POST /api/testdrives                         | Gateway             | USR-REG   | **Request**           | EVD-AUDIT  |
| 6    | 6.2     | Validar datos                                | AppointmentService  | Sistema   | Validation            | EVD-LOG    |
| 6    | 6.3     | Verificar slot aún disponible                | AppointmentService  | Sistema   | Slot check            | EVD-LOG    |
| 6    | 6.4     | **Crear TestDrive**                          | AppointmentService  | Sistema   | **TestDrive created** | EVD-AUDIT  |
| 7    | 7.1     | **Generar formulario de responsabilidad**    | AppointmentService  | Sistema   | **Waiver generated**  | EVD-DOC    |
| 8    | 8.1     | **Notificar al dealer**                      | NotificationService | SYS-NOTIF | **Dealer notified**   | EVD-COMM   |
| 8    | 8.2     | **Confirmar al usuario**                     | NotificationService | SYS-NOTIF | **Confirmation sent** | EVD-COMM   |
| 8    | 8.3     | Agregar a calendario (ICS)                   | AppointmentService  | Sistema   | Calendar invite       | EVD-DOC    |
| 9    | 9.1     | **Recordatorio 24h antes**                   | Scheduler           | SYS-NOTIF | **Reminder sent**     | EVD-COMM   |
| 9    | 9.2     | **Recordatorio 2h antes**                    | Scheduler           | SYS-NOTIF | **Reminder sent**     | EVD-COMM   |
| 10   | 10.1    | **Audit trail**                              | AuditService        | Sistema   | Complete audit        | EVD-AUDIT  |

---

## 📊 Proceso TESTDRIVE-002: Ejecutar Test Drive

```
┌─────────────────────────────────────────────────────────────────────────┐
│ PROCESO: TESTDRIVE-002 - Ejecutar Test Drive (en el dealer)            │
├─────────────────────────────────────────────────────────────────────────┤
│ Actor Iniciador: USR-REG + USR-DEALER                                  │
│ Sistemas: AppointmentService, NotificationService                      │
│ Duración: 30-60 minutos                                                │
│ Criticidad: ALTA                                                        │
└─────────────────────────────────────────────────────────────────────────┘
```

| Paso | Subpaso | Acción                                   | Sistema        | Actor      | Evidencia              | Código    |
| ---- | ------- | ---------------------------------------- | -------------- | ---------- | ---------------------- | --------- |
| 1    | 1.1     | Usuario llega al dealer                  | Físico         | USR-REG    | Arrival                | EVD-LOG   |
| 1    | 1.2     | Dealer busca cita en sistema             | Dashboard      | USR-DEALER | Appointment found      | EVD-LOG   |
| 2    | 2.1     | Verificar licencia de conducir           | Dashboard      | USR-DEALER | License verified       | EVD-AUDIT |
| 2    | 2.2     | Comparar con foto subida                 | Dashboard      | USR-DEALER | Photo compared         | EVD-LOG   |
| 3    | 3.1     | **Firmar formulario de responsabilidad** | Tablet/Kiosk   | USR-REG    | **Waiver signed**      | EVD-AUDIT |
| 3    | 3.2     | Capturar firma digital                   | Frontend       | USR-REG    | Signature captured     | EVD-FILE  |
| 3    | 3.3     | POST /api/testdrives/{id}/waiver/sign    | Gateway        | USR-REG    | **Request**            | EVD-AUDIT |
| 4    | 4.1     | **Check-in**                             | Dashboard      | USR-DEALER | **Check-in**           | EVD-AUDIT |
| 4    | 4.2     | Registrar odómetro inicial               | Dashboard      | USR-DEALER | Odometer logged        | EVD-LOG   |
| 4    | 4.3     | **Tomar fotos pre-test**                 | Mobile         | USR-DEALER | **Photos taken**       | EVD-FILE  |
| 4    | 4.4     | POST /api/testdrives/{id}/checkin        | Gateway        | USR-DEALER | **Request**            | EVD-AUDIT |
| 5    | 5.1     | **PRUEBA DE MANEJO**                     | Físico         | USR-REG    | **Test drive**         | EVD-AUDIT |
| 5    | 5.2     | Vendedor acompaña (si aplica)            | Físico         | USR-DEALER | Accompanied            | EVD-LOG   |
| 5    | 5.3     | Ruta aprobada                            | Físico         | Sistema    | Route followed         | EVD-LOG   |
| 6    | 6.1     | Regresar al dealer                       | Físico         | USR-REG    | Return                 | EVD-LOG   |
| 6    | 6.2     | **Check-out**                            | Dashboard      | USR-DEALER | **Check-out**          | EVD-AUDIT |
| 6    | 6.3     | Registrar odómetro final                 | Dashboard      | USR-DEALER | Odometer logged        | EVD-LOG   |
| 6    | 6.4     | **Tomar fotos post-test**                | Mobile         | USR-DEALER | **Photos taken**       | EVD-FILE  |
| 6    | 6.5     | POST /api/testdrives/{id}/checkout       | Gateway        | USR-DEALER | **Request**            | EVD-AUDIT |
| 7    | 7.1     | Si depósito: reembolsar                  | BillingService | Sistema    | Deposit refunded       | EVD-AUDIT |
| 8    | 8.1     | **Solicitar feedback (1h después)**      | Scheduler      | SYS-NOTIF  | **Feedback request**   | EVD-COMM  |
| 8    | 8.2     | POST /api/testdrives/{id}/feedback       | Gateway        | USR-REG    | **Feedback submitted** | EVD-AUDIT |
| 9    | 9.1     | Registrar outcome                        | Dashboard      | USR-DEALER | Outcome recorded       | EVD-LOG   |
| 10   | 10.1    | Si interesado: seguimiento automático    | CRM            | Sistema    | Follow-up scheduled    | EVD-LOG   |
| 11   | 11.1    | **Audit trail completo**                 | AuditService   | Sistema    | Complete audit         | EVD-AUDIT |

### Evidencia de Test Drive Completado

```json
{
  "processCode": "TESTDRIVE-002",
  "testDrive": {
    "id": "td-12345",
    "vehicle": {
      "id": "veh-67890",
      "title": "Toyota Corolla 2023",
      "vin": "1HGBH41JXMN109186"
    },
    "driver": {
      "id": "user-001",
      "name": "Juan Pérez",
      "cedula": "001-*****-8",
      "licenseNumber": "RD-*****-12",
      "licenseVerified": true
    },
    "dealer": {
      "id": "dealer-001",
      "name": "AutoMax RD",
      "salesRep": "Carlos Vendedor"
    },
    "schedule": {
      "date": "2026-01-21",
      "time": "10:00",
      "duration": 30
    },
    "legal": {
      "waiverSigned": true,
      "waiverSignedAt": "2026-01-21T09:55:00Z",
      "signatureUrl": "s3://testdrives/td-12345/signature.png",
      "waiverPdf": "s3://testdrives/td-12345/waiver-signed.pdf"
    },
    "execution": {
      "checkedIn": true,
      "checkInTime": "2026-01-21T10:00:00Z",
      "odometerIn": 45230,
      "preTestPhotos": ["s3://testdrives/td-12345/pre-1.jpg", "..."],
      "checkedOut": true,
      "checkOutTime": "2026-01-21T10:35:00Z",
      "odometerOut": 45248,
      "kmDriven": 18,
      "postTestPhotos": ["s3://testdrives/td-12345/post-1.jpg", "..."],
      "routeUsed": "Ruta Urbana Centro"
    },
    "feedback": {
      "overallRating": 5,
      "comfortRating": 4,
      "performanceRating": 5,
      "conditionRating": 5,
      "meetsExpectations": true,
      "interestedInBuying": true,
      "likes": "Excelente manejo, muy silencioso",
      "dislikes": "El color no es mi favorito"
    },
    "outcome": "MADE_OFFER",
    "createdAt": "2026-01-20T15:00:00Z",
    "completedAt": "2026-01-21T10:35:00Z"
  }
}
```

---

## 📊 Métricas Prometheus

```yaml
# Test Drives
testdrive_scheduled_total{dealer}
testdrive_completed_total
testdrive_no_show_total
testdrive_cancelled_total

# Conversión
testdrive_to_offer_rate
testdrive_to_purchase_rate

# Satisfacción
testdrive_rating_average
testdrive_would_recommend_rate

# Operacional
testdrive_avg_km_driven
testdrive_avg_duration_minutes
testdrive_waiver_sign_rate
```

---

## 🔗 Referencias

- [05-AGENDAMIENTO/01-appointment-service.md](01-appointment-service.md)
- [06-CRM-LEADS-CONTACTOS/01-lead-service.md](../06-CRM-LEADS-CONTACTOS/01-lead-service.md)
- [03-VEHICULOS-INVENTARIO/01-vehicles-sale-service.md](../03-VEHICULOS-INVENTARIO/01-vehicles-sale-service.md)
