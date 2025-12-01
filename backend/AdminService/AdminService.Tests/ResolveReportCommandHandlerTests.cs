using AdminService.Application.Interfaces;
using AdminService.Application.UseCases.Reports.ResolveReport;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AdminService.Tests.Application.UseCases.Reports;

public class ResolveReportCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_ReturnsTrue()
    {
        // Arrange
        var auditMock = new Mock<IAuditServiceClient>();
        var notificationMock = new Mock<INotificationServiceClient>();
        var loggerMock = new Mock<ILogger<ResolveReportCommandHandler>>();
        
        var handler = new ResolveReportCommandHandler(
            auditMock.Object,
            notificationMock.Object,
            loggerMock.Object
        );

        var command = new ResolveReportCommand(
            ReportId: Guid.NewGuid(),
            ResolvedBy: "admin@test.com",
            Resolution: "Issue has been resolved",
            ReporterEmail: "reporter@test.com",
            ReportSubject: "Spam complaint"
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        auditMock.Verify(x => x.LogReportResolvedAsync(
            It.IsAny<Guid>(), 
            It.IsAny<string>(), 
            It.IsAny<string>()), Times.Once);
        notificationMock.Verify(x => x.SendReportResolvedNotificationAsync(
            It.IsAny<string>(), 
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Handle_BothServicesFail_StillReturnsTrue()
    {
        // Arrange
        var auditMock = new Mock<IAuditServiceClient>();
        auditMock.Setup(x => x.LogReportResolvedAsync(
            It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Audit unavailable"));

        var notificationMock = new Mock<INotificationServiceClient>();
        notificationMock.Setup(x => x.SendReportResolvedNotificationAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Notification unavailable"));
            
        var loggerMock = new Mock<ILogger<ResolveReportCommandHandler>>();
        
        var handler = new ResolveReportCommandHandler(
            auditMock.Object,
            notificationMock.Object,
            loggerMock.Object
        );

        var command = new ResolveReportCommand(
            Guid.NewGuid(),
            "admin@test.com",
            "Resolved",
            "reporter@test.com",
            "Test Report"
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert - Should handle failures gracefully
        Assert.True(result);
    }
}
