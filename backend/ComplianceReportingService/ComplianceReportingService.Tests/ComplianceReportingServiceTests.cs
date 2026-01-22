// =====================================================
// C8: ComplianceReportingService - Tests Unitarios
// Reportes Consolidados de Cumplimiento
// =====================================================

using FluentAssertions;
using ComplianceReportingService.Domain.Entities;
using ComplianceReportingService.Domain.Enums;
using Xunit;

namespace ComplianceReportingService.Tests;

public class ComplianceReportingServiceTests
{
    // =====================================================
    // Tests de Generación de Reportes
    // =====================================================

    [Fact]
    public void ComplianceReport_ShouldBeCreated_WithRequiredFields()
    {
        // Arrange & Act
        var report = new ComplianceReport
        {
            Id = Guid.NewGuid(),
            ReportNumber = "RPT-2026-00001",
            ReportType = ReportType.Monthly,
            RegulatoryBody = RegulatoryBody.DGII,
            Period = "202601",
            Status = ReportStatus.Draft,
            GeneratedAt = DateTime.UtcNow,
            GeneratedBy = "Sistema",
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        report.Should().NotBeNull();
        report.ReportNumber.Should().StartWith("RPT-");
        report.Status.Should().Be(ReportStatus.Draft);
    }

    [Theory]
    [InlineData(ReportType.Daily)]
    [InlineData(ReportType.Weekly)]
    [InlineData(ReportType.Monthly)]
    [InlineData(ReportType.Quarterly)]
    [InlineData(ReportType.Annual)]
    [InlineData(ReportType.OnDemand)]
    public void ReportType_ShouldHaveExpectedValues(ReportType type)
    {
        // Assert
        Enum.IsDefined(typeof(ReportType), type).Should().BeTrue();
    }

    [Theory]
    [InlineData(RegulatoryBody.DGII)]
    [InlineData(RegulatoryBody.UAF)]
    [InlineData(RegulatoryBody.ProConsumidor)]
    [InlineData(RegulatoryBody.INTRANT)]
    [InlineData(RegulatoryBody.OGTIC)]
    [InlineData(RegulatoryBody.SuperintendenciaBancos)]
    public void RegulatoryBody_ShouldHaveExpectedValues(RegulatoryBody body)
    {
        // Assert
        Enum.IsDefined(typeof(RegulatoryBody), body).Should().BeTrue();
    }

    // =====================================================
    // Tests de Estado de Reporte
    // =====================================================

    [Theory]
    [InlineData(ReportStatus.Draft)]
    [InlineData(ReportStatus.Generated)]
    [InlineData(ReportStatus.Validated)]
    [InlineData(ReportStatus.Submitted)]
    [InlineData(ReportStatus.Accepted)]
    [InlineData(ReportStatus.Rejected)]
    [InlineData(ReportStatus.Error)]
    public void ReportStatus_ShouldHaveExpectedValues(ReportStatus status)
    {
        // Assert
        Enum.IsDefined(typeof(ReportStatus), status).Should().BeTrue();
    }

    // =====================================================
    // Tests de Programación de Reportes
    // =====================================================

    [Fact]
    public void ReportSchedule_ShouldBeCreated_WithValidCron()
    {
        // Arrange & Act
        var schedule = new ReportSchedule
        {
            Id = Guid.NewGuid(),
            ReportType = ReportType.Monthly,
            RegulatoryBody = RegulatoryBody.DGII,
            CronExpression = "0 0 1 * *", // Primer día de cada mes a medianoche
            IsActive = true,
            NextExecutionAt = DateTime.UtcNow.AddDays(10),
            LastExecutedAt = null,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        schedule.CronExpression.Should().NotBeNullOrEmpty();
        schedule.IsActive.Should().BeTrue();
        schedule.NextExecutionAt.Should().BeAfter(DateTime.UtcNow);
    }

    // =====================================================
    // Tests de Datos del Reporte
    // =====================================================

    [Fact]
    public void ReportData_ShouldContainSummaryInfo()
    {
        // Arrange & Act
        var reportData = new ReportData
        {
            Id = Guid.NewGuid(),
            ReportId = Guid.NewGuid(),
            TotalRecords = 1500,
            TotalAmount = 50000000m,
            Currency = "DOP",
            PeriodStart = new DateTime(2026, 1, 1),
            PeriodEnd = new DateTime(2026, 1, 31),
            DataAsJson = "{\"transactions\": [...]}",
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        reportData.TotalRecords.Should().BePositive();
        reportData.TotalAmount.Should().BePositive();
        reportData.PeriodEnd.Should().BeAfter(reportData.PeriodStart);
    }

    // =====================================================
    // Tests de Validación de Reportes
    // =====================================================

    [Fact]
    public void ReportValidation_ShouldPass_WhenAllRulesOk()
    {
        // Arrange & Act
        var validation = new ReportValidation
        {
            Id = Guid.NewGuid(),
            ReportId = Guid.NewGuid(),
            ValidationDate = DateTime.UtcNow,
            TotalRules = 10,
            PassedRules = 10,
            FailedRules = 0,
            IsValid = true,
            ValidatedBy = "Sistema",
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        validation.IsValid.Should().BeTrue();
        validation.PassedRules.Should().Be(validation.TotalRules);
        validation.FailedRules.Should().Be(0);
    }

    [Fact]
    public void ReportValidation_ShouldFail_WhenRulesFail()
    {
        // Arrange & Act
        var validation = new ReportValidation
        {
            Id = Guid.NewGuid(),
            ReportId = Guid.NewGuid(),
            ValidationDate = DateTime.UtcNow,
            TotalRules = 10,
            PassedRules = 7,
            FailedRules = 3,
            IsValid = false,
            ValidationErrors = "Error en formato de RNC, Monto negativo, Fecha inválida",
            ValidatedBy = "Sistema",
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        validation.IsValid.Should().BeFalse();
        validation.FailedRules.Should().Be(3);
        validation.ValidationErrors.Should().NotBeNullOrEmpty();
    }

    // =====================================================
    // Tests de Submission a Entes
    // =====================================================

    [Fact]
    public void ReportSubmission_ShouldBeCreated_WithTrackingInfo()
    {
        // Arrange & Act
        var submission = new ReportSubmission
        {
            Id = Guid.NewGuid(),
            ReportId = Guid.NewGuid(),
            RegulatoryBody = RegulatoryBody.DGII,
            SubmissionMethod = SubmissionMethod.Api,
            SubmittedAt = DateTime.UtcNow,
            ConfirmationNumber = "DGII-2026-123456",
            Status = SubmissionStatus.Confirmed,
            ResponseMessage = "Reporte recibido exitosamente",
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        submission.ConfirmationNumber.Should().NotBeNullOrEmpty();
        submission.Status.Should().Be(SubmissionStatus.Confirmed);
    }

    [Theory]
    [InlineData(SubmissionMethod.Api)]
    [InlineData(SubmissionMethod.Sftp)]
    [InlineData(SubmissionMethod.Email)]
    [InlineData(SubmissionMethod.Portal)]
    [InlineData(SubmissionMethod.Manual)]
    public void SubmissionMethod_ShouldHaveExpectedValues(SubmissionMethod method)
    {
        // Assert
        Enum.IsDefined(typeof(SubmissionMethod), method).Should().BeTrue();
    }

    // =====================================================
    // Tests de Fechas Límite
    // =====================================================

    [Fact]
    public void ReportDeadline_ShouldBeCalculated_ForDGII()
    {
        // Arrange - DGII: 15 días después del cierre del período
        var periodEnd = new DateTime(2026, 1, 31);
        var deadlineDays = 15;

        // Act
        var deadline = periodEnd.AddDays(deadlineDays);

        // Assert
        deadline.Should().Be(new DateTime(2026, 2, 15));
    }

    [Fact]
    public void ReportDeadline_ShouldBeCalculated_ForUAF()
    {
        // Arrange - UAF ROS: 15 días hábiles desde detección
        var detectionDate = DateTime.UtcNow;
        var deadlineBusinessDays = 15;

        // Act
        var deadline = AddBusinessDays(detectionDate, deadlineBusinessDays);

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
    // Tests de Formatos de Reporte
    // =====================================================

    [Theory]
    [InlineData(ReportFormat.Pdf)]
    [InlineData(ReportFormat.Excel)]
    [InlineData(ReportFormat.Csv)]
    [InlineData(ReportFormat.Xml)]
    [InlineData(ReportFormat.Json)]
    [InlineData(ReportFormat.Txt607)] // Formato específico DGII
    public void ReportFormat_ShouldHaveExpectedValues(ReportFormat format)
    {
        // Assert
        Enum.IsDefined(typeof(ReportFormat), format).Should().BeTrue();
    }

    // =====================================================
    // Tests de Historial de Reportes
    // =====================================================

    [Fact]
    public void ReportHistory_ShouldTrackChanges()
    {
        // Arrange & Act
        var history = new ReportHistory
        {
            Id = Guid.NewGuid(),
            ReportId = Guid.NewGuid(),
            Action = ReportAction.StatusChanged,
            PreviousValue = "Draft",
            NewValue = "Generated",
            ChangedBy = "Sistema",
            ChangedAt = DateTime.UtcNow,
            IpAddress = "192.168.1.100"
        };

        // Assert
        history.Action.Should().Be(ReportAction.StatusChanged);
        history.PreviousValue.Should().NotBe(history.NewValue);
    }

    [Theory]
    [InlineData(ReportAction.Created)]
    [InlineData(ReportAction.Generated)]
    [InlineData(ReportAction.Validated)]
    [InlineData(ReportAction.Submitted)]
    [InlineData(ReportAction.StatusChanged)]
    [InlineData(ReportAction.Corrected)]
    [InlineData(ReportAction.Cancelled)]
    public void ReportAction_ShouldHaveExpectedValues(ReportAction action)
    {
        // Assert
        Enum.IsDefined(typeof(ReportAction), action).Should().BeTrue();
    }

    // =====================================================
    // Tests de Dashboard de Cumplimiento
    // =====================================================

    [Fact]
    public void ComplianceDashboard_ShouldShowOverallStatus()
    {
        // Arrange & Act
        var dashboard = new
        {
            TotalReports = 50,
            PendingReports = 5,
            SubmittedReports = 40,
            RejectedReports = 2,
            OverdueReports = 3,
            ComplianceScore = 94.0m // 47/50 = 94%
        };

        // Assert
        dashboard.ComplianceScore.Should().BeGreaterOrEqualTo(90); // Meta: 90%+
        dashboard.OverdueReports.Should().BeLessThan(5); // Alerta si > 5
    }
}
