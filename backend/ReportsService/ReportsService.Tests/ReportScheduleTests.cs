using FluentAssertions;
using Xunit;
using ReportsService.Domain.Entities;

namespace ReportsService.Tests;

public class ReportScheduleTests
{
    private readonly Guid _dealerId = Guid.NewGuid();
    private readonly Guid _reportId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public void Constructor_WithValidData_ShouldCreateSchedule()
    {
        // Arrange & Act
        var schedule = new ReportSchedule(
            _dealerId,
            _reportId,
            "Daily Sales Report",
            ScheduleFrequency.Daily,
            _userId);

        // Assert
        schedule.Id.Should().NotBeEmpty();
        schedule.DealerId.Should().Be(_dealerId);
        schedule.ReportId.Should().Be(_reportId);
        schedule.Name.Should().Be("Daily Sales Report");
        schedule.Frequency.Should().Be(ScheduleFrequency.Daily);
        schedule.IsActive.Should().BeTrue();
        schedule.SendEmail.Should().BeTrue();
        schedule.SaveToStorage.Should().BeTrue();
        schedule.CreatedBy.Should().Be(_userId);
    }

    [Fact]
    public void Constructor_WithEmptyName_ShouldThrowException()
    {
        // Arrange & Act
        var act = () => new ReportSchedule(_dealerId, _reportId, "", ScheduleFrequency.Daily, _userId);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Name is required*");
    }

    [Fact]
    public void SetExecutionTime_ShouldSetTime()
    {
        // Arrange
        var schedule = new ReportSchedule(_dealerId, _reportId, "Test Schedule", ScheduleFrequency.Daily, _userId);
        var time = new TimeOnly(9, 0);

        // Act
        schedule.SetExecutionTime(time);

        // Assert
        schedule.ExecutionTime.Should().Be(time);
    }

    [Fact]
    public void SetWeeklySchedule_ShouldSetDayOfWeek()
    {
        // Arrange
        var schedule = new ReportSchedule(_dealerId, _reportId, "Weekly Report", ScheduleFrequency.Weekly, _userId);

        // Act
        schedule.SetWeeklySchedule(DayOfWeek.Monday);

        // Assert
        schedule.DayOfWeek.Should().Be(DayOfWeek.Monday);
    }

    [Fact]
    public void SetMonthlySchedule_WithValidDay_ShouldSetDayOfMonth()
    {
        // Arrange
        var schedule = new ReportSchedule(_dealerId, _reportId, "Monthly Report", ScheduleFrequency.Monthly, _userId);

        // Act
        schedule.SetMonthlySchedule(15);

        // Assert
        schedule.DayOfMonth.Should().Be(15);
    }

    [Fact]
    public void SetMonthlySchedule_WithInvalidDay_ShouldThrowException()
    {
        // Arrange
        var schedule = new ReportSchedule(_dealerId, _reportId, "Monthly Report", ScheduleFrequency.Monthly, _userId);

        // Act
        var act = () => schedule.SetMonthlySchedule(32);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Day of month must be between 1 and 31*");
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalseAndClearNextRun()
    {
        // Arrange
        var schedule = new ReportSchedule(_dealerId, _reportId, "Test Schedule", ScheduleFrequency.Daily, _userId);

        // Act
        schedule.Deactivate();

        // Assert
        schedule.IsActive.Should().BeFalse();
        schedule.NextRunAt.Should().BeNull();
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var schedule = new ReportSchedule(_dealerId, _reportId, "Test Schedule", ScheduleFrequency.Daily, _userId);
        schedule.Deactivate();

        // Act
        schedule.Activate();

        // Assert
        schedule.IsActive.Should().BeTrue();
    }

    [Fact]
    public void SetDeliveryOptions_ShouldUpdateDeliverySettings()
    {
        // Arrange
        var schedule = new ReportSchedule(_dealerId, _reportId, "Test Schedule", ScheduleFrequency.Daily, _userId);

        // Act
        schedule.SetDeliveryOptions(sendEmail: false, saveToStorage: true);

        // Assert
        schedule.SendEmail.Should().BeFalse();
        schedule.SaveToStorage.Should().BeTrue();
    }
}

