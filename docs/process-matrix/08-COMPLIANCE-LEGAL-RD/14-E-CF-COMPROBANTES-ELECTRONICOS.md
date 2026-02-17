# 📄 Comprobantes Fiscales Electrónicos (e-CF) - OKLA S.R.L.

> **Empresa:** OKLA S.R.L.  
> **RNC:** 1-33-32590-1  
> **Registro Mercantil:** 196339PSD  
> **Fecha de Creación:** Enero 25, 2026  
> **Propósito:** Implementación de Facturación Electrónica según Norma General 06-2018

---

## 📋 RESUMEN EJECUTIVO

OKLA implementará **Comprobantes Fiscales Electrónicos (e-CF)** para todas sus operaciones de facturación, cumpliendo con la Norma General 06-2018 de DGII. Esto permitirá:

1. ✅ Emisión instantánea de facturas validadas por DGII
2. ✅ Eliminación de papel y almacenamiento físico
3. ✅ Trazabilidad completa de todas las transacciones
4. ✅ Integración directa con sistemas DGII
5. ✅ Generación automática de Formatos 606/607/608

---

## 1. ¿QUÉ ES EL e-CF?

### 1.1 Definición

```
┌─────────────────────────────────────────────────────────────────────────┐
│              COMPROBANTE FISCAL ELECTRÓNICO (e-CF)                      │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  El e-CF es un documento tributario emitido y transmitido              │
│  electrónicamente a la DGII, que tiene la misma validez legal          │
│  que un NCF en papel.                                                   │
│                                                                         │
│  📋 CARACTERÍSTICAS:                                                    │
│  ├── Firmado digitalmente                                               │
│  ├── Transmitido en tiempo real a DGII                                  │
│  ├── Validado y autorizado por DGII                                     │
│  ├── Almacenado electrónicamente                                        │
│  └── Incluye código QR de verificación                                  │
│                                                                         │
│  📊 ESTRUCTURA DEL e-CF:                                                │
│  E + Tipo (2 dígitos) + Secuencial (8 dígitos)                          │
│  Ejemplo: E310000001 (Factura de Crédito Fiscal Electrónica)            │
│                                                                         │
│  📜 BASE LEGAL:                                                         │
│  • Norma General 06-2018 (DGII)                                         │
│  • Resolución 13-2019 (Especificaciones Técnicas)                       │
│  • Ley 11-92 (Código Tributario)                                        │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 1.2 Tipos de e-CF

| Código | Tipo de e-CF              | Equivalente NCF | Uso en OKLA                   |
| ------ | ------------------------- | --------------- | ----------------------------- |
| E31    | Factura de Crédito Fiscal | B01             | Ventas a empresas con RNC     |
| E32    | Factura de Consumo        | B02             | Ventas a consumidores finales |
| E33    | Nota de Débito            | B03             | Ajustes a favor de OKLA       |
| E34    | Nota de Crédito           | B04             | Devoluciones, descuentos      |
| E41    | Compras                   | B11             | Compras a informales          |
| E43    | Gastos Menores            | B13             | Gastos del exterior           |
| E44    | Regímenes Especiales      | B14             | (No aplica a OKLA)            |
| E45    | Gubernamental             | B15             | Ventas al gobierno            |
| E46    | Exportaciones             | B16             | (No aplica a OKLA)            |
| E47    | Compras Exterior          | B17             | Pagos internacionales         |

### 1.3 Tipos de e-CF que OKLA Emitirá

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    e-CF QUE OKLA EMITIRÁ                                │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  🧾 VENTAS (Formato 607)                                                │
│  ─────────────────────────                                              │
│                                                                         │
│  E31 - Factura de Crédito Fiscal Electrónica                            │
│  └── Cuando: Dealer con RNC compra suscripción o boost                  │
│  └── Ejemplo: Dealer ABC SRL paga $129/mes plan Pro                    │
│  └── ITBIS: 18% ($23.22)                                               │
│                                                                         │
│  E32 - Factura de Consumo Electrónica                                   │
│  └── Cuando: Individuo paga listing o boost                            │
│  └── Ejemplo: Juan Pérez paga $29 por publicar vehículo                │
│  └── ITBIS: 18% ($5.22)                                                │
│                                                                         │
│  E34 - Nota de Crédito Electrónica                                      │
│  └── Cuando: Reembolso, devolución, descuento posterior                │
│  └── Ejemplo: Cliente cancela suscripción, reembolso prorrateado       │
│  └── Debe referenciar e-CF original                                    │
│                                                                         │
│  📦 COMPRAS (Formato 606)                                               │
│  ─────────────────────────                                              │
│                                                                         │
│  E47 - Comprobante de Compras Exterior                                  │
│  └── Cuando: Pago a Digital Ocean, Stripe, Google Ads, etc.           │
│  └── NO tiene ITBIS (es gasto exterior)                                │
│  └── Sistema genera e-CF interno para control                          │
│                                                                         │
│  E41 - Comprobante de Compras                                           │
│  └── Cuando: Proveedor local no tiene NCF (raro)                       │
│  └── Retención de 2% ISR                                               │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 2. PROCESO DE CERTIFICACIÓN e-CF

### 2.1 Requisitos para ser Emisor e-CF

```
┌─────────────────────────────────────────────────────────────────────────┐
│              REQUISITOS PARA EMITIR e-CF                                │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  1️⃣ REQUISITOS LEGALES                                                  │
│  ├── ✅ RNC activo y al día                                             │
│  ├── ✅ Sin deudas con DGII                                             │
│  ├── ✅ Declaraciones al día (IT-1, IR-17, etc.)                        │
│  └── ✅ Representante legal registrado                                   │
│                                                                         │
│  2️⃣ REQUISITOS TÉCNICOS                                                 │
│  ├── 🔐 Certificado Digital (Firma Electrónica)                         │
│  │   └── Emitido por: INDOTEL o proveedor autorizado                   │
│  │   └── Costo: ~RD$5,000-15,000/año                                   │
│  │   └── Opciones: Cámara de Comercio, CertiSign, etc.                 │
│  ├── 💻 Sistema de Facturación Electrónica                              │
│  │   └── Desarrollo propio (OKLA) o software certificado               │
│  │   └── Debe cumplir especificaciones DGII                            │
│  └── 🌐 Conexión a Servicios Web DGII                                   │
│      └── URLs de prueba y producción                                   │
│      └── Autenticación con certificado                                 │
│                                                                         │
│  3️⃣ PROCESO DE CERTIFICACIÓN                                            │
│  ├── Paso 1: Solicitar acceso a ambiente de pruebas                     │
│  ├── Paso 2: Desarrollar integración                                    │
│  ├── Paso 3: Ejecutar pruebas (mínimo 20 e-CF de cada tipo)            │
│  ├── Paso 4: DGII valida y aprueba                                      │
│  ├── Paso 5: Migrar a producción                                        │
│  └── Tiempo estimado: 30-60 días                                       │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 2.2 Arquitectura de Integración DGII

