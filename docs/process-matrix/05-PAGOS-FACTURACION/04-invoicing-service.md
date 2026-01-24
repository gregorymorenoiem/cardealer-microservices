# 🧾 Invoicing Service - Facturación DGII - Matriz de Procesos

> **Servicio:** InvoicingService  
> **Puerto:** 5046  
> **Última actualización:** Enero 21, 2026  
> **Estado:** 🟢 ACTIVO

---

## 📊 Resumen de Implementación

| Componente                     | Total | Implementado | Pendiente | Estado         |
| ------------------------------ | ----- | ------------ | --------- | -------------- |
| **Controllers**                | 1     | 0            | 1         | 🔴 Pendiente   |
| **INV-NCF-\*** (Comprobantes)  | 4     | 0            | 4         | 🔴 Pendiente   |
| **INV-GEN-\*** (Generación)    | 4     | 0            | 4         | 🔴 Pendiente   |
| **INV-SEND-\*** (Envío)        | 3     | 0            | 3         | 🔴 Pendiente   |
| **INV-VOID-\*** (Anulación)    | 3     | 0            | 3         | 🔴 Pendiente   |
| **INV-REP-\*** (Reportes DGII) | 4     | 0            | 4         | 🔴 Pendiente   |
| **Tests**                      | 0     | 0            | 18        | 🔴 Pendiente   |
| **TOTAL**                      | 19    | 0            | 19        | 🔴 0% Completo |

---

## 1. Información General

### 1.1 Descripción

Sistema de facturación electrónica conforme a las regulaciones de la DGII (Dirección General de Impuestos Internos) de República Dominicana. Genera NCF (Número de Comprobante Fiscal) para cada transacción y cumple con la Ley 253-12.

### 1.2 Marco Regulatorio

| Ley/Norma     | Descripción                     |
| ------------- | ------------------------------- |
| Ley 253-12    | Ley sobre Comprobantes Fiscales |
| Norma 06-2018 | Factura Electrónica             |
| Norma 08-2019 | Secuencias de NCF               |
| ITBIS         | 18% sobre servicios digitales   |

### 1.3 Dependencias

| Servicio            | Propósito             |
| ------------------- | --------------------- |
| BillingService      | Pagos de origen       |
| UserService         | Datos del cliente     |
| DealerService       | Datos del vendedor    |
| NotificationService | Envío de facturas     |
| DGII API            | Validación de RNC/NCF |
| MediaService        | Almacenamiento PDFs   |

---

## 2. Endpoints API

### 2.1 InvoicesController

| Método | Endpoint                     | Descripción       | Auth | Roles         |
| ------ | ---------------------------- | ----------------- | ---- | ------------- |
| `GET`  | `/api/invoices`              | Listar facturas   | ✅   | User, Dealer  |
| `GET`  | `/api/invoices/{id}`         | Obtener factura   | ✅   | Owner         |
| `GET`  | `/api/invoices/{id}/pdf`     | Descargar PDF     | ✅   | Owner         |
| `POST` | `/api/invoices`              | Crear factura     | ✅   | System, Admin |
| `POST` | `/api/invoices/{id}/send`    | Enviar por email  | ✅   | Owner         |
| `POST` | `/api/invoices/{id}/void`    | Anular factura    | ✅   | Admin         |
| `GET`  | `/api/invoices/ncf-sequence` | Ver secuencia NCF | ✅   | Admin         |

### 2.2 CreditNotesController

| Método | Endpoint                 | Descripción             | Auth | Roles        |
| ------ | ------------------------ | ----------------------- | ---- | ------------ |
| `GET`  | `/api/credit-notes`      | Listar notas de crédito | ✅   | User, Dealer |
| `GET`  | `/api/credit-notes/{id}` | Obtener nota            | ✅   | Owner        |
| `POST` | `/api/credit-notes`      | Crear nota crédito      | ✅   | Admin        |

### 2.3 DGIIController

| Método | Endpoint                       | Descripción         | Auth | Roles |
| ------ | ------------------------------ | ------------------- | ---- | ----- |
| `GET`  | `/api/dgii/validate-rnc/{rnc}` | Validar RNC         | ✅   | User  |
| `GET`  | `/api/dgii/validate-ncf/{ncf}` | Validar NCF         | ✅   | Admin |
| `POST` | `/api/dgii/report/606`         | Generar reporte 606 | ✅   | Admin |
| `POST` | `/api/dgii/report/607`         | Generar reporte 607 | ✅   | Admin |

