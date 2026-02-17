using BackupDRService.Core.Data;
using BackupDRService.Core.Entities;
using BackupDRService.Core.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BackupDRService.Tests.Repositories;

public class AuditLogRepositoryTests : IDisposable
{
    private readonly BackupDbContext _context;
    private readonly AuditLogRepository _repository;

    public AuditLogRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<BackupDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new BackupDbContext(options);
        _repository = new AuditLogRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task GetByIdAsync_ExistingLog_ReturnsLog()
    {
        // Arrange
        var log = new AuditLog
        {
            UserId = "user-123",
            Action = "Create",
            EntityType = "BackupSchedule",
            EntityId = "1",
            Details = "Created new backup schedule",
            Timestamp = DateTime.UtcNow
        };
        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(log.Id);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be("user-123");
        result.Action.Should().Be("Create");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingLog_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsLogsOrderedByTimestampDescending()
    {
        // Arrange
        var logs = new[]
        {
            new AuditLog { UserId = "user-1", Action = "Create", EntityType = "Policy", EntityId = "1", Details = "First", Timestamp = DateTime.UtcNow.AddHours(-2) },
            new AuditLog { UserId = "user-2", Action = "Update", EntityType = "Schedule", EntityId = "2", Details = "Middle", Timestamp = DateTime.UtcNow.AddHours(-1) },
            new AuditLog { UserId = "user-3", Action = "Delete", EntityType = "Backup", EntityId = "3", Details = "Latest", Timestamp = DateTime.UtcNow }
        };
        _context.AuditLogs.AddRange(logs);
        await _context.SaveChangesAsync();

        // Act
        var result = (await _repository.GetAllAsync()).ToList();

        // Assert
        result.Should().HaveCount(3);
        result[0].Details.Should().Be("Latest");
        result[1].Details.Should().Be("Middle");
        result[2].Details.Should().Be("First");
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
    public async Task GetByUserIdAsync_ReturnsLogsForUser()
    {
        // Arrange
        var logs = new[]
        {
            new AuditLog { UserId = "target-user", Action = "Create", EntityType = "Policy", EntityId = "1", Details = "First", Timestamp = DateTime.UtcNow.AddHours(-2) },
            new AuditLog { UserId = "other-user", Action = "Update", EntityType = "Schedule", EntityId = "2", Details = "Other", Timestamp = DateTime.UtcNow.AddHours(-1) },
            new AuditLog { UserId = "target-user", Action = "Delete", EntityType = "Backup", EntityId = "3", Details = "Second", Timestamp = DateTime.UtcNow }
        };
        _context.AuditLogs.AddRange(logs);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByUserIdAsync("target-user");

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(l => l.UserId.Should().Be("target-user"));
    }

    [Fact]
    public async Task GetByUserIdAsync_OrderedByTimestampDescending()
    {
        // Arrange
        var logs = new[]
        {
            new AuditLog { UserId = "user-1", Action = "Create", EntityType = "Policy", EntityId = "1", Details = "First", Timestamp = DateTime.UtcNow.AddHours(-2) },
            new AuditLog { UserId = "user-1", Action = "Update", EntityType = "Schedule", EntityId = "2", Details = "Second", Timestamp = DateTime.UtcNow }
        };
        _context.AuditLogs.AddRange(logs);
        await _context.SaveChangesAsync();

        // Act
        var result = (await _repository.GetByUserIdAsync("user-1")).ToList();

        // Assert
        result[0].Details.Should().Be("Second");
        result[1].Details.Should().Be("First");
    }

    [Fact]
    public async Task GetByEntityTypeAsync_ReturnsLogsForEntityType()
    {
        // Arrange
        var logs = new[]
        {
            new AuditLog { UserId = "user-1", Action = "Create", EntityType = "BackupSchedule", EntityId = "1", Details = "Schedule", Timestamp = DateTime.UtcNow.AddHours(-2) },
            new AuditLog { UserId = "user-2", Action = "Update", EntityType = "RetentionPolicy", EntityId = "2", Details = "Policy", Timestamp = DateTime.UtcNow.AddHours(-1) },
            new AuditLog { UserId = "user-3", Action = "Delete", EntityType = "BackupSchedule", EntityId = "3", Details = "Another Schedule", Timestamp = DateTime.UtcNow }
        };
        _context.AuditLogs.AddRange(logs);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByEntityTypeAsync("BackupSchedule");

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(l => l.EntityType.Should().Be("BackupSchedule"));
    }

    [Fact]
    public async Task GetByActionAsync_ReturnsLogsForAction()
    {
        // Arrange
        var logs = new[]
        {
            new AuditLog { UserId = "user-1", Action = "Create", EntityType = "Policy", EntityId = "1", Details = "Created", Timestamp = DateTime.UtcNow.AddHours(-2) },
            new AuditLog { UserId = "user-2", Action = "Update", EntityType = "Schedule", EntityId = "2", Details = "Updated", Timestamp = DateTime.UtcNow.AddHours(-1) },
            new AuditLog { UserId = "user-3", Action = "Create", EntityType = "Backup", EntityId = "3", Details = "Created Again", Timestamp = DateTime.UtcNow }
        };
        _context.AuditLogs.AddRange(logs);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByActionAsync("Create");

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(l => l.Action.Should().Be("Create"));
    }

    [Fact]
    public async Task GetByDateRangeAsync_ReturnsLogsInRange()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var logs = new[]
        {
            new AuditLog { UserId = "user-1", Action = "Create", EntityType = "Policy", EntityId = "1", Details = "Before Range", Timestamp = now.AddDays(-10) },
            new AuditLog { UserId = "user-2", Action = "Update", EntityType = "Schedule", EntityId = "2", Details = "In Range 1", Timestamp = now.AddDays(-3) },
            new AuditLog { UserId = "user-3", Action = "Delete", EntityType = "Backup", EntityId = "3", Details = "In Range 2", Timestamp = now.AddDays(-1) },
            new AuditLog { UserId = "user-4", Action = "Create", EntityType = "Policy", EntityId = "4", Details = "After Range", Timestamp = now.AddDays(2) }
        };
        _context.AuditLogs.AddRange(logs);
        await _context.SaveChangesAsync();

        var startDate = now.AddDays(-5);
        var endDate = now;

        // Act
        var result = await _repository.GetByDateRangeAsync(startDate, endDate);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(l => l.Timestamp.Should().BeOnOrAfter(startDate).And.BeOnOrBefore(endDate));
    }

    [Fact]
    public async Task GetByDateRangeAsync_OrderedByTimestampDescending()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var logs = new[]
        {
            new AuditLog { UserId = "user-1", Action = "Create", EntityType = "Policy", EntityId = "1", Details = "Earlier", Timestamp = now.AddHours(-2) },
            new AuditLog { UserId = "user-2", Action = "Update", EntityType = "Schedule", EntityId = "2", Details = "Later", Timestamp = now.AddHours(-1) }
        };
        _context.AuditLogs.AddRange(logs);
        await _context.SaveChangesAsync();

        // Act
        var result = (await _repository.GetByDateRangeAsync(now.AddDays(-1), now)).ToList();

        // Assert
        result[0].Details.Should().Be("Later");
        result[1].Details.Should().Be("Earlier");
    }

