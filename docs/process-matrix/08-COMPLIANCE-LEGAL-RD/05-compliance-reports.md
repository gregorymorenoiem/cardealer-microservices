# 📜 Compliance Reports - Reportes de Cumplimiento - Matriz de Procesos

> **Servicio:** ComplianceService  
> **Puerto:** 5027  
> **Última actualización:** Enero 25, 2026  
> **Estado:** 🟡 EN DESARROLLO  
> **Estado de Implementación:** 🟡 40% Backend | 🔴 0% UI

---

## ⚠️ AUDITORÍA DE ACCESO UI (Enero 25, 2026)

| Proceso                    | Backend         | UI Access  | Observación   |
| -------------------------- | --------------- | ---------- | ------------- |
| CMPL-606-001 Formato 606   | 🔴 Pendiente    | 🔴 Falta   | Sin generador |
| CMPL-607-001 Formato 607   | 🔴 Pendiente    | 🔴 Falta   | Sin generador |
| CMPL-AML-001 Reportes AML  | 🟡 Parcial      | 🔴 Falta   | Sin dashboard |
| CMPL-AUDIT-001 Audit Trail | ✅ AuditService | 🟡 Parcial | Solo admin    |

### Rutas UI Existentes ✅

- `/admin/audit` → Vista básica de audit log

### Rutas UI Faltantes 🔴

- `/admin/compliance/reports` → Generador de reportes
- `/admin/compliance/dgii/606` → Formato 606 interactivo
- `/admin/compliance/dgii/607` → Formato 607 interactivo
- `/admin/compliance/schedule` → Calendario de vencimientos

**Verificación Backend:** ComplianceService parcialmente implementado 🟡

---

## 📊 Resumen de Implementación

| Componente                       | Total | Implementado | Pendiente | Estado         |
| -------------------------------- | ----- | ------------ | --------- | -------------- |
| **Controllers**                  | 1     | 1            | 0         | ✅ Completo    |
| **CMPL-606-\*** (Reporte 606)    | 3     | 0            | 3         | 🔴 Pendiente   |
| **CMPL-607-\*** (Reporte 607)    | 3     | 0            | 3         | 🔴 Pendiente   |
| **CMPL-AML-\*** (Anti-Lavado)    | 4     | 2            | 2         | 🟡 Parcial     |
| **CMPL-AUDIT-\*** (Auditoría)    | 3     | 3            | 0         | ✅ Completo    |
| **CMPL-SCHED-\*** (Programación) | 3     | 0            | 3         | 🔴 Pendiente   |
| **Tests**                        | 15    | 6            | 9         | 🟡 Parcial     |
| **TOTAL**                        | 32    | 12           | 20        | 🟡 40% Backend |

---

## 1. Información General

### 1.1 Descripción

Sistema de generación de reportes para cumplimiento regulatorio en República Dominicana. Genera reportes para DGII, Pro Consumidor, Superintendencia de Bancos, y auditorías internas.

### 1.2 Tipos de Reportes

| Reporte                 | Destino        | Frecuencia | Descripción                  |
| ----------------------- | -------------- | ---------- | ---------------------------- |
| **606**                 | DGII           | Mensual    | Compras y gastos             |
| **607**                 | DGII           | Mensual    | Ventas de bienes y servicios |
| **NCF Summary**         | DGII           | Mensual    | Resumen de NCF emitidos      |
| **AML Report**          | SB             | Trimestral | Anti-lavado de activos       |
| **Consumer Complaints** | Pro Consumidor | Mensual    | Quejas de consumidores       |
| **Data Privacy**        | INDOTEL        | Anual      | Cumplimiento Ley 172-13      |
| **Audit Trail**         | Interno        | On-demand  | Historial de acciones        |
| **Transaction Report**  | Interno        | Diario     | Transacciones del día        |

