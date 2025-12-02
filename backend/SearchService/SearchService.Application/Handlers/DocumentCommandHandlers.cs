using MediatR;
using SearchService.Application.Commands;
using SearchService.Domain.Interfaces;

namespace SearchService.Application.Handlers;

/// <summary>
/// Handlers para comandos de documentos
/// </summary>
public class DocumentCommandHandlers :
    IRequestHandler<IndexDocumentCommand, string>,
    IRequestHandler<UpdateDocumentCommand, bool>,
    IRequestHandler<DeleteDocumentCommand, bool>,
    IRequestHandler<BulkIndexCommand, (int Successful, int Failed)>
{
    private readonly ISearchRepository _searchRepository;

    public DocumentCommandHandlers(ISearchRepository searchRepository)
    {
        _searchRepository = searchRepository;
    }

    public async Task<string> Handle(IndexDocumentCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.IndexName) || string.IsNullOrWhiteSpace(request.DocumentId))
        {
            throw new ArgumentException("IndexName and DocumentId are required");
        }

        return await _searchRepository.IndexDocumentAsync(request.IndexName, request.DocumentId, request.Document, cancellationToken);
    }

    public async Task<bool> Handle(UpdateDocumentCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.IndexName) || string.IsNullOrWhiteSpace(request.DocumentId))
        {
            throw new ArgumentException("IndexName and DocumentId are required");
        }

        return await _searchRepository.UpdateDocumentAsync(request.IndexName, request.DocumentId, request.Document, cancellationToken);
    }

    public async Task<bool> Handle(DeleteDocumentCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.IndexName) || string.IsNullOrWhiteSpace(request.DocumentId))
        {
            throw new ArgumentException("IndexName and DocumentId are required");
        }

        return await _searchRepository.DeleteDocumentAsync(request.IndexName, request.DocumentId, cancellationToken);
    }

    public async Task<(int Successful, int Failed)> Handle(BulkIndexCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.IndexName))
        {
            throw new ArgumentException("IndexName is required");
        }

        if (!request.Documents.Any())
        {
            return (0, 0);
        }

        return await _searchRepository.BulkIndexAsync(request.IndexName, request.Documents, cancellationToken);
    }
}
