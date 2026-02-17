// DisputeService - EF Core DbContext

namespace DisputeService.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using DisputeService.Domain.Entities;

public class DisputeDbContext : DbContext
{
    public DisputeDbContext(DbContextOptions<DisputeDbContext> options) : base(options) { }

    public DbSet<Dispute> Disputes => Set<Dispute>();
    public DbSet<DisputeEvidence> DisputeEvidences => Set<DisputeEvidence>();
    public DbSet<DisputeComment> DisputeComments => Set<DisputeComment>();
    public DbSet<DisputeTimelineEvent> DisputeTimelineEvents => Set<DisputeTimelineEvent>();
    public DbSet<MediationSession> MediationSessions => Set<MediationSession>();
    public DbSet<DisputeParticipant> DisputeParticipants => Set<DisputeParticipant>();
    public DbSet<ResolutionTemplate> ResolutionTemplates => Set<ResolutionTemplate>();
    public DbSet<DisputeSlaConfiguration> DisputeSlaConfigurations => Set<DisputeSlaConfiguration>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Dispute
        modelBuilder.Entity<Dispute>(entity =>
        {
            entity.ToTable("disputes");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CaseNumber).HasColumnName("case_number").HasMaxLength(50);
            entity.Property(e => e.Type).HasColumnName("type").HasConversion<string>();
            entity.Property(e => e.Status).HasColumnName("status").HasConversion<string>();
            entity.Property(e => e.Priority).HasColumnName("priority").HasConversion<string>();
            
            entity.Property(e => e.ComplainantId).HasColumnName("complainant_id");
            entity.Property(e => e.ComplainantName).HasColumnName("complainant_name").HasMaxLength(200);
            entity.Property(e => e.ComplainantEmail).HasColumnName("complainant_email").HasMaxLength(200);
            entity.Property(e => e.RespondentId).HasColumnName("respondent_id");
            entity.Property(e => e.RespondentName).HasColumnName("respondent_name").HasMaxLength(200);
            entity.Property(e => e.RespondentEmail).HasColumnName("respondent_email").HasMaxLength(200);
            
            entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(500);
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DisputedAmount).HasColumnName("disputed_amount").HasPrecision(18, 2);
            entity.Property(e => e.Currency).HasColumnName("currency").HasMaxLength(3);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            
            entity.Property(e => e.AssignedMediatorId).HasColumnName("assigned_mediator_id");
            entity.Property(e => e.AssignedMediatorName).HasColumnName("assigned_mediator_name").HasMaxLength(200);
            entity.Property(e => e.AssignedAt).HasColumnName("assigned_at");
            
            entity.Property(e => e.Resolution).HasColumnName("resolution").HasConversion<string>();
            entity.Property(e => e.ResolutionSummary).HasColumnName("resolution_summary");
            entity.Property(e => e.ResolvedAt).HasColumnName("resolved_at");
            entity.Property(e => e.ResolvedBy).HasColumnName("resolved_by").HasMaxLength(200);
            
            entity.Property(e => e.IsEscalated).HasColumnName("is_escalated");
            entity.Property(e => e.EscalatedAt).HasColumnName("escalated_at");
            entity.Property(e => e.ReferredToProConsumidor).HasColumnName("referred_to_pro_consumidor");
            entity.Property(e => e.ProConsumidorCaseNumber).HasColumnName("pro_consumidor_case_number").HasMaxLength(50);
            entity.Property(e => e.ProConsumidorReferralDate).HasColumnName("pro_consumidor_referral_date");
            
