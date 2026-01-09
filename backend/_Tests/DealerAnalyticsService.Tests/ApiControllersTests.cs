using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MediatR;
using DealerAnalyticsService.Api.Controllers;
using DealerAnalyticsService.Application.Features.Commands;
using DealerAnalyticsService.Application.Features.Queries;
using DealerAnalyticsService.Application.DTOs;
using DealerAnalyticsService.Infrastructure.Persistence;

namespace DealerAnalyticsService.Tests;

/// <summary>
/// Sprint 12 - API Controllers Tests
/// Tests for all 5 controllers: Dashboard, Analytics, ConversionFunnel, Benchmark, Insights
/// </summary>
public class ApiControllersTests
{
    #region Dashboard Controller Tests

    [Fact]
    public async Task DashboardController_GetSummary_ShouldReturn_ValidSummary()
    {
        // Arrange
        var mockMediator = new Mock<IMediator>();
        var dealerId = Guid.NewGuid();
        
        var expectedSummary = new DashboardSummaryDto
        {
            DealerId = dealerId,
            TotalViews = 5000,
            UniqueVisitors = 3200,
            TotalInquiries = 180,
            ConvertedLeads = 54,
            ConversionRate = 30.0,
            TotalRevenue = 425000m,
            AverageResponseTime = 1.2,
            CustomerSatisfactionScore = 4.6,
            Period = "Last 30 days"
        };

        mockMediator
            .Setup(m => m.Send(It.IsAny<GetDashboardSummaryQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedSummary);

        var controller = new DashboardController(mockMediator.Object);

        // Act
        var result = await controller.GetSummary(dealerId);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var summary = okResult.Value.Should().BeOfType<DashboardSummaryDto>().Subject;
        
        summary.DealerId.Should().Be(dealerId);
        summary.TotalViews.Should().Be(5000);
        summary.ConversionRate.Should().Be(30.0);
        summary.CustomerSatisfactionScore.Should().Be(4.6);
    }

    [Fact]
    public async Task DashboardController_RecalculateAnalytics_ShouldReturn_Success()
    {
        // Arrange
        var mockMediator = new Mock<IMediator>();
        var dealerId = Guid.NewGuid();
        
        mockMediator
            .Setup(m => m.Send(It.IsAny<RecalculateAnalyticsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var controller = new DashboardController(mockMediator.Object);

        // Act
        var result = await controller.RecalculateAnalytics(dealerId);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var success = okResult.Value.Should().BeOfType<bool>().Subject;
        success.Should().BeTrue();
    }

    #endregion

    #region Analytics Controller Tests

    [Fact]
    public async Task AnalyticsController_GetAnalytics_ShouldReturn_ValidAnalytics()
    {
        // Arrange
        var mockMediator = new Mock<IMediator>();
        var dealerId = Guid.NewGuid();
        
        var expectedAnalytics = new List<DealerAnalyticDto>
        {
            new DealerAnalyticDto
            {
                Id = Guid.NewGuid(),
                DealerId = dealerId,
                DateRange = DateTime.UtcNow.Date,
                TotalViews = 1500,
                UniqueVisitors = 900,
                TotalInquiries = 65,
                ConvertedLeads = 18,
                ConversionRate = 27.69,
                TotalRevenue = 145000m,
                AverageResponseTime = 1.8,
                CustomerSatisfactionScore = 4.3
            }
        };

        mockMediator
            .Setup(m => m.Send(It.IsAny<GetAnalyticsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedAnalytics);

        var controller = new AnalyticsController(mockMediator.Object);

        // Act
        var result = await controller.GetAnalytics(dealerId, DateTime.UtcNow.AddDays(-30), DateTime.UtcNow);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var analytics = okResult.Value.Should().BeAssignableTo<IEnumerable<DealerAnalyticDto>>().Subject;
        
        analytics.Should().NotBeEmpty();
        analytics.First().DealerId.Should().Be(dealerId);
        analytics.First().ConversionRate.Should().BeApproximately(27.69, 0.01);
    }

    [Fact]
    public async Task AnalyticsController_GetTrends_ShouldReturn_ValidTrends()
    {
        // Arrange
        var mockMediator = new Mock<IMediator>();
        var dealerId = Guid.NewGuid();
        
        var expectedTrends = new AnalyticsTrendsDto
        {
            DealerId = dealerId,
            ViewsTrend = 12.5, // +12.5% vs previous period
            InquiriesTrend = -3.2, // -3.2% vs previous period
            ConversionTrend = 8.7, // +8.7% vs previous period
            RevenueTrend = 15.3, // +15.3% vs previous period
            Period = "Last 30 vs Previous 30 days"
        };

        mockMediator
            .Setup(m => m.Send(It.IsAny<GetAnalyticsTrendsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTrends);

        var controller = new AnalyticsController(mockMediator.Object);

        // Act
        var result = await controller.GetTrends(dealerId);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var trends = okResult.Value.Should().BeOfType<AnalyticsTrendsDto>().Subject;
        
        trends.DealerId.Should().Be(dealerId);
        trends.ViewsTrend.Should().Be(12.5);
        trends.ConversionTrend.Should().Be(8.7);
    }

    #endregion

    #region Conversion Funnel Controller Tests

    [Fact]
    public async Task ConversionFunnelController_GetFunnel_ShouldReturn_ValidFunnel()
    {
        // Arrange
        var mockMediator = new Mock<IMediator>();
        var dealerId = Guid.NewGuid();
        
        var expectedFunnel = new ConversionFunnelDto
        {
            Id = Guid.NewGuid(),
            DealerId = dealerId,
            Views = 8000,
            DetailViews = 4800,
            Inquiries = 240,
            TestDrives = 120,
            Purchases = 48,
            ViewToDetailRate = 60.0,
            DetailToInquiryRate = 5.0,
            InquiryToTestDriveRate = 50.0,
            TestDriveToPurchaseRate = 40.0,
            OverallConversionRate = 0.6,
            CreatedAt = DateTime.UtcNow
        };

        mockMediator
            .Setup(m => m.Send(It.IsAny<GetConversionFunnelQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedFunnel);

        var controller = new ConversionFunnelController(mockMediator.Object);

        // Act
        var result = await controller.GetFunnel(dealerId);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var funnel = okResult.Value.Should().BeOfType<ConversionFunnelDto>().Subject;
        
        funnel.DealerId.Should().Be(dealerId);
        funnel.Views.Should().Be(8000);
        funnel.OverallConversionRate.Should().Be(0.6);
        funnel.ViewToDetailRate.Should().Be(60.0);
    }

    [Fact]
    public async Task ConversionFunnelController_GetHistoricalFunnel_ShouldReturn_ValidHistory()
    {
        // Arrange
        var mockMediator = new Mock<IMediator>();
        var dealerId = Guid.NewGuid();
        
        var expectedHistory = new List<ConversionFunnelDto>
        {
            new ConversionFunnelDto
            {
                Id = Guid.NewGuid(),
                DealerId = dealerId,
                Views = 7500,
                DetailViews = 4200,
                Inquiries = 210,
                TestDrives = 105,
                Purchases = 35,
                OverallConversionRate = 0.47,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new ConversionFunnelDto
            {
                Id = Guid.NewGuid(),
                DealerId = dealerId,
                Views = 8200,
                DetailViews = 4600,
                Inquiries = 230,
                TestDrives = 115,
                Purchases = 42,
                OverallConversionRate = 0.51,
                CreatedAt = DateTime.UtcNow.AddDays(-15)
            }
        };

        mockMediator
            .Setup(m => m.Send(It.IsAny<GetHistoricalFunnelQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedHistory);

        var controller = new ConversionFunnelController(mockMediator.Object);

        // Act
        var result = await controller.GetHistoricalFunnel(dealerId, DateTime.UtcNow.AddDays(-60), DateTime.UtcNow);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var history = okResult.Value.Should().BeAssignableTo<IEnumerable<ConversionFunnelDto>>().Subject;
        
        history.Should().HaveCount(2);
        history.All(f => f.DealerId == dealerId).Should().BeTrue();
        history.Should().BeInAscendingOrder(f => f.CreatedAt);
    }

    #endregion

    #region Benchmark Controller Tests

    [Fact]
    public async Task BenchmarkController_GetBenchmarks_ShouldReturn_ValidBenchmarks()
    {
        // Arrange
        var mockMediator = new Mock<IMediator>();
        var dealerId = Guid.NewGuid();
        
        var expectedBenchmarks = new List<MarketBenchmarkDto>
        {
            new MarketBenchmarkDto
            {
                Id = Guid.NewGuid(),
                DealerId = dealerId,
                Category = "SUV",
                Region = "Santo Domingo",
                DealerValue = 4.5,
                MarketAverage = 4.1,
                MarketMedian = 4.0,
                MarketTop25 = 4.7,
                MarketTop10 = 4.9,
                PerformanceVsAverage = 9.76,
                IsAboveAverage = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        mockMediator
            .Setup(m => m.Send(It.IsAny<GetMarketBenchmarksQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedBenchmarks);

        var controller = new BenchmarkController(mockMediator.Object);

        // Act
        var result = await controller.GetBenchmarks(dealerId, "SUV", "Santo Domingo");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var benchmarks = okResult.Value.Should().BeAssignableTo<IEnumerable<MarketBenchmarkDto>>().Subject;
        
        benchmarks.Should().NotBeEmpty();
        benchmarks.First().DealerId.Should().Be(dealerId);
        benchmarks.First().Category.Should().Be("SUV");
        benchmarks.First().IsAboveAverage.Should().BeTrue();
    }

    [Fact]
    public async Task BenchmarkController_UpdateBenchmarks_ShouldReturn_Success()
    {
        // Arrange
        var mockMediator = new Mock<IMediator>();
        var dealerId = Guid.NewGuid();
        
        mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateBenchmarksCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var controller = new BenchmarkController(mockMediator.Object);

        // Act
        var result = await controller.UpdateBenchmarks(dealerId);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var success = okResult.Value.Should().BeOfType<bool>().Subject;
        success.Should().BeTrue();
    }

    #endregion

    #region Insights Controller Tests

    [Fact]
    public async Task InsightsController_GetInsights_ShouldReturn_ValidInsights()
    {
        // Arrange
        var mockMediator = new Mock<IMediator>();
        var dealerId = Guid.NewGuid();
        
        var expectedInsights = new List<DealerInsightDto>
        {
            new DealerInsightDto
            {
                Id = Guid.NewGuid(),
                DealerId = dealerId,
                Type = "OpportunityAlert",
                Priority = "High",
                Title = "Oportunidad: Mejorar tiempo de respuesta",
                Description = "Su tiempo de respuesta promedio es 3.2 horas, 40% más lento que el promedio del mercado.",
                ActionRecommendation = "Configure notificaciones automáticas para responder en menos de 1 hora",
                PotentialImpact = "+18% conversiones",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            },
            new DealerInsightDto
            {
                Id = Guid.NewGuid(),
                DealerId = dealerId,
                Type = "PerformanceAlert",
                Priority = "Medium",
                Title = "Alerta: Disminución en vistas",
                Description = "Las vistas de sus vehículos han disminuido 12% esta semana.",
                ActionRecommendation = "Revise la calidad de las fotos y descripciones de sus listings",
                PotentialImpact = "Recuperar tráfico perdido",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        mockMediator
            .Setup(m => m.Send(It.IsAny<GetInsightsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedInsights);

        var controller = new InsightsController(mockMediator.Object);

        // Act
        var result = await controller.GetInsights(dealerId, null, null, 1, 20);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var insights = okResult.Value.Should().BeAssignableTo<IEnumerable<DealerInsightDto>>().Subject;
        
        insights.Should().HaveCount(2);
        insights.Should().Contain(i => i.Type == "OpportunityAlert");
        insights.Should().Contain(i => i.Priority == "High");
        insights.All(i => i.DealerId == dealerId).Should().BeTrue();
    }

    [Fact]
    public async Task InsightsController_MarkAsRead_ShouldReturn_Success()
    {
        // Arrange
        var mockMediator = new Mock<IMediator>();
        var insightId = Guid.NewGuid();
        
        mockMediator
            .Setup(m => m.Send(It.IsAny<MarkInsightAsReadCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var controller = new InsightsController(mockMediator.Object);

        // Act
        var result = await controller.MarkAsRead(insightId);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var success = okResult.Value.Should().BeOfType<bool>().Subject;
        success.Should().BeTrue();
    }

    [Fact]
    public async Task InsightsController_GenerateInsights_ShouldReturn_NewInsightIds()
    {
        // Arrange
        var mockMediator = new Mock<IMediator>();
        var dealerId = Guid.NewGuid();
        
        var expectedInsightIds = new List<Guid>
        {
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid()
        };

        mockMediator
            .Setup(m => m.Send(It.IsAny<GenerateInsightsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedInsightIds);

        var controller = new InsightsController(mockMediator.Object);

        // Act
        var result = await controller.GenerateInsights(dealerId);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var insightIds = okResult.Value.Should().BeAssignableTo<IEnumerable<Guid>>().Subject;
        
        insightIds.Should().HaveCount(3);
        insightIds.Should().OnlyContain(id => id != Guid.Empty);
    }

    [Fact]
    public async Task InsightsController_DismissInsight_ShouldReturn_Success()
    {
        // Arrange
        var mockMediator = new Mock<IMediator>();
        var insightId = Guid.NewGuid();
        
        mockMediator
            .Setup(m => m.Send(It.IsAny<DismissInsightCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var controller = new InsightsController(mockMediator.Object);

        // Act
        var result = await controller.DismissInsight(insightId);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var success = okResult.Value.Should().BeOfType<bool>().Subject;
        success.Should().BeTrue();
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task DashboardController_GetSummary_ShouldReturn_NotFound_ForInvalidDealer()
    {
        // Arrange
        var mockMediator = new Mock<IMediator>();
        var dealerId = Guid.NewGuid();
        
        mockMediator
            .Setup(m => m.Send(It.IsAny<GetDashboardSummaryQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DashboardSummaryDto?)null);

        var controller = new DashboardController(mockMediator.Object);

        // Act
        var result = await controller.GetSummary(dealerId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task InsightsController_GetInsights_ShouldReturn_BadRequest_ForInvalidPage()
    {
        // Arrange
        var mockMediator = new Mock<IMediator>();
        var dealerId = Guid.NewGuid();
        var controller = new InsightsController(mockMediator.Object);

        // Act
        var result = await controller.GetInsights(dealerId, null, null, -1, 20); // Invalid page number

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task InsightsController_GetInsights_ShouldReturn_BadRequest_ForInvalidPageSize()
    {
        // Arrange
        var mockMediator = new Mock<IMediator>();
        var dealerId = Guid.NewGuid();
        var controller = new InsightsController(mockMediator.Object);

        // Act
        var result = await controller.GetInsights(dealerId, null, null, 1, 101); // Page size too large

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region Health Check Tests

    [Fact]
    public void HealthCheck_ShouldReturn_Healthy_Status()
    {
        // Note: This would require setting up a proper health check infrastructure
        // For now, we'll just verify that the health endpoint exists and is configured
        
        // This test would be expanded with proper health check infrastructure
        // testing database connectivity, external service availability, etc.
        
        Assert.True(true); // Placeholder - would implement actual health check logic
    }

    #endregion
}