using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PostgresDbService.Infrastructure.Repositories;
using PostgresDbService.Domain.Entities;
using PostgresDbService.Tests.Helpers;
using System.Text.Json;

namespace PostgresDbService.Tests.Infrastructure;

/// <summary>
/// Tests for GenericRepository
/// </summary>
public class GenericRepositoryTests : IDisposable
{
    private readonly Infrastructure.Persistence.CentralizedDbContext _context;
    private readonly GenericRepository _repository;

    public GenericRepositoryTests()
    {
        _context = TestDbContextFactory.CreateInMemoryContext(nameof(GenericRepositoryTests));
        _repository = new GenericRepository(_context);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateEntity_WhenValidData()
    {
        // Arrange
        var entity = new GenericEntity
        {
            ServiceName = "TestService",
            EntityType = "TestEntity",
            EntityId = "test-123",
            DataJson = """{"Name": "Test", "Value": 42}""",
            IndexData = """{"Name": "test", "Value": 42}""",
            CreatedBy = "test-user"
        };

        // Act
        var result = await _repository.CreateAsync(entity);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBe(Guid.Empty);
        result.ServiceName.Should().Be("TestService");
        result.EntityType.Should().Be("TestEntity");
        result.EntityId.Should().Be("test-123");
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.CreatedBy.Should().Be("test-user");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnEntity_WhenExists()
    {
        // Arrange
        var entity = new GenericEntity
        {
            ServiceName = "TestService",
            EntityType = "TestEntity",
            EntityId = "test-get",
            DataJson = """{"Name": "GetTest"}""",
            CreatedBy = "system"
        };
        await _repository.CreateAsync(entity);

        // Act
        var result = await _repository.GetByIdAsync("TestService", "TestEntity", "test-get");

        // Assert
        result.Should().NotBeNull();
        result!.EntityId.Should().Be("test-get");
        result.DataJson.Should().Contain("GetTest");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Act
        var result = await _repository.GetByIdAsync("TestService", "TestEntity", "non-existent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task QueryAsync_ShouldReturnMatchingEntities()
    {
        // Arrange
        var entities = new[]
        {
            new GenericEntity
            {
                ServiceName = "TestService",
                EntityType = "TestEntity",
                EntityId = "query-1",
                DataJson = """{"Status": "Active", "Name": "Test 1"}""",
                CreatedBy = "system"
            },
            new GenericEntity
            {
                ServiceName = "TestService",
                EntityType = "TestEntity",
                EntityId = "query-2",
                DataJson = """{"Status": "Inactive", "Name": "Test 2"}""",
                CreatedBy = "system"
            },
            new GenericEntity
            {
                ServiceName = "TestService",
                EntityType = "TestEntity",
                EntityId = "query-3",
                DataJson = """{"Status": "Active", "Name": "Test 3"}""",
                CreatedBy = "system"
            }
        };

        foreach (var entity in entities)
        {
            await _repository.CreateAsync(entity);
        }

        // Act
        var results = await _repository.QueryAsync("TestService", "TestEntity", "$.Status", "Active");

        // Assert
        results.Should().HaveCount(2);
        results.All(r => r.DataJson.Contains("\"Status\": \"Active\"")).Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateEntity()
    {
        // Arrange
        var entity = new GenericEntity
        {
            ServiceName = "TestService",
            EntityType = "TestEntity",
            EntityId = "update-test",
            DataJson = """{"Name": "Original"}""",
            CreatedBy = "system"
        };
        var created = await _repository.CreateAsync(entity);

        // Modify the entity
        created.DataJson = """{"Name": "Updated"}""";
        created.UpdatedBy = "updater";

        // Act
        var result = await _repository.UpdateAsync(created);

        // Assert
        result.Should().NotBeNull();
        result.DataJson.Should().Contain("Updated");
        result.UpdatedBy.Should().Be("updater");
        result.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteEntity()
    {
        // Arrange
        var entity = new GenericEntity
        {
            ServiceName = "TestService",
            EntityType = "TestEntity",
            EntityId = "delete-test",
            DataJson = """{"Name": "ToDelete"}""",
            CreatedBy = "system"
        };
        await _repository.CreateAsync(entity);

        // Act
        var result = await _repository.DeleteAsync("TestService", "TestEntity", "delete-test", "deleter");

        // Assert
        result.Should().BeTrue();

        // Verify soft delete
        var deletedEntity = await _context.Entities
            .IgnoreQueryFilters() // Ignore soft delete filter
            .FirstOrDefaultAsync(e => e.ServiceName == "TestService" && 
                                    e.EntityType == "TestEntity" && 
                                    e.EntityId == "delete-test");

        deletedEntity.Should().NotBeNull();
        deletedEntity!.IsDeleted.Should().BeTrue();
        deletedEntity.DeletedAt.Should().NotBeNull();
        deletedEntity.DeletedBy.Should().Be("deleter");

        // Verify entity is not returned by normal queries
        var normalQuery = await _repository.GetByIdAsync("TestService", "TestEntity", "delete-test");
        normalQuery.Should().BeNull();
    }

    [Fact]
    public async Task SearchAsync_ShouldFindEntitiesWithText()
    {
        // Arrange
        var entities = new[]
        {
            new GenericEntity
            {
                ServiceName = "TestService",
                EntityType = "TestEntity",
                EntityId = "search-1",
                DataJson = """{"Name": "Toyota Camry"}""",
                IndexData = """{"Name": "toyota camry"}""",
                CreatedBy = "system"
            },
            new GenericEntity
            {
                ServiceName = "TestService",
                EntityType = "TestEntity", 
                EntityId = "search-2",
                DataJson = """{"Name": "Honda Civic"}""",
                IndexData = """{"Name": "honda civic"}""",
                CreatedBy = "system"
            }
        };

        foreach (var entity in entities)
        {
            await _repository.CreateAsync(entity);
        }

        // Act
        var results = await _repository.SearchAsync("TestService", "TestEntity", "toyota");

        // Assert
        results.Should().HaveCount(1);
        results.First().DataJson.Should().Contain("Toyota");
    }

    [Fact]
    public async Task BulkCreateAsync_ShouldCreateMultipleEntities()
    {
        // Arrange
        var entities = new[]
        {
            new GenericEntity
            {
                ServiceName = "TestService",
                EntityType = "TestEntity",
                EntityId = "bulk-1",
                DataJson = """{"Name": "Bulk 1"}""",
                CreatedBy = "system"
            },
            new GenericEntity
            {
                ServiceName = "TestService", 
                EntityType = "TestEntity",
                EntityId = "bulk-2",
                DataJson = """{"Name": "Bulk 2"}""",
                CreatedBy = "system"
            },
            new GenericEntity
            {
                ServiceName = "TestService",
                EntityType = "TestEntity", 
                EntityId = "bulk-3",
                DataJson = """{"Name": "Bulk 3"}""",
                CreatedBy = "system"
            }
        };

        // Act
        var results = await _repository.BulkCreateAsync(entities);

        // Assert
        results.Should().HaveCount(3);
        results.All(r => r.Id != Guid.Empty).Should().BeTrue();
        results.All(r => r.CreatedAt != default).Should().BeTrue();

        // Verify all entities exist in database
        var dbEntities = await _repository.GetByServiceAsync("TestService", "TestEntity");
        dbEntities.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetByServiceAsync_ShouldReturnAllEntitiesForService()
    {
        // Arrange
        var entities = new[]
        {
            new GenericEntity { ServiceName = "Service1", EntityType = "Type1", EntityId = "1", DataJson = "{}", CreatedBy = "system" },
            new GenericEntity { ServiceName = "Service1", EntityType = "Type1", EntityId = "2", DataJson = "{}", CreatedBy = "system" },
            new GenericEntity { ServiceName = "Service2", EntityType = "Type1", EntityId = "3", DataJson = "{}", CreatedBy = "system" }
        };

        foreach (var entity in entities)
        {
            await _repository.CreateAsync(entity);
        }

        // Act
        var results = await _repository.GetByServiceAsync("Service1", "Type1");

        // Assert
        results.Should().HaveCount(2);
        results.All(r => r.ServiceName == "Service1").Should().BeTrue();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}