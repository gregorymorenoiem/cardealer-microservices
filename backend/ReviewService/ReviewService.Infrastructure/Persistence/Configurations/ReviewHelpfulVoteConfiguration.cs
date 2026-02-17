using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReviewService.Domain.Entities;

namespace ReviewService.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración EF para ReviewHelpfulVote
/// </summary>
public class ReviewHelpfulVoteConfiguration : IEntityTypeConfiguration<ReviewHelpfulVote>
{
    public void Configure(EntityTypeBuilder<ReviewHelpfulVote> builder)
    {
        builder.ToTable("review_helpful_votes");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.ReviewId)
            .IsRequired()
            .HasColumnName("review_id");

        builder.Property(x => x.UserId)
            .IsRequired()
            .HasColumnName("user_id");

        builder.Property(x => x.IsHelpful)
            .IsRequired()
            .HasColumnName("is_helpful");

        builder.Property(x => x.VotedAt)
            .IsRequired()
            .HasColumnName("voted_at")
            .HasColumnType("timestamptz");

        builder.Property(x => x.UserIpAddress)
            .HasMaxLength(45)
            .HasColumnName("user_ip_address");

        // Índices
        builder.HasIndex(x => new { x.ReviewId, x.UserId })
            .IsUnique()
            .HasDatabaseName("ix_review_helpful_votes_review_user");

        builder.HasIndex(x => x.ReviewId)
            .HasDatabaseName("ix_review_helpful_votes_review_id");

        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("ix_review_helpful_votes_user_id");

        builder.HasIndex(x => x.UserIpAddress)
            .HasDatabaseName("ix_review_helpful_votes_ip_address");

        builder.HasIndex(x => x.VotedAt)
            .HasDatabaseName("ix_review_helpful_votes_voted_at");

        // Relación con Review
        builder.HasOne(x => x.Review)
            .WithMany(x => x.HelpfulVotesList)
            .HasForeignKey(x => x.ReviewId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}