### 1.3 Arquitectura

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    Compliance Reports Architecture                       │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│   Report Request                                                        │
│   (Admin Panel or Scheduled Job)                                        │
│        │                                                                │
│        ▼                                                                │
│   ┌─────────────────────────────────────────────────────────────────┐   │
│   │                  ComplianceService API                           │   │
│   │                                                                   │   │
│   │   ┌─────────────────────────────────────────────────────────┐   │   │
│   │   │                Report Generator Engine                   │   │   │
│   │   │                                                          │   │   │
│   │   │   1. Validate parameters (date range, type)             │   │   │
│   │   │   2. Fetch data from multiple services                  │   │   │
│   │   │   3. Apply business rules and calculations              │   │   │
│   │   │   4. Format according to regulatory specs               │   │   │
│   │   │   5. Generate output (PDF, Excel, XML, TXT)             │   │   │
│   │   │   6. Store in S3 and record in DB                       │   │   │
│   │   └─────────────────────────────────────────────────────────┘   │   │
│   └───────────────────────────────┬─────────────────────────────────┘   │
│                                   │                                      │
│              ┌────────────────────┼────────────────────┐                │
│              │                    │                    │                 │
│              ▼                    ▼                    ▼                 │
│   ┌──────────────────┐ ┌──────────────────┐ ┌──────────────────┐        │
│   │   Data Sources   │ │      S3          │ │   PostgreSQL     │        │
│   │                  │ │   (Reports)      │ │   (Metadata)     │        │
│   │   - Billing      │ │                  │ │                  │        │
│   │   - Vehicles     │ │   /compliance/   │ │   - Reports      │        │
│   │   - Users        │ │   /2026/01/      │ │   - Schedules    │        │
│   │   - Audit        │ │   /607_xxx.pdf   │ │   - History      │        │
│   └──────────────────┘ └──────────────────┘ └──────────────────┘        │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 2. Endpoints

### 2.1 Generación de Reportes

| Método | Endpoint                                | Descripción        | Auth       |
| ------ | --------------------------------------- | ------------------ | ---------- |
| `POST` | `/api/compliance/reports/generate`      | Generar reporte    | Compliance |
| `GET`  | `/api/compliance/reports`               | Listar reportes    | Compliance |
| `GET`  | `/api/compliance/reports/{id}`          | Detalle de reporte | Compliance |
| `GET`  | `/api/compliance/reports/{id}/download` | Descargar reporte  | Compliance |

### 2.2 Reportes Específicos

| Método | Endpoint                              | Descripción | Auth       |
| ------ | ------------------------------------- | ----------- | ---------- |
| `POST` | `/api/compliance/reports/606`         | Generar 606 | Compliance |
| `POST` | `/api/compliance/reports/607`         | Generar 607 | Compliance |
| `POST` | `/api/compliance/reports/ncf-summary` | Resumen NCF | Compliance |
| `POST` | `/api/compliance/reports/aml`         | Reporte AML | Compliance |
| `POST` | `/api/compliance/reports/audit-trail` | Audit trail | Admin      |

### 2.3 Programación

| Método   | Endpoint                         | Descripción           | Auth       |
| -------- | -------------------------------- | --------------------- | ---------- |
| `GET`    | `/api/compliance/schedules`      | Listar programados    | Compliance |
| `POST`   | `/api/compliance/schedules`      | Crear programación    | Admin      |
| `DELETE` | `/api/compliance/schedules/{id}` | Cancelar programación | Admin      |

---

## 3. Entidades

### 3.1 ComplianceReport

```csharp
public class ComplianceReport
{
    public Guid Id { get; set; }
    public ReportType Type { get; set; }
    public string Name { get; set; } = string.Empty;

    // Period
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public string PeriodDescription { get; set; } = string.Empty; // "Enero 2026"

    // Status
    public ReportStatus Status { get; set; }
    public string? ErrorMessage { get; set; }

    // Output
    public string? FileUrl { get; set; }
    public string? S3Key { get; set; }
    public string FileFormat { get; set; } = string.Empty; // PDF, Excel, XML
    public long FileSizeBytes { get; set; }

    // Statistics
    public int TotalRecords { get; set; }
    public decimal? TotalAmount { get; set; }
    public Dictionary<string, object> Summary { get; set; } = new();

    // Audit
    public Guid GeneratedById { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public TimeSpan? GenerationDuration { get; set; }
}

public enum ReportType
{
    DGII_606,
    DGII_607,
    NCF_Summary,
    AML_Report,
    ConsumerComplaints,
    DataPrivacy,
    AuditTrail,
    TransactionDaily,
    DealerActivity,
    Custom
}

public enum ReportStatus
{
    Pending,
    Generating,
    Completed,
    Failed,
    Expired
}
```

