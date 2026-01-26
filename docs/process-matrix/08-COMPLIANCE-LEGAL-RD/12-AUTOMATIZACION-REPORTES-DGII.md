# 🤖 Automatización de Reportes DGII - OKLA S.R.L.

> **Empresa:** OKLA S.R.L.  
> **RNC:** 1-33-32590-1  
> **Fecha de Creación:** Enero 25, 2026  
> **Actualización:** Enero 25, 2026 (Integración e-CF)  
> **Propósito:** Especificaciones técnicas para automatizar reportes fiscales con Facturación Electrónica

---

## 📋 RESUMEN EJECUTIVO

Este documento define la arquitectura técnica para automatizar la generación y **ENVÍO AUTOMÁTICO** de reportes fiscales a la DGII, aprovechando la **Facturación Electrónica (e-CF)** para eliminar casi completamente el trabajo manual.

### Objetivo

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    AUTOMATIZACIÓN CON e-CF                              │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  ANTES (NCF Manual):                  AHORA (e-CF Automático):          │
│  ════════════════════                 ════════════════════════          │
│                                                                         │
│  • Excel manual                    →  • Sistema genera automáticamente  │
│  • Contador ingresa datos          →  • Datos ya en base de datos       │
│  • Error humano frecuente          →  • Validaciones automáticas        │
│  • Subir archivos a Oficina Virtual→  • ✅ ENVÍO AUTOMÁTICO A DGII      │
│  • Formato 607 manual              →  • ✅ SE GENERA DE e-CF EMITIDOS   │
│  • Formato 606 manual              →  • ✅ ENVÍO ELECTRÓNICO            │
│  • IT-1 manual                     →  • ✅ PRE-LLENADO DESDE e-CF       │
│  • IR-17 manual                    →  • ✅ ENVÍO ELECTRÓNICO            │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 🚀 REPORTES CON ENVÍO AUTOMÁTICO A DGII (e-CF)

### Reportes que se Envían Automáticamente

```
┌─────────────────────────────────────────────────────────────────────────┐
│          REPORTES CON TRANSMISIÓN AUTOMÁTICA A DGII                     │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  ✅ ENVÍO EN TIEMPO REAL (Cada Transacción)                             │
│  ──────────────────────────────────────────                             │
│                                                                         │
│  📄 e-CF Emitidos (E31, E32, E34)                                       │
│     └── Cada factura se envía a DGII en < 5 segundos                   │
│     └── DGII valida y responde con TrackingNumber                      │
│     └── Sin intervención humana                                        │
│                                                                         │
│  ✅ GENERACIÓN AUTOMÁTICA DESDE e-CF (Mensual)                          │
│  ─────────────────────────────────────────────                          │
│                                                                         │
│  📊 Formato 607 (Ventas y Operaciones)                                  │
│     └── Se genera AUTOMÁTICAMENTE de e-CF emitidos                     │
│     └── NO requiere acción manual                                      │
│     └── DGII ya tiene los datos de cada e-CF                           │
│     └── Solo validar que el resumen coincida                           │
│                                                                         │
│  📊 Formato 608 (e-CF Anulados)                                         │
│     └── Se genera de notas de crédito (E34) emitidas                   │
│     └── Automático desde tabla de anulaciones                          │
│                                                                         │
│  ✅ ENVÍO ELECTRÓNICO (Mensual - Semi-automático)                       │
│  ─────────────────────────────────────────────────                      │
│                                                                         │
│  📊 Formato 606 (Compras y Gastos)                                      │
│     └── Sistema genera archivo TXT automáticamente                     │
│     └── Envío vía Web Service DGII o carga a OFV                       │
│     └── Validación previa automática                                   │
│                                                                         │
│  📊 IT-1 (ITBIS Mensual)                                                │
│     └── Pre-llenado desde datos de e-CF + Formato 606                  │
│     └── Envío electrónico a DGII                                       │
│     └── Cálculo automático: ITBIS cobrado - ITBIS pagado               │
│                                                                         │
│  📊 IR-17 (Retenciones ISR)                                             │
│     └── Generado de gastos con retención                               │
│     └── Envío electrónico a DGII                                       │
│                                                                         │
│  📊 Formato 609 (Compras del Exterior)                                  │
│     └── Generado de gastos internacionales                             │
│     └── Envío electrónico a DGII                                       │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### Flujo de Envío Automático

```
┌─────────────────────────────────────────────────────────────────────────┐
│              FLUJO: VENTA → e-CF → DGII (AUTOMÁTICO)                    │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  1️⃣ CLIENTE PAGA EN OKLA                                                │
│     └── Stripe/AZUL procesa pago                                       │
│     └── BillingService registra payment                                │
│                                                                         │
│                        ▼ (Automático)                                   │
│                                                                         │
│  2️⃣ ECFService GENERA e-CF                                              │
│     └── Crea XML firmado digitalmente                                  │
│     └── Tipo E31 (RNC) o E32 (consumidor)                              │
│                                                                         │
│                        ▼ (Automático)                                   │
│                                                                         │
│  3️⃣ TRANSMISIÓN A DGII EN TIEMPO REAL                                   │
│     └── POST a ecf.dgii.gov.do/recepcion                               │
│     └── DGII valida en < 3 segundos                                    │
│     └── Respuesta: Aceptado/Rechazado + TrackingNumber                 │
│                                                                         │
│                        ▼ (Automático)                                   │
│                                                                         │
│  4️⃣ REGISTRO EN BASE DE DATOS                                           │
│     └── Guardar e-CF con estado y tracking                             │
│     └── Generar PDF con QR                                             │
│     └── Enviar al cliente por email                                    │
│                                                                         │
│                        ▼ (Fin de mes)                                   │
│                                                                         │
│  5️⃣ DGII YA TIENE FORMATO 607                                           │
│     └── DGII consolida todos los e-CF del mes                          │
│     └── Formato 607 se genera automáticamente en OFV                   │
│     └── Solo verificar y confirmar                                     │
│                                                                         │
│  ⏱️ TIEMPO TOTAL HUMANO: 0 segundos por transacción                     │
│  ⏱️ FIN DE MES: ~10 minutos para verificar resúmenes                    │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

