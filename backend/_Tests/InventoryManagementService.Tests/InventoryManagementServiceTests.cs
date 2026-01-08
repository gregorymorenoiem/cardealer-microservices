using Xunit;
using FluentAssertions;
using InventoryManagementService.Domain.Entities;

namespace InventoryManagementService.Tests;

public class InventoryManagementServiceTests
{
    [Fact]
    public void InventoryItem_ShouldBeCreated_WithValidData()
    {
        // Arrange
        var dealerId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var listPrice = 25000m;

        // Act
        var item = new InventoryItem
        {
            Id = Guid.NewGuid(),
            DealerId = dealerId,
            VehicleId = vehicleId,
            Status = InventoryStatus.Active,
            ListPrice = listPrice,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        item.Should().NotBeNull();
        item.DealerId.Should().Be(dealerId);
        item.VehicleId.Should().Be(vehicleId);
        item.ListPrice.Should().Be(listPrice);
        item.Status.Should().Be(InventoryStatus.Active);
    }

    [Fact]
    public void InventoryItem_ShouldCalculateDaysOnMarket()
    {
        // Arrange
        var item = new InventoryItem
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            VehicleId = Guid.NewGuid(),
            ListPrice = 25000m,
            CreatedAt = DateTime.UtcNow.AddDays(-45)
        };

        // Act
        var days = item.DaysOnMarket;

        // Assert
        days.Should().Be(45);
    }

    [Fact]
    public void InventoryItem_ShouldMarkAsSold()
    {
        // Arrange
        var item = new InventoryItem
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            VehicleId = Guid.NewGuid(),
            ListPrice = 25000m,
            Status = InventoryStatus.Active
        };
        var soldPrice = 23000m;
        var soldTo = "John Doe";

        // Act
        item.MarkAsSold(soldPrice, soldTo);

        // Assert
        item.Status.Should().Be(InventoryStatus.Sold);
        item.SoldPrice.Should().Be(soldPrice);
        item.SoldTo.Should().Be(soldTo);
        item.SoldAt.Should().NotBeNull();
    }

    [Fact]
    public void InventoryItem_ShouldCalculateProfit_WhenSold()
    {
        // Arrange
        var item = new InventoryItem
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            VehicleId = Guid.NewGuid(),
            CostPrice = 20000m,
            ListPrice = 25000m,
            Status = InventoryStatus.Active
        };
        var soldPrice = 23000m;

        // Act
        item.MarkAsSold(soldPrice, "Test Buyer");
        var profit = soldPrice - item.CostPrice;

        // Assert
        profit.Should().Be(3000m); // 23000 - 20000
    }

    [Fact]
    public void InventoryItem_ShouldBeHot_WhenHighActivity()
    {
        // Arrange
        var item = new InventoryItem
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            VehicleId = Guid.NewGuid(),
            ListPrice = 25000m,
            ViewCount = 150,
            InquiryCount = 12,
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        };

        // Act
        var isHot = item.IsHot;

        // Assert
        isHot.Should().BeTrue(); // More than 10 views/day AND more than 10 inquiries
    }

    [Fact]
    public void InventoryItem_ShouldBeOverdue_After90Days()
    {
        // Arrange
        var item = new InventoryItem
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            VehicleId = Guid.NewGuid(),
            ListPrice = 25000m,
            CreatedAt = DateTime.UtcNow.AddDays(-95)
        };

        // Act
        var isOverdue = item.IsOverdue;

        // Assert
        isOverdue.Should().BeTrue();
    }

    [Fact]
    public void InventoryItem_ShouldActivate_WhenPaused()
    {
        // Arrange
        var item = new InventoryItem
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            VehicleId = Guid.NewGuid(),
            ListPrice = 25000m,
            Status = InventoryStatus.Paused
        };

        // Act
        item.Activate();

        // Assert
        item.Status.Should().Be(InventoryStatus.Active);
        item.PublishedAt.Should().NotBeNull();
    }

    [Fact]
    public void InventoryItem_ShouldPause_WhenActive()
    {
        // Arrange
        var item = new InventoryItem
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            VehicleId = Guid.NewGuid(),
            ListPrice = 25000m,
            Status = InventoryStatus.Active
        };

        // Act
        item.Pause();

        // Assert
        item.Status.Should().Be(InventoryStatus.Paused);
    }

    [Fact]
    public void InventoryItem_ShouldRecordView()
    {
        // Arrange
        var item = new InventoryItem
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            VehicleId = Guid.NewGuid(),
            ListPrice = 25000m,
            ViewCount = 0
        };

        // Act
        item.RecordView();

        // Assert
        item.ViewCount.Should().Be(1);
        item.LastViewedAt.Should().NotBeNull();
    }

    [Fact]
    public void BulkImportJob_ShouldStart()
    {
        // Arrange
        var job = new BulkImportJob
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            FileName = "test.csv",
            FileUrl = "https://s3.amazonaws.com/test.csv",
            FileType = ImportFileType.CSV,
            Status = ImportJobStatus.Pending,
            TotalRows = 100
        };

        // Act
        job.Start();

        // Assert
        job.Status.Should().Be(ImportJobStatus.Processing);
        job.StartedAt.Should().NotBeNull();
    }

    [Fact]
    public void BulkImportJob_ShouldComplete()
    {
        // Arrange
        var job = new BulkImportJob
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            FileName = "test.csv",
            FileUrl = "https://s3.amazonaws.com/test.csv",
            FileType = ImportFileType.CSV,
            Status = ImportJobStatus.Processing,
            TotalRows = 100,
            SuccessfulRows = 95,
            FailedRows = 5
        };

        // Act
        job.Complete();

        // Assert
        job.Status.Should().Be(ImportJobStatus.Completed);
        job.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public void BulkImportJob_ShouldCalculateProgressPercentage()
    {
        // Arrange
        var job = new BulkImportJob
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            FileName = "test.csv",
            FileUrl = "https://s3.amazonaws.com/test.csv",
            FileType = ImportFileType.CSV,
            TotalRows = 100,
            ProcessedRows = 50
        };

        // Act
        var progress = job.ProgressPercentage;

        // Assert
        progress.Should().Be(50);
    }

    [Fact]
    public void InventoryStatus_ShouldHaveExpectedValues()
    {
        // Assert
        InventoryStatus.Active.ToString().Should().Be("Active");
        InventoryStatus.Paused.ToString().Should().Be("Paused");
        InventoryStatus.Sold.ToString().Should().Be("Sold");
    }

    [Fact]
    public void InventoryVisibility_ShouldHaveExpectedValues()
    {
        // Assert
        InventoryVisibility.Public.ToString().Should().Be("Public");
        InventoryVisibility.Private.ToString().Should().Be("Private");
    }
}
