using MediatR;
using Microsoft.Extensions.Logging;
using AdminService.Application.Interfaces;

namespace AdminService.Application.UseCases.Dealers;

/// <summary>
/// Handler for getting dealers with filtering
/// </summary>
public class GetDealersQueryHandler : IRequestHandler<GetDealersQuery, PaginatedDealerResult>
{
    private readonly IDealerService _dealerService;
    private readonly ILogger<GetDealersQueryHandler> _logger;

    public GetDealersQueryHandler(IDealerService dealerService, ILogger<GetDealersQueryHandler> logger)
    {
        _dealerService = dealerService;
        _logger = logger;
    }

    public async Task<PaginatedDealerResult> Handle(GetDealersQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching dealers: search={Search}, status={Status}, plan={Plan}",
            request.Search, request.Status, request.Plan);

        return await _dealerService.GetDealersAsync(
            search: request.Search,
            status: request.Status,
            plan: request.Plan,
            verified: request.Verified,
            page: request.Page,
            pageSize: request.PageSize,
            cancellationToken: cancellationToken);
    }
}

/// <summary>
/// Handler for getting dealer statistics
/// </summary>
public class GetDealerStatsQueryHandler : IRequestHandler<GetDealerStatsQuery, DealerStatsDto>
{
    private readonly IDealerService _dealerService;
    private readonly ILogger<GetDealerStatsQueryHandler> _logger;

    public GetDealerStatsQueryHandler(IDealerService dealerService, ILogger<GetDealerStatsQueryHandler> logger)
    {
        _dealerService = dealerService;
        _logger = logger;
    }

    public async Task<DealerStatsDto> Handle(GetDealerStatsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching dealer statistics");
        return await _dealerService.GetDealerStatsAsync(cancellationToken);
    }
}

/// <summary>
/// Handler for getting a single dealer
/// </summary>
public class GetDealerByIdQueryHandler : IRequestHandler<GetDealerByIdQuery, AdminDealerDto?>
{
    private readonly IDealerService _dealerService;
    private readonly ILogger<GetDealerByIdQueryHandler> _logger;

    public GetDealerByIdQueryHandler(IDealerService dealerService, ILogger<GetDealerByIdQueryHandler> logger)
    {
        _dealerService = dealerService;
        _logger = logger;
    }

    public async Task<AdminDealerDto?> Handle(GetDealerByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching dealer {DealerId}", request.DealerId);
        return await _dealerService.GetDealerByIdAsync(request.DealerId, cancellationToken);
    }
}

/// <summary>
/// Handler for verifying a dealer
/// </summary>
public class VerifyDealerCommandHandler : IRequestHandler<VerifyDealerCommand, Unit>
{
    private readonly IDealerService _dealerService;
    private readonly ILogger<VerifyDealerCommandHandler> _logger;

    public VerifyDealerCommandHandler(IDealerService dealerService, ILogger<VerifyDealerCommandHandler> logger)
    {
        _dealerService = dealerService;
        _logger = logger;
    }

    public async Task<Unit> Handle(VerifyDealerCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Verifying dealer {DealerId}", request.DealerId);
        await _dealerService.VerifyDealerAsync(request.DealerId, cancellationToken);
        return Unit.Value;
    }
}

/// <summary>
/// Handler for suspending a dealer
/// </summary>
public class SuspendDealerCommandHandler : IRequestHandler<SuspendDealerCommand, Unit>
{
    private readonly IDealerService _dealerService;
    private readonly ILogger<SuspendDealerCommandHandler> _logger;

    public SuspendDealerCommandHandler(IDealerService dealerService, ILogger<SuspendDealerCommandHandler> logger)
    {
        _dealerService = dealerService;
        _logger = logger;
    }

    public async Task<Unit> Handle(SuspendDealerCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Suspending dealer {DealerId}: {Reason}", request.DealerId, request.Reason);
        await _dealerService.SuspendDealerAsync(request.DealerId, request.Reason, cancellationToken);
        return Unit.Value;
    }
}

/// <summary>
/// Handler for reactivating a dealer
/// </summary>
public class ReactivateDealerCommandHandler : IRequestHandler<ReactivateDealerCommand, Unit>
{
    private readonly IDealerService _dealerService;
    private readonly ILogger<ReactivateDealerCommandHandler> _logger;

    public ReactivateDealerCommandHandler(IDealerService dealerService, ILogger<ReactivateDealerCommandHandler> logger)
    {
        _dealerService = dealerService;
        _logger = logger;
    }

    public async Task<Unit> Handle(ReactivateDealerCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Reactivating dealer {DealerId}", request.DealerId);
        await _dealerService.ReactivateDealerAsync(request.DealerId, cancellationToken);
        return Unit.Value;
    }
}

/// <summary>
/// Handler for deleting a dealer
/// </summary>
public class DeleteDealerCommandHandler : IRequestHandler<DeleteDealerCommand, Unit>
{
    private readonly IDealerService _dealerService;
    private readonly ILogger<DeleteDealerCommandHandler> _logger;

    public DeleteDealerCommandHandler(IDealerService dealerService, ILogger<DeleteDealerCommandHandler> logger)
    {
        _dealerService = dealerService;
        _logger = logger;
    }

    public async Task<Unit> Handle(DeleteDealerCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting dealer {DealerId}", request.DealerId);
        await _dealerService.DeleteDealerAsync(request.DealerId, cancellationToken);
        return Unit.Value;
    }
}
