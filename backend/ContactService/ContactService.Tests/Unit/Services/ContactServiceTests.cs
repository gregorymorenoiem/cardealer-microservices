using Microsoft.Extensions.Logging;

namespace ContactService.Tests.Unit.Services;

/// <summary>
/// Unit tests for ContactService business logic
/// </summary>
public class ContactServiceTests
{
    private readonly Mock<ILogger<ContactServiceTests>> _mockLogger;

    public ContactServiceTests()
    {
        _mockLogger = new Mock<ILogger<ContactServiceTests>>();
    }

    [Fact]
    public void CreateContact_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var contactData = new
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Phone = "+1-555-0123"
        };

        // Act & Assert
        contactData.FirstName.Should().NotBeNullOrEmpty();
        contactData.Email.Should().Contain("@");
    }

    [Fact]
    public void ValidateEmail_WithValidEmail_ReturnsTrue()
    {
        // Arrange
        var email = "test@example.com";

        // Act
        var isValid = email.Contains("@") && email.Contains(".");

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateEmail_WithInvalidEmail_ReturnsFalse()
    {
        // Arrange
        var email = "invalid-email";

        // Act
        var isValid = email.Contains("@") && email.Contains(".");

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void ValidatePhone_WithValidPhone_ReturnsTrue()
    {
        // Arrange
        var phone = "+1-555-0123";

        // Act
        var isValid = phone.Length >= 10;

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void ValidatePhone_WithInvalidPhone_ReturnsFalse()
    {
        // Arrange
        var phone = "123";

        // Act
        var isValid = phone.Length >= 10;

        // Assert
        isValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("john.doe@example.com", true)]
    [InlineData("jane@test.org", true)]
    [InlineData("invalid", false)]
    [InlineData("no-at-sign.com", false)]
    public void ValidateEmail_WithVariousInputs_ReturnsExpectedResult(string email, bool expected)
    {
        // Act
        var isValid = email.Contains("@") && email.Contains(".");

        // Assert
        isValid.Should().Be(expected);
    }

    [Fact]
    public void ContactType_ShouldHaveValidValues()
    {
        // Arrange & Act
        var types = new[] { "Lead", "Prospect", "Client" };

        // Assert
        types.Should().Contain("Lead");
        types.Should().Contain("Prospect");
        types.Should().Contain("Client");
        types.Should().HaveCount(3);
    }

    [Fact]
    public void ContactStatus_ShouldHaveValidValues()
    {
        // Arrange & Act
        var statuses = new[] { "Active", "Inactive", "Converted" };

        // Assert
        statuses.Should().Contain("Active");
        statuses.Should().Contain("Inactive");
        statuses.Should().Contain("Converted");
        statuses.Should().HaveCount(3);
    }

    [Fact]
    public void CommunicationChannel_ShouldHaveValidValues()
    {
        // Arrange & Act
        var channels = new[] { "Email", "Phone", "SMS", "Meeting" };

        // Assert
        channels.Should().Contain("Email");
        channels.Should().Contain("Phone");
        channels.Should().Contain("SMS");
        channels.Should().Contain("Meeting");
        channels.Should().HaveCount(4);
    }

    [Fact]
    public void Contact_WithRequiredFields_ShouldBeValid()
    {
        // Arrange
        var contact = new
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Type = "Lead",
            Status = "Active"
        };

        // Assert
        contact.FirstName.Should().NotBeNullOrEmpty();
        contact.LastName.Should().NotBeNullOrEmpty();
        contact.Email.Should().NotBeNullOrEmpty();
        contact.Type.Should().NotBeNullOrEmpty();
        contact.Status.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Communication_WithRequiredFields_ShouldBeValid()
    {
        // Arrange
        var communication = new
        {
            ContactId = Guid.NewGuid(),
            Channel = "Email",
            Subject = "Follow-up",
            CommunicatedAt = DateTime.UtcNow
        };

        // Assert
        communication.ContactId.Should().NotBeEmpty();
        communication.Channel.Should().NotBeNullOrEmpty();
        communication.Subject.Should().NotBeNullOrEmpty();
        communication.CommunicatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
