using FluentValidation.TestHelper;
using RoleService.Application.DTOs.Roles;
using RoleService.Application.UseCases.Roles.CreateRole;
using Xunit;

namespace RoleService.Tests.Validators;

/// <summary>
/// Tests para CreateRoleCommandValidator.
/// Valida reglas de negocio: nombre de rol, displayName, descripción, permisos.
/// </summary>
public class CreateRoleCommandValidatorTests
{
    private readonly CreateRoleCommandValidator _validator;

    public CreateRoleCommandValidatorTests()
    {
        _validator = new CreateRoleCommandValidator();
    }

    #region Name Validation Tests

    [Fact]
    public void Name_ShouldBeRequired()
    {
        // Arrange
        var command = new CreateRoleCommand(new CreateRoleRequest
        {
            Name = string.Empty,
            DisplayName = "Valid Display Name",
            Description = "Valid description"
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.Name)
            .WithErrorMessage("Role name is required")
            .WithErrorCode("INVALID_ROLE_NAME");
    }

    [Theory]
    [InlineData("ab")] // Too short
    [InlineData("x")]  // Too short
    public void Name_ShouldHaveMinimumLength3(string invalidName)
    {
        // Arrange
        var command = new CreateRoleCommand(new CreateRoleRequest
        {
            Name = invalidName,
            DisplayName = "Valid Display Name",
            Description = "Valid description"
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.Name)
            .WithErrorMessage("Role name must be at least 3 characters");
    }

    [Fact]
    public void Name_ShouldNotExceed50Characters()
    {
        // Arrange
        var longName = new string('a', 51); // 51 characters
        var command = new CreateRoleCommand(new CreateRoleRequest
        {
            Name = longName,
            DisplayName = "Valid Display Name",
            Description = "Valid description"
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.Name)
            .WithErrorMessage("Role name cannot exceed 50 characters");
    }

    [Theory]
    [InlineData("1InvalidRole")]      // Starts with number
    [InlineData("_InvalidRole")]      // Starts with underscore
    [InlineData("-InvalidRole")]      // Starts with hyphen
    [InlineData("Invalid Role")]      // Contains space
    [InlineData("Invalid@Role")]      // Contains special char
    [InlineData("Invalid.Role")]      // Contains dot
    public void Name_ShouldMatchValidFormat(string invalidName)
    {
        // Arrange
        var command = new CreateRoleCommand(new CreateRoleRequest
        {
            Name = invalidName,
            DisplayName = "Valid Display Name",
            Description = "Valid description"
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.Name)
            .WithErrorMessage("Role name must start with a letter and contain only letters, numbers, underscores and hyphens");
    }

    [Theory]
    [InlineData("ValidRole")]           // Simple valid
    [InlineData("Admin")]               // Simple valid
    [InlineData("Super_Admin")]         // With underscore
    [InlineData("Content-Manager")]     // With hyphen
    [InlineData("Manager_Level_2")]     // Multiple underscores
    [InlineData("Sales2024")]           // With numbers
    public void Name_ShouldAcceptValidFormats(string validName)
    {
        // Arrange
        var command = new CreateRoleCommand(new CreateRoleRequest
        {
            Name = validName,
            DisplayName = "Valid Display Name",
            Description = "Valid description"
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Request.Name);
    }

    #endregion

    #region DisplayName Validation Tests

    [Fact]
    public void DisplayName_ShouldBeRequired()
    {
        // Arrange
        var command = new CreateRoleCommand(new CreateRoleRequest
        {
            Name = "ValidRole",
            DisplayName = string.Empty,
            Description = "Valid description"
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.DisplayName)
            .WithErrorMessage("Display name is required")
            .WithErrorCode("INVALID_DISPLAY_NAME");
    }

    [Theory]
    [InlineData("ab")] // Too short
    [InlineData("x")]  // Too short
    public void DisplayName_ShouldHaveMinimumLength3(string invalidDisplayName)
    {
        // Arrange
        var command = new CreateRoleCommand(new CreateRoleRequest
        {
            Name = "ValidRole",
            DisplayName = invalidDisplayName,
            Description = "Valid description"
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.DisplayName)
            .WithErrorMessage("Display name must be at least 3 characters");
    }

    [Fact]
    public void DisplayName_ShouldNotExceed100Characters()
    {
        // Arrange
        var longDisplayName = new string('A', 101); // 101 characters
        var command = new CreateRoleCommand(new CreateRoleRequest
        {
            Name = "ValidRole",
            DisplayName = longDisplayName,
            Description = "Valid description"
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.DisplayName)
            .WithErrorMessage("Display name cannot exceed 100 characters");
    }

    #endregion

    #region Description Validation Tests

    [Fact]
    public void Description_ShouldBeOptional()
    {
        // Arrange
        var command = new CreateRoleCommand(new CreateRoleRequest
        {
            Name = "ValidRole",
            DisplayName = "Valid Display Name",
            Description = null // Optional
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Request.Description);
    }

    [Fact]
    public void Description_ShouldNotExceed500Characters()
    {
        // Arrange
        var longDescription = new string('D', 501); // 501 characters
        var command = new CreateRoleCommand(new CreateRoleRequest
        {
            Name = "ValidRole",
            DisplayName = "Valid Display Name",
            Description = longDescription
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.Description)
            .WithErrorMessage("Description cannot exceed 500 characters");
    }

    #endregion

    #region PermissionIds Validation Tests

    [Fact]
    public void PermissionIds_ShouldNotExceed100Items()
    {
        // Arrange
        var tooManyPermissions = Enumerable.Range(1, 101).Select(_ => Guid.NewGuid()).ToList();
        var command = new CreateRoleCommand(new CreateRoleRequest
        {
            Name = "ValidRole",
            DisplayName = "Valid Display Name",
            Description = "Valid description",
            PermissionIds = tooManyPermissions
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.PermissionIds)
            .WithErrorMessage("Cannot assign more than 100 permissions to a role")
            .WithErrorCode("TOO_MANY_PERMISSIONS");
    }

    [Fact]
    public void PermissionIds_ShouldNotContainDuplicates()
    {
        // Arrange
        var duplicateId = Guid.NewGuid();
        var permissionsWithDuplicates = new List<Guid>
        {
            duplicateId,
            Guid.NewGuid(),
            duplicateId, // Duplicate
            Guid.NewGuid()
        };

        var command = new CreateRoleCommand(new CreateRoleRequest
        {
            Name = "ValidRole",
            DisplayName = "Valid Display Name",
            Description = "Valid description",
            PermissionIds = permissionsWithDuplicates
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.PermissionIds)
            .WithErrorMessage("Duplicate permission IDs are not allowed")
            .WithErrorCode("DUPLICATE_PERMISSION_IDS");
    }

    [Fact]
    public void PermissionIds_ShouldAcceptValidList()
    {
        // Arrange
        var validPermissions = new List<Guid>
        {
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid()
        };

        var command = new CreateRoleCommand(new CreateRoleRequest
        {
            Name = "ValidRole",
            DisplayName = "Valid Display Name",
            Description = "Valid description",
            PermissionIds = validPermissions
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Request.PermissionIds);
    }

    [Fact]
    public void PermissionIds_ShouldBeOptional()
    {
        // Arrange
        var command = new CreateRoleCommand(new CreateRoleRequest
        {
            Name = "ValidRole",
            DisplayName = "Valid Display Name",
            Description = "Valid description",
            PermissionIds = null // Optional
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Request.PermissionIds);
    }

    #endregion

    #region Complete Valid Request Test

    [Fact]
    public void CompleteValidRequest_ShouldPassValidation()
    {
        // Arrange
        var command = new CreateRoleCommand(new CreateRoleRequest
        {
            Name = "Content_Manager",
            DisplayName = "Content Manager",
            Description = "Manages all content on the platform including vehicles, properties, and media",
            PermissionIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() }
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion
}
