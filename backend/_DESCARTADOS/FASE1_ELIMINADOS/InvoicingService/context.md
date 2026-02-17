# InvoicingService - Context Documentation

## 📋 INFORMACIÓN GENERAL

- **Nombre del Servicio:** InvoicingService
- **Puerto en Desarrollo:** 5028
- **Estado:** 🚧 **EN DESARROLLO - NO DESPLEGADO**
- **Base de Datos:** PostgreSQL (`invoicingservice`)
- **Imagen Docker:** Local only

### Propósito
Servicio de facturación y emisión de comprobantes fiscales (NCF en República Dominicana). Genera facturas, recibos, notas de crédito y gestiona la numeración de comprobantes según DGII.

---

## 🏗️ ARQUITECTURA

```
InvoicingService/
├── InvoicingService.Api/
│   ├── Controllers/
│   │   ├── InvoicesController.cs
│   │   ├── TaxReceiptsController.cs
│   │   └── NCFController.cs
│   └── Program.cs
├── InvoicingService.Application/
│   └── Services/
│       ├── InvoiceGeneratorService.cs
│       └── NCFManagerService.cs
├── InvoicingService.Domain/
│   ├── Entities/
│   │   ├── Invoice.cs
│   │   ├── InvoiceItem.cs
│   │   ├── TaxReceipt.cs
│   │   └── NCFSequence.cs
│   └── Enums/
│       ├── InvoiceType.cs
│       └── TaxDocumentType.cs
└── InvoicingService.Infrastructure/
```

---

## 📦 ENTIDADES PRINCIPALES

### Invoice
```csharp
public class Invoice
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; }      // INV-2026-001234
    
    // Tipo de comprobante (República Dominicana - DGII)
    public TaxDocumentType DocumentType { get; set; } // NCF para Crédito Fiscal, Consumo, etc.
    public string? NCF { get; set; }               // B0100000001 (Comprobante Fiscal)
    
    // Cliente
    public Guid? CustomerId { get; set; }
    public string CustomerName { get; set; }
    public string? CustomerRNC { get; set; }       // RNC (Registro Nacional Contribuyente)
    public string? CustomerAddress { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    
    // Empresa emisora
    public string CompanyName { get; set; }
    public string CompanyRNC { get; set; }
    public string CompanyAddress { get; set; }
    
    // Fechas
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    
    // Montos
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }         // ITBIS 18%
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "DOP";
    
    // Estado
    public InvoiceStatus Status { get; set; }      // Draft, Issued, Paid, Cancelled, Overdue
    public DateTime? PaidAt { get; set; }
    public string? PaymentMethod { get; set; }
    
    // Relación con transacción
    public Guid? TransactionId { get; set; }       // VehicleSale, PropertySale, etc.
    public string? TransactionType { get; set; }
    
    // Archivos
    public string? PdfUrl { get; set; }
    public string? XmlUrl { get; set; }            // Para envío a DGII
    
    // Navegación
    public ICollection<InvoiceItem> Items { get; set; }
}
```

### InvoiceItem
```csharp
public class InvoiceItem
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
    
    // Item
    public string Description { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Amount { get; set; }            // Quantity * UnitPrice
    
    // Tax
    public decimal TaxRate { get; set; } = 0.18m;  // ITBIS 18%
    public decimal TaxAmount { get; set; }
    
    // Discount
    public decimal DiscountPercent { get; set; }
    public decimal DiscountAmount { get; set; }
    
    // Total
    public decimal TotalAmount { get; set; }
    
    public Invoice Invoice { get; set; }
}
```

### TaxReceipt (Recibo de pago)
```csharp
public class TaxReceipt
{
    public Guid Id { get; set; }
    public string ReceiptNumber { get; set; }
    public string? NCF { get; set; }               // Comprobante de pago
    
    // Factura asociada
    public Guid InvoiceId { get; set; }
    public Invoice Invoice { get; set; }
    
    // Pago
    public DateTime PaymentDate { get; set; }
    public decimal AmountPaid { get; set; }
    public string PaymentMethod { get; set; }      // Cash, Card, Transfer, Check
    public string? PaymentReference { get; set; }
    
    // Cliente
    public string PayerName { get; set; }
    public string? PayerRNC { get; set; }
    
    // Archivos
    public string? PdfUrl { get; set; }
    
    public DateTime CreatedAt { get; set; }
}
```

### NCFSequence (Numeración de Comprobantes Fiscales)
```csharp
public class NCFSequence
{
    public Guid Id { get; set; }
    
    // Tipo de comprobante
    public TaxDocumentType DocumentType { get; set; }
    
    // Secuencia autorizada por DGII
    public string SeriesPrefix { get; set; }       // B01, B02, B14, B15, etc.
    public long StartNumber { get; set; }          // 00000001
    public long EndNumber { get; set; }            // 00050000
    public long CurrentNumber { get; set; }        // Último usado
    
    // Vigencia
    public DateTime ValidFrom { get; set; }
    public DateTime ValidUntil { get; set; }
    public bool IsActive { get; set; }
    
    // Alerta
    public int AlertThreshold { get; set; } = 100; // Alertar cuando queden 100
    public bool AlertSent { get; set; }
}
```

---

## 📡 ENDPOINTS (Propuestos)

