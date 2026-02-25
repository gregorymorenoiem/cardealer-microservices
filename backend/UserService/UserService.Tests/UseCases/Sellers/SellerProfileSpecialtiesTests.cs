using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using UserService.Application.DTOs;
using UserService.Application.UseCases.Sellers.CreateSellerProfile;
using UserService.Application.UseCases.Sellers.UpdateSellerProfile;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;
using UserService.Shared.Exceptions;

namespace UserService.Tests.UseCases.Sellers;

/// <summary>
/// Tests para FASE 1: Especialidades en SellerProfile
/// </summary>
public class SellerProfileSpecialtiesTests
{
    private readonly Mock<ISellerProfileRepository> _mockSellerProfileRepository;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<ILogger<CreateSellerProfileCommandHandler>> _mockLoggerCreate;
    private readonly Mock<ILogger<UpdateSellerProfileCommandHandler>> _mockLoggerUpdate;

    public SellerProfileSpecialtiesTests()
    {
        _mockSellerProfileRepository = new Mock<ISellerProfileRepository>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockLoggerCreate = new Mock<ILogger<CreateSellerProfileCommandHandler>>();
        _mockLoggerUpdate = new Mock<ILogger<UpdateSellerProfileCommandHandler>>();
    }

    #region CreateSellerProfileCommand Tests

    [Fact]
    public async Task CreateSellerProfile_WithSpecialties_ShouldPersistSpecialties()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var specialties = new[] { "Sedanes", "SUVs", "Camionetas" };

        var request = new CreateSellerProfileRequest
        {
            UserId = userId,
            FullName = "Juan García",
            City = "Santo Domingo",
            Country = "DO",
            Specialties = specialties
        };

        var user = new User { Id = userId, Email = "juan@example.com" };
        _mockUserRepository.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _mockSellerProfileRepository.Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync((SellerProfile?)null);

        SellerProfile? capturedProfile = null;
        _mockSellerProfileRepository.Setup(r => r.AddAsync(It.IsAny<SellerProfile>()))
            .Callback<SellerProfile>(p => capturedProfile = p)
            .ReturnsAsync((SellerProfile p) => p);

        var handler = new CreateSellerProfileCommandHandler(
            _mockSellerProfileRepository.Object,
            _mockUserRepository.Object,
            _mockLoggerCreate.Object);

