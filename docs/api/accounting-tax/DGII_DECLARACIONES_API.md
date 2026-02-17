# 📋 Declaraciones Fiscales DGII

**Categoría:** DGII Tax Declarations  
**Entidad:** Dirección General de Impuestos Internos  
**Uso:** Envío electrónico de declaraciones tributarias  
**Última Actualización:** Enero 15, 2026

---

## 📊 Declaraciones Requeridas

| Formulario | Nombre                   | Frecuencia | Fecha Límite             |
| ---------- | ------------------------ | ---------- | ------------------------ |
| **IT-1**   | ITBIS Mensual            | Mensual    | Día 20 del mes siguiente |
| **IR-17**  | Retenciones              | Mensual    | Día 10 del mes siguiente |
| **IR-2**   | Renta Anual              | Anual      | 31 de Marzo              |
| **606**    | Compras Bienes/Servicios | Mensual    | Día 15 del mes siguiente |
| **607**    | Ventas Bienes/Servicios  | Mensual    | Día 15 del mes siguiente |

---

## 🌐 API Virtual Office DGII

### Endpoints

```http
# Base URL
https://dgii.gov.do/virtual/

# Formato 606 (Compras)
POST /api/formatos/606
Authorization: Bearer {token}
Content-Type: application/json

{
  "rnc": "123456789",
  "periodo": "202601",
  "registros": [
    {
      "rncProveedor": "987654321",
      "tipoId": "1",
      "ncf": "E310000000001",
      "fechaComprobante": "20260115",
      "montoFacturado": 100000.00,
      "itbisFacturado": 18000.00,
      "formaPago": "04"
    }
  ]
}

# Formato 607 (Ventas)
POST /api/formatos/607
{
  "rnc": "123456789",
  "periodo": "202601",
  "registros": [
    {
      "rncComprador": "987654321",
      "ncf": "E310000000002",
      "fechaComprobante": "20260115",
      "montoFacturado": 150000.00,
      "itbisCobrado": 27000.00
    }
  ]
}

# Declaración IT-1 (ITBIS)
POST /api/declaraciones/it1
{
  "rnc": "123456789",
  "periodo": "202601",
  "ventasBrutasFirma": 1500000.00,
  "itbisCobrado": 270000.00,
  "comprasBienes": 800000.00,
  "itbisPagado": 144000.00,
  "itbisNeto": 126000.00
}

# Declaración IR-17 (Retenciones)
POST /api/declaraciones/ir17
{
  "rnc": "123456789",
  "periodo": "202601",
  "retenciones": [
    {
      "tipo": "Servicios",
      "baseRetencion": 100000.00,
      "porcentaje": 10,
      "montoRetenido": 10000.00
    }
  ]
}
```

---

## 💻 Modelos C#

```csharp
namespace AccountingTaxService.Domain.Entities;

public record Formato606Entry(
    string RncProveedor,
    string TipoIdentificacion,
    string Ncf,
    DateTime FechaComprobante,
    decimal MontoFacturado,
    decimal ItbisFacturado,
    FormaPago FormaPago
);

public record Formato607Entry(
    string RncComprador,
    string Ncf,
    DateTime FechaComprobante,
    decimal MontoFacturado,
    decimal ItbisCobrado
);

public record DeclaracionIT1(
    string Rnc,
    string Periodo, // "202601"
    decimal VentasBrutas,
    decimal ItbisCobrado,
    decimal ComprasBienes,
    decimal ItbisPagado,
    decimal ItbisNeto,
    DeclaracionStatus Status
);

public record DeclaracionIR17(
    string Rnc,
    string Periodo,
    List<Retencion> Retenciones,
    decimal TotalRetenido,
    DeclaracionStatus Status
);

public record Retencion(
    TipoRetencion Tipo,
    decimal BaseRetencion,
    decimal Porcentaje,
    decimal MontoRetenido
);

public enum TipoRetencion
{
    Servicios = 10,        // 10%
    Alquileres = 10,       // 10%
    Intereses = 10,        // 10%
    Dividendos = 10,       // 10%
    Premios = 15,          // 15%
    Honorarios = 10,       // 10%
    Comisiones = 10        // 10%
}

public enum FormaPago
{
    Efectivo = 1,
    Transferencia = 2,
    TarjetaCredito = 3,
    Credito = 4,
    Mixto = 5
}

public enum DeclaracionStatus
{
    Draft,
    Submitted,
    Accepted,
    Rejected,
    Paid
}
```

---

## 🔧 Service Interface

```csharp
namespace AccountingTaxService.Domain.Interfaces;

public interface ITaxDeclarationService
{
    // Formatos 606/607
    Task<SubmissionResult> SubmitFormato606Async(
        string periodo, List<Formato606Entry> entries);
    Task<SubmissionResult> SubmitFormato607Async(
        string periodo, List<Formato607Entry> entries);

    // Declaraciones
    Task<DeclaracionIT1> GenerateIT1Async(string periodo);
    Task<SubmissionResult> SubmitIT1Async(DeclaracionIT1 declaracion);
    Task<DeclaracionIR17> GenerateIR17Async(string periodo);
    Task<SubmissionResult> SubmitIR17Async(DeclaracionIR17 declaracion);

    // Consultas
    Task<List<DeclaracionStatus>> GetDeclaracionesAsync(int year);
}

public record SubmissionResult(
    bool Success,
    string? ConfirmationNumber,
    string? ErrorMessage,
    DateTime SubmittedAt
);
```