---

## 3. Entidades y Enums

### 3.1 InvoiceType (Enum)

```csharp
public enum InvoiceType
{
    // Comprobantes Fiscales
    ConsumidorFinal = 01,       // B01 - Factura de Consumo
    CreditoFiscal = 02,         // B02 - Factura de Crédito Fiscal
    NotaDebito = 03,            // B03 - Nota de Débito
    NotaCredito = 04,           // B04 - Nota de Crédito
    ComprobanteCompras = 11,    // B11 - Comprobante de Compras
    GastosExterior = 13,        // B13 - Gastos del Exterior
    Gubernamental = 14,         // B14 - Gubernamental
    RegimenEspecial = 15,       // B15 - Régimen Especial
    FacturaExportacion = 16,    // B16 - Exportación
    FacturaUnica = 17           // B17 - Factura Única
}
```

### 3.2 InvoiceStatus (Enum)

```csharp
public enum InvoiceStatus
{
    Draft = 0,                  // Borrador
    Issued = 1,                 // Emitida
    Sent = 2,                   // Enviada al cliente
    Paid = 3,                   // Pagada
    PartiallyPaid = 4,          // Parcialmente pagada
    Overdue = 5,                // Vencida
    Voided = 6,                 // Anulada
    Cancelled = 7               // Cancelada con NC
}
```

### 3.3 Invoice (Entidad)

```csharp
public class Invoice
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; }    // Interno OKLA-2026-00001
    public string NCF { get; set; }              // B0100000001
    public InvoiceType Type { get; set; }
    public InvoiceStatus Status { get; set; }

    // Referencias
    public Guid? PaymentId { get; set; }
    public Guid? SubscriptionId { get; set; }
    public Guid? OrderId { get; set; }

    // Emisor (OKLA o Dealer)
    public Guid IssuerId { get; set; }
    public string IssuerRNC { get; set; }
    public string IssuerName { get; set; }
    public string IssuerAddress { get; set; }

    // Receptor (Cliente)
    public Guid? CustomerId { get; set; }
    public string? CustomerRNC { get; set; }       // Si tiene
    public string CustomerName { get; set; }
    public string CustomerEmail { get; set; }
    public string? CustomerAddress { get; set; }
    public string? CustomerPhone { get; set; }

    // Montos
    public string Currency { get; set; }          // DOP, USD
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }        // ITBIS 18%
    public decimal DiscountAmount { get; set; }
    public decimal Total { get; set; }

    // Items
    public List<InvoiceItem> Items { get; set; }

    // Fechas
    public DateTime IssueDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? PaidAt { get; set; }

    // PDF
    public string? PdfUrl { get; set; }
    public string? PdfHash { get; set; }          // SHA-256

    // Notas de Crédito relacionadas
    public List<CreditNote>? CreditNotes { get; set; }

    // DGII
    public bool ReportedToDGII { get; set; }
    public DateTime? ReportedAt { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime? VoidedAt { get; set; }
    public string? VoidReason { get; set; }
}
```

### 3.4 InvoiceItem (Entidad)

```csharp
public class InvoiceItem
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }

    public string Description { get; set; }
    public string? ProductCode { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal TaxRate { get; set; }          // 0.18 = 18%
    public decimal TaxAmount { get; set; }
    public decimal Total { get; set; }
}
```

### 3.5 NCFSequence (Entidad)

```csharp
public class NCFSequence
{
    public Guid Id { get; set; }
    public InvoiceType Type { get; set; }
    public string Prefix { get; set; }            // B01, B02, etc.
    public long CurrentNumber { get; set; }
    public long StartNumber { get; set; }
    public long EndNumber { get; set; }           // Autorizado por DGII
    public DateTime AuthorizationDate { get; set; }
    public DateTime ExpirationDate { get; set; }
    public bool IsActive { get; set; }
}
```

---

## 4. Procesos Detallados

### 4.1 INV-001: Generar Factura Automática

| Campo       | Valor                    |
| ----------- | ------------------------ |
| **ID**      | INV-001                  |
| **Nombre**  | Generar Factura por Pago |
| **Actor**   | Sistema                  |
| **Trigger** | Evento payment.completed |

