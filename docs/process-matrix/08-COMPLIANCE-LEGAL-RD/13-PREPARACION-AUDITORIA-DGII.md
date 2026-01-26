# 🔍 Preparación para Auditoría DGII - OKLA S.R.L.

> **Empresa:** OKLA S.R.L.  
> **RNC:** 1-33-32590-1  
> **Registro Mercantil:** 196339PSD  
> **Fecha de Creación:** Enero 25, 2026  
> **Propósito:** Tener toda la información lista para fiscalizaciones DGII

---

## 📋 RESUMEN EJECUTIVO

Este documento establece los procedimientos y la documentación necesaria para responder de forma rápida y completa ante una auditoría o fiscalización de la DGII.

### Objetivo

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    PREPARACIÓN PARA AUDITORÍA                           │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  🎯 OBJETIVO:                                                           │
│  Cuando un auditor de DGII solicite información, poder entregar        │
│  TODA la documentación requerida en MENOS DE 24 HORAS.                  │
│                                                                         │
│  ✅ Documentos organizados por año/mes                                  │
│  ✅ Formatos 606/607/608 con acuses                                     │
│  ✅ Facturas emitidas con NCF                                           │
│  ✅ Facturas recibidas con NCF verificados                              │
│  ✅ Comprobantes de pago                                                │
│  ✅ Estados de cuenta bancarios                                         │
│  ✅ Declaraciones IT-1, IR-17, IR-2                                     │
│  ✅ Libros contables y auxiliares                                       │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 1. DOCUMENTOS QUE DGII PUEDE SOLICITAR

### 1.1 Por Tipo de Fiscalización