---

## 🏗️ Service Implementation

```csharp
namespace AccountingTaxService.Infrastructure.Services;

public class DgiiTaxDeclarationService : ITaxDeclarationService
{
    private readonly HttpClient _httpClient;
    private readonly IAccountingService _accounting;
    private readonly ILogger<DgiiTaxDeclarationService> _logger;
    private readonly string _rnc;

    public DgiiTaxDeclarationService(
        HttpClient httpClient,
        IAccountingService accounting,
        IConfiguration config,
        ILogger<DgiiTaxDeclarationService> logger)
    {
        _httpClient = httpClient;
        _accounting = accounting;
        _logger = logger;
        _rnc = config["Company:Rnc"]!;
    }

    public async Task<DeclaracionIT1> GenerateIT1Async(string periodo)
    {
        // Parsear periodo "202601" a fechas
        var year = int.Parse(periodo[..4]);
        var month = int.Parse(periodo[4..]);
        var from = new DateTime(year, month, 1);
        var to = from.AddMonths(1).AddDays(-1);

        // Obtener datos del software contable
        var income = await _accounting.GetIncomeStatementAsync(from, to);
        var invoices = await _accounting.GetInvoicesAsync(from, to);

        var ventasBrutas = invoices.Sum(i => i.Subtotal);
        var itbisCobrado = invoices.Sum(i => i.Tax);

        // Calcular ITBIS pagado de compras (del 606)
        var compras = await _accounting.GetPurchasesAsync(from, to);
        var itbisPagado = compras.Sum(c => c.Tax);

        return new DeclaracionIT1(
            Rnc: _rnc,
            Periodo: periodo,
            VentasBrutas: ventasBrutas,
            ItbisCobrado: itbisCobrado,
            ComprasBienes: compras.Sum(c => c.Subtotal),
            ItbisPagado: itbisPagado,
            ItbisNeto: itbisCobrado - itbisPagado,
            Status: DeclaracionStatus.Draft
        );
    }

    public async Task<SubmissionResult> SubmitIT1Async(DeclaracionIT1 decl)
    {
        var payload = new
        {
            rnc = decl.Rnc,
            periodo = decl.Periodo,
            ventasBrutasFirma = decl.VentasBrutas,
            itbisCobrado = decl.ItbisCobrado,
            comprasBienes = decl.ComprasBienes,
            itbisPagado = decl.ItbisPagado,
            itbisNeto = decl.ItbisNeto
        };

        var response = await _httpClient.PostAsJsonAsync(
            "api/declaraciones/it1", payload);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content
                .ReadFromJsonAsync<DgiiResponse>();

            return new SubmissionResult(
                true,
                result?.NumeroConfirmacion,
                null,
                DateTime.UtcNow
            );
        }

        var error = await response.Content.ReadAsStringAsync();
        return new SubmissionResult(false, null, error, DateTime.UtcNow);
    }

    public async Task<SubmissionResult> SubmitFormato606Async(
        string periodo, List<Formato606Entry> entries)
    {
        var payload = new
        {
            rnc = _rnc,
            periodo = periodo,
            registros = entries.Select(e => new
            {
                rncProveedor = e.RncProveedor,
                tipoId = ((int)e.TipoIdentificacion).ToString(),
                ncf = e.Ncf,
                fechaComprobante = e.FechaComprobante.ToString("yyyyMMdd"),
                montoFacturado = e.MontoFacturado,
                itbisFacturado = e.ItbisFacturado,
                formaPago = ((int)e.FormaPago).ToString("00")
            })
        };

        var response = await _httpClient.PostAsJsonAsync(
            "api/formatos/606", payload);

        // Manejar respuesta similar a SubmitIT1Async
        if (response.IsSuccessStatusCode)
        {
            return new SubmissionResult(true, "606-OK", null, DateTime.UtcNow);
        }

        var error = await response.Content.ReadAsStringAsync();
        return new SubmissionResult(false, null, error, DateTime.UtcNow);
    }
}
```

---

## ⚛️ React Component