#### Flujo del Proceso

| Paso | Acción                  | Sistema             | Validación        |
| ---- | ----------------------- | ------------------- | ----------------- |
| 1    | Recibir evento de pago  | RabbitMQ            | payment.completed |
| 2    | Obtener datos del pago  | BillingService      | Monto, método     |
| 3    | Determinar tipo factura | InvoicingService    | B01 o B02         |
| 4    | Si cliente tiene RNC    | Check               | Validar en DGII   |
| 5    | Obtener próximo NCF     | InvoicingService    | De secuencia      |
| 6    | Calcular ITBIS          | InvoicingService    | 18% si aplica     |
| 7    | Crear factura           | Database            | Status = Issued   |
| 8    | Generar PDF             | PdfService          | Con template      |
| 9    | Subir PDF               | MediaService        | S3                |
| 10   | Enviar por email        | NotificationService | Adjunto           |
| 11   | Publicar evento         | RabbitMQ            | invoice.created   |

#### Cálculo de ITBIS

```csharp
public InvoiceCalculation CalculateTotals(decimal subtotal, bool applyTax)
{
    decimal itbisRate = 0.18m;
    decimal taxAmount = applyTax ? subtotal * itbisRate : 0;
    decimal total = subtotal + taxAmount;

    return new InvoiceCalculation
    {
        Subtotal = subtotal,
        TaxRate = applyTax ? itbisRate : 0,
        TaxAmount = taxAmount,
        Total = total
    };
}
```

---

### 4.2 INV-002: Generar NCF

| Campo       | Valor                                |
| ----------- | ------------------------------------ |
| **ID**      | INV-002                              |
| **Nombre**  | Generar Número de Comprobante Fiscal |
| **Actor**   | Sistema                              |
| **Trigger** | Creación de factura                  |

#### Flujo del Proceso

| Paso | Acción                   | Sistema          | Validación      |
| ---- | ------------------------ | ---------------- | --------------- |
| 1    | Determinar tipo NCF      | InvoicingService | B01, B02, etc.  |
| 2    | Obtener secuencia activa | Database         | IsActive = true |
| 3    | Validar no expirada      | InvoicingService | ExpirationDate  |
| 4    | Validar disponibilidad   | InvoicingService | Current < End   |
| 5    | Incrementar contador     | Database         | Thread-safe     |
| 6    | Formatear NCF            | InvoicingService | B0100000001     |
| 7    | Retornar NCF             | Response         | String          |
| 8    | Si agotándose            | Alert            | Notificar admin |

#### Formato NCF

```
B01 00000001
│││ ││││││││
│││ └──────── Secuencia (8 dígitos)
│└────────── Tipo de comprobante (2 dígitos)
└─────────── Prefijo "B"
```

---

### 4.3 INV-003: Emitir Nota de Crédito

| Campo       | Valor                  |
| ----------- | ---------------------- |
| **ID**      | INV-003                |
| **Nombre**  | Emitir Nota de Crédito |
| **Actor**   | Admin                  |
| **Trigger** | POST /api/credit-notes |

#### Flujo del Proceso

| Paso | Acción                   | Sistema             | Validación            |
| ---- | ------------------------ | ------------------- | --------------------- |
| 1    | Admin solicita NC        | Dashboard           | Para factura X        |
| 2    | Obtener factura original | Database            | Status = Issued/Paid  |
| 3    | Validar no tiene NC      | InvoicingService    | CreditNotes empty     |
| 4    | Ingresar monto y razón   | Frontend            | <= monto original     |
| 5    | Generar NCF tipo B04     | InvoicingService    | Secuencia NC          |
| 6    | Crear nota de crédito    | Database            | Referencia a factura  |
| 7    | Actualizar factura       | Database            | Cancelled si es total |
| 8    | Generar PDF              | PdfService          | Template NC           |
| 9    | Enviar al cliente        | NotificationService | Email                 |
| 10   | Publicar evento          | RabbitMQ            | credit_note.created   |

#### Request

```json
{
  "invoiceId": "uuid",
  "amount": 12900.0,
  "reason": "Devolución de suscripción - Cliente canceló",
  "items": [
    {
      "description": "Reembolso Suscripción Plan Pro",
      "quantity": 1,
      "unitPrice": 12900.0
    }
  ]
}
```