```

---

## 1. ARQUITECTURA DEL SISTEMA

### 1.1 Componentes

```

┌─────────────────────────────────────────────────────────────────────────┐
│ ARQUITECTURA DGII REPORTING │
├─────────────────────────────────────────────────────────────────────────┤
│ │
│ ┌─────────────────────────────────────────────────────────────────┐ │
│ │ FRONTEND (Admin Dashboard) │ │
│ │ • Dashboard fiscal mensual │ │
│ │ • Formularios de gastos/ingresos │ │
│ │ • Vista de reportes generados │ │
│ │ • Alertas y calendario │ │
│ └─────────────────────────────────────────────────────────────────┘ │
│ │ │
│ ▼ │
│ ┌─────────────────────────────────────────────────────────────────┐ │
│ │ API GATEWAY (Ocelot) │ │
│ └─────────────────────────────────────────────────────────────────┘ │
│ │ │
│ ┌───────────────────┼───────────────────┐ │
│ ▼ ▼ ▼ │
│ ┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐ │
│ │ BillingService │ │ ExpenseService │ │ DGIIService │ │
│ │ (Ingresos/NCF) │ │ (Gastos) │ │ (Reportes) │ │
│ │ │ │ │ │ │ │
│ │ • Facturas │ │ • Proveedores │ │ • Formato 606 │ │
│ │ • NCF B01/B02 │ │ • Gastos │ │ • Formato 607 │ │
│ │ • Notas crédito │ │ • Documentos │ │ • Formato 608 │ │
│ │ • Suscripciones │ │ • Retenciones │ │ • IR-17 │ │
│ └────────┬────────┘ └────────┬────────┘ │ • IT-1 │ │
│ │ │ │ • Validaciones │ │
│ │ │ └────────┬────────┘ │
│ │ │ │ │
│ └───────────────────┴───────────────────┘ │
│ │ │
│ ▼ │
│ ┌─────────────────────────────────────────────────────────────────┐ │
│ │ PostgreSQL Database │ │
│ │ • ncf_sequences • ncf_issued • ncf_received │ │
│ │ • expenses • expense_providers • dgii_formats │ │
│ │ • invoices • subscriptions • audit_log │ │
│ └─────────────────────────────────────────────────────────────────┘ │
│ │ │
│ ▼ │
│ ┌─────────────────────────────────────────────────────────────────┐ │
│ │ S3 (Digital Ocean Spaces) │ │
│ │ • /invoices/ • /expenses/ • /dgii-reports/ │ │
│ │ • /ncf-documents/ • /audit-files/ • /backups/ │ │
│ └─────────────────────────────────────────────────────────────────┘ │
│ │
└─────────────────────────────────────────────────────────────────────────┘

```

### 1.2 Microservicio: DGIIService

```

DGIIService/
├── DGIIService.Api/
│ ├── Controllers/
│ │ ├── Format606Controller.cs
│ │ ├── Format607Controller.cs
│ │ ├── Format608Controller.cs
│ │ ├── IR17Controller.cs
│ │ ├── IT1Controller.cs
│ │ ├── NCFController.cs
│ │ └── ReportsController.cs
│ ├── Program.cs
│ └── Dockerfile
├── DGIIService.Application/
│ ├── Features/
│ │ ├── Format606/
│ │ │ ├── GenerateFormat606Command.cs
│ │ │ ├── ValidateFormat606Query.cs
│ │ │ └── Format606Dto.cs
│ │ ├── Format607/
│ │ ├── Format608/
│ │ ├── IR17/
│ │ └── NCF/
│ ├── Services/
│ │ ├── NCFGeneratorService.cs
│ │ ├── Format606GeneratorService.cs
│ │ ├── Format607GeneratorService.cs
│ │ ├── ITBISCalculatorService.cs
│ │ └── DGIIValidatorService.cs
│ └── DTOs/
├── DGIIService.Domain/
│ ├── Entities/
│ │ ├── NCFSequence.cs
│ │ ├── NCFIssued.cs
│ │ ├── NCFReceived.cs
│ │ ├── DGIIFormat.cs
│ │ └── FiscalPeriod.cs
│ ├── Enums/
│ │ ├── NCFType.cs
│ │ ├── ExpenseType.cs
│ │ └── FormatStatus.cs
│ └── Interfaces/
└── DGIIService.Infrastructure/
├── Persistence/
│ ├── DGIIDbContext.cs
│ └── Repositories/
├── External/
│ └── DGIIApiClient.cs
└── FileGenerators/
├── Format606FileGenerator.cs
├── Format607FileGenerator.cs
└── Format608FileGenerator.cs

