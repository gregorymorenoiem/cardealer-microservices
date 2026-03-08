using CarDealer.Shared.Persistence;
using FluentAssertions;

namespace CarDealer.Shared.Tests.Persistence;

/// <summary>
/// Tests for EntityBase: audit fields, soft delete, concurrency stamp, restore.
/// </summary>
public class EntityBaseTests
{
    private class TestEntity : EntityBase { }

    [Fact]
    public void NewEntity_HasIdAndDefaults()
    {
        var entity = new TestEntity();

        entity.Id.Should().NotBeEmpty();
        entity.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        entity.UpdatedAt.Should().BeNull();
        entity.IsDeleted.Should().BeFalse();
        entity.DeletedAt.Should().BeNull();
        entity.DeletedBy.Should().BeNull();
        entity.ConcurrencyStamp.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void MarkAsUpdated_SetsUpdatedAtAndNewConcurrencyStamp()
    {
        var entity = new TestEntity();
        var originalStamp = entity.ConcurrencyStamp;

        entity.MarkAsUpdated();

        entity.UpdatedAt.Should().NotBeNull();
        entity.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        entity.ConcurrencyStamp.Should().NotBe(originalStamp);
    }

    [Fact]
    public void MarkAsDeleted_SetsAllDeleteFields()
    {
        var entity = new TestEntity();
        var originalStamp = entity.ConcurrencyStamp;

        entity.MarkAsDeleted("admin@okla.com");

        entity.IsDeleted.Should().BeTrue();
        entity.DeletedAt.Should().NotBeNull();
        entity.DeletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        entity.DeletedBy.Should().Be("admin@okla.com");
        entity.ConcurrencyStamp.Should().NotBe(originalStamp);
    }

    [Fact]
    public void MarkAsDeleted_WithoutDeletedBy_SetsNullDeletedBy()
    {
        var entity = new TestEntity();

        entity.MarkAsDeleted();

        entity.IsDeleted.Should().BeTrue();
        entity.DeletedBy.Should().BeNull();
    }

    [Fact]
    public void Restore_ClearsAllDeleteFields()
    {
        var entity = new TestEntity();
        entity.MarkAsDeleted("admin@okla.com");
        var stampAfterDelete = entity.ConcurrencyStamp;

        entity.Restore();

        entity.IsDeleted.Should().BeFalse();
        entity.DeletedAt.Should().BeNull();
        entity.DeletedBy.Should().BeNull();
        entity.ConcurrencyStamp.Should().NotBe(stampAfterDelete);
    }

    [Fact]
    public void ConcurrencyStamp_ChangesOnEveryMutation()
    {
        var entity = new TestEntity();
        var stamps = new HashSet<string>();
        stamps.Add(entity.ConcurrencyStamp);

        entity.MarkAsUpdated();
        stamps.Add(entity.ConcurrencyStamp);

        entity.MarkAsDeleted();
        stamps.Add(entity.ConcurrencyStamp);

        entity.Restore();
        stamps.Add(entity.ConcurrencyStamp);

        stamps.Should().HaveCount(4, "each mutation should produce a unique concurrency stamp");
    }
}