### 3.2 ReportSchedule

```csharp
public class ReportSchedule
{
    public Guid Id { get; set; }
    public ReportType ReportType { get; set; }
    public string Name { get; set; } = string.Empty;

    // Schedule
    public ScheduleFrequency Frequency { get; set; }
    public int DayOfMonth { get; set; } // 1-28 for monthly
    public DayOfWeek? DayOfWeek { get; set; } // For weekly
    public TimeSpan TimeOfDay { get; set; }
    public string TimeZone { get; set; } = "America/Santo_Domingo";

    // Notifications
    public List<string> NotifyEmails { get; set; } = new();
    public bool AttachReport { get; set; } = true;

    // Status
    public bool IsActive { get; set; } = true;
    public DateTime? NextRunAt { get; set; }
    public DateTime? LastRunAt { get; set; }
    public Guid? LastReportId { get; set; }

    public DateTime CreatedAt { get; set; }
}

public enum ScheduleFrequency
{
    Daily,
    Weekly,
    Monthly,
    Quarterly,
    Yearly
}
```

---

## 4. Formatos de Reportes DGII

### 4.1 Formato 606 (Compras)

```
Especificación DGII Formato 606
================================

Línea de encabezado:
RNC|Periodo|Cantidad de registros

Líneas de detalle:
RNC Proveedor|Tipo ID|NCF|NCF Modificado|Tipo Compra|Fecha|
Monto Pagado|ITBIS Facturado|ITBIS Retenido|Monto Retención Renta|
Tipo Retención|Año de Pago|Mes de Pago|Forma de Pago

Ejemplo:
101010101|2|E310000000001||02|20260115|50000.00|9000.00|0.00|0.00|0|2026|01|04
```

### 4.2 Formato 607 (Ventas)

```
Especificación DGII Formato 607
================================

Línea de encabezado:
RNC|Periodo|Cantidad de registros

Líneas de detalle:
RNC/Cédula Cliente|Tipo ID|NCF|NCF Modificado|Tipo Ingreso|Fecha|
Fecha Retención|Monto Facturado|ITBIS Facturado|ITBIS Retenido por Terceros|
ITBIS Percibido|Retención Renta por Terceros|ISR Percibido|
Impuesto Selectivo al Consumo|Otros Impuestos|Monto Propina Legal|
Efectivo|Cheque/Transferencia|Tarjeta Débito/Crédito|
Venta a Crédito|Bonos o Certificados|Permuta|Otras Formas Venta

Ejemplo:
40212345678|1|B0100000001||01|20260115||15000.00|2700.00|0.00|0.00|0.00|0.00|0.00|0.00|0.00|15000.00|0.00|0.00|0.00|0.00|0.00|0.00
```

---

## 5. Procesos Detallados

### 5.1 COMP-001: Generar Reporte 607

| Paso | Acción                    | Sistema             | Validación         |
| ---- | ------------------------- | ------------------- | ------------------ |
| 1    | Solicitar reporte 607     | Admin               | Compliance auth    |
| 2    | Validar período           | ComplianceService   | Valid period       |
| 3    | Crear registro de reporte | PostgreSQL          | Report record      |
| 4    | Obtener transacciones     | BillingService      | Date range         |
| 5    | Obtener datos de clientes | UserService         | Customer data      |
| 6    | Filtrar ventas con NCF    | ComplianceService   | NCF required       |
| 7    | Calcular totales por tipo | ComplianceService   | Aggregations       |
| 8    | Formatear según DGII      | ComplianceService   | Format 607         |
| 9    | Generar archivo TXT       | ComplianceService   | File created       |
| 10   | Upload a S3               | S3                  | File stored        |
| 11   | Actualizar registro       | PostgreSQL          | Status = Completed |
| 12   | Notificar a compliance    | NotificationService | Email sent         |

