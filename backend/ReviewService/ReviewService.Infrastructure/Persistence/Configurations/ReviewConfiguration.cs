using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReviewService.Domain.Entities;

namespace ReviewService.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración EF Core para Review entity
/// </summary>
public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("reviews");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .ValueGeneratedOnAdd();

        builder.Property(r => r.BuyerId)
            .IsRequired();

        builder.Property(r => r.SellerId)
            .IsRequired();

        builder.Property(r => r.VehicleId)
            .IsRequired(false);

        builder.Property(r => r.OrderId)
            .IsRequired(false);

        builder.Property(r => r.Rating)
            .IsRequired()
            .HasAnnotation("MinValue", 1)
            .HasAnnotation("MaxValue", 5);

        builder.Property(r => r.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.Content)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(r => r.IsApproved)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(r => r.IsVerifiedPurchase)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(r => r.RejectionReason)
            .HasMaxLength(500);

        builder.Property(r => r.BuyerName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.BuyerPhotoUrl)
            .HasMaxLength(500);

        builder.Property(r => r.HelpfulVotes)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(r => r.TotalVotes)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(r => r.CreatedAt)
            .IsRequired();

        builder.Property(r => r.UpdatedAt)
            .IsRequired();

        // Relaciones
        builder.HasOne(r => r.Response)
            .WithOne(rr => rr.Review)
            .HasForeignKey<ReviewResponse>(rr => rr.ReviewId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices para performance
        builder.HasIndex(r => r.SellerId)
            .HasDatabaseName("IX_reviews_seller_id");

        builder.HasIndex(r => r.BuyerId)
            .HasDatabaseName("IX_reviews_buyer_id");

        builder.HasIndex(r => new { r.IsApproved, r.Rating })
            .HasDatabaseName("IX_reviews_approved_rating");

        builder.HasIndex(r => r.OrderId)
            .HasDatabaseName("IX_reviews_order_id")
            .IsUnique()
            .HasFilter("\"OrderId\" IS NOT NULL");

        builder.HasIndex(r => new { r.BuyerId, r.SellerId, r.VehicleId })
            .HasDatabaseName("IX_reviews_unique_buyer_seller_vehicle")
            .IsUnique()
            .HasFilter("\"VehicleId\" IS NOT NULL");
    }
}