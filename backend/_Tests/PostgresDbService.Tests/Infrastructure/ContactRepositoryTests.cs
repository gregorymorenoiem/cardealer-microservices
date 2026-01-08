using Xunit;
using FluentAssertions;
using PostgresDbService.Infrastructure.Repositories;
using PostgresDbService.Tests.Helpers;
using System.Text.Json;

namespace PostgresDbService.Tests.Infrastructure;

/// <summary>
/// Tests for ContactRepository
/// </summary>
public class ContactRepositoryTests : IDisposable
{
    private readonly Infrastructure.Persistence.CentralizedDbContext _context;
    private readonly GenericRepository _genericRepository;
    private readonly ContactRepository _contactRepository;

    public ContactRepositoryTests()
    {
        _context = TestDbContextFactory.CreateInMemoryContext(nameof(ContactRepositoryTests));
        _genericRepository = new GenericRepository(_context);
        _contactRepository = new ContactRepository(_genericRepository);
    }

    [Fact]
    public async Task CreateContactRequestAsync_ShouldCreateRequest_WithCorrectData()
    {
        // Arrange
        var contactData = new
        {
            BuyerId = Guid.NewGuid().ToString(),
            SellerId = Guid.NewGuid().ToString(),
            VehicleId = Guid.NewGuid().ToString(),
            BuyerName = "John Buyer",
            BuyerEmail = "buyer@test.com",
            BuyerPhone = "809-555-0123",
            Message = "I'm interested in this vehicle",
            IsUrgent = true,
            PreferredContactMethod = "Phone"
        };

        // Act
        var result = await _contactRepository.CreateContactRequestAsync(contactData, "system");

        // Assert
        result.Should().NotBeNull();
        result.ServiceName.Should().Be("ContactService");
        result.EntityType.Should().Be("ContactRequest");
        result.CreatedBy.Should().Be("system");
        
        var data = JsonSerializer.Deserialize<JsonElement>(result.DataJson);
        data.GetProperty("BuyerName").GetString().Should().Be("John Buyer");
        data.GetProperty("BuyerEmail").GetString().Should().Be("buyer@test.com");
        data.GetProperty("IsUrgent").GetBoolean().Should().BeTrue();
        data.GetProperty("Status").GetString().Should().Be("Open");
    }

    [Fact]
    public async Task GetContactRequestsByBuyerAsync_ShouldReturnBuyerRequests()
    {
        // Arrange
        var buyerId = Guid.NewGuid();
        var requests = new[]
        {
            new {
                BuyerId = buyerId.ToString(),
                SellerId = Guid.NewGuid().ToString(),
                VehicleId = Guid.NewGuid().ToString(),
                BuyerName = "Buyer 1",
                BuyerEmail = "buyer1@test.com",
                Message = "Interested in car 1"
            },
            new {
                BuyerId = buyerId.ToString(),
                SellerId = Guid.NewGuid().ToString(),
                VehicleId = Guid.NewGuid().ToString(),
                BuyerName = "Buyer 1",
                BuyerEmail = "buyer1@test.com",
                Message = "Interested in car 2"
            },
            new {
                BuyerId = Guid.NewGuid().ToString(), // Different buyer
                SellerId = Guid.NewGuid().ToString(),
                VehicleId = Guid.NewGuid().ToString(),
                BuyerName = "Other Buyer",
                BuyerEmail = "other@test.com",
                Message = "Different buyer request"
            }
        };

        foreach (var request in requests)
        {
            await _contactRepository.CreateContactRequestAsync(request, "system");
        }

        // Act
        var results = await _contactRepository.GetContactRequestsByBuyerAsync(buyerId);

        // Assert
        results.Should().HaveCount(2);
        results.All(r => 
        {
            var data = JsonSerializer.Deserialize<JsonElement>(r.DataJson);
            return data.GetProperty("BuyerId").GetString() == buyerId.ToString();
        }).Should().BeTrue();
    }

    [Fact]
    public async Task GetContactRequestsBySellerAsync_ShouldReturnSellerRequests()
    {
        // Arrange
        var sellerId = Guid.NewGuid();
        var requests = new[]
        {
            new {
                BuyerId = Guid.NewGuid().ToString(),
                SellerId = sellerId.ToString(),
                VehicleId = Guid.NewGuid().ToString(),
                BuyerName = "Buyer A",
                BuyerEmail = "buyera@test.com",
                Message = "Request for seller vehicle 1"
            },
            new {
                BuyerId = Guid.NewGuid().ToString(),
                SellerId = sellerId.ToString(),
                VehicleId = Guid.NewGuid().ToString(),
                BuyerName = "Buyer B",
                BuyerEmail = "buyerb@test.com",
                Message = "Request for seller vehicle 2"
            }
        };

        foreach (var request in requests)
        {
            await _contactRepository.CreateContactRequestAsync(request, "system");
        }

        // Act
        var results = await _contactRepository.GetContactRequestsBySellerAsync(sellerId);

        // Assert
        results.Should().HaveCount(2);
        results.All(r => 
        {
            var data = JsonSerializer.Deserialize<JsonElement>(r.DataJson);
            return data.GetProperty("SellerId").GetString() == sellerId.ToString();
        }).Should().BeTrue();
    }

    [Fact]
    public async Task CreateContactMessageAsync_ShouldCreateMessage()
    {
        // Arrange
        var messageData = new
        {
            ContactRequestId = Guid.NewGuid().ToString(),
            SenderId = Guid.NewGuid().ToString(),
            SenderName = "Message Sender",
            Message = "This is a test message",
            MessageType = "Text",
            IsFromBuyer = true
        };

        // Act
        var result = await _contactRepository.CreateContactMessageAsync(messageData, "sender");

        // Assert
        result.Should().NotBeNull();
        result.ServiceName.Should().Be("ContactService");
        result.EntityType.Should().Be("ContactMessage");
        result.CreatedBy.Should().Be("sender");
        
        var data = JsonSerializer.Deserialize<JsonElement>(result.DataJson);
        data.GetProperty("SenderName").GetString().Should().Be("Message Sender");
        data.GetProperty("Message").GetString().Should().Be("This is a test message");
        data.GetProperty("IsFromBuyer").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task GetContactMessagesByRequestAsync_ShouldReturnMessagesForRequest()
    {
        // Arrange
        var contactRequestId = Guid.NewGuid();
        var messages = new[]
        {
            new {
                ContactRequestId = contactRequestId.ToString(),
                SenderId = "sender1",
                SenderName = "Sender 1",
                Message = "First message",
                IsFromBuyer = true
            },
            new {
                ContactRequestId = contactRequestId.ToString(),
                SenderId = "sender2",
                SenderName = "Sender 2",
                Message = "Reply message",
                IsFromBuyer = false
            },
            new {
                ContactRequestId = Guid.NewGuid().ToString(), // Different request
                SenderId = "sender3",
                SenderName = "Sender 3",
                Message = "Different request message",
                IsFromBuyer = true
            }
        };

        foreach (var message in messages)
        {
            await _contactRepository.CreateContactMessageAsync(message, "system");
        }

        // Act
        var results = await _contactRepository.GetContactMessagesByRequestAsync(contactRequestId);

        // Assert
        results.Should().HaveCount(2);
        results.All(m => 
        {
            var data = JsonSerializer.Deserialize<JsonElement>(m.DataJson);
            return data.GetProperty("ContactRequestId").GetString() == contactRequestId.ToString();
        }).Should().BeTrue();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}