```
┌─────────────────────────────────────────────────────────────────────────┐
│              ARQUITECTURA e-CF - OKLA                                   │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  ┌──────────────┐    ┌──────────────┐    ┌──────────────┐              │
│  │   Frontend   │───▶│ BillingService│───▶│  ECFService  │              │
│  │  (Checkout)  │    │              │    │              │              │
│  └──────────────┘    └──────────────┘    └──────┬───────┘              │
│                                                  │                      │
│                                                  ▼                      │
│                                         ┌──────────────┐                │
│                                         │ Certificado  │                │
│                                         │   Digital    │                │
│                                         └──────┬───────┘                │
│                                                  │                      │
│                                                  ▼                      │
│  ┌──────────────────────────────────────────────────────────────┐      │
│  │                    DGII Web Services                          │      │
│  │  ┌────────────────┐  ┌────────────────┐  ┌────────────────┐  │      │
│  │  │ RecepcionECF   │  │ ConsultaECF    │  │ AnulacionECF   │  │      │
│  │  │ (Enviar e-CF)  │  │ (Verificar)    │  │ (Cancelar)     │  │      │
│  │  └────────────────┘  └────────────────┘  └────────────────┘  │      │
│  └──────────────────────────────────────────────────────────────┘      │
│                                                                         │
│  URLs DGII:                                                             │
│  • Pruebas: https://ecf.dgii.gov.do/testecf/                           │
│  • Producción: https://ecf.dgii.gov.do/ecf/                            │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 3. IMPLEMENTACIÓN TÉCNICA

### 3.1 ECFService - Microservicio de Facturación Electrónica

```csharp
// ECFService.Domain/Entities/ElectronicInvoice.cs

namespace ECFService.Domain.Entities;

public class ElectronicInvoice
{
    public Guid Id { get; set; }
    public string ECFNumber { get; set; } = string.Empty;  // E3100000001
    public ECFType Type { get; set; }
    public ECFStatus Status { get; set; }

    // Emisor (OKLA)
    public string IssuerRNC { get; set; } = "133325901";
    public string IssuerName { get; set; } = "OKLA S.R.L.";

    // Receptor (Cliente)
    public string? ReceiverRNC { get; set; }
    public string ReceiverName { get; set; } = string.Empty;
    public ReceiverType ReceiverType { get; set; }  // Empresa, PersonaFisica, Consumidor

    // Montos
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }  // ITBIS 18%
    public decimal Total { get; set; }

    // Fechas
    public DateTime IssueDate { get; set; }
    public DateTime? DGIISubmitDate { get; set; }
    public DateTime? DGIIApprovalDate { get; set; }

    // DGII Response
    public string? TrackingNumber { get; set; }  // Número de seguimiento DGII
    public string? ApprovalCode { get; set; }
    public string? DGIIMessage { get; set; }

    // XML y PDF
    public string? SignedXmlUrl { get; set; }  // XML firmado en S3
    public string? PdfUrl { get; set; }         // PDF con QR en S3
    public string? QRCode { get; set; }         // Código QR de verificación

    // Relaciones
    public string? OriginalECFNumber { get; set; }  // Para notas de crédito
    public Guid? PaymentId { get; set; }
    public ICollection<ElectronicInvoiceItem> Items { get; set; } = new List<ElectronicInvoiceItem>();
}

public enum ECFType
{
    E31_CreditoFiscal = 31,
    E32_Consumo = 32,
    E33_NotaDebito = 33,
    E34_NotaCredito = 34,
    E41_Compras = 41,
    E43_GastosMenores = 43,
    E47_ComprasExterior = 47
}

public enum ECFStatus
{
    Draft,          // En edición
    Pending,        // Pendiente de firma
    Signed,         // Firmado, pendiente de envío
    Submitted,      // Enviado a DGII
    Approved,       // Aprobado por DGII
    Rejected,       // Rechazado por DGII
    Cancelled       // Anulado
}
```

### 3.2 Servicio de Generación de e-CF

```csharp
// ECFService.Application/Services/ECFGeneratorService.cs

public class ECFGeneratorService : IECFGeneratorService
{
    private readonly IECFRepository _repository;
    private readonly IDigitalSignatureService _signatureService;
    private readonly IDGIIWebService _dgiiService;
    private readonly IS3Service _s3;
    private readonly IPdfGenerator _pdfGenerator;