```

---

## 2. GENERACIÓN AUTOMÁTICA DE FORMATO 606

### 2.1 Flujo de Generación

```

┌─────────────────────────────────────────────────────────────────────────┐
│ FLUJO: GENERACIÓN FORMATO 606 │
├─────────────────────────────────────────────────────────────────────────┤
│ │
│ 1️⃣ TRIGGER │
│ ├── Manual: Admin clickea "Generar 606" │
│ ├── Automático: Job scheduled día 10 del mes │
│ └── API: POST /api/dgii/606/generate │
│ │
│ 2️⃣ RECOLECCIÓN DE DATOS │
│ ├── Query gastos del período con status = APPROVED │
│ ├── Filtrar por fiscal_year y fiscal_month │
│ ├── Incluir gastos locales e internacionales │
│ └── Excluir nómina (va en TSS/IR-17) │
│ │
│ 3️⃣ TRANSFORMACIÓN │
│ ├── Convertir cada gasto a línea 606 │
│ ├── Aplicar códigos de tipo de gasto │
│ ├── Formatear RNC (con ceros a la izquierda) │
│ ├── Formatear fechas (YYYYMMDD) │
│ └── Calcular totales │
│ │
│ 4️⃣ VALIDACIÓN │
│ ├── Verificar estructura del archivo │
│ ├── Validar NCF format (11 caracteres) │
│ ├── Verificar montos no negativos │
│ └── Comparar con validador DGII │
│ │
│ 5️⃣ GENERACIÓN DE ARCHIVO │
│ ├── Crear archivo TXT con formato DGII │
│ ├── Nombre: 606_RNC_YYYYMM.txt │
│ ├── Encoding: UTF-8 sin BOM │
│ └── Separador: pipe (|) │
│ │
│ 6️⃣ ALMACENAMIENTO │
│ ├── Subir a S3: /dgii-reports/606/2026/01/ │
│ ├── Guardar registro en dgii_formats │
│ ├── Marcar gastos como included_in_606 = true │
│ └── Enviar notificación a admin │
│ │
└─────────────────────────────────────────────────────────────────────────┘

````

### 2.2 Implementación C#

