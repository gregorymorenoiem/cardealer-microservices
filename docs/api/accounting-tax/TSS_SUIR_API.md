# 👥 TSS/SUIR API - Seguridad Social

**Entidad:** Tesorería de la Seguridad Social (TSS)  
**Sistema:** SUIR (Sistema Único de Información y Recaudo)  
**Portal:** [suir.gob.do](https://suir.gob.do)  
**Última Actualización:** Enero 15, 2026

---

## 📋 Servicios de Seguridad Social RD

| Entidad      | Siglas                            | Función                 |
| ------------ | --------------------------------- | ----------------------- |
| **TSS**      | Tesorería Seguridad Social        | Recaudación aportes     |
| **CNSS**     | Consejo Nacional Seguridad Social | Regulador               |
| **SISALRIL** | Superintendencia Salud            | ARS y riesgos laborales |
| **SIPEN**    | Superintendencia Pensiones        | AFP                     |

---

## 🎯 Uso en OKLA

Como empleador, OKLA debe:

1. Registrar empleados en TSS
2. Reportar nómina mensualmente
3. Pagar aportes (AFP, ARS, Riesgos Laborales)
4. Mantener empleados afiliados a ARS

---

## 📊 Aportes Obligatorios (2026)

| Concepto              | Empleador | Empleado | Total    |
| --------------------- | --------- | -------- | -------- |
| **AFP (Pensiones)**   | 7.10%     | 2.87%    | 9.97%    |
| **SFS (Salud)**       | 7.09%     | 3.04%    | 10.13%   |
| **Riesgos Laborales** | 1.0-1.4%  | 0%       | Variable |
| **INFOTEP**           | 1.0%      | 0%       | 1.0%     |
| **TOTAL**             | ~16.19%   | ~5.91%   | ~22.1%   |

---

## 📡 Endpoints SUIR

### Autenticación

```http
POST https://suir.gob.do/api/auth/token
Content-Type: application/json

{
  "rnc": "123456789",
  "usuario": "empresa@email.com",
  "clave": "password"
}
```

### Reportar Nómina (TSS-4)

```http
POST https://suir.gob.do/api/nomina/reportar
Authorization: Bearer {token}
Content-Type: application/json

{
  "periodo": "2026-01",
  "empleados": [
    {
      "cedula": "001-1234567-8",
      "nombres": "Juan Pérez",
      "salarioCotizable": 50000.00,
      "diasTrabajados": 30,
      "tipoTrabajador": "01",
      "ars": "ARS-001",
      "afp": "AFP-001"
    }
  ]
}
```

### Consultar Empleado

```http
GET https://suir.gob.do/api/empleados/{cedula}
Authorization: Bearer {token}
```

### Generar Factura TSS

```http
POST https://suir.gob.do/api/factura/generar
Authorization: Bearer {token}

{
  "periodo": "2026-01"
}
```

---

## 💻 Implementación C#

### Domain Models

```csharp
namespace AccountingTaxService.Domain.Entities;

public class Employee
{
    public Guid Id { get; set; }
    public string Cedula { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public decimal BaseSalary { get; set; }
    public DateTime HireDate { get; set; }
    public string ArsCode { get; set; } = string.Empty;
    public string AfpCode { get; set; } = string.Empty;
    public EmployeeStatus Status { get; set; }
    public string? TssNumber { get; set; }
}

public enum EmployeeStatus
{
    Active,
    OnLeave,
    Terminated
}

public class PayrollEntry
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string Period { get; set; } = string.Empty; // "2026-01"
    public decimal GrossSalary { get; set; }
    public int WorkedDays { get; set; }
    public decimal AfpEmployer { get; set; }
    public decimal AfpEmployee { get; set; }
    public decimal SfsEmployer { get; set; }
    public decimal SfsEmployee { get; set; }
    public decimal RiskEmployer { get; set; }
    public decimal Infotep { get; set; }
    public decimal NetSalary { get; set; }
    public PayrollStatus Status { get; set; }
}

public enum PayrollStatus
{
    Draft,
    Calculated,
    Submitted,
    Paid
}
```

### Service Interface

```csharp
namespace AccountingTaxService.Domain.Interfaces;

public interface ITssService
{
    Task<bool> RegisterEmployeeAsync(Employee employee);
    Task<bool> TerminateEmployeeAsync(string cedula, DateTime date);
    Task<PayrollSubmissionResult> SubmitPayrollAsync(
        string period, List<PayrollEntry> entries);
    Task<TssInvoice> GenerateInvoiceAsync(string period);
    Task<EmployeeInfo?> GetEmployeeInfoAsync(string cedula);
    Task<List<Contribution>> GetContributionsAsync(
        string period);
}

public record PayrollSubmissionResult(
    bool Success,
    string? TrackingNumber,
    string? ErrorMessage,
    List<PayrollError>? Errors
);

public record PayrollError(
    string Cedula,
    string Field,
    string Message
);

public record TssInvoice(
    string InvoiceNumber,
    string Period,
    decimal TotalAfp,
    decimal TotalSfs,
    decimal TotalRisk,
    decimal TotalInfotep,
    decimal GrandTotal,
    DateTime DueDate,
    string PaymentReference
);

public record Contribution(
    string Cedula,
    string EmployeeName,
    decimal Salary,
    decimal AfpTotal,
    decimal SfsTotal,
    decimal RiskTotal
);
```

### TSS Service Implementation

```csharp
namespace AccountingTaxService.Infrastructure.Services;

public class TssService : ITssService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<TssService> _logger;
    private string? _token;

    private const decimal AFP_EMPLOYER_RATE = 0.0710m;
    private const decimal AFP_EMPLOYEE_RATE = 0.0287m;
    private const decimal SFS_EMPLOYER_RATE = 0.0709m;
    private const decimal SFS_EMPLOYEE_RATE = 0.0304m;
    private const decimal RISK_RATE = 0.0120m;
    private const decimal INFOTEP_RATE = 0.0100m;

    public TssService(
        HttpClient httpClient,
        IConfiguration config,
        ILogger<TssService> logger)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://suir.gob.do/api/");
        _config = config;
        _logger = logger;
    }

    public static PayrollEntry CalculateContributions(
        Employee employee, decimal grossSalary, int workedDays)
    {
        // Tope cotizable (actualizar anualmente)
        var topeCotizable = 204495.00m; // 4 salarios mínimos
        var salarioCotizable = Math.Min(grossSalary, topeCotizable);

        return new PayrollEntry
        {
            EmployeeId = employee.Id,
            GrossSalary = grossSalary,
            WorkedDays = workedDays,
            AfpEmployer = salarioCotizable * AFP_EMPLOYER_RATE,
            AfpEmployee = salarioCotizable * AFP_EMPLOYEE_RATE,
            SfsEmployer = salarioCotizable * SFS_EMPLOYER_RATE,
            SfsEmployee = salarioCotizable * SFS_EMPLOYEE_RATE,
            RiskEmployer = salarioCotizable * RISK_RATE,
            Infotep = salarioCotizable * INFOTEP_RATE,
            NetSalary = grossSalary
                - (salarioCotizable * AFP_EMPLOYEE_RATE)
                - (salarioCotizable * SFS_EMPLOYEE_RATE)
        };
    }

    public async Task<PayrollSubmissionResult> SubmitPayrollAsync(
        string period, List<PayrollEntry> entries)
    {
        await EnsureAuthenticatedAsync();

        var payload = new
        {
            periodo = period,
            empleados = entries.Select(e => new
            {
                cedula = e.Employee?.Cedula,
                salarioCotizable = e.GrossSalary,
                diasTrabajados = e.WorkedDays,
                afpEmpleador = e.AfpEmployer,
                afpEmpleado = e.AfpEmployee,
                sfsEmpleador = e.SfsEmployer,
                sfsEmpleado = e.SfsEmployee,
                rl = e.RiskEmployer,
                infotep = e.Infotep
            })
        };

        var response = await _httpClient.PostAsJsonAsync(
            "nomina/reportar", payload);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            _logger.LogError("TSS submission error: {Error}", error);
            return new PayrollSubmissionResult(false, null, error, null);
        }

        var result = await response.Content
            .ReadFromJsonAsync<TssResponse>();

        return new PayrollSubmissionResult(
            true,
            result!.NumeroSeguimiento,
            null,
            null
        );
    }

    public async Task<TssInvoice> GenerateInvoiceAsync(string period)
    {
        await EnsureAuthenticatedAsync();

        var response = await _httpClient.PostAsJsonAsync(
            "factura/generar", new { periodo = period });

        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<InvoiceResponse>();

        return new TssInvoice(
            result!.NumeroFactura,
            period,
            result.TotalAfp,
            result.TotalSfs,
            result.TotalRl,
            result.TotalInfotep,
            result.Total,
            result.FechaLimite,
            result.ReferenciaPago
        );
    }

    private async Task EnsureAuthenticatedAsync()
    {
        if (_token != null) return;

        var auth = new
        {
            rnc = _config["TSS:RNC"],
            usuario = _config["TSS:Usuario"],
            clave = _config["TSS:Clave"]
        };

        var response = await _httpClient.PostAsJsonAsync("auth/token", auth);
        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<TokenResponse>();
        _token = result!.Token;

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _token);
    }
}
```

---

## ⚛️ React Component

```tsx
// components/PayrollSummary.tsx
import { useQuery } from "@tanstack/react-query";
import { tssService } from "@/services/tssService";

export function PayrollSummary({ period }: { period: string }) {
  const { data, isLoading } = useQuery({
    queryKey: ["payroll", period],
    queryFn: () => tssService.getContributions(period),
  });

  const totals = data?.reduce(
    (acc, c) => ({
      salary: acc.salary + c.salary,
      afp: acc.afp + c.afpTotal,
      sfs: acc.sfs + c.sfsTotal,
      risk: acc.risk + c.riskTotal,
    }),
    { salary: 0, afp: 0, sfs: 0, risk: 0 }
  );

  return (
    <div className="bg-white rounded-lg shadow p-6">
      <h2 className="text-xl font-bold mb-4">Nómina TSS - {period}</h2>

      {isLoading ? (
        <p>Cargando...</p>
      ) : (
        <>
          <div className="grid grid-cols-4 gap-4 mb-6">
            <div className="text-center p-4 bg-blue-50 rounded">
              <p className="text-sm text-gray-500">Salarios</p>
              <p className="text-xl font-bold">
                RD$ {totals?.salary.toLocaleString()}
              </p>
            </div>
            <div className="text-center p-4 bg-green-50 rounded">
              <p className="text-sm text-gray-500">AFP</p>
              <p className="text-xl font-bold">
                RD$ {totals?.afp.toLocaleString()}
              </p>
            </div>
            <div className="text-center p-4 bg-purple-50 rounded">
              <p className="text-sm text-gray-500">SFS</p>
              <p className="text-xl font-bold">
                RD$ {totals?.sfs.toLocaleString()}
              </p>
            </div>
            <div className="text-center p-4 bg-orange-50 rounded">
              <p className="text-sm text-gray-500">R. Laborales</p>
              <p className="text-xl font-bold">
                RD$ {totals?.risk.toLocaleString()}
              </p>
            </div>
          </div>

          <table className="w-full text-sm">
            <thead>
              <tr className="border-b">
                <th className="text-left p-2">Empleado</th>
                <th className="text-right p-2">Salario</th>
                <th className="text-right p-2">AFP</th>
                <th className="text-right p-2">SFS</th>
              </tr>
            </thead>
            <tbody>
              {data?.map((c) => (
                <tr key={c.cedula} className="border-b">
                  <td className="p-2">{c.employeeName}</td>
                  <td className="text-right p-2">
                    RD$ {c.salary.toLocaleString()}
                  </td>
                  <td className="text-right p-2">
                    RD$ {c.afpTotal.toLocaleString()}
                  </td>
                  <td className="text-right p-2">
                    RD$ {c.sfsTotal.toLocaleString()}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </>
      )}
    </div>
  );
}
```

---

## 🧪 Tests

```csharp
public class TssServiceTests
{
    [Theory]
    [InlineData(50000, 3550, 1435)] // AFP: 7.1% empl, 2.87% emp
    [InlineData(100000, 7100, 2870)]
    public void CalculateContributions_CorrectAfp(
        decimal salary, decimal expectedEmployer, decimal expectedEmployee)
    {
        var employee = new Employee { Id = Guid.NewGuid() };

        var result = TssService.CalculateContributions(
            employee, salary, 30);

        result.AfpEmployer.Should().BeApproximately(
            expectedEmployer, 1);
        result.AfpEmployee.Should().BeApproximately(
            expectedEmployee, 1);
    }
}
```

---

## 📅 Fechas Límite

| Concepto        | Fecha Límite            |
| --------------- | ----------------------- |
| Reporte Nómina  | Día 3 del mes siguiente |
| Pago TSS        | Día 3 del mes siguiente |
| Rectificaciones | Hasta día 10            |

---

**Anterior:** [BANKING_APIS.md](./BANKING_APIS.md)  
**Próximo:** [DGII_DECLARACIONES_API.md](./DGII_DECLARACIONES_API.md)
