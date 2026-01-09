using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReviewService.Domain.Entities;

namespace ReviewService.Infrastructure.Persistence.Configurations;

/// &lt;summary&gt;
/// Configuración EF Core para ReviewResponse entity
/// &lt;/summary&gt;
public class ReviewResponseConfiguration : IEntityTypeConfiguration&lt;ReviewResponse&gt;
{
    public void Configure(EntityTypeBuilder&lt;ReviewResponse&gt; builder)
    {
        builder.ToTable("review_responses");

        builder.HasKey(rr =&gt; rr.Id);

        builder.Property(rr =&gt; rr.Id)
            .ValueGeneratedOnAdd();

        builder.Property(rr =&gt; rr.ReviewId)
            .IsRequired();

        builder.Property(rr =&gt; rr.SellerId)
            .IsRequired();

        builder.Property(rr =&gt; rr.Content)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(rr =&gt; rr.IsApproved)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(rr =&gt; rr.SellerName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(rr =&gt; rr.CreatedAt)
            .IsRequired();

        builder.Property(rr =&gt; rr.UpdatedAt)
            .IsRequired();

        // Índices
        builder.HasIndex(rr =&gt; rr.ReviewId)
            .IsUnique()
            .HasDatabaseName("IX_review_responses_review_id");

        builder.HasIndex(rr =&gt; rr.SellerId)
            .HasDatabaseName("IX_review_responses_seller_id");
    }
}

/// &lt;summary&gt;
/// Configuración EF Core para ReviewSummary entity
/// &lt;/summary&gt;
public class ReviewSummaryConfiguration : IEntityTypeConfiguration&lt;ReviewSummary&gt;
{
    public void Configure(EntityTypeBuilder&lt;ReviewSummary&gt; builder)
    {
        builder.ToTable("review_summaries");

        builder.HasKey(rs =&gt; rs.Id);

        builder.Property(rs =&gt; rs.Id)
            .ValueGeneratedOnAdd();

        builder.Property(rs =&gt; rs.SellerId)
            .IsRequired();

        builder.Property(rs =&gt; rs.TotalReviews)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(rs =&gt; rs.AverageRating)
            .HasColumnType("decimal(3,2)")
            .HasDefaultValue(0);

        builder.Property(rs =&gt; rs.FiveStarReviews)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(rs =&gt; rs.FourStarReviews)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(rs =&gt; rs.ThreeStarReviews)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(rs =&gt; rs.TwoStarReviews)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(rs =&gt; rs.OneStarReviews)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(rs =&gt; rs.PositivePercentage)
            .HasColumnType("decimal(5,2)")
            .HasDefaultValue(0);

        builder.Property(rs =&gt; rs.VerifiedPurchaseReviews)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(rs =&gt; rs.CreatedAt)
            .IsRequired();

        builder.Property(rs =&gt; rs.UpdatedAt)
            .IsRequired();

        // Índices
        builder.HasIndex(rs =&gt; rs.SellerId)
            .IsUnique()
            .HasDatabaseName("IX_review_summaries_seller_id");

        builder.HasIndex(rs =&gt; rs.AverageRating)
            .HasDatabaseName("IX_review_summaries_average_rating");

        builder.HasIndex(rs =&gt; rs.TotalReviews)
            .HasDatabaseName("IX_review_summaries_total_reviews");
    }
}