```csharp
// DGIIService.Application/Services/Format606GeneratorService.cs

public class Format606GeneratorService : IFormat606GeneratorService
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly IProviderRepository _providerRepository;
    private readonly IS3Service _s3Service;
    private readonly ILogger<Format606GeneratorService> _logger;

    private const string OKLA_RNC = "133325901"; // Sin guiones

    public async Task<Format606Result> GenerateAsync(int year, int month)
    {
        _logger.LogInformation("Generating Format 606 for {Year}-{Month}", year, month);

        // 1. Obtener gastos aprobados del período
        var expenses = await _expenseRepository.GetApprovedByPeriodAsync(year, month);

        if (!expenses.Any())
        {
            return new Format606Result { Success = false, Message = "No hay gastos aprobados para este período" };
        }

        // 2. Generar líneas del 606
        var lines = new List<Format606Line>();

        foreach (var expense in expenses)
        {
            var provider = await _providerRepository.GetByIdAsync(expense.ProviderId);
            var line = MapToFormat606Line(expense, provider);
            lines.Add(line);
        }

        // 3. Generar encabezado
        var header = GenerateHeader(year, month, lines.Count);

        // 4. Generar contenido del archivo
        var fileContent = GenerateFileContent(header, lines);

        // 5. Validar formato
        var validationResult = ValidateFormat(fileContent);
        if (!validationResult.IsValid)
        {
            return new Format606Result { Success = false, Errors = validationResult.Errors };
        }

        // 6. Subir a S3
        var fileName = $"606_{OKLA_RNC}_{year}{month:D2}.txt";
        var s3Key = $"dgii-reports/606/{year}/{month:D2}/{fileName}";
        var fileUrl = await _s3Service.UploadAsync(s3Key, fileContent, "text/plain");

        // 7. Guardar registro
        var formatRecord = new DGIIFormat
        {
            FormatType = "606",
            PeriodMonth = month,
            PeriodYear = year,
            FileContent = fileContent,
            FileUrl = fileUrl,
            RecordCount = lines.Count,
            TotalServices = lines.Sum(l => l.ServicesAmount),
            TotalGoods = lines.Sum(l => l.GoodsAmount),
            TotalITBIS = lines.Sum(l => l.ITBISAmount),
            TotalISRWithheld = lines.Sum(l => l.ISRWithheld),
            GeneratedAt = DateTime.UtcNow,
            Status = FormatStatus.Generated
        };

        await _formatRepository.AddAsync(formatRecord);

        // 8. Marcar gastos como incluidos
        foreach (var expense in expenses)
        {
            expense.IncludedIn606 = true;
            expense.Format606Id = formatRecord.Id;
        }
        await _expenseRepository.UpdateRangeAsync(expenses);

        return new Format606Result
        {
            Success = true,
            FileUrl = fileUrl,
            RecordCount = lines.Count,
            TotalAmount = lines.Sum(l => l.TotalAmount),
            TotalITBIS = lines.Sum(l => l.ITBISAmount)
        };
    }

    private Format606Line MapToFormat606Line(Expense expense, ExpenseProvider provider)
    {
        return new Format606Line
        {
            // Campo 1: RNC/Cédula (0 si internacional)
            RNCCedula = provider.ProviderType == "INTERNATIONAL" ? "0" : FormatRNC(provider.RNC),

            // Campo 2: Tipo de Identificación
            IdentificationType = GetIdentificationType(provider),

            // Campo 3: Tipo de Gasto
            ExpenseType = expense.ExpenseType,

            // Campo 4: NCF
            NCF = expense.NCFReceived ?? expense.NCFInternal,

            // Campo 5: NCF Modificado (vacío generalmente)
            NCFModified = "",

            // Campo 6: Fecha de Comprobante
            InvoiceDate = expense.OriginalInvoiceDate.ToString("yyyyMMdd"),

            // Campo 7: Fecha de Pago
            PaymentDate = expense.PaymentDate?.ToString("yyyyMMdd") ?? expense.OriginalInvoiceDate.ToString("yyyyMMdd"),

            // Campo 8: Monto de Servicios
            ServicesAmount = expense.ExpenseType != "09" && expense.ExpenseType != "10"
                ? expense.Subtotal : 0,

            // Campo 9: Monto de Bienes
            GoodsAmount = expense.ExpenseType == "09" || expense.ExpenseType == "10"
                ? expense.Subtotal : 0,

            // Campo 10: Monto Total
            TotalAmount = expense.Subtotal,

            // Campo 11: ITBIS Facturado
            ITBISAmount = expense.ITBISAmount,

            // Campo 12-15: ITBIS adicionales (generalmente 0)
            ITBISWithheld = expense.ITBISWithheld,
            ITBISProportionality = 0,
            ITBISToCost = 0,
            ITBISAdvanced = 0,

            // Campo 16: ISR Retenido
            ISRWithheld = expense.ISRWithheld,

            // Campo 17: Forma de Pago
            PaymentMethod = expense.PaymentMethod ?? "03" // Default: Tarjeta
        };
    }

    private string GenerateFileContent(Format606Header header, List<Format606Line> lines)
    {
        var sb = new StringBuilder();

        // Línea de encabezado
        sb.AppendLine($"606|{header.RNC}|{header.Period}|{header.RecordCount}");

        // Líneas de detalle
        foreach (var line in lines)
        {
            sb.AppendLine(FormatLine(line));
        }

        return sb.ToString();
    }

    private string FormatLine(Format606Line line)
    {
        return string.Join("|",
            line.RNCCedula,
            line.IdentificationType,
            line.ExpenseType,
            line.NCF,
            line.NCFModified,
            line.InvoiceDate,
            line.PaymentDate,
            FormatAmount(line.ServicesAmount),
            FormatAmount(line.GoodsAmount),
            FormatAmount(line.TotalAmount),
            FormatAmount(line.ITBISAmount),
            FormatAmount(line.ITBISWithheld),
            FormatAmount(line.ITBISProportionality),
            FormatAmount(line.ITBISToCost),
            FormatAmount(line.ITBISAdvanced),
            FormatAmount(line.ISRWithheld),
            line.PaymentMethod
        );
    }

    private string FormatAmount(decimal amount)
    {
        return amount.ToString("F2", CultureInfo.InvariantCulture);
    }

    private string FormatRNC(string rnc)
    {
        // Remover guiones y rellenar con ceros a la izquierda
        var clean = rnc?.Replace("-", "") ?? "";
        return clean.PadLeft(11, '0');
    }
}
````

### 2.3 API Controller

```csharp
// DGIIService.Api/Controllers/Format606Controller.cs

[ApiController]
[Route("api/dgii/606")]
[Authorize(Roles = "Admin,Accountant")]
public class Format606Controller : ControllerBase
{
    private readonly IFormat606GeneratorService _generator;
    private readonly IFormat606Repository _repository;

    /// <summary>
    /// Obtener preview del Formato 606 sin generar archivo
    /// </summary>
    [HttpGet("preview")]
    public async Task<ActionResult<Format606PreviewDto>> GetPreview(
        [FromQuery] int year,
        [FromQuery] int month)
    {
        var preview = await _generator.GetPreviewAsync(year, month);
        return Ok(preview);
    }

    /// <summary>
    /// Generar Formato 606 oficial
    /// </summary>
    [HttpPost("generate")]
    public async Task<ActionResult<Format606Result>> Generate(
        [FromBody] GenerateFormat606Request request)
    {
        var result = await _generator.GenerateAsync(request.Year, request.Month);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Descargar archivo 606 generado
    /// </summary>
    [HttpGet("{formatId}/download")]
    public async Task<IActionResult> Download(Guid formatId)
    {
        var format = await _repository.GetByIdAsync(formatId);

        if (format == null)
            return NotFound();

        var bytes = Encoding.UTF8.GetBytes(format.FileContent);
        var fileName = $"606_{format.PeriodYear}{format.PeriodMonth:D2}.txt";

        return File(bytes, "text/plain", fileName);
    }

    /// <summary>
    /// Historial de formatos 606 generados
    /// </summary>
    [HttpGet("history")]
    public async Task<ActionResult<List<DGIIFormatDto>>> GetHistory(
        [FromQuery] int? year)
    {
        var formats = await _repository.GetHistoryAsync("606", year);
        return Ok(formats);
    }
}
```

---

## 3. GENERACIÓN AUTOMÁTICA DE FORMATO 607

### 3.1 Flujo de Generación

```csharp
// DGIIService.Application/Services/Format607GeneratorService.cs

public class Format607GeneratorService : IFormat607GeneratorService
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IS3Service _s3Service;

