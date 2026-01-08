using Microsoft.EntityFrameworkCore;
using PostgresDbService.Infrastructure.Persistence;

namespace PostgresDbService.Tests.Helpers;

/// <summary>
/// Helper class for creating test database contexts
/// </summary>
public static class TestDbContextFactory
{
    /// <summary>
    /// Creates an in-memory database context for testing
    /// </summary>
    public static CentralizedDbContext CreateInMemoryContext(string databaseName = "")
    {
        if (string.IsNullOrEmpty(databaseName))
            databaseName = Guid.NewGuid().ToString();

        var options = new DbContextOptionsBuilder&lt;CentralizedDbContext&gt;()
            .UseInMemoryDatabase(databaseName)
            .Options;

        var context = new CentralizedDbContext(options);
        
        // Ensure database is created
        context.Database.EnsureCreated();

        return context;
    }

    /// <summary>
    /// Creates a context with seeded test data
    /// </summary>
    public static CentralizedDbContext CreateContextWithTestData(string databaseName = "")
    {
        var context = CreateInMemoryContext(databaseName);
        
        // Add some test entities
        SeedTestData(context);
        
        return context;
    }

    private static void SeedTestData(CentralizedDbContext context)
    {
        // Add test users
        var testUsers = new[]
        {
            new PostgresDbService.Domain.Entities.GenericEntity
            {
                Id = Guid.NewGuid(),
                ServiceName = "UserService",
                EntityType = "User",
                EntityId = "user-1",
                DataJson = """{"Email": "test@example.com", "FullName": "Test User", "Role": "Buyer", "IsActive": true}""",
                IndexData = """{"Email": "test@example.com", "FullName": "test user", "Role": "Buyer", "IsActive": true}""",
                CreatedBy = "system",
                CreatedAt = DateTime.UtcNow
            },
            new PostgresDbService.Domain.Entities.GenericEntity
            {
                Id = Guid.NewGuid(),
                ServiceName = "UserService",
                EntityType = "User",
                EntityId = "user-2",
                DataJson = """{"Email": "seller@example.com", "FullName": "Test Seller", "Role": "Seller", "IsActive": true}""",
                IndexData = """{"Email": "seller@example.com", "FullName": "test seller", "Role": "Seller", "IsActive": true}""",
                CreatedBy = "system",
                CreatedAt = DateTime.UtcNow
            }
        };

        // Add test vehicles
        var testVehicles = new[]
        {
            new PostgresDbService.Domain.Entities.GenericEntity
            {
                Id = Guid.NewGuid(),
                ServiceName = "VehiclesSaleService",
                EntityType = "Vehicle",
                EntityId = "vehicle-1",
                DataJson = """{"Make": "Toyota", "Model": "Camry", "Year": 2022, "Price": 25000, "SellerId": "user-2", "Status": "Active"}""",
                IndexData = """{"Make": "toyota", "Model": "camry", "Year": 2022, "Price": 25000, "SellerId": "user-2", "Status": "Active"}""",
                CreatedBy = "system",
                CreatedAt = DateTime.UtcNow
            },
            new PostgresDbService.Domain.Entities.GenericEntity
            {
                Id = Guid.NewGuid(),
                ServiceName = "VehiclesSaleService",
                EntityType = "Vehicle",
                EntityId = "vehicle-2",
                DataJson = """{"Make": "Honda", "Model": "Civic", "Year": 2021, "Price": 20000, "SellerId": "user-2", "Status": "Active"}""",
                IndexData = """{"Make": "honda", "Model": "civic", "Year": 2021, "Price": 20000, "SellerId": "user-2", "Status": "Active"}""",
                CreatedBy = "system",
                CreatedAt = DateTime.UtcNow
            }
        };

        // Add test contact requests
        var testContacts = new[]
        {
            new PostgresDbService.Domain.Entities.GenericEntity
            {
                Id = Guid.NewGuid(),
                ServiceName = "ContactService",
                EntityType = "ContactRequest",
                EntityId = "contact-1",
                DataJson = """{"BuyerId": "user-1", "SellerId": "user-2", "VehicleId": "vehicle-1", "BuyerName": "Test User", "BuyerEmail": "test@example.com", "Message": "Interested in this car", "Status": "Open"}""",
                IndexData = """{"BuyerId": "user-1", "SellerId": "user-2", "VehicleId": "vehicle-1", "Status": "Open", "BuyerEmail": "test@example.com"}""",
                CreatedBy = "system",
                CreatedAt = DateTime.UtcNow
            }
        };

        context.Entities.AddRange(testUsers);
        context.Entities.AddRange(testVehicles);
        context.Entities.AddRange(testContacts);
        context.SaveChanges();
    }
}