```tsx
// components/TaxDeclarations.tsx
import { useState } from "react";
import { useQuery, useMutation } from "@tanstack/react-query";
import { taxService } from "@/services/taxService";
import { FileText, Send, CheckCircle, AlertCircle } from "lucide-react";

export function TaxDeclarations() {
  const [periodo, setPeriodo] = useState(
    new Date().toISOString().slice(0, 7).replace("-", "")
  );

  const { data: it1, isLoading } = useQuery({
    queryKey: ["it1-preview", periodo],
    queryFn: () => taxService.generateIT1(periodo),
  });

  const submitMutation = useMutation({
    mutationFn: () => taxService.submitIT1(periodo),
    onSuccess: () => alert("Declaración enviada exitosamente"),
  });

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">Declaraciones DGII</h1>
        <input
          type="month"
          value={`${periodo.slice(0, 4)}-${periodo.slice(4)}`}
          onChange={(e) => setPeriodo(e.target.value.replace("-", ""))}
          className="px-4 py-2 border rounded-lg"
        />
      </div>

      {isLoading ? (
        <p>Calculando...</p>
      ) : (
        it1 && (
          <div className="bg-white rounded-lg shadow p-6">
            <div className="flex items-center gap-3 mb-6">
              <FileText className="w-8 h-8 text-blue-600" />
              <div>
                <h2 className="text-xl font-bold">IT-1 ITBIS Mensual</h2>
                <p className="text-gray-500">Período: {periodo}</p>
              </div>
            </div>

            <div className="grid grid-cols-2 gap-4 mb-6">
              <div className="p-4 bg-gray-50 rounded">
                <p className="text-sm text-gray-500">Ventas Brutas</p>
                <p className="text-xl font-bold">
                  RD$ {it1.ventasBrutas.toLocaleString()}
                </p>
              </div>
              <div className="p-4 bg-green-50 rounded">
                <p className="text-sm text-gray-500">ITBIS Cobrado</p>
                <p className="text-xl font-bold text-green-600">
                  RD$ {it1.itbisCobrado.toLocaleString()}
                </p>
              </div>
              <div className="p-4 bg-gray-50 rounded">
                <p className="text-sm text-gray-500">Compras</p>
                <p className="text-xl font-bold">
                  RD$ {it1.comprasBienes.toLocaleString()}
                </p>
              </div>
              <div className="p-4 bg-red-50 rounded">
                <p className="text-sm text-gray-500">ITBIS Pagado</p>
                <p className="text-xl font-bold text-red-600">
                  RD$ {it1.itbisPagado.toLocaleString()}
                </p>
              </div>
            </div>

            <div className="p-4 bg-blue-50 rounded mb-6">
              <p className="text-sm text-gray-500">ITBIS Neto a Pagar</p>
              <p className="text-3xl font-bold text-blue-600">
                RD$ {it1.itbisNeto.toLocaleString()}
              </p>
            </div>

            <button
              onClick={() => submitMutation.mutate()}
              disabled={submitMutation.isPending}
              className="w-full flex items-center justify-center gap-2 
                       bg-blue-600 text-white py-3 rounded-lg 
                       hover:bg-blue-700 disabled:opacity-50"
            >
              <Send className="w-5 h-5" />
              {submitMutation.isPending ? "Enviando..." : "Enviar a DGII"}
            </button>

            {submitMutation.isSuccess && (
              <div className="mt-4 p-4 bg-green-50 rounded flex items-center gap-2">
                <CheckCircle className="w-5 h-5 text-green-600" />
                <span className="text-green-700">
                  Declaración enviada correctamente
                </span>
              </div>
            )}

            {submitMutation.isError && (
              <div className="mt-4 p-4 bg-red-50 rounded flex items-center gap-2">
                <AlertCircle className="w-5 h-5 text-red-600" />
                <span className="text-red-700">
                  Error al enviar declaración
                </span>
              </div>
            )}
          </div>
        )
      )}
    </div>
  );
}
```

---

## 📅 Calendario Fiscal

| Mes       | Fecha Límite 606/607 | Fecha Límite IR-17 | Fecha Límite IT-1 |
| --------- | -------------------- | ------------------ | ----------------- |
| Enero     | 15 Feb               | 10 Feb             | 20 Feb            |
| Febrero   | 15 Mar               | 10 Mar             | 20 Mar            |
| ...       | ...                  | ...                | ...               |
| Diciembre | 15 Ene               | 10 Ene             | 20 Ene            |

---

## 🧪 Tests

```csharp
public class TaxDeclarationServiceTests
{
    [Fact]
    public async Task GenerateIT1_ShouldCalculateCorrectly()
    {
        // Arrange
        var accounting = new Mock<IAccountingService>();
        accounting.Setup(a => a.GetInvoicesAsync(It.IsAny<DateTime>(),
            It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Invoice>
            {
                new Invoice(1, 1, "Test", DateTime.Now, DateTime.Now,
                    100000, 18000, 118000, InvoiceStatus.Paid, "E31001")
            });

        var service = new DgiiTaxDeclarationService(
            new HttpClient(), accounting.Object, GetConfig(),
            Mock.Of<ILogger<DgiiTaxDeclarationService>>()
        );

        // Act
        var it1 = await service.GenerateIT1Async("202601");

        // Assert
        it1.ItbisCobrado.Should().Be(18000);
    }
}
```

---

**Anterior:** [ACCOUNTING_SOFTWARE_INTEGRATIONS.md](./ACCOUNTING_SOFTWARE_INTEGRATIONS.md)  
**Índice:** [README.md](./README.md)
