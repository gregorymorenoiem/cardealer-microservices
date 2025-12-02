using MediatR;

namespace SearchService.Application.Commands;

/// <summary>
/// Comando para indexar múltiples documentos en batch
/// </summary>
public class BulkIndexCommand : IRequest<(int Successful, int Failed)>
{
    public string IndexName { get; set; } = string.Empty;
    public List<(string Id, object Document)> Documents { get; set; } = new();

    public BulkIndexCommand(string indexName, List<(string Id, object Document)> documents)
    {
        IndexName = indexName;
        Documents = documents;
    }
}