```csharp
public class Generate607ReportHandler : IRequestHandler<Generate607Command, ComplianceReport>
{
    public async Task<ComplianceReport> Handle(Generate607Command request, CancellationToken ct)
    {
        var report = new ComplianceReport
        {
            Type = ReportType.DGII_607,
            Name = $"Formato 607 - {request.Period:MMMM yyyy}",
            PeriodStart = new DateTime(request.Year, request.Month, 1),
            PeriodEnd = new DateTime(request.Year, request.Month, 1).AddMonths(1).AddDays(-1),
            PeriodDescription = $"{request.Month:00}/{request.Year}",
            Status = ReportStatus.Generating,
            GeneratedById = request.UserId,
            CreatedAt = DateTime.UtcNow
        };

        await _reportRepository.AddAsync(report, ct);

        try
        {
            // 1. Get all invoices for the period
            var invoices = await _billingService.GetInvoicesAsync(
                report.PeriodStart,
                report.PeriodEnd,
                ct);

            // 2. Filter only those with NCF
            var ncfInvoices = invoices.Where(i => !string.IsNullOrEmpty(i.NcfNumber)).ToList();

            // 3. Enrich with customer data
            var customerIds = ncfInvoices.Select(i => i.CustomerId).Distinct();
            var customers = await _userService.GetUsersAsync(customerIds, ct);
            var customerMap = customers.ToDictionary(c => c.Id);

            // 4. Build 607 lines
            var lines = new List<string>();

            // Header
            lines.Add($"{_companyRnc}|{report.PeriodDescription}|{ncfInvoices.Count}");

            // Detail lines
            foreach (var invoice in ncfInvoices.OrderBy(i => i.CreatedAt))
            {
                var customer = customerMap.GetValueOrDefault(invoice.CustomerId);

                var line = new StringBuilder();
                line.Append(customer?.Rnc ?? customer?.Cedula ?? "");
                line.Append("|");
                line.Append(GetIdType(customer));
                line.Append("|");
                line.Append(invoice.NcfNumber);
                line.Append("|"); // NCF Modificado
                line.Append(GetIncomeType(invoice));
                line.Append("|");
                line.Append(invoice.CreatedAt.ToString("yyyyMMdd"));
                line.Append("|"); // Fecha Retención
                line.Append(invoice.Total.ToString("F2"));
                line.Append("|");
                line.Append(invoice.TaxAmount.ToString("F2"));
                // ... remaining fields

                lines.Add(line.ToString());
            }

            // 5. Generate file
            var content = string.Join("\r\n", lines);
            var fileName = $"607_{_companyRnc}_{report.PeriodDescription.Replace("/", "")}.txt";

            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

            // 6. Upload to S3
            var s3Key = $"compliance/{request.Year}/{request.Month:00}/{fileName}";
            await _s3Client.UploadAsync(s3Key, stream, "text/plain", ct);

            // 7. Update report
            report.Status = ReportStatus.Completed;
            report.S3Key = s3Key;
            report.FileUrl = $"https://reports.okla.com.do/{s3Key}";
            report.FileFormat = "TXT";
            report.FileSizeBytes = stream.Length;
            report.TotalRecords = ncfInvoices.Count;
            report.TotalAmount = ncfInvoices.Sum(i => i.Total);
            report.CompletedAt = DateTime.UtcNow;
            report.GenerationDuration = DateTime.UtcNow - report.CreatedAt;
            report.Summary = new Dictionary<string, object>
            {
                ["totalInvoices"] = ncfInvoices.Count,
                ["totalAmount"] = ncfInvoices.Sum(i => i.Total),
                ["totalTax"] = ncfInvoices.Sum(i => i.TaxAmount),
                ["byPaymentMethod"] = ncfInvoices.GroupBy(i => i.PaymentMethod)
                    .ToDictionary(g => g.Key, g => g.Sum(i => i.Total))
            };

            await _reportRepository.UpdateAsync(report, ct);

            // 8. Notify
            await _notificationService.SendReportReadyAsync(report, ct);

            return report;
        }
        catch (Exception ex)
        {
            report.Status = ReportStatus.Failed;
            report.ErrorMessage = ex.Message;
            await _reportRepository.UpdateAsync(report, ct);
            throw;
        }
    }
}
```

