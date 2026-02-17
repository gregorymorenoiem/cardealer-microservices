using AdminService.Application.DTOs;
using MediatR;

namespace AdminService.Application.UseCases.Moderation;

/// <summary>
/// Query to get paginated moderation queue
/// </summary>
public record GetModerationQueueQuery(
    string? Type = null,
    string? Priority = null,
    string? Status = null,
    int Page = 1,
    int PageSize = 10
) : IRequest<PaginatedModerationResponse>;

/// <summary>
/// Query to get a specific moderation item by ID
/// </summary>
public record GetModerationItemQuery(Guid Id) : IRequest<ModerationItemDto?>;

/// <summary>
/// Query to get moderation statistics
/// </summary>
public record GetModerationStatsQuery() : IRequest<ModerationStatsDto>;

/// <summary>
/// Command to process a moderation action (approve, reject, escalate)
/// </summary>
public record ProcessModerationActionCommand(
    Guid ItemId,
    string Action,
    string ReviewerId,
    string? Reason = null,
    string? Notes = null
) : IRequest<ModerationActionResponseDto>;
