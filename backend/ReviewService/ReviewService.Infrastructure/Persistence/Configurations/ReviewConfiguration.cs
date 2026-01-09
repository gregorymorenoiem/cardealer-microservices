using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReviewService.Domain.Entities;

namespace ReviewService.Infrastructure.Persistence.Configurations;

/// &lt;summary&gt;
/// Configuración EF Core para Review entity
/// &lt;/summary&gt;
public class ReviewConfiguration : IEntityTypeConfiguration&lt;Review&gt;
{
    public void Configure(EntityTypeBuilder&lt;Review&gt; builder)
    {
        builder.ToTable("reviews");

        builder.HasKey(r =&gt; r.Id);

        builder.Property(r =&gt; r.Id)
            .ValueGeneratedOnAdd();

        builder.Property(r =&gt; r.BuyerId)
            .IsRequired();

        builder.Property(r =&gt; r.SellerId)
            .IsRequired();

        builder.Property(r =&gt; r.VehicleId)
            .IsRequired(false);

        builder.Property(r =&gt; r.OrderId)
            .IsRequired(false);

        builder.Property(r =&gt; r.Rating)
            .IsRequired()
            .HasAnnotation("MinValue", 1)
            .HasAnnotation("MaxValue", 5);

        builder.Property(r =&gt; r.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r =&gt; r.Content)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(r =&gt; r.IsApproved)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(r =&gt; r.IsVerifiedPurchase)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(r =&gt; r.RejectionReason)
            .HasMaxLength(500);

        builder.Property(r =&gt; r.BuyerName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r =&gt; r.BuyerPhotoUrl)
            .HasMaxLength(500);

        builder.Property(r =&gt; r.HelpfulVotes)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(r =&gt; r.TotalVotes)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(r =&gt; r.CreatedAt)
            .IsRequired();

        builder.Property(r =&gt; r.UpdatedAt)
            .IsRequired();

        // Relaciones
        builder.HasOne(r =&gt; r.Response)
            .WithOne(rr =&gt; rr.Review)
            .HasForeignKey&lt;ReviewResponse&gt;(rr =&gt; rr.ReviewId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices para performance
        builder.HasIndex(r =&gt; r.SellerId)
            .HasDatabaseName("IX_reviews_seller_id");

        builder.HasIndex(r =&gt; r.BuyerId)
            .HasDatabaseName("IX_reviews_buyer_id");

        builder.HasIndex(r =&gt; new { r.IsApproved, r.Rating })
            .HasDatabaseName("IX_reviews_approved_rating");

        builder.HasIndex(r =&gt; r.OrderId)
            .HasDatabaseName("IX_reviews_order_id")
            .IsUnique()
            .HasFilter("\"OrderId\" IS NOT NULL");

        builder.HasIndex(r =&gt; new { r.BuyerId, r.SellerId, r.VehicleId })
            .HasDatabaseName("IX_reviews_unique_buyer_seller_vehicle")
            .IsUnique()
            .HasFilter("\"VehicleId\" IS NOT NULL");
    }
}