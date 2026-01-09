using Xunit;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ReviewService.Application.Features.HelpfulVotes.Commands;
using ReviewService.Domain.Entities;
using ReviewService.Domain.Interfaces;

namespace ReviewService.Tests.Application.Features.HelpfulVotes;

/// <summary>
/// Tests para VoteReviewHelpfulCommandHandler
/// </summary>
public class VoteReviewHelpfulCommandHandlerTests
{
    private readonly Mock<IReviewHelpfulVoteRepository> _voteRepositoryMock;
    private readonly Mock<IReviewRepository> _reviewRepositoryMock;
    private readonly Mock<ILogger<VoteReviewHelpfulCommandHandler>> _loggerMock;
    private readonly VoteReviewHelpfulCommandHandler _handler;

    public VoteReviewHelpfulCommandHandlerTests()
    {
        _voteRepositoryMock = new Mock<IReviewHelpfulVoteRepository>();
        _reviewRepositoryMock = new Mock<IReviewRepository>();
        _loggerMock = new Mock<ILogger<VoteReviewHelpfulCommandHandler>>();
        _handler = new VoteReviewHelpfulCommandHandler(_voteRepositoryMock.Object, _reviewRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateNewVote_WhenUserHasNotVotedBefore()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var review = CreateSampleReview(reviewId);
        var command = new VoteReviewHelpfulCommand(reviewId, userId, true, "192.168.1.1");

        _reviewRepositoryMock.Setup(x => x.GetByIdAsync(reviewId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(review);
        _voteRepositoryMock.Setup(x => x.GetByReviewAndUserAsync(reviewId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ReviewHelpfulVote?)null);
        _voteRepositoryMock.Setup(x => x.AddAsync(It.IsAny<ReviewHelpfulVote>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ReviewId.Should().Be(reviewId);
        result.UserId.Should().Be(userId);
        result.IsHelpful.Should().BeTrue();
        result.UserIpAddress.Should().Be("192.168.1.1");

        _voteRepositoryMock.Verify(x => x.AddAsync(It.Is<ReviewHelpfulVote>(v =>
            v.ReviewId == reviewId &&
            v.UserId == userId &&
            v.IsHelpful == true &&
            v.UserIpAddress == "192.168.1.1"
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUpdateExistingVote_WhenUserHasVotedBefore()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var existingVoteId = Guid.NewGuid();
        var review = CreateSampleReview(reviewId);
        var existingVote = new ReviewHelpfulVote
        {
            Id = existingVoteId,
            ReviewId = reviewId,
            UserId = userId,
            IsHelpful = false,
            VotedAt = DateTime.UtcNow.AddDays(-1),
            UserIpAddress = "192.168.1.1"
        };
        var command = new VoteReviewHelpfulCommand(reviewId, userId, true, "192.168.1.1");

        _reviewRepositoryMock.Setup(x => x.GetByIdAsync(reviewId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(review);
        _voteRepositoryMock.Setup(x => x.GetByReviewAndUserAsync(reviewId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingVote);
        _voteRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<ReviewHelpfulVote>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(existingVoteId);
        result.IsHelpful.Should().BeTrue();
        existingVote.IsHelpful.Should().BeTrue(); // Should be updated

        _voteRepositoryMock.Verify(x => x.UpdateAsync(existingVote, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenReviewNotFound()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new VoteReviewHelpfulCommand(reviewId, userId, true);

        _reviewRepositoryMock.Setup(x => x.GetByIdAsync(reviewId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Review?)null);

        // Act & Assert
        await _handler.Invoking(h => h.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Review with ID {reviewId} not found");
    }

    [Fact]
    public async Task Handle_ShouldUseUnknownIpAddress_WhenIpNotProvided()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var review = CreateSampleReview(reviewId);
        var command = new VoteReviewHelpfulCommand(reviewId, userId, true);

        _reviewRepositoryMock.Setup(x => x.GetByIdAsync(reviewId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(review);
        _voteRepositoryMock.Setup(x => x.GetByReviewAndUserAsync(reviewId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ReviewHelpfulVote?)null);
        _voteRepositoryMock.Setup(x => x.AddAsync(It.IsAny<ReviewHelpfulVote>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.UserIpAddress.Should().Be("Unknown");

        _voteRepositoryMock.Verify(x => x.AddAsync(It.Is<ReviewHelpfulVote>(v =>
            v.UserIpAddress == "Unknown"
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    private static Review CreateSampleReview(Guid reviewId)
    {
        return new Review
        {
            Id = reviewId,
            BuyerId = Guid.NewGuid(),
            SellerId = Guid.NewGuid(),
            Rating = 5,
            Title = "Great service",
            Content = "Very satisfied with the purchase",
            IsApproved = true,
            IsVerifiedPurchase = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}