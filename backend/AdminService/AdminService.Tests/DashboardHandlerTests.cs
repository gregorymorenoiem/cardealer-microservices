using AdminService.Application.Interfaces;
using AdminService.Application.UseCases.Dashboard;
using AdminService.Domain.Entities;
using AdminService.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AdminService.Tests.Application.UseCases.Dashboard;

public class GetDashboardStatsQueryHandlerTests
{
    private readonly Mock<IStatisticsRepository> _statisticsRepoMock;
    private readonly Mock<IModerationRepository> _moderationRepoMock;
    private readonly Mock<IDealerService> _dealerServiceMock;
    private readonly Mock<IPlatformUserService> _userServiceMock;
    private readonly IMemoryCache _memoryCache;
    private readonly Mock<ILogger<GetDashboardStatsQueryHandler>> _loggerMock;
    private readonly GetDashboardStatsQueryHandler _handler;

    public GetDashboardStatsQueryHandlerTests()
    {
        _statisticsRepoMock = new Mock<IStatisticsRepository>();
        _moderationRepoMock = new Mock<IModerationRepository>();
        _dealerServiceMock = new Mock<IDealerService>();
        _userServiceMock = new Mock<IPlatformUserService>();
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _loggerMock = new Mock<ILogger<GetDashboardStatsQueryHandler>>();

        _handler = new GetDashboardStatsQueryHandler(
            _statisticsRepoMock.Object,
            _moderationRepoMock.Object,
            _dealerServiceMock.Object,
            _userServiceMock.Object,
            _memoryCache,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsAggregatedStats()
    {
        // Arrange
        _statisticsRepoMock.Setup(x => x.GetPlatformStatsAsync())
            .ReturnsAsync(new PlatformStats
            {
                TotalListings = 500,
                PendingListings = 25,
                OpenTickets = 8
            });

        _dealerServiceMock.Setup(x => x.GetDealerStatsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AdminService.Application.UseCases.Dealers.DealerStatsDto
            {
                Total = 50,
                Active = 40,
                Pending = 10,
                TotalMrr = 4500m
            });

        _userServiceMock.Setup(x => x.GetUserStatsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AdminService.Application.UseCases.PlatformUsers.PlatformUserStatsDto
            {
                Total = 2000,
                Active = 1800
            });

        _moderationRepoMock.Setup(x => x.GetPendingReportsAsync(100))
            .ReturnsAsync(new List<PendingReportInfo>
            {
                new() { Id = Guid.NewGuid(), Type = "spam" },
                new() { Id = Guid.NewGuid(), Type = "fraud" },
                new() { Id = Guid.NewGuid(), Type = "inappropriate" }
            });

        var query = new GetDashboardStatsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2000, result.TotalUsers);
        Assert.Equal(500, result.TotalVehicles);
        Assert.Equal(475, result.ActiveVehicles); // 500 - 25
        Assert.Equal(50, result.TotalDealers);
        Assert.Equal(40, result.ActiveDealers);
        Assert.Equal(10, result.PendingApprovals);
        Assert.Equal(25, result.PendingVerifications);
        Assert.Equal(3, result.TotalReports);
        Assert.Equal(8, result.OpenSupportTickets);
        Assert.Equal(4500m, result.Mrr);
    }

    [Fact]
    public async Task Handle_EmptyData_ReturnsZeroStats()
    {
        // Arrange
        _statisticsRepoMock.Setup(x => x.GetPlatformStatsAsync())
            .ReturnsAsync(new PlatformStats());

        _dealerServiceMock.Setup(x => x.GetDealerStatsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AdminService.Application.UseCases.Dealers.DealerStatsDto());

        _userServiceMock.Setup(x => x.GetUserStatsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AdminService.Application.UseCases.PlatformUsers.PlatformUserStatsDto());

        _moderationRepoMock.Setup(x => x.GetPendingReportsAsync(100))
            .ReturnsAsync(Enumerable.Empty<PendingReportInfo>());

        var query = new GetDashboardStatsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.TotalUsers);
        Assert.Equal(0, result.TotalVehicles);
        Assert.Equal(0, result.TotalDealers);
        Assert.Equal(0m, result.Mrr);
    }

