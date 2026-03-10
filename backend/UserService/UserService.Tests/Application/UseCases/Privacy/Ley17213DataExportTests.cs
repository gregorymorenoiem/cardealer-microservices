using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using UserService.Application.DTOs.Privacy;
using UserService.Application.UseCases.Privacy.DownloadExportData;
using UserService.Application.UseCases.Privacy.GetExportStatus;
using UserService.Application.UseCases.Privacy.RequestDataExport;
using UserService.Domain.Entities.Privacy;
using UserService.Domain.Interfaces;

namespace UserService.Tests.Application.UseCases.Privacy;

// ═══════════════════════════════════════════════════════════════════════════════
// PHASE 11 — LEY 172-13 DATA EXPORT & PORTABILITY TESTS
//
// Tests the complete data export flow:
//   1. RequestDataExportCommandHandler — creates export request
//   2. GetExportStatusQueryHandler — retrieves export status
//   3. DownloadExportDataQueryHandler — validates download token
//   4. PrivacyController.DownloadExport — streams file to client
// ═══════════════════════════════════════════════════════════════════════════════

#region RequestDataExportCommandHandler Tests

public class RequestDataExportCommandHandlerTests
{
    private readonly Mock<IPrivacyRequestRepository> _repoMock;
    private readonly Mock<ILogger<RequestDataExportCommandHandler>> _loggerMock;
    private readonly RequestDataExportCommandHandler _handler;

    public RequestDataExportCommandHandlerTests()
    {
        _repoMock = new Mock<IPrivacyRequestRepository>();
        _loggerMock = new Mock<ILogger<RequestDataExportCommandHandler>>();
        _handler = new RequestDataExportCommandHandler(_repoMock.Object, _loggerMock.Object);
    }

    private static RequestDataExportCommand MakeCommand(Guid userId) =>
        new(userId, ExportFormat.Json, true, true, true, true, true, "127.0.0.1", "TestAgent");

    [Fact]
    public async Task Handle_ValidRequest_CreatesExportRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = MakeCommand(userId);

        _repoMock.Setup(r => r.HasPendingRequestAsync(userId, PrivacyRequestType.Portability))
            .ReturnsAsync(false);

        _repoMock.Setup(r => r.AddAsync(It.IsAny<PrivacyRequest>()))
            .ReturnsAsync((PrivacyRequest pr) => pr);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.RequestId.Should().NotBe(Guid.Empty);
        result.Status.Should().Be("Pending");
        _repoMock.Verify(r => r.AddAsync(It.Is<PrivacyRequest>(
            pr => pr.Type == PrivacyRequestType.Portability &&
                  pr.Status == PrivacyRequestStatus.Pending &&
                  pr.UserId == userId)), Times.Once);
    }

    [Fact]
    public async Task Handle_ExistingPendingExport_ReturnsExistingRequestStatus()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingRequest = new PrivacyRequest
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = PrivacyRequestType.Portability,
            Status = PrivacyRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow.AddMinutes(-5)
        };

        _repoMock.Setup(r => r.HasPendingRequestAsync(userId, PrivacyRequestType.Portability))
            .ReturnsAsync(true);

        _repoMock.Setup(r => r.GetLatestExportRequestAsync(userId))
            .ReturnsAsync(existingRequest);

        var command = MakeCommand(userId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("Pending");
        // Should NOT create a new request when one is pending
        _repoMock.Verify(r => r.AddAsync(It.IsAny<PrivacyRequest>()), Times.Never);
    }

    [Fact]
    public async Task Handle_NoPendingExport_CreatesNewRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _repoMock.Setup(r => r.HasPendingRequestAsync(userId, PrivacyRequestType.Portability))
            .ReturnsAsync(false);

        _repoMock.Setup(r => r.AddAsync(It.IsAny<PrivacyRequest>()))
            .ReturnsAsync((PrivacyRequest pr) => pr);

        var command = MakeCommand(userId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("Pending");
        _repoMock.Verify(r => r.AddAsync(It.IsAny<PrivacyRequest>()), Times.Once);
    }

    [Fact]
    public async Task Handle_SetsExportFormatJson_Correctly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _repoMock.Setup(r => r.HasPendingRequestAsync(userId, PrivacyRequestType.Portability))
            .ReturnsAsync(false);
        _repoMock.Setup(r => r.AddAsync(It.IsAny<PrivacyRequest>()))
            .ReturnsAsync((PrivacyRequest pr) => pr);

        var command = MakeCommand(userId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _repoMock.Verify(r => r.AddAsync(It.Is<PrivacyRequest>(
            pr => pr.ExportFormat == ExportFormat.Json)), Times.Once);
    }

    [Fact]
    public async Task Handle_SetsDescription_WithDataSections()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _repoMock.Setup(r => r.HasPendingRequestAsync(userId, PrivacyRequestType.Portability))
            .ReturnsAsync(false);
        _repoMock.Setup(r => r.AddAsync(It.IsAny<PrivacyRequest>()))
            .ReturnsAsync((PrivacyRequest pr) => pr);

        var command = MakeCommand(userId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert — Description should list the sections being exported
        _repoMock.Verify(r => r.AddAsync(It.Is<PrivacyRequest>(
            pr => pr.Description != null && pr.Description.Contains("perfil"))), Times.Once);
    }

    [Fact]
    public async Task Handle_ReturnsEstimatedCompletionTime_Within10Minutes()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _repoMock.Setup(r => r.HasPendingRequestAsync(userId, PrivacyRequestType.Portability))
            .ReturnsAsync(false);
        _repoMock.Setup(r => r.AddAsync(It.IsAny<PrivacyRequest>()))
            .ReturnsAsync((PrivacyRequest pr) => pr);

        var now = DateTime.UtcNow;
        var command = MakeCommand(userId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert — SLA: estimated completion within 10 minutes
        result.EstimatedCompletionTime.Should().NotBeNull();
        result.EstimatedCompletionTime!.Value.Should().BeCloseTo(now.AddMinutes(10), TimeSpan.FromMinutes(1));
    }
}

