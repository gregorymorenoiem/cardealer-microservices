// ComplianceService - DbContext

namespace ComplianceService.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using ComplianceService.Domain.Entities;

public class ComplianceDbContext : DbContext
{
    public ComplianceDbContext(DbContextOptions<ComplianceDbContext> options) : base(options) { }

    public DbSet<RegulatoryFramework> RegulatoryFrameworks => Set<RegulatoryFramework>();
    public DbSet<ComplianceRequirement> ComplianceRequirements => Set<ComplianceRequirement>();
    public DbSet<ComplianceControl> ComplianceControls => Set<ComplianceControl>();
    public DbSet<ControlTest> ControlTests => Set<ControlTest>();
    public DbSet<ComplianceAssessment> ComplianceAssessments => Set<ComplianceAssessment>();
    public DbSet<ComplianceFinding> ComplianceFindings => Set<ComplianceFinding>();
    public DbSet<RemediationAction> RemediationActions => Set<RemediationAction>();
    public DbSet<RegulatoryReport> RegulatoryReports => Set<RegulatoryReport>();
    public DbSet<ComplianceCalendar> ComplianceCalendars => Set<ComplianceCalendar>();
    public DbSet<ComplianceTraining> ComplianceTrainings => Set<ComplianceTraining>();
    public DbSet<TrainingCompletion> TrainingCompletions => Set<TrainingCompletion>();
    public DbSet<ComplianceMetric> ComplianceMetrics => Set<ComplianceMetric>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // RegulatoryFramework
        modelBuilder.Entity<RegulatoryFramework>(entity =>
        {
            entity.ToTable("regulatory_frameworks");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Type).HasColumnName("type");
            entity.Property(e => e.LegalReference).HasColumnName("legal_reference");
            entity.Property(e => e.RegulatoryBody).HasColumnName("regulatory_body");
            entity.Property(e => e.EffectiveDate).HasColumnName("effective_date");
            entity.Property(e => e.ExpirationDate).HasColumnName("expiration_date");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.Version).HasColumnName("version").HasMaxLength(50);
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by").HasMaxLength(100);
            entity.HasIndex(e => e.Code).IsUnique();
        });

        // ComplianceRequirement
        modelBuilder.Entity<ComplianceRequirement>(entity =>
        {
            entity.ToTable("compliance_requirements");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.FrameworkId).HasColumnName("framework_id");
            entity.Property(e => e.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(300).IsRequired();
            entity.Property(e => e.Description).HasColumnName("description").IsRequired();
            entity.Property(e => e.Criticality).HasColumnName("criticality");
            entity.Property(e => e.ArticleReference).HasColumnName("article_reference");
            entity.Property(e => e.DeadlineDays).HasColumnName("deadline_days");
            entity.Property(e => e.EvaluationFrequency).HasColumnName("evaluation_frequency");
            entity.Property(e => e.RequiresEvidence).HasColumnName("requires_evidence");
            entity.Property(e => e.RequiresApproval).HasColumnName("requires_approval");
            entity.Property(e => e.EvidenceRequirements).HasColumnName("evidence_requirements");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100);
            entity.HasOne(e => e.Framework)
                  .WithMany(f => f.Requirements)
                  .HasForeignKey(e => e.FrameworkId);
        });

        // ComplianceControl
        modelBuilder.Entity<ComplianceControl>(entity =>
        {
            entity.ToTable("compliance_controls");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.FrameworkId).HasColumnName("framework_id");
            entity.Property(e => e.RequirementId).HasColumnName("requirement_id");
            entity.Property(e => e.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasColumnName("description").IsRequired();
            entity.Property(e => e.Type).HasColumnName("type");
            entity.Property(e => e.ImplementationDetails).HasColumnName("implementation_details");
            entity.Property(e => e.ResponsibleRole).HasColumnName("responsible_role");
            entity.Property(e => e.TestingFrequency).HasColumnName("testing_frequency");
            entity.Property(e => e.LastTestedAt).HasColumnName("last_tested_at");
            entity.Property(e => e.NextTestDate).HasColumnName("next_test_date");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.EffectivenessScore).HasColumnName("effectiveness_score");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by").HasMaxLength(100);
            entity.HasOne(e => e.Framework)
                  .WithMany(f => f.Controls)
                  .HasForeignKey(e => e.FrameworkId);
            entity.HasOne(e => e.Requirement)
                  .WithMany(r => r.Controls)
                  .HasForeignKey(e => e.RequirementId);
        });

        // ControlTest
        modelBuilder.Entity<ControlTest>(entity =>
        {
            entity.ToTable("control_tests");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ControlId).HasColumnName("control_id");
            entity.Property(e => e.TestDate).HasColumnName("test_date");
            entity.Property(e => e.TestedBy).HasColumnName("tested_by").HasMaxLength(100);
            entity.Property(e => e.TestProcedure).HasColumnName("test_procedure");
            entity.Property(e => e.TestResults).HasColumnName("test_results");
            entity.Property(e => e.IsPassed).HasColumnName("is_passed");
            entity.Property(e => e.EffectivenessScore).HasColumnName("effectiveness_score");
            entity.Property(e => e.Findings).HasColumnName("findings");
            entity.Property(e => e.Recommendations).HasColumnName("recommendations");
            entity.Property(e => e.EvidenceDocuments).HasColumnName("evidence_documents")
                  .HasConversion(
                      v => string.Join(";", v),
                      v => v.Split(";", StringSplitOptions.RemoveEmptyEntries).ToList()
                  );
            entity.HasOne(e => e.Control)
                  .WithMany(c => c.Tests)
                  .HasForeignKey(e => e.ControlId);
        });

        // ComplianceAssessment
        modelBuilder.Entity<ComplianceAssessment>(entity =>
        {
            entity.ToTable("compliance_assessments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EntityType).HasColumnName("entity_type").HasMaxLength(100);
            entity.Property(e => e.EntityId).HasColumnName("entity_id");
            entity.Property(e => e.RequirementId).HasColumnName("requirement_id");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.AssessmentDate).HasColumnName("assessment_date");
            entity.Property(e => e.AssessedBy).HasColumnName("assessed_by").HasMaxLength(100);
            entity.Property(e => e.Score).HasColumnName("score");
            entity.Property(e => e.Observations).HasColumnName("observations");
            entity.Property(e => e.EvidenceProvided).HasColumnName("evidence_provided");
            entity.Property(e => e.NextAssessmentDate).HasColumnName("next_assessment_date");
            entity.Property(e => e.DeadlineDate).HasColumnName("deadline_date");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne(e => e.Requirement)
                  .WithMany(r => r.Assessments)
                  .HasForeignKey(e => e.RequirementId);
            entity.HasIndex(e => new { e.EntityType, e.EntityId });
        });

        // ComplianceFinding
        modelBuilder.Entity<ComplianceFinding>(entity =>
        {
            entity.ToTable("compliance_findings");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AssessmentId).HasColumnName("assessment_id");
            entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(300);
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Type).HasColumnName("type");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.Criticality).HasColumnName("criticality");
            entity.Property(e => e.RootCause).HasColumnName("root_cause");
            entity.Property(e => e.Impact).HasColumnName("impact");
            entity.Property(e => e.Recommendation).HasColumnName("recommendation");
            entity.Property(e => e.AssignedTo).HasColumnName("assigned_to").HasMaxLength(100);
            entity.Property(e => e.DueDate).HasColumnName("due_date");
            entity.Property(e => e.ResolvedAt).HasColumnName("resolved_at");
            entity.Property(e => e.ResolvedBy).HasColumnName("resolved_by").HasMaxLength(100);
            entity.Property(e => e.Resolution).HasColumnName("resolution");
            entity.Property(e => e.EvidenceDocuments).HasColumnName("evidence_documents")
                  .HasConversion(
                      v => string.Join(";", v),
                      v => v.Split(";", StringSplitOptions.RemoveEmptyEntries).ToList()
                  );
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by").HasMaxLength(100);
            entity.HasOne(e => e.Assessment)
                  .WithMany(a => a.Findings)
                  .HasForeignKey(e => e.AssessmentId);
        });

        // RemediationAction
        modelBuilder.Entity<RemediationAction>(entity =>
        {
            entity.ToTable("remediation_actions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.FindingId).HasColumnName("finding_id");
            entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(300);
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.AssignedTo).HasColumnName("assigned_to").HasMaxLength(100);
            entity.Property(e => e.DueDate).HasColumnName("due_date");
            entity.Property(e => e.Priority).HasColumnName("priority");
            entity.Property(e => e.CompletionNotes).HasColumnName("completion_notes");
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
            entity.Property(e => e.CompletedBy).HasColumnName("completed_by").HasMaxLength(100);
            entity.Property(e => e.RequiresVerification).HasColumnName("requires_verification");
            entity.Property(e => e.VerifiedAt).HasColumnName("verified_at");
            entity.Property(e => e.VerifiedBy).HasColumnName("verified_by").HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100);
            entity.HasOne(e => e.Finding)
                  .WithMany(f => f.RemediationActions)
                  .HasForeignKey(e => e.FindingId);
        });

        // RegulatoryReport
        modelBuilder.Entity<RegulatoryReport>(entity =>
        {
            entity.ToTable("regulatory_reports");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ReportNumber).HasColumnName("report_number").HasMaxLength(50);
            entity.Property(e => e.Type).HasColumnName("type");
            entity.Property(e => e.RegulationType).HasColumnName("regulation_type");
            entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(300);
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.PeriodStart).HasColumnName("period_start");
            entity.Property(e => e.PeriodEnd).HasColumnName("period_end");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.RegulatoryBody).HasColumnName("regulatory_body").HasMaxLength(100);
            entity.Property(e => e.SubmissionDeadline).HasColumnName("submission_deadline");
            entity.Property(e => e.SubmittedAt).HasColumnName("submitted_at");
            entity.Property(e => e.SubmittedBy).HasColumnName("submitted_by").HasMaxLength(100);
            entity.Property(e => e.SubmissionReference).HasColumnName("submission_reference").HasMaxLength(100);
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.Attachments).HasColumnName("attachments")
                  .HasConversion(
                      v => string.Join(";", v),
                      v => v.Split(";", StringSplitOptions.RemoveEmptyEntries).ToList()
                  );
            entity.Property(e => e.ReviewComments).HasColumnName("review_comments");
            entity.Property(e => e.RegulatoryResponse).HasColumnName("regulatory_response");
            entity.Property(e => e.PreparedBy).HasColumnName("prepared_by").HasMaxLength(100);
            entity.Property(e => e.PreparedAt).HasColumnName("prepared_at");
            entity.Property(e => e.ReviewedBy).HasColumnName("reviewed_by").HasMaxLength(100);
            entity.Property(e => e.ReviewedAt).HasColumnName("reviewed_at");
            entity.Property(e => e.ApprovedBy).HasColumnName("approved_by").HasMaxLength(100);
            entity.Property(e => e.ApprovedAt).HasColumnName("approved_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by").HasMaxLength(100);
            entity.HasIndex(e => e.ReportNumber).IsUnique();
        });

        // ComplianceCalendar
        modelBuilder.Entity<ComplianceCalendar>(entity =>
        {
            entity.ToTable("compliance_calendars");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(300);
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.RegulationType).HasColumnName("regulation_type");
            entity.Property(e => e.DueDate).HasColumnName("due_date");
            entity.Property(e => e.ReminderDaysBefore).HasColumnName("reminder_days_before");
            entity.Property(e => e.IsRecurring).HasColumnName("is_recurring");
            entity.Property(e => e.RecurrencePattern).HasColumnName("recurrence_pattern");
            entity.Property(e => e.AssignedTo).HasColumnName("assigned_to").HasMaxLength(100);
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
            entity.Property(e => e.CompletedBy).HasColumnName("completed_by").HasMaxLength(100);
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.NotificationSent).HasColumnName("notification_sent");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100);
            entity.HasIndex(e => e.DueDate);
        });

        // ComplianceTraining
        modelBuilder.Entity<ComplianceTraining>(entity =>
        {
            entity.ToTable("compliance_trainings");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(300);
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.RegulationType).HasColumnName("regulation_type");
            entity.Property(e => e.TargetRoles).HasColumnName("target_roles");
            entity.Property(e => e.IsMandatory).HasColumnName("is_mandatory");
            entity.Property(e => e.DurationMinutes).HasColumnName("duration_minutes");
            entity.Property(e => e.ContentUrl).HasColumnName("content_url");
            entity.Property(e => e.ExamUrl).HasColumnName("exam_url");
            entity.Property(e => e.PassingScore).HasColumnName("passing_score");
            entity.Property(e => e.ValidUntil).HasColumnName("valid_until");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100);
        });

        // TrainingCompletion
        modelBuilder.Entity<TrainingCompletion>(entity =>
        {
            entity.ToTable("training_completions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.TrainingId).HasColumnName("training_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.StartedAt).HasColumnName("started_at");
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
            entity.Property(e => e.Score).HasColumnName("score");
            entity.Property(e => e.IsPassed).HasColumnName("is_passed");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.CertificateUrl).HasColumnName("certificate_url");
            entity.HasOne(e => e.Training)
                  .WithMany(t => t.Completions)
                  .HasForeignKey(e => e.TrainingId);
            entity.HasIndex(e => new { e.UserId, e.TrainingId });
        });

        // ComplianceMetric
        modelBuilder.Entity<ComplianceMetric>(entity =>
        {
            entity.ToTable("compliance_metrics");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.RegulationType).HasColumnName("regulation_type");
            entity.Property(e => e.MetricName).HasColumnName("metric_name").HasMaxLength(200);
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.PeriodStart).HasColumnName("period_start");
            entity.Property(e => e.PeriodEnd).HasColumnName("period_end");
            entity.Property(e => e.Value).HasColumnName("value").HasPrecision(18, 4);
            entity.Property(e => e.Unit).HasColumnName("unit").HasMaxLength(50);
            entity.Property(e => e.Target).HasColumnName("target").HasPrecision(18, 4);
            entity.Property(e => e.Threshold).HasColumnName("threshold").HasPrecision(18, 4);
            entity.Property(e => e.IsWithinTarget).HasColumnName("is_within_target");
            entity.Property(e => e.CalculationMethod).HasColumnName("calculation_method");
            entity.Property(e => e.CalculatedAt).HasColumnName("calculated_at");
            entity.Property(e => e.CalculatedBy).HasColumnName("calculated_by").HasMaxLength(100);
            entity.HasIndex(e => new { e.MetricName, e.PeriodEnd });
        });
    }
}
