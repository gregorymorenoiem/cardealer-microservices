using AdminService.Domain.Entities;
using AdminService.Domain.Interfaces;
using AdminService.Infrastructure.External;
using Microsoft.Extensions.Logging;

namespace AdminService.Infrastructure.Persistence;

/// <summary>
/// Implementation of IStatisticsRepository — fetches vehicle stats from VehiclesSaleService.
/// User and Dealer stats are aggregated by the Dashboard handler (via IPlatformUserService / IDealerService).
/// </summary>
public class EfStatisticsRepository : IStatisticsRepository
{
    private readonly IVehicleServiceClient _vehicleServiceClient;
    private readonly ILogger<EfStatisticsRepository> _logger;

    public EfStatisticsRepository(
        IVehicleServiceClient vehicleServiceClient,
        ILogger<EfStatisticsRepository> logger)
    {
        _vehicleServiceClient = vehicleServiceClient;
        _logger = logger;
    }

    public async Task<PlatformStats> GetPlatformStatsAsync()
    {
        _logger.LogDebug("Getting vehicle stats from VehiclesSaleService");

        try
        {
            var vehicleStats = await _vehicleServiceClient.GetVehicleStatsAsync();

            return new PlatformStats
            {
                TotalDealers = 0,    // Provided by IDealerService in DashboardHandler
                ActiveDealers = 0,
                PendingDealers = 0,
                TotalUsers = 0,      // Provided by IPlatformUserService in DashboardHandler
                TotalListings = vehicleStats.Total,
                PendingListings = vehicleStats.Pending,
                TotalTransactions = 0,
                MonthlyRevenue = 0,  // Provided by IDealerService.TotalMrr in DashboardHandler
                OpenTickets = 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching vehicle stats from VehiclesSaleService");
            return new PlatformStats();
        }
    }

    public async Task<IEnumerable<PendingDealerInfo>> GetPendingDealersAsync(int limit)
    {
        _logger.LogDebug("GetPendingDealersAsync — returning empty (use IDealerService instead)");
        await Task.CompletedTask;
        return new List<PendingDealerInfo>();
    }

    public async Task<PlatformSettings> GetPlatformSettingsAsync()
    {
        _logger.LogDebug("Getting platform settings");
        await Task.CompletedTask;

        return new PlatformSettings
        {
            MaintenanceMode = false,
            MaintenanceMessage = null,
            RegistrationEnabled = true,
            DealerRegistrationEnabled = true,
            EarlyBirdActive = false,
            EarlyBirdEndDate = null,
            EarlyBirdDiscount = 0,
            FeatureFlags = new Dictionary<string, object>(),
            Limits = new Dictionary<string, object>()
        };
    }

    public async Task UpdatePlatformSettingsAsync(Dictionary<string, object> settings)
    {
        _logger.LogInformation("Updating platform settings: {SettingsCount} settings", settings.Count);
        await Task.CompletedTask;
    }
}
