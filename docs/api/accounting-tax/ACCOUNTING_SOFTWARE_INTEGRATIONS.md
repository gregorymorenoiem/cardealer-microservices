# 📊 Integraciones con Software Contable

**Categoría:** Accounting Software Integrations  
**Uso:** Sincronización automática de datos financieros  
**Última Actualización:** Enero 15, 2026

---

## 📋 Software Contable Compatible

| Software            | API     | Popularidad RD | Costo          | Recomendación     |
| ------------------- | ------- | -------------- | -------------- | ----------------- |
| **Alegra**          | ✅ REST | ⭐⭐⭐⭐⭐     | $25-99/mes     | ⭐ RECOMENDADO    |
| **Siigo**           | ✅ REST | ⭐⭐⭐⭐       | $30-150/mes    | Empresas medianas |
| **QuickBooks**      | ✅ REST | ⭐⭐⭐         | $15-100/mes    | Internacional     |
| **Xero**            | ✅ REST | ⭐⭐⭐         | $12-70/mes     | Startups          |
| **Contabilidad.do** | ✅ REST | ⭐⭐⭐⭐       | RD$1,500-5,000 | Local RD          |

---

## 🌟 ALEGRA (Recomendado)

### Información

| Campo             | Valor                                                |
| ----------------- | ---------------------------------------------------- |
| **Website**       | [alegra.com](https://alegra.com)                     |
| **API Base**      | `https://api.alegra.com/api/v1/`                     |
| **Documentación** | [developer.alegra.com](https://developer.alegra.com) |
| **Autenticación** | Basic Auth (email:token)                             |

### Endpoints Principales

```http
# Autenticación (Basic Auth)
Authorization: Basic base64(email:apiToken)

# Crear Factura
POST /invoices
{
  "client": 123,
  "date": "2026-01-15",
  "dueDate": "2026-02-15",
  "items": [
    {
      "id": 1,
      "name": "Toyota Corolla 2024",
      "price": 100000,
      "quantity": 1,
      "tax": [{"id": 1}]
    }
  ],
  "stamp": {"generateStamp": true}
}

# Obtener Clientes
GET /contacts?type=client

# Crear Cliente
POST /contacts
{
  "name": "Cliente SRL",
  "identification": "123456789",
  "email": "cliente@email.com",
  "type": ["client"]
}

# Obtener Productos
GET /items

# Obtener Facturas
GET /invoices?start=2026-01-01&end=2026-01-31

# Registrar Pago
POST /payments
{
  "date": "2026-01-15",
  "bankAccount": 1,
  "invoices": [{"id": 123, "amount": 118000}]
}

# Obtener Cuentas Bancarias
GET /bank-accounts

# Balance General
GET /reports/balance-sheet?date=2026-01-31

# Estado de Resultados
GET /reports/income-statement
    ?start=2026-01-01
    &end=2026-01-31
```

---

## 💻 Implementación C#

### Service Interface

```csharp
namespace AccountingTaxService.Domain.Interfaces;

public interface IAccountingService
{
    // Clientes/Contactos
    Task<Contact> CreateContactAsync(ContactRequest contact);
    Task<Contact?> GetContactByRncAsync(string rnc);
    Task<List<Contact>> GetContactsAsync(ContactType type);

    // Facturas
    Task<Invoice> CreateInvoiceAsync(InvoiceRequest invoice);
    Task<Invoice?> GetInvoiceAsync(int id);
    Task<List<Invoice>> GetInvoicesAsync(DateTime from, DateTime to);

    // Pagos
    Task<Payment> RecordPaymentAsync(PaymentRequest payment);

    // Productos
    Task<Product> CreateProductAsync(ProductRequest product);
    Task<List<Product>> GetProductsAsync();

    // Reportes
    Task<BalanceSheet> GetBalanceSheetAsync(DateTime asOf);
    Task<IncomeStatement> GetIncomeStatementAsync(DateTime from, DateTime to);

    // Sincronización
    Task SyncFromOklaAsync();
}

public record Contact(
    int Id,
    string Name,
    string? Identification,
    string? Email,
    string? Phone,
    ContactType Type
);

public enum ContactType { Client, Provider }

public record Invoice(
    int Id,
    int ClientId,
    string ClientName,
    DateTime Date,
    DateTime DueDate,
    decimal Subtotal,
    decimal Tax,
    decimal Total,
    InvoiceStatus Status,
    string? Ncf
);

public enum InvoiceStatus { Draft, Sent, Paid, Overdue, Cancelled }

public record BalanceSheet(
    DateTime AsOf,
    decimal TotalAssets,
    decimal TotalLiabilities,
    decimal Equity,
    List<AccountBalance> Assets,
    List<AccountBalance> Liabilities
);

public record AccountBalance(
    string AccountCode,
    string AccountName,
    decimal Balance
);
```

### Alegra Service

```csharp
namespace AccountingTaxService.Infrastructure.Services;

public class AlegraService : IAccountingService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<AlegraService> _logger;

    public AlegraService(
        HttpClient httpClient,
        IConfiguration config,
        ILogger<AlegraService> logger)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://api.alegra.com/api/v1/");
        _config = config;
        _logger = logger;

        // Configurar Basic Auth
        var credentials = Convert.ToBase64String(
            Encoding.UTF8.GetBytes(
                $"{config["Alegra:Email"]}:{config["Alegra:ApiToken"]}"
            )
        );
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", credentials);
    }

    public async Task<Contact> CreateContactAsync(ContactRequest request)
    {
        var payload = new
        {
            name = request.Name,
            identification = request.Rnc,
            email = request.Email,
            phone = request.Phone,
            type = new[] { request.Type.ToString().ToLower() }
        };

        var response = await _httpClient.PostAsJsonAsync("contacts", payload);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<Contact>()
            ?? throw new Exception("Failed to create contact");
    }

    public async Task<Contact?> GetContactByRncAsync(string rnc)
    {
        var response = await _httpClient.GetAsync(
            $"contacts?identification={rnc}");

        if (!response.IsSuccessStatusCode)
            return null;

        var contacts = await response.Content
            .ReadFromJsonAsync<List<Contact>>();

        return contacts?.FirstOrDefault();
    }

    public async Task<Invoice> CreateInvoiceAsync(InvoiceRequest request)
    {
        // Buscar o crear cliente
        var contact = await GetContactByRncAsync(request.ClientRnc)
            ?? await CreateContactAsync(new ContactRequest(
                request.ClientName,
                request.ClientRnc,
                request.ClientEmail,
                null,
                ContactType.Client
            ));

        var payload = new
        {
            client = contact.Id,
            date = request.Date.ToString("yyyy-MM-dd"),
            dueDate = request.DueDate.ToString("yyyy-MM-dd"),
            items = request.Items.Select(i => new
            {
                name = i.Description,
                price = i.UnitPrice,
                quantity = i.Quantity,
                tax = new[] { new { id = 1 } } // ITBIS 18%
            }),
            stamp = new { generateStamp = true }
        };

        var response = await _httpClient.PostAsJsonAsync("invoices", payload);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<Invoice>()
            ?? throw new Exception("Failed to create invoice");
    }

    public async Task<Payment> RecordPaymentAsync(PaymentRequest request)
    {
        var payload = new
        {
            date = request.Date.ToString("yyyy-MM-dd"),
            bankAccount = request.BankAccountId,
            invoices = new[]
            {
                new { id = request.InvoiceId, amount = request.Amount }
            }
        };

        var response = await _httpClient.PostAsJsonAsync("payments", payload);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<Payment>()
            ?? throw new Exception("Failed to record payment");
    }

    public async Task<BalanceSheet> GetBalanceSheetAsync(DateTime asOf)
    {
        var response = await _httpClient.GetAsync(
            $"reports/balance-sheet?date={asOf:yyyy-MM-dd}");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<BalanceSheet>()
            ?? throw new Exception("Failed to get balance sheet");
    }

    public async Task<IncomeStatement> GetIncomeStatementAsync(
        DateTime from, DateTime to)
    {
        var response = await _httpClient.GetAsync(
            $"reports/income-statement?start={from:yyyy-MM-dd}&end={to:yyyy-MM-dd}");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<IncomeStatement>()
            ?? throw new Exception("Failed to get income statement");
    }

    public async Task SyncFromOklaAsync()
    {
        _logger.LogInformation("Starting sync from OKLA to Alegra");

        // 1. Sincronizar ventas del día
        // 2. Sincronizar pagos recibidos
        // 3. Sincronizar gastos
        // 4. Actualizar inventario

        _logger.LogInformation("Sync completed");
    }
}
```

---

## ⚛️ React Component

```tsx
// components/AccountingDashboard.tsx
import { useQuery } from "@tanstack/react-query";
import { accountingService } from "@/services/accountingService";
import { TrendingUp, TrendingDown, DollarSign } from "lucide-react";

export function AccountingDashboard() {
  const currentMonth = new Date().toISOString().slice(0, 7);

  const { data: income } = useQuery({
    queryKey: ["income-statement", currentMonth],
    queryFn: () =>
      accountingService.getIncomeStatement(
        `${currentMonth}-01`,
        new Date().toISOString().slice(0, 10)
      ),
  });

  const { data: balance } = useQuery({
    queryKey: ["balance-sheet"],
    queryFn: () =>
      accountingService.getBalanceSheet(new Date().toISOString().slice(0, 10)),
  });

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold">Dashboard Contable</h1>

      <div className="grid grid-cols-3 gap-4">
        <div className="bg-white rounded-lg shadow p-6">
          <div className="flex items-center gap-3">
            <div className="p-3 bg-green-100 rounded-full">
              <TrendingUp className="w-6 h-6 text-green-600" />
            </div>
            <div>
              <p className="text-sm text-gray-500">Ingresos del Mes</p>
              <p className="text-2xl font-bold">
                RD$ {income?.totalRevenue?.toLocaleString() || 0}
              </p>
            </div>
          </div>
        </div>

        <div className="bg-white rounded-lg shadow p-6">
          <div className="flex items-center gap-3">
            <div className="p-3 bg-red-100 rounded-full">
              <TrendingDown className="w-6 h-6 text-red-600" />
            </div>
            <div>
              <p className="text-sm text-gray-500">Gastos del Mes</p>
              <p className="text-2xl font-bold">
                RD$ {income?.totalExpenses?.toLocaleString() || 0}
              </p>
            </div>
          </div>
        </div>

        <div className="bg-white rounded-lg shadow p-6">
          <div className="flex items-center gap-3">
            <div className="p-3 bg-blue-100 rounded-full">
              <DollarSign className="w-6 h-6 text-blue-600" />
            </div>
            <div>
              <p className="text-sm text-gray-500">Utilidad Neta</p>
              <p
                className={`text-2xl font-bold ${
                  (income?.netIncome || 0) >= 0
                    ? "text-green-600"
                    : "text-red-600"
                }`}
              >
                RD$ {income?.netIncome?.toLocaleString() || 0}
              </p>
            </div>
          </div>
        </div>
      </div>

      <div className="grid grid-cols-2 gap-6">
        <div className="bg-white rounded-lg shadow p-6">
          <h2 className="text-lg font-bold mb-4">Activos</h2>
          <p className="text-3xl font-bold text-blue-600">
            RD$ {balance?.totalAssets?.toLocaleString() || 0}
          </p>
        </div>

        <div className="bg-white rounded-lg shadow p-6">
          <h2 className="text-lg font-bold mb-4">Pasivos</h2>
          <p className="text-3xl font-bold text-red-600">
            RD$ {balance?.totalLiabilities?.toLocaleString() || 0}
          </p>
        </div>
      </div>
    </div>
  );
}
```

---

## 🔧 Configuración

```json
{
  "Alegra": {
    "Email": "contabilidad@okla.com.do",
    "ApiToken": "xxxxx"
  }
}
```

---

## 🔄 Sincronización Automática

```csharp
// Background job para sincronización diaria
public class AccountingSyncJob : BackgroundService
{
    private readonly IServiceProvider _services;

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            using var scope = _services.CreateScope();
            var accounting = scope.ServiceProvider
                .GetRequiredService<IAccountingService>();

            await accounting.SyncFromOklaAsync();

            // Ejecutar cada 4 horas
            await Task.Delay(TimeSpan.FromHours(4), ct);
        }
    }
}
```

---

**Anterior:** [TSS_SUIR_API.md](./TSS_SUIR_API.md)  
**Próximo:** [DGII_DECLARACIONES_API.md](./DGII_DECLARACIONES_API.md)
