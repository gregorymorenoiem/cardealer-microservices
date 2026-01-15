# 🏦 APIs Bancarias - República Dominicana

**Categoría:** Banking & Open Banking  
**País:** República Dominicana 🇩🇴  
**Regulador:** Superintendencia de Bancos  
**Última Actualización:** Enero 15, 2026

---

## 📋 Bancos con APIs Disponibles

| Banco              | API Tipo | Autenticación | Funcionalidades                  |
| ------------------ | -------- | ------------- | -------------------------------- |
| **Banco Popular**  | REST     | OAuth 2.0     | Consultas, Transferencias, Pagos |
| **Banreservas**    | REST     | API Key       | Consultas, Pagos                 |
| **BHD León**       | REST     | OAuth 2.0     | Consultas, Transferencias        |
| **Scotiabank**     | SOAP     | Certificado   | Solo consultas                   |
| **ACH Dominicana** | REST     | Certificado   | Transferencias interbancarias    |

---

## 🌟 BANCO POPULAR (Principal para OKLA)

### Información General

| Campo               | Valor                                                              |
| ------------------- | ------------------------------------------------------------------ |
| **Portal Empresas** | [popularenlinea.com/empresas](https://popularenlinea.com/empresas) |
| **API Base**        | `https://api.bpd.com.do/v1/`                                       |
| **Documentación**   | Portal desarrolladores BPD                                         |
| **Autenticación**   | OAuth 2.0                                                          |

### Endpoints Principales

```http
# Autenticación
POST /oauth/token
Content-Type: application/x-www-form-urlencoded

grant_type=client_credentials
&client_id={client_id}
&client_secret={client_secret}

# Consultar Cuentas
GET /accounts
Authorization: Bearer {token}

# Consultar Balance
GET /accounts/{accountId}/balance

# Movimientos
GET /accounts/{accountId}/transactions
    ?from=2026-01-01
    &to=2026-01-31

# Transferencia
POST /transfers
{
  "sourceAccount": "123456789",
  "destinationAccount": "987654321",
  "amount": 50000.00,
  "currency": "DOP",
  "concept": "Pago proveedor",
  "reference": "OKLA-001"
}
```

---

## 💳 AZUL (Procesador de Pagos)

### Información

| Campo        | Valor                                    |
| ------------ | ---------------------------------------- |
| **Website**  | [azul.com.do](https://azul.com.do)       |
| **API Base** | `https://pagos.azul.com.do/webservices/` |
| **Comisión** | 2.5-3.5% por transacción                 |

### Endpoints

```http
# Procesar Pago
POST /JSON/Default.aspx
{
  "Channel": "EC",
  "Store": "12345",
  "PosInputMode": "E-Commerce",
  "TrxType": "Sale",
  "Amount": 100000,
  "Itbis": 18000,
  "CardNumber": "4111111111111111",
  "Expiration": "202612",
  "CVC": "123",
  "CustomOrderId": "OKLA-VEH-001"
}

# Consultar Transacción
POST /JSON/Default.aspx
{
  "TrxType": "Inquiry",
  "AzulOrderId": "12345678"
}

# Anular/Reversar
POST /JSON/Default.aspx
{
  "TrxType": "Void",
  "AzulOrderId": "12345678"
}
```

---

## 💻 Implementación C#

### Service Interface

```csharp
namespace AccountingTaxService.Domain.Interfaces;

public interface IBankingService
{
    Task<List<BankAccount>> GetAccountsAsync();
    Task<AccountBalance> GetBalanceAsync(string accountId);
    Task<List<Transaction>> GetTransactionsAsync(
        string accountId, DateTime from, DateTime to);
    Task<TransferResult> TransferAsync(TransferRequest request);
    Task<ReconciliationResult> ReconcileAsync(
        string accountId, DateTime date);
}

public record BankAccount(
    string Id,
    string Number,
    string Name,
    string Type,
    string Currency,
    decimal AvailableBalance
);

public record AccountBalance(
    decimal Available,
    decimal Current,
    decimal Pending,
    DateTime AsOf
);

public record Transaction(
    string Id,
    DateTime Date,
    string Description,
    decimal Amount,
    string Type,
    decimal Balance,
    string? Reference
);

public record TransferRequest(
    string SourceAccount,
    string DestinationAccount,
    decimal Amount,
    string Concept,
    string? Reference
);

public record TransferResult(
    bool Success,
    string? TransferId,
    string? ConfirmationCode,
    string? ErrorMessage
);

public record ReconciliationResult(
    int TotalTransactions,
    int Matched,
    int Unmatched,
    decimal Difference,
    List<Transaction> UnmatchedTransactions
);
```

### Banco Popular Service

```csharp
namespace AccountingTaxService.Infrastructure.Services;

public class BancoPopularService : IBankingService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<BancoPopularService> _logger;
    private string? _token;
    private DateTime _tokenExpiry;

    public BancoPopularService(
        HttpClient httpClient,
        IConfiguration config,
        ILogger<BancoPopularService> logger)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://api.bpd.com.do/v1/");
        _config = config;
        _logger = logger;
    }

    private async Task EnsureAuthenticatedAsync()
    {
        if (_token != null && DateTime.UtcNow < _tokenExpiry)
            return;

        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = _config["BancoPopular:ClientId"]!,
            ["client_secret"] = _config["BancoPopular:ClientSecret"]!
        });

        var response = await _httpClient.PostAsync("oauth/token", content);
        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<OAuthResponse>();

        _token = result!.AccessToken;
        _tokenExpiry = DateTime.UtcNow.AddSeconds(result.ExpiresIn - 60);

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _token);
    }

    public async Task<List<BankAccount>> GetAccountsAsync()
    {
        await EnsureAuthenticatedAsync();

        var response = await _httpClient.GetAsync("accounts");
        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<AccountsResponse>();

        return result!.Accounts.Select(a => new BankAccount(
            a.Id,
            a.Number,
            a.Name,
            a.Type,
            a.Currency,
            a.AvailableBalance
        )).ToList();
    }

    public async Task<AccountBalance> GetBalanceAsync(string accountId)
    {
        await EnsureAuthenticatedAsync();

        var response = await _httpClient.GetAsync(
            $"accounts/{accountId}/balance");
        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<BalanceResponse>();

        return new AccountBalance(
            result!.Available,
            result.Current,
            result.Pending,
            DateTime.UtcNow
        );
    }

    public async Task<List<Transaction>> GetTransactionsAsync(
        string accountId, DateTime from, DateTime to)
    {
        await EnsureAuthenticatedAsync();

        var response = await _httpClient.GetAsync(
            $"accounts/{accountId}/transactions" +
            $"?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}");

        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<TransactionsResponse>();

        return result!.Transactions.Select(t => new Transaction(
            t.Id,
            t.Date,
            t.Description,
            t.Amount,
            t.Type,
            t.Balance,
            t.Reference
        )).ToList();
    }

    public async Task<TransferResult> TransferAsync(TransferRequest request)
    {
        await EnsureAuthenticatedAsync();

        var payload = new
        {
            sourceAccount = request.SourceAccount,
            destinationAccount = request.DestinationAccount,
            amount = request.Amount,
            currency = "DOP",
            concept = request.Concept,
            reference = request.Reference
        };

        var response = await _httpClient.PostAsJsonAsync("transfers", payload);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            return new TransferResult(false, null, null, error);
        }

        var result = await response.Content
            .ReadFromJsonAsync<TransferResponse>();

        return new TransferResult(
            true,
            result!.TransferId,
            result.ConfirmationCode,
            null
        );
    }

    public async Task<ReconciliationResult> ReconcileAsync(
        string accountId, DateTime date)
    {
        var bankTx = await GetTransactionsAsync(
            accountId, date, date.AddDays(1));

        // Comparar con transacciones internas
        // Lógica de reconciliación...

        return new ReconciliationResult(
            bankTx.Count,
            bankTx.Count, // Matched
            0, // Unmatched
            0, // Difference
            new List<Transaction>()
        );
    }
}
```

---

## ⚛️ React Components

### Account Balance Card

```tsx
// components/BankAccountCard.tsx
import { useQuery } from "@tanstack/react-query";
import { bankingService } from "@/services/bankingService";

export function BankAccountCard({ accountId }: { accountId: string }) {
  const { data: balance, isLoading } = useQuery({
    queryKey: ["balance", accountId],
    queryFn: () => bankingService.getBalance(accountId),
    refetchInterval: 60000, // Cada minuto
  });

  if (isLoading) return <div>Cargando...</div>;

  return (
    <div className="bg-white rounded-lg shadow p-6">
      <h3 className="text-gray-500 text-sm">Balance Disponible</h3>
      <p className="text-3xl font-bold text-green-600">
        RD$ {balance?.available.toLocaleString()}
      </p>
      <div className="mt-4 text-sm text-gray-500">
        <p>Balance actual: RD$ {balance?.current.toLocaleString()}</p>
        <p>Pendiente: RD$ {balance?.pending.toLocaleString()}</p>
      </div>
    </div>
  );
}
```

### Transaction History

```tsx
// components/TransactionHistory.tsx
import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { ArrowUpRight, ArrowDownLeft } from "lucide-react";

export function TransactionHistory({ accountId }: { accountId: string }) {
  const [dateRange, setDateRange] = useState({
    from: new Date(Date.now() - 30 * 24 * 60 * 60 * 1000)
      .toISOString()
      .slice(0, 10),
    to: new Date().toISOString().slice(0, 10),
  });

  const { data: transactions } = useQuery({
    queryKey: ["transactions", accountId, dateRange],
    queryFn: () =>
      bankingService.getTransactions(accountId, dateRange.from, dateRange.to),
  });

  return (
    <div className="bg-white rounded-lg shadow">
      <div className="p-4 border-b">
        <h2 className="text-xl font-bold">Movimientos</h2>
      </div>
      <div className="divide-y">
        {transactions?.map((tx) => (
          <div key={tx.id} className="p-4 flex items-center">
            <div
              className={`p-2 rounded-full mr-3 ${
                tx.amount > 0 ? "bg-green-100" : "bg-red-100"
              }`}
            >
              {tx.amount > 0 ? (
                <ArrowDownLeft className="w-4 h-4 text-green-600" />
              ) : (
                <ArrowUpRight className="w-4 h-4 text-red-600" />
              )}
            </div>
            <div className="flex-1">
              <p className="font-medium">{tx.description}</p>
              <p className="text-sm text-gray-500">
                {new Date(tx.date).toLocaleDateString()}
              </p>
            </div>
            <div
              className={`font-bold ${
                tx.amount > 0 ? "text-green-600" : "text-red-600"
              }`}
            >
              {tx.amount > 0 ? "+" : ""}
              RD$ {tx.amount.toLocaleString()}
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
```

---

## 🧪 Tests

```csharp
public class BancoPopularServiceTests
{
    [Fact]
    public async Task GetAccountsAsync_ReturnsAccounts()
    {
        var service = CreateService();

        var accounts = await service.GetAccountsAsync();

        accounts.Should().NotBeEmpty();
        accounts.First().Number.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetBalanceAsync_ReturnsBalance()
    {
        var service = CreateService();

        var balance = await service.GetBalanceAsync("123456");

        balance.Available.Should().BeGreaterOrEqualTo(0);
    }
}
```

---

## 🔧 Configuración

```json
{
  "BancoPopular": {
    "ClientId": "xxxxx",
    "ClientSecret": "xxxxx",
    "Environment": "production"
  },
  "Azul": {
    "MerchantId": "12345",
    "AuthKey": "xxxxx"
  }
}
```

---

## 🔐 Seguridad

- Todas las comunicaciones via HTTPS/TLS 1.2+
- Tokens con expiración corta (15-30 min)
- IP Whitelisting requerido por bancos
- Logs de auditoría obligatorios

---

**Anterior:** [FACTURACION_ELECTRONICA_PROVIDERS.md](./FACTURACION_ELECTRONICA_PROVIDERS.md)  
**Próximo:** [TSS_SUIR_API.md](./TSS_SUIR_API.md)
