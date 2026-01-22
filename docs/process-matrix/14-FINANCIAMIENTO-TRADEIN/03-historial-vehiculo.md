# 📜 Historial de Vehículo (CARFAX RD)

> **Código:** VEH-004  
> **Versión:** 1.0  
> **Última actualización:** Enero 21, 2026  
> **Criticidad:** 🔴 ALTA (Diferenciador clave del marketplace)

---

## 📋 Información General

| Campo             | Valor                                                         |
| ----------------- | ------------------------------------------------------------- |
| **Servicio**      | VehicleHistoryService                                         |
| **Puerto**        | 5084                                                          |
| **Base de Datos** | `vehiclehistoryservice`                                       |
| **Dependencias**  | VehiclesSaleService, TrustService                             |
| **Integraciones** | INTRANT, DGII, Aseguradoras (Seguros Universal, Mapfre, etc.) |

---

## 🎯 Objetivo del Proceso

Proporcionar un historial completo del vehículo similar a CARFAX, incluyendo:

1. **Historial de propietarios** - Cuántos dueños ha tenido
2. **Accidentes reportados** - Claims de seguro, siniestros
3. **Historial de mantenimiento** - Servicios realizados
4. **Verificación de kilometraje** - Detección de alteración
5. **Estado legal** - Gravámenes, multas, robos

---

## 📡 Endpoints

| Método | Endpoint                     | Descripción                 | Auth |
| ------ | ---------------------------- | --------------------------- | ---- |
| `GET`  | `/api/history/{vin}`         | Obtener historial por VIN   | ✅   |
| `GET`  | `/api/history/plate/{plate}` | Obtener historial por placa | ✅   |
| `POST` | `/api/history/request`       | Solicitar reporte completo  | ✅   |
| `GET`  | `/api/history/reports`       | Mis reportes comprados      | ✅   |
| `GET`  | `/api/history/sample/{vin}`  | Vista previa gratis         | ❌   |

---

## 🗃️ Entidades

### VehicleHistory

```csharp
public class VehicleHistory
{
    public Guid Id { get; set; }
    public string VIN { get; set; }
    public string LicensePlate { get; set; }

    // Información del vehículo
    public string Make { get; set; }
    public string Model { get; set; }
    public int Year { get; set; }
    public string Color { get; set; }
    public string EngineNumber { get; set; }
    public string ChassisNumber { get; set; }

    // Historial de propietarios
    public int TotalOwners { get; set; }
    public List<OwnershipRecord> Owners { get; set; }

    // Accidentes
    public int TotalAccidents { get; set; }
    public List<AccidentRecord> Accidents { get; set; }

    // Mantenimiento
    public List<MaintenanceRecord> MaintenanceHistory { get; set; }

    // Kilometraje
    public List<MileageRecord> MileageHistory { get; set; }
    public bool OdometerRollbackDetected { get; set; }

    // Estado legal
    public LegalStatus LegalStatus { get; set; }

    // Score de confiabilidad
    public int ReliabilityScore { get; set; }    // 0-100
    public HistoryRating Rating { get; set; }

    // Metadata
    public DateTime LastUpdated { get; set; }
    public DateTime DataAsOf { get; set; }
    public List<string> DataSources { get; set; }
}

public enum HistoryRating
{
    Excellent,    // Sin problemas, 1 dueño
    Good,         // 1-2 dueños, sin accidentes
    Fair,         // 3+ dueños o accidente menor
    Concern,      // Accidente mayor o problema legal menor
    Warning       // Múltiples problemas, título salvage
}
```

### OwnershipRecord

```csharp
public class OwnershipRecord
{
    public int OwnerNumber { get; set; }          // 1, 2, 3...
    public OwnerType Type { get; set; }
    public string Province { get; set; }          // Sin datos personales
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? EstimatedKilometersOwned { get; set; }
    public UsageType EstimatedUsage { get; set; }
}

public enum OwnerType
{
    Individual,
    Dealer,
    RentalCompany,
    Government,
    Commercial
}

public enum UsageType
{
    Personal,
    Commercial,
    Rental,
    Government,
    Taxi,
    Unknown
}
```

