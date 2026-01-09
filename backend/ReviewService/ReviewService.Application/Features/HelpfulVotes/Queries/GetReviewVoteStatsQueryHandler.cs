using MediatR;
using ReviewService.Application.DTOs;
using ReviewService.Domain.Interfaces;

namespace ReviewService.Application.Features.HelpfulVotes.Queries;

/// <summary>
/// Handler para obtener estadísticas de votos útiles
/// </summary>
public class GetReviewVoteStatsQueryHandler : IRequestHandler<GetReviewVoteStatsQuery, ReviewVoteStatsDto>
{
    private readonly IReviewHelpfulVoteRepository _voteRepository;

    public GetReviewVoteStatsQueryHandler(IReviewHelpfulVoteRepository voteRepository)
    {
        _voteRepository = voteRepository;
    }

    public async Task<ReviewVoteStatsDto> Handle(GetReviewVoteStatsQuery request, CancellationToken cancellationToken)
    {
        var stats = await _voteRepository.GetVoteStatsAsync(request.ReviewId, cancellationToken);
        
        return new ReviewVoteStatsDto
        {
            ReviewId = request.ReviewId,
            TotalVotes = stats.totalVotes,
            HelpfulVotes = stats.helpfulVotes,
            NotHelpfulVotes = stats.totalVotes - stats.helpfulVotes,
            HelpfulPercentage = stats.totalVotes > 0 ? (decimal)stats.helpfulVotes / stats.totalVotes * 100 : 0
        };
    }
}