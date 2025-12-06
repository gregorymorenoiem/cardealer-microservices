using FluentAssertions;
using MarketingService.Domain.Entities;
using Xunit;

namespace MarketingService.Tests;

public class EmailTemplateTests
{
    private readonly Guid _dealerId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public void EmailTemplate_ShouldBeCreated_WithValidParameters()
    {
        // Arrange & Act
        var template = new EmailTemplate(
            _dealerId,
            "Welcome Template",
            TemplateType.Email,
            "Welcome!",
            "<h1>Welcome</h1>",
            _userId,
            "Welcome description"
        );

        // Assert
        template.Name.Should().Be("Welcome Template");
        template.Type.Should().Be(TemplateType.Email);
        template.Subject.Should().Be("Welcome!");
        template.Body.Should().Be("<h1>Welcome</h1>");
        template.IsActive.Should().BeTrue();
        template.IsDefault.Should().BeFalse();
    }

    [Fact]
    public void EmailTemplate_ShouldThrow_WhenNameIsEmpty()
    {
        // Arrange & Act
        var act = () => new EmailTemplate(_dealerId, "", TemplateType.Email, "Subject", "<p>Content</p>", _userId);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Update_ShouldUpdateTemplateContent()
    {
        // Arrange
        var template = new EmailTemplate(_dealerId, "Original", TemplateType.Email, "Subject", "<p>Old</p>", _userId);

        // Act
        template.Update("Updated", "New Subject", "<p>New</p>", "<html>New</html>");

        // Assert
        template.Name.Should().Be("Updated");
        template.Subject.Should().Be("New Subject");
        template.Body.Should().Be("<p>New</p>");
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveFalse()
    {
        // Arrange
        var template = new EmailTemplate(_dealerId, "Test", TemplateType.Email, "Subject", "<p>Content</p>", _userId);

        // Act
        template.Deactivate();

        // Assert
        template.IsActive.Should().BeFalse();
    }

    [Fact]
    public void SetAsDefault_ShouldSetIsDefaultTrue()
    {
        // Arrange
        var template = new EmailTemplate(_dealerId, "Test", TemplateType.Email, "Subject", "<p>Content</p>", _userId);

        // Act
        template.SetAsDefault();

        // Assert
        template.IsDefault.Should().BeTrue();
    }
}
