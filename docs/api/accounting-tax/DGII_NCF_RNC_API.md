# 🔍 DGII NCF/RNC API - Validación de Comprobantes

**Entidad:** Dirección General de Impuestos Internos (DGII)  
**Servicio:** Consulta y Validación de NCF/RNC  
**Ambiente:** `https://dgii.gov.do/app/WebApps/ConsultasWeb2/`  
**Costo:** GRATIS  
**Última Actualización:** Enero 15, 2026

---

## 📋 Servicios Disponibles

| Servicio              | Descripción                    | Uso en OKLA                     |
| --------------------- | ------------------------------ | ------------------------------- |
| **Validar NCF**       | Verificar si un NCF es válido  | Validar facturas de proveedores |
| **Consultar RNC**     | Obtener datos de contribuyente | Verificar clientes/proveedores  |
| **Consultar Cédula**  | Datos de persona física        | Verificar compradores           |
| **Estado Tributario** | Verificar si está al día       | Due diligence                   |

---

## 📡 Endpoints

### 1. Consultar RNC

```http
GET https://dgii.gov.do/app/WebApps/ConsultasWeb2/ConsultaRNCResult.aspx
    ?rnc=123456789
```

**Scraping Response (HTML → JSON):**

```json
{
  "rnc": "123456789",
  "razonSocial": "EMPRESA EJEMPLO SRL",
  "nombreComercial": "EMPRESA EJEMPLO",
  "categoria": "PERSONA JURIDICA",
  "regimen": "NORMAL",
  "estado": "ACTIVO",
  "actividadEconomica": "VENTA DE VEHICULOS",
  "direccion": "AV. WINSTON CHURCHILL 123, SANTO DOMINGO"
}
```

### 2. Validar NCF

```http
GET https://dgii.gov.do/app/WebApps/ConsultasWeb2/ConsultaNCF.aspx
    ?rnc=123456789
    &ncf=B0100000001
```

**Response:**

```json
{
  "ncfValido": true,
  "rncEmisor": "123456789",
  "razonSocial": "EMPRESA EJEMPLO SRL",
  "tipoComprobante": "01 - FACTURA DE CREDITO FISCAL",
  "fechaEmision": "2026-01-15",
  "fechaVencimiento": "2026-12-31",
  "estado": "VIGENTE"
}
```

### 3. Consultar Cédula

```http
GET https://dgii.gov.do/app/WebApps/ConsultasWeb2/ConsultaCedulaResult.aspx
    ?cedula=00112345678
```

**Response:**

```json
{
  "cedula": "001-1234567-8",
  "nombre": "JUAN",
  "apellido1": "PEREZ",
  "apellido2": "GARCIA",
  "nombreCompleto": "JUAN PEREZ GARCIA"
}
```

---

## 💻 Implementación C#

### Service Interface

```csharp
namespace AccountingTaxService.Domain.Interfaces;

public interface IRncService
{
    Task<RncInfo?> GetByRncAsync(string rnc);
    Task<RncInfo?> GetByCedulaAsync(string cedula);
    Task<NcfValidation> ValidateNcfAsync(string rnc, string ncf);
    Task<List<RncInfo>> SearchByNameAsync(string name);
    Task<TaxStatus> GetTaxStatusAsync(string rnc);
}

public record RncInfo(
    string Rnc,
    string RazonSocial,
    string? NombreComercial,
    string Categoria,
    string Regimen,
    string Estado,
    string? ActividadEconomica,
    string? Direccion
);

public record NcfValidation(
    bool IsValid,
    string? RncEmisor,
    string? RazonSocial,
    string? TipoComprobante,
    DateTime? FechaEmision,
    DateTime? FechaVencimiento,
    string? Estado,
    string? ErrorMessage
);

public record TaxStatus(
    bool IsActive,
    bool HasPendingDeclarations,
    DateTime? LastDeclarationDate
);
```

### Service Implementation

