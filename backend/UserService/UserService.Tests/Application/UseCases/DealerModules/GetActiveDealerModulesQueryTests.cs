using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using UserService.Application.UseCases.DealerModules;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;

namespace UserService.Tests.Application.UseCases.DealerModules;

public class GetActiveDealerModulesQueryTests
{
    private readonly Mock<IDealerModuleRepository> _dealerModuleRepositoryMock;
    private readonly Mock<ILogger<GetActiveDealerModulesQueryHandler>> _loggerMock;
    private readonly GetActiveDealerModulesQueryHandler _handler;

    public GetActiveDealerModulesQueryTests()
    {
        _dealerModuleRepositoryMock = new Mock<IDealerModuleRepository>();
        _loggerMock = new Mock<ILogger<GetActiveDealerModulesQueryHandler>>();
        _handler = new GetActiveDealerModulesQueryHandler(
            _dealerModuleRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnActiveModules_WhenModulesExist()
    {
        // Arrange
        var dealerId = Guid.NewGuid();
        var dealerModules = new List<DealerModule>
        {
            new DealerModule
            {
                Id = Guid.NewGuid(),
                DealerId = dealerId,
                ModuleId = Guid.NewGuid(),
                IsActive = true,
                ActivatedAt = DateTime.UtcNow.AddDays(-30),
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                Module = new Module { Name = "CRM Module", IsActive = true }
            },
            new DealerModule
            {
                Id = Guid.NewGuid(),
                DealerId = dealerId,
                ModuleId = Guid.NewGuid(),
                IsActive = true,
                ActivatedAt = DateTime.UtcNow.AddDays(-15),
                Module = new Module { Name = "Analytics Module", IsActive = true }
            }
        };

        _dealerModuleRepositoryMock.Setup(r => r.GetActiveByDealerIdAsync(dealerId))
            .ReturnsAsync(dealerModules);

        var query = new GetActiveDealerModulesQuery(dealerId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].ModuleName.Should().Be("CRM Module");
        result[1].ModuleName.Should().Be("Analytics Module");
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoActiveModules()
    {
        // Arrange
        var dealerId = Guid.NewGuid();
        _dealerModuleRepositoryMock.Setup(r => r.GetActiveByDealerIdAsync(dealerId))
            .ReturnsAsync(new List<DealerModule>());

        var query = new GetActiveDealerModulesQuery(dealerId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}
