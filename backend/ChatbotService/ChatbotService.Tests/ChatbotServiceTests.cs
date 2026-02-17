using Xunit;
using FluentAssertions;
using ChatbotService.Domain.Entities;
using ChatbotService.Domain.Enums;

namespace ChatbotService.Tests;

public class ChatSessionTests
{
    [Fact]
    public void ChatSession_ShouldBeCreated_WithValidData()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var configId = Guid.NewGuid();

        // Act
        var session = new ChatSession
        {
            Id = sessionId,
            ChatbotConfigurationId = configId,
            SessionToken = "test-token-123",
            SessionType = SessionType.WebChat,
            Status = SessionStatus.Active,
            Channel = "web",
            MaxInteractionsPerSession = 10,
            Language = "es"
        };

        // Assert
        session.Id.Should().Be(sessionId);
        session.ChatbotConfigurationId.Should().Be(configId);
        session.SessionToken.Should().Be("test-token-123");
        session.SessionType.Should().Be(SessionType.WebChat);
        session.Status.Should().Be(SessionStatus.Active);
        session.MaxInteractionsPerSession.Should().Be(10);
        session.MessageCount.Should().Be(0);
        session.InteractionCount.Should().Be(0);
    }

    [Fact]
    public void ChatSession_ShouldTrackMessages()
    {
        // Arrange
        var session = new ChatSession
        {
            Id = Guid.NewGuid(),
            ChatbotConfigurationId = Guid.NewGuid(),
            SessionToken = "token-1"
        };

        // Act
        session.MessageCount = 5;
        session.InteractionCount = 3;

        // Assert
        session.MessageCount.Should().Be(5);
        session.InteractionCount.Should().Be(3);
    }

    [Fact]
    public void ChatSession_ShouldDetectInteractionLimitReached()
    {
        // Arrange
        var session = new ChatSession
        {
            Id = Guid.NewGuid(),
            ChatbotConfigurationId = Guid.NewGuid(),
            SessionToken = "token-1",
            MaxInteractionsPerSession = 10
        };

        // Act
        session.InteractionCount = 10;
        session.InteractionLimitReached = session.InteractionCount >= session.MaxInteractionsPerSession;

        // Assert
        session.InteractionLimitReached.Should().BeTrue();
    }
}

public class ChatMessageTests
{
    [Fact]
    public void ChatMessage_ShouldBeCreated_WithValidData()
    {
        // Arrange & Act
        var message = new ChatMessage
        {
            Id = Guid.NewGuid(),
            SessionId = Guid.NewGuid(),
            Content = "¿Tienen carros Toyota en venta?",
            Type = MessageType.UserText,
            IsFromBot = false,
            IntentName = "vehicle.search",
            ConfidenceScore = 0.95m,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        message.Content.Should().Contain("Toyota");
        message.Type.Should().Be(MessageType.UserText);
        message.IsFromBot.Should().BeFalse();
        message.IntentName.Should().Be("vehicle.search");
        message.ConfidenceScore.Should().Be(0.95m);
    }

    [Fact]
    public void ChatMessage_BotResponse_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var message = new ChatMessage
        {
            Id = Guid.NewGuid(),
            SessionId = Guid.NewGuid(),
            Content = "Sí, tenemos varios Toyota disponibles. ¿Qué modelo le interesa?",
            Type = MessageType.BotText,
            IsFromBot = true,
            ResponseTimeMs = 150,
            ConsumedInteraction = true,
            InteractionCost = 0.002m
        };

        // Assert
        message.Type.Should().Be(MessageType.BotText);
        message.IsFromBot.Should().BeTrue();
        message.ResponseTimeMs.Should().Be(150);
        message.ConsumedInteraction.Should().BeTrue();
        message.InteractionCost.Should().Be(0.002m);
    }
}

