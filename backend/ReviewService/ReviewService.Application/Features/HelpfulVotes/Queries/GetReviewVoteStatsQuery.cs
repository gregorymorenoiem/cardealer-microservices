using MediatR;
using ReviewService.Application.DTOs;

namespace ReviewService.Application.Features.HelpfulVotes.Queries;

/// <summary>
/// Query para obtener estadísticas de votos útiles de una review
/// </summary>
public record GetReviewVoteStatsQuery(Guid ReviewId) : IRequest<ReviewVoteStatsDto>;