```
┌─────────────────────────────────────────────────────────────────────────┐
│              TIPOS DE FISCALIZACIÓN DGII                                │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  📋 VERIFICACIÓN DE OFICIO (Más común)                                  │
│  ───────────────────────────────────────                                │
│  • Verifican consistencia de declaraciones                              │
│  • Pueden pedir: 606, 607, muestras de facturas                        │
│  • Duración: 1-3 días                                                   │
│  • Riesgo: Bajo                                                         │
│                                                                         │
│  📊 FISCALIZACIÓN PARCIAL                                               │
│  ────────────────────────                                               │
│  • Revisan un período específico (ej: últimos 6 meses)                  │
│  • Pueden pedir: Todos los formatos, facturas, bancos                   │
│  • Duración: 1-4 semanas                                                │
│  • Riesgo: Medio                                                        │
│                                                                         │
│  🔍 FISCALIZACIÓN INTEGRAL                                              │
│  ─────────────────────────                                              │
│  • Revisan todo: 3-5 años de operaciones                                │
│  • Piden: TODA la documentación                                         │
│  • Duración: 1-6 meses                                                  │
│  • Riesgo: Alto                                                         │
│                                                                         │
│  ⚠️ INVESTIGACIÓN ESPECIAL                                             │
│  ───────────────────────────                                            │
│  • Por denuncia o inconsistencias graves                                │
│  • Pueden incluir visita sorpresa                                       │
│  • Requiere abogado tributario                                          │
│  • Riesgo: Muy Alto                                                     │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 1.2 Documentos Requeridos por Categoría

#### A. Información General de la Empresa

| Documento                 | Formato | Ubicación                      | Responsable |
| ------------------------- | ------- | ------------------------------ | ----------- |
| Registro Mercantil        | PDF     | S3: /legal/registro-mercantil/ | Legal       |
| Acta Constitutiva         | PDF     | S3: /legal/actas/              | Legal       |
| RNC (Certificado DGII)    | PDF     | S3: /legal/rnc/                | Contador    |
| Estatutos Sociales        | PDF     | S3: /legal/estatutos/          | Legal       |
| Actas de Asambleas        | PDF     | S3: /legal/actas/              | Legal       |
| Poderes de Representación | PDF     | S3: /legal/poderes/            | Legal       |
| Contratos de Servicios    | PDF     | S3: /legal/contratos/          | Legal       |

#### B. Declaraciones Fiscales

| Documento        | Período | Formato | Ubicación                       |
| ---------------- | ------- | ------- | ------------------------------- |
| IT-1 (ITBIS)     | Mensual | PDF     | S3: /dgii-reports/it1/YYYY/MM/  |
| IR-17 (Retenc.)  | Mensual | PDF     | S3: /dgii-reports/ir17/YYYY/MM/ |
| IR-2 (ISR Anual) | Anual   | PDF     | S3: /dgii-reports/ir2/YYYY/     |
| Formato 606      | Mensual | TXT+PDF | S3: /dgii-reports/606/YYYY/MM/  |
| Formato 607      | Mensual | TXT+PDF | S3: /dgii-reports/607/YYYY/MM/  |
| Formato 608      | Mensual | TXT+PDF | S3: /dgii-reports/608/YYYY/MM/  |

#### C. Facturas Emitidas (Ventas)

| Documento            | Formato | Ubicación                          | Retención |
| -------------------- | ------- | ---------------------------------- | --------- |
| Facturas B01         | PDF     | S3: /invoices/emitted/YYYY/MM/B01/ | 10 años   |
| Facturas B02         | PDF     | S3: /invoices/emitted/YYYY/MM/B02/ | 10 años   |
| Notas de Crédito B04 | PDF     | S3: /invoices/emitted/YYYY/MM/B04/ | 10 años   |
| Libro de Ventas      | Excel   | S3: /accounting/sales/YYYY/        | 10 años   |
| Conciliación con 607 | Excel   | S3: /reconciliation/607/YYYY/MM/   | 10 años   |

#### D. Facturas Recibidas (Compras/Gastos)

| Documento                 | Formato | Ubicación                            | Retención |
| ------------------------- | ------- | ------------------------------------ | --------- |
| Facturas de proveedores   | PDF     | S3: /invoices/received/YYYY/MM/      | 10 años   |
| NCF B13 (Internacionales) | PDF     | S3: /invoices/international/YYYY/MM/ | 10 años   |
| Libro de Compras          | Excel   | S3: /accounting/purchases/YYYY/      | 10 años   |
| Conciliación con 606      | Excel   | S3: /reconciliation/606/YYYY/MM/     | 10 años   |
| Verificación NCF          | PDF     | S3: /ncf-verification/YYYY/MM/       | 10 años   |

#### E. Documentos Bancarios

| Documento                | Formato | Ubicación                     | Retención |
| ------------------------ | ------- | ----------------------------- | --------- |
| Estados de cuenta        | PDF     | S3: /banking/statements/YYYY/ | 10 años   |
| Conciliaciones bancarias | Excel   | S3: /banking/recon/YYYY/MM/   | 10 años   |
| Cheques emitidos         | PDF     | S3: /banking/checks/YYYY/     | 10 años   |
| Transferencias           | PDF     | S3: /banking/transfers/YYYY/  | 10 años   |

#### F. Nómina y Empleados

| Documento             | Formato | Ubicación                  | Retención  |
| --------------------- | ------- | -------------------------- | ---------- |
| Nóminas mensuales     | Excel   | S3: /payroll/YYYY/MM/      | 10 años    |
| Comprobantes TSS      | PDF     | S3: /payroll/tss/YYYY/MM/  | 10 años    |
| IR-3 (retenciones)    | PDF     | S3: /payroll/ir3/YYYY/MM/  | 10 años    |
| Contratos laborales   | PDF     | S3: /hr/contracts/         | Indefinido |
| Cálculo regalía/bonos | Excel   | S3: /payroll/bonuses/YYYY/ | 10 años    |

---

## 2. ESTRUCTURA DE ARCHIVOS EN S3

### 2.1 Estructura Completa

```
s3://okla-fiscal/
│
├── 📁 legal/
│   ├── registro-mercantil/
│   │   └── rm-196339psd.pdf
│   ├── actas/
│   │   ├── acta-constitutiva.pdf
│   │   └── asambleas/
│   │       └── 2026/
│   ├── estatutos/
│   │   └── estatutos-vigentes.pdf
│   ├── poderes/
│   │   └── poder-representacion-gerente.pdf
│   └── contratos/
│       ├── hosting/
│       ├── servicios/
│       └── empleados/
│
├── 📁 dgii-reports/
│   ├── 606/
│   │   └── 2026/
│   │       ├── 01/
│   │       │   ├── 606_133325901_202601.txt
│   │       │   ├── 606_133325901_202601_acuse.pdf
│   │       │   └── 606_202601_detalle.xlsx
│   │       ├── 02/
│   │       └── ...
│   ├── 607/
│   │   └── 2026/
│   │       └── ...
│   ├── 608/
│   │   └── 2026/
│   │       └── ...
│   ├── it1/
│   │   └── 2026/
│   │       ├── 01/
│   │       │   ├── it1_202601.pdf
│   │       │   ├── it1_202601_pago.pdf
│   │       │   └── it1_202601_calculo.xlsx
│   │       └── ...
│   ├── ir17/
│   │   └── 2026/
│   │       ├── 01/
│   │       │   ├── ir17_202601.pdf
│   │       │   └── ir17_202601_detalle.xlsx
│   │       └── ...
│   └── ir2/
│       └── 2025/
│           ├── ir2_2025.pdf
│           └── ir2_2025_anexos.xlsx
│
├── 📁 invoices/
│   ├── emitted/
│   │   └── 2026/
│   │       ├── 01/
│   │       │   ├── B01/
│   │       │   │   ├── B0100000001.pdf
│   │       │   │   ├── B0100000002.pdf
│   │       │   │   └── ...
│   │       │   └── B02/
│   │       │       ├── B0200000001.pdf
│   │       │       └── ...
│   │       └── ...
│   ├── received/
│   │   └── 2026/
│   │       ├── 01/
│   │       │   ├── local/
│   │       │   │   ├── contador-b0100000789.pdf
│   │       │   │   ├── claro-b0100098765.pdf
│   │       │   │   └── ...
│   │       │   └── ...
│   │       └── ...
│   └── international/
│       └── 2026/
│           ├── 01/
│           │   ├── digitalocean-inv-001234.pdf
│           │   ├── stripe-inv-jan2026.pdf
│           │   ├── github-inv-202601.pdf
│           │   ├── google-ads-jan2026.pdf
│           │   └── ...
│           └── ...
│
├── 📁 expenses/
│   └── 2026/
│       ├── 01/
│       │   ├── gastos-enero-2026.xlsx
│       │   └── conciliacion-606.xlsx
│       └── ...
│
├── 📁 banking/
│   ├── statements/
│   │   └── 2026/
│   │       ├── popular/
│   │       │   ├── 2026-01.pdf
│   │       │   └── ...
│   │       └── bhd/
│   │           └── ...
│   ├── recon/
│   │   └── 2026/
│   │       ├── 01/
│   │       │   └── conciliacion-enero.xlsx
│   │       └── ...
│   └── transfers/
│       └── 2026/
│           └── ...
│
├── 📁 payroll/
│   ├── 2026/
│   │   ├── 01/
│   │   │   ├── nomina-enero-2026.xlsx
│   │   │   └── recibos/
│   │   └── ...
│   ├── tss/
│   │   └── 2026/
│   │       ├── 01/
│   │       │   └── tss-enero-2026.pdf
│   │       └── ...
│   └── ir3/
│       └── 2026/
│           └── ...
│
├── 📁 accounting/
│   ├── sales/
│   │   └── 2026/
│   │       └── libro-ventas-2026.xlsx
│   ├── purchases/
│   │   └── 2026/
│   │       └── libro-compras-2026.xlsx
│   ├── general-ledger/
│   │   └── 2026/
│   │       └── mayor-general-2026.xlsx
│   └── trial-balance/
│       └── 2026/
│           ├── balance-comprobacion-Q1.xlsx
│           ├── balance-comprobacion-Q2.xlsx
│           └── ...
│
├── 📁 reconciliation/
│   ├── 606/
│   │   └── 2026/
│   │       └── ...
│   ├── 607/
│   │   └── 2026/
│   │       └── ...
│   └── bank/
│       └── 2026/
│           └── ...
│
└── 📁 audit/
    ├── responses/
    │   └── 2026/
    │       └── fiscalizacion-marzo/
    │           ├── requerimiento-dgii.pdf
    │           ├── respuesta-okla.pdf
    │           └── anexos/
    └── internal/
        └── 2026/
            └── auditoria-interna-q1.pdf