### Facturas
- `POST /api/invoices` - Crear factura (draft)
  ```json
  {
    "customerId": "uuid",
    "customerName": "Juan Pérez",
    "customerRNC": "131-12345-6",
    "documentType": "CreditoFiscal",
    "items": [
      {
        "description": "Toyota Corolla 2020",
        "quantity": 1,
        "unitPrice": 1500000,
        "taxRate": 0.18
      }
    ]
  }
  ```
- `GET /api/invoices/{id}` - Ver factura
- `PUT /api/invoices/{id}` - Actualizar factura (solo si status=Draft)
- `POST /api/invoices/{id}/issue` - Emitir factura (asigna NCF, genera PDF)
- `POST /api/invoices/{id}/cancel` - Anular factura
- `GET /api/invoices` - Listar facturas (con filtros)
- `GET /api/invoices/{id}/pdf` - Descargar PDF

### Recibos
- `POST /api/receipts` - Crear recibo de pago
- `GET /api/receipts/{id}` - Ver recibo
- `GET /api/receipts/{id}/pdf` - Descargar PDF

### NCF Management
- `GET /api/ncf/sequences` - Ver secuencias de NCF
- `POST /api/ncf/sequences` - Registrar nueva secuencia
- `GET /api/ncf/sequences/status` - Estado de uso de secuencias
- `GET /api/ncf/next-available` - Obtener próximo NCF disponible

---

## 💡 FUNCIONALIDADES PLANEADAS

### Tipos de Comprobantes Fiscales (República Dominicana)

| Código | Descripción                    | Uso                         |
| ------ | ------------------------------ | --------------------------- |
| B01    | Factura de Crédito Fiscal      | Empresas con RNC            |
| B02    | Factura de Consumo             | Consumidor final            |
| B14    | Nota de Crédito                | Devoluciones, descuentos    |
| B15    | Nota de Débito                 | Cargos adicionales          |
| B16    | Factura de Regímenes Especiales| Contribuyentes especiales   |

### Generación de PDF
Template con:
- Logo de la empresa
- Datos fiscales (RNC, dirección)
- Datos del cliente
- Tabla de items
- Subtotal, ITBIS (18%), Total
- NCF visible
- Código QR (opcional)

### Cálculo de ITBIS
```csharp
public class TaxCalculator
{
    public decimal CalculateITBIS(decimal amount)
    {
        const decimal ITBIS_RATE = 0.18m;  // 18%
        return Math.Round(amount * ITBIS_RATE, 2);
    }
    
    public decimal CalculateTotalWithTax(decimal subtotal)
    {
        var itbis = CalculateITBIS(subtotal);
        return subtotal + itbis;
    }
}
```

### Reportes DGII
Generar reportes mensuales para DGII:
- 606 (Compras)
- 607 (Ventas)
- 608 (Cancelaciones)

En formato TXT según especificaciones de DGII.

### Alertas de NCF
- Cuando queden < 100 comprobantes: enviar alerta
- Cuando secuencia esté por vencer: notificar 30 días antes
- NCF vencido: bloquear emisión de facturas

### Facturación Recurrente
Para subscriptions del BillingService:
- Auto-generar factura mensual
- Enviar por email
- Marcar como pagada cuando BillingService confirme pago

---

## 🔗 INTEGRACIÓN CON OTROS SERVICIOS

### BillingService
- Cuando se completa pago → crear factura automáticamente
- Sincronizar estado de pagos

### VehiclesSaleService / PropertiesSaleService
- Al cerrar venta → generar factura
- Incluir detalles del vehículo/propiedad

### MediaService
- Guardar PDFs generados en S3
- URL pública para descargar facturas

### NotificationService
- Enviar factura por email al cliente
- Recordatorios de facturas vencidas

### UserService
- Obtener datos fiscales del cliente (RNC)
- Historial de facturas por usuario

---

## 🎯 BUSINESS RULES

### Numeración de NCF
- Cada tipo de comprobante tiene su propia secuencia
- No se puede reutilizar NCF anulado
- Al anular factura: generar Nota de Crédito con nuevo NCF

### Impuestos
- ITBIS estándar: 18%
- Algunos items pueden estar exentos (verificar con contabilidad)

### Validación RNC
- RNC debe tener formato: XXX-XXXXX-X (9 dígitos)
- Para facturas B01 (Crédito Fiscal): RNC obligatorio
- Para facturas B02 (Consumo): RNC opcional

### Anulaciones
- Solo se puede anular factura no pagada
- Generar Nota de Crédito (B14) por el monto total
- Notificar a DGII dentro de 24h

---

## 🔄 EVENTOS PUBLICADOS (RabbitMQ)

### InvoiceIssued
```json
{
  "invoiceId": "uuid",
  "invoiceNumber": "INV-2026-001234",
  "ncf": "B0100000001",
  "customerId": "uuid",
  "totalAmount": 1770000,
  "timestamp": "2026-01-07T10:30:00Z"
}
```

### InvoicePaid
```json
{
  "invoiceId": "uuid",
  "paidAt": "2026-01-07T14:00:00Z",
  "amountPaid": 1770000,
  "paymentMethod": "Transfer"
}
```

---

**Estado:** 🚧 EN DESARROLLO - No desplegado en producción  
**Versión:** 0.1.0  
**Región:** República Dominicana (DGII)
