# 📊 Reportes Automatizados - Especificación Técnica

> **Propósito:** Definir los reportes que el sistema debe generar automáticamente  
> **Servicio:** FiscalReportingService (Puerto 5072)  
> **Última actualización:** Enero 25, 2026

---

## 📋 CATÁLOGO DE REPORTES

### Reportes DGII

| ID       | Reporte                   | Frecuencia   | Formato   | Prioridad  |
| -------- | ------------------------- | ------------ | --------- | ---------- |
| DGII-001 | Formato 606 (Compras)     | Mensual      | TXT/Excel | 🔴 Crítica |
| DGII-002 | Formato 607 (Ventas)      | Mensual      | TXT/Excel | 🔴 Crítica |
| DGII-003 | Formato 608 (Anulaciones) | Mensual      | TXT/Excel | 🔴 Crítica |
| DGII-004 | Resumen ITBIS             | Mensual      | PDF/Excel | 🔴 Crítica |
| DGII-005 | Libro de Ventas           | Mensual      | Excel     | 🟡 Alta    |
| DGII-006 | Libro de Compras          | Mensual      | Excel     | 🟡 Alta    |
| DGII-007 | Secuencias NCF            | Bajo demanda | Excel     | 🟡 Alta    |

### Reportes UAF/AML

| ID      | Reporte                  | Frecuencia | Formato    | Prioridad  |
| ------- | ------------------------ | ---------- | ---------- | ---------- |
| UAF-001 | Alertas AML Generadas    | Diario     | Excel/JSON | 🔴 Crítica |
| UAF-002 | Transacciones Alto Valor | Diario     | Excel      | 🔴 Crítica |
| UAF-003 | Usuarios Alto Riesgo     | Semanal    | Excel      | 🟡 Alta    |
| UAF-004 | ROS Pendientes           | Diario     | Internal   | 🔴 Crítica |
| UAF-005 | Resumen KYC              | Mensual    | PDF        | 🟡 Alta    |
| UAF-006 | Capacitaciones AML       | Trimestral | PDF        | 🟢 Media   |

### Reportes Protección de Datos

| ID     | Reporte                  | Frecuencia | Formato | Prioridad  |
| ------ | ------------------------ | ---------- | ------- | ---------- |
| PD-001 | Solicitudes ARCO         | Mensual    | Excel   | 🟡 Alta    |
| PD-002 | Consentimientos          | Mensual    | Excel   | 🟡 Alta    |
| PD-003 | Brechas de Seguridad     | Por evento | PDF     | 🔴 Crítica |
| PD-004 | Acceso a Datos Sensibles | Semanal    | Excel   | 🟡 Alta    |

### Reportes Pro Consumidor

| ID     | Reporte                 | Frecuencia   | Formato | Prioridad |
| ------ | ----------------------- | ------------ | ------- | --------- |
| PC-001 | Quejas Recibidas        | Mensual      | Excel   | 🟡 Alta   |
| PC-002 | Tiempo de Resolución    | Mensual      | PDF     | 🟡 Alta   |
| PC-003 | Devoluciones/Reembolsos | Mensual      | Excel   | 🟡 Alta   |
| PC-004 | Libro de Reclamaciones  | Bajo demanda | PDF     | 🟡 Alta   |

### Reportes Internos/Auditoría

| ID      | Reporte                | Frecuencia | Formato   | Prioridad  |
| ------- | ---------------------- | ---------- | --------- | ---------- |
| AUD-001 | Resumen Ejecutivo      | Mensual    | PDF       | 🔴 Crítica |
| AUD-002 | Estado de Cumplimiento | Semanal    | Dashboard | 🔴 Crítica |
| AUD-003 | Obligaciones Vencidas  | Diario     | Alerta    | 🔴 Crítica |
| AUD-004 | Log de Auditoría       | Continuo   | JSON      | 🟡 Alta    |

---

## 📄 ESPECIFICACIÓN FORMATO 606

### Estructura del Archivo

```
Nombre: 606RNCAAAAMMDD.txt
Donde:
  - RNC: RNC del contribuyente (11 dígitos con guiones)
  - AAAA: Año del período
  - MM: Mes del período
  - DD: Día de generación
```

### Campos del Registro