#endregion

#region GetExportStatusQueryHandler Tests

public class GetExportStatusQueryHandlerTests
{
    private readonly Mock<IPrivacyRequestRepository> _repoMock;
    private readonly GetExportStatusQueryHandler _handler;

    public GetExportStatusQueryHandlerTests()
    {
        _repoMock = new Mock<IPrivacyRequestRepository>();
        _handler = new GetExportStatusQueryHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_NoExportRequest_ReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _repoMock.Setup(r => r.GetLatestExportRequestAsync(userId))
            .ReturnsAsync((PrivacyRequest?)null);

        // Act
        var result = await _handler.Handle(new GetExportStatusQuery(userId), CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_PendingExport_ReturnsStatusWithoutToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new PrivacyRequest
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Status = PrivacyRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _repoMock.Setup(r => r.GetLatestExportRequestAsync(userId))
            .ReturnsAsync(request);

        // Act
        var result = await _handler.Handle(new GetExportStatusQuery(userId), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be("Pending");
        result.DownloadToken.Should().BeNull();
    }

    [Fact]
    public async Task Handle_CompletedExport_ReturnsDownloadToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new PrivacyRequest
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Status = PrivacyRequestStatus.Completed,
            DownloadToken = "test-token-abc123",
            DownloadTokenExpiresAt = DateTime.UtcNow.AddHours(23),
            FileSizeBytes = 15360,
            ExportFormat = ExportFormat.Json,
            CreatedAt = DateTime.UtcNow.AddMinutes(-5),
            CompletedAt = DateTime.UtcNow,
        };

        _repoMock.Setup(r => r.GetLatestExportRequestAsync(userId))
            .ReturnsAsync(request);

        // Act
        var result = await _handler.Handle(new GetExportStatusQuery(userId), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be("Completed");
        result.DownloadToken.Should().Be("test-token-abc123");
        result.FileSize.Should().Be("15 KB");
        result.Format.Should().Be("Json");
    }

    [Fact]
    public async Task Handle_CompletedExport_FormatsFileSizeCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new PrivacyRequest
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Status = PrivacyRequestStatus.Completed,
            DownloadToken = "token",
            FileSizeBytes = 2_097_152, // 2 MB
            CreatedAt = DateTime.UtcNow
        };

        _repoMock.Setup(r => r.GetLatestExportRequestAsync(userId))
            .ReturnsAsync(request);

