using MediatR;

namespace SearchService.Application.Commands;

/// <summary>
/// Comando para eliminar un índice
/// </summary>
public class DeleteIndexCommand : IRequest<bool>
{
    public string IndexName { get; set; } = string.Empty;

    public DeleteIndexCommand(string indexName)
    {
        IndexName = indexName;
    }
}