| #   | Campo                         | Longitud | Tipo | Descripción                         |
| --- | ----------------------------- | -------- | ---- | ----------------------------------- |
| 1   | RNC/Cédula Proveedor          | 11       | AN   | Sin guiones                         |
| 2   | Tipo Identificación           | 1        | N    | 1=RNC, 2=Cédula                     |
| 3   | Tipo Bienes/Servicios         | 2        | N    | 01=Gastos personal, 02=Servicios... |
| 4   | NCF                           | 19       | AN   | Comprobante fiscal                  |
| 5   | NCF Modificado                | 19       | AN   | Si es nota de crédito               |
| 6   | Fecha Comprobante             | 8        | N    | AAAAMMDD                            |
| 7   | Fecha Pago                    | 8        | N    | AAAAMMDD                            |
| 8   | Monto Servicios               | 12       | N    | Sin decimales (centavos)            |
| 9   | Monto Bienes                  | 12       | N    | Sin decimales                       |
| 10  | Total Facturado               | 12       | N    | Sin ITBIS                           |
| 11  | ITBIS Facturado               | 12       | N    | 18%                                 |
| 12  | ITBIS Retenido                | 12       | N    | Si aplica retención                 |
| 13  | ITBIS Sujeto Proporcionalidad | 12       | N    | Si aplica                           |
| 14  | ITBIS a Costo                 | 12       | N    | Si aplica                           |
| 15  | ITBIS Adelantado              | 12       | N    | Si aplica                           |
| 16  | ISR Retenido                  | 12       | N    | Si aplica                           |
| 17  | Tipo Retención ISR            | 2        | N    | Código retención                    |
| 18  | Forma Pago                    | 2        | N    | 01=Efectivo, 02=Cheque...           |

### Query de Generación

```sql
-- Generar datos para Formato 606
SELECT
    REPLACE(s.rnc, '-', '') as rnc_proveedor,
    '1' as tipo_id,  -- 1=RNC
    e.tipo_bien_servicio as tipo_bs,
    e.ncf_proveedor as ncf,
    '' as ncf_modificado,
    TO_CHAR(e.fecha, 'YYYYMMDD') as fecha_comprobante,
    TO_CHAR(e.fecha_pago, 'YYYYMMDD') as fecha_pago,
    ROUND(CASE WHEN e.es_servicio THEN e.subtotal * 100 ELSE 0 END) as monto_servicios,
    ROUND(CASE WHEN NOT e.es_servicio THEN e.subtotal * 100 ELSE 0 END) as monto_bienes,
    ROUND(e.subtotal * 100) as total_facturado,
    ROUND(e.itbis * 100) as itbis_facturado,
    ROUND(COALESCE(e.itbis_retenido, 0) * 100) as itbis_retenido,
    0 as itbis_proporcionalidad,
    0 as itbis_costo,
    0 as itbis_adelantado,
    ROUND(COALESCE(e.isr_retenido, 0) * 100) as isr_retenido,
    COALESCE(e.tipo_retencion_isr, '') as tipo_retencion,
    e.forma_pago as forma_pago
FROM expenses e
JOIN suppliers s ON e.supplier_id = s.id
WHERE e.fecha BETWEEN :fecha_inicio AND :fecha_fin
AND e.has_valid_ncf = true
ORDER BY e.fecha;
```

### Validaciones

```csharp
public class Formato606Validator
{
    public ValidationResult Validate(Formato606Record record)
    {
        var errors = new List<string>();

        // RNC válido
        if (!IsValidRnc(record.RncProveedor))
            errors.Add("RNC de proveedor inválido");

        // NCF válido
        if (!IsValidNcf(record.Ncf))
            errors.Add("NCF inválido");

        // Fecha válida
        if (record.FechaComprobante > DateTime.Now)
            errors.Add("Fecha de comprobante futura no permitida");

        // Montos consistentes
        if (record.TotalFacturado != record.MontoServicios + record.MontoBienes)
            errors.Add("Total no coincide con suma de servicios y bienes");

        // ITBIS correcto (18%)
        var itbisEsperado = record.TotalFacturado * 0.18m;
        if (Math.Abs(record.ItbisFacturado - itbisEsperado) > 1)
            errors.Add("ITBIS no corresponde al 18%");

        return new ValidationResult(errors);
    }
}
```

---

## 📄 ESPECIFICACIÓN FORMATO 607

### Estructura del Archivo

```
Nombre: 607RNCAAAAMMDD.txt
```

