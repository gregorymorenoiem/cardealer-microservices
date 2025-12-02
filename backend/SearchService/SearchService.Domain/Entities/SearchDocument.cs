using SearchService.Domain.Enums;

namespace SearchService.Domain.Entities;

/// <summary>
/// Representa un documento indexado en Elasticsearch
/// </summary>
public class SearchDocument
{
    /// <summary>
    /// ID único del documento en Elasticsearch
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Nombre del índice donde está almacenado
    /// </summary>
    public string IndexName { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de documento (vehicle, user, contact, etc.)
    /// </summary>
    public string DocumentType { get; set; } = string.Empty;

    /// <summary>
    /// Contenido del documento en formato JSON
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de creación del documento
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Última actualización del documento
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Metadatos adicionales del documento
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Score de relevancia (calculado por Elasticsearch en búsquedas)
    /// </summary>
    public double? Score { get; set; }

    /// <summary>
    /// Fragmentos destacados (highlights) del documento
    /// </summary>
    public Dictionary<string, List<string>> Highlights { get; set; } = new();

    /// <summary>
    /// Indica si el documento está activo
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Tags asociados al documento para filtrado
    /// </summary>
    public List<string> Tags { get; set; } = new();
}
