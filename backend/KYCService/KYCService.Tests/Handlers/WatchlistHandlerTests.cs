using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit;
using KYCService.Application.Commands;
using KYCService.Application.DTOs;
using KYCService.Application.Handlers;
using KYCService.Application.Queries;
using KYCService.Domain.Entities;
using KYCService.Domain.Interfaces;

namespace KYCService.Tests.Handlers;

#region AddWatchlistEntryHandler Tests

public class AddWatchlistEntryHandlerTests
{
    private readonly Mock<IWatchlistRepository> _repositoryMock;
    private readonly AddWatchlistEntryHandler _handler;

    public AddWatchlistEntryHandlerTests()
    {
        _repositoryMock = new Mock<IWatchlistRepository>();
        _handler = new AddWatchlistEntryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ValidPEPEntry_ShouldCreateEntry()
    {
        // Arrange
        var command = new AddWatchlistEntryCommand
        {
            FullName = "Juan Político",
            ListType = WatchlistType.PEP,
            Source = "Junta Central Electoral",
            Details = "Diputado activo",
            Nationality = "Dominicana"
        };

        _repositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<WatchlistEntry>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((WatchlistEntry e, CancellationToken ct) => e);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.FullName.Should().Be("Juan Político");
        result.ListType.Should().Be(WatchlistType.PEP);
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<WatchlistEntry>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_SanctionsEntry_ShouldSetCorrectListType()
    {
        // Arrange
        var command = new AddWatchlistEntryCommand
        {
            FullName = "Sanctioned Entity",
            ListType = WatchlistType.Sanctions,
            Source = "OFAC SDN List",
            Details = "Added to OFAC SDN list"
        };

        _repositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<WatchlistEntry>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((WatchlistEntry e, CancellationToken ct) => e);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ListType.Should().Be(WatchlistType.Sanctions);
    }

    [Fact]
    public async Task Handle_EntryWithAliases_ShouldStoreAliases()
    {
        // Arrange
        var command = new AddWatchlistEntryCommand
        {
            FullName = "John Smith",
            ListType = WatchlistType.InternalBlacklist,
            Source = "Internal Review",
            Aliases = new List<string> { "Johnny S", "J. Smith", "John S." }
        };

        _repositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<WatchlistEntry>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((WatchlistEntry e, CancellationToken ct) => e);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Aliases.Should().Contain("Johnny S");
        result.Aliases.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_EntryWithDocumentNumber_ShouldStore()
    {
        // Arrange
        var command = new AddWatchlistEntryCommand
        {
            FullName = "Maria García",
            ListType = WatchlistType.PEP,
            Source = "JCE",
            DocumentNumber = "001-1234567-8",
            DateOfBirth = new DateTime(1980, 5, 15),
            Nationality = "Dominicana"
        };

        _repositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<WatchlistEntry>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((WatchlistEntry e, CancellationToken ct) => e);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.DocumentNumber.Should().Be("001-1234567-8");
        result.DateOfBirth.Should().Be(new DateTime(1980, 5, 15));
    }
}

#endregion

#region ScreenWatchlistHandler Tests

public class ScreenWatchlistHandlerTests
{
    private readonly Mock<IWatchlistRepository> _repositoryMock;
    private readonly ScreenWatchlistHandler _handler;

    public ScreenWatchlistHandlerTests()
    {
        _repositoryMock = new Mock<IWatchlistRepository>();
        _handler = new ScreenWatchlistHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_NoMatches_ShouldReturnEmptyResult()
    {
        // Arrange
        var command = new ScreenWatchlistCommand
        {
            FullName = "Regular Person",
            DocumentNumber = "002-9876543-2"
        };

        _repositoryMock
            .Setup(r => r.ScreenAsync(
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<WatchlistMatchResult>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.HasMatches.Should().BeFalse();
        result.TotalMatches.Should().Be(0);
        result.Matches.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithMatches_ShouldReturnMatchingEntries()
    {
        // Arrange
        var command = new ScreenWatchlistCommand
        {
            FullName = "Juan Político",
            DocumentNumber = "001-1234567-8"
        };

        var matchedEntry = new WatchlistEntry
        {
            Id = Guid.NewGuid(),
            FullName = "Juan Político",
            ListType = WatchlistType.PEP,
            Source = "JCE",
            DocumentNumber = "001-1234567-8",
            ListedDate = DateTime.UtcNow,
            IsActive = true
        };

        var matches = new List<WatchlistMatchResult>
        {
            new WatchlistMatchResult
            {
                Entry = matchedEntry,
                MatchScore = 95,
                MatchedFields = new List<string> { "FullName", "DocumentNumber" },
                IsExactMatch = true
            }
        };

        _repositoryMock
            .Setup(r => r.ScreenAsync(
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(matches);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.HasMatches.Should().BeTrue();
        result.TotalMatches.Should().Be(1);
        result.Matches.Should().HaveCount(1);
        result.Matches[0].MatchScore.Should().Be(95);
        result.Matches[0].IsExactMatch.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_MultipleMatches_ShouldReturnAllMatches()
    {
        // Arrange
        var command = new ScreenWatchlistCommand
        {
            FullName = "Common Name"
        };

        var matches = new List<WatchlistMatchResult>
        {
            new WatchlistMatchResult
            {
                Entry = new WatchlistEntry { Id = Guid.NewGuid(), FullName = "Common Name 1", ListType = WatchlistType.PEP },
                MatchScore = 90,
                MatchedFields = new List<string> { "FullName" },
                IsExactMatch = false
            },
            new WatchlistMatchResult
            {
                Entry = new WatchlistEntry { Id = Guid.NewGuid(), FullName = "Common Name 2", ListType = WatchlistType.Sanctions },
                MatchScore = 85,
                MatchedFields = new List<string> { "FullName" },
                IsExactMatch = false
            }
        };

        _repositoryMock
            .Setup(r => r.ScreenAsync(
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(matches);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.HasMatches.Should().BeTrue();
        result.TotalMatches.Should().Be(2);
        result.Matches.Should().HaveCount(2);
    }
}

#endregion

#region SearchWatchlistHandler Tests

public class SearchWatchlistHandlerTests
{
    private readonly Mock<IWatchlistRepository> _repositoryMock;
    private readonly SearchWatchlistHandler _handler;

    public SearchWatchlistHandlerTests()
    {
        _repositoryMock = new Mock<IWatchlistRepository>();
        _handler = new SearchWatchlistHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_SearchByType_ShouldReturnMatchingEntries()
    {
        // Arrange
        var query = new SearchWatchlistQuery
        {
            ListType = WatchlistType.PEP
        };

        var entries = new List<WatchlistEntry>
        {
            new WatchlistEntry
            {
                Id = Guid.NewGuid(),
                FullName = "Político 1",
                ListType = WatchlistType.PEP,
                Source = "JCE",
                ListedDate = DateTime.UtcNow,
                IsActive = true
            },
            new WatchlistEntry
            {
                Id = Guid.NewGuid(),
                FullName = "Político 2",
                ListType = WatchlistType.PEP,
                Source = "JCE",
                ListedDate = DateTime.UtcNow,
                IsActive = true
            }
        };

        // SearchAsync(string searchTerm, WatchlistType? type, CancellationToken)
        _repositoryMock
            .Setup(r => r.SearchAsync(
                It.IsAny<string>(),
                It.IsAny<WatchlistType?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(entries);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.All(e => e.ListType == WatchlistType.PEP).Should().BeTrue();
    }

    [Fact]
    public async Task Handle_SearchByName_ShouldReturnMatchingEntries()
    {
        // Arrange
        var query = new SearchWatchlistQuery
        {
            SearchTerm = "García"
        };

        var entries = new List<WatchlistEntry>
        {
            new WatchlistEntry
            {
                Id = Guid.NewGuid(),
                FullName = "María García",
                ListType = WatchlistType.PEP,
                Source = "JCE",
                ListedDate = DateTime.UtcNow,
                IsActive = true
            }
        };

        _repositoryMock
            .Setup(r => r.SearchAsync(
                It.IsAny<string>(),
                It.IsAny<WatchlistType?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(entries);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].FullName.Should().Contain("García");
    }

    [Fact]
    public async Task Handle_NoResults_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new SearchWatchlistQuery
        {
            SearchTerm = "NonexistentName"
        };

        _repositoryMock
            .Setup(r => r.SearchAsync(
                It.IsAny<string>(),
                It.IsAny<WatchlistType?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<WatchlistEntry>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}

#endregion

#region GetWatchlistEntryByIdHandler Tests

public class GetWatchlistEntryByIdHandlerTests
{
    private readonly Mock<IWatchlistRepository> _repositoryMock;
    private readonly GetWatchlistEntryByIdHandler _handler;

    public GetWatchlistEntryByIdHandlerTests()
    {
        _repositoryMock = new Mock<IWatchlistRepository>();
        _handler = new GetWatchlistEntryByIdHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_EntryExists_ShouldReturnEntry()
    {
        // Arrange
        var entryId = Guid.NewGuid();
        var query = new GetWatchlistEntryByIdQuery(entryId);

        var entry = new WatchlistEntry
        {
            Id = entryId,
            FullName = "Test Person",
            ListType = WatchlistType.PEP,
            Source = "Test Source",
            ListedDate = DateTime.UtcNow,
            IsActive = true
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(entryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entry);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(entryId);
        result.FullName.Should().Be("Test Person");
    }

    [Fact]
    public async Task Handle_EntryNotFound_ShouldReturnNull()
    {
        // Arrange
        var entryId = Guid.NewGuid();
        var query = new GetWatchlistEntryByIdQuery(entryId);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(entryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WatchlistEntry?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}

#endregion
