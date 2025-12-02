using Microsoft.Extensions.Logging;
using Nest;
using SearchService.Domain.Entities;
using SearchService.Domain.Enums;
using SearchService.Domain.Interfaces;
using SearchService.Infrastructure.Configuration;

namespace SearchService.Infrastructure.Services;

/// <summary>
/// Gestor de índices de Elasticsearch
/// </summary>
public class ElasticsearchIndexManager : IIndexManager
{
    private readonly IElasticClient _client;
    private readonly ILogger<ElasticsearchIndexManager> _logger;
    private readonly ElasticsearchOptions _options;

    public ElasticsearchIndexManager(
        IElasticClient client,
        ILogger<ElasticsearchIndexManager> logger,
        ElasticsearchOptions options)
    {
        _client = client;
        _logger = logger;
        _options = options;
    }

    public async Task<bool> CreateIndexAsync(string indexName, Dictionary<string, object>? mappings = null, Dictionary<string, object>? settings = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var fullIndexName = GetFullIndexName(indexName);

            var createIndexDescriptor = new CreateIndexDescriptor(fullIndexName)
                .Settings(s => s
                    .NumberOfShards(_options.DefaultShards)
                    .NumberOfReplicas(_options.DefaultReplicas));

            if (settings != null)
            {
                // Aplicar settings custom
                foreach (var setting in settings)
                {
                    createIndexDescriptor = createIndexDescriptor.Settings(s => s.Setting(setting.Key, setting.Value));
                }
            }

            var response = await _client.Indices.CreateAsync(createIndexDescriptor, cancellationToken);

            if (!response.IsValid)
            {
                _logger.LogError("Error creating index {IndexName}: {Error}", fullIndexName, response.DebugInformation);
                return false;
            }

            _logger.LogInformation("Index {IndexName} created successfully", fullIndexName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating index {IndexName}", indexName);
            return false;
        }
    }

    public async Task<bool> DeleteIndexAsync(string indexName, CancellationToken cancellationToken = default)
    {
        try
        {
            var fullIndexName = GetFullIndexName(indexName);
            var response = await _client.Indices.DeleteAsync(fullIndexName, ct: cancellationToken);

            if (!response.IsValid)
            {
                _logger.LogError("Error deleting index {IndexName}: {Error}", fullIndexName, response.DebugInformation);
                return false;
            }

            _logger.LogInformation("Index {IndexName} deleted successfully", fullIndexName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting index {IndexName}", indexName);
            return false;
        }
    }

    public async Task<bool> IndexExistsAsync(string indexName, CancellationToken cancellationToken = default)
    {
        try
        {
            var fullIndexName = GetFullIndexName(indexName);
            var response = await _client.Indices.ExistsAsync(fullIndexName, ct: cancellationToken);
            return response.Exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if index {IndexName} exists", indexName);
            return false;
        }
    }

    public async Task<IndexMetadata?> GetIndexMetadataAsync(string indexName, CancellationToken cancellationToken = default)
    {
        try
        {
            var fullIndexName = GetFullIndexName(indexName);

            // Obtener estadísticas del índice
            var statsResponse = await _client.Indices.StatsAsync(fullIndexName, ct: cancellationToken);
            if (!statsResponse.IsValid)
            {
                return null;
            }

            // Obtener información del índice
            var getResponse = await _client.Indices.GetAsync(fullIndexName, ct: cancellationToken);
            if (!getResponse.IsValid)
            {
                return null;
            }

            var indexStats = statsResponse.Indices.FirstOrDefault().Value;
            var indexInfo = getResponse.Indices.FirstOrDefault().Value;

            return new IndexMetadata
            {
                Name = indexName,
                Status = IndexStatus.Active,
                DocumentCount = indexStats?.Total?.Documents?.Count ?? 0,
                SizeInBytes = (long)(indexStats?.Total?.Store?.SizeInBytes ?? 0),
                Mappings = new Dictionary<string, object>(),
                Settings = new Dictionary<string, object>(),
                PrimaryShards = indexInfo?.Settings?.NumberOfShards ?? _options.DefaultShards,
                ReplicaCount = indexInfo?.Settings?.NumberOfReplicas ?? _options.DefaultReplicas,
                UpdatedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting metadata for index {IndexName}", indexName);
            return null;
        }
    }

    public async Task<List<string>> ListIndicesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _client.Cat.IndicesAsync(new CatIndicesRequest { Format = "json" }, cancellationToken);

            if (!response.IsValid)
            {
                _logger.LogError("Error listing indices: {Error}", response.DebugInformation);
                return new List<string>();
            }

            var prefix = string.IsNullOrWhiteSpace(_options.IndexPrefix)
                ? string.Empty
                : $"{_options.IndexPrefix}_";

            return response.Records
                .Where(r => r.Index.StartsWith(prefix))
                .Select(r => r.Index.Replace(prefix, string.Empty))
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing indices");
            return new List<string>();
        }
    }

