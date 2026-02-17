using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReviewService.Domain.Entities;

namespace ReviewService.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración EF Core para ReviewResponse entity
/// </summary>
public class ReviewResponseConfiguration : IEntityTypeConfiguration<ReviewResponse>
{
    public void Configure(EntityTypeBuilder<ReviewResponse> builder)
    {
        builder.ToTable("review_responses");

        builder.HasKey(rr => rr.Id);

        builder.Property(rr => rr.Id)
            .ValueGeneratedOnAdd();

        builder.Property(rr => rr.ReviewId)
            .IsRequired();

        builder.Property(rr => rr.SellerId)
            .IsRequired();

        builder.Property(rr => rr.Content)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(rr => rr.IsApproved)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(rr => rr.SellerName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(rr => rr.CreatedAt)
            .IsRequired();

        builder.Property(rr => rr.UpdatedAt)
            .IsRequired();

        // Índices
        builder.HasIndex(rr => rr.ReviewId)
            .IsUnique()
            .HasDatabaseName("IX_review_responses_review_id");

        builder.HasIndex(rr => rr.SellerId)
            .HasDatabaseName("IX_review_responses_seller_id");
    }
}

/// <summary>
/// Configuración EF Core para ReviewSummary entity
/// </summary>
public class ReviewSummaryConfiguration : IEntityTypeConfiguration<ReviewSummary>
{
    public void Configure(EntityTypeBuilder<ReviewSummary> builder)
    {
        builder.ToTable("review_summaries");

        builder.HasKey(rs => rs.Id);

        builder.Property(rs => rs.Id)
            .ValueGeneratedOnAdd();

        builder.Property(rs => rs.SellerId)
            .IsRequired();

        builder.Property(rs => rs.TotalReviews)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(rs => rs.AverageRating)
            .HasColumnType("decimal(3,2)")
            .HasDefaultValue(0);

        builder.Property(rs => rs.FiveStarReviews)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(rs => rs.FourStarReviews)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(rs => rs.ThreeStarReviews)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(rs => rs.TwoStarReviews)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(rs => rs.OneStarReviews)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(rs => rs.PositivePercentage)
            .HasColumnType("decimal(5,2)")
            .HasDefaultValue(0);

        builder.Property(rs => rs.VerifiedPurchaseReviews)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(rs => rs.CreatedAt)
            .IsRequired();

        builder.Property(rs => rs.UpdatedAt)
            .IsRequired();

        // Índices
        builder.HasIndex(rs => rs.SellerId)
            .IsUnique()
            .HasDatabaseName("IX_review_summaries_seller_id");

        builder.HasIndex(rs => rs.AverageRating)
            .HasDatabaseName("IX_review_summaries_average_rating");

        builder.HasIndex(rs => rs.TotalReviews)
            .HasDatabaseName("IX_review_summaries_total_reviews");
    }
}