using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using UserService.Application.UseCases.DealerOnboarding;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;
using UserService.Shared.Exceptions;

namespace UserService.Tests.Application.UseCases.DealerOnboarding;

public class CompleteOnboardingStepCommandTests
{
    private readonly Mock<IDealerOnboardingRepository> _onboardingRepositoryMock;
    private readonly Mock<IDealerRepository> _dealerRepositoryMock;
    private readonly Mock<ILogger<CompleteOnboardingStepCommandHandler>> _loggerMock;
    private readonly CompleteOnboardingStepCommandHandler _handler;

    public CompleteOnboardingStepCommandTests()
    {
        _onboardingRepositoryMock = new Mock<IDealerOnboardingRepository>();
        _dealerRepositoryMock = new Mock<IDealerRepository>();
        _loggerMock = new Mock<ILogger<CompleteOnboardingStepCommandHandler>>();
        _handler = new CompleteOnboardingStepCommandHandler(
            _onboardingRepositoryMock.Object,
            _dealerRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCompleteStep_WhenValidStep()
    {
        // Arrange
        var dealerId = Guid.NewGuid();
        var onboarding = new DealerOnboardingProcess
        {
            Id = Guid.NewGuid(),
            DealerId = dealerId,
            Status = OnboardingStatus.InProgress,
            CurrentStep = DealerOnboardingStep.BasicInfo,
            StepsCompleted = "[]",
            StepsSkipped = "[]"
        };

        _onboardingRepositoryMock.Setup(r => r.GetByDealerIdAsync(dealerId))
            .ReturnsAsync(onboarding);

        var command = new CompleteOnboardingStepCommand(dealerId, "BasicInfo", null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        _onboardingRepositoryMock.Verify(r => r.UpdateAsync(It.Is<DealerOnboardingProcess>(
            o => o.StepsCompleted.Contains("BasicInfo"))), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenOnboardingNotFound()
    {
        // Arrange
        var dealerId = Guid.NewGuid();
        _onboardingRepositoryMock.Setup(r => r.GetByDealerIdAsync(dealerId))
            .ReturnsAsync((DealerOnboardingProcess?)null);

        var command = new CompleteOnboardingStepCommand(dealerId, "BasicInfo", null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequestException_WhenInvalidStep()
    {
        // Arrange
        var dealerId = Guid.NewGuid();
        var onboarding = new DealerOnboardingProcess
        {
            Id = Guid.NewGuid(),
            DealerId = dealerId,
            Status = OnboardingStatus.InProgress,
            CurrentStep = DealerOnboardingStep.BasicInfo,
            StepsCompleted = "[]",
            StepsSkipped = "[]"
        };

        _onboardingRepositoryMock.Setup(r => r.GetByDealerIdAsync(dealerId))
            .ReturnsAsync(onboarding);

        var command = new CompleteOnboardingStepCommand(dealerId, "InvalidStep", null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_ShouldUpdateDealerData_WhenDataProvided()
    {
        // Arrange
        var dealerId = Guid.NewGuid();
        var onboarding = new DealerOnboardingProcess
        {
            Id = Guid.NewGuid(),
            DealerId = dealerId,
            Status = OnboardingStatus.InProgress,
            CurrentStep = DealerOnboardingStep.BasicInfo,
            StepsCompleted = "[]",
            StepsSkipped = "[]"
        };

        var dealer = new Dealer { Id = dealerId, BusinessName = "Old Name" };

        _onboardingRepositoryMock.Setup(r => r.GetByDealerIdAsync(dealerId))
            .ReturnsAsync(onboarding);
        _dealerRepositoryMock.Setup(r => r.GetByIdAsync(dealerId))
            .ReturnsAsync(dealer);

        var data = new Dictionary<string, object>
        {
            { "BusinessName", "New Business Name" },
            { "Phone", "809-555-9999" }
        };

        var command = new CompleteOnboardingStepCommand(dealerId, "BasicInfo", data);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        _dealerRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Dealer>()), Times.Once);
    }
}