    public async Task<Format607Result> GenerateAsync(int year, int month)
    {
        // Obtener todas las facturas emitidas en el período
        var invoices = await _invoiceRepository.GetByPeriodAsync(year, month);

        // Filtrar facturas con NCF válido
        var validInvoices = invoices.Where(i => !string.IsNullOrEmpty(i.NCF)).ToList();

        var lines = new List<Format607Line>();

        foreach (var invoice in validInvoices)
        {
            lines.Add(new Format607Line
            {
                // Campo 1: RNC/Cédula del cliente
                RNCCedula = FormatRNC(invoice.CustomerRNC),

                // Campo 2: Tipo de Identificación
                IdentificationType = GetIdentificationType(invoice.CustomerRNC),

                // Campo 3: NCF
                NCF = invoice.NCF,

                // Campo 4: NCF Modificado
                NCFModified = invoice.NCFModified ?? "",

                // Campo 5: Tipo de Ingreso (siempre 02 para servicios de OKLA)
                IncomeType = "02", // Ingresos por servicios

                // Campo 6: Fecha del Comprobante
                InvoiceDate = invoice.IssueDate.ToString("yyyyMMdd"),

                // Campo 7: Fecha de Retención
                RetentionDate = "",

                // Campo 8: Monto Facturado
                InvoicedAmount = invoice.Subtotal,

                // Campo 9: ITBIS Facturado
                ITBISInvoiced = invoice.ITBISAmount,

                // Campo 10-12: ITBIS adicionales
                ThirdPartyITBIS = 0,
                ITBISWithheld = invoice.ITBISWithheld,
                ITBISPerceived = 0,

                // Campo 13: ISR Retenido
                ISRWithheld = invoice.ISRWithheld,

                // Campo 14: ISR Percibido
                ISRPerceived = 0,

                // Campo 15: Impuesto Selectivo (N/A para OKLA)
                SelectiveTax = 0,

                // Campo 16: Otros Impuestos
                OtherTaxes = 0,

                // Campo 17: Propina Legal (N/A)
                LegalTip = 0,

                // Campo 18: Efectivo
                CashAmount = invoice.PaymentMethod == "01" ? invoice.Total : 0,

                // Campo 19: Cheque/Transferencia
                CheckAmount = invoice.PaymentMethod == "02" ? invoice.Total : 0,

                // Campo 20: Tarjeta
                CardAmount = invoice.PaymentMethod == "03" ? invoice.Total : 0,

                // Campo 21: Crédito
                CreditAmount = invoice.PaymentMethod == "04" ? invoice.Total : 0,

                // Campo 22: Bonos/Certificados (N/A)
                VoucherAmount = 0,

                // Campo 23: Permuta (N/A)
                BarterAmount = 0,

                // Campo 24: Otras formas de pago
                OtherPaymentAmount = 0
            });
        }

        // Generar archivo
        var fileContent = GenerateFileContent(year, month, lines);

        // Subir y guardar
        var fileName = $"607_{OKLA_RNC}_{year}{month:D2}.txt";
        var s3Key = $"dgii-reports/607/{year}/{month:D2}/{fileName}";
        var fileUrl = await _s3Service.UploadAsync(s3Key, fileContent, "text/plain");

        return new Format607Result
        {
            Success = true,
            FileUrl = fileUrl,
            RecordCount = lines.Count,
            TotalInvoiced = lines.Sum(l => l.InvoicedAmount),
            TotalITBIS = lines.Sum(l => l.ITBISInvoiced)
        };
    }
}
```

---

## 4. JOBS AUTOMATIZADOS

### 4.1 Configuración de Hangfire/Quartz

```csharp
// DGIIService.Api/Jobs/DGIIJobsConfiguration.cs

