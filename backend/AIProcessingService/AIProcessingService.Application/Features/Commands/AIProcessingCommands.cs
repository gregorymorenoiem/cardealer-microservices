using MediatR;
using AIProcessingService.Application.DTOs;

namespace AIProcessingService.Application.Features.Commands;

// ═══════════════════════════════════════════════════════════════════════════
// PROCESS SINGLE IMAGE
// ═══════════════════════════════════════════════════════════════════════════

public record ProcessImageCommand(
    Guid VehicleId,
    Guid? UserId,
    string ImageUrl,
    ProcessingType Type,
    ProcessingOptionsDto? Options
) : IRequest<ProcessImageResponse>;

// ═══════════════════════════════════════════════════════════════════════════
// PROCESS BATCH
// ═══════════════════════════════════════════════════════════════════════════

public record ProcessBatchCommand(
    Guid VehicleId,
    Guid? UserId,
    List<string> ImageUrls,
    ProcessingType Type,
    ProcessingOptionsDto? Options
) : IRequest<ProcessBatchResponse>;

// ═══════════════════════════════════════════════════════════════════════════
// GENERATE 360
// ═══════════════════════════════════════════════════════════════════════════

public record Generate360Command(
    Guid VehicleId,
    Guid? UserId,
    Spin360SourceType SourceType,
    string? VideoUrl,
    List<string>? ImageUrls,
    int FrameCount,
    Spin360OptionsDto? Options
) : IRequest<Generate360Response>;

// ═══════════════════════════════════════════════════════════════════════════
// CANCEL JOB
// ═══════════════════════════════════════════════════════════════════════════

public record CancelJobCommand(Guid JobId) : IRequest<bool>;

// ═══════════════════════════════════════════════════════════════════════════
// RETRY JOB
// ═══════════════════════════════════════════════════════════════════════════

public record RetryJobCommand(Guid JobId) : IRequest<ProcessImageResponse>;
// ═══════════════════════════════════════════════════════════════════════════
// UPDATE JOB STATUS (from worker callback)
// ═══════════════════════════════════════════════════════════════════════════

public record UpdateJobStatusCommand(
    Guid JobId,
    bool Success,
    string? ProcessedImageUrl,
    string? MaskUrl,
    int? ProcessingTimeMs,
    string? Error
) : IRequest<Unit>;