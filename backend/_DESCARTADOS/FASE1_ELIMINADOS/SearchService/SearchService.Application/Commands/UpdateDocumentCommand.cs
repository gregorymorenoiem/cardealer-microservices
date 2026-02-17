using MediatR;

namespace SearchService.Application.Commands;

/// <summary>
/// Comando para actualizar un documento existente
/// </summary>
public class UpdateDocumentCommand : IRequest<bool>
{
    public string IndexName { get; set; } = string.Empty;
    public string DocumentId { get; set; } = string.Empty;
    public object Document { get; set; } = new();

    public UpdateDocumentCommand(string indexName, string documentId, object document)
    {
        IndexName = indexName;
        DocumentId = documentId;
        Document = document;
    }
}
