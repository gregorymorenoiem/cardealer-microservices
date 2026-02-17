using Asp.Versioning;

namespace CarDealer.Shared.ApiVersioning.Attributes;

/// <summary>
/// Atributo para marcar controladores o endpoints como versión 1.0
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public class ApiV1Attribute : ApiVersionAttribute
{
    public ApiV1Attribute() : base(new ApiVersion(1, 0)) { }
}

/// <summary>
/// Atributo para marcar controladores o endpoints como versión 2.0
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public class ApiV2Attribute : ApiVersionAttribute
{
    public ApiV2Attribute() : base(new ApiVersion(2, 0)) { }
}

/// <summary>
/// Atributo para marcar controladores o endpoints como versión 3.0
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public class ApiV3Attribute : ApiVersionAttribute
{
    public ApiV3Attribute() : base(new ApiVersion(3, 0)) { }
}

/// <summary>
/// Atributo para marcar una versión como deprecada con fecha límite
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class ApiVersionDeprecatedAttribute : Attribute
{
    /// <summary>
    /// Fecha en que la versión será removida
    /// </summary>
    public DateTime? SunsetDate { get; }

    /// <summary>
    /// Mensaje personalizado de deprecación
    /// </summary>
    public string? Message { get; }

    /// <summary>
    /// Versión recomendada como reemplazo
    /// </summary>
    public string? RecommendedVersion { get; set; }

    public ApiVersionDeprecatedAttribute(string? message = null)
    {
        Message = message;
    }

    public ApiVersionDeprecatedAttribute(int year, int month, int day, string? message = null)
    {
        SunsetDate = new DateTime(year, month, day);
        Message = message;
    }
}

/// <summary>
/// Atributo para endpoints que soportan múltiples versiones
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class ApiVersionNeutralAttribute : Attribute
{
}
