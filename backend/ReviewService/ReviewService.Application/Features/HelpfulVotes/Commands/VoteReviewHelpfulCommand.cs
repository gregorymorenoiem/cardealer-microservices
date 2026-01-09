using MediatR;
using ReviewService.Application.DTOs;

namespace ReviewService.Application.Features.HelpfulVotes.Commands;

/// <summary>
/// Comando para votar que una review es útil
/// </summary>
public record VoteReviewHelpfulCommand(
    Guid ReviewId,
    Guid UserId,
    bool IsHelpful,
    string? UserIpAddress = null) : IRequest<ReviewHelpfulVoteDto>;