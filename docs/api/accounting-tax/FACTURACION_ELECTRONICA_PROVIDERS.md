# 🏢 Proveedores de Facturación Electrónica (e-CF)

**Categoría:** E-Invoicing Providers  
**País:** República Dominicana 🇩🇴  
**Regulador:** DGII  
**Última Actualización:** Enero 15, 2026

---

## 📋 Proveedores Autorizados por DGII

| Proveedor          | API REST    | Costo Mensual   | Soporte | Recomendación  |
| ------------------ | ----------- | --------------- | ------- | -------------- |
| **Facturedo**      | ✅ Completa | RD$2,500-15,000 | 24/7    | ⭐ RECOMENDADO |
| **TribuFácil**     | ✅ Sí       | RD$1,500-8,000  | Horario | ⭐⭐⭐⭐       |
| **ComprobantesRD** | ✅ Sí       | RD$2,000-10,000 | 24/7    | ⭐⭐⭐⭐       |
| **E-Factura**      | ✅ Sí       | RD$3,000-12,000 | 24/7    | ⭐⭐⭐         |
| **FacturaDigital** | ✅ Sí       | RD$1,800-9,000  | Email   | ⭐⭐⭐         |

---

## 🌟 FACTUREDO (Recomendado para OKLA)

### Información General