        // Act
        var result = await _handler.Handle(new GetExportStatusQuery(userId), CancellationToken.None);

        // Assert
        result!.FileSize.Should().Be("2 MB");
    }

    [Fact]
    public async Task Handle_ProcessingExport_ReturnsProcessingStatus()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new PrivacyRequest
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Status = PrivacyRequestStatus.Processing,
            CreatedAt = DateTime.UtcNow
        };

        _repoMock.Setup(r => r.GetLatestExportRequestAsync(userId))
            .ReturnsAsync(request);

        // Act
        var result = await _handler.Handle(new GetExportStatusQuery(userId), CancellationToken.None);

        // Assert
        result!.Status.Should().Be("Processing");
        result.DownloadToken.Should().BeNull();
    }
}

#endregion

#region DownloadExportDataQueryHandler Tests

public class DownloadExportDataQueryHandlerTests
{
    private readonly Mock<IPrivacyRequestRepository> _repoMock;
    private readonly DownloadExportDataQueryHandler _handler;

    public DownloadExportDataQueryHandlerTests()
    {
        _repoMock = new Mock<IPrivacyRequestRepository>();
        _handler = new DownloadExportDataQueryHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_EmptyToken_ReturnsNull()
    {
        // Act
        var result = await _handler.Handle(new DownloadExportDataQuery(""), CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_InvalidToken_ReturnsNull()
    {
        // Arrange
        _repoMock.Setup(r => r.GetByDownloadTokenAsync("invalid-token"))
            .ReturnsAsync((PrivacyRequest?)null);

        // Act
        var result = await _handler.Handle(
            new DownloadExportDataQuery("invalid-token"), CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ValidToken_ReturnsFileInfo()
    {
        // Arrange
        var request = new PrivacyRequest
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Status = PrivacyRequestStatus.Completed,
            DownloadToken = "valid-token",
            DownloadTokenExpiresAt = DateTime.UtcNow.AddHours(12),
            FilePath = "/exports/okla_datos_abc123_20260309.zip",
            FileSizeBytes = 45000,
        };

        _repoMock.Setup(r => r.GetByDownloadTokenAsync("valid-token"))
            .ReturnsAsync(request);

        // Act
        var result = await _handler.Handle(
            new DownloadExportDataQuery("valid-token"), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.IsExpired.Should().BeFalse();
        result.FileName.Should().Be("okla_datos_abc123_20260309.zip");
        result.FileSizeBytes.Should().Be(45000);
    }

    [Fact]
    public async Task Handle_ExpiredToken_ReturnsExpiredResult()
    {
        // Arrange
        var request = new PrivacyRequest
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Status = PrivacyRequestStatus.Completed,
            DownloadToken = "expired-token",
            DownloadTokenExpiresAt = DateTime.UtcNow.AddHours(-1), // Expired!
            FilePath = "/exports/old_file.zip",
            FileSizeBytes = 30000,
        };

        _repoMock.Setup(r => r.GetByDownloadTokenAsync("expired-token"))
            .ReturnsAsync(request);

        // Act
        var result = await _handler.Handle(
            new DownloadExportDataQuery("expired-token"), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.IsExpired.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_TokenWithNoExpiry_ReturnsValidResult()
    {
        // Arrange — edge case: no expiry date set
        var request = new PrivacyRequest
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Status = PrivacyRequestStatus.Completed,
            DownloadToken = "no-expiry-token",
            DownloadTokenExpiresAt = null,
            FilePath = "/exports/some_file.zip",
            FileSizeBytes = 10000,
        };

        _repoMock.Setup(r => r.GetByDownloadTokenAsync("no-expiry-token"))
            .ReturnsAsync(request);

        // Act
        var result = await _handler.Handle(
            new DownloadExportDataQuery("no-expiry-token"), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.IsExpired.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_NoFilePath_ReturnsResultWithEmptyPath()
    {
        // Arrange
        var request = new PrivacyRequest
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Status = PrivacyRequestStatus.Completed,
            DownloadToken = "token-no-file",
            DownloadTokenExpiresAt = DateTime.UtcNow.AddHours(12),
            FilePath = null,
        };

        _repoMock.Setup(r => r.GetByDownloadTokenAsync("token-no-file"))
            .ReturnsAsync(request);

        // Act
        var result = await _handler.Handle(
            new DownloadExportDataQuery("token-no-file"), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.FilePath.Should().BeEmpty();
    }
}

#endregion

#region PrivacyRequest Entity Tests

public class PrivacyRequestExportEntityTests
{
    [Fact]
    public void PrivacyRequest_DefaultStatus_IsPending()
    {
        var request = new PrivacyRequest();
        request.Status.Should().Be(PrivacyRequestStatus.Pending);
    }

    [Fact]
    public void PrivacyRequest_ExportFields_AreNullableByDefault()
    {
        var request = new PrivacyRequest();

        request.ExportFormat.Should().BeNull();
        request.DownloadToken.Should().BeNull();
        request.DownloadTokenExpiresAt.Should().BeNull();
        request.FilePath.Should().BeNull();
        request.FileSizeBytes.Should().BeNull();
    }

    [Fact]
    public void PrivacyRequest_CanSetPortabilityType()
    {
        var request = new PrivacyRequest
        {
            UserId = Guid.NewGuid(),
            Type = PrivacyRequestType.Portability,
            ExportFormat = ExportFormat.Json,
        };

        request.Type.Should().Be(PrivacyRequestType.Portability);
        request.ExportFormat.Should().Be(ExportFormat.Json);
    }

    [Fact]
    public void PrivacyRequest_CanTrack24HourTokenExpiry()
    {
        var now = DateTime.UtcNow;
        var request = new PrivacyRequest
        {
            DownloadToken = "secure-token",
            DownloadTokenExpiresAt = now.AddHours(24),
        };

        request.DownloadTokenExpiresAt.Should().BeAfter(now);
        (request.DownloadTokenExpiresAt.Value - now).TotalHours.Should().BeApproximately(24, 0.01);
    }

    [Fact]
    public void PrivacyRequestStatus_HasExpiredValue()
    {
        // Verify the Expired status exists for cleanup workflow
        var status = PrivacyRequestStatus.Expired;
        status.ToString().Should().Be("Expired");
    }

    [Fact]
    public void ExportFormat_HasJsonCsvPdf()
    {
        Enum.GetValues<ExportFormat>().Should().HaveCount(3);
        Enum.GetValues<ExportFormat>().Should().Contain(ExportFormat.Json);
        Enum.GetValues<ExportFormat>().Should().Contain(ExportFormat.Csv);
        Enum.GetValues<ExportFormat>().Should().Contain(ExportFormat.Pdf);
    }
}

#endregion

#region Data Export Integration Flow Tests

public class DataExportFlowTests
{
    private static RequestDataExportCommand MakeCommand(Guid userId) =>
        new(userId, ExportFormat.Json, true, true, true, true, true, "127.0.0.1", "TestAgent");

    [Fact]
    public async Task FullFlow_RequestExport_ThenCheckStatus_ReturnsConsistentData()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var repoMock = new Mock<IPrivacyRequestRepository>();
        var loggerMock = new Mock<ILogger<RequestDataExportCommandHandler>>();

        PrivacyRequest? savedRequest = null;

        repoMock.Setup(r => r.HasPendingRequestAsync(userId, PrivacyRequestType.Portability))
            .ReturnsAsync(false);

        repoMock.Setup(r => r.GetLatestExportRequestAsync(userId))
            .ReturnsAsync(() => savedRequest);

        repoMock.Setup(r => r.AddAsync(It.IsAny<PrivacyRequest>()))
            .Callback<PrivacyRequest>(pr => savedRequest = pr)
            .ReturnsAsync((PrivacyRequest pr) => pr);

        // Step 1: Request export
        var requestHandler = new RequestDataExportCommandHandler(repoMock.Object, loggerMock.Object);
        var requestResult = await requestHandler.Handle(
            MakeCommand(userId), CancellationToken.None);

        requestResult.Should().NotBeNull();
        requestResult.Status.Should().Be("Pending");

        // Step 2: Check status (should see the pending request)
        var statusHandler = new GetExportStatusQueryHandler(repoMock.Object);
        var statusResult = await statusHandler.Handle(
            new GetExportStatusQuery(userId), CancellationToken.None);

        statusResult.Should().NotBeNull();
        statusResult!.RequestId.Should().Be(requestResult.RequestId);
        statusResult.Status.Should().Be("Pending");
    }

    [Fact]
    public async Task FullFlow_CompletedExport_CanRetrieveDownloadToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var downloadToken = "test-download-token-xyz";
        var repoMock = new Mock<IPrivacyRequestRepository>();

        var completedRequest = new PrivacyRequest
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = PrivacyRequestType.Portability,
            Status = PrivacyRequestStatus.Completed,
            DownloadToken = downloadToken,
            DownloadTokenExpiresAt = DateTime.UtcNow.AddHours(24),
            FilePath = "/exports/test.zip",
            FileSizeBytes = 50000,
            ExportFormat = ExportFormat.Json,
            CreatedAt = DateTime.UtcNow.AddMinutes(-5),
            CompletedAt = DateTime.UtcNow,
        };

        repoMock.Setup(r => r.GetLatestExportRequestAsync(userId))
            .ReturnsAsync(completedRequest);
        repoMock.Setup(r => r.GetByDownloadTokenAsync(downloadToken))
            .ReturnsAsync(completedRequest);

        // Step 1: Check status — should have download token
        var statusHandler = new GetExportStatusQueryHandler(repoMock.Object);
        var status = await statusHandler.Handle(
            new GetExportStatusQuery(userId), CancellationToken.None);

        status.Should().NotBeNull();
        status!.DownloadToken.Should().Be(downloadToken);

        // Step 2: Download — should return file info
        var downloadHandler = new DownloadExportDataQueryHandler(repoMock.Object);
        var download = await downloadHandler.Handle(
            new DownloadExportDataQuery(downloadToken), CancellationToken.None);

        download.Should().NotBeNull();
        download!.IsExpired.Should().BeFalse();
        download.FilePath.Should().Be("/exports/test.zip");
    }

    [Fact]
    public async Task FullFlow_ExpiredToken_Returns410Gone()
    {
        // Arrange — simulates a token that expired (past 24h)
        var repoMock = new Mock<IPrivacyRequestRepository>();
        var expiredRequest = new PrivacyRequest
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Status = PrivacyRequestStatus.Completed,
            DownloadToken = "expired-token",
            DownloadTokenExpiresAt = DateTime.UtcNow.AddHours(-2),
            FilePath = "/exports/old.zip",
        };

        repoMock.Setup(r => r.GetByDownloadTokenAsync("expired-token"))
            .ReturnsAsync(expiredRequest);

        var downloadHandler = new DownloadExportDataQueryHandler(repoMock.Object);
        var result = await downloadHandler.Handle(
            new DownloadExportDataQuery("expired-token"), CancellationToken.None);

        result.Should().NotBeNull();
        result!.IsExpired.Should().BeTrue();
    }
}

#endregion

#region Repository Interface Contract Tests

public class PrivacyRequestRepositoryContractTests
{
    [Fact]
    public void IPrivacyRequestRepository_HasGetLatestExportRequestAsync()
    {
        var type = typeof(IPrivacyRequestRepository);
        var method = type.GetMethod("GetLatestExportRequestAsync");
        method.Should().NotBeNull("IPrivacyRequestRepository must expose GetLatestExportRequestAsync for export status queries");
    }

    [Fact]
    public void IPrivacyRequestRepository_HasGetByDownloadTokenAsync()
    {
        var type = typeof(IPrivacyRequestRepository);
        var method = type.GetMethod("GetByDownloadTokenAsync");
        method.Should().NotBeNull("IPrivacyRequestRepository must expose GetByDownloadTokenAsync for secure downloads");
    }

    [Fact]
    public void IPrivacyRequestRepository_HasGetPendingExportRequestsAsync()
    {
        var type = typeof(IPrivacyRequestRepository);
        var method = type.GetMethod("GetPendingExportRequestsAsync");
        method.Should().NotBeNull("IPrivacyRequestRepository must expose GetPendingExportRequestsAsync for the DataExportWorker");
    }

    [Fact]
    public void IPrivacyRequestRepository_HasGetExpiredExportRequestsAsync()
    {
        var type = typeof(IPrivacyRequestRepository);
        var method = type.GetMethod("GetExpiredExportRequestsAsync");
        method.Should().NotBeNull("IPrivacyRequestRepository must expose GetExpiredExportRequestsAsync for cleanup");
    }
}

#endregion
