using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserService.Domain.Entities;

namespace UserService.Infrastructure.Persistence.Configurations;

public class SellerProfileConfiguration : IEntityTypeConfiguration<SellerProfile>
{
    public void Configure(EntityTypeBuilder<SellerProfile> builder)
    {
        builder.ToTable("seller_profiles");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id");

        builder.Property(e => e.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(e => e.FullName)
            .HasColumnName("full_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.DisplayName)
            .HasColumnName("display_name")
            .HasMaxLength(200);

        builder.Property(e => e.Bio)
            .HasColumnName("bio")
            .HasMaxLength(500);

        builder.Property(e => e.AvatarUrl)
            .HasColumnName("avatar_url")
            .HasMaxLength(500);

        builder.Property(e => e.CoverPhotoUrl)
            .HasColumnName("cover_photo_url")
            .HasMaxLength(500);

        builder.Property(e => e.Phone)
            .HasColumnName("phone")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.WhatsApp)
            .HasColumnName("whatsapp")
            .HasMaxLength(20);

        builder.Property(e => e.Email)
            .HasColumnName("email")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(e => e.City)
            .HasColumnName("city")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.State)
            .HasColumnName("state")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.Country)
            .HasColumnName("country")
            .HasMaxLength(2)
            .HasDefaultValue("DO");

        builder.Property(e => e.SellerType)
            .HasColumnName("seller_type")
            .HasConversion<int>()
            .HasDefaultValue(SellerType.Individual);

        builder.Property(e => e.VerificationStatus)
            .HasColumnName("verification_status")
            .HasConversion<int>()
            .HasDefaultValue(SellerVerificationStatus.NotSubmitted);

        builder.Property(e => e.AverageRating)
            .HasColumnName("average_rating")
            .HasPrecision(3, 2)
            .HasDefaultValue(0);

        builder.Property(e => e.TotalListings)
            .HasColumnName("total_listings")
            .HasDefaultValue(0);

        builder.Property(e => e.ActiveListings)
            .HasColumnName("active_listings")
            .HasDefaultValue(0);

        builder.Property(e => e.TotalSales)
            .HasColumnName("total_sales")
            .HasDefaultValue(0);

        builder.Property(e => e.TotalReviews)
            .HasColumnName("total_reviews")
            .HasDefaultValue(0);

        builder.Property(e => e.ResponseTimeMinutes)
            .HasColumnName("response_time_minutes")
            .HasDefaultValue(0);

        builder.Property(e => e.ResponseRate)
            .HasColumnName("response_rate")
            .HasDefaultValue(100);

        builder.Property(e => e.ViewsThisMonth)
            .HasColumnName("views_this_month")
            .HasDefaultValue(0);

        builder.Property(e => e.LeadsThisMonth)
            .HasColumnName("leads_this_month")
            .HasDefaultValue(0);

        builder.Property(e => e.MaxActiveListings)
            .HasColumnName("max_active_listings")
            .HasDefaultValue(3);

