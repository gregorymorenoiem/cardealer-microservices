// ContractService - Entity Framework DbContext

namespace ContractService.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using ContractService.Domain.Entities;

public class ContractDbContext : DbContext
{
    public ContractDbContext(DbContextOptions<ContractDbContext> options) : base(options) { }

    public DbSet<ContractTemplate> ContractTemplates => Set<ContractTemplate>();
    public DbSet<TemplateClause> TemplateClauses => Set<TemplateClause>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<ContractParty> ContractParties => Set<ContractParty>();
    public DbSet<ContractSignature> ContractSignatures => Set<ContractSignature>();
    public DbSet<ContractClause> ContractClauses => Set<ContractClause>();
    public DbSet<ContractVersion> ContractVersions => Set<ContractVersion>();
    public DbSet<ContractDocument> ContractDocuments => Set<ContractDocument>();
    public DbSet<ContractAuditLog> ContractAuditLogs => Set<ContractAuditLog>();
    public DbSet<CertificationAuthority> CertificationAuthorities => Set<CertificationAuthority>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Use snake_case for PostgreSQL
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            entity.SetTableName(ToSnakeCase(entity.GetTableName()!));
            foreach (var property in entity.GetProperties())
            {
                property.SetColumnName(ToSnakeCase(property.Name));
            }
        }

        #region ContractTemplate Configuration

        modelBuilder.Entity<ContractTemplate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.ContentHtml).IsRequired();
            entity.Property(e => e.Language).HasMaxLength(10);
            entity.Property(e => e.LegalBasis).HasMaxLength(500);
            entity.Property(e => e.RequiredVariables).HasColumnType("jsonb");
            entity.Property(e => e.OptionalVariables).HasColumnType("jsonb");
            entity.Property(e => e.Type).HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);

            entity.HasMany(e => e.Clauses)
                .WithOne(c => c.Template)
                .HasForeignKey(c => c.TemplateId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        #endregion

        #region TemplateClause Configuration

        modelBuilder.Entity<TemplateClause>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.Type).HasConversion<string>().HasMaxLength(50);
        });

        #endregion

        #region Contract Configuration

        modelBuilder.Entity<Contract>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ContractNumber).HasMaxLength(50).IsRequired();
            entity.HasIndex(e => e.ContractNumber).IsUnique();
            entity.Property(e => e.Title).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.ContentHtml).IsRequired();
            entity.Property(e => e.ContentHash).HasMaxLength(128);
            entity.Property(e => e.Type).HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.SubjectType).HasMaxLength(100);
            entity.Property(e => e.SubjectDescription).HasMaxLength(500);
            entity.Property(e => e.Currency).HasMaxLength(3);
            entity.Property(e => e.ContractValue).HasPrecision(18, 2);
            entity.Property(e => e.LegalJurisdiction).HasMaxLength(200);
            entity.Property(e => e.TerminationReason).HasMaxLength(1000);
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);

            entity.HasOne(e => e.Template)
                .WithMany(t => t.Contracts)
                .HasForeignKey(e => e.TemplateId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(e => e.Parties)
                .WithOne(p => p.Contract)
                .HasForeignKey(p => p.ContractId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Signatures)
                .WithOne(s => s.Contract)
                .HasForeignKey(s => s.ContractId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Clauses)
                .WithOne(c => c.Contract)
                .HasForeignKey(c => c.ContractId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Versions)
                .WithOne(v => v.Contract)
                .HasForeignKey(v => v.ContractId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Documents)
                .WithOne(d => d.Contract)
                .HasForeignKey(d => d.ContractId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.AuditLogs)
                .WithOne(a => a.Contract)
                .HasForeignKey(a => a.ContractId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => new { e.SubjectType, e.SubjectId });
            entity.HasIndex(e => e.EffectiveDate);
            entity.HasIndex(e => e.ExpirationDate);
        });

        #endregion

        #region ContractParty Configuration

        modelBuilder.Entity<ContractParty>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.Role).HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.FullName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.DocumentType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.DocumentNumber).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.CompanyName).HasMaxLength(200);
            entity.Property(e => e.RNC).HasMaxLength(20);
            entity.Property(e => e.LegalRepresentative).HasMaxLength(200);
            entity.Property(e => e.PowerOfAttorneyNumber).HasMaxLength(100);

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.DocumentNumber);
        });

        #endregion

        #region ContractSignature Configuration

        modelBuilder.Entity<ContractSignature>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.VerificationStatus).HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.SignatureData).HasColumnType("text");
            entity.Property(e => e.SignatureImage).HasColumnType("text");
            entity.Property(e => e.CertificateData).HasColumnType("text");
            entity.Property(e => e.DocumentHash).HasMaxLength(128);
            entity.Property(e => e.TimestampToken).HasMaxLength(500);
            entity.Property(e => e.IPAddress).HasMaxLength(50);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.GeoLocation).HasMaxLength(200);
            entity.Property(e => e.DeviceFingerprint).HasMaxLength(200);
            entity.Property(e => e.BiometricType).HasMaxLength(50);
            entity.Property(e => e.DeclineReason).HasMaxLength(500);
            entity.Property(e => e.VerificationDetails).HasMaxLength(1000);

            entity.HasOne(e => e.Party)
                .WithMany()
                .HasForeignKey(e => e.PartyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.CertificationAuthority)
                .WithMany()
                .HasForeignKey(e => e.CertificationAuthorityId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.Status);
        });

        #endregion

        #region ContractClause Configuration

        modelBuilder.Entity<ContractClause>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.Type).HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.ModifiedBy).HasMaxLength(100);
            entity.Property(e => e.ModificationReason).HasMaxLength(500);

            entity.HasOne(e => e.TemplateClause)
                .WithMany()
                .HasForeignKey(e => e.TemplateClauseId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.ContractId);
        });

        #endregion

        #region ContractVersion Configuration

        modelBuilder.Entity<ContractVersion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ContentHash).HasMaxLength(128);
            entity.Property(e => e.ChangeDescription).HasMaxLength(500);
            entity.Property(e => e.CreatedBy).HasMaxLength(100);

            entity.HasIndex(e => new { e.ContractId, e.VersionNumber }).IsUnique();
        });

        #endregion

        #region ContractDocument Configuration

        modelBuilder.Entity<ContractDocument>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.DocumentType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.FileName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.ContentType).HasMaxLength(100);
            entity.Property(e => e.StoragePath).HasMaxLength(500).IsRequired();
            entity.Property(e => e.FileHash).HasMaxLength(128);
            entity.Property(e => e.UploadedBy).HasMaxLength(100);
            entity.Property(e => e.VerifiedBy).HasMaxLength(100);

            entity.HasIndex(e => e.ContractId);
        });

        #endregion

        #region ContractAuditLog Configuration

        modelBuilder.Entity<ContractAuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EventType).HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.OldValue).HasColumnType("text");
            entity.Property(e => e.NewValue).HasColumnType("text");
            entity.Property(e => e.PerformedBy).HasMaxLength(100);
            entity.Property(e => e.IPAddress).HasMaxLength(50);
            entity.Property(e => e.UserAgent).HasMaxLength(500);

            entity.HasIndex(e => e.ContractId);
            entity.HasIndex(e => e.PerformedAt);
            entity.HasIndex(e => e.EventType);
        });

        #endregion

        #region CertificationAuthority Configuration

        modelBuilder.Entity<CertificationAuthority>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.Website).HasMaxLength(300);
            entity.Property(e => e.CertificateUrl).HasMaxLength(500);
            entity.Property(e => e.PublicKey).HasColumnType("text");
            entity.Property(e => e.AccreditationNumber).HasMaxLength(100);
            entity.Property(e => e.ContactEmail).HasMaxLength(200);
            entity.Property(e => e.SupportPhone).HasMaxLength(50);
            entity.Property(e => e.ApiEndpoint).HasMaxLength(500);
            entity.Property(e => e.ApiKey).HasMaxLength(200);

            entity.HasIndex(e => e.IsActive);
        });

        #endregion

        // Seed data for certification authorities in RD
        SeedCertificationAuthorities(modelBuilder);
    }

    private void SeedCertificationAuthorities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CertificationAuthority>().HasData(
            new CertificationAuthority
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Code = "INDOTEL-CA",
                Name = "INDOTEL - Instituto Dominicano de las Telecomunicaciones",
                Description = "Autoridad certificadora gubernamental de República Dominicana",
                Country = "República Dominicana",
                Website = "https://indotel.gob.do",
                IsActive = true,
                IsGovernmentApproved = true,
                AccreditationNumber = "AC-001-2020",
                ValidFrom = new DateTime(2020, 1, 1),
                ValidUntil = new DateTime(2030, 12, 31),
                CreatedAt = new DateTime(2024, 1, 1)
            },
            new CertificationAuthority
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                Code = "OGTIC-CA",
                Name = "OGTIC - Oficina Gubernamental de Tecnologías de la Información",
                Description = "Infraestructura de clave pública del gobierno dominicano",
                Country = "República Dominicana",
                Website = "https://ogtic.gob.do",
                IsActive = true,
                IsGovernmentApproved = true,
                AccreditationNumber = "AC-002-2020",
                ValidFrom = new DateTime(2020, 1, 1),
                ValidUntil = new DateTime(2030, 12, 31),
                CreatedAt = new DateTime(2024, 1, 1)
            }
        );
    }

    private static string ToSnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        
        var result = new System.Text.StringBuilder();
        for (int i = 0; i < input.Length; i++)
        {
            var c = input[i];
            if (char.IsUpper(c))
            {
                if (i > 0) result.Append('_');
                result.Append(char.ToLower(c));
            }
            else
            {
                result.Append(c);
            }
        }
        return result.ToString();
    }
}