---

### 4.4 INV-004: Validar RNC en DGII

| Campo       | Valor                            |
| ----------- | -------------------------------- |
| **ID**      | INV-004                          |
| **Nombre**  | Validar RNC contra DGII          |
| **Actor**   | Sistema/Usuario                  |
| **Trigger** | GET /api/dgii/validate-rnc/{rnc} |

#### Flujo del Proceso

| Paso | Acción             | Sistema          | Validación          |
| ---- | ------------------ | ---------------- | ------------------- |
| 1    | Recibir RNC        | InvoicingService | 9 u 11 dígitos      |
| 2    | Verificar formato  | InvoicingService | Regex               |
| 3    | Verificar caché    | Redis            | TTL 24h             |
| 4    | Si en caché        | Response         | Retornar datos      |
| 5    | Consultar API DGII | HTTP             | dgii.gov.do         |
| 6    | Parsear respuesta  | InvoicingService | JSON/XML            |
| 7    | Guardar en caché   | Redis            | 24h                 |
| 8    | Retornar resultado | Response         | Datos contribuyente |

#### Response

```json
{
  "rnc": "131456789",
  "valid": true,
  "name": "Autos del Caribe SRL",
  "commercialName": "Autos del Caribe",
  "status": "ACTIVO",
  "paymentRegime": "NORMAL",
  "activity": "Venta de vehículos usados"
}
```

---

### 4.5 INV-005: Generar Reporte 606/607

| Campo       | Valor                           |
| ----------- | ------------------------------- |
| **ID**      | INV-005                         |
| **Nombre**  | Generar Reportes DGII           |
| **Actor**   | Admin                           |
| **Trigger** | POST /api/dgii/report/606 o 607 |

#### Reportes DGII

| Reporte | Descripción                   | Contenido          |
| ------- | ----------------------------- | ------------------ |
| 606     | Compras de Bienes y Servicios | Facturas recibidas |
| 607     | Ventas de Bienes y Servicios  | Facturas emitidas  |
| 608     | Comprobantes Anulados         | NCF anulados       |

#### Flujo del Proceso (607)

| Paso | Acción                         | Sistema          | Validación      |
| ---- | ------------------------------ | ---------------- | --------------- |
| 1    | Admin solicita reporte         | Dashboard        | Mes/Año         |
| 2    | Consultar facturas del período | Database         | Por fecha       |
| 3    | Agrupar por tipo NCF           | InvoicingService | B01, B02, etc.  |
| 4    | Calcular totales               | InvoicingService | Subtotal, ITBIS |
| 5    | Formatear según DGII           | InvoicingService | TXT delimitado  |
| 6    | Generar archivo                | InvoicingService | 607MMAAAA.txt   |
| 7    | Retornar para descarga         | Response         | File            |

#### Formato 607

```
RNC|Fecha|NCF|NCF Modificado|Tipo|RNC Cliente|Monto Facturado|ITBIS|Fecha Ret|ITBIS Retenido|ISR Retenido
130123456|20260115|B0100000001||B01|001234567|10000.00|1800.00|||
```

---

## 5. Template de Factura PDF

### 5.1 Estructura