    public async Task<bool> UpdateIndexSettingsAsync(string indexName, Dictionary<string, object> settings, CancellationToken cancellationToken = default)
    {
        try
        {
            var fullIndexName = GetFullIndexName(indexName);

            var updateSettingsDescriptor = new UpdateIndexSettingsDescriptor(fullIndexName);

            foreach (var setting in settings)
            {
                updateSettingsDescriptor = updateSettingsDescriptor.IndexSettings(s => s.Setting(setting.Key, setting.Value));
            }

            var response = await _client.Indices.UpdateSettingsAsync(updateSettingsDescriptor, cancellationToken);

            if (!response.IsValid)
            {
                _logger.LogError("Error updating settings for index {IndexName}: {Error}", fullIndexName, response.DebugInformation);
                return false;
            }

            _logger.LogInformation("Settings updated for index {IndexName}", fullIndexName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating settings for index {IndexName}", indexName);
            return false;
        }
    }

    public async Task<bool> ReindexAsync(string sourceIndex, string destinationIndex, CancellationToken cancellationToken = default)
    {
        try
        {
            var sourceFullName = GetFullIndexName(sourceIndex);
            var destFullName = GetFullIndexName(destinationIndex);

            var response = await _client.ReindexOnServerAsync(r => r
                .Source(s => s.Index(sourceFullName))
                .Destination(d => d.Index(destFullName))
                .WaitForCompletion(false), cancellationToken);

            if (!response.IsValid)
            {
                _logger.LogError("Error reindexing from {Source} to {Destination}: {Error}",
                    sourceIndex, destinationIndex, response.DebugInformation);
                return false;
            }

            _logger.LogInformation("Reindex started from {Source} to {Destination}", sourceIndex, destinationIndex);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reindexing from {Source} to {Destination}", sourceIndex, destinationIndex);
            return false;
        }
    }

    public async Task<bool> CreateAliasAsync(string indexName, string aliasName, CancellationToken cancellationToken = default)
    {
        try
        {
            var fullIndexName = GetFullIndexName(indexName);

            var response = await _client.Indices.PutAliasAsync(fullIndexName, aliasName, ct: cancellationToken);

            if (!response.IsValid)
            {
                _logger.LogError("Error creating alias {Alias} for index {IndexName}: {Error}",
                    aliasName, fullIndexName, response.DebugInformation);
                return false;
            }

            _logger.LogInformation("Alias {Alias} created for index {IndexName}", aliasName, fullIndexName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating alias {Alias} for index {IndexName}", aliasName, indexName);
            return false;
        }
    }

    public async Task<bool> DeleteAliasAsync(string indexName, string aliasName, CancellationToken cancellationToken = default)
    {
        try
        {
            var fullIndexName = GetFullIndexName(indexName);

            var response = await _client.Indices.DeleteAliasAsync(fullIndexName, aliasName, ct: cancellationToken);

            if (!response.IsValid)
            {
                _logger.LogError("Error deleting alias {Alias} from index {IndexName}: {Error}",
                    aliasName, fullIndexName, response.DebugInformation);
                return false;
            }

            _logger.LogInformation("Alias {Alias} deleted from index {IndexName}", aliasName, fullIndexName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting alias {Alias} from index {IndexName}", aliasName, indexName);
            return false;
        }
    }

    public async Task<bool> RefreshIndexAsync(string indexName, CancellationToken cancellationToken = default)
    {
        try
        {
            var fullIndexName = GetFullIndexName(indexName);
            var response = await _client.Indices.RefreshAsync(fullIndexName, ct: cancellationToken);

            return response.IsValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing index {IndexName}", indexName);
            return false;
        }
    }

    private string GetFullIndexName(string indexName)
    {
        return string.IsNullOrWhiteSpace(_options.IndexPrefix)
            ? indexName.ToLowerInvariant()
            : $"{_options.IndexPrefix}_{indexName}".ToLowerInvariant();
    }
}
