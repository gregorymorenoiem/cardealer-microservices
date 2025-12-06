using FluentAssertions;
using Xunit;
using AppointmentService.Domain.Entities;

namespace AppointmentService.Tests;

public class AppointmentTests
{
    private readonly Guid _dealerId = Guid.NewGuid();
    private readonly Guid _customerId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public void Constructor_WithValidData_ShouldCreateAppointment()
    {
        // Arrange
        var scheduledDate = DateTime.UtcNow.Date.AddDays(1);
        var startTime = new TimeOnly(10, 0);

        // Act
        var appointment = new Appointment(
            _dealerId,
            _customerId,
            "John Doe",
            "john@example.com",
            AppointmentType.TestDrive,
            scheduledDate,
            startTime,
            60,
            _userId,
            "555-1234");

        // Assert
        appointment.Id.Should().NotBeEmpty();
        appointment.DealerId.Should().Be(_dealerId);
        appointment.CustomerId.Should().Be(_customerId);
        appointment.CustomerName.Should().Be("John Doe");
        appointment.CustomerEmail.Should().Be("john@example.com");
        appointment.CustomerPhone.Should().Be("555-1234");
        appointment.Type.Should().Be(AppointmentType.TestDrive);
        appointment.Status.Should().Be(AppointmentStatus.Scheduled);
        appointment.ScheduledDate.Should().Be(scheduledDate);
        appointment.StartTime.Should().Be(startTime);
        appointment.EndTime.Should().Be(new TimeOnly(11, 0));
        appointment.DurationMinutes.Should().Be(60);
        appointment.CreatedBy.Should().Be(_userId);
    }

    [Fact]
    public void Constructor_WithEmptyCustomerName_ShouldThrowException()
    {
        // Arrange & Act
        var act = () => new Appointment(
            _dealerId,
            _customerId,
            "",
            "john@example.com",
            AppointmentType.TestDrive,
            DateTime.UtcNow.Date,
            new TimeOnly(10, 0),
            60,
            _userId);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Customer name is required*");
    }

    [Fact]
    public void Constructor_WithInvalidDuration_ShouldThrowException()
    {
        // Arrange & Act
        var act = () => new Appointment(
            _dealerId,
            _customerId,
            "John Doe",
            "john@example.com",
            AppointmentType.TestDrive,
            DateTime.UtcNow.Date,
            new TimeOnly(10, 0),
            0,
            _userId);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Duration must be positive*");
    }

    [Fact]
    public void Confirm_ShouldSetStatusToConfirmed()
    {
        // Arrange
        var appointment = CreateValidAppointment();

        // Act
        appointment.Confirm();

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Confirmed);
        appointment.ConfirmedAt.Should().NotBeNull();
    }

    [Fact]
    public void Complete_ShouldSetStatusToCompleted()
    {
        // Arrange
        var appointment = CreateValidAppointment();
        appointment.Confirm();
        appointment.Start();

        // Act
        appointment.Complete();

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Completed);
        appointment.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public void Cancel_ShouldSetStatusToCancelledWithReason()
    {
        // Arrange
        var appointment = CreateValidAppointment();

        // Act
        appointment.Cancel("Customer requested cancellation");

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Cancelled);
        appointment.CancellationReason.Should().Be("Customer requested cancellation");
        appointment.CancelledAt.Should().NotBeNull();
    }

    [Fact]
    public void Reschedule_ShouldUpdateDateTimeAndStatus()
    {
        // Arrange
        var appointment = CreateValidAppointment();
        var newDate = DateTime.UtcNow.Date.AddDays(5);
        var newTime = new TimeOnly(14, 0);

        // Act
        appointment.Reschedule(newDate, newTime, 90);

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Rescheduled);
        appointment.ScheduledDate.Should().Be(newDate);
        appointment.StartTime.Should().Be(newTime);
        appointment.DurationMinutes.Should().Be(90);
        appointment.EndTime.Should().Be(new TimeOnly(15, 30));
    }

    [Fact]
    public void SetVehicle_ShouldSetVehicleInfo()
    {
        // Arrange
        var appointment = CreateValidAppointment();
        var vehicleId = Guid.NewGuid();

        // Act
        appointment.SetVehicle(vehicleId, "2024 Honda Civic");

        // Assert
        appointment.VehicleId.Should().Be(vehicleId);
        appointment.VehicleDescription.Should().Be("2024 Honda Civic");
    }

    [Fact]
    public void AssignTo_ShouldSetAssignedUser()
    {
        // Arrange
        var appointment = CreateValidAppointment();
        var userId = Guid.NewGuid();

        // Act
        appointment.AssignTo(userId, "Sales Rep");

        // Assert
        appointment.AssignedToUserId.Should().Be(userId);
        appointment.AssignedToUserName.Should().Be("Sales Rep");
    }

    private Appointment CreateValidAppointment() => new(
        _dealerId,
        _customerId,
        "John Doe",
        "john@example.com",
        AppointmentType.TestDrive,
        DateTime.UtcNow.Date.AddDays(1),
        new TimeOnly(10, 0),
        60,
        _userId);
}
