using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ChatbotService.Domain.Entities;
using ChatbotService.Domain.Interfaces;
using ChatbotService.Domain.Models;

namespace ChatbotService.Infrastructure.Services;

/// <summary>
/// Configuración del servicio de embeddings
/// </summary>
public class EmbeddingSettings
{
    /// <summary>URL del servidor de embeddings (LlmServer endpoint)</summary>
    public string ServerUrl { get; set; } = "http://llm-server:8000";
    
    /// <summary>Modelo de embeddings a usar</summary>
    public string Model { get; set; } = "all-MiniLM-L6-v2";
    
    /// <summary>Dimensiones del vector de embedding</summary>
    public int Dimensions { get; set; } = 384;
    
    /// <summary>Timeout en segundos para generar embeddings</summary>
    public int TimeoutSeconds { get; set; } = 30;
}

/// <summary>
/// Servicio que genera embeddings usando el endpoint /v1/embeddings del LlmServer.
/// Usa sentence-transformers (all-MiniLM-L6-v2) para vectorización local.
/// </summary>
public class EmbeddingService : IEmbeddingService
{
    private readonly EmbeddingSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<EmbeddingService> _logger;

    public EmbeddingService(
        IOptions<EmbeddingSettings> settings,
        IHttpClientFactory httpClientFactory,
        ILogger<EmbeddingService> logger)
    {
        _settings = settings.Value;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct = default)
    {
        var results = await GenerateEmbeddingsBatchAsync(new List<string> { text }, ct);
        return results.FirstOrDefault() ?? Array.Empty<float>();
    }

    public async Task<List<float[]>> GenerateEmbeddingsBatchAsync(
        List<string> texts, CancellationToken ct = default)
    {
        if (!texts.Any())
            return new List<float[]>();

        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(_settings.TimeoutSeconds));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token);

        try
        {
            var client = _httpClientFactory.CreateClient("EmbeddingServer");
            
            var request = new
            {
                input = texts,
                model = _settings.Model
            };

            var response = await client.PostAsJsonAsync(
                "/v1/embeddings", request, linkedCts.Token);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<EmbeddingResponse>(
                cancellationToken: linkedCts.Token);

            if (result?.Data == null)
            {
                _logger.LogWarning("Embedding response has no data");
                return texts.Select(_ => new float[_settings.Dimensions]).ToList();
            }

            return result.Data
                .OrderBy(d => d.Index)
                .Select(d => d.Embedding)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate embeddings for {Count} texts", texts.Count);
            // Retornar vectores vacíos como fallback
            return texts.Select(_ => new float[_settings.Dimensions]).ToList();
        }
    }

    private class EmbeddingResponse
    {
        public List<EmbeddingData>? Data { get; set; }
        public string? Model { get; set; }
    }

    private class EmbeddingData
    {
        public int Index { get; set; }
        public float[] Embedding { get; set; } = Array.Empty<float>();
    }
}

/// <summary>
/// Servicio de búsqueda vectorial usando pgvector para RAG.
/// Implementa búsqueda híbrida: semántica (cosine similarity) + filtros SQL.
/// </summary>
public class VectorSearchService : IVectorSearchService
{
    private readonly IEmbeddingService _embeddingService;
    private readonly IVehicleEmbeddingRepository _embeddingRepository;
    private readonly ILogger<VectorSearchService> _logger;

    public VectorSearchService(
        IEmbeddingService embeddingService,
        IVehicleEmbeddingRepository embeddingRepository,
        ILogger<VectorSearchService> logger)
    {
        _embeddingService = embeddingService;
        _embeddingRepository = embeddingRepository;
        _logger = logger;
    }

    public async Task<List<VehicleSearchResult>> SearchAsync(
        Guid dealerId,
        string query,
        VehicleSearchFilters? filters = null,
        int topK = 5,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new List<VehicleSearchResult>();

        // 1. Generar embedding del query del usuario
        var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(query, ct);
        
        if (queryEmbedding.Length == 0)
        {
            _logger.LogWarning("Failed to generate query embedding, falling back to text search");
            return new List<VehicleSearchResult>();
        }

        // 2. Búsqueda híbrida: vector similarity + SQL filters
        var results = await _embeddingRepository.HybridSearchAsync(
            dealerId, queryEmbedding, filters, topK, ct);

        _logger.LogInformation(
            "Vector search for dealer {DealerId}: query='{Query}', results={Count}, " +
            "filters={HasFilters}",
            dealerId, query.Length > 50 ? query[..50] + "..." : query,
            results.Count, filters != null);

        return results;
    }

