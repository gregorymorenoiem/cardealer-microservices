using MediatR;
using SearchService.Application.Queries;
using SearchService.Domain.Entities;
using SearchService.Domain.Interfaces;
using SearchService.Domain.ValueObjects;

namespace SearchService.Application.Handlers;

/// <summary>
/// Handlers para queries de búsqueda
/// </summary>
public class SearchQueryHandlers :
    IRequestHandler<ExecuteSearchQuery, SearchResult>,
    IRequestHandler<GetDocumentQuery, SearchDocument?>,
    IRequestHandler<GetIndexMetadataQuery, IndexMetadata?>,
    IRequestHandler<ListIndicesQuery, List<string>>
{
    private readonly ISearchRepository _searchRepository;
    private readonly IIndexManager _indexManager;

    public SearchQueryHandlers(ISearchRepository searchRepository, IIndexManager indexManager)
    {
        _searchRepository = searchRepository;
        _indexManager = indexManager;
    }

    public async Task<SearchResult> Handle(ExecuteSearchQuery request, CancellationToken cancellationToken)
    {
        if (!request.SearchQuery.IsValid())
        {
            throw new ArgumentException("Invalid search query parameters");
        }

        return await _searchRepository.SearchAsync(request.SearchQuery, cancellationToken);
    }

    public async Task<SearchDocument?> Handle(GetDocumentQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.IndexName) || string.IsNullOrWhiteSpace(request.DocumentId))
        {
            throw new ArgumentException("IndexName and DocumentId are required");
        }

        return await _searchRepository.GetDocumentAsync(request.IndexName, request.DocumentId, cancellationToken);
    }

    public async Task<IndexMetadata?> Handle(GetIndexMetadataQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.IndexName))
        {
            throw new ArgumentException("IndexName is required");
        }

        return await _indexManager.GetIndexMetadataAsync(request.IndexName, cancellationToken);
    }

    public async Task<List<string>> Handle(ListIndicesQuery request, CancellationToken cancellationToken)
    {
        return await _indexManager.ListIndicesAsync(cancellationToken);
    }
}