    [Fact]
    public async Task GetRecentAsync_ReturnsLimitedNumberOfLogs()
    {
        // Arrange
        for (int i = 0; i < 20; i++)
        {
            _context.AuditLogs.Add(new AuditLog
            {
                UserId = $"user-{i}",
                Action = "Create",
                EntityType = "Policy",
                EntityId = i.ToString(),
                Details = $"Log {i}",
                Timestamp = DateTime.UtcNow.AddMinutes(-i)
            });
        }
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetRecentAsync(5);

        // Assert
        result.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetRecentAsync_ReturnsNewestFirst()
    {
        // Arrange
        var logs = new[]
        {
            new AuditLog { UserId = "user-1", Action = "Create", EntityType = "Policy", EntityId = "1", Details = "Oldest", Timestamp = DateTime.UtcNow.AddHours(-3) },
            new AuditLog { UserId = "user-2", Action = "Update", EntityType = "Schedule", EntityId = "2", Details = "Middle", Timestamp = DateTime.UtcNow.AddHours(-2) },
            new AuditLog { UserId = "user-3", Action = "Delete", EntityType = "Backup", EntityId = "3", Details = "Newest", Timestamp = DateTime.UtcNow.AddHours(-1) }
        };
        _context.AuditLogs.AddRange(logs);
        await _context.SaveChangesAsync();

        // Act
        var result = (await _repository.GetRecentAsync(2)).ToList();

        // Assert
        result[0].Details.Should().Be("Newest");
        result[1].Details.Should().Be("Middle");
    }

    [Fact]
    public async Task CreateAsync_AddsLogToDatabase()
    {
        // Arrange
        var log = new AuditLog
        {
            UserId = "new-user",
            Action = "Create",
            EntityType = "BackupSchedule",
            EntityId = "123",
            Details = "New log entry",
            IpAddress = "192.168.1.1",
            Timestamp = DateTime.UtcNow
        };

        // Act
        var result = await _repository.CreateAsync(log);

        // Assert
        result.Id.Should().BeGreaterThan(0);
        var saved = await _context.AuditLogs.FindAsync(result.Id);
        saved.Should().NotBeNull();
        saved!.UserId.Should().Be("new-user");
        saved.IpAddress.Should().Be("192.168.1.1");
    }

    [Fact]
    public async Task DeleteOlderThanAsync_RemovesOldLogs()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var logs = new[]
        {
            new AuditLog { UserId = "user-1", Action = "Create", EntityType = "Policy", EntityId = "1", Details = "Very Old", Timestamp = now.AddDays(-100) },
            new AuditLog { UserId = "user-2", Action = "Update", EntityType = "Schedule", EntityId = "2", Details = "Old", Timestamp = now.AddDays(-40) },
            new AuditLog { UserId = "user-3", Action = "Delete", EntityType = "Backup", EntityId = "3", Details = "Recent", Timestamp = now.AddDays(-5) }
        };
        _context.AuditLogs.AddRange(logs);
        await _context.SaveChangesAsync();

        var cutoffDate = now.AddDays(-30);

        // Act
        await _repository.DeleteOlderThanAsync(cutoffDate);

        // Assert
        var remaining = await _context.AuditLogs.ToListAsync();
        remaining.Should().HaveCount(1);
        remaining[0].Details.Should().Be("Recent");
    }

