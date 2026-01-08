using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserService.Domain.Entities;

namespace UserService.Infrastructure.Persistence.Configurations;

public class UserOnboardingConfiguration : IEntityTypeConfiguration<UserOnboarding>
{
    public void Configure(EntityTypeBuilder<UserOnboarding> builder)
    {
        builder.ToTable("user_onboardings");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .HasColumnName("Id");

        builder.Property(o => o.UserId)
            .IsRequired()
            .HasColumnName("UserId");

        builder.Property(o => o.IsCompleted)
            .IsRequired()
            .HasColumnName("IsCompleted")
            .HasDefaultValue(false);

        builder.Property(o => o.CompletedAt)
            .HasColumnName("CompletedAt");

        builder.Property(o => o.WasSkipped)
            .IsRequired()
            .HasColumnName("WasSkipped")
            .HasDefaultValue(false);

        builder.Property(o => o.SkippedAt)
            .HasColumnName("SkippedAt");

        // Step 1: Profile
        builder.Property(o => o.StepProfileCompleted)
            .IsRequired()
            .HasColumnName("StepProfileCompleted")
            .HasDefaultValue(false);

        builder.Property(o => o.StepProfileCompletedAt)
            .HasColumnName("StepProfileCompletedAt");

        // Step 2: Preferences
        builder.Property(o => o.StepPreferencesCompleted)
            .IsRequired()
            .HasColumnName("StepPreferencesCompleted")
            .HasDefaultValue(false);

        builder.Property(o => o.StepPreferencesCompletedAt)
            .HasColumnName("StepPreferencesCompletedAt");

        // Step 3: First Search
        builder.Property(o => o.StepFirstSearchCompleted)
            .IsRequired()
            .HasColumnName("StepFirstSearchCompleted")
            .HasDefaultValue(false);

        builder.Property(o => o.StepFirstSearchCompletedAt)
            .HasColumnName("StepFirstSearchCompletedAt");

        // Step 4: Tour
        builder.Property(o => o.StepTourCompleted)
            .IsRequired()
            .HasColumnName("StepTourCompleted")
            .HasDefaultValue(false);

        builder.Property(o => o.StepTourCompletedAt)
            .HasColumnName("StepTourCompletedAt");

        builder.Property(o => o.CreatedAt)
            .IsRequired()
            .HasColumnName("CreatedAt")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(o => o.UpdatedAt)
            .IsRequired()
            .HasColumnName("UpdatedAt")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Indexes
        builder.HasIndex(o => o.UserId)
            .IsUnique()
            .HasDatabaseName("idx_user_onboardings_user");

        builder.HasIndex(o => o.IsCompleted)
            .HasDatabaseName("idx_user_onboardings_completed");

        builder.HasIndex(o => o.WasSkipped)
            .HasDatabaseName("idx_user_onboardings_skipped");
    }
}
