using SearchService.Domain.Entities;

namespace SearchService.Domain.ValueObjects;

/// <summary>
/// Representa el resultado de una búsqueda
/// </summary>
public class SearchResult
{
    /// <summary>
    /// Total de documentos encontrados
    /// </summary>
    public long TotalCount { get; set; }

    /// <summary>
    /// Documentos en la página actual
    /// </summary>
    public List<SearchDocument> Documents { get; set; } = new();

    /// <summary>
    /// Página actual
    /// </summary>
    public int CurrentPage { get; set; }

    /// <summary>
    /// Tamaño de página
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Tiempo de ejecución de la query en milisegundos
    /// </summary>
    public long ExecutionTimeMs { get; set; }

    /// <summary>
    /// Facets o agregaciones de la búsqueda
    /// </summary>
    public Dictionary<string, Dictionary<string, long>> Facets { get; set; } = new();

    /// <summary>
    /// Sugerencias de búsqueda (did you mean?)
    /// </summary>
    public List<string> Suggestions { get; set; } = new();

    /// <summary>
    /// Indica si la búsqueda tuvo timeout
    /// </summary>
    public bool TimedOut { get; set; }

    /// <summary>
    /// Score máximo obtenido en los resultados
    /// </summary>
    public double? MaxScore { get; set; }

    /// <summary>
    /// Metadatos adicionales de la búsqueda
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Calcula el número total de páginas
    /// </summary>
    public int GetTotalPages()
    {
        if (PageSize <= 0) return 0;
        return (int)Math.Ceiling(TotalCount / (double)PageSize);
    }

    /// <summary>
    /// Verifica si hay página siguiente
    /// </summary>
    public bool HasNextPage() => CurrentPage < GetTotalPages();

    /// <summary>
    /// Verifica si hay página anterior
    /// </summary>
    public bool HasPreviousPage() => CurrentPage > 1;

    /// <summary>
    /// Verifica si hay resultados
    /// </summary>
    public bool HasResults() => Documents.Any();

    /// <summary>
    /// Obtiene un resumen de la búsqueda
    /// </summary>
    public string GetSummary()
    {
        return $"Found {TotalCount} results in {ExecutionTimeMs}ms (Page {CurrentPage}/{GetTotalPages()})";
    }
}
