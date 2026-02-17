using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit;
using KYCService.Application.Commands;
using KYCService.Application.DTOs;
using KYCService.Application.Handlers;
using KYCService.Domain.Entities;
using KYCService.Domain.Interfaces;

namespace KYCService.Tests.Handlers;

#region CreateSTRHandler Tests

public class CreateSTRHandlerTests
{
    private readonly Mock<ISuspiciousTransactionReportRepository> _repositoryMock;
    private readonly CreateSTRHandler _handler;

    public CreateSTRHandlerTests()
    {
        _repositoryMock = new Mock<ISuspiciousTransactionReportRepository>();
        // CreateSTRHandler only takes 1 parameter
        _handler = new CreateSTRHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateSTR()
    {
        // Arrange
        var command = new CreateSuspiciousTransactionReportCommand
        {
            UserId = Guid.NewGuid(),
            SuspiciousActivityType = "Money Laundering",
            Description = "Estructuración de depósitos menores a $10,000",
            Amount = 45000,
            Currency = "DOP",
            TransactionId = Guid.NewGuid(),
            RedFlags = new List<string> { "Depósitos fraccionados", "Documentos inconsistentes" },
            CreatedBy = Guid.NewGuid(),
            CreatedByName = "Compliance Officer",
            DetectedAt = DateTime.UtcNow
        };

        _repositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<SuspiciousTransactionReport>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SuspiciousTransactionReport r, CancellationToken ct) => r);

        _repositoryMock
            .Setup(r => r.GenerateReportNumberAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("STR-2026-0001");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.SuspiciousActivityType.Should().Be("Money Laundering");
        result.Status.Should().Be(STRStatus.Draft);
        result.RedFlags.Should().HaveCount(2);
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<SuspiciousTransactionReport>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithKYCProfile_ShouldLinkProfile()
    {
        // Arrange
        var kycProfileId = Guid.NewGuid();
        var command = new CreateSuspiciousTransactionReportCommand
        {
            UserId = Guid.NewGuid(),
            KYCProfileId = kycProfileId,
            SuspiciousActivityType = "Fraud",
            Description = "Uso de documentos falsificados",
            RedFlags = new List<string> { "Documento alterado" },
            CreatedBy = Guid.NewGuid(),
            CreatedByName = "Compliance Officer",
            DetectedAt = DateTime.UtcNow
        };

        _repositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<SuspiciousTransactionReport>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SuspiciousTransactionReport r, CancellationToken ct) => r);

        _repositoryMock
            .Setup(r => r.GenerateReportNumberAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("STR-2026-0002");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.KYCProfileId.Should().Be(kycProfileId);
    }

    [Fact]
    public async Task Handle_ShouldSetCorrectDeadline()
    {
        // Arrange - Per Ley 155-17, deadline is 15 working days
        var detectedAt = DateTime.UtcNow;
        var command = new CreateSuspiciousTransactionReportCommand
        {
            UserId = Guid.NewGuid(),
            SuspiciousActivityType = "Terrorist Financing",
            Description = "Transferencia a cuenta en zona de alto riesgo",
            RedFlags = new List<string> { "País sanctionado" },
            CreatedBy = Guid.NewGuid(),
            CreatedByName = "Compliance Officer",
            DetectedAt = detectedAt
        };

        _repositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<SuspiciousTransactionReport>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SuspiciousTransactionReport r, CancellationToken ct) => r);

        _repositoryMock
            .Setup(r => r.GenerateReportNumberAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("STR-2026-0003");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ReportingDeadline.Should().BeAfter(detectedAt);
        result.ReportingDeadline.Should().Be(detectedAt.AddDays(15)); // 15 días según Ley 155-17
    }
}

#endregion

#region ApproveSTRHandler Tests

public class ApproveSTRHandlerTests
{
    private readonly Mock<ISuspiciousTransactionReportRepository> _repositoryMock;
    private readonly ApproveSTRHandler _handler;

