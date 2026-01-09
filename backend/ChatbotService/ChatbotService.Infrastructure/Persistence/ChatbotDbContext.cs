using Microsoft.EntityFrameworkCore;
using ChatbotService.Domain.Entities;

namespace ChatbotService.Infrastructure.Persistence;

public class ChatbotDbContext : DbContext
{
    public ChatbotDbContext(DbContextOptions<ChatbotDbContext> options) : base(options)
    {
    }

    public DbSet<ChatConversation> Conversations => Set<ChatConversation>();
    public DbSet<ChatMessage> Messages => Set<ChatMessage>();
    public DbSet<ChatbotConfiguration> Configurations => Set<ChatbotConfiguration>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ChatConversation configuration
        modelBuilder.Entity<ChatConversation>(entity =>
        {
            entity.ToTable("chat_conversations");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.SessionId).HasColumnName("session_id").HasMaxLength(100);
            entity.Property(e => e.VehicleId).HasColumnName("vehicle_id");
            entity.Property(e => e.VehicleContext).HasColumnName("vehicle_context").HasColumnType("jsonb");
            entity.Property(e => e.Status).HasColumnName("status").HasConversion<string>();
            entity.Property(e => e.UserEmail).HasColumnName("user_email").HasMaxLength(256);
            entity.Property(e => e.UserName).HasColumnName("user_name").HasMaxLength(256);
            entity.Property(e => e.UserPhone).HasColumnName("user_phone").HasMaxLength(50);
            entity.Property(e => e.MessageCount).HasColumnName("message_count");
            entity.Property(e => e.TotalTokensUsed).HasColumnName("total_tokens_used");
            entity.Property(e => e.EstimatedCost).HasColumnName("estimated_cost").HasPrecision(10, 6);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.EndedAt).HasColumnName("ended_at");
            entity.Property(e => e.EndReason).HasColumnName("end_reason").HasMaxLength(500);
            entity.Property(e => e.LeadQualification).HasColumnName("lead_qualification").HasConversion<string>();
            entity.Property(e => e.LeadScore).HasColumnName("lead_score");
            entity.Property(e => e.Metadata).HasColumnName("metadata").HasColumnType("jsonb");

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.SessionId);
            entity.HasIndex(e => e.VehicleId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);

            entity.HasMany(e => e.Messages)
                .WithOne(m => m.Conversation)
                .HasForeignKey(m => m.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ChatMessage configuration
        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.ToTable("chat_messages");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ConversationId).HasColumnName("conversation_id");
            entity.Property(e => e.Role).HasColumnName("role").HasConversion<string>();
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.Type).HasColumnName("type").HasConversion<string>();
            entity.Property(e => e.TokenCount).HasColumnName("token_count");
            entity.Property(e => e.ResponseTime).HasColumnName("response_time");
            entity.Property(e => e.IntentDetected).HasColumnName("intent_detected").HasMaxLength(100);
            entity.Property(e => e.SentimentScore).HasColumnName("sentiment_score");
            entity.Property(e => e.Metadata).HasColumnName("metadata").HasColumnType("jsonb");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasIndex(e => e.ConversationId);
            entity.HasIndex(e => e.CreatedAt);
        });

        // ChatbotConfiguration configuration
        modelBuilder.Entity<ChatbotConfiguration>(entity =>
        {
            entity.ToTable("chatbot_configurations");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100);
            entity.Property(e => e.SystemPrompt).HasColumnName("system_prompt");
            entity.Property(e => e.Model).HasColumnName("model").HasMaxLength(50);
            entity.Property(e => e.Temperature).HasColumnName("temperature");
            entity.Property(e => e.MaxTokens).HasColumnName("max_tokens");
            entity.Property(e => e.MaxConversationMessages).HasColumnName("max_conversation_messages");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.WelcomeMessage).HasColumnName("welcome_message");
            entity.Property(e => e.FallbackMessage).HasColumnName("fallback_message");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.Ignore(e => e.QuickReplies);
        });
    }
}