public static class DGIIJobsConfiguration
{
    public static void ConfigureJobs(IServiceCollection services)
    {
        services.AddQuartz(q =>
        {
            // Job: Recordatorio de IR-17 (día 8 de cada mes a las 9:00 AM)
            q.AddJob<IR17ReminderJob>(opts => opts.WithIdentity("ir17-reminder"));
            q.AddTrigger(opts => opts
                .ForJob("ir17-reminder")
                .WithIdentity("ir17-reminder-trigger")
                .WithCronSchedule("0 0 9 8 * ?"));  // 9:00 AM día 8

            // Job: Generar preview de 606 (día 10 de cada mes a las 8:00 AM)
            q.AddJob<Format606PreviewJob>(opts => opts.WithIdentity("606-preview"));
            q.AddTrigger(opts => opts
                .ForJob("606-preview")
                .WithIdentity("606-preview-trigger")
                .WithCronSchedule("0 0 8 10 * ?"));  // 8:00 AM día 10

            // Job: Recordatorio de envío 606/607/608 (día 13 de cada mes)
            q.AddJob<FormatsReminderJob>(opts => opts.WithIdentity("formats-reminder"));
            q.AddTrigger(opts => opts
                .ForJob("formats-reminder")
                .WithIdentity("formats-reminder-trigger")
                .WithCronSchedule("0 0 9 13 * ?"));  // 9:00 AM día 13

            // Job: Recordatorio IT-1 (día 18 de cada mes)
            q.AddJob<IT1ReminderJob>(opts => opts.WithIdentity("it1-reminder"));
            q.AddTrigger(opts => opts
                .ForJob("it1-reminder")
                .WithIdentity("it1-reminder-trigger")
                .WithCronSchedule("0 0 9 18 * ?"));  // 9:00 AM día 18

            // Job: Verificar secuencias NCF (diario a las 7:00 AM)
            q.AddJob<NCFSequenceCheckJob>(opts => opts.WithIdentity("ncf-check"));
            q.AddTrigger(opts => opts
                .ForJob("ncf-check")
                .WithIdentity("ncf-check-trigger")
                .WithCronSchedule("0 0 7 * * ?"));  // 7:00 AM todos los días

            // Job: Backup de datos fiscales (semanal, domingos 2:00 AM)
            q.AddJob<FiscalDataBackupJob>(opts => opts.WithIdentity("fiscal-backup"));
            q.AddTrigger(opts => opts
                .ForJob("fiscal-backup")
                .WithIdentity("fiscal-backup-trigger")
                .WithCronSchedule("0 0 2 ? * SUN"));  // 2:00 AM domingos
        });

        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
    }
}
```

### 4.2 Implementación de Jobs

```csharp
// DGIIService.Api/Jobs/Format606PreviewJob.cs

public class Format606PreviewJob : IJob
{
    private readonly IFormat606GeneratorService _generator;
    private readonly INotificationService _notifications;
    private readonly ILogger<Format606PreviewJob> _logger;

    public async Task Execute(IJobExecutionContext context)
    {
        var now = DateTime.Now;
        var previousMonth = now.AddMonths(-1);
        var year = previousMonth.Year;
        var month = previousMonth.Month;

        _logger.LogInformation("Generating 606 preview for {Year}-{Month}", year, month);

        try
        {
            var preview = await _generator.GetPreviewAsync(year, month);

            // Enviar notificación al admin
            await _notifications.SendAsync(new NotificationRequest
            {
                Recipients = new[] { "admin@okla.com.do", "contador@okla.com.do" },
                Subject = $"📊 Preview Formato 606 - {GetMonthName(month)} {year}",
                Template = "dgii-606-preview",
                Data = new
                {
                    Year = year,
                    Month = GetMonthName(month),
                    RecordCount = preview.Lines.Count,
                    TotalAmount = preview.TotalAmount,
                    TotalITBIS = preview.TotalITBIS,
                    TotalISRWithheld = preview.TotalISRWithheld,
                    Deadline = $"15 de {GetMonthName(now.Month)} {now.Year}",
                    PreviewUrl = $"https://admin.okla.com.do/fiscal/606/preview?year={year}&month={month}"
                }
            });

            _logger.LogInformation("606 preview sent successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating 606 preview");

            // Notificar error
            await _notifications.SendAsync(new NotificationRequest
            {
                Recipients = new[] { "admin@okla.com.do" },
                Subject = "⚠️ Error en generación de 606",
                Template = "dgii-error",
                Data = new { Error = ex.Message }
            });
        }
    }
}
```

---

## 5. DASHBOARD FISCAL (FRONTEND)

### 5.1 Componentes React

```typescript
// frontend/web/src/pages/admin/FiscalDashboard.tsx

import React from 'react';
import { useQuery } from '@tanstack/react-query';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { Calendar, FileText, AlertTriangle, CheckCircle } from 'lucide-react';
import { dgiiService } from '@/services/dgiiService';

