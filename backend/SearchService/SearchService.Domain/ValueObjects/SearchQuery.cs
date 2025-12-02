using SearchService.Domain.Enums;

namespace SearchService.Domain.ValueObjects;

/// <summary>
/// Representa una consulta de búsqueda con sus parámetros
/// </summary>
public class SearchQuery
{
    /// <summary>
    /// Texto de búsqueda
    /// </summary>
    public string QueryText { get; set; } = string.Empty;

    /// <summary>
    /// Nombre del índice donde buscar
    /// </summary>
    public string IndexName { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de búsqueda a realizar
    /// </summary>
    public SearchType SearchType { get; set; } = SearchType.FullText;

    /// <summary>
    /// Campos donde buscar (si está vacío, busca en todos)
    /// </summary>
    public List<string> Fields { get; set; } = new();

    /// <summary>
    /// Filtros adicionales (clave-valor)
    /// </summary>
    public Dictionary<string, object> Filters { get; set; } = new();

    /// <summary>
    /// Página a recuperar (1-based)
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Número de resultados por página
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Campo por el cual ordenar
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Orden de clasificación
    /// </summary>
    public SortOrder SortOrder { get; set; } = SortOrder.Descending;

    /// <summary>
    /// Habilitar highlighting de resultados
    /// </summary>
    public bool EnableHighlighting { get; set; } = true;

    /// <summary>
    /// Fuzziness para búsquedas fuzzy (AUTO, 0, 1, 2)
    /// </summary>
    public string Fuzziness { get; set; } = "AUTO";

    /// <summary>
    /// Boost scores de campos específicos
    /// </summary>
    public Dictionary<string, double> FieldBoosts { get; set; } = new();

    /// <summary>
    /// Score mínimo requerido para incluir resultados
    /// </summary>
    public double? MinScore { get; set; }

    /// <summary>
    /// Timeout de la consulta en milisegundos
    /// </summary>
    public int TimeoutMs { get; set; } = 5000;

    /// <summary>
    /// Valida que la query sea válida
    /// </summary>
    public bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(QueryText))
            return false;

        if (string.IsNullOrWhiteSpace(IndexName))
            return false;

        if (Page < 1)
            return false;

        if (PageSize < 1 || PageSize > 100)
            return false;

        return true;
    }

    /// <summary>
    /// Calcula el offset para paginación
    /// </summary>
    public int GetOffset() => (Page - 1) * PageSize;

    /// <summary>
    /// Obtiene el número máximo de resultados a recuperar
    /// </summary>
    public int GetLimit() => PageSize;
}
