namespace SearchService.Domain.Enums;

/// <summary>
/// Tipos de búsqueda soportados por el servicio
/// </summary>
public enum SearchType
{
    /// <summary>
    /// Búsqueda de texto completo (analiza y tokeniza)
    /// </summary>
    FullText = 0,

    /// <summary>
    /// Búsqueda fuzzy (tolera errores de escritura)
    /// </summary>
    Fuzzy = 1,

    /// <summary>
    /// Búsqueda exacta (match completo)
    /// </summary>
    Exact = 2,

    /// <summary>
    /// Búsqueda con wildcards (* y ?)
    /// </summary>
    Wildcard = 3,

    /// <summary>
    /// Búsqueda por prefijo (autocompletado)
    /// </summary>
    Prefix = 4,

    /// <summary>
    /// Búsqueda por rango (fechas, números)
    /// </summary>
    Range = 5
}