### AccidentRecord

```csharp
public class AccidentRecord
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public string Province { get; set; }

    public AccidentSeverity Severity { get; set; }
    public List<string> DamagedAreas { get; set; }  // Front, Rear, Side, etc.
    public bool Airbags { get; set; }

    public string InsuranceCompany { get; set; }
    public decimal? ClaimAmount { get; set; }
    public bool TotalLoss { get; set; }

    public string Description { get; set; }
}

public enum AccidentSeverity
{
    Minor,      // Daños menores, sin bolsas de aire
    Moderate,   // Daños moderados, posible estructural
    Major,      // Daños mayores, bolsas activadas
    TotalLoss   // Pérdida total
}
```

### MileageRecord

```csharp
public class MileageRecord
{
    public DateTime Date { get; set; }
    public int Kilometers { get; set; }
    public string Source { get; set; }           // INTRANT, Service, Sale, etc.
    public bool Verified { get; set; }
}
```

### LegalStatus

```csharp
public class LegalStatus
{
    // Título
    public TitleStatus TitleStatus { get; set; }
    public bool HasSalvageTitle { get; set; }
    public bool HasRebuiltTitle { get; set; }

    // Gravámenes
    public bool HasLiens { get; set; }
    public List<LienRecord> Liens { get; set; }

    // Multas
    public int PendingTickets { get; set; }
    public decimal PendingTicketAmount { get; set; }

    // Robo
    public bool ReportedStolen { get; set; }
    public DateTime? StolenReportDate { get; set; }

    // INTRANT
    public bool InspectionCurrent { get; set; }
    public DateTime? LastInspectionDate { get; set; }
    public DateTime? NextInspectionDue { get; set; }

    // Importación
    public bool IsImported { get; set; }
    public string ImportOrigin { get; set; }
    public DateTime? ImportDate { get; set; }
}

public enum TitleStatus
{
    Clean,
    Salvage,
    Rebuilt,
    Flood,
    Lemon,
    Unknown
}
```

### HistoryReport

```csharp
public class HistoryReport
{
    public Guid Id { get; set; }
    public Guid RequestedBy { get; set; }
    public string VIN { get; set; }
    public Guid? VehicleId { get; set; }

    // Contenido
    public VehicleHistory History { get; set; }
    public string PdfUrl { get; set; }

    // Pago
    public ReportType Type { get; set; }
    public decimal Price { get; set; }
    public Guid? PaymentId { get; set; }

    // Validez
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }         // 30 días
    public int ViewsRemaining { get; set; }          // Ilimitado o 5
}

public enum ReportType
{
    Basic,          // RD$ 500 - Solo vista previa
    Standard,       // RD$ 1,500 - Reporte completo
    Premium         // RD$ 2,500 - Reporte + 3 actualizaciones
}
```

---

## 📊 Proceso VEH-004: Obtener Historial de Vehículo

```
┌─────────────────────────────────────────────────────────────────────────┐
│ PROCESO: VEH-004 - Obtener Historial de Vehículo                       │
├─────────────────────────────────────────────────────────────────────────┤
│ Actor Iniciador: USR-REG (Comprador interesado)                        │
│ Sistemas: VehicleHistoryService, BillingService, External APIs         │
│ Duración: Instantáneo (datos cached) o 5-30 min (fetch nuevo)          │
│ Criticidad: ALTA                                                        │
└─────────────────────────────────────────────────────────────────────────┘
```