```

---

## 3. GENERACIÓN AUTOMÁTICA DE PAQUETE DE AUDITORÍA

### 3.1 API para Generar Paquete

```csharp
// DGIIService.Api/Controllers/AuditPackageController.cs

[ApiController]
[Route("api/audit")]
[Authorize(Roles = "Admin")]
public class AuditPackageController : ControllerBase
{
    private readonly IAuditPackageService _packageService;

    /// <summary>
    /// Generar paquete completo de documentos para auditoría
    /// </summary>
    [HttpPost("generate-package")]
    public async Task<ActionResult<AuditPackageResult>> GeneratePackage(
        [FromBody] AuditPackageRequest request)
    {
        // request.StartDate, request.EndDate, request.Categories

        var result = await _packageService.GenerateAsync(
            request.StartDate,
            request.EndDate,
            request.Categories);

        return Ok(result);
    }

    /// <summary>
    /// Descargar paquete como ZIP
    /// </summary>
    [HttpGet("download/{packageId}")]
    public async Task<IActionResult> DownloadPackage(Guid packageId)
    {
        var package = await _packageService.GetByIdAsync(packageId);

        if (package == null)
            return NotFound();

        // Retornar URL de S3 para descarga directa
        return Ok(new { downloadUrl = package.ZipFileUrl });
    }
}
```

### 3.2 Servicio de Generación de Paquete

```csharp
// DGIIService.Application/Services/AuditPackageService.cs

