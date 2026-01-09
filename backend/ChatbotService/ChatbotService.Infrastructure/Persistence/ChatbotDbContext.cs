using Microsoft.EntityFrameworkCore;
using ChatbotService.Domain.Entities;

namespace ChatbotService.Infrastructure.Persistence;

public class ChatbotDbContext : DbContext
{
    public ChatbotDbContext(DbContextOptions<ChatbotDbContext> options) : base(options) { }

    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<IntentAnalysis> IntentAnalyses => Set<IntentAnalysis>();
    public DbSet<WhatsAppHandoff> WhatsAppHandoffs => Set<WhatsAppHandoff>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.ToTable("conversations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.UserName).HasMaxLength(200);
            entity.Property(e => e.UserEmail).HasMaxLength(200);
            entity.Property(e => e.UserPhone).HasMaxLength(50);
            
            entity.Property(e => e.VehicleTitle).HasMaxLength(300);
            entity.Property(e => e.VehiclePrice).HasColumnType("decimal(18,2)");
            
            entity.Property(e => e.DealerName).HasMaxLength(200);
            entity.Property(e => e.DealerWhatsApp).HasMaxLength(50);
            
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.LeadScore).HasDefaultValue(0);
            entity.Property(e => e.LeadTemperature).HasDefaultValue(LeadTemperature.Unknown);
            
            entity.Property(e => e.BuyingSignals).HasColumnType("jsonb");
            entity.Property(e => e.PurchaseTimeframe).HasMaxLength(100);
            entity.Property(e => e.HandoffNotes).HasMaxLength(500);
            
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.DealerId);
            entity.HasIndex(e => e.VehicleId);
            entity.HasIndex(e => e.LeadScore);
            entity.HasIndex(e => e.StartedAt);
            entity.HasIndex(e => new { e.Status, e.LeadTemperature });
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.ToTable("messages");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            
            entity.Property(e => e.Role).IsRequired();
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.Type).IsRequired();
            
            entity.Property(e => e.DetectedIntent).HasMaxLength(100);
            entity.Property(e => e.ExtractedSignals).HasColumnType("jsonb");
            entity.Property(e => e.Metadata).HasColumnType("jsonb");
            
            entity.HasOne(e => e.Conversation)
                .WithMany(c => c.Messages)
                .HasForeignKey(e => e.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(e => e.ConversationId);
            entity.HasIndex(e => e.Timestamp);
        });

        modelBuilder.Entity<IntentAnalysis>(entity =>
        {
            entity.ToTable("intent_analyses");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            
            entity.Property(e => e.IntentType).IsRequired();
            entity.Property(e => e.Confidence).IsRequired();
            entity.Property(e => e.BuyingSignals).HasColumnType("jsonb");
            
            entity.Property(e => e.ExtractedUrgency).HasMaxLength(100);
            entity.Property(e => e.ExtractedBudget).HasMaxLength(100);
            entity.Property(e => e.TradeInDetails).HasMaxLength(500);
            
            entity.HasIndex(e => e.ConversationId);
            entity.HasIndex(e => e.MessageId);
            entity.HasIndex(e => e.IntentType);
        });

        modelBuilder.Entity<WhatsAppHandoff>(entity =>
        {
            entity.ToTable("whatsapp_handoffs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            
            entity.Property(e => e.UserName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.UserPhone).IsRequired().HasMaxLength(50);
            entity.Property(e => e.LeadScore).IsRequired();
            entity.Property(e => e.LeadTemperature).IsRequired();
            
            entity.Property(e => e.ConversationSummary).IsRequired();
            entity.Property(e => e.BuyingSignals).HasColumnType("jsonb");
            entity.Property(e => e.VehicleDetails).HasMaxLength(500);
            
            entity.Property(e => e.DealerWhatsAppNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.WhatsAppMessageId).HasMaxLength(200);
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.ErrorMessage).HasMaxLength(500);
            
            entity.HasOne(e => e.Conversation)
                .WithMany()
                .HasForeignKey(e => e.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(e => e.ConversationId);
            entity.HasIndex(e => e.DealerId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.InitiatedAt);
        });
    }
}
