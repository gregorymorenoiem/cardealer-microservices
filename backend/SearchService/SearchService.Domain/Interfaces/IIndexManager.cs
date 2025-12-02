using SearchService.Domain.Entities;

namespace SearchService.Domain.Interfaces;

/// <summary>
/// Gestor de índices de Elasticsearch
/// </summary>
public interface IIndexManager
{
    /// <summary>
    /// Crea un nuevo índice con su configuración
    /// </summary>
    Task<bool> CreateIndexAsync(string indexName, Dictionary<string, object>? mappings = null, Dictionary<string, object>? settings = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina un índice
    /// </summary>
    Task<bool> DeleteIndexAsync(string indexName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si un índice existe
    /// </summary>
    Task<bool> IndexExistsAsync(string indexName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene metadatos de un índice
    /// </summary>
    Task<IndexMetadata?> GetIndexMetadataAsync(string indexName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista todos los índices disponibles
    /// </summary>
    Task<List<string>> ListIndicesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza la configuración de un índice
    /// </summary>
    Task<bool> UpdateIndexSettingsAsync(string indexName, Dictionary<string, object> settings, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reindexar documentos de un índice a otro
    /// </summary>
    Task<bool> ReindexAsync(string sourceIndex, string destinationIndex, CancellationToken cancellationToken = default);

    /// <summary>
    /// Crea o actualiza un alias
    /// </summary>
    Task<bool> CreateAliasAsync(string indexName, string aliasName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina un alias
    /// </summary>
    Task<bool> DeleteAliasAsync(string indexName, string aliasName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refresca un índice para hacer los cambios visibles
    /// </summary>
    Task<bool> RefreshIndexAsync(string indexName, CancellationToken cancellationToken = default);
}