```
┌─────────────────────────────────────────────────────────────────────────┐
│                                                    [LOGO OKLA]          │
│  FACTURA DE VENTA                                                       │
│  ═══════════════                                                        │
│                                                                          │
│  NCF: B0100000001                          Fecha: 21/01/2026            │
│  No. Factura: OKLA-2026-00123                                           │
│                                                                          │
│  ─────────────────────────────────────────────────────────────────────  │
│                                                                          │
│  EMISOR:                           CLIENTE:                             │
│  OKLA Technologies SRL             Juan Pérez                           │
│  RNC: 131-12345-6                  Cédula: 001-1234567-8               │
│  Av. 27 de Febrero #123            Tel: 829-555-0100                   │
│  Santo Domingo, RD                 juan@email.com                       │
│                                                                          │
│  ─────────────────────────────────────────────────────────────────────  │
│                                                                          │
│  DESCRIPCIÓN                           CANT.    P.UNIT.     TOTAL       │
│  ─────────────────────────────────────────────────────────────────────  │
│  Suscripción Plan Pro - Mensual         1     RD$ 12,900   RD$ 12,900  │
│  Periodo: 15/01/2026 - 15/02/2026                                       │
│                                                                          │
│  ─────────────────────────────────────────────────────────────────────  │
│                                                                          │
│                                          SUBTOTAL:    RD$ 10,932.20     │
│                                          ITBIS (18%): RD$  1,967.80     │
│                                          ──────────────────────────     │
│                                          TOTAL:       RD$ 12,900.00     │
│                                                                          │
│  ─────────────────────────────────────────────────────────────────────  │
│                                                                          │
│  Método de pago: Tarjeta de crédito terminada en ****4242               │
│  Fecha de pago: 15/01/2026                                              │
│                                                                          │
│  ─────────────────────────────────────────────────────────────────────  │
│  Documento generado electrónicamente conforme a la Ley 253-12           │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 6. Reglas de Negocio

### 6.1 NCF

| Regla                       | Valor                     |
| --------------------------- | ------------------------- |
| Vigencia secuencia          | 2 años desde autorización |
| Alerta de agotamiento       | Cuando queden < 100       |
| Prefijo para consumidor     | B01                       |
| Prefijo para crédito fiscal | B02                       |
| Prefijo nota de crédito     | B04                       |

### 6.2 Facturación

| Regla                     | Valor                |
| ------------------------- | -------------------- |
| ITBIS estándar            | 18%                  |
| Retención ITBIS a dealers | 30% si > $50,000/mes |
| Retención ISR a dealers   | 10% si > $50,000/mes |
| Vencimiento default       | 30 días              |

### 6.3 Anulaciones

| Regla                      | Valor                      |
| -------------------------- | -------------------------- |
| Periodo máximo para anular | 30 días                    |
| Requiere nota de crédito   | Si ya fue reportada a DGII |
| Autorización requerida     | Admin                      |

---

## 7. Eventos RabbitMQ

| Evento                | Exchange         | Payload                     |
| --------------------- | ---------------- | --------------------------- |
| `invoice.created`     | `billing.events` | `{ invoiceId, ncf, total }` |
| `invoice.sent`        | `billing.events` | `{ invoiceId, email }`      |
| `invoice.paid`        | `billing.events` | `{ invoiceId }`             |
| `invoice.voided`      | `billing.events` | `{ invoiceId, reason }`     |
| `credit_note.created` | `billing.events` | `{ id, invoiceId, amount }` |
| `ncf_sequence.low`    | `billing.alerts` | `{ type, remaining }`       |

---

## 8. Métricas

```
# Facturas
invoices_created_total{type="B01|B02|B04"}
invoices_total_amount{currency="DOP|USD"}
invoices_status_total{status="issued|paid|voided"}

# NCF
ncf_sequences_remaining{type="B01|B02|B04"}
ncf_generated_total{type="..."}

# DGII
dgii_rnc_validations_total{result="valid|invalid"}
dgii_reports_generated_total{type="606|607|608"}

# PDF
invoice_pdf_generated_total
invoice_pdf_generation_duration_seconds
```

---

## 9. Configuración

```json
{
  "Invoicing": {
    "IssuerRNC": "131-12345-6",
    "IssuerName": "OKLA Technologies SRL",
    "IssuerAddress": "Av. 27 de Febrero #123, Santo Domingo",
    "DefaultDueDays": 30,
    "ITBISRate": 0.18,
    "NCFAlertThreshold": 100
  },
  "DGII": {
    "ApiUrl": "https://dgii.gov.do/api/v1",
    "ApiKey": "${DGII_API_KEY}",
    "RNCCacheTTL": 86400
  },
  "Pdf": {
    "TemplatePath": "templates/invoice.html",
    "LogoUrl": "https://okla.com.do/logo.png"
  }
}
```

---

## 📚 Referencias

- [01-billing-service.md](01-billing-service.md) - Facturación
- [02-stripe-payment.md](02-stripe-payment.md) - Pagos Stripe
- [03-azul-payment.md](03-azul-payment.md) - Pagos AZUL
- [DGII](https://dgii.gov.do) - Portal de la DGII
- [Norma 06-2018](https://dgii.gov.do/legislacion) - E-Factura
