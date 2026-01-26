using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RoleService.Application.DTOs.Roles;
using RoleService.Application.UseCases.Roles.GetRoles;
using RoleService.Domain.Entities;
using RoleService.Domain.Interfaces;
using RoleService.Shared.Models;
using Xunit;

namespace RoleService.Tests.Handlers.Roles;

public class GetRolesQueryHandlerTests
{
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly Mock<ILogger<GetRolesQueryHandler>> _loggerMock;
    private readonly GetRolesQueryHandler _handler;

    public GetRolesQueryHandlerTests()
    {
        _roleRepositoryMock = new Mock<IRoleRepository>();
        _loggerMock = new Mock<ILogger<GetRolesQueryHandler>>();

        _handler = new GetRolesQueryHandler(
            _roleRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPaginatedRoles()
    {
        // Arrange
        var roles = new List<Role>
        {
            new Role { Id = Guid.NewGuid(), Name = "Admin", DisplayName = "Administrator", IsActive = true },
            new Role { Id = Guid.NewGuid(), Name = "User", DisplayName = "Regular User", IsActive = true }
        };

        _roleRepositoryMock.Setup(x => x.GetPaginatedAsync(null, 1, 50, It.IsAny<CancellationToken>()))
            .ReturnsAsync((roles, 2));

        var query = new GetRolesQuery(null, 1, 50);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_WithIsActiveFilter_ShouldFilterRoles()
    {
        // Arrange
        var activeRoles = new List<Role>
        {
            new Role { Id = Guid.NewGuid(), Name = "Admin", IsActive = true }
        };

        _roleRepositoryMock.Setup(x => x.GetPaginatedAsync(true, 1, 50, It.IsAny<CancellationToken>()))
            .ReturnsAsync((activeRoles, 1));

        var query = new GetRolesQuery(true, 1, 50);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_EmptyResult_ShouldReturnEmptyList()
    {
        // Arrange
        _roleRepositoryMock.Setup(x => x.GetPaginatedAsync(It.IsAny<bool?>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Role>(), 0));

        var query = new GetRolesQuery(null, 1, 50);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }
}