### Campos del Registro

| #   | Campo                        | Longitud | Tipo | Descripción                    |
| --- | ---------------------------- | -------- | ---- | ------------------------------ |
| 1   | RNC/Cédula Cliente           | 11       | AN   | Sin guiones (vacío si B02)     |
| 2   | Tipo Identificación          | 1        | N    | 1=RNC, 2=Cédula, 3=Pasaporte   |
| 3   | NCF                          | 19       | AN   | Comprobante emitido            |
| 4   | NCF Modificado               | 19       | AN   | Si es nota de crédito          |
| 5   | Tipo Ingreso                 | 2        | N    | 01=Operativo, 02=Financiero... |
| 6   | Fecha Comprobante            | 8        | N    | AAAAMMDD                       |
| 7   | Fecha Retención              | 8        | N    | Si aplica                      |
| 8   | Monto Facturado              | 12       | N    | Sin ITBIS                      |
| 9   | ITBIS Facturado              | 12       | N    | 18%                            |
| 10  | ITBIS Retenido por Terceros  | 12       | N    | Si aplica                      |
| 11  | ITBIS Percibido              | 12       | N    | Si aplica                      |
| 12  | Retención Renta por Terceros | 12       | N    | Si aplica                      |
| 13  | ISR Percibido                | 12       | N    | Si aplica                      |
| 14  | Impuesto Selectivo           | 12       | N    | No aplica a OKLA               |
| 15  | Otros Impuestos              | 12       | N    | Si aplica                      |
| 16  | Monto Propina Legal          | 12       | N    | No aplica                      |
| 17  | Efectivo                     | 12       | N    | Monto pagado en efectivo       |
| 18  | Cheque/Transferencia         | 12       | N    | Monto en cheque                |
| 19  | Tarjeta Débito/Crédito       | 12       | N    | Monto en tarjeta               |
| 20  | Venta a Crédito              | 12       | N    | Monto a crédito                |
| 21  | Bonos o Certificados         | 12       | N    | No aplica                      |
| 22  | Permuta                      | 12       | N    | No aplica                      |
| 23  | Otras Formas Venta           | 12       | N    | Otros medios                   |

### Query de Generación

```sql
-- Generar datos para Formato 607
SELECT
    CASE
        WHEN i.ncf_type = 'B01' THEN REPLACE(c.document_number, '-', '')
        ELSE ''
    END as rnc_cliente,
    CASE
        WHEN i.ncf_type = 'B01' AND c.document_type = 'rnc' THEN '1'
        WHEN i.ncf_type = 'B01' AND c.document_type = 'cedula' THEN '2'
        ELSE ''
    END as tipo_id,
    i.ncf_number as ncf,
    COALESCE(i.ncf_modificado, '') as ncf_modificado,
    '01' as tipo_ingreso,  -- Operativo
    TO_CHAR(i.created_at, 'YYYYMMDD') as fecha,
    '' as fecha_retencion,
    ROUND(i.subtotal * 100) as monto_facturado,
    ROUND(i.itbis_amount * 100) as itbis,
    0 as itbis_retenido,
    0 as itbis_percibido,
    0 as retencion_renta,
    0 as isr_percibido,
    0 as impuesto_selectivo,
    0 as otros_impuestos,
    0 as propina,
    0 as efectivo,
    0 as cheque,
    ROUND(i.total * 100) as tarjeta,  -- Asumimos todo es tarjeta
    0 as credito,
    0 as bonos,
    0 as permuta,
    0 as otras_formas
FROM invoices i
LEFT JOIN customers c ON i.customer_id = c.id
WHERE i.created_at BETWEEN :fecha_inicio AND :fecha_fin
AND i.status = 'completed'
ORDER BY i.created_at;
```

---

## 📄 ESPECIFICACIÓN FORMATO 608

### Estructura del Archivo

```
Nombre: 608RNCAAAAMMDD.txt
```

### Campos del Registro

| #   | Campo           | Longitud | Tipo | Descripción         |
| --- | --------------- | -------- | ---- | ------------------- |
| 1   | NCF Anulado     | 19       | AN   | Comprobante anulado |
| 2   | Tipo NCF        | 2        | AN   | B01, B02, B04, etc. |
| 3   | Fecha Anulación | 8        | N    | AAAAMMDD            |

### Query de Generación

