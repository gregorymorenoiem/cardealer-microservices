using MediatR;
using AIProcessingService.Application.DTOs;

namespace AIProcessingService.Application.Features.Queries;

// ═══════════════════════════════════════════════════════════════════════════
// GET JOB STATUS
// ═══════════════════════════════════════════════════════════════════════════

public record GetJobStatusQuery(Guid JobId) : IRequest<JobStatusResponse?>;

// ═══════════════════════════════════════════════════════════════════════════
// GET 360 STATUS
// ═══════════════════════════════════════════════════════════════════════════

public record GetSpin360StatusQuery(Guid JobId) : IRequest<Spin360StatusResponse?>;

// ═══════════════════════════════════════════════════════════════════════════
// GET VEHICLE PROCESSED IMAGES
// ═══════════════════════════════════════════════════════════════════════════

public record GetVehicleProcessedImagesQuery(Guid VehicleId) : IRequest<List<JobStatusResponse>>;

// ═══════════════════════════════════════════════════════════════════════════
// GET VEHICLE 360 SPIN
// ═══════════════════════════════════════════════════════════════════════════

public record GetVehicleSpin360Query(Guid VehicleId) : IRequest<Spin360StatusResponse?>;

// ═══════════════════════════════════════════════════════════════════════════
// GET AVAILABLE BACKGROUNDS
// ═══════════════════════════════════════════════════════════════════════════

public record GetAvailableBackgroundsQuery(
    string AccountType,
    bool HasActiveSubscription
) : IRequest<AvailableBackgroundsResponse>;

// ═══════════════════════════════════════════════════════════════════════════
// GET FEATURES
// ═══════════════════════════════════════════════════════════════════════════

public record GetFeaturesQuery(
    string AccountType,
    bool HasActiveSubscription
) : IRequest<FeaturesResponse>;

// ═══════════════════════════════════════════════════════════════════════════
// GET QUEUE STATS
// ═══════════════════════════════════════════════════════════════════════════

public record GetQueueStatsQuery() : IRequest<QueueStatsResponse>;
