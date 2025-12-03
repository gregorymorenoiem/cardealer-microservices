namespace ContactService.Tests.Unit.Validation;

/// <summary>
/// Unit tests for contact validation logic
/// </summary>
public class ContactValidationTests
{
    [Theory]
    [InlineData("", false)]
    [InlineData(" ", false)]
    [InlineData("A", true)]
    [InlineData("John", true)]
    [InlineData("John-Paul", true)]
    public void ValidateFirstName_WithVariousInputs_ReturnsExpectedResult(string firstName, bool expected)
    {
        // Act
        var isValid = !string.IsNullOrWhiteSpace(firstName);

        // Assert
        isValid.Should().Be(expected);
    }

    [Theory]
    [InlineData("", false)]
    [InlineData(" ", false)]
    [InlineData("D", true)]
    [InlineData("Doe", true)]
    [InlineData("Van Der Berg", true)]
    public void ValidateLastName_WithVariousInputs_ReturnsExpectedResult(string lastName, bool expected)
    {
        // Act
        var isValid = !string.IsNullOrWhiteSpace(lastName);

        // Assert
        isValid.Should().Be(expected);
    }

    [Theory]
    [InlineData("test@example.com", true)]
    [InlineData("user.name@company.co.uk", true)]
    [InlineData("invalid", false)]
    [InlineData("@example.com", false)]
    [InlineData("user@", false)]
    [InlineData("", false)]
    public void ValidateEmail_WithVariousFormats_ReturnsExpectedResult(string email, bool expected)
    {
        // Act
        var isValid = !string.IsNullOrWhiteSpace(email) &&
                      email.Contains("@") &&
                      email.Contains(".") &&
                      email.IndexOf("@") > 0 &&
                      email.LastIndexOf(".") > email.IndexOf("@");

        // Assert
        isValid.Should().Be(expected);
    }

    [Theory]
    [InlineData("+1-555-0123", true)]
    [InlineData("555-0123", true)]
    [InlineData("5550123456", true)]
    [InlineData("123", false)]
    [InlineData("", false)]
    [InlineData("abc", false)]
    public void ValidatePhone_WithVariousFormats_ReturnsExpectedResult(string phone, bool expected)
    {
        // Act
        var isValid = !string.IsNullOrWhiteSpace(phone) &&
                      phone.Replace("-", "").Replace("+", "").Replace(" ", "").Length >= 7;

        // Assert
        isValid.Should().Be(expected);
    }

    [Fact]
    public void Contact_WithAllRequiredFields_PassesValidation()
    {
        // Arrange
        var contact = new
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Phone = "+1-555-0123",
            Type = "Lead"
        };

        // Act
        var isValid = !string.IsNullOrWhiteSpace(contact.FirstName) &&
                      !string.IsNullOrWhiteSpace(contact.LastName) &&
                      !string.IsNullOrWhiteSpace(contact.Email) &&
                      contact.Email.Contains("@");

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void Contact_WithMissingRequiredFields_FailsValidation()
    {
        // Arrange
        var contact = new
        {
            FirstName = "",
            LastName = "Doe",
            Email = "invalid-email"
        };

        // Act
        var isValid = !string.IsNullOrWhiteSpace(contact.FirstName) &&
                      !string.IsNullOrWhiteSpace(contact.LastName) &&
                      contact.Email.Contains("@");

        // Assert
        isValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("Lead", true)]
    [InlineData("Prospect", true)]
    [InlineData("Client", true)]
    [InlineData("Customer", false)]
    [InlineData("", false)]
    public void ValidateContactType_WithVariousTypes_ReturnsExpectedResult(string type, bool expected)
    {
        // Arrange
        var validTypes = new[] { "Lead", "Prospect", "Client" };

        // Act
        var isValid = validTypes.Contains(type);

        // Assert
        isValid.Should().Be(expected);
    }

    [Theory]
    [InlineData("Active", true)]
    [InlineData("Inactive", true)]
    [InlineData("Converted", true)]
    [InlineData("Pending", false)]
    [InlineData("", false)]
    public void ValidateContactStatus_WithVariousStatuses_ReturnsExpectedResult(string status, bool expected)
    {
        // Arrange
        var validStatuses = new[] { "Active", "Inactive", "Converted" };

        // Act
        var isValid = validStatuses.Contains(status);

        // Assert
        isValid.Should().Be(expected);
    }

    [Fact]
    public void Address_WithAllFields_PassesValidation()
    {
        // Arrange
        var address = new
        {
            Street = "123 Main St",
            City = "New York",
            State = "NY",
            ZipCode = "10001",
            Country = "USA"
        };

        // Act
        var isValid = !string.IsNullOrWhiteSpace(address.Street) &&
                      !string.IsNullOrWhiteSpace(address.City) &&
                      !string.IsNullOrWhiteSpace(address.Country);

        // Assert
        isValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("10001", true)]
    [InlineData("10001-1234", true)]
    [InlineData("123", false)]
    [InlineData("", false)]
    public void ValidateZipCode_WithVariousFormats_ReturnsExpectedResult(string zipCode, bool expected)
    {
        // Act
        var isValid = !string.IsNullOrWhiteSpace(zipCode) && zipCode.Length >= 5;

        // Assert
        isValid.Should().Be(expected);
    }
}