        builder.Property(e => e.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(e => e.IsDeleted)
            .HasColumnName("is_deleted")
            .HasDefaultValue(false);

        builder.Property(e => e.IsPhoneVerified)
            .HasColumnName("is_phone_verified")
            .HasDefaultValue(false);

        builder.Property(e => e.IsIdentityVerified)
            .HasColumnName("is_identity_verified")
            .HasDefaultValue(false);

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(e => e.LastActiveAt)
            .HasColumnName("last_active_at");

        builder.Property(e => e.VerifiedAt)
            .HasColumnName("verified_at");

        builder.Property(e => e.DealerId)
            .HasColumnName("dealer_id");

        builder.Property(e => e.BusinessName)
            .HasColumnName("business_name")
            .HasMaxLength(200);

        builder.Property(e => e.Website)
            .HasColumnName("website")
            .HasMaxLength(256);

        // Índices
        builder.HasIndex(e => e.UserId)
            .IsUnique()
            .HasDatabaseName("ix_seller_profiles_user_id");

        builder.HasIndex(e => e.City)
            .HasDatabaseName("ix_seller_profiles_city");

        builder.HasIndex(e => e.VerificationStatus)
            .HasDatabaseName("ix_seller_profiles_verification_status");

        builder.HasIndex(e => e.SellerType)
            .HasDatabaseName("ix_seller_profiles_seller_type");

        builder.HasIndex(e => e.AverageRating)
            .HasDatabaseName("ix_seller_profiles_average_rating");

        // Relaciones
        builder.HasMany(e => e.IdentityDocuments)
            .WithOne(d => d.SellerProfile)
            .HasForeignKey(d => d.SellerProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Badges)
            .WithOne(b => b.SellerProfile)
            .HasForeignKey(b => b.SellerProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.ContactPreferences)
            .WithOne(cp => cp.SellerProfile)
            .HasForeignKey<ContactPreferences>(cp => cp.SellerProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ContactPreferencesConfiguration : IEntityTypeConfiguration<ContactPreferences>
{
    public void Configure(EntityTypeBuilder<ContactPreferences> builder)
    {
        builder.ToTable("contact_preferences");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id");

        builder.Property(e => e.SellerProfileId)
            .HasColumnName("seller_profile_id")
            .IsRequired();

        builder.Property(e => e.AllowPhoneCalls)
            .HasColumnName("allow_phone_calls")
            .HasDefaultValue(true);

        builder.Property(e => e.AllowWhatsApp)
            .HasColumnName("allow_whatsapp")
            .HasDefaultValue(true);

        builder.Property(e => e.AllowEmail)
            .HasColumnName("allow_email")
            .HasDefaultValue(true);

        builder.Property(e => e.AllowInAppChat)
            .HasColumnName("allow_in_app_chat")
            .HasDefaultValue(true);

        builder.Property(e => e.ContactHoursStart)
            .HasColumnName("contact_hours_start")
            .HasDefaultValue(new TimeSpan(8, 0, 0));

        builder.Property(e => e.ContactHoursEnd)
            .HasColumnName("contact_hours_end")
            .HasDefaultValue(new TimeSpan(18, 0, 0));

        builder.Property(e => e.ContactDays)
            .HasColumnName("contact_days")
            .HasMaxLength(200);

        builder.Property(e => e.ShowPhoneNumber)
            .HasColumnName("show_phone_number")
            .HasDefaultValue(true);

        builder.Property(e => e.ShowWhatsAppNumber)
            .HasColumnName("show_whatsapp_number")
            .HasDefaultValue(true);

        builder.Property(e => e.ShowEmail)
            .HasColumnName("show_email")
            .HasDefaultValue(false);

        builder.Property(e => e.PreferredContactMethod)
            .HasColumnName("preferred_contact_method")
            .HasMaxLength(20)
            .HasDefaultValue("WhatsApp");

        builder.Property(e => e.AutoReplyMessage)
            .HasColumnName("auto_reply_message")
            .HasMaxLength(500);

        builder.Property(e => e.AwayMessage)
            .HasColumnName("away_message")
            .HasMaxLength(500);

        builder.Property(e => e.RequireVerifiedBuyers)
            .HasColumnName("require_verified_buyers")
            .HasDefaultValue(false);

        builder.Property(e => e.BlockAnonymousContacts)
            .HasColumnName("block_anonymous_contacts")
            .HasDefaultValue(true);

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasIndex(e => e.SellerProfileId)
            .IsUnique()
            .HasDatabaseName("ix_contact_preferences_seller_profile_id");
    }
}

public class SellerBadgeAssignmentConfiguration : IEntityTypeConfiguration<SellerBadgeAssignment>
{
    public void Configure(EntityTypeBuilder<SellerBadgeAssignment> builder)
    {
        builder.ToTable("seller_badge_assignments");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id");

        builder.Property(e => e.SellerProfileId)
            .HasColumnName("seller_profile_id")
            .IsRequired();

        builder.Property(e => e.Badge)
            .HasColumnName("badge")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(e => e.EarnedAt)
            .HasColumnName("earned_at")
            .HasDefaultValueSql("NOW()");

        builder.Property(e => e.ExpiresAt)
            .HasColumnName("expires_at");

        builder.Property(e => e.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(e => e.Reason)
            .HasColumnName("reason")
            .HasMaxLength(500);

        builder.HasIndex(e => new { e.SellerProfileId, e.Badge })
            .HasDatabaseName("ix_seller_badge_assignments_profile_badge");

        builder.HasIndex(e => e.IsActive)
            .HasDatabaseName("ix_seller_badge_assignments_is_active");
    }
}
