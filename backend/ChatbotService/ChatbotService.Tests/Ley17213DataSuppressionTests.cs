using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChatbotService.Domain.Entities;
using ChatbotService.Domain.Enums;
using ChatbotService.Infrastructure.Persistence;
using ChatbotService.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ChatbotService.Tests;

// ═══════════════════════════════════════════════════════════════════════════════
// LEY 172-13 — DERECHO DE SUPRESIÓN (CASCADE DELETION) TESTS
//
// Validates that when a user requests data suppression under Dominican Republic
// Ley 172-13 (Art. 5), ALL user data is completely purged from ChatbotService:
//   • Chat sessions
//   • Chat messages
//   • Chat leads
//   • Interaction usage records
//
// These tests use EF Core InMemory provider and real repository implementations.
// ═══════════════════════════════════════════════════════════════════════════════

public class Ley17213DataSuppressionTests : IDisposable
{
    private readonly ChatbotDbContext _context;
    private readonly ChatSessionRepository _sessionRepo;
    private readonly ChatMessageRepository _messageRepo;
    private readonly ChatLeadRepository _leadRepo;
    private readonly InteractionUsageRepository _usageRepo;

    private static readonly Guid TargetUserId = Guid.NewGuid();
    private static readonly Guid OtherUserId = Guid.NewGuid();
    private static readonly Guid ConfigId = Guid.NewGuid();
    private static readonly Guid DealerId = Guid.NewGuid();

    public Ley17213DataSuppressionTests()
    {
        var options = new DbContextOptionsBuilder<ChatbotDbContext>()
            .UseInMemoryDatabase(databaseName: $"Ley17213Tests_{Guid.NewGuid()}")
            .Options;

        _context = new ChatbotDbContext(options);
        _sessionRepo = new ChatSessionRepository(_context);
        _messageRepo = new ChatMessageRepository(_context);
        _leadRepo = new ChatLeadRepository(_context);
        _usageRepo = new InteractionUsageRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    #region Helper Methods

    private ChatbotConfiguration CreateConfig()
    {
        return new ChatbotConfiguration
        {
            Id = ConfigId,
            DealerId = DealerId,
            BotName = "Test Bot",
            WelcomeMessage = "Hola",
            IsEnabled = true,
            Plan = ChatbotPlan.Enterprise,
            CreatedAt = DateTime.UtcNow
        };
    }

    private ChatSession CreateSession(Guid userId, string token, SessionStatus status = SessionStatus.Active)
    {
        return new ChatSession
        {
            Id = Guid.NewGuid(),
            SessionToken = token,
            UserId = userId,
            ChatbotConfigurationId = ConfigId,
            DealerId = DealerId,
            Status = status,
            Channel = "web",
            UserName = userId == TargetUserId ? "Juan Pérez" : "Otro Usuario",
            UserEmail = userId == TargetUserId ? "juan@ejemplo.do" : "otro@ejemplo.do",
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            LastActivityAt = DateTime.UtcNow.AddDays(-1),
            ConsentAccepted = true,
            ConsentAcceptedAt = DateTime.UtcNow.AddDays(-10)
        };
    }

    private ChatMessage CreateMessage(Guid sessionId, string content, bool isFromBot = false)
    {
        return new ChatMessage
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            Content = content,
            IsFromBot = isFromBot,
            CreatedAt = DateTime.UtcNow.AddHours(-1),
            IntentCategory = IntentCategory.Greeting,
            ConsumedInteraction = isFromBot
        };
    }

