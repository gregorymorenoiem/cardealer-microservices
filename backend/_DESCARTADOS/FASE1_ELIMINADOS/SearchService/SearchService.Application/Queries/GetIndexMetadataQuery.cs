using MediatR;
using SearchService.Domain.Entities;

namespace SearchService.Application.Queries;

/// <summary>
/// Query para obtener metadatos de un índice
/// </summary>
public class GetIndexMetadataQuery : IRequest<IndexMetadata?>
{
    public string IndexName { get; set; } = string.Empty;

    public GetIndexMetadataQuery(string indexName)
    {
        IndexName = indexName;
    }
}
