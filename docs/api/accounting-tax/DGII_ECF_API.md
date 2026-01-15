# 🧾 DGII e-CF API - Facturación Electrónica

**Entidad:** Dirección General de Impuestos Internos (DGII)  
**Sistema:** Comprobantes Fiscales Electrónicos (e-CF)  
**Ambiente Producción:** `https://ecf.dgii.gov.do/`  
**Ambiente Pruebas:** `https://ecf.dgii.gov.do/testecf/`  
**Documentación Oficial:** [dgii.gov.do/cicloContribuyente/facturacion](https://dgii.gov.do/cicloContribuyente/facturacion)

---

## 📋 Tipos de e-CF

| Código | Tipo                      | Uso en OKLA                   |
| ------ | ------------------------- | ----------------------------- |
| **31** | Factura de Crédito Fiscal | Ventas B2B (a empresas)       |
| **32** | Factura de Consumo        | Ventas B2C (consumidor final) |
| **33** | Nota de Débito            | Cargos adicionales            |
| **34** | Nota de Crédito           | Devoluciones, descuentos      |
| **41** | Compras                   | Registro de compras           |
| **43** | Gastos Menores            | Gastos sin comprobante        |
| **44** | Regímenes Especiales      | Zona franca                   |
| **45** | Gubernamental             | Ventas al gobierno            |

---

## 🔐 Autenticación

### Certificado Digital

```csharp
// Cargar certificado .p12
var certificate = new X509Certificate2(
    "certificado_dgii.p12",
    "password",
    X509KeyStorageFlags.MachineKeySet
);
```

### Headers Requeridos

```http
Content-Type: application/json
Authorization: Bearer {token}
X-DGII-RNC: 123456789
X-DGII-Ambiente: 1 (Producción) | 2 (Pruebas)
```

---

## 📡 Endpoints Principales

### 1. Autenticación

```http
POST /api/Autenticacion
Content-Type: application/json

{
  "rncEmisor": "123456789",
  "passwordCertificado": "...",
  "certificadoBase64": "..."
}
```

**Response:**

```json
{
  "token": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expira": "2026-01-15T23:59:59Z"
}
```

### 2. Emitir e-CF

```http
POST /api/ecf
Authorization: Bearer {token}
Content-Type: application/json

{
  "encabezado": {
    "idDoc": {
      "tipoeCF": 32,
      "eNCF": "E320000000001",
      "fechaVencimientoSecuencia": "2026-12-31"
    },
    "emisor": {
      "rncEmisor": "123456789",
      "razonSocialEmisor": "OKLA SRL",
      "direccionEmisor": "Av. Winston Churchill 123",
      "fechaEmision": "2026-01-15"
    },
    "comprador": {
      "rncComprador": "987654321",
      "razonSocialComprador": "Cliente SRL"
    },
    "totales": {
      "montoGravadoTotal": 100000.00,
      "montoGravadoI1": 100000.00,
      "totalITBIS": 18000.00,
      "totalITBIS1": 18000.00,
      "montoTotal": 118000.00
    }
  },
  "detallesItems": [
    {
      "numeroLinea": 1,
      "indicadorFacturacion": 1,
      "nombreItem": "Toyota Corolla 2024",
      "indicadorBienoServicio": 1,
      "cantidadItem": 1,
      "precioUnitarioItem": 100000.00,
      "montoItem": 100000.00
    }
  ]
}
```

### 3. Consultar Estado

```http
GET /api/ecf/estado/{trackingId}
Authorization: Bearer {token}
```

### 4. Anular e-CF

```http
POST /api/ecf/anular
Authorization: Bearer {token}

{
  "eNCF": "E320000000001",
  "motivoAnulacion": "Error en datos del comprador"
}
```

---

## 💻 Implementación C#

### Domain Models

```csharp
namespace AccountingTaxService.Domain.Entities;

public class ElectronicInvoice
{
    public Guid Id { get; set; }
    public string ENCF { get; set; } = string.Empty;
    public EcfType Type { get; set; }
    public string EmitterRNC { get; set; } = string.Empty;
    public string BuyerRNC { get; set; } = string.Empty;
    public string BuyerName { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal ITBIS { get; set; }
    public decimal Total { get; set; }
    public DateTime EmissionDate { get; set; }
    public EcfStatus Status { get; set; }
    public string? DgiiTrackingId { get; set; }
    public string? DgiiAuthCode { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<InvoiceItem> Items { get; set; } = new();
}

public enum EcfType
{
    CreditoFiscal = 31,
    Consumo = 32,
    NotaDebito = 33,
    NotaCredito = 34,
    Compras = 41,
    GastosMenores = 43,
    RegimenesEspeciales = 44,
    Gubernamental = 45
}

public enum EcfStatus
{
    Draft,
    Pending,
    Accepted,
    Rejected,
    Cancelled
}

public class InvoiceItem
{
    public int LineNumber { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Total { get; set; }
    public decimal ITBISRate { get; set; } = 0.18m;
}
```

### Service Interface

```csharp
namespace AccountingTaxService.Domain.Interfaces;

public interface IEcfService
{
    Task<string> AuthenticateAsync();
    Task<EcfResult> EmitAsync(ElectronicInvoice invoice);
    Task<EcfStatus> GetStatusAsync(string trackingId);
    Task<bool> CancelAsync(string encf, string reason);
    Task<ElectronicInvoice?> GetByEncfAsync(string encf);
    Task<List<ElectronicInvoice>> GetByDateRangeAsync(DateTime from, DateTime to);
}

public record EcfResult(
    bool Success,
    string? ENCF,
    string? TrackingId,
    string? AuthCode,
    string? ErrorMessage
);
```

### Service Implementation

```csharp
namespace AccountingTaxService.Infrastructure.Services;

public class DgiiEcfService : IEcfService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<DgiiEcfService> _logger;
    private string? _token;
    private DateTime _tokenExpiry;

    public DgiiEcfService(
        HttpClient httpClient,
        IConfiguration config,
        ILogger<DgiiEcfService> logger)
    {
        _httpClient = httpClient;
        _config = config;
        _logger = logger;
    }

    public async Task<string> AuthenticateAsync()
    {
        if (_token != null && DateTime.UtcNow < _tokenExpiry)
            return _token;

        var certPath = _config["DGII:CertificatePath"];
        var certPassword = _config["DGII:CertificatePassword"];
        var rnc = _config["DGII:RNC"];

        var certBytes = await File.ReadAllBytesAsync(certPath!);
        var certBase64 = Convert.ToBase64String(certBytes);

        var request = new
        {
            rncEmisor = rnc,
            passwordCertificado = certPassword,
            certificadoBase64 = certBase64
        };

        var response = await _httpClient.PostAsJsonAsync(
            "/api/Autenticacion", request);

        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<AuthResponse>();

        _token = result!.Token;
        _tokenExpiry = result.Expira.AddMinutes(-5);

        return _token;
    }

    public async Task<EcfResult> EmitAsync(ElectronicInvoice invoice)
    {
        try
        {
            await EnsureAuthenticatedAsync();

            var ecfRequest = MapToEcfRequest(invoice);

            var response = await _httpClient.PostAsJsonAsync(
                "/api/ecf", ecfRequest);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("DGII Error: {Error}", error);
                return new EcfResult(false, null, null, null, error);
            }

            var result = await response.Content
                .ReadFromJsonAsync<EcfResponse>();

            return new EcfResult(
                true,
                result!.ENCF,
                result.TrackingId,
                result.CodigoAutorizacion,
                null
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error emitting e-CF");
            return new EcfResult(false, null, null, null, ex.Message);
        }
    }

    public async Task<EcfStatus> GetStatusAsync(string trackingId)
    {
        await EnsureAuthenticatedAsync();

        var response = await _httpClient.GetAsync(
            $"/api/ecf/estado/{trackingId}");

        if (!response.IsSuccessStatusCode)
            return EcfStatus.Pending;

        var result = await response.Content
            .ReadFromJsonAsync<StatusResponse>();

        return result!.Estado switch
        {
            "Aceptado" => EcfStatus.Accepted,
            "Rechazado" => EcfStatus.Rejected,
            "Anulado" => EcfStatus.Cancelled,
            _ => EcfStatus.Pending
        };
    }

    public async Task<bool> CancelAsync(string encf, string reason)
    {
        await EnsureAuthenticatedAsync();

        var request = new { eNCF = encf, motivoAnulacion = reason };

        var response = await _httpClient.PostAsJsonAsync(
            "/api/ecf/anular", request);

        return response.IsSuccessStatusCode;
    }

    private async Task EnsureAuthenticatedAsync()
    {
        var token = await AuthenticateAsync();
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    private object MapToEcfRequest(ElectronicInvoice invoice)
    {
        return new
        {
            encabezado = new
            {
                idDoc = new
                {
                    tipoeCF = (int)invoice.Type,
                    eNCF = invoice.ENCF,
                    fechaVencimientoSecuencia = DateTime.Now.AddYears(1)
                },
                emisor = new
                {
                    rncEmisor = invoice.EmitterRNC,
                    razonSocialEmisor = _config["Company:Name"],
                    direccionEmisor = _config["Company:Address"],
                    fechaEmision = invoice.EmissionDate
                },
                comprador = new
                {
                    rncComprador = invoice.BuyerRNC,
                    razonSocialComprador = invoice.BuyerName
                },
                totales = new
                {
                    montoGravadoTotal = invoice.Subtotal,
                    montoGravadoI1 = invoice.Subtotal,
                    totalITBIS = invoice.ITBIS,
                    totalITBIS1 = invoice.ITBIS,
                    montoTotal = invoice.Total
                }
            },
            detallesItems = invoice.Items.Select(i => new
            {
                numeroLinea = i.LineNumber,
                indicadorFacturacion = 1,
                nombreItem = i.Description,
                indicadorBienoServicio = 1,
                cantidadItem = i.Quantity,
                precioUnitarioItem = i.UnitPrice,
                montoItem = i.Total
            })
        };
    }
}
```

---

## ⚛️ React Component

```tsx
// components/InvoiceEmitter.tsx
import { useState } from "react";
import { useMutation } from "@tanstack/react-query";
import { ecfService } from "@/services/ecfService";

interface InvoiceFormData {
  buyerRnc: string;
  buyerName: string;
  items: Array<{
    description: string;
    quantity: number;
    unitPrice: number;
  }>;
}

export function InvoiceEmitter() {
  const [formData, setFormData] = useState<InvoiceFormData>({
    buyerRnc: "",
    buyerName: "",
    items: [{ description: "", quantity: 1, unitPrice: 0 }],
  });

  const emitMutation = useMutation({
    mutationFn: ecfService.emit,
    onSuccess: (data) => {
      alert(`e-CF emitido: ${data.encf}`);
    },
    onError: (error) => {
      alert(`Error: ${error.message}`);
    },
  });

  const subtotal = formData.items.reduce(
    (sum, item) => sum + item.quantity * item.unitPrice,
    0
  );
  const itbis = subtotal * 0.18;
  const total = subtotal + itbis;

  return (
    <div className="bg-white rounded-lg shadow p-6">
      <h2 className="text-xl font-bold mb-4">Emitir Factura Electrónica</h2>

      <div className="grid grid-cols-2 gap-4 mb-4">
        <input
          placeholder="RNC Comprador"
          value={formData.buyerRnc}
          onChange={(e) =>
            setFormData({
              ...formData,
              buyerRnc: e.target.value,
            })
          }
          className="border p-2 rounded"
        />
        <input
          placeholder="Nombre/Razón Social"
          value={formData.buyerName}
          onChange={(e) =>
            setFormData({
              ...formData,
              buyerName: e.target.value,
            })
          }
          className="border p-2 rounded"
        />
      </div>

      <div className="border-t pt-4 mt-4">
        <div className="flex justify-between text-lg">
          <span>Subtotal:</span>
          <span>RD$ {subtotal.toLocaleString()}</span>
        </div>
        <div className="flex justify-between text-lg">
          <span>ITBIS (18%):</span>
          <span>RD$ {itbis.toLocaleString()}</span>
        </div>
        <div className="flex justify-between text-xl font-bold">
          <span>Total:</span>
          <span>RD$ {total.toLocaleString()}</span>
        </div>
      </div>

      <button
        onClick={() =>
          emitMutation.mutate({
            ...formData,
            subtotal,
            itbis,
            total,
          })
        }
        disabled={emitMutation.isPending}
        className="mt-4 w-full bg-blue-600 text-white py-2 rounded"
      >
        {emitMutation.isPending ? "Emitiendo..." : "Emitir e-CF"}
      </button>
    </div>
  );
}
```

---

## 🧪 Tests

```csharp
public class DgiiEcfServiceTests
{
    [Fact]
    public async Task EmitAsync_ValidInvoice_ReturnsSuccess()
    {
        // Arrange
        var mockHttp = new Mock<HttpClient>();
        var service = new DgiiEcfService(mockHttp.Object, ...);

        var invoice = new ElectronicInvoice
        {
            Type = EcfType.Consumo,
            BuyerRNC = "123456789",
            Subtotal = 100000,
            ITBIS = 18000,
            Total = 118000
        };

        // Act
        var result = await service.EmitAsync(invoice);

        // Assert
        result.Success.Should().BeTrue();
        result.ENCF.Should().NotBeNullOrEmpty();
    }
}
```

---

## 🔧 Troubleshooting

| Error                  | Causa               | Solución                  |
| ---------------------- | ------------------- | ------------------------- |
| `401 Unauthorized`     | Token expirado      | Re-autenticar             |
| `RNC no válido`        | RNC incorrecto      | Validar con /api/rnc      |
| `Secuencia agotada`    | NCFs consumidos     | Solicitar nueva secuencia |
| `Certificado inválido` | Certificado vencido | Renovar en DGII           |

---

**Próximo:** [DGII_NCF_RNC_API.md](./DGII_NCF_RNC_API.md)