```sql
-- Generar datos para Formato 608
SELECT
    i.ncf_number as ncf_anulado,
    i.ncf_type as tipo_ncf,
    TO_CHAR(i.cancelled_at, 'YYYYMMDD') as fecha_anulacion
FROM invoices i
WHERE i.status = 'cancelled'
AND i.cancelled_at BETWEEN :fecha_inicio AND :fecha_fin
ORDER BY i.cancelled_at;
```

---

## 🔧 ARQUITECTURA DEL SERVICIO

### FiscalReportingService (Puerto 5072)

```
┌─────────────────────────────────────────────────────────────────────────┐
│                      FISCAL REPORTING SERVICE                           │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │                         API LAYER                                │   │
│  │  ├── POST /api/fiscal/reports/generate                          │   │
│  │  ├── GET  /api/fiscal/reports/{id}                              │   │
│  │  ├── GET  /api/fiscal/reports/{type}/{period}                   │   │
│  │  ├── POST /api/fiscal/606/generate                              │   │
│  │  ├── POST /api/fiscal/607/generate                              │   │
│  │  ├── POST /api/fiscal/608/generate                              │   │
│  │  ├── GET  /api/fiscal/calendar                                  │   │
│  │  └── GET  /api/fiscal/obligations                               │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│                              │                                          │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │                      APPLICATION LAYER                           │   │
│  │  ├── Formato606Generator                                        │   │
│  │  ├── Formato607Generator                                        │   │
│  │  ├── Formato608Generator                                        │   │
│  │  ├── ITBISCalculator                                            │   │
│  │  ├── ReportValidator                                            │   │
│  │  └── ReportScheduler                                            │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│                              │                                          │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │                     INFRASTRUCTURE LAYER                         │   │
│  │  ├── BillingServiceClient (obtener facturas)                    │   │
│  │  ├── ExpenseRepository (obtener gastos)                         │   │
│  │  ├── FileGenerator (TXT, Excel, PDF)                            │   │
│  │  ├── S3Storage (guardar reportes)                               │   │
│  │  └── NotificationClient (alertar deadlines)                     │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### Endpoints

| Método | Endpoint                     | Descripción                     |
| ------ | ---------------------------- | ------------------------------- |
| `POST` | `/api/fiscal/606/generate`   | Generar Formato 606             |
| `POST` | `/api/fiscal/607/generate`   | Generar Formato 607             |
| `POST` | `/api/fiscal/608/generate`   | Generar Formato 608             |
| `GET`  | `/api/fiscal/606/{period}`   | Descargar 606 generado          |
| `GET`  | `/api/fiscal/607/{period}`   | Descargar 607 generado          |
| `GET`  | `/api/fiscal/608/{period}`   | Descargar 608 generado          |
| `GET`  | `/api/fiscal/itbis/{period}` | Resumen ITBIS del período       |
| `GET`  | `/api/fiscal/calendar`       | Calendario de obligaciones      |
| `GET`  | `/api/fiscal/upcoming`       | Próximas obligaciones           |
| `POST` | `/api/fiscal/validate`       | Validar reporte antes de enviar |

### Request/Response Examples

**Generar Formato 606:**

```json
// POST /api/fiscal/606/generate
{
  "year": 2026,
  "month": 1,
  "format": "txt",
  "validate": true
}

// Response
{
  "success": true,
  "reportId": "606-2026-01-001",
  "period": "202601",
  "recordCount": 45,
  "totalAmount": 1250000.00,
  "totalItbis": 225000.00,
  "validationErrors": [],
  "downloadUrl": "/api/fiscal/606/202601/download",
  "generatedAt": "2026-02-05T10:30:00Z"
}
```

---

## ⏰ PROGRAMACIÓN AUTOMÁTICA

### Jobs Schedulados

| Job                  | Cron         | Descripción |
| -------------------- | ------------ | ----------- |
| `GenerateFormato606` | `0 6 5 * *`  | Día 5, 6am  |
| `GenerateFormato607` | `0 6 5 * *`  | Día 5, 6am  |
| `GenerateFormato608` | `0 6 5 * *`  | Día 5, 6am  |
| `CalculateITBIS`     | `0 6 16 * *` | Día 16, 6am |
| `AlertDeadlines`     | `0 8 * * *`  | Diario, 8am |
| `AMLDailyReport`     | `0 7 * * *`  | Diario, 7am |
| `WeeklyAuditSummary` | `0 9 * * 1`  | Lunes, 9am  |

### Configuración de Alertas

```yaml
fiscal_alerts:
  formato_606:
    generate_day: 5
    due_day: 15
    alerts:
      - days_before: 10 # Día 5
        action: "generate"
        notify: ["fiscal@okla.com.do"]
      - days_before: 5 # Día 10
        action: "remind"
        notify: ["fiscal@okla.com.do"]
      - days_before: 2 # Día 13
        action: "urgent"
        notify: ["fiscal@okla.com.do", "gerencia@okla.com.do"]
      - days_before: 0 # Día 15
        action: "overdue"
        notify: ["all"]