public class ChatbotConfigurationTests
{
    [Fact]
    public void ChatbotConfiguration_ShouldBeCreated_WithDefaults()
    {
        // Arrange & Act
        var config = new ChatbotConfiguration
        {
            Id = Guid.NewGuid(),
            Name = "OKLA Chatbot",
            LlmProjectId = "okla-chatbot",
            LanguageCode = "es"
        };

        // Assert
        config.Name.Should().Be("OKLA Chatbot");
        config.IsActive.Should().BeTrue();
        config.IsEnabled.Should().BeTrue();
        config.MaxInteractionsPerSession.Should().Be(10);
        config.MaxInteractionsPerUserPerDay.Should().Be(50);
        config.MaxGlobalInteractionsPerMonth.Should().Be(100000);
        config.FreeInteractionsPerMonth.Should().Be(180);
        config.CostPerInteraction.Should().Be(0.002m);
    }

    [Fact]
    public void ChatbotConfiguration_IsEnabled_ShouldSyncWithIsActive()
    {
        // Arrange
        var config = new ChatbotConfiguration();

        // Act & Assert
        config.IsActive = true;
        config.IsEnabled.Should().BeTrue();

        config.IsActive = false;
        config.IsEnabled.Should().BeFalse();

        config.IsEnabled = true;
        config.IsActive.Should().BeTrue();
    }

    [Fact]
    public void ChatbotConfiguration_ShouldHaveAutomationSettings()
    {
        // Arrange & Act
        var config = new ChatbotConfiguration
        {
            EnableAutoInventorySync = true,
            InventorySyncIntervalMinutes = 60,
            EnableAutoReports = true,
            EnableAutoLearning = true,
            EnableHealthMonitoring = true,
            HealthCheckIntervalMinutes = 5
        };

        // Assert
        config.EnableAutoInventorySync.Should().BeTrue();
        config.InventorySyncIntervalMinutes.Should().Be(60);
        config.EnableAutoReports.Should().BeTrue();
        config.EnableAutoLearning.Should().BeTrue();
        config.EnableHealthMonitoring.Should().BeTrue();
    }
}

public class MaintenanceTaskTests
{
    [Fact]
    public void MaintenanceTask_ShouldBeCreated_WithValidData()
    {
        // Arrange & Act
        var task = new MaintenanceTask
        {
            Id = Guid.NewGuid(),
            ChatbotConfigurationId = Guid.NewGuid(),
            TaskType = MaintenanceTaskType.InventorySync,
            Name = "Sincronización de inventario",
            CronExpression = "0 * * * *",
            IsEnabled = true,
            MaxRetries = 3
        };

        // Assert
        task.TaskType.Should().Be(MaintenanceTaskType.InventorySync);
        task.IsEnabled.Should().BeTrue();
        task.MaxRetries.Should().Be(3);
        task.Status.Should().Be(MaintenanceTaskStatus.Pending);
    }

    [Fact]
    public void MaintenanceTask_ShouldTrackExecutionMetrics()
    {
        // Arrange
        var task = new MaintenanceTask
        {
            Id = Guid.NewGuid(),
            ChatbotConfigurationId = Guid.NewGuid(),
            TaskType = MaintenanceTaskType.HealthCheck,
            Name = "Health Check"
        };

        // Act
        task.TotalExecutions = 100;
        task.SuccessfulExecutions = 95;
        task.FailedExecutions = 5;
        task.AvgExecutionTimeMs = 150.5m;
        task.LastRunSuccess = true;
        task.LastRunAt = DateTime.UtcNow;

        // Assert
        task.TotalExecutions.Should().Be(100);
        task.SuccessfulExecutions.Should().Be(95);
        task.FailedExecutions.Should().Be(5);
    }
}

public class QuickResponseTests
{
    [Fact]
    public void QuickResponse_ShouldBeCreated_WithValidData()
    {
        // Arrange & Act
        var response = new QuickResponse
        {
            Id = Guid.NewGuid(),
            ChatbotConfigurationId = Guid.NewGuid(),
            Name = "Greeting",
            Category = "general",
            Response = "¡Hola! Soy el asistente virtual de OKLA.",
            Priority = 100,
            BypassLlm = true,
            IsActive = true
        };

        // Assert
        response.Name.Should().Be("Greeting");
        response.ResponseText.Should().Be("¡Hola! Soy el asistente virtual de OKLA.");
        response.BypassLlm.Should().BeTrue();
        response.Priority.Should().Be(100);
    }

