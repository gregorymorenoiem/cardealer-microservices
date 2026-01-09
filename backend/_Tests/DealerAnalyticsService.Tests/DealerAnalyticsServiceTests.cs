using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.EntityFrameworkCore;
using MediatR;
using DealerAnalyticsService.Domain.Entities;
using DealerAnalyticsService.Domain.Interfaces;
using DealerAnalyticsService.Application.DTOs;
using DealerAnalyticsService.Application.Features.Commands;
using DealerAnalyticsService.Application.Features.Queries;
using DealerAnalyticsService.Infrastructure.Persistence;

namespace DealerAnalyticsService.Tests;

/// <summary>
/// Sprint 12 - Dashboard Avanzado: Tests Completos del DealerAnalyticsService
/// Tests for Domain Entities, Application Queries/Commands, and Infrastructure
/// </summary>
public class DealerAnalyticsServiceTests
{
    #region Domain Tests - Entities and Business Logic

    [Fact]
    public void DealerAnalytic_ShouldBeCreated_WithValidData()
    {
        // Arrange
        var dealerId = Guid.NewGuid();
        var dateRange = DateTime.UtcNow.Date;

        // Act
        var analytic = new DealerAnalytic
        {
            Id = Guid.NewGuid(),
            DealerId = dealerId,
            DateRange = dateRange,
            TotalViews = 1250,
            UniqueVisitors = 850,
            TotalInquiries = 45,
            ConvertedLeads = 12,
            TotalRevenue = 125000.50m,
            AverageResponseTime = 2.5,
            CustomerSatisfactionScore = 4.7,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Assert
        analytic.Should().NotBeNull();
        analytic.DealerId.Should().Be(dealerId);
        analytic.TotalViews.Should().Be(1250);
        analytic.ConversionRate.Should().BeApproximately(26.67, 0.01); // 12/45 * 100
        analytic.ViewToInquiryRate.Should().BeApproximately(3.6, 0.01); // 45/1250 * 100
    }

    [Fact]
    public void ConversionFunnel_ShouldCalculateCorrectRates()
    {
        // Arrange & Act
        var funnel = new ConversionFunnel
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            Views = 5000,
            DetailViews = 2500,
            Inquiries = 150,
            TestDrives = 75,
            Purchases = 25,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        funnel.ViewToDetailRate.Should().Be(50.0); // 2500/5000 * 100
        funnel.DetailToInquiryRate.Should().Be(6.0); // 150/2500 * 100
        funnel.InquiryToTestDriveRate.Should().Be(50.0); // 75/150 * 100
        funnel.TestDriveToPurchaseRate.Should().BeApproximately(33.33, 0.01); // 25/75 * 100
        funnel.OverallConversionRate.Should().Be(0.5); // 25/5000 * 100
    }

    [Fact]
    public void MarketBenchmark_ShouldHaveValidComparison()
    {
        // Arrange & Act
        var benchmark = new MarketBenchmark
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            Category = "SUV",
            Region = "Santo Domingo",
            DealerValue = 4.2,
            MarketAverage = 3.8,
            MarketMedian = 3.9,
            MarketTop25 = 4.5,
            MarketTop10 = 4.8,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        benchmark.Should().NotBeNull();
        benchmark.PerformanceVsAverage.Should().BeApproximately(10.53, 0.01); // ((4.2-3.8)/3.8)*100
        benchmark.PerformanceVsMedian.Should().BeApproximately(7.69, 0.01); // ((4.2-3.9)/3.9)*100
        benchmark.IsAboveAverage.Should().BeTrue();
        benchmark.IsInTop25.Should().BeFalse(); // 4.2 < 4.5
    }

    [Fact]
    public void DealerInsight_ShouldBeCreated_WithCorrectPriority()
    {
        // Arrange & Act
        var insight = new DealerInsight
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            Type = Domain.Enums.InsightType.OpportunityAlert,
            Priority = Domain.Enums.InsightPriority.High,
            Title = "Oportunidad: Incrementar respuesta a consultas",
            Description = "Su tiempo de respuesta promedio es 4.2 horas, 65% más lento que el promedio del mercado. Responder más rápido puede incrementar conversiones en 23%.",
            ActionRecommendation = "Configure notificaciones push para responder consultas en menos de 1 hora",
            PotentialImpact = "+23% conversiones",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        insight.Should().NotBeNull();
        insight.Type.Should().Be(Domain.Enums.InsightType.OpportunityAlert);
        insight.Priority.Should().Be(Domain.Enums.InsightPriority.High);
        insight.IsRead.Should().BeFalse();
        insight.Title.Should().NotBeNullOrEmpty();
        insight.ActionRecommendation.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Application Layer Tests - Commands and Queries

    [Fact]
    public async Task GetDashboardSummaryQuery_ShouldReturnSummary_WithValidDealerId()
    {
        // Arrange
        var dealerId = Guid.NewGuid();
        var options = new DbContextOptionsBuilder<DealerAnalyticsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new DealerAnalyticsDbContext(options);
        
        // Seed test data
        var analytic = new DealerAnalytic
        {
            Id = Guid.NewGuid(),
            DealerId = dealerId,
            DateRange = DateTime.UtcNow.Date,
            TotalViews = 1000,
            UniqueVisitors = 600,
            TotalInquiries = 30,
            ConvertedLeads = 8,
            TotalRevenue = 85000m,
            AverageResponseTime = 1.8,
            CustomerSatisfactionScore = 4.5,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.DealerAnalytics.Add(analytic);
        await context.SaveChangesAsync();

        var repository = new Infrastructure.Persistence.Repositories.DealerAnalyticRepository(context);
        var query = new GetDashboardSummaryQuery { DealerId = dealerId };
        var handler = new Application.Features.Queries.GetDashboardSummaryQueryHandler(repository);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TotalViews.Should().Be(1000);
        result.ConversionRate.Should().BeApproximately(26.67, 0.01); // 8/30 * 100
        result.CustomerSatisfactionScore.Should().Be(4.5);
    }

    [Fact]
    public async Task RecalculateAnalyticsCommand_ShouldUpdateAnalytics_ForValidDealer()
    {
        // Arrange
        var dealerId = Guid.NewGuid();
        var options = new DbContextOptionsBuilder<DealerAnalyticsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new DealerAnalyticsDbContext(options);
        var repository = new Infrastructure.Persistence.Repositories.DealerAnalyticRepository(context);
        
        var command = new RecalculateAnalyticsCommand 
        { 
            DealerId = dealerId,
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow
        };

        var handler = new Application.Features.Commands.RecalculateAnalyticsCommandHandler(repository);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        
        // Verify analytics were created (would be based on actual vehicle/inquiry data in real implementation)
        var analytics = await repository.GetByDealerIdAsync(dealerId);
        analytics.Should().NotBeNull(); // In real scenario, this would contain recalculated data
    }

    [Fact]
    public async Task GenerateInsightsCommand_ShouldCreateInsights_BasedOnAnalytics()
    {
        // Arrange
        var dealerId = Guid.NewGuid();
        var options = new DbContextOptionsBuilder<DealerAnalyticsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new DealerAnalyticsDbContext(options);
        
        // Add sample analytic data
        context.DealerAnalytics.Add(new DealerAnalytic
        {
            Id = Guid.NewGuid(),
            DealerId = dealerId,
            DateRange = DateTime.UtcNow.Date,
            TotalViews = 500,
            TotalInquiries = 50,
            ConvertedLeads = 5,
            AverageResponseTime = 6.5, // Slow response time should generate insight
            CustomerSatisfactionScore = 3.2, // Low score should generate insight
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();

        var analyticRepository = new Infrastructure.Persistence.Repositories.DealerAnalyticRepository(context);
        var insightRepository = new Infrastructure.Persistence.Repositories.DealerInsightRepository(context);
        
        var command = new GenerateInsightsCommand { DealerId = dealerId };
        var handler = new Application.Features.Commands.GenerateInsightsCommandHandler(
            analyticRepository, insightRepository);

        // Act
        var insightIds = await handler.Handle(command, CancellationToken.None);

        // Assert
        insightIds.Should().NotBeEmpty();
        
        var insights = await insightRepository.GetByDealerIdAsync(dealerId);
        insights.Should().NotBeEmpty();
        insights.Should().Contain(i => i.Type == Domain.Enums.InsightType.OpportunityAlert);
    }

    #endregion

    #region Repository Tests - Infrastructure Layer

    [Fact]
    public async Task DealerAnalyticRepository_ShouldSaveAndRetrieve_Analytics()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DealerAnalyticsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new DealerAnalyticsDbContext(options);
        var repository = new Infrastructure.Persistence.Repositories.DealerAnalyticRepository(context);
        
        var dealerId = Guid.NewGuid();
        var analytic = new DealerAnalytic
        {
            Id = Guid.NewGuid(),
            DealerId = dealerId,
            DateRange = DateTime.UtcNow.Date,
            TotalViews = 2000,
            UniqueVisitors = 1200,
            TotalInquiries = 80,
            ConvertedLeads = 20,
            TotalRevenue = 150000m,
            AverageResponseTime = 1.2,
            CustomerSatisfactionScore = 4.8,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        await repository.AddAsync(analytic);
        await repository.SaveChangesAsync();
        
        var retrievedAnalytic = await repository.GetByDealerIdAsync(dealerId);

        // Assert
        retrievedAnalytic.Should().NotBeNull();
        retrievedAnalytic!.TotalViews.Should().Be(2000);
        retrievedAnalytic.ConversionRate.Should().Be(25.0); // 20/80 * 100
    }

    [Fact]
    public async Task ConversionFunnelRepository_ShouldCalculateAggregates_Correctly()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DealerAnalyticsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new DealerAnalyticsDbContext(options);
        var repository = new Infrastructure.Persistence.Repositories.ConversionFunnelRepository(context);
        
        var dealerId = Guid.NewGuid();
        var funnel = new ConversionFunnel
        {
            Id = Guid.NewGuid(),
            DealerId = dealerId,
            Views = 10000,
            DetailViews = 6000,
            Inquiries = 300,
            TestDrives = 120,
            Purchases = 40,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        await repository.AddAsync(funnel);
        await repository.SaveChangesAsync();
        
        var retrievedFunnel = await repository.GetByDealerIdAsync(dealerId);

        // Assert
        retrievedFunnel.Should().NotBeNull();
        retrievedFunnel!.OverallConversionRate.Should().Be(0.4); // 40/10000 * 100
        retrievedFunnel.ViewToDetailRate.Should().Be(60.0); // 6000/10000 * 100
    }

    [Fact]
    public async Task DealerInsightRepository_ShouldFilterBy_Priority()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DealerAnalyticsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new DealerAnalyticsDbContext(options);
        var repository = new Infrastructure.Persistence.Repositories.DealerInsightRepository(context);
        
        var dealerId = Guid.NewGuid();
        
        var highPriorityInsight = new DealerInsight
        {
            Id = Guid.NewGuid(),
            DealerId = dealerId,
            Type = Domain.Enums.InsightType.PerformanceAlert,
            Priority = Domain.Enums.InsightPriority.High,
            Title = "Urgent: Conversión muy baja",
            Description = "Sus conversiones han bajado 45% esta semana",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        var mediumPriorityInsight = new DealerInsight
        {
            Id = Guid.NewGuid(),
            DealerId = dealerId,
            Type = Domain.Enums.InsightType.OpportunityAlert,
            Priority = Domain.Enums.InsightPriority.Medium,
            Title = "Mejore su tiempo de respuesta",
            Description = "Puede mejorar conversiones respondiendo más rápido",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        context.DealerInsights.AddRange(highPriorityInsight, mediumPriorityInsight);
        await context.SaveChangesAsync();

        // Act
        var highPriorityInsights = await repository.GetByPriorityAsync(dealerId, Domain.Enums.InsightPriority.High);
        var allInsights = await repository.GetByDealerIdAsync(dealerId);

        // Assert
        highPriorityInsights.Should().HaveCount(1);
        highPriorityInsights.First().Priority.Should().Be(Domain.Enums.InsightPriority.High);
        allInsights.Should().HaveCount(2);
    }

    #endregion

    #region Integration Tests - Full Workflow

    [Fact]
    public async Task FullAnalyticsWorkflow_ShouldWork_EndToEnd()
    {
        // Arrange
        var dealerId = Guid.NewGuid();
        var options = new DbContextOptionsBuilder<DealerAnalyticsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new DealerAnalyticsDbContext(options);
        
        var analyticRepo = new Infrastructure.Persistence.Repositories.DealerAnalyticRepository(context);
        var funnelRepo = new Infrastructure.Persistence.Repositories.ConversionFunnelRepository(context);
        var benchmarkRepo = new Infrastructure.Persistence.Repositories.MarketBenchmarkRepository(context);
        var insightRepo = new Infrastructure.Persistence.Repositories.DealerInsightRepository(context);

        // Step 1: Add analytic data
        var analytic = new DealerAnalytic
        {
            Id = Guid.NewGuid(),
            DealerId = dealerId,
            DateRange = DateTime.UtcNow.Date,
            TotalViews = 3000,
            UniqueVisitors = 1800,
            TotalInquiries = 120,
            ConvertedLeads = 36,
            TotalRevenue = 280000m,
            AverageResponseTime = 0.8,
            CustomerSatisfactionScore = 4.9,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Step 2: Add funnel data
        var funnel = new ConversionFunnel
        {
            Id = Guid.NewGuid(),
            DealerId = dealerId,
            Views = 3000,
            DetailViews = 2100,
            Inquiries = 120,
            TestDrives = 60,
            Purchases = 36,
            CreatedAt = DateTime.UtcNow
        };

        // Step 3: Add benchmark data
        var benchmark = new MarketBenchmark
        {
            Id = Guid.NewGuid(),
            DealerId = dealerId,
            Category = "Sedan",
            Region = "Santiago",
            DealerValue = 4.9,
            MarketAverage = 4.2,
            MarketMedian = 4.1,
            MarketTop25 = 4.6,
            MarketTop10 = 4.8,
            CreatedAt = DateTime.UtcNow
        };

        await analyticRepo.AddAsync(analytic);
        await funnelRepo.AddAsync(funnel);
        await benchmarkRepo.AddAsync(benchmark);
        await context.SaveChangesAsync();

        // Act - Retrieve dashboard summary
        var dashboardQuery = new GetDashboardSummaryQuery { DealerId = dealerId };
        var dashboardHandler = new Application.Features.Queries.GetDashboardSummaryQueryHandler(analyticRepo);
        var summary = await dashboardHandler.Handle(dashboardQuery, CancellationToken.None);

        // Assert - Verify complete workflow
        summary.Should().NotBeNull();
        summary.TotalViews.Should().Be(3000);
        summary.ConversionRate.Should().Be(30.0); // 36/120 * 100
        summary.CustomerSatisfactionScore.Should().Be(4.9);

        // Verify funnel exists
        var retrievedFunnel = await funnelRepo.GetByDealerIdAsync(dealerId);
        retrievedFunnel.Should().NotBeNull();
        retrievedFunnel!.OverallConversionRate.Should().Be(1.2); // 36/3000 * 100

        // Verify benchmark exists
        var retrievedBenchmark = await benchmarkRepo.GetByDealerIdAsync(dealerId);
        retrievedBenchmark.Should().NotBeNull();
        retrievedBenchmark!.IsAboveAverage.Should().BeTrue(); // 4.9 > 4.2
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task DealerAnalytics_ShouldHandle_LargeDataSet()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DealerAnalyticsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new DealerAnalyticsDbContext(options);
        var repository = new Infrastructure.Persistence.Repositories.DealerAnalyticRepository(context);

        var dealerId = Guid.NewGuid();
        var analytics = new List<DealerAnalytic>();

        // Create 365 days of analytics (1 year)
        for (int i = 0; i < 365; i++)
        {
            analytics.Add(new DealerAnalytic
            {
                Id = Guid.NewGuid(),
                DealerId = dealerId,
                DateRange = DateTime.UtcNow.AddDays(-i).Date,
                TotalViews = Random.Shared.Next(100, 1000),
                UniqueVisitors = Random.Shared.Next(50, 600),
                TotalInquiries = Random.Shared.Next(5, 50),
                ConvertedLeads = Random.Shared.Next(1, 15),
                TotalRevenue = Random.Shared.Next(10000, 100000),
                AverageResponseTime = Random.Shared.NextDouble() * 5,
                CustomerSatisfactionScore = 3.0 + Random.Shared.NextDouble() * 2,
                CreatedAt = DateTime.UtcNow.AddDays(-i),
                UpdatedAt = DateTime.UtcNow.AddDays(-i)
            });
        }

        context.DealerAnalytics.AddRange(analytics);
        await context.SaveChangesAsync();

        // Act - Test performance of large data retrieval
        var startTime = DateTime.UtcNow;
        var result = await repository.GetAnalyticsByDateRangeAsync(
            dealerId, 
            DateTime.UtcNow.AddDays(-30), 
            DateTime.UtcNow);
        var endTime = DateTime.UtcNow;
        var executionTime = endTime - startTime;

        // Assert
        result.Should().NotBeEmpty();
        result.Should().HaveCountLessOrEqualTo(30); // Last 30 days
        executionTime.Should().BeLessThan(TimeSpan.FromSeconds(2)); // Should be fast
    }

    #endregion
}