```csharp
namespace AccountingTaxService.Infrastructure.Services;

public class DgiiRncService : IRncService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DgiiRncService> _logger;
    private readonly IMemoryCache _cache;
    private const string BaseUrl = "https://dgii.gov.do/app/WebApps/ConsultasWeb2/";

    public DgiiRncService(
        HttpClient httpClient,
        ILogger<DgiiRncService> logger,
        IMemoryCache cache)
    {
        _httpClient = httpClient;
        _logger = logger;
        _cache = cache;
    }

    public async Task<RncInfo?> GetByRncAsync(string rnc)
    {
        // Normalizar RNC (quitar guiones)
        rnc = NormalizeRnc(rnc);

        // Verificar cache (RNC no cambia frecuentemente)
        var cacheKey = $"rnc:{rnc}";
        if (_cache.TryGetValue(cacheKey, out RncInfo? cached))
            return cached;

        try
        {
            var url = $"{BaseUrl}ConsultaRNCResult.aspx?rnc={rnc}";
            var html = await _httpClient.GetStringAsync(url);

            var result = ParseRncHtml(html);

            if (result != null)
            {
                _cache.Set(cacheKey, result, TimeSpan.FromHours(24));
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error consulting RNC {Rnc}", rnc);
            return null;
        }
    }

    public async Task<RncInfo?> GetByCedulaAsync(string cedula)
    {
        cedula = NormalizeCedula(cedula);

        try
        {
            var url = $"{BaseUrl}ConsultaCedulaResult.aspx?cedula={cedula}";
            var html = await _httpClient.GetStringAsync(url);

            return ParseCedulaHtml(html);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error consulting cedula");
            return null;
        }
    }

    public async Task<NcfValidation> ValidateNcfAsync(string rnc, string ncf)
    {
        rnc = NormalizeRnc(rnc);

        try
        {
            var url = $"{BaseUrl}ConsultaNCF.aspx?rnc={rnc}&ncf={ncf}";
            var html = await _httpClient.GetStringAsync(url);

            return ParseNcfHtml(html);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating NCF");
            return new NcfValidation(
                false, null, null, null, null, null, null,
                ex.Message
            );
        }
    }

    public async Task<List<RncInfo>> SearchByNameAsync(string name)
    {
        try
        {
            var url = $"{BaseUrl}ConsultaRNCNombre.aspx?nombre={Uri.EscapeDataString(name)}";
            var html = await _httpClient.GetStringAsync(url);

            return ParseSearchHtml(html);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching by name");
            return new List<RncInfo>();
        }
    }

    private static string NormalizeRnc(string rnc)
    {
        return rnc.Replace("-", "").Replace(" ", "").Trim();
    }

    private static string NormalizeCedula(string cedula)
    {
        return cedula.Replace("-", "").Replace(" ", "").Trim();
    }

    private RncInfo? ParseRncHtml(string html)
    {
        // Usar HtmlAgilityPack para parsear
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var rncNode = doc.DocumentNode.SelectSingleNode("//td[@id='lblRNC']");
        if (rncNode == null) return null;

        return new RncInfo(
            Rnc: rncNode.InnerText.Trim(),
            RazonSocial: GetNodeText(doc, "lblRazonSocial"),
            NombreComercial: GetNodeText(doc, "lblNombreComercial"),
            Categoria: GetNodeText(doc, "lblCategoria"),
            Regimen: GetNodeText(doc, "lblRegimen"),
            Estado: GetNodeText(doc, "lblEstado"),
            ActividadEconomica: GetNodeText(doc, "lblActividadEconomica"),
            Direccion: GetNodeText(doc, "lblDireccion")
        );
    }

    private static string GetNodeText(HtmlDocument doc, string id)
    {
        var node = doc.DocumentNode.SelectSingleNode($"//td[@id='{id}']");
        return node?.InnerText.Trim() ?? string.Empty;
    }

    private NcfValidation ParseNcfHtml(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var validNode = doc.DocumentNode.SelectSingleNode("//td[@id='lblEstado']");
        var isValid = validNode?.InnerText.Contains("VIGENTE") ?? false;

        return new NcfValidation(
            IsValid: isValid,
            RncEmisor: GetNodeText(doc, "lblRNC"),
            RazonSocial: GetNodeText(doc, "lblRazonSocial"),
            TipoComprobante: GetNodeText(doc, "lblTipoNCF"),
            FechaEmision: ParseDate(GetNodeText(doc, "lblFechaEmision")),
            FechaVencimiento: ParseDate(GetNodeText(doc, "lblFechaVencimiento")),
            Estado: GetNodeText(doc, "lblEstado"),
            ErrorMessage: isValid ? null : "NCF no válido o vencido"
        );
    }

    private static DateTime? ParseDate(string dateStr)
    {
        if (DateTime.TryParse(dateStr, out var date))
            return date;
        return null;
    }
}
```

---

## ⚛️ React Components

### RNC Lookup