    public ApproveSTRHandlerTests()
    {
        _repositoryMock = new Mock<ISuspiciousTransactionReportRepository>();
        _handler = new ApproveSTRHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ValidApproval_ShouldSetStatusToApproved()
    {
        // Arrange
        var reportId = Guid.NewGuid();
        var command = new ApproveSTRCommand
        {
            Id = reportId,
            ApprovedBy = Guid.NewGuid()
        };

        var existingReport = new SuspiciousTransactionReport
        {
            Id = reportId,
            ReportNumber = "STR-2026-0001",
            Status = STRStatus.PendingReview,
            SuspiciousActivityType = "Money Laundering",
            Description = "Test",
            RedFlags = new List<string> { "Test" }
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(reportId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingReport);

        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<SuspiciousTransactionReport>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SuspiciousTransactionReport r, CancellationToken ct) => r);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(STRStatus.Approved);
        // Verify entity was updated with correct ApprovedBy
        _repositoryMock.Verify(r => r.UpdateAsync(
            It.Is<SuspiciousTransactionReport>(rpt => 
                rpt.Status == STRStatus.Approved && 
                rpt.ApprovedBy == command.ApprovedBy),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ReportNotFound_ShouldThrow()
    {
        // Arrange
        var command = new ApproveSTRCommand
        {
            Id = Guid.NewGuid(),
            ApprovedBy = Guid.NewGuid()
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SuspiciousTransactionReport?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ReportNotInPendingReview_ShouldThrow()
    {
        // Arrange
        var reportId = Guid.NewGuid();
        var command = new ApproveSTRCommand
        {
            Id = reportId,
            ApprovedBy = Guid.NewGuid()
        };

        var existingReport = new SuspiciousTransactionReport
        {
            Id = reportId,
            ReportNumber = "STR-2026-0001",
            Status = STRStatus.Draft, // Not PendingReview
            SuspiciousActivityType = "Test",
            Description = "Test",
            RedFlags = new List<string> { "Test" }
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(reportId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingReport);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }
}

#endregion

#region SendSTRToUAFHandler Tests

public class SendSTRToUAFHandlerTests
{
    private readonly Mock<ISuspiciousTransactionReportRepository> _repositoryMock;
    private readonly SendSTRToUAFHandler _handler;

    public SendSTRToUAFHandlerTests()
    {
        _repositoryMock = new Mock<ISuspiciousTransactionReportRepository>();
        _handler = new SendSTRToUAFHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ApprovedReport_ShouldSendToUAF()
    {
        // Arrange
        var reportId = Guid.NewGuid();
        var command = new SendSTRToUAFCommand
        {
            Id = reportId,
            SentBy = Guid.NewGuid(),
            UAFReportNumber = "UAF-2026-0001"
        };

        var existingReport = new SuspiciousTransactionReport
        {
            Id = reportId,
            ReportNumber = "STR-2026-0001",
            Status = STRStatus.Approved,
            SuspiciousActivityType = "Money Laundering",
            Description = "Test",
            RedFlags = new List<string> { "Test" },
            ApprovedBy = Guid.NewGuid(),
            ApprovedAt = DateTime.UtcNow.AddHours(-1)
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(reportId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingReport);

        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<SuspiciousTransactionReport>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SuspiciousTransactionReport r, CancellationToken ct) => r);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(STRStatus.SentToUAF);
        result.UAFReportNumber.Should().Be("UAF-2026-0001");
        result.SentToUAFAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_NotApprovedReport_ShouldThrow()
    {
        // Arrange
        var command = new SendSTRToUAFCommand
        {
            Id = Guid.NewGuid(),
            SentBy = Guid.NewGuid(),
            UAFReportNumber = "UAF-2026-0002"
        };

        var existingReport = new SuspiciousTransactionReport
        {
            Id = command.Id,
            ReportNumber = "STR-2026-0002",
            Status = STRStatus.Draft, // Not approved
            SuspiciousActivityType = "Test",
            Description = "Test",
            RedFlags = new List<string> { "Test" }
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingReport);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ValidSend_ShouldRecordSentByInEntity()
    {
        // Arrange
        var reportId = Guid.NewGuid();
        var sentBy = Guid.NewGuid();
        var command = new SendSTRToUAFCommand
        {
            Id = reportId,
            SentBy = sentBy,
            UAFReportNumber = "UAF-2026-0003"
        };

        var existingReport = new SuspiciousTransactionReport
        {
            Id = reportId,
            ReportNumber = "STR-2026-0003",
            Status = STRStatus.Approved,
            SuspiciousActivityType = "Fraud",
            Description = "Test",
            RedFlags = new List<string> { "Test" },
            ApprovedBy = Guid.NewGuid(),
            ApprovedAt = DateTime.UtcNow.AddHours(-1)
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(reportId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingReport);

        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<SuspiciousTransactionReport>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SuspiciousTransactionReport r, CancellationToken ct) => r);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - Verify entity update was called with correct SentBy
        _repositoryMock.Verify(r => r.UpdateAsync(
            It.Is<SuspiciousTransactionReport>(rpt => 
                rpt.SentBy == sentBy && 
                rpt.Status == STRStatus.SentToUAF),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ReportNotFound_ShouldThrow()
    {
        // Arrange
        var command = new SendSTRToUAFCommand
        {
            Id = Guid.NewGuid(),
            SentBy = Guid.NewGuid(),
            UAFReportNumber = "UAF-2026-0004"
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SuspiciousTransactionReport?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }
}

#endregion