export const FiscalDashboard = () => {
  const currentDate = new Date();
  const currentMonth = currentDate.getMonth() + 1;
  const currentYear = currentDate.getFullYear();

  // Query para estado de reportes del mes actual
  const { data: reportStatus } = useQuery({
    queryKey: ['fiscal-status', currentYear, currentMonth],
    queryFn: () => dgiiService.getMonthStatus(currentYear, currentMonth)
  });

  // Query para alertas de vencimiento
  const { data: alerts } = useQuery({
    queryKey: ['fiscal-alerts'],
    queryFn: () => dgiiService.getUpcomingDeadlines()
  });

  // Query para secuencias NCF
  const { data: ncfStatus } = useQuery({
    queryKey: ['ncf-sequences'],
    queryFn: () => dgiiService.getNCFSequenceStatus()
  });

  return (
    <div className="p-6 space-y-6">
      <h1 className="text-3xl font-bold">Dashboard Fiscal DGII</h1>

      {/* Alertas de Vencimiento */}
      {alerts?.upcoming && alerts.upcoming.length > 0 && (
        <Alert variant="warning">
          <AlertTriangle className="h-4 w-4" />
          <AlertDescription>
            <strong>Próximos vencimientos:</strong>
            <ul className="mt-2">
              {alerts.upcoming.map((alert, i) => (
                <li key={i}>
                  📅 {alert.name}: {alert.deadline} ({alert.daysRemaining} días)
                </li>
              ))}
            </ul>
          </AlertDescription>
        </Alert>
      )}

      {/* Grid de Estado de Reportes */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">

        {/* Formato 606 */}
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium">Formato 606</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="flex items-center justify-between">
              <div>
                <p className="text-2xl font-bold">
                  {reportStatus?.format606?.recordCount || 0}
                </p>
                <p className="text-xs text-gray-500">registros</p>
              </div>
              <StatusBadge status={reportStatus?.format606?.status} />
            </div>
            <div className="mt-4 space-x-2">
              <Button size="sm" variant="outline"
                onClick={() => handlePreview('606')}>
                Preview
              </Button>
              <Button size="sm"
                onClick={() => handleGenerate('606')}
                disabled={reportStatus?.format606?.status === 'SUBMITTED'}>
                Generar
              </Button>
            </div>
          </CardContent>
        </Card>

        {/* Formato 607 */}
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium">Formato 607</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="flex items-center justify-between">
              <div>
                <p className="text-2xl font-bold">
                  {reportStatus?.format607?.recordCount || 0}
                </p>
                <p className="text-xs text-gray-500">facturas</p>
              </div>
              <StatusBadge status={reportStatus?.format607?.status} />
            </div>
            <div className="mt-4 space-x-2">
              <Button size="sm" variant="outline">Preview</Button>
              <Button size="sm">Generar</Button>
            </div>
          </CardContent>
        </Card>

        {/* IR-17 */}
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium">IR-17</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="flex items-center justify-between">
              <div>
                <p className="text-2xl font-bold">
                  RD${reportStatus?.ir17?.totalRetentions?.toLocaleString() || 0}
                </p>
                <p className="text-xs text-gray-500">retenciones</p>
              </div>
              <StatusBadge status={reportStatus?.ir17?.status} />
            </div>
            <p className="text-xs text-gray-400 mt-2">
              Vence: día 10 del mes
            </p>
          </CardContent>
        </Card>

        {/* IT-1 (ITBIS) */}
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium">IT-1 (ITBIS)</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="flex items-center justify-between">
              <div>
                <p className="text-2xl font-bold">
                  {reportStatus?.it1?.balance >= 0
                    ? `RD$${reportStatus.it1.balance.toLocaleString()}`
                    : `(RD$${Math.abs(reportStatus.it1.balance).toLocaleString()})`}
                </p>
                <p className="text-xs text-gray-500">
                  {reportStatus?.it1?.balance >= 0 ? 'a pagar' : 'crédito fiscal'}
                </p>
              </div>
              <StatusBadge status={reportStatus?.it1?.status} />
            </div>
            <p className="text-xs text-gray-400 mt-2">
              Vence: día 20 del mes
            </p>
          </CardContent>
        </Card>
      </div>

      {/* Estado de Secuencias NCF */}
      <Card>
        <CardHeader>
          <CardTitle>Secuencias NCF</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
            {ncfStatus?.sequences?.map((seq) => (
              <div key={seq.type} className="p-4 border rounded-lg">
                <div className="flex justify-between items-center">
                  <span className="font-medium">{seq.type}</span>
                  {seq.remaining < 100 && (
                    <AlertTriangle className="h-4 w-4 text-yellow-500" />
                  )}
                </div>
                <p className="text-2xl font-bold mt-2">{seq.remaining}</p>
                <p className="text-xs text-gray-500">disponibles</p>
                <div className="mt-2">
                  <div className="w-full bg-gray-200 rounded-full h-2">
                    <div
                      className={`h-2 rounded-full ${
                        seq.percentage > 20 ? 'bg-green-500' : 'bg-yellow-500'
                      }`}
                      style={{ width: `${seq.percentage}%` }}
                    />
                  </div>
                </div>
              </div>
            ))}
          </div>
        </CardContent>
      </Card>

      {/* Calendario Fiscal */}
      <Card>
        <CardHeader>
          <CardTitle>Calendario Fiscal - {getMonthName(currentMonth)} {currentYear}</CardTitle>
        </CardHeader>
        <CardContent>
          <FiscalCalendar
            month={currentMonth}
            year={currentYear}
            events={reportStatus?.calendarEvents}
          />
        </CardContent>
      </Card>
    </div>
  );
};
```

---

## 6. INTEGRACIÓN CON DGII (FUTURO)

### 6.1 API de DGII (Cuando esté disponible)

```csharp
// DGIIService.Infrastructure/External/DGIIApiClient.cs

public class DGIIApiClient : IDGIIApiClient
{
    private readonly HttpClient _httpClient;
    private readonly DGIISettings _settings;

    // Verificar NCF
    public async Task<NCFVerificationResult> VerifyNCFAsync(string ncf, string rnc)
    {
        // Actualmente se hace scraping de la página de DGII
        // En el futuro usará API oficial

        var response = await _httpClient.GetAsync(
            $"{_settings.BaseUrl}/consultaNCF?ncf={ncf}&rnc={rnc}");

        // Parsear respuesta
        return new NCFVerificationResult
        {
            NCF = ncf,
            IsValid = /* parsear respuesta */,
            BusinessName = /* parsear respuesta */,
            Status = /* parsear respuesta */
        };
    }

    // Enviar formato (cuando DGII habilite API)
    public async Task<SubmissionResult> SubmitFormatAsync(
        string formatType,
        string fileContent,
        int year,
        int month)
    {
        // Implementar cuando DGII habilite API de envío
        throw new NotImplementedException(
            "DGII no tiene API de envío. Usar Oficina Virtual manualmente.");
    }
}
```

### 6.2 Scraping para Verificación NCF

```csharp
// Servicio temporal mientras DGII no tenga API
public class DGIIScrapingService : IDGIIVerificationService
{
    public async Task<NCFVerificationResult> VerifyAsync(string ncf, string rnc)
    {
        using var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync();
        var page = await browser.NewPageAsync();

        await page.GotoAsync("https://dgii.gov.do/herramientas/consultas/Paginas/ncf.aspx");

        // Llenar formulario
        await page.FillAsync("#ctl00_ContentPlaceHolder1_txtRNC", rnc);
        await page.FillAsync("#ctl00_ContentPlaceHolder1_txtNCF", ncf);
        await page.ClickAsync("#ctl00_ContentPlaceHolder1_btnConsultar");

        // Esperar resultado
        await page.WaitForSelectorAsync(".resultado");

        // Extraer datos
        var result = await page.TextContentAsync(".resultado");

        await browser.CloseAsync();

        return ParseResult(result);
    }
}
```

---

## 7. SEGURIDAD Y AUDITORÍA

### 7.1 Logging de Operaciones Fiscales

```csharp
// Middleware para auditar operaciones fiscales
public class FiscalAuditMiddleware
{
    public async Task InvokeAsync(HttpContext context, IFiscalAuditService audit)
    {
        if (context.Request.Path.StartsWithSegments("/api/dgii"))
        {
            var userId = context.User.GetUserId();
            var action = $"{context.Request.Method} {context.Request.Path}";

            await audit.LogAsync(new FiscalAuditEntry
            {
                UserId = userId,
                Action = action,
                IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                Timestamp = DateTime.UtcNow,
                RequestBody = await ReadBodyAsync(context.Request),
                UserAgent = context.Request.Headers["User-Agent"]
            });
        }

        await _next(context);
    }
}
```

### 7.2 Tabla de Auditoría

```sql
CREATE TABLE fiscal_audit_log (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    timestamp TIMESTAMP NOT NULL DEFAULT NOW(),
    user_id UUID NOT NULL,
    user_email VARCHAR(200),
    action VARCHAR(100) NOT NULL,
    entity_type VARCHAR(50),           -- EXPENSE, INVOICE, NCF, FORMAT_606, etc.
    entity_id UUID,
    old_values JSONB,
    new_values JSONB,
    ip_address VARCHAR(50),
    user_agent TEXT,
    result VARCHAR(20),                -- SUCCESS, FAILURE, ERROR
    error_message TEXT
);

CREATE INDEX idx_fiscal_audit_timestamp ON fiscal_audit_log(timestamp);
CREATE INDEX idx_fiscal_audit_user ON fiscal_audit_log(user_id);
CREATE INDEX idx_fiscal_audit_entity ON fiscal_audit_log(entity_type, entity_id);
```

---

## 📋 CHECKLIST DE IMPLEMENTACIÓN

### Fase 1: Base de Datos (Sprint 1)

- [ ] Crear tablas de gastos (expenses, expense_providers, expense_documents)
- [ ] Crear tablas de NCF (ncf_sequences, ncf_issued, ncf_received)
- [ ] Crear tablas de formatos DGII (dgii_formats, fiscal_audit_log)
- [ ] Crear índices y relaciones
- [ ] Migrar datos existentes (si los hay)

### Fase 2: APIs Backend (Sprint 2)

- [ ] Implementar ExpenseService CRUD
- [ ] Implementar ProviderService CRUD
- [ ] Implementar NCFService (generación y verificación)
- [ ] Implementar Format606GeneratorService
- [ ] Implementar Format607GeneratorService
- [ ] Implementar Format608GeneratorService
- [ ] Implementar IR17Service
- [ ] Implementar ITBISCalculatorService

### Fase 3: Jobs y Automatización (Sprint 3)

- [ ] Configurar Quartz/Hangfire
- [ ] Implementar job de recordatorios
- [ ] Implementar job de generación automática de previews
- [ ] Implementar job de verificación de secuencias NCF
- [ ] Implementar job de backup

### Fase 4: Frontend Admin (Sprint 4)

- [ ] Dashboard fiscal principal
- [ ] Formulario de registro de gastos
- [ ] Lista de gastos con filtros
- [ ] Vista de aprobación de gastos
- [ ] Generador de reportes 606/607/608
- [ ] Calendario fiscal interactivo
- [ ] Monitor de secuencias NCF

### Fase 5: Integración DGII (Sprint 5)

- [ ] Servicio de verificación de NCF (scraping)
- [ ] Validador de formatos
- [ ] Descarga de archivos generados
- [ ] Documentación para contador

---

**Documento creado:** Enero 25, 2026  
**Próxima revisión:** Al implementar cada fase  
**Responsable:** Equipo de Desarrollo