    [Fact]
    public async Task DeleteOlderThanAsync_NoLogsToDelete_DoesNotThrow()
    {
        // Arrange
        var log = new AuditLog
        {
            UserId = "user-1",
            Action = "Create",
            EntityType = "Policy",
            EntityId = "1",
            Details = "Recent",
            Timestamp = DateTime.UtcNow
        };
        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync();

        // Act & Assert
        var act = async () => await _repository.DeleteOlderThanAsync(DateTime.UtcNow.AddDays(-30));
        await act.Should().NotThrowAsync();

        var remaining = await _context.AuditLogs.CountAsync();
        remaining.Should().Be(1);
    }

    [Fact]
    public async Task DeleteOlderThanAsync_EmptyDatabase_DoesNotThrow()
    {
        // Act & Assert
        var act = async () => await _repository.DeleteOlderThanAsync(DateTime.UtcNow);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task CreateAsync_WithAllProperties_SavesCorrectly()
    {
        // Arrange
        var log = new AuditLog
        {
            UserId = "user-full",
            Action = "Update",
            EntityType = "RetentionPolicy",
            EntityId = "456",
            Details = "Updated retention policy with all details",
            IpAddress = "10.0.0.1",
            UserAgent = "Mozilla/5.0",
            Timestamp = DateTime.UtcNow
        };

        // Act
        var result = await _repository.CreateAsync(log);

        // Assert
        var saved = await _context.AuditLogs.FindAsync(result.Id);
        saved.Should().NotBeNull();
        saved!.UserId.Should().Be("user-full");
        saved.Action.Should().Be("Update");
        saved.EntityType.Should().Be("RetentionPolicy");
        saved.EntityId.Should().Be("456");
        saved.IpAddress.Should().Be("10.0.0.1");
        saved.UserAgent.Should().Be("Mozilla/5.0");
    }

    [Fact]
    public async Task GetByUserIdAsync_NoMatchingLogs_ReturnsEmptyList()
    {
        // Arrange
        var log = new AuditLog
        {
            UserId = "user-1",
            Action = "Create",
            EntityType = "Policy",
            EntityId = "1",
            Details = "Test",
            Timestamp = DateTime.UtcNow
        };
        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByUserIdAsync("non-existent-user");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByEntityTypeAsync_NoMatchingLogs_ReturnsEmptyList()
    {
        // Arrange
        var log = new AuditLog
        {
            UserId = "user-1",
            Action = "Create",
            EntityType = "BackupSchedule",
            EntityId = "1",
            Details = "Test",
            Timestamp = DateTime.UtcNow
        };
        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByEntityTypeAsync("NonExistentType");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByActionAsync_NoMatchingLogs_ReturnsEmptyList()
    {
        // Arrange
        var log = new AuditLog
        {
            UserId = "user-1",
            Action = "Create",
            EntityType = "Policy",
            EntityId = "1",
            Details = "Test",
            Timestamp = DateTime.UtcNow
        };
        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByActionAsync("Delete");

        // Assert
        result.Should().BeEmpty();
    }
}