    [Fact]
    public async Task Handle_AllServicesCalled_InParallel()
    {
        // Arrange
        _statisticsRepoMock.Setup(x => x.GetPlatformStatsAsync())
            .ReturnsAsync(new PlatformStats { TotalListings = 100 });

        _dealerServiceMock.Setup(x => x.GetDealerStatsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AdminService.Application.UseCases.Dealers.DealerStatsDto { Total = 10 });

        _userServiceMock.Setup(x => x.GetUserStatsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AdminService.Application.UseCases.PlatformUsers.PlatformUserStatsDto { Total = 500 });

        _moderationRepoMock.Setup(x => x.GetPendingReportsAsync(100))
            .ReturnsAsync(Enumerable.Empty<PendingReportInfo>());

        var query = new GetDashboardStatsQuery();

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert — verify all 4 service calls were made
        _statisticsRepoMock.Verify(x => x.GetPlatformStatsAsync(), Times.Once);
        _dealerServiceMock.Verify(x => x.GetDealerStatsAsync(It.IsAny<CancellationToken>()), Times.Once);
        _userServiceMock.Verify(x => x.GetUserStatsAsync(It.IsAny<CancellationToken>()), Times.Once);
        _moderationRepoMock.Verify(x => x.GetPendingReportsAsync(100), Times.Once);
    }
}

public class GetDashboardActivityQueryHandlerTests
{
    private readonly Mock<IAdminActionLogRepository> _actionLogRepoMock;
    private readonly Mock<ILogger<GetDashboardActivityQueryHandler>> _loggerMock;
    private readonly GetDashboardActivityQueryHandler _handler;