```tsx
// components/RncLookup.tsx
import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { rncService } from "@/services/rncService";

export function RncLookup() {
  const [rnc, setRnc] = useState("");
  const [search, setSearch] = useState("");

  const { data, isLoading, error } = useQuery({
    queryKey: ["rnc", search],
    queryFn: () => rncService.getByRnc(search),
    enabled: search.length >= 9,
  });

  return (
    <div className="bg-white rounded-lg shadow p-6">
      <h2 className="text-xl font-bold mb-4">Consultar RNC</h2>

      <div className="flex gap-2">
        <input
          placeholder="RNC (ej: 123456789)"
          value={rnc}
          onChange={(e) => setRnc(e.target.value)}
          className="flex-1 border p-2 rounded"
          maxLength={11}
        />
        <button
          onClick={() => setSearch(rnc)}
          className="bg-blue-600 text-white px-4 rounded"
        >
          Buscar
        </button>
      </div>

      {isLoading && <p className="mt-4">Buscando...</p>}

      {error && <p className="mt-4 text-red-600">Error al consultar</p>}

      {data && (
        <div className="mt-4 border rounded p-4">
          <h3 className="font-bold">{data.razonSocial}</h3>
          <p className="text-gray-600">{data.nombreComercial}</p>
          <div className="grid grid-cols-2 gap-2 mt-2 text-sm">
            <span>RNC:</span>
            <span>{data.rnc}</span>
            <span>Estado:</span>
            <span
              className={
                data.estado === "ACTIVO" ? "text-green-600" : "text-red-600"
              }
            >
              {data.estado}
            </span>
            <span>Régimen:</span>
            <span>{data.regimen}</span>
            <span>Actividad:</span>
            <span>{data.actividadEconomica}</span>
          </div>
        </div>
      )}
    </div>
  );
}
```

### NCF Validator

```tsx
// components/NcfValidator.tsx
import { useState } from "react";
import { useMutation } from "@tanstack/react-query";
import { Check, X } from "lucide-react";

export function NcfValidator() {
  const [rnc, setRnc] = useState("");
  const [ncf, setNcf] = useState("");

  const validateMutation = useMutation({
    mutationFn: () => rncService.validateNcf(rnc, ncf),
  });

  return (
    <div className="bg-white rounded-lg shadow p-6">
      <h2 className="text-xl font-bold mb-4">Validar NCF</h2>

      <div className="space-y-3">
        <input
          placeholder="RNC del Emisor"
          value={rnc}
          onChange={(e) => setRnc(e.target.value)}
          className="w-full border p-2 rounded"
        />
        <input
          placeholder="NCF (ej: B0100000001)"
          value={ncf}
          onChange={(e) => setNcf(e.target.value.toUpperCase())}
          className="w-full border p-2 rounded"
        />
        <button
          onClick={() => validateMutation.mutate()}
          disabled={validateMutation.isPending}
          className="w-full bg-green-600 text-white py-2 rounded"
        >
          Validar
        </button>
      </div>

      {validateMutation.data && (
        <div
          className={`mt-4 p-4 rounded flex items-center gap-2 ${
            validateMutation.data.isValid
              ? "bg-green-100 text-green-800"
              : "bg-red-100 text-red-800"
          }`}
        >
          {validateMutation.data.isValid ? (
            <>
              <Check className="w-5 h-5" />
              <span>NCF Válido - {validateMutation.data.razonSocial}</span>
            </>
          ) : (
            <>
              <X className="w-5 h-5" />
              <span>NCF Inválido o Vencido</span>
            </>
          )}
        </div>
      )}
    </div>
  );
}
```

---

## 🧪 Tests

```csharp
public class DgiiRncServiceTests
{
    [Theory]
    [InlineData("130297118", true)]  // DGII oficial
    [InlineData("999999999", false)] // No existe
    public async Task GetByRncAsync_ReturnsExpectedResult(
        string rnc, bool shouldExist)
    {
        var service = new DgiiRncService(...);

        var result = await service.GetByRncAsync(rnc);

        if (shouldExist)
            result.Should().NotBeNull();
        else
            result.Should().BeNull();
    }

    [Fact]
    public async Task ValidateNcfAsync_ValidNcf_ReturnsTrue()
    {
        var service = new DgiiRncService(...);

        var result = await service.ValidateNcfAsync(
            "130297118", "B0100000001");

        result.IsValid.Should().BeTrue();
    }
}
```

---

## 🔧 Troubleshooting

| Error               | Causa                 | Solución          |
| ------------------- | --------------------- | ----------------- |
| `RNC no encontrado` | RNC no existe         | Verificar dígitos |
| `Timeout`           | DGII lento            | Implementar retry |
| `HTML cambió`       | DGII actualizó página | Actualizar parser |
| `Rate limited`      | Muchas consultas      | Implementar cache |

---

**Anterior:** [DGII_ECF_API.md](./DGII_ECF_API.md)  
**Próximo:** [DGII_DECLARACIONES_API.md](./DGII_DECLARACIONES_API.md)
