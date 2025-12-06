using FluentAssertions;
using Xunit;
using ReportsService.Domain.Entities;

namespace ReportsService.Tests;

public class ReportTests
{
    private readonly Guid _dealerId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public void Constructor_WithValidData_ShouldCreateReport()
    {
        // Arrange & Act
        var report = new Report(
            _dealerId,
            "Monthly Sales Report",
            ReportType.Sales,
            ReportFormat.Pdf,
            _userId,
            "Sales report for the month");

        // Assert
        report.Id.Should().NotBeEmpty();
        report.DealerId.Should().Be(_dealerId);
        report.Name.Should().Be("Monthly Sales Report");
        report.Type.Should().Be(ReportType.Sales);
        report.Format.Should().Be(ReportFormat.Pdf);
        report.Status.Should().Be(ReportStatus.Draft);
        report.Description.Should().Be("Sales report for the month");
        report.CreatedBy.Should().Be(_userId);
        report.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Constructor_WithEmptyName_ShouldThrowException()
    {
        // Arrange & Act
        var act = () => new Report(_dealerId, "", ReportType.Sales, ReportFormat.Pdf, _userId);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Name is required*");
    }

    [Fact]
    public void SetDateRange_WithValidDates_ShouldSetDates()
    {
        // Arrange
        var report = new Report(_dealerId, "Test Report", ReportType.Sales, ReportFormat.Pdf, _userId);
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow;

        // Act
        report.SetDateRange(startDate, endDate);

        // Assert
        report.StartDate.Should().Be(startDate);
        report.EndDate.Should().Be(endDate);
    }

    [Fact]
    public void SetDateRange_WithEndDateBeforeStartDate_ShouldThrowException()
    {
        // Arrange
        var report = new Report(_dealerId, "Test Report", ReportType.Sales, ReportFormat.Pdf, _userId);
        var startDate = DateTime.UtcNow;
        var endDate = DateTime.UtcNow.AddDays(-30);

        // Act
        var act = () => report.SetDateRange(startDate, endDate);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*End date must be after start date*");
    }

    [Fact]
    public void StartGeneration_ShouldSetStatusToGenerating()
    {
        // Arrange
        var report = new Report(_dealerId, "Test Report", ReportType.Sales, ReportFormat.Pdf, _userId);

        // Act
        report.StartGeneration();

        // Assert
        report.Status.Should().Be(ReportStatus.Generating);
    }

    [Fact]
    public void Complete_ShouldSetStatusAndFileInfo()
    {
        // Arrange
        var report = new Report(_dealerId, "Test Report", ReportType.Sales, ReportFormat.Pdf, _userId);
        report.StartGeneration();

        // Act
        report.Complete("/reports/test.pdf", 1024);

        // Assert
        report.Status.Should().Be(ReportStatus.Ready);
        report.FilePath.Should().Be("/reports/test.pdf");
        report.FileSize.Should().Be(1024);
        report.GeneratedAt.Should().NotBeNull();
        report.ExpiresAt.Should().NotBeNull();
    }

    [Fact]
    public void Fail_ShouldSetStatusAndErrorMessage()
    {
        // Arrange
        var report = new Report(_dealerId, "Test Report", ReportType.Sales, ReportFormat.Pdf, _userId);
        report.StartGeneration();

        // Act
        report.Fail("Database connection failed");

        // Assert
        report.Status.Should().Be(ReportStatus.Failed);
        report.ErrorMessage.Should().Be("Database connection failed");
    }

    [Fact]
    public void MarkAsExpired_ShouldSetStatusToExpired()
    {
        // Arrange
        var report = new Report(_dealerId, "Test Report", ReportType.Sales, ReportFormat.Pdf, _userId);
        report.Complete("/reports/test.pdf", 1024);

        // Act
        report.MarkAsExpired();

        // Assert
        report.Status.Should().Be(ReportStatus.Expired);
    }
}

