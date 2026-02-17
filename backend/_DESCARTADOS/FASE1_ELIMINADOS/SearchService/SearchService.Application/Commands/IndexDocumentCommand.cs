using MediatR;

namespace SearchService.Application.Commands;

/// <summary>
/// Comando para indexar un documento en Elasticsearch
/// </summary>
public class IndexDocumentCommand : IRequest<string>
{
    public string IndexName { get; set; } = string.Empty;
    public string DocumentId { get; set; } = string.Empty;
    public object Document { get; set; } = new();

    public IndexDocumentCommand(string indexName, string documentId, object document)
    {
        IndexName = indexName;
        DocumentId = documentId;
        Document = document;
    }
}
