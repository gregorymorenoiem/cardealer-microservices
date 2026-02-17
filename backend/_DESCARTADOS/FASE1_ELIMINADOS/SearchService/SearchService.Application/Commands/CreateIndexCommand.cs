using MediatR;

namespace SearchService.Application.Commands;

/// <summary>
/// Comando para crear un nuevo índice
/// </summary>
public class CreateIndexCommand : IRequest<bool>
{
    public string IndexName { get; set; } = string.Empty;
    public Dictionary<string, object>? Mappings { get; set; }
    public Dictionary<string, object>? Settings { get; set; }

    public CreateIndexCommand(string indexName, Dictionary<string, object>? mappings = null, Dictionary<string, object>? settings = null)
    {
        IndexName = indexName;
        Mappings = mappings;
        Settings = settings;
    }
}