    private ChatLead CreateLead(Guid sessionId)
    {
        return new ChatLead
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            FullName = "Juan Pérez",
            Email = "juan@ejemplo.do",
            Phone = "809-555-0001",
            Status = LeadStatus.New,
            CreatedAt = DateTime.UtcNow.AddDays(-5)
        };
    }

    private async Task SeedTestData()
    {
        var config = CreateConfig();
        _context.ChatbotConfigurations.Add(config);

        // Target user: 3 sessions
        var session1 = CreateSession(TargetUserId, "target-session-1");
        var session2 = CreateSession(TargetUserId, "target-session-2");
        var session3 = CreateSession(TargetUserId, "target-session-3", SessionStatus.Completed);
        _context.ChatSessions.AddRange(session1, session2, session3);

        // Messages for target user sessions
        _context.ChatMessages.AddRange(
            CreateMessage(session1.Id, "Hola, busco un Toyota"),
            CreateMessage(session1.Id, "Tenemos varios Toyota disponibles", isFromBot: true),
            CreateMessage(session2.Id, "¿Tienen financiamiento?"),
            CreateMessage(session2.Id, "Sí, ofrecemos planes de financiamiento", isFromBot: true),
            CreateMessage(session3.Id, "Gracias por la info")
        );

        // Leads for target user sessions
        _context.ChatLeads.AddRange(
            CreateLead(session1.Id),
            CreateLead(session2.Id)
        );

        // Interaction usage for target user
        _context.InteractionUsages.AddRange(
            new InteractionUsage
            {
                Id = Guid.NewGuid(),
                ChatbotConfigurationId = ConfigId,
                UserId = TargetUserId,
                UsageDate = DateTime.UtcNow.Date,
                InteractionCount = 5,
                TotalCost = 0.40m
            },
            new InteractionUsage
            {
                Id = Guid.NewGuid(),
                ChatbotConfigurationId = ConfigId,
                UserId = TargetUserId,
                UsageDate = DateTime.UtcNow.Date.AddDays(-1),
                InteractionCount = 3,
                TotalCost = 0.24m
            }
        );

        // Other user: 2 sessions (should NOT be affected)
        var otherSession1 = CreateSession(OtherUserId, "other-session-1");
        var otherSession2 = CreateSession(OtherUserId, "other-session-2");
        _context.ChatSessions.AddRange(otherSession1, otherSession2);

        _context.ChatMessages.AddRange(
            CreateMessage(otherSession1.Id, "Busco un Honda"),
            CreateMessage(otherSession1.Id, "Tenemos Honda Civic 2024", isFromBot: true),
            CreateMessage(otherSession2.Id, "¿Cuánto cuesta?")
        );

        _context.ChatLeads.Add(new ChatLead
        {
            Id = Guid.NewGuid(),
            SessionId = otherSession1.Id,
            FullName = "María López",
            Email = "maria@ejemplo.do",
            Phone = "809-555-9999",
            Status = LeadStatus.New,
            CreatedAt = DateTime.UtcNow.AddDays(-3)
        });

        _context.InteractionUsages.Add(new InteractionUsage
        {
            Id = Guid.NewGuid(),
            ChatbotConfigurationId = ConfigId,
            UserId = OtherUserId,
            UsageDate = DateTime.UtcNow.Date,
            InteractionCount = 2,
            TotalCost = 0.16m
        });

        await _context.SaveChangesAsync();
    }

    #endregion

    // ═══════════════════════════════════════════════════════════════
    // SESSION DELETION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public async Task DeleteAllByUserIdAsync_DeletesAllUserSessions()
    {
        await SeedTestData();

        var deletedIds = await _sessionRepo.DeleteAllByUserIdAsync(TargetUserId);

        deletedIds.Should().HaveCount(3);
        var remainingSessions = await _context.ChatSessions.ToListAsync();
        remainingSessions.Should().HaveCount(2);
        remainingSessions.Should().OnlyContain(s => s.UserId == OtherUserId);
    }

    [Fact]
    public async Task DeleteAllByUserIdAsync_ReturnsSessionIdsForChildCleanup()
    {
        await SeedTestData();

        var deletedIds = await _sessionRepo.DeleteAllByUserIdAsync(TargetUserId);

        deletedIds.Should().HaveCount(3);
        deletedIds.Should().OnlyContain(id => id != Guid.Empty);
    }

    [Fact]
    public async Task DeleteAllByUserIdAsync_ReturnsEmptyForNonexistentUser()
    {
        await SeedTestData();

        var deletedIds = await _sessionRepo.DeleteAllByUserIdAsync(Guid.NewGuid());

        deletedIds.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteAllByUserIdAsync_DoesNotAffectOtherUsers()
    {
        await SeedTestData();

        await _sessionRepo.DeleteAllByUserIdAsync(TargetUserId);

        var otherSessions = await _sessionRepo.GetByUserIdAsync(OtherUserId);
        otherSessions.Should().HaveCount(2);
    }

    // ═══════════════════════════════════════════════════════════════
    // MESSAGE DELETION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public async Task DeleteBySessionIdsAsync_Messages_DeletesAllMessagesForSessions()
    {
        await SeedTestData();
        var sessions = await _sessionRepo.GetByUserIdAsync(TargetUserId);
        var sessionIds = sessions.Select(s => s.Id).ToList();

        var deleted = await _messageRepo.DeleteBySessionIdsAsync(sessionIds);

        deleted.Should().Be(5);
        var remainingMessages = await _context.ChatMessages.ToListAsync();
        remainingMessages.Should().HaveCount(3); // other user's messages
    }

    [Fact]
    public async Task DeleteBySessionIdsAsync_Messages_EmptyListDeletesNothing()
    {
        await SeedTestData();

        var deleted = await _messageRepo.DeleteBySessionIdsAsync(new List<Guid>());

        deleted.Should().Be(0);
        var allMessages = await _context.ChatMessages.CountAsync();
        allMessages.Should().Be(8); // all messages intact
    }

    [Fact]
    public async Task DeleteBySessionIdsAsync_Messages_DoesNotAffectOtherSessions()
    {
        await SeedTestData();
        var sessions = await _sessionRepo.GetByUserIdAsync(TargetUserId);
        var sessionIds = sessions.Select(s => s.Id).ToList();

        await _messageRepo.DeleteBySessionIdsAsync(sessionIds);

        var otherSessions = await _sessionRepo.GetByUserIdAsync(OtherUserId);
        var otherSessionIds = otherSessions.Select(s => s.Id).ToList();
        foreach (var sid in otherSessionIds)
        {
            var msgs = await _messageRepo.GetBySessionIdAsync(sid);
            msgs.Should().NotBeEmpty("other user's messages should remain intact");
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // LEAD DELETION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public async Task DeleteBySessionIdsAsync_Leads_DeletesAllLeadsForSessions()
    {
        await SeedTestData();
        var sessions = await _sessionRepo.GetByUserIdAsync(TargetUserId);
        var sessionIds = sessions.Select(s => s.Id).ToList();

        var deleted = await _leadRepo.DeleteBySessionIdsAsync(sessionIds);

        deleted.Should().Be(2);
        var remainingLeads = await _context.ChatLeads.ToListAsync();
        remainingLeads.Should().HaveCount(1); // other user's lead
    }

    [Fact]
    public async Task DeleteBySessionIdsAsync_Leads_EmptyListDeletesNothing()
    {
        await SeedTestData();

        var deleted = await _leadRepo.DeleteBySessionIdsAsync(new List<Guid>());

        deleted.Should().Be(0);
    }

    [Fact]
    public async Task DeleteBySessionIdsAsync_Leads_PreservesOtherUserLeads()
    {
        await SeedTestData();
        var sessions = await _sessionRepo.GetByUserIdAsync(TargetUserId);
        var sessionIds = sessions.Select(s => s.Id).ToList();

        await _leadRepo.DeleteBySessionIdsAsync(sessionIds);

        var allLeads = await _context.ChatLeads.ToListAsync();
        allLeads.Should().HaveCount(1);
        allLeads.First().FullName.Should().NotBe("Juan Pérez");
    }

    // ═══════════════════════════════════════════════════════════════
    // INTERACTION USAGE DELETION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public async Task DeleteByUserIdAsync_Usage_DeletesAllUsageForUser()
    {
        await SeedTestData();

        var deleted = await _usageRepo.DeleteByUserIdAsync(TargetUserId);

        deleted.Should().Be(2);
        var remaining = await _context.InteractionUsages.ToListAsync();
        remaining.Should().HaveCount(1);
        remaining.First().UserId.Should().Be(OtherUserId);
    }

    [Fact]
    public async Task DeleteByUserIdAsync_Usage_ReturnsZeroForNonexistentUser()
    {
        await SeedTestData();

        var deleted = await _usageRepo.DeleteByUserIdAsync(Guid.NewGuid());

        deleted.Should().Be(0);
    }

    [Fact]
    public async Task DeleteByUserIdAsync_Usage_DoesNotAffectOtherUsers()
    {
        await SeedTestData();

        await _usageRepo.DeleteByUserIdAsync(TargetUserId);

        var otherUsage = await _context.InteractionUsages
            .Where(u => u.UserId == OtherUserId)
            .ToListAsync();
        otherUsage.Should().HaveCount(1);
        otherUsage.First().InteractionCount.Should().Be(2);
    }

    // ═══════════════════════════════════════════════════════════════
    // END-TO-END CASCADE DELETION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public async Task CascadeDeletion_DeletesAllUserDataInCorrectOrder()
    {
        // Arrange — simulate what UserDataDeletionConsumer does
        await SeedTestData();

        // Act — cascade deletion sequence (same as consumer)
        var sessions = await _sessionRepo.GetByUserIdAsync(TargetUserId);
        var sessionIds = sessions.Select(s => s.Id).ToList();

        var messagesDeleted = await _messageRepo.DeleteBySessionIdsAsync(sessionIds);
        var leadsDeleted = await _leadRepo.DeleteBySessionIdsAsync(sessionIds);
        var deletedSessionIds = await _sessionRepo.DeleteAllByUserIdAsync(TargetUserId);
        var usageDeleted = await _usageRepo.DeleteByUserIdAsync(TargetUserId);

        // Assert — all target user data gone
        messagesDeleted.Should().Be(5);
        leadsDeleted.Should().Be(2);
        deletedSessionIds.Should().HaveCount(3);
        usageDeleted.Should().Be(2);

        // Assert — other user data intact
        var otherSessions = await _sessionRepo.GetByUserIdAsync(OtherUserId);
        otherSessions.Should().HaveCount(2);

        var otherMessages = await _context.ChatMessages.CountAsync();
        otherMessages.Should().Be(3);

        var otherLeads = await _context.ChatLeads.CountAsync();
        otherLeads.Should().Be(1);

        var otherUsage = await _context.InteractionUsages.CountAsync();
        otherUsage.Should().Be(1);
    }

    [Fact]
    public async Task CascadeDeletion_UserWithNoData_CompletesWithoutError()
    {
        await SeedTestData();
        var phantomUserId = Guid.NewGuid();

        // Act
        var sessions = await _sessionRepo.GetByUserIdAsync(phantomUserId);
        var sessionIds = sessions.Select(s => s.Id).ToList();

        var messagesDeleted = await _messageRepo.DeleteBySessionIdsAsync(sessionIds);
        var leadsDeleted = await _leadRepo.DeleteBySessionIdsAsync(sessionIds);
        var deletedSessionIds = await _sessionRepo.DeleteAllByUserIdAsync(phantomUserId);
        var usageDeleted = await _usageRepo.DeleteByUserIdAsync(phantomUserId);

        // Assert — no-op, no errors
        messagesDeleted.Should().Be(0);
        leadsDeleted.Should().Be(0);
        deletedSessionIds.Should().BeEmpty();
        usageDeleted.Should().Be(0);

        // Assert — all data still intact
        var totalSessions = await _context.ChatSessions.CountAsync();
        totalSessions.Should().Be(5); // 3 target + 2 other
    }

    [Fact]
    public async Task CascadeDeletion_DeletesEndedSessions_NotJustActive()
    {
        await SeedTestData();

        var deletedIds = await _sessionRepo.DeleteAllByUserIdAsync(TargetUserId);

        // Should include the ended session too
        deletedIds.Should().HaveCount(3, "all sessions including ended ones must be deleted per Ley 172-13");
    }

    [Fact]
    public async Task CascadeDeletion_IdempotentOnDoubleExecution()
    {
        await SeedTestData();

        // First deletion
        var sessions1 = await _sessionRepo.GetByUserIdAsync(TargetUserId);
        var ids1 = sessions1.Select(s => s.Id).ToList();
        await _messageRepo.DeleteBySessionIdsAsync(ids1);
        await _leadRepo.DeleteBySessionIdsAsync(ids1);
        await _sessionRepo.DeleteAllByUserIdAsync(TargetUserId);
        await _usageRepo.DeleteByUserIdAsync(TargetUserId);

        // Second deletion — should be idempotent
        var sessions2 = await _sessionRepo.GetByUserIdAsync(TargetUserId);
        sessions2.Should().BeEmpty();

        var ids2 = sessions2.Select(s => s.Id).ToList();
        var msg2 = await _messageRepo.DeleteBySessionIdsAsync(ids2);
        var leads2 = await _leadRepo.DeleteBySessionIdsAsync(ids2);
        var sess2 = await _sessionRepo.DeleteAllByUserIdAsync(TargetUserId);
        var usage2 = await _usageRepo.DeleteByUserIdAsync(TargetUserId);

        msg2.Should().Be(0);
        leads2.Should().Be(0);
        sess2.Should().BeEmpty();
        usage2.Should().Be(0);
    }

    // ═══════════════════════════════════════════════════════════════
    // PII PURGE VERIFICATION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public async Task CascadeDeletion_NoPiiRemainsInDatabase()
    {
        await SeedTestData();

        // Act — full cascade
        var sessions = await _sessionRepo.GetByUserIdAsync(TargetUserId);
        var sessionIds = sessions.Select(s => s.Id).ToList();
        await _messageRepo.DeleteBySessionIdsAsync(sessionIds);
        await _leadRepo.DeleteBySessionIdsAsync(sessionIds);
        await _sessionRepo.DeleteAllByUserIdAsync(TargetUserId);
        await _usageRepo.DeleteByUserIdAsync(TargetUserId);

        // Assert — no trace of target user's PII
        var anySessionWithEmail = await _context.ChatSessions
            .AnyAsync(s => s.UserEmail == "juan@ejemplo.do");
        anySessionWithEmail.Should().BeFalse("user email must not remain after deletion");

        var anySessionWithName = await _context.ChatSessions
            .AnyAsync(s => s.UserName == "Juan Pérez");
        anySessionWithName.Should().BeFalse("user name must not remain after deletion");

        var anyLeadWithPhone = await _context.ChatLeads
            .AnyAsync(l => l.Phone == "809-555-0001");
        anyLeadWithPhone.Should().BeFalse("user phone must not remain in leads after deletion");

        var anyMessageFromUser = await _context.ChatMessages
            .AnyAsync(m => sessionIds.Contains(m.SessionId));
        anyMessageFromUser.Should().BeFalse("no messages should reference deleted sessions");
    }

    [Fact]
    public async Task CascadeDeletion_NoOrphanedRecords()
    {
        await SeedTestData();

        // Act — full cascade
        var sessions = await _sessionRepo.GetByUserIdAsync(TargetUserId);
        var sessionIds = sessions.Select(s => s.Id).ToList();
        await _messageRepo.DeleteBySessionIdsAsync(sessionIds);
        await _leadRepo.DeleteBySessionIdsAsync(sessionIds);
        await _sessionRepo.DeleteAllByUserIdAsync(TargetUserId);
        await _usageRepo.DeleteByUserIdAsync(TargetUserId);

        // Assert — no orphaned messages (messages without a valid session)
        var allMessages = await _context.ChatMessages.Include(m => m.Session).ToListAsync();
        allMessages.Should().OnlyContain(m => m.Session != null, "no orphaned messages should exist");

        // Assert — no orphaned leads
        var allLeads = await _context.ChatLeads.Include(l => l.Session).ToListAsync();
        allLeads.Should().OnlyContain(l => l.Session != null, "no orphaned leads should exist");
    }

    // ═══════════════════════════════════════════════════════════════
    // INTERFACE CONTRACT TESTS
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public async Task IChatSessionRepository_DeleteAllByUserIdAsync_IsInInterface()
    {
        // Verify the interface contract exists
        Domain.Interfaces.IChatSessionRepository repo = _sessionRepo;
        var result = await repo.DeleteAllByUserIdAsync(Guid.NewGuid());
        result.Should().NotBeNull();
        result.Should().BeAssignableTo<IReadOnlyList<Guid>>();
    }

    [Fact]
    public async Task IChatMessageRepository_DeleteBySessionIdsAsync_IsInInterface()
    {
        Domain.Interfaces.IChatMessageRepository repo = _messageRepo;
        var result = await repo.DeleteBySessionIdsAsync(new List<Guid>());
        result.Should().Be(0);
    }

    [Fact]
    public async Task IChatLeadRepository_DeleteBySessionIdsAsync_IsInInterface()
    {
        Domain.Interfaces.IChatLeadRepository repo = _leadRepo;
        var result = await repo.DeleteBySessionIdsAsync(new List<Guid>());
        result.Should().Be(0);
    }

    [Fact]
    public async Task IInteractionUsageRepository_DeleteByUserIdAsync_IsInInterface()
    {
        Domain.Interfaces.IInteractionUsageRepository repo = _usageRepo;
        var result = await repo.DeleteByUserIdAsync(Guid.NewGuid());
        result.Should().Be(0);
    }
}
