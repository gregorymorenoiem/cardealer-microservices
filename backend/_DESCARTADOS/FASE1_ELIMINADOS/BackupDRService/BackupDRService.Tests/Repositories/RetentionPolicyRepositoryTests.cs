using BackupDRService.Core.Data;
using BackupDRService.Core.Entities;
using BackupDRService.Core.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BackupDRService.Tests.Repositories;

public class RetentionPolicyRepositoryTests : IDisposable
{
    private readonly BackupDbContext _context;
    private readonly RetentionPolicyRepository _repository;

    public RetentionPolicyRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<BackupDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new BackupDbContext(options);
        _repository = new RetentionPolicyRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task GetByIdAsync_ExistingPolicy_ReturnsPolicy()
    {
        // Arrange
        var policy = new RetentionPolicy
        {
            Name = "Standard Retention",
            DailyRetentionDays = 7,
            WeeklyRetentionWeeks = 4,
            MonthlyRetentionMonths = 12,
            YearlyRetentionYears = 2,
            IsActive = true
        };
        _context.RetentionPolicies.Add(policy);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(policy.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Standard Retention");
        result.DailyRetentionDays.Should().Be(7);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingPolicy_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_IncludesBackupSchedules()
    {
        // Arrange
        var policy = new RetentionPolicy
        {
            Name = "Policy with Schedules",
            DailyRetentionDays = 7,
            WeeklyRetentionWeeks = 4,
            MonthlyRetentionMonths = 12,
            YearlyRetentionYears = 1,
            IsActive = true
        };
        _context.RetentionPolicies.Add(policy);
        await _context.SaveChangesAsync();

        var schedule = new BackupSchedule
        {
            Name = "Schedule",
            DatabaseName = "Db",
            BackupType = "Full",
            CronExpression = "0 0 * * *",
            IsEnabled = true,
            StoragePath = "/backup",
            RetentionPolicyId = policy.Id
        };
        _context.BackupSchedules.Add(schedule);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(policy.Id);

        // Assert
        result.Should().NotBeNull();
        result!.BackupSchedules.Should().NotBeNull();
        result.BackupSchedules.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByNameAsync_ExistingPolicy_ReturnsPolicy()
    {
        // Arrange
        var policy = new RetentionPolicy
        {
            Name = "Unique Policy Name",
            DailyRetentionDays = 30,
            WeeklyRetentionWeeks = 8,
            MonthlyRetentionMonths = 24,
            YearlyRetentionYears = 5,
            IsActive = true
        };
        _context.RetentionPolicies.Add(policy);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByNameAsync("Unique Policy Name");

        // Assert
        result.Should().NotBeNull();
        result!.DailyRetentionDays.Should().Be(30);
    }

    [Fact]
    public async Task GetByNameAsync_NonExistingPolicy_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByNameAsync("NonExistent Policy");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllPoliciesOrderedByName()
    {
        // Arrange
        var policies = new[]
        {
            new RetentionPolicy { Name = "Zebra Policy", DailyRetentionDays = 7, WeeklyRetentionWeeks = 4, MonthlyRetentionMonths = 12, YearlyRetentionYears = 1, IsActive = true },
            new RetentionPolicy { Name = "Alpha Policy", DailyRetentionDays = 14, WeeklyRetentionWeeks = 8, MonthlyRetentionMonths = 24, YearlyRetentionYears = 2, IsActive = false },
            new RetentionPolicy { Name = "Middle Policy", DailyRetentionDays = 30, WeeklyRetentionWeeks = 12, MonthlyRetentionMonths = 36, YearlyRetentionYears = 3, IsActive = true }
        };
        _context.RetentionPolicies.AddRange(policies);
        await _context.SaveChangesAsync();

        // Act
        var result = (await _repository.GetAllAsync()).ToList();

        // Assert
        result.Should().HaveCount(3);
        result[0].Name.Should().Be("Alpha Policy");
        result[1].Name.Should().Be("Middle Policy");
        result[2].Name.Should().Be("Zebra Policy");
    }

    [Fact]
    public async Task GetAllAsync_EmptyDatabase_ReturnsEmptyList()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetActiveAsync_ReturnsOnlyActivePolicies()
    {
        // Arrange
        var policies = new[]
        {
            new RetentionPolicy { Name = "Active 1", DailyRetentionDays = 7, WeeklyRetentionWeeks = 4, MonthlyRetentionMonths = 12, YearlyRetentionYears = 1, IsActive = true },
            new RetentionPolicy { Name = "Inactive", DailyRetentionDays = 14, WeeklyRetentionWeeks = 8, MonthlyRetentionMonths = 24, YearlyRetentionYears = 2, IsActive = false },
            new RetentionPolicy { Name = "Active 2", DailyRetentionDays = 30, WeeklyRetentionWeeks = 12, MonthlyRetentionMonths = 36, YearlyRetentionYears = 3, IsActive = true }
        };
        _context.RetentionPolicies.AddRange(policies);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetActiveAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(p => p.IsActive.Should().BeTrue());
    }

    [Fact]
    public async Task GetActiveAsync_OrderedByName()
    {
        // Arrange
        var policies = new[]
        {
            new RetentionPolicy { Name = "Zulu", DailyRetentionDays = 7, WeeklyRetentionWeeks = 4, MonthlyRetentionMonths = 12, YearlyRetentionYears = 1, IsActive = true },
            new RetentionPolicy { Name = "Alpha", DailyRetentionDays = 14, WeeklyRetentionWeeks = 8, MonthlyRetentionMonths = 24, YearlyRetentionYears = 2, IsActive = true }
        };
        _context.RetentionPolicies.AddRange(policies);
        await _context.SaveChangesAsync();

        // Act
        var result = (await _repository.GetActiveAsync()).ToList();

        // Assert
        result[0].Name.Should().Be("Alpha");
        result[1].Name.Should().Be("Zulu");
    }

    [Fact]
    public async Task CreateAsync_AddsPolicyToDatabase()
    {
        // Arrange
        var policy = new RetentionPolicy
        {
            Name = "New Policy",
            DailyRetentionDays = 7,
            WeeklyRetentionWeeks = 4,
            MonthlyRetentionMonths = 12,
            YearlyRetentionYears = 1,
            IsActive = true,
            MaxStorageSizeBytes = 100L * 1024 * 1024 * 1024, // 100GB
            MaxBackupCount = 50
        };

        // Act
        var result = await _repository.CreateAsync(policy);

        // Assert
        result.Id.Should().BeGreaterThan(0);
        var saved = await _context.RetentionPolicies.FindAsync(result.Id);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("New Policy");
        saved.MaxStorageSizeBytes.Should().Be(100L * 1024 * 1024 * 1024);
        saved.MaxBackupCount.Should().Be(50);
    }

    [Fact]
    public async Task UpdateAsync_ModifiesExistingPolicy()
    {
        // Arrange
        var policy = new RetentionPolicy
        {
            Name = "Original Policy",
            DailyRetentionDays = 7,
            WeeklyRetentionWeeks = 4,
            MonthlyRetentionMonths = 12,
            YearlyRetentionYears = 1,
            IsActive = true
        };
        _context.RetentionPolicies.Add(policy);
        await _context.SaveChangesAsync();

        // Act
        policy.Name = "Updated Policy";
        policy.DailyRetentionDays = 14;
        policy.IsActive = false;
        var result = await _repository.UpdateAsync(policy);

        // Assert
        result.Name.Should().Be("Updated Policy");
        result.DailyRetentionDays.Should().Be(14);
        result.IsActive.Should().BeFalse();
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task DeleteAsync_RemovesPolicy()
    {
        // Arrange
        var policy = new RetentionPolicy
        {
            Name = "ToDelete",
            DailyRetentionDays = 7,
            WeeklyRetentionWeeks = 4,
            MonthlyRetentionMonths = 12,
            YearlyRetentionYears = 1,
            IsActive = true
        };
        _context.RetentionPolicies.Add(policy);
        await _context.SaveChangesAsync();
        var id = policy.Id;

        // Act
        await _repository.DeleteAsync(id);

        // Assert
        var deleted = await _context.RetentionPolicies.FindAsync(id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_NonExistingPolicy_DoesNotThrow()
    {
        // Act & Assert
        var act = async () => await _repository.DeleteAsync(999);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task CreateAsync_WithAllProperties_SavesCorrectly()
    {
        // Arrange
        var policy = new RetentionPolicy
        {
            Name = "Complete Policy",
            Description = "A policy with all properties set",
            DailyRetentionDays = 7,
            WeeklyRetentionWeeks = 4,
            MonthlyRetentionMonths = 12,
            YearlyRetentionYears = 5,
            MaxStorageSizeBytes = 500L * 1024 * 1024 * 1024, // 500GB
            MaxBackupCount = 1000,
            IsActive = true
        };

        // Act
        var result = await _repository.CreateAsync(policy);

        // Assert
        var saved = await _context.RetentionPolicies.FindAsync(result.Id);
        saved.Should().NotBeNull();
        saved!.Description.Should().Be("A policy with all properties set");
        saved.DailyRetentionDays.Should().Be(7);
        saved.WeeklyRetentionWeeks.Should().Be(4);
        saved.MonthlyRetentionMonths.Should().Be(12);
        saved.YearlyRetentionYears.Should().Be(5);
        saved.MaxStorageSizeBytes.Should().Be(500L * 1024 * 1024 * 1024);
        saved.MaxBackupCount.Should().Be(1000);
    }
}