### 5.2 COMP-002: Reporte AML (Anti-Lavado)

| Paso | Acción                            | Sistema             | Validación          |
| ---- | --------------------------------- | ------------------- | ------------------- |
| 1    | Trigger trimestral                | Scheduler           | Quarterly           |
| 2    | Obtener transacciones > $10,000   | BillingService      | Threshold           |
| 3    | Obtener PICs                      | CRMService          | Politically Exposed |
| 4    | Obtener transacciones sospechosas | AuditService        | Flagged             |
| 5    | Agregar por cliente               | ComplianceService   | Group by            |
| 6    | Calcular risk scores              | ComplianceService   | ML model            |
| 7    | Generar reporte SB                | ComplianceService   | Format required     |
| 8    | Subir a S3                        | S3                  | Encrypted           |
| 9    | Notificar compliance officer      | NotificationService | Urgent              |

---

## 6. Programación de Reportes

```yaml
# Reportes automáticos configurados
schedules:
  - type: DGII_607
    frequency: Monthly
    dayOfMonth: 15
    timeOfDay: "02:00"
    notifyEmails:
      - compliance@okla.com.do
      - cfo@okla.com.do

  - type: DGII_606
    frequency: Monthly
    dayOfMonth: 15
    timeOfDay: "03:00"
    notifyEmails:
      - compliance@okla.com.do

  - type: AML_Report
    frequency: Quarterly
    dayOfMonth: 10 # 10 del primer mes del trimestre
    timeOfDay: "04:00"
    notifyEmails:
      - compliance@okla.com.do
      - legal@okla.com.do

  - type: TransactionDaily
    frequency: Daily
    timeOfDay: "06:00"
    notifyEmails:
      - finance@okla.com.do
```

---

## 7. Reglas de Negocio

| Código   | Regla                             | Validación        |
| -------- | --------------------------------- | ----------------- |
| COMP-R01 | 607 solo incluye ventas con NCF   | NCF not null      |
| COMP-R02 | 606 solo para proveedores con RNC | RNC required      |
| COMP-R03 | AML incluye transacciones > $10K  | Amount threshold  |
| COMP-R04 | Reportes DGII deadline: día 15    | Date check        |
| COMP-R05 | Archivos se encriptan en reposo   | S3 encryption     |
| COMP-R06 | Retención de reportes: 10 años    | Legal requirement |

---

## 8. Códigos de Error

| Código     | HTTP | Mensaje                  | Causa            |
| ---------- | ---- | ------------------------ | ---------------- |
| `COMP_001` | 400  | Invalid period           | Período inválido |
| `COMP_002` | 400  | No data for period       | Sin datos        |
| `COMP_003` | 500  | Report generation failed | Error interno    |
| `COMP_004` | 404  | Report not found         | No existe        |
| `COMP_005` | 403  | Cannot access report     | Sin permiso      |

---

## 9. Eventos RabbitMQ

| Evento                  | Exchange            | Descripción     |
| ----------------------- | ------------------- | --------------- |
| `ReportGeneratedEvent`  | `compliance.events` | Reporte listo   |
| `ReportFailedEvent`     | `compliance.events` | Error generando |
| `ScheduleExecutedEvent` | `compliance.events` | Job ejecutado   |

---

## 10. Métricas Prometheus

```
# Report generation
compliance_reports_generated_total{type="...", status="completed|failed"}
compliance_report_generation_duration_seconds{type="..."}
compliance_report_records_total{type="..."}

# Schedules
compliance_schedule_executions_total{type="...", status="..."}
compliance_schedule_next_run_timestamp{type="..."}
```

---

## 📚 Referencias

- [01-ley-155-17.md](01-ley-155-17.md) - Ley 155-17 AML
- [03-dgii-integration.md](03-dgii-integration.md) - Integración DGII
- [01-reports-service.md](../09-REPORTES-ANALYTICS/01-reports-service.md) - Reportes generales
