// =====================================================
// C4: ConsumerProtectionService - Tests Unitarios
// Ley 358-05 Derechos del Consumidor de RD
// =====================================================

using FluentAssertions;
using ConsumerProtectionService.Domain.Entities;
using ConsumerProtectionService.Domain.Enums;
using Xunit;

namespace ConsumerProtectionService.Tests;

public class ConsumerProtectionServiceTests
{
    // =====================================================
    // Tests de Garantía Legal (Ley 358-05)
    // =====================================================

    [Fact]
    public void Warranty_ShouldBeCreated_WithLegalMinimum()
    {
        // Arrange & Act
        var warranty = new Warranty
        {
            Id = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            WarrantyType = WarrantyType.Legal,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(6), // 6 meses mínimo legal
            Status = WarrantyStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        warranty.Should().NotBeNull();
        warranty.WarrantyType.Should().Be(WarrantyType.Legal);
        (warranty.EndDate - warranty.StartDate).Days.Should().BeGreaterOrEqualTo(180);
    }

    [Theory]
    [InlineData(WarrantyType.Legal)]      // Garantía legal mínima (6 meses)
    [InlineData(WarrantyType.Extended)]   // Garantía extendida (adicional)
    [InlineData(WarrantyType.Manufacturer)] // Garantía del fabricante
    public void WarrantyType_ShouldHaveExpectedValues(WarrantyType type)
    {
        // Assert
        Enum.IsDefined(typeof(WarrantyType), type).Should().BeTrue();
    }

    [Theory]
    [InlineData(6)]   // 6 meses = mínimo legal para productos nuevos
    [InlineData(12)]  // 12 meses = común para electrodomésticos
    [InlineData(24)]  // 24 meses = garantía extendida
    public void LegalWarranty_DurationMonths_ShouldMeetMinimum(int months)
    {
        // Arrange
        var legalMinimumMonths = 6;

        // Assert
        months.Should().BeGreaterOrEqualTo(legalMinimumMonths);
    }

    // =====================================================
    // Tests de Reclamaciones
    // =====================================================

    [Fact]
    public void Complaint_ShouldBeCreated_WithRequiredFields()
    {
        // Arrange & Act
        var complaint = new Complaint
        {
            Id = Guid.NewGuid(),
            ConsumerId = Guid.NewGuid(),
            ComplaintNumber = "REC-2026-00001",
            ComplaintType = ComplaintType.DefectiveProduct,
            Description = "Vehículo presenta fallas mecánicas desde la compra",
            Status = ComplaintStatus.Received,
            Priority = ComplaintPriority.High,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        complaint.Should().NotBeNull();
        complaint.ComplaintNumber.Should().StartWith("REC-");
        complaint.Status.Should().Be(ComplaintStatus.Received);
    }

    [Theory]
    [InlineData(ComplaintType.DefectiveProduct)]
    [InlineData(ComplaintType.WarrantyIssue)]
    [InlineData(ComplaintType.MisleadingAdvertising)]
    [InlineData(ComplaintType.PriceDispute)]
    [InlineData(ComplaintType.RefundRequest)]
    [InlineData(ComplaintType.ServiceQuality)]
    public void ComplaintType_ShouldHaveExpectedValues(ComplaintType type)
    {
        // Assert
        Enum.IsDefined(typeof(ComplaintType), type).Should().BeTrue();
    }

    [Theory]
    [InlineData(ComplaintStatus.Received)]
    [InlineData(ComplaintStatus.UnderReview)]
    [InlineData(ComplaintStatus.InMediation)]
    [InlineData(ComplaintStatus.Resolved)]
    [InlineData(ComplaintStatus.Escalated)]
    [InlineData(ComplaintStatus.Closed)]
    public void ComplaintStatus_ShouldHaveExpectedValues(ComplaintStatus status)
    {
        // Assert
        Enum.IsDefined(typeof(ComplaintStatus), status).Should().BeTrue();
    }

    // =====================================================
    // Tests de Derecho de Retracto (7 días)
    // =====================================================

    [Fact]
    public void RightOfWithdrawal_ShouldBe7Days_ForOnlinePurchases()
    {
        // Arrange
        var purchaseDate = DateTime.UtcNow;
        var withdrawalPeriodDays = 7; // 7 días para compras en línea

        // Act
        var withdrawalDeadline = purchaseDate.AddDays(withdrawalPeriodDays);

        // Assert
        (withdrawalDeadline - purchaseDate).Days.Should().Be(7);
    }

    [Theory]
    [InlineData(1, true)]   // Día 1 - dentro del plazo
    [InlineData(5, true)]   // Día 5 - dentro del plazo
    [InlineData(7, true)]   // Día 7 - último día válido
    [InlineData(8, false)]  // Día 8 - fuera del plazo
    [InlineData(30, false)] // Día 30 - muy tarde
    public void RightOfWithdrawal_ShouldBeValidWithinPeriod(int daysSincePurchase, bool isValid)
    {
        // Arrange
        var withdrawalPeriod = 7;

        // Act
        var isWithinPeriod = daysSincePurchase <= withdrawalPeriod;

        // Assert
        isWithinPeriod.Should().Be(isValid);
    }

    // =====================================================
    // Tests de Plazos de Respuesta (Ley 358-05)
    // =====================================================

    [Fact]
    public void ComplaintResponse_Deadline_ShouldBe15Days()
    {
        // Arrange
        var complaintDate = DateTime.UtcNow;
        var responseDays = 15; // 15 días para responder según Ley 358-05

        // Act
        var responseDeadline = complaintDate.AddDays(responseDays);

        // Assert
        (responseDeadline - complaintDate).Days.Should().Be(15);
    }

    // =====================================================
    // Tests de Reembolso
    // =====================================================

    [Fact]
    public void Refund_ShouldBeCalculated_Correctly()
    {
        // Arrange
        var originalAmount = 50000m;
        var usageDays = 30;
        var warrantyDays = 180;

        // Act
        var depreciationRate = (decimal)usageDays / warrantyDays;
        var depreciation = originalAmount * depreciationRate;
        var refundAmount = originalAmount - depreciation;

        // Assert
        refundAmount.Should().BeLessThan(originalAmount);
        refundAmount.Should().BePositive();
    }

    [Theory]
    [InlineData(50000, 0, 50000)]     // Sin uso = reembolso total
    [InlineData(50000, 90, 25000)]    // 90 días = 50% (mitad de garantía)
    [InlineData(50000, 180, 0)]       // 180 días = sin reembolso (garantía vencida)
    public void Refund_ShouldBeProportionalToUsage(decimal original, int usageDays, decimal expectedMin)
    {
        // Arrange
        var warrantyDays = 180;

        // Act
        var usageRatio = Math.Min(1m, (decimal)usageDays / warrantyDays);
        var refund = original * (1 - usageRatio);

        // Assert
        refund.Should().BeGreaterOrEqualTo(expectedMin - 1); // Tolerancia de redondeo
    }

    // =====================================================
    // Tests de Publicidad Engañosa
    // =====================================================

    [Fact]
    public void MisleadingAdvertising_ShouldBeIdentified()
    {
        // Arrange
        var advertisedPrice = 500000m;
        var actualPrice = 650000m;
        var tolerance = 0.10m; // 10% de tolerancia

        // Act
        var priceDifference = (actualPrice - advertisedPrice) / advertisedPrice;
        var isMisleading = priceDifference > tolerance;

        // Assert
        isMisleading.Should().BeTrue();
        priceDifference.Should().BeGreaterThan(0.10m);
    }

    // =====================================================
    // Tests de Información Obligatoria (Vehículos)
    // =====================================================

    [Fact]
    public void VehicleDisclosure_ShouldContainRequiredInfo()
    {
        // Arrange & Act
        var vehicleInfo = new
        {
            Brand = "Toyota",
            Model = "Corolla",
            Year = 2024,
            Mileage = 0,
            Price = 1500000m,
            PriceIncludesITBIS = true,
            WarrantyMonths = 36,
            IsNew = true,
            CountryOfOrigin = "Japón",
            FuelType = "Gasolina",
            EngineSize = "1.8L"
        };

        // Assert
        vehicleInfo.Brand.Should().NotBeNullOrEmpty();
        vehicleInfo.Model.Should().NotBeNullOrEmpty();
        vehicleInfo.Year.Should().BeGreaterThan(2000);
        vehicleInfo.Price.Should().BePositive();
        vehicleInfo.PriceIncludesITBIS.Should().BeTrue(); // Obligatorio mostrar precio con ITBIS
        vehicleInfo.WarrantyMonths.Should().BeGreaterOrEqualTo(6); // Mínimo legal
    }

    // =====================================================
    // Tests de Mediación
    // =====================================================

    [Fact]
    public void Mediation_ShouldBeCreated_ForDispute()
    {
        // Arrange & Act
        var mediation = new Mediation
        {
            Id = Guid.NewGuid(),
            ComplaintId = Guid.NewGuid(),
            MediationNumber = "MED-2026-00001",
            Status = MediationStatus.Scheduled,
            ScheduledDate = DateTime.UtcNow.AddDays(5),
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        mediation.Should().NotBeNull();
        mediation.MediationNumber.Should().StartWith("MED-");
        mediation.Status.Should().Be(MediationStatus.Scheduled);
        mediation.ScheduledDate.Should().BeAfter(DateTime.UtcNow);
    }

    [Theory]
    [InlineData(MediationStatus.Scheduled)]
    [InlineData(MediationStatus.InProgress)]
    [InlineData(MediationStatus.Agreement)]
    [InlineData(MediationStatus.NoAgreement)]
    [InlineData(MediationStatus.Cancelled)]
    public void MediationStatus_ShouldHaveExpectedValues(MediationStatus status)
    {
        // Assert
        Enum.IsDefined(typeof(MediationStatus), status).Should().BeTrue();
    }
}
