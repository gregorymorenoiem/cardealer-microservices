// =====================================================
// DigitalSignatureService - DbContext
// Ley 339-22 Firma Digital de República Dominicana
// =====================================================

using Microsoft.EntityFrameworkCore;
using DigitalSignatureService.Domain.Entities;

namespace DigitalSignatureService.Infrastructure.Persistence;

public class SignatureDbContext : DbContext
{
    public SignatureDbContext(DbContextOptions<SignatureDbContext> options) : base(options) { }

    public DbSet<DigitalCertificate> Certificates => Set<DigitalCertificate>();
    public DbSet<DigitalSignature> Signatures => Set<DigitalSignature>();
    public DbSet<SignatureVerification> Verifications => Set<SignatureVerification>();
    public DbSet<TimeStamp> TimeStamps => Set<TimeStamp>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // DigitalCertificate
        modelBuilder.Entity<DigitalCertificate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SerialNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.SubjectName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.SubjectIdentification).IsRequired().HasMaxLength(20);
            entity.Property(e => e.IssuerName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.CertificateType).HasConversion<string>();
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.PublicKey).IsRequired();
            entity.HasIndex(e => e.SerialNumber).IsUnique();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.SubjectIdentification);
        });

        // DigitalSignature
        modelBuilder.Entity<DigitalSignature>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DocumentHash).IsRequired();
            entity.Property(e => e.SignatureValue).IsRequired();
            entity.Property(e => e.SignatureAlgorithm).HasConversion<string>();
            entity.Property(e => e.SignerName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.SignerIdentification).IsRequired().HasMaxLength(20);
            entity.HasOne(e => e.Certificate)
                  .WithMany(c => c.Signatures)
                  .HasForeignKey(e => e.CertificateId);
            entity.HasIndex(e => e.DocumentId);
            entity.HasIndex(e => e.CertificateId);
        });

        // SignatureVerification
        modelBuilder.Entity<SignatureVerification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.VerificationDetails).HasMaxLength(500);
            entity.HasOne(e => e.Signature)
                  .WithMany()
                  .HasForeignKey(e => e.SignatureId);
        });

        // TimeStamp
        modelBuilder.Entity<TimeStamp>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TimeStampToken).IsRequired();
            entity.Property(e => e.TsaName).IsRequired().HasMaxLength(200);
            entity.HasOne(e => e.Signature)
                  .WithMany()
                  .HasForeignKey(e => e.SignatureId);
        });
    }
}