    public async Task<ElectronicInvoice> GenerateFromPaymentAsync(Payment payment)
    {
        // 1. Determinar tipo de e-CF
        var ecfType = DetermineECFType(payment);

        // 2. Obtener siguiente número secuencial
        var nextNumber = await _repository.GetNextSequentialAsync(ecfType);
        var ecfNumber = FormatECFNumber(ecfType, nextNumber);

        // 3. Crear e-CF
        var invoice = new ElectronicInvoice
        {
            Id = Guid.NewGuid(),
            ECFNumber = ecfNumber,
            Type = ecfType,
            Status = ECFStatus.Draft,
            IssuerRNC = "133325901",
            IssuerName = "OKLA S.R.L.",
            ReceiverRNC = payment.CustomerRNC,
            ReceiverName = payment.CustomerName,
            ReceiverType = DetermineReceiverType(payment),
            Subtotal = payment.Subtotal,
            TaxAmount = payment.TaxAmount,
            Total = payment.Total,
            IssueDate = DateTime.UtcNow,
            PaymentId = payment.Id
        };

        // 4. Agregar items
        invoice.Items = payment.Items.Select(i => new ElectronicInvoiceItem
        {
            Description = i.Description,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            TaxAmount = i.TaxAmount,
            Total = i.Total
        }).ToList();

        // 5. Generar XML según especificaciones DGII
        var xml = await GenerateXmlAsync(invoice);

        // 6. Firmar digitalmente
        var signedXml = await _signatureService.SignXmlAsync(xml);
        invoice.Status = ECFStatus.Signed;

        // 7. Guardar XML firmado en S3
        var xmlKey = $"ecf/{invoice.IssueDate:yyyy/MM}/{invoice.ECFNumber}.xml";
        invoice.SignedXmlUrl = await _s3.UploadAsync(xmlKey, signedXml);

        // 8. Enviar a DGII
        var dgiiResponse = await _dgiiService.SubmitECFAsync(signedXml);

        if (dgiiResponse.Success)
        {
            invoice.Status = ECFStatus.Approved;
            invoice.TrackingNumber = dgiiResponse.TrackingNumber;
            invoice.ApprovalCode = dgiiResponse.ApprovalCode;
            invoice.DGIISubmitDate = DateTime.UtcNow;
            invoice.DGIIApprovalDate = DateTime.UtcNow;

            // 9. Generar PDF con QR
            invoice.QRCode = GenerateQRCode(invoice);
            var pdf = await _pdfGenerator.GenerateECFPdfAsync(invoice);
            var pdfKey = $"ecf/{invoice.IssueDate:yyyy/MM}/{invoice.ECFNumber}.pdf";
            invoice.PdfUrl = await _s3.UploadAsync(pdfKey, pdf);
        }
        else
        {
            invoice.Status = ECFStatus.Rejected;
            invoice.DGIIMessage = dgiiResponse.ErrorMessage;
        }

        // 10. Guardar en base de datos
        await _repository.AddAsync(invoice);

        return invoice;
    }

    private ECFType DetermineECFType(Payment payment)
    {
        // Si tiene RNC → E31 (Crédito Fiscal)
        // Si no tiene RNC → E32 (Consumo)
        if (!string.IsNullOrEmpty(payment.CustomerRNC))
            return ECFType.E31_CreditoFiscal;

        return ECFType.E32_Consumo;
    }

    private string FormatECFNumber(ECFType type, int sequential)
    {
        // E + tipo (2 dígitos) + secuencial (8 dígitos)
        return $"E{(int)type:D2}{sequential:D8}";
    }

    private string GenerateQRCode(ElectronicInvoice invoice)
    {
        // URL de verificación DGII
        var verificationUrl = $"https://dgii.gov.do/ecf/consulta?" +
            $"rnc={invoice.IssuerRNC}&ecf={invoice.ECFNumber}";

        // Generar QR con la URL
        return QRCodeGenerator.Generate(verificationUrl);
    }
}
```

### 3.3 Integración con Web Services DGII

```csharp
// ECFService.Infrastructure/Services/DGIIWebService.cs

public class DGIIWebService : IDGIIWebService
{
    private readonly HttpClient _httpClient;
    private readonly DGIISettings _settings;
    private readonly ILogger<DGIIWebService> _logger;

    private const string PRODUCTION_URL = "https://ecf.dgii.gov.do/ecf/";
    private const string TEST_URL = "https://ecf.dgii.gov.do/testecf/";

