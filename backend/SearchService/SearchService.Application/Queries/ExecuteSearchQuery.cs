using MediatR;
using SearchService.Domain.ValueObjects;

namespace SearchService.Application.Queries;

/// <summary>
/// Query para ejecutar una búsqueda en Elasticsearch
/// </summary>
public class ExecuteSearchQuery : IRequest<SearchResult>
{
    public SearchQuery SearchQuery { get; set; } = new();

    public ExecuteSearchQuery(SearchQuery searchQuery)
    {
        SearchQuery = searchQuery;
    }
}