            entity.Property(e => e.ResponseDueDate).HasColumnName("response_due_date");
            entity.Property(e => e.ResolutionDueDate).HasColumnName("resolution_due_date");
            entity.Property(e => e.FiledAt).HasColumnName("filed_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(e => e.CaseNumber).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.ComplainantId);
            entity.HasIndex(e => e.RespondentId);
            entity.HasIndex(e => e.AssignedMediatorId);
        });

        // DisputeEvidence
        modelBuilder.Entity<DisputeEvidence>(entity =>
        {
            entity.ToTable("dispute_evidences");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DisputeId).HasColumnName("dispute_id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(200);
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.EvidenceType).HasColumnName("evidence_type").HasMaxLength(100);
            entity.Property(e => e.FileName).HasColumnName("file_name").HasMaxLength(500);
            entity.Property(e => e.ContentType).HasColumnName("content_type").HasMaxLength(100);
            entity.Property(e => e.FileSize).HasColumnName("file_size");
            entity.Property(e => e.StoragePath).HasColumnName("storage_path").HasMaxLength(1000);
            entity.Property(e => e.SubmittedById).HasColumnName("submitted_by_id");
            entity.Property(e => e.SubmittedByName).HasColumnName("submitted_by_name").HasMaxLength(200);
            entity.Property(e => e.SubmitterRole).HasColumnName("submitter_role").HasConversion<string>();
            entity.Property(e => e.Status).HasColumnName("status").HasConversion<string>();
            entity.Property(e => e.ReviewedAt).HasColumnName("reviewed_at");
            entity.Property(e => e.ReviewedBy).HasColumnName("reviewed_by").HasMaxLength(200);
            entity.Property(e => e.ReviewNotes).HasColumnName("review_notes");
            entity.Property(e => e.SubmittedAt).HasColumnName("submitted_at");

            entity.HasIndex(e => e.DisputeId);
            entity.HasIndex(e => e.Status);
        });

        // DisputeComment
        modelBuilder.Entity<DisputeComment>(entity =>
        {
            entity.ToTable("dispute_comments");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DisputeId).HasColumnName("dispute_id");
            entity.Property(e => e.AuthorId).HasColumnName("author_id");
            entity.Property(e => e.AuthorName).HasColumnName("author_name").HasMaxLength(200);
            entity.Property(e => e.AuthorRole).HasColumnName("author_role").HasConversion<string>();
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.IsInternal).HasColumnName("is_internal");
            entity.Property(e => e.IsOfficial).HasColumnName("is_official");
            entity.Property(e => e.ParentCommentId).HasColumnName("parent_comment_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.EditedAt).HasColumnName("edited_at");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

            entity.HasIndex(e => e.DisputeId);
        });

        // DisputeTimelineEvent
        modelBuilder.Entity<DisputeTimelineEvent>(entity =>
        {
            entity.ToTable("dispute_timeline_events");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DisputeId).HasColumnName("dispute_id");
            entity.Property(e => e.EventType).HasColumnName("event_type").HasMaxLength(100);
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.OldValue).HasColumnName("old_value");
            entity.Property(e => e.NewValue).HasColumnName("new_value");
            entity.Property(e => e.PerformedBy).HasColumnName("performed_by").HasMaxLength(200);
            entity.Property(e => e.PerformerRole).HasColumnName("performer_role").HasConversion<string>();
            entity.Property(e => e.OccurredAt).HasColumnName("occurred_at");
            entity.Property(e => e.IpAddress).HasColumnName("ip_address").HasMaxLength(50);

            entity.HasIndex(e => e.DisputeId);
        });

        // MediationSession
        modelBuilder.Entity<MediationSession>(entity =>
        {
            entity.ToTable("mediation_sessions");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DisputeId).HasColumnName("dispute_id");
            entity.Property(e => e.SessionNumber).HasColumnName("session_number").HasMaxLength(50);
            entity.Property(e => e.ScheduledAt).HasColumnName("scheduled_at");
            entity.Property(e => e.StartedAt).HasColumnName("started_at");
            entity.Property(e => e.EndedAt).HasColumnName("ended_at");
            entity.Property(e => e.DurationMinutes).HasColumnName("duration_minutes");
            entity.Property(e => e.Channel).HasColumnName("channel").HasConversion<string>();
            entity.Property(e => e.MeetingLink).HasColumnName("meeting_link").HasMaxLength(500);
            entity.Property(e => e.Location).HasColumnName("location").HasMaxLength(500);
            entity.Property(e => e.MediatorId).HasColumnName("mediator_id");
            entity.Property(e => e.MediatorName).HasColumnName("mediator_name").HasMaxLength(200);
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(50);
            entity.Property(e => e.Summary).HasColumnName("summary");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.PartiesAgreed).HasColumnName("parties_agreed");
            entity.Property(e => e.ComplainantAttended).HasColumnName("complainant_attended");
            entity.Property(e => e.RespondentAttended).HasColumnName("respondent_attended");

            entity.HasIndex(e => e.DisputeId);
            entity.HasIndex(e => e.MediatorId);
        });

        // DisputeParticipant
        modelBuilder.Entity<DisputeParticipant>(entity =>
        {
            entity.ToTable("dispute_participants");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DisputeId).HasColumnName("dispute_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.UserName).HasColumnName("user_name").HasMaxLength(200);
            entity.Property(e => e.UserEmail).HasColumnName("user_email").HasMaxLength(200);
            entity.Property(e => e.Role).HasColumnName("role").HasConversion<string>();
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.JoinedAt).HasColumnName("joined_at");

            entity.HasIndex(e => e.DisputeId);
            entity.HasIndex(e => new { e.DisputeId, e.UserId }).IsUnique();
        });

        // ResolutionTemplate
        modelBuilder.Entity<ResolutionTemplate>(entity =>
        {
            entity.ToTable("resolution_templates");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(200);
            entity.Property(e => e.ForDisputeType).HasColumnName("for_dispute_type").HasConversion<string>();
            entity.Property(e => e.ResolutionType).HasColumnName("resolution_type").HasConversion<string>();
            entity.Property(e => e.TemplateContent).HasColumnName("template_content");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(200);
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasIndex(e => e.ForDisputeType);
        });

        // DisputeSlaConfiguration
        modelBuilder.Entity<DisputeSlaConfiguration>(entity =>
        {
            entity.ToTable("dispute_sla_configurations");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DisputeType).HasColumnName("dispute_type").HasConversion<string>();
            entity.Property(e => e.Priority).HasColumnName("priority").HasConversion<string>();
            entity.Property(e => e.ResponseDeadlineHours).HasColumnName("response_deadline_hours");
            entity.Property(e => e.ResolutionDeadlineHours).HasColumnName("resolution_deadline_hours");
            entity.Property(e => e.EscalationThresholdHours).HasColumnName("escalation_threshold_hours");
            entity.Property(e => e.IsActive).HasColumnName("is_active");

            entity.HasIndex(e => new { e.DisputeType, e.Priority }).IsUnique();
        });
    }
}