    public async Task UpsertVehicleEmbeddingAsync(
        ChatbotVehicle vehicle, Guid dealerId, CancellationToken ct = default)
    {
        // Construir texto para embedding
        var text = BuildVehicleEmbeddingText(vehicle);
        
        // Generar embedding
        var embedding = await _embeddingService.GenerateEmbeddingAsync(text, ct);
        
        if (embedding.Length == 0)
        {
            _logger.LogWarning("Failed to generate embedding for vehicle {VehicleId}", vehicle.VehicleId);
            return;
        }

        var vehicleEmbedding = new VehicleEmbedding
        {
            Id = Guid.NewGuid(),
            VehicleId = vehicle.VehicleId,
            DealerId = dealerId,
            Content = text,
            Embedding = embedding,
            Metadata = new VehicleEmbeddingMetadata
            {
                Make = vehicle.Make,
                Model = vehicle.Model,
                Year = vehicle.Year,
                Price = vehicle.Price,
                FuelType = vehicle.FuelType,
                Transmission = vehicle.Transmission,
                BodyType = vehicle.BodyType,
                Mileage = vehicle.Mileage,
                ExteriorColor = vehicle.ExteriorColor ?? vehicle.Color,
                IsAvailable = vehicle.IsAvailable
            },
            UpdatedAt = DateTime.UtcNow
        };

        await _embeddingRepository.UpsertAsync(vehicleEmbedding, ct);
        
        _logger.LogDebug("Upserted embedding for vehicle {VehicleId} ({Year} {Make} {Model})",
            vehicle.VehicleId, vehicle.Year, vehicle.Make, vehicle.Model);
    }

    public async Task DeleteVehicleEmbeddingAsync(Guid vehicleId, CancellationToken ct = default)
    {
        await _embeddingRepository.DeleteByVehicleIdAsync(vehicleId, ct);
        _logger.LogDebug("Deleted embedding for vehicle {VehicleId}", vehicleId);
    }

    public async Task<int> RebuildDealerEmbeddingsAsync(
        Guid dealerId, IEnumerable<ChatbotVehicle> vehicles, CancellationToken ct = default)
    {
        var vehicleList = vehicles.ToList();
        if (!vehicleList.Any()) return 0;

        // Generar textos para todos los vehículos
        var texts = vehicleList.Select(BuildVehicleEmbeddingText).ToList();
        
        // Generar embeddings en batch
        var embeddings = await _embeddingService.GenerateEmbeddingsBatchAsync(texts, ct);

        // Crear entidades de embedding
        var vehicleEmbeddings = vehicleList.Select((vehicle, i) => new VehicleEmbedding
        {
            Id = Guid.NewGuid(),
            VehicleId = vehicle.VehicleId,
            DealerId = dealerId,
            Content = texts[i],
            Embedding = embeddings[i],
            Metadata = new VehicleEmbeddingMetadata
            {
                Make = vehicle.Make,
                Model = vehicle.Model,
                Year = vehicle.Year,
                Price = vehicle.Price,
                FuelType = vehicle.FuelType,
                Transmission = vehicle.Transmission,
                BodyType = vehicle.BodyType,
                Mileage = vehicle.Mileage,
                ExteriorColor = vehicle.ExteriorColor ?? vehicle.Color,
                IsAvailable = vehicle.IsAvailable
            },
            UpdatedAt = DateTime.UtcNow
        }).ToList();

        // Bulk upsert
        await _embeddingRepository.BulkUpsertAsync(dealerId, vehicleEmbeddings, ct);

        _logger.LogInformation("Rebuilt {Count} embeddings for dealer {DealerId}",
            vehicleEmbeddings.Count, dealerId);

        return vehicleEmbeddings.Count;
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct = default)
    {
        return await _embeddingService.GenerateEmbeddingAsync(text, ct);
    }

    /// <summary>
    /// Construye el texto de un vehículo para generar su embedding.
    /// Formato optimizado para búsqueda semántica en español dominicano.
    /// </summary>
    private static string BuildVehicleEmbeddingText(ChatbotVehicle vehicle)
    {
        var parts = new List<string>
        {
            $"{vehicle.Year} {vehicle.Make} {vehicle.Model}",
            vehicle.Trim ?? "",
            $"Precio RD${vehicle.Price:N0}",
            vehicle.Mileage.HasValue ? $"{vehicle.Mileage:N0} kilómetros" : "",
            $"Combustible: {vehicle.FuelType ?? "N/A"}",
            $"Transmisión: {vehicle.Transmission ?? "N/A"}",
            $"Tipo: {vehicle.BodyType ?? "N/A"}",
            $"Color: {vehicle.ExteriorColor ?? vehicle.Color ?? "N/A"}",
            !string.IsNullOrEmpty(vehicle.Description) ? vehicle.Description : "",
            vehicle.IsOnSale ? "EN OFERTA" : ""
        };

        return string.Join(". ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
    }
}
