using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace AdvertisingService.Application.Clients;

public class VehicleServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<VehicleServiceClient> _logger;

    public VehicleServiceClient(HttpClient httpClient, ILogger<VehicleServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<VehicleBasicInfo?> GetVehicleBasicInfoAsync(Guid vehicleId, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/vehicles/{vehicleId}", ct);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get vehicle {VehicleId}: {StatusCode}", vehicleId, response.StatusCode);
                return null;
            }

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponseWrapper<VehicleBasicInfo>>(cancellationToken: ct);
            return apiResponse?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling VehicleService for vehicle {VehicleId}", vehicleId);
            return null;
        }
    }
}

public class VehicleBasicInfo
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Make { get; set; }
    public string? Model { get; set; }
    public int? Year { get; set; }
    public decimal Price { get; set; }
    public int ImageCount { get; set; }
    public bool HasDescription { get; set; }
    public Guid SellerId { get; set; }
}

public class ApiResponseWrapper<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Error { get; set; }
}
