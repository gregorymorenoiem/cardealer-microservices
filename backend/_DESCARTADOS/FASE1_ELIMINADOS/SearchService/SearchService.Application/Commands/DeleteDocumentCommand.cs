using MediatR;

namespace SearchService.Application.Commands;

/// <summary>
/// Comando para eliminar un documento del índice
/// </summary>
public class DeleteDocumentCommand : IRequest<bool>
{
    public string IndexName { get; set; } = string.Empty;
    public string DocumentId { get; set; } = string.Empty;

    public DeleteDocumentCommand(string indexName, string documentId)
    {
        IndexName = indexName;
        DocumentId = documentId;
    }
}
