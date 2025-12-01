using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserService.Domain.Entities;

namespace UserService.Infrastructure.Persistence.Configurations
{
    public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
    {
        public void Configure(EntityTypeBuilder<UserRole> builder)
        {
            builder.ToTable("UserRoles");

            builder.HasKey(ur => ur.Id);

            builder.Property(ur => ur.UserId)
                .IsRequired();

            builder.Property(ur => ur.RoleId)
                .IsRequired();

            builder.Property(ur => ur.AssignedAt)
                .IsRequired();

            builder.Property(ur => ur.AssignedBy)
                .IsRequired()
                .HasMaxLength(100)
                .HasDefaultValue("system");

            builder.Property(ur => ur.RevokedBy)
                .HasMaxLength(100);

            builder.Property(ur => ur.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Índices
            builder.HasIndex(ur => ur.UserId)
                .HasDatabaseName("IX_UserRoles_UserId");

            builder.HasIndex(ur => ur.RoleId)
                .HasDatabaseName("IX_UserRoles_RoleId");

            builder.HasIndex(ur => ur.IsActive)
                .HasDatabaseName("IX_UserRoles_IsActive");

            builder.HasIndex(ur => new { ur.UserId, ur.RoleId })
                .IsUnique()
                .HasDatabaseName("UQ_UserRoles_UserId_RoleId");

            // Relación con User
            builder.HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
