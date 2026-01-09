using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReviewService.Domain.Entities;

namespace ReviewService.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración EF para SellerBadge
/// </summary>
public class SellerBadgeConfiguration : IEntityTypeConfiguration<SellerBadge>
{
    public void Configure(EntityTypeBuilder<SellerBadge> builder)
    {
        builder.ToTable("seller_badges");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.SellerId)
            .IsRequired()
            .HasColumnName("seller_id");

        builder.Property(x => x.BadgeType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasColumnName("badge_type");

        builder.Property(x => x.GrantedAt)
            .IsRequired()
            .HasColumnName("granted_at")
            .HasColumnType("timestamptz");

        builder.Property(x => x.RevokedAt)
            .HasColumnName("revoked_at")
            .HasColumnType("timestamptz");

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true)
            .HasColumnName("is_active");

        builder.Property(x => x.Notes)
            .HasMaxLength(500)
            .HasColumnName("notes");

        builder.Property(x => x.QualifyingStats)
            .HasColumnType("jsonb")
            .HasColumnName("qualifying_stats");

        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnName("created_at")
            .HasColumnType("timestamptz");

        builder.Property(x => x.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz");

        // Índices
        builder.HasIndex(x => x.SellerId)
            .HasDatabaseName("ix_seller_badges_seller_id");

        builder.HasIndex(x => new { x.SellerId, x.BadgeType, x.IsActive })
            .HasDatabaseName("ix_seller_badges_seller_type_active");

        builder.HasIndex(x => x.BadgeType)
            .HasDatabaseName("ix_seller_badges_badge_type");

        builder.HasIndex(x => x.IsActive)
            .HasDatabaseName("ix_seller_badges_is_active");

        builder.HasIndex(x => x.GrantedAt)
            .HasDatabaseName("ix_seller_badges_granted_at");
    }
}