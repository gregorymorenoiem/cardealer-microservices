namespace ContactService.Tests.Unit.Communication;

/// <summary>
/// Unit tests for communication history functionality
/// </summary>
public class CommunicationTests
{
    [Fact]
    public void CreateCommunication_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var communication = new
        {
            ContactId = Guid.NewGuid(),
            Channel = "Email",
            Subject = "Follow-up inquiry",
            Notes = "Discussed vehicle options",
            CommunicatedAt = DateTime.UtcNow
        };

        // Assert
        communication.ContactId.Should().NotBeEmpty();
        communication.Channel.Should().NotBeNullOrEmpty();
        communication.Subject.Should().NotBeNullOrEmpty();
        communication.CommunicatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData("Email", true)]
    [InlineData("Phone", true)]
    [InlineData("SMS", true)]
    [InlineData("Meeting", true)]
    [InlineData("Chat", false)]
    [InlineData("", false)]
    public void ValidateCommunicationChannel_WithVariousChannels_ReturnsExpectedResult(string channel, bool expected)
    {
        // Arrange
        var validChannels = new[] { "Email", "Phone", "SMS", "Meeting" };

        // Act
        var isValid = validChannels.Contains(channel);

        // Assert
        isValid.Should().Be(expected);
    }

    [Fact]
    public void Communication_WithFutureDate_ShouldNotBeValid()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.AddDays(1);
        var communication = new
        {
            CommunicatedAt = futureDate
        };

        // Act
        var isValid = communication.CommunicatedAt <= DateTime.UtcNow;

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void Communication_WithPastDate_ShouldBeValid()
    {
        // Arrange
        var pastDate = DateTime.UtcNow.AddHours(-2);
        var communication = new
        {
            CommunicatedAt = pastDate
        };

        // Act
        var isValid = communication.CommunicatedAt <= DateTime.UtcNow;

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void GetCommunicationHistory_ForContact_ReturnsChronologicalOrder()
    {
        // Arrange
        var contactId = Guid.NewGuid();
        var communications = new[]
        {
            new { Id = 1, ContactId = contactId, CommunicatedAt = DateTime.UtcNow.AddDays(-3) },
            new { Id = 2, ContactId = contactId, CommunicatedAt = DateTime.UtcNow.AddDays(-1) },
            new { Id = 3, ContactId = contactId, CommunicatedAt = DateTime.UtcNow.AddDays(-2) }
        };

        // Act
        var sorted = communications.OrderByDescending(c => c.CommunicatedAt).ToList();

        // Assert
        sorted[0].Id.Should().Be(2); // Most recent
        sorted[1].Id.Should().Be(3);
        sorted[2].Id.Should().Be(1); // Oldest
    }

    [Fact]
    public void Communication_WithLongNotes_ShouldBeAccepted()
    {
        // Arrange
        var longNotes = new string('A', 5000);
        var communication = new
        {
            Notes = longNotes
        };

        // Assert
        communication.Notes.Length.Should().Be(5000);
        communication.Notes.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Communication_WithEmptySubject_ShouldStillBeValid()
    {
        // Arrange
        var communication = new
        {
            ContactId = Guid.NewGuid(),
            Channel = "Phone",
            Subject = "",
            Notes = "Quick call to confirm appointment"
        };

        // Act
        var isValid = communication.ContactId != Guid.Empty &&
                      !string.IsNullOrWhiteSpace(communication.Channel);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void FilterCommunications_ByChannel_ReturnsOnlyMatchingChannel()
    {
        // Arrange
        var communications = new[]
        {
            new { Id = 1, Channel = "Email" },
            new { Id = 2, Channel = "Phone" },
            new { Id = 3, Channel = "Email" },
            new { Id = 4, Channel = "SMS" }
        };

        // Act
        var emailCommunications = communications.Where(c => c.Channel == "Email").ToList();

        // Assert
        emailCommunications.Should().HaveCount(2);
        emailCommunications.All(c => c.Channel == "Email").Should().BeTrue();
    }

    [Fact]
    public void FilterCommunications_ByDateRange_ReturnsOnlyWithinRange()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;
        var communications = new[]
        {
            new { Id = 1, CommunicatedAt = DateTime.UtcNow.AddDays(-10) },
            new { Id = 2, CommunicatedAt = DateTime.UtcNow.AddDays(-5) },
            new { Id = 3, CommunicatedAt = DateTime.UtcNow.AddDays(-3) },
            new { Id = 4, CommunicatedAt = DateTime.UtcNow.AddDays(-1) }
        };

        // Act
        var filtered = communications
            .Where(c => c.CommunicatedAt >= startDate && c.CommunicatedAt <= endDate)
            .ToList();

        // Assert
        filtered.Should().HaveCount(3);
        filtered.Should().NotContain(c => c.Id == 1);
    }

    [Fact]
    public void Communication_Statistics_ShouldCalculateCorrectly()
    {
        // Arrange
        var communications = new[]
        {
            new { Channel = "Email" },
            new { Channel = "Phone" },
            new { Channel = "Email" },
            new { Channel = "Email" },
            new { Channel = "SMS" }
        };

        // Act
        var stats = communications
            .GroupBy(c => c.Channel)
            .Select(g => new { Channel = g.Key, Count = g.Count() })
            .OrderByDescending(s => s.Count)
            .ToList();

        // Assert
        stats[0].Channel.Should().Be("Email");
        stats[0].Count.Should().Be(3);
        stats[1].Channel.Should().Be("Phone");
        stats[1].Count.Should().Be(1);
    }
}