public class AuditPackageService : IAuditPackageService
{
    private readonly IS3Service _s3;
    private readonly IExpenseRepository _expenses;
    private readonly IInvoiceRepository _invoices;
    private readonly IDGIIFormatRepository _formats;

    public async Task<AuditPackageResult> GenerateAsync(
        DateTime startDate,
        DateTime endDate,
        List<AuditCategory> categories)
    {
        var packageId = Guid.NewGuid();
        var tempDir = Path.Combine(Path.GetTempPath(), packageId.ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            // 1. Información general de empresa
            if (categories.Contains(AuditCategory.CompanyInfo))
            {
                await DownloadCategoryAsync(tempDir, "1-informacion-empresa", new[]
                {
                    "legal/registro-mercantil/",
                    "legal/actas/acta-constitutiva.pdf",
                    "legal/estatutos/"
                });
            }

            // 2. Formatos DGII del período
            if (categories.Contains(AuditCategory.DGIIFormats))
            {
                var formatDir = Path.Combine(tempDir, "2-formatos-dgii");
                Directory.CreateDirectory(formatDir);

                for (var date = startDate; date <= endDate; date = date.AddMonths(1))
                {
                    var monthDir = Path.Combine(formatDir, $"{date:yyyy-MM}");
                    Directory.CreateDirectory(monthDir);

                    // Descargar 606, 607, 608, IT-1, IR-17
                    await DownloadFormatsForMonthAsync(monthDir, date.Year, date.Month);
                }
            }

            // 3. Facturas emitidas
            if (categories.Contains(AuditCategory.SalesInvoices))
            {
                var invoiceDir = Path.Combine(tempDir, "3-facturas-emitidas");
                Directory.CreateDirectory(invoiceDir);

                var invoices = await _invoices.GetByDateRangeAsync(startDate, endDate);

                foreach (var invoice in invoices)
                {
                    var monthDir = Path.Combine(invoiceDir, $"{invoice.IssueDate:yyyy-MM}");
                    Directory.CreateDirectory(monthDir);

                    await _s3.DownloadAsync(invoice.PdfUrl,
                        Path.Combine(monthDir, $"{invoice.NCF}.pdf"));
                }

                // Generar Excel resumen
                await GenerateSalesExcelAsync(invoiceDir, invoices);
            }

            // 4. Facturas recibidas (gastos)
            if (categories.Contains(AuditCategory.ExpenseInvoices))
            {
                var expenseDir = Path.Combine(tempDir, "4-facturas-recibidas");
                Directory.CreateDirectory(expenseDir);

                var expenses = await _expenses.GetByDateRangeAsync(startDate, endDate);

                // Separar locales e internacionales
                var localDir = Path.Combine(expenseDir, "locales");
                var intlDir = Path.Combine(expenseDir, "internacionales");
                Directory.CreateDirectory(localDir);
                Directory.CreateDirectory(intlDir);

                foreach (var expense in expenses)
                {
                    var targetDir = expense.Type == "INTERNATIONAL" ? intlDir : localDir;
                    var monthDir = Path.Combine(targetDir, $"{expense.OriginalInvoiceDate:yyyy-MM}");
                    Directory.CreateDirectory(monthDir);

                    if (!string.IsNullOrEmpty(expense.InvoiceDocumentUrl))
                    {
                        await _s3.DownloadAsync(expense.InvoiceDocumentUrl,
                            Path.Combine(monthDir, $"{expense.ExpenseNumber}.pdf"));
                    }
                }

                // Generar Excel resumen
                await GenerateExpensesExcelAsync(expenseDir, expenses);
            }

            // 5. Estados de cuenta bancarios
            if (categories.Contains(AuditCategory.BankStatements))
            {
                await DownloadCategoryAsync(tempDir, "5-estados-cuenta-banco",
                    GetBankingPaths(startDate, endDate));
            }

            // 6. Nómina
            if (categories.Contains(AuditCategory.Payroll))
            {
                await DownloadCategoryAsync(tempDir, "6-nomina",
                    GetPayrollPaths(startDate, endDate));
            }

            // 7. Generar índice
            await GenerateIndexAsync(tempDir, startDate, endDate);

            // 8. Crear ZIP
            var zipPath = Path.Combine(Path.GetTempPath(), $"auditoria-okla-{startDate:yyyyMM}-{endDate:yyyyMM}.zip");
            ZipFile.CreateFromDirectory(tempDir, zipPath);

            // 9. Subir ZIP a S3
            var zipKey = $"audit/packages/{packageId}/auditoria-okla-{startDate:yyyyMM}-{endDate:yyyyMM}.zip";
            var zipUrl = await _s3.UploadFileAsync(zipKey, zipPath);

            // 10. Registrar paquete
            var package = new AuditPackage
            {
                Id = packageId,
                StartDate = startDate,
                EndDate = endDate,
                Categories = categories,
                ZipFileUrl = zipUrl,
                FileSizeBytes = new FileInfo(zipPath).Length,
                GeneratedAt = DateTime.UtcNow,
                GeneratedBy = _currentUser.Id
            };

            await _packageRepository.AddAsync(package);

            return new AuditPackageResult
            {
                Success = true,
                PackageId = packageId,
                DownloadUrl = zipUrl,
                FileSizeMB = package.FileSizeBytes / 1024.0 / 1024.0,
                DocumentCount = CountDocuments(tempDir)
            };
        }
        finally
        {
            // Limpiar directorio temporal
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    private async Task GenerateIndexAsync(string dir, DateTime start, DateTime end)
    {
        var indexPath = Path.Combine(dir, "INDICE-DOCUMENTOS.xlsx");

        using var workbook = new XLWorkbook();
        var sheet = workbook.AddWorksheet("Índice");

        sheet.Cell(1, 1).Value = "PAQUETE DE AUDITORÍA - OKLA S.R.L.";
        sheet.Cell(2, 1).Value = $"RNC: 1-33-32590-1";
        sheet.Cell(3, 1).Value = $"Período: {start:MMMM yyyy} - {end:MMMM yyyy}";
        sheet.Cell(4, 1).Value = $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}";

        var row = 6;
        sheet.Cell(row, 1).Value = "Carpeta";
        sheet.Cell(row, 2).Value = "Descripción";
        sheet.Cell(row, 3).Value = "Documentos";

        // Listar contenido de cada carpeta
        foreach (var folder in Directory.GetDirectories(dir))
        {
            row++;
            var folderName = Path.GetFileName(folder);
            var fileCount = Directory.GetFiles(folder, "*", SearchOption.AllDirectories).Length;

            sheet.Cell(row, 1).Value = folderName;
            sheet.Cell(row, 2).Value = GetFolderDescription(folderName);
            sheet.Cell(row, 3).Value = fileCount;
        }

        workbook.SaveAs(indexPath);
    }
}
```

---

## 4. CHECKLIST DE AUDITORÍA

### 4.1 Checklist Pre-Auditoría (Mensual)

```markdown
## Checklist Mensual - Preparación para Auditoría

### Documentos del Mes

- [ ] Formato 606 generado y enviado
- [ ] Formato 607 generado y enviado
- [ ] Formato 608 generado y enviado (si hay anulaciones)
- [ ] IT-1 presentado y pagado
- [ ] IR-17 presentado y pagado
- [ ] Acuses de DGII archivados en S3

### Facturas Emitidas

- [ ] Todas las facturas tienen NCF
- [ ] PDFs archivados en S3
- [ ] Libro de ventas actualizado
- [ ] Conciliación con 607 realizada

### Facturas Recibidas

- [ ] Todas las facturas locales tienen NCF
- [ ] NCF verificados en DGII
- [ ] Invoices internacionales archivados
- [ ] Libro de compras actualizado
- [ ] Conciliación con 606 realizada

### Bancos

- [ ] Estados de cuenta descargados
- [ ] Conciliación bancaria realizada
- [ ] Comprobantes de transferencia archivados

### Nómina

- [ ] Nómina del mes archivada
- [ ] TSS pagado y comprobante archivado
- [ ] IR-3 incluido en IR-17

### Verificación

- [ ] Totales de 606 = Libro de compras
- [ ] Totales de 607 = Libro de ventas
- [ ] ITBIS calculado coincide con IT-1
- [ ] Retenciones IR-17 = Suma de retenciones
```

### 4.2 Checklist Anual (Cierre Fiscal)

```markdown
## Checklist Anual - Cierre Fiscal

### Antes del 30 de Abril

- [ ] Todos los meses tienen 606/607/608 enviados
- [ ] Todos los IT-1 presentados y pagados
- [ ] Todos los IR-17 presentados y pagados
- [ ] Estados financieros preparados
- [ ] IR-2 preparado y presentado

### Documentación Anual

- [ ] Libro mayor general completo
- [ ] Balance de comprobación
- [ ] Estado de resultados
- [ ] Balance general
- [ ] Notas a los estados financieros

### Archivo

- [ ] Todos los documentos en S3
- [ ] Backup anual realizado
- [ ] Índice de documentos actualizado
```

---

## 5. RESPUESTA A REQUERIMIENTOS DGII

### 5.1 Proceso de Respuesta

```
┌─────────────────────────────────────────────────────────────────────────┐
│              PROCESO DE RESPUESTA A DGII                                │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  1️⃣ RECEPCIÓN DEL REQUERIMIENTO                                         │
│     ├── Vía: Correo certificado, notificación digital, visita          │
│     ├── Acción: Registrar fecha de recepción                           │
│     ├── Plazo típico: 5-15 días hábiles                                │
│     └── Notificar a: Gerente, Contador, Abogado                        │
│                                                                         │
│  2️⃣ ANÁLISIS DEL REQUERIMIENTO                                          │
│     ├── Identificar documentos solicitados                             │
│     ├── Identificar período requerido                                   │
│     ├── Verificar que tenemos toda la documentación                    │
│     └── Si falta algo: Buscar inmediatamente                           │
│                                                                         │
│  3️⃣ GENERACIÓN DE PAQUETE                                               │
│     ├── Usar API: POST /api/audit/generate-package                      │
│     ├── Seleccionar categorías requeridas                              │
│     ├── Seleccionar período                                            │
│     └── Descargar ZIP                                                  │
│                                                                         │
│  4️⃣ PREPARACIÓN DE RESPUESTA                                            │
│     ├── Carta de respuesta formal                                       │
│     ├── Índice de documentos entregados                                │
│     ├── Copia del requerimiento                                        │
│     └── Revisión por abogado (si es fiscalización grande)              │
│                                                                         │
│  5️⃣ ENTREGA                                                             │
│     ├── Preferible: Entrega física con acuse de recibo                 │
│     ├── Alternativa: Correo certificado                                │
│     ├── Guardar copia de todo lo entregado                             │
│     └── Registrar fecha de entrega                                     │
│                                                                         │
│  6️⃣ SEGUIMIENTO                                                         │
│     ├── Esperar respuesta de DGII                                       │
│     ├── Si piden más documentos: Repetir proceso                       │
│     ├── Si hay observaciones: Analizar y responder                     │
│     └── Guardar todo en /audit/responses/                              │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 5.2 Plantilla de Carta de Respuesta

```
Santo Domingo, [FECHA]

Señores
Dirección General de Impuestos Internos
Departamento de Fiscalización
Ciudad.-

REF: Respuesta a Requerimiento No. [NÚMERO]
     Expediente: [NÚMERO]
     Contribuyente: OKLA S.R.L.
     RNC: 1-33-32590-1

Distinguidos señores:

En atención a su requerimiento de fecha [FECHA REQUERIMIENTO],
recibido en nuestras oficinas el día [FECHA RECEPCIÓN], mediante
el cual solicitan documentación correspondiente al período
[PERÍODO], procedemos a entregar los siguientes documentos:

1. INFORMACIÓN GENERAL
   - Registro Mercantil (1 documento)
   - Acta Constitutiva (1 documento)

2. DECLARACIONES DGII
   - Formatos 606 del período (X documentos)
   - Formatos 607 del período (X documentos)
   - IT-1 del período (X documentos)
   - IR-17 del período (X documentos)

3. FACTURAS EMITIDAS
   - Facturas B01 (X documentos)
   - Facturas B02 (X documentos)
   - Libro de ventas (1 documento)

4. FACTURAS RECIBIDAS
   - Facturas locales (X documentos)
   - Invoices internacionales (X documentos)
   - Libro de compras (1 documento)

5. DOCUMENTOS BANCARIOS
   - Estados de cuenta (X documentos)
   - Conciliaciones bancarias (X documentos)

TOTAL DE DOCUMENTOS ENTREGADOS: XXX

Quedamos a su disposición para cualquier aclaración adicional
que requieran.

Atentamente,

________________________
[NOMBRE]
Gerente General
OKLA S.R.L.
RNC: 1-33-32590-1
Tel: [TELÉFONO]
Email: legal@okla.com.do
```

---

## 6. REPORTES DE AUDITORÍA INTERNA

### 6.1 Reporte Mensual de Cumplimiento

```csharp
// DGIIService.Application/Services/ComplianceReportService.cs

public async Task<ComplianceReport> GenerateMonthlyReportAsync(int year, int month)
{
    var report = new ComplianceReport
    {
        Period = new DateTime(year, month, 1),
        GeneratedAt = DateTime.UtcNow
    };

    // 1. Estado de formatos DGII
    report.Format606 = await CheckFormatStatusAsync("606", year, month);
    report.Format607 = await CheckFormatStatusAsync("607", year, month);
    report.Format608 = await CheckFormatStatusAsync("608", year, month);
    report.IT1 = await CheckFormatStatusAsync("IT1", year, month);
    report.IR17 = await CheckFormatStatusAsync("IR17", year, month);

    // 2. Verificar NCF
    report.NCFStatus = await CheckNCFSequencesAsync();

    // 3. Verificar documentación
    report.DocumentationStatus = new DocumentationStatus
    {
        SalesInvoicesCount = await _invoices.CountByPeriodAsync(year, month),
        SalesInvoicesWithPDF = await _invoices.CountWithPDFAsync(year, month),
        ExpensesCount = await _expenses.CountByPeriodAsync(year, month),
        ExpensesWithDocuments = await _expenses.CountWithDocumentsAsync(year, month),
        ExpensesWithNCFVerified = await _expenses.CountWithNCFVerifiedAsync(year, month)
    };

    // 4. Calcular score de cumplimiento
    report.ComplianceScore = CalculateScore(report);

    // 5. Generar alertas
    report.Alerts = GenerateAlerts(report);

    return report;
}

private decimal CalculateScore(ComplianceReport report)
{
    var scores = new List<decimal>();

    // Formatos DGII (40% del score)
    if (report.Format606.IsSubmitted) scores.Add(8);
    if (report.Format607.IsSubmitted) scores.Add(8);
    if (report.Format608.IsSubmitted) scores.Add(8);
    if (report.IT1.IsSubmitted) scores.Add(8);
    if (report.IR17.IsSubmitted) scores.Add(8);

    // Documentación (40% del score)
    var docScore = (report.DocumentationStatus.SalesInvoicesWithPDF * 1.0m /
                   report.DocumentationStatus.SalesInvoicesCount) * 20;
    scores.Add(docScore);

    var expenseDocScore = (report.DocumentationStatus.ExpensesWithDocuments * 1.0m /
                          report.DocumentationStatus.ExpensesCount) * 20;
    scores.Add(expenseDocScore);

    // NCF (20% del score)
    if (report.NCFStatus.AllSequencesActive) scores.Add(10);
    if (report.NCFStatus.MinimumRemaining > 100) scores.Add(10);

    return scores.Sum();
}
```

### 6.2 Dashboard de Cumplimiento

```typescript
// frontend/web/src/pages/admin/ComplianceDashboard.tsx

export const ComplianceDashboard = () => {
  const { data: report } = useQuery({
    queryKey: ['compliance-report'],
    queryFn: () => complianceService.getMonthlyReport()
  });

  return (
    <div className="p-6 space-y-6">
      <h1 className="text-3xl font-bold">Cumplimiento Fiscal</h1>

      {/* Score de Cumplimiento */}
      <Card>
        <CardHeader>
          <CardTitle>Score de Cumplimiento</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex items-center justify-center">
            <CircularProgress
              value={report?.complianceScore || 0}
              maxValue={100}
              size={200}
              strokeWidth={20}
              color={getScoreColor(report?.complianceScore)}
            />
          </div>
          <p className="text-center mt-4 text-lg">
            {getScoreLabel(report?.complianceScore)}
          </p>
        </CardContent>
      </Card>

      {/* Alertas */}
      {report?.alerts?.length > 0 && (
        <Card className="border-red-200">
          <CardHeader>
            <CardTitle className="text-red-600">
              ⚠️ Alertas de Cumplimiento
            </CardTitle>
          </CardHeader>
          <CardContent>
            <ul className="space-y-2">
              {report.alerts.map((alert, i) => (
                <li key={i} className="flex items-start gap-2">
                  <AlertTriangle className="h-5 w-5 text-red-500 mt-0.5" />
                  <span>{alert.message}</span>
                </li>
              ))}
            </ul>
          </CardContent>
        </Card>
      )}

      {/* Estado de Formatos */}
      <Card>
        <CardHeader>
          <CardTitle>Estado de Formatos DGII</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-5 gap-4">
            <FormatStatus name="606" status={report?.format606} />
            <FormatStatus name="607" status={report?.format607} />
            <FormatStatus name="608" status={report?.format608} />
            <FormatStatus name="IT-1" status={report?.it1} />
            <FormatStatus name="IR-17" status={report?.ir17} />
          </div>
        </CardContent>
      </Card>

      {/* Documentación */}
      <Card>
        <CardHeader>
          <CardTitle>Estado de Documentación</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="space-y-4">
            <ProgressBar
              label="Facturas emitidas con PDF"
              current={report?.documentationStatus.salesInvoicesWithPDF}
              total={report?.documentationStatus.salesInvoicesCount}
            />
            <ProgressBar
              label="Gastos con documentos"
              current={report?.documentationStatus.expensesWithDocuments}
              total={report?.documentationStatus.expensesCount}
            />
            <ProgressBar
              label="NCF verificados"
              current={report?.documentationStatus.expensesWithNCFVerified}
              total={report?.documentationStatus.expensesCount}
            />
          </div>
        </CardContent>
      </Card>
    </div>
  );
};
```

---

## 7. CONTACTOS DE EMERGENCIA

### 7.1 Equipo de Respuesta

| Rol             | Nombre                   | Teléfono     | Email                |
| --------------- | ------------------------ | ------------ | -------------------- |
| Gerente General | Nicauris Mateo Alcántara | 809-XXX-XXXX | gerencia@okla.com.do |
| Socio           | Gregory Moreno           | 809-XXX-XXXX | gmoreno@okla.com.do  |
| Contador        | [Por asignar]            | 809-XXX-XXXX | contador@okla.com.do |
| Abogado Fiscal  | [Por asignar]            | 809-XXX-XXXX | legal@okla.com.do    |
| Soporte Técnico | [Por asignar]            | 809-XXX-XXXX | soporte@okla.com.do  |

### 7.2 Contactos DGII

| Oficina            | Teléfono     | Dirección                        |
| ------------------ | ------------ | -------------------------------- |
| Centro de Atención | 809-689-3444 | Av. México esq. Leopoldo Navarro |
| Oficina Virtual    | N/A          | https://dgii.gov.do              |
| Fiscalización      | 809-689-3444 | Ext. XXX                         |

---

## 📋 RESUMEN

### Documentos Críticos para Auditoría

1. ✅ Formatos 606, 607, 608 con acuses
2. ✅ Declaraciones IT-1 e IR-17 con pagos
3. ✅ Facturas emitidas (PDF con NCF)
4. ✅ Facturas recibidas (locales e internacionales)
5. ✅ Estados de cuenta bancarios
6. ✅ Conciliaciones bancarias
7. ✅ Nóminas y TSS
8. ✅ Libros de ventas y compras
9. ✅ Documentos legales de la empresa

### Tiempo de Respuesta

| Tipo de Solicitud  | Tiempo Máximo |
| ------------------ | ------------- |
| Información básica | 4 horas       |
| Paquete de un mes  | 24 horas      |
| Paquete de un año  | 48 horas      |
| Paquete multi-año  | 72 horas      |

### Sistema Automatizado

- ✅ Generación automática de paquete de auditoría
- ✅ Índice de documentos
- ✅ Score de cumplimiento
- ✅ Alertas de documentos faltantes
- ✅ Dashboard de estado

---

**Documento creado:** Enero 25, 2026  
**Próxima revisión:** Trimestral  
**Responsable:** Gerente General + Contador