    public async Task<DGIIResponse> SubmitECFAsync(string signedXml)
    {
        var url = _settings.IsProduction ? PRODUCTION_URL : TEST_URL;
        url += "RecepcionECF";

        try
        {
            // Configurar cliente con certificado
            var handler = new HttpClientHandler();
            handler.ClientCertificates.Add(_settings.Certificate);

            using var client = new HttpClient(handler);

            var content = new StringContent(signedXml, Encoding.UTF8, "application/xml");
            var response = await client.PostAsync(url, content);

            var responseXml = await response.Content.ReadAsStringAsync();

            // Parsear respuesta DGII
            return ParseDGIIResponse(responseXml);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando e-CF a DGII");
            return new DGIIResponse
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<DGIIResponse> CancelECFAsync(string ecfNumber, string reason)
    {
        var url = _settings.IsProduction ? PRODUCTION_URL : TEST_URL;
        url += "AnulacionECF";

        var cancellationXml = GenerateCancellationXml(ecfNumber, reason);
        var signedXml = await _signatureService.SignXmlAsync(cancellationXml);

        // Similar al submit...
        return await SubmitXmlAsync(url, signedXml);
    }

    public async Task<ECFVerificationResult> VerifyECFAsync(string ecfNumber)
    {
        var url = _settings.IsProduction ? PRODUCTION_URL : TEST_URL;
        url += $"ConsultaECF?ecf={ecfNumber}&rnc=133325901";

        var response = await _httpClient.GetStringAsync(url);
        return ParseVerificationResponse(response);
    }
}
```

---

## 4. FLUJO DE FACTURACIÓN ELECTRÓNICA

### 4.1 Flujo Completo de Venta

```
┌─────────────────────────────────────────────────────────────────────────┐
│              FLUJO DE FACTURACIÓN ELECTRÓNICA - OKLA                    │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  1️⃣ CLIENTE REALIZA COMPRA                                              │
│     ├── Selecciona servicio (Listing $29, Boost, Suscripción)          │
│     ├── Ingresa datos de facturación                                   │
│     │   ├── Nombre completo                                            │
│     │   ├── RNC (opcional, pero necesario para E31)                    │
│     │   └── Email para recibir factura                                 │
│     └── Procede al pago (AZUL o Stripe)                                │
│                                                                         │
│                        ▼                                                │
│                                                                         │
│  2️⃣ PAGO PROCESADO                                                      │
│     ├── Stripe/AZUL confirma transacción                               │
│     ├── BillingService registra Payment                                │
│     └── Dispara evento: PaymentCompletedEvent                          │
│                                                                         │
│                        ▼                                                │
│                                                                         │
│  3️⃣ GENERACIÓN DE e-CF                                                  │
│     ├── ECFService recibe evento                                       │
│     ├── Determina tipo: E31 (con RNC) o E32 (sin RNC)                  │
│     ├── Genera XML según especificaciones DGII                         │
│     ├── Firma digitalmente con certificado                             │
│     └── Valida estructura XML                                          │
│                                                                         │
│                        ▼                                                │
│                                                                         │
│  4️⃣ ENVÍO A DGII                                                        │
│     ├── POST a https://ecf.dgii.gov.do/ecf/RecepcionECF                │
│     ├── DGII valida: RNC, montos, estructura, firma                    │
│     └── DGII responde en < 3 segundos                                  │
│                                                                         │
│                        ▼                                                │
│                                                                         │
│  5️⃣ RESPUESTA DGII                                                      │
│     ├── ✅ APROBADO:                                                    │
│     │   ├── Guardar TrackingNumber y ApprovalCode                      │
│     │   ├── Generar PDF con código QR                                  │
│     │   ├── Almacenar en S3                                            │
│     │   └── Enviar al cliente por email                                │
│     │                                                                   │
│     └── ❌ RECHAZADO:                                                   │
│         ├── Registrar código de error                                  │
│         ├── Alertar a administrador                                    │
│         ├── Corregir y reintentar                                      │
│         └── Si persiste: Generar NCF manual (backup)                   │
│                                                                         │
│                        ▼                                                │
│                                                                         │
│  6️⃣ ALMACENAMIENTO Y REPORTE                                            │
│     ├── e-CF almacenado en PostgreSQL                                  │
│     ├── XML/PDF almacenados en S3                                      │
│     ├── Incluido automáticamente en Formato 607                        │
│     └── Disponible para auditoría                                      │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 4.2 Flujo de Nota de Crédito (Reembolso)

```
┌─────────────────────────────────────────────────────────────────────────┐
│              FLUJO DE NOTA DE CRÉDITO (E34)                             │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  1️⃣ SOLICITUD DE REEMBOLSO                                              │
│     ├── Cliente solicita cancelación/reembolso                         │
│     ├── Admin aprueba en dashboard                                     │
│     └── Sistema identifica e-CF original                               │
│                                                                         │
│                        ▼                                                │
│                                                                         │
│  2️⃣ GENERACIÓN DE NOTA DE CRÉDITO                                       │
│     ├── Tipo: E34 (Nota de Crédito Electrónica)                        │
│     ├── Referencia: e-CF original (ej: E3100000123)                    │
│     ├── Monto: Total o parcial del reembolso                           │
│     └── Motivo: Cancelación, error, devolución                         │
│                                                                         │
│                        ▼                                                │
│                                                                         │
│  3️⃣ ENVÍO A DGII                                                        │
│     ├── Mismo proceso que factura                                      │
│     ├── DGII valida que e-CF original existe                           │
│     └── DGII valida que monto no excede original                       │
│                                                                         │
│                        ▼                                                │
│                                                                         │
│  4️⃣ PROCESAMIENTO                                                       │
│     ├── Si aprobado: Procesar reembolso (Stripe/AZUL)                  │
│     ├── Actualizar estado del e-CF original                            │
│     ├── Enviar nota de crédito al cliente                              │
│     └── Incluir en Formato 607 como crédito                            │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 5. ESTRUCTURA XML DEL e-CF

### 5.1 Ejemplo de XML (E31 - Crédito Fiscal)

```xml
<?xml version="1.0" encoding="UTF-8"?>
<ECF xmlns="https://dgii.gov.do/ecf">
  <Encabezado>
    <Version>1.0</Version>
    <IdDoc>
      <TipoECF>31</TipoECF>
      <eNCF>E3100000001</eNCF>
      <FechaEmision>2026-01-25</FechaEmision>
      <FechaVencimiento>2026-02-25</FechaVencimiento>
    </IdDoc>
    <Emisor>
      <RNCEmisor>133325901</RNCEmisor>
      <RazonSocialEmisor>OKLA S.R.L.</RazonSocialEmisor>
      <DireccionEmisor>Santo Domingo, RD</DireccionEmisor>
      <FechaConstitucion>2024-01-15</FechaConstitucion>
    </Emisor>
    <Receptor>
      <RNCReceptor>101234567</RNCReceptor>
      <RazonSocialReceptor>Auto Dealer ABC S.R.L.</RazonSocialReceptor>
      <DireccionReceptor>Santiago, RD</DireccionReceptor>
    </Receptor>
    <Totales>
      <MontoGravadoI>129.00</MontoGravadoI>
      <ITBIS1>23.22</ITBIS1>
      <TotalITBIS>23.22</TotalITBIS>
      <MontoTotal>152.22</MontoTotal>
    </Totales>
  </Encabezado>
  <DetallesItem>
    <Item>
      <NumeroLinea>1</NumeroLinea>
      <IndicadorFacturacion>1</IndicadorFacturacion>
      <NombreItem>Suscripción Plan Pro - Enero 2026</NombreItem>
      <IndicadorBienoServicio>2</IndicadorBienoServicio>
      <CantidadItem>1</CantidadItem>
      <PrecioUnitarioItem>129.00</PrecioUnitarioItem>
      <MontoItem>129.00</MontoItem>
    </Item>
  </DetallesItem>
  <SubTotales>
    <SubTotalGravadoI>129.00</SubTotalGravadoI>
    <SubTotalITBIS>23.22</SubTotalITBIS>
  </SubTotales>
  <DescuentosRecargos />
  <Paginacion>
    <PaginaActual>1</PaginaActual>
    <TotalPaginas>1</TotalPaginas>
  </Paginacion>
  <FechaHoraFirma>2026-01-25T10:30:00</FechaHoraFirma>
</ECF>
```

---

## 6. BASE DE DATOS

### 6.1 Schema para e-CF

```sql
-- Tabla principal de e-CF
CREATE TABLE electronic_invoices (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    ecf_number VARCHAR(20) NOT NULL UNIQUE,  -- E3100000001
    ecf_type INTEGER NOT NULL,                -- 31, 32, 34, etc.
    status VARCHAR(20) NOT NULL DEFAULT 'Draft',

    -- Emisor
    issuer_rnc VARCHAR(15) NOT NULL DEFAULT '133325901',
    issuer_name VARCHAR(200) NOT NULL DEFAULT 'OKLA S.R.L.',

    -- Receptor
    receiver_rnc VARCHAR(15),
    receiver_name VARCHAR(200) NOT NULL,
    receiver_type VARCHAR(20) NOT NULL,  -- Empresa, PersonaFisica, Consumidor
    receiver_email VARCHAR(200),

    -- Montos
    subtotal DECIMAL(18,2) NOT NULL,
    tax_rate DECIMAL(5,2) DEFAULT 18.00,
    tax_amount DECIMAL(18,2) NOT NULL,
    total DECIMAL(18,2) NOT NULL,
    currency VARCHAR(3) DEFAULT 'DOP',

    -- Fechas
    issue_date TIMESTAMP NOT NULL,
    due_date TIMESTAMP,
    dgii_submit_date TIMESTAMP,
    dgii_approval_date TIMESTAMP,

    -- DGII Response
    tracking_number VARCHAR(50),
    approval_code VARCHAR(50),
    dgii_message TEXT,

    -- Documentos
    signed_xml_url TEXT,
    pdf_url TEXT,
    qr_code TEXT,

    -- Referencias
    original_ecf_number VARCHAR(20),  -- Para notas de crédito
    payment_id UUID,

    -- Auditoría
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    created_by VARCHAR(100)
);

-- Items del e-CF
CREATE TABLE electronic_invoice_items (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    electronic_invoice_id UUID NOT NULL REFERENCES electronic_invoices(id),
    line_number INTEGER NOT NULL,
    description VARCHAR(500) NOT NULL,
    quantity DECIMAL(18,4) DEFAULT 1,
    unit_price DECIMAL(18,2) NOT NULL,
    tax_amount DECIMAL(18,2) NOT NULL,
    total DECIMAL(18,2) NOT NULL,
    is_service BOOLEAN DEFAULT true
);

-- Secuencias por tipo de e-CF
CREATE TABLE ecf_sequences (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    ecf_type INTEGER NOT NULL UNIQUE,
    current_number INTEGER NOT NULL DEFAULT 0,
    prefix VARCHAR(10) NOT NULL,  -- E31, E32, E34, etc.
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Inicializar secuencias
INSERT INTO ecf_sequences (ecf_type, current_number, prefix) VALUES
(31, 0, 'E31'),  -- Crédito Fiscal
(32, 0, 'E32'),  -- Consumo
(34, 0, 'E34'),  -- Nota de Crédito
(47, 0, 'E47'); -- Compras Exterior

-- Índices
CREATE INDEX idx_ecf_number ON electronic_invoices(ecf_number);
CREATE INDEX idx_ecf_type ON electronic_invoices(ecf_type);
CREATE INDEX idx_ecf_status ON electronic_invoices(status);
CREATE INDEX idx_ecf_issue_date ON electronic_invoices(issue_date);
CREATE INDEX idx_ecf_receiver_rnc ON electronic_invoices(receiver_rnc);
CREATE INDEX idx_ecf_payment ON electronic_invoices(payment_id);
```

---

## 7. MIGRACIÓN DE NCF A e-CF

### 7.1 Plan de Migración

```
┌─────────────────────────────────────────────────────────────────────────┐
│              PLAN DE MIGRACIÓN NCF → e-CF                               │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  FASE 1: PREPARACIÓN (Semanas 1-2)                                      │
│  ─────────────────────────────────                                      │
│  • Obtener certificado digital                                          │
│  • Configurar ambiente de pruebas DGII                                  │
│  • Desarrollar ECFService                                               │
│  • Crear tablas en base de datos                                        │
│                                                                         │
│  FASE 2: DESARROLLO (Semanas 3-4)                                       │
│  ────────────────────────────────                                       │
│  • Implementar generación de XML                                        │
│  • Implementar firma digital                                            │
│  • Integrar con Web Services DGII (pruebas)                             │
│  • Generar PDFs con QR                                                  │
│                                                                         │
│  FASE 3: PRUEBAS (Semanas 5-6)                                          │
│  ──────────────────────────────                                         │
│  • Ejecutar 20+ e-CF de cada tipo en ambiente de pruebas                │
│  • Validar respuestas de DGII                                           │
│  • Probar flujos de error                                               │
│  • Probar notas de crédito                                              │
│                                                                         │
│  FASE 4: CERTIFICACIÓN (Semana 7)                                       │
│  ─────────────────────────────────                                      │
│  • Solicitar revisión de DGII                                           │
│  • Corregir observaciones                                               │
│  • Obtener aprobación                                                   │
│                                                                         │
│  FASE 5: PRODUCCIÓN (Semana 8)                                          │
│  ─────────────────────────────                                          │
│  • Migrar a URLs de producción                                          │
│  • Activar para todas las transacciones                                 │
│  • Monitorear primeras 100 transacciones                                │
│  • Desactivar emisión de NCF tradicional                                │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 8. ENVÍO AUTOMÁTICO DE REPORTES FISCALES

### 8.1 Reportes con Transmisión Automática

Con la implementación de e-CF, los siguientes reportes se transmiten **automáticamente** a la DGII:

```
┌─────────────────────────────────────────────────────────────────────────┐
│          REPORTES CON ENVÍO AUTOMÁTICO A DGII                           │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  ✅ ENVÍO EN TIEMPO REAL (Cada Transacción)                             │
│  ─────────────────────────────────────────────                          │
│                                                                         │
│  📄 e-CF Emitidos (E31, E32, E34)                                       │
│     └── Cada factura → DGII en < 5 segundos                            │
│     └── Validación instantánea                                         │
│     └── TrackingNumber como confirmación                               │
│     └── 0% intervención humana                                         │
│                                                                         │
│  ✅ GENERACIÓN AUTOMÁTICA (Mensual - Sin envío manual)                 │
│  ──────────────────────────────────────────────────────                 │
│                                                                         │
│  📊 Formato 607 (Ventas y Operaciones)                                  │
│     └── DGII ya tiene todos los e-CF → Formato 607 PRE-GENERADO        │
│     └── Solo verificar en Oficina Virtual                              │
│     └── Tiempo: 5 minutos de revisión                                  │
│                                                                         │
│  📊 Formato 608 (e-CF Anulados)                                         │
│     └── Generado automáticamente de notas de crédito (E34)             │
│     └── DGII consolida anulaciones                                     │
│                                                                         │
│  ✅ ENVÍO ELECTRÓNICO (Mensual - Semi-automático)                      │
│  ─────────────────────────────────────────────────                      │
│                                                                         │
│  📊 Formato 606 (Compras y Gastos)                                      │
│     └── Sistema genera TXT automáticamente                             │
│     └── Envío vía Web Service DGII                                     │
│     └── Validación previa automática                                   │
│                                                                         │
│  📊 Formato 609 (Compras del Exterior)                                  │
│     └── Generado de gastos internacionales (DO, Stripe, etc.)          │
│     └── Envío electrónico a DGII                                       │
│                                                                         │
│  📊 IT-1 (Declaración ITBIS Mensual)                                    │
│     └── Pre-llenado desde e-CF + Formato 606                           │
│     └── Cálculo: ITBIS cobrado - ITBIS pagado                         │
│     └── Envío electrónico a DGII                                       │
│                                                                         │
│  📊 IR-17 (Retenciones ISR)                                             │
│     └── Generado de gastos con retención 10%                           │
│     └── Envío electrónico a DGII                                       │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 8.2 Implementación: Servicio de Envío Automático

```csharp
// ECFService.Application/Services/AutomaticReportSubmissionService.cs

public class AutomaticReportSubmissionService : IAutomaticReportSubmissionService
{
    private readonly IDGIIWebService _dgiiService;
    private readonly IFormat606Service _format606Service;
    private readonly IFormat609Service _format609Service;
    private readonly IIT1Service _it1Service;
    private readonly IIR17Service _ir17Service;
    private readonly ILogger<AutomaticReportSubmissionService> _logger;

    /// <summary>
    /// Enviar todos los reportes del mes automáticamente
    /// Se ejecuta el día 10 de cada mes a las 8:00 AM
    /// </summary>
    public async Task<MonthlyReportResult> SubmitAllMonthlyReportsAsync(int year, int month)
    {
        var result = new MonthlyReportResult { Year = year, Month = month };

        _logger.LogInformation("Iniciando envío automático de reportes {Year}-{Month}", year, month);

        // 1. Formato 606 (Compras)
        try
        {
            var format606 = await _format606Service.GenerateAsync(year, month);
            var response606 = await _dgiiService.SubmitFormat606Async(format606);
            result.Format606 = new ReportSubmissionResult
            {
                Success = response606.Success,
                TrackingNumber = response606.TrackingNumber,
                SubmittedAt = DateTime.UtcNow
            };
            _logger.LogInformation("Formato 606 enviado: {Success}", response606.Success);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando Formato 606");
            result.Format606 = new ReportSubmissionResult { Success = false, Error = ex.Message };
        }

        // 2. Formato 609 (Compras Exterior)
        try
        {
            var format609 = await _format609Service.GenerateAsync(year, month);
            if (format609.HasRecords)
            {
                var response609 = await _dgiiService.SubmitFormat609Async(format609);
                result.Format609 = new ReportSubmissionResult
                {
                    Success = response609.Success,
                    TrackingNumber = response609.TrackingNumber,
                    SubmittedAt = DateTime.UtcNow
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando Formato 609");
            result.Format609 = new ReportSubmissionResult { Success = false, Error = ex.Message };
        }

        // 3. IT-1 (ITBIS)
        try
        {
            var it1Data = await _it1Service.CalculateAsync(year, month);
            var responseIT1 = await _dgiiService.SubmitIT1Async(it1Data);
            result.IT1 = new ReportSubmissionResult
            {
                Success = responseIT1.Success,
                TrackingNumber = responseIT1.TrackingNumber,
                AmountToPay = it1Data.ITBISToPay,
                SubmittedAt = DateTime.UtcNow
            };
            _logger.LogInformation("IT-1 enviado: ITBIS a pagar = {Amount}", it1Data.ITBISToPay);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando IT-1");
            result.IT1 = new ReportSubmissionResult { Success = false, Error = ex.Message };
        }

        // 4. IR-17 (Retenciones)
        try
        {
            var ir17Data = await _ir17Service.GenerateAsync(year, month);
            if (ir17Data.TotalWithheld > 0)
            {
                var responseIR17 = await _dgiiService.SubmitIR17Async(ir17Data);
                result.IR17 = new ReportSubmissionResult
                {
                    Success = responseIR17.Success,
                    TrackingNumber = responseIR17.TrackingNumber,
                    AmountToPay = ir17Data.TotalWithheld,
                    SubmittedAt = DateTime.UtcNow
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando IR-17");
            result.IR17 = new ReportSubmissionResult { Success = false, Error = ex.Message };
        }

        // 5. Notificar resultado
        await _notificationService.SendMonthlyReportSummaryAsync(result);

        return result;
    }
}
```

### 8.3 Servicio de Envío a DGII (Web Services)

```csharp
// ECFService.Infrastructure/Services/DGIIReportWebService.cs

public class DGIIReportWebService : IDGIIReportWebService
{
    private readonly HttpClient _httpClient;
    private readonly IXmlSignatureService _signatureService;
    private readonly DGIISettings _settings;

    private const string BASE_URL_PROD = "https://dgii.gov.do/webservices/";
    private const string BASE_URL_TEST = "https://dgii.gov.do/testwebservices/";

    /// <summary>
    /// Enviar Formato 606 electrónicamente
    /// </summary>
    public async Task<DGIISubmissionResponse> SubmitFormat606Async(Format606Data data)
    {
        var url = GetBaseUrl() + "WSFormatos606/EnviarFormato606";

        var request = new Format606Request
        {
            RNCDeclarante = "133325901", // OKLA
            Periodo = $"{data.Year}{data.Month:D2}",
            CantidadRegistros = data.Records.Count,
            TotalMontoFacturado = data.TotalAmount,
            TotalITBIS = data.TotalITBIS,
            Registros = data.Records.Select(MapToFormat606Record).ToList()
        };

        var signedXml = await _signatureService.SignAsync(request.ToXml());

        return await SendToWebServiceAsync(url, signedXml);
    }

    /// <summary>
    /// Enviar IT-1 (Declaración ITBIS) electrónicamente
    /// </summary>
    public async Task<DGIISubmissionResponse> SubmitIT1Async(IT1Data data)
    {
        var url = GetBaseUrl() + "WSIT1/EnviarDeclaracionIT1";

        var request = new IT1Request
        {
            RNC = "133325901",
            Periodo = $"{data.Year}{data.Month:D2}",
            // Ventas
            VentasBienesGravados = 0, // OKLA no vende bienes físicos
            VentasServiciosGravados = data.TotalSalesBeforeTax,
            ITBISCobrado = data.ITBISCollected,
            // Compras
            ComprasBienesGravados = data.PurchasesGoods,
            ComprasServiciosGravados = data.PurchasesServices,
            ITBISPagado = data.ITBISPaid,
            // Cálculo
            ITBISNeto = data.ITBISCollected - data.ITBISPaid,
            ITBISAPagar = Math.Max(0, data.ITBISCollected - data.ITBISPaid),
            SaldoAFavor = Math.Max(0, data.ITBISPaid - data.ITBISCollected)
        };

        var signedXml = await _signatureService.SignAsync(request.ToXml());

        return await SendToWebServiceAsync(url, signedXml);
    }

    /// <summary>
    /// Enviar IR-17 (Retenciones) electrónicamente
    /// </summary>
    public async Task<DGIISubmissionResponse> SubmitIR17Async(IR17Data data)
    {
        var url = GetBaseUrl() + "WSIR17/EnviarDeclaracionIR17";

        var request = new IR17Request
        {
            RNCAgente = "133325901",
            Periodo = $"{data.Year}{data.Month:D2}",
            // Retenciones realizadas
            RetencionesServicios = data.WithholdingsServices, // 10%
            RetencionesAlquileres = data.WithholdingsRent,     // 10%
            TotalRetenido = data.TotalWithheld,
            // Detalle por proveedor
            Detalle = data.Details.Select(d => new IR17DetailRecord
            {
                RNCRetenido = d.SupplierRNC,
                NombreRetenido = d.SupplierName,
                TipoRetencion = d.Type,
                MontoSujeto = d.BaseAmount,
                MontoRetenido = d.WithheldAmount
            }).ToList()
        };

        var signedXml = await _signatureService.SignAsync(request.ToXml());

        return await SendToWebServiceAsync(url, signedXml);
    }

    /// <summary>
    /// Enviar Formato 609 (Compras Exterior) electrónicamente
    /// </summary>
    public async Task<DGIISubmissionResponse> SubmitFormat609Async(Format609Data data)
    {
        var url = GetBaseUrl() + "WSFormatos609/EnviarFormato609";

        var request = new Format609Request
        {
            RNCDeclarante = "133325901",
            Periodo = $"{data.Year}{data.Month:D2}",
            CantidadRegistros = data.Records.Count,
            TotalMontoPagado = data.TotalAmount,
            // Detalle de pagos internacionales
            Registros = data.Records.Select(r => new Format609Record
            {
                IdentificacionProveedor = r.ProviderIdentification,
                NombreProveedor = r.ProviderName,
                PaisProveedor = r.Country,
                FechaPago = r.PaymentDate,
                TipoServicio = r.ServiceType,
                MontoUSD = r.AmountUSD,
                MontoDOP = r.AmountDOP,
                TasaCambio = r.ExchangeRate
            }).ToList()
        };

        var signedXml = await _signatureService.SignAsync(request.ToXml());

        return await SendToWebServiceAsync(url, signedXml);
    }

    private async Task<DGIISubmissionResponse> SendToWebServiceAsync(string url, string signedXml)
    {
        try
        {
            var content = new StringContent(signedXml, Encoding.UTF8, "application/xml");
            var response = await _httpClient.PostAsync(url, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            return ParseResponse(responseBody);
        }
        catch (Exception ex)
        {
            return new DGIISubmissionResponse
            {
                Success = false,
                ErrorCode = "CONNECTION_ERROR",
                ErrorMessage = ex.Message
            };
        }
    }
}
```

### 8.4 Job Programado para Envío Mensual

```csharp
// ECFService.Api/Jobs/MonthlyReportSubmissionJob.cs

public class MonthlyReportSubmissionJob : IJob
{
    private readonly IAutomaticReportSubmissionService _submissionService;
    private readonly ILogger<MonthlyReportSubmissionJob> _logger;

    /// <summary>
    /// Se ejecuta el día 10 de cada mes a las 8:00 AM
    /// Cron: 0 8 10 * *
    /// </summary>
    public async Task Execute(IJobExecutionContext context)
    {
        // Enviar reportes del mes anterior
        var targetDate = DateTime.Today.AddMonths(-1);
        var year = targetDate.Year;
        var month = targetDate.Month;

        _logger.LogInformation("Ejecutando envío automático de reportes para {Year}-{Month}", year, month);

        var result = await _submissionService.SubmitAllMonthlyReportsAsync(year, month);

        _logger.LogInformation(
            "Envío completado: 606={F606}, 609={F609}, IT1={IT1}, IR17={IR17}",
            result.Format606?.Success,
            result.Format609?.Success,
            result.IT1?.Success,
            result.IR17?.Success
        );
    }
}

// Configuración en Program.cs
services.AddQuartz(q =>
{
    q.AddJob<MonthlyReportSubmissionJob>(opts => opts.WithIdentity("monthly-report-submission"));
    q.AddTrigger(opts => opts
        .ForJob("monthly-report-submission")
        .WithIdentity("monthly-report-submission-trigger")
        .WithCronSchedule("0 8 10 * * ?")); // Día 10, 8:00 AM
});
```

### 8.5 Resumen de Automatización

```
┌─────────────────────────────────────────────────────────────────────────┐
│              RESUMEN: ESFUERZO MANUAL CON e-CF                          │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  REPORTE          │ ANTES (NCF)      │ AHORA (e-CF)                    │
│  ─────────────────┼──────────────────┼───────────────────────────────  │
│                   │                  │                                  │
│  Emisión factura  │ 5 min/factura    │ 0 segundos (automático)         │
│  Formato 607      │ 2-4 horas/mes    │ 5 min verificación              │
│  Formato 608      │ 1 hora/mes       │ 0 min (automático)              │
│  Formato 606      │ 2-4 horas/mes    │ 10 min revisión                 │
│  Formato 609      │ 1-2 horas/mes    │ 0 min (automático)              │
│  IT-1 ITBIS       │ 1-2 horas/mes    │ 5 min verificación              │
│  IR-17 Retenc.    │ 1-2 horas/mes    │ 5 min verificación              │
│  ─────────────────┼──────────────────┼───────────────────────────────  │
│  TOTAL MENSUAL    │ 10-15 horas      │ ~30 minutos                     │
│                   │                  │                                  │
│  AHORRO: 95% DEL TIEMPO 🎉                                             │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 9. MONITOREO Y ALERTAS

### 9.1 Dashboard de e-CF

```typescript
// frontend/web/src/pages/admin/ECFDashboard.tsx

export const ECFDashboard = () => {
  const { data: stats } = useQuery({
    queryKey: ['ecf-stats'],
    queryFn: () => ecfService.getStats()
  });

  return (
    <div className="p-6 space-y-6">
      <h1 className="text-3xl font-bold">Facturación Electrónica</h1>

      {/* Estadísticas del día */}
      <div className="grid grid-cols-4 gap-4">
        <StatCard
          title="e-CF Emitidos Hoy"
          value={stats?.todayCount}
          icon={<FileText />}
        />
        <StatCard
          title="Aprobados"
          value={stats?.approvedCount}
          icon={<CheckCircle className="text-green-500" />}
        />
        <StatCard
          title="Rechazados"
          value={stats?.rejectedCount}
          icon={<XCircle className="text-red-500" />}
          alert={stats?.rejectedCount > 0}
        />
        <StatCard
          title="Monto Total"
          value={formatCurrency(stats?.totalAmount)}
          icon={<DollarSign />}
        />
      </div>

      {/* Alertas */}
      {stats?.rejectedCount > 0 && (
        <Alert variant="destructive">
          <AlertTitle>e-CF Rechazados</AlertTitle>
          <AlertDescription>
            Hay {stats.rejectedCount} e-CF rechazados por DGII que requieren atención.
            <Button variant="link" onClick={() => navigate('/admin/ecf/rejected')}>
              Ver detalles
            </Button>
          </AlertDescription>
        </Alert>
      )}

      {/* Lista de e-CF recientes */}
      <Card>
        <CardHeader>
          <CardTitle>Últimos e-CF Emitidos</CardTitle>
        </CardHeader>
        <CardContent>
          <ECFTable data={stats?.recentECFs} />
        </CardContent>
      </Card>
    </div>
  );
};
```

---

## 📋 CHECKLIST DE IMPLEMENTACIÓN

### Pre-requisitos

- [ ] Certificado digital obtenido
- [ ] Acceso a ambiente de pruebas DGII
- [ ] Credenciales de Web Services

### Backend

- [ ] ECFService microservicio creado
- [ ] Entidades y DTOs definidos
- [ ] Repositorios implementados
- [ ] Generador de XML implementado
- [ ] Firma digital implementada
- [ ] Integración DGII implementada
- [ ] Generador de PDF con QR
- [ ] Event handlers para PaymentCompleted

### Base de Datos

- [ ] Tablas creadas
- [ ] Secuencias inicializadas
- [ ] Índices creados

### Frontend

- [ ] Dashboard de e-CF
- [ ] Visualizador de e-CF
- [ ] Gestión de rechazados
- [ ] Reportes

### Pruebas

- [ ] 20+ E31 enviados a DGII pruebas
- [ ] 20+ E32 enviados a DGII pruebas
- [ ] 10+ E34 enviados a DGII pruebas
- [ ] Flujos de error probados
- [ ] Certificación DGII obtenida

### Producción

- [ ] URLs de producción configuradas
- [ ] Monitoreo activo
- [ ] Alertas configuradas

---

**Documento creado:** Enero 25, 2026  
**Próxima revisión:** Antes de implementación  
**Responsable:** Equipo de Desarrollo + Contador
