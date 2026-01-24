// EscrowService - Entity Framework DbContext

namespace EscrowService.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using EscrowService.Domain.Entities;

public class EscrowDbContext : DbContext
{
    public EscrowDbContext(DbContextOptions<EscrowDbContext> options) : base(options) { }

    public DbSet<EscrowAccount> EscrowAccounts => Set<EscrowAccount>();
    public DbSet<ReleaseCondition> ReleaseConditions => Set<ReleaseCondition>();
    public DbSet<FundMovement> FundMovements => Set<FundMovement>();
    public DbSet<EscrowDocument> EscrowDocuments => Set<EscrowDocument>();
    public DbSet<EscrowDispute> EscrowDisputes => Set<EscrowDispute>();
    public DbSet<EscrowAuditLog> EscrowAuditLogs => Set<EscrowAuditLog>();
    public DbSet<EscrowFeeConfiguration> EscrowFeeConfigurations => Set<EscrowFeeConfiguration>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // EscrowAccount Configuration
        modelBuilder.Entity<EscrowAccount>(entity =>
        {
            entity.ToTable("escrow_accounts");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccountNumber).HasColumnName("account_number").HasMaxLength(50).IsRequired();
            entity.Property(e => e.TransactionType).HasColumnName("transaction_type").HasConversion<string>();
            entity.Property(e => e.Status).HasColumnName("status").HasConversion<string>();
            
            entity.Property(e => e.BuyerId).HasColumnName("buyer_id").IsRequired();
            entity.Property(e => e.BuyerName).HasColumnName("buyer_name").HasMaxLength(200).IsRequired();
            entity.Property(e => e.BuyerEmail).HasColumnName("buyer_email").HasMaxLength(255).IsRequired();
            
            entity.Property(e => e.SellerId).HasColumnName("seller_id").IsRequired();
            entity.Property(e => e.SellerName).HasColumnName("seller_name").HasMaxLength(200).IsRequired();
            entity.Property(e => e.SellerEmail).HasColumnName("seller_email").HasMaxLength(255).IsRequired();
            
            entity.Property(e => e.SubjectType).HasColumnName("subject_type").HasMaxLength(50);
            entity.Property(e => e.SubjectId).HasColumnName("subject_id");
            entity.Property(e => e.SubjectDescription).HasColumnName("subject_description").HasMaxLength(500);
            entity.Property(e => e.ContractId).HasColumnName("contract_id");
            
            entity.Property(e => e.TotalAmount).HasColumnName("total_amount").HasPrecision(18, 2);
            entity.Property(e => e.FundedAmount).HasColumnName("funded_amount").HasPrecision(18, 2);
            entity.Property(e => e.ReleasedAmount).HasColumnName("released_amount").HasPrecision(18, 2);
            entity.Property(e => e.RefundedAmount).HasColumnName("refunded_amount").HasPrecision(18, 2);
            entity.Property(e => e.FeeAmount).HasColumnName("fee_amount").HasPrecision(18, 2);
            entity.Property(e => e.Currency).HasColumnName("currency").HasMaxLength(3).HasDefaultValue("DOP");
            
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.FundedAt).HasColumnName("funded_at");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.ReleasedAt).HasColumnName("released_at");
            
            entity.Property(e => e.BuyerApproved).HasColumnName("buyer_approved");
            entity.Property(e => e.SellerApproved).HasColumnName("seller_approved");
            entity.Property(e => e.BuyerApprovedAt).HasColumnName("buyer_approved_at");
            entity.Property(e => e.SellerApprovedAt).HasColumnName("seller_approved_at");
            
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.TermsAccepted).HasColumnName("terms_accepted");
            entity.Property(e => e.TermsAcceptedAt).HasColumnName("terms_accepted_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100);

            entity.HasIndex(e => e.AccountNumber).IsUnique();
            entity.HasIndex(e => e.BuyerId);
            entity.HasIndex(e => e.SellerId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.ExpiresAt);
        });

        // ReleaseCondition Configuration
        modelBuilder.Entity<ReleaseCondition>(entity =>
        {
            entity.ToTable("release_conditions");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EscrowAccountId).HasColumnName("escrow_account_id");
            entity.Property(e => e.Type).HasColumnName("type").HasConversion<string>();
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(500);
            entity.Property(e => e.Status).HasColumnName("status").HasConversion<string>();
            entity.Property(e => e.IsMandatory).HasColumnName("is_mandatory");
            entity.Property(e => e.Order).HasColumnName("order");
            entity.Property(e => e.RequiresEvidence).HasColumnName("requires_evidence");
            entity.Property(e => e.DueDate).HasColumnName("due_date");
            entity.Property(e => e.MetAt).HasColumnName("met_at");
            entity.Property(e => e.VerifiedBy).HasColumnName("verified_by").HasMaxLength(100);
            entity.Property(e => e.VerificationNotes).HasColumnName("verification_notes").HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasIndex(e => e.EscrowAccountId);
            entity.HasIndex(e => e.Status);
        });

        // FundMovement Configuration
        modelBuilder.Entity<FundMovement>(entity =>
        {
            entity.ToTable("fund_movements");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EscrowAccountId).HasColumnName("escrow_account_id");
            entity.Property(e => e.TransactionNumber).HasColumnName("transaction_number").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Type).HasColumnName("type").HasConversion<string>();
            entity.Property(e => e.PaymentMethod).HasColumnName("payment_method").HasConversion<string>();
            entity.Property(e => e.Amount).HasColumnName("amount").HasPrecision(18, 2);
            entity.Property(e => e.FeeAmount).HasColumnName("fee_amount").HasPrecision(18, 2);
            entity.Property(e => e.Currency).HasColumnName("currency").HasMaxLength(3);
            entity.Property(e => e.SourceAccount).HasColumnName("source_account").HasMaxLength(100);
            entity.Property(e => e.DestinationAccount).HasColumnName("destination_account").HasMaxLength(100);
            entity.Property(e => e.BankName).HasColumnName("bank_name").HasMaxLength(100);
            entity.Property(e => e.BankReference).HasColumnName("bank_reference").HasMaxLength(100);
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
            entity.Property(e => e.InitiatedBy).HasColumnName("initiated_by").HasMaxLength(100);
            entity.Property(e => e.Notes).HasColumnName("notes");

            entity.HasIndex(e => e.TransactionNumber).IsUnique();
            entity.HasIndex(e => e.EscrowAccountId);
        });

        // EscrowDocument Configuration
        modelBuilder.Entity<EscrowDocument>(entity =>
        {
            entity.ToTable("escrow_documents");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EscrowAccountId).HasColumnName("escrow_account_id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(500);
            entity.Property(e => e.DocumentType).HasColumnName("document_type").HasMaxLength(50);
            entity.Property(e => e.FileName).HasColumnName("file_name").HasMaxLength(255);
            entity.Property(e => e.FilePath).HasColumnName("file_path").HasMaxLength(500);
            entity.Property(e => e.ContentType).HasColumnName("content_type").HasMaxLength(100);
            entity.Property(e => e.FileSize).HasColumnName("file_size");
            entity.Property(e => e.FileHash).HasColumnName("file_hash").HasMaxLength(128);
            entity.Property(e => e.IsVerified).HasColumnName("is_verified");
            entity.Property(e => e.VerifiedAt).HasColumnName("verified_at");
            entity.Property(e => e.VerifiedBy).HasColumnName("verified_by").HasMaxLength(100);
            entity.Property(e => e.UploadedAt).HasColumnName("uploaded_at");
            entity.Property(e => e.UploadedBy).HasColumnName("uploaded_by").HasMaxLength(100);
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");

            entity.HasIndex(e => e.EscrowAccountId);
            entity.HasIndex(e => e.DocumentType);
        });

        // EscrowDispute Configuration
        modelBuilder.Entity<EscrowDispute>(entity =>
        {
            entity.ToTable("escrow_disputes");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EscrowAccountId).HasColumnName("escrow_account_id");
            entity.Property(e => e.DisputeNumber).HasColumnName("dispute_number").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Status).HasColumnName("status").HasConversion<string>();
            entity.Property(e => e.FiledById).HasColumnName("filed_by_id");
            entity.Property(e => e.FiledByName).HasColumnName("filed_by_name").HasMaxLength(200);
            entity.Property(e => e.FiledByType).HasColumnName("filed_by_type").HasMaxLength(20);
            entity.Property(e => e.Reason).HasColumnName("reason").HasMaxLength(100);
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DisputedAmount).HasColumnName("disputed_amount").HasPrecision(18, 2);
            entity.Property(e => e.Category).HasColumnName("category").HasMaxLength(50);
            entity.Property(e => e.Resolution).HasColumnName("resolution");
            entity.Property(e => e.ResolvedBuyerAmount).HasColumnName("resolved_buyer_amount").HasPrecision(18, 2);
            entity.Property(e => e.ResolvedSellerAmount).HasColumnName("resolved_seller_amount").HasPrecision(18, 2);
            entity.Property(e => e.FiledAt).HasColumnName("filed_at");
            entity.Property(e => e.ResolvedAt).HasColumnName("resolved_at");
            entity.Property(e => e.ResolvedBy).HasColumnName("resolved_by").HasMaxLength(100);
            entity.Property(e => e.AssignedTo).HasColumnName("assigned_to").HasMaxLength(100);
            entity.Property(e => e.EscalatedAt).HasColumnName("escalated_at");
            entity.Property(e => e.EscalationReason).HasColumnName("escalation_reason").HasMaxLength(500);

            entity.HasIndex(e => e.DisputeNumber).IsUnique();
            entity.HasIndex(e => e.EscrowAccountId);
            entity.HasIndex(e => e.Status);
        });

        // EscrowAuditLog Configuration
        modelBuilder.Entity<EscrowAuditLog>(entity =>
        {
            entity.ToTable("escrow_audit_logs");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EscrowAccountId).HasColumnName("escrow_account_id");
            entity.Property(e => e.EventType).HasColumnName("event_type").HasConversion<string>();
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(500);
            entity.Property(e => e.OldValue).HasColumnName("old_value");
            entity.Property(e => e.NewValue).HasColumnName("new_value");
            entity.Property(e => e.AmountInvolved).HasColumnName("amount_involved").HasPrecision(18, 2);
            entity.Property(e => e.PerformedBy).HasColumnName("performed_by").HasMaxLength(100);
            entity.Property(e => e.PerformedAt).HasColumnName("performed_at");
            entity.Property(e => e.IpAddress).HasColumnName("ip_address").HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasColumnName("user_agent").HasMaxLength(500);

            entity.HasIndex(e => e.EscrowAccountId);
            entity.HasIndex(e => e.PerformedAt);
        });

        // EscrowFeeConfiguration Configuration
        modelBuilder.Entity<EscrowFeeConfiguration>(entity =>
        {
            entity.ToTable("escrow_fee_configurations");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            entity.Property(e => e.TransactionType).HasColumnName("transaction_type").HasConversion<string>();
            entity.Property(e => e.MinAmount).HasColumnName("min_amount").HasPrecision(18, 2);
            entity.Property(e => e.MaxAmount).HasColumnName("max_amount").HasPrecision(18, 2);
            entity.Property(e => e.FeePercentage).HasColumnName("fee_percentage").HasPrecision(5, 2);
            entity.Property(e => e.MinFee).HasColumnName("min_fee").HasPrecision(18, 2);
            entity.Property(e => e.MaxFee).HasColumnName("max_fee").HasPrecision(18, 2);
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.EffectiveFrom).HasColumnName("effective_from");
            entity.Property(e => e.EffectiveTo).HasColumnName("effective_to");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100);

            entity.HasIndex(e => new { e.TransactionType, e.IsActive });
        });
    }
}
