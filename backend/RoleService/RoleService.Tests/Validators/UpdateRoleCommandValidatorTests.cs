using FluentValidation.TestHelper;
using RoleService.Application.DTOs.Roles;
using RoleService.Application.UseCases.Roles.UpdateRole;
using Xunit;

namespace RoleService.Tests.Validators;

/// <summary>
/// Tests para UpdateRoleCommandValidator.
/// Valida que campos opcionales tengan formato correcto cuando se proporcionan.
/// </summary>
public class UpdateRoleCommandValidatorTests
{
    private readonly UpdateRoleCommandValidator _validator;

    public UpdateRoleCommandValidatorTests()
    {
        _validator = new UpdateRoleCommandValidator();
    }

    #region RoleId Validation Tests

    [Fact]
    public void RoleId_ShouldBeRequired()
    {
        // Arrange
        var command = new UpdateRoleCommand(Guid.Empty, new UpdateRoleRequest
        {
            DisplayName = "Updated Display Name"
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RoleId)
            .WithErrorMessage("Role ID is required")
            .WithErrorCode("INVALID_ROLE_ID");
    }

    [Fact]
    public void RoleId_ShouldAcceptValidGuid()
    {
        // Arrange
        var command = new UpdateRoleCommand(Guid.NewGuid(), new UpdateRoleRequest
        {
            DisplayName = "Updated Display Name"
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.RoleId);
    }

    #endregion

    #region DisplayName Validation Tests

    [Fact]
    public void DisplayName_ShouldBeOptional()
    {
        // Arrange
        var command = new UpdateRoleCommand(Guid.NewGuid(), new UpdateRoleRequest
        {
            DisplayName = null, // Optional
            Description = "Some description"
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Request.DisplayName);
    }

    [Theory]
    [InlineData("ab")] // Too short
    [InlineData("x")]  // Too short
    public void DisplayName_WhenProvided_ShouldHaveMinimumLength3(string shortDisplayName)
    {
        // Arrange
        var command = new UpdateRoleCommand(Guid.NewGuid(), new UpdateRoleRequest
        {
            DisplayName = shortDisplayName
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.DisplayName)
            .WithErrorMessage("Display name must be at least 3 characters");
    }

    [Fact]
    public void DisplayName_WhenProvided_ShouldNotExceed100Characters()
    {
        // Arrange
        var longDisplayName = new string('A', 101);
        var command = new UpdateRoleCommand(Guid.NewGuid(), new UpdateRoleRequest
        {
            DisplayName = longDisplayName
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.DisplayName)
            .WithErrorMessage("Display name cannot exceed 100 characters");
    }

    [Fact]
    public void DisplayName_WhenProvided_ShouldAcceptValidValue()
    {
        // Arrange
        var command = new UpdateRoleCommand(Guid.NewGuid(), new UpdateRoleRequest
        {
            DisplayName = "Updated Administrator"
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Request.DisplayName);
    }

    #endregion

    #region Description Validation Tests

    [Fact]
    public void Description_ShouldBeOptional()
    {
        // Arrange
        var command = new UpdateRoleCommand(Guid.NewGuid(), new UpdateRoleRequest
        {
            Description = null, // Optional
            DisplayName = "Valid Display Name"
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Request.Description);
    }

    [Fact]
    public void Description_WhenProvided_ShouldNotExceed500Characters()
    {
        // Arrange
        var longDescription = new string('D', 501);
        var command = new UpdateRoleCommand(Guid.NewGuid(), new UpdateRoleRequest
        {
            Description = longDescription
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.Description)
            .WithErrorMessage("Description cannot exceed 500 characters");
    }

    [Fact]
    public void Description_WhenProvided_ShouldAcceptValidValue()
    {
        // Arrange
        var command = new UpdateRoleCommand(Guid.NewGuid(), new UpdateRoleRequest
        {
            Description = "Updated description with valid length"
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Request.Description);
    }

    #endregion

    #region PermissionIds Validation Tests

    [Fact]
    public void PermissionIds_ShouldBeOptional()
    {
        // Arrange
        var command = new UpdateRoleCommand(Guid.NewGuid(), new UpdateRoleRequest
        {
            DisplayName = "Valid Display Name",
            PermissionIds = null // Optional
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Request.PermissionIds);
    }

    [Fact]
    public void PermissionIds_ShouldNotExceed100Items()
    {
        // Arrange
        var tooManyPermissions = Enumerable.Range(1, 101).Select(_ => Guid.NewGuid()).ToList();
        var command = new UpdateRoleCommand(Guid.NewGuid(), new UpdateRoleRequest
        {
            DisplayName = "Valid Display Name",
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

        var command = new UpdateRoleCommand(Guid.NewGuid(), new UpdateRoleRequest
        {
            DisplayName = "Valid Display Name",
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

        var command = new UpdateRoleCommand(Guid.NewGuid(), new UpdateRoleRequest
        {
            DisplayName = "Valid Display Name",
            PermissionIds = validPermissions
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Request.PermissionIds);
    }

    #endregion

    #region IsActive Field Test

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    [InlineData(null)]
    public void IsActive_ShouldAcceptAnyBooleanValue(bool? isActive)
    {
        // Arrange
        var command = new UpdateRoleCommand(Guid.NewGuid(), new UpdateRoleRequest
        {
            DisplayName = "Valid Display Name",
            IsActive = isActive
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert - IsActive no tiene validaciones especiales
        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion

    #region Complete Valid Request Tests

    [Fact]
    public void CompleteValidRequest_WithAllFields_ShouldPassValidation()
    {
        // Arrange
        var command = new UpdateRoleCommand(Guid.NewGuid(), new UpdateRoleRequest
        {
            DisplayName = "Updated Content Manager",
            Description = "Updated description for content manager role",
            IsActive = true,
            PermissionIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() }
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void PartialUpdate_OnlyDisplayName_ShouldPassValidation()
    {
        // Arrange
        var command = new UpdateRoleCommand(Guid.NewGuid(), new UpdateRoleRequest
        {
            DisplayName = "New Display Name"
            // Other fields are null (partial update)
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void PartialUpdate_OnlyDescription_ShouldPassValidation()
    {
        // Arrange
        var command = new UpdateRoleCommand(Guid.NewGuid(), new UpdateRoleRequest
        {
            Description = "New description"
            // Other fields are null (partial update)
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void PartialUpdate_OnlyPermissions_ShouldPassValidation()
    {
        // Arrange
        var command = new UpdateRoleCommand(Guid.NewGuid(), new UpdateRoleRequest
        {
            PermissionIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() }
            // Other fields are null (partial update)
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion
}
