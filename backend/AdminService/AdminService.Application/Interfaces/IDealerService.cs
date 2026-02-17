using AdminService.Application.UseCases.Dealers;

namespace AdminService.Application.Interfaces;

/// <summary>
/// Interface for dealer management operations.
/// This service calls UserService/DealerManagementService via HTTP.
/// </summary>
public interface IDealerService
{
    Task<PaginatedDealerResult> GetDealersAsync(
        string? search = null,
        string? status = null,
        string? plan = null,
        bool? verified = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);

    Task<DealerStatsDto> GetDealerStatsAsync(CancellationToken cancellationToken = default);

    Task<AdminDealerDto?> GetDealerByIdAsync(Guid dealerId, CancellationToken cancellationToken = default);

    Task VerifyDealerAsync(Guid dealerId, CancellationToken cancellationToken = default);

    Task SuspendDealerAsync(Guid dealerId, string reason, CancellationToken cancellationToken = default);

    Task ReactivateDealerAsync(Guid dealerId, CancellationToken cancellationToken = default);

    Task DeleteDealerAsync(Guid dealerId, CancellationToken cancellationToken = default);
}
