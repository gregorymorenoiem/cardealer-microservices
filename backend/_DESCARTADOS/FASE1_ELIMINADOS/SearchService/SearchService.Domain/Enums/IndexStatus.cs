namespace SearchService.Domain.Enums;

/// <summary>
/// Estado de un índice de Elasticsearch
/// </summary>
public enum IndexStatus
{
    /// <summary>
    /// Índice activo y disponible
    /// </summary>
    Active = 0,

    /// <summary>
    /// Índice deshabilitado temporalmente
    /// </summary>
    Disabled = 1,

    /// <summary>
    /// Índice en proceso de reindexación
    /// </summary>
    Reindexing = 2,

    /// <summary>
    /// Índice con errores
    /// </summary>
    Error = 3
}
