using Xunit;

using FluentAssertions;
using ReviewService.Domain.Entities;

namespace ReviewService.Tests.Domain.Entities;

/// <summary>
/// Tests para ReviewHelpfulVote entity
/// </summary>
public class ReviewHelpfulVoteTests
{
    [Fact]
    public void ReviewHelpfulVote_ShouldBeCreatedSuccessfully_WithValidData()
    {
        // Arrange & Act
        var vote = new ReviewHelpfulVote
        {
            Id = Guid.NewGuid(),
            ReviewId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            IsHelpful = true,
            VotedAt = DateTime.UtcNow,
            UserIpAddress = "192.168.1.1"
        };

        // Assert
        vote.Id.Should().NotBe(Guid.Empty);
        vote.ReviewId.Should().NotBe(Guid.Empty);
        vote.UserId.Should().NotBe(Guid.Empty);
        vote.IsHelpful.Should().BeTrue();
        vote.VotedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        vote.UserIpAddress.Should().Be("192.168.1.1");
    }

    [Fact]
    public void ReviewHelpfulVote_ShouldAllowNotHelpfulVote()
    {
        // Arrange & Act
        var vote = new ReviewHelpfulVote
        {
            Id = Guid.NewGuid(),
            ReviewId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            IsHelpful = false,
            VotedAt = DateTime.UtcNow,
            UserIpAddress = "10.0.0.1"
        };

        // Assert
        vote.IsHelpful.Should().BeFalse();
    }

    [Theory]
    [InlineData("192.168.1.1")]
    [InlineData("10.0.0.1")]
    [InlineData("172.16.0.1")]
    [InlineData("2001:db8::1")]
    [InlineData("Unknown")]
    public void ReviewHelpfulVote_ShouldAcceptValidIpAddresses(string ipAddress)
    {
        // Arrange & Act
        var vote = new ReviewHelpfulVote
        {
            Id = Guid.NewGuid(),
            ReviewId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            IsHelpful = true,
            VotedAt = DateTime.UtcNow,
            UserIpAddress = ipAddress
        };

        // Assert
        vote.UserIpAddress.Should().Be(ipAddress);
    }
}