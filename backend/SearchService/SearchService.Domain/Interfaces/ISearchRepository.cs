using SearchService.Domain.Entities;
using SearchService.Domain.ValueObjects;

namespace SearchService.Domain.Interfaces;

/// <summary>
/// Repositorio para operaciones de búsqueda en Elasticsearch
/// </summary>
public interface ISearchRepository
{
    /// <summary>
    /// Ejecuta una búsqueda en Elasticsearch
    /// </summary>
    Task<SearchResult> SearchAsync(SearchQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene un documento por su ID
    /// </summary>
    Task<SearchDocument?> GetDocumentAsync(string indexName, string documentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Indexa un documento en Elasticsearch
    /// </summary>
    Task<string> IndexDocumentAsync(string indexName, string documentId, object document, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza un documento existente
    /// </summary>
    Task<bool> UpdateDocumentAsync(string indexName, string documentId, object document, CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina un documento del índice
    /// </summary>
    Task<bool> DeleteDocumentAsync(string indexName, string documentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Indexa múltiples documentos en batch
    /// </summary>
    Task<(int Successful, int Failed)> BulkIndexAsync(string indexName, IEnumerable<(string Id, object Document)> documents, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si un documento existe
    /// </summary>
    Task<bool> DocumentExistsAsync(string indexName, string documentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cuenta documentos que coinciden con un filtro
    /// </summary>
    Task<long> CountDocumentsAsync(string indexName, Dictionary<string, object>? filters = null, CancellationToken cancellationToken = default);
}
