using Microsoft.EntityFrameworkCore;
using LegalDocumentService.Domain.Entities;

namespace LegalDocumentService.Infrastructure.Persistence;

public class LegalDocumentDbContext : DbContext
{
    public LegalDocumentDbContext(DbContextOptions<LegalDocumentDbContext> options)
        : base(options)
    {
    }

    public DbSet<LegalDocument> LegalDocuments => Set<LegalDocument>();
    public DbSet<LegalDocumentVersion> LegalDocumentVersions => Set<LegalDocumentVersion>();
    public DbSet<UserAcceptance> UserAcceptances => Set<UserAcceptance>();
    public DbSet<DocumentTemplate> DocumentTemplates => Set<DocumentTemplate>();
    public DbSet<TemplateVariable> TemplateVariables => Set<TemplateVariable>();
    public DbSet<ComplianceRequirement> ComplianceRequirements => Set<ComplianceRequirement>();
    public DbSet<RequiredDocument> RequiredDocuments => Set<RequiredDocument>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // LegalDocument
        modelBuilder.Entity<LegalDocument>(entity =>
        {
            entity.ToTable("legal_documents");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(500).IsRequired();
            entity.Property(e => e.Slug).HasColumnName("slug").HasMaxLength(600).IsRequired();
            entity.Property(e => e.DocumentType).HasColumnName("document_type");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.ContentHtml).HasColumnName("content_html");
            entity.Property(e => e.Summary).HasColumnName("summary").HasMaxLength(1000);
            entity.Property(e => e.VersionMajor).HasColumnName("version_major");
            entity.Property(e => e.VersionMinor).HasColumnName("version_minor");
            entity.Property(e => e.VersionLabel).HasColumnName("version_label").HasMaxLength(20);
            entity.Property(e => e.Jurisdiction).HasColumnName("jurisdiction");
            entity.Property(e => e.Language).HasColumnName("language");
            entity.Property(e => e.EffectiveDate).HasColumnName("effective_date");
            entity.Property(e => e.ExpirationDate).HasColumnName("expiration_date");
            entity.Property(e => e.PublishedAt).HasColumnName("published_at");
            entity.Property(e => e.RequiresAcceptance).HasColumnName("requires_acceptance");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.IsMandatory).HasColumnName("is_mandatory");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100);
            entity.Property(e => e.ApprovedBy).HasColumnName("approved_by").HasMaxLength(100);
            entity.Property(e => e.ApprovedAt).HasColumnName("approved_at");
            entity.Property(e => e.LegalReferences).HasColumnName("legal_references");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.MetadataJson).HasColumnName("metadata_json");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

            entity.HasIndex(e => e.Slug).IsUnique();
            entity.HasIndex(e => e.DocumentType);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.IsActive);

            entity.HasMany(e => e.Versions)
                .WithOne(v => v.LegalDocument)
                .HasForeignKey(v => v.LegalDocumentId);
        });

        // LegalDocumentVersion
        modelBuilder.Entity<LegalDocumentVersion>(entity =>
        {
            entity.ToTable("legal_document_versions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.LegalDocumentId).HasColumnName("legal_document_id");
            entity.Property(e => e.VersionMajor).HasColumnName("version_major");
            entity.Property(e => e.VersionMinor).HasColumnName("version_minor");
            entity.Property(e => e.VersionLabel).HasColumnName("version_label").HasMaxLength(20);
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.ContentHtml).HasColumnName("content_html");
            entity.Property(e => e.ChangeNotes).HasColumnName("change_notes").HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasIndex(e => e.LegalDocumentId);
        });

        // UserAcceptance
        modelBuilder.Entity<UserAcceptance>(entity =>
        {
            entity.ToTable("user_acceptances");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.LegalDocumentId).HasColumnName("legal_document_id");
            entity.Property(e => e.UserId).HasColumnName("user_id").HasMaxLength(100).IsRequired();
            entity.Property(e => e.TransactionId).HasColumnName("transaction_id").HasMaxLength(100);
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.Method).HasColumnName("method");
            entity.Property(e => e.AcceptedAt).HasColumnName("accepted_at");
            entity.Property(e => e.DeclinedAt).HasColumnName("declined_at");
            entity.Property(e => e.RevokedAt).HasColumnName("revoked_at");
            entity.Property(e => e.IpAddress).HasColumnName("ip_address").HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasColumnName("user_agent").HasMaxLength(500);
            entity.Property(e => e.GeoLocation).HasColumnName("geo_location").HasMaxLength(200);
            entity.Property(e => e.DocumentVersionAccepted).HasColumnName("document_version_accepted").HasMaxLength(20);
            entity.Property(e => e.DocumentChecksum).HasColumnName("document_checksum").HasMaxLength(100);
            entity.Property(e => e.SignatureDataJson).HasColumnName("signature_data_json");
            entity.Property(e => e.ConsentProofJson).HasColumnName("consent_proof_json");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.LegalDocumentId);
            entity.HasIndex(e => new { e.UserId, e.LegalDocumentId });
            entity.HasIndex(e => e.TransactionId);

            entity.HasOne(e => e.LegalDocument)
                .WithMany()
                .HasForeignKey(e => e.LegalDocumentId);
        });

        // DocumentTemplate
        modelBuilder.Entity<DocumentTemplate>(entity =>
        {
            entity.ToTable("document_templates");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            entity.Property(e => e.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
            entity.Property(e => e.DocumentType).HasColumnName("document_type");
            entity.Property(e => e.TemplateContent).HasColumnName("template_content");
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(500);
            entity.Property(e => e.Language).HasColumnName("language");
            entity.Property(e => e.Jurisdiction).HasColumnName("jurisdiction");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.IsActive);

            entity.HasMany(e => e.Variables)
                .WithOne(v => v.DocumentTemplate)
                .HasForeignKey(v => v.DocumentTemplateId);
        });

        // TemplateVariable
        modelBuilder.Entity<TemplateVariable>(entity =>
        {
            entity.ToTable("template_variables");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DocumentTemplateId).HasColumnName("document_template_id");
            entity.Property(e => e.VariableName).HasColumnName("variable_name").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Placeholder).HasColumnName("placeholder").HasMaxLength(100).IsRequired();
            entity.Property(e => e.VariableType).HasColumnName("variable_type");
            entity.Property(e => e.IsRequired).HasColumnName("is_required");
            entity.Property(e => e.DefaultValue).HasColumnName("default_value").HasMaxLength(500);
            entity.Property(e => e.ValidationRegex).HasColumnName("validation_regex").HasMaxLength(500);
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

            entity.HasIndex(e => e.DocumentTemplateId);
        });

        // ComplianceRequirement
        modelBuilder.Entity<ComplianceRequirement>(entity =>
        {
            entity.ToTable("compliance_requirements");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            entity.Property(e => e.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.LegalBasis).HasColumnName("legal_basis").HasMaxLength(500);
            entity.Property(e => e.ArticleReference).HasColumnName("article_reference").HasMaxLength(200);
            entity.Property(e => e.Jurisdiction).HasColumnName("jurisdiction");
            entity.Property(e => e.IsMandatory).HasColumnName("is_mandatory");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.EffectiveDate).HasColumnName("effective_date");
            entity.Property(e => e.SunsetDate).HasColumnName("sunset_date");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.IsActive);

            entity.HasMany(e => e.RequiredDocuments)
                .WithOne(r => r.ComplianceRequirement)
                .HasForeignKey(r => r.ComplianceRequirementId);
        });

        // RequiredDocument
        modelBuilder.Entity<RequiredDocument>(entity =>
        {
            entity.ToTable("required_documents");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ComplianceRequirementId).HasColumnName("compliance_requirement_id");
            entity.Property(e => e.DocumentType).HasColumnName("document_type");
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(500);
            entity.Property(e => e.IsMandatory).HasColumnName("is_mandatory");
            entity.Property(e => e.DisplayOrder).HasColumnName("display_order");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

            entity.HasIndex(e => e.ComplianceRequirementId);
        });
    }
}
