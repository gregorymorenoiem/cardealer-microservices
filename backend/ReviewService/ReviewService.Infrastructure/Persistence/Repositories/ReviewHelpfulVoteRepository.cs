using Microsoft.EntityFrameworkCore;
using ReviewService.Domain.Entities;
using ReviewService.Domain.Interfaces;
using ReviewService.Infrastructure.Persistence;

namespace ReviewService.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementación del repositorio de votos útiles
/// </summary>
public class ReviewHelpfulVoteRepository : Repository<ReviewHelpfulVote, Guid>, IReviewHelpfulVoteRepository
{
    public ReviewHelpfulVoteRepository(ReviewDbContext context) : base(context)
    {
    }

    public async Task<ReviewHelpfulVote?> GetByReviewAndUserAsync(Guid reviewId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.ReviewHelpfulVotes
            .FirstOrDefaultAsync(x => x.ReviewId == reviewId && x.UserId == userId, cancellationToken);
    }

    public async Task<(int helpfulVotes, int totalVotes)> GetVoteStatsAsync(Guid reviewId, CancellationToken cancellationToken = default)
    {
        var votes = await _context.ReviewHelpfulVotes
            .Where(x => x.ReviewId == reviewId)
            .ToListAsync(cancellationToken);

        var totalVotes = votes.Count;
        var helpfulVotes = votes.Count(x => x.IsHelpful);

        return (helpfulVotes, totalVotes);
    }

    /// <summary>
    /// Obtiene todos los votos de una review
    /// </summary>
    public async Task<List<ReviewHelpfulVote>> GetByReviewIdAsync(Guid reviewId, CancellationToken cancellationToken = default)
    {
        return await _context.ReviewHelpfulVotes
            .Where(v => v.ReviewId == reviewId)
            .Include(v => v.Review)
            .OrderByDescending(v => v.VotedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasUserVotedAsync(Guid reviewId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.ReviewHelpfulVotes
            .AnyAsync(x => x.ReviewId == reviewId && x.UserId == userId, cancellationToken);
    }

    public async Task<List<ReviewHelpfulVote>> GetByUserIdAsync(Guid userId, int limit = 100, CancellationToken cancellationToken = default)
    {
        return await _context.ReviewHelpfulVotes
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.VotedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ReviewHelpfulVote>> GetSuspiciousVotesByIpAsync(string ipAddress, int timeWindowHours = 24, CancellationToken cancellationToken = default)
    {
        var cutoffTime = DateTime.UtcNow.AddHours(-timeWindowHours);

        return await _context.ReviewHelpfulVotes
            .Where(x => x.UserIpAddress == ipAddress && x.VotedAt >= cutoffTime)
            .OrderByDescending(x => x.VotedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Dictionary<Guid, (int Total, int Helpful)>> GetVoteStatsBatchAsync(List<Guid> reviewIds, CancellationToken cancellationToken = default)
    {
        var votes = await _context.ReviewHelpfulVotes
            .Where(x => reviewIds.Contains(x.ReviewId))
            .GroupBy(x => x.ReviewId)
            .Select(g => new
            {
                ReviewId = g.Key,
                Total = g.Count(),
                Helpful = g.Count(x => x.IsHelpful)
            })
            .ToListAsync(cancellationToken);

        return votes.ToDictionary(x => x.ReviewId, x => (x.Total, x.Helpful));
    }
}