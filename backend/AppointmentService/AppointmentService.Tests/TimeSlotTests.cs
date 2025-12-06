using FluentAssertions;
using Xunit;
using AppointmentService.Domain.Entities;

namespace AppointmentService.Tests;

public class TimeSlotTests
{
    private readonly Guid _dealerId = Guid.NewGuid();

    [Fact]
    public void Constructor_WithValidData_ShouldCreateTimeSlot()
    {
        // Arrange
        var startTime = new TimeOnly(9, 0);
        var endTime = new TimeOnly(17, 0);

        // Act
        var timeSlot = new TimeSlot(
            _dealerId,
            DayOfWeek.Monday,
            startTime,
            endTime,
            60,
            2);

        // Assert
        timeSlot.Id.Should().NotBeEmpty();
        timeSlot.DealerId.Should().Be(_dealerId);
        timeSlot.DayOfWeek.Should().Be(DayOfWeek.Monday);
        timeSlot.StartTime.Should().Be(startTime);
        timeSlot.EndTime.Should().Be(endTime);
        timeSlot.SlotDurationMinutes.Should().Be(60);
        timeSlot.MaxConcurrentAppointments.Should().Be(2);
        timeSlot.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Constructor_WithEndTimeBeforeStartTime_ShouldThrowException()
    {
        // Arrange & Act
        var act = () => new TimeSlot(
            _dealerId,
            DayOfWeek.Monday,
            new TimeOnly(17, 0),
            new TimeOnly(9, 0),
            60,
            1);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*End time must be after start time*");
    }

    [Fact]
    public void Constructor_WithInvalidDuration_ShouldThrowException()
    {
        // Arrange & Act
        var act = () => new TimeSlot(
            _dealerId,
            DayOfWeek.Monday,
            new TimeOnly(9, 0),
            new TimeOnly(17, 0),
            0,
            1);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Slot duration must be positive*");
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateTimeSlot()
    {
        // Arrange
        var timeSlot = CreateValidTimeSlot();
        var newStartTime = new TimeOnly(8, 0);
        var newEndTime = new TimeOnly(18, 0);

        // Act
        timeSlot.Update(newStartTime, newEndTime, 30, 3);

        // Assert
        timeSlot.StartTime.Should().Be(newStartTime);
        timeSlot.EndTime.Should().Be(newEndTime);
        timeSlot.SlotDurationMinutes.Should().Be(30);
        timeSlot.MaxConcurrentAppointments.Should().Be(3);
        timeSlot.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var timeSlot = CreateValidTimeSlot();

        // Act
        timeSlot.Deactivate();

        // Assert
        timeSlot.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var timeSlot = CreateValidTimeSlot();
        timeSlot.Deactivate();

        // Act
        timeSlot.Activate();

        // Assert
        timeSlot.IsActive.Should().BeTrue();
    }

    private TimeSlot CreateValidTimeSlot() => new(
        _dealerId,
        DayOfWeek.Monday,
        new TimeOnly(9, 0),
        new TimeOnly(17, 0),
        60,
        2);
}