        var command = new CreateSellerProfileCommand(request);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Specialties);
        Assert.Equal(specialties.Length, result.Specialties.Length);
        Assert.NotNull(capturedProfile);
        Assert.NotEmpty(capturedProfile.Specialties);
        Assert.Equal("Sedanes", capturedProfile.Specialties[0]);
        _mockSellerProfileRepository.Verify(r => r.AddAsync(It.IsAny<SellerProfile>()), Times.Once);
    }

    [Fact]
    public async Task CreateSellerProfile_WithoutSpecialties_ShouldCreateWithEmptyArray()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var request = new CreateSellerProfileRequest
        {
            UserId = userId,
            FullName = "Juan García",
            City = "Santo Domingo",
            Country = "DO",
            Specialties = null // No specialties
        };

        var user = new User { Id = userId, Email = "juan@example.com" };
        _mockUserRepository.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _mockSellerProfileRepository.Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync((SellerProfile?)null);

        SellerProfile? capturedProfile2 = null;
        _mockSellerProfileRepository.Setup(r => r.AddAsync(It.IsAny<SellerProfile>()))
            .Callback<SellerProfile>(p => capturedProfile2 = p)
            .ReturnsAsync((SellerProfile p) => p);

        var handler = new CreateSellerProfileCommandHandler(
            _mockSellerProfileRepository.Object,
            _mockUserRepository.Object,
            _mockLoggerCreate.Object);

        var command = new CreateSellerProfileCommand(request);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Specialties);
        Assert.Empty(result.Specialties);
        Assert.NotNull(capturedProfile2);
        Assert.Empty(capturedProfile2.Specialties);
    }

    [Fact]
    public async Task CreateSellerProfile_WithMaxSpecialties_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var specialties = Enumerable.Range(1, 10)
            .Select(i => $"Especialidad{i}")
            .ToArray();

        var request = new CreateSellerProfileRequest
        {
            UserId = userId,
            FullName = "Juan García",
            City = "Santo Domingo",
            Country = "DO",
            Specialties = specialties
        };

        var user = new User { Id = userId, Email = "juan@example.com" };
        _mockUserRepository.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _mockSellerProfileRepository.Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync((SellerProfile?)null);

        _mockSellerProfileRepository.Setup(r => r.AddAsync(It.IsAny<SellerProfile>()))
            .ReturnsAsync((SellerProfile p) => p);

        var handler = new CreateSellerProfileCommandHandler(
            _mockSellerProfileRepository.Object,
            _mockUserRepository.Object,
            _mockLoggerCreate.Object);

        var command = new CreateSellerProfileCommand(request);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10, result.Specialties.Length);
    }

    #endregion

    #region UpdateSellerProfileCommand Tests

    [Fact]
    public async Task UpdateSellerProfile_WithNewSpecialties_ShouldUpdate()
    {
        // Arrange
        var sellerId = Guid.NewGuid();
        var oldSpecialties = new[] { "Sedanes" };
        var newSpecialties = new[] { "SUVs", "Camionetas", "Minivanes" };

        var existingProfile = new SellerProfile
        {
            Id = sellerId,
            UserId = Guid.NewGuid(),
            FullName = "Juan García",
            City = "Santo Domingo",
            Specialties = oldSpecialties
        };

        var request = new UpdateSellerProfileRequest
        {
            Specialties = newSpecialties
        };

        _mockSellerProfileRepository.Setup(r => r.GetByIdAsync(sellerId))
            .ReturnsAsync(existingProfile);

        SellerProfile? capturedProfile3 = null;
        _mockSellerProfileRepository.Setup(r => r.UpdateAsync(It.IsAny<SellerProfile>()))
            .Callback<SellerProfile>(p => capturedProfile3 = p)
            .ReturnsAsync((SellerProfile p) => p);

        var handler = new UpdateSellerProfileCommandHandler(
            _mockSellerProfileRepository.Object,
            _mockLoggerUpdate.Object);

        var command = new UpdateSellerProfileCommand(sellerId, request);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Specialties.Length);
        Assert.NotNull(capturedProfile3);
        Assert.Equal(newSpecialties.Length, capturedProfile3.Specialties.Length);
        Assert.Equal("SUVs", capturedProfile3.Specialties[0]);
        _mockSellerProfileRepository.Verify(r => r.UpdateAsync(It.IsAny<SellerProfile>()), Times.Once);
    }

    [Fact]
    public async Task UpdateSellerProfile_ClearSpecialties_ShouldSucceed()
    {
        // Arrange
        var sellerId = Guid.NewGuid();

        var existingProfile = new SellerProfile
        {
            Id = sellerId,
            UserId = Guid.NewGuid(),
            FullName = "Juan García",
            City = "Santo Domingo",
            Specialties = new[] { "Sedanes", "SUVs" }
        };

        var request = new UpdateSellerProfileRequest
        {
            Specialties = Array.Empty<string>() // Clear specialties
        };

        _mockSellerProfileRepository.Setup(r => r.GetByIdAsync(sellerId))
            .ReturnsAsync(existingProfile);

        SellerProfile? capturedProfile4 = null;
        _mockSellerProfileRepository.Setup(r => r.UpdateAsync(It.IsAny<SellerProfile>()))
            .Callback<SellerProfile>(p => capturedProfile4 = p)
            .ReturnsAsync((SellerProfile p) => p);

        var handler = new UpdateSellerProfileCommandHandler(
            _mockSellerProfileRepository.Object,
            _mockLoggerUpdate.Object);

        var command = new UpdateSellerProfileCommand(sellerId, request);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Specialties);
        Assert.NotNull(capturedProfile4);
        Assert.Empty(capturedProfile4.Specialties);
    }

    [Fact]
    public async Task UpdateSellerProfile_WithoutSpecialties_ShouldNotChange()
    {
        // Arrange
        var sellerId = Guid.NewGuid();
        var originalSpecialties = new[] { "Sedanes", "SUVs" };

        var existingProfile = new SellerProfile
        {
            Id = sellerId,
            UserId = Guid.NewGuid(),
            FullName = "Juan García",
            City = "Santo Domingo",
            Specialties = originalSpecialties
        };

        var request = new UpdateSellerProfileRequest
        {
            FullName = "Juan García Updated"
            // No specialties in request
        };

        _mockSellerProfileRepository.Setup(r => r.GetByIdAsync(sellerId))
            .ReturnsAsync(existingProfile);

        SellerProfile? capturedProfile = null;
        _mockSellerProfileRepository.Setup(r => r.UpdateAsync(It.IsAny<SellerProfile>()))
            .Callback<SellerProfile>(p =>
            {
                capturedProfile = p;
            })
            .ReturnsAsync((SellerProfile p) => p);

        var handler = new UpdateSellerProfileCommandHandler(
            _mockSellerProfileRepository.Object,
            _mockLoggerUpdate.Object);

        var command = new UpdateSellerProfileCommand(sellerId, request);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(originalSpecialties.Length, result.Specialties.Length);
        Assert.Equal("Sedanes", result.Specialties[0]);
    }

    #endregion

    #region Error Cases

    [Fact]
    public async Task UpdateSellerProfile_ProfileNotFound_ShouldThrowException()
    {
        // Arrange
        var sellerId = Guid.NewGuid();

        _mockSellerProfileRepository.Setup(r => r.GetByIdAsync(sellerId))
            .ReturnsAsync((SellerProfile?)null);

        var handler = new UpdateSellerProfileCommandHandler(
            _mockSellerProfileRepository.Object,
            _mockLoggerUpdate.Object);

        var command = new UpdateSellerProfileCommand(
            sellerId,
            new UpdateSellerProfileRequest { Specialties = new[] { "Test" } });

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task CreateSellerProfile_UserNotFound_ShouldThrowException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockUserRepository.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        var handler = new CreateSellerProfileCommandHandler(
            _mockSellerProfileRepository.Object,
            _mockUserRepository.Object,
            _mockLoggerCreate.Object);

        var request = new CreateSellerProfileRequest
        {
            UserId = userId,
            FullName = "Juan García",
            Specialties = new[] { "Sedanes" }
        };

        var command = new CreateSellerProfileCommand(request);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(command, CancellationToken.None));
    }

    #endregion

    #region DTO Mapping Tests

    [Fact]
    public void SellerProfileDto_SpecialtiesProperty_ShouldBeSerializable()
    {
        // Arrange
        var dto = new SellerProfileDto
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            FullName = "Test Seller",
            Specialties = new[] { "Sedanes", "SUVs" },
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var json = System.Text.Json.JsonSerializer.Serialize(dto);

        // Assert
        Assert.NotNull(json);
        Assert.Contains("Specialties", json);
        Assert.Contains("Sedanes", json);
    }

    [Fact]
    public void CreateSellerProfileRequest_SpecialtiesProperty_ShouldBeValid()
    {
        // Arrange
        var request = new CreateSellerProfileRequest
        {
            UserId = Guid.NewGuid(),
            FullName = "Test Seller",
            Specialties = new[] { "Sedanes", "SUVs", "Camionetas" }
        };

        // Act
        var specialties = request.Specialties;

        // Assert
        Assert.NotNull(specialties);
        Assert.Equal(3, specialties.Length);
    }

    #endregion
}
