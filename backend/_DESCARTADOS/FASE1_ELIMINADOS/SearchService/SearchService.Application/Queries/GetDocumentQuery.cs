using MediatR;
using SearchService.Domain.Entities;

namespace SearchService.Application.Queries;

/// <summary>
/// Query para obtener un documento específico por ID
/// </summary>
public class GetDocumentQuery : IRequest<SearchDocument?>
{
    public string IndexName { get; set; } = string.Empty;
    public string DocumentId { get; set; } = string.Empty;

    public GetDocumentQuery(string indexName, string documentId)
    {
        IndexName = indexName;
        DocumentId = documentId;
    }
}
