using MessageBusService.Domain.Entities;
using MessageBusService.Domain.Enums;
using Xunit;

namespace MessageBusService.Tests.Entities;

public class SagaTests
{
    [Fact]
    public void AddStep_ShouldSetCorrectOrder()
    {
        // Arrange
        var saga = new Saga { Id = Guid.NewGuid() };
        var step1 = new SagaStep { Id = Guid.NewGuid(), Name = "Step1" };
        var step2 = new SagaStep { Id = Guid.NewGuid(), Name = "Step2" };

        // Act
        saga.AddStep(step1);
        saga.AddStep(step2);

        // Assert
        Assert.Equal(0, step1.Order);
        Assert.Equal(1, step2.Order);
        Assert.Equal(2, saga.TotalSteps);
    }

    [Fact]
    public void Start_ShouldSetRunningStatusAndStartedAt()
    {
        // Arrange
        var saga = new Saga { Status = SagaStatus.Created };

        // Act
        saga.Start();

        // Assert
        Assert.Equal(SagaStatus.Running, saga.Status);
        Assert.NotNull(saga.StartedAt);
    }

    [Fact]
    public void Complete_ShouldSetCompletedStatusAndTime()
    {
        // Arrange
        var saga = new Saga { Status = SagaStatus.Running };

        // Act
        saga.Complete();

        // Assert
        Assert.Equal(SagaStatus.Completed, saga.Status);
        Assert.NotNull(saga.CompletedAt);
    }

    [Fact]
    public void Fail_ShouldSetCompensatingStatusAndErrorMessage()
    {
        // Arrange
        var saga = new Saga { Status = SagaStatus.Running };
        var errorMessage = "Test error";

        // Act
        saga.Fail(errorMessage);

        // Assert
        Assert.Equal(SagaStatus.Compensating, saga.Status);
        Assert.Equal(errorMessage, saga.ErrorMessage);
        Assert.NotNull(saga.FailedAt);
    }

    [Fact]
    public void Compensate_ShouldSetCompensatedStatus()
    {
        // Arrange
        var saga = new Saga { Status = SagaStatus.Compensating };

        // Act
        saga.Compensate();

        // Assert
        Assert.Equal(SagaStatus.Compensated, saga.Status);
    }

    [Fact]
    public void HasTimedOut_ShouldReturnTrue_WhenTimeoutExceeded()
    {
        // Arrange
        var saga = new Saga
        {
            StartedAt = DateTime.UtcNow.AddMinutes(-10),
            Timeout = TimeSpan.FromMinutes(5)
        };

        // Act
        var result = saga.HasTimedOut();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasTimedOut_ShouldReturnFalse_WhenWithinTimeout()
    {
        // Arrange
        var saga = new Saga
        {
            StartedAt = DateTime.UtcNow.AddMinutes(-2),
            Timeout = TimeSpan.FromMinutes(5)
        };

        // Act
        var result = saga.HasTimedOut();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetNextStep_ShouldReturnFirstPendingStep()
    {
        // Arrange
        var saga = new Saga();
        saga.AddStep(new SagaStep { Name = "Step1", Status = SagaStepStatus.Completed });
        saga.AddStep(new SagaStep { Name = "Step2", Status = SagaStepStatus.Pending });
        saga.AddStep(new SagaStep { Name = "Step3", Status = SagaStepStatus.Pending });

        // Act
        var nextStep = saga.GetNextStep();

        // Assert
        Assert.NotNull(nextStep);
        Assert.Equal("Step2", nextStep.Name);
        Assert.Equal(1, nextStep.Order);
    }

    [Fact]
    public void GetNextStep_ShouldReturnNull_WhenNoMorePendingSteps()
    {
        // Arrange
        var saga = new Saga();
        saga.AddStep(new SagaStep { Name = "Step1", Status = SagaStepStatus.Completed });
        saga.AddStep(new SagaStep { Name = "Step2", Status = SagaStepStatus.Completed });

        // Act
        var nextStep = saga.GetNextStep();

        // Assert
        Assert.Null(nextStep);
    }

    [Fact]
    public void GetStepsToCompensate_ShouldReturnCompletedStepsInReverseOrder()
    {
        // Arrange
        var saga = new Saga();
        saga.AddStep(new SagaStep { Name = "Step1", Status = SagaStepStatus.Completed });
        saga.AddStep(new SagaStep { Name = "Step2", Status = SagaStepStatus.Completed });
        saga.AddStep(new SagaStep { Name = "Step3", Status = SagaStepStatus.Failed });

        // Act
        var stepsToCompensate = saga.GetStepsToCompensate();

        // Assert
        Assert.Equal(2, stepsToCompensate.Count);
        Assert.Equal("Step2", stepsToCompensate[0].Name);
        Assert.Equal("Step1", stepsToCompensate[1].Name);
    }
}
