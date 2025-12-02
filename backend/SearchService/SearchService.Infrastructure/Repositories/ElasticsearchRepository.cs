using Microsoft.Extensions.Logging;
using Nest;
using SearchService.Domain.Entities;
using SearchService.Domain.Enums;
using SearchService.Domain.Interfaces;
using SearchService.Domain.ValueObjects;
using SearchService.Infrastructure.Configuration;
using System.Diagnostics;

namespace SearchService.Infrastructure.Repositories;

/// <summary>
/// Implementación del repositorio de búsqueda usando Elasticsearch
/// </summary>
public class ElasticsearchRepository : ISearchRepository
{
    private readonly IElasticClient _client;
    private readonly ILogger<ElasticsearchRepository> _logger;
    private readonly ElasticsearchOptions _options;

    public ElasticsearchRepository(
        IElasticClient client,
        ILogger<ElasticsearchRepository> logger,
        ElasticsearchOptions options)
    {
        _client = client;
        _logger = logger;
        _options = options;
    }

    public async Task<SearchResult> SearchAsync(SearchQuery query, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var searchDescriptor = BuildSearchDescriptor(query);

            var response = await _client.SearchAsync<object>(searchDescriptor, cancellationToken);

            stopwatch.Stop();

            if (!response.IsValid)
            {
                _logger.LogError("Elasticsearch search error: {Error}", response.DebugInformation);
                throw new InvalidOperationException($"Search failed: {response.ServerError?.Error?.Reason ?? "Unknown error"}");
            }

            return MapToSearchResult(response, query, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing search query");
            throw;
        }
    }

    public async Task<SearchDocument?> GetDocumentAsync(string indexName, string documentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _client.GetAsync<object>(documentId, idx => idx.Index(GetFullIndexName(indexName)), cancellationToken);

            if (!response.IsValid || !response.Found)
            {
                return null;
            }

            return new SearchDocument
            {
                Id = response.Id,
                IndexName = indexName,
                Content = System.Text.Json.JsonSerializer.Serialize(response.Source),
                UpdatedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting document {DocumentId} from index {IndexName}", documentId, indexName);
            throw;
        }
    }

