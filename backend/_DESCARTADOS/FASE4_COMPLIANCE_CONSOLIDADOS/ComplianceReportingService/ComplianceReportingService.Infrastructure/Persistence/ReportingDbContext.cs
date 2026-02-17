// ComplianceReportingService - DbContext
// Contexto de base de datos para reportería regulatoria

namespace ComplianceReportingService.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using ComplianceReportingService.Domain.Entities;

public class ReportingDbContext : DbContext
{
    public ReportingDbContext(DbContextOptions<ReportingDbContext> options) : base(options)
    {
    }

    public DbSet<Report> Reports => Set<Report>();
    public DbSet<ReportSchedule> ReportSchedules => Set<ReportSchedule>();
    public DbSet<ReportTemplate> ReportTemplates => Set<ReportTemplate>();
    public DbSet<ReportExecution> ReportExecutions => Set<ReportExecution>();
    public DbSet<ReportSubscription> ReportSubscriptions => Set<ReportSubscription>();
    public DbSet<DGIISubmission> DGIISubmissions => Set<DGIISubmission>();
    public DbSet<UAFSubmission> UAFSubmissions => Set<UAFSubmission>();
    public DbSet<ComplianceMetric> ComplianceMetrics => Set<ComplianceMetric>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Report
        modelBuilder.Entity<Report>(entity =>
        {
            entity.ToTable("reports");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ReportNumber).HasColumnName("report_number").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Type).HasColumnName("type").HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(30);
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(300).IsRequired();
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(1000);
            entity.Property(e => e.PeriodStart).HasColumnName("period_start");
            entity.Property(e => e.PeriodEnd).HasColumnName("period_end");
            entity.Property(e => e.Format).HasColumnName("format").HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.GeneratedAt).HasColumnName("generated_at");
            entity.Property(e => e.FilePath).HasColumnName("file_path").HasMaxLength(500);
            entity.Property(e => e.FileSize).HasColumnName("file_size");
            entity.Property(e => e.Destination).HasColumnName("destination").HasConversion<string>().HasMaxLength(30);
            entity.Property(e => e.SubmittedAt).HasColumnName("submitted_at");
            entity.Property(e => e.SubmissionReference).HasColumnName("submission_reference").HasMaxLength(100);
            entity.Property(e => e.RecordCount).HasColumnName("record_count");
            entity.Property(e => e.TotalAmount).HasColumnName("total_amount").HasPrecision(18, 2);
            entity.Property(e => e.Currency).HasColumnName("currency").HasMaxLength(3);
            entity.Property(e => e.ParametersJson).HasColumnName("parameters_json");
            entity.Property(e => e.Notes).HasColumnName("notes").HasMaxLength(2000);
            entity.Property(e => e.DueDate).HasColumnName("due_date");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(e => e.ReportNumber).IsUnique();
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Destination);
            entity.HasIndex(e => new { e.PeriodStart, e.PeriodEnd });
        });

        // ReportSchedule
        modelBuilder.Entity<ReportSchedule>(entity =>
        {
            entity.ToTable("report_schedules");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            entity.Property(e => e.ReportType).HasColumnName("report_type").HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.Frequency).HasColumnName("frequency").HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.Format).HasColumnName("format").HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.Destination).HasColumnName("destination").HasConversion<string>().HasMaxLength(30);
            entity.Property(e => e.CronExpression).HasColumnName("cron_expression").HasMaxLength(100);
            entity.Property(e => e.NextRunAt).HasColumnName("next_run_at");
            entity.Property(e => e.LastRunAt).HasColumnName("last_run_at");
            entity.Property(e => e.AutoSubmit).HasColumnName("auto_submit");
            entity.Property(e => e.NotificationEmail).HasColumnName("notification_email").HasMaxLength(200);
            entity.Property(e => e.ParametersJson).HasColumnName("parameters_json");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100);

            entity.HasIndex(e => e.ReportType);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.NextRunAt);
        });

        // ReportTemplate
        modelBuilder.Entity<ReportTemplate>(entity =>
        {
            entity.ToTable("report_templates");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            entity.Property(e => e.ForReportType).HasColumnName("for_report_type").HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(1000);
            entity.Property(e => e.TemplateContent).HasColumnName("template_content");
            entity.Property(e => e.QueryDefinition).HasColumnName("query_definition");
            entity.Property(e => e.ParametersSchema).HasColumnName("parameters_schema");
            entity.Property(e => e.Version).HasColumnName("version").HasMaxLength(20);
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.ForReportType);
        });

        // ReportExecution
        modelBuilder.Entity<ReportExecution>(entity =>
        {
            entity.ToTable("report_executions");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ReportId).HasColumnName("report_id");
            entity.Property(e => e.ScheduleId).HasColumnName("schedule_id");
            entity.Property(e => e.StartedAt).HasColumnName("started_at");
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
            entity.Property(e => e.DurationMs).HasColumnName("duration_ms");
            entity.Property(e => e.Success).HasColumnName("success");
            entity.Property(e => e.ErrorMessage).HasColumnName("error_message").HasMaxLength(2000);
            entity.Property(e => e.ExecutedBy).HasColumnName("executed_by").HasMaxLength(100);

            entity.HasIndex(e => e.ReportId);
            entity.HasIndex(e => e.ScheduleId);
            entity.HasIndex(e => e.Success);
        });

        // ReportSubscription
        modelBuilder.Entity<ReportSubscription>(entity =>
        {
            entity.ToTable("report_subscriptions");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.ReportType).HasColumnName("report_type").HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.Frequency).HasColumnName("frequency").HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.DeliveryMethod).HasColumnName("delivery_method").HasMaxLength(30);
            entity.Property(e => e.DeliveryAddress).HasColumnName("delivery_address").HasMaxLength(200);
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ReportType);
        });

        // DGIISubmission
        modelBuilder.Entity<DGIISubmission>(entity =>
        {
            entity.ToTable("dgii_submissions");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ReportId).HasColumnName("report_id");
            entity.Property(e => e.ReportCode).HasColumnName("report_code").HasMaxLength(10).IsRequired();
            entity.Property(e => e.RNC).HasColumnName("rnc").HasMaxLength(11).IsRequired();
            entity.Property(e => e.Period).HasColumnName("period").HasMaxLength(10).IsRequired();
            entity.Property(e => e.SubmissionDate).HasColumnName("submission_date");
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(30);
            entity.Property(e => e.ConfirmationNumber).HasColumnName("confirmation_number").HasMaxLength(50);
            entity.Property(e => e.ResponseMessage).HasColumnName("response_message").HasMaxLength(1000);
            entity.Property(e => e.Attempts).HasColumnName("attempts");

            entity.HasIndex(e => e.ReportId);
            entity.HasIndex(e => new { e.RNC, e.Period, e.ReportCode });
            entity.HasIndex(e => e.Status);
        });

        // UAFSubmission
        modelBuilder.Entity<UAFSubmission>(entity =>
        {
            entity.ToTable("uaf_submissions");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ReportId).HasColumnName("report_id");
            entity.Property(e => e.ReportCode).HasColumnName("report_code").HasMaxLength(10).IsRequired();
            entity.Property(e => e.EntityRNC).HasColumnName("entity_rnc").HasMaxLength(11).IsRequired();
            entity.Property(e => e.ReportingPeriod).HasColumnName("reporting_period").HasMaxLength(10);
            entity.Property(e => e.SubmissionDate).HasColumnName("submission_date");
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(30);
            entity.Property(e => e.UAFCaseNumber).HasColumnName("uaf_case_number").HasMaxLength(50);
            entity.Property(e => e.IsUrgent).HasColumnName("is_urgent");

            entity.HasIndex(e => e.ReportId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.IsUrgent);
        });

        // ComplianceMetric
        modelBuilder.Entity<ComplianceMetric>(entity =>
        {
            entity.ToTable("compliance_metrics");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.MetricCode).HasColumnName("metric_code").HasMaxLength(50).IsRequired();
            entity.Property(e => e.MetricName).HasColumnName("metric_name").HasMaxLength(200).IsRequired();
            entity.Property(e => e.Category).HasColumnName("category").HasMaxLength(100);
            entity.Property(e => e.Value).HasColumnName("value").HasPrecision(18, 4);
            entity.Property(e => e.Threshold).HasColumnName("threshold").HasPrecision(18, 4);
            entity.Property(e => e.Unit).HasColumnName("unit").HasMaxLength(30);
            entity.Property(e => e.MeasuredAt).HasColumnName("measured_at");
            entity.Property(e => e.IsAlert).HasColumnName("is_alert");
            entity.Property(e => e.AlertMessage).HasColumnName("alert_message").HasMaxLength(500);
            entity.Property(e => e.RecordedBy).HasColumnName("recorded_by").HasMaxLength(100);

            entity.HasIndex(e => e.MetricCode);
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.IsAlert);
            entity.HasIndex(e => e.MeasuredAt);
        });
    }
}
