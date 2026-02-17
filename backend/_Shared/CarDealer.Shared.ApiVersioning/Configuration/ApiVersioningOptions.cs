namespace CarDealer.Shared.ApiVersioning.Configuration;

/// <summary>
/// Opciones de configuración para API versioning
/// </summary>
public class ApiVersioningOptions
{
    public const string SectionName = "ApiVersioning";

    /// <summary>
    /// Habilitar versionamiento de API
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Versión por defecto (major.minor)
    /// </summary>
    public string DefaultVersion { get; set; } = "1.0";

    /// <summary>
    /// Asumir versión por defecto cuando no se especifica
    /// </summary>
    public bool AssumeDefaultVersionWhenUnspecified { get; set; } = true;

    /// <summary>
    /// Reportar las versiones de API disponibles en los headers
    /// </summary>
    public bool ReportApiVersions { get; set; } = true;

    /// <summary>
    /// Configuración de lectura de versión
    /// </summary>
    public VersionReaderOptions VersionReader { get; set; } = new();

    /// <summary>
    /// Configuración de Swagger/OpenAPI
    /// </summary>
    public SwaggerVersionOptions Swagger { get; set; } = new();
}

/// <summary>
/// Opciones de lectura de versión de API
/// </summary>
public class VersionReaderOptions
{
    /// <summary>
    /// Leer versión desde query string (?api-version=1.0)
    /// </summary>
    public bool ReadFromQueryString { get; set; } = true;

    /// <summary>
    /// Nombre del parámetro de query string
    /// </summary>
    public string QueryStringParameter { get; set; } = "api-version";

    /// <summary>
    /// Leer versión desde header (X-Api-Version: 1.0)
    /// </summary>
    public bool ReadFromHeader { get; set; } = true;

    /// <summary>
    /// Nombre del header
    /// </summary>
    public string HeaderName { get; set; } = "X-Api-Version";

    /// <summary>
    /// Leer versión desde URL (/api/v1.0/resource)
    /// </summary>
    public bool ReadFromUrl { get; set; } = true;

    /// <summary>
    /// Leer versión desde media type (Accept: application/json;v=1.0)
    /// </summary>
    public bool ReadFromMediaType { get; set; } = false;

    /// <summary>
    /// Nombre del parámetro en media type
    /// </summary>
    public string MediaTypeParameter { get; set; } = "v";
}

/// <summary>
/// Opciones de Swagger para versioning
/// </summary>
public class SwaggerVersionOptions
{
    /// <summary>
    /// Habilitar múltiples documentos de Swagger por versión
    /// </summary>
    public bool EnableMultipleDocuments { get; set; } = true;

    /// <summary>
    /// Título del API
    /// </summary>
    public string Title { get; set; } = "CarDealer API";

    /// <summary>
    /// Descripción del API
    /// </summary>
    public string Description { get; set; } = "API de microservicios CarDealer";

    /// <summary>
    /// Información de contacto
    /// </summary>
    public ContactInfo? Contact { get; set; }

    /// <summary>
    /// Información de licencia
    /// </summary>
    public LicenseInfo? License { get; set; }
}

public class ContactInfo
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Url { get; set; }
}

public class LicenseInfo
{
    public string? Name { get; set; }
    public string? Url { get; set; }
}