| Paso | Subpaso | Acción                          | Sistema               | Actor     | Evidencia              | Código     |
| ---- | ------- | ------------------------------- | --------------------- | --------- | ---------------------- | ---------- |
| 1    | 1.1     | Usuario ve listing              | Frontend              | USR-REG   | Listing viewed         | EVD-LOG    |
| 1    | 1.2     | Click "Ver Historial"           | Frontend              | USR-REG   | CTA clicked            | EVD-LOG    |
| 2    | 2.1     | GET /api/history/sample/{vin}   | Gateway               | USR-REG   | **Request**            | EVD-AUDIT  |
| 2    | 2.2     | Mostrar preview gratis          | Frontend              | USR-REG   | Preview shown          | EVD-LOG    |
| 3    | 3.1     | Usuario quiere reporte completo | Frontend              | USR-REG   | CTA clicked            | EVD-LOG    |
| 3    | 3.2     | Seleccionar tipo de reporte     | Frontend              | USR-REG   | Type selected          | EVD-LOG    |
| 4    | 4.1     | POST /api/history/request       | Gateway               | USR-REG   | **Request**            | EVD-AUDIT  |
| 4    | 4.2     | Verificar cache existente       | VehicleHistoryService | Sistema   | Cache check            | EVD-LOG    |
| 5    | 5.1     | Crear payment intent            | BillingService        | Sistema   | Payment intent         | EVD-LOG    |
| 5    | 5.2     | Usuario paga                    | Stripe/Azul           | USR-REG   | **Payment**            | EVD-AUDIT  |
| 5    | 5.3     | Confirmar pago                  | BillingService        | Sistema   | Payment confirmed      | EVD-EVENT  |
| 6    | 6.1     | **Fetch datos de INTRANT**      | VehicleHistoryService | Sistema   | **INTRANT data**       | EVD-AUDIT  |
| 6    | 6.2     | **Fetch datos de aseguradoras** | VehicleHistoryService | Sistema   | **Insurance data**     | EVD-AUDIT  |
| 6    | 6.3     | **Fetch datos de DGII**         | VehicleHistoryService | Sistema   | **DGII data**          | EVD-AUDIT  |
| 7    | 7.1     | Compilar historial              | VehicleHistoryService | Sistema   | History compiled       | EVD-LOG    |
| 7    | 7.2     | Detectar inconsistencias        | VehicleHistoryService | Sistema   | **Anomalies detected** | EVD-AUDIT  |
| 7    | 7.3     | Calcular reliability score      | VehicleHistoryService | Sistema   | Score calculated       | EVD-LOG    |
| 8    | 8.1     | **Crear VehicleHistory**        | VehicleHistoryService | Sistema   | **History created**    | EVD-AUDIT  |
| 8    | 8.2     | **Generar PDF**                 | VehicleHistoryService | Sistema   | **PDF generated**      | EVD-DOC    |
| 9    | 9.1     | **Crear HistoryReport**         | VehicleHistoryService | Sistema   | **Report created**     | EVD-AUDIT  |
| 9    | 9.2     | Guardar en cache (7 días)       | VehicleHistoryService | Sistema   | Cache stored           | EVD-LOG    |
| 10   | 10.1    | **Mostrar reporte**             | Frontend              | USR-REG   | Report displayed       | EVD-SCREEN |
| 10   | 10.2    | Enviar PDF por email            | NotificationService   | SYS-NOTIF | **PDF sent**           | EVD-COMM   |
| 11   | 11.1    | **Audit trail**                 | AuditService          | Sistema   | Complete audit         | EVD-AUDIT  |

### Evidencia de Reporte de Historial