    [Fact]
    public void QuickResponse_GetTriggers_ShouldParseJsonArray()
    {
        // Arrange
        var response = new QuickResponse
        {
            Id = Guid.NewGuid(),
            ChatbotConfigurationId = Guid.NewGuid(),
            Name = "Greeting",
            TriggersJson = "[\"hola\", \"buenos días\", \"hi\"]"
        };

        // Act
        var triggers = response.GetTriggers();

        // Assert
        triggers.Should().HaveCount(3);
        triggers.Should().Contain("hola");
        triggers.Should().Contain("buenos días");
        triggers.Should().Contain("hi");
    }

    [Fact]
    public void QuickResponse_TriggerKeywords_ShouldSyncWithTriggersJson()
    {
        // Arrange
        var response = new QuickResponse();

        // Act
        response.TriggerKeywords = "[\"test\", \"demo\"]";

        // Assert
        response.TriggersJson.Should().Be("[\"test\", \"demo\"]");
    }
}

public class ChatLeadTests
{
    [Fact]
    public void ChatLead_ShouldBeCreated_WithValidData()
    {
        // Arrange & Act
        var lead = new ChatLead
        {
            Id = Guid.NewGuid(),
            SessionId = Guid.NewGuid(),
            FullName = "Juan Pérez",
            Email = "juan@example.com",
            Phone = "+1809-555-1234",
            Status = LeadStatus.New,
            Temperature = LeadTemperature.Hot,
            PreferredContactMethod = "phone"
        };

        // Assert
        lead.FullName.Should().Be("Juan Pérez");
        lead.Email.Should().Be("juan@example.com");
        lead.Status.Should().Be(LeadStatus.New);
        lead.Temperature.Should().Be(LeadTemperature.Hot);
    }

    [Fact]
    public void ChatLead_TemperatureValues_ShouldExist()
    {
        // Assert
        Enum.GetValues<LeadTemperature>().Should().Contain(LeadTemperature.Cold);
        Enum.GetValues<LeadTemperature>().Should().Contain(LeadTemperature.Warm);
        Enum.GetValues<LeadTemperature>().Should().Contain(LeadTemperature.Hot);
    }
}

public class InteractionUsageTests
{
    [Fact]
    public void InteractionUsage_UsageDate_ShouldComputeFromYearMonthDay()
    {
        // Arrange
        var usage = new InteractionUsage
        {
            Year = 2026,
            Month = 1,
            Day = 15
        };

        // Act
        var usageDate = usage.UsageDate;

        // Assert
        usageDate.Year.Should().Be(2026);
        usageDate.Month.Should().Be(1);
        usageDate.Day.Should().Be(15);
    }

    [Fact]
    public void InteractionUsage_UsageDate_ShouldSetYearMonthDay()
    {
        // Arrange
        var usage = new InteractionUsage();

        // Act
        usage.UsageDate = new DateTime(2026, 3, 20);

        // Assert
        usage.Year.Should().Be(2026);
        usage.Month.Should().Be(3);
        usage.Day.Should().Be(20);
    }
}

