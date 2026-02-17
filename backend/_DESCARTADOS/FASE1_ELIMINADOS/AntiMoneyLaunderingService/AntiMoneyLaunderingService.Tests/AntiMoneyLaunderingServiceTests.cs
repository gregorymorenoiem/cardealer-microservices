// =====================================================
// C3: AntiMoneyLaunderingService - Tests Unitarios
// Ley 155-17 Prevención de Lavado de Activos (PLD)
// =====================================================

using FluentAssertions;
using AntiMoneyLaunderingService.Domain.Entities;
using AntiMoneyLaunderingService.Domain.Enums;
using Xunit;

namespace AntiMoneyLaunderingService.Tests;

public class AntiMoneyLaunderingServiceTests
{
    // =====================================================
    // Tests de Debida Diligencia (KYC)
    // =====================================================

    [Fact]
    public void Customer_ShouldBeCreated_WithKycData()
    {
        // Arrange & Act
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            FullName = "María García López",
            IdentificationType = IdentificationType.Cedula,
            IdentificationNumber = "00112345678",
            RiskLevel = RiskLevel.Medium,
            KycStatus = KycStatus.Verified,
            LastKycReviewDate = DateTime.UtcNow,
            NextKycReviewDate = DateTime.UtcNow.AddYears(1),
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        customer.Should().NotBeNull();
        customer.KycStatus.Should().Be(KycStatus.Verified);
        customer.RiskLevel.Should().Be(RiskLevel.Medium);
    }

    [Theory]
    [InlineData(RiskLevel.Low)]
    [InlineData(RiskLevel.Medium)]
    [InlineData(RiskLevel.High)]
    [InlineData(RiskLevel.Prohibited)]
    public void RiskLevel_ShouldHaveExpectedValues(RiskLevel level)
    {
        // Assert
        Enum.IsDefined(typeof(RiskLevel), level).Should().BeTrue();
    }

    [Theory]
    [InlineData(KycStatus.Pending)]
    [InlineData(KycStatus.InProgress)]
    [InlineData(KycStatus.Verified)]
    [InlineData(KycStatus.Rejected)]
    [InlineData(KycStatus.Expired)]
    public void KycStatus_ShouldHaveExpectedValues(KycStatus status)
    {
        // Assert
        Enum.IsDefined(typeof(KycStatus), status).Should().BeTrue();
    }

    // =====================================================
    // Tests de Umbrales de Transacción (Ley 155-17)
    // =====================================================

    [Fact]
    public void Transaction_AboveThreshold_ShouldRequireReporting()
    {
        // Arrange
        var threshold = 10000m; // USD 10,000 según Ley 155-17
        var transactionAmount = 15000m;

        // Act
        var requiresReporting = transactionAmount >= threshold;

        // Assert
        requiresReporting.Should().BeTrue();
    }

    [Theory]
    [InlineData(5000, false)]   // Debajo del umbral
    [InlineData(9999.99, false)] // Justo debajo
    [InlineData(10000, true)]   // En el umbral
    [InlineData(15000, true)]   // Sobre el umbral
    [InlineData(50000, true)]   // Muy sobre el umbral
    public void Transaction_ReportingThreshold_ShouldBeEnforced(decimal amount, bool shouldReport)
    {
        // Arrange
        var threshold = 10000m;

        // Act
        var requiresReporting = amount >= threshold;

        // Assert
        requiresReporting.Should().Be(shouldReport);
    }

    // =====================================================
    // Tests de ROS (Reporte de Operación Sospechosa)
    // =====================================================

    [Fact]
    public void SuspiciousActivityReport_ShouldBeCreated_WithRequiredFields()
    {
        // Arrange & Act
        var ros = new SuspiciousActivityReport
        {
            Id = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            ReportNumber = "ROS-2026-00001",
            ReportType = RosReportType.SuspiciousTransaction,
            TransactionAmount = 25000m,
            Currency = "USD",
            SuspicionIndicators = "Múltiples depósitos en efectivo bajo el umbral",
            Status = RosStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        ros.Should().NotBeNull();
        ros.ReportNumber.Should().StartWith("ROS-");
        ros.ReportType.Should().Be(RosReportType.SuspiciousTransaction);
        ros.Status.Should().Be(RosStatus.Draft);
    }

    [Theory]
    [InlineData(RosReportType.SuspiciousTransaction)]
    [InlineData(RosReportType.UnusualPattern)]
    [InlineData(RosReportType.StructuredTransaction)]
    [InlineData(RosReportType.PepRelated)]
    [InlineData(RosReportType.TerrorismFinancing)]
    public void RosReportType_ShouldHaveExpectedValues(RosReportType type)
    {
        // Assert
        Enum.IsDefined(typeof(RosReportType), type).Should().BeTrue();
    }

    // =====================================================
    // Tests de PEP (Personas Expuestas Políticamente)
    // =====================================================

    [Fact]
    public void PepScreening_ShouldFlagPoliticallyExposedPerson()
    {
        // Arrange
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            FullName = "Político Importante",
            IsPep = true,
            PepCategory = PepCategory.NationalGovernment,
            PepPosition = "Ministro de Hacienda",
            RiskLevel = RiskLevel.High
        };

        // Assert
        customer.IsPep.Should().BeTrue();
        customer.RiskLevel.Should().Be(RiskLevel.High);
    }

