using ContactService.Domain.Entities;
using FluentAssertions;
using System.Reflection;

namespace ContactService.Tests.Integration;

public class ContactServiceIntegrationTests
{
    [Fact]
    public void ContactRequest_Should_Have_Correct_Properties()
    {
        // Arrange & Act
        var contactRequest = new ContactRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Test Subject",
            "John Doe",
            "john@test.com",
            "Test message"
        );

        // Assert
        contactRequest.Id.Should().NotBe(Guid.Empty);
        contactRequest.Subject.Should().Be("Test Subject");
        contactRequest.BuyerName.Should().Be("John Doe");
        contactRequest.BuyerEmail.Should().Be("john@test.com");
        contactRequest.Message.Should().Be("Test message");
        contactRequest.Status.Should().Be("Open");
        contactRequest.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10));
    }

    [Fact]
    public void ContactMessage_Should_Create_With_Correct_Values()
    {
        // Arrange
        var inquiryId = Guid.NewGuid();
        var senderId = Guid.NewGuid();
        var message = "This is a test message";

        // Act
        var contactMessage = new ContactMessage(inquiryId, senderId, message, true);

        // Assert
        contactMessage.Id.Should().NotBe(Guid.Empty);
        contactMessage.ContactRequestId.Should().Be(inquiryId);
        contactMessage.SenderId.Should().Be(senderId);
        contactMessage.Message.Should().Be(message);
        contactMessage.IsFromBuyer.Should().BeTrue();
        contactMessage.IsRead.Should().BeFalse();
        contactMessage.SentAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10));
    }

    [Fact]
    public void ContactService_Assembly_Should_Load_Successfully()
    {
        // Arrange & Act
        var assembly = Assembly.GetAssembly(typeof(ContactRequest));

        // Assert
        assembly.Should().NotBeNull();
        assembly!.GetName().Name.Should().Be("ContactService.Domain");
    }

    [Fact]
    public void ContactService_Api_Assembly_Should_Load_Successfully()
    {
        // Arrange & Act
        var assembly = Assembly.GetAssembly(typeof(ContactService.Api.Controllers.ContactRequestsController));

        // Assert
        assembly.Should().NotBeNull();
        assembly!.GetName().Name.Should().Be("ContactService.Api");
    }

    [Fact]
    public void ContactService_Infrastructure_Assembly_Should_Load_Successfully()
    {
        // Arrange & Act  
        var assembly = Assembly.GetAssembly(typeof(ContactService.Infrastructure.Persistence.ApplicationDbContext));

        // Assert
        assembly.Should().NotBeNull();
        assembly!.GetName().Name.Should().Be("ContactService.Infrastructure");
    }

    [Fact]
    public void ContactRequest_Should_Initialize_Empty_Messages_List()
    {
        // Arrange & Act
        var contactRequest = new ContactRequest();

        // Assert
        contactRequest.Messages.Should().NotBeNull();
        contactRequest.Messages.Should().BeEmpty();
        contactRequest.Id.Should().NotBe(Guid.Empty);
        contactRequest.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10));
    }

    [Fact]
    public void ContactMessage_Default_Constructor_Should_Set_Values()
    {
        // Arrange & Act
        var message = new ContactMessage();

        // Assert
        message.Id.Should().NotBe(Guid.Empty);
        message.IsRead.Should().BeFalse();
        message.SentAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10));
    }
}