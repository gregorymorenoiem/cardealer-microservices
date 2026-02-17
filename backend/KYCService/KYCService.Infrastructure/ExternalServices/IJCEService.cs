namespace KYCService.Infrastructure.ExternalServices;

/// <summary>
/// Interface para integración con la Junta Central Electoral (JCE) de República Dominicana
/// </summary>
public interface IJCEService
{
    /// <summary>
    /// Valida una cédula contra los registros de la JCE
    /// </summary>
    Task<JCEValidationResult> ValidateCedulaAsync(string cedula, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene los datos públicos de un ciudadano por su cédula
    /// </summary>
    Task<JCECitizenData?> GetCitizenDataAsync(string cedula, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si un ciudadano está en el padrón electoral
    /// </summary>
    Task<bool> IsInElectoralPadronAsync(string cedula, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si el servicio de JCE está disponible
    /// </summary>
    Task<bool> IsServiceAvailableAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Resultado de validación de cédula contra JCE
/// </summary>
public class JCEValidationResult
{
    /// <summary>
    /// Indica si la cédula existe en los registros de la JCE
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Indica si la cédula está activa (no cancelada)
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Indica si el titular está fallecido
    /// </summary>
    public bool IsDeceased { get; set; }

    /// <summary>
    /// Datos del ciudadano (si la validación fue exitosa)
    /// </summary>
    public JCECitizenData? CitizenData { get; set; }

    /// <summary>
    /// Mensaje de error (si la validación falló)
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Código de respuesta del servicio JCE
    /// </summary>
    public string? ResponseCode { get; set; }

    /// <summary>
    /// Timestamp de la validación
    /// </summary>
    public DateTime ValidatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Tiempo de respuesta del servicio en milisegundos
    /// </summary>
    public long ResponseTimeMs { get; set; }
}

/// <summary>
/// Datos de un ciudadano según registros de la JCE
/// </summary>
public class JCECitizenData
{
    /// <summary>
    /// Número de cédula (formato: XXX-XXXXXXX-X)
    /// </summary>
    public string Cedula { get; set; } = string.Empty;

    /// <summary>
    /// Primer nombre
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Segundo nombre (opcional)
    /// </summary>
    public string? MiddleName { get; set; }

    /// <summary>
    /// Primer apellido
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Segundo apellido (opcional)
    /// </summary>
    public string? SecondLastName { get; set; }

    /// <summary>
    /// Nombre completo
    /// </summary>
    public string FullName => string.Join(" ", new[] { FirstName, MiddleName, LastName, SecondLastName }
        .Where(s => !string.IsNullOrWhiteSpace(s)));

    /// <summary>
    /// Fecha de nacimiento
    /// </summary>
    public DateTime DateOfBirth { get; set; }

    /// <summary>
    /// Lugar de nacimiento (municipio)
    /// </summary>
    public string? BirthPlace { get; set; }

    /// <summary>
    /// Sexo (M/F)
    /// </summary>
    public string Gender { get; set; } = string.Empty;

    /// <summary>
    /// Nacionalidad
    /// </summary>
    public string Nationality { get; set; } = "DOMINICANA";

    /// <summary>
    /// Estado civil
    /// </summary>
    public string? MaritalStatus { get; set; }

    /// <summary>
    /// Dirección registrada
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Municipio de residencia
    /// </summary>
    public string? Municipality { get; set; }

    /// <summary>
    /// Provincia de residencia
    /// </summary>
    public string? Province { get; set; }

    /// <summary>
    /// URL de la foto (si disponible)
    /// </summary>
    public string? PhotoUrl { get; set; }

    /// <summary>
    /// Fecha de emisión de la cédula
    /// </summary>
    public DateTime? IssueDate { get; set; }

    /// <summary>
    /// Fecha de expiración de la cédula
    /// </summary>
    public DateTime? ExpiryDate { get; set; }

    /// <summary>
    /// Estado de la cédula
    /// </summary>
    public JCECedulaStatus Status { get; set; } = JCECedulaStatus.Active;
}

/// <summary>
/// Estados posibles de una cédula
/// </summary>
public enum JCECedulaStatus
{
    /// <summary>
    /// Cédula activa y vigente
    /// </summary>
    Active = 1,

    /// <summary>
    /// Cédula expirada (requiere renovación)
    /// </summary>
    Expired = 2,

    /// <summary>
    /// Cédula cancelada (reportada como perdida/robada)
    /// </summary>
    Cancelled = 3,

    /// <summary>
    /// Titular fallecido
    /// </summary>
    Deceased = 4,

    /// <summary>
    /// Cédula suspendida por proceso judicial
    /// </summary>
    Suspended = 5,

    /// <summary>
    /// Estado desconocido
    /// </summary>
    Unknown = 0
}