```

---

## 📊 DASHBOARD DE REPORTES

### UI Propuesta

```
┌─────────────────────────────────────────────────────────────────────────┐
│  📊 CENTRO DE REPORTES FISCALES                                        │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  PERÍODO: [Enero 2026 ▼]                                               │
│                                                                         │
│  REPORTES DGII                                                          │
│  ┌────────────────┬────────────────┬────────────────┬───────────────┐  │
│  │ FORMATO 606    │ FORMATO 607    │ FORMATO 608    │ ITBIS         │  │
│  │ ────────────── │ ────────────── │ ────────────── │ ───────────── │  │
│  │ Estado: 🟢     │ Estado: 🟢     │ Estado: 🟡     │ Estado: 🔴    │  │
│  │ Generado       │ Generado       │ Pendiente      │ No calculado  │  │
│  │                │                │                │               │  │
│  │ 45 registros   │ 128 registros  │ 3 anulaciones  │ ---           │  │
│  │ $1,250,000     │ $3,450,000     │ ---            │ ---           │  │
│  │                │                │                │               │  │
│  │ [📥 Descargar] │ [📥 Descargar] │ [⚙️ Generar]   │ [⚙️ Calcular] │  │
│  │ [👁️ Ver]       │ [👁️ Ver]       │               │               │  │
│  └────────────────┴────────────────┴────────────────┴───────────────┘  │
│                                                                         │
│  PRÓXIMAS FECHAS LÍMITE                                                 │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │ 📅 10 Feb - IR-17 Retenciones        [5 días]  🟡 Preparar     │   │
│  │ 📅 15 Feb - Formatos 606/607/608     [10 días] 🟢 Listo        │   │
│  │ 📅 20 Feb - Declaración ITBIS        [15 días] 🔴 Pendiente    │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│                                                                         │
│  HISTÓRICO DE REPORTES                                                  │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │ Período    │ 606   │ 607   │ 608   │ ITBIS  │ IR-17  │ Enviado │   │
│  │ ────────── │ ───── │ ───── │ ───── │ ────── │ ────── │ ─────── │   │
│  │ Dic 2025   │ N/A   │ N/A   │ N/A   │ N/A    │ N/A    │ N/A     │   │
│  │ Ene 2026   │ ✅    │ ✅    │ 🟡    │ 🔴     │ 🔴     │ ❌      │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 📁 ALMACENAMIENTO DE REPORTES

### Estructura de Archivos

```
/reports/
├── fiscal/
│   ├── 606/
│   │   ├── 2026/
│   │   │   ├── 606-202601.txt
│   │   │   ├── 606-202601.xlsx
│   │   │   ├── 606-202601-validation.json
│   │   │   └── 606-202601-acuse.pdf
│   ├── 607/
│   │   └── ...
│   ├── 608/
│   │   └── ...
│   └── itbis/
│       └── ...
├── aml/
│   ├── alerts/
│   ├── ros/
│   └── kyc/
├── privacy/
│   ├── arco/
│   └── consents/
└── audit/
    ├── executive/
    └── logs/
```

### Retención de Reportes

| Tipo              | Retención | Ubicación     |
| ----------------- | --------- | ------------- |
| Formatos DGII     | 10 años   | S3 Glacier    |
| Acuses DGII       | 10 años   | S3 Glacier    |
| Reportes AML      | 10 años   | S3 Glacier    |
| Reportes Internos | 5 años    | S3 Standard   |
| Logs de Auditoría | 2 años    | Elasticsearch |

---

**Última revisión:** Enero 25, 2026  
**Próxima revisión:** Antes de primera generación de formatos  
**Responsable:** Equipo de Desarrollo + Fiscal