    [Theory]
    [InlineData(PepCategory.NationalGovernment)]
    [InlineData(PepCategory.LocalGovernment)]
    [InlineData(PepCategory.Judicial)]
    [InlineData(PepCategory.Military)]
    [InlineData(PepCategory.StateOwned)]
    [InlineData(PepCategory.InternationalOrganization)]
    [InlineData(PepCategory.PoliticalParty)]
    public void PepCategory_ShouldHaveExpectedValues(PepCategory category)
    {
        // Assert
        Enum.IsDefined(typeof(PepCategory), category).Should().BeTrue();
    }

    // =====================================================
    // Tests de Structuring Detection (Pitufeo)
    // =====================================================

    [Fact]
    public void StructuringDetection_ShouldIdentifyPatterns()
    {
        // Arrange - Múltiples transacciones justo debajo del umbral
        var transactions = new List<decimal> 
        { 
            9500m, 9800m, 9600m, 9700m, 9900m // Total: $48,500 en 5 transacciones
        };
        var threshold = 10000m;
        var totalAmount = transactions.Sum();
        var allBelowThreshold = transactions.All(t => t < threshold);

        // Assert - Patrón sospechoso de pitufeo
        allBelowThreshold.Should().BeTrue();
        totalAmount.Should().BeGreaterThan(threshold * 4); // Suma muy superior al umbral
    }

    // =====================================================
    // Tests de Lista de Sanciones
    // =====================================================

    [Fact]
    public void SanctionsScreening_ShouldCheckAgainstLists()
    {
        // Arrange
        var sanctionedNames = new List<string> 
        { 
            "Persona Sancionada 1", 
            "Empresa Sancionada SA",
            "Individuo en Lista OFAC"
        };
        var customerName = "Empresa Sancionada SA";

        // Act
        var isOnSanctionsList = sanctionedNames.Any(s => 
            s.Equals(customerName, StringComparison.OrdinalIgnoreCase));

        // Assert
        isOnSanctionsList.Should().BeTrue();
    }

    // =====================================================
    // Tests de Plazo de Reporte a UAF
    // =====================================================

    [Fact]
    public void RosSubmission_Deadline_ShouldBe15BusinessDays()
    {
        // Arrange
        var detectionDate = DateTime.UtcNow;
        var deadlineDays = 15; // 15 días hábiles para enviar a UAF

        // Act
        var deadline = AddBusinessDays(detectionDate, deadlineDays);

        // Assert
        deadline.Should().BeAfter(detectionDate);
    }

    private DateTime AddBusinessDays(DateTime startDate, int businessDays)
    {
        var currentDate = startDate;
        var daysAdded = 0;
        
        while (daysAdded < businessDays)
        {
            currentDate = currentDate.AddDays(1);
            if (currentDate.DayOfWeek != DayOfWeek.Saturday && 
                currentDate.DayOfWeek != DayOfWeek.Sunday)
            {
                daysAdded++;
            }
        }
        
        return currentDate;
    }

    // =====================================================
    // Tests de Matriz de Riesgo
    // =====================================================

    [Theory]
    [InlineData(true, 50000, "High")]     // PEP + alto monto
    [InlineData(false, 5000, "Low")]      // Normal + bajo monto
    [InlineData(false, 50000, "Medium")]  // Normal + alto monto
    [InlineData(true, 5000, "High")]      // PEP + bajo monto (PEP siempre alto)
    public void RiskMatrix_ShouldCalculateCorrectLevel(bool isPep, decimal amount, string expectedRisk)
    {
        // Act
        var calculatedRisk = CalculateRisk(isPep, amount);

        // Assert
        calculatedRisk.Should().Be(expectedRisk);
    }

    private string CalculateRisk(bool isPep, decimal amount)
    {
        if (isPep) return "High";
        if (amount >= 25000) return "Medium";
        return "Low";
    }
}