    public GetDashboardActivityQueryHandlerTests()
    {
        _actionLogRepoMock = new Mock<IAdminActionLogRepository>();
        _loggerMock = new Mock<ILogger<GetDashboardActivityQueryHandler>>();

        _handler = new GetDashboardActivityQueryHandler(
            _actionLogRepoMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task Handle_ReturnsRecentActivity()
    {
        // Arrange
        var logs = new List<AdminActionLog>
        {
            new()
            {
                Id = Guid.NewGuid(),
                AdminId = Guid.NewGuid(),
                Action = "vehicle.approved",
                Description = "Approved vehicle listing",
                TargetType = "vehicle",
                TargetId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow.AddMinutes(-5)
            },
            new()
            {
                Id = Guid.NewGuid(),
                AdminId = Guid.NewGuid(),
                Action = "dealer.verified",
                Description = "Verified dealer account",
                TargetType = "dealer",
                TargetId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow.AddMinutes(-15)
            }
        };

        _actionLogRepoMock.Setup(x => x.GetRecentAsync(10))
            .ReturnsAsync(logs);

        var query = new GetDashboardActivityQuery(10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("vehicle.approved", result[0].Action);
        Assert.Equal("vehicle", result[0].SubjectType);
        Assert.Equal("dealer.verified", result[1].Action);
        Assert.Equal("dealer", result[1].SubjectType);
    }

    [Fact]
    public async Task Handle_EmptyLogs_ReturnsEmptyList()
    {
        // Arrange
        _actionLogRepoMock.Setup(x => x.GetRecentAsync(10))
            .ReturnsAsync(Enumerable.Empty<AdminActionLog>());

        var query = new GetDashboardActivityQuery(10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Theory]
    [InlineData("user", "user")]
    [InlineData("dealer", "dealer")]
    [InlineData("vehicle", "vehicle")]
    [InlineData("listing", "vehicle")]
    [InlineData("payment", "payment")]
    [InlineData("transaction", "payment")]
    [InlineData("report", "report")]
    [InlineData("unknown", "user")]
    [InlineData(null, "user")]
    public async Task Handle_MapsTargetTypeToSubjectType_Correctly(string? targetType, string expectedSubjectType)
    {
        // Arrange
        var logs = new List<AdminActionLog>
        {
            new()
            {
                Id = Guid.NewGuid(),
                AdminId = Guid.NewGuid(),
                Action = "test.action",
                TargetType = targetType,
                TargetId = "123",
                Timestamp = DateTime.UtcNow
            }
        };

        _actionLogRepoMock.Setup(x => x.GetRecentAsync(1))
            .ReturnsAsync(logs);

        var query = new GetDashboardActivityQuery(1);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Single(result);
        Assert.Equal(expectedSubjectType, result[0].SubjectType);
    }
}

public class GetDashboardPendingQueryHandlerTests
{
    private readonly Mock<IStatisticsRepository> _statisticsRepoMock;
    private readonly Mock<IModerationRepository> _moderationRepoMock;
    private readonly Mock<IDealerService> _dealerServiceMock;
    private readonly Mock<ILogger<GetDashboardPendingQueryHandler>> _loggerMock;
    private readonly GetDashboardPendingQueryHandler _handler;

    public GetDashboardPendingQueryHandlerTests()
    {
        _statisticsRepoMock = new Mock<IStatisticsRepository>();
        _moderationRepoMock = new Mock<IModerationRepository>();
        _dealerServiceMock = new Mock<IDealerService>();
        _loggerMock = new Mock<ILogger<GetDashboardPendingQueryHandler>>();

        _handler = new GetDashboardPendingQueryHandler(
            _statisticsRepoMock.Object,
            _moderationRepoMock.Object,
            _dealerServiceMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task Handle_WithPendingItems_ReturnsAllPendingActions()
    {
        // Arrange
        _statisticsRepoMock.Setup(x => x.GetPlatformStatsAsync())
            .ReturnsAsync(new PlatformStats
            {
                PendingListings = 15,
                OpenTickets = 25
            });

        _dealerServiceMock.Setup(x => x.GetDealerStatsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AdminService.Application.UseCases.Dealers.DealerStatsDto { Pending = 8 });

        _moderationRepoMock.Setup(x => x.GetPendingReportsAsync(100))
            .ReturnsAsync(Enumerable.Range(0, 12).Select(_ => new PendingReportInfo
            {
                Id = Guid.NewGuid(),
                Type = "spam"
            }));

        var query = new GetDashboardPendingQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(4, result.Count); // moderation, verification, report, support

        // Vehicles > 10 → high priority
        var vehicleAction = result.First(x => x.Type == "moderation");
        Assert.Equal("high", vehicleAction.Priority);
        Assert.Equal(15, vehicleAction.Count);

        // Dealers > 5 → high priority
        var dealerAction = result.First(x => x.Type == "verification");
        Assert.Equal("high", dealerAction.Priority);
        Assert.Equal(8, dealerAction.Count);

        // Reports > 10 → high priority
        var reportAction = result.First(x => x.Type == "report");
        Assert.Equal("high", reportAction.Priority);
        Assert.Equal(12, reportAction.Count);

        // Tickets > 20 → high priority
        var supportAction = result.First(x => x.Type == "support");
        Assert.Equal("high", supportAction.Priority);
        Assert.Equal(25, supportAction.Count);
    }

    [Fact]
    public async Task Handle_NoPendingItems_ReturnsEmptyList()
    {
        // Arrange
        _statisticsRepoMock.Setup(x => x.GetPlatformStatsAsync())
            .ReturnsAsync(new PlatformStats
            {
                PendingListings = 0,
                OpenTickets = 0
            });

        _dealerServiceMock.Setup(x => x.GetDealerStatsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AdminService.Application.UseCases.Dealers.DealerStatsDto { Pending = 0 });

        _moderationRepoMock.Setup(x => x.GetPendingReportsAsync(100))
            .ReturnsAsync(Enumerable.Empty<PendingReportInfo>());

        var query = new GetDashboardPendingQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_LowCounts_ReturnsMediumOrLowPriority()
    {
        // Arrange
        _statisticsRepoMock.Setup(x => x.GetPlatformStatsAsync())
            .ReturnsAsync(new PlatformStats
            {
                PendingListings = 3,
                OpenTickets = 5
            });

        _dealerServiceMock.Setup(x => x.GetDealerStatsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AdminService.Application.UseCases.Dealers.DealerStatsDto { Pending = 2 });

        _moderationRepoMock.Setup(x => x.GetPendingReportsAsync(100))
            .ReturnsAsync(new List<PendingReportInfo>
            {
                new() { Id = Guid.NewGuid(), Type = "spam" }
            });

        var query = new GetDashboardPendingQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(4, result.Count);

        // PendingListings = 3, <= 10 → medium
        Assert.Equal("medium", result.First(x => x.Type == "moderation").Priority);
        // Pending dealers = 2, <= 5 → medium
        Assert.Equal("medium", result.First(x => x.Type == "verification").Priority);
        // Reports = 1, <= 10 → medium
        Assert.Equal("medium", result.First(x => x.Type == "report").Priority);
        // OpenTickets = 5, <= 20 → low
        Assert.Equal("low", result.First(x => x.Type == "support").Priority);
    }
}
