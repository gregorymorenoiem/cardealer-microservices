using FluentValidation.TestHelper;
using RoleService.Application.DTOs.Permissions;
using RoleService.Application.UseCases.Permissions.CreatePermission;
using Xunit;

namespace RoleService.Tests.Validators;

/// <summary>
/// Tests para CreatePermissionCommandValidator.
/// Valida formato de permisos (resource:action), módulos permitidos, acciones válidas.
/// </summary>
public class CreatePermissionCommandValidatorTests
{
    private readonly CreatePermissionCommandValidator _validator;

    public CreatePermissionCommandValidatorTests()
    {
        _validator = new CreatePermissionCommandValidator();
    }

    #region Name Validation Tests

    [Fact]
    public void Name_ShouldBeRequired()
    {
        // Arrange
        var command = new CreatePermissionCommand(new CreatePermissionRequest
        {
            Name = string.Empty,
            DisplayName = "Valid Display",
            Resource = "users",
            Action = "read",
            Module = "Users"
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.Name)
            .WithErrorMessage("Permission name is required");
    }

    [Fact]
    public void Name_ShouldNotExceed100Characters()
    {
        // Arrange
        var longName = new string('a', 101);
        var command = new CreatePermissionCommand(new CreatePermissionRequest
        {
            Name = longName,
            DisplayName = "Valid Display",
            Resource = "users",
            Action = "read",
            Module = "Users"
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.Name)
            .WithErrorMessage("Permission name cannot exceed 100 characters");
    }

    [Theory]
    [InlineData("users:create")]              // Valid
    [InlineData("vehicles:read")]             // Valid
    [InlineData("dealers:delete")]            // Valid
    public void Name_ShouldAcceptValidFormats(string validName)
    {
        // Arrange
        var command = new CreatePermissionCommand(new CreatePermissionRequest
        {
            Name = validName,
            DisplayName = "Valid Display",
            Resource = "users",
            Action = "read",
            Module = "Users"
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Request.Name);
    }

    [Theory]
    [InlineData("UsersCreate")]           // Missing colon
    [InlineData("users:")]                // Missing action
    [InlineData(":create")]               // Missing resource
    [InlineData("users:Create")]          // Uppercase in action
    [InlineData("Users:create")]          // Uppercase in resource
    [InlineData("users create")]          // Space instead of colon
    [InlineData("users:create:delete")]   // Multiple colons
    [InlineData("users:create_vehicle")]  // Underscore not allowed
    public void Name_ShouldRejectInvalidFormats(string invalidName)
    {
        // Arrange
        var command = new CreatePermissionCommand(new CreatePermissionRequest
        {
            Name = invalidName,
            DisplayName = "Valid Display",
            Resource = "users",
            Action = "read",
            Module = "Users"
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.Name)
            .WithErrorMessage("Permission name must follow format 'resource:action' (e.g., 'users:create', 'vehicles:manage-featured')");
    }

    #endregion

    #region DisplayName Validation Tests

    [Fact]
    public void DisplayName_ShouldBeRequired()
    {
        // Arrange
        var command = new CreatePermissionCommand(new CreatePermissionRequest
        {
            Name = "users:create",
            DisplayName = string.Empty,
            Resource = "users",
            Action = "read",
            Module = "Users"
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.DisplayName)
            .WithErrorMessage("Display name is required");
    }

    [Fact]
    public void DisplayName_ShouldNotExceed150Characters()
    {
        // Arrange
        var longDisplayName = new string('D', 151);
        var command = new CreatePermissionCommand(new CreatePermissionRequest
        {
            Name = "users:create",
            DisplayName = longDisplayName,
            Resource = "users",
            Action = "read",
            Module = "Users"
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.DisplayName)
            .WithErrorMessage("Display name cannot exceed 150 characters");
    }

    #endregion

    #region Description Validation Tests

    [Fact]
    public void Description_ShouldBeOptional()
    {
        // Arrange
        var command = new CreatePermissionCommand(new CreatePermissionRequest
        {
            Name = "users:create",
            DisplayName = "Create Users",
            Resource = "users",
            Action = "create",
            Module = "Users",
            Description = null
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
        var longDescription = new string('D', 501);
        var command = new CreatePermissionCommand(new CreatePermissionRequest
        {
            Name = "users:create",
            DisplayName = "Create Users",
            Resource = "users",
            Action = "create",
            Module = "Users",
            Description = longDescription
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.Description)
            .WithErrorMessage("Description cannot exceed 500 characters");
    }

    #endregion

    #region Resource Validation Tests

    [Fact]
    public void Resource_ShouldBeRequired()
    {
        // Arrange
        var command = new CreatePermissionCommand(new CreatePermissionRequest
        {
            Name = "users:create",
            DisplayName = "Create Users",
            Resource = string.Empty,
            Action = "create",
            Module = "Users"
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.Resource)
            .WithErrorMessage("Resource is required");
    }

    [Fact]
    public void Resource_ShouldNotExceed100Characters()
    {
        // Arrange
        var longResource = new string('r', 101);
        var command = new CreatePermissionCommand(new CreatePermissionRequest
        {
            Name = "users:create",
            DisplayName = "Create Users",
            Resource = longResource,
            Action = "create",
            Module = "Users"
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.Resource)
            .WithErrorMessage("Resource cannot exceed 100 characters");
    }

    [Theory]
    [InlineData("users")]              // Valid
    [InlineData("vehicles")]           // Valid
    [InlineData("user-roles")]         // Valid with hyphen
    [InlineData("featured-vehicles")]  // Valid multiple hyphens
    public void Resource_ShouldAcceptValidFormats(string validResource)
    {
        // Arrange
        var command = new CreatePermissionCommand(new CreatePermissionRequest
        {
            Name = "users:create",
            DisplayName = "Create Users",
            Resource = validResource,
            Action = "create",
            Module = "Users"
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Request.Resource);
    }

    [Theory]
    [InlineData("Users")]          // Uppercase
    [InlineData("User_Roles")]     // Underscore not allowed
    [InlineData("user roles")]     // Space not allowed
    [InlineData("user@roles")]     // Special char not allowed
    public void Resource_ShouldRejectInvalidFormats(string invalidResource)
    {
        // Arrange
        var command = new CreatePermissionCommand(new CreatePermissionRequest
        {
            Name = "users:create",
            DisplayName = "Create Users",
            Resource = invalidResource,
            Action = "create",
            Module = "Users"
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.Resource)
            .WithErrorMessage("Resource must be lowercase alphanumeric with optional hyphens");
    }

    #endregion

    #region Action Validation Tests

    [Fact]
    public void Action_ShouldBeRequired()
    {
        // Arrange
        var command = new CreatePermissionCommand(new CreatePermissionRequest
        {
            Name = "users:create",
            DisplayName = "Create Users",
            Resource = "users",
            Action = string.Empty,
            Module = "Users"
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.Action)
            .WithErrorMessage("Action is required");
    }

    [Theory]
    [InlineData("Read")]
    [InlineData("Create")]
    [InlineData("Update")]
    [InlineData("Delete")]
    [InlineData("Execute")]  // Este SÍ es válido según el enum real
    public void Action_ShouldAcceptValidEnumValues(string validAction)
    {
        // Arrange
        var command = new CreatePermissionCommand(new CreatePermissionRequest
        {
            Name = "users:create",
            DisplayName = "Create Users",
            Resource = "users",
            Action = validAction,
            Module = "Users"
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Request.Action);
    }

    [Theory]
    [InlineData("InvalidAction")]
    [InlineData("Manage")]  // Manage NO existe en el enum
    [InlineData("Run")]
    public void Action_ShouldRejectInvalidEnumValues(string invalidAction)
    {
        // Arrange
        var command = new CreatePermissionCommand(new CreatePermissionRequest
        {
            Name = "users:create",
            DisplayName = "Create Users",
            Resource = "users",
            Action = invalidAction,
            Module = "Users"
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.Action);
    }

    #endregion

    #region Module Validation Tests

    [Fact]
    public void Module_ShouldBeRequired()
    {
        // Arrange
        var command = new CreatePermissionCommand(new CreatePermissionRequest
        {
            Name = "users:create",
            DisplayName = "Create Users",
            Resource = "users",
            Action = "create",
            Module = string.Empty
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.Module)
            .WithErrorMessage("Module is required");
    }

    [Fact]
    public void Module_ShouldNotExceed100Characters()
    {
        // Arrange
        var longModule = new string('M', 101);
        var command = new CreatePermissionCommand(new CreatePermissionRequest
        {
            Name = "users:create",
            DisplayName = "Create Users",
            Resource = "users",
            Action = "create",
            Module = longModule
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.Module)
            .WithErrorMessage("Module cannot exceed 100 characters");
    }

    [Theory]
    [InlineData("users")]           // Minúscula, válido en AllowedModules
    [InlineData("dealers")]
    [InlineData("vehicles")]
    [InlineData("billing")]
    [InlineData("media")]
    [InlineData("notifications")]
    [InlineData("reports")]
    [InlineData("analytics")]
    [InlineData("admin")]
    [InlineData("auth")]
    [InlineData("kyc")]
    [InlineData("support")]
    public void Module_ShouldAcceptValidModules(string validModule)
    {
        // Arrange
        var command = new CreatePermissionCommand(new CreatePermissionRequest
        {
            Name = "users:create",
            DisplayName = "Create Users",
            Resource = "users",
            Action = "create",
            Module = validModule
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Request.Module);
    }

    [Theory]
    [InlineData("InvalidModule")]
    [InlineData("NonExistentModule")]
    [InlineData("Foo")]
    [InlineData("Roles")]        // NO está en AllowedModules
    [InlineData("Permissions")]  // NO está en AllowedModules
    [InlineData("Audit")]        // NO está en AllowedModules
    public void Module_ShouldRejectInvalidModules(string invalidModule)
    {
        // Arrange
        var command = new CreatePermissionCommand(new CreatePermissionRequest
        {
            Name = "users:create",
            DisplayName = "Create Users",
            Resource = "users",
            Action = "create",
            Module = invalidModule
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.Module);
    }

    #endregion

    #region Name Consistency Validation Test

    [Theory]
    [InlineData("users", "create", "users:create")]        // Valid
    [InlineData("vehicles", "read", "vehicles:read")]      // Valid
    [InlineData("dealers", "delete", "dealers:delete")]    // Valid
    public void Name_ShouldMatchResourceAndAction(string resource, string action, string expectedName)
    {
        // Arrange
        var command = new CreatePermissionCommand(new CreatePermissionRequest
        {
            Name = expectedName,
            DisplayName = "Valid Display",
            Resource = resource,
            Action = action,
            Module = "Users"
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Request);
    }

    [Fact]
    public void Name_ShouldFailWhenNotMatchingResourceAndAction()
    {
        // Arrange
        var command = new CreatePermissionCommand(new CreatePermissionRequest
        {
            Name = "vehicles:create",    // Name says "vehicles"
            DisplayName = "Create Users",
            Resource = "users",          // But resource is "users"
            Action = "create",
            Module = "Users"
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request)
            .WithErrorMessage("Permission name must match 'resource:action' format based on provided Resource and Action values");
    }

    #endregion

    #region Complete Valid Request Test

    [Fact]
    public void CompleteValidRequest_ShouldPassValidation()
    {
        // Arrange
        var command = new CreatePermissionCommand(new CreatePermissionRequest
        {
            Name = "vehicles:create",
            DisplayName = "Create Vehicles",
            Description = "Allows user to create new vehicle listings",
            Resource = "vehicles",
            Action = "Create",      // Válido en PermissionAction enum
            Module = "vehicles"     // Minúscula, válido en AllowedModules
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion
}
