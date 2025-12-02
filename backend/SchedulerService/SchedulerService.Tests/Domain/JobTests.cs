using SchedulerService.Domain.Entities;
using SchedulerService.Domain.Enums;
using Xunit;

namespace SchedulerService.Tests.Domain;

public class JobTests
{
    [Fact]
    public void Job_ShouldBeCreated_WithValidData()
    {
        // Arrange
        var name = "TestJob";
        var cronExpression = "0 0 * * *";
        var jobType = "TestJobType";

        // Act
        var job = new Job
        {
            Name = name,
            CronExpression = cronExpression,
            JobType = jobType
        };

        // Assert
        Assert.NotNull(job);
        Assert.Equal(name, job.Name);
        Assert.Equal(cronExpression, job.CronExpression);
        Assert.Equal(jobType, job.JobType);
        Assert.Equal(JobStatus.Enabled, job.Status);
    }

    [Fact]
    public void Enable_ShouldChangeStatus_ToEnabled()
    {
        // Arrange
        var job = new Job
        {
            Name = "TestJob",
            CronExpression = "0 0 * * *",
            JobType = "TestJobType",
            Status = JobStatus.Disabled
        };

        // Act
        job.Enable();

        // Assert
        Assert.Equal(JobStatus.Enabled, job.Status);
    }

    [Fact]
    public void Disable_ShouldChangeStatus_ToDisabled()
    {
        // Arrange
        var job = new Job
        {
            Name = "TestJob",
            CronExpression = "0 0 * * *",
            JobType = "TestJobType",
            Status = JobStatus.Enabled
        };

        // Act
        job.Disable();

        // Assert
        Assert.Equal(JobStatus.Disabled, job.Status);
    }

    [Fact]
    public void Pause_ShouldChangeStatus_ToPaused()
    {
        // Arrange
        var job = new Job
        {
            Name = "TestJob",
            CronExpression = "0 0 * * *",
            JobType = "TestJobType",
            Status = JobStatus.Enabled
        };

        // Act
        job.Pause();

        // Assert
        Assert.Equal(JobStatus.Paused, job.Status);
    }

    [Fact]
    public void IsExecutable_ShouldReturnTrue_WhenJobIsEnabled()
    {
        // Arrange
        var job = new Job
        {
            Name = "TestJob",
            CronExpression = "0 0 * * *",
            JobType = "TestJobType",
            Status = JobStatus.Enabled
        };

        // Act
        var result = job.IsExecutable();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsExecutable_ShouldReturnFalse_WhenJobIsDisabled()
    {
        // Arrange
        var job = new Job
        {
            Name = "TestJob",
            CronExpression = "0 0 * * *",
            JobType = "TestJobType",
            Status = JobStatus.Disabled
        };

        // Act
        var result = job.IsExecutable();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void UpdateLastExecution_ShouldUpdateTimestamp()
    {
        // Arrange
        var job = new Job
        {
            Name = "TestJob",
            CronExpression = "0 0 * * *",
            JobType = "TestJobType"
        };
        var executionTime = DateTime.UtcNow;

        // Act
        job.UpdateLastExecution(executionTime);

        // Assert
        Assert.Equal(executionTime, job.LastExecutionAt);
    }

    [Fact]
    public void UpdateNextExecution_ShouldUpdateTimestamp()
    {
        // Arrange
        var job = new Job
        {
            Name = "TestJob",
            CronExpression = "0 0 * * *",
            JobType = "TestJobType"
        };
        var nextExecution = DateTime.UtcNow.AddDays(1);

        // Act
        job.UpdateNextExecution(nextExecution);

        // Assert
        Assert.Equal(nextExecution, job.NextExecutionAt);
    }
}