    public async Task<string> IndexDocumentAsync(string indexName, string documentId, object document, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _client.IndexAsync(document, idx => idx
                .Index(GetFullIndexName(indexName))
                .Id(documentId)
                .Refresh(Elasticsearch.Net.Refresh.WaitFor), cancellationToken);

            if (!response.IsValid)
            {
                _logger.LogError("Error indexing document: {Error}", response.DebugInformation);
                throw new InvalidOperationException($"Indexing failed: {response.ServerError?.Error?.Reason ?? "Unknown error"}");
            }

            _logger.LogInformation("Document {DocumentId} indexed in {IndexName}", documentId, indexName);
            return response.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error indexing document {DocumentId} in index {IndexName}", documentId, indexName);
            throw;
        }
    }

    public async Task<bool> UpdateDocumentAsync(string indexName, string documentId, object document, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _client.UpdateAsync<object, object>(documentId, u => u
                .Index(GetFullIndexName(indexName))
                .Doc(document)
                .Refresh(Elasticsearch.Net.Refresh.WaitFor), cancellationToken);

            if (!response.IsValid)
            {
                _logger.LogError("Error updating document: {Error}", response.DebugInformation);
                return false;
            }

            _logger.LogInformation("Document {DocumentId} updated in {IndexName}", documentId, indexName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating document {DocumentId} in index {IndexName}", documentId, indexName);
            return false;
        }
    }

    public async Task<bool> DeleteDocumentAsync(string indexName, string documentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _client.DeleteAsync<object>(documentId, d => d
                .Index(GetFullIndexName(indexName))
                .Refresh(Elasticsearch.Net.Refresh.WaitFor), cancellationToken);

            if (!response.IsValid)
            {
                _logger.LogError("Error deleting document: {Error}", response.DebugInformation);
                return false;
            }

            _logger.LogInformation("Document {DocumentId} deleted from {IndexName}", documentId, indexName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document {DocumentId} from index {IndexName}", documentId, indexName);
            return false;
        }
    }

    public async Task<(int Successful, int Failed)> BulkIndexAsync(string indexName, IEnumerable<(string Id, object Document)> documents, CancellationToken cancellationToken = default)
    {
        try
        {
            var bulkDescriptor = new BulkDescriptor();
            var fullIndexName = GetFullIndexName(indexName);

            foreach (var (id, document) in documents)
            {
                bulkDescriptor.Index<object>(op => op
                    .Index(fullIndexName)
                    .Id(id)
                    .Document(document));
            }

            var response = await _client.BulkAsync(bulkDescriptor, cancellationToken);

            if (!response.IsValid)
            {
                _logger.LogError("Bulk indexing error: {Error}", response.DebugInformation);
            }

            var successful = response.Items.Count(i => i.IsValid);
            var failed = response.Items.Count(i => !i.IsValid);

            _logger.LogInformation("Bulk index completed: {Successful} successful, {Failed} failed", successful, failed);

            return (successful, failed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during bulk indexing");
            throw;
        }
    }

    public async Task<bool> DocumentExistsAsync(string indexName, string documentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _client.DocumentExistsAsync<object>(documentId, d => d.Index(GetFullIndexName(indexName)), cancellationToken);
            return response.Exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking document existence");
            return false;
        }
    }

    public async Task<long> CountDocumentsAsync(string indexName, Dictionary<string, object>? filters = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _client.CountAsync<object>(c => 
            {
                c.Index(GetFullIndexName(indexName));
                if (filters != null && filters.Any())
                {
                    c.Query(q => BuildFiltersQuery(q, filters));
                }
                return c;
            }, cancellationToken);

            return response.IsValid ? response.Count : 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error counting documents");
            return 0;
        }
    }

    private string GetFullIndexName(string indexName)
    {
        return string.IsNullOrWhiteSpace(_options.IndexPrefix)
            ? indexName.ToLowerInvariant()
            : $"{_options.IndexPrefix}_{indexName}".ToLowerInvariant();
    }

    private SearchDescriptor<object> BuildSearchDescriptor(SearchQuery query)
    {
        var descriptor = new SearchDescriptor<object>()
            .Index(GetFullIndexName(query.IndexName))
            .From(query.GetOffset())
            .Size(query.GetLimit())
            .RequestConfiguration(r => r.RequestTimeout(TimeSpan.FromMilliseconds(query.TimeoutMs)));

        // Construir la query según el tipo de búsqueda
        descriptor = descriptor.Query(q => BuildQuery(q, query));

        // Aplicar sorting
        if (!string.IsNullOrWhiteSpace(query.SortBy))
        {
            descriptor = descriptor.Sort(s => s
                .Field(query.SortBy, query.SortOrder == Domain.Enums.SortOrder.Ascending
                    ? Nest.SortOrder.Ascending
                    : Nest.SortOrder.Descending));
        }
        else
        {
            descriptor = descriptor.Sort(s => s.Descending(SortSpecialField.Score));
        }

        // Aplicar highlighting
        if (query.EnableHighlighting)
        {
            descriptor = descriptor.Highlight(h => h
                .PreTags("<mark>")
                .PostTags("</mark>")
                .Fields(f => f.Field("*")));
        }

        // Aplicar min_score
        if (query.MinScore.HasValue)
        {
            descriptor = descriptor.MinScore(query.MinScore.Value);
        }

        return descriptor;
    }

    private QueryContainer BuildQuery(QueryContainerDescriptor<object> q, SearchQuery query)
    {
        QueryContainer mainQuery = query.SearchType switch
        {
            SearchType.FullText => BuildFullTextQuery(q, query),
            SearchType.Fuzzy => BuildFuzzyQuery(q, query),
            SearchType.Exact => BuildExactQuery(q, query),
            SearchType.Wildcard => BuildWildcardQuery(q, query),
            SearchType.Prefix => BuildPrefixQuery(q, query),
            _ => BuildFullTextQuery(q, query)
        };

        // Aplicar filtros adicionales
        if (query.Filters.Any())
        {
            mainQuery = q.Bool(b => b
                .Must(mainQuery)
                .Filter(f => BuildFiltersQuery(f, query.Filters)));
        }

        return mainQuery;
    }

    private QueryContainer BuildFullTextQuery(QueryContainerDescriptor<object> q, SearchQuery query)
    {
        if (query.Fields.Any())
        {
            return q.MultiMatch(mm => mm
                .Query(query.QueryText)
                .Fields(query.Fields.Select(f => new Field(f)).ToArray())
                .Fuzziness(Fuzziness.Auto));
        }

        return q.QueryString(qs => qs.Query(query.QueryText));
    }

    private QueryContainer BuildFuzzyQuery(QueryContainerDescriptor<object> q, SearchQuery query)
    {
        if (query.Fields.Any())
        {
            return q.MultiMatch(mm => mm
                .Query(query.QueryText)
                .Fields(query.Fields.Select(f => new Field(f)).ToArray())
                .Fuzziness(Fuzziness.Auto));
        }

        return q.Match(m => m
            .Field("_all")
            .Query(query.QueryText)
            .Fuzziness(Fuzziness.Auto));
    }

    private QueryContainer BuildExactQuery(QueryContainerDescriptor<object> q, SearchQuery query)
    {
        if (query.Fields.Any())
        {
            var shouldQueries = query.Fields.Select(field =>
                q.Term(t => t.Field(field).Value(query.QueryText))
            ).ToArray();

            return q.Bool(b => b.Should(shouldQueries));
        }

        return q.Term(t => t.Field("_all").Value(query.QueryText));
    }

    private QueryContainer BuildWildcardQuery(QueryContainerDescriptor<object> q, SearchQuery query)
    {
        if (query.Fields.Any())
        {
            var shouldQueries = query.Fields.Select(field =>
                q.Wildcard(w => w.Field(field).Value(query.QueryText))
            ).ToArray();

            return q.Bool(b => b.Should(shouldQueries));
        }

        return q.Wildcard(w => w.Field("_all").Value(query.QueryText));
    }

    private QueryContainer BuildPrefixQuery(QueryContainerDescriptor<object> q, SearchQuery query)
    {
        if (query.Fields.Any())
        {
            var shouldQueries = query.Fields.Select(field =>
                q.Prefix(p => p.Field(field).Value(query.QueryText))
            ).ToArray();

            return q.Bool(b => b.Should(shouldQueries));
        }

        return q.Prefix(p => p.Field("_all").Value(query.QueryText));
    }

    private QueryContainer BuildFiltersQuery(QueryContainerDescriptor<object> q, Dictionary<string, object> filters)
    {
        var filterQueries = filters.Select(filter =>
            q.Term(t => t.Field(filter.Key).Value(filter.Value))
        ).ToArray();

        return q.Bool(b => b.Must(filterQueries));
    }

    private SearchResult MapToSearchResult(ISearchResponse<object> response, SearchQuery query, long executionTimeMs)
    {
        var documents = response.Hits.Select(hit => new SearchDocument
        {
            Id = hit.Id,
            IndexName = query.IndexName,
            Content = System.Text.Json.JsonSerializer.Serialize(hit.Source),
            Score = hit.Score,
            Highlights = hit.Highlight?.ToDictionary(
                h => h.Key,
                h => h.Value.ToList()
            ) ?? new Dictionary<string, List<string>>()
        }).ToList();

        return new SearchResult
        {
            TotalCount = response.Total,
            Documents = documents,
            CurrentPage = query.Page,
            PageSize = query.PageSize,
            ExecutionTimeMs = executionTimeMs,
            TimedOut = response.TimedOut,
            MaxScore = response.MaxScore
        };
    }
}
