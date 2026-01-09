using MediatR;
using ReviewService.Application.DTOs;
using ReviewService.Domain.Base;

namespace ReviewService.Application.Features.Reviews.Commands;

/// <summary>
/// Command para moderar una review (aprobar/rechazar)
/// </summary>
public record ModerateReviewCommand : IRequest<Result<bool>>
{
    public Guid ReviewId { get; init; }
    public Guid ModeratorId { get; init; }
    public bool IsApproved { get; init; }
    public string? RejectionReason { get; init; }
}