| Campo             | Valor                                            |
| ----------------- | ------------------------------------------------ |
| **Website**       | [facturedo.com](https://facturedo.com)           |
| **API Base**      | `https://api.facturedo.com/v2/`                  |
| **Documentación** | [docs.facturedo.com](https://docs.facturedo.com) |
| **Autenticación** | API Key + Secret                                 |

### Planes y Precios

| Plan            | e-CF/mes | Precio    | Ideal para        |
| --------------- | -------- | --------- | ----------------- |
| **Básico**      | 100      | RD$2,500  | Pequeñas empresas |
| **Profesional** | 500      | RD$6,000  | Medianas empresas |
| **Empresarial** | 2,000    | RD$12,000 | Grandes empresas  |
| **Ilimitado**   | ∞        | RD$18,000 | Alto volumen      |

### Endpoints API

```http
# Autenticación
POST /auth/token
{
  "apiKey": "fac_live_xxxxx",
  "apiSecret": "xxxxx"
}

# Emitir e-CF
POST /ecf/emit
Authorization: Bearer {token}
{
  "tipo": 32,
  "comprador": {
    "rnc": "123456789",
    "razonSocial": "Cliente SRL"
  },
  "items": [
    {
      "descripcion": "Toyota Corolla 2024",
      "cantidad": 1,
      "precio": 100000,
      "itbis": 18000
    }
  ]
}

# Consultar e-CF
GET /ecf/{encf}

# Anular e-CF
DELETE /ecf/{encf}
{
  "motivo": "Error en datos"
}

# Listar e-CF
GET /ecf?desde=2026-01-01&hasta=2026-01-31
```

---

## 💻 Implementación C#

### Service Interface

```csharp
namespace AccountingTaxService.Domain.Interfaces;

public interface IInvoicingProvider
{
    string ProviderName { get; }
    Task<string> AuthenticateAsync();
    Task<InvoiceResult> EmitInvoiceAsync(InvoiceRequest request);
    Task<Invoice?> GetInvoiceAsync(string encf);
    Task<bool> CancelInvoiceAsync(string encf, string reason);
    Task<List<Invoice>> ListInvoicesAsync(DateTime from, DateTime to);
    Task<byte[]> GetPdfAsync(string encf);
    Task<byte[]> GetXmlAsync(string encf);
}

public record InvoiceRequest(
    EcfType Type,
    BuyerInfo Buyer,
    List<InvoiceItem> Items,
    DateTime? EmissionDate = null
);

public record BuyerInfo(
    string Rnc,
    string RazonSocial,
    string? Email = null,
    string? Telefono = null
);

public record InvoiceResult(
    bool Success,
    string? ENCF,
    string? TrackingId,
    string? PdfUrl,
    string? XmlUrl,
    string? ErrorMessage
);
```

### Facturedo Implementation

```csharp
namespace AccountingTaxService.Infrastructure.Services;

public class FacturedoService : IInvoicingProvider
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<FacturedoService> _logger;
    private string? _token;
    private DateTime _tokenExpiry;

    public string ProviderName => "Facturedo";

    public FacturedoService(
        HttpClient httpClient,
        IConfiguration config,
        ILogger<FacturedoService> logger)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://api.facturedo.com/v2/");
        _config = config;
        _logger = logger;
    }

    public async Task<string> AuthenticateAsync()
    {
        if (_token != null && DateTime.UtcNow < _tokenExpiry)
            return _token;

        var request = new
        {
            apiKey = _config["Facturedo:ApiKey"],
            apiSecret = _config["Facturedo:ApiSecret"]
        };

        var response = await _httpClient.PostAsJsonAsync(
            "auth/token", request);

        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<TokenResponse>();

        _token = result!.Token;
        _tokenExpiry = DateTime.UtcNow.AddHours(23);

        return _token;
    }

    public async Task<InvoiceResult> EmitInvoiceAsync(InvoiceRequest request)
    {
        try
        {
            await EnsureAuthenticatedAsync();

            var facturedo = new
            {
                tipo = (int)request.Type,
                comprador = new
                {
                    rnc = request.Buyer.Rnc,
                    razonSocial = request.Buyer.RazonSocial,
                    email = request.Buyer.Email
                },
                items = request.Items.Select(i => new
                {
                    descripcion = i.Description,
                    cantidad = i.Quantity,
                    precio = i.UnitPrice,
                    itbis = i.UnitPrice * 0.18m
                }),
                fechaEmision = request.EmissionDate ?? DateTime.Now
            };

            var response = await _httpClient.PostAsJsonAsync(
                "ecf/emit", facturedo);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Facturedo error: {Error}", error);
                return new InvoiceResult(false, null, null, null, null, error);
            }

            var result = await response.Content
                .ReadFromJsonAsync<FacturedoResponse>();

            return new InvoiceResult(
                true,
                result!.Encf,
                result.TrackingId,
                result.PdfUrl,
                result.XmlUrl,
                null
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error emitting invoice via Facturedo");
            return new InvoiceResult(false, null, null, null, null, ex.Message);
        }
    }

    public async Task<Invoice?> GetInvoiceAsync(string encf)
    {
        await EnsureAuthenticatedAsync();

        var response = await _httpClient.GetAsync($"ecf/{encf}");

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<Invoice>();
    }

    public async Task<bool> CancelInvoiceAsync(string encf, string reason)
    {
        await EnsureAuthenticatedAsync();

        var request = new HttpRequestMessage(HttpMethod.Delete, $"ecf/{encf}")
        {
            Content = JsonContent.Create(new { motivo = reason })
        };

        var response = await _httpClient.SendAsync(request);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<Invoice>> ListInvoicesAsync(DateTime from, DateTime to)
    {
        await EnsureAuthenticatedAsync();

        var response = await _httpClient.GetAsync(
            $"ecf?desde={from:yyyy-MM-dd}&hasta={to:yyyy-MM-dd}");

        if (!response.IsSuccessStatusCode)
            return new List<Invoice>();

        var result = await response.Content
            .ReadFromJsonAsync<ListResponse<Invoice>>();

        return result?.Data ?? new List<Invoice>();
    }

    public async Task<byte[]> GetPdfAsync(string encf)
    {
        await EnsureAuthenticatedAsync();
        return await _httpClient.GetByteArrayAsync($"ecf/{encf}/pdf");
    }

    public async Task<byte[]> GetXmlAsync(string encf)
    {
        await EnsureAuthenticatedAsync();
        return await _httpClient.GetByteArrayAsync($"ecf/{encf}/xml");
    }

    private async Task EnsureAuthenticatedAsync()
    {
        var token = await AuthenticateAsync();
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }
}
```

---

## ⚛️ React Component

```tsx
// components/InvoiceManager.tsx
import { useState } from "react";
import { useQuery, useMutation } from "@tanstack/react-query";
import { invoiceService } from "@/services/invoiceService";
import { FileText, Download, X } from "lucide-react";

export function InvoiceManager() {
  const [dateRange, setDateRange] = useState({
    from: new Date().toISOString().slice(0, 7) + "-01",
    to: new Date().toISOString().slice(0, 10),
  });

  const { data: invoices, isLoading } = useQuery({
    queryKey: ["invoices", dateRange],
    queryFn: () => invoiceService.list(dateRange.from, dateRange.to),
  });

  const cancelMutation = useMutation({
    mutationFn: ({ encf, reason }: { encf: string; reason: string }) =>
      invoiceService.cancel(encf, reason),
  });

  return (
    <div className="bg-white rounded-lg shadow">
      <div className="p-4 border-b flex justify-between">
        <h2 className="text-xl font-bold">Facturas Electrónicas</h2>
        <div className="flex gap-2">
          <input
            type="date"
            value={dateRange.from}
            onChange={(e) =>
              setDateRange((prev) => ({
                ...prev,
                from: e.target.value,
              }))
            }
            className="border p-1 rounded"
          />
          <input
            type="date"
            value={dateRange.to}
            onChange={(e) =>
              setDateRange((prev) => ({
                ...prev,
                to: e.target.value,
              }))
            }
            className="border p-1 rounded"
          />
        </div>
      </div>

      {isLoading ? (
        <p className="p-4">Cargando...</p>
      ) : (
        <table className="w-full">
          <thead className="bg-gray-50">
            <tr>
              <th className="p-3 text-left">e-NCF</th>
              <th className="p-3 text-left">Cliente</th>
              <th className="p-3 text-right">Total</th>
              <th className="p-3 text-center">Estado</th>
              <th className="p-3 text-center">Acciones</th>
            </tr>
          </thead>
          <tbody>
            {invoices?.map((inv) => (
              <tr key={inv.encf} className="border-t">
                <td className="p-3 font-mono">{inv.encf}</td>
                <td className="p-3">{inv.buyerName}</td>
                <td className="p-3 text-right">
                  RD$ {inv.total.toLocaleString()}
                </td>
                <td className="p-3 text-center">
                  <span
                    className={`px-2 py-1 rounded text-xs ${
                      inv.status === "accepted"
                        ? "bg-green-100 text-green-800"
                        : inv.status === "cancelled"
                        ? "bg-red-100 text-red-800"
                        : "bg-yellow-100 text-yellow-800"
                    }`}
                  >
                    {inv.status}
                  </span>
                </td>
                <td className="p-3 text-center">
                  <div className="flex justify-center gap-2">
                    <button
                      onClick={() => window.open(inv.pdfUrl)}
                      title="Descargar PDF"
                    >
                      <FileText className="w-4 h-4" />
                    </button>
                    <button
                      onClick={() => window.open(inv.xmlUrl)}
                      title="Descargar XML"
                    >
                      <Download className="w-4 h-4" />
                    </button>
                    {inv.status !== "cancelled" && (
                      <button
                        onClick={() => {
                          const reason = prompt("Motivo de anulación:");
                          if (reason) {
                            cancelMutation.mutate({
                              encf: inv.encf,
                              reason,
                            });
                          }
                        }}
                        title="Anular"
                        className="text-red-600"
                      >
                        <X className="w-4 h-4" />
                      </button>
                    )}
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}
```

---

## 🧪 Tests

```csharp
public class FacturedoServiceTests
{
    [Fact]
    public async Task EmitInvoiceAsync_ValidRequest_ReturnsEncf()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("/ecf/emit")
            .Respond("application/json", @"{
                ""encf"": ""E320000000001"",
                ""trackingId"": ""abc123"",
                ""pdfUrl"": ""https://..."",
                ""xmlUrl"": ""https://...""
            }");

        var service = new FacturedoService(
            new HttpClient(mockHttp), ...);

        var result = await service.EmitInvoiceAsync(new InvoiceRequest(
            EcfType.Consumo,
            new BuyerInfo("123456789", "Test SRL"),
            new List<InvoiceItem> {
                new("Producto", 1, 1000)
            }
        ));

        result.Success.Should().BeTrue();
        result.ENCF.Should().Be("E320000000001");
    }
}
```

---

## 🔧 Configuración

```json
// appsettings.json
{
  "Facturedo": {
    "ApiKey": "fac_live_xxxxx",
    "ApiSecret": "xxxxx",
    "Environment": "production"
  }
}
```

---

## 💰 Costos Comparativos

| Proveedor          | 500 e-CF/mes | 2000 e-CF/mes | API Quality |
| ------------------ | ------------ | ------------- | ----------- |
| **Facturedo**      | RD$6,000     | RD$12,000     | ⭐⭐⭐⭐⭐  |
| **TribuFácil**     | RD$5,500     | RD$9,000      | ⭐⭐⭐⭐    |
| **ComprobantesRD** | RD$6,500     | RD$14,000     | ⭐⭐⭐⭐    |
| **DGII Directo**   | GRATIS       | GRATIS        | ⭐⭐⭐      |

---

**Anterior:** [DGII_NCF_RNC_API.md](./DGII_NCF_RNC_API.md)  
**Próximo:** [BANKING_APIS.md](./BANKING_APIS.md)
