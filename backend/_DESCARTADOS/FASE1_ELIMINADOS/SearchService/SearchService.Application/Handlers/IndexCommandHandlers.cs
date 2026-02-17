using MediatR;
using SearchService.Application.Commands;
using SearchService.Domain.Interfaces;

namespace SearchService.Application.Handlers;

/// <summary>
/// Handlers para comandos de gestión de índices
/// </summary>
public class IndexCommandHandlers :
    IRequestHandler<CreateIndexCommand, bool>,
    IRequestHandler<DeleteIndexCommand, bool>
{
    private readonly IIndexManager _indexManager;

    public IndexCommandHandlers(IIndexManager indexManager)
    {
        _indexManager = indexManager;
    }

    public async Task<bool> Handle(CreateIndexCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.IndexName))
        {
            throw new ArgumentException("IndexName is required");
        }

        // Verificar si el índice ya existe
        var exists = await _indexManager.IndexExistsAsync(request.IndexName, cancellationToken);
        if (exists)
        {
            throw new InvalidOperationException($"Index '{request.IndexName}' already exists");
        }

        return await _indexManager.CreateIndexAsync(request.IndexName, request.Mappings, request.Settings, cancellationToken);
    }

    public async Task<bool> Handle(DeleteIndexCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.IndexName))
        {
            throw new ArgumentException("IndexName is required");
        }

        // Verificar si el índice existe antes de eliminarlo
        var exists = await _indexManager.IndexExistsAsync(request.IndexName, cancellationToken);
        if (!exists)
        {
            throw new InvalidOperationException($"Index '{request.IndexName}' does not exist");
        }

        return await _indexManager.DeleteIndexAsync(request.IndexName, cancellationToken);
    }
}