```json
{
  "processCode": "VEH-004",
  "historyReport": {
    "id": "report-12345",
    "requestedBy": {
      "userId": "user-001",
      "name": "María Compradora"
    },
    "vehicle": {
      "vin": "1HGBH41JXMN109186",
      "plate": "A123456",
      "make": "Honda",
      "model": "Civic",
      "year": 2021,
      "color": "Blanco"
    },
    "type": "STANDARD",
    "payment": {
      "amount": 1500,
      "currency": "DOP",
      "transactionId": "pay-12345"
    },
    "history": {
      "reliabilityScore": 82,
      "rating": "GOOD",
      "summary": {
        "totalOwners": 2,
        "totalAccidents": 0,
        "odometerOK": true,
        "titleClean": true,
        "noLiens": true
      },
      "owners": [
        {
          "ownerNumber": 1,
          "type": "INDIVIDUAL",
          "province": "Santo Domingo",
          "startDate": "2021-03-15",
          "endDate": "2024-06-20",
          "estimatedKm": 45000,
          "usage": "PERSONAL"
        },
        {
          "ownerNumber": 2,
          "type": "INDIVIDUAL",
          "province": "Santiago",
          "startDate": "2024-06-20",
          "endDate": null,
          "estimatedKm": 12000,
          "usage": "PERSONAL"
        }
      ],
      "accidents": [],
      "mileageHistory": [
        { "date": "2021-03-15", "km": 0, "source": "Sale", "verified": true },
        {
          "date": "2022-03-10",
          "km": 15200,
          "source": "Service",
          "verified": true
        },
        {
          "date": "2023-03-08",
          "km": 31500,
          "source": "INTRANT",
          "verified": true
        },
        {
          "date": "2024-06-20",
          "km": 45000,
          "source": "Sale",
          "verified": true
        },
        {
          "date": "2025-01-15",
          "km": 57000,
          "source": "INTRANT",
          "verified": true
        }
      ],
      "odometerAnalysis": {
        "consistent": true,
        "averageKmPerYear": 14250,
        "rollbackDetected": false
      },
      "legalStatus": {
        "title": "CLEAN",
        "salvage": false,
        "liens": [],
        "pendingTickets": 0,
        "stolen": false,
        "inspectionCurrent": true,
        "lastInspection": "2025-01-15",
        "nextInspectionDue": "2026-01-15"
      },
      "importInfo": {
        "imported": true,
        "origin": "USA",
        "importDate": "2021-02-28",
        "importedKm": 0,
        "importCondition": "NEW"
      },
      "recalls": [
        {
          "issueDate": "2022-05-15",
          "description": "Airbag sensor recall",
          "status": "COMPLETED",
          "completedDate": "2022-08-20"
        }
      ]
    },
    "dataSources": [
      "INTRANT",
      "DGII",
      "Seguros Universal",
      "Honda RD (Manufacturer)"
    ],
    "dataAsOf": "2026-01-21T10:30:00Z",
    "pdf": "s3://history/report-12345/full-report.pdf",
    "expiresAt": "2026-02-20T10:30:00Z",
    "viewsRemaining": -1
  }
}
```

---

## 📊 Detección de Anomalías

```csharp
public class AnomalyDetector
{
    public List<Anomaly> Detect(VehicleHistory history)
    {
        var anomalies = new List<Anomaly>();

        // 1. Detección de rollback de odómetro
        var mileages = history.MileageHistory.OrderBy(m => m.Date).ToList();
        for (int i = 1; i < mileages.Count; i++)
        {
            if (mileages[i].Kilometers < mileages[i-1].Kilometers)
            {
                anomalies.Add(new Anomaly
                {
                    Type = AnomalyType.OdometerRollback,
                    Severity = AnomalySeverity.Critical,
                    Description = $"Kilometraje bajó de {mileages[i-1].Kilometers} a {mileages[i].Kilometers}",
                    Date = mileages[i].Date
                });
            }
        }

        // 2. Kilometraje excesivo (>40,000 km/año)
        var kmPerYear = CalculateKmPerYear(mileages);
        if (kmPerYear > 40000)
        {
            anomalies.Add(new Anomaly
            {
                Type = AnomalyType.ExcessiveMileage,
                Severity = AnomalySeverity.Warning,
                Description = $"Uso intensivo: {kmPerYear:N0} km/año"
            });
        }

        // 3. Cambios frecuentes de dueño
        if (history.TotalOwners > 3 && CalculateVehicleAge(history) < 5)
        {
            anomalies.Add(new Anomaly
            {
                Type = AnomalyType.FrequentOwnership,
                Severity = AnomalySeverity.Concern,
                Description = $"{history.TotalOwners} dueños en {CalculateVehicleAge(history)} años"
            });
        }

        // 4. Uso comercial/rental
        if (history.Owners.Any(o => o.Type == OwnerType.RentalCompany))
        {
            anomalies.Add(new Anomaly
            {
                Type = AnomalyType.RentalHistory,
                Severity = AnomalySeverity.Info,
                Description = "Vehículo fue usado como rental"
            });
        }

        // 5. Accidente mayor
        if (history.Accidents.Any(a => a.Severity >= AccidentSeverity.Major))
        {
            anomalies.Add(new Anomaly
            {
                Type = AnomalyType.MajorAccident,
                Severity = AnomalySeverity.Critical,
                Description = "Accidente mayor reportado"
            });
        }

        return anomalies;
    }
}

public enum AnomalyType
{
    OdometerRollback,
    ExcessiveMileage,
    FrequentOwnership,
    RentalHistory,
    MajorAccident,
    TitleIssue,
    UnresolvedRecall,
    FloodDamage
}

public enum AnomalySeverity
{
    Info,       // Información, no afecta score
    Warning,    // Alerta, -5 al score
    Concern,    // Preocupación, -15 al score
    Critical    // Crítico, -30 al score
}
```

