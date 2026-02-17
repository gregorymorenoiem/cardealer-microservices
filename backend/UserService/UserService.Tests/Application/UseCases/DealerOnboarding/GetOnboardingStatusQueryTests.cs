using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using UserService.Application.UseCases.DealerOnboarding;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;
using UserService.Shared.Exceptions;

namespace UserService.Tests.Application.UseCases.DealerOnboarding;

public class GetOnboardingStatusQueryTests
{
    private readonly Mock<IDealerOnboardingRepository> _onboardingRepositoryMock;
    private readonly Mock<ILogger<GetOnboardingStatusQueryHandler>> _loggerMock;
    private readonly GetOnboardingStatusQueryHandler _handler;

    public GetOnboardingStatusQueryTests()
    {
        _onboardingRepositoryMock = new Mock<IDealerOnboardingRepository>();
        _loggerMock = new Mock<ILogger<GetOnboardingStatusQueryHandler>>();
        _handler = new GetOnboardingStatusQueryHandler(
            _onboardingRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnOnboardingStatus_WhenExists()
    {
        // Arrange
        var dealerId = Guid.NewGuid();
        var onboarding = new DealerOnboardingProcess
        {
            Id = Guid.NewGuid(),
            DealerId = dealerId,
            Status = OnboardingStatus.InProgress,
            CurrentStep = DealerOnboardingStep.Documents,
            StartedAt = DateTime.UtcNow.AddDays(-5),
            StepsCompleted = "[\"BasicInfo\"]",
            StepsSkipped = "[]"
        };

        _onboardingRepositoryMock.Setup(r => r.GetByDealerIdAsync(dealerId))
            .ReturnsAsync(onboarding);

        var query = new GetOnboardingStatusQuery(dealerId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.DealerId.Should().Be(dealerId);
        result.Status.Should().Be("InProgress");
        result.CurrentStep.Should().Be("Documents");
        result.StepsCompleted.Should().Contain("BasicInfo");
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenOnboardingNotFound()
    {
        // Arrange
        var dealerId = Guid.NewGuid();
        _onboardingRepositoryMock.Setup(r => r.GetByDealerIdAsync(dealerId))
            .ReturnsAsync((DealerOnboardingProcess?)null);

        var query = new GetOnboardingStatusQuery(dealerId);

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
