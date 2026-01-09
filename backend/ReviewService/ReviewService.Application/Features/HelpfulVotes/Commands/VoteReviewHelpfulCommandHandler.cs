using MediatR;
using Microsoft.Extensions.Logging;
using ReviewService.Application.DTOs;
using ReviewService.Domain.Entities;
using ReviewService.Domain.Interfaces;

namespace ReviewService.Application.Features.HelpfulVotes.Commands;

/// <summary>
/// Handler para votar una review como útil
/// </summary>
public class VoteReviewHelpfulCommandHandler : IRequestHandler<VoteReviewHelpfulCommand, ReviewHelpfulVoteDto>
{
    private readonly IReviewHelpfulVoteRepository _voteRepository;
    private readonly IReviewRepository _reviewRepository;
    private readonly ILogger<VoteReviewHelpfulCommandHandler> _logger;

    public VoteReviewHelpfulCommandHandler(
        IReviewHelpfulVoteRepository voteRepository, 
        IReviewRepository reviewRepository,
        ILogger<VoteReviewHelpfulCommandHandler> logger)
    {
        _voteRepository = voteRepository;
        _reviewRepository = reviewRepository;
        _logger = logger;
    }

    public async Task<ReviewHelpfulVoteDto> Handle(VoteReviewHelpfulCommand request, CancellationToken cancellationToken)
    {
        // Verificar que la review existe
        var review = await _reviewRepository.GetByIdAsync(request.ReviewId);
        if (review == null)
        {
            throw new InvalidOperationException($"Review with ID {request.ReviewId} not found");
        }

        // Verificar si el usuario ya votó esta review
        var existingVote = await _voteRepository.GetByReviewAndUserAsync(request.ReviewId, request.UserId, cancellationToken);
        if (existingVote != null)
        {
            // Actualizar voto existente
            existingVote.IsHelpful = request.IsHelpful;
            existingVote.CreatedAt = DateTime.UtcNow;
            
            await _voteRepository.UpdateAsync(existingVote);
            
            _logger.LogInformation("Updated helpful vote for ReviewId {ReviewId} by UserId {UserId}: {IsHelpful}", 
                request.ReviewId, request.UserId, request.IsHelpful);
            
            return new ReviewHelpfulVoteDto
            {
                Id = existingVote.Id,
                ReviewId = existingVote.ReviewId,
                UserId = existingVote.UserId,
                IsHelpful = existingVote.IsHelpful,
                VotedAt = existingVote.CreatedAt,
                UserIpAddress = existingVote.UserIpAddress
            };
        }
        else
        {
            // Crear nuevo voto
            var vote = new ReviewHelpfulVote
            {
                Id = Guid.NewGuid(),
                ReviewId = request.ReviewId,
                UserId = request.UserId,
                IsHelpful = request.IsHelpful,
                CreatedAt = DateTime.UtcNow,
                UserIpAddress = request.UserIpAddress ?? "Unknown"
            };

            await _voteRepository.AddAsync(vote);
            
            _logger.LogInformation("Created helpful vote for ReviewId {ReviewId} by UserId {UserId}: {IsHelpful}", 
                request.ReviewId, request.UserId, request.IsHelpful);

            return new ReviewHelpfulVoteDto
            {
                Id = vote.Id,
                ReviewId = vote.ReviewId,
                UserId = vote.UserId,
                IsHelpful = vote.IsHelpful,
                VotedAt = vote.CreatedAt,
                UserIpAddress = vote.UserIpAddress
            };
        }
    }
}