---

## 💰 Precios de Reportes

| Tipo                 | Precio (DOP) | Incluye                               | Validez            |
| -------------------- | ------------ | ------------------------------------- | ------------------ |
| **Preview (Gratis)** | RD$ 0        | # dueños, # accidentes, score general | Vista instantánea  |
| **Basic**            | RD$ 500      | Preview + detalles básicos            | 7 días, 1 vista    |
| **Standard**         | RD$ 1,500    | Reporte completo + PDF                | 30 días, ilimitado |
| **Premium**          | RD$ 2,500    | Standard + 3 actualizaciones          | 90 días, ilimitado |

### Bundles

| Bundle                 | Precio     | Ahorro |
| ---------------------- | ---------- | ------ |
| 3 reportes Standard    | RD$ 3,500  | 22%    |
| 5 reportes Standard    | RD$ 5,500  | 27%    |
| Dealer (ilimitado/mes) | RD$ 15,000 | -      |

---

## 🔌 Integraciones Externas

### INTRANT (Dirección General de Tránsito)

```csharp
public interface IIntrantClient
{
    Task<VehicleRegistration> GetRegistrationAsync(string plate);
    Task<List<InspectionRecord>> GetInspectionsAsync(string plate);
    Task<List<TicketRecord>> GetTicketsAsync(string plate);
    Task<bool> VerifyOwnershipAsync(string plate, string cedula);
}
```

### Aseguradoras (API genérica)

```csharp
public interface IInsuranceProvider
{
    string ProviderName { get; }
    Task<List<ClaimRecord>> GetClaimsAsync(string vin);
    Task<bool> IsCurrentlyInsuredAsync(string vin);
}
```

### DGII (Verificación de RNC)

```csharp
public interface IDgiiClient
{
    Task<ImportRecord> GetImportInfoAsync(string vin);
    Task<TaxStatus> GetTaxStatusAsync(string plate);
}
```

---

## 📊 Métricas Prometheus

```yaml
# Reportes
history_reports_total{type}
history_report_revenue_total
history_cache_hit_rate

# Datos externos
history_intrant_requests_total{status}
history_insurance_requests_total{provider, status}
history_dgii_requests_total{status}

# Anomalías
history_anomalies_detected_total{type, severity}

# Performance
history_report_generation_time_seconds
history_external_api_latency_seconds{provider}
```

---

## 🔗 Referencias

- [03-VEHICULOS-INVENTARIO/01-vehicles-sale-service.md](../03-VEHICULOS-INVENTARIO/01-vehicles-sale-service.md)
- [15-CONFIANZA-SEGURIDAD/01-verificacion-identidad.md](../15-CONFIANZA-SEGURIDAD/01-verificacion-identidad.md)
- [INTRANT Portal](https://intrant.gob.do)
- [DGII Portal](https://dgii.gov.do)
