using MessageBusService.Domain.Entities;
using MessageBusService.Domain.Enums;
using Xunit;

namespace MessageBusService.Tests.Entities;

public class SagaStepTests
{
    [Fact]
    public void Start_ShouldSetRunningStatusAndStartedAt()
    {
        // Arrange
        var step = new SagaStep { Status = SagaStepStatus.Pending };

        // Act
        step.Start();

        // Assert
        Assert.Equal(SagaStepStatus.Running, step.Status);
        Assert.NotNull(step.StartedAt);
    }

    [Fact]
    public void Complete_ShouldSetCompletedStatusAndTime()
    {
        // Arrange
        var step = new SagaStep { Status = SagaStepStatus.Running };
        var responsePayload = "{\"result\":\"success\"}";

        // Act
        step.Complete(responsePayload);

        // Assert
        Assert.Equal(SagaStepStatus.Completed, step.Status);
        Assert.NotNull(step.CompletedAt);
        Assert.Equal(responsePayload, step.ResponsePayload);
    }

    [Fact]
    public void Fail_ShouldSetFailedStatusAndErrorMessage()
    {
        // Arrange
        var step = new SagaStep { Status = SagaStepStatus.Running };
        var errorMessage = "Test error";

        // Act
        step.Fail(errorMessage);

        // Assert
        Assert.Equal(SagaStepStatus.Failed, step.Status);
        Assert.Equal(errorMessage, step.ErrorMessage);
        Assert.NotNull(step.FailedAt);
    }

    [Fact]
    public void StartCompensation_ShouldSetCompensatingStatus()
    {
        // Arrange
        var step = new SagaStep { Status = SagaStepStatus.Completed };

        // Act
        step.StartCompensation();

        // Assert
        Assert.Equal(SagaStepStatus.Compensating, step.Status);
        Assert.NotNull(step.CompensationStartedAt);
    }

    [Fact]
    public void CompleteCompensation_ShouldSetCompensatedStatus()
    {
        // Arrange
        var step = new SagaStep { Status = SagaStepStatus.Compensating };

        // Act
        step.CompleteCompensation();

        // Assert
        Assert.Equal(SagaStepStatus.Compensated, step.Status);
        Assert.NotNull(step.CompensationCompletedAt);
    }

    [Fact]
    public void FailCompensation_ShouldSetCompensationFailedStatus()
    {
        // Arrange
        var step = new SagaStep { Status = SagaStepStatus.Compensating };
        var errorMessage = "Compensation failed";

        // Act
        step.FailCompensation(errorMessage);

        // Assert
        Assert.Equal(SagaStepStatus.CompensationFailed, step.Status);
        Assert.Equal(errorMessage, step.ErrorMessage);
    }

    [Fact]
    public void HasTimedOut_ShouldReturnTrue_WhenTimeoutExceeded()
    {
        // Arrange
        var step = new SagaStep
        {
            StartedAt = DateTime.UtcNow.AddSeconds(-30),
            Timeout = TimeSpan.FromSeconds(10)
        };

        // Act
        var result = step.HasTimedOut();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasTimedOut_ShouldReturnFalse_WhenWithinTimeout()
    {
        // Arrange
        var step = new SagaStep
        {
            StartedAt = DateTime.UtcNow.AddSeconds(-5),
            Timeout = TimeSpan.FromSeconds(10)
        };

        // Act
        var result = step.HasTimedOut();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CanRetry_ShouldReturnTrue_WhenBelowMaxRetries()
    {
        // Arrange
        var step = new SagaStep
        {
            RetryAttempts = 1,
            MaxRetries = 3
        };

        // Act
        var result = step.CanRetry();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanRetry_ShouldReturnFalse_WhenMaxRetriesReached()
    {
        // Arrange
        var step = new SagaStep
        {
            RetryAttempts = 3,
            MaxRetries = 3
        };

        // Act
        var result = step.CanRetry();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IncrementRetry_ShouldIncreaseRetryCount()
    {
        // Arrange
        var step = new SagaStep { RetryAttempts = 1 };

        // Act
        step.IncrementRetry();

        // Assert
        Assert.Equal(2, step.RetryAttempts);
    }

    [Fact]
    public void HasCompensation_ShouldReturnTrue_WhenCompensationActionDefined()
    {
        // Arrange
        var step = new SagaStep
        {
            CompensationActionType = "rollback"
        };

        // Act
        var result = step.HasCompensation();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasCompensation_ShouldReturnFalse_WhenNoCompensationAction()
    {
        // Arrange
        var step = new SagaStep
        {
            CompensationActionType = null
        };

        // Act
        var result = step.HasCompensation();

        // Assert
        Assert.False(result);
    }
}