public class ChatbotVehicleTests
{
    [Fact]
    public void ChatbotVehicle_ShouldBeCreated_WithValidData()
    {
        // Arrange & Act
        var vehicle = new ChatbotVehicle
        {
            Id = Guid.NewGuid(),
            ChatbotConfigurationId = Guid.NewGuid(),
            VehicleId = Guid.NewGuid(),
            Make = "Toyota",
            Model = "Corolla",
            Year = 2024,
            Price = 1200000m,
            ExteriorColor = "Blanco",
            InteriorColor = "Negro",
            Description = "Corolla 2024 en excelente estado",
            IsAvailable = true
        };

        // Assert
        vehicle.Make.Should().Be("Toyota");
        vehicle.Model.Should().Be("Corolla");
        vehicle.Year.Should().Be(2024);
        vehicle.ExteriorColor.Should().Be("Blanco");
        vehicle.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public void ChatbotVehicle_ViewCount_ShouldSyncWithViewsFromChatbot()
    {
        // Arrange
        var vehicle = new ChatbotVehicle();

        // Act
        vehicle.ViewCount = 100;

        // Assert
        vehicle.ViewsFromChatbot.Should().Be(100);
    }

    [Fact]
    public void ChatbotVehicle_InquiryCount_ShouldSyncWithInquiriesFromChatbot()
    {
        // Arrange
        var vehicle = new ChatbotVehicle();

        // Act
        vehicle.InquiryCount = 50;

        // Assert
        vehicle.InquiriesFromChatbot.Should().Be(50);
    }
}

public class UnansweredQuestionTests
{
    [Fact]
    public void UnansweredQuestion_OriginalQuestion_ShouldSyncWithQuestion()
    {
        // Arrange
        var uq = new UnansweredQuestion();

        // Act
        uq.OriginalQuestion = "¿Tienen financiamiento?";

        // Assert
        uq.Question.Should().Be("¿Tienen financiamiento?");
    }

    [Fact]
    public void UnansweredQuestion_AttemptedConfidenceScore_ShouldSyncWithAttemptedConfidence()
    {
        // Arrange
        var uq = new UnansweredQuestion();

        // Act
        uq.AttemptedConfidenceScore = 0.35m;

        // Assert
        uq.AttemptedConfidence.Should().Be(0.35m);
    }
}

public class EnumTests
{
    [Fact]
    public void SessionStatus_ShouldHaveExpectedValues()
    {
        // Assert
        Enum.GetValues<SessionStatus>().Should().Contain(SessionStatus.Active);
        Enum.GetValues<SessionStatus>().Should().Contain(SessionStatus.Completed);
        Enum.GetValues<SessionStatus>().Should().Contain(SessionStatus.TransferredToAgent);
    }

    [Fact]
    public void SessionType_ShouldHaveExpectedValues()
    {
        // Assert
        Enum.GetValues<SessionType>().Should().Contain(SessionType.WebChat);
        Enum.GetValues<SessionType>().Should().Contain(SessionType.WhatsApp);
        Enum.GetValues<SessionType>().Should().Contain(SessionType.FacebookMessenger);
    }

    [Fact]
    public void MessageType_ShouldHaveExpectedValues()
    {
        // Assert
        Enum.GetValues<MessageType>().Should().Contain(MessageType.UserText);
        Enum.GetValues<MessageType>().Should().Contain(MessageType.BotText);
        Enum.GetValues<MessageType>().Should().Contain(MessageType.QuickReply);
    }

    [Fact]
    public void IntentCategory_ShouldHaveExpectedValues()
    {
        // Assert
        Enum.GetValues<IntentCategory>().Should().Contain(IntentCategory.Greeting);
        Enum.GetValues<IntentCategory>().Should().Contain(IntentCategory.Fallback);
        Enum.GetValues<IntentCategory>().Should().Contain(IntentCategory.VehicleSearch);
    }

    [Fact]
    public void MaintenanceTaskType_ShouldHaveExpectedValues()
    {
        // Assert
        Enum.GetValues<MaintenanceTaskType>().Should().Contain(MaintenanceTaskType.InventorySync);
        Enum.GetValues<MaintenanceTaskType>().Should().Contain(MaintenanceTaskType.HealthCheck);
        Enum.GetValues<MaintenanceTaskType>().Should().Contain(MaintenanceTaskType.AutoLearning);
        Enum.GetValues<MaintenanceTaskType>().Should().Contain(MaintenanceTaskType.DailyReport);
    }

    [Fact]
    public void LeadStatus_ShouldHaveExpectedValues()
    {
        // Assert
        Enum.GetValues<LeadStatus>().Should().Contain(LeadStatus.New);
        Enum.GetValues<LeadStatus>().Should().Contain(LeadStatus.Contacted);
        Enum.GetValues<LeadStatus>().Should().Contain(LeadStatus.Qualified);
    }
}
