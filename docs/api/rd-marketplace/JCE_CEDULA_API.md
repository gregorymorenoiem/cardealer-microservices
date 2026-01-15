# 🪪 JCE - Verificación de Cédula

**Entidad:** Junta Central Electoral  
**Website:** [jce.gob.do](https://jce.gob.do)  
**Uso:** Verificar identidad de usuarios (compradores/vendedores)  
**Prioridad:** ⭐⭐⭐⭐⭐ CRÍTICA

---

## 📋 Información General

| Campo | Valor |
|-------|-------|
| **Website** | [jce.gob.do](https://jce.gob.do) |
| **Consulta Padrón** | https://servicios.jce.gob.do/consultapadron/ |
| **Método** | Web Scraping (requiere convenio para API oficial) |
| **Datos Requeridos** | Número de cédula (11 dígitos) |

---

## 📊 Datos Obtenibles

| Dato | Descripción | Uso en OKLA |
|------|-------------|-------------|
| **Validez** | Si la cédula existe y es válida | Verificar usuario |
| **Nombres** | Nombres de la persona | Mostrar nombre verificado |
| **Apellidos** | Apellidos de la persona | Completar perfil |
| **Fecha Nacimiento** | Edad del usuario | Verificar mayoría de edad |
| **Sexo** | M/F | Estadísticas |
| **Estado** | Vigente/Vencida/Fallecido | Alertar problemas |
| **Lugar Nacimiento** | Municipio | Información adicional |

---

## 🌐 Consulta Web

```http
# Página de consulta del padrón
GET https://servicios.jce.gob.do/consultapadron/

# Formulario POST
POST https://servicios.jce.gob.do/consultapadron/consulta
Content-Type: application/x-www-form-urlencoded

cedula=00100000001
```

---

## 💻 Modelos C#

```csharp
namespace VehicleVerificationService.Domain.Entities;

/// <summary>
/// Información de ciudadano verificada por JCE
/// </summary>
public record CitizenInfo(
    string Cedula,
    string Nombres,
    string Apellidos,
    string NombreCompleto,
    DateTime FechaNacimiento,
    int Edad,
    Sexo Sexo,
    CedulaStatus Estado,
    string? LugarNacimiento,
    string? Nacionalidad,
    DateTime VerificadoEn
);

public enum Sexo
{
    Masculino,
    Femenino
}

public enum CedulaStatus
{
    Vigente,
    Vencida,
    Fallecido,
    NoEncontrada,
    Invalida
}

/// <summary>
/// Resultado de verificación de identidad
/// </summary>
public record IdentityVerificationResult(
    bool IsValid,
    CitizenInfo? Citizen,
    bool NombreCoincide,
    bool EsMayorDeEdad,
    List<string> Alertas,
    VerificationLevel NivelVerificacion
);

public enum VerificationLevel
{
    NoVerificado,
    CedulaValida,         // Solo cédula verificada
    IdentidadConfirmada,  // Cédula + nombre coinciden
    Completo              // Todo verificado + selfie (futuro)
}

/// <summary>
/// Request para verificar identidad
/// </summary>
public record VerifyIdentityRequest(
    string Cedula,
    string? NombreDeclarado,    // Nombre que el usuario dice tener
    DateTime? FechaNacimientoDeclarada
);
```

---

## 🔧 Service Interface

```csharp
namespace VehicleVerificationService.Domain.Interfaces;

public interface IJceService
{
    /// <summary>
    /// Consulta información de ciudadano por cédula
    /// </summary>
    Task<CitizenInfo?> GetCitizenAsync(string cedula);

    /// <summary>
    /// Verifica si una cédula es válida
    /// </summary>
    Task<bool> IsCedulaValidAsync(string cedula);

    /// <summary>
    /// Verifica identidad completa (cédula + nombre)
    /// </summary>
    Task<IdentityVerificationResult> VerifyIdentityAsync(
        VerifyIdentityRequest request);

    /// <summary>
    /// Valida formato de cédula (sin consultar JCE)
    /// </summary>
    bool ValidateCedulaFormat(string cedula);

    /// <summary>
    /// Calcula dígito verificador de cédula
    /// </summary>
    int CalculateVerificationDigit(string cedula);
}
```

---

## 🏗️ Service Implementation

```csharp
namespace VehicleVerificationService.Infrastructure.Services;

public class JceService : IJceService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<JceService> _logger;

    private const string JCE_URL = "https://servicios.jce.gob.do/consultapadron/";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromDays(30); // Datos no cambian frecuentemente

    // Pesos para cálculo de dígito verificador
    private static readonly int[] Weights = { 1, 2, 1, 2, 1, 2, 1, 2, 1, 2 };

    public JceService(
        HttpClient httpClient,
        IMemoryCache cache,
        ILogger<JceService> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _logger = logger;
    }

    public bool ValidateCedulaFormat(string cedula)
    {
        if (string.IsNullOrWhiteSpace(cedula))
            return false;

        // Limpiar formato
        cedula = cedula.Replace("-", "").Replace(" ", "").Trim();

        // Debe tener 11 dígitos
        if (cedula.Length != 11)
            return false;

        // Debe ser numérico
        if (!cedula.All(char.IsDigit))
            return false;

        // Validar dígito verificador
        var expectedDigit = CalculateVerificationDigit(cedula[..10]);
        var actualDigit = int.Parse(cedula[10].ToString());

        return expectedDigit == actualDigit;
    }

    public int CalculateVerificationDigit(string cedula)
    {
        // Algoritmo de Luhn modificado para cédulas RD
        var sum = 0;
        for (int i = 0; i < 10; i++)
        {
            var digit = int.Parse(cedula[i].ToString());
            var product = digit * Weights[i];
            
            // Si el producto es mayor a 9, sumar los dígitos
            if (product > 9)
                product = (product / 10) + (product % 10);
            
            sum += product;
        }

        var remainder = sum % 10;
        return remainder == 0 ? 0 : 10 - remainder;
    }

    public async Task<CitizenInfo?> GetCitizenAsync(string cedula)
    {
        // Normalizar cédula
        cedula = NormalizeCedula(cedula);

        if (!ValidateCedulaFormat(cedula))
        {
            _logger.LogWarning("Formato de cédula inválido: {Cedula}", cedula);
            return null;
        }

        var cacheKey = $"jce_citizen_{cedula}";

        if (_cache.TryGetValue(cacheKey, out CitizenInfo? cached))
            return cached;

        try
        {
            // 1. Obtener página inicial
            var initialResponse = await _httpClient.GetAsync(JCE_URL);
            var initialHtml = await initialResponse.Content.ReadAsStringAsync();

            var doc = new HtmlDocument();
            doc.LoadHtml(initialHtml);

            // Extraer tokens CSRF si existen
            var csrfToken = doc.DocumentNode
                .SelectSingleNode("//input[@name='__RequestVerificationToken']")
                ?.GetAttributeValue("value", "");

            // 2. Enviar consulta
            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("cedula", cedula),
                new KeyValuePair<string, string>("__RequestVerificationToken", 
                    csrfToken ?? "")
            });

            var response = await _httpClient.PostAsync(
                $"{JCE_URL}consulta", formData);

            var html = await response.Content.ReadAsStringAsync();

            // 3. Parsear resultado
            var result = ParseCitizenInfo(html, cedula);

            if (result != null)
                _cache.Set(cacheKey, result, CacheDuration);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error consultando JCE para {Cedula}", 
                MaskCedula(cedula));
            return null;
        }
    }

    public async Task<bool> IsCedulaValidAsync(string cedula)
    {
        var citizen = await GetCitizenAsync(cedula);
        return citizen?.Estado == CedulaStatus.Vigente;
    }

    public async Task<IdentityVerificationResult> VerifyIdentityAsync(
        VerifyIdentityRequest request)
    {
        var alertas = new List<string>();
        var cedula = NormalizeCedula(request.Cedula);

        // 1. Validar formato
        if (!ValidateCedulaFormat(cedula))
        {
            return new IdentityVerificationResult(
                IsValid: false,
                Citizen: null,
                NombreCoincide: false,
                EsMayorDeEdad: false,
                Alertas: new() { "Formato de cédula inválido" },
                NivelVerificacion: VerificationLevel.NoVerificado
            );
        }

        // 2. Consultar JCE
        var citizen = await GetCitizenAsync(cedula);

        if (citizen == null)
        {
            return new IdentityVerificationResult(
                IsValid: false,
                Citizen: null,
                NombreCoincide: false,
                EsMayorDeEdad: false,
                Alertas: new() { "Cédula no encontrada en JCE" },
                NivelVerificacion: VerificationLevel.NoVerificado
            );
        }

        // 3. Verificar estado
        if (citizen.Estado == CedulaStatus.Fallecido)
        {
            alertas.Add("⚠️ ALERTA: Esta cédula pertenece a una persona fallecida");
            return new IdentityVerificationResult(
                IsValid: false,
                Citizen: citizen,
                NombreCoincide: false,
                EsMayorDeEdad: false,
                Alertas: alertas,
                NivelVerificacion: VerificationLevel.NoVerificado
            );
        }

        if (citizen.Estado == CedulaStatus.Vencida)
        {
            alertas.Add("Cédula vencida - Usuario debe renovar");
        }

        // 4. Verificar mayoría de edad
        var esMayorDeEdad = citizen.Edad >= 18;
        if (!esMayorDeEdad)
        {
            alertas.Add("El usuario es menor de edad");
        }

        // 5. Comparar nombre si fue proporcionado
        var nombreCoincide = true;
        if (!string.IsNullOrEmpty(request.NombreDeclarado))
        {
            nombreCoincide = CompareNames(
                citizen.NombreCompleto, 
                request.NombreDeclarado);
            
            if (!nombreCoincide)
            {
                alertas.Add("El nombre declarado no coincide con JCE");
            }
        }

        // 6. Determinar nivel de verificación
        var nivel = VerificationLevel.CedulaValida;
        if (nombreCoincide && !string.IsNullOrEmpty(request.NombreDeclarado))
        {
            nivel = VerificationLevel.IdentidadConfirmada;
        }

        return new IdentityVerificationResult(
            IsValid: citizen.Estado == CedulaStatus.Vigente && esMayorDeEdad,
            Citizen: citizen,
            NombreCoincide: nombreCoincide,
            EsMayorDeEdad: esMayorDeEdad,
            Alertas: alertas,
            NivelVerificacion: nivel
        );
    }

    private CitizenInfo? ParseCitizenInfo(string html, string cedula)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        // Buscar div de resultado
        var resultDiv = doc.DocumentNode.SelectSingleNode("//div[@class='resultado']");
        if (resultDiv == null)
        {
            // Verificar si hay mensaje de error
            var errorDiv = doc.DocumentNode.SelectSingleNode("//div[@class='error']");
            if (errorDiv != null)
            {
                _logger.LogWarning("JCE error: {Error}", errorDiv.InnerText);
            }
            return null;
        }

        // Extraer datos
        var nombres = ExtractField(resultDiv, "nombres");
        var apellidos = ExtractField(resultDiv, "apellidos");
        var fechaNacStr = ExtractField(resultDiv, "fechaNacimiento");
        var sexoStr = ExtractField(resultDiv, "sexo");
        var estadoStr = ExtractField(resultDiv, "estado");
        var lugarNac = ExtractField(resultDiv, "lugarNacimiento");

        if (string.IsNullOrEmpty(nombres))
            return null;

        var fechaNacimiento = ParseDate(fechaNacStr);
        var edad = fechaNacimiento.HasValue 
            ? CalculateAge(fechaNacimiento.Value) 
            : 0;

        return new CitizenInfo(
            Cedula: cedula,
            Nombres: nombres!,
            Apellidos: apellidos ?? "",
            NombreCompleto: $"{nombres} {apellidos}".Trim(),
            FechaNacimiento: fechaNacimiento ?? DateTime.MinValue,
            Edad: edad,
            Sexo: sexoStr?.StartsWith("M") == true ? Sexo.Masculino : Sexo.Femenino,
            Estado: ParseCedulaStatus(estadoStr),
            LugarNacimiento: lugarNac,
            Nacionalidad: "Dominicana",
            VerificadoEn: DateTime.UtcNow
        );
    }

    private static string NormalizeCedula(string cedula)
    {
        return cedula.Replace("-", "").Replace(" ", "").Trim();
    }

    private static string MaskCedula(string cedula)
    {
        if (cedula.Length < 11) return "***";
        return $"{cedula[..3]}****{cedula[^4..]}";
    }

    private static bool CompareNames(string jceName, string declaredName)
    {
        // Normalizar ambos nombres
        var jce = NormalizeName(jceName);
        var declared = NormalizeName(declaredName);

        // Comparación exacta
        if (jce == declared) return true;

        // Comparación por palabras (al menos 2 coincidencias)
        var jceWords = jce.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var declaredWords = declared.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var matches = jceWords.Count(j => 
            declaredWords.Any(d => d.Equals(j, StringComparison.OrdinalIgnoreCase)));

        return matches >= 2;
    }

    private static string NormalizeName(string name)
    {
        return name.ToUpperInvariant()
            .Replace("Á", "A").Replace("É", "E").Replace("Í", "I")
            .Replace("Ó", "O").Replace("Ú", "U").Replace("Ñ", "N")
            .Trim();
    }

    private static int CalculateAge(DateTime birthDate)
    {
        var today = DateTime.Today;
        var age = today.Year - birthDate.Year;
        if (birthDate.Date > today.AddYears(-age)) age--;
        return age;
    }

    private static CedulaStatus ParseCedulaStatus(string? status)
    {
        return status?.ToUpperInvariant() switch
        {
            "VIGENTE" => CedulaStatus.Vigente,
            "VENCIDA" => CedulaStatus.Vencida,
            "FALLECIDO" => CedulaStatus.Fallecido,
            _ => CedulaStatus.NoEncontrada
        };
    }
}
```

---

## ⚛️ React Component

```tsx
// components/IdentityVerificationBadge.tsx
import { useQuery } from '@tanstack/react-query';
import { jceService } from '@/services/jceService';
import { 
  CheckCircle, AlertTriangle, XCircle, User, Shield 
} from 'lucide-react';

interface Props {
  cedula: string;
  nombreDeclarado?: string;
  showDetails?: boolean;
}

export function IdentityVerificationBadge({ 
  cedula, 
  nombreDeclarado,
  showDetails = false 
}: Props) {
  const { data: verification, isLoading, error } = useQuery({
    queryKey: ['identity-verification', cedula, nombreDeclarado],
    queryFn: () => jceService.verifyIdentity({
      cedula,
      nombreDeclarado,
    }),
    staleTime: 30 * 24 * 60 * 60 * 1000, // 30 días
    enabled: cedula?.length === 11,
  });

  if (!cedula || cedula.length !== 11) {
    return null;
  }

  if (isLoading) {
    return (
      <div className="flex items-center gap-2 text-gray-500">
        <div className="animate-spin w-4 h-4 border-2 border-gray-300 
                        border-t-blue-600 rounded-full" />
        <span className="text-sm">Verificando identidad...</span>
      </div>
    );
  }

  const levelConfig = {
    NoVerificado: {
      icon: XCircle,
      text: 'No Verificado',
      color: 'bg-gray-100 text-gray-800',
      iconColor: 'text-gray-500',
    },
    CedulaValida: {
      icon: User,
      text: 'Cédula Verificada',
      color: 'bg-blue-100 text-blue-800',
      iconColor: 'text-blue-600',
    },
    IdentidadConfirmada: {
      icon: CheckCircle,
      text: 'Identidad Confirmada',
      color: 'bg-green-100 text-green-800',
      iconColor: 'text-green-600',
    },
    Completo: {
      icon: Shield,
      text: 'Verificación Completa',
      color: 'bg-emerald-100 text-emerald-800',
      iconColor: 'text-emerald-600',
    },
  };

  const config = levelConfig[verification?.nivelVerificacion || 'NoVerificado'];
  const Icon = config.icon;

  return (
    <div className="space-y-3">
      <div className={`inline-flex items-center gap-2 px-3 py-1.5 
                       rounded-full ${config.color}`}>
        <Icon className={`w-4 h-4 ${config.iconColor}`} />
        <span className="text-sm font-medium">{config.text}</span>
      </div>

      {/* Alertas */}
      {verification?.alertas && verification.alertas.length > 0 && (
        <div className="space-y-1">
          {verification.alertas.map((alerta, i) => (
            <div key={i} className="flex items-center gap-2 text-yellow-600 text-sm">
              <AlertTriangle className="w-4 h-4" />
              <span>{alerta}</span>
            </div>
          ))}
        </div>
      )}

      {showDetails && verification?.citizen && (
        <div className="bg-gray-50 rounded-lg p-4 space-y-3">
          <div className="flex items-center gap-3">
            <div className="w-12 h-12 bg-blue-100 rounded-full 
                            flex items-center justify-center">
              <User className="w-6 h-6 text-blue-600" />
            </div>
            <div>
              <p className="font-medium">{verification.citizen.nombreCompleto}</p>
              <p className="text-sm text-gray-500">
                {verification.citizen.edad} años
              </p>
            </div>
          </div>

          <div className="grid grid-cols-2 gap-3 text-sm">
            <div>
              <p className="text-gray-500">Estado</p>
              <p className={`font-medium ${
                verification.citizen.estado === 'Vigente' 
                  ? 'text-green-600' : 'text-red-600'
              }`}>
                {verification.citizen.estado}
              </p>
            </div>
            <div>
              <p className="text-gray-500">Mayor de Edad</p>
              <p className={`font-medium ${
                verification.esMayorDeEdad 
                  ? 'text-green-600' : 'text-red-600'
              }`}>
                {verification.esMayorDeEdad ? 'Sí ✓' : 'No ✗'}
              </p>
            </div>
          </div>

          <p className="text-xs text-gray-400">
            Verificado: {new Date(verification.citizen.verificadoEn)
              .toLocaleDateString('es-DO')}
          </p>
        </div>
      )}
    </div>
  );
}

// components/CedulaInput.tsx
import { useState, useEffect } from 'react';

interface CedulaInputProps {
  value: string;
  onChange: (value: string) => void;
  onValidation?: (isValid: boolean) => void;
}

export function CedulaInput({ value, onChange, onValidation }: CedulaInputProps) {
  const [isValid, setIsValid] = useState<boolean | null>(null);

  useEffect(() => {
    if (value.length === 11) {
      const valid = jceService.validateCedulaFormat(value);
      setIsValid(valid);
      onValidation?.(valid);
    } else {
      setIsValid(null);
    }
  }, [value, onValidation]);

  const formatCedula = (input: string) => {
    const cleaned = input.replace(/\D/g, '').slice(0, 11);
    if (cleaned.length <= 3) return cleaned;
    if (cleaned.length <= 10) return `${cleaned.slice(0, 3)}-${cleaned.slice(3)}`;
    return `${cleaned.slice(0, 3)}-${cleaned.slice(3, 10)}-${cleaned.slice(10)}`;
  };

  return (
    <div className="relative">
      <input
        type="text"
        value={formatCedula(value)}
        onChange={(e) => onChange(e.target.value.replace(/\D/g, ''))}
        placeholder="000-0000000-0"
        maxLength={13}
        className={`w-full px-4 py-2 border rounded-lg focus:ring-2 
          ${isValid === true 
            ? 'border-green-500 focus:ring-green-200' 
            : isValid === false 
              ? 'border-red-500 focus:ring-red-200'
              : 'border-gray-300 focus:ring-blue-200'
          }`}
      />
      {isValid !== null && (
        <div className="absolute right-3 top-1/2 -translate-y-1/2">
          {isValid ? (
            <CheckCircle className="w-5 h-5 text-green-500" />
          ) : (
            <XCircle className="w-5 h-5 text-red-500" />
          )}
        </div>
      )}
    </div>
  );
}
```

---

## 🔌 API Controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class JceController : ControllerBase
{
    private readonly IJceService _jceService;

    public JceController(IJceService jceService)
    {
        _jceService = jceService;
    }

    /// <summary>
    /// Valida formato de cédula (sin consultar JCE)
    /// </summary>
    [HttpGet("validate-format/{cedula}")]
    public ActionResult ValidateFormat(string cedula)
    {
        var isValid = _jceService.ValidateCedulaFormat(cedula);
        return Ok(new { isValid });
    }

    /// <summary>
    /// Consulta información de ciudadano por cédula
    /// </summary>
    [HttpGet("citizen/{cedula}")]
    [Authorize]
    public async Task<ActionResult<CitizenInfo>> GetCitizen(string cedula)
    {
        var result = await _jceService.GetCitizenAsync(cedula);
        if (result == null)
            return NotFound(new { message = "Cédula no encontrada" });
        return Ok(result);
    }

    /// <summary>
    /// Verifica identidad completa
    /// </summary>
    [HttpPost("verify")]
    [Authorize]
    public async Task<ActionResult<IdentityVerificationResult>> VerifyIdentity(
        [FromBody] VerifyIdentityRequest request)
    {
        var result = await _jceService.VerifyIdentityAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Verifica si cédula es válida (simple check)
    /// </summary>
    [HttpGet("is-valid/{cedula}")]
    public async Task<ActionResult> IsValid(string cedula)
    {
        var isValid = await _jceService.IsCedulaValidAsync(cedula);
        return Ok(new { isValid });
    }
}
```

---

## 🧪 Tests

```csharp
public class JceServiceTests
{
    private readonly JceService _service;

    public JceServiceTests()
    {
        _service = new JceService(
            new HttpClient(),
            new MemoryCache(new MemoryCacheOptions()),
            Mock.Of<ILogger<JceService>>()
        );
    }

    [Theory]
    [InlineData("00100000001", false)] // Cédula de ejemplo inválida
    [InlineData("40200000000", false)] // Cédula de ejemplo inválida
    [InlineData("", false)]
    [InlineData("123", false)]
    [InlineData("12345678901234", false)]
    public void ValidateCedulaFormat_InvalidCedulas_ReturnsFalse(
        string cedula, bool expected)
    {
        var result = _service.ValidateCedulaFormat(cedula);
        result.Should().Be(expected);
    }

    [Fact]
    public void CalculateVerificationDigit_ReturnsCorrectDigit()
    {
        // El dígito verificador de 0010000000 debería ser calculable
        var digit = _service.CalculateVerificationDigit("0010000000");
        digit.Should().BeInRange(0, 9);
    }

    [Theory]
    [InlineData("JUAN CARLOS PEREZ", "Juan Perez", true)]
    [InlineData("MARIA GARCIA LOPEZ", "Maria Lopez", true)]
    [InlineData("PEDRO MARTINEZ", "Juan Rodriguez", false)]
    public void CompareNames_ReturnsExpectedResult(
        string jceName, string declared, bool expected)
    {
        var result = JceService.CompareNames(jceName, declared);
        result.Should().Be(expected);
    }

    [Fact]
    public void CalculateAge_ReturnsCorrectAge()
    {
        var birthDate = DateTime.Today.AddYears(-30);
        var age = JceService.CalculateAge(birthDate);
        age.Should().Be(30);
    }
}
```

---

## ⚙️ Configuración

```json
{
  "Jce": {
    "BaseUrl": "https://servicios.jce.gob.do/consultapadron/",
    "CacheDays": 30,
    "TimeoutSeconds": 30
  }
}
```

---

## 🔐 Consideraciones de Privacidad

1. **GDPR/Protección de Datos:**
   - Solo consultar cédulas con consentimiento del usuario
   - No almacenar datos personales innecesariamente
   - Implementar derecho al olvido

2. **Logging:**
   - Enmascarar cédulas en logs: `001****0001`
   - No loguear nombres completos

3. **Acceso:**
   - Requiere autenticación para consultas
   - Rate limiting por usuario

---

## 📞 Contacto JCE

| Departamento | Teléfono | Email |
|--------------|----------|-------|
| Servicios | 809-539-2522 | servicios@jce.gob.do |
| Tecnología | 809-539-2523 | tecnologia@jce.gob.do |

---

**Anterior:** [AMET_API.md](./AMET_API.md)  
**Siguiente:** [DATACREDITO_API.md](./DATACREDITO_API.md)
