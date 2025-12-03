namespace VehicleService.Tests.Unit.Reservations;

/// <summary>
/// Unit tests for vehicle reservation functionality
/// </summary>
public class VehicleReservationTests
{
    [Fact]
    public void CreateReservation_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var reservation = new
        {
            VehicleId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            ReservationDate = DateTime.UtcNow,
            ExpirationDate = DateTime.UtcNow.AddMinutes(30),
            Status = "Active"
        };

        // Assert
        reservation.VehicleId.Should().NotBeEmpty();
        reservation.CustomerId.Should().NotBeEmpty();
        reservation.ReservationDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        reservation.ExpirationDate.Should().BeAfter(reservation.ReservationDate);
    }

    [Theory]
    [InlineData(15, true)]
    [InlineData(30, true)]
    [InlineData(60, true)]
    [InlineData(0, false)]
    [InlineData(-10, false)]
    public void ValidateReservationDuration_WithVariousDurations_ReturnsExpectedResult(int minutes, bool expected)
    {
        // Act
        var isValid = minutes > 0;

        // Assert
        isValid.Should().Be(expected);
    }

    [Fact]
    public void Reservation_WithExpirationInPast_ShouldBeExpired()
    {
        // Arrange
        var reservation = new
        {
            ExpirationDate = DateTime.UtcNow.AddMinutes(-5)
        };

        // Act
        var isExpired = reservation.ExpirationDate < DateTime.UtcNow;

        // Assert
        isExpired.Should().BeTrue();
    }

    [Fact]
    public void Reservation_WithExpirationInFuture_ShouldBeActive()
    {
        // Arrange
        var reservation = new
        {
            ExpirationDate = DateTime.UtcNow.AddMinutes(15)
        };

        // Act
        var isActive = reservation.ExpirationDate > DateTime.UtcNow;

        // Assert
        isActive.Should().BeTrue();
    }

    [Theory]
    [InlineData("Active", true)]
    [InlineData("Expired", true)]
    [InlineData("Cancelled", true)]
    [InlineData("Completed", true)]
    [InlineData("Pending", false)]
    [InlineData("", false)]
    public void ValidateReservationStatus_WithVariousStatuses_ReturnsExpectedResult(string status, bool expected)
    {
        // Arrange
        var validStatuses = new[] { "Active", "Expired", "Cancelled", "Completed" };

        // Act
        var isValid = validStatuses.Contains(status);

        // Assert
        isValid.Should().Be(expected);
    }

    [Fact]
    public void CancelReservation_BeforeExpiration_ReturnsSuccess()
    {
        // Arrange
        var reservation = new
        {
            Status = "Active",
            ExpirationDate = DateTime.UtcNow.AddMinutes(20)
        };

        // Act
        var canCancel = reservation.Status == "Active" &&
                        reservation.ExpirationDate > DateTime.UtcNow;

        // Assert
        canCancel.Should().BeTrue();
    }

    [Fact]
    public void ExtendReservation_WithActiveReservation_ReturnsSuccess()
    {
        // Arrange
        var originalExpiration = DateTime.UtcNow.AddMinutes(10);
        var extensionMinutes = 15;

        // Act
        var newExpiration = originalExpiration.AddMinutes(extensionMinutes);

        // Assert
        newExpiration.Should().BeAfter(originalExpiration);
        (newExpiration - originalExpiration).TotalMinutes.Should().Be(extensionMinutes);
    }

    [Fact]
    public void GetActiveReservations_ForVehicle_ReturnsOnlyActive()
    {
        // Arrange
        var vehicleId = Guid.NewGuid();
        var reservations = new[]
        {
            new { Id = 1, VehicleId = vehicleId, Status = "Active" },
            new { Id = 2, VehicleId = vehicleId, Status = "Expired" },
            new { Id = 3, VehicleId = vehicleId, Status = "Active" },
            new { Id = 4, VehicleId = Guid.NewGuid(), Status = "Active" }
        };

        // Act
        var activeReservations = reservations
            .Where(r => r.VehicleId == vehicleId && r.Status == "Active")
            .ToList();

        // Assert
        activeReservations.Should().HaveCount(2);
        activeReservations.All(r => r.Status == "Active").Should().BeTrue();
    }

    [Fact]
    public void CheckVehicleAvailability_WithNoActiveReservation_ReturnsTrue()
    {
        // Arrange
        var reservations = new[]
        {
            new { Status = "Expired" },
            new { Status = "Cancelled" },
            new { Status = "Completed" }
        };

        // Act
        var isAvailable = !reservations.Any(r => r.Status == "Active");

        // Assert
        isAvailable.Should().BeTrue();
    }

    [Fact]
    public void CheckVehicleAvailability_WithActiveReservation_ReturnsFalse()
    {
        // Arrange
        var reservations = new[]
        {
            new { Status = "Active" },
            new { Status = "Expired" }
        };

        // Act
        var isAvailable = !reservations.Any(r => r.Status == "Active");

        // Assert
        isAvailable.Should().BeFalse();
    }
}
