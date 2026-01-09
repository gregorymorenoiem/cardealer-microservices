using MediatR;
using ReviewService.Application.DTOs;
using CarDealer.Shared.Application.Interfaces;

namespace ReviewService.Application.Features.Reviews.Commands;

/// &lt;summary&gt;
/// Command para moderar una review (aprobar/rechazar)
/// &lt;/summary&gt;
public record ModerateReviewCommand : IRequest&lt;Result&lt;bool&gt;&gt;
{
    public Guid ReviewId { get; init; }
    public Guid ModeratorId { get; init; }
    public bool IsApproved { get; init; }
    public string? RejectionReason { get; init; }
}