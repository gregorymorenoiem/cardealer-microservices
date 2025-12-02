using SchedulerService.Domain.Entities;
using SchedulerService.Domain.Enums;
using Xunit;

namespace SchedulerService.Tests.Domain;

public class JobExecutionTests
{
    [Fact]
    public void JobExecution_ShouldBeCreated_WithValidData()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var scheduledAt = DateTime.UtcNow;

        // Act
        var execution = new JobExecution
        {
            JobId = jobId,
            ScheduledAt = scheduledAt,
            Status = ExecutionStatus.Scheduled
        };

        // Assert
        Assert.NotNull(execution);
        Assert.Equal(jobId, execution.JobId);
        Assert.Equal(scheduledAt, execution.ScheduledAt);
        Assert.Equal(ExecutionStatus.Scheduled, execution.Status);
    }

    [Fact]
    public void MarkAsRunning_ShouldUpdateStatusAndTimestamp()
    {
        // Arrange
        var execution = new JobExecution
        {
            JobId = Guid.NewGuid(),
            ScheduledAt = DateTime.UtcNow,
            Status = ExecutionStatus.Scheduled
        };
        var executedBy = "TestWorker";

        // Act
        execution.MarkAsRunning(executedBy);

        // Assert
        Assert.Equal(ExecutionStatus.Running, execution.Status);
        Assert.NotNull(execution.StartedAt);
        Assert.Equal(executedBy, execution.ExecutedBy);
    }

    [Fact]
    public void MarkAsSucceeded_ShouldCalculateDuration_WhenStartedAndCompleted()
    {
        // Arrange
        var execution = new JobExecution
        {
            JobId = Guid.NewGuid(),
            ScheduledAt = DateTime.UtcNow,
            Status = ExecutionStatus.Running,
            StartedAt = DateTime.UtcNow
        };
        var result = "Success";

        // Act
        execution.MarkAsSucceeded(result);

        // Assert
        Assert.Equal(ExecutionStatus.Succeeded, execution.Status);
        Assert.NotNull(execution.CompletedAt);
        Assert.Equal(result, execution.Result);
        Assert.NotNull(execution.DurationMs);
    }

    [Fact]
    public void MarkAsFailed_ShouldCalculateDuration_WhenStartedAndCompleted()
    {
        // Arrange
        var execution = new JobExecution
        {
            JobId = Guid.NewGuid(),
            ScheduledAt = DateTime.UtcNow,
            Status = ExecutionStatus.Running,
            StartedAt = DateTime.UtcNow
        };
        var errorMessage = "Test error";
        var stackTrace = "Stack trace details";

        // Act
        execution.MarkAsFailed(errorMessage, stackTrace);

        // Assert
        Assert.Equal(ExecutionStatus.Failed, execution.Status);
        Assert.NotNull(execution.CompletedAt);
        Assert.Equal(errorMessage, execution.ErrorMessage);
        Assert.Equal(stackTrace, execution.StackTrace);
        Assert.NotNull(execution.DurationMs);
    }

    [Fact]
    public void MarkAsCancelled_ShouldUpdateStatus()
    {
        // Arrange
        var execution = new JobExecution
        {
            JobId = Guid.NewGuid(),
            ScheduledAt = DateTime.UtcNow,
            Status = ExecutionStatus.Running,
            StartedAt = DateTime.UtcNow
        };

        // Act
        execution.MarkAsCancelled();

        // Assert
        Assert.Equal(ExecutionStatus.Cancelled, execution.Status);
        Assert.NotNull(execution.CompletedAt);
    }

    [Fact]
    public void MarkAsRetrying_ShouldUpdateStatusAndIncrementAttempt()
    {
        // Arrange
        var execution = new JobExecution
        {
            JobId = Guid.NewGuid(),
            ScheduledAt = DateTime.UtcNow,
            Status = ExecutionStatus.Failed,
            AttemptNumber = 1
        };

        // Act
        execution.MarkAsRetrying();

        // Assert
        Assert.Equal(ExecutionStatus.Retrying, execution.Status);
        Assert.Equal(2, execution.AttemptNumber);
    }

    [Fact]
    public void IsTerminalState_ShouldReturnTrue_WhenExecutionCompleted()
    {
        // Arrange
        var execution = new JobExecution
        {
            JobId = Guid.NewGuid(),
            ScheduledAt = DateTime.UtcNow,
            Status = ExecutionStatus.Succeeded
        };

        // Act
        var result = execution.IsTerminalState();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsTerminalState_ShouldReturnFalse_WhenExecutionInProgress()
    {
        // Arrange
        var execution = new JobExecution
        {
            JobId = Guid.NewGuid(),
            ScheduledAt = DateTime.UtcNow,
            Status = ExecutionStatus.Running
        };

        // Act
        var result = execution.IsTerminalState();

        // Assert
        Assert.False(result);
    }
}
