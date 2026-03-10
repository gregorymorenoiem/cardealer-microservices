using ContactService.Domain.Entities;
using ContactService.Domain.Interfaces;
using ContactService.Infrastructure.Persistence;
using ContactService.Infrastructure.Repositories;
using CarDealer.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace ContactService.Tests.Unit.Ley17213;

// ═══════════════════════════════════════════════════════════════════════════════
// LEY 172-13 DATA SUPPRESSION TESTS — CONTACTSERVICE
//
// Validates cascade anonymization (not deletion) of buyer PII when
// UserDeletedEvent is consumed. Contact requests and messages are
// ANONYMIZED (not deleted) to preserve seller business records.
//
// Test matrix:
//   • Contact request PII anonymization (buyer fields → "[SUPRIMIDO]")
//   • Contact message anonymization (buyer content → "[MENSAJE SUPRIMIDO …]")
//   • Other user data remains completely untouched
//   • Seller messages are NOT anonymized
//   • Idempotency (double call is safe)
//   • Edge cases (no data, mixed scenarios)
// ═══════════════════════════════════════════════════════════════════════════════

public class Ley17213DataSuppressionTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly ContactRequestRepository _contactRequestRepo;
    private readonly ContactMessageRepository _contactMessageRepo;

    // ── Test user IDs ──
    private static readonly Guid TargetBuyerId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid OtherBuyerId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly Guid SellerId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
    private static readonly Guid DealerId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");

    public Ley17213DataSuppressionTests()
    {
        var tenantContext = new Mock<ITenantContext>();
        tenantContext.Setup(t => t.CurrentDealerId).Returns((Guid?)null);
        tenantContext.Setup(t => t.HasDealerContext).Returns(false);

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"Ley17213ContactTests_{Guid.NewGuid()}")
            .Options;

        _context = new ApplicationDbContext(options, tenantContext.Object);
        _contactRequestRepo = new ContactRequestRepository(_context);
        _contactMessageRepo = new ContactMessageRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // SEED HELPERS
    // ═══════════════════════════════════════════════════════════════════════

    private async Task SeedTargetUserData()
    {
        // Target buyer has 2 contact requests with messages
        var request1 = new ContactRequest
        {
            Id = Guid.NewGuid(),
            DealerId = DealerId,
            VehicleId = Guid.NewGuid(),
            BuyerId = TargetBuyerId,
            SellerId = SellerId,
            Subject = "Interesado en Toyota Corolla 2024",
            BuyerName = "Juan Pérez",
            BuyerEmail = "juan.perez@test.com",
            BuyerPhone = "809-555-0001",
            Name = "Juan Pérez",
            Email = "juan.perez@test.com",
            Phone = "809-555-0001",
            Message = "¿Está disponible este vehículo?",
            Status = "Open",
            CreatedAt = DateTime.UtcNow.AddDays(-5),
        };

        var request2 = new ContactRequest
        {
            Id = Guid.NewGuid(),
            DealerId = DealerId,
            VehicleId = Guid.NewGuid(),
            BuyerId = TargetBuyerId,
            SellerId = SellerId,
            Subject = "Consulta Honda Civic 2023",
            BuyerName = "Juan Pérez",
            BuyerEmail = "juan.perez@test.com",
            BuyerPhone = "809-555-0001",
            Name = "Juan Pérez",
            Email = "juan.perez@test.com",
            Phone = "809-555-0001",
            Message = "Quisiera más información sobre este Honda",
            Status = "Replied",
            CreatedAt = DateTime.UtcNow.AddDays(-3),
        };

        _context.ContactRequests.AddRange(request1, request2);
        await _context.SaveChangesAsync();

        // Messages on request1: 2 from buyer, 1 from seller
        var msg1 = new ContactMessage(request1.Id, TargetBuyerId, "¿Cuál es el precio final?", true)
        {
            DealerId = DealerId,
        };
        var msg2 = new ContactMessage(request1.Id, SellerId, "El precio es RD$1,200,000", false)
        {
            DealerId = DealerId,
        };
        var msg3 = new ContactMessage(request1.Id, TargetBuyerId, "¿Acepta financiamiento?", true)
        {
            DealerId = DealerId,
        };

        // Message on request2: 1 from buyer
        var msg4 = new ContactMessage(request2.Id, TargetBuyerId, "Mi teléfono es 809-555-0001, llámeme", true)
        {
            DealerId = DealerId,
        };

        _context.ContactMessages.AddRange(msg1, msg2, msg3, msg4);
        await _context.SaveChangesAsync();
    }

    private async Task SeedOtherUserData()
    {
        var otherRequest = new ContactRequest
        {
            Id = Guid.NewGuid(),
            DealerId = DealerId,
            VehicleId = Guid.NewGuid(),
            BuyerId = OtherBuyerId,
            SellerId = SellerId,
            Subject = "Consulta Hyundai Tucson 2024",
            BuyerName = "María López",
            BuyerEmail = "maria.lopez@test.com",
            BuyerPhone = "829-555-0002",
            Name = "María López",
            Email = "maria.lopez@test.com",
            Phone = "829-555-0002",
            Message = "¿Tiene financiamiento disponible?",
            Status = "Open",
            CreatedAt = DateTime.UtcNow.AddDays(-1),
        };

        _context.ContactRequests.Add(otherRequest);
        await _context.SaveChangesAsync();

        var otherMsg = new ContactMessage(otherRequest.Id, OtherBuyerId, "Estoy interesada", true)
        {
            DealerId = DealerId,
        };

        _context.ContactMessages.Add(otherMsg);
        await _context.SaveChangesAsync();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // CONTACT REQUEST ANONYMIZATION TESTS
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task AnonymizeByBuyerIdAsync_ReplacesAllPiiFields_WithSuprimido()
    {
        await SeedTargetUserData();

        var count = await _contactRequestRepo.AnonymizeByBuyerIdAsync(TargetBuyerId);

        count.Should().Be(2);

        var requests = await _context.ContactRequests
            .Where(cr => cr.BuyerId == TargetBuyerId)
            .ToListAsync();

        foreach (var req in requests)
        {
            req.BuyerName.Should().Be("[SUPRIMIDO]");
            req.BuyerEmail.Should().Be("[SUPRIMIDO]");
            req.BuyerPhone.Should().BeNull();
            req.Name.Should().Be("[SUPRIMIDO]");
            req.Email.Should().Be("[SUPRIMIDO]");
            req.Phone.Should().Be("[SUPRIMIDO]");
        }
    }

    [Fact]
    public async Task AnonymizeByBuyerIdAsync_PreservesNonPiiFields()
    {
        await SeedTargetUserData();

        await _contactRequestRepo.AnonymizeByBuyerIdAsync(TargetBuyerId);

        var requests = await _context.ContactRequests
            .Where(cr => cr.BuyerId == TargetBuyerId)
            .ToListAsync();

        foreach (var req in requests)
        {
            // Structural fields must remain intact
            req.BuyerId.Should().Be(TargetBuyerId);
            req.SellerId.Should().Be(SellerId);
            req.DealerId.Should().Be(DealerId);
            req.Subject.Should().NotBeNullOrEmpty();
            req.Status.Should().NotBeNullOrEmpty();
            req.CreatedAt.Should().NotBe(default);
        }
    }

    [Fact]
    public async Task AnonymizeByBuyerIdAsync_DoesNotAffectOtherUsers()
    {
        await SeedTargetUserData();
        await SeedOtherUserData();

        await _contactRequestRepo.AnonymizeByBuyerIdAsync(TargetBuyerId);

        var otherRequests = await _context.ContactRequests
            .Where(cr => cr.BuyerId == OtherBuyerId)
            .ToListAsync();

        otherRequests.Should().HaveCount(1);
        otherRequests[0].BuyerName.Should().Be("María López");
        otherRequests[0].BuyerEmail.Should().Be("maria.lopez@test.com");
        otherRequests[0].BuyerPhone.Should().Be("829-555-0002");
        otherRequests[0].Name.Should().Be("María López");
        otherRequests[0].Email.Should().Be("maria.lopez@test.com");
        otherRequests[0].Phone.Should().Be("829-555-0002");
    }

    [Fact]
    public async Task AnonymizeByBuyerIdAsync_NoData_ReturnsZero()
    {
        var count = await _contactRequestRepo.AnonymizeByBuyerIdAsync(Guid.NewGuid());
        count.Should().Be(0);
    }

    [Fact]
    public async Task AnonymizeByBuyerIdAsync_Idempotent_SameResultOnSecondCall()
    {
        await SeedTargetUserData();

        var firstCount = await _contactRequestRepo.AnonymizeByBuyerIdAsync(TargetBuyerId);
        var secondCount = await _contactRequestRepo.AnonymizeByBuyerIdAsync(TargetBuyerId);

        firstCount.Should().Be(2);
        secondCount.Should().Be(2); // Same records, already anonymized

        var requests = await _context.ContactRequests
            .Where(cr => cr.BuyerId == TargetBuyerId)
            .ToListAsync();

        foreach (var req in requests)
        {
            req.BuyerName.Should().Be("[SUPRIMIDO]");
            req.BuyerEmail.Should().Be("[SUPRIMIDO]");
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // CONTACT MESSAGE ANONYMIZATION TESTS
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task AnonymizeByUserIdAsync_ReplacesBuyerMessageContent()
    {
        await SeedTargetUserData();

        var count = await _contactMessageRepo.AnonymizeByUserIdAsync(TargetBuyerId);

        // Target buyer has 3 messages (msg1, msg3, msg4 — all IsFromBuyer=true)
        count.Should().Be(3);

        var buyerMessages = await _context.ContactMessages
            .Include(cm => cm.ContactRequest)
            .Where(cm => cm.ContactRequest!.BuyerId == TargetBuyerId && cm.IsFromBuyer)
            .ToListAsync();

        foreach (var msg in buyerMessages)
        {
            msg.Message.Should().Be("[MENSAJE SUPRIMIDO — Ley 172-13]");
        }
    }

    [Fact]
    public async Task AnonymizeByUserIdAsync_DoesNotAffectSellerMessages()
    {
        await SeedTargetUserData();

        await _contactMessageRepo.AnonymizeByUserIdAsync(TargetBuyerId);

        // Seller's response (msg2) should remain intact
        var sellerMessages = await _context.ContactMessages
            .Include(cm => cm.ContactRequest)
            .Where(cm => cm.ContactRequest!.BuyerId == TargetBuyerId && !cm.IsFromBuyer)
            .ToListAsync();

        sellerMessages.Should().HaveCount(1);
        sellerMessages[0].Message.Should().Be("El precio es RD$1,200,000");
    }

    [Fact]
    public async Task AnonymizeByUserIdAsync_DoesNotAffectOtherBuyerMessages()
    {
        await SeedTargetUserData();
        await SeedOtherUserData();

        await _contactMessageRepo.AnonymizeByUserIdAsync(TargetBuyerId);

        var otherMessages = await _context.ContactMessages
            .Include(cm => cm.ContactRequest)
            .Where(cm => cm.ContactRequest!.BuyerId == OtherBuyerId)
            .ToListAsync();

        otherMessages.Should().HaveCount(1);
        otherMessages[0].Message.Should().Be("Estoy interesada");
    }

    [Fact]
    public async Task AnonymizeByUserIdAsync_NoData_ReturnsZero()
    {
        var count = await _contactMessageRepo.AnonymizeByUserIdAsync(Guid.NewGuid());
        count.Should().Be(0);
    }

    [Fact]
    public async Task AnonymizeByUserIdAsync_Idempotent_SafeOnDoubleCall()
    {
        await SeedTargetUserData();

        var firstCount = await _contactMessageRepo.AnonymizeByUserIdAsync(TargetBuyerId);
        var secondCount = await _contactMessageRepo.AnonymizeByUserIdAsync(TargetBuyerId);

        firstCount.Should().Be(3);
        secondCount.Should().Be(3);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // END-TO-END CASCADE ANONYMIZATION TESTS
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task FullCascade_AnonymizesBothRequestsAndMessages()
    {
        await SeedTargetUserData();
        await SeedOtherUserData();

        // Simulate the consumer's flow: messages first, then requests
        var messagesAnonymized = await _contactMessageRepo.AnonymizeByUserIdAsync(TargetBuyerId);
        var requestsAnonymized = await _contactRequestRepo.AnonymizeByBuyerIdAsync(TargetBuyerId);

        messagesAnonymized.Should().Be(3);
        requestsAnonymized.Should().Be(2);

        // Verify target user is fully anonymized
        var targetRequests = await _context.ContactRequests
            .Where(cr => cr.BuyerId == TargetBuyerId)
            .ToListAsync();

        targetRequests.Should().AllSatisfy(req =>
        {
            req.BuyerName.Should().Be("[SUPRIMIDO]");
            req.BuyerEmail.Should().Be("[SUPRIMIDO]");
        });

        var targetBuyerMessages = await _context.ContactMessages
            .Include(cm => cm.ContactRequest)
            .Where(cm => cm.ContactRequest!.BuyerId == TargetBuyerId && cm.IsFromBuyer)
            .ToListAsync();

        targetBuyerMessages.Should().AllSatisfy(msg =>
        {
            msg.Message.Should().Be("[MENSAJE SUPRIMIDO — Ley 172-13]");
        });

        // Verify other user is completely untouched
        var otherRequests = await _context.ContactRequests
            .Where(cr => cr.BuyerId == OtherBuyerId)
            .ToListAsync();

        otherRequests.Should().HaveCount(1);
        otherRequests[0].BuyerName.Should().Be("María López");
        otherRequests[0].BuyerEmail.Should().Be("maria.lopez@test.com");

        var otherMessages = await _context.ContactMessages
            .Include(cm => cm.ContactRequest)
            .Where(cm => cm.ContactRequest!.BuyerId == OtherBuyerId)
            .ToListAsync();

        otherMessages.Should().HaveCount(1);
        otherMessages[0].Message.Should().Be("Estoy interesada");
    }

    [Fact]
    public async Task FullCascade_PreservesRecordCounts()
    {
        await SeedTargetUserData();
        await SeedOtherUserData();

        await _contactMessageRepo.AnonymizeByUserIdAsync(TargetBuyerId);
        await _contactRequestRepo.AnonymizeByBuyerIdAsync(TargetBuyerId);

        // Records are anonymized, NOT deleted — total counts must be preserved
        var totalRequests = await _context.ContactRequests.CountAsync();
        var totalMessages = await _context.ContactMessages.CountAsync();

        totalRequests.Should().Be(3); // 2 target + 1 other
        totalMessages.Should().Be(5); // 4 target (3 buyer + 1 seller) + 1 other
    }

    [Fact]
    public async Task FullCascade_SellerResponsesRemainReadable()
    {
        await SeedTargetUserData();

        await _contactMessageRepo.AnonymizeByUserIdAsync(TargetBuyerId);
        await _contactRequestRepo.AnonymizeByBuyerIdAsync(TargetBuyerId);

        // Seller's response must remain readable for their business records
        var sellerMessage = await _context.ContactMessages
            .Include(cm => cm.ContactRequest)
            .Where(cm => cm.ContactRequest!.BuyerId == TargetBuyerId && !cm.IsFromBuyer)
            .SingleAsync();

        sellerMessage.Message.Should().Be("El precio es RD$1,200,000");
        sellerMessage.SenderId.Should().Be(SellerId);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // PII PURGE VERIFICATION TESTS
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task PiiPurge_NoTraceOfOriginalBuyerName()
    {
        await SeedTargetUserData();

        await _contactRequestRepo.AnonymizeByBuyerIdAsync(TargetBuyerId);
        await _contactMessageRepo.AnonymizeByUserIdAsync(TargetBuyerId);

        // Scan ALL text fields for any trace of PII
        var allRequests = await _context.ContactRequests.ToListAsync();
        var allMessages = await _context.ContactMessages.ToListAsync();

        var piiPatterns = new[] { "Juan Pérez", "juan.perez@test.com", "809-555-0001" };

        foreach (var req in allRequests)
        {
            foreach (var pii in piiPatterns)
            {
                req.BuyerName.Should().NotContain(pii);
                req.BuyerEmail.Should().NotContain(pii);
                req.Name.Should().NotContain(pii);
                req.Email.Should().NotContain(pii);
            }
        }

        foreach (var msg in allMessages)
        {
            foreach (var pii in piiPatterns)
            {
                msg.Message.Should().NotContain(pii, because: "Ley 172-13 requires complete PII removal from buyer messages");
            }
        }
    }

    [Fact]
    public async Task PiiPurge_BuyerPhoneFieldIsNullAfterAnonymization()
    {
        await SeedTargetUserData();

        await _contactRequestRepo.AnonymizeByBuyerIdAsync(TargetBuyerId);

        var requests = await _context.ContactRequests
            .Where(cr => cr.BuyerId == TargetBuyerId)
            .ToListAsync();

        foreach (var req in requests)
        {
            req.BuyerPhone.Should().BeNull(because: "BuyerPhone is nullable and should be set to null on suppression");
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // INTERFACE CONTRACT TESTS
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void IContactRequestRepository_HasAnonymizeByBuyerIdAsyncMethod()
    {
        var method = typeof(IContactRequestRepository).GetMethod("AnonymizeByBuyerIdAsync");
        method.Should().NotBeNull("Ley 172-13 requires cascade anonymization capability");
        method!.ReturnType.Should().Be(typeof(Task<int>));
    }

    [Fact]
    public void IContactMessageRepository_HasAnonymizeByUserIdAsyncMethod()
    {
        var method = typeof(IContactMessageRepository).GetMethod("AnonymizeByUserIdAsync");
        method.Should().NotBeNull("Ley 172-13 requires cascade anonymization capability");
        method!.ReturnType.Should().Be(typeof(Task<int>));
    }

    [Fact]
    public void ContactRequestRepository_ImplementsIContactRequestRepository()
    {
        typeof(ContactRequestRepository).Should().Implement<IContactRequestRepository>();
    }

    [Fact]
    public void ContactMessageRepository_ImplementsIContactMessageRepository()
    {
        typeof(ContactMessageRepository).Should().Implement<IContactMessageRepository